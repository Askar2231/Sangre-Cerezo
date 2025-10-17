# 🎮 **Combat Input Mappings - Reference Guide**

## 📋 **Current Button Mappings**

All combat systems now use these standardized mappings:

---

### **🖱️ Keyboard & Mouse**

| Action           | Button            | Text Fallback      |
| ---------------- | ----------------- | ------------------ |
| **Light Attack** | Mouse Left Click  | `[Clic Izquierdo]` |
| **Heavy Attack** | Mouse Right Click | `[Clic Derecho]`   |
| **Skill 1**      | Q Key             | `[Q]`              |
| **Skill 2**      | R Key             | `[R]`              |
| **Move**         | WASD              | `[WASD]`           |
| **Run**          | Shift             | `[Shift]`          |
| **Interact**     | E Key             | `[E]`              |
| **QTE/Parry**    | Space             | `[Espacio]`        |
| **Confirm**      | Enter             | `[Enter]`          |
| **Cancel**       | ESC               | `[ESC]`            |
| **End Turn**     | Tab               | `[Tab]`            |

---

### **🎮 Xbox Controller**

| Action           | Button              | Text Fallback  |
| ---------------- | ------------------- | -------------- |
| **Light Attack** | X Button (Square)   | `[X]`          |
| **Heavy Attack** | Y Button (Triangle) | `[Y]`          |
| **Skill 1**      | LT (Left Trigger)   | `[LT]`         |
| **Skill 2**      | RT (Right Trigger)  | `[RT]`         |
| **Move**         | Left Stick          | `[Stick Izq.]` |
| **Run**          | LT                  | `[LT]`         |
| **Interact**     | A Button            | `[A]`          |
| **QTE/Parry**    | A Button            | `[A]`          |
| **Confirm**      | A Button            | `[A]`          |
| **Cancel**       | B Button            | `[B]`          |
| **End Turn**     | RB (Right Bumper)   | `[RB]`         |

---

### **🎮 PlayStation Controller**

| Action           | Button       | Text Fallback  |
| ---------------- | ------------ | -------------- |
| **Light Attack** | Square (□)   | `[Cuadrado]`   |
| **Heavy Attack** | Triangle (△) | `[Triángulo]`  |
| **Skill 1**      | L2           | `[L2]`         |
| **Skill 2**      | R2           | `[R2]`         |
| **Move**         | Left Stick   | `[Stick Izq.]` |
| **Run**          | L2           | `[L2]`         |
| **Interact**     | Cross (✕)    | `[Cruz]`       |
| **QTE/Parry**    | Cross (✕)    | `[Cruz]`       |
| **Confirm**      | Cross (✕)    | `[Cruz]`       |
| **Cancel**       | Circle (○)   | `[Círculo]`    |
| **End Turn**     | R1           | `[R1]`         |

---

## 🔧 **Implementation Files**

These mappings are implemented in:

1. **`InputIconMapper.cs`** - Master mapping system

   - `GetKeyboardText()` - Lines 291-305
   - `GetXboxText()` - Lines 311-327
   - `GetPlayStationText()` - Lines 331-347

2. **`InputActionHelper.cs`** - Helper for InputActionReference

   - `MapToInputAction()` - Lines 85-125
   - Maps action names to enum values

3. **`DynamicBattleUI.cs`** - UI button generation

   - Uses InputActionHelper to get sprites
   - Displays device-specific icons

4. **`BattleInputHandler.cs`** - Direct input processing
   - Reads from BattleInputActions.inputactions
   - Maps physical inputs to actions

---

## 📖 **How to Use**

### **For Developers:**

```csharp
// Get the sprite/text for an action
string display = InputIconMapper.Instance.ProcessTextPlaceholders("{LightAttack}");
// Returns: Mouse sprite or "[Clic Izquierdo]"

// In tutorial text
"Press {LightAttack} to attack"
// Becomes: "Press [Mouse Left Icon] to attack"
```

