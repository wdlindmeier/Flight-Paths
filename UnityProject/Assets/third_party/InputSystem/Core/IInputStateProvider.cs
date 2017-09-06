using System;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
    public interface IInputStateProvider
    {
        InputState GetDeviceStateForDeviceSlotKey(int deviceKey);
        int GetOrAddDeviceSlotKey(InputDevice device);
    }

    // A control domain source is a source for domains that provide controls.
    // An example is a control scheme, where each device used in the control
    // scheme is a separate domain.
    // Another example is an ActionMap, which has only one domain (itself).
    public interface IControlDomainSource
    {
        List<DomainEntry> GetDomainEntries();
        List<DomainEntry> GetControlEntriesOfType(int domainId, Type controlType);
    }
}
