using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class PointerEvent : InputEvent
    {
        public int pointerId { get; set; }
        public Vector3 position { get; set; }
        public float pressure { get; set; }
        public float twist { get; set; }
        public Vector2 tilt { get; set; }
        public Vector3 radius { get; set; }
        public int displayIndex { get; set; }

        public override void Reset()
        {
            base.Reset();
            pointerId = 0;
            position = default(Vector3);
            pressure = 0;
            tilt = default(Vector2);
            twist = 0;
            radius = default(Vector3);
            displayIndex = 0;
        }

        public override string ToString()
        {
            return string.Format("({0}, pos:{1})", base.ToString(), position);
        }
    }
}
