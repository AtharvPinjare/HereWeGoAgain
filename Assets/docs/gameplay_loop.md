# Gameplay Loop Design Document
---
**References:** game_design.md — Sections 2, 3, 4, 6, 8, 9, 11 | anomaly_system.md — Sections 4, 6, 7
**Primary Scripts:** GameManager.cs, EventBus.cs, ButtonManager.cs, HealthSystem.cs,
                     FalsePositiveController.cs, ProgressTracker.cs, EnvironmentController.cs
**Depends On:** AnomalyManager.cs, UIManager.cs

---

## Table of Contents
1. State Machine Overview
2. GameManager — Responsibilities & Structure
3. State Definitions & Transitions
4. Full Day Lifecycle — Sequence Diagrams
5. Button System — ButtonManager
6. Health System — HealthSystem
7. False Positive Controller — FalsePositiveController
8. Progress Tracker — ProgressTracker
9. Environment Controller — EnvironmentController
10. EventBus — Complete Event Registry
11. Run Initialisation Sequence
12. Win Sequence
13. Death & Reset Sequence
14. Implementation Notes for Codex

---

## 1. State Machine Overview

The entire game is driven by a single enum-based state machine owned by GameManager. No other system changes game state directly — they fire events and GameManager responds.

```
GameState Enum
├── MainMenu
├── RunStart          // Initialisation phase before Day 1
├── DayStart          // Player on balcony, walking into house
├── DayActive         // Player inside, anomaly or safe day running
├── DayResolved       // Correct button pressed, day ending
├── FalsePositive     // RED pressed on safe day — devil sequence
├── PlayerDead        // Death confirmed, reset pending
└── GameWon           // All 7 days completed successfully
```

### State Transition Map

```
MainMenu
   └──► RunStart (StartGame())
            └──► DayStart (day 1)
                     └──► DayActive
                               ├──► DayResolved (correct button pressed)
                               │         └──► DayStart (next day)
                               │                   ... repeat days 1–7 ...
                               │                            └──► GameWon (day 7 resolved)
                               │
                               ├──► FalsePositive (RED on safe day)
                               │         └──► PlayerDead
                               │                   └──► RunStart (full reset)
                               │
                               └──► PlayerDead (anomaly fail state)
                                         └──► RunStart (full reset)
```

---

## 2. GameManager — Responsibilities & Structure

GameManager is a persistent singleton (DontDestroyOnLoad). It owns the game state, drives all phase transitions, and tracks all run-level variables.

```
GameManager : MonoBehaviour [Singleton]
│
├── Run Variables
│   ├── GameState currentState
│   ├── int currentDay                    // 1–7
│   ├── int buttonPoolSize                // Starts at 3, max 7
│   ├── int totalAnomalyDays             // Set at run start (4 or 5)
│   ├── int successfulAnomalyDays        // Incremented on each anomaly resolved
│   └── bool[] dayCompletionRecord       // Index = day-1, true = completed
│
├── Public Methods
│   ├── void StartGame()                  // MainMenu → RunStart
│   ├── void BeginRun()                   // RunStart → DayStart (day 1)
│   ├── void OnDayResolved()              // DayActive → DayResolved → next DayStart
│   ├── void OnPlayerDied()              // Any state → PlayerDead → RunStart
│   └── void OnGameWon()                 // DayResolved (day 7) → GameWon
│
├── Private Methods
│   ├── void TransitionToState(GameState newState)
│   ├── void HandleDayStart()
│   ├── void HandleDayResolved()
│   └── void HandleRunReset()
│
└── EventBus Subscriptions
    ├── OnAnomalyResolved   → HandleAnomalyResolved()   // Expand button pool, mark day
    ├── OnSafeDayCleared    → HandleSafeDayCleared()    // Mark day, no pool expansion
    ├── OnFalsePositive     → TransitionToState(FalsePositive)
    └── OnPlayerDied        → OnPlayerDied()
```

**Rules:**
- GameManager never directly calls anomaly scripts.
- GameManager never directly calls UIManager. It fires events; UIManager listens.
- All state transitions must go through TransitionToState() — never set currentState directly.
- TransitionToState() fires EventBus.OnGameStateChanged(GameState) on every call.

