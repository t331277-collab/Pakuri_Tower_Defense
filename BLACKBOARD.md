# BLACKBOARD.md

## Role

This file is now the root persistent-state index.

Do not use this file as the default detailed task log. Start with `AGENTS.md` and `MDTREE.md`, then read the relevant `boards/` files selected by the routing rules.

The full pre-hierarchy task history is preserved at:

- `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`

## Current Global Status

- Board hierarchy was created on 2026-04-30.
- Detailed task blocks were copied into domain-specific files under `boards/`.
- `BLACKBOARD.md` should stay small and contain only routing, global status, and cross-domain notes.
- Code Reviewer execution requires explicit user permission.
- Unity-MCP Play Mode gameplay verification remains user-owned; Codex records build/compile/console/editor-state evidence only.
- 2026-05-12 global policy task: role-specific content was split out of `AGENTS.md` into `GAMEDESIGNER.md`, `GAMEBULIDER.md`, and `GAMEREVIWER.md`; `AGENTS.md` now keeps startup, evidence, routing, and role entry-point rules. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-12 global policy task: `GAMEDESIGNER.md` and `GAMEBULIDER.md` were compacted into lightweight role entry points, with detailed Designer and Code Builder track rules split into `GAMEDESIGNER_*` and `GAMEBULIDER_*` files. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-12 global policy task: role-related `GAME*.md` files were moved under `AGENTS_ROLE/`, and `AGENTS.md` / `MDTREE.md` role references now point to `AGENTS_ROLE/...` paths. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-18 cross-domain UI/RUN status: `NewRunScene` now has `DebugUI.cs` on `Canvas`, `MonsterPanelUI.cs` on `Canvas/MonsterPanel`, and Offering/debug active-skill acquisition now syncs `RunSession` learned state into active `MonsterUnitRuntimeModel.State` before rebuilding runtime skills. Detailed records are in `boards/UI/RUNSCENE_UI.md` and `boards/RUN/RUN_BLACKBOARD.md`.
- 2026-05-18 cross-domain UI/RUN/DATA status: `InGameUIManager.cs` now resolves prisoner reward display names from the CSV-backed runtime enemy catalog instead of showing mojibake plus raw enemy IDs, and `OfferingUI.cs` now uses CSV-backed monster/skill/passive/reward fields for choice titles/descriptions. Detailed records are in `boards/UI/RUNSCENE_UI.md`, `boards/RUN/RUN_BLACKBOARD.md`, and `boards/DATA/DATA_BLACKBOARD.md`.
- 2026-07-17 routing/policy status: Skill Builder now accepts one exact skill Reference MD as its only semantic input, routes new Base authoring to one of six existing-family blueprints, and routes Enhancement/Master authoring to the shared node blueprint. New runtime/schema/node/code/asset work remains outside the track. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-07-18 board maintenance status: active COMBAT, DATA, MON, OPS, RUN, and UI boards now retain only July-dated task blocks; 180 earlier or undated records were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md`. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-22 cross-domain DATA/COMBAT/MON/OPS status: multi-effect rows now separate application target from visual center/anchor with `center_mode` and `visual_anchor_mode`; Ariel-C ally buff visuals use `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab` on applied ally targets, while Ariel-C attack waves can stay on the primary SingleAttack center. Detailed records are in `boards/OPS/AUTOMATION_GUIDE.md`, `boards/DATA/DATA_BLACKBOARD.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, and `boards/MON/ARIEL_MONSTER.md`.
- 2026-05-22 cross-domain UI/RUN/COMBAT/MON status: `AutoBtn` now toggles selected 1P Auto mode, selected 1P learned active skills require click input while Auto is off, automatic player skill routing requires a visible MainCamera enemy instead of `StageState.Combat`, enemies act by target/range/cooldown rules as soon as they spawn, and committed no-hit monster casts now start cooldowns. Detailed records are in `boards/UI/RUNSCENE_UI.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/COMBAT/ENEMY_BLACKBOARD.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, and `boards/MON/ARIEL_MONSTER.md`.
- 2026-05-22 cross-domain DATA/COMBAT/MON status: Ariel-C stale choice support states were corrected, Ariel-E conditional shield/damage/sanctuary effects and Ariel-B trait 5 were represented as CSV multi-effect rows, and shared runtime now applies choice duration modifiers to statuses plus choice amount/duration modifiers and multi-effects to shield skills. Detailed records are in `boards/MON/ARIEL_MONSTER.md`, `boards/DATA/DATA_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- 2026-05-22 cross-domain DATA/COMBAT/MON status: `monster_skill_triger.csv` was added for CSV-owned trigger skills; Ariel `ariel-a-master-1` now runs a repeated last-projectile prefab-hitbox SingleAttack trigger, and `ariel-b-trait-4` now runs an `OnShieldExpire` prefab-hitbox trigger using shield-applied amount. Detailed records are in `boards/MON/ARIEL_MONSTER.md`, `boards/DATA/DATA_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- 2026-05-24 cross-domain DATA/COMBAT/MON status: Eve F-J passive runtime work is now implemented on shared passive effect/trigger/status paths; `monster_skill_effects.csv` gained conditional-status and applied-duration bonus columns, `monster_skill_triger.csv` gained condition/attribute/proc/internal-cooldown columns, and shared status/trigger runtime now parses expression-style condition statuses such as `chill;freeze` and `shock:5`. Detailed records are in `boards/MON/EVE_MONSTER.md`, `boards/DATA/DATA_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- 2026-05-18 routing/policy status: the active report board was removed from routing, report work now reads the related active domain board only, `AGENTS_ROLE/GAMEDESIGNER.md` now includes a short HTML report structure rule, and inspected `Pakuri/Assets/**/*.csv` files were all already valid UTF-8 so no CSV data rewrite was needed. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-18 routing/policy status: `AGENTS.md` now defines a `SimpelWorker` role for very simple path-based work, `AGENTS_ROLE/SIMPELWORKER.md` was added, SimpelWorker reads no extra markdown after `AGENTS.md` and `MDTREE.md`, and it automatically falls back to Designer when no exact work path is provided. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-19 routing/policy status: `AGENTS.md` now recognizes `Skill Builder` as a Code Builder track, `AGENTS_ROLE/GAMEBULIDER_SKILL.md` defines that track, `AGENTS_ROLE/GAMEBULIDER.md` routes skill implementation work to it, and individual projectile/BeamSkill blueprint rules were moved out of the Builder entry file. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-19 routing/policy status: `AGENTS_ROLE/COMMON.md` now contains shared evidence, Unity Play Mode, Git, Reviewer, and board-update boundaries for Designer, Code Builder, Skill Builder, and Code Reviewer; role/track files were compacted to avoid repeating root/common rules. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-19 OPS maintenance status: active OPS boards were compacted; older automation and reviewer task blocks were moved to `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md` and `boards/ARCHIVE/OPS_REVIEWER_ARCHIVE_2026-05-19.md`. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-18 cross-domain DATA/RUN/COMBAT/MON status: `monster_skills.csv` now uses `SingleAttack` for one-shot area rows `ariel-c`, `ariel-e`, `rin-e`, `vega-b`, and `eve-d`; Eve C/E are `AreaAttack` sustained zone rows with reference-correct names/durations; `SingleAttackData.cs`, `InGameZoneSkillActor.cs`, and shared executors implement the runtime paths. Detailed records are in `boards/DATA/DATA_BLACKBOARD.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, and MON files for Eve/Ariel/Rin/Vega.
- 2026-05-18 cross-domain DATA/RUN/COMBAT status: `stage_one_enemies.csv` now carries `passive_skill_id` and `passive_skill_value`; Stage 1 enemy passive application reads those fields, and `PhysicalDamageUp` is applied only when enemy damage attribute is `Physical`. Detailed records are in `boards/DATA/DATA_BLACKBOARD.md`, `boards/RUN/RUN_BLACKBOARD.md`, and `boards/COMBAT/ENEMY_BLACKBOARD.md`.
- 2026-05-13 refactoring policy task: `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md` was added to track broad architecture/refactoring plans, starting with the `CombatRuntimeController` structure split sequence from the 2026-05-10 proposal.
- 2026-05-13 implementation status: Phase 1 `Battlefield Facade Boundary` is implemented by `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs`; runtime/editor builds passed with existing warnings.
- 2026-05-13 implementation status: Phase 2 `Manifested Party Runtime Split` has started with `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs`; runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 2-A `Manifested Party View Binder` split moved manifested party status-view binding and HP/shield label/bar refresh into `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyView.cs`; runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 2-B `Manifested Party Skill Dispatcher` split moved manifested party skill dispatch and fallback cooldown/magazine ticking into `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartySkills.cs`; runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 2-C `Manifested Party Drone Lifecycle` split moved manifested Eve drone runtime, deployment, ticking, and cleanup into `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDrones.cs`; runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 2-D `Manifested Party Skill Visual / Scene Object Helper` split moved manifested non-drone skill visual duration, circle effect, line effect, and shared visual configuration helpers into `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyVisuals.cs`; runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 2-E `Manifested Party Damage / Projectile Fire Helper` split moved generic manifested skill fire, projectile fire, projectile hit/status, generic damage application, and damage/projectile resolver helpers into `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyDamage.cs`; runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 reviewer status: user-requested external Code Reviewer for Phase 2 `Manifested Party` refactor completed with `REVIEW_RESULT: PASS`; detailed record is in `boards/OPS/REVIEWER_BLACKBOARD.md` and `codex_loop_logs/phase2_manifested_party_reviewer_20260513.md`.
- 2026-05-13 Code Builder closeout status: Phase 2 `Manifested Party` closeout was verified with code inspection, runtime/editor builds, and Unity-MCP refresh/console evidence; no small independent Phase 2-F slice was identified, so the next default implementation phase is Phase 3 `Projectile / Effect / Drone Simulation Split`. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- 2026-05-13 roadmap status: `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` was amended to explicitly map the 2026-05-10 shared target / temporary effect proposal into Phase 7, same-type skill reuse into Phase 6, and prefab/view commonization into Phase 8 after target/effect stabilization. Detailed records are in `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`, `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, `boards/COMBAT/ENEMY_BLACKBOARD.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- 2026-05-13 Phase 3 planning status: Designer split `Projectile / Effect / Drone Simulation Split` into Phase 3-A through 3-H before implementation: projectile boundary, projectile cleanup/lifetime, projectile hit routing, skill-effect boundary, skill-effect hit/expiry routing, selected drone boundary, manifested drone alignment, and closeout verification. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/COMBAT_STATE_OWNERSHIP_MAP.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- 2026-05-13 implementation status: Phase 3-A `Projectile Simulation Boundary Shell` is implemented by `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectileSimulation.cs`; `UpdateProjectiles()` now routes through a boundary shell into the unchanged projectile loop body renamed to `UpdateProjectilesCore()`. Runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 3-B `Projectile Cleanup / Lifetime Ownership` moved projectile lookup, missing-runtime removal, lifetime ticking, X-edge checks, and cleanup/destruction behind `CombatRuntimeProjectileSimulation.cs`; projectile hit and damage routing remains in `CombatRuntimeProjectiles.cs`. Runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 3-C `Projectile Hit Routing Helpers` split the projectile loop into enemy, manifested, and selected/player projectile handlers in `CombatRuntimeProjectiles.cs`; damage APIs and formulas remain in place. Runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 3-D `Skill Effect Simulation Boundary Shell` moved the shared `skillEffects` lifetime/tick/expiry/removal loop into `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeSkillEffectSimulation.cs`; existing effect damage and expiry callbacks remain in skill files. Runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 3-E `Skill Effect Hit / Expiry Routing` split skill-effect enemy eligibility, shape checks, hit dispatch, Eve fallback status handling, and expiry dispatch inside `CombatRuntimeSkillEffectSimulation.cs`; Sein, Vega, and manifested effect formulas remain in their existing helper files. Runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 3-F `Selected Drone Simulation Boundary` moved selected Eve `DroneRuntime` ticking and drone projectile creation into `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeDroneSimulation.cs`; selected Eve drone deployment remains in `CombatRuntimeEveSkills.cs`, and manifested drones remain separate for Phase 3-G. Runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 implementation status: Phase 3-G `Manifested Drone Simulation Alignment` moved manifested drone ticking, projectile fire cadence, and cleanup behind `CombatRuntimeDroneSimulation.cs` while leaving `ManifestedDroneRuntime` definition and deployment in `CombatRuntimeManifestedPartyDrones.cs`. Runtime/editor builds passed with existing warnings and Unity-MCP console showed only MCP client handler logs after import.
- 2026-05-13 closeout status: Phase 3-H `Phase 3 Closeout / Ownership Verification` completed. Phase 3 `Projectile / Effect / Drone Simulation Split` is complete in Code Builder scope; next default implementation phase is Phase 4 `Enemy Simulation Split`. User-owned Play Mode verification and optional Code Reviewer remain outside this Builder closeout.
- 2026-05-13 Combat V2 design status: Designer created `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html` for a new combat-only runtime/scene direction that preserves Run UI Flow, `RunSession`, and CSV/Data loading while using model/view separation, shared `MonsterUnitActor` for selected and manifested units, default-on auto attack toggles, reusable skill executors, and learned-choice lookup from unit state. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, and `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- 2026-05-14 implementation status: Code Builder created minimal compileable Combat V2 skeleton scripts under `Pakuri/Assets/Scripts2/CombatV2` for core context/result/controller, shared unit shells, and Blueprint-first skill data shells. `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing assembly reference warnings. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- 2026-05-14 Phase1-B implementation status: Code Builder expanded Combat V2 Blueprint skill data files under `Pakuri/Assets/Scripts2/CombatV2/Skills/Data`, adding reusable spec classes and data-only ScriptableObject fields. Runtime build passed with 0 errors; Unity-MCP force refresh showed no Combat V2 compile errors after import. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- 2026-05-14 Phase1-C implementation status: Code Builder added a Combat V2 data bridge that reuses existing `PakuriCsvRuntimeData` / `PakuriDataManager` instead of adding a new CSV loader. `ariel-a` and `ariel-b` mapped to `ProjectileSkillData` and `ShieldSkillData` in Unity-MCP editor code execution. `CombatV2TestDataBootstrap.Awake()` is marked test-only; production data timing remains the MainMenuScene / RunStartContext handoff. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- 2026-05-14 scene contract: User declared `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` as the Combat V2 test scene and intended final in-game scene. Scene YAML confirms named objects `BG`, `1PSpawnPoint` through `5PSpawnPoint`, `GameManager`, and `Nexus`; user-defined roles are BG = background sprite, spawn points = player/manifested monster anchors, GameManager = core logic host, Nexus = nexus. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/RUN/RUN_BLACKBOARD.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- 2026-05-14 asset storage contract: `Pakuri/Assets/Prefab` exists with `Enemy`, `Monster`, and `Skill` subfolders and will store future enemy, monster/player-unit, and skill prefabs. `Pakuri/Assets/SO` exists and will store ScriptableObject data assets. Detailed records are in `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`.
- 2026-05-15 InGame Phase3-C status: Code Builder added a resource mutation pipeline and same-bar HP/Shield segment display without connecting Phase3-B enemy attack attempts to real damage. Runtime/editor builds passed with existing warnings; Unity-MCP `execute_code` remained blocked by the known Windows mono path-length issue. Detailed records are in `boards/COMBAT/ENEMY_BLACKBOARD.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, and `boards/RUN/RUN_BLACKBOARD.md`.
- 2026-05-15 InGame Phase4-A status: Code Builder added learned active skill runtime state files under `Assets/Scripts2/InGame/Skills/Runtime`, connected `BaseUnitRuntimeModel.SkillRuntime`, and marked Phase4-A complete in the InGame roadmap. This is state/tick/cooldown/magazine/reload only; skill executors, damage, shields, targeting, projectiles, and Play Mode behavior remain later phases. Detailed records are in `boards/RUN/RUN_BLACKBOARD.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, and `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- 2026-05-15 InGame Phase4-C follow-up status: Code Builder fixed projectile-hit resource mutation to round damage/HP/Shield values, unregister and destroy units at 0 HP, make HP Fill shrink from left to right inside the actual rendered `Background` sprite bounds, and show prefab `Damage` Text feedback as `N(Damage)` while rising by about 1 local Y and fading. Runtime/editor builds passed with existing warnings; Unity-MCP console showed no remaining C# compile errors after the popup helper inclusion fix. Detailed records are in `boards/RUN/RUN_BLACKBOARD.md`, `boards/COMBAT/ENEMY_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- 2026-05-18 board maintenance status: `boards/COMBAT/ENEMY_BLACKBOARD.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, `boards/DATA/DATA_BLACKBOARD.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, `boards/MON/EVE_MONSTER.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, and `boards/UI/UI_BLACKBOARD.md` were compacted so active files keep only current routing-relevant state; full pre-cleanup snapshots plus report history were moved to dated archive files under `boards/ARCHIVE/`, and `MDTREE.md` was rewritten to match the slimmer active-board structure.
- 2026-05-14 Phase2-A implementation status: Code Builder split InGame unit runtime models into `BaseUnitRuntimeModel`, `MonsterUnitRuntimeModel`, and `EnemyUnitRuntimeModel`; `UnitFactory` now creates Eve as `MonsterUnitRuntimeModel` and `stage1-swordsman` as `EnemyUnitRuntimeModel`. Code Builder does not generate prefabs by code; the user creates prefabs in Unity Editor and later attaches provided Actor/binding components. Definition skill/projectile tuning remains deferred to later SkillData mapper work. A required Reviewer attempt ran once and returned a P2 fix request in the broader uncommitted set at `InGameSkillDataValidator.cs:363-365`, not a Phase2-A unit model pass. Detailed records are in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/DATA/DATA_BLACKBOARD.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`, and `boards/OPS/REVIEWER_BLACKBOARD.md`.
- 2026-05-14 Eve-E data status: Code Builder changed Eve-E from `MagazineProjectile` to `Field` in source CSV and Eve asset data so the InGame mapper resolves it as `ZoneSkillData`; Unity-MCP Editor code execution returned `mapped=ZoneSkillData|zone=True|errors=0|warnings=0`. Eve-E zone radius remains a later tuning gap because the inspected reference did not provide a numeric radius. Detailed records are in `boards/MON/EVE_MONSTER.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/DATA/DATA_BLACKBOARD.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- 2026-05-14 Combat V2 target structure status: Designer added `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html`, describing the final ingame class responsibilities and reference directions if the Combat V2 build roadmap is completed. Current shells are separated from proposed future services. Detailed records are in `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`, `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`, and `boards/RUN/RUN_BLACKBOARD.md`.
- 2026-05-12 board maintenance task: `boards/MON/*.md` files were compacted to each file's latest dated task blocks, with older or undated MON task blocks archived to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-12 board maintenance task: active `boards/**/*BLACKBOARD.md` files were compacted to latest dated task blocks, and older task blocks were archived under `boards/ARCHIVE/` in seven-day ranges plus an undated archive. Detailed record is in `boards/OPS/AUTOMATION_GUIDE.md`.
- 2026-05-14 board maintenance task: `boards/UI/DEBUGSCENE_UI.md` and `boards/UI/MAINMENU_UI.md` were moved to `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md` and `boards/ARCHIVE/MAINMENU_UI_ARCHIVE_2026-05-14.md` because their latest internal dates were before 2026-05-12. Active routing now uses `boards/UI/UI_BLACKBOARD.md` for DebugScene/MainMenu UI work and consults those archived files only for older history.
- 2026-05-14 board maintenance task: per user request, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and the full `boards/REFACTORING` folder were moved under `boards/ARCHIVE`. Active routing now uses narrower remaining boards and consults these archived files only for older history.
- 2026-05-08 recent cross-domain task: Rin-first shared `CombatUnitRuntime` plus `CombatSkillRuntime` parity was implemented for selected 1P and manifested 2P-5P Rin B/C/D/E, and manifested slot status UI now reuses existing scene children when present. Detailed records are in `boards/MON/RIN_MONSTER.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, and `boards/RUN/REWARD_BLACKBOARD.md`.
- 2026-05-08 recent cross-domain task: Manifested 2P-5P HP bar live sprite repair was implemented so already-bound `MonsterHpBar/Fill` renderers with `sprite=null` are normalized during HP/status refresh. Detailed records are in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/UI/RUNSCENE_UI.md`, `boards/RUN/RUN_BLACKBOARD.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- 2026-05-09 recent cross-domain task: `Pakuri/Assets/Scripts` was reorganized into clearer Combat, Data, and Run subfolders without code-behavior changes. Detailed records are in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/DATA/DATA_BLACKBOARD.md`, and `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- 2026-05-09 recent cross-domain task: Interrupted Sein unit executor migration was resumed so manifested Sein A-E dispatch through Sein-specific `CombatUnitRuntime` paths and Sein projectile/effect damage reads unit F-J passive state. Detailed records are in `boards/MON/SEIN_MONSTER.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, and `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- 2026-05-10 recent cross-domain task: Vega unit executor migration was implemented so Manifested Vega A-E dispatch through Vega-specific `CombatUnitRuntime` paths and Vega projectile/skill damage reads unit F-J passive state. Detailed records are in `boards/MON/VEGA_MONSTER.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- 2026-05-10 recent cross-domain task: Ariel unit executor migration was implemented so Manifested Ariel A-E dispatch through Ariel-specific `CombatUnitRuntime` paths, and Ariel B/E shields now apply to selected plus manifested party units. Detailed records are in `boards/MON/ARIEL_MONSTER.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- 2026-05-10 recent cross-domain task: Ariel selected-unit shield expiry and Archangel Descent visual follow-up was implemented so 1P shields granted by 2P-5P Ariel tick outside selected-Ariel-only cooldown logic, selected/Manifested Ariel E use a dedicated battlefield-wide effect path, and selected/manifested shield timers now skip decay on the application frame to align duration semantics. Detailed records are in `boards/MON/ARIEL_MONSTER.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.

