## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/ARIEL_MONSTER.md`.

# ARIEL_MONSTER

## Scope

Ariel dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Ariel file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Task: 2026-07-11 Ariel Skill Graph Nodes Migration Design

### Task title

Design Ariel's first migration from authored node ids and node params to owner-keyed skill graph nodes.

### Goals

- Replace Ariel's 124 current node instance rows and 179 param rows with 124 graph node rows that own the real values.
- Use real `choice_id` values for Choice graph ownership instead of semantic Effect names.
- Reclassify Ariel Effect composition under Choice, Skill, or Trigger graph owners without changing runtime behavior.
- Keep all non-Ariel legacy node/effect paths operational during the first migration.

### Constraints

- Role Owner is Designer.
- Documentation/design only; implementation has not started.
- Ariel legacy effects are already zero, but the shared effects directory remains because it contains 96 Eve/Rin/Sein/Vega rows.
- Existing Ariel trigger wide payload conversion is outside the first storage-compatibility migration except for two Effect graph references.
- No MSW-MCP was used.

### Role Owner

Designer

### Status

Code Builder handoff created.

### Next Actions

- Implement `boards/MON/ARIEL_SKILL_GRAPH_NODES_MIGRATION_PLAN.md` in phases.
- Preserve current player-facing Ariel behavior while compiling graph rows into the existing `SkillNodeDefinition` and `SkillEffectDefinition` runtime contracts.
- Require user Play Mode verification after compile/catalog validation.

### Evidence

- Created `boards/MON/ARIEL_SKILL_GRAPH_NODES_MIGRATION_PLAN.md`.
- Ariel currently has 50 Choice rows, 39 Choice-owned nodes, and 85 Effect-owned nodes grouped into 20 Effects.
- Choice-gated Ariel Effect groups account for 45 nodes across 11 graphs; no-Choice Effect groups account for 40 nodes across 9 graphs.
- Target ownership is 39 Choice/Plan rows, 45 Choice/Effect rows, 36 Skill/Effect rows, and 4 Trigger/Effect rows.
- `ariel-a-master-2-holy-exposure-on-hit` becomes `Choice / ariel-a-master-2 / Effect / 0` and uses the real choice id as owner identity.
- Three Ariel references to old Effect owner strings must move with graph identity: two trigger effect references and one condition `source_skill_id` reference.

### History

- 2026-07-11: User requested an implementation-design MD for first migrating Ariel to definition-only nodes plus value-owning `skill_graph_nodes`, while retaining legacy effects until all monsters finish migration.

## Task: 2026-07-11 Ariel Node Decomposition Guide

### Task title

Create an Ariel-grounded guide for decomposing other monsters' legacy effect rows into semantic nodes.

### Goals

- Compare Ariel A-J reference intent with the current Ariel Choice/Effect node implementation.
- Document the current implemented node handlers and distinguish runtime-wired handlers from schema-only names.
- Give an AI agent a repeatable method for separating base skill bodies, Choice modifiers, Effect groups, and Trigger bindings.
- Prefer existing handlers and require evidence plus user approval before shared runtime handler expansion.

### Constraints

- Role Owner is Designer.
- This task creates design/documentation only; no skill CSV or runtime code implementation was performed.
- Evidence is limited to the user-provided Ariel reference directory, current runtime node/effect CSVs, relevant runtime code, and routed Ariel/DATA boards.
- No other monster reference markdown or archived monster implementation was inspected for value discovery.
- No MSW-MCP was used.
- Unity Play Mode verification remains user-owned for later implementation work.

### Role Owner

Designer

### Status

Design and implementation handoff created.

### Next Actions

- Code Builder or Skill Builder can migrate a selected monster in small skill-kind batches using `boards/MON/ARIEL_NODE_DECOMPOSITION_GUIDE.md`.
- Start with legacy effect rows that the guide classifies as supported by current handlers.
- If a selected row needs shared handler work, provide the required-handler evidence bundle and obtain user approval before expanding runtime scope.

### Evidence

- Created `boards/MON/ARIEL_NODE_DECOMPOSITION_GUIDE.md`.
- Inspected all 10 files under `Pakuri/reference/2.Monster/ariel/skill` and compared their A-J behavior families with current Ariel node groups.
- Current disk aggregation returned 124 Ariel nodes: 39 Choice-owned nodes and 85 Effect-owned nodes grouped under 20 Effect owner ids.
- The guide records the current Ariel pattern: base skill body remains in base CSV, numeric changes use Choice nodes, independent behavior uses one-operation Effect groups, and event timing remains in trigger CSV.
- The guide explicitly keeps current Trigger rows out of normalized migration because validation reports `owner_kind=Trigger` is not wired into runtime plans.
- Current non-Ariel effect audit returned 96 rows: 58 immediately representable, 16 requiring existing-handler Effect composer extension, and 22 requiring genuinely new semantic handler support.

### History

- 2026-07-11: User requested an evidence-based MD that lets an AI agent use Ariel's current node implementation to decompose other monsters' effect CSV behavior, reuse existing nodes first, and add handlers only when necessary.

## Task: 2026-07-10 Ariel Runtime Visual Migration Implementation

### Task title

Implement Ariel base/trigger/status runtime visual and hitbox assembly without prefab fallback.

### Goals

- Convert Ariel A/B/C/D/E base visual data and Ariel A/B trigger visual-hitbox data to `runtime_visual_*` CSV fields.
- Keep Ariel skill prefabs in the repository, but avoid using them as runtime fallback for converted Ariel base/trigger/status paths.
- Preserve user corrections: do not add node CSV columns, do not migrate local position/rotation, do not preserve Ariel E's incorrect collider offset, and treat animation as visual playback rather than universal lifetime.

### Constraints

- Role Owner is Code Builder.
- Work is based on `boards/MON/ARIEL_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md` and inspected runtime code/CSV/prefab evidence.
- No MSW-MCP was used.
- Node-owned explicit effect prefab references are not converted in this task because the user previously rejected adding runtime visual/hitbox columns to node CSVs.
- Unity Play Mode visual parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Run Unity Editor CSV catalog sync/validation if editor asset refresh validation is required.
- User verifies Ariel A projectile, Ariel B shield visual, Ariel B trait4 trigger hitbox, Ariel C/E single-attack hitboxes, and Ariel D status visual in Play Mode.
- If full node-owned visual removal is desired later, create a separate node-effect visual migration that does not add broad node CSV columns without user approval.

