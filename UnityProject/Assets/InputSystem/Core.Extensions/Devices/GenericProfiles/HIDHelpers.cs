using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngineInternal.Input;

namespace UnityEngine.Experimental.Input
{
    // Synced to HIDInputElementDescriptor (unity\Modules\Input\InputHIDUtilities.h).
    [System.Serializable]
    public class HIDElementDescriptor
    {
        public HIDElementDescriptor()
        {}

        public HIDElementDescriptor(int id, string name, string type, int usageID, int usagePageID)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.usageID = usageID;
            this.usagePageID = usagePageID;
        }

        public int id;
        public string name;
        public string type;
        public int usageID;
        public int usagePageID;
        public int logicalMin;
        public int logicalMax;
        public string reportType;
        public string reportID;
        public string reportCount;
        public string reportSizeInBits;
    }

    // Synced to HIDInputDeviceDescriptor (unity\Modules\Input\InputHIDUtilities.h)
    public class HIDDescriptor
    {
        public int usageID;
        public int usagePageID;
        public HIDElementDescriptor[] elements;
    }

    public static class HIDHelpers
    {
        const int kMaxButtons = 32;

        // This function needs to perform the same operation as 'int HIDMakeControlId(int usagePage, int usage)' from (unity\Modules\Input\InputHIDUtilities.h)
        public static int GetControlId(int pageId, int usageId)
        {
            return (usageId & 0xFFFF) | ((pageId & 0xFFFF) << 16);
        }

        public static bool IsDefinedHIDUsage(HIDElementDescriptor hidElement)
        {
            bool isKnown = false;
            if (PageId.IsDefined(typeof(PageId), hidElement.usagePageID))
            {
                switch ((PageId)hidElement.usagePageID)
                {
                    case PageId.GenericDesktopPage:
                    {
                        isKnown = GenericDesktopUsage.IsDefined(typeof(GenericDesktopUsage), hidElement.usageID);
                    }
                    break;
                    case PageId.SimulationPage:
                    {
                        isKnown = SimulationUsage.IsDefined(typeof(SimulationUsage), hidElement.usageID);
                    }
                    break;
                    case PageId.ButtonPage:
                    {
                        isKnown = hidElement.usageID > 0 && hidElement.usageID <= kMaxButtons;
                    }
                    break;
                }
            }
            return isKnown;
        }

        public static void AddHIDControl(ControlSetup controlSetup, HIDElementDescriptor hidElement)
        {
            switch (hidElement.reportType)
            {
                case "Input":
                {
                    AddInputControl(controlSetup, hidElement);
                }
                break;
                case "Output":
                case "Feature":
                    // Once we are ready to support these, control creation should be implemented here
                    break;
            }
        }

        static void AddInputControl(ControlSetup controlSetup, HIDElementDescriptor hidElement)
        {
            int usageId = hidElement.usageID;
            int usagePageId = hidElement.usagePageID;
            string usageName = HIDHelpers.GetUsageName(usagePageId, usageId);

            switch (hidElement.type)
            {
                case "Button":
                {
                    SupportedControl buttonControl = SupportedControl.Get<ButtonControl>(usageName);
                    controlSetup.AddControl(buttonControl);
                    controlSetup.Mapping(hidElement.id, buttonControl);
                }
                break;
                case "Axis":
                case "Misc": // OSX has a tendency to label axes as Misc from native
                {
                    if (usageId == (int)GenericDesktopUsage.HatSwitch && usagePageId == (int)PageId.GenericDesktopPage)
                    {
                        SupportedControl upControl = SupportedControl.Get<ButtonControl>(usageName + " Up");
                        SupportedControl rightControl = SupportedControl.Get<ButtonControl>(usageName + " Right");
                        SupportedControl downControl = SupportedControl.Get<ButtonControl>(usageName + " Down");
                        SupportedControl leftControl = SupportedControl.Get<ButtonControl>(usageName + " Left");
                        controlSetup.AddControl(upControl);
                        controlSetup.AddControl(downControl);
                        controlSetup.AddControl(leftControl);
                        controlSetup.AddControl(rightControl);

                        int startingIndex = hidElement.logicalMin;
                        controlSetup.HatMapping(hidElement.id, leftControl, rightControl, downControl, upControl, startingIndex);
                    }
                    else
                    {
                        SupportedControl axisControl = SupportedControl.Get<AxisControl>(usageName);
                        controlSetup.AddControl(axisControl);
                        controlSetup.Mapping(hidElement.id, axisControl);
                    }
                }
                break;
                default:
                    break;
            }
        }

        public static void AddDefaultControls(ControlSetup controlSetup)
        {
            System.Array usagesArray = GenericDesktopUsage.GetValues(typeof(GenericDesktopUsage));
            for (int i = 0; i < usagesArray.Length; i++)
            {
                GenericDesktopUsage usageId = (GenericDesktopUsage)usagesArray.GetValue(i);
                string usageType = GetGenericDesktopUsageType(usageId);
                int controlId = GetControlId((int)PageId.GenericDesktopPage, (int)usageId);
                string usageName = GetUsageName((int)PageId.GenericDesktopPage, (int)usageId);
                AddHIDControl(controlSetup, new HIDElementDescriptor(controlId, usageName, usageType, (int)usageId, (int)PageId.GenericDesktopPage));
            }

            usagesArray = SimulationUsage.GetValues(typeof(SimulationUsage));
            for (int i = 0; i < usagesArray.Length; i++)
            {
                SimulationUsage usageId = (SimulationUsage)usagesArray.GetValue(i);
                string usageType = GetSimulationUsageType(usageId);
                int controlId = GetControlId((int)PageId.SimulationPage, (int)usageId);
                string usageName = GetUsageName((int)PageId.SimulationPage, (int)usageId);
                AddHIDControl(controlSetup, new HIDElementDescriptor(controlId, usageName, usageType, (int)usageId, (int)PageId.SimulationPage));
            }

            // Usages are 1 based
            for (int i = 1; i <= kMaxButtons; i++)
            {
                string usageType = "Button";
                int controlId = GetControlId((int)PageId.ButtonPage, i);
                string usageName = GetUsageName((int)PageId.ButtonPage, i);
                AddHIDControl(controlSetup, new HIDElementDescriptor(controlId, usageName, usageType, i, (int)PageId.ButtonPage));
            }
        }

        static string GetUsageName(int pageId, int usageId)
        {
            string usageName = string.Format("Unknown[{0}][{1}]", pageId, usageId);
            switch ((PageId)pageId)
            {
                case PageId.GenericDesktopPage:
                {
                    if (GenericDesktopUsage.IsDefined(typeof(GenericDesktopUsage), usageId))
                    {
                        usageName = ((GenericDesktopUsage)usageId).ToString();
                    }
                }
                break;
                case PageId.SimulationPage:
                {
                    if (SimulationUsage.IsDefined(typeof(SimulationUsage), usageId))
                    {
                        usageName = ((SimulationUsage)usageId).ToString();
                    }
                }
                break;
                case PageId.ButtonPage:
                {
                    usageName = string.Format("Button {0}", usageId);
                }
                break;
                default:
                    break;
            }
            return usageName;
        }

        // All usages retrieved from http://www.usb.org/developers/hidpage/Hut1_12v2.pdf
        enum PageId
        {
            GenericDesktopPage = 0x01,
            SimulationPage = 0x02,
            ButtonPage = 0x09,
        }

        // If you create a new entry here, don't forget to update GetGenericDesktopUsageType for the full HID inputs info
        enum GenericDesktopUsage
        {
            X = 0x30,
            Y = 0x31,
            Z = 0x32,
            Rx = 0x33,
            Ry = 0x34,
            Rz = 0x35,
            Slider = 0x36,
            Dial = 0x37,
            Wheel = 0x38,
            HatSwitch = 0x39,
            Start = 0x3d,
            Select = 0x3e,
            SystemMenuContext = 0x84,
            SystemMenuMain = 0x85,
            SystemMenuApp = 0x86,
            SystemMenuHelp = 0x87,
            SystemMenuExit = 0x88,
            SystemMenuSelect = 0x89,
            SystemMenuRight = 0x8a,
            SystemMenuLeft = 0x8b,
            SystemMenuUp = 0x8c,
            SystemMenuDown = 0x8d,
            DPadUp = 0x90,
            DPadDown = 0x91,
            DPadRight = 0x92,
            DPadLeft = 0x93,
        }

        static string GetGenericDesktopUsageType(GenericDesktopUsage usageId)
        {
            string usageType;
            switch (usageId)
            {
                case GenericDesktopUsage.Start:
                case GenericDesktopUsage.Select:
                case GenericDesktopUsage.SystemMenuContext:
                case GenericDesktopUsage.SystemMenuMain:
                case GenericDesktopUsage.SystemMenuApp:
                case GenericDesktopUsage.SystemMenuHelp:
                case GenericDesktopUsage.SystemMenuExit:
                case GenericDesktopUsage.SystemMenuSelect:
                case GenericDesktopUsage.SystemMenuRight:
                case GenericDesktopUsage.SystemMenuLeft:
                case GenericDesktopUsage.SystemMenuUp:
                case GenericDesktopUsage.SystemMenuDown:
                case GenericDesktopUsage.DPadUp:
                case GenericDesktopUsage.DPadDown:
                case GenericDesktopUsage.DPadRight:
                case GenericDesktopUsage.DPadLeft:
                    usageType = "Button";
                    break;
                case GenericDesktopUsage.X:
                case GenericDesktopUsage.Y:
                case GenericDesktopUsage.Z:
                case GenericDesktopUsage.Rx:
                case GenericDesktopUsage.Ry:
                case GenericDesktopUsage.Rz:
                case GenericDesktopUsage.Slider:
                case GenericDesktopUsage.Dial:
                case GenericDesktopUsage.Wheel:
                case GenericDesktopUsage.HatSwitch:
                default:
                    usageType = "Axis";
                    break;
            }
            return usageType;
        }

        // If you create a new entry here, don't forget to update GetSimulationUsageType for the full HID inputs info
        enum SimulationUsage
        {
            Aileron = 0xb0,
            AileronTrim = 0xb1,
            AntiTorqueControl = 0xb2,
            AutopilotEnable = 0xb3,
            ChaffRelease = 0xb4,
            CollectiveControl = 0xb5,
            DiveBrake = 0xb6,
            ElectronicCountermeasures = 0xb7,
            Elevator = 0xb8,
            ElevatorTrim = 0xb9,
            Rudder = 0xba,
            Throttle = 0xbb,
            FlightCommunications = 0xbc,
            FlareRelease = 0xbd,
            LandingGear = 0xbe,
            ToeBrake = 0xbf,
            Trigger = 0xc0,
            WeaponsArm = 0xc1,
            WeaponsSelect = 0xc2,
            WingFlaps = 0xc3,
            Accelerator = 0xc4,
            Brake = 0xc5,
            Clutch = 0xc6,
            Shifter = 0xc7,
            Steering = 0xc8,
            TurretDirection = 0xc9,
            BarrelElevation = 0xca,
            DivePlane = 0xcb,
            Ballast = 0xcc,
            BicycleCrank = 0xcd,
            HandleBars = 0xce,
            FrontBrake = 0xcf,
            RearBrake = 0xd0,
        }

        static string GetSimulationUsageType(SimulationUsage usageId)
        {
            string usageType;
            switch (usageId)
            {
                case SimulationUsage.AutopilotEnable:
                case SimulationUsage.ChaffRelease:
                case SimulationUsage.ElectronicCountermeasures:
                case SimulationUsage.FlightCommunications:
                case SimulationUsage.FlareRelease:
                case SimulationUsage.LandingGear:
                case SimulationUsage.Trigger:
                case SimulationUsage.WeaponsArm:
                case SimulationUsage.WeaponsSelect:
                    usageType = "Button";
                    break;
                case SimulationUsage.Aileron:
                case SimulationUsage.AileronTrim:
                case SimulationUsage.AntiTorqueControl:
                case SimulationUsage.CollectiveControl:
                case SimulationUsage.DiveBrake:
                case SimulationUsage.Elevator:
                case SimulationUsage.ElevatorTrim:
                case SimulationUsage.Rudder:
                case SimulationUsage.Throttle:
                case SimulationUsage.ToeBrake:
                case SimulationUsage.WingFlaps:
                case SimulationUsage.Accelerator:
                case SimulationUsage.Brake:
                case SimulationUsage.Clutch:
                case SimulationUsage.Shifter:
                case SimulationUsage.Steering:
                case SimulationUsage.TurretDirection:
                case SimulationUsage.BarrelElevation:
                case SimulationUsage.DivePlane:
                case SimulationUsage.Ballast:
                case SimulationUsage.BicycleCrank:
                case SimulationUsage.HandleBars:
                case SimulationUsage.FrontBrake:
                case SimulationUsage.RearBrake:
                default:
                    usageType = "Axis";
                    break;
            }
            return usageType;
        }
    }

    public static class HidOutputHelpers
    {
        public static void WriteOutputHeader(BinaryWriter writer, int usageId, int usagePageId, int length, int sizeInBytes)
        {
            writer.Write(HIDHelpers.GetControlId(usagePageId, usageId));
            writer.Write(length);
            writer.Write(sizeInBytes);
        }
    }
}
