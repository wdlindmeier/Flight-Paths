using System;
using System.Collections.Generic;
using Assets.Utilities;
using UnityEngine;
using UnityEngineInternal.Input;

namespace UnityEngine.Experimental.Input
{
    public class Keyboard : InputDevice
    {
        public static Keyboard current { get { return InputSystem.GetCurrentDeviceOfType<Keyboard>(); } }

        public Action<char> onCharacterReceived;

        public override void AddStandardControls(ControlSetup setup)
        {
            int controlCount = EnumHelpers.GetValueCount<KeyCode>();
            for (var i = 0; i < controlCount; ++i)
            {
                setup.AddControl(SupportedControl.Get<ButtonControl>(((KeyCode)i).ToString()));
            }

            // Make sure any additional controls come *after* the keys as we map directly
            // from KeyCode indices to control indices.
        }

        public override bool ProcessEventIntoState(InputEvent inputEvent, InputState intoState)
        {
            var keyEvent = inputEvent as KeyEvent;
            if (keyEvent != null)
            {
                var control = intoState.controls[(int)keyEvent.key] as ButtonControl;
                if (!control.enabled)
                    return false;

                control.SetValueFromEvent(keyEvent.isDown ? 1 : 0);
                return true;
            }

            var textEvent = inputEvent as TextEvent;
            if (textEvent != null)
            {
                if (onCharacterReceived != null)
                    onCharacterReceived(textEvent.text);
                return true;
            }

            return base.ProcessEventIntoState(inputEvent, intoState);
        }

        // Return the identifier of the current keyboard layout. The identifier is made up
        // of a locale name (like "en_US") and optionally a qualifier in parenthesis.
        // Example: "uk_UA" for Ukranian and "uk_UA (Extended)" for extended layout.
        public virtual string GetCurrentLayout()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool GetKeyCodeInfoForCurrentLayout(KeyCode key, out KeyCodeInfo info)
        {
            if (nativeId != 0)
            {
                var controlIndex = (int)key;
                var controlConfiguration = NativeInputSystem.GetControlConfiguration(nativeId, controlIndex);

                if (!string.IsNullOrEmpty(controlConfiguration))
                {
                    // Have to go through boxing to accomodate the JsonUtility API.
                    object keyCodeInfo = new KeyCodeInfo();
                    JsonUtility.FromJsonOverwrite(controlConfiguration, keyCodeInfo);
                    info = (KeyCodeInfo)keyCodeInfo;
                    return true;
                }
            }

            info = new KeyCodeInfo();
            return false;
        }

        public ButtonControl GetControl(KeyCode keyCode)
        {
            return (ButtonControl)GetControl((int)keyCode);
        }
    }
}
