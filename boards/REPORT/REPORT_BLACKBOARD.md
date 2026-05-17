## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps active report task blocks after the 2026-05-12 archive pass; newer report tasks may be appended above older retained context.
- Source file: `boards/REPORT/REPORT_BLACKBOARD.md`.

## Task: 2026-05-17 Projectile Blueprint Markdown

### Task title

Create a standalone Markdown projectile blueprint for future InGame projectile skills.

### Goals

- Provide an AI-first implementation guide so future projectile skill work can start from a narrow, evidence-based file list instead of rereading the whole codebase.
- Mark common projectile behavior as supported, partial, or unsupported.
- Explicitly state that Vega-A timed three-projectile behavior, branch variants, bounce, homing, installed projectiles, multi-hitbox projectiles, and mark payloads require deliberate exceptions or reusable extensions.

### Constraints

- Role Owner is Designer.
- Documentation-only update; no C# script, scene, prefab, or CSV behavior was changed.
- Claims are based on inspected InGame projectile scripts, CSV rows, scene YAML, prefab YAML, and existing report-board evidence.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer

### Status

Completed as a new Markdown guide.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-17-projectile-blueprint.md` before implementing additional projectile skills.
- Code Builder should classify each new projectile behavior as common, partial, or exceptional before editing runtime code.

### Evidence

- Added `Pakuri/reference/Report/2026-05-17-projectile-blueprint.md`.
- `SkillExecutors.cs` contains `ProjectileSkillExecutor`, additional projectile, pierce, status, branch, prefab instantiate, and common target resolution paths.
- `InGameProjectileActor.cs` contains movement, trigger/distance hit, damage, status, branch spawn, pierce depletion, and destroy-boundary behavior.
- `InGameCombatManager.cs` contains `ResolveSkillEffectPrefab("eve-a")`, `InstantiateSkillPrefab(...)`, `ResolveProjectileDestroyBoundaryX()`, `ApplyDamage(...)`, and `ApplyStatus(...)`.
- `StatusEffectKind.cs`, `BaseUnitRuntimeModel.cs`, `MonsterUnitActor.cs`, and `EnemyUnitActor.cs` contain shared status enum, status runtime storage, ticking, and name-label display paths.
- `SkillData.csv`, `monster_skills.csv`, `SkillChoiceModifierData.csv`, `NewRunScene.unity`, and `Eve_A.prefab` were inspected for Eve-A projectile data, modifier data, scene prefab assignment, and actor/collider setup.

### History

- 2026-05-17: User asked for a Markdown projectile blueprint and requested explicit support/partial/unsupported marking plus special-behavior exception guidance.

## Task: 2026-05-17 InGame Current Implementation And Projectile Blueprint Report

### Task title

Create an updated HTML report comparing the Combat V2 roadmap with current InGame implementation and projectile blueprint work.

### Goals

- Compare the current implementation against `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Document newly implemented Eve-A runtime, status enum/label work, and `NewRunUnitSpawnManager` split.
- Add a projectile blueprint section explaining how later projectile skills such as Vega-A and Ariel-A should be implemented.

### Constraints

- Role Owner is Designer.
- Documentation-only update; no C# script, scene, prefab, or CSV behavior was changed.
- Claims must be based on inspected code, CSV rows, scene YAML, and command output.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer

### Status

Completed as a new HTML report.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-17-ingame-current-implementation-and-projectile-blueprint.html` as the current projectile-skill implementation guide.
- Before implementing Vega-A or Ariel-A special master effects, design shared hooks for last-shot, delayed projectile, impact AoE, and on-kill behavior.

### Evidence

- Added `Pakuri/reference/Report/2026-05-17-ingame-current-implementation-and-projectile-blueprint.html`.
- `Select-String` confirmed the report contains sections for roadmap comparison, `NewRunUnitSpawnManager`, Eve-A implementation, status enum labels, projectile blueprint, Vega-A/Ariel-A application, priorities, and evidence.
- `Select-String` over `NewRunSceneEntryManager.cs` found `RequireComponent(typeof(NewRunUnitSpawnManager))`, `unitSpawnManager`, `SpawnEnemyById(...)`, and `SpawnManifestedMonster(...)` delegation.
- `Select-String` over `NewRunUnitSpawnManager.cs` found selected player spawn, manifested monster spawn, enemy spawn, `RegisterPlayerMonster`, `RegisterEnemy`, spawn point resolution, and runtime root resolution.
- `Select-String` over `SkillExecutors.cs` and `InGameProjectileActor.cs` found projectile damage, status, branch, additional projectile, and pierce execution paths.
- `Select-String` over `StatusEffectKind.cs`, `BaseUnitRuntimeModel.cs`, `InGameCombatManager.cs`, `MonsterUnitActor.cs`, and `EnemyUnitActor.cs` found enum status runtime, stack label suffix, and actor refresh paths.
- `Select-String` over `monster_skills.csv`, `monster_skill_choices.csv`, and `SkillChoiceModifierData.csv` found Eve-A, Vega-A, Ariel-A skill and modifier data.
- `git diff --check -- Pakuri\reference\Report\2026-05-17-ingame-current-implementation-and-projectile-blueprint.html` passed.

### History

- 2026-05-17: User requested a new HTML report that compares current implementation with the 2026-05-14 roadmap, adds missing implementation updates, documents the NewRunUnitSpawnManager split, and records a projectile blueprint for future projectile skills.

## Task: 2026-05-16 InGame Roadmap Enemy Ownership Amendment

### Task title

Add StageManager enemy spawn and enemy-management ownership direction to the InGame roadmap.

### Goals

- Record that Stage encounter selection and enemy spawn scheduling already run through `NewRunStageManager`.
- Record that active enemy registration/count, death cleanup, and enemy simulation tick still have responsibilities inside `InGameCombatManager`.
- Add the roadmap direction that enemy lifecycle ownership should move toward StageManager or a Stage-owned enemy lifecycle service, while `InGameCombatManager` narrows to combat rule APIs and roster/query facade.

### Constraints

- Role Owner is Designer.
- Documentation-only update; no C# script, scene, prefab, or CSV behavior was changed.
- Claims must distinguish current implementation from target ownership direction.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- Before expanding Stage waves, design or implement a Stage-owned enemy lifecycle boundary so `InGameCombatManager` does not keep growing as the owner of enemy spawn/clear/update concerns.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- `Select-String` over `InGameCombatManager.cs` found `ActiveEnemyCount`, `RegisterEnemy`, `UnregisterUnit`, `RemoveUnitIfDead`, internal `UnitRosterService`, and `EnemyCombatSimulationSystem.Tick(...)`.
- `Select-String` over `NewRunStageManager.cs` found `SpawnEncounterRows(...)`, `entryManager.SpawnEnemyById(...)`, `WaitForEnemyClear()`, `SelectBossRows()`, and boss health multiplier handling.
- The roadmap now has section `4. 적 스폰 / 적 관리 책임 이관 방향`.

### History

- 2026-05-16: User requested adding to the Combat V2 roadmap that enemy spawning and enemy management currently associated with `InGameCombatManager.cs` should be transferred to StageManager.
- 2026-05-16: Designer amended the roadmap while separating current evidence from the target responsibility direction.

## Task: 2026-05-16 InGame Roadmap Current Implementation Refresh

### Task title

Refresh the Combat V2 InGame build roadmap against the current implemented files.

### Goals

- Compare `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` with the current InGame implementation.
- Remove stale claims that CSVData files are 0 byte or that already-implemented Phase3/Phase4/Stage/Reward work is still pending.
- Record completed, partial, and remaining roadmap areas using inspected file and CSV evidence.

### Constraints

- Role Owner is Designer.
- Documentation-only update; no C# script, scene, prefab, or CSV behavior was changed.
- Claims must stay grounded in inspected code, CSV rows, scene YAML, and command output.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer

### Status

Completed as an HTML report refresh.

### Next Actions

- Use the refreshed roadmap before planning the next InGame work slice.
- Recommended next design/implementation focus is temporary effect/status layering, Beam/Zone/Buff executor expansion, CSVdata/CSVData source-of-truth cleanup, and user Play Mode acceptance checks.

### Evidence

- Rewrote `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- `Get-ChildItem Pakuri\Assets\Scripts2\InGame -Recurse -Filter *.cs` listed current Core, Units, Skills/Data, Skills/Execution, Skills/Runtime, and UI scripts including `NewRunStageManager.cs`, `InGameUIManager.cs`, `SkillExecutors.cs`, and `InGameProjectileActor.cs`.
- `Import-Csv Pakuri\Assets\CSVdata\StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv` returned 11, 30, and 5 rows.
- `Import-Csv Pakuri\Assets\CSVdata\EnemyStat.csv` returned Stage 1 enemy rows for swordsman, shieldbearer, rogue, priest, guardian captain, attack captain, and hero Karin.
- `Import-Csv Pakuri\Assets\CSVdata\SkillData.csv` returned sample rows `eve-a` and `ariel-b`.
- `Select-String` over `NewRunScene.unity` found `NewRunSceneEntryManager`, `InGameCombatManager`, `NewRunStageManager`, `InGameUIManager`, `RewardPanel`, and Stage CSV TextAsset references.
- `Select-String` over `InGameCombatManager.cs` found `ApplyDamage`, `GrantShield`, `Heal`, `RemoveUnitIfDead`, and `ShowDamageIfChanged`.
- `Select-String` over `EnemyCombatSimulationSystem.cs` found Stage 1 enemy skill execution paths for `Slash`, `ShurikenThrow`, `Heal`, `ShieldUp`, `GuardianFlag`, `ChargeCommand`, and `SacredSwordWave`.
- `Select-String` over `SkillExecutors.cs` found `ProjectileSkillExecutor`, `ShieldSkillExecutor`, `ApplyDamage`, and `GrantShield` calls.

