using UnityEngine;
using UnityEditor;

namespace UnityEngine.Experimental.Input
{
    public static class InputSystemEditorUtility
    {
        static SerializedObject s_PlayerSettings;
        static SerializedProperty s_EnabledInputSystemProperty;
        static GUIStyle s_WarningIcon = new GUIStyle("CN EntryWarn");

        static InputSystemEditorUtility()
        {
            PlayerSettings[] array = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            var playerSettings = array.Length > 0 ? array[0] : null;
            if (playerSettings)
            {
                s_PlayerSettings = new SerializedObject(playerSettings);
                s_EnabledInputSystemProperty = s_PlayerSettings.FindProperty("enableNativePlatformBackendsForNewInputSystem");
            }

            s_WarningIcon.margin = new RectOffset(5, 5, 5, 5);
        }

        public static bool inputSystemEnabled
        {
            get
            {
                s_PlayerSettings.UpdateIfRequiredOrScript();
                return s_EnabledInputSystemProperty.boolValue;
            }
            set
            {
                s_PlayerSettings.UpdateIfRequiredOrScript();
                if (s_EnabledInputSystemProperty.boolValue == value)
                    return;
                EditorUtility.DisplayDialog("Unity editor restart required", "The Unity editor must be restarted for this change to take effect.", "OK");
                s_EnabledInputSystemProperty.boolValue = value;
                s_PlayerSettings.ApplyModifiedProperties();
            }
        }

        public static void ShowSystemNotEnabledHelpbox()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(GUIContent.none, s_WarningIcon, GUILayout.ExpandWidth(false));
            GUILayout.Label("The Input System (Preview) must be enabled in this project in order for Action Maps to work.", EditorStyles.wordWrappedMiniLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Enable", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                inputSystemEnabled = true;
            EditorGUILayout.EndHorizontal();
        }
    }
}
