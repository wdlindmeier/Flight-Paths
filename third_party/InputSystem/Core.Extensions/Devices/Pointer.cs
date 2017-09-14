using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // A device that can point at and click on things.
    public class Pointer : InputDevice
    {
        public static Pointer current { get { return InputSystem.GetCurrentDeviceOfType<Pointer>(); } }

        public override void AddStandardControls(ControlSetup setup)
        {
            position = (Vector3Control)setup.AddControl(CommonControls.Position3d);
            positionX = (AxisControl)setup.AddControl(CommonControls.PositionX);
            positionY = (AxisControl)setup.AddControl(CommonControls.PositionY);
            positionZ = (AxisControl)setup.AddControl(CommonControls.PositionZ);

            delta = (Vector3Control)setup.AddControl(CommonControls.Delta3d);
            deltaX = (AxisControl)setup.AddControl(CommonControls.DeltaX);
            deltaY = (AxisControl)setup.AddControl(CommonControls.DeltaY);
            deltaZ = (AxisControl)setup.AddControl(CommonControls.DeltaZ);

            pressure = (AxisControl)setup.AddControl(CommonControls.Pressure);
            twist = (AxisControl)setup.AddControl(CommonControls.Twist);

            tilt = (Vector2Control)setup.AddControl(CommonControls.Tilt2d);
            tiltX = (AxisControl)setup.AddControl(CommonControls.TiltX);
            tiltY = (AxisControl)setup.AddControl(CommonControls.TiltY);

            radius = (Vector3Control)setup.AddControl(CommonControls.Radius3d);
            radiusX = (AxisControl)setup.AddControl(CommonControls.RadiusX);
            radiusY = (AxisControl)setup.AddControl(CommonControls.RadiusY);
            radiusZ = (AxisControl)setup.AddControl(CommonControls.RadiusZ);

            // For these, subclasses or profiles should assign better names.
            primaryAction = (ButtonControl)setup.AddControl(CommonControls.Action1);
            secondaryAction = (ButtonControl)setup.AddControl(CommonControls.Action2);

            doubleClick = (EventControl)setup.AddControl(CommonControls.DoubleClick);
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var consumed = false;

            var pointerEvent = inputEvent as PointerEvent;
            if (pointerEvent != null)
            {
                consumed |= intoState.SetValueFromEvent(positionX.index, pointerEvent.position.x);
                consumed |= intoState.SetValueFromEvent(positionY.index, pointerEvent.position.y);
                consumed |= intoState.SetValueFromEvent(positionZ.index, pointerEvent.position.z);

                consumed |= intoState.SetValueFromEvent(radiusX.index, pointerEvent.radius.x);
                consumed |= intoState.SetValueFromEvent(radiusY.index, pointerEvent.radius.y);
                consumed |= intoState.SetValueFromEvent(radiusZ.index, pointerEvent.radius.z);

                consumed |= intoState.SetValueFromEvent(tiltX.index, pointerEvent.tilt.x);
                consumed |= intoState.SetValueFromEvent(tiltY.index, pointerEvent.tilt.y);

                consumed |= intoState.SetValueFromEvent(twist.index, pointerEvent.twist);
                consumed |= intoState.SetValueFromEvent(pressure.index, pointerEvent.pressure);

                var moveEvent = pointerEvent as PointerMoveEvent;
                if (moveEvent != null)
                {
                    consumed |= intoState.SetValueFromEvent(deltaX.index, moveEvent.delta.x);
                    consumed |= intoState.SetValueFromEvent(deltaY.index, moveEvent.delta.y);
                    consumed |= intoState.SetValueFromEvent(deltaZ.index, moveEvent.delta.z);
                }

                return consumed;
            }

            var floatEvent = inputEvent as GenericControlEvent<float>;
            if (floatEvent != null)
            {
                switch (floatEvent.controlIndex)
                {
                    case 0:
                        consumed = intoState.SetValueFromEvent(primaryAction.index, floatEvent.value);
                        break;
                    case 1:
                        consumed = intoState.SetValueFromEvent(secondaryAction.index, floatEvent.value);
                        break;
                }

                if (consumed)
                    return consumed;
            }

            var clickEvent = inputEvent as ClickEvent;
            if (clickEvent != null)
            {
                switch (clickEvent.controlIndex)
                {
                    case 0:
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX //OSX/Windows differ on whether to send a double click event on the press or release of the second click.
                        // We use Modulo so that on OSX a click count of 4 will return a second double click (there is no possibility of a 0 click)
                        if ((clickEvent.clickCount % 2 == 0) && !clickEvent.isDown)
#else
                        if ((clickEvent.clickCount % 2 == 0) && clickEvent.isDown)
#endif
                        {
                            consumed = intoState.SetValueFromEvent(doubleClick.index, true);
                        }
                        break;
                }

                // There are no valid pointer event handlers beyond this point
                return consumed;
            }

            return base.ProcessEventIntoState(inputEvent, intoState);
        }

        public override void PostProcessState(InputState state)
        {
            var positionValue = new Vector3(
                    ((AxisControl)state.controls[positionX.index]).value,
                    ((AxisControl)state.controls[positionY.index]).value,
                    ((AxisControl)state.controls[positionZ.index]).value
                    );
            ((Vector3Control)state.controls[position.index]).value = positionValue;

            var deltaValue = new Vector3(
                    ((AxisControl)state.controls[deltaX.index]).value,
                    ((AxisControl)state.controls[deltaY.index]).value,
                    ((AxisControl)state.controls[deltaZ.index]).value
                    );
            ((Vector3Control)state.controls[delta.index]).value = deltaValue;

            var radiusValue = new Vector3(
                    ((AxisControl)state.controls[radiusX.index]).value,
                    ((AxisControl)state.controls[radiusY.index]).value,
                    ((AxisControl)state.controls[radiusZ.index]).value
                    );
            ((Vector3Control)state.controls[radius.index]).value = radiusValue;

            var tiltValue = new Vector2(
                    ((AxisControl)state.controls[tiltX.index]).value,
                    ((AxisControl)state.controls[tiltY.index]).value
                    );
            ((Vector2Control)state.controls[tilt.index]).value = tiltValue;
        }

        public Vector3Control position { get; private set; }
        public AxisControl positionX { get; private set; }
        public AxisControl positionY { get; private set; }
        public AxisControl positionZ { get; private set; }

        public Vector3Control delta { get; private set; }
        public AxisControl deltaX { get; private set; }
        public AxisControl deltaY { get; private set; }
        public AxisControl deltaZ { get; private set; }

        public ButtonControl primaryAction { get; private set; }
        public ButtonControl secondaryAction { get; private set; }

        public EventControl doubleClick { get; private set; }

        public AxisControl pressure { get; private set; }
        public AxisControl twist { get; private set; }

        public Vector3Control radius { get; private set; }
        public AxisControl radiusX { get; private set; }
        public AxisControl radiusY { get; private set; }
        public AxisControl radiusZ { get; private set; }

        public Vector2Control tilt { get; private set; }
        public AxisControl tiltX { get; private set; }
        public AxisControl tiltY { get; private set; }

        // Convenience aliases.
        public AxisControl horizontal { get { return positionX; } }
        public AxisControl vertical { get { return positionY;  } }
        public AxisControl horizontalDelta { get { return deltaX; } }
        public AxisControl verticalDelta { get { return deltaY;  } }

        ////REVIEW: okay, maybe the concept of a per-pointer cursor is bogus after all...
        public Cursor cursor { get; protected set; }
    }
}
