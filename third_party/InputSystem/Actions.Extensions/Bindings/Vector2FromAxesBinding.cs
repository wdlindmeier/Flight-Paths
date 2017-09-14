using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    [Serializable]
    public class Vector2FromAxesBinding : InputBinding<Vector2Control, Vector2>, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public InputBinding<AxisControl, float> m_X = new ControlReferenceBinding<AxisControl, float>();
        public InputBinding<AxisControl, float> x { get { return m_X; } set { m_X = value; } }
        [NonSerialized]
        public InputBinding<AxisControl, float> m_Y = new ControlReferenceBinding<AxisControl, float>();
        public InputBinding<AxisControl, float> y { get { return m_Y; } set { m_Y = value; } }

        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializedX;
        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializedY;

        [SerializeField]
        private string m_SourceNameFormat = "{0}, {1}";

        // Needed for instances created with Activator.
        public Vector2FromAxesBinding() {}

        // For convenience for instances created manually (hardcoded).
        public Vector2FromAxesBinding(InputBinding<AxisControl, float> x, InputBinding<AxisControl, float> y)
        {
            m_X = x;
            m_Y = y;
        }

        public virtual void OnBeforeSerialize()
        {
            m_SerializedX = SerializationHelper.SerializeObj(x);
            m_SerializedY = SerializationHelper.SerializeObj(y);
        }

        public virtual void OnAfterDeserialize()
        {
            x = SerializationHelper.DeserializeObj<InputBinding<AxisControl, float>>(m_SerializedX, new object[] {});
            y = SerializationHelper.DeserializeObj<InputBinding<AxisControl, float>>(m_SerializedY, new object[] {});
            m_SerializedX = new SerializationHelper.JSONSerializedElement();
            m_SerializedY = new SerializationHelper.JSONSerializedElement();
        }

        public override void Initialize(IInputStateProvider stateProvider)
        {
            x.Initialize(stateProvider);
            y.Initialize(stateProvider);
        }

        public override void EndUpdate()
        {
            x.EndUpdate();
            y.EndUpdate();
            value = new Vector2(x.value, y.value);
        }

        public override object Clone()
        {
            var clone = (Vector2FromAxesBinding)Activator.CreateInstance(GetType());
            clone.x = x.Clone() as InputBinding<AxisControl, float>;
            clone.y = y.Clone() as InputBinding<AxisControl, float>;
            return clone;
        }

        public override string GetSourceName(ControlScheme controlScheme, bool forceStandardized)
        {
            return string.Format(
                m_SourceNameFormat,
                x == null ? "None" : x.GetSourceName(controlScheme, forceStandardized),
                x == null ? "None" : y.GetSourceName(controlScheme, forceStandardized));
        }

        public override void ExtractBindingsOfType<L>(List<L> bindings)
        {
            x.ExtractBindingsOfType(bindings);
            y.ExtractBindingsOfType(bindings);
        }

        public override void ExtractLabeledEndBindings(string label, List<LabeledBinding> bindings)
        {
            x.ExtractLabeledEndBindings(label + " Horizontal", bindings);
            y.ExtractLabeledEndBindings(label + " Vertical", bindings);
        }

        #if UNITY_EDITOR
        public static GUIContent s_XContent = new GUIContent("X");
        public static GUIContent s_YContent = new GUIContent("Y");

        public override void OnGUI(Rect position, IControlDomainSource domainSource)
        {
            position.height = ControlGUIUtility.GetControlHeight(m_X, s_XContent);
            ControlGUIUtility.ControlField(position, m_X, s_XContent, domainSource, b => m_X = b);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = ControlGUIUtility.GetControlHeight(m_Y, s_YContent);
            ControlGUIUtility.ControlField(position, m_Y, s_YContent, domainSource, b => m_Y = b);
        }

        public override float GetPropertyHeight()
        {
            return
                ControlGUIUtility.GetControlHeight(m_X, s_XContent) +
                ControlGUIUtility.GetControlHeight(m_Y, s_YContent) +
                EditorGUIUtility.standardVerticalSpacing;
        }

        #endif
    }
}
