using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Image promptImage;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);

    private Transform target;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (target == null || interactionPrompt == null || !interactionPrompt.activeSelf)
            return;

        UpdatePromptPosition();
    }

    public void Show(Transform interactableTransform)
    {
        if (interactableTransform == null)
            return;

        target = interactableTransform;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            UpdatePromptPosition();
        }
    }

    public void Hide()
    {
        target = null;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void UpdatePromptPosition()
    {
        if (target == null || interactionPrompt == null || Camera.main == null)
            return;

        Vector3 worldPosition = target.position + worldOffset;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        interactionPrompt.transform.position = screenPosition;
    }
}