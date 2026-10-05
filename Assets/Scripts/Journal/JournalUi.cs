using UnityEngine;
using TMPro;

public class JournalUi : MonoBehaviour
{
    [SerializeField] private TMP_Text journalTitle;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private TMP_Text journalText;

    [SerializeField] private GameObject previousButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pageFlipSound;

    private void Awake()
    {
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

    public void Open(JournalEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("No journal entry provided.");
            return;
        }

        if (journalPanel != null)
        {
            journalPanel.SetActive(true);
        }

        if (journalTitle != null)
        {
            journalTitle.text = entry.title;
        }

        if (journalText != null && entry.pages != null && entry.pages.Count > 0)
        {
            journalText.text = entry.pages[0];
        }

        UpdateButtons();
    }

    public void Close()
    {
        if (journalPanel != null)
        {
            journalPanel.SetActive(false);
        }
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