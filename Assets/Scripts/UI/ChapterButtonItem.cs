using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChapterButtonItem : MonoBehaviour
{
    [SerializeField] private TMP_Text chapterNameText;
    [SerializeField] private Button button;

    private static readonly Color bloodRed = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color bloodRedHover = new Color(0.80f, 0.12f, 0.12f, 1f);
    private static readonly Color bloodRedPressed = new Color(0.45f, 0.05f, 0.05f, 1f);
    private static readonly Color ghostWhite = new Color(0.85f, 0.82f, 0.78f, 1f);

    private void Awake()
    {
        EnsureReferences();
    }

    public void Setup(string displayName, Action onClick)
    {
        EnsureReferences();

        if (chapterNameText != null)
        {
            EnsureFontAsset(chapterNameText);
            chapterNameText.text = displayName;
            chapterNameText.fontSize = 24;
            chapterNameText.fontStyle = FontStyles.Bold;
            chapterNameText.color = ghostWhite;
            chapterNameText.alignment = TextAlignmentOptions.Center;
            chapterNameText.gameObject.SetActive(true);
        }

        if (button == null) button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("ChapterButtonItem requiere un componente Button.");
            return;
        }

        // Estilo del boton
        Image btnImage = button.GetComponent<Image>();
        if (btnImage != null) btnImage.color = bloodRed;

        ColorBlock colors = button.colors;
        colors.normalColor = bloodRed;
        colors.highlightedColor = bloodRedHover;
        colors.pressedColor = bloodRedPressed;
        colors.selectedColor = bloodRed;
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    private void EnsureReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (chapterNameText == null)
            chapterNameText = GetComponentInChildren<TMP_Text>(true);

        if (chapterNameText == null && button != null)
        {
            GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(transform, false);

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            chapterNameText = textObj.GetComponent<TextMeshProUGUI>();
            chapterNameText.alignment = TextAlignmentOptions.Center;
            chapterNameText.fontSize = 24;
            chapterNameText.raycastTarget = false;
            chapterNameText.color = ghostWhite;
            EnsureFontAsset(chapterNameText);
        }
    }

    private static void EnsureFontAsset(TMP_Text text)
    {
        if (text == null || text.font != null) return;

        TMP_FontAsset font = null;
        if (TMP_Settings.instance != null)
            font = TMP_Settings.defaultFontAsset;
        if (font == null)
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            text.font = font;
    }
}
