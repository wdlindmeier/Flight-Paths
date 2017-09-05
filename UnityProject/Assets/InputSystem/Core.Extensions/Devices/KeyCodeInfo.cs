using UnityEngine;

////TODO: copy KeyCode enum to HL system and kill off anything that's not from the keyboard

namespace UnityEngine.Experimental.Input
{
    ////REVIEW: this API here (along with the method on Keyboard) has not really been tried out in practice

    // Provides detailed information about a key on a keyboard.
    // See Keyboard.GetKeyCodeInfoForCurrentLayout.
    public struct KeyCodeInfo
    {
        // Virtual key code that uniquely refers to this physical key. This is what is reported
        // by "KeyboardEvent.key".
        public KeyCode keyCode;

        // Platform-dependent scan code used to refer to the key.
        public int scanCode;

        // Symbol that is on the key according to the current keyboard layout.
        //
        // For example, for KeyCode.A and a French keyboard layout, symbol would be "q". For
        // KeyCode.Enter and the same layout, it would be "Entrée". And for KeyCode.Alpha1, it
        // would be "&".
        public string symbol;

        // Symbol that is on the key according to the current keyboard layout when used in
        // combination with the shift key. Same as "symbol" if the key has no specific combination
        // with the shift key.
        //
        // For example, for KeyCode.A and a French keyboard layout, symbol would be "Q". For
        // KeyCode.Space, this would be "Space". And for KeyCode.Alpha1, it would be 1.
        public string shiftSymbol;

        // Symbol that is on the key according to the current keyboard layout when used in
        // combination with the Alt/AltGr key. Only applicable if the key has a tertiary mapping
        // like e.g. German keyboard layouts have. Same as "symbol" if the key does not have such
        // a mapping.
        public string altSymbol;
    }
}