## Board Tree

### Monster

- `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`: archived common monster/player-monster creation rules, terms, skill-slot rules, and monster data history.
- `boards/MON/EVE_MONSTER.md`: Eve-specific skill/runtime implementation history.
- Character files: `boards/MON/VEGA_MONSTER.md`, `boards/MON/ARIEL_MONSTER.md`, `boards/MON/SEIN_MONSTER.md`, `boards/MON/RIN_MONSTER.md`.

### Combat

- `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`: archived common combat runtime.
- `boards/COMBAT/ENEMY_BLACKBOARD.md`: enemy spawn, target priority, enemy HP/projectiles.
- `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`: archived player/enemy projectile behavior.
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`: shock, freeze, chill, slow, vulnerability, shield, and other status effects.

### Run

- `boards/RUN/RUN_BLACKBOARD.md`: run flow, day progression, combat type, RunSession.
- `boards/RUN/REWARD_BLACKBOARD.md`: reward buttons, material rewards, skill-choice rewards.
- `boards/RUN/SAVELOAD_BLACKBOARD.md`: save/load and checkpoint planning.

### UI

- `boards/UI/UI_BLACKBOARD.md`: shared UI layout/edit-mode policy.
- `boards/UI/UI_BLACKBOARD.md`: shared UI layout/edit-mode policy, including active DebugScene/MainMenu UI routing after 2026-05-14 archive cleanup.
- `boards/ARCHIVE/DEBUGSCENE_UI_ARCHIVE_2026-05-14.md`: older DebugScene UI canvas, skill panel, enhancement modal, editable scene UI history.
- `boards/ARCHIVE/MAINMENU_UI_ARCHIVE_2026-05-14.md`: older main menu and monster selection UI history.
- `boards/UI/RUNSCENE_UI.md`: RunScene combat/reward UI.

### Data

- `boards/DATA/DATA_BLACKBOARD.md`: data pipeline overview.
- `boards/ARCHIVE/CSV_BLACKBOARD_ARCHIVE_2026-05-14.md`: archived CSV source data role and limitations.
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`: Unity static assets, `GameDataCatalog`, `MonsterDefinition`.

