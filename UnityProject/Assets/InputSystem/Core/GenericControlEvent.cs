using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public abstract class GenericControlEvent : InputEvent
    {
        public int controlIndex { get; set; }
        public bool alreadyRemapped { get; set; }
        public abstract void CopyDefaultValueFromControl(object control);
        public abstract void CopyValueToControl(object control);

        public override void Reset()
        {
            base.Reset();
            controlIndex = 0;
            alreadyRemapped = false;
        }
    }

    public class GenericControlEvent<T> : GenericControlEvent
    {
        public T value { get; set; }

        public override void Reset()
        {
            base.Reset();
            value = default(T);
        }

        public override void CopyDefaultValueFromControl(object control)
        {
            value = ((InputControl<T>)control).defaultValue;
        }

        public override void CopyValueToControl(object control)
        {
            ((InputControl<T>)control).SetValueFromEvent(value);
        }

        public override string ToString()
        {
            return string.Format("({0}, index:{1}, value:{2})", base.ToString(), controlIndex, value);
        }
    }
}
