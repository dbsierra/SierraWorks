using UnityEngine;

namespace SierraWorks.PARAM
{
    public class PresetWriter : MonoBehaviour
    {
        [SerializeField] private ParameterSetData parameterHub;
        [SerializeField] private string presetName = "NewPreset";

        public void WriteCurrentValuesAsPreset()
        {
            if (parameterHub == null)
            {
                Debug.LogError("Parameter Hub is not assigned!");
                return;
            }

            if (string.IsNullOrEmpty(presetName))
            {
                Debug.LogError("Preset name cannot be empty!");
                return;
            }

            if (presetName == "Default")
            {
                Debug.LogError("Cannot use 'Default' as a preset name!");
                return;
            }

            // Check if preset exists
            var existingPreset = parameterHub.presets.Find(p => p.presetName == presetName);

            if (existingPreset == null)
            {
                // Create new preset
                Debug.Log($"Creating new preset: {presetName}");
                parameterHub.AddPreset(presetName);
                existingPreset = parameterHub.presets.Find(p => p.presetName == presetName);
            }
            else
            {
                Debug.Log($"Updating existing preset: {presetName}");
            }

            if (existingPreset == null)
            {
                Debug.LogError("Failed to create or find preset!");
                return;
            }

            // Copy current values to default values in the target preset
            foreach (var group in parameterHub.groups)
            {
                foreach (var param in group.parameters)
                {
                    object currentValue = param.GetCurrentValue();
                    existingPreset.SetDefaultValue(param.ID, param.type, currentValue);
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(parameterHub);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            Debug.Log($"Successfully saved current values to preset: {presetName}");
        }

        public void WriteCurrentValuesAsPreset(string newPresetName)
        {
            presetName = newPresetName;
            WriteCurrentValuesAsPreset();
        }

        public void SetPresetName(string newName)
        {
            presetName = newName;
        }
    }
}
