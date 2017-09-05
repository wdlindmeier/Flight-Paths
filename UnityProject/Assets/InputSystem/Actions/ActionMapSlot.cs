using System;

namespace UnityEngine.Experimental.Input
{
    [Serializable]
    public class ActionMapSlot
    {
        public ActionMap actionMap;
        public bool active = true;
        public bool blockSubsequent;
    }
}
