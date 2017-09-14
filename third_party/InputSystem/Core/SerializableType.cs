using System;

namespace UnityEngine.Experimental.Input
{
    [Serializable]
    public struct SerializableType : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string m_TypeName;

        [NonSerialized]
        private Type m_CachedType;

        public void OnBeforeSerialize()
        {
            // Required by ISerializationCallbackReceiver.
        }

        public void OnAfterDeserialize()
        {
            // Yes, actually needed in order for undo to work.
            // Do not remove and then not test that undo still works.
            // TODO: Add test.
            m_CachedType = null;
        }

        public SerializableType(Type t)
            : this()
        {
            value = t;
        }

        public Type value
        {
            get
            {
                if (m_CachedType == null)
                {
                    if (string.IsNullOrEmpty(m_TypeName))
                        return null;
                    m_CachedType = Type.GetType(m_TypeName);
                }
                return m_CachedType;
            }
            set
            {
                m_CachedType = value;
                if (value == null)
                    m_TypeName = string.Empty;
                else
                    m_TypeName = m_CachedType.AssemblyQualifiedName;
            }
        }

        public string name
        {
            get
            {
                var type = value;
                if (type == null)
                    return string.Empty;
                return type.Name;
            }
        }

        public static implicit operator Type(SerializableType t)
        {
            return t.value;
        }

        public static implicit operator SerializableType(Type t)
        {
            return new SerializableType(t);
        }

        internal bool IsValid()
        {
            return !string.IsNullOrEmpty(m_TypeName);
        }
    }
}
