using UnityEngine;
using System.Collections.Generic;

public class JournalManager : MonoBehaviour
{
    public static JournalManager Instance { get; private set; }

    [Header("Journal Entries")]
    [SerializeField] private List<JournalEntry> journalEntries;

    [SerializeField] private JournalUi journalUi;

    private int currentEntryIndex;
    private int currentPage;

    public JournalEntry CurrentJournalEntry
    {
        get
        {
            if (journalEntries == null || journalEntries.Count == 0)
                return null;

            return journalEntries[currentEntryIndex];
        }
    }

    public int CurrentPage => currentPage;

    public int CurrentEntryIndex => currentEntryIndex;

    public int JournalEntryCount => journalEntries != null ? journalEntries.Count : 0;

    public bool IsJournalOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void OpenJournal()
    {
        if (journalEntries == null || journalEntries.Count == 0)
        {
            Debug.LogWarning("No journal entries found.");
            return;
        }

        currentEntryIndex = 0;
        currentPage = 0;

        IsJournalOpen = true;

        if (journalUi != null)
        {
            journalUi.Open(CurrentJournalEntry);
        }
    }

    public void CloseJournal()
    {
        IsJournalOpen = false;

        if (journalUi != null)
        {
            journalUi.Close();
        }
    }

    public void NextPage()
    {
        JournalEntry entry = CurrentJournalEntry;

        if (entry == null)
            return;

        // Move to the next page of the current entry
        if (currentPage < entry.pages.Count - 1)
        {
            currentPage++;

            // We have reached the last page of THIS journal entry
            if (currentPage == entry.pages.Count - 1)
            {
                if (GameState.Instance != null)
                {
                    GameState.Instance.MarkJournalAsRead();
                }
            }

            return;
        }

        // We are already on the last page of this entry.
        // Move to the next journal entry if one exists.
        if (currentEntryIndex < journalEntries.Count - 1)
        {
            currentEntryIndex++;
            currentPage = 0;
        }
    }

    public void PreviousPage()
    {
        // Move to previous page in the current entry
        if (currentPage > 0)
        {
            currentPage--;
            return;
        }

        // We are on the first page of this entry.
        // Move to the previous journal entry if one exists.
        if (currentEntryIndex > 0)
        {
            currentEntryIndex--;

            JournalEntry previousEntry = CurrentJournalEntry;

            if (previousEntry != null && previousEntry.pages != null)
            {
                currentPage = previousEntry.pages.Count - 1;
            }
        }
    }
}