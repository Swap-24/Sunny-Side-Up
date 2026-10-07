using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;

public class DialogueInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Dialogue Asset")]
    [Tooltip("The InteractionDialogue asset containing both First-Time and Repeatable lines.")]
    [FormerlySerializedAs("interactionDialogue")]
    [SerializeField] private InteractionDialogue dialogue;

    [Header("Prompt Settings")]
    [SerializeField] private string interactionPrompt = "[E] - Inspect";

    [Header("Optional Persistence Across Scenes")]
    [Tooltip("Optional unique ID if you want first-interaction memory to persist when changing scenes.")]
    [SerializeField] private string uniqueID;

    [Header("Runtime State (Debug)")]
    [SerializeField] private bool hasInteracted = false;

    public string GetInteractionText()
    {
        return interactionPrompt;
    }

    public bool CanInteract()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
            return false;

        return dialogue != null && (dialogue.HasFirstTimeDialogue || dialogue.HasRepeatableDialogue);
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[DialogueInteractable] DialogueManager Instance is missing in the scene!");
            return;
        }

        if (dialogue == null)
        {
            Debug.LogWarning($"[DialogueInteractable] '{gameObject.name}' has no InteractionDialogue asset assigned!", this);
            return;
        }

        bool isFirst = IsFirstInteraction();
        List<DialogueLine> linesToPlay = dialogue.GetLines(isFirst);

        if (linesToPlay != null && linesToPlay.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(linesToPlay);
            MarkInteracted();
        }
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

    [ContextMenu("Reset Interaction State")]
    public void ResetInteractionState()
    {
        hasInteracted = false;
        Debug.Log($"[DialogueInteractable] Reset interaction state for '{gameObject.name}'");
    }
}