### Ops

- `boards/OPS/REVIEWER_BLACKBOARD.md`: reviewer wrapper and review flow.
- `boards/OPS/CODEX_CLI_BLACKBOARD.md`: Codex CLI setup and command findings.
- `boards/OPS/UNITY_MCP_BLACKBOARD.md`: Unity MCP bridge and usage notes.
- `boards/OPS/AUTOMATION_GUIDE.md`: automation responsibility rules.

### Archived Reports

- `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`: archived HTML/report work history after the active report board was removed from routing.

### Refactoring

- `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`: archived broad architecture refactor plans and implementation phase ordering.
- `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/COMBAT_STATE_OWNERSHIP_MAP.md`: archived Phase 0 combat mutable-state ownership map for the `CombatRuntimeController` structure split.

## Current Task Block

### Task title

Hierarchical board migration and routing rule update.

### Goals

- Replace the previous always-read `BLACKBOARD.md` workflow with `AGENTS.md` + `MDTREE.md` routing.
- Preserve the old detailed `BLACKBOARD.md` history.
- Split task history into domain-specific board files.
- Add rules requiring related board files to be updated together.

### Constraints

- Preserve evidence and old task history.
- Do not run Unity Play Mode gameplay verification.
- Do not run Code Reviewer without user permission.

### Role Owner

