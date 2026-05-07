using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona el ciclo de vida de los cadáveres en La Mansión de Simón.
/// Cuando todos los NPCs vivos han hablado de la desaparición de un compañero
/// (npc.cadaver.ready.{id} == true), el cadáver del NPC se coloca en una sala
/// desbloqueada y se activa cuando el jugador entra.
///
/// Flujo:
///   1. NpcAnxietyDropoutSystem / NpcLocationManager.ProcessAnxietyDropouts:
///      • npc.dead.{id} = true
///      • npc.disappearance.pending.{id} = true
///      • npc.cadaver.ready.{id} = false
///
///   2. NpcInteractable.TryHandlePendingDisappearances:
///      • Al hablar con cada NPC vivo, se marca npc.disappearance.reported.{dead}.{alive}
///      • Cuando todos han comentado: npc.cadaver.ready.{id} = true
///
///   3. CadaverManager.OnRoomChanged:
///      • Cuando el jugador entra a la sala asignada al cadáver y cadaver.ready es true,
///        activa el GameObject del cadáver y dispara la conversación de descubrimiento.
/// </summary>
public class CadaverManager : MonoBehaviour
{
    [Serializable]
    public class CadaverEntry
    {
        [Tooltip("ID del NPC (ej. 'robert', 'ben')")]
        public string npcId;

        [Tooltip("Sala donde aparecerá el cadáver (ej. 'studio', 'bedroom')")]
        public string cadaverRoomId;

        [Tooltip("GameObject del sprite/prefab del cadáver (desactivado al inicio)")]
        public GameObject cadaverObject;

        [Tooltip("ID de conversación que se dispara al descubrir el cadáver")]
        public string discoveryConversationId;

        [HideInInspector] public bool alreadyDiscovered;
    }

    public static CadaverManager Instance { get; private set; }

    [Header("Cadáveres configurados")]
    [SerializeField] private List<CadaverEntry> cadavers = new List<CadaverEntry>();

    private readonly Dictionary<string, CadaverEntry> cadaverLookup = new Dictionary<string, CadaverEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RebuildLookup();
    }

    private void Start()
    {
        HideAllCadavers();

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= OnRoomChanged;
            RoomManager.Instance.RoomChanged += OnRoomChanged;
        }
    }

    private void OnDestroy()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= OnRoomChanged;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // API pública
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna la sala asignada al cadáver de un NPC.
    /// </summary>
    public string GetCadaverRoom(string npcId)
    {
        if (cadaverLookup.TryGetValue(npcId, out CadaverEntry entry))
        {
            return entry.cadaverRoomId;
        }

        return null;
    }

    /// <summary>
    /// Asigna dinámicamente la sala del cadáver de un NPC.
    /// Útil para colocar el cadáver en la primera sala desbloqueada distinta al lobby.
    /// </summary>
    public void AssignCadaverRoom(string npcId, string roomId)
    {
        if (!cadaverLookup.TryGetValue(npcId, out CadaverEntry entry))
        {
            return;
        }

        entry.cadaverRoomId = roomId;
        Debug.Log($"[CadaverManager] Cadaver de '{npcId}' asignado a sala '{roomId}'.");
    }

    // ─────────────────────────────────────────────────────────────────
    // Lógica interna
    // ─────────────────────────────────────────────────────────────────

    private void OnRoomChanged(string previousRoom, string newRoom)
    {
        if (string.IsNullOrWhiteSpace(newRoom) || StoryState.Instance == null)
        {
            return;
        }

        for (int i = 0; i < cadavers.Count; i++)
        {
            CadaverEntry entry = cadavers[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.npcId))
            {
                continue;
            }

            // ¿Ya fue descubierto?
            if (entry.alreadyDiscovered)
            {
                // Solo refrescar visibilidad
                RefreshCadaverVisibility(entry, newRoom);
                continue;
            }

            // ¿El cadáver está listo y el jugador entró a la sala correcta?
            bool cadaverReady = StoryState.Instance.HasFlag($"npc.cadaver.ready.{entry.npcId}");
            bool inCorrectRoom = string.Equals(newRoom, entry.cadaverRoomId, StringComparison.OrdinalIgnoreCase);

            if (cadaverReady && inCorrectRoom)
            {
                DiscoverCadaver(entry);
            }
            else
            {
                RefreshCadaverVisibility(entry, newRoom);
            }
        }
    }

    private void DiscoverCadaver(CadaverEntry entry)
    {
        entry.alreadyDiscovered = true;
        StoryState.Instance.SetFlag($"npc.cadaver.discovered.{entry.npcId}", true);

        // Mostrar el objeto del cadáver
        if (entry.cadaverObject != null)
        {
            entry.cadaverObject.SetActive(true);
        }

        Debug.Log($"[CadaverManager] Cadaver de '{entry.npcId}' descubierto en sala '{entry.cadaverRoomId}'.");

        // Disparar conversación de descubrimiento
        if (!string.IsNullOrWhiteSpace(entry.discoveryConversationId))
        {
            DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
            if (runner != null && !runner.IsRunning && runner.HasConversation(entry.discoveryConversationId))
            {
                runner.StartConversation(entry.discoveryConversationId, "start");
            }
        }
    }

    private void RefreshCadaverVisibility(CadaverEntry entry, string currentRoom)
    {
        if (entry.cadaverObject == null)
        {
            return;
        }

        bool shouldShow = entry.alreadyDiscovered
                       && string.Equals(currentRoom, entry.cadaverRoomId, StringComparison.OrdinalIgnoreCase);
        entry.cadaverObject.SetActive(shouldShow);
    }

    private void HideAllCadavers()
    {
        for (int i = 0; i < cadavers.Count; i++)
        {
            if (cadavers[i] != null && cadavers[i].cadaverObject != null)
            {
                cadavers[i].cadaverObject.SetActive(false);
            }
        }
    }

    private void RebuildLookup()
    {
        cadaverLookup.Clear();
        for (int i = 0; i < cadavers.Count; i++)
        {
            CadaverEntry entry = cadavers[i];
            if (entry != null && !string.IsNullOrWhiteSpace(entry.npcId))
            {
                cadaverLookup[entry.npcId] = entry;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Utilidad: asignar cadáver a la primera sala desbloqueada disponible
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Dado el npcId, selecciona automáticamente la primera sala que NO sea 'lobby'
    /// ni 'missing' y tenga el flag de desbloqueada (o siempre abierta).
    /// Orden de preferencia: studio, bedroom, gallery, northstreet, livingroom.
    /// </summary>
    public static string ResolvePreferredCadaverRoom(string npcId)
    {
        // Orden de preferencia de salas para colocar cadáveres (de más oscura a más accesible)
        string[] candidates = { "studio", "bedroom", "gallery", "northstreet", "livingroom" };

        // Evitar sala del lobby (start room)
        foreach (string candidate in candidates)
        {
            if (RoomManager.Instance != null && RoomManager.Instance.HasRoom(candidate))
            {
                return candidate;
            }
        }

        return "studio"; // fallback
    }
}
