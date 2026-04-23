using System.Collections.Generic;
using UnityEngine;

public class DialogueLibrary : MonoBehaviour
{
    [SerializeField] private List<DialogueConversation> conversations = new List<DialogueConversation>();

    public bool HasAnyConversation()
    {
        return conversations != null && conversations.Count > 0;
    }

    public void ReplaceConversations(List<DialogueConversation> newConversations)
    {
        conversations = newConversations ?? new List<DialogueConversation>();
    }

    public DialogueConversation GetConversation(string conversationId)
    {
        for (int i = 0; i < conversations.Count; i++)
        {
            DialogueConversation conversation = conversations[i];
            if (conversation != null && conversation.id == conversationId)
            {
                return conversation;
            }
        }

        return null;
    }

    public bool HasConversation(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            return false;
        }

        return GetConversation(conversationId) != null;
    }
}
