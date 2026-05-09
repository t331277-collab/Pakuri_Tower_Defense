# VEGA_MONSTER

## Scope

Vega dedicated monster, skill, and runtime persistent-state file.

At the start of new work, read `boards/MON/MON_BLACKBOARD.md` first and consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Vega active skills A-E and passive skills F-J are implemented and locally validated.

## Task: 2026-05-08 Manifested Vega Common Runtime Parity

### Task title

Apply Vega Offering choices through manifested projectile and common skill runtime.

### Goals

- Preserve manifested Vega A three-sword cadence.
- Apply Vega manifested Offering choices to damage, cooldown, magazine, reload, and name-mark projectile state.
- Keep Vega field/area skills on shared manifested SO-driven execution.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Vega private skill code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Vega A and Offering-upgraded skills in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1073` applies Vega A damage and mark-stack choices inside the manifested queued projectile update.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Vega skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Vega cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1250` and `:1278` include Vega A magazine/reload choice handling.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

## Required Sections For Future Work

- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Task: 2026-05-07 Vega Active Skills A-E Runtime Implementation

### Task title

Implement Vega active skills A-E from `Pakuri/reference/2.Monster/vega/skill`.

### Goals

- Implement A `삼검난무` as a magazine skill that queues three piercing sword projectiles per shot and applies `이름표식`.
- Implement B `침묵의 대태도` as a line slash that applies physical damage and silence.
- Implement C `몰살 허가` as a self buff for Vega action speed and attack power.
- Implement D `검은 명부 개방` as area slashes around all enemies with `이름표식`.
- Implement E `최종선고` as a single-target execute-style hit against the highest-mark enemy with mark-scaled damage and mark consumption.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega A-E behavior in Play Mode, especially A delayed three-shot cadence, B silence, D overlap behavior, and E mark consumption.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Source references read: `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md`, `b-silent-greatblade.md`, `c-extermination-permit.md`, `d-black-ledger-release.md`, and `e-final-sentence.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs` now contains `FireManualVegaThreeSwordFlurry(...)`, `TryCastVegaSilentGreatblade(...)`, `TryCastVegaExterminationPermit(...)`, `TryCastVegaBlackLedgerRelease(...)`, `TryCastVegaFinalSentence(...)`, `HandleVegaProjectileHit(...)`, and Vega mark/silence/damage helpers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores Vega enemy mark/silence state, Vega projectile mark stacks, and Vega delayed effect silence duration.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeEveSkills.cs`, and `CombatRuntimeEnemies.cs` now connect Vega cooldowns, action speed, projectile hit tracking, skill-effect ticks, status labels, and silence blocking.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/vega.asset` now mark Vega A-E as `RuntimeImplemented`; B/C/D/E are classified as `LineAttack`, `Buff`, `AreaAttack`, and `Execute`.
- Unity-MCP `execute_code` confirmed runtime catalog rows: `vega-a:MagazineProjectile:RuntimeImplemented`, `vega-b:LineAttack:RuntimeImplemented`, `vega-c:Buff:RuntimeImplemented`, `vega-d:AreaAttack:RuntimeImplemented`, `vega-e:Execute:RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP refresh/compile was requested; console error query returned existing missing-script reference errors and MCP client handler logs, not C# compile errors from this change.
- `git diff --check` completed with no whitespace errors; it reported only CRLF conversion warnings.

### History

- 2026-05-07: User requested implementation of Vega active skills A-E.
- 2026-05-07: Code Builder implemented Vega A-E runtime behavior, updated Vega data state, and completed local build/Unity-MCP validation.

## Task: 2026-05-07 Vega Passive Skills F-J Runtime Implementation

### Task title

Implement Vega passive skills F-J from `Pakuri/reference/2.Monster/vega/skill`.

### Goals

- Implement F `각인 심화` as mark-target damage amplification and physical defense reduction at 10+ name-mark stacks.
- Implement G `봉인검식` as silence-target damage amplification, B mark application, silence duration trait, and critical chance trait.
- Implement H `처형 준비` as Extermination Permit duration, action-speed, attack-power, and mark-target damage expansion.
- Implement I `연쇄 참결` as Black Ledger area-damage vulnerability and D cooldown charge after area damage.
- Implement J `사형 집행인` as Final Sentence kill cooldown charge and survivor damage vulnerability.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega F-J behavior in Play Mode, especially B mark stacking, D area vulnerability, and E kill/survivor branches.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Source references read: `f-deep-engraving.md`, `g-sealing-sword-form.md`, `h-execution-prep.md`, `i-chain-cleaving.md`, and `j-executioner.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:725` adds B passive mark-stack resolution.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:798`, `:846`, and `:862` apply Vega passive damage, defense-reduction, and critical-chance modifiers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:922`, `:939`, and `:963` add I/J target vulnerability and J cooldown-charge helper paths.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:88` through `:91` add Vega I/J enemy timer/bonus state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:909` and `:914` display the new Vega vulnerability labels.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:48` through `:52` mark Vega F-J as `RuntimeImplemented`.
- `Pakuri/Assets/Data/GameData/Monsters/vega.asset:434`, `:470`, `:505`, `:540`, and `:579` mark Vega F-J as `ImplementationState: 2`.
- Unity-MCP `execute_code` synced the CSV runtime catalogs and returned `vega-f` through `vega-j` as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP console error query returned MCP client handler logs only, not C# compile errors from this change.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\CombatRuntimeVegaSkills.cs Pakuri\Assets\Scripts\Combat\CombatRuntimeController.cs Pakuri\Assets\Scripts\Combat\CombatRuntimeEnemies.cs Pakuri\Assets\CSVdata\source\monster_skills.csv Pakuri\Assets\Data\GameData\Monsters\vega.asset` completed with CRLF warnings only.

