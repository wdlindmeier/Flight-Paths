using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    public interface IControlReference
    {
        int controlHash { get; set; }
    }

    public sealed class ControlReferenceBinding<C, T> : InputBinding<C, T>, IEndBinding, IControlReference
        where C : InputControl<T>
    {
        [SerializeField]
        private int m_ControlHash = -1;
        public int controlHash
        {
            get { return m_ControlHash; }
            set { m_ControlHash = value; }
        }
        public SupportedControl supportedControl
        {
            get { return InputSystem.GetSupportedControl(m_ControlHash); }
            set { m_ControlHash = value.hash; }
        }

        [SerializeField]
        private int m_DeviceKey;
        public int deviceKey { get { return m_DeviceKey; } set { m_DeviceKey = value; } }

        // The source control from the ActionMapInput's state.
        private InputControl<T> m_SourceControl;
        public InputControl<T> sourceControl { get { return m_SourceControl; } }

        public override void EndUpdate()
        {
            if (sourceControl == null)
                value = default(T);
            else
                value = sourceControl.value;
        }

        public override string GetSourceName(ControlScheme controlScheme, bool forceStandardized)
        {
            if (sourceControl != null && !forceStandardized)
                return sourceControl.sourceName;

            SupportedControl supported = supportedControl;

            if (supported.Equals(SupportedControl.None))
                return "Unassigned";

            return supported.standardName;
        }

        // Needed for instances created with Activator.
        public ControlReferenceBinding() {}

        // For convenience for instances created manually (hardcoded).
        public ControlReferenceBinding(SupportedControl supportedControl)
        {
            m_ControlHash = supportedControl.hash;
            // deviceType set to null since it's not relevant for bindings on InputDevices,
            // which is the common use case for this constructor.
            deviceKey = DeviceSlot.kInvalidKey;
        }

        public override void Initialize(IInputStateProvider stateProvider)
        {
            if (controlHash == -1)
                return;

            var deviceState = stateProvider.GetDeviceStateForDeviceSlotKey(deviceKey);
            int controlIndex = deviceState.controlProvider.GetControlIndexFromHash(m_ControlHash);
            m_SourceControl = deviceState.controls[controlIndex] as InputControl<T>;
        }

        public void Reset() {}

        public override object Clone()
        {
            var clone = new ControlReferenceBinding<C, T>();
            clone.m_SourceControl = m_SourceControl;
            clone.m_ControlHash = m_ControlHash;
            clone.m_DeviceKey = m_DeviceKey;
            return clone;
        }

        public void ExtractDeviceTypesAndControlHashes(Dictionary<int, List<int>> controlIndicesPerDeviceType)
        {
            if (deviceKey == DeviceSlot.kInvalidKey || controlHash == -1)
                return;
            List<int> entries;
            if (!controlIndicesPerDeviceType.TryGetValue(deviceKey, out entries))
            {
                entries = new List<int>();
                controlIndicesPerDeviceType[deviceKey] = entries;
            }

            entries.Add(controlHash);
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
            var controlOfRightType = control as C;
            if (controlOfRightType == null)
                return false;

            SetSource(controlOfRightType, stateProvider);
            return true;
        }

        public void SetSource(InputControl<T> source, IInputStateProvider stateProvider)
        {
            deviceKey = stateProvider.GetOrAddDeviceSlotKey(source.provider as InputDevice);
            m_ControlHash = source.provider.GetHashForControlIndex(source.index);
            Initialize(stateProvider);
        }

        #if UNITY_EDITOR
        public override void OnGUI(Rect position, IControlDomainSource domainSource)
        {
            EditorGUI.HelpBox(
                position,
                "Program error. OnGUI of ControlReferenceBinding should not be called.",
                MessageType.Error);
        }

        public override float GetPropertyHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        #endif
    }
}
