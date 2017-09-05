using System;
using UnityEngine.Events;
using UnityEngineInternal.Input;

namespace UnityEngine.Experimental.Input
{
    // Contains the global state for the input system. Must be able
    // to survive a domain reload while carrying its state over intact.
    internal class InputSystemManager : ScriptableObject
    {
        public InputDeviceManager deviceManager;
        public InputEventManager eventManager;
        public NativeInputDeviceManager nativeDeviceManager;

        private NativeInputUpdateType currentUpdateType { get; set; }


        // Device profiles register themselves on every domain load. This happens before
        // heap state gets restored in the editor. Given that persisting the profile
        // manager would require making all profiles serializable and given that profiles
        // register themselves on startup, we don't make the profile manager part of the
        // serialized input system state and rather just create it on demand.
        [NonSerialized]
        private InputDeviceProfileManager m_ProfileManager;
        public InputDeviceProfileManager profileManager
        {
            get
            {
                if (m_ProfileManager == null)
                    m_ProfileManager = new InputDeviceProfileManager();
                return m_ProfileManager;
            }
        }

        private UnityEvent m_OnGenerateEvents;
        public UnityEvent onGenerateEvents
        {
            get { return m_OnGenerateEvents; }
            set { m_OnGenerateEvents = value; }
        }

        internal void InvokeGenerateEvents()
        {
            if (m_OnGenerateEvents != null)
                m_OnGenerateEvents.Invoke();
        }

        // This one does not contain any state we need to persist across reloads.
        [NonSerialized]
        public NativeInputEventManager nativeEventManager;

        [NonSerialized]
        private bool m_IsInitialized;

        // The amount of time the virtual unscaled time lags behind the current real time.
        // Events are executed up to and including the current unscaled time,
        // meaning those with a timestamp higher than the unscaled time are postponed.
        // We do this to get proper distribution over multiple FixedUpdate calls.
        // However, since unscaled time lags behind realtime, we can get a little lag this way
        // (though it's typically a small fraction of a frame; much less than the lag from
        // Update callbacks to rendering.)
        // Nevertheless, we keep track of the lag and offset the timestamps by this lag
        // when comparing against unscaled time, such that we can eliminate this lag entirely.
        // We record what the lag is just before processing dynamic update events, such that
        // we're guaranteed to catch all events in the dynamic update input processing call,
        // which is the last one of the frame.
        // The same recorded lag is used in the subsequent fixed update calls of the next
        // frame. This won't give a 100% correct distribution if the next frame delta time
        // turns out to be shorter or longer, but it's a decent estimate and the real thing
        // can't be known in advance.
        private float m_VirtualTimeLag;

        public void OnEnable()
        {
            Initialize();
        }

        protected void Initialize()
        {
            if (m_IsInitialized)
                return;

            if (deviceManager == null)
                deviceManager = new InputDeviceManager();

            if (eventManager == null)
                eventManager = new InputEventManager();

            if (nativeDeviceManager == null)
                nativeDeviceManager = new NativeInputDeviceManager();

            #if UNITY_EDITOR
            // Clean up profiles used by devices. We don't serialize profiles but let InputDeviceManager
            // re-create them after domain reloads.
            profileManager.ConsolidateProfilesWithThoseUsedByDevices(deviceManager.devices);
            #endif

            nativeDeviceManager.Initialize(deviceManager, profileManager);

            nativeEventManager = new NativeInputEventManager();
            nativeEventManager.Initialize(eventManager, nativeDeviceManager);
            nativeEventManager.onReceivedEvents += OnProcessEvents;

            eventManager.rewriters.children.Add(
                new InputHandlerCallback { processEvent = deviceManager.RemapEvent });
            eventManager.consumers.children.Insert(0, deviceManager);

            NativeInputSystem.onUpdate += OnUpdate;

            currentUpdateType = NativeInputUpdateType.EndBeforeRender;

            m_IsInitialized = true;
        }

        private void OnUpdate(NativeInputUpdateType requestedUpdateType)
        {
#if UNITY_EDITOR
            var gameIsPlayingAndHasFocus = UnityEditor.EditorApplication.isPlaying && Application.isFocused;
#endif
            currentUpdateType = requestedUpdateType;
            switch (currentUpdateType)
            {
                // NOTE: BeginFixed and BeginDynamic have to set InputSystem.isActive for the time period
                //       until the *next* BeginFixed/BeginDynamic and not just to EndFixed/EndDynamic. This
                //       is because the game code for fixed and dynamic updates runs *after* the respective
                //       end update notification.

                case NativeInputUpdateType.BeginFixed:
#if UNITY_EDITOR
                    InputSystem.isActive = gameIsPlayingAndHasFocus;
#endif
                    if (InputSystem.isActive)
                        eventManager.handlerRoot.BeginUpdate();
                    break;

                case NativeInputUpdateType.EndFixed:
                    if (InputSystem.isActive)
                        eventManager.handlerRoot.EndUpdate();
                    break;

                case NativeInputUpdateType.BeginDynamic:
#if UNITY_EDITOR
                    InputSystem.isActive = gameIsPlayingAndHasFocus;
#endif
                    if (InputSystem.isActive)
                        eventManager.handlerRoot.BeginUpdate();
                    break;

                case NativeInputUpdateType.EndDynamic:
                    if (InputSystem.isActive)
                        eventManager.handlerRoot.EndUpdate();
                    break;
                case NativeInputUpdateType.BeginBeforeRender:
                    // we only require that the update type is correctly set BeginBeforeRender
                    break;
                case NativeInputUpdateType.EndBeforeRender:
                    deviceManager.PostProcess();
                    break;

#if UNITY_EDITOR
                case NativeInputUpdateType.BeginEditor:
                    InputSystem.isActive = !gameIsPlayingAndHasFocus;
                    if (InputSystem.isActive)
                        eventManager.handlerRoot.BeginUpdate();
                    break;

                case NativeInputUpdateType.EndEditor:
                    if (InputSystem.isActive)
                        eventManager.handlerRoot.EndUpdate();
                    break;
#endif
            }
        }

        private void OnProcessEvents()
        {
            // Don't process events when we're not active. This relies on the system being continuously activated
            // by either the player loop or the editor update code. If that doesn't happen, we may end up accumulating
            // massive amounts of events here.
            if (!InputSystem.isActive)
                return;

            InvokeGenerateEvents();

            if (!Time.inFixedTimeStep)
                m_VirtualTimeLag = Time.realtimeSinceStartup - Time.unscaledTime;

            if (currentUpdateType != NativeInputUpdateType.BeginBeforeRender)
            {
                eventManager.ExecuteEvents(Time.unscaledTime + m_VirtualTimeLag);
            }
            else
            {
                // we only want to update tracking events during the BeginBeforeRender pass..
                eventManager.ExecuteEventsByType<TrackingEvent>(Time.unscaledTime + m_VirtualTimeLag);
            }
        }
    }
}
