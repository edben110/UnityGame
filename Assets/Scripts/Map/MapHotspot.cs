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
        RefreshChapterVisibility();

        // Escuchar cambios de capítulo para mostrar/ocultar
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged += RefreshChapterVisibility;
        }
    }

    private void OnDestroy()
    {
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged -= RefreshChapterVisibility;
        }
    }

    /// <summary>
    /// Controla la visibilidad del hotspot según el capítulo actual.
    /// 
    /// REGLA DE PERSISTENCIA:
    ///   - requiredChapterId actúa como "capítulo MÍNIMO de activación", NO como capítulo exacto.
    ///   - Un hotspot se activa cuando el jugador alcanza su capítulo requerido.
    ///   - Una vez activado, permanece disponible en capítulos posteriores SI aún no fue recogido.
    ///   - Esto evita softlocks: objetos olvidados siguen siendo accesibles al volver.
    ///   - Si el objeto ya fue recogido (IsItemCollected), se oculta normalmente.
    /// </summary>
    private void RefreshChapterVisibility()
    {
        if (string.IsNullOrWhiteSpace(requiredChapterId) || StoryState.Instance == null)
        {
            return;
        }

        // Determinar si el capítulo actual es >= al capítulo requerido (activación mínima)
        bool chapterReached = IsCurrentChapterAtOrAfter(requiredChapterId);

        // El hotspot es visible si:
        // 1. El jugador ha alcanzado o superado el capítulo de activación
        // 2. Y el objeto aún no ha sido recogido/consumido
        bool shouldBeVisible = chapterReached;

        // Si tiene reward de item y ya fue recogido, ocultarlo
        if (HasItemReward() && IsItemCollected())
        {
            shouldBeVisible = false;
        }

        // Si fue consumido (consumeAfterUse), ocultarlo
        if (consumeAfterUse && StoryState.Instance.HasFlag(GetUsedFlag()))
        {
            shouldBeVisible = false;
        }

        // Solo ocultar el renderer, no el GameObject completo
        // (para que siga recibiendo eventos de StateChanged)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = shouldBeVisible;
        }

        // Ocultar/mostrar el collider para que no sea clickeable fuera de su capítulo
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = shouldBeVisible;
        }

        // Ocultar marcador de debug también
        if (debugMarkerInstance != null)
        {
            debugMarkerInstance.SetActive(shouldBeVisible);
        }
    }

    /// <summary>
    /// Compara el capítulo actual con el requerido usando orden numérico.
    /// Retorna true si el capítulo actual es igual o posterior al requerido.
    /// Orden: prologue < chapter1 < chapter2 < chapter3 < chapter4 < chapter5
    /// </summary>
    private static bool IsCurrentChapterAtOrAfter(string requiredChapter)
    {
        if (StoryState.Instance == null)
        {
            return false;
        }

        int currentOrder = GetChapterOrder(StoryState.Instance.CurrentChapterId);
        int requiredOrder = GetChapterOrder(requiredChapter);
        return currentOrder >= requiredOrder;
    }

    private static int GetChapterOrder(string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            return -1;
        }

        switch (chapterId.ToLowerInvariant())
        {
            case "prologue": return 0;
            case "chapter1": return 1;
            case "chapter2": return 2;
            case "chapter3": return 3;
            case "chapter4": return 4;
            case "chapter5": return 5;
            default: return -1;
        }
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

        // NUEVA LÓGICA: requiredChapterId actúa como capítulo MÍNIMO, no exacto.
        // El hotspot es interactuable si el jugador ha alcanzado o superado ese capítulo.
        if (!string.IsNullOrWhiteSpace(requiredChapterId) && !IsCurrentChapterAtOrAfter(requiredChapterId))
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

        if (StoryState.Instance != null && (hideAfterPickup || consumeAfterUse) && IsItemCollected())
        {
            StoryState.Instance.SetFlag(GetUsedFlag(), true);
            RefreshAvailability();
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
        if (StoryState.Instance == null)
        {
            return;
        }

        if (hideAfterPickup && HasItemReward() && IsItemCollected())
        {
            gameObject.SetActive(false);
            return;
        }

        if (!consumeAfterUse || !StoryState.Instance.HasFlag(GetUsedFlag()))
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

    private bool IsItemCollected()
    {
        if (!HasItemReward())
        {
            return false;
        }

        if (InventoryState.HasItem(grantItemId))
        {
            return true;
        }

        return StoryState.Instance != null && StoryState.Instance.HasFlag(GetUsedFlag());
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

        if (InventoryCatalog.Instance != null && InventoryCatalog.Instance.TryGet(grantItemId, out InventoryItemDefinition definition) && !string.IsNullOrWhiteSpace(definition.description))
        {
            return definition.description;
        }

        // Fallback: usar defaults narrativos
        return InventoryNarrativeDefaults.GetDefaultDescription(grantItemId);
    }

    private Sprite ResolveItemSprite()
    {
        string canonicalItemId = InventoryCatalog.CanonicalizeItemId(grantItemId);

        if (grantItemSprite != null)
        {
            return grantItemSprite;
        }

        if (InventoryCatalog.Instance != null && InventoryCatalog.Instance.TryGet(canonicalItemId, out InventoryItemDefinition definition) && definition.icon != null)
        {
            return definition.icon;
        }

        // Fallback: intentar cargar desde Resources
        Sprite sprite = Resources.Load<Sprite>($"Sprites/{canonicalItemId}");
        if (sprite != null)
        {
            return sprite;
        }

        // Fallback: usar el SpriteRenderer del propio hotspot si tiene uno
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            return sr.sprite;
        }

        // Fallback: buscar Image component (para hotspots basados en UI)
        UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
        if (img != null && img.sprite != null)
        {
            return img.sprite;
        }

        return null;
    }

    private string GetUsedFlag()
    {
        string id = string.IsNullOrWhiteSpace(hotspotId) ? gameObject.name : hotspotId;
        return $"hotspot.used.{id}";
    }
}
