# Anomaly System Design Document
---
**References:** game_design.md — Sections 2, 3, 7, 10
**Primary Scripts:** AnomalyManager.cs, AnomalyBase.cs, [AnomalyName]Anomaly.cs (x7)
**Depends On:** GameManager.cs, EventBus.cs, HealthSystem.cs, ButtonManager.cs

---

## Table of Contents
1. System Overview
2. Anomaly Data Structure
3. AnomalyBase — Abstract Contract
4. AnomalyManager — Responsibilities
5. Anomaly States
6. Detection & Resolution Rules
7. EventBus Events — Anomaly System
8. Per-Anomaly Technical Specifications
9. Anomaly Assignment Rules
10. Escalation Timer Model
11. Implementation Notes for Codex

---

## 1. System Overview

The anomaly system is responsible for:
- Knowing which anomaly (if any) is assigned to the current day
- Activating the correct anomaly at the start of a day
- Running that anomaly's escalation logic independently
- Responding to player detection (RED button press) with resolution
- Responding to player failure (GREEN button press on anomaly day / timer expiry) with its own fail state
- Communicating all state changes to other systems exclusively via EventBus

Each anomaly is a **self-contained MonoBehaviour** that owns its own escalation, fail state, and resolution logic. AnomalyManager is a coordinator — it activates and deactivates anomalies. It does not own escalation logic.

**Anomaly objects are always present in the scene. They are never instantiated or destroyed at runtime. They are activated and deactivated.**

---

## 2. Anomaly Data Structure

```
AnomalyData [Serializable]
├── string id                     // Unique identifier e.g. "anomaly_fire"
├── AnomalyType type              // Enum value
├── string displayName            // Human-readable e.g. "The Burning Plant"
├── int assignedDay               // Set by AnomalyManager at run start (1–7), 0 = unassigned
├── bool isActive                 // Currently running this day
├── bool isResolved               // Player pressed RED successfully
├── float escalationRate          // Base speed multiplier, exposed as [SerializeField]
└── int hitCountToKill            // Hits required to kill player (1–3), per anomaly
```

```
AnomalyType [Enum]
├── BurningPlant
├── AttackingDolls
├── ElectricityShortage
├── BleedingFrame
├── ContaminatedBathroom
├── BackwardClock
└── CorruptedText
```

---

## 3. AnomalyBase — Abstract Contract

All anomaly scripts inherit from AnomalyBase. No anomaly script may bypass this contract.

```
AnomalyBase : MonoBehaviour [Abstract]
│
├── [SerializeField] AnomalyData data
│
├── Abstract Methods (must implement in every concrete class)
│   ├── void Activate()           // Begin escalation. Called by AnomalyManager.
│   ├── void Resolve()            // Player pressed RED. Stop escalation. Cleanup. 
│   │                             // Fire EventBus.OnAnomalyResolved.
│   ├── void TriggerFailState()   // Escalation expired OR GREEN pressed on anomaly day.
│   │                             // Owns its own death sequence. Fire EventBus.OnPlayerDied.
│   └── void ResetAnomaly()       // Full reset to idle state. Called on day start and death reset.
│
├── Protected Methods (shared utility, implemented in base)
│   ├── void DealDamageToPlayer() // Calls HealthSystem.TakeDamage()
│   └── IEnumerator BlackoutAndReset(float delay) // Standard death transition
│
└── Properties
    ├── bool IsActive             // Read-only. True between Activate() and Resolve()/TriggerFailState()
    └── AnomalyData Data          // Read-only public accessor
```

**Rules:**
- Anomaly scripts are disabled (enabled = false) on non-assigned days.
- AnomalyManager sets the assigned day, then enables the script on the correct day.
- Anomaly scripts must never call GameManager directly. Use EventBus only.
- All `[SerializeField]` escalation floats must have sensible default values for first compile.

---

## 4. AnomalyManager — Responsibilities

AnomalyManager is a MonoBehaviour singleton. It coordinates all anomaly scripts.

