using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class GenericHID : InputDevice
    {
        HIDDescriptor m_DeviceDescriptor = null;
        Dictionary<int, IControlMapping> m_Mappings;

        public GenericHID()
        {}

        public GenericHID(HIDDescriptor deviceDescriptor)
        {
            m_DeviceDescriptor = deviceDescriptor;
            ControlSetup defaultSetup = new ControlSetup(this);
            m_Mappings = defaultSetup.mappings;
        }

        public override bool RemapEvent(InputEvent inputEvent)
        {
            GenericControlEvent controlEvent = inputEvent as GenericControlEvent;
            if (controlEvent != null)
            {
                IControlMapping controlMapping = null;
                if (m_Mappings.TryGetValue(controlEvent.controlIndex, out controlMapping))
                {
                    return controlMapping.Remap(controlEvent);
                }
            }

            return base.RemapEvent(inputEvent);
        }

        public override void AddStandardControls(ControlSetup setup)
        {
            // If we have no device descriptor we just map to all available controls, since we can't be sure what we are getting.
            if (m_DeviceDescriptor == null)
            {
                HIDHelpers.AddDefaultControls(setup);
            }
            else
            {
                for (int i = 0; i < m_DeviceDescriptor.elements.Length; i++)
                {
                    HIDHelpers.AddHIDControl(setup, m_DeviceDescriptor.elements[i]);
                }
            }
        }
    }
}
