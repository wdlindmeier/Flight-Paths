using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // This control type has no corresponding ActionSlot since it's enough
    // for the user to access a DeltaAxisControl as an AxisControl.

    // DeltaAxisControl is for axes that repesent a change in value.
    // Cursor position deltas and scroll wheel deltas are typical examples.
    // The value accumulates during a frame, and is then automatically reset
    // at the beginning of each new frame.
    // Special logic ensures this is handled correctly simultaneously for
    // dynamic frames and fixed frames.
    public class DeltaAxisControl : AxisControl
    {
        public DeltaAxisControl() {}
        public DeltaAxisControl(string name) : base(name) {}

        public override void AdvanceFrame()
        {
            base.AdvanceFrame();
            value = defaultValue;
        }

        public override void SetValueFromEvent(float newValue)
        {
            // We have to access the protected values directly here,
            // since we want to increment both by the same amount,
            // not just set them to the same value.
            m_ValueDynamic += newValue;
            m_ValueFixed += newValue;
        }
    }
}