Code Builder

### Status

Completed.

### Next Actions

- Use `AGENTS.md` + `MDTREE.md` routing for future sessions and read `BLACKBOARD.md` only when the routed scope needs global status.
- Run build only when later tasks change code; this migration itself changed markdown only.

### Evidence

- Original detailed `BLACKBOARD.md` was archived to `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.
- `MDTREE.md` defines routing rules for MON, COMBAT, RUN, UI, DATA, and OPS boards, with report work routed through the related active domain board instead of an active report board.
- `AGENTS.md` now says to read `AGENTS.md` and `MDTREE.md` first, then route to related boards.
- `AGENTS.md` now states Code Reviewer execution requires explicit user permission.
- `AGENTS.md` keeps Unity-MCP Play Mode gameplay verification assigned to the user.
- 2026-05-02 validation: `Test-Path` confirmed `AGENTS.md`, `MDTREE.md`, root `BLACKBOARD.md`, the archive file, and all routed board files exist.
- 2026-05-02 validation: `Get-ChildItem boards -Recurse -File` listed the then-active MON, COMBAT, RUN, UI, DATA, OPS, and report board files plus `boards/ARCHIVE/BLACKBOARD_2026-04-30_PRE_HIERARCHY.md`.
- 2026-05-02 validation: `Select-String` over `boards/` for `## Migrated Task Blocks`, `## Task:`, `### Task title`, and `### Status` confirmed migrated task-block sections exist in the domain boards.
- 2026-05-02 validation: `run_codex.bat`, `codex_builder_reviewer.ps1`, and `codex_prompt.txt` all exist at the repository root.

