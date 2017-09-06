using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    public class InvertProcessor : InputBindingProcessor<AxisControl, float>
    {
        [SerializeField]
        bool m_Invert;

        public override float ProcessValue(AxisControl control, float newValue)
        {
            if (m_Invert)
                return -newValue;
            return newValue;
        }

        #if UNITY_EDITOR
        public override void OnGUI(Rect position)
        {
            m_Invert = EditorGUI.Toggle(position, "Invert", m_Invert);
        }

        public override float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        #endif
    }
}
