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
    public class OpenVRControllerProfile : GenericVRControllerProfile
    {
        public Range defaultDeadZones = new Range(0.2f, 0.9f);

        static OpenVRControllerProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<OpenVRControllerProfile>();
        }

        public OpenVRControllerProfile()
        {
            name = "OpenVR Controller";
            matchingDeviceRegexes = new List<string>()
            {
                "^(?=.*product:(?=.*OpenVR.*Controller))(?=.*interface:.*\\[VR\\])(?=.*type:.*Controller.*).*$"
            };
            lastResortDeviceRegex = null; // Clear from GenericVRControllerProfile.
            neverMatchDeviceRegex = null; // Clear from GenericVRControllerProfile.

            var setup = GetControlSetup(new OpenVRController());
            mappings = setup.FinishMappings();

            hapticsProcessor = new GenericHapticsProcessor(setup.GetControl(CommonControls.Vibration).index);
        }

        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            return new OpenVRController();
        }
    }
}
