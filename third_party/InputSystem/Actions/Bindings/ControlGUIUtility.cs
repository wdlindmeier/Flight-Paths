#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace UnityEngine.Experimental.Input
{
    public class ControlGUIUtility
    {
        static InputSystem.BindingListener s_BindingListener;
        static InputBinding s_Binding;
        static EditorApplication.CallbackFunction s_UpdateCallback;

        public static void ControlField<C, T>(Rect position, InputBinding<C, T> binding, GUIContent label, IControlDomainSource domainSource, Action<InputBinding<C, T>> action, SerializedProperty prop = null) where C : InputControl<T>
        {
            if (prop != null)
                label = EditorGUI.BeginProperty(position, label, prop);

            position.height = EditorGUIUtility.singleLineHeight;

            Rect buttonPosition = EditorGUI.PrefixLabel(position, label);
            Rect detectPosition = buttonPosition;

            ControlScheme scheme = domainSource as ControlScheme;
            bool detectionSupport = (scheme != null);
            if (detectionSupport)
            {
                detectPosition.xMin = detectPosition.xMax - 20;
                buttonPosition.xMax -= (20 + 4);
            }

            if (EditorGUI.DropdownButton(buttonPosition, new GUIContent(GetName(binding, domainSource) ?? "None"), FocusType.Keyboard))
            {
                GenericMenu menu = GetMenu(
                        binding, domainSource,
                        a =>
                    {
                        if (prop != null)
                            Undo.RecordObjects(prop.serializedObject.targetObjects, "Control");

                        action(a);

                        if (prop != null)
                        {
                            // Flushing seems necessaary to have prefab property override status change without lag.
                            Undo.FlushUndoRecordObjects();
                            prop.serializedObject.SetIsDifferentCacheDirty();
                        }
                    });
                menu.DropDown(buttonPosition);

                // GenericMenu doesn't modify GUI.changed because it relies on a callback, so let's assume that something was changed, so target object dirtying doesn't go missing
                GUI.changed = true;
            }

            if (detectionSupport)
            {
                EditorGUI.BeginDisabledGroup(s_Binding != null);
                //if (Event.current.type == EventType.repaint)
                //    EditorStyles.miniButton.Draw(detectPosition, "O", false, false, s_Binding == binding, false);
                if (GUI.Toggle(detectPosition, s_Binding == binding, "O", EditorStyles.miniButton) && s_Binding == null)
                {
                    EditorWindow window = EditorWindow.focusedWindow;
                    window.ShowNotification(new GUIContent("Waiting for input."));
                    s_BindingListener = (InputControl control) =>
                        {
                            if (!(control is C))
                            {
                                window.ShowNotification(new GUIContent("Incompatible control type."));
                                window.Repaint();
                                return false;
                            }

                            DeviceSlot match = null;
                            Type controlProviderType = control.provider.GetType();
                            for (int slotIndex = 0; slotIndex < scheme.deviceSlots.Count; slotIndex++)
                            {
                                Type deviceType = scheme.deviceSlots[slotIndex].type.value;
                                if (deviceType == null)
                                    continue;
                                if (deviceType.IsAssignableFrom(controlProviderType))
                                {
                                    match = scheme.deviceSlots[slotIndex];
                                    break;
                                }
                            }
                            if (match == null)
                            {
                                window.ShowNotification(new GUIContent("Incompatible device type."));
                                window.Repaint();
                                return false;
                            }

                            var newReference = new ControlReferenceBinding<C, T>();
                            newReference.deviceKey = match.key;
                            newReference.controlHash = control.provider.GetHashForControlIndex(control.index);
                            action(newReference);

                            StopListening(window);

                            return true;
                        };
                    InputSystem.ListenForBinding(s_BindingListener);
                    s_Binding = binding;

                    float time = Time.realtimeSinceStartup;
                    s_UpdateCallback = () => {
                            if (Time.realtimeSinceStartup > time + 3)
                            {
                                StopListening(window);
                            }
                        };
                    EditorApplication.update += s_UpdateCallback;
                }
                EditorGUI.EndDisabledGroup();
            }

            if (binding != null && !(binding is ControlReferenceBinding<C, T>))
            {
                EditorGUI.indentLevel++;
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                binding.OnGUI(position, domainSource);
                EditorGUI.indentLevel--;
            }

            if (prop != null)
                EditorGUI.EndProperty();
        }

        static void StopListening(EditorWindow window)
        {
            EditorApplication.update -= s_UpdateCallback;
            InputSystem.ListenForBinding(s_BindingListener, false);

            s_BindingListener = null;
            s_Binding = null;
            s_UpdateCallback = null;

            window.RemoveNotification();
            window.Repaint();
        }

        static string GetName<C, T>(InputBinding<C, T> binding, IControlDomainSource domainSource) where C : InputControl<T>
        {
            if (binding == null || domainSource == null)
                return null;

            var reference = binding as ControlReferenceBinding<C, T>;
            if (reference != null)
            {
                List<DomainEntry> domainEntries = domainSource.GetDomainEntries();
                int domainIndex = domainEntries.FindIndex(e => e.hash == reference.deviceKey);
                if (domainIndex < 0)
                    return null;

                DomainEntry domain = domainEntries[domainIndex];
                string domainName = domain.name;
                if (!string.IsNullOrEmpty(domainName))
                    domainName = domainName + "/";

                List<DomainEntry> controlEntries = domainSource.GetControlEntriesOfType(domain.hash, typeof(C));
                int controlIndex = controlEntries.FindIndex(e => e.hash == reference.controlHash);
                if (controlIndex < 0)
                    return null;

                return domainName + controlEntries[controlIndex].name;
            }

            return NicifyBindingName(binding.GetType().Name);
        }

        static GenericMenu GetMenu<C, T>(InputBinding<C, T> binding, IControlDomainSource domainSource, Action<InputBinding<C, T>> action) where C : InputControl<T>
        {
            GenericMenu menu = new GenericMenu();

            Type[] derivedTypes = null;
            string[] derivedNames = null;
            Dictionary<Type, int> indicesOfDerivedTypes = null;
            TypeGUI.GetDerivedTypesInfo(typeof(InputBinding<C, T>), out derivedTypes, out derivedNames, out indicesOfDerivedTypes);

            Type bindingType = typeof(ControlReferenceBinding<C, T>);
            Type existingType = binding == null ? null : binding.GetType();

            var reference = binding as ControlReferenceBinding<C, T>;

            // Add control references for devices.
            bool hasReferences = false;
            if (derivedTypes.Contains(bindingType))
            {
                hasReferences = true;
                List<DomainEntry> domainEntries = domainSource.GetDomainEntries();
                for (int i = 0; i < domainEntries.Count; i++)
                {
                    int domainHash = domainEntries[i].hash;
                    List<DomainEntry> controlEntries = domainSource.GetControlEntriesOfType(domainHash, typeof(C));

                    bool showFlatList = (domainEntries.Count <= 1 && controlEntries.Count <= 20);
                    string prefix = showFlatList ? string.Empty : domainEntries[i].name + "/";

                    bool nonStandardizedSectionStart = false;
                    for (int j = 0; j < controlEntries.Count; j++)
                    {
                        bool selected = (reference != null
                                         && reference.deviceKey == domainHash
                                         && reference.controlHash == controlEntries[j].hash);

                        if (!nonStandardizedSectionStart && !controlEntries[j].standardized)
                        {
                            nonStandardizedSectionStart = true;
                            menu.AddSeparator(prefix);
                        }

                        GUIContent name = new GUIContent(prefix + controlEntries[j].name);
                        int index = j; // See "close over the loop variable".
                        menu.AddItem(name, selected,
                            () => {
                                var newReference = new ControlReferenceBinding<C, T>();
                                newReference.deviceKey = domainHash;
                                newReference.controlHash = controlEntries[index].hash;
                                action(newReference);
                            });
                    }
                }
            }

            if (derivedTypes.Length <= (hasReferences ? 1 : 0))
                return menu;

            menu.AddSeparator("");

            // Add other control types.
            for (int i = 0; i < derivedTypes.Length; i++)
            {
                if (derivedTypes[i] != bindingType)
                {
                    bool selected = (existingType == derivedTypes[i]);
                    string name = NicifyBindingName(derivedNames[i]);
                    int index = i; // See "close over the loop variable".
                    menu.AddItem(new GUIContent(name), selected,
                        () => {
                            var newBinding = Activator.CreateInstance(derivedTypes[index]) as InputBinding<C, T>;
                            action(newBinding);
                        });
                }
            }

            return menu;
        }

        static string NicifyBindingName(string bindingTypeName)
        {
            return ObjectNames.NicifyVariableName(bindingTypeName.Replace("Binding", string.Empty));
        }

        public static float GetControlHeight<C, T>(InputBinding<C, T> control, GUIContent label) where C : InputControl<T>
        {
            if (control == null || control is ControlReferenceBinding<C, T>)
                return EditorGUIUtility.singleLineHeight;
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + control.GetPropertyHeight();
        }
    }
}
#endif
