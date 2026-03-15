# Task List — 48-Hour GameJam Sprint Backlog
---
**Game:** Anomaly Detection Horror Game
**Theme:** The More You Use It, The Worse It Gets
**Engine:** Unity 2022.3 LTS | URP | PC Standalone

---

## How To Use This File

1. Work top to bottom. Never skip a task or reorder within a sprint.
2. Each task shows exactly which doc sections to paste into the Codex prompt.
3. When you reach a task, tell your Prompt Engineer (Claude) which task number you are on.
   Claude will write the full Codex prompt for that task on the spot.
4. Check off tasks as you complete them: [ ] → [x]
5. Do not start Sprint 2 until all Sprint 1 tasks are checked.
6. Do not start Sprint 3 until all Sprint 2 tasks are checked.

---

## Sprint 1 — Foundation & Core Systems
### Target Window: Hours 0 – 14
### Goal: A compilable, runnable Unity project with all core systems scaffolded.
### Exit Criteria: Play mode launches, player can walk through the house, both buttons exist in the scene, no compiler errors.

---

### SCENE SETUP (Manual — No Codex)

- [ ] **TASK 0.1 — Unity Project Init**
  Create new Unity 2022.3 LTS project with URP template.
  Install DOTween from Asset Store.
  Create the full folder structure:
  ```
  /Assets
    /Scripts
      /Core
      /Anomalies
      /Environment
      /UI
      /Interactions
    /Prefabs
    /Materials
    /Audio
    /AI_Assets       ← Tripo AI exports land here
  /docs              ← paste your three .md files here
  /tasks             ← this file lives here
  ```
  **No Codex needed. Do this manually. ~20 minutes.**

---

