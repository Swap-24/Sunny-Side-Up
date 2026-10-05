using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    public bool IsDialogueActive { get; private set; }

    private DialogueData currentDialogue;
    private int currentLineIndex;

    private Coroutine typingCoroutine;

    private bool isTyping;
    private bool skipTyping;

    private static readonly Dictionary<string, CharacterVoice> registeredVoices = new(StringComparer.OrdinalIgnoreCase);

    public static void RegisterVoice(string name, CharacterVoice voice)
    {
        if (!string.IsNullOrEmpty(name) && voice != null)
        {
            registeredVoices[name] = voice;
        }
    }

    public static void UnregisterVoice(string name, CharacterVoice voice)
    {
        if (!string.IsNullOrEmpty(name) && registeredVoices.TryGetValue(name, out var current) && current == voice)
        {
            registeredVoices.Remove(name);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
    }

    private CharacterVoice GetVoiceForSpeaker(string speakerName)
    {
        if (string.IsNullOrEmpty(speakerName))
            return null;

        if (registeredVoices.TryGetValue(speakerName, out var registered) && registered != null)
        {
            return registered;
        }

        // Fallback search in scene
        CharacterVoice[] allVoices = FindObjectsByType<CharacterVoice>();
        foreach (var v in allVoices)
        {
            if (v != null && string.Equals(v.CharacterName, speakerName, StringComparison.OrdinalIgnoreCase))
            {
                registeredVoices[speakerName] = v;
                return v;
            }
        }

        return null;
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines.Count == 0)
            return;

        currentDialogue = dialogue;
        currentLineIndex = 0;

        IsDialogueActive = true;
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];

        speakerText.text = line.speaker;
        dialogueText.text = "";

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
        skipTyping = false;

        CharacterVoice speakerVoice = GetVoiceForSpeaker(line.speaker);
        AudioClip voiceClip = speakerVoice != null ? speakerVoice.Voice : null;
        float pitch = speakerVoice != null ? speakerVoice.Pitch : 1f;
        float volume = speakerVoice != null ? speakerVoice.Volume : 1f;

        if (line.textSpeed <= 0)
        {
            dialogueText.text = line.text;
            isTyping = false;
            yield break;
        }

        float delay = 1f / line.textSpeed;

        foreach (char character in line.text)
        {
            if (skipTyping)
            {
                dialogueText.text = line.text;
                break;
            }

            dialogueText.text += character;

            // Play voice sound on letters
            if (char.IsLetter(character) && voiceClip != null && audioSource != null)
            {
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(voiceClip, volume);
            }

            yield return new WaitForSeconds(delay);
        }

        dialogueText.text = line.text;
        isTyping = false;
    }

    public void HandleInput()
    {
        if (!IsDialogueActive)
            return;

        // If text is still typing, E instantly finishes the line.
        if (isTyping)
        {
            skipTyping = true;
            return;
        }

        // Otherwise move to the next line.
        if (currentLineIndex < currentDialogue.lines.Count - 1)
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
        else
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        IsDialogueActive = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        currentDialogue = null;
        currentLineIndex = 0;

        isTyping = false;
        skipTyping = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}