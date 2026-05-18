using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra un sprite a pantalla completa desde el inventario (pistas visuales).
/// </summary>
public class InventoryHintOverlay : MonoBehaviour
{
    public static InventoryHintOverlay Instance { get; private set; }

    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private Image hintImage;

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

    public static void ShowHint(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        EnsureInstance();
        Instance.Display(sprite);
    }

    public static void ShowHintFromResources(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture != null)
            {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        ShowHint(sprite);
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(InventoryHintOverlay));
        host.AddComponent<InventoryHintOverlay>();
    }

    private void Display(Sprite sprite)
    {
        EnsureUiBuilt();

        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.Hide();
        }

        hintImage.sprite = sprite;
        hintImage.preserveAspect = true;
        hintImage.color = Color.white;
        overlayRoot.SetActive(true);
    }

    public void Hide()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }

        if (hintImage != null)
        {
            hintImage.sprite = null;
        }
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
        canvas.sortingOrder = 200;

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

        overlayRoot = new GameObject("HintOverlayRoot");
        overlayRoot.transform.SetParent(transform, false);
        RectTransform rootRect = overlayRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject darkObj = new GameObject("DarkBackground");
        darkObj.transform.SetParent(overlayRoot.transform, false);
        RectTransform darkRect = darkObj.AddComponent<RectTransform>();
        darkRect.anchorMin = Vector2.zero;
        darkRect.anchorMax = Vector2.one;
        darkRect.offsetMin = Vector2.zero;
        darkRect.offsetMax = Vector2.zero;
        Image darkImage = darkObj.AddComponent<Image>();
        darkImage.color = new Color(0f, 0f, 0f, 0.88f);
        darkImage.raycastTarget = true;

        GameObject imageObj = new GameObject("HintImage");
        imageObj.transform.SetParent(overlayRoot.transform, false);
        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.05f, 0.08f);
        imageRect.anchorMax = new Vector2(0.95f, 0.92f);
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        hintImage = imageObj.AddComponent<Image>();
        hintImage.raycastTarget = false;
        hintImage.preserveAspect = true;

        GameObject closeObj = new GameObject("BtnCerrarPista");
        closeObj.transform.SetParent(overlayRoot.transform, false);
        RectTransform closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.92f, 0.92f);
        closeRect.anchorMax = new Vector2(0.99f, 0.99f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;
        Image closeImage = closeObj.AddComponent<Image>();
        closeImage.color = new Color(0.2f, 0.15f, 0.12f, 0.9f);
        Button closeButton = closeObj.AddComponent<Button>();
        closeButton.onClick.AddListener(Hide);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(closeObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text label = labelObj.AddComponent<Text>();
        label.text = "X";
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.92f, 0.82f, 0.55f, 1f);
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 22;
        label.raycastTarget = false;
    }
}
