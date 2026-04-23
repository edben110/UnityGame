using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text label;
    private Color normalColor;
    private Color hoverColor;
    private float normalSize;
    private float hoverSize;

    public void Init(Color normal, Color hover, float baseFontSize)
    {
        label = GetComponentInChildren<TMP_Text>(true);
        normalColor = normal;
        hoverColor = hover;
        normalSize = baseFontSize;
        hoverSize = baseFontSize + 3;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (label == null) return;
        label.color = hoverColor;
        label.fontSize = hoverSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (label == null) return;
        label.color = normalColor;
        label.fontSize = normalSize;
    }
}