```
AnomalyManager
│
├── Fields
│   ├── [SerializeField] List<AnomalyBase> allAnomalies     // All 7, assigned in Inspector
│   ├── AnomalyBase currentActiveAnomaly                    // Null on safe days
│   ├── int[] dayAssignments                                // Index: day number. Value: anomaly index or -1 (safe)
│   └── int safeDayCount                                    // 2 or 3, randomised at run start
│
├── Public Methods
│   ├── void InitialiseRun()          // Called by GameManager on game start.
│   │                                 // Randomly assigns anomalies to days.
│   │                                 // Respects Text Anomaly Day 3+ constraint.
│   │                                 // Sets safe day count to 2 or 3.
│   │
│   ├── void ActivateDayAnomaly(int day)  // Called by GameManager on each day start.
│   │                                     // Enables and calls Activate() on assigned anomaly.
│   │                                     // If safe day: currentActiveAnomaly = null.
│   │
│   ├── void ResolveCurrentAnomaly()  // Called by ButtonManager when RED is pressed.
│   │                                 // Calls currentActiveAnomaly.Resolve().
│   │                                 // Fires EventBus.OnAnomalyResolved.
│   │
│   ├── void TriggerGreenOnAnomalyDay() // Called by ButtonManager when GREEN pressed on anomaly day.
│   │                                    // Calls currentActiveAnomaly.TriggerFailState().
│   │
│   └── void ResetAll()               // Called on player death. Calls ResetAnomaly() on all scripts.
│                                     // Clears dayAssignments. Re-runs InitialiseRun().
│
└── Private Methods
    └── void AssignAnomalyDays()      // Internal random assignment logic (see Section 9).
```

---

## 5. Anomaly States

Each anomaly passes through these states in order. Only forward transitions are valid.

```
IDLE ──► ACTIVE ──► RESOLVED
                └──► FAIL_STATE ──► DEAD (triggers reset)
```

| State | Description |
|---|---|
| IDLE | Script disabled. Anomaly objects in default scene positions. |
| ACTIVE | Script enabled. Activate() has been called. Escalation running. |
| RESOLVED | Player pressed RED. Resolve() called. Escalation stopped. Scene cleaned up. |
| FAIL_STATE | Timer expired or GREEN pressed on anomaly day. TriggerFailState() called. Death sequence playing. |
| DEAD | Death sequence complete. EventBus.OnPlayerDied fired. Awaiting ResetAll(). |

---

## 6. Detection & Resolution Rules

### Correct Detection — Player Presses RED on Anomaly Day
1. ButtonManager detects RED press.
2. ButtonManager calls AnomalyManager.ResolveCurrentAnomaly().
3. AnomalyManager calls currentActiveAnomaly.Resolve().
4. Anomaly fires EventBus.OnAnomalyResolved(string anomalyId).
5. GameManager receives event, increments day, expands button pool by 1.
6. ProgressTracker receives event, blooms next plant.

### Missed Detection — GREEN Pressed on Anomaly Day
1. ButtonManager detects GREEN press.
2. ButtonManager checks with AnomalyManager: is currentActiveAnomaly active?
3. If yes: AnomalyManager.TriggerGreenOnAnomalyDay() → anomaly's TriggerFailState() runs.
4. Anomaly fires EventBus.OnPlayerDied.

### Correct Safe Day — GREEN Pressed on Safe Day
1. ButtonManager detects GREEN press.
2. AnomalyManager confirms currentActiveAnomaly is null.
3. EventBus.OnSafeDayCleared fires.
4. GameManager increments day. Button pool unchanged (no RED was pressed).

### False Positive — RED Pressed on Safe Day
1. ButtonManager detects RED press.
2. AnomalyManager confirms currentActiveAnomaly is null.
3. EventBus.OnFalsePositive fires.
4. FalsePositiveController handles the devil spawn sequence independently.
5. FalsePositiveController fires EventBus.OnPlayerDied after sequence completes.

### Timer Expiry — Player Does Nothing
1. Active anomaly's internal timer reaches zero.
2. Anomaly calls its own TriggerFailState().
3. EventBus.OnPlayerDied fires.

