using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registro de ítems del inventario que se activan al hacer clic en la grilla.
/// </summary>
public static class InventoryUsableItems
{
    private static readonly Dictionary<string, Action> handlers =
        new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

    static InventoryUsableItems()
    {
        Register(AcertijoPuzzleService.PuzzleBoxItemId, OpenBedroomPuzzleBox);
        Register(Acertijo2PuzzleService.PuzzleBoxItemId, OpenGalleryPuzzleBox);
        Register("retrato", ShowRetratoHint);
    }

    public static void Register(string itemId, Action onUse)
    {
        string normalized = Normalize(itemId);
        if (string.IsNullOrWhiteSpace(normalized) || onUse == null)
        {
            return;
        }

        handlers[normalized] = onUse;
    }

    public static bool TryUse(string itemId)
    {
        string normalized = Normalize(itemId);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        if (!handlers.TryGetValue(normalized, out Action handler) || handler == null)
        {
            return false;
        }

        handler.Invoke();
        return true;
    }

    public static bool IsUsable(string itemId)
    {
        string normalized = Normalize(itemId);
        return !string.IsNullOrWhiteSpace(normalized) && handlers.ContainsKey(normalized);
    }

    private static void OpenBedroomPuzzleBox()
    {
        if (AcertijoPuzzleService.Instance == null)
        {
            GameObject host = new GameObject(nameof(AcertijoPuzzleService));
            host.AddComponent<AcertijoPuzzleService>();
        }

        AcertijoPuzzleService.Instance.OpenFromInventory();
    }

    private static void OpenGalleryPuzzleBox()
    {
        if (Acertijo2PuzzleService.Instance == null)
        {
            GameObject host = new GameObject(nameof(Acertijo2PuzzleService));
            host.AddComponent<Acertijo2PuzzleService>();
        }

        Acertijo2PuzzleService.Instance.OpenFromInventory();
    }

    private static void ShowRetratoHint()
    {
        InventoryHintOverlay.ShowHintFromResources("Sprites/Puzzle/acertijo_pista");
    }

    private static string Normalize(string itemId)
    {
        return string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim().ToLowerInvariant();
    }
}
