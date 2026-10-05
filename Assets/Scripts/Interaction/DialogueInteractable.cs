using UnityEngine;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [Header("Dialogue Configuration")]
    [Tooltip("Standard dialogue played upon interaction.")]
    [SerializeField] private DialogueData interactionDialogue;

    [Tooltip("If true, a special dialogue will play on the very first interaction.")]
    [SerializeField] private bool specialFirstDialogue = false;

    [Tooltip("Special dialogue played only on the first interaction.")]
    [SerializeField] private DialogueData specialDialogue;

    [Header("Prompt Settings")]
    [SerializeField] private string interactionPrompt = "[E] - Inspect";

    [Header("Optional Persistence Across Scenes")]
    [Tooltip("Optional unique ID if you want first-interaction memory to persist when changing scenes.")]
    [SerializeField] private string uniqueID;

    private bool hasInteracted = false;

    public string GetInteractionText()
    {
        return interactionPrompt;
    }

    public bool CanInteract()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return false;

        return interactionDialogue != null || (specialFirstDialogue && specialDialogue != null);
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null)
            return;

        bool isFirst = IsFirstInteraction();

        if (specialFirstDialogue && isFirst && specialDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(specialDialogue);
        }
        else if (interactionDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(interactionDialogue);
        }

        MarkInteracted();
    }

    private bool IsFirstInteraction()
    {
        if (!string.IsNullOrEmpty(uniqueID) && GameState.Instance != null)
        {
            return !GameState.Instance.HasInteractedWith(uniqueID);
        }

        return !hasInteracted;
    }

    private void MarkInteracted()
    {
        hasInteracted = true;

        if (!string.IsNullOrEmpty(uniqueID) && GameState.Instance != null)
        {
            GameState.Instance.RecordInteraction(uniqueID);
        }
    }
}
