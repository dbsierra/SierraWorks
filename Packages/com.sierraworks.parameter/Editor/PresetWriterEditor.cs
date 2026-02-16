using UnityEngine;
using UnityEditor;

namespace SierraWorks.Parameter.Editor
{
    [CustomEditor(typeof(PresetWriter))]
    public class PresetWriterEditor : UnityEditor.Editor
    {
        private SerializedProperty parameterHubProp;
        private SerializedProperty presetNameProp;

        private void OnEnable()
        {
            parameterHubProp = serializedObject.FindProperty("parameterHub");
            presetNameProp = serializedObject.FindProperty("presetName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the script field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(parameterHubProp, new GUIContent("Parameter Hub"));
            EditorGUILayout.PropertyField(presetNameProp, new GUIContent("Preset Name"));

            EditorGUILayout.Space(10);

            PresetWriter writer = (PresetWriter)target;

            GUI.enabled = writer != null && parameterHubProp.objectReferenceValue != null && !string.IsNullOrEmpty(presetNameProp.stringValue);

            if (GUILayout.Button("Save Preset", GUILayout.Height(30)))
            {
                writer.WriteCurrentValuesAsPreset();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Click 'Save Preset' to save the current parameter values to the specified preset. If the preset doesn't exist, it will be created.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
