using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class ClickEvent : GenericControlEvent<bool>
    {
        public int clickCount { get; set; }
        public bool isDown { get; set; }

        public override void Reset()
        {
            base.Reset();
            clickCount = 0;
            isDown = false;
        }
    }
}
