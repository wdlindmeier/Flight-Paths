using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    public class DeltaAxisToSpeedProcessor : InputBindingProcessor<AxisControl, float>
    {
        public override float ProcessValue(AxisControl control, float newValue)
        {
            return newValue / Time.smoothDeltaTime;
        }

        #if UNITY_EDITOR
        public override void OnGUI(Rect position)
        {
            EditorGUI.LabelField(position, "Convert Delta to Speed");
        }

        public override float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        #endif
    }
}
