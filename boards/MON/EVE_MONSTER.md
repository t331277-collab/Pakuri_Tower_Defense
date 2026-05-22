## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/EVE_MONSTER_ARCHIVE_2026-05-18.md`.
- Older monster-wide history remains in `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` and `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`.
- This active file now keeps only the current Eve A-J runtime baseline still useful for ongoing work.

# EVE_MONSTER

This is the active Eve-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Scope

- Active focus is the Scripts2 `NewRunScene` Eve A-J path.
- Older RunScene/Manifested/CombatRuntime detail is preserved in archive files and should be read only when older history is actually needed.

## Cross-Board Update Requirements

- Status work: update this file and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Data/catalog/Offering work: update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- NewRunScene UI or Offering gating changes: update this file and `boards/UI/RUNSCENE_UI.md`.
- Eve reports: update this file when a report changes active Eve facts. There is no active report board.

## Task: 2026-05-23 Eve-D Shock-Gated Delayed Recast

### Task title

Implement Eve-D on the shared SingleAttack path with a shock-gated delayed follow-up that reuses the same prefab but does not recurse.

### Goals

- Keep base `eve-d` on the shared `SingleAttack` runtime using `Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- Keep trait 3 cooldown reduction on the existing shared cooldown multiplier field instead of adding a new cooldown schema.
- Keep trait 4 on the shared status-stack path by adding one extra `shock` stack on hit.
- Implement `master-1` so targets already in `shock` when struck by Eve-D receive one extra Eve-D follow-up after `0.5` seconds at `50%` damage.
- Prevent the follow-up cast from scheduling another follow-up explosion.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV and runtime files were explicitly treated as the parsed source for this task.
- The follow-up must stay on the shared `SingleAttack` executor path; no Eve-only executor class was added.
- No new CSV columns were added for this task because the current parser requires header-width alignment across every row.
- Base Eve-D visual and master-1 follow-up visual both use `Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that base Eve-D uses `Eve_D.prefab`, trait 3 reduces cooldown, and trait 4 adds one extra `shock` stack.
- User verifies that `master-1` only recasts on enemies already carrying `shock` at the moment the first Eve-D hit lands, waits `0.5` seconds, deals `50%` damage, and does not trigger another follow-up.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps `eve-d` as `runtime_kind=SingleAttack`, `attribute=Lightning`, `base_damage=10`, `spell_power_coefficient=0.7`, `radius=3.5`, `cooldown_seconds=7`, `status_effect_id=shock`, and `status_chance=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-d-trait-3` to `cooldown_multiplier=0.8` and `damage_multiplier=1.15`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-d-trait-4` to `status_tag=shock` and `status_stacks_bonus=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-d-master-1` to `skill_effect_prefab_path=Assets/Prefab/Skill/Eve/Eve_D.prefab`, `branch_count=1`, `branch_damage_multiplier=0.5`, `branch_search_radius=0.5`, and `status_tag=shock`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now extends `SingleAttackSkillExecutor` so `ResolveFollowUpSpec(...)` interprets that scoped Eve-D choice payload, `RegisterFollowUpTarget(...)` records only targets that already have the required status, `ScheduleConditionalFollowUps(...)` waits per repeat, and `ExecuteAtCenter(..., false)` prevents recursive re-explosions.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `SkillId: eve-d` under monster `eve` to the `Eve_D.prefab` GUID.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the implementation; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User told Skill Builder to treat the current CSV/code as the parsed source and required Eve-D master 1 to recast once after `0.5` seconds only when the struck enemy was already shocked, without allowing the follow-up to explode again.

## Task: 2026-05-23 Eve-C Shared Hurtbox Root Fix

### Task title

Fix Eve-C prefab-hitbox misses by making shared collider-contact skills resolve enemy hurtboxes from the spawned unit root instead of the actor child transform.

### Goals

- Stop Eve-C from reading `targetColliders=[]` when Stage 1 enemies already have body colliders on the spawned unit hierarchy.
- Keep collider-authoritative contact skills using real unit hurtboxes.
- Leave non-contact skills such as explicit target-designated skills and radius/battlefield-only paths on their existing logic.

### Constraints

- Role Owner is Code Builder.
- Keep the solution shared; do not add an Eve-only collider lookup branch.
- Preserve existing non-contact targeting behavior for skills such as Ariel-D mark-style selection.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User reruns Eve-C in Play Mode and confirms `[ZoneHitboxDebug:eve-c]` no longer reports `targetColliders=[]`.
- User verifies Eve-C now damages only targets whose spawned-unit hurtboxes overlap the prefab collider.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs` now stores `HitboxRoot`, caches unit hitbox colliders, and exposes shared collider-overlap utility logic through `UnitHitboxUtility`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` now registers player/enemy roster entries with the spawned unit root transform instead of only the nested actor transform.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now resolves `FindUnitByCollider(...)` through `UnitRosterEntry.ContainsTransform(...)`, which includes the spawned unit root hierarchy.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now reads target hurtboxes from the shared unit hitbox contract, which fixes the Eve-C prefab-hitbox path without changing the non-contact radius fallback branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` and `InGameEnemySkillHitboxActor.cs` now prefer collider-authoritative roster-hit checks when the attacking object actually has colliders, and keep the old radius fallback only when no collider hitbox exists.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the change; existing `MSB3277` warnings remained. One earlier parallel build attempt hit a temporary file-lock on `obj\\Debug\\Assembly-CSharp.dll` before the successful rerun.

