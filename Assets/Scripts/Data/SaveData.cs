using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    [Serializable]
    public struct ChapterUnlockEntry
    {
        public string chapterId;
        public bool unlocked;
    }

    [Serializable]
    public struct DecisionEntry
    {
        public string key;
        public string value;
    }

    public string lastSceneName;
    public int basicProgress;
    public List<string> chapterOrder = new List<string>();
    public List<ChapterUnlockEntry> serializedChapterUnlocks = new List<ChapterUnlockEntry>();
    public List<DecisionEntry> serializedDecisions = new List<DecisionEntry>();

    [NonSerialized]
    public Dictionary<string, bool> chapterUnlocks = new Dictionary<string, bool>();

    [NonSerialized]
    public Dictionary<string, string> decisions = new Dictionary<string, string>();

    public void EnsureValid(string fallbackSceneName, List<string> defaultChapterIds)
    {
        if (basicProgress < 0)
        {
            basicProgress = 0;
        }

        if (chapterOrder == null)
        {
            chapterOrder = new List<string>();
        }

        if (serializedChapterUnlocks == null)
        {
            serializedChapterUnlocks = new List<ChapterUnlockEntry>();
        }

        if (chapterUnlocks == null)
        {
            chapterUnlocks = new Dictionary<string, bool>();
        }

        if (serializedDecisions == null)
        {
            serializedDecisions = new List<DecisionEntry>();
        }

        if (decisions == null)
        {
            decisions = new Dictionary<string, string>();
        }

        if (string.IsNullOrWhiteSpace(lastSceneName))
        {
            lastSceneName = fallbackSceneName;
        }

        if (defaultChapterIds == null)
        {
            return;
        }

        for (int i = 0; i < defaultChapterIds.Count; i++)
        {
            string chapterId = defaultChapterIds[i];
            if (string.IsNullOrWhiteSpace(chapterId))
            {
                continue;
            }

            if (!chapterOrder.Contains(chapterId))
            {
                chapterOrder.Add(chapterId);
            }

            if (!chapterUnlocks.ContainsKey(chapterId))
            {
                chapterUnlocks[chapterId] = i == 0;
            }
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
        if (chapterUnlocks == null)
        {
            chapterUnlocks = new Dictionary<string, bool>();
        }

        if (decisions == null)
        {
            decisions = new Dictionary<string, string>();
        }

        serializedChapterUnlocks.Clear();
        foreach (KeyValuePair<string, bool> pair in chapterUnlocks)
        {
            serializedChapterUnlocks.Add(new ChapterUnlockEntry
            {
                chapterId = pair.Key,
                unlocked = pair.Value
            });
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
        if (serializedChapterUnlocks == null)
        {
            serializedChapterUnlocks = new List<ChapterUnlockEntry>();
        }

        if (serializedDecisions == null)
        {
            serializedDecisions = new List<DecisionEntry>();
        }

        chapterUnlocks = new Dictionary<string, bool>();
        foreach (ChapterUnlockEntry entry in serializedChapterUnlocks)
        {
            if (string.IsNullOrWhiteSpace(entry.chapterId))
            {
                continue;
            }

            chapterUnlocks[entry.chapterId] = entry.unlocked;
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
