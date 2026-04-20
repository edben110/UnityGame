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
    [SerializeField] private string gameTitle = "Simon Mansion";

    [Header("Pantallas")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject chaptersPanel;

    [Header("Botones menu principal")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button chaptersButton;
    [SerializeField] private Button quitButton;

    [Header("Etiquetas por defecto")]
    [SerializeField] private string newGameLabel = "Nuevo Juego";
    [SerializeField] private string continueLabel = "Continuar";
    [SerializeField] private string chaptersLabel = "Capitulos";
    [SerializeField] private string quitLabel = "Salir";

    [Header("Capitulos")]
    [SerializeField] private Transform chapterButtonsContainer;
    [SerializeField] private ChapterButtonItem chapterButtonPrefab;
    [SerializeField] private Button backToMainMenuButton;

    private void Start()
    {
        ValidateReferences();
        EnsureTitle();
        EnsureMainMenuLabels();
        WireEvents();
        ShowMainMenu();
        RefreshContinueState();
    }

    private void OnDestroy()
    {
        UnwireEvents();
    }

    public void ShowMainMenu()
    {
        if (debugLogs)
        {
            Debug.Log("UIManager: Mostrar MainMenuPanel");
        }

        SetPanelState(mainMenuPanel, true);
        SetPanelState(chaptersPanel, false);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.transform.SetAsLastSibling();
        }

        RefreshContinueState();
    }

    public void ShowChapters()
    {
        if (debugLogs)
        {
            Debug.Log("UIManager: Click en Capítulos detectado. Mostrando ChaptersPanel.");
        }

        SetPanelState(mainMenuPanel, false);
        SetPanelState(chaptersPanel, true);

        if (chaptersPanel != null)
        {
            chaptersPanel.transform.SetAsLastSibling();
        }

        RefreshContinueState();
        BuildChapterButtons();
    }

    private void WireEvents()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(OnClickNewGame);
            newGameButton.onClick.AddListener(OnClickNewGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnClickContinue);
            continueButton.onClick.AddListener(OnClickContinue);
        }

        if (chaptersButton != null)
        {
            chaptersButton.onClick.RemoveListener(ShowChapters);
            chaptersButton.onClick.AddListener(ShowChapters);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnClickQuit);
            quitButton.onClick.AddListener(OnClickQuit);
        }

        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.RemoveListener(ShowMainMenu);
            backToMainMenuButton.onClick.AddListener(ShowMainMenu);
        }
    }

    private void UnwireEvents()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(OnClickNewGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnClickContinue);
        }

        if (chaptersButton != null)
        {
            chaptersButton.onClick.RemoveListener(ShowChapters);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnClickQuit);
        }

        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.RemoveListener(ShowMainMenu);
        }
    }

    private void OnClickNewGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no encontrado.");
            return;
        }

        GameManager.Instance.StartNewGame();
    }

    private void OnClickContinue()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no encontrado.");
            return;
        }

        GameManager.Instance.ContinueGame();
    }

    private void OnClickQuit()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no encontrado.");
            return;
        }

        GameManager.Instance.QuitGame();
    }

    private void RefreshContinueState()
    {
        if (continueButton == null)
        {
            return;
        }

        continueButton.interactable = GameManager.Instance != null && GameManager.Instance.CanContinue();
    }

    private void BuildChapterButtons()
    {
        if (chapterButtonsContainer == null)
        {
            Debug.LogError("No se asigno chapterButtonsContainer en UIManager.");
            return;
        }

        if (chaptersPanel != null && chapterButtonsContainer == chaptersPanel.transform)
        {
            Debug.LogWarning("chapterButtonsContainer apunta al ChaptersPanel completo. Debe apuntar al objeto interno ChaptersButtonContainer.");
        }

        if (chapterButtonPrefab == null)
        {
            Debug.LogError("No se asigno chapterButtonPrefab en UIManager.");
            return;
        }

        ClearContainer(chapterButtonsContainer);

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no encontrado.");
            return;
        }

        List<ChapterDefinition> unlocked = GameManager.Instance.GetUnlockedChapters();
        foreach (ChapterDefinition chapter in unlocked)
        {
            ChapterButtonItem item = Instantiate(chapterButtonPrefab, chapterButtonsContainer);
            item.transform.SetParent(chapterButtonsContainer, false);
            item.gameObject.SetActive(true);
            item.transform.SetAsLastSibling();

            string chapterId = chapter.id;
            string label = string.IsNullOrWhiteSpace(chapter.displayName) ? chapter.id : chapter.displayName;
            item.Setup(label, () => GameManager.Instance.LoadChapterById(chapterId));
        }

        if (debugLogs)
        {
            Debug.Log($"UIManager: Se generaron {unlocked.Count} botones de capítulos en {chapterButtonsContainer.name}.");
        }
    }

    private static void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child.GetComponent<ChapterButtonItem>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private static void SetPanelState(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void ValidateReferences()
    {
        if (mainMenuPanel == null)
        {
            Debug.LogError("UIManager: mainMenuPanel no esta asignado.");
        }

        if (chaptersPanel == null)
        {
            Debug.LogError("UIManager: chaptersPanel no esta asignado.");
        }

        if (chaptersButton == null)
        {
            Debug.LogError("UIManager: chaptersButton no esta asignado.");
        }

        if (backToMainMenuButton == null)
        {
            Debug.LogWarning("UIManager: backToMainMenuButton no esta asignado. No podras volver desde panel de capitulos.");
        }
    }

    private void EnsureMainMenuLabels()
    {
        EnsureButtonLabel(newGameButton, newGameLabel);
        EnsureButtonLabel(continueButton, continueLabel);
        EnsureButtonLabel(chaptersButton, chaptersLabel);
        EnsureButtonLabel(quitButton, quitLabel);
    }

    private void EnsureTitle()
    {
        if (gameTitleText == null)
        {
            return;
        }

        if (gameTitleText.font == null)
        {
            TMP_FontAsset fallbackFont = GetSafeTmpFont();
            if (fallbackFont != null)
            {
                gameTitleText.font = fallbackFont;
            }
        }

        gameTitleText.text = gameTitle;
        gameTitleText.gameObject.SetActive(true);
    }

    private static void EnsureButtonLabel(Button targetButton, string label)
    {
        if (targetButton == null)
        {
            return;
        }

        TMP_Text labelText = targetButton.GetComponentInChildren<TMP_Text>(true);
        if (labelText == null)
        {
            GameObject textObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(targetButton.transform, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            labelText = textObject.GetComponent<TextMeshProUGUI>();
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 28;
            labelText.raycastTarget = false;
        }

        if (labelText.color.a < 0.1f)
        {
            labelText.color = Color.black;
        }

        if (labelText.font == null)
        {
            TMP_FontAsset fallbackFont = GetSafeTmpFont();
            if (fallbackFont != null)
            {
                labelText.font = fallbackFont;
            }
            else
            {
                Debug.LogWarning("No hay fuente TMP disponible todavia. Importa TMP Essentials y reintenta.");
            }
        }

        labelText.text = label;
        labelText.gameObject.SetActive(true);
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
