using System;
using System.Collections.Generic;

namespace SierraWorks.PARAM
{
    [Serializable]
    public class ParameterSetSaveData
    {
        public string id;
        public string name;
        public string displayName;
        public string groupName;
        public ParameterType type;

        public float floatValue;
        public int intValue;
        public bool boolValue;
        public string stringValue;

        // Unity types need special handling
        public float[] vector2Value = new float[2];
        public float[] vector3Value = new float[3];
        public float[] colorValue = new float[4];

        public ParameterSetSaveData() { }

        public ParameterSetSaveData(Parameter param)
        {
            id = param.ID;
            name = param.name;
            displayName = param.displayName;
            groupName = param.groupName;
            type = param.type;

            switch (type)
            {
                case ParameterType.Float:
                    floatValue = param.floatCurrentValue;
                    break;
                case ParameterType.Int:
                    intValue = param.intCurrentValue;
                    break;
                case ParameterType.Bool:
                    boolValue = param.boolCurrentValue;
                    break;
                case ParameterType.String:
                    stringValue = param.stringCurrentValue;
                    break;
                case ParameterType.Vector2:
                    vector2Value[0] = param.vector2CurrentValue.x;
                    vector2Value[1] = param.vector2CurrentValue.y;
                    break;
                case ParameterType.Vector3:
                    vector3Value[0] = param.vector3CurrentValue.x;
                    vector3Value[1] = param.vector3CurrentValue.y;
                    vector3Value[2] = param.vector3CurrentValue.z;
                    break;
                case ParameterType.Color:
                    colorValue[0] = param.colorCurrentValue.r;
                    colorValue[1] = param.colorCurrentValue.g;
                    colorValue[2] = param.colorCurrentValue.b;
                    colorValue[3] = param.colorCurrentValue.a;
                    break;
            }
        }

        public void ApplyCurrentValueToParameter(Parameter param)
        {
            switch (type)
            {
                case ParameterType.Float:
                    param.floatCurrentValue = floatValue;
                    break;
                case ParameterType.Int:
                    param.intCurrentValue = intValue;
                    break;
                case ParameterType.Bool:
                    param.boolCurrentValue = boolValue;
                    break;
                case ParameterType.String:
                    param.stringCurrentValue = stringValue;
                    break;
                case ParameterType.Vector2:
                    param.vector2CurrentValue = new UnityEngine.Vector2(vector2Value[0], vector2Value[1]);
                    break;
                case ParameterType.Vector3:
                    param.vector3CurrentValue = new UnityEngine.Vector3(vector3Value[0], vector3Value[1], vector3Value[2]);
                    break;
                case ParameterType.Color:
                    param.colorCurrentValue = new UnityEngine.Color(colorValue[0], colorValue[1], colorValue[2], colorValue[3]);
                    break;
            }
        }
    }

    [Serializable]
    public class DefaultValueSaveData
    {
        public string parameterId;
        public float floatValue;
        public int intValue;
        public bool boolValue;
        public string stringValue;
        public float[] vector2Value = new float[2];
        public float[] vector3Value = new float[3];
        public float[] colorValue = new float[4];

        public DefaultValueSaveData() { }

        public DefaultValueSaveData(ParameterDefaultValue defaultValue)
        {
            parameterId = defaultValue.parameterId;
            floatValue = defaultValue.floatValue;
            intValue = defaultValue.intValue;
            boolValue = defaultValue.boolValue;
            stringValue = defaultValue.stringValue;
            vector2Value[0] = defaultValue.vector2Value.x;
            vector2Value[1] = defaultValue.vector2Value.y;
            vector3Value[0] = defaultValue.vector3Value.x;
            vector3Value[1] = defaultValue.vector3Value.y;
            vector3Value[2] = defaultValue.vector3Value.z;
            colorValue[0] = defaultValue.colorValue.r;
            colorValue[1] = defaultValue.colorValue.g;
            colorValue[2] = defaultValue.colorValue.b;
            colorValue[3] = defaultValue.colorValue.a;
        }

        public ParameterDefaultValue ToParameterDefaultValue()
        {
            return new ParameterDefaultValue(parameterId)
            {
                floatValue = floatValue,
                intValue = intValue,
                boolValue = boolValue,
                stringValue = stringValue,
                vector2Value = new UnityEngine.Vector2(vector2Value[0], vector2Value[1]),
                vector3Value = new UnityEngine.Vector3(vector3Value[0], vector3Value[1], vector3Value[2]),
                colorValue = new UnityEngine.Color(colorValue[0], colorValue[1], colorValue[2], colorValue[3])
            };
        }
    }

    [Serializable]
    public class GroupSaveData
    {
        public string groupName;
        public List<ParameterSetSaveData> parameters = new List<ParameterSetSaveData>();

        public GroupSaveData() { }

        public GroupSaveData(ParameterGroup group)
        {
            groupName = group.groupName;
            foreach (var param in group.parameters)
            {
                parameters.Add(new ParameterSetSaveData(param));
            }
        }
    }

    [Serializable]
    public class PresetSaveData
    {
        public string presetName;
        public List<DefaultValueSaveData> defaultValues = new List<DefaultValueSaveData>();

        public PresetSaveData() { }

        public PresetSaveData(ParameterPreset preset)
        {
            presetName = preset.presetName;
            foreach (var defaultValue in preset.defaultValues)
            {
                defaultValues.Add(new DefaultValueSaveData(defaultValue));
            }
        }
    }

    [Serializable]
    public class ParameterHubSaveData
    {
        public List<GroupSaveData> groups = new List<GroupSaveData>();
        public List<PresetSaveData> presets = new List<PresetSaveData>();
        public int currentPresetIndex = 0;

        public ParameterHubSaveData() { }

        public ParameterHubSaveData(ParameterSetData hubData)
        {
            currentPresetIndex = hubData.currentPresetIndex;

            foreach (var group in hubData.groups)
            {
                groups.Add(new GroupSaveData(group));
            }

            foreach (var preset in hubData.presets)
            {
                presets.Add(new PresetSaveData(preset));
            }
        }
    }
}
