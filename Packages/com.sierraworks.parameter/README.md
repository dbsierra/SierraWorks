# ParameterHub Package

A comprehensive Unity package for creating, managing, and synchronizing custom parameters with built-in preset support and JSON serialization.

## Features

- **Parameter Management**: Create and organize parameters with different types (Float, Int, Bool, String, Vector2, Vector3, Color)
- **Group Organization**: Organize parameters into groups with a dual-panel editor interface
- **Preset System**: Create multiple presets with different default values
- **ParameterSender**: Send values to parameters from components or UI
- **ParameterReceiver**: Automatically sync parameter values to component fields/properties using reflection
- **JSON Serialization**: Save and load parameter data in human-readable JSON format
- **UnityEvents**: Hook up parameter changes to UnityEvents for flexible integration

## Getting Started

### 1. Create a ParameterHub Data Asset

Right-click in your Project window:
- `Create > ParameterHub > Parameter Hub Data`

This creates a ScriptableObject that holds all your parameters and presets.

### 2. Setting Up Parameters

Select your ParameterHub Data asset to open the custom inspector:

- **Left Panel (Groups)**:
  - View all parameter groups
  - Add new groups using the text field and "+" button
  - Select a group to view its parameters
  - Remove groups (except Default) with "Remove Group" button
  - Drag parameters between groups

- **Right Panel (Parameters)**:
  - View all parameters in the selected group
  - Add new parameters with "Add Parameter" button
  - Configure parameter properties:
    - Name: Internal identifier
    - Display Name: User-friendly name for UI
    - Type: Variable type
    - Default Value: Initial value
    - Current Value: Runtime value
  - Remove parameters with the "×" button
  - Reset individual parameters to default values

### 3. Creating Presets

At the top of the inspector:
- Use the dropdown to switch between presets
- Enter a name and click "Add Preset" to create a new preset
- Each preset has the same parameter structure but different default values
- Remove presets (except Default) with "Remove Current Preset"

## Components

### ParameterSender

Sends values to parameters from your game objects.

**Usage**:
1. Add `ParameterSender` component to a GameObject
2. Assign your ParameterHub Data asset
3. Select a parameter from the dropdown
4. Set the value you want to send
5. Call `SetValue()` or type-specific methods:
   - `SetFloatValue(float value)`
   - `SetIntValue(int value)`
   - `SetBoolValue(bool value)`
   - `SetStringValue(string value)`
   - `SetVector2Value(Vector2 value)`
   - `SetVector3Value(Vector3 value)`
   - `SetColorValue(Color value)`

**Example**: Hook up a UI Slider's `OnValueChanged` event to `SetFloatValue()` to control a parameter.

### ParameterReceiver

Automatically receives parameter changes and updates target component fields/properties.

**Usage**:
1. Add `ParameterReceiver` component to a GameObject
2. Assign your ParameterHub Data asset
3. Select a parameter from the dropdown
4. Assign a Target GameObject
5. Select the Component on that GameObject
6. Select the Field/Property to update
7. (Optional) Use the `onParameterChanged` UnityEvent for additional logic

The component automatically updates the target field/property whenever the parameter value changes.

**Reflection Support**:
- Public fields
- Serialized private fields (with `[SerializeField]`)
- Public properties with setters
- Automatic type conversion for numeric types

### PresetWriter

Captures current parameter values and saves them as a preset.

**Usage**:
1. Add `PresetWriter` component to a GameObject
2. Assign your ParameterHub Data asset
3. Set the preset name
4. Call `WriteCurrentValuesAsPreset()` to save all current values as defaults for that preset
   - Creates a new preset if it doesn't exist
   - Updates an existing preset if it already exists

**Methods**:
- `WriteCurrentValuesAsPreset()`: Saves using the preset name in the inspector
- `WriteCurrentValuesAsPreset(string presetName)`: Saves with a specified name
- `SetPresetName(string name)`: Changes the preset name

### ParameterSetSerializer

Handles JSON serialization for saving/loading parameter data at runtime.

**Usage**:
1. Add `ParameterSetSerializer` component to a GameObject
2. Assign your ParameterHub Data asset
3. Configure save settings:
   - `saveFileName`: Name of the JSON file
   - `usePersistentDataPath`: Use Application.persistentDataPath (recommended) vs Application.dataPath

