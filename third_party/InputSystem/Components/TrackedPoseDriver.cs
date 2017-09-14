// comment out to use the old VR Node system
#define USE_NEW_INPUT_SYSTEM

using System;
using Assets.Utilities;

namespace UnityEngine.Experimental.Input.Spatial
{
    /// <summary>
    /// Base tracked object class.
    /// </summary>
    ///
    // The DefaultExecutionOrder it needed because TrackedPoseDriver does some
    // of its work in regular Update and FixedUpdate calls, but this needs to
    // be done before regular user scripts have their own Update and
    // FixedUpdate calls, in order that they correctly get the values for this
    // frame and not the previous.
    // -32000 is the minimal possible execution order value; -30000 makes it
    // unlikely users choses lower values for their scripts by accident, but
    // still makes it possible.
    [DefaultExecutionOrder(-30000)]
    [Serializable]
    [AddComponentMenu("XR/Tracked Pose Driver")]
    public class TrackedPoseDriver : MonoBehaviour, ISerializationCallbackReceiver
    {
        public enum TrackingSource
        {
            Device = 0,
            Action = 1,
        }
        [SerializeField]
        TrackingSource m_TrackingSource;
        public TrackingSource trackingSource
        {
            get { return m_TrackingSource; }
            set { m_TrackingSource = value; }
        }

        [SerializeField]
        DeviceSlot m_DeviceSlot;
        public DeviceSlot deviceSlot
        {
            get { return m_DeviceSlot; }
            set { m_DeviceSlot = value; }
        }

        [NonSerialized]
        ControlReferenceBinding<PoseControl, Pose> m_Binding = new ControlReferenceBinding<PoseControl, Pose>();
        public ControlReferenceBinding<PoseControl, Pose> binding
        {
            get { return m_Binding; }
            set { m_Binding = value; m_BindingDirty = true; }
        }

        protected bool m_BindingDirty = true;

        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializedBinding;


        [SerializeField]
        PoseAction m_PoseAction;
        public PoseAction poseAction
        {
            get { return poseAction; }
            set { poseAction = value; m_BindingDirty = true; }
        }

        [SerializeField]
        PlayerHandleProvider m_playerHandleProvider;

        public PlayerHandleProvider playerHandleProvider
        {
            get { return m_playerHandleProvider; }
            set { m_playerHandleProvider = value; m_BindingDirty = true; }
        }

        public enum TrackingType
        {
            RotationAndPosition,
            RotationOnly,
            PositionOnly
        }

        [SerializeField]
        TrackingType m_TrackingType;
        public TrackingType trackingType
        {
            get { return m_TrackingType; }
            set { m_TrackingType = value; }
        }

        public enum UpdateType
        {
            UpdateAndBeforeRender,
            Update,
            BeforeRender,
        }
        [SerializeField]
        UpdateType m_UpdateType;
        public UpdateType updateType
        {
            get { return m_UpdateType; }
            set { m_UpdateType = value; }
        }

        [SerializeField]
        bool m_UseRelativeTransform;
        public bool UseRelativeTransform
        {
            get { return m_UseRelativeTransform; }
            set { m_UseRelativeTransform = value; }
        }

#if UNITY_EDITOR
        // Normally, we subscribe to OnBeforeRender in the Awake callback.
        // However, if the user changes any script files while playing
        // in the Editor, all script files are reloaded, and Awake is not called.
        // This means the TrackedPoseDriver will unexpectedly stop updating.
        [NonSerialized]
        private bool m_IsInitialized = false;

        protected void EnsureInitialized()
        {
            if (!m_IsInitialized)
            {
                Application.onBeforeRender += OnBeforeRender;
                m_IsInitialized = true;
            }
        }

#endif

        protected Pose m_OriginPose;

        /// <summary>
        /// originPose is an offset applied to any tracking data read from this object.
        /// Setting this value should be reserved for dealing with edge-cases, such as
        /// achieving parity between room-scale (floor centered) and stationary (head centered)
        /// tracking - without having to alter the transform hierarchy.
        /// For user locomotion and gameplay purposes you are usually better off just
        ///  moving the parent transform of this object.
        /// </summary>
        public Pose originPose
        {
            get { return m_OriginPose; }
            set { m_OriginPose = value; }
        }

        protected virtual void CacheLocalPosition()
        {
            m_OriginPose.translation = transform.localPosition;
            m_OriginPose.rotation = transform.localRotation;
        }

        protected virtual void ResetToCachedLocalPosition()
        {
            SetLocalTransform(m_OriginPose.translation, m_OriginPose.rotation);
        }

        protected virtual void Awake()
        {
            CacheLocalPosition();

            if (this.enabled)
                AcquireDevice();

#if UNITY_EDITOR
            EnsureInitialized();
#else
            Application.onBeforeRender += OnBeforeRender;
#endif

            if (HasStereoCamera())
            {
                UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(GetComponent<Camera>(), true);
            }
        }

