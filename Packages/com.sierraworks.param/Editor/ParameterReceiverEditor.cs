using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace SierraWorks.PARAM.Editor
{
    [CustomEditor(typeof(ParameterReceiver))]
    public class ParameterReceiverEditor : UnityEditor.Editor
    {
        private ParameterReceiver receiver;
        private SerializedProperty parameterHubProp;
        private SerializedProperty selectedParameterPathProp;
        private SerializedProperty targetObjectProp;
        private SerializedProperty targetComponentNameProp;
        private SerializedProperty targetFieldNameProp;
        private SerializedProperty onParameterChangedProp;

        private void OnEnable()
        {
            receiver = (ParameterReceiver)target;
            parameterHubProp = serializedObject.FindProperty("parameterHub");
            selectedParameterPathProp = serializedObject.FindProperty("selectedParameterPath");
            targetObjectProp = serializedObject.FindProperty("targetObject");
            targetComponentNameProp = serializedObject.FindProperty("targetComponentName");
            targetFieldNameProp = serializedObject.FindProperty("targetFieldName");
            onParameterChangedProp = serializedObject.FindProperty("onParameterChanged");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the script field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(parameterHubProp, new GUIContent("Parameter Hub"));

            var hub = receiver.GetParameterHub();

            if (hub == null)
            {
                EditorGUILayout.HelpBox("Please assign a Parameter Hub Data asset.", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space(5);

            // Parameter selection dropdown
            var allParameterPaths = hub.GetAllParameterDisplayNames();

            if (allParameterPaths.Count == 0)
            {
                EditorGUILayout.HelpBox("No parameters available in the Parameter Hub.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            string currentPath = receiver.GetSelectedParameterPath();

            // Try to find current parameter using multiple strategies (ID, display path, internal path)
            var currentParam = hub.GetParameterByPath(currentPath);
            string currentDisplayPath = currentParam != null ?
                (currentParam.groupName == "Default" ? currentParam.displayName : $"{currentParam.groupName}/{currentParam.displayName}") :
                currentPath;

            int currentIndex = allParameterPaths.IndexOf(currentDisplayPath);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex = EditorGUILayout.Popup("Parameter", currentIndex, allParameterPaths.ToArray());

            if (newIndex != currentIndex || string.IsNullOrEmpty(currentPath))
            {
                // Get the parameter from the display path and store its unique ID
                var selectedParam = hub.GetParameterByDisplayPath(allParameterPaths[newIndex]);
                if (selectedParam != null)
                {
                    receiver.SetSelectedParameter(selectedParam.ID);
                    EditorUtility.SetDirty(receiver);
                }
            }

            // Display current value of the selected parameter (read-only)
            if (currentParam != null)
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

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Target Settings", EditorStyles.boldLabel);

            // Target Object
            GameObject previousTarget = receiver.GetTargetObject();
            EditorGUILayout.PropertyField(targetObjectProp, new GUIContent("Target GameObject"));
            GameObject newTarget = targetObjectProp.objectReferenceValue as GameObject;

            if (newTarget != previousTarget)
            {
                receiver.SetTargetObject(newTarget);
                receiver.SetTargetComponent("");
                receiver.SetTargetField("");
                EditorUtility.SetDirty(receiver);
            }

            if (newTarget != null)
            {
                EditorGUILayout.Space(5);

                // Component selection
                Component[] components = newTarget.GetComponents<Component>();
                List<string> componentNames = new List<string>();
                foreach (var comp in components)
                {
                    if (comp != null)
                    {
                        componentNames.Add(comp.GetType().Name);
                    }
                }

                if (componentNames.Count > 0)
                {
                    string currentComponentName = receiver.GetTargetComponentName();
                    int componentIndex = componentNames.IndexOf(currentComponentName);
                    if (componentIndex < 0) componentIndex = 0;

                    int newComponentIndex = EditorGUILayout.Popup("Component", componentIndex, componentNames.ToArray());

                    if (newComponentIndex != componentIndex || string.IsNullOrEmpty(currentComponentName))
                    {
                        receiver.SetTargetComponent(componentNames[newComponentIndex]);
                        receiver.SetTargetField("");
                        EditorUtility.SetDirty(receiver);
                    }

                    // Field/Property selection
                    if (!string.IsNullOrEmpty(receiver.GetTargetComponentName()))
                    {
                        Component selectedComponent = components.FirstOrDefault(c => c != null && c.GetType().Name == receiver.GetTargetComponentName());

                        if (selectedComponent != null)
                        {
                            EditorGUILayout.Space(5);

                            string selectedPath = receiver.GetSelectedParameterPath();
                            var parameter = hub.GetParameterByPath(selectedPath);

                            var compatibleMembers = GetCompatibleMembers(selectedComponent, parameter);

                            if (compatibleMembers.Count > 0)
                            {
                                string currentFieldName = receiver.GetTargetFieldName();
                                int fieldIndex = compatibleMembers.IndexOf(currentFieldName);

                                // If the current field is not in the compatible list, auto-select the first one
                                if (fieldIndex < 0)
                                {
                                    fieldIndex = 0;
                                    receiver.SetTargetField(compatibleMembers[fieldIndex]);
                                    EditorUtility.SetDirty(receiver);
                                }

                                int newFieldIndex = EditorGUILayout.Popup("Field/Property", fieldIndex, compatibleMembers.ToArray());

                                if (newFieldIndex != fieldIndex)
                                {
                                    receiver.SetTargetField(compatibleMembers[newFieldIndex]);
                                    EditorUtility.SetDirty(receiver);
                                }
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("No compatible fields or properties found on this component.", MessageType.Info);
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onParameterChangedProp);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Manual Update"))
            {
                receiver.ManualUpdate();
            }

            EditorGUILayout.HelpBox("This component automatically updates the target field/property when the parameter value changes.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
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
