using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PositionNpcsAndDoors
{
    // Posiciones calculadas para el fondo del Lobby (aspecto ~16:9, cámara ortográfica size 5)
    // Y = -2.5 es el nivel del suelo visual en el lobby
    private static readonly float floorY = -2.0f;

    // NPCs distribuidos en el lobby, cada uno cerca de una puerta o zona
    private static readonly (string npcName, float x, float y, float scale)[] npcPositions = new[]
    {
        ("NPC_Robert", -4.5f, floorY, 1.8f),   // Izquierda, cerca de puerta a galería
        ("NPC_Ana",    -2.0f, floorY, 1.8f),    // Centro-izquierda
        ("NPC_Ben",     0.0f, floorY, 1.8f),    // Centro, frente a la escalera
        ("NPC_Lisa",    2.0f, floorY, 1.8f),    // Centro-derecha
        ("NPC_Lucas",   4.5f, floorY, 1.8f),    // Derecha, cerca de puerta al estudio
    };

    // Puertas posicionadas en los bordes de la imagen
    private static readonly (string doorName, float x, float y, float sizeX, float sizeY)[] doorPositions = new[]
    {
        // Lobby doors
        ("Door_lobby_to_galeria",     -6.5f, -1.0f, 1.5f, 3.5f),  // Puerta izquierda
        ("Door_lobby_to_estudio",      6.5f, -1.0f, 1.5f, 3.5f),  // Puerta derecha
        ("Door_lobby_to_habitacion",   0.0f,  3.0f, 2.0f, 1.5f),  // Escalera arriba

        // Estudio doors
        ("Door_estudio_to_lobby",         -6.5f, -1.0f, 1.5f, 3.5f),
        ("Door_estudio_to_sala_vigilancia", 6.5f, -1.0f, 1.5f, 3.5f),

        // Habitación doors
        ("Door_habitacion_to_lobby",    0.0f, -3.5f, 2.0f, 1.5f),
        ("Door_habitacion_to_galeria",  6.5f, -1.0f, 1.5f, 3.5f),

        // Galería doors
        ("Door_galeria_to_lobby",       6.5f, -1.0f, 1.5f, 3.5f),
        ("Door_galeria_to_habitacion", -6.5f, -1.0f, 1.5f, 3.5f),

        // Sala de vigilancia doors
        ("Door_sala_vigilancia_to_estudio", -6.5f, -1.0f, 1.5f, 3.5f),
        ("Door_sala_vigilancia_to_lobby",    0.0f, -3.5f, 2.0f, 1.5f),
    };

    [MenuItem("Tools/Posicionar NPCs y Puertas")]
    public static void PositionAll()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        int npcCount = 0;
        int doorCount = 0;

        // --- Posicionar NPCs ---
        foreach (var (npcName, x, y, scale) in npcPositions)
        {
            GameObject npc = FindInScene(npcName);
            if (npc == null)
            {
                Debug.LogWarning($"NPC no encontrado: {npcName}");
                continue;
            }

            npc.transform.position = new Vector3(x, y, 0f);

            // Escalar basado en el sprite
            SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                float targetHeight = scale;
                float spriteHeight = sr.sprite.bounds.size.y;
                if (spriteHeight > 0)
                {
                    float s = targetHeight / spriteHeight;
                    npc.transform.localScale = new Vector3(s, s, 1f);
                }

                // Ajustar el pivot para que los pies estén en la posición Y
                // El sprite se renderiza desde el centro, así que subimos medio sprite
                float halfHeight = (sr.sprite.bounds.size.y * npc.transform.localScale.y) / 2f;
                npc.transform.position = new Vector3(x, y + halfHeight, 0f);

                sr.sortingOrder = 10;
            }

            npcCount++;
            Debug.Log($"NPC posicionado: {npcName} en ({x}, {y})");
        }

        // --- Posicionar y redimensionar puertas ---
        foreach (var (doorName, x, y, sizeX, sizeY) in doorPositions)
        {
            GameObject door = FindInScene(doorName);
            if (door == null)
            {
                continue;
            }

            door.transform.localPosition = new Vector3(x, y, 0f);

            BoxCollider2D col = door.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(sizeX, sizeY);
            }

            // Quitar el SpriteRenderer de debug si existe (los cuadrados amarillos)
            SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Object.DestroyImmediate(sr);
            }

            doorCount++;
        }

        // --- Posicionar hotspots originales del lobby ---
        PositionOriginalHotspot("HS_Lobby_Book",  -1.5f, -1.5f);
        PositionOriginalHotspot("HS_Lobby_Coat",   3.5f, -0.5f);
        PositionOriginalHotspot("HS_Lobby_Photo",  0.0f,  1.5f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Posiciones Actualizadas",
            $"NPCs posicionados: {npcCount}\n" +
            $"Puertas posicionadas: {doorCount}\n\n" +
            "Los NPCs están en el suelo del lobby.\n" +
            "Las puertas están en los bordes (izquierda, derecha, arriba).\n" +
            "Los cuadrados amarillos de debug fueron eliminados.",
            "OK");
    }

    private static void PositionOriginalHotspot(string name, float x, float y)
    {
        GameObject obj = FindInScene(name);
        if (obj == null) return;

        obj.transform.localPosition = new Vector3(x, y, 0f);
        Debug.Log($"Hotspot posicionado: {name} en ({x}, {y})");
    }

    private static GameObject FindInScene(string name)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.name == name && t.gameObject.scene.isLoaded)
            {
                return t.gameObject;
            }
        }
        return null;
    }
}
