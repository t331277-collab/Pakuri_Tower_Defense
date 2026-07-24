# New Core Architecture Blueprint

## Task title

New Core Architecture Blueprint

## Goals

- Define a new Core architecture without using the existing code structure as a design standard.
- Preserve `Pakuri/Assets/CSVdata` and the column names and terminology of each CSV exactly as they are.
- Parse the CSV files into Definitions once when the game starts.
- Separate responsibilities for Definitions, runtime Models, skill-learning state, combat execution, skill lifecycles, visual effects, and run progression.
- Make each monster directly manage its own skill-learning progress.
- Reconnect the current CSV, prefab, scene, sprite, animation, and AnimatorController resources to the new Core.
- After the final transition, reduce runtime, serialization, and compilation dependencies on legacy types under `Pakuri/Assets/Scripts` to zero.

## Constraints

- This document is a draft blueprint.
- Do not apply it to the existing game code, CSV files, prefabs, or scenes.
- Do not use the existing structure as a design standard for the new structure.
- Base claims about actual existence and the CSV contracts to retain on inspected files.
- Do not arbitrarily rename CSV columns in the CSV Definition layer.
- Do not copy the folder and class division under the existing `Pakuri/Assets/Scripts` as a design standard for the new structure.
- For gameplay rules the user has not confirmed, inspect the actual behavior under the existing `Pakuri/Assets/Scripts` as compatibility evidence.
- If inspecting the existing Scripts still does not establish a single meaning, the implementer must ask the user instead of deciding arbitrarily.
- The existing Scripts are read-only evidence for pre-transition behavior and asset wiring; they are not call targets, inheritance targets, fallbacks, or compatibility layers for the new code.
- The final structure must not reference types, namespaces, Script GUIDs, static state, or runtime objects from the existing Scripts.
- Record progress and verification evidence for this complete replacement only in this document's Phase execution records, not in `BLACKBOARD.md` or `boards/**/*BLACKBOARD.md`.
- Every Phase must pass Unity recompilation and Console-log checks before the next Phase begins.
- Use Play Mode only for behavior that cannot be proven through static inspection and non-runtime verification, and first present the execution purpose and verification scenario to the user.
- Do not add classes, features, fields, or numeric values without evidence.

## Role Owner

Designer

## Status

Implementation v0.8. The full blueprint has been translated to English without changing its structure. Phase 0 passed Code Reviewer loop 3, Phase 1 passed loop 2, and Phase 2 passed loop 2. Phase 3 is Builder-complete and pending Code Reviewer; Phases 4 through 6 have not started.

## Next Actions

- Add new Core components or revise the existing draft according to user instructions.
- Establish each class's public API, owned state, inputs, outputs, and prohibited responsibilities.
- Detail ID references among Definitions and the startup initialization order.
- Before implementation, establish each new type's direct owner, caller, state authority, and deletion condition.
- Establish the migration inventory and new owner for every scene, prefab, and `.asset` that references a legacy Script GUID.
- Before implementation, establish policies for reconnecting active resources and handling unnecessary Legacy serialized assets.
- Proceed sequentially from Phase 0 and pass each Phase's exit conditions and Unity Console gate.
- During implementation, update state, changed paths, log evidence, and Play Mode status only in this document's Phase execution records.
- Establish public APIs and local verification procedures in the Code Builder handoff.
- Do not apply the blueprint to code before the final blueprint is confirmed.

## Evidence

- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/`
- `Pakuri/Assets/CSVdata/authoring/monster/monsters.csv`
- `Pakuri/Assets/CSVdata/authoring/enemy/enemies.csv`
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/`
- `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv`
- `Pakuri/Assets/CSVdata/stage_flow/StageDay.csv`
- `Pakuri/Assets/CSVdata/stage_flow/StageEncounter.csv`
- `Pakuri/Assets/CSVdata/stage_flow/StageReward.csv`
- `Pakuri/Assets/Scripts/GameFlow/Stage/MonsterDayRecovery.cs`
- `Pakuri/Assets/Scripts/UI/InGame/InGameUIManager.cs`
- `Pakuri/Assets/Scripts/GameFlow/RunSession.cs`
- `Pakuri/Assets/Scripts/Units/Monster/Input/PlayerCombatInputController.cs`
- `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyActionController.cs`
- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyCombatDecision.cs`
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`
- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/CsvRuntimeCatalog.asset`
- `Pakuri/Assets/Legacy/Data/GameData/`
- `Pakuri/reference/Work/new-core-phase0-manifest-generator.ps1`
- `Pakuri/reference/Work/new-core-phase0-csv-contract-manifest.csv`
- `Pakuri/reference/Work/new-core-phase0-script-reference-manifest.csv`
- `Pakuri/reference/Work/new-core-phase0-retained-resource-manifest.csv`
- `Pakuri/reference/Work/new-core-phase0-inspector-snapshot.csv`
- `boards/UI/UI_BLACKBOARD.md`
- `boards/UI/RUNSCENE_UI.md`

## History

- 2026-07-23: The user requested a blueprint for a new object-oriented Core architecture without applying or referring to the existing structure.
- 2026-07-23: Inspected the CSV contracts to retain and the round-reset behavior in `MonsterDayRecovery.cs`.
- 2026-07-23: Recorded draft responsibilities for Definitions, runtime Models, SkillBuckets, Managers, Executors, Actors, status effects, and visual effects.
- 2026-07-23: Added `RunSessionModel`, `PartyRoster`, `PrisonerInventory`, `RewardService`, `OfferingService`, `ManifestationService`, and the action/movement structure, and consolidated the name `StageRunManager` into `StageManager`.
- 2026-07-23: Added a boundary for retaining the current `NewRunScene` UI structure while adapting its bindings to the new Core's Model and Service APIs.
- 2026-07-23: Established manifestation candidates, prisoner consumption, uniform offering draws, manual mouse aiming, and the execution order inside the combat Manager from actual existing Scripts.
- 2026-07-23: Added a compatibility principle and final target folder tree: inspect the existing `Pakuri/Assets/Scripts` first for unresolved meanings during implementation, then ask the user if they remain unclear.
- 2026-07-23: Established that all Skill Actors use a central Tick, the existing recruit/skip flow of the manifestation-success popup remains, and Save/Load will not be created.
- 2026-07-23: Added implementation conventions that reference the existing Scripts' coding format while prioritizing the blueprint's responsibility boundaries and avoiding Naive Code Filter findings for unnecessary indirection, multiple authorities, duplicate validation, and dead code.
- 2026-07-23: The user established the final goal of retaining current resources while severing every dependency on existing Scripts and fully replacing them with the new blueprint implementation.
- 2026-07-23: Inspected 239 Unity serialization files and confirmed that 21 legacy Scripts are referenced 56 times across 40 assets; included the runtime catalog and Legacy `.asset` files, not only scenes and prefabs, as migration-or-removal decision targets.
- 2026-07-23: The user requested dividing the complete replacement into Phases, recording none of this work in BLACKBOARD-family files, checking Unity logs in every Phase, and using Play Mode only when essential.
- 2026-07-23: Translated the entire blueprint to English while preserving all headings, code fences, code identifiers, CSV terms, paths, and Phase structure.
- 2026-07-23: Code Builder completed the Phase 0 baseline manifests and repeatable Unity and filesystem checks; Code Reviewer validation is pending.
- 2026-07-23: Phase 0 Reviewer loop 1 returned FIX REQUIRED for an incomplete retained-resource inventory, stale post-record document counts, and an undefined serialization-extension scope. Code Builder added a reproducible generator, transitive retained-resource inventory, exact Inspector payload snapshots, and `.scenetemplate` coverage for re-review.
- 2026-07-23: Phase 0 Reviewer loop 2 verified every requested fix but returned FIX REQUIRED for the unsupported word `materials`; the manifest contains no reachable `.mat` row, so the claim was removed.
- 2026-07-23: Phase 0 Code Reviewer loop 3 returned PASS with no remaining blocker or user-only verification gap.

---

## 1. Core Design Principles

Separate the core responsibilities as follows.

```text
Definition       = What it is
Model            = What state it is currently in
SkillBucket      = What it has learned
SkillCooldown    = Whether it can be used now
SkillTargeting   = Who it will be used on
Executor         = What will be executed
Actor            = When the created skill ends
CombatManager    = How results are applied to combat
EffectManager    = How it appears
StageManager     = How far the run has progressed and how much currency is held
```

Create Definitions from CSV at game startup, then treat them as immutable data.

Each `MonsterModel` owns its own `MonsterSkillBucket`. No other Manager owns a monster's active, passive, enhancement, or master learning state on its behalf.

### 1.1 Evidence Priority for Unresolved Behavior During Implementation

```text
1. Rules directly confirmed by the user in this blueprint
2. Actual columns and data in the CSV files to retain
3. Actual gameplay behavior in Pakuri/Assets/Scripts
4. If these still do not establish one answer, stop implementation and ask the user
```

The existing `Pakuri/Assets/Scripts` is not a standard for copying a new folder structure or class responsibilities. It is evidence for identifying gameplay behavior whose meaning is missing from the new structure and preserving behavioral compatibility with the existing game.

The implementer must not invent numbers, mappings, priorities, failure handling, or targeting rules absent from the existing Scripts.

Inspecting the existing Scripts does not mean the new implementation references legacy types. Do not connect legacy types to the new implementation through parameters, fields, inheritance, interfaces, events, reflection strings, adapters, or fallbacks.

## 2. Definition Layer

### 2.1 Skill Definition

```text
SkillDefinition
├─ ProjectileDefinition
├─ LineAttackDefinition
├─ AreaAttackDefinition
├─ SingleAttackDefinition
├─ BuffDefinition
├─ HealDefinition
├─ ShieldDefinition
└─ PassiveDefinition
```

`SkillDefinition` defines common basic skill information.

Common candidate fields:

- `skill_id`
- `monster_id`
- `slot`
- `display_name`
- `runtime_kind`
- `description_text`
- `summary`

Each derived Definition uses the exact column names that actually exist in its corresponding CSV.

For example, `ProjectileDefinition` uses the following projectile CSV terminology.

- `base_damage`
- `spell_power_coefficient`
- `magazine_capacity`
- `reload_seconds`
- `shot_interval_seconds`
- `projectile_burst_count`
- `projectile_speed`
- `pierce_count`
- `critical_allowed`
- `target_selection`
- `cooldown_seconds`
- `runtime_visual_sprite_path`
- `runtime_impact_visual_sprite_path`

The enemy-skill CSV set contains separate `heal` and `shield` skill files. Therefore, retaining the full CSV set requires `HealDefinition` and `ShieldDefinition`.

### 2.2 Choice and Node Definitions

The actual CSV contract is divided into Choices, graph nodes, node types, and node parameters.

```text
SkillChoiceDefinition
ChoiceNodeDefinition
NodeTypeDefinition
NodeParamDefinition
```

`SkillChoiceDefinition`:

- `choice_id`
- `skill_id`
- `monster_id`
- `target_skill_id`
- `choice_group`
- `sort_order`
- `title`
- `description_text`

`ChoiceNodeDefinition`:

- `monster_id`
- `owner_kind`
- `owner_id`
- `graph_kind`
- `graph_index`
- `target_skill_id`
- `node_order`
- `node_type_id`
- `arg_1`
- `arg_2`
- `arg_3`
- `arg_4`
- `arg_5`
- `arg_6`
- `arg_7`
- `arg_8`
- `arg_9`
- `arg_10`
- `arg_11`
- `arg_12`
- `excludes_active_choice_id`

`NodeTypeDefinition`:

- `node_type_id`
- `handler_id`
- `node_kind`
- `runtime_support_state`
- `runtime_support_notes`

`NodeParamDefinition`:

- `node_type_id`
- `param_order`
- `param_key`
- `value_type`
- `required`
- `allowed_values`

`handler_id` and `param_key` exist in separate CSV files. Do not force them into a single `ChoiceNodeDefinition`, so the original CSV contract remains intact.

### 2.3 Trigger Definition

The following Definition is required to parse monster and enemy Trigger CSV files.

```text
SkillTriggerDefinition
```

`SkillTriggerDefinition` is based on the following terminology from the actual Trigger CSV files.

- `trigger_id`
- `source_skill_id`
- `trigger_event`
- `triggered_skill_id`
- `runtime_kind`
- `sort_order`
- `target_side`
- `target_selection`
- `target_shape`
- `center_mode`
- `proc_chance`
- `trigger_action`
- `internal_cooldown_seconds`

Because actual column sets differ among Trigger CSV files, do not assume that a field absent from a given file exists.

### 2.4 Unit Definition

Record the class name as `UnitDefinition`, not `UnitDefinotion`.

```text
UnitDefinition
├─ MonsterDefinition
└─ EnemyDefinition
```

`UnitDefinition` defines common unit data.

- Maximum health
- Attack power
- Spell power
- Movement speed
- Critical-hit chance
- Critical-hit damage
- Critical-hit resistance
- Physical defense
- Fire defense
- Lightning defense
- Ice defense
- Darkness defense
- Holy defense

`MonsterDefinition` uses monster CSV terminology.

- `id`
- `display_name`
- `role_summary`
- `element_label`
- `primary_attribute`
- `max_health`
- `power_stat`
- `base_damage`
- `power_coefficient`
- `base_attack_power`
- `base_spell_power`
- `base_move_speed`
- `base_crit_chance`
- `base_crit_damage`
- `base_crit_resistance`
- `def_physical`
- `def_fire`
- `def_lightning`
- `def_ice`
- `def_darkness`
- `def_holy`
- `MonsterIconImage`

`EnemyDefinition` uses enemy CSV terminology.

- `enemy_id`
- `stage_id`
- `sort_order`
- `display_name`
- `encounter_role`
- `attack_type`
- `attribute`
- `max_health`
- `attack_power`
- `spell_power`
- `move_speed`
- `crit_chance`
- `crit_damage`
- `crit_resistance`
- `def_physical`
- `def_fire`
- `def_lightning`
- `def_ice`
- `def_darkness`
- `def_holy`
- `skill_slot_a_id`
- `skill_slot_b_id`
- `passive_id`
- `nexus_damage`

### 2.5 Stage Definition

Structure stage data according to the three actual CSV files.

```text
StageDefinition
├─ StageDayDefinition
├─ StageEncounterDefinition
└─ StageRewardDefinition
```

`StageDayDefinition`:

- Define the day and battle type
- Link encounter and reward rules
- Retain shop, event, and elite-choice fields

`StageEncounterDefinition`:

- Enemy spawn order
- Enemy types and counts
- Spawn intervals and positions
- Boss candidates and confirmed boss
- Confirmed prisoners

`StageRewardDefinition`:

- Gold
- Dark Trace
- Prisoner-count probabilities
- Manifestation success probability
- Additional elite prisoners
- Number of relic choices

## 3. Startup CSV Parsing

```text
CsvParser.cs
    ↓
