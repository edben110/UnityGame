using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Script base para cada casilla (slot) del inventario RPG.
/// Controla la visualización del ícono del ítem y el highlight de selección.
/// Diseñado para ser reutilizable en una grilla 8x6 dentro de ZonaGrilla.
/// 
/// Implementa IPointerClickHandler para detectar clics del jugador.
/// El clic se propaga al controlador externo mediante el callback OnSlotClicked.
/// 
/// La selección se muestra mediante 4 bordes reales (top, bottom, left, right)
/// que se activan/desactivan individualmente para formar un marco dorado visible.
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referencias UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject selectionHighlight;

    [Header("Bordes de selección (4 lados)")]
    [SerializeField] private GameObject borderTop;
    [SerializeField] private GameObject borderBottom;
    [SerializeField] private GameObject borderLeft;
    [SerializeField] private GameObject borderRight;

    [Header("Datos")]
    [SerializeField] private int slotIndex;

    /// <summary>
    /// Callback invocado cuando el jugador hace clic en este slot.
    /// Parámetro: el índice del slot (SlotIndex).
    /// Se asigna externamente por el controlador (InventoryOverlayCanvas).
    /// </summary>
    public Action<int> OnSlotClicked;

    /// <summary>Índice del slot dentro de la grilla.</summary>
    public int SlotIndex
    {
        get => slotIndex;
        set => slotIndex = value;
    }

    /// <summary>Referencia al Image del ícono del ítem.</summary>
    public Image ItemIcon => itemIcon;

    /// <summary>Referencia al GameObject del highlight de selección (legacy, puede ser null).</summary>
    public GameObject SelectionHighlight => selectionHighlight;

    /// <summary>Indica si este slot tiene un ítem asignado (sprite visible).</summary>
    public bool HasItem => itemIcon != null && itemIcon.enabled && itemIcon.sprite != null;

    /// <summary>Devuelve el sprite actual del ítem, o null si está vacío.</summary>
    public Sprite CurrentSprite => (itemIcon != null && itemIcon.enabled) ? itemIcon.sprite : null;

    private void Awake()
    {
        EnsureReferences();

        // Estado inicial: ícono vacío/transparente, bordes desactivados
        ClearSlot();
    }

    private void OnValidate()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (itemIcon == null)
        {
            Transform iconTransform = transform.Find("ItemIcon");
            if (iconTransform != null)
            {
                itemIcon = iconTransform.GetComponent<Image>();
            }
        }

        if (selectionHighlight == null)
        {
            Transform highlightTransform = transform.Find("SelectionHighlight");
            if (highlightTransform != null)
            {
                selectionHighlight = highlightTransform.gameObject;
            }
        }

        if (borderTop == null)
        {
            Transform borderTransform = transform.Find("BorderTop");
            if (borderTransform != null)
            {
                borderTop = borderTransform.gameObject;
            }
        }

        if (borderBottom == null)
        {
            Transform borderTransform = transform.Find("BorderBottom");
            if (borderTransform != null)
            {
                borderBottom = borderTransform.gameObject;
            }
        }

        if (borderLeft == null)
        {
            Transform borderTransform = transform.Find("BorderLeft");
            if (borderTransform != null)
            {
                borderLeft = borderTransform.gameObject;
            }
        }

        if (borderRight == null)
        {
            Transform borderTransform = transform.Find("BorderRight");
            if (borderTransform != null)
            {
                borderRight = borderTransform.gameObject;
            }
        }
    }

    /// <summary>
    /// Detecta el clic del jugador sobre este slot.
    /// Propaga el evento al controlador externo mediante OnSlotClicked.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(slotIndex);
    }

    /// <summary>
    /// Cambia el cursor a mano al pasar sobre el slot.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.SetHand();
    }

    /// <summary>
    /// Restaura el cursor al salir del slot.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.SetDefault();
    }

    /// <summary>
    /// Asigna un sprite al ícono del slot.
    /// Si el sprite es null, oculta el ícono.
    /// </summary>
    /// <param name="sprite">Sprite del ítem a mostrar, o null para vaciar.</param>
    public void SetItem(Sprite sprite)
    {
        if (itemIcon == null)
        {
            return;
        }

        itemIcon.sprite = sprite;
        itemIcon.enabled = sprite != null;

        // Mantener transparencia cuando no hay sprite
        Color iconColor = itemIcon.color;
        iconColor.a = sprite != null ? 1f : 0f;
        itemIcon.color = iconColor;
    }

    /// <summary>
    /// Activa o desactiva el highlight visual de selección.
    /// Usa los 4 bordes reales (top, bottom, left, right) para formar un marco dorado.
    /// </summary>
    /// <param name="isSelected">True para mostrar selección, false para ocultarla.</param>
    public void SetSelected(bool isSelected)
    {
        // Activar/desactivar los 4 bordes reales
        if (borderTop != null) borderTop.SetActive(isSelected);
        if (borderBottom != null) borderBottom.SetActive(isSelected);
        if (borderLeft != null) borderLeft.SetActive(isSelected);
        if (borderRight != null) borderRight.SetActive(isSelected);
    }

    /// <summary>
    /// Limpia el slot: oculta ícono y desactiva highlight.
    /// </summary>
    public void ClearSlot()
    {
        SetItem(null);
        SetSelected(false);
    }
}
