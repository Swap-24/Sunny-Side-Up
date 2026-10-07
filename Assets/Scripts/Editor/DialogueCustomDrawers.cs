#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// =========================================================================
// CUSTOM EDITOR FOR INTERACTION DIALOGUE (SINGLE ASSET FOR OBJECT INSPECTION)
// =========================================================================
[CustomEditor(typeof(InteractionDialogue), true)]
public class InteractionDialogueEditor : Editor
{
    private SerializedProperty firstTimeLinesProp;
    private SerializedProperty repeatableLinesProp;
    private ReorderableList firstTimeList;
    private ReorderableList repeatableList;

    private void OnEnable()
    {
        if (target == null || serializedObject == null)
            return;

        firstTimeLinesProp = serializedObject.FindProperty("firstTimeLines");
        repeatableLinesProp = serializedObject.FindProperty("repeatableLines");

        if (firstTimeLinesProp != null)
            firstTimeList = CreateDialogueReorderableList(firstTimeLinesProp, "🌟 First-Time Dialogue (Played Once on First Inspect)");

        if (repeatableLinesProp != null)
            repeatableList = CreateDialogueReorderableList(repeatableLinesProp, "🔁 Repeatable Dialogue (Played on Subsequent Inspects)");
    }

    private ReorderableList CreateDialogueReorderableList(SerializedProperty property, string headerTitle)
    {
        ReorderableList list = new ReorderableList(serializedObject, property, true, true, true, true);

        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, headerTitle, EditorStyles.boldLabel);
        };

        list.elementHeightCallback = (int index) =>
        {
            if (index < 0 || index >= property.arraySize)
                return EditorGUIUtility.singleLineHeight;

            SerializedProperty element = property.GetArrayElementAtIndex(index);
            return element.isExpanded ? 92f : 70f;
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            if (index < 0 || index >= property.arraySize)
                return;

            SerializedProperty element = property.GetArrayElementAtIndex(index);
            SerializedProperty textProp = element.FindPropertyRelative("text");
            SerializedProperty speedProp = element.FindPropertyRelative("textSpeed");

            float lineH = EditorGUIUtility.singleLineHeight;
            float fullW = rect.width;
            float currentY = rect.y + 4f;

            // Row 1: Line label & Speed toggle
            Rect numRect = new Rect(rect.x, currentY, fullW * 0.45f, lineH);
            Rect toggleRect = new Rect(rect.x + fullW * 0.5f, currentY, fullW * 0.5f, lineH);

            EditorGUI.LabelField(numRect, $"Line {index + 1}", EditorStyles.miniBoldLabel);
            element.isExpanded = EditorGUI.Foldout(toggleRect, element.isExpanded, "Speed Override", true);
            currentY += lineH + 2f;

            // Optional Speed field
            if (element.isExpanded)
            {
                Rect speedRect = new Rect(rect.x, currentY, fullW, lineH);
                speedProp.floatValue = EditorGUI.FloatField(speedRect, "Speed (0=Default)", speedProp.floatValue);
                currentY += lineH + 4f;
            }

            // Dialogue text area
            Rect textRect = new Rect(rect.x, currentY, fullW, 40f);
            GUIStyle areaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            textProp.stringValue = EditorGUI.TextArea(textRect, textProp.stringValue ?? "", areaStyle);
        };

        list.onAddCallback = (ReorderableList l) =>
        {
            int newIdx = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = newIdx;
            SerializedProperty elem = l.serializedProperty.GetArrayElementAtIndex(newIdx);
            elem.FindPropertyRelative("text").stringValue = "";
            elem.FindPropertyRelative("textSpeed").floatValue = 0f;
            elem.FindPropertyRelative("speaker").stringValue = "Sol";
        };

        return list;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (firstTimeList == null || repeatableList == null)
        {
            OnEnable();
        }

        // Info Banner
        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox("Speaker: Sol (Voiced automatically using Sol's profile & typewriter sound).", MessageType.Info);
        EditorGUILayout.Space(4);

        if (firstTimeList != null)
            firstTimeList.DoLayoutList();

        EditorGUILayout.Space(8);

        if (repeatableList != null)
            repeatableList.DoLayoutList();

        EditorGUILayout.Space(4);

        serializedObject.ApplyModifiedProperties();
    }
}