### History

- 2026-04-30: User requested a hierarchical board structure to reduce token use and speed up task routing.
- 2026-04-30: Created domain board hierarchy under `boards/`, added `MDTREE.md`, and changed `BLACKBOARD.md` into a root index.
- 2026-05-02: Validated the root files, archived pre-hierarchy log, domain board hierarchy, migrated task-block structure, and reviewer-wrapper artifacts; the task is now complete.

## Recent Task: 2026-05-08 RunScene Prisoner Manifest Party

### Task title

RunScene prisoner choice, Manifest result storage, and limited Manifested party combat.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster/Data-spanning Manifest implementation.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for prisoner choice panels, Manifest result, next-day 2P+ party display, and limited A/basic auto combat.

### Evidence

- Changed `Pakuri/Assets/CSVdata/source/monster_skills.csv`, `Pakuri/Assets/Scripts/Run/RunSession.cs`, `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs`, `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs`, and added `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: Code Builder implemented the requested ordered RunScene Manifest flow and recorded domain-specific board evidence.

## Recent Task: 2026-05-08 Prisoner Offering Panel And Manifest Follow-up

### Task title

Fix RunScene prisoner Offering panel routing and Manifested party baseline state.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning follow-up.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for Offering opening `PrisonerOfferingPanel`, Manifest return marking the prisoner reward as used, and Manifested monsters using own HP/stat/A-skill nearest-enemy auto attack.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- Unity-MCP scene inspection confirmed `RunCombatCanvas/PrisonerOfferingPanel` and separate `RunCombatCanvas/PrisonerPanel` both exist.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported Offering opened the wrong panel, Manifested monster behavior needed correction, and Manifest return did not show the prisoner reward button as used.

## Recent Task: 2026-05-08 Manifested Party Member Growth State

### Task title

Make Manifested monsters behave as growable party-member monster states.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning Manifested monster growth fix.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification that Manifested monsters start from their own registered monster state, auto-cast registered learned skills at nearest enemies, and gain skills/modifiers through Offering.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunSession.cs`, `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, and `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `RunSession.cs` now has `RunMonsterState`, `PartyMembers`, monster-ID scoped learned skill/reward methods, and `RecordManifestedMonster(MonsterDefinition monster)`.
- `RunCombatUiController.cs` now builds Offering choices for selected plus Manifested target monsters and commits choices by `choice.MonsterId`.
- `CombatRuntimeParty.cs` now syncs Manifested learned active IDs to registered `SkillDefinition` entries and auto-casts them at the nearest living enemy.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP script refresh returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.
- `git diff --check` on the changed Run/Combat scripts completed with no whitespace errors, aside from Git LF-to-CRLF normalization warnings.

