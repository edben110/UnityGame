using System;
using System.Collections.Generic;
using UnityEngine;

public static class InventoryState
{
    [Serializable]
    private class StringListWrapper
    {
        public List<string> values = new List<string>();
    }

    private const string InventoryDecisionKey = "inventory.items";

    private static readonly HashSet<string> items = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static string cachedRaw = string.Empty;
    private static bool hasUnsyncedChanges;

    public static event Action Changed;

    public static bool AddItem(string itemId)
    {
        string normalized = NormalizeItemId(itemId);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        SyncFromStoryState();
        bool added = items.Add(normalized);
        if (!added)
        {
            return false;
        }

        Persist();
        Changed?.Invoke();
        return true;
    }

    public static bool RemoveItem(string itemId)
    {
        string normalized = NormalizeItemId(itemId);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        SyncFromStoryState();
        bool removed = items.Remove(normalized);
        if (!removed)
        {
            return false;
        }

        Persist();
        Changed?.Invoke();
        return true;
    }

    public static bool HasItem(string itemId)
    {
        string normalized = NormalizeItemId(itemId);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        SyncFromStoryState();
        return items.Contains(normalized);
    }

    public static List<string> GetItems()
    {
        SyncFromStoryState();
        return new List<string>(items);
    }

    public static void Clear()
    {
        SyncFromStoryState();
        if (items.Count == 0)
        {
            return;
        }

        items.Clear();
        Persist();
        Changed?.Invoke();
    }

    private static void SyncFromStoryState()
    {
        StoryState story = StoryState.Instance;
        if (story == null)
        {
            return;
        }

        if (hasUnsyncedChanges)
        {
            Persist();
            return;
        }

        string raw = story.GetDecision(InventoryDecisionKey, string.Empty) ?? string.Empty;
        if (raw == cachedRaw)
        {
            return;
        }

        items.Clear();

        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                StringListWrapper parsed = JsonUtility.FromJson<StringListWrapper>(raw);
                if (parsed != null && parsed.values != null)
                {
                    for (int i = 0; i < parsed.values.Count; i++)
                    {
                        string normalized = NormalizeItemId(parsed.values[i]);
                        if (!string.IsNullOrWhiteSpace(normalized))
                        {
                            items.Add(normalized);
                        }
                    }
                }
            }
            catch
            {
                // Si el inventario guardado no puede parsearse, se rehace al siguiente guardado.
            }
        }

        cachedRaw = raw;
    }

    private static void Persist()
    {
        StringListWrapper wrapper = new StringListWrapper();
        foreach (string item in items)
        {
            wrapper.values.Add(item);
        }

        wrapper.values.Sort(StringComparer.OrdinalIgnoreCase);
        string raw = JsonUtility.ToJson(wrapper);
        cachedRaw = raw;

        StoryState story = StoryState.Instance;
        if (story == null)
        {
            hasUnsyncedChanges = true;
            return;
        }

        hasUnsyncedChanges = false;
        story.SetDecision(InventoryDecisionKey, raw);
    }

    private static string NormalizeItemId(string itemId)
    {
        return string.IsNullOrWhiteSpace(itemId)
            ? string.Empty
            : itemId.Trim().ToLowerInvariant();
    }
}