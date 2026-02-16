using UnityEngine;
using UnityEngine.Events;

namespace SierraWorks.PARAM
{
    public class PresetLoader : MonoBehaviour
    {
        [SerializeField] private ParameterSetData parameterHub;

        [System.Serializable]
        public class PresetLoadedEvent : UnityEvent<string> { }

        public PresetLoadedEvent onPresetLoaded;

        public void LoadPresetByIndex(int index)
        {
            if (parameterHub == null)
            {
                Debug.LogError("Parameter Hub is not assigned!");
                return;
            }

            if (index < 0 || index >= parameterHub.presets.Count)
            {
                Debug.LogError($"Invalid preset index: {index}");
                return;
            }

            parameterHub.currentPresetIndex = index;

            // Load the default values from the preset into the parameters' current values
            // and notify all receivers
            var currentPreset = parameterHub.CurrentPreset;
            if (currentPreset != null)
            {
                foreach (var group in parameterHub.groups)
                {
                    foreach (var param in group.parameters)
                    {
                        // Get the default value for this parameter in the current preset
                        var defaultValue = currentPreset.GetDefaultValue(param.ID);
                        if (defaultValue != null)
                        {
                            param.SetCurrentValue(defaultValue.GetValue(param.type));
                        }

                        // Notify receivers that parameter value has changed
                        parameterHub.NotifyParameterChanged(param);
                    }
                }

                Debug.Log($"Loaded preset: {currentPreset.presetName}");
                onPresetLoaded?.Invoke(currentPreset.presetName);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(parameterHub);
#endif
            }
        }

        public void LoadPresetByName(string presetName)
        {
            if (parameterHub == null)
            {
                Debug.LogError("Parameter Hub is not assigned!");
                return;
            }

            int index = parameterHub.presets.FindIndex(p => p.presetName == presetName);

            if (index < 0)
            {
                Debug.LogError($"Preset '{presetName}' not found!");
                return;
            }

            LoadPresetByIndex(index);
        }

        public void LoadNextPreset()
        {
            if (parameterHub == null) return;

            int nextIndex = (parameterHub.currentPresetIndex + 1) % parameterHub.presets.Count;
            LoadPresetByIndex(nextIndex);
        }

        public void LoadPreviousPreset()
        {
            if (parameterHub == null) return;

            int prevIndex = parameterHub.currentPresetIndex - 1;
            if (prevIndex < 0) prevIndex = parameterHub.presets.Count - 1;

            LoadPresetByIndex(prevIndex);
        }

        public string GetCurrentPresetName()
        {
            if (parameterHub == null || parameterHub.CurrentPreset == null)
            {
                return "";
            }

            return parameterHub.CurrentPreset.presetName;
        }

        public int GetCurrentPresetIndex()
        {
            if (parameterHub == null)
            {
                return -1;
            }

            return parameterHub.currentPresetIndex;
        }

        public int GetPresetCount()
        {
            if (parameterHub == null)
            {
                return 0;
            }

            return parameterHub.presets.Count;
        }
    }
}
