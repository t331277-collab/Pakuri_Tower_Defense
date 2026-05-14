## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/EVE_MONSTER.md`.

# EVE_MONSTER

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Scope

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

Legacy non-English note retained these code references: `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.

## Eve Runtime Summary

- Eve active skills A-E runtime work exists in the migrated task blocks below.
- Eve passive skills F-J runtime work exists in the migrated task blocks below.
- Arc Bolt has projectile, branch damage, magazine, reload, and enhancement/master behavior history.
- Eve status runtime includes shock, chill/freeze interactions, vulnerability, shield, action-speed, and passive damage modifiers.
- DebugScene testing for Eve skill toggles is tied to `boards/UI/UI_BLACKBOARD.md`; older DebugScene UI history is archived at `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md`.

## Cross-Board Update Requirements

- Projectile changes: update this file; older projectile history is archived at `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Status/shield/freeze/vulnerability changes: update this file and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- DebugScene Eve skill toggle changes: update this file and `boards/UI/UI_BLACKBOARD.md`; consult `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md` and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` only when older history is needed.
- Eve data asset changes: update this file and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- Reports about Eve implementation: update this file and `boards/REPORT/REPORT_BLACKBOARD.md`.

## Task: 2026-05-14 Eve-E Field Data Implementation

### Task title

Implement Eve-E as a Field / ZoneSkillData source skill.

### Goals

- Change Eve-E source data from projectile classification to field classification.
- Keep Eve-E mapped to `ZoneSkillData` by the current InGame skill mapper.
- Align visible data with the Plasma Field reference: lightning element, 5.0 second duration, 0.8 second tick interval.

### Constraints

- Role Owner is Code Builder.
- Do not implement combat execution behavior or Play Mode verification in this task.
- Do not create prefabs or scene objects.
- Eve-E reference does not provide a numeric radius, so this task does not invent a radius value.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Later skill execution work should define Eve-E's zone radius/placement behavior if the reference remains incomplete.
- User-owned Play Mode verification is still needed when Eve-E execution is migrated to the InGame skill executor path.

### Evidence

- Updated `Pakuri/Assets/CSVdata/source/monster_skills.csv` so `eve-e` is `Field`, `Lightning`, duration source value `5`, magazine `3`, reload `6`, and tick interval `0.8`.
- Updated `Pakuri/Assets/Data/GameData/Monsters/eve.asset` so `eve-e` has `DisplayName` Plasma Field, `RuntimeKind: 4`, `Attribute: 2`, `CooldownSeconds: 5`, and `ShotIntervalSeconds: 0.8`.
- Updated Eve-E choice text in `monster_skill_choices.csv` and `eve.asset` from old beacon/ice wording to Plasma Field/lightning wording where the changed reference required it.
- Unity-MCP Editor code execution returned `skill=eve-e|name=플라즈마 필드|kind=Field|attr=Lightning|cooldown=5|mag=3|reload=6|interval=0.8|mapped=ZoneSkillData|zone=True|errors=0|warnings=0`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User explicitly assigned Code Builder and requested changing Eve-E `RuntimeKind` from `MagazineProjectile` to `Field`.

## Task: 2026-05-14 Eve-E Plasma Field Zone Classification

### Task title

Classify Eve-E as a ZoneSkillData field skill instead of a projectile or summon skill.

### Goals

- Treat `eve-e` as the updated Plasma Field / 장판형 설치 skill from the reference document.
- Map Eve-E to `ZoneSkillData` in the InGame skill data shape.
- Avoid using `ShotIntervalSeconds` projectile validation as the controlling requirement for Eve-E.

### Constraints

- Role Owner is Designer.
- Do not implement code or data edits in this design note.
- Keep claims grounded in inspected files and current CSV/data state.

### Role Owner

Designer

### Status

Design decision recorded; Code Builder implementation is still needed.

### Next Actions

- Code Builder should change the Eve-E source/data classification away from `MagazineProjectile` and into a zone-compatible runtime kind, preferably `Field` for a persistent ticking area.
- Code Builder should ensure the mapper routes Eve-E to `ZoneSkillData` and validates its duration/tick/radius rules instead of projectile shot interval rules.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` names the skill `플라즈마 필드` and describes it as a `장판형 설치 스킬`.
- The same reference gives `드론 지속시간` 5.0 seconds and `공격 주기` 0.8 seconds, matching zone duration/tick semantics rather than direct projectile shot interval semantics.
- `C:\TowerDefence_Pakuri\towerdefense_pakuri_docs\docs\dev\skill-class-design.md` lists Eve-E under `ZoneSkillData`.
- Before the Code Builder implementation in the task above, `Pakuri/Assets/CSVdata/source/monster_skills.csv` still had `eve-e` with `MagazineProjectile`.
- `InGameSkillDefinitionMapper` maps `MagazineProjectile` to `ProjectileSkillData`, while `AreaAttack` and `Field` map to `ZoneSkillData`.

### History

- 2026-05-14: User clarified that Eve-E changed from the old drone skill to a field/zone skill and should be classified as `ZoneSkillData`.

## Task: 2026-05-14 InGame Phase2-A Eve Unit Model Mapping

