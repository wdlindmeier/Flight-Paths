using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.Experimental.Input
{
    public class PlayerInput : PlayerHandleProvider
    {
        // Should this player handle request assignment of an input device as soon as the component awakes?
        [FormerlySerializedAs("autoAssignGlobal")]
        public bool autoAssign = true;
        [FormerlySerializedAs("autoAssignGlobal")]
        public bool global = true;

        public List<ActionMapSlot> actionMaps = new List<ActionMapSlot>();

        public PlayerHandle handle { get; set; }
        public override PlayerHandle GetHandle() { return handle; }

        void Awake()
        {
            if (autoAssign)
            {
                handle = PlayerHandleManager.GetNewPlayerHandle();
                handle.global = global;
                SetPlayerHandleMaps(handle, true, true);
            }
        }

        public void SetPlayerHandleMaps(PlayerHandle handle, bool initializeWithDevices = true, bool allowAssignUnassignedDevices = false)
        {
            handle.maps.Clear();
            for (int i = 0; i < actionMaps.Count; i++)
            {
                ActionMapSlot actionMapSlot = actionMaps[i];
                ActionMapInput actionMapInput = ActionMapInput.Create(actionMapSlot.actionMap);
                actionMapInput.active = actionMapSlot.active;
                actionMapInput.blockSubsequent = actionMapSlot.blockSubsequent;
                if (initializeWithDevices)
                    actionMapInput.TryInitializeWithDevices(handle.GetApplicableDevices(allowAssignUnassignedDevices));
                if (!handle.global && allowAssignUnassignedDevices)
                {
                    List<InputDevice> usedDevices = actionMapInput.GetCurrentlyUsedDevices();
                    for (int deviceIndex = 0; deviceIndex < usedDevices.Count; deviceIndex++)
                    {
                        if (usedDevices[deviceIndex].GetAssignment() == null)
                            handle.AssignDevice(usedDevices[deviceIndex]);
                    }
                }
                handle.maps.Add(actionMapInput);
            }
        }

        public bool CanInstantiateNewPlayerInputFromThis()
        {
            var unassignedDevices = new List<InputDevice>();
            for (int i = 0; i < InputSystem.devices.Count; i++)
            {
                if (InputSystem.devices[i].GetAssignment() == null)
                    unassignedDevices.Add(InputSystem.devices[i]);
            }

            for (int i = 0; i < actionMaps.Count; i++)
            {
                ActionMapSlot actionMapSlot = actionMaps[i];
                if (!actionMapSlot.actionMap.CanInitializeInstanceWithDevices(unassignedDevices))
                    return false;
            }
            return true;
        }

        public T GetActions<T>() where T : ActionMapInput
        {
            if (handle == null)
                return null;
            return handle.GetActions<T>();
        }
    }
}
