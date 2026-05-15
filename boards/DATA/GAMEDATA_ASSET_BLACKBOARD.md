## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-08` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.

## Task: 2026-05-15 Phase4-C-0 Skill Prefab Actor Asset Wiring

### Task title

Record skill prefab actor components and NewRunScene serialized references for Phase4-C-0.

### Goals

- Attach the shared projectile actor component to the Eve-A skill prefab.
- Attach the shared attached-effect actor component to the Ariel-B skill prefab.
- Serialize NewRunScene prefab references for Eve-A projectile and Ariel-B shield visual.
- Keep prefabs as presentation/runtime actor carriers, not owners of skill formulas.

### Constraints

- Role Owner is Code Builder.
- User-authored prefab/image assets under `Assets/Prefab/Skill` and `Assets/Image/Monster` are preserved as authored assets.
- No broad ScriptableObject migration or new generated skill data asset was created.
- Ariel-A was not connected because the inspected minimum `SkillData.csv` rows include `eve-a` and `ariel-b`, not `ariel-a`.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Keep future skill prefabs under `Pakuri/Assets/Prefab/Skill/{Monster}` and attach shared actor components where needed.
- Add data validation that reports a missing prefab reference before skill execution falls back to direct damage.
- Add final collider/hitbox authoring rules after enemy prefab colliders are decided.

### Evidence

- `Assets/Prefab/Skill/Eve/Eve_A.prefab` has `Pakuri.InGame.InGameProjectileActor` and a trigger `BoxCollider2D`.
- `Assets/Prefab/Skill/Ariel/Ariel_B.prefab` has `Pakuri.InGame.InGameAttachedSkillEffectActor`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes `eveAProjectilePrefab`, `arielBShieldEffectPrefab`, and `projectileDestroyBoundary` on `GameManager`'s `InGameCombatManager`.
- `Pakuri/Assets/Legacy/Data/GameData/Monsters/ariel.asset` updates `ariel-b` `BaseDamage` to `35` so the current legacy bridge supplies the same shield base as the CSV seed row.
- Runtime/editor builds passed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed no C# compile errors.

### History

- 2026-05-15: Phase4-C-0 connected user-authored Eve-A and Ariel-B skill prefabs to the new shared actor components and NewRunScene references.

## Task: 2026-05-15 InGame Phase4-B Skill Choice Snapshot Data Flow

### Task title

Record Phase4-B use of choice modifier CSV data in the skill execution contract.

### Goals

- Parse optional `SkillChoiceModifierData.csv` text into runtime modifier records.
- Apply modifier records to `SkillExecutionSnapshot` only when the unit has matching `ChosenChoiceIds`.
- Keep base `SkillData` and CSV source rows immutable during execution.

### Constraints

- Role Owner is Code Builder.
- `SkillChoiceData.csv` remains metadata only in this slice; runtime code currently parses modifier rows only.
- No actual skill effects, projectile assets, ScriptableObject assets, or data migration were implemented.
- New broad character choice data entry is still deferred.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Add validation around modifier column ranges before broad data entry.
- Connect minimum Phase4-C effect execution after confirming snapshot values drive executor behavior.

### Evidence

- Added `SkillChoiceModifierCsvParser.cs`, `SkillChoiceModifierLibrary.cs`, and `SkillChoiceModifierRecord.cs`.
- `SkillExecutionSnapshot.cs` stores explicit choice-modified fields including damage, magazine, additional projectile, pierce, reload/shot interval multipliers, branch fields, and status fields.
- `SkillChoiceResolver.cs` applies both existing `SkillData.EnhancementChoices` / `MasterChoices` and parsed `SkillChoiceModifierRecord` rows from chosen choice IDs.
- `NewRunScene.unity` links `InGameCombatManager.skillChoiceModifierCsv` to `Assets/CSVdata/SkillChoiceModifierData.csv`.
- Runtime and editor builds passed with 0 errors and existing warnings.

### History

- 2026-05-15: Phase4-B connected the Eve-A choice modifier CSV seed to the execution snapshot path.

## Task: 2026-05-15 Eve-A Skill Choice Data Assets

### Task title

Record Eve-A CSVData skill choice and modifier asset additions.

### Goals

- Add asset-side CSV files that describe Eve-A enhancement/master choices and numeric modifiers.
- Keep `SkillData.csv` as the base Eve-A skill row and store choice deltas separately.
- Preserve explicit modifier columns for future authoring and validation.

### Constraints

- Role Owner is Code Builder.
- CSV assets only; no ScriptableObject assets, prefabs, scene assets, loader code, or executor code were changed.
- `null` marks unused modifier fields until a parser/validator is implemented.
- Current `Scripts2/InGame` still uses the existing legacy data bridge and does not yet consume the new choice modifier CSV files.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Implement parser/validator support before making these choice modifier files authoritative.
- Keep broad 5-character choice data expansion deferred until the Eve-A sample schema is exercised by Phase4-B/C.

### Evidence

