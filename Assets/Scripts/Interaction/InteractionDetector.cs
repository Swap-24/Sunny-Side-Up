using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    public IInteractable CurrentInteractable { get; private set; }

    [SerializeField] private InteractionUI interactionUI;

    private InteractionUI UI
    {
        get
        {
            if (interactionUI == null)
            {
                interactionUI = InteractionUI.Instance != null ? InteractionUI.Instance : FindFirstObjectByType<InteractionUI>();
            }
            return interactionUI;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            CurrentInteractable = interactable;

            if (UI != null)
            {
                UI.Show(other.transform);
            }

            Debug.Log("Interactable detected: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null)
            return;

        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null && CurrentInteractable == interactable)
        {
            CurrentInteractable = null;

            if (UI != null)
            {
                UI.Hide();
            }

            Debug.Log("Left interaction range");
        }
    }

    public void RefreshInteractionUI()
    {
        if (CurrentInteractable == null)
            return;

        if (CurrentInteractable is MonoBehaviour mb && mb != null)
        {
            if (UI != null)
            {
                UI.Show(mb.transform);
            }
        }
    }
}