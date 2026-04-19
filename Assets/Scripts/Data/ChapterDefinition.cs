using System;
using UnityEngine;

[Serializable]
public class ChapterDefinition
{
    public string id;
    public string displayName;
    public string sceneName;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(sceneName);
    }
}
