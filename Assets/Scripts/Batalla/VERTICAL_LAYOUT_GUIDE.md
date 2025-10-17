# 📐 Vertical Layout Setup Guide - Attack Buttons

## ✨ Overview

This guide explains how to setup the attack button container with **vertical layout**, **large gaps**, and **maintained aspect ratio** for the battle UI.

---

## 🎯 Key Features

✅ **Vertical Layout** - Buttons stack vertically instead of horizontally
✅ **Large Gaps** - Configurable spacing between buttons (default: 40px)
✅ **Aspect Ratio Maintained** - Buttons scale uniformly without distortion
✅ **Automatic Detection** - Uses `AspectRatioFitter` to preserve prefab proportions

---

## 🛠️ Setup in Unity Inspector

### Step 1: Select DynamicBattleUI GameObject

Find the GameObject with the `DynamicBattleUI` component in your battle scene.

### Step 2: Configure Layout Settings

In the **Layout Settings** section:

| Setting                             | Value      | Description                                     |
| ----------------------------------- | ---------- | ----------------------------------------------- |
| **Use Vertical Layout For Attacks** | ✅ Checked | Stacks attack buttons vertically                |
| **Attack Button Spacing**           | `40`       | Gap between buttons (in pixels)                 |
| **Maintain Aspect Ratio**           | ✅ Checked | Preserves button proportions when scaling       |
| **Button Spacing**                  | `10`       | Default spacing for horizontal layouts (skills) |
| **Force Expand Buttons**            | ✅ Checked | Allows buttons to scale to fit container        |

### Step 3: Setup Container Hierarchy

Your UI structure should look like this:

```
Battle UI Canvas
├─ AttackButtonsContainer (VerticalLayoutGroup - auto-added)
│   ├─ LightAttackButton (auto-generated with AspectRatioFitter)
│   └─ HeavyAttackButton (auto-generated with AspectRatioFitter)
│
└─ SkillButtonsContainer (HorizontalLayoutGroup - auto-added)
    ├─ Skill1Button (auto-generated)
    └─ Skill2Button (auto-generated)
```

---

## 🔧 How It Works

### Vertical Layout Configuration

When `useVerticalLayoutForAttacks` is enabled:

```csharp
VerticalLayoutGroup Settings:
├─ Spacing: 40px (attackButtonSpacing)
├─ Child Alignment: Upper Center
├─ Child Control Width: FALSE (allows aspect ratio maintenance)
├─ Child Control Height: FALSE (allows aspect ratio maintenance)
├─ Child Scale Width: TRUE (uniform scaling)
└─ Child Scale Height: TRUE (uniform scaling)
```

### Aspect Ratio Maintenance

Each button gets an `AspectRatioFitter` component:

```csharp
AspectRatioFitter Settings:
├─ Aspect Mode: WidthControlsHeight
├─ Aspect Ratio: Auto-calculated from prefab size
└─ Example: 150px width ÷ 60px height = 2.5 ratio
```

This ensures:

- ✅ Button width changes → height adjusts proportionally
- ✅ Original button shape preserved
- ✅ No stretching or squashing

---

## 🎨 Customization Options

### Adjust Spacing

To change the gap between attack buttons:

1. Select `DynamicBattleUI` GameObject
2. Change **Attack Button Spacing** value
   - Small gaps: `10-20px`
   - Medium gaps: `30-40px`
   - Large gaps: `50-80px`

### Switch Back to Horizontal

To use horizontal layout for attack buttons:

1. Uncheck **Use Vertical Layout For Attacks**
2. Buttons will arrange horizontally like skills

### Custom Aspect Ratio

If auto-detection fails, you can set a default:

```csharp
// In SetupButtonLayout() method
if (float.IsNaN(aspectRatio) || aspectRatio == 0)
{
    aspectRatio = 2.5f; // Change this value
    // 2.5 = wide button (150x60)
    // 1.0 = square button
    // 0.5 = tall button
}
```

---

## 📏 Container Size Recommendations

For optimal vertical layout appearance:

### Attack Buttons Container

```
RectTransform Settings:
├─ Width: 200-300px (fixed or flexible)
├─ Height: 200-300px (should fit 2-3 buttons + spacing)
├─ Anchors: Top-Left or Center (depends on UI design)
└─ Pivot: (0.5, 1) - Center top for vertical stacking
```

### Button Prefab Size

Your button prefab should have sensible default dimensions:

```
Recommended Sizes:
├─ Small: 100x40px (ratio 2.5)
├─ Medium: 150x60px (ratio 2.5)
└─ Large: 200x80px (ratio 2.5)
```

