using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConversation", menuName = "Conversation/Conversation Data")]
public class ConversationData : ScriptableObject
{
    [Tooltip("Optional title / identifier for this conversation.")]
    public string conversationTitle;

    [Tooltip("List of back-and-forth conversation lines.")]
    public List<ConversationLine> lines = new();
}

