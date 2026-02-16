using UnityEngine;

namespace SierraWorks.Parameter
{
    /// <summary>
    /// Example script showing how to use ParameterHub from code.
    /// Attach this to a GameObject and assign a ParameterHub Data asset to see it in action.
    /// </summary>
    public class ParameterSetExample : MonoBehaviour
    {
        [SerializeField] private ParameterSetData parameterHub;

        private void Start()
        {
            if (parameterHub == null)
            {
                Debug.LogWarning("ParameterHub is not assigned to ParameterSetExample!");
                return;
            }

            // Subscribe to parameter change events
            parameterHub.OnParameterChanged += OnAnyParameterChanged;

            // Example: Get all group names
            var groups = parameterHub.GetAllGroupNames();
            Debug.Log($"Available groups: {string.Join(", ", groups)}");

            // Example: Get all parameters in a group
            if (groups.Count > 0)
            {
                var parameters = parameterHub.GetParametersInGroup(groups[0]);
                Debug.Log($"Parameters in {groups[0]}: {parameters.Count}");

                // Example: Set a parameter value
                if (parameters.Count > 0)
                {
                    var param = parameters[0];
                    Debug.Log($"Setting parameter {param.displayName}...");

                    switch (param.type)
                    {
                        case ParameterType.Float:
                            parameterHub.SetParameterValue(param.groupName, param.name, 42.5f);
                            break;
                        case ParameterType.Int:
                            parameterHub.SetParameterValue(param.groupName, param.name, 100);
                            break;
                        case ParameterType.Bool:
                            parameterHub.SetParameterValue(param.groupName, param.name, true);
                            break;
                        case ParameterType.String:
                            parameterHub.SetParameterValue(param.groupName, param.name, "Hello ParameterHub!");
                            break;
                    }
                }
            }

            // Example: Get a specific parameter
            var myParam = parameterHub.GetParameter("Default", "MyParameter");
            if (myParam != null)
            {
                Debug.Log($"Found parameter: {myParam.displayName}, Current value: {myParam.GetCurrentValue()}");
            }
        }

        private void OnAnyParameterChanged(Parameter parameter)
        {
            Debug.Log($"Parameter changed: {parameter.displayName} = {parameter.GetCurrentValue()} (Type: {parameter.type})");
        }

        private void OnDisable()
        {
            if (parameterHub != null)
            {
                parameterHub.OnParameterChanged -= OnAnyParameterChanged;
            }
        }

        // Example methods you can call from buttons or other components
        /*
        public void ResetAllParametersToDefault()
        {
            if (parameterHub == null || parameterHub.CurrentPreset == null) return;

            foreach (var group in parameterHub.CurrentPreset.groups)
            {
                foreach (var param in group.parameters)
                {
                    param.ResetToDefault();
                }
            }

            Debug.Log("All parameters reset to default values.");
        }

        public void LogAllParameterValues()
        {
            if (parameterHub == null || parameterHub.CurrentPreset == null) return;

            Debug.Log($"=== Parameter Values (Preset: {parameterHub.CurrentPreset.presetName}) ===");

            foreach (var group in parameterHub.CurrentPreset.groups)
            {
                Debug.Log($"Group: {group.groupName}");

                foreach (var param in group.parameters)
                {
                    Debug.Log($"  {param.displayName}: {param.GetCurrentValue()} (Default: {param.GetDefaultValue()})");
                }
            }
        }
        */
    }
}