### Evidence

- Added `RuntimeSkillVisualSpec`, `RuntimeSkillHitboxSpec`, and runtime visual CSV fields in `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`.
- Added runtime AnimatorController catalog support in `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeAssetCatalog.cs`, `PakuriCsvRuntimeData.AssetReferences.cs`, `PakuriCsvRuntimeData.Editor.cs`, `PakuriCsvRuntimeData.Validation.cs`, and `PakuriCsvRuntimeData.Build.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/RuntimeSkillVisualFactory.cs`.
- Runtime creation is used by `ProjectileSkillExecutor`, `SingleAttackSkillExecutor`, `SupportSkillExecutors`, `SkillTriggerRuntime`, `SkillVisualSpawnUtility`, and status visuals in `InGameCombatManager`.
- Ariel runtime CSV rows were updated in `base/projectile/skills_projectile.csv`, `base/buff/skills_buff.csv`, `base/single_attack/skills_single_attack.csv`, `triggers/buff/buff_skill_triger.csv`, and `triggers/projectile/projectile_skill_triger.csv`.
- CSV field-count verification passed for all five edited CSV files.
- Removed the implemented shape/trigger-state CSV columns from the runtime visual schema. Positive hitbox size now creates a runtime `BoxCollider2D`; projectile runtime visuals pass trigger mode in code, while non-projectile runtime visual paths default to non-trigger mode.
- Search confirmed base/trigger/status runtime skill CSVs contain no Ariel `Assets/Prefab/Skill/Ariel` prefab paths after this pass. Remaining Ariel prefab paths are node-owned entries in `nodes/single_attack/single_attack_skill_node_params.csv` only.
- Follow-up Unity auto-sync row-width failure was traced to stale imported TextAsset risk rather than the current disk CSV: `PakuriCsvLineCodec` counted `skills_projectile.csv` row 4 as 38 columns, matching the 38-column header.
- `PakuriCsvRuntimeData.Editor.cs` now forces synchronous AssetDatabase import before runtime catalog sync reads source CSV TextAssets.
- Unity-MCP sync and validation menus completed without CSV fatal errors after the refresh/import change.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-07-10: User requested Code Builder to implement from `ARIEL_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`, keep Ariel skill prefabs undeleted, and ensure skill implementation does not use prefabs as fallback.
- 2026-07-10: Implemented runtime visual/hitbox specs and converted Ariel base/trigger/status CSV rows. Ariel prefabs were not deleted.
- 2026-07-10: User requested removing common hitbox shape/trigger CSV columns and clearing Ariel prefab paths from base/trigger/status; Code Builder removed those columns, moved trigger-state policy into code, and verified converted base/trigger/status paths no longer reference Ariel prefabs.
- 2026-07-10: User reported Unity auto-sync row 4 width error; Code Builder verified current CSV shape, forced Unity sync/validation successfully, and added sync-time source TextAsset refresh to reduce stale import recurrence.

## Task: 2026-07-10 Ariel Runtime Visual Migration Plan

### Task title

Create Ariel-only runtime visual and hitbox migration design.

### Goals

- Document how Ariel skill prefabs can move to runtime-created visual/hitbox objects.
- Keep difficult authored-prefab structures out of the Ariel-only migration scope.
- Preserve the user's corrections: base/trigger-owned values should live in base/trigger CSVs, node CSVs should not receive new runtime visual/hitbox columns in this Ariel migration, root local position/rotation should not be migrated, Ariel E hitbox offset should be treated as `0,0`, and animation controllers are visual playback data rather than universal lifetime data.

### Constraints

- Role Owner is Designer.
- This is a design/documentation task only; no CSV, code, scene, or prefab implementation was performed.
- The plan is based on inspected Ariel prefab YAML, runtime executor code, current runtime CSV headers/rows, and Ariel board history.
- No MSW-MCP was used.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Designer

### Status

Design handoff created.

### Next Actions

- Code Builder can implement from `boards/MON/ARIEL_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md` after user approval.
- Before converting `Ariel_C.prefab`, Code Builder must resolve or intentionally retire the missing MonoBehaviour GUID `e8261e6f2e5fac44da64da2b23939e9a`.
- When implementation changes CSV/runtime asset authority, update this board together with the relevant DATA board.

### Evidence

