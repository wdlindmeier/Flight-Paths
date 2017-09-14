using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class Gamepad : InputDevice
    {
        public static Gamepad current { get { return InputSystem.GetCurrentDeviceOfType<Gamepad>(); } }

        ////REVIEW: It's odd that this method modifies both the state of ControlSetup and the state of the device.
        ////        Guess it should have at least a more precise name.
        public override void AddStandardControls(ControlSetup setup)
        {
            leftStickLeft = (ButtonControl)setup.AddControl(CommonControls.LeftStickLeft);
            leftStickRight = (ButtonControl)setup.AddControl(CommonControls.LeftStickRight);
            leftStickDown = (ButtonControl)setup.AddControl(CommonControls.LeftStickDown);
            leftStickUp = (ButtonControl)setup.AddControl(CommonControls.LeftStickUp);
            leftStickX = (AxisControl)setup.AddControl(CommonControls.LeftStickX);
            leftStickY = (AxisControl)setup.AddControl(CommonControls.LeftStickY);
            leftStick = (Vector2Control)setup.AddControl(CommonControls.LeftStick);
            leftStickButton = (ButtonControl)setup.AddControl(CommonControls.LeftStickButton);

            rightStickLeft = (ButtonControl)setup.AddControl(CommonControls.RightStickLeft);
            rightStickRight = (ButtonControl)setup.AddControl(CommonControls.RightStickRight);
            rightStickDown = (ButtonControl)setup.AddControl(CommonControls.RightStickDown);
            rightStickUp = (ButtonControl)setup.AddControl(CommonControls.RightStickUp);
            rightStickX = (AxisControl)setup.AddControl(CommonControls.RightStickX);
            rightStickY = (AxisControl)setup.AddControl(CommonControls.RightStickY);
            rightStick = (Vector2Control)setup.AddControl(CommonControls.RightStick);
            rightStickButton = (ButtonControl)setup.AddControl(CommonControls.RightStickButton);

            dPadLeft = (ButtonControl)setup.AddControl(CommonControls.DPadLeft);
            dPadRight = (ButtonControl)setup.AddControl(CommonControls.DPadRight);
            dPadDown = (ButtonControl)setup.AddControl(CommonControls.DPadDown);
            dPadUp = (ButtonControl)setup.AddControl(CommonControls.DPadUp);
            dPadX = (AxisControl)setup.AddControl(CommonControls.DPadX);
            dPadY = (AxisControl)setup.AddControl(CommonControls.DPadY);
            dPad = (Vector2Control)setup.AddControl(CommonControls.DPad);

            action1 = (ButtonControl)setup.AddControl(CommonControls.Action1);
            action2 = (ButtonControl)setup.AddControl(CommonControls.Action2);
            action3 = (ButtonControl)setup.AddControl(CommonControls.Action3);
            action4 = (ButtonControl)setup.AddControl(CommonControls.Action4);

            leftTrigger = (ButtonControl)setup.AddControl(CommonControls.LeftTrigger);
            rightTrigger = (ButtonControl)setup.AddControl(CommonControls.RightTrigger);

            leftBumper = (ButtonControl)setup.AddControl(CommonControls.LeftBumper);
            rightBumper = (ButtonControl)setup.AddControl(CommonControls.RightBumper);

            leftVibration = (AxisOutput)setup.AddControl(
                    SupportedControl.Get<AxisOutput>("Left Vibration"),
                    new AxisOutput("Left Vibration"));
            rightVibration = (AxisOutput)setup.AddControl(
                    SupportedControl.Get<AxisOutput>("Right Vibration"),
                    new AxisOutput("Right Vibration"));
        }

        public override void PostProcessState(InputState state)
        {
            // Right now all dead zones come from the default dead zones on the device profile.
            // Maybe in the future copy deadzone over ti fields on Gamepad,
            // either a single field or per control, and make it possible to change through API.
            var gamepadProfile = (GamepadProfile)profile;
            Range deadZones;
            if (gamepadProfile != null)
                deadZones = gamepadProfile.defaultDeadZones;
            else
                deadZones = Range.positive;

            ControlHelper.BuildXYCircularInputBasedOnAxisInput(
                (ButtonControl)state.controls[leftStickLeft.index],
                (ButtonControl)state.controls[leftStickRight.index],
                (ButtonControl)state.controls[leftStickDown.index],
                (ButtonControl)state.controls[leftStickUp.index],
                (AxisControl)state.controls[leftStickX.index],
                (AxisControl)state.controls[leftStickY.index],
                (Vector2Control)state.controls[leftStick.index],
                deadZones);

            ControlHelper.BuildXYCircularInputBasedOnAxisInput(
                (ButtonControl)state.controls[rightStickLeft.index],
                (ButtonControl)state.controls[rightStickRight.index],
                (ButtonControl)state.controls[rightStickDown.index],
                (ButtonControl)state.controls[rightStickUp.index],
                (AxisControl)state.controls[rightStickX.index],
                (AxisControl)state.controls[rightStickY.index],
                (Vector2Control)state.controls[rightStick.index],
                deadZones);

            ControlHelper.BuildXYSquareInputBasedOnAxisInput(
                (ButtonControl)state.controls[dPadLeft.index],
                (ButtonControl)state.controls[dPadRight.index],
                (ButtonControl)state.controls[dPadDown.index],
                (ButtonControl)state.controls[dPadUp.index],
                (AxisControl)state.controls[dPadX.index],
                (AxisControl)state.controls[dPadY.index],
                (Vector2Control)state.controls[dPad.index],
                deadZones);

            ((ButtonControl)state.controls[leftTrigger.index]).value =
                ControlHelper.GetDeadZoneAdjustedValue(((ButtonControl)state.controls[leftTrigger.index]).value, deadZones);

            ((ButtonControl)state.controls[rightTrigger.index]).value =
                ControlHelper.GetDeadZoneAdjustedValue(((ButtonControl)state.controls[rightTrigger.index]).value, deadZones);
        }

        public override void PostProcessEnabledControls(InputState state)
        {
            ControlHelper.SetAxisControlsEnabledBasedOnXY(
                state.controls[leftStickLeft.index],
                state.controls[leftStickRight.index],
                state.controls[leftStickDown.index],
                state.controls[leftStickUp.index],
                state.controls[leftStickX.index],
                state.controls[leftStickY.index],
                state.controls[leftStick.index]);

            ControlHelper.SetAxisControlsEnabledBasedOnXY(
                state.controls[rightStickLeft.index],
                state.controls[rightStickRight.index],
                state.controls[rightStickDown.index],
                state.controls[rightStickUp.index],
                state.controls[rightStickX.index],
                state.controls[rightStickY.index],
                state.controls[rightStick.index]);

            ControlHelper.SetAxisControlsEnabledBasedOnXY(
                state.controls[dPadLeft.index],
                state.controls[dPadRight.index],
                state.controls[dPadDown.index],
                state.controls[dPadUp.index],
                state.controls[dPadX.index],
                state.controls[dPadY.index],
                state.controls[dPad.index]);
        }

        public ButtonControl leftStickLeft { get; private set; }
        public ButtonControl leftStickRight { get; private set; }
        public ButtonControl leftStickDown { get; private set; }
        public ButtonControl leftStickUp { get; private set; }
        public AxisControl leftStickX { get; private set; }
        public AxisControl leftStickY { get; private set; }
        public Vector2Control leftStick { get; private set; }
        public ButtonControl leftStickButton { get; private set; }

        public ButtonControl rightStickLeft { get; private set; }
        public ButtonControl rightStickRight { get; private set; }
        public ButtonControl rightStickDown { get; private set; }
        public ButtonControl rightStickUp { get; private set; }
        public AxisControl rightStickX { get; private set; }
        public AxisControl rightStickY { get; private set; }
        public Vector2Control rightStick { get; private set; }
        public ButtonControl rightStickButton { get; private set; }

        public ButtonControl dPadLeft { get; private set; }
        public ButtonControl dPadRight { get; private set; }
        public ButtonControl dPadDown { get; private set; }
        public ButtonControl dPadUp { get; private set; }
        public AxisControl dPadX { get; private set; }
        public AxisControl dPadY { get; private set; }
        public Vector2Control dPad { get; private set; }

        public ButtonControl action1 { get; private set; }
        public ButtonControl action2 { get; private set; }
        public ButtonControl action3 { get; private set; }
        public ButtonControl action4 { get; private set; }

        public ButtonControl leftTrigger { get; private set; }
        public ButtonControl rightTrigger { get; private set; }

        public ButtonControl leftBumper { get; private set; }
        public ButtonControl rightBumper { get; private set; }

        public AxisOutput leftVibration { get; private set; }
        public AxisOutput rightVibration { get; private set; }
    }
}
