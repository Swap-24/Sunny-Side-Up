using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoomExit : MonoBehaviour, IPlayerTrigger
{
    [Header("Destination")]
    [SerializeField] private string destinationScene;
    [SerializeField] private string destinationSpawnPoint;

    [Header("Requirements")]
    [SerializeField] private bool requiresJournal;

    [Header("Blocked Dialogue")]
    [SerializeField] private DialogueData blockedDialogue;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning = false;

    public void OnPlayerEnter()
    {
        if (isTransitioning)
            return;

        if (requiresJournal && (GameState.Instance == null || !GameState.Instance.HasReadJournal))
        {
            if (DialogueManager.Instance != null && blockedDialogue != null)
            {
                DialogueManager.Instance.StartDialogue(blockedDialogue);
            }
            return;
        }

        StartCoroutine(TransitionRoutine());
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