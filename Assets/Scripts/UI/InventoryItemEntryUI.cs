using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemEntryUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private InventoryItemDragHandler dragHandler;

    public void Setup(string itemId, string displayName, Sprite icon)
    {
        if (labelText != null)
        {
            labelText.text = string.IsNullOrWhiteSpace(displayName) ? itemId : displayName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (dragHandler != null)
        {
            dragHandler.Configure(itemId, icon);
        }
    }
}
