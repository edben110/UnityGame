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
        public Camera cameraObject;
    }

    public static RoomManager Instance { get; private set; }

    public event Action<string, string> RoomChanged;

    [Header("Salas")]
    [SerializeField] private List<RoomDefinition> rooms = new List<RoomDefinition>();

    [Header("Sala inicial")]
    [SerializeField] private string startingRoomId = "lobby";

    private string currentRoomId;
    private RoomDefinition currentRoomDefinition;
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

        // Ocultar TODAS las salas y sus cámaras
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

        // Cambiar cámara si está asignada o puede resolverse
        Camera roomCamera = ResolveRoomCamera(targetRoom);
        if (roomCamera != null)
        {
            targetRoom.cameraObject = roomCamera;
            PositionRoomCamera(targetRoom);
            roomCamera.enabled = true;
            Debug.Log($"[RoomManager] Cámara activada: {roomCamera.name}");
        }
        else
        {
            Debug.LogWarning($"[RoomManager] No hay cámara asignada para la sala '{roomId}'");
        }

        currentRoomDefinition = targetRoom;
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

    public string GetRoomDisplayName(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
        {
            return string.Empty;
        }

        if (roomLookup.TryGetValue(roomId, out RoomDefinition room) && !string.IsNullOrWhiteSpace(room.displayName))
        {
            return room.displayName;
        }

        return roomId;
    }

    public bool IsObjectInCurrentRoom(GameObject target)
    {
        if (target == null || !target.activeInHierarchy)
        {
            return false;
        }

        NpcInteractable npc = target.GetComponentInParent<NpcInteractable>();
        if (npc != null)
        {
            return IsNpcInCurrentRoom(npc);
        }

        if (currentRoomDefinition == null)
        {
            return true;
        }

        Transform targetTransform = target.transform;
        bool isInHotspots = IsSameOrChildOf(targetTransform, currentRoomDefinition.hotspotsContainer);
        bool isInBackground = IsSameOrChildOf(targetTransform, currentRoomDefinition.backgroundObject);
        if (isInHotspots || isInBackground)
        {
            return true;
        }

        bool hasRoomRoots = currentRoomDefinition.hotspotsContainer != null || currentRoomDefinition.backgroundObject != null;
        if (hasRoomRoots)
        {
            return false;
        }

        return true;
    }

    private bool IsNpcInCurrentRoom(NpcInteractable npc)
    {
        if (npc == null)
        {
            return false;
        }

        if (NpcLocationManager.Instance == null)
        {
            return true;
        }

        string npcRoom = NpcLocationManager.Instance.GetNpcRoom(npc.NpcId);
        if (string.IsNullOrWhiteSpace(npcRoom))
        {
            return false;
        }

        return string.Equals(npcRoom, currentRoomId, StringComparison.OrdinalIgnoreCase);
    }

    private void HideAllRooms()
    {
        currentRoomDefinition = null;

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

            if (room.cameraObject != null)
            {
                room.cameraObject.enabled = false;
            }
        }
    }

    private Camera ResolveRoomCamera(RoomDefinition room)
    {
        if (room == null)
        {
            return null;
        }

        if (room.cameraObject != null)
        {
            return room.cameraObject;
        }

        string cameraName = $"Camera_{room.roomId}";
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (candidate != null && candidate.name == cameraName)
            {
                return candidate;
            }
        }

        if (Camera.main != null)
        {
            return Camera.main;
        }

        if (cameras.Length > 0)
        {
            return cameras[0];
        }

        return null;
    }

    private void PositionRoomCamera(RoomDefinition room)
    {
        if (room == null || room.cameraObject == null)
        {
            return;
        }

        Vector3 focus = GetRoomFocusPosition(room);
        room.cameraObject.transform.position = focus + Vector3.back * 10f;

        if (room.cameraObject.gameObject.tag != "MainCamera")
        {
            room.cameraObject.gameObject.tag = "MainCamera";
        }
    }

    private static Vector3 GetRoomFocusPosition(RoomDefinition room)
    {
        if (room.backgroundObject != null)
        {
            return room.backgroundObject.transform.position;
        }

        if (room.hotspotsContainer != null)
        {
            return room.hotspotsContainer.transform.position;
        }

        return Vector3.zero;
    }

    private static bool IsSameOrChildOf(Transform target, GameObject root)
    {
        if (target == null || root == null)
        {
            return false;
        }

        Transform rootTransform = root.transform;
        return target == rootTransform || target.IsChildOf(rootTransform);
    }
}