---

## 7. EventBus Events — Anomaly System

These events must exist on EventBus.cs. All anomaly scripts communicate exclusively through these.

```csharp
// Anomaly lifecycle
static event Action<string> OnAnomalyActivated;       // anomalyId
static event Action<string> OnAnomalyResolved;        // anomalyId — player pressed RED correctly
static event Action<string> OnAnomalyFailState;       // anomalyId — escalation or GREEN fail

// Day flow
static event Action<int>    OnDayStarted;             // dayNumber
static event Action         OnSafeDayCleared;         // GREEN correctly pressed on safe day
static event Action         OnFalsePositive;          // RED pressed on safe day

// Health
static event Action         OnPlayerHit;              // any anomaly dealt damage
static event Action         OnPlayerDied;             // final death — trigger reset
```

---

## 8. Per-Anomaly Technical Specifications

---

### Anomaly 1 — BurningPlantAnomaly.cs

**Scene Objects Required:**
- Plant GameObject (always in scene)
- Fire particle system (child of plant, disabled at start)
- Fire radius trigger collider (grows over time)

**Serialised Fields:**
```
float fireSpreadRate        // Units per second the radius expands (default: 0.5)
float damageProximityRadius // Player distance to take damage (default: 1.5)
float damageInterval        // Seconds between damage ticks while in radius (default: 1.0)
```

**Activate():**
- Enable fire particle system on plant.
- Begin expanding fire radius collider via Update loop using fireSpreadRate.
- Start damage interval coroutine: if player within damageProximityRadius, call DealDamageToPlayer() every damageInterval seconds.

**Escalation:**
- Fire radius grows continuously until it fills the room (target radius: ~8 units).
- When radius reaches room fill threshold: TriggerFailState().

**Resolve():**
- Disable fire particle system.
- Stop radius expansion.
- Stop damage coroutine.
- Reset radius to 0.

**TriggerFailState():**
- Flash full fire screen overlay.
- BlackoutAndReset(1.5f).

**hitCountToKill:** Damage-over-time model. Not hit-based. Player dies when health reaches 0 via repeated DealDamageToPlayer() calls.

---

### Anomaly 2 — AttackingDollsAnomaly.cs

**Scene Objects Required:**
- 5x Doll GameObjects, each with a pre-authored rail path (Transform waypoints in scene)
- Each doll has a trigger collider for player contact detection

**Serialised Fields:**
```
float baseMovementSpeed       // Speed of first doll (default: 1.2)
float speedIncreasePerDoll    // Added to speed for each successive doll (default: 0.4)
float activationInterval      // Seconds between each doll activating (default: 8.0)
Transform[] dollWaypoints     // Array of waypoints per doll, set in Inspector
```

**Activate():**
- Start coroutine: activate dolls one at a time separated by activationInterval.
- Each doll moves along its fixed waypoint path toward player at current speed.
- Speed = baseMovementSpeed + (dollIndex * speedIncreasePerDoll).

**Escalation:**
- Dolls activate sequentially. Each successive doll is faster.
- If all 5 dolls have been active and player is still alive: loop speed continues scaling but no new dolls spawn.

**Player Contact:**
- Each doll trigger: OnTriggerEnter with Player tag → DealDamageToPlayer().
- Cooldown between hits from same doll: 1.0 seconds (prevent rapid hit stacking).

**hitCountToKill:** 2–3 (uses shared HealthSystem).

**Resolve():**
- All active dolls freeze (stop movement).
- Play return animation or lerp back to starting positions over 1.5 seconds.
- Disable all doll movement scripts.

**TriggerFailState():**
- Screen vignette goes full red.
- BlackoutAndReset(1.0f).

**Note for Codex:** Dolls use waypoint-based movement only. No NavMesh, no pathfinding. Each doll has its own array of Transform waypoints assigned in the Inspector. Movement = lerp between current waypoint and next waypoint.

---

### Anomaly 3 — ElectricityShortageAnomaly.cs

