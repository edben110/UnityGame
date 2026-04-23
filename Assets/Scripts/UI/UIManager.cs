using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("Titulo")]
    [SerializeField] private TMP_Text gameTitleText;
    [SerializeField] private string gameTitle = "SIMON'S\nMANSION";
    [SerializeField] private string logoImagePath = "Sprites/logojuego";

    [Header("Pantallas")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chaptersPanel;

    [Header("Botones menu principal")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button chaptersButton;
    [SerializeField] private Button quitButton;

    [Header("Etiquetas por defecto")]
    [SerializeField] private string newGameLabel = "NUEVA PARTIDA";
    [SerializeField] private string continueLabel = "CONTINUAR";
    [SerializeField] private string chaptersLabel = "CAPITULOS";
    [SerializeField] private string quitLabel = "SALIR";

    [Header("Capitulos")]
    [SerializeField] private Transform chapterButtonsContainer;
    [SerializeField] private ChapterButtonItem chapterButtonPrefab;
    [SerializeField] private Button backToMainMenuButton;

    [Header("Fondo")]
    [SerializeField] private string backgroundImagePath = "Sprites/InicioSimonsMansion";

    // Paleta
    private static readonly Color bloodRed = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color ghostWhite = new Color(0.78f, 0.75f, 0.70f, 1f);
    private static readonly Color dimWhite = new Color(0.45f, 0.42f, 0.40f, 1f);
    private static readonly Color transparent = new Color(0, 0, 0, 0);

    private Canvas rootCanvas;
    private GameObject backgroundObj;

    private void Start()
    {
        HideOldSceneUI();
        EnsureCanvas();
        EnsureBackground();

        // Forzar creacion propia ignorando referencias de escena
        mainMenuPanel = null;
        chaptersPanel = null;
        gameTitleText = null;
        newGameButton = null;
        continueButton = null;
        chaptersButton = null;
        quitButton = null;
        chapterButtonsContainer = null;
        backToMainMenuButton = null;

        EnsureMainMenuPanel();
        EnsureChaptersPanel();
        WireEvents();
        ShowMainMenu();
        Invoke(nameof(RefreshContinueState), 0f);
    }

    private void OnDestroy()
    {
        UnwireEvents();
    }

    // ==================== PANELES ====================

    private void HideOldSceneUI()
    {
        // Ocultar paneles/botones viejos creados en la escena
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (chaptersPanel != null) chaptersPanel.SetActive(false);
        if (gameTitleText != null) gameTitleText.gameObject.SetActive(false);
    }

    public void ShowMainMenu()
    {
        if (debugLogs) Debug.Log("UIManager: Mostrar MainMenuPanel");
        SetPanelState(mainMenuPanel, true);
        SetPanelState(chaptersPanel, false);
        if (backgroundObj != null) backgroundObj.SetActive(true);
        RefreshContinueState();
    }

    public void ShowChapters()
    {
        if (debugLogs) Debug.Log("UIManager: Mostrando ChaptersPanel.");
        SetPanelState(mainMenuPanel, false);
        SetPanelState(chaptersPanel, true);
        if (backgroundObj != null) backgroundObj.SetActive(true);
        BuildChapterButtons();
    }

    // ==================== EVENTOS ====================

    private void WireEvents()
    {
        WireButton(newGameButton, OnClickNewGame);
        WireButton(continueButton, OnClickContinue);
        WireButton(chaptersButton, ShowChapters);
        WireButton(quitButton, OnClickQuit);
        WireButton(backToMainMenuButton, ShowMainMenu);
    }

    private void UnwireEvents()
    {
        UnwireButton(newGameButton, OnClickNewGame);
        UnwireButton(continueButton, OnClickContinue);
        UnwireButton(chaptersButton, ShowChapters);
        UnwireButton(quitButton, OnClickQuit);
        UnwireButton(backToMainMenuButton, ShowMainMenu);
    }

    private static void WireButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    private static void UnwireButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveListener(action);
    }

    private void OnClickNewGame()
    {
        if (GameManager.Instance == null) { Debug.LogError("GameManager no encontrado."); return; }
        GameManager.Instance.StartNewGame();
    }

    private void OnClickContinue()
    {
        if (GameManager.Instance == null) { Debug.LogError("GameManager no encontrado."); return; }
        GameManager.Instance.ContinueGame();
    }

    private void OnClickQuit()
    {
        if (GameManager.Instance == null) { Debug.LogError("GameManager no encontrado."); return; }
        GameManager.Instance.QuitGame();
    }

    private void RefreshContinueState()
    {
        if (continueButton == null) return;
        bool canContinue = GameManager.Instance != null && GameManager.Instance.CanContinue();
        continueButton.interactable = canContinue;

        TMP_Text label = continueButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.color = canContinue ? ghostWhite : dimWhite;
    }

    // ==================== CAPITULOS ====================

    private void BuildChapterButtons()
    {
        if (chapterButtonsContainer == null) { Debug.LogError("No se asigno chapterButtonsContainer."); return; }

        ClearContainer(chapterButtonsContainer);

        if (GameManager.Instance == null) { Debug.LogError("GameManager no encontrado."); return; }

        List<ChapterDefinition> unlocked = GameManager.Instance.GetUnlockedChapters();
        foreach (ChapterDefinition chapter in unlocked)
        {
            string chapterId = chapter.id;
            string label = string.IsNullOrWhiteSpace(chapter.displayName) ? chapter.id : chapter.displayName;

            if (chapterButtonPrefab != null)
            {
                ChapterButtonItem item = Instantiate(chapterButtonPrefab, chapterButtonsContainer);
                item.gameObject.SetActive(true);
                item.Setup(label, () => GameManager.Instance.LoadChapterById(chapterId));
            }
            else
            {
                Button btn = CreateTextButton(chapterButtonsContainer, label.ToUpper(), 28, ghostWhite);
                string capturedId = chapterId;
                btn.onClick.AddListener(() => GameManager.Instance.LoadChapterById(capturedId));
            }
        }

        if (debugLogs) Debug.Log($"UIManager: {unlocked.Count} botones de capitulos generados.");
    }

    private static void ClearContainer(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    private static void SetPanelState(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    // ==================== AUTO-CONSTRUCCION ====================

    private void EnsureCanvas()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();

        if (rootCanvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            rootCanvas = canvasObj.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            transform.SetParent(canvasObj.transform, false);
        }
        else
        {
            // Asegurar scaler correcto
            CanvasScaler scaler = rootCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }

    private void EnsureBackground()
    {
        backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(rootCanvas.transform, false);
        backgroundObj.transform.SetAsFirstSibling();

        RawImage bg = backgroundObj.AddComponent<RawImage>();
        bg.raycastTarget = false;

        RectTransform rect = backgroundObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Texture2D tex = Resources.Load<Texture2D>(backgroundImagePath);
        if (tex != null)
        {
            bg.texture = tex;
            bg.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"No se encontro imagen de fondo en Resources/{backgroundImagePath}. Usando color oscuro.");
            bg.color = new Color(0.05f, 0.05f, 0.07f, 1f);
        }
    }

    private void EnsureMainMenuPanel()
    {
        if (mainMenuPanel == null)
            mainMenuPanel = CreateTransparentPanel("MainMenuPanel", rootCanvas.transform);

        // Hacer el panel transparente (la imagen de fondo se ve detras)
        Image panelImg = mainMenuPanel.GetComponent<Image>();
        if (panelImg != null) panelImg.color = transparent;

        // Logo arriba a la izquierda
        CreateLogo(mainMenuPanel.transform);

        // Contenedor de botones a la izquierda
        GameObject buttonsArea = new GameObject("ButtonsArea", typeof(RectTransform));
        buttonsArea.transform.SetParent(mainMenuPanel.transform, false);

        RectTransform baRect = buttonsArea.GetComponent<RectTransform>();
        baRect.anchorMin = new Vector2(0.05f, 0.15f);
        baRect.anchorMax = new Vector2(0.35f, 0.55f);
        baRect.offsetMin = Vector2.zero;
        baRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = buttonsArea.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // Crear botones como texto simple (sin fondo)
        if (newGameButton == null)
            newGameButton = CreateTextButton(buttonsArea.transform, newGameLabel, 30, bloodRed);
        else
            StyleTextButton(newGameButton, newGameLabel, 30, bloodRed);

        if (continueButton == null)
            continueButton = CreateTextButton(buttonsArea.transform, continueLabel, 26, ghostWhite);
        else
            StyleTextButton(continueButton, continueLabel, 26, ghostWhite);

        if (chaptersButton == null)
            chaptersButton = CreateTextButton(buttonsArea.transform, chaptersLabel, 26, ghostWhite);
        else
            StyleTextButton(chaptersButton, chaptersLabel, 26, ghostWhite);

        if (quitButton == null)
            quitButton = CreateTextButton(buttonsArea.transform, quitLabel, 26, ghostWhite);
        else
            StyleTextButton(quitButton, quitLabel, 26, ghostWhite);

        // Texto de ubicacion abajo a la izquierda
        CreateLocationText(mainMenuPanel.transform);
    }

    private void EnsureChaptersPanel()
    {
        if (chaptersPanel == null)
            chaptersPanel = CreateTransparentPanel("ChaptersPanel", rootCanvas.transform);

        Image panelImg = chaptersPanel.GetComponent<Image>();
        if (panelImg != null) panelImg.color = new Color(0, 0, 0, 0.7f);

        // Titulo
        Transform existingTitle = chaptersPanel.transform.Find("ChaptersTitle");
        if (existingTitle == null)
            CreateSectionTitle(chaptersPanel.transform, "CAPITULOS");

        // Contenedor
        if (chapterButtonsContainer == null)
        {
            GameObject container = new GameObject("ChaptersButtonContainer", typeof(RectTransform));
            container.transform.SetParent(chaptersPanel.transform, false);

            RectTransform cRect = container.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.05f, 0.20f);
            cRect.anchorMax = new Vector2(0.45f, 0.75f);
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            chapterButtonsContainer = container.transform;
        }

        // Boton volver
        if (backToMainMenuButton == null)
            backToMainMenuButton = CreateTextButton(chaptersPanel.transform, "VOLVER", 26, ghostWhite);
        else
            StyleTextButton(backToMainMenuButton, "VOLVER", 26, ghostWhite);

        RectTransform backRect = backToMainMenuButton.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.05f, 0.05f);
        backRect.anchorMax = new Vector2(0.25f, 0.12f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;

        LayoutElement le = backToMainMenuButton.GetComponent<LayoutElement>();
        if (le == null) le = backToMainMenuButton.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    // ==================== CREACION DE ELEMENTOS ====================

    private static GameObject CreateTransparentPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = panel.GetComponent<Image>();
        img.color = transparent;
        img.raycastTarget = false;

        return panel;
    }

    private void CreateLogo(Transform parent)
    {
        GameObject logoObj = new GameObject("Logo", typeof(RectTransform), typeof(RawImage));
        logoObj.transform.SetParent(parent, false);

        RawImage img = logoObj.GetComponent<RawImage>();
        img.raycastTarget = false;

        Texture2D tex = Resources.Load<Texture2D>(logoImagePath);
        if (tex != null)
        {
            img.texture = tex;
            img.color = Color.white;

            // Mantener proporcion original
            AspectRatioFitter fitter = logoObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            fitter.aspectRatio = (float)tex.width / tex.height;
        }
        else
        {
            Debug.LogWarning($"No se encontro logo en Resources/{logoImagePath}");
            img.color = transparent;
        }

        RectTransform rect = logoObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.55f);
        rect.anchorMax = new Vector2(0.35f, 0.95f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CreateSectionTitle(Transform parent, string text)
    {
        GameObject obj = new GameObject("ChaptersTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.05f, 0.82f);
        rect.anchorMax = new Vector2(0.5f, 0.95f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 52;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = ghostWhite;
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        tmp.raycastTarget = false;
        EnsureFontAsset(tmp);
    }

    private static void CreateLocationText(Transform parent)
    {
        GameObject obj = new GameObject("LocationText", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.03f, 0.02f);
        rect.anchorMax = new Vector2(0.3f, 0.10f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = "AFUERAS DE BERNA, SUIZA\n1952";
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Normal;
        tmp.color = new Color(0.55f, 0.52f, 0.48f, 0.8f);
        tmp.alignment = TextAlignmentOptions.BottomLeft;
        tmp.raycastTarget = false;
        EnsureFontAsset(tmp);
    }

    private static Button CreateTextButton(Transform parent, string label, int fontSize, Color color)
    {
        GameObject btnObj = new GameObject(label + "Btn", typeof(RectTransform), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = transparent;

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(1, 1, 1, 0.4f);
        btn.colors = colors;

        // Texto
        GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.raycastTarget = false;
        EnsureFontAsset(tmp);

        // Hover effect
        ButtonHoverEffect hover = btnObj.AddComponent<ButtonHoverEffect>();
        Color hoverColor = color == bloodRed ? new Color(0.9f, 0.15f, 0.15f, 1f) : Color.white;
        hover.Init(color, hoverColor, fontSize);

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize + 18;
        le.flexibleWidth = 1;

        return btn;
    }

    private static void StyleTextButton(Button btn, string label, int fontSize, Color color)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = transparent;

        TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            EnsureFontAsset(tmp);
        }
    }

    // ==================== UTILIDADES ====================

    private static TMP_FontAsset GetSafeTmpFont()
    {
        TMP_FontAsset font = null;
        if (TMP_Settings.instance != null)
            font = TMP_Settings.defaultFontAsset;
        if (font == null)
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        return font;
    }

    private static void EnsureFontAsset(TMP_Text text)
    {
        if (text == null || text.font != null) return;
        TMP_FontAsset font = GetSafeTmpFont();
        if (font != null) text.font = font;
        else Debug.LogWarning("No hay fuente TMP. Importa TMP Essentials desde Window > TextMeshPro.");
    }
}
