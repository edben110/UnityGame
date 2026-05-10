using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcInteractable : Interactable
{
    private const string ItemConversationSeparator = "_item_";

    [Header("NPC")]
    [SerializeField] private string npcId;
    [SerializeField] private string npcDisplayName;

    [Header("Flujo")]
    [SerializeField] private List<string> validChapterIds = new List<string> { "chapter1", "chapter2", "chapter3", "chapter4", "chapter5" };
    [SerializeField] private string talkConversationId;
    [SerializeField] private string motivoConversationId;
    [SerializeField] private string criticalTalkConversationId;

    public string NpcId => npcId;

    public bool OpenInteractionForItem(string itemId)
    {
        if (!CanOpenInteraction(out DialogueRunner runner))
        {
            return false;
        }

        if (NpcInteractionMenuUI.Instance == null)
        {
            Debug.LogError("No se encontro NpcInteractionMenuUI en la escena.");
            return false;
        }

        Action askItemAction;
        string askItemLabel;
        bool hasItemAction = TryBuildSpecificItemQuestionAction(runner, itemId, out askItemAction, out askItemLabel);

        NpcInteractionMenuUI.Instance.Show(GetDisplayName(), OnTalkPressed, OnVerifyPressed, hasItemAction ? askItemAction : null, hasItemAction ? askItemLabel : string.Empty);
        TryHandlePendingDisappearances();

        if (!hasItemAction)
        {
            NpcInteractionMenuUI.Instance.ShowStatusText("Este personaje no puede responder sobre ese objeto todavia.");
        }

        return true;
    }

public override void Interact()
    {
        base.Interact();

        if (!CanOpenInteraction(out DialogueRunner runner))
        {
            return;
        }

        if (NpcInteractionMenuUI.Instance == null)
        {
            Debug.LogError("No se encontro NpcInteractionMenuUI en la escena.");
            return;
        }

        Action askItemAction;
        string askItemLabel;
        TryBuildItemQuestionAction(runner, out askItemAction, out askItemLabel);

        // Construir accion de motivo si aun no se ha visto
        Action motivoAction = null;
        string motivoLabel = string.Empty;
        if (!string.IsNullOrWhiteSpace(motivoConversationId)
            && runner.HasConversation(motivoConversationId)
            && StoryState.Instance != null
            && !StoryState.Instance.HasFlag($"npc.motivo.seen.{npcId}"))
        {
            string capturedMotivoId = motivoConversationId;
            motivoAction = () => OnMotivoPressed(capturedMotivoId);
            motivoLabel = "\u00bfPor qu\u00e9 est\u00e1s aqu\u00ed?";
        }

        NpcInteractionMenuUI.Instance.Show(GetDisplayName(), OnTalkPressed, OnVerifyPressed, askItemAction ?? motivoAction, askItemLabel.Length > 0 ? askItemLabel : motivoLabel);
        TryHandlePendingDisappearances();
    }

    private bool CanOpenInteraction(out DialogueRunner runner)
    {
        runner = null;

        if (StoryState.Instance == null)
        {
            return false;
        }

        if (validChapterIds != null && validChapterIds.Count > 0 && !validChapterIds.Contains(StoryState.Instance.CurrentChapterId))
        {
            return false;
        }

        if (StoryState.Instance.HasFlag($"npc.dead.{npcId}"))
        {
            return false;
        }

        // Restricción espacial deshabilitada temporalmente
        // TODO: Reimplementar cuando el sistema de salas esté estable
        // Los NPCs solo deberían responder en la sala donde están físicamente

        runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null || runner.IsRunning)
        {
            return false;
        }

        return true;
    }

    private void OnMotivoPressed(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return;
        }

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            return;
        }

        bool started = runner.StartConversation(conversationId, "start");
        if (!started)
        {
            return;
        }

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag($"npc.motivo.seen.{npcId}", true);
        }

        NpcInteractionMenuUI.Instance.Hide();
    }

    
