# Sistema de Tutoriales - Sangre y Cerezo
## Guía Completa de Configuración y Uso

---

## 📋 Resumen

Sistema completo de tutoriales para enseñar mecánicas de movimiento y combate. Incluye diálogos simples y tarjetas emergentes con soporte para iconos dinámicos según el dispositivo de entrada (teclado/gamepad).

**Compatible con:** MovimientoV2 y BattleManagerV2

---

## 📑 Índice
1. [Componentes Implementados](#componentes-implementados)
2. [Características Principales](#características-principales)
3. [Quick Start - Configuración Mínima](#quick-start---configuración-mínima)
4. [Configuración Detallada](#configuración-detallada)
5. [Progresión de Tutoriales](#progresión-de-tutoriales)
6. [API y Uso](#api-y-uso)
7. [Debug y Testing](#debug-y-testing)
8. [Personalización](#personalización)
9. [Troubleshooting](#troubleshooting)
10. [Extensiones Futuras](#extensiones-futuras)

---

## ✅ Componentes Implementados

### 📦 Core System
- **TutorialEnums.cs** - Enumeraciones (TutorialType, DialogPosition, InputDeviceType, InputAction)
- **TutorialCard.cs** - Clase para tarjetas individuales de tutorial
- **TutorialData.cs** - ScriptableObject para definir tutoriales
- **TutorialManager.cs** - Gestor central singleton con persistencia PlayerPrefs

### 🎮 Input System
- **InputIconMapper.cs** - Detecta dispositivos y mapea acciones a iconos/texto
- **DynamicInputIcon.cs** - Componente para actualizar iconos en TextMeshPro automáticamente

### 🎨 UI Presenters
- **SimpleDialogPresenter.cs** - Presenta diálogos simples en la parte superior
- **CardPopoverPresenter.cs** - Presenta tarjetas emergentes con imágenes y secuencias

### 🎯 Trigger System
- **TutorialTrigger.cs** - Clase base abstracta para triggers
- **TutorialTriggerZone.cs** - Trigger por colisión/interacción física
- **BattleTutorialTrigger.cs** - Trigger basado en eventos de batalla

### 🛠️ Editor Tools
- **TutorialDataEditor.cs** - Inspector personalizado para TutorialData con validación

---

## 📚 Características Principales

### ✨ Tipos de Tutorial

**1. SimpleDialog**: Diálogo en la parte superior de la pantalla
- No intrusivo
- Ideal para tutoriales de movimiento
- Auto-cierre o manual
- Posiciones configurables (TopLeft, TopCenter, TopRight)

**2. CardPopover**: Tarjetas emergentes con contenido rico
- Múltiples tarjetas en secuencia
- Soporte para imágenes
- Pausa el juego
- Indicadores de progreso (números y puntos)
- Navegación con {Confirm} o skip con {Cancel}

### 🎮 Sistema de Input Dinámico
- **Detección automática** de dispositivo (Keyboard, Xbox, PlayStation)
- **Placeholders** en texto: `{Move}`, `{Run}`, `{QTE}`, `{Parry}`, etc.
- **Actualización en tiempo real** al cambiar dispositivo
- Soporte para iconos sprite o texto fallback

### 💾 Persistencia
- Guarda tutoriales completados en **PlayerPrefs**
- Opción de "mostrar solo una vez"
- Función de reseteo para testing
- Estado global de tutoriales habilitados/deshabilitados

### 🎯 Sistema de Triggers

#### TutorialTriggerZone
- Basado en colisión (Trigger Collider)
- Auto-trigger o requiere interacción (presionar E/A)
- Visualización con Gizmos en editor
- Radio de interacción configurable

#### BattleTutorialTrigger
Eventos soportados:
- `OnBattleStart` - Inicio de batalla
- `OnFirstPlayerTurn` - Primer turno del jugador
- `OnFirstAttack` - Primer ataque
- `OnFirstQTEWindow` - Primera ventana QTE
- `OnFirstEnemyTurn` - Primer turno enemigo
- `OnFirstParryWindow` - Primera ventana de parry
- `OnStaminaLow` - Resistencia baja (configurable)
- `OnFirstSkillUse` - Primer uso de habilidad
- `OnBattleVictory` - Victoria
- `OnBattleLoss` - Derrota

### 🎨 Animaciones
- **Fade In/Out** para todos los elementos
- Transiciones suaves entre tarjetas
- Duración configurable
- Usa `Time.unscaledDeltaTime` (funciona con pausa)

---

## 🚀 Quick Start - Configuración Mínima

### ⏱️ Tiempo Estimado: 1-2 horas

Este quick start te permitirá tener un sistema funcional con 3 tutoriales básicos.

### 1️⃣ Configuración en Escena (15-20 min)

#### Paso 1: Crear TutorialManager
- [ ] Crear GameObject vacío llamado "TutorialManager" en la escena
- [ ] Agregar componente `TutorialManager`
- [ ] Agregar componente `InputIconMapper`
- [ ] Verificar que se marquen como DontDestroyOnLoad automáticamente

#### Paso 2: Crear UI Canvas
- [ ] Crear Canvas (si no existe) → Render Mode: Screen Space - Overlay
- [ ] Nombrar "TutorialCanvas"

#### Paso 3: Crear SimpleDialog UI
```
Crear estructura:
TutorialCanvas/
└── SimpleDialog (GameObject)
    ├── Componente: SimpleDialogPresenter
    └── Hijo: DialogContainer (Image - Panel)
        ├── Componente: CanvasGroup
        └── Hijo: DialogText (TextMeshProUGUI)
```

**Configurar DialogContainer:**
- [ ] Anchors: Top-Center
- [ ] Width: 800, Height: 100
- [ ] Pos Y: -50
- [ ] Color: Negro semi-transparente (A: 0.8)
- [ ] CanvasGroup → Alpha: 0

**Configurar DialogText:**
- [ ] Anchors: Stretch All
- [ ] Margins: 20 en todos lados
- [ ] Font Size: 24
- [ ] Alignment: Center
- [ ] Color: Blanco

**Crear Anchors (GameObjects vacíos con RectTransform):**
- [ ] TopLeftAnchor → Anchors: Top-Left, Position: (50, -50)
- [ ] TopCenterAnchor → Anchors: Top-Center, Position: (0, -50)
- [ ] TopRightAnchor → Anchors: Top-Right, Position: (-50, -50)

**Asignar en SimpleDialogPresenter:**
- [ ] Dialog Container → DialogContainer
- [ ] Dialog Text → DialogText
- [ ] Canvas Group → CanvasGroup (del DialogContainer)
- [ ] Top Left/Center/Right Anchors

#### Paso 4: Crear CardPopover UI
```
TutorialCanvas/
└── CardPopover (GameObject)
    ├── Componente: CardPopoverPresenter
    └── Hijo: CardContainer (Image - Panel oscuro)
        ├── Componente: CanvasGroup
        ├── Hijo: CardImage (Image)
        ├── Hijo: CardText (TextMeshProUGUI)
        ├── Hijo: ConfirmPrompt (GameObject)
        │   └── Hijo: ConfirmText (TextMeshProUGUI)
        ├── Hijo: PageIndicator (TextMeshProUGUI)
        └── Hijo: DotIndicatorContainer (GameObject)
            └── Componente: HorizontalLayoutGroup
```

**Configurar CardContainer:**
- [ ] Anchors: Center
- [ ] Width: 600, Height: 500
- [ ] Color: Negro oscuro (A: 0.95)
- [ ] CanvasGroup → Alpha: 0

**Configurar CardImage:**
- [ ] Anchors: Top-Center
- [ ] Width: 560, Height: 280
- [ ] Pos Y: -20

**Configurar CardText:**
- [ ] Anchors: Stretch (con márgenes)
- [ ] Top: -310, Bottom: 100, Left: 20, Right: 20
- [ ] Font Size: 20
- [ ] Alignment: Top-Left
- [ ] Enable Word Wrapping

**Configurar ConfirmPrompt:**
- [ ] Anchors: Bottom-Center
- [ ] Pos Y: 50
- [ ] Desactivar por defecto

**Configurar PageIndicator:**
- [ ] Anchors: Bottom-Center
- [ ] Pos Y: 20
- [ ] Font Size: 16

**Configurar DotIndicatorContainer:**
- [ ] Anchors: Bottom-Center
- [ ] Pos Y: -10
- [ ] Spacing: 10

**Crear DotPrefab:**
- [ ] Crear Image pequeña (Width: 10, Height: 10)
- [ ] Shape: Circle (o sprite circular)
- [ ] Convertir a Prefab
- [ ] Asignar en CardPopoverPresenter

#### Paso 5: Conectar Referencias en TutorialManager
- [ ] TutorialManager → Simple Dialog Presenter
- [ ] TutorialManager → Card Popover Presenter
- [ ] Activar Debug Mode (opcional, para testing)

---

### 2️⃣ Configurar Input Icons (20-30 min)

#### Paso 1: Importar/Crear Sprites
Necesitas sprites para:

**Keyboard:**
- [ ] WASD o flechas (Move)
- [ ] Shift (Run)
- [ ] E (Interact)
- [ ] Espacio (QTE/Parry)
- [ ] Enter (Confirm)
- [ ] ESC (Cancel)

**Xbox:**
- [ ] Left Stick (Move)
- [ ] LT (Run)
- [ ] A (Interact/QTE/Parry/Confirm)
- [ ] B (Cancel)

**PlayStation:**
- [ ] Left Stick (Move)
- [ ] L2 (Run)
- [ ] Cruz/X (Interact/QTE/Parry/Confirm)
- [ ] Círculo (Cancel)

**Recursos Gratuitos:**
- Kenney.nl (Input Prompts)
- Itch.io
- Game-icons.net

#### Paso 2: Asignar en InputIconMapper
- [ ] Seleccionar TutorialManager → InputIconMapper component
- [ ] Asignar todos los sprites en las secciones correspondientes
- [ ] Activar Debug Mode si quieres ver cambios de dispositivo

---

### 3️⃣ Crear TutorialData Assets (30-40 min)

#### Crear Carpeta
- [ ] Crear: `Assets/Data/Tutorials/`

#### Tutoriales Mínimos para Empezar (3 básicos)

**Tutorial 1: Movement Basic**
- [ ] Create → Tutorial System → Tutorial Data
- [ ] Nombrar: `Tutorial_MovementBasic`
- [ ] Tutorial ID: `movement_basic`
- [ ] Tipo: SimpleDialog
- [ ] Texto: `Usa {Move} para moverte. Tu personaje siempre se mueve hacia adelante.`
- [ ] Pause Game: NO
- [ ] Display Duration: 5.0

**Tutorial 2: Sprint**
- [ ] Crear `Tutorial_Sprint`
- [ ] Tutorial ID: `movement_sprint`
- [ ] Tipo: SimpleDialog
- [ ] Texto: `Mantén {Run} mientras te mueves para correr más rápido.`
- [ ] Pause Game: NO
- [ ] Display Duration: 5.0

**Tutorial 3: Battle Intro (ejemplo de tarjetas)**
- [ ] Crear `Tutorial_BattleIntro`
- [ ] Tutorial ID: `battle_intro`
- [ ] Tipo: CardPopover
- [ ] Pause Game: SÍ
- [ ] Agregar 2 tarjetas:
  - Tarjeta 1: "¡Combate iniciado! Este es un sistema por turnos."
  - Tarjeta 2: "Puedes realizar múltiples acciones hasta que se agote tu resistencia."

---

### 4️⃣ Colocar Triggers (10-15 min)

#### Trigger de Movimiento Básico
- [ ] Crear GameObject: "Trigger_MovementBasic"
- [ ] Add Component: Box Collider → Is Trigger: ✓
- [ ] Size: (5, 3, 5) - ajustar según necesidad
- [ ] Add Component: TutorialTriggerZone
- [ ] Asignar Tutorial Data: Tutorial_MovementBasic
- [ ] Require Interaction: NO
- [ ] Trigger Once: SÍ
- [ ] Posicionar cerca del spawn del jugador

#### Trigger de Sprint
- [ ] Similar al anterior
- [ ] Asignar Tutorial_Sprint
- [ ] Posicionar en un camino largo

#### Trigger de Batalla (en BattleManager scene)
- [ ] Crear GameObject hijo de BattleManager: "Trigger_BattleIntro"
- [ ] Add Component: BattleTutorialTrigger
- [ ] Trigger Type: OnBattleStart
- [ ] Battle Manager: Asignar BattleManagerV2
- [ ] Tutorial Data: Tutorial_BattleIntro
- [ ] Trigger Once: SÍ

---

### 5️⃣ Testing (5-10 min)

#### Test Básico
- [ ] Play Mode
- [ ] Caminar hacia trigger de Movement Basic
- [ ] Verificar que aparezca diálogo arriba
- [ ] Verificar que el texto tenga iconos procesados
- [ ] Esperar 5 segundos o presionar Enter
- [ ] Verificar que desaparezca

#### Test de Dispositivos
- [ ] Presionar tecla de teclado → ver icono de teclado
- [ ] Conectar gamepad y presionar botón → ver iconos de gamepad
- [ ] Verificar que cambien en tiempo real

#### Test de Batalla
- [ ] Iniciar batalla
- [ ] Verificar que aparezca tutorial de batalla (tarjetas)
- [ ] Verificar que pause el juego
- [ ] Navegar tarjetas con Enter/A
- [ ] Verificar que se cierre al final

#### Test de Persistencia
- [ ] Completar un tutorial
- [ ] Salir de Play Mode
- [ ] Volver a Play Mode
- [ ] Verificar que NO se muestre de nuevo
- [ ] Llamar `TutorialManager.Instance.ResetAllTutorials()`
- [ ] Verificar que se muestre de nuevo

### ✅ ¡Mínimo Viable Completado!

Con estos 5 pasos tienes:
- ✅ 3 tutoriales funcionando
- ✅ Iconos dinámicos
- ✅ Persistencia
- ✅ Triggers automáticos y de batalla

---

## 🔧 Configuración Detallada

### Crear TutorialData ScriptableObjects

#### Ubicación
Crear carpeta: `Assets/Data/Tutorials/`

#### Proceso
1. Click derecho en Project → Create → Tutorial System → Tutorial Data
2. Nombrar el asset según convención: `Tutorial_[nombre]`
3. Configurar campos en el Inspector

### Estructura de UI Detallada

#### Simple Dialog UI:
```
Canvas (TutorialCanvas)
└── SimpleDialog (GameObject + SimpleDialogPresenter)
    ├── DialogContainer (Panel con CanvasGroup)
    │   └── DialogText (TextMeshProUGUI)
    ├── TopLeftAnchor (RectTransform vacío)
    ├── TopCenterAnchor (RectTransform vacío)
    └── TopRightAnchor (RectTransform vacío)
```

#### Card Popover UI:
```
Canvas (TutorialCanvas)
└── CardPopover (GameObject + CardPopoverPresenter)
    └── CardContainer (Panel con CanvasGroup)
        ├── CardImage (Image)
        ├── CardText (TextMeshProUGUI)
        ├── ConfirmPrompt (GameObject)
        │   └── ConfirmText (TextMeshProUGUI)
        ├── PageIndicator (TextMeshProUGUI)
        └── DotIndicatorContainer (HorizontalLayoutGroup)
            └── DotPrefab (Image - crear como prefab)
```

### 3. Asignar Referencias en TutorialManager
- Asignar SimpleDialogPresenter
- Asignar CardPopoverPresenter
- Activar Debug Mode si es necesario

### 4. Configurar InputIconMapper
Asignar sprites para cada tipo de input:
- Teclado: WASD, Shift, Space, E, Enter, ESC
- Xbox: Stick Izq, LT, A, B, etc.
- PlayStation: Stick Izq, L2, Cruz, Círculo, etc.

---

## Crear TutorialData ScriptableObjects

### Ubicación
Crear carpeta: `Assets/Data/Tutorials/`

### Proceso
1. Click derecho en Project → Create → Tutorial System → Tutorial Data
2. Nombrar el asset según convención: `Tutorial_[nombre]`
3. Configurar campos

---

## Progresión de Tutoriales

### Fase 1: Exploración Básica

#### 1. Tutorial_MovementBasic
**Tipo:** SimpleDialog  
**ID:** `movement_basic`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✗
- Block Input: ✗
- Dialog Position: TopCenter
- Display Duration: 5.0

**Texto:**
```
Usa {Move} para moverte. Tu personaje siempre se mueve hacia adelante en la dirección que mira.
```

---

#### 2. Tutorial_Sprint
**Tipo:** SimpleDialog  
**ID:** `movement_sprint`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✗
- Block Input: ✗
- Dialog Position: TopCenter
- Display Duration: 5.0

**Texto:**
```
Mantén presionado {Run} mientras te mueves para correr más rápido.
```

---

#### 3. Tutorial_CameraControl
**Tipo:** SimpleDialog  
**ID:** `camera_control`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✗
- Block Input: ✗
- Dialog Position: TopCenter
- Display Duration: 5.0

**Texto:**
```
Usa el ratón o el stick derecho para controlar la cámara y mirar a tu alrededor.
```

---

### Fase 2: Introducción a la Batalla

#### 4. Tutorial_BattleIntro
**Tipo:** CardPopover  
**ID:** `battle_intro`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Imagen de combate por turnos]
- Text: `¡Ha comenzado el combate! Este es un sistema de combate por turnos. Tú y el enemigo se alternarán.`
- Min Display Time: 2.0
- Require Confirmation: ✓

**Tarjeta 2:**
- Image: [Diagrama de turnos]
- Text: `Durante tu turno, puedes realizar múltiples acciones hasta que se agote tu resistencia. ¡Usa tus acciones sabiamente!`
- Min Display Time: 2.0
- Require Confirmation: ✓

---

#### 5. Tutorial_BattleStamina
**Tipo:** CardPopover  
**ID:** `battle_stamina`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Barra de resistencia]
- Text: `Esta es tu barra de resistencia. Se restaura completamente al inicio de cada turno.`
- Min Display Time: 2.0
- Require Confirmation: ✓

**Tarjeta 2:**
- Image: [Acciones y costos]
- Text: `Cada acción consume resistencia. Cuando esté muy baja para realizar acciones, termina tu turno.`
- Min Display Time: 2.0
- Require Confirmation: ✓

---

#### 6. Tutorial_BattleAttack
**Tipo:** CardPopover  
**ID:** `battle_attack`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Botón de ataque]
- Text: `Selecciona la acción de Ataque para golpear al enemigo. Los ataques tienen diferentes velocidades y daño.`
- Min Display Time: 2.0
- Require Confirmation: ✓

---

#### 7. Tutorial_QTEIntroduction
**Tipo:** CardPopover  
**ID:** `qte_introduction`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Destello QTE]
- Text: `Durante tus ataques, ¡observa los destellos brillantes! Estos son Eventos de Tiempo Rápido (QTE).`
- Min Display Time: 2.0
- Require Confirmation: ✓

**Tarjeta 2:**
- Image: [Diagrama de timing]
- Text: `Presiona {QTE} cuando aparezca el destello. ¡El timing perfecto aumenta tu daño!`
- Min Display Time: 2.0
- Require Confirmation: ✓

**Tarjeta 3:**
- Image: [Bonus de resistencia]
- Text: `Los QTE exitosos también restauran un poco de resistencia. ¡Inténtalo ahora!`
- Min Display Time: 2.0
- Require Confirmation: ✓

---

### Fase 3: Mecánicas Defensivas

#### 8. Tutorial_EnemyTurnIntro
**Tipo:** CardPopover  
**ID:** `enemy_turn_intro`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Turno enemigo]
- Text: `¡Es el turno del enemigo! Ahora atacará. Prepárate para defenderte.`
- Min Display Time: 2.0
- Require Confirmation: ✓

---

#### 9. Tutorial_ParryIntroduction
**Tipo:** CardPopover  
**ID:** `parry_introduction`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Destello rojo de ataque]
- Text: `¡El enemigo está atacando! Observa el indicador de destello rojo.`
- Min Display Time: 2.0
- Require Confirmation: ✓

**Tarjeta 2:**
- Image: [Ventana de parry]
- Text: `Presiona {Parry} JUSTO cuando aparezca el destello para bloquear el ataque. ¡Parry perfecto = sin daño + bonus de resistencia!`
- Min Display Time: 2.5
- Require Confirmation: ✓

---

#### 10. Tutorial_ParryPractice
**Tipo:** SimpleDialog  
**ID:** `parry_practice`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✗
- Block Input: ✗
- Dialog Position: TopCenter
- Display Duration: 4.0

**Texto:**
```
Sigue practicando el parry. El timing es crucial para dominar el combate.
```

---

### Fase 4: Combate Avanzado

#### 11. Tutorial_MultipleActions
**Tipo:** CardPopover  
**ID:** `multiple_actions`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Múltiples ataques]
- Text: `Puedes realizar múltiples ataques en un solo turno si tienes suficiente resistencia. ¡Encadena tus ataques!`
- Min Display Time: 2.0
- Require Confirmation: ✓

**Tarjeta 2:**
- Image: [Estrategia]
- Text: `Decide cuándo atacar y cuándo terminar tu turno. Guarda resistencia para los QTE.`
- Min Display Time: 2.0
- Require Confirmation: ✓

---

#### 12. Tutorial_SkillIntroduction
**Tipo:** CardPopover  
**ID:** `skill_introduction`  
**Configuración:**
- Show Only Once: ✓
- Pause Game: ✓
- Block Input: ✓

**Tarjetas:**

**Tarjeta 1:**
- Image: [Botón de habilidad]
- Text: `¡Has desbloqueado una habilidad especial! Las habilidades son más poderosas pero consumen más resistencia.`
- Min Display Time: 2.0
- Require Confirmation: ✓

**Tarjeta 2:**
- Image: [Efectos de habilidad]
- Text: `Algunas habilidades causan daño, otras curan o aplican efectos especiales. ¡Úsalas estratégicamente!`
- Min Display Time: 2.0
- Require Confirmation: ✓

---

## 🎮 API y Uso

### Crear un Tutorial Simple (Código)
```csharp
// Opción 1: Asignar TutorialData desde Inspector y disparar
public TutorialData myTutorial;

void Start() {
    TutorialManager.Instance.TriggerTutorial(myTutorial);
}

// Opción 2: Cargar dinámicamente
TutorialData tutorial = Resources.Load<TutorialData>("Tutorials/Tutorial_MovementBasic");
TutorialManager.Instance.TriggerTutorial(tutorial);
```

### Crear Trigger de Zona (Código)
```csharp
// En cualquier MonoBehaviour
public class MyTrigger : MonoBehaviour
{
    [SerializeField] private TutorialData tutorialToShow;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.TriggerTutorial(tutorialToShow);
        }
    }
}
```

### API del TutorialManager

```csharp
// Disparar tutorial
TutorialManager.Instance.TriggerTutorial(tutorialData);

// Verificar si tutorial completado
bool completed = TutorialManager.Instance.HasCompletedTutorial("tutorial_id");

// Marcar tutorial como completado manualmente
TutorialManager.Instance.MarkTutorialComplete("tutorial_id");

// Resetear todos los tutoriales (testing)
TutorialManager.Instance.ResetAllTutorials();

// Habilitar/Deshabilitar sistema completo
TutorialManager.Instance.SetTutorialsEnabled(false);

// Verificar si están habilitados
bool enabled = TutorialManager.Instance.AreTutorialsEnabled();

// Debug: Log estado actual
TutorialManager.Instance.DebugLogTutorialStatus();
```

### Eventos del TutorialManager

```csharp
// Suscribirse a eventos
void OnEnable()
{
    TutorialManager.Instance.OnTutorialStarted.AddListener(HandleTutorialStarted);
    TutorialManager.Instance.OnTutorialCompleted.AddListener(HandleTutorialCompleted);
}

void OnDisable()
{
    TutorialManager.Instance.OnTutorialStarted.RemoveListener(HandleTutorialStarted);
    TutorialManager.Instance.OnTutorialCompleted.RemoveListener(HandleTutorialCompleted);
}

void HandleTutorialStarted(string tutorialId)
{
    Debug.Log($"Tutorial iniciado: {tutorialId}");
}

void HandleTutorialCompleted(string tutorialId)
{
    Debug.Log($"Tutorial completado: {tutorialId}");
    // Desbloquear siguiente nivel, dar recompensa, etc.
}
```

### Integración con Sistemas Existentes

#### Con MovimientoV2
```csharp
// Los tutoriales de movimiento NO pausan el juego
// El jugador puede seguir moviéndose mientras lee
// Configurar en TutorialData: pauseGame = false
```

#### Con BattleManagerV2
```csharp
// Los tutoriales de batalla SÍ pausan el juego
// Se suscriben automáticamente a eventos del sistema
// Configurar en TutorialData: pauseGame = true

// Ejemplo de uso en batalla:
public class MyBattleScript : MonoBehaviour
{
    void Start()
    {
        // Los BattleTutorialTrigger se encargan automáticamente
        // Solo necesitas configurarlos en el Inspector
    }
}
```

---

## Configurar Triggers

### Triggers de Zona (Exploración)

#### Trigger para Movement Basic
1. Crear GameObject con BoxCollider (trigger)
2. Agregar componente `TutorialTriggerZone`
3. Configurar:
   - Tutorial Data: Tutorial_MovementBasic
   - Trigger Once: ✓
   - Require Interaction: ✗ (auto-trigger)
   - Player Tag: "Player"
4. Posicionar cerca del spawn del jugador

#### Trigger para Sprint
1. Crear GameObject con BoxCollider (trigger)
2. Agregar componente `TutorialTriggerZone`
3. Configurar:
   - Tutorial Data: Tutorial_Sprint
   - Trigger Once: ✓
   - Require Interaction: ✗
   - Player Tag: "Player"
4. Posicionar en un camino largo

#### Trigger para Camera Control (Nota Interactuable)
1. Crear GameObject (nota en el suelo)
2. Agregar BoxCollider (trigger)
3. Agregar componente `TutorialTriggerZone`
4. Configurar:
   - Tutorial Data: Tutorial_CameraControl
   - Trigger Once: ✓
   - Require Interaction: ✓ (presionar E)
   - Player Tag: "Player"
   - Interaction Distance: 3.0
5. Agregar modelo 3D de nota o placa

---

### Triggers de Batalla

#### En el GameObject del BattleManager, crear hijos:

#### Trigger_BattleIntro
1. Crear GameObject hijo de BattleManager
2. Agregar componente `BattleTutorialTrigger`
3. Configurar:
   - Tutorial Data: Tutorial_BattleIntro
   - Trigger Type: OnBattleStart
   - Battle Manager: (asignar BattleManagerV2)
   - Trigger Once: ✓

#### Trigger_BattleStamina
1. Crear GameObject hijo de BattleManager
2. Agregar componente `BattleTutorialTrigger`
3. Configurar:
   - Tutorial Data: Tutorial_BattleStamina
   - Trigger Type: OnFirstPlayerTurn
   - Battle Manager: (asignar)
   - Trigger Once: ✓

#### Trigger_BattleAttack
1. Similar al anterior
2. Trigger Type: OnFirstPlayerTurn (se muestra después de stamina)
3. Tutorial Data: Tutorial_BattleAttack

#### Trigger_QTE
1. Crear GameObject hijo de BattleManager
2. Agregar componente `BattleTutorialTrigger`
3. Configurar:
   - Tutorial Data: Tutorial_QTEIntroduction
   - Trigger Type: OnFirstQTEWindow
   - Battle Manager: (asignar)
   - Trigger Once: ✓

#### Trigger_EnemyTurn
1. Crear GameObject hijo de BattleManager
2. Agregar componente `BattleTutorialTrigger`
3. Configurar:
   - Tutorial Data: Tutorial_EnemyTurnIntro
   - Trigger Type: OnFirstEnemyTurn
   - Battle Manager: (asignar)
   - Trigger Once: ✓

#### Trigger_Parry
1. Crear GameObject hijo de BattleManager
2. Agregar componente `BattleTutorialTrigger`
3. Configurar:
   - Tutorial Data: Tutorial_ParryIntroduction
   - Trigger Type: OnFirstParryWindow
   - Battle Manager: (asignar)
   - Trigger Once: ✓

#### Trigger_MultipleActions
1. Similar, pero:
2. Trigger Type: OnFirstPlayerTurn
3. Trigger Once: ✗
4. Agregar lógica para disparar solo en el segundo turno del jugador

#### Trigger_Skill
1. Trigger Type: OnFirstSkillUse
2. Tutorial Data: Tutorial_SkillIntroduction

---

## Configuración de Iconos de Input

### Preparar Sprites
1. Importar sprites de iconos de input
2. Configurar como Sprite (2D and UI)
3. Organizar en carpetas:
   - `Icons/Keyboard/`
   - `Icons/Xbox/`
   - `Icons/PlayStation/`

### Crear TMP Sprite Assets (Opcional, para iconos inline)
Si quieres iconos dentro del texto en lugar de reemplazo de texto:
1. Window → TextMeshPro → Sprite Importer
2. Crear Sprite Assets para cada conjunto de iconos
3. Asignar en TMP Settings

---

## 🐛 Debug y Testing

### Debug Mode
Activar en TutorialManager Inspector para ver logs detallados:
- Tutoriales disparados
- Tutoriales completados
- Estado de triggers

```csharp
// Activar desde código
TutorialManager.Instance.debugMode = true;
```

### Métodos de Debug
```csharp
// Log de estado actual completo
TutorialManager.Instance.DebugLogTutorialStatus();

// Output:
// === ESTADO DE TUTORIALES ===
// Habilitados: true
// Tutorial activo: movement_basic
// Tutoriales en cola: 2
// Tutoriales completados: 5
//   - movement_basic
//   - movement_sprint
//   ...
```

### Resetear Tutoriales

**Opción 1: Desde Código**
```csharp
TutorialManager.Instance.ResetAllTutorials();
```

**Opción 2: Desde Inspector**
1. Seleccionar TutorialManager en escena (Play Mode)
2. Llamar método público `ResetAllTutorials()`

**Opción 3: Manualmente**
```csharp
PlayerPrefs.DeleteKey("CompletedTutorials");
PlayerPrefs.DeleteKey("TutorialsEnabled");
PlayerPrefs.Save();
```

### Disparar Tutorial Manualmente (Testing)
```csharp
// Durante Play Mode
TutorialManager.Instance.TriggerTutorial(tutorialData);

// Forzar mostrar aunque ya esté completado
tutorialData.showOnlyOnce = false;
TutorialManager.Instance.TriggerTutorial(tutorialData);
```

### Gizmos en Editor
TutorialTriggerZone dibuja zonas de trigger:
- **Verde semi-transparente** = zona de trigger
- **Amarillo wireframe** = radio de interacción (si require interaction)
- Más visible cuando está seleccionado

### Verificar Estado
```csharp
// ¿Está el sistema habilitado?
bool enabled = TutorialManager.Instance.AreTutorialsEnabled();

// ¿Tutorial específico completado?
bool done = TutorialManager.Instance.HasCompletedTutorial("movement_basic");

// ¿Hay tutorial activo ahora?
bool active = TutorialManager.Instance.isTutorialActive; // (propiedad privada, ver en Inspector)
```

---

## ⚙️ Personalización

### Tamaño de Texto
**En Components UI:**
1. Seleccionar SimpleDialogPresenter o CardPopoverPresenter
2. Encontrar TextMeshProUGUI component
3. Ajustar Font Size directamente

**Escala Global desde Opciones:**
```csharp
public class TutorialSettings : MonoBehaviour
{
    public SimpleDialogPresenter dialogPresenter;
    public CardPopoverPresenter cardPresenter;
    
    private float baseFontSizeDialog = 24f;
    private float baseFontSizeCard = 20f;
    
    public void SetTextScale(float scale) // 1.0 = 100%, 1.2 = 120%
    {
        // Aplicar a SimpleDialog
        var dialogText = dialogPresenter.GetComponentInChildren<TextMeshProUGUI>();
        dialogText.fontSize = baseFontSizeDialog * scale;
        
        // Aplicar a CardPopover
        var cardTexts = cardPresenter.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in cardTexts)
        {
            text.fontSize = baseFontSizeCard * scale;
        }
    }
}
```

### Colores y Estilos
**Backgrounds:**
```csharp
// Cambiar transparencia de fondos
CanvasGroup canvasGroup = dialogContainer.GetComponent<CanvasGroup>();
canvasGroup.alpha = 0.9f; // Más opaco

Image background = dialogContainer.GetComponent<Image>();
background.color = new Color(0, 0, 0, 0.95f);
```

**Indicadores de Progreso:**
```csharp
// En CardPopoverPresenter Inspector
activeDotColor = Color.white;
inactiveDotColor = Color.gray;
```

### Duraciones de Animación
```csharp
// En SimpleDialogPresenter/CardPopoverPresenter Inspector
fadeInDuration = 0.3f;  // Más rápido: 0.2f, Más lento: 0.5f
fadeOutDuration = 0.3f;
cardTransitionDuration = 0.2f;
```

### Nuevos Placeholders de Input
Para agregar nuevas acciones de input:

**1. Agregar a enum en TutorialEnums.cs:**
```csharp
public enum InputAction
{
    Move,
    Run,
    // ... existentes
    Jump,        // NUEVO
    Crouch       // NUEVO
}
```

**2. Agregar sprites en InputIconMapper.cs:**
```csharp
[Header("Sprites de Teclado")]
public Sprite keyboardJump;
public Sprite keyboardCrouch;

[Header("Sprites de Xbox")]
public Sprite xboxJump;
public Sprite xboxCrouch;
// ... etc
```

**3. Agregar mapeo en InputIconMapper.cs:**
```csharp
private void UpdateCurrentIconSet()
{
    // ... existente
    currentIconSet[InputAction.Jump] = keyboardJump; // o xbox/ps según dispositivo
    currentIconSet[InputAction.Crouch] = keyboardCrouch;
}

private string GetKeyboardText(InputAction action)
{
    switch (action)
    {
        // ... existentes
        case InputAction.Jump: return "Espacio";
        case InputAction.Crouch: return "Ctrl";
        default: return action.ToString();
    }
}
```

**4. Agregar reemplazo en ProcessTextPlaceholders():**
```csharp
processed = processed.Replace("{Jump}", GetTextForAction(InputAction.Jump));
processed = processed.Replace("{Crouch}", GetTextForAction(InputAction.Crouch));
```

**5. Usar en tutoriales:**
```
"Presiona {Jump} para saltar y {Crouch} para agacharte"
```

---

## 🔧 Troubleshooting

### ❌ Los tutoriales no se disparan

**Causas posibles:**
- TutorialManager no está en la escena
- Triggers no tienen TutorialData asignado
- Tutorial ya completado y `showOnlyOnce = true`
- Sistema de tutoriales deshabilitado

**Soluciones:**
```csharp
// 1. Verificar TutorialManager existe
if (TutorialManager.Instance == null)
    Debug.LogError("TutorialManager no encontrado!");

// 2. Activar Debug Mode
TutorialManager.Instance.debugMode = true;

// 3. Resetear tutorial específico
TutorialManager.Instance.ResetAllTutorials();

// 4. Verificar sistema habilitado
if (!TutorialManager.Instance.AreTutorialsEnabled())
    TutorialManager.Instance.SetTutorialsEnabled(true);
```

---

### ❌ Los iconos no se muestran (aparece texto con {Move})

**Causas posibles:**
- InputIconMapper no tiene sprites asignados
- InputIconMapper no está en la escena
- Placeholders mal escritos

**Soluciones:**
1. Verificar que TutorialManager tenga componente InputIconMapper
2. Asignar todos los sprites en Inspector
3. Verificar que placeholders sean exactos: `{Move}` NO `{move}` o `{MOVE}`
4. Comprobar que InputIconMapper.Instance no sea null

---

### ❌ El juego no se pausa durante tutoriales de batalla

**Causas posibles:**
- TutorialData tiene `pauseGame = false`
- Otro script está modificando `Time.timeScale`

**Soluciones:**
```csharp
// 1. Verificar configuración del tutorial
tutorialData.pauseGame = true; // Debe estar activado

// 2. Comprobar Time.timeScale en Update
void Update()
{
    if (isTutorialActive && Time.timeScale != 0f)
        Debug.LogWarning("Algo está modificando timeScale!");
}
```

---

### ❌ Las tarjetas no avanzan

**Causas posibles:**
- Input no funciona
- `requireConfirmation = false` pero `minDisplayTime = 0`

**Soluciones:**
1. Verificar que tarjetas tengan `requireConfirmation = true`
2. Probar con Enter (teclado) o A (gamepad)
3. Revisar que Input System esté configurado
4. Verificar en Console si hay errores de Input System

---

### ❌ Tutorial se muestra cada vez (no se guarda completado)

**Causas posibles:**
- `showOnlyOnce = false` en TutorialData
- `tutorialId` vacío o duplicado
- PlayerPrefs no se guarda

**Soluciones:**
```csharp
// 1. Verificar configuración
tutorialData.showOnlyOnce = true;
tutorialData.tutorialId = "unique_id_here"; // No vacío!

// 2. Forzar guardado
PlayerPrefs.Save();

// 3. Verificar guardado
string saved = PlayerPrefs.GetString("CompletedTutorials");
Debug.Log($"Tutoriales completados guardados: {saved}");
```

---

### ❌ Gizmos no se ven en el editor

**Solución:**
1. Asegurarse que Gizmos esté activado en Scene View (botón arriba derecha)
2. Seleccionar el GameObject con TutorialTriggerZone
3. Los gizmos solo se dibujan en Scene View, no en Game View

---

### ❌ Error: "TutorialManager no encontrado"

**Causa:**
TutorialManager se destruyó o no está en la escena

**Solución:**
```csharp
// Verificar antes de usar
if (TutorialManager.Instance != null)
{
    TutorialManager.Instance.TriggerTutorial(tutorial);
}
else
{
    Debug.LogError("Asegúrate de tener TutorialManager en la escena!");
}
```

---

## 📝 Notas Importantes

### ⚠️ Requisitos
- **Unity Input System** (nuevo sistema de input)
- **TextMeshPro** (incluido en Unity 2020+)
- **UnityEngine.UI**

### ✅ Buenas Prácticas
- Usar IDs únicos para cada tutorial (`movement_basic`, NO `tutorial1`)
- Nombrar assets como `Tutorial_[NombreDescriptivo]`
- Agrupar triggers en GameObjects organizados
- Probar flujo completo antes de producción
- Mantener textos cortos y claros (máx 2-3 líneas)
- Usar placeholders consistentemente: `{Move}` no `{move}`

### 🚫 Limitaciones Actuales
- Solo español (sin sistema de localización)
- Solo text size scaling (sin otras opciones de accesibilidad)
- PlayerPrefs para persistencia (no archivos de guardado)
- Solo animaciones fade (no slide, scale, etc.)
- No soporta video en tarjetas (solo imágenes estáticas)

---

## Arquitectura de Archivos Final

```
Assets/
├── Scripts/
│   └── TutorialSystem/
│       ├── TutorialEnums.cs
│       ├── TutorialCard.cs
│       ├── TutorialData.cs
│       ├── TutorialManager.cs
│       ├── InputIconMapper.cs
│       ├── UI/
│       │   ├── SimpleDialogPresenter.cs
│       │   ├── CardPopoverPresenter.cs
│       │   └── DynamicInputIcon.cs
│       ├── Triggers/
│       │   ├── TutorialTrigger.cs
│       │   ├── TutorialTriggerZone.cs
│       │   └── BattleTutorialTrigger.cs
│       └── TUTORIAL_SETUP_GUIDE.md (este archivo)
│
├── Data/
│   └── Tutorials/
│       ├── Tutorial_MovementBasic.asset
│       ├── Tutorial_Sprint.asset
│       ├── Tutorial_CameraControl.asset
│       ├── Tutorial_BattleIntro.asset
│       ├── Tutorial_BattleStamina.asset
│       ├── Tutorial_BattleAttack.asset
│       ├── Tutorial_QTEIntroduction.asset
│       ├── Tutorial_EnemyTurnIntro.asset
│       ├── Tutorial_ParryIntroduction.asset
│       ├── Tutorial_ParryPractice.asset
│       ├── Tutorial_MultipleActions.asset
│       └── Tutorial_SkillIntroduction.asset
│
└── Sprites/
    └── Icons/
        ├── Keyboard/
        ├── Xbox/
        └── PlayStation/
```

---

## 📁 Estructura de Archivos Completa

```
Assets/
├── Scripts/
│   └── TutorialSystem/
│       ├── TutorialEnums.cs
│       ├── TutorialCard.cs
│       ├── TutorialData.cs
│       ├── TutorialManager.cs
│       ├── InputIconMapper.cs
│       ├── UI/
│       │   ├── SimpleDialogPresenter.cs
│       │   ├── CardPopoverPresenter.cs
│       │   └── DynamicInputIcon.cs
│       ├── Triggers/
│       │   ├── TutorialTrigger.cs
│       │   ├── TutorialTriggerZone.cs
│       │   └── BattleTutorialTrigger.cs
│       ├── Editor/
│       │   └── TutorialDataEditor.cs
│       └── TUTORIAL_SETUP_GUIDE.md (este archivo)
│
├── Data/
│   └── Tutorials/
│       ├── Tutorial_MovementBasic.asset
│       ├── Tutorial_Sprint.asset
│       ├── Tutorial_CameraControl.asset
│       ├── Tutorial_BattleIntro.asset
│       ├── Tutorial_BattleStamina.asset
│       ├── Tutorial_BattleAttack.asset
│       ├── Tutorial_QTEIntroduction.asset
│       ├── Tutorial_EnemyTurnIntro.asset
│       ├── Tutorial_ParryIntroduction.asset
│       ├── Tutorial_ParryPractice.asset
│       ├── Tutorial_MultipleActions.asset
│       └── Tutorial_SkillIntroduction.asset
│
└── Sprites/
    └── Icons/
        ├── Keyboard/
        ├── Xbox/
        └── PlayStation/
```

---

## 🔮 Extensiones Futuras

### Fáciles de Agregar
- [ ] **Tutorial debug panel** en editor window
- [ ] **Replay de tutoriales** desde menú de opciones
- [ ] **Más tipos de triggers** (tiempo, distancia recorrida, combos)
- [ ] **Sonidos** en fade in/out de tarjetas
- [ ] **Highlight de UI** durante tutoriales (outline en botones)
- [ ] **Skip tutorial button** visible en pantalla
- [ ] **Tutorial hints** en loading screens

### Mediana Complejidad
- [ ] **Sistema de hints** (recordatorios si el jugador olvida controles)
- [ ] **Analytics de tutoriales** (cuáles se saltan más, tiempo de lectura)
- [ ] **Tutorial de gamepad** específico al conectar por primera vez
- [ ] **GIFs animados** en tarjetas
- [ ] **Tutorials progresivos** (desbloquear según progreso)
- [ ] **Voice-over** support para tutoriales
- [ ] **Tutorial achievement system** (completar todos los tutoriales)

### Complejas
- [ ] **Sistema de localización** multi-idioma completo
- [ ] **Tutoriales contextuales adaptativos** (basados en errores del jugador)
- [ ] **Editor visual** para crear tutoriales sin código
- [ ] **Tutorial recording system** (grabar inputs del jugador y reproducir)
- [ ] **Integración con sistema de logros** de Steam/PlayStation
- [ ] **Tutoriales interactivos** (con validación de acciones)
- [ ] **Tutorial difficulty scaling** (más/menos ayuda según skill)

---

## 📊 Próximos Pasos Sugeridos

### Fase 1: Implementación Básica ✅
- [x] Crear todos los scripts core
- [x] Implementar UI presenters
- [x] Sistema de triggers
- [x] Persistencia con PlayerPrefs
- [ ] **→ Configurar UI en escena**
- [ ] **→ Crear TutorialData assets**
- [ ] **→ Colocar triggers en nivel**

### Fase 2: Contenido
- [ ] Importar sprites de iconos de input
- [ ] Crear imágenes para tarjetas de tutorial
- [ ] Escribir todos los textos de tutoriales
- [ ] Testear flujo completo de tutoriales
- [ ] Ajustar timings según feedback

### Fase 3: Polish
- [ ] Ajustar colores y estilos visuales
- [ ] Añadir sonidos (opcional)
- [ ] Optimizar transiciones
- [ ] Testing con jugadores reales
- [ ] Iterar basado en feedback

### Fase 4: Avanzado (Opcional)
- [ ] Implementar algunas extensiones de la lista
- [ ] Sistema de hints
- [ ] Analytics básicos
- [ ] Replay desde opciones

---

## 👨‍💻 Información del Proyecto

**Sistema:** Tutorial System  
**Proyecto:** Sangre y Cerezo  
**Compatible con:** Unity 2021.3+  
**Dependencias:** Unity Input System, TextMeshPro  
**Integrado con:** MovimientoV2, BattleManagerV2  

**Características Principales:**
- ✅ 2 tipos de tutoriales (SimpleDialog, CardPopover)
- ✅ Input dinámico (auto-detecta teclado/gamepad)
- ✅ 3 tipos de triggers (Zona, Batalla, Manual)
- ✅ Persistencia automática
- ✅ Editor personalizado
- ✅ Sistema de progresión (12 tutoriales predefinidos)
- ✅ Totalmente extensible

---

## 📚 Resumen Final

Este sistema de tutoriales proporciona todo lo necesario para enseñar las mecánicas de tu juego de forma efectiva:

1. **Quick Start** (1-2 horas) → Sistema funcional mínimo
2. **Configuración Detallada** → Personalización completa
3. **12 Tutoriales Predefinidos** → Cobertura completa del juego
4. **API Completa** → Fácil integración con otros sistemas
5. **Debug Tools** → Testing y troubleshooting eficiente
6. **Extensible** → Listo para futuras mejoras

**¡Sistema completo y listo para usar!** 🎉

---

### Última Actualización
Este documento fue generado como guía consolidada del sistema de tutoriales.  
Incluye toda la información necesaria para configurar, usar y extender el sistema.

**Si tienes dudas:**
1. Revisa la sección de Troubleshooting
2. Activa Debug Mode y revisa Console
3. Verifica que seguiste todos los pasos del Quick Start

**¡Buena suerte con tu tutorial system!** 🚀

