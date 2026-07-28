## Archived History

- Non-July task blocks from `boards\MON\ARIEL_MONSTER.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

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

- Role Owner is Designer / Code Builder.
- Ariel graph storage/runtime compatibility 구현은 완료했으며 사용자 Play Mode A-J 회귀 검증은 아직 남아 있다.
- Ariel legacy effects are already zero, but the shared effects directory remains because it contains 96 Eve/Rin/Sein/Vega rows.
- Existing Ariel trigger wide payload conversion is outside the first storage-compatibility migration except for two Effect graph references.
- No MSW-MCP was used.

### Role Owner

Designer / Code Builder

### Status

Code Builder implementation and Unity CSV validation completed; user Play Mode regression pending.

### Next Actions

- 사용자 Play Mode에서 Ariel A-J 기본/특성/마스터 조합을 회귀 검증한다.
- 다른 몬스터 전환 전에는 Rin/Vega legacy node와 shared legacy effects 경로를 유지한다.

### Evidence

- Created `boards/MON/ARIEL_SKILL_GRAPH_NODES_MIGRATION_PLAN.md`.
- Ariel currently has 50 Choice rows, 39 Choice-owned nodes, and 85 Effect-owned nodes grouped into 20 Effects.
- Choice-gated Ariel Effect groups account for 45 nodes across 11 graphs; no-Choice Effect groups account for 40 nodes across 9 graphs.
- Target ownership is 39 Choice/Plan rows, 45 Choice/Effect rows, 36 Skill/Effect rows, and 4 Trigger/Effect rows.
- `ariel-a-master-2-holy-exposure-on-hit` becomes `Choice / ariel-a-master-2 / Effect / 0` and uses the real choice id as owner identity.
- Three Ariel references to old Effect owner strings must move with graph identity: two trigger effect references and one condition `source_skill_id` reference.
- 구현 결과는 definition 32종/param contract 53행, Ariel graph row 124행, Effect graph 20개이며 분포는 Choice/Plan 39, Choice/Effect 45, Skill/Effect 36, Trigger/Effect 4다.
- Ariel legacy node/param은 0행이 되었고 Rin/Vega legacy node 15행/param 33행 및 legacy effects 96행은 유지되었다.
- `PakuriCsvRuntimeData` loader가 graph CSV를 기존 `SkillNodeRow`/`SkillNodeParamRow`로 materialize하여 기존 mapper/executor 계약을 재사용하며, trigger graph reference는 build 시 생성 EffectId로 해석한다.
- `dotnet build Assembly-CSharp.csproj`와 `Assembly-CSharp-Editor.csproj`는 각각 오류 0건이었다.
- Unity-MCP에서 `Pakuri/Sync CSV Runtime Catalog Assets` 후 `Pakuri/Validate CSV Source Data`를 실행했고, 리소스 카탈로그에서 몬스터 5종·스테이지 적 8+8종을 로드했으며 콘솔 오류는 0건이었다.
- 후속 graph schema 정리로 네 `skill_graph_nodes_*.csv`에서 `requires_active_choice_id`, 두 passive gate, `runtime_support_state`, `runtime_support_notes`를 제거해 각 파일을 21열로 축소했다.
- `excludes_active_choice_id`는 `Skill/ariel-c/Effect/0`의 `ariel-c-master-1` 제외 조건 1건만 보존했으며, Choice 소유 Effect의 required-choice 조건은 loader가 owner id에서 계속 생성한다.
- 후속 정리 뒤 Runtime/Editor 빌드는 오류 0건이었고 Unity-MCP catalog sync/source validation도 몬스터 5종·스테이지 적 8+8종 로드로 재통과했다.

### History

- 2026-07-11: User requested an implementation-design MD for first migrating Ariel to definition-only nodes plus value-owning `skill_graph_nodes`, while retaining legacy effects until all monsters finish migration.
- 2026-07-11: Code Builder implemented the Ariel-only graph migration, preserved legacy compatibility paths, and completed compile plus Unity catalog validation. User Play Mode A-J regression remains pending.
- 2026-07-11: User requested a slimmer graph schema. Code Builder removed five redundant graph columns while preserving the single Ariel C master exclusion gate, then revalidated builds and the Unity runtime catalog.

## Task: 2026-07-11 Ariel Node Decomposition Guide

### Task title

Maintain the current Ariel skill-graph conversion guide for decomposing legacy effect rows into graph nodes.

### Goals

- Use the current 21-column `skill_graph_nodes` schema as the authoring authority.
- Document `node_type_id + param_order -> arg_N` mapping, graph ownership, generated IDs, and trigger references.
- Give an AI agent a repeatable method for separating base skill bodies, Choice Plan graphs, Effect graphs, and Trigger bindings.
- Record current graph/direct-node incompatibility and stop when existing graph definitions cannot preserve a legacy effect field.

### Constraints

- Role Owner is Designer.
- This task creates design/documentation only; no skill CSV or runtime code implementation was performed.
- Evidence is limited to current runtime skill CSVs, relevant runtime code, and routed Ariel/DATA boards.
- No monster reference markdown, archive, UI, RUN, or unrelated monster board was inspected for value discovery.
- No MSW-MCP was used.
- Unity Play Mode verification remains user-owned for later implementation work.

### Role Owner

Designer

### Status

Guide rewritten for the current `skill_graph_nodes` schema and loader behavior.

### Next Actions

- Code Builder or Skill Builder selects one monster, routes the minimum active CSV set, and audits every non-empty legacy field before migration.
- Eve/Sein have no direct-node overlap and can begin only in a kind whose graph file already exists; Rin/Vega must first resolve their 15 remaining legacy direct nodes.
- Stop for user approval when a graph file, node type, param, CSV column, or shared runtime composer/mapper extension is required.

### Evidence

- Rewrote `boards/MON/ARIEL_NODE_DECOMPOSITION_GUIDE.md` around the current graph instance files under `choices/{kind}/skill_graph_nodes_{kind}.csv` instead of direct node/param authoring.
- Current disk aggregation returned 124 Ariel graph rows: Choice/Plan 39 rows in 36 graphs, Choice/Effect 45 rows in 11 graphs, Skill/Effect 36 rows in 8 graphs, and Trigger/Effect 4 rows in 1 graph.
- All 20 current Ariel Effect graphs contain exactly one operation node.
- Current graph files exist for buff, passive, projectile, and single-attack only; area-attack and line-attack graph files do not exist.
- `skill_node_definitions.csv` has 32 node types and `skill_node_definition_params.csv` has 53 param rows; the maximum defined arg is `EffectTarget.arg_8`, while current Ariel data uses at most `arg_5`.
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` rejects graph/direct-node mixing for one monster, rejects undefined/required arg violations, generates node/effect IDs, and resolves Choice/Skill/Trigger owners.
- Current legacy compatibility data remains 15 direct nodes with 33 params for Rin/Vega and 96 effect rows for Eve/Rin/Sein/Vega.
- The obsolete 58/16/22 effect classification and the statement that Trigger-owned nodes are unsupported were removed; current Ariel has one Trigger/Effect graph and two trigger graph-reference rows.

