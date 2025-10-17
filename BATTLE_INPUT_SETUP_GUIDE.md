# Battle Input System - Setup Guide

## 🎯 Quick Start

This guide shows you how to set up the new **BattleInputManager** system for centralized input handling in your battle scenes.

---

## 📦 Prerequisites

✅ Unity's Input System package installed
✅ `InputSystem_Actions.inputactions` file exists with Battle action map
✅ BattleManagerV2 scene set up

---

## 🛠️ Step-by-Step Setup

### **1. Add BattleInputManager to Scene**

1. In your Battle scene hierarchy, create an empty GameObject
2. Name it: `BattleInputManager`
3. Add Component → `BattleInputManager` script

### **2. Assign InputActionReferences**

In the Inspector, expand **BattleInputManager** component:

#### **Player Turn Actions:**

- **Light Attack Input:** Select `InputSystem_Actions → Battle → LightAttack`
- **Heavy Attack Input:** Select `InputSystem_Actions → Battle → HeavyAttack`
- **Skill 1 Input:** Select `InputSystem_Actions → Battle → Skill1`
- **Skill 2 Input:** Select `InputSystem_Actions → Battle → Skill2`
- **End Turn Input:** Select `InputSystem_Actions → Battle → EndTurn`

#### **Timing Actions:**

- **Parry Input:** Select `InputSystem_Actions → Battle → Parry`
- **QTE Input:** Select `InputSystem_Actions → Battle → QTE`

### **3. Configure Settings**

#### **Settings:**

- ✅ **Debug Mode:** `true` (recommended for testing)
- ☐ **Allow Input During Animations:** `false` (default)
- ✅ **Show Debug HUD:** `true` (optional, shows on-screen state)

#### **Debug UI (Optional):**

- **Debug Text:** Drag a TextMeshProUGUI object here to show live input state

#### **System References (Optional - for direct routing):**

- **Parry System:** Drag your ParrySystem object (auto-routes parry inputs)
- **QTE Manager:** Drag your QTEManager object (auto-routes QTE inputs)

### **4. Connect to BattleManagerV2**

1. Select your `BattleManagerV2` GameObject
2. In the Inspector, find the **Systems** section
3. **Input Manager:** Drag your `BattleInputManager` GameObject here

### **5. Connect to BattleUIButtonController**

1. Select your UI Canvas with battle buttons
2. Find the `BattleUIButtonController` component
3. **System References** section:
   - **Input Manager:** Drag your `BattleInputManager` GameObject here

---

## 🎮 Input Configuration

### **Default Input Bindings**

The system expects these actions in your `InputSystem_Actions.inputactions` file under the **Battle** action map:

| Action          | Keyboard | Gamepad                      |
| --------------- | -------- | ---------------------------- |
| **LightAttack** | J        | X (PlayStation) / A (Xbox)   |
| **HeavyAttack** | K        | △ (PlayStation) / Y (Xbox)   |
| **Skill1**      | U        | L1 (PlayStation) / LB (Xbox) |
| **Skill2**      | I        | R1 (PlayStation) / RB (Xbox) |
| **Parry**       | Space    | ○ (PlayStation) / B (Xbox)   |
| **QTE**         | E        | × (PlayStation) / A (Xbox)   |
| **EndTurn**     | Enter    | Start                        |

### **How to Change Bindings**

1. Open `Assets/InputSystem_Actions.inputactions`
2. Select **Battle** action map
3. Click on an action (e.g., "LightAttack")
4. Modify bindings in the Inspector
5. **No code changes needed!** BattleInputManager automatically uses new bindings

---

## 🔍 Verification

### **Inspector Checklist**

✅ All InputActionReferences assigned (no "None" entries)
✅ BattleManagerV2 has BattleInputManager reference
✅ BattleUIButtonController has BattleInputManager reference
✅ Debug Mode enabled for testing

### **Runtime Test**

1. Start battle
2. Open Console (should see): `[BattleInput] State: Disabled → PlayerTurn`
3. Press keyboard key for light attack
4. Console should show: `[BattleInput] ✅ Light Attack | State: PlayerTurn | Result: ALLOWED`

---

## 🐛 Debugging

### **Debug Logs**

When `Debug Mode = true`, you'll see detailed logs:

```
[BattleInput] State: Disabled → PlayerTurn
[BattleInput] ✅ Light Attack | State: PlayerTurn | Result: ALLOWED
[BattleInput] State: PlayerTurn → ExecutingAction
[BattleInput] ❌ Heavy Attack | State: ExecutingAction | Result: BLOCKED | Wrong state
[BattleInput] Parry Window: OPEN
[BattleInput] ✅ Parry | State: ParryWindow | Result: ALLOWED
```

