# Game Design Document
## Project: [WORKING TITLE — TBD]
**Engine:** Unity 2022.3 LTS | **Pipeline:** URP | **Target:** PC Standalone  
**Jam Theme:** The More You Use It, The Worse It Gets  
**Secondary Theme (Backup):** Cliché  
**Format:** Single-player | Horror | Anomaly Detection  
**Scope:** 48-hour GameJam build

---

## 1. Concept

The player is a lone observer living in a small house — a metaphorical representation of Planet Earth. Each day, disasters occur in the world, manifested as anomalies inside the player's home. The player must patrol the house, detect these anomalies, and press the RED Neutralise Button to regulate and contain each event before it escalates.

If the world is functioning normally, the player presses the GREEN All-Okay Button to pass the day safely.

Survive all 7 days with correct decisions. Save the world.

---

## 2. Core Theme Mechanic — "The More You Use It, The Worse It Gets"

The **RED Button** is "it." Every time the player successfully presses RED and neutralises an anomaly, the world's resistance to regulation increases.

**Mechanic — Expanding Button Pool (B2):**
- On Day 1, the RED button is placed randomly among **3 possible positions** in the environment.
- Each successful RED button press permanently adds **1 new position** to the random placement pool for future anomaly days.
- The pool **caps at 7 positions** (the maximum physical locations seeded in the environment).
- The GREEN button is always at a single fixed position. It never moves.

**Design Intent:**  
The act of saving the world makes finding the means to save it progressively harder. Early days are forgiving. Late days require the player to thoroughly search all rooms. The player is punished for being good at the game — a direct mechanical expression of the theme.

---

## 3. Narrative Frame

The player's room is a microcosm of Earth. Every anomaly inside the home corresponds to a global disaster happening in the outside world:

| Anomaly | Real-World Event |
|---|---|
| Plant catching fire | Forest fires / deforestation |
| Dolls attacking | Rise of dark/occult influence |
| Electricity flickering | Major power plant failure / energy crisis |
| Blood seeping from photo frame | Viral epidemic / disease outbreak |
| Bathroom deteriorating | Water contamination / sewage crisis |
| Clock ticking backwards | Disruption of natural order / entropy |
| Text becoming threatening | Information warfare / mass manipulation |

---

## 4. Game Structure

### 4.1 — Day Count
- **7 days total** per run.
- **2 or 3 days are anomaly-free** per run — randomised between these two values at the start of each playthrough.
- The player never knows in advance how many safe days exist or which days they fall on.

### 4.2 — Anomaly-to-Day Mapping
- Anomaly types are assigned to days **completely at random** each playthrough.
- **Exception:** The Text Anomaly (Anomaly 7) is locked to appear **no earlier than Day 3** to ensure the player has had prior exposure to the original text in the environment.
- No anomaly type repeats within a single 7-day run.

### 4.3 — Day Cycle Flow

```
SPAWN ON BALCONY
       ↓
Observe balcony plants (progress tracker)
       ↓
Player manually walks through door → enters Living Room
       ↓
[ANOMALY DAY]                    [SAFE DAY]
Search rooms for anomaly         No anomaly present
Find RED button, press it        Find GREEN button, press it
       ↓                                ↓
Anomaly neutralised              Day passes safely
       ↓                                ↓
          New plant blooms on balcony
                    ↓
             Next day begins
```

### 4.4 — Win Condition
Player correctly presses RED on all anomaly days and GREEN on all safe days across all 7 days.

**Win Screen:** Fade to black → "THE WORLD IS SAVED" title card → All 7 balcony plants shown fully glowing → Credits roll.

### 4.5 — Lose Condition
Player dies at any point during days 1–7.

**On death:** All plants on the balcony reset to dead/dry. Player resets to Day 1. Anomaly-to-day mapping is re-randomised. Button pool resets to 3.

---

## 5. Player Controls & Interactions

The player's interaction set is intentionally minimal:

| Input | Action |
|---|---|
| WASD / Arrow Keys | Move through the environment |
| Mouse | Look / orient camera |
| E (proximity) | Press RED button or GREEN button |

