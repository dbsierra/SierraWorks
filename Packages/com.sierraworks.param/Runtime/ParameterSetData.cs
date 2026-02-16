using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SierraWorks.PARAM
{
    [CreateAssetMenu(fileName = "ParameterSetData", menuName = "SierraWorks/PARAM/Parameter Set Data")]
    public class ParameterSetData : ScriptableObject
    {
        // Single source of truth for parameter definitions
        public List<ParameterGroup> groups = new List<ParameterGroup>();

        // Presets only store default values, not parameter definitions
        public List<ParameterPreset> presets = new List<ParameterPreset>();
        public int currentPresetIndex = 0;

        // Events for parameter changes
        public event Action<Parameter> OnParameterChanged;

        /// <summary>
        /// Notifies all listeners that a parameter value has changed.
        /// </summary>
        public void NotifyParameterChanged(Parameter parameter)
        {
            OnParameterChanged?.Invoke(parameter);
        }

        private void OnEnable()
        {
            if (groups == null || groups.Count == 0)
            {
                groups = new List<ParameterGroup>();
                groups.Add(new ParameterGroup("Default"));
            }

            if (presets == null || presets.Count == 0)
            {
                presets = new List<ParameterPreset>();
                presets.Add(new ParameterPreset("Default"));
            }

            // Ensure all presets have default values for all parameters
            foreach (var preset in presets)
            {
                preset.EnsureDefaultValuesExist(groups);
            }
        }

        public ParameterPreset CurrentPreset
        {
            get
            {
                if (presets.Count == 0) return null;
                if (currentPresetIndex >= presets.Count) currentPresetIndex = 0;
                return presets[currentPresetIndex];
            }
        }

        #region Preset Management

        public void AddPreset(string presetName)
        {
            if (presets.Any(p => p.presetName == presetName))
            {
                Debug.LogWarning($"Preset '{presetName}' already exists.");
                return;
            }

            var newPreset = new ParameterPreset(presetName);

            // Copy default values from current preset
            if (CurrentPreset != null)
            {
                foreach (var defaultValue in CurrentPreset.defaultValues)
                {
                    var newDefault = new ParameterDefaultValue(defaultValue.parameterId)
                    {
                        floatValue = defaultValue.floatValue,
                        intValue = defaultValue.intValue,
                        boolValue = defaultValue.boolValue,
                        stringValue = defaultValue.stringValue,
                        vector2Value = defaultValue.vector2Value,
                        vector3Value = defaultValue.vector3Value,
                        colorValue = defaultValue.colorValue,
                        // New presets should start with isOverridden = false so they inherit from Default
                        isOverridden = false
                    };
                    newPreset.defaultValues.Add(newDefault);
                }
            }
            else
            {
                // Ensure default values exist for all parameters
                newPreset.EnsureDefaultValuesExist(groups);
            }

            presets.Add(newPreset);
        }

        public void RemovePreset(int index)
        {
            if (index < 0 || index >= presets.Count) return;
            if (presets[index].presetName == "Default")
            {
                Debug.LogWarning("Cannot remove the Default preset.");
                return;
            }

            presets.RemoveAt(index);
            if (currentPresetIndex >= presets.Count)
            {
                currentPresetIndex = presets.Count - 1;
            }
        }

        #endregion

        #region Group Management

        public void AddGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                Debug.LogWarning("Invalid group name.");
                return;
            }

            if (!groups.Any(g => g.groupName == groupName))
            {
                groups.Add(new ParameterGroup(groupName));
            }
        }

        public void RemoveGroup(string groupName)
        {
            if (groups.Count <= 1)
            {
                Debug.LogWarning("Cannot remove the last remaining group.");
                return;
            }

            var group = groups.Find(g => g.groupName == groupName);
            if (group != null)
            {
                // Remove default values for all parameters in this group from all presets
                foreach (var param in group.parameters)
                {
                    foreach (var preset in presets)
                    {
                        preset.RemoveDefaultValue(param.ID);
                    }
                }

                groups.Remove(group);
            }
        }

        #endregion

        #region Parameter Management

        public void AddParameter(string groupName, Parameter parameter)
        {
            var group = groups.Find(g => g.groupName == groupName);
            if (group == null)
            {
                Debug.LogWarning($"Group '{groupName}' not found.");
                return;
            }

            // Set the group name on the parameter
            parameter.groupName = groupName;

            // Ensure the parameter has an ID
            string paramId = parameter.ID;

            group.parameters.Add(parameter);

            // Add default values for this parameter to all presets
            foreach (var preset in presets)
            {
                var defaultValue = new ParameterDefaultValue(paramId);

                // Default preset should have isOverridden = true (it's the source of truth)
                // All other presets should have isOverridden = false (they inherit from Default)
                defaultValue.isOverridden = (preset.presetName == "Default");

                preset.defaultValues.Add(defaultValue);
            }
        }

        public void RemoveParameter(string groupName, string parameterName)
        {
            var group = groups.Find(g => g.groupName == groupName);
            if (group == null) return;

            var param = group.parameters.Find(p => p.name == parameterName);
            if (param != null)
            {
                // Remove default values from all presets
                foreach (var preset in presets)
                {
                    preset.RemoveDefaultValue(param.ID);
                }

                group.parameters.Remove(param);
            }
        }

        public void MoveParameter(Parameter parameter, string fromGroup, string toGroup)
        {
            var fromGroupObj = groups.Find(g => g.groupName == fromGroup);
            var toGroupObj = groups.Find(g => g.groupName == toGroup);

            if (fromGroupObj != null && toGroupObj != null)
            {
                var param = fromGroupObj.parameters.Find(p => p.name == parameter.name);
                if (param != null)
                {
                    fromGroupObj.parameters.Remove(param);
                    param.groupName = toGroup;
                    toGroupObj.parameters.Add(param);
                }
            }
        }

        #endregion

        #region Parameter Access

        public Parameter GetParameter(string groupName, string parameterName)
        {
            var group = groups.Find(g => g.groupName == groupName);
            return group?.parameters.Find(p => p.name == parameterName);
        }

        public Parameter GetParameterByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            foreach (var group in groups)
            {
                foreach (var param in group.parameters)
                {
                    if (param.ID == id)
                    {
                        return param;
                    }
                }
            }
            return null;
        }

        public Parameter GetParameterByDisplayPath(string displayPath)
        {
            foreach (var group in groups)
            {
                foreach (var param in group.parameters)
                {
                    string fullName = group.groupName == "Default"
                        ? param.displayName
                        : $"{group.groupName}/{param.displayName}";

                    if (fullName == displayPath)
                    {
                        return param;
                    }
                }
            }

            // Fallback: try to find by internal name
            return GetParameterByInternalPath(displayPath);
        }

        public Parameter GetParameterByInternalPath(string internalPath)
        {
            foreach (var group in groups)
            {
                foreach (var param in group.parameters)
                {
                    string fullName = group.groupName == "Default"
                        ? param.name
                        : $"{group.groupName}/{param.name}";

                    if (fullName == internalPath)
                    {
                        return param;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a parameter by path, trying multiple strategies.
        /// First tries by ID, then display path, then internal path.
        /// </summary>
        public Parameter GetParameterByPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            // Strategy 1: Try as ID
            var paramById = GetParameterByID(path);
            if (paramById != null) return paramById;

            // Strategy 2: Try as display path
            var paramByDisplayPath = GetParameterByDisplayPath(path);
            if (paramByDisplayPath != null) return paramByDisplayPath;

            // Strategy 3: Try as internal path
            return GetParameterByInternalPath(path);
        }

        public string GetParameterInternalPath(Parameter param)
        {
            if (param == null) return "";
            return param.groupName == "Default"
                ? param.name
                : $"{param.groupName}/{param.name}";
        }

        public List<string> GetAllGroupNames()
        {
            return groups.Select(g => g.groupName).ToList();
        }

        public List<Parameter> GetParametersInGroup(string groupName)
        {
            var group = groups.Find(g => g.groupName == groupName);
            return group?.parameters ?? new List<Parameter>();
        }

        public List<string> GetAllParameterDisplayNames()
        {
            List<string> names = new List<string>();

            foreach (var group in groups)
            {
                foreach (var param in group.parameters)
                {
                    string fullName = group.groupName == "Default"
                        ? param.displayName
                        : $"{group.groupName}/{param.displayName}";
                    names.Add(fullName);
                }
            }
            return names;
        }

        #endregion

        #region Value Management

        /// <summary>
        /// Gets the default value for a parameter in the current preset.
        /// </summary>
        public object GetParameterDefaultValue(Parameter parameter)
        {
            if (parameter == null || CurrentPreset == null) return null;

            var defaultValue = CurrentPreset.GetDefaultValue(parameter.ID);
            return defaultValue?.GetValue(parameter.type);
        }

        /// <summary>
        /// Sets the default value for a parameter in the current preset.
        /// </summary>
        public void SetParameterDefaultValue(Parameter parameter, object value)
        {
            if (parameter == null || CurrentPreset == null) return;

            CurrentPreset.SetDefaultValue(parameter.ID, parameter.type, value);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Sets the current (runtime) value of a parameter and notifies listeners.
        /// </summary>
        public void SetParameterValue(string groupName, string parameterName, object value)
        {
            var parameter = GetParameter(groupName, parameterName);
            if (parameter != null)
            {
                parameter.SetCurrentValue(value);
                OnParameterChanged?.Invoke(parameter);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// Gets the effective default value for a parameter in a given preset.
        /// If the parameter is not overridden in that preset, falls back to the Default preset.
        /// </summary>
        public ParameterDefaultValue GetEffectiveDefaultValue(Parameter parameter, ParameterPreset preset)
        {
            if (parameter == null || preset == null) return null;

            var defaultValue = preset.GetDefaultValue(parameter.ID);
            if (defaultValue != null && !defaultValue.isOverridden)
            {
                // Fall back to Default preset
                var defaultPreset = presets.Find(p => p.presetName == "Default");
                if (defaultPreset != null)
                {
                    var fallback = defaultPreset.GetDefaultValue(parameter.ID);
                    if (fallback != null) return fallback;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Loads the default values from the current preset into the parameters' current values.
        /// </summary>
        public void LoadCurrentPresetDefaults()
        {
            if (CurrentPreset == null) return;

            foreach (var group in groups)
            {
                foreach (var param in group.parameters)
                {
                    var defaultValue = GetEffectiveDefaultValue(param, CurrentPreset);
                    if (defaultValue != null)
                    {
                        param.SetCurrentValue(defaultValue.GetValue(param.type));
                    }
                }
            }
        }

        /// <summary>
        /// Resets a specific parameter to its default value for the current preset.
        /// </summary>
        public void ResetParameterToDefault(Parameter parameter)
        {
            if (parameter == null || CurrentPreset == null) return;

            var defaultValue = GetEffectiveDefaultValue(parameter, CurrentPreset);
            if (defaultValue != null)
            {
                parameter.SetCurrentValue(defaultValue.GetValue(parameter.type));
                OnParameterChanged?.Invoke(parameter);
            }
        }

        /// <summary>
        /// Resets all parameters to their default values for the current preset and notifies listeners.
        /// </summary>
        public void ResetAllToDefaultsAndNotify()
        {
            if (CurrentPreset == null) return;

            foreach (var group in groups)
            {
                foreach (var param in group.parameters)
                {
                    var defaultValue = GetEffectiveDefaultValue(param, CurrentPreset);
                    if (defaultValue != null)
                    {
                        param.SetCurrentValue(defaultValue.GetValue(param.type));
                        OnParameterChanged?.Invoke(param);
                    }
                }
            }
        }

        /// <summary>
        /// Syncs default values from the Default preset to all non-overridden parameters in other presets.
        /// Call this after modifying the Default preset's values programmatically.
        /// </summary>
        public void SyncDefaultPresetToNonOverridden()
        {
            var defaultPreset = presets.Find(p => p.presetName == "Default");
            if (defaultPreset == null) return;

            foreach (var defaultValue in defaultPreset.defaultValues)
            {
                // For each parameter in the Default preset, sync to all other presets where not overridden
                foreach (var preset in presets)
                {
                    if (preset.presetName == "Default") continue;

                    var targetValue = preset.GetDefaultValue(defaultValue.parameterId);
                    if (targetValue != null && !targetValue.isOverridden)
                    {
                        // Copy all values
                        targetValue.floatValue = defaultValue.floatValue;
                        targetValue.intValue = defaultValue.intValue;
                        targetValue.boolValue = defaultValue.boolValue;
                        targetValue.stringValue = defaultValue.stringValue;
                        targetValue.vector2Value = defaultValue.vector2Value;
                        targetValue.vector3Value = defaultValue.vector3Value;
                        targetValue.colorValue = defaultValue.colorValue;
                    }
                }
            }
        }

        #endregion
    }
}
