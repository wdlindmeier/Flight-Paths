namespace UnityEngine.Experimental.Input
{
    public class PlayerDeviceAssignment
    {
        public readonly PlayerHandle player;
        public readonly InputDevice device;

        public PlayerDeviceAssignment(PlayerHandle playerHandle, InputDevice device)
        {
            this.device = device;
            this.player = playerHandle;
        }

        public void Assign()
        {
            player.assignments.Add(this);
            device.SetAssignment(this);

            if (PlayerHandle.onChanged != null)
                PlayerHandle.onChanged();
        }

        public void Unassign()
        {
            player.assignments.Remove(this);
            device.SetAssignment(null);

            if (PlayerHandle.onChanged != null)
                PlayerHandle.onChanged();
        }
    }
}
