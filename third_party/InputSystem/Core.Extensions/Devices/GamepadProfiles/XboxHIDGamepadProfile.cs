#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#define IS_WINDOWS
#endif

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngineInternal.Input;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR || UNITY_STANDALONE

namespace UnityEngine.Experimental.Input
{
    // Xbox One and Xbox 360 gamepad interfacing through HID.
    // Unfortunately, the Xbox controllers aren't really HIDs but rather rely on driver support to
    // act as HIDs. Most importantly that means the HID descriptor of the gamepad is *not* coming from
    // the device but rather from the driver -- which means that Microsoft's drivers and third-party
    // drivers on other platforms can and will differ.
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class XboxHIDGamepadProfile : GamepadProfile
    {
        static XboxHIDGamepadProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<XboxHIDGamepadProfile>();
        }

        public XboxHIDGamepadProfile()
        {
            name = "Xbox Controller";
            matchingDeviceRegexes = new List<string>()
            {
                // Windows (XInput devices rely on a common product format of 'Product:[Controller(...)]')
                "^(?=.*product:(?=.*Controller \\(.*\\)))(?=.*interface:.*\\[HID\\])(?=.*type:.*HID.*Page:0x1.*Id:0x5).*$",
                // OSX
                "^(?=.*product:(?=.*Controller)(?=.*Xbox))(?=.*interface:.*\\[HID\\])(?=.*type:.*HID.*Page:0x1.*Id:0x5).*$",
            };

            ControlSetup setup = GetControlSetup(new Gamepad());

            // Setup mapping.
            setup.SplitMapping(0x010030, CommonControls.LeftStickLeft, CommonControls.LeftStickRight);
            setup.SplitMapping(0x010031, CommonControls.LeftStickUp, CommonControls.LeftStickDown);

            setup.SplitMapping(0x010033, CommonControls.RightStickLeft, CommonControls.RightStickRight);
            setup.SplitMapping(0x010034, CommonControls.RightStickUp, CommonControls.RightStickDown);

            setup.Mapping(0x090001, CommonControls.Action1);
            setup.Mapping(0x090002, CommonControls.Action2);
            setup.Mapping(0x090003, CommonControls.Action3);
            setup.Mapping(0x090004, CommonControls.Action4);

            setup.Mapping(0x090005, CommonControls.LeftBumper);
            setup.Mapping(0x090006, CommonControls.RightBumper);

#if IS_WINDOWS
            // Triggers are combined into a single [-1..1] range. Left is positive, right is negative.
            // At the USB level, the controller properly splits the triggers. XInput is picking it up from there.
            // Unfortunately, the MS HID driver for Xbox controllers combines them.
            setup.SplitMapping(0x010032, CommonControls.RightTrigger, CommonControls.LeftTrigger);

            setup.Mapping(0x090009, CommonControls.LeftStickButton);
            setup.Mapping(0x09000A, CommonControls.RightStickButton);

            setup.Mapping(0x090007, CommonControls.Back);
            setup.Mapping(0x090008, CommonControls.Start);

            // The dpad is done as a HID hatswitch.  The Xbox Hat Switch data is 1-based as it's starting value
            setup.HatMapping(0x010039, CommonControls.DPadLeft, CommonControls.DPadRight, CommonControls.DPadDown, CommonControls.DPadUp, 1);
#else
            setup.Mapping(0x010032, CommonControls.LeftTrigger, Range.full, Range.positive);
            setup.Mapping(0x010035, CommonControls.RightTrigger, Range.full, Range.positive);

            setup.Mapping(0x090007, CommonControls.LeftStickButton);
            setup.Mapping(0x090008, CommonControls.RightStickButton);

            setup.Mapping(0x09000A, CommonControls.Back);
            setup.Mapping(0x090009, CommonControls.Start);

            setup.Mapping(0x09000C, CommonControls.DPadUp, Range.full, Range.positive);
            setup.Mapping(0x09000D, CommonControls.DPadDown, Range.full, Range.positive);
            setup.Mapping(0x09000E, CommonControls.DPadLeft, Range.full, Range.positive);
            setup.Mapping(0x09000F, CommonControls.DPadRight, Range.full, Range.positive);
#endif

            mappings = setup.FinishMappings();

            // Haptics right now only works on Windows, but we intend to extend that to OSX/Linux in the near term.
#if IS_WINDOWS
            hapticsProcessor = new XboxHIDHapticsProcessor(setup.GetControl(SupportedControl.Get<AxisOutput>("Left Vibration")).index,
                    setup.GetControl(SupportedControl.Get<AxisOutput>("Right Vibration")).index,
                    setup.GetControl(SupportedControl.Get<AxisOutput>("Left Trigger Vibration")).index,
                    setup.GetControl(SupportedControl.Get<AxisOutput>("Right Trigger Vibration")).index);
#endif
        }

