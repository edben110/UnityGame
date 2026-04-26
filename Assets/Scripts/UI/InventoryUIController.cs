using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject inventoryPanelRoot;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private InventoryItemEntryUI itemEntryPrefab;
    [SerializeField] private TMP_Text emptyStateText;

    [Header("Pruebas")]
    [SerializeField] private bool seedTestItemsOnStart;
    [SerializeField] private List<string> testItemIds = new List<string>();

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    private void Awake()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }

        if (inventoryPanelRoot != null)
        {
            inventoryPanelRoot.SetActive(false);
        }

        SeedTestItems();

        Refresh();
    }

    private void OnEnable()
    {
        InventoryState.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        InventoryState.Changed -= Refresh;
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePanel);
        }
    }

    public void TogglePanel()
    {
        if (inventoryPanelRoot == null)
        {
            return;
        }

        bool next = !inventoryPanelRoot.activeSelf;
        inventoryPanelRoot.SetActive(next);
        if (next)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        ClearSpawned();

        if (itemsContainer == null || itemEntryPrefab == null)
        {
            return;
        }

        List<string> ids = InventoryState.GetItems();
        bool hasItems = ids.Count > 0;

        if (emptyStateText != null)
        {
            emptyStateText.gameObject.SetActive(!hasItems);
            emptyStateText.text = hasItems ? string.Empty : "Inventario vacio";
        }

        for (int i = 0; i < ids.Count; i++)
        {
            string itemId = ids[i];
            InventoryItemEntryUI entry = Instantiate(itemEntryPrefab, itemsContainer);
            entry.gameObject.SetActive(true);
            spawnedEntries.Add(entry.gameObject);

            string displayName = itemId;
            Sprite icon = null;

            if (InventoryCatalog.Instance != null && InventoryCatalog.Instance.TryGet(itemId, out InventoryItemDefinition definition))
            {
                displayName = string.IsNullOrWhiteSpace(definition.displayName) ? itemId : definition.displayName;
                icon = definition.icon;
            }

            entry.Setup(itemId, displayName, icon);
        }
    }

    private void SeedTestItems()
    {
        if (!seedTestItemsOnStart)
        {
            return;
        }

        for (int i = 0; i < testItemIds.Count; i++)
        {
            string itemId = testItemIds[i];
            if (string.IsNullOrWhiteSpace(itemId))
            {
                continue;
            }

            InventoryState.AddItem(itemId);
        }
    }

    private void ClearSpawned()
    {
        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            if (spawnedEntries[i] != null)
            {
                Destroy(spawnedEntries[i]);
            }
        }

        spawnedEntries.Clear();
    }
}
