# 2026-06-17 Normalized Skill Authoring Row Table Handoff

Role Owner: Designer

Status: Code-evidence-based design handoff

Source feedback:

- `Pakuri/reference/Report/2026-05-29-skill-runtime-refactor-feedback-handoff.md`
- `Pakuri/reference/Report/2026-05-29-skill-runtime-refactor-feedback-handoff.html`

## Goal

Design the next data-authoring refactor after the `UnitSkillController` / `SkillExecutionPlan` runtime slice.

The purpose is not to delete the current CSVs immediately. The purpose is to stop adding a new `monster_skills.csv` or `monster_skill_choices.csv` column whenever a new exception skill appears.

New exception behavior should be added as a new runtime node handler plus node rows, not as another wide CSV column plus another flat definition/snapshot field.

## Inspected Evidence

- The source feedback says the problem is not "CSV versus code"; the problem is that new skill behavior becomes wide CSV fields plus wide snapshot fields plus central executor logic.
- The source feedback recommends condition, targeting, damage modifier, hit action, kill action, projectile behavior, status/buff, and visual handlers.
- The source feedback says physical CSV splitting should happen only after runtime can consume behavior nodes.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` currently has 72 columns and 51 imported data rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` currently has 114 columns and 253 imported data rows.
- `monster_skills.csv:1` contains behavior columns such as `execute_health_ratio_threshold`, `require_execute_threshold_to_cast`, `execute_damage_multiplier`, `kill_cooldown_refund_ratio`, `boss_damage_multiplier`, `deployment_required_target_status_id`, target-stack damage fields, and consume-target-status fields.
- `monster_skill_choices.csv:1` contains behavior columns such as `execute_health_ratio_bonus`, `execute_crit_chance_bonus`, `boss_damage_multiplier`, `kill_cooldown_refund_ratio_bonus`, `kill_resets_cooldown`, `kill_resets_cooldown_requires_execute`, `on_hit_additional_damage_*`, `core_*`, `hit_count_cooldown_refund_*`, repeat fields, target-stack damage fields, consume override fields, conditional crit fields, and redistribute-on-kill fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:417` to `:421` parses the current execute/boss/kill base skill columns directly from `monster_skills.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:505` to `:533` parses execute/boss/kill choice columns directly from `monster_skill_choices.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:418` to `:490` shows `SkillDefinition` still owns many wide behavior fields beside metadata and basic tuning.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:264` to `:350` shows `SkillChoiceDefinition` still owns many wide behavior fields.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:204` to `:256` maps skill CSV rows into `SkillDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:434` to `:583` maps choice CSV rows into `SkillChoiceDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs:6` to `:24` already defines authoring source and node kind enums.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs:26` to `:101` already defines `SkillExecutionPlanNode`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs:213` to `:226` already has a compiler overload that can accept normalized node rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:938` to `:973` currently bridges old flat fields into operation records and a plan.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` already exists as a row-like table with 70 columns and 132 rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` already exists as a row-like table with 47 columns and 57 rows.

## Current Problem

`monster_skill_effects.csv` and `monster_skill_triger.csv` already show a partial row-table direction, but the two main authoring files are still wide behavior surfaces:

- `monster_skills.csv` mixes identity, UI text, basic numeric tuning, status payloads, targeting, deployment filters, execute rules, boss damage, kill cooldown, target-stack damage, and consume-status behavior.
- `monster_skill_choices.csv` mixes choice identity/UI text with damage modifiers, projectile modifiers, status modifiers, execute/boss/kill modifiers, hit actions, core-hitbox rules, repeat rules, conditional crit, and redistribution behavior.

If the project continues this way, every new exception skill still requires:

1. a new CSV column;
2. a parser field;
3. a definition field;
4. a snapshot field;
5. a mapper/build assignment;
6. one more executor or utility branch.

That is the exact pattern the source handoff says to stop.

## Design Decision

Do not split the current files by copying every current column into many new wide tables.

Instead, use a two-layer normalized authoring model:

1. Stable base tables keep skill and choice identity, UI text, basic selection metadata, and compatibility lifecycle fields.
2. A node graph expresses behavior as rows. Node payload values are stored as key/value parameter rows so new handler parameters do not require new CSV headers.

This means adding a future exception skill should normally add:

