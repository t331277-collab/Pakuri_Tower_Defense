# RIN_MONSTER

## Scope

Rin dedicated monster, skill, and runtime persistent-state file.

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

## Task: 2026-05-04 Rin D Execution Target And Hit Effect Fix

### Task title

Make Rin D `Finishing Blow` cast only on execution-threshold enemies and show a simple hit effect.

### Goals

- Stop Rin D from attacking full-health or otherwise non-executable enemies.
- Make Rin D cast only when an enemy is at or below the execution-health threshold.
- Preserve the master skill meaning that execution threshold `-10%` changes the base threshold from 30% max HP to 20% max HP.
- Add a simple visible circle effect when Rin D lands.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Rin D does not fire when every enemy is above the current execution threshold.
- User verifies in Play Mode that Rin D shows the new circle hit effect on an executable target.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Before this fix, `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` `FindRinFinishingBlowTarget(...)` kept a nearest-enemy fallback and returned `executeTarget ?? fallback`, so Rin D could attack a 100% HP enemy whenever no enemy met the execution threshold.
- `CombatRuntimeRinSkills.cs:604-632` now returns only an enemy whose health ratio is at or below `GetRinFinishingBlowExecuteThreshold()`, with no nearest fallback.
- `CombatRuntimeRinSkills.cs:635-649` now centralizes the execution threshold calculation: base `30%`, trait 2 `+10%`, and master 2 `-10%`, so master 2 changes the base 30% threshold to 20% when applied alone.
- `CombatRuntimeRinSkills.cs:336` now creates a Rin D hit effect after damage is applied, and `:651-667` creates `RinFinishingBlowHit` using the existing circle effect helper and generated circle sprite.
- `Pakuri/reference/2.Monster/rin/skill/d-finishing-blow.md:55-61` now documents that Finishing Blow only casts on enemies at or below the threshold, does not fire without such a target, and that threshold `-10%` means 30% to 20%.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh reached idle with `ready_for_tools=true`; Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.

### History

- 2026-05-04: User reported Rin D was targeting 100% HP enemies instead of only enemies at or below 30% max HP.
- 2026-05-04: Builder found the cause was the nearest-enemy fallback in `FindRinFinishingBlowTarget(...)`, removed that fallback, added the hit effect, clarified the master threshold rule, and validated with builds plus Unity refresh/console checks.

## Task: 2026-05-04 Rin Non-Magazine Skill Map-Wide Range

### Task title

Make Rin non-magazine active skills use the whole battlefield map as runtime range.

### Goals

