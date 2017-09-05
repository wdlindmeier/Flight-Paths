using UnityEngine;
using UnityEngine.Experimental.Input;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using Array = System.Array;

[CustomEditor(typeof(ActionMap))]
public class ActionMapEditor : Editor
{
    static class Styles
    {
        public static GUIContent iconToolbarPlus =  EditorGUIUtility.IconContent("Toolbar Plus", "Add to list");
        public static GUIContent iconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus", "Remove from list");
        public static GUIContent iconToolbarPlusMore =  EditorGUIUtility.IconContent("Toolbar Plus More", "Choose to add to list");
    }

    ActionMap m_ActionMapEditCopy;

    int m_SelectedScheme = 0;
    int m_SelectedDeviceIndex = 0;
    [System.NonSerialized]
    InputAction m_SelectedAction = null;
    List<string> m_PropertyNames = new List<string>();
    HashSet<string> m_PropertyBlacklist  = new HashSet<string>();
    Dictionary<string, string> m_PropertyErrors = new Dictionary<string, string>();
    bool m_Modified = false;

    int selectedScheme
    {
        get { return m_SelectedScheme; }
        set
        {
            if (m_SelectedScheme == value)
                return;
            m_SelectedScheme = value;
        }
    }

    InputAction selectedAction
    {
        get { return m_SelectedAction; }
        set
        {
            if (m_SelectedAction == value)
                return;
            m_SelectedAction = value;
        }
    }

    void OnEnable()
    {
        Revert();
        RefreshPropertyNames();
        CalculateBlackList();
    }

    public virtual void OnDisable()
    {
        // When destroying the editor check if we have any unapplied modifications and ask about applying them.
        if (m_Modified)
        {
            string dialogText = "Unapplied changes to ActionMap '" + serializedObject.targetObject.name + "'.";
            if (EditorUtility.DisplayDialog("Unapplied changes", dialogText, "Apply", "Revert"))
                Apply();
        }
    }

