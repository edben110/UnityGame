using UnityEngine;

public class MapHotspot : Interactable
{
    private static Sprite debugSprite;

    [Header("Identidad")]
    [SerializeField] private string hotspotId;

    [Header("Flujo narrativo")]
    [SerializeField] private string requiredChapterId = "chapter1";
    [SerializeField] private string conversationId;
    [SerializeField] private string startNodeId = "start";

    [Header("Estado")]
    [SerializeField] private bool consumeAfterUse = false;
    [SerializeField] private string setFlagOnInteract;

    [Header("Item opcional")]
    [SerializeField] private string grantItemId;
    [SerializeField] private string grantItemDisplayName;
    [TextArea(2, 5)] [SerializeField] private string grantItemDescription;
    [SerializeField] private Sprite grantItemSprite;
    [SerializeField] private bool hideAfterPickup = true;

    [Header("Depuracion")]
    [SerializeField] private bool showDebugMarkerInGame;
    [SerializeField] private Color debugMarkerColor = new Color(1f, 1f, 0f, 0.65f);
    [SerializeField] private Vector2 debugMarkerSize = new Vector2(0.9f, 0.9f);

    private bool pendingPickup;
    private GameObject debugMarkerInstance;

    private void Start()
    {
        EnsureDebugMarker();
        RefreshAvailability();
    }

    private void HideAnxietyOverlayIfVisible()
    {
        AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
        if (anxietySystem != null)
        {
            anxietySystem.HideVerificationOverlay();
        }
    }

    
public override void Interact()
    {
        base.Interact();
        HideAnxietyOverlayIfVisible();

        if (StoryState.Instance == null)
        {
            Debug.LogWarning("MapHotspot no puede continuar sin StoryState.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredChapterId) && StoryState.Instance.CurrentChapterId != requiredChapterId)
        {
            return;
        }

        if (consumeAfterUse && StoryState.Instance.HasFlag(GetUsedFlag()))
        {
            return;
        }

        if (HasItemReward())
        {
            OpenItemPanelOrPickupImmediately();
            return;
        }

        TriggerNarrativeConversation();
    }

    private void OpenItemPanelOrPickupImmediately()
    {
        if (HotspotItemPanelUI.Instance == null)
        {
            PickupItem();
            return;
        }

        HotspotItemPanelUI.Instance.Show(
            gameObject.name,
            ResolveItemDisplayName(),
            ResolveItemDescription(),
            ResolveItemSprite(),
            PickupItem);
    }

    private void PickupItem()
    {
        if (pendingPickup)
        {
            return;
        }

        pendingPickup = true;

        if (HasItemReward())
        {
            if (InventoryCatalog.Instance != null)
            {
                InventoryCatalog.Instance.RegisterRuntimeItem(
                    grantItemId,
                    ResolveItemDisplayName(),
                    ResolveItemDescription(),
                    ResolveItemSprite());
            }

            InventoryState.AddItem(grantItemId);
        }

        TriggerNarrativeConversation();

        if (HotspotItemPanelUI.Instance != null)
        {
            HotspotItemPanelUI.Instance.Hide();
        }

        pendingPickup = false;
    }

    private void TriggerNarrativeConversation()
    {
        if (!string.IsNullOrWhiteSpace(setFlagOnInteract))
        {
            StoryState.Instance.SetFlag(setFlagOnInteract, true);
        }

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner != null && !runner.IsRunning && !string.IsNullOrWhiteSpace(conversationId))
        {
            runner.StartConversation(conversationId, string.IsNullOrWhiteSpace(startNodeId) ? "start" : startNodeId);
        }

        if (consumeAfterUse)
        {
            StoryState.Instance.SetFlag(GetUsedFlag(), true);
            RefreshAvailability();
        }
    }

    private void RefreshAvailability()
    {
        if (!consumeAfterUse || StoryState.Instance == null || !StoryState.Instance.HasFlag(GetUsedFlag()))
        {
            return;
        }

        if (hideAfterPickup)
        {
            gameObject.SetActive(false);
            return;
        }

        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
    }

    private void EnsureDebugMarker()
    {
        if (!showDebugMarkerInGame)
        {
            return;
        }

        if (GetComponent<SpriteRenderer>() != null)
        {
            // Si ya hay un sprite real, no sobreponer marcador de debug.
            return;
        }

        if (debugMarkerInstance != null)
        {
            return;
        }

        debugMarkerInstance = new GameObject("HotspotDebugMarker");
        debugMarkerInstance.transform.SetParent(transform, false);
        debugMarkerInstance.transform.localPosition = Vector3.zero;
        debugMarkerInstance.transform.localScale = new Vector3(debugMarkerSize.x, debugMarkerSize.y, 1f);

        SpriteRenderer renderer = debugMarkerInstance.AddComponent<SpriteRenderer>();
        renderer.sprite = GetDebugSprite();
        renderer.color = debugMarkerColor;
        renderer.sortingOrder = 150;
    }

    private static Sprite GetDebugSprite()
    {
        if (debugSprite != null)
        {
            return debugSprite;
        }

        Rect rect = new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height);
        debugSprite = Sprite.Create(Texture2D.whiteTexture, rect, new Vector2(0.5f, 0.5f), 100f);
        return debugSprite;
    }

    private bool HasItemReward()
    {
        return !string.IsNullOrWhiteSpace(grantItemId);
    }

    private string ResolveItemDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(grantItemDisplayName))
        {
            return grantItemDisplayName;
        }

        if (InventoryCatalog.Instance != null)
        {
            return InventoryCatalog.Instance.GetDisplayNameOrFallback(grantItemId);
        }

        return grantItemId;
    }

    private string ResolveItemDescription()
    {
        if (!string.IsNullOrWhiteSpace(grantItemDescription))
        {
            return grantItemDescription;
        }

        if (InventoryCatalog.Instance != null && InventoryCatalog.Instance.TryGet(grantItemId, out InventoryItemDefinition definition))
        {
            return definition.description;
        }

        return string.Empty;
    }

    private Sprite ResolveItemSprite()
    {
        if (grantItemSprite != null)
        {
            return grantItemSprite;
        }

        if (InventoryCatalog.Instance != null && InventoryCatalog.Instance.TryGet(grantItemId, out InventoryItemDefinition definition))
        {
            return definition.icon;
        }

        return null;
    }

    private string GetUsedFlag()
    {
        string id = string.IsNullOrWhiteSpace(hotspotId) ? gameObject.name : hotspotId;
        return $"hotspot.used.{id}";
    }
}
