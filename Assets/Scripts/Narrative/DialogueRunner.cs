using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueLibrary library;
    [SerializeField] private DialoguePanelUI panelUI;

    private DialogueConversation activeConversation;
    private DialogueNode activeNode;
    private int lineIndex;
    private List<DialogueChoice> currentVisibleChoices = new List<DialogueChoice>();

    public bool IsRunning { get; private set; }

    public event Action<string> ConversationEnded;

    private void Awake()
    {
        if (panelUI != null)
        {
            panelUI.ContinuePressed += OnContinuePressed;
            panelUI.ChoicePressed += OnChoicePressed;
            panelUI.Hide();
        }
    }

    private void OnDestroy()
    {
        if (panelUI != null)
        {
            panelUI.ContinuePressed -= OnContinuePressed;
            panelUI.ChoicePressed -= OnChoicePressed;
        }
    }

    public bool StartConversation(string conversationId, string startNodeId = "start")
    {
        if (library == null || panelUI == null)
        {
            Debug.LogError("DialogueRunner necesita referencias de library y panelUI.");
            return false;
        }

        DialogueConversation conversation = library.GetConversation(conversationId);
        if (conversation == null)
        {
            Debug.LogError($"No existe conversacion: {conversationId}");
            return false;
        }

        DialogueNode node = conversation.GetNode(startNodeId);
        if (node == null)
        {
            Debug.LogError($"No existe nodo inicial '{startNodeId}' para '{conversationId}'.");
            return false;
        }

        activeConversation = conversation;
        activeNode = node;
        lineIndex = 0;
        IsRunning = true;

        ShowCurrentLineOrChoices();
        return true;
    }

    public bool HasConversation(string conversationId)
    {
        if (library == null || string.IsNullOrWhiteSpace(conversationId))
        {
            return false;
        }

        return library.HasConversation(conversationId);
    }

    private void OnContinuePressed()
    {
        if (!IsRunning || activeNode == null)
        {
            return;
        }

        lineIndex++;
        ShowCurrentLineOrChoices();
    }

    private void OnChoicePressed(int index)
    {
        if (!IsRunning || activeNode == null)
        {
            return;
        }

        if (index < 0 || index >= currentVisibleChoices.Count)
        {
            return;
        }

        DialogueChoice selected = currentVisibleChoices[index];
        if (selected == null)
        {
            return;
        }

        if (selected.anxietyDelta != 0f && StoryState.Instance != null)
        {
            StoryState.Instance.AddAnxiety(selected.anxietyDelta);
        }

        if (!string.IsNullOrWhiteSpace(selected.setFlag) && StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(selected.setFlag, true);
        }

        if (!string.IsNullOrWhiteSpace(selected.id) && StoryState.Instance != null)
        {
            StoryState.Instance.SetDecision($"choice.{activeConversation.id}.{activeNode.id}", selected.id);
        }

        if (string.IsNullOrWhiteSpace(selected.nextNodeId))
        {
            EndConversation();
            return;
        }

        MoveToNode(selected.nextNodeId);
    }

    private void ShowCurrentLineOrChoices()
    {
        if (activeNode == null)
        {
            EndConversation();
            return;
        }

        if (lineIndex < activeNode.lines.Count)
        {
            DialogueLine line = activeNode.lines[lineIndex];
            if (!LineAllowedByFlags(line))
            {
                lineIndex++;
                ShowCurrentLineOrChoices();
                return;
            }

            ApplyLineEffects(line);
            panelUI.ShowLine(line.speaker, line.text);
            return;
        }

        currentVisibleChoices = BuildVisibleChoices(activeNode.choices);
        if (currentVisibleChoices.Count > 0)
        {
            panelUI.ShowChoices(currentVisibleChoices);
            return;
        }

        if (!string.IsNullOrWhiteSpace(activeNode.nextNodeIdIfNoChoices))
        {
            MoveToNode(activeNode.nextNodeIdIfNoChoices);
            return;
        }

        if (activeNode.endsConversation)
        {
            EndConversation();
        }
    }

    private void MoveToNode(string nodeId)
    {
        DialogueNode next = activeConversation.GetNode(nodeId);
        if (next == null)
        {
            Debug.LogWarning($"Nodo '{nodeId}' no encontrado. Se finaliza la conversacion.");
            EndConversation();
            return;
        }

        activeNode = next;
        lineIndex = 0;
        ShowCurrentLineOrChoices();
    }

    private static bool LineAllowedByFlags(DialogueLine line)
    {
        if (line == null || string.IsNullOrWhiteSpace(line.requiredFlag) || StoryState.Instance == null)
        {
            return true;
        }

        return StoryState.Instance.HasFlag(line.requiredFlag);
    }

    private static void ApplyLineEffects(DialogueLine line)
    {
        if (line == null || StoryState.Instance == null)
        {
            return;
        }

        if (line.anxietyDelta != 0f)
        {
            StoryState.Instance.AddAnxiety(line.anxietyDelta);
        }

        if (!string.IsNullOrWhiteSpace(line.setFlag))
        {
            StoryState.Instance.SetFlag(line.setFlag, true);
        }

        if (!string.IsNullOrWhiteSpace(line.addInventoryItemId))
        {
            string itemId = line.addInventoryItemId;
            string canonicalItemId = InventoryCatalog.CanonicalizeItemId(itemId);

            // Ensure the inventory catalog has a sprite for items added via dialogue.
            // Some items are granted directly from Dialogue lines (addInventoryItemId)
            // and previously were only added to InventoryState, leaving no registered
            // icon for the UI. Try to register a runtime item with a sprite if found
            // in Resources or already present in the catalog.
            if (InventoryCatalog.Instance != null)
            {
                bool hasDef = InventoryCatalog.Instance.TryGet(itemId, out InventoryItemDefinition def);
                bool hasIcon = hasDef && def != null && def.icon != null;

                if (!hasIcon)
                {
                    // Try common resource paths for item sprites
                    Sprite sprite = null;
                    string[] tryPaths = new string[] {
                        $"Sprites/{itemId}",
                        $"Sprites/{itemId.ToLowerInvariant()}",
                        $"Sprites/{canonicalItemId}",
                        $"Sprites/{canonicalItemId.ToLowerInvariant()}",
                        $"Sprites/Inventory/{canonicalItemId}",
                        $"Icons/{canonicalItemId}",
                        $"Items/{canonicalItemId}"
                    };

                    foreach (string path in tryPaths)
                    {
                        sprite = Resources.Load<Sprite>(path);
                        if (sprite != null) break;

                        Sprite[] all = Resources.LoadAll<Sprite>(path);
                        if (all != null && all.Length > 0)
                        {
                            sprite = all[0];
                            break;
                        }
                    }

                    if (sprite != null)
                    {
                        InventoryCatalog.Instance.RegisterRuntimeItem(itemId, null, null, sprite);
                        Debug.Log($"DialogueRunner: Registered runtime item '{itemId}' with sprite from Resources.");
                    }
                    else
                    {
                        // If no sprite found, register a minimal runtime entry so the catalog
                        // can at least return a display name fallback. This avoids missing
                        // definitions causing UI inconsistencies.
                        InventoryCatalog.Instance.RegisterRuntimeItem(itemId, null, null, null);
                        Debug.LogWarning($"DialogueRunner: Item '{itemId}' added via dialogue has no sprite in catalog or Resources. The inventory slot will remain empty until a sprite is assigned.");
                    }
                }
            }

            InventoryState.AddItem(itemId);
        }

        if (!string.IsNullOrWhiteSpace(line.removeInventoryItemId))
        {
            string removeItemId = line.removeInventoryItemId;
            bool removed = InventoryState.RemoveItem(removeItemId);
            if (removed)
            {
                Debug.Log($"DialogueRunner: Item '{removeItemId}' eliminado del inventario via diálogo.");
            }
        }
    }

    private static List<DialogueChoice> BuildVisibleChoices(List<DialogueChoice> allChoices)
    {
        List<DialogueChoice> visible = new List<DialogueChoice>();
        if (allChoices == null)
        {
            return visible;
        }

        float anxiety = StoryState.Instance != null ? StoryState.Instance.Anxiety : 0f;

        for (int i = 0; i < allChoices.Count; i++)
        {
            DialogueChoice choice = allChoices[i];
            if (choice == null)
            {
                continue;
            }

            if (anxiety < choice.requiredAnxietyMin || anxiety > choice.requiredAnxietyMax)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(choice.requiredFlag) && StoryState.Instance != null && !StoryState.Instance.HasFlag(choice.requiredFlag))
            {
                continue;
            }

            visible.Add(choice);
        }

        return visible;
    }

    private void EndConversation()
    {
        string endedConversationId = activeConversation != null ? activeConversation.id : string.Empty;

        IsRunning = false;
        activeConversation = null;
        activeNode = null;
        lineIndex = 0;
        currentVisibleChoices.Clear();

        if (panelUI != null)
        {
            panelUI.Hide();
        }

        ConversationEnded?.Invoke(endedConversationId);
    }
}
