using System;
using UnityEngine;

namespace UnityEngine.Experimental.Input
{
    // We need a non-generic base class so that we can create a PropertyDrawer for it.
    public abstract class ActionSlot {}

    public class ActionSlot<T> : ActionSlot where T : InputControl
    {
        public InputAction action;
        public T control;

        public void Bind(PlayerHandle player)
        {
            ActionMapInput map = player.GetActions(action.actionMap);
            control = map.GetControl(action.actionIndex) as T;
        }
    }
}
