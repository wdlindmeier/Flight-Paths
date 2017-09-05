using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class OculusRemote : InputDevice
    {
        public OculusRemote() : base() {}

        public override void AddStandardControls(ControlSetup setup)
        {
            start = (ButtonControl)setup.AddControl(CommonControls.Start);
            back = (ButtonControl)setup.AddControl(CommonControls.Back);

            dPadLeft = (ButtonControl)setup.AddControl(CommonControls.DPadLeft);
            dPadRight = (ButtonControl)setup.AddControl(CommonControls.DPadRight);
            dPadDown = (ButtonControl)setup.AddControl(CommonControls.DPadDown);
            dPadUp = (ButtonControl)setup.AddControl(CommonControls.DPadUp);
            dPadX = (AxisControl)setup.AddControl(CommonControls.DPadX);
            dPadY = (AxisControl)setup.AddControl(CommonControls.DPadY);
            dPad = (Vector2Control)setup.AddControl(CommonControls.DPad);

            setup.Mapping(k_MaxNumAxes + 0, start.index);
            setup.Mapping(k_MaxNumAxes + 1, back.index);
            setup.Mapping(5, dPadX.index);
            setup.Mapping(6, dPadY.index);
        }

        public override void PostProcessState(InputState state)
        {
            // Right now all dead zones come from the default dead zones on the device profile.
            // Maybe in the future copy deadzone over ti fields on Gamepad,
            // either a single field or per control, and make it possible to change through API.
            var oculusTouchProfile = (OculusRemoteProfile)profile;
            Range deadZones;
            if (oculusTouchProfile != null)
                deadZones = oculusTouchProfile.defaultDeadZones;
            else
                deadZones = Range.positive;

            ControlHelper.BuildAxisInputCircularBasedOnXY(
                (ButtonControl)state.controls[dPadLeft.index],
                (ButtonControl)state.controls[dPadRight.index],
                (ButtonControl)state.controls[dPadDown.index],
                (ButtonControl)state.controls[dPadUp.index],
                (AxisControl)state.controls[dPadX.index],
                (AxisControl)state.controls[dPadY.index],
                (Vector2Control)state.controls[dPad.index],
                deadZones);
        }

        public override void PostProcessEnabledControls(InputState state)
        {
            ControlHelper.SetAxisControlsEnabledBasedOnXY(
                (ButtonControl)state.controls[dPadLeft.index],
                (ButtonControl)state.controls[dPadRight.index],
                (ButtonControl)state.controls[dPadDown.index],
                (ButtonControl)state.controls[dPadUp.index],
                (AxisControl)state.controls[dPadX.index],
                (AxisControl)state.controls[dPadY.index],
                (Vector2Control)state.controls[dPad.index]);
        }

        // Position / Rotation are provided by the underlying Tracked Controller class.

        public ButtonControl start { get; private set; }
        public ButtonControl back { get; private set; }

        public ButtonControl dPadLeft { get; private set; }
        public ButtonControl dPadRight { get; private set; }
        public ButtonControl dPadDown { get; private set; }
        public ButtonControl dPadUp { get; private set; }
        public AxisControl dPadX { get; private set; }
        public AxisControl dPadY { get; private set; }
        public Vector2Control dPad { get; private set; }
    }
}
