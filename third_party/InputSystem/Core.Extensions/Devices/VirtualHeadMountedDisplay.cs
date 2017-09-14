using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.Experimental.Input
{
    public class VirtualHeadMountedDisplay : HeadMountedDisplay
    {
        public static new VirtualHeadMountedDisplay current { get { return InputSystem.GetCurrentDeviceOfType<VirtualHeadMountedDisplay>(); } }
    }
}
