using UnityEngine;

/// <summary>
/// Botón interactuable dentro de la Sala de Seguridad (Security Room).
/// Al presionarlo, desactiva el sistema de seguridad y desbloquea Door_ToNorthStreet permanentemente.
///
/// FLAGS QUE SETEA:
///   - SecuritySystemDisabled = TRUE
///   - Door_ToNorthStreetUnlocked = TRUE
///
/// PERSISTENCIA:
///   Los flags se guardan en StoryState, que persiste entre escenas y sesiones.
///
/// NARRATIVA:
///   Franz dice: "Parece que este panel controla los bloqueos de la mansión...
///   eso debería abrir el acceso al ala norte."
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class SecurityUnlockButton : Interactable
{
    [Header("Flags de desbloqueo")]
    [SerializeField] private string securityDisabledFlag = "SecuritySystemDisabled";
    [SerializeField] private string doorUnlockedFlag = "Door_ToNorthStreetUnlocked";

    [Header("Feedback narrativo")]
    [TextArea(2, 4)]
    [SerializeField] private string unlockMessage = "Parece que este panel controla los bloqueos de la mansión... eso debería abrir el acceso al ala norte.";
    [TextArea(2, 4)]
    [SerializeField] private string alreadyUnlockedMessage = "El sistema de seguridad ya está desactivado. La puerta al ala norte debería estar abierta.";

    [Header("Audio (opcional)")]
    [SerializeField] private AudioClip unlockSound;

    private bool hasBeenActivated;

    private void Start()
    {
        // Verificar si ya fue activado previamente (persistencia)
        if (StoryState.Instance != null)
        {
            hasBeenActivated = StoryState.Instance.HasFlag(securityDisabledFlag)
                            && StoryState.Instance.HasFlag(doorUnlockedFlag);
        }

        // Asegurar que tiene collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && !col.enabled)
        {
            col.enabled = true;
        }
    }

    public override void Interact()
    {
        base.Interact();

        if (hasBeenActivated)
        {
            ShowMessage(alreadyUnlockedMessage);
            return;
        }

        // Desactivar sistema de seguridad
        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(securityDisabledFlag, true);
            StoryState.Instance.SetFlag(doorUnlockedFlag, true);
        }

        hasBeenActivated = true;

        // Feedback sonoro
        if (unlockSound != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            audioSource.PlayOneShot(unlockSound);
        }

        // Feedback narrativo
        ShowMessage(unlockMessage);

        Debug.Log("[SecurityUnlockButton] ★ Sistema de seguridad DESACTIVADO. Door_ToNorthStreet desbloqueada permanentemente.");
    }

    private static void ShowMessage(string message)
    {
        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(message);
        }
        else
        {
            Debug.Log("[SecurityUnlockButton] " + message);
        }
    }
}
