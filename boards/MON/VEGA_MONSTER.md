## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/VEGA_MONSTER.md`.

# VEGA_MONSTER

## Scope

Vega dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Vega file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Vega active skills A-E are implemented and locally validated.
Vega passive skills F-J are now implemented on shared runtime/CSV paths and passed local build plus Unity CSV validation/sync on 2026-05-31.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-05-31 Vega F-J Passive Shared Runtime And CSV Implementation

### Task title

Implement Vega passive skills F-J on shared runtime contracts, then author the passive base/effect/trigger rows in the active CSV set.

### Goals

- Keep Vega F-J on reusable shared runtime paths instead of adding Vega-only combat branches.
- Implement the missing common-runtime surfaces identified by the earlier handoff: burst-index status bonus, source-status-gated passive aura, runtime-kind-filtered passive damage modifiers/triggers, and all-allies cooldown refund.
- Author the final Vega F-J passive base/effect/trigger rows in the active CSV authority and clear stale unsupported metadata.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Runtime authority stayed on the current shared Scripts2 combat/runtime path.
- CSV authority stayed on `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv` plus the already-active `monster_skills.csv`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, build-verified, and Unity CSV-validated/synced.

### Next Actions

- User verifies in Play Mode that `vega-f` trait 3 adds the extra `name-mark` stack only on Vega-A's final burst projectile.
- User verifies in Play Mode that `vega-h` ally buffs/debuffs follow live `slaughter-permit` uptime and stop immediately when the owner loses that status.
- User verifies in Play Mode that `vega-i` applies and consumes only `Area`-kind damage interactions and that `vega-j` refunds cooldown to all allied active skills on Vega-E kills.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains the Vega F-J passive completion rows:
  - `vega-h-base-duration` as `PassiveBase` at line 254.
  - `vega-f-trait-1` through `vega-j-trait-3` as `RuntimeImplemented` rows at lines 189-203.
  - `vega-f-trait-3` now authors the burst hook through `runtime_target_skill_ids=vega-a`, `burst_status_projectile_index=0`, and `burst_status_stacks_bonus=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes the passive effect rows that were absent during the earlier re-audit:
  - Vega-F rows `vega-f-name-mark-damage-base` through `vega-f-name-mark-resist-trait2` at lines 114-117.
  - Vega-G rows `vega-g-silence-damage-base`, `vega-g-silence-damage-trait1`, and `vega-g-silence-mark-crit-trait3` at lines 118-120.
  - Vega-H source-status-gated aura rows `vega-h-slaughter-action-base` through `vega-h-slaughter-mark-damage-trait3` at lines 122-125.
  - Vega-I triggered area-vulnerability rows `vega-d-i-area-vulnerability-base` through `vega-d-i-area-vulnerability-trait3-trait2` at lines 126-131.
  - Vega-J survive-target rows `vega-e-j-survive-target-base` and `vega-e-j-survive-target-trait2` at lines 132-133.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now includes the passive trigger rows that were absent during the earlier re-audit:
  - `vega-g-mark-on-hit-base` at line 46.
  - `vega-i-area-vulnerability-base` through `vega-i-area-vulnerability-trait3-trait2` at lines 47-53.
  - `vega-i-area-cooldown-base` at line 49 with `event_source_scope=all_allies`, `target_skill_id=vega-d`, and `event_skill_runtime_kinds=Area`.
  - `vega-j-cooldown-base`, `vega-j-cooldown-trait1`, `vega-j-survive-target-base`, `vega-j-survive-target-trait2`, and `vega-j-vega-d-cooldown-trait3` at lines 54-58.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now exposes shared `EventSkillRuntimeKinds`, `StatusConditionalIncomingSkillRuntimeKinds`, `StatusConditionalOutgoingSkillRuntimeKinds`, `HasBurstStatusProjectileIndex`, `BurstStatusProjectileIndex`, and `BurstStatusStacksBonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse/map/validate the Vega F-J shared fields including `required_source_status_id`, `event_skill_runtime_kinds`, the runtime-kind conditional status fields, and the burst-status choice fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now consumes `snapshot.ResolveBurstStatusStacksBonus(...)`, which is the shared runtime hook used by Vega-F trait 3.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now resolves conditional incoming/outgoing damage modifiers through `MatchesSkillRuntimeKinds(...)`, which is the shared `Area` damage filter used by Vega-I.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now checks `trigger.EventSkillRuntimeKinds`, can execute direct effect rows through `SkillMultiEffectExecutor.ExecuteDirect(...)`, and resolves cooldown/reload targets through `ResolveTargetRuntimes(...)`, including `TargetSide=AllAllies` for Vega-J.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` header/type rows were normalized to 70 columns so the newly added generic effect fields are accepted by the Unity CSV loader; after a forced Unity asset refresh, `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` both completed successfully.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after the final validation pass logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity console after the final sync pass logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Designer first re-audited Vega F-J and produced `boards/MON/VEGA_FJ_COMMON_RUNTIME_HANDOFF.md` because the then-inspected active CSV set had no passive base/effect/trigger authoring for F-J.
- 2026-05-31: User then explicitly requested Code Builder runtime implementation and Skill Builder row authoring from that handoff, the Vega reference markdown, and `boards/SkillBluePrint/passive-stat-blueprint.md`.
- 2026-05-31: Initial Unity validation failed with `CsvFatalException: CSV file 'monster_skill_effects.csv' row 114 has 70 columns but expected 66.` because the new generic effect fields had been added to authored rows without matching header/type-row normalization.
- 2026-05-31: Code Builder normalized the effect CSV header/type rows to 70 columns, forced a Unity asset refresh, and re-ran validation/sync successfully.

## Task: 2026-05-31 Vega F-J Passive Runtime Re-audit And Code Builder Handoff

### Task title

Re-audit whether Vega passive skills F-J and their enhancement rows are actually implementable on the current CSV/common-runtime surface, then prepare a Code Builder handoff for the missing work.

### Goals

- Separate metadata-only passive rows from real gameplay-supported passive runtime behavior.
- Identify which Vega F-J pieces are CSV-authorable today and which still need shared runtime additions.
- Produce a concrete Code Builder handoff markdown for the missing common-runtime work.

### Constraints

- Role Owner is Designer.
- Conclusions must stay grounded in inspected active CSV/runtime files only.
- Designer does not implement runtime code or CSV behavior rows.

### Role Owner

Designer

### Status

Handoff markdown created. Re-audit completed.

### Next Actions

- Code Builder should start from `boards/MON/VEGA_FJ_COMMON_RUNTIME_HANDOFF.md`.
- Code Builder should first decide whether to implement exact shared contracts or ask the user to approve approximations for the currently unsupported semantics.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` contains `vega-f` through `vega-j` as passive rows with `runtime_kind=Passive`, but that file alone does not create runtime behavior.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` contains Vega F-J `PassiveEnhancement` rows, but there are no Vega F-J `PassiveBase` rows in the active choice CSV.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` contains no `vega-f` through `vega-j` rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` contains no `vega-f` through `vega-j` rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` shows that `PassiveDefinition` behavior is built only from `PassiveBase`, `PassiveEnhancement`, and passive effect rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/InGamePassiveEffectRuntime.cs` shows that learned passives only execute runtime behavior when `PassiveEffects` exist.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` shows that passive base modifiers only apply when `BaseModifierChoices` exist.
- Follow-up recheck then confirmed that some previously flagged blockers already have reusable support in current code, including `condition_status_id` stack-threshold expressions in `StatusEffectRuntime.TryParseConditionStatusExpression(...)`, `status_duration_bonus_status_id` passive-base duration overrides, and the two-stage effect-application gate formed by `condition_status_id` plus `status_conditional_target_status_id`.
- `boards/MON/VEGA_FJ_COMMON_RUNTIME_HANDOFF.md` now records the detailed Designer handoff for Code Builder.

