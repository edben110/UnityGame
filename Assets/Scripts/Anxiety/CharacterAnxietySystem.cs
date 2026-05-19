using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterAnxietySystem : MonoBehaviour
{
    [Serializable]
    public class CharacterAnxietyEntry
    {
        public string id;
        public string displayName;
        [Range(0f, 100f)] public float anxiety = 10f;
        [Range(0f, 30f)] public float increasePerMinute = 15f;
        [Range(0f, 50f)] public float talkReduction = 18f;
        [Range(0f, 100f)] public float criticalThreshold = 75f;
    }

    public static CharacterAnxietySystem Instance { get; private set; }

    [Header("Configuracion")]
    [SerializeField] private List<string> activeChapterIds = new List<string> { "chapter1", "chapter2", "chapter3", "chapter4", "chapter5" };
    [SerializeField] private List<CharacterAnxietyEntry> characters = new List<CharacterAnxietyEntry>();
    [SerializeField] private TMP_Text debugStatusText;

    private readonly Dictionary<string, CharacterAnxietyEntry> lookup = new Dictionary<string, CharacterAnxietyEntry>();

    public event Action<string, float> CharacterAnxietyChanged;

    private bool subscribedToRoomChanges;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RebuildLookup();
        CharacterGroupStateRepository.Load();
        ApplyRepositoryToCharacters();
    }

    private void Start()
    {
        EnsureDeadNpcsAreHidden();
        SubscribeToRoomChanges();
        ValidateAnxietyLevels();
    }

    private void OnEnable()
    {
        SubscribeToRoomChanges();
    }

    private void OnDisable()
    {
        UnsubscribeFromRoomChanges();
    }

    private void Update()
    {
        if (StoryState.Instance == null)
        {
            return;
        }

        if (!IsAnxietyActiveForCurrentChapter())
        {
            return;
        }

        float deltaMinutes = Time.deltaTime / 60f;
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterAnxietyEntry entry = characters[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.id))
            {
                continue;
            }

            if (IsDead(entry.id))
            {
                continue;
            }

            float before = entry.anxiety;
            entry.anxiety = Mathf.Clamp(entry.anxiety + entry.increasePerMinute * deltaMinutes, 0f, 100f);
            if (!Mathf.Approximately(before, entry.anxiety))
            {
                PersistCharacterState(entry);
                CharacterAnxietyChanged?.Invoke(entry.id, entry.anxiety);
            }
        }

        if (debugStatusText != null)
        {
            debugStatusText.text = BuildFullStatusText();
        }
    }

    public bool HasCharacter(string characterId)
    {
        return !string.IsNullOrWhiteSpace(characterId) && lookup.ContainsKey(characterId);
    }

    public float GetAnxiety(string characterId)
    {
        if (!lookup.TryGetValue(characterId, out CharacterAnxietyEntry entry) || entry == null)
        {
            return 0f;
        }

        return entry.anxiety;
    }

    public string GetFormattedStatus(string characterId)
    {
        if (!lookup.TryGetValue(characterId, out CharacterAnxietyEntry entry) || entry == null)
        {
            return "Personaje no configurado.";
        }

        string mood = GetMoodLabel(entry.anxiety, entry.criticalThreshold);
        return $"{GetDisplayName(entry)} - Ansiedad {Mathf.RoundToInt(entry.anxiety)}/100 ({mood})";
    }

    public bool IsAtMaxAnxiety(string characterId)
    {
        return GetAnxiety(characterId) >= 99.9f;
    }

    public bool IsDead(string characterId)
    {
        if (StoryState.Instance == null || string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        return StoryState.Instance.HasFlag($"npc.dead.{characterId}");
    }

    public string GetCharacterDisplayName(string characterId)
    {
        if (!lookup.TryGetValue(characterId, out CharacterAnxietyEntry entry) || entry == null)
        {
            return characterId;
        }

        return GetDisplayName(entry);
    }

    public List<string> GetCharacterIds()
    {
        List<string> ids = new List<string>(lookup.Count);
        foreach (var pair in lookup)
        {
            ids.Add(pair.Key);
        }

        return ids;
    }

    public List<string> GetAliveCharacterIds()
    {
        List<string> ids = GetCharacterIds();
        if (StoryState.Instance == null)
        {
            return ids;
        }

        ids.RemoveAll(IsDead);
        return ids;
    }

    public void ApplyTalkRelief(string characterId)
    {
        if (!lookup.TryGetValue(characterId, out CharacterAnxietyEntry entry) || entry == null)
        {
            return;
        }

        // Si la ansiedad ya esta al maximo (100%), no se puede reducir hablando.
        // El personaje esta demasiado perturbado para responder con normalidad.
        if (entry.anxiety >= 99.9f)
        {
            Debug.Log($"[CharacterAnxietySystem] {GetDisplayName(entry)}: ansiedad maxima, hablar no ayuda.");
            return;
        }

        entry.anxiety = Mathf.Clamp(entry.anxiety - entry.talkReduction, 0f, 100f);
        PersistCharacterState(entry);
        CharacterAnxietyChanged?.Invoke(entry.id, entry.anxiety);

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag($"npc.talked.{entry.id}", true);
            if (entry.anxiety < entry.criticalThreshold)
            {
                StoryState.Instance.SetFlag($"npc.critical.{entry.id}", false);
            }
        }
    }

    public void RebuildLookup()
    {
        lookup.Clear();
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterAnxietyEntry entry = characters[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.id))
            {
                continue;
            }

            lookup[entry.id] = entry;
        }
    }

    public void ApplySavedAnxiety(string characterId, float anxietyValue)
    {
        if (!lookup.TryGetValue(characterId, out CharacterAnxietyEntry entry) || entry == null)
        {
            return;
        }

        entry.anxiety = Mathf.Clamp(anxietyValue, 0f, 100f);
    }

    private void ApplyRepositoryToCharacters()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterAnxietyEntry entry = characters[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.id))
            {
                continue;
            }

            if (CharacterGroupStateRepository.TryGet(entry.id, out CharacterGroupStateEntry saved))
            {
                entry.anxiety = Mathf.Clamp(saved.anxiety, 0f, 100f);

                if (!saved.isInGroup)
                {
                    CharacterAnxietyDeathDirector.ApplySeparation(entry.id, saved.cinematicPlayed);
                }
            }
            else
            {
                PersistCharacterState(entry);
            }
        }
    }

    private static void PersistCharacterState(CharacterAnxietyEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.id))
        {
            return;
        }

        CharacterGroupStateRepository.SetAnxiety(entry.id, entry.anxiety);
    }

    private void EnsureDeadNpcsAreHidden()
    {
        if (NpcLocationManager.Instance == null)
        {
            return;
        }

        List<string> ids = GetCharacterIds();
        for (int i = 0; i < ids.Count; i++)
        {
            string characterId = ids[i];
            if (!IsDead(characterId))
            {
                continue;
            }

            if (!string.Equals(NpcLocationManager.Instance.GetNpcRoom(characterId), "missing", StringComparison.OrdinalIgnoreCase))
            {
                NpcLocationManager.Instance.MoveNpc(characterId, "missing");
            }
        }

        NpcLocationManager.Instance.RefreshNpcVisibilityPublic();
    }

    /// <summary>
    /// Revisa umbrales críticos y ansiedad máxima (se llama al cambiar de habitación).
    /// </summary>
    public void ValidateAnxietyLevels()
    {
        if (StoryState.Instance == null || !IsAnxietyActiveForCurrentChapter())
        {
            return;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterAnxietyEntry entry = characters[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || IsDead(entry.id))
            {
                continue;
            }

            ApplyAnxietyLevelFlags(entry);

            if (entry.anxiety >= 99.9f)
            {
                CharacterAnxietyDeathDirector.Instance?.TryStartSeparationSequence(entry.id);
            }
        }
    }

    private void SubscribeToRoomChanges()
    {
        if (subscribedToRoomChanges || RoomManager.Instance == null)
        {
            return;
        }

        RoomManager.Instance.RoomChanged += HandleRoomChanged;
        subscribedToRoomChanges = true;
    }

    private void UnsubscribeFromRoomChanges()
    {
        if (!subscribedToRoomChanges || RoomManager.Instance == null)
        {
            return;
        }

        RoomManager.Instance.RoomChanged -= HandleRoomChanged;
        subscribedToRoomChanges = false;
    }

    private void HandleRoomChanged(string previousRoom, string newRoom)
    {
        if (string.IsNullOrWhiteSpace(newRoom))
        {
            return;
        }

        ValidateAnxietyLevels();
    }

    private bool IsAnxietyActiveForCurrentChapter()
    {
        if (StoryState.Instance == null)
        {
            return false;
        }

        if (activeChapterIds == null || activeChapterIds.Count == 0)
        {
            return true;
        }

        return activeChapterIds.Contains(StoryState.Instance.CurrentChapterId);
    }

    private static void ApplyAnxietyLevelFlags(CharacterAnxietyEntry entry)
    {
        if (entry == null || StoryState.Instance == null)
        {
            return;
        }

        if (entry.anxiety >= entry.criticalThreshold)
        {
            StoryState.Instance.SetFlag($"npc.critical.{entry.id}", true);
        }
        else
        {
            StoryState.Instance.SetFlag($"npc.critical.{entry.id}", false);
        }
    }

    private string BuildFullStatusText()
    {
        if (characters.Count == 0)
        {
            return "Sin personajes configurados.";
        }

        List<string> lines = new List<string>();
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterAnxietyEntry entry = characters[i];
            if (entry == null)
            {
                continue;
            }

            lines.Add($"{GetDisplayName(entry)}: {Mathf.RoundToInt(entry.anxiety)}");
        }

        return string.Join("\n", lines);
    }

    private static string GetDisplayName(CharacterAnxietyEntry entry)
    {
        return string.IsNullOrWhiteSpace(entry.displayName) ? entry.id : entry.displayName;
    }

    private static string GetMoodLabel(float anxiety, float criticalThreshold)
    {
        if (anxiety >= criticalThreshold)
        {
            return "critico";
        }

        if (anxiety >= criticalThreshold * 0.66f)
        {
            return "alto";
        }

        if (anxiety >= criticalThreshold * 0.33f)
        {
            return "medio";
        }

        return "estable";
    }
}
