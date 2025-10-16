# 🎮 Combat Input Quick Reference

## 📚 Documentation Order

Read these guides in this order:

1. **INPUT_SYSTEM_SETUP.md** ← START HERE
2. **COMPLETE_SETUP_GUIDE.md**
3. **ANIMATION_SETUP_GUIDE.md**
4. **PARRY_ANIMATION_GUIDE.md**

---

## ⚡ Quick Setup Checklist

### ✅ Input System Setup (5 minutes)

1. Create Input Actions asset:
   - Right-click Project → Create → Input Actions
   - Name: `CombatInputActions`
   - Location: `Assets/Input/`

2. Add Combat Actions:
   - Action Map: `Combat`
   - Action: `QTE` (Button)
     - Binding: Space
     - Binding: Gamepad/Button South
   - Action: `Parry` (Button)
     - Binding: Space
     - Binding: Gamepad/Button South

3. Save Asset

4. Assign in Inspector:
   - QTEManager → `Qte Input Action` → CombatInputActions/Combat/QTE
   - ParrySystem → `Parry Input Action` → CombatInputActions/Combat/Parry

---

## 🎯 Component Configuration

### QTEManager
```
Qte Window Duration: 0.5
Qte Input Action: [CombatInputActions/Combat/QTE]
Perfect Timing Window: 0.1
```

### ParrySystem
```
Parry Input Action: [CombatInputActions/Combat/Parry]
Parry Window Duration: 0.3
Stamina Reward: 30
Perfect Parry Window: 0.1
```

### BattleManagerV2
```
Animation Sequencer: [AnimationSequencer GameObject]
QTE Manager: [QTEManager GameObject]
Parry System: [ParrySystem GameObject]
Turn Manager: [TurnManager GameObject]
Player: [Player GameObject - must have BattleCharacter]
Enemy: [Enemy GameObject - must have BattleCharacter]
```

### PlayerBattleController
```
Animation Sequencer: [assigned via code by BattleManager]
QTE Manager: [assigned via code by BattleManager]
Attack Animation Name: "LightAttack"
Parry Animation Name: "Parry"
Perfect Parry Animation Name: "ParryPerfect"
```

### EnemyBattleController
```
Parry System: [ParrySystem GameObject]
Attack Animation Name: "EnemyAttack"
```

---

## 🎮 Default Controls

| Action | Keyboard | Xbox | PlayStation | Switch |
|--------|----------|------|-------------|--------|
| QTE/Parry | Space | A | Cross | B |

---

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| Input not working | Check Input Action assigned in Inspector |
| No QTE window | Check AnimationSequencer assigned in BattleManager |
| Parry not working | Check ParrySystem assigned in EnemyBattleController |
| Animation not playing | Check animation name matches Animator state name |
| UI not updating | Check event connections in BattleManager Inspector |

---

## 📂 Key Files

```
Assets/
├── Input/
│   └── CombatInputActions.inputactions  [YOU CREATE THIS]
├── Scripts/
│   └── Batalla/
│       ├── BattleManagerV2.cs
│       ├── Controllers/
│       │   ├── PlayerBattleController.cs
│       │   └── EnemyBattleController.cs
│       ├── Systems/
│       │   ├── AnimationSequencer.cs
│       │   ├── QTEManager.cs          [UPDATED: Input System]
│       │   └── ParrySystem.cs         [UPDATED: Input System]
│       └── Data/
│           ├── AttackAnimationData.cs
│           └── SkillData.cs
└── ScriptableObjects/
    └── AttackData/
        └── PlayerLightAttack.asset    [YOU CREATE THIS]
```

---

## 🔗 Quick Links

- **Full Input System Guide:** INPUT_SYSTEM_SETUP.md
- **Complete Scene Setup:** COMPLETE_SETUP_GUIDE.md  
- **Animation Configuration:** ANIMATION_SETUP_GUIDE.md
- **Parry System Details:** PARRY_ANIMATION_GUIDE.md
- **Migration Notes:** INPUT_SYSTEM_MIGRATION.md

---

## 🎯 Testing Flow

1. **Setup Scene** (10 min)
   - Create hierarchy
   - Add components
   - Assign references

2. **Create Input Actions** (5 min)
   - CombatInputActions asset
   - QTE and Parry actions
   - Assign in Inspector

3. **Create Attack Data** (5 min)
   - Right-click → Create → Batalla → Attack Animation Data
   - Configure timings

4. **Setup Animations** (15 min)
   - Create Animator states
   - Configure animation names
   - Set normalized times

5. **Test** (5 min)
   - Enter Play Mode
   - Trigger attack
   - Test QTE
   - Test parry

**Total Time:** ~40 minutes for complete setup

---

## 💡 Pro Tips

1. **Start Simple:** Get one attack working before adding complexity
2. **Use Debug Logs:** Console shows QTE/Parry windows - watch for them
3. **Test Incrementally:** Test after each major setup step
4. **Timing Tweaking:** Adjust window durations based on feel, not theory
5. **Multiple Bindings:** Add Space + E for easier testing

---

## 🚀 Next Steps After Setup

1. Create more attack animations
2. Add combo system
3. Implement skill variations
4. Add visual effects on perfect timing
5. Create UI prompts for button presses
6. Add sound effects
7. Implement camera shake on impacts
8. Add particle effects

---

## 📞 Support

If something isn't working:

1. Check all Inspector assignments (most common issue)
2. Look for errors in Console
3. Verify Input Actions are saved
4. Check animation state names match code
5. Review COMPLETE_SETUP_GUIDE.md Section 9 (Troubleshooting)

---

**Good luck! Your combat system awaits! ⚔️**
