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
    public class AxisFromButtonsBinding : InputBinding<AxisControl, float>, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public InputBinding<ButtonControl, float> m_Negative = new ControlReferenceBinding<ButtonControl, float>();
        public InputBinding<ButtonControl, float> negative { get { return m_Negative; } set { m_Negative = value; } }
        [NonSerialized]
        public InputBinding<ButtonControl, float> m_Positive = new ControlReferenceBinding<ButtonControl, float>();
        public InputBinding<ButtonControl, float> positive { get { return m_Positive; } set { m_Positive = value; } }

        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializedNegative;
        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializedPositive;

        [SerializeField]
        private string m_SourceNameFormat = "{0} & {1}";

        // Needed for instances created with Activator.
        public AxisFromButtonsBinding() {}

        // For convenience for instances created manually (hardcoded).
        public AxisFromButtonsBinding(InputBinding<ButtonControl, float> negative, InputBinding<ButtonControl, float> positive)
        {
            this.negative = negative;
            this.positive = positive;
        }

        public virtual void OnBeforeSerialize()
        {
            m_SerializedNegative = SerializationHelper.SerializeObj(negative);
            m_SerializedPositive = SerializationHelper.SerializeObj(positive);
        }

        public virtual void OnAfterDeserialize()
        {
            negative = SerializationHelper.DeserializeObj<InputBinding<ButtonControl, float>>(m_SerializedNegative, new object[] {});
            positive = SerializationHelper.DeserializeObj<InputBinding<ButtonControl, float>>(m_SerializedPositive, new object[] {});
            m_SerializedNegative = new SerializationHelper.JSONSerializedElement();
            m_SerializedPositive = new SerializationHelper.JSONSerializedElement();
        }

        public override void Initialize(IInputStateProvider stateProvider)
        {
            negative.Initialize(stateProvider);
            positive.Initialize(stateProvider);
        }

        public override void EndUpdate()
        {
            positive.EndUpdate();
            negative.EndUpdate();
            value = positive.value - negative.value;
        }

        public override object Clone()
        {
            var clone = (AxisFromButtonsBinding)Activator.CreateInstance(GetType());
            clone.negative = negative.Clone() as InputBinding<ButtonControl, float>;
            clone.positive = positive.Clone() as InputBinding<ButtonControl, float>;
            return clone;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", negative, positive);
        }

        public override string GetSourceName(ControlScheme controlScheme, bool forceStandardized)
        {
            return string.Format(
                m_SourceNameFormat,
                negative == null ? "None" : negative.GetSourceName(controlScheme, forceStandardized),
                positive == null ? "None" : positive.GetSourceName(controlScheme, forceStandardized));
        }

        public override void ExtractBindingsOfType<L>(List<L> bindings)
        {
            negative.ExtractBindingsOfType(bindings);
            positive.ExtractBindingsOfType(bindings);
        }

        public override void ExtractLabeledEndBindings(string label, List<LabeledBinding> bindings)
        {
            negative.ExtractLabeledEndBindings(label + " Negative", bindings);
            positive.ExtractLabeledEndBindings(label + " Positive", bindings);
        }

        #if UNITY_EDITOR
        public static GUIContent s_NegativeContent = new GUIContent("Negative");
        public static GUIContent s_PositiveContent = new GUIContent("Positive");

        public override void OnGUI(Rect position, IControlDomainSource domainSource)
        {
            position.height = ControlGUIUtility.GetControlHeight(m_Negative, s_NegativeContent);
            ControlGUIUtility.ControlField(position, m_Negative, s_NegativeContent, domainSource,
                b => m_Negative = b);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = ControlGUIUtility.GetControlHeight(m_Positive, s_PositiveContent);
            ControlGUIUtility.ControlField(position, m_Positive, s_PositiveContent, domainSource,
                b => m_Positive = b);
        }

        public override float GetPropertyHeight()
        {
            return
                ControlGUIUtility.GetControlHeight(m_Negative, s_NegativeContent) +
                ControlGUIUtility.GetControlHeight(m_Positive, s_PositiveContent) +
                EditorGUIUtility.standardVerticalSpacing;
        }

        #endif
    }
}
