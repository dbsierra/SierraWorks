using System;
using UnityEngine;

namespace SierraWorks.Parameter
{
    /// <summary>
    /// Stores the default value for a parameter within a specific preset.
    /// References the parameter by its unique ID.
    /// </summary>
    [Serializable]
    public class ParameterDefaultValue
    {
        public string parameterId;

        // If true, this preset has its own value; if false, it follows the Default preset's value
        public bool isOverridden = true;

        // Store all possible type values (only one is used based on parameter type)
        public float floatValue;
        public int intValue;
        public bool boolValue;
        public string stringValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Color colorValue;

        public ParameterDefaultValue() { }

        public ParameterDefaultValue(string paramId)
        {
            parameterId = paramId;
        }

        public object GetValue(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Float: return floatValue;
                case ParameterType.Int: return intValue;
                case ParameterType.Bool: return boolValue;
                case ParameterType.String: return stringValue;
                case ParameterType.Vector2: return vector2Value;
                case ParameterType.Vector3: return vector3Value;
                case ParameterType.Color: return colorValue;
                default: return null;
            }
        }

        public void SetValue(ParameterType type, object value)
        {
            switch (type)
            {
                case ParameterType.Float:
                    floatValue = Convert.ToSingle(value);
                    break;
                case ParameterType.Int:
                    intValue = Convert.ToInt32(value);
                    break;
                case ParameterType.Bool:
                    boolValue = Convert.ToBoolean(value);
                    break;
                case ParameterType.String:
                    stringValue = value?.ToString() ?? "";
                    break;
                case ParameterType.Vector2:
                    vector2Value = (Vector2)value;
                    break;
                case ParameterType.Vector3:
                    vector3Value = (Vector3)value;
                    break;
                case ParameterType.Color:
                    colorValue = (Color)value;
                    break;
            }
        }
    }
}
