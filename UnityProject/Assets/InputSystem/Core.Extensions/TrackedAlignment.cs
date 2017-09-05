using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;

namespace UnityEngine.Experimental.Input
{
    public class TrackedAlignment : MonoBehaviour, ISerializationCallbackReceiver
    {
        public enum Source
        {
            Device,
            ActionMap
        }

        [SerializeField]
        Source m_Source;
        public Source source { get { return m_Source; } set { m_Source = value; } }

        // Device source fields.

        [SerializeField]
        DeviceSlot m_DeviceSlot;
        public DeviceSlot deviceSlot { get { return m_DeviceSlot; } set { m_DeviceSlot = value; } }

        [NonSerialized]
        public ControlReferenceBinding<Vector3Control, Vector3> m_Binding = new ControlReferenceBinding<Vector3Control, Vector3>();
        public ControlReferenceBinding<Vector3Control, Vector3> binding { get { return m_Binding; } set { m_Binding = value; } }

        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializedBinding;

        // ActionMap source fields.

        [SerializeField]
        PlayerInput m_PlayerInput;
        public PlayerInput playerInput { get { return m_PlayerInput; } set { m_PlayerInput = value; } }

        [SerializeField]
        Vector3Action m_Action;
        public Vector3Action action { get { return m_Action; } set { m_Action = value; } }

        // Other fields.

        // ...

        public virtual void OnBeforeSerialize()
        {
            m_SerializedBinding = SerializationHelper.SerializeObj(binding);
        }

        public virtual void OnAfterDeserialize()
        {
            binding = SerializationHelper.DeserializeObj<ControlReferenceBinding<Vector3Control, Vector3>>(m_SerializedBinding, new object[] {});
            m_SerializedBinding = new SerializationHelper.JSONSerializedElement();
        }
    }
}
