using System;
using UnityEngine;

namespace SierraWorks.PARAM
{
    [Serializable]
    public class Parameter
    {
        [SerializeField] private string id;
        public string name;
        public string displayName;
        public string groupName;
        public ParameterType type;

        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = System.Guid.NewGuid().ToString();
                }
                return id;
            }
        }

        // Current runtime values (non-serialized - these change at runtime)
        [System.NonSerialized] public float floatCurrentValue;
        [System.NonSerialized] public int intCurrentValue;
        [System.NonSerialized] public bool boolCurrentValue;
        [System.NonSerialized] public string stringCurrentValue;
        [System.NonSerialized] public Vector2 vector2CurrentValue;
        [System.NonSerialized] public Vector3 vector3CurrentValue;
        [System.NonSerialized] public Color colorCurrentValue;

        public Parameter()
        {
            name = "NewParameter";
            displayName = "New Parameter";
            groupName = "Default";
            type = ParameterType.Float;
        }

        public object GetCurrentValue()
        {
            switch (type)
            {
                case ParameterType.Float: return floatCurrentValue;
                case ParameterType.Int: return intCurrentValue;
                case ParameterType.Bool: return boolCurrentValue;
                case ParameterType.String: return stringCurrentValue;
                case ParameterType.Vector2: return vector2CurrentValue;
                case ParameterType.Vector3: return vector3CurrentValue;
                case ParameterType.Color: return colorCurrentValue;
                default: return null;
            }
        }

        public void SetCurrentValue(object value)
        {
            switch (type)
            {
                case ParameterType.Float:
                    floatCurrentValue = Convert.ToSingle(value);
                    break;
                case ParameterType.Int:
                    intCurrentValue = Convert.ToInt32(value);
                    break;
                case ParameterType.Bool:
                    boolCurrentValue = Convert.ToBoolean(value);
                    break;
                case ParameterType.String:
                    stringCurrentValue = value?.ToString() ?? "";
                    break;
                case ParameterType.Vector2:
                    vector2CurrentValue = (Vector2)value;
                    break;
                case ParameterType.Vector3:
                    vector3CurrentValue = (Vector3)value;
                    break;
                case ParameterType.Color:
                    colorCurrentValue = (Color)value;
                    break;
            }
        }
    }
}
