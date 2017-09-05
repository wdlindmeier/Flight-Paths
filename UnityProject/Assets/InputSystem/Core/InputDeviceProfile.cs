using System;
using System.Collections.Generic;
using Assets.Utilities;

namespace UnityEngine.Experimental.Input
{
    public interface IHapticsProcessor
    {
        void Process(InputDevice inputDevice);
    }

    public abstract class InputDeviceProfile
    {
        protected internal List<string> matchingDevices;
        protected internal List<string> matchingDeviceRegexes;
        protected internal string lastResortDeviceRegex;
        protected internal string neverMatchDeviceRegex { get; protected set; }

        protected IHapticsProcessor hapticsProcessor;

        ////REVIEW: do these still make sense or are the #ifs good enough?
        public Version minUnityVersion;
        public Version maxUnityVersion;

        public string name { get; protected set; }

        ////REVIEW: not good to expose internal state writable like this
        // Control indices coming in may be all over the place so instead of using an array, we use
        // a dictionary lookup. After remapping, indices are nice and contiguous.
        public Dictionary<int, IControlMapping> mappings;

        ////TODO: instead of bools, we should turn that into enums (same for ProcessEventIntoState); it's hard to remember otherwise what true/false means
        public virtual bool Remap(InputEvent inputEvent)
        {
            var controlEvent = inputEvent as GenericControlEvent;

            // Let other event types pass through.
            if (controlEvent == null)
                return false;

            IControlMapping mapping;
            if (mappings != null && mappings.TryGetValue(controlEvent.controlIndex, out mapping))
                return mapping.Remap(controlEvent);

            // Ignore generic control events that are not handled by mapping.
            return true;
        }

        // NOTE: This method is allowed to return null which means the profile opts
        //       for the device to be ignored.
        public abstract InputDevice TryCreateDevice(string deviceDescriptor);

        public virtual ControlSetup GetControlSetup(InputDevice device)
        {
            return new ControlSetup(device);
        }

        public virtual void ProcessHaptics(InputDevice device)
        {
            if (hapticsProcessor != null)
            {
                hapticsProcessor.Process(device);
            }
        }

        public virtual string DeriveTagFromDescriptor(string deviceDescriptor)
        {
            return CommonDeviceTags.None;
        }
    }
}