### Task title

Track Eve-specific Phase2-A model creation.

### Goals

- Resolve Eve from current data loading as the Phase2-A selected monster sample.
- Build an Eve `UnitRuntimeModel` without creating an Eve-only unit class.
- Carry default run learned active state into the model state bucket for test evidence.

### Constraints

- Role Owner is Code Builder.
- No Eve combat behavior, projectile behavior, status behavior, prefab, scene binding, or Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Bind Eve's model to `MonsterUnitActor` in Phase2-B before adding combat execution.
- Keep Eve A-E/F-J execution deferred to later skill runtime/executor phases.

### Evidence

- `InGameTestDataManager` defaults `sampleMonsterId` to `UnitFactory.DefaultPhase2AMonsterId`, which is `eve`.
- `UnitFactory.TryCreatePhase2ATestModels(...)` resolves Eve and creates a selected monster model through `RunSession.Begin(eve)`.
- Unity-MCP Editor code execution returned `ok=True|monster=eve|monsterHp=220|learnedA=1|enemy=stage1-swordsman|enemyHp=100`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-14: User specified Eve as the monster for Phase2-A; Builder implemented the data-to-model path.

## Task: 2026-05-13 Eve Phase 3 Closeout

### Task title

Verify Eve projectile/effect/drone ownership after Phase 3.

### Goals

- Confirm selected Eve persistent effects and selected drone update order remain intact.
- Confirm selected and manifested Eve drone lifecycle work is behind the drone simulation boundary.
- Preserve Eve-specific formulas and defer shared skill grouping.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; Eve skill verification remains user-owned.
- Do not merge selected and manifested Eve drone runtime types in Phase 3-H.

### Role Owner

Code Builder

### Status

Completed and locally validated.

### Next Actions

- User verifies Eve B/C persistent effects and selected/manifested Eve Drone Beacon behavior in Play Mode if needed.
- Keep broader Eve skill grouping with other monster skills for Phase 6.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1171` through `:1184` still creates selected Eve `DroneRuntime` values.
- `CombatRuntimeEveSkills.cs:1196` through `:1199` still calls persistent skill-effect update before selected drone update.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:81` through `:97` preserves Eve B slow and Eve C chill/freeze effect hit handling.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:46` through `:72` owns selected Eve drone ticking and cleanup.
- `CombatRuntimeDroneSimulation.cs:118` through `:183` owns manifested Eve drone ticking, projectile fire cadence, and cleanup.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder verified Eve-specific Phase 3 boundaries and concluded no additional Eve Phase 3 implementation slice is needed.

## Task: 2026-05-13 Eve Manifested Drone Simulation Alignment

### Task title

Record Eve-file impact of Phase 3-G manifested drone alignment.

### Goals

- Keep manifested Eve Drone Beacon deployment behavior stable.
- Move manifested Eve drone duration, attack cadence, projectile firing, and cleanup into the drone simulation boundary.
- Preserve selected and manifested Eve drone runtime separation.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; Eve skill verification remains user-owned.
- Do not merge selected and manifested drone runtime classes in Phase 3-G.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Eve Drone Beacon duration, fire cadence, target lookup, projectile behavior, and cleanup in Play Mode if needed.
- Continue with Phase 3-H closeout verification.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs:8` through `:17` still defines the manifested Eve drone runtime fields.
- `CombatRuntimeManifestedPartyDrones.cs:19` through `:49` still owns manifested Eve drone deployment, visual setup, duration setup, and status label.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:118` through `:160` now owns manifested Eve drone duration ticking, attack cooldown, target lookup, projectile fire, and cooldown reset.
- `CombatRuntimeDroneSimulation.cs:162` through `:183` now owns manifested Eve drone cleanup.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder moved manifested Eve drone ticking and projectile fire from `CombatRuntimeManifestedPartyDrones.cs` into `CombatRuntimeDroneSimulation.cs`.

## Task: 2026-05-13 Eve Selected Drone Simulation Boundary

### Task title

Record Eve-file impact of Phase 3-F selected drone boundary.

### Goals

- Keep Eve E Drone Beacon deployment behavior stable.
- Move selected Eve drone duration, attack cadence, projectile spawning, and cleanup out of `CombatRuntimeEveSkills.cs`.
- Preserve selected Eve effect-before-drone update order.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; Eve skill verification remains user-owned.
- Do not merge selected and manifested drone runtime classes in Phase 3-F.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected Eve Drone Beacon duration, fire cadence, target lookup, projectile behavior, and cleanup in Play Mode if needed.
- Continue with manifested drone alignment only as Phase 3-G.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1171` through `:1184` still creates the selected Eve `DroneRuntime` with duration, attack period, range, damage, attribute, vulnerable stacks, and `SkillId = "eve-e"`.
- `CombatRuntimeEveSkills.cs:1196` through `:1200` still calls `UpdatePersistentSkillEffects()` before `UpdateDrones()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs:22` through `:25` now owns the selected drone update boundary entry point.
- `CombatRuntimeDroneSimulation.cs:36` through `:63` now owns selected Eve drone duration ticking, attack cadence, firing call, cleanup, and removal.
- `CombatRuntimeDroneSimulation.cs:65` through `:105` now owns selected Eve drone projectile creation.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder moved Eve selected drone ticking and projectile fire from `CombatRuntimeEveSkills.cs` into `CombatRuntimeDroneSimulation.cs`.

