using System;
using UnityEngine;

/// <summary>
/// Puerta/trigger que al ser clickeada cambia de sala.
/// Actúa como checkpoint narrativo: valida condiciones antes de permitir el paso.
/// 
/// VALIDACIÓN PARA PUERTA DEL ESTUDIO (Cap 1 → Cap 2):
///   - Sin NPC interrogados → BLOQUEADA (aunque tenga llave)
///   - Con 1+ NPC interrogados + llave → PERMITIDA (inicia Cap 2)
///   - Con TODOS los NPC interrogados + llave → Muestra decisión antes de avanzar
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

    [Header("Validación NPC (checkpoint narrativo)")]
    [Tooltip("Mínimo de NPCs con los que se debe haber hablado para abrir esta puerta")]
    [SerializeField] private int requiredNpcTalkCount = 0;
    [Tooltip("Si true, al abrir esta puerta se dispara la transición de capítulo")]
    [SerializeField] private bool triggersChapterTransition = false;
    [Tooltip("ID del capítulo que se activa al cruzar esta puerta")]
    [SerializeField] private string transitionToChapterId;

    [Header("Feedback")]
    [SerializeField] private string lockedMessage = "Esta puerta está cerrada.";

    [Header("Depuración")]
    [SerializeField] private bool showDebugGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);

    // Flags de NPCs hablados
    private static readonly string[] npcTalkFlags = {
        "npc.talked.robert",
        "npc.talked.ana",
        "npc.talked.ben",
        "npc.talked.lisa",
        "npc.talked.lucas"
    };

    private void Start()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null && !col.enabled)
        {
            col.enabled = true;
        }

        Debug.Log($"[DoorTrigger] '{gameObject.name}' inicializado. Destino: '{targetRoomId}', RequiereNPC: {requiredNpcTalkCount}, TransiciónCap: {triggersChapterTransition}");
    }

    private void HideAnxietyOverlayIfVisible()
    {
        AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
        if (anxietySystem != null)
        {
            anxietySystem.HideVerificationOverlay();
        }
    }

    public override void Interact()
    {
        HideAnxietyOverlayIfVisible();

        if (string.IsNullOrWhiteSpace(targetRoomId))
        {
            Debug.LogError($"[DoorTrigger] '{gameObject.name}': targetRoomId vacío!");
            return;
        }

        if (RoomManager.Instance == null)
        {
            Debug.LogError("[DoorTrigger] No hay RoomManager en la escena.");
            return;
        }

        // ═══ VALIDACIÓN COMPLETA ═══
        var validation = ValidateAccess();
        LogValidation(validation);

        if (!validation.canPass)
        {
            // Mostrar feedback narrativo
            DialoguePanelUI panel = DialoguePanelUI.Instance;
            if (panel != null)
            {
                panel.ShowSystemMessage(validation.blockReason);
            }
            return;
        }

        // ═══ ACCESO PERMITIDO ═══

        // Determinar si esta puerta debe disparar transición de capítulo
        bool shouldTriggerTransition = triggersChapterTransition;
        string effectiveTransitionChapter = transitionToChapterId;

        // Auto-detectar: puerta al estudio en Cap 1 siempre dispara transición a Cap 2
        if (!shouldTriggerTransition && IsStudioTransitionDoor())
        {
            shouldTriggerTransition = true;
            effectiveTransitionChapter = "chapter2";
            Debug.Log("[DoorTrigger] Auto-detectado: puerta al estudio en Cap 1 → transición a Cap 2");
        }

        // Si esta puerta dispara transición de capítulo
        if (shouldTriggerTransition && !string.IsNullOrWhiteSpace(effectiveTransitionChapter))
        {
            // Verificar si debe mostrar panel de decisión primero (todos los NPC hablados)
            if (validation.allNpcsTalked && !validation.decisionAlreadyShown)
            {
                LaunchChapterDecisionThenTransition(effectiveTransitionChapter);
                return;
            }

            // Transición directa al nuevo capítulo
            ExecuteChapterTransition(effectiveTransitionChapter);
            return;
        }

        // Puerta normal (sin transición de capítulo)
        DialoguePanelUI normalPanel = DialoguePanelUI.Instance;
        if (normalPanel != null)
        {
            normalPanel.ShowSystemMessage(BuildOpenMessage(), () =>
            {
                normalPanel.Hide();
                ChangeRoom();
            });
            return;
        }

        ChangeRoom();
    }

    // ═══════════════════════════════════════════════════════════════
    //  VALIDACIÓN
    // ═══════════════════════════════════════════════════════════════

    private struct ValidationResult
    {
        public bool canPass;
        public string blockReason;
        public bool hasKey;
        public int npcTalkCount;
        public bool allNpcsTalked;
        public bool decisionAlreadyShown;
    }

    private ValidationResult ValidateAccess()
    {
        var result = new ValidationResult();
        result.canPass = true;
        result.npcTalkCount = CountNpcTalks();
        result.allNpcsTalked = result.npcTalkCount >= 5;
        result.hasKey = true;

        // 1. Verificar capítulo requerido
        if (!string.IsNullOrWhiteSpace(requiredChapterId) && StoryState.Instance != null)
        {
            if (StoryState.Instance.CurrentChapterId != requiredChapterId)
            {
                result.canPass = false;
                result.blockReason = "No puedes acceder a esta zona en este momento.";
                return result;
            }
        }

        // 2. Verificar flag requerido
        if (!string.IsNullOrWhiteSpace(requiredFlag) && StoryState.Instance != null)
        {
            if (!StoryState.Instance.HasFlag(requiredFlag))
            {
                result.canPass = false;
                result.blockReason = BuildLockedMessage(false);
                return result;
            }
        }

        // 3. Verificar llaves
        if (requiredKeys != null && requiredKeys.Length > 0)
        {
            foreach (KeyType keyType in requiredKeys)
            {
                string itemId = keyType.ToString();
                if (!InventoryState.HasItem(itemId))
                {
                    result.hasKey = false;
                    result.canPass = false;
                    result.blockReason = BuildLockedMessage(true);
                    return result;
                }
            }
        }

        // 4. Verificar NPCs interrogados (CHECKPOINT NARRATIVO)
        //    Si esta puerta va al estudio Y estamos en chapter1, SIEMPRE requiere mínimo 1 NPC
        //    Esto funciona incluso si triggersChapterTransition no está configurado en el Inspector
        int effectiveNpcRequired = requiredNpcTalkCount;
        bool isStudioDoorInChapter1 = IsStudioTransitionDoor();
        
        if (isStudioDoorInChapter1 && effectiveNpcRequired < 1)
        {
            effectiveNpcRequired = 1;
        }
        
        if (triggersChapterTransition && effectiveNpcRequired < 1)
        {
            effectiveNpcRequired = 1;
        }

        if (effectiveNpcRequired > 0)
        {
            if (result.npcTalkCount < effectiveNpcRequired)
            {
                result.canPass = false;
                result.blockReason = "Sería mejor interrogar a alguien antes de avanzar. Habla con los personajes usando el botón 'Hablar'.";
                return result;
            }
        }

        // 5. Verificar si la decisión ya se mostró
        if (StoryState.Instance != null)
        {
            string currentChapter = StoryState.Instance.CurrentChapterId;
            result.decisionAlreadyShown = StoryState.Instance.HasFlag($"chapter.{currentChapter}.complete");
        }

        return result;
    }

    private void LogValidation(ValidationResult v)
    {
        string keyStatus = (requiredKeys != null && requiredKeys.Length > 0) ? v.hasKey.ToString().ToUpper() : "N/A";
        Debug.Log($"[CHAPTER VALIDATION] Door: {gameObject.name}\n" +
                  $"  HasKey: {keyStatus}\n" +
                  $"  NPCInterrogatedCount: {v.npcTalkCount}/5\n" +
                  $"  CanEnterStudio: {v.canPass.ToString().ToUpper()}\n" +
                  $"  AllNPCsTalked: {v.allNpcsTalked}\n" +
                  $"  ShowDecisionPanel: {(v.allNpcsTalked && !v.decisionAlreadyShown).ToString().ToUpper()}\n" +
                  $"  TriggerChapterTransition: {triggersChapterTransition.ToString().ToUpper()}\n" +
                  $"  Reason: {(v.canPass ? "ACCESS GRANTED" : v.blockReason)}");
    }

    // ═══════════════════════════════════════════════════════════════
    //  TRANSICIÓN DE CAPÍTULO
    // ═══════════════════════════════════════════════════════════════

    private void ExecuteChapterTransition(string targetChapterId)
    {
        if (StoryState.Instance == null)
        {
            ChangeRoom();
            return;
        }

        // Marcar capítulo actual como completo
        string currentChapter = StoryState.Instance.CurrentChapterId;
        StoryState.Instance.SetFlag($"chapter.{currentChapter}.complete", true);

        // Cambiar al nuevo capítulo
        StoryState.Instance.SetChapter(targetChapterId);
        StoryState.Instance.SetFlag($"{targetChapterId}.intro.seen", true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockChapter(targetChapterId);
        }

        if (NpcLocationManager.Instance != null)
        {
            NpcLocationManager.Instance.UpdateNpcPositionsForChapter(targetChapterId);
        }

        Debug.Log($"[DoorTrigger] ═══ TRANSICIÓN: {currentChapter} → {targetChapterId} ═══");

        // Cambiar de sala
        ChangeRoom();

        // Lanzar intro del nuevo capítulo
        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner != null && !runner.IsRunning)
        {
            string introId = $"{targetChapterId}_intro";
            if (runner.HasConversation(introId))
            {
                runner.StartConversation(introId, "start");
                Debug.Log($"[DoorTrigger] Lanzando intro: {introId}");
            }
        }
    }

    private void LaunchChapterDecisionThenTransition(string targetChapterId)
    {
        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null || runner.IsRunning)
        {
            ExecuteChapterTransition(targetChapterId);
            return;
        }

        string currentChapter = StoryState.Instance != null ? StoryState.Instance.CurrentChapterId : "chapter1";
        string decisionId = $"{currentChapter}_decision";

        if (!runner.HasConversation(decisionId))
        {
            ExecuteChapterTransition(targetChapterId);
            return;
        }

        Debug.Log($"[DoorTrigger] Lanzando decisión '{decisionId}' antes de transición a '{targetChapterId}'.");

        string capturedTarget = targetChapterId;
        Action<string> onDecisionEnd = null;
        onDecisionEnd = (endedId) =>
        {
            if (endedId == decisionId)
            {
                runner.ConversationEnded -= onDecisionEnd;
                ExecuteChapterTransition(capturedTarget);
            }
        };

        runner.ConversationEnded += onDecisionEnd;
        runner.StartConversation(decisionId, "start");
    }

    // ═══════════════════════════════════════════════════════════════
    //  UTILIDADES
    // ═══════════════════════════════════════════════════════════════

    private void ChangeRoom()
    {
        if (RoomManager.Instance == null)
        {
            return;
        }

        bool success = RoomManager.Instance.ChangeRoom(targetRoomId);
        Debug.Log($"[DoorTrigger] ChangeRoom('{targetRoomId}') => {success}");
    }

    private static int CountNpcTalks()
    {
        if (StoryState.Instance == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < npcTalkFlags.Length; i++)
        {
            if (StoryState.Instance.HasFlag(npcTalkFlags[i]))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Detecta si esta puerta es la del estudio durante el capítulo 1.
    /// Funciona por convención de nombres sin necesidad de configuración manual.
    /// </summary>
    private bool IsStudioTransitionDoor()
    {
        if (StoryState.Instance == null)
        {
            return false;
        }

        // Solo aplica durante el capítulo 1
        if (StoryState.Instance.CurrentChapterId != "chapter1")
        {
            return false;
        }

        // Detectar por targetRoomId
        if (targetRoomId == "estudio" || targetRoomId == "studio")
        {
            return true;
        }

        return false;
    }

    private bool IsLobbyTarget()
    {
        return string.Equals(targetRoomId, "lobby", StringComparison.OrdinalIgnoreCase);
    }

    private string GetTargetDisplayName()
    {
        if (RoomManager.Instance == null)
        {
            return targetRoomId;
        }

        return RoomManager.Instance.GetRoomDisplayName(targetRoomId);
    }

    private string BuildLockedMessage(bool needsKey)
    {
        if (IsLobbyTarget())
        {
            return needsKey
                ? "La puerta para Volver al Lobby está cerrada, requiere una llave."
                : "La puerta para Volver al Lobby está cerrada.";
        }

        string displayName = GetTargetDisplayName();
        return needsKey
            ? $"La puerta a {displayName} está cerrada, requiere una llave."
            : $"La puerta a {displayName} está cerrada.";
    }

    private string BuildOpenMessage()
    {
        if (IsLobbyTarget())
        {
            return "Volver al Lobby.";
        }

        string displayName = GetTargetDisplayName();
        return $"La puerta del {displayName} se ha abierto.";
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
