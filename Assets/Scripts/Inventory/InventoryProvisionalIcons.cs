using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asigna iconos provisionales desde Resources hasta que existan sprites definitivos por ítem.
/// </summary>
public static class InventoryProvisionalIcons
{
    private static Sprite[] objetosClaveSheet;
    private static Sprite placeholder;
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    public static Sprite GetForItem(string itemId)
    {
        string normalized = NormalizeId(itemId);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return GetPlaceholder();
        }

        if (cache.TryGetValue(normalized, out Sprite cached) && cached != null)
        {
            return cached;
        }

        Sprite resolved = ResolveSprite(normalized);
        cache[normalized] = resolved;
        return resolved;
    }

    private static Sprite ResolveSprite(string itemId)
    {
        switch (itemId)
        {
            case "puzzle_box":
            case "caja_puzzle_1":
                return FirstNonNull(
                    Load("Sprites/Puzzle/puzzle_box_icon"),
                    FirstFromSheet("Sprites/Puzzle/acertijo 1"),
                    PickObjetosClave(4));

            case "puzzle_box_2":
                return FirstNonNull(
                    Load("Sprites/Puzzle/acertijo2"),
                    FirstFromSheet("Sprites/Puzzle/acertijo2"),
                    PickObjetosClave(3));

            case "retrato":
                return FirstNonNull(
                    Load("Sprites/Puzzle/acertijo_pista"),
                    PickObjetosClave(8));

            case "foto_padre_hijo":
                return FirstNonNull(Load("Sprites/Items/Fotografia"), PickObjetosClave(0));

            case "libro_contabilidad":
                return FirstNonNull(Load("Sprites/Items/Libro_de_cuentas"), PickObjetosClave(1));

            case "carpeta_evidencia":
            case "papeles_lisa":
                return FirstNonNull(Load("Sprites/Items/Folder"), PickObjetosClave(2));

            case "carta_inconclusa":
            case "carta_padre":
                return FirstNonNull(Load("Sprites/Items/Carta"), PickObjetosClave(3));

            case "relicario_lucas":
            case "relicario":
            case "relicario_plata":
                return FirstNonNull(Load("Sprites/Items/Relicario"), PickObjetosClave(5));

            case "estuche_joyas":
                return FirstNonNull(Load("Sprites/Items/Joyas"), PickObjetosClave(6));

            case "mapa_ala_norte":
            case "lobby_newspaper":
                return PickObjetosClave(7);

            case "lobby_book":
            case "diario_final":
                return PickObjetosClave(8);

            case "lobby_coat":
            case "foto_tablero_corcho":
                return PickObjetosClave(9);

            case "codigo_4729":
            case "notas_medicas":
                return PickObjetosClave(10);

            case "bedroomkey":
                return PickObjetosClave(2);
            case "gallerykey":
                return PickObjetosClave(1);
            case "basementkey":
                return PickObjetosClave(5);
            case "studykey":
                return PickObjetosClave(3);
            case "lobbykey":
                return PickObjetosClave(0);
            case "smallkey":
                return PickObjetosClave(6);
            case "singleusekey":
                return PickObjetosClave(11);
        }

        if (TryGetKeyTypeSprite(itemId, out Sprite keySprite))
        {
            return keySprite;
        }

        return PickObjetosClave(Math.Abs(itemId.GetHashCode()) % Mathf.Max(1, GetObjetosClaveSheet().Length));
    }

    private static bool TryGetKeyTypeSprite(string itemId, out Sprite sprite)
    {
        sprite = null;
        if (!TryParseKeyType(itemId, out KeyType keyType))
        {
            return false;
        }

        sprite = PickObjetosClave((int)keyType % Mathf.Max(1, GetObjetosClaveSheet().Length));
        return sprite != null;
    }

    private static Sprite PickObjetosClave(int index)
    {
        Sprite[] sheet = GetObjetosClaveSheet();
        if (sheet == null || sheet.Length == 0)
        {
            return GetPlaceholder();
        }

        index = Mathf.Abs(index) % sheet.Length;
        return sheet[index];
    }

    private static Sprite[] GetObjetosClaveSheet()
    {
        if (objetosClaveSheet != null && objetosClaveSheet.Length > 0)
        {
            return objetosClaveSheet;
        }

        objetosClaveSheet = Resources.LoadAll<Sprite>("Sprites/Objetos_clave");
        if (objetosClaveSheet == null || objetosClaveSheet.Length == 0)
        {
            objetosClaveSheet = Resources.LoadAll<Sprite>("Sprites/Objetos_clave");
        }

        Array.Sort(objetosClaveSheet, (a, b) => string.CompareOrdinal(a.name, b.name));
        return objetosClaveSheet;
    }

    private static Sprite Load(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        Sprite[] all = Resources.LoadAll<Sprite>(path);
        return all != null && all.Length > 0 ? all[0] : null;
    }

    private static Sprite FirstFromSheet(string path)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(path);
        return all != null && all.Length > 0 ? all[0] : null;
    }

    private static Sprite FirstNonNull(params Sprite[] candidates)
    {
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] != null)
            {
                return candidates[i];
            }
        }

        return GetPlaceholder();
    }

    private static Sprite GetPlaceholder()
    {
        if (placeholder != null)
        {
            return placeholder;
        }

        placeholder = PickObjetosClave(0);
        if (placeholder != null)
        {
            return placeholder;
        }

        Texture2D tex = Texture2D.whiteTexture;
        placeholder = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return placeholder;
    }

    private static string NormalizeId(string itemId)
    {
        return InventoryCatalog.CanonicalizeItemId(itemId);
    }

    private static bool TryParseKeyType(string itemId, out KeyType keyType)
    {
        keyType = default;
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        try
        {
            keyType = (KeyType)Enum.Parse(typeof(KeyType), itemId, true);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
