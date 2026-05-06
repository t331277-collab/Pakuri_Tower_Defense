# SEIN_MONSTER

## Scope

Sein dedicated monster, skill, and runtime persistent-state file.

At the start of new work, read `boards/MON/MON_BLACKBOARD.md` first and consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Not populated yet.

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
