using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 6)] public string text;
    public float anxietyDelta;
    public string requiredFlag;
    public string setFlag;
    public string addInventoryItemId;
    public string removeInventoryItemId;
}

[Serializable]
public class DialogueChoice
{
    public string id;
    public string text;
    public string nextNodeId;
    public float anxietyDelta;
    public float requiredAnxietyMin;
    public float requiredAnxietyMax = 100f;
    public string requiredFlag;
    public string setFlag;
}

[Serializable]
public class DialogueNode
{
    public string id = "start";
    public List<DialogueLine> lines = new List<DialogueLine>();
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    public string nextNodeIdIfNoChoices;
    public bool endsConversation = true;
}

[Serializable]
public class DialogueConversation
{
    public string id;
    public List<DialogueNode> nodes = new List<DialogueNode>();

    public DialogueNode GetNode(string nodeId)
    {
        if (nodes == null)
        {
            return null;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            DialogueNode node = nodes[i];
            if (node != null && node.id == nodeId)
            {
                return node;
            }
        }

        return null;
    }
}