### History

- 2026-05-31: User asked for a Designer opinion on whether Vega F-J passives and their enhancements fit the current CSV/common-runtime surface and requested a Code Builder handoff file if shared runtime was still needed.
- 2026-05-31: Designer re-audited the active CSV/runtime paths and found that the current board-level claim that Vega F-J were already implemented was broader than the inspected active data/runtime evidence.
- 2026-05-31: User then requested a second-pass search for already-existing generic contracts before keeping anything on the “new common logic required” list, and the handoff was narrowed accordingly.

## Task: 2026-05-31 Vega-D Deployment Center Spawn Fix

### Task title

Fix Vega-D so each marked-target AoE slash spawns at the resolved target center instead of snapping back to Monster Vega's own position.

### Goals

- Preserve the overlapping local AoE fanout behavior authored for Vega D.
- Keep `hit_target_count=global` from forcing the prefab hitbox origin back to the caster when the skill is also using status-filtered deployments.
- Avoid any new common runtime feature beyond the minimal executor bug fix.

### Constraints

- Role Owner is Code Builder.
- The user explicitly requested immediate implementation and instructed Builder to stop only if new common logic became necessary; inspected current executor already had the needed deployment-center path.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies in Play Mode that Vega-D now appears at each marked target position instead of at Vega's own position.
- User verifies in Play Mode that the overlapping local AoE and `즉시 / +0.5s / +1.0s` repeat timing still behave as previously authored after the center-origin fix.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors `vega-d` with both `hit_target_count=global` and `deployment_required_target_status_id=name-mark`, so the bug had to be in the shared executor's hitbox-origin decision rather than in the active row bundle.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` still maps `hit_target_count=global` to `single.HitAllTargets=true` while also mapping `deployment_required_target_status_id` to `single.UsePrefabHitbox=true` and `single.UseMultiDeployment=true`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` previously routed any `HitAllTargets` prefab hitbox through `ResolvePrefabHitboxCenter(...)` back to the caster position; it now keeps the resolved deployment center when `UsesStatusFilteredDeployments(skill)` is true.
- The same executor still resolves one center per marked target and still applies prefab scaling on that center, so this fix changes spawn origin only and does not alter the authored overlap/repeat semantics.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed runtime catalog load plus sync. The console also still showed one non-blocking MCP bridge warning `Client handler error: Cannot access a disposed object.`

