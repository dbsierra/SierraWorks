# PARAM Quick Start Guide

Get up and running with PARAM in 5 minutes!

## Step 1: Create Your Parameter Hub Data (30 seconds)

1. In the Project window, right-click in any folder
2. Select `Create > SierraWorks > PARAM > Parameter Hub Data`
3. Name it something like "MyPARAM"

## Step 2: Add Some Parameters (2 minutes)

1. Select your PARAM asset
2. In the Inspector, you'll see:
   - **Preset Management** at the top (you're in the "Default" preset)
   - **Groups Panel** on the left (showing "Default" group)
   - **Parameters Panel** on the right

3. Add your first parameter:
   - Click "Add Parameter" button
   - Configure it:
     - Name: `speed`
     - Display Name: `Movement Speed`
     - Type: `Float`
     - Default Value: `5.0`
     - Current Value: `5.0`

4. Add a few more parameters if you want!

## Step 3: Send Values to a Parameter (1 minute)

1. In your scene, right-click in Hierarchy
2. Select `GameObject > PARAM > Parameter Sender`
3. In the Inspector:
   - Assign your PARAM asset
   - Select a parameter from the dropdown
   - Set a value
   - Click "Send Value Now" to test

**Pro Tip**: Hook up a UI slider's `OnValueChanged` event to the ParameterSender's `SetFloatValue` method!

## Step 4: Receive Parameter Values (1.5 minutes)

1. Create a GameObject with any component that has public fields (e.g., a Light)
2. Right-click in Hierarchy: `GameObject > PARAM > Parameter Receiver`
3. In the Inspector:
   - Assign your PARAM asset
   - Select the parameter you want to listen to
   - Set Target GameObject to your Light (or whatever)
   - Select the Component (e.g., "Light")
   - Select the Field/Property (e.g., "intensity")

4. Now when that parameter changes, your Light's intensity updates automatically!

## Step 5: Try Multiple Presets (Optional - 1 minute)

1. Select your PARAM asset
2. In Preset Management section:
   - Enter "FastMode" as the preset name
   - Click "Add Preset"
3. Switch to FastMode using the dropdown
4. Change the default values of your parameters (e.g., set speed to 10.0)
5. Switch back and forth between "Default" and "FastMode" presets

Notice how parameters reset to different defaults for each preset!

## Step 6: Save/Load at Runtime (Optional - 1 minute)

1. Right-click in Hierarchy: `GameObject > PARAM > Serializer`
2. Assign your PARAM asset
3. In Play mode:
   - Change some parameter values using your ParameterSender
   - Call `SaveCurrentValuesToJSON()` method (or add a button that calls it)
   - Stop Play mode
4. Enter Play mode again:
   - Call `LoadCurrentValuesFromJSON()` method
   - Your values are restored!

The JSON file is saved to `Application.persistentDataPath` and is human-readable!

## Common Use Cases

### Use Case 1: Game Settings
- Create parameters for volume, brightness, quality, etc.
- Use ParameterReceivers to sync to AudioListener volume, camera settings, etc.
- Use ParameterSetSerializer to save/load player preferences

### Use Case 2: Difficulty Presets
- Create presets: "Easy", "Normal", "Hard"
- Set different default values for enemy health, damage, spawn rates
- Use PresetLoader to switch between them at runtime

### Use Case 3: Visual Effects Control
- Create parameters for particle colors, sizes, speeds
- Use ParameterSenders from debug UI
- Use ParameterReceivers to update ParticleSystem properties
- Use PresetWriter to save good-looking configurations

## Next Steps

- Read the full README.md for detailed documentation
- Check out ParameterSetExample.cs for code examples
- Experiment with different parameter types (Vector3, Color, etc.)
- Try the UnityEvent on ParameterReceiver for custom logic

## Tips

- Parameters are grouped! Add groups to organize your parameters
- Drag parameters between groups in the editor
- The Default group and Default preset cannot be deleted
- Use meaningful Display Names - they appear in dropdowns
- Parameter changes trigger events that all receivers listen to

Happy Parameterizing! 🎮
