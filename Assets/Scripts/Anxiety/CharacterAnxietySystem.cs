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
        [Range(0f, 20f)] public float increasePerMinute = 4f;
        [Range(0f, 50f)] public float talkReduction = 18f;
        [Range(0f, 100f)] public float criticalThreshold = 75f;
    }

    public static CharacterAnxietySystem Instance { get; private set; }

    [Header("Configuracion")]
    [SerializeField] private string activeChapterId = "chapter1";
    [SerializeField] private List<CharacterAnxietyEntry> characters = new List<CharacterAnxietyEntry>();
    [SerializeField] private TMP_Text debugStatusText;

    private readonly Dictionary<string, CharacterAnxietyEntry> lookup = new Dictionary<string, CharacterAnxietyEntry>();

    public event Action<string, float> CharacterAnxietyChanged;

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
    }

    private void Update()
    {
        if (StoryState.Instance == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(activeChapterId) && StoryState.Instance.CurrentChapterId != activeChapterId)
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

            float before = entry.anxiety;
            entry.anxiety = Mathf.Clamp(entry.anxiety + entry.increasePerMinute * deltaMinutes, 0f, 100f);
            if (!Mathf.Approximately(before, entry.anxiety))
            {
                CharacterAnxietyChanged?.Invoke(entry.id, entry.anxiety);
            }

            if (entry.anxiety >= entry.criticalThreshold)
            {
                StoryState.Instance.SetFlag($"npc.critical.{entry.id}", true);
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

        entry.anxiety = Mathf.Clamp(entry.anxiety - entry.talkReduction, 0f, 100f);
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