GameDefinitionCatalog
```

### 3.1 CsvParser

`CsvParser` responsibilities:

- Parse the CSV files once at game startup.
- Read each CSV's column names unchanged.
- Create Definitions appropriate to each CSV type.
- Resolve ID references.
- Check for duplicate IDs.
- Check for missing references.
- Validate invalid enums and numbers.
- Do not silently guess or correct erroneous Definitions.

`CsvParser` does not have the following runtime responsibilities.

- Damage calculation
- Skill execution
- Unit creation
- Run progression
- Skill-learning state
- Visual-effect creation

### 3.2 GameDefinitionCatalog

`GameDefinitionCatalog` responsibilities:

- Store parsed Definitions
- Retrieve Definitions by ID
- Provide immutable game data with resolved references

`GameDefinitionCatalog` does not store current runtime health, status effects, cooldowns, or learning state.

### 3.3 Initialization Order

```text
Status Definition
→ NodeType / NodeParam Definition
→ Skill / Choice / ChoiceNode / Trigger Definition
→ Monster / Enemy Definition
→ Stage Definition
→ Validate all ID references
→ Finalize GameDefinitionCatalog
```

## 4. Runtime Model Layer

```text
UnitBaseModel
├─ MonsterModel
├─ EnemyModel
└─ NexusModel
```

### 4.1 UnitBaseModel

Holds common runtime values for each unit.

- Definition reference
- Current health
- Current shield
- Current stats
- Survival state
- Current position
- Current status effects
- Current cooldown state

### 4.2 MonsterModel

`MonsterModel` responsibilities:

- Reference `MonsterDefinition`
- Own `MonsterSkillBucket`
- Auto-attack state
- Auto-skill state
- Its own status-effect and resource state
- Reset its own state for the next round

No other Manager owns the monster's learned-skill list on its behalf.

### 4.3 EnemyModel

`EnemyModel` responsibilities:

- Reference `EnemyDefinition`
- Own `EnemySkillBucket`
- State for active and passive skills assigned to the enemy
- Its own status-effect and resource state
- Survival and Nexus-contact state

### 4.4 NexusModel

`NexusModel` responsibilities:

- Current health
- Maximum health
- Survival state
- Apply received Nexus damage

### 4.5 RunSessionModel

Stores progression state retained between battles in a single run.

`RunSessionModel` responsibilities:

- Current stage identifier
- Current day
- Current encounter identifier
- Reference `PartyRoster`
- Reference `PrisonerInventory`
- Current reward-processing state
- Run victory and defeat state

Per the user's specification, `StageManager`, not `RunSessionModel`, manages the player's `Gold` and `DarkTrace`.

`RunSessionModel` does not have the following responsibilities.

- Damage calculation
- Unit actions
- Skill execution
- UI presentation
- CSV parsing

### 4.6 PartyRoster

Manages the player's ordered monster party within a run.

`PartyRoster` responsibilities:

- Register the initially selected monster in the first slot
- Register a manifested monster in the next empty slot
- Preserve party-member order
- Enforce the maximum party-slot limit
- Prevent duplicate monster registration
- Report whether a party member can be added
- Retrieve a party member by monster identifier

`PartyRoster` does not determine whether a party member is alive on the current field. `StageManager` manages all units and surviving units on the current field.

The current UI's 1P-through-5P order uses `PartyRoster` order unchanged. The UI must not recombine the selected monster and manifested-monster lists.

### 4.7 PrisonerInventory

Manages prisoners acquired as battle rewards.

`PrisonerInventory` responsibilities:

- Register prisoners by `enemy_id`
- Retrieve held prisoners
- Select a prisoner for manifestation or offering
- Check whether a prisoner can be consumed
- Consume a prisoner
- Prevent reuse of an already consumed prisoner
- Clear the previous list when new battle rewards are generated
- Clear remaining prisoners when advancing to the next day

`PrisonerInventory` does not determine manifestation success or generate offering candidates.

In the existing Scripts, prisoners are retained only during the current battle's reward stage, not accumulated as a run-wide resource. Unused prisoners do not carry over to the next day.

## 5. SkillBucket

```text
SkillBucket
├─ MonsterSkillBucket
└─ EnemySkillBucket
```

Record the class name as `SkillBucket`, not `Skillbucket`.

### 5.1 MonsterSkillBucket

Each `MonsterModel` owns one.

- Learned active skills
- Learned passive skills
- Selected enhancements
- Selected masters
- Per-skill acquisition limits
- Prevention of duplicate learning
- Provide Choices and Nodes to apply when using a skill

The final authority that changes a monster's learning state is that monster's `MonsterSkillBucket`.

### 5.2 EnemySkillBucket

Each `EnemyModel` owns one.

- Active skills assigned to the enemy
- Passives assigned to the enemy
- Available skills
- Enemy skill-slot limits

## 6. SkillCooldown

An object referenced by each Model that evaluates skill-use conditions.

`SkillCooldown` responsibilities:

- Current cooldown
- Magazine
- Reloading
- Shot interval
- Determine availability
- Return the `CanUse()` result
- Update runtime state after skill use
- Reset for the next round

`SkillCooldown` does not have the following responsibilities.

- Target search
- Damage calculation
- Status-effect application
- Visual-effect creation
- Changing skill-learning state

## 7. SkillTargeting

Finds skill targets in automatic mode.

`SkillTargeting` responsibilities:

- Apply `target_selection`
- Apply `target_scope`
- Apply `radius`
- Use candidates among currently living units
- Return the final target or target list

`SkillTargeting` does not calculate damage or execute skills.

Manual targeting is established in section 8.6 from the user's confirmed requirements and the inspected compatibility behavior.

## 8. Action and Movement Structure

### 8.1 InGameActionManager

Coordinates the execution order of action Controllers during combat.

`InGameActionManager` responsibilities:

- Check `StageManager` combat-progression state
- Update action Controllers for living units
- Coordinate player-input processing order and automatic-action processing order
- Block execution for units unable to act
- Connect the call flow among `SkillCooldown`, `SkillTargeting`, and Executors

`InGameActionManager` owns neither damage calculation nor skill-learning state.

### 8.2 UnitActionController

```text
UnitActionController
├─ MonsterActionController
└─ EnemyActionController
```

Common responsibilities:

- Reference the assigned `UnitBaseModel`
- Check unit survival state
- Check whether movement, actions, and special skills are available
- Retrieve available skills
- Call `SkillCooldown.CanUse()`
- Request Executor execution when a target is ready

### 8.3 MonsterActionController

`MonsterActionController` responsibilities:

- Reference `MonsterModel` and `MonsterSkillBucket`
- Check the selected monster's manual or automatic skill state
- Process automatic actions for manifested monsters
- Select a target through `SkillTargeting` in automatic mode
- Request the Executor for an available skill

`PlayerInputController` delivers manual input. `MonsterActionController` does not directly find UI buttons.

### 8.4 EnemyActionController

`EnemyActionController` responsibilities:

- Reference `EnemyModel` and `EnemySkillBucket`
- Find attackable player units
- Use the Nexus as the target when no attackable target exists
- Request movement from `UnitMovementController` when outside attack range
- Execute an available skill when inside attack range
- Request application of `nexus_damage` when the Nexus-contact condition is met

It does not determine enemy spawn timing or enemy type. Those are the responsibilities of `StageManager` and `SpawnManager`.

### 8.5 UnitMovementController

Handles only position changes for movable units.

`UnitMovementController` responsibilities:

- Check current and target positions
- Apply movement speed
- Apply movement availability from status effects
- Update position according to `deltaTime`
- Return whether the target has been reached

`UnitMovementController` does not select targets, attack, calculate damage, or execute skills.

It handles enemy movement toward targets in the current game. Monster movement rules are not yet in the user's requirements, so monsters are not established as default movement subjects.

### 8.6 PlayerInputController

Handles manual input and automatic-state changes for the selected player monster.

`PlayerInputController` responsibilities:

- Identify the selected monster
- Change automatic skill state
- Request manual skill use
- Deliver UI input to `MonsterActionController`

Manual skill-targeting rules:

- Only the selected monster in party slot 0 receives manual input.
- Process manual input only while the selected monster's Auto-skill state is off.
- Use left-mouse-button input.
- Ignore combat input while the pointer is over the UI.
- Convert mouse screen coordinates into combat-world coordinates.
- Use the direction from the selected monster's position to the world aim point as `aimDirection`.
- Use the world aim point as `targetPoint`.
- A non-projectile skill attempts execution once using the aim point from the frame in which the button was pressed.
- A projectile skill uses the latest aim while the button is held.
- A projectile in a burst continues its remaining shots with the last stored aim even after the button is released.
- Skills requiring an area center prioritize the manual `targetPoint`.
- Use that skill's automatic `target_selection` rule only when no manual aim point exists.

Manual input provides only the aim direction and target point. Each Executor and `SkillTargeting` determine the final hit targets and whether they fall within the area.

### 8.7 Action Execution Order

```text
1. Apply passive changes
2. Tick cooldown, magazine, and reload states for all registered units
3. Attempt automatic skills for player and manifested monsters in registration order, then active-skill-list order
4. Process manual input for the selected monster
5. Process enemy actions in enemy registration order
6. `SkillActorManager` Ticks currently active Skill Actors in registration order
7. Process status-effect durations and expiration for all units
8. Apply final passive changes caused by state changes
```

Within-frame action order for one enemy unit:

```text
1. Check death and AutoAttackEnabled
2. Tick any active charge action
3. Find the nearest living player
4. Select the Nexus if no player exists
5. First attempt an available support-type B skill
6. Prefer an offensive B skill; select the A skill if B cannot be used
7. Move when outside range
8. Execute the selected skill when inside range and able to act
9. On Nexus contact, apply nexus_damage and then remove the enemy
```

This order uses the actual call order in the existing `InGameCombatManager.Update()`, `SkillExecution.TryExecuteAutomaticSkills(...)`, `PlayerCombatInputController.HandleManualInput(...)`, and `EnemyActionController.Tick(...)` as its compatibility standard.

The new structure removes independent Unity `Update()` methods from every Skill Actor. `InGameActionManager` Ticks once per frame in the order above and calls `SkillActorManager.Tick(deltaTime)` at step 6.

Register Skill Actors created by skill execution in the current frame in `pendingAdd`; begin Ticking them in the next frame. Register Actors that end during a Tick in `pendingRemove`; remove them after the current Actor iteration finishes. This rule prevents collection mutation and duplicate Ticks in the creation frame.

## 9. Skill Executor

Provide an Executor for each skill type.

```text
ProjectileExecutor
LineAttackExecutor
AreaAttackExecutor
SingleAttackExecutor
BuffExecutor
HealExecutor
ShieldExecutor
PassiveExecutor
```

Executor responsibilities:

- Read the skill Definition
- Read learned content from the caster's `SkillBucket`
- Combine applicable Choices and Nodes
- Apply Trigger conditions
- Produce actual skill-execution results
- Request creation of required Actors
- Request damage, healing, shield, and status-effect application from `InGameCombatManager`

Executors do not directly change skill-learning state.

Executors do not handle stage progression or visual-effect deletion.

## 10. Skill Actor

Tracks the runtime lifecycle of each spawned skill.

Examples:

```text
ProjectileActor
LineAttackActor
AreaAttackActor
SingleAttackActor
BuffActor
```

Actor responsibilities:

- Record creation time
- Movement
- Duration
- Collision
- Hit handling
- End conditions
- Signal skill-effect termination when ending
- Signal visual-effect deletion to `EffectManager`

Actors do not own damage formulas.

Actors do not own a monster's learning state.

Actors do not have independent Unity `Update()` methods. Update them only through the following common method.

```text
Tick(float deltaTime)
```

### 10.1 SkillActorManager

Manages the centralized lifecycle of currently active Skill Actors.

`SkillActorManager` responsibilities:

- Receive registration requests for created Skill Actors
- Manage `pendingAdd` for next-frame registration
- Tick active Actors in registration order
- Manage `pendingRemove` for ending Actors
- Remove Actors after iteration
- Request visual-effect deletion from `EffectManager` when removing an Actor
- Clear all Actors when combat ends and when transitioning to the next round

`SkillActorManager` does not handle damage calculation, target selection, or skill-learning state.

Only `InGameActionManager` calls `SkillActorManager.Tick(deltaTime)`. No other Manager or UI directly Ticks Skill Actors.

## 11. InGameCombatManager

Coordinates in-game combat results.

Responsibilities:

- Receive skill execution requests
- Invoke the appropriate Executor
- Calculate final damage
- Apply damage
- Apply healing
- Apply shields
- Request status effect application
- Dispatch hit, kill, and skill activation events

Prohibited responsibilities:

- Own skill learning state
- Spawn enemies
- Spawn manifested monsters
- Progress stages and days
- Manage prefab lifecycles
- Own run rewards

`InGameCombatManager` is not a monolithic object that directly implements all logic. It is a coordinator that connects combat execution.

## 12. SpawnManager

Responsible for creating in-game units.

Responsibilities:

- Spawn enemies
- Spawn manifested monsters
- Create Models from Definitions
- Connect Models to scene Actors
- Register created units in the `StageManager` field-unit list

Prohibited responsibilities:

- Decide which enemies appear on which day
- Calculate damage
- Learn skills
- Decide the next stage

## 13. StageManager

Responsible for the `RunSessionModel`, player currencies, and current field progression.

Responsibilities:

- Own the active `RunSessionModel`
- Current stage
- Current day
- Current encounter
- Manage player `Gold`
- Manage player `DarkTrace`
- Add currencies and check whether they can be spent
- Spend currencies
- All field units
- Currently living units
- Start a day
- Progress the enemy spawn sequence
- Determine combat completion
- Advance to the next day
- Advance to the next stage
- Victory and defeat
- Request unit resets when transitioning between rounds
- Command `SpawnManager` to create units

Currency changes are performed only through `StageManager` methods.

```text
AddGold(amount)
CanSpendGold(amount)
SpendGold(amount)
AddDarkTrace(amount)
CanSpendDarkTrace(amount)
SpendDarkTrace(amount)
```

`RewardService`, `OfferingService`, `ManifestationService`, and UI do not directly modify the Gold and DarkTrace fields.

`StageManager` does not implement `PartyRoster` or `PrisonerInventory` itself. It accesses both objects through the active `RunSessionModel`, while each object and Service owns the actual party and prisoner rules.

`StageManager` does not directly reset a unit's internal state fields.

It sends each Model a reset request such as:

```text
monsterModel.ResetForNextDay()
enemyModel.ResetForNextDay()
```

Round-reset targets based on the inspected `MonsterDayRecovery.cs`:

- Remove status effects
- Remove direct shields
- Remove current shields
- Reset skill runtime state
- Fully restore health
- Enable automatic attacks
- Enable automatic skills when the monster is not selected

In the new structure, `StageManager` requests these operations while iterating over units, and each Model performs the actual reset on its own state.

### 13.1 RewardService

After combat ends, converts `StageRewardDefinition` into actual run rewards and grants them.

`RewardService` responsibilities:

- Look up the `StageRewardDefinition` matching the current stage and combat type
- Calculate Gold and DarkTrace rewards
- Determine the prisoner count
- Apply boss and guaranteed-prisoner rules
- Grant Gold and DarkTrace through `StageManager`
- Register prisoners in `PrisonerInventory`
- Return reward results for the UI to display

`RewardService` does not create UI buttons and does not execute manifestation or offering.

### 13.2 OfferingService

Creates offering candidates for the selected party monster and applies the selection result.

`OfferingService` responsibilities:

- Verify that the target monster is in `PartyRoster`
- Look up the target `MonsterSkillBucket`
- Generate learnable active candidates according to the existing appearance rules
- Generate learnable passive candidates according to the existing appearance rules
- Generate selectable enhancement and master candidates according to the existing appearance rules
- Combine all eligible candidates into one list
- Uniformly shuffle all candidates with equal weight
- Return only the first three shuffled results
- Retain a generated candidate set until selection is complete
- Do not offer rerolls
- Apply the selection result to the corresponding `MonsterSkillBucket`
- Consume the used prisoner from `PrisonerInventory`

`OfferingService` does not execute skill combat effects. It changes learning results only.

Existing appearance eligibility rules:

- A maximum of two additional active skills beyond the default A active skill
- A maximum of five passives
- A passive that requires a prerequisite active becomes eligible only after that active has been learned.
- A maximum of three active enhancements per target skill
- An active master becomes eligible only after selecting all three active enhancements for the target skill, and is limited to one
- A maximum of one passive enhancement per target passive
- Exclude items already learned or selected

When the offering button is pressed, uniformly shuffle all candidates that pass the rules above and display up to three. There are no additional per-type weights or fixed ratios.

If the offering panel opens with no eligible candidates, do not consume a prisoner. Consume the prisoner only when one candidate is confirmed.

### 13.3 ManifestationService

Owns the rules for using a prisoner to add a new monster to the party.

`ManifestationService` responsibilities:

- Verify ownership of the selected prisoner
- Find the next empty slot in `PartyRoster`
- Check for duplicate monsters and the maximum party limit
- Immediately consume the prisoner when a manifestation attempt begins
- Build candidates from all player monsters not currently in the party, regardless of `enemy_id`
- Randomly select from all manifestation candidates with equal probability
- Apply the current reward's `manifest_success_chance`
- Do not return the consumed prisoner on failure
- Register the manifested monster in `PartyRoster` on success
- Return the success or failure result
- When recruitment of a successfully manifested monster is confirmed, immediately request placement from `SpawnManager` through `StageManager`
- Place it in the next party slot on the current field without waiting for the next round

`ManifestationService` does not instantiate prefabs directly.

There is no direct mapping between a prisoner's `enemy_id` and an eligible manifestation `monster_id`. The prisoner's `enemy_id` is used for UI display and material identification, not for manifestation candidate selection.

After manifestation succeeds, the existing UI offers a choice to recruit or skip the successful monster. Confirming recruitment immediately registers the monster in the party and spawns it in the scene within the same flow. If skipped, the prisoner remains consumed and the monster is not added to the party.

### 13.4 Run Reward Flow

```text
Combat ends
→ StageManager enters the Reward state
→ RewardService generates rewards
→ Gold and DarkTrace are granted through StageManager
→ Prisoners are registered in PrisonerInventory
→ User selects a prisoner
→ OfferingService or ManifestationService executes
→ Proceed to the next day
```

## 14. StatusDefinition and StatusEffect

### 14.1 StatusDefinition

Stores source definitions for status effects from `status_effects.csv`.

- `status_effect_id`
- `status_effect_label`
- `effect_type`
- `attribute`
- `default_duration_seconds`
- `is_permanent`
- `max_stacks`
- `base_stack_amount`
- `can_move`
- `can_act`
- `can_use_special_skill`
- `action_speed_bonus_per_stack`
- `move_speed_bonus_per_stack`
- `attack_power_bonus_per_stack`
- `damage_taken_bonus_per_stack`
- `critical_damage_taken_bonus_per_stack`
- `critical_resistance_bonus_per_stack`
- `element_resist_reduction_per_stack`
- `element_damage_taken_bonus_per_stack`
- `status_effect_prefab_path`

### 14.2 StatusEffect

Stores and executes the state actually applied to a specific unit.

- `StatusDefinition` reference
- Remaining duration
- Current stacks
- Applying unit
- Affected unit
- Apply effect
- Refresh effect
- Stack effect
- Remove effect

`StatusDefinition` is immutable data. `StatusEffect` is state that changes during combat.

## 15. EffectManager

Manages skill runtime visual effects and prefab visual effects.

Responsibilities:

- Create visual effects
- Instantiate prefabs
- Return an Effect handle for each Actor
- Update position and direction
- Handle termination signals from an Actor or Executor
- Remove visual effects

Prohibited responsibilities:

- Determine damage
- Determine status effects
- Determine skill availability
- Select targets
- Own skill learning state

## 16. Existing UI Structure Reuse and Modification Boundary

Reuse the current `NewRunScene` UI hierarchy and visual layout. This new blueprint does not redesign the UI; it replaces the objects read and called by the existing UI with the new Core APIs.

Reuse targets:

- Main menu and run-entry flow
- RewardPanel
- PrisonPanel
- Party slots from 1P through 5P
- Offering UI
- Manifestation success and failure UI
- Monster panel
- Nexus health display
- Damage meter
- Auto button
- Game speed button
- DebugPanel

UI modification principles:

- UI does not directly modify `UnitBaseModel`, `MonsterSkillBucket`, or currency fields.
- Display Gold and DarkTrace as read-only values from `StageManager`.
- Display party slots in the exact order of `RunSessionModel.PartyRoster`.
- Display the prisoner list from `RunSessionModel.PrisonerInventory`.
- The reward UI displays results returned by `RewardService`.
- The offering UI receives candidates from `OfferingService` and only sends selection commands.
- The manifestation UI sends execution commands to `ManifestationService` and displays success or failure results.
- The Auto UI changes the selected monster's state through `PlayerInputController`.
- The damage meter subscribes to confirmed-damage events from `InGameCombatManager`.
- The Nexus UI displays `NexusModel` state.
- `EffectManager` manages only skill visual effects, not panel UI.

Existing scene object names and layouts may be retained. Any existing UI script that directly reads previous state objects or internal Manager fields must be modified to use the new `StageManager`, `RunSessionModel`, `PartyRoster`, and Service APIs.

### 16.1 GameBootstrap

An entry point is required to start the game and connect the new Core.

`GameBootstrap` responsibilities:

- Execute `CsvParser`
- Create `GameDefinitionCatalog`
- Create `RunSessionModel`
- Initialize `StageManager`
- Initialize `InGameCombatManager` and `InGameActionManager`
- Create and connect `RewardService`, `OfferingService`, and `ManifestationService`
- Connect new Core query and command APIs to existing UI Controllers

`GameBootstrap` does not directly implement game rules.

## 17. Complete Execution Flow

### 17.1 Game Start

```text
Game starts
→ CsvParser
→ Create Definitions
→ Validate ID references
→ Finalize GameDefinitionCatalog
→ GameBootstrap
→ Create RunSessionModel
→ Initialize StageManager and Services
→ Connect existing UI to the new Core APIs
```

### 17.2 Unit Creation

```text
StageManager
→ SpawnManager
→ Look up UnitDefinition
→ Create a UnitBaseModel-derived Model
→ Create SkillBucket
→ Register field unit
```

### 17.3 Automatic Skill Execution

```text
InGameActionManager
→ MonsterActionController or EnemyActionController
→ Look up the skill to use from SkillBucket
→ SkillCooldown.CanUse()
→ Select targets through SkillTargeting
→ Execute the Executor for the skill type
→ InGameCombatManager applies combat results
→ Create required Actors
→ EffectManager creates visual effects
```

### 17.4 Enemy Movement and Nexus Attacks

```text
EnemyActionController checks its attack target
→ If outside attack range, invoke UnitMovementController
→ If inside attack range, execute the enemy skill Executor
→ If there are no player units, move toward the Nexus
→ On Nexus contact, request InGameCombatManager to apply nexus_damage
```

### 17.5 Skill Termination

```text
Actor detects its termination condition
→ Signal combat-effect termination
→ Signal EffectManager to remove the effect
→ Remove Actor
```

### 17.6 Rewards, Offering, and Manifestation

```text
Combat ends
→ RewardService generates rewards
→ StageManager updates Gold and DarkTrace
→ PrisonerInventory registers prisoners
→ Existing PrisonPanel displays PartyRoster and prisoners
→ OfferingService or ManifestationService executes
→ MonsterSkillBucket or PartyRoster changes
```

### 17.7 Next Round

```text
StageManager confirms combat completion
→ Request ResetForNextDay() from all field units
→ Update the current day
→ Look up the next StageDefinition
→ Request SpawnManager to create the next encounter
```

## 18. Implementation Feasibility Assessment

### 18.1 Currently Active Game Elements

This blueprint provides an explicit owner for every active element below.

- Monsters, enemies, and the Nexus
- Party and manifested monsters
- Active, passive, enhancement, and master
- Skill Choice, Node, Trigger
- Damage, healing, shields, and status effects
- Projectile, line, area, single-target attack, and buff
- Automatic actions and manual skill requests
- Enemy movement and Nexus attacks
- Stages, days, encounters, and spawning
- Victory, defeat, and round reset
- Gold, DarkTrace
- Prisoners
- Rewards
- Offering
- Manifestation
- Existing party, reward, prisoner, offering, manifestation, Nexus, damage meter, and Auto UI

Therefore, the currently identified active game elements can be implemented with this structure.

### 18.2 Confirmed Compatibility Rules

The following rules are confirmed by user instructions and inspection of the existing Scripts.

- Do not directly connect a prisoner's `enemy_id` to a manifestation `monster_id`.
- Select manifestation candidates with equal probability from all player monsters not currently in the party.
- Consume the prisoner when a manifestation attempt begins and do not return it on failure.
- Immediately place a manifested monster on the current field when its recruitment is confirmed.
- Select up to three offering candidates with equal probability from all candidates that pass the existing learning, prerequisite, and count limits.
- There are no separate per-offering-type weights and no rerolls.
- Consume the prisoner when an offering selection is confirmed.
- A manual skill uses the selected monster's mouse world-space aim point and direction.
- The order within the central combat frame is cooldown Tick, automatic skills, manual input, enemy actions, and status-effect Tick.

### 18.3 Additional Confirmed Decisions

- Every Skill Actor uses the central `SkillActorManager` Tick rather than an independent Unity `Update()`.
- The manifestation-success popup retains the existing UI flow.
- The user may recruit or skip a successfully manifested monster.
- Recruiting immediately registers the monster in the party and places it on the field.
- Skipping keeps the prisoner consumed and does not add the monster.
- Save/Load does not currently exist and will not be created as part of this structure.
- Gold, DarkTrace, PartyRoster, and PrisonerInventory own only the in-memory state of the current active run.

No additional information is missing that would block implementation of the currently active game elements. If a new semantic gap is discovered during implementation, apply the evidence priority from section 1.1.

### 18.4 Currently Inactive or Data-Only Elements

Although the CSV contains fields for them, shops, events, elite selection, and relic UI have not been identified as active elements and are not confirmed as runtime implementation targets in this blueprint.

Activating these elements requires adding separate Service, state Model, and UI responsibilities.

### 18.5 Complete Replacement and Resource Reconnection Boundary

The final goal is not a coexistence structure that overlays the new structure on top of the existing Scripts.

```text
Retain
├─ Pakuri/Assets/CSVdata
├─ Currently used scene hierarchy and UI objects
├─ Currently used prefabs
├─ Sprites
├─ Animations and AnimatorControllers
└─ Resource values used in the Inspector

