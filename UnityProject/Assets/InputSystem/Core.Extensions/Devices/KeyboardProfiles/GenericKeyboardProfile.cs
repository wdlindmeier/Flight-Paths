using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GenericKeyboardProfile : InputDeviceProfile
    {
        static GenericKeyboardProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<GenericKeyboardProfile>();
        }

        public GenericKeyboardProfile()
        {
            name = "Keyboard";

            // This profile isn't very specific so we put us in the fallback position in case
            // someone wants to match a specific keyboard by device name. We do, however, exclude
            // HID keyboards with this profile as we're generally only interested in the dedicated
            // keyboard support on each platform rather than the generic HID keyboard support.
            lastResortDeviceRegex = "type:\\[Keyboard\\]";
            neverMatchDeviceRegex = "interface:\\[HID\\]";
        }

        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            return new Keyboard();
        }

        public override bool Remap(InputEvent inputEvent)
        {
            // Don't filter events for generic devices.
            return false;
        }
    }
}
