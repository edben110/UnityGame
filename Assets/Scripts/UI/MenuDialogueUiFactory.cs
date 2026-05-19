using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crea un sistema de diálogo mínimo para MenuScene (nueva partida).
/// </summary>
public static class MenuDialogueUiFactory
{
    public struct MenuDialogueSystem
    {
        public DialogueLibrary Library;
        public DialogueRunner Runner;
        public DialoguePanelUI Panel;
        public Canvas Canvas;
    }

    public static MenuDialogueSystem Create(Transform overlayRoot)
    {
        GameObject panelRoot = new GameObject("DialoguePanelRoot");
        panelRoot.transform.SetParent(overlayRoot, false);
        RectTransform panelRootRect = panelRoot.AddComponent<RectTransform>();
        panelRootRect.anchorMin = new Vector2(0.08f, 0.06f);
        panelRootRect.anchorMax = new Vector2(0.92f, 0.38f);
        panelRootRect.offsetMin = Vector2.zero;
        panelRootRect.offsetMax = Vector2.zero;

        GameObject dialoguePanel = new GameObject("DialoguePanel");
        dialoguePanel.transform.SetParent(panelRoot.transform, false);
        RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = dialoguePanel.AddComponent<Image>();
        Sprite frameSprite = Resources.Load<Sprite>("Sprites/marcoDialogoas");
        if (frameSprite != null)
        {
            panelBg.sprite = frameSprite;
            panelBg.type = Image.Type.Sliced;
            panelBg.color = Color.white;
        }
        else
        {
            panelBg.color = new Color(0.08f, 0.07f, 0.1f, 0.92f);
        }

        panelBg.raycastTarget = true;

        TMP_Text speakerText = CreateText(dialoguePanel.transform, "SpeakerText", 28,
            new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.95f), FontStyles.Bold);
        TMP_Text bodyText = CreateText(dialoguePanel.transform, "BodyText", 24,
            new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.7f), FontStyles.Normal);
        bodyText.alignment = TextAlignmentOptions.TopLeft;

        GameObject choicesContainer = new GameObject("ChoicesContainer");
        choicesContainer.transform.SetParent(dialoguePanel.transform, false);
        RectTransform choicesRect = choicesContainer.AddComponent<RectTransform>();
        choicesRect.anchorMin = new Vector2(0.08f, 0.02f);
        choicesRect.anchorMax = new Vector2(0.92f, 0.2f);
        choicesRect.offsetMin = Vector2.zero;
        choicesRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = choicesContainer.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = 8f;
        choicesContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject dialogueSystemObj = new GameObject("MenuDialogueSystem");
        dialogueSystemObj.transform.SetParent(overlayRoot, false);

        Button choicePrefab = CreateChoiceButtonPrefab(choicesContainer.transform);

        DialogueLibrary library = dialogueSystemObj.AddComponent<DialogueLibrary>();
        DialoguePanelUI panelUI = dialogueSystemObj.AddComponent<DialoguePanelUI>();

        SetPrivateField(panelUI, "root", dialoguePanel);
        SetPrivateField(panelUI, "speakerText", speakerText);
        SetPrivateField(panelUI, "bodyText", bodyText);
        SetPrivateField(panelUI, "choicesContainer", choicesContainer.transform);
        SetPrivateField(panelUI, "choiceButtonPrefab", choicePrefab);

        DialogueRunner runner = dialogueSystemObj.AddComponent<DialogueRunner>();
        panelUI.WireUi();
        runner.Bind(library, panelUI);

        dialoguePanel.SetActive(true);

        return new MenuDialogueSystem
        {
            Library = library,
            Runner = runner,
            Panel = panelUI,
            Canvas = overlayRoot.GetComponentInParent<Canvas>()
        };
    }

    public static Button CreateOverlayContinueButton(Transform parent)
    {
        return CreateContinueButton(parent);
    }

    private static TMP_Text CreateText(Transform parent, string name, float fontSize, Vector2 anchorMin, Vector2 anchorMax, FontStyles style)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = new Color(0.93f, 0.88f, 0.75f, 1f);
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");
        if (font != null)
        {
            text.font = font;
        }

        return text;
    }

    private static Button CreateContinueButton(Transform parent)
    {
        GameObject obj = new GameObject("ContinueButton");
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.72f, 0.04f);
        rect.anchorMax = new Vector2(0.96f, 0.12f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = obj.AddComponent<Image>();
        Sprite frameSprite = Resources.Load<Sprite>("Sprites/marcoDialogoas");
        if (frameSprite != null)
        {
            image.sprite = frameSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }
        else
        {
            image.color = new Color(0.2f, 0.15f, 0.12f, 0.95f);
        }

        Button button = obj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
        colors.pressedColor = new Color(0.85f, 0.78f, 0.65f, 1f);
        button.colors = colors;

        TMP_Text label = CreateText(obj.transform, "Label", 26, Vector2.zero, Vector2.one, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        label.text = "Continuar";
        label.color = new Color(0.93f, 0.88f, 0.75f, 1f);

        return button;
    }

    private static Button CreateChoiceButtonPrefab(Transform parent)
    {
        GameObject obj = new GameObject("ChoiceButtonPrefab");
        obj.transform.SetParent(parent, false);
        obj.SetActive(false);
        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.18f, 0.14f, 0.11f, 0.95f);
        Button button = obj.AddComponent<Button>();

        TMP_Text label = CreateText(obj.transform, "Label", 20, Vector2.zero, Vector2.one, FontStyles.Normal);
        label.alignment = TextAlignmentOptions.Center;
        label.text = "Opción";

        return button;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            return;
        }

        System.Reflection.FieldInfo field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}
