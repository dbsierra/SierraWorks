using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SierraWorks.PARAM.Editor
{
    [CustomEditor(typeof(PresetLoader))]
    public class PresetLoaderEditor : UnityEditor.Editor
    {
        private SerializedProperty parameterHubProp;
        private SerializedProperty onPresetLoadedProp;
        private int selectedPresetIndex = 0;

        private void OnEnable()
        {
            parameterHubProp = serializedObject.FindProperty("parameterHub");
            onPresetLoadedProp = serializedObject.FindProperty("onPresetLoaded");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the script field
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(parameterHubProp, new GUIContent("Parameter Hub"));

            PresetLoader loader = (PresetLoader)target;
            ParameterSetData hub = parameterHubProp.objectReferenceValue as ParameterSetData;

            if (hub != null && hub.presets != null && hub.presets.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Load Preset", EditorStyles.boldLabel);

                string[] presetNames = hub.presets.Select(p => p.presetName).ToArray();
                selectedPresetIndex = EditorGUILayout.Popup("Select Preset", selectedPresetIndex, presetNames);

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Load Selected Preset", GUILayout.Height(30)))
                {
                    loader.LoadPresetByIndex(selectedPresetIndex);
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Load Previous"))
                {
                    loader.LoadPreviousPreset();
                }

                if (GUILayout.Button("Load Next"))
                {
                    loader.LoadNextPreset();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox($"Current preset in ParameterHub: {hub.CurrentPreset?.presetName ?? "None"}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Please assign a Parameter Hub with at least one preset.", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onPresetLoadedProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
