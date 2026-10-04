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

    public void Open(JournalEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("No journal entry provided.");
            return;
        }

        journalPanel.SetActive(true);

        journalTitle.text = entry.title;
        journalText.text = entry.pages[0];

        UpdateButtons();
    }

    public void Close()
    {
        journalPanel.SetActive(false);
    }

    public void DisplayPage(string text)
    {
        journalText.text = text;
    }

public void NextPage()
{
    JournalManager.Instance.NextPage();

    if (audioSource != null && pageFlipSound != null)
    {
        audioSource.PlayOneShot(pageFlipSound);
    }

    UpdatePage();
}

public void PreviousPage()
{
    JournalManager.Instance.PreviousPage();

    if (audioSource != null && pageFlipSound != null)
    {
        audioSource.PlayOneShot(pageFlipSound);
    }

    UpdatePage();
}

    private void UpdatePage()
    {
        JournalEntry entry = JournalManager.Instance.CurrentJournalEntry;

        if (entry == null)
            return;

        int page = JournalManager.Instance.CurrentPage;

        // Update the title too.
        // This is important when moving from Day 1 to Day 2.
        journalTitle.text = entry.title;

        journalText.text = entry.pages[page];

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        JournalEntry entry = JournalManager.Instance.CurrentJournalEntry;

        if (entry == null || entry.pages == null || entry.pages.Count == 0)
            return;

        int currentPage = JournalManager.Instance.CurrentPage;
        int lastPage = entry.pages.Count - 1;

        // Show Previous if:
        // - We are not on the first page
        // OR
        // - There is a previous journal entry
        bool canGoPrevious =
            currentPage > 0 ||
            JournalManager.Instance.CurrentEntryIndex > 0;

        // Show Next if:
        // - We are not on the last page
        // OR
        // - There is another journal entry
        bool canGoNext =
            currentPage < lastPage ||
            JournalManager.Instance.CurrentEntryIndex <
            JournalManager.Instance.JournalEntryCount - 1;

        previousButton.SetActive(canGoPrevious);
        nextButton.SetActive(canGoNext);
    }
}