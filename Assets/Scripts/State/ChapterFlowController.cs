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
    [SerializeField] private string chapter4Id = "chapter4";
    [SerializeField] private string chapter5Id = "chapter5";

    [Header("Flags")]
    [SerializeField] private string prologueCompleteFlag = "chapter.prologue.complete";
    [SerializeField] private string chapter1CompleteFlag = "chapter.chapter1.complete";
    [SerializeField] private string chapter2CompleteFlag = "chapter.chapter2.complete";
    [SerializeField] private string chapter3CompleteFlag = "chapter.chapter3.complete";
    [SerializeField] private string chapter4CompleteFlag = "chapter.chapter4.complete";
    [SerializeField] private string chapter5CompleteFlag = "chapter.chapter5.complete";

    [Header("Prueba")]
    [SerializeField] private bool forceFreshStartOnPlay = false;

    private DialogueRunner dialogueRunner;

    private string lastObservedChapterId = string.Empty;
    private readonly HashSet<string> talkedNpcIdsInCurrentChapter = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
    private int npcTalkedCountInCurrentChapter;

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

        if (StoryState.Instance.HasFlag("newgame.intro.completed"))
        {
            if (!StoryState.Instance.HasFlag("session.started"))
            {
                StoryState.Instance.SetFlag("session.started", true);
            }

            if (StoryState.Instance.CurrentChapterId == prologueChapterId)
            {
                StoryState.Instance.SetChapter(chapter1Id);
            }

            if (RoomManager.Instance != null && RoomManager.Instance.HasRoom("lobby"))
            {
                RoomManager.Instance.ChangeRoom("lobby");
            }

            ResumeFromCurrentChapter();
            return;
        }

        if (forceFreshStartOnPlay)
        {
            StoryState.Instance.ResetForNewGame(prologueChapterId);
            Debug.Log("[ChapterFlow] Fresh start → Prólogo (solo editor/debug)");
        }

        if (!StoryState.Instance.HasFlag("session.started"))
        {
            StoryState.Instance.SetFlag("session.started", true);
            StoryState.Instance.SetChapter(prologueChapterId);

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

    private void OnStateChanged()
    {
        SyncChapterTracking(false);
    }

    private void OnRoomChanged(string previousRoom, string newRoom)
    {
        if (StoryState.Instance == null || StoryState.Instance.CurrentChapterId != chapter3Id)
        {
            return;
        }

        if (!string.Equals(newRoom, "lobby", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        StoryState.Instance.SetFlag("PlayerEnteredNpcRoom", true);
        StoryState.Instance.SetFlag("chapter3.npc_room.entered", true);
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
