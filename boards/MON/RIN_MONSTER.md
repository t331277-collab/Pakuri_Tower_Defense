## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-08 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/RIN_MONSTER.md`.

# RIN_MONSTER

## Scope

Rin dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Rin file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Not populated yet.

## Task: 2026-05-24 Rin-A Master-2 On-Hit Lightning Revision

### Task title

Revise `rin-a-master-2` from projectile branch launch behavior to shared on-hit Lightning additional damage and every-third-hit chain damage.

### Goals

- Make every Rin-A primary hit apply Lightning additional damage equal to 40% of the resolved hit damage.
- Make every 3rd Rin-A primary hit chain Lightning damage equal to 40% of the resolved hit damage to up to 2 enemies near the hit target.
- Keep the behavior on a shared on-hit damage extension usable by projectile, beam, zone, and single-attack hit paths.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User provided the parsed behavior values in the request.
- No Rin-only hardcoded runtime branch was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-A master-2 applies the direct Lightning extra hit on each hit and chains to 2 nearby enemies every 3rd primary hit.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gives `rin-a-master-2` `on_hit_additional_damage_chance=1`, `on_hit_additional_damage_multiplier=0.4`, `on_hit_additional_damage_attribute=Lightning`, `on_hit_additional_damage_target=HitTarget`, `on_hit_chain_hit_period=3`, `on_hit_chain_target_count=2`, `on_hit_chain_search_radius=4.5`, `on_hit_chain_damage_multiplier=0.4`, and `on_hit_chain_damage_attribute=Lightning`.
- The same row now has blank `branch_chance_set`, `branch_count`, `branch_damage_multiplier`, `branch_launch_period`, and `branch_launch_chance_set`, so it no longer uses the projectile branch launch override path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillOnHitAdditionalDamageUtility.cs` applies shared direct on-hit extra damage and every-nth-hit chain damage.
- Unity-MCP editor execution returned `rin-a-master-2|extra=True:1:0.4:Lightning:HitTarget|chain=3:2:4.5:0.4:Lightning|branch=False:False:0:False`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-24: User clarified that master-2 should be on-hit Lightning additional damage plus every-third-hit chain damage, not projectile branch launch chance behavior.

## Task: 2026-05-24 Rin-A Choice Runtime Completion

### Task title

Implement Rin-A remaining enhancement and master choice data on the shared projectile path.

### Goals

- Move `rin-a-trait-5` from partial support to shared projectile critical bonus fields.
- Keep `rin-a-master-1` on existing shared damage, magazine, and shot-interval fields.
- Implement `rin-a-master-2` with shared projectile branch behavior plus every-third-projectile-launch chance override.
- Use `Assets/Prefab/Skill/Rin/Rin_A.prefab` for Rin-A master-2 effect prefab resolution.

### Constraints

- Role Owner is Skill Builder.
- User explicitly approved treating current CSV/code as the parsed source.
- Implementation stayed on the selected projectile blueprint common path and did not add Rin-only hardcoded runtime logic.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-A trait 5 applies critical chance/damage bonuses.
- User verifies in Play Mode that Rin-A master 2 branches with 40% chance normally and 100% chance every 3rd projectile launch.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now includes `branch_launch_period` and `branch_launch_chance_set`.
- `rin-a-trait-5` now has `crit_chance_bonus=0.1`, `crit_damage_bonus=0.25`, blank `damage_multiplier`, and `runtime_support_state=RuntimeImplemented`.
- `rin-a-master-1` remains data-authored with `damage_multiplier=1.12`, `magazine_bonus=6`, and `shot_interval_multiplier=0.8200000000000001`.
- `rin-a-master-2` now has `skill_effect_prefab_path=Assets/Prefab/Skill/Rin/Rin_A.prefab`, `branch_chance_set=0.4`, `branch_count=2`, `branch_damage_multiplier=0.4`, `branch_search_radius=4.5`, `branch_launch_period=3`, `branch_launch_chance_set=1`, and `runtime_support_state=RuntimeImplemented`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `Assets/Prefab/Skill/Rin/Rin_A.prefab` with GUID `19bfba788239eba498a44cb67c2622c6`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-24: User authorized current CSV/code as parsed source for Rin-A master-2, remaining enhancements, and master-1 implementation.

## Task: 2026-05-19 Rin-A Shared Projectile Wiring

### Task title

Wire `rin-a` into the current shared projectile runtime and common modifier table.

### Goals

- Bind `rin-a` base projectile visuals through the active `EffectManager` scene mapping.
- Keep `rin-a` on the shared `MagazineProjectile` runtime path.
- Add the common projectile-compatible Rin-A choice modifiers to `SkillChoiceModifierData.csv`.
- Leave unsupported crit-only or sequence-state behavior explicitly unsupported instead of guessing new monster-only runtime logic.

### Constraints

- Role Owner is Code Builder.
- Claims are based on inspected Scripts2 runtime code, active scene YAML, active modifier CSV, and the inspected `Rin_A.prefab` asset path provided by the user.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by file inspection and build.

### Next Actions

- User verifies in Play Mode that `rin-a` now spawns `Rin_A.prefab` through the shared projectile path.
- If full `rin-a-trait-5` crit modifiers or `rin-a-master-2` extra lightning / every-third-hit chain are required in Scripts2 runtime, request a shared extension or a one-off approved exception before implementing.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Rin/Rin_A.prefab` exists and its prefab GUID is `19bfba788239eba498a44cb67c2622c6`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps monster `rin` skill `rin-a` to `Rin_A.prefab` through the `EffectManager` `monsterSkillEffects` list.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` already maps projectile active rows into `ProjectileSkillData`, including magazine size, reload, shot interval, projectile speed, pierce count, and on-hit status.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` already routes `ProjectileSkillData` through `ProjectileSkillExecutor`, resolves base visuals through `EffectManager.ResolveMonsterSkillEffectPrefab(...)`, and applies modifier snapshot bonuses for additional projectiles and pierce.
- `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` now includes common-path `rin-a` rows for trait 1/2/3/4 and master 1, while trait 5 and master 2 are marked `DataOnlyUnsupported` because current shared projectile runtime has no crit modifier fields and no built-in every-third-hit chain behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-19: User requested Code Builder implementation of `rin-a`.
- 2026-05-19: User clarified the base effect prefab path as `Assets/Prefab/Skill/Rin/Rin_A.prefab`.

