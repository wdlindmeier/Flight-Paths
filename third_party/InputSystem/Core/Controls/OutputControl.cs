using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public abstract class OutputControl<T> : InputControl
    {
        protected T m_Value;
        public virtual T value
        {
            get
            {
                if (!InputSystem.isActive)
                    return defaultValue;

                return m_Value;
            }
            set
            {
                m_Value = value;
                m_ChangedValue = true;
            }
        }

        private T m_DefaultValue;
        public T defaultValue { get { return m_DefaultValue; } }

        private bool m_ChangedValue;
        public override bool changedValue { get { return m_ChangedValue; } }

        public override object valueObject { get { return value; } }
        public override bool isDefaultValue { get { return value.Equals(m_DefaultValue); } }

        public override void AdvanceFrame()
        {
            m_ChangedValue = false;
        }

        public override void Reset()
        {
            m_Value = m_DefaultValue;
            m_ChangedValue = false;
        }

        public override void CopyValueFromControl(InputControl outputControl)
        {
            m_Value = ((OutputControl<T>)outputControl).value;
            m_ChangedValue = true;
        }

        public override object Clone()
        {
            var clone = (OutputControl<T>)Activator.CreateInstance(GetType());
            clone.m_Value = m_Value;
            clone.m_DefaultValue = m_DefaultValue;
            clone.m_Enabled = m_Enabled;
            clone.m_ChangedValue = m_ChangedValue;
            clone.provider = provider;
            clone.index = index;
            clone.name = name;
            return clone;
        }
    }
}