- Added `Pakuri/Assets/CSVdata/SkillChoiceData.csv`.
- Added `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv`.
- Added `Pakuri/Assets/CSVdata/SkillChoiceData.csv.meta` and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv.meta`.
- `SkillChoiceData.csv` contains Eve-A choices from `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` sections 4 and 5.
- `SkillChoiceModifierData.csv` contains the structured Eve-A deltas for damage, magazine, reload multiplier, shot interval multiplier, pierce, additional projectiles, branch chance/count/damage/radius, and shock stack set.
- PowerShell `Import-Csv` parsed both files and confirmed seven choice rows and seven modifier rows.
- Unity-MCP asset refresh completed with editor state idle.

### History

- 2026-05-15: User approved explicit modifier columns and requested Code Builder to create the Eve-targeted choice CSV files.

## Task: 2026-05-15 InGame Phase4-A Skill Blueprint Runtime Split

### Task title

Record the data/runtime split for Phase4-A skill runtime state.

### Goals

- Keep `SkillData` and its subclasses as blueprint data.
- Store combat-time mutable skill state outside the `SkillData` objects.
- Activate only learned active skill IDs copied from run state into `UnitStateBucket`.

### Constraints

- Role Owner is Code Builder.
- No CSV rows, ScriptableObject assets, prefab assets, or scene assets were changed in this slice.
- `InGameSkillCatalog` remains the current bridge from existing data definitions into transient InGame `SkillData`.
- No skill effects, projectile assets, damage values, or shield formulas were executed.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase4-B should keep `SkillData` unmutated and calculate choice-modified execution snapshots from `ChosenChoiceIds`.
- Phase4-C should verify sample skills through `InGameCombatManager.ApplyDamage(...)` and `GrantShield(...)`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` already exposes `Timing`, `Targeting`, `EnhancementChoices`, and `MasterChoices`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/ProjectileSkillData.cs` exposes `Projectile.MagazineSize` and `Projectile.ReloadTime`.
- Added `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs`, `UnitSkillRuntimeSet.cs`, and `SkillRuntimeFactory.cs`.
- `SkillRuntimeInstance` reads timing and projectile magazine/reload data but does not call damage, shield, projectile, or effect APIs.
- `SkillRuntimeFactory` uses `InGameSkillCatalog.TryGetActiveSkill(...)` and `UnitStateBucket.LearnedActiveSkillIds` to decide which active skills become runtime instances.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-15: Phase4-A created runtime state around the existing skill blueprint data bridge without changing source data.

## Task: 2026-05-15 Stage1 Enemy Data And Prefab Binding Expansion

### Task title

Record data and asset-side changes for stage-one Warrior, Rogue, and Priest enemy entry spawning.

### Goals

- Store Rogue and Priest data alongside the existing stage-one swordsman row.
- Align current legacy data with the user-confirmed `Buffer` priest type.
- Bind the newly authored Rogue and Priest prefabs to NewRunScene entry spawning.

### Constraints

- Role Owner is Code Builder.
- Prefab visual authoring remains user-owned; Code Builder only verified and serialized references.
- No enemy behavior execution for ranged attacks, heals, shields, buffs, or damage was implemented.
- Do not run Unity Play Mode.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in Play Mode that all three enemy prefabs appear and initialize their actor HP/name views.
- Later data-loader work should bridge `Assets/CSVData/EnemyStat.csv` into runtime definitions before declaring the new CSVData path authoritative.

### Evidence

- `Pakuri/Assets/CSVData/EnemyStat.csv` has rows for `stage1-swordsman`, `stage1-rogue`, and `stage1-priest` using `Melee`, `Ranged`, and `Buffer`.
- `Pakuri/Assets/Legacy/Data/GameData/Enemies/stage1-priest.asset` now stores `AttackType: 3`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes `stageOneEnemyPrefab` as GUID `f2892daa44e860e49b1ea2b17f8682dc`, `stageOneRangedEnemyPrefab` as GUID `41679be8164df3d4fa09bc87aa4c1fb7`, and `stageOneBufferEnemyPrefab` as GUID `1fd46dd360e001847b2d0e89ded130d9`.
- Unity-MCP `manage_prefabs get_hierarchy` confirmed `Assets/Prefab/Enemy/Stage1_Rogue_Unit.prefab` and `Assets/Prefab/Enemy/Stage1_Priest_Unit.prefab` include `Pakuri.InGame.EnemyUnitActor` and the expected HP/name child objects.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-15: User stated that melee, Ranged, and buffer type enemy prefabs were added under `Assets/Prefab/Enemy` and requested they be connected through CSV data and NewRunScene entry spawning.

## Task: 2026-05-15 CSV Skill Numeric Map-Scale Contract

### Task title

Record how future CSV skill numeric fields should map to the visible NewRunScene coordinate scale.

### Goals

- Treat future CSV skill numeric tuning values as values authored for the visible map scale, not arbitrary screen pixels.
- Use X `0~31` and Y `0~17` as the current camera-visible coordinate baseline.
- Make CSV radius/range/area-width values line up with actual map distances during skill implementation.
- Preserve authored scene object `Transform` values as direct world/scene coordinates.

### Constraints

- Role Owner is Designer.
- No CSV row edits, data loader edits, ScriptableObject edits, scene edits, or runtime implementation in this task.
- Do not retroactively claim current CSV skill radius values are correct; this only records the required tuning baseline for future implementation.
- If a later CSV field needs a different unit, the field name or mapper must document that exception explicitly.

### Role Owner

Designer

### Status

Recorded as data tuning contract for future skill implementation.

### Next Actions

- When Code Builder implements CSVData Phase5 or skill execution, map fields such as radius, range, beam width, zone size, projectile travel distance, and enemy attack range to the X `0~31`, Y `0~17` visible-map scale.
- Keep fields derived from actual scene object positions, such as spawn point X/Y or target `Transform.position`, as raw Transform reads.
- Future validation or debug tools should report skill radii/ranges in map-coordinate units so Play Mode verification can compare them to the 31 by 17 visible area.

### Evidence

- User stated on 2026-05-15 that the current camera-visible map basis is X `0~31` and Y `0~17`.
- User stated that camera right edge should be treated as X `31`, and camera bottom-to-top height should be treated as Y `17`.
- User excluded actual object `Transform` reads from the CSV numeric tuning conversion rule.
- User specifically cited CSV skill radius-like numeric values as the kind of data that must be tuned to actual map coordinates during skill implementation.

### History

- 2026-05-15: User asked to record the visible-map coordinate baseline on the board before future skill implementation.

## Task: 2026-05-15 Stage1 Warrior Enemy Prefab Binding Handoff

### Task title

Record asset-side readiness for the first InGame enemy prefab binding.

### Goals

- Treat `Assets/Prefab/Enemy/Stage1_Warrior_Unit.prefab` as the first enemy presentation prefab for Phase2-B residual enemy binding.
- Keep enemy stats sourced from the existing `stage1-swordsman` data path, not from prefab-authored combat values.
- Preserve user-authored HP bar and label children as presentation assets.

### Constraints

- Role Owner is Designer for this handoff; no prefab, data, or code edits were made.
- Prefab authoring is user-owned; Code Builder should add binding APIs and runtime model injection only.
- Movement/attack/skill tuning remains later skill/combat-loop work.

### Role Owner

Designer -> Code Builder

### Status

Builder implementation completed and locally verified for Phase2-B enemy prefab/model binding.

### Next Actions

- User verifies in Play Mode that the authored enemy prefab presentation appears and the HP/name debug display initializes.
- Later combat work should mutate `EnemyUnitRuntimeModel.Resources` and call `EnemyUnitActor.RefreshDebugView()` after damage/shield changes.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Prefab/Enemy/Stage1_Warrior_Unit.prefab:530` contains `m_Name: Stage1_Warrior_Unit`.
- `Pakuri/Assets/Prefab/Enemy/Stage1_Warrior_Unit.prefab:624` contains `m_EditorClassIdentifier: Assembly-CSharp::Pakuri.InGame.EnemyUnitActor`.
- Unity-MCP `manage_prefabs get_hierarchy` found `MonsterHpBar/Background`, `MonsterHpBar/Fill`, inactive `MonsterHpBar/Shield`, `MonsterHpLabel`, and `MonsterNameLabel` under the enemy prefab.
- `boards/COMBAT/ENEMY_BLACKBOARD.md` already records `stage1-swordsman` seed data in `EnemyStat.csv` with HP `100`, attack `12`, and physical defense `5`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs:29` defines the existing enemy model creation API.
- Updated `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` so the prefab actor stores `EnemyUnitRuntimeModel`, resolves `MonsterNameLabel`, `MonsterHpLabel`, `MonsterHpBar/Background`, `Fill`, and `Shield`, and updates text/fill scale from model identity/resources.
- Updated `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` so the enemy prefab is a serialized `stageOneEnemyPrefab` reference and the enemy model is created from `initialEnemyId`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:686` stores `stageOneEnemyPrefab` as `Stage1_Warrior_Unit.prefab` GUID `f2892daa44e860e49b1ea2b17f8682dc`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- `git diff --check` on changed scripts, scene, and boards completed with exit code 0, aside from LF-to-CRLF normalization warnings.

