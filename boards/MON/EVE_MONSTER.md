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
