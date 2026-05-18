Archived snapshot created during 2026-05-18 board cleanup.

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

## Task: 2026-05-17 Eve Status Enum And Visible Shock Label

### Task title

Move Eve-facing status runtime checks to `StatusEffectKind` and surface active statuses in name labels.

### Goals

- Ensure Eve-A shock uses the shared status enum path instead of local hardcoded string behavior.
- Preserve existing Eve skill data strings by parsing them into `StatusEffectKind`.
- Make Eve-applied statuses visible on the affected unit label.

### Constraints

- Role Owner is Code Builder.
- This task does not implement Eve B-E executor behavior or Eve F-J passive damage hooks.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in NewRunScene Play Mode that Eve-A hit shock appears as `[감전]` on the target label.
- Continue Eve B-E/F-J implementation against `StatusEffectKind` after this status path is accepted.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:168` reads `StatusEffectData.Kind`, `:169` parses string data with `StatusEffectUtility.TryParse(...)`, and `:181` handles Eve-A shock chance through `StatusEffectKind.Shock`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:203` maps status data strings into enum kind data.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectData.cs:9` adds the serialized enum field while retaining the existing string field.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs:360` stores projectile hit status kind as `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs:53` and `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs:53` append the central status display suffix to labels.
- Runtime/editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.

### History

- 2026-05-17: User requested status enum centralization and visible status label output after asking whether Eve-A shock was still hardcoded.

## Task: 2026-05-17 Eve A-J Runtime Step 2 Projectile Modifier Execution

### Task title

Connect Eve-A projectile runtime modifiers, shock, and branch lightning to the shared InGame projectile execution path.

### Goals

- Make Eve-A chosen modifier snapshot fields affect actual projectile runtime behavior.
- Apply additional projectiles, pierce, magazine bonus, reload multiplier, and shot interval multiplier from selected Eve-A choices.
- Apply Eve-A shock on hit through the shared status runtime.
- Spawn branch lightning projectiles from modifier data instead of an Eve-only hardcoded executor branch.

### Constraints

- Role Owner is Code Builder.
- This slice targets the common projectile executor and Eve-A data behavior only.
- Eve B-E Beam/Zone behavior and Eve F-J passive hooks remain pending.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies in NewRunScene Play Mode that Eve-A shock, additional projectile fan-out, pierce, branch lightning, magazine, reload, and shot interval choices behave as expected after Offering choices are acquired.
- Implement Eve B-E Beam/Zone executor behavior after projectile behavior is accepted.
- Run Code Reviewer only when explicitly permitted by the user.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs:73` now exposes `CanCastWithSnapshot(...)`, and `:90` exposes `TryBeginCast(SkillExecutionSnapshot)` so magazine/reload/shot interval modifiers can affect cast gating and cast cost.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:112` resolves the choice snapshot before cast gating and passes the same snapshot into `TryBeginCast(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:82` consumes `AdditionalProjectileBonus`, while `:87` resolves branch behavior, and `:158` resolves on-hit status behavior.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs:153` applies status on hit and `:154` routes branch spawning.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md:29` through `:30` define Eve-A shock chance 15% and 1 stack; `:47` through `:58` define Eve-A magazine, reload, pierce, additional projectile, branch, and shock-stack enhancements.
- `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv:2` through `:8` contain the Eve-A modifier rows consumed by the snapshot path.
- Runtime/editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP script refresh reached idle and console warning/error read returned only MCP client handler logs.
- `git diff --check` on changed runtime scripts passed with only LF-to-CRLF normalization warnings.

### History

- 2026-05-17: User asked Code Builder to implement step 2 after the shared status runtime foundation was completed.

## Task: 2026-05-17 Eve A-J Runtime Step 1 Status Foundation

### Task title

Prepare the shared InGame status runtime foundation for Eve A-J execution.

### Goals

- Start Eve A-J implementation from the required status runtime layer before wiring individual skills.
- Provide a common unit status store for Eve A shock, B slow/resistance follow-up, C chill/freeze, D shock-stack checks, E vulnerable, and F-J passive conditions.
- Keep individual Eve skill behavior unchanged until each executor path consumes the status APIs.

### Constraints

