using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanelUI : MonoBehaviour
{
    private static DialoguePanelUI instance;

    [SerializeField] private float choiceButtonHorizontalPadding = 40f;
    [SerializeField] private float choiceButtonVerticalPadding = 20f;

    [Header("Panel")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button continueButton;

    [Header("Opciones")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private readonly List<Button> activeChoiceButtons = new List<Button>();
    private Action pendingContinueAction;

    public static DialoguePanelUI Instance => instance;

    public event Action ContinuePressed;
    public event Action<int> ChoicePressed;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinuePressed);
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinuePressed);
        }

        ClearChoices();
    }

    public void ShowLine(string speaker, string text)
    {
        ShowLine(speaker, text, null);
    }

    public void ShowLine(string speaker, string text, Action continueAction)
    {
        pendingContinueAction = continueAction;

        if (root != null)
        {
            root.SetActive(true);
        }

        if (speakerText != null)
        {
            speakerText.text = string.IsNullOrWhiteSpace(speaker) ? "Narrador" : speaker;
        }

        if (bodyText != null)
        {
            bodyText.text = text ?? string.Empty;
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
        }

        SetChoicesVisible(false);
    }

    public void ShowSystemMessage(string text, Action continueAction = null)
    {
        ShowLine("Narrador", text, continueAction ?? Hide);
    }

    private void HandleContinuePressed()
    {
        ContinuePressed?.Invoke();

        Action action = pendingContinueAction;
        pendingContinueAction = null;
        action?.Invoke();
    }

    public void ShowChoices(List<DialogueChoice> choices)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        ClearChoices();

        if (choicesContainer == null || choiceButtonPrefab == null || choices == null)
        {
            return;
        }

        ConfigureChoicesContainer();
        SetChoicesVisible(true);

        for (int i = 0; i < choices.Count; i++)
        {
            int capturedIndex = i;
            DialogueChoice choice = choices[i];

            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.gameObject.SetActive(true);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = choice != null ? choice.text : "...";
                ConfigureChoiceLabel(label);
            }

            ConfigureChoiceButton(button, label);
            button.onClick.AddListener(() => ChoicePressed?.Invoke(capturedIndex));
            activeChoiceButtons.Add(button);
        }

        if (choicesContainer is RectTransform choicesRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(choicesRect);
        }
    }

    public void Hide()
    {
        pendingContinueAction = null;

        if (root != null)
        {
            root.SetActive(false);
        }

        ClearChoices();
    }

    private void ClearChoices()
    {
        for (int i = 0; i < activeChoiceButtons.Count; i++)
        {
            if (activeChoiceButtons[i] != null)
            {
                Destroy(activeChoiceButtons[i].gameObject);
            }
        }

        activeChoiceButtons.Clear();
    }

    private void SetChoicesVisible(bool visible)
    {
        if (choicesContainer != null)
        {
            choicesContainer.gameObject.SetActive(visible);
        }
    }

    private void ConfigureChoicesContainer()
    {
        if (choicesContainer == null)
        {
            return;
        }

        VerticalLayoutGroup layout = choicesContainer.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 0, 4, 4);
        }

        ContentSizeFitter fitter = choicesContainer.GetComponent<ContentSizeFitter>();
        if (fitter != null)
        {
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private void ConfigureChoiceButton(Button button, TMP_Text label)
    {
        if (button == null || label == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null)
        {
            return;
        }

        Vector2 preferredSize = label.GetPreferredValues(label.text, Mathf.Infinity, Mathf.Infinity);
        float buttonWidth = Mathf.Ceil(preferredSize.x + choiceButtonHorizontalPadding);
        float buttonHeight = Mathf.Ceil(Mathf.Max(preferredSize.y + choiceButtonVerticalPadding, 40f));

        LayoutElement layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = button.gameObject.AddComponent<LayoutElement>();
        }

        layoutElement.minWidth = buttonWidth;
        layoutElement.minHeight = buttonHeight;
        layoutElement.preferredWidth = buttonWidth;
        layoutElement.preferredHeight = buttonHeight;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;

        ContentSizeFitter buttonFitter = button.GetComponent<ContentSizeFitter>();
        if (buttonFitter == null)
        {
            buttonFitter = button.gameObject.AddComponent<ContentSizeFitter>();
        }

        buttonFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        buttonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private static void ConfigureChoiceLabel(TMP_Text label)
    {
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Overflow;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }
}