### History

- 2026-05-08: User clarified Manifested monsters should be equivalent to MainMenu-starting monsters added during gameplay, not unregistered weird-skill users, and should grow through Offering.
- 2026-05-08: Code Builder added per-monster run party state, made Offering target that state, and made Manifested combat use registered learned skills.

## Recent Task: 2026-05-08 Manifested Scene Slots And Summoner Back Button

### Task title

Use authored NPMonster slots and add summoner return.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning correction to use `CombatRoot/2PMonster` through `5PMonster`.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for Manifested monster slot activation order, MonsterPanel display, A/default skill behavior, and summoner Back to Reward behavior.

### Evidence

- Unity-MCP found `CombatRoot/EveUnit`, `CombatRoot/2PMonster`, `CombatRoot/3PMonster`, `CombatRoot/4PMonster`, and `CombatRoot/5PMonster`.
- Changed `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` to bind Manifested monsters to those scene slots.
- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` to add `PrisonerSummonerPanel/BackButton`.
- Saved `Pakuri/Assets/Scenes/RunScene.unity`; `RunScene.unity:5233` has `m_Name: BackButton`, and `:8429` has `m_Text: Back to Reward`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP script refresh returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User clarified that the scene already has `2PMonster` through `5PMonster` under `CombatRoot`, and requested using those slots plus adding a summoner return button.
- 2026-05-08: Code Builder changed Manifested runtime slot binding and added the `Back to Reward` path.

## Recent Task: 2026-05-08 Manifested Summon Sync And Vega A

### Task title

Fix first Manifested summon synchronization and Manifested Vega A three-projectile behavior.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Monster-spanning Manifest follow-up.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/MON/VEGA_MONSTER.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for first Summon/Continue application, Manifested Vega A three-projectile behavior, and Offering-acquired Manifested skill firing.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `RunCombatUiController.cs:702` and `:1246` refresh Manifested party state after Manifest success and Offering commit.
- `CombatRuntimeParty.cs:149` exposes `RefreshManifestedMonsterParty(RunSession session)`.
- `CombatRuntimeParty.cs:747` through `:774` queues Manifested Vega A as three projectiles, with 0.12 second spacing and 2x third-hit damage.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP editor state returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported first Manifested application delay, Manifested Vega A missing its three-projectile baseline, and requested checking Offering-acquired Manifested skills.
- 2026-05-08: Code Builder added immediate Manifested party refresh and Manifested Vega A-specific projectile burst behavior.

## Recent Task: 2026-05-08 Manifested Skill Visual Runtime Unification Follow-up

### Task title

Route Manifested non-projectile skill visuals through skill-kind effect dispatch.

### Goals

- Stop Offering-acquired Manifested non-projectile skills from always rendering as a thin beam.
- Use `SkillRuntimeKind` and `SkillEffectPrefab` for 2P-5P skill visuals, matching the selected-monster effect factory path where the current code structure allows.

### Constraints

- Detailed evidence is in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, and `boards/UI/RUNSCENE_UI.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for Offering-acquired Manifested B-E skills, checking that area/buff/execute skills show non-beam monster effect visuals.

### Evidence

