using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public static PlayerMove Instance { get; private set; }

    [SerializeField] private float moveSpeed = 5f;

    [Header("Movement State")]
    [SerializeField] private bool isMovementFrozen = false;

    private Animator animator;

    public bool IsMovementFrozen
    {
        get => isMovementFrozen;
        set => isMovementFrozen = value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("PlayerMove could not find an Animator on Sol.");
        }
    }

    private void Start()
    {
        PositionPlayerAtSpawnPoint();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void PositionPlayerAtSpawnPoint()
    {
        string targetSpawn = GameState.Instance != null ? GameState.Instance.TargetSpawnPoint : null;

        SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>();
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        SpawnPoint chosenSpawn = null;

        if (!string.IsNullOrEmpty(targetSpawn))
        {
            foreach (var sp in spawnPoints)
            {
                if (sp != null && !string.IsNullOrEmpty(sp.SpawnPointID) &&
                    sp.SpawnPointID.Trim().Equals(targetSpawn.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    chosenSpawn = sp;
                    break;
                }
            }
        }

        // If no matching ID found, check for a default spawn point
        if (chosenSpawn == null)
        {
            foreach (var sp in spawnPoints)
            {
                if (sp != null && sp.IsDefaultSpawn)
                {
                    chosenSpawn = sp;
                    break;
                }
            }
        }

        if (chosenSpawn != null)
        {
            transform.position = chosenSpawn.transform.position;

            // Snap camera immediately to avoid camera sliding across the room on load
            CameraFollow cam = FindAnyObjectByType<CameraFollow>();
            if (cam != null && cam.player == transform)
            {
                cam.transform.position = new Vector3(transform.position.x, transform.position.y, cam.transform.position.z);
            }
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.ClearTargetSpawnPoint();
        }
    }

    public void FreezeMovement()
    {
        isMovementFrozen = true;
    }

    public void UnfreezeMovement()
    {
        isMovementFrozen = false;
    }

    public void SetMovementFrozen(bool freeze)
    {
        isMovementFrozen = freeze;
    }

    public bool CanMove()
    {
        if (isMovementFrozen)
            return false;

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive)
            return false;

        if (JournalManager.Instance != null &&
            JournalManager.Instance.IsJournalOpen)
            return false;

        if (SleepManager.Instance != null &&
            SleepManager.Instance.IsSleeping)
            return false;

        if (FadeManager.Instance != null &&
            FadeManager.Instance.IsFading)
            return false;

        return true;
    }

    private void Update()
    {
        // Player cannot move right now
        if (!CanMove())
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        // Horizontal input
        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        // Vertical input
        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        // Create movement vector
        Vector3 movement = new Vector3(
            horizontal,
            vertical,
            0f
        ).normalized;

        // Move player
        transform.position += movement * moveSpeed * Time.deltaTime;

        bool isMoving = movement != Vector3.zero;

        // Tell Animator whether we're walking
        animator.SetBool("IsMoving", isMoving);

        // Only change facing direction while moving
        if (isMoving)
        {
            // Use the dominant axis so we only use
            // one of the four directional animations.
            if (Mathf.Abs(horizontal) > Mathf.Abs(vertical))
            {
                animator.SetFloat("MoveX", Mathf.Sign(horizontal));
                animator.SetFloat("MoveY", 0f);
            }
            else
            {
                animator.SetFloat("MoveX", 0f);
                animator.SetFloat("MoveY", Mathf.Sign(vertical));
            }
        }
    }
}