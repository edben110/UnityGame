using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject inventoryPanelRoot;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private InventoryItemEntryUI itemEntryPrefab;
    [SerializeField] private TMP_Text emptyStateText;
    [SerializeField] private Button backToMapButton;
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private Image detailsImage;
    [SerializeField] private TMP_Text detailsNameText;
    [SerializeField] private TMP_Text detailsDescriptionText;
    [SerializeField] private Button detailsContinueButton;

    [Header("Pruebas")]
    [SerializeField] private bool seedTestItemsOnStart;
    [SerializeField] private List<string> testItemIds = new List<string>();

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();
    private readonly Dictionary<string, InventoryItemDefinition> cachedDefinitions = new Dictionary<string, InventoryItemDefinition>();
    private string expandedItemId;

    public bool IsInventoryPanelOpen =>
        inventoryPanelRoot != null && inventoryPanelRoot.activeInHierarchy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Ensure InventoryCatalog exists in scene
        if (InventoryCatalog.Instance == null)
        {
            GameObject catalogObj = new GameObject("InventoryCatalog");
            catalogObj.AddComponent<InventoryCatalog>();
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
            if (toggleButton.GetComponent<CursorHoverUI>() == null)
            {
                toggleButton.gameObject.AddComponent<CursorHoverUI>();
            }
        }

        if (inventoryPanelRoot != null)
        {
            inventoryPanelRoot.SetActive(false);
        }

        EnsureItemsContainerLayout();
        SetDetailsVisible(false);

        if (detailsContinueButton != null)
        {
            detailsContinueButton.onClick.AddListener(HideDetailsAndReturnToInventory);
        }

        SeedTestItems();

        Refresh();
    }

    private void Start()
    {
        // Verificación tardía: si el nuevo overlay se inicializó después de OnEnable,
        // desuscribir la UI vieja y desactivar su panel visual.
        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryState.Changed -= Refresh;
            InventoryState.SelectedChanged -= OnSelectedChanged;

            if (inventoryPanelRoot != null)
            {
                inventoryPanelRoot.SetActive(false);
            }
        }
    }
    private void OnEnable()
    {
        // Si el nuevo InventoryOverlayCanvas está activo, no suscribirse a eventos
        // para evitar refrescar la UI antigua innecesariamente.
        if (InventoryOverlayCanvas.Instance != null)
        {
            // Mantener el toggle button funcional pero no refrescar la UI vieja
            if (inventoryPanelRoot != null)
            {
                inventoryPanelRoot.SetActive(false);
            }
            return;
        }

        InventoryState.Changed += Refresh;
        InventoryState.SelectedChanged += OnSelectedChanged;
        Refresh();
    }

    private void OnDisable()
    {
        InventoryState.Changed -= Refresh;
        InventoryState.SelectedChanged -= OnSelectedChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePanel);
        }
        if (backToMapButton != null)
        {
            backToMapButton.onClick.RemoveListener(ClosePanel);
        }

        if (detailsContinueButton != null)
        {
            detailsContinueButton.onClick.RemoveListener(HideDetailsAndReturnToInventory);
        }
    }

    public void TogglePanel()
    {
        // Delegar al nuevo InventoryOverlayCanvas si está disponible
        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.Toggle();
            return;
        }

        // Fallback: comportamiento antiguo (solo si el nuevo overlay no existe)
        if (inventoryPanelRoot == null)
        {
            return;
        }

        bool next = !inventoryPanelRoot.activeSelf;
        inventoryPanelRoot.SetActive(next);
        if (next && backToMapButton != null)
        {
            backToMapButton.onClick.RemoveAllListeners();
            backToMapButton.onClick.AddListener(ClosePanel);
        }
        if (next)
        {
            SetDetailsVisible(false);
            expandedItemId = string.Empty;
            Refresh();
        }
    }

    private void ClosePanel()
    {
        if (inventoryPanelRoot != null)
        {
            inventoryPanelRoot.SetActive(false);
        }
    }

    public void Refresh()
    {
        ClearSpawned();
        cachedDefinitions.Clear();

        EnsureItemsContainerLayout();

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
                displayName = !string.IsNullOrWhiteSpace(definition.displayName) ? definition.displayName : itemId.Replace('_', ' ');
                icon = definition.icon;
                cachedDefinitions[itemId] = definition;
            }
            else if (InventoryCatalog.Instance != null)
            {
                displayName = InventoryCatalog.Instance.GetDisplayNameOrFallback(itemId);
            }
            else
            {
                displayName = itemId.Replace('_', ' ');
            }

            entry.Setup(
                itemId,
                displayName,
                icon,
                OnEntryClicked,
                OnEntryViewPressed,
                OnEntrySelectPressed);

            string selected = InventoryState.GetSelectedItem();
            entry.SetSelected(!string.IsNullOrEmpty(selected) && selected == itemId);
            entry.SetExpanded(!string.IsNullOrEmpty(expandedItemId) && expandedItemId == itemId);
        }
    }

    private void OnSelectedChanged(string itemId)
    {
        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            GameObject go = spawnedEntries[i];
            if (go == null)
            {
                continue;
            }

            InventoryItemEntryUI ui = go.GetComponent<InventoryItemEntryUI>();
            if (ui == null)
            {
                continue;
            }

            ui.SetSelected(!string.IsNullOrEmpty(itemId) && ui.ItemId == itemId);
        }
    }

    private void OnEntryClicked(string itemId)
    {
        expandedItemId = expandedItemId == itemId ? string.Empty : itemId;
        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            GameObject go = spawnedEntries[i];
            if (go == null)
            {
                continue;
            }

            InventoryItemEntryUI ui = go.GetComponent<InventoryItemEntryUI>();
            if (ui == null)
            {
                continue;
            }

            ui.SetExpanded(!string.IsNullOrEmpty(expandedItemId) && ui.ItemId == expandedItemId);
        }
    }

    private void OnEntryViewPressed(string itemId)
    {
        if (!cachedDefinitions.TryGetValue(itemId, out InventoryItemDefinition definition) || definition == null)
        {
            if (detailsNameText != null)
            {
                detailsNameText.text = itemId;
            }

            if (detailsDescriptionText != null)
            {
                detailsDescriptionText.text = "Sin descripcion para este objeto.";
            }

            if (detailsImage != null)
            {
                detailsImage.sprite = null;
                detailsImage.enabled = false;
            }

            SetDetailsVisible(true);
            return;
        }

        if (detailsNameText != null)
        {
            detailsNameText.text = string.IsNullOrWhiteSpace(definition.displayName) ? itemId : definition.displayName;
        }

        if (detailsDescriptionText != null)
        {
            detailsDescriptionText.text = string.IsNullOrWhiteSpace(definition.description)
                ? "Sin descripcion para este objeto."
                : definition.description;
        }

        if (detailsImage != null)
        {
            detailsImage.sprite = definition.icon;
            detailsImage.enabled = definition.icon != null;
        }

        SetDetailsVisible(true);
    }

    private void HideDetailsAndReturnToInventory()
    {
        SetDetailsVisible(false);

        // Si el nuevo overlay está activo, no reactivar el panel viejo
        if (InventoryOverlayCanvas.Instance != null)
        {
            return;
        }

        if (inventoryPanelRoot != null)
        {
            inventoryPanelRoot.SetActive(true);
        }

        Refresh();
    }

    private void OnEntrySelectPressed(string itemId)
    {
        InventoryState.SetSelectedItem(itemId);
    }

    private void SetDetailsVisible(bool visible)
    {
        if (detailsRoot != null)
        {
            detailsRoot.SetActive(visible);
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

    private void EnsureItemsContainerLayout()
    {
        if (itemsContainer == null)
        {
            return;
        }

        GridLayoutGroup gridLayout = itemsContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            RectTransform rectTransform = itemsContainer as RectTransform;
            float width = rectTransform != null ? rectTransform.rect.width : 360f;
            float usableWidth = Mathf.Max(240f, width - 12f);

            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 1;
            gridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.spacing = new Vector2(0f, 8f);
            gridLayout.cellSize = new Vector2(usableWidth, 80f);
            gridLayout.padding = new RectOffset(6, 6, 6, 6);
        }
        else
        {
            VerticalLayoutGroup layout = itemsContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = itemsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;
            layout.padding = new RectOffset(6, 6, 6, 6);
        }

        ContentSizeFitter fitter = itemsContainer.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = itemsContainer.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform containerRectTransform = itemsContainer as RectTransform;
        if (containerRectTransform != null)
        {
            containerRectTransform.anchorMin = new Vector2(0f, 1f);
            containerRectTransform.anchorMax = new Vector2(1f, 1f);
            containerRectTransform.pivot = new Vector2(0.5f, 1f);
        }
    }
}
