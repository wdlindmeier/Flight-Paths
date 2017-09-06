namespace UnityEngine.Experimental.Input
{
    public class InputHandlerCallback : IInputHandler
    {
        public delegate bool ProcessInputDelegate(InputEvent inputEvent);
        public delegate void UpdateDelegate();

        public ProcessInputDelegate processEvent { get; set; }

        public UpdateDelegate beginUpdate { get; set; }

        public UpdateDelegate endUpdate { get; set; }

        public bool ProcessEvent(InputEvent inputEvent)
        {
            if (processEvent != null)
                return processEvent(inputEvent);
            return false;
        }

        public void BeginUpdate()
        {
            if (beginUpdate != null)
                beginUpdate();
        }

        public void EndUpdate()
        {
            if (endUpdate != null)
                endUpdate();
        }
    }
}