---

## 3. State Definitions & Transitions

### MainMenu
- Entry: Application launch.
- Active: Main menu UI visible. No game systems running.
- Exit: Player presses Start → StartGame() → RunStart.

---

### RunStart
- Entry: StartGame() called OR death reset completes.
- Active:
  - AnomalyManager.InitialiseRun() — assigns anomalies to days, sets safe day count.
  - ButtonManager.InitialisePool() — resets button pool to size 3.
  - ProgressTracker.ResetAll() — all balcony plants set to dead.
  - HealthSystem.Reset() — health fully restored.
  - EnvironmentController.ResetScene() — all scene objects to default positions/states.
  - currentDay = 1.
- Exit: All systems confirm initialisation complete → BeginRun() → DayStart.
- Duration: Single frame (no player visibility during this state).

---

### DayStart
- Entry: BeginRun() (day 1) or OnDayResolved() (days 2–7).
- Active:
  - Player spawns on balcony (or transitions to balcony).
  - EventBus.OnDayStarted(currentDay) fires.
  - UIManager shows day number display briefly.
  - AnomalyManager.ActivateDayAnomaly(currentDay) called.
  - ButtonManager.PlaceButtonForDay() called.
  - Player can move freely. No interaction with buttons until inside house.
- Exit: Player walks through front door into living room → DayActive.
- Trigger: OnTriggerEnter with "FrontDoor" zone collider on player.

---

### DayActive
- Entry: Player crosses front door threshold.
- Active:
  - Full player control enabled (movement + button interaction).
  - Active anomaly (if any) running its escalation logic independently.
  - ButtonManager listening for player proximity to either button.
  - No time limit imposed by GameManager — each anomaly owns its own timer.
- Exit paths:
  - RED pressed + anomaly active → DayResolved.
  - GREEN pressed + safe day → DayResolved.
  - RED pressed + safe day → FalsePositive.
  - GREEN pressed + anomaly active → anomaly TriggerFailState() → PlayerDead.
  - Anomaly timer expires → anomaly TriggerFailState() → PlayerDead.

---

### DayResolved
- Entry: Correct button pressed (RED on anomaly day or GREEN on safe day).
- Active:
  - If anomaly day: AnomalyManager.ResolveCurrentAnomaly().
  - buttonPoolSize incremented by 1 (only on anomaly days, capped at 7).
  - dayCompletionRecord[currentDay-1] = true.
  - EventBus.OnDayResolved(currentDay) fires.
  - ProgressTracker blooms the next plant.
  - Brief pause (1.5 seconds) — player sees the resolution.
- Exit:
  - If currentDay < 7: currentDay++ → DayStart.
  - If currentDay == 7: → GameWon.

---

### FalsePositive
- Entry: RED pressed on a safe day. EventBus.OnFalsePositive fires.
- Active:
  - FalsePositiveController owns this state entirely.
  - Player input fully locked.
  - 5-second silence.
  - Devil spawns and approaches.
  - On contact: EventBus.OnPlayerDied fires.
- Exit: EventBus.OnPlayerDied → PlayerDead.
- GameManager does not drive any logic during this state — it only listens for OnPlayerDied.

---

### PlayerDead
- Entry: EventBus.OnPlayerDied received by GameManager.
- Active:
  - Blackout screen held for 2.0 seconds.
  - "Day 1" title card shown briefly.
  - All systems reset (see Section 13 — Death & Reset Sequence).
- Exit: Reset complete → RunStart.

---

### GameWon
- Entry: Day 7 resolved successfully.
- Active:
  - Fade to black.
  - "THE WORLD IS SAVED" title card displayed.
  - ProgressTracker shows all 7 plants glowing.
  - Credits sequence.
- Exit: Player returns to MainMenu (button on win screen) or application closes.

---

## 4. Full Day Lifecycle — Sequence Diagrams

### Anomaly Day — Successful Resolution

