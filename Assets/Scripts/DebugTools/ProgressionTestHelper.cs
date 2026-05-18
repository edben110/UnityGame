using UnityEngine;

/// <summary>
/// Herramienta de testing para bypass temporal de puzzles.
/// Permite probar la progresión completa del juego sin resolver puzzles.
/// 
/// USO: Activar desde el menú Tools > Debug > [opciones]
/// o desde el Inspector en modo Play.
///
/// IMPORTANTE: Solo para testing. Desactivar antes de build final.
/// </summary>
public class ProgressionTestHelper : MonoBehaviour
{
    [Header("Testing Controls")]
    [SerializeField] private bool enableTestingMode = true;

    [Header("Bypass de Llaves")]
    [Tooltip("Al activar, otorga todas las llaves necesarias al inventario")]
    [SerializeField] private bool grantAllKeys = false;

    [Header("Bypass de Items NPC")]
    [Tooltip("Al activar, otorga todos los objetos de NPC al inventario")]
    [SerializeField] private bool grantAllNpcItems = false;

    [Header("Bypass de Capítulo")]
    [Tooltip("Forzar avance al capítulo especificado")]
    [SerializeField] private string forceChapterId = "";

    [Header("Bypass de Contraseña")]
    [Tooltip("Desbloquea la sala de seguridad sin contraseña")]
    [SerializeField] private bool unlockSecurityRoom = false;

    [Header("Simular Entregas")]
    [Tooltip("Marca todos los items como entregados a NPCs")]
    [SerializeField] private bool simulateAllDeliveries = false;

    [Header("Simular Rescate")]
    [Tooltip("Marca a Simón como rescatado")]
    [SerializeField] private bool simulateSimonRescue = false;

    [Header("Evaluar Final")]
    [Tooltip("Evalúa y muestra qué final se obtendría con el estado actual")]
    [SerializeField] private bool evaluateEnding = false;

    private void Update()
    {
        if (!enableTestingMode) return;

        if (grantAllKeys)
        {
            grantAllKeys = false;
            GrantAllKeys();
        }

        if (grantAllNpcItems)
        {
            grantAllNpcItems = false;
            GrantAllNpcItems();
        }

        if (!string.IsNullOrWhiteSpace(forceChapterId))
        {
            string chapter = forceChapterId;
            forceChapterId = "";
            ForceChapter(chapter);
        }

        if (unlockSecurityRoom)
        {
            unlockSecurityRoom = false;
            UnlockSecurityRoom();
        }

        if (simulateAllDeliveries)
        {
            simulateAllDeliveries = false;
            SimulateAllDeliveries();
        }

        if (simulateSimonRescue)
        {
            simulateSimonRescue = false;
            SimulateSimonRescue();
        }

        if (evaluateEnding)
        {
            evaluateEnding = false;
            EvaluateCurrentEnding();
        }
    }

    /// <summary>
    /// Otorga todas las llaves al inventario (bypass de puzzles).
    /// </summary>
    public void GrantAllKeys()
    {
        string[] keys = { "StudyKey", "BedroomKey", "GalleryKey", "BasementKey", "SmallKey", "SingleUseKey" };

        foreach (string key in keys)
        {
            if (!InventoryState.HasItem(key))
            {
                InventoryState.AddItem(key);

                // Registrar en catálogo
                if (InventoryCatalog.Instance != null)
                {
                    System.Enum.TryParse(key, true, out KeyType keyType);
                    string displayName = KeyTypeDisplayNames.GetDisplayName(keyType);
                    string description = KeyTypeDisplayNames.GetDescription(keyType);
                    InventoryCatalog.Instance.RegisterRuntimeItem(key, displayName, description, null);
                }

                Debug.Log($"[TestHelper] Llave otorgada: {key}");
            }
        }

        Debug.Log("[TestHelper] ★ Todas las llaves otorgadas al inventario.");
    }

    /// <summary>
    /// Otorga todos los objetos de NPC al inventario.
    /// </summary>
    public void GrantAllNpcItems()
    {
        var items = new (string id, string name, string desc)[]
        {
            ("libro_contabilidad", "Libro de Contabilidad", "Entradas marcadas con 'B' roja. Discrepancias financieras."),
            ("carpeta_evidencia", "Carpeta con Evidencia", "Fotografías y documentos del crimen del puerto."),
            ("carta_padre", "Carta del Padre", "Carta manuscrita del padre común de Simón y Robert."),
            ("estuche_joyas", "Estuche de Joyas", "Estuche de cuero con joyas familiares de Simón."),
            ("relicario_lucas", "Relicario de Lucas", "Relicario de plata: 'Para Lucas. Siempre.'")
        };

        foreach (var item in items)
        {
            if (!InventoryState.HasItem(item.id))
            {
                InventoryState.AddItem(item.id);
                if (InventoryCatalog.Instance != null)
                {
                    InventoryCatalog.Instance.RegisterRuntimeItem(item.id, item.name, item.desc, null);
                }
                Debug.Log($"[TestHelper] Item NPC otorgado: {item.id}");
            }
        }

        Debug.Log("[TestHelper] ★ Todos los objetos NPC otorgados al inventario.");
    }

