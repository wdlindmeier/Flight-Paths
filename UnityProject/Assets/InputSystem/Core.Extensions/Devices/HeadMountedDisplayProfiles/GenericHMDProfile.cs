#if UNITY_EDITOR
using UnityEditor;
#endif
using Assets.Utilities;
using UnityEngine.VR;

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GenericHMDProfile : InputDeviceProfile
    {
        static GenericHMDProfile()
        {
            Register();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InputSystem.RegisterDeviceProfile<GenericHMDProfile>();
        }

        public GenericHMDProfile()
        {
            name = "HMD";
            lastResortDeviceRegex = "HMD";

            GetControlSetup(new HeadMountedDisplay());
        }

        public override InputDevice TryCreateDevice(string deviceDescriptor)
        {
            return new HeadMountedDisplay();
        }

        public override bool Remap(InputEvent inputEvent)
        {
            var trackingEvent = inputEvent as TrackingEvent;
            if (trackingEvent != null)
            {
                switch (trackingEvent.nodeId)
                {
                    case (int)UnityEngine.XR.XRNode.LeftEye: trackingEvent.nodeId = (int)HeadMountedDisplay.Node.LeftEye; break;
                    case (int)UnityEngine.XR.XRNode.RightEye: trackingEvent.nodeId = (int)HeadMountedDisplay.Node.RightEye; break;
                    case (int)UnityEngine.XR.XRNode.CenterEye: trackingEvent.nodeId = (int)HeadMountedDisplay.Node.CenterEye; break;
                    case (int)UnityEngine.XR.XRNode.Head: trackingEvent.nodeId = (int)HeadMountedDisplay.Node.Head; break;
                }
            }
            return false;
        }
    }
}