### History

- 2026-05-15: User stated the enemy prefab now has HP and `EnemyUnitActor.cs` assigned and is ready for Phase2-B enemy spawn work.
- 2026-05-15: Code Builder implemented enemy actor/model binding and serialized prefab linkage for the first NewRunScene enemy spawn.

## Task: 2026-05-14 Five Monster Prefab HP Bar Asset Binding

### Task title

Record five monster prefab HP bar sprite and fallback catalog binding.

### Goals

- Give `MonsterHpBar` SpriteRenderers a real sprite asset so they are visible.
- Store all five player-unit prefab references on the NewRunScene entry manager.
- Use an assigned `GameDataCatalog` fallback when CSV runtime source loading is unavailable.

### Constraints

- Role Owner is Code Builder.
- No CSV row or monster definition value changes.
- No Play Mode verification from Codex.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Keep `MonsterHpBarPixel.png` as a shared placeholder presentation asset until final UI art is available.
- If CSV runtime source import is restored, reconfirm the fallback path still resolves the same five monster IDs.

### Evidence

- Added `Pakuri/Assets/Prefab/Monster/MonsterHpBarPixel.png` with sprite importer metadata.
- `Select-String` over the five prefabs found `Background`, `Fill`, and `Shield` SpriteRenderers with non-empty `m_Sprite` references, sorting orders 34/35/36, and visible HP/shield colors.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now references `Assets/Legacy/Data/GameData/GameDataCatalog.asset` as `fallbackCatalog`.
- Unity-MCP verification returned `modelOk=True` and exact model IDs for `ariel`, `eve`, `sein`, `vega`, and `rin`.