// =========================================================================
// CUSTOM PROPERTY DRAWER FOR CONVERSATION LINE (STORY / MULTI-SPEAKER)
// =========================================================================
[CustomPropertyDrawer(typeof(ConversationLine))]
public class ConversationLineDrawer : PropertyDrawer
{
    private static List<SpeakerProfile> cachedProfiles;
    private static string[] profileNames;
    private static double lastCacheTime;

    private const float VerticalPadding = 6f;
    private const float RowSpacing = 4f;
    private const float TextAreaHeight = 44f;

    private static void RefreshProfiles()
    {
        if (cachedProfiles != null && (EditorApplication.timeSinceStartup - lastCacheTime) < 2.0)
            return;

        cachedProfiles = new List<SpeakerProfile>();
        List<string> names = new List<string> { "<None / Custom Name>" };

        string[] guids = AssetDatabase.FindAssets("t:SpeakerProfile");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpeakerProfile profile = AssetDatabase.LoadAssetAtPath<SpeakerProfile>(path);
            if (profile != null)
            {
                cachedProfiles.Add(profile);
                string displayName = string.IsNullOrEmpty(profile.SpeakerName)
                    ? profile.name
                    : $"{profile.SpeakerName} ({profile.name})";
                names.Add(displayName);
            }
        }

        profileNames = names.ToArray();
        lastCacheTime = EditorApplication.timeSinceStartup;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float totalHeight = VerticalPadding * 2f;

        // Row 1: Speaker dropdown
        totalHeight += lineH;

        // Row 1.5: Custom speaker name (if no profile selected)
        SerializedProperty speakerProfileProp = property.FindPropertyRelative("speakerProfile");
        if (speakerProfileProp != null && speakerProfileProp.objectReferenceValue == null)
        {
            totalHeight += RowSpacing + lineH;
        }

        // Row 2: Text area (label + box)
        totalHeight += RowSpacing + lineH + 2f + TextAreaHeight;

        // Row 3: Speed & Overrides foldout toggle
        totalHeight += RowSpacing + lineH;

        // Row 4 & 5: Overrides (if expanded)
        if (property.isExpanded)
        {
            totalHeight += RowSpacing + lineH; // Voice Override
            totalHeight += RowSpacing + lineH; // Pitch Override
        }

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RefreshProfiles();

        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty speakerProfileProp = property.FindPropertyRelative("speakerProfile");
        SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty textSpeedProp = property.FindPropertyRelative("textSpeed");
        SerializedProperty voiceOverrideProp = property.FindPropertyRelative("voiceOverride");
        SerializedProperty pitchOverrideProp = property.FindPropertyRelative("pitchOverride");

        // Background card
        Rect boxRect = new Rect(position.x, position.y + 2f, position.width, position.height - 4f);
        GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

        float lineH = EditorGUIUtility.singleLineHeight;
        float contentX = position.x + 8f;
        float contentW = position.width - 16f;
        float currentY = position.y + VerticalPadding;

        // --- ROW 1: Speaker Profile Dropdown ---
        float labelW = 60f;
        Rect speakerLabelRect = new Rect(contentX, currentY, labelW, lineH);
        Rect speakerPopupRect = new Rect(contentX + labelW, currentY, contentW - labelW, lineH);

        EditorGUI.LabelField(speakerLabelRect, "Speaker:", EditorStyles.boldLabel);

        int currentIndex = 0;
        SpeakerProfile currentProfile = speakerProfileProp.objectReferenceValue as SpeakerProfile;
        if (currentProfile != null && cachedProfiles != null)
        {
            int found = cachedProfiles.IndexOf(currentProfile);
            if (found >= 0)
            {
                currentIndex = found + 1;
            }
        }

        int newIndex = EditorGUI.Popup(speakerPopupRect, currentIndex, profileNames);
        if (newIndex != currentIndex)
        {
            if (newIndex == 0)
            {
                speakerProfileProp.objectReferenceValue = null;
            }
            else if (cachedProfiles != null && (newIndex - 1) < cachedProfiles.Count)
            {
                speakerProfileProp.objectReferenceValue = cachedProfiles[newIndex - 1];
            }
        }

        currentY += lineH;

        // --- ROW 1.5: Custom Name (if profile is None) ---
        if (speakerProfileProp.objectReferenceValue == null)
        {
            currentY += RowSpacing;
            Rect customNameRect = new Rect(contentX, currentY, contentW, lineH);
            speakerProp.stringValue = EditorGUI.TextField(customNameRect, "Custom Name", speakerProp.stringValue);
            currentY += lineH;
        }

