using UnityEngine;

public class NpcInteractable : Interactable
{
    [Header("NPC")]
    [SerializeField] private string npcId;
    [SerializeField] private string npcDisplayName;

    [Header("Flujo")]
    [SerializeField] private string requiredChapterId = "chapter1";
    [SerializeField] private string talkConversationId;

    public override void Interact()
    {
        base.Interact();

        if (StoryState.Instance == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredChapterId) && StoryState.Instance.CurrentChapterId != requiredChapterId)
        {
            return;
        }

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null || runner.IsRunning)
        {
            return;
        }

        if (NpcInteractionMenuUI.Instance == null)
        {
            Debug.LogError("No se encontro NpcInteractionMenuUI en la escena.");
            return;
        }

        NpcInteractionMenuUI.Instance.Show(GetDisplayName(), OnTalkPressed, OnVerifyPressed);
    }

    private void OnTalkPressed()
    {
        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            return;
        }

        bool started = runner.StartConversation(talkConversationId, "start");
        if (!started)
        {
            return;
        }

        if (CharacterAnxietySystem.Instance != null)
        {
            CharacterAnxietySystem.Instance.ApplyTalkRelief(npcId);
        }

        NpcInteractionMenuUI.Instance.Hide();
    }

    private void OnVerifyPressed()
    {
        if (NpcInteractionMenuUI.Instance == null)
        {
            return;
        }

        if (CharacterAnxietySystem.Instance == null)
        {
            NpcInteractionMenuUI.Instance.ShowStatusText("Sistema de ansiedad no configurado.");
            return;
        }

        NpcInteractionMenuUI.Instance.ShowStatusText(CharacterAnxietySystem.Instance.GetFormattedStatus(npcId));
    }

    private string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(npcDisplayName))
        {
            return npcDisplayName;
        }

        if (!string.IsNullOrWhiteSpace(npcId))
        {
            return npcId;
        }

        return gameObject.name;
    }
}
