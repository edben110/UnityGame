using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de epílogo que determina y ejecuta el final del juego.
/// 
/// FINALES (del flujoHistoria.txt):
/// 
/// FINAL A — TODOS SOBREVIVEN
///   Condiciones: Entregar objetos a NPCs + Rescatar a Simón
///   Epílogo: "A veces la verdad no mata. A veces salva."
///
/// FINAL B — NO TODOS SOBREVIVEN
///   Condiciones: Al menos 1 NPC con ansiedad 100 (muerto) O Simón no rescatado
///   Epílogo: "El silencio mata más lento que un cuchillo. Pero mata igual."
///
/// FINAL C — EL PROTAGONISTA ES EL CULPABLE
///   Condiciones: No entregar objetos + Elegir "Buscar al asesino" + Todos los NPCs mueren
///   Epílogo: "La mansión guarda sus secretos. Y los de quien la visita."
///
/// VARIABLES CRÍTICAS:
///   - ending.ben.item_delivered / ending.lisa.item_delivered / etc.
///   - PlayerRefusedNpc.ben / PlayerRefusedNpc.lisa / etc.
///   - chapter4.decision.rescue_simon / chapter4.decision.stay_group
///   - SingleUseKey.consumed.door.X (qué puerta eligió)
///   - npc.dead.X (NPCs muertos por ansiedad)
///   - chapter5.simon.rescued (si rescató a Simón)
///   - chapter5.choice.buscar_asesino (si eligió buscar al asesino)
/// </summary>
public class EpilogueSystem : MonoBehaviour
{
    public static EpilogueSystem Instance { get; private set; }

    public enum EndingType
    {
        None,
        FinalA_AllSurvive,
        FinalB_NotAllSurvive,
        FinalC_ProtagonistGuilty
    }

    [Header("Configuración")]
    [SerializeField] private string epilogueConversationPrefix = "epilogue_";

    private EndingType determinedEnding = EndingType.None;

    public EndingType DeterminedEnding => determinedEnding;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Evalúa las condiciones y determina qué final corresponde.
    /// Debe llamarse al completar el Capítulo 5.
    /// </summary>
    public EndingType EvaluateEnding()
    {
        if (StoryState.Instance == null)
        {
            Debug.LogError("[Epilogue] No hay StoryState. No se puede evaluar final.");
            return EndingType.FinalB_NotAllSurvive;
        }

        bool allItemsDelivered = AreAllItemsDelivered();
        bool anyItemDelivered = AnyItemDelivered();
        bool simonRescued = IsSimonRescued();
        int deadNpcCount = GetDeadNpcCount();
        bool allNpcsDead = deadNpcCount >= 5;
        bool anyNpcDead = deadNpcCount > 0;
        bool choseBuscarAsesino = StoryState.Instance.HasFlag("chapter5.choice.buscar_asesino");
        bool noItemsDelivered = !anyItemDelivered;

        Debug.Log($"[Epilogue] === EVALUACIÓN DE FINAL ===");
        Debug.Log($"  AllItemsDelivered: {allItemsDelivered}");
        Debug.Log($"  AnyItemDelivered: {anyItemDelivered}");
        Debug.Log($"  SimonRescued: {simonRescued}");
        Debug.Log($"  DeadNPCs: {deadNpcCount}/5");
        Debug.Log($"  ChoseBuscarAsesino: {choseBuscarAsesino}");

        // FINAL C: No entregar objetos + Buscar al asesino + Todos mueren
        if (noItemsDelivered && choseBuscarAsesino && allNpcsDead)
        {
            determinedEnding = EndingType.FinalC_ProtagonistGuilty;
            Debug.Log("[Epilogue] ★ FINAL C — El protagonista es el culpable");
            return determinedEnding;
        }

        // FINAL A: Entregar objetos + Rescatar a Simón + Ningún NPC muerto
        if (allItemsDelivered && simonRescued && !anyNpcDead)
        {
            determinedEnding = EndingType.FinalA_AllSurvive;
            Debug.Log("[Epilogue] ★ FINAL A — Todos sobreviven");
            return determinedEnding;
        }

        // FINAL B: Al menos 1 NPC muerto O Simón no rescatado
        determinedEnding = EndingType.FinalB_NotAllSurvive;
        Debug.Log("[Epilogue] ★ FINAL B — No todos sobreviven");
        return determinedEnding;
    }

