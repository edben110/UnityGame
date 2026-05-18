using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el flujo narrativo entre capítulos.
/// 
/// RESPONSABILIDADES:
///   1. Iniciar el prólogo al comenzar el juego
///   2. Escuchar cuando terminan conversaciones clave (decisiones)
///   3. Avanzar el capítulo cuando corresponde
///   4. Proveer información de progreso para otros sistemas (DoorTrigger)
///
/// FLUJO:
///   Prólogo → Cap 1 intro → [jugador explora] → DoorTrigger dispara transición → Cap 2
///   
/// NOTA: La transición Cap1→Cap2 ahora la maneja DoorTrigger del estudio.
///       Este controller solo maneja prólogo→cap1 y las decisiones de cap2/cap3.
/// </summary>
public class ChapterFlowController : MonoBehaviour
{
    public static ChapterFlowController Instance { get; private set; }

    [Header("Conversaciones clave")]
    [SerializeField] private string prologueIntroConversationId = "prologue_intro";
    [SerializeField] private string chapter1IntroConversationId = "chapter1_intro";
    [SerializeField] private string chapter1DecisionConversationId = "chapter1_decision";
    [SerializeField] private string chapter2IntroConversationId = "chapter2_intro";
    [SerializeField] private string chapter2InitialDecisionConversationId = "chapter2_initial_decision";
    [SerializeField] private string chapter2BookDecisionConversationId = "chapter2_book_decision";
    [SerializeField] private string chapter3IntroConversationId = "chapter3_intro";
    [SerializeField] private string chapter3DecisionConversationId = "chapter3_decision";

    [Header("Capitulos")]
    [SerializeField] private string prologueChapterId = "prologue";
    [SerializeField] private string chapter1Id = "chapter1";
    [SerializeField] private string chapter2Id = "chapter2";
    [SerializeField] private string chapter3Id = "chapter3";

    [Header("Cap 3: Decisión de la carta")]
    [SerializeField] private string chapter3NpcRoomId = "lobby";
    [SerializeField] private string[] chapter3NpcAllowedBackgroundNames = { "LivingRoom", "NPCRoom", "Lobby" };
    private const string Chapter3LetterDecisionShownFlag = "chapter3.letter_decision.shown";

    [Header("Flags")]
    [SerializeField] private string prologueCompleteFlag = "chapter.prologue.complete";
    [SerializeField] private string chapter1CompleteFlag = "chapter.chapter1.complete";
    [SerializeField] private string chapter2CompleteFlag = "chapter.chapter2.complete";
    [SerializeField] private string chapter3CompleteFlag = "chapter.chapter3.complete";

    [Header("Progreso Cap 2 y 3 (auto-decisión)")]
    [SerializeField] private int chapter2RequiredClues = 2;

    [Header("Prueba")]
    [SerializeField] private bool forceFreshStartOnPlay = true;

    private DialogueRunner dialogueRunner;
    private bool chapter2DecisionLaunched;
    private bool pendingProgressCheck;

    // Flags que setean los hotspots del Cap 2
    private string lastObservedChapterId = string.Empty;
    private readonly HashSet<string> talkedNpcIdsInCurrentChapter = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
    private int npcTalkedCountInCurrentChapter;
    private static readonly string[] chapter2ClueFlags = {
        "clue.estudio.agenda",
        "clue.estudio.libro_contabilidad",
        "clue.estudio.nota_tablon",
        "clue.estudio.tablero_corcho",
        "clue.estudio.archivador_visto"
    };

    // Flags de NPCs hablados
    private static readonly string[] npcTalkFlags = {
        "npc.talked.robert",
        "npc.talked.ana",
        "npc.talked.ben",
        "npc.talked.lisa",
        "npc.talked.lucas"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        if (dialogueRunner == null)
        {
            Debug.LogError("[ChapterFlow] ERROR: No hay DialogueRunner.");
            return;
        }

        dialogueRunner.ConversationEnded += OnConversationEnded;

        if (StoryState.Instance == null)
        {
            Debug.LogError("[ChapterFlow] ERROR: No hay StoryState.");
            return;
        }

        StoryState.Instance.StateChanged += OnStateChanged;

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged += OnRoomChanged;
        }

        if (forceFreshStartOnPlay)
        {
            StoryState.Instance.ResetForNewGame(prologueChapterId);
            Debug.Log("[ChapterFlow] Fresh start → Prólogo");
        }

        if (!StoryState.Instance.HasFlag("session.started"))
        {
            StoryState.Instance.SetFlag("session.started", true);
            StoryState.Instance.SetChapter(prologueChapterId);

            // Intentar cambiar a la sala del prólogo si existe
            if (RoomManager.Instance != null && RoomManager.Instance.HasRoom("prologue"))
            {
                RoomManager.Instance.ChangeRoom("prologue");
            }

            dialogueRunner.StartConversation(prologueIntroConversationId, "start");
            return;
        }