```
GameManager          AnomalyManager        ButtonManager         Player
     |                     |                     |                  |
DayStart ──────────────────►                     |                  |
     |         ActivateDayAnomaly(day)            |                  |
     |                     |                     |                  |
     |               Activate()                  |                  |
     |               [anomaly running]     PlaceButtonForDay()      |
     |                     |                     |                  |
     |                     |                     |    [player walks in]
     ◄──────────────── DayActive ────────────────────────────────── ►
     |                     |                     |                  |
     |                     |           [player near RED, presses E] |
     |                     |         OnREDPressed ◄─────────────────|
     |                     |                     |                  |
     |    ResolveCurrentAnomaly() ◄──────────────|                  |
     |                     |                     |                  |
     |               Resolve()                   |                  |
     |                     |── OnAnomalyResolved ──────────────────►|
     |                     |                     |                  |
OnAnomalyResolved ◄─────── |                     |                  |
     |                     |                     |                  |
[buttonPoolSize++]          |                     |                  |
[dayCompletionRecord]       |                     |                  |
     |── OnDayResolved ─────────────────────────────────────────────►
     |                     |                     |                  |
  [1.5s pause]             |                     |                  |
     |                     |                     |                  |
  DayStart (day+1)         |                     |                  |
```

---

### Safe Day — Correct GREEN Press

```
GameManager          AnomalyManager        ButtonManager         Player
     |                     |                     |                  |
DayStart ──────────────────►                     |                  |
     |    ActivateDayAnomaly(day)                 |                  |
     |    [returns null — safe day]         PlaceButtonForDay()      |
     |                     |               [RED placed, glowing]    |
     |                     |                                        |
     ◄──────────────── DayActive ────────────────────────────────── ►
     |                     |                     |                  |
     |                     |         [player near GREEN, presses E] |
     |                     |         OnGREENPressed ◄───────────────|
     |                     |                     |                  |
     |  [confirm safe day: currentActiveAnomaly == null]            |
     |                     |                     |                  |
     |── OnSafeDayCleared ──────────────────────────────────────────►
     |                     |                     |                  |
[dayCompletionRecord]       |                     |                  |
[buttonPoolSize unchanged]  |                     |                  |
     |                     |                     |                  |
  DayStart (day+1)         |                     |                  |
```

---

### False Positive — RED on Safe Day

```
GameManager      FalsePositiveController    ButtonManager         Player
     |                     |                     |                  |
  DayActive                |                     |                  |
     |                     |         [player presses RED, safe day] |
     |                     |         OnREDPressed ◄───────────────  |
     |                     |                     |                  |
     |  [AnomalyManager confirms: no active anomaly]               |
     |── OnFalsePositive ──►|                    |                  |
     |                     |                     |                  |
FalsePositive state    [lock player input]        |                  |
     |               [5 seconds silence]          |                  |
     |               [devil spawns]               |                  |
     |               [devil approaches]           |                  |
     |               [contact: OnPlayerDied]──────────────────────► |
     |                     |                     |                  |
PlayerDead ◄───────────────|                     |                  |
     |                     |                     |                  |
  [reset sequence]         |                     |                  |
```

---

## 5. Button System — ButtonManager

