using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Al alcanzar ansiedad 100, muestra aviso, cinemática y retira al personaje del mapa.
/// </summary>
public class CharacterAnxietyDeathDirector : MonoBehaviour
{
    public static CharacterAnxietyDeathDirector Instance { get; private set; }

    private readonly HashSet<string> processingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private Coroutine activeSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void TryStartSeparationSequence(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId) || processingIds.Contains(characterId))
        {
            return;
        }

        if (CharacterAnxietySystem.Instance != null && CharacterAnxietySystem.Instance.IsDead(characterId))
        {
            return;
        }

        if (StoryState.Instance != null && StoryState.Instance.HasFlag($"npc.cinematic.played.{characterId}"))
        {
            if (NpcLocationManager.Instance != null)
            {
                NpcLocationManager.Instance.HideNpcFromMap(characterId);
            }

            ApplySeparation(characterId, playCinematic: false);
            return;
        }

        if (activeSequence != null)
        {
            return;
        }

        activeSequence = StartCoroutine(RunSeparationSequence(characterId));
    }

    private IEnumerator RunSeparationSequence(string characterId)
    {
        processingIds.Add(characterId);
        GameInputBlocker.Block();

        if (NpcLocationManager.Instance != null)
        {
            NpcLocationManager.Instance.HideNpcFromMap(characterId);
        }

        string displayName = CharacterAnxietySystem.Instance != null
            ? CharacterAnxietySystem.Instance.GetCharacterDisplayName(characterId)
            : characterId;

        bool introCompleted = false;
        AnxietySeparationIntroUI.Show(displayName, () => introCompleted = true);
        while (!introCompleted)
        {
            yield return null;
        }

        bool cinematicCompleted = false;
        AnxietyDeathCinematicPlayer.Play(characterId, () => cinematicCompleted = true);
        while (!cinematicCompleted)
        {
            yield return null;
        }

        ApplySeparation(characterId, playCinematic: true);
        processingIds.Remove(characterId);
        activeSequence = null;
        GameInputBlocker.Unblock();
    }

    public static void ApplySeparation(string characterId, bool playCinematic)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return;
        }

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag($"npc.dead.{characterId}", true);
            StoryState.Instance.SetFlag($"npc.separated.{characterId}", true);
            StoryState.Instance.SetFlag("npc.dead.any", true);
            StoryState.Instance.SetFlag($"npc.disappearance.pending.{characterId}", true);
            StoryState.Instance.SetFlag($"npc.cadaver.ready.{characterId}", false);

            if (playCinematic)
            {
                StoryState.Instance.SetFlag($"npc.cinematic.played.{characterId}", true);
            }
        }

        CharacterGroupStateRepository.SetInGroup(characterId, false);
        if (playCinematic)
        {
            CharacterGroupStateRepository.SetCinematicPlayed(characterId, true);
        }

        CharacterGroupStateRepository.SetAnxiety(characterId, 100f);

        if (NpcLocationManager.Instance != null)
        {
            NpcLocationManager.Instance.HideNpcFromMap(characterId);
        }

        CharacterGroupStateRepository.Save();

        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.Hide();
        }

        Debug.Log($"[CharacterAnxietyDeathDirector] {characterId} se separo del grupo.");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(CharacterAnxietyDeathDirector));
        host.AddComponent<CharacterAnxietyDeathDirector>();
    }
}
