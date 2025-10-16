# Input Icon Sprite Atlas Setup Guide

## Overview

The `InputIconMapper` system has been refactored to use **sprite name references** instead of direct sprite assignments. This makes it much more flexible and easier to manage, especially when working with TextMeshPro Sprite Assets.

## How It Works

Instead of assigning individual sprites for each input action and device type, you now configure **sprite names** that reference sprites in your TMP Sprite Atlas.

### Key Components

1. **InputActionSpriteMap** - A serializable class that maps each input action to sprite names for different devices
2. **Sprite Name Mappings** - A list in the Inspector where you configure sprite names for each action
3. **Automatic Device Detection** - The system automatically switches sprite names based on the active input device

## Setup Instructions

### 1. Configure Sprite Name Mappings

In the Unity Inspector, find the `InputIconMapper` component and locate the **Sprite Name Mappings** section:

```
Sprite Name Mappings
  └─ Size: 12 (one for each InputAction)
      ├─ Element 0
      │   ├─ Action: Move
      │   ├─ Keyboard Sprite Name: "WASD"
      │   ├─ Xbox Sprite Name: "Xbox_LS"
      │   └─ PlayStation Sprite Name: "PS_LS"
      ├─ Element 1
      │   ├─ Action: Run
      │   ├─ Keyboard Sprite Name: "Shift"
      │   ├─ Xbox Sprite Name: "Xbox_R3"
      │   └─ PlayStation Sprite Name: "PS_R3"
      └─ ...
```

### 2. Default Sprite Name Mappings

The system comes with default sprite name mappings:

| Action      | Keyboard    | Xbox    | PlayStation |
| ----------- | ----------- | ------- | ----------- |
| Move        | WASD        | Xbox_LS | PS_LS       |
| Run         | Shift       | Xbox_R3 | PS_R3       |
| Interact    | E           | Xbox_A  | PS_Cross    |
| QTE         | Space       | Xbox_A  | PS_Cross    |
| Parry       | Space       | Xbox_A  | PS_Cross    |
| Confirm     | Enter       | Xbox_A  | PS_Cross    |
| Cancel      | ESC         | Xbox_B  | PS_Circle   |
| LightAttack | Mouse_Left  | Xbox_X  | PS_Square   |
| HeavyAttack | Mouse_Right | Xbox_Y  | PS_Triangle |
| Skill1      | Q           | Xbox_LT | PS_L2       |
| Skill2      | R           | Xbox_RT | PS_R2       |
| EndTurn     | Tab         | Xbox_RB | PS_R1       |

### 3. Create Your TMP Sprite Asset

1. **Gather your input icon sprites** - Ensure all sprites referenced in the mappings are in your project
2. **Create a TMP Sprite Asset**:
   - Window → TextMeshPro → Sprite Importer
   - Drag all your input icon sprites
   - Make sure the sprite names match the names you configured in Step 1
3. **Assign the Sprite Asset to your TextMeshPro components**

### 4. Using the System

#### In Code

```csharp
// Get the sprite name for an action
string spriteName = InputIconMapper.Instance.GetSpriteNameForAction(InputAction.Run);
// Returns: "Shift" (keyboard), "Xbox_R3" (Xbox), or "PS_R3" (PlayStation)

// Get formatted text with sprite tags
string formattedText = InputIconMapper.Instance.ProcessTextPlaceholders("Press {Run} to sprint");
// Returns: "Press <sprite name=\"Shift\"> to sprint" (for keyboard)

// Get QTE icon directly
string qteIcon = InputIconMapper.Instance.GetIconForQTE();
```

#### In TextMeshPro

Simply use placeholders in your text:

```
"Press {Move} to move around"
"Hold {Run} to sprint"
"Press {Interact} to interact"
```

The system will automatically replace these with the appropriate sprite tags based on the active device.

## Customization

### Changing Sprite Names

You can customize sprite names at runtime or in the Inspector:

```csharp
// Find the mapping for a specific action
var runMapping = InputIconMapper.Instance.spriteNameMappings
    .Find(m => m.action == InputAction.Run);

// Update sprite names
runMapping.keyboardSpriteName = "Key_Shift_Alt";
runMapping.xboxSpriteName = "Xbox_RightStick_Click";

// Refresh the icon set
InputIconMapper.Instance.UpdateCurrentIconSet();
```

