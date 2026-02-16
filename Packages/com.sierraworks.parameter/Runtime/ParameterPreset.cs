using System;
using System.Collections.Generic;

namespace SierraWorks.Parameter
{
    /// <summary>
    /// A preset stores a name and a collection of default values for parameters.
    /// The parameters themselves are defined in ParameterSetData.
    /// </summary>
    [Serializable]
    public class ParameterPreset
    {
        public string presetName;
        public List<ParameterDefaultValue> defaultValues = new List<ParameterDefaultValue>();

        public ParameterPreset(string name)
        {
            presetName = name;
        }

        /// <summary>
        /// Gets the default value entry for a parameter by its ID.
        /// </summary>
        public ParameterDefaultValue GetDefaultValue(string parameterId)
        {
            return defaultValues.Find(dv => dv.parameterId == parameterId);
        }

        /// <summary>
        /// Sets or creates the default value for a parameter.
        /// </summary>
        public void SetDefaultValue(string parameterId, ParameterType type, object value)
        {
            var existing = defaultValues.Find(dv => dv.parameterId == parameterId);
            if (existing != null)
            {
                existing.SetValue(type, value);
            }
            else
            {
                var newDefault = new ParameterDefaultValue(parameterId);
                newDefault.SetValue(type, value);
                defaultValues.Add(newDefault);
            }
        }

        /// <summary>
        /// Removes the default value entry for a parameter.
        /// </summary>
        public void RemoveDefaultValue(string parameterId)
        {
            defaultValues.RemoveAll(dv => dv.parameterId == parameterId);
        }

        /// <summary>
        /// Ensures all parameters have default value entries.
        /// </summary>
        public void EnsureDefaultValuesExist(List<ParameterGroup> groups)
        {
            foreach (var group in groups)
            {
                foreach (var param in group.parameters)
                {
                    if (GetDefaultValue(param.ID) == null)
                    {
                        var defaultValue = new ParameterDefaultValue(param.ID);

                        // Default preset should have isOverridden = true (it's the source of truth)
                        // All other presets should have isOverridden = false (they inherit from Default)
                        defaultValue.isOverridden = (presetName == "Default");

                        defaultValues.Add(defaultValue);
                    }
                }
            }
        }
    }
}