- Role Owner is Code Builder.
- No Eve-A hit status application, Eve B-E executor behavior, Eve F-J passive bonus, or Play Mode verification was implemented in this slice.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Implement Eve-A shock application through the projectile hit path after deciding status duration/chance from data/reference.
- Implement Beam/Zone executor consumption for Eve B-E after the status store is proven.
- Later implement F-J passive hooks through damage/resistance calculation using status queries.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` now stores `Statuses = new UnitStatusRuntimeSet()`.
- `UnitStatusRuntimeSet` supports applying normalized status tags, stack accumulation/capping, timed or permanent duration, ticking, querying, and removal.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now exposes `ApplyStatus(...)`, `HasStatus(...)`, `GetStatusStacks(...)`, and `RemoveStatus(...)`.
- `InGameCombatManager.Update()` now ticks unit statuses every frame through `TickUnitStatuses(Time.deltaTime)`.
- Runtime/editor builds completed with 0 errors and existing assembly reference warnings; Unity-MCP refresh reached idle and console warning/error read showed only MCP client handler logs.

### History

- 2026-05-17: User accepted the Designer plan and asked Code Builder to begin with item 1, the common status runtime foundation.

## Task: 2026-05-17 Eve A-J Data And Offering Mapping

### Task title

Fill Eve A-J skill, choice, modifier, and Offering reward data from the Eve skill reference folder.

### Goals

- Align Eve A-E active and F-J passive rows with `Pakuri/reference/2.Monster/eve/skill`.
- Add Eve A-E trait/master and F-J passive trait metadata and modifier rows.
- Make Offering enhancement reward IDs match the skill choice modifier IDs.
- Prevent chosen Eve skill modifiers from applying to unrelated active skill snapshots.

### Constraints

- Role Owner is Code Builder.
- Passive F-J trait rows are data-entered, but their conditional passive effects remain `DataOnlyUnsupported` where the current InGame executor/schema lacks support.
- No Play Mode verification was run by Codex.
- Unity-MCP `execute_code` catalog inspection failed with the known Windows Mono path-length error, so verification used CSV parsing, Unity refresh/console, and dotnet builds.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Offering initially shows Eve-A enhancements, later shows B-E enhancements only after the corresponding active is learned, and shows F-J passive traits only after each passive is learned.
- Later implementation should add the missing passive/status/resistance/conditional-damage runtime fields currently marked `DataOnlyUnsupported`.

### Evidence

- Updated `Pakuri/Assets/CSVdata/source/monster_skills.csv` with 10 Eve A-J rows from the Eve skill reference folder.
- Updated `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`, `Pakuri/Assets/CSVdata/SkillChoiceData.csv`, and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` to 50 Eve choice/modifier rows.
- Updated `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` to 50 Eve Offering reward rows whose IDs match `SkillChoiceModifierData.csv`.
- CSV consistency check returned `EveSkillRows=10; Active=5; Passive=5; ChoiceData=50; SourceChoices=50; Modifiers=50; EveRewards=50; MissingChoiceMods=0; MissingRewardChoices=0; MissingSourceChoices=0; BadEveRewards=0; BadNumeric=0`.
- `SkillChoiceResolver.cs` now applies modifier records only when the chosen ID belongs to the current skill's `EnhancementChoices` or `MasterChoices`.
- `InGameUIManager.cs` now filters skill-choice reward IDs so they appear only after the target active/passive skill is learned.
- Runtime/editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP refresh reached idle and console warning/error read showed only MCP client handler logs.
- 2026-05-17 follow-up: Replaced the malformed Eve A-J `monster_skills.csv` records with fresh 26-column rows after Unity reported row 43 invalid `attribute`.
- Follow-up validation returned `Headers=26; Rows=50; EveRows=10; Bad=0; EveAAttribute=Lightning; EveABaseDamage=24; EveDImplementation=RuntimeImplemented; EveDRequiredSlot=A`.
- Follow-up Unity refresh reached idle and console warning/error read showed only MCP client handler logs.
- 2026-05-17 follow-up: Changed Eve slot A `display_name` from `Arc Bolt` to `아크 볼트` and Eve slot F `display_name` from `Voltage Calibration` to `전압 보정` in `monster_skills.csv` to match `monsters.csv`.
- Follow-up exact-name validation returned `ANameMatch=True`, `FNameMatch=True`; quote-aware CSV parsing returned `ExpectedColumns=26`, `TotalRows=52`, `BadRows=0`.
- Follow-up runtime/editor builds completed with 0 errors and existing assembly reference warnings; Unity refresh reached idle and console showed no Eve default skill name validation errors.