## Task: 2026-05-14 Rin NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Rin prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Rin_Unit` through `NewRunSceneEntryManager`.
- Verify Rin creates an exact `rin` runtime model and initializes `MonsterUnitActor`.
- Make Rin's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Rin combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified. 2026-05-18 Rin active skill CSV rows were updated to the new skill-owned projectile/status schema. 2026-05-18 Rin design-only labels remain non-runtime statuses with `status_chance=0`.

### Next Actions

- User verifies Rin selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Rin_Unit.prefab` in `rinUnitPrefab`.
- Unity-MCP verification returned `rin:prefab=Rin_Unit|modelOk=True|model=rin|actor=True|actorModel=True|hpText=HP 260/260|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Rin_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `rin-a` `projectile_speed=13`, `pierce_count=0`, `magazine_capacity=10`, `reload_seconds=4`, and `shot_interval_seconds=0.34`, matching `Pakuri/reference/2.Monster/rin/skill/a-shattering-fist.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `rin-c` `radius=1.6` and `status_effect_label=넉백`, matching `Pakuri/reference/2.Monster/rin/skill/c-shockwave.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores Rin design-only labels `행동속도 증가` and `넉백` with `status_chance=0`; runtime CSV validation rejects positive chance on unsupported status labels.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` can still resolve supported labels such as `감전` from `status_effect_label` if a Rin row is intentionally edited to use a supported status later.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.
- 2026-05-18: Code Builder moved Rin projectile/status tuning into the skill CSV row and filled Rin-C width from the reference document.
- 2026-05-18: Code Builder normalized Rin design-only status labels to chance 0 and added supported status-label fallback/CSV sync batch support.

## Task: 2026-05-18 Rin-E SingleAttack Runtime Kind

### Task title

Route Rin-E collapse strike through the new SingleAttack runtime kind.

### Goals

- Keep Rin-E as one-shot area damage rather than sustained `AreaAttack`.
- Preserve CSV-authored damage, coefficient, radius, and cooldown.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Rin-E applies one immediate area hit.

### Evidence

- `Pakuri/reference/2.Monster/rin/skill/e-collapse-strike.md` names Rin-E `붕괴 타격`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `rin-e runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV row 17 as a one-shot area attack skill for the new `SingleAttack` type.

## Task: 2026-05-13 Rin Battlefield Facade Registration

### Task title

Route Rin battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Rin skill behavior while replacing direct battlefield list registration writes.
- Keep Rin projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin skills in Play Mode if needed.

### Evidence

- `CombatRuntimeRinSkills.cs:575` now calls `AddBattlefieldProjectile(...)`.
- Rin skill-effect registrations now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Rin battlefield object registration through facade methods.

## Task: 2026-05-08 Rin CombatUnitRuntime Parity Resume

### Task title

Route selected Rin and manifested Rin through shared unit skill runtime paths.

### Goals

- Make selected 1P Rin and manifested 2P-5P Rin call `CombatUnitRuntime` plus `CombatSkillRuntime` based execution for Rin B/C/D/E.
- Preserve Rin A magazine/projectile handling on the existing path.
- Keep manifested Rin Howling buff duration and Howling dark follow-up on the unit runtime, not on selected-only fields.
- Reuse existing RunScene slot status children for manifested monster name, HP text, and HP/shield bars.

### Constraints

