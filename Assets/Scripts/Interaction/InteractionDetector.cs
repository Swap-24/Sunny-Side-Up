using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    public IInteractable CurrentInteractable { get; private set; }

    [Header("Directional Detection Settings")]
    [Tooltip("Distance in front of the player to check for interactables")]
    [SerializeField] private float interactionDistance = 0.75f;

    [Tooltip("Size of the detection box")]
    [SerializeField] private Vector2 boxSize = new Vector2(0.6f, 0.6f);

    [Tooltip("Offset from the player transform origin (e.g. center of sprite)")]
    [SerializeField] private Vector2 originOffset = Vector2.zero;

    [Tooltip("Which layers contain interactable objects")]
    [SerializeField] private LayerMask interactableLayers = ~0;

    [Header("UI Reference")]
    [SerializeField] private InteractionUI interactionUI;

    private PlayerMove playerMove;
    private readonly Collider2D[] hitResults = new Collider2D[16];
    private ContactFilter2D contactFilter;

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

    private void Awake()
    {
        playerMove = GetComponentInParent<PlayerMove>();
        if (playerMove == null)
        {
            playerMove = GetComponent<PlayerMove>();
        }

        contactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = interactableLayers,
            useTriggers = true
        };
    }

    private void Update()
    {
        DetectInteractableInFront();
    }

    private void DetectInteractableInFront()
    {
        Vector2 facing = GetFacingDirection();
        Vector2 checkPosition = (Vector2)transform.position + originOffset + (facing * interactionDistance);

        contactFilter.layerMask = interactableLayers;
        int hitCount = Physics2D.OverlapBox(checkPosition, boxSize, 0f, contactFilter, hitResults);

        IInteractable foundInteractable = null;
        Transform interactableTransform = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitResults[i];
            if (hit == null) continue;

            // Ignore the player's own GameObject and colliders
            if (hit.transform == transform || (playerMove != null && hit.transform == playerMove.transform))
                continue;

            if (hit.CompareTag("Player"))
                continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = hit.GetComponentInParent<IInteractable>();
            }

            if (interactable != null)
            {
                foundInteractable = interactable;
                interactableTransform = hit.transform;
                break;
            }
        }

        // State changed
        if (foundInteractable != CurrentInteractable)
        {
            CurrentInteractable = foundInteractable;

            if (CurrentInteractable != null)
            {
                if (UI != null && interactableTransform != null)
                {
                    UI.Show(interactableTransform);
                }
            }
            else
            {
                if (UI != null)
                {
                    UI.Hide();
                }
            }
        }
    }

    public Vector2 GetFacingDirection()
    {
        if (playerMove != null && playerMove.FacingDirection != Vector2.zero)
        {
            return playerMove.FacingDirection;
        }

        if (PlayerMove.Instance != null && PlayerMove.Instance.FacingDirection != Vector2.zero)
        {
            return PlayerMove.Instance.FacingDirection;
        }

        return Vector2.down;
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

    private void OnDrawGizmosSelected()
    {
        Vector2 facing = Application.isPlaying ? GetFacingDirection() : Vector2.down;
        Vector2 checkPosition = (Vector2)transform.position + originOffset + (facing * interactionDistance);

        Gizmos.color = CurrentInteractable != null ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(checkPosition, boxSize);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine((Vector2)transform.position + originOffset, checkPosition);
    }
}