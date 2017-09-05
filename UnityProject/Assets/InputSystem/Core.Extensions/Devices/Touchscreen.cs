using System.Collections.Generic;

////TODO: add mouse emulation code as something that can be hooked into the event tree

namespace UnityEngine.Experimental.Input
{
    public class Touchscreen : Pointer
    {
        public const int kMaxConcurrentTouches = 10;
        public const int kInvalidFingerId = 0;

        public new static Touchscreen current { get { return InputSystem.GetCurrentDeviceOfType<Touchscreen>(); } }

        public override void AddStandardControls(ControlSetup setup)
        {
            base.AddStandardControls(setup);

            for (var i = 0; i < kMaxConcurrentTouches; ++i)
            {
                var controls = new TouchControls();
                m_TouchControls.Add(controls);

                var prefix = "Touch " + i;
                controls.isDown = (ButtonControl)setup.AddControl(SupportedControl.Get<ButtonControl>(prefix));
                controls.fingerId = (DiscreteStatesControl)setup.AddControl(SupportedControl.Get<DiscreteStatesControl>(prefix + " ID"));
                controls.position = (Vector2Control)setup.AddControl(SupportedControl.Get<Vector2Control>(prefix + " Position"));
                controls.positionX = (AxisControl)setup.AddControl(SupportedControl.Get<AxisControl>(prefix + " Position X"));
                controls.positionY = (AxisControl)setup.AddControl(SupportedControl.Get<AxisControl>(prefix + " Position Y"));
                controls.delta = (Vector2Control)setup.AddControl(SupportedControl.Get<Vector2Control>(prefix + " Delta"));
                controls.deltaX = (DeltaAxisControl)setup.AddControl(SupportedControl.Get<DeltaAxisControl>(prefix + " Delta X"));
                controls.deltaY = (DeltaAxisControl)setup.AddControl(SupportedControl.Get<DeltaAxisControl>(prefix + " Delta Y"));
                controls.radius = (Vector2Control)setup.AddControl(SupportedControl.Get<Vector2Control>(prefix + " Radius"));
                controls.radiusX = (AxisControl)setup.AddControl(SupportedControl.Get<AxisControl>(prefix + " Radius X"));
                controls.radiusY = (AxisControl)setup.AddControl(SupportedControl.Get<AxisControl>(prefix + " Radius Y"));
                controls.pressure = (AxisControl)setup.AddControl(SupportedControl.Get<AxisControl>(prefix + " Pressure"));
            }

            setup.GetControl(CommonControls.DoubleClick).name = "Double Tap";
            setup.GetControl(CommonControls.Action1).name = "One Finger";
            setup.GetControl(CommonControls.Action2).name = "Two Fingers";
        }

