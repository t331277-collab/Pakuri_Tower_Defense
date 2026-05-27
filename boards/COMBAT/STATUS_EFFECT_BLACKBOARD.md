## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/STATUS_EFFECT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/status history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current shared status runtime baseline and the resource-display rule still relevant to active work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-05-27 Sein Projectile/Zone Conditional Status Additions

### Task title

Add shared runtime status identities for Sein-C trait-5 and Sein-D trait-5 conditional damage logic.

### Goals

- Keep Sein-C trait-5 on a shared conditional-target-status path instead of a hardcoded target-memory branch.
- Keep Sein-D trait-5 on a shared status-stack threshold path driven by the zone hit runtime.
- Route both statuses through the existing `StatusEffectKind` / shared combat-manager status store.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The status ids were added to shared status/runtime files and active CSV authority; no parallel status store was introduced.
- `sein-a-hit-mark` duration `5s` is inferred because the inspected request bundle did not provide an explicit duration.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-C trait-5 damage only increases against enemies recently hit by Sein-A.
- User verifies in Play Mode that Sein-D trait-5 only increases damage after the same target has accumulated at least 4 recent zone-hit stacks.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now defines `SeinAHitMark` and `SeinDHeatStack`, accepts ids `sein-a-hit-mark` and `sein-d-heat-stack` in `TryParse(...)`, and returns shared runtime definitions for both statuses.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains rows `sein-a-hit-mark` and `sein-d-heat-stack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now applies `sein-a-hit-mark` from Sein-A hits and `sein-d-heat-stack` from Sein-D zone ticks.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gates `sein-c-trait-5` with `conditional_target_status_id=sein-a-hit-mark` and `conditional_target_status_min_stacks=1`, and gates `sein-d-trait-5` with `conditional_target_status_id=sein-d-heat-stack` and `conditional_target_status_min_stacks=4`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-C trait-5 and Sein-D trait-5 required reusable status-gated damage conditions, so new shared status identities were added instead of hardcoding those checks inside the skill runtime.

## Task: 2026-05-26 SingleAttack OnHit Status Effect Support

### Task title

Let shared SingleAttack hits apply choice-gated OnHit status effects for Rin-E master-2 slow.

### Goals

- Reuse `monster_skill_effects.csv` OnHit status rows for SingleAttack hit targets.
- Keep Rin-E master-2 slow on shared status application instead of a Rin-only branch.
- Preserve existing shared `SkillStatusApplyUtility` status application.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The change is scoped to SingleAttack hit targets and status-type OnHit effects.
- Unity Play Mode gameplay verification remains user-owned.
- Unity CSV runtime catalog sync is pending because batchmode reported another Unity instance has this project open.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-E master 2 applies the 2-second, -25% move speed slow to each hit enemy.
- Sync runtime catalog assets once Unity project locking allows it.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves `SkillMultiEffectTiming.OnHit` status effects with `SkillMultiEffectExecutor.ShouldRun(...)` and applies them to each SingleAttack hit target through `SkillStatusApplyUtility.TryApplyStatus(...)`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `rin-e-master2-slow` with `effect_timing=OnHit`, `status_effect_id=slow`, `status_duration_seconds=2`, and `status_move_speed_bonus=-0.25`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed only because Unity batchmode reported another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Follow-up enum validation found the `DamageAttribute` enum defines `Darkness`, not `Dark`; `rin-e-master2-slow.attribute` was corrected to `Darkness`, and a CSV enum scan returned `ENUM_VALIDATION_OK`.

### History

- 2026-05-26: Rin-E master-2 implementation required slow on each SingleAttack hit target, so SingleAttack adopted the existing shared OnHit status-effect pattern already used by beam/line runtime paths.
- 2026-05-26: User reported Unity auto-sync failing on `monster_skill_effects.csv` row 78 because `attribute=Dark` was not a valid enum value; Builder corrected the status-effect row to use `Darkness`.

## Task: 2026-05-24 Shared Passive Condition-Status And Trigger Expression Support

### Task title

Extend shared status/trigger runtime so passive effect rows and trigger rows can target expression-style condition statuses and shared proc-gated routed skills.

### Goals

- Let shared runtime parse condition-status expressions such as `chill;freeze` and `shock:5`.
- Let passive effect rows and trigger rows both consume the same condition-status matcher instead of duplicating string logic.
- Keep routed trigger validation aligned with actual runtime semantics so non-`SingleAttack` routed skills do not need fake damage payloads.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The implementation must stay on shared status parsing, trigger runtime, and CSV validation paths.
- Unity Play Mode gameplay verification remains user-owned.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity CSV validation passed.

### Next Actions

- Reuse the shared condition-status expression format for future passive or trigger work that needs OR lists or minimum stack gates before inventing another status-condition schema.
- Keep trigger damage-field validation scoped to `SingleAttack` unless a future routed trigger runtime begins consuming its own damage payload.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now defines shared condition-status parsing and matching helpers used by both target status checks and status-expire trigger checks.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `SkillTriggerRuntime.cs` now delegate condition-status checks to `StatusEffectRuntime`, and `SkillTriggerRuntime.cs` now supports multi-attribute trigger filters such as `Lightning;Ice`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now validates expression-style `condition_status_id` values and shared trigger-attribute lists, and now limits trigger damage payload validation to `runtime_kind=SingleAttack`.
- The validation follow-up was grounded by the failing Eve-G trigger rows `eve-g-auto-prism-ray` and `eve-g-auto-prism-ray-trait1`, which route `LineAttack` `eve-b` and therefore should not require synthetic `base_damage` values on the trigger row.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the shared runtime/validation change; only the existing `MSB3277` warnings remained.
- Unity `Pakuri/Validate CSV Source Data` then completed successfully and logged the runtime catalog load summary instead of `CsvFatalException`.

### History

- 2026-05-24: Eve F-J passive completion exposed that shared trigger validation was over-constraining routed non-`SingleAttack` triggers and that passive condition-status rows needed shared expression parsing.

## Task: 2026-05-26 Rin F-J Passive Status/Trigger Runtime Extensions

### Task title

Support Rin F-J passive status bonuses, hit-count effects, and trigger actions on shared combat runtime paths.

### Goals

- Let statuses grant outgoing critical damage bonus in the same modifier path as action speed, attack power, and critical chance.
- Let multi-effect rows run on `OnHitCount` for hit-count-gated passive effects.
- Let passive triggers filter by event skill, event source scope, trigger count, and status source skill before running a `SingleAttack`, effect, cooldown refund, or reload reduction action.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The extension is shared runtime behavior, not Rin-only branches.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Use `StatusEffectRuntime.ResolveCriticalDamageBonus(...)` for future outgoing critical damage status bonuses.
- Use `SkillMultiEffectTiming.OnHitCount` plus `condition_hit_count_min` for future hit-count-gated passive effects.
- Keep count gate evaluation before proc/internal cooldown consumption for `trigger_every_count` trigger rows.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` now includes `BuffModifierSpec.CritDamageBonusRate`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now initializes, measures, and resolves critical-damage status bonuses through `ResolveCriticalDamageBonus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` adds resolved status critical damage to outgoing critical damage calculation and stores passive trigger counts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now supports `ExecuteOnHitCount(...)`, health-ratio target conditions, hit-count conditions, and status critical damage bonus mapping.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now supports `SkillTriggerActionKind` actions, event-applied damage source, delayed triggers, event skill filters, event source scope filters, condition status source skill filters, count gates, cooldown refund, and reload reduction.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary and did not log a Pakuri CSV validation failure.

