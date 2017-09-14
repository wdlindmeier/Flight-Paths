#if UNITY_EDITOR
using UnityEditor;
#endif
using Assets.Utilities;
using UnityEngine.VR;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class OpenVRHMDProfile : GenericHMDProfile
    {
        static OpenVRHMDProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<OpenVRHMDProfile>();
        }

        public OpenVRHMDProfile()
        {
            name = "Open VR HMD";
            matchingDeviceRegexes = new List<string>()
            {
                ".*manufacturer:.*OpenVR.*HMD.*"
            };
            lastResortDeviceRegex = null; // Clear from GenericHMDProfile.
            neverMatchDeviceRegex = null; // Clear from GenericHMDProfile.

            var setup = GetControlSetup(new HeadMountedDisplay());
            mappings = setup.FinishMappings();
        }
    }
}
