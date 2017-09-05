using Assets.Utilities;

namespace UnityEngine.Experimental.Input
{
    public class OculusTouchController : TrackedController
    {
        public override void AddStandardControls(ControlSetup setup)
        {
            base.AddStandardControls(setup);

            acceleration = (Vector3Control)setup.AddControl(CommonControls.Acceleration3d);
            angularAcceleration = (Vector3Control)setup.AddControl(CommonControls.AngularAcceleration3d);
            velocity = (Vector3Control)setup.AddControl(CommonControls.Velocity3d);
            angularVelocity = (Vector3Control)setup.AddControl(CommonControls.AngularVelocity3d);

            trigger = (ButtonControl)setup.AddControl(CommonControls.Trigger);
            handTrigger = (ButtonControl)setup.AddControl(CommonControls.Squeeze);

            triggerTouch = (ButtonControl)setup.AddControl(CommonControls.TriggerTouch);
            triggerNearTouch = (ButtonControl)setup.AddControl(SupportedControl.Get<ButtonControl>("Trigger Near Touch"));

            action1Touch = (ButtonControl)setup.AddControl(SupportedControl.Get<ButtonControl>("Action 1 Touch"));

            action2 = (ButtonControl)setup.AddControl(CommonControls.Action2);
            action2Touch = (ButtonControl)setup.AddControl(SupportedControl.Get<ButtonControl>("Action 2 Touch"));

            thumbRestTouch = (ButtonControl)setup.AddControl(SupportedControl.Get<ButtonControl>("Thumb Rest Touch"));
            thumbNearTouch = (ButtonControl)setup.AddControl(SupportedControl.Get<ButtonControl>("Thumb Near Touch"));

            start = (ButtonControl)setup.AddControl(CommonControls.Start);

            // Ideally we would be able to switch these to right stick based on handedness change
            stickPress = (ButtonControl)setup.AddControl(CommonControls.LeftStickButton);
            stickTouch = (ButtonControl)setup.AddControl(CommonControls.LeftStickTouch);
            stickX = (AxisControl)setup.AddControl(CommonControls.LeftStickX);
            stickY = (AxisControl)setup.AddControl(CommonControls.LeftStickY);
            stickLeft = (ButtonControl)setup.AddControl(CommonControls.LeftStickLeft);
            stickRight = (ButtonControl)setup.AddControl(CommonControls.LeftStickRight);
            stickUp = (ButtonControl)setup.AddControl(CommonControls.LeftStickUp);
            stickDown = (ButtonControl)setup.AddControl(CommonControls.LeftStickDown);
            stick = (Vector2Control)setup.AddControl(CommonControls.LeftStick);

            // haptics rumble output axis
            rumble = (AxisOutput)setup.AddControl(CommonControls.Vibration);

            handTrigger.name = "Hand Trigger";
            stickPress.name = "Stick Press";
            stickTouch.name = "Stick Touch";
            stickX.name = "Stick X";
            stickY.name = "Stick Y";
            stickLeft.name = "Stick Left";
            stickRight.name = "Stick Right";
            stickUp.name = "Stick Up";
            stickDown.name = "Stick Down";
            stick.name = "Stick";

            setup.Mapping(k_MaxNumAxes + 0, action1.index);
            setup.Mapping(k_MaxNumAxes + 1, action2.index);
            setup.Mapping(k_MaxNumAxes + 2, action1.index);
            setup.Mapping(k_MaxNumAxes + 3, action2.index);
            setup.Mapping(k_MaxNumAxes + 10, action1Touch.index);
            setup.Mapping(k_MaxNumAxes + 11, action2Touch.index);
            setup.Mapping(k_MaxNumAxes + 12, action1Touch.index);
            setup.Mapping(k_MaxNumAxes + 13, action2Touch.index);

            setup.Mapping(k_MaxNumAxes + 7, start.index);
            setup.Mapping(k_MaxNumAxes + 8, stickPress.index);
            setup.Mapping(k_MaxNumAxes + 9, stickPress.index);
            setup.Mapping(k_MaxNumAxes + 16, handTrigger.index);
            setup.Mapping(k_MaxNumAxes + 17, handTrigger.index);

            setup.Mapping(0, stickX.index);
            setup.Mapping(1, stickY.index);
            setup.Mapping(3, stickX.index);
            setup.Mapping(4, stickY.index);

            setup.Mapping(k_MaxNumAxes + 16, stickTouch.index);
            setup.Mapping(k_MaxNumAxes + 17, stickTouch.index);

            setup.Mapping(8, trigger.index);
            setup.Mapping(9, trigger.index);
            setup.Mapping(k_MaxNumAxes + 14, triggerTouch.index);
            setup.Mapping(k_MaxNumAxes + 15, triggerTouch.index);
            setup.Mapping(12, triggerNearTouch.index);
            setup.Mapping(13, triggerNearTouch.index);

            setup.Mapping(10, handTrigger.index);
            setup.Mapping(11, handTrigger.index);

            setup.Mapping(14, thumbNearTouch.index);
            setup.Mapping(15, thumbNearTouch.index);
            setup.Mapping(k_MaxNumAxes + 18, thumbRestTouch.index);
            setup.Mapping(k_MaxNumAxes + 19, thumbRestTouch.index);
        }

