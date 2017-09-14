using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utilities
{
    [Serializable]
    public struct Range
    {
        public static readonly Range none = new Range(0, 0);
        public static readonly Range full = new Range(-1, 1);
        public static readonly Range negative = new Range(0, -1);
        public static readonly Range positive = new Range(0, 1);
        public static readonly Range fullInverse = new Range(1, -1);

        public float min;
        public float max;

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