```
ButtonManager : MonoBehaviour
│
├── Fields
│   ├── [SerializeField] Transform[] redButtonPositions    // All 7 physical positions in scene
│   ├── [SerializeField] Transform greenButtonPosition     // Single fixed position
│   ├── [SerializeField] GameObject redButtonObject        // The RED button prop
│   ├── [SerializeField] float interactionRadius           // Distance for E key interaction (default: 1.5)
│   └── int currentPoolSize                               // Synced with GameManager.buttonPoolSize
│
├── Public Methods
│   ├── void InitialisePool()
│   │       Sets currentPoolSize = 3. Called on RunStart.
│   │
│   ├── void ExpandPool()
│   │       currentPoolSize = Mathf.Min(currentPoolSize + 1, 7). Called on OnAnomalyResolved.
│   │
│   └── void PlaceButtonForDay()
│           On anomaly days: picks random position from redButtonPositions[0..currentPoolSize-1].
│           Moves redButtonObject to that position. Enables glow.
│           On safe days: picks random position from full pool (all 7 positions).
│           RED button still placed and glowing — players cannot use its presence as a safe day signal.
│
├── Private Methods
│   ├── void Update()
│   │       Each frame during DayActive: check player distance to RED and GREEN buttons.
│   │       If within interactionRadius and E key pressed: fire appropriate event.
│   │
│   ├── void OnREDPressed()
│   │       If AnomalyManager.currentActiveAnomaly != null:
│   │           AnomalyManager.ResolveCurrentAnomaly()
│   │       Else:
│   │           EventBus.OnFalsePositive.Invoke()
│   │
│   └── void OnGREENPressed()
│           If AnomalyManager.currentActiveAnomaly != null:
│               AnomalyManager.TriggerGreenOnAnomalyDay()
│           Else:
│               EventBus.OnSafeDayCleared.Invoke()
│
└── EventBus Subscriptions
    ├── OnAnomalyResolved → ExpandPool()
    └── OnGameStateChanged → enable/disable Update() logic based on state
```

**Rules:**
- ButtonManager is only active (processing input) during DayActive state.
- RED button is always physically present and glowing — every day without exception.
- Button positions array is populated entirely in the Inspector. No runtime position generation.

---

## 6. Health System — HealthSystem

```
HealthSystem : MonoBehaviour
│
├── Fields
│   ├── [SerializeField] int maxHits          // Default: 3
│   ├── int currentHits                       // Hits taken this day
│   ├── [SerializeField] Image vignetteImage  // Screen corner vignette UI element
│   ├── [SerializeField] Color[] vignetteStages  // Color per hit stage [clear, light red, dark red]
│   └── bool isDead
│
├── Public Methods
│   ├── void TakeDamage()
│   │       Increments currentHits.
│   │       Updates vignette to vignetteStages[currentHits].
│   │       Fires EventBus.OnPlayerHit.
│   │       If currentHits >= maxHits: SetDead().
│   │
│   ├── void Reset()
│   │       currentHits = 0. isDead = false.
│   │       Vignette set to vignetteStages[0] (fully clear).
│   │       Called at start of each new day via OnDayStarted event.
│   │
│   └── void SetDead()
│           isDead = true.
│           Fires EventBus.OnPlayerDied.
│
└── EventBus Subscriptions
    └── OnDayStarted → Reset()
```

**Notes:**
- HealthSystem never kills the player directly in anomaly fail states. Fail states use BlackoutAndReset() from AnomalyBase, which fires OnPlayerDied independently.
- TakeDamage() is the only path for contact-damage anomalies (dolls, frame).
- HealthSystem.Reset() is called by OnDayStarted — health clears automatically every day.

---

## 7. False Positive Controller — FalsePositiveController

```
FalsePositiveController : MonoBehaviour
│
├── Fields
│   ├── [SerializeField] GameObject devilPrefab         // Devil entity
│   ├── [SerializeField] Transform devilSpawnPoint      // Fixed spawn in front of player start
│   ├── [SerializeField] float silenceDuration          // Default: 5.0 seconds
│   ├── [SerializeField] float devilMoveSpeed           // Default: 3.5
│   └── GameObject activeDevil
│
├── Private Methods
│   └── IEnumerator FalsePositiveSequence()
│           Step 1: Lock all player input (EventBus.OnInputLocked fires or direct PlayerController disable).
│           Step 2: yield WaitForSeconds(silenceDuration).
│           Step 3: Instantiate devil at devilSpawnPoint.
│           Step 4: Devil moves toward frozen player at devilMoveSpeed.
│                   Movement is a simple Transform.MoveTowards — no NavMesh.
│           Step 5: OnTriggerEnter with Player → EventBus.OnPlayerDied.Invoke().
│           Step 6: Destroy activeDevil.
│
└── EventBus Subscriptions
    └── OnFalsePositive → StartCoroutine(FalsePositiveSequence())
```

**Implementation Note:** The devil is the only runtime-instantiated entity in the game. All other anomaly objects are always present in the scene. This is acceptable since FalsePositiveController is a rare single-instance event per run.

