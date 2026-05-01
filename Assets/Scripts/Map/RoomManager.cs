using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la navegación entre salas dentro de MainMapScene.
/// Cada sala tiene un fondo (SpriteRenderer) y opcionalmente un contenedor de interactuables.
/// Al cambiar de sala, se oculta todo y se muestra solo la sala nueva.
/// </summary>
public class RoomManager : MonoBehaviour
{
    [Serializable]
    public class RoomDefinition
    {
        public string roomId;
        public string displayName;
        public GameObject backgroundObject;
        public GameObject hotspotsContainer;
    }

    public static RoomManager Instance { get; private set; }

    public event Action<string, string> RoomChanged;

    [Header("Salas")]
    [SerializeField] private List<RoomDefinition> rooms = new List<RoomDefinition>();

    [Header("Sala inicial")]
    [SerializeField] private string startingRoomId = "lobby";

    private string currentRoomId;
    private readonly Dictionary<string, RoomDefinition> roomLookup = new Dictionary<string, RoomDefinition>();

    public string CurrentRoomId => currentRoomId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        roomLookup.Clear();
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomDefinition room = rooms[i];
            if (room == null || string.IsNullOrWhiteSpace(room.roomId))
            {
                continue;
            }

            roomLookup[room.roomId] = room;
        }

        HideAllRooms();
    }

    private void Start()
    {
        Debug.Log($"[RoomManager] Inicializado. {roomLookup.Count} salas registradas.");
        foreach (var pair in roomLookup)
        {
            var r = pair.Value;
            Debug.Log($"  Sala '{pair.Key}': bg={r.backgroundObject?.name ?? "null"} (active={r.backgroundObject?.activeSelf}), hotspots={r.hotspotsContainer?.name ?? "null"} (active={r.hotspotsContainer?.activeSelf})");
        }

        ChangeRoom(startingRoomId);
    }

    public bool ChangeRoom(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            Debug.LogWarning("[RoomManager] roomId vacío.");
            return false;
        }

        if (!roomLookup.TryGetValue(roomId, out RoomDefinition targetRoom))
        {
            Debug.LogError($"[RoomManager] Sala no encontrada: '{roomId}'");
            return false;
        }

        if (currentRoomId == roomId)
        {
            return true;
        }

        string previousRoom = currentRoomId;

        // Ocultar TODAS las salas
        HideAllRooms();

        // Mostrar la nueva
        if (targetRoom.backgroundObject != null)
        {
            targetRoom.backgroundObject.SetActive(true);
            Debug.Log($"[RoomManager] Fondo activado: {targetRoom.backgroundObject.name}");
        }

        if (targetRoom.hotspotsContainer != null)
        {
            targetRoom.hotspotsContainer.SetActive(true);
            // También activar todos los hijos
            for (int i = 0; i < targetRoom.hotspotsContainer.transform.childCount; i++)
            {
                targetRoom.hotspotsContainer.transform.GetChild(i).gameObject.SetActive(true);
            }
            Debug.Log($"[RoomManager] Hotspots activados: {targetRoom.hotspotsContainer.name} ({targetRoom.hotspotsContainer.transform.childCount} hijos)");
        }

        currentRoomId = roomId;

        Debug.Log($"[RoomManager] *** CAMBIO DE SALA: [{previousRoom ?? "inicio"}] -> [{roomId}] ({targetRoom.displayName}) ***");
        RoomChanged?.Invoke(previousRoom, roomId);
        return true;
    }

    public bool HasRoom(string roomId)
    {
        return !string.IsNullOrWhiteSpace(roomId) && roomLookup.ContainsKey(roomId);
    }

    public string GetCurrentRoomDisplayName()
    {
        if (string.IsNullOrWhiteSpace(currentRoomId) || !roomLookup.TryGetValue(currentRoomId, out RoomDefinition room))
        {
            return string.Empty;
        }

        return room.displayName;
    }

    private void HideAllRooms()
    {
        foreach (var pair in roomLookup)
        {
            RoomDefinition room = pair.Value;
            if (room.backgroundObject != null)
            {
                room.backgroundObject.SetActive(false);
            }

            if (room.hotspotsContainer != null)
            {
                room.hotspotsContainer.SetActive(false);
            }
        }
    }
}
