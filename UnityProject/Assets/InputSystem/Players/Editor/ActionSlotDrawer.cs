using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ActionSlot), true)]
public class ActionSlotDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.ObjectField(position, property.FindPropertyRelative("action"), label);
    }
}