### History

- 2026-05-17: User asked Code Builder to enter Eve A-J data using `Pakuri/reference/2.Monster/eve/skill` and perform the earlier data/Offering validation work first.
- 2026-05-17: User reported CSV row 43 enum errors; Builder fixed the Eve A-J source rows so Unity's CSV parser can read the expected columns.
- 2026-05-17: User reported Eve active/passive representative name mismatch errors; Builder aligned Eve A/F display names with the monster row.

## Task: 2026-05-15 Eve-A Phase4-C-0 Projectile Actor Minimum Execution

### Task title

Connect Eve-A to the first shared InGame projectile actor path.

### Goals

- Add a reusable projectile actor that moves a spawned skill prefab and relays damage hits to `InGameCombatManager`.
- Connect Eve-A projectile execution through the shared `ProjectileSkillExecutor`.
- Support 1P manual Eve-A firing by held left mouse input and AutoBtn switching 1P A to automatic fire.
- Destroy Eve-A projectiles when they move past the assigned `SpawnPoint` X boundary.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- Eve-A shock/status application, branch lightning, additional projectile fan-out, and master-skill branch behavior are not implemented in this slice.
- Enemy prefabs currently lack colliders, so the projectile actor includes a temporary roster-distance hit fallback in addition to trigger hits.

### Role Owner

Code Builder

### Status

Builder implementation completed and compile/editor-refresh verified.

### Next Actions

