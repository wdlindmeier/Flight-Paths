using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngineInternal.Input;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace UnityEngine.Experimental.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class InputSystem
    {
        private static InputSystemManager s_Input;

        // State that is static and is recreated on every domain reload can go directly
        // in the InputSystem class rather than inside InputSystemManager or it's sub-managers.
        static List<BindingListener> s_BindingListeners = new List<BindingListener>();
        static Dictionary<int, SupportedControl> s_SupportedControls = new Dictionary<int, SupportedControl>();
        static Dictionary<Type, Dictionary<SupportedControl, bool>> s_ControlsPerDeviceType = new Dictionary<Type, Dictionary<SupportedControl, bool>>();

        public static UnityEvent onGenerateEvents { get { return s_Input.onGenerateEvents; } }

        // We have three ways that cause the input system to be initialized:
        // 1. Normal game/editor startup.
        // 2. Domain reload in editor (after script changes or when entering playmode).
        // 3. Manual Reset() from tests.
        //
        // 1 is initiated from InitializeOnLoad on editor startup and from RuntimeInitializeOnLoadMethod
        // in the player.
        //
        // 2 is initiated from RuntimeInitializeOnLoadMethod both in the editor and in the player. However,
        // InitializeOnLoad will *also* trigger in the editor.
        //
        // 3 is triggered from the Reset() method below.

        static InputSystem()
        {
#if UNITY_EDITOR
            // This gets run after every appdomain initialization. If it's a domain reload,
            // we probably have an InputSystemManager sticking around from before.
            //
            // NOTE: InitializeOnLoad in the editor will get executed *before* heap state is
            //       restored, so it is paramount that we do not attempt to access any of the
            //       serialized state until after OnEnable() has been called on InputSystemManager.
            InputSystemManager[] array = Resources.FindObjectsOfTypeAll<InputSystemManager>();
            s_Input = array.Length > 0 ? array[0] : null;
#endif
            if (s_Input == null)
                Reset();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void InitializeIfNotInitializedAlready()
        {
            // Will implicitly run InputSystem class constructor, if it hasn't been run before.
        }

        internal static void Reset()
        {
            // Grab the manager for native devices as we want those to survive the reset.
            NativeInputDeviceManager oldNativeDeviceManager = null;
            if (s_Input != null)
                oldNativeDeviceManager = s_Input.nativeDeviceManager;

            // Also, we want all profiles to survive the reset.
            InputDeviceProfileManager oldProfileManager = null;
            if (s_Input != null)
                oldProfileManager = s_Input.profileManager;

            // And we want no native events still going to the old native event manager.
            if (s_Input != null)
                s_Input.nativeEventManager.Uninitialize();

            // Create blank input system state.
            s_Input = ScriptableObject.CreateInstance<InputSystemManager>();
            s_Input.hideFlags = HideFlags.HideAndDontSave;

            // Hand existing profiles over to the new profile manager. We can't simply replace
            // the profile manager with the old one because InputSystemManager has already wired
            // up state using the new profile manager.
            if (oldProfileManager != null)
                s_Input.profileManager.StealProfilesFrom(oldProfileManager);

            // Recreate native devices, if we had them before.
            if (oldNativeDeviceManager != null)
            {
                s_Input.nativeDeviceManager.RecreateNativeDevicesFrom(oldNativeDeviceManager);
                oldNativeDeviceManager.Uninitialize();
            }
        }

        public static void QueueEvent(InputEvent inputEvent)
        {
            s_Input.eventManager.queue.Enqueue(inputEvent);
        }

        public static void ExecuteEvents(double upToTime = double.MaxValue)
        {
            s_Input.eventManager.ExecuteEvents(upToTime);
        }

        public static void RegisterDevice(InputDevice device)
        {
            s_Input.deviceManager.RegisterDevice(device);
        }

        public static void UnregisterDevice(InputDevice device)
        {
            s_Input.deviceManager.UnregisterDevice(device);
        }

        public static InputDevice GetCurrentDeviceOfType(Type deviceType)
        {
            return s_Input.deviceManager.GetCurrentDeviceOfType(deviceType);
        }

        public static TDevice GetCurrentDeviceOfType<TDevice>() where TDevice : InputDevice
        {
            return s_Input.deviceManager.GetCurrentDeviceOfType<TDevice>();
        }

        public static int GetDeviceCountOfType(Type deviceType)
        {
            return s_Input.deviceManager.GetDeviceCountOfType(deviceType);
        }

        public static InputDevice LookupDevice(Type deviceType, int deviceIndex)
        {
            return s_Input.deviceManager.LookupDevice(deviceType, deviceIndex);
        }

        public static int LookupDeviceIndex(InputDevice inputDevice)
        {
            return s_Input.deviceManager.LookupDeviceIndex(inputDevice);
        }

        public static void RegisterDeviceProfile(InputDeviceProfile profile)
        {
            s_Input.profileManager.RegisterProfile(profile);
        }

        // REVIEW: What's the reason for registering a type instead of an instance,
        // when the type is just used to create an instance anyway?
        public static void RegisterDeviceProfile<TProfile>()
            where TProfile : InputDeviceProfile, new()
        {
            RegisterDeviceProfile(new TProfile());
        }

        public static List<InputDevice> devices
        {
            get { return s_Input.deviceManager.devices; }
        }

#if UNITY_EDITOR
        public static List<string> unrecognizedDevices
        {
            get { return s_Input.nativeDeviceManager.unrecognizedDevices; }
        }
#endif

        public static float nativeInputPollingFrequency
        {
            set { NativeInputSystem.SetPollingFrequency(value); }
        }

        public static bool isActive
        {
#if !UNITY_EDITOR
            // In the player the input system is always active.
            ////REVIEW: should we deactivate when the application does not have focus?
            get { return true; }
#else
            // In the editor the input system is activated and deactivated by InputSystemManager.
            get; set;
#endif
        }

        ////TEMPORARY
        private static InputHandlerNode s_EventHandlerNode;
        public static InputEventHandler eventHandler
        {
            get
            {
                if (s_EventHandlerNode == null)
                    return null;

                return s_EventHandlerNode.handler;
            }
            set
            {
                if (s_EventHandlerNode == null)
                {
                    s_EventHandlerNode = new InputHandlerNode();
                    s_Input.eventManager.handlerRoot.children.Add(s_EventHandlerNode);
                }

                s_EventHandlerNode.handler = value;
            }
        }

        public static bool ExecuteEvent(InputEvent inputEvent)
        {
            var wasConsumed = s_Input.eventManager.handlerRoot.ProcessEvent(inputEvent);
            s_Input.eventManager.pool.Return(inputEvent);
            return wasConsumed;
        }

        // TODO
        public static InputHandlerNode rewriters { get { return s_Input.eventManager.rewriters; } }
        public static InputHandlerNode consumers { get { return s_Input.eventManager.consumers; } }
        public static InputHandlerNode globalPlayers { get { return s_Input.eventManager.globalPlayers; } }
        public static InputHandlerNode assignedPlayers { get { return s_Input.eventManager.assignedPlayers; } }

        public delegate bool BindingListener(InputControl control);

        public static void ListenForBinding(BindingListener listener)
        {
            ListenForBinding(listener, true);
        }

        public static void ListenForBinding(BindingListener listener, bool listen)
        {
            if (listen)
                s_BindingListeners.Add(listener);
            else
                s_BindingListeners.Remove(listener);
        }

        public static bool listeningForBinding
        {
            get { return s_BindingListeners.Count > 0; }
        }

        internal static void RegisterBinding(InputControl control)
        {
            for (int i = s_BindingListeners.Count - 1; i >= 0; i--)
            {
                if (s_BindingListeners[i] == null)
                {
                    s_BindingListeners.RemoveAt(i);
                    continue;
                }
                bool used = s_BindingListeners[i](control);
                if (used)
                {
                    // Sanity check in case the listener removed itself.
                    if (s_BindingListeners.Count > i)
                        s_BindingListeners.RemoveAt(i);
                    break;
                }
            }
        }

        public static TEvent CreateEvent<TEvent>() where TEvent : InputEvent, new()
        {
            var newEvent = s_Input.eventManager.pool.ReuseOrCreate<TEvent>();
            newEvent.time = Time.time;
            return newEvent;
        }

        public static void RegisterControl(SupportedControl control, Type deviceType, bool standardized)
        {
            if (!s_SupportedControls.ContainsKey(control.hash))
                s_SupportedControls[control.hash] = control;

            Dictionary<SupportedControl, bool> available = null;
            if (!s_ControlsPerDeviceType.TryGetValue(deviceType, out available))
            {
                available = new Dictionary<SupportedControl, bool>();
                s_ControlsPerDeviceType[deviceType] = available;
            }
            if (standardized || !available.ContainsKey(control))
                available[control] = standardized;
        }

        public static SupportedControl GetSupportedControl(int hash)
        {
            SupportedControl control;
            if (!s_SupportedControls.TryGetValue(hash, out control))
                return SupportedControl.None;
            return control;
        }

        public static Dictionary<SupportedControl, bool> GetSupportedControlsForDeviceType(Type deviceType)
        {
            Dictionary<SupportedControl, bool> result = null;
            s_ControlsPerDeviceType.TryGetValue(deviceType, out result);
            if (result == null)
            {
                var newDict = new Dictionary<SupportedControl, bool>();
                s_ControlsPerDeviceType[deviceType] = newDict;
                return newDict;
            }
            return result;
        }
    }

    public delegate bool InputEventHandler(InputEvent inputEvent);
}
