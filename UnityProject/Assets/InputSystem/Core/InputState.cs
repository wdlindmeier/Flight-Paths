using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // InputState is used for the following things:
    //  - The state in an InputDevice.
    //  - The device state in an ActionMapInput.
    //  - The action state in an ActionMapInput.
    //
    // Note that unlike InputControlProvider, input states wouldn't be able to implement e.g. GetSourceName,
    // nor would it independently know how to process events to populate the state in its controls.
    public class InputState
    {
        private List<InputControl> m_Controls = new List<InputControl>();
        public  List<InputControl> controls { get { return m_Controls; } }

        // ControlProvider is not necessarily the same object that has this state.
        //  - For a state in an InputDevice, the controlProvider is the InputDevice itself.
        //  - For a device state in an ActionMapInput (of which there can be multiple in the same ActionMapInput),
        //    the controlProvider is the corresponding device.
        //  - For an action state in an ActionMapInput, the controlProvider is the ActionMapInput itself.
        //
        // Note that as a consequence, InputState.controlProvider.controls can refer to a different list
        // than InputState.controls.
        public InputControlProvider controlProvider { get; set; }

        public InputState(InputControlProvider controlProvider, List<InputControl> controls)
            : this(controlProvider, controls, null) {}

        public InputState(InputControlProvider controlProvider, List<InputControl> controls, List<int> usedControlIndices)
        {
            this.controlProvider = controlProvider;
            m_Controls = controls;
            for (var i = 0; i < m_Controls.Count; i++)
            {
                if (m_Controls[i] != null)
                {
                    m_Controls[i].index = i;
                    m_Controls[i].provider = controlProvider;
                }
            }
            if (usedControlIndices == null)
                SetAllControlsEnabled(true);
            else
                SetUsedControls(usedControlIndices);
        }

        public void SetUsedControls(List<int> usedControlIndices)
        {
            if (usedControlIndices == null)
                throw new ArgumentNullException("usedControlIndices");

            SetAllControlsEnabled(false);
            for (var i = 0; i < usedControlIndices.Count; i++)
            {
                int index = usedControlIndices[i];
                if (index < 0 || index >= m_Controls.Count)
                    throw new ArgumentOutOfRangeException("usedControlIndices");
                if (m_Controls[index] != null)
                    m_Controls[index].enabled = true;
            }
        }

        public bool SetValueFromEvent<T>(int index, T value)
        {
            if (index < 0 || index >= m_Controls.Count)
                throw new ArgumentOutOfRangeException("index",
                    string.Format("Control index {0} is out of range; state has {1} entries", index, m_Controls.Count));

            if (!controls[index].enabled)
                return false;

            var control = m_Controls[index] as InputControl<T>;
            if (control == null)
                throw new Exception(string.Format(
                        "Control index {0} is of type {1} but was attempted to be set with value type {2}.",
                        index, m_Controls[index].GetType().Name, value.GetType().Name
                        ));

            control.SetValueFromEvent(value);
            return true;
        }

        public void SetAllControlsEnabled(bool enabled)
        {
            for (var i = 0; i < m_Controls.Count; ++i)
            {
                if (m_Controls[i] != null)
                    m_Controls[i].enabled = enabled;
            }
        }

        public void InitToDevice()
        {
            if (controlProvider.state == this)
                return;

            List<InputControl> referenceControls = controlProvider.state.controls;
            for (int i = 0; i < m_Controls.Count; i++)
            {
                if (m_Controls[i] == null)
                    continue;
                if (m_Controls[i].enabled)
                    m_Controls[i].CopyValueFromControl(referenceControls[i]);
                else
                    m_Controls[i].Reset();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < m_Controls.Count; i++)
                if (m_Controls[i] != null)
                    m_Controls[i].Reset();
        }

        internal void BeginUpdate()
        {
            var stateCount = m_Controls.Count;
            for (var index = 0; index < stateCount; ++index)
            {
                if (m_Controls[index] == null || !m_Controls[index].enabled)
                    continue;

                if (InputSystem.listeningForBinding)
                {
                    if (m_Controls[index].changedValue && !m_Controls[index].isDefaultValue)
                        InputSystem.RegisterBinding(controlProvider.GetControl(index));
                }

                m_Controls[index].AdvanceFrame();
            }
        }

        public int count
        {
            get { return m_Controls.Count; }
        }
    }
}
