# Complete Combat System Setup Guide
## Step-by-Step: From Scene Setup to Working Battle with Animations & UI

---

## ⚠️ Prerequisites

Before starting this guide, you **MUST** setup the Input System:
- **📖 See:** `INPUT_SYSTEM_SETUP.md` for creating Input Actions
- This guide assumes you have already created:
  - `CombatInputActions` Input Actions asset
  - `QTE` and `Parry` actions configured

If you haven't done this yet, **STOP** and complete `INPUT_SYSTEM_SETUP.md` first!

---

## 📋 Table of Contents
1. [Scene Setup](#1-scene-setup)
2. [Create Battle Systems](#2-create-battle-systems)
3. [Setup Player](#3-setup-player)
4. [Setup Enemy](#4-setup-enemy)
5. [Create Attack Data](#5-create-attack-data)
6. [Setup Animations](#6-setup-animations)
7. [Setup UI](#7-setup-ui)
8. [Connect Everything](#8-connect-everything)
9. [Test](#9-test)

---

## 1. Scene Setup

### **Step 1.1: Create Battle Scene**

1. Create a new scene: `File → New Scene`
2. Save it as: `BattleScene`

### **Step 1.2: Create GameObjects Hierarchy**

Create this hierarchy in your scene:

```
BattleScene
├── BattleManager (empty GameObject)
├── Player (your player character)
├── Enemy (your enemy character)
├── BattleSystems (empty GameObject - parent for organization)
│   ├── AnimationSequencer
│   ├── QTEManager
│   └── ParrySystem
├── BattleCamera (your camera)
└── BattleUI (Canvas)
    ├── HealthDisplay (Panel)
    ├── ActionButtons (Panel)
    └── TurnDisplay (Panel)
```

**To create each:**
- Right-click in Hierarchy → Create Empty
- Name them as shown above

---

## 2. Create Battle Systems

### **Step 2.1: AnimationSequencer**

1. Select **AnimationSequencer** GameObject
2. **Add Component** → Search for **"Animation Sequencer"**
3. In Inspector, leave the **Animator** field empty for now (will be set via code)

### **Step 2.2: QTEManager**

1. Select **QTEManager** GameObject
2. **Add Component** → Search for **"QTE Manager"**
3. Configure in Inspector:
   ```
   QTE Settings:
   • QTE Window Duration: 0.5
   • QTE Input Action: [Assign CombatInputActions/Combat/QTE]
   
   Timing:
   • Perfect Timing Window: 0.1
   ```

> **📌 Note:** You need to create Input Actions first! See **INPUT_SYSTEM_SETUP.md** for detailed instructions.

### **Step 2.3: ParrySystem**

1. Select **ParrySystem** GameObject
2. **Add Component** → Search for **"Parry System"**
3. Configure in Inspector:
   ```
   Parry Settings:
   • Parry Input Action: [Assign CombatInputActions/Combat/Parry]
   • Parry Window Duration: 0.3
   • Stamina Reward On Successful Parry: 30
   
   Visual Feedback:
   • Perfect Parry Window: 0.1
   ```

> **📌 Note:** The Input Action Reference should point to the same action as QTE by default (Space/A button).

### **Step 2.4: BattleManager**

1. Select **BattleManager** GameObject
2. **Add Component** → Search for **"Battle Manager V2"**
3. Leave fields empty for now (we'll assign them later)

---

## 3. Setup Player

### **Step 3.1: Add Required Components**

Select your **Player** GameObject and add:

1. **Animator** component (if not already present)
   - Create or assign your Player Animator Controller

2. **BattleCharacter** component
   - Configure in Inspector:
     ```
     Stats:
     • Max Health: 100
     • Max Stamina: 100
     
     Components:
     • Animator: [Drag Player's Animator here or leave for auto-detect]
     
     Character Type:
     • Is Player: ✅ (checked)
     ```

3. **PlayerBattleController** component
   - Leave fields empty for now (we'll configure after creating data)

### **Step 3.2: Setup Player Animator**

1. Open your **Player Animator Controller** (Window → Animation → Animator)
2. Create these animation states:
   - **"Idle"** (default state)
   - **"LightAttack"** (your light attack animation)
   - **"Parry"** (parry animation)
   - **"ParryPerfect"** (perfect parry animation - optional)
   - Any skill animations (e.g., "PowerStrike")

3. **Create Transitions:**
   - From **Any State** → **LightAttack**
     - Conditions: None
     - Has Exit Time: ✅ true
     - Exit Time: 1.0
     - Transition Duration: 0.0
   
   - From **Any State** → **Parry**
     - Same settings as above
   
   - All states should return to **Idle** automatically when complete

---

## 4. Setup Enemy

### **Step 4.1: Add Required Components**

Select your **Enemy** GameObject and add:

1. **Animator** component (if not already present)
   - Create or assign your Enemy Animator Controller

2. **BattleCharacter** component
   - Configure in Inspector:
     ```
     Stats:
     • Max Health: 80
     • Max Stamina: 50
     
     Components:
     • Animator: [Drag Enemy's Animator here or leave for auto-detect]
     
     Character Type:
     • Is Player: ❌ (unchecked)
     ```

3. **EnemyBattleController** component
   - Configure in Inspector:
     ```
     Attack Settings:
     • Attack Animation Name: "EnemyAttack"
     • Attack Damage: 15
     • Attack Duration: 2.0
     
     Attack Timing:
     • Parry Window Start Time: 1.5
     • Damage Application Time: 1.7
     
     AI Settings:
     • Thinking Duration: 1.0
     ```

### **Step 4.2: Setup Enemy Animator**

1. Open your **Enemy Animator Controller**
2. Create these animation states:
   - **"Idle"** (default state)
   - **"EnemyAttack"** (enemy attack animation)
   - **"Staggered"** (recoil/stagger animation when parried)

3. **Create Transitions:**
   - From **Any State** → **EnemyAttack**
     - Has Exit Time: ✅ true
     - Exit Time: 1.0
     - Transition Duration: 0.0
   
   - From **Any State** → **Staggered**
     - Same settings

---

## 5. Create Attack Data

### **Step 5.1: Create AttackAnimationData ScriptableObject**

1. In Project window, navigate to: `Assets/Data/Battle/` (create folders if needed)
2. Right-click → **Create → Battle → Attack Animation Data**
3. Name it: **"LightAttack_Data"**

### **Step 5.2: Configure Attack Data**

Select **LightAttack_Data** and configure:

```
ANIMATION:
• Animation State Name: "LightAttack"
• Animation Duration: 1.5 (adjust to your animation length)

QTE TIMINGS (Normalized 0-1):
• QTE Normalized Timings:
  [0] 0.4
  [1] 0.6
  [2] 0.85

DAMAGE:
• Base Damage: 15
• QTE Success Multiplier: 1.5

HIT TIMING:
• Hit Normalized Time: 0.75

STAMINA:
• Stamina Cost: 20
```

**How to calculate values:**
- **Animation Duration:** The length of your animation in seconds
- **QTE Timings:** When to show QTE prompts (0.0 = start, 1.0 = end)
- **Hit Normalized Time:** When weapon connects (e.g., if hit is at 1.125s in a 1.5s animation: 1.125/1.5 = 0.75)

### **Step 5.3: Create SkillData (Optional)**

1. Right-click → **Create → Battle → Skill Data**
2. Name it: **"PowerStrike_Skill"**
3. Configure:

```
BASIC INFO:
• Skill Name: "Power Strike"
• Description: "A powerful attack"

ANIMATION:
• Animation State Name: "PowerStrike"

STATS:
• Stamina Cost: 40
• Damage Amount: 30

EFFECTS:
• Heals Player: ❌
• Heal Amount: 0
```

---

## 6. Setup Animations

### **Step 6.1: Assign Animation Clips**

For **each animation state** in your Animator Controllers:
1. Select the state
2. In Inspector → **Motion** field
3. Drag your animation clip

Example:
- **LightAttack** state → assign your "player_light_attack.anim" clip
- **EnemyAttack** state → assign your "enemy_slash.anim" clip
- **Parry** state → assign your "player_parry.anim" clip

### **Step 6.2: Verify Animation Settings**

For each animation clip:
1. Select the clip in Project window
2. In Inspector, check:
   - **Loop Time:** ❌ (should be unchecked for attacks)
   - Note the **length** (you'll need this for timing calculations)

---

## 7. Setup UI

### **Step 7.1: Create Battle UI Canvas**

1. Right-click in Hierarchy → **UI → Canvas**
2. Name it: **"BattleUI"**
3. Set **Canvas Scaler** settings:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080

### **Step 7.2: Create Health Display Panel**

1. Right-click **BattleUI** → **UI → Panel**
2. Name it: **"HealthDisplay"**
3. Position at top of screen
4. Add Text elements (Right-click HealthDisplay → **UI → Text - TextMeshPro**):
   - **"PlayerHealthText"** → Position top-left
   - **"PlayerStaminaText"** → Position below player health
   - **"EnemyHealthText"** → Position top-right

Configure each text:
- Font Size: 24
- Color: White
- Alignment: Left (for player) / Right (for enemy)

### **Step 7.3: Create Action Buttons Panel**

1. Right-click **BattleUI** → **UI → Panel**
2. Name it: **"ActionSelectionUI"**
3. Position at bottom-center of screen
4. Add Buttons (Right-click ActionSelectionUI → **UI → Button - TextMeshPro**):
   - **"AttackButton"** → Text: "Attack"
   - **"SkillButton"** → Text: "Skill"
   - **"EndTurnButton"** → Text: "End Turn"

Layout buttons horizontally (use Horizontal Layout Group or position manually)

### **Step 7.4: Create Turn Display**

1. Right-click **BattleUI** → **UI → Text - TextMeshPro**
2. Name it: **"TurnDisplayText"**
3. Position at center-top of screen
4. Configure:
   - Font Size: 48
   - Color: Yellow
   - Alignment: Center
   - Text: "PLAYER TURN"

### **Step 7.5: Setup Button Events**

Select each button and configure the **OnClick()** event:

**AttackButton:**
1. Click **+** to add event
2. Drag **BattleManager** GameObject to object field
3. Select function: **BattleManagerV2 → PlayerChooseAttack()**

**SkillButton:**
1. Click **+** to add event
2. Drag **BattleManager** GameObject to object field
3. Select function: **BattleManagerV2 → PlayerChooseSkill(int)**
4. Set int parameter to: **0** (first skill)

**EndTurnButton:**
1. Click **+** to add event
2. Drag **BattleManager** GameObject to object field
3. Select function: **BattleManagerV2 → PlayerEndTurn()**

---

## 8. Connect Everything

### **Step 8.1: Configure PlayerBattleController**

Select your **Player** GameObject:
1. Find **PlayerBattleController** component
2. Assign fields:

```
References:
• Player Character: [Drag Player's BattleCharacter here]

Attack Data:
• Light Attack Data: [Drag LightAttack_Data ScriptableObject here]

Skills:
• Available Skills:
  Size: 1
  Element 0: [Drag PowerStrike_Skill here]

Parry Animations:
• Parry Animation Name: "Parry"
• Perfect Parry Animation Name: "ParryPerfect"
```

### **Step 8.2: Configure BattleManager**

Select **BattleManager** GameObject:
1. Find **BattleManagerV2** component
2. Assign ALL fields:

```
BATTLE PARTICIPANTS:
• Player Controller: [Drag Player GameObject here]
• Enemy Controller: [Drag Enemy GameObject here]

SYSTEMS:
• Player Animation Sequencer: [Drag AnimationSequencer GameObject here]
• QTE Manager: [Drag QTEManager GameObject here]
• Parry System: [Drag ParrySystem GameObject here]

PLAYER MOVEMENT:
• Player Movement: [Drag your MovimientoV2 component here, or leave empty]

UI (OPTIONAL):
• Action Selection UI: [Drag ActionSelectionUI Panel here]

CAMERA:
• Battle Camera: [Drag your camera component here, or leave empty]

PARRY SYSTEM:
• Parry Indicator Prefab: [Create/assign a prefab for parry indicator]
• Parry Indicator Height: 1.5
• Parry Indicator Scale: 0.8

UI HEALTH DISPLAY:
• Player Health Text: [Drag PlayerHealthText here]
• Enemy Health Text: [Drag EnemyHealthText here]
• Player Stamina Text: [Drag PlayerStaminaText here]

UI TURN DISPLAY:
• Turn Display Text: [Drag TurnDisplayText here]
```

### **Step 8.3: Create Parry Indicator Prefab (Optional)**

1. Create a 3D object (e.g., Quad or Sprite)
2. Add a visual (e.g., star sprite, exclamation mark)
3. Drag to Project window to create prefab
4. Assign to BattleManager's **Parry Indicator Prefab** field

Or skip this for now and the parry window will work without visual indicator.

---

## 9. Test

### **Step 9.1: Start Battle Manually**

Since `BattleManagerV2` doesn't auto-start, you need a trigger. Options:

**Option A: Create Battle Starter**

1. Create script: **`BattleStarter.cs`**
```csharp
using UnityEngine;

public class BattleStarter : MonoBehaviour
{
    public BattleManagerV2 battleManager;

    void Start()
    {
        if (battleManager != null)
        {
            // Start battle after 1 second
            Invoke("StartBattle", 1f);
        }
    }

    void StartBattle()
    {
        battleManager.StartBattle();
    }
}
```

2. Add to an empty GameObject in scene
3. Assign BattleManager reference

**Option B: Use Trigger Zone**

Use your existing `BattleStarter.cs` with trigger collider.

### **Step 9.2: Enter Play Mode**

Press **Play** and check:

**Initial State:**
- [ ] Battle starts automatically or on trigger
- [ ] Console shows: "=== BATTLE START ==="
- [ ] Console shows: "=== PLAYER TURN ==="
- [ ] UI shows player/enemy health
- [ ] UI shows "PLAYER TURN"
- [ ] Action buttons appear

### **Step 9.3: Test Player Attack**

Click **Attack Button** and check:
- [ ] Player attack animation plays
- [ ] Console: "Playing attack animation through AnimationSequencer"
- [ ] QTE windows appear during animation
- [ ] Damage applies to enemy at correct time
- [ ] Enemy health decreases
- [ ] Console: "Player deals X damage to Enemy"

### **Step 9.4: Test Enemy Turn**

Click **End Turn** button and check:
- [ ] UI shows "ENEMY TURN"
- [ ] Enemy attack animation plays
- [ ] Console: "Playing enemy attack animation: EnemyAttack"
- [ ] Parry indicator appears (if prefab assigned)
- [ ] Console: "Parry window opened!"
- [ ] If you press Space: "Parry successful!"
- [ ] If you miss: Damage applies to player

### **Step 9.5: Test Parry**

During enemy turn, when parry window opens:
- Press **Space** key
- [ ] Player parry animation plays
- [ ] Enemy stagger animation plays
- [ ] Console: "Playing player parry animation: Parry"
- [ ] Console: "Parry successful!"
- [ ] Player takes no damage
- [ ] Player gains stamina

---

## 🐛 Troubleshooting

### **"AnimationSequencer is null" Warning**
✅ **Fix:** Assign AnimationSequencer GameObject in BattleManager Inspector

### **"No AnimationSequencer or attack data"**
✅ **Fix:** 
1. Assign AttackAnimationData to PlayerBattleController
2. Make sure AnimationSequencer is assigned in BattleManager

### **Animations don't play**
✅ **Check:**
1. Animation state names match exactly (case-sensitive!)
2. Animation clips assigned to states
3. Animator component assigned to BattleCharacter
4. Transitions have "Has Exit Time" enabled

### **QTE windows don't appear**
✅ **Check:**
1. QTEManager is assigned in BattleManager
2. AttackAnimationData has values in `qteNormalizedTimings` array
3. Console for: "QTE Window triggered!"

### **Buttons don't work**
✅ **Check:**
1. Button OnClick events are set up
2. BattleManager GameObject is assigned to button events
3. Correct functions are selected in dropdown
4. Canvas has EventSystem (auto-created with Canvas)

### **Health/Stamina UI doesn't update**
✅ **Check:**
1. Text components are assigned in BattleManager Inspector
2. Check Console for any null reference errors

### **Battle doesn't start**
✅ **Check:**
1. Call `battleManager.StartBattle()` from code or trigger
2. Player and Enemy controllers are assigned
3. Enemy GameObject is active in scene

---

## ✅ Complete Setup Checklist

### **Scene Setup:**
- [ ] BattleManager GameObject created with BattleManagerV2 component
- [ ] Player GameObject with Animator, BattleCharacter, PlayerBattleController
- [ ] Enemy GameObject with Animator, BattleCharacter, EnemyBattleController
- [ ] AnimationSequencer GameObject with component
- [ ] QTEManager GameObject with component
- [ ] ParrySystem GameObject with component
- [ ] BattleUI Canvas with all panels and texts

### **Data Assets:**
- [ ] AttackAnimationData ScriptableObject created
- [ ] SkillData ScriptableObject created (optional)
- [ ] All data configured with correct values

### **Animations:**
- [ ] Player Animator Controller with states: Idle, LightAttack, Parry
- [ ] Enemy Animator Controller with states: Idle, EnemyAttack, Staggered
- [ ] Animation clips assigned to states
- [ ] Transitions configured

### **Inspector Assignments:**
- [ ] PlayerBattleController: AttackData, Skills assigned
- [ ] BattleManagerV2: All references assigned (Player, Enemy, Systems, UI)
- [ ] Button OnClick events configured

### **Testing:**
- [ ] Battle starts
- [ ] Player attack works with animations and QTE
- [ ] Enemy attack works with parry system
- [ ] UI updates correctly
- [ ] Turn flow works

---

## 📚 Additional Resources

- **ANIMATION_SETUP_GUIDE.md** - Detailed animation timing guide
- **PARRY_ANIMATION_GUIDE.md** - Parry system setup
- **ANIMATION_SYSTEM_ENABLED.md** - Technical details
- **WHERE_ATTACK_ANIMATIONS_TRIGGER.md** - Code flow explanation

---

## 🎓 Next Steps After Basic Setup

1. **Fine-tune animation timings** based on feel
2. **Add more attacks** with different data
3. **Create multiple skills** for variety
4. **Add visual effects** at hit moments
5. **Add sound effects** for attacks/parries
6. **Implement camera shake** on impacts
7. **Add victory/defeat animations**
8. **Create battle transition effects**

---

**You now have everything needed to set up a fully functional turn-based combat system with animations, QTE, and parry mechanics!** 🎮✨
