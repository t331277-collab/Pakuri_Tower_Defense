## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-09 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/SEIN_MONSTER.md`.

# SEIN_MONSTER

## Scope

Sein dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Sein file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Not populated yet.

## Task: 2026-05-20 Sein-B Shared Burst Projectile Implementation

### Task title

Implement Sein-B through the shared projectile burst extension.

### Goals

- Add a shared sequential burst count path instead of a Sein-only projectile branch.
- Make `sein-b` fire 5 projectiles per cycle at `shot_interval_seconds`, repeat that cycle `magazine_capacity` times, then wait on cooldown/reload.
- Wire `sein-b` to the requested `Assets/Prefab/Skill/Sein/Sein_A.prefab` visual through `EffectManager`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Unity Play Mode gameplay verification remains user-owned.
- Keep the implementation reusable for future projectile skills such as Vega.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and non-gameplay verified.

### Next Actions

- User verifies in Play Mode that Sein-B emits 5 sequential projectiles per cycle and repeats for 4 magazine cycles before the 6 second recovery.
- If Sein-B crit-chance master behavior is required, implement that as a separate choice-modifier extension because the current shared choice path still lacks crit chance modifiers.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md` defines `탄환 수 5`, `탄창 수 4`, `재장전 시간 6.0초`, `발사 간격 0.18초`, base fire damage `14`, attack coefficient `0.65`, and projectile speed `20.0`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `projectile_burst_count`; the `sein-b` row maps to `projectile_burst_count=5`, `magazine_capacity=4`, `shot_interval_seconds=0.18`, `cooldown_seconds=6`, `reload_seconds=6`, `projectile_speed=20`, and `pierce_count=0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now tracks queued burst shots and starts recovery only after the queued burst completes and the magazine is exhausted.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` keeps `AdditionalProjectileBonus` as simultaneous fan-spread only when `BurstProjectileCount <= 1`; burst skills use that bonus in runtime burst count instead.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `sein-b` to prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`, the existing `Assets/Prefab/Skill/Sein/Sein_A.prefab`.
- `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` followed by runtime mapping inspection returned `sein-b:burst=5;mag=4;interval=0.18;cooldown=6;reload=6;speed=20`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained. A first parallel runtime build hit only an `Assembly-CSharp.dll` file lock and passed when rerun alone.
- Unity-MCP console after refresh still contained MCP client-exit and `UnityEditor.Graphs` exceptions, but no `Pakuri` skill/CSV error was reported in the retrieved entries.

### History

- 2026-05-20: User approved an exact shared implementation for the Sein-B 5-shot burst cycle instead of the approximate existing magazine projectile behavior.

## Task: 2026-05-19 Sein-A Auto Fire Clarification And Effect Wiring

### Task title

Clarify why selected Sein-A appears idle on scene entry and restore the missing `EffectManager` prefab mapping.

### Goals

- Confirm from inspected runtime code whether selected `sein-a` is supposed to auto-fire on scene entry.
- Restore the missing `NewRunScene` `EffectManager` mapping for `sein-a`.
- Keep the result grounded in the current Scripts2 runtime and actual scene/prefab assets.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Do not claim Sein-specific attack logic is broken without code evidence.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

`sein-a` visual mapping was restored in `NewRunScene`. The inspected runtime still keeps selected 1P slot `A` on manual fire by default until `AutoBtn` enables `playerAutoSkillEnabled`.

### Next Actions

- User verifies in Play Mode that `AutoBtn` or held primary mouse input now shows the `Sein_A` projectile visual.
- If the user wants the selected 1P default `A` skill to auto-fire immediately on scene entry for all monsters, that is a separate global combat-policy change.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` routes the selected 1P slot `A` through `HandleSelectedPlayerPrimarySkillInput()` when `playerAutoSkillEnabled` is false and only auto-routes that skill after `EnablePlayerAutoSkillMode()`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10373` serializes `playerAutoSkillEnabled: 0`, so the default scene state keeps selected 1P `A` on manual fire until the user clicks `AutoBtn`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs` and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:14188` show `AutoBtn` exists and is wired to `InGameCombatManager.EnablePlayerAutoSkillMode()`.
- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab` exists in the repository and its `.meta` GUID is `256552cb82ec9c2499fc2e0e01d20dd2`.
- Before this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468` serialized `MonsterId: sein` with `SkillEffects: []`.
- After this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10471` serializes `MonsterId: sein`, `SkillId: sein-a`, and prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`.

### History

- 2026-05-19: User reported that Sein did not appear to attack in-game and noted the missing `EffectManager` Sein prefab assignment.
- 2026-05-19: Code Builder confirmed the missing scene mapping, restored the `sein-a` prefab entry, and recorded that selected 1P `A` remains manual by default unless `AutoBtn` enables auto fire.

