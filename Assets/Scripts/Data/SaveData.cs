using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    [Serializable]
    public struct DecisionEntry
    {
        public string key;
        public string value;
    }

    public string currentChapterId;
    public int basicProgress;
    public List<string> unlockedChapterIds = new List<string>();
    public List<DecisionEntry> serializedDecisions = new List<DecisionEntry>();

    [NonSerialized]
    public Dictionary<string, string> decisions = new Dictionary<string, string>();

    public void EnsureValid(string fallbackChapterId)
    {
        if (basicProgress < 0)
        {
            basicProgress = 0;
        }

        if (unlockedChapterIds == null)
        {
            unlockedChapterIds = new List<string>();
        }

        if (serializedDecisions == null)
        {
            serializedDecisions = new List<DecisionEntry>();
        }

        if (decisions == null)
        {
            decisions = new Dictionary<string, string>();
        }

        if (string.IsNullOrWhiteSpace(currentChapterId))
        {
            currentChapterId = fallbackChapterId;
        }

        if (!string.IsNullOrWhiteSpace(fallbackChapterId) && !unlockedChapterIds.Contains(fallbackChapterId))
        {
            unlockedChapterIds.Add(fallbackChapterId);
        }
    }

    public void SetDecision(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        decisions[key] = value ?? string.Empty;
    }

    public string GetDecision(string key, string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(key) || decisions == null)
        {
            return defaultValue;
        }

        return decisions.TryGetValue(key, out string value) ? value : defaultValue;
    }

    public void SyncBeforeSave()
    {
        if (decisions == null)
        {
            decisions = new Dictionary<string, string>();
        }

        serializedDecisions.Clear();
        foreach (KeyValuePair<string, string> pair in decisions)
        {
            serializedDecisions.Add(new DecisionEntry
            {
                key = pair.Key,
                value = pair.Value
            });
        }
    }

    public void SyncAfterLoad()
    {
        if (serializedDecisions == null)
        {
            serializedDecisions = new List<DecisionEntry>();
        }

        decisions = new Dictionary<string, string>();
        foreach (DecisionEntry entry in serializedDecisions)
        {
            if (string.IsNullOrWhiteSpace(entry.key))
            {
                continue;
            }

            decisions[entry.key] = entry.value ?? string.Empty;
        }
    }
}