- Role Owner is Code Builder.
- Claims are based on inspected files, Unity-MCP scene hierarchy output, and command output.
- Unity Play Mode verification is user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build, Unity refresh, and console checks.

### Next Actions

- User verifies selected Rin and manifested Rin B/C/D/E behavior in RunScene Play Mode.
- User verifies 2P-5P monster status UI does not duplicate labels or bars when manifested monsters appear.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:76` defines `TickSelectedRinUnitSkillRuntimes(...)` for selected Rin skill runtime ticking.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:128` routes Rin automatic skill execution through `TryTriggerRinUnitAutomaticSkills(CombatUnitRuntime runtime)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:240`, `:321`, and `:401` implement unit-runtime casts for Rin B, Rin D, and Rin E; Rin C is routed through the same unit skill tick and manifested shockwave path.
- `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs:15` through `:18` stores separate name label, HP label, HP bar fill, and shield bar fill references.
- `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs:25`, `:59`, `:104`, and `:128` store, tick, and reset manifested Rin Howling state on the unit runtime.
- Unity-MCP scene hierarchy inspection found `CombatRoot/2PMonster`, `3PMonster`, `4PMonster`, `5PMonster`, and `EveUnit`; 2P/3P/Eve children included `MonsterHpLabel`, `MonsterHpBar/Fill`, `MonsterHpBar/Shield`, and `MonsterNameLabel`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP script refresh reached idle; console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-08: User resumed an interrupted request to start from Rin and make selected 1P and manifested 2P-5P monsters use the same `CombatUnitRuntime` plus `CombatSkillRuntime` execution basis.

## Task: 2026-05-08 Manifested Rin C Shockwave Parity Fix

### Task title

Make manifested Rin C apply selected Rin C beam and knockback behavior.

### Goals

- Fix manifested Rin C so it does more than visual line damage.
- Apply selected Rin C's map-wide beam hit shape, knockback, width choices, master slow, master lightning follow-up, and reload reduction behavior where applicable.
- Keep damage multiplier sourced through existing manifested Rin C choice multiplier logic.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Rin C knockback in RunScene Play Mode.
- User verifies Rin C master/trait choices if those choices are learned on the manifested Rin.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:220` through `:310` shows selected Rin C uses map-wide range, `IsPointInsideBeam(...)`, `ApplyRinKnockback(...)`, master lightning follow-up, master slow, and trait reload reduction.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:499` routes manifested `rin-c` into `TryFireManifestedRinShockwave(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:545` implements the manifested Rin C beam path using selected-runtime helper methods and manifested Offering checks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:627` reduces manifested Rin A reload when manifested Rin C trait 5 hits while Rin A is reloading.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported selected Rin C knockback works, but manifested Rin C only showed effect/beam without moving enemies.

## Task: 2026-05-08 Manifested Rin Common Runtime Parity

### Task title

Apply Rin Offering choices through manifested projectile and common skill runtime.

### Goals

- Keep manifested Rin skills sourced from `SkillDefinition` data.
- Apply Rin manifested Offering choices in shared damage, cooldown, magazine, reload, and shot interval paths.
- Preserve manifested projectile/status handling through the common combat service.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Rin private skill code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Rin skills and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Rin skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Rin cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1250`, `:1278`, and `:1310` include Rin A magazine/reload/shot-interval choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:693` applies manifested projectile status from skill data.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: Manifested Rin common runtime parity was implemented and retained as the latest active Rin task block during MON board compaction.

## Required Sections For Future Work

- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Task: 2026-05-08 Manifested Rin Passive And Targeting Continuation

### Task title

Make manifested Rin use Rin passive skill runtime effects and participate as an enemy target.

### Goals

- Apply Rin F-J passive effects to manifested Rin A/C/D/E runtime paths through `CombatUnitRuntime`.
- Keep manifested Rin cooldown ticking affected by Rin action-speed passives.
- Fix missing manifested HP slide bar fallback.
- Allow enemies to target and damage manifested Rin and other manifested monsters.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification is user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build, diff check, Unity refresh, and console read.

### Next Actions

- User verifies in RunScene Play Mode that manifested Rin gets passive effects from Offering, has one HP bar, and can be attacked by enemies.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:197` ticks manifested Rin unit skill cooldowns with `GetRinUnitActionSpeedMultiplier(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1073` adds `TryApplyRinUnitProjectileHit(...)` for manifested Rin projectile damage with unit passive modifiers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1269` tracks manifested Rin physical hit count for Rin H.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1848` implements manifested Rin action-speed passive calculation.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:793` routes manifested Rin C damage through `ApplyRinUnitSkillDamage(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `git diff --check` over touched combat files completed with exit code 0 and CRLF warnings only.
- Unity-MCP script refresh requested compilation; console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-08: User requested resuming work so manifested Rin gains passive skills like selected Rin, manifested monsters have HP slide bars, and enemies attack manifested monsters too.
