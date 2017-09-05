using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    [CreateAssetMenu()]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class ActionMap : ScriptableObject, IControlDomainSource
    {
        [FormerlySerializedAs("entries")]
        [SerializeField]
        private List<InputAction> m_Actions = new List<InputAction>();
        public List<InputAction> actions { get { return m_Actions; } set { m_Actions = value; } }

        [SerializeField]
        private List<ControlScheme> m_ControlSchemes = new List<ControlScheme>();
        public List<ControlScheme> controlSchemes
        {
            get { return m_ControlSchemes; }
            set { m_ControlSchemes = value; }
        }

        public Type mapType
        {
            get
            {
                if (m_CachedMapType == null)
                {
                    if (m_MapTypeName == null)
                        return null;
                    m_CachedMapType = customActionMapType;
                }
                return m_CachedMapType;
            }
            set
            {
                m_CachedMapType = value;
                m_CustomNamespace = m_CachedMapType.Namespace;
                m_MapTypeName = m_CachedMapType.Name;
            }
        }
        [SerializeField]
        private string m_MapTypeName;
        private Type m_CachedMapType;
        public void SetMapTypeName(string name)
        {
            m_MapTypeName = name;
        }

        Type customActionMapType
        {
            get
            {
                Type t = null;

                string typeString = null;
                if (!string.IsNullOrEmpty(m_CustomNamespace))
                {
                    typeString = string.Format(
                            "{0}.{1}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                            m_CustomNamespace, m_MapTypeName);
                    try
                    {
                        t = Type.GetType(typeString);
                    }
                    catch {}
                }

                if (t != null)
                    return t;

                typeString = string.Format(
                        "{0}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                        m_MapTypeName);
                try
                {
                    t = Type.GetType(typeString);
                }
                catch {}

                if (t != null)
                    return t;

                return null;
            }
        }

        [SerializeField]
        private string m_CustomNamespace;
        public string customNamespace
        {
            get
            {
                return m_CustomNamespace;
            }
            set
            {
                m_CustomNamespace = value;
            }
        }

        public void RestoreCustomizations(string customizations)
        {
            var customizedControlSchemes = JsonUtility.FromJson<List<ControlScheme>>(customizations);
            for (int i = 0; i < customizedControlSchemes.Count; i++)
            {
                // See if it replaces an existing scheme.
                var customizedScheme = customizedControlSchemes[i];
                var replacesExisting = false;
                for (var j = 0; j < controlSchemes.Count; j++)
                {
                    if (String.Compare(controlSchemes[j].name, customizedScheme.name, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0)
                    {
                        // Yes, so get rid of current scheme.
                        controlSchemes[j] = customizedScheme;
                        replacesExisting = true;
                        break;
                    }
                }

                if (!replacesExisting)
                {
                    // No, so add as new scheme.
                    controlSchemes.Add(customizedScheme);
                }
            }
        }

        public void EnforceBindingsTypeConsistency()
        {
            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                var action = actions[actionIndex];
                Type controlType = action.controlType;
                Type bindingType = null;
                if (controlType != null)
                {
                    // InputControl > InputControl<T>
                    // We know the selected type is RootBinding<T> or a derived type.
                    // We want to find out what type the T is.
                    // Getting first generic argument doesn't work if the selected type
                    // is a non-generic class that derives from InputControl<T> (with a specific T)
                    // rather than being InputControl<T> itself.
                    // So we need to go down to the InputControl<T> base type first.
                    Type currentType = controlType;
                    while (currentType.BaseType != typeof(InputControl))
                    {
                        currentType = currentType.BaseType;
                        if (currentType == typeof(object))
                            throw new Exception("Selected Control Type does not derive from InputControl");
                    }

                    Type genericArgumentType = currentType.GetGenericArguments()[0];
                    bindingType = typeof(RootBinding<, >).MakeGenericType(new System.Type[] { controlType, genericArgumentType });
                }

                // Scheme bindings.
                for (int schemeIndex = 0; schemeIndex < controlSchemes.Count; schemeIndex++)
                {
                    ControlScheme scheme = controlSchemes[schemeIndex];
                    if (scheme.bindings.Count <= actionIndex)
                    {
                        if (bindingType == null || action.combined)
                            scheme.bindings.Add(null);
                        else
                            scheme.bindings.Add(CreateBinding(bindingType, controlType));
                    }
                    else if (((scheme.bindings[actionIndex] == null) != (bindingType == null || action.combined)) ||
                             (scheme.bindings[actionIndex] != null && scheme.bindings[actionIndex].GetType() != bindingType))
                    {
                        if (bindingType == null || action.combined)
                            scheme.bindings[actionIndex] = null;
                        else
                            scheme.bindings[actionIndex] = CreateBinding(bindingType, controlType);
                    }
                }

                // Self binding.
                if (((action.selfBinding == null) != (bindingType == null || !action.combined)) ||
                    (action.selfBinding != null && action.selfBinding.GetType() != bindingType))
                {
                    if (bindingType == null || !action.combined)
                        action.selfBinding = null;
                    else
                        action.selfBinding = CreateBinding(bindingType, controlType);
                }
            }
        }

        InputBinding CreateBinding(Type bindingType, Type controlType)
        {
            var binding = (InputBinding)Activator.CreateInstance(bindingType);
            bindingType
            .GetMethod("SetReferenceControl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(binding, new object[] { controlType });
            return binding;
        }

        public void ExtractCombinedBindingsOfType<L>(List<L> bindingsList)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action.combined && action.selfBinding != null)
                    action.selfBinding.ExtractBindingsOfType(bindingsList);
            }
        }

        public List<DomainEntry> GetDomainEntries()
        {
            return new List<DomainEntry>() { new DomainEntry() { name = "Self", hash = 0 } };
        }

        public List<DomainEntry> GetControlEntriesOfType(int domainId, Type controlType)
        {
            var entries = new List<DomainEntry>();
            for (int i = 0; i < actions.Count; i++)
            {
                InputAction action = actions[i];
                if (action.combined || action.controlType != controlType)
                    continue;
                entries.Add(new DomainEntry() { name = action.name, hash = action.actionIndex });
            }
            return entries;
        }

        public bool CanInitializeInstanceWithDevices(List<InputDevice> availableDevices)
        {
            int dummyIndex;
            List<InputDevice> dummyList;
            return CanInitializeInstanceWithDevices(availableDevices, out dummyIndex, out dummyList);
        }

        public bool CanInitializeInstanceWithDevices(
            List<InputDevice> availableDevices,
            out int bestControlSchemeIndex,
            out List<InputDevice> bestFoundDevices,
            List<InputDevice> requiredDevices = null,
            int requiredControlSchemeIndex = -1)
        {
            bestControlSchemeIndex = -1;
            bestFoundDevices = null;
            double mostRecentTime = -1;

            int firstScheme = 0;
            int lastScheme = controlSchemes.Count - 1;
            if (requiredControlSchemeIndex >= 0)
            {
                if (requiredControlSchemeIndex > lastScheme)
                    return false;
                firstScheme = lastScheme = requiredControlSchemeIndex;
            }

            List<InputDevice> foundDevices = new List<InputDevice>();
            for (int scheme = firstScheme; scheme <= lastScheme; scheme++)
            {
                double timeForScheme = -1;
                foundDevices.Clear();
                var deviceSlots = controlSchemes[scheme].deviceSlots;
                bool matchesAll = true;
                for (int i = 0; i < deviceSlots.Count; i++)
                {
                    var deviceSlot = deviceSlots[i];
                    InputDevice foundDevice = null;
                    double foundDeviceTime = -1;
                    for (int j = 0; j < availableDevices.Count; j++)
                    {
                        var device = availableDevices[j];
                        if (!deviceSlot.IsDeviceCompatible(device))
                            continue;
                        bool required = (requiredDevices != null && requiredDevices.Contains(device));
                        if (required || device.lastEventTime > foundDeviceTime)
                        {
                            foundDevice = device;
                            foundDeviceTime = device.lastEventTime;
                            if (required)
                                break;
                        }
                    }
                    if (foundDevice != null)
                    {
                        foundDevices.Add(foundDevice);
                        timeForScheme = Math.Max(timeForScheme, foundDeviceTime);
                    }
                    else
                    {
                        matchesAll = false;
                        break;
                    }
                }

                // Don't switch schemes in the case where we require a specific device for an event that is getting processed.
                if (matchesAll && requiredDevices != null && requiredDevices.Count > 0)
                {
                    for (int i = 0; i < requiredDevices.Count; i++)
                    {
                        if (!foundDevices.Contains(requiredDevices[i]))
                        {
                            matchesAll = false;
                            break;
                        }
                    }
                }

                if (!matchesAll)
                    continue;

                // If we reach this point we know that control scheme both matches required and matches all.
                if (timeForScheme > mostRecentTime)
                {
                    bestControlSchemeIndex = scheme;
                    bestFoundDevices = new List<InputDevice>(foundDevices);
                    mostRecentTime = timeForScheme;
                }
            }

            return (bestControlSchemeIndex != -1);
        }
    }
}
