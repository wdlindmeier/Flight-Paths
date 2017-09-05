using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    public class AxisMultiplierProcessor : InputBindingProcessor<AxisControl, float>
    {
        [SerializeField]
        float m_Multiplier;

        public override float ProcessValue(AxisControl control, float newValue)
        {
            return newValue * m_Multiplier;
        }

        #if UNITY_EDITOR
        public override void OnGUI(Rect position)
        {
            m_Multiplier = EditorGUI.FloatField(position, "Multiplier", m_Multiplier);
        }

        public override float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        #endif
    }
}