- User verifies in Play Mode that 1P Eve-A fires toward the mouse while held, then fires automatically after `Canvas/AutoBtn` is clicked.
- Add reusable status/branch projectile behavior in a later Phase4-C subtask instead of hardcoding Eve-A branch logic.
- Add enemy colliders or replace the roster-distance fallback with the final hit-detection contract.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs`.
- `InGameProjectileActor.cs` contains the required `Canvas/AutoBtn` comment and destroys the projectile when `transform.position.x > destroyBeyondX`.
- `SkillExecutors.cs` now makes `ProjectileSkillExecutor` instantiate a projectile prefab and initialize `InGameProjectileActor`.
- `InGameCombatManager.cs` now routes first-player A skill manual mouse input through `TryExecuteManual(...)` while other skill routes remain automatic.
- `NewRunScene.unity` assigns `eveAProjectilePrefab` to `Assets/Prefab/Skill/Eve/Eve_A.prefab` and `projectileDestroyBoundary` to `SpawnPoint`.
- `Assets/Prefab/Skill/Eve/Eve_A.prefab` has `Pakuri.InGame.InGameProjectileActor` and its `BoxCollider2D` is serialized as trigger.
- Runtime and editor builds passed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed no C# compile errors.
- 2026-05-16 follow-up: `SkillExecutionUtility.FindNearestTarget(...)` now ignores `Targeting.Range`, so AutoBtn-routed Eve-A targets the full enemy roster instead of only enemies inside the former range value.
- 2026-05-16 follow-up: `SkillData.csv` no longer contains the `range` column for `eve-a`; `InGameSkillDefinitionMapper` maps source range to ignored `Targeting.Range = 0f`.

### History

- 2026-05-15: User asked Code Builder to create the common projectile/effect actor component and connect Eve-A minimum execution as the first Phase4-C subtask.
- 2026-05-16: User requested all skills to ignore range and Auto targeting to cover the whole map; Builder removed the InGame range filter affecting Eve-A auto targeting.

## Task: 2026-05-15 Eve-A Phase4-B Execution Contract Wiring

### Task title

Record Eve-A reaching the Phase4-B no-effect execution contract path.

### Goals

- Ensure selected Eve's learned A skill can be built as a `SkillRuntimeInstance` during NewRunScene entry.
- Route projectile-type skills through a type-based no-effect executor contract.
- Apply Eve-A choice modifier data to snapshots when matching choice IDs are present.

### Constraints

- Role Owner is Code Builder.
- No Eve-A projectile prefab, branch projectile, damage, shock application, pierce, or Play Mode gameplay behavior was implemented.
- Branch Circuit remains data/snapshot only in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Phase4-C should connect base Eve-A damage through `ApplyDamage(...)` before branch behavior.
- Branch projectile behavior should remain reusable projectile behavior and not become an Eve-specific code path.

### Evidence

- `NewRunSceneEntryManager.cs` calls `SkillRuntimeFactory.RebuildLearnedActiveSet(...)` after selected monster model creation.
- `SkillExecutionSystem.cs` ticks unit skill runtime sets and routes cast-ready skills through `SkillExecutorRegistry`.
- `SkillExecutorRegistry.cs` registers `ProjectileSkillExecutor`, so Eve-A's `ProjectileSkillData` routes through a projectile executor contract.
- `SkillChoiceModifierData.csv` rows for `eve-a-master-1` and `eve-a-master-2` are parsed through the new modifier parser/library path when assigned as `skillChoiceModifierCsv`.
- Runtime/editor builds passed with 0 errors and existing warnings.

### History

- 2026-05-15: Phase4-B implementation connected Eve-A data/runtime setup to a no-effect execution contract.

## Task: 2026-05-15 Eve-A Choice Modifier CSV Seed

### Task title

Record Eve-A enhancement and master choice modifier data seed.

### Goals

- Represent Eve-A Arc Bolt enhancement and master choice effects in new structured CSVData files.
- Capture Branch Circuit as data fields for a future reusable projectile branch behavior rather than an Eve-only hardcoded exception.
- Capture Overcharged Barrage as pierce, additional projectile, shot interval, damage, and shock stack fields.

### Constraints

- Role Owner is Code Builder.
- No Eve skill execution, projectile spawning, branch projectile runtime, status application, or Play Mode verification was implemented.
- Branch behavior is data-only in this slice.
- Fire speed/reload speed wording from the reference is stored as derived interval/time multipliers with notes.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Phase4-B should read these rows into a choice resolver/snapshot path.
- Branch projectile runtime should remain a reusable projectile behavior, not an Eve-A-specific executor branch.
- User verifies gameplay only after later executor/runtime work connects these modifiers to actual skill behavior.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` lists five traits and two master skills for Arc Bolt.
- Added `Pakuri/Assets/CSVdata/SkillChoiceData.csv` with Eve-A trait/master rows.
- Added `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` with explicit modifier columns and Eve-A rows.
- `SkillChoiceModifierData.csv` records `eve-a-master-1` with `damage_multiplier=1.35`, `magazine_bonus=2`, `branch_chance_set=1`, `branch_count=2`, `branch_damage_multiplier=0.6`, and `branch_search_radius=4.5`.
- `SkillChoiceModifierData.csv` records `eve-a-master-2` with `damage_multiplier=1.45`, `additional_projectile_bonus=2`, `pierce_bonus=2`, `shot_interval_multiplier=1.2`, `status_tag=감전`, and `status_stacks_set=2`.
- PowerShell `Import-Csv` parsed both CSV files and the ID consistency check found seven choices and seven modifiers with no missing modifier rows.

### History

- 2026-05-15: User asked to create Eve-first skill choice CSV files before full Phase4-B implementation, using explicit columns instead of a generic `value` field.

## Task: 2026-05-14 Eve Prefab HP Bar Visibility And Binding Fix

### Task title

Confirm Eve prefab actor/model binding and HP bar sprite visibility.

### Goals