### History

- 2026-05-14: User reported `MonsterHpBar` was not visible and asked to verify all five prefab bindings.

## Task: 2026-05-14 NewRunScene Phase2-B Runtime Model Binding

### Task title

Record data and asset ownership for selected monster actor/model binding.

### Goals

- Keep selected monster stats and learned state sourced from the existing CSV/Data runtime catalog and `RunSession`.
- Keep unit prefabs as presentation/scene assets.
- Bind the runtime model to the prefab actor after instantiation.

### Constraints

- Role Owner is Code Builder.
- No CSV rows, ScriptableObject data assets, or prefab contents were edited in this slice.
- `MonsterUnitRuntimeModel` is not a prefab-assignable `MonoBehaviour`; it is created from runtime data and passed to `MonsterUnitActor`.
- Code Reviewer was not run because the user did not explicitly request Reviewer execution for this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User assigns Ariel/Sein/Vega/Rin prefab fields on `NewRunSceneEntryManager` manually, as requested.
- Later data work should continue using `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)` for selected monster definitions.
- Combat HP/shield mutation should update the bound runtime model, then refresh the actor debug view.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` resolves `GameDataCatalog`, `MonsterDefinition`, `RunSession`, and `MonsterUnitRuntimeModel` before prefab actor initialization.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` stores the `MonsterUnitRuntimeModel` and reads `Identity`, `Stats`, and `Resources` for debug display.
- Unity-MCP editor code execution returned `modelMonster=eve|modelHp=220|learnedA=1` while inspecting `Assets/Scenes/NewScene/NewRunScene.unity` and `Assets/Prefab/Monster/Eve_Unit.prefab`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User asked to start Phase2-B after adding Eve prefab debug HP/name children and `MonsterUnitActor`.

## Task: 2026-05-14 Eve Unit Prefab Entry Binding

### Task title

Bind the existing Eve unit prefab to the NewRunScene entry manager.

### Goals

- Use the current `Assets/Prefab/Monster/Eve_Unit.prefab` as the first NewRunScene 1P prefab.
- Store the binding on the scene manager component instead of hardcoding an asset load path.

### Constraints

- Role Owner is Code Builder.
- No prefab content editing, model binding, combat behavior, or Play Mode verification.
- Code Reviewer was explicitly skipped by the user.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Add prefab bindings for Ariel, Sein, Vega, and Rin after their prefabs exist under `Assets/Prefab/Monster`.
- Bind spawned prefabs to InGame actor/model scripts in the next implementation slice.

### Evidence

- `Get-ChildItem Pakuri\Assets\Prefab\Monster` found `Eve_Unit.prefab`.
- `Pakuri/Assets/Prefab/Monster/Eve_Unit.prefab.meta` has prefab GUID `768bb9d217c3cc64a84cd7059fe5e154`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references that prefab GUID in `NewRunSceneEntryManager.eveUnitPrefab`.
- Unity-MCP read-only component inspection returned `prefab=Eve_Unit`.

### History

- 2026-05-14: User requested spawning the existing Eve shell prefab at `1PSpawnPoint` during NewRunScene entry.

## Task: 2026-05-14 CSVData Phase0-2 Seed Rows

### Task title

Implement CSVData Phase0~2 headers and minimum seed rows.

### Goals

- Define the first schema headers for `MonsterStat.csv`, `EnemyStat.csv`, and `SkillData.csv`.
- Add minimum seed rows for Eve, Ariel, `stage1-swordsman`, `eve-a`, and `ariel-b`.
- Preserve evidence for values that come from reference documents and values copied from current project data because the reference page does not list them.

### Constraints

- Role Owner is Code Builder.
- No C# loader, ScriptableObject asset, prefab, scene, or Play Mode changes.
- Values must be traceable to inspected reference files, inspected current project data, or explicit `source_notes` in the CSV rows.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Implement the CSVData loader and unit/skill mapping in the later CSVData Phase3~5 slices.
- Before making CSVData authoritative, remove or isolate `Scripts2/InGame` references to legacy `Pakuri.Data` types.
- Revisit monster base stat source ownership because Eve and Ariel reference pages do not list base HP values; the current seed rows mark those values as current project data.

### Evidence