- Changed `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `CombatRuntimeParty.cs:512` now calls `CreateManifestedSkillVisual(...)`.
- `CombatRuntimeParty.cs:896` dispatches Manifested visuals by `SkillRuntimeKind`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP script refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User requested not to keep patching Manifested-only simple skills and to make 1P selected monsters and 2P-5P Manifested monsters share the monster skill runtime/effect route as much as current code supports.

## Recent Task: 2026-05-08 Manifested Sustained Skill Duration Follow-up

### Task title

Keep Manifested sustained skill visuals alive for their monster runtime duration.

### Goals

- Fix Manifested sustained skills such as Eve Prism Ray, Frost Field, and Drone Beacon ending visually after the short fallback effect lifetime.
- Keep Manifested projectile and Vega A burst behavior intact.
- Confirm the current RunScene selected-monster path applies the selected monster to `EveUnit`.

### Constraints

- Detailed evidence is in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/MON/EVE_MONSTER.md`, `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, and `boards/UI/RUNSCENE_UI.md`.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Manifested Eve B lasts about 1.2 seconds, Eve C about 4 seconds, Eve E deploys a 5 second drone, and RunScene still applies the selected 1P monster to `EveUnit`.

### Evidence

- Changed `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `CombatRuntimeParty.cs` now resolves Manifested visual durations by skill ID for `eve-b`, `eve-c`, `eve-e`, `sein-d`, `vega-c`, `ariel-b`, and `ariel-c`.
- `CombatRuntimeParty.cs` now deploys Manifested Eve Drone Beacon as a timed drone runtime that fires Manifested projectiles every `EveDroneAttackPeriod`.
- `RunSceneBootstrap.BeginCombat(...)` calls `CombatRuntimeController.BeginConfiguredDay(...)`; `BeginConfiguredDay(...)` calls `ConfigureMonster(monster)`; `ConfigureMonster(...)` sets `selectedUnitSprite`; `EnsureAnchorVisuals()` applies that sprite to `eveAnchor` / `EveUnit`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP refresh completed; console warning/error read returned only an MCP client handler log.

### History

- 2026-05-08: User reported Manifested skill effect kinds were visible, but sustained skills such as Eve Drone Beacon, Frost Field, and Prism Ray ended far too quickly and asked whether RunScene applies the selected monster to `EveUnit`.

## Recent Task: 2026-05-08 Manifest Candidate Duplicate And Failure Popup

### Task title

Fix Manifest duplicate selected monsters, non-Vega mark leakage, and Manifest failure popup.

### Goals

- Track the global status of the Run/Reward/UI/Combat/Projectile/Monster/Vega Manifest follow-up.

### Constraints

- Detailed evidence is in `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, `boards/UI/RUNSCENE_UI.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/MON/VEGA_MONSTER.md`.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User performs Play Mode verification for ManifestButton roll timing, failure popup, no selected-monster duplicate Manifest, and non-Vega Manifested A attacks not applying Vega name marks.

### Evidence

- Changed `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`, and `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `RunCombatUiController.cs:367` binds `ManifestButton` to the roll method; `:391` no longer binds `SummonButton` to the roll method.
- `RunCombatUiController.cs:396` creates `PrisonerManifestFailurePopup`; `:657` and `:665` show it for failure cases.
- `RunCombatUiController.cs:791`, `RunSession.cs:321`, `RunSession.cs:334`, and `CombatRuntimeParty.cs:156` prevent selected-monster duplicate Manifest candidates/records/combat slots.
- `CombatRuntimeParty.cs:554`, `:617`, and `:1120` through `:1121` restrict Vega name-mark stacks to Manifested Vega A.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported non-Vega Manifested A attacks appeared to leave Vega marks, Eve could be duplicated by Manifest, and requested Manifest chance roll on `ManifestButton` with failure popup.

## Recent Task: 2026-05-10 Monster Shield Skill Review

### Task title

Review and fix Ariel/Eve shield runtime behavior.

### Goals

- Track global status for shield skill review across monster reference, combat, and status boards.
- Fix Eve F shield timing and application to lightning-skill allies.

### Constraints

- Detailed evidence is in `boards/MON/EVE_MONSTER.md`, `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel B/E and Eve F shield behavior in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Changed `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeEveSkills.cs` for Eve F.
- Existing shield changes in `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs`, `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs`, `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs`, and `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs` were inspected for Ariel selected/manifested shield timing and first-frame handling.
- Reference search found concrete shield skills for Ariel B/E and Eve F under `Pakuri/reference/2.Monster`; other shield mentions were generic pattern notes or enemy shield-target damage text.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP refresh reached idle; console error read returned only MCP client handler logs.

### History

- 2026-05-10: User asked to inspect all monster shield logic under `Pakuri/reference/2.Monster`, noting Eve shield appeared not to apply correctly.

- 2026-05-21 01:31:09 +09:00: Builder -> Reviewer loop started. Run directory: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260521_013109
- 2026-05-21 01:31:44 +09:00: Loop 1 Builder started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260521_013109\loop_01_builder.md
- 2026-05-21 01:32:16 +09:00: Loop 1 Builder finished with exit code 1.

## Recent Task: 2026-05-21 Ariel-D SingleAttack Target Fix

### Task title

Fix Ariel-D hitting all enemies after Mark-to-SingleAttack conversion.

### Goals

- Keep Ariel-D as `runtime_kind=SingleAttack`.
- Make `target_selection=HighestHealth` route Ariel-D into the single-target branch instead of the cover-all branch.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Claims are grounded in inspected `monster_skills.csv`, `InGameSkillDefinitionMapper.cs`, `SkillExecutors.cs`, and `InGameZoneSkillActor.cs`.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Run Code Reviewer stage if network execution is allowed.
- User verifies Ariel-D in Play Mode against multiple enemies.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Ariel-D row has `runtime_kind=SingleAttack`, `radius=0`, and `target_selection=HighestHealth`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now sets `single.Area.CoverAll = source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection)`.
- Before this fix, `SingleAttackSkillExecutor` ORed `skill.Area.CoverAll` with `skill.Targeting.CoverAll`, so Ariel-D's `radius=0` kept `areaCoversAll=true` and bypassed `InGameZoneSkillActor.ApplyAreaTick(...)`'s single-target branch.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity-MCP `Pakuri/Validate CSV Source Data` was executed; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-21: User reported Ariel-D appeared to affect all targets instead of the strongest enemy. Code inspection found `SingleAttackData.Area.CoverAll` still used only `radius <= 0f`; Builder aligned it with `target_selection`.

## Recent Task: 2026-05-22 Skill Execution Utility and Beam Width Refactor

### Task title

Fix Self targeting, prefab radius scale, and beam width enhancement runtime flow.

### Goals

- Make Self multi-effect targeting resolve only to the caster.
- Move common targeting, area radius, status application, and skill visual spawn behavior into utility surfaces.
- Add CSV/runtime support for `beam_width_bonus`.

### Constraints

- Role Owner is Code Builder.
- Details are mirrored in `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, `boards/DATA/DATA_BLACKBOARD.md`, and `boards/RUN/RUN_BLACKBOARD.md`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission is required by the repository role workflow.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Self multi-effects, SingleAttack prefab scaling, and Eve-B beam width behavior in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added `SkillTargetingUtility.cs`, `SkillAreaUtility.cs`, `SkillStatusApplyUtility.cs`, and `SkillVisualSpawnUtility.cs` under `Pakuri/Assets/Scripts2/InGame/Skills/Execution/`.
- `monster_skill_choices.csv` now has `beam_width_bonus`; `eve-b-trait-2` uses `beam_width_bonus=0.3` and no longer uses `radius_multiplier=1.3`.
- `SkillExecutionSnapshot.cs` exposes `BeamWidthBonus`; `SkillExecutors.cs` uses it for beam width.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP forced refresh removed missing utility type compiler errors; remaining console entries were Unity graph/MCP client handler exceptions, not script compiler errors.

