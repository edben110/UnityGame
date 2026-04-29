using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

public class SetupSceneBackgrounds
{
    private static readonly string[] spriteNames = new[]
    {
        "Ala_norte",
        "Capitulo_1",
        "Estudio_simon",
        "Galeria_de_arte",
        "Habitacion_Simon",
        "Lobby",
    };

    [MenuItem("Tools/Agregar Fondos a MainMapScene")]
    public static void AddBackgroundsToMainMap()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Crear un padre para organizar los fondos
        GameObject parent = GameObject.Find("Backgrounds");
        if (parent == null)
        {
            parent = new GameObject("Backgrounds");
            parent.transform.position = Vector3.zero;
        }

        float xOffset = 0f;

        for (int i = 0; i < spriteNames.Length; i++)
        {
            string spritePath = $"Assets/Sprites/{spriteNames[i]}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (sprite == null)
            {
                Debug.LogWarning($"No se encontró el sprite: {spritePath}");
                continue;
            }

            string goName = $"BG_{spriteNames[i]}";
            GameObject bg = GameObject.Find(goName);
            if (bg == null)
            {
                bg = new GameObject(goName);
                bg.transform.SetParent(parent.transform);
            }

            var sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = bg.AddComponent<SpriteRenderer>();
            }

            sr.sprite = sprite;
            sr.sortingOrder = -100;

            // Posicionar cada fondo uno al lado del otro
            bg.transform.localPosition = new Vector3(xOffset, 0, 0);
            xOffset += sprite.bounds.size.x + 1f; // espacio entre fondos

            Debug.Log($"Fondo agregado: {goName}");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Todos los fondos agregados a MainMapScene.");
        EditorUtility.DisplayDialog("Fondos en MainMapScene",
            $"Se agregaron {spriteNames.Length} fondos a MainMapScene.",
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
                Debug.Log($"Eliminado Background de: {scenePath}");
            }

            GameObject cam = GameObject.Find("Main Camera");
            if (cam != null)
            {
                Object.DestroyImmediate(cam);
                changed = true;
                Debug.Log($"Eliminada Main Camera de: {scenePath}");
            }

            if (changed)
            {
                EditorSceneManager.SaveScene(scene);
                count++;
            }
        }

        Debug.Log($"Limpiadas {count} escenas.");
        EditorUtility.DisplayDialog("Escenas Limpiadas",
            $"Se limpiaron {count} escenas de capitulos.\nSe eliminaron Background y Main Camera.",
            "OK");
    }
}
