using UnityEngine;

/// <summary>
/// Validación robusta para la entrada al Capítulo 5.
/// 
/// El Capítulo 5 NO inicia automáticamente. Solo inicia al interactuar con Door_ToNorthStreet.
///
/// FÓRMULA DE VALIDACIÓN:
///   HasAllRequiredKeys
///   AND TalkedToNpcAtLeastOnce
///   AND (RejectedNpcAtLeastOnce OR AllNpcItemsDelivered)
///
/// LLAVES REQUERIDAS (verificadas via KeyType/RequiredKeys del Inspector):
///   - GalleryKey
///   - StudyKey
///   - BedroomKey (SimonBedroomKey)
///   - SingleUseKey
///   - SmallKey
///
/// SOCIAL:
///   - TalkedToNpcInLivingRoom >= 1
///
/// DECISIONES:
///   - RejectedNpcAtLeastOnce == TRUE  OR  AllNpcItemsDelivered == TRUE
///
/// RESULTADO AL PASAR:
///   - Fade transition
///   - Change background
///   - Start Chapter 5
///   - Show chapter panel
///   - Initialize variables
/// </summary>
public class Chapter5ValidationGate : MonoBehaviour
{
    public static Chapter5ValidationGate Instance { get; private set; }

    [Header("Llaves requeridas para Cap 5")]
    [SerializeField] private KeyType[] requiredKeysForChapter5 = new KeyType[]
    {
        KeyType.GalleryKey,
        KeyType.StudyKey,
        KeyType.BedroomKey,
        KeyType.SingleUseKey,
        KeyType.SmallKey
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Valida todas las condiciones para iniciar el Capítulo 5.
    /// Retorna true si todas las condiciones se cumplen.
    /// </summary>
    public bool ValidateChapter5Entry(out string blockReason)
    {
        blockReason = string.Empty;

        if (StoryState.Instance == null)
        {
            blockReason = "Error interno: StoryState no disponible.";
            return false;
        }

        // Verificar que estamos en chapter4
        if (StoryState.Instance.CurrentChapterId != "chapter4")
        {
            blockReason = "No es momento de ir al Ala Norte todavía.";
            return false;
        }

        // VALIDACIÓN 1: Llaves
        if (!HasAllRequiredKeys(out string missingKeyMessage))
        {
            blockReason = missingKeyMessage;
            return false;
        }

        // VALIDACIÓN 2: Social - Haber hablado con al menos 1 NPC en el capítulo actual
        if (!HasTalkedToNpcAtLeastOnce())
        {
            blockReason = "Debería hablar con alguien del grupo antes de ir al Ala Norte. Podrían tener información útil.";
            return false;
        }

        // VALIDACIÓN 3: Decisiones - Haber rechazado al menos un NPC O haber entregado todos los items
        if (!HasMadeNpcDecision())
        {
            blockReason = "Aún tengo objetos que pertenecen a los demás. Debería decidir qué hacer con ellos antes de avanzar.";
            return false;
        }

        Debug.Log("[Chapter5Gate] ★ TODAS LAS VALIDACIONES PASARON. Capítulo 5 puede iniciar.");
        return true;
    }

    /// <summary>
    /// Verifica que el jugador tiene todas las llaves requeridas.
    /// </summary>
    private bool HasAllRequiredKeys(out string missingMessage)
    {
        missingMessage = string.Empty;

        if (requiredKeysForChapter5 == null || requiredKeysForChapter5.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < requiredKeysForChapter5.Length; i++)
        {
            KeyType keyType = requiredKeysForChapter5[i];
            string itemId = keyType.ToString();

            // SingleUseKey: verificar si la tiene O si ya fue usada (consumida = válida)
            if (keyType == KeyType.SingleUseKey)
            {
                bool hasKey = InventoryState.HasItem(itemId);
                bool consumed = StoryState.Instance != null && StoryState.Instance.HasFlag("OneTimeKeyUsed");
                if (!hasKey && !consumed)
                {
                    missingMessage = $"Necesito encontrar la {KeyTypeDisplayNames.GetDisplayName(keyType).ToLower()} antes de ir al Ala Norte.";
                    return false;
                }
                continue;
            }

            if (!InventoryState.HasItem(itemId))
            {
                missingMessage = $"Necesito la {KeyTypeDisplayNames.GetDisplayName(keyType).ToLower()} para acceder al Ala Norte.";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Verifica que el jugador habló con al menos 1 NPC en el capítulo actual.
    /// </summary>
    private bool HasTalkedToNpcAtLeastOnce()
    {
        if (ChapterFlowController.Instance != null)
        {
            return ChapterFlowController.Instance.GetNpcTalkCount() >= 1;
        }

        // Fallback: verificar flags directamente
        if (StoryState.Instance == null)
        {
            return false;
        }

        string currentChapter = StoryState.Instance.CurrentChapterId;
        string[] npcIds = { "robert", "ana", "ben", "lisa", "lucas" };

        for (int i = 0; i < npcIds.Length; i++)
        {
            if (StoryState.Instance.HasFlag($"chapter.{currentChapter}.npc.talked.{npcIds[i]}"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica que el jugador ha tomado una decisión sobre los NPCs:
    /// - Ha rechazado al menos un NPC (PlayerRefusedNpc.X)
    /// - O ha entregado todos los items
    /// </summary>
    private bool HasMadeNpcDecision()
    {
        // Opción A: Todos los items entregados
        if (NpcDeliverySystem.Instance != null && NpcDeliverySystem.Instance.AreAllItemsDelivered())
        {
            return true;
        }

        // Opción B: Rechazó al menos un NPC
        if (NpcDeliverySystem.Instance != null && NpcDeliverySystem.Instance.HasRejectedAtLeastOneNpc())
        {
            return true;
        }

        // Fallback: verificar flags directamente
        if (StoryState.Instance == null)
        {
            return false;
        }

        string[] npcIds = { "ben", "lisa", "robert", "ana", "lucas" };

        // Verificar si rechazó alguno
        for (int i = 0; i < npcIds.Length; i++)
        {
            if (StoryState.Instance.HasFlag($"PlayerRefusedNpc.{npcIds[i]}"))
            {
                return true;
            }
        }

        // Verificar si entregó todos
        bool allDelivered = true;
        for (int i = 0; i < npcIds.Length; i++)
        {
            if (!StoryState.Instance.HasFlag($"npc.delivery.{npcIds[i]}.completed"))
            {
                allDelivered = false;
                break;
            }
        }

        return allDelivered;
    }

    /// <summary>
    /// Genera un resumen de estado para debug.
    /// </summary>
    public string GetValidationSummary()
    {
        bool hasKeys = HasAllRequiredKeys(out string keyMsg);
        bool hasTalked = HasTalkedToNpcAtLeastOnce();
        bool hasDecision = HasMadeNpcDecision();

        return $"[Chapter5Gate] Keys: {hasKeys} | Talked: {hasTalked} | Decision: {hasDecision} | " +
               $"Overall: {(hasKeys && hasTalked && hasDecision)}";
    }
}
