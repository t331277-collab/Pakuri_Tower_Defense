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

Builder implementation completed and locally verified.

### Next Actions

- User verifies Sein selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Sein_Unit.prefab` in `seinUnitPrefab`.
- Unity-MCP verification returned `sein:prefab=Sein_Unit|modelOk=True|model=sein|actor=True|actorModel=True|hpText=HP 210/210|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Sein_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.

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