        public override ControlSetup GetControlSetup(InputDevice device)
        {
            ControlSetup setup = new ControlSetup(device);

            setup.AddControl(CommonControls.Back);
            setup.AddControl(CommonControls.Start);

            // Add the two additional motors on the triggers.
            var leftTriggerMotor = new AxisOutput("Left Trigger Vibration");
            var rightTriggerMotor = new AxisOutput("Right Trigger Vibration");
            setup.AddControl(SupportedControl.Get<AxisOutput>("Left Trigger Vibration"), leftTriggerMotor);
            setup.AddControl(SupportedControl.Get<AxisOutput>("Right Trigger Vibration"), rightTriggerMotor);

            // Section for control name overrides.
            setup.GetControl(CommonControls.Action1).name = "A";
            setup.GetControl(CommonControls.Action2).name = "B";
            setup.GetControl(CommonControls.Action3).name = "X";
            setup.GetControl(CommonControls.Action4).name = "Y";

            return setup;
        }
    }

#if IS_WINDOWS
    class XboxHIDHapticsProcessor : IHapticsProcessor
    {
        int m_LowFrequencyMotorIndex;
        int m_HighFrequencyMotorIndex;
        int m_LeftTriggerMotorIndex;
        int m_RightTriggerMotorIndex;

        MemoryStream outputStream = new MemoryStream();

        public XboxHIDHapticsProcessor(int lowFrequencyMotorIndex, int highFrequencyMotorIndex, int leftTriggerMotorIndex, int rightTriggerMotorIndex)
        {
            this.m_LowFrequencyMotorIndex = lowFrequencyMotorIndex;
            this.m_HighFrequencyMotorIndex = highFrequencyMotorIndex;
            this.m_LeftTriggerMotorIndex = leftTriggerMotorIndex;
            this.m_RightTriggerMotorIndex = rightTriggerMotorIndex;
        }

        static byte GetScaledAndClampedMotorSpeed(float motorSpeed, byte maxMotorSpeed)
        {
            return (byte)Mathf.Clamp(motorSpeed * maxMotorSpeed, 0, maxMotorSpeed);
        }

        public void Process(InputDevice inputDevice)
        {
            OutputControl<float> lfMotorControl = inputDevice.GetControl(m_LowFrequencyMotorIndex) as OutputControl<float>;
            OutputControl<float> hfMotorControl = inputDevice.GetControl(m_HighFrequencyMotorIndex) as OutputControl<float>;
            OutputControl<float> ltMotorControl = inputDevice.GetControl(m_LeftTriggerMotorIndex) as OutputControl<float>;
            OutputControl<float> rtMotorControl = inputDevice.GetControl(m_RightTriggerMotorIndex) as OutputControl<float>;
            if ((lfMotorControl != null && lfMotorControl.changedValue) ||
                (hfMotorControl != null && hfMotorControl.changedValue) ||
                (ltMotorControl != null && ltMotorControl.changedValue) ||
                (rtMotorControl != null && rtMotorControl.changedValue))
            {
                float lfMotorSpeed = lfMotorControl != null ? lfMotorControl.value : 0f;
                float hfMotorSpeed = hfMotorControl != null ? hfMotorControl.value : 0f;
                float ltMotorSpeed = ltMotorControl != null ? ltMotorControl.value : 0f;
                float rtMotorSpeed = rtMotorControl != null ? rtMotorControl.value : 0f;

                BinaryWriter writer = new BinaryWriter(outputStream);
                {
                    writer.Write((int)0); // Report Id

                    HidOutputHelpers.WriteOutputHeader(writer, 0x97, 0xF, 1, 1);
                    byte motorsEnabled = (byte)(((ltMotorSpeed != 0f) ? 1 : 0) |
                                                (((rtMotorSpeed != 0f) ? 1 : 0) << 1) |
                                                (((lfMotorSpeed != 0f) ? 1 : 0) << 2) |
                                                (((hfMotorSpeed != 0f) ? 1 : 0) << 3));
                    writer.Write(motorsEnabled);

                    const int kMaxMotorSpeed = 100;

                    HidOutputHelpers.WriteOutputHeader(writer, 0x70, 0xF, 4, 1);
                    writer.Write(GetScaledAndClampedMotorSpeed(ltMotorSpeed, kMaxMotorSpeed));
                    writer.Write(GetScaledAndClampedMotorSpeed(rtMotorSpeed, kMaxMotorSpeed));
                    writer.Write(GetScaledAndClampedMotorSpeed(lfMotorSpeed, kMaxMotorSpeed));
                    writer.Write(GetScaledAndClampedMotorSpeed(hfMotorSpeed, kMaxMotorSpeed));

                    HidOutputHelpers.WriteOutputHeader(writer, 0x50, 0xF, 1, 1);
                    writer.Write((byte)255);

                    HidOutputHelpers.WriteOutputHeader(writer, 0xA7, 0xF, 1, 1);
                    writer.Write((byte)0);
                }

                //var buffer = outputStream.GetBuffer();
                //NativeInputSystem.SendOutput(inputDevice.nativeId, buffer, (int)outputStream.Position);
                outputStream.Position = 0;
            }
        }
    }
#endif
}

#endif
