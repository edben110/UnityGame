using UnityEngine;

/// <summary>
/// Centraliza cuándo el jugador no debe poder usar clics sobre el mundo (Interactable).
/// Las UIs siguen funcionando mediante EventSystem / botones (incluido Continuar del diálogo).
/// </summary>
public static class WorldInteractionGate
{
    /// <summary>
    /// Cierra todas las interacciones de mundo con un solo clic (puertas, hotspots, PNJs vía clic).
    /// Incluye inventario abierto: el mundo no recibe clic; el inventario sí (arrastrar, etc.).
    /// </summary>
    public static bool BlocksMapPointAndClick
    {
        get
        {
            if (BlocksModalOverlaysExceptInventory)
            {
                return true;
            }

            if (InventoryUIController.Instance != null && InventoryUIController.Instance.IsInventoryPanelOpen)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Al soltar un objeto del inventario sobre el mundo, no aplicar efectos sobre colliders del mapa
    /// si hay un overlay modal (diálogo, menú de PNJ, etc.). Si solo está el inventario, el drop sigue válido.
    /// </summary>
    public static bool BlocksInventoryDragToWorldInteractables =>
        BlocksModalOverlaysExceptInventory;

    private static bool BlocksModalOverlaysExceptInventory
    {
        get
        {
            if (DialoguePanelUI.Instance != null && DialoguePanelUI.Instance.IsOpen)
            {
                return true;
            }

            if (NpcInteractionMenuUI.Instance != null && NpcInteractionMenuUI.Instance.IsOpen)
            {
                return true;
            }

            if (HotspotItemPanelUI.Instance != null && HotspotItemPanelUI.Instance.IsOpen)
            {
                return true;
            }

            if (NotificationPopup.Instance != null && NotificationPopup.Instance.IsNotificationOpen)
            {
                return true;
            }

            return false;
        }
    }
}
