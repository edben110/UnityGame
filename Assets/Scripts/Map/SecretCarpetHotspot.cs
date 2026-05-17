using UnityEngine;

/// <summary>
/// PROBLEMA 3 – Primer descubrimiento de puerta secreta.
///
/// Hotspot especial para la alfombra secreta del sótano.
/// 
/// FLUJO:
///   1. Primer click en la alfombra:
///      → Protagonista dice: "La alfombra posee una forma extraña…"
///      → Setea flag carpet.first_look
///   
///   2. Segundo click (o inmediato si ya se exploró):
///      → Se revela la puerta secreta (flag BasementDiscovered)
///      → DoorTrigger activa su collider
///   
///   3. Si el jugador intenta bajar sin condiciones:
///      → "No puedo bajar por ahí todavía… debería hablar con los demás primero."
/// 
/// RESTRICCIÓN: Solo funciona en chapter3
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SecretCarpetHotspot : Interactable
{
    [Header("Configuración")]
    [SerializeField] private string requiredChapterId = "chapter3";
    [SerializeField] private GameObject carpetObject;
    [SerializeField] private GameObject basementDoorObject;

    [Header("Mensajes narrativos")]
    [SerializeField] private string firstLookMessage = "La alfombra posee una forma extraña…";
    [SerializeField] private string revealMessage = "Hay una puerta secreta debajo. ¿Qué habrá ahí abajo?";
    [SerializeField] private string blockedMessage = "No puedo bajar por ahí todavía… debería hablar con los demás primero.";
    [SerializeField] private string alreadyDiscoveredMessage = "Una puerta secreta que lleva al sótano. Necesito la llave para abrirla.";

    private const string FirstLookFlag = "carpet.first_look";
    private const string BasementDiscoveredFlag = "BasementDiscovered";

    private void Start()
    {
        // Ocultar la puerta del sótano hasta que sea descubierta
        RefreshBasementDoorVisibility();

        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged += OnStateChanged;
        }

        // Bloquear interacción si no es chapter3
        RefreshCollider();
    }

    private void OnDestroy()
    {
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged()
    {
        RefreshCollider();
        RefreshBasementDoorVisibility();
    }

    private void RefreshCollider()
    {
        if (StoryState.Instance == null)
        {
            return;
        }

        // PROBLEMA 2: Completamente inaccesible antes de chapter3
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            bool isChapter3 = StoryState.Instance.CurrentChapterId == requiredChapterId;
            col.enabled = isChapter3;
        }
    }

    private void RefreshBasementDoorVisibility()
    {
        if (basementDoorObject == null || StoryState.Instance == null)
        {
            return;
        }

        bool discovered = StoryState.Instance.HasFlag(BasementDiscoveredFlag);
        basementDoorObject.SetActive(discovered);
    }

    public override void Interact()
    {
        base.Interact();

        if (StoryState.Instance == null)
        {
            return;
        }

        // Validación de capítulo
        if (StoryState.Instance.CurrentChapterId != requiredChapterId)
        {
            return;
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;

        // Ya fue descubierto el sótano
        if (StoryState.Instance.HasFlag(BasementDiscoveredFlag))
        {
            if (panel != null)
            {
                panel.ShowSystemMessage(alreadyDiscoveredMessage);
            }
            return;
        }

        // Primer click – observación inicial
        if (!StoryState.Instance.HasFlag(FirstLookFlag))
        {
            StoryState.Instance.SetFlag(FirstLookFlag, true);

            if (panel != null)
            {
                panel.ShowSystemMessage(firstLookMessage, panel.Hide);
            }
            return;
        }

        // Ya observó pero no descubrió: revelar el sótano
        RevealBasement();
    }

    private void RevealBasement()
    {
        if (StoryState.Instance == null)
        {
            return;
        }

        // Marcar como descubierto
        StoryState.Instance.SetFlag(BasementDiscoveredFlag, true);
        Debug.Log("[SecretCarpetHotspot] ★ Sótano descubierto. Flag 'BasementDiscovered' seteado.");

        // Ocultar alfombra
        if (carpetObject != null)
        {
            carpetObject.SetActive(false);
        }

        // Mostrar puerta del sótano
        RefreshBasementDoorVisibility();

        // Desactivar collider de la alfombra
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Mostrar mensaje de revelación
        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(revealMessage);
        }
    }

    /// <summary>
    /// Llamado por la puerta del sótano si el jugador intenta bajar sin condiciones.
    /// </summary>
    public static void ShowBlockedMessage()
    {
        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage("No puedo bajar por ahí todavía… debería hablar con los demás primero.");
        }
    }
}