**Scene Objects Required:**
- All room light sources (references stored in array)
- Ambient light source (always on, low intensity)

**Serialised Fields:**
```
float flickerWindowDuration   // Total seconds before fail state (default: 25.0)
float flickerOnDuration       // Seconds lights stay on per cycle (default: 1.2)
float flickerOffDuration      // Seconds lights stay off per cycle (default: 0.8)
float ambientIntensity        // Light intensity during off phase (default: 0.05)
float normalIntensity         // Light intensity during on phase (default: 1.0)
```

**Activate():**
- Store current remaining time = flickerWindowDuration.
- Begin flicker coroutine: alternate all room lights between normalIntensity and ambientIntensity on fixed interval.
- Begin countdown. If countdown reaches 0 before RED pressed: TriggerFailState().

**Escalation:**
- Lights flicker on regular repeating pattern. No change to pattern over time.
- RED button glow remains active throughout (player navigation anchor).

**Resolve():**
- Stop flicker coroutine.
- Restore all lights to normalIntensity.

**TriggerFailState():**
- All lights cut to zero instantly.
- BlackoutAndReset(0.5f).

**hitCountToKill:** N/A — this anomaly has no contact damage. Fail state is timer-only.

---

### Anomaly 4 — BleedingFrameAnomaly.cs

**Scene Objects Required:**
- Photo frame GameObject (always on same wall — position never changes)
- Frame material with blood spread shader parameter
- Frame Rigidbody (kinematic, switched to non-kinematic in Stage 2)

**Serialised Fields:**
```
float stage1Duration          // Observation window in seconds (default: 60.0)
float bloodSpreadRate         // Shader parameter fill speed (default: 0.015 per second)
float floatSpeed              // Frame movement speed in Stage 2 (default: 2.0)
float shakeIntensity          // Frame shake amplitude in Stage 1 (default: 0.04)
float shakeFrequency          // Frame shake frequency in Stage 1 (default: 18.0)
```

**Stage 1 — Activate():**
- Begin blood spread: lerp shader fill parameter from 0 to 1 over stage1Duration.
- Begin shake: oscillate frame position using shakeIntensity and shakeFrequency.
- Bleed rate is slow at start, accelerates as stage1Duration approaches zero.
- If stage1Duration expires without RED press: begin Stage 2.

**Stage 2:**
- Frame detaches from wall (disable wall anchor, enable Rigidbody physics briefly then switch to scripted movement).
- Frame moves toward player on a fixed arc/bounce path using floatSpeed.
- On contact with player: DealDamageToPlayer(). Contact cooldown: 1.2 seconds.

**hitCountToKill:** 2–3 (shared HealthSystem).

**Resolve():**
- Stop all shake and movement.
- Frame snaps back to wall position.
- Blood shader parameter resets to 0.

**TriggerFailState():**
- Screen flash red.
- BlackoutAndReset(1.0f).

---

### Anomaly 5 — ContaminatedBathroomAnomaly.cs

**Scene Objects Required:**
- Bathroom wall/tile materials (swappable to stained variants)
- Bathroom audio source (cracking sounds)
- Screen contamination effect (full-screen shader or overlay image — used for GREEN fail state)

**Serialised Fields:**
```
float stainSpreadRate         // Rate at which stain material coverage grows (default: 0.02 per second)
float crackAudioFadeInTime    // Seconds to reach full cracking volume (default: 10.0)
float failStateTimer          // Seconds before walls shatter if RED not pressed (default: 45.0)
float contaminationOverlayDuration  // Duration of contamination visual on GREEN press (default: 1.5)
```

**Activate():**
- Begin stain spread on bathroom surfaces (lerp material stain parameter).
- Begin cracking audio fade-in.
- Start failStateTimer countdown.

**Escalation:**
- Stain spreads from drain/sink outward.
- Cracking audio grows in volume over time.
- If failStateTimer reaches 0: TriggerFailState() via timer path.

**Resolve():**
- Stop stain spread. Reset stain parameter.
- Stop and reset cracking audio.

