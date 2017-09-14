using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class EventControl : InputControl<bool>
    {
        public override void AdvanceFrame()
        {
            base.AdvanceFrame();

            // Note: This relies on the fact that the property will only set the dynamic or fixed value in the right timestep (and not both like SetValue)
            value = false;
        }

        public override bool GetCombinedValue(bool[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