        public override void BeginUpdate()
        {
            base.BeginUpdate();

            ////TODO: this will also be necessary for the state copies used by ActionMaps; however, ATM there is no way to "pre-process" those with device-dependent logic
            // Reset finger IDs of released touches so that we can reuse their entries. We do that here in BeginUpdate()
            // rather than directly in ProcessEventIntoState so that released fingers keep their state during their respective
            // update and only go back to unused in the next update.
            for (var i = 0; i < m_TouchControls.Count; ++i)
            {
                var controls = m_TouchControls[i];
                if (!controls.isDown.isHeld)
                    controls.fingerId.value = 0;
            }
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            ////TODO: cancelation
            ////TODO: set base controls (position etc) from the first finger (whichever one that currently is)
            ////FIXME: fingers can get stuck

            var pointerEvent = inputEvent as PointerEvent;
            if (pointerEvent != null)
            {
                int touchIndex = GetTouchIndexForFinger(pointerEvent.pointerId);
                if (touchIndex != -1)
                {
                    var controls = m_TouchControls[touchIndex];
                    var isDown = pointerEvent is PointerDownEvent || pointerEvent is PointerMoveEvent;

                    var consumed = false;

                    consumed |= intoState.SetValueFromEvent(controls.isDown.index, isDown ? 1.0f : 0.0f);
                    consumed |= intoState.SetValueFromEvent(controls.fingerId.index, pointerEvent.pointerId);
                    consumed |= intoState.SetValueFromEvent(controls.position.index, new Vector2(pointerEvent.position.x, pointerEvent.position.y));
                    consumed |= intoState.SetValueFromEvent(controls.positionX.index, pointerEvent.position.x);
                    consumed |= intoState.SetValueFromEvent(controls.positionY.index, pointerEvent.position.y);
                    consumed |= intoState.SetValueFromEvent(controls.radius.index, new Vector2(pointerEvent.radius.x, pointerEvent.radius.y));
                    consumed |= intoState.SetValueFromEvent(controls.radiusX.index, pointerEvent.radius.x);
                    consumed |= intoState.SetValueFromEvent(controls.radiusY.index, pointerEvent.radius.y);
                    consumed |= intoState.SetValueFromEvent(controls.pressure.index, pointerEvent.pressure);

                    var moveEvent = pointerEvent as PointerMoveEvent;
                    if (moveEvent != null)
                    {
                        consumed |= intoState.SetValueFromEvent(controls.delta.index, new Vector2(moveEvent.delta.x, moveEvent.delta.y));
                        consumed |= intoState.SetValueFromEvent(controls.deltaX.index, moveEvent.delta.x);
                        consumed |= intoState.SetValueFromEvent(controls.deltaY.index, moveEvent.delta.y);
                    }

                    // Mark 'touches' as out-of-date.
                    m_TouchesUpdated = false;

                    if (consumed)
                        return true;
                }
            }

            return base.ProcessEventIntoState(inputEvent, intoState);
        }

        private int GetTouchIndexForFinger(int fingerId)
        {
            var firstFreeIndex = -1;

            for (var i = 0; i < m_TouchControls.Count; ++i)
            {
                var currentFingerId = m_TouchControls[i].fingerId.value;
                if (currentFingerId == fingerId)
                    return i;

                if (currentFingerId == kInvalidFingerId && firstFreeIndex == -1)
                    firstFreeIndex = i;
            }

            if (firstFreeIndex != -1)
                return firstFreeIndex;

            // Out of available touches.
            return -1;
        }

        public List<Touch> touches
        {
            get
            {
                if (!m_TouchesUpdated)
                    UpdateTouches();

                return m_Touches;
            }
        }

        ////REVIEW: this might be worth exposing
        private class TouchControls
        {
            public ButtonControl isDown;
            public DiscreteStatesControl fingerId;
            public Vector2Control position;
            public AxisControl positionX;
            public AxisControl positionY;
            public Vector2Control delta;
            public AxisControl deltaX;
            public AxisControl deltaY;
            public Vector2Control radius;
            public AxisControl radiusX;
            public AxisControl radiusY;
            public AxisControl pressure;
        }

        private bool m_TouchesUpdated;
        private List<Touch> m_Touches = new List<Touch>(kMaxConcurrentTouches);
        // This keeps a list of the controls we add for each touch.
        private List<TouchControls> m_TouchControls = new List<TouchControls>(kMaxConcurrentTouches);

        // Convert the state we keep to a list of UnityEngine.Touch instances.
        private void UpdateTouches()
        {
            m_Touches.Clear();

            for (var i = 0; i < m_TouchControls.Count; ++i)
            {
                var controls = m_TouchControls[i];
                if (controls.fingerId.value == kInvalidFingerId)
                    continue;

                var phase = TouchPhase.Moved;
                if (!controls.isDown.isHeld)
                    phase = TouchPhase.Ended;
                else if (controls.isDown.wasJustPressed)
                    phase = TouchPhase.Began;
                else if (!controls.position.changedValue)
                    phase = TouchPhase.Stationary;

                m_Touches.Add(new Touch
                {
                    fingerId = controls.fingerId.value,
                    phase = phase,
                    position = controls.position.value,
                    rawPosition = controls.position.value,
                    deltaPosition = controls.delta.value,
                    pressure = controls.pressure.value,
                    radius = Mathf.Max(controls.radius.value.x, controls.radius.value.y)
                        ////TODO: deltaTime
                });
            }

            m_TouchesUpdated = true;
        }
    }
}
