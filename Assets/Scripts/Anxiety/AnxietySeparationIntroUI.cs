using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla previa a la cinemática: "{nombre} Se separo del grupo".
/// </summary>
public class AnxietySeparationIntroUI : MonoBehaviour
{
    public static AnxietySeparationIntroUI Instance { get; private set; }

    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private Text messageText;
    [SerializeField] private Button continueButton;

    private Action onContinue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUiBuilt();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void Show(string characterDisplayName, Action continueCallback)
    {
        EnsureInstance();
        Instance.Display(characterDisplayName, continueCallback);
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(AnxietySeparationIntroUI));
        host.AddComponent<AnxietySeparationIntroUI>();
    }

    private void Display(string characterDisplayName, Action continueCallback)
    {
        EnsureUiBuilt();
        onContinue = continueCallback;

        string name = string.IsNullOrWhiteSpace(characterDisplayName) ? "Alguien" : characterDisplayName;
        messageText.text = $"{name} Se separo del grupo";

        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.Hide();
        }

        overlayRoot.SetActive(true);
    }

    public void Hide()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }

        onContinue = null;
    }

    private void HandleContinue()
    {
        Action callback = onContinue;
        Hide();
        callback?.Invoke();
    }

    private void EnsureUiBuilt()
    {
        if (overlayRoot != null)
        {
            return;
        }

        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 320;

        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        overlayRoot = new GameObject("SeparationIntroRoot");
        overlayRoot.transform.SetParent(transform, false);
        RectTransform rootRect = overlayRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject darkObj = new GameObject("Background");
        darkObj.transform.SetParent(overlayRoot.transform, false);
        RectTransform darkRect = darkObj.AddComponent<RectTransform>();
        darkRect.anchorMin = Vector2.zero;
        darkRect.anchorMax = Vector2.one;
        darkRect.offsetMin = Vector2.zero;
        darkRect.offsetMax = Vector2.zero;
        Image darkImage = darkObj.AddComponent<Image>();
        darkImage.color = new Color(0.04f, 0.03f, 0.05f, 0.96f);

        GameObject textObj = new GameObject("Message");
        textObj.transform.SetParent(overlayRoot.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.08f, 0.35f);
        textRect.anchorMax = new Vector2(0.92f, 0.65f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        messageText = textObj.AddComponent<Text>();
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.fontSize = 42;
        messageText.color = new Color(0.93f, 0.86f, 0.72f, 1f);
        messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        messageText.verticalOverflow = VerticalWrapMode.Overflow;

        GameObject buttonObj = new GameObject("BtnContinuar");
        buttonObj.transform.SetParent(overlayRoot.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.42f, 0.12f);
        buttonRect.anchorMax = new Vector2(0.58f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.22f, 0.16f, 0.12f, 0.95f);
        continueButton = buttonObj.AddComponent<Button>();
        continueButton.onClick.AddListener(HandleContinue);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text label = labelObj.AddComponent<Text>();
        label.text = "Continuar";
        label.alignment = TextAnchor.MiddleCenter;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.color = new Color(0.92f, 0.82f, 0.55f, 1f);
        label.raycastTarget = false;
    }
}
