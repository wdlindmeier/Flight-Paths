using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Note corresponding ActionSlot in Players/ActionSlots folder.
    public class Vector2Control : InputControl<Vector2>
    {
        public Vector2Control() {}
        public Vector2Control(string name)
        {
            this.name = name;
        }

        public override Vector2 GetCombinedValue(Vector2[] values)
        {
            Vector2 value = Vector2.zero;
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
