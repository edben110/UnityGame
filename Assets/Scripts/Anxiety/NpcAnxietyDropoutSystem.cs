using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Si un NPC alcanza ansiedad maxima y el jugador cambia de sala,
/// el NPC se marca como desaparecido y deja de mostrarse.
/// </summary>
public class NpcAnxietyDropoutSystem : MonoBehaviour
{
    [SerializeField] private string hiddenRoomId = "missing";

    private void OnEnable()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged += HandleRoomChanged;
        }
    }

    private void OnDisable()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= HandleRoomChanged;
        }
    }

    private void Start()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.RoomChanged -= HandleRoomChanged;
            RoomManager.Instance.RoomChanged += HandleRoomChanged;
        }
    }

    private void HandleRoomChanged(string previousRoom, string newRoom)
    {
        if (string.IsNullOrWhiteSpace(previousRoom)
            || CharacterAnxietySystem.Instance == null
            || NpcLocationManager.Instance == null
            || StoryState.Instance == null)
        {
            return;
        }

        List<string> ids = CharacterAnxietySystem.Instance.GetCharacterIds();
        for (int i = 0; i < ids.Count; i++)
        {
            string npcId = ids[i];
            if (string.IsNullOrWhiteSpace(npcId))
            {
                continue;
            }

            if (StoryState.Instance.HasFlag($"npc.dead.{npcId}"))
            {
                continue;
            }

            if (!CharacterAnxietySystem.Instance.IsAtMaxAnxiety(npcId))
            {
                continue;
            }

            string npcRoom = NpcLocationManager.Instance.GetNpcRoom(npcId);
            if (!string.Equals(npcRoom, previousRoom, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            NpcLocationManager.Instance.MoveNpc(npcId, hiddenRoomId);
            StoryState.Instance.SetFlag($"npc.dead.{npcId}", true);
            StoryState.Instance.SetFlag("npc.dead.any", true);
            StoryState.Instance.SetFlag($"npc.disappearance.pending.{npcId}", true);
            StoryState.Instance.SetFlag($"npc.cadaver.ready.{npcId}", false);

            string name = CharacterAnxietySystem.Instance.GetCharacterDisplayName(npcId);
            Debug.Log($"[NpcAnxietyDropoutSystem] {name} desaparecio tras abandonar la sala {previousRoom}.");
        }
    }
}
