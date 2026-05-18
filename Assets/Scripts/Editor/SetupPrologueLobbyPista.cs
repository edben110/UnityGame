using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Crea HS_Lobby_Pista en el prólogo y elimina Key_GalleryKey del mapa.
/// </summary>
public static class SetupPrologueLobbyPista
{
    private const string MainMapScenePath = "Assets/Scenes/MainMapScene.unity";
    private const string RetratoItemId = "retrato";

    [MenuItem("Tools/Puzzle/Configurar prólogo (HS_Lobby_Pista + quitar GalleryKey)")]
    public static void Configure()
    {
        var scene = EditorSceneManager.OpenScene(MainMapScenePath, OpenSceneMode.Single);

        RemoveGalleryKeyFromScene();
        SetupLobbyPistaHotspot();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog(
            "Prólogo y galería",
            "Listo:\n" +
            "- Key_GalleryKey eliminada del escenario\n" +
            "- HS_Lobby_Pista creado (capítulo prólogo → item retrato)\n\n" +
            "Clic en retrato en inventario → muestra la pista visual.",
            "OK");
    }

    private static void RemoveGalleryKeyFromScene()
    {
        GameObject key = GameObject.Find("Key_GalleryKey");
        if (key == null)
        {
            return;
        }

        Object.DestroyImmediate(key);
    }

    private static void SetupLobbyPistaHotspot()
    {
        Transform hotspotsRoot = FindHotspotsRoot();
        if (hotspotsRoot == null)
        {
            Debug.LogError("No se encontró el contenedor Hotspots.");
            return;
        }

        Transform existing = hotspotsRoot.Find("HS_Lobby_Pista");
        GameObject hotspotObject = existing != null ? existing.gameObject : new GameObject("HS_Lobby_Pista");
        if (existing == null)
        {
            hotspotObject.transform.SetParent(hotspotsRoot, false);
        }

        RectTransform rect = hotspotObject.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = hotspotObject.AddComponent<RectTransform>();
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(1.2f, 2.1f);
        rect.sizeDelta = new Vector2(0.85f, 0.85f);

        if (hotspotObject.GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D box = hotspotObject.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
        }

        MapHotspot hotspot = hotspotObject.GetComponent<MapHotspot>();
        if (hotspot == null)
        {
            hotspot = hotspotObject.AddComponent<MapHotspot>();
        }

        Sprite icon = LoadSprite("Assets/Resources/Sprites/Puzzle/acertijo_pista.png");
        if (icon == null)
        {
            icon = LoadSprite("Assets/Sprites/Puzzle/acertijo_pista.png");
        }

        SerializedObject so = new SerializedObject(hotspot);
        so.FindProperty("hotspotId").stringValue = "lobby_pista";
        so.FindProperty("requiredChapterId").stringValue = "prologue";
        so.FindProperty("conversationId").stringValue = string.Empty;
        so.FindProperty("consumeAfterUse").boolValue = false;
        so.FindProperty("setFlagOnInteract").stringValue = "clue.lobby.pista";
        so.FindProperty("grantItemId").stringValue = RetratoItemId;
        so.FindProperty("grantItemDisplayName").stringValue = "Retrato";
        so.FindProperty("grantItemDescription").stringValue =
            "quiza sea una pista para algo, pero que...";
        so.FindProperty("grantItemSprite").objectReferenceValue = icon;
        so.FindProperty("hideAfterPickup").boolValue = true;
        so.FindProperty("showDebugMarkerInGame").boolValue = true;
        so.FindProperty("debugMarkerSize").vector2Value = new Vector2(6f, 6f);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Transform FindHotspotsRoot()
    {
        GameObject hotspots = GameObject.Find("Hotspots");
        if (hotspots != null)
        {
            return hotspots.transform;
        }

        GameObject hotsPots = GameObject.Find("HotsPots");
        return hotsPots != null ? hotsPots.transform : null;
    }

    private static Sprite LoadSprite(string assetPath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
        {
            return sprite;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        if (assets == null)
        {
            return null;
        }

        foreach (Object asset in assets)
        {
            if (asset is Sprite sheetSprite)
            {
                return sheetSprite;
            }
        }

        return null;
    }
}
