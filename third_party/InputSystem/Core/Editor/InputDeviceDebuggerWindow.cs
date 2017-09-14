using UnityEditor;
using System;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Input
{
    public class InputDeviceDebuggerWindow : EditorWindow
    {
        struct EventEntry
        {
            public string name;
            public List<string> properties;
        }

        static class Styles
        {
            public static GUIStyle previewBackground = new GUIStyle("preBackground");
            public static GUIStyle previewToolbar = new GUIStyle("preToolbar");
            public static GUIStyle toolbarSearchField = new GUIStyle("ToolbarSeachTextField");
            public static GUIStyle toolbarSearchFieldCancel = new GUIStyle("ToolbarSeachCancelButton");
            public static GUIStyle toolbarSearchFieldCancelEmpty = new GUIStyle("ToolbarSeachCancelButtonEmpty");
            public static GUIStyle labelRightAligned;

            public static string notFoundHelpText = "Device could not be found.";

            static Styles()
            {
                labelRightAligned = new GUIStyle(EditorStyles.label);
                labelRightAligned.alignment = TextAnchor.MiddleRight;
            }
        }

        InputDevice m_Device;

        [SerializeField]
        string m_SerialNumber;
        [SerializeField]
        string m_TypeName;
        [SerializeField]
        Vector2 m_ScrollControls;
        [SerializeField]
        string m_EventSearchString;

        // We don't serialize this as we're not serializing the list of events either. We should probably
        // do that but that will require custom serialization to deal with the fact that the lists involve polymorphism.
        Vector2 m_ScrollEvents;

        PreviewResizer m_Preview = new PreviewResizer();

        InputHandlerCallback m_BeforeRemap;
        InputHandlerCallback m_AfterRemap;

        List<string> m_ControlIndexStrings;

        // We keep a circular buffer of recorded event data.
        const int k_MaxEventsToKeep = 30;
        EventEntry[] m_Events = new EventEntry[k_MaxEventsToKeep];
        int m_EventsNextIndex;

        static System.Type[] k_GetMethodParameters = new System.Type[] { typeof(string) };
        static object[] k_ToStringParameters = new object[] { "0.00" };

        public static void Create(InputDevice device)
        {
            InputDeviceDebuggerWindow window = EditorWindow.CreateInstance<InputDeviceDebuggerWindow>();
            window.m_SerialNumber = device.serialNumber;
            window.m_TypeName = device.GetType().FullName;
            window.m_Device = device;
            window.minSize = new Vector2(270, 300);
            window.Show();
            window.titleContent = new GUIContent(device.name);
        }

        void RecordEventBeforeRemap(InputEvent inputEvent)
        {
            if (inputEvent.device != m_Device)
                return;

            // We only remap GenericControlEvents so only those should be included
            // in "before remapping" events. Additionally, only those not marked
            // as having already been remapped.
            var genericContolEvent = inputEvent as GenericControlEvent;
            if (genericContolEvent == null || genericContolEvent.alreadyRemapped)
                return;

            RecordEvent(inputEvent);
        }

        void RecordEventAfterRemap(InputEvent inputEvent)
        {
            if (inputEvent.device != m_Device)
                return;

            RecordEvent(inputEvent);
        }

        void RecordEvent(InputEvent inputEvent)
        {
            m_Events[m_EventsNextIndex].name = inputEvent.GetType().GetNiceName();
            if (m_Events[m_EventsNextIndex].properties == null)
                m_Events[m_EventsNextIndex].properties = new List<string>();
            m_Events[m_EventsNextIndex].properties.Clear();

            var properties = inputEvent.GetType().GetProperties();
            Array.Sort(properties, (a, b) => string.Compare(a.Name, b.Name));
            foreach (var property in properties)
            {
                m_Events[m_EventsNextIndex].properties.Add(property.Name);
                m_Events[m_EventsNextIndex].properties.Add(ToStringWithDecimals(property.GetValue(inputEvent, null)));
            }

            m_EventsNextIndex = (m_EventsNextIndex + 1) % k_MaxEventsToKeep;
        }

        void OnEnable()
        {
            EditorApplication.playmodeStateChanged += Repaint;
            for (int i = 0; i < InputSystem.devices.Count; i++)
            {
                InputDevice device = InputSystem.devices[i];
                if (device.serialNumber == m_SerialNumber && device.GetType().FullName == m_TypeName)
                {
                    m_Device = device;
                    break;
                }
            }

            m_BeforeRemap = new InputHandlerCallback
            {
                processEvent =
                    evt =>
                    {
                        RecordEventBeforeRemap(evt);
                        return false;
                    }
            };
            m_AfterRemap = new InputHandlerCallback
            {
                processEvent =
                    evt =>
                    {
                        RecordEventAfterRemap(evt);
                        return false;
                    }
            };
            InputSystem.rewriters.children.Insert(0, m_BeforeRemap);
            InputSystem.consumers.children.Insert(0, m_AfterRemap);

            m_Preview.Init("InputDeviceDebugger");
        }

        void InitControlNames()
        {
            // We can't do this in OnEnable since the device has not
            // been assigned yet there when the window is first opened.
            m_ControlIndexStrings = new List<string>(m_Device.controlCount);
            for (int i = 0; i < m_Device.controlCount; i++)
            {
                var control = m_Device.GetControl(i);
                m_ControlIndexStrings.Add(control.index.ToString());
            }
        }

        void OnDisable()
        {
            EditorApplication.playmodeStateChanged -= Repaint;

            InputSystem.rewriters.children.Remove(m_BeforeRemap);
            InputSystem.consumers.children.Remove(m_AfterRemap);
        }

        void OnGUI()
        {
            if (m_Device == null)
            {
                if (!InputSystemEditorUtility.inputSystemEnabled)
                {
                    InputSystemEditorUtility.ShowSystemNotEnabledHelpbox();
                    return;
                }

                EditorGUILayout.HelpBox(Styles.notFoundHelpText, MessageType.Warning);
                return;
            }
            if (m_ControlIndexStrings == null || m_ControlIndexStrings.Count != m_Device.controlCount)
                InitControlNames();

            EditorGUILayout.BeginVertical("OL Box", GUILayout.ExpandHeight(false));
            EditorGUILayout.LabelField("Name", m_Device.name);
            EditorGUILayout.LabelField("Profile", m_Device.profile != null ? m_Device.profile.GetType().Name : "");
            EditorGUILayout.LabelField("Connected", m_Device.isConnected.ToString());
            EditorGUILayout.LabelField("Active", m_Device.active.ToString());
            EditorGUILayout.LabelField("Manufacturer", m_Device.manufacturer);
            EditorGUILayout.LabelField("Serial Number", m_Device.serialNumber);
            EditorGUILayout.LabelField("Device Type", m_Device.GetType().Name);
            EditorGUILayout.LabelField("Device Tag Index", m_Device.tagIndex.ToString());
            EditorGUILayout.LabelField("Native Device ID", m_Device.nativeId.ToString());
            EditorGUILayout.EndVertical();

            bool wasActive = InputSystem.isActive;
            InputSystem.isActive = true;

            m_ScrollControls = EditorGUILayout.BeginScrollView(m_ScrollControls);
            int controlCount = m_Device.controlCount;
            EditorGUI.indentLevel += 2;
            for (int i = 0; i < controlCount; i++)
            {
                InputControl control = m_Device.GetControl(i);
                string str = ToStringWithDecimals(control.valueObject);
                EditorGUILayout.LabelField(control.name, str);

                Rect rect = GUILayoutUtility.GetLastRect();
                rect.width = 28;
                GUI.Label(rect, m_ControlIndexStrings[i], Styles.labelRightAligned);
            }
            EditorGUI.indentLevel -= 2;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();

            InputSystem.isActive = wasActive;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Events", GUILayout.MinWidth(100));
            Rect toolbarHandle = GUILayoutUtility.GetLastRect();

            Rect searchRect = GUILayoutUtility.GetRect(GUIContent.none, Styles.toolbarSearchField, GUILayout.MinWidth(140));
            if (m_Preview.GetExpanded())
            {
                m_EventSearchString = EditorGUI.TextField(searchRect, m_EventSearchString, Styles.toolbarSearchField);
                if (GUILayout.Button(
                        GUIContent.none,
                        m_EventSearchString == string.Empty ? Styles.toolbarSearchFieldCancelEmpty : Styles.toolbarSearchFieldCancel))
                {
                    m_EventSearchString = string.Empty;
                    EditorGUIUtility.keyboardControl = 0;
                }
            }

            GUILayout.EndHorizontal();

            float height = m_Preview.ResizeHandle(new Rect(toolbarHandle.x, position.y, toolbarHandle.width, position.height), 100, 250, 17);
            if (height > 0)
                ShowEvents(height);
        }

        string ToStringWithDecimals(object obj)
        {
            if (obj is Enum)
                return obj.ToString();

            // Get value as string. Many types have an overload of ToString which accepts
            // a formatting string. Use that if available. We have to use reflection
            // as there's no interface for it or similar.
            System.Reflection.MethodInfo info =
                obj.GetType().GetMethod("ToString", k_GetMethodParameters);
            if (info != null)
                return (string)info.Invoke(obj, k_ToStringParameters);
            else
                return obj.ToString();
        }

        void ShowEvents(float height)
        {
            m_ScrollEvents = EditorGUILayout.BeginScrollView(m_ScrollEvents, GUILayout.Height(height));
            bool didShowAny = false;
            for (var i = 0; i < k_MaxEventsToKeep; i++)
            {
                int index = (m_EventsNextIndex - 1 - i + k_MaxEventsToKeep) % k_MaxEventsToKeep;
                var eventEntry = m_Events[index];
                if (eventEntry.name != null)
                {
                    DrawEvent(eventEntry);
                    didShowAny = true;
                }
            }
            if (!didShowAny)
                EditorGUILayout.LabelField("None");
            EditorGUILayout.EndScrollView();
        }

        void DrawEvent(EventEntry eventEntry)
        {
            if (!string.IsNullOrEmpty(m_EventSearchString))
            {
                var searchStringFound = false;
                for (var i = 0; i < eventEntry.properties.Count; i += 2)
                    if (eventEntry.properties[i + 1].IndexOf(m_EventSearchString, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        searchStringFound = true;
                        break;
                    }

                if (!searchStringFound)
                    searchStringFound = eventEntry.name.IndexOf(m_EventSearchString, StringComparison.OrdinalIgnoreCase) != -1;

                if (!searchStringFound)
                    return;
            }

            GUILayout.Label(eventEntry.name, EditorStyles.boldLabel);
            for (int i = 0; i < eventEntry.properties.Count; i += 2)
            {
                EditorGUILayout.LabelField(eventEntry.properties[i], eventEntry.properties[i + 1]);
            }
        }

        void Update()
        {
            Repaint();
        }
    }
}
