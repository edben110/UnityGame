using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FixDialogueUI
{
    [MenuItem("Tools/Arreglar UI de Diálogos")]
    public static void Fix()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        DialoguePanelUI panelUI = Object.FindFirstObjectByType<DialoguePanelUI>(FindObjectsInactive.Include);
        if (panelUI == null)
        {
            Debug.LogError("No se encontró DialoguePanelUI en la escena.");
            return;
        }

        SerializedObject so = new SerializedObject(panelUI);

        // --- Arreglar botón Continue ---
        SerializedProperty continueProp = so.FindProperty("continueButton");
        if (continueProp != null && continueProp.objectReferenceValue != null)
        {
            Button continueBtn = continueProp.objectReferenceValue as Button;
            if (continueBtn != null)
            {
                RectTransform rt = continueBtn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // Anclar abajo-derecha del panel con tamaño fijo
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    rt.anchoredPosition = new Vector2(-15f, 10f);
                    rt.sizeDelta = new Vector2(200f, 40f);
                    Debug.Log("Botón Continue reposicionado: abajo-derecha del panel, 200x40.");
                }

                // Asegurar que el Image del botón tenga color visible
                Image btnImage = continueBtn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
                }

                // Asegurar texto visible
                TMP_Text label = continueBtn.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = "Continuar ▸";
                    label.fontSize = 20;
                    label.color = Color.white;
                    label.alignment = TextAlignmentOptions.Center;
                    Debug.Log("Texto del botón Continue configurado.");
                }
            }
        }

        // --- Arreglar OptionsContainer ---
        SerializedProperty choicesProp = so.FindProperty("choicesContainer");
        if (choicesProp != null && choicesProp.objectReferenceValue != null)
        {
            Transform container = choicesProp.objectReferenceValue as Transform;
            if (container != null)
            {
                RectTransform rt = container.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.05f, 0.05f);
                    rt.anchorMax = new Vector2(0.95f, 0.45f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.sizeDelta = Vector2.zero;
                    Debug.Log("OptionsContainer reposicionado.");
                }

                // VerticalLayoutGroup
                var layout = container.GetComponent<VerticalLayoutGroup>();
                if (layout == null)
                {
                    layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
                }
                layout.spacing = 10f;
                layout.padding = new RectOffset(10, 10, 5, 5);
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                // ContentSizeFitter para que crezca con los botones
                var fitter = container.GetComponent<ContentSizeFitter>();
                if (fitter == null)
                {
                    fitter = container.gameObject.AddComponent<ContentSizeFitter>();
                }
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                Debug.Log("OptionsContainer layout configurado.");
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("UI Arreglada",
            "Botón 'Continuar' reposicionado y visible.\n" +
            "Contenedor de opciones configurado.\n\n" +
            "Dale Play para probar.",
            "OK");
    }
}