Replace
├─ Existing MonoBehaviour connections
├─ Existing ScriptableObject type connections
├─ Existing runtime Managers and static state
├─ Call relationships between existing code
└─ Initialization paths that require existing Scripts
```

The blueprint Markdown file itself cannot be connected to Unity resources. After the Code Builder implements new C# types based on the blueprint, current resources must be reconnected to the new types' Script GUIDs and serialized fields.

During the transition, existing Scripts may be read to compare behavior and connections. However, the new and existing runtimes must not simultaneously execute as authorities over game state. Once a feature-level transition is complete, remove the corresponding existing components from scenes and prefabs and connect only the new components.

Transition scope confirmed by the current inspection:

- 240 Unity serialized files were inspected using the explicit whitelist `.unity`, `.prefab`, `.asset`, `.controller`, `.overrideController`, `.anim`, `.playable`, `.mat`, and `.scenetemplate`.
- 21 existing Script types are referenced 56 times in total across 40 serialized assets.
- Reference locations include scenes, prefabs, `CsvRuntimeCatalog.asset`, and `Legacy/Data/GameData/*.asset`.
- No serialized Animation Event function names were found in `.anim` files.
- There are zero missing `m_Script: {fileID: 0}` connections in current scenes and prefabs.

The retained-resource inventory uses an evidence-defined reachability boundary rather than assuming that every file under `Assets` is active:

1. Start with the 24 non-Legacy serialized assets that currently host existing Script components.
2. Add all non-schema CSV values from columns ending in `_path`.
3. Recursively follow serialized GUID references from those roots to project assets under `Pakuri/Assets`.
4. Exclude existing `.cs`, `.asmdef`, and `.dll` code dependencies because the Script-reference manifest and final dependency-removal gate track them separately.

This reproducible boundary yields 781 resource-reference rows and 593 unique retained project assets. It includes current scenes, prefabs, sprites, animations, AnimatorControllers, fonts, shaders, CSV TextAssets, and referenced data assets. All recorded retained paths exist.

The Inspector snapshot stores the exact serialized MonoBehaviour YAML payload for all 56 existing Script references as UTF-8 Base64 plus SHA-256. This preserves pre-migration Inspector values for the Phase 5 before-and-after mapping without treating the previous component as a runtime dependency.

Therefore, completing the new implementation alone does not finish the transition. Every existing Script GUID reference must be classified and handled in one of the following ways.

1. Reconnect assets required for current gameplay to new components or new ScriptableObject types.
2. For Legacy data assets replaced by CSV parsing and no longer required, verify actual references and obtain user approval either to remove them or archive them outside `Assets`.
3. Explicitly migrate Inspector values still required by the new structure to new serialized fields with equivalent meaning.
4. If any asset still requires a previous type, classify the complete replacement as incomplete.

The final transition may be activated all at once after preparation and verification are completed incrementally. Do not claim that implementation, asset reconnection, compilation, and gameplay verification were completed in a single change without evidence.

### 18.6 Complete Replacement Completion Conditions

All of the following conditions must be satisfied to classify the replacement as complete and capable of normal gameplay.

- New code has zero references to types and namespaces from the existing Scripts.
- New code has zero paths that invoke the existing Scripts through reflection strings, `SendMessage`, fallbacks, or adapters.
- Unity serialized assets under `Pakuri/Assets` have zero references to previous Script GUIDs.
- Scenes and prefabs have zero Missing Scripts.
- Retained CSV files and column names remain unchanged.
- Active prefabs, sprites, animations, and AnimatorControllers are connected to their new owners.
- Combat, actions and movement, stages, rewards, offering, manifestation, and UI flows execute using only the new Core.
- No previous component changes combat state outside the central Tick.
- There are no compilation errors or Unity Console errors.
- The user confirms normal gameplay in Unity Play Mode.

Disconnecting existing `.cs` files from scenes alone does not remove compile-time dependencies. If previous source files remain under `Assets` in the final phase, Unity still compiles them. After all new asset connections and gameplay verification are complete, previous sources must either be deleted with user approval or moved to an archive location outside Unity's compilation scope.

## 19. Final Target Folder and Script Structure

The structure below is the target state after implementation. It does not mean this structure already exists in the current repository.

```text
Pakuri/Assets/Scripts/
├─ Core/
│  ├─ Bootstrap/
│  │  └─ GameBootstrap.cs
│  ├─ Parsing/
│  │  └─ CsvParser.cs
│  ├─ Catalog/
│  │  └─ GameDefinitionCatalog.cs
│  └─ Definitions/
│     ├─ Skills/
│     │  ├─ SkillDefinition.cs
│     │  ├─ ProjectileDefinition.cs
│     │  ├─ LineAttackDefinition.cs
│     │  ├─ AreaAttackDefinition.cs
│     │  ├─ SingleAttackDefinition.cs
│     │  ├─ BuffDefinition.cs
│     │  ├─ HealDefinition.cs
│     │  ├─ ShieldDefinition.cs
│     │  ├─ PassiveDefinition.cs
│     │  └─ SkillTriggerDefinition.cs
│     ├─ Choices/
│     │  ├─ SkillChoiceDefinition.cs
│     │  ├─ ChoiceNodeDefinition.cs
│     │  ├─ NodeTypeDefinition.cs
│     │  └─ NodeParamDefinition.cs
│     ├─ Units/
│     │  ├─ UnitDefinition.cs
│     │  ├─ MonsterDefinition.cs
│     │  └─ EnemyDefinition.cs
│     ├─ Stage/
│     │  ├─ StageDefinition.cs
│     │  ├─ StageDayDefinition.cs
│     │  ├─ StageEncounterDefinition.cs
│     │  └─ StageRewardDefinition.cs
│     └─ Status/
│        └─ StatusDefinition.cs
├─ Run/
│  ├─ RunSessionModel.cs
│  ├─ StageManager.cs
│  ├─ PartyRoster.cs
│  ├─ PrisonerInventory.cs
│  └─ Services/
│     ├─ RewardService.cs
│     ├─ OfferingService.cs
│     └─ ManifestationService.cs
├─ Units/
│  ├─ Models/
│  │  ├─ UnitBaseModel.cs
│  │  ├─ MonsterModel.cs
│  │  ├─ EnemyModel.cs
│  │  └─ NexusModel.cs
│  └─ Actors/
│     ├─ UnitActor.cs
│     ├─ MonsterActor.cs
│     ├─ EnemyActor.cs
│     └─ NexusActor.cs
├─ Combat/
│  ├─ InGameCombatManager.cs
│  ├─ Actions/
│  │  ├─ InGameActionManager.cs
│  │  ├─ UnitActionController.cs
│  │  ├─ MonsterActionController.cs
│  │  ├─ EnemyActionController.cs
│  │  ├─ UnitMovementController.cs
│  │  └─ PlayerInputController.cs
│  ├─ Skills/
│  │  ├─ Runtime/
│  │  │  ├─ SkillBucket.cs
│  │  │  ├─ MonsterSkillBucket.cs
│  │  │  ├─ EnemySkillBucket.cs
│  │  │  └─ SkillCooldown.cs
│  │  ├─ Execution/
│  │  │  ├─ SkillTargeting.cs
│  │  │  ├─ ProjectileExecutor.cs
│  │  │  ├─ LineAttackExecutor.cs
│  │  │  ├─ AreaAttackExecutor.cs
│  │  │  ├─ SingleAttackExecutor.cs
│  │  │  ├─ BuffExecutor.cs
│  │  │  ├─ HealExecutor.cs
│  │  │  ├─ ShieldExecutor.cs
│  │  │  └─ PassiveExecutor.cs
│  │  └─ Actors/
│  │     ├─ SkillActorManager.cs
│  │     ├─ ProjectileActor.cs
│  │     ├─ LineAttackActor.cs
│  │     ├─ AreaAttackActor.cs
│  │     ├─ SingleAttackActor.cs
│  │     └─ BuffActor.cs
│  ├─ Status/
│  │  └─ StatusEffect.cs
│  └─ Effects/
│     └─ EffectManager.cs
├─ Spawn/
│  └─ SpawnManager.cs
└─ UI/
   ├─ MainMenu/
   │  └─ Reuse and modify the existing MainMenu UI scripts
   └─ InGame/
      ├─ InGameUIManager.cs
      ├─ RewardPanelController.cs
      ├─ PrisonPanelController.cs
      ├─ OfferingPanelController.cs
      ├─ ManifestationPanelController.cs
      ├─ MonsterPanel/
      │  └─ Reuse and modify the existing MonsterPanel UI scripts
      ├─ Nexus/
      │  └─ Reuse and modify the existing Nexus UI scripts
      ├─ DamageMeter/
      │  └─ Reuse and modify the existing DamageMeter UI scripts
      ├─ UtilityPanel/
      │  └─ Reuse and modify the existing Auto and time-scale UI scripts
      └─ Debug/
         └─ Reuse and modify the existing Debug UI scripts
```

### 19.1 Core

The data layer for game startup.

- `CsvParser` reads the retained CSV files.
- Definitions preserve CSV terminology exactly.
- `GameDefinitionCatalog` provides validated Definitions.
- `GameBootstrap` connects Managers, Services, and the existing UI.

### 19.2 Run

The run-progression layer retained between combats.

- `RunSessionModel` groups the current run state.
- `StageManager` manages the stage, day, field units, Gold, and DarkTrace.
- `PartyRoster` manages party order from 1P through 5P.
- `PrisonerInventory` manages prisoners.
- The three Services respectively execute reward, offering, and manifestation rules.

### 19.3 Units

The unit layer between Definitions and scene GameObjects.

- A Model owns current health, status, and skill state.
- An Actor connects Transform, Collider, Animation, and scene presentation.
- A Model does not directly find UI or prefabs.

### 19.4 Combat

The combat decision and execution layer.

- `InGameCombatManager` coordinates the results of damage, healing, shields, and status application.
- The Actions folder handles automatic actions, manual input, enemy decisions, and movement.
- Skill Runtime owns per-unit learning and cooldown state.
- Executors execute game effects by skill type.
- Skill Actors own the lifecycles of instantiated skills.
- `EffectManager` handles only visual effects.

### 19.5 Spawn

Creates unit Models and Actors from Definitions and registers them with `StageManager`.

It does not own stage selection, manifestation success determination, party limits, or damage calculation.

### 19.6 UI

Reuses the current scene hierarchy and visual layout.

UI scripts display read-only state from the new Core and send commands to Services or Controllers. They do not directly modify Model fields, currencies, or SkillBuckets.

## 20. Coding Guidelines and Conventions

### 20.1 Implementation Reference Standards

Before the transition, inspect the actual code under `Pakuri/Assets/Scripts` as read-only evidence and use it as a reference for the following:

- Current gameplay behavior
- Unity component connection patterns
- Input handling patterns
- Combat-result delivery patterns
- Existing UI object and event connections
- Naming, braces, and indentation style
- Error handling and Unity serialization boundaries

Do not copy the existing Scripts' massive classes, duplicated state, temporary fallbacks, or indirect call structures as-is. Reference their behavior and writing style, while following this blueprint for responsibility allocation.

Using them as a reference does not imply source dependency. If new code invokes, inherits, or wraps existing types, or sends results back to an existing Manager, it violates the complete replacement conditions.

Implementation priority:

```text
Responsibility boundaries in the blueprint
→ User-confirmed gameplay rules
→ CSV contracts
→ Behavior and coding style of the existing Scripts
```

### 20.2 Files and Names

- Match the primary public type name in a file to the file name.
- Use `PascalCase` for types, methods, and properties.
- Use `camelCase` for private fields and local variables.
- As required by the user, CSV-backed fields in CSV Definitions use the exact actual CSV column names.
- Do not create misspelled names such as `UnitDefinotion`, `Skillbucket`, or `CSVparser`.
- Use the `Manager`, `Service`, `Controller`, `Model`, `Definition`, and `Actor` suffixes only when they match the responsibility meanings defined in this document.
- Do not use semantically unclear names such as `Helper`, `Util`, `Common`, `Temp`, `Data2`, or `New`.

### 20.3 Separation of Responsibilities

- One class has one state authority or one execution responsibility.
- A Manager coordinates flow and does not reimplement the detailed rules of subordinate objects.
- A Model owns its mutable state and does not find UI or prefabs.
- A Definition is immutable data and does not own runtime state.
- A Service executes one domain rule and does not directly instantiate scene objects.
- An Executor executes skill effects and does not own learning state or a visual-effect list.
- An Actor handles lifecycle and scene presentation and does not own damage formulas or skill learning state.
- UI displays state and delivers commands; it does not directly modify Core state.

When a second independent responsibility becomes necessary, first explain its boundary with the existing responsibility and then separate it. Do not add a pass-through wrapper class merely because a method is long.

### 20.4 Single Authority

Do not store the same fact in two or more independently mutable locations.

Examples of authority:

- Gold and DarkTrace: `StageManager`
- Party order: `PartyRoster`
- Prisoners in the current reward phase: `PrisonerInventory`
- Monster learning state: the corresponding `MonsterSkillBucket`
- Enemy skill state: the corresponding `EnemySkillBucket`
- Unit health, shields, and statuses: the corresponding `UnitBaseModel`
- Cooldowns, magazines, and reloads: the corresponding `SkillCooldown`
- Active Skill Actor list: `SkillActorManager`
- Immutable game data: `GameDefinitionCatalog`

UI display values must be projections read from the authorities above. Do not create separately writable copies in the UI.

### 20.5 Direct Paths and Prohibition of Unnecessary Indirection

Do not create the following structures unless they provide a necessary transformation, lifecycle, or dependency boundary.

```text
A → B → A
A → Wrapper → Actual A method
Model → Temporary DTO → Restore the same Model
Service → Manager → Invoke the same Service again
```

If a method only forwards a call, the code and blueprint must demonstrate why that boundary is necessary.

Use events only when actual multi-subscriber delivery, UI separation, asynchronous lifecycle, or re-entry prevention is required.

### 20.6 Validation and Fallbacks

Perform validation once at untrusted boundaries.

- CSV input: `CsvParser`
- ID connections and duplicates: before creating `GameDefinitionCatalog`
- Unity Inspector and scene references: `GameBootstrap` or initialization of the relevant Actor
- User UI input: the public entry point of the relevant UI Controller and Service
- Externally callable public API: that API's entry point

After initialization succeeds, do not repeat the same null, ID, enum, and collection checks on every internal call.

When required data is missing, do not silently continue with arbitrary defaults, temporary objects, or fallbacks to the previous system. Return an explicit error or fail initialization.

Do not add numeric code fallbacks for values whose authority is the CSV, such as `manifest_success_chance`, skill values, or status-effect values.

### 20.7 Prohibition of Dead Code and Speculative Expansion

- Do not create public APIs in advance when they have no current caller.
- Do not create interfaces, fields, or empty methods for future Save/Load.
- Do not create stubs for shops, events, or relics that are not currently active.
- Do not register empty Executors, Actors, or Services.
- Do not create fields with no reader or write-only state.
- Do not create unused overloads or general-purpose string lookup methods.
- Do not leave compatibility branches that never execute.
- Do not connect an execution path containing only a `TODO` as though it were complete functionality.

Before adding a new type, first record these four items.

```text
Owner   = Who owns this type's lifecycle
Caller  = Who actually invokes this type
State   = Which state this type uniquely owns
Delete  = Under what condition this type becomes unnecessary
```

Do not add the type if any one of the four items has no answer.

### 20.8 Access Scope and Dependency Direction

- Fields are `private` by default.
- When external reading is required, provide a read-only property or an explicit query method.
- Changes are made only through command methods on the relevant state authority.
- Prefer `internal` for implementation-only types.
- Use `[SerializeField] private` only for fields that require Unity Inspector connections.
- Core Definitions do not depend on Unity scenes or UI.
- Run does not depend on UI.
- Combat does not depend on UI panels.
- UI depends on public query and command APIs from Core and Run.
- Do not create circular dependencies.

### 20.9 Central Tick and Unity Lifecycle

- Provide exactly one combat Tick entry point.
- `InGameActionManager` Ticks Cooldown, automatic skills, manual input, enemy actions, Skill Actors, and Status in the defined order.
- Each Skill Actor does not have an independent `Update()`.
- Only `SkillActorManager` Ticks Skill Actors.
- During Tick, use `pendingAdd` and `pendingRemove` rather than directly adding to or removing from collections.
- New Actors begin Ticking on the next frame.
- Clear both the central Actor list and pending lists when combat ends or the next day begins.
- Ensure that a UI display `Update()` does not change Core game state.

### 20.10 Method Design

- One method performs one action or decision.
- Express expected failures with `Try...` or `Can...` results.
- Do not silently ignore initialization invariant violations.
- Meaningful intermediate values and lifecycle captures may be retained as local variables.
- Do not create a local variable that merely copies an existing value under a different name.
- Do not search the entire scene or use `FindObjectsOfType` every frame.
- If list order is a game rule, explicitly define sort or registration order.
- At the call site, make the candidate list and uniform or weighted random-selection rules explicit.

### 20.11 Comments

As in the existing Scripts, use short Korean comments where responsibilities and execution rationale need explanation.

Comment targets:

- Frame execution order
- State authority
- Whether a resource is consumed on failure
- Candidate exclusion conditions
- Lifecycle decisions such as next-frame registration
- Places where a CSV field and its runtime meaning may appear different

Do not write comments that merely restate the code in Korean.

### 20.12 Naive Code Filter Readiness Check

The implementer self-checks each file against the following criteria.

- Does every type and method have an actual caller?
- Does every field have the required writer and reader?
- Is the same state modified in more than one place?
- Is there any unnecessary round trip that enters another object and returns to the original object?
- Are the same validation and fallback checks repeated after initialization?
- Does a pass-through wrapper actually provide a necessary boundary?
- Are there unused overloads, temporary variables, caches, or compatibility branches?
- Were dynamic references such as UnityEvent, Inspector, scene, prefab, and animation event checked?
- Does a removable previous authority remain permanently alongside the new authority?

Naive Code Filter is an inspection-only role. It neither automatically approves nor modifies the actual implementation. When separately requested, run it against the exact script or folder.

### 20.13 Existing Scripts Dependency Removal Check

Before the final transition, perform the following checks as a separate gate.

- Check new `.cs` files for references to existing namespaces and types
- Check Unity serialized files, including scenes, prefabs, `.asset` files, and AnimatorControllers, for previous Script GUIDs
- Check for Missing Scripts
- Compare Inspector serialized values before and after migration
- Check whether existing and new Managers execute simultaneously
- Check Unity recompilation with previous sources removed
- User Play Mode full-flow check

If any one of these checks fails, removal of dependencies on the previous Scripts and implementation of normal gameplay remain incomplete.

## 21. Complete Replacement Work Phase Plan

### 21.1 Single Location for Work Records

This complete replacement work does not mix its state with other active work boards.

- Update progress, blockers, next actions, and check results only in this document.
- Do not modify the root `BLACKBOARD.md`.
- Do not modify files under `boards/` whose names contain `BLACKBOARD.md`.
- Do not duplicate records in MON, COMBAT, DATA, RUN, UI, or OPS boards because of Phase progress.
- After prompt reset, session restart, or reboot, check `21.11 Phase Execution Records` in this document for the last incomplete Phase and its next action.
- If the user separately and explicitly requests a board update, reassess only the scope of that request.

This rule applies only to the complete replacement work governed by `new-core-architecture-blueprint.md`. It does not change the recording policy for other independent work.

### 21.2 Common Execution Gate for Every Phase

Proceed through each Phase in this order.

```text
Confirm Phase scope
→ Check Unity Console state before changes
→ Implement only the current Phase scope
→ Unity Refresh and recompile
→ Wait for compilation to complete
→ Check Unity Console Errors, Exceptions, and Warnings
→ Perform static file, reference, and test checks
→ Request user Play Mode verification only when required
→ Update the Phase execution record
→ Proceed to the next Phase when exit conditions are satisfied
```

Common exit conditions:

- There are zero Unity compilation errors.
- There are zero new Errors and Exceptions caused by the current Phase changes.
- Record the cause and disposition of Warnings in the execution record.
- A Phase that changes the Inspector, scenes, prefabs, or `.asset` files checks for Missing Scripts and missing required references.
- Record executed tests or check commands and their actual results in Evidence.
- Do not add temporary fallbacks, empty components, or calls to previous Managers to conceal failed checks.
- Do not proceed to the next Phase until the exit conditions pass.

Console check procedure:

1. Before changes, read current Errors, Exceptions, and Warnings and record the existing log baseline.
2. Clear the Console so previous and new logs can be distinguished.
3. Run Asset Refresh and recompilation, then wait for Unity compilation to complete.
4. Check Errors and Exceptions first, then check Warnings separately.
5. If errors exist, resolve them within the current Phase based on the exact stack and associated files.
6. After resolving them, repeat the same procedure.

### 21.3 Minimal Play Mode Execution Rules

Unity Play Mode gameplay verification belongs to the user. Codex does not start Play Mode on its own.

Conditions under which Play Mode may be requested:

- Frame execution order or `Time.deltaTime` behavior must be verified.
- Collider, Rigidbody, collision, or projectile movement must be verified.
- Actual keyboard and mouse input and automatic-combat transitions must be verified.
- Animator, effects, UI display, and button flow must be verified in the actual scene.
- An integrated flow crossing multiple systems, such as day transition, reward, offering, or manifestation, must be verified.
- Full gameplay must be verified after final removal of the existing Scripts.

Cases that do not require requesting Play Mode:

- C# compilation error checks
- CSV column, ID, duplicate, and referential-integrity checks
- Checks for references to existing namespaces and types in new code
- Script GUID checks in Unity serialized files
- Static Missing Script checks
- Deterministic tests of pure Models, Definitions, and Services

Required record before requesting Play Mode:

```text
Reason        = Why static checks cannot prove the behavior
Scene         = Exact scene to run
Setup         = Required starting state
Actions       = Inputs the user must perform
Expected      = Expected result
Failure       = Failure criteria
LogCheck      = Console logs to check after completion
```

Even when a Phase requires Play Mode, the compilation and Console gates must pass first. If verification scenarios from adjacent Phases can be combined into one short integrated run, do not run them redundantly.

### 21.4 Phase 0 — Freeze the Baseline and Transition Inventory

**Task title:** Existing Scripts Complete Replacement Baseline

**Goals:**

- Freeze the list of retained CSVs and active resources.
- Create a complete list of scenes, prefabs, and `.asset` files that reference existing Script GUIDs.
- Freeze behavior preserved from existing gameplay and user-confirmed rules as acceptance conditions.

**Constraints:**

- Do not change game code, CSVs, scenes, or prefabs.
- Inspect the existing Scripts only as read-only evidence of behavior and connections.
- Reconfirm the current counts—240 serialized files in the explicit extension scope, 21 existing Script types, 40 assets, and 56 references—with a repeatable check.

**Role Owner:** The Designer confirms the baseline, and the Code Builder provides repeatable check results.

**Status:** PASS — Code Reviewer loop 3

**Next Actions:**

- Begin Phase 1 without changing the retained CSV files.
- Use the CSV contract manifest to detect accidental data changes.
- Preserve the Phase 0 migration and Inspector manifests unchanged as the before-state baseline.

**Evidence:**

- `new-core-phase0-csv-contract-manifest.csv`: 42 retained CSV files; every recorded SHA-256 and byte count matches the current file; zero mismatches.
- `new-core-phase0-script-reference-manifest.csv`: 56 serialized references, 21 unique existing Script GUIDs, and 40 unique assets; 16 rows are under a Legacy path and 40 rows are outside a Legacy path.
- `new-core-phase0-retained-resource-manifest.csv`: 781 evidence rows and 593 unique retained project assets reachable from 24 non-Legacy migration roots, 86 concrete CSV path references, and recursive serialized GUID edges; zero missing retained paths.
- `new-core-phase0-inspector-snapshot.csv`: 56 exact component payloads; all Base64 payloads decode and match their recorded SHA-256.
- `new-core-phase0-manifest-generator.ps1`: repeatably regenerates all four Phase 0 manifests from the inspected project state.
- Repeatable serialization scan: 240 files in the explicit extension scope, 56 matching references, 21 unique existing Script types, and 40 unique assets.
- Existing code baseline: 69 C# files and 38,083 lines under `Pakuri/Assets/Scripts`.
- Unity 6000.3.14f1 project `Pakuri`; Asset Refresh and requested recompilation completed; Editor returned to idle and ready state.
- Unity Console after refresh and recompilation: zero Errors, Exceptions, and Warnings.
- Blueprint translation-only snapshot before Phase execution records were appended: 2,063 split lines, 107 headings, and 72 code-fence lines. The current document intentionally grows as records are appended; current-file QA separately requires balanced fences, zero Hangul characters, zero trailing-whitespace lines, and all Phase 0 through Phase 6 headings.

**History:** 2026-07-23 Phase plan created. 2026-07-23 Code Builder completed the baseline and submitted it to Code Reviewer. Reviewer loop 1 returned FIX REQUIRED; the requested resource, Inspector, QA-scope, and serialization-scope fixes were applied. Reviewer loop 2 found only an unsupported material-coverage word; it was removed. Reviewer loop 3 returned PASS.

**Play Mode:** Do not run by default. Request baseline confirmation from the user only if existing gameplay behavior is discovered that cannot be confirmed through static checks.

**Exit Gate:** The retained-asset list, previous-Script reference list, and gameplay-compatibility list must be confirmed without contradictions.

### 21.5 Phase 1 — CSV Definition and Bootstrap Foundation

**Task title:** New Core Data Foundation

**Goals:**

- Implement a Definition layer that preserves CSV column names.
- Implement the initialization boundaries of `CsvParser`, `GameDefinitionCatalog`, and `GameBootstrap`.
- Ensure invalid CSVs, duplicate IDs, and missing references fail explicitly during initialization.

**Constraints:**

- Do not change files, column names, or values under `Pakuri/Assets/CSVdata`.
- Do not call existing data types or Parsers.
- Do not preimplement runtime Models, combat, or UI.

**Role Owner:** Code Builder

**Status:** PASS — Code Reviewer loop 2

**Next Actions:** Begin Phase 2 using the immutable Definitions and Catalog as inputs. Preserve all 42 retained CSV files and Phase 0 manifests.

**Evidence:** The new `Pakuri.NewCore` and `Pakuri.NewCore.EditMode.Tests` assemblies compile. All 42 retained CSVs parse into 1,836 immutable Definitions, including 39 schema rows and the empty enemy single-attack Trigger CSV. EditMode job `e47a8c682bfd4609979c2060f1c0b8b7` passed 13 of 13 tests covering the retained-data success path, quoted and actual-CRLF multiline fields, immutability, eager rejection of blank required Monster fields, duplicate IDs, invalid int/float/bool/enum values, missing references, missing retained sources, and malformed quotes. The final Unity Console gate has zero Errors, Exceptions, and Warnings.

**History:** 2026-07-23 Phase plan created. 2026-07-24 Code Builder implemented the isolated data foundation and submitted it to Code Reviewer after all deterministic checks passed. Reviewer loop 1 returned FIX REQUIRED for lazy required-field checks, missing actual-newline quoted-field coverage, and inaccurate top-status and production-caller wording. Code Builder added eager construction validation, blank-required-field and CRLF tests, and corrected the Phase records. Reviewer loop 2 returned PASS with 13 of 13 independent EditMode tests passing and no user-only verification gap.

**Play Mode:** Do not run. Verify parsing and the catalog through non-runtime or deterministic tests.

**Exit Gate:** Retained CSVs must parse into new Definitions, and both success and failure paths must be verified without existing types.

### 21.6 Phase 2 — Run State and Unit Models

**Task title:** Establish New State Authorities

**Goals:**

- Implement `RunSessionModel`, `StageManager`, `PartyRoster`, and `PrisonerInventory`.
- Implement `UnitBaseModel`, `MonsterModel`, `EnemyModel`, and `NexusModel`.
- Implement ownership and lifecycles for SkillBuckets, SkillCooldowns, and status effects.

**Constraints:**

- Models do not search scenes, prefabs, or UI.
- Gold, DarkTrace, party, prisoners, health, and skill learning state are not modified by more than one object.
- Do not replace existing scene components yet.

**Role Owner:** Code Builder

**Status:** PASS — Code Reviewer loop 2

**Next Actions:** Begin Phase 3 using the verified state authorities and immutable Definitions. Preserve the Phase 0 artifacts and all retained CSV hashes.

**Evidence:** The isolated `Pakuri.NewCore.Runtime` assembly compiles without Unity Engine or previous-Script references. Its 13 runtime C# files implement the Run, Model, SkillBucket, SkillCooldown, and StatusEffect authorities. After Code Reviewer loop 1 requested two fixes, EditMode job `9bd4afbde4f24aaaa8d93c9569e75ad8` passed all 27 tests: the 13 retained Phase 1 tests and 14 Phase 2 state-transition, edge-case, ownership, compatibility, PassiveBase-prerequisite, and atomic-refresh tests. All 42 retained CSV hashes match the Phase 0 contract manifest, every Phase 0 artifact hash is unchanged, and the final forced Asset Refresh and compilation gate has zero Errors, Exceptions, and Warnings.

**History:** 2026-07-23 Phase plan created. 2026-07-24 Code Builder implemented and locally verified Phase 2, then submitted it to Code Reviewer. Reviewer loop 1 returned FIX REQUIRED because PassiveBase prerequisites were not enforced during both learning and selection, and StatusEffect refresh could partially mutate stacks before rejecting an invalid duration. Code Builder applied both exact fixes. Reviewer loop 2 independently passed 27 of 27 EditMode tests and returned PASS with no user-only verification gap. Phase 3 was not started.

**Play Mode:** Do not run. Verify pure state transitions with deterministic tests.

**Exit Gate:** Each mutable state has exactly one writer, and new Models operate without existing runtime types.

### 21.7 Phase 3 — Central Combat, Actions, and Movement

**Task title:** New Combat Execution Loop

**Goals:**

- Implement the central `InGameActionManager` Tick.
- Implement the execution order for damage, targeting, automatic and manual skills, enemy actions, movement, and status effects.
- Implement responsibilities among Executors, SkillActorManager, Skill Actors, and EffectManager.

**Constraints:**

- Skill Actors do not use independent `Update()` methods.
- Do not call existing Combat Managers, Executors, Actors, or input Controllers.
- Use pending lists for collection changes during Tick.

**Role Owner:** Code Builder

**Status:** Pending Code Reviewer loop 2

**Next Actions:** Run Phase 3 Code Reviewer loop 2. If the Reviewer returns FIX REQUIRED, keep Phase 4 untouched and continue repairing Phase 3 until the Reviewer returns PASS.

**Evidence:** The isolated pure-C# runtime adds the exact eight-step central Tick, result coordination, targeting, action and movement Controllers, manual-input request boundary, eight skill-family Executors, centralized Actor pending lists, Effect handles, scheduled burst/pierce/repeat execution, retained Effect graph execution, and dispatch for all eight retained Trigger events. Reviewer loop 1 exposed retained-row semantic gaps in Choice ownership, Effect/Trigger targeting, projectile and repeated attacks, Trigger context and lifecycle, geometry, passive execution, shield ownership, Nexus cleanup, and deterministic ordering. Code Builder repaired those exact paths and added deterministic regressions. Runtime construction still audits all 88 reachable node types and all 59 Trigger rows with unsupported counts of zero. EditMode job `ce06dd99c06f41fdb82766e32549a3d3` passed 55 of 55 tests, including all retained Phase 1 and Phase 2 tests. All 42 retained CSV hashes have zero mismatches, the 49-file isolated runtime scan has zero forbidden engine/previous-runtime markers and zero missing `.meta` pairs, and the final Unity recompilation gate has zero Errors, Exceptions, and Warnings.

**History:** 2026-07-23 Phase plan created. 2026-07-24 Code Builder implemented the engine-agnostic Phase 3 combat loop, closed the initial node/Trigger coverage gaps, passed the complete deterministic suite and final Console gate, and submitted Phase 3 to Code Reviewer. Reviewer loop 1 returned FIX REQUIRED with 20 exact blockers. Code Builder repaired all listed Phase 3 paths, expanded the combat test class from 15 to 27 tests, passed the 54-test complete suite, and resubmitted Phase 3 for reviewer loop 2 without starting Phase 4.

**Play Mode:** Conditional. Request a limited scene scenario from the user only for items that cannot be proven by static or deterministic tests, such as physics collisions, frame timing, and mouse aiming.

**Exit Gate:** Only one central Tick changes combat state, and the execution and termination of every active skill family must be verified without existing types.

### 21.8 Phase 4 — Stage, Spawn, and Reward Services

**Task title:** New Run Progression Flow

**Goals:**

- Implement stage and day transitions, enemy spawning, victory and defeat, and round resets.
- Implement Reward, Offering, and Manifestation flows.
- Implement the confirmed rules for prisoner consumption, uniform candidate selection, recruitment, and skipping.

**Constraints:**

- Run state is changed only through the authorities from Phase 2.
- Services do not directly find scene objects or modify UI.
- Do not invoke existing StageManager, RunSession, SpawnManager, or UI Manager as fallbacks.

**Role Owner:** Code Builder

**Status:** Not Started

**Next Actions:** Confirm public APIs that connect the Phase 2 state authorities to the Phase 3 combat-completion signal.

**Evidence:** Day and stage state transitions, reward-candidate and prisoner-consumption tests, Unity recompilation, and Console results.

**History:** 2026-07-23 Phase plan created.

**Play Mode:** Do not run by default. If an issue remains that occurs only in the actual scene lifecycle across multiple systems, combine it with Phase 5 integrated verification and run it once.

**Exit Gate:** State transitions from combat start through reward completion and entry into the next day must be verified without UI.

### 21.9 Phase 5 — Reconnect Current Resources

**Task title:** Scene, Prefab, UI, and Visual Resource Migration

**Goals:**

- Connect the current scene hierarchy and UI objects to new Controller and Service APIs.
- Connect active unit and skill prefabs to new Actors and visual boundaries.
- Migrate sprites, animations, AnimatorControllers, and Inspector values to their new owners.
- Replace `CsvRuntimeCatalog.asset` to match the new initialization boundary.

**Constraints:**

- Do not create or replace visual resources required for current gameplay without evidence.
- Do not simultaneously execute existing and new components as authorities over the same state.
- Record a before-and-after mapping table for serialized field values.

**Role Owner:** The Code Builder performs asset connections, and the user verifies gameplay in Play Mode.

**Status:** Not Started

**Next Actions:** Convert the Phase 0 asset-connection table into a migration checklist for each new component.

**Evidence:** Scene, prefab, and `.asset` reference checks; Missing Script checks; Inspector mapping table; Unity recompilation; and Console results.

**History:** 2026-07-23 Phase plan created.

**Play Mode:** Required. After compilation and static connection checks succeed, request from the user an exact integrated scenario combining input, combat, UI, effects, and stage transitions.

**Exit Gate:** The active game flow must execute using only the new Core and new components, and current resources must display correctly.

### 21.10 Phase 6 — Remove Existing Scripts and Complete the Final Transition

**Task title:** Zero Existing Dependencies and Final Acceptance

**Goals:**

- Reduce all references to previous Script GUIDs to zero.
- Remove obsolete existing `.cs` files and Legacy serialized assets through an approved method, or move them outside Unity's compilation scope.
- Accept full gameplay with only the new Core remaining.

**Constraints:**

- Delete source files or move them outside `Assets` only after presenting an exact target list and obtaining user approval.
- Do not retain previous-code fallbacks, compatibility components, or empty replacement components.
- Completion must satisfy both sections 18.6 and 20.13.

**Role Owner:** The Code Builder provides static-transition and log evidence, and the user performs final Play Mode verification.

**Status:** Not Started

**Next Actions:** After Phase 5 succeeds, present exact paths for removal and retention targets and obtain approval.

**Evidence:** Zero existing-type references, zero previous Script GUID references, zero Missing Scripts, Unity recompilation, Console results, and final user Play Mode results.

**History:** 2026-07-23 Phase plan created.

**Play Mode:** Required. After removal of previous sources and recompilation succeed, request the final full-run scenario from the user.

**Exit Gate:** The project must compile without existing Scripts, have no Console errors, and receive user confirmation of normal gameplay.

### 21.11 Phase Execution Records

Add the newest record first under this section.

## Phase Record — 2026-07-24 03:13 +09:00

Task title: New Combat Execution Loop — Code Reviewer Loop 1 Repairs

Goals: Close every Phase 3 Reviewer loop-1 blocker without widening into Phase 4: exact retained Choice/graph ownership, status thresholds and stack scaling, repeated and follow-up attacks, Trigger context and graph routing, real EventTarget/AppliedTargets timing, Area/Line geometry reevaluation, source-specific shields, built-in passives, Nexus one-shot removal, combat cleanup, stable ordering, and validation-before-mutation.

Constraints: Only Phase 3 pure runtime code, deterministic EditMode tests, the Phase 3 plan status, and this newest record changed. No Stage progression, Spawn, Reward, Offering, Manifestation, scene, prefab, `.asset`, UI, Unity input polling, Unity physics, retained CSV, Phase 0 artifact, BLACKBOARD-family file, or Phase 4 through Phase 6 implementation changed. No other Markdown was read or modified. Play Mode was not run.

Role Owner: Code Builder; resubmitted to Code Reviewer loop 2.

Status: Pending Code Reviewer loop 2

Next Actions: Code Reviewer independently inspect the repaired retained-row semantics and rerun the full EditMode suite and final Console gate. Keep Phase 4 untouched until Phase 3 receives PASS. If any blocker remains, return to Code Builder and continue the loop.

Changed Paths:

- `Pakuri/Assets/Scripts/Units/Models/UnitBaseModel.cs`
- `Pakuri/Assets/Scripts/Combat/Actions/NewCore/`
- `Pakuri/Assets/Scripts/Combat/NewCore/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/NewCore/`
- `Pakuri/Assets/Scripts/Core/Tests/Editor/NewCoreCombatLoopTests.cs`
- `Pakuri/reference/Work/new-core-architecture-blueprint.md`

Evidence:

- Choice and graph ownership: plans include the current Skill owner and only selected Choices whose effective target is that Skill. Effect graphs use the same scope; Trigger graphs use the exact retained `triggered_graph_owner_id`, kind, and index without incorrectly applying the event-Skill target filter.
- Hit timing and targets: OnCast and OnHit Effect graphs execute separately. Each completed damage hit records its real EventTarget and AppliedTargets; EventTarget and AppliedTargets resolution has no caster fallback. `visual_anchor_mode=AppliedTargets` no longer overrides an explicit AllAllies effect target.
- Retained node semantics: `ThresholdApplyStatus(name-mark,10,silence)` runs after the base status, Eve-D target-status-stack damage receives its retained per-stack rate bonus, SingleAttack executes `RepeatPerTarget`, projectile burst index 0 is exact, and `FollowUpProjectile` is a distinct scheduled shot rather than an ordinary projectile-count increase.
- Status payloads: an Effect graph containing ApplyStatus plus AttachStatusPayload performs the attached payload application once. `ConditionStatus` reads its retained minimum stacks from `arg_4`.
- Trigger semantics: dispatcher context now carries applied damage, per-source absorbed shield amount, expired shield amount, tracked incoming damage and attribute, event-execute state, source-status skill and stacks, source scope, geometry, count, shape, center, and exact triggered graph identity. Blank retained physical attributes normalize to Physical for matching. Combat reset clears count, cooldown, and recursion state.
- Trigger recursion: every Trigger-created Skill request and Effect-damage request carries its Trigger ancestry through delayed Actors. A Trigger whose ID is already in that ancestry cannot dispatch again, including after the originating Actor callback has completed. Dedicated job `bdcd77da13c64426bf8395efb7b1ce2b` passed the retained Sein auto-barrage recursion regression.
- Shield ownership and reflection: shields are stored and removed as source-and-skill-specific layers. Absorption reports each consumed layer's owner, so `ariel-b-master-2` reflects the retained 35% of the amount absorbed even when Ariel shielded another ally; one shield expiry cannot erase another source's shield.
- Geometry and lifecycle: Area and Line attacks query the full registered candidate set and reevaluate positions every scheduled tick; Line applies its forward half-plane and full width. Nexus contact is terminal and the controller is removed after exactly one damage request. EndCombat clears Actors and Trigger state and unsubscribes every observed status-expiry handler.
- Passive and deterministic flow: learned monster passives and assigned enemy passives have a built-in central caller. Enemy DamageUp, DefenseUp, CritChanceUp, CritDamageUp, HealingUp, and IncomingDamageDown values are read by combat calculations. Equal-distance targeting preserves registration order, and invalid selected-monster registration validates before collection mutation.
- Phase 3 combat regression job `cbff5c430cb04cadbddb661c8638f001`: 27 passed, 0 failed, 0 skipped.
- Complete EditMode job `ce06dd99c06f41fdb82766e32549a3d3`: 55 passed, 0 failed, 0 skipped, result `Passed`; this includes all retained Phase 1 and Phase 2 tests.
- Retained CSV contract: 42 manifest rows checked, SHA-256 mismatches = 0.
- Static isolation: 49 runtime C# files and 7,560 lines checked; forbidden `UnityEngine`, `MonoBehaviour`, `ScriptableObject`, `GameObject`, `SendMessage`, previous `Pakuri.InGame`, `TODO`, `FIXME`, and independent `Update()` matches = 0; missing `.meta` pairs = 0.

Unity Before Log: The Editor was idle and not in Play Mode. The prior test and tool session had no new-runtime compile error.

Unity Compile Result: A final forced Script Refresh and requested compilation completed in Unity 6000.3.14f1; the Editor returned to idle with no pending domain reload.

Unity Error/Exception: 0 after clearing the Unity-MCP transport's own disposed-client diagnostic and reading the final Console gate.

Unity Warning: 0 at the final Console gate.

Play Mode: Not Run

Play Mode Reason: All Reviewer loop-1 blockers are pure deterministic Phase 3 ownership, scheduling, state, geometry-math, and lifecycle contracts. The 54-test EditMode suite, static isolation scan, retained hashes, forced compilation, and final Console gate prove this scope. Phase 5 presentation, real mouse polling, Unity physics, and integrated scene wiring still do not exist, so Play Mode would not add evidence for these repairs.

User Result: Not requested for Phase 3.

History: Reviewer loop 1 returned FIX REQUIRED with 20 exact blockers despite the earlier 42-test pass. Code Builder kept Phase 4 untouched, repaired each retained-row behavior, and used successive Unity compilations and focused test runs to expose two additional mistakes: lowercase `global` target counts were parsed as integers, and `visual_anchor_mode=AppliedTargets` was incorrectly treated as effect-target selection. Both were corrected. A final static recursion audit added ancestry propagation for delayed Trigger-created skills and effects and an exact Sein regression. The complete suite passed 55 of 55, all 42 retained CSV hashes matched, the isolated runtime scan found zero forbidden markers and zero missing `.meta` pairs, and the final Console gate was zero. Phase 3 is resubmitted for Code Reviewer loop 2.

## Phase Record — 2026-07-24 02:05 +09:00

Task title: New Combat Execution Loop

Goals: Establish one central combat Tick; deterministic damage, healing, shield, status, cooldown, targeting, automatic, manual-request, enemy, movement, Actor, and effect-handle paths; and an Executor for every retained active skill family.

Constraints: Only Phase 3 pure runtime code, the minimum Phase 2 commands required for current position and cooldown reduction, deterministic EditMode tests, this Phase 3 plan status, and this newest record were changed. No Stage progression, Spawn, Reward, Offering, Manifestation, scene, prefab, `.asset`, UI, Unity input polling, Unity physics, retained CSV, Phase 0 artifact, BLACKBOARD-family file, or Phase 4 through Phase 6 implementation was added or modified. Existing Scripts were restored byte-for-byte after an initial path collision and remain read-only compatibility evidence.

Role Owner: Code Builder; ready for Code Reviewer.

Status: Pending Code Reviewer

Next Actions: Run the Phase 3 Code Reviewer loop. Keep Phase 4 untouched until Phase 3 receives PASS, and repair every exact Reviewer finding before resubmission.

Changed Paths:

- `Pakuri/Assets/Scripts/Units/Models/CombatVector2.cs`
- `Pakuri/Assets/Scripts/Units/Models/UnitBaseModel.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/SkillCooldown.cs`
- `Pakuri/Assets/Scripts/Combat/NewCore/`
- `Pakuri/Assets/Scripts/Combat/Actions/NewCore/`
- `Pakuri/Assets/Scripts/Combat/Effects/NewCore/`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/NewCore/`
- `Pakuri/Assets/Scripts/Combat/Skills/Actors/`
- `Pakuri/Assets/Scripts/Core/Tests/Editor/NewCoreCombatLoopTests.cs`
- Unity-generated `.meta` files for all new Phase 3 assets and directories
- `Pakuri/reference/Work/new-core-architecture-blueprint.md`

Evidence:

- Central Tick: `InGameActionManager.Tick` is the sole combat-frame entry and executes Passive-before, all registered cooldowns, automatic monsters in registration order, the selected monster's queued manual request, enemies in registration order, `SkillActorManager`, all registered statuses, and Passive-after in the blueprint order.
- Manual boundary: `PlayerInputController` contains no Unity input API. It accepts Pressed, Held, Released, pointer-over-UI, aim direction, and target point values from a future Phase 5 adapter; non-projectiles accept Pressed only, projectiles retain the latest aim for explicit burst continuation, and only the first registered selected monster is accepted.
- Model authority: `CombatVector2` and `UnitBaseModel.Position` provide an engine-independent current-position authority. `UnitMovementController` is the only Phase 3 movement writer and honors `can_move`, status move-speed bonuses, delta time, and an explicit stop distance.
- Combat results: raw skill value is `base_damage + flat_value + attack_power * attack_power_coefficient + spell_power * spell_power_coefficient`; defense applies `100 / (100 + defense)`; critical chance and resistance are evaluated through the injected deterministic random source; results are rounded; Model shield absorption precedes health damage; healing, shield, status, skill-activation, and defeat events expose confirmed results.
- Targeting: living non-Nexus candidates are selected by side and retained `target_scope`; Nearest, CurrentTarget, HighestHealth, LowestHealth, HighestStacks, Farthest, Random, All, Self, and manual target-point ordering are deterministic with registration-order tie preservation.
- Skill families: `ProjectileExecutor`, `LineAttackExecutor`, `AreaAttackExecutor`, `SingleAttackExecutor`, `BuffExecutor`, `HealExecutor`, `ShieldExecutor`, and `PassiveExecutor` all have actual dispatch callers through `SkillExecutionRuntime`. Selected retained Choice nodes project damage, radius, duration, status-stack, additional-status, and critical modifiers without mutating Definitions.
- Reachable coverage: runtime construction resolves every one of the 88 distinct reachable retained node types to Plan or Effect execution behavior; unsupported reachable node types = 0. Plan execution covers conditions, damage/critical/core/execute/stack/count/burst/consecutive rules, additional/branch/chain damage, cooldown/reload/magazine, status stacks/duration/chance/max stacks, consume/redistribute, targeting/radius/knockback, projectile/pierce/burst/repeat, shield, and proc modifiers. Effect execution groups retained graphs by owner and index and applies their conditions, targets, lifetime/delay, damage, shield, status payload, runtime modifier, visual-handle, status-extension, and recast operations.
- Trigger coverage: `SkillTriggerDispatcher` validates all 59 retained Trigger rows and dispatches CombatStart, OnSkillCast, OnOutgoingDamage, OnMagazineLastProjectileHit, OnKill, OnStatusExpire, OnShieldExpire, and OnShieldAbsorb. It enforces event-skill/runtime/status filters, all required and excluded Choices, count/proc/internal-cooldown gates, delay/repeat scheduling, target resolution, Effect graphs, triggered skills, coefficient damage, cooldown refunds, and reload reductions; unsupported Trigger rows = 0.
- Projectile and repeated fields: burst shots are scheduled at retained intervals, new projectile Actors begin on the next Actor Tick, pierce walks ordered living targets, magazine bonuses and reload/shot-interval modifiers update runtime cooldown authority, damage delays are scheduled, and Area/Line repeat fields execute through `ScheduledSkillActor`.
- Actor lifecycle: no Phase 3 Actor contains `Update`. `SkillActorManager` alone Ticks Actors in registration order, removes completed Actors after iteration, promotes `pendingAdd` after the current iteration so new Actors first Tick next frame, and clears active and pending lists plus Effect handles at combat end.
- Effect lifecycle: `EffectManager` owns engine-independent Effect handles and only creates, updates, removes, or clears position/direction/resource-path projections. Prefab instantiation remains Phase 5.
- Enemy flow: each enemy first resolves support B when a damaged ally exists, then offensive B, then A; it moves when outside CSV range, targets the nearest living Monster, falls back to the Nexus, requests exact CSV `nexus_damage` at the explicit contact boundary, marks contact, and unregisters itself from the verified `StageManager` field list.
- Static scan: 36 Phase 3 runtime C# files and 5,015 lines; zero `UnityEngine`, `MonoBehaviour`, `ScriptableObject`, `GameObject`, scene search, `SendMessage`, reflection, previous `Pakuri.InGame` namespace, `TODO`, or `FIXME` matches; zero `Update()` methods; zero missing `.meta` pairs.
- First full EditMode job `c30948a5aa3f46e2b24a08519cfbefbc`: 37 total, 36 passed, one targeting-test expectation failed because the retained HighestHealth rule compares absolute current health, not health ratio. The implementation already matched the inspected behavior; the test expectation was corrected to compare actual current health.
- Final EditMode job `e3913c482eb84d2a8d71a8fe4760d7f3`: 42 total, 42 passed, 0 failed, 0 skipped, result `Passed`; includes all retained Phase 1 and Phase 2 tests.
- Phase 3 tests cover exact Tick order, Actor next-frame registration and post-iteration removal, scheduled initial delay and repeat count, damage/heal/shield/status authority, cooldown blocking and completion, automatic target selections and manual target point, every active skill family, retained Choice and Effect graphs, all 88 reachable node and 59 Trigger contracts, projectile burst plus ordered pierce, a real CombatStart Trigger, manual aim/request rules, movement/status blocking, and the enemy-to-Nexus request.
- Retained CSV contract: all 42 current byte counts and SHA-256 values match `new-core-phase0-csv-contract-manifest.csv`; mismatches = 0.
- Phase 0 artifact hashes remain exactly `37A9D131EFC61EA20EEA13AF3C3BCA693DB6BE2524B8AFE44D80AA2DF64A0788`, `2E12342D4C45AC1D4A67D68ED20561F584DD60BA56F0AEDB9CAE50229AFDA604`, `6AFD7D0916B6AA14E4A5F881FCCE47EAD0364930D58EB1EF37428DEBFC92F07C`, `D201C258DE6BD5346E0132E3FCE579B875C689977ED3FE24E8AEC92F3B07AD90`, and `832BD377E1CCC468B4FE2D2B197F8603F21B4923D185A193B12267D68C153654`.

Unity Before Log: Error/Exception/Warning entries = 0. Editor was idle, not compiling, not in Play Mode, and the active scene was `NewMainMenu`.

Unity Compile Result: Unity 6000.3.14f1 generated all required `.meta` files and compiled `Pakuri.NewCore.dll`, `Pakuri.NewCore.Runtime.dll`, the unchanged existing runtime, and `Pakuri.NewCore.EditMode.Tests.dll`. The Editor returned to idle with no pending domain reload.

Unity Error/Exception: 0 after the final forced Asset Refresh and compilation gate.

Unity Warning: 0 after the final forced Asset Refresh and compilation gate. The package-owned Performance Testing setup and cleanup Warnings and results-path message emitted after the successful test job were recorded, cleared, and did not contain a new-runtime stack.

Play Mode: Not Run

Play Mode Reason: Phase 3 deliberately exposes engine-independent position, aim, collision-result, and manual-input request boundaries. Exact ordering, delta-time movement math, target selection, damage, all Executor families, Actor next-frame lifecycle, and Effect-handle cleanup are proven by deterministic EditMode tests. Actual mouse polling, Rigidbody/Collider collision, prefab visuals, and frame integration do not exist until Phase 5 connects presentation, so starting Play Mode in Phase 3 would not prove an integrated new-Core scene path.

User Result: Not requested for Phase 3.

History: Code Builder read only the mandatory role files and the English blueprint among Markdown inputs. The first compile exposed that the legacy `EffectManager.cs` path had been displaced while adding the new same-named type; the existing file and GUID were restored byte-for-byte and the new type was isolated under `Combat/Effects/NewCore`. The next forced compile passed. The initial full test run found one incorrect HighestHealth test assumption; inspected existing targeting compares absolute current health, so the implementation was retained and the assertion was corrected. A subsequent coverage audit found 66 reachable specialized node paths and all 59 Trigger rows still missing from execution, so Phase 3 remained In Progress while the Builder implemented Plan and Effect graph execution, Trigger dispatch, burst, pierce, delay, repeat, runtime status modifiers, follow-up damage, resource mutation, and exact contract-audit tests. Final EditMode job `e3913c482eb84d2a8d71a8fe4760d7f3` passed 42 of 42 tests. Unsupported reachable node and Trigger counts are both zero. Phase 3 is pending Code Reviewer; Phase 4 was not started.

## Phase Record — 2026-07-24 01:18 +09:00

Task title: Establish New State Authorities

Goals: Establish one writer for run currencies, field-unit registration, party order, reward-phase prisoners, unit health and shields, status lifecycles, monster learning state, enemy assigned skills, and per-skill cooldown, magazine, reload, and shot-interval state.

Constraints: Only Phase 2 pure runtime state, its deterministic EditMode tests, the Phase 2 plan section, and this newest record were changed. No Actor, combat execution, action Controller, movement, Service, Spawn, scene, prefab, `.asset`, UI, CSV, Phase 0 artifact, BLACKBOARD-family file, or Phase 3 through Phase 6 implementation was added or modified. No Play Mode was used. Existing Scripts were not called, inherited, wrapped, or used as fallbacks.

Role Owner: Code Builder; submitted to Code Reviewer.

Status: PASS — Code Reviewer loop 2

Next Actions: Begin Phase 3. Use only the verified Phase 2 state authorities and retain all Phase 0 and CSV hash baselines.

Changed Paths:

- `Pakuri/Assets/Scripts/Run/Pakuri.NewCore.Runtime.asmdef`
- `Pakuri/Assets/Scripts/Run/RunSessionModel.cs`
- `Pakuri/Assets/Scripts/Run/StageManager.cs`
- `Pakuri/Assets/Scripts/Run/PartyRoster.cs`
- `Pakuri/Assets/Scripts/Run/PrisonerInventory.cs`
- `Pakuri/Assets/Scripts/Units/Models/Pakuri.NewCore.Runtime.asmref`
- `Pakuri/Assets/Scripts/Units/Models/UnitBaseModel.cs`
- `Pakuri/Assets/Scripts/Units/Models/MonsterModel.cs`
- `Pakuri/Assets/Scripts/Units/Models/EnemyModel.cs`
- `Pakuri/Assets/Scripts/Units/Models/NexusModel.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/Pakuri.NewCore.Runtime.asmref`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/SkillBucket.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/MonsterSkillBucket.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/EnemySkillBucket.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/SkillCooldown.cs`
- `Pakuri/Assets/Scripts/Combat/Status/NewCore/Pakuri.NewCore.Runtime.asmref`
- `Pakuri/Assets/Scripts/Combat/Status/NewCore/StatusEffect.cs`
- `Pakuri/Assets/Scripts/Core/Tests/Editor/Pakuri.NewCore.EditMode.Tests.asmdef`
- `Pakuri/Assets/Scripts/Core/Tests/Editor/NewCoreRuntimeStateTests.cs`
- Unity-generated `.meta` files for the four new runtime directories, 17 runtime assets, and the new test file
- `Pakuri/reference/Work/new-core-architecture-blueprint.md`

Evidence:

- Assembly boundary: `Pakuri.NewCore.Runtime` references only `Pakuri.NewCore`, has `noEngineReferences: true`, and uses `.asmref` files to include only the new Phase 2 folders. `StatusEffect` is placed under `Combat/Status/NewCore` so the existing files already present in `Combat/Status` are not pulled into the new assembly.
- Static runtime scan: 13 C# files and 1,076 lines; zero `UnityEngine`, `MonoBehaviour`, `ScriptableObject`, `GameObject`, scene-wide search, `SendMessage`, reflection, `Legacy`, previous-Script namespace marker, `TODO`, or `FIXME` matches.
- Type ownership: `StageManager` owns the active `RunSessionModel`, currency fields, and field-unit list; `RunSessionModel` owns immutable run-location inputs plus its `PartyRoster` and `PrisonerInventory` references; `PartyRoster` owns the ordered one-through-five party list; `PrisonerInventory` creates and owns exact `Prisoner` handles; each unit Model owns its health, shield, survival projection, and applied-status list; each `MonsterModel` creates and owns its `MonsterSkillBucket`; each `EnemyModel` creates and owns its `EnemySkillBucket`; each Bucket owns its learned or assigned Definition references and cooldown objects; each `SkillCooldown` owns one skill's use-state; and each `StatusEffect` owns the duration and stack state for one applying-unit, affected-unit, and immutable `StatusDefinition` tuple.
- Current callers: deterministic Phase 2 tests construct the bootstrap-independent state graph; Models directly construct their owned Buckets; Buckets construct `SkillCooldown`; `UnitBaseModel.ApplyStatus` constructs, refreshes, Ticks, removes, and clears `StatusEffect`; `StageManager`, future Phase 3 Controllers, and future Phase 4 Services use only the exposed query and command boundaries. Production creation by `GameBootstrap` and `SpawnManager` remains Phase 4 or Phase 5 scope and was not preimplemented.
- Deletion conditions: Run authorities become unnecessary only if the active-run flow is removed; Models and their Buckets become unnecessary only if their corresponding unit runtime is removed; `SkillCooldown` becomes unnecessary only if skill-use cadence is removed; `StatusEffect` becomes unnecessary only if applied status runtime is removed; the assembly references become unnecessary only if these files are later consolidated under one isolated new-runtime assembly without including previous Scripts.
- `PartyRoster`: slot 0 is established only by the constructor; manifested additions preserve registration order; maximum slots are five; duplicate `monster_id` additions and full-roster additions fail without mutation.
- `PrisonerInventory`: duplicate enemy types remain distinct handles; consumption removes the exact held handle; repeat consumption fails; replacing rewards validates the entire replacement before clearing; both reward replacement and explicit clear prevent previous prisoners from carrying forward.
- Unit state: health and shield writes are private to `UnitBaseModel`; survival is derived from current health rather than duplicated; shield absorption precedes health damage; dead units cannot be healed or shielded; round reset restores health, clears shields and statuses, resets skill runtime, enables AutoAttack, and enables AutoSkill only for a non-selected monster.
- Status lifecycle: immutable `StatusDefinition`, applying-unit, and affected-unit references are retained; same-source reapplication refreshes duration and adds stacks; positive `max_stacks` is enforced; permanent statuses have no countdown; expired statuses are removed by the owning Model; explicit removal and round clear are verified.
- Monster learning: the immutable slot-A skill is required at construction; no more than two additional active skills and five passives can be learned; duplicate and foreign-monster skills fail; active enhancements are capped at three per skill; a master requires three enhancements and is capped at one; passive enhancement is capped at one per learned passive; duplicate Choices fail.
- Reviewer loop 1: FIX REQUIRED. `MonsterSkillBucket` allowed learning and selecting a PassiveBase without checking a non-empty `target_skill_id` prerequisite, and `StatusEffect.Refresh` assigned refreshed stacks before validating the requested duration.
- PassiveBase fix: each `MonsterModel` now provides its immutable retained PassiveBase Choices when creating its Bucket. When a PassiveBase has a non-empty `target_skill_id`, `CanLearnPassive`, `TryLearnPassive`, `CanSelectChoice`, and `TrySelectChoice` all require that active skill to be learned. The retained `sein-i-base-shot-interval` Choice is verified to require `sein-d`; both learning and selection reject before learning `sein-d`, accept after learning it, and reject duplicate selection.
- Atomic refresh fix: `StatusEffect.Refresh` resolves and validates both the new stack count and duration in locals before assigning either field. A regression test proves that an invalid negative refresh duration throws while preserving the original stacks, remaining duration, and owned status list.
- Enemy skills: exactly two Definition-backed active slots and one passive are validated against `EnemyDefinition`; duplicate A/B skill assignments remain two ordered slots while sharing one `SkillCooldown` authority for the same `skill_id`.
- Cooldown lifecycle: non-magazine cooldown, magazine count, shot interval, reload block, refill, invalid negative delta, and next-round reset are deterministic. A technical `0.00001f` completion tolerance prevents accumulated `float` frame deltas from leaving a nominally completed timer infinitesimally positive; it does not change any CSV tuning value.
- Initial EditMode job `90d2b5a8b7e14715a2d4e582ace9d68b`: total 25, passed 24, failed 1. The exact failure was the `6.49f + 0.01f` timer boundary. The completion tolerance fixed that numeric edge without changing a Definition or CSV.
- Final EditMode job `a522f9f8baaf414e81da2bba99702786`: total 25, passed 25, failed 0, skipped 0, result `Passed`, including all 13 Phase 1 tests and 12 new Phase 2 tests.
- Re-review EditMode job `9bd4afbde4f24aaaa8d93c9569e75ad8`: total 27, passed 27, failed 0, skipped 0, result `Passed`, including all 13 Phase 1 tests and 14 Phase 2 tests after both Reviewer loop-1 fixes.
- Retained CSV contract: all 42 current byte counts and SHA-256 values match `new-core-phase0-csv-contract-manifest.csv`; mismatches = 0.
- Phase 0 artifact hashes remain exactly `37A9D131EFC61EA20EEA13AF3C3BCA693DB6BE2524B8AFE44D80AA2DF64A0788`, `2E12342D4C45AC1D4A67D68ED20561F584DD60BA56F0AEDB9CAE50229AFDA604`, `6AFD7D0916B6AA14E4A5F881FCCE47EAD0364930D58EB1EF37428DEBFC92F07C`, `D201C258DE6BD5346E0132E3FCE579B875C689977ED3FE24E8AEC92F3B07AD90`, and `832BD377E1CCC468B4FE2D2B197F8603F21B4923D185A193B12267D68C153654` for the CSV contract, Inspector snapshot, generator, retained-resource manifest, and Script-reference manifest respectively.
- Unity generated every required `.meta`; missing asset-to-meta pairs = 0.
- The Test Runner emitted its package-owned Performance Testing setup and cleanup Warnings and a results-path message classified as an Exception after the successful test job. They originate under Unity package/editor paths and contain no new-runtime stack. The Console was cleared, then the final forced Asset Refresh and requested compilation returned to idle with zero Errors, Exceptions, and Warnings.

Unity Before Log: Error/Exception/Warning entries = 0. Editor was idle, not compiling, not in Play Mode, and the active scene was `NewMainMenu`.

Unity Compile Result: Unity 6000.3.14f1 imported the new runtime assembly, assembly references, C# files, tests, and generated `.meta` files. `Pakuri.NewCore.dll`, `Pakuri.NewCore.Runtime.dll`, and `Pakuri.NewCore.EditMode.Tests.dll` compiled, and the Editor returned to idle with no pending domain reload.

Unity Error/Exception: 0 after the final forced Asset Refresh and compilation gate.

Unity Warning: 0 after the final forced Asset Refresh and compilation gate. The two package-owned Test Runner warnings were recorded and cleared before this gate.

Play Mode: Not Run

Play Mode Reason: Phase 2 changes only pure Models and state authorities. Deterministic EditMode transitions, assembly isolation, retained-hash checks, compilation, editor state, and Console evidence prove the requested scope without scene lifecycle or gameplay input.

User Result: Not requested for Phase 2.

History: Code Builder read only the mandatory role files and the English blueprint among Markdown inputs. The initial implementation compiled with zero Console errors. The first full test job exposed one floating-point timer completion edge; the timer reduction was made tolerant only at the numeric completion boundary. The second full job passed 25 of 25 tests. Code Reviewer loop 1 then returned FIX REQUIRED for missing PassiveBase prerequisite enforcement and non-atomic StatusEffect refresh. Code Builder added immutable PassiveBase rule input to each MonsterSkillBucket, enforced the retained `sein-i-base-shot-interval` to `sein-d` prerequisite in both learning and selection paths, made refresh assignment atomic, and added two regression tests. The re-review job passed 27 of 27 tests. Reviewer loop 2 independently ran job `3248fa6ffdf54249910223fc57641b8f`, passed all 27 tests, verified final Console zero and retained hashes, and returned PASS with no user-only verification gap. Phase 3 was not started.

## Phase Record — 2026-07-24 00:37 +09:00

Task title: New Core Data Foundation

Goals: Parse every retained CSV into immutable Definitions while preserving exact CSV column names, validate startup invariants explicitly, expose a read-only `GameDefinitionCatalog`, and provide a concrete data-only `GameBootstrap` initialization boundary.

Constraints: Only Phase 1 code under `Pakuri/Assets/Scripts/Core` and this Phase record were changed. Retained CSVs, runtime Models, Combat, UI, scenes, prefabs, `.asset` files, Phase 0 manifests, BLACKBOARD-family files, and Phase 2 through Phase 6 implementation were not changed. No legacy type is called, inherited, wrapped, or used as a fallback.

Role Owner: Code Builder; submitted to Code Reviewer.

Status: PASS — Code Reviewer loop 2

Next Actions: Begin Phase 2. Preserve the immutable data foundation, retained CSV hashes, and Phase 0 before-state manifests.

Changed Paths:

- `Pakuri/Assets/Scripts/Core.meta`
- `Pakuri/Assets/Scripts/Core/Pakuri.NewCore.asmdef`
- `Pakuri/Assets/Scripts/Core/Bootstrap/GameBootstrap.cs`
- `Pakuri/Assets/Scripts/Core/Parsing/CsvParser.cs`
- `Pakuri/Assets/Scripts/Core/Catalog/GameDefinitionCatalog.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/CsvDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/SkillDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/ProjectileDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/LineAttackDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/AreaAttackDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/SingleAttackDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/BuffDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/HealDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/ShieldDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/PassiveDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Skills/SkillTriggerDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Choices/SkillChoiceDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Choices/MonsterModifierSkillChoiceDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Choices/ChoiceNodeDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Choices/NodeTypeDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Choices/NodeParamDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Units/UnitDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Units/MonsterDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Units/EnemyDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Units/CatalogMonsterDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Stage/StageDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Stage/StageDayDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Stage/StageEncounterDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Stage/StageRewardDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Definitions/Status/StatusDefinition.cs`
- `Pakuri/Assets/Scripts/Core/Tests/Editor/Pakuri.NewCore.EditMode.Tests.asmdef`
- `Pakuri/Assets/Scripts/Core/Tests/Editor/NewCoreDataFoundationTests.cs`
- `Pakuri/Assets/Scripts/Core/**/*.meta` generated by Unity for 42 new files and directories
- `Pakuri/reference/Work/new-core-architecture-blueprint.md`

Evidence:

- Type ownership: `GameBootstrap` owns the initialized Catalog and is currently invoked by deterministic Phase 1 tests as the concrete data-bootstrap boundary. It is not yet called by a production scene, serialized component, or runtime startup path; that wiring remains Phase 5 scope. `CsvParser` is owned and called by Bootstrap and owns only the startup parse operation; `GameDefinitionCatalog` owns all immutable Definitions and is read by later Core layers; concrete Definitions are created by Parser and owned by Catalog; `CsvDefinitionData` is an internal construction payload owned and consumed by Parser during Definition creation. These types become unnecessary only if the retained CSV startup boundary itself is removed or replaced.
- Retained inputs: 42 required CSV paths are explicit and deterministic; 39 authoring files parse their schema/type row; three `stage_flow` files use exact typed contracts because the inspected files contain no schema row; the empty enemy single-attack Trigger CSV is counted and validated.
- Successful Catalog: 1,836 immutable Definition rows, 82 unique skills, five monsters, and 16 enemies. Quoted commas and escaped quotes remain intact.
- Explicit validation: RFC-style quoted fields and escaped quotes, row widths, duplicate headers, schema types, finite invariant-culture floats, integers, booleans, evidence-defined enum domains, unique IDs and composite keys, node parameter types and allowed values, and required cross-file references.
- Reference distinctions proven by retained data: semicolon-delimited Trigger references are validated individually; synthetic `triggered_skill_id` and `condition_status_source_skill_id` values are not misclassified as base-skill references; the observed `baseSkill@effect1` node reference validates its base skill and exact selector.
- Every property declared through `RequiredString(...)` is validated eagerly by the matching Definition constructor through `ValidateRequired(...)`. `GameBootstrap` therefore fails during initialization rather than waiting for a later property read.
- EditMode test job `e47a8c682bfd4609979c2060f1c0b8b7`: total 13, passed 13, failed 0, skipped 0, result `Passed`.
- Test coverage: all retained CSV success path and Catalog immutability; quoted and escaped fields; a quoted field containing actual CRLF; blank required Monster `display_name` and `primary_attribute`; duplicate ID; invalid int, float, bool, and enum; missing cross-file reference; missing retained CSV; unterminated quote.
- Retained CSV contract check: all 42 current byte counts and SHA-256 values match `new-core-phase0-csv-contract-manifest.csv`; mismatches = 0.
- Static new-code scan: 29 C# files and two asmdefs; zero `TODO`, `FIXME`, `SendMessage`, reflection, scene-wide search, or Legacy markers; runtime assembly references only `System` and `Pakuri.NewCore`.
- No Runtime Model, Combat, UI, scene, prefab, or `.asset` implementation was added or modified.
- Unity Test Runner emitted its package-owned Performance Testing setup/cleanup warnings and a `Saving results to ... TestResults.xml` message classified as an Exception after test execution. These entries originate under Unity package/editor paths, not the new Core. The Console was cleared and the final forced Asset Refresh and compilation gate returned zero Errors, Exceptions, and Warnings.

Unity Before Log: Error/Exception/Warning entries = 0. Editor was idle, not compiling, not in Play Mode, and ready for tools.

Unity Compile Result: Unity 6000.3.14f1 imported the new files, generated their `.meta` files, built `Pakuri.NewCore.dll` and `Pakuri.NewCore.EditMode.Tests.dll`, and returned to idle with no pending domain reload.

Unity Error/Exception: 0 after the final forced Asset Refresh and compilation gate.

Unity Warning: 0 after the final forced Asset Refresh and compilation gate. The two package-owned Test Runner warnings were recorded and cleared before the final gate.

Play Mode: Not Run

Play Mode Reason: Phase 1 contains only deterministic CSV parsing, immutable Definitions and Catalog state, and a data-only Bootstrap boundary. Compilation, static contract checks, and EditMode tests prove the requested behavior without gameplay execution.

User Result: Not requested for Phase 1.

History: The first script-only refresh did not import new assets, so a full Asset Refresh was used. The first discovered test run exposed semicolon-delimited Trigger references. Later runs exposed synthetic source identifiers and the `baseSkill@effect1` node selector. Validation was narrowed to the inspected contracts rather than guessing. The initial final run passed all 10 tests and was submitted to Code Reviewer. Reviewer loop 1 returned FIX REQUIRED because required string fields were checked only when their getters were read, quoted multiline parsing lacked an actual-newline test, and the top status and `GameBootstrap` production-caller wording were inaccurate. Code Builder added eager validation for every declared required string, two blank Monster required-field tests, an actual-CRLF quoted-field test, and corrected the records. Re-review job `e47a8c682bfd4609979c2060f1c0b8b7` passed all 13 tests. Reviewer loop 2 independently ran job `cdcb6938b53d409685c1dcf16f2be5dd`, passed all 13 tests, verified final Console zero, and returned PASS with no user-only verification gap. Phase 2 was not started.

## Phase Record — 2026-07-23 23:56 +09:00

Task title: Existing Scripts Complete Replacement Baseline

Goals: Freeze the retained CSV contract, existing Script GUID migration inventory, compatibility rules, and pre-change Unity baseline.

Constraints: No game code, retained CSV, scene, prefab, `.asset`, or BLACKBOARD-family file was changed. Existing Scripts were inspected only as read-only evidence. Play Mode was not used.

Role Owner: Code Builder; submitted to Code Reviewer.

Status: PASS — Code Reviewer loop 3

Next Actions: Begin Phase 1. Keep all four Phase 0 manifests as immutable before-state evidence.

Changed Paths:

- `Pakuri/reference/Work/new-core-architecture-blueprint.md`
- `Pakuri/reference/Work/new-core-phase0-manifest-generator.ps1`
- `Pakuri/reference/Work/new-core-phase0-csv-contract-manifest.csv`
- `Pakuri/reference/Work/new-core-phase0-script-reference-manifest.csv`
- `Pakuri/reference/Work/new-core-phase0-retained-resource-manifest.csv`
- `Pakuri/reference/Work/new-core-phase0-inspector-snapshot.csv`

Evidence:

- Blueprint translation-only snapshot before Phase records: strict UTF-8 PASS; 2,063 split lines; 107 headings; 72 code-fence lines. Current-file QA: balanced fences; zero Hangul; zero trailing whitespace; all Phase headings retained.
- Retained CSV contract: 42 rows; current path, byte count, and SHA-256 mismatches = 0.
- Existing Script references: 56 rows; unique Script GUIDs = 21; unique assets = 40; required-column omissions = 0.
- Serialized-file scan: 240 files across `.unity`, `.prefab`, `.asset`, `.controller`, `.overrideController`, `.anim`, `.playable`, `.mat`, and `.scenetemplate`; Legacy-path reference rows = 16; non-Legacy-path reference rows = 40.
- Retained resources: 781 reference rows; 593 unique project assets; 24 non-Legacy migration roots; 86 concrete CSV path rows; zero missing retained paths.
- Retained-resource kinds include 514 PNG edges, 89 AnimatorController edges, 75 animation-clip edges, 46 prefab edges, 42 CSV TextAsset edges, 6 data-asset edges, 4 shader edges, 3 font edges, and 2 scene roots.
- Inspector baseline: 56 exact serialized component payloads; Base64 decode failures = 0; SHA-256 mismatches = 0.
- Existing Scripts baseline: 69 `.cs` files; 38,083 lines.
- Unity project: `Pakuri`, Unity 6000.3.14f1, StandaloneWindows64.

Unity Before Log: Error/Exception/Warning entries = 0.

Unity Compile Result: Forced Asset Refresh and requested script compilation completed. Editor state returned to idle, not compiling, no domain reload pending, and ready for tools.

Unity Error/Exception: 0.

Unity Warning: 0.

Play Mode: Not Run

Play Mode Reason: Phase 0 changes only documentation and generated baseline manifests outside `Assets`; filesystem, serialization, compilation, editor-state, and Console checks are sufficient.

User Result: Not requested for Phase 0.

History: Phase 0 completed by Code Builder and submitted to Code Reviewer. Reviewer loop 1 returned FIX REQUIRED for retained-resource coverage, QA-count scope, and serialized-file scope. Code Builder applied all three fixes. Reviewer loop 2 verified those fixes and found only an unsupported material-coverage claim; Code Builder removed it. Reviewer loop 3 returned PASS with no remaining blocker or user-only verification gap. No Phase 1 work has started.

Record format:

```text
## Phase Record — YYYY-MM-DD HH:mm

Task title:
Goals:
Constraints:
Role Owner:
Status:
Next Actions:
Changed Paths:
Evidence:
Unity Before Log:
Unity Compile Result:
Unity Error/Exception:
Unity Warning:
Play Mode:
Play Mode Reason:
User Result:
History:
```

Use exactly one of the following values for `Play Mode`.

```text
Not Run
Requested From User
Completed By User
Failed By User Evidence
```

If Play Mode was not run, record in `Play Mode Reason` why static checks were sufficient. If it was requested, also record the Reason, Scene, Setup, Actions, Expected, Failure, and LogCheck fields from section 21.3.
