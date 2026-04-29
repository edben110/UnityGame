using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;

public class SetupSceneBackgrounds
{
    private static readonly (string scenePath, string spriteName)[] sceneBackgrounds = new[]
    {
        ("Assets/Scenes/Chapter01Scene.unity", "Capitulo_1"),
        ("Assets/Scenes/Chapter02Scene.unity", "Lobby"),
        ("Assets/Scenes/Chapter03Scene.unity", "Habitacion_Simon"),
        ("Assets/Scenes/Chapter04Scene.unity", "Estudio_simon"),
        ("Assets/Scenes/Chapter05Scene.unity", "Galeria_de_arte"),
        ("Assets/Scenes/PrologueScene.unity",  "Ala_norte"),
    };

    [MenuItem("Tools/Asignar Fondos a Escenas")]
    public static void AssignBackgrounds()
    {
        int count = 0;

        foreach (var (scenePath, spriteName) in sceneBackgrounds)
        {
            string spritePath = $"Assets/Sprites/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (sprite == null)
            {
                Debug.LogWarning($"No se encontró el sprite: {spritePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // --- Cámara ---
            Camera cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                GameObject camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<UniversalAdditionalCameraData>();
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                cam.nearClipPlane = -10f;
                cam.farClipPlane = 10f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                camGO.transform.position = new Vector3(0, 0, -10);
                Debug.Log($"Cámara creada en: {scenePath}");
            }
            else
            {
                cam.orthographic = true;
                cam.orthographicSize = 5f;
            }

            // --- Fondo ---
            GameObject bg = GameObject.Find("Background");
            if (bg == null)
            {
                bg = new GameObject("Background");
                bg.AddComponent<SpriteRenderer>();
            }

            var sr = bg.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -100;

            // Escalar el sprite para que cubra toda la cámara
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;

            float spriteHeight = sprite.bounds.size.y;
            float spriteWidth = sprite.bounds.size.x;

            float scaleX = camWidth / spriteWidth;
            float scaleY = camHeight / spriteHeight;
            float scale = Mathf.Max(scaleX, scaleY); // cubrir toda la pantalla

            bg.transform.localScale = new Vector3(scale, scale, 1f);
            bg.transform.position = Vector3.zero;

            EditorSceneManager.SaveScene(scene);
            count++;
            Debug.Log($"Escena configurada: {scenePath} -> {spriteName}");
        }

        Debug.Log($"Fondos y cámaras configurados en {count} escenas.");
        EditorUtility.DisplayDialog("Escenas Configuradas",
            $"Se configuraron {count} escenas con cámara y fondo.\nRevisa la consola para detalles.",
            "OK");
    }
}
