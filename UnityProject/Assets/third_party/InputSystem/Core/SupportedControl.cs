namespace UnityEngine.Experimental.Input
{
    [System.Serializable]
    public struct SupportedControl
    {
        public SerializableType controlType;
        public string standardName;
        public int hash;

        // *begin-nonstandard-formatting*
        public static readonly SupportedControl None = new SupportedControl() { standardName = "None", hash = -1 };
        // *end-nonstandard-formatting*

        private SupportedControl(System.Type type, string standardName)
        {
            this.controlType = type;
            this.standardName = standardName;
            hash = standardName.GetHashCode() ^ type.Name.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hash;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", standardName, controlType.name);
        }

        public static SupportedControl Get<T>(string standardName) where T : InputControl
        {
            return new SupportedControl(typeof(T), standardName);
        }
    }
}