    void Apply()
    {
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
            m_ActionMapEditCopy.controlSchemes[i].UpdateUsedControlHashes();

        EditorGUIUtility.keyboardControl = 0;

        m_ActionMapEditCopy.name = target.name;
        EditorUtility.CopySerialized(m_ActionMapEditCopy, target);

        // Make sure references in control schemes to action map itself are stored correctly.
        serializedObject.Update();
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
            serializedObject.FindProperty("m_ControlSchemes")
            .GetArrayElementAtIndex(i)
            .FindPropertyRelative("m_ActionMap").objectReferenceValue = target;
        serializedObject.ApplyModifiedProperties();

        var existingAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));

        // Add action sub-assets.
        ActionMap actionMap = (ActionMap)target;
        for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
        {
            InputAction action = m_ActionMapEditCopy.actions[i];
            action.actionMap = actionMap;
            action.actionIndex = i;
            if (existingAssets.Contains(action))
                continue;
            AssetDatabase.AddObjectToAsset(action, target);
        }

        m_Modified = false;
        // Reimporting is needed in order for the sub-assets to show up.
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));

        UpdateActionMapScript();
    }

    void Revert()
    {
        EditorGUIUtility.keyboardControl = 0;

        ActionMap original = (ActionMap)serializedObject.targetObject;
        m_ActionMapEditCopy = Instantiate<ActionMap>(original);
        m_ActionMapEditCopy.name = original.name;
        m_ActionMapEditCopy.EnforceBindingsTypeConsistency();

        m_Modified = false;
    }

    void SetActionMapDirty()
    {
        EditorUtility.SetDirty(m_ActionMapEditCopy);
        m_Modified = true;
    }

    void RefreshPropertyNames()
    {
        // Calculate property names.
        m_PropertyNames.Clear();
        for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
            m_PropertyNames.Add(GetCamelCaseString(m_ActionMapEditCopy.actions[i].name, false));

        // Calculate duplicates.
        HashSet<string> duplicates = new HashSet<string>(m_PropertyNames.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key));

        // Calculate errors.
        m_PropertyErrors.Clear();
        for (int i = 0; i < m_PropertyNames.Count; i++)
        {
            string name = m_PropertyNames[i];
            if (m_PropertyBlacklist.Contains(name))
                m_PropertyErrors[name] = "Invalid action name: " + name + ".";
            else if (duplicates.Contains(name))
                m_PropertyErrors[name] = "Duplicate action name: " + name + ".";
        }
    }

    void CalculateBlackList()
    {
        m_PropertyBlacklist = new HashSet<string>(typeof(ActionMapInput).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Select(e => e.Name));
    }

    public override void OnInspectorGUI()
    {
        if (!InputSystemEditorUtility.inputSystemEnabled)
        {
            InputSystemEditorUtility.ShowSystemNotEnabledHelpbox();
            EditorGUILayout.Space();
        }

        EditorGUI.BeginChangeCheck();

        DrawCustomNamespaceGUI();

        EditorGUILayout.Space();
        DrawControlSchemeSelection();

        if (m_ActionMapEditCopy.controlSchemes.Count > 0)
        {
            EditorGUILayout.Space();
            DrawControlSchemeGUI();

            EditorGUILayout.Space();
            DrawActionList();

            if (selectedAction != null)
            {
                EditorGUILayout.Space();
                DrawActionGUI();
            }

            EditorGUILayout.Space();
        }

        if (EditorGUI.EndChangeCheck())
        {
            SetActionMapDirty();
        }

        ApplyRevertGUI();
    }

    void DrawCustomNamespaceGUI()
    {
        EditorGUI.BeginChangeCheck();
        string customNamespace = EditorGUILayout.TextField("Custom Namespace", m_ActionMapEditCopy.customNamespace);
        if (EditorGUI.EndChangeCheck())
            m_ActionMapEditCopy.customNamespace = customNamespace;
    }

    void DrawControlSchemeSelection()
    {
        if (selectedScheme >= m_ActionMapEditCopy.controlSchemes.Count)
            selectedScheme = m_ActionMapEditCopy.controlSchemes.Count - 1;

        // Show schemes
        EditorGUILayout.LabelField("Control Schemes");

        EditorGUIUtility.GetControlID(FocusType.Passive);
        EditorGUILayout.BeginVertical("Box");
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.keyboardControl = 0;
                selectedScheme = i;
                Event.current.Use();
            }

            if (selectedScheme == i)
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

            EditorGUI.LabelField(rect, m_ActionMapEditCopy.controlSchemes[i].name);
        }
        EditorGUILayout.EndVertical();

        // Control scheme remove and add buttons
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(15 * EditorGUI.indentLevel);

            if (GUILayout.Button(Styles.iconToolbarMinus, GUIStyle.none))
                RemoveControlScheme();

            if (GUILayout.Button(Styles.iconToolbarPlus, GUIStyle.none))
                AddControlScheme();

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    void AddControlScheme()
    {
        var controlScheme = new ControlScheme("New Control Scheme", m_ActionMapEditCopy);
        m_ActionMapEditCopy.controlSchemes.Add(controlScheme);
        m_ActionMapEditCopy.EnforceBindingsTypeConsistency();

        selectedScheme = m_ActionMapEditCopy.controlSchemes.Count - 1;
    }

    void RemoveControlScheme()
    {
        m_ActionMapEditCopy.controlSchemes.RemoveAt(selectedScheme);
        if (selectedScheme >= m_ActionMapEditCopy.controlSchemes.Count)
            selectedScheme = m_ActionMapEditCopy.controlSchemes.Count - 1;
    }

    void DrawControlSchemeGUI()
    {
        ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];

        EditorGUI.BeginChangeCheck();
        string schemeName = EditorGUILayout.TextField("Control Scheme Name", scheme.name);
        if (EditorGUI.EndChangeCheck())
            scheme.name = schemeName;

        for (int i = 0; i < scheme.deviceSlots.Count; i++)
        {
            var deviceSlot = scheme.deviceSlots[i];

            Rect rect = EditorGUILayout.GetControlRect();
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                m_SelectedDeviceIndex = i;
                Repaint();
            }
            if (m_SelectedDeviceIndex == i)
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);

            deviceSlot.OnGUI(rect, new GUIContent("Device Type"));
        }

        // Device remove and add buttons
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(15 * EditorGUI.indentLevel);

            if (GUILayout.Button(Styles.iconToolbarMinus, GUIStyle.none))
                RemoveDevice();

            if (GUILayout.Button(Styles.iconToolbarPlus, GUIStyle.none))
                AddDevice();

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        // Pad this area with spacing so all control schemes use same heights,
        // and the actions table below doesn't move when switching control scheme.
        int maxDevices = 0;
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
            maxDevices = Mathf.Max(maxDevices, m_ActionMapEditCopy.controlSchemes[i].deviceSlots.Count);
        int extraLines = maxDevices - scheme.deviceSlots.Count;
        EditorGUILayout.GetControlRect(true, extraLines * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
    }

    void AddDevice()
    {
        ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];
        var deviceSlot = new DeviceSlot()
        {
            key = GetNextDeviceKey()
        };
        scheme.deviceSlots.Add(deviceSlot);
    }

    void RemoveDevice()
    {
        ControlScheme scheme = m_ActionMapEditCopy.controlSchemes[selectedScheme];
        if (m_SelectedDeviceIndex >= 0 && m_SelectedDeviceIndex < scheme.deviceSlots.Count)
        {
            scheme.deviceSlots.RemoveAt(m_SelectedDeviceIndex);
        }
    }

    int GetNextDeviceKey()
    {
        int key = 0;
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
        {
            var deviceSlots = m_ActionMapEditCopy.controlSchemes[i].deviceSlots;
            for (int j = 0; j < deviceSlots.Count; j++)
            {
                key = Mathf.Max(deviceSlots[j].key, key);
            }
        }

        return key + 1;
    }

    void DrawActionList()
    {
        // Show actions
        EditorGUILayout.LabelField("Actions", m_ActionMapEditCopy.controlSchemes[selectedScheme].name + " Bindings");
        EditorGUILayout.BeginVertical("Box");
        {
            for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
            {
                DrawActionRow(m_ActionMapEditCopy.actions[i], selectedScheme);
            }
            if (m_ActionMapEditCopy.actions.Count == 0)
                EditorGUILayout.GetControlRect();
        }
        EditorGUILayout.EndVertical();

        // Action remove and add buttons
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(15 * EditorGUI.indentLevel);

            if (GUILayout.Button(Styles.iconToolbarMinus, GUIStyle.none))
                RemoveAction();

            if (GUILayout.Button(Styles.iconToolbarPlus, GUIStyle.none))
                AddAction();

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    void AddAction()
    {
        var action = ScriptableObject.CreateInstance<InputAction>();
        action.name = "New Control";
        m_ActionMapEditCopy.actions.Add(action);
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
        {
            m_ActionMapEditCopy.controlSchemes[i].bindings.Add(null);
        }

        selectedAction = m_ActionMapEditCopy.actions[m_ActionMapEditCopy.actions.Count - 1];

        RefreshPropertyNames();
    }

    void RemoveAction()
    {
        int actionIndex = m_ActionMapEditCopy.actions.IndexOf(selectedAction);
        m_ActionMapEditCopy.actions.RemoveAt(actionIndex);
        for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
        {
            m_ActionMapEditCopy.controlSchemes[i].bindings.RemoveAt(actionIndex);
        }
        // Shift indexes of actions.
        for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
        {
            InputAction action = m_ActionMapEditCopy.actions[i];
            action.actionIndex = i;
        }

        // Shift indexes of combined actions referencing the actions that have become shifted.
        var combinedReferences = new List<IControlReference>();
        m_ActionMapEditCopy.ExtractCombinedBindingsOfType(combinedReferences);
        for (int i = 0; i < combinedReferences.Count; i++)
        {
            // This may seem weird, but in the case of the combined references,
            // the hashes are actually just indices and thus we can compare hashes and indices.
            if (combinedReferences[i].controlHash == actionIndex)
                combinedReferences[i].controlHash = -1;
            if (combinedReferences[i].controlHash > actionIndex)
                combinedReferences[i].controlHash = combinedReferences[i].controlHash - 1;
        }

        ScriptableObject.DestroyImmediate(selectedAction, true);

        if (m_ActionMapEditCopy.actions.Count == 0)
            selectedAction = null;
        else
            selectedAction = m_ActionMapEditCopy.actions[Mathf.Min(actionIndex, m_ActionMapEditCopy.actions.Count - 1)];

        RefreshPropertyNames();
    }

    void ApplyRevertGUI()
    {
        bool valid = true;
        if (m_PropertyErrors.Count > 0)
        {
            valid = false;
            EditorGUILayout.HelpBox(string.Join("\n", m_PropertyErrors.Values.ToArray()), MessageType.Error);
        }

        EditorGUI.BeginDisabledGroup(!m_Modified);

        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Revert"))
                Revert();

            EditorGUI.BeginDisabledGroup(!valid);
            if (GUILayout.Button("Apply"))
                Apply();
            EditorGUI.EndDisabledGroup();
        }
        GUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();
    }

    void DrawActionRow(InputAction action, int selectedScheme)
    {
        int actionIndex = m_ActionMapEditCopy.actions.IndexOf(action);

        float height = EditorGUIUtility.singleLineHeight + 8;
        Rect totalRect = GUILayoutUtility.GetRect(1, height);

        Rect baseRect = totalRect;
        baseRect.yMin += 4;
        baseRect.yMax -= 4;

        if (selectedAction == action)
            GUI.DrawTexture(totalRect, EditorGUIUtility.whiteTexture);

        // Show control fields

        Rect rect = baseRect;
        rect.height = EditorGUIUtility.singleLineHeight;
        rect.width = EditorGUIUtility.labelWidth - 4;

        EditorGUI.LabelField(rect, action.name);

        // Show binding fields

        InputBinding binding = m_ActionMapEditCopy.controlSchemes[selectedScheme].bindings[actionIndex];
        if (binding != null)
        {
            rect = baseRect;
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.xMin += EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(rect, binding.GetSourceName(m_ActionMapEditCopy.controlSchemes[m_SelectedScheme], true));
        }

        if (Event.current.type == EventType.MouseDown && totalRect.Contains(Event.current.mousePosition))
        {
            EditorGUIUtility.keyboardControl = 0;
            selectedAction = action;
            Event.current.Use();
        }
    }

    void UpdateActionMapScript()
    {
        ActionMap original = (ActionMap)serializedObject.targetObject;
        string className = GetCamelCaseString(original.name, true);
        StringBuilder str = new StringBuilder();

        string indent = string.Empty;
        string customNamespace = String.Empty;

        if (!string.IsNullOrEmpty(original.customNamespace))
        {
            customNamespace = string.Format("namespace {0}\n{{\n", original.customNamespace);
            indent = "    ";
        }

        str.AppendFormat(@"using UnityEngine;
using UnityEngine.Experimental.Input;

// GENERATED FILE - DO NOT EDIT MANUALLY
{1}{0}public class {2} : ActionMapInput
{0}{{
{0}    public {2}(ActionMap actionMap) : base(actionMap) {{}}

", indent, customNamespace, className);

        for (int i = 0; i < m_ActionMapEditCopy.actions.Count; i++)
        {
            Type controlType = m_ActionMapEditCopy.actions[i].controlType;
            if (controlType == null)
                continue;
            string typeStr = controlType.Name;
            str.AppendFormat("{0}    public {3} @{1} {{ get {{ return ({3})GetControl({2}); }} }}\n",
                indent, GetCamelCaseString(m_ActionMapEditCopy.actions[i].name, false), i, typeStr);
        }

        str.AppendFormat("{0}}}\n", indent);
        if (!string.IsNullOrEmpty(customNamespace))
            str.AppendLine("}");

        string path = AssetDatabase.GetAssetPath(original);
        path = path.Substring(0, path.Length - Path.GetExtension(path).Length) + ".cs";
        File.WriteAllText(path, str.ToString());
        AssetDatabase.ImportAsset(path);

        original.SetMapTypeName(className);
    }

    string GetCamelCaseString(string input, bool capitalFirstLetter)
    {
        string output = string.Empty;
        bool capitalize = capitalFirstLetter;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == ' ')
            {
                capitalize = true;
                continue;
            }
            if (char.IsLetter(c))
            {
                if (capitalize)
                    output += char.ToUpper(c);
                else if (output.Length == 0)
                    output += char.ToLower(c);
                else
                    output += c;
                capitalize = false;
                continue;
            }
            if (char.IsDigit(c))
            {
                if (output.Length > 0)
                {
                    output += c;
                    capitalize = false;
                }
                continue;
            }
            if (c == '_')
            {
                output += c;
                capitalize = true;
                continue;
            }
        }
        return output;
    }

    void DrawActionGUI()
    {
        EditorGUI.BeginChangeCheck();
        string name = EditorGUILayout.TextField("Name", selectedAction.name);
        if (EditorGUI.EndChangeCheck())
        {
            selectedAction.name = name;
            RefreshPropertyNames();
        }

        EditorGUI.BeginChangeCheck();
        Rect rect = EditorGUILayout.GetControlRect();
        Type type = TypeGUI.TypeField(rect, new GUIContent("Control Type"), typeof(InputControl), selectedAction.controlType);
        if (EditorGUI.EndChangeCheck())
        {
            int actionIndex = m_ActionMapEditCopy.actions.IndexOf(selectedAction);
            bool anyBindings = false;
            for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
            {
                var scheme = m_ActionMapEditCopy.controlSchemes[i];
                if (scheme.bindings[actionIndex] != null)
                {
                    anyBindings = true;
                    break;
                }
            }
            bool proceed = true;
            if (anyBindings)
                proceed = EditorUtility.DisplayDialog(
                        "Change Action Control Type",
                        "Changing the Control Type will clear all bindings for this action ('" + selectedAction.name + "').",
                        "Change",
                        "Cancel");
            if (proceed)
            {
                selectedAction.controlType = type;
                m_ActionMapEditCopy.EnforceBindingsTypeConsistency();
            }
        }

        EditorGUI.BeginChangeCheck();
        rect = EditorGUILayout.GetControlRect();
        bool combined = EditorGUI.Toggle(rect, new GUIContent("Combined"), selectedAction.combined);
        if (EditorGUI.EndChangeCheck())
        {
            int actionIndex = m_ActionMapEditCopy.actions.IndexOf(selectedAction);
            bool anyBindings = false;
            for (int i = 0; i < m_ActionMapEditCopy.controlSchemes.Count; i++)
            {
                var scheme = m_ActionMapEditCopy.controlSchemes[i];
                if (scheme.bindings[actionIndex] != null)
                {
                    anyBindings = true;
                    break;
                }
            }
            bool proceed = true;
            if (anyBindings)
                proceed = EditorUtility.DisplayDialog(
                        "Change Action",
                        "Changing whether the Action is combined will clear all bindings for this action ('" + selectedAction.name + "').",
                        "Change",
                        "Cancel");
            if (proceed)
            {
                selectedAction.combined = combined;
                m_ActionMapEditCopy.EnforceBindingsTypeConsistency();
            }
        }

        if (selectedAction.combined)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions being combined");
            EditorGUILayout.Space();

            InputBinding binding = selectedAction.selfBinding;
            float height = binding.GetPropertyHeight();
            rect = EditorGUILayout.GetControlRect(true, height);
            binding.OnGUI(rect, m_ActionMapEditCopy);
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bindings for " + m_ActionMapEditCopy.controlSchemes[selectedScheme].name);
            EditorGUILayout.Space();

            if (selectedScheme >= 0 && selectedScheme < m_ActionMapEditCopy.controlSchemes.Count)
            {
                int actionIndex = m_ActionMapEditCopy.actions.IndexOf(selectedAction);
                InputBinding binding = m_ActionMapEditCopy.controlSchemes[selectedScheme].bindings[actionIndex];
                if (binding != null)
                {
                    float height = binding.GetPropertyHeight();
                    rect = EditorGUILayout.GetControlRect(true, height);
                    binding.OnGUI(rect, m_ActionMapEditCopy.controlSchemes[m_SelectedScheme]);
                }
            }
        }
    }
}