- one handler id in runtime code, only if the behavior is genuinely new;
- one or more `monster_skill_nodes.csv` rows;
- several `monster_skill_node_params.csv` rows;
- no new CSV column.

## Proposed New CSV Tables

### 1. `monster_skill_base.csv`

Purpose:

- Replacement target for stable identity and base skill metadata now inside `monster_skills.csv`.
- Keeps base skill rows readable and small.
- Does not own exception behavior.

Suggested columns:

```csv
skill_id,monster_id,skill_kind,slot,display_name,runtime_kind,implementation_state,is_default_learned,is_available_without_active_requirement,required_active_slot,skill_icon_path,description_text,summary
```

Notes:

- `runtime_kind` remains because executor compatibility still needs the broad execution family.
- `implementation_state` remains because the current data workflow uses support-state tracking.
- Basic user-facing description stays here.
- Do not add new behavior columns here after migration.

### 2. `monster_skill_choice_base.csv`

Purpose:

- Replacement target for stable choice identity and UI metadata now inside `monster_skill_choices.csv`.
- Keeps choice rows small.
- Does not own modifier/action payloads.

Suggested columns:

```csv
choice_id,monster_id,skill_id,target_skill_id,runtime_target_skill_ids,choice_group,sort_order,title,description_text,skill_icon_path,skill_effect_prefab_path,runtime_support_state,runtime_support_notes
```

Notes:

- `target_skill_id` and `runtime_target_skill_ids` stay here because they define where choice-authored nodes attach.
- `skill_effect_prefab_path` may later move to visual nodes, but keep it here during the first migration to avoid asset-catalog churn.

### 3. `monster_skill_nodes.csv`

Purpose:

- Main normalized behavior node table.
- One row means "attach this behavior node to this skill or choice".

Suggested columns:

```csv
node_id,owner_kind,owner_id,target_skill_id,node_kind,handler_id,sort_order,enabled_by_default,requires_active_choice_id,excludes_active_choice_id,requires_passive_skill_id,excludes_passive_skill_id,runtime_support_state,runtime_support_notes
```

Column meaning:

- `node_id`: stable id for params and validation.
- `owner_kind`: `Skill`, `Choice`, `Passive`, `Effect`, or `Trigger`.
- `owner_id`: skill id, choice id, passive id, effect id, or trigger id.
- `target_skill_id`: optional. Used when a choice/passive node modifies another skill.
- `node_kind`: runtime category, aligned with `SkillExecutionPlanNodeKind`: `CastCondition`, `Action`, `DamageModifier`, `CritModifier`, `OnHitAction`, `OnKillAction`, `OnExpireAction`, `Trigger`, `Visual`.
- `handler_id`: concrete runtime handler key, such as `TargetHealthRatioCondition`, `BossDamageMultiplier`, `CooldownResetOnKill`, `SpawnProjectile`, or `ApplyStatus`.
- `sort_order`: deterministic node order.
- `requires_*` / `excludes_*`: compatibility gates for choice/passive activation.

### 4. `monster_skill_node_params.csv`

Purpose:

- Payload values for node rows.
- Prevents new CSV columns for every new handler argument.

Suggested columns:

```csv
node_id,param_key,value_type,value
```

Column meaning:

- `node_id`: references `monster_skill_nodes.node_id`.
- `param_key`: handler-defined key, such as `threshold`, `multiplier`, `ratio`, `status_id`, `target_side`, `attribute`, `radius`, `repeat_count`.
- `value_type`: `string`, `int`, `float`, `bool`, `enum`, `asset_path`, `skill_id`, `status_id`, or `choice_id`.
- `value`: raw authored value.

Validation rule:

- Runtime code owns a handler schema registry.
- Each `handler_id` declares required and optional `param_key` values.
- CSV validation fails if a node has unknown params, missing required params, invalid enum values, invalid asset paths, or invalid references.

### 5. Optional later: family-specific readable views

After the generic node table is stable, the project may add narrow convenience tables for high-volume node families:

- `monster_skill_condition_nodes.csv`
- `monster_skill_action_nodes.csv`
- `monster_skill_modifier_nodes.csv`
- `monster_skill_visual_nodes.csv`

But this should not happen first. Starting with family-specific wide tables risks recreating the same column-growth problem in multiple files.

