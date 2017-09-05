using UnityEngine;
using Assets.Utilities;

namespace UnityEngine.Experimental.Input
{
    public abstract class GamepadProfile : InputDeviceProfile
    {
        public Range defaultDeadZones = new Range(0.2f, 0.9f);

        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            return new Gamepad();
        }
    }
}
