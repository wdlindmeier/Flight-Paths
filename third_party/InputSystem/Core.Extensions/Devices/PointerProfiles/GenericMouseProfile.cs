using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GenericMouseProfile : InputDeviceProfile
    {
        static GenericMouseProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<GenericMouseProfile>();
        }

        public GenericMouseProfile()
        {
            name = "Mouse";

            // This profile isn't very specific so we put us in the fallback position in case
            // someone wants to match a specific keyboard by device name.
            lastResortDeviceRegex = "type:\\[Mouse\\]";
            neverMatchDeviceRegex = "interface:\\[HID\\]"; // Don't handle mice as HID devices.
        }

        public override InputDevice TryCreateDevice(string deviceString)
        {
            return new Mouse();
        }

        public override bool Remap(InputEvent inputEvent)
        {
            // Don't filter events for generic devices.
            return false;
        }
    }
}
