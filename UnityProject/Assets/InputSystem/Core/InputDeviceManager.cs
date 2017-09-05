using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [Serializable]
#endif
    internal class InputDeviceManager : IInputHandler
#if UNITY_EDITOR // This class is internal so switching around the API like this is okay.
        , ISerializationCallbackReceiver
#endif
    {
        private List<InputDevice> m_InputDevices = new List<InputDevice>();
        public List<InputDevice> devices { get { return m_InputDevices; } }

        private Dictionary<Type, List<InputDevice>> m_DevicesByType = new Dictionary<Type, List<InputDevice>>();
        private Dictionary<Type, InputDevice> m_MostRecentDeviceOfType = new Dictionary<Type, InputDevice>();

        public void RegisterDevice(InputDevice inputDevice)
        {
            if (m_InputDevices.Contains(inputDevice))
                return;

            m_InputDevices.Add(inputDevice);

            var deviceType = inputDevice.GetType();
            RegisterDeviceByTypes(deviceType, inputDevice);
        }

        public void UnregisterDevice(InputDevice inputDevice)
        {
            if (!m_InputDevices.Contains(inputDevice))
                return;

            m_InputDevices.Remove(inputDevice);

            var deviceType = inputDevice.GetType();
            UnregisterDeviceByTypes(deviceType, inputDevice);
        }

        private void RegisterDeviceByTypes(Type deviceType, InputDevice inputDevice)
        {
            List<InputDevice> list;
            if (!m_DevicesByType.TryGetValue(deviceType, out list))
            {
                list = new List<InputDevice>();
                m_DevicesByType[deviceType] = list;
            }
            list.Add(inputDevice);
            m_MostRecentDeviceOfType[deviceType] = inputDevice;

            var baseType = deviceType.BaseType;
            if (baseType != typeof(InputDevice))
                RegisterDeviceByTypes(baseType, inputDevice);
        }

        private void UnregisterDeviceByTypes(Type deviceType, InputDevice inputDevice)
        {
            List<InputDevice> list;
            if (m_DevicesByType.TryGetValue(deviceType, out list))
            {
                list.Remove(inputDevice);
            }

            var baseType = deviceType.BaseType;
            if (baseType != typeof(InputDevice))
                UnregisterDeviceByTypes(baseType, inputDevice);
        }

        public InputDevice GetCurrentDeviceOfType(Type deviceType)
        {
            InputDevice device;
            if (m_MostRecentDeviceOfType.TryGetValue(deviceType, out device))
                return device;
            return null;
        }

        public TDevice GetCurrentDeviceOfType<TDevice>() where TDevice : InputDevice
        {
            return (TDevice)GetCurrentDeviceOfType(typeof(TDevice));
        }

        public int GetDeviceCountOfType(Type deviceType)
        {
            List<InputDevice> list;
            if (!m_DevicesByType.TryGetValue(deviceType, out list))
                return 0;

            return list.Count;
        }

        public InputDevice LookupDevice(Type deviceType, int deviceIndex)
        {
            List<InputDevice> list;
            if (!m_DevicesByType.TryGetValue(deviceType, out list) || deviceIndex >= list.Count)
                return null;

            return list[deviceIndex];
        }

        public int LookupDeviceIndex(InputDevice inputDevice)
        {
            List<InputDevice> list;
            if (!m_DevicesByType.TryGetValue(inputDevice.GetType(), out list))
                return -1;

            return list.IndexOf(inputDevice);
        }

        public bool RemapEvent(InputEvent inputEvent)
        {
            if (inputEvent.device == null)
                return false;

            GenericControlEvent genericEvent = inputEvent as GenericControlEvent;
            if (genericEvent != null && genericEvent.alreadyRemapped)
                return false;

            return inputEvent.device.RemapEvent(inputEvent);
        }

        public bool ProcessEvent(InputEvent inputEvent)
        {
            InputDevice device = inputEvent.device;

            // TODO: Ignore if disconnected.
            if (device != null)
            {
                var consumed = device.ProcessEvent(inputEvent);
                MakeMostRecentDevice(device);
                return consumed;
            }

            return false;
        }

        public void BeginUpdate()
        {
            for (int i = 0; i < devices.Count; i++)
                devices[i].BeginUpdate();
        }

        public void EndUpdate()
        {
            for (int i = 0; i < devices.Count; i++)
                devices[i].EndUpdate();
        }

        public void PostProcess()
        {
            for (int i = 0; i < devices.Count; i++)
                devices[i].PostProcess();
        }

        void MakeMostRecentDevice(InputDevice device)
        {
            for (var type = device.GetType(); type != typeof(InputDevice); type = type.BaseType)
                m_MostRecentDeviceOfType[type] = device;
        }

// Support for surviving domain reloads.
// We can't leave serialization of InputDevices to Unity directly as it doesn't
// support polymorphism. Instead, we work around it here by serializing snapshots
// of each individual device manually.
#if UNITY_EDITOR
        [Serializable]
        private struct SerializedDeviceState
        {
            public SerializableType deviceType;
            public SerializableType profileType;
            public int nativeDeviceId;
            public bool isMostRecentDevice;
            public string name;
            public string manufacturer;
            public string serialNumber;
            public int tagIndex;
        }

        [Serializable]
        private class SerializedState
        {
            public List<SerializedDeviceState> deviceStates = new List<SerializedDeviceState>();
        }

        [SerializeField]
        private SerializedState m_SerializedState;

        public void OnBeforeSerialize()
        {
            m_SerializedState = new SerializedState();
            for (int i = 0; i < m_InputDevices.Count; i++)
            {
                var device = m_InputDevices[i];

                // We only serialize the non-native devices.  The native devices are recreated by the NativeInputDeviceManager
                if (device.nativeId == 0)
                {
                    var isMostRecentDevice = (m_MostRecentDeviceOfType[device.GetType()] == device);
                    m_SerializedState.deviceStates.Add(new SerializedDeviceState
                    {
                        deviceType = new SerializableType(device.GetType()),
                        profileType = device.profile != null ? new SerializableType(device.profile.GetType()) : new SerializableType(),
                        nativeDeviceId = device.nativeId,
                        isMostRecentDevice = isMostRecentDevice,
                        name = device.name,
                        manufacturer = device.manufacturer,
                        serialNumber = device.serialNumber,
                        tagIndex = device.tagIndex
                    });
                }
            }
        }

        public void OnAfterDeserialize()
        {
            m_InputDevices = new List<InputDevice>();
            var profiles = new Dictionary<Type, InputDeviceProfile>();

            if (m_SerializedState != null)
            {
                m_InputDevices.Capacity = m_SerializedState.deviceStates.Count;
                for (int i = 0; i < m_SerializedState.deviceStates.Count; i++)
                {
                    var deviceState = m_SerializedState.deviceStates[i];
                    var deviceType = deviceState.deviceType.value;
                    if (deviceType != null)
                    {
                        var device = (InputDevice)Activator.CreateInstance(deviceType);
                        device.nativeId = deviceState.nativeDeviceId;
                        device.name = deviceState.name;
                        device.manufacturer = deviceState.manufacturer;
                        device.serialNumber = deviceState.serialNumber;
                        device.tagIndex = deviceState.tagIndex;

                        var profileType = deviceState.profileType.value;
                        if (profileType != null)
                        {
                            InputDeviceProfile profile;
                            if (!profiles.TryGetValue(profileType, out profile))
                            {
                                profile = (InputDeviceProfile)Activator.CreateInstance(profileType);
                                profiles.Add(profileType, profile);
                            }
                            device.SetupFromProfile(profile);
                        }
                        else
                            device.SetupWithoutProfile();

                        m_InputDevices.Add(device);
                        if (deviceState.isMostRecentDevice)
                            MakeMostRecentDevice(device);
                    }
                }
                m_SerializedState = null;
            }
        }

#endif
    }
}
