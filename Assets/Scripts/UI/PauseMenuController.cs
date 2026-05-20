using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menú de pausa global (ESC): pantalla completa con volver al menú principal o salir del juego.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private int canvasSortingOrder = 250;
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.82f);

    private GameObject overlayRoot;
    private Button returnToMenuButton;
    private bool isOpen;
    private float previousTimeScale = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureExists();
    }

    public static void EnsureExists()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject("PauseMenuController");
        host.AddComponent<PauseMenuController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUi();
        SetOpen(false);
    }

    private void OnDestroy()
    {
        RestoreTimeScale();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!WasEscapePressedThisFrame())
        {
            return;
        }

        if (!CanTogglePause())
        {
            return;
        }

        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private static bool WasEscapePressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }

        return Input.GetKeyDown(KeyCode.Escape);
    }

    private bool CanTogglePause()
    {
        if (EndingCinematicPlayer.IsPlaying)
        {
            return false;
        }

        if (EndingFlowController.IsResolvingEnding)
        {
            return false;
        }

        return true;
    }

    public void Open()
    {
        if (overlayRoot == null)
        {
            BuildUi();
        }

        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.Hide();
        }

        SetOpen(true);
        GameInputBlocker.Block();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void Close()
    {
        SetOpen(false);
        GameInputBlocker.Unblock();
        RestoreTimeScale();
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(open);
        }

        UpdateReturnButtonVisibility();
    }

    private void UpdateReturnButtonVisibility()
    {
        if (returnToMenuButton == null)
        {
            return;
        }

        bool alreadyInMenu = SceneManager.GetActiveScene().name == menuSceneName;
        returnToMenuButton.gameObject.SetActive(!alreadyInMenu);
    }

    private void OnReturnToMenuClicked()
    {
        RestoreTimeScale();
        GameInputBlocker.Unblock();
        SetOpen(false);

        if (GameManager.Instance != null)
        {
            new SceneController().TryLoadScene(menuSceneName);
            return;
        }

        if (!new SceneController().TryLoadScene(menuSceneName))
        {
            Debug.LogError("[PauseMenu] No se pudo cargar MenuScene.");
        }
    }

    private void OnQuitClicked()
    {
        RestoreTimeScale();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BuildUi()
    {
        if (overlayRoot != null)
        {
            Destroy(overlayRoot);
        }

        GameObject canvasObj = new GameObject("PauseMenuCanvas");
        canvasObj.transform.SetParent(transform, false);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortingOrder;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        overlayRoot = new GameObject("PauseOverlayRoot");
        overlayRoot.transform.SetParent(canvasObj.transform, false);
        RectTransform rootRect = overlayRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject backdrop = new GameObject("Backdrop");
        backdrop.transform.SetParent(overlayRoot.transform, false);
        RectTransform backdropRect = backdrop.AddComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;
        Image backdropImage = backdrop.AddComponent<Image>();
        backdropImage.color = overlayColor;
        backdropImage.raycastTarget = true;

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(overlayRoot.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.32f, 0.28f);
        panelRect.anchorMax = new Vector2(0.68f, 0.72f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = panel.AddComponent<Image>();
        Sprite frameSprite = Resources.Load<Sprite>("Sprites/marcoDialogoas");
        if (frameSprite != null)
        {
            panelBg.sprite = frameSprite;
            panelBg.type = Image.Type.Sliced;
            panelBg.color = Color.white;
        }
        else
        {
            panelBg.color = new Color(0.12f, 0.1f, 0.14f, 0.96f);
        }

        TMP_Text title = CreateText(panel.transform, "Title", "Pausa", 42f, FontStyles.Bold,
            new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.94f));

        returnToMenuButton = CreateMenuButton(panel.transform, "ReturnToMenuButton",
            "Volver a Pantalla de Inicio", new Vector2(0.12f, 0.44f), new Vector2(0.88f, 0.58f),
            OnReturnToMenuClicked);

        Button quitButton = CreateMenuButton(panel.transform, "QuitButton",
            "Salir del juego", new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.36f),
            OnQuitClicked);

        ButtonHoverEffect hoverReturn = returnToMenuButton.gameObject.AddComponent<ButtonHoverEffect>();
        hoverReturn.Init(new Color(0.93f, 0.88f, 0.75f, 1f), new Color(1f, 0.92f, 0.55f, 1f), 28f);

        ButtonHoverEffect hoverQuit = quitButton.gameObject.AddComponent<ButtonHoverEffect>();
        hoverQuit.Init(new Color(0.93f, 0.88f, 0.75f, 1f), new Color(1f, 0.92f, 0.55f, 1f), 28f);

        title.alignment = TextAlignmentOptions.Center;
    }

    private static TMP_Text CreateText(Transform parent, string name, string content, float fontSize,
        FontStyles style, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
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

    private static Button CreateMenuButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
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
        button.onClick.AddListener(onClick);

        TMP_Text labelText = CreateText(obj.transform, "Label", label, 28f, FontStyles.Bold,
            Vector2.zero, Vector2.one);
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.raycastTarget = false;

        return button;
    }
}