- Keep `Eve_Unit` on the same `MonsterUnitActor` / `MonsterUnitRuntimeModel` entry path as the other selectable monsters.
- Make Eve's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Eve combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies Eve selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Prefab/Monster/Eve_Unit.prefab` now has sprite references on HP bar `Background`, `Fill`, and `Shield`.
- Unity-MCP verification returned `eve:prefab=Eve_Unit|modelOk=True|model=eve|actor=True|actorModel=True|hpText=HP 220/220|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Eve_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.

### History

- 2026-05-14: User asked to fix invisible `MonsterHpBar` and verify five selectable prefab bindings.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.

## Task: 2026-05-14 Eve Phase2-B Actor Model Binding

### Task title

Bind Eve's `Eve_Unit` prefab actor to the selected player runtime model.

### Goals

- Use the user-authored `Assets/Prefab/Monster/Eve_Unit.prefab` debug HP/name children.
- Initialize the prefab's `MonsterUnitActor` with Eve's `MonsterUnitRuntimeModel` after spawn.
- Display Eve's current HP/max HP and name through the prefab debug labels.

### Constraints

- Role Owner is Code Builder.
- No Eve combat execution, projectile execution, damage loop, or Play Mode verification in this slice.
- `MonsterUnitRuntimeModel` is not a Unity component and therefore is created at runtime rather than assigned to the prefab in the Inspector.
- Code Reviewer was not run because the user did not explicitly request Reviewer execution for this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies Eve 1P spawn and debug HP/name display in Play Mode.
- Later combat slices should call `MonsterUnitActor.RefreshDebugView()` after HP/shield changes.
- Keep Eve skill execution work deferred until the actor/model entry binding is confirmed in Play Mode.

### Evidence

- `Pakuri/Assets/Prefab/Monster/Eve_Unit.prefab` contains `MonsterUnitActor`, `MonsterHpLabel`, `MonsterHpBar`, `MonsterNameLabel`, `Fill`, and `Shield`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now exposes `Initialize(MonsterUnitRuntimeModel)` and resolves the Eve debug child objects by name.
- Unity-MCP editor code execution returned `evePrefab=True|actor=True|initialize=True|refresh=True|nameLabel=True|hpLabel=True|hpFill=True|shieldFill=True|modelMonster=eve|modelHp=220|learnedA=1`.
- Runtime and editor `dotnet build` checks completed with 0 errors and existing assembly reference warnings.
- Unity-MCP console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-14: User confirmed Eve prefab has HP SlideBar, debug HP/name labels, and `MonsterUnitActor`, then requested Phase2-B.

## Task: 2026-05-14 Eve NewRunScene 1P Prefab Spawn

### Task title

Record Eve shell prefab spawning through the new run entry flow.

### Goals

- Use `Assets/Prefab/Monster/Eve_Unit.prefab` as the current 1P visual shell for NewRunScene entry.
- Spawn the prefab at `1PSpawnPoint`.
- Keep combat behavior and skill execution out of this slice.

### Constraints

- Role Owner is Code Builder.
- No Eve combat execution, skill behavior, HP binding, or Play Mode verification.
- Code Reviewer was explicitly skipped by the user.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- Bind the spawned Eve shell to `MonsterUnitActor` and `MonsterUnitRuntimeModel` in the next Phase2-B slice.
- User verifies Play Mode scene entry and visible 1P spawn.

### Evidence

- `Pakuri/Assets/Prefab/Monster/Eve_Unit.prefab` exists and has root object name `Eve_Unit`.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` maps selected monster ID `eve` to `eveUnitPrefab`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` stores `eveUnitPrefab` as the prefab GUID for `Eve_Unit` and `playerSpawnPoint` as `1PSpawnPoint`.
- Unity-MCP read-only code returned `spawn=1PSpawnPoint|prefab=Eve_Unit`.

### History

- 2026-05-14: User requested spawning the Eve shell prefab as 1P during NewRunScene entry.

## Task: 2026-05-14 Eve CSVData Phase0-2 Seed Rows

### Task title

Record Eve rows added to the new CSVData files.

### Goals

- Seed Eve identity/stat data in `MonsterStat.csv`.
- Seed Eve-A Arc Bolt in `SkillData.csv`.
- Keep source notes clear where values come from current project data instead of the Eve reference page.

### Constraints

- Role Owner is Code Builder.
- No Eve runtime behavior, prefab, scene, or Play Mode changes.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData mapping should read `eve` from `MonsterStat.csv` and `eve-a` from `SkillData.csv`.
- Reconfirm Eve base HP ownership before CSVData becomes the authoritative source because `eve-tower.md` does not list HP.

### Evidence

- `Pakuri/Assets/CSVData/MonsterStat.csv` now contains the `eve` row with `max_health` 220, attack 30, spell 30, and default skill IDs.
- `Pakuri/Assets/CSVData/SkillData.csv` now contains `eve-a` with Arc Bolt projectile, damage, coefficient, magazine, reload, interval, and shock values.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` provides the inspected Arc Bolt numeric values used by `eve-a`.
- `Import-Csv Pakuri\Assets\CSVData\SkillData.csv` returned `eve-a` as `ProjectileSkillData` with `damage_element` Lightning and `base_damage` 24.

### History

- 2026-05-14: Code Builder added Eve seed data as part of CSVData Phase0~2.

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
