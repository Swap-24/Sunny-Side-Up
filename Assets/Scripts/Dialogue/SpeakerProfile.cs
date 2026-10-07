using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeaker", menuName = "Dialogue/Speaker Profile")]
public class SpeakerProfile : ScriptableObject
{
    [Header("Speaker Info")]
    [Tooltip("The display name shown in the dialogue nameplate.")]
    [SerializeField] private string speakerName = "Sol";

    [Tooltip("Color of the speaker's name in the UI.")]
    [SerializeField] private Color nameColor = Color.white;

    [Header("Voice Configuration")]
    [Tooltip("Voice audio clip played per letter typed.")]
    [SerializeField] private AudioClip voiceSound;

    [Range(0.1f, 3f)]
    [Tooltip("Base pitch of the character's voice.")]
    [SerializeField] private float voicePitch = 1f;

    [Range(0f, 0.2f)]
    [Tooltip("Subtle random pitch variance per letter for organic, lively speech.")]
    [SerializeField] private float pitchVariance = 0.05f;

    [Range(0f, 1f)]
    [Tooltip("Voice volume multiplier.")]
    [SerializeField] private float voiceVolume = 1f;

    [Header("Typing Preferences")]
    [Tooltip("Default typing speed in characters per second.")]
    [SerializeField] private float defaultTextSpeed = 35f;

    public string SpeakerName => speakerName;
    public Color NameColor => nameColor;
    public AudioClip VoiceSound => voiceSound;
    public float VoicePitch => voicePitch;
    public float PitchVariance => pitchVariance;
    public float VoiceVolume => voiceVolume;
    public float DefaultTextSpeed => defaultTextSpeed > 0 ? defaultTextSpeed : 35f;
}

