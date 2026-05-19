using UnityEngine;

/// <summary>
/// Hitbox invisible clickeable que representa un dígito del teclado numérico.
/// Se coloca sobre el número ya dibujado en el sprite del fondo.
/// NO tiene visual propio — es solo un collider invisible.
/// 
/// Al ser clickeado, reporta al NumericPasswordPanel qué dígito fue presionado.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DigitButton : Interactable
{
    private int digitValue;
    private NumericPasswordPanel parentPanel;
    private bool initialized;

    /// <summary>
    /// Inicializa el botón con su valor y referencia al panel padre.
    /// Llamado por NumericPasswordPanel al crear los hitboxes.
    /// </summary>
    public void Initialize(int value, NumericPasswordPanel panel)
    {
        digitValue = value;
        parentPanel = panel;
        initialized = true;
    }

    public override void Interact()
    {
        if (!initialized || parentPanel == null)
        {
            Debug.LogWarning($"[DigitButton] No inicializado correctamente. Digit: {digitValue}");
            return;
        }

        if (parentPanel.IsUnlocked)
        {
            return;
        }

        parentPanel.OnDigitPressed(digitValue);
    }
}