### History

- 2026-05-23: Eve-C debug logs showed `targetColliders=[]` on visibly overlapping Stage 1 enemies, so Code Builder moved shared contact hit detection to a spawned-unit-root hurtbox contract.

## Task: 2026-05-23 Eve-C Prefab Hitbox Debug Logging

### Task title

Instrument Eve-C prefab-hitbox ticks so live overlap misses can be explained from runtime logs.

### Goals

- Log Eve-C zone initialization with cached prefab collider data.
- Log Eve-C tick candidate counts, target collider sets, collider-pair overlap results, and routed hit/miss outcomes.
- Keep the logs narrow to Eve-C so shared AreaAttack runtime spam stays contained.

### Constraints

- Role Owner is Code Builder.
- Do not change Eve-C gameplay behavior while adding the logs.
- Limit the debug path to inspected Eve-C runtime id evidence instead of enabling all zone skills.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User runs Eve-C in Play Mode and captures lines beginning with `[ZoneHitboxDebug:eve-c]`.
- If the logs show collider overlap `false` while visuals appear to touch, inspect the reported enemy child collider bounds instead of the sprite only.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now logs Eve-C-only prefab-hitbox initialization, tick start/end, per-target collider summaries, per collider-pair `Distance(...).isOverlapped` results, and hit/miss routing.
- The debug gate is `runtime.SkillId == "eve-c"` inside `IsDebugSkill(...)`, so other AreaAttack skills do not emit the new logs.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the logging edit; existing `MSB3277` warnings remained.

### History

- 2026-05-23: User reported that `Eve_C(Clone)` looked physically overlapping in scene view but enemies still took no damage, so Code Builder added runtime overlap diagnostics on the Eve-C prefab-hitbox path.

## Task: 2026-05-23 Eve-C Prefab Collider Tick AreaAttack

### Task title

Make Eve-C follow its prefab collider and prefab scale on the shared AreaAttack path.

### Goals

- Stop Eve-C from using a fixed radius-only zone hit check when `Eve_C.prefab` already has a collider.
- Keep Eve-C visual size owned by the instantiated prefab instead of force-fitting the sprite to `radius * 2`.
- Let Eve-C trait radius scaling keep working through `radius_multiplier` by scaling the prefab hitbox and visual together.

### Constraints

