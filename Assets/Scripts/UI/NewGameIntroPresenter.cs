using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Intro de nueva partida en MenuScene. La UI vive bajo el Canvas de la escena (editable en Scene View).
/// </summary>
public class NewGameIntroPresenter : MonoBehaviour
{
    public const string IntroUiRootName = "NewGameIntroUI";

    public static NewGameIntroPresenter Instance { get; private set; }

    private const string TutorialConversationId = "newgame_tutorial";
    private const string NarrativeConversationId = "newgame_narrative";

    [Serializable]
    public class IntroScreenView
    {
        public GameObject root;
        public Image background;
        public TMP_Text title;
    }

    [SerializeField] private string mainMapSceneName = "MainMapScene";
    [SerializeField] private string chapterAfterIntroId = "chapter1";

    [Header("UI bajo Canvas de MenuScene")]
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private IntroScreenView tutorialView = new IntroScreenView();
    [SerializeField] private IntroScreenView prologueView = new IntroScreenView();

    [Header("Diálogo compartido")]
    [SerializeField] private GameObject dialoguePanelRoot;
    [SerializeField] private Button overlayContinueButton;
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private DialoguePanelUI dialoguePanel;
    [SerializeField] private SamplePrologueChapter1Builder dialogueBuilder;

    private MenuDialogueUiFactory.MenuDialogueSystem dialogueSystem;
    private bool isRunning;

