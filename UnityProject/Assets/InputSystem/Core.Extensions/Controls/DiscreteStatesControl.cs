using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class DiscreteStatesControl : InputControl<int>
    {
        public DiscreteStatesControl() {}
        public DiscreteStatesControl(string name)
        {
            this.name = name;
        }
    }
}
