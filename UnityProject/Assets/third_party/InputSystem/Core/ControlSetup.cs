using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class ControlSetup
    {
        public List<InputControl> controls = new List<InputControl>();
        public Dictionary<int, int> supportedControlIndices = new Dictionary<int, int>();
        public Dictionary<int, IControlMapping> mappings = new Dictionary<int, IControlMapping>();

        Type m_DeviceType;
        bool m_DefaultAdditionsAsStandardized;

        public ControlSetup(InputDevice device)
        {
            m_DeviceType = device.GetType();
            m_DefaultAdditionsAsStandardized = true;
            device.AddStandardControls(this);
            m_DefaultAdditionsAsStandardized = false;
        }

        public InputControl AddControl(SupportedControl supportedControl)
        {
            return AddControl(supportedControl, null, m_DefaultAdditionsAsStandardized);
        }

        public InputControl AddControl(SupportedControl supportedControl, InputControl control)
        {
            if (!control.GetType().IsAssignableFrom(supportedControl.controlType.value))
                throw new Exception("Control type does not match type of SupportedControl.");
            return AddControl(supportedControl, control, m_DefaultAdditionsAsStandardized);
        }

        InputControl AddControl(SupportedControl supportedControl, InputControl control, bool standardized)
        {
            int index;
            if (supportedControlIndices.TryGetValue(supportedControl.hash, out index))
                return controls[index];

            if (control == null)
                control = Activator.CreateInstance(supportedControl.controlType.value) as InputControl;
            if (control.name == null)
                control.name = supportedControl.standardName;

            index = controls.Count;
            supportedControlIndices[supportedControl.hash] = index;
            control.index = index;
            controls.Add(control);

            InputSystem.RegisterControl(supportedControl, m_DeviceType, standardized);

            return control;
        }

        public InputControl GetControl(SupportedControl supportedControl)
        {
            return controls[supportedControlIndices[supportedControl.hash]];
        }

        public Dictionary<int, IControlMapping> FinishMappings()
        {
            ////REVIEW: probably should kill our internal state
            return mappings;
        }
    }
}
