using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Experimental.Input
{
    [Serializable]
    public class DeviceSlot : ICloneable
    {
        public static readonly int kInvalidKey = -1;
        const string k_InvalidSlotString = "Invalid Device Slot";

        [SerializeField]
        private int m_Key = kInvalidKey;
        public int key
        {
            get { return m_Key; }
            set { m_Key = value; }
        }

        [SerializeField]
        private SerializableType m_Type;
        public SerializableType type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

        [SerializeField]
        private int m_TagIndex = -1;
        public int tagIndex
        {
            get { return m_TagIndex; }
            set { m_TagIndex = value; }
        }

        [SerializeField]
        private List<int> m_SortedCachedUsedControlHashes;

        public object Clone()
        {
            var clone = new DeviceSlot();
            clone.m_Key = m_Key;
            clone.m_TagIndex = m_TagIndex;
            clone.m_Type = m_Type;
            clone.m_SortedCachedUsedControlHashes = new List<int>(m_SortedCachedUsedControlHashes);

            return clone;
        }

        public override string ToString()
        {
            if (type.value == null)
                return k_InvalidSlotString;

            if (tagIndex == -1)
                return type.name;

            List<string> tags = InputDeviceUtility.GetDeviceTags(type);
            return string.Format("{0}.{1}", type.name, tags[tagIndex]);
        }

        public bool IsDeviceCompatible(InputDevice device)
        {
            if (tagIndex != -1 && tagIndex != device.tagIndex)
                return false;

            // If a device has been setup but no bindings using it, match by type instead.
            if (m_SortedCachedUsedControlHashes.Count == 0 && !type.value.IsInstanceOfType(device))
                return false;

            // Match by supported controls.
            int supportScore = device.GetSupportScoreForSupportedControlHashes(m_SortedCachedUsedControlHashes);
            if (supportScore < 0)
                return false;

            return true;
        }

        public void SetSortedCachedUsedControlHashes(List<int> sortedHashes)
        {
            m_SortedCachedUsedControlHashes = sortedHashes;
        }

        #if UNITY_EDITOR
        public void OnGUI(Rect rect, GUIContent label, Type baseType = null, SerializedProperty prop = null)
        {
            if (prop != null)
                label = EditorGUI.BeginProperty(rect, label, prop);

            if (baseType == null)
                baseType = typeof(InputDevice);

            List<string> tagNames = null;
            Vector2 tagMaxSize = Vector2.zero;
            if (type.value != null)
            {
                tagNames = InputDeviceUtility.GetDeviceTags(type.value);
                if (tagNames != null)
                {
                    GUIContent content = new GUIContent();
                    for (var j = 0; j < tagNames.Count; j++)
                    {
                        content.text = tagNames[j];
                        Vector2 size = EditorStyles.popup.CalcSize(content);
                        tagMaxSize = Vector2.Max(size, tagMaxSize);
                    }
                }
            }

            rect.width -= tagMaxSize.x; // Adjust width to leave room for tag
            EditorGUI.BeginChangeCheck();
            Type t = TypeGUI.TypeField(rect, label, baseType, type);
            if (EditorGUI.EndChangeCheck())
            {
                if (prop != null)
                    Undo.RecordObjects(prop.serializedObject.targetObjects, "Input Device");

                type = t;

                if (prop != null)
                {
                    // Flushing seems necessaary to have prefab property override status change without lag.
                    Undo.FlushUndoRecordObjects();
                    prop.serializedObject.SetIsDifferentCacheDirty();
                }
            }
            if (tagNames != null)
            {
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.BeginChangeCheck();
                // We want to have the ability to unset a tag after specifying one, so add an "Any" option
                var popupTags = new string[tagNames.Count + 1];
                popupTags[0] = "Any";
                tagNames.CopyTo(popupTags, 1);
                int newTagIndex = tagIndex + 1;
                rect.x += rect.width;
                rect.width = tagMaxSize.x;
                newTagIndex = EditorGUI.Popup(
                        rect,
                        newTagIndex,
                        popupTags);
                if (EditorGUI.EndChangeCheck())
                {
                    if (prop != null)
                        Undo.RecordObjects(prop.serializedObject.targetObjects, "Control");

                    tagIndex = newTagIndex - 1;

                    if (prop != null)
                    {
                        // Flushing seems necessaary to have prefab property override status change without lag.
                        Undo.FlushUndoRecordObjects();
                        prop.serializedObject.SetIsDifferentCacheDirty();
                    }
                }

                EditorGUI.indentLevel = oldIndent;
            }
            else
            {
                // if we're no longer showing tags, reset the tag field so that we can still search on the current
                // tag selection of the device slot.
                tagIndex = -1;
            }

            if (prop != null)
                EditorGUI.EndProperty();
        }

        #endif
    }
}