### **For Content Creators:**

Use these placeholders in tutorial text:

- `{LightAttack}` - Shows light attack button
- `{HeavyAttack}` - Shows heavy attack button
- `{Skill1}` - Shows skill 1 button
- `{Skill2}` - Shows skill 2 button
- `{Confirm}` - Shows confirm button
- `{Cancel}` - Shows cancel button
- `{EndTurn}` - Shows end turn button

---

## ⚙️ **Input Actions Configuration**

Your `BattleInputActions.inputactions` should have:

```
Action Map: Battle
├── LightAttack
│   ├── Binding: <Mouse>/leftButton
│   └── Binding: <Gamepad>/buttonWest (X)
│
├── HeavyAttack
│   ├── Binding: <Mouse>/rightButton
│   └── Binding: <Gamepad>/buttonNorth (Y)
│
├── Skill1
│   ├── Binding: <Keyboard>/q
│   └── Binding: <Gamepad>/leftTrigger
│
├── Skill2
│   ├── Binding: <Keyboard>/r
│   └── Binding: <Gamepad>/rightTrigger
│
├── EndTurn
│   ├── Binding: <Keyboard>/tab
│   └── Binding: <Gamepad>/rightShoulder (RB)
│
└── Cancel
    ├── Binding: <Keyboard>/escape
    └── Binding: <Gamepad>/buttonEast (B)
```

---

## 🎯 **Design Rationale**

### **Why These Mappings?**

**Keyboard:**

- **Mouse Buttons** for attacks = Natural for JRPG combat
- **Q & R** for skills = Easy to reach, common in RPGs
- **Space** for QTE/Parry = Large, easy to hit quickly

**Xbox:**

- **X & Y** for attacks = Face buttons, accessible
- **LT & RT** for skills = Analog triggers, feel powerful
- **RB** for End Turn = Right shoulder, deliberate action

**PlayStation:**

- **Square & Triangle** for attacks = Standard combat buttons
- **L2 & R2** for skills = Trigger buttons, powerful feel
- **R1** for End Turn = Quick access

---

## 🔄 **Consistency Check**

✅ **All systems use the same mappings:**

- InputIconMapper text fallbacks match sprite indices
- BattleInputHandler reads correct actions
- DynamicBattleUI displays correct icons
- Tutorial system shows correct sprites

✅ **Device detection works:**

- Keyboard/mouse → Keyboard mappings
- Xbox controller → Xbox mappings
- PlayStation controller → PlayStation mappings
- Generic gamepad → Xbox mappings (fallback)

---

## 🛠️ **Changing Mappings**

If you want to change a mapping:

1. **Update InputIconMapper.cs:**

   - Modify `GetKeyboardText()` for keyboard text
   - Modify `GetXboxText()` for Xbox text
   - Modify `GetPlayStationText()` for PlayStation text

2. **Update Input Actions asset:**

   - Open `BattleInputActions.inputactions`
   - Change the physical key/button binding
   - Save and regenerate C# class

3. **Update sprites (if needed):**

   - Assign new sprites in InputIconMapper Inspector
   - Regenerate TMP Sprite Asset using custom editor

4. **Update this document** to reflect changes

---

## 📊 **Current Status**

✅ All combat input mappings are **standardized**
✅ All systems use **InputIconMapper** as source of truth
✅ **Device-specific** sprites/text working
✅ **Tutorial system** uses same mappings
✅ **Battle UI** uses same mappings
✅ **Direct input** uses same mappings

**No changes needed** - everything is already consistent! 🎉

---

## 🎮 **Player Experience**

When players see:

- **Keyboard:** They see mouse/keyboard icons naturally
- **Xbox Controller:** They see familiar Xbox button colors
- **PlayStation Controller:** They see familiar PlayStation symbols
- **Text Fallback:** If no sprites, they see clear text labels

**Result:** Players always know which button to press! ✨
