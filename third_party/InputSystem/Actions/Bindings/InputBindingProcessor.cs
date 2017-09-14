using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    // Only InputBindingProcessor<T> should inherit from InputBindingProcessor.
    public abstract class InputBindingProcessor : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        #if UNITY_EDITOR
        public abstract void OnGUI(Rect position);
        public abstract float GetPropertyHeight();
        #endif
    }

    public abstract class InputBindingProcessor<C, T> : InputBindingProcessor where C : InputControl<T>
    {
        public abstract T ProcessValue(C control, T newValue);

        #if UNITY_EDITOR
        public static void ShowAddProcessorDropdown(Rect position, Action<InputBindingProcessor<C, T>> action)
        {
            Type baseType = typeof(InputBindingProcessor<C, T>);
            Type[] derivedTypes = null;
            string[] derivedNames = null;
            Dictionary<Type, int> indicesOfDerivedTypes = null;
            TypeGUI.GetDerivedTypesInfo(baseType, out derivedTypes, out derivedNames, out indicesOfDerivedTypes);

            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < derivedNames.Length; i++)
            {
                int index = i; //
                menu.AddItem(
                    new GUIContent(derivedNames[index]),
                    false,
                    () => action(Activator.CreateInstance(derivedTypes[index]) as InputBindingProcessor<C, T>));
            }

            menu.DropDown(position);
        }

        #endif
    }
}
