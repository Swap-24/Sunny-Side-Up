using UnityEngine;

public class NPCConversation : MonoBehaviour, IInteractable
{
    [Header("One-Time Story Conversation")]
    [Tooltip("The main full-fledged story conversation played on the first interaction.")]
    [SerializeField] private ConversationData initialConversation;

    [Header("Subsequent Interactions (Repeatable)")]
    [Tooltip("Repeatable conversation played on all subsequent interactions.")]
    [SerializeField] private ConversationData repeatableConversation;

    [Tooltip("Alternatively: Repeatable interaction dialogue played on subsequent interactions.")]
    [SerializeField] private InteractionDialogue repeatableDialogue;

    [Header("Prompt Settings")]
    [SerializeField] private string promptText = "[E] - Talk";

    [Header("Persistence")]
    [Tooltip("Unique ID to remember that this conversation has completed across scene changes.")]
    [SerializeField] private string uniqueID;

    private bool hasCompletedInitialConversation = false;

    public string GetInteractionText()
    {
        return promptText;
    }

    public bool CanInteract()
    {
        if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive)
            return false;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return false;

        bool isFirst = IsFirstInteraction();
        if (isFirst)
        {
            return initialConversation != null || repeatableConversation != null || repeatableDialogue != null;
        }

        return repeatableConversation != null || repeatableDialogue != null || initialConversation != null;
    }

    public void Interact()
    {
        bool isFirst = IsFirstInteraction();

        if (isFirst && initialConversation != null)
        {
            if (ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartConversation(initialConversation);
                MarkInteracted();
            }
        }
        else
        {
            // Subsequent interaction: Play repeatable conversation or dialogue
            if (repeatableConversation != null && ConversationManager.Instance != null)
            {
                ConversationManager.Instance.StartConversation(repeatableConversation);
            }
            else if (repeatableDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(repeatableDialogue);
            }
            else if (initialConversation != null && ConversationManager.Instance != null)
            {
                // Fallback if no repeatable dialogue is set
                ConversationManager.Instance.StartConversation(initialConversation);
            }
        }
    }

    private bool IsFirstInteraction()
    {
        if (!string.IsNullOrEmpty(uniqueID) && GameState.Instance != null)
        {
            return !GameState.Instance.HasInteractedWith(uniqueID);
        }

        return !hasCompletedInitialConversation;
    }

    private void MarkInteracted()
    {
        hasCompletedInitialConversation = true;

        if (!string.IsNullOrEmpty(uniqueID) && GameState.Instance != null)
        {
            GameState.Instance.RecordInteraction(uniqueID);
        }
    }

    [ContextMenu("Reset Conversation State")]
    public void ResetConversationState()
    {
        hasCompletedInitialConversation = false;
        Debug.Log($"[NPCConversation] Reset conversation state for '{gameObject.name}'");
    }
}
