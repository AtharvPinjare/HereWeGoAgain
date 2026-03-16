# Anomaly Watch

A **first-person anomaly detection horror game** built in **Unity 2022.3 LTS** for the **Tripo AI Game Jam**.

**Theme:** Cliché and “The More You Use It, The Worse It Gets”

Players patrol a mysterious apartment building where global disasters manifest as supernatural anomalies.  
Each day you must decide whether the world is safe — or if something has gone terribly wrong.

Make the wrong decision, and the entire run resets.

---

# Game Concept

You are assigned to monitor an apartment building.

Each day something may be wrong.

Your job is simple:

- If an **anomaly exists** → press the **RED button**
- If **everything is normal** → press the **GREEN button**

Survive **7 days** and save the world.

But there’s a catch.

Every time you fix a problem, it becomes harder to fix the next one.

---

# Core Theme Mechanic  
## “The More You Use It, The Worse It Gets”

The **RED button** neutralises disasters.

However, every time it is used successfully, the number of places it can appear **increases**.

| Successful RED Presses | Possible Locations |
|---|---|
| 0 | 3 |
| 1 | 4 |
| 2 | 5 |
| 3 | 6 |
| 4 | 7 |
| 5+ | 8 (maximum) |

This forces the player to search more rooms over time.

Saving the world becomes progressively harder.

---

# Game Structure

Each run contains **7 days**.

- **2–3 days are safe**
- **4–5 days contain anomalies**
- Anomalies appear in **random order**
- Each anomaly appears **once per run**

The player never knows which days are safe.

---

# Gameplay Loop

1. Player spawns in the **Ground Floor Lobby**
2. Lobby plants show progress from previous days
3. Player enters the apartment
4. Investigates rooms and looks for anomalies
5. Decides which button to press

Correct decision → next day  
Wrong decision → run resets to **Day 1**

---

# Anomalies

Seven anomaly types can occur in the apartment.

| # | Anomaly | Location | Concept |
|---|---|---|---|
| 1 | Burning Plant | Living Room | Forest fires |
| 2 | Attacking Dolls | Bedroom | Dark influence |
| 3 | Electricity Shortage | Entire apartment | Power failure |
| 4 | Bleeding Frame | Living Room | Viral outbreak |
| 5 | Contaminated Bathroom | Bathtub bathroom | Water contamination |
| 6 | Backward Clock | Living Room | Time disruption |
| 7 | Corrupted Text | Throughout apartment | Information warfare |

Each anomaly escalates over time and must be resolved before it reaches its fail state.

---

# Player Controls

| Input | Action |
|---|---|
W / A / S / D | Move |
Mouse | Look |
E | Press RED or GREEN button |
Left Click | Open / close doors |

The player **cannot**:

- Sprint
- Attack
- Pick up objects
- Use inventory

All gameplay is based on **observation and decision making**.

---

# Health System

The player has a hidden health buffer.

- Around **2–3 hits before death**
- No visible health bar
- Damage shown via **red vignette on screen**

Health resets at the start of each day.

---

# False Positive System

Pressing **RED on a safe day** triggers a special failure sequence.

1. Nothing happens for **5 seconds**
2. A loud supernatural event occurs
3. Player controls freeze
4. Screen fades to black
5. Instant death

The run resets to **Day 1**.

---

# Progress Tracker

The lobby contains **7 plants**.

Each successful day causes one plant to **bloom**.

If the player dies:

All plants reset to **dead**.

---

# Win Condition

Correctly resolve all **7 days**.

Win sequence:

1. Screen fades to black
2. Message appears:  
   **“THE WORLD IS SAVED”**
3. All lobby plants glow
4. Credits appear

---

# Lose Condition

The player dies if:

- An anomaly timer expires
- An anomaly attacks the player
- GREEN is pressed during an anomaly
- RED is pressed on a safe day

Death resets the run to **Day 1**.

---

# Project Structure

---

# Core Systems

| System | Description |
|---|---|
GameManager | Controls day progression and game states |
AnomalyManager | Assigns and activates anomalies |
ButtonManager | Manages RED button pool and placement |
HealthSystem | Handles player damage and death |
ProgressTracker | Controls lobby plant progress |
EnvironmentController | Handles environment scatter events |
FalsePositiveController | Handles false RED press failure |
UIManager | HUD, win screen, and death screen |

---

# Engine & Tools

- **Unity 2022.3 LTS**
- **Universal Render Pipeline (URP)**
- **DOTween**
- **Apartment Kit — Brick Project Studio**
- **Tripo AI** generated assets

---

# Assets

### Apartment Kit
Provides the apartment environment, furniture, FPS controller, and door interaction system.

### Tripo AI Assets

Used for:

- Doll / action figure models (Attacking Dolls anomaly)
- RED button prop
- GREEN button prop
- Dead plant variants (lobby progress tracker)
- Alive / glowing plant variants (lobby progress tracker)
- Wall clock
- Living room plant
- Devil statue
- Broken chair
- Broken table

If asset generation fails, placeholder primitives can be used.

---

# Running the Project

1. Open the project in **Unity 2022.3 LTS**
2. Load the **GameScene**
3. Press **Play**

Ensure:

- URP is enabled
- Apartment Kit assets are imported

---

# Game Jam Submission

Project created for:

**Tripo AI Game Jam**

Theme: **Cliché**

---

# Team

**Team Name:** Neural Ninjas

**Members**

- Atharv Pinjare  
- Purvansh Tayal  

---

# Credits

Environment  
Apartment Kit — Brick Project Studio

AI Generated Models  
Tripo AI

Engine  
Unity Technologies