### History

- 2026-05-31: After the overlap/repeat re-authoring, the user observed that Vega-D was spawning on Vega's own position instead of each target center.
- 2026-05-31: Code Builder traced the bug to the shared `ResolvePrefabHitboxCenter(...)` branch that still treated all `HitAllTargets` skills like caster-anchored slashes and narrowed that branch to exclude status-filtered deployments.

## Task: 2026-05-31 Vega-D Overlapping Area Fanout Re-authoring

### Task title

Re-author Vega D back to overlapping local area hits per marked target and update master-1 to add two delayed extra slashes on the existing shared repeat path.

### Goals

- Keep Vega D on the shared `SingleAttack` status-filtered fanout path without adding a new runtime branch.
- Let each marked-target deployment center hit all enemies in its local radius so overlaps can stack.
- Author Vega-D master-1 as base hit plus two extra delayed repeats at `0.5s` and `1.0s`.

### Constraints

- Role Owner is Code Builder.
- The user explicitly required work to stop if a new common runtime was needed; inspected current runtime already supported local multi-hit count, prefab radius scaling, and delayed per-target repeats.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies in Play Mode that each marked-target slash now damages every enemy inside the local radius and that overlapped circles stack damage.
- User verifies in Play Mode that Vega-D master-1 now lands at `즉시 / +0.5s / +1.0s` per marked-target center and that each hit uses the authored `-35%` power adjustment.
- User verifies in Play Mode that Vega-D master-2 still enlarges the live slash prefab together with the effective hit radius.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-d` with `hit_target_count=global` while keeping `runtime_kind=SingleAttack`, `radius=1.25`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_D.prefab`, `deployment_required_target_status_id=name-mark`, and `deployment_required_target_status_min_stacks=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-d-master-1` with description `각 표식 대상 위치에 범위 참격 2회 추가 발생, 각 참격 위력 -35%`, `damage_multiplier=0.65`, `repeat_count_per_target=2`, `repeat_interval_seconds=0.5`, and `repeat_damage_multiplier=1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` already keeps status-filtered fanout centers through `ResolveDeploymentCenters(...)`, resolves unlimited local hits when `HitAllTargets` is authored through `ResolveEffectiveHitTargetCount(...)`, and schedules per-target repeats with `delaySeconds = snapshot.RepeatIntervalSeconds * repeatIndex`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` still uses `SkillExecutionUtility.ApplyPrefabScale(...)` for the current Vega-D status-filtered fanout path instead of the stretched line visual branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillAreaUtility.cs` still maps radius modifiers into both effective radius and prefab scale through `ResolveRadius(...)` and `ResolvePrefabScaleFactor(...)`, so Vega-D master-2 radius growth remains data-driven.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: An earlier same-day Vega-D pass temporarily re-authored the row toward single-target local hits to remove unintended overlap behavior.
- 2026-05-31: User then explicitly requested overlapping local area damage plus base hit and two delayed extra slashes, so Code Builder re-authored the active Vega-D rows on the already-inspected shared runtime path without adding new common logic.

## Task: 2026-05-31 Vega-D Marked-Target Fanout Single-Target Fix

### Task title

Keep Vega D on marked-target fanout while restoring per-target single-hit behavior and removing the unintended beam-like prefab stretch.

### Goals

