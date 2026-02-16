using UnityEngine;
using System.IO;

namespace SierraWorks.PARAM
{
    public class ParameterSetSerializer : MonoBehaviour
    {
        [SerializeField] private ParameterSetData parameterHub;
        [SerializeField] private string saveFileName = "ParameterHubSave.json";
        [SerializeField] private bool usePersistentDataPath = true;

        private string SavePath
        {
            get
            {
                if (usePersistentDataPath)
                {
                    return Path.Combine(Application.persistentDataPath, saveFileName);
                }
                else
                {
                    return Path.Combine(Application.dataPath, saveFileName);
                }
            }
        }

        public void SaveToJSON()
        {
            if (parameterHub == null)
            {
                Debug.LogError("Parameter Hub is not assigned!");
                return;
            }

            try
            {
                ParameterHubSaveData saveData = new ParameterHubSaveData(parameterHub);
                string json = JsonUtility.ToJson(saveData, true);

                File.WriteAllText(SavePath, json);

                Debug.Log($"Parameter Hub saved to: {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save Parameter Hub: {e.Message}");
            }
        }

        public void LoadFromJSON()
        {
            if (parameterHub == null)
            {
                Debug.LogError("Parameter Hub is not assigned!");
                return;
            }

            if (!File.Exists(SavePath))
            {
                Debug.LogWarning($"Save file not found at: {SavePath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                ParameterHubSaveData saveData = JsonUtility.FromJson<ParameterHubSaveData>(json);

                // Clear existing data
                parameterHub.groups.Clear();
                parameterHub.presets.Clear();

                // Restore groups and parameters
                foreach (var groupSaveData in saveData.groups)
                {
                    var group = new ParameterGroup(groupSaveData.groupName);

                    foreach (var paramSaveData in groupSaveData.parameters)
                    {
                        var param = new Parameter
                        {
                            name = paramSaveData.name,
                            displayName = paramSaveData.displayName,
                            groupName = paramSaveData.groupName,
                            type = paramSaveData.type
                        };
                        paramSaveData.ApplyCurrentValueToParameter(param);
                        group.parameters.Add(param);
                    }

                    parameterHub.groups.Add(group);
                }

                // Restore presets
                foreach (var presetSaveData in saveData.presets)
                {
                    var preset = new ParameterPreset(presetSaveData.presetName);

                    foreach (var defaultValueSaveData in presetSaveData.defaultValues)
                    {
                        preset.defaultValues.Add(defaultValueSaveData.ToParameterDefaultValue());
                    }

                    parameterHub.presets.Add(preset);
                }

                parameterHub.currentPresetIndex = saveData.currentPresetIndex;

                Debug.Log($"Parameter Hub loaded from: {SavePath}");

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(parameterHub);
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load Parameter Hub: {e.Message}");
            }
        }

        public void SaveCurrentValuesToJSON()
        {
            if (parameterHub == null)
            {
                Debug.LogError("Parameter Hub is not assigned!");
                return;
            }

            try
            {
                // Create a simplified save that only stores current values
                ParameterHubSaveData saveData = new ParameterHubSaveData();
                saveData.currentPresetIndex = parameterHub.currentPresetIndex;

                // Save groups with current values
                foreach (var group in parameterHub.groups)
                {
                    var groupData = new GroupSaveData();
                    groupData.groupName = group.groupName;

                    foreach (var param in group.parameters)
                    {
                        groupData.parameters.Add(new ParameterSetSaveData(param));
                    }

                    saveData.groups.Add(groupData);
                }

                string json = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(SavePath, json);

                Debug.Log($"Current parameter values saved to: {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save current values: {e.Message}");
            }
        }

        public void LoadCurrentValuesFromJSON()
        {
            if (parameterHub == null)
            {
                Debug.LogError("Parameter Hub is not assigned!");
                return;
            }

            if (!File.Exists(SavePath))
            {
                Debug.LogWarning($"Save file not found at: {SavePath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                ParameterHubSaveData saveData = JsonUtility.FromJson<ParameterHubSaveData>(json);

                if (saveData.groups.Count == 0)
                {
                    Debug.LogWarning("No group data found in save file.");
                    return;
                }

                // Load the current values from the saved groups
                foreach (var savedGroup in saveData.groups)
                {
                    var group = parameterHub.groups.Find(g => g.groupName == savedGroup.groupName);
                    if (group == null) continue;

                    foreach (var savedParam in savedGroup.parameters)
                    {
                        var param = group.parameters.Find(p => p.name == savedParam.name);
                        if (param != null)
                        {
                            savedParam.ApplyCurrentValueToParameter(param);
                        }
                    }
                }

                Debug.Log($"Current parameter values loaded from: {SavePath}");

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(parameterHub);
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load current values: {e.Message}");
            }
        }

        public string GetSavePath()
        {
            return SavePath;
        }
    }
}