- Created `boards/MON/ARIEL_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.
- `Airel_A.prefab`, `ariel-b-trait-4_Skill.prefab`, `Ariel_B.prefab`, `Ariel_C-Buff.prefab`, `Ariel_C.prefab`, `Ariel_D.prefab`, and `Ariel_E.prefab` were inspected for SpriteRenderer, Animator, BoxCollider2D, scale, sprite GUID, animator controller GUID, and script GUID evidence.
- Runtime code evidence used in the plan includes `EffectManager.InstantiateSkillPrefab(...)`, `ProjectileSkillExecutor`, `InGameProjectileActor`, `ZoneSkillExecutor`, `InGameZoneSkillActor`, `BeamSkillExecutor`, `InGameLineAttackActor`, `SingleAttackSkillExecutor`, and `SkillVisualSpawnUtility`.
- Current CSV evidence used in the plan includes `skills_projectile.csv`, `skills_buff.csv`, `skills_single_attack.csv`, `buff_skill_triger.csv`, `projectile_skill_triger.csv`, and `single_attack_skill_node_params.csv`.

### History

- 2026-07-10: User asked for an MD based on the discussion of converting Ariel skill prefabs to runtime structure, explicitly requiring the corrections about CSV ownership, unnecessary local position/rotation, optional hitbox offset, Ariel E offset correction, and animation/lifetime separation to be included.
- 2026-07-10: User clarified that this Ariel migration should not add new runtime visual/hitbox values to node CSVs; the plan was corrected to target base and trigger CSV columns, while leaving existing node-owned prefab references as fallback or separate future work.

## Task: 2026-07-09 Ariel F-J Passive EffectTarget Param Cleanup

### Task title

Clean copied EffectTarget defaults from Ariel F-J passive node params.

### Goals

- Apply `boards/TRAIT_MASTER/ARIEL_FJ_PASSIVE_NODE_CONVERSION_PLAN.md` to Ariel F-J passive runtime node params.
- Keep Ariel passive effects decomposed into functional nodes rather than copied old effect-row target defaults.
- Preserve all gameplay-carrying params for F-J passives and traits.

### Constraints

- Role Owner is Code Builder.
- Current files are skill-kind paths under `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes`.
- The implementation is CSV-only.
- No MSW-MCP was used.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and verified by CSV checks plus runtime/editor compile.

### Next Actions

- User verifies Ariel F/J holy damage, action speed, shield, blessing, holy-exposure, and shielded-holy passive interactions in Play Mode if gameplay parity is required.
- Future Ariel passive node edits should avoid reintroducing `target_selection=Owner`, `target_shape=Battlefield`, `center_mode=Caster`, or no-visual `visual_anchor_mode=AppliedTargets` on status/shield EffectTarget nodes.

### Evidence

- `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/passive/passive_skill_node_params.csv` no longer has Ariel F-J passive rows with `target_selection`, `target_shape`, `center_mode`, or `visual_anchor_mode`.
- Ariel F-J passive EffectTarget rows still retain functional target sides: F/G/H/J ally effects use `target_side=AllAllies`, and Ariel I holy-exposure effects now explicitly use `target_side=Enemy`.
- `ariel-g-start-shield-effect-target` still keeps `apply_once=true`; `ariel-j-shielded-holy-damage-condition-status` still keeps `source_skill_id=ariel-e-shield-base`.
- Condition/status/modifier values remain in node params, including `status_id=shield`, `status_id=blessing`, `status_id=holy-exposure`, `min_stacks=1`, Holy attributes, bonuses, multipliers, and lifetimes.
- Removed-param scan returned `REMOVED_PASSIVE_TARGET_DEFAULTS_OK`.
- Passive node-param reference check returned `PASSIVE_NODE_PARAM_REFS_OK nodes=58 params=68`.
- Full runtime skill CSV shape check returned `CSV_SHAPE_OK files=31`.
- Full node-param reference check returned `NODE_PARAM_REFS_OK nodes=139 params=212 paramFiles=4`.
- Runtime and editor `dotnet build` commands passed with 0 errors and 2 warnings each.

### History

- 2026-07-09: User requested Code Builder to implement the Ariel F-J passive node conversion plan after identifying copied EffectTarget-style params in passive node params.

## Task: 2026-07-09 Ariel Enhancement/Master Node Cleanup

### Task title

Restore Ariel enhancement and master behavior to functional normalized nodes.

### Goals

- Remove Ariel copied Effect groups that represented enhancement/master value combinations instead of atomic node behavior.
- Keep base effects as effect-owned nodes and keep enhancement/master deltas as Choice-owned functional nodes.
- Fix blessing duration modifiers to use status-targeted duration nodes.

### Constraints

- Role Owner is Code Builder.
- Current files are skill-kind paths under `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes`.
- No MSW-MCP was used.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and verified by CSV checks plus runtime/editor compile.

### Next Actions

- User verifies Ariel C blessing combinations, Ariel E shield trait/master combinations, Ariel G/J passive modifiers, and Ariel I passive modifier in Play Mode.
- Future Ariel modifiers should avoid copied `EffectTarget`/`EffectLifetime` groups when a functional Choice node can modify the base effect.

### Evidence

- `nodes/passive/passive_skill_nodes.csv` no longer has copied Effect owners `ariel-j-after-e-action-speed-trait1`, `ariel-g-shield-received-trait1`, `ariel-g-start-shield-trait2`, or `ariel-i-holy-exposure-damage-taken-trait1`.
- `nodes/passive/passive_skill_node_params.csv` no longer has params for those copied Effect nodes.
- `nodes/single_attack/single_attack_skill_nodes.csv` no longer has `MigratedToEffectBinding` copies for Ariel C blessing trait/master combinations or Ariel E shield trait/master combinations.
- `nodes/single_attack/single_attack_skill_node_params.csv` no longer has params for the removed copied Effect nodes.
- `ariel-c-trait-3-duration-bonus` and `ariel-h-trait-3-duration-bonus` are now `StatusDurationBonus` nodes with `status_id=blessing` and `bonus_seconds=2`.
- Retained functional nodes include `StatusActionSpeedBonus` for `ariel-c-trait-2-blessing-action-speed` and `ariel-j-trait-1-after-e-action-speed-bonus`, `ShieldAmountMultiplier` for Ariel E/G shield modifiers, `StatusShieldReceivedBonus` for `ariel-g-trait-1`, and `StatusDamageTakenBonus` for `ariel-i-trait-1`.
- Inspected runtime code confirms choice nodes are applied through `SkillExecutionSnapshot.ApplyNodeBackedChoiceDefinition`, `SkillExecutionSystem.AppliesToSkill`, `SkillMultiEffectExecutor.ResolveStatusSpec`, and `SkillMultiEffectExecutor.ResolveStatusEffectShieldAmount`.
- Removed-id scan and `MigratedToEffectBinding` scan under runtime skill nodes returned no rows.
- CSV checks returned `CSV_SHAPE_OK files=31`, `NODE_PARAM_REFS_OK nodes=139 paramFiles=4`, and `MIGRATED_TO_EFFECT_BINDING_COUNT=0`.
- Runtime and editor `dotnet build` commands passed with 0 errors and 2 warnings each.

### History

- 2026-07-09: User identified passive rows such as `ariel-j-after-e-action-speed-trait1-effect-target` as copied old CSV values instead of fully node-based behavior, then requested Code Builder implementation and verification.

## Task: 2026-07-08 Ariel Node CSV Kind Folder Split

### Task title

Split Ariel normalized node CSVs into `node` and `nodes_param` folders by target skill kind.

### Goals

- Replace `nodes/ariel/ariel_skill_nodes.csv` with kind-owned node CSV files under `nodes/ariel/node`.
- Replace `nodes/ariel/ariel_skill_node_params.csv` with matching kind-owned param CSV files under `nodes/ariel/nodes_param`.
- Keep row semantics unchanged and preserve runtime loader compatibility through existing `_skill_nodes.csv` / `_skill_node_params.csv` suffix collection.

### Constraints

- Role Owner is Code Builder.
- Split classification is based on Ariel base skill kinds and inspected node `target_skill_id`; blank `target_skill_id` rows are classified by the `ariel-a` through `ariel-j` prefix in `owner_id` or `node_id`.
- No MSW-MCP was used.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If Unity Editor asset import validation is needed, run `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` through Unity-MCP.
- Keep future Ariel node files in `nodes/ariel/node/{kind}_skill_nodes.csv` and matching params in `nodes/ariel/nodes_param/{kind}_skill_node_params.csv`.

### Evidence

- `PakuriCsvRuntimeData.Editor.cs` uses `Directory.GetFiles(..., SearchOption.AllDirectories)` and collects node files by `_skill_nodes.csv` and params by `_skill_node_params.csv`, so the new nested folders remain collectible.
- Created `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/ariel/node/` with `ariel_buff_skill_nodes.csv`, `ariel_passive_skill_nodes.csv`, `ariel_projectile_skill_nodes.csv`, and `ariel_single_attack_skill_nodes.csv`.
- Created `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/ariel/nodes_param/` with matching `ariel_*_skill_node_params.csv` files.
- Deleted old aggregate files `nodes/ariel/ariel_skill_nodes.csv` and `nodes/ariel/ariel_skill_node_params.csv` plus their `.meta` files.
- Split verification returned node row counts `buff=10`, `passive=74`, `projectile=7`, `single_attack=102`, total `193`.
- Split verification returned param row counts `buff=16`, `passive=122`, `projectile=11`, `single_attack=181`, total `330`.
- Integrity verification returned `duplicate_node_ids=0`, `missing_param_node_refs=0`, and `classification_bad=0`.
- `PakuriCsvRuntimeSourceCatalog.asset` now references all 8 new Ariel split node/param GUIDs and no longer references old GUIDs `490b714ca887432a88e50852efb95d86` or `2226b360df36427c83062e3be7af19c3`.
- Unity auto-sync reported `ArgumentException: An item with the same key has already been added. Key: ApplyStatus`; inspected code showed duplicate `AddSkillNodeHandlerSchema(schemas, "ApplyStatus", ...)` registrations at `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now keeps only the effect-owned `ApplyStatus` schema registration used by current Ariel node CSVs.
- Follow-up Unity validation reported unknown `triggered_effect_id` for `ariel-a-master-2-holy-exposure-on-hit` and `ariel-j-after-e-action-speed`, plus unknown skill `ariel-e-shield-base` for `source_skill_id`.
- `PakuriCsvRuntimeData.Validation.cs` now treats every `IsEffectOperationHandler(...)` node-owned effect operation, including `ApplyStatus`, `ApplyShield`, and `StatusModifier`, as a valid skill effect source.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now allows a `source_skill_id` node param to reference a node-owned effect source when that id is not a base skill id.
- Unity-MCP ran `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data`; console filters returned 0 entries for `Pakuri CSV source validation failed`, `unknown triggered_effect_id`, and `references unknown skill 'ariel-e-shield-base'`, and logged `InGame skill data validation passed with 0 warning(s).`
- `git diff --check` passed with only the existing line-ending normalization warning for `PakuriCsvRuntimeSourceCatalog.asset`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors and 2 existing warnings each; the editor build was rerun after an initial transient `Assembly-CSharp.dll` file-lock failure.

