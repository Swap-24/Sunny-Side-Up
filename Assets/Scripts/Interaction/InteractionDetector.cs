using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    public IInteractable CurrentInteractable { get; private set; }

    [SerializeField] private InteractionUI interactionUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            CurrentInteractable = interactable;

            interactionUI.Show(other.transform);

            Debug.Log(
                "Interactable detected: " +
                other.gameObject.name
            );
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null &&
            CurrentInteractable == interactable)
        {
            CurrentInteractable = null;

            interactionUI.Hide();

            Debug.Log("Left interaction range");
        }
    }
    public void RefreshInteractionUI()
{
    if (CurrentInteractable == null)
        return;

    interactionUI.Show(
        ((MonoBehaviour)CurrentInteractable).transform
    );
}
}