## Relationship To Existing Row Tables

Do not delete or rewrite these immediately:

- `monster_skill_effects.csv`
- `monster_skill_triger.csv`

They already are separate row-like tables and already have runtime parser/build/validation support.

First migration should make the new node compiler able to consume:

```text
legacy monster_skills / monster_skill_choices wide fields
  + existing monster_skill_effects / monster_skill_triger rows
  + new monster_skill_nodes / monster_skill_node_params rows
  -> SkillExecutionPlan
```

After that is stable, `monster_skill_effects.csv` and `monster_skill_triger.csv` can be either:

- kept as specialized authoring tables compiled into plan nodes; or
- migrated into the generic node tables in a later data cleanup.

Do not combine that decision with the first normalized authoring implementation.

## Mapping Current Wide Fields To Nodes

### Base skill fields from `monster_skills.csv`

| Current column | Proposed node |
|---|---|
| `require_execute_threshold_to_cast` + `execute_health_ratio_threshold` | `node_kind=CastCondition`, `handler_id=TargetHealthRatioCondition`, params `threshold`, `reject_if_missing_target` |
| `execute_damage_multiplier` | `node_kind=DamageModifier`, `handler_id=ExecuteDamageMultiplier`, params `threshold_source=TargetHealthRatioCondition`, `multiplier` |
| `boss_damage_multiplier` | `node_kind=DamageModifier`, `handler_id=TargetPredicateDamageMultiplier`, params `predicate=is_boss`, `multiplier` |
| `kill_cooldown_refund_ratio` | `node_kind=OnKillAction`, `handler_id=CooldownRefund`, params `ratio` |
| `damage_delay_seconds` | `node_kind=Action`, `handler_id=DelayedDamage`, params `delay_seconds` |
| `deployment_required_target_status_id` / `deployment_required_target_status_min_stacks` | `node_kind=CastCondition` or `ActionFilter`, `handler_id=RequiredTargetStatus`, params `status_id`, `min_stacks` |
| target-stack damage columns | `node_kind=DamageModifier` or `Action`, `handler_id=TargetStatusStackDamage`, params `status_id`, `max_stacks`, `base_damage`, coefficients |
| consume-target-status columns | `node_kind=OnHitAction`, `handler_id=ConsumeTargetStatus`, params `status_id`, `ratio`, `stacks` |

### Choice fields from `monster_skill_choices.csv`

| Current column family | Proposed node |
|---|---|
| `damage_multiplier`, `base_damage_bonus`, `cooldown_multiplier` | `DamageModifier` / `CooldownModifier` nodes on `owner_kind=Choice` |
| projectile branch columns | `ProjectileModifier`, `handler_id=BranchDamage` |
| `execute_health_ratio_bonus` | `CastCondition` modifier node, `handler_id=TargetHealthRatioThresholdBonus` |
| `execute_crit_chance_bonus` | `CritModifier`, `handler_id=ExecuteCritChanceBonus` |
| `boss_damage_multiplier` | `DamageModifier`, `handler_id=TargetPredicateDamageMultiplier`, `predicate=is_boss` |
| `kill_cooldown_refund_ratio_bonus` | `OnKillAction`, `handler_id=CooldownRefundBonus` |
| `kill_resets_cooldown` / `kill_resets_cooldown_requires_execute` | `OnKillAction`, `handler_id=CooldownReset`, params `requires_execute` |
| `on_hit_additional_damage_*` | `OnHitAction`, `handler_id=AdditionalDamage` |
| `on_hit_chain_*` | `OnHitAction`, `handler_id=EveryNthHitChainDamage` |
| `core_hitbox_name`, `core_damage_multiplier`, `core_on_hit_*` | `DamageModifier` / `OnHitAction`, params `hitbox_name`, `multiplier` |
| `hit_count_cooldown_refund_*` | `OnHitAction`, `handler_id=HitCountCooldownRefund` |
| `repeat_count_per_target`, `repeat_interval_seconds`, `repeat_damage_multiplier` | `Action`, `handler_id=RepeatPerTarget` |
| conditional crit columns | `CritModifier`, `handler_id=TargetStatusCritBonus` |
| redistribute-on-kill columns | `OnKillAction`, `handler_id=RedistributeConsumedStatus` |

