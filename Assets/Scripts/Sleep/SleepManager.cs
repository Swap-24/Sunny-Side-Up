using UnityEngine;
using System.Collections;

public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance { get; private set; }

    [SerializeField] private bool canSleep = true;

    [Header("Dialogue")]
    [SerializeField] private InteractionDialogue notSleepyDialogue;

    public bool IsSleeping { get; private set; }

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

    public void TrySleep()
    {
        if (!canSleep)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(notSleepyDialogue);
            }
            return;
        }

        IsSleeping = true;

        if (FadeManager.Instance != null)
        {
            StartCoroutine(
                FadeManager.Instance.FadeToBlack(2f, AfterSleep)
            );
        }
        else
        {
            AfterSleep();
        }
    }

    private void AfterSleep()
    {
        StartCoroutine(FadeBackIn());
    }

    private IEnumerator FadeBackIn()
    {
        yield return new WaitForSeconds(1f);

        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(
                FadeManager.Instance.FadeFromBlack(2f)
            );
        }

        IsSleeping = false;
    }
}