using UnityEngine;

/// <summary>
/// Singleton centralizado para gestionar el cambio visual del cursor.
/// 
/// Uso:
///   CursorManager.SetHand()    → cursor tipo mano (interacción)
///   CursorManager.SetDefault() → cursor flecha personalizado
///
/// Carga las texturas desde Resources/Cursors/:
///   - hand_cursor.png    → cursor de mano para hover sobre interactuables
///   - default_cursor.png → cursor de flecha personalizado (reemplaza el del SO)
///
/// ACCIÓN MANUAL REQUERIDA:
/// En Unity Inspector, seleccionar ambos PNG y configurar:
///   - Texture Type: Cursor
///   - Max Size: 32
///   - Apply
///
/// Compatible con New Input System y Unity UI EventSystem.
/// </summary>
public static class CursorManager
{
    private static bool isHand;
    private static Texture2D handCursorTexture;
    private static Texture2D defaultCursorTexture;
    private static bool texturesLoaded;

    /// <summary>
    /// Inicializa el sistema de cursores cargando las texturas y aplicando el cursor por defecto.
    /// Debe llamarse una vez al inicio del juego (desde un MonoBehaviour temprano).
    /// Se llama automáticamente en el primer uso de SetHand() o SetDefault() si no se invocó antes.
    /// </summary>
    public static void Initialize()
    {
        EnsureTexturesLoaded();
        Debug.Log($"[CursorManager] Initialize() ejecutado. default={defaultCursorTexture != null}, hand={handCursorTexture != null}");
        ApplyDefaultCursor();
    }

    /// <summary>
    /// Cambia el cursor al estilo "mano" (interactivo).
    /// </summary>
    public static void SetHand()
    {
        if (isHand) return;
        isHand = true;

        EnsureTexturesLoaded();
        Debug.Log($"[CursorManager] SetHand() llamado. handTexture={handCursorTexture != null}");

        if (handCursorTexture != null)
        {
            Cursor.SetCursor(handCursorTexture, new Vector2(6f, 0f), CursorMode.Auto);
        }
    }

    /// <summary>
    /// Restaura el cursor a la flecha personalizada (default_cursor.png).
    /// </summary>
    public static void SetDefault()
    {
        if (!isHand) return;
        isHand = false;

        Debug.Log($"[CursorManager] SetDefault() llamado. defaultTexture={defaultCursorTexture != null}");
        ApplyDefaultCursor();
    }

    /// <summary>
    /// Devuelve true si el cursor está actualmente en modo "mano".
    /// </summary>
    public static bool IsHand => isHand;

    /// <summary>
    /// Fuerza la restauración del cursor al estado por defecto.
    /// Útil al cambiar de escena o al desactivar sistemas.
    /// </summary>
    public static void ForceReset()
    {
        isHand = false;
        ApplyDefaultCursor();
    }

    /// <summary>
    /// Aplica la textura del cursor por defecto (flecha personalizada).
    /// </summary>
    private static void ApplyDefaultCursor()
    {
        EnsureTexturesLoaded();

        if (defaultCursorTexture != null)
        {
            Debug.Log($"[CursorManager] ApplyDefaultCursor() — aplicando textura '{defaultCursorTexture.name}' ({defaultCursorTexture.width}x{defaultCursorTexture.height})");
            Cursor.SetCursor(defaultCursorTexture, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Debug.Log("[CursorManager] ApplyDefaultCursor() — textura null, usando cursor del SO");
            // Fallback al cursor del SO si no se encontró la textura
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    /// <summary>
    /// Carga ambas texturas desde Resources/Cursors/.
    /// Solo se ejecuta una vez.
    /// </summary>
    private static void EnsureTexturesLoaded()
    {
        if (texturesLoaded) return;
        texturesLoaded = true;

        handCursorTexture = Resources.Load<Texture2D>("Cursors/hand_cursor");
        defaultCursorTexture = Resources.Load<Texture2D>("Cursors/default_cursor");

        // Fallback: intentar nombres alternativos si los principales no se encontraron
        if (handCursorTexture == null)
        {
            handCursorTexture = Resources.Load<Texture2D>("Cursors/Link-select");
        }
        if (defaultCursorTexture == null)
        {
            defaultCursorTexture = Resources.Load<Texture2D>("Cursors/Normal-select");
        }

        Debug.Log($"[CursorManager] EnsureTexturesLoaded() — hand={handCursorTexture != null} ({(handCursorTexture != null ? handCursorTexture.name : "NULL")}), default={defaultCursorTexture != null} ({(defaultCursorTexture != null ? defaultCursorTexture.name : "NULL")})");

        if (handCursorTexture == null)
        {
            Debug.LogWarning(
                "[CursorManager] No se encontró 'Resources/Cursors/hand_cursor'.\n" +
                "El cursor de mano no se mostrará. Importa hand_cursor.png (32x32, Texture Type: Cursor).");
        }

        if (defaultCursorTexture == null)
        {
            Debug.LogWarning(
                "[CursorManager] No se encontró 'Resources/Cursors/default_cursor'.\n" +
                "Se usará el cursor del SO como fallback. Importa default_cursor.png (32x32, Texture Type: Cursor).");
        }
    }
}
