# SEIN_MONSTER

## Scope

Sein dedicated monster, skill, and runtime persistent-state file.

At the start of new work, read `boards/MON/MON_BLACKBOARD.md` first and consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Not populated yet.

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

## Task: 2026-05-08 Manifested Sein A Pierce And Hit-State Fix

### Task title

Make manifested Sein A keep selected Sein A pierce and hit-state behavior.

### Goals

- Fix manifested Sein A base pierce being lost in the generic manifested projectile path.
- Apply manifested Sein A pierce upgrades from `sein-a-trait-4` and `sein-a-master-1`.
- Preserve Sein A hit-state behavior by setting `SeinScorchingArrowTimer` from manifested projectile hits.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Sein A pierces one enemy by default and more when the relevant choices are learned.
- User verifies manifested Sein A heat-state interactions in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:1057` shows selected Sein A pierce starts at `1`, adds `sein-a-trait-4`, and adds `sein-a-master-1`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:674` now sends generic manifested projectiles through `ResolveManifestedProjectilePierce(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:688` through `:694` implements manifested Sein A pierce as selected base `1`, plus trait 4 and master 1.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:825` sets `enemy.SeinScorchingArrowTimer` on manifested Sein A hits and checks `sein-a-master-2` via the projectile's `ManifestedSource`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported selected Sein A had one pierce, but manifested Sein A had no pierce.

## Task: 2026-05-08 Manifested Sein Common Runtime Parity

### Task title

Apply Sein Offering choices and field status through the manifested common skill runtime.

### Goals

- Keep manifested Sein skills sourced from `SkillDefinition` data.
- Apply Sein manifested Offering choices in shared damage, cooldown, reload, and shot interval paths.
- Let manifested Sein D field ticks mark superheated zone state.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Sein private skill code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Sein skills and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:760` applies Sein D manifested field tick superheated-zone state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Sein skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Sein cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1278` and `:1310` include Sein reload and shot-interval choice handling.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

## Required Sections For Future Work

- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Task: 2026-05-04 Sein Runtime Sprite Catalog Follow-up

### Task title

Fix Sein runtime sprite catalog paths.

### Goals

- Ensure Sein's runtime selected-Monster sprite and projectile sprite resolve through the active CSV runtime catalog.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this follow-up.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein selection and projectile visuals in Play Mode.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Unity AssetDatabase inspection showed `Pakuri/Assets/Data/GameData/Monsters/sein.asset` already had assigned `UnitSprite` and `ProjectileSprite`.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now fills Sein with `Assets/Image/Monster/Sein/Sein_Temp.png` and `Assets/Image/Monster/Sein/Sein_Shoot.png`.
- Unity-MCP import/sync validation resolved runtime `sein` with non-null UnitSprite and ProjectileSprite assets.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains the Sein sprite path entries generated from the CSV runtime source.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log.

### History

- 2026-05-04: User reported Sein was one of the two Monsters whose `PrototypeCombatTuning` sprites were not applied.
- 2026-05-04: Builder found the active runtime CSV path was empty for Sein and filled/synced the runtime catalog.

## Task: 2026-05-06 Sein A-E Active Runtime Implementation

### Task title

Implement Sein active skills A-E from the reference skill documents.

### Goals

- Implement Sein A `Scorching Arrow` as the manual magazine projectile path.
- Implement Sein B-E as click-triggered automatic skills using current selected-Monster combat runtime.
- Mark Sein A-E runtime data as implemented and classify B-E with non-shared-magazine runtime kinds where needed.

### Constraints

- Role Owner is Code Builder.
- Implementation is grounded in `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md` through `e-doomsday-line.md`.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein A-E behavior in Play Mode, especially A magazine fire, B volley cadence, C delayed explosion, D field ticks, and E fire-resistance reduction.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md` through `e-doomsday-line.md` define Sein A-E active skill stats and trait/master effects.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now implements Sein A-E runtime helpers and selected-Monster checks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` routes Sein A primary fire through `FireManualSeinScorchingArrow(...)` and calls `TrackSeinProjectileHit(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` now includes Sein in selected-Monster cooldown, automatic-skill, magazine, and action-speed dispatch.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` adds enemy state for Sein heat/fire-resistance effects and panel cooldown reporting for Sein B-E.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` now mark Sein A-E as `RuntimeImplemented`; B is `CooldownProjectile`, C is `AreaAttack`, D is `Field`, and E is `LineAttack`.
- Unity-MCP `execute_code` confirmed both `sein.asset` and `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)` read Sein A-E as `RuntimeImplemented` with the expected runtime kinds.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity refresh/compile was requested; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.

### History

- 2026-05-06: User requested implementation of active skills A-E under `Pakuri/reference/2.Monster/sein/skill`.
- 2026-05-06: Code Builder implemented Sein A-E runtime code, updated data classifications, and completed local build/Unity-MCP validation.

## Task: 2026-05-06 Sein F-J Passive Runtime Implementation

### Task title

Implement Sein passive skills F-J from the reference skill documents and CSV data.

### Goals

- Implement Sein F heated-aim passive bonuses for fire damage, fire crit chance, crit damage trait, and Scorching Arrow magazine trait.
- Implement Sein G flame-barrage passive auto Blazing Volley proc with internal cooldown and proc traits.
- Implement Sein H, I, and J target debuff/passive interactions for fire resistance, fire damage taken, Superheated Zone tick speed/radius, and Doomsday cooldown charge.
- Mark Sein F-J runtime data as implemented.

### Constraints

- Role Owner is Code Builder.
- Implementation is grounded in `Pakuri/reference/2.Monster/sein/skill/f-heated-aim.md` through `j-doomsday-omen.md`, `Pakuri/Assets/CSVdata/source/monster_skills.csv`, and `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein F-J passive behavior in Play Mode, especially G auto volley proc cadence, H/I/J target debuffs, and J cooldown charge on Doomsday kill.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now implements Sein F-J passive checks, fire damage/critical modifiers, Flame Barrage proc handling, H/I/J timed target debuffs, D tick speed/radius passive modifiers, and J cooldown charge helper.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now applies Sein final-damage, critical chance, critical multiplier, flat fire-defense reduction, and projectile-hit passive tracking hooks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores enemy timers and damage-taken bonuses for Sein H/I/J passive debuffs.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` now mark Sein F-J as `RuntimeImplemented`.
- Unity-MCP `execute_code` confirmed runtime catalog rows `sein-f` through `sein-j` resolve as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity refresh/compile was requested; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.

### History

- 2026-05-06: User requested implementation of the remaining Sein passive skills F-J.
- 2026-05-06: Code Builder implemented Sein F-J passive runtime code, updated data implementation flags, and completed local build/Unity-MCP validation.

## Task: 2026-05-07 Sein Active Skill Correction Pass

### Task title

Correct Sein A, B, C, and E active skill behavior from user gameplay feedback.

### Goals

- Make A `폭염 화살` explosion also damage the original hit target.
- Change B from fan-like volley cooldown behavior to a separate 4-shot magazine skill where each use fires 5 arrows and reloads for 6 seconds.
- Change C into a target-locked curved projectile that explodes on contact with its selected enemy target.
- Change E so its visual lines start from sky position `(10, 10)` and damage only up to one target per line.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies A explosion target inclusion, B magazine/reload cadence, C target-locked projectile explosion, and E sky-origin target-only lines in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now removes the A explosion exclusion so `sein-a-master-2` also damages the original hit target.
- `CombatRuntimeSeinSkills.cs` now tracks B ammo, reload, and shot interval separately with `seinBlazingVolleyShotsRemaining`, `seinBlazingVolleyReloadRemaining`, and `seinBlazingVolleyShotCooldownRemaining`.
- `CombatRuntimeSeinSkills.cs` now creates target-locked C projectiles through `FireSeinFlameTrajectoryProjectile(...)` and updates their homing/arc movement through `UpdateSeinLockedTargetProjectile(...)`.
- `CombatRuntimeSeinSkills.cs` now creates E line visuals from `GetSeinDoomsdaySkyOrigin()` and selects one enemy per line through `FindNearestSeinDoomsdayTarget(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now resolves locked-target projectiles only against their assigned enemy target and shares projectile damage resolution through `ResolvePlayerProjectileDamage(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores locked-target projectile metadata and shows Sein B panel ammo/cooldown using the B magazine state.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` now classify `sein-b` as `MagazineProjectile`.
- Unity-MCP `execute_code` confirmed both `sein.asset` and `PakuriCsvRuntimeData.ResolveCatalogOrFallback(null)` read `sein-b` as `MagazineProjectile`, `RuntimeImplemented`, magazine `4`, reload `6`, cooldown `6`, and interval `0.18`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity console error query after refresh returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-07: User clarified four corrections for Sein A, B, C, and E active skills.
- 2026-05-07: Code Builder applied the correction pass and validated through builds, Unity import, runtime catalog inspection, console check, and `git diff --check`.

## Task: 2026-05-07 Sein C/E Follow-up Correction

### Task title

Fix Sein C delayed explosion/path residual behavior and E ash-zone target placement.

### Goals

- Make C `Flame Trajectory` explode after projectile contact using the documented 0.8 second delay.
- Make C `Piercing Trajectory` leave short path segments while the projectile travels instead of drawing/applying the full line immediately.
- Make C `Falling Trajectory` spawn its residual fire zone after the delayed explosion.
- Make E `Ashen Sky` create zones on actual hit targets instead of grouping all zones around the initially selected target.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies C delayed explosion, C moving path trail, C residual fire zone, and E per-target ash zones in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/c-flame-trajectory.md:23` defines C delay as `0.8초`; `:49` defines `낙화 궤적`; `:50` defines `관통 궤도`.
- `Pakuri/reference/2.Monster/sein/skill/e-doomsday-line.md:22` defines E as 3 straight-line hits; `:51` defines `잿빛 하늘`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:25`, `:29`, and `:52` now record previous projectile position, create C path segments during travel, and route C contact into delayed impact handling before normal projectile damage.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:484` implements C moving path segment visuals/damage; `:529` implements delayed C impact; `:624` creates C falling residual fire zone after explosion expiry.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:358` passes actual E hit enemies into ash-zone creation; `:762` creates up to 3 ash zones from those target positions.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings when rerun without the parallel file-lock conflict.
- `git diff --check` completed with no whitespace errors; it reported only CRLF conversion warnings.
- Unity-MCP refresh/compile was requested. Unity console error query returned 3 existing missing-script reference errors with no file/line and 2 MCP client handler logs, not C# compile errors from this change.

### History

- 2026-05-07: User reported C delay/path/residual behavior issues and E `Ashen Sky` zones incorrectly grouping around the first target.
- 2026-05-07: Code Builder changed C impact from immediate explosion to delayed skill effect, changed `Piercing Trajectory` into travel-time path segments, added C residual-zone expiry handling, and changed E ash zones to use actual hit target positions.