- Updated `Pakuri/Assets/CSVData/MonsterStat.csv` with Eve and Ariel rows.
- Updated `Pakuri/Assets/CSVData/EnemyStat.csv` with `stage1-swordsman`.
- Updated `Pakuri/Assets/CSVData/SkillData.csv` with `eve-a` and `ariel-b`.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` provides `eve-a` damage, coefficient, projectile, magazine, reload, and status values.
- `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md` provides `ariel-b` shield, coefficient, duration, cooldown, target, and refresh values.
- `Pakuri/reference/5.enemy/stage-1-enemies.md` provides `stage1-swordsman` stat, defense, active skill, and passive values.
- `Pakuri/reference/3.combat/combat-stat-system.md` and `combat-attribute-and-damage-system.md` provide default attack/spell and critical baseline context.
- `Import-Csv Pakuri\Assets\CSVData\MonsterStat.csv` returned rows for `eve` and `ariel`.
- `Import-Csv Pakuri\Assets\CSVData\EnemyStat.csv` returned `stage1-swordsman` with HP `100`, attack `12`, physical defense `5`, and active skill `베기`.
- `Import-Csv Pakuri\Assets\CSVData\SkillData.csv` returned `eve-a` as `ProjectileSkillData` and `ariel-b` as `ShieldSkillData`.

### History

- 2026-05-14: User explicitly assigned Code Builder to proceed with CSVData Phase0~2 and use `reference/2.Monster`, `reference/5.enemy`, and `reference/3.combat` for minimum sample names and values.

## Task: 2026-05-14 Eve-E Field Data Implementation

### Task title

Implement Eve-E source and asset data as a field skill.

### Goals

- Update the source CSV row for Eve-E from `MagazineProjectile` to `Field`.
- Update the current Eve ScriptableObject asset to match the source classification.
- Confirm the InGame mapper produces `ZoneSkillData`.

### Constraints

- Role Owner is Code Builder.
- No scene, prefab, combat executor, or Play Mode changes.
- Do not invent a zone radius because the inspected Eve-E reference did not provide one.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Fill Eve-E radius/placement tuning in a later skill execution/data pass when a numeric design value is available.

### Evidence

- Updated `Pakuri/Assets/CSVdata/source/monster_skills.csv` Eve-E row to `플라즈마 필드`, `Field`, `Lightning`, `CooldownSeconds` 5, `MagazineCapacity` 3, `ReloadSeconds` 6, and `ShotIntervalSeconds` 0.8.
- Updated `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` Eve-E changed wording from beacon/ice to Plasma Field/lightning where applicable.
- Updated `Pakuri/Assets/Data/GameData/Monsters/eve.asset` Eve-E serialized values to `RuntimeKind: 4`, `Attribute: 2`, `CooldownSeconds: 5`, and `ShotIntervalSeconds: 0.8`.
- Unity-MCP Editor code execution returned `skill=eve-e|name=플라즈마 필드|kind=Field|attr=Lightning|cooldown=5|mag=3|reload=6|interval=0.8|mapped=ZoneSkillData|zone=True|errors=0|warnings=0`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-14: Code Builder implemented the Eve-E field data classification requested by the user.

## Task: 2026-05-14 Eve-E ZoneSkillData Classification

### Task title

Record Eve-E data classification change to ZoneSkillData.

### Goals

- Track that Eve-E should be represented as a zone/field skill in the InGame skill data model.
- Keep the current CSV/data mismatch visible until Code Builder updates it.
- Avoid fixing the Reviewer `ShotIntervalMissing` issue by adding an Eve-only projectile validator exception.

### Constraints

- Role Owner is Designer.
- No CSV, asset, code, or scene edits in this design note.
- Current implementation evidence must come from inspected files.

### Role Owner

Designer

### Status

Design decision recorded; implementation pending.

### Next Actions

- Code Builder should update the Eve-E source row and generated/runtime data so `eve-e` maps to `ZoneSkillData`.
- Code Builder should align validation so zone skills require zone duration/tick/radius semantics, not projectile `ShotIntervalSeconds`.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` describes Eve-E as `플라즈마 필드` and a `장판형 설치 스킬`.
- `C:\TowerDefence_Pakuri\towerdefense_pakuri_docs\docs\dev\skill-class-design.md` lists Eve-E in the `ZoneSkillData` section.
- Before the Code Builder implementation in the task above, `Pakuri/Assets/CSVdata/source/monster_skills.csv` listed `eve-e` as `MagazineProjectile`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps `MagazineProjectile` to `ProjectileSkillData` and maps `AreaAttack` / `Field` to `ZoneSkillData`.

### History

- 2026-05-14: User clarified Eve-E is now a zone field skill, not the old drone projectile/summon classification.

## Task: 2026-05-14 InGame Phase2-A Definition To Unit Model Mapping

### Task title

Record data-side Phase2-A base unit model mapping.

### Goals

- Reuse existing CSV/Data runtime catalog resolution for unit model creation.
- Map `MonsterDefinition` and `EnemyDefinition` into `BaseUnitRuntimeModel` family classes without editing CSV rows or ScriptableObject assets.
- Preserve defense data by carrying `AttributeDefenseSet` values into InGame model state.
- Leave definition-owned skill/projectile tuning for later SkillData mapper implementation.

### Constraints

- Role Owner is Code Builder.
- No CSV edit, ScriptableObject asset generation, code-generated prefab changes, scene edits, or gameplay execution.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Later data work should keep using `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)` rather than adding a second InGame loader.
- Full skill data expansion remains deferred until execution paths are proven.
- User-authored prefabs under `Pakuri/Assets/Prefab/Monster`, `Enemy`, and `Skill` are the future prefab flow.

### Evidence

- `UnitFactory.TryCreatePhase2ATestModels(...)` uses `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)` and `PakuriDataManager` to resolve Eve and a stage-one enemy.
- `UnitFactory.TryCreatePhase2ATestModels(...)` returns `MonsterUnitRuntimeModel` and `EnemyUnitRuntimeModel`.
- `BaseUnitRuntimeModel.cs` contains common identity, stats, defenses, resources, and auto flags.
- `UnitDefenseRuntime.FromDefinition(...)` maps `AttributeDefenseSet` into runtime model defense fields.
- Unity-MCP Editor code execution returned `ok=True|monster=eve|monsterHp=220|learnedA=1|enemy=stage1-swordsman|enemyHp=100`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: Phase2-A added data-definition to unit-model mapping without modifying source data assets.
- 2026-05-14: User directed prefab creation to stay manual and skill/projectile tuning to move later through SkillData mapping.

