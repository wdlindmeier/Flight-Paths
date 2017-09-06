using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class KeyEvent : InputEvent
    {
        public KeyCode key { get; set; }
        public bool isDown { get; set; }

        public override void Reset()
        {
            base.Reset();
            key = default(KeyCode);
            isDown = false;
        }

        public override string ToString()
        {
            return string.Format("({0}, key:{1}, isDown:{2})", base.ToString(), key, isDown);
        }
    }
}
