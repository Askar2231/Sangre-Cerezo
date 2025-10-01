# Battle System V2 - Setup Guide

## Overview
This battle system implements turn-based combat with real-time action execution, similar to Claire Obscur: Expedition 33. The system uses **normalized time tracking** for animation events, making it independent of animation lengths and speeds.

## System Architecture

### Core Components
1. **BattleManagerV2** - Main coordinator
2. **TurnManager** - Handles turn flow
3. **BattleCharacter** - Base character class
4. **PlayerBattleController** - Player-specific logic
5. **EnemyBattleController** - Enemy AI and behavior

### Systems
1. **StaminaManager** - Stamina tracking per character
2. **AnimationSequencer** - Animation control with normalized time
3. **QTEManager** - Quick Time Events
4. **ParrySystem** - Parry mechanics

### Data
1. **AttackAnimationData** - ScriptableObject for attack configurations
2. **SkillData** - ScriptableObject for skill configurations

---

## Unity Setup Instructions

### Step 1: Scene Setup
1. Create a new scene called "BattleScene"
2. Add an empty GameObject called "BattleManager"
3. Attach the `BattleManagerV2` component to it

### Step 2: Player Setup
1. Create a GameObject called "Player"
2. Add `BattleCharacter` component
3. Add `PlayerBattleController` component
4. Assign an Animator component with your attack animations
5. Configure stats (Health, Stamina)

### Step 3: Enemy Setup
1. Create a GameObject called "Enemy"
2. Add `BattleCharacter` component
3. Add `EnemyBattleController` component
4. Assign an Animator component with enemy attack animations
5. Configure stats and attack settings

### Step 4: Systems Setup
1. Create a GameObject called "AnimationSequencer"
   - Add `AnimationSequencer` component
2. Create a GameObject called "QTEManager"
   - Add `QTEManager` component
   - Configure QTE window duration (default: 0.5s)
   - Set QTE input key (default: Space)
3. Create a GameObject called "ParrySystem"
   - Add `ParrySystem` component
   - Configure parry window duration (default: 0.3s)
   - Set stamina reward on successful parry

### Step 5: BattleManagerV2 Configuration
In the `BattleManagerV2` inspector:
- **Battle Participants:**
  - Assign PlayerBattleController
  - Assign EnemyBattleController
- **Systems:**
  - Assign AnimationSequencer
  - Assign QTEManager
  - Assign ParrySystem
