using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Note corresponding ActionSlot in Players/ActionSlots folder.
    public class Vector3Control : InputControl<Vector3>
    {
        public Vector3Control() {}
        public Vector3Control(string name)
        {
            this.name = name;
        }

        public override Vector3 GetCombinedValue(Vector3[] values)
        {
            Vector3 value = Vector3.zero;
            for (int i = 0; i < values.Length; i++)
            {
                var current = values[i];
                if (current.sqrMagnitude > value.sqrMagnitude)
                    value = current;
            }
            return value;
        }
    }
}