        ResumeFromCurrentChapter();
    }

    private void OnDestroy()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.ConversationEnded -= OnConversationEnded;
        }

        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged -= OnStateChanged;

            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.RoomChanged -= OnRoomChanged;
            }
        }
    }

    private void Update()
    {
        // Verificar progreso cuando el diálogo termina
        if (pendingProgressCheck && dialogueRunner != null && !dialogueRunner.IsRunning)
        {
            pendingProgressCheck = false;
            CheckAutoDecisions();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  API PÚBLICA (usada por DoorTrigger)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Retorna cuántos NPCs ha interrogado el jugador.
    /// </summary>
    public int GetNpcTalkCount()
    {
        return npcTalkedCountInCurrentChapter;
    }

    /// <summary>
    /// Retorna mensaje de qué le falta al jugador para avanzar.
    /// </summary>
    public string GetMissingRequirementsMessage()
    {
        if (StoryState.Instance == null)
        {
            return string.Empty;
        }

        string chapter = StoryState.Instance.CurrentChapterId;

        if (chapter == chapter1Id)
        {
            int npcTalks = GetNpcTalkCount();
            if (npcTalks == 0)
            {
                return "Sería mejor interrogar a alguien antes de avanzar. Habla con los personajes usando el botón 'Hablar'.";
            }
        }

        return string.Empty;
    }

    // ═══════════════════════════════════════════════════════════════
    //  AUTO-DECISIONES (Cap 2 y Cap 3)
    // ═══════════════════════════════════════════════════════════════

    private void OnStateChanged()
    {
        SyncChapterTracking(false);
        pendingProgressCheck = true;
    }

    private void OnRoomChanged(string previousRoom, string newRoom)
    {
        if (StoryState.Instance == null || StoryState.Instance.CurrentChapterId != chapter3Id)
        {
            return;
        }

        if (!string.Equals(newRoom, chapter3NpcRoomId, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        StoryState.Instance.SetFlag("PlayerEnteredNpcRoom", true);
        StoryState.Instance.SetFlag("chapter3.npc_room.entered", true);
        TryLaunchChapter3DecisionInNpcRoom();
    }

    private void SyncChapterTracking(bool forceRebuild)
    {
        if (StoryState.Instance == null)
        {
            return;
        }

        string currentChapter = StoryState.Instance.CurrentChapterId;
        if (!forceRebuild && string.Equals(currentChapter, lastObservedChapterId, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        lastObservedChapterId = currentChapter;
        ResetNpcTalkProgress();
        RebuildNpcTalkProgressForCurrentChapter();
    }

    private void ResetNpcTalkProgress()
    {
        talkedNpcIdsInCurrentChapter.Clear();
        npcTalkedCountInCurrentChapter = 0;
    }

    public void ResetNpcTalkTrackingForNewChapter()
    {
        ResetNpcTalkProgress();
        lastObservedChapterId = StoryState.Instance != null
            ? StoryState.Instance.CurrentChapterId
            : string.Empty;
    }

    private void RebuildNpcTalkProgressForCurrentChapter()
    {
        if (StoryState.Instance == null || string.IsNullOrWhiteSpace(StoryState.Instance.CurrentChapterId))
        {
            return;
        }

        string currentChapter = StoryState.Instance.CurrentChapterId;
        for (int i = 0; i < npcTalkFlags.Length; i++)
        {
            string npcId = npcTalkFlags[i].Replace("npc.talked.", string.Empty);
            if (StoryState.Instance.HasFlag($"chapter.{currentChapter}.npc.talked.{npcId}"))
            {
                talkedNpcIdsInCurrentChapter.Add(npcId);
            }
        }

        npcTalkedCountInCurrentChapter = talkedNpcIdsInCurrentChapter.Count;
    }

    public bool HasTalkedToNpcInCurrentChapter(string npcId)
    {
        return !string.IsNullOrWhiteSpace(npcId) && talkedNpcIdsInCurrentChapter.Contains(npcId);
    }

    public void RegisterNpcTalked(string npcId)
    {
        if (StoryState.Instance == null || string.IsNullOrWhiteSpace(npcId))
        {
            return;
        }

        string normalizedNpcId = npcId.Trim().ToLowerInvariant();
        if (talkedNpcIdsInCurrentChapter.Add(normalizedNpcId))
        {
            npcTalkedCountInCurrentChapter = talkedNpcIdsInCurrentChapter.Count;
        }

        string currentChapter = StoryState.Instance.CurrentChapterId;
        if (!string.IsNullOrWhiteSpace(currentChapter))
        {
            StoryState.Instance.SetFlag($"chapter.{currentChapter}.npc.talked.{normalizedNpcId}", true);
        }
    }

    /// <summary>
    /// Verifica si se deben lanzar decisiones para Cap 2 y Cap 3.
    /// </summary>
    private void CheckAutoDecisions()
    {
        if (StoryState.Instance == null || dialogueRunner == null || dialogueRunner.IsRunning)
        {
            return;
        }

        string chapter = StoryState.Instance.CurrentChapterId;

        // CAP 2: Disparar PRIMERA decisión cuando se han hablado con los 5 NPCs
        if (chapter == chapter2Id && !chapter2DecisionLaunched && !StoryState.Instance.HasFlag(chapter2CompleteFlag))
        {
            int npcTalkCount = GetNpcTalkCount();
            if (npcTalkCount >= 5)  // Se han hablado con TODOS los NPCs
            {
                // Verificar que el jugador está en la sala de NPCs
                bool inNpcRoom = RoomManager.Instance == null || RoomManager.Instance.CurrentRoomId == "lobby";
                if (inNpcRoom && !StoryState.Instance.HasFlag("chapter2.initial_decision.shown"))
                {
                    chapter2DecisionLaunched = true;
                    Debug.Log("[ChapterFlow] ★ Cap 2: Hablado con los 5 NPCs. Lanzando decisión inicial...");
                    Invoke(nameof(LaunchChapter2InitialDecision), 1.5f);
                }
            }
        }

        // CAP 3: Disparar decisión al tener la carta y entrar a la sala de NPCs
        // La decisión de la carta se dispara solamente al entrar en la sala de NPCs.
    }

    private void TryLaunchChapter3DecisionInNpcRoom()
    {
        if (StoryState.Instance == null || dialogueRunner == null)
        {
            return;
        }

        if (StoryState.Instance.CurrentChapterId != chapter3Id)
        {
            return;
        }

        if (RoomManager.Instance == null || !string.Equals(RoomManager.Instance.CurrentRoomId, chapter3NpcRoomId, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!IsChapter3NpcVisibleContext())
        {
            Debug.Log("[ChapterFlow] Cap 3 decision blocked: the player is not in the allowed visible NPC background context.");
            return;
        }

        if (!InventoryState.HasItem("HS_Unfinished_Letter"))
        {
            return;
        }

        if (StoryState.Instance.HasFlag(Chapter3LetterDecisionShownFlag) || StoryState.Instance.HasFlag("chapter3.decision.shown"))
        {
            return;
        }

        StoryState.Instance.SetFlag(Chapter3LetterDecisionShownFlag, true);
        StoryState.Instance.SetFlag("chapter3.decision.shown", true);
        Debug.Log("[ChapterFlow] ★ Cap 3: Carta encontrada. En sala de NPCs. Lanzando decisión...");
        Invoke(nameof(LaunchChapter3Decision), 1.5f);
    }

    private bool IsChapter3NpcVisibleContext()
    {
        if (RoomManager.Instance == null)
        {
            return false;
        }

        string currentRoomId = RoomManager.Instance.CurrentRoomId;
        if (!string.Equals(currentRoomId, chapter3NpcRoomId, System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string displayName = RoomManager.Instance.CurrentRoomDisplayName;
        string backgroundName = RoomManager.Instance.GetCurrentRoomBackgroundObjectName();

        return MatchesAllowedNpcContext(displayName)
            || MatchesAllowedNpcContext(backgroundName);
    }

    private bool MatchesAllowedNpcContext(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || chapter3NpcAllowedBackgroundNames == null)
        {
            return false;
        }

        for (int i = 0; i < chapter3NpcAllowedBackgroundNames.Length; i++)
        {
            string allowed = chapter3NpcAllowedBackgroundNames[i];
            if (string.IsNullOrWhiteSpace(allowed))
            {
                continue;
            }

            if (string.Equals(value, allowed, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void LaunchChapter2InitialDecision()
    {
        if (dialogueRunner != null && !dialogueRunner.IsRunning)
        {
            StoryState.Instance.SetFlag("chapter2.initial_decision.shown", true);
            dialogueRunner.StartConversation(chapter2InitialDecisionConversationId, "start");
        }
        else
        {
            Invoke(nameof(LaunchChapter2InitialDecision), 1f);
        }
    }

    private void LaunchChapter3Decision()
    {
        if (dialogueRunner != null && !dialogueRunner.IsRunning)
        {
            dialogueRunner.StartConversation(chapter3DecisionConversationId, "start");
        }
        else
        {
            Invoke(nameof(LaunchChapter3Decision), 1f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CONVERSACIONES TERMINADAS
    // ═══════════════════════════════════════════════════════════════

    private void OnConversationEnded(string conversationId)
    {
        if (StoryState.Instance == null)
        {
            return;
        }

        Debug.Log($"[ChapterFlow] Conversación terminada: '{conversationId}'");

        // ─── Prólogo → Cap 1 ───
        if (conversationId == prologueIntroConversationId)
        {
            StoryState.Instance.SetFlag(prologueCompleteFlag, true);
            StoryState.Instance.SetChapter(chapter1Id);
            StoryState.Instance.SetFlag("chapter1.intro.seen", true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockChapter(chapter1Id);
            }

            // Cambiar a la sala del lobby para iniciar Cap 1
            if (RoomManager.Instance != null && RoomManager.Instance.HasRoom("lobby"))
            {
                RoomManager.Instance.ChangeRoom("lobby");
            }

            Debug.Log("[ChapterFlow] ═══ PRÓLOGO → CAP 1 ═══");
            dialogueRunner.StartConversation(chapter1IntroConversationId, "start");
            return;
        }

        // ─── Cap 1 decisión completada (si se lanzó desde DoorTrigger) ───
        if (conversationId == chapter1DecisionConversationId)
        {
            // La transición real la maneja DoorTrigger.ExecuteChapterTransition()
            Debug.Log("[ChapterFlow] Cap 1 decisión completada.");
            return;
        }

        // ─── Cap 2 DECISIÓN INICIAL completada ───
        if (conversationId == chapter2InitialDecisionConversationId)
        {
            Debug.Log("[ChapterFlow] Cap 2 decisión inicial completada. El jugador puede continuar explorando.");
            // No hacemos transición automática. El jugador continúa investigando.
            return;
        }

        // ─── Cap 2 DECISIÓN DEL LIBRO completada (sin transición automática) ───
        if (conversationId == chapter2BookDecisionConversationId)
        {
            bool accountingBookSelected = InventoryState.HasItem("libro_contabilidad");
            bool talkedToBen = HasTalkedToNpcInCurrentChapter("ben");
            bool searchBedroom = StoryState.Instance.HasFlag("chapter2.choice.search_bedroom");
            bool confrontBen = StoryState.Instance.HasFlag("chapter2.choice.confront_ben");

            StoryState.Instance.SetFlag("chapter2.book_decision.completed", true);
            StoryState.Instance.SetFlag("chapter2.objective.go_to_simon_room", searchBedroom || confrontBen);

            Debug.Log("[BEN CONFRONTATION]");
            Debug.Log($"AccountingBookSelected: {accountingBookSelected.ToString().ToUpper()}");
            Debug.Log($"TalkedToBen: {talkedToBen.ToString().ToUpper()}");
            Debug.Log("DecisionPanelOpened: TRUE");
            Debug.Log("StartChapter3: FALSE");
            Debug.Log("[ChapterFlow] Cap 2 decisión del libro completada. El jugador mantiene control y debe abrir la habitación de Simón manualmente.");
            return;
        }

        // ─── Cap 3 decisión completada ───
        if (conversationId == chapter3DecisionConversationId)
        {
            StoryState.Instance.SetFlag(chapter3CompleteFlag, true);
            Debug.Log("[ChapterFlow] ═══ CAP 3 COMPLETADO ═══");
            return;
        }

        // Después de cualquier conversación, verificar auto-decisiones
        pendingProgressCheck = true;
    }

    private void ResumeFromCurrentChapter()
    {
        string currentChapter = StoryState.Instance.CurrentChapterId;

        if (currentChapter == prologueChapterId && !StoryState.Instance.HasFlag(prologueCompleteFlag))
        {
            // Asegurar que estamos en la sala del prólogo
            if (RoomManager.Instance != null && RoomManager.Instance.HasRoom("prologue"))
            {
                RoomManager.Instance.ChangeRoom("prologue");
            }
            dialogueRunner.StartConversation(prologueIntroConversationId, "start");
            return;
        }

        if (currentChapter == chapter1Id && !StoryState.Instance.HasFlag("chapter1.intro.seen"))
        {
            dialogueRunner.StartConversation(chapter1IntroConversationId, "start");
            return;
        }

        if (currentChapter == chapter2Id && !StoryState.Instance.HasFlag("chapter2.intro.seen"))
        {
            dialogueRunner.StartConversation(chapter2IntroConversationId, "start");
            return;
        }

        if (currentChapter == chapter3Id && !StoryState.Instance.HasFlag("chapter3.intro.seen"))
        {
            dialogueRunner.StartConversation(chapter3IntroConversationId, "start");
        }
    }

    private static int CountFlags(string[] flags)
    {
        int count = 0;
        for (int i = 0; i < flags.Length; i++)
        {
            if (StoryState.Instance.HasFlag(flags[i]))
            {
                count++;
            }
        }
        return count;
    }
}