- **Player Movement:**
  - Assign MovimientoV2 (player's movement script)
  - This will automatically disable movement during battle
- **UI (Optional):**
  - Assign action selection UI GameObject

---

## Creating Attack Data

### Step 1: Create AttackAnimationData
1. Right-click in Project → Create → Battle → Attack Animation Data
2. Configure the ScriptableObject:

```
Animation State Name: "LightAttack"
QTE Normalized Timings: [0.3, 0.6, 0.9]  // Three QTE windows
Base Damage: 10
QTE Success Multiplier: 1.5
Hit Normalized Time: 0.75  // When damage is applied
Stamina Cost: 20
```

### Step 2: Understanding Normalized Timings
- **0.0** = Start of animation
- **0.5** = Halfway through animation
- **1.0** = End of animation

Example for a 1-second animation:
- QTE at 0.3 = triggers at 0.3 seconds
- QTE at 0.6 = triggers at 0.6 seconds

Example for a 2-second animation:
- QTE at 0.3 = triggers at 0.6 seconds
- QTE at 0.6 = triggers at 1.2 seconds

**This is why normalized time is scalable!**

### Step 3: Assign to PlayerBattleController
In PlayerBattleController inspector:
- Assign the AttackAnimationData to "Light Attack Data"

---

## Creating Skills

### Step 1: Create SkillData
1. Right-click in Project → Create → Battle → Skill Data
2. Configure:

```
Skill Name: "Power Strike"
Description: "A powerful attack"
Animation State Name: "PowerStrike"
Stamina Cost: 30
Damage Amount: 25
Heals Player: false
```

### Step 2: Assign to PlayerBattleController
In PlayerBattleController inspector:
- Add skills to "Available Skills" array

---

## Combat Flow

### Player Turn
1. **Stamina Restored** to max
2. **Action Selection UI** appears
3. Player chooses:
   - **Attack** → Plays animation with QTE windows
   - **Skill** → Plays skill animation and applies effects
   - **End Turn** → Switches to enemy turn

### QTE System
- Window appears at normalized timing points
- Player presses configured key (default: Space)
- Success = damage multiplier applied
- Fail = base damage only

### Enemy Turn
1. **Thinking Phase** (1 second delay)
2. **Attack Animation** plays
3. **Parry Window** opens before hit
4. If parried:
   - No damage to player
   - Player gains stamina
5. If not parried:
   - Full damage to player

### Battle End
- Battle ends when either character's HP reaches 0
- `OnBattleEnded` event fires with result

---

## Public API for UI/Input

### BattleManagerV2 Methods
```csharp
// Player actions
battleManager.PlayerChooseAttack();
battleManager.PlayerChooseSkill(0);  // Index of skill
battleManager.PlayerEndTurn();

// Get stamina info
var (current, max) = battleManager.GetPlayerStamina();

// Events
battleManager.OnBattleStateChanged += HandleStateChange;
battleManager.OnBattleEnded += HandleBattleEnd;
```

### Example UI Button Setup
```csharp
public Button attackButton;
public Button skillButton;
public Button endTurnButton;

void Start()
{
    attackButton.onClick.AddListener(() => battleManager.PlayerChooseAttack());
    skillButton.onClick.AddListener(() => battleManager.PlayerChooseSkill(0));
    endTurnButton.onClick.AddListener(() => battleManager.PlayerEndTurn());
}
```

---

## Animation Setup

### Animator Controller Requirements

#### Player Animator
Must have states:
- "LightAttack" (or whatever you name in AttackAnimationData)
- Skill animation states

#### Enemy Animator
Must have states:
- "EnemyAttack" (configure in EnemyBattleController)

### Important Notes
- **Don't use Animation Events** - the system controls timing via normalized time
- Animations can be any length - timings auto-adjust
- You can change animation speeds without breaking QTE timings

---

## Testing

### Debug Mode
Call `battleManager.DebugStartBattle()` to manually restart battle.

### Console Output
The system logs all state changes and actions:
- `=== BATTLE START ===`
- `=== PLAYER TURN ===`
- `QTE Window triggered!`
- `Attack hit for X damage!`
- etc.

### Testing Checklist
1. ✅ Player can attack with QTEs
2. ✅ QTEs increase damage
3. ✅ Stamina decreases on actions
4. ✅ Can't attack without enough stamina
5. ✅ Turn ends when player chooses
6. ✅ Enemy attacks after player turn
7. ✅ Parry window appears during enemy attack
8. ✅ Successful parry grants stamina
9. ✅ Battle ends when HP reaches 0

---

## Extension Points

### Adding New Attack Types
1. Create new AttackAnimationData
2. Add field in PlayerBattleController
3. Add public method in BattleManagerV2
4. Connect to UI

### Adding Status Effects
Extend `SkillData` with:
- Status effect types
- Duration
- Apply in `SkillAction.ApplySkillEffects()`

### AI Improvements
Modify `EnemyBattleController.ExecuteThinking()`:
- Add decision-making logic
- Multiple attack types
- Pattern-based behavior

### Multi-hit Combos
Modify `AttackAction`:
- Chain multiple AttackAnimationData
- Track combo count
- Apply combo multipliers

---

## Movement Integration

The battle system automatically integrates with `MovimientoV2`:

**Automatic Behavior:**
1. **Battle Starts** → Player movement disabled
2. **Battle Ends** → Player movement re-enabled
3. **Smooth Transition** → Character animations smoothly stop when disabled

**How It Works:**
- `BattleManagerV2` references `MovimientoV2` script
- Calls `SetMovementEnabled(false)` on battle start
- Calls `SetMovementEnabled(true)` on battle end
- Character smoothly decelerates to idle when disabled

**Manual Control (if needed):**
```csharp
// Disable movement
playerMovement.SetMovementEnabled(false);

// Re-enable movement
playerMovement.SetMovementEnabled(true);

// Force instant stop (no smooth deceleration)
playerMovement.ForceStopMovement();
```

---

## Troubleshooting

### QTEs not triggering
- Check `qteNormalizedTimings` are between 0-1
- Verify animation is playing
- Check AnimationSequencer is initialized

### Animations not playing
- Verify Animator is assigned
- Check animation state names match exactly
- Ensure Animator Controller has the states

### Stamina issues
- Check stamina costs in data
- Verify max stamina is set on BattleCharacter
- Check stamina restoration on turn start

### Parry not working
- Verify ParrySystem is assigned
- Check parry window timing in EnemyBattleController
- Ensure input key is correct

---

## Performance Notes

- System uses coroutines for timing (not Update loops where possible)
- Normalized time checking happens in Update for AnimationSequencer
- Events used for decoupling - clean architecture
- No reflection or string comparisons in hot paths

---

## Credits
Battle System V2 - Designed for turn-based combat with real-time action execution.
Inspired by Claire Obscur: Expedition 33.