## Example Node Rows

### Base execute condition currently in `monster_skills.csv`

`monster_skill_nodes.csv`

```csv
node_id,owner_kind,owner_id,target_skill_id,node_kind,handler_id,sort_order,enabled_by_default,requires_active_choice_id,excludes_active_choice_id,requires_passive_skill_id,excludes_passive_skill_id,runtime_support_state,runtime_support_notes
rin-d-execute-condition,Skill,rin-d,,CastCondition,TargetHealthRatioCondition,100,true,,,,,RuntimeImplemented,
```

`monster_skill_node_params.csv`

```csv
node_id,param_key,value_type,value
rin-d-execute-condition,threshold,float,0.3
rin-d-execute-condition,reject_if_missing_target,bool,true
```

### Choice kill cooldown reset currently in `monster_skill_choices.csv`

`monster_skill_nodes.csv`

```csv
node_id,owner_kind,owner_id,target_skill_id,node_kind,handler_id,sort_order,enabled_by_default,requires_active_choice_id,excludes_active_choice_id,requires_passive_skill_id,excludes_passive_skill_id,runtime_support_state,runtime_support_notes
rin-d-master-kill-reset,Choice,rin-d-master-2,rin-d,OnKillAction,CooldownReset,200,true,,,,,RuntimeImplemented,
```

`monster_skill_node_params.csv`

```csv
node_id,param_key,value_type,value
rin-d-master-kill-reset,requires_execute,bool,true
```

## Runtime Data Shape To Add

Add data classes close to the CSV runtime model and plan compiler:

```text
SkillNodeDefinition
  NodeId
  OwnerKind
  OwnerId
  TargetSkillId
  NodeKind
  HandlerId
  SortOrder
  EnabledByDefault
  Gate fields
  RuntimeSupportState
  RuntimeSupportNotes
  Params[]

SkillNodeParamDefinition
  NodeId
  ParamKey
  ValueType
  Value
```

Then compile these definitions into `SkillExecutionPlanNode` rows.

Do not store handler-specific fields directly on `SkillDefinition` or `SkillChoiceDefinition` for new behavior after this migration starts.

## Required Compiler Direction

The compiler should merge three streams:

```text
Legacy wide fields
  -> compatibility operation bridges

Normalized node rows
  -> SkillExecutionPlanNode[]

Existing effect/trigger rows
  -> either current runtime definitions or plan nodes through a later adapter
```

Initial implementation can keep existing flat fields active and add normalized rows beside them. When both legacy and normalized rows express the same behavior for the same owner, validation should fail or the migration tool should mark one side disabled. Silent double-application is not allowed.

## Work Phases For Code Builder

### Phase A: Schema design file and parser skeleton

Tasks:

- Add schema constants for `monster_skill_base.csv`, `monster_skill_choice_base.csv`, `monster_skill_nodes.csv`, and `monster_skill_node_params.csv`.
- Add data model classes for node rows and param rows.
- Add parser support for optional new files. Missing files should be allowed during the first compatibility phase.
- Add validation for duplicate `node_id`, missing owner, missing handler, missing params, unknown params, invalid references, and invalid `value_type`.

Acceptance:

- Current CSV files still validate when the new files are absent.
- Empty new node files validate.
- No current skill behavior changes.

### Phase B: Node compiler integration

Tasks:

- Add a `SkillNodeHandlerSchemaRegistry`.
- Map `SkillNodeDefinition` + params into `SkillExecutionPlanNode`.
- Feed normalized nodes into `SkillExecutionPlanCompiler.Compile(source, snapshot, normalizedRows)`.
- Keep old wide fields active through `SkillExecutionSnapshot` bridges.

Acceptance:

- A test normalized node can appear in `SkillExecutionPlan.Nodes`.
- Existing execute/boss/kill behavior remains unchanged when no normalized rows exist.

### Phase C: Migrate first sample behavior

Tasks:

- Choose one small current behavior sample: execute threshold + execute multiplier + boss multiplier + kill cooldown behavior.
- Author equivalent node rows for that sample in the new files.
- Add a guard so the same behavior is not applied from both legacy wide fields and normalized rows.
- Keep old source columns present and readable.

Acceptance:

