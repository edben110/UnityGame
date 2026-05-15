using System;
using UnityEngine;

[Serializable]
public class InventoryItemDefinition
{
    public string id;
    public string displayName;
    [TextArea(2, 5)] public string description;
    public Sprite icon;
}
