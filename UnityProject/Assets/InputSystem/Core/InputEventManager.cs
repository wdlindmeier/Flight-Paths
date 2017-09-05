using System;

namespace UnityEngine.Experimental.Input
{
    [Serializable]
    internal class InputEventManager : IInputEventManager
    {
        [SerializeField]
        private InputEventQueue m_Queue = new InputEventQueue();
        public InputEventQueue queue { get { return m_Queue; } }
        IInputEventQueue IInputEventManager.queue { get { return queue; } }

        // Not serialized. Pool will start cold after each domain load.
        private InputEventPool m_Pool = new InputEventPool();
        public InputEventPool pool { get { return m_Pool; } }
        IInputEventPool IInputEventManager.pool { get { return pool; } }

        // Not serialized. Handlers will have to re-register themselves.
        public InputHandlerNode handlerRoot { get; private set; }

        // TODO: Review. Move these somewhere else so they're not part of core system?
        public InputHandlerNode rewriters { get; private set; }
        public InputHandlerNode consumers { get; private set; }
        public InputHandlerNode assignedPlayers { get; private set; }
        public InputHandlerNode globalPlayers { get; private set; }

        public InputEventManager()
        {
            handlerRoot = new InputHandlerNode();

            rewriters = new InputHandlerNode();
            handlerRoot.children.Add(rewriters);

            consumers = new InputHandlerNode();
            handlerRoot.children.Add(consumers);

            assignedPlayers = new InputHandlerNode();
            consumers.children.Add(assignedPlayers);

            // Global consumers should be processed last.
            globalPlayers = new InputHandlerNode();
            consumers.children.Add(globalPlayers);
        }

        public void ExecuteEvents(double upToAndIncludingTime)
        {
            var processedEventCount = 0;

            InputEvent inputEvent;
            while (m_Queue.Dequeue(upToAndIncludingTime, out inputEvent))
            {
                handlerRoot.ProcessEvent(inputEvent);

                // We return the event to the pool regardless of whether the handler signaled it got processed. Might be
                // simply no one was interested in the event. We don't want to be leaking here, though.
                pool.Return(inputEvent);

                ++processedEventCount;
            }
        }

        public void ExecuteEventsByType<T>(double upToAndIncludingTime)
        {
            var processedEventCount = 0;
            var playerHasFocus = Application.isFocused;

            InputEvent inputEvent;
            while (m_Queue.DequeueByType<T>(upToAndIncludingTime, out inputEvent))
            {
                if (!playerHasFocus || handlerRoot.ProcessEvent(inputEvent))
                    pool.Return(inputEvent);
                ++processedEventCount;
            }
        }
    }
}
