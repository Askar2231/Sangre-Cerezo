# Input Icon Mapper Architecture

## System Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    INPUT ICON MAPPER SYSTEM                      │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────┐
│   Unity Inspector    │
│                      │
│  Sprite Name Maps:   │
│  ┌────────────────┐  │
│  │ Action: Run    │  │
│  │ KB: "Shift"    │  │
│  │ XB: "Xbox_R3"  │  │
│  │ PS: "PS_R3"    │  │
│  └────────────────┘  │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────────────┐
│                   InputIconMapper.cs                              │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  1. Device Detection                                              │
│     ┌──────────┐  ┌──────────┐  ┌──────────┐                    │
│     │ Keyboard │  │   Xbox   │  │PlayStation│                    │
│     └────┬─────┘  └────┬─────┘  └────┬─────┘                    │
│          └─────────────┼─────────────┘                           │
│                        ▼                                          │
│            currentDeviceType = Keyboard/Xbox/PS                   │
│                                                                   │
│  2. Sprite Name Selection                                         │
│     Dictionary<InputAction, string> currentSpriteNameSet          │
│                                                                   │
│     InputAction.Run  →  "Shift"     (if Keyboard)                │
│     InputAction.Run  →  "Xbox_R3"   (if Xbox)                    │
│     InputAction.Run  →  "PS_R3"     (if PlayStation)             │
│                                                                   │
│  3. Text Processing                                               │
│     "Press {Run}" → "Press <sprite name='Shift'>"                │
│                                                                   │
└────────────────────────────┬─────────────────────────────────────┘
                             │
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                    TextMeshPro Component                          │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  TMP Sprite Asset:                                                │
│  ┌─────────────────────────────────────────┐                     │
│  │ "Shift"    → [Shift Key Icon Image]     │                     │
│  │ "Xbox_R3"  → [Xbox R3 Icon Image]       │                     │
│  │ "PS_R3"    → [PS R3 Icon Image]         │                     │
│  └─────────────────────────────────────────┘                     │
│                                                                   │
│  Rendered Text:                                                   │
│  "Press [🎮] to sprint"  ← Icon shown based on device           │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

## Data Flow Example

### Example: User Presses Shift Key

```
1. INPUT DETECTION
   ┌─────────────┐
   │ Shift Key   │ ──┐
   │  Pressed    │   │
   └─────────────┘   │
                     ▼
          InputIconMapper.Update()
                     │
                     ▼
         SetDeviceType(Keyboard)
                     │
                     ▼
         UpdateCurrentIconSet()
                     │
                     ▼
    currentSpriteNameSet[Run] = "Shift"


2. TEXT PROCESSING
   Input:  "Hold {Run} to sprint"
           │
           ▼
   ProcessTextPlaceholders()
           │
           ├─ Find {Run}
           ├─ Get sprite name for InputAction.Run
           ├─ currentSpriteNameSet[Run] = "Shift"
           └─ Replace with <sprite name="Shift">
           │
           ▼
   Output: "Hold <sprite name=\"Shift\"> to sprint"


3. RENDERING
   TextMeshPro receives:
   "Hold <sprite name=\"Shift\"> to sprint"
           │
           ▼
   Looks up "Shift" in TMP Sprite Asset
           │
           ▼
   Finds sprite image for Shift key
           │
           ▼
   Renders: "Hold [⇧] to sprint"
```

## Class Structure

```
InputIconMapper (MonoBehaviour, Singleton)
│
├─ Fields
│  ├─ currentDeviceType: InputDeviceType
│  ├─ currentSpriteNameSet: Dictionary<InputAction, string>
│  └─ spriteNameMappings: List<InputActionSpriteMap>
│
├─ Methods
│  ├─ GetSpriteNameForAction(InputAction) → string
│  ├─ ProcessTextPlaceholders(string) → string
│  ├─ GetCurrentDeviceType() → InputDeviceType
│  ├─ GetTextForAction(InputAction) → string
│  └─ GetIconForQTE() → string
│
└─ Events
   └─ OnDeviceChanged(InputDeviceType)

InputActionSpriteMap (Serializable Class)
│
└─ Fields
   ├─ action: InputAction
   ├─ keyboardSpriteName: string
   ├─ xboxSpriteName: string
   └─ playStationSpriteName: string
```

