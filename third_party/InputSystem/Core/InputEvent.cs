namespace UnityEngine.Experimental.Input
{
    public class InputEvent
    {
        public double time { get; set; }
        public InputDevice device { get; set; }

        public virtual void Reset()
        {
            time = 0;
            device = null;
        }

        public virtual InputEvent Clone()
        {
            return (InputEvent)MemberwiseClone();
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