## Task: 2026-05-14 InGame Phase1-D Skill Data Validation

### Task title

Record data-side validation for InGame skill Blueprint mapping.

### Goals

- Validate existing `MonsterDefinition.ActiveSkills` and `MonsterDefinition.PassiveSkills` before full InGame skill data expansion.
- Keep existing CSV/Data loading as the source of truth.
- Avoid creating or editing ScriptableObject assets in this phase.

### Constraints

- Role Owner is Code Builder.
- No CSV edits, asset generation, prefab changes, scene edits, or gameplay execution.
- Validator must read current data definitions and mapped `SkillData` without taking ownership of runtime skill execution.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Use `InGameSkillDataValidator.ValidateCatalog(...)` or the Unity menu `Pakuri/InGame/Validate Skill Data` before bulk skill data entry.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs`.
- Added `Pakuri/Assets/Scripts2/InGame/Editor/InGameSkillDataValidationMenu.cs`.
- The validator resolves the current catalog through `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)`, matching the Phase1-C data bridge decision to reuse existing CSV/Data loading.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-14: Code Builder added Phase1-D validation for existing data-to-InGame skill mapping.

## Task: 2026-05-14 Combat V2 Final Data Ownership Structure

### Task title

Record final Combat V2 data and asset ownership boundaries.

### Goals

- Keep existing CSV/Data loading as the source of `GameDataCatalog`, `MonsterDefinition`, `EnemyDefinition`, `SkillDefinition`, and `PassiveDefinition`.
- Keep V2 `SkillData` as the runtime Blueprint shape used by execution code.
- Keep prefabs as presentation/scene assets rather than owners of combat logic.

### Constraints

- Role Owner is Designer.
- No CSV edit, ScriptableObject asset creation, prefab creation, or data migration in this task.
- Full 50-skill data expansion remains deferred until validation and minimum execution paths are proven.

### Role Owner

Designer

### Status

Completed as data architecture context.

### Next Actions

- Phase1-D should validate mapped V2 skill data before full data entry.
- Later Code Builder work should continue reusing `PakuriCsvRuntimeData`, `GameDataCatalog`, and `PakuriDataManager` rather than adding a second CSV loader.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html`.
- `Pakuri/Assets/Scripts/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` builds `MonsterDefinition`, `EnemyDefinition`, active skills, passive skills, reward choices, and skill choices from CSV source rows.
- `Pakuri/Assets/Scripts/Data/Runtime/PakuriDataManager.cs` registers and resolves monsters, enemies, active skills, passive skills, skill choices, and reward choices.
- `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/CombatV2SkillCatalog.cs` reads the existing runtime catalog and current data manager.
- `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/CombatV2SkillDefinitionMapper.cs` maps `SkillDefinition` / `PassiveDefinition` into V2 `SkillData` subclasses.
- `Get-ChildItem Pakuri\Assets\Prefab -Directory` listed `Enemy`, `Monster`, and `Skill`; `Test-Path Pakuri\Assets\SO` returned `True`.

### History

- 2026-05-14: Designer documented the completed Combat V2 data ownership and asset storage responsibilities for the final ingame structure.

## Task: 2026-05-14 Combat V2 Prefab And SO Asset Locations

### Task title

Record Combat V2 prefab and ScriptableObject storage locations.

### Goals

- Use `Pakuri/Assets/Prefab` as the storage root for future monster/player-unit, enemy, and skill prefabs.
- Use `Pakuri/Assets/SO` as the storage root for future ScriptableObject data assets.
- Preserve existing `Prefab` subfolder organization for Combat V2 asset creation.

### Constraints

- Role Owner is Designer.
- No asset creation, folder creation, or file move in this task.
- Folder roles are recorded as the user's design intent, with existence verified by file checks.

### Role Owner

Designer

### Status

Recorded as asset-location context.

### Next Actions

- Future Code Builder asset tasks should place monster/player-unit prefabs under `Pakuri/Assets/Prefab/Monster`.
- Future Code Builder asset tasks should place enemy prefabs under `Pakuri/Assets/Prefab/Enemy`.
- Future Code Builder asset tasks should place skill/effect prefabs under `Pakuri/Assets/Prefab/Skill`.
- Future ScriptableObject data assets should be stored under `Pakuri/Assets/SO`.

### Evidence

- `Test-Path .\Pakuri\Assets\Prefab` returned `True`.
- `Test-Path .\Pakuri\Assets\SO` returned `True`.
- `Get-ChildItem .\Pakuri\Assets\Prefab` listed `Enemy`, `Monster`, and `Skill` subfolders.
- User stated that `Assets/Prefab` will store future monster/player-unit, enemy, and skill prefabs.
- User stated that `Assets/SO` will store ScriptableObject data.

### History

- 2026-05-14: User declared future prefab and SO asset storage locations.

## Task: 2026-05-14 Combat V2 Phase1-C Existing Data Bridge

### Task title