    public IntroScreenView TutorialView => tutorialView;
    public IntroScreenView PrologueView => prologueView;
    public RectTransform UiRoot => uiRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!HasSceneUi())
        {
            DontDestroyOnLoad(gameObject);
        }

        EnsureUiReady();
        HideAllIntroUi();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void CleanupAndDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        HideAllIntroUi();
        Destroy(gameObject);
    }

    public void BeginIntro()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        NewGamePendingState.Begin();
        QueueDefaultIntroFlags();
        HideMainMenu();
        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator RunIntroSequence()
    {
        EnsureDialogueData();

        yield return RunTutorialPhase();
        yield return RunNarrativePhase();

        isRunning = false;
        GameManager.Instance?.CompleteNewGameIntroAndLoadMainMap(chapterAfterIntroId, NewGamePendingState.Anxiety);
    }

    private IEnumerator RunTutorialPhase()
    {
        ShowTutorialView();
        SetOverlayContinueVisible(true);
        bool finished = false;
        dialogueSystem.Runner.ConversationEnded += OnConversationEnded;
        if (!dialogueSystem.Runner.StartConversation(TutorialConversationId, "start"))
        {
            dialogueSystem.Runner.ConversationEnded -= OnConversationEnded;
            yield break;
        }

        void OnConversationEnded(string id)
        {
            if (id == TutorialConversationId)
            {
                finished = true;
            }
        }

        while (!finished)
        {
            yield return null;
        }

        dialogueSystem.Runner.ConversationEnded -= OnConversationEnded;
        dialogueSystem.Panel.Hide();
        SetOverlayContinueVisible(false);
    }

    private IEnumerator RunNarrativePhase()
    {
        ShowPrologueView();
        SetOverlayContinueVisible(true);
        bool finished = false;
        dialogueSystem.Runner.ConversationEnded += OnConversationEnded;
        if (!dialogueSystem.Runner.StartConversation(NarrativeConversationId, "start"))
        {
            dialogueSystem.Runner.ConversationEnded -= OnConversationEnded;
            yield break;
        }

        void OnConversationEnded(string id)
        {
            if (id == NarrativeConversationId)
            {
                finished = true;
            }
        }

        while (!finished)
        {
            yield return null;
        }

        dialogueSystem.Runner.ConversationEnded -= OnConversationEnded;
        dialogueSystem.Panel.Hide();
        SetOverlayContinueVisible(false);
    }

    private void ShowTutorialView()
    {
        EnsureUiRootActive();
        SetViewActive(tutorialView, true);
        SetViewActive(prologueView, false);
        SetDialogueVisible(true);
    }

    private void ShowPrologueView()
    {
        EnsureUiRootActive();
        SetViewActive(tutorialView, false);
        SetViewActive(prologueView, true);
        SetDialogueVisible(true);
        ApplyPrologueSpriteFallback();
    }

    private void ApplyPrologueSpriteFallback()
    {
        if (prologueView.background == null || prologueView.background.sprite != null)
        {
            return;
        }

        Sprite sprite = LoadPrologoSprite();
        if (sprite != null)
        {
            prologueView.background.sprite = sprite;
            prologueView.background.preserveAspect = true;
        }
    }

    private static Sprite LoadPrologoSprite()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Prologo");
        if (sprites != null)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null && sprites[i].name == "Prologo_0")
                {
                    return sprites[i];
                }
            }
        }

        sprites = Resources.LoadAll<Sprite>("Prologo");
        if (sprites != null)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null && sprites[i].name == "Prologo_0")
                {
                    return sprites[i];
                }
            }
        }

        return Resources.Load<Sprite>("Sprites/Prologo");
    }

    private void EnsureDialogueData()
    {
        EnsureDialogueSystem();

        if (dialogueBuilder == null && dialogueSystem.Runner != null)
        {
            dialogueBuilder = dialogueSystem.Runner.GetComponent<SamplePrologueChapter1Builder>();
            if (dialogueBuilder == null)
            {
                dialogueBuilder = dialogueSystem.Runner.gameObject.AddComponent<SamplePrologueChapter1Builder>();
            }
        }

        dialogueBuilder?.EnsureMenuIntroData();
    }

    private void EnsureUiReady()
    {
        if (HasSceneUi())
        {
            BindSceneDialogueSystem();
            WireOverlayContinueButton();
            return;
        }

        EnsureRuntimeUiUnderSceneCanvas();
        EnsureDialogueSystem();
    }

    private void BindSceneDialogueSystem()
    {
        Canvas canvas = uiRoot != null ? uiRoot.GetComponentInParent<Canvas>() : null;
        dialogueSystem = new MenuDialogueUiFactory.MenuDialogueSystem
        {
            Library = dialogueRunner != null ? dialogueRunner.GetComponent<DialogueLibrary>() : null,
            Runner = dialogueRunner,
            Panel = dialoguePanel,
            Canvas = canvas
        };

        if (overlayContinueButton != null && overlayContinueButton.GetComponent<CursorHoverUI>() == null)
        {
            overlayContinueButton.gameObject.AddComponent<CursorHoverUI>();
        }
    }

    private void EnsureDialogueSystem()
    {
        if (dialogueSystem.Runner != null || uiRoot == null)
        {
            return;
        }

        dialogueSystem = MenuDialogueUiFactory.Create(uiRoot);
        dialogueRunner = dialogueSystem.Runner;
        dialoguePanel = dialogueSystem.Panel;
        dialoguePanelRoot = dialogueSystem.Panel != null
            ? dialogueSystem.Panel.transform.parent?.gameObject
            : null;
        EnsureOverlayContinueButton();
        WireOverlayContinueButton();

        if (overlayContinueButton != null)
        {
            overlayContinueButton.transform.SetAsLastSibling();
        }
    }

    private void EnsureRuntimeUiUnderSceneCanvas()
    {
        if (uiRoot != null)
        {
            return;
        }

        Canvas sceneCanvas = FindSceneMenuCanvas();
        if (sceneCanvas == null)
        {
            Debug.LogError("[NewGameIntro] No hay Canvas en la escena para crear la UI de intro.");
            return;
        }

        GameObject rootObj = new GameObject(IntroUiRootName, typeof(RectTransform), typeof(MenuIntroUiLayout));
        uiRoot = rootObj.GetComponent<RectTransform>();
        uiRoot.SetParent(sceneCanvas.transform, false);
        MenuIntroUiLayout.StretchFullScreen(uiRoot);
        rootObj.layer = 5;

        tutorialView.root = CreateRuntimeView(uiRoot, "TutorialView", Color.black, null, "Tutorial", true);
        tutorialView.background = tutorialView.root.GetComponentInChildren<Image>();
        tutorialView.title = tutorialView.root.GetComponentInChildren<TMP_Text>();

        prologueView.root = CreateRuntimeView(uiRoot, "PrologueView", Color.white, LoadPrologoSprite(), null, false);
        prologueView.background = prologueView.root.GetComponentInChildren<Image>();
        prologueView.title = prologueView.root.GetComponentInChildren<TMP_Text>(true);

        rootObj.SetActive(false);
    }

    private static Canvas FindSceneMenuCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].gameObject.name == "Canvas")
            {
                return canvases[i];
            }
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    private static GameObject CreateRuntimeView(Transform parent, string name, Color bgColor, Sprite bgSprite, string title, bool showTitle)
    {
        GameObject viewRoot = new GameObject(name, typeof(RectTransform));
        viewRoot.layer = 5;
        viewRoot.transform.SetParent(parent, false);
        RectTransform viewRect = viewRoot.GetComponent<RectTransform>();
        MenuIntroUiLayout.StretchFullScreen(viewRect);

        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.layer = 5;
        bgObj.transform.SetParent(viewRoot.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        MenuIntroUiLayout.StretchFullScreen(bgRect);
        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.color = bgColor;
        bgImage.sprite = bgSprite;
        bgImage.preserveAspect = bgSprite != null;
        bgImage.raycastTarget = false;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer));
        titleObj.layer = 5;
        titleObj.transform.SetParent(viewRoot.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.82f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 56f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.92f, 0.84f, 0.62f, 1f);
        titleText.text = title ?? string.Empty;
        titleObj.SetActive(showTitle);

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");
        if (font != null)
        {
            titleText.font = font;
        }

        viewRoot.SetActive(false);
        return viewRoot;
    }

    private void EnsureOverlayContinueButton()
    {
        if (overlayContinueButton != null || uiRoot == null)
        {
            return;
        }

        overlayContinueButton = MenuDialogueUiFactory.CreateOverlayContinueButton(uiRoot);
        overlayContinueButton.gameObject.SetActive(false);

        if (overlayContinueButton.GetComponent<CursorHoverUI>() == null)
        {
            overlayContinueButton.gameObject.AddComponent<CursorHoverUI>();
        }

        overlayContinueButton.transform.SetAsLastSibling();
    }

    private void WireOverlayContinueButton()
    {
        if (overlayContinueButton == null || dialogueSystem.Panel == null)
        {
            return;
        }

        overlayContinueButton.onClick.RemoveAllListeners();
        overlayContinueButton.onClick.AddListener(() => dialogueSystem.Panel.InvokeContinue());
    }

    private void SetOverlayContinueVisible(bool visible)
    {
        if (overlayContinueButton == null)
        {
            return;
        }

        overlayContinueButton.gameObject.SetActive(visible);
        overlayContinueButton.interactable = visible;
    }

    private static void QueueDefaultIntroFlags()
    {
        NewGamePendingState.SetFlag("session.started", true);
        NewGamePendingState.SetFlag("chapter.prologue.complete", true);
        NewGamePendingState.SetFlag("chapter1.intro.seen", true);
        NewGamePendingState.SetFlag("newgame.intro.completed", true);
    }

    private static void HideMainMenu()
    {
        UIManager ui = FindAnyObjectByType<UIManager>();
        if (ui != null)
        {
            ui.HideAllMenuUi();
        }
    }

    private void EnsureUiRootActive()
    {
        if (uiRoot != null)
        {
            uiRoot.gameObject.SetActive(true);
        }
    }

    private void SetDialogueVisible(bool visible)
    {
        if (dialoguePanelRoot != null)
        {
            dialoguePanelRoot.SetActive(visible);
        }
    }

    private void HideAllIntroUi()
    {
        SetOverlayContinueVisible(false);
        SetViewActive(tutorialView, false);
        SetViewActive(prologueView, false);
        SetDialogueVisible(false);

        if (uiRoot != null && Application.isPlaying)
        {
            uiRoot.gameObject.SetActive(false);
        }
    }

    private static void SetViewActive(IntroScreenView view, bool active)
    {
        if (view?.root != null)
        {
            view.root.SetActive(active);
        }
    }

    private bool HasSceneUi()
    {
        return uiRoot != null
               && tutorialView.root != null
               && tutorialView.background != null
               && prologueView.root != null
               && prologueView.background != null
               && dialogueRunner != null
               && dialoguePanel != null;
    }

