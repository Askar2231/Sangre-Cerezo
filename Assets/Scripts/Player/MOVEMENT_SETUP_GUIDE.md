# Movement System Setup Guide (1D Forward-Only Movement)

## Overview
The `MovimientoV2` script supports forward-only movement with rotation:
- ✅ Unity's new Input System (with legacy fallback)
- ✅ 1D Animation Blend Tree (Speed parameter only)
- ✅ Character rotates towards joystick direction
- ✅ Only moves forward (no strafing or backpedaling)
- ✅ Walking and Running speeds
- ✅ Smooth acceleration and deceleration
- ✅ Keyboard and gamepad support

**Similar to:** Dark Souls, Monster Hunter, Resident Evil 4, etc.

---

## Unity Input System Setup

### Step 1: Install Input System Package
1. Open **Window → Package Manager**
2. Search for **"Input System"**
3. Click **Install**
4. Unity will ask to restart - click **Yes**

### Step 2: Create Input Actions Asset
1. Right-click in Project → **Create → Input Actions**
2. Name it `PlayerInputActions`
3. Double-click to open the Input Actions editor

### Step 3: Configure Input Actions
In the Input Actions window:

#### Add Action Map: "Player"
1. Click **+** next to "Action Maps"
2. Name it `Player`

#### Add Movement Action
1. Select `Player` action map
2. Click **+** next to "Actions"
3. Name it `Move`
4. Set **Action Type** to `Value`
5. Set **Control Type** to `Vector2`

**Add Bindings:**
- Click **+** next to `Move` → **Add 2D Vector Composite**
- Configure bindings:
  - **Up**: W [Keyboard]
  - **Down**: S [Keyboard]
  - **Left**: A [Keyboard]
  - **Right**: D [Keyboard]

- Click **+** next to `Move` again → **Add Binding**
- Path: **Left Stick [Gamepad]**

#### Add Run Action
1. Click **+** next to "Actions"
2. Name it `Run`
3. Set **Action Type** to `Button`

**Add Bindings:**
- Click **+** next to `Run` → **Add Binding**
  - Path: **Left Shift [Keyboard]**
- Click **+** next to `Run` → **Add Binding**
  - Path: **Left Trigger [Gamepad]**

### Step 4: Save and Generate C# Class (Optional)
1. Click **Save Asset** (top left)
2. Check **Generate C# Class** (in inspector)
3. Click **Apply**

---

## MovimientoV2 Inspector Setup

### Inspector Configuration

```
Movement Settings:
├── Walk Speed: 2           (units per second when walking)
├── Run Speed: 5            (units per second when running)
├── Rotation Speed: 720     (degrees per second - how fast character rotates)
└── Camera Transform: [Assign Main Camera or Camera Follow]

Acceleration:
├── Acceleration Time: 0.2  (seconds to reach max speed)
└── Deceleration Time: 0.15 (seconds to stop completely)

Animation Speed Limits:
├── Walk Animation Speed: 1.0  (animation speed value for walking)
└── Run Animation Speed: 2.0   (animation speed value for running)

Input (Unity Input System):
├── Move Action: [Assign Player/Move]
└── Run Action: [Assign Player/Run]
```

### Assigning Input Action References
1. Drag `PlayerInputActions` asset to the scene or select the Player GameObject
2. In `MovimientoBasico` component:
   - **Move Action**: Click + icon → Select `Player/Move`
   - **Run Action**: Click + icon → Select `Player/Run`

---

## Animator Setup (1D Blend Tree)

### Required Animation Parameter
Your Animator Controller needs only **one Float** parameter:
- `Speed` - Movement speed (0 to 2)
  - 0 = Idle
  - 1 = Walking
  - 2 = Running

### Creating a 1D Blend Tree

#### Step 1: Create Blend Tree
1. Open your **Animator Controller**
2. Right-click in Base Layer → **Create State → From New Blend Tree**
3. Name it `Locomotion`

#### Step 2: Configure 1D Blend Tree
1. Double-click the `Locomotion` state
2. In Inspector:
   - **Blend Type**: `1D`
   - **Parameter**: `Speed`

#### Step 3: Add Animation Clips
Click **+ → Add Motion Field** for each animation:

**Minimum Setup (3 animations):**
```
Threshold | Animation
----------|------------------
   0.0    | Idle
   1.0    | Walk Forward
   2.0    | Run Forward
```

**Recommended Setup (5 animations for smoother transitions):**
```
Threshold | Animation
----------|------------------
   0.0    | Idle
   0.5    | Walk Start
   1.0    | Walk Forward
   1.5    | Run Start
   2.0    | Run Forward
```

**That's it!** Much simpler than 2D blend trees.

---

## How the System Works

### Movement Behavior
1. **Input Detection** - Player pushes joystick/presses WASD in any direction
2. **Character Rotation** - Character rotates to face that direction (camera-relative)
3. **Forward Movement** - Character moves forward along their facing direction
4. **No Backpedaling** - Character always faces where they're moving

### Animation Speed Values
The script sends a single `Speed` value to the animator:

**Idle (no input):**
- Speed: `0`

**Walking (not holding Shift/LT):**
- Speed smoothly transitions to: `1.0`

**Running (holding Shift/LT):**
- Speed smoothly transitions to: `2.0`

### Acceleration/Deceleration
- `Acceleration Time`: How long it takes to reach target speed (0.2s default)
- `Deceleration Time`: How long it takes to stop (0.15s default)
- Uses `SmoothDamp` for smooth, natural transitions

