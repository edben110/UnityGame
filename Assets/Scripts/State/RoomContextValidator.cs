using UnityEngine;

/// <summary>
/// Sistema centralizado de validación de contexto narrativo por habitación.
/// 
/// RESPONSABILIDADES:
///   1. Validar si el jugador está en la misma habitación que un NPC
///   2. Validar si pueden ocurrir diálogos de NPC
///   3. Validar si pueden ocurrir decisiones grupales
///   4. Validar restricciones espaciales de interacciones
///
/// RESTRICCIONES OBLIGATORIAS:
///   - Los NPC solo pueden dialogar/decidir si el jugador está en su misma habitación
///   - Los NPCs permanecen SIEMPRE en la sala de NPCs ("lobby")
///   - El protagonista investiga SOLO
///   - Si el jugador no está en la sala de NPCs, no hay diálogos de NPC
///
/// USO:
///   if (!RoomContextValidator.CanNpcDialogue("ben", "chapter2")) {
///       // Bloquear diálogo
///   }
/// </summary>
public class RoomContextValidator : MonoBehaviour
{
    public static RoomContextValidator Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private string npcRoomId = "lobby";

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
    /// ¿Puede un NPC dialogar con el jugador AHORA?
    /// 
    /// Retorna TRUE solo si:
    ///   - El jugador está en la habitación del NPC (lobby)
    ///   - El NPC está presente
    ///   - Se cumplen validaciones narrativas
    /// </summary>
    public bool CanNpcDialogue(string npcId, string chapterId)
    {
        // Validación 1: ¿El jugador está en la sala correcta?
        if (!IsPlayerInNpcRoom())
        {
            return false;
        }

        // Validación 2: ¿Está el NPC en la sala?
        if (!IsNpcInCurrentRoom(npcId))
        {
            return false;
        }

        // Validación 3: ¿Es el capítulo correcto?
        if (StoryState.Instance != null && StoryState.Instance.CurrentChapterId != chapterId)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// ¿Pueden ocurrir decisiones grupales AHORA?
    /// 
    /// Retorna TRUE solo si:
    ///   - El jugador está en la sala de NPCs
    ///   - Todos los NPCs relevantes están presentes
    ///   - Se cumplen validaciones narrativas
    /// </summary>
    public bool CanGroupDecision(string chapterId)
    {
        // Las decisiones grupales SOLO pueden ocurrir en la sala de NPCs
        if (!IsPlayerInNpcRoom())
        {
            return false;
        }

        // Verificar que estamos en el capítulo correcto
        if (StoryState.Instance != null && StoryState.Instance.CurrentChapterId != chapterId)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// ¿El jugador está actualmente en la sala de NPCs?
    /// </summary>
    public bool IsPlayerInNpcRoom()
    {
        if (RoomManager.Instance == null)
        {
            return false;
        }

        return RoomManager.Instance.CurrentRoomId == npcRoomId;
    }

    /// <summary>
    /// ¿Está el NPC en la habitación actual del jugador?
    /// </summary>
    public bool IsNpcInCurrentRoom(string npcId)
    {
        if (NpcLocationManager.Instance == null || RoomManager.Instance == null)
        {
            return false;
        }

        string npcRoom = NpcLocationManager.Instance.GetNpcRoom(npcId);
        string playerRoom = RoomManager.Instance.CurrentRoomId;

        return npcRoom == playerRoom;
    }

    /// <summary>
    /// Obtener la habitación donde están los NPCs.
    /// </summary>
    public string GetNpcRoomId()
    {
        return npcRoomId;
    }

    /// <summary>
    /// Debug: obtener información de validación.
    /// </summary>
    public void DebugLogValidation(string npcId, string chapterId)
    {
        bool playerInNpcRoom = IsPlayerInNpcRoom();
        bool npcInCurrentRoom = IsNpcInCurrentRoom(npcId);
        bool validChapter = StoryState.Instance != null && StoryState.Instance.CurrentChapterId == chapterId;
        bool canDialogue = CanNpcDialogue(npcId, chapterId);

        Debug.Log($"[RoomContextValidator] NPC '{npcId}' - Cap '{chapterId}':");
        Debug.Log($"  PlayerInNpcRoom: {playerInNpcRoom}");
        Debug.Log($"  NpcInCurrentRoom: {npcInCurrentRoom}");
        Debug.Log($"  ValidChapter: {validChapter}");
        Debug.Log($"  CanDialogue: {canDialogue}");
    }
}