#if UNITY_EDITOR
    public void EditorBeginEditTutorial()
    {
        EnsureUiReady();
        PrepareUiRootForEditor();
        ShowTutorialView();
        SetOverlayContinueVisible(true);
        UnityEditor.SceneView.lastActiveSceneView?.FrameSelected();
        UnityEditor.SceneView.RepaintAll();
    }

    public void EditorBeginEditPrologue()
    {
        EnsureUiReady();
        PrepareUiRootForEditor();
        ShowPrologueView();
        SetOverlayContinueVisible(true);
        UnityEditor.SceneView.lastActiveSceneView?.FrameSelected();
        UnityEditor.SceneView.RepaintAll();
    }

    public void EditorHidePreview()
    {
        HideAllIntroUi();
        UnityEditor.SceneView.RepaintAll();
    }

    private void PrepareUiRootForEditor()
    {
        if (uiRoot == null)
        {
            return;
        }

        uiRoot.gameObject.SetActive(true);
        MenuIntroUiLayout.StretchFullScreen(uiRoot);

        Canvas sceneCanvas = uiRoot.GetComponentInParent<Canvas>();
        if (sceneCanvas != null && sceneCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            sceneCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            Camera cam = sceneCanvas.worldCamera;
            if (cam == null)
            {
                cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
                sceneCanvas.worldCamera = cam;
            }

            sceneCanvas.planeDistance = 10f;
        }
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureOnMenuScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != "MenuScene")
        {
            return;
        }

        if (Instance != null || FindAnyObjectByType<NewGameIntroPresenter>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(NewGameIntroPresenter));
        host.AddComponent<NewGameIntroPresenter>();
    }
}