**TriggerFailState() — Timer Path:**
- Cracking audio peaks instantly.
- Bathroom wall shatter visual (particle burst or material swap).
- BlackoutAndReset(1.0f).

**TriggerFailState() — GREEN Pressed Path:**
- Screen fade briefly.
- Play contamination overlay (dirty water ripple / red mist full-screen effect).
- Camera shake for 0.8 seconds.
- BlackoutAndReset(1.2f).

**Note:** Both fail paths call TriggerFailState() but accept an enum parameter to differentiate the two visual sequences:
```csharp
enum FailReason { TimerExpired, GreenPressed }
override void TriggerFailState(FailReason reason)
```

---

### Anomaly 6 — BackwardClockAnomaly.cs

**Scene Objects Required:**
- Wall clock GameObject (always in living room scene)
- Clock hour hand Transform
- Clock minute hand Transform
- Clock audio source (ticking sound — forward on normal days, reversed on anomaly day)

**Serialised Fields:**
```
float normalTickInterval      // Seconds per tick forward on safe days (default: 1.0)
float reverseTickSpeed        // Initial backward rotation speed (default: -1.0)
float tempoEscalationRate     // How fast tick speed increases over time (default: 0.08 per second)
float maxReverseSpeed         // Speed cap before Shatter triggers (default: -12.0)
float shatterTriggerThreshold // Absolute speed value that triggers Shatter (default: 11.5)
```

**Idle Behaviour (non-anomaly days):**
- Clock ticks forward normally using normalTickInterval.
- This script runs passively at low cost on all non-anomaly days to keep clock hands moving.
- This is NOT managed by AnomalyManager — clock idle behaviour is always-on.

**Activate():**
- Switch ticking audio to reversed clip.
- Begin counter-clockwise rotation of clock hands.
- Begin escalating reverseTickSpeed over time using tempoEscalationRate.
- Pitch of ticking audio increases in sync with speed.

**Escalation:**
- Speed and pitch increase continuously.
- When absolute speed exceeds shatterTriggerThreshold: TriggerFailState().

**Resolve():**
- Stop reverse rotation.
- Switch audio back to forward tick.
- Snap hands to current real-time position (or fixed position).
- Reset speed to normalTickInterval.

**TriggerFailState() — "The Shatter":**
1. Clock hands spin to maximum speed with high-pitched screech (audio).
2. Clock explodes off wall (particle burst, clock GameObject deactivates).
3. In the **same frame** as explosion: all registered room objects teleport to randomised positions within their allowed bounds. (EnvironmentController.ScatterAllObjects() — see Systems Overview.)
4. One frame held on scattered chaos.
5. Immediate BlackoutAndReset(0.0f) — no delay. Instant cut to black.

**Note for Codex:** Step 3 requires EnvironmentController to maintain a list of all scatterable objects and their allowed position bounds. BackwardClockAnomaly calls EnvironmentController.ScatterAllObjects() directly (this is the one permitted direct system call outside EventBus, due to frame-timing requirements).

---

### Anomaly 7 — CorruptedTextAnomaly.cs

**Day Constraint:** Cannot be assigned before Day 3. See Section 9.

**Scene Objects Required:**
- All readable text objects registered in an array (posters, door labels, frames with text)
- Each text object has an original TextMesh/TMP component and a corrupted variant
- Full-screen text jumpscare overlay (Canvas UI element, disabled by default)

**Serialised Fields:**
```
float corruptionSpreadInterval   // Seconds between each new text object becoming corrupted (default: 8.0)
float fullscreenJumpscareDelay   // Seconds after final object corrupts before jumpscare (default: 3.0)
string[] alarmingWords           // Always-included alarming words pool e.g. {"HELP","RUN","DYING"}
```

**Activate():**
- Begin corruption spread coroutine.
- Every corruptionSpreadInterval seconds, one additional text object corrupts.
- First object to corrupt always contains one word from alarmingWords array (guaranteed detection signal).
- Subsequent corruptions: random mix of scribbles, threatening messages, mirrored text.

