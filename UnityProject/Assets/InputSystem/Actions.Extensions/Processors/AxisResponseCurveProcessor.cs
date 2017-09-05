using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    public class AxisResponseCurveProcessor : InputBindingProcessor<AxisControl, float>
    {
        [SerializeField]
        AnimationCurve m_Curve;

        public override float ProcessValue(AxisControl control, float newValue)
        {
            return m_Curve.Evaluate(newValue);
        }

        #if UNITY_EDITOR
        public override void OnGUI(Rect position)
        {
            m_Curve = EditorGUI.CurveField(position, "Response Curve", m_Curve);
        }

        public override float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        #endif
    }
}
