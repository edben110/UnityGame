using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script de Editor para crear el prefab InventorySlot con todas las referencias correctas.
/// Ejecutar desde: Tools > Crear InventorySlot Prefab
/// Se puede eliminar este script después de crear el prefab.
/// </summary>
public static class CreateInventorySlotPrefab
{
    [MenuItem("Tools/Crear InventorySlot Prefab")]
    public static void Create()
    {
        // === GameObject raíz: InventorySlot ===
        GameObject root = new GameObject("InventorySlot");
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(80f, 80f);

        // Fondo del slot: color oscuro RPG con ligera transparencia
        root.AddComponent<CanvasRenderer>();
        Image slotBackground = root.AddComponent<Image>();
        slotBackground.color = new Color(0.12f, 0.10f, 0.14f, 0.85f);
        slotBackground.raycastTarget = true;
        slotBackground.sprite = null; // Sin sprite, color sólido estilizado
        slotBackground.type = Image.Type.Simple;

        // === Hijo: ItemIcon ===
        GameObject iconObj = new GameObject("ItemIcon");
        iconObj.transform.SetParent(root.transform, false);

        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        // Centrado con padding interno (10% por cada lado)
        iconRect.anchorMin = new Vector2(0.12f, 0.12f);
        iconRect.anchorMax = new Vector2(0.88f, 0.88f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        iconRect.pivot = new Vector2(0.5f, 0.5f);

        iconObj.AddComponent<CanvasRenderer>();
        Image itemIcon = iconObj.AddComponent<Image>();
        itemIcon.sprite = null;
        itemIcon.color = new Color(1f, 1f, 1f, 0f); // Transparente inicialmente
        itemIcon.raycastTarget = false;
        itemIcon.preserveAspect = true;
        itemIcon.enabled = false; // Desactivado hasta que se asigne un ítem

        // === Hijo: SelectionHighlight ===
        GameObject highlightObj = new GameObject("SelectionHighlight");
        highlightObj.transform.SetParent(root.transform, false);

        RectTransform highlightRect = highlightObj.AddComponent<RectTransform>();
        // Cubre todo el slot (borde iluminado overlay)
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;
        highlightRect.pivot = new Vector2(0.5f, 0.5f);

        highlightObj.AddComponent<CanvasRenderer>();
        Image highlightImage = highlightObj.AddComponent<Image>();
        // Fondo completamente transparente: solo el Outline dibuja el borde
        highlightImage.color = new Color(0f, 0f, 0f, 0f);
        highlightImage.sprite = null;
        highlightImage.type = Image.Type.Simple;
        highlightImage.raycastTarget = false;

        // Outline dorado cálido como borde de selección (~3px)
        Outline outline = highlightObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.92f, 0.78f, 0.40f, 0.9f);
        outline.effectDistance = new Vector2(3f, 3f);
        outline.useGraphicAlpha = false; // Dibuja independiente del alpha del Image

        // Estado inicial: highlight DESACTIVADO
        highlightObj.SetActive(false);

        // === Script InventorySlotUI ===
        InventorySlotUI slotUI = root.AddComponent<InventorySlotUI>();

        // Asignar referencias via SerializedObject para que persistan en el prefab
        SerializedObject serializedSlot = new SerializedObject(slotUI);
        serializedSlot.FindProperty("itemIcon").objectReferenceValue = itemIcon;
        serializedSlot.FindProperty("selectionHighlight").objectReferenceValue = highlightObj;
        serializedSlot.FindProperty("slotIndex").intValue = 0;
        serializedSlot.ApplyModifiedPropertiesWithoutUndo();

        // === Guardar como Prefab ===
        string prefabPath = "Assets/Prefabs/InventorySlot.prefab";

        // Asegurar que la carpeta existe
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Crear el prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

        // Limpiar el objeto temporal de la escena
        Object.DestroyImmediate(root);

        if (prefab != null)
        {
            Debug.Log($"<color=green>✓ Prefab creado exitosamente en: {prefabPath}</color>");
            Debug.Log("Jerarquía del prefab:");
            Debug.Log("  InventorySlot (RectTransform, Image[fondo oscuro], InventorySlotUI)");
            Debug.Log("    ├─ ItemIcon (Image, preserveAspect=true, transparente, desactivado)");
            Debug.Log("    └─ SelectionHighlight (Image[dorado], Outline, DESACTIVADO)");

            // Seleccionar el prefab en el Project
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }
        else
        {
            Debug.LogError("Error al crear el prefab InventorySlot.");
        }
    }
}
