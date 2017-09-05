using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Assets.Utilities;

#if UNITY_EDITOR
using System.Linq;
#endif

namespace UnityEngine.Experimental.Input
{
    [Serializable]
    public sealed class ControlScheme : ISerializationCallbackReceiver, IControlDomainSource, ICloneable
    {
        [SerializeField]
        private string m_Name;
        public string name { get { return m_Name; } set { m_Name = value; } }

        [SerializeField]
        private List<DeviceSlot> m_DeviceSlots = new List<DeviceSlot>();
        public List<DeviceSlot> deviceSlots { get { return m_DeviceSlots; } set { m_DeviceSlots = value; } }

        [SerializeField]
        private ActionMap m_ActionMap;
        public ActionMap actionMap { get { return m_ActionMap; } }

        [NonSerialized]
        private List<InputBinding> m_Bindings = new List<InputBinding>();
        public List<InputBinding> bindings { get { return m_Bindings; } set { m_Bindings = value; } }
        [SerializeField]
        List<SerializationHelper.JSONSerializedElement> m_SerializableBindings = new List<SerializationHelper.JSONSerializedElement>();

        public bool customized { get; internal set; }

        public void OnBeforeSerialize()
        {
            m_SerializableBindings = SerializationHelper.Serialize(m_Bindings);
        }

        public void OnAfterDeserialize()
        {
            m_Bindings = SerializationHelper.Deserialize<InputBinding>(m_SerializableBindings, new object[] {});
            m_SerializableBindings = null;
        }

        public ControlScheme()
        {
        }

        public ControlScheme(string name, ActionMap actionMap)
        {
            m_Name = name;
            m_ActionMap = actionMap;
        }

        public object Clone()
        {
            var clone = new ControlScheme();
            clone.m_Name = m_Name;
            clone.m_DeviceSlots = m_DeviceSlots.SemiDeepClone();
            clone.m_ActionMap = m_ActionMap;
            clone.m_Bindings = m_Bindings.SemiDeepClone();
            // Don't clone customized flag.
            return clone;
        }

        public int GetDeviceKey(InputDevice device)
        {
            for (int i = 0; i < m_DeviceSlots.Count; i++)
            {
                var deviceSlot = m_DeviceSlots[i];
                if (device.GetType().IsInstanceOfType(deviceSlot.type.value) &&
                    (device.tagIndex == -1 || device.tagIndex == deviceSlot.tagIndex))
                    return deviceSlot.key;
            }

            return DeviceSlot.kInvalidKey;
        }

        public DeviceSlot GetDeviceSlot(int key)
        {
            for (int i = 0; i < m_DeviceSlots.Count; i++)
            {
                var deviceSlot = m_DeviceSlots[i];
                if (deviceSlot.key == key)
                    return deviceSlot;
            }

            return null;
        }

        public void ExtractDeviceTypesAndControlHashes(Dictionary<int, List<int>> controlIndicesPerDeviceType)
        {
            List<IEndBinding> endBindings = new List<IEndBinding>();
            ExtractBindingsOfType<IEndBinding>(endBindings);
            for (int i = 0; i < endBindings.Count; i++)
            {
                var binding = endBindings[i];
                binding.ExtractDeviceTypesAndControlHashes(controlIndicesPerDeviceType);
            }
        }

        public void ExtractBindingsOfType<L>(List<L> bindingsList)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding != null)
                    binding.ExtractBindingsOfType(bindingsList);
            }
        }

        public void ExtractLabeledEndBindings(List<LabeledBinding> endBindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (bindings[i] != null)
                    bindings[i].ExtractLabeledEndBindings(actionMap.actions[i].name, endBindings);
            }
        }

        public void Initialize(IInputStateProvider stateProvider)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (bindings[i] != null)
                    bindings[i].Initialize(stateProvider);
            }
        }

        public List<DomainEntry> GetDomainEntries()
        {
            var entries = new List<DomainEntry>(deviceSlots.Count);
            for (int i = 0; i < deviceSlots.Count; i++)
            {
                DeviceSlot slot = deviceSlots[i];
                if (slot == null)
                    entries.Add(new DomainEntry() { name = string.Empty, hash = -1 });
                else
                    entries.Add(new DomainEntry() { name = slot.ToString(), hash = slot.key });
            }
            return entries;
        }

        public List<DomainEntry> GetControlEntriesOfType(int domainId, Type controlType)
        {
            DeviceSlot slot = GetDeviceSlot(domainId);
            return InputDeviceUtility.GetDeviceControlEntriesOfType(slot == null ? null : slot.type, controlType);
        }

        #if UNITY_EDITOR
        public void UpdateUsedControlHashes()
        {
            // Gather a mapping of device types to list of bindings that use the given type.
            var perDeviceTypeUsedControlIndices = new Dictionary<int, List<int>>();
            ExtractDeviceTypesAndControlHashes(perDeviceTypeUsedControlIndices);

            for (int i = 0; i < deviceSlots.Count; i++)
            {
                var deviceSlot = deviceSlots[i];
                List<int> hashes;
                if (perDeviceTypeUsedControlIndices.TryGetValue(deviceSlot.key, out hashes))
                {
                    hashes = hashes.Distinct().ToList();
                    hashes.Sort();
                    deviceSlot.SetSortedCachedUsedControlHashes(hashes);
                }
            }
        }

        #endif
    }
}
