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
    public class OculusTouchProfile : GenericVRControllerProfile
    {
        public Range defaultDeadZones = new Range(0.2f, 0.9f);

        static OculusTouchProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<OculusTouchProfile>();
        }

        public OculusTouchProfile()
        {
            name = "Oculus Touch";
            matchingDeviceRegexes = new List<string>()
            {
                ".*Oculus.*Touch.*Controller.*"
            };
            lastResortDeviceRegex = null; // Clear from GenericVRControllerProfile.
            neverMatchDeviceRegex = null; // Clear from GenericVRControllerProfile.

            var setup = GetControlSetup(new OculusTouchController());
            mappings = setup.FinishMappings();

            hapticsProcessor = new GenericHapticsProcessor(setup.GetControl(CommonControls.Vibration).index);
        }

        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            return new OculusTouchController();
        }
    }
}
