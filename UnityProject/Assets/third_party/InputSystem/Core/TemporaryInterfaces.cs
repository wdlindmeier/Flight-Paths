////TODO: Right now, we don't have the ability to isolate C# code that references concrete implementations
////  directly. The custom NSubstitute fork that's being worked on will solve that. When that is ready,
////  these interface should be killed and the tests should do Substitute.For<> directly on the concrete
////  implementations.

namespace UnityEngine.Experimental.Input
{
    internal interface IInputEventQueue
    {
        void Enqueue(InputEvent inputEvent);
        bool Dequeue(double targetTime, out InputEvent inputEvent);
    }

    internal interface INativeInputDeviceManager
    {
        InputDevice FindInputDeviceByNativeDeviceId(int nativeDeviceId);
    }

    internal interface IInputEventPool
    {
        TEvent ReuseOrCreate<TEvent>() where TEvent : InputEvent, new();
        void Return(InputEvent inputEvent);
    }

    internal interface IInputEventManager
    {
        IInputEventPool pool { get; }
        IInputEventQueue queue { get; }

        void ExecuteEvents(double upToAndIncludingTime);
    }
}
