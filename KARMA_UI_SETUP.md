# 📊 Karma UI System - Setup & Verification Guide

## ✅ Changes Made

### 1. KarmaManager.cs - Enhanced with UI Support

**Added:**

- `TextMeshProUGUI karmaText` - Reference to display karma value
- `GameObject karmaUI` - Optional parent container for karma display
- Color settings for positive (green), neutral (white), and negative (red) karma
- `OnKarmaChanged` event for other systems to react to karma changes
- `UpdateKarmaUI()` - Automatically updates display when karma changes
- `SetKarmaUIVisible()` - Show/hide karma display
- `ResetKarma()` - Reset karma to 0 (for testing)
- Better debug logging showing karma changes

### 2. InteractionManager.cs - Enhanced Karma Logging

**Added:**

- Logs showing karma effect value for each choice
- Warning when a choice has no karma effect configured
- Clear indication when karma is being applied

### 3. KarmaUITester.cs - NEW Testing Helper

**Features:**

- Context menu options to test karma in editor
- Public methods for UI buttons
- Can add positive/negative karma or reset

## 🎮 Unity Setup Instructions

### Step 1: Create Karma UI in Scene

1. **Create Karma Display GameObject:**

   - In Hierarchy, find your Canvas (or create one: GameObject → UI → Canvas)
   - Right-click Canvas → UI → Text - TextMeshPro
   - Name it "KarmaText"

2. **Configure Text Appearance:**

   - Position it where you want (top-right corner is typical)
   - Set font size (e.g., 24-36)
   - Set alignment to center or right
   - Change text to "Karma: 0" temporarily to see it

3. **Optional: Create Container:**
   - Right-click Canvas → Create Empty
   - Name it "KarmaUI"
   - Drag "KarmaText" to be a child of "KarmaUI"
   - This lets you show/hide the entire karma display as one unit

### Step 2: Configure KarmaManager

1. **Find KarmaManager in Scene:**

   - Look for an existing KarmaManager GameObject
   - If it doesn't exist, create one: Create Empty GameObject → Name it "KarmaManager"
   - Add the KarmaManager component

2. **Assign UI References:**

   - Select KarmaManager GameObject
   - In Inspector, find the KarmaManager component
   - Drag "KarmaText" from Hierarchy to the **Karma Text** field
   - Drag "KarmaUI" (if you created it) to the **Karma UI** field

3. **Configure Display Settings:**
   - **Show Karma UI:** Check/uncheck to show/hide on start
   - **Karma Prefix:** "Karma: " (default, or customize like "名誉: " for Japanese)
   - **Positive Karma Color:** Green (default)
   - **Neutral Karma Color:** White (default)
   - **Negative Karma Color:** Red (default)

### Step 3: Verify Karma is Configured on Choices

You need to check your Decision ScriptableObjects and make sure karma values are set:

1. **Initial Merchant Dialogue:**

   - In Project window, find your merchant dialogue Decision asset
   - Open it in Inspector
   - Check each Choice:
     - **Accept Help:** Should have positive karma (e.g., +5 or +10)
     - **Decline Help:** Could have negative karma (e.g., -5) or 0

2. **Post-Combat Decision (Thief):**

   - **Forgive Thief:** High positive karma (e.g., +20)
   - **Kill Thief:** Negative karma (e.g., -15)

3. **Final Merchant Decision (Omamori):**
   - **Return Omamori:** Positive karma (e.g., +15)
   - **Keep Omamori:** Negative karma (e.g., -10)

### Step 4: Testing

#### Method 1: Use Context Menu (Editor Only)

1. Select KarmaManager GameObject
2. Add the KarmaUITester component
3. Right-click KarmaUITester component header
4. Select:
   - "Add Positive Karma (+10)"
   - "Add Negative Karma (-10)"
   - "Reset Karma to 0"
   - "Show Current Karma"
5. **Watch the UI update** in Game view (must be in Play Mode)

#### Method 2: Test In-Game

1. Enter Play Mode
2. Start the robbery quest
3. Make choices
4. **Watch Console** for karma logs:
   ```
   [InteractionManager] 📊 Aplicando karma: +10
   [KarmaManager] Karma añadido: 10. Karma anterior: 0 → Karma actual: 10
   ```