- Role Owner is Code Builder.
- Keep the change on the shared AreaAttack runtime path; do not add an Eve-only executor branch.
- Do not reinterpret `radius_bonus=1.3`; this task keeps `radius_multiplier` as the scaling input and does not author a new `radius_bonus` usage.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-C only hits targets overlapping the instantiated `Eve_C.prefab` collider.
- User verifies Eve-C trait radius upgrades enlarge the prefab hitbox and visible effect together instead of using the old fixed-radius sprite fit.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Eve/Eve_C.prefab` exists and includes `BoxCollider2D` with authored size data.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now scales instantiated AreaAttack prefabs only when they actually contain collider hitboxes, using the existing snapshot radius-multiplier path before zone ticking starts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now caches prefab colliders, skips the old sprite-to-radius rescale when a prefab hitbox exists, and applies tick damage/status through collider overlap checks with the old radius path kept as fallback.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the change; existing `MSB3277` warnings remained.
- Unity-MCP console read after refresh showed no C# compile errors; remaining entries were existing `UnityEditor.Graphs` null-reference logs and MCP transport logs.

### History

- 2026-05-23: User asked Code Builder to stop treating Eve-C as a fixed-size radius zone and to make it follow the prefab collider/scale path like projectile and prefab-hitbox SingleAttack behavior.

## Task: 2026-05-23 Eve-C Shared AreaAttack Completion

### Task title

Implement Eve-C base runtime plus trait/master support on the shared AreaAttack path.

### Goals

- Keep Eve-C on the shared `AreaAttack` runtime with `Assets/Prefab/Skill/Eve/Eve_C.prefab` as the base scene visual.
- Support `trait-3` cooldown reduction through the existing choice cooldown multiplier.
- Support `trait-5` and `master-1` freeze-duration bonuses through a targeted choice status-duration bonus path.
- Support `master-1` as a shared threshold-status rule: `chill >= 4 -> freeze`.
- Support `master-2` as a shared `OnExpire` effect that bursts once from the zone center with `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Eve-C must stay on shared AreaAttack and multi-effect runtime paths; no Eve-only executor branch.
- Unity Play Mode gameplay verification remains user-owned.
- Native `codex review --uncommitted` could not complete because the local review command failed first on missing PATH and then on blocked websocket/network access, so final Reviewer evidence is a manual pass over the changed diff plus build results.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, compile-verified, and manual-review-passed. Unity-MCP menu/console calls timed out during CSV runtime sync, so runtime catalog prefab evidence for `eve-c-master-2` was recorded from the serialized asset file after a direct catalog update.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-C ticks `chill`, immediately freezes at 4+ chill stacks when `master-1` is learned, and that freeze duration increases only for Eve-C trait/master paths.
- User verifies that `master-2` fires exactly once when the field ends and uses `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-c-trait-3` to `cooldown_multiplier=0.85`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-c-trait-5` to `status_duration_bonus_status_id=freeze` and `status_duration_bonus=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-c-master-1` to `status_duration_bonus_status_id=freeze`, `status_duration_bonus=1.5`, `threshold_status_id=chill`, `threshold_status_min_stacks=4`, and `threshold_apply_status_id=freeze`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `eve-c-master-2` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `eve-c-master2-expire-burst` with `effect_timing=OnExpire`, `attribute=Ice`, `base_damage=24`, `spell_power_coefficient=1.5`, `requires_active_choice_id=eve-c-master-2`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now routes choice-targeted status duration bonuses, threshold-status application, and zone `OnExpire` effects through the shared runtime.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs` now applies a second shared status when the newly applied source status reaches a configured stack threshold.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now executes filtered `OnExpire` effect rows once before the zone actor is destroyed.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `SkillId: eve-c` to prefab GUID `383d4c700df69d44898dc953ea18b9d4`, which is `Assets/Prefab/Skill/Eve/Eve_C.prefab`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab` with GUID `30a4745c2cff29f41acf72125c981f67`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.monster_id -eq 'eve' -and $_.skill_id -eq 'eve-c' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned no rows after the edit.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User asked Code Builder / Skill Builder to implement Eve-C with a shared choice status-duration bonus, a shared threshold-status rule for `chill >= 4 -> freeze`, and an `OnExpire` master-2 burst using `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.

## Task: 2026-05-21 Eve-A Recursive Branch Projectile Rule

### Task title

Implement Eve-A Arc Bolt branch recursion, branch damage falloff, and fallback branch directions on the shared projectile path.

### Goals

- Let Eve-A branch projectiles apply the same shared shock-on-hit rule as the parent projectile.
- Let branch projectiles branch again through the same shared projectile path.
- Keep branch damage falloff data-owned at 70% per generation.
- Keep trait 5 and master 1 branch chance as additive choice data instead of forced 100% set values.

### Constraints