- Preserve the shared `SingleAttack` resolved-deployment path that fires once per enemy carrying `name-mark`.
- Stop status-filtered fanout casts from inheriting the line-style multi-deployment visual scaling.
- Restore authored single-target hit count per deployment instead of unlimited local hits.

### Constraints

- Role Owner is Code Builder.
- The user explicitly required that work stop only if firing separately at every marked enemy needed new shared common logic; inspected runtime already had that shared deployment path, so this task stayed inside the existing executor.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified.

### Next Actions

- User verifies in Play Mode that Vega D now spawns one slash per `name-mark` target without the stretched beam presentation.
- User verifies in Play Mode that each slash damages only the intended marked target instead of also clipping nearby enemies around the deployment center.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors `vega-d` as `runtime_kind=SingleAttack` with `deployment_required_target_status_id=name-mark`, `deployment_required_target_status_min_stacks=1`, `radius=1.25`, and empty `hit_target_count`, so the active row still requests one deployment per marked enemy rather than a separate runtime kind.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` still maps any `DeploymentRequiredTargetStatusId` row to `UsePrefabHitbox=true` and `UseMultiDeployment=true`, which is why the fix had to stay inside the shared `SingleAttackSkillExecutor` behavior split rather than in CSV alone.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now distinguishes status-filtered deployments from line-style multi-deployment visuals through `UsesStatusFilteredDeployments(...)`, `UsesLineStyleMultiDeploymentVisual(...)`, and `ResolveEffectiveHitTargetCount(...)`.
- In that executor, status-filtered fanout casts now keep the shared resolved-deployment center logic but no longer call `ConfigureMultiDeploymentPrefabVisual(...)`; they instead follow the normal prefab scaling path and use the authored hit-target count floor of `1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity console after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: User reported that Vega D should stay `SingleAttack` and fire at every marked enemy, but the current effect looked like a beam and the per-cast damage was not behaving as single-target.
- 2026-05-31: Code Builder verified that the beam-like look came from `ConfigureMultiDeploymentPrefabVisual(...)` and that unlimited local hits came from `effectiveHitTargetCount = int.MaxValue` on the generic `UseMultiDeployment` path, then split the status-filtered fanout behavior from the line-style multi-deployment branch.

## Task: 2026-05-31 Vega-E Shared Runtime Implementation And CSV Authoring

### Task title

Implement the shared runtime extensions and active CSV rows required to bring Vega E onto the current common `SingleAttack` path.

### Goals

- Keep Vega E on shared runtime rather than adding a Vega-only executor branch.
- Support marked-target selection, mark-stack-based extra damage, and partial mark consumption through reusable runtime/data contracts.
- Author the active Vega E CSV rows on those shared fields and keep unsupported data explicit where reference authority is still missing.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Implementation authority started from `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md`, then the final row-authoring pass used `boards/SkillBluePrint/single-attack-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/e-final-sentence.md`, and the routed active CSV files.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, build-verified, and Unity CSV-validated. Vega-E trait-5 is now fully authored on the shared redistribution path with user-provided search radius `100` and target count `1`.

### Next Actions

- User verifies in Play Mode that Vega E now targets the enemy with the highest `name-mark` stack count and refuses to cast only when no marked target exists.
- User verifies in Play Mode that base damage, per-stack bonus damage, and consumed-mark amount match the authored Vega E values across trait/master combinations.
- User verifies in Play Mode that trait-5 kill redistribution sends `25%` of consumed `name-mark` to one nearby enemy using search radius `100`.

### Evidence

- `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` was used as the implementation contract for the shared Vega E runtime work.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs` and `.../SkillExecutionUtility.cs` now support `HighestStacks` targeting keyed by `target_selection_status_id` plus a minimum required stack count.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs`, `SkillDefinition.cs`, `InGameSkillDefinitionMapper.cs`, `SkillExecutionSnapshot.cs`, and the `PakuriCsvRuntimeData.*` CSV runtime files now carry shared target-status-stack damage, target-status consumption, conditional crit, and consumed-status redistribution fields.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now expose shared partial status-stack consumption helpers.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves shared target-status-stack bonus damage, consumes target stacks on hit, and can redistribute a portion of consumed stacks on kill.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-e` with `target_selection=HighestStacks`, `target_selection_status_id=name-mark`, `target_selection_status_min_stacks=1`, `target_status_stack_status_id=name-mark`, `target_status_stack_base_damage=6`, `target_status_stack_attack_power_coefficient=0.18`, `consume_target_status_id=name-mark`, `consume_target_status_ratio=0.5`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `vega-e-trait-1`, `trait-2`, `trait-3`, `trait-4`, `trait-5`, `master-1`, and `master-2` as shared-runtime-backed rows; `vega-e-trait-5` now authors `redistribute_consumed_status_ratio_on_kill=0.25`, `redistribute_consumed_status_id=name-mark`, `redistribute_consumed_status_search_radius=100`, and `redistribute_consumed_status_target_count=1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- The current redistribution split behavior inside `SingleAttackSkillExecutor.cs` only matters when the authored target count exceeds `1`; current Vega-E trait-5 authors target count `1`, so no multi-target split inference is exercised for this skill row.

