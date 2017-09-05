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
    public class ButtonComboBinding : InputBinding<ButtonControl, float>, ISerializationCallbackReceiver, IEndBinding
    {
        [NonSerialized]
        public ControlReferenceBinding<ButtonControl, float> m_Main = new ControlReferenceBinding<ButtonControl, float>();
        public ControlReferenceBinding<ButtonControl, float> main { get { return m_Main; } set { m_Main = value; } }
        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializedMain;

        [NonSerialized]
        private List<ControlReferenceBinding<ButtonControl, float>> m_Modifiers = new List<ControlReferenceBinding<ButtonControl, float>>();
        public List<ControlReferenceBinding<ButtonControl, float>> modifiers { get { return m_Modifiers; } set { m_Modifiers = value; } }
        [SerializeField]
        List<SerializationHelper.JSONSerializedElement> m_SerializedModifiers = new List<SerializationHelper.JSONSerializedElement>();

        [NonSerialized]
        private List<ControlReferenceBinding<ButtonControl, float>> m_ModifierSet = new List<ControlReferenceBinding<ButtonControl, float>>();
        public List<ControlReferenceBinding<ButtonControl, float>> modifierSet { get { return m_ModifierSet; } set { m_ModifierSet = value; } }
        [SerializeField]
        List<SerializationHelper.JSONSerializedElement> m_SerializedModifierSet = new List<SerializationHelper.JSONSerializedElement>();

        public virtual void OnBeforeSerialize()
        {
            m_SerializedMain = SerializationHelper.SerializeObj(m_Main);
            m_SerializedModifiers = SerializationHelper.Serialize(m_Modifiers);
            m_SerializedModifierSet = SerializationHelper.Serialize(m_ModifierSet);
        }

        public virtual void OnAfterDeserialize()
        {
            m_Main = SerializationHelper.DeserializeObj<ControlReferenceBinding<ButtonControl, float>>(m_SerializedMain, new object[] {});
            m_Modifiers = SerializationHelper.Deserialize<ControlReferenceBinding<ButtonControl, float>>(m_SerializedModifiers, new object[] {});
            m_ModifierSet = SerializationHelper.Deserialize<ControlReferenceBinding<ButtonControl, float>>(m_SerializedModifierSet, new object[] {});
            m_SerializedMain = new SerializationHelper.JSONSerializedElement();
            m_SerializedModifiers = null;
            m_SerializedModifierSet = null;
        }

        public override void Initialize(IInputStateProvider stateProvider)
        {
            main.Initialize(stateProvider);
            for (int i = 0; i < m_Modifiers.Count; i++)
                modifiers[i].Initialize(stateProvider);
            for (int i = 0; i < m_ModifierSet.Count; i++)
                modifierSet[i].Initialize(stateProvider);
        }

        public override void EndUpdate()
        {
            bool allModifiersHeld = true;
            for (int i = 0; i < modifiers.Count; i++)
            {
                modifiers[i].EndUpdate();
                if (modifiers[i].value <= 0.5f)
                    allModifiersHeld = false;
                // Do not early out. EndUpdate needs to be called on all modifiers.
            }

            bool mainPressedPrev = (main.value > 0.5f);
            main.EndUpdate();

            // Button combo (like a keyboard shortcut) should only be triggered
            // if modifier keys are already held down when main key is pressed down.
            // If player presses main key first and then modifier keys, it should not trigger.
            if (value <= 0.5f)
            {
                if (main.value > 0.5f && !mainPressedPrev && allModifiersHeld)
                    value = 1;
            }
            // Once triggered, button combo keeps being active as long as the main key is held,
            // regardless of whether the modifier keys are released.
            else
            {
                if (main.value <= 0.5f)
                    value = 0;
            }
        }

        public override object Clone()
        {
            var clone = (ButtonComboBinding)Activator.CreateInstance(GetType());
            clone.main = main.Clone() as ControlReferenceBinding<ButtonControl, float>;
            clone.modifiers = modifiers.SemiDeepClone();
            clone.modifierSet = modifierSet.SemiDeepClone();
            return clone;
        }

        public override string ToString()
        {
            return string.Format("({0})", main);
        }

        public override string GetSourceName(ControlScheme controlScheme, bool forceStandardized)
        {
            if (main == null)
                return "None";

            string str = main.GetSourceName(controlScheme, forceStandardized);
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (modifiers[i] != null)
                    str = modifiers[i].GetSourceName(controlScheme, forceStandardized) + " + " + str;
            }
            return str;
        }

        public void ExtractDeviceTypesAndControlHashes(Dictionary<int, List<int>> controlIndicesPerDeviceType)
        {
            main.ExtractDeviceTypesAndControlHashes(controlIndicesPerDeviceType);
            for (int i = 0; i < m_Modifiers.Count; i++)
                modifiers[i].ExtractDeviceTypesAndControlHashes(controlIndicesPerDeviceType);
        }

        public override void ExtractBindingsOfType<L>(List<L> bindings)
        {
            if (this is L)
                bindings.Add((L)(object)this);
        }

        public override void ExtractLabeledEndBindings(string label, List<LabeledBinding> bindings)
        {
            bindings.Add(new LabeledBinding(label, this));
        }

        public bool TryBindControl(InputControl control, IInputStateProvider stateProvider)
        {
            var controlOfRightType = control as ButtonControl;
            if (controlOfRightType == null)
                return false;

            for (int i = 0; i < modifierSet.Count; i++)
            {
                if (control.provider == modifierSet[i].sourceControl.provider && control.index == modifierSet[i].sourceControl.index)
                    return false;
            }

            main.SetSource(controlOfRightType, stateProvider);

            modifiers.Clear();
            for (int i = 0; i < modifierSet.Count; i++)
            {
                // Check if this modifier is held down.
                // Since we don't spend resources updating the states of the possible modifier keys,
                // we look instead directly on the corresponding controls on the InputDevice.
                // the sourceControl is the control in the ActionMap's own state.
                // To get to the InputDevice we need to get its provider and look up the control there.
                if (((ButtonControl)(modifierSet[i].sourceControl.provider.GetControlFromHash(modifierSet[i].supportedControl.hash))).value > 0.5f)
                    modifiers.Add((ControlReferenceBinding<ButtonControl, float>)modifierSet[i].Clone());
            }

            return true;
        }

        #if UNITY_EDITOR
        public static GUIContent s_MainContent = new GUIContent("Main");
        public static GUIContent s_CountContent = new GUIContent("Count");
        public static GUIContent s_ModifierContent = new GUIContent("Modifier");

        public override void OnGUI(Rect position, IControlDomainSource domainSource)
        {
            position.height = ControlGUIUtility.GetControlHeight(m_Main, s_MainContent);
            ControlGUIUtility.ControlField(position, m_Main, s_MainContent, domainSource, b => m_Main = b as ControlReferenceBinding<ButtonControl, float>);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            int count = EditorGUI.IntField(position, s_CountContent, modifiers.Count);
            if (EditorGUI.EndChangeCheck())
            {
                if (count > modifiers.Count)
                {
                    while (modifiers.Count < count)
                        modifiers.Add(null);
                }
                else
                {
                    modifiers.RemoveRange(count, modifiers.Count - count);
                }
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                int index = i; // See "close over the loop variable".
                var control = modifiers[i];
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                position.height = ControlGUIUtility.GetControlHeight(control, s_ModifierContent);
                ControlGUIUtility.ControlField(position, control, s_ModifierContent, domainSource,
                    b => modifiers[index] = b as ControlReferenceBinding<ButtonControl, float>);
            }

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            count = EditorGUI.IntField(position, s_CountContent, modifierSet.Count);
            if (EditorGUI.EndChangeCheck())
            {
                if (count > modifierSet.Count)
                {
                    while (modifierSet.Count < count)
                        modifierSet.Add(null);
                }
                else
                {
                    modifierSet.RemoveRange(count, modifierSet.Count - count);
                }
            }

            for (int i = 0; i < modifierSet.Count; i++)
            {
                int index = i; // See "close over the loop variable".
                var control = modifierSet[i];
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                position.height = ControlGUIUtility.GetControlHeight(control, s_ModifierContent);
                ControlGUIUtility.ControlField(position, control, s_ModifierContent, domainSource,
                    b => modifierSet[index] = b as ControlReferenceBinding<ButtonControl, float>);
            }
        }

        public override float GetPropertyHeight()
        {
            float height =
                ControlGUIUtility.GetControlHeight(m_Main, s_MainContent) +
                EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;

            for (int i = 0; i < modifiers.Count; i++)
            {
                var control = modifiers[i];
                height +=
                    ControlGUIUtility.GetControlHeight(control, s_ModifierContent) +
                    EditorGUIUtility.standardVerticalSpacing;
            }

            for (int i = 0; i < modifierSet.Count; i++)
            {
                var control = modifierSet[i];
                height +=
                    ControlGUIUtility.GetControlHeight(control, s_ModifierContent) +
                    EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        #endif
    }
}
