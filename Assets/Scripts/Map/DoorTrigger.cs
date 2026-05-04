using UnityEngine;

/// <summary>
/// Puerta/trigger que al ser clickeada cambia de sala.
/// Hereda de Interactable para funcionar con el ClickManager existente.
/// Requiere un BoxCollider2D para detectar clics.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DoorTrigger : Interactable
{
    [Header("Destino")]
    [SerializeField] private string targetRoomId;

    [Header("Condiciones opcionales")]
    [SerializeField] private string requiredFlag;

    [SerializeField] private KeyType[] requiredKeys = new KeyType[0];
    [SerializeField] private string requiredChapterId;

    [Header("Feedback")]
    [SerializeField] private string lockedMessage = "Esta puerta está cerrada.";

    [Header("Depuración")]
    [SerializeField] private bool showDebugGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);

    private void Start()
    {
        // Asegurar que el collider esté habilitado
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && !col.enabled)
        {
            col.enabled = true;
        }

        Debug.Log($"DoorTrigger '{gameObject.name}' inicializado. Destino: '{targetRoomId}'. Collider: {(col != null ? "OK" : "FALTA")}");
    }

    public override void Interact()
    {
        Debug.Log($"DoorTrigger '{gameObject.name}' clickeado! Destino: '{targetRoomId}'");

        if (string.IsNullOrWhiteSpace(targetRoomId))
        {
            Debug.LogError($"DoorTrigger '{gameObject.name}': targetRoomId está vacío!");
            return;
        }

        if (RoomManager.Instance == null)
        {
            Debug.LogError("DoorTrigger: No hay RoomManager en la escena.");
            return;
        }

        // Verificar condición de capítulo
        if (!string.IsNullOrWhiteSpace(requiredChapterId) && StoryState.Instance != null)
        {
            if (StoryState.Instance.CurrentChapterId != requiredChapterId)
            {
                Debug.Log($"DoorTrigger: Puerta bloqueada, requiere capítulo '{requiredChapterId}'.");
                DialoguePanelUI dialoguePanel = DialoguePanelUI.Instance;
                if (dialoguePanel != null)
                {
                    dialoguePanel.ShowSystemMessage(lockedMessage);
                }
                return;
            }
        }

        // Verificar condición de flag
        if (!string.IsNullOrWhiteSpace(requiredFlag) && StoryState.Instance != null)
        {
            if (!StoryState.Instance.HasFlag(requiredFlag))
            {
                Debug.Log($"DoorTrigger: Puerta bloqueada, requiere flag '{requiredFlag}'.");
                DialoguePanelUI dialoguePanel = DialoguePanelUI.Instance;
                if (dialoguePanel != null)
                {
                    dialoguePanel.ShowSystemMessage(lockedMessage);
                }
                return;
            }
        }

        // Verificar llaves requeridas (consultar inventario)
        if (requiredKeys != null && requiredKeys.Length > 0)
        {
            // Depuración: listar llaves requeridas y contenido del inventario
            string invContents = string.Join(", ", InventoryState.GetItems().ToArray());
            Debug.Log($"DoorTrigger '{gameObject.name}': RequiredKeys=[{string.Join(", ", System.Array.ConvertAll(requiredKeys, k => k.ToString()))}] Inventory=[{invContents}] SelectedItem={InventoryState.GetSelectedItem()}");

            bool hasAllKeys = true;
            foreach (KeyType keyType in requiredKeys)
            {
                string itemId = keyType.ToString();
                bool has = InventoryState.HasItem(itemId);
                Debug.Log($"DoorTrigger: Checking key '{itemId}' => {has}");
                if (!has)
                {
                    hasAllKeys = false;
                }
            }

            if (!hasAllKeys)
            {
                Debug.Log($"DoorTrigger: {lockedMessage}");
                DialoguePanelUI dialoguePanel = DialoguePanelUI.Instance;
                if (dialoguePanel != null)
                {
                    dialoguePanel.ShowSystemMessage(lockedMessage);
                }
                return;
            }
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage("¡La puerta se ha abierto!", () =>
            {
                panel.Hide();
                ChangeRoomAfterDialogue();
            });
            return;
        }

        ChangeRoomAfterDialogue();
    }

    private void ChangeRoomAfterDialogue()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("DoorTrigger: No hay RoomManager en la escena.");
            return;
        }

        bool success = RoomManager.Instance.ChangeRoom(targetRoomId);
        Debug.Log($"DoorTrigger: ChangeRoom('{targetRoomId}') resultado: {success}");

        if (success)
        {
            Debug.Log("La puerta se ha abierto.");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmo)
        {
            return;
        }

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        Vector3 center = transform.position + (Vector3)box.offset;
        Vector3 size = new Vector3(box.size.x * transform.lossyScale.x, box.size.y * transform.lossyScale.y, 0.1f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(center, size);
    }
}
