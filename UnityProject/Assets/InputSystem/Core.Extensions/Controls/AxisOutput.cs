using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Note corresponding ActionSlot in Players/ActionSlots folder.
    public class AxisOutput : OutputControl<float>
    {
        public AxisOutput() {}
        public AxisOutput(string name)
        {
            this.name = name;
        }

        public override float value
        {
            set
            {
                if (value == base.value)
                    return;
                base.value = value;

                var handler = action;
                if (handler != null)
                    handler(base.value);
            }
        }

        public Action<float> action { get; set; }
    }
}