### History

- 2026-05-30: Designer produced `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` after confirming Vega E still needed shared targeting/consumption/scaling extensions.
- 2026-05-31: Code Builder implemented the shared runtime fields and re-authored the active Vega E rows on that path.
- 2026-05-31: First Unity CSV validation exposed a temporary Vega E row-shape regression in `monster_skill_choices.csv`; Builder corrected the row alignment and revalidated successfully.
- 2026-05-31: User later provided trait-5 nearby-search authority (`radius 100`, `target count 1`) plus the final prefab path `Assets/Prefab/Skill/Vega/Vega_E.prefab`, and Skill Builder completed the remaining row authoring.

## Task: 2026-05-30 Vega-E Common Runtime Code Builder Handoff

### Task title

Prepare a Designer handoff for the remaining shared-runtime work needed to fully support Vega E on current CSV/runtime authority.

### Goals

- Separate Vega E behaviors that already fit current shared runtime from behaviors that still need shared extension.
- Hand off the minimum shared runtime surface needed for Vega E without proposing Vega-only hardcoded branches.
- Give Code Builder a concrete implementation order and acceptance target.

### Constraints

- Role Owner is Designer.
- Designer does not implement code or scene changes.
- Conclusions must stay grounded in inspected current code and active CSV rows only.

### Role Owner

Designer

### Status

Handoff markdown created. Implementation not started.

### Next Actions

- If user requests implementation, Code Builder should use `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` as the starting contract.
- Code Builder should first re-audit which Vega E rows can move to shared runtime by CSV re-authoring alone before extending code.

### Evidence

- `boards/MON/VEGA_E_COMMON_RUNTIME_HANDOFF.md` now records the Code Builder handoff for Vega E.
- `monster_skills.csv` currently authors `vega-e` as shared `SingleAttack`, but still with `target_selection=Nearest`.
- `monster_skill_choices.csv` currently marks all `vega-e-*` choice rows `DataOnlyUnsupported`.
- Inspected shared runtime already supports generic damage multiplier, cooldown multiplier, and kill cooldown refund paths, so not every Vega E row necessarily needs new code.
- Inspected shared runtime did not show a current generic contract for highest-mark target selection, mark-stack-based damage scaling, consumed-mark tracking, or consumed-mark redistribution.

### History

- 2026-05-30: User asked whether Vega E could be implemented on current common logic and CSV.
- 2026-05-30: Designer concluded Vega E base cast already routes through shared `SingleAttack`, but full intended behavior still needs shared targeting/consumption/scaling extensions.
- 2026-05-30: User then requested a Code Builder handoff markdown for Vega E.

## Task: 2026-05-30 Vega-C And Vega-D Shared Runtime Implementation And Skill Authoring

### Task title

Implement the shared runtime extensions and active CSV rows required to bring Vega C and Vega D onto reusable common paths.

### Goals

- Keep Vega C on shared `Buff` while adding reusable buff-active modifier support and attached-buff scalar choice overrides.
- Move Vega D from the mismatched `AreaAttack` row shape to shared `SingleAttack` marked-target fanout.
- Finish the routed active CSV authoring for Vega C and Vega D, including the user-provided prefab paths.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Shared runtime extensions were allowed because the user explicitly asked for Code Builder implementation from the handoff.
- User explicitly clarified that Vega D is `SingleAttack`-style repeated slashes at target positions, not a zone-style area attack.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, build-verified, and Unity CSV-validated.

### Next Actions

- User verifies in Play Mode that Vega C buff-active bonuses affect the intended follow-up skills only while `slaughter-permit` is active.
- User verifies in Play Mode that Vega D casts one slash per marked enemy position, allows overlap, and repeats one extra slash per marked target when master-1 is learned.

### Evidence

