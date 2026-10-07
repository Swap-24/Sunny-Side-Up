using System;
using UnityEngine;

[Serializable]
public class ConversationLine
{
    [Tooltip("Speaker profile (controls name, nameplate color, and voice blip).")]
    [SerializeField] private SpeakerProfile speakerProfile;

    [Tooltip("Fallback speaker name (used if Speaker Profile is not assigned).")]
    public string speaker;

    [Tooltip("The dialogue text to display.")]
    public string text;

    [Tooltip("Characters typed per second (0 = use profile default, ~35).")]
    public float textSpeed = 0f;

    [Tooltip("Override voice sound clip for this line (e.g. scream, giggle, gasp).")]
    [SerializeField] private AudioClip voiceOverride;

    [Tooltip("Override pitch for this specific line (0 = use profile default).")]
    [SerializeField] private float pitchOverride = 0f;

    // Helper Properties
    public SpeakerProfile Profile => speakerProfile;

    public string SpeakerName
    {
        get
        {
            if (speakerProfile != null && !string.IsNullOrEmpty(speakerProfile.SpeakerName))
                return speakerProfile.SpeakerName;
            return speaker;
        }
    }

    public Color SpeakerColor
    {
        get
        {
            if (speakerProfile != null)
                return speakerProfile.NameColor;
            return Color.white;
        }
    }

    public AudioClip VoiceClip
    {
        get
        {
            if (voiceOverride != null)
                return voiceOverride;
            if (speakerProfile != null)
                return speakerProfile.VoiceSound;
            return null;
        }
    }

    public float BasePitch
    {
        get
        {
            if (pitchOverride > 0.01f)
                return pitchOverride;
            if (speakerProfile != null)
                return speakerProfile.VoicePitch;
            return 1f;
        }
    }

    public float PitchVariance
    {
        get
        {
            if (speakerProfile != null)
                return speakerProfile.PitchVariance;
            return 0.05f;
        }
    }

    public float Volume
    {
        get
        {
            if (speakerProfile != null)
                return speakerProfile.VoiceVolume;
            return 1f;
        }
    }

    public float EffectiveSpeed
    {
        get
        {
            if (textSpeed > 0f)
                return textSpeed;
            if (speakerProfile != null && speakerProfile.DefaultTextSpeed > 0f)
                return speakerProfile.DefaultTextSpeed;
            return 35f;
        }
    }
}

