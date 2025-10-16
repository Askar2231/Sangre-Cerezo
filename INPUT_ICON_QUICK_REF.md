# Input Icon Mapper - Quick Reference

## What Changed?

✅ **New System**: Configure sprite names in Inspector instead of assigning sprite references  
✅ **More Flexible**: Easy to swap sprites by just changing names  
✅ **Device-Specific**: Automatically shows correct icons based on input device (Keyboard/Xbox/PlayStation)

## Quick Setup (3 Steps)

### 1. Configure Sprite Names in Inspector

Find `InputIconMapper` in your scene and fill in the **Sprite Name Mappings**:

```
Action: Run
├─ Keyboard Sprite Name: "Shift"
├─ Xbox Sprite Name: "Xbox_R3"
└─ PlayStation Sprite Name: "PS_R3"
```

### 2. Create TMP Sprite Asset

- Window → TextMeshPro → Sprite Importer
- Add all your input icon sprites
- Ensure sprite names match what you configured in Step 1

### 3. Use in Code

```csharp
// Simple placeholder replacement
string text = "Press {Run} to sprint";
string processed = InputIconMapper.Instance.ProcessTextPlaceholders(text);

// Get specific sprite name
string spriteName = InputIconMapper.Instance.GetSpriteNameForAction(InputAction.Run);
// Returns: "Shift" (keyboard), "Xbox_R3" (Xbox), or "PS_R3" (PS)
```

## Default Sprite Name Mappings

| Action      | Keyboard    | Xbox        | PlayStation |
| ----------- | ----------- | ----------- | ----------- |
| Move        | WASD        | Xbox_LS     | PS_LS       |
| Run         | **Shift**   | **Xbox_R3** | **PS_R3**   |
| Interact    | E           | Xbox_A      | PS_Cross    |
| Confirm     | Enter       | Xbox_A      | PS_Cross    |
| Cancel      | ESC         | Xbox_B      | PS_Circle   |
| LightAttack | Mouse_Left  | Xbox_X      | PS_Square   |
| HeavyAttack | Mouse_Right | Xbox_Y      | PS_Triangle |
| Skill1      | Q           | Xbox_LT     | PS_L2       |
| Skill2      | R           | Xbox_RT     | PS_R2       |
| EndTurn     | Tab         | Xbox_RB     | PS_R1       |

## API Reference

### Get Sprite Name

```csharp
string spriteName = InputIconMapper.Instance.GetSpriteNameForAction(InputAction.Run);
```

### Process Text Placeholders

```csharp
string text = InputIconMapper.Instance.ProcessTextPlaceholders("Press {Run} to sprint");
```

### Get Current Device

```csharp
InputDeviceType device = InputIconMapper.Instance.GetCurrentDeviceType();
```

### Listen to Device Changes

```csharp
InputIconMapper.Instance.OnDeviceChanged += (newDevice) => {
    Debug.Log($"Switched to: {newDevice}");
};
```

## Example: Running Action

**Goal**: Show "Shift" for keyboard, "R3" for PlayStation controllers

**Step 1 - Inspector Setup:**

```
Action: Run
Keyboard Sprite Name: "Shift"
Xbox Sprite Name: "Xbox_R3"
PlayStation Sprite Name: "PS_R3"
```

**Step 2 - In TMP Sprite Asset:**

- Add sprite named "Shift" (shift key icon)
- Add sprite named "Xbox_R3" (Xbox R3 icon)
- Add sprite named "PS_R3" (PlayStation R3 icon)

**Step 3 - In Code:**

```csharp
tutorialText.text = InputIconMapper.Instance.ProcessTextPlaceholders("Hold {Run} to sprint");
```

**Result:**

- Keyboard: "Hold **[Shift icon]** to sprint"
- PlayStation: "Hold **[R3 icon]** to sprint"

## Placeholder Syntax

Use these in your strings:

- `{Move}` - Movement
- `{Run}` - Run/Sprint
- `{Interact}` - Interact
- `{Confirm}` - Confirm
- `{Cancel}` - Cancel
- `{LightAttack}` - Light Attack
- `{HeavyAttack}` - Heavy Attack
- `{Skill1}` - Skill 1
- `{Skill2}` - Skill 2
- `{EndTurn}` - End Turn
- `{QTE}` - Quick Time Event
- `{Parry}` - Parry

## Tips

💡 **Sprite Naming**: Use consistent naming like `PS_Square`, `Xbox_A`, `Key_Shift`  
💡 **Fallback Text**: System shows text if sprite name is missing  
💡 **Debug Mode**: Enable in Inspector to see which sprites are being requested  
💡 **Hot Swapping**: Change sprite names at runtime if needed

## Files

- `InputIconMapper.cs` - Main system
- `TutorialEnums.cs` - InputAction and InputDeviceType enums
- `InputIconMapperExample.cs` - Usage examples
- `INPUT_ICON_SPRITE_SETUP.md` - Full documentation
