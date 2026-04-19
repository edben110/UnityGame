using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChapterButtonItem : MonoBehaviour
{
    [SerializeField] private TMP_Text chapterNameText;
    [SerializeField] private Button button;

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

            if (chapterNameText.color.a < 0.1f)
            {
                chapterNameText.color = Color.black;
            }

            chapterNameText.text = displayName;
            chapterNameText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("ChapterButtonItem no encontro un TMP_Text para mostrar el nombre del capitulo.");
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            Debug.LogError("ChapterButtonItem requiere un componente Button.");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    private void EnsureReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (chapterNameText == null)
        {
            chapterNameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (chapterNameText == null && button != null)
        {
            GameObject textObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(transform, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            chapterNameText = textObject.GetComponent<TextMeshProUGUI>();
            chapterNameText.alignment = TextAlignmentOptions.Center;
            chapterNameText.fontSize = 28;
            chapterNameText.raycastTarget = false;
            chapterNameText.color = Color.black;
            EnsureFontAsset(chapterNameText);
        }
    }

    private static void EnsureFontAsset(TMP_Text textComponent)
    {
        if (textComponent == null)
        {
            return;
        }

        if (textComponent.font == null)
        {
            TMP_FontAsset fallbackFont = GetSafeTmpFont();
            if (fallbackFont != null)
            {
                textComponent.font = fallbackFont;
            }
        }
    }

    private static TMP_FontAsset GetSafeTmpFont()
    {
        TMP_FontAsset fallbackFont = null;

        if (TMP_Settings.instance != null)
        {
            fallbackFont = TMP_Settings.defaultFontAsset;
        }

        if (fallbackFont == null)
        {
            fallbackFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        return fallbackFont;
    }
}