**Unity Timeline Note:** If frame-accurate jumpscare precision is preferred over the coroutine approach, replace FalsePositiveSequence() with a PlayableDirector playing a Timeline asset. The Timeline approach is recommended for final polish but the coroutine is sufficient for jam scope.

---

## 8. Progress Tracker — ProgressTracker

```
ProgressTracker : MonoBehaviour
│
├── Fields
│   └── [SerializeField] GameObject[] plantSlots   // 7 plants: index 0 = Day 1 plant
│       Each slot has two child objects:
│           - DeadPlant (active by default)
│           - AlivePlant (inactive by default, glowing material)
│
├── Public Methods
│   ├── void BloomPlant(int dayIndex)
│   │       Deactivates DeadPlant child.
│   │       Activates AlivePlant child.
│   │       Plays a brief scale-up animation (DOTween punch scale).
│   │
│   └── void ResetAll()
│           All plants: DeadPlant active, AlivePlant inactive.
│           Called on RunStart.
│
└── EventBus Subscriptions
    ├── OnDayResolved(int day) → BloomPlant(day - 1)
    └── OnPlayerDied → ResetAll()
```

---

## 9. Environment Controller — EnvironmentController

```
EnvironmentController : MonoBehaviour
│
├── Fields
│   ├── [SerializeField] ScatterableObject[] scatterableObjects
│   │       Each ScatterableObject holds:
│   │           - Transform target
│   │           - Vector3 originalPosition
│   │           - Vector3 originalRotation
│   │           - Bounds allowedScatterBounds   // Box within which it can be randomly placed
│   │
│   └── [SerializeField] Light[] roomLights     // All room light sources
│
├── Public Methods
│   ├── void ScatterAllObjects()
│   │       For each scatterableObject: teleport to random position within allowedScatterBounds.
│   │       Called directly by BackwardClockAnomaly on The Shatter fail state.
│   │       Executes in a single frame — no coroutine, no lerp. Instant.
│   │
│   └── void ResetScene()
│           For each scatterableObject: restore originalPosition and originalRotation.
│           Reset all light intensities to default.
│           Called on RunStart via OnGameStateChanged(RunStart).
│
└── EventBus Subscriptions
    └── OnGameStateChanged(RunStart) → ResetScene()
```

---

## 10. EventBus — Complete Event Registry

Single static class. All events listed. No system may communicate cross-system outside of these events (except the BackwardClock → EnvironmentController direct call).

```csharp
public static class EventBus
{
    // ── Game State ──────────────────────────────────────────
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int>       OnDayStarted;         // day number
    public static event Action<int>       OnDayResolved;        // day number

    // ── Anomaly ─────────────────────────────────────────────
    public static event Action<string>    OnAnomalyActivated;   // anomalyId
    public static event Action<string>    OnAnomalyResolved;    // anomalyId
    public static event Action<string>    OnAnomalyFailState;   // anomalyId

    // ── Button ──────────────────────────────────────────────
    public static event Action            OnSafeDayCleared;
    public static event Action            OnFalsePositive;

    // ── Player ──────────────────────────────────────────────
    public static event Action            OnPlayerHit;
    public static event Action            OnPlayerDied;
    public static event Action            OnInputLocked;
    public static event Action            OnInputUnlocked;

    // ── Run ─────────────────────────────────────────────────
    public static event Action            OnRunStarted;
    public static event Action            OnGameWon;
}
```

**EventBus Rules for Codex:**
- All events are static. No instance required.
- Always null-check before invoking: `OnPlayerDied?.Invoke()`
- Subscribers must unsubscribe in OnDestroy to prevent ghost listeners after reset.
- No event carries a reference to a MonoBehaviour. Data only (primitives, strings, enums).

---

## 11. Run Initialisation Sequence

Called once on game start and once after every death reset. Exact order matters.

