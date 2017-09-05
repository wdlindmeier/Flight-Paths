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
    public class OculusRiftHMDProfile : GenericHMDProfile
    {
        static OculusRiftHMDProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<OculusRiftHMDProfile>();
        }

        public OculusRiftHMDProfile()
        {
            name = "Oculus Rift HMD";
            matchingDeviceRegexes = new List<string>()
            {
                ".*manufacturer:.*Oculus.*HMD.*"
            };
            lastResortDeviceRegex = null; // Clear from GenericHMDProfile.
            neverMatchDeviceRegex = null; // Clear from GenericHMDProfile.

            var setup = GetControlSetup(new HeadMountedDisplay());
            mappings = setup.FinishMappings();
        }
    }
}
