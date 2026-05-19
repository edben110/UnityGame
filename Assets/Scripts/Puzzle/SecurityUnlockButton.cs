using UnityEngine;

/// <summary>
/// Botón interactuable dentro de la Sala de Seguridad (Security Room).
/// Al presionarlo, desbloquea Door_ToNorthStreet permanentemente.
///
/// FLAG QUE SETEA:
///   - UnlockNorthStreetDoor = TRUE
///
/// PERSISTENCIA:
///   El flag se guarda en StoryState, que persiste entre escenas y sesiones.
///
/// IMPORTANTE:
///   Este botón NO inicia el Capítulo 5 automáticamente.
///   Solo desbloquea físicamente la puerta.
///   Las validaciones narrativas del Cap 5 siguen siendo obligatorias.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class SecurityUnlockButton : Interactable
{
    [Header("Flag de desbloqueo")]
    [SerializeField] private string doorUnlockedFlag = "UnlockNorthStreetDoor";

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
            hasBeenActivated = StoryState.Instance.HasFlag(doorUnlockedFlag);
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

        // Desbloquear Door_ToNorthStreet permanentemente
        if (StoryState.Instance != null)
        {
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

        Debug.Log("[SecurityUnlockButton] ★ Door_ToNorthStreet desbloqueada permanentemente (UnlockNorthStreetDoor = TRUE).");
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