Record Phase1-C reuse of existing data loading for Combat V2 sample skills.

### Goals

- Reuse current CSV/Data loading for Combat V2 sample skill lookup.
- Avoid adding a second CSV parser or changing current CSV files.
- Convert existing `SkillDefinition` samples into transient Combat V2 `SkillData` objects for connection tests.

### Constraints

- Role Owner is Code Builder.
- No CSV edits, ScriptableObject asset generation, or production data migration in this task.
- `CombatV2TestDataBootstrap.Awake()` loading is test-only; production should keep MainMenuScene / RunStartContext data timing.

### Role Owner

Code Builder

### Status

Builder implementation completed and verified.

### Next Actions

- Phase1-D should validate mapped data before more sample skills or full data entry are added.

### Evidence

- Added `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/CombatV2SkillCatalog.cs`.
- Added `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/CombatV2SkillDefinitionMapper.cs`.
- Added `Pakuri/Assets/Scripts2/CombatV2/Core/CombatV2TestDataBootstrap.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors.
- Unity-MCP editor code execution returned `ariel-a:ProjectileSkillData|ariel-b:ShieldSkillData|sourceCatalog=True`.

### History

- 2026-05-14: User requested Phase1-C and asked to note that current loading timing is for testing, while the original MainMenuScene handoff remains the production target.

## Task: 2026-05-14 Combat V2 Sample Data Roadmap

### Task title

Record data-entry order for Combat V2 Blueprint skills.

### Goals

- Use 1-2 sample Blueprint skill data assets for connection testing before full data production.
- Defer full 50-skill data entry until skill lookup, validation, and minimum execution are proven.
- Keep legacy `SkillDefinition` compatibility as a later bridge task.

### Constraints

- Role Owner is Designer.
- No data assets, CSV edits, or ScriptableObject migration in this task.

### Role Owner

Designer

### Status

Completed as roadmap/report work.

### Next Actions

- Phase1-C should reuse the existing `Assets/Scripts/Data` CSV/Data flow and add only a thin Combat V2 catalog/mapper for 1-2 sample skills.
- Phase1-D should add validation rules before broad data entry.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/SkillData.cs` exposes Blueprint skill fields and choice arrays.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs` exposes current legacy skill data fields and choice arrays.
- `Pakuri/Assets/Scripts/Data/Definition/MonsterDefinition.cs` still stores active/passive skill definitions through legacy arrays.
- `Pakuri/Assets/Scripts/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` currently builds `GameDataCatalog`, `MonsterDefinition`, active skills, passive skills, and skill choices from CSV source rows.
- `Pakuri/Assets/Scripts/Data/Runtime/PakuriDataManager.cs` currently provides active/passive skill lookup by monster ID and slot.

### History

- 2026-05-14: User asked whether data should be connected first; Designer chose sample data before full data entry.
- 2026-05-14: User proposed referencing existing `Assets/Scripts/Data` loading scripts for the first 1-2 connected skills; Designer agreed and chose reuse plus a thin Combat V2 mapper over a new CSV loader.

## Task: 2026-05-14 Combat V2 Phase1-B Blueprint Skill Data

### Task title

Write the Phase1-B Blueprint skill data structures for Combat V2.

### Goals

- Expand the previous minimal skill shell files into editable Unity ScriptableObject blueprint data.
- Keep the files data-only: no skill execution, target search, damage application, or runtime mutation logic.
- Add reusable serializable specs for timing, targeting, damage, status application, projectiles, areas, ally effects, buffs, shields, and passives.

### Constraints

- Role Owner is Code Builder.
- No CSV edits, ScriptableObject asset instances, legacy data migration, scene edits, or Play Mode verification in this task.
- Existing `SkillDefinition` compatibility remains deferred to a later bridge/adapter slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and compile verified.

### Next Actions

- After review permission, run Code Reviewer for the Phase1-B data shape.
- A later slice should add a legacy `SkillDefinition` bridge or create sample Blueprint assets, but not both in the same narrow slice.

### Evidence

- Added `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/SkillBlueprintSpecs.cs`.
- Updated `SkillData.cs`, `ProjectileSkillData.cs`, `BeamSkillData.cs`, `ZoneSkillData.cs`, `BuffSkillData.cs`, `ShieldSkillData.cs`, `PassiveSkillData.cs`, `StatusEffectData.cs`, and `SkillChoiceEffectSpec.cs` under `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/`.
- `Pakuri/Assembly-CSharp.csproj` includes `Assets\Scripts2\CombatV2\Skills\Data\SkillBlueprintSpecs.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP force refresh cleared the initial missing-type import errors; the final console read showed only MCP client handler messages and no `Assets\Scripts2\CombatV2\Skills\Data` compile errors.

### History

- 2026-05-14: User requested Code Builder to start Phase1-B by writing the new Blueprint skill data files.
- 2026-05-14: Code Builder expanded the Blueprint data files while keeping runtime skill execution unimplemented.

## Task: 2026-05-14 Combat V2 Blueprint Skill Data Shells

### Task title

Record Blueprint-first Combat V2 skill data shell creation.

### Goals

- Create new `SkillData` blueprint shells first, following the user's latest direction.
- Include projectile, beam, zone, buff, shield, passive, status-effect, and choice-effect data shells.
- Preserve existing CSV/Data flow for now by avoiding data asset migration or runtime loader changes.

