using System.Collections.Generic;
using UnityEngine;

public class InventoryCatalog : MonoBehaviour
{
    public static InventoryCatalog Instance { get; private set; }

    [SerializeField] private List<InventoryItemDefinition> items = new List<InventoryItemDefinition>();

    private readonly Dictionary<string, InventoryItemDefinition> lookup = new Dictionary<string, InventoryItemDefinition>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RebuildLookup();
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

        return lookup.TryGetValue(normalized, out definition) && definition != null;
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
        return string.IsNullOrWhiteSpace(itemId)
            ? string.Empty
            : itemId.Trim().ToLowerInvariant();
    }
}
