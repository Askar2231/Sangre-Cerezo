# Input System Setup for Combat

## Overview
Both **QTEManager** and **ParrySystem** have been updated to use Unity's **Input System** (InputActionReference) instead of the old KeyCode-based input. This provides better controller support, rebinding capabilities, and integration with Unity's modern input pipeline.

---

## 🎮 What Changed

### Before (Old Input System)
```csharp
[SerializeField] private KeyCode qteInputKey = KeyCode.Space;

if (Input.GetKeyDown(qteInputKey))
{
    // Handle input
}
```

### After (New Input System)
```csharp
[SerializeField] private InputActionReference qteInputAction;

if (qteInputAction != null && qteInputAction.action.WasPressedThisFrame())
{
    // Handle input
}
```

---

## 📋 Setup Steps

### 1. Create Input Actions Asset

1. **Create the Asset:**
   - Right-click in Project window → `Create` → `Input Actions`
   - Name it: `CombatInputActions`
   - Location: `Assets/Input/` (create folder if needed)

2. **Open the Asset:**
   - Double-click `CombatInputActions`
   - You'll see the Input Actions window

3. **Create Action Map:**
   - Click `+` to add new Action Map
   - Name it: `Combat`

4. **Create Actions:**

   **a) QTE Action:**
   - In the Combat map, click `+` to add Action
   - Name: `QTE`
   - Action Type: `Button`
   - Click `+` next to QTE to add binding
   - Press `Space` key (or desired key)
   - Click `+` again for gamepad support
   - Select: `Gamepad > Button South (A/Cross)`

   **b) Parry Action:**
   - Click `+` to add another Action
   - Name: `Parry`
   - Action Type: `Button`
   - Add binding: `Space` key
   - Add binding: `Gamepad > Button South (A/Cross)`

5. **Save:**
   - Click `Save Asset` in top-left
   - Close Input Actions window

### 2. Configure Components in Scene

#### QTEManager Setup

1. Select your `BattleManager` GameObject (or wherever QTEManager is attached)
2. Find the **QTE Manager (Script)** component
3. **QTE Input Action:**
   - Click the circle target icon next to `Qte Input Action`
   - Navigate: `CombatInputActions > Combat > QTE`
   - Select it

#### ParrySystem Setup

1. Select your `BattleManager` GameObject (or wherever ParrySystem is attached)
2. Find the **Parry System (Script)** component
3. **Parry Input Action:**
   - Click the circle target icon next to `Parry Input Action`
   - Navigate: `CombatInputActions > Combat > Parry`
   - Select it

---

## 🎯 Inspector Configuration

### QTEManager Component
```
QTE Manager (Script)
├── QTE Settings
│   ├── Qte Window Duration: 0.5
│   └── Qte Input Action: CombatInputActions/Combat/QTE  ← ASSIGN THIS
└── Timing
    └── Perfect Timing Window: 0.1
```

### ParrySystem Component
```
Parry System (Script)
├── Parry Settings
│   ├── Parry Input Action: CombatInputActions/Combat/Parry  ← ASSIGN THIS
│   ├── Parry Window Duration: 0.3
│   └── Stamina Reward On Successful Parry: 30
└── Visual Feedback
    └── Perfect Parry Window: 0.1
```

---

## ✅ Testing

### In Editor (Play Mode)

1. **Test QTE:**
   - Start battle
   - When player attacks, QTE window should open
   - Press `Space` or gamepad `A/Cross`
   - Console should show "QTE Success!" or "PERFECT QTE!"

2. **Test Parry:**
   - Let enemy attack
   - When parry window opens (console shows "PARRY WINDOW!")
   - Press `Space` or gamepad `A/Cross`
   - Console should show "Perfect Parry!" or "Successful Parry!"

3. **Check Input Actions:**
   - Window → Analysis → Input Debugger
   - Play Mode → Combat map should be enabled
   - Press buttons to see action triggers

---

## 🎮 Multiple Input Bindings

You can add multiple bindings for the same action:

### Example: Add Controller Triggers for Parry

1. Open `CombatInputActions`
2. Select `Parry` action
3. Click `+` to add binding
4. Choose: `Gamepad > Right Trigger`
5. Save Asset

Now players can use:
- **Keyboard:** Space
- **Controller:** A/Cross button OR Right Trigger

### Example: Add Keyboard Alternative

1. Select `QTE` action
2. Add binding: `E` key
3. Save Asset

