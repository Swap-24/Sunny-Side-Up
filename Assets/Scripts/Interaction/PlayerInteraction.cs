using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private InteractionDetector detector;
    private InteractionUI interactionUI;

    private void Awake()
    {
        detector = GetComponentInChildren<InteractionDetector>();
        interactionUI = FindAnyObjectByType<InteractionUI>();
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame)
            return;

        // =========================
        // DIALOGUE
        // =========================

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            DialogueManager.Instance.HandleInput();
            return;
        }

        // =========================
        // JOURNAL
        // =========================

        if (JournalManager.Instance != null && JournalManager.Instance.IsJournalOpen)
        {
            JournalManager.Instance.CloseJournal();

            if (detector != null)
            {
                detector.RefreshInteractionUI();
            }
            return;
        }

        // =========================
        // FADING / SLEEPING
        // =========================

        if (FadeManager.Instance != null && FadeManager.Instance.IsFading)
            return;

        if (SleepManager.Instance != null && SleepManager.Instance.IsSleeping)
            return;

        // =========================
        // NORMAL INTERACTION
        // =========================

        if (detector != null)
        {
            IInteractable interactable = detector.CurrentInteractable;

            if (interactable != null)
            {
                if (interactable.CanInteract())
                {
                    // Hide prompt while the interaction is happening
                    if (interactionUI != null)
                    {
                        interactionUI.Hide();
                    }

                    // Perform interaction
                    interactable.Interact();
                }
            }
        }
    }
}