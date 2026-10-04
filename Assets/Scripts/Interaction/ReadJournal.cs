using UnityEngine;

public class ReadJournal : MonoBehaviour, IInteractable
{
    public string GetInteractionText()
    {
        return "[E] - Open Journal";
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        JournalManager.Instance.OpenJournal();
    }
}