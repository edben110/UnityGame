using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class FixHotspotsHierarchy
{
    [MenuItem("Tools/Arreglar Jerarquía de Hotspots y Puertas")]
    public static void Fix()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject hotsPots = GameObject.Find("HotsPots");
        if (hotsPots == null)
        {
            Debug.LogError("No se encontró HotsPots en la escena.");
            return;
        }

        // Encontrar el contenedor Hotspots_lobby
        Transform lobbyContainer = null;
        for (int i = 0; i < hotsPots.transform.childCount; i++)
        {
            Transform child = hotsPots.transform.GetChild(i);
            if (child.name == "Hotspots_lobby")
            {
                lobbyContainer = child;
                break;
            }
        }

        if (lobbyContainer == null)
        {
            Debug.LogError("No se encontró Hotspots_lobby.");
            return;
        }

        // Mover hotspots sueltos (HS_*) que están directamente en HotsPots al contenedor del lobby
        List<Transform> toMove = new List<Transform>();
        for (int i = 0; i < hotsPots.transform.childCount; i++)
        {
            Transform child = hotsPots.transform.GetChild(i);
            // Si no es un contenedor de sala (Hotspots_*), moverlo al lobby
            if (!child.name.StartsWith("Hotspots_"))
            {
                toMove.Add(child);
            }
        }

        foreach (Transform t in toMove)
        {
            Debug.Log($"Movido '{t.name}' de HotsPots -> Hotspots_lobby");
            t.SetParent(lobbyContainer);
        }

        // Verificar que TODAS las puertas tengan BoxCollider2D y DoorTrigger
        int doorsFixed = 0;
        DoorTrigger[] allDoors = Object.FindObjectsByType<DoorTrigger>(FindObjectsSortMode.None);
        foreach (DoorTrigger door in allDoors)
        {
            BoxCollider2D col = door.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = door.gameObject.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.5f, 2f);
                doorsFixed++;
            }

            // Asegurar que el collider no sea trigger (necesitamos raycast, no trigger)
            col.isTrigger = false;

            Debug.Log($"Puerta verificada: {door.gameObject.name} -> collider size: {col.size}, enabled: {col.enabled}");
        }

        // Verificar que el ClickManager existe
        ClickManager clickManager = Object.FindFirstObjectByType<ClickManager>();
        if (clickManager == null)
        {
            Debug.LogWarning("No se encontró ClickManager en la escena. Los clics no funcionarán.");
        }
        else
        {
            Debug.Log($"ClickManager encontrado en: {clickManager.gameObject.name}");
        }

        // Log del estado final
        Debug.Log("=== Estado final de HotsPots ===");
        LogHierarchy(hotsPots.transform, 0);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Jerarquía Arreglada",
            $"Hotspots sueltos movidos a Hotspots_lobby: {toMove.Count}\n" +
            $"Puertas verificadas: {allDoors.Length}\n" +
            $"Puertas corregidas: {doorsFixed}\n\n" +
            "Dale Play y revisa la consola.",
            "OK");
    }

    private static void LogHierarchy(Transform t, int depth)
    {
        string indent = new string(' ', depth * 2);
        string active = t.gameObject.activeSelf ? "✓" : "✗";
        Debug.Log($"{indent}{active} {t.name}");
        for (int i = 0; i < t.childCount; i++)
        {
            LogHierarchy(t.GetChild(i), depth + 1);
        }
    }
}
