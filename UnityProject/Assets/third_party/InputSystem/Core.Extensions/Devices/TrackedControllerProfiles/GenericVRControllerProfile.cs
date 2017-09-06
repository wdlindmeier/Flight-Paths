using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngineInternal.Input;

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GenericVRControllerProfile : InputDeviceProfile
    {
        public enum SendEventHeaderID
        {
            None = 0,
            EnqueueRumble = 1,
        }

        static GenericVRControllerProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<GenericVRControllerProfile>();
        }

        public GenericVRControllerProfile()
        {
            name = "VR Controller";
            lastResortDeviceRegex = "VR.*[Cc]ontroller";
        }

        public override bool Remap(InputEvent inputEvent)
        {
            var controlEvent = inputEvent as GenericControlEvent;
            if (controlEvent != null)
            {
                return base.Remap(inputEvent);
            }

            // Swallow any unrecognized events. This also gets rids of index 2
            // which combines left and right index trigger values into a single axis.
            var trackedEvent = inputEvent as TrackingEvent;
            return trackedEvent == null;
        }

        public override string DeriveTagFromDescriptor(string deviceDescriptor)
        {
            if (deviceDescriptor != null)
            {
                // The deviceDescriptor comes from native side and is not determined by our CommonDeviceTags strings
                // on managed side. Hence we cannot replace the string literals here with CommonDeviceTags constants.
                if (deviceDescriptor.IndexOf("Left", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    return CommonDeviceTags.Left;
                }
                else if (deviceDescriptor.IndexOf("Right", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    return CommonDeviceTags.Right;
                }
            }
            return CommonDeviceTags.None;
        }

        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            return new TrackedController();
        }

        public class GenericHapticsProcessor : IHapticsProcessor
        {
            int m_RumbleIndex;
            MemoryStream m_OutputStream = new MemoryStream();
            BinaryWriter m_Writer;

            public GenericHapticsProcessor(int rumbleIndex)
            {
                m_RumbleIndex = rumbleIndex;
                m_Writer = new BinaryWriter(m_OutputStream);
            }

            public void Process(InputDevice inputDevice)
            {
                var rumble = inputDevice.GetControl(m_RumbleIndex) as OutputControl<float>;

                if (rumble != null)
                {
                    m_Writer.Write((int)SendEventHeaderID.EnqueueRumble); // header
                    m_Writer.Write(rumble.value); // intensity
                    m_Writer.Write((int)0);       // channel
                }

                //var buffer = m_OutputStream.GetBuffer();
                //NativeInputSystem.SendOutput(inputDevice.nativeId, buffer, (int)m_OutputStream.Position);
                m_OutputStream.Position = 0;
            }
        }
    }
}
