using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    public IInteractable CurrentInteractable { get; private set; }

    [Header("Directional Detection Settings")]
    [Tooltip("How far in front of the player to check for interactables")]
    [SerializeField] private float interactionDistance = 1.0f;

    [Tooltip("Size of the detection box projected in front of the player")]
    [SerializeField] private Vector2 boxSize = new Vector2(0.8f, 0.8f);

    [Tooltip("Offset from the player transform origin (e.g. center of sprite)")]
    [SerializeField] private Vector2 originOffset = Vector2.zero;

    [Tooltip("Minimum dot product to consider the player 'facing' the object (0.4 = ~66 degree cone in front)")]
    [Range(0f, 1f)]
    [SerializeField] private float minFacingAngleDot = 0.4f;

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
                interactionUI = InteractionUI.Instance != null ? InteractionUI.Instance : FindAnyObjectByType<InteractionUI>();
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
        Vector2 playerCenter = (Vector2)transform.position + originOffset;
        Vector2 checkPosition = playerCenter + (facing * (interactionDistance * 0.5f));

        contactFilter.layerMask = interactableLayers;
        int hitCount = Physics2D.OverlapBox(checkPosition, boxSize, 0f, contactFilter, hitResults);

        IInteractable bestInteractable = null;
        Transform bestTransform = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitResults[i];
            if (hit == null) continue;

            // Ignore the player's own GameObject, children, and colliders
            if (hit.transform == transform || (playerMove != null && (hit.transform == playerMove.transform || hit.transform.IsChildOf(playerMove.transform))))
                continue;

            if (hit.CompareTag("Player"))
                continue;

            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = hit.GetComponentInParent<IInteractable>();
            }

            if (interactable == null)
                continue;

            // Find the closest point on the interactable collider to the player
            Vector2 closestPoint = hit.ClosestPoint(playerCenter);
            Vector2 dirToObject = closestPoint - playerCenter;

            // If player is overlapping the center of the collider, fallback to bounds center
            if (dirToObject.sqrMagnitude < 0.0001f)
            {
                dirToObject = (Vector2)hit.bounds.center - playerCenter;
            }

            // Check if the object is genuinely in FRONT of the player
            float dot = Vector2.Dot(facing, dirToObject.normalized);

            // Reject if the object is behind or to the side
            if (dot < minFacingAngleDot)
                continue;

            // Score based on directional alignment and proximity
            float distance = dirToObject.magnitude;
            float score = (dot * 2f) - distance;

            if (score > bestScore)
            {
                bestScore = score;
                bestInteractable = interactable;
                bestTransform = hit.transform;
            }
        }

        // State changed
        if (bestInteractable != CurrentInteractable)
        {
            CurrentInteractable = bestInteractable;

            if (CurrentInteractable != null)
            {
                if (UI != null && bestTransform != null)
                {
                    UI.Show(bestTransform);
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
            return playerMove.FacingDirection.normalized;
        }

        if (PlayerMove.Instance != null && PlayerMove.Instance.FacingDirection != Vector2.zero)
        {
            return PlayerMove.Instance.FacingDirection.normalized;
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
        Vector2 playerCenter = (Vector2)transform.position + originOffset;
        Vector2 checkPosition = playerCenter + (facing * (interactionDistance * 0.5f));

        Gizmos.color = CurrentInteractable != null ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(checkPosition, boxSize);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(playerCenter, playerCenter + (facing * interactionDistance));
    }
}