## Configuration Workflow

```
STEP 1: Define Sprite Names in Inspector
┌────────────────────────────────────────┐
│ InputIconMapper GameObject             │
│                                        │
│ Sprite Name Mappings (List)            │
│ ┌────────────────────────────────────┐ │
│ │ [0] Move                           │ │
│ │     KB: "WASD"                     │ │
│ │     XB: "Xbox_LS"                  │ │
│ │     PS: "PS_LS"                    │ │
│ ├────────────────────────────────────┤ │
│ │ [1] Run                            │ │
│ │     KB: "Shift"                    │ │
│ │     XB: "Xbox_R3"                  │ │
│ │     PS: "PS_R3"                    │ │
│ └────────────────────────────────────┘ │
└────────────────────────────────────────┘
           │
           ▼
STEP 2: Create TMP Sprite Asset
┌────────────────────────────────────────┐
│ Window → TextMeshPro → Sprite Importer│
│                                        │
│ Import sprites with matching names:   │
│ • Shift.png       → "Shift"           │
│ • Xbox_R3.png     → "Xbox_R3"         │
│ • PS_R3.png       → "PS_R3"           │
│ • ...                                 │
└────────────────────────────────────────┘
           │
           ▼
STEP 3: Assign to TextMeshPro
┌────────────────────────────────────────┐
│ TextMeshProUGUI Component              │
│                                        │
│ Extra Settings                         │
│   Sprite Asset: [InputIcons]          │
│                                        │
│ Text: Use ProcessTextPlaceholders()   │
└────────────────────────────────────────┘
```

## Runtime Behavior

```
┌─────────────────────────────────────────┐
│        DEVICE CHANGE DETECTION          │
└─────────────────────────────────────────┘

Keyboard Input             Gamepad Input
     │                          │
     ▼                          ▼
Any key pressed?          Any button pressed?
     │                          │
     │ YES                      │ YES
     ▼                          ▼
currentDeviceType        Detect gamepad type:
  = Keyboard            • "dualshock" → PlayStation
                        • "xbox" → Xbox
                        • other → Generic (uses Xbox)
     │                          │
     └──────────┬───────────────┘
                ▼
      UpdateCurrentIconSet()
                │
                ▼
      OnDeviceChanged event fired
                │
                ▼
        UI automatically updates
```

## Integration Points

```
┌───────────────────────────────────────────────────────────┐
│                  YOUR GAME SYSTEMS                         │
└───────────────────────────────────────────────────────────┘

Tutorial System          Battle UI           QTE System
     │                       │                    │
     ├─ Get action text      ├─ Show controls    ├─ Get QTE button
     │                       │                    │
     └───────────┬───────────┴────────────────────┘
                 ▼
         InputIconMapper.Instance
                 │
                 ├─ GetSpriteNameForAction()
                 ├─ ProcessTextPlaceholders()
                 └─ GetIconForQTE()
                 │
                 ▼
        Returns sprite tags or names
                 │
                 ▼
         TextMeshPro displays icons
```

## Benefits of This Architecture

✅ **Separation of Concerns**

- Sprite names defined in Inspector
- Icons stored in TMP Sprite Asset
- Logic handled by InputIconMapper

✅ **Device-Agnostic**

- Single text placeholder: "{Run}"
- Automatically shows correct icon per device

✅ **Easy Updates**

- Change sprite? Just update TMP asset
- Change mapping? Just update Inspector
- No code changes needed

✅ **Runtime Flexibility**

- Detect device changes automatically
- Switch icons on-the-fly
- Subscribe to device change events

✅ **Designer-Friendly**

- All configuration in Inspector
- No coding required for setup
- Visual sprite assignment
