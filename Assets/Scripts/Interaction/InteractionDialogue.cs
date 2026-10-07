using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInteractionDialogue", menuName = "Dialogue/Interaction Dialogue")]
public class InteractionDialogue : ScriptableObject
{
    [Tooltip("Special dialogue played ONLY the first time the player inspects this object. (Leave empty if none).")]
    public List<DialogueLine> firstTimeLines = new();

    [Tooltip("Repeatable dialogue played on subsequent inspections (or always, if firstTimeLines is empty).")]
    public List<DialogueLine> repeatableLines = new();

    public bool HasFirstTimeDialogue => firstTimeLines != null && firstTimeLines.Count > 0;
    public bool HasRepeatableDialogue => repeatableLines != null && repeatableLines.Count > 0;

    public List<DialogueLine> GetLines(bool isFirstTime)
    {
        if (isFirstTime && HasFirstTimeDialogue)
            return firstTimeLines;

        if (HasRepeatableDialogue)
            return repeatableLines;

        return firstTimeLines;
    }
}