## Task: 2026-05-14 Sein NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Sein prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Sein_Unit` through `NewRunSceneEntryManager`.
- Verify Sein creates an exact `sein` runtime model and initializes `MonsterUnitActor`.
- Make Sein's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Sein combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified. 2026-05-18 Sein active skill CSV rows were updated to the new skill-owned projectile/status schema. 2026-05-18 Sein design-only labels remain non-runtime statuses with `status_chance=0`.

### Next Actions

- User verifies Sein selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Sein_Unit.prefab` in `seinUnitPrefab`.
- Unity-MCP verification returned `sein:prefab=Sein_Unit|modelOk=True|model=sein|actor=True|actorModel=True|hpText=HP 210/210|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Sein_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `sein-a` `projectile_speed=18`, `pierce_count=1`, `magazine_capacity=8`, `reload_seconds=4.4`, and `shot_interval_seconds=0.32`, matching `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `sein-b` `projectile_speed=20`, `pierce_count=0`, `magazine_capacity=4`, `reload_seconds=6`, and `shot_interval_seconds=0.18`, matching `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` records `sein-d` label `초열 지대` and `sein-e` label `화염 저항 감소`; these are design labels because the current `StatusEffectKind` enum does not include Sein-specific fire-resistance statuses.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps Sein design labels `초열 지대` and `화염 저항 감소` with `status_chance=0`; runtime CSV validation rejects positive chance on unsupported status labels.
- Supported labels can still be introduced later through CSV because `StatusEffectKind.cs` and `InGameSkillDefinitionMapper.cs` now parse supported Korean labels from `status_effect_label` when `status_effect_id` is blank.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.
- 2026-05-18: Code Builder moved Sein projectile/status tuning into skill CSV rows using the reference documents for A/B projectile values and D/E status labels.
- 2026-05-18: Code Builder normalized Sein design-only status labels to chance 0 and added supported status-label fallback/CSV sync batch support.

## Task: 2026-05-13 Sein Battlefield Facade Registration

### Task title

Route Sein battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Sein skill behavior while replacing direct battlefield list registration writes.
- Keep Sein projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein skills in Play Mode if needed.

### Evidence

- `CombatRuntimeSeinSkills.cs:704`, `:757`, `:814`, and `:871` now call `AddBattlefieldProjectile(...)`.
- Sein skill-effect registrations now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Sein battlefield object registration through facade methods.

## Task: 2026-05-09 Sein Unit Executor Migration Resume

### Task title

Resume Sein unit executor migration for A-J skill behavior.

### Goals

- Route manifested Sein A-E learned active skills through a Sein-specific `CombatUnitRuntime` executor before the generic manifested fallback.
- Make manifested Sein A/B/C projectiles use Sein unit fire-damage, critical, heat, and Flame Barrage passive hooks from the source unit state.
- Make manifested Sein C/D/E effect ticks and delayed/residual effects read the source unit's F-J passive and Offering choices.
- Preserve the selected 1P Sein manual A input path.

### Constraints

- Role Owner is Code Builder after Designer handoff from `Pakuri/reference/Report/2026-05-09-sein-unit-executor-migration-design.md`.
- Do not run Unity Play Mode; user performs gameplay verification.
- Unity-MCP refresh could not run because no Unity Editor instance was connected.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds.

### Next Actions

- User verifies manifested Sein A pierce/heat, B magazine volley, C delayed explosion/path/residual, D superheated zone, E sky-line/ash zones, and F-J passive effects in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/Report/2026-05-09-sein-unit-executor-migration-design.md` existed before this resume and identified the missing Sein unit executor.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:625` dispatches `TryTickSeinUnitSkill(...)` before generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1048` lets `TryApplySeinUnitProjectileHit(...)` resolve manifested Sein projectile damage before generic damage.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:127` adds `TryTickSeinUnitSkill(...)`.
- `CombatRuntimeSeinSkills.cs:160`, `:211`, `:277`, `:301`, and `:369` add unit executor paths for Sein A/B/C/D/E.
- `CombatRuntimeSeinSkills.cs:1352` adds manifested Sein unit projectile-hit damage and A heat/master explosion handling.
- `CombatRuntimeSeinSkills.cs:2064` adds `HasSeinUnitPassive(...)` so F-J passive checks can read the unit state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\Manager\CombatRuntimeParty.cs Pakuri\Assets\Scripts\Combat\Skill\CombatRuntimeSeinSkills.cs` completed with exit code 0.
- Unity-MCP `refresh_unity` returned `No Unity Editor instances found`.

### History

- 2026-05-09: User reported the Sein unit executor migration had been interrupted and asked to resume the A-J migration from the report's remaining-work section.
- 2026-05-09: Code Builder resumed the migration, added Sein unit active/projectile/effect/passive hooks, and validated with local C# builds.
