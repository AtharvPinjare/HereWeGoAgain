# Game Design Document
### Project Codename: TBD
---
**Engine:** Unity 2022.3 LTS
**Render Pipeline:** URP (manually upgraded from Built-in — visually confirmed working)
**Platform:** PC Standalone
**Jam Theme:** The More You Use It, The Worse It Gets
**Genre:** First-Person Horror / Anomaly Detection
**Scope:** 48-Hour GameJam Build
**Environment Asset:** Apartment Kit by Brick Project Studio (v4.2)
**Supplementary Assets:** Tripo AI (decorative props, dolls, buttons, devil entity)
**Development Pipeline:** OpenAI Codex + Unity MCP

---

## Table of Contents
1. Concept & Narrative
2. Core Theme Mechanic
3. Game Structure
4. Player Controls & Interactions
5. Environment Layout
6. The Two Buttons
7. Health System
8. False Positive — Pressing RED on a Safe Day
9. Progress Tracker — Ground Floor Lobby Plants
10. Anomaly Definitions
11. Win & Lose Conditions
12. Systems Overview
13. Out of Scope
14. Asset Notes

---

## 1. Concept & Narrative

The player inhabits a multi-floor apartment building — a microcosm of Planet Earth. Each day,
disasters unfold across the world and their consequences manifest as physical anomalies inside
the apartment. The player's role is to patrol the building, correctly detect whether a disaster
is occurring, and respond with one of two actions:

- Press the **RED Neutralise Button** to regulate the anomaly and contain the disaster.
- Press the **GREEN All-Okay Button** to confirm the day is safe and move on.

Survive all 7 days with correct decisions. Save the world.

### Anomaly-to-World Metaphor

| In-Game Anomaly | Room | Real-World Disaster |
|---|---|---|
| Plant catching fire | Living Area | Forest fires / deforestation |
| Dolls attacking the player | Bedroom 1 | Rise of dark / occult influence |
| Electricity flickering | Entire apartment | Major power plant failure |
| Blood seeping from photo frame | Living Area | Viral epidemic / disease outbreak |
| Bathroom deteriorating | Bathtub Bathroom (locked) | Water contamination / sewage crisis |
| Clock ticking backwards | Living Area | Disruption of natural order / entropy |
| Text becoming threatening | Throughout apartment | Information warfare / mass manipulation |

---

## 2. Core Theme Mechanic

**Theme:** The More You Use It, The Worse It Gets
**"It":** The RED Neutralise Button.
**"The Worse It Gets":** The world's resistance to regulation grows with every intervention.

### Mechanic — Expanding RED Button Pool

Every time the player successfully presses RED and neutralises an anomaly, one new position
is permanently added to the pool of locations where the RED button can randomly appear in
future anomaly days.

| Successful RED Presses | Button Pool Size |
|---|---|
| 0 — Run Start | 3 positions |
| 1 | 4 positions |
| 2 | 5 positions |
| 3 | 6 positions |
| 4 | 7 positions |
| 5+ | 8 positions (hard cap) |

> **Note:** The cap has been raised from 7 to 8 to take advantage of the expanded apartment
> environment. The larger building provides more natural, distributed button placement locations
> across multiple rooms and floors.

- Each anomaly day, the RED button is placed at **one randomly selected position** from within
  the current pool.
- The GREEN button is always at one **fixed position** in the Ground Floor Lobby. It never moves.

**Design Intent:**
Saving the world early makes finding the means to save it progressively harder. The expanded
apartment space makes this tension more pronounced — by mid-run the player must search multiple
rooms across two floors. The act of regulation is itself the source of growing chaos — a direct
mechanical expression of the theme.

---

## 3. Game Structure

### 3.1 Day Count
- **7 days total** per run.
- **2 or 3 days are anomaly-free** — randomised between these two values at the start of each
  playthrough.
- The player never knows in advance how many safe days exist or which days they fall on.

### 3.2 Anomaly-to-Day Assignment
- Anomaly types are assigned to days **completely randomly** at the start of each playthrough.
- **Single exception:** The Text Anomaly (Anomaly 7) is locked to appear **no earlier than
  Day 3**, ensuring the player has had prior exposure to original text before it becomes corrupted.
- No anomaly type repeats within a single 7-day run.

### 3.3 Daily Flow

