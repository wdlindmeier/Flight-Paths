using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Experimental.Input;

namespace UnityEngine.Experimental.Input.Spatial
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrackedPoseDriver))]
    internal class TrackedPoseDriverEditor : Editor, IControlDomainSource
    {
        static class Styles
        {
            public static GUIContent trackingSourceLabel = new GUIContent("Tracking Source");
            public static GUIContent deviceLabel = new GUIContent("Device Type");
            public static GUIContent controlLabel = new GUIContent("Control");
            public static GUIContent poseActionLabel = new GUIContent("Action");
            public static GUIContent playerHandleProviderLabel = new GUIContent("Player Handle Provider");
            public static GUIContent trackingLabel = new GUIContent("Tracking Type");
            public static GUIContent updateLabel = new GUIContent("Update Type");
            public static GUIContent relativeLabel = new GUIContent("Use Relative Transform");
        }

        SerializedProperty m_TrackingSourceProp = null;
        SerializedProperty m_PlayerHandleProviderProp = null;
        SerializedProperty m_PoseActionProp = null;
        SerializedProperty m_DeviceSlotProp = null;
        SerializedProperty m_BindingProp = null;
        SerializedProperty m_TrackingTypeProp = null;
        SerializedProperty m_UpdateTypeProp = null;
        SerializedProperty m_UseRelativeTransformProp = null;

        TrackedPoseDriver m_TrackedPoseDriver;

        void OnEnable()
        {
            m_TrackedPoseDriver = target as TrackedPoseDriver;

            m_TrackingSourceProp = this.serializedObject.FindProperty("m_TrackingSource");
            m_PoseActionProp = this.serializedObject.FindProperty("m_PoseAction");
            m_PlayerHandleProviderProp = this.serializedObject.FindProperty("m_playerHandleProvider");
            m_DeviceSlotProp = this.serializedObject.FindProperty("m_DeviceSlot");
            m_BindingProp = this.serializedObject.FindProperty("m_SerializedBinding");
            m_TrackingTypeProp = this.serializedObject.FindProperty("m_TrackingType");
            m_UpdateTypeProp = this.serializedObject.FindProperty("m_UpdateType");
            m_UseRelativeTransformProp = this.serializedObject.FindProperty("m_UseRelativeTransform");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_TrackingSourceProp, Styles.trackingSourceLabel);
            if (m_TrackingSourceProp.enumValueIndex == (int)TrackedPoseDriver.TrackingSource.Device)
            {
                float height = ControlGUIUtility.GetControlHeight(m_TrackedPoseDriver.binding, Styles.controlLabel);
                Rect rect = EditorGUILayout.GetControlRect(true, height);
                EditorGUI.BeginChangeCheck();

                m_TrackedPoseDriver.deviceSlot.OnGUI(
                    rect,
                    Styles.deviceLabel,
                    typeof(TrackedInputDevice),
                    m_DeviceSlotProp);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (targets[i] != m_TrackedPoseDriver)
                            (targets[i] as TrackedPoseDriver).deviceSlot =
                                (DeviceSlot)m_TrackedPoseDriver.deviceSlot.Clone();
                    }
                    // If control is not set, set it to standard pose control by default.
                    for (int i = 0; i < targets.Length; i++)
                    {
                        var binding = ((TrackedPoseDriver)targets[i]).binding;
                        if (!HasValidAssignedControl(binding))
                        {
                            binding.deviceKey = 0;
                            binding.controlHash = CommonControls.Pose.hash;
                        }
                    }
                }

                Rect position = EditorGUILayout.GetControlRect(true, height);
                ControlGUIUtility.ControlField(position, m_TrackedPoseDriver.binding, Styles.controlLabel, this,
                    b =>
                    {
                        for (int i = 0; i < targets.Length; i++)
                            (targets[i] as TrackedPoseDriver).binding =
                                b as ControlReferenceBinding<PoseControl, Pose>;
                    },
                    m_BindingProp);
            }
            else
            {
                EditorGUILayout.PropertyField(m_PoseActionProp, Styles.poseActionLabel);
                EditorGUILayout.PropertyField(m_PlayerHandleProviderProp, Styles.playerHandleProviderLabel);
            }
            EditorGUILayout.PropertyField(m_TrackingTypeProp, Styles.trackingLabel);
            EditorGUILayout.PropertyField(m_UpdateTypeProp, Styles.updateLabel);
            EditorGUILayout.PropertyField(m_UseRelativeTransformProp, Styles.relativeLabel);

            serializedObject.ApplyModifiedProperties();
        }

        public List<DomainEntry> GetDomainEntries()
        {
            return new List<DomainEntry>() { new DomainEntry() { name = "", hash = 0 } };
        }

        public List<DomainEntry> GetControlEntriesOfType(int domainId, Type controlType)
        {
            DeviceSlot slot = m_TrackedPoseDriver.deviceSlot;
            return InputDeviceUtility.GetDeviceControlEntriesOfType(slot == null ? null : slot.type, typeof(PoseControl));
        }

        bool HasValidAssignedControl(ControlReferenceBinding<PoseControl, Pose> binding)
        {
            List<DomainEntry> controlEntries = GetControlEntriesOfType(binding.deviceKey, typeof(PoseControl));
            if (controlEntries == null)
                return false;
            return (controlEntries.FindIndex(e => e.hash == binding.controlHash) >= 0);
        }
    }
}
