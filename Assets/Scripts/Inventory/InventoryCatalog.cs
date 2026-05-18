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

        InventoryNarrativeDefaults.EnsureAllDefaultsRegistered();
        RefreshRegisteredInventoryItems();
    }

    private void Start()
    {
        RefreshRegisteredInventoryItems();
    }

    private static void RefreshRegisteredInventoryItems()
    {
        List<string> owned = InventoryState.GetItems();
        for (int i = 0; i < owned.Count; i++)
        {
            InventoryNarrativeDefaults.EnsureItemRegistered(owned[i]);
        }
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

        if (runtimeOverrides.TryGetValue(normalized, out InventoryItemDefinition runtimeExisting) && runtimeExisting != null)
        {
            MergeDefinition(runtimeExisting, ref merged);
        }

        if (lookup.TryGetValue(normalized, out InventoryItemDefinition existing) && existing != null)
        {
            MergeDefinition(existing, ref merged);
        }

        FillMissingFromNarrativeDefaults(normalized, merged);
        runtimeOverrides[normalized] = merged;
    }

    public void EnsureItemRegistered(string itemId)
    {
        InventoryNarrativeDefaults.EnsureItemRegistered(itemId);
    }

    private static void MergeDefinition(InventoryItemDefinition source, ref InventoryItemDefinition target)
    {
        if (source == null || target == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(target.displayName) || target.displayName == target.id)
        {
            if (!string.IsNullOrWhiteSpace(source.displayName))
            {
                target.displayName = source.displayName;
            }
        }

        if (string.IsNullOrWhiteSpace(target.description) && !string.IsNullOrWhiteSpace(source.description))
        {
            target.description = source.description;
        }

        if (target.icon == null && source.icon != null)
        {
            target.icon = source.icon;
        }
    }

    private static void FillMissingFromNarrativeDefaults(string normalized, InventoryItemDefinition merged)
    {
        if (string.IsNullOrWhiteSpace(merged.displayName) || merged.displayName == normalized)
        {
            merged.displayName = InventoryNarrativeDefaults.GetDefaultDisplayName(normalized);
        }

        if (string.IsNullOrWhiteSpace(merged.description))
        {
            merged.description = InventoryNarrativeDefaults.GetDefaultDescription(normalized);
        }

        if (merged.icon == null)
        {
            merged.icon = InventoryProvisionalIcons.GetForItem(normalized);
        }
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

        if (string.Equals(normalized, "gallerykey", StringComparison.Ordinal) ||
            string.Equals(normalized, "gallery_key", StringComparison.Ordinal))
        {
            return "gallerykey";
        }

        if (string.Equals(normalized, "bedroomkey", StringComparison.Ordinal) ||
            string.Equals(normalized, "bedroom_key", StringComparison.Ordinal))
        {
            return "bedroomkey";
        }

        if (string.Equals(normalized, "basementkey", StringComparison.Ordinal) ||
            string.Equals(normalized, "basement_key", StringComparison.Ordinal))
        {
            return "basementkey";
        }

        if (string.Equals(normalized, "studykey", StringComparison.Ordinal) ||
            string.Equals(normalized, "study_key", StringComparison.Ordinal))
        {
            return "studykey";
        }

        if (string.Equals(normalized, "lobbykey", StringComparison.Ordinal) ||
            string.Equals(normalized, "lobby_key", StringComparison.Ordinal))
        {
            return "lobbykey";
        }

        return normalized;
    }
}
