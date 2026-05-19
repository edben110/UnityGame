using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Intro de nueva partida en MenuScene: Tutorial (fondo negro) y narrativa (Prologo_0).
/// </summary>
public class NewGameIntroPresenter : MonoBehaviour
{
    public static NewGameIntroPresenter Instance { get; private set; }

    private const string TutorialConversationId = "newgame_tutorial";
    private const string NarrativeConversationId = "newgame_narrative";

    [SerializeField] private string mainMapSceneName = "MainMapScene";
    [SerializeField] private string chapterAfterIntroId = "chapter1";

    private Canvas overlayCanvas;
    private Image backgroundImage;
    private TMP_Text titleText;
    private Button overlayContinueButton;
    private MenuDialogueUiFactory.MenuDialogueSystem dialogueSystem;
    private SamplePrologueChapter1Builder dialogueBuilder;
    private bool isRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureOverlayUi();
        EnsureDialogueSystem();
        HideOverlay();
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
        ApplyTutorialVisuals();
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
        ApplyNarrativeVisuals();
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

    private void ApplyTutorialVisuals()
    {
        ShowOverlay();
        backgroundImage.color = Color.black;
        backgroundImage.sprite = null;
        titleText.gameObject.SetActive(true);
        titleText.text = "Tutorial";
    }

    private void ApplyNarrativeVisuals()
    {
        ShowOverlay();
        titleText.gameObject.SetActive(false);
        backgroundImage.sprite = LoadPrologoSprite();
        backgroundImage.color = Color.white;
        backgroundImage.preserveAspect = true;
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

    private void EnsureDialogueSystem()
    {
        if (dialogueSystem.Runner != null)
        {
            return;
        }

        dialogueSystem = MenuDialogueUiFactory.Create(overlayCanvas.transform);
        EnsureOverlayContinueButton();
        WireOverlayContinueButton();

        if (overlayContinueButton != null)
        {
            overlayContinueButton.transform.SetAsLastSibling();
        }
    }

    private void EnsureOverlayUi()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        GameObject canvasObj = new GameObject("NewGameIntroOverlay");
        canvasObj.transform.SetParent(transform, false);
        overlayCanvas = canvasObj.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.raycastTarget = false;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.82f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 56f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.92f, 0.84f, 0.62f, 1f);

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");
        if (font != null)
        {
            titleText.font = font;
        }

        EnsureOverlayContinueButton();
    }

    private void EnsureOverlayContinueButton()
    {
        if (overlayContinueButton != null || overlayCanvas == null)
        {
            return;
        }

        overlayContinueButton = MenuDialogueUiFactory.CreateOverlayContinueButton(overlayCanvas.transform);
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

    private void ShowOverlay()
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(true);
        }
    }

    private void HideOverlay()
    {
        SetOverlayContinueVisible(false);

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureOnMenuScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != "MenuScene")
        {
            return;
        }

        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(NewGameIntroPresenter));
        host.AddComponent<NewGameIntroPresenter>();
    }
}