    /// <summary>
    /// Fuerza el avance a un capítulo específico.
    /// </summary>
    public void ForceChapter(string chapterId)
    {
        if (StoryState.Instance == null)
        {
            Debug.LogError("[TestHelper] No hay StoryState.");
            return;
        }

        StoryState.Instance.SetChapter(chapterId);
        StoryState.Instance.SetFlag($"{chapterId}.intro.seen", true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockChapter(chapterId);
        }

        Debug.Log($"[TestHelper] ★ Capítulo forzado a: {chapterId}");
    }

    /// <summary>
    /// Desbloquea la sala de seguridad sin contraseña.
    /// </summary>
    public void UnlockSecurityRoom()
    {
        if (StoryState.Instance == null)
        {
            Debug.LogError("[TestHelper] No hay StoryState.");
            return;
        }

        StoryState.Instance.SetFlag("SecurityRoom.Unlocked", true);

        NumericPasswordPanel panel = FindAnyObjectByType<NumericPasswordPanel>();
        if (panel != null)
        {
            panel.ForceUnlock();
        }

        Debug.Log("[TestHelper] ★ Sala de seguridad desbloqueada (bypass).");
    }

    /// <summary>
    /// Simula que todos los items fueron entregados a NPCs.
    /// </summary>
    public void SimulateAllDeliveries()
    {
        if (StoryState.Instance == null) return;

        string[] npcIds = { "ben", "lisa", "robert", "ana", "lucas" };
        foreach (string npc in npcIds)
        {
            StoryState.Instance.SetFlag($"npc.delivery.{npc}.completed", true);
            StoryState.Instance.SetFlag($"ending.{npc}.item_delivered", true);
        }

        StoryState.Instance.SetFlag("NpcDeliveryMode", true);
        StoryState.Instance.SetFlag("chapter4.npc_delivery.activated", true);

        Debug.Log("[TestHelper] ★ Todas las entregas NPC simuladas como completadas.");
    }

    /// <summary>
    /// Simula que Simón fue rescatado.
    /// </summary>
    public void SimulateSimonRescue()
    {
        if (StoryState.Instance == null) return;

        StoryState.Instance.SetFlag("chapter5.simon.rescued", true);
        StoryState.Instance.SetFlag("chapter5.found.simon", true);
        StoryState.Instance.SetFlag("chapter5.chose.door_simon", true);

        Debug.Log("[TestHelper] ★ Rescate de Simón simulado.");
    }

    /// <summary>
    /// Evalúa qué final se obtendría con el estado actual.
    /// </summary>
    public void EvaluateCurrentEnding()
    {
        EpilogueSystem epilogue = FindAnyObjectByType<EpilogueSystem>();
        if (epilogue == null)
        {
            Debug.LogError("[TestHelper] No hay EpilogueSystem en la escena.");
            return;
        }

        var ending = epilogue.EvaluateEnding();
        string summary = epilogue.GetEndingVariablesSummary();
        Debug.Log($"[TestHelper] ★ Final evaluado: {ending}\n{summary}");
    }

    /// <summary>
    /// Setup completo para testing de Cap 4 → Cap 5.
    /// Otorga todo lo necesario para probar la transición.
    /// </summary>
    public void SetupFullChapter4To5Test()
    {
        GrantAllKeys();
        GrantAllNpcItems();
        UnlockSecurityRoom();

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetChapter("chapter4");
            StoryState.Instance.SetFlag("chapter4.intro.seen", true);
            StoryState.Instance.SetFlag("BasementDiscovered", true);
            StoryState.Instance.SetFlag("chapter.chapter3.complete", true);

            // Simular que habló con al menos 1 NPC
            StoryState.Instance.SetFlag("chapter.chapter4.npc.talked.ben", true);
            if (ChapterFlowController.Instance != null)
            {
                ChapterFlowController.Instance.RegisterNpcTalked("ben");
            }
        }

        Debug.Log("[TestHelper] ★ Setup completo para test Cap 4 → Cap 5.");
    }
}