- [ ] **TASK 0.2 — Placeholder Scene Build (Manual)**
  Build a bare minimum scene by hand before writing a single script:
  - Create a Balcony area (plane + railing cubes)
  - Create a Living Room (box room with floor/walls/ceiling)
  - Create a Bathroom (smaller box room attached to living room)
  - Connect rooms with open doorways (no door objects yet)
  - Place 7 empty GameObjects on the balcony labeled PlantSlot_1 through PlantSlot_7
  - Place a placeholder cube for RED button — label it RedButton
  - Place a placeholder cube for GREEN button — label it GreenButton (fixed position)
  - Place 7 empty Transform GameObjects labeled RedButtonPos_1 through RedButtonPos_7
    distributed across the living room and balcony
  - Place a PlayerSpawn empty GameObject on the balcony
  - Add a basic FPS character controller (Unity's built-in or a simple Rigidbody capsule)
  - Tag all placeholder interactable objects with tag "Interactable"
  - Add a trigger collider at the front door threshold — label it FrontDoorTrigger

  **No Codex needed. Do this manually. ~45 minutes.**
  **Run Tripo AI asset generation in parallel during this time.**

---

### CORE SCRIPTS (Codex — Sequential)

- [ ] **TASK 1.1 — GameState.cs + EventBus.cs**
  📄 Doc Reference: `gameplay_loop.md` → Section 10
  🔑 These are the foundation. Every other script depends on them.
  Generate first. Verify compile before moving on.

---

- [ ] **TASK 1.2 — GameManager.cs**
  📄 Doc Reference: `gameplay_loop.md` → Sections 2, 3, 11
  🔗 Depends On: GameState.cs, EventBus.cs (must be in project)
  Paste EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 1.3 — HealthSystem.cs**
  📄 Doc Reference: `gameplay_loop.md` → Sections 6, 14
  🔗 Depends On: EventBus.cs
  Paste EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 1.4 — ButtonManager.cs**
  📄 Doc Reference: `gameplay_loop.md` → Sections 5, 14
  🔗 Depends On: EventBus.cs, GameManager.cs
  Paste EventBus.cs + GameManager.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 1.5 — ProgressTracker.cs**
  📄 Doc Reference: `gameplay_loop.md` → Section 8
  🔗 Depends On: EventBus.cs
  Paste EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 1.6 — EnvironmentController.cs**
  📄 Doc Reference: `gameplay_loop.md` → Section 9
  🔗 Depends On: EventBus.cs
  Paste EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 1.7 — FalsePositiveController.cs**
  📄 Doc Reference: `gameplay_loop.md` → Sections 7, 14
  🔗 Depends On: EventBus.cs
  Paste EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 1.8 — AnomalyData.cs + AnomalyType.cs + AnomalyBase.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 2, 3, 11
  🔗 Depends On: EventBus.cs, HealthSystem.cs
  Paste EventBus.cs + HealthSystem.cs as EXISTING CODE in prompt.
  🔑 AnomalyBase is the parent class for all 7 anomaly scripts. Must be solid before Sprint 2.

---

- [ ] **TASK 1.9 — AnomalyManager.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 4, 5, 6, 9, 11
  🔗 Depends On: EventBus.cs, AnomalyBase.cs, AnomalyData.cs, AnomalyType.cs, GameManager.cs
  Paste EventBus.cs + AnomalyBase.cs + GameManager.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 1.10 — UIManager.cs (Skeleton)**
  📄 Doc Reference: `gameplay_loop.md` → Sections 3, 12, 13
  🔗 Depends On: EventBus.cs, GameManager.cs
  Paste EventBus.cs as EXISTING CODE in prompt.
  ⚠️ Sprint 1 scope only: Day number display, basic state label (Observe/Hunt etc.), win/lose screen placeholders. No polish.

---

- [ ] **TASK 1.11 — Scene Wiring (Manual)**
  Attach all generated scripts to GameObjects in the scene:
  - GameManager → empty GameObject "GameManager"
  - AnomalyManager → empty GameObject "AnomalyManager"
  - ButtonManager → empty GameObject "ButtonManager"
  - HealthSystem → empty GameObject "HealthSystem"
  - ProgressTracker → empty GameObject "ProgressTracker"
  - EnvironmentController → empty GameObject "EnvironmentController"
  - FalsePositiveController → empty GameObject "FalsePositiveController"
  - UIManager → Canvas GameObject
  Wire all Inspector fields (RedButtonPositions array, PlantSlots array, etc.)
  **Press Play. Verify no null reference errors in Console.**

---

### Sprint 1 Exit Check
Before starting Sprint 2, confirm all of the following in Play mode:
- [ ] No compiler errors
- [ ] No null reference exceptions on startup
- [ ] Player can walk from Balcony through front door into Living Room
- [ ] Both buttons are present in the scene
- [ ] Console logs confirm EventBus events firing on button press (add debug logs temporarily)

---

## Sprint 2 — Anomaly Systems
### Target Window: Hours 14 – 28
### Goal: All 7 anomalies implemented, triggering correctly, with working fail states and resolution.
### Exit Criteria: Each anomaly can be triggered manually in Play mode, escalates, and resolves or kills the player correctly.

---

- [ ] **TASK 2.1 — BurningPlantAnomaly.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 3, 8 (Anomaly 1), 11
  🔗 Depends On: AnomalyBase.cs, EventBus.cs, HealthSystem.cs
  Paste AnomalyBase.cs + EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 2.2 — AttackingDollsAnomaly.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 3, 8 (Anomaly 2), 11
  🔗 Depends On: AnomalyBase.cs, EventBus.cs, HealthSystem.cs
  Paste AnomalyBase.cs + EventBus.cs as EXISTING CODE in prompt.
  ⚠️ Dolls use waypoint-based movement only. No NavMesh. Confirm this is in prompt constraints.

---

- [ ] **TASK 2.3 — ElectricityShortageAnomaly.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 3, 8 (Anomaly 3), 11
  🔗 Depends On: AnomalyBase.cs, EventBus.cs
  Paste AnomalyBase.cs + EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 2.4 — BleedingFrameAnomaly.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 3, 8 (Anomaly 4), 11
  🔗 Depends On: AnomalyBase.cs, EventBus.cs, HealthSystem.cs
  Paste AnomalyBase.cs + EventBus.cs as EXISTING CODE in prompt.

---

- [ ] **TASK 2.5 — ContaminatedBathroomAnomaly.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 3, 8 (Anomaly 5), 11
  🔗 Depends On: AnomalyBase.cs, EventBus.cs
  Paste AnomalyBase.cs + EventBus.cs as EXISTING CODE in prompt.
  ⚠️ This anomaly has two distinct fail states (timer vs GREEN press). Confirm FailReason enum is in prompt.

---

- [ ] **TASK 2.6 — ClockIdleBehaviour.cs**
  📄 Doc Reference: `anomaly_system.md` → Section 8 (Anomaly 6 — Idle Behaviour note)
  🔗 Depends On: Nothing (standalone lightweight component)
  ⚠️ This is NOT managed by AnomalyManager. It runs always-on. Generate separately from BackwardClockAnomaly.

---

- [ ] **TASK 2.7 — BackwardClockAnomaly.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 3, 8 (Anomaly 6), 11
  🔗 Depends On: AnomalyBase.cs, EventBus.cs, EnvironmentController.cs
  Paste AnomalyBase.cs + EventBus.cs + EnvironmentController.cs as EXISTING CODE in prompt.
  ⚠️ ScatterAllObjects() is a direct call to EnvironmentController — not via EventBus. Flag this explicitly in prompt.
  ⚠️ The Shatter fail state executes in a single frame. No lerp, no coroutine. Flag this in prompt.

---

- [ ] **TASK 2.8 — CorruptedTextAnomaly.cs**
  📄 Doc Reference: `anomaly_system.md` → Sections 3, 8 (Anomaly 7), 11
  🔗 Depends On: AnomalyBase.cs, EventBus.cs
  Paste AnomalyBase.cs + EventBus.cs as EXISTING CODE in prompt.
  ⚠️ Day 3+ constraint is handled by AnomalyManager assignment, not this script. Do not add day-check logic here.

---

- [ ] **TASK 2.9 — Full Anomaly Integration Test (Manual)**
  Wire all 7 anomaly scripts to their scene GameObjects.
  Set AnomalyManager's allAnomalies array in Inspector.
  Test each anomaly individually by temporarily hardcoding currentDay in GameManager.
  For each anomaly verify:
  - [ ] Activates correctly
  - [ ] Escalation runs as expected
  - [ ] RED press resolves it cleanly
  - [ ] Fail state triggers correctly (timer or GREEN press)
  - [ ] ResetAnomaly() fully clears all state
  Remove hardcoded test values after all 7 pass.

---

### Sprint 2 Exit Check
Before starting Sprint 3, confirm all of the following:
- [ ] All 7 anomaly scripts compile with zero errors
- [ ] Each anomaly activates, escalates, resolves, and resets without null references
- [ ] AnomalyManager correctly assigns anomalies to random days on InitialiseRun()
- [ ] Text anomaly never assigned before Day 3 (test by logging dayAssignments[] on RunStart)
- [ ] ButtonManager correctly identifies anomaly vs safe days and routes button presses
- [ ] False positive devil sequence plays and resets the run

---

## Sprint 3 — Integration, Polish & Ship
### Target Window: Hours 28 – 42
### Goal: Full game loop playable from Day 1 through Day 7, win and lose states working, audio and visual feedback present.
### Exit Criteria: A complete playable build that can be submitted to the jam.

---

- [ ] **TASK 3.1 — UIManager.cs (Full Implementation)**
  📄 Doc Reference: `gameplay_loop.md` → Sections 3, 12, 13, 14
  🔗 Depends On: EventBus.cs, GameManager.cs
  Paste EventBus.cs + UIManager skeleton from Task 1.10 as EXISTING CODE.
  Implement fully:
  - Day number display
  - Win screen ("THE WORLD IS SAVED" + all plants glowing)
  - Death/reset screen (brief Day 1 indicator)
  - Screen blackout fade (used by all death sequences)

---

- [ ] **TASK 3.2 — Full Run Integration Test (Manual)**
  Play a complete run from Day 1 to Day 7 without hardcoding anything.
  Verify:
  - [ ] Days randomise correctly each run
  - [ ] Button pool expands with each RED press
  - [ ] Plants bloom correctly per completed day
  - [ ] All plants reset on death
  - [ ] Win screen fires after Day 7
  - [ ] Death from each anomaly type resets correctly
  Record any broken states in a bug list.

---

- [ ] **TASK 3.3 — Bug Fix Pass (Codex micro-prompts)**
  For each bug found in Task 3.2:
  Use the micro-prompt pattern (paste error + affected method only).
  Fix one bug per prompt. Do not batch fixes.

---

- [ ] **TASK 3.4 — Audio Implementation (Manual + Codex)**
  Add AudioSource components to relevant scene objects.
  Minimum viable audio:
  - Ambient room tone (loop)
  - Fire crackle (BurningPlant)
  - Doll movement scrape (AttackingDolls)
  - Electricity flicker hum (ElectricityShortage)
  - Clock ticking forward + reversed (BackwardClock)
  - Bathroom cracking (ContaminatedBathroom)
  - Button press click (both buttons)
  - Death blackout sting
  Use free/AI-generated SFX. Codex prompt only needed if AudioManager script is required.

---

- [ ] **TASK 3.5 — Tripo AI Asset Swap (Manual)**
  Replace all placeholder cubes with Tripo AI generated models.
  Minimum required assets in scene:
  - [ ] Living room furniture set
  - [ ] Wall clock
  - [ ] Potted plant (dead + alive variants)
  - [ ] Photo frame
  - [ ] 5x doll / action figure models
  - [ ] Bathroom fixtures
  - [ ] Balcony railing + 7 plant slot props
  - [ ] Devil / demonic entity
  - [ ] RED button prop
  - [ ] GREEN button prop
  ⚠️ Do not break existing script references when swapping. Keep original GameObject names.
  ⚠️ Re-run all anomaly tests after asset swap to catch any broken transform references.

---

- [ ] **TASK 3.6 — Visual Polish Pass (Codex + Manual)**
  Minimum viable polish only. Skip anything that takes more than 30 minutes:
  - [ ] Plant bloom DOTween animation (ProgressTracker.BloomPlant)
  - [ ] Screen vignette fade (HealthSystem hit response)
  - [ ] RED button glow pulse material (shader or emission animation)
  - [ ] Balcony → Living Room door walk-in feel (fog of war or light difference)

---

- [ ] **TASK 3.7 — Main Menu Scene (Codex + Manual)**
  Minimal main menu:
  - [ ] Game title
  - [ ] Start button → loads game scene
  - [ ] Quit button
  No Codex prompt needed unless menu has state logic. Simple UnityEngine.SceneManagement call.

---

- [ ] **TASK 3.8 — Build & Smoke Test**
  File → Build Settings → PC Standalone.
  Build and run the executable.
  Play two full runs outside the Editor:
  - [ ] Run 1: Attempt to win (press correctly each day)
  - [ ] Run 2: Intentionally die from 3 different anomalies
  Fix any build-only bugs with micro-prompts.

---

### Sprint 3 Exit Check — Submission Readiness
- [ ] Builds without errors
- [ ] Full 7-day loop plays from start to finish
- [ ] Win state and lose state both reachable
- [ ] No null reference exceptions during normal play
- [ ] All 7 anomalies trigger at least once across two full runs
- [ ] False positive devil sequence plays correctly
- [ ] Audio present on all major events
- [ ] Tripo AI assets in scene (no placeholder cubes visible)

---

## Buffer — Final Hours 42–48
### Do NOT add features during this window.
### Allowed actions only:
- Playtesting and micro-fix bugs found during play
- Adjusting [SerializeField] timing values in the Inspector
- Writing the jam submission page (description, screenshots, controls)
- Exporting final build
- Uploading to itch.io or jam platform

---

## Task Summary

| Sprint | Tasks | Est. Hours | Goal |
|---|---|---|---|
| Sprint 1 | 0.1 → 1.11 | 0 – 14 | All core systems compilable and wired |
| Sprint 2 | 2.1 → 2.9 | 14 – 28 | All 7 anomalies working in isolation |
| Sprint 3 | 3.1 → 3.8 | 28 – 42 | Full game loop shippable |
| Buffer | — | 42 – 48 | Polish, test, submit |

## Hard Rule
**At Hour 20, close this file and freeze scope.**
No new tasks may be added after Hour 20.
Everything after Hour 20 is execution only.