### History

- 2026-05-07: User requested Vega passive skill F-J implementation after the A-E active pass.
- 2026-05-07: Code Builder implemented the passive runtime behavior, updated Vega data state, synced Unity runtime catalog output, and completed local build/Unity-MCP validation.

## Task: 2026-05-07 Vega Projectile Sprite CSV Runtime Fix

### Task title

Fix Vega runtime projectile sprite source after SO-only sprite assignment was not reflected in Play Mode.

### Goals

- Make Vega runtime projectile visuals use the same sprite assigned in `Assets/Data/GameData/Monsters/vega.asset`.
- Keep the active CSV runtime source and generated runtime asset catalog aligned.

### Constraints

- Role Owner is Code Builder after Designer confirmed the data-source mismatch.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega projectile visuals in Play Mode.
- Keep future Vega unit/projectile sprite changes in `Pakuri/Assets/CSVdata/source/monsters.csv`, then sync the CSV runtime catalog.

### Evidence

- Unity-MCP `execute_code` first reported Vega SO projectile path `Assets/Image/Monster/Vega/Vega_Shoot2.png` while runtime path was `Assets/Image/Monster/Vega/Vega_Shoot_Temp.png`.
- `Pakuri/Assets/CSVdata/source/monsters.csv:7` now uses `Assets/Image/Monster/Vega/Vega_Shoot2.png` for Vega projectile sprite path.
- Unity-MCP `execute_code` imported `monsters.csv`, ran `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()`, and then reported runtime Vega projectile path `Assets/Image/Monster/Vega/Vega_Shoot2.png`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset:33` now contains `Assets/Image/Monster/Vega/Vega_Shoot2.png`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP console error query returned existing missing-script and MCP client-handler logs, not C# compile errors from this change.

### History

- 2026-05-07: User reported that the newly assigned Projectile Sprite in Vega SO was not applied and the old sprite still appeared.
- 2026-05-07: Code Builder aligned the CSV runtime sprite path and regenerated the runtime asset catalog.

## Task: 2026-05-07 Vega B Target Rectangle Correction

### Task title

Change Vega B from Vega-origin line damage to target-centered instant rectangle damage.

### Goals

- Stop B from drawing and damaging along a line from Vega to the enemy.
- Make B pick an enemy target and immediately apply area damage in a temporary 3 by 1 rectangle centered on that target.
- Preserve existing B damage, silence, name-mark, passive, and cooldown behavior where applicable.

### Constraints

- Role Owner is Code Builder after Designer handoff.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega B in Play Mode: no Vega-origin line, immediate rectangle effect on the target, and silence/name-mark application on enemies inside the 3 by 1 area.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before this correction, `CombatRuntimeVegaSkills.cs` `TryCastVegaSilentGreatblade()` called `ApplyVegaLineSlash(...)`, which used `CreateVegaLineVisual(...)` and `IsPointInsideBeam(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:12` and `:13` now define `VegaSilentGreatbladeAreaWidth = 3f` and `VegaSilentGreatbladeAreaHeight = 1f`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:207` now calls `ApplyVegaTargetRectangleSlash(...)` at the target enemy position.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:422` through `:441` applies immediate damage, silence, and name marks only to enemies inside the target-centered rectangle.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:595` creates the rectangular visual through `CreateVegaRectangleVisual(...)`, not from Vega's position.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:919` adds rectangle containment through `IsPointInsideVegaRectangle(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` removed the now-unused delayed Vega silence field from `SkillEffectRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP script refresh reached idle and console error query returned only MCP client-handler logs.

### History

- 2026-05-07: User reported Vega B should not connect a line from Vega to the enemy and requested an immediate temporary 3 by 1 rectangular effect on the enemy.
- 2026-05-07: Code Builder changed Vega B to target-centered rectangle damage and removed the leftover delayed line/silence state.

## Task: 2026-05-08 Manifested Vega A Three-Sword Follow-up

### Task title

Make Manifested Vega use `vega-a` as three sequential sword projectiles.

### Goals

- Preserve Vega A reference behavior when Vega is Manifested into the party.
- Fire three projectiles per A magazine shot.
- Apply the third projectile's 2x damage and Vega name-mark stacks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Vega A in RunScene Play Mode.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md` defines Vega A as three projectiles, 0.12 second bullet interval, third projectile 200% damage, magazine 5, and shot interval 0.55.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:728` detects Manifested `vega-a`.
- `CombatRuntimeParty.cs:747` through `:774` queues three Manifested Vega projectiles and applies 2x damage to the third projectile.
- `CombatRuntimeParty.cs:581`, `:625`, and `:627` carry and apply Vega name-mark stacks on Manifested projectile hits.
- Unity-MCP `execute_code` confirmed runtime catalog `vega-a` resolves as `MagazineProjectile`, magazine `5`, and shot interval `0.55`.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported Manifested Vega did not appear to reference Vega A's proper three-projectile basic attack.
- 2026-05-08: Code Builder added Vega-specific Manifested A projectile burst behavior.

## Task: 2026-05-08 Manifested Vega Name-Mark Guard

### Task title

Keep Manifested Vega A name marks while blocking non-Vega mark leakage.

### Goals

- Preserve Manifested Vega A's name-mark application from queued sword projectiles.
- Ensure the shared Manifested projectile path does not give non-Vega monsters Vega name marks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Vega A still applies marks, and non-Vega Manifested A attacks do not.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:554` passes 0 name-mark stacks for generic Manifested projectile fire.
- `CombatRuntimeParty.cs:617` gates stored `VegaNameMarkStacks` behind `IsManifestedVegaThreeSwordFlurry(skill)`.
- `CombatRuntimeParty.cs:1120` and `:1121` keep Manifested Vega A queued projectiles passing mark stacks.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported non-Vega Manifested A attacks appeared to leave Vega name marks.
