using UnityEngine;

public class ChapterFlowController : MonoBehaviour
{
    [Header("Conversaciones clave")]
    [SerializeField] private string prologueIntroConversationId = "prologue_intro";
    [SerializeField] private string chapter1IntroConversationId = "chapter1_intro";
    [SerializeField] private string chapter1DecisionConversationId = "chapter1_decision";

    [Header("Capitulos")]
    [SerializeField] private string prologueChapterId = "prologue";
    [SerializeField] private string chapter1Id = "chapter1";

    [Header("Flags")]
    [SerializeField] private string prologueCompleteFlag = "chapter.prologue.complete";
    [SerializeField] private string chapter1CompleteFlag = "chapter.chapter1.complete";

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

        if (StoryState.Instance.CurrentChapterId == prologueChapterId && !StoryState.Instance.HasFlag(prologueCompleteFlag))
        {
            dialogueRunner.StartConversation(prologueIntroConversationId, "start");
            return;
        }

        if (StoryState.Instance.CurrentChapterId == chapter1Id && !StoryState.Instance.HasFlag("chapter1.intro.seen"))
        {
            dialogueRunner.StartConversation(chapter1IntroConversationId, "start");
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

        if (conversationId == chapter1DecisionConversationId)
        {
            StoryState.Instance.SetFlag(chapter1CompleteFlag, true);
            return;
        }
    }
}
