using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEditor;

[CustomEditor(typeof(InputAction))]
public class InputActionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Select the main Action Map asset to edit actions.", MessageType.Info);
    }
}