### Adding New Actions

1. Add the new action to the `InputAction` enum
2. Add a new `InputActionSpriteMap` to the mappings list
3. Configure sprite names for all device types

### Sprite Naming Conventions

Recommended naming conventions for your sprites in the atlas:

**Keyboard:**

- Keys: `Key_A`, `Key_Shift`, `Key_Space`, etc.
- Mouse: `Mouse_Left`, `Mouse_Right`, `Mouse_Middle`
- Combined: `WASD`, `Arrows`

**Xbox:**

- Buttons: `Xbox_A`, `Xbox_B`, `Xbox_X`, `Xbox_Y`
- Triggers: `Xbox_LT`, `Xbox_RT`, `Xbox_LB`, `Xbox_RB`
- Sticks: `Xbox_LS`, `Xbox_RS`, `Xbox_L3`, `Xbox_R3`
- D-Pad: `Xbox_DPad_Up`, `Xbox_DPad_Down`, etc.

**PlayStation:**

- Buttons: `PS_Cross`, `PS_Circle`, `PS_Square`, `PS_Triangle`
- Triggers: `PS_L1`, `PS_L2`, `PS_R1`, `PS_R2`
- Sticks: `PS_LS`, `PS_RS`, `PS_L3`, `PS_R3`
- D-Pad: `PS_DPad_Up`, `PS_DPad_Down`, etc.

## Example: Running Action Setup

For the **Run** action with keyboard using "Shift" and PlayStation using "R3":

1. **In Inspector:**

   ```
   Action: Run
   Keyboard Sprite Name: "Shift"
   Xbox Sprite Name: "Xbox_R3"
   PlayStation Sprite Name: "PS_R3"
   ```

2. **In your TMP Sprite Asset:**

   - Add sprite named "Shift" (keyboard shift key icon)
   - Add sprite named "Xbox_R3" (Xbox right stick press icon)
   - Add sprite named "PS_R3" (PlayStation right stick press icon)

3. **In your tutorial text:**

   ```
   "Hold {Run} to sprint"
   ```

4. **Result:**
   - Keyboard: "Hold <sprite name="Shift"> to sprint" → Shows shift key icon
   - PlayStation: "Hold <sprite name="PS_R3"> to sprint" → Shows R3 icon

## Benefits

✅ **Centralized Configuration** - All sprite mappings in one place  
✅ **No Direct Sprite References** - Just string names that reference your atlas  
✅ **Easy to Update** - Change sprite names without touching code  
✅ **Device-Specific Icons** - Automatically switches based on input device  
✅ **Inspector-Friendly** - Configure everything visually  
✅ **Runtime Flexibility** - Can modify mappings at runtime if needed

## Migration from Old System

The old system used direct `Sprite` references (`keyboardMove`, `xboxRun`, etc.). These have been removed in favor of string-based sprite names.

### What Changed

- ❌ Removed: `[SerializeField] private Sprite keyboardMove;`
- ✅ Added: `[SerializeField] private List<InputActionSpriteMap> spriteNameMappings;`

### Deprecated Methods

- `GetIconForAction(InputAction action)` - Returns null (use `GetSpriteNameForAction` instead)

### New Methods

- `GetSpriteNameForAction(InputAction action)` - Returns the sprite name string
- `InitializeDefaultMappings()` - Sets up default sprite name mappings

## Troubleshooting

### Icons Not Showing

1. **Check sprite names match** - Sprite names in mappings must exactly match names in TMP Sprite Asset
2. **Verify TMP Sprite Asset is assigned** - TextMeshPro component needs the sprite asset
3. **Enable debug mode** - Set `debugMode = true` in Inspector to see which sprites are being requested

### Wrong Device Icons Showing

1. **Check device detection** - The system auto-detects based on last input
2. **Force device type** - You can manually call `SetDeviceType()` if needed
3. **Verify mappings** - Ensure all three device types have sprite names configured

### Sprite Name Not Found

If you see warnings like "No se encontró sprite name para la acción":

1. Check the mapping exists in the Sprite Name Mappings list
2. Verify the sprite name field is not empty
3. Ensure the action enum matches exactly