    /// <summary>
    /// Inicia la secuencia del epílogo con el final determinado.
    /// </summary>
    public void StartEpilogue()
    {
        if (determinedEnding == EndingType.None)
        {
            EvaluateEnding();
        }

        // Guardar el final en StoryState para persistencia
        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag("game.ending.determined", true);
            StoryState.Instance.SetDecision("game.ending.type", determinedEnding.ToString());
        }

        // Lanzar conversación del epílogo
        string conversationId = GetEpilogueConversationId();
        DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
        if (runner != null && !runner.IsRunning && runner.HasConversation(conversationId))
        {
            runner.StartConversation(conversationId, "start");
            Debug.Log($"[Epilogue] Iniciando epílogo: {conversationId}");
        }
        else
        {
            Debug.LogWarning($"[Epilogue] No se encontró conversación '{conversationId}'. Mostrando resumen.");
            ShowFallbackEpilogue();
        }
    }

    private string GetEpilogueConversationId()
    {
        switch (determinedEnding)
        {
            case EndingType.FinalA_AllSurvive:
                return $"{epilogueConversationPrefix}final_a";
            case EndingType.FinalB_NotAllSurvive:
                return $"{epilogueConversationPrefix}final_b";
            case EndingType.FinalC_ProtagonistGuilty:
                return $"{epilogueConversationPrefix}final_c";
            default:
                return $"{epilogueConversationPrefix}final_b";
        }
    }

    private void ShowFallbackEpilogue()
    {
        string message = determinedEnding switch
        {
            EndingType.FinalA_AllSurvive => "A veces la verdad no mata. A veces salva.",
            EndingType.FinalB_NotAllSurvive => "El silencio mata más lento que un cuchillo. Pero mata igual.",
            EndingType.FinalC_ProtagonistGuilty => "La mansión guarda sus secretos. Y los de quien la visita.",
            _ => "Fin."
        };

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  VERIFICACIONES DE ESTADO
    // ═══════════════════════════════════════════════════════════════

    private bool AreAllItemsDelivered()
    {
        string[] npcIds = { "ben", "lisa", "robert", "ana", "lucas" };
        for (int i = 0; i < npcIds.Length; i++)
        {
            if (!StoryState.Instance.HasFlag($"ending.{npcIds[i]}.item_delivered"))
            {
                return false;
            }
        }
        return true;
    }

    private bool AnyItemDelivered()
    {
        string[] npcIds = { "ben", "lisa", "robert", "ana", "lucas" };
        for (int i = 0; i < npcIds.Length; i++)
        {
            if (StoryState.Instance.HasFlag($"ending.{npcIds[i]}.item_delivered"))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsSimonRescued()
    {
        return StoryState.Instance.HasFlag("chapter5.simon.rescued");
    }

    private int GetDeadNpcCount()
    {
        int count = 0;
        string[] npcIds = { "ben", "lisa", "robert", "ana", "lucas" };
        for (int i = 0; i < npcIds.Length; i++)
        {
            if (StoryState.Instance.HasFlag($"npc.dead.{npcIds[i]}"))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Genera un resumen de las variables que afectan el final.
    /// Útil para debug.
    /// </summary>
    public string GetEndingVariablesSummary()
    {
        if (StoryState.Instance == null) return "StoryState no disponible.";

        string[] npcIds = { "ben", "lisa", "robert", "ana", "lucas" };
        List<string> lines = new List<string>();
        lines.Add("=== VARIABLES DE FINAL ===");

        for (int i = 0; i < npcIds.Length; i++)
        {
            string npc = npcIds[i];
            bool delivered = StoryState.Instance.HasFlag($"ending.{npc}.item_delivered");
            bool refused = StoryState.Instance.HasFlag($"PlayerRefusedNpc.{npc}");
            bool dead = StoryState.Instance.HasFlag($"npc.dead.{npc}");
            lines.Add($"  {npc}: delivered={delivered} refused={refused} dead={dead}");
        }

        lines.Add($"  SimonRescued: {IsSimonRescued()}");
        lines.Add($"  SingleUseKeyUsed: {StoryState.Instance.HasFlag("OneTimeKeyUsed")}");
        lines.Add($"  BuscarAsesino: {StoryState.Instance.HasFlag("chapter5.choice.buscar_asesino")}");
        lines.Add($"  DeadNPCs: {GetDeadNpcCount()}/5");
        lines.Add($"  Ending: {determinedEnding}");

        return string.Join("\n", lines);
    }
}