private void OnTalkPressed()
    {
        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            return;
        }

        if (CharacterAnxietySystem.Instance != null && CharacterAnxietySystem.Instance.IsAtMaxAnxiety(npcId))
        {
            string criticalConversationId = ResolveCriticalConversationId(runner);
            bool startedCritical = !string.IsNullOrWhiteSpace(criticalConversationId)
                                 && runner.HasConversation(criticalConversationId)
                                 && runner.StartConversation(criticalConversationId, "start");

            if (!startedCritical)
            {
                NpcInteractionMenuUI.Instance.ShowStatusText("Su ansiedad es extrema. Solo balbucea frases nerviosas e incoherentes.");
                return;
            }

            NpcInteractionMenuUI.Instance.Hide();
            return;
        }

        // Intentar primero la conversación específica del capítulo actual
        string resolvedTalkId = ResolveTalkConversationId(runner);
        bool started = runner.StartConversation(resolvedTalkId, "start");
        if (!started)
        {
            return;
        }

        // Registrar que el jugador habló con este NPC
        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag($"npc.talked.{npcId}", true);
            Debug.Log($"[NPC] Jugador habló con {npcId}. Flag 'npc.talked.{npcId}' seteado.");
        }

        if (CharacterAnxietySystem.Instance != null)
        {
            CharacterAnxietySystem.Instance.ApplyTalkRelief(npcId);
        }

        NpcInteractionMenuUI.Instance.Hide();
    }

    /// <summary>
    /// Resuelve el ID de conversación de ansiedad según el capítulo actual.
    /// Intenta chapter2_npc_robert, luego cae a chapter1_npc_robert.
    /// </summary>
    private string ResolveTalkConversationId(DialogueRunner runner)
    {
        string currentChapter = StoryState.Instance != null ? StoryState.Instance.CurrentChapterId : "chapter1";
        string chapterSpecific = $"{currentChapter}_npc_{npcId}";

        if (runner.HasConversation(chapterSpecific))
        {
            return chapterSpecific;
        }

        return talkConversationId;
    }

    private string ResolveCriticalConversationId(DialogueRunner runner)
    {
        // Intentar primero el crítico del capítulo actual
        string currentChapter = StoryState.Instance != null ? StoryState.Instance.CurrentChapterId : "chapter1";
        string chapterCritical = $"{currentChapter}_npc_{npcId}_critical";

        if (runner.HasConversation(chapterCritical))
        {
            return chapterCritical;
        }

        if (!string.IsNullOrWhiteSpace(criticalTalkConversationId))
        {
            return criticalTalkConversationId;
        }

        if (string.IsNullOrWhiteSpace(talkConversationId))
        {
            return string.Empty;
        }

        return $"{talkConversationId}_critical";
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
            AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
            if (anxietySystem != null)
            {
                anxietySystem.HideVerificationOverlay();
            }
            return;
        }

        float anxiety = CharacterAnxietySystem.Instance.GetAnxiety(npcId);
        NpcInteractionMenuUI.Instance.ShowStatusText(CharacterAnxietySystem.Instance.GetFormattedStatus(npcId));

        AnxietySystem overlaySystem = FindAnyObjectByType<AnxietySystem>();
        if (overlaySystem != null)
        {
            overlaySystem.ShowVerificationOverlay(anxiety / 100f);
        }
    }

    private void OnAskItemPressed(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return;
        }

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
        {
            return;
        }

        bool started = runner.StartConversation(conversationId, "start");
        if (!started)
        {
            return;
        }

        AnxietySystem anxietySystem = FindAnyObjectByType<AnxietySystem>();
        if (anxietySystem != null)
        {
            anxietySystem.HideVerificationOverlay();
        }

        NpcInteractionMenuUI.Instance.Hide();
    }

    private void TryBuildItemQuestionAction(DialogueRunner runner, out Action action, out string label)
    {
        action = null;
        label = string.Empty;

        if (runner == null || string.IsNullOrWhiteSpace(talkConversationId))
        {
            return;
        }

        List<string> items = InventoryState.GetItems();
        for (int i = 0; i < items.Count; i++)
        {
            string itemId = items[i];
            if (string.IsNullOrWhiteSpace(itemId))
            {
                continue;
            }

            if (!TryBuildSpecificItemQuestionAction(runner, itemId, out action, out label))
            {
                continue;
            }

            return;
        }
    }

    private bool TryBuildSpecificItemQuestionAction(DialogueRunner runner, string itemId, out Action action, out string label)
    {
        action = null;
        label = string.Empty;

        if (runner == null || string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        string conversationId = BuildItemConversationId(itemId);
        if (!runner.HasConversation(conversationId))
        {
            return false;
        }

        string capturedConversationId = conversationId;
        action = () => OnAskItemPressed(capturedConversationId);
        label = BuildItemQuestionLabel(itemId);
        return true;
    }

    private string BuildItemConversationId(string itemId)
    {
        return $"{talkConversationId}{ItemConversationSeparator}{NormalizeItemId(itemId)}";
    }

    private static string NormalizeItemId(string itemId)
    {
        return string.IsNullOrWhiteSpace(itemId)
            ? string.Empty
            : itemId.Trim().ToLowerInvariant();
    }

    private static string BuildItemQuestionLabel(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return "Preguntar por objeto";
        }

        if (InventoryCatalog.Instance != null)
        {
            string displayName = InventoryCatalog.Instance.GetDisplayNameOrFallback(itemId);
            return $"Preguntar por {displayName}";
        }

        if (itemId.Contains("foto", StringComparison.OrdinalIgnoreCase))
        {
            return "Preguntar por la foto";
        }

        return $"Preguntar por {itemId.Replace('_', ' ')}";
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

    private void TryHandlePendingDisappearances()
    {
        if (StoryState.Instance == null || CharacterAnxietySystem.Instance == null || NpcInteractionMenuUI.Instance == null)
        {
            return;
        }

        List<string> allIds = CharacterAnxietySystem.Instance.GetCharacterIds();
        for (int i = 0; i < allIds.Count; i++)
        {
            string missingId = allIds[i];
            if (!StoryState.Instance.HasFlag($"npc.disappearance.pending.{missingId}"))
            {
                continue;
            }

            if (string.Equals(missingId, npcId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string reportFlag = $"npc.disappearance.reported.{missingId}.{npcId}";
            if (StoryState.Instance.HasFlag(reportFlag))
            {
                continue;
            }

            string missingName = CharacterAnxietySystem.Instance.GetCharacterDisplayName(missingId);
            NpcInteractionMenuUI.Instance.ShowStatusText($"{missingName} se march\u00f3 con un aspecto nervioso. Nadie lo vio volver.");
            StoryState.Instance.SetFlag(reportFlag, true);
            EvaluateDisappearanceConsensus(missingId);

            TryStartReactionConversation(missingId);
            return;
        }
    }

    private void TryStartReactionConversation(string missingId)
    {
        string currentChapter = StoryState.Instance != null ? StoryState.Instance.CurrentChapterId : "chapter1";
        string conversationId = $"{currentChapter}_npc_{npcId}_reaction_{missingId}";

        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null || runner.IsRunning)
        {
            return;
        }

        if (!runner.HasConversation(conversationId))
        {
            conversationId = $"chapter1_npc_reaction_generic_{missingId}";
            if (!runner.HasConversation(conversationId))
            {
                return;
            }
        }

        NpcInteractionMenuUI.Instance.Hide();
        runner.StartConversation(conversationId, "start");
    }

    private static void EvaluateDisappearanceConsensus(string missingId)
    {
        if (StoryState.Instance == null || CharacterAnxietySystem.Instance == null)
        {
            return;
        }

        List<string> aliveIds = CharacterAnxietySystem.Instance.GetAliveCharacterIds();
        bool allReported = true;
        for (int i = 0; i < aliveIds.Count; i++)
        {
            string aliveId = aliveIds[i];
            if (string.Equals(aliveId, missingId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string reportFlag = $"npc.disappearance.reported.{missingId}.{aliveId}";
            if (!StoryState.Instance.HasFlag(reportFlag))
            {
                allReported = false;
                break;
            }
        }

        if (!allReported)
        {
            return;
        }

        StoryState.Instance.SetFlag($"npc.cadaver.ready.{missingId}", true);
        StoryState.Instance.SetFlag($"npc.disappearance.pending.{missingId}", false);

        if (CadaverManager.Instance != null)
        {
            string cadaverRoom = CadaverManager.Instance.GetCadaverRoom(missingId);
            if (string.IsNullOrWhiteSpace(cadaverRoom) || cadaverRoom == "missing")
            {
                string resolvedRoom = CadaverManager.ResolvePreferredCadaverRoom(missingId);
                CadaverManager.Instance.AssignCadaverRoom(missingId, resolvedRoom);
            }
        }

        string displayName = CharacterAnxietySystem.Instance.GetCharacterDisplayName(missingId);
        Debug.Log($"[NpcInteractable] Consenso alcanzado: cadaver de '{displayName}' listo para aparecer.");
    }
}
