using UnityEngine;

/// <summary>
/// Botón de dígito del teclado numérico (colocado en escena sobre cada número).
/// Debe tener parentPanel asignado al NumericPasswordPanel de Door_ToSecurityRoom.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DigitButton : Interactable
{
    [SerializeField] private int digitValue = 1;
    [SerializeField] private NumericPasswordPanel parentPanel;

    public int DigitValue => digitValue;

    private void Awake()
    {
        digitValue = Mathf.Clamp(digitValue, 1, 9);
    }

    public void BindToPanel(NumericPasswordPanel panel)
    {
        parentPanel = panel;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        digitValue = Mathf.Clamp(digitValue, 1, 9);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(transform.position, col.bounds.size);
    }
#endif

    public override void Interact()
    {
        if (parentPanel == null)
        {
            Debug.LogWarning($"[DigitButton] Sin panel asignado. Dígito: {digitValue}");
            return;
        }

        if (parentPanel.IsUnlocked || !parentPanel.IsPanelShowing)
        {
            return;
        }

        parentPanel.TryRegisterDigit(digitValue);
    }
}
