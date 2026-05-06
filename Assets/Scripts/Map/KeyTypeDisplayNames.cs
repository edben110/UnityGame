using System.Collections.Generic;

/// <summary>
/// Mapea KeyType a nombres legibles para mostrar en UI.
/// </summary>
public static class KeyTypeDisplayNames
{
    private static readonly Dictionary<KeyType, string> DisplayNameMap = new Dictionary<KeyType, string>()
    {
        { KeyType.LobbyKey, "Llave del Lobby" },
        { KeyType.GalleryKey, "Llave de la Galería" },
        { KeyType.BedroomKey, "Llave de la Habitación" },
        { KeyType.LibraryKey, "Llave de la Biblioteca" },
        { KeyType.BasementKey, "Llave del Sótano" }
    };

    private static readonly Dictionary<KeyType, string> DescriptionMap = new Dictionary<KeyType, string>()
    {
        { KeyType.LobbyKey, "Una llave que abre la puerta del lobby." },
        { KeyType.GalleryKey, "Una llave que abre la puerta de la galería de arte." },
        { KeyType.BedroomKey, "Una llave que abre la puerta de la habitación." },
        { KeyType.LibraryKey, "Una llave que abre la puerta de la biblioteca." },
        { KeyType.BasementKey, "Una llave que abre la puerta del sótano." }
    };

    public static string GetDisplayName(KeyType keyType)
    {
        if (DisplayNameMap.TryGetValue(keyType, out string name))
        {
            return name;
        }

        return keyType.ToString();
    }

    public static string GetDescription(KeyType keyType)
    {
        if (DescriptionMap.TryGetValue(keyType, out string desc))
        {
            return desc;
        }

        return string.Empty;
    }
}
