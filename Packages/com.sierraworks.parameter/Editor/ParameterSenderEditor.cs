using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SierraWorks.Parameter.Editor
{
    [CustomEditor(typeof(ParameterSender))]
    public class ParameterSenderEditor : UnityEditor.Editor
    {
        private ParameterSender sender;
        private SerializedProperty parameterHubProp;
        private SerializedProperty selectedParameterPathProp;
        private SerializedProperty floatValueProp;
        private SerializedProperty intValueProp;
        private SerializedProperty boolValueProp;
        private SerializedProperty stringValueProp;
        private SerializedProperty vector2ValueProp;
        private SerializedProperty vector3ValueProp;
        private SerializedProperty colorValueProp;

        private void OnEnable()
        {
            sender = (ParameterSender)target;
            parameterHubProp = serializedObject.FindProperty("parameterHub");
            selectedParameterPathProp = serializedObject.FindProperty("selectedParameterPath");
            floatValueProp = serializedObject.FindProperty("floatValue");
            intValueProp = serializedObject.FindProperty("intValue");
            boolValueProp = serializedObject.FindProperty("boolValue");
            stringValueProp = serializedObject.FindProperty("stringValue");
            vector2ValueProp = serializedObject.FindProperty("vector2Value");
            vector3ValueProp = serializedObject.FindProperty("vector3Value");
            colorValueProp = serializedObject.FindProperty("colorValue");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the script field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(parameterHubProp, new GUIContent("Parameter Hub"));

            var hub = sender.GetParameterHub();

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

            string currentPath = sender.GetSelectedParameterPath();

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
                var newSelectedParam = hub.GetParameterByDisplayPath(allParameterPaths[newIndex]);
                if (newSelectedParam != null)
                {
                    sender.SetSelectedParameter(newSelectedParam.ID);

                    // Set the field to the parameter's default value
                    SetFieldToDefaultValue(hub, newSelectedParam);
                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(sender);
                    Repaint();
                }
            }

            var selectedParam = sender.GetCachedParameter();

            if (selectedParam != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Value to Send", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                // Show appropriate value field based on parameter type
                switch (selectedParam.type)
                {
                    case ParameterType.Float:
                        EditorGUILayout.PropertyField(floatValueProp, new GUIContent("Float Value"));
                        break;
                    case ParameterType.Int:
                        EditorGUILayout.PropertyField(intValueProp, new GUIContent("Int Value"));
                        break;
                    case ParameterType.Bool:
                        EditorGUILayout.PropertyField(boolValueProp, new GUIContent("Bool Value"));
                        break;
                    case ParameterType.String:
                        EditorGUILayout.PropertyField(stringValueProp, new GUIContent("String Value"));
                        break;
                    case ParameterType.Vector2:
                        EditorGUILayout.PropertyField(vector2ValueProp, new GUIContent("Vector2 Value"));
                        break;
                    case ParameterType.Vector3:
                        EditorGUILayout.PropertyField(vector3ValueProp, new GUIContent("Vector3 Value"));
                        break;
                    case ParameterType.Color:
                        EditorGUILayout.PropertyField(colorValueProp, new GUIContent("Color Value"));
                        break;
                }

                // Auto-send when value changes
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    sender.SetValue();
                }

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Send Value Now"))
                {
                    sender.SetValue();
                }

                if (GUILayout.Button("Reset to Default"))
                {
                    SetFieldToDefaultValue(hub, selectedParam);
                    serializedObject.ApplyModifiedProperties();
                    sender.SetValue();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Value automatically sends when changed. Use Reset to Default to restore the parameter's default value.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SetFieldToDefaultValue(ParameterSetData hub, Parameter param)
        {
            if (hub.CurrentPreset == null) return;

            var defaultValue = hub.CurrentPreset.GetDefaultValue(param.ID);
            if (defaultValue == null) return;

            switch (param.type)
            {
                case ParameterType.Float:
                    floatValueProp.floatValue = defaultValue.floatValue;
                    break;
                case ParameterType.Int:
                    intValueProp.intValue = defaultValue.intValue;
                    break;
                case ParameterType.Bool:
                    boolValueProp.boolValue = defaultValue.boolValue;
                    break;
                case ParameterType.String:
                    stringValueProp.stringValue = defaultValue.stringValue ?? "";
                    break;
                case ParameterType.Vector2:
                    vector2ValueProp.vector2Value = defaultValue.vector2Value;
                    break;
                case ParameterType.Vector3:
                    vector3ValueProp.vector3Value = defaultValue.vector3Value;
                    break;
                case ParameterType.Color:
                    colorValueProp.colorValue = defaultValue.colorValue;
                    break;
            }
        }
    }
}
