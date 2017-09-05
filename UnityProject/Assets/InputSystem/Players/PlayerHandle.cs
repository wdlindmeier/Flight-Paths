using System;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
    public class PlayerHandle : IInputHandler
    {
        public readonly int index;
        public List<PlayerDeviceAssignment> assignments = new List<PlayerDeviceAssignment>();
        public List<ActionMapInput> maps = new List<ActionMapInput>();

        public bool processAll { get; set; }

        public static Action onChanged;

        private bool m_Global = false;
        private List<bool> m_MapsPrevActiveStates = new List<bool>();
        private int m_FirstMapToReceiveEvents = 0;

        private double m_AutoReinitializeMinDelay = 0.5;

        InputHandlerNode currentInputHandler
        {
            get
            {
                return m_Global ? InputSystem.globalPlayers : InputSystem.assignedPlayers;
            }
        }

        internal PlayerHandle(int index)
        {
            this.index = index;
            currentInputHandler.children.Add(this);

            if (onChanged != null)
                onChanged();
        }

        public void Destroy()
        {
            for (int i = 0; i < maps.Count; i++)
                maps[i].active = false;

            for (int i = assignments.Count - 1; i >= 0; i--)
                assignments[i].Unassign();

            currentInputHandler.children.Remove(this);

            PlayerHandleManager.RemovePlayerHandle(this);
            if (onChanged != null)
                onChanged();
        }

        public bool global
        {
            get { return m_Global; }
            set
            {
                if (value == m_Global)
                    return;

                // Note: value of m_Global changes what currentInputHandler is.
                currentInputHandler.children.Remove(this);
                m_Global = value;
                currentInputHandler.children.Add(this);

                if (onChanged != null)
                    onChanged();
            }
        }

        public T GetActions<T>() where T : ActionMapInput
        {
            // If already contains ActionMapInput if this type, return that.
            for (int i = 0; i < maps.Count; i++)
                if (maps[i].GetType() == typeof(T))
                    return (T)maps[i];
            return null;
        }

        public ActionMapInput GetActions(ActionMap actionMap)
        {
            // If already contains ActionMapInput based on this ActionMap, return that.
            for (int i = 0; i < maps.Count; i++)
                if (maps[i].actionMap == actionMap)
                    return maps[i];
            return null;
        }

        public bool AssignDevice(InputDevice device)
        {
            if (device.GetAssignment() != null)
            {
                // If already assigned to this player, accept as success. Otherwise, fail.
                if (device.GetAssignment().player == this)
                    return true;
                else
                    return false;
            }

            var assignment = new PlayerDeviceAssignment(this, device);
            assignment.Assign();

            return true;
        }

        public void UnassignDevice(InputDevice device)
        {
            if (device.GetAssignment().player == this)
                device.GetAssignment().Unassign();
        }

        public bool ProcessEvent(InputEvent inputEvent)
        {
            if (!global && (inputEvent.device.GetAssignment() == null || inputEvent.device.GetAssignment().player != this))
                return false;

            for (int i = m_FirstMapToReceiveEvents; i < maps.Count; i++)
            {
                if ((processAll || maps[i].active) && (global || maps[i].CurrentlyUsesDevice(inputEvent.device)))
                {
                    if ((ProcessEventInMap(maps[i], inputEvent) && !processAll) || maps[i].blockSubsequent)
                        return true;
                }
            }

            return false;
        }

        bool ProcessEventInMap(ActionMapInput map, InputEvent inputEvent)
        {
            if (map.ProcessEvent(inputEvent))
                return true;

            if (!map.autoReinitialize)
                return false;

            if (map.CurrentlyUsesDevice(inputEvent.device))
                return false;

            // Only switch control scheme if devices in existing scheme weren't used for a little while,
            // to avoid rapid switching.
            if (inputEvent.time < map.GetLastDeviceInputTime() + m_AutoReinitializeMinDelay)
                return false;

            // If this event uses a different device than the current control scheme
            // then try and initialize a control scheme that has that device.
            // Otherwise, leave the current current control scheme state alone
            // as a re-initialization of the same control scheme will cause a reset in the process.
            if (!map.TryInitializeWithDevices(GetApplicableDevices(), new List<InputDevice>() { inputEvent.device }))
                return false;

            m_FirstMapToReceiveEvents = maps.IndexOf(map) + 1;
            map.SendControlResetEvents();
            m_FirstMapToReceiveEvents = 0;

            // When changing control scheme, we do not want to init control scheme to device states
            // like we normally want, so do a hard reset here, before processing the new event.
            map.Reset(false);

            return map.ProcessEvent(inputEvent);
        }

        public List<InputDevice> GetApplicableDevices(bool alwaysIncludeUnassigned = false)
        {
            List<InputDevice> applicable = new List<InputDevice>();
            if (global || alwaysIncludeUnassigned)
            {
                for (int i = 0; i < InputSystem.devices.Count; i++)
                {
                    if (InputSystem.devices[i].GetAssignment() == null)
                        applicable.Add(InputSystem.devices[i]);
                }
            }

            if (global)
                return applicable;

            for (int i = 0; i < assignments.Count; i++)
            {
                applicable.Add(assignments[i].device);
            }
            return applicable;
        }

        public void BeginUpdate()
        {
            if (m_MapsPrevActiveStates.Count != maps.Count)
            {
                while (maps.Count > m_MapsPrevActiveStates.Count)
                    m_MapsPrevActiveStates.Add(false);
                while (maps.Count < m_MapsPrevActiveStates.Count)
                    m_MapsPrevActiveStates.RemoveAt(m_MapsPrevActiveStates.Count - 1);
            }
            for (int i = 0; i < m_MapsPrevActiveStates.Count; i++)
            {
                if (maps[i].active != m_MapsPrevActiveStates[i])
                {
                    if (maps[i].active && maps[i].autoReinitialize)
                        maps[i].TryInitializeWithDevices(GetApplicableDevices());

                    m_FirstMapToReceiveEvents = i + 1;
                    maps[i].SendControlResetEvents();
                    m_FirstMapToReceiveEvents = 0;

                    maps[i].Reset(maps[i].active);
                }
                m_MapsPrevActiveStates[i] = maps[i].active;
            }

            for (int i = 0; i < maps.Count; i++)
            {
                if (processAll || maps[i].active)
                    maps[i].BeginUpdate();
            }
        }

        public void EndUpdate()
        {
            for (int i = 0; i < maps.Count; i++)
            {
                if (processAll || maps[i].active)
                    maps[i].EndUpdate();
            }
        }
    }
}
