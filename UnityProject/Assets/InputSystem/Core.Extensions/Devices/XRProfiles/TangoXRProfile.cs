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
    public class TangoXRProfile : GenericVRControllerProfile
    {
        static TangoXRProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<TangoXRProfile>();
        }

        public TangoXRProfile()
        {
            name = "Tango XR Device";
            matchingDeviceRegexes = new List<string>()
            {
                ".*Tango.*"
            };
            lastResortDeviceRegex = null; // Clear from GenericVRControllerProfile.
            neverMatchDeviceRegex = null; // Clear from GenericVRControllerProfile.

            var setup = GetControlSetup(new TangoXRDevice());
            mappings = setup.FinishMappings();
        }

        //REVIEW - seems redundant
        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            TangoXRDevice device = new TangoXRDevice();
            return device;
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

            // Swallow any unrecognized events. This also gets rids of index 2
            // which combines left and right index trigger values into a single axis.
            var trackedEvent = inputEvent as TrackingEvent;
            if (trackedEvent == null)
                return true;

            return false;
        }
    }
}
