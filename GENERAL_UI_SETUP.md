# 🎨 General UI Canvas Setup Guide

## Overview

The **GeneralUICanvas** is UI that should be visible **outside of combat** (exploration mode), while being automatically hidden during battles. This is perfect for displaying:

- Karma display
- Quest objectives
- Minimap
- Player stats (HP/Stamina when not in battle)
- Tutorial hints
- Any other exploration-mode UI

## ✅ Changes Made

### BattleManagerV2.cs

**Added:**

- `GameObject generalUICanvas` - Reference to UI shown outside combat
- `GameObject battleUICanvas` - Optional reference to UI shown during combat
- `SetGeneralUIVisible(bool)` - Helper method to show/hide general UI
- `SetBattleUIVisible(bool)` - Helper method to show/hide battle UI

**Automatic Behavior:**

- `InitializeBattle()` - Hides general UI when battle starts
- `EndBattle()` - Shows general UI when battle ends

## 🎮 Unity Setup Instructions

### Step 1: Organize Your Canvas Hierarchy

You likely already have UI in your scene. Let's organize it:

**Option A: Single Canvas with Groups (Recommended)**

```
Canvas
├─ GeneralUI (GameObject - this is your GeneralUICanvas)
│  ├─ KarmaDisplay
│  ├─ QuestTracker
│  ├─ Minimap
│  └─ ExplorationStats
│
└─ BattleUI (GameObject - optional BattleUICanvas)
   ├─ ActionButtons
   ├─ HealthBars
   ├─ TurnIndicator
   └─ NotificationSystem
```

**Option B: Separate Canvases**

```
GeneralUICanvas (Canvas)
├─ KarmaDisplay
├─ QuestTracker
└─ Minimap

BattleUICanvas (Canvas)
├─ ActionButtons
├─ HealthBars
└─ TurnIndicator
```

### Step 2: Create/Identify GeneralUICanvas

**If you already have a Canvas:**

1. In Hierarchy, find your main Canvas
2. Right-click inside it → Create Empty
3. Name it **"GeneralUI"**
4. **Move your Karma UI** (and any other exploration UI) to be a child of GeneralUI

**If you need to create from scratch:**

1. Right-click in Hierarchy → UI → Canvas
2. Name it **"GeneralUICanvas"**
3. Inside it, create your UI elements (Karma display, etc.)

### Step 3: Assign References in BattleManagerV2

1. **Find BattleManager GameObject:**

   - Look in your scene hierarchy for BattleManager or BattleManagerV2

2. **Select BattleManager:**

   - In Inspector, find the BattleManagerV2 component
   - Scroll to the **UI** section

3. **Assign General UI Canvas:**

   - Drag your **GeneralUI** GameObject (or GeneralUICanvas) from Hierarchy to the **General UI Canvas** field

4. **Optional: Assign Battle UI Canvas:**
   - If you have a separate battle UI parent, drag it to **Battle UI Canvas** field
   - If your battle UI is managed by `actionSelectionUI`, you can leave this empty

### Step 4: Configure Initial Visibility

**In your scene (Edit Mode):**

1. Make sure **GeneralUI is ACTIVE** (checked in Inspector)
2. Make sure **BattleUI is INACTIVE** (unchecked in Inspector) if you have one
3. This ensures the game starts with exploration UI visible

### Step 5: Test

1. **Enter Play Mode**
2. **Verify General UI is visible** (you should see Karma, etc.)
3. **Trigger a battle** (walk into an enemy)
4. **Watch Console** for logs:
   ```
   [BattleManager] General UI set to: HIDDEN
   [BattleManager] Battle UI set to: VISIBLE
   ```
5. **Verify General UI disappears** during battle
6. **Win/lose the battle**
7. **Watch Console** for logs:
   ```
   [BattleManager] General UI set to: VISIBLE
   [BattleManager] Battle UI set to: HIDDEN
   ```
8. **Verify General UI reappears** after battle

## 🎨 Example UI Layout

Here's a recommended layout for your GeneralUICanvas:

```
GeneralUI
├─ TopBar (anchored to top)
│  ├─ KarmaDisplay (top-right)
│  └─ QuestTitle (top-left)
│
├─ BottomBar (anchored to bottom)
│  ├─ PlayerHP (bottom-left)
│  └─ PlayerStamina (bottom-left)
│
└─ CenterElements
   ├─ InteractionPrompt (center-bottom)
   └─ TutorialHints (center)
```

