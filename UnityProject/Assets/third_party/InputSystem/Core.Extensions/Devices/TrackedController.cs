namespace UnityEngine.Experimental.Input
{
    // A combination of gamepad-style controls with motion tracking.
    // We mandate it has at least a "fire" button in addition to tracking.
    // Example: Oculus Touch, OpenVR controllers
    public class TrackedController : TrackedInputDevice
    {
        public override void AddStandardControls(ControlSetup setup)
        {
            base.AddStandardControls(setup);
            action1 = (ButtonControl)setup.AddControl(CommonControls.Action1);
            supportedTags.Add(CommonDeviceTags.Left);
            supportedTags.Add(CommonDeviceTags.Right);
        }

        ////TODO: implement speedier lookups rather than crawling through all devices looking for left and right
        public static TrackedController leftHand
        {
            get
            {
                for (int i = 0; i < InputSystem.devices.Count; i++)
                {
                    if (InputSystem.devices[i] is TrackedController && InputSystem.devices[i].tagIndex == 0)
                        return (TrackedController)InputSystem.devices[i];
                }
                return null;
            }
        }

        public static TrackedController rightHand
        {
            get
            {
                for (int i = 0; i < InputSystem.devices.Count; i++)
                {
                    if (InputSystem.devices[i] is TrackedController && InputSystem.devices[i].tagIndex == 1)
                        return (TrackedController)InputSystem.devices[i];
                }
                return null;
            }
        }

        public ButtonControl action1 { get; private set; }
    }
}
