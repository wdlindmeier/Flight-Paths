using System.Collections.Generic;
using Assets.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class OculusRemoteProfile : InputDeviceProfile
    {
        public Range defaultDeadZones = new Range(0.2f, 0.9f);

        static OculusRemoteProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<OculusRemoteProfile>();
        }

        public OculusRemoteProfile()
        {
            name = "Oculus Remote";
            matchingDeviceRegexes = new List<string>()
            {
                "Oculus.*Remote"
            };
            lastResortDeviceRegex = null; // Clear from GenericVRControllerProfile.
            neverMatchDeviceRegex = null; // Clear from GenericVRControllerProfile.

            var setup = GetControlSetup(new OculusRemote());
            mappings = setup.FinishMappings();
        }

        public override bool Remap(InputEvent inputEvent)
        {
            var controlEvent = inputEvent as GenericControlEvent<float>;
            if (controlEvent != null)
            {
                IControlMapping mapping;
                if (mappings != null && mappings.TryGetValue(controlEvent.controlIndex, out mapping))
                    return mapping.Remap(controlEvent);
            }

            return false;
        }

        public override InputDevice TryCreateDevice(string deviceString)
        {
            return new OculusRemote();
        }
    }
}
