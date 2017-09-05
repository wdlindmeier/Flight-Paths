using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Assets.Utilities;

namespace UnityEngine.Experimental.Input
{
    public class InputAction : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        [FormerlySerializedAs("m_Name")]
        private string m_ActionName;
        public new string name { get { return m_ActionName; } set { m_ActionName = value; base.name = value; } }

        [SerializeField]
        private ActionMap m_ActionMap;
        public ActionMap actionMap { get { return m_ActionMap; } set { m_ActionMap = value; } }

        [SerializeField]
        private int m_ActionIndex;
        public int actionIndex { get { return m_ActionIndex; } set { m_ActionIndex = value; } }

        [SerializeField]
        private SerializableType m_ControlType;
        public Type controlType { get { return m_ControlType; } set { m_ControlType = value; } }

        [SerializeField]
        private bool m_Combined;
        public bool combined { get { return m_Combined; } set { m_Combined = value; } }

        [NonSerialized]
        private InputBinding m_SelfBinding = null;
        public InputBinding selfBinding { get { return m_SelfBinding; } set { m_SelfBinding = value; } }
        [SerializeField]
        SerializationHelper.JSONSerializedElement m_SerializableSelfBinding;

        public virtual void OnBeforeSerialize()
        {
            m_SerializableSelfBinding = SerializationHelper.SerializeObj(selfBinding);
        }

        public virtual void OnAfterDeserialize()
        {
            selfBinding = SerializationHelper.DeserializeObj<InputBinding>(m_SerializableSelfBinding, new object[] {});
            m_SerializableSelfBinding = new SerializationHelper.JSONSerializedElement();
        }
    }
}