### History

- 2026-07-08: User clarified that the requested two-folder split meant `node` versus `nodes_param`, not `choices` versus `effects`; Code Builder split Ariel node CSV rows by strengthened target skill kind in those two folders.

## Task: 2026-07-08 Ariel Effect CSV Full Node Migration

### Task title

Move Ariel's remaining skill effects from `ariel_skill_effects.csv` into effect-owned normalized nodes.

### Goals

- Keep `ariel_skill_triger.csv` unchanged while preserving its `triggered_effect_id` bindings.
- Move all 36 Ariel effect rows into `nodes/ariel/ariel_skill_nodes.csv` and `nodes/ariel/ariel_skill_node_params.csv`.
- Make `holy-exposure` itself carry the common Holy damage-taken +15% behavior in `status_effects.csv`.
- Delete `skills/effects/ariel/ariel_skill_effects.csv` after verifying all effect ids exist as node-owned effect definitions.

### Constraints

- Role Owner is Code Builder.
- Work is based on inspected runtime CSV/code only.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if editor validation is needed.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, corrected to semantic nodes, compiled, file-verified, and deleted the Ariel effect CSV.

### Next Actions

- User verifies Ariel effect parity in Play Mode, especially Ariel A master2 holy exposure, Ariel C blessing variants, Ariel E shield/sanctuary, and Ariel J post-E trigger.
- If Unity Editor validation is needed, run `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` through Unity-MCP.
- Keep future Ariel effect-owned node params semantic-only; do not recreate deleted effect CSV rows as `*-base` nodes.

### Evidence

