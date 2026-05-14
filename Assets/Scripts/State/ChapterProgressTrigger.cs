using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Monitorea el progreso del jugador en cada capítulo y dispara
/// la conversación de decisión cuando se cumplen las condiciones.
/// 
/// Capítulo 1: Después de explorar al menos 2 hotspots del lobby
///             Y hablar con al menos 2 NPCs, se dispara chapter1_decision.
/// IMPORTANTE:
/// Este trigger legacy solo se mantiene para Capítulo 1.
/// Capítulo 2 y Capítulo 3 están controlados por ChapterFlowController + DoorTrigger
/// para evitar disparos prematuros y softlocks.
/// </summary>
public class ChapterProgressTrigger : MonoBehaviour
{
    [Header("Configuración Cap 1")]
    [SerializeField] private int chapter1RequiredHotspots = 2;

    private DialogueRunner dialogueRunner;
    private bool chapter1DecisionTriggered;

    // Flags de hotspots del lobby (Cap 1)
    private static readonly string[] chapter1HotspotFlags = new[]
    {
        "clue.lobby.book",
        "clue.lobby.coat",
        "clue.lobby.photo",
        "clue.lobby.newspaper"
    };

    private void Start()
    {
        dialogueRunner = FindAnyObjectByType<DialogueRunner>();

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
        if (StoryState.Instance == null || dialogueRunner == null || dialogueRunner.IsRunning)
        {
            return;
        }

        string chapter = StoryState.Instance.CurrentChapterId;

        switch (chapter)
        {
            case "chapter1":
                TryTriggerChapter1Decision();
                break;
        }
    }

    private void TryTriggerChapter1Decision()
    {
        if (chapter1DecisionTriggered)
        {
            return;
        }

        if (StoryState.Instance.HasFlag("chapter.chapter1.complete"))
        {
            return;
        }

        int hotspotsFound = CountFlags(chapter1HotspotFlags);

        if (hotspotsFound >= chapter1RequiredHotspots)
        {
            chapter1DecisionTriggered = true;
            Debug.Log($"[ChapterProgressTrigger] Cap 1 decisión disparada. Hotspots explorados: {hotspotsFound}/{chapter1RequiredHotspots}");
            Invoke(nameof(LaunchChapter1Decision), 0.5f);
        }
    }

    private void LaunchChapter1Decision()
    {
        if (dialogueRunner != null && !dialogueRunner.IsRunning)
        {
            dialogueRunner.StartConversation("chapter1_decision", "start");
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
