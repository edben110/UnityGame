using UnityEngine;

/// <summary>
/// Representa una llave interactiva que el jugador puede recoger.
/// Al clickear, se añade al inventario y se desbloquean puertas asociadas.
/// </summary>
public class KeyItem : Interactable
{
    [Header("Tipo de llave")]
    [SerializeField] private KeyType keyType;

    [Header("Feedback")]
    [SerializeField] private string pickupMessage = "Has recogido una llave!";
    [SerializeField] private bool playSound = true;

    private bool alreadyPickedUp = false;

    private void OnValidate()
    {
        keyType = ResolveKeyTypeFromName(gameObject.name, keyType);
    }

    private void Start()
    {
        keyType = ResolveKeyTypeFromName(gameObject.name, keyType);

        // Verificar si ya fue recogida (desde el inventario persistente)
        string normalized = keyType.ToString();
        if (InventoryState.HasItem(normalized))
        {
            alreadyPickedUp = true;
            gameObject.SetActive(false);
            return;
        }
    }

    private static KeyType ResolveKeyTypeFromName(string objectName, KeyType fallback)
    {
        string normalizedName = objectName.ToLowerInvariant();

        if (normalizedName.Contains("gallery") || normalizedName.Contains("galeria"))
        {
            return KeyType.GalleryKey;
        }

        if (normalizedName.Contains("bedroom") || normalizedName.Contains("habitacion"))
        {
            return KeyType.BedroomKey;
        }

        // StudyKey antes de LibraryKey para evitar ambigüedad con "estudio"
        if (normalizedName.Contains("study") || normalizedName.Contains("estudio"))
        {
            return KeyType.StudyKey;
        }

        if (normalizedName.Contains("small") || normalizedName.Contains("pequena") || normalizedName.Contains("archivador"))
        {
            return KeyType.SmallKey;
        }

        if (normalizedName.Contains("basement") || normalizedName.Contains("north") || normalizedName.Contains("sotano"))
        {
            return KeyType.BasementKey;
        }

        if (normalizedName.Contains("singleuse") || normalizedName.Contains("desgastada") || normalizedName.Contains("onetime") || normalizedName.Contains("one_time"))
        {
            return KeyType.SingleUseKey;
        }

        if (normalizedName.Contains("lobby"))
        {
            return KeyType.LobbyKey;
        }

        return fallback;
    }

    public override void Interact()
    {
        if (alreadyPickedUp)
        {
            return;
        }

        Debug.Log($"[KeyItem] ¡Llave recogida: {keyType}!");

        string itemId = keyType.ToString();
        string displayName = KeyTypeDisplayNames.GetDisplayName(keyType);
        string description = KeyTypeDisplayNames.GetDescription(keyType);

        Sprite worldSprite = null;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            worldSprite = spriteRenderer.sprite;
        }

        if (InventoryCatalog.Instance != null && worldSprite != null)
        {
            InventoryCatalog.Instance.RegisterRuntimeItem(itemId, displayName, description, worldSprite);
        }

        InventoryNarrativeDefaults.EnsureItemRegistered(itemId);
        bool added = InventoryState.AddItem(itemId);

        // Para compatibilidad, también marcar un flag en StoryState opcional
        string flagKey = $"KeyPickedUp_{keyType}";
        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(flagKey, true);
        }

        // Feedback al jugador: mostrar diálogo en el panel existente
        if (added)
        {
            Debug.Log(pickupMessage);

            DialoguePanelUI dialoguePanel = DialoguePanelUI.Instance;
            if (dialoguePanel != null)
            {
                dialoguePanel.ShowSystemMessage($"Has recogido {displayName}.");
            }
        }
        else
        {
            Debug.Log($"[KeyItem] La llave '{itemId}' ya estaba en el inventario.");
        }

        // Destruir la llave del mundo
        Destroy(gameObject);
    }
}