- `PakuriCsvRuntimeData.Build.cs` now builds `SkillEffectDefinition` entries from `owner_kind=Effect` semantic node groups and still supports legacy `SkillEffectRow` data for other monsters.
- `PakuriCsvRuntimeData.Build.cs` no longer requires a `*-base` effect node or a carrier `EffectStatus` node. It creates the effect definition from one operation node (`ApplyStatus`, `ApplyShield`, `StatusModifier`, `EffectDamage`, or `EffectExtendStatusDuration`) and composes `EffectTarget`, `EffectVisual`, `ConditionStatus`, `ConditionSkillAttribute`, `EffectLifetime`, and status modifier nodes into that definition.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers effect-owned operation handlers `ApplyStatus`, `ApplyShield`, and `StatusModifier` in addition to `EffectDamage` and `EffectExtendStatusDuration`.
- `ConditionStatus` now supports `status_id`, `target_side`, `min_stacks`, and optional `source_skill_id`; `Build.cs` converts `min_stacks` into the existing condition expression format such as `shield:2`.
- `StatusDamageBonusRate` and `StatusFlatElementResistReduction` now own their `attribute` param directly instead of relying on an old effect-row/base attribute field.
- `PakuriCsvRuntimeData.Validation.cs` now accepts `triggered_effect_id` when it is supplied by an effect-owned node group.
- `PakuriCsvRuntimeData.AssetReferences.cs` now collects `AssetPath` node params, so effect prefabs moved out of effect CSV remain in the asset catalog.
- `ariel_skill_nodes.csv` / `ariel_skill_node_params.csv` now contain 36 effect-owned semantic node groups with no effect `node_id` ending in `-base`, no `EffectStatus` nodes, and no `passive-buff` value in node params.
- Ariel node verification returned handler counts: `ApplyShield=6`, `ApplyStatus=13`, `StatusModifier=14`, `EffectDamage=2`, `EffectExtendStatusDuration=1`, `ConditionStatus=10`, `EffectLifetime=33`, `EffectTarget=36`, and `EffectVisual=11`.
- Ariel node verification returned `effects=36`, `nodes=193`, `params=330`, `passive_buff_param_count=0`, and `effect_status_nodes=0`.
- Effect group verification returned `effect_groups=36` and `missing_count=0`; each group has exactly one operation node and required params for operation, condition, lifetime, and visual nodes.
- `ariel-b-shielded-holy-trait5` is now split into `StatusModifier`, `EffectTarget`, `ConditionStatus`, `EffectLifetime`, and `StatusDamageBonusRate`; the condition carries `status_id=shield`, `target_side=AllAllies`, and `min_stacks=1`, while the Holy attribute lives on `StatusDamageBonusRate`.
- Shield effects such as `ariel-e-shield-base` now use `ApplyShield` with shield amount params (`base_damage`, `spell_power_coefficient`, optional `damage_multiplier`) instead of `EffectStatus(status_id=shield)`.
- Real status applications such as `ariel-a-master-2-holy-exposure-on-hit` use `ApplyStatus(status_id=holy-exposure)`.
- `ariel-a-master-2-holy-exposure-element-damage-taken` was removed from Ariel choice nodes; `status_effects.csv` now has `holy-exposure` with `element_damage_taken_bonus_per_stack=0.15`.
- Verification before deletion returned `effect_csv_count=36 node_base_count=36 missing=0 extra=0`; after semantic-node correction, parity returned `effect_csv_count=36 node_owner_count=36 missing=0 extra=0`.
- Trigger verification returned `ariel-a-master2-holy-exposure-on-hit` and `ariel-j-after-e-action-speed-trigger` with `node_effect_exists=True`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/effects/ariel/ariel_skill_effects.csv` and its `.meta` were deleted.
- `PakuriCsvRuntimeSourceCatalog.asset` no longer references deleted GUID `de95dfd09fa14fd5bffaf64855a35d25`; post-delete search returned no matches.
- `git diff --check` returned `DIFF_CHECK_OK`; Git also printed existing line-ending normalization warnings.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-07-08: User confirmed all `holy-exposure` instances should share Holy damage +15%, then requested Code Builder to migrate the remaining Ariel effect CSV content to nodes and delete `ariel_skill_effects.csv` only after confirming full migration.
- 2026-07-08: User pointed out migrated effect node params still contained meaningless defaults like the original `ariel-b-shielded-holy-trait5-base` expansion; Code Builder first compacted defaults, then replaced that insufficient approach after the user clarified the intended node-composition structure.
- 2026-07-08: User clarified the previous conversion was still wrong because it copied deleted effect CSV rows into `*-base` nodes instead of implementing semantic nodes. Code Builder rebuilt Ariel effect-owned nodes without `*-base` operation nodes and split targeting, visuals, conditions, lifetime, and modifiers into separate handlers.
- 2026-07-08: User clarified that merely removing the `-base` suffix was still insufficient because `EffectStatus(status_id=passive-buff|shield|blessing)` rows were carrier rows. Code Builder replaced those carriers with semantic `ApplyStatus`, `ApplyShield`, and `StatusModifier` operations and removed `passive-buff` from Ariel node params.

## Task: 2026-06-19 Ariel Plan-Action Runtime Migration

### Task title

Move Ariel choice modifiers from old wide choice folding to Ariel-only `SkillExecutionPlan` action handling.

### Goals

- Remove remaining Ariel old wide behavior payloads from `monster_skill_choices.csv`.
- Compile Ariel choice-owned normalized nodes into `SkillExecutionPlanNode.Action` payloads.
- Make Ariel snapshot mutation use plan action handlers instead of `ApplyNormalizedChoiceNodes(...)` folding into `SkillChoiceEffectSpec`.
- Keep Ariel A master2 status application on the explicit trigger/effect handler path while the +15% Holy damage-taken modifier stays a normalized node.

### Constraints

- Role Owner is Code Builder, then Code Reviewer.
- Compatibility gate is Ariel-only by `monster_id=ariel` or `choice_id` prefix `ariel-`.
- Full trigger/effect rows are not yet unified into `SkillExecutionPlan.Actions`; they remain explicit `monster_skill_triger.csv` / `monster_skill_effects.csv` runtime objects.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder / Code Reviewer

### Status

Implemented and reviewed. Passed for the Ariel-first target scope; full future goal of putting every trigger/effect action inside one `SkillExecutionPlan` is still not complete.

### Next Actions

- User verifies Ariel A master2, Ariel D trait4/master1, and dynamic shield-count damage behavior in Play Mode.
- Future migration can move trigger/effect CSV rows into plan action nodes if the target architecture requires a single plan-owned execution list.

### Evidence

- `SkillExecutionPlan.cs` now exposes `SkillExecutionPlanNode.Action`, `SkillExecutionPlanNode.FromAction(...)`, and `SkillExecutionPlan.Actions`.
- `SkillExecutionSnapshot.cs` now detects Ariel choices, maps `choice.NormalizedPlanNodes` through `InGameSkillDefinitionMapper.MapSkillNodeDefinitions(...)`, applies `ApplyPlanActionNodes(...)`, and skips the old `SkillChoiceEffectSpec` path for Ariel.
- `InGameSkillDefinitionMapper.cs` now skips `ApplyNormalizedChoiceNodes(...)` for Ariel choices and maps normalized node handlers such as `HitTargetCountBonus` and `StatusCriticalDamageTakenBonus` into `SkillActionOp`.
- `SkillExecutionSystem.cs` now resolves Ariel dynamic `CountStatusDamageMultiplier` through mapped plan action nodes while keeping the old wide dynamic path for non-Ariel compatibility.
- `monster_skill_choices.csv` Ariel old behavior-field scan returned `arielWideNonDefault=0`.
- `monster_skill_nodes.csv` has `ariel-d-trait-4-hit-target-count-bonus` with handler `HitTargetCountBonus`, and `monster_skill_node_params.csv` stores `bonus=1`.
- `monster_skill_nodes.csv` has `ariel-d-master-1-status-critical-damage-taken` with handler `StatusCriticalDamageTakenBonus`, and `monster_skill_node_params.csv` stores `bonus=0.25`.
- `monster_skill_triger.csv` has `ariel-a-master2-holy-exposure-on-hit` with `trigger_event=OnOutgoingDamage`, `requires_active_choice_id=ariel-a-master-2`, `target_selection=EventTarget`, `trigger_action=Effect`, and `triggered_effect_id=ariel-a-master-2-holy-exposure-on-hit`.
- `monster_skill_effects.csv` has `ariel-a-master-2-holy-exposure-on-hit` with `status_effect_id=holy-exposure`, `status_chance=1`, and `status_stack_amount=1`.
- Runtime and editor `dotnet build` commands passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP sync/validate menus logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`; warning/error console read returned 0 entries.

