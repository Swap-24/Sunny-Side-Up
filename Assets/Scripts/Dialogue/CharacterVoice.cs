using UnityEngine;

public class CharacterVoice : MonoBehaviour
{
    [Header("Character Identification")]
    [Tooltip("The speaker name matching the DialogueLine speaker field (e.g. 'Sol').")]
    [SerializeField] private string characterName = "Sol";

    [Header("Voice Audio")]
    [Tooltip("Audio clip to play per letter typed.")]
    [SerializeField] private AudioClip voice;

    [Range(0.1f, 3f)]
    [Tooltip("Pitch multiplier for the voice clip.")]
    [SerializeField] private float pitch = 1f;

    [Range(0f, 1f)]
    [Tooltip("Volume multiplier for the voice clip.")]
    [SerializeField] private float volume = 1f;

    public string CharacterName => characterName;
    public AudioClip Voice => voice;
    public float Pitch => pitch;
    public float Volume => volume;

    private void Awake()
    {
        Register();
    }

    private void OnEnable()
    {
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void OnDestroy()
    {
        Unregister();
    }

    private void Register()
    {
        if (!string.IsNullOrEmpty(characterName))
        {
            DialogueManager.RegisterVoice(characterName, this);
        }
    }

    private void Unregister()
    {
        if (!string.IsNullOrEmpty(characterName))
        {
            DialogueManager.UnregisterVoice(characterName, this);
        }
    }
}