### History

- 2026-05-16: User asked to compare current implementation against the 2026-05-14 Combat V2 build roadmap and update that HTML report.
- 2026-05-16: Designer refreshed the roadmap as a current implementation status and next-work guide.

## Task: 2026-05-15 InGame Roadmap Phase4-B Completion Update

### Task title

Update the InGame build roadmap after Code Builder implemented Phase4-B skill execution contracts.

### Goals

- Mark Phase4-B as completed in the roadmap.
- Record that execution contracts, choice modifier snapshot flow, registry, and no-effect executors exist.
- Keep actual damage, shield, status, projectile/beam/zone runtime, and trigger relay marked as future work.

### Constraints

- Role Owner is Code Builder.
- Report claims must match inspected implementation and build evidence.
- Do not claim Play Mode skill behavior is implemented.

### Role Owner

Code Builder

### Status

Completed as an HTML report amendment.

### Next Actions

- Use the Phase4-C row as the next implementation target for minimum Eve-A damage and Ariel-B shield behavior.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- The Phase4-B row now uses `<span class="tag done">완료</span>`.
- The row cites `Assets/Scripts2/InGame/Skills/Execution`, `InGameCombatManager`, `NewRunSceneEntryManager`, and `NewRunScene.unity` choice modifier CSV wiring.
- Runtime/editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-15: Code Builder implemented Phase4-B and amended the roadmap completion status.

## Task: 2026-05-15 InGame Verification Section Reorder And Structure Tree

### Task title

Reorder the InGame implementation verification report and add a visual monster/enemy structure tree.

### Goals

- Move the 2026-05-10 structure proposal comparison section from section 7 to section 1.
- Add a section 2 visual structure tree using `|_` notation for monster, enemy, combat manager, service, and planned skill paths.
- Remove the top summary card that labels Play Mode verification as the user area.
- Keep unimplemented skill executor / relay / projectile / beam / zone / shield / temporary effect items visible as planned future work.

### Constraints

- Role Owner is Designer.
- No C# implementation, scene edit, prefab edit, or Play Mode verification in this task.
- Claims must stay based on the report and inspected InGame files.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- Use the reordered report as the first-read structure summary before Phase4-B.
- Code Builder should treat the section 2 tree as documentation of current boundaries and planned skill boundaries, not as proof that planned skill files already exist.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-15-ingame-implemented-structure-verification.html`.
- The report now has section `1. 2026-05-10 구조 제안 대비 검증`.
- The report now has section `2. 몬스터와 적 구조 시각화` with `|_` tree notation for `NewRunSceneEntryManager`, `MonsterUnitActor`, `MonsterUnitRuntimeModel`, `EnemyUnitActor`, `EnemyUnitRuntimeModel`, `InGameCombatManager`, `UnitRosterService`, `EnemyCombatSimulationSystem`, `UnitResourceMutationService`, and planned skill executor/relay/effect items.
- Removed the top summary card containing `Play Mode 검증`, `사용자 영역`, and `Codex는 Play Mode를 실행하지 않는다...`.
- `Select-String` confirmed no remaining top-level `Play Mode 검증`, `사용자 영역`, or `Codex는 Play Mode` card text in the h2/h3 scan output.

### History

- 2026-05-15: User asked to move section 7 to section 1, add a visual `|_` monster/enemy structure expression as section 2, pre-mark unimplemented skills as planned, and delete the Play Mode verification user-area block.

## Task: 2026-05-15 InGame Verification God Class And Shared Target Amendment

### Task title

Amend the InGame implementation verification report with God Class and shared target / temporary effect checks.

### Goals

- Compare the current implemented InGame structure against `2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- Check whether the current InGame structure has the shared target / temporary effect problems described by `2026-05-10-shared-combat-target-and-temporary-effect-design.html`.
- Keep the conclusion grounded in inspected code and report files.

### Constraints

