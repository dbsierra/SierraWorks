using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace SierraWorks.PARAM.Editor
{
    [CustomEditor(typeof(ParameterReceiver))]
    [CanEditMultipleObjects]
    public class ParameterReceiverEditor : UnityEditor.Editor
    {
        private SerializedProperty parameterSetProp;
        private SerializedProperty selectedParameterPathProp;
        private SerializedProperty targetObjectProp;
        private SerializedProperty targetComponentNameProp;
        private SerializedProperty targetFieldNameProp;
        private SerializedProperty onFloatChangedProp;
        private SerializedProperty onIntChangedProp;
        private SerializedProperty onBoolChangedProp;
        private SerializedProperty onStringChangedProp;
        private SerializedProperty onVector2ChangedProp;
        private SerializedProperty onVector3ChangedProp;
        private SerializedProperty onColorChangedProp;

        private void OnEnable()
        {
            parameterSetProp = serializedObject.FindProperty("parameterSet");
            selectedParameterPathProp = serializedObject.FindProperty("selectedParameterPath");
            targetObjectProp = serializedObject.FindProperty("targetObject");
            targetComponentNameProp = serializedObject.FindProperty("targetComponentName");
            targetFieldNameProp = serializedObject.FindProperty("targetFieldName");
            onFloatChangedProp = serializedObject.FindProperty("onFloatChanged");
            onIntChangedProp = serializedObject.FindProperty("onIntChanged");
            onBoolChangedProp = serializedObject.FindProperty("onBoolChanged");
            onStringChangedProp = serializedObject.FindProperty("onStringChanged");
            onVector2ChangedProp = serializedObject.FindProperty("onVector2Changed");
            onVector3ChangedProp = serializedObject.FindProperty("onVector3Changed");
            onColorChangedProp = serializedObject.FindProperty("onColorChanged");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool isMultiEdit = targets.Length > 1;

            // Draw the script field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(parameterSetProp, new GUIContent("Parameter Set"));

            // Check if all targets share the same hub
            ParameterSetData sharedHub = GetSharedHub();

            if (sharedHub == null)
            {
                if (parameterSetProp.hasMultipleDifferentValues)
                {
                    EditorGUILayout.HelpBox("Selected objects have different Parameter Set assets. Parameter selection is not available for mixed hubs.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("Please assign a Parameter Set Data asset.", MessageType.Warning);
                }
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space(5);

            // Parameter selection dropdown
            var allParameterPaths = sharedHub.GetAllParameterDisplayNames();

            if (allParameterPaths.Count == 0)
            {
                EditorGUILayout.HelpBox("No parameters available in the Parameter Set.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawParameterDropdown(sharedHub, allParameterPaths, isMultiEdit);

            // Resolve the current parameter for display purposes (use first target's parameter)
            Parameter currentParam = GetSharedParameter(sharedHub);

            // Display current value of the selected parameter (read-only, single-select only)
            if (currentParam != null && !isMultiEdit)
            {
                DrawCurrentValue(currentParam);
            }
            else if (currentParam != null && isMultiEdit && !selectedParameterPathProp.hasMultipleDifferentValues)
            {
                // All targets share the same parameter, show the value
                DrawCurrentValue(currentParam);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Target Settings", EditorStyles.boldLabel);

            DrawTargetObjectField(isMultiEdit);

            DrawComponentAndFieldDropdowns(sharedHub, isMultiEdit);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            DrawEventsSection(sharedHub);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Manual Update"))
            {
                foreach (var t in targets)
                {
                    var recv = (ParameterReceiver)t;
                    recv.ManualUpdate();
                }
            }

            EditorGUILayout.HelpBox("This component automatically updates the target field/property when the parameter value changes.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Returns the shared ParameterSetData if all targets use the same hub, or null if they differ or none is assigned.
        /// </summary>
        private ParameterSetData GetSharedHub()
        {
            if (parameterSetProp.hasMultipleDifferentValues)
                return null;

            return ((ParameterReceiver)target).GetParameterHub();
        }

        /// <summary>
        /// Returns the Parameter if all targets share the same selected parameter path, or null if they differ.
        /// </summary>
        private Parameter GetSharedParameter(ParameterSetData hub)
        {
            if (hub == null) return null;
            if (selectedParameterPathProp.hasMultipleDifferentValues) return null;

            string currentPath = selectedParameterPathProp.stringValue;
            if (string.IsNullOrEmpty(currentPath)) return null;

            return hub.GetParameterByPath(currentPath);
        }

        private void DrawParameterDropdown(ParameterSetData hub, List<string> allParameterPaths, bool isMultiEdit)
        {
            if (selectedParameterPathProp.hasMultipleDifferentValues)
            {
                // Show mixed value indicator with dropdown
                EditorGUI.showMixedValue = true;
                int newIndex = EditorGUILayout.Popup("Parameter", -1, allParameterPaths.ToArray());
                EditorGUI.showMixedValue = false;

                if (newIndex >= 0)
                {
                    // User selected a value, apply to all targets
                    var selectedParam = hub.GetParameterByDisplayPath(allParameterPaths[newIndex]);
                    if (selectedParam != null)
                    {
                        foreach (var t in targets)
                        {
                            var recv = (ParameterReceiver)t;
                            recv.SetSelectedParameter(selectedParam.ID);
                            EditorUtility.SetDirty(recv);
                        }
                    }
                }
            }
            else
            {
                string currentPath = selectedParameterPathProp.stringValue;

                var currentParam = hub.GetParameterByPath(currentPath);
                string currentDisplayPath = currentParam != null ?
                    (currentParam.groupName == "Default" ? currentParam.displayName : $"{currentParam.groupName}/{currentParam.displayName}") :
                    currentPath;

                int currentIndex = allParameterPaths.IndexOf(currentDisplayPath);
                if (currentIndex < 0) currentIndex = 0;

                int newIndex = EditorGUILayout.Popup("Parameter", currentIndex, allParameterPaths.ToArray());

                if (newIndex != currentIndex || string.IsNullOrEmpty(currentPath))
                {
                    var selectedParam = hub.GetParameterByDisplayPath(allParameterPaths[newIndex]);
                    if (selectedParam != null)
                    {
                        foreach (var t in targets)
                        {
                            var recv = (ParameterReceiver)t;
                            recv.SetSelectedParameter(selectedParam.ID);
                            EditorUtility.SetDirty(recv);
                        }
                    }
                }
            }
        }

        private void DrawCurrentValue(Parameter currentParam)
        {
            EditorGUILayout.Space(5);
            GUI.enabled = false;

            object currentValue = currentParam.GetCurrentValue();

            switch (currentParam.type)
            {
                case ParameterType.Float:
                    EditorGUILayout.FloatField("Current Value", (float)currentValue);
                    break;
                case ParameterType.Int:
                    EditorGUILayout.IntField("Current Value", (int)currentValue);
                    break;
                case ParameterType.Bool:
                    EditorGUILayout.Toggle("Current Value", (bool)currentValue);
                    break;
                case ParameterType.String:
                    EditorGUILayout.TextField("Current Value", (string)currentValue);
                    break;
                case ParameterType.Vector2:
                    EditorGUILayout.Vector2Field("Current Value", (Vector2)currentValue);
                    break;
                case ParameterType.Vector3:
                    EditorGUILayout.Vector3Field("Current Value", (Vector3)currentValue);
                    break;
                case ParameterType.Color:
                    EditorGUILayout.ColorField("Current Value", (Color)currentValue);
                    break;
            }

            GUI.enabled = true;
        }

        private void DrawTargetObjectField(bool isMultiEdit)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(targetObjectProp, new GUIContent("Target GameObject"));
            if (EditorGUI.EndChangeCheck())
            {
                // Target object changed — clear component and field for all targets
                serializedObject.ApplyModifiedProperties();
                foreach (var t in targets)
                {
                    var recv = (ParameterReceiver)t;
                    recv.SetTargetComponent("");
                    recv.SetTargetField("");
                    EditorUtility.SetDirty(recv);
                }
                serializedObject.Update();
            }
        }

        private void DrawComponentAndFieldDropdowns(ParameterSetData hub, bool isMultiEdit)
        {
            // If target objects differ across selections, we can't show component/field dropdowns meaningfully
            if (targetObjectProp.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox("Selected objects have different Target GameObjects. Component and field selection is not available.", MessageType.Info);
                return;
            }

            GameObject targetObj = targetObjectProp.objectReferenceValue as GameObject;
            if (targetObj == null) return;

            EditorGUILayout.Space(5);

            // Component selection
            Component[] components = targetObj.GetComponents<Component>();
            List<string> componentNames = new List<string>();
            foreach (var comp in components)
            {
                if (comp != null)
                {
                    componentNames.Add(comp.GetType().Name);
                }
            }

            if (componentNames.Count == 0) return;

            // Component dropdown
            if (targetComponentNameProp.hasMultipleDifferentValues)
            {
                EditorGUI.showMixedValue = true;
                int newComponentIndex = EditorGUILayout.Popup("Component", -1, componentNames.ToArray());
                EditorGUI.showMixedValue = false;

                if (newComponentIndex >= 0)
                {
                    foreach (var t in targets)
                    {
                        var recv = (ParameterReceiver)t;
                        recv.SetTargetComponent(componentNames[newComponentIndex]);
                        recv.SetTargetField("");
                        EditorUtility.SetDirty(recv);
                    }
                    serializedObject.Update();
                }
            }
            else
            {
                string currentComponentName = targetComponentNameProp.stringValue;
                int componentIndex = componentNames.IndexOf(currentComponentName);
                if (componentIndex < 0) componentIndex = 0;

                int newComponentIndex = EditorGUILayout.Popup("Component", componentIndex, componentNames.ToArray());

                if (newComponentIndex != componentIndex || string.IsNullOrEmpty(currentComponentName))
                {
                    foreach (var t in targets)
                    {
                        var recv = (ParameterReceiver)t;
                        recv.SetTargetComponent(componentNames[newComponentIndex]);
                        recv.SetTargetField("");
                        EditorUtility.SetDirty(recv);
                    }
                    serializedObject.Update();
                }
            }

            // Field/Property selection
            string resolvedComponentName = targetComponentNameProp.hasMultipleDifferentValues ? null : targetComponentNameProp.stringValue;
            if (string.IsNullOrEmpty(resolvedComponentName)) return;

            Component selectedComponent = components.FirstOrDefault(c => c != null && c.GetType().Name == resolvedComponentName);
            if (selectedComponent == null) return;

            EditorGUILayout.Space(5);

            // Resolve the parameter for compatible member filtering
            Parameter parameter = null;
            if (!selectedParameterPathProp.hasMultipleDifferentValues)
            {
                string selectedPath = selectedParameterPathProp.stringValue;
                parameter = hub.GetParameterByPath(selectedPath);
            }

            var compatibleMembers = GetCompatibleMembers(selectedComponent, parameter);

            if (compatibleMembers.Count == 0)
            {
                EditorGUILayout.HelpBox("No compatible fields or properties found on this component.", MessageType.Info);
                return;
            }

            // Field dropdown
            if (targetFieldNameProp.hasMultipleDifferentValues)
            {
                EditorGUI.showMixedValue = true;
                int newFieldIndex = EditorGUILayout.Popup("Field/Property", -1, compatibleMembers.ToArray());
                EditorGUI.showMixedValue = false;

                if (newFieldIndex >= 0)
                {
                    foreach (var t in targets)
                    {
                        var recv = (ParameterReceiver)t;
                        recv.SetTargetField(compatibleMembers[newFieldIndex]);
                        EditorUtility.SetDirty(recv);
                    }
                    serializedObject.Update();
                }
            }
            else
            {
                string currentFieldName = targetFieldNameProp.stringValue;
                int fieldIndex = compatibleMembers.IndexOf(currentFieldName);

                // If the current field is not in the compatible list, auto-select the first one
                if (fieldIndex < 0)
                {
                    fieldIndex = 0;
                    foreach (var t in targets)
                    {
                        var recv = (ParameterReceiver)t;
                        recv.SetTargetField(compatibleMembers[fieldIndex]);
                        EditorUtility.SetDirty(recv);
                    }
                    serializedObject.Update();
                }

                int newFieldIndex = EditorGUILayout.Popup("Field/Property", fieldIndex, compatibleMembers.ToArray());

                if (newFieldIndex != fieldIndex)
                {
                    foreach (var t in targets)
                    {
                        var recv = (ParameterReceiver)t;
                        recv.SetTargetField(compatibleMembers[newFieldIndex]);
                        EditorUtility.SetDirty(recv);
                    }
                    serializedObject.Update();
                }
            }
        }

        private void DrawEventsSection(ParameterSetData hub)
        {
            // If parameter paths differ, we can't determine a single type — show all events
            if (selectedParameterPathProp.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox("Selected objects have different parameters. Showing all event types.", MessageType.Info);
                EditorGUILayout.PropertyField(onFloatChangedProp, new GUIContent("On Value Changed (Float)"));
                EditorGUILayout.PropertyField(onIntChangedProp, new GUIContent("On Value Changed (Int)"));
                EditorGUILayout.PropertyField(onBoolChangedProp, new GUIContent("On Value Changed (Bool)"));
                EditorGUILayout.PropertyField(onStringChangedProp, new GUIContent("On Value Changed (String)"));
                EditorGUILayout.PropertyField(onVector2ChangedProp, new GUIContent("On Value Changed (Vector2)"));
                EditorGUILayout.PropertyField(onVector3ChangedProp, new GUIContent("On Value Changed (Vector3)"));
                EditorGUILayout.PropertyField(onColorChangedProp, new GUIContent("On Value Changed (Color)"));
                return;
            }

            Parameter currentParam = GetSharedParameter(hub);

            if (currentParam != null)
            {
                switch (currentParam.type)
                {
                    case ParameterType.Float:
                        EditorGUILayout.PropertyField(onFloatChangedProp, new GUIContent("On Value Changed (Float)"));
                        break;
                    case ParameterType.Int:
                        EditorGUILayout.PropertyField(onIntChangedProp, new GUIContent("On Value Changed (Int)"));
                        break;
                    case ParameterType.Bool:
                        EditorGUILayout.PropertyField(onBoolChangedProp, new GUIContent("On Value Changed (Bool)"));
                        break;
                    case ParameterType.String:
                        EditorGUILayout.PropertyField(onStringChangedProp, new GUIContent("On Value Changed (String)"));
                        break;
                    case ParameterType.Vector2:
                        EditorGUILayout.PropertyField(onVector2ChangedProp, new GUIContent("On Value Changed (Vector2)"));
                        break;
                    case ParameterType.Vector3:
                        EditorGUILayout.PropertyField(onVector3ChangedProp, new GUIContent("On Value Changed (Vector3)"));
                        break;
                    case ParameterType.Color:
                        EditorGUILayout.PropertyField(onColorChangedProp, new GUIContent("On Value Changed (Color)"));
                        break;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a parameter to configure the output event.", MessageType.Info);
            }
        }

        private List<string> GetCompatibleMembers(Component component, Parameter parameter)
        {
            List<string> memberNames = new List<string>();

            if (parameter == null)
            {
                return memberNames;
            }

            System.Type componentType = component.GetType();
            System.Type parameterValueType = GetParameterValueType(parameter.type);

            // Get all public and serialized fields
            FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                // Include public fields or fields with SerializeField attribute
                bool isSerializedField = field.GetCustomAttribute<SerializeField>() != null;
                if (field.IsPublic || isSerializedField)
                {
                    if (IsTypeCompatible(parameterValueType, field.FieldType))
                    {
                        memberNames.Add(field.Name);
                    }
                }
            }

            // Get all public properties with setters
            PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.CanWrite && IsTypeCompatible(parameterValueType, property.PropertyType))
                {
                    memberNames.Add(property.Name);
                }
            }

            return memberNames;
        }

        private System.Type GetParameterValueType(ParameterType paramType)
        {
            switch (paramType)
            {
                case ParameterType.Float: return typeof(float);
                case ParameterType.Int: return typeof(int);
                case ParameterType.Bool: return typeof(bool);
                case ParameterType.String: return typeof(string);
                case ParameterType.Vector2: return typeof(Vector2);
                case ParameterType.Vector3: return typeof(Vector3);
                case ParameterType.Color: return typeof(Color);
                default: return null;
            }
        }

        private bool IsTypeCompatible(System.Type paramType, System.Type targetType)
        {
            if (paramType == null || targetType == null) return false;

            // Direct match
            if (targetType.IsAssignableFrom(paramType)) return true;

            // Numeric conversions
            if (IsNumericType(paramType) && IsNumericType(targetType)) return true;

            return false;
        }

        private bool IsNumericType(System.Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(double) ||
                   type == typeof(long) || type == typeof(short) || type == typeof(byte);
        }
    }
}