---

## 🐛 Troubleshooting

### Problem: Buttons are stretched/squashed

**Solution:**

- ✅ Check **Maintain Aspect Ratio** is enabled
- ✅ Verify `AspectRatioFitter` component is on buttons
- ✅ Check prefab RectTransform has valid size (not 0x0)

### Problem: Buttons are too small/large

**Solution:**

- Adjust container size (RectTransform width/height)
- Change `layoutElement.minWidth` in code
- Modify button prefab base size

### Problem: Gaps are too small/large

**Solution:**

- Adjust **Attack Button Spacing** in Inspector
- Range: 10-100px depending on screen size
- Test on different resolutions

### Problem: Buttons overlap

**Solution:**

- Increase container height
- Reduce **Attack Button Spacing**
- Check if too many buttons are being generated

---

## 📖 Code Reference

### Key Methods

| Method                           | Purpose                                           |
| -------------------------------- | ------------------------------------------------- |
| `SetupLayoutGroups()`            | Decides which layout to use for each container    |
| `SetupVerticalContainerLayout()` | Configures VerticalLayoutGroup                    |
| `SetupContainerLayout()`         | Configures HorizontalLayoutGroup (skills)         |
| `SetupButtonLayout()`            | Adds AspectRatioFitter + LayoutElement to buttons |

### Inspector Variables

| Variable                      | Type  | Default | Description                        |
| ----------------------------- | ----- | ------- | ---------------------------------- |
| `useVerticalLayoutForAttacks` | bool  | true    | Enable vertical layout for attacks |
| `attackButtonSpacing`         | float | 40f     | Gap between attack buttons (px)    |
| `maintainAspectRatio`         | bool  | true    | Preserve button proportions        |
| `buttonSpacing`               | float | 10f     | Gap for horizontal layouts         |
| `forceExpandButtons`          | bool  | true    | Allow buttons to scale             |

---

## 🎮 Testing

### Test Checklist

1. **Enter Play Mode**
2. **Check Attack Buttons**:
   - ✅ Are they stacked vertically?
   - ✅ Is there visible spacing between them?
   - ✅ Do they maintain proportions (not stretched)?
3. **Check Skill Buttons**:
   - ✅ Are they arranged horizontally?
   - ✅ Do they look consistent?
4. **Resize Container**:
   - Change container width → buttons scale uniformly
   - Change container height → buttons don't distort
5. **Different Resolutions**:
   - Test on various screen sizes
   - Verify layout adapts correctly

---

## 🚀 Advanced Usage

### Mixed Layouts

You can have different layouts per container:

```csharp
// Attacks: Vertical
useVerticalLayoutForAttacks = true

// Skills: Horizontal (default)
// Automatically uses HorizontalLayoutGroup
```

### Custom Aspect Ratios Per Button Type

Modify `CreateAttackButton()` to set different ratios:

```csharp
private void CreateAttackButton(AttackAnimationData attackData, Transform parent, bool isHeavy)
{
    // ... existing code ...

    // Custom aspect ratio for heavy attacks
    if (isHeavy)
    {
        var fitter = buttonObj.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = 3.0f; // Wider heavy attack button
        }
    }
}
```

### Dynamic Spacing Based on Button Count

Adjust spacing based on number of buttons:

```csharp
private void SetupVerticalContainerLayout(Transform container, string containerName, float spacing)
{
    int buttonCount = container.childCount;
    float dynamicSpacing = buttonCount > 2 ? spacing * 0.7f : spacing;

    verticalLayout.spacing = dynamicSpacing;
    // ... rest of configuration ...
}
```

---

## 📚 Related Components

- **VerticalLayoutGroup**: Unity's built-in vertical stacking component
- **AspectRatioFitter**: Maintains width/height proportions
- **LayoutElement**: Controls min/preferred/flexible sizes
- **RectTransform**: Defines UI element position and size

---

## 💡 Tips & Best Practices

1. **Container Size Matters**: Make sure the container is tall enough for all buttons + spacing
2. **Consistent Prefabs**: Use the same aspect ratio for all button prefabs
3. **Test Early**: Check layout in Editor Scene view before Play mode
4. **Flexible Sizing**: Enable `forceExpandButtons` for responsive layouts
5. **Clear Hierarchy**: Keep button containers as direct children of parent UI

---

## 📝 Summary

With this setup, your attack buttons will:

- ✅ Stack vertically with customizable gaps
- ✅ Scale uniformly to fit the container
- ✅ Maintain their original aspect ratio
- ✅ Work responsively across different screen sizes

No manual tweaking needed - the system handles everything automatically! 🎉
