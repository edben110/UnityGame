using System;
using TMPro;
using UnityEngine;
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
    [SerializeField] private Button closeButton;

    private Action onTalk;
    private Action onVerify;

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

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show(string npcName, Action talkAction, Action verifyAction)
    {
        onTalk = talkAction;
        onVerify = verifyAction;

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(npcName) ? "Interaccion" : npcName;
        }

        if (statusText != null)
        {
            statusText.text = "Elige una opcion.";
        }

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
}
