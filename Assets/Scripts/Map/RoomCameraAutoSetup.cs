using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Auto-configuración de cámaras para rooms.
/// Se ejecuta automáticamente en Awake antes del RoomManager.
/// </summary>
public class RoomCameraAutoSetup : MonoBehaviour
{
    private void Awake()
    {
        // Ejecutar ANTES de que RoomManager se inicialice
        SetupCamerasIfNeeded();
    }

    private void SetupCamerasIfNeeded()
    {
        var roomManager = FindAnyObjectByType<RoomManager>();
        if (roomManager == null)
            return;

        // Usar reflexión para acceder a la lista de rooms privada
        var roomsField = typeof(RoomManager).GetField("rooms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (roomsField == null)
            return;

        var rooms = roomsField.GetValue(roomManager) as List<RoomManager.RoomDefinition>;
        if (rooms == null || rooms.Count == 0)
            return;

        bool needsSetup = false;
        foreach (var room in rooms)
        {
            if (room != null && room.cameraObject == null)
            {
                needsSetup = true;
                break;
            }
        }

        if (!needsSetup)
            return;

        Debug.Log("🔧 [RoomCameraAutoSetup] Configurando cámaras faltantes...");

        foreach (var room in rooms)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.roomId))
                continue;

            if (room.cameraObject != null)
                continue;

            // Crear cámara para esta room
            string cameraName = $"Camera_{room.roomId}";
            GameObject cameraGO = new GameObject(cameraName);
            cameraGO.tag = "MainCamera";

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.depth = 0;
            camera.enabled = false; // Se activará cuando se entre al room

            // Posicionar según el background de la room
            Vector3 position = Vector3.back * 10f;
            if (room.backgroundObject != null)
            {
                position = room.backgroundObject.transform.position + Vector3.back * 10f;
            }
            cameraGO.transform.position = position;

            // Opcionalmente: hacer que sea hijo del RoomManager para organización
            cameraGO.transform.SetParent(roomManager.transform);

            // Asignar la cámara al room
            room.cameraObject = camera;

            Debug.Log($"  ✓ Cámara creada: {cameraName} en posición {position}");
        }

        Debug.Log("🔧 [RoomCameraAutoSetup] ¡Configuración completada!");
    }
}