- Role Owner is Designer.
- No C# implementation, scene edit, prefab edit, or Play Mode verification in this task.
- Do not claim absent executor/effect/modifier files exist.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- Phase4-B should keep skill executor, relay, temporary effect, and modifier responsibilities outside `InGameCombatManager` and `NewRunSceneEntryManager`.
- If wave spawning, pooling, run reward, or skill load logic expands, split `NewRunSceneEntryManager` into narrower bootstrap/spawn/data-resolve services.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-15-ingame-implemented-structure-verification.html`.
- Inspected `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`; it identifies the old `CombatRuntimeController` as a shared-state combat object and recommends state ownership, projectile/effect/drone, enemy simulation, selected unit combat, and adapter-boundary separation.
- Inspected `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html`; it proposes `CombatTargetModel.ActiveEffects`, `TemporaryEffectInstance`, common modifier aggregation, `ApplyTemporaryEffect(...)`, `GrantShield(...)`, and shield subsystem separation.
- Inspected `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`; it owns `UnitRosterService`, `EnemyCombatSimulationSystem`, and `UnitResourceMutationService`, and delegates resource mutation / actor refresh rather than directly owning all detailed logic.
- Inspected `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs`; it currently owns bootstrap, selected monster spawn, three enemy spawn sequence, catalog/definition resolve, prefab resolve, and combat manager registration, so it is a future growth-risk boundary.
- Inspected `Pakuri/Assets/Scripts2/InGame/Core/UnitResourceMutationService.cs`, `BaseUnitRuntimeModel.cs`, `UnitStateBucket.cs`, `MonsterUnitActor.cs`, and `EnemyUnitActor.cs`; current resource/shield display is common, but active temporary effects and modifier aggregation are not implemented.
- `Select-String` over `Scripts2/InGame` found no current `ActiveEffects`, `TemporaryEffect`, `StatModifier`, `ModifierState`, `SkillExecutorRegistry`, `SkillExecutionSnapshot`, or `SkillHitboxRelay` implementation.

### History

- 2026-05-15: User asked to include whether the implemented structure has the God Class problem from the 2026-05-10 controller proposal and whether it still has the shared combat target / temporary effect problems from the 2026-05-10 shared-target proposal.

## Task: 2026-05-15 InGame Implemented Structure Verification HTML

### Task title

Create an evidence-based HTML verification report for the current InGame implementation structure.

### Goals

- Verify current data loading and assignment flow.
- Verify current monster and enemy runtime/model/actor structure.
- Verify what skill work is implemented and what remains future work.
- Clearly separate current implementation from planned Phase4-B/C direction.

### Constraints

- Role Owner is Designer.
- No C# implementation, scene edit, prefab edit, or Play Mode verification in this task.
- Claims must be based on inspected files and command output.

### Role Owner

Designer

### Status

Completed as a new HTML verification report.

### Next Actions

- Use the report before Phase4-B implementation to confirm the current boundaries.
- Code Builder should keep CSVData source-of-truth migration separate from skill executor implementation unless explicitly requested.

### Evidence

- Added `Pakuri/reference/Report/2026-05-15-ingame-implemented-structure-verification.html`.
- Inspected `NewRunSceneEntryManager.cs`, `InGameCombatManager.cs`, `EnemyCombatSimulationSystem.cs`, `UnitResourceMutationService.cs`, `UnitFactory.cs`, `UnitRosterService.cs`, monster/enemy runtime model files, monster/enemy actor files, InGame skill data files, and Phase4-A skill runtime files.
- `Import-Csv` over `Pakuri/Assets/CSVData/MonsterStat.csv`, `EnemyStat.csv`, and `SkillData.csv` showed Eve/Ariel, Warrior/Rogue/Priest, and `eve-a`/`ariel-b` sample rows.
- `Select-String` over `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` confirmed serialized fallback catalog, five monster prefab fields, three enemy prefab fields, enemy IDs, Y spawn range, spawn interval, and enemy combat simulation flags.
- `Get-ChildItem Pakuri/Assets/Prefab/Monster -Filter *.prefab` listed five monster prefabs; `Get-ChildItem Pakuri/Assets/Prefab/Enemy -Filter *.prefab` listed three stage-one enemy prefabs.
- `Test-Path Pakuri/Assets/CSVdata/source` returned `False`, so the report records legacy CSV auto-sync risk separately from current fallback catalog usage.

### History

- 2026-05-15: User requested an HTML that verifies the structures implemented so far, covering data load/assignment, monster/enemy structure, and current/future skill implementation.

## Task: 2026-05-15 InGame Roadmap Skill Hitbox Relay Amendment

### Task title

Update the InGame build roadmap with projectile, beam, and zone hitbox relay direction.

### Goals

- Record that skill prefabs may use trigger colliders for contact signals.
- Keep actual damage, status, pierce, tick, and duplicate-hit logic outside prefabs.
- Set Beam direction to a long trigger-collider beam prefab whose runtime adjusts length, rotation, and duration.
- Preserve the no-hardcoded-skill-ID scaling rule for 25+ skills.

### Constraints

- Role Owner is Designer.
- No C# implementation, scene edit, prefab edit, or Play Mode verification in this task.
- Claims must stay grounded in the current roadmap and inspected InGame skill data/runtime files.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- Code Builder Phase4-B should add the relay/executor contract without applying actual skill effects.
- Code Builder Phase4-C should use the relay/runtime path when implementing minimum projectile and shield samples.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Phase4-B now states that Projectile, Beam, and Zone prefabs use trigger colliders only to send contact signals through a `SkillHitboxRelay`-style contract.
- Phase4-C now states that Beam uses a long trigger-collider beam prefab, with runtime-controlled length, rotation, and duration.
- Added section `3-2. Skill hitbox / prefab 판정 방향` describing Projectile, Beam, and Zone prefab responsibilities versus runtime responsibilities.
- The data principle now states that `SkillEffectPrefab` and `ProjectilePrefab` are for presentation and trigger-signal relay, not hidden skill logic.

### History

- 2026-05-15: User chose the Beam implementation direction as a long trigger-collider prefab and asked to update the InGame build roadmap.

## Task: 2026-05-15 InGame Roadmap Phase4-A Completion Update

### Task title

Update the InGame build roadmap after Code Builder implemented Phase4-A skill runtime state.

### Goals

- Mark Phase4-A as completed in `3. Phase별 작업`.
- Record the implemented files for skill runtime state.
- Keep Phase4-B/C marked as pending because executor/effect behavior is not implemented yet.

### Constraints

- Role Owner is Code Builder.
- Report claims must match inspected implementation files and build output.
- Do not claim projectile, damage, shield, target query, or Play Mode skill behavior was implemented.

### Role Owner

Code Builder

### Status

Completed as an HTML report amendment.

### Next Actions

- Use the Phase4-B row as the next implementation target for executor interface/registry and choice snapshot resolution.
- Keep minimum `eve-a` and `ariel-b` effect execution in Phase4-C.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Section `3. Phase별 작업` now marks `Phase4-A` as completed.
- The Phase4-A row now cites `Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs`, `UnitSkillRuntimeSet.cs`, `SkillRuntimeFactory.cs`, and `BaseUnitRuntimeModel.SkillRuntime`.
- Runtime and editor builds completed with 0 errors and existing warnings.
- Unity-MCP console read after force refresh showed only MCP client handler logs.

### History

- 2026-05-15: Code Builder implemented Phase4-A and amended the roadmap status.

## Task: 2026-05-15 InGame Roadmap Skill Runtime And Learning Amendment

### Task title

Update the InGame build roadmap with Phase4-A skill runtime, run learning, enhancement, and scalable executor guidance.

### Goals

- Expand section `3. Phase별 작업` in the InGame roadmap with the planned monster skill-use flow.
- Clarify that Phase4-A creates `SkillRuntimeInstance` state, not actual skill effects.
- Clarify how `RunSession` learned/choice state flows into `UnitStateBucket` and affects skill runtime/execution.
- Record the no-hardcoded-skill-ID scaling rule for 25+ skills.

### Constraints

- Role Owner is Designer.
- No C# implementation, scene edit, prefab edit, data edit, or Play Mode verification in this task.
- Claims must be grounded in inspected current files and the amended report.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- Use the updated Phase4-A row as the next Code Builder handoff when skill runtime work starts.
- Keep actual damage/shield effects for Phase4-C executor work, not Phase4-A runtime-state work.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Section `3. Phase별 작업` now expands `Phase4-A` to create `SkillRuntimeInstance` and a unit skill runtime storage path for cooldown, casting, magazine, reload, active duration, and tick state.
- Section `3. Phase별 작업` now expands `Phase4-B` to include executor registry plus choice resolution through a calculated execution snapshot instead of mutating `SkillData`.
- Section `3. Phase별 작업` now expands `Phase4-C` to route `eve-a` damage and `ariel-b` shield through `InGameCombatManager.ApplyDamage(...)` and `GrantShield(...)`.
- Section `3. Phase별 작업` now expands `Phase5-A` to describe `RunSession` / `RunMonsterState` learned and chosen choice state flowing into `UnitStateBucket`.
- Added subsections `3-1. Monster 스킬 사용 흐름` and `3-2. 런 중 스킬 학습과 강화 흐름`.
- Added an `확장성 기준` note stating that 25+ skills should reuse type-based executors and isolate exceptions through custom executor or behavior hook paths rather than accumulating skill-ID conditionals.
- Current evidence files inspected for the design include `RunSession.cs`, `UnitStateBucket.cs`, `SkillChoiceEffectSpec.cs`, `SkillData.cs`, `SkillBlueprintSpecs.cs`, and `InGameSkillCatalog.cs`.
- `git diff --check` on the amended HTML passed with only LF-to-CRLF normalization warnings.

### History

- 2026-05-15: User asked how run-time skill learning/enhancement should work and whether the structure can support at least 25 skills without hardcoding.
- 2026-05-15: User asked to update `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html` and add the details to section `3. Phase별 작업`.

## Task: 2026-05-15 InGame Roadmap Phase3-A Completion Update

### Task title

Update the InGame build roadmap after Code Builder implemented Phase3-A combat roster ownership.

### Goals

- Mark Phase3-A as completed in the roadmap.
- Record the implemented owner boundary: `InGameCombatManager` owns `UnitRosterService`, while movement/targeting/attack/damage remain later systems.
- Keep the report aligned with actual changed files and scene wiring.

### Constraints

- Role Owner is Code Builder.
- Do not claim gameplay success; Play Mode verification is user-owned.
- Do not mark Phase3-B or Phase3-C complete.

### Role Owner

Code Builder

### Status

Completed as an HTML report amendment.

### Next Actions

- Use the roadmap Phase3-B row as the next implementation target for movement, targeting, and basic attacks.
- Keep HP decrease testing deferred until Monster skills and enemy attacks exist.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Section `3. Phase별 작업` now marks `Phase3-A` as completed.
- The Phase3-A row now states that `InGameCombatManager` owns `UnitRosterService`, and `NewRunSceneEntryManager` registers the spawned selected monster and first enemy into the manager roster.
- The Phase3-A row keeps movement, targeting, attack, buff/shield, damage, and Actor refresh details deferred to Phase3-B/C.
- Implementation evidence exists in `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs`, `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs`, and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`.

