using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
    internal class InputEventQueue : IInputEventQueue
    {
        ////TODO: serialize properly for domain reload

        // SortedList doesn't allow duplicate keys and thus makes it implementation defined
        // what order we end up with when we use a custom comparer that never returns 0.
        // Much simpler to just defer sorting to be a pass before dequeuing. Usually queuing and
        // dequeuing each happen in batches.
        // NOTE: We sort in *reverse* order so that we can efficiently pop events off the back
        //       of the list instead of having to take them from the front.
        private readonly List<InputEvent> m_Events = new List<InputEvent>();
        private SortType m_IsSorted = SortType.SortedNormally;

        private enum SortType
        {
            Unsorted,
            SortedNormally,
            SortedByType
        }


        public IEnumerable<InputEvent> events
        {
            get { SortEvents(); return m_Events; }
        }

        private class SortInputEventsByTimeAndType<T>   : IComparer<InputEvent>
        {
            public int Compare(InputEvent event1, InputEvent event2)
            {
                if (event1 is T && event2 is T)
                {
                    var time1 = event1.time;
                    var time2 = event2.time;

                    if (time1 == time2)
                        return 0;
                    else if (time1 > time2)
                        return -1; // inverse
                    else
                        return 1; // inverse
                }
                else if (event1 is T)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }
        private class SortInputEventsByTime : IComparer<InputEvent>
        {
            public int Compare(InputEvent event1, InputEvent event2)
            {
                var time1 = event1.time;
                var time2 = event2.time;

                if (time1 == time2)
                    return 0;
                else if (time1 > time2)
                    return -1; // inverse
                else
                    return 1; // inverse
            }
        }


        private static SortInputEventsByTime s_Comparer = new SortInputEventsByTime();

        private void SortEvents()
        {
            if (m_IsSorted == SortType.SortedNormally)
                return;

            m_Events.Sort(s_Comparer);

            m_IsSorted = SortType.SortedNormally;
        }

        private void SortEventsByTimeAndType<T>()
        {
            if (m_IsSorted == SortType.SortedByType)
                return;

            SortInputEventsByTimeAndType<T> comparer = new SortInputEventsByTimeAndType<T>();
            m_Events.Sort(comparer);

            m_IsSorted = SortType.SortedByType;
        }

        public void Enqueue(InputEvent inputEvent)
        {
            m_Events.Add(inputEvent);
            m_IsSorted = SortType.Unsorted;
        }

        public bool Dequeue(double targetTime, out InputEvent inputEvent)
        {
            SortEvents();

            var eventCount = m_Events.Count;
            if (eventCount == 0)
            {
                inputEvent = null;
                return false;
            }

            var nextEvent = m_Events[eventCount - 1];
            if (nextEvent.time > targetTime)
            {
                inputEvent = null;
                return false;
            }

            m_Events.RemoveAt(eventCount - 1);
            inputEvent = nextEvent;
            return true;
        }

        // T is the event type you wish to dequeue
        public bool DequeueByType<T>(double targetTime, out InputEvent inputEvent)
        {
            SortEventsByTimeAndType<T>();

            var eventCount = m_Events.Count;
            if (eventCount == 0)
            {
                inputEvent = null;
                return false;
            }

            var nextEvent = m_Events[eventCount - 1];
            if (nextEvent.time > targetTime || !(nextEvent is T))
            {
                inputEvent = null;
                return false;
            }

            m_Events.RemoveAt(eventCount - 1);
            inputEvent = nextEvent;
            return true;
        }
    }
}
