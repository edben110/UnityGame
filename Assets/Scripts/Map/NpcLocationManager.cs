using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la ubicación lógica de los NPCs en las salas.
/// Los NPCs no se destruyen al cambiar de sala; su posición es global.
/// Cuando el jugador entra a una sala, se muestran solo los NPCs que están ahí.
/// </summary>
public class NpcLocationManager : MonoBehaviour
{
    [Serializable]
    public class NpcRoomAssignment
    {
        public string npcId;
        public string displayName;
        public string currentRoomId;
        public GameObject npcObject;
    }

    public static NpcLocationManager Instance { get; private set; }

    [Header("Asignaciones de NPCs")]
    [SerializeField] private List<NpcRoomAssignment> npcAssignments = new List<NpcRoomAssignment>();

    private readonly Dictionary<string, NpcRoomAssignment> npcLookup = new Dictionary<string, NpcRoomAssignment>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        npcLookup.Clear();
        for (int i = 0; i < npcAssignments.Count; i++)
        {
            NpcRoomAssignment npc = npcAssignments[i];
            if (npc != null && !string.IsNullOrWhiteSpace(npc.npcId))
            {
                npcLookup[npc.npcId] = npc;
            }
        }
    }

    private void OnEnable()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged += OnRoomChanged;
        }
    }

    private void OnDisable()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= OnRoomChanged;
        }
    }

    private void Start()
    {
        // Suscribirse después de que RoomManager se inicialice
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= OnRoomChanged;
            RoomManager.Instance.RoomChanged += OnRoomChanged;
        }

        RefreshNpcVisibility();
    }

    /// <summary>
    /// Mueve un NPC a otra sala. Se usa desde el sistema narrativo
    /// para actualizar posiciones según el progreso de la historia.
    /// </summary>
    public void MoveNpc(string npcId, string newRoomId)
    {
        if (!npcLookup.TryGetValue(npcId, out NpcRoomAssignment npc))
        {
            Debug.LogWarning($"NpcLocationManager: NPC no encontrado: {npcId}");
            return;
        }

        string previousRoom = npc.currentRoomId;
        npc.currentRoomId = newRoomId;

        // Guardar en StoryState
        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetDecision($"npc.location.{npcId}", newRoomId);
        }

        Debug.Log($"NPC {npc.displayName} movido de [{previousRoom}] a [{newRoomId}]");
        RefreshNpcVisibility();
    }

    /// <summary>
    /// Obtiene la sala actual de un NPC.
    /// </summary>
    public string GetNpcRoom(string npcId)
    {
        if (npcLookup.TryGetValue(npcId, out NpcRoomAssignment npc))
        {
            return npc.currentRoomId;
        }

        return null;
    }

    /// <summary>
    /// Obtiene todos los NPCs en una sala específica.
    /// </summary>
    public List<string> GetNpcsInRoom(string roomId)
    {
        List<string> result = new List<string>();
        foreach (var pair in npcLookup)
        {
            if (pair.Value.currentRoomId == roomId)
            {
                result.Add(pair.Key);
            }
        }

        return result;
    }

    /// <summary>
    /// Actualiza las posiciones de los NPCs según el progreso de la historia.
    /// Llamar cuando cambia el capítulo o se activa un evento narrativo.
    /// </summary>
    public void UpdateNpcPositionsForChapter(string chapterId)
    {
        // Posiciones iniciales según el script de Python
        switch (chapterId)
        {
            case "prologue":
            case "chapter1":
                // Todos en el lobby al inicio
                SetAllNpcsToRoom("lobby");
                break;

            case "chapter2":
                // Se mueven al estudio
                SetAllNpcsToRoom("estudio");
                break;

            case "chapter3":
                // Se dispersan
                MoveNpcSilent("robert", "lobby");
                MoveNpcSilent("ana", "galeria");
                MoveNpcSilent("ben", "estudio");
                MoveNpcSilent("lisa", "habitacion");
                MoveNpcSilent("lucas", "sala_vigilancia");
                break;

            case "chapter4":
                MoveNpcSilent("robert", "galeria");
                MoveNpcSilent("ana", "estudio");
                MoveNpcSilent("ben", "lobby");
                MoveNpcSilent("lisa", "habitacion");
                MoveNpcSilent("lucas", "sala_vigilancia");
                break;

            case "chapter5":
                // Todos se reúnen en la sala de vigilancia
                SetAllNpcsToRoom("sala_vigilancia");
                break;
        }

        RefreshNpcVisibility();
    }

    private void SetAllNpcsToRoom(string roomId)
    {
        foreach (var pair in npcLookup)
        {
            pair.Value.currentRoomId = roomId;
        }
    }

    private void MoveNpcSilent(string npcId, string roomId)
    {
        if (npcLookup.TryGetValue(npcId, out NpcRoomAssignment npc))
        {
            npc.currentRoomId = roomId;
        }
    }

    private void OnRoomChanged(string previousRoom, string newRoom)
    {
        ProcessAnxietyDropouts(previousRoom);
        RefreshNpcVisibility();
    }

    private void ProcessAnxietyDropouts(string previousRoom)
    {
        if (string.IsNullOrWhiteSpace(previousRoom)
            || CharacterAnxietySystem.Instance == null
            || StoryState.Instance == null)
        {
            return;
        }

        foreach (var pair in npcLookup)
        {
            string npcId = pair.Key;
            NpcRoomAssignment assignment = pair.Value;
            if (assignment == null || string.IsNullOrWhiteSpace(npcId))
            {
                continue;
            }

            if (StoryState.Instance.HasFlag($"npc.dead.{npcId}"))
            {
                continue;
            }

            if (!CharacterAnxietySystem.Instance.IsAtMaxAnxiety(npcId))
            {
                continue;
            }

            if (!string.Equals(assignment.currentRoomId, previousRoom, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            assignment.currentRoomId = "missing";
            StoryState.Instance.SetDecision($"npc.location.{npcId}", "missing");
            StoryState.Instance.SetFlag($"npc.dead.{npcId}", true);
            StoryState.Instance.SetFlag("npc.dead.any", true);
            StoryState.Instance.SetFlag($"npc.disappearance.pending.{npcId}", true);
            StoryState.Instance.SetFlag($"npc.cadaver.ready.{npcId}", false);

            string displayName = CharacterAnxietySystem.Instance.GetCharacterDisplayName(npcId);
            Debug.Log($"NpcLocationManager: {displayName} desaparecio tras cambiar de sala desde [{previousRoom}].");
        }
    }

    private void RefreshNpcVisibility()
    {
        string currentRoom = RoomManager.Instance != null ? RoomManager.Instance.CurrentRoomId : null;

        foreach (var pair in npcLookup)
        {
            NpcRoomAssignment npc = pair.Value;
            if (npc.npcObject == null)
            {
                continue;
            }

            bool shouldShow = npc.currentRoomId == currentRoom;
            npc.npcObject.SetActive(shouldShow);
        }
    }
}
