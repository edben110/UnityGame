using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script de depuración para verificar y crear cámaras para cada room en RoomManager.
/// Este script debe ejecutarse una sola vez en el Editor.
/// </summary>
public class RoomCameraSetup : MonoBehaviour
{
    [ContextMenu("Verificar y Crear Cámaras Faltantes")]
    public void SetupCameras()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("RoomManager no encontrado");
            return;
        }

        // Usar reflexión para acceder a la lista de rooms privada
        var roomsField = RoomManager.Instance.GetType().GetField("rooms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rooms = roomsField.GetValue(RoomManager.Instance) as List<RoomManager.RoomDefinition>;

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("No hay rooms configurados en RoomManager");
            return;
        }

        Debug.Log("=== VERIFICANDO ROOMS Y CÁMARAS ===");
        List<string> roomsNeedingCamera = new List<string>();

        foreach (var room in rooms)
        {
            if (room == null || string.IsNullOrWhiteSpace(room.roomId))
                continue;

            if (room.cameraObject == null)
            {
                Debug.LogWarning($"Room '{room.roomId}' NO TIENE CÁMARA ASIGNADA");
                roomsNeedingCamera.Add(room.roomId);
            }
            else
            {
                Debug.Log($"✓ Room '{room.roomId}' tiene cámara: {room.cameraObject.name}");
            }
        }

        if (roomsNeedingCamera.Count > 0)
        {
            Debug.Log($"\n=== CREANDO {roomsNeedingCamera.Count} CÁMARAS FALTANTES ===");
            foreach (var room in rooms)
            {
                if (room == null || !roomsNeedingCamera.Contains(room.roomId))
                    continue;

                // Crear GameObject para la cámara
                string cameraName = $"Camera_{room.roomId}";
                GameObject cameraGO = new GameObject(cameraName);
                cameraGO.tag = "MainCamera";
                
                // Agregar componente Camera
                Camera camera = cameraGO.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.depth = 0;

                // Posicionar en el centro de la sala
                if (room.backgroundObject != null)
                {
                    cameraGO.transform.position = room.backgroundObject.transform.position + Vector3.back * 10f;
                }
                else
                {
                    cameraGO.transform.position = Vector3.back * 10f;
                }

                cameraGO.transform.SetParent(transform);

                // Asignar a la room
                room.cameraObject = camera;

                Debug.Log($"✓ Cámara creada para room '{room.roomId}': {cameraName}");
            }

            // Marcar la escena como modificada
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(RoomManager.Instance.gameObject);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif

            Debug.Log("\n=== CONFIGURACIÓN COMPLETADA ===");
        }
        else
        {
            Debug.Log("✓ Todas las rooms ya tienen cámaras asignadas");
        }
    }

    [ContextMenu("Listar Configuración Actual")]
    public void ListCameraConfig()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("RoomManager no encontrado");
            return;
        }

        var roomsField = RoomManager.Instance.GetType().GetField("rooms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rooms = roomsField.GetValue(RoomManager.Instance) as List<RoomManager.RoomDefinition>;

        Debug.Log("\n=== CONFIGURACIÓN DE ROOMS ===");
        foreach (var room in rooms)
        {
            if (room == null)
                continue;

            string status = room.cameraObject != null ? "✓" : "✗";
            Debug.Log($"{status} {room.roomId} → Camera: {room.cameraObject?.name ?? "FALTA"}");
        }

        var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log($"\n=== CÁMARAS EN ESCENA: {cameras.Length} ===");
        foreach (var cam in cameras)
        {
            Debug.Log($"  {cam.gameObject.name} (enabled: {cam.enabled})");
        }
    }
}