```
[DAY BEGINS]
      |
Player spawns in Ground Floor Lobby
Observes lobby plant progress tracker
Opens apartment entrance door → enters building
      |
      |—————————————————————|
      |                     |
[ANOMALY DAY]          [SAFE DAY]
Search apartment       No anomaly present
Find and press RED     Find and press GREEN
      |                     |
      |—————————————————————|
      |
Day resolved successfully
One lobby plant blooms
      |
[NEXT DAY BEGINS]
```

### 3.4 Anomaly Timer Model
There is **no single global timer**. Each anomaly has its own independent internal rate and
timing. All timing values are `[SerializeField]` floats — tunable in Inspector without code
changes.

| Anomaly | Timer Type | Fail Trigger |
|---|---|---|
| Burning Plant | Continuous radius growth | Radius fills room |
| Attacking Dolls | Sequential activation interval | Player health depleted |
| Electricity Shortage | Countdown window | Window expires |
| Bleeding Frame | Stage 1 countdown + Stage 2 contact | Health depleted |
| Contaminated Bathroom | Countdown window | Window/GREEN press |
| Backward Clock | Speed escalation | Speed threshold crossed |
| Corrupted Text | Spread interval chain | All objects corrupted + delay |

---

## 4. Player Controls & Interactions

The interaction set is minimal by design. The Apartment Kit's built-in FPS controller is used
directly with no modification.

| Input | Action |
|---|---|
| WASD / Arrow Keys | Move |
| Mouse | Look (camera) |
| E key (proximity to button) | Press RED button or GREEN button |
| E key (proximity to door) | Open / close door (Apartment Kit built-in) |

**Permitted interactions:**
- Movement and camera look
- RED and GREEN button press
- Door open/close via Apartment Kit's native interaction system

**No other interactions exist.** The player cannot pick up objects, sprint, crouch, or interact
with any environment element beyond doors and the two buttons. All anomaly detection is purely
observational.

### Door Interaction Design Note
Doors use the Apartment Kit's native interaction system without modification. This provides two
benefits:
1. Zero additional scripting required for door functionality.
2. The Bathtub Bathroom's glass pane door provides partial visibility of the bathroom interior
   before the player opens it — supporting the detection design of Anomaly 5.

**Do not override or disable the Apartment Kit's door scripts.**

---

## 5. Environment Layout

The apartment is a two-floor structure provided entirely by the Apartment Kit asset. No
structural modifications are made to the geometry.

```
GROUND FLOOR LOBBY
├── Player spawn point (start of every day)
├── 7 plant slots (progress tracker — dead → alive per completed day)
├── GREEN button — single fixed position (never moves)
└── Main apartment entrance door (interactable)

APARTMENT — 1ST FLOOR
├── Living Area
│   ├── Primary anomaly space
│   ├── Potted plant (Anomaly 1 — Burning Plant)
│   ├── Photo frame on fixed wall (Anomaly 4 — Bleeding Frame)
│   └── Wall clock on fixed wall (Anomaly 6 — Backward Clock)
├── Kitchen
│   └── Search space — RED button candidate position
├── Hallway / Corridor
│   └── Search space — RED button candidate position
├── Bedroom 1
│   ├── 5× doll / action figure props (Anomaly 2 — Attacking Dolls)
│   └── RED button candidate position
├── Bedroom 2
│   └── RED button candidate position
├── Bathtub Bathroom ← LOCKED FOR ANOMALY 5
│   ├── Glass pane door (Apartment Kit built-in interactable)
│   ├── Bathtub, sink, wall tiles (anomaly surfaces)
│   └── RED button candidate position
├── Second Bathroom
│   └── RED button candidate position
└── Empty Rooms (×2)
    └── RED button candidate positions
```

### RED Button — Physical Position Pool (8 Total)

| Index | Location |
|---|---|
| 1 | Living Area — near plant |
| 2 | Living Area — near photo frame wall |
| 3 | Kitchen — counter area |
| 4 | Hallway — mid-corridor |
| 5 | Bedroom 1 — beside doll shelf |
| 6 | Bedroom 2 — near window |
| 7 | Bathtub Bathroom — beside sink |
| 8 | Second Bathroom or Empty Room area |

> Positions are placed as named Transform GameObjects in the scene during setup.
> Exact placement is adjusted to fit real apartment geometry during scene wiring.

---

## 6. The Two Buttons

