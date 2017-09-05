using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public static class ControlMappingExtensions
    {
        public static void Mapping(this ControlSetup setup, int sourceIndex, SupportedControl control)
        {
            setup.mappings[sourceIndex] = new ControlMapping(
                    setup.GetControl(control).index);
        }

        public static void Mapping(this ControlSetup setup, int sourceIndex, int destIndex)
        {
            setup.mappings[sourceIndex] = new ControlMapping(
                    destIndex);
        }

        public static void Mapping(this ControlSetup setup, int sourceIndex, SupportedControl control, Range fromRange, Range toRange)
        {
            setup.mappings[sourceIndex] = new ControlMapping(
                    setup.GetControl(control).index,
                    fromRange,
                    toRange);
        }

        public static void SplitMapping(this ControlSetup setup, int sourceIndex, SupportedControl negative, SupportedControl positive)
        {
            setup.mappings[sourceIndex] = new ControlSplitMapping(
                    setup.GetControl(negative).index,
                    setup.GetControl(positive).index);
        }

        public static void HatMapping(this ControlSetup setup, int sourceIndex, SupportedControl left, SupportedControl right, SupportedControl down, SupportedControl up, int startingIndex = 0)
        {
            setup.mappings[sourceIndex] = new ControlHatMapping(
                    setup.GetControl(left).index,
                    setup.GetControl(right).index,
                    setup.GetControl(down).index,
                    setup.GetControl(up).index,
                    startingIndex);
        }
    }

    [Serializable]
    public class ControlMapping : IControlMapping
    {
        public int targetIndex;
        public Range fromRange;
        public Range toRange;

        public ControlMapping() {}

        public ControlMapping(int targetIndex)
        {
            this.targetIndex = targetIndex;
            fromRange = Range.full;
            toRange = Range.full;
        }

        public ControlMapping(int targetIndex, Range fromRange, Range toRange)
        {
            this.targetIndex = targetIndex;
            this.fromRange = fromRange;
            this.toRange = toRange;
        }

        public bool Remap(GenericControlEvent controlEvent)
        {
            if (targetIndex == -1)
                return false;

            controlEvent.controlIndex = targetIndex;

            var floatEvent = controlEvent as GenericControlEvent<float>;
            if (floatEvent == null)
                return false;

            floatEvent.value = Mathf.InverseLerp(fromRange.min, fromRange.max, floatEvent.value);
            floatEvent.value = Mathf.Lerp(toRange.min, toRange.max, floatEvent.value);
            return false;
        }
    }

    [Serializable]
    public class ControlSplitMapping : IControlMapping
    {
        public int targetIndexNeg;
        public Range fromRangeNeg;

        public int targetIndexPos;
        public Range fromRangePos;

        public ControlSplitMapping(int targetIndexNeg, int targetIndexPos)
        {
            this.targetIndexNeg = targetIndexNeg;
            this.targetIndexPos = targetIndexPos;
            fromRangeNeg = Range.negative;
            fromRangePos = Range.positive;
        }

        public ControlSplitMapping(int targetIndexNeg, Range fromRangeNeg, int targetIndexPos, Range fromRangePos)
        {
            this.targetIndexNeg = targetIndexNeg;
            this.fromRangeNeg = fromRangeNeg;
            this.targetIndexPos = targetIndexPos;
            this.fromRangePos = fromRangePos;
        }

        public bool Remap(GenericControlEvent controlEvent)
        {
            var floatEvent = controlEvent as GenericControlEvent<float>;
            if (floatEvent == null)
                return false;

            var eventNeg = InputSystem.CreateEvent<GenericControlEvent<float>>();
            eventNeg.device = floatEvent.device;
            eventNeg.controlIndex = targetIndexNeg;
            eventNeg.value = floatEvent.value;
            eventNeg.time = floatEvent.time;
            eventNeg.value = Mathf.InverseLerp(fromRangeNeg.min, fromRangeNeg.max, eventNeg.value);
            eventNeg.alreadyRemapped = true;
            InputSystem.ExecuteEvent(eventNeg);

            var eventPos = InputSystem.CreateEvent<GenericControlEvent<float>>();
            eventPos.device = floatEvent.device;
            eventPos.controlIndex = targetIndexPos;
            eventPos.value = floatEvent.value;
            eventPos.time = floatEvent.time;
            eventPos.value = Mathf.InverseLerp(fromRangePos.min, fromRangePos.max, eventPos.value);
            eventPos.alreadyRemapped = true;
            InputSystem.ExecuteEvent(eventPos);

            return true;
        }
    }

    [Serializable]
    public class ControlHatMapping : IControlMapping
    {
        public int targetIndexLeft;
        public int targetIndexRight;
        public int targetIndexDown;
        public int targetIndexUp;

        public int startingIndex;

        public ControlHatMapping(int targetIndexLeft, int targetIndexRight, int targetIndexDown, int targetIndexUp, int startingIndex = 0)
        {
            this.targetIndexLeft = targetIndexLeft;
            this.targetIndexRight = targetIndexRight;
            this.targetIndexDown = targetIndexDown;
            this.targetIndexUp = targetIndexUp;
            this.startingIndex = startingIndex;
        }

        public bool Remap(GenericControlEvent controlEvent)
        {
            var floatEvent = controlEvent as FloatValueEvent;
            if (floatEvent == null)
                return false;

            var up = InputSystem.CreateEvent<GenericControlEvent<float>>();
            var down = InputSystem.CreateEvent<GenericControlEvent<float>>();
            var left = InputSystem.CreateEvent<GenericControlEvent<float>>();
            var right = InputSystem.CreateEvent<GenericControlEvent<float>>();

            up.device = down.device = left.device = right.device = floatEvent.device;
            up.time = down.time = left.time = right.time = floatEvent.time;

            up.alreadyRemapped = down.alreadyRemapped = left.alreadyRemapped = right.alreadyRemapped = true;

            left.controlIndex = targetIndexLeft;
            right.controlIndex = targetIndexRight;
            down.controlIndex = targetIndexDown;
            up.controlIndex = targetIndexUp;

            int index = floatEvent.rawValue - startingIndex;
            switch (index)
            {
                case 0: up.value = 1.0f; break;
                case 1: up.value = 1.0f; right.value = 1.0f; break;
                case 2: right.value = 1.0f; break;
                case 3: right.value = 1.0f; down.value = 1.0f; break;
                case 4: down.value = 1.0f; break;
                case 5: down.value = 1.0f; left.value = 1.0f; break;
                case 6: left.value = 1.0f; break;
                case 7: left.value = 1.0f; up.value = 1.0f; break;
            }

            InputSystem.ExecuteEvent(up);
            InputSystem.ExecuteEvent(down);
            InputSystem.ExecuteEvent(left);
            InputSystem.ExecuteEvent(right);

            return true;
        }
    }
}