5. **Watch the UI** - text should update and change color

## 🐛 Troubleshooting

### Issue: Karma UI Not Showing

**Check:**

1. Is "Show Karma UI" enabled in KarmaManager Inspector?
2. Is the KarmaText reference assigned?
3. Is the Canvas active in hierarchy?
4. Is the KarmaText GameObject active?
5. Try pressing Play - UI might only appear at runtime

### Issue: Karma Not Changing

**Check:**

1. Open Console (Window → General → Console)
2. Make a choice in-game
3. Look for logs:
   ```
   [InteractionManager] Esta elección no tiene efecto de karma configurado
   ```
4. If you see this, the karma value is 0 on that choice
5. Go to your Decision ScriptableObject and set the **Karma Effect** field

### Issue: Choices Not Triggering

**Check:**

1. QuestManager callbacks are registered
2. Look for:
   ```
   ✓ Callbacks de misión registrados exitosamente en ChoiceEventSystem.
   ```
3. Make sure Choice IDs match:
   - "merchant_accept_help"
   - "merchant_decline_help"
   - "thief_forgive"
   - "thief_kill"
   - "merchant_keep_omamori"
   - "merchant_return_omamori"

### Issue: UI Text is Blank

**Check:**

1. Is TextMeshPro imported? (Window → TextMeshPro → Import TMP Essential Resources)
2. Is the text color visible against your background?
3. Try changing Karma Text color to pure white temporarily
4. Check font asset is assigned

## 📋 Expected Karma Flow for Robbery Quest

| Action                  | Karma Effect            | Total Karma (Example)  |
| ----------------------- | ----------------------- | ---------------------- |
| Start:                  | 0                       | 0                      |
| Accept to help merchant | +5 to +10               | 5-10                   |
| Defeat thief            | 0 (combat has no karma) | 5-10                   |
| **Forgive thief**       | +15 to +20              | 20-30                  |
| **OR Kill thief**       | -10 to -15              | -5 to -5               |
| **Return Omamori**      | +10 to +15              | 30-45 (good path)      |
| **OR Keep Omamori**     | -10 to -15              | -15 to -20 (evil path) |

**Good Path Total:** 30-45 karma (green text)
**Evil Path Total:** -15 to -20 karma (red text)
**Mixed Path:** Could be anywhere in between (white/green/red)

## 🎨 UI Customization Tips

### Position Options:

- **Top Right:** Classic UI position
- **Top Left:** Near health/stamina bars
- **Bottom Center:** Near other quest info

### Style Options:

- Add background image (Panel) behind text for readability
- Add icon/sprite next to karma value
- Use different fonts for Japanese aesthetic
- Add glow/shadow effect for visibility

### Advanced: Show Karma Changes

You could create a popup that briefly shows "+10 Karma!" when making a choice:

1. Create a separate UI text that fades in/out
2. Subscribe to `KarmaManager.OnKarmaChanged` event
3. Show the delta (change amount) briefly
4. Fade out after 2-3 seconds

## 🧪 Testing Checklist

- [ ] KarmaManager exists in scene with component
- [ ] KarmaText reference is assigned
- [ ] Karma UI is visible in Game view (Play Mode)
- [ ] Context menu test changes the displayed value
- [ ] Text color changes: green when positive, red when negative
- [ ] Accepting merchant quest shows karma increase in console
- [ ] Forgiving thief shows karma increase in console
- [ ] Killing thief shows karma decrease in console
- [ ] Returning Omamori shows karma increase in console
- [ ] Keeping Omamori shows karma decrease in console
- [ ] UI updates in real-time when making choices

## 📝 Suggested Karma Values

Here are balanced karma values for your quest:

```
merchant_accept_help: +10 (helping those in need)
merchant_decline_help: -5 (refusing to help)

thief_forgive: +25 (mercy and compassion)
thief_kill: -20 (unnecessary violence)

merchant_return_omamori: +15 (honesty and honor)
merchant_keep_omamori: -15 (theft and dishonor)

Best Outcome: +50 karma (help + forgive + return)
Worst Outcome: -40 karma (decline + kill + keep)
```

---

**Status:** ✅ Karma UI system fully implemented
**Next Steps:** Configure karma values in Decision ScriptableObjects and test in-game