### RED Button — Neutralise
- Physically present in the apartment **every day** — both anomaly days and safe days.
- **Always glows and pulses** regardless of whether an anomaly is active.
- Glow state is intentionally consistent: players cannot use button presence or glow to
  identify safe days. Environmental observation is always required.
- Placed at one randomly selected position from the current pool at the start of each day.
- Pool starts at 3 positions, grows by 1 per successful RED press, caps at 8.

### GREEN Button — All Okay
- Fixed position in the **Ground Floor Lobby**. Never moves.
- Does not glow or change visual state.
- Correct to press on safe days.
- Pressing it during an active anomaly triggers that anomaly's specific fail state.

---

## 7. Health System

- Player has a soft health buffer of **2–3 hits** before death, varying by anomaly.
- **No health UI is displayed.**
- **Visual feedback only:** Screen corner vignette turns red after the first hit. Redness
  intensifies with each subsequent hit. Final hit triggers blackout death.
- Health **fully resets** at the start of each new day. Red vignette clears completely.

---

## 8. False Positive — Pressing RED on a Safe Day

**Sequence:**
1. Player presses RED on a day with no active anomaly.
2. **5 seconds of silence.** Nothing happens. Apartment is completely still.
3. At exactly the 5-second mark: a devil/demonic entity spawns directly in front of the player.
4. **Player is immediately frozen.** All movement and input locks completely.
5. Devil runs toward the frozen, helpless player.
6. On contact: instant blackout. Reset to Day 1.

**No cancel window. No recovery.** The freeze is intentional — helplessness amplifies the scare
and makes the false positive feel viscerally different from a standard anomaly death.

**Implementation Note:** Implemented via **Unity Timeline** for frame-accurate jumpscare
precision. Devil model sourced from Tripo AI.

---

## 9. Progress Tracker — Ground Floor Lobby Plants

> **Design Change:** The original balcony plant tracker concept has been replaced by a
> Ground Floor Lobby plant tracker. The lobby serves as the player's persistent spawn and
> progress display space — functionally identical to the original balcony concept, repositioned
> to match the apartment kit's actual architecture.

- The Ground Floor Lobby contains **7 plant slots**, all beginning as dry and dead at run start.
- After each successfully completed day, the corresponding plant slot transitions from dead to
  a **fresh, glowing, living plant**.
- Player spawns in the lobby at the start of every day and observes plant state before entering
  the apartment.
- On player death: **all plants revert to dead.** Lobby resets fully.
- Plants are visual only — no gameplay mechanics attach to plant count or state.

---

## 10. Anomaly Definitions

Each anomaly has its own internal timer and escalation rate. All timing values are
Inspector-tunable `[SerializeField]` floats with sensible defaults.

---

### Anomaly 1 — The Burning Plant
**Location:** Living Area
**Represents:** Forest fires / deforestation

**Trigger:** The living area potted plant spontaneously ignites.

**Escalation:**
- Fire originates on the plant and spreads via a particle-based expanding radius.
- Radius grows continuously until it fills the living area.
- Player takes proximity damage when within the fire radius.
- Player can still navigate — fire does not instantly block all paths.

**Fail State:** Fire fills the living area. Player engulfed. Blackout → Reset to Day 1.
**On RED Pressed:** Fire extinguishes instantly. Day ends.

---

### Anomaly 2 — The Attacking Dolls
**Location:** Bedroom 1 (primary), may extend into hallway via waypoints
**Represents:** Rise of dark / occult influence

**Trigger:** Doll and action figure props in Bedroom 1 begin moving.

**Escalation:**
- 5 doll entities placed in Bedroom 1 at scene setup (Tripo AI models).
- Activate one at a time, each faster than the last.
- Each moves along a **fixed waypoint rail path** — no NavMesh, no dynamic pathfinding.
- Contact with player: 1 hit (shared vignette health system).

**Player Interaction:** Spatial avoidance only. No attack or dodge input exists.

**Fail State:** Player takes 2–3 cumulative hits. Blackout → Reset to Day 1.
**On RED Pressed:** All dolls freeze. Animate back to starting positions. Day ends.

---

### Anomaly 3 — Electricity Shortage
**Location:** Entire apartment — all lights
**Represents:** Major power plant failure

**Trigger:** All apartment lights begin flickering simultaneously.

**Escalation:**
- All room lights flicker on a regular, consistent interval pattern.
- During dark intervals: apartment drops to ambient light only — dim but navigable.
- RED button glow remains visible during dark intervals — navigation anchor for the player.
- No enemy spawns. No contact damage.

