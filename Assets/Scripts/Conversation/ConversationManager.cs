using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject conversationPanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Tooltip("Optional continue indicator prompt (e.g. arrow icon) shown when typing finishes.")]
    [SerializeField] private GameObject continuePrompt;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Punctuation Pacing (Seconds)")]
    [SerializeField] private float commaPause = 0.12f;
    [SerializeField] private float endSentencePause = 0.28f;
    [SerializeField] private float ellipsisPause = 0.45f;

    public bool IsConversationActive { get; private set; }

    private ConversationData currentConversation;
    private int currentLineIndex;
    private Action onConversationComplete;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool skipTyping;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (conversationPanel != null)
        {
            conversationPanel.SetActive(false);
        }

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
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

    public void StartConversation(ConversationData conversation, Action onComplete = null)
    {
        if (conversation == null || conversation.lines.Count == 0)
            return;

        currentConversation = conversation;
        currentLineIndex = 0;
        onConversationComplete = onComplete;

        IsConversationActive = true;
        if (conversationPanel != null)
        {
            conversationPanel.SetActive(true);
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        ConversationLine line = currentConversation.lines[currentLineIndex];

        // Update speaker name & color
        if (speakerText != null)
        {
            speakerText.text = line.SpeakerName;
            speakerText.color = line.SpeakerColor;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(ConversationLine line)
    {
        isTyping = true;
        skipTyping = false;

        string fullText = line.text ?? "";
        float speed = line.EffectiveSpeed;
        float baseDelay = speed > 0 ? (1f / speed) : 0.03f;

        AudioClip clipToPlay = line.VoiceClip;
        float basePitch = line.BasePitch;
        float pitchVariance = line.PitchVariance;
        float volume = line.Volume;

        StringBuilder displayedText = new StringBuilder();
        int i = 0;

        while (i < fullText.Length)
        {
            if (skipTyping)
            {
                dialogueText.text = fullText;
                break;
            }

            char c = fullText[i];

            // Rich Text Tag handling (instantly skips and renders tags like <color=...> or <b>)
            if (c == '<')
            {
                int closeTagIndex = fullText.IndexOf('>', i);
                if (closeTagIndex != -1)
                {
                    string tag = fullText.Substring(i, closeTagIndex - i + 1);
                    displayedText.Append(tag);
                    i = closeTagIndex + 1;
                    dialogueText.text = displayedText.ToString();
                    continue;
                }
            }

            displayedText.Append(c);
            dialogueText.text = displayedText.ToString();

            // Play voice blip sound on letters
            if (char.IsLetterOrDigit(c) && clipToPlay != null && audioSource != null)
            {
                float randomizedPitch = basePitch + UnityEngine.Random.Range(-pitchVariance, pitchVariance);
                audioSource.pitch = Mathf.Clamp(randomizedPitch, 0.1f, 3f);
                audioSource.PlayOneShot(clipToPlay, volume);
            }

            // Punctuation Pacing
            float waitTime = baseDelay;

            if (i < fullText.Length - 1 && !char.IsWhiteSpace(fullText[i]))
            {
                if (c == '.' && i + 2 < fullText.Length && fullText[i + 1] == '.' && fullText[i + 2] == '.')
                {
                    displayedText.Append("..");
                    dialogueText.text = displayedText.ToString();
                    i += 2;
                    waitTime = ellipsisPause;
                }
                else if (c == '.' || c == '!' || c == '?')
                {
                    waitTime = endSentencePause;
                }
                else if (c == ',' || c == ';' || c == ':' || c == '-')
                {
                    waitTime = commaPause;
                }
            }

            i++;
            yield return new WaitForSeconds(waitTime);
        }

        dialogueText.text = fullText;
        isTyping = false;

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(true);
        }
    }

    public void HandleInput()
    {
        if (!IsConversationActive)
            return;

        if (isTyping)
        {
            skipTyping = true;
            return;
        }

        if (currentLineIndex < currentConversation.lines.Count - 1)
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
        else
        {
            CloseConversation();
        }
    }

    public void CloseConversation()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        IsConversationActive = false;

        if (conversationPanel != null)
        {
            conversationPanel.SetActive(false);
        }

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        currentConversation = null;
        currentLineIndex = 0;
        isTyping = false;
        skipTyping = false;

        onConversationComplete?.Invoke();
        onConversationComplete = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

