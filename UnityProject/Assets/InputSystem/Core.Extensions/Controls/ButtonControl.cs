using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Note corresponding ActionSlot in Players/ActionSlots folder.

    // ButtonControl is for digital and analog buttons.
    // Digital buttons can be keyboard keys, classic joystick/gamepad buttons,
    // and d-pad directions such as d-pad left. They have values of either 0 or 1.
    // Analog buttons can be triggers and other pressure sentitive buttons, and
    // analog stick directions such as left stick left.
    // Device profiles should ensure that values for ButtonControls fall between 0 and 1.
    public class ButtonControl : InputControl<float>
    {
        public ButtonControl() {}
        public ButtonControl(string name)
        {
            this.name = name;
        }

        // NOTE: These properties check provider.active in the event that the AMI has processAll enabled because the
        // control will still get input events. These properties shouldn't reflect those values unless the AMI is active.
        public bool isHeld
        {
            get { return provider.active && value > 0.5f; }
        }

        public bool wasJustPressed
        {
            get { return provider.active && value > 0.5f && previousValue <= 0.5f; }
        }

        public bool wasJustReleased
        {
            get { return provider.active && value <= 0.5f && previousValue > 0.5f; }
        }

        public override float GetCombinedValue(float[] values)
        {
            float value = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var current = values[i];
                if (Mathf.Abs(current) > Mathf.Abs(value))
                    value = current;
            }
            return value;
        }
    }
}