        protected override void OnTagChanged()
        {
            base.OnTagChanged();
            if (tag == CommonDeviceTags.Left)
            {
                GetControl(SupportedControl.Get<ButtonControl>("Action 1 Touch")).name = "X Touch";
                GetControl(SupportedControl.Get<ButtonControl>("Action 2 Touch")).name = "X Touch";
                GetControl(CommonControls.Action1).name = "X";
                GetControl(CommonControls.Action2).name = "Y";
            }
            else if (tag == CommonDeviceTags.Right)
            {
                GetControl(SupportedControl.Get<ButtonControl>("Action 1 Touch")).name = "A Touch";
                GetControl(SupportedControl.Get<ButtonControl>("Action 2 Touch")).name = "B Touch";
                GetControl(CommonControls.Action1).name = "A";
                GetControl(CommonControls.Action2).name = "B";
            }
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var consumed = false;

            // Uncrack the acceleration & velocity values here. then pass to the base class for position/rotation
            var trackingEvent = inputEvent as TrackingEvent;
            if (trackingEvent != null)
            {
                if ((trackingEvent.availableFields & TrackingEvent.Flags.VelocityAvailable) != 0)
                    consumed |= intoState.SetValueFromEvent(velocity.index, trackingEvent.velocity);

                if ((trackingEvent.availableFields & TrackingEvent.Flags.AngularVelocityAvailable) != 0)
                    consumed |= intoState.SetValueFromEvent(angularVelocity.index, trackingEvent.angularVelocity);

                if ((trackingEvent.availableFields & TrackingEvent.Flags.AccelerationAvailable) != 0)
                    consumed |= intoState.SetValueFromEvent(acceleration.index, trackingEvent.acceleration);

                if ((trackingEvent.availableFields & TrackingEvent.Flags.AngularAccelerationAvailable) != 0)
                    consumed |= intoState.SetValueFromEvent(angularAcceleration.index, trackingEvent.angularAcceleration);
            }

            consumed |= base.ProcessEventIntoState(inputEvent, intoState);
            return consumed;
        }

        public override void PostProcessState(InputState state)
        {
            // Right now all dead zones come from the default dead zones on the device profile.
            // Maybe in the future copy deadzone over ti fields on Gamepad,
            // either a single field or per control, and make it possible to change through API.
            var oculusTouchProfile = (OculusTouchProfile)profile;
            var deadZones = oculusTouchProfile != null ? oculusTouchProfile.defaultDeadZones : Range.positive;

            ControlHelper.BuildAxisInputCircularBasedOnXY(
                (ButtonControl)state.controls[stickLeft.index],
                (ButtonControl)state.controls[stickRight.index],
                (ButtonControl)state.controls[stickDown.index],
                (ButtonControl)state.controls[stickUp.index],
                (AxisControl)state.controls[stickX.index],
                (AxisControl)state.controls[stickY.index],
                (Vector2Control)state.controls[stick.index],
                deadZones);
        }

        public override void PostProcessEnabledControls(InputState state)
        {
            ControlHelper.SetAxisControlsEnabledBasedOnXY(
                (ButtonControl)state.controls[stickLeft.index],
                (ButtonControl)state.controls[stickRight.index],
                (ButtonControl)state.controls[stickDown.index],
                (ButtonControl)state.controls[stickUp.index],
                (AxisControl)state.controls[stickX.index],
                (AxisControl)state.controls[stickY.index],
                (Vector2Control)state.controls[stick.index]);
        }

        // Position / Rotation are provided by the underlying Tracked Controller class.

        public Vector3Control acceleration { get; private set; }
        public Vector3Control angularAcceleration { get; private set; }
        public Vector3Control velocity { get; private set; }
        public Vector3Control angularVelocity { get; private set; }

        public ButtonControl triggerTouch { get; private set; }
        public ButtonControl triggerNearTouch { get; private set; }
        public ButtonControl trigger { get; private set; }
        public ButtonControl handTrigger { get; private set; }

        public ButtonControl action1Touch { get; private set; }
        public ButtonControl action2 { get; private set; }
        public ButtonControl action2Touch { get; private set; }

        public ButtonControl thumbRestTouch { get; private set; }
        public ButtonControl thumbNearTouch { get; private set; }

        public ButtonControl start { get; private set; }

        public ButtonControl stickPress { get; private set; }
        public ButtonControl stickTouch { get; private set; }
        public AxisControl stickX { get; private set; }
        public AxisControl stickY { get; private set; }
        public ButtonControl stickLeft { get; private set; }
        public ButtonControl stickRight { get; private set; }
        public ButtonControl stickDown { get; private set; }
        public ButtonControl stickUp { get; private set; }
        public Vector2Control stick { get; private set; }
        public AxisOutput rumble { get; private set; }
    }
}