**Methods**:

**Full Save/Load** (includes all presets and structure):
- `SaveToJSON()`: Saves entire ParameterHub with all presets
- `LoadFromJSON()`: Loads and replaces all presets from JSON

**Current Values Only** (saves only current values of active preset):
- `SaveCurrentValuesToJSON()`: Saves only current parameter values
- `LoadCurrentValuesFromJSON()`: Loads only values (preserves structure)

**Utility**:
- `GetSavePath()`: Returns the full path where files are saved

**JSON Format**: The JSON files are human-readable and can be edited in any text editor.

## Example Workflows

### Workflow 1: UI Slider controlling a Light Intensity

1. Create a Float parameter called "LightIntensity" in the Default group
2. Add a `ParameterSender` to your UI Slider GameObject
3. Hook the slider's `OnValueChanged` to `ParameterSender.SetFloatValue()`
4. Add a `ParameterReceiver` to a GameObject
5. Select the "LightIntensity" parameter
6. Set Target GameObject to your Light
7. Select the Light component
8. Select the "intensity" field

Now your slider controls the light intensity through the parameter system!

### Workflow 2: Saving Player Settings

1. Create parameters for various settings (master volume, graphics quality, etc.)
2. Use `ParameterSender` components on your UI to change these values
3. Add a `ParameterSetSerializer` to a settings manager GameObject
4. Call `SaveCurrentValuesToJSON()` when the player clicks "Apply Settings"
5. Call `LoadCurrentValuesFromJSON()` on game start to restore settings

### Workflow 3: Multiple Configuration Presets

1. Create parameters for your game configuration
2. Create presets for "Easy", "Normal", "Hard" difficulties
3. Adjust the default values for each preset in the ParameterHub inspector
4. At runtime, switch presets by changing `parameterHub.currentPresetIndex`
5. All parameters update to their preset defaults automatically

## Tips

- Use meaningful Display Names for UI presentation
- Group related parameters together for organization
- The Default group and Default preset cannot be removed
- Parameter changes trigger the `OnParameterChanged` event for receivers
- JSON saves are stored in `Application.persistentDataPath` by default for persistence across sessions
- Use "Send Value Now" button in ParameterSender inspector for testing

## Script Access

Access parameters from your own scripts:

```csharp
using SierraWorks.Parameter;

public class MyScript : MonoBehaviour
{
    [SerializeField] private ParameterSetData parameterHub;

    void Start()
    {
        // Get a parameter
        Parameter param = parameterHub.GetParameter("Default", "MyParameter");

        // Get current value
        float value = (float)param.GetCurrentValue();

        // Set a value
        parameterHub.SetParameterValue("Default", "MyParameter", 5.0f);

        // Listen for changes
        parameterHub.OnParameterChanged += OnParameterChanged;
    }

    void OnParameterChanged(Parameter param)
    {
        Debug.Log($"Parameter {param.displayName} changed to {param.GetCurrentValue()}");
    }
}
```

## Package Structure

```
Packages/com.SierraWorks.Parameter/
├── package.json
├── README.md
├── QUICKSTART.md
├── Runtime/
│   ├── SierraWorks.Parameter.Runtime.asmdef
│   ├── ParameterType.cs
│   ├── Parameter.cs
│   ├── ParameterGroup.cs
│   ├── ParameterPreset.cs
│   ├── ParameterSetData.cs
│   ├── ParameterSender.cs
│   ├── ParameterReceiver.cs
│   ├── PresetWriter.cs
│   ├── PresetLoader.cs
│   ├── ParameterHubSaveData.cs
│   ├── ParameterSetSerializer.cs
│   └── ParameterSetExample.cs
└── Editor/
    ├── SierraWorks.Parameter.Editor.asmdef
    ├── ParameterSetDataEditor.cs
    ├── ParameterSenderEditor.cs
    ├── ParameterReceiverEditor.cs
    └── ParameterHubMenuItems.cs
```

## Installation

This package is already installed in your project's `Packages` folder. Unity will automatically recognize it and include it in your project.

## License

Free to use in your projects.