```
1. GameManager.TransitionToState(RunStart)
2. EventBus.OnGameStateChanged(RunStart) fires
3. EnvironmentController.ResetScene()          ← scene objects back to default
4. HealthSystem.Reset()                        ← health cleared
5. ProgressTracker.ResetAll()                  ← all plants dead
6. ButtonManager.InitialisePool()              ← pool reset to size 3
7. AnomalyManager.InitialiseRun()              ← anomalies randomly assigned to days
8. currentDay = 1
9. GameManager.BeginRun()
10. GameManager.TransitionToState(DayStart)
11. EventBus.OnDayStarted(1) fires
12. AnomalyManager.ActivateDayAnomaly(1)
13. ButtonManager.PlaceButtonForDay()
14. Player spawns on balcony — player input enabled
```

---

## 12. Win Sequence

Triggered when DayResolved fires and currentDay == 7.

```
1. GameManager.TransitionToState(GameWon)
2. EventBus.OnGameWon fires
3. Player input locked
4. Camera fade to black (2.0 seconds)
5. UIManager shows "THE WORLD IS SAVED" title card
6. ProgressTracker displays all 7 plants glowing (if not already all bloomed)
7. Credits text fades in below title card
8. After 8.0 seconds: Return to Main Menu button appears
```

---

## 13. Death & Reset Sequence

Triggered when EventBus.OnPlayerDied fires from any source.

```
1. GameManager receives OnPlayerDied
2. GameManager.TransitionToState(PlayerDead)
3. All player input immediately locked (EventBus.OnInputLocked fires)
4. Active anomaly's blackout sequence plays (owned by anomaly or FalsePositiveController)
5. Screen held on black for 2.0 seconds
6. UIManager shows brief "Day 1" reset indicator
7. GameManager.HandleRunReset() called:
   a. AnomalyManager.ResetAll()             ← all anomaly scripts reset
   b. currentDay = 1
   c. buttonPoolSize = 3
   d. dayCompletionRecord cleared
8. GameManager.TransitionToState(RunStart)
9. Full run initialisation sequence begins (Section 11)
```

---

## 14. Implementation Notes for Codex

These rules must be included in every gameplay-loop-related Codex prompt:

1. **GameManager is a persistent singleton.** Use DontDestroyOnLoad. Implement with a standard instance pattern — check for duplicate on Awake and destroy if one already exists.

2. **All state transitions go through TransitionToState().** Never set currentState directly anywhere in the codebase.

3. **GameManager never touches UI.** UIManager listens to EventBus events and updates independently.

4. **GameManager never touches anomaly escalation logic.** It calls AnomalyManager methods only. AnomalyManager calls individual anomaly scripts.

5. **EventBus events must null-check on invoke.** Pattern: `EventBus.OnPlayerDied?.Invoke()`. This prevents null reference errors when no subscribers are attached during early development.

6. **All EventBus subscriptions must be removed in OnDestroy.** Pattern: `EventBus.OnPlayerDied -= OnPlayerDied` in every OnDestroy. This is mandatory — ghost subscriptions after reset are a primary source of bugs in agentic-generated Unity code.

7. **ButtonManager is only active during DayActive state.** It must subscribe to OnGameStateChanged and enable/disable its Update logic accordingly. Not using enabled = true/false on the component — use a private bool isListening flag checked at the top of Update.

8. **HealthSystem.Reset() is triggered by OnDayStarted, not by GameManager directly.** GameManager fires the event. HealthSystem responds. This keeps the reset decoupled.

9. **FalsePositiveController devil is the only runtime Instantiate call in the project.** Every other entity is always present in the scene and toggled via SetActive.

10. **The devil spawn point is a fixed Transform in the scene assigned in the Inspector.** It is not calculated at runtime relative to player position. Position it in the scene to appear directly in the player's forward view at a distance of approximately 5 units.

11. **EnvironmentController.ScatterAllObjects() must execute in a single frame.** No lerp, no animation, no coroutine. The visual impact of The Shatter depends entirely on the instantaneous chaos of the transition. A single frame of scattered objects followed by immediate blackout is the intended experience.

12. **DOTween is permitted only in ProgressTracker.BloomPlant() and UIManager transitions.** No other system uses DOTween. Keep the dependency surface minimal.
