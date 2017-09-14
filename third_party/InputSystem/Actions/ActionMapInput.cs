using System;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
    /*
    Things to test for action map / control schemes.

    - When pressing e.g. mouse button or gamepad trigger in one action map creates a new action map
      based on the same device, the new action map should not immediately have wasJustPressed be true.
      Hence the state in the newly created control scheme should be initialized to the state
      of the devices it's based on.

    - When pressing e.g. mouse button or gamepad trigger and it causes a switch in control scheme
      within an existing action map, the new control scheme *should* immediately have wasJustPressed be true.
      Hence the state in the newly created control scheme should not be initialized to the state
      of the devices it's based on.

    */
    public class ActionMapInput : InputControlProvider, IInputStateProvider
    {
        private ActionMap m_ActionMap;
        public ActionMap actionMap { get { return m_ActionMap; } }

        private ControlScheme m_ControlScheme;
        public ControlScheme controlScheme { get { return m_ControlScheme; } }

        private List<ControlScheme> m_ControlSchemes;
        public List<ControlScheme> controlSchemes
        {
            get { return m_ControlSchemes; }
            set
            {
                int currentIndex = (m_ControlSchemes == null ? 0 : m_ControlSchemes.IndexOf(m_ControlScheme));
                m_ControlSchemes = value;
                m_ControlScheme = m_ControlSchemes[currentIndex];
            }
        }

        private List<InputState> m_DeviceStates = new List<InputState>();
        private List<InputState> deviceStates { get { return m_DeviceStates; } }

        private InputBinding[] m_SelfBindings;

        public void ResetControlSchemes()
        {
            // Note: Using property intentially here to have setter logic invoked.
            controlSchemes = m_ActionMap.controlSchemes.SemiDeepClone();
        }

        // Control whether this ActionMapInput will attempt to reinitialize with applicable devices
        // in order to process events.
        public bool autoReinitialize { get; set; }

        public bool blockSubsequent { get; set; }

        public static ActionMapInput Create(ActionMap actionMap)
        {
            ActionMapInput map =
                (ActionMapInput)Activator.CreateInstance(actionMap.mapType, new object[] { actionMap });
            return map;
        }

        protected ActionMapInput(ActionMap actionMap)
        {
            autoReinitialize = true;
            m_ActionMap = actionMap;
            ResetControlSchemes();

            // Create list of controls from ActionMap.
            var controls = new List<InputControl>();
            for (int i = 0; i < actionMap.actions.Count; i++)
            {
                var action = actionMap.actions[i];
                var control = (InputControl)Activator.CreateInstance(action.controlType);
                control.name = action.name;
                controls.Add(control);
            }
            SetControls(controls);

            m_SelfBindings = new InputBinding[actionMap.actions.Count];
            for (int i = 0; i < actionMap.actions.Count; i++)
            {
                if (actionMap.actions[i].combined)
                {
                    m_SelfBindings[i] = (InputBinding)(actionMap.actions[i].selfBinding.Clone());
                    m_SelfBindings[i].Initialize(this);
                }
            }
        }

        /// <summary>
        /// Find the best control scheme for the available devices and initialize the action map input.
        /// </summary>
        /// <param name="availableDevices">Available devices in the system</param>
        /// <param name="requiredDevice">Required device for scheme</param>
        /// <returns></returns>
        public bool TryInitializeWithDevices(List<InputDevice> availableDevices,
            List<InputDevice> requiredDevices = null,
            int requiredControlSchemeIndex = -1)
        {
            int bestScheme = -1;
            List<InputDevice> bestFoundDevices = null;
            bool success = actionMap.CanInitializeInstanceWithDevices(
                    availableDevices,
                    out bestScheme, out bestFoundDevices,
                    requiredDevices, requiredControlSchemeIndex);

            if (!success)
                return false;

            ControlScheme matchingControlScheme = controlSchemes[bestScheme];
            Assign(matchingControlScheme, bestFoundDevices);
            return true;
        }

        private void Assign(ControlScheme controlScheme, List<InputDevice> devices)
        {
            m_ControlScheme = controlScheme;

            // Create state for every device.
            var deviceStates = new List<InputState>();
            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                deviceStates.Add(new InputState(device, GetClonedControlsList(device)));
            }
            m_DeviceStates = deviceStates;
            m_ControlScheme.Initialize(this);
            RefreshBindings();

            if (onStatusChanged != null)
                onStatusChanged();
        }

        private List<InputControl> GetClonedControlsList(InputControlProvider provider)
        {
            List<InputControl> controls = new List<InputControl>();
            for (int i = 0; i < provider.controlCount; i++)
                if (provider.GetControl(i) == null)
                    controls.Add(null);
                else
                    controls.Add((InputControl)provider.GetControl(i).Clone());
            return controls;
        }

        public void SendControlResetEvents()
        {
            for (int i = 0; i < m_DeviceStates.Count; i++)
            {
                var state = m_DeviceStates[i];
                for (int j = 0; j < state.count; j++)
                {
                    if (blockSubsequent || (state.controls[j] != null && state.controls[j].enabled))
                    {
                        var evt = GetControlResetEventForControl(state.controls[j]);
                        if (evt == null)
                            continue;
                        evt.device = state.controlProvider as InputDevice;
                        InputSystem.consumers.ProcessEvent(evt);
                    }
                }
            }
        }

        private GenericControlEvent GetControlResetEventForControl(InputControl control)
        {
            Type genericType = control.GetType();
            while (genericType.BaseType != typeof(InputControl))
                genericType = genericType.BaseType;
            if (genericType.GetGenericTypeDefinition() != typeof(InputControl<>))
                return null;
            Type genericArgumentType = genericType.GetGenericArguments()[0];
            Type eventType = typeof(GenericControlEvent<>).MakeGenericType(new System.Type[] { genericArgumentType });
            GenericControlEvent evt = (GenericControlEvent)Activator.CreateInstance(eventType);
            evt.controlIndex = control.index;
            evt.CopyDefaultValueFromControl(control);
            evt.time = Time.time;
            return evt;
        }

        public bool CurrentlyUsesDevice(InputDevice device)
        {
            for (int i = 0; i < deviceStates.Count; i++)
            {
                var deviceState = deviceStates[i];
                if (deviceState.controlProvider == device)
                    return true;
            }
            return false;
        }

        public double GetLastDeviceInputTime()
        {
            double time = 0;
            for (int i = 0; i < deviceStates.Count; i++)
            {
                var deviceState = deviceStates[i];
                time = Math.Max(time, deviceState.controlProvider.lastEventTime);
            }
            return time;
        }

        public override bool ProcessEvent(InputEvent inputEvent)
        {
            var consumed = false;

            // Update device state (if event actually goes to one of the devices we talk to).
            for (int i = 0; i < deviceStates.Count; i++)
            {
                var deviceState = deviceStates[i];
                ////FIXME: should refer to proper type
                var device = (InputDevice)deviceState.controlProvider;

                // Skip state if event is not meant for device associated with it.
                if (device != inputEvent.device)
                    continue;

                // Give device a stab at converting the event into state.
                if (device.ProcessEventIntoState(inputEvent, deviceState))
                {
                    consumed = true;
                    break;
                }
            }

            if (!consumed)
                return false;

            return true;
        }

        public void Reset(bool initToDeviceState = true)
        {
            if (initToDeviceState)
            {
                for (int i = 0; i < deviceStates.Count; i++)
                {
                    var deviceState = deviceStates[i];
                    deviceState.InitToDevice();
                }

                ExtractCurrentValuesFromSources();

                // Copy current values into prev values.
                state.BeginUpdate();
            }
            else
            {
                for (int i = 0; i < deviceStates.Count; i++)
                {
                    var deviceState = deviceStates[i];
                    deviceState.Reset();
                }
                state.Reset();
            }
        }

        public List<InputDevice> GetCurrentlyUsedDevices()
        {
            List<InputDevice> list = new List<InputDevice>();
            for (int i = 0; i < deviceStates.Count; i++)
                list.Add(deviceStates[i].controlProvider as InputDevice);
            return list;
        }

        public InputState GetDeviceStateForDeviceSlotKey(int deviceKey)
        {
            // If deviceKey is 0, return own state instead.
            // This is used by combined bindings which create actions from other actions.
            if (deviceKey == 0)
                return state;

            // Otherwise find relevant device state.
            int deviceSlotIndex = -1;
            for (int i = 0; i < controlScheme.deviceSlots.Count; i++)
                if (controlScheme.deviceSlots[i].key == deviceKey)
                    deviceSlotIndex = i;
            if (deviceSlotIndex == -1)
                return null;
            return deviceStates[deviceSlotIndex];
        }

        public int GetOrAddDeviceSlotKey(InputDevice device)
        {
            var deviceSlots = controlScheme.deviceSlots;
            for (int i = 0; i < deviceSlots.Count; i++)
            {
                var slot = deviceSlots[i];
                if (slot.IsDeviceCompatible(device))
                    return slot.key;
            }

            // TODO: Add new device state if not already present.
            return DeviceSlot.kInvalidKey;
        }

        public void BeginUpdate()
        {
            state.BeginUpdate();
            for (int i = 0; i < deviceStates.Count; i++)
            {
                var deviceState = deviceStates[i];
                deviceState.BeginUpdate();
            }
        }

        public void EndUpdate()
        {
            for (int i = 0; i < deviceStates.Count; i++)
            {
                var deviceState = deviceStates[i];
                (deviceState.controlProvider as InputDevice).PostProcessState(deviceState);
            }
            ExtractCurrentValuesFromSources();
        }

        private void ExtractCurrentValuesFromSources()
        {
            // Fill state that is bound to controls.
            for (var i = 0; i < actionMap.actions.Count; i++)
            {
                InputAction action = actionMap.actions[i];
                if (action.combined)
                    continue;
                var binding = controlScheme.bindings[i];
                if (binding != null)
                {
                    binding.EndUpdate();
                    (binding as IRootBinding).ProcessValueAndApply(state.controls[i]);
                }
            }
            // Fill state that is combined from other state.
            // This must be done in a separate pass afterwards,
            // since it relies on the other state having already been filled out.
            for (var i = 0; i < actionMap.actions.Count; i++)
            {
                InputAction action = actionMap.actions[i];
                if (!action.combined)
                    continue;
                var binding = m_SelfBindings[i];
                if (binding != null)
                {
                    binding.EndUpdate();
                    (binding as IRootBinding).ProcessValueAndApply(state.controls[i]);
                }
            }
        }

        public override string GetSourceName(int controlIndex)
        {
            return controlScheme.bindings[controlIndex].GetSourceName(null, false);
        }

        ////REVIEW: the binding may come from anywhere; method assumes we get passed some state we actually own
        public bool BindControl(IEndBinding binding, InputControl control, bool restrictToExistingDevices)
        {
            if (restrictToExistingDevices)
            {
                bool existingDevice = false;
                for (int i = 0; i < m_DeviceStates.Count; i++)
                {
                    if (control.provider == m_DeviceStates[i].controlProvider)
                    {
                        existingDevice = true;
                        break;
                    }
                }
                if (!existingDevice)
                    return false;
            }

            if (!binding.TryBindControl(control, this))
                return false;
            m_ControlScheme.customized = true;
            RefreshBindings();
            return true;
        }

        private void RefreshBindings()
        {
            // Gather a mapping of device types to list of bindings that use the given type.
            var perDeviceTypeUsedControlIndices = new Dictionary<int, List<int>>();
            controlScheme.ExtractDeviceTypesAndControlHashes(perDeviceTypeUsedControlIndices);

            for (int slotIndex = 0; slotIndex < controlScheme.deviceSlots.Count; slotIndex++)
            {
                DeviceSlot slot = controlScheme.deviceSlots[slotIndex];
                InputState state = deviceStates[slotIndex];
                List<int> hashes;
                if (perDeviceTypeUsedControlIndices.TryGetValue(slot.key, out hashes))
                {
                    var indices = new List<int>(hashes.Count);
                    for (int i = 0; i < hashes.Count; i++)
                        indices.Add(state.controlProvider.GetControlIndexFromHash(hashes[i]));
                    state.SetUsedControls(indices);
                    (state.controlProvider as InputDevice).PostProcessEnabledControls(state);
                }
                else
                {
                    state.SetAllControlsEnabled(false);
                }
            }
        }

        public override int GetControlIndexFromHash(int hash)
        {
            return hash;
        }

        public override int GetHashForControlIndex(int controlIndex)
        {
            return controlIndex;
        }
    }
}
