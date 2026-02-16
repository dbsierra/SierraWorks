using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

namespace SierraWorks.PARAM
{
    public class ParameterReceiver : MonoBehaviour
    {
        [SerializeField] private ParameterSetData parameterHub;
        [SerializeField] private string selectedParameterPath;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private string targetComponentName;
        [SerializeField] private string targetFieldName;

        [System.Serializable]
        public class ParameterChangedEvent : UnityEvent<object> { }

        public ParameterChangedEvent onParameterChanged;

        private Parameter cachedParameter;
        private Component targetComponent;
        private FieldInfo targetField;
        private PropertyInfo targetProperty;
        private bool isProperty;

        private void Start()
        {
            if (parameterHub != null)
            {
                CacheReferences();

                // Initialize parameter's current value from the effective default value on first start
                if (cachedParameter != null && parameterHub.CurrentPreset != null)
                {
                    var effectiveDefault = parameterHub.GetEffectiveDefaultValue(cachedParameter, parameterHub.CurrentPreset);
                    if (effectiveDefault != null)
                    {
                        cachedParameter.SetCurrentValue(effectiveDefault.GetValue(cachedParameter.type));
                    }
                }

                UpdateTargetValue();
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

        private void CacheReferences()
        {
            if (parameterHub == null || string.IsNullOrEmpty(selectedParameterPath))
            {
                cachedParameter = null;
                return;
            }

            // With the new architecture, there's only one Parameter instance per parameter
            cachedParameter = parameterHub.GetParameterByPath(selectedParameterPath);

            if (targetObject != null && !string.IsNullOrEmpty(targetComponentName))
            {
                Component[] components = targetObject.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp.GetType().Name == targetComponentName)
                    {
                        targetComponent = comp;
                        break;
                    }
                }

                if (targetComponent != null && !string.IsNullOrEmpty(targetFieldName))
                {
                    // Check if it's a field
                    targetField = targetComponent.GetType().GetField(targetFieldName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (targetField != null)
                    {
                        isProperty = false;
                    }
                    else
                    {
                        // Check if it's a property
                        targetProperty = targetComponent.GetType().GetProperty(targetFieldName,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (targetProperty != null)
                        {
                            isProperty = true;
                        }
                    }
                }
            }
        }

        private void OnParameterValueChanged(Parameter parameter)
        {
            // With the new architecture, we can compare by reference since there's only one instance
            if (cachedParameter != null && ReferenceEquals(parameter, cachedParameter))
            {
                UpdateTargetValue();
            }
        }

        private void UpdateTargetValue()
        {
            if (cachedParameter == null) return;

            object value = cachedParameter.GetCurrentValue();

            // Update target field/property
            if (targetComponent != null && value != null)
            {
                try
                {
                    if (isProperty && targetProperty != null && targetProperty.CanWrite)
                    {
                        object convertedValue = ConvertValue(value, targetProperty.PropertyType);
                        targetProperty.SetValue(targetComponent, convertedValue);
                    }
                    else if (!isProperty && targetField != null)
                    {
                        object convertedValue = ConvertValue(value, targetField.FieldType);
                        targetField.SetValue(targetComponent, convertedValue);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to set value on {targetComponentName}.{targetFieldName}: {e.Message}");
                }
            }

            // Invoke UnityEvent
            onParameterChanged?.Invoke(value);
        }

        private object ConvertValue(object value, System.Type targetType)
        {
            // Handle direct assignments
            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            // Handle conversions
            try
            {
                return System.Convert.ChangeType(value, targetType);
            }
            catch
            {
                Debug.LogWarning($"Cannot convert {value.GetType()} to {targetType}");
                return value;
            }
        }

        public void ManualUpdate()
        {
            CacheReferences();
            UpdateTargetValue();
        }

#if UNITY_EDITOR
        public void SetParameterHub(ParameterSetData hub)
        {
            parameterHub = hub;
        }

        public void SetSelectedParameter(string path)
        {
            selectedParameterPath = path;
        }

        public void SetTargetObject(GameObject obj)
        {
            targetObject = obj;
        }

        public void SetTargetComponent(string componentName)
        {
            targetComponentName = componentName;
        }

        public void SetTargetField(string fieldName)
        {
            targetFieldName = fieldName;
        }

        public ParameterSetData GetParameterHub()
        {
            return parameterHub;
        }

        public string GetSelectedParameterPath()
        {
            return selectedParameterPath;
        }

        public GameObject GetTargetObject()
        {
            return targetObject;
        }

        public string GetTargetComponentName()
        {
            return targetComponentName;
        }

        public string GetTargetFieldName()
        {
            return targetFieldName;
        }
#endif
    }
}