### History

- 2026-05-15: Code Builder implemented the Phase3-A roster ownership boundary and updated the roadmap completion status.

## Task: 2026-05-15 InGame Roadmap Phase3 Combat Loop Amendment

### Task title

Update the InGame build roadmap Phase table after Phase2-B enemy spawn/model/actor binding and the Phase3 combat-loop structure decision.

### Goals

- Mark Phase2-B enemy spawn/model/actor-binding scope as completed in the roadmap.
- Mark the separated NewRunScene test entry path as completed for the current selected monster plus first enemy spawn scope.
- Record that `InGameCombatManager` should orchestrate the combat loop but not directly own movement, targeting, attack, damage, shield, and Actor refresh details.
- Record Phase3 work slices for roster ownership, enemy movement/targeting/basic attack, damage/shield services, and dirty Actor refresh.

### Constraints

- Role Owner is Designer.
- No C# implementation, scene edit, prefab edit, or Play Mode verification in this task.
- The HP real-time decrease path is documented as a later test after Monster skills and enemy attacks exist.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- Future Code Builder work should start Phase3-A by separating combat runtime ownership before implementing enemy movement/attack logic.
- User verifies Phase2-B/Phase2-C gameplay appearance in Unity Play Mode.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Section `3. Phase별 작업` now marks `Phase2-B` as completed and states that `Stage1_Warrior_Unit.prefab` spawns at `SpawnPoint.x` with Y `-5~+5` and binds `stage1-swordsman` `EnemyUnitRuntimeModel` to `EnemyUnitActor`.
- Section `3. Phase별 작업` now marks `Phase2-C` as completed for the current `NewRunSceneEntryManager` path separated from `RunSceneBootstrap`.
- Section `3. Phase별 작업` now defines `Phase3-A` as combat runtime ownership separation where `InGameCombatManager` is the loop orchestrator and detailed logic belongs to systems/roster.
- Section `3. Phase별 작업` now defines `Phase3-B` as enemy movement, targeting, and basic attack systems, including melee, ranged, and shield-support enemies.
- Section `3. Phase별 작업` now adds `Phase3-C` for damage, status, shield, and Actor refresh services, including dirty HP UI refresh and later HP decrease testing after Monster skills/enemy attacks are implemented.

### History

- 2026-05-15: User asked whether `InGameCombatManager` should own all monster/enemy movement, targeting, basic attack, damage, and Actor refresh for future 100+ enemies.
- 2026-05-15: User clarified that enemies can include ranged attackers and shield-support enemies, and that HP decrease testing should wait until Monster skills and enemy attacks are implemented.
- 2026-05-15: User asked to update the corresponding Phase section in `2026-05-14-combat-v2-build-roadmap.html`.

## Task: 2026-05-14 InGame Roadmap MonsterHpBar Scale Amendment

### Task title

Reflect user-authored `MonsterHpBar` Scale ownership in the InGame build roadmap.

### Goals

- Record that the visible `MonsterHpBar` size is controlled by the user's prefab Transform values, not by Code Builder runtime logic.
- Update Phase2-B wording so Code Builder preserves user-authored HP bar Scale while continuing Actor/Model binding work.
- Keep the report evidence-based against inspected prefab YAML and the existing roadmap.

### Constraints

- Role Owner is Designer.
- No C# implementation, scene edit, prefab edit, or Play Mode verification in this task.
- Do not claim visual success beyond inspected prefab YAML values.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- Future Code Builder work must not reset `MonsterHpBar` Transform Scale while updating HP ratio/text logic.
- User verifies the visual result in Unity Play Mode or Scene View.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- `Select-String` over `Pakuri/Assets/Prefab/Monster/Ariel_Unit.prefab`, `Eve_Unit.prefab`, `Sein_Unit.prefab`, `Vega_Unit.prefab`, and `Rin_Unit.prefab` found `MonsterHpBar` root scales around `{x: 3.3, y: 1.7, z: 1.35}`.
- The same prefab search found `Background` and `Fill` scales of `{x: 20, y: 2.5, z: 1}` and sprite renderer sorting orders `34`, `35`, and `36`.
- The roadmap now states that `MonsterHpBar` position, Scale, visible size, and child SpriteRenderer layout are user-authored prefab responsibility, while runtime code should only update HP ratio/text.

### History

- 2026-05-14: User said they directly modified the Scale values and asked to update `2026-05-14-combat-v2-build-roadmap.html`.

