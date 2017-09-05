using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Note corresponding ActionSlot in Players/ActionSlots folder.
    public class QuaternionControl : InputControl<Quaternion>
    {
        public QuaternionControl()
        {
            defaultValue = Quaternion.identity;
            Reset();
        }

        public QuaternionControl(string name) : this()
        {
            this.name = name;
        }

        public override Quaternion GetCombinedValue(Quaternion[] values)
        {
            // Can't really combine multiple quaternion sources.
            // We'll just return first one that is not identity.
            for (int i = 0; i < values.Length; i++)
            {
                var current = values[i];
                if (current != Quaternion.identity)
                    return current;
            }
            return defaultValue;
        }
    }
}
