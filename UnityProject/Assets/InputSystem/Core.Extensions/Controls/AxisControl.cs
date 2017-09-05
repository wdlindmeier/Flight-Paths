using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Note corresponding ActionSlot in Players/ActionSlots folder.

    // AxisControl is for axes where the resting state is somewhere in the middle,
    // or where the range of possible values is not fixed.
    // (For controls with a fixed range where the resting state is at one end, use
    // ButtonControl instead. Triggers and analog buttons are typical examples of that.)
    // For axes with fixed ranges, device profiles should map the values to fall between
    // -1 and 1 (with the resting state being 0) unless there is a specific reason not to.
    public class AxisControl : InputControl<float>
    {
        public AxisControl() {}
        public AxisControl(string name)
        {
            this.name = name;
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
