using UnityEngine;
using TMPro;
using System.Collections;

public class JournalUi : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text journalTitle;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private TMP_Text journalText;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pageFlipSound;

    [Header("Animation Settings")]
    [SerializeField] private float openDuration = 0.22f;
    [SerializeField] private float closeDuration = 0.16f;
    [SerializeField] private float startScale = 0.3f;
    [SerializeField] private bool useFade = true;

    private Vector3 initialScale = Vector3.one;
    private bool isScaleCaptured = false;
    private CanvasGroup canvasGroup;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        InitializeComponents();

        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.RegisterUi(this);
        }
    }

    private void OnEnable()
    {
        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.RegisterUi(this);
        }
    }

    private void OnDestroy()
    {
        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.UnregisterUi(this);
        }
    }

    private void InitializeComponents()
    {
        GameObject targetObj = journalPanel != null ? journalPanel : gameObject;

        // Capture initial scale only ONCE so it never gets corrupted by scaled-down states
        if (!isScaleCaptured)
        {
            Vector3 currentScale = targetObj.transform.localScale;
            initialScale = currentScale != Vector3.zero ? currentScale : Vector3.one;
            isScaleCaptured = true;
        }

        if (useFade && canvasGroup == null)
        {
            canvasGroup = targetObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = targetObj.AddComponent<CanvasGroup>();
            }
        }
    }

    public void Open(JournalEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("No journal entry provided.");
            return;
        }

        // Activate GameObject and Panel first so coroutines can run
        gameObject.SetActive(true);
        if (journalPanel != null)
        {
            journalPanel.SetActive(true);
        }

        InitializeComponents();

        if (journalTitle != null)
        {
            journalTitle.text = entry.title;
        }

        if (journalText != null && entry.pages != null && entry.pages.Count > 0)
        {
            journalText.text = entry.pages[0];
        }

        UpdateButtons();

        if (audioSource != null && pageFlipSound != null)
        {
            audioSource.PlayOneShot(pageFlipSound);
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(AnimateOpen());
    }

    public void Close()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        if (gameObject.activeInHierarchy)
        {
            transitionCoroutine = StartCoroutine(AnimateClose());
        }
        else
        {
            if (journalPanel != null)
            {
                journalPanel.transform.localScale = initialScale;
                journalPanel.SetActive(false);
            }
        }
    }

    private IEnumerator AnimateOpen()
    {
        GameObject targetObj = journalPanel != null ? journalPanel : gameObject;
        Transform panelTransform = targetObj.transform;
        Vector3 fromScale = initialScale * startScale;
        Vector3 toScale = initialScale;

        panelTransform.localScale = fromScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        float timer = 0f;

        while (timer < openDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / openDuration);

            // Ease-out pop
            float easePop = 1f + 2.7f * Mathf.Pow(t - 1f, 3f) + 1.7f * Mathf.Pow(t - 1f, 2f);

            panelTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, easePop);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Clamp01(t * 1.5f);
            }

            yield return null;
        }

        panelTransform.localScale = toScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        transitionCoroutine = null;
    }

    private IEnumerator AnimateClose()
    {
        GameObject targetObj = journalPanel != null ? journalPanel : gameObject;
        Transform panelTransform = targetObj.transform;
        Vector3 fromScale = panelTransform.localScale;
        Vector3 toScale = initialScale * startScale;

        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        float timer = 0f;

        while (timer < closeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / closeDuration);

            float easeIn = t * t;
            panelTransform.localScale = Vector3.Lerp(fromScale, toScale, easeIn);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            }

            yield return null;
        }

        // Restore clean scale and alpha before deactivating so it is ready for next open
        panelTransform.localScale = initialScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (journalPanel != null)
        {
            journalPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

        transitionCoroutine = null;
    }

    public void DisplayPage(string text)
    {
        if (journalText != null)
        {
            journalText.text = text;
        }
    }

    public void NextPage()
    {
        if (JournalManager.Instance == null)
            return;

        JournalManager.Instance.NextPage();

        if (audioSource != null && pageFlipSound != null)
        {
            audioSource.PlayOneShot(pageFlipSound);
        }

        UpdatePage();
    }

    public void PreviousPage()
    {
        if (JournalManager.Instance == null)
            return;

        JournalManager.Instance.PreviousPage();

        if (audioSource != null && pageFlipSound != null)
        {
            audioSource.PlayOneShot(pageFlipSound);
        }

        UpdatePage();
    }

    private void UpdatePage()
    {
        if (JournalManager.Instance == null)
            return;

        JournalEntry entry = JournalManager.Instance.CurrentJournalEntry;
        if (entry == null)
            return;

        int page = JournalManager.Instance.CurrentPage;

        if (journalTitle != null)
        {
            journalTitle.text = entry.title;
        }

        if (journalText != null && entry.pages != null && page >= 0 && page < entry.pages.Count)
        {
            journalText.text = entry.pages[page];
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (JournalManager.Instance == null)
            return;

        JournalEntry entry = JournalManager.Instance.CurrentJournalEntry;
        if (entry == null || entry.pages == null || entry.pages.Count == 0)
            return;

        int currentPage = JournalManager.Instance.CurrentPage;
        int lastPage = entry.pages.Count - 1;

        bool canGoPrevious = currentPage > 0 || JournalManager.Instance.CurrentEntryIndex > 0;
        bool canGoNext = currentPage < lastPage || JournalManager.Instance.CurrentEntryIndex < JournalManager.Instance.JournalEntryCount - 1;

        if (previousButton != null)
        {
            previousButton.SetActive(canGoPrevious);
        }

        if (nextButton != null)
        {
            nextButton.SetActive(canGoNext);
        }
    }
}