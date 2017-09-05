using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.Experimental.Input
{
    public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum ButtonOption
        {
            Action1,
            Action2,
            Action3,
            Action4
        }

        public ButtonOption m_ButtonControl = ButtonOption.Action1;

        public void OnPointerUp(PointerEventData data)
        {
            Gamepad gamepad = VirtualDeviceManager.GetDevice<Gamepad>();
            ButtonControl control;
            switch (m_ButtonControl)
            {
                case ButtonOption.Action1: control = gamepad.action1; break;
                case ButtonOption.Action2: control = gamepad.action2; break;
                case ButtonOption.Action3: control = gamepad.action3; break;
                case ButtonOption.Action4: control = gamepad.action4; break;
                default: return;
            }
            VirtualDeviceManager.SendValueToControl(control, 0);
        }

        public void OnPointerDown(PointerEventData data)
        {
            Gamepad gamepad = VirtualDeviceManager.GetDevice<Gamepad>();
            ButtonControl control;
            switch (m_ButtonControl)
            {
                case ButtonOption.Action1: control = gamepad.action1; break;
                case ButtonOption.Action2: control = gamepad.action2; break;
                case ButtonOption.Action3: control = gamepad.action3; break;
                case ButtonOption.Action4: control = gamepad.action4; break;
                default: return;
            }
            VirtualDeviceManager.SendValueToControl(control, 1);
        }
    }
}
