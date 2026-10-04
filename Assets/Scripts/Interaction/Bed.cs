using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public string GetInteractionText()
    {
        return "[E] - Sleep";
    }

    public bool CanInteract()
    {
        return true;
    }

   public void Interact()
{
    SleepManager.Instance.TrySleep();
}
}