### Input Sources
The script supports **both**:
1. **Unity Input System** (if actions assigned)
2. **Legacy Input** (automatic fallback if not assigned)

### Analog Stick Support
- Partial joystick push = slower movement
- Full joystick push = full speed
- Speed parameter scales with input magnitude

---

## Testing Without Input System

If you don't want to use the Input System yet:
1. Leave `Move Action` and `Run Action` **empty**
2. The script will use **legacy input**:
   - WASD or Arrow Keys for movement
   - Left Shift for running

---

## Common Issues & Solutions

### Issue: "UnityEngine.InputSystem not found"
**Solution:**
- Install Input System package (see Step 1 above)
- OR add `#if UNITY_INPUT_SYSTEM_INSTALLED` preprocessor directives

### Issue: Animations not playing
**Solution:**
- Check Animator has parameter: `Speed` (Float)
- Verify Blend Tree thresholds: 0 (Idle), 1 (Walk), 2 (Run)
- Ensure animations are assigned to motion fields

### Issue: Character moves too fast/slow
**Solution:**
- Adjust `Walk Speed` and `Run Speed` in inspector
- Default: Walk=2, Run=5

### Issue: Animations don't match movement speed
**Solution:**
- Check `Animation Speed Limits` values
- Walk should be 1.0, Run should be 2.0
- Match these values to your Blend Tree thresholds

### Issue: Character slides when stopping
**Solution:**
- Decrease `Deceleration Time` (try 0.1 or 0.05)
- Or disable root motion if enabled

### Issue: Character rotates too slowly/quickly
**Solution:**
- Adjust `Rotation Speed` in inspector
- Lower = slower, smoother turns (360-480)
- Higher = faster, snappier turns (720-1080)

### Issue: Gamepad not working
**Solution:**
- Ensure Input Actions are assigned
- Check gamepad is connected before starting play mode
- Verify bindings in Input Actions (Left Stick, LT)

---

## Animation Parameters Reference

Set this in your **Animator Controller**:

| Parameter | Type  | Range   | Values            |
|-----------|-------|---------|-------------------|
| Speed     | Float | 0 to 2  | 0=Idle, 1=Walk, 2=Run |

---

## Example Animator Transitions

### From Idle to Locomotion
- **Has Exit Time**: No
- **Transition Duration**: 0.1s
- **Conditions**: `Speed > 0.01`

### From Locomotion to Idle
- **Has Exit Time**: No
- **Transition Duration**: 0.15s
- **Conditions**: `Speed < 0.01`

---

## Performance Notes

- Uses `Animator.StringToHash` for parameter IDs (cached)
- `SmoothDamp` is efficient for smooth movement
- Input System is more performant than legacy input
- No GetComponent calls in Update loop

---

## Extension Ideas

### Add Sprint Stamina
```csharp
[SerializeField] private float maxStamina = 100f;
private float currentStamina;

private void UpdateStamina()
{
    if (isRunning && currentInput.magnitude > 0.1f)
    {
        currentStamina -= Time.deltaTime * 10f;
        if (currentStamina <= 0)
            isRunning = false;
    }
    else
    {
        currentStamina = Mathf.Min(currentStamina + Time.deltaTime * 5f, maxStamina);
    }
}
```

### Add Footstep Sounds
```csharp
public void OnFootstep() // Called from animation event
{
    if (currentAnimationBlend.magnitude > 0.1f)
    {
        AudioSource.PlayOneShot(footstepSound);
    }
}
```

### Disable During Battle ✅ **INTEGRATED**
The MovimientoV2 script now has built-in movement control:

```csharp
// Disable movement (e.g., when battle starts)
playerMovement.SetMovementEnabled(false);

// Re-enable movement (e.g., when battle ends)
playerMovement.SetMovementEnabled(true);

// Force instant stop (no smooth deceleration)
playerMovement.ForceStopMovement();
```

**Automatic Integration with BattleManagerV2:**
- Movement is automatically disabled when battle starts
- Movement is automatically re-enabled when battle ends
- Just assign the MovimientoV2 component in BattleManagerV2 inspector
- Character smoothly decelerates to idle when disabled

---

## Movement Type Comparison

### 1D Forward-Only (MovimientoV2) ✅ **YOU ARE HERE**
- Character rotates towards input
- Only moves forward
- **Pros:** Simpler setup, only 3 animations needed, feels deliberate
- **Best for:** Action games, Soulslike, survival horror
- **Examples:** Dark Souls, Monster Hunter, Resident Evil 4

### 2D Strafe Movement (Alternative)
- Character can move in any direction independently of facing
- Can strafe, backpedal
- **Pros:** More fluid, modern feel
- **Best for:** Shooters, open-world games
- **Examples:** GTA V, Uncharted, modern action games

---

## Quick Setup Checklist

- [ ] Install Unity Input System package
- [ ] Create PlayerInputActions asset
- [ ] Configure Move (Vector2) and Run (Button) actions
- [ ] Assign Input Action References in inspector
- [ ] Create Animator parameter: Speed (Float)
- [ ] Setup 1D Blend Tree with Idle, Walk, Run animations
- [ ] Assign Camera Transform
- [ ] Adjust Rotation Speed to your preference
- [ ] Test with keyboard (WASD + Shift)
- [ ] Test with gamepad (Left Stick + LT)

---

Your forward-only movement system is ready! Character will rotate and move like Dark Souls. 🎮