**Fail State:** Flickering window expires. Instant blackout death → Reset to Day 1.
*(Placeholder window duration: 25 seconds. Tuned during playtesting.)*
**On RED Pressed:** All lights stabilise immediately. Day ends.

---

### Anomaly 4 — The Bleeding Frame
**Location:** Living Area — fixed wall (same wall every playthrough)
**Represents:** Viral epidemic / disease outbreak

**Trigger:** A photo frame on a fixed living area wall begins bleeding.

**Stage 1 — Observation Window (~60 seconds, tunable to 2–3 minutes):**
- Blood seeps slowly through the frame surface.
- Frame shakes and vibrates on the wall.
- Bleeding rate accelerates as the window approaches its end.
- Player can press RED at any point during Stage 1.

**Stage 2 — Active Threat (Stage 1 expires without RED press):**
- Frame detaches from the wall and floats through the air.
- Moves toward the player on a **fixed arc/bounce path** — no dynamic pathfinding.
- Contact: 1 hit per strike (shared vignette health system).
- 2–3 hits to kill.

**The frame is always on the same wall.** Position never changes between runs.

**Fail State:** Player takes 2–3 hits. Blackout → Reset to Day 1.
**On RED Pressed:** Frame drops. Blood stops. Day ends.

---

### Anomaly 5 — The Contaminated Bathroom
**Location:** Bathtub Bathroom ONLY — permanently locked to this room
**Represents:** Water contamination / sewage crisis

> **Asset Note:** The Bathtub Bathroom is specifically the bathroom containing the bathtub
> with the Apartment Kit's built-in glass pane door. This is the only bathroom used for
> this anomaly in every playthrough. The second bathroom is never the anomaly location.

**Trigger:** The Bathtub Bathroom begins visibly and audibly deteriorating.

**Detection:**
- Cracking and splitting sounds are audible from the hallway — drawing the player before entry.
- The glass pane door provides partial visibility of staining through the glass.
- Full confirmation requires the player to open the door and enter.
- Visual: deep red/rust staining spreads across tiles, bathtub, and walls from the drain outward.

**Fail State A — GREEN Pressed While Active:**
1. Player presses All-Okay.
2. Brief screen fade.
3. Contamination wave across screen (dirty water ripple / red mist).
4. Camera shake / distortion.
5. Blackout → Reset to Day 1.

**Fail State B — Timer Expires:**
- Cracking audio peaks.
- Bathroom walls shatter visually.
- Blackout → Reset to Day 1.

**On RED Pressed:** Staining recedes. Cracking stops. Day ends.

---

### Anomaly 6 — The Backward Clock
**Location:** Living Area — fixed wall clock (always same position)
**Represents:** Disruption of natural order / entropy

**Idle Behaviour (all non-anomaly days):** Clock ticks forward normally via a lightweight
always-on component (ClockIdleBehaviour.cs) — not managed by AnomalyManager.

**Trigger:** Clock hands begin moving counter-clockwise.

**Detection:**
- Hands move backwards — subtle at first.
- Reversed ticking audio plays, increasing in tempo and pitch over time.
- At full escalation, the sound is impossible to ignore from anywhere in the living area.

**Fail State — "The Shatter":**
1. Clock hands spin to maximum speed with a high-pitched screech.
2. Clock **explodes off the wall** — sharp crack audio, GameObject deactivates.
3. In the **exact same frame** as the explosion: every registered room object teleports to
   a completely randomised position within its allowed bounds. One frame of total visual chaos.
4. Immediate blackout. No delay. → Reset to Day 1.

**The jumpscare is the room itself breaking — not a creature.**

**On RED Pressed:** Clock hands reset to normal position. Ticking normalises. Day ends.

---

### Anomaly 7 — The Corrupted Text
**Day Constraint:** Cannot appear before Day 3.
**Location:** Throughout the entire apartment — all readable surfaces
**Represents:** Information warfare / mass manipulation

**Trigger:** Text throughout the apartment begins mutating.

**Detection:**
- Posters, door labels, bedroom signs, kitchen labels, and other readable surfaces change.
- Changes include: random scribbles, threatening messages, mirrored text, alarming word
  replacements.
- **At least one text object always displays a clearly alarming word** (HELP / RUN / DYING)
  — guaranteed detection signal regardless of prior memorisation.
- The apartment's large surface area means corruption can spread across significantly more
  objects than a single-room environment — this anomaly is stronger here than in the original
  small-house concept.

**Escalation:**
- Corruption spreads one object at a time across the apartment on a timed interval.
- After all text objects are corrupted: brief delay, then full-screen text jumpscare overlay.

**Fail State A — GREEN Pressed While Active:**
Immediate blackout → Reset to Day 1.

**Fail State B — Escalation Completes:**
Full-screen text overlay held for 0.5 seconds → Blackout → Reset to Day 1.

**On RED Pressed:** All text reverts to original content instantly. Day ends.

---

## 11. Win & Lose Conditions

### Win Condition
Player correctly presses RED on all anomaly days and GREEN on all safe days across all 7 days.

**Win Screen Sequence:**
1. Fade to black (2 seconds).
2. "THE WORLD IS SAVED" title card.
3. All 7 Ground Floor Lobby plants shown glowing simultaneously.
4. Credits text fades in below title card.
5. Return to Main Menu button appears after 8 seconds.

### Lose Condition
Player dies at any point during days 1–7 from any source:
- Anomaly fail state (timer expiry, contact damage, GREEN on anomaly day)
- False positive devil sequence

**On Death:**
- Active anomaly or FalsePositiveController plays its own blackout sequence.
- Screen held on black for 2 seconds.
- Brief "Day 1" reset indicator shown by UIManager.
- All systems reset. Run fully re-randomised. → Day 1.

---

## 12. Systems Overview

| System | Script | Responsibility |
|---|---|---|
| Game State Machine | GameManager.cs | Day counter, state transitions, button pool size, win/lose |
| Anomaly Coordination | AnomalyManager.cs | Day-to-anomaly assignment, activation, resolution routing |
| Button Interaction | ButtonManager.cs | Pool management, random placement, proximity, press routing |
| Player Health | HealthSystem.cs | Hit count, vignette control, death trigger, per-day reset |
| Progress Display | ProgressTracker.cs | Lobby plant states, bloom on complete, full reset on death |
| False Positive | FalsePositiveController.cs | 5s delay, devil spawn, player freeze, death trigger |
| Environment Reset | EnvironmentController.cs | Scatterable objects, ScatterAllObjects() for Clock anomaly |
| UI | UIManager.cs | Day display, win/lose screens, blackout fade — EventBus only |
| Anomaly Base | AnomalyBase.cs | Abstract contract all 7 anomaly scripts inherit from |
| Per-Anomaly Scripts | [Name]Anomaly.cs ×7 | Escalation, fail state, resolution, reset per anomaly |
| Clock Idle | ClockIdleBehaviour.cs | Always-on forward tick — independent of AnomalyManager |

---

## 13. Out of Scope

Explicitly cut for jam scope:

- Structural modification to Apartment Kit scene geometry
- Custom pathfinding or NavMesh for any entity
- Per-tile fire spread simulation (particle radius only)
- Visible health bar or health UI of any kind
- Difficulty settings or accessibility options
- Save system or run persistence between application sessions
- Multiple devil variants for false positive sequence
- Dynamic text generation (text changes are pre-authored TMP/texture swaps)
- Any modification to the Apartment Kit's FPS controller or door scripts

---

## 14. Asset Notes

### Apartment Kit — Brick Project Studio v4.2
- Provides: full multi-room apartment environment, all furniture and fixtures, built-in FPS
  controller, built-in door interaction system.
- URP pipeline upgrade applied manually — visually confirmed working.
- **Do not modify** the Apartment Kit's door interaction scripts or FPS controller.
  Build all game scripts to work alongside them, not replace them.

### Tripo AI Assets (Supplementary Only)
The Apartment Kit provides all environmental furniture. Tripo AI is used only for props not
included in the kit:

| Asset | Used For |
|---|---|
| 5× Doll / action figure models | Anomaly 2 — Attacking Dolls |
| Devil / demonic entity model | False Positive sequence |
| RED button prop | Player interaction point |
| GREEN button prop | Player interaction point |
| Dead plant variant (×7) | Lobby progress tracker — dead state |
| Alive / glowing plant variant (×7) | Lobby progress tracker — alive state |

> **Time constraint fallback:** If Tripo AI generation falls behind schedule, RED and GREEN
> buttons can ship as coloured emissive primitive cubes. Gameplay and scripting are entirely
> unaffected by button visual fidelity.
