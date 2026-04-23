using UnityEngine;

public class MapHotspot : Interactable
{
    [Header("Identidad")]
    [SerializeField] private string hotspotId;

    [Header("Flujo narrativo")]
    [SerializeField] private string requiredChapterId = "chapter1";
    [SerializeField] private string conversationId;
    [SerializeField] private string startNodeId = "start";

    [Header("Estado")]
    [SerializeField] private bool consumeAfterUse = false;
    [SerializeField] private string setFlagOnInteract;

    public override void Interact()
    {
        base.Interact();

        if (StoryState.Instance == null)
        {
            Debug.LogWarning("MapHotspot no puede continuar sin StoryState.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(requiredChapterId) && StoryState.Instance.CurrentChapterId != requiredChapterId)
        {
            return;
        }

        if (consumeAfterUse && StoryState.Instance.HasFlag(GetUsedFlag()))
        {
            return;
        }

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            Debug.LogError("No se encontro DialogueRunner en escena.");
            return;
        }

        if (runner.IsRunning)
        {
            return;
        }

        bool started = runner.StartConversation(conversationId, string.IsNullOrWhiteSpace(startNodeId) ? "start" : startNodeId);
        if (!started)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(setFlagOnInteract))
        {
            StoryState.Instance.SetFlag(setFlagOnInteract, true);
        }

        if (consumeAfterUse)
        {
            StoryState.Instance.SetFlag(GetUsedFlag(), true);
        }
    }

    private string GetUsedFlag()
    {
        string id = string.IsNullOrWhiteSpace(hotspotId) ? gameObject.name : hotspotId;
        return $"hotspot.used.{id}";
    }
}