### Constraints

- Role Owner is Code Builder.
- No CSV edits, ScriptableObject asset creation, or legacy data replacement in this task.
- Existing `SkillDefinition` compatibility is now deferred to a future bridge/adapter task instead of being the first implementation step.

### Role Owner

Code Builder

### Status

Builder implementation completed as compileable data shells.

### Next Actions

- After review, decide whether the first bridge should map legacy `SkillDefinition` into the new `SkillData` shape or keep both schemas side by side during early Combat V2 tests.
- Do not migrate current CSV rows until the bridge path is proven by code.

### Evidence

- Added `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/SkillData.cs`.
- Added `ProjectileSkillData.cs`, `BeamSkillData.cs`, `ZoneSkillData.cs`, `BuffSkillData.cs`, `ShieldSkillData.cs`, `PassiveSkillData.cs`, `StatusEffectData.cs`, and `SkillChoiceEffectSpec.cs` under `Pakuri/Assets/Scripts2/CombatV2/Skills/Data/`.
- `Pakuri/reference/skill-class-design.md` was previously inspected and defines the requested skill families.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors.

### History

- 2026-05-14: User clarified that the first implementation should create new Blueprint skill data scripts rather than start from the old `SkillDefinition` adapter.
- 2026-05-14: Code Builder created the minimal compileable Blueprint data shell files.

## Task: 2026-05-14 Combat V2 Skill Data Adapter Note

### Task title

Record data compatibility for using `skill-class-design.md` as the Combat V2 skill schema reference.

### Goals

- Preserve current `SkillDefinition` / `PassiveDefinition` / `MonsterDefinition` data compatibility at the start of Combat V2.
- Use `skill-class-design.md` as the target schema reference for projectile, beam, zone, buff, shield, passive, and status-effect data.
- Introduce a proposed adapter layer before any direct data schema replacement.

### Constraints

- Role Owner is Designer.
- No data asset or CSV edits in this task.
- Any future schema migration must be additive until Code Builder proves current data compatibility is preserved.

### Role Owner

Designer

### Status

Completed as design context.

### Next Actions

- Code Builder should implement compatibility through a proposed `SkillSpecAdapter` or equivalent before replacing current `SkillDefinition` usage.
- Keep `SkillEffectPrefab` as visual/presentation data only.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-unit-skill-component-architecture.html`.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs:54` defines current `SkillDefinition`.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs:63` exposes current `SkillEffectPrefab`.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs:78` through `:79` exposes current enhancement and master choice arrays.
- `Pakuri/reference/skill-class-design.md:256` defines proposed `StatusEffectData`.
- `Pakuri/reference/skill-class-design.md:288` through `:306` proposes the implementation order from base skill data through advanced exceptions.

### History

- 2026-05-14: User accepted using `skill-class-design.md` as the skill structure reference and asked for the recommended architecture to be expressed as HTML.

## Task: 2026-05-13 Combat V2 Data Compatibility Note

### Task title

Record data-asset compatibility requirements for Combat V2.

### Goals

- Keep current CSV/Data loading and ScriptableObject definitions as the source of combat data.
- Use `SkillDefinition` data plus reusable skill executors rather than hardcoding every skill into one controller.
- Treat `SkillEffectPrefab` as presentation data, not as the owner of skill logic.

### Constraints

- Role Owner is Designer.
- No data asset or CSV edits in this task.
- Combat V2 implementation must preserve current data compatibility unless a later task explicitly migrates data schema.

### Role Owner

Designer

### Status

Completed as design context.

### Next Actions

- Code Builder should reuse `MonsterDefinition`, `EnemyDefinition`, `SkillDefinition`, `PassiveDefinition`, and `RunSession.RunMonsterState` in the first Combat V2 implementation slice.
- Any new executor mapping should be additive and should not require changing existing CSV rows first.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html`.
- `Pakuri/Assets/Scripts/Data/Definition/MonsterDefinition.cs` already exposes combat tuning, active skills, passive skills, and reward choices.
- `Pakuri/Assets/Scripts/Data/Definition/EnemyDefinition.cs` already exposes stats, defenses, attack type, Stage 1 skill kind, and active skill values.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs` already exposes `SkillRuntimeKind`, `SkillEffectPrefab`, coefficients, cooldown, magazine, reload, status ID, and enhancement/master choices.
- User requested skill management that is reusable and avoids hardcoding as much as possible.

### History

- 2026-05-13: User confirmed Combat V2 should preserve existing CSV/Data loading and use a flexible reusable skill structure.

## Task: 2026-05-08 Rin F-J CSV/SO Runtime State Alignment

### Task title

Align Rin F-J CSV implementation state with the existing SO state.

### Goals

- Remove the Rin F-J CSV/SO state mismatch before implementing Manifest party flow.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from actual CSV/SO inspection and build output.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Keep CSV and SO implementation-state fields aligned when future skill runtime states change.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now marks `rin-f`, `rin-g`, `rin-h`, `rin-i`, and `rin-j` as `RuntimeImplemented`.
- Existing Rin SO data had F-J `ImplementationState: 2`; this task changed the CSV side to match the SO/runtime-implemented state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked to clean up the Rin F-J CSV/SO mismatch before Manifest implementation.
