using System.IO;
using System.Collections.Generic;
using UnityEngineInternal.Input;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR || UNITY_STANDALONE

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GenericHIDProfile : InputDeviceProfile
    {
        static GenericHIDProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<GenericHIDProfile>();
        }

        public GenericHIDProfile()
        {
            name = "Generic HID";
            matchingDeviceRegexes = new List<string>();
            lastResortDeviceRegex = "(interface:\\[(HID\\])).+(type:\\[(HID))";
            neverMatchDeviceRegex = "(type:\\[(Keyboard\\]))";
        }

        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            HIDDescriptor hidDescriptor = JsonUtility.FromJson<HIDDescriptor>(deviceDescriptor);
            if (IsDeviceValidAsGenericHID(hidDescriptor))
            {
                return AddGenericHID(hidDescriptor);
            }

            return null;
        }

        public override ControlSetup GetControlSetup(InputDevice device)
        {
            return new ControlSetup(device);
        }

        bool IsDeviceValidAsGenericHID(HIDDescriptor descriptor)
        {
            bool isValid = false;

            // The Descriptor needs at least one known element
            if (descriptor.elements != null && descriptor.elements.Length != 0)
            {
                for (int i = 0; i < descriptor.elements.Length; i++)
                {
                    if (HIDHelpers.IsDefinedHIDUsage(descriptor.elements[i]))
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            return isValid;
        }

        InputDevice AddGenericHID(HIDDescriptor hidDescriptor)
        {
            return new GenericHID(hidDescriptor);
        }
    }
}

#endif
