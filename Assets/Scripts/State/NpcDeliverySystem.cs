using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de entregas NPC para el Capítulo 4.
/// 
/// FLUJO (basado en flujoHistoria.txt):
/// 1. Se activa cuando: CurrentChapter == 4 AND PlayerIn(LivingRoom) AND AllNpcQuestItemsFound == TRUE
/// 2. Muestra panel de decisión: "Sí, hablar con ellos" / "No todavía"
/// 3. Si acepta: NpcDeliveryMode = TRUE, el botón NPC cambia a "Entregar [ObjetoSeleccionado]"
/// 4. Validación: SelectedItem == NPC.RequiredItem
/// 5. Si coincide: diálogo contextual → entregar o no
/// 6. Si no coincide: "No creo que eso tenga relación conmigo..."
///
/// PERSISTENCIA:
/// - Entregas completadas se guardan como flags en StoryState
/// - Rechazos se guardan como PlayerRefusedNpc.[name]
/// - El sistema no se reinicia al cambiar escena
/// </summary>
public class NpcDeliverySystem : MonoBehaviour
{
    public static NpcDeliverySystem Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private string requiredChapterId = "chapter4";
    [SerializeField] private string requiredRoomId = "lobby";

    /// <summary>
    /// Mapeo NPC → Item requerido para la entrega.
    /// </summary>
    private static readonly Dictionary<string, string> NpcRequiredItems = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "ben", "libro_contabilidad" },
        { "lisa", "carpeta_evidencia" },
        { "robert", "carta_padre" },
        { "ana", "estuche_joyas" },
        { "lucas", "relicario_lucas" }
    };

    /// <summary>
    /// Mapeo NPC → ID de conversación de entrega.
    /// </summary>
    private static readonly Dictionary<string, string> NpcDeliveryConversations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "ben", "chapter4_deliver_ben_libro_contabilidad" },
        { "lisa", "chapter4_deliver_lisa_carpeta_evidencia" },
        { "robert", "chapter4_deliver_robert_carta_padre" },
        { "ana", "chapter4_deliver_ana_estuche_joyas" },
        { "lucas", "chapter4_deliver_lucas_relicario" }
    };

    private bool deliveryPromptShown;

    public bool IsDeliveryModeActive
    {
        get
        {
            return StoryState.Instance != null && StoryState.Instance.HasFlag("NpcDeliveryMode");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged += OnStateChanged;
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged += OnRoomChanged;
        }
    }

    private void OnDestroy()
    {
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged -= OnStateChanged;
        }

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= OnRoomChanged;
        }
    }

    private void OnStateChanged()
    {
        TryActivateDeliveryPrompt();
    }

    private void OnRoomChanged(string previousRoom, string newRoom)
    {
        TryActivateDeliveryPrompt();
    }

    /// <summary>
    /// Intenta activar el panel de entregas si se cumplen todas las condiciones.
    /// Condiciones: CurrentChapter == 4 AND PlayerIn(Lobby/LivingRoom) AND AllNpcQuestItemsFound AND !DeliverySystemActivated
    /// </summary>
    private void TryActivateDeliveryPrompt()
    {
        if (deliveryPromptShown)
        {
            return;
        }

        if (StoryState.Instance == null)
        {
            return;
        }

        // Ya se activó previamente
        if (StoryState.Instance.HasFlag("chapter4.npc_delivery.activated") || StoryState.Instance.HasFlag("DeliverySystemActivated"))
        {
            return;
        }

        // Verificar capítulo
        if (StoryState.Instance.CurrentChapterId != requiredChapterId)
        {
            return;
        }

        // Verificar sala
        if (RoomManager.Instance != null && !string.Equals(RoomManager.Instance.CurrentRoomId, requiredRoomId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Verificar que tiene TODOS los items de NPC
        if (!HasAllNpcQuestItems())
        {
            return;
        }

        // Lanzar el prompt
        deliveryPromptShown = true;
        StoryState.Instance.SetFlag("DeliverySystemActivated", true);

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner != null && !runner.IsRunning && runner.HasConversation("chapter4_npc_delivery_prompt"))
        {
            runner.StartConversation("chapter4_npc_delivery_prompt", "start");
            Debug.Log("[NpcDeliverySystem] ★ Activando panel de entregas NPC.");
        }
    }

    /// <summary>
    /// Verifica si el jugador tiene todos los items que los NPCs buscan.
    /// </summary>
    public bool HasAllNpcQuestItems()
    {
        foreach (KeyValuePair<string, string> pair in NpcRequiredItems)
        {
            if (!InventoryState.HasItem(pair.Value))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Verifica si un item específico corresponde a un NPC.
    /// </summary>
    public bool IsItemForNpc(string npcId, string itemId)
    {
        if (string.IsNullOrWhiteSpace(npcId) || string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        if (!NpcRequiredItems.TryGetValue(npcId, out string requiredItem))
        {
            return false;
        }

        return string.Equals(requiredItem, itemId.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtiene el ID de conversación de entrega para un NPC.
    /// </summary>
    public string GetDeliveryConversationId(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            return string.Empty;
        }

        return NpcDeliveryConversations.TryGetValue(npcId, out string conversationId) ? conversationId : string.Empty;
    }

    /// <summary>
    /// Obtiene el item requerido por un NPC.
    /// </summary>
    public string GetRequiredItemForNpc(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
        {
            return string.Empty;
        }

        return NpcRequiredItems.TryGetValue(npcId, out string itemId) ? itemId : string.Empty;
    }

    /// <summary>
    /// Verifica si la entrega a un NPC ya fue completada.
    /// </summary>
    public bool IsDeliveryCompleted(string npcId)
    {
        if (StoryState.Instance == null || string.IsNullOrWhiteSpace(npcId))
        {
            return false;
        }

        return StoryState.Instance.HasFlag($"npc.delivery.{npcId.ToLowerInvariant()}.completed");
    }

    /// <summary>
    /// Verifica si TODOS los items fueron entregados.
    /// </summary>
    public bool AreAllItemsDelivered()
    {
        foreach (string npcId in NpcRequiredItems.Keys)
        {
            if (!IsDeliveryCompleted(npcId))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Verifica si el jugador rechazó al menos un NPC.
    /// </summary>
    public bool HasRejectedAtLeastOneNpc()
    {
        if (StoryState.Instance == null)
        {
            return false;
        }

        foreach (string npcId in NpcRequiredItems.Keys)
        {
            if (StoryState.Instance.HasFlag($"PlayerRefusedNpc.{npcId}"))
            {
                return true;
            }
        }

        return false;
    }
}
