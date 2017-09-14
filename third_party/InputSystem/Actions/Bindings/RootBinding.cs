using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    public interface IRootBinding
    {
        void ProcessValueAndApply(InputControl control);
    }

    [Serializable]
    internal sealed class RootBinding<C, T> : InputBinding<C, T>, IRootBinding, ISerializationCallbackReceiver where C : InputControl<T>
    {
        [NonSerialized]
        private List<InputBinding<C, T>> m_Sources = new List<InputBinding<C, T>>();
        public List<InputBinding<C, T>> sources { get { return m_Sources; } set { m_Sources = value; } }
        [SerializeField]
        List<SerializationHelper.JSONSerializedElement> m_SerializableSources = new List<SerializationHelper.JSONSerializedElement>();

        [NonSerialized]
        private List<InputBindingProcessor<C, T>> m_Processors = new List<InputBindingProcessor<C, T>>();
        public List<InputBindingProcessor<C, T>> processors { get { return m_Processors; } set { m_Processors = value; } }
        [SerializeField]
        List<SerializationHelper.JSONSerializedElement> m_SerializableProcessors = new List<SerializationHelper.JSONSerializedElement>();

        [NonSerialized]
        private T[] m_Values;

        // The reference control is needed since it knows how to combine multiple values into one.
        [NonSerialized]
        private InputControl<T> m_ReferenceControl;
        [SerializeField]
        private SerializationHelper.JSONSerializedElement m_SerializableReferenceControl;
        // Invoked by reflection in ActionMap.
        internal void SetReferenceControl(Type type)
        {
            m_ReferenceControl = (InputControl<T>)Activator.CreateInstance(type);
        }

        public RootBinding()
        {
            m_Sources.Add(new ControlReferenceBinding<C, T>());
        }

        public void OnBeforeSerialize()
        {
            m_SerializableSources = SerializationHelper.Serialize(m_Sources);
            m_SerializableProcessors = SerializationHelper.Serialize(m_Processors);

            m_SerializableReferenceControl = SerializationHelper.SerializeObj(m_ReferenceControl);
        }

        public void OnAfterDeserialize()
        {
            m_Sources = SerializationHelper.Deserialize<InputBinding<C, T>>(m_SerializableSources, new object[] {});
            m_SerializableSources = null;
            m_Processors = SerializationHelper.Deserialize<InputBindingProcessor<C, T>>(m_SerializableProcessors, new object[] {});
            m_SerializableProcessors = null;

            m_ReferenceControl = SerializationHelper.DeserializeObj<InputControl<T>>(m_SerializableReferenceControl, new object[] {});
            m_SerializableReferenceControl = new SerializationHelper.JSONSerializedElement();
        }

        public override void EndUpdate()
        {
            // REVIEW: We shouldn't need to check and initialize m_Values here since it should always
            // have been assigned in the Initialized call prior to EndUpdate being called.
            // But errors in Standalone player indicates m_Values can sometimes be null in EndUpdate.
            if (m_Values == null)
            {
                m_Values = new T[m_Sources.Count];
                Debug.LogError("m_Values array was not initialized - was Initialize method on binding not called? Source length is " + m_Sources.Count);
            }
            for (int i = 0; i < m_Sources.Count; i++)
            {
                m_Sources[i].EndUpdate();
                m_Values[i] = m_Sources[i].value;
            }
            // InputControl<T> knows how to combine multiple values into one for the specific type of T.
            value = m_ReferenceControl.GetCombinedValue(m_Values);
        }

        public void ProcessValueAndApply(InputControl inputControl)
        {
            var control = (C)inputControl;

            // Process value and apply.
            for (int i = 0; i < m_Processors.Count; i++)
            {
                value = m_Processors[i].ProcessValue(control, value);
            }
            control.value = value;

            // On ActionMapInput controls (which represent actions), .rawValue returns the same value as the
            // final value. This is because we have no way of supporting a rawValue that's consistent between
            // actions that reference controls on devices, and actions that are composed of other actions,
            // without introducing significant extra processing.
            control.rawValue = value;
        }

        public override object Clone()
        {
            var clone = new RootBinding<C, T>();
            clone.m_Sources = m_Sources.SemiDeepClone();
            clone.m_Processors = m_Processors.SemiDeepClone();
            clone.m_ReferenceControl = m_ReferenceControl;
            return clone;
        }

        public override void Initialize(IInputStateProvider stateProvider)
        {
            m_Values = new T[sources.Count];
            for (int i = 0; i < sources.Count; i++)
            {
                sources[i].Initialize(stateProvider);
            }
        }

        public override void ExtractBindingsOfType<L>(List<L> bindings)
        {
            for (int i = 0; i < sources.Count; i++)
            {
                sources[i].ExtractBindingsOfType(bindings);
            }
        }

        public override void ExtractLabeledEndBindings(string label, List<LabeledBinding> bindings)
        {
            if (sources.Count > 0)
                sources[0].ExtractLabeledEndBindings(label, bindings);
            if (sources.Count > 1)
                sources[1].ExtractLabeledEndBindings(label + " Alt", bindings);
            for (int i = 2; i < sources.Count; i++)
            {
                sources[i].ExtractLabeledEndBindings(label + " Alt " + i, bindings);
            }
        }

        public void Reset() {}

        public override string GetSourceName(ControlScheme controlScheme, bool forceStandardized)
        {
            if (sources == null || sources.Count == 0)
                return "None";

            string str = sources[0].GetSourceName(controlScheme, forceStandardized);
            for (int i = 1; i < sources.Count; i++)
                str = string.Format("{0} / {1}", str, sources[i].GetSourceName(controlScheme, forceStandardized));

            return str;
        }

        #if UNITY_EDITOR
        object m_Selected = null;

        static class Styles
        {
            public static GUIContent iconToolbarPlus =  EditorGUIUtility.IconContent("Toolbar Plus", "Add to list");
            public static GUIContent iconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus", "Remove from list");
            public static GUIContent bindingContent = new GUIContent("Binding");
            public const int k_Spacing = 10;
        }

        public override void OnGUI(Rect position, IControlDomainSource domainSource)
        {
            // Bindings
            for (int i = 0; i < sources.Count; i++)
            {
                InputBinding<C, T> source = sources[i];
                position.height = ControlGUIUtility.GetControlHeight(source, Styles.bindingContent);
                DrawSourceSettings(position, i, domainSource);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }

            // Bindings remove and add buttons
            position.height = EditorGUIUtility.singleLineHeight;
            Rect buttonPosition = position;
            buttonPosition.width = Styles.iconToolbarMinus.image.width;
            if (GUI.Button(buttonPosition, Styles.iconToolbarMinus, GUIStyle.none))
            {
                var selected = m_Selected as InputBinding<C, T>;
                if (sources.Contains(selected))
                    sources.Remove(selected);
            }
            buttonPosition.x += buttonPosition.width;
            if (GUI.Button(buttonPosition, Styles.iconToolbarPlus, GUIStyle.none))
            {
                var source = new ControlReferenceBinding<C, T>();
                sources.Add(source);
                m_Selected = source;
            }
            position.y += position.height + Styles.k_Spacing;

            position.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(position, "Processors");
            position.y += position.height + Styles.k_Spacing;

            // Processors
            for (int i = 0; i < processors.Count; i++)
            {
                InputBindingProcessor<C, T> processor = processors[i];
                position.height = processor.GetPropertyHeight();
                DrawProcessorSettings(position, i);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }

            // Processors remove and add buttons
            position.height = EditorGUIUtility.singleLineHeight;
            buttonPosition = position;
            buttonPosition.width = Styles.iconToolbarMinus.image.width;
            if (GUI.Button(buttonPosition, Styles.iconToolbarMinus, GUIStyle.none))
            {
                var selected = m_Selected as InputBindingProcessor<C, T>;
                if (selected != null && processors.Contains(selected))
                    processors.Remove(selected);
            }
            buttonPosition.x += buttonPosition.width;
            if (GUI.Button(buttonPosition, Styles.iconToolbarPlus, GUIStyle.none))
            {
                InputBindingProcessor<C, T>.ShowAddProcessorDropdown(buttonPosition, p => processors.Add(p));
            }
        }

        void DrawSourceSettings(Rect position, int bindingIndex, IControlDomainSource domainSource)
        {
            InputBinding<C, T> source = sources[bindingIndex];

            bool used = false;
            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            {
                m_Selected = source;
                used = true;
            }
            if (m_Selected == source)
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);

            ControlGUIUtility.ControlField(position, source, Styles.bindingContent, domainSource,
                b => sources[bindingIndex] = b);

            if (used)
                Event.current.Use();
        }

        void DrawProcessorSettings(Rect position, int processorIndex)
        {
            InputBindingProcessor<C, T> processor = processors[processorIndex];

            bool used = false;
            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            {
                m_Selected = processor;
                used = true;
            }
            if (m_Selected == processor)
                GUI.DrawTexture(position, EditorGUIUtility.whiteTexture);

            processor.OnGUI(position);

            if (used)
                Event.current.Use();
        }

        public override float GetPropertyHeight()
        {
            float height = EditorGUIUtility.singleLineHeight * 3 + Styles.k_Spacing * 2;

            for (int i = 0; i < sources.Count; i++)
                height += ControlGUIUtility.GetControlHeight(sources[i], Styles.bindingContent);
            height += EditorGUIUtility.standardVerticalSpacing * sources.Count;

            for (int i = 0; i < processors.Count; i++)
                height += processors[i].GetPropertyHeight();
            height += EditorGUIUtility.standardVerticalSpacing * processors.Count;

            return height;
        }

        #endif
    }
}
