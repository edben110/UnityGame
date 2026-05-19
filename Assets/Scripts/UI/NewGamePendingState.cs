using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal class NewGameFlagListWrapper
{
    public List<string> values = new List<string>();
}

/// <summary>
/// Estado narrativo temporal mientras el intro de nueva partida corre en MenuScene (sin StoryState).
/// </summary>
public static class NewGamePendingState
{
    private static readonly HashSet<string> flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> decisions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static float Anxiety { get; private set; }
    public static bool IsActive { get; private set; }

    public static void Begin()
    {
        Reset();
        IsActive = true;
    }

    public static void Reset()
    {
        flags.Clear();
        decisions.Clear();
        Anxiety = 0f;
        IsActive = false;
    }

    public static void SetFlag(string flag, bool enabled = true)
    {
        if (string.IsNullOrWhiteSpace(flag))
        {
            return;
        }

        if (enabled)
        {
            flags.Add(flag);
        }
        else
        {
            flags.Remove(flag);
        }
    }

    public static void SetDecision(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        decisions[key] = value ?? string.Empty;
    }

    public static void AddAnxiety(float delta)
    {
        Anxiety = Mathf.Clamp(Anxiety + delta, 0f, 100f);
    }

    public static IReadOnlyCollection<string> GetFlags()
    {
        return flags;
    }

    public static bool HasFlag(string flag)
    {
        return !string.IsNullOrWhiteSpace(flag) && flags.Contains(flag);
    }

    public static void WriteToSaveData(SaveData saveData, string chapterId, float anxiety)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.SetDecision("story.chapter", chapterId);
        saveData.SetDecision("story.anxiety", anxiety.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        saveData.SetDecision("newgame.intro.completed", "true");

        foreach (KeyValuePair<string, string> pair in decisions)
        {
            saveData.SetDecision(pair.Key, pair.Value);
        }

        NewGameFlagListWrapper flagWrapper = new NewGameFlagListWrapper();
        foreach (string flag in flags)
        {
            flagWrapper.values.Add(flag);
        }

        saveData.SetDecision("story.flags", JsonUtility.ToJson(flagWrapper));
    }
}