### History

- 2026-06-19: User requested Code Builder to perform steps 1-6 for Ariel-first target-structure migration, then Code Reviewer review.
- 2026-06-19: Builder added plan action payloads and Ariel-only runtime routing; Reviewer found no blocking defects in the scoped Ariel-first migration, with the explicit caveat that trigger/effect rows are still not single-plan action nodes.

## Task: 2026-06-19 Ariel A Master2 Trigger Binding Fix

### Task title

Convert Ariel A master2 holy exposure from old choice wide columns to trigger/effect/node composition.

### Goals

- Replace `ariel-a-master-2` choice-wide status payload with a trigger-bound status effect object.
- Apply holy exposure to the hit event target through the current trigger runtime.
- Keep the +15% Holy damage taken value in a normalized status modifier node.
- Prevent migrated Ariel E shield variants from executing through leftover choice gates.

### Constraints

- Role Owner is Code Builder.
- Current trigger enum has no `OnHit`; the implemented runtime event is `OnOutgoingDamage`, which is the existing hit-success trigger path.
- Active CSV authority is under `Pakuri/Assets/CSVdata/runtime`.
- Unity Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-checked, and Unity-MCP validated. Code Reviewer was not rerun.

### Next Actions

- User verifies in Play Mode that Ariel A master2 applies `holy-exposure` to enemies hit by Ariel A.
- User verifies Ariel E shield amount variants no longer double-apply old precombined shield rows with the new shield amount nodes.

### Evidence

- `monster_skill_choices.csv` now keeps `ariel-a-master-2` as `RuntimeImplemented` with blank `status_tag`, `status_chance_bonus`, `status_stacks_set`, and `status_element_damage_taken_bonus`.
- `monster_skill_triger.csv` now has `ariel-a-master2-holy-exposure-on-hit` with `source_skill_id=ariel-a`, `trigger_event=OnOutgoingDamage`, `requires_active_choice_id=ariel-a-master-2`, `target_selection=EventTarget`, `trigger_action=Effect`, and `triggered_effect_id=ariel-a-master-2-holy-exposure-on-hit`.
- `monster_skill_effects.csv` now has `ariel-a-master-2-holy-exposure-on-hit` as a `Status` effect for `status_effect_id=holy-exposure`, `status_chance=1`, and `status_stack_amount=1`.
- `monster_skill_nodes.csv` and `monster_skill_node_params.csv` now add `ariel-a-master-2-holy-exposure-element-damage-taken` with handler `StatusElementDamageTakenBonus` and `bonus=0.15`.
- `monster_skill_effects.csv` no longer has executable `requires_active_choice_id` or `requires_passive_skill_id` gates on `MigratedToEffectBinding` rows, including the three Ariel E shield variants.
- `SkillTriggerRuntime.ExecuteEffect(...)` now forwards `triggerContext.EventTarget` into `SkillExecutionContext`, so `target_selection=EventTarget` works for trigger-bound effect rows.
- `PakuriCsvRuntimeData.Validation.cs` now rejects `MigratedToEffectBinding` skill effects that still carry executable choice/passive gates.
- `Import-Csv` property-count check returned `monster_skill_choices.csv props=114 dataRows=253 bad=`, `monster_skill_effects.csv props=71 dataRows=133 bad=`, `monster_skill_triger.csv props=47 dataRows=59 bad=`, `monster_skill_nodes.csv props=14 dataRows=54 bad=`, and `monster_skill_node_params.csv props=4 dataRows=76 bad=`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` executed; console logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error entries.

### History

- 2026-06-19: Code Reviewer found `ariel-a-master-2` still used old choice-wide status columns and Ariel E migrated shield variants still had executable choice gates.
- 2026-06-19: User confirmed Ariel A master2 should be represented as trigger on hit, event target, apply status, and `status_id=holy-exposure`; Builder implemented the current-runtime equivalent using `OnOutgoingDamage` plus trigger-bound status effect and normalized node modifier.

## Task: 2026-06-19 Ariel Passive Node Decomposition Follow-up

### Task title

Convert remaining Ariel passive numeric modifiers to atomic normalized nodes.

### Goals

- Make Ariel F/G/H/I/J passive numeric upgrades compose like Ariel C: base effect objects plus modifier nodes and trigger bindings.
- Remove duplicate execution paths where old choice-gated effect or trigger rows would stack with the new nodes.
- Keep conceptually separate effects such as F trait3 crit, G trait3 shielded holy damage, and I trait3 holy resist reduction as effect objects.

### Constraints

- Role Owner is Code Builder.
- The implementation stays on `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No MSW-MCP is used; Unity checks use Unity-MCP only.
- Unity Play Mode parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-checked, and Unity-MCP validated.