- Document that Rin non-magazine skills B-E use the whole battlefield map as target/search range.
- Keep Rin A as the magazine projectile skill, while changing Rin C/D/E runtime target acquisition away from short asset `Range` values.
- Explain why Rin skills initially felt very short-ranged.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin B-E in Play Mode, especially C/D/E against enemies beyond the old 7.5-8.5 unit range.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Data/GameData/Monsters/rin.asset` stores short active-skill ranges: `rin-c` Range `8.5`, `rin-d` Range `7.5`, and `rin-e` Range `8`.
- Before this change, `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` used those `skill.Range` values directly in `FindNearestEnemy(...)` / `FindRinFinishingBlowTarget(...)`, so non-magazine target selection was limited to those short distances from `eveAnchor`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:19` adds `RinMapWideSkillRangePadding`, and `:731-735` computes battlefield-wide range from `fieldSize`, `EnemySpawnX`, and `BattlefieldMaxY`.
- `CombatRuntimeRinSkills.cs:196-211` now uses the map-wide range for Rin C target search and beam length.
- `CombatRuntimeRinSkills.cs:297-298` now uses the map-wide range for Rin D target search.
- `CombatRuntimeRinSkills.cs:367-368` now uses the map-wide range for Rin E target search.
- `Pakuri/reference/2.Monster/rin/skill/b-howling.md:41-44`, `c-shockwave.md:53-56`, `d-finishing-blow.md:55-58`, and `e-collapse-strike.md:53-56` now document the Rin non-magazine whole-battlefield runtime range rule.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing System.Net.Http/System.IO.Compression warnings.
- A first parallel `Assembly-CSharp-Editor` build failed with `CS2012` because the shared `obj\Debug\Assembly-CSharp.dll` was locked by the simultaneous build process; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` by itself completed with 0 errors and the same existing warnings.
- Unity refresh reached idle; Unity console error query returned only MCP-FOR-UNITY client handler disposal/exit logs, not project compile errors.

### History

- 2026-05-04: User reported Rin skill range was very short and clarified that non-magazine skills should treat the whole map as range.
- 2026-05-04: Builder found the short range came from Rin C/D/E reading the `SkillDefinition.Range` values from `rin.asset`.
- 2026-05-04: Builder documented the non-magazine map-wide range rule in Rin B-E skill markdown files and changed Rin C/D/E runtime target search to use battlefield-wide range.

## Task: 2026-05-04 Rin Runtime Sprite And A Projectile Cleanup Follow-up

### Task title

Fix Rin runtime sprite catalog paths and prevent Rin A from expiring by short lifetime.

### Goals

- Ensure Rin's runtime selected-Monster sprite and projectile sprite resolve through the CSV runtime catalog.
- Make Rin A projectile deletion follow the common selected-Monster projectile X-edge rule instead of the computed lifetime.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this follow-up.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin A travel and cleanup in Play Mode.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Unity AssetDatabase inspection showed `Pakuri/Assets/Data/GameData/Monsters/rin.asset` already had `UnitSprite=Rin_Temp (2)` and `ProjectileSprite=Rin_Shoot`.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now fills Rin with `Assets/Image/Monster/Rin/Rin_Temp (2).png` and `Assets/Image/Monster/Rin/Rin_Shoot.png`.
- Unity-MCP import/sync validation resolved runtime `rin` as `UnitSprite=Rin_Temp (2), ProjectileSprite=Rin_Shoot, ProjectileLifetime=4`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now uses `HasPlayerProjectileReachedBattlefieldXEdge(...)` for selected-Monster projectile cleanup after no hit, so Rin A no longer depends on `range / RinShatteringFistProjectileSpeed` lifetime for cleanup.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log.

### History

- 2026-05-04: User reported Rin's sprite application issue and Rin A disappearing again after about 0.5 seconds.
- 2026-05-04: Builder fixed the CSV runtime sprite path and changed selected-Monster projectile cleanup to battlefield X-edge cleanup.

## Task: 2026-05-04 Rin A-E Active Runtime Implementation

### Task title

Implement Rin active skills A-E and their enhancement/master effects from the Rin reference skill documents.

### Goals

- Implement `rin-a` Shattering Fist as Rin's physical magazine projectile.
- Implement `rin-b` Howling, `rin-c` Shockwave, `rin-d` Finishing Blow, and `rin-e` Collapse Strike in the selected-Monster combat runtime.
- Apply active-skill trait and master choices from the Rin A-E reference documents.
- Use the user's confirmed defaults: `rin-e` auto-targets the nearest enemy, Rin A chain radius is `3.0`, Rin D explosion radius is `2.4`, and elemental extra damage is calculated from the physical final damage dealt by the source hit.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this implementation.

### Role Owner

Code Builder

### Status

Implemented, Reviewer findings fixed, and locally validated. Code Reviewer has not been rerun because the user has not requested a second review.

### Next Actions

- User verifies Rin A-E behavior in Play Mode.
- Run another Code Reviewer pass only if the user explicitly requests it.
- If a later full ally-party model is added, revisit Howling's all-ally wording; this implementation applies it to the current selected Monster combat model.

### Evidence

- `Pakuri/reference/2.Monster/rin/skill/a-shattering-fist.md` through `e-collapse-strike.md` were read as the source references for Rin A-E values and enhancement/master effects.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:77` implements Rin A projectile fire, `:152` implements Howling, `:187` implements Shockwave, `:287` implements Finishing Blow, and `:357` implements Collapse Strike.
- `CombatRuntimeRinSkills.cs:445` handles Rin A master 2 extra lightning and every-third-hit chain using the user-confirmed `3.0` radius.
- `CombatRuntimeProjectiles.cs:52` now passes the actual `appliedDamage` from `ApplyDamageToEnemy(...)` into `HandleRinProjectileHit(...)` for Rin A follow-up effects.
- `CombatRuntimeRinSkills.cs:502`, `:504`, and `:523` now use and return the actual applied physical/additional damage instead of returning the calculated `FinalDamage`.
- `CombatRuntimeRinSkills.cs:507` applies elemental extra damage from the actual physical damage applied by the source hit.
- `Pakuri/Assets/Data/GameData/Monsters/rin.asset:88`, `:155`, `:222`, `:287`, and `:354` mark Rin A-E as `ImplementationState: 2`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- Unity-MCP script refresh reached `resulting_state: idle`; Unity console error query returned only MCP-FOR-UNITY client handler logs.
- External Reviewer output at `codex_loop_logs\rin_skill_reviewer_20260504.md` returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding was fixed: Rin A/C/D/E elemental extra damage now receives the physical damage actually dealt after `ApplyDamageToEnemy(...)` caps shields and remaining HP.

### History

- 2026-05-04: User requested implementation of Rin active skills A-E and enhancements from `Pakuri/reference/2.Monster/rin/skill`.
- 2026-05-04: Code Builder asked for clarification on targeting, branch/explosion radius, and elemental extra-damage basis.
- 2026-05-04: User confirmed the default targeting/radius assumptions and clarified that elemental extra damage is based on the physical damage dealt by the source hit.
- 2026-05-04: Code Builder implemented Rin A-E runtime behavior and validated with dotnet builds plus Unity-MCP refresh/console checks.
- 2026-05-04: User requested Code Reviewer execution; external Reviewer ran once and returned `NEEDS_CHANGES` for the elemental extra-damage basis.
- 2026-05-04: User requested fixing the Reviewer findings; Code Builder changed Rin projectile and skill follow-up damage to use applied damage, then revalidated with both dotnet builds and Unity-MCP refresh/console checks.
