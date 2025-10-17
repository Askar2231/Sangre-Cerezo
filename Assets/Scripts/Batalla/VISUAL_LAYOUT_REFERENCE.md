# 🎨 Visual Layout Reference

## Before & After Comparison

### HORIZONTAL LAYOUT (Skills - Default)

```
┌─────────────────────────────────────┐
│  AttackButtonsContainer             │
│  ┌─────────┐ ┌─────────┐           │
│  │ Light   │ │ Heavy   │           │
│  │ Attack  │ │ Attack  │           │
│  └─────────┘ └─────────┘           │
│  ◄─ 10px ─►                        │
└─────────────────────────────────────┘
```

### VERTICAL LAYOUT (Attacks - NEW)

```
┌──────────────────┐
│ AttackButtons    │
│ Container        │
│                  │
│  ┌────────────┐  │
│  │   Light    │  │
│  │   Attack   │  │
│  └────────────┘  │
│        ▲         │
│     40px gap     │
│        ▼         │
│  ┌────────────┐  │
│  │   Heavy    │  │
│  │   Attack   │  │
│  └────────────┘  │
│                  │
└──────────────────┘
```

## Aspect Ratio Preservation

### WITHOUT AspectRatioFitter (BAD ❌)

```
Original Prefab (150x60):
┌───────────────┐
│   Button      │  <- Normal proportions
└───────────────┘

Stretched in Vertical Layout:
┌─────────────────────┐
│                     │
│      Button         │  <- TOO WIDE!
│                     │
└─────────────────────┘
```

### WITH AspectRatioFitter (GOOD ✅)

```
Original Prefab (150x60):
┌───────────────┐
│   Button      │  <- Normal proportions
└───────────────┘

Scaled in Vertical Layout:
┌──────────────┐
│   Button     │  <- Same proportions, just scaled!
└──────────────┘

or larger:

┌─────────────────────┐
│      Button         │  <- Same ratio (2.5:1)
└─────────────────────┘
```

## Component Hierarchy

```
BattleActionButton GameObject
├─ RectTransform (position & size)
├─ Image (button background)
├─ TextMeshProUGUI (label)
├─ Button (click interaction)
├─ BattleActionButton (script logic)
├─ LayoutElement (size hints) ◄── Auto-added
└─ AspectRatioFitter (ratio lock) ◄── Auto-added
```

## Size Flow Chart

```
Container Size
     │
     ▼
┌──────────────────────────────┐
│ VerticalLayoutGroup decides: │
│ - Available height           │
│ - Spacing between buttons    │
│ - Number of children         │
└──────────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│ LayoutElement suggests:      │
│ - Preferred width            │
│ - Flexible width (scale)     │
└──────────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│ AspectRatioFitter enforces:  │
│ - Width changes              │
│ - Height adjusts to match    │
│ - Ratio preserved            │
└──────────────────────────────┘
     │
     ▼
   Final Button Size
   (Scaled uniformly!)
```

## Spacing Examples

### Small Spacing (10px)

```
┌────────┐
│ Button │
├────────┤ ◄─ 10px
│ Button │
└────────┘
```

### Medium Spacing (30px)

```
┌────────┐
│ Button │
│        │
├────────┤ ◄─ 30px
│        │
│ Button │
└────────┘
```

### Large Spacing (60px)

```
┌────────┐
│ Button │
│        │
│        │
├────────┤ ◄─ 60px
│        │
│        │
│ Button │
└────────┘
```

## Inspector Settings at a Glance

```
DynamicBattleUI Component
├─ References
│  └─ attackButtonsContainer ◄── Assign your container here
│
├─ Layout Settings
│  ├─ useVerticalLayoutForAttacks: ✓ ON  ◄── VERTICAL MODE
│  ├─ attackButtonSpacing: 40         ◄── GAP SIZE
│  ├─ maintainAspectRatio: ✓ ON     ◄── PRESERVE RATIO
│  ├─ buttonSpacing: 10 (for skills)
│  └─ forceExpandButtons: ✓ ON      ◄── ALLOW SCALING
│
└─ Settings
   └─ autoSetupLayoutGroups: ✓ ON   ◄── AUTO-CONFIGURE
```

## Common Configurations

### Compact Vertical (Small Screen)

```csharp
attackButtonSpacing = 20f
Container Height = 180px
Result: Tight, space-efficient
```

### Standard Vertical (Default)

```csharp
attackButtonSpacing = 40f
Container Height = 250px
Result: Comfortable spacing
```

### Spacious Vertical (Large Screen)

```csharp
attackButtonSpacing = 60f
Container Height = 350px
Result: Luxurious, easy to click
```

## Quick Setup Checklist

1. ✅ `DynamicBattleUI` component on GameObject
2. ✅ `attackButtonsContainer` assigned
3. ✅ `useVerticalLayoutForAttacks` = **TRUE**
4. ✅ `attackButtonSpacing` = **40**
5. ✅ `maintainAspectRatio` = **TRUE**
6. ✅ Container RectTransform height ≥ 200px
7. ✅ Button prefab has reasonable default size

## Result Preview

```
╔════════════════════════════════════╗
║     BATTLE UI - VERTICAL MODE      ║
╠════════════════════════════════════╣
║                                    ║
║     ┌──────────────────────┐      ║
║     │  🖱️ Light Attack     │      ║
║     │  (Mouse Left)         │      ║
║     └──────────────────────┘      ║
║              ▼ 40px               ║
║     ┌──────────────────────┐      ║
║     │  🖱️ Heavy Attack     │      ║
║     │  (Mouse Right)        │      ║
║     └──────────────────────┘      ║
║                                    ║
║  ═══════════════════════════      ║
║                                    ║
║  ┌─────────┐  ┌─────────┐        ║
║  │ Skill 1 │  │ Skill 2 │        ║
║  └─────────┘  └─────────┘        ║
║                                    ║
╚════════════════════════════════════╝
```

---

**TIP:** The buttons will automatically scale to fit your container while maintaining their original shape! No manual tweaking required. 🎉
