using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class TextEvent : InputEvent
    {
        public char text { get; set; }

        public override void Reset()
        {
            base.Reset();
            text = default(char);
        }
    }
}