### History

- 2026-05-26: User approved extending shared runtime support for Rin F-J, including all-allied physical damage counts and reusable trigger/effect structures.

## Task: 2026-05-17 InGame Shared Status Runtime Baseline

### Task title

Keep the current Scripts2 status runtime grounded in `StatusEffectKind` and the shared unit-status store.

### Goals

- Keep all new status work routed through `StatusEffectKind` instead of ad hoc strings.
- Keep status storage, ticking, apply/remove/query, and label refresh owned by shared runtime code.
- Keep Eve-A shock application on the shared projectile hit path.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older status-effect slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active status runtime baseline summarized and retained for future work. 2026-05-18 Code Builder refactor keeps status labels on the actor path while centralizing shared actor presentation in `UnitActorView`. 2026-05-18 projectile/status tuning now reads status chance and label from `monster_skills.csv`; supported runtime labels can now be used as a fallback when `status_effect_id` is blank.

### Next Actions

- Future skills should apply statuses only through `InGameCombatManager.ApplyStatus(...)`.
- Later passive/resistance/damage work should query `StatusEffectKind`-based runtime state rather than adding parallel status storage.
- Use the archive snapshot when older shield/freeze/temporary-effect details are needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` defines the shared enum and central status display helpers.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` owns the current unit status store and ticking behavior.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` owns status apply/remove/query plus actor refresh on state changes.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` delegate active status label presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `InGameProjectileActor.cs` currently route Eve-A shock through the shared projectile hit path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now contains `status_chance` and `status_effect_label` per skill; Eve-A stores `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=媛먯쟾`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` passes CSV `StatusChance` into `StatusApplicationSpec.Chance`; `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains the Eve-A shock chance special case.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels `媛먯쟾`, `異붿쐞`, `?됯린`, `鍮숆껐`, `?뷀솕`, `痍⑥빟`, and `諛⑹뼱留? in addition to the canonical ids.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now resolves blank `status_effect_id` from a parseable `status_effect_label` and stores the canonical status tag from `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` rejects positive `status_chance` values on unsupported runtime status labels/ids.
- Unsupported design labels such as `移⑤У`, `?대쫫?쒖떇`, `?좎꽦 ?몄텧`, `?붿뿼 ???媛먯냼`, `?됰룞?띾룄 利앷?`, and `?됰갚` remain label-only in `monster_skills.csv` with `status_chance=0` unless a matching `StatusEffectKind` is added later.

### History

- 2026-05-17: Shared status runtime, enum centralization, label suffix display, and Eve-A shock application became the active baseline.
- 2026-05-18: Code Builder commonized `MonsterUnitActor`/`EnemyUnitActor` display refresh through `UnitActorView.cs`.
- 2026-05-18: Code Builder moved status chance/label authority from monster-level rows and hardcoded Eve-A executor logic into per-skill CSV rows.
- 2026-05-18: Code Builder made supported Korean status labels parseable from CSV, added validation for unsupported positive `status_chance`, and normalized design-only labels to chance 0.