### Karma Display Setup (Inside GeneralUI)

Since you created the Karma UI, it should be a child of GeneralUI:

```
GeneralUI
└─ KarmaUI (your karma container)
   └─ KarmaText (TextMeshPro)
```

Make sure in KarmaManager:

- `karmaUI` field is assigned to your KarmaUI GameObject
- `karmaText` field is assigned to your KarmaText component

## 🔧 Advanced Configuration

### Fade Transitions (Optional Enhancement)

If you want smooth fade in/out instead of instant show/hide:

1. Add a CanvasGroup component to GeneralUI
2. Modify `SetGeneralUIVisible()` to use alpha fading:

```csharp
private void SetGeneralUIVisible(bool visible)
{
    if (generalUICanvas != null)
    {
        // Option 1: Instant
        generalUICanvas.SetActive(visible);

        // Option 2: Fade (requires CanvasGroup)
        // CanvasGroup group = generalUICanvas.GetComponent<CanvasGroup>();
        // if (group != null)
        // {
        //     StartCoroutine(FadeCanvasGroup(group, visible ? 1f : 0f, 0.3f));
        // }
    }
}
```

### Separate UI for Different Contexts

You could extend this system for different UI contexts:

```csharp
[Header("Context UI")]
[SerializeField] private GameObject explorationUI;
[SerializeField] private GameObject dialogueUI;
[SerializeField] private GameObject battleUI;
[SerializeField] private GameObject shopUI;
```

Then create a method:

```csharp
public void SetUIContext(UIContext context)
{
    explorationUI.SetActive(context == UIContext.Exploration);
    dialogueUI.SetActive(context == UIContext.Dialogue);
    battleUI.SetActive(context == UIContext.Battle);
    shopUI.SetActive(context == UIContext.Shop);
}
```

## 🐛 Troubleshooting

### Issue: General UI Doesn't Hide During Battle

**Check:**

1. Is the GeneralUICanvas reference assigned in BattleManagerV2?
2. Look for the warning in Console:
   ```
   [BattleManager] GeneralUICanvas reference is not assigned!
   ```
3. If you see this, go back to Step 3 and assign the reference

### Issue: General UI Doesn't Show After Battle

**Check:**

1. Make sure `EndBattle()` is being called (check console for "=== BATTLE END ===")
2. Check if there's a parent GameObject that's disabled
3. Try manually enabling GeneralUI in Inspector after battle to verify it's not destroyed

### Issue: UI Flickers or Shows at Wrong Times

**Check:**

1. Make sure you don't have multiple scripts controlling the same UI
2. Check if `BattleUIManager.ShowUI()` from the old system is conflicting
3. Verify the initial state in Edit Mode (GeneralUI should start active)

### Issue: Karma UI Doesn't Appear

**Check:**

1. Is KarmaUI a child of GeneralUI? It should be!
2. Is KarmaManager's `karmaUI` field assigned?
3. Is `showKarmaUI` enabled in KarmaManager Inspector?

## 📋 Integration Checklist

- [ ] GeneralUI GameObject created/identified
- [ ] Karma UI is a child of GeneralUI
- [ ] GeneralUICanvas reference assigned in BattleManagerV2
- [ ] BattleUICanvas reference assigned (if applicable)
- [ ] GeneralUI starts active in scene
- [ ] BattleUI starts inactive (if separate)
- [ ] Tested battle start - General UI hides ✓
- [ ] Tested battle end - General UI shows ✓
- [ ] No console warnings about missing references ✓

## 💡 Best Practices

1. **Keep It Simple:** Start with one general UI parent, add complexity later
2. **Use Anchors:** Make sure UI elements are properly anchored for different screen sizes
3. **Canvas Groups:** Use CanvasGroup for fading effects and easy enable/disable
4. **Layer It:** Consider sorting order if you have overlapping UI
5. **Test Both Paths:** Test both victory and defeat to ensure UI returns correctly

## 🎯 What This Solves

✅ **Karma display visible during exploration**
✅ **Karma display hidden during combat** (less UI clutter)
✅ **Automatic UI switching** (no manual management needed)
✅ **Clean separation** between exploration and combat UI
✅ **Easy to extend** for quest trackers, minimaps, etc.

---

**Status:** ✅ General UI management fully implemented in BattleManagerV2
**Next Steps:** Assign references in Unity Inspector and test
