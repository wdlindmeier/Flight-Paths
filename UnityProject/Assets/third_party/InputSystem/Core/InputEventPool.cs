using System;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
    internal class InputEventPool : IInputEventPool
    {
        private Dictionary<Type, List<InputEvent>> m_CachedEvents = new Dictionary<Type, List<InputEvent>>();

        public TEvent ReuseOrCreate<TEvent>()
            where TEvent : InputEvent, new()
        {
            var type = typeof(TEvent);

            List<InputEvent> cachedEvents;
            if (m_CachedEvents.TryGetValue(type, out cachedEvents))
            {
                var count = cachedEvents.Count;
                if (count > 0)
                {
                    var inputEvent = cachedEvents[count - 1];
                    cachedEvents.RemoveAt(count - 1);
                    return (TEvent)inputEvent;
                }
            }

            return new TEvent();
        }

        public void Return(InputEvent inputEvent)
        {
            var type = inputEvent.GetType();

            List<InputEvent> cachedEvents;
            if (!m_CachedEvents.TryGetValue(type, out cachedEvents))
            {
                cachedEvents = new List<InputEvent>();
                m_CachedEvents[type] = cachedEvents;
            }

            inputEvent.Reset();
            cachedEvents.Add(inputEvent);
        }
    }
}
