using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupSceneBackgrounds
{
    // Salas del juego según el script de Python
    private static readonly (string roomId, string displayName, string spriteName)[] roomDefinitions = new[]
    {
        ("lobby",              "Lobby",                  "Lobby"),
        ("estudio",            "Estudio de Simón",       "Estudio_simon"),
        ("habitacion",         "Habitación de Simón",    "Habitacion_Simon"),
        ("galeria",            "Galería de Arte",        "Galeria_de_arte"),
        ("sala_vigilancia",    "Sala de Vigilancia",     "Ala_norte"),
    };

    // Conexiones entre salas (puertas): sala origen -> sala destino
    private static readonly (string fromRoom, string toRoom, string doorName, Vector3 position)[] doorConnections = new[]
    {
        // Desde el Lobby
        ("lobby", "estudio",         "Puerta_Estudio",        new Vector3(3f, 0f, 0f)),
        ("lobby", "galeria",         "Puerta_Galeria",        new Vector3(-3f, 0f, 0f)),
        ("lobby", "habitacion",      "Puerta_Habitacion",     new Vector3(0f, 3f, 0f)),

        // Desde el Estudio
        ("estudio", "lobby",         "Puerta_Lobby",          new Vector3(-3f, 0f, 0f)),
        ("estudio", "sala_vigilancia","Puerta_Vigilancia",    new Vector3(3f, 0f, 0f)),

        // Desde la Habitación
        ("habitacion", "lobby",      "Puerta_Lobby",          new Vector3(0f, -3f, 0f)),
        ("habitacion", "galeria",    "Puerta_Galeria",        new Vector3(3f, 0f, 0f)),

        // Desde la Galería
        ("galeria", "lobby",         "Puerta_Lobby",          new Vector3(3f, 0f, 0f)),
        ("galeria", "habitacion",    "Puerta_Habitacion",     new Vector3(-3f, 0f, 0f)),

        // Desde la Sala de Vigilancia
        ("sala_vigilancia", "estudio","Puerta_Estudio",       new Vector3(-3f, 0f, 0f)),
        ("sala_vigilancia", "lobby",  "Puerta_Lobby",         new Vector3(0f, -3f, 0f)),
    };

    [MenuItem("Tools/Configurar Salas en MainMapScene")]
    public static void SetupRooms()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Buscar o crear el contenedor Backgrounds
        GameObject bgParent = GameObject.Find("Backgrounds");
        if (bgParent == null)
        {
            bgParent = new GameObject("Backgrounds");
            bgParent.transform.position = Vector3.zero;
        }

        // Buscar o crear el contenedor HotsPots
        GameObject hotspotsParent = GameObject.Find("HotsPots");
        if (hotspotsParent == null)
        {
            hotspotsParent = new GameObject("HotsPots");
            hotspotsParent.transform.position = Vector3.zero;
        }

        // Crear fondos y contenedores de hotspots por sala
        foreach (var (roomId, displayName, spriteName) in roomDefinitions)
        {
            // Fondo
            string bgName = $"BG_{spriteName}";
            GameObject bg = FindChildByName(bgParent.transform, bgName);
            if (bg == null)
            {
                bg = new GameObject(bgName);
                bg.transform.SetParent(bgParent.transform);
                bg.transform.localPosition = Vector3.zero;
            }

            var sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = bg.AddComponent<SpriteRenderer>();
            }

            string spritePath = $"Assets/Sprites/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.sortingOrder = -100;
                Debug.Log($"Fondo configurado: {bgName} -> {spritePath}");
            }
            else
            {
                Debug.LogWarning($"Sprite no encontrado: {spritePath}");
            }

            // Contenedor de hotspots para esta sala
            string hotspotsName = $"Hotspots_{roomId}";
            GameObject roomHotspots = FindChildByName(hotspotsParent.transform, hotspotsName);
            if (roomHotspots == null)
            {
                roomHotspots = new GameObject(hotspotsName);
                roomHotspots.transform.SetParent(hotspotsParent.transform);
                roomHotspots.transform.localPosition = Vector3.zero;
            }

            // Crear puertas para esta sala
            foreach (var (fromRoom, toRoom, doorName, position) in doorConnections)
            {
                if (fromRoom != roomId)
                {
                    continue;
                }

                string fullDoorName = $"Door_{fromRoom}_to_{toRoom}";
                GameObject door = FindChildByName(roomHotspots.transform, fullDoorName);
                if (door == null)
                {
                    door = new GameObject(fullDoorName);
                    door.transform.SetParent(roomHotspots.transform);
                    door.transform.localPosition = position;

                    // Agregar BoxCollider2D
                    var collider = door.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(1.5f, 2f);

                    // Agregar DoorTrigger
                    // No podemos agregar el componente aquí porque necesita compilar primero
                    // Se agrega manualmente o con otro script después
                    Debug.Log($"Puerta creada: {fullDoorName} (de {fromRoom} a {toRoom})");
                }
            }
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Salas configuradas en MainMapScene.");
        EditorUtility.DisplayDialog("Salas Configuradas",
            "Se configuraron las salas con fondos y puertas en MainMapScene.\n\n" +
            "Siguiente paso: Agregar el componente RoomManager al WorldMap\n" +
            "y configurar las referencias en el Inspector.",
            "OK");
    }

    [MenuItem("Tools/Limpiar Fondos de Escenas de Capitulos")]
    public static void CleanChapterScenes()
    {
        string[] chapterScenes = new[]
        {
            "Assets/Scenes/Chapter01Scene.unity",
            "Assets/Scenes/Chapter02Scene.unity",
            "Assets/Scenes/Chapter03Scene.unity",
            "Assets/Scenes/Chapter04Scene.unity",
            "Assets/Scenes/Chapter05Scene.unity",
            "Assets/Scenes/PrologueScene.unity",
        };

        int count = 0;
        foreach (string scenePath in chapterScenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool changed = false;

            GameObject bg = GameObject.Find("Background");
            if (bg != null)
            {
                Object.DestroyImmediate(bg);
                changed = true;
            }

            GameObject cam = GameObject.Find("Main Camera");
            if (cam != null)
            {
                Object.DestroyImmediate(cam);
                changed = true;
            }

            if (changed)
            {
                EditorSceneManager.SaveScene(scene);
                count++;
            }
        }

        EditorUtility.DisplayDialog("Escenas Limpiadas",
            $"Se limpiaron {count} escenas de capítulos.", "OK");
    }

    private static GameObject FindChildByName(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name)
            {
                return parent.GetChild(i).gameObject;
            }
        }
        return null;
    }
}
