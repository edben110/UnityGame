using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupRoomSystem
{
    private static readonly (string roomId, string displayName, string spriteName)[] rooms = new[]
    {
        ("lobby",           "Lobby",                "Lobby"),
        ("estudio",         "Estudio de Simón",     "Estudio_simon"),
        ("habitacion",      "Habitación de Simón",  "Habitacion_Simon"),
        ("galeria",         "Galería de Arte",      "Galeria_de_arte"),
        ("sala_vigilancia", "Sala de Vigilancia",   "Ala_norte"),
    };

    private static readonly (string fromRoom, string toRoom, string doorName, Vector3 position)[] doors = new[]
    {
        ("lobby", "estudio",          "Puerta al Estudio",       new Vector3(3f, 0f, 0f)),
        ("lobby", "galeria",          "Puerta a la Galería",     new Vector3(-3f, 0f, 0f)),
        ("lobby", "habitacion",       "Puerta a la Habitación",  new Vector3(0f, 3f, 0f)),

        ("estudio", "lobby",          "Puerta al Lobby",         new Vector3(-3f, 0f, 0f)),
        ("estudio", "sala_vigilancia","Puerta a Vigilancia",     new Vector3(3f, 0f, 0f)),

        ("habitacion", "lobby",       "Puerta al Lobby",         new Vector3(0f, -3f, 0f)),
        ("habitacion", "galeria",     "Puerta a la Galería",     new Vector3(3f, 0f, 0f)),

        ("galeria", "lobby",          "Puerta al Lobby",         new Vector3(3f, 0f, 0f)),
        ("galeria", "habitacion",     "Puerta a la Habitación",  new Vector3(-3f, 0f, 0f)),

        ("sala_vigilancia", "estudio","Puerta al Estudio",       new Vector3(-3f, 0f, 0f)),
        ("sala_vigilancia", "lobby",  "Puerta al Lobby",         new Vector3(0f, -3f, 0f)),
    };

    [MenuItem("Tools/Configurar Sistema de Salas Completo")]
    public static void SetupAll()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // --- Contenedores ---
        GameObject bgParent = FindOrCreate("Backgrounds", null);
        GameObject hotspotsParent = FindOrCreate("HotsPots", null);

        // --- Crear fondos y contenedores de hotspots ---
        Dictionary<string, GameObject> bgObjects = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> hotspotContainers = new Dictionary<string, GameObject>();

        foreach (var (roomId, displayName, spriteName) in rooms)
        {
            // Fondo
            string bgName = $"BG_{spriteName}";
            GameObject bg = FindOrCreateChild(bgName, bgParent.transform);
            var sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bg.AddComponent<SpriteRenderer>();

            string spritePath = $"Assets/Sprites/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.sortingOrder = -100;
            }
            bg.transform.localPosition = Vector3.zero;
            bgObjects[roomId] = bg;

            // Contenedor de hotspots
            string hsName = $"Hotspots_{roomId}";
            GameObject hs = FindOrCreateChild(hsName, hotspotsParent.transform);
            hs.transform.localPosition = Vector3.zero;
            hotspotContainers[roomId] = hs;
        }

        // --- Crear puertas con DoorTrigger ---
        foreach (var (fromRoom, toRoom, doorName, position) in doors)
        {
            if (!hotspotContainers.TryGetValue(fromRoom, out GameObject parent))
                continue;

            string fullName = $"Door_{fromRoom}_to_{toRoom}";
            GameObject door = FindOrCreateChild(fullName, parent.transform);
            door.transform.localPosition = position;

            // BoxCollider2D
            var collider = door.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = door.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1.5f, 2f);
            }

            // DoorTrigger
            var trigger = door.GetComponent<DoorTrigger>();
            if (trigger == null)
            {
                trigger = door.AddComponent<DoorTrigger>();
            }

            // Configurar targetRoomId via SerializedObject
            SerializedObject so = new SerializedObject(trigger);
            var targetProp = so.FindProperty("targetRoomId");
            if (targetProp != null)
            {
                targetProp.stringValue = toRoom;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Debug.Log($"Puerta: {fullName} -> {toRoom}");
        }

        // --- RoomManager en WorldMap ---
        GameObject worldMap = GameObject.Find("WorldMap");
        if (worldMap == null)
        {
            worldMap = FindOrCreate("WorldMap", null);
        }

        var roomManager = worldMap.GetComponent<RoomManager>();
        if (roomManager == null)
        {
            roomManager = worldMap.AddComponent<RoomManager>();
        }

        // Configurar la lista de salas
        SerializedObject rmSO = new SerializedObject(roomManager);
        var roomsProp = rmSO.FindProperty("rooms");
        roomsProp.ClearArray();

        for (int i = 0; i < rooms.Length; i++)
        {
            var (roomId, displayName, spriteName) = rooms[i];

            roomsProp.InsertArrayElementAtIndex(i);
            var element = roomsProp.GetArrayElementAtIndex(i);

            element.FindPropertyRelative("roomId").stringValue = roomId;
            element.FindPropertyRelative("displayName").stringValue = displayName;

            if (bgObjects.TryGetValue(roomId, out GameObject bgObj))
                element.FindPropertyRelative("backgroundObject").objectReferenceValue = bgObj;

            if (hotspotContainers.TryGetValue(roomId, out GameObject hsObj))
                element.FindPropertyRelative("hotspotsContainer").objectReferenceValue = hsObj;
        }

        var startProp = rmSO.FindProperty("startingRoomId");
        if (startProp != null)
            startProp.stringValue = "lobby";

        rmSO.ApplyModifiedPropertiesWithoutUndo();

        // --- NpcLocationManager en WorldMap ---
        var npcManager = worldMap.GetComponent<NpcLocationManager>();
        if (npcManager == null)
        {
            npcManager = worldMap.AddComponent<NpcLocationManager>();
        }

        // --- Guardar ---
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Sistema de salas configurado completamente.");
        EditorUtility.DisplayDialog("Sistema de Salas Listo",
            "Se configuró todo en MainMapScene:\n\n" +
            "✓ 5 fondos de sala\n" +
            "✓ 11 puertas con DoorTrigger\n" +
            "✓ RoomManager en WorldMap\n" +
            "✓ NpcLocationManager en WorldMap\n\n" +
            "Dale Play para probar la navegación entre salas.",
            "OK");
    }

    private static GameObject FindOrCreate(string name, Transform parent)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
            if (parent != null)
                obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
        }
        return obj;
    }

    private static GameObject FindOrCreateChild(string name, Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
                return parent.GetChild(i).gameObject;
        }

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }
}
