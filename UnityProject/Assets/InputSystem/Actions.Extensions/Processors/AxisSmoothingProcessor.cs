using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    public class AxisSmoothingProcessor : InputBindingProcessor<AxisControl, float>
    {
        [SerializeField]
        float m_Gravity = 3;
        [SerializeField]
        float m_Sensitivity = 3;
        [SerializeField]
        bool m_Snap;

        public override float ProcessValue(AxisControl control, float newValue)
        {
            // If new value is opposite sign of old value, snap to zero if snapping is enabled.
            // (We can't use Sign function as it returns positive sign for zero
            // and zero should not count as opposite of negative numbers.)
            if (m_Snap && newValue * control.previousValue < 0)
                newValue = 0;
            else if (newValue != 0)
                newValue = Mathf.MoveTowards(control.previousValue, newValue, m_Sensitivity * Time.deltaTime);
            else
                newValue = Mathf.MoveTowards(control.previousValue, 0, m_Gravity * Time.deltaTime);

            return newValue;
        }

        #if UNITY_EDITOR
        public override void OnGUI(Rect position)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(position, "Smoothing");
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;
            m_Gravity = EditorGUI.FloatField(position, "Gravity", m_Gravity);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            m_Sensitivity = EditorGUI.FloatField(position, "Sensitivity", m_Sensitivity);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            m_Snap = EditorGUI.Toggle(position, "Snap", m_Snap);
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 3;
        }

        #endif
    }
}
