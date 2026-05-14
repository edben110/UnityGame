using UnityEngine;
using TMPro;

/// <summary>
/// Script de depuración para monitorear el inventario y las llaves en tiempo real.
/// Adjunta a un Canvas o GameObject con TextMeshPro para ver logs en pantalla.
/// </summary>
public class InventoryDebugDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private bool showDebug = true;

    private void OnEnable()
    {
        InventoryState.Changed += RefreshDisplay;
        RefreshDisplay();
    }

    private void OnDisable()
    {
        InventoryState.Changed -= RefreshDisplay;
    }

    private void RefreshDisplay()
    {
        if (!showDebug || debugText == null)
        {
            return;
        }

        var items = InventoryState.GetItems();
        string selected = InventoryState.GetSelectedItem();

        string display = $"<b>Inventario ({items.Count} items)</b>\n";

        if (items.Count == 0)
        {
            display += "[VACIO]\n";
        }
        else
        {
            foreach (var item in items)
            {
                string marker = (item == selected) ? "→ " : "  ";
                string displayName = item;

                // Try to get display name from catalog
                if (InventoryCatalog.Instance != null && InventoryCatalog.Instance.TryGet(item, out InventoryItemDefinition def))
                {
                    displayName = !string.IsNullOrWhiteSpace(def.displayName) ? def.displayName : item;
                }

                display += $"{marker}{displayName}\n";
            }
        }

        display += $"\n<i>Selected: {(string.IsNullOrEmpty(selected) ? "[NONE]" : selected)}</i>";

        debugText.text = display;
    }
}