## Task: 2026-05-14 InGame Roadmap CSVData Timing Amendment

### Task title

Reflect CSVData pipeline timing in the InGame build roadmap report.

### Goals

- Add the timing relationship between `2026-05-14-csvdata-transition-roadmap.html` and `2026-05-14-combat-v2-build-roadmap.html`.
- Make clear that CSVData schema/sample rows should be fixed before deeper Phase2-B prefab binding.
- Make clear that the new CSV loader/unit mapping should happen around Phase2-B/Phase2-C and that `SkillData.csv` mapping is required before Phase4 skill execution.

### Constraints

- Role Owner is Designer.
- No C# implementation, CSV data entry, scene edits, prefab edits, or Play Mode verification in this task.
- Claims must be based on inspected HTML reports, current CSV file length checks, and current `Scripts2/InGame` dependency search results.

### Role Owner

Designer

### Status

Completed as an HTML report amendment.

### Next Actions

- CSVData Phase0~2 header/schema and minimum sample rows are implemented; Code Builder should proceed toward Phase2-B actor binding while keeping bindings ID-based.
- Follow Phase2-B with CSVData Phase3~5 loader, unit mapping, and `SkillData.csv` mapping before Phase4 skill execution.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Added section `2-1. CSVData 파이프라인 삽입 타이밍`.
- Updated section `4. 다음 Code Builder 작업 추천` so the next recommendation is CSVData Phase0~2, then Phase2-B, then CSVData Phase3~4, with CSVData Phase5 required before Phase4.
- Updated section `5. 데이터 연결 원칙` to state Phase1-C, Phase1-D, and Phase2-A are completed bridge validations and the CSVData transition is now inserted between later InGame phases.
- Updated section `6. Code Builder 인수 기준` to list CSVData Phase0~2, Phase2-B, and CSVData Phase3~5 acceptance criteria.
- `Get-Item Pakuri/Assets/CSVData/EnemyStat.csv, MonsterStat.csv, SkillData.csv` returned all three files with length `0`.
- `Get-ChildItem Pakuri/Assets/Scripts2/InGame -Recurse -File -Filter *.cs | Select-String -Pattern "PakuriCsvRuntimeData|PakuriDataManager|MonsterDefinition|EnemyDefinition|SkillDefinition|Pakuri\.Data"` found current InGame legacy data dependencies.
- 2026-05-14 Code Builder follow-up: `Pakuri/Assets/CSVData/MonsterStat.csv`, `EnemyStat.csv`, and `SkillData.csv` now contain Phase0~2 headers and minimum rows.
- `Import-Csv` over the three CSVData files returned Eve/Ariel monster rows, `stage1-swordsman`, `eve-a`, and `ariel-b`.

### History

- 2026-05-14: User asked how to time the CSVData pipeline work against the existing InGame build roadmap.
- 2026-05-14: User asked to reflect that timing in `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`; Designer amended the HTML report and recorded the evidence.
- 2026-05-14: Code Builder implemented the CSVData Phase0~2 seed files referenced by this roadmap amendment.

## Task: 2026-05-14 CSVData Source Transition Roadmap HTML

### Task title

Create a roadmap for replacing legacy data/runtime references with new `Assets/CSVData` source files.

### Goals

- Record the user's decision direction that `Assets/Legacy` should become reference-only and not runtime-authoritative.
- Define the phased order from CSV schema creation through loader implementation, unit model mapping, skill data mapping, prefab binding, skill execution, and legacy deactivation.
- Keep the report evidence-based against inspected files and command output.

### Constraints

- Role Owner is Designer.
- No C# implementation, CSV header/data entry, scene edits, prefab edits, or Play Mode verification in this task.
- The report must state that moving files under `Assets/Legacy` is not enough to disable them while they remain compiled or referenced.

### Role Owner

Designer

### Status

Completed as an HTML design roadmap.

### Next Actions