- Role Owner is Code Builder.
- Keep the code change minimal and inside the existing shared projectile runtime.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented in the shared projectile actor and choice CSV, then compile-verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-A branch hits can recursively branch, apply shock, and fall off as `100 -> 70 -> 49`.
- If live tuning shows branch spread is too tight or too wide, tune only the fallback random-right angle range instead of adding Eve-only executor branches.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` now lets `TrySpawnBranches(...)` keep spawning up to `branchOnHit.Count`, use nearest enemies first, and fall back to `SpawnFallbackBranchProjectile(...)` when nearby targets are missing.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` now initializes child branch projectiles with the parent `statusOnHit` and `branchOnHit.CloneForChild()` instead of `null`, which keeps shock application and recursive branch checks on the shared path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` still scales child damage by `damage * branchOnHit.DamageMultiplier`, so `branch_damage_multiplier=0.7` yields the requested chained falloff.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-trait-5` to `branch_chance_bonus=0.35`, blank `branch_chance_set`, `branch_count=2`, and `branch_damage_multiplier=0.7`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-master-1` to `branch_chance_bonus=0.6`, blank `branch_chance_set`, `branch_count=2`, `branch_damage_multiplier=0.7`, and `branch_search_radius=4.5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` returned `eve-a-trait-5 0.35 / blank / 2 / 0.7` and `eve-a-master-1 0.6 / blank / 2 / 0.7 / 4.5` for the branch fields after the edit.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the edit; existing MSB3277 warnings remained.

### History

- 2026-05-21: User required the new Arc Bolt branch rule to be implemented as minimal shared runtime code plus CSV tuning instead of an Eve-only special-case executor.

## Task: 2026-05-17 Eve A-J Active Runtime Baseline

### Task title

Keep the current Eve A-J Scripts2 runtime state compact and explicit.

### Goals

- Preserve the current Eve A-J data/Offering baseline from the active CSV source files.
- Preserve the shared status-runtime foundation and visible label output used by Eve-A shock.
- Preserve Eve-A projectile modifier execution through the shared InGame execution path.
- Keep the board explicit that Eve B-E executor depth and F-J passive effect depth still remain later work.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older Eve slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active Eve baseline summarized and retained for future work. 2026-05-18 Eve-A/Eve status values are now read from `monster_skills.csv`. 2026-05-18 supported Korean status labels can now resolve through the shared status parser.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-A shock, modifier choices, and Offering gating behave as recorded.
- Continue later Eve work from the shared status/runtime path instead of reintroducing Eve-only special-case state.
- Use the archive snapshot when older prefab-binding or CombatRuntime-era Eve history is needed.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv`, `monster_skill_choices.csv`, and `monster_modifier_skill_choice.csv` hold the retained Eve A-J source rows and active choice/modifier mappings.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs`, `SkillExecutionSystem.cs`, `SkillRuntimeInstance.cs`, and `InGameProjectileActor.cs` own the current Eve-A projectile modifier, branch, and shock execution path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectData.cs`, `StatusEffectKind.cs`, `InGameSkillDefinitionMapper.cs`, and `BaseUnitRuntimeModel.cs` own the retained shared status foundation used by Eve work.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` was recorded as the current Offering gating point for learned active/passive Eve reward choices.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores Eve-A `projectile_speed=15`, `pierce_count=0`, `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=감전`; Eve-B/C/D/E status rows are `slow`/`chill`/`shock`/`vulnerable` with labels.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains an Eve-A-only shock chance override; `InGameSkillDefinitionMapper.cs` now maps status chance from CSV into `StatusApplicationSpec`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels such as `감전`, `둔화`, `추위`, and `취약`, and `InGameSkillDefinitionMapper.cs` can use a parseable `status_effect_label` when `status_effect_id` is blank.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` shows Eve's positive runtime statuses as `eve-a shock 0.15 감전`, `eve-b slow 0.2 둔화`, `eve-c chill 1 추위`, `eve-d shock 1 감전`, and `eve-e vulnerable 1 취약`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-17: Eve A-J source data, Offering mapping, shared status foundation, Eve-A projectile modifier execution, and visible status label behavior became the current active baseline.
- 2026-05-18: Code Builder moved Eve-A shock chance and projectile speed from hardcoded/monster-level data into the Eve skill row.
- 2026-05-18: Code Builder added supported Korean status-label parsing/fallback and CSV runtime sync batch support.

