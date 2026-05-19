using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Reemplaza Key_BedroomKey por Puzzle_Box (hotspot) y registra AcertijoScene en Build Settings.
/// </summary>
public static class SetupBedroomPuzzleBox
{
    private const string MainMapScenePath = "Assets/Scenes/MainMapScene.unity";
    private const string AcertijoScenePath = "Assets/Scenes/AcertijoScene.unity";

    [MenuItem("Tools/Puzzle/Configurar Puzzle_Box y AcertijoScene")]
    public static void Configure()
    {
        EnsureAcertijoSceneInBuild();
        SetupAcertijoSceneBootstrap();
        SetupMainMapHotspot();

        EditorUtility.DisplayDialog(
            "Puzzle habitación",
            "Listo:\n" +
            "- Key_BedroomKey → Puzzle_Box (hotspot con ítem puzzle_box)\n" +
            "- AcertijoScene en Build Settings\n" +
            "- Bootstrap del acertijo añadido\n\n" +
            "Al hacer clic en puzzle_box en el inventario se abre el acertijo.\n" +
            "Al resolverlo se obtiene BedroomKey.",
            "OK");
    }

    private static void EnsureAcertijoSceneInBuild()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == AcertijoScenePath)
            {
                scenes[i] = new EditorBuildSettingsScene(AcertijoScenePath, true);
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(AcertijoScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void SetupAcertijoSceneBootstrap()
    {
        var scene = EditorSceneManager.OpenScene(AcertijoScenePath, OpenSceneMode.Additive);
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogWarning("SetupBedroomPuzzleBox: no se encontró Canvas en AcertijoScene.");
            EditorSceneManager.CloseScene(scene, true);
            return;
        }

        if (canvas.GetComponent<AcertijoSceneBootstrap>() == null)
        {
            canvas.AddComponent<AcertijoSceneBootstrap>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, true);
    }

    private static void SetupMainMapHotspot()
    {
        var scene = EditorSceneManager.OpenScene(MainMapScenePath, OpenSceneMode.Single);

        GameObject keyObj = GameObject.Find("Key_BedroomKey");
        if (keyObj == null)
        {
            keyObj = GameObject.Find("Puzzle_Box");
        }

        if (keyObj == null)
        {
            Debug.LogError("No se encontró Key_BedroomKey ni Puzzle_Box en MainMapScene.");
            return;
        }

        keyObj.name = "Puzzle_Box";

        KeyItem keyItem = keyObj.GetComponent<KeyItem>();
        if (keyItem != null)
        {
            Object.DestroyImmediate(keyItem, true);
        }

        MapHotspot hotspot = keyObj.GetComponent<MapHotspot>();
        if (hotspot == null)
        {
            hotspot = keyObj.AddComponent<MapHotspot>();
        }

        SerializedObject so = new SerializedObject(hotspot);
        so.FindProperty("hotspotId").stringValue = "bedroom_puzzle_box";
        so.FindProperty("requiredChapterId").stringValue = "chapter2";
        so.FindProperty("conversationId").stringValue = string.Empty;
        so.FindProperty("startNodeId").stringValue = "start";
        so.FindProperty("consumeAfterUse").boolValue = false;
        so.FindProperty("setFlagOnInteract").stringValue = string.Empty;
        so.FindProperty("grantItemId").stringValue = AcertijoPuzzleService.PuzzleBoxItemId;
        so.FindProperty("grantItemDisplayName").stringValue = "Caja puzzle cerrada";
        so.FindProperty("grantItemDescription").stringValue =
            "Una caja de madera con un mecanismo de botones en la tapa. Parece que hay que pulsar los botones en el orden correcto para abrirla.";
        so.FindProperty("hideAfterPickup").boolValue = true;
        so.FindProperty("showDebugMarkerInGame").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        Sprite boxSprite = LoadPuzzleBoxSprite();
        if (boxSprite != null)
        {
            so.FindProperty("grantItemSprite").objectReferenceValue = boxSprite;

            SpriteRenderer sr = keyObj.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = keyObj.AddComponent<SpriteRenderer>();
            }

            sr.sprite = boxSprite;
            sr.sortingOrder = 50;
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("MainMapScene: Puzzle_Box configurado en Hotspots.");
    }

    private static Sprite LoadPuzzleBoxSprite()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Puzzle/acertijo 1_0");
        if (sprite != null)
        {
            return sprite;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Puzzle/acertijo 1.png");
        if (assets != null)
        {
            foreach (Object asset in assets)
            {
                if (asset is Sprite s && s.name.Contains("0"))
                {
                    return s;
                }
            }

            foreach (Object asset in assets)
            {
                if (asset is Sprite s)
                {
                    return s;
                }
            }
        }

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Puzzle/puzzle_box_icon.png");
        if (sprite != null)
        {
            return sprite;
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Puzzle/acertijo2.png");
    }
}
