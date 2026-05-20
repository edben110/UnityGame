using UnityEngine;

/// <summary>
/// Controla la aparición condicional de la SingleUseKey.
/// 
/// FLUJO NARRATIVO (basado en flujoHistoria.txt):
///   1. Security Room desbloqueada (NumericPasswordPanel resuelto)
///   2. Jugador entra a Security Room (Door_ToSecurityRoom interactuada)
///   3. Jugador interactúa con Door_ToaBasementeFromSecureDoor
///   4. SingleUseKey hotspot aparece (sprite visible, collider activo)
///
/// La llave NO existe desde el inicio. Solo se materializa cuando
/// el jugador ha completado la secuencia de exploración de la sala de seguridad.
///
/// FLAG DE ACTIVACIÓN:
///   "chapter4.basement_from_security.entered" → activa el hotspot
///
/// DIÁLOGO AL RECOGER:
///   "No me percaté de esa llave antes, parece haberse caído de algún lugar al abrir la puerta."
/// </summary>
public class SingleUseKeySpawner : MonoBehaviour
{
    [Header("Hotspot de la llave")]
    [SerializeField] private MapHotspot singleUseKeyHotspot;

    [Header("Flags requeridos")]
    [SerializeField] private string securityRoomUnlockedFlag = "SecurityRoom.Unlocked";
    [SerializeField] private string basementFromSecurityEnteredFlag = "chapter4.basement_from_security.entered";

    [Header("Diálogo al aparecer")]
    [TextArea(2, 4)]
    [SerializeField] private string spawnDialogue = "No me percaté de esa llave antes, parece haberse caído de algún lugar al abrir la puerta.";

    private bool hasSpawned;
    private bool dialogueShown;

    private void Start()
    {
        // Verificar si ya fue recogida
        if (InventoryState.HasItem("SingleUseKey") || InventoryState.HasItem("singleusekey"))
        {
            hasSpawned = true;
            HideHotspot();
            return;
        }

        // Verificar si ya fue spawneada previamente
        if (StoryState.Instance != null && StoryState.Instance.HasFlag("chapter4.single_use_key.spawned"))
        {
            hasSpawned = true;
            ShowHotspot();
            return;
        }

        // Ocultar hasta que se cumplan condiciones
        HideHotspot();

        // Suscribirse a cambios de estado
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged += OnStateChanged;
        }
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
        if (hasSpawned)
        {
            return;
        }

        TrySpawnKey();
    }

    private void TrySpawnKey()
    {
        if (hasSpawned)
        {
            return;
        }

        if (StoryState.Instance == null)
        {
            return;
        }

        // Condición 1: Estar en chapter4
        if (StoryState.Instance.CurrentChapterId != "chapter4")
        {
            return;
        }

        // Condición 2: Security Room desbloqueada
        if (!StoryState.Instance.HasFlag(securityRoomUnlockedFlag))
        {
            return;
        }

        // Condición 3: Jugador interactuó con Door_ToaBasementeFromSecureDoor
        if (!StoryState.Instance.HasFlag(basementFromSecurityEnteredFlag))
        {
            return;
        }

        // Todas las condiciones cumplidas — spawnar la llave
        hasSpawned = true;
        StoryState.Instance.SetFlag("chapter4.single_use_key.spawned", true);
        ShowHotspot();
        ShowSpawnDialogue();

        Debug.Log("[SingleUseKeySpawner] ★ SingleUseKey ha aparecido en la sala de seguridad.");
    }

    private void ShowHotspot()
    {
        if (singleUseKeyHotspot == null)
        {
            return;
        }

        singleUseKeyHotspot.gameObject.SetActive(true);

        SpriteRenderer sr = singleUseKeyHotspot.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
        }

        Collider2D col = singleUseKeyHotspot.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    private void HideHotspot()
    {
        if (singleUseKeyHotspot == null)
        {
            return;
        }

        SpriteRenderer sr = singleUseKeyHotspot.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }

        Collider2D col = singleUseKeyHotspot.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private void ShowSpawnDialogue()
    {
        if (dialogueShown)
        {
            return;
        }

        dialogueShown = true;

        if (string.IsNullOrWhiteSpace(spawnDialogue))
        {
            return;
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(spawnDialogue);
        }
    }
}