### Next Actions

- User verifies Ariel F/G/H/I/J passive combinations in Play Mode.
- Keep future passive numeric add-ons on normalized nodes before adding choice-gated duplicate effect rows.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers `StatusDamageBonusRate`, `StatusShieldReceivedBonus`, `StatusCriticalChanceBonus`, `StatusDamageTakenBonus`, and `StatusFlatElementResistReduction` normalized handlers.
- `SkillChoiceEffectSpec`, `SkillExecutionSnapshot`, `InGameSkillDefinitionMapper`, `SkillStatusSpecUtility`, and `SingleAttackSkillExecutor` now carry those status modifier nodes into status data; existing element/crit/ailment status bonuses now accumulate instead of replacing base values.
- `monster_skill_nodes.csv` now uses status modifier nodes for `ariel-f-trait-1-holy-damage-bonus`, `ariel-g-trait-1-shield-received-bonus`, `ariel-g-trait-2-start-shield-amount-multiplier`, `ariel-h-trait-1-blessed-holy-damage-bonus`, `ariel-h-trait-2-blessed-action-speed-bonus`, `ariel-i-trait-1-exposure-damage-taken-bonus`, `ariel-j-trait-1-after-e-action-speed-bonus`, and `ariel-j-trait-2-shielded-holy-damage-bonus`.
- `monster_skill_effects.csv` marks `ariel-g-shield-received-trait1`, `ariel-g-start-shield-trait2`, `ariel-i-holy-exposure-damage-taken-trait1`, and `ariel-j-after-e-action-speed-trait1` as `MigratedToEffectBinding`.
- `monster_skill_triger.csv` no longer contains `ariel-j-after-e-action-speed-trait1-trigger`; J trait1 now modifies the base J post-E trigger effect through a normalized node.
- CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=52 bad=`, `monster_skill_node_params.csv header=4 rows=74 bad=`, `monster_skill_effects.csv header=71 rows=131 bad=`, and `monster_skill_triger.csv header=47 rows=57 bad=`.
- Ariel spot check returned `{"fNode":1,"gNodes":2,"hNodes":2,"iTrait1Migrated":1,"jTrait1TriggerRows":0,"oldGenericPassiveDamageNodes":0}`.
- Active old-support check returned `activeReferenceDirectEffects=0 referenceDirectTriggers=0` for Ariel rows.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP console after `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` logged runtime catalog sync/load and `InGame skill data validation passed with 0 warning(s)`, with 0 warning/error console entries.

### History

- 2026-06-19: User requested Code Builder to decompose every Ariel skill like Ariel C using atomic effect object + modifier node + binding node, using lines 373-962 of the Ariel handoff report as the detailed node standard.

## Task: 2026-06-19 Ariel Effect Object Node Pilot

### Task title

Implement the first Ariel normalized-node pilot for numeric choice modifiers and Ariel C blessing composition.

### Goals

- Move Ariel numeric choice effects from wide `monster_skill_choices.csv` fields to reusable normalized nodes.
- Prove Ariel C can reduce pre-combined blessing rows by composing base effect rows with trait/passive nodes.
- Keep old effect rows only as compatibility rows when not yet replaced by the generic node path.

### Constraints

- Role Owner is Code Builder.
- User selected generic `monster_skill_nodes.csv` and `monster_skill_node_params.csv`; no new specialized effect tables were added.
- User answered D trait 5 condition is attacker-self shield, J requires Ariel-E-generated shield, I exposure damage taken applies to all incoming damage, passives are always active, and durations stay seconds.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- User verifies Ariel C base, trait2, trait3, H trait3, trait5, master1, and master2 combinations in Play Mode.
- Later pass should implement source-specific shield checks so Ariel J can require Ariel-E-generated shield instead of generic shield.
- Continue Ariel B/E/passive ownership cleanup only after Ariel C parity is confirmed.

### Evidence

- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_nodes.csv` now contains Ariel normalized choice nodes for damage, cooldown, magazine, reload, pierce, duration, shield-count damage, status-conditional damage-taken, and status modifier bonuses.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_node_params.csv` now carries the matching values; initial migration output reported `migrated=28 nodes=47 params=68`, and the final Ariel C trait2 targeted action-speed node addition brought the parsed param row count to 69.
- TextFieldParser CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=47 bad=`, `monster_skill_node_params.csv header=4 rows=69 bad=`, and `monster_skill_effects.csv header=70 rows=131 bad=`.
- `ariel-c-trait-2-blessing-action-speed` is a `StatusActionSpeedBonus` node with `status_id=blessing` and `bonus=0.06`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_effects.csv` keeps Ariel C base rows but disables 9 pre-combined rows as `MigratedToEffectBinding`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now applies normalized choice nodes during combat snapshot creation and resolves status-targeted action speed bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now applies snapshot status overrides through `SkillStatusSpecUtility.ResolveStatusData(...)`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged `InGame skill data validation passed with 0 warning(s).`

### History

- 2026-06-19: User requested Code Builder implementation of `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md` and provided answers to all ambiguous design questions.

## Task: 2026-06-19 Ariel Phase 2-5 Effect Object Cleanup

### Task title

Continue Ariel B/E/A/D/F-J normalized node, trigger-binding, and passive ownership cleanup after the Ariel C pilot.

### Goals

- Move Ariel B shield amount modifiers onto a shield-specific normalized node handler.
- Reduce Ariel E shield variants to one active shield effect plus shield amount nodes.
- Move Ariel J post-E action-speed behavior out of Ariel E effect rows into J-owned trigger/effect rows.
- Keep Ariel A/B/D trigger rows as explicit specialized trigger-binding compatibility rows.
- Add a source-specific effect condition so Ariel J shielded holy damage requires the Ariel E shield effect, not any shield.

### Constraints

- Role Owner is Code Builder, followed by one Code Reviewer pass requested by the user.
- The implementation stays on generic `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No MSW-MCP is used; Unity checks use Unity-MCP only.
- Unity Play Mode parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-checked, and Unity-MCP validated. Code Reviewer pass pending in the same user request.

