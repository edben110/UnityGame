using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupGalleryPuzzleBox
{
    private const string MainMapScenePath = "Assets/Scenes/MainMapScene.unity";
    private const string Acertijo2ScenePath = "Assets/Scenes/Acertijo2Scene.unity";

    [MenuItem("Tools/Puzzle/Configurar Acertijo2 (HS_Bed + GalleryKey)")]
    public static void Configure()
    {
        EnsureAcertijo2InBuild();
        SetupAcertijo2SceneBootstrap();
        SetupMainMapHotspot();
        RemoveGalleryKeyFromScene();

        EditorUtility.DisplayDialog(
            "Acertijo 2",
            "Listo:\n" +
            "- HS_Bed otorga puzzle_box_2 tras el diálogo\n" +
            "- Key_GalleryKey eliminada del mapa\n" +
            "- Acertijo2Scene en Build Settings\n\n" +
            "Clic en puzzle_box_2 en inventario → Acertijo2Scene.\n" +
            "Al resolver → GalleryKey.",
            "OK");
    }

    private static void EnsureAcertijo2InBuild()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == Acertijo2ScenePath)
            {
                scenes[i] = new EditorBuildSettingsScene(Acertijo2ScenePath, true);
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(Acertijo2ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void SetupAcertijo2SceneBootstrap()
    {
        var scene = EditorSceneManager.OpenScene(Acertijo2ScenePath, OpenSceneMode.Additive);
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null && canvas.GetComponent<Acertijo2SceneBootstrap>() == null)
        {
            canvas.AddComponent<Acertijo2SceneBootstrap>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, true);
    }

    private static void SetupMainMapHotspot()
    {
        var scene = EditorSceneManager.OpenScene(MainMapScenePath, OpenSceneMode.Single);
        GameObject bed = GameObject.Find("HS_Bed");
        if (bed == null)
        {
            Debug.LogError("No se encontró HS_Bed.");
            return;
        }

        MapHotspot hotspot = bed.GetComponent<MapHotspot>();
        if (hotspot == null)
        {
            hotspot = bed.AddComponent<MapHotspot>();
        }

        SerializedObject so = new SerializedObject(hotspot);
        so.FindProperty("grantItemIdAfterDialogue").stringValue = Acertijo2PuzzleService.PuzzleBoxItemId;
        so.FindProperty("grantItemDisplayNameAfterDialogue").stringValue = "Caja puzzle de la cama";
        so.FindProperty("grantItemDescriptionAfterDialogue").stringValue =
            "Caja de madera con un rompecabezas deslizante en la tapa. Ábrela desde el inventario para obtener la llave de la galería.";

        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Puzzle/acertijo2.png");
        if (icon == null)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Puzzle/acertijo2.png");
            if (assets != null)
            {
                foreach (Object asset in assets)
                {
                    if (asset is Sprite sprite)
                    {
                        icon = sprite;
                        break;
                    }
                }
            }
        }

        so.FindProperty("grantItemSpriteAfterDialogue").objectReferenceValue = icon;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RemoveGalleryKeyFromScene()
    {
        GameObject key = GameObject.Find("Key_GalleryKey");
        if (key != null)
        {
            Object.DestroyImmediate(key);
        }
    }
}