## Task: 2026-05-13 Eve Skill Effect Hit Routing Split

### Task title

Record Eve-file impact of Phase 3-E skill-effect hit and expiry routing.

### Goals

- Keep Eve selected skill-effect behavior stable while moving the shared hit dispatcher out of `CombatRuntimeEveSkills.cs`.
- Preserve Eve B slow and Eve C chill/freeze handling.
- Preserve `UpdateEveSkillEffects()` order relative to selected Eve drones.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; Eve skill verification remains user-owned.
- Do not introduce common temporary effects or selected drone boundary work in Phase 3-E.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve B/C sustained effects and selected Eve drone cadence in Play Mode if needed.
- Continue with Phase 3-F only as a separate selected Eve drone simulation boundary slice.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1200` still calls `UpdatePersistentSkillEffects()` before `UpdateDrones()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:81` through `:97` now owns the Eve fallback effect hit helper, preserving Eve B slow and Eve C chill/freeze handling.
- `CombatRuntimeSkillEffectSimulation.cs:58` through `:79` dispatches Sein, Vega, manifested, then Eve fallback in the existing order.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder moved Eve's shared effect hit fallback into `CombatRuntimeSkillEffectSimulation.cs` while leaving Eve update order intact.

## Task: 2026-05-13 Eve Skill Effect Lifecycle Boundary

### Task title

Record Eve-file impact of Phase 3-D skill-effect simulation boundary.

### Goals

- Keep Eve selected skill-effect behavior stable while moving the shared `skillEffects` lifecycle loop out of `CombatRuntimeEveSkills.cs`.
- Preserve `UpdateEveSkillEffects()` order relative to selected Eve drones.
- Keep Eve/Sein/Vega/manifested effect damage callbacks unchanged.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; Eve skill verification remains user-owned.
- Do not introduce common temporary effects or merge drone work in Phase 3-D.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve B/C sustained effects and selected Eve drone cadence in Play Mode if needed.
- Continue with Phase 3-E only as a separate effect hit/expiry routing slice.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1196` through `:1200` still calls `UpdatePersistentSkillEffects()` before `UpdateDrones()`.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs:22` through `:25` now routes `UpdatePersistentSkillEffects()` through the new boundary.
- `CombatRuntimeSkillEffectSimulation.cs:36` through `:64` owns the moved shared skill-effect lifecycle loop.
- `CombatRuntimeEveSkills.cs:1202` still owns `TickSkillEffect(...)`, including existing Eve, Sein, Vega, and manifested effect damage routing.
- Runtime and Editor builds completed with 0 errors and existing assembly reference warnings.

### History

- 2026-05-13: Builder moved the shared effect loop into `CombatRuntimeSkillEffectSimulation.cs` and left Eve's public update order unchanged.

## Task: 2026-05-13 Eve Battlefield Facade Registration

### Task title

Route Eve battlefield projectile, effect, and drone registration through the Phase 1 facade.

### Goals

- Preserve Eve skill behavior while replacing direct battlefield list registration writes.
- Keep Eve projectile/effect/drone creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve skills in Play Mode if needed.

### Evidence

- `CombatRuntimeEveSkills.cs:816`, `:877`, and `:1342` now call `AddBattlefieldProjectile(...)`.
- `CombatRuntimeEveSkills.cs:1171` now calls `AddBattlefieldDrone(...)`.
- Eve skill-effect registrations now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Eve battlefield object registration through facade methods.

## Migrated Task Blocks

## Task: 2026-05-10 Eve Voltage Calibration Shield Review

### Task title

Fix Eve F shield timing and ally application.

### Goals

- Review monster reference files under `Pakuri/reference/2.Monster` for shield-bearing skills.
- Make Eve F apply its battle-start shield to lightning-skill allies, not only the selected 1P unit.
- Prevent selected Eve's shield timer from being decremented by both Eve-specific and shared shield timer paths.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve F in RunScene Play Mode with selected Eve and manifested lightning-skill allies.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Reference search found concrete shield skills in `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `Pakuri/reference/2.Monster/eve/eve-tower.md`, and `Pakuri/reference/2.Monster/eve/skill/f-voltage-calibration.md`.
- `Pakuri/reference/2.Monster/eve/skill/f-voltage-calibration.md:18` defines the shield as Eve power 120% for 12 seconds on lightning-skill allies.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:93` through `:108` no longer decrements `unitShieldTimer`; selected shield duration is handled by the shared shield timer path.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1558` through `:1594` checks selected and manifested units for lightning skills.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs:1682` through `:1732` applies Eve F shield to the selected lightning unit and manifested lightning-skill allies, stamps `ShieldAppliedFrame`, and updates manifested labels.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh reached idle; console error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-10: User asked to review shield logic among monsters under `Pakuri/reference/2.Monster`, specifically noting Eve shield seemed not to apply correctly.
