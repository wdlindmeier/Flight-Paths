using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Pen device that moves across digitizer surface. Tip is considered button mapping to primary action,
    // eraser (if present) is considered button mapping to secondary action.
    public class Stylus : Pointer
    {
        public new static Stylus current { get { return InputSystem.GetCurrentDeviceOfType<Stylus>(); } }

        public override void AddStandardControls(ControlSetup setup)
        {
            base.AddStandardControls(setup);

            barrelButton = (ButtonControl)setup.AddControl(CommonControls.Action3);

            setup.GetControl(CommonControls.Action1).name = "Tip";
            setup.GetControl(CommonControls.Action2).name = "Eraser";
            setup.GetControl(CommonControls.Action3).name = "Barrel Button";
        }

        ////FIXME: not getting eraser events with my Wacom

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var consumed = false;

            // Sync tip and eraser buttons from pointer IDs on pointer events.
            // Note that we still let those events pass down into the Pointer base class for
            // handling the other pointer-related data fields.
            var downEvent = inputEvent as PointerDownEvent;
            if (downEvent != null)
            {
                if (downEvent.pointerId == kTipPointerId)
                    consumed |= intoState.SetValueFromEvent(tip.index, 1.0f);
                else if (downEvent.pointerId == kEraserPointerId)
                    consumed |= intoState.SetValueFromEvent(eraser.index, 1.0f);
            }
            else
            {
                var upEvent = inputEvent as PointerUpEvent;
                if (upEvent != null)
                {
                    if (upEvent.pointerId == kTipPointerId)
                        consumed |= intoState.SetValueFromEvent(tip.index, 0.0f);
                    else if (upEvent.pointerId == kEraserPointerId)
                        consumed |= intoState.SetValueFromEvent(eraser.index, 0.0f);
                }
            }

            var floatEvent = inputEvent as GenericControlEvent<float>;
            if (floatEvent != null)
            {
                switch (floatEvent.controlIndex)
                {
                    case 2: consumed = intoState.SetValueFromEvent(barrelButton.index, floatEvent.value); break;
                }

                if (consumed)
                    return true;
            }

            return base.ProcessEventIntoState(inputEvent, intoState);
        }

        public ButtonControl tip { get { return primaryAction; } }
        public ButtonControl eraser { get { return secondaryAction; } }
        public ButtonControl barrelButton { get; private set; }

        public const int kTipPointerId = 0;
        public const int kEraserPointerId = 1;
    }
}