- `boards/MON/VEGA_CD_COMMON_RUNTIME_HANDOFF.md` was used as the explicit implementation contract for the shared Vega C/D extension work.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now exposes shared choice/runtime fields for `RuntimeTargetSkillIds`, attached buff action-speed / attack-power overrides, `RequiredSourceStatusId`, repeat-per-target fields, and `DeploymentRequiredTargetStatusId`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now applies choice rows only when the source status requirement is met and accepts delimited `RuntimeTargetSkillIds`, so Vega C buff-active modifiers stay on shared runtime routing instead of Vega-only branches.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillStatusSpecUtility.cs` now clones attached status data with snapshot-provided action-speed and attack-power overrides, which lets Vega C trait-2, trait-3, and master-2 modify the attached `slaughter-permit` buff through shared logic.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now treats `DeploymentRequiredTargetStatusId` as a shared resolved-deployment path and schedules shared repeat deployments per center, so Vega D stays on `SingleAttack` while fanning out across marked targets.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillExecutionUtility.cs` and `.../SkillTargetingUtility.cs` now expose shared ordered-target resolution filtered by required target status and minimum stacks.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-c` with `Assets/Prefab/Skill/Vega/Vega_C.prefab`, and authors `vega-d` as `runtime_kind=SingleAttack`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_D.prefab`, `deployment_required_target_status_id=name-mark`, and `deployment_required_target_status_min_stacks=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now re-authors Vega C rows through shared `status_action_speed_bonus`, `status_attack_power_bonus`, `runtime_target_skill_ids`, and `required_source_status_id` fields, and re-authors Vega D trait-4 / trait-5 / master-1 through shared conditional-damage, status-set, and repeat-per-target fields.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity console also logged one MCP bridge warning `Client handler error: Cannot access a disposed object.`, but no new Vega CSV parse failure or C# compile failure appeared in the inspected console output.

### History

- 2026-05-30: User asked Designer whether Vega C and Vega D could be implemented from the current repository state and then requested an English markdown handoff for Code Builder.
- 2026-05-30: User clarified that Vega D should be treated as `SingleAttack` semantics rather than area-zone semantics; Designer reflected that correction in the handoff.
- 2026-05-30: User then explicitly requested Code Builder implementation from `boards/MON/VEGA_CD_COMMON_RUNTIME_HANDOFF.md` followed by Skill Builder authoring for Vega C and Vega D with the provided prefab paths.

## Task: 2026-05-28 Vega-B Master-1 Follow-up Returned To LineAttack

### Task title

Convert the Vega-B master-1 delayed second slash from the shared triggered `SingleAttack` path to the shared triggered `LineAttack` path so it matches the aimed slash behavior of the Vega-B base skill.

### Goals

- Make the delayed second slash rotate and travel on the same shared line-attack presentation path as base `vega-b`.
- Keep the authored `0.4s` delay, `45%` scaled damage, prefab path, and linked `1s` silence effect.
- Preserve CSV validation and runtime-catalog sync after the trigger-path change.

### Constraints

- Role Owner is Code Builder.
- This change stays on the existing `vega-b-master1-second-slash` trigger row plus the shared trigger runtime; no hidden helper skill row was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that Vega-B master-1 second slash now aims like base `vega-b` instead of appearing as the older self-centered `SingleAttack` follow-up.
- If design later requires the delayed slash to lock to the exact original cast target/path instead of re-resolving nearest target at `0.4s`, that would need a separate trigger-context extension.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `runtime_kind=LineAttack`, `trigger_action=LineAttack`, `target_selection=Nearest`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`, and linked effect `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors base `vega-b` as `runtime_kind=LineAttack`, so the base and follow-up now share the same runtime kind and prefab path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now includes an explicit `SkillTriggerActionKind.LineAttack` branch and `ExecuteLineAttack(...)` shared trigger path for direct delayed line slashes.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` now resolves linked OnHit status effects through the passed `SkillExecutionSnapshot`, so the triggered line path keeps source-skill choice-gated status rules instead of losing them on the beam actor path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-28: After base `vega-b` was returned to `LineAttack`, the user reported that the master-1 delayed second slash still looked like the older `SingleAttack` follow-up and requested the same aimed slash path for the follow-up hit.

## Task: 2026-05-28 Vega-B Base Skill Returned To LineAttack

### Task title

Return Vega-B base skill to the shared `LineAttack` path so the slash aims toward the target instead of spawning as a self-centered `SingleAttack`.

### Goals

- Fix the current “cast on self” visual feel reported by the user.
- Keep Vega-B using the shared beam/line actor rotation path like other straight aimed slashes.
- Preserve base damage, silence payload, cooldown, width, and prefab path.

