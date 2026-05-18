using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nombres, descripciones e iconos provisionales para todos los ítems del inventario.
/// </summary>
public static class InventoryNarrativeDefaults
{
    private struct ItemDefault
    {
        public string displayName;
        public string description;
    }

    private static readonly Dictionary<string, ItemDefault> defaults =
        new Dictionary<string, ItemDefault>(StringComparer.OrdinalIgnoreCase)
        {
            // Cap 1 — Lobby
            { "foto_padre_hijo", new ItemDefault { displayName = "Foto padre e hijo", description = "Fotografía en blanco y negro de Simón joven con un hombre mayor. Al dorso: 'Padre e hijo, 1987'." } },
            { "lobby_book", new ItemDefault { displayName = "Libro de visitas", description = "Un libro con firmas antiguas. Una entrada reciente está tachada con tinta roja." } },
            { "lobby_coat", new ItemDefault { displayName = "Nota del abrigo", description = "Una nota oculta en el bolsillo del abrigo: 'No confíes en nadie que llegue antes que tú'." } },
            { "lobby_newspaper", new ItemDefault { displayName = "Periódico del puerto", description = "Periódico local con un titular sobre el incendio del puerto. La investigación fue reabierta." } },

            // Cap 2 — Estudio
            { "libro_contabilidad", new ItemDefault { displayName = "Libro de contabilidad", description = "Documentos financieros con movimientos sospechosos marcados con una 'B' roja. Podría implicar a Ben." } },
            { "foto_tablero_corcho", new ItemDefault { displayName = "Foto del tablero", description = "Fotografía del tablero de corcho: seis personas unidas por hilos rojos. Una figura tiene el rostro tapado." } },
            { "agenda_simon", new ItemDefault { displayName = "Agenda de Simón", description = "Agenda personal con citas tachadas y anotaciones en los márgenes. Alguien revisó las últimas páginas." } },

            // Cap 2 / habitación — puzzle caja
            { "puzzle_box", new ItemDefault { displayName = "Caja puzzle cerrada", description = "Caja de madera con botones en la tapa. Debes abrirla desde el inventario y resolver la secuencia correcta." } },
            { "puzzle_box_2", new ItemDefault { displayName = "Caja puzzle de la cama", description = "Caja de madera con un rompecabezas deslizante en la tapa. Ábrela desde el inventario para intentar obtener la llave de la galería." } },
            { "retrato", new ItemDefault { displayName = "Retrato", description = "quiza sea una pista para algo, pero que..." } },

            // Cap 3 — Habitación de Simón
            { "carta_inconclusa", new ItemDefault { displayName = "Carta inconclusa", description = "Carta a medio escribir: 'Si alguien lee esto, no estoy muerto. Estoy en el Ala Norte...'" } },
            { "mapa_ala_norte", new ItemDefault { displayName = "Mapa del Ala Norte", description = "Plano de la mansión con el Ala Norte rodeada en rojo. Debajo: 'Aquí termina todo. O empieza.'" } },
            { "notas_medicas", new ItemDefault { displayName = "Notas médicas", description = "Registros médicos de Simón: dosis incrementada y episodios de desorientación." } },
            { "caja_puzzle_1", new ItemDefault { displayName = "Caja puzzle", description = "Caja de madera con un mecanismo de engranajes en la tapa. Esconde algo importante." } },
            { "papeles_lisa", new ItemDefault { displayName = "Papeles de Lisa", description = "Documentos que Lisa buscaba en la habitación. Pueden cambiar cómo termina la investigación." } },

            // Cap 3 — Galería
            { "carta_padre", new ItemDefault { displayName = "Carta del padre", description = "Carta manuscrita del padre común de Robert y Simón. Fecha: 4 de julio de 1929." } },
            { "llave_pequena", new ItemDefault { displayName = "Llave pequeña", description = "Llave de bronce pequeña, envuelta en un pañuelo. Parece abrir un archivador o cajón." } },
            { "smallkey", new ItemDefault { displayName = "Llave pequeña", description = "Llave de bronce pequeña. Parece abrir un archivador del estudio." } },
            { "estuche_joyas", new ItemDefault { displayName = "Estuche de joyas", description = "Estuche de cuero con joyas familiares de Simón. Ana los reconoce al instante." } },

            // Cap 4 — Seguridad / finales
            { "relicario_lucas", new ItemDefault { displayName = "Relicario de Lucas", description = "Relicario de plata guardado en el maletín. Lucas lo buscaba desde que llegó a la mansión." } },
            { "relicario", new ItemDefault { displayName = "Relicario", description = "Relicario antiguo con una inscripción casi borrada. Pertenece a la familia de Simón." } },
            { "relicario_plata", new ItemDefault { displayName = "Relicario de plata", description = "Relicario de plata con inscripción: 'Para Lucas. Siempre.'" } },
            { "codigo_4729", new ItemDefault { displayName = "Código 4-7-2-9", description = "Secuencia numérica hallada en un cuadro abstracto. Podría ser un código de acceso." } },
            { "carpeta_evidencia", new ItemDefault { displayName = "Carpeta de evidencia", description = "Fotografías y documentos del incendio del puerto. Lisa los necesitaba para su investigación." } },
            { "diario_final", new ItemDefault { displayName = "Diario final de Simón", description = "Últimas páginas del diario: Simón investigaba algo peligroso y temía que lo retuvieran." } },
            { "singleusekey", new ItemDefault { displayName = "Llave desgastada", description = "Llave vieja y corroída. Parece que solo resistirá un giro más antes de romperse." } },

            // Llaves (KeyType → id en minúsculas)
            { "lobbykey", new ItemDefault { displayName = "Llave del lobby", description = "Llave que abre la puerta principal del lobby." } },
            { "gallerykey", new ItemDefault { displayName = "Llave de la galería", description = "Llave que abre la galería de arte de la mansión." } },
            { "bedroomkey", new ItemDefault { displayName = "Llave de la habitación", description = "Llave que abre la habitación de Simón. La obtuviste al resolver la caja puzzle." } },
            { "basementkey", new ItemDefault { displayName = "Llave del sótano", description = "Llave pesada de hierro negro. La etiqueta dice: 'Sótano'." } },
            { "studykey", new ItemDefault { displayName = "Llave del estudio", description = "Llave de latón con etiqueta gastada: 'Estudio de Simón'." } },
        };