### Next Actions

- Run the requested Code Reviewer pass against the current diff.
- User verifies Ariel B shield amount/duration, E shield trait/master combinations, J after-E action-speed, and J Ariel-E-shield-only holy damage in Play Mode.

### Evidence

- `SkillChoiceEffectSpec`, `SkillExecutionSnapshot`, `InGameSkillDefinitionMapper`, `SkillExecutionUtility`, and `SkillMultiEffectExecutor` now carry `ShieldAmountMultiplier`; active shield skill bodies and status-effect shield amounts can use shield-specific normalized choice nodes.
- `SkillEffectDefinition`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `StatusEffectRuntime`, and `SkillMultiEffectExecutor` now carry `condition_status_source_skill_id` for effect condition checks.
- `monster_skill_nodes.csv` contains four Ariel `ShieldAmountMultiplier` nodes: B trait1, B master1, E trait2, and E master2.
- `monster_skill_effects.csv` has exactly one active `ariel-e-shield*` row, while `ariel-e-shield-trait2`, `ariel-e-shield-master2`, and `ariel-e-shield-trait2-master2` are `MigratedToEffectBinding`.
- `monster_skill_effects.csv` no longer contains `ariel-e-passive-j-*`; the post-E action-speed effects now live as `ariel-j-after-e-action-speed` and `ariel-j-after-e-action-speed-trait1`.
- `monster_skill_triger.csv` contains J-owned `OnSkillCast` trigger rows for `event_skill_id=ariel-e`, including the trait1-gated trigger.
- `ariel-j-shielded-holy-damage` now has `condition_status_id=shield` and `condition_status_source_skill_id=ariel-e-shield-base`, matching the actual effect-created shield status source.
- CSV field-count check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Acceptance spot-check returned `eActiveShieldRows=1`, `eDisabledShieldVariants=3`, `shieldAmountNodes=4`, `oldEJRows=0`, `jTriggerRows=2`, and `jShieldSource=ariel-e-shield-base`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged catalog load and `InGame skill data validation passed with 0 warning(s)`, with 0 error/warning console entries.

### History

- 2026-06-19: User requested Code Builder to perform the remaining Phase 2-5 work from `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md`, then run Code Reviewer.

## Task: 2026-05-22 Ariel Final Shared Choice Runtime Completion

### Task title

Implement `ariel-a-trait-5` and `ariel-d-trait-5` through shared choice/status contracts and re-audit Ariel coverage.

### Goals

- Add a shared choice snapshot rule that counts shielded allies and converts the count into a per-cast damage multiplier.
- Add a shared status rule that increases incoming damage only when the attacker has a required status and the target carries the marked status.
- Confirm that no Ariel skill, choice, effect, or trigger row remains unsupported after this pass.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay reusable in shared runtime/data paths rather than adding Ariel-only execution branches.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and CSV-sync-verified.

### Next Actions

- User verifies in Play Mode that `ariel-a-trait-5` scales Ariel-A damage by `+6%` per currently shielded ally at cast time.
- User verifies in Play Mode that `ariel-d-trait-5` increases damage only when the attacker has `shield` and the target carries Ariel-D's `holy-exposure` mark.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:7` now marks `ariel-a-trait-5` as `RuntimeImplemented` with `count_status_id=shield`, `count_target_side=AllAllies`, and `damage_multiplier_per_count=0.06`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:28` now marks `ariel-d-trait-5` as `RuntimeImplemented` with `status_conditional_source_status_id=shield` and `status_conditional_damage_taken_bonus=0.1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:216-285` now resolves choices with roster context, counts matching status holders, and applies the dynamic damage multiplier to the cast snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:291-337`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:234-246`, `:366-374`, and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:965-1011` now carry source-conditional incoming-damage status data through status resolution and the live damage path.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1-2` now contains the current status payload schema columns, including `status_ailment_resistance_bonus` and `status_flat_element_resist_reduction`, so editor CSV sync matches the parser contract.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv | Where-Object { $_.monster_id -eq 'ariel' -and $_.implementation_state -notin @('RuntimeImplemented','ReferenceDirect') }`, the matching `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv` checks all returned no rows.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.
- Unity-MCP console after clear plus `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-22: User asked Code Builder to implement `ariel-a-trait-5` and `ariel-d-trait-5` and confirm whether every Ariel skill was now implemented.

## Task: 2026-06-07 Ariel Animation Clip Controller And Prefab Wiring

### Task title

Create Ariel's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Ariel's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Ariel_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Ariel_Unit.prefab` and connect `MonsterUnitActor.animationController`.

### Constraints

- Role Owner is Code Builder.
- The controller contract follows inspected `Rin_Animation_Cont.controller`.
- Unity Editor import and Play Mode animation verification were not available in this session.

### Role Owner

Code Builder

### Status

Implemented and locally YAML/build-verified.

### Next Actions

- User lets Unity import the new `.anim` and `.controller` assets.
- User verifies in Play Mode that Ariel plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/ariel/Animation/Animation_Ariel_Sprite` now contains 6 `Anim_Ariel_*.anim` files, 6 matching `.anim.meta` files, `Ariel_Animation_Cont.controller`, and `Ariel_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Ariel_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Ariel_Attack_1`, `Anim_Ariel_Attack_2`, `Anim_Ariel_Attack_3`, `Anim_Ariel_Hit`, `Anim_Ariel_Idle`, and `Anim_Ariel_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Ariel_Unit.prefab` now has `animationController: {fileID: 900100000000002}`, an `Animator` with controller GUID `b2339c033d324ea8a1f138797de25ab8`, and an `Animation_Controller` with `idleState: Anim_Ariel_Idle`, `deadState: Anim_Ariel_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Ariel controllerGuid=b2339c033d324ea8a1f138797de25ab8 linked=True`.
- The generated idle clip check returned `Ariel idleName=Anim_Ariel_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Ariel root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Ariel_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.