### Constraints

- Role Owner is Code Builder.
- This change is limited to the active Vega-B base skill row and runtime-catalog sync.
- The existing master-1 delayed second slash trigger row remains on the shared triggered `SingleAttack` path for now.

### Role Owner

Code Builder

### Status

Implemented and Unity CSV-validated.

### Next Actions

- User verifies in Play Mode that base Vega-B now rotates toward the current target like a straight aimed line attack.
- If master-1 must also rotate on the same path, that follow-up still needs a separate shared trigger-beam design decision.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b runtime_kind=LineAttack`, keeps `radius=1.8`, `cooldown_seconds=8`, `status_effect_id=silence`, and keeps `Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/BeamSkillExecutor.cs` resolves target direction from nearest target and spawns the prefab with `ResolveRotation(direction)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` rotates the live line actor from `lineDirection`, which is why the LineAttack path matches the user-requested aimed slash behavior.
- Unity menu `Pakuri/Validate CSV Source Data` completed and the console logged the runtime catalog load summary without new Vega-B CSV errors.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` completed and the console logged sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-28: After the earlier SingleAttack contact implementation, the user reported that Vega-B still looked like a self-cast slash even though damage landed; targeted inspection confirmed the visual issue was caused by the SingleAttack prefab spawn path using identity rotation.

## Task: 2026-05-28 Vega-B Follow-up Trigger Payload Correction

### Task title

Fix the authored Vega-B master-1 follow-up trigger row so CSV validation passes and the second slash deals the intended scaled damage.

### Goals

- Remove the current Vega-B source CSV validation failure.
- Keep the second slash at the intended `45%` scaling while giving the trigger row a real damage payload.
- Preserve the existing shared triggered `SingleAttack` plus linked OnHit silence path.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The correction stays inside the existing Vega-B row bundle and shared validator; no hidden helper skill row was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that the second slash now deals the scaled damage as expected, not just the linked `1s` silence.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `base_damage=30`, `attack_power_coefficient=1.4`, and `damage_multiplier=0.45`.
- Unity menu `Pakuri/Validate CSV Source Data` completed after the correction, and the console logged the runtime catalog load summary instead of the previous Vega-B trigger validation failure.

### History

- 2026-05-28: The first authored row kept only `damage_multiplier=0.45` and zeroed the real payload fields, which was both validator-invalid and runtime-zero-damage.

## Task: 2026-05-28 Vega-B Triggered Second Slash And Silence Authoring

### Task title

Author Vega-B on the shared SingleAttack path and extend triggered SingleAttack so the delayed second slash can carry OnHit silence.

### Goals

- Keep Vega-B on `SingleAttack` with the user-provided `Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- Implement base silence, trait-2 silence duration bonus, trait-5 Name Mark application, master-1 delayed second slash, and master-2 10-stack silence extension.
- Avoid a Vega-only helper runtime or hidden extra active-skill slot.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Authority stayed on `boards/SkillBluePrint/single-attack-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md`, routed active CSV files, and the user-provided prefab path.
- The shared runtime/common-logic extension was user-approved before implementation.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Vega-B now emits the slash from the caster position, damages each enemy on the path once, and applies base `3s` silence.
- User verifies that trait-2 extends Vega-B silence by `+1s`, trait-5 adds `name-mark` `+2` on hit, master-1 fires the delayed `0.4s` second slash with `45%` damage and `1s` silence, and master-2 refreshes silence by `+1s` at `name-mark>=10`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b` as `SingleAttack` with `hit_target_count=global`, `status_effect_id=silence`, `status_duration_seconds=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `vega-b-trait-2` and `vega-b-master-2` `RuntimeImplemented` through shared silence-duration and threshold-status fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `vega-b-trait5-name-mark` and `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `vega-b-master1-second-slash`, which routes a delayed `SingleAttack` slash at `0.4s`, `damage_multiplier=0.45`, and links `vega-b-master1-second-silence`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now lets triggered `SingleAttack` hits carry shared `OnHit` status effects with the source-skill active-choice snapshot, so Vega-B master-1 reuses shared status gating and silence-duration bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now anchors `HitAllTargets` prefab hitboxes at the caster position, which matches the Vega-B slash-path prefab behavior instead of centering the hitbox on the target group.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.

### History

- 2026-05-28: Initial inspection confirmed Vega-B was already authored as `SingleAttack`, so the work stayed on the shared SingleAttack blueprint path instead of the beam blueprint.
- 2026-05-28: The user considered a hidden follow-up skill row for the second slash, but current active-slot validation and learned-runtime loading made that path larger than a small shared triggered-SingleAttack extension.