**No other interactions exist.** The player cannot open doors, pick up objects, crouch, sprint, or interact with any environment element beyond the two buttons. All anomaly escalation is observational — the player watches, navigates, and decides.

---

## 6. Environment Layout

```
[BALCONY] — Player spawn point. 7 plant slots (dead → alive per completed day).
     |
[FRONT DOOR — always open]
     |
[LIVING ROOM] — Main observable space. Contains most anomaly objects.
     |
[BATHROOM DOOR — always open]
     |
[BATHROOM] — Secondary room. Site of Anomaly 5.
```

- All doors are **permanently open** — no door interaction required.
- The bathroom is always partially visible from the living room through the open doorway.
- The RED button spawns at one of its pool positions across these rooms.
- The GREEN button is always at one fixed position in the living room.

---

## 7. The Two Buttons

### RED Button — Neutralise
- Always physically present in the environment, every day (both anomaly and safe days).
- On **anomaly days:** glows and pulses visibly.
- On **safe days:** still physically present and glowing. This is intentional — the player cannot use button glow-state as a shortcut to identify safe days.
- Placed randomly within the current **button pool** (starts at 3 positions, grows by 1 per RED press, caps at 7).

### GREEN Button — All Okay
- Always at one fixed position in the living room.
- Never glows or changes state.
- Pressing it on an anomaly day triggers a death event specific to the active anomaly's fail state.

---

## 8. Health System

- Player has a **soft health buffer** of **2–3 hits** depending on anomaly.
- **No explicit health UI.**
- **Visual indicator:** Screen corners begin vignetting red after the first hit. Redness intensifies with each subsequent hit. Final hit triggers blackout death.
- **Health resets fully at the start of each new day.**
- The red vignette also fully clears between days.

---

## 9. False Positive — Pressing RED on a Safe Day

**Sequence:**
1. Player presses RED on a day with no active anomaly.
2. Nothing happens for exactly **5 seconds.** Room is silent.
3. On the 5th second: a devil/demonic entity spawns directly in front of the player.
4. Player is **immediately frozen** — all movement and input locked.
5. Devil runs toward the frozen player.
6. On contact: instant blackout. Reset to Day 1.

**Design intent:** Pure punishing jumpscare. No cancel window, no recovery. The freeze amplifies helplessness and makes the mistake feel viscerally consequential. This moment should be the most frightening thing in the game.

**Implementation note:** This sequence can be implemented either via a C# scripted timeline or Unity's Timeline/Animator. Timeline is recommended for the jump scare sequencing precision — flag for Codex prompt.

---

## 10. Progress Tracker — Balcony Plants

- The balcony contains **7 plant slots**, all starting as dry/dead at the beginning of a run.
- After each successfully completed day, the corresponding plant slot transitions from dead to a **fresh, glowing, living plant.**
- On player death and reset, **all plants revert to dead.** The balcony resets fully.
- This system is purely visual — no gameplay mechanics attach to plant count.

---

## 11. Anomaly Definitions

Each anomaly has its own internal timer/rate. There is no shared global timer across anomalies. Escalation speed, spread rate, and kill timing are all anomaly-specific and tuned during playtesting.

---

### Anomaly 1 — The Burning Plant (Forest Fire / Deforestation)

**Trigger:** The living room plant spontaneously ignites.

**Escalation:**
- Fire begins on the plant.
- Spreads progressively outward through the room (particle-based spread, not per-tile simulation).
- Fire radius expands over time until it fills the room.

**Player Interaction with Fire:**
- Player takes damage if within the fire's radius (proximity damage zone).
- Player can still navigate around the room — fire does not instantly block all paths.
- Navigation challenge: finding the RED button while avoiding the expanding fire radius.

**Fail State:** Fire fills the entire room. Player is engulfed. Blackout. Reset to Day 1.

**After RED Pressed:** Fire extinguishes immediately. Day ends. Next day begins.

---

### Anomaly 2 — The Attacking Dolls (Rise of Dark Influence)

