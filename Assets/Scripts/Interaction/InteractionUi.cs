using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Image promptImage;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    private Transform target;

    private void Update()
    {
        if (target == null || !interactionPrompt.activeSelf)
            return;

        Vector3 worldPosition = target.position + worldOffset;

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(worldPosition);

        interactionPrompt.transform.position = screenPosition;
    }

    public void Show(Transform interactableTransform)
    {
        target = interactableTransform;

        interactionPrompt.SetActive(true);

        UpdatePromptPosition();
    }

    public void Hide()
    {
        target = null;

        interactionPrompt.SetActive(false);
    }

    private void UpdatePromptPosition()
    {
        if (target == null)
            return;

        Vector3 worldPosition = target.position + worldOffset;

        Vector3 screenPosition =
            Camera.main.WorldToScreenPoint(worldPosition);

        interactionPrompt.transform.position = screenPosition;
    }
}