- Code Builder should implement the roadmap in small slices: CSV schema, seed data, new loader, unit mapping, skill mapping, prefab binding, skill execution, then legacy deactivation checks.
- Before declaring Legacy disabled, verify compile targets, scene references, Resources references, and `Scripts2/InGame` legacy type references.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-csvdata-transition-roadmap.html`.
- `Get-Item Pakuri\Assets\CSVData\EnemyStat.csv, MonsterStat.csv, SkillData.csv` confirmed all three new CSV files exist and currently have length `0`.
- `Get-ChildItem Pakuri\Assets\Legacy` confirmed `CSVdata`, `Data`, and `Scripts` exist under `Assets/Legacy`.
- `Select-String Pakuri\Assembly-CSharp.csproj -Pattern "Legacy"` confirmed legacy scripts are still included as compile targets.
- `Select-String Pakuri\Assets\Scripts2 -Pattern "Pakuri.Data|MonsterDefinition|SkillDefinition|PakuriCsvRuntimeData"` confirmed current InGame scripts still reference legacy data types and loaders.
- `Get-ChildItem Pakuri\reference\2.Monster` and `Get-ChildItem Pakuri\reference\5.enemy` confirmed monster and enemy reference documents exist.

### History

- 2026-05-14: User proposed making `Assets/CSVData/EnemyStat.csv`, `MonsterStat.csv`, and `SkillData.csv` the new source of truth and asked for an HTML work-order summary.

## Task: 2026-05-14 InGame Phase2-A Roadmap Update

### Task title

Update the InGame roadmap after Phase2-A base unit model split.

### Goals

- Mark Phase2-A as completed in the roadmap report.
- Record Eve and `stage1-swordsman` as the verified model creation samples.
- Record that prefabs are user-authored in Unity Editor and not generated by Code Builder.
- Record that Definition skill/projectile tuning is deferred to later SkillData mapper work.
- Keep Phase2-B and later actor/scene/combat tasks pending.

### Constraints

- Role Owner is Code Builder.
- Do not create a new report file.
- Report claims must match inspected implementation files and command output.

### Role Owner

Code Builder

### Status

Completed.

### Next Actions

- Use the roadmap as the sequencing reference for Phase2-B actor binding on user-authored prefabs.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Phase2-A now points to `UnitFactory.cs`, `BaseUnitRuntimeModel.cs`, `MonsterUnitRuntimeModel.cs`, `EnemyUnitRuntimeModel.cs`, `UnitDefenseRuntime.cs`, and `InGameTestDataManager.cs`.
- The roadmap says Code Builder does not create Monster/Enemy/Skill prefabs by code; the user creates them under `Pakuri/Assets/Prefab`.
- The roadmap says skill/projectile tuning is split later through `SkillData` mapper work.
- Unity-MCP Editor code execution returned `ok=True|monster=eve|monsterHp=220|learnedA=1|enemy=stage1-swordsman|enemyHp=100`.

### History

- 2026-05-14: Code Builder updated the roadmap report while implementing Phase2-A.
- 2026-05-14: Code Builder updated the roadmap report again after the user selected manual prefab authoring and a base unit runtime model split.

## Task: 2026-05-14 InGame Phase1-D Roadmap Update

### Task title

Update the InGame roadmap after Phase1-D validation implementation.

### Goals

- Mark Phase1-D as completed in the roadmap report.
- Replace old CombatV2 path/class names in the roadmap with current InGame names where Phase1-D is described.
- Record the validator and editor menu files as the implementation handoff.

### Constraints

- Role Owner is Code Builder.
- Do not create a new report file for this phase.
- Report claims must match inspected implementation files and build output.

### Role Owner

Code Builder

### Status

Completed.

### Next Actions

- Keep the roadmap as the sequencing reference for Phase2-A.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Phase1-D now points to `Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs` and `Assets/Scripts2/InGame/Editor/InGameSkillDataValidationMenu.cs`.
- `Select-String` found no remaining `CombatV2` / `Combat V2` / `Assets/Scripts2/CombatV2` references in the roadmap after the update.

### History

- 2026-05-14: Code Builder updated the roadmap report while implementing Phase1-D validation.

## Task: 2026-05-14 InGame Final Ingame Structure HTML

### Task title

Create a InGame final ingame structure and class responsibility report.

### Goals

- Explain how ingame combat should run if `2026-05-14-combat-v2-build-roadmap.html` is completed through Phase8-A.
- Describe class responsibility, reference direction, service boundaries, and runtime execution order.
- Clearly separate currently existing classes from proposed/future classes.

### Constraints

- Role Owner is Designer.
- Do not implement C# logic, scene wiring, prefab authoring, or Play Mode verification in this task.
- Base the report on inspected code, scene YAML, board records, and the existing roadmap.

### Role Owner

Designer

### Status

Completed as an HTML design report.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html` as the high-level target structure reference before Phase1-D and Phase2-A implementation.
- Keep proposed services such as `SkillRuntimeInstance`, `SkillExecutorRegistry`, `TargetQueryService`, `DamageService`, and `StatusEffectService` marked as future work until Code Builder creates them.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html`.
- Inspected `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Inspected `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `InGameContextManager.cs`, and `InGameResultManager.cs`; all three currently exist as empty shells.
- Inspected `Pakuri/Assets/Scripts2/InGame/Units/UnitRuntimeModel.cs`; it stores `Identity`, `Stats`, `Resources`, `State`, `AutoAttackEnabled`, and `AutoSkillEnabled`.
- Inspected `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs`, `EnemyUnitActor.cs`, `UnitFactory.cs`, and `UnitRosterService.cs`; they currently exist as empty shells.
- Inspected `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs`, `SkillBlueprintSpecs.cs`, `InGameSkillCatalog.cs`, and `InGameSkillDefinitionMapper.cs`.
- Inspected `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs`, `RunStartContext.cs`, and `Pakuri/Assets/Scripts/Run/Session/RunSession.cs` for current Run-to-combat input and learned-state ownership.
- `Select-String` over `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` confirmed `BG`, `1PSpawnPoint` through `5PSpawnPoint`, `GameManager`, and `Nexus`.
- `Get-ChildItem Pakuri\Assets\Prefab -Directory` listed `Enemy`, `Monster`, and `Skill`; `Test-Path Pakuri\Assets\SO` returned `True`.
- Updated section 1 of `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html` to remove the current-state evidence table and state the completed target: common unit inheritance/target contracts solve the temporary-effect split described in `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html`, and skill Blueprint data follows `C:\TowerDefence_Pakuri\towerdefense_pakuri_docs\docs\dev\skill-class-design.md`.
- Updated `Pakuri/reference/Report/2026-05-14-combat-v2-final-ingame-structure.html` again so class names and paths refer to `InGame`, including `InGameCombatManager`, `InGameContextManager`, `InGameResultManager`, `InGameSkillCatalog`, and `Pakuri/Assets/Scripts2/InGame`.
- Verified the report rename against implementation evidence: `Pakuri/Assets/Scripts2/InGame` exists, `Pakuri/Assembly-CSharp.csproj` references the new paths, and Unity-MCP reflection found `Pakuri.InGame.InGameCombatManager`.

### History

- 2026-05-14: User asked for an HTML report explaining what the ingame structure, class interactions, references, and responsibilities would be if the InGame build roadmap is completed.
- 2026-05-14: User asked to remove the “evidence/current state” section from the final ingame structure report and replace it with the common unit inheritance plus reusable skill Blueprint completion message.
- 2026-05-14: User asked Code Builder to reflect the CombatV2-to-InGame rename in the HTML report and related boards.

## Task: 2026-05-14 InGame Build Roadmap HTML

### Task title

Create a InGame implementation-order roadmap report.

### Goals

- Decide whether data should be connected before or after the core InGame skeleton.
- Record a phased roadmap from the completed skeleton and Blueprint data work to sample data, unit binding, skill execution, compatibility bridge, full data expansion, and Run integration.
- Save the roadmap as an HTML report.

### Constraints

- Role Owner is Designer.
- Do not implement C# logic, Unity scenes, ScriptableObject assets, or data migration in this task.
- Base the roadmap on inspected existing files and board records.

### Role Owner

Designer

### Status

Completed as an HTML report.

### Next Actions

- Phase1-C is now implemented; next recommended task is Phase1-D validation for mapped InGame skill data.
- Keep full 50-skill data entry deferred until validation and minimum execution paths are proven.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-build-roadmap.html`.
- Inspected `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`; it is still an empty `MonoBehaviour`.
- Inspected `Pakuri/Assets/Scripts2/InGame/Units/UnitRuntimeModel.cs`; it currently stores identity, stats, resources, state, and auto flags.
- Inspected `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` and `SkillBlueprintSpecs.cs`; they define Blueprint data fields but no skill execution.
- Inspected `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs`; it currently owns legacy skill ID, runtime kind, tuning, status ID, and choice arrays.
- Inspected `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs`; it still starts the current combat controller via `BeginConfiguredDay`.
- Inspected `Pakuri/Assets/Scripts/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`; it already builds current runtime catalog data from CSV rows.

### History

- 2026-05-14: User asked to stop and organize the work order as an HTML roadmap.
- 2026-05-14: User proposed reusing the existing `Assets/Scripts/Data` loading approach for the first 1-2 connected skills; Designer amended the roadmap accordingly.
- 2026-05-14: Code Builder implemented Phase1-C and the roadmap was updated to mark Phase1-C complete.

## Task: 2026-05-14 InGame Unit Skill Component Architecture HTML

### Task title

Create a InGame unit creation and skill implementation component architecture report.

### Goals

- Explain how InGame should create and manage monster/enemy units.
- Explain how skills should use `skill-class-design.md` as the data schema reference while preserving current `SkillDefinition` compatibility.
- Provide proposed file structure and component diagrams as an HTML report.

### Constraints

- Role Owner is Designer.
- Do not implement runtime C# or Unity scene changes in this task.
- Mark proposed files/classes as proposed rather than existing implementation.
- Base conclusions on inspected repository files and reports.

### Role Owner

Designer

### Status