### **Debug HUD**

If you assigned a TextMeshProUGUI to **Debug Text** and enabled **Show Debug HUD**, you'll see:

```
BATTLE INPUT DEBUG
━━━━━━━━━━━━━━━━━━━━━━━━━━━
Input State: PlayerTurn
Parry Window: CLOSED ✗
QTE Window: CLOSED ✗
Last Input: Light Attack (0.5s ago)
Device: Gamepad (Xbox Controller)

Allowed Actions:
✅ Light/Heavy Attack
✅ Skills
❌ Parry
❌ QTE
```

---

## ❓ Troubleshooting

### **Problem: Input not working**

**Solutions:**

1. Check InputActionReferences are assigned
2. Verify `InputSystem_Actions.inputactions` has Battle action map
3. Check Console for `Input Action 'XYZ' not assigned` warnings
4. Ensure BattleManagerV2 is calling `inputManager.SetInputState(PlayerTurn)`

### **Problem: Parry/QTE not triggering**

**Solutions:**

1. Check ParrySystem/QTEManager references in BattleInputManager
2. Verify parry window is opening (check console logs)
3. Ensure input state changes to `ParryWindow` or `QTEWindow`
4. Check parry/QTE InputActionReferences are assigned

### **Problem: UI buttons not working**

**Solutions:**

1. Check BattleUIButtonController has BattleInputManager reference
2. Verify button onClick events are connected (should be automatic in OnEnable)
3. Check `isInputEnabled` is true during player turn
4. Look for errors in Console

### **Problem: Input triggers twice (keyboard + UI button)**

**Solution:** This should NOT happen - BattleInputManager handles deduplication. If it does:

1. Check you haven't subscribed to old `BattleUIButtonController.OnLightAttackPressed` events
2. Ensure old BattleInputHandler is disabled
3. Check Console for duplicate log messages

---

## 🔄 State Machine Reference

### **Input States**

| State               | Description             | Allowed Input                  |
| ------------------- | ----------------------- | ------------------------------ |
| **Disabled**        | No input allowed        | None                           |
| **PlayerTurn**      | Player selecting action | Attacks, Skills, End Turn      |
| **ExecutingAction** | Animation playing       | None (optional: allow queuing) |
| **ParryWindow**     | Enemy attack incoming   | Parry only                     |
| **QTEWindow**       | QTE timing active       | QTE only                       |

### **State Transitions**

```
Battle Start
    ↓
Disabled
    ↓
Player Turn → PlayerTurn (attacks/skills allowed)
    ↓
Executing Attack → ExecutingAction (input blocked)
    ↓
Enemy Turn → Disabled
    ↓
Parry Window Opens → ParryWindow (only parry)
    ↓
Enemy Attack Ends → Disabled
    ↓
Back to Player Turn → PlayerTurn
```

---

## 🎨 Advanced Configuration

### **Enable Input Queuing**

Allow players to queue next action during animation:

1. Select BattleInputManager
2. Enable: **Allow Input During Animations**
3. Actions will be queued when state is `ExecutingAction`

### **Custom Debug HUD Position**

1. Create UI Text (TextMeshPro) in your Canvas
2. Position it (e.g., top-left corner)
3. Assign to BattleInputManager → **Debug Text**
4. Enable **Show Debug HUD**

### **Remove Legacy Events**

Once system is tested and working, you can clean up:

1. In `BattleManagerV2.Start()`: Remove old UI event subscriptions
2. In `BattleUIButtonController.cs`: Remove `[Obsolete]` events
3. Remove backward compatibility `#pragma warning disable` sections

---

## 📝 Summary

**Setup Essentials:**

1. ✅ Add BattleInputManager to scene
2. ✅ Assign all InputActionReferences
3. ✅ Connect to BattleManagerV2
4. ✅ Connect to BattleUIButtonController
5. ✅ Enable Debug Mode for testing

**Input Flow:**

```
Player Input (Keyboard/Gamepad/UI Button)
    ↓
BattleInputManager (validates state)
    ↓
Fires Event (OnLightAttackRequested, etc.)
    ↓
BattleManagerV2 (executes action)
    ↓
PlayerBattleController (plays animation)
```

**Benefits:**

- ✅ Single place to manage all input
- ✅ All bindings configurable in Inspector
- ✅ State-based validation prevents illegal inputs
- ✅ Works with keyboard, gamepad, AND UI buttons
- ✅ Easy to debug with detailed logging

---

## 🚀 You're Ready!

Test your setup:

1. Start battle
2. Try keyboard input
3. Try UI button clicks
4. Verify both work the same
5. Check debug logs

**Need help?** Check the troubleshooting section or review console logs with Debug Mode enabled.
