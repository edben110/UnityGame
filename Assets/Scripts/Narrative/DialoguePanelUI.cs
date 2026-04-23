using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button continueButton;

    [Header("Opciones")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private readonly List<Button> activeChoiceButtons = new List<Button>();

    public event Action ContinuePressed;
    public event Action<int> ChoicePressed;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(() => ContinuePressed?.Invoke());
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
        }

        ClearChoices();
    }

    public void ShowLine(string speaker, string text)
    {
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
            }

            button.onClick.AddListener(() => ChoicePressed?.Invoke(capturedIndex));
            activeChoiceButtons.Add(button);
        }
    }

    public void Hide()
    {
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
}