### History

- 2026-05-22: User asked Code Builder to implement the reviewed Skills cleanup order.

## Recent Task: 2026-07-18 Scripts Unused Code Cleanup

### Task title

Remove repository-dead scripts, APIs, legacy skill-effect row paths, and overbroad type exposure under `Pakuri/Assets/Scripts`.

### Goals

- Remove types and methods whose declarations had no current code or Unity asset references.
- Remove the no-op passive executor and make typed executors require an explicit implementation.
- Remove the unpopulated legacy `SkillEffectRow` source path while preserving normalized effect-node construction.
- Reduce public exposure for implementation-only helper types.

### Constraints

- Role Owner is Code Builder.
- Existing skill, CSV, prefab, scene, and player-facing behavior must remain unchanged.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit permission is required.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies representative active and passive skills plus enemy attacks in Unity Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Deleted `Pakuri/Assets/Scripts/InGame/Skills/Execution/Actors/InGameEnemySkillHitboxActor.cs` and its `.meta`; repository search found no code, scene, prefab, or GUID references before deletion and no remaining asset references afterward.
- Removed empty `InGameContextManager`, `InGameResultManager`, `UnitRuntimeModel`, and the empty partial `PakuriCsvRuntimeData` declaration in `PakuriCsvRuntimeData.Types.cs`.
- Removed unused APIs and helpers including `ResolveEnemy`, `CreateLearnedActiveSet`, `CloneRuntimeCopy`, `RecordRewardChoice`, stage-specific enemy getters, `ResolveTargetGroupCenter`, `LoadOptionalCsvTable`, and the unused effect parameter helpers.
- Removed `PassiveSkillExecutor`; `TypedSkillExecutor<TSkillData>.Execute(...)` is now abstract, and every remaining registered typed executor has an override.
- Removed the unpopulated legacy `SkillEffectRow` dictionary/parser/validation/build/asset-reference branches; `BuildSkillEffects(...)` now returns normalized effect-owned node definitions.
- Reduced `SkillExecutorRegistry`, `SkillChoiceResolver`, `EnemyCombatState`, and `UnitResourceMutationService` to `internal`; EffectManager's serialized entry/group types are private nested types.
- `git diff --stat -- Pakuri/Assets/Scripts` reports 25 changed files, 11 insertions, and 828 deletions; `git diff --check -- Pakuri/Assets/Scripts` passed with only line-ending notices.
- Runtime and Editor `dotnet build --no-restore` each passed with 0 errors and the existing 2 MSB3277 assembly-version warnings.
- Unity-MCP forced script compilation, then reported `ready_for_tools=true`; an `Assets/Scripts`-filtered error read returned 0 entries. The unfiltered console contained one MCP package client-handler `Cannot access a disposed object` error, not a project script compiler error.

### History

- 2026-07-18: User explicitly switched to Code Builder and requested removal of the unnecessary code identified in the preceding code-based review.

## Recent Task: 2026-07-18 July-Only Active Board Compaction

### Task title

Archive every non-July task record from active COMBAT, DATA, MON, OPS, RUN, and UI boards.

### Goals

- Keep active domain context limited to July 2026 work.
- Preserve all removed history in a source-grouped archive.
- Keep archive history discoverable from active boards and `MDTREE.md`.

### Constraints

- Role Owner is Code Builder.
- No gameplay code, CSV, prefab, scene, or unrelated worktree content is changed by this documentation migration.
- Undated records are archived without inventing dates.

### Role Owner

Code Builder

### Status

Implemented and structurally validated.

### Next Actions

- Use the active domain boards for July work.
- Read `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` only when older or undated history is required.

### Evidence

- The migration retained 116 pre-existing July task blocks and archived 180 earlier or undated task records from 18 sources.
- Seventeen active board files now link to the new archive; `boards/UI/DAMAGE_METER_UI_HANDOFF.md` was removed after its content was normalized into the archive.
- Detailed commands, counts, and maintenance history are recorded in `boards/OPS/AUTOMATION_GUIDE.md`.

### History

- 2026-07-18: User directed Code Builder to leave only July work in the active domain boards and archive everything else.
