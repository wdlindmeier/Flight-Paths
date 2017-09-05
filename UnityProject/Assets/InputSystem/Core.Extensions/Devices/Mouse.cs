using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class Mouse : Pointer
    {
        public new static Mouse current { get { return InputSystem.GetCurrentDeviceOfType<Mouse>(); } }

        public Mouse()
        {
            cursor = new Cursor();
        }

        public override void AddStandardControls(ControlSetup setup)
        {
            base.AddStandardControls(setup);

            middleButton = (ButtonControl)setup.AddControl(CommonControls.Action3);

            scrollWheel = (Vector2Control)setup.AddControl(CommonControls.ScrollWheel2d);
            scrollWheelX = (AxisControl)setup.AddControl(CommonControls.ScrollWheelX);
            scrollWheelY = (AxisControl)setup.AddControl(CommonControls.ScrollWheelY);

            setup.GetControl(CommonControls.Action1).name = "Left Button";
            setup.GetControl(CommonControls.Action2).name = "Right Button";
            setup.GetControl(CommonControls.Action3).name = "Middle Button";

            setup.GetControl(CommonControls.ScrollWheelX).name = "Scroll Horizontal";
            setup.GetControl(CommonControls.ScrollWheelY).name = "Scroll Vertical";
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var consumed = false;

            var floatEvent = inputEvent as GenericControlEvent<float>;
            if (floatEvent != null)
            {
                switch (floatEvent.controlIndex)
                {
                    case 2: consumed = intoState.SetValueFromEvent(middleButton.index, floatEvent.value); break;
                    case 3: consumed = intoState.SetValueFromEvent(scrollWheelY.index, floatEvent.value); break;
                    case 4: consumed = intoState.SetValueFromEvent(scrollWheelX.index, floatEvent.value); break;
                }

                if (consumed)
                    return true;
            }

            return base.ProcessEventIntoState(inputEvent, intoState);
        }

        public override void PostProcessState(InputState state)
        {
            base.PostProcessState(state);

            var scrollWheelValue = new Vector2(
                    ((AxisControl)state.controls[scrollWheelX.index]).value,
                    ((AxisControl)state.controls[scrollWheelY.index]).value
                    );
            ((Vector2Control)state.controls[scrollWheel.index]).value = scrollWheelValue;
        }

        public ButtonControl leftButton { get { return primaryAction; } }
        public ButtonControl rightButton { get { return secondaryAction; } }
        public ButtonControl middleButton { get; private set; }

        public Vector2Control scrollWheel { get; private set; }
        public AxisControl scrollWheelX { get; private set; }
        public AxisControl scrollWheelY { get; private set; }
    }
}
