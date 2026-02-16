using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace SierraWorks.PARAM.Editor
{
    [CustomEditor(typeof(ParameterSetData))]
    public class ParameterSetDataEditor : UnityEditor.Editor
    {
        private ParameterSetData data;
        private int selectedGroupIndex = 0;
        private int lastSelectedGroupIndex = -1;
        private Vector2 groupScrollPosition;
        private Vector2 parameterScrollPosition;
        private string newGroupName = "";
        private string newPresetName = "";
        private Parameter draggedParameter = null;
        private int draggedGroupIndex = -1;
        private int groupDropTargetIndex = -1;

        private void OnEnable()
        {
            data = (ParameterSetData)target;
            EnsureParameterIDs();
        }

        private void EnsureParameterIDs()
        {
            // Ensure all parameters have unique IDs by accessing the ID property
            // This will auto-generate GUIDs for any parameters that don't have them yet
            bool isDirty = false;
            foreach (var group in data.groups)
            {
                foreach (var param in group.parameters)
                {
                    // Access ID property to ensure it's generated
                    string id = param.ID;
                    if (!string.IsNullOrEmpty(id))
                    {
                        isDirty = true;
                    }
                }
            }

            if (isDirty)
            {
                EditorUtility.SetDirty(data);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the script field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((ScriptableObject)target), typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.Space(10);
            DrawPresetSelector();
            EditorGUILayout.Space(10);

            if (data.groups.Count == 0) return;

            EditorGUILayout.BeginHorizontal();

            // Left panel - Groups
            DrawGroupsPanel();

            // Right panel - Parameters
            DrawParametersPanel();

            EditorGUILayout.EndHorizontal();

            // Draw drag ghost overlay
            if (draggedParameter != null || draggedGroupIndex >= 0)
            {
                DrawDragGhost();
                Repaint();
            }

            // Reset all to defaults button
            if (GUILayout.Button("Set All Parameters to Default Values", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Set to Defaults",
                    $"Set all parameters to their default values from preset '{data.CurrentPreset?.presetName ?? "N/A"}'?\n\nThis will notify all receivers.",
                    "Yes", "Cancel"))
                {
                    Undo.RecordObject(data, "Set All Parameters to Defaults");
                    data.ResetAllToDefaultsAndNotify();
                    EditorUtility.SetDirty(data);
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(data);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawPresetSelector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Preset Management", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Preset:", GUILayout.Width(100));

            // Mark "Default" entries with (locked) suffix in dropdown
            string[] presetNames = data.presets.Select(p =>
                p.presetName == "Default" ? "Default (locked)" : p.presetName).ToArray();
            int newPresetIndex = EditorGUILayout.Popup(data.currentPresetIndex, presetNames);

            if (newPresetIndex != data.currentPresetIndex)
            {
                data.currentPresetIndex = newPresetIndex;
                EditorUtility.SetDirty(data);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            newPresetName = EditorGUILayout.TextField("New Preset Name:", newPresetName);

            GUI.enabled = !string.IsNullOrEmpty(newPresetName) && newPresetName != "Default" &&
                          !data.presets.Any(p => p.presetName == newPresetName);

            if (GUILayout.Button("Add Preset", GUILayout.Width(100)))
            {
                data.AddPreset(newPresetName);
                newPresetName = "";
                EditorUtility.SetDirty(data);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset All to Defaults", GUILayout.Width(150)))
            {
                if (EditorUtility.DisplayDialog("Reset All Parameters",
                    "Are you sure you want to reset all parameters to their default values for the current preset?",
                    "Yes", "No"))
                {
                    ResetAllParametersToDefaults();
                    EditorUtility.SetDirty(data);
                }
            }

            GUILayout.FlexibleSpace();

            GUI.enabled = data.presets.Count > 1 && data.CurrentPreset != null && data.CurrentPreset.presetName != "Default";

            if (GUILayout.Button("Remove Current Preset", GUILayout.Width(150)))
            {
                if (EditorUtility.DisplayDialog("Remove Preset",
                    $"Are you sure you want to remove preset '{data.CurrentPreset.presetName}'?",
                    "Yes", "No"))
                {
                    data.RemovePreset(data.currentPresetIndex);
                    EditorUtility.SetDirty(data);
                }
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawGroupsPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(115));

            EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);

            groupScrollPosition = EditorGUILayout.BeginScrollView(groupScrollPosition,
                EditorStyles.helpBox, GUILayout.Height(600), GUILayout.Width(115));

            var groups = data.groups;
            groupDropTargetIndex = -1;

            for (int i = 0; i < groups.Count; i++)
            {
                bool isSelected = selectedGroupIndex == i;

                // Draw insertion indicator above this item if it's the drop target
                if (draggedGroupIndex >= 0)
                {
                    Rect insertRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
                    HandleGroupDropTarget(insertRect, i);
                    if (groupDropTargetIndex == i)
                    {
                        EditorGUI.DrawRect(new Rect(insertRect.x, insertRect.y, insertRect.width, 2),
                            new Color(0.2f, 0.6f, 1f, 1f));
                    }
                }

                // Create a clickable box with background highlight
                Rect lineRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

                // Highlight drop target when dragging parameters
                if (draggedParameter != null && lineRect.Contains(Event.current.mousePosition)
                    && draggedParameter.groupName != groups[i].groupName)
                {
                    EditorGUI.DrawRect(lineRect, new Color(0.4f, 0.8f, 0.4f, 0.3f));
                }
                // Dim the dragged group entry
                else if (draggedGroupIndex == i)
                {
                    EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.2f));
                }
                // Draw background highlight for selected item
                else if (isSelected)
                {
                    EditorGUI.DrawRect(lineRect, new Color(0.3f, 0.5f, 0.8f, 0.3f));
                }

                GUIStyle labelStyle = isSelected ? EditorStyles.boldLabel : EditorStyles.label;

                // Draw the group name as a label (not a button, so we can handle mouse events ourselves)
                GUILayout.Label(groups[i].groupName, labelStyle);

                EditorGUILayout.EndHorizontal();

                // Handle click-to-select and drag initiation on the same rect
                HandleGroupClickAndDrag(lineRect, i);

                // Handle parameter drag and drop target
                HandleParameterDrop(lineRect, groups[i].groupName);
            }

            // Draw insertion indicator after the last item
            if (draggedGroupIndex >= 0)
            {
                Rect insertRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));
                HandleGroupDropTarget(insertRect, groups.Count);
                if (groupDropTargetIndex == groups.Count)
                {
                    EditorGUI.DrawRect(new Rect(insertRect.x, insertRect.y, insertRect.width, 2),
                        new Color(0.2f, 0.6f, 1f, 1f));
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(5);

            // Add new group
            EditorGUILayout.BeginHorizontal(GUILayout.Width(150));
            newGroupName = EditorGUILayout.TextField(newGroupName, GUILayout.Width(115));

            GUI.enabled = !string.IsNullOrEmpty(newGroupName) &&
                          !groups.Any(g => g.groupName == newGroupName);

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                data.AddGroup(newGroupName);
                newGroupName = "";
                EditorUtility.SetDirty(data);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            // Remove group button (disabled if only one group remains)
            GUI.enabled = selectedGroupIndex >= 0 && selectedGroupIndex < groups.Count &&
                          groups.Count > 1;

            if (GUILayout.Button("Remove Group", GUILayout.Width(150)))
            {
                string groupToRemove = groups[selectedGroupIndex].groupName;
                if (EditorUtility.DisplayDialog("Remove Group",
                    $"Are you sure you want to remove group '{groupToRemove}' and all its parameters?",
                    "Yes", "No"))
                {
                    Undo.RecordObject(data, $"Remove Group '{groupToRemove}'");
                    data.RemoveGroup(groupToRemove);
                    selectedGroupIndex = 0;
                    EditorUtility.SetDirty(data);
                }
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        private void DrawParametersPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            var groups = data.groups;

            if (selectedGroupIndex < 0 || selectedGroupIndex >= groups.Count)
            {
                EditorGUILayout.LabelField("No group selected");
                EditorGUILayout.EndVertical();
                return;
            }

            var selectedGroup = groups[selectedGroupIndex];
            bool isDefaultPreset = data.CurrentPreset != null && data.CurrentPreset.presetName == "Default";

            // Deselect the text field when switching groups
            if (selectedGroupIndex != lastSelectedGroupIndex)
            {
                lastSelectedGroupIndex = selectedGroupIndex;
                GUI.FocusControl(null);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Group Name:", EditorStyles.boldLabel, GUILayout.Width(85));
            EditorGUI.BeginChangeCheck();
            string newGroupName = EditorGUILayout.TextField(selectedGroup.groupName);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newGroupName) && newGroupName != selectedGroup.groupName)
            {
                // Check for duplicate names
                if (!data.groups.Exists(g => g.groupName == newGroupName))
                {
                    Undo.RecordObject(data, "Rename Group");
                    selectedGroup.groupName = newGroupName;
                    // Update groupName on all parameters in this group
                    foreach (var p in selectedGroup.parameters)
                    {
                        p.groupName = newGroupName;
                    }
                    EditorUtility.SetDirty(data);
                }
            }
            EditorGUILayout.EndHorizontal();

            parameterScrollPosition = EditorGUILayout.BeginScrollView(parameterScrollPosition,
                EditorStyles.helpBox, GUILayout.Height(600), GUILayout.ExpandWidth(true));

            for (int i = 0; i < selectedGroup.parameters.Count; i++)
            {
                if (isDefaultPreset)
                {
                    DrawParameter(selectedGroup.parameters[i], i);
                }
                else
                {
                    DrawParameterPresetRow(selectedGroup.parameters[i]);
                }
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(5);

            // Add new parameter (only on default preset)
            if (isDefaultPreset)
            {
                if (GUILayout.Button("Add Parameter"))
                {
                    var newParam = new Parameter
                    {
                        name = $"NewParameter_{selectedGroup.parameters.Count}",
                        displayName = $"New Parameter {selectedGroup.parameters.Count}",
                        groupName = selectedGroup.groupName,
                        type = ParameterType.Float
                    };
                    data.AddParameter(selectedGroup.groupName, newParam);
                    EditorUtility.SetDirty(data);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawParameter(Parameter param, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal();

            // Drag handle
            Rect dragRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
            EditorGUI.LabelField(dragRect, "≡");
            HandleParameterDrag(dragRect, param);

            EditorGUILayout.LabelField(param.displayName, EditorStyles.boldLabel, GUILayout.Width(150));

            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog("Remove Parameter",
                    $"Are you sure you want to remove parameter '{param.displayName}'?",
                    "Yes", "No"))
                {
                    Undo.RecordObject(data, $"Remove Parameter '{param.displayName}'");
                    data.RemoveParameter(param.groupName, param.name);
                    EditorUtility.SetDirty(data);
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            // Parameter metadata fields
            param.name = EditorGUILayout.TextField("Name", param.name);
            param.displayName = EditorGUILayout.TextField("Display Name", param.displayName);
            param.type = (ParameterType)EditorGUILayout.EnumPopup("Type", param.type);

            // Default value for current preset
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default Value:", GUILayout.Width(100));
            DrawDefaultValueField(param);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawDefaultValueField(Parameter param)
        {
            if (data.CurrentPreset == null) return;

            var defaultValue = data.CurrentPreset.GetDefaultValue(param.ID);
            if (defaultValue == null)
            {
                // Create a default value entry if it doesn't exist
                defaultValue = new ParameterDefaultValue(param.ID);
                data.CurrentPreset.defaultValues.Add(defaultValue);
            }

            bool isDefaultPreset = data.CurrentPreset.presetName == "Default";
            EditorGUI.BeginChangeCheck();

            switch (param.type)
            {
                case ParameterType.Float:
                    defaultValue.floatValue = EditorGUILayout.FloatField(defaultValue.floatValue);
                    break;
                case ParameterType.Int:
                    defaultValue.intValue = EditorGUILayout.IntField(defaultValue.intValue);
                    break;
                case ParameterType.Bool:
                    defaultValue.boolValue = EditorGUILayout.Toggle(defaultValue.boolValue);
                    break;
                case ParameterType.String:
                    defaultValue.stringValue = EditorGUILayout.TextField(defaultValue.stringValue ?? "");
                    break;
                case ParameterType.Vector2:
                    defaultValue.vector2Value = EditorGUILayout.Vector2Field("", defaultValue.vector2Value);
                    break;
                case ParameterType.Vector3:
                    defaultValue.vector3Value = EditorGUILayout.Vector3Field("", defaultValue.vector3Value);
                    break;
                case ParameterType.Color:
                    defaultValue.colorValue = EditorGUILayout.ColorField(defaultValue.colorValue);
                    break;
            }

            // If value changed in Default preset, sync to all non-overridden presets
            if (EditorGUI.EndChangeCheck() && isDefaultPreset)
            {
                SyncDefaultValueToNonOverriddenPresets(param.ID, defaultValue);
            }
        }

        private void SyncDefaultValueToNonOverriddenPresets(string parameterId, ParameterDefaultValue sourceValue)
        {
            foreach (var preset in data.presets)
            {
                // Skip the Default preset itself
                if (preset.presetName == "Default") continue;

                var targetValue = preset.GetDefaultValue(parameterId);
                if (targetValue != null && !targetValue.isOverridden)
                {
                    // Copy all values from source
                    targetValue.floatValue = sourceValue.floatValue;
                    targetValue.intValue = sourceValue.intValue;
                    targetValue.boolValue = sourceValue.boolValue;
                    targetValue.stringValue = sourceValue.stringValue;
                    targetValue.vector2Value = sourceValue.vector2Value;
                    targetValue.vector3Value = sourceValue.vector3Value;
                    targetValue.colorValue = sourceValue.colorValue;
                }
            }
        }

        private void DrawParameterPresetRow(Parameter param)
        {
            if (data.CurrentPreset == null) return;

            var defaultValue = data.CurrentPreset.GetDefaultValue(param.ID);
            if (defaultValue == null)
            {
                defaultValue = new ParameterDefaultValue(param.ID);
                data.CurrentPreset.defaultValues.Add(defaultValue);
                EditorUtility.SetDirty(data);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // Override toggle
            EditorGUI.BeginChangeCheck();
            bool isOverridden = EditorGUILayout.Toggle(defaultValue.isOverridden, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(data, "Toggle Parameter Override");
                defaultValue.isOverridden = isOverridden;

                // When turning off override, copy value from Default preset
                if (!isOverridden)
                {
                    CopyFromDefaultPreset(param, defaultValue);
                }

                EditorUtility.SetDirty(data);
            }

            // If not overridden, grey out the name and value
            if (!isOverridden)
            {
                GUI.enabled = false;
            }

            // Parameter name
            EditorGUILayout.LabelField(param.displayName, GUILayout.Width(150));

            // Value field
            DrawPresetValueField(param, defaultValue);

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPresetValueField(Parameter param, ParameterDefaultValue defaultValue)
        {
            switch (param.type)
            {
                case ParameterType.Float:
                    EditorGUI.BeginChangeCheck();
                    float newFloat = EditorGUILayout.FloatField(defaultValue.floatValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data, "Change Parameter Default");
                        defaultValue.floatValue = newFloat;
                        EditorUtility.SetDirty(data);
                    }
                    break;
                case ParameterType.Int:
                    EditorGUI.BeginChangeCheck();
                    int newInt = EditorGUILayout.IntField(defaultValue.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data, "Change Parameter Default");
                        defaultValue.intValue = newInt;
                        EditorUtility.SetDirty(data);
                    }
                    break;
                case ParameterType.Bool:
                    EditorGUI.BeginChangeCheck();
                    bool newBool = EditorGUILayout.Toggle(defaultValue.boolValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data, "Change Parameter Default");
                        defaultValue.boolValue = newBool;
                        EditorUtility.SetDirty(data);
                    }
                    break;
                case ParameterType.String:
                    EditorGUI.BeginChangeCheck();
                    string newString = EditorGUILayout.TextField(defaultValue.stringValue ?? "");
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data, "Change Parameter Default");
                        defaultValue.stringValue = newString;
                        EditorUtility.SetDirty(data);
                    }
                    break;
                case ParameterType.Vector2:
                    EditorGUI.BeginChangeCheck();
                    Vector2 newV2 = EditorGUILayout.Vector2Field("", defaultValue.vector2Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data, "Change Parameter Default");
                        defaultValue.vector2Value = newV2;
                        EditorUtility.SetDirty(data);
                    }
                    break;
                case ParameterType.Vector3:
                    EditorGUI.BeginChangeCheck();
                    Vector3 newV3 = EditorGUILayout.Vector3Field("", defaultValue.vector3Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data, "Change Parameter Default");
                        defaultValue.vector3Value = newV3;
                        EditorUtility.SetDirty(data);
                    }
                    break;
                case ParameterType.Color:
                    EditorGUI.BeginChangeCheck();
                    Color newColor = EditorGUILayout.ColorField(defaultValue.colorValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(data, "Change Parameter Default");
                        defaultValue.colorValue = newColor;
                        EditorUtility.SetDirty(data);
                    }
                    break;
            }
        }

        private void CopyFromDefaultPreset(Parameter param, ParameterDefaultValue targetValue)
        {
            // Find the Default preset
            var defaultPreset = data.presets.Find(p => p.presetName == "Default");
            if (defaultPreset == null) return;

            var sourceValue = defaultPreset.GetDefaultValue(param.ID);
            if (sourceValue == null) return;

            targetValue.floatValue = sourceValue.floatValue;
            targetValue.intValue = sourceValue.intValue;
            targetValue.boolValue = sourceValue.boolValue;
            targetValue.stringValue = sourceValue.stringValue;
            targetValue.vector2Value = sourceValue.vector2Value;
            targetValue.vector3Value = sourceValue.vector3Value;
            targetValue.colorValue = sourceValue.colorValue;
        }

        private void HandleParameterDrag(Rect dragRect, Parameter param)
        {
            Event evt = Event.current;
            bool isDefaultPreset = data.CurrentPreset != null && data.CurrentPreset.presetName == "Default";

            if (!isDefaultPreset) return;

            if (evt.type == EventType.MouseDown && dragRect.Contains(evt.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[0];
                DragAndDrop.SetGenericData("ParameterDrag", param);
                DragAndDrop.StartDrag($"Move '{param.displayName}'");
                draggedParameter = param;
                evt.Use();
            }
        }

        private void HandleParameterDrop(Rect dropRect, string targetGroupName)
        {
            Event evt = Event.current;
            bool isDefaultPreset = data.CurrentPreset != null && data.CurrentPreset.presetName == "Default";

            if (!isDefaultPreset) return;

            if (evt.type == EventType.DragUpdated && dropRect.Contains(evt.mousePosition))
            {
                var dragData = DragAndDrop.GetGenericData("ParameterDrag") as Parameter;
                if (dragData != null && dragData.groupName != targetGroupName)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform && dropRect.Contains(evt.mousePosition))
            {
                var dragData = DragAndDrop.GetGenericData("ParameterDrag") as Parameter;
                if (dragData != null && dragData.groupName != targetGroupName)
                {
                    DragAndDrop.AcceptDrag();
                    Undo.RecordObject(data, "Move Parameter");
                    data.MoveParameter(dragData, dragData.groupName, targetGroupName);
                    draggedParameter = null;
                    EditorUtility.SetDirty(data);
                    evt.Use();
                }
            }
            else if (evt.type == EventType.DragExited)
            {
                draggedParameter = null;
            }
        }

        private int groupMouseDownIndex = -1;
        private Vector2 groupMouseDownPos;

        private void HandleGroupClickAndDrag(Rect rect, int groupIndex)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (rect.Contains(evt.mousePosition) && evt.button == 0)
                    {
                        groupMouseDownIndex = groupIndex;
                        groupMouseDownPos = evt.mousePosition;
                        evt.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (groupMouseDownIndex == groupIndex && draggedGroupIndex < 0)
                    {
                        // Start drag if moved enough distance
                        float dist = Vector2.Distance(evt.mousePosition, groupMouseDownPos);
                        if (dist > 5f)
                        {
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new Object[0];
                            DragAndDrop.SetGenericData("GroupDrag", groupIndex);
                            DragAndDrop.StartDrag($"Move '{data.groups[groupIndex].groupName}'");
                            draggedGroupIndex = groupIndex;
                            groupMouseDownIndex = -1;
                            evt.Use();
                        }
                    }
                    break;

                case EventType.MouseUp:
                    if (groupMouseDownIndex == groupIndex && rect.Contains(evt.mousePosition) && evt.button == 0)
                    {
                        // It was a click (no drag happened)
                        selectedGroupIndex = groupIndex;
                        groupMouseDownIndex = -1;
                        evt.Use();
                    }
                    break;
            }
        }

        private void HandleGroupDropTarget(Rect dropRect, int insertIndex)
        {
            Event evt = Event.current;

            // Expand the hit area vertically for easier targeting
            Rect hitRect = new Rect(dropRect.x, dropRect.y - 8, dropRect.width, 18);

            if (evt.type == EventType.DragUpdated && hitRect.Contains(evt.mousePosition))
            {
                var dragData = DragAndDrop.GetGenericData("GroupDrag");
                if (dragData is int sourceIndex)
                {
                    // Don't allow dropping at the same position or adjacent (no-op)
                    if (insertIndex != sourceIndex && insertIndex != sourceIndex + 1)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        groupDropTargetIndex = insertIndex;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform && hitRect.Contains(evt.mousePosition))
            {
                var dragData = DragAndDrop.GetGenericData("GroupDrag");
                if (dragData is int sourceIndex && insertIndex != sourceIndex && insertIndex != sourceIndex + 1)
                {
                    DragAndDrop.AcceptDrag();
                    Undo.RecordObject(data, "Reorder Group");

                    var group = data.groups[sourceIndex];
                    data.groups.RemoveAt(sourceIndex);

                    // Adjust insert index after removal
                    int adjustedIndex = insertIndex > sourceIndex ? insertIndex - 1 : insertIndex;
                    data.groups.Insert(adjustedIndex, group);

                    // Update selected group index to follow the selection
                    if (selectedGroupIndex == sourceIndex)
                    {
                        selectedGroupIndex = adjustedIndex;
                    }
                    else if (sourceIndex < selectedGroupIndex && adjustedIndex >= selectedGroupIndex)
                    {
                        selectedGroupIndex--;
                    }
                    else if (sourceIndex > selectedGroupIndex && adjustedIndex <= selectedGroupIndex)
                    {
                        selectedGroupIndex++;
                    }

                    draggedGroupIndex = -1;
                    groupDropTargetIndex = -1;
                    EditorUtility.SetDirty(data);
                    evt.Use();
                }
            }
            else if (evt.type == EventType.DragExited)
            {
                draggedGroupIndex = -1;
                groupDropTargetIndex = -1;
            }
        }

        private void DrawDragGhost()
        {
            if (Event.current.type != EventType.Repaint) return;

            string ghostLabel = null;
            if (draggedParameter != null)
            {
                ghostLabel = draggedParameter.displayName;
            }
            else if (draggedGroupIndex >= 0 && draggedGroupIndex < data.groups.Count)
            {
                ghostLabel = data.groups[draggedGroupIndex].groupName;
            }

            if (ghostLabel == null) return;

            Vector2 mousePos = Event.current.mousePosition;

            // Draw a semi-transparent ghost
            Rect ghostRect = new Rect(mousePos.x + 10, mousePos.y - 10, 160, 22);
            Color prevColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            GUI.Box(ghostRect, "");
            GUI.color = new Color(1f, 1f, 1f, 0.85f);
            GUI.Label(new Rect(ghostRect.x + 5, ghostRect.y + 3, ghostRect.width - 10, ghostRect.height),
                ghostLabel, EditorStyles.boldLabel);
            GUI.color = prevColor;
        }

        private void ResetAllParametersToDefaults()
        {
            if (data.CurrentPreset == null) return;

            foreach (var group in data.groups)
            {
                foreach (var param in group.parameters)
                {
                    var defaultValue = data.CurrentPreset.GetDefaultValue(param.ID);
                    if (defaultValue != null)
                    {
                        param.SetCurrentValue(defaultValue.GetValue(param.type));
                    }
                }
            }
        }
    }
}
