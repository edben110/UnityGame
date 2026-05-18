using UnityEngine;

/// <summary>
/// Script de limpieza que se ejecuta automáticamente al cargar la escena.
/// Elimina el NumericPasswordPanel duplicado de Door_ToSecurityRoom y limpia
/// el DigitButton incorrecto del PasswordPanel standalone.
/// 
/// El sistema de contraseña ahora vive exclusivamente en el objeto "Panel" / "PasswordPanel".
/// Este script puede eliminarse una vez que la escena sea guardada limpia.
/// </summary>
public static class PasswordPanelMigrationCleanup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Cleanup()
    {
        // Buscar Door_ToSecurityRoom y eliminar NumericPasswordPanel si existe
        GameObject door = GameObject.Find("Door_ToSecurityRoom");
        if (door != null)
        {
            NumericPasswordPanel oldPanel = door.GetComponent<NumericPasswordPanel>();
            if (oldPanel != null)
            {
                Object.Destroy(oldPanel);
                Debug.Log("[MigrationCleanup] NumericPasswordPanel ELIMINADO de Door_ToSecurityRoom.");
            }
        }

        // Buscar el panel standalone y limpiar DigitButton incorrecto
        NumericPasswordPanel[] panels = Object.FindObjectsByType<NumericPasswordPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var panel in panels)
        {
            if (panel.gameObject == door)
            {
                continue; // Skip the one we're destroying
            }

            // Renombrar si se llama "Panel"
            if (panel.gameObject.name == "Panel")
            {
                panel.gameObject.name = "PasswordPanel";
                Debug.Log("[MigrationCleanup] Panel renombrado a 'PasswordPanel'.");
            }

            // Eliminar DigitButton del panel padre si existe (solo debe estar en hijos)
            DigitButton wrongButton = panel.GetComponent<DigitButton>();
            if (wrongButton != null)
            {
                Object.Destroy(wrongButton);
                Debug.Log("[MigrationCleanup] DigitButton incorrecto eliminado del PasswordPanel.");
            }
        }
    }
}