## Task: 2026-05-18 Eve-B LineAttack Runtime

### Task title

Implement Eve-B as a reusable CSV-driven LineAttack and translate Eve skill rows.

### Goals

- Translate Eve A-J rows in `Pakuri/Assets/CSVdata/source/monster_skills.csv` to Korean display text.
- Keep Eve-B tuning in CSV instead of hardcoded skill-ID branches.
- Route Eve-B through the shared `BeamSkillData` / LineAttack runtime so later monster LineAttack skills can reuse it.

### Constraints

- Role Owner is Code Builder.
- Numeric tuning must come from `monster_skills.csv`.
- `Eve_B.prefab` is the visual asset for Eve-B.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated by runtime/editor builds, Unity CSV sync, and Unity-MCP data mapping inspection.

### Next Actions

- User verifies in `NewRunScene` Play Mode that learned Eve-B fires as a line attack, shows `Assets/Prefab/Skill/Eve/Eve_B.prefab`, ticks for 1.2 seconds at 0.15 second intervals, and applies slow at the CSV chance.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has Korean Eve A-J display/description/summary rows and `active_duration_seconds`; Eve-B is `LineAttack`, damage `12`, spell coefficient `1.6`, width `3.2`, cooldown `6.5`, active duration `1.2`, tick interval `0.15`, status `slow`, chance `0.2`, label `둔화`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now carry `ActiveDurationSeconds` into `SkillTimingSpec.ActiveDuration`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now implements `BeamSkillExecutor` for reusable line targeting, status resolution, visual instantiation, and CSV-driven damage/timing.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` applies repeated line ticks to targets inside the beam rectangle and applies status through `InGameCombatManager.ApplyStatus(...)`.
- Unity-MCP asset info confirmed `Assets/Prefab/Skill/Eve/Eve_B.prefab` exists with GUID `224f5e7622cd0264b961ee388a015d65`.
- Unity-MCP `GameManager` component inspection confirmed `EffectManager` maps monster `eve` skill `eve-b` to `Assets/Prefab/Skill/Eve/Eve_B.prefab`.
- Unity-MCP CSV mapping inspection returned `name=프리즘 레이|runtime=LineAttack|activeDuration=1.2|cooldown=6.5|tick=0.15|beamDuration=1.2|beamTick=0.15|damage=12|coef=1.6|width=3.2|status=slow|chance=0.2`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity-MCP refresh reached idle; console warning/error read after clearing showed only `UnityEditor.Graphs.Edge.WakeUp` and MCP client handler logs, not C# compile errors.

### History

- 2026-05-18: User requested Eve skill CSV Korean translation and Eve-B LineAttack implementation using `monster_skills.csv` tuning and `BeamSkillData.cs` as the reference structure.

## Task: 2026-05-18 Eve C/D/E Runtime Kind And Names

### Task title

Correct Eve C/D/E names and AreaAttack/SingleAttack runtime kinds from reference files.

### Goals

- Keep Eve C named `프로스트 필드`, not translated as `서리 지대`.
- Keep Eve D named `스태틱 오버라이드`, not translated as `정전기 과부하`.
- Route Eve C/E as sustained `AreaAttack` and Eve D as one-shot `SingleAttack`.

### Constraints

- Role Owner is Code Builder.
- Eve C/D/E names are grounded in `Pakuri/reference/2.Monster/eve/skill`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C ticks for 4 seconds, Eve E ticks for 5 seconds, and Eve D performs a one-shot area hit.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` lists `스킬명 | 프로스트 필드`.
- `Pakuri/reference/2.Monster/eve/skill/d-static-override.md` lists `스킬명 | 스태틱 오버라이드`.
- `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` lists `스킬명 | 플라즈마 필드`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-c` display `프로스트 필드`, runtime `AreaAttack`, tick interval `0.5`, and active duration `4`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-d` display `스태틱 오버라이드` and runtime `SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-e` runtime `AreaAttack`, tick interval `0.8`, and active duration `5`.
- Eve passive descriptions in `monster_skills.csv` now refer to `프로스트 필드` and `스태틱 오버라이드`.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User reported that the issue was not CSV corruption but wrong translated/hardcoded skill naming and requested Code Builder correction.