### History

- 2026-07-11: User requested an evidence-based MD that lets an AI agent use Ariel's current node implementation to decompose other monsters' effect CSV behavior, reuse existing nodes first, and add handlers only when necessary.
- 2026-07-11: User requested updating the guide after the CSV structure changed; Designer rewrote it for current graph rows, positional args, generated IDs, trigger references, compatibility gates, and stop conditions.

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

- Apply the Ariel F-J passive node conversion and EffectTarget cleanup to the passive runtime node params.
- Keep Ariel passive effects decomposed into functional nodes rather than copied old effect-row target defaults.
- Preserve all gameplay-carrying params for F-J passives and traits.

### Constraints

- Role Owner is Code Builder.
- Current files are skill-kind paths under `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes`.
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

- `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/passive/passive_skill_node_params.csv` no longer has Ariel F-J passive rows with `target_selection`, `target_shape`, `center_mode`, or `visual_anchor_mode`.
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
- Current files are skill-kind paths under `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes`.
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
- Created `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/ariel/node/` with `ariel_buff_skill_nodes.csv`, `ariel_passive_skill_nodes.csv`, `ariel_projectile_skill_nodes.csv`, and `ariel_single_attack_skill_nodes.csv`.
- Created `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/ariel/nodes_param/` with matching `ariel_*_skill_node_params.csv` files.
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
- `Pakuri/Assets/CSVdata/authoring/monster/skills/effects/ariel/ariel_skill_effects.csv` and its `.meta` were deleted.
- `PakuriCsvRuntimeSourceCatalog.asset` no longer references deleted GUID `de95dfd09fa14fd5bffaf64855a35d25`; post-delete search returned no matches.
- `git diff --check` returned `DIFF_CHECK_OK`; Git also printed existing line-ending normalization warnings.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-07-08: User confirmed all `holy-exposure` instances should share Holy damage +15%, then requested Code Builder to migrate the remaining Ariel effect CSV content to nodes and delete `ariel_skill_effects.csv` only after confirming full migration.
- 2026-07-08: User pointed out migrated effect node params still contained meaningless defaults like the original `ariel-b-shielded-holy-trait5-base` expansion; Code Builder first compacted defaults, then replaced that insufficient approach after the user clarified the intended node-composition structure.
- 2026-07-08: User clarified the previous conversion was still wrong because it copied deleted effect CSV rows into `*-base` nodes instead of implementing semantic nodes. Code Builder rebuilt Ariel effect-owned nodes without `*-base` operation nodes and split targeting, visuals, conditions, lifetime, and modifiers into separate handlers.
- 2026-07-08: User clarified that merely removing the `-base` suffix was still insufficient because `EffectStatus(status_id=passive-buff|shield|blessing)` rows were carrier rows. Code Builder replaced those carriers with semantic `ApplyStatus`, `ApplyShield`, and `StatusModifier` operations and removed `passive-buff` from Ariel node params.
