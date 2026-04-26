using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NpcInteractionMenuUI : MonoBehaviour
{
    public static NpcInteractionMenuUI Instance { get; private set; }

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

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(npcName) ? "Interaccion" : npcName;
        }

        if (statusText != null)
        {
            statusText.text = "Elige una opcion.";
        }

        SetAskItemButtonState(askItemAction != null, askItemLabel);

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

        SetAskItemButtonState(false, string.Empty);

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

    private void SetAskItemButtonState(bool visible, string askItemLabel)
    {
        Button button = GetAskButton();
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(visible);
        if (!visible)
        {
            return;
        }

        TMP_Text buttonLabel = askItemButtonText != null ? askItemButtonText : button.GetComponentInChildren<TMP_Text>(true);
        if (buttonLabel != null)
        {
            buttonLabel.text = string.IsNullOrWhiteSpace(askItemLabel) ? "Preguntar por objeto" : askItemLabel;
        }
    }

    private Button GetAskButton()
    {
        return askItemButton;
    }
}