**Trigger:** Dolls and action figures in the room begin moving.

**Escalation:**
- 5 doll/figure entities are present in the room.
- They activate and begin moving toward the player **one at a time**, not simultaneously.
- Each moves along a **fixed rail path** (no dynamic pathfinding) — speed increases with each successive doll activation.
- On contact with player: 1 hit of damage (screen redness system).

**Player Interaction:**
- Spatial avoidance only — player must navigate around approaching dolls while searching for RED button.
- No attack input, no block input. Movement only.

**Fail State:** Player takes 2–3 cumulative hits. Blackout. Reset to Day 1.

**After RED Pressed:** All dolls immediately freeze. Animate back to starting positions. Day ends.

---

### Anomaly 3 — Electricity Shortage (Power Plant Failure)

**Trigger:** Room lights begin flickering.

**Escalation:**
- Lights flicker on a regular pattern (not chaotic — regular interval for jam scope).
- During dark intervals: room dims to **ambient light only** — enough to navigate but not comfortably.
- The RED button glows during both lit and dim intervals, providing a navigation anchor.

**Player Interaction:**
- Player must navigate to the RED button within the flickering window.
- No enemy, no damage from the environment itself.

**Fail State:** Flickering window expires. Instant blackout death. Reset to Day 1.  
*(Timer duration to be tuned during playtesting — placeholder: 25 seconds.)*

**After RED Pressed:** Lights stabilise immediately. Day ends.

---

### Anomaly 4 — The Bleeding Frame (Viral Epidemic)

**Trigger:** A photo frame on the living room wall begins bleeding.

**Escalation — Stage 1 (Observation Window):**
- Blood slowly seeps through the frame surface.
- Frame begins vibrating/shaking on the wall.
- Duration: approximately 60 seconds (tuned to 2–3 minutes based on playtesting).
- Bleeding rate scales inversely with time remaining in Stage 1 (faster near end of window).

**Escalation — Stage 2 (Active Threat):**
- Frame detaches from the wall and begins **floating through the air**.
- Moves toward the player along a fixed arc/bounce pattern (no dynamic pathfinding).
- On contact: 1 hit of damage (shared screen redness health system, same as dolls).
- Frame requires **2–3 hits** to kill the player (same hit count as dolls).

**Frame Consistency:** The photo frame is always on the **same wall** in every playthrough. Its starting position never changes. Players can rely on this.

**Fail State:** Player takes 2–3 cumulative hits from the frame. Blackout. Reset to Day 1.

**After RED Pressed:** Frame drops to the floor. Blood stops. Day ends.

---

### Anomaly 5 — The Contaminated Bathroom (Water/Sewage Crisis)

**Trigger:** The bathroom begins visibly and audibly deteriorating.

**Detection:**
- Bathroom door is always open — bathroom is always partially visible from the living room.
- Visual cue: bathroom surfaces, toilet paper, and walls develop a deep red/rust stain spreading outward.
- Audio cue: cracking and splitting sounds emit from the bathroom continuously.
- The combined audio and visual signal is designed to draw the player's attention without requiring mandatory room entry.

**Player Interaction:**
- Player walks into the bathroom to confirm the anomaly.
- Locates and presses the RED button (somewhere in the expanded pool).

**Fail State — Pressing GREEN while bathroom is active:**
- Player presses All-Okay.
- Screen fades briefly, then a contamination wave effect washes across the screen (visual: dirty water ripple or red mist).
- Player convulses (brief camera shake/distortion).
- Blackout. Reset to Day 1.

**Fail State — Timer expiry:**
- Cracking sounds intensify to maximum.
- Bathroom walls shatter visually.
- Blackout. Reset to Day 1.

**After RED Pressed:** Staining recedes. Sounds stop. Day ends.

---

### Anomaly 6 — The Backward Clock (Disruption of Natural Order)

**Trigger:** The wall clock in the living room begins ticking backwards.

**Detection:**
- Clock is always present in the living room, ticking normally on non-anomaly days.
- On anomaly day: clock hands begin moving counter-clockwise.
- Ticking sound plays **backwards** — subtle at first.
- As time passes: ticking tempo and pitch **increase progressively**, becoming impossible to ignore.

