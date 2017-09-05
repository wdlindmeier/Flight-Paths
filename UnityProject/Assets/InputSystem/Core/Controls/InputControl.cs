using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // Only InputControl<T> should inherit from InputControl.
    public abstract class InputControl : ICloneable
    {
        protected bool m_Enabled;
        public bool enabled { get { return m_Enabled; } set { m_Enabled = value; } }

        public InputControlProvider provider { get; internal set; }
        public int index { get; internal set; }
        // For an InputControl in an ActionMapInput the name is the action name
        // and the source name is the name of the controls it's bound to.
        public string name { get; internal set; }
        public string sourceName { get { return provider.GetSourceName(index); } }

        public abstract object valueObject { get; }
        public abstract bool changedValue { get; }
        public abstract bool isDefaultValue { get; }

        public abstract void Reset();
        public abstract void CopyValueFromControl(InputControl inputControl);
        public abstract void AdvanceFrame();
        public abstract object Clone();
    }

    public abstract class InputControl<T> : InputControl
    {
        protected T m_RawValueDynamic;
        protected T m_RawValueFixed;
        public T rawValue
        {
            get
            {
                return Time.inFixedTimeStep ? m_RawValueFixed : m_RawValueDynamic;
            }
            set
            {
                if (Time.inFixedTimeStep)
                    m_RawValueFixed = value;
                else
                    m_RawValueDynamic = value;
            }
        }

        protected T m_ValueDynamic;
        protected T m_ValueFixed;
        public T value
        {
            get
            {
                if (!InputSystem.isActive)
                    return defaultValue;

                return Time.inFixedTimeStep ? m_ValueFixed : m_ValueDynamic;
            }
            // When setting a value in any callback that comes from processing an event,
            // SetValueFromEvent should be used instead of the value setter.
            // This does not apply in EndUpdate / PostProcessState callbacks.
            set
            {
                if (Time.inFixedTimeStep)
                    m_ValueFixed = value;
                else
                    m_ValueDynamic = value;
            }
        }

        private T m_PreviousValueDynamic;
        private T m_PreviousValueFixed;
        public T previousValue
        {
            get { return Time.inFixedTimeStep ? m_PreviousValueFixed : m_PreviousValueDynamic; }
        }

        private T m_DefaultValue;
        public T defaultValue
        {
            get { return m_DefaultValue; }
            protected set { m_DefaultValue = value; }
        }

        public override object valueObject { get { return value; } }
        public override bool changedValue { get { return !value.Equals(previousValue); } }
        public override bool isDefaultValue { get { return value.Equals(m_DefaultValue); } }

        // When setting a value in any callback that comes from processing an event,
        // SetValueFromEvent should be used.
        // Otherwise, the value property setter should be used.
        public virtual void SetValueFromEvent(T newValue)
        {
            m_RawValueDynamic = newValue;
            m_RawValueFixed = newValue;
            m_ValueDynamic = newValue;
            m_ValueFixed = newValue;
        }

        public override void AdvanceFrame()
        {
            if (Time.inFixedTimeStep)
                m_PreviousValueFixed = m_ValueFixed;
            else
                m_PreviousValueDynamic = m_ValueDynamic;
        }

        public override void Reset()
        {
            m_RawValueFixed = m_DefaultValue;
            m_RawValueDynamic = m_DefaultValue;
            m_ValueFixed = m_DefaultValue;
            m_ValueDynamic = m_DefaultValue;
            m_PreviousValueFixed = m_DefaultValue;
            m_PreviousValueDynamic = m_DefaultValue;
        }

        public override void CopyValueFromControl(InputControl inputControl)
        {
            InputControl<T> control = ((InputControl<T>)inputControl);
            m_RawValueFixed = control.rawValue;
            m_RawValueDynamic = control.rawValue;
            m_ValueFixed = control.value;
            m_ValueDynamic = control.value;
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }

        public virtual T GetCombinedValue(T[] values)
        {
            return values.Length > 0 ? values[0] : defaultValue;
        }
    }
}
