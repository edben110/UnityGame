using System;
using System.Collections.Generic;
using UnityEngine;

public class StoryState : MonoBehaviour
{
    [Serializable]
    private class StringListWrapper
    {
        public List<string> values = new List<string>();
    }

    [Serializable]
    private class KeyValue
    {
        public string key;
        public string value;
    }

    [Serializable]
    private class KeyValueListWrapper
    {
        public List<KeyValue> values = new List<KeyValue>();
    }

    public static StoryState Instance { get; private set; }

    public event Action StateChanged;

    [Header("Estado narrativo")]
    [SerializeField] private string currentChapterId = "prologue";
    [SerializeField, Range(0f, 100f)] private float anxiety = 0f;

    private readonly HashSet<string> flags = new HashSet<string>();
    private readonly Dictionary<string, string> decisions = new Dictionary<string, string>();

    public string CurrentChapterId => currentChapterId;
    public float Anxiety => anxiety;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public bool HasFlag(string flag)
    {
        return !string.IsNullOrWhiteSpace(flag) && flags.Contains(flag);
    }

    public void SetFlag(string flag, bool enabled = true)
    {
        if (string.IsNullOrWhiteSpace(flag))
        {
            return;
        }

        bool changed = enabled ? flags.Add(flag) : flags.Remove(flag);
        if (!changed)
        {
            return;
        }

        Save();
        NotifyStateChanged();
    }

    public void SetDecision(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        decisions[key] = value ?? string.Empty;
        Save();
        NotifyStateChanged();
    }

    public string GetDecision(string key, string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return defaultValue;
        }

        return decisions.TryGetValue(key, out string value) ? value : defaultValue;
    }

    public void SetChapter(string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId) || chapterId == currentChapterId)
        {
            return;
        }

        currentChapterId = chapterId;
        Save();
        NotifyStateChanged();
    }

    public void AddAnxiety(float delta)
    {
        SetAnxiety(anxiety + delta);
    }

    public void SetAnxiety(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, 100f);
        if (Mathf.Approximately(clamped, anxiety))
        {
            return;
        }

        anxiety = clamped;
        Save();
        NotifyStateChanged();
    }

    public void ResetForNewGame(string startingChapterId = "prologue")
    {
        currentChapterId = string.IsNullOrWhiteSpace(startingChapterId) ? "prologue" : startingChapterId;
        anxiety = 0f;
        flags.Clear();
        decisions.Clear();
        Save();
        NotifyStateChanged();
    }

    public void Load()
    {
        if (GameManager.Instance != null)
        {
            currentChapterId = GameManager.Instance.GetDecision("story.chapter", "prologue");
            anxiety = ParseFloat(GameManager.Instance.GetDecision("story.anxiety", "0"));

            flags.Clear();
            StringListWrapper loadedFlags = ParseStringList(GameManager.Instance.GetDecision("story.flags", string.Empty));
            for (int i = 0; i < loadedFlags.values.Count; i++)
            {
                string flag = loadedFlags.values[i];
                if (!string.IsNullOrWhiteSpace(flag))
                {
                    flags.Add(flag);
                }
            }

            decisions.Clear();
            KeyValueListWrapper loadedDecisions = ParseKeyValueList(GameManager.Instance.GetDecision("story.decisions", string.Empty));
            for (int i = 0; i < loadedDecisions.values.Count; i++)
            {
                KeyValue item = loadedDecisions.values[i];
                if (!string.IsNullOrWhiteSpace(item.key))
                {
                    decisions[item.key] = item.value ?? string.Empty;
                }
            }

            NotifyStateChanged();
            return;
        }

        currentChapterId = PlayerPrefs.GetString("story.chapter", "prologue");
        anxiety = PlayerPrefs.GetFloat("story.anxiety", 0f);

        flags.Clear();
        StringListWrapper fallbackFlags = ParseStringList(PlayerPrefs.GetString("story.flags", string.Empty));
        for (int i = 0; i < fallbackFlags.values.Count; i++)
        {
            string flag = fallbackFlags.values[i];
            if (!string.IsNullOrWhiteSpace(flag))
            {
                flags.Add(flag);
            }
        }

        decisions.Clear();
        KeyValueListWrapper fallbackDecisions = ParseKeyValueList(PlayerPrefs.GetString("story.decisions", string.Empty));
        for (int i = 0; i < fallbackDecisions.values.Count; i++)
        {
            KeyValue item = fallbackDecisions.values[i];
            if (!string.IsNullOrWhiteSpace(item.key))
            {
                decisions[item.key] = item.value ?? string.Empty;
            }
        }

        NotifyStateChanged();
    }

    public void Save()
    {
        string chapter = string.IsNullOrWhiteSpace(currentChapterId) ? "prologue" : currentChapterId;
        string anxietyString = anxiety.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        StringListWrapper flagWrapper = new StringListWrapper();
        foreach (string flag in flags)
        {
            flagWrapper.values.Add(flag);
        }

        KeyValueListWrapper decisionWrapper = new KeyValueListWrapper();
        foreach (KeyValuePair<string, string> pair in decisions)
        {
            decisionWrapper.values.Add(new KeyValue
            {
                key = pair.Key,
                value = pair.Value
            });
        }

        string flagsJson = JsonUtility.ToJson(flagWrapper);
        string decisionsJson = JsonUtility.ToJson(decisionWrapper);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveDecision("story.chapter", chapter);
            GameManager.Instance.SaveDecision("story.anxiety", anxietyString);
            GameManager.Instance.SaveDecision("story.flags", flagsJson);
            GameManager.Instance.SaveDecision("story.decisions", decisionsJson);
            return;
        }

        PlayerPrefs.SetString("story.chapter", chapter);
        PlayerPrefs.SetFloat("story.anxiety", anxiety);
        PlayerPrefs.SetString("story.flags", flagsJson);
        PlayerPrefs.SetString("story.decisions", decisionsJson);
        PlayerPrefs.Save();
    }

    private static float ParseFloat(string text)
    {
        if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed))
        {
            return Mathf.Clamp(parsed, 0f, 100f);
        }

        return 0f;
    }

    private static StringListWrapper ParseStringList(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new StringListWrapper();
        }

        try
        {
            StringListWrapper parsed = JsonUtility.FromJson<StringListWrapper>(raw);
            return parsed ?? new StringListWrapper();
        }
        catch
        {
            return new StringListWrapper();
        }
    }

    private static KeyValueListWrapper ParseKeyValueList(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new KeyValueListWrapper();
        }

        try
        {
            KeyValueListWrapper parsed = JsonUtility.FromJson<KeyValueListWrapper>(raw);
            return parsed ?? new KeyValueListWrapper();
        }
        catch
        {
            return new KeyValueListWrapper();
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
