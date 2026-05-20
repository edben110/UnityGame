using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dispara los finales del juego según puertas visitadas y estado de los NPCs.
///
/// Final 1: Door_ToKidNappedSimon → KidNappedSimon + al menos un NPC vivo → Final_1
/// Final 2: Door_ToKidNappedSimon → KidNappedSimon + todos los NPCs muertos → Final_2
/// Final 3: Door_ToEmptyRoom (marca visita) + Door_ToKillerBunker → killerbunker → Final_3
/// </summary>
public class EndingFlowController : MonoBehaviour
{
    public static EndingFlowController Instance { get; private set; }

    public static bool IsResolvingEnding =>
        Instance != null && Instance.isResolvingEnding;

    private const string FlagGameComplete = "ending.game_complete";
    private const string FlagPlayedPrefix = "ending.played.final_";

    [Header("Puertas / habitaciones")]
    [SerializeField] private string kidnappedSimonDoorName = "Door_ToKidNappedSimon";
    [SerializeField] private string kidnappedSimonRoomId = "KidNappedSimon";
    [SerializeField] private string emptyRoomDoorName = "Door_ToEmptyRoom";
    [SerializeField] private string emptyRoomId = "emptyRoom";
    [SerializeField] private string killerBunkerDoorName = "Door_ToKillerBunker";
    [SerializeField] private string killerBunkerRoomId = "killerbunker";

    [Header("Mensajes post-final")]
    [TextArea(2, 4)]
    [SerializeField] private string final1Epilogue = "Algunos sobrevivieron. La verdad no murió con ellos.";
    [TextArea(2, 4)]
    [SerializeField] private string final2Epilogue = "Nadie quedó. Solo el silencio de la mansión.";
    [TextArea(2, 4)]
    [SerializeField] private string final3Epilogue = "El sótano del asesino guardaba el último secreto.";

    [Header("Créditos")]
    [Tooltip("Escena a cargar al terminar Creditos.")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [Tooltip("Si true, se reproducen créditos después del final y luego vuelve a MenuScene.")]
    [SerializeField] private bool playCreditsAfterEnding = true;

    private bool isResolvingEnding;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Llamado por DoorTrigger tras un cambio de habitación exitoso.
    /// </summary>
    public void OnRoomEntered(string doorObjectName, string roomId)
    {
        if (isResolvingEnding || EndingCinematicPlayer.IsPlaying)
        {
            return;
        }

        if (StoryState.Instance != null && StoryState.Instance.HasFlag(FlagGameComplete))
        {
            return;
        }

        if (StoryState.Instance == null || StoryState.Instance.CurrentChapterId != "chapter5")
        {
            return;
        }

        string door = doorObjectName ?? string.Empty;
        string room = roomId ?? string.Empty;

        // Final 3: ahora se dispara al interactuar con cualquiera de las dos puertas.
        if (Matches(door, emptyRoomDoorName) || Matches(room, emptyRoomId)
            || Matches(door, killerBunkerDoorName) || Matches(room, killerBunkerRoomId))
        {
            PlayEnding(3, final3Epilogue);
            return;
        }

        if (Matches(door, kidnappedSimonDoorName) || Matches(room, kidnappedSimonRoomId))
        {
            TryPlayKidnappedSimonEnding();
        }
    }

    private void TryPlayKidnappedSimonEnding()
    {
        if (HasPlayedEnding(1) || HasPlayedEnding(2))
        {
            return;
        }

        if (AnyNpcAlive())
        {
            PlayEnding(1, final1Epilogue);
            return;
        }

        if (AllNpcsDead())
        {
            PlayEnding(2, final2Epilogue);
        }
    }

    // (Final 3 ya no requiere validación previa.)

    private void PlayEnding(int endingIndex, string epilogueMessage)
    {
        if (isResolvingEnding || HasPlayedEnding(endingIndex))
        {
            return;
        }

        isResolvingEnding = true;
        Debug.Log($"[EndingFlow] Iniciando cinemática Final_{endingIndex}.");

        EndingCinematicPlayer.Play(endingIndex, () => OnEndingCinematicFinished(endingIndex, epilogueMessage));
    }

    private void OnEndingCinematicFinished(int endingIndex, string epilogueMessage)
    {
        isResolvingEnding = false;

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag($"{FlagPlayedPrefix}{endingIndex}", true);
            StoryState.Instance.SetFlag(FlagGameComplete, true);
            StoryState.Instance.SetFlag($"ending.final_{endingIndex}", true);
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null && !string.IsNullOrWhiteSpace(epilogueMessage))
        {
            panel.ShowSystemMessage(epilogueMessage);
        }

        Debug.Log($"[EndingFlow] Final_{endingIndex} completado. Juego finalizado.");

        if (playCreditsAfterEnding)
        {
            // Bloquear interacción del mundo mientras corren créditos (WorldInteractionGate ya lo usa).
            EndingCinematicPlayer.PlayCredits(ReturnToMenuScene);
        }
        else
        {
            ReturnToMenuScene();
        }
    }

    private void ReturnToMenuScene()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[EndingFlow] No hay GameManager. No se puede volver a MenuScene.");
            return;
        }

        // Volver al menú. La UI del menú controla nueva partida/continuar.
        var sceneController = new SceneController();
        sceneController.TryLoadScene(menuSceneName);
    }

    public static bool AnyNpcAlive()
    {
        if (CharacterAnxietySystem.Instance == null)
        {
            return false;
        }

        List<string> ids = CharacterAnxietySystem.Instance.GetCharacterIds();
        for (int i = 0; i < ids.Count; i++)
        {
            if (!CharacterAnxietySystem.Instance.IsDead(ids[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool AllNpcsDead()
    {
        if (CharacterAnxietySystem.Instance == null)
        {
            return true;
        }

        List<string> ids = CharacterAnxietySystem.Instance.GetCharacterIds();
        if (ids.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < ids.Count; i++)
        {
            if (!CharacterAnxietySystem.Instance.IsDead(ids[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasPlayedEnding(int endingIndex)
    {
        if (StoryState.Instance == null)
        {
            return false;
        }

        return StoryState.Instance.HasFlag($"{FlagPlayedPrefix}{endingIndex}");
    }

    private static bool Matches(string value, string expected)
    {
        return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
    }
}
