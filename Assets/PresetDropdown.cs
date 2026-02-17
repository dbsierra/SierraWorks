using UnityEngine;
using TMPro;

namespace SierraWorks.PARAM.Samples
{
    public class PresetDropdown : MonoBehaviour
    {
        [SerializeField] private ParameterSetData parameterSet;
        [SerializeField] private TMP_Dropdown dropdown;

        private void Start()
        {
            if (parameterSet == null || dropdown == null)
            {
                Debug.LogWarning("PresetDropdown: Please assign both a ParameterSetData asset and a TMP_Dropdown.");
                return;
            }

            PopulateDropdown();

            dropdown.onValueChanged.AddListener(OnPresetSelected);
        }

        private void OnDestroy()
        {
            if (dropdown != null)
            {
                dropdown.onValueChanged.RemoveListener(OnPresetSelected);
            }
        }

        private void PopulateDropdown()
        {
            dropdown.ClearOptions();

            var options = new System.Collections.Generic.List<string>();
            foreach (var preset in parameterSet.presets)
            {
                options.Add(preset.presetName);
            }

            dropdown.AddOptions(options);

            // Set the dropdown to reflect the current preset index
            dropdown.SetValueWithoutNotify(parameterSet.currentPresetIndex);
        }

        private void OnPresetSelected(int index)
        {
            if (index < 0 || index >= parameterSet.presets.Count) return;

            parameterSet.currentPresetIndex = index;
            parameterSet.ResetAllToDefaultsAndNotify();
        }
    }
}
