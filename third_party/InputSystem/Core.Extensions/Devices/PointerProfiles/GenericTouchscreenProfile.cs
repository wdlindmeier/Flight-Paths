using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GenericTouchscreenProfile : InputDeviceProfile
    {
        static GenericTouchscreenProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<GenericTouchscreenProfile>();
        }

        public GenericTouchscreenProfile()
        {
            name = "Touchscreen";

            // This profile isn't very specific so we put us in the fallback position in case
            // someone wants to match a specific touchscreen by device name.
            lastResortDeviceRegex = "type:\\[Touch\\]";
            neverMatchDeviceRegex = "interface:\\[HID\\]"; // Don't handle touch as HID devices.
        }

        public override InputDevice TryCreateDevice(string deviceString)
        {
            return new Touchscreen();
        }

        public override bool Remap(InputEvent inputEvent)
        {
            // Don't filter events for generic devices.
            return false;
        }
    }
}