        protected virtual void OnDestroy()
        {
            Application.onBeforeRender -= OnBeforeRender;
            if (HasStereoCamera())
            {
                UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(GetComponent<Camera>(), false);
            }
        }

        protected virtual void AcquireDevice()
        {
            // if we're using the device grab the first one that matches the tag (need to fix this)
            if (m_TrackingSource == TrackingSource.Device &&
                m_DeviceSlot != null &&
                m_DeviceSlot.type.value != null &&
                m_Binding != null)
            {
                TrackedInputDevice device = null;
                for (int i = 0; i < InputSystem.devices.Count; i++)
                {
                    var current = InputSystem.devices[i];
                    if (m_DeviceSlot.type.value.Equals(current.GetType()) && current.tagIndex == m_DeviceSlot.tagIndex)
                    {
                        device = (TrackedInputDevice)current;
                        break;
                    }
                }
                if (device == null)
                {
                    // if the device is null attempt a fallback search for anything we can cast from
                    for (int i = 0; i < InputSystem.devices.Count; i++)
                    {
                        var current = InputSystem.devices[i];
                        if (m_DeviceSlot.type.value.IsAssignableFrom(current.GetType()) && current.tagIndex == m_DeviceSlot.tagIndex)
                        {
                            device = (TrackedInputDevice)current;
                            break;
                        }
                    }
                }
                if (device != null)
                {
                    m_Binding.Initialize(device);
                    m_BindingDirty = false;
                }
            }
            else if (m_TrackingSource == TrackingSource.Action &&
                     m_PoseAction != null &&
                     m_playerHandleProvider != null)
            {
                PlayerHandleProvider iph = m_playerHandleProvider as PlayerHandleProvider;
                if (iph != null)
                {
                    var handle = iph.GetHandle();
                    if (handle != null)
                    {
                        m_PoseAction.Bind(handle);
                        m_BindingDirty = false;
                    }
                }
            }
        }

        protected virtual void OnDisable()
        {
            // remove delegate registration
            ResetToCachedLocalPosition();
        }

#if UNITY_EDITOR
        protected virtual void OnEnable()
        {
            EnsureInitialized();
        }

#endif

        protected virtual void FixedUpdate()
        {
            if (m_UpdateType == UpdateType.Update ||
                m_UpdateType == UpdateType.UpdateAndBeforeRender)
            {
                PerformUpdate();
            }
        }

        protected virtual void Update()
        {
            if (m_UpdateType == UpdateType.Update ||
                m_UpdateType == UpdateType.UpdateAndBeforeRender)
            {
                PerformUpdate();
            }
        }

        protected virtual void OnBeforeRender()
        {
            if (m_UpdateType == UpdateType.BeforeRender ||
                m_UpdateType == UpdateType.UpdateAndBeforeRender)
            {
                PerformUpdate();
            }
        }

        protected virtual bool HasBinding()
        {
            return ((m_BindingDirty == false) &&
                    (m_Binding.sourceControl != null) || (m_PoseAction.control != null));
        }

        protected virtual void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
        {
            if (m_TrackingType == TrackingType.RotationAndPosition ||
                m_TrackingType == TrackingType.RotationOnly)
            {
                transform.localRotation = newRotation;
            }

            if (m_TrackingType == TrackingType.RotationAndPosition ||
                m_TrackingType == TrackingType.PositionOnly)
            {
                transform.localPosition = newPosition;
            }
        }

        protected Pose TransformPoseByOriginIfNeeded(Pose pose)
        {
            if (m_UseRelativeTransform)
            {
                return pose.GetTransformedBy(m_OriginPose);
            }
            else
            {
                return pose;
            }
        }

        protected virtual bool HasStereoCamera()
        {
            Camera camera = GetComponent<Camera>();
            return camera != null && camera.stereoEnabled;
        }

        protected virtual void PerformUpdate()
        {
            if (!this.enabled)
                return;

            if (m_BindingDirty)
            {
                AcquireDevice();
            }

            if (HasBinding())
            {
                if (m_TrackingSource == TrackingSource.Device)
                    m_Binding.EndUpdate();

                Pose localPose = TransformPoseByOriginIfNeeded(GetBindingValue());
                SetLocalTransform(localPose.translation, localPose.rotation);
            }
        }

        protected virtual Pose GetBindingValue()
        {
            if (m_TrackingSource == TrackingSource.Device)
                return m_Binding.value;
            else if (m_TrackingSource == TrackingSource.Action)
                return m_PoseAction.control.value;

            return Pose.identity;
        }

        public virtual void OnBeforeSerialize()
        {
            m_SerializedBinding = SerializationHelper.SerializeObj(binding);
        }

        public virtual void OnAfterDeserialize()
        {
            binding = SerializationHelper.DeserializeObj<ControlReferenceBinding<PoseControl, Pose>>(m_SerializedBinding, new object[] {});
            m_SerializedBinding = new SerializationHelper.JSONSerializedElement();
        }
    }
}
