using UnityEngine;

namespace SierraWorks.PARAM
{
    public class ParameterSender : MonoBehaviour
    {
        [SerializeField] private ParameterSetData parameterHub;
        [SerializeField] private string selectedParameterPath;

        // Serialized values for each type
        [SerializeField] private float floatValue;
        [SerializeField] private int intValue;
        [SerializeField] private bool boolValue;
        [SerializeField] private string stringValue;
        [SerializeField] private Vector2 vector2Value;
        [SerializeField] private Vector3 vector3Value;
        [SerializeField] private Color colorValue;

        private Parameter cachedParameter;

        private void Awake()
        {
            if (parameterHub != null && !string.IsNullOrEmpty(selectedParameterPath))
            {
                cachedParameter = parameterHub.GetParameterByPath(selectedParameterPath);
            }
        }

        private void OnEnable()
        {
            if (parameterHub != null)
            {
                parameterHub.OnParameterChanged += OnParameterValueChanged;
            }
        }

        private void OnDisable()
        {
            if (parameterHub != null)
            {
                parameterHub.OnParameterChanged -= OnParameterValueChanged;
            }
        }

        private void OnParameterValueChanged(Parameter parameter)
        {
            if (cachedParameter == null) { return; }

            if (parameter.ID != cachedParameter.ID)
            {
                return;
            }

            switch (parameter.type)
            {
                case ParameterType.Float:
                    floatValue = parameter.floatCurrentValue;
                    break;
                case ParameterType.Int:
                    intValue = parameter.intCurrentValue;
                    break;
                case ParameterType.Bool:
                    boolValue = parameter.boolCurrentValue;
                    break;
                case ParameterType.String:
                    stringValue = parameter.stringCurrentValue;
                    break;
                case ParameterType.Vector2:
                    vector2Value = parameter.vector2CurrentValue;
                    break;
                case ParameterType.Vector3:
                    vector3Value = parameter.vector3CurrentValue;
                    break;
                case ParameterType.Color:
                    colorValue = parameter.colorCurrentValue;
                    break;
            }
        }

        public void SetValue()
        {
            if (cachedParameter == null || parameterHub == null) return;

            object valueToSet = null;

            switch (cachedParameter.type)
            {
                case ParameterType.Float:
                    valueToSet = floatValue;
                    break;
                case ParameterType.Int:
                    valueToSet = intValue;
                    break;
                case ParameterType.Bool:
                    valueToSet = boolValue;
                    break;
                case ParameterType.String:
                    valueToSet = stringValue;
                    break;
                case ParameterType.Vector2:
                    valueToSet = vector2Value;
                    break;
                case ParameterType.Vector3:
                    valueToSet = vector3Value;
                    break;
                case ParameterType.Color:
                    valueToSet = colorValue;
                    break;
            }

            if (valueToSet != null)
            {
                parameterHub.SetParameterValue(cachedParameter.groupName, cachedParameter.name, valueToSet);
            }
        }

        public void SetFloatValue(float value)
        {
            floatValue = value;
            SetValue();
        }

        public void SetIntValue(int value)
        {
            intValue = value;
            SetValue();
        }

        public void SetBoolValue(bool value)
        {
            boolValue = value;
            SetValue();
        }

        public void SetStringValue(string value)
        {
            stringValue = value;
            SetValue();
        }

        public void SetVector2Value(Vector2 value)
        {
            vector2Value = value;
            SetValue();
        }

        public void SetVector3Value(Vector3 value)
        {
            vector3Value = value;
            SetValue();
        }

        public void SetColorValue(Color value)
        {
            colorValue = value;
            SetValue();
        }

#if UNITY_EDITOR
        public void SetParameterHub(ParameterSetData hub)
        {
            parameterHub = hub;
        }

        public void SetSelectedParameter(string path)
        {
            selectedParameterPath = path;
            if (parameterHub != null)
            {
                cachedParameter = parameterHub.GetParameterByDisplayPath(path);
            }
        }

        public ParameterSetData GetParameterHub()
        {
            return parameterHub;
        }

        public string GetSelectedParameterPath()
        {
            return selectedParameterPath;
        }

        public Parameter GetCachedParameter()
        {
            if (cachedParameter == null && parameterHub != null && !string.IsNullOrEmpty(selectedParameterPath))
            {
                cachedParameter = parameterHub.GetParameterByPath(selectedParameterPath);
            }
            return cachedParameter;
        }
#endif
    }
}
