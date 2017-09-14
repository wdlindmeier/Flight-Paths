using Assets.Utilities;

namespace UnityEngine.Experimental.Input
{
    public class OpenVRController : TrackedController
    {
        public override void AddStandardControls(ControlSetup setup)
        {
            base.AddStandardControls(setup);

            velocity = (Vector3Control)setup.AddControl(CommonControls.Velocity3d);
            angularVelocity = (Vector3Control)setup.AddControl(CommonControls.AngularVelocity3d);

            trigger = (ButtonControl)setup.AddControl(CommonControls.Trigger);
            grip = (ButtonControl)setup.AddControl(CommonControls.Squeeze);

            triggerTouch = (ButtonControl)setup.AddControl(CommonControls.TriggerTouch);

            trackpadPress = (ButtonControl)setup.AddControl(CommonControls.LeftStickButton);
            trackpadTouch = (ButtonControl)setup.AddControl(CommonControls.LeftStickTouch);
            trackpadX = (AxisControl)setup.AddControl(CommonControls.LeftStickX);
            trackpadY = (AxisControl)setup.AddControl(CommonControls.LeftStickY);
            trackpadLeft = (ButtonControl)setup.AddControl(CommonControls.LeftStickLeft);
            trackpadRight = (ButtonControl)setup.AddControl(CommonControls.LeftStickRight);
            trackpadUp = (ButtonControl)setup.AddControl(CommonControls.LeftStickUp);
            trackpadDown = (ButtonControl)setup.AddControl(CommonControls.LeftStickDown);
            trackpad = (Vector2Control)setup.AddControl(CommonControls.LeftStick);

            // haptics rumble output axis
            rumble = (AxisOutput)setup.AddControl(CommonControls.Vibration);

            // Rename the common controls to their OpenVR equivalents
            action1.name = "Menu";
            grip.name = "Grip";
            trackpadPress.name = "Trackpad Press";
            trackpadTouch.name = "Trackpad Touch";
            trackpadX.name = "Trackpad X";
            trackpadY.name = "Trackpad Y";
            trackpadLeft.name = "Trackpad Left";
            trackpadRight.name = "Trackpad Right";
            trackpadUp.name = "Trackpad Up";
            trackpadDown.name = "Trackpad Down";
            trackpad.name = "Trackpad";

            setup.Mapping(k_MaxNumAxes + 0, action1.index);
            setup.Mapping(k_MaxNumAxes + 2, action1.index);

            setup.Mapping(k_MaxNumAxes + 8, trackpadPress.index);
            setup.Mapping(k_MaxNumAxes + 9, trackpadPress.index);
            setup.Mapping(10, grip.index);
            setup.Mapping(11, grip.index);

            setup.Mapping(0, trackpadX.index);
            setup.Mapping(1, trackpadY.index);
            setup.Mapping(3, trackpadX.index);
            setup.Mapping(4, trackpadY.index);

            setup.Mapping(k_MaxNumAxes + 16, trackpadTouch.index);
            setup.Mapping(k_MaxNumAxes + 17, trackpadTouch.index);

            setup.Mapping(8, trigger.index);
            setup.Mapping(9, trigger.index);
            setup.Mapping(k_MaxNumAxes + 14, triggerTouch.index);
            setup.Mapping(k_MaxNumAxes + 15, triggerTouch.index);
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var consumed = false;

            // Uncrack the velocity values here. then pass to the base class for position/rotation
            var trackingEvent = inputEvent as TrackingEvent;
            if (trackingEvent != null)
            {
                if ((trackingEvent.availableFields & TrackingEvent.Flags.VelocityAvailable) != 0)
                    consumed |= intoState.SetValueFromEvent(velocity.index, trackingEvent.velocity);

                if ((trackingEvent.availableFields & TrackingEvent.Flags.AngularVelocityAvailable) != 0)
                    consumed |= intoState.SetValueFromEvent(angularVelocity.index, trackingEvent.angularVelocity);
            }

            consumed |= base.ProcessEventIntoState(inputEvent, intoState);
            return consumed;
        }

        public override void PostProcessState(InputState state)
        {
            // Right now all dead zones come from the default dead zones on the device profile.
            // Maybe in the future copy deadzone over ti fields on Gamepad,
            // either a single field or per control, and make it possible to change through API.
            var openVRControllerProfile = (OpenVRControllerProfile)profile;
            var deadZones = openVRControllerProfile != null ? openVRControllerProfile.defaultDeadZones : Range.positive;

            // We flip the y axis here to match the expect up-down on the trackpad
            var trackpadYControl = (AxisControl)state.controls[trackpadY.index];
            var trackpadYValue = trackpadYControl.rawValue;
            trackpadYControl.rawValue = -trackpadYValue;

            ControlHelper.BuildAxisInputCircularBasedOnXY(
                (ButtonControl)state.controls[trackpadLeft.index],
                (ButtonControl)state.controls[trackpadRight.index],
                (ButtonControl)state.controls[trackpadDown.index],
                (ButtonControl)state.controls[trackpadUp.index],
                (AxisControl)state.controls[trackpadX.index],
                (AxisControl)state.controls[trackpadY.index],
                (Vector2Control)state.controls[trackpad.index],
                deadZones);

            // Reset the axis flipping
            trackpadYControl.rawValue = trackpadYValue;
        }

        public override void PostProcessEnabledControls(InputState state)
        {
            ControlHelper.SetAxisControlsEnabledBasedOnXY(
                (ButtonControl)state.controls[trackpadLeft.index],
                (ButtonControl)state.controls[trackpadRight.index],
                (ButtonControl)state.controls[trackpadDown.index],
                (ButtonControl)state.controls[trackpadUp.index],
                (AxisControl)state.controls[trackpadX.index],
                (AxisControl)state.controls[trackpadY.index],
                (Vector2Control)state.controls[trackpad.index]);
        }

        // Position / Rotation are provided by the underlying Tracked Controller class.

        public Vector3Control velocity { get; private set; }
        public Vector3Control angularVelocity { get; private set; }

        public ButtonControl triggerTouch { get; private set; }
        public ButtonControl trigger { get; private set; }
        public ButtonControl grip { get; private set; }

        public ButtonControl trackpadPress { get; private set; }
        public ButtonControl trackpadTouch { get; private set; }
        public AxisControl trackpadX { get; private set; }
        public AxisControl trackpadY { get; private set; }
        public ButtonControl trackpadLeft { get; private set; }
        public ButtonControl trackpadRight { get; private set; }
        public ButtonControl trackpadDown { get; private set; }
        public ButtonControl trackpadUp { get; private set; }
        public Vector2Control trackpad { get; private set; }
        public AxisOutput rumble { get; private set; }
    }
}
