# InputActionSpriteMap - Standalone Class

## Overview

The `InputActionSpriteMap` class has been separated from `InputIconMapper.cs` into its own file for better code organization and reusability.

## File Location

```
Assets/Scripts/TutorialSystem/InputActionSpriteMap.cs
```

## Purpose

This class represents a mapping between an input action and its corresponding sprite names for different device types (Keyboard, Xbox, PlayStation).

## Class Structure

```csharp
[Serializable]
public class InputActionSpriteMap
{
    public InputAction action;
    public string keyboardSpriteName;
    public string xboxSpriteName;
    public string playStationSpriteName;
}
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `action` | `InputAction` | The input action this mapping applies to (Move, Run, Attack, etc.) |
| `keyboardSpriteName` | `string` | Sprite name for keyboard/mouse input (e.g., "Shift", "WASD") |
| `xboxSpriteName` | `string` | Sprite name for Xbox controller (e.g., "Xbox_A", "Xbox_R3") |
| `playStationSpriteName` | `string` | Sprite name for PlayStation controller (e.g., "PS_Cross", "PS_R3") |

## Usage

### In Inspector

This class is serializable and appears in the Unity Inspector as part of the `InputIconMapper` component:

```
InputIconMapper
└─ Sprite Name Mappings (List<InputActionSpriteMap>)
    ├─ Element 0
    │   ├─ Action: Run
    │   ├─ Keyboard Sprite Name: "Shift"
    │   ├─ Xbox Sprite Name: "Xbox_R3"
    │   └─ PlayStation Sprite Name: "PS_R3"
    └─ Element 1
        ├─ Action: Move
        ├─ Keyboard Sprite Name: "WASD"
        ├─ Xbox Sprite Name: "Xbox_LS"
        └─ PlayStation Sprite Name: "PS_LS"
```

### In Code

```csharp
// Create a new mapping
var runMapping = new InputActionSpriteMap
{
    action = InputAction.Run,
    keyboardSpriteName = "Shift",
    xboxSpriteName = "Xbox_R3",
    playStationSpriteName = "PS_R3"
};

// Access properties
Debug.Log($"Keyboard sprite: {runMapping.keyboardSpriteName}");
Debug.Log($"Xbox sprite: {runMapping.xboxSpriteName}");
Debug.Log($"PS sprite: {runMapping.playStationSpriteName}");

// Modify at runtime
runMapping.keyboardSpriteName = "Key_Shift_Alt";
```

### Creating Default Mappings

```csharp
List<InputActionSpriteMap> mappings = new List<InputActionSpriteMap>
{
    new InputActionSpriteMap 
    { 
        action = InputAction.Move,
        keyboardSpriteName = "WASD",
        xboxSpriteName = "Xbox_LS",
        playStationSpriteName = "PS_LS"
    },
    new InputActionSpriteMap 
    { 
        action = InputAction.Run,
        keyboardSpriteName = "Shift",
        xboxSpriteName = "Xbox_R3",
        playStationSpriteName = "PS_R3"
    }
};
```

## Why Separate This Class?

### Benefits of Separation

✅ **Better Organization** - Cleaner code structure with single responsibility  
✅ **Reusability** - Can be used by other systems if needed  
✅ **Easier Testing** - Can test the class independently  
✅ **Clearer Documentation** - Separate file makes documentation more focused  
✅ **Version Control** - Changes to this class don't affect InputIconMapper commits  

### Before Separation

```
InputIconMapper.cs (469 lines)
├─ InputIconMapper class
└─ InputActionSpriteMap class (nested)
```

### After Separation

```
InputIconMapper.cs (459 lines)
└─ InputIconMapper class

InputActionSpriteMap.cs (32 lines)
└─ InputActionSpriteMap class
```

## Related Classes

- **InputIconMapper** - Uses `List<InputActionSpriteMap>` to manage sprite mappings
- **InputAction** (enum) - Defines available input actions
- **InputDeviceType** (enum) - Defines device types (Keyboard, Xbox, PlayStation, Generic)

## Integration with InputIconMapper

The `InputIconMapper` uses this class internally:

```csharp
public class InputIconMapper : MonoBehaviour
{
    [SerializeField] 
    private List<InputActionSpriteMap> spriteNameMappings;
    
    private void UpdateCurrentIconSet()
    {
        foreach (var mapping in spriteNameMappings)
        {
            string spriteName = currentDeviceType switch
            {
                InputDeviceType.Keyboard => mapping.keyboardSpriteName,
                InputDeviceType.XboxController => mapping.xboxSpriteName,
                InputDeviceType.PlayStationController => mapping.playStationSpriteName,
                _ => mapping.xboxSpriteName
            };
            
            currentSpriteNameSet[mapping.action] = spriteName;
        }
    }
}
```

## Best Practices

### Naming Conventions

Use consistent sprite naming:

```csharp
// Keyboard sprites
keyboardSpriteName = "Key_Shift"   // or "Shift"
keyboardSpriteName = "WASD"
keyboardSpriteName = "Mouse_Left"

// Xbox sprites
xboxSpriteName = "Xbox_A"
xboxSpriteName = "Xbox_R3"
xboxSpriteName = "Xbox_LT"

// PlayStation sprites
playStationSpriteName = "PS_Cross"
playStationSpriteName = "PS_R3"
playStationSpriteName = "PS_L2"
```

### Validation

Consider adding validation:

```csharp
public bool IsValid()
{
    return !string.IsNullOrEmpty(keyboardSpriteName) ||
           !string.IsNullOrEmpty(xboxSpriteName) ||
           !string.IsNullOrEmpty(playStationSpriteName);
}

public string GetSpriteNameForDevice(InputDeviceType deviceType)
{
    return deviceType switch
    {
        InputDeviceType.Keyboard => keyboardSpriteName,
        InputDeviceType.XboxController => xboxSpriteName,
        InputDeviceType.PlayStationController => playStationSpriteName,
        InputDeviceType.GenericGamepad => xboxSpriteName,
        _ => ""
    };
}
```

## Migration Notes

No migration needed! The class was simply moved to its own file. All existing references and serialized data remain intact.

## Files

- `InputActionSpriteMap.cs` - The standalone class
- `InputIconMapper.cs` - Uses this class
- `TutorialEnums.cs` - Defines InputAction and InputDeviceType enums
