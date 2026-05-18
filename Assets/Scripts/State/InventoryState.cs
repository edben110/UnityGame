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
    public static event Action<string> SelectedChanged;

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

        InventoryNarrativeDefaults.EnsureItemRegistered(normalized);

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

        if (string.Equals(selectedItem, normalized, StringComparison.OrdinalIgnoreCase))
        {
            selectedItem = string.Empty;
            SelectedChanged?.Invoke(selectedItem);
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

    private static string selectedItem;

    public static string GetSelectedItem()
    {
        return string.IsNullOrWhiteSpace(selectedItem) ? string.Empty : selectedItem;
    }

    public static string CurrentSelectedInventoryItem => GetSelectedItem();

    public static string CurrentlySelectedInventoryItem => GetSelectedItem();

    public static void SetSelectedItem(string itemId)
    {
        string normalized = NormalizeItemId(itemId);
        if (string.Equals(normalized, selectedItem)) return;
        selectedItem = normalized;
        SelectedChanged?.Invoke(selectedItem);
    }

    public static void ClearSelectedItem()
    {
        if (string.IsNullOrWhiteSpace(selectedItem)) return;
        selectedItem = string.Empty;
        SelectedChanged?.Invoke(selectedItem);
    }

    public static void Clear()
    {
        SyncFromStoryState();
        if (items.Count == 0)
        {
            return;
        }

        items.Clear();
        if (!string.IsNullOrWhiteSpace(selectedItem))
        {
            selectedItem = string.Empty;
            SelectedChanged?.Invoke(selectedItem);
        }
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
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return string.Empty;
        }

        string normalized = itemId.Trim().ToLowerInvariant();

        if (string.Equals(normalized, "hs_unfinished_letter", StringComparison.Ordinal) ||
            string.Equals(normalized, "unfinished_letter", StringComparison.Ordinal))
        {
            return "carta_inconclusa";
        }

        // Equivalence for the small key to match enum SmallKey string representation
        if (string.Equals(normalized, "llave_pequena", StringComparison.Ordinal) ||
            string.Equals(normalized, "llavepequena", StringComparison.Ordinal))
        {
            return "smallkey";
        }

        // Equivalence for the single use key
        if (string.Equals(normalized, "llave_desgastada", StringComparison.Ordinal) ||
            string.Equals(normalized, "llave_un_solo_uso", StringComparison.Ordinal) ||
            string.Equals(normalized, "one_time_key", StringComparison.Ordinal))
        {
            return "singleusekey";
        }

        // Backward compatibility: versiones anteriores guardaban esta pista como "lobby_photo".
        if (string.Equals(normalized, "lobby_photo", StringComparison.Ordinal))
        {
            return "foto_padre_hijo";
        }

        return normalized;
    }
}