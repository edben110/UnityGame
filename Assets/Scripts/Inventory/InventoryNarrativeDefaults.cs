using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Proporciona nombres y descripciones narrativas por defecto para items
/// que no tienen sprite o definición en el catálogo.
/// Basado en Historia_Completa_La_Mansion_de_Simon.txt (flujoHistoria).
/// 
/// Garantiza que NINGÚN objeto quede inutilizable por falta de sprite.
/// </summary>
public static class InventoryNarrativeDefaults
{
    private struct ItemDefault
    {
        public string displayName;
        public string description;
    }

    private static readonly Dictionary<string, ItemDefault> defaults = new Dictionary<string, ItemDefault>(System.StringComparer.OrdinalIgnoreCase)
    {
        // Cap 1 — Lobby
        { "foto_padre_hijo", new ItemDefault { displayName = "Foto padre e hijo", description = "Fotografía en blanco y negro de Simón joven con un hombre mayor. Al dorso: 'Padre e hijo, 1987'." } },
        { "lobby_book", new ItemDefault { displayName = "Libro de visitas", description = "Un libro con firmas antiguas. Una entrada reciente está tachada con tinta roja." } },
        { "lobby_coat", new ItemDefault { displayName = "Nota del abrigo", description = "Una nota oculta en el bolsillo: 'No confíes en nadie que llegue antes que tú'." } },
        { "lobby_newspaper", new ItemDefault { displayName = "Periódico del puerto", description = "Periódico local con un titular doblado: incendio en el puerto, investigación reabierta." } },

        // Cap 2 — Estudio
        { "libro_contabilidad", new ItemDefault { displayName = "Libro de contabilidad", description = "Documentos financieros con movimientos sospechosos marcados con 'B' roja. Evidencia contra Ben." } },
        { "foto_tablero_corcho", new ItemDefault { displayName = "Foto del tablero", description = "Fotografía del tablero de corcho con 6 personas conectadas por hilos rojos. Una tiene el rostro cubierto." } },

        // Cap 3 — Habitación de Simón
        { "carta_inconclusa", new ItemDefault { displayName = "Carta inconclusa", description = "Una carta a medio escribir: 'Si alguien lee esto, no estoy muerto. Estoy en el Ala Norte...'" } },
        { "mapa_ala_norte", new ItemDefault { displayName = "Mapa del Ala Norte", description = "Mapa de la mansión con el Ala Norte marcada con un círculo rojo. 'Aquí termina todo. O empieza.'" } },
        { "notas_medicas", new ItemDefault { displayName = "Notas médicas", description = "Registros médicos de Simón. Dosis incrementada, alucinaciones recurrentes." } },
        { "caja_puzzle_1", new ItemDefault { displayName = "Caja puzzle", description = "Una caja de madera con un mecanismo de rompecabezas en la tapa." } },

        // Cap 3 — Galería
        { "carta_padre", new ItemDefault { displayName = "Carta del padre", description = "Carta manuscrita del padre común de Robert y Simón. Fecha: 4 de julio de 1929." } },
        { "llave_pequena", new ItemDefault { displayName = "Llave pequeña", description = "Una llave de bronce pequeña. Parece ser la llave de un archivador." } },
        { "smallkey", new ItemDefault { displayName = "Llave pequeña", description = "Una llave de bronce pequeña. Parece ser la llave de un archivador." } },
        { "estuche_joyas", new ItemDefault { displayName = "Estuche de joyas", description = "Estuche de cuero con joyas familiares de Simón. Lo que Ana vino a buscar." } },
        { "basementkey", new ItemDefault { displayName = "Llave del sótano", description = "Una llave pesada de hierro negro con una etiqueta desgastada: 'Sótano'." } },

        // Cap 4 — Sala de Seguridad
        { "relicario_plata", new ItemDefault { displayName = "Relicario de plata", description = "Relicario antiguo con inscripción: 'Para Lucas. Siempre.'" } },
        { "codigo_4729", new ItemDefault { displayName = "Código 4-7-2-9", description = "Secuencia numérica encontrada en un cuadro abstracto. Podría ser un código de acceso." } },
        { "carpeta_evidencia", new ItemDefault { displayName = "Carpeta con evidencia", description = "Fotografías y documentos del incendio del puerto. La evidencia que Lisa vino a buscar." } },

        // Llaves del sistema KeyType
        { "00000000", new ItemDefault { displayName = "Llave del lobby", description = "Llave que permite acceder al lobby." } },
        { "01000000", new ItemDefault { displayName = "Llave de la galería", description = "Llave que permite acceder a la galería de arte." } },
        { "02000000", new ItemDefault { displayName = "Llave de la habitación", description = "Llave que permite acceder a la habitación de Simón." } },
        { "03000000", new ItemDefault { displayName = "Llave del Ala Norte", description = "Llave que permite acceder al pasillo del Ala Norte." } },
        { "04000000", new ItemDefault { displayName = "Llave del estudio", description = "Llave de latón con etiqueta gastada: 'Estudio de Simón'." } },
        { "05000000", new ItemDefault { displayName = "Llave del sótano", description = "Llave pesada de hierro negro. Etiqueta: 'Sótano'." } },
        { "06000000", new ItemDefault { displayName = "Llave pequeña", description = "Llave de bronce pequeña para un archivador." } },
    };

    /// <summary>
    /// Obtiene el nombre narrativo por defecto de un item.
    /// </summary>
    public static string GetDefaultDisplayName(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return "Objeto desconocido";
        }

        string normalized = itemId.Trim().ToLowerInvariant();
        if (defaults.TryGetValue(normalized, out ItemDefault def))
        {
            return def.displayName;
        }

        // Fallback: convertir ID a nombre legible
        return itemId.Replace('_', ' ').Replace('-', ' ');
    }

    /// <summary>
    /// Obtiene la descripción narrativa por defecto de un item.
    /// </summary>
    public static string GetDefaultDescription(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return "Un objeto misterioso.";
        }

        string normalized = itemId.Trim().ToLowerInvariant();
        if (defaults.TryGetValue(normalized, out ItemDefault def))
        {
            return def.description;
        }

        return "Un objeto encontrado durante la investigación.";
    }

    /// <summary>
    /// Registra todos los defaults en el InventoryCatalog como runtime items.
    /// Llamar al inicio del juego para garantizar que todos los items tengan metadata.
    /// </summary>
    public static void EnsureAllDefaultsRegistered()
    {
        if (InventoryCatalog.Instance == null)
        {
            return;
        }

        foreach (var pair in defaults)
        {
            if (!InventoryCatalog.Instance.TryGet(pair.Key, out _))
            {
                InventoryCatalog.Instance.RegisterRuntimeItem(
                    pair.Key,
                    pair.Value.displayName,
                    pair.Value.description,
                    null);
            }
        }
    }
}