## Task: 2026-05-28 Vega-A Shared Projectile Runtime Extension And Skill Authoring

### Task title

Extend the shared projectile runtime for Vega-A burst timing, per-burst damage rules, and follow-up shadow shots, then author the active Vega-A data on that path.

### Goals

- Keep Vega-A on the projectile blueprint path instead of adding a Vega-only runtime.
- Author the inspected reference values for 3-hit burst timing, third-hit bonus, Name Mark application, trait-4 last-hit bonus, trait-5 conditional damage, and master-1 shadow follow-up.
- Keep master-2 grounded on the user-provided slash coefficient and prefab path without adding a Vega-only runtime.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Authority stayed on `boards/SkillBluePrint/projectile-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md`, the routed active CSV files, and the user-provided prefab path `Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- The base reference did not provide a numeric slash-damage value for master-2, but the user later provided `attack coefficient 0.5` and `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` as explicit authority.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, compile-verified, and Unity editor-validated.

### Next Actions

- User verifies in Play Mode that Vega-A fires 3-hit bursts with `0.12s` internal spacing and `0.55s` outer cadence.
- User verifies that trait-4 boosts only the last burst hit, trait-5 boosts only targets with at least 10 `name-mark` stacks, and master-1 spawns one next-frame shadow projectile at `45%` damage.
- User verifies in Play Mode that master-2 kill triggers now deal the small slash through the shared triggered-effect path and use `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md` specifies 3 bullets, `3번째 탄환 200%`, shot interval `0.55`, bullet interval `0.12`, hit-applied `name-mark` 1 stack, trait-4 last-hit `+50%`, trait-5 `+25%` vs `name-mark` 10+, and master-1 shadow projectile `45%`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-a` with `shot_interval_seconds=0.55`, `burst_interval_seconds=0.12`, `projectile_burst_count=3`, `burst_damage_projectile_index=3`, `burst_damage_multiplier=2`, `status_effect_id=name-mark`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-a-trait-4` through the shared last-burst-hit multiplier path, `vega-a-trait-5` through the shared conditional target-status multiplier path, and `vega-a-master-1` through the shared follow-up projectile path.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors `vega-a-master2-transfer-mark` as a shared `Damage` effect with `attack_power_coefficient=0.5`, `status_stack_amount=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` and `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now mark Vega-A master-2 `RuntimeImplemented` on the existing nearest-enemy OnKill trigger/effect path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs`, `.../Execution/Runtime/SkillExecutionSnapshot.cs`, and `.../Execution/Executors/ProjectileSkillExecutor.cs` now carry separate burst interval, burst-index damage rules, and follow-up projectile execution on the shared projectile runtime path.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now accepts shared `Damage` effect rows with positive `attack_power_coefficient` or `spell_power_coefficient` even when `base_damage=0`, matching the actual runtime formula used by `SkillExecutionUtility.ResolveDamage(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity refresh completed to `idle`, and the filtered Unity console returned no CSV/runtime errors after the trigger-row contract fix.

### History

- 2026-05-28: User first challenged whether burst-internal spacing already existed from Sein-B; re-inspection confirmed the existing shared burst path and narrowed the required extensions to shared burst-index damage rules and shared follow-up projectile support.
- 2026-05-28: The new Vega master-2 trigger row initially failed CSV parsing because `monster_skill_triger.csv` requires a non-empty `triggered_skill_id`; the row was corrected and Unity validation then completed without further errors.
- 2026-05-28: User later provided the missing master-2 slash authority as `attack coefficient 0.5` plus `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`, which completed the existing trigger/effect implementation path without further shared code changes.
- 2026-05-28: Unity source validation then exposed a shared mismatch: coeff-only `Damage` effect rows were runtime-valid but validator-invalid. Builder fixed the shared validator so Vega-A master-2 and future coeff-only damage effects no longer require fake positive `base_damage`.

## Task: 2026-05-18 Vega-B SingleAttack Runtime Kind

### Task title

Route Vega-B through the new SingleAttack runtime kind for one-shot area damage.

### Goals

- Move Vega-B out of `LineAttack` because the requested CSV row belongs to one-shot `SingleAttack`.
- Preserve existing CSV-authored damage, coefficient, radius, and cooldown.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Vega-B now behaves as a one-shot area hit in the current shared executor path.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md` names Vega-B `移⑤У????쒕룄`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `vega-b runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV row 34 as a one-shot area attack skill for the new `SingleAttack` type.
