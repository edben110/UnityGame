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
    [SerializeField] private Button verifyAnxietyButton;
    [FormerlySerializedAs("contextButton")]
    [SerializeField] private Button askItemButton;
    [FormerlySerializedAs("contextButtonText")]
    [SerializeField] private TMP_Text askItemButtonText;
    [SerializeField] private Button closeButton;

    private Action onTalk;
    private Action onVerify;
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
        }

        if (verifyAnxietyButton != null)
        {
            verifyAnxietyButton.onClick.AddListener(HandleVerify);
        }

        if (GetAskButton() != null)
        {
            GetAskButton().onClick.AddListener(HandleAskItem);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
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

        if (verifyAnxietyButton != null)
        {
            verifyAnxietyButton.onClick.RemoveListener(HandleVerify);
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

    public void Show(string npcName, Action talkAction, Action verifyAction, Action askItemAction = null, string askItemLabel = "")
    {
        onTalk = talkAction;
        onVerify = verifyAction;
        onAskItem = askItemAction;
        askItemLabelOverride = askItemLabel ?? string.Empty;
        askItemVisible = askItemAction != null;

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(npcName) ? "Interaccion" : npcName;
        }

        if (statusText != null)
        {
            statusText.text = "Elige una opcion.";
        }

        SetAskItemButtonState(askItemVisible);
        RefreshAskItemButtonLabel();

        if (root != null)
        {
            root.SetActive(true);
        }
    }

    public void ShowStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text ?? string.Empty;
        }
    }

public void Hide()
    {
        onTalk = null;
        onVerify = null;
        onAskItem = null;
        askItemLabelOverride = string.Empty;
        askItemVisible = false;

        // NO ocultamos el overlay de ansiedad aqui;
        // el overlay se gestiona independientemente y debe
        // permanecer hasta que el jugador cambie de contexto.

        SetAskItemButtonState(false);

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void HandleTalk()
    {
        onTalk?.Invoke();
    }

    private void HandleVerify()
    {
        onVerify?.Invoke();
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

        buttonLabel.text = $"Preguntar por: {displayName}";
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