**Escalation:**
- Text corruption spreads object by object across the environment.
- After all text objects are corrupted: wait fullscreenJumpscareDelay seconds.
- Then: enable full-screen text jumpscare overlay → TriggerFailState().

**Resolve():**
- Stop corruption coroutine.
- Revert all corrupted text objects to their original content.
- Disable jumpscare overlay if active.

**TriggerFailState() — Jumpscare Path (escalation complete):**
- Full-screen text overlay fills screen.
- Hold for 0.5 seconds.
- BlackoutAndReset(0.5f).

**TriggerFailState() — GREEN Pressed Path:**
- Immediate BlackoutAndReset(0.3f). No jumpscare visual — instant cut.

---

## 9. Anomaly Assignment Rules

This logic lives inside AnomalyManager.AssignAnomalyDays():

```
Step 1: Determine safe day count
        safeDayCount = Random.Range(0,2) == 0 ? 2 : 3

Step 2: Create list of 7 day slots [1, 2, 3, 4, 5, 6, 7]

Step 3: Randomly select safeDayCount slots → mark as safe (-1)

Step 4: Remaining slots = anomaly slots
        Create anomaly pool = all 7 AnomalyType values

Step 5: Remove CorruptedText from pool temporarily

Step 6: Shuffle remaining pool
        Assign one anomaly per anomaly slot in order

Step 7: Re-insert CorruptedText
        Find all anomaly slots with day number >= 3
        Randomly pick one → assign CorruptedText
        If no anomaly slot exists on day 3 or later: 
            swap CorruptedText with the latest-day anomaly
            that is currently assigned to day 1 or 2

Step 8: Store final assignments in dayAssignments[7]
        dayAssignments[i] = anomaly index or -1 for safe days
```

---

## 10. Escalation Timer Model

There is no global anomaly timer. Each anomaly owns its own internal timing completely.

| Anomaly | Timer Type | Fail Trigger |
|---|---|---|
| Burning Plant | Continuous radius growth | Radius fills room |
| Attacking Dolls | Sequential activation interval | Player health depleted |
| Electricity Shortage | Countdown window | Window expires |
| Bleeding Frame | Stage 1 countdown + Stage 2 contact | Health depleted |
| Contaminated Bathroom | Countdown window | Window expires or GREEN pressed |
| Backward Clock | Speed escalation | Speed threshold crossed |
| Corrupted Text | Spread interval chain | All objects corrupted + delay |

All timing values are `[SerializeField]` floats. Default values ship in code. Tuning happens in the Inspector during playtesting — no code changes required.

---

## 11. Implementation Notes for Codex

These rules must be included in every anomaly-related Codex prompt:

1. **All anomaly scripts inherit from AnomalyBase.** No exceptions.
2. **No anomaly script may reference GameManager directly.** Use EventBus only. Exception: BackwardClockAnomaly may call EnvironmentController.ScatterAllObjects() directly.
3. **All scene object references are assigned in the Inspector.** No FindObjectOfType, no GetComponent on other GameObjects in Awake.
4. **All timing values are [SerializeField] floats with default values.** No hardcoded magic numbers in logic methods.
5. **Anomaly GameObjects are never Instantiated or Destroyed.** Enable/disable only.
6. **Each anomaly owns its fail state sequence completely.** AnomalyManager does not run death sequences.
7. **DealDamageToPlayer() is always called through the AnomalyBase protected method.** Never directly access HealthSystem from a concrete anomaly class.
8. **Coroutines must be stopped in both Resolve() and ResetAnomaly().** Use StopAllCoroutines() as a safety measure in ResetAnomaly().
9. **FailReason enum parameter on TriggerFailState() applies only to ContaminatedBathroomAnomaly.** All other anomalies use the standard parameterless signature.
10. **BackwardClockAnomaly has idle behaviour on non-anomaly days.** This is handled by a separate lightweight always-on component (ClockIdleBehaviour.cs) that is NOT managed by AnomalyManager.
