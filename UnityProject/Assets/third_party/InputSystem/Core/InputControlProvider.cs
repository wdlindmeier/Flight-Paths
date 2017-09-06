using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    public abstract class InputControlProvider
    {
        private InputState m_State;
        public InputState state { get { return m_State; } }

        public static Action onStatusChanged;

        // Needed in this base class rather than directly in InputDevice since
        // ButtonControl uses the property and works with any InputControlProvider.
        private bool m_Active = true;
        public bool active
        {
            get
            {
                return m_Active;
            }
            set
            {
                if (m_Active == value)
                    return;

                m_Active = value;
                if (onStatusChanged != null)
                    onStatusChanged();
            }
        }

        protected void SetControls(List<InputControl> controls)
        {
            m_State = new InputState(this, controls);
        }

        public virtual bool ProcessEvent(InputEvent inputEvent)
        {
            lastEventTime = inputEvent.time;
            return false;
        }

        public int controlCount
        {
            get { return m_State.controls.Count; }
        }

        public IEnumerable<InputControl> controls
        {
            get
            {
                Debug.Assert(m_State != null);
                return m_State.controls;
            }
        }

        public InputControl GetControl(int index)
        {
            if (index < 0 || index >= m_State.controls.Count)
                throw new IndexOutOfRangeException();
            return m_State.controls[index];
        }

        public InputControl GetControl(string controlName)
        {
            for (var i = 0; i < m_State.controls.Count; ++i)
            {
                if (m_State.controls[i].name == controlName)
                    return m_State.controls[i];
            }
            throw new KeyNotFoundException(controlName);
        }

        public virtual string GetSourceName(int controlIndex)
        {
            return GetControl(controlIndex).name;
        }

        public InputControl GetControlFromHash(int hash)
        {
            int index = GetControlIndexFromHash(hash);
            if (index == -1)
                return null;
            return GetControl(index);
        }

        public abstract int GetControlIndexFromHash(int hash);

        public abstract int GetHashForControlIndex(int controlIndex);

        public double lastEventTime { get; protected set; }
    }
}
