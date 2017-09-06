using System;
using System.CodeDom;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Experimental.Input
{
    public class InputDebuggerWindow : EditorWindow
    {
        const int k_DeviceElementWidth = 150;

        static int s_MaxAssignedDevices;
        static int s_MaxMapDevices;
        static int s_MaxMaps;
        static int s_PlayerElementWidth = k_DeviceElementWidth * 2 + 4;

        bool m_ShowMaps;
        bool m_ShowUnrecognized;
        Vector2 m_ScrollPos;

        Dictionary<InputDevice, Rect> m_DevicePositionTargets = new Dictionary<InputDevice, Rect>();
        Dictionary<InputDevice, Rect> m_DevicePositions = new Dictionary<InputDevice, Rect>();

        static class Styles
        {
            public static GUIStyle deviceStyle;
            public static GUIStyle playerStyle;
            public static GUIStyle mapStyle;
            public static GUIStyle nodeLabel;

            public static GUIContent showMaps = new GUIContent("Show Maps");
            public static GUIContent showUnrecognized = new GUIContent("Show Unrecognized");

            static Styles()
            {
                deviceStyle = new GUIStyle("button");

                playerStyle = new GUIStyle("flow node 0");
                playerStyle.margin = new RectOffset(4, 4, 4, 4);

                mapStyle = new GUIStyle("box");
                mapStyle.padding = new RectOffset(4, 4, 24, 4);
                mapStyle.contentOffset = new Vector2(0, -20);
                if (EditorGUIUtility.isProSkin)
                    mapStyle.normal.textColor = EditorStyles.label.normal.textColor;

                nodeLabel = new GUIStyle(EditorStyles.label);
            }
        }

        [MenuItem("Window/Input Debugger", false, 2100)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            InputDebuggerWindow window = (InputDebuggerWindow)EditorWindow.GetWindow(typeof(InputDebuggerWindow));
            window.Show();
            window.titleContent = new GUIContent("Input Debug");
        }

        void OnEnable()
        {
            PlayerHandle.onChanged += Repaint;
            ActionMapInput.onStatusChanged += Repaint;
            EditorApplication.playmodeStateChanged += Repaint;
        }

        void OnDisable()
        {
            PlayerHandle.onChanged -= Repaint;
            ActionMapInput.onStatusChanged -= Repaint;
            EditorApplication.playmodeStateChanged -= Repaint;
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            m_ShowMaps = GUILayout.Toggle(m_ShowMaps, Styles.showMaps, EditorStyles.toolbarButton);
            m_ShowUnrecognized = GUILayout.Toggle(m_ShowUnrecognized, Styles.showUnrecognized, EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!InputSystemEditorUtility.inputSystemEnabled)
            {
                EditorGUILayout.Space();
                InputSystemEditorUtility.ShowSystemNotEnabledHelpbox();
                EditorGUILayout.Space();
            }

            if (m_ShowMaps)
                s_PlayerElementWidth = k_DeviceElementWidth * 2 + 4;
            else
                s_PlayerElementWidth = k_DeviceElementWidth;

            var devices = InputSystem.devices;
            var unrecognizedDevices = InputSystem.unrecognizedDevices;
            var players = PlayerHandleManager.players;

            s_MaxAssignedDevices = 1;
            foreach (var player in players)
                s_MaxAssignedDevices = Mathf.Max(s_MaxAssignedDevices, player.assignments.Count);

            s_MaxMaps = 1;
            foreach (var player in players)
                s_MaxMaps = Mathf.Max(s_MaxMaps, player.maps.Count);

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
            {
                ShowUnassignedDevices(devices);

                if (m_ShowUnrecognized)
                {
                    EditorGUILayout.Space();

                    ShowUnrecognizedDevices(unrecognizedDevices);
                }

                EditorGUILayout.Space();

                ShowGlobalPlayerHandles(devices, players);

                EditorGUILayout.Space();

                ShowPlayerHandles(devices, players);
            }
            DrawDevices(devices);
            EditorGUILayout.EndScrollView();
        }

        void DrawDevices(List<InputDevice> devices)
        {
            bool repaint = false;
            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                Rect rect;
                Rect target = m_DevicePositionTargets[device];
                if (m_DevicePositions.TryGetValue(device, out rect))
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        m_DevicePositions[device] = rect = new Rect(
                                    Vector2.Lerp(rect.position, target.position, 0.1f),
                                    Vector2.Lerp(rect.size, target.size, 0.1f));
                    }
                }
                else
                {
                    if (Event.current.type == EventType.Repaint)
                        m_DevicePositions[device] = rect = target;
                }
                if (rect != target)
                    repaint = true;
                DrawDevice(rect, device);
            }
            if (repaint)
                Repaint();
        }

        void DrawDevice(Rect position, InputDevice device)
        {
            if (GUI.Button(position, device.name, Styles.deviceStyle))
                InputDeviceDebuggerWindow.Create(device);
        }

        void ShowUnassignedDevices(List<InputDevice> devices)
        {
            GUILayout.Label("Unassigned Devices", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                if (device.GetAssignment() != null)
                    continue;
                Rect rect = GUILayoutUtility.GetRect(new GUIContent(device.name), Styles.deviceStyle, GUILayout.Width(k_DeviceElementWidth));
                m_DevicePositionTargets[device] = rect;
            }
            EditorGUILayout.EndHorizontal();
        }

        void ShowUnrecognizedDevices(List<string> devices)
        {
            GUILayout.Label("Unrecognized Devices", EditorStyles.boldLabel);

            if (devices.Count == 0)
                GUILayout.Label("None");

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false));
            foreach (var device in devices)
                GUILayout.Label(device);
            EditorGUILayout.EndVertical();
        }

        void ShowGlobalPlayerHandles(List<InputDevice> devices, IEnumerable<PlayerHandle> players)
        {
            GUILayout.Label("Global Player Handles", EditorStyles.boldLabel);
            GUILayout.Label("Listen to all unassigned devices.");

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            foreach (var player in players)
            {
                if (!player.global)
                    continue;
                DrawPlayerHandle(player);
            }
            EditorGUILayout.EndHorizontal();
        }

        void ShowPlayerHandles(List<InputDevice> devices, IEnumerable<PlayerHandle> players)
        {
            GUILayout.Label("Player Handles", EditorStyles.boldLabel);
            GUILayout.Label("Listen to devices they have assigned.");

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            foreach (var player in players)
            {
                if (player.global)
                    continue;
                DrawPlayerHandle(player);
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawPlayerHandle(PlayerHandle player)
        {
            EditorGUIUtility.labelWidth = 160;

            GUIContent playerContent = new GUIContent("Player " + player.index);
            GUILayout.BeginVertical(playerContent, Styles.playerStyle, GUILayout.Width(s_PlayerElementWidth));
            {
                GUILayout.Label("Assigned Devices", Styles.nodeLabel);
                for (int i = 0; i < s_MaxAssignedDevices; i++)
                {
                    Rect deviceRect = GUILayoutUtility.GetRect(GUIContent.none, Styles.deviceStyle, GUILayout.Width(k_DeviceElementWidth));
                    if (i >= player.assignments.Count)
                        continue;
                    m_DevicePositionTargets[player.assignments[i].device] = deviceRect;
                }

                if (m_ShowMaps)
                {
                    GUILayout.Label("Action Map Inputs", Styles.nodeLabel);
                    for (int i = 0; i < player.maps.Count; i++)
                        DrawActionMapInput(player.maps[i]);
                }
            }
            EditorGUILayout.EndVertical();
            if (player.global)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                GUI.Label(rect, "(Global)");
            }
        }

        void DrawActionMapInput(ActionMapInput map)
        {
            EditorGUI.BeginDisabledGroup(!map.active);
            GUIContent mapContent = new GUIContent(map.actionMap.name);
            GUILayout.BeginVertical(mapContent, Styles.mapStyle);
            {
                LabelField("Block Subsequent", map.blockSubsequent.ToString());

                string schemeString = "-";
                if (map.active && map.controlScheme != null)
                    schemeString = map.controlScheme.name;
                LabelField("Current Control Scheme", schemeString);

                string devicesString = "";
                if (map.active)
                    devicesString = string.Join(", ", map.GetCurrentlyUsedDevices().Select(e => e.name).ToArray());
                if (string.IsNullOrEmpty(devicesString))
                    devicesString = "-";
                LabelField("Currently Used Devices", devicesString);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }

        void LabelField(string label1, string label2)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            Rect rect1 = rect;
            Rect rect2 = rect;
            rect2.xMin += EditorGUIUtility.labelWidth;

            rect1.xMax = rect2.xMin - 2;
            GUI.Label(rect1, label1, Styles.nodeLabel);
            GUI.Label(rect2, label2, Styles.nodeLabel);
        }
    }
}
