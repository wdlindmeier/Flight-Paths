using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GenericStylusProfile : InputDeviceProfile
    {
        static GenericStylusProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<GenericStylusProfile>();
        }

        public GenericStylusProfile()
        {
            name = "Stylus";

            // This profile isn't very specific so we put us in the fallback position in case
            // someone wants to match a specific pen by device name.
            lastResortDeviceRegex = "type:\\[(Stylus|Pen)\\]";
            neverMatchDeviceRegex = "interface:\\[HID\\]"; // Don't handle pen as HID devices.
        }

        public override InputDevice TryCreateDevice(string deviceString)
        {
            return new Stylus();
        }

        public override bool Remap(InputEvent inputEvent)
        {
            // Don't filter events for generic devices.
            return false;
        }
    }
}
