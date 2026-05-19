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

        // Ocultar TODOS los hijos del canvas que no sean UIManager o Camera
        if (rootCanvas == null) rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = FindObjectOfType<Canvas>();
        if (rootCanvas != null)
        {
            for (int i = 0; i < rootCanvas.transform.childCount; i++)
            {
                Transform child = rootCanvas.transform.GetChild(i);
                if (child.GetComponent<UIManager>() == null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
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

    public void HideAllMenuUi()
    {
        SetPanelState(mainMenuPanel, false);
        SetPanelState(chaptersPanel, false);

        if (backgroundObj != null)
        {
            backgroundObj.SetActive(false);
        }
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

        // Cargar marco
        Sprite marcoSprite = Resources.Load<Sprite>("Sprites/marcoCapitulos");
        if (marcoSprite == null)
        {
            // Intentar cargar como Texture2D y crear sprite
            Texture2D marcoTex = Resources.Load<Texture2D>("Sprites/marcoCapitulos");
            if (marcoTex != null)
                marcoSprite = Sprite.Create(marcoTex, new Rect(0, 0, marcoTex.width, marcoTex.height), new Vector2(0.5f, 0.5f));
        }

        // Mostrar los 5 capitulos (desbloqueados y bloqueados)
        IReadOnlyList<ChapterDefinition> allChapters = GameManager.Instance.Chapters;
        List<ChapterDefinition> unlocked = GameManager.Instance.GetUnlockedChapters();

        foreach (ChapterDefinition chapter in allChapters)
        {
            if (chapter == null || !chapter.IsValid()) continue;

            bool isUnlocked = false;
            foreach (ChapterDefinition u in unlocked)
            {
                if (u.id == chapter.id) { isUnlocked = true; break; }
            }

            string label = string.IsNullOrWhiteSpace(chapter.displayName) ? chapter.id : chapter.displayName;
            CreateChapterCard(chapterButtonsContainer, label, chapter.id, marcoSprite, isUnlocked);
        }

        if (debugLogs) Debug.Log($"UIManager: {allChapters.Count} tarjetas de capitulos generadas.");
    }

    private void CreateChapterCard(Transform parent, string label, string chapterId, Sprite marcoSprite, bool unlocked)
    {
        GameObject cardObj = new GameObject(label + "Card", typeof(RectTransform), typeof(Button));
        cardObj.transform.SetParent(parent, false);

        // Marco como fondo usando RawImage
        RawImage cardImage = cardObj.AddComponent<RawImage>();
        Texture2D marcoTex = Resources.Load<Texture2D>("Sprites/marcoCapitulos");
        if (marcoTex != null)
        {
            cardImage.texture = marcoTex;
            cardImage.color = unlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.85f);
            Debug.Log($"Marco cargado OK para {label}");
        }
        else
        {
            Debug.LogError("NO se pudo cargar Sprites/marcoCapitulos");
            cardImage.color = unlocked ? new Color(0.25f, 0.08f, 0.08f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
        }

        // Tamano flexible
        LayoutElement le = cardObj.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;
        le.flexibleHeight = 1;

        // Texto del capitulo
        GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(cardObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.55f);
        textRect.anchorMax = new Vector2(0.9f, 0.85f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label.ToUpper();
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = unlocked ? ghostWhite : dimWhite;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        EnsureFontAsset(tmp);

        // Candado si esta bloqueado
        if (!unlocked)
        {
            GameObject lockObj = new GameObject("LockText", typeof(RectTransform), typeof(TextMeshProUGUI));
            lockObj.transform.SetParent(cardObj.transform, false);

            RectTransform lockRect = lockObj.GetComponent<RectTransform>();
            lockRect.anchorMin = new Vector2(0.1f, 0.15f);
            lockRect.anchorMax = new Vector2(0.9f, 0.45f);
            lockRect.offsetMin = Vector2.zero;
            lockRect.offsetMax = Vector2.zero;

            TextMeshProUGUI lockTmp = lockObj.GetComponent<TextMeshProUGUI>();
            lockTmp.text = "BLOQUEADO";
            lockTmp.fontSize = 22;
            lockTmp.fontStyle = FontStyles.Italic;
            lockTmp.alignment = TextAlignmentOptions.Center;
            lockTmp.color = new Color(0.7f, 0.3f, 0.3f, 0.9f);
            lockTmp.raycastTarget = false;
            EnsureFontAsset(lockTmp);
        }

        // Boton
        Button btn = cardObj.GetComponent<Button>();
        btn.interactable = unlocked;
        btn.transition = Selectable.Transition.None;

        // Hover en tarjetas desbloqueadas
        if (unlocked)
        {
            CardHoverEffect hover = cardObj.AddComponent<CardHoverEffect>();
            hover.Init(cardImage.color);

            string capturedId = chapterId;
            btn.onClick.AddListener(() => GameManager.Instance.LoadChapterById(capturedId));
        }
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

        Image panelImg = mainMenuPanel.GetComponent<Image>();
        if (panelImg != null) panelImg.color = transparent;

        // Logo arriba a la izquierda
        CreateLogo(mainMenuPanel.transform);

        // Botones centrados con el logo
        float leftEdge = 0.075f;
        float rightEdge = 0.275f;
        float startY = 0.54f;
        float btnHeight = 0.05f;
        float gap = 0.012f;

        newGameButton = CreateFixedButton(mainMenuPanel.transform, newGameLabel, 32, ghostWhite,
            leftEdge, startY - btnHeight, rightEdge, startY);

        float y1 = startY - btnHeight - gap;
        continueButton = CreateFixedButton(mainMenuPanel.transform, continueLabel, 28, ghostWhite,
            leftEdge, y1 - btnHeight, rightEdge, y1);

        float y2 = y1 - btnHeight - gap;
        chaptersButton = CreateFixedButton(mainMenuPanel.transform, chaptersLabel, 28, ghostWhite,
            leftEdge, y2 - btnHeight, rightEdge, y2);

        float y3 = y2 - btnHeight - gap;
        quitButton = CreateFixedButton(mainMenuPanel.transform, quitLabel, 28, ghostWhite,
            leftEdge, y3 - btnHeight, rightEdge, y3);

        // Marco decorativo justo debajo del ultimo boton
        float marcoTop = y3 - btnHeight - 0.005f;
        CreateMarcoDecorativo(mainMenuPanel.transform, leftEdge, marcoTop);

        // Texto de ubicacion abajo a la izquierda
        CreateLocationText(mainMenuPanel.transform);
    }

    private void EnsureChaptersPanel()
    {
        if (chaptersPanel == null)
            chaptersPanel = CreateTransparentPanel("ChaptersPanel", rootCanvas.transform);

        Image panelImg = chaptersPanel.GetComponent<Image>();
        if (panelImg != null) panelImg.color = new Color(0, 0, 0, 0.85f);

        // Titulo
        Transform existingTitle = chaptersPanel.transform.Find("ChaptersTitle");
        if (existingTitle == null)
            CreateSectionTitle(chaptersPanel.transform, "CAPITULOS");

        // Contenedor horizontal para las tarjetas de capitulos
        if (chapterButtonsContainer == null)
        {
            GameObject container = new GameObject("ChaptersButtonContainer", typeof(RectTransform));
            container.transform.SetParent(chaptersPanel.transform, false);

            RectTransform cRect = container.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.10f, 0.10f);
            cRect.anchorMax = new Vector2(0.90f, 0.80f);
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;

            GridLayoutGroup glg = container.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(380, 380);
            glg.spacing = new Vector2(30, 20);
            glg.childAlignment = TextAnchor.MiddleCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            chapterButtonsContainer = container.transform;
        }

        // Boton volver
        if (backToMainMenuButton == null)
            backToMainMenuButton = CreateTextButton(chaptersPanel.transform, "VOLVER", 28, ghostWhite);
        else
            StyleTextButton(backToMainMenuButton, "VOLVER", 28, ghostWhite);

        RectTransform backRect = backToMainMenuButton.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.02f, 0.02f);
        backRect.anchorMax = new Vector2(0.15f, 0.10f);
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
        rect.anchorMin = new Vector2(0.01f, 0.55f);
        rect.anchorMax = new Vector2(0.34f, 0.95f);
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

    private static void CreateMarcoDecorativo(Transform parent, float leftEdge, float topY)
    {
        GameObject marcoObj = new GameObject("MarcoDecorativo", typeof(RectTransform), typeof(RawImage));
        marcoObj.transform.SetParent(parent, false);

        RectTransform rect = marcoObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(leftEdge + 0.02f, topY - 0.18f);
        rect.anchorMax = new Vector2(leftEdge + 0.18f, topY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        RawImage img = marcoObj.GetComponent<RawImage>();
        img.raycastTarget = false;

        Texture2D tex = Resources.Load<Texture2D>("Sprites/MarcoDebajo");
        if (tex != null)
        {
            img.texture = tex;
            img.color = Color.white;
            Debug.Log("MarcoDebajo cargado correctamente.");
        }
        else
        {
            Debug.LogError("NO se encontro MarcoDebajo en Resources/Sprites/. Verifica el nombre del archivo.");
            img.color = new Color(0.5f, 0.05f, 0.05f, 0.5f); // Rojo visible para debug
        }
    }

    private static Button CreateFixedButton(Transform parent, string label, int fontSize, Color color,
        float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
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

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(anchorMinX, anchorMinY);
        btnRect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label.ToUpper();
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.raycastTarget = false;
        EnsureFontAsset(tmp);

        ButtonHoverEffect hover = btnObj.AddComponent<ButtonHoverEffect>();
        hover.Init(color, bloodRed, fontSize);

        return btn;
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
        tmp.text = label.ToUpper();
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.raycastTarget = false;
        EnsureFontAsset(tmp);

        // Hover effect
        ButtonHoverEffect hover = btnObj.AddComponent<ButtonHoverEffect>();
        hover.Init(color, bloodRed, fontSize);

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredHeight = fontSize + 22;
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
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");

        if (font == null && TMP_Settings.instance != null)
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