Now players can use Space OR E key for QTE.

---

## 🔧 Advanced Configuration

### Timing Adjustments

If QTE/Parry feels too strict or loose:

**Make it Easier:**
- Increase `Qte Window Duration` (e.g., 0.7)
- Increase `Parry Window Duration` (e.g., 0.5)
- Increase `Perfect Timing Window` (e.g., 0.15)

**Make it Harder:**
- Decrease `Qte Window Duration` (e.g., 0.3)
- Decrease `Parry Window Duration` (e.g., 0.2)
- Decrease `Perfect Timing Window` (e.g., 0.05)

### Control Scheme Detection

Add this to your BattleManagerV2 if you want to detect control scheme:

```csharp
using UnityEngine.InputSystem;

private void Update()
{
    if (Gamepad.current != null)
    {
        // Player is using gamepad
        Debug.Log("Using Gamepad");
    }
    else if (Keyboard.current != null)
    {
        // Player is using keyboard
        Debug.Log("Using Keyboard");
    }
}
```

---

## 🚨 Troubleshooting

### Input Not Working

**Problem:** Pressing button does nothing

**Solutions:**
1. Check Input Action is assigned in Inspector
2. Make sure Input Actions asset has been saved
3. Verify action is enabled:
   - Open Input Debugger (Window → Analysis → Input Debugger)
   - Check if Combat map shows "Enabled"
4. Check console for errors

### "Input System Package Not Installed"

**Solution:**
1. Window → Package Manager
2. Search for "Input System"
3. Click Install
4. Unity will ask to restart - click Yes

### Actions Not Showing in Inspector

**Problem:** Dropdown is empty when selecting Input Action

**Solutions:**
1. Make sure Input Actions asset is saved
2. Reimport the asset:
   - Right-click `CombatInputActions`
   - Reimport
3. Restart Unity

### Gamepad Not Responding

**Solution:**
1. Connect gamepad before starting Play Mode
2. Check if gamepad is detected:
   - Window → Analysis → Input Debugger
   - Should see your gamepad listed
3. Add gamepad bindings if missing (see Multiple Input Bindings section)

---

## 📚 Related Files

- **QTEManager:** `Assets/Scripts/Batalla/Systems/QTEManager.cs`
- **ParrySystem:** `Assets/Scripts/Batalla/Systems/ParrySystem.cs`
- **Input Actions:** `Assets/Input/CombatInputActions.inputactions` (you create this)

---

## 🎓 Additional Resources

### Unity Input System Documentation
- [Input System Overview](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)
- [Input Actions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/Actions.html)
- [Input Action Reference](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/ActionAssets.html#using-input-action-references)

### Key Differences from Old System

| Old Input System | New Input System |
|-----------------|------------------|
| `Input.GetKeyDown(KeyCode.Space)` | `action.WasPressedThisFrame()` |
| Hardcoded keys | Rebindable actions |
| Keyboard only by default | Multi-device support |
| No action maps | Organized action maps |
| No event-driven option | Events available |

---

## ✨ Benefits of New System

1. **Controller Support:** Works with Xbox, PlayStation, Switch controllers out of the box
2. **Rebinding:** Players can customize controls
3. **Multiple Bindings:** Same action, multiple keys/buttons
4. **Action Maps:** Organize inputs by context (Combat, Menu, Exploration)
5. **Visual Editor:** Easy to configure without coding
6. **Better Performance:** Event-driven instead of polling (when using callbacks)

---

## 🎮 Recommended Bindings

### Keyboard
- **QTE/Parry:** Space (primary), E (secondary)

### Xbox Controller
- **QTE/Parry:** A button (primary), RT (secondary)

### PlayStation Controller
- **QTE/Parry:** Cross button (primary), R2 (secondary)

### Switch Controller
- **QTE/Parry:** B button (primary), ZR (secondary)

---

## 📝 Next Steps

1. ✅ Create Input Actions asset
2. ✅ Configure QTE and Parry actions
3. ✅ Assign in QTEManager Inspector
4. ✅ Assign in ParrySystem Inspector
5. ✅ Test in Play Mode
6. 🔄 Adjust timings if needed
7. 🔄 Add additional bindings for player comfort
8. 🔄 Create UI prompts showing correct button (see Input Icon Mapper script)

---

Good luck with your combat system! The new Input System provides a much more robust and player-friendly experience. 🎮⚔️
