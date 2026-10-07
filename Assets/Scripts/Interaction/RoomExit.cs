using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomExit : MonoBehaviour, IPlayerTrigger
{
    [Header("Destination")]
    [SerializeField] private string destinationScene;
    [SerializeField] private string destinationSpawnPoint;

    [Header("Blockage / Quest Requirement (Optional)")]
    [Tooltip("Flag required in GameState to pass through this exit (e.g. 'ReadJournal', 'TalkedToDad'). Leave empty if unblocked.")]
    [SerializeField] private string requiredFlag = "ReadJournal";

    [Tooltip("Dialogue played when the exit is blocked.")]
    [SerializeField] private InteractionDialogue blockedDialogue;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 1f;

    // Legacy support for older scene instances
    [SerializeField, HideInInspector] private bool requiresJournal = false;

    private bool isTransitioning = false;

    public void OnPlayerEnter()
    {
        if (isTransitioning)
            return;

        if (IsBlocked())
        {
            if (DialogueManager.Instance != null && blockedDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(blockedDialogue);
            }
            return;
        }

        StartCoroutine(TransitionRoutine());
    }

    private bool IsBlocked()
    {
        // Check if legacy requiresJournal is checked
        if (requiresJournal && (GameState.Instance == null || !GameState.Instance.HasFlag("ReadJournal")))
        {
            return true;
        }

        // Check modular flag
        if (!string.IsNullOrEmpty(requiredFlag))
        {
            if (GameState.Instance == null || !GameState.Instance.HasFlag(requiredFlag))
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        if (PlayerMove.Instance != null)
        {
            PlayerMove.Instance.FreezeMovement();
        }

        if (GameState.Instance != null)
        {
            GameState.Instance.SetTargetSpawnPoint(destinationSpawnPoint);
        }

        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.FadeToBlack(fadeDuration));
        }

        if (!string.IsNullOrEmpty(destinationScene))
        {
            SceneManager.LoadScene(destinationScene);
        }
        else
        {
            Debug.LogWarning("RoomExit: Destination scene name is empty!");
            isTransitioning = false;
            if (PlayerMove.Instance != null)
            {
                PlayerMove.Instance.UnfreezeMovement();
            }
        }
    }
}