    public static string GetDefaultDisplayName(string itemId)
    {
        if (TryResolveKeyTypeMetadata(itemId, out string keyName, out _))
        {
            return keyName;
        }

        string normalized = Normalize(itemId);
        if (defaults.TryGetValue(normalized, out ItemDefault def))
        {
            return def.displayName;
        }

        return string.IsNullOrWhiteSpace(itemId) ? "Objeto desconocido" : itemId.Replace('_', ' ');
    }

    public static string GetDefaultDescription(string itemId)
    {
        if (TryResolveKeyTypeMetadata(itemId, out _, out string keyDesc) && !string.IsNullOrWhiteSpace(keyDesc))
        {
            return keyDesc;
        }

        string normalized = Normalize(itemId);
        if (defaults.TryGetValue(normalized, out ItemDefault def))
        {
            return def.description;
        }

        return "Un objeto encontrado durante la investigación en la mansión.";
    }

    /// <summary>
    /// Registra o completa metadatos (nombre, descripción, icono provisional) en el catálogo.
    /// </summary>
    public static void EnsureItemRegistered(string itemId)
    {
        if (InventoryCatalog.Instance == null || string.IsNullOrWhiteSpace(itemId))
        {
            return;
        }

        string normalized = InventoryCatalog.CanonicalizeItemId(itemId);
        string displayName = GetDefaultDisplayName(itemId);
        string description = GetDefaultDescription(itemId);
        Sprite icon = InventoryProvisionalIcons.GetForItem(normalized);

        InventoryCatalog.Instance.RegisterRuntimeItem(normalized, displayName, description, icon);
    }

    public static void EnsureAllDefaultsRegistered()
    {
        if (InventoryCatalog.Instance == null)
        {
            return;
        }

        foreach (string itemId in defaults.Keys)
        {
            EnsureItemRegistered(itemId);
        }

        foreach (KeyType keyType in Enum.GetValues(typeof(KeyType)))
        {
            EnsureItemRegistered(keyType.ToString());
        }
    }

    private static bool TryResolveKeyTypeMetadata(string itemId, out string displayName, out string description)
    {
        displayName = null;
        description = null;

        if (!TryParseKeyType(itemId, out KeyType keyType))
        {
            return false;
        }

        displayName = KeyTypeDisplayNames.GetDisplayName(keyType);
        description = KeyTypeDisplayNames.GetDescription(keyType);
        return true;
    }

    private static string Normalize(string itemId)
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
