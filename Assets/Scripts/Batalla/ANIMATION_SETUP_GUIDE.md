# Combat Animation Setup Guide

## Complete Tutorial for Setting Up Attacks, Skills, and Parry Timings

---

## 📋 Table of Contents

1. [Understanding the System](#understanding-the-system)
2. [Unity Animator Setup](#unity-animator-setup)
3. [Player Attack Animations](#player-attack-animations)
4. [Enemy Attack Animations](#enemy-attack-animations)
5. [Skill Animations](#skill-animations)
6. [Testing Your Animations](#testing-your-animations)
7. [Troubleshooting](#troubleshooting)

---

## 🎯 Understanding the System

Your battle system uses **normalized time tracking** (0.0 to 1.0), which means:

- **0.0** = Animation start
- **0.5** = Halfway through animation
- **1.0** = Animation end

### Why Normalized Time?

✅ Animations can be any length (1 second, 2 seconds, 5 seconds)  
✅ You can change animation speed without breaking QTE timings  
✅ Easy to understand: 0.5 always means "halfway through"  
✅ System automatically scales timings to match your animation

### Key Systems

1. **AnimationSequencer** - Tracks normalized time and triggers QTE windows
2. **QTEManager** - Handles Quick Time Events for player attacks
3. **ParrySystem** - Handles defensive timing windows during enemy attacks

---

## 🎮 Unity Animator Setup

### Step 1: Create Your Animator Controller

1. In Unity, right-click in Project → **Create → Animator Controller**
2. Name it: `PlayerBattleAnimator` (and `EnemyBattleAnimator` for enemies)
3. Open the Animator window (Window → Animation → Animator)

### Step 2: Add Animation States

#### For Player:

Create these animation states:

- **Idle** (default state)
- **LightAttack**
- **HeavyAttack**
- **SkillAttack** (or multiple like "PowerStrike", "FireSlash", etc.)
- **TakeDamage** (optional)
- **Victory** (optional)

#### For Enemy:

Create these animation states:

- **Idle** (default state)
- **EnemyAttack** (or "SlashAttack", "PunchAttack", etc.)
- **TakeDamage** (optional)
- **Death** (optional)

### Step 3: Set Up Transitions

**CRITICAL: Do NOT create transitions FROM Idle to attack states!**

The battle system uses `animator.Play()` which directly forces state changes, bypassing transitions. If you create transitions from Idle, the animator may auto-transition to attacks even outside combat!

**Correct Setup:**

✅ **Create transitions FROM attack states TO Idle:**

- LightAttack → Idle
- HeavyAttack → Idle
- SkillAttack → Idle
- TakeDamage → Idle
- Victory → Idle
- Death → Idle (no transition needed, stays in Death)

❌ **DO NOT create transitions FROM Idle TO attack states**

**Transition Settings (for transitions TO Idle):**

- **Has Exit Time:** ✅ Enabled
- **Exit Time:** 1.0 (animation completes fully)
- **Transition Duration:** 0.1-0.2 seconds (smooth blend)
- **Interruption Source:** None

This ensures animations play fully before returning to Idle, but they never auto-play from Idle.

### Example Animator Structure:

```
┌─────────┐
│  Idle   │ (Default State - No outgoing transitions!)
└─────────┘
     ↑
     │ (Only incoming transitions)
     │
     ├── LightAttack ──┘
     ├── HeavyAttack ──┘
     ├── SkillAttack ──┘
     ├── TakeDamage ──┘
     └── Victory ──────┘

Death (stays in Death state, no transition back)
```

### Why This Works:

- `animator.Play("LightAttack")` in code **forces** the animator to LightAttack state
- Once animation completes (Exit Time = 1.0), it automatically returns to Idle
- Since there are no transitions FROM Idle, it never auto-plays animations
- This prevents animations from playing outside of combat

---

## ⚔️ Player Attack Animations

Player attacks use **AttackAnimationData** ScriptableObjects with QTE windows.

### Step 1: Analyze Your Animation

First, watch your animation in Unity's Animation window and identify key moments:

**Example Light Attack (1.5 seconds long):**

```
0.0s (0%) - Start stance
0.3s (20%) - Wind up begins
0.6s (40%) - Swing starts         ← QTE Window 1
0.9s (60%) - Mid-swing            ← QTE Window 2
1.1s (73%) - Hit connects         ← Damage Applied
1.3s (87%) - Follow through       ← QTE Window 3
1.5s (100%) - Return to stance
```

### Step 2: Create AttackAnimationData

1. Right-click in Project → **Create → Battle → Attack Animation Data**
2. Name it: `LightAttack_Data`
3. Configure in Inspector:

```
╔════════════════════════════════════════╗
║ AttackAnimationData - Light Attack     ║
╠════════════════════════════════════════╣
║ ANIMATION                              ║
║ • Animation State Name: "LightAttack"  ║
║ • Animation Duration: 1.5              ║
║   (For reference only)                 ║
║                                        ║
║ QTE TIMINGS (Normalized 0-1)           ║
║ • QTE Timings:                         ║
║   [0] 0.4   ← 40% through animation    ║
║   [1] 0.6   ← 60% through animation    ║
║   [2] 0.87  ← 87% through animation    ║
║                                        ║
║ DAMAGE                                 ║
║ • Base Damage: 15                      ║
║ • QTE Success Multiplier: 1.5          ║
║   (Damage becomes 15 * 1.5 = 22.5      ║
║    if player hits all QTEs)            ║
║                                        ║
║ HIT TIMING                             ║
║ • Hit Normalized Time: 0.73            ║
║   (When damage is applied to enemy)    ║
║                                        ║
║ STAMINA                                ║
║ • Stamina Cost: 20                     ║
╚════════════════════════════════════════╝
```

### Step 3: Guidelines for QTE Placement

#### Good QTE Placement:

✅ Place QTEs during **visually impactful moments**  
✅ Spread them evenly (e.g., 0.3, 0.6, 0.9)  
✅ 2-4 QTE windows per attack is ideal  
✅ Last QTE should be **before** the hit frame

#### Bad QTE Placement:

❌ Too close together (0.5, 0.51, 0.52)  
❌ After the hit connects  
❌ During idle parts of animation  
❌ More than 5 QTE windows (too overwhelming)

### Step 4: Determine Hit Timing

The **Hit Normalized Time** is when damage actually applies to the enemy.

**How to find it:**

1. Watch your animation frame-by-frame
2. Find the exact moment the weapon **connects** with target
3. Divide that time by total animation length

**Example:**

- Hit connects at **1.1 seconds**
- Animation is **1.5 seconds** long
- Hit Normalized Time = 1.1 / 1.5 = **0.73**

### Step 5: Create Multiple Attack Types

Create different AttackAnimationData for variety:

#### Light Attack

- **Fast** (1-1.5 seconds)
- **2-3 QTE windows**
- **Lower damage** (10-20)
- **Low stamina cost** (15-25)

#### Heavy Attack

- **Slow** (2-3 seconds)
- **4-5 QTE windows**
- **Higher damage** (30-50)
- **High stamina cost** (40-60)

---

## 👹 Enemy Attack Animations

Enemy attacks are simpler but need **parry timing** setup.

### Step 1: Analyze Enemy Animation

Watch the enemy attack and identify:

1. **Wind-up phase** - enemy prepares attack
2. **Strike phase** - weapon/fist moves toward player
3. **Hit moment** - exact frame damage should apply
4. **Recovery phase** - enemy returns to stance

**Example Enemy Slash (2.0 seconds):**

```
0.0s (0%) - Start stance
0.5s (25%) - Raise weapon (wind-up)
1.2s (60%) - Swing begins
1.5s (75%) - Weapon connects    ← HIT MOMENT
1.8s (90%) - Follow through
2.0s (100%) - Return to stance
```

### Step 2: Calculate Parry Window Timing

The parry window should open **slightly before** the hit connects.

**Formula:**

```
Parry Window Start = Hit Time - Parry Window Duration
```

**Example:**

- Hit happens at 1.5s (75% normalized)
- Parry window duration = 0.3s
- In a 2.0s animation: 0.3s = 15% of animation
- Parry window should open at: 75% - 15% = **60% normalized time**

### Step 3: Configure EnemyBattleController

In your Enemy GameObject's Inspector:

```
╔════════════════════════════════════════╗
║ EnemyBattleController                  ║
╠════════════════════════════════════════╣
║ ATTACK SETTINGS                        ║
║ • Attack Animation Name: "EnemySlash"  ║
║ • Attack Damage: 25                    ║
║ • Attack Animation Duration: 2.0       ║
║                                        ║
║ PARRY TIMING                           ║
║ • Parry Window Start Time: 0.60        ║
║   (60% through animation)              ║
║                                        ║
║ AI SETTINGS                            ║
║ • Think Duration: 1.0                  ║
║   (Delay before attacking)             ║
╚════════════════════════════════════════╝
```

### Step 4: Visual Reference for Parry Timing

```
Enemy Attack Timeline (2.0 seconds):

0%              60%          75%              100%
│────────────────│────────────│────────────────│
Start         Parry       Hit             End
Stance        Opens     Connects
              ↓
         ┌────┴────┐
         │ 0.3s    │ ← Parry Window Duration
         │ Window  │
         └─────────┘
```

### Step 5: Configure ParrySystem

In Unity, select your ParrySystem GameObject:

```
╔════════════════════════════════════════╗
║ ParrySystem                            ║
╠════════════════════════════════════════╣
║ • Parry Input Key: Space               ║
║ • Parry Window Duration: 0.3           ║
║ • Perfect Parry Window: 0.1            ║
║ • Stamina Reward: 30                   ║
╚════════════════════════════════════════╝
```

**Parry Window Duration:** How long player has to parry  
**Perfect Parry Window:** Extra tight timing for "perfect" parry  
**Stamina Reward:** Stamina gained on successful parry

---

## 🔮 Skill Animations

Skills are similar to attacks but use **SkillData** instead.

### Step 1: Create SkillData

1. Right-click in Project → **Create → Battle → Skill Data**
2. Name it: `PowerStrike_Skill`
3. Configure:

```
╔════════════════════════════════════════╗
║ SkillData - Power Strike              ║
╠════════════════════════════════════════╣
║ BASIC INFO                             ║
║ • Skill Name: "Power Strike"           ║
║ • Description: "A devastating blow"    ║
║                                        ║
║ ANIMATION                              ║
║ • Animation State Name: "PowerStrike"  ║
║                                        ║
║ STATS                                  ║
║ • Stamina Cost: 40                     ║
║ • Damage Amount: 35                    ║
║                                        ║
║ EFFECTS                                ║
║ • Heals Player: false                  ║
║ • Heal Amount: 0                       ║
╚════════════════════════════════════════╝
```

### Step 2: Skills vs Attacks

**Attacks (AttackAnimationData):**

- Have QTE windows
- Damage scales with QTE success
- More interactive

**Skills (SkillData):**

- No QTE windows
- Fixed damage
- Higher cost, higher damage
- Can have special effects (healing)

### Step 3: Creating a Healing Skill

```
╔════════════════════════════════════════╗
║ SkillData - Meditation                ║
╠════════════════════════════════════════╣
║ • Skill Name: "Meditation"             ║
║ • Animation State Name: "Meditate"     ║
║ • Stamina Cost: 50                     ║
║ • Damage Amount: 0                     ║
║ • Heals Player: ✅ true                ║
║ • Heal Amount: 40                      ║
╚════════════════════════════════════════╝
```

---

## 🧪 Testing Your Animations

### Phase 1: Test Animations in Isolation

1. **Open Animation window** (Window → Animation → Animation)
2. **Select your character**
3. **Play each animation** and note:
   - Total duration
   - Key frames (wind-up, hit, recovery)
   - Visual impact moments

### Phase 2: Test in Battle Scene

1. **Add debug logs** to see timing:

```csharp
// In AnimationSequencer.cs, the TriggerQTE method already logs:
Debug.Log($"QTE Window {qteIndex + 1} triggered!");

// In ParrySystem.cs, OpenParryWindow logs:
Debug.Log($"PARRY WINDOW! Press {parryInputKey}");
```

2. **Enter Play Mode**
3. **Start a battle**
4. **Watch the Console** for timing logs

### Phase 3: Adjust Timings

If QTE windows feel wrong:

#### QTE Comes Too Early

- Increase the normalized time value
- Example: Change 0.4 → 0.5

#### QTE Comes Too Late

- Decrease the normalized time value
- Example: Change 0.8 → 0.7

#### Hit Connects Before Damage Applies

- Decrease Hit Normalized Time
- Example: Change 0.8 → 0.75

#### Parry Window Opens Too Late

- Decrease Parry Window Start Time
- Example: Change 0.7 → 0.6

### Phase 4: Feel Testing

Play the game and ask yourself:

✅ **QTE Windows:**

- Do they appear during visually exciting moments?
- Are they spaced out enough to be fair?
- Do they feel satisfying to hit?

✅ **Damage Timing:**

- Does damage apply when weapon visually connects?
- Does it feel impactful?

✅ **Parry Windows:**

- Does the parry window open with enough warning?
- Is it possible but challenging?
- Does the visual indicator appear at the right time?

---

## 🎯 Practical Example: Full Setup

Let's create a complete attack from scratch.

### Animation: "Spinning Slash" (2.5 seconds)

#### Step 1: Timeline Analysis

```
Time    Normalized    What's Happening
0.0s    0.0           Character plants feet
0.5s    0.2           Begins to spin
1.0s    0.4           Full spin, building momentum    ← QTE 1
1.5s    0.6           Second rotation                 ← QTE 2
1.8s    0.72          Weapon extends for final slash  ← QTE 3
2.0s    0.8           Blade connects with enemy       ← HIT
2.3s    0.92          Follow-through
2.5s    1.0           Return to ready stance
```

#### Step 2: Create AttackAnimationData

```
Name: SpinningSlash_Data

Animation State Name: "SpinningSlash"
Animation Duration: 2.5

QTE Timings:
  [0] 0.4
  [1] 0.6
  [2] 0.72

Base Damage: 25
QTE Success Multiplier: 1.8
Hit Normalized Time: 0.8
Stamina Cost: 35
```

#### Step 3: Test & Refine

1. Play the animation
2. If QTE at 0.4 feels early, try 0.45
3. If damage applies too late, try Hit Time 0.75
4. Iterate until it feels good!

---

## 🐛 Troubleshooting

### Problem: QTE windows never appear

**Check:**

1. Is `QTEManager` assigned in `BattleManagerV2`?
2. Is the animation state name spelled exactly correct?
3. Are QTE timings between 0.0 and 1.0?

**Debug:**
Add this to `AnimationSequencer.cs` in `CheckQTETriggers()`:

```csharp
Debug.Log($"Current time: {currentTime}, Previous: {previousTime}");
```

---

### Problem: Animations play automatically outside combat / Player attacks without input

**Cause:**
You have transitions FROM Idle TO attack states in your Animator Controller.

**Solution:**

1. Open your Animator Controller
2. Select the Idle state
3. **Delete ALL outgoing transitions** from Idle (right-click transition → Delete)
4. Keep only the incoming transitions (Attack → Idle, TakeDamage → Idle, etc.)

**Why:**
The battle system uses `animator.Play()` to force state changes. Transitions from Idle are unnecessary and cause auto-playing. Only create transitions that return TO Idle, never FROM Idle.

**Correct Structure:**

```
     ↑ Only incoming transitions
     │
Idle │ ← LightAttack
     │ ← HeavyAttack
     │ ← TakeDamage

❌ No outgoing transitions from Idle!
```

---

### Problem: Damage applies at wrong time

**Check:**

1. Watch animation frame-by-frame
2. Note exact time weapon connects
3. Recalculate: (hit time in seconds) / (total animation length)

**Example:**

- If 2.0s animation, hit at 1.6s
- Correct timing: 1.6 / 2.0 = 0.8

---

### Problem: Parry window opens too late/early

**Solution:**
The parry system uses **absolute time in seconds**, not normalized time!

In `EnemyBattleController`, you need to calculate when to call `OpenParryWindow()`.

**Formula:**

```
Parry Start (seconds) = Hit Time (seconds) - Parry Duration

Then in code, use a coroutine:
yield return new WaitForSeconds(parryStartTime);
parrySystem.OpenParryWindow();
```

---

### Problem: Animation doesn't return to Idle

**Check:**

1. Animator transitions have "Has Exit Time" enabled
2. Exit Time is set to 1.0
3. There's a transition back to Idle state

---

### Problem: QTE timing feels inconsistent

**Check:**

1. Animation is not set to loop
2. Animation speed is 1.0 (not faster or slower)
3. Normalized time calculations are correct

---

## 📊 Quick Reference Tables

### Recommended QTE Counts

| Attack Speed | Duration | QTE Count |
| ------------ | -------- | --------- |
| Very Fast    | < 1.0s   | 1-2       |
| Fast         | 1.0-1.5s | 2-3       |
| Medium       | 1.5-2.5s | 3-4       |
| Slow         | 2.5-3.5s | 4-5       |
| Very Slow    | > 3.5s   | 5-6       |

### Recommended Parry Window Durations

| Difficulty | Duration |
| ---------- | -------- |
| Easy       | 0.5s     |
| Normal     | 0.3s     |
| Hard       | 0.2s     |
| Expert     | 0.15s    |

### Typical Damage Values

| Attack Type | Base Damage | With QTEs | Stamina Cost |
| ----------- | ----------- | --------- | ------------ |
| Light       | 10-15       | 15-22     | 15-25        |
| Medium      | 20-25       | 30-37     | 30-40        |
| Heavy       | 30-40       | 45-60     | 45-60        |
| Skill       | 35-50       | N/A       | 40-70        |

---

## ✅ Checklist: Before Testing

### For Each Player Attack:

- [ ] Animation imported and added to Animator
- [ ] AttackAnimationData ScriptableObject created
- [ ] Animation state name matches exactly
- [ ] QTE timings set (0.0 to 1.0)
- [ ] Hit normalized time set
- [ ] Damage and stamina values configured
- [ ] Assigned to PlayerBattleController

### For Each Enemy Attack:

- [ ] Animation imported and added to Animator
- [ ] Animation state name set in EnemyBattleController
- [ ] Attack damage configured
- [ ] Parry window timing calculated
- [ ] Parry system duration set

### Systems Setup:

- [ ] QTEManager exists in scene
- [ ] ParrySystem exists in scene
- [ ] AnimationSequencer exists in scene
- [ ] All systems assigned to BattleManagerV2
- [ ] Input keys configured (default: Space)

---

## 🎓 Final Tips

1. **Start Simple**: Begin with 1-2 attacks, test thoroughly, then add more
2. **Watch Real Gameplay**: Play action RPGs and note how they time things
3. **Feel Over Math**: If it feels good, the numbers are right
4. **Iterate**: First pass won't be perfect - adjust based on feel
5. **Get Feedback**: Have others playtest and note their reactions
6. **Use Visual Indicators**: Make sure players know when to press buttons
7. **Sound Effects Help**: Audio cues make timing easier to feel

---

## 🔗 Next Steps

After setting up animations:

1. Create UI indicators for QTE windows
2. Add visual effects at hit moments
3. Implement camera shake on impacts
4. Add particle effects for successful parries
5. Create combo system between attacks

---

Good luck with your combat system! 🗡️✨
