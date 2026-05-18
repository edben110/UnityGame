using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente reutilizable para cambiar el cursor a "mano" cuando el mouse
/// pasa sobre cualquier elemento UI interactuable.
///
/// Uso: Agregar este componente a cualquier GameObject con un Graphic (Image, Button, etc.)
/// que tenga raycastTarget = true.
///
/// Funciona con:
///   - Botones (Button)
///   - Slots del inventario (InventorySlotUI)
///   - Botón "I" del HUD
///   - Botón X del inventario
///   - Cualquier UI interactuable
///
/// Compatible con New Input System y EventSystem estándar de Unity.
/// No interfiere con clicks ni drag existentes.
/// </summary>
public class CursorHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.SetHand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.SetDefault();
    }

    private void OnDisable()
    {
        // Si el objeto se desactiva mientras el cursor está encima, restaurar cursor
        CursorManager.SetDefault();
    }
}
