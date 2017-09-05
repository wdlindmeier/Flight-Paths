using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utilities
{
    public static class EnumHelpers
    {
        public static int GetValueCount<TEnum>()
        {
            // Slow...
            var values = (int[])Enum.GetValues(typeof(TEnum));
            var set = new HashSet<int>();
            var count = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (set.Add(values[i]))
                    count++;
            }
            return count;
        }
    }
}
