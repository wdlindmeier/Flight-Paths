namespace UnityEngine.Experimental.Input
{
    public interface IInputHandler
    {
        bool ProcessEvent(InputEvent inputEvent);
        void BeginUpdate();
        void EndUpdate();
    }
}
