using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcInteractable : Interactable
{
    private const string ItemConversationSeparator = "_item_";

    [Header("NPC")]
    [SerializeField] private string npcId;
    [SerializeField] private string npcDisplayName;

    [Header("Flujo")]
    [SerializeField] private string requiredChapterId = "chapter1";
    [SerializeField] private string talkConversationId;

    public bool OpenInteractionForItem(string itemId)
    {
        if (!CanOpenInteraction(out DialogueRunner runner))
        {
            return false;
        }

        if (NpcInteractionMenuUI.Instance == null)
        {
            Debug.LogError("No se encontro NpcInteractionMenuUI en la escena.");
            return false;
        }

        Action askItemAction;
        string askItemLabel;
        bool hasItemAction = TryBuildSpecificItemQuestionAction(runner, itemId, out askItemAction, out askItemLabel);

        NpcInteractionMenuUI.Instance.Show(GetDisplayName(), OnTalkPressed, OnVerifyPressed, hasItemAction ? askItemAction : null, hasItemAction ? askItemLabel : string.Empty);

        if (!hasItemAction)
        {
            NpcInteractionMenuUI.Instance.ShowStatusText("Este personaje no puede responder sobre ese objeto todavia.");
        }

        return true;
    }

    public override void Interact()
    {
        base.Interact();

        if (!CanOpenInteraction(out DialogueRunner runner))
        {
            return;
        }

        if (NpcInteractionMenuUI.Instance == null)
        {
            Debug.LogError("No se encontro NpcInteractionMenuUI en la escena.");
            return;
        }

        Action askItemAction;
        string askItemLabel;
        TryBuildItemQuestionAction(runner, out askItemAction, out askItemLabel);

        NpcInteractionMenuUI.Instance.Show(GetDisplayName(), OnTalkPressed, OnVerifyPressed, askItemAction, askItemLabel);
    }

    private bool CanOpenInteraction(out DialogueRunner runner)
    {
        runner = null;

        if (StoryState.Instance == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(requiredChapterId) && StoryState.Instance.CurrentChapterId != requiredChapterId)
        {
            return false;
        }

        runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null || runner.IsRunning)
        {
            return false;
        }

        return true;
    }

    private void OnTalkPressed()
    {
        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            return;
        }

        bool started = runner.StartConversation(talkConversationId, "start");
        if (!started)
        {
            return;
        }

        if (CharacterAnxietySystem.Instance != null)
        {
            CharacterAnxietySystem.Instance.ApplyTalkRelief(npcId);
        }

        NpcInteractionMenuUI.Instance.Hide();
    }

    private void OnVerifyPressed()
    {
        if (NpcInteractionMenuUI.Instance == null)
        {
            return;
        }

        if (CharacterAnxietySystem.Instance == null)
        {
            NpcInteractionMenuUI.Instance.ShowStatusText("Sistema de ansiedad no configurado.");
            AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
            if (anxietySystem != null)
            {
                anxietySystem.HideVerificationOverlay();
            }
            return;
        }

        float anxiety = CharacterAnxietySystem.Instance.GetAnxiety(npcId);
        NpcInteractionMenuUI.Instance.ShowStatusText(CharacterAnxietySystem.Instance.GetFormattedStatus(npcId));

        AnxietySystem overlaySystem = FindAnyObjectByType<AnxietySystem>();
        if (overlaySystem != null)
        {
            overlaySystem.ShowVerificationOverlay(anxiety / 100f);
        }
    }

    private void OnAskItemPressed(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return;
        }

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            return;
        }

        bool started = runner.StartConversation(conversationId, "start");
        if (!started)
        {
            return;
        }

        AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
        if (anxietySystem != null)
        {
            anxietySystem.HideVerificationOverlay();
        }

        NpcInteractionMenuUI.Instance.Hide();
    }

    private void TryBuildItemQuestionAction(DialogueRunner runner, out Action action, out string label)
    {
        action = null;
        label = string.Empty;

        if (runner == null || string.IsNullOrWhiteSpace(talkConversationId))
        {
            return;
        }

        List<string> items = InventoryState.GetItems();
        for (int i = 0; i < items.Count; i++)
        {
            string itemId = items[i];
            if (string.IsNullOrWhiteSpace(itemId))
            {
                continue;
            }

            if (!TryBuildSpecificItemQuestionAction(runner, itemId, out action, out label))
            {
                continue;
            }

            return;
        }
    }

    private bool TryBuildSpecificItemQuestionAction(DialogueRunner runner, string itemId, out Action action, out string label)
    {
        action = null;
        label = string.Empty;

        if (runner == null || string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        string conversationId = BuildItemConversationId(itemId);
        if (!runner.HasConversation(conversationId))
        {
            return false;
        }

        string capturedConversationId = conversationId;
        action = () => OnAskItemPressed(capturedConversationId);
        label = BuildItemQuestionLabel(itemId);
        return true;
    }

    private string BuildItemConversationId(string itemId)
    {
        return $"{talkConversationId}{ItemConversationSeparator}{NormalizeItemId(itemId)}";
    }

    private static string NormalizeItemId(string itemId)
    {
        return string.IsNullOrWhiteSpace(itemId)
            ? string.Empty
            : itemId.Trim().ToLowerInvariant();
    }

    private static string BuildItemQuestionLabel(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return "Preguntar por objeto";
        }

        if (InventoryCatalog.Instance != null)
        {
            string displayName = InventoryCatalog.Instance.GetDisplayNameOrFallback(itemId);
            return $"Preguntar por {displayName}";
        }

        if (itemId.Contains("foto", StringComparison.OrdinalIgnoreCase))
        {
            return "Preguntar por la foto";
        }

        return $"Preguntar por {itemId.Replace('_', ' ')}";
    }

    private string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(npcDisplayName))
        {
            return npcDisplayName;
        }

        if (!string.IsNullOrWhiteSpace(npcId))
        {
            return npcId;
        }

        return gameObject.name;
    }
}
