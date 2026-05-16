using UnityEngine;

/// <summary>
/// Inicializa el sistema de cursores al arrancar el juego.
/// Establece el cursor por defecto personalizado (default_cursor.png) desde el primer frame.
///
/// USO: Agregar este componente a un GameObject en la primera escena del juego
/// (por ejemplo, al mismo objeto que tiene ClickManager o GameManager).
/// Alternativamente, se ejecuta automáticamente via [RuntimeInitializeOnLoadMethod].
/// </summary>
public class CursorInitializer : MonoBehaviour
{
    /// <summary>
    /// Se ejecuta automáticamente al cargar el juego, antes de cualquier escena.
    /// Garantiza que el cursor personalizado esté activo desde el primer frame.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        Debug.Log("[CursorInitializer] AutoInitialize() ejecutado (RuntimeInitializeOnLoadMethod.BeforeSceneLoad)");
        CursorManager.Initialize();
    }

    private void Awake()
    {
        Debug.Log("[CursorInitializer] Awake() ejecutado");
        // Redundante con AutoInitialize, pero garantiza inicialización
        // si el atributo [RuntimeInitializeOnLoadMethod] no se ejecutó por alguna razón.
        CursorManager.Initialize();
    }
}