        // --- ROW 2: Dialogue Text Area ---
        currentY += RowSpacing;
        Rect textLabelRect = new Rect(contentX, currentY, contentW, lineH);
        EditorGUI.LabelField(textLabelRect, "Dialogue Text:", EditorStyles.miniBoldLabel);
        currentY += lineH + 2f;

        Rect textAreaRect = new Rect(contentX, currentY, contentW, TextAreaHeight);
        GUIStyle textStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
        textProp.stringValue = EditorGUI.TextArea(textAreaRect, textProp.stringValue ?? "", textStyle);
        currentY += TextAreaHeight;

        // --- ROW 3: Speed & Overrides Toggle ---
        currentY += RowSpacing;
        float halfW = (contentW - 8f) * 0.5f;
        Rect speedRect = new Rect(contentX, currentY, halfW, lineH);
        Rect foldoutRect = new Rect(contentX + halfW + 8f, currentY, halfW, lineH);

        textSpeedProp.floatValue = EditorGUI.FloatField(speedRect, "Speed (0=Def)", textSpeedProp.floatValue);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "Overrides", true);
        currentY += lineH;

        // --- ROW 4 & 5: Overrides (if expanded) ---
        if (property.isExpanded)
        {
            currentY += RowSpacing;
            Rect voiceRect = new Rect(contentX, currentY, contentW, lineH);
            voiceOverrideProp.objectReferenceValue = EditorGUI.ObjectField(
                voiceRect,
                "Voice Clip",
                voiceOverrideProp.objectReferenceValue,
                typeof(AudioClip),
                false
            );
            currentY += lineH;

            currentY += RowSpacing;
            Rect pitchRect = new Rect(contentX, currentY, contentW, lineH);
            pitchOverrideProp.floatValue = EditorGUI.Slider(
                pitchRect,
                "Pitch (0=Def)",
                pitchOverrideProp.floatValue,
                0f,
                3f
            );
            currentY += lineH;
        }

        EditorGUI.EndProperty();
    }
}

// =========================================================================
// CUSTOM PROPERTY DRAWER FOR DIALOGUE LINE (CLEAN/MINIMALIST FOR INTERACTION)
// =========================================================================
[CustomPropertyDrawer(typeof(DialogueLine))]
public class DialogueLineDrawer : PropertyDrawer
{
    private const float VerticalPadding = 4f;
    private const float RowSpacing = 4f;
    private const float TextAreaHeight = 44f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineH = EditorGUIUtility.singleLineHeight;
        float totalHeight = VerticalPadding * 2f;

        // Row 1: Header + Foldout
        totalHeight += lineH;

        // Row 2: Text Area
        totalHeight += RowSpacing + TextAreaHeight;

        // Row 3: Speed Override (if expanded)
        if (property.isExpanded)
        {
            totalHeight += RowSpacing + lineH;
        }

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty textProp = property.FindPropertyRelative("text");
        SerializedProperty textSpeedProp = property.FindPropertyRelative("textSpeed");

        Rect boxRect = new Rect(position.x, position.y + 1f, position.width, position.height - 2f);
        GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

        float lineH = EditorGUIUtility.singleLineHeight;
        float contentX = position.x + 6f;
        float contentW = position.width - 12f;
        float currentY = position.y + VerticalPadding;

        // Row 1: Header & Foldout
        Rect labelRect = new Rect(contentX, currentY, contentW * 0.5f, lineH);
        Rect foldoutRect = new Rect(contentX + contentW * 0.5f, currentY, contentW * 0.5f, lineH);

        EditorGUI.LabelField(labelRect, label.text, EditorStyles.miniBoldLabel);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, "Speed Override", true);
        currentY += lineH;

        // Row 2: Text Area
        currentY += RowSpacing;
        Rect textAreaRect = new Rect(contentX, currentY, contentW, TextAreaHeight);
        GUIStyle textStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
        textProp.stringValue = EditorGUI.TextArea(textAreaRect, textProp.stringValue ?? "", textStyle);
        currentY += TextAreaHeight;

        // Row 3: Speed (if expanded)
        if (property.isExpanded)
        {
            currentY += RowSpacing;
            Rect speedRect = new Rect(contentX, currentY, contentW, lineH);
            textSpeedProp.floatValue = EditorGUI.FloatField(speedRect, "Speed (0=Def)", textSpeedProp.floatValue);
        }

        EditorGUI.EndProperty();
    }
}
#endif
