namespace UnityEngine.Experimental.Input
{
    // All device tags that make sense to generalize across multiple devices should preferably be put in this class.
    public static class CommonDeviceTags
    {
        // None should not be supported by any devices but be the fallback value.
        public const string None = "None";

        public const string Left = "Left";
        public const string Right = "Right";
    }
}