- Current CSV import still succeeds.
- The sample skill produces equivalent plan operations from node rows.
- The legacy wide columns are not deleted.

### Phase D: Migrate choice behavior families

Tasks:

- Convert representative `monster_skill_choices.csv` behavior families into nodes:
  - damage/cooldown/radius modifiers;
  - execute/boss/kill actions;
  - on-hit additional damage;
  - repeat per target;
  - conditional crit;
  - redistribute on kill.
- Keep choice base metadata in `monster_skill_choice_base.csv`.

Acceptance:

- New exception choice behavior can be added with node rows and params only.
- No new behavior column is added to `monster_skill_choices.csv`.

### Phase E: Freeze new wide behavior columns

Tasks:

- Add a repository rule or board rule: new exception skill behavior must use nodes unless Code Builder/Designer explicitly approves a shared wide field.
- Mark old wide behavior columns as compatibility/deprecated in documentation.
- Keep old columns until enough migrated rows are proven through Play Mode.

Acceptance:

- Future handoffs route new behavior to nodes by default.
- Old CSV rows still load.
- New row-set authoring coexists with old wide-column authoring.

Phase E active rule:

- The DATA board and Skill Builder exception guide now define normalized nodes as the default path for new exception skill behavior.
- New behavior should be added through `monster_skill_nodes.csv` plus `monster_skill_node_params.csv`; a new handler schema/runtime handler is added only when behavior is genuinely new.
- New behavior columns in `monster_skills.csv` or `monster_skill_choices.csv` require explicit Designer or Code Builder approval recorded in the active handoff or DATA board task.
- Existing behavior columns in `monster_skills.csv` and `monster_skill_choices.csv` are compatibility/deprecated inputs. They stay readable for current rows and migrations, but they are not the default authoring surface for new exception behavior.
- `monster_skill_effects.csv` and `monster_skill_triger.csv` remain valid specialized row tables until a later migration explicitly adapts or replaces them.

## Compatibility Rules

- Do not delete `monster_skills.csv` or `monster_skill_choices.csv` in the first implementation.
- Do not delete old wide columns in the first implementation.
- Do not change `SkillDefinition` or `SkillChoiceDefinition` public field names unless all current runtime call sites are migrated.
- Do not break `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()`.
- Keep `RuntimeImplemented`, `DataOnlyUnsupported`, and notes fields available during migration.
- Existing `monster_skill_effects.csv` and `monster_skill_triger.csv` stay valid.
- Generated runtime catalog assets must remain readable by current scenes.

## Validation Requirements

Code Builder should add validation that proves:

- `node_id` is unique.
- Every node owner exists.
- Every `target_skill_id` exists when provided.
- Every `handler_id` is registered in the handler schema registry.
- Every required param exists.
- No unknown param exists unless the handler explicitly allows extension params.
- `value_type` matches the parsed value.
- Asset path params resolve through the existing runtime asset catalog.
- `skill_id`, `choice_id`, `status_id`, and enum params reference valid known values.
- A migrated behavior is not active in both legacy wide fields and normalized node rows at the same time.

## Verification Expected From Code Builder

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`
- Unity `Pakuri/Validate CSV Source Data`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` when new CSV files or catalog asset references are added
- CSV field-count and reference checks for all new files
- Code inspection showing at least one current execute/boss/kill behavior can compile from normalized rows
- Code Reviewer pass before using the new node path for real skill authoring

## Non-Goals

- Do not physically delete old columns in this step.
- Do not migrate every existing skill row at once.
- Do not redesign rewards, run flow, UI, or monster-specific balance in this handoff.
- Do not convert `monster_skill_effects.csv` and `monster_skill_triger.csv` in the first pass.
- Do not claim Play Mode equivalence from Codex. User owns gameplay verification.

## Related Boards To Update

- `boards/DATA/DATA_BLACKBOARD.md`: primary board for CSV schema, parser, validation, and catalog behavior.
- `boards/COMBAT/ENEMY_BLACKBOARD.md`: update when normalized nodes affect `SkillExecutionPlan`, executor routing, or skill behavior runtime.
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`: update only if node params add new asset-path catalog ownership.
- `boards/RUN/RUN_BLACKBOARD.md`: update only if NewRunScene run flow or reward offering behavior changes.
