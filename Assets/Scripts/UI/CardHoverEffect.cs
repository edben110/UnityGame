using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RawImage cardImage;
    private Color normalColor;
    private Color hoverColor;
    private Vector3 normalScale;
    private Vector3 hoverScale;

    public void Init(Color normal)
    {
        cardImage = GetComponent<RawImage>();
        normalColor = normal;
        hoverColor = new Color(
            Mathf.Min(normal.r + 0.25f, 1f),
            Mathf.Min(normal.g + 0.15f, 1f),
            Mathf.Min(normal.b + 0.15f, 1f),
            1f
        );
        normalScale = Vector3.one;
        hoverScale = new Vector3(1.05f, 1.05f, 1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardImage != null)
            cardImage.color = hoverColor;
        transform.localScale = hoverScale;

        CursorManager.SetHand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cardImage != null)
            cardImage.color = normalColor;
        transform.localScale = normalScale;

        CursorManager.SetDefault();
    }

    private void OnDisable()
    {
        CursorManager.SetDefault();
    }
}
