using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "New Journal Entry",
    menuName = "Journal/Journal Entry"
)]
public class JournalEntry : ScriptableObject
{
    [Header("Entry Information")]
    public string title;

    [Header("Pages")]
    [TextArea(5, 15)]
    public List<string> pages;
}