**Player Interaction:**
- Player identifies the anomaly by observing/hearing the clock.
- Locates and presses the RED button.

**Fail State — "The Shatter":**
- Clock hands spin to maximum speed with a high-pitched screech.
- Clock **explodes off the wall** with a sharp crack sound.
- In the **exact same frame** as the explosion: every object in the room (dolls, plants, frames, furniture) simultaneously teleports to a completely randomised position. One frame of total visual chaos.
- Immediate blackout death. Reset to Day 1.

**Design intent:** The jumpscare is not a creature — it is the room itself breaking. The single frame of total chaos should feel like a glitch in reality.

**After RED Pressed:** Clock hands reset to current time. Ticking normalises. Day ends.

---

### Anomaly 7 — The Corrupted Text (Information Warfare)

**Day Constraint:** Cannot appear before Day 3. Assigned randomly to Day 3, 4, 5, 6, or 7.

**Trigger:** Text throughout the environment begins changing.

**Detection:**
- Text on posters, doors, labels, and other readable surfaces the player has seen on prior days begins mutating.
- Changes include: random scribbles replacing words, threatening messages, reversed/mirrored text, and specific words replaced with alarming ones.
- **At least one piece of text always spells a clearly alarming word** (e.g. "HELP", "RUN", "DYING") — this ensures first-time players can detect the anomaly even without having memorised every original text string.

**Escalation:**
- Text corruption begins on a single object.
- Spreads progressively to more surfaces across the environment over time.
- Eventually: corrupted text appears **directly in front of the player's face** as a full-screen jumpscare — overlaid text fills the entire screen.
- Contact with the text jumpscare = instant blackout death.

**Fail State — Pressing GREEN while text is active:**
- Player presses All-Okay.
- Immediate blackout. Reset to Day 1.

**Fail State — Escalation completes:**
- Full-screen text jumpscare. Blackout. Reset to Day 1.

**After RED Pressed:** All text immediately reverts to original content. Day ends.

---

## 12. Systems Overview (For Implementation Reference)

| System | Responsibility |
|---|---|
| `GameManager` | Day counter, safe/anomaly day assignment, win/lose state, button pool size tracking |
| `AnomalyManager` | Anomaly type assignment per day, anomaly activation, escalation timers, fail state triggers |
| `ButtonManager` | RED button position pool, random placement per anomaly day, proximity detection |
| `HealthSystem` | Hit tracking, screen vignette controller, death trigger, per-day reset |
| `ProgressTracker` | Balcony plant states, update on day complete, full reset on death |
| `FalsePositiveController` | Devil spawn sequence, player freeze, 5-second delay logic |
| `EnvironmentController` | Room state management, object position randomisation (Clock anomaly), texture/material swaps |
| Per-Anomaly Scripts | One script per anomaly type, owns escalation logic, owns fail state, communicates via EventBus |

---

## 13. Scope Boundaries — What Is Explicitly Out of Scope

The following will **not** be implemented for the jam build:

- Dynamic door opening/closing interactions
- Player abilities beyond movement and button press
- Pathfinding AI for dolls or the frame (fixed rail/arc paths only)
- Per-tile fire spread simulation (particle effect expansion only)
- Health UI (vignette only)
- Difficulty settings or menu options beyond Start / Quit
- More than 2 rooms + balcony (contingent on available Tripo AI assets)

---

## 14. Asset Notes — Tripo AI

All 3D assets are AI-generated via Tripo AI. Minimum required asset list:

- Living room furniture set (sofa, table, shelving)
- Wall clock
- Potted plant (dead variant + living/glowing variant)
- Photo frame
- 5x doll / action figure models
- Bathroom fixtures (toilet, sink, tiles)
- Balcony railing + 7 plant slot props
- Devil / demonic entity (false positive sequence)
- RED button prop
- GREEN button prop

Tripo AI generation should run **in parallel** with Codex code generation during Sprint 1.
