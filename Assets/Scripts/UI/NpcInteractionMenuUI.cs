using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NpcInteractionMenuUI : MonoBehaviour
{
    public static NpcInteractionMenuUI Instance { get; private set; }

    public bool IsOpen => root != null && root.activeInHierarchy;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button talkButton;
    [FormerlySerializedAs("verifyAnxietyButton")]
    [SerializeField] private Button verifyAnxietyButton;
    [FormerlySerializedAs("contextButton")]
    [SerializeField] private Button askItemButton;
    [FormerlySerializedAs("contextButtonText")]
    [SerializeField] private TMP_Text askItemButtonText;
    [SerializeField] private Button closeButton;

    private Action onTalk;
    private Action onAskItem;
    private string askItemLabelOverride = string.Empty;
    private bool askItemVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (talkButton != null)
        {
            talkButton.onClick.AddListener(HandleTalk);
            EnsureCursorHover(talkButton.gameObject);
        }

        if (verifyAnxietyButton != null)
        {
            verifyAnxietyButton.gameObject.SetActive(false);
        }

        if (GetAskButton() != null)
        {
            GetAskButton().onClick.AddListener(HandleAskItem);
            EnsureCursorHover(GetAskButton().gameObject);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
            EnsureCursorHover(closeButton.gameObject);
        }

        Hide();
    }

    private static void EnsureCursorHover(GameObject obj)
    {
        if (obj != null && obj.GetComponent<CursorHoverUI>() == null)
        {
            obj.AddComponent<CursorHoverUI>();
        }
    }

    private void OnEnable()
    {
        InventoryState.SelectedChanged += OnSelectedItemChanged;
    }

    private void OnDisable()
    {
        InventoryState.SelectedChanged -= OnSelectedItemChanged;
    }

    private void OnDestroy()
    {
        if (talkButton != null)
        {
            talkButton.onClick.RemoveListener(HandleTalk);
        }

        if (GetAskButton() != null)
        {
            GetAskButton().onClick.RemoveListener(HandleAskItem);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show(string npcName, string npcId, Action talkAction, Action askItemAction = null, string askItemLabel = "")
    {
        onTalk = talkAction;
        onAskItem = askItemAction;
        askItemLabelOverride = askItemLabel ?? string.Empty;
        askItemVisible = askItemAction != null;

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(npcName) ? "Interaccion" : npcName;
        }

        RefreshNpcAnxiety(npcId);
        SetAskItemButtonState(askItemVisible);
        RefreshAskItemButtonLabel();

        if (root != null)
        {
            root.SetActive(true);
        }
    }

    public void ShowStatusText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(text);
            return;
        }

        Debug.Log($"[NpcInteractionMenuUI] {text}");
    }

    public void Hide()
    {
        onTalk = null;
        onAskItem = null;
        askItemLabelOverride = string.Empty;
        askItemVisible = false;

        SetAskItemButtonState(false);

        if (root != null)
        {
            root.SetActive(false);
        }

        AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
        if (anxietySystem != null)
        {
            anxietySystem.HideVerificationOverlay();
        }

        CursorManager.SetDefault();
    }

    private void RefreshNpcAnxiety(string npcId)
    {
        if (CharacterAnxietySystem.Instance == null || string.IsNullOrWhiteSpace(npcId))
        {
            DisplayAnxietyValue(0);
            return;
        }

        float anxiety = CharacterAnxietySystem.Instance.GetAnxiety(npcId);
        DisplayAnxietyValue(Mathf.RoundToInt(anxiety));
    }

    private void DisplayAnxietyValue(int anxietyValue)
    {
        int clamped = Mathf.Clamp(anxietyValue, 0, 100);

        AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
        if (anxietySystem != null)
        {
            anxietySystem.ShowNpcAnxiety(clamped);
            return;
        }

        if (statusText == null)
        {
            return;
        }

        statusText.gameObject.SetActive(true);
        statusText.richText = true;
        statusText.text = $"<color=#FFFFFF><b>{clamped}</b></color><color=#8A8580>/100</color>";
        statusText.color = Color.white;
    }

    private void HandleTalk()
    {
        onTalk?.Invoke();
    }

    private void HandleAskItem()
    {
        onAskItem?.Invoke();
    }

    private void SetAskItemButtonState(bool visible)
    {
        Button button = GetAskButton();
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(visible);
    }

    private void RefreshAskItemButtonLabel()
    {
        if (!askItemVisible)
        {
            return;
        }

        Button button = GetAskButton();
        if (button == null)
        {
            return;
        }

        TMP_Text buttonLabel = askItemButtonText != null ? askItemButtonText : button.GetComponentInChildren<TMP_Text>(true);
        if (buttonLabel == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(askItemLabelOverride))
        {
            buttonLabel.text = askItemLabelOverride;
            return;
        }

        string selectedItem = InventoryState.CurrentlySelectedInventoryItem;
        if (string.IsNullOrWhiteSpace(selectedItem))
        {
            buttonLabel.text = "Preguntar por objeto";
            return;
        }

        string displayName = selectedItem.Replace('_', ' ');
        if (InventoryCatalog.Instance != null)
        {
            displayName = InventoryCatalog.Instance.GetDisplayNameOrFallback(selectedItem);
        }

        buttonLabel.text = $"Preguntar por {displayName}";
    }

    private void OnSelectedItemChanged(string _)
    {
        RefreshAskItemButtonLabel();
    }

    private Button GetAskButton()
    {
        return askItemButton;
    }
}