Completed as an HTML design report.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-14-combat-v2-unit-skill-component-architecture.html` as the handoff for unit creation, skill data adapter, executor, passive trigger, and presentation responsibility boundaries.
- First Code Builder implementation should still begin with InGame contracts/model skeleton and not UI integration.

### Evidence

- Added `Pakuri/reference/Report/2026-05-14-combat-v2-unit-skill-component-architecture.html`.
- Inspected `Pakuri/reference/skill-class-design.md`; lines `11` through `20` define `SkillData`, active skill data subclasses, and `PassiveSkillData`.
- Inspected `Pakuri/reference/skill-class-design.md`; lines `177` through `178` and `306` identify Rin-D and Vega-E as hardcoded exception logic rather than common data-class cases.
- Inspected `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs`; it already exposes `SkillRuntimeKind`, `SkillEffectPrefab`, enhancement choices, and master choices.
- Inspected `Pakuri/Assets/Scripts/Data/Definition/MonsterDefinition.cs`; it exposes `ActiveSkills` and `PassiveSkills`.
- Inspected `Pakuri/Assets/Scripts/Run/Flow/RunSceneBootstrap.cs`; lines `53` through `57` show current combat input contract via `MonsterDefinition`, `RunSession`, and `GameDataCatalog`.

### History

- 2026-05-14: User asked to express the recommended unit creation management and skill implementation approach as an HTML report with file structure and component diagrams.

## Task: 2026-05-13 InGame Foundation Architecture HTML

### Task title

Create a foundation architecture report for a new InGame scene/runtime.

### Goals

- Record the user's confirmed direction for a new combat-only scene/runtime while preserving current Run UI Flow, `RunSession`, and CSV/Data loading.
- Define the foundation structure for reusable units, skill executors, learned choices, auto attack, status effects, and prefab/view separation.
- Save the design as an HTML report without runtime C# implementation.

### Constraints

- Role Owner is Designer.
- Do not implement new runtime C# or Unity scene changes in this task.
- Base the structure on inspected current code and user decisions.
- Keep existing `RunCombatUiController` integration out of scope for now.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html` as the foundation handoff before InGame implementation.
- First Code Builder slice should define only the InGame contracts/model skeleton and avoid UI integration.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html`.
- Inspected `Pakuri/Assets/Scripts/Data/Definition/MonsterDefinition.cs`, `EnemyDefinition.cs`, `SkillDefinition.cs`, `CombatStatModels.cs`, `CombatUnitRuntime.cs`, `CombatRuntimeController.cs`, `CombatSkillRuntime.cs`, `CombatMonsterSkillRuntime.cs`, and `CombatEffectFactory.cs`.
- `MonsterDefinition.cs` currently exposes monster stats, defenses, health/damage/projectile tuning, active skills, passive skills, and reward choices.
- `EnemyDefinition.cs` currently exposes enemy stats, defenses, attack type, Stage 1 skill kind, active skill values, and passive summary.
- `SkillDefinition.cs` currently exposes `SkillRuntimeKind`, `SkillEffectPrefab`, damage coefficients, cooldown, magazine, reload, status ID, and enhancement/master choices.
- `CombatRuntimeController.cs` currently keeps `EnemyRuntime`, `ProjectileRuntime`, `SkillEffectRuntime`, and `DroneRuntime` as private nested runtime classes.
- User selected model/view separation, data-based unit identity, reusable skill management, shared `MonsterUnitActor` for 1P and manifested units, auto attack toggles, deferred UI integration, and learned-choice lookup from unit state.

### History

- 2026-05-13: User proposed a rough new combat architecture with `Base_Unit`, Monster/Enemy prefabs, skill families, animation controller, and skill effect separation.
- 2026-05-13: Designer asked six structural questions after inspecting current code.
- 2026-05-13: User confirmed the major design choices; Designer created the foundation HTML report.

## Task: 2026-05-13 Roadmap Shared Target And Skill Reuse Amendment

### Task title

Amend the post-Phase-2-E roadmap with explicit coverage of the 2026-05-10 shared target / temporary effect proposal.

### Goals

- Confirm whether the 2026-05-10 shared combat target and temporary effect proposal is covered by the 2026-05-13 roadmap.
- Add missing timing details for same-type skill reuse, common target model, temporary effects, Monster / Enemy common base, and prefab-based actor authoring.
- Keep the output as an HTML report amendment without runtime C# changes.

### Constraints

- Role Owner is Designer.
- Base every conclusion on inspected reports, board records, or current code search.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Treat `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` as the amended roadmap.
- Keep same-type skill reuse scope open until Phase 6 begins.
- Keep prefab-based Monster / Enemy common authoring as a Phase 8 view/component question, not a replacement for Phase 7 target/effect model migration.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` with section `7. 2026-05-10 공통 대상 / 임시효과 제안 반영 여부`.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:258` through `:268` proposes `CombatTargetModel` with `ActiveEffects`.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:330` through `:346` proposes `ApplyTemporaryEffect(...)`, `GrantShield(...)`, and shield subsystem separation.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:385` through `:392` lists the migration checklist for selected target, `CombatUnitRuntime`, `EnemyRuntime`, modifier aggregator, action speed, movement/damage multipliers, shield, and status effects.
- Current code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` defines `EnemyRuntime` as a private nested class and `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` defines manifested units as a `MonoBehaviour`.
- Current code search confirmed enemy objects are composed through `AddComponent` calls in `CombatRuntimeEnemies.cs:354`, `:419`, and `:517`; no `enemyPrefab` / `Instantiate` path was found in the searched enemy creation code.

### History

- 2026-05-13: User asked whether the 2026-05-10 shared target / temporary effect improvement proposal was included in the newly created roadmap and asked to amend the HTML with skill reuse, common parent/prefab, and temporary-effect timing.

## Task: 2026-05-13 Combat Runtime Refactor Roadmap After Phase 2-E

### Task title

Create an evidence-based HTML roadmap from Phase 2 closure through the remaining combat runtime refactor.

### Goals

- Confirm whether Phase 2 should be closed after Phase 2-E.
- Explain the remaining Phase 3 through Phase 7 sequence with inspected-code and board evidence.
- Place the user's proposed skill reuse refactor and common combat target / Monster-Enemy base model proposal at the safest timing.
- Save the result as an HTML report.

### Constraints

- Role Owner is Designer because this is design/report work, not runtime implementation.
- Base every conclusion on inspected files, board records, existing HTML reports, or command output.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` as the current roadmap before starting Phase 3.
- If implementation starts, begin with a small Phase 3 projectile/effect/drone simulation boundary slice rather than common target model or full skill-executor reuse.

### Evidence

- Read `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/COMBAT_STATE_OWNERSHIP_MAP.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`, `boards/COMBAT/ENEMY_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Inspected `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html`, `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`, `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html`, and `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html`.
- Code search confirmed `CombatRuntimeController.cs:307` through `:310` still owns `enemies`, `projectiles`, `skillEffects`, and `drones`.
- Code search confirmed `CombatRuntimeController.cs:498` through `:503` still calls `UpdateSpawning()`, `UpdateEnemies()`, `UpdateProjectiles()`, `UpdateMonsterSkillRuntimeEffects()`, `UpdateManifestedMonsterPartyCombat()`, and `UpdateSelectedMonsterCombat()` directly.
- Code search confirmed `CombatRuntimeProjectiles.cs:14` still owns `UpdateProjectiles()` and `CombatRuntimeProjectiles.cs:516` still owns `UpdateSelectedMonsterCombat()`.
- Code search confirmed `CombatRuntimeEnemies.cs:306`, `:706`, and `:945` still own enemy spawning, enemy update, and enemy target priority.
- Code search confirmed `CombatMonsterSkillRuntime.cs:29` still exposes the full `CombatRuntimeController` reference to monster runtime adapters.
- Code search confirmed `CombatUnitRuntime.cs:8` is a `MonoBehaviour` and `CombatUnitRuntime.cs:193` still calls `Owner.TickManifestedUnitSkill(...)`.
- Added `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html`.

### History

- 2026-05-13: User asked to create a detailed evidence-based HTML roadmap starting from Phase 2 closure verification, including the timing for skill reuse and common combat model / Monster-Enemy base-class proposals.

