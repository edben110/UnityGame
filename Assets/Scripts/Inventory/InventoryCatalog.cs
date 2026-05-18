using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCatalog : MonoBehaviour
{
    public static InventoryCatalog Instance { get; private set; }

    [SerializeField] private List<InventoryItemDefinition> items = new List<InventoryItemDefinition>();

    private readonly Dictionary<string, InventoryItemDefinition> lookup = new Dictionary<string, InventoryItemDefinition>();
    private readonly Dictionary<string, InventoryItemDefinition> runtimeOverrides = new Dictionary<string, InventoryItemDefinition>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RebuildLookup();

        // Registrar defaults narrativos para items sin sprite
        InventoryNarrativeDefaults.EnsureAllDefaultsRegistered();
    }

    private void OnValidate()
    {
        RebuildLookup();
    }

    public bool TryGet(string itemId, out InventoryItemDefinition definition)
    {
        definition = null;
        string normalized = Normalize(itemId);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        if (runtimeOverrides.TryGetValue(normalized, out InventoryItemDefinition runtimeDefinition) && runtimeDefinition != null)
        {
            definition = runtimeDefinition;
            return true;
        }

        return lookup.TryGetValue(normalized, out definition) && definition != null;
    }

    public void RegisterRuntimeItem(string itemId, string displayName, string description, Sprite icon)
    {
        string normalized = Normalize(itemId);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        InventoryItemDefinition merged = new InventoryItemDefinition
        {
            id = normalized,
            displayName = string.IsNullOrWhiteSpace(displayName) ? itemId : displayName,
            description = description ?? string.Empty,
            icon = icon
        };

        if (lookup.TryGetValue(normalized, out InventoryItemDefinition existing) && existing != null)
        {
            if (string.IsNullOrWhiteSpace(merged.displayName))
            {
                merged.displayName = existing.displayName;
            }

            if (string.IsNullOrWhiteSpace(merged.description))
            {
                merged.description = existing.description;
            }

            if (merged.icon == null)
            {
                merged.icon = existing.icon;
            }
        }

        runtimeOverrides[normalized] = merged;
    }

    public static string CanonicalizeItemId(string itemId)
    {
        return Normalize(itemId);
    }

    public string GetDisplayNameOrFallback(string itemId)
    {
        if (TryGet(itemId, out InventoryItemDefinition definition) && !string.IsNullOrWhiteSpace(definition.displayName))
        {
            return definition.displayName;
        }

        return string.IsNullOrWhiteSpace(itemId) ? "objeto" : itemId.Replace('_', ' ');
    }

    private void RebuildLookup()
    {
        lookup.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            InventoryItemDefinition entry = items[i];
            if (entry == null)
            {
                continue;
            }

            string normalized = Normalize(entry.id);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            lookup[normalized] = entry;
        }
    }

    private static string Normalize(string itemId)
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

        if (string.Equals(normalized, "lobby_photo", StringComparison.Ordinal))
        {
            return "foto_padre_hijo";
        }

        return normalized;
    }
}
