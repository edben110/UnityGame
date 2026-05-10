using UnityEngine;

/// <summary>
/// Controla el flujo narrativo entre capítulos.
/// Escucha el fin de conversaciones clave y avanza el capítulo
/// cuando el jugador completa las decisiones requeridas.
///
/// Flujo soportado:
///   Prólogo → Cap 1 → Cap 2 → Cap 3 → (Cap 4, Cap 5 futuro)
/// </summary>
public class ChapterFlowController : MonoBehaviour
{
    [Header("Conversaciones clave")]
    [SerializeField] private string prologueIntroConversationId = "prologue_intro";
    [SerializeField] private string chapter1IntroConversationId = "chapter1_intro";
    [SerializeField] private string chapter1DecisionConversationId = "chapter1_decision";
    [SerializeField] private string chapter2IntroConversationId = "chapter2_intro";
    [SerializeField] private string chapter2DecisionConversationId = "chapter2_decision";
    [SerializeField] private string chapter3IntroConversationId = "chapter3_intro";
    [SerializeField] private string chapter3DecisionConversationId = "chapter3_decision";

    [Header("Capitulos")]
    [SerializeField] private string prologueChapterId = "prologue";
    [SerializeField] private string chapter1Id = "chapter1";
    [SerializeField] private string chapter2Id = "chapter2";
    [SerializeField] private string chapter3Id = "chapter3";

    [Header("Flags")]
    [SerializeField] private string prologueCompleteFlag = "chapter.prologue.complete";
    [SerializeField] private string chapter1CompleteFlag = "chapter.chapter1.complete";
    [SerializeField] private string chapter2CompleteFlag = "chapter.chapter2.complete";
    [SerializeField] private string chapter3CompleteFlag = "chapter.chapter3.complete";

    [Header("Prueba")]
    [SerializeField] private bool forceFreshStartOnPlay = true;

    private DialogueRunner dialogueRunner;

    private void Start()
    {
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        if (dialogueRunner == null)
        {
            Debug.LogError("ChapterFlowController necesita un DialogueRunner en la escena.");
            return;
        }

        dialogueRunner.ConversationEnded += OnConversationEnded;

        if (StoryState.Instance == null)
        {
            Debug.LogError("ChapterFlowController necesita StoryState en la escena.");
            return;
        }

        if (forceFreshStartOnPlay)
        {
            StoryState.Instance.ResetForNewGame(prologueChapterId);
        }

        if (!StoryState.Instance.HasFlag("session.started"))
        {
            StoryState.Instance.SetFlag("session.started", true);
            StoryState.Instance.SetChapter(prologueChapterId);
            dialogueRunner.StartConversation(prologueIntroConversationId, "start");
            return;
        }

        // Reanudar según el capítulo actual
        string currentChapter = StoryState.Instance.CurrentChapterId;

        if (currentChapter == prologueChapterId && !StoryState.Instance.HasFlag(prologueCompleteFlag))
        {
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

    private void OnDestroy()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.ConversationEnded -= OnConversationEnded;
        }
    }

    private void OnConversationEnded(string conversationId)
    {
        if (StoryState.Instance == null)
        {
            return;
        }

        // ─── Prólogo completado → Cap 1 ───
        if (conversationId == prologueIntroConversationId)
        {
            StoryState.Instance.SetFlag(prologueCompleteFlag, true);
            StoryState.Instance.SetChapter(chapter1Id);
            StoryState.Instance.SetFlag("chapter1.intro.seen", true);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockChapter(chapter1Id);
            }

            dialogueRunner.StartConversation(chapter1IntroConversationId, "start");
            return;
        }

        // ─── Cap 1 decisión completada → Cap 2 ───
        if (conversationId == chapter1DecisionConversationId)
        {
            StoryState.Instance.SetFlag(chapter1CompleteFlag, true);
            StoryState.Instance.SetChapter(chapter2Id);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockChapter(chapter2Id);
            }

            // Mover NPCs al estudio para Cap 2
            if (NpcLocationManager.Instance != null)
            {
                NpcLocationManager.Instance.UpdateNpcPositionsForChapter(chapter2Id);
            }

            Debug.Log("[ChapterFlowController] Capítulo 2 desbloqueado.");
            return;
        }

        // ─── Cap 2 decisión completada → Cap 3 ───
        if (conversationId == chapter2DecisionConversationId)
        {
            StoryState.Instance.SetFlag(chapter2CompleteFlag, true);
            StoryState.Instance.SetChapter(chapter3Id);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockChapter(chapter3Id);
            }

            // Dispersar NPCs para Cap 3
            if (NpcLocationManager.Instance != null)
            {
                NpcLocationManager.Instance.UpdateNpcPositionsForChapter(chapter3Id);
            }

            Debug.Log("[ChapterFlowController] Capítulo 3 desbloqueado.");
            return;
        }

        // ─── Cap 3 decisión completada → (futuro Cap 4) ───
        if (conversationId == chapter3DecisionConversationId)
        {
            StoryState.Instance.SetFlag(chapter3CompleteFlag, true);
            Debug.Log("[ChapterFlowController] Capítulo 3 completado. Cap 4 pendiente de implementación.");
            return;
        }
    }
}