## Task: 2026-05-13 Combat Runtime Phase 2-E Alignment Report

### Task title

Create a Phase 2-E alignment report against the 2026-05-10 CombatRuntimeController refactor proposal.

### Goals

- Compare the current Phase 2-E refactor result with `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- Check whether the current direction satisfies section 9, `권장 결론`.
- Record what work remains after Phase 2-E.
- Save the result as an HTML report.

### Constraints

- Role Owner is Designer because this is a design/status report, not runtime code implementation.
- Base every conclusion on inspected files, board records, reviewer output, or command output.
- Do not run Unity Play Mode.
- Do not edit runtime C# behavior for this report task.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html` as the current Phase 2-E alignment summary.
- Continue the refactor with Phase 3 `Projectile / Effect / Drone Simulation Split` unless a smaller remaining Phase 2 formula/field-effect slice is identified from inspected code.

### Evidence

- Read `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`; lines `543` through `550` propose state ownership, Manifested Party, Projectile / Effect / Drone, Enemy Simulation, Selected Unit Combat, then adapter narrowing order.
- Read `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`; lines `730` through `736` define section 9, `권장 결론`.
- Read `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`, `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/COMBAT_STATE_OWNERSHIP_MAP.md`, `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`, and `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md` for Phase 0 through Phase 2-E evidence.
- Inspected current code evidence in `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs`, `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs`, `CombatRuntimeManifestedPartyView.cs`, `CombatRuntimeManifestedPartySkills.cs`, `CombatRuntimeManifestedPartyDrones.cs`, `CombatRuntimeManifestedPartyVisuals.cs`, `CombatRuntimeManifestedPartyDamage.cs`, `CombatRuntimeController.cs`, `CombatRuntimeParty.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeEnemies.cs`, `CombatMonsterSkillRuntime.cs`, and `CombatUnitRuntime.cs`.
- Current `CombatRuntime*.cs` search found 20 files and 16,221 total lines under `Pakuri/Assets/Scripts/Combat`.
- `Select-String` found direct battlefield list additions centralized inside `CombatRuntimeBattlefield.cs:63`, `:68`, `:73`, and `:78`; other matches were projectile hit-set additions or `manifestedDrones`.
- `codex_loop_logs/phase2_manifested_party_reviewer_20260513.md` exists and ends with `REVIEW_RESULT: PASS`.
- Added `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html`.

### History

- 2026-05-13: User requested an HTML report checking whether Phase 2-E refactoring follows the 2026-05-10 proposal direction, whether section 9 is satisfied, and what work remains.
- 2026-05-13: Designer created the Phase 2-E alignment report without changing runtime C# behavior.

## Task: 2026-05-13 Combat Refactor Start Plan HTML

### Task title

Create a refactoring start plan from the two 2026-05-10 combat reports.

### Goals

- Read the existing shared combat target / temporary effect design report.
- Read the existing CombatRuntimeController AI-token refactor proposal report.
- Inspect current combat runtime code to confirm whether the reported problems still exist.
- Produce a new HTML design report that identifies what problem to solve first and what order to use for the broader refactor.

### Constraints

- Role Owner is Designer because the user requested refactoring structure design and an HTML report.
- Base all conclusions on inspected files and command output.
- Do not run Unity Play Mode.
- No code implementation is included in this design report.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If implementation starts, begin with a Code Builder task for a small `CombatBattlefield` / battlefield facade extraction before introducing full `CombatTargetModel` state ownership.

### Evidence

- Read `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html`.
- Read `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- Inspected `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs`, `CombatRuntimeParty.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeEnemies.cs`, `CombatUnitRuntime.cs`, and `CombatSkillRuntime.cs`.
- Current partial `CombatRuntimeController` files total 14 files, 14,022 lines, and 668,782 characters by command output.
- Added `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html`.
- 2026-05-13 follow-up verification: user-provided `C:\Users\t3312\Downloads\2026-05-10-shared-combat-target-and-temporary-effect-design.html` did not exist by `Test-Path`, so the same local report under `Pakuri/reference/Report/` was used as inspected evidence.
- 2026-05-13 follow-up verification: updated `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html` with a goal-by-goal verification matrix covering God Class, skill reuse, common target model, temporary effects, Monster/Enemy objectification, and common base-class inheritance.
- 2026-05-13 planning follow-up: added `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md` as the phase-order board for the `CombatRuntimeController` structure split described by `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- 2026-05-13 Phase 0 follow-up: added `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/COMBAT_STATE_OWNERSHIP_MAP.md` as the concrete state ownership map required before the first code extraction slice.

### History

- 2026-05-13: User asked to recognize the current structural problem from the two 2026-05-10 reports and create an HTML plan for which refactor work should start first.
- 2026-05-13: User asked whether following the new HTML would actually satisfy the two proposals' goals such as skill reuse, common Monster/Enemy objectification, inheritance, and God Class removal; Designer verified and amended the report with explicit coverage and gaps.
- 2026-05-13: User asked to record the `CombatRuntimeController` structure split implementation order in `boards/ARCHIVE/REFACTORING_ARCHIVE_2026-05-14/REFACTORING.md`.
- 2026-05-13: User asked to start from Phase 0, `State Ownership Map`; Designer created the ownership map as a refactoring board artifact.

## Task: 2026-05-12 Boards Korean Translation Export

### Task title

Translate board Markdown files into category-level Korean Markdown reports.

### Goals

- Translate all Markdown files under `boards/` into category-level Markdown outputs.
- Save the generated outputs under `Report/`.
- Preserve source file boundaries so each translated category report can be traced back to the original board file.

### Constraints

- Role Owner is Designer -> Code Builder because the user request was documentation generation and file output.
- Evidence must come from actual `boards/**/*.md` file discovery and generated file checks.
- Code identifiers, file paths, command names, evidence strings, and already-corrupted legacy encoding text are preserved as much as possible for evidence integrity.
- No Unity Play Mode or gameplay verification is involved.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Use `Report/boards_korean_translation_index.md` as the entry point for the generated category translation files.
- If a later task requires polished human translation for a specific category, start from the corresponding file under `Report/boards_korean_translation/`.

### Evidence

- `Get-ChildItem -Path boards -Recurse -File -Filter *.md` found 26 source Markdown files across 8 categories: `ARCHIVE`, `COMBAT`, `DATA`, `MON`, `OPS`, `REPORT`, `RUN`, and `UI`.
- Generated `Report/boards_korean_translation_index.md`.
- Generated `Report/boards_korean_translation/ARCHIVE.md`, `COMBAT.md`, `DATA.md`, `MON.md`, `OPS.md`, `REPORT.md`, `RUN.md`, and `UI.md`.
- `Select-String -Path Report\boards_korean_translation\*.md -Pattern '^## 원본 파일:' | Measure-Object` returned `Count = 26`, matching the discovered source Markdown file count.
- UTF-8 verification read `Report/boards_korean_translation_index.md` and returned Korean character code points such as `52852`, `53580`, `44256`, and `47532`, confirming the file contents are stored as Korean Unicode even though the PowerShell console rendering displayed mojibake.

### History

- 2026-05-12: User requested translating all category Markdown files under `C:\TowerDefence_Pakuri\Test\boards` and saving category-level Markdown outputs under `C:\TowerDefence_Pakuri\Test\Report`.
