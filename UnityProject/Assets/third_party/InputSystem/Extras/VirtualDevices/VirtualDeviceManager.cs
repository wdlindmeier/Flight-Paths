using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public class VirtualDeviceManager : MonoBehaviour
    {
        static VirtualDeviceManager s_Instance;

        Dictionary<Type, InputDevice> m_Devices = new Dictionary<Type, InputDevice>();

        public static TInputDevice GetDevice<TInputDevice>() where TInputDevice : InputDevice, new()
        {
            if (s_Instance == null)
            {
                var go = new GameObject("VirtualDeviceManager");
                go.hideFlags = HideFlags.HideInHierarchy;
                s_Instance = go.AddComponent<VirtualDeviceManager>();
                DontDestroyOnLoad(go);
            }

            return s_Instance.GetDeviceInternal<TInputDevice>();
        }

        TInputDevice GetDeviceInternal<TInputDevice>() where TInputDevice : InputDevice, new()
        {
            InputDevice device = null;
            if (!m_Devices.TryGetValue(typeof(TInputDevice), out device))
            {
                device = new TInputDevice();
                device.SetupWithoutProfile();
                InputSystem.RegisterDevice(device);
                m_Devices[typeof(TInputDevice)] = device;
            }
            return (TInputDevice)device;
        }

        void OnDestroy()
        {
            foreach (var kvp in m_Devices)
            {
                InputSystem.UnregisterDevice(kvp.Value);
            }
        }

        public static void SendValueToControl<TValue>(InputControl<TValue> control, TValue value)
        {
            TValue currentValue = control.value;
            if (value.Equals(currentValue))
                return;

            var inputEvent = InputSystem.CreateEvent<GenericControlEvent<TValue>>();
            inputEvent.device = (InputDevice)control.provider;
            inputEvent.controlIndex = control.index;
            inputEvent.value = value;
            inputEvent.alreadyRemapped = true;
            InputSystem.QueueEvent(inputEvent);
        }
    }
}
