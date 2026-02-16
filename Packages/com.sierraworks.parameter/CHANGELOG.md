# Changelog

All notable changes to the PARAM package will be documented in this file.

## [1.0.0] - 2026-01-18

### Added
- Initial release of PARAM package
- Core parameter system with 7 supported types (Float, Int, Bool, String, Vector2, Vector3, Color)
- Parameter grouping system with Default group
- Preset system for managing different parameter configurations
- Custom dual-panel editor for PARAM Data ScriptableObject
- ParameterSender component for sending values to parameters
- ParameterReceiver component with reflection-based field/property synchronization
- PresetWriter component for capturing current values as presets
- PresetLoader component for runtime preset switching
- ParameterSetSerializer component for JSON save/load functionality
- ParameterSetExample script with code usage examples
- Custom editors for all components with dropdown menus
- GameObject menu items for quick component creation

### Features
- Drag and drop parameters between groups in editor
- Automatic parameter change notifications via events
- UnityEvent support in ParameterReceiver
- Type conversion for numeric types
- Human-readable JSON serialization
- Support for both full and incremental saves
- Persistent data path support for cross-session saves
