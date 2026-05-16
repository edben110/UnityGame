using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image sourceIcon;

    private string itemId;
    private Sprite itemSprite;
    private RectTransform ghost;
    private Canvas rootCanvas;

    public void Configure(string configuredItemId, Sprite configuredSprite)
    {
        itemId = string.IsNullOrWhiteSpace(configuredItemId) ? string.Empty : configuredItemId.Trim().ToLowerInvariant();
        itemSprite = configuredSprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null)
        {
            return;
        }

        GameObject ghostObject = new GameObject("DraggedInventoryItem");
        ghostObject.transform.SetParent(rootCanvas.transform, false);
        ghostObject.transform.SetAsLastSibling();

        ghost = ghostObject.AddComponent<RectTransform>();
        ghost.sizeDelta = new Vector2(84f, 84f);

        Image ghostImage = ghostObject.AddComponent<Image>();
        ghostImage.raycastTarget = false;
        ghostImage.sprite = itemSprite != null ? itemSprite : (sourceIcon != null ? sourceIcon.sprite : null);
        ghostImage.color = new Color(1f, 1f, 1f, 0.9f);

        UpdateGhostPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost == null)
        {
            return;
        }

        UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        TryDropOnNpc(eventData);

        if (ghost != null)
        {
            Destroy(ghost.gameObject);
            ghost = null;
        }
    }

    private void UpdateGhostPosition(PointerEventData eventData)
    {
        if (ghost == null || rootCanvas == null)
        {
            return;
        }

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        if (canvasRect == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            ghost.localPosition = localPoint;
        }
    }

    private void TryDropOnNpc(PointerEventData eventData)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        Camera activeCamera = Camera.main;
        if (activeCamera == null)
        {
            activeCamera = FindAnyObjectByType<Camera>();
        }

        if (activeCamera == null)
        {
            return;
        }

        Vector3 screenPoint = eventData.position;
        screenPoint.z = Mathf.Abs(activeCamera.transform.position.z);
        Vector2 worldPoint = activeCamera.ScreenToWorldPoint(screenPoint);

        if (WorldInteractionGate.BlocksInventoryDragToWorldInteractables)
        {
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider == null)
        {
            return;
        }

        NpcInteractable npc = hit.collider.GetComponentInParent<NpcInteractable>();
        if (npc == null)
        {
            return;
        }

        npc.OpenInteractionForItem(itemId);
    }
}
