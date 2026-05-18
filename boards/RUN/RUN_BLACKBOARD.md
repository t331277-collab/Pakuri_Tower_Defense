## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUN_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older run/combat flow history remains in that snapshot and earlier archives.
- This active file now keeps only the current `NewRunScene` authority split and the surviving new-scene flow baseline.

## Task: 2026-05-18 Remove NewRun Prefix From Runtime Script Names

### Task title

Remove the `NewRun` prefix from current run-flow script filenames and matching type names.

### Goals

- Rename `NewRunSceneEntryManager.cs`, `NewRunStageManager.cs`, and `NewRunStartContext.cs` by removing the `NewRun` prefix.
- Keep Unity component compatibility by moving each `.meta` file with its script.
- Update C# references, scene class identifiers, and project compile paths.

### Constraints

- Role Owner is Code Builder.
- Behavior must remain unchanged; this is a naming refactor only.
- Unity Play Mode verification remains user-owned.
- Existing scene name/path strings such as `NewRunScene.unity` were not renamed because the request was limited to script filenames.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want runtime scene behavior confirmation after the script rename.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` and `.meta`; GUID `b6ff00e786df7fb46ae905aa63bee059` was preserved.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStageManager.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` and `.meta`; GUID `7c2fbcf1f36342aca23eac2221b2c1e8` was preserved.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStartContext.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/StartContext.cs` and `.meta`; GUID `11eb246df33aa9b4388af02ec8175fd4` was preserved.
- `SceneEntryManager.cs`, `StageManager.cs`, and `StartContext.cs` now declare `SceneEntryManager`, `StageManager`, and `StartContext`; `NewRunStageState` was renamed to `StageState`.
- `DebugUI.cs`, `InGameUIManager.cs`, `MonsterPanelUI.cs`, and `UIManager.cs` now reference the renamed runtime types; the Menifest flow now lives inside `InGameUIManager.cs`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now records `Pakuri.InGame.SceneEntryManager` and `Pakuri.InGame.StageManager`.
- `Pakuri/Assembly-CSharp.csproj` now compiles `SceneEntryManager.cs`, `StageManager.cs`, and `StartContext.cs`.
- `Get-ChildItem -Path Pakuri\Assets -Recurse -File -Filter 'NewRun*.cs'` returned no files after the rename.
- Search found no remaining `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunStartContext`, or `NewRunStageState` references in scripts, scene assets, prefab assets, asset files, or `Assembly-CSharp.csproj`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP refresh reached idle; console warning/error read showed MCP client handler logs and the existing UnityEditor.Graphs `NullReferenceException`, not a C# compile error.

### History

- 2026-05-18: User asked to remove `NewRun` from all filenames that currently start with `NewRun`; Code Builder renamed the three inspected scripts and their matching C# types.

## Task: 2026-05-18 Enemy Spawn Manager Rename

### Task title

Keep scene entry flow wired after renaming the spawn manager to `EnemySpawnManger`.

### Goals

- Preserve `SceneEntryManager` spawning and manifest entry points after the script rename.
- Preserve the existing scene MonoBehaviour reference by retaining the script GUID.
- Keep the current new-scene runtime authority behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- Requested source file `NewRunStageSpawnManager.cs` did not exist; the inspected scene and scripts used the former `NewRunUnitSpawnManager.cs`, now `EnemySpawnManger.cs`.
- This task changed naming and references only, not spawn behavior.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode if runtime spawn behavior needs visual confirmation after the rename.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` uses `[RequireComponent(typeof(EnemySpawnManger))]`, a serialized `EnemySpawnManger unitSpawnManager`, and `GetComponent<EnemySpawnManger>()` / `AddComponent<EnemySpawnManger>()`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` preserves the previous spawn APIs, including selected player unit, manifested monster, and enemy spawn methods.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` continues to reference script GUID `fa013f8b8851bec4882efe505f98b801` and now records `Pakuri.InGame.EnemySpawnManger`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and a standalone `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP refresh reached idle; after clearing stale console output, no C# compile error remained.

### History

- 2026-05-18: Code Builder renamed the actual scene spawn manager file/class from `NewRunUnitSpawnManager` to `EnemySpawnManger` because the user-requested `NewRunStageSpawnManager.cs` was not present in the repository.

## Task: 2026-05-18 NewRunScene Current Runtime Authority

### Task title

Keep the current `NewRunScene` runtime authority split explicit and compact.

### Goals

- Preserve `EffectManager` as the current monster/enemy skill visual authority in the kept new scene flow.
- Preserve the explicit separation between chosen reward IDs and chosen runtime choice IDs.
- Preserve the current CSV runtime catalog path without the serialized `fallbackCatalog` dependency on `NewRunScene`.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed intermediate migration history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active run/runtime authority summarized and retained for future work. 2026-05-18 Code Builder refactor keeps the same runtime authority while retaining Offering and Menifest flow helpers inside `InGameUIManager.cs`. 2026-05-18 monster projectile/status runtime tuning is now skill-row based. 2026-05-18 follow-up renamed the enemy combat owner to `EnemyCombatSystem.cs` and absorbed the former cooldown helper into that file.

### Next Actions

- If runtime ownership changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/COMBAT/ENEMY_BLACKBOARD.md`.
- Use the archive snapshot when older step-by-step migration history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `Assets/Scenes/NewScene/NewRunScene.unity` own the current monster/enemy skill visual registry path.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now owns `EnemyCombatSystem`, and `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` now holds both the enemy combat loop and the former cooldown-rule helper logic used during `NewRunScene` combat ticks.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` now keeps `ChosenRewardIds` and `ChosenChoiceIds` separately.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` keeps the top-level reward/UI binding and now contains the Offering and Menifest flow helper types directly in the same file.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still passes `rewardId` plus `linkedChoiceId` separately into the session and owns active/passive/enhancement Offering choice construction through its integrated helper types.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still owns Menifest candidate, fail, success, commit, and skip popup flow while preserving the same scene-binding entry points.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` and `InGameTestDataManager.cs` no longer keep the retained `fallbackCatalog` scene dependency.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now owns per-skill projectile speed, pierce count, status chance, and status label; `monsters.csv` no longer owns those duplicate projectile/status columns.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps projectile speed, pierce, and status chance directly from `SkillDefinition`; `SkillExecutors.cs` no longer overrides Eve-A shock chance in code.

### History

- 2026-05-18: EffectManager scene authority, reward/choice separation, and removal of the serialized fallback catalog became the active run/runtime baseline.
- 2026-05-18: Code Builder split Offering and Menifest UI flows into separate helpers while keeping `InGameUIManager.cs` as the scene-binding facade.
- 2026-05-18: Code Builder later merged `OfferingUI.cs` and `MenifestUI.cs` back into `InGameUIManager.cs` during the repository-wide high-integration consolidation pass, keeping the same flow ownership in one file.
- 2026-05-18: Code Builder consolidated monster projectile/status tuning into skill rows and verified runtime/editor builds with 0 errors.
- 2026-05-18: Code Builder renamed `EnemyCombatSimulationSystem.cs` to `EnemyCombatSystem.cs` and absorbed `EnemySkillCooldown.cs` into that owner while preserving the same `NewRunScene` runtime authority path.

## Task: 2026-05-18 NewRunScene Debug Skill Acquisition Runtime Sync

### Task title

Keep debug skill acquisition and Offering skill acquisition synchronized with active runtime models.

### Goals

- Debug A-E skill buttons must add the selected 1P monster's active skill through the Offering/session acquisition path.
- The active in-scene `MonsterUnitRuntimeModel` must receive the newly learned skill state before active runtime skills are rebuilt.
- Offering active-skill acquisition must use the same runtime-state synchronization so future Offering picks become usable immediately in the current combat scene.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented. Debug acquisition records the choice in `RunSession`, syncs the 1P runtime model's `UnitStateBucket`, rebuilds learned active skills, and refreshes the actor. 2026-05-18 follow-up verified the learned runtime-skill count drives `MonsterPanel` slot activation: one default learned skill activates `Active1`, and three learned skills activate `Active1`-`Active3`. Active slot Text now displays magazine count only for magazine skills.

### Next Actions

- User verifies in Play Mode that DebugUI skill acquisition immediately appears in the selected monster runtime and can be used by normal skill execution.
- If other runtime acquisition paths are added, keep the `RunSession` state and `MonsterUnitRuntimeModel.State` synchronization explicit.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` uses `RunSession.RecordOfferingChoice(...)`, `SkillRuntimeFactory.RebuildLearnedActiveSet(...)`, and `InGameCombatManager.RefreshUnitActor(...)` for debug active-skill acquisition.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs` reads `MonsterUnitRuntimeModel.SkillRuntime.ActiveSkills`, not all source-defined monster skills, so `Active1`-`Active3` represent the learned runtime skill list.
- Unity-MCP editor code simulation after registering an Eve model with only default `eve-a` returned `runtimeSkills=1`, `Active1=True`, `Active1Text=6/6`, `Active2=False`, and `Active3=False`.
- Unity-MCP editor code simulation after adding `eve-b` and `eve-e` to the same session path returned `runtimeSkills=3`, with `Active1=True:6/6`, `Active2=True:프리즘 레이`, and `Active3=True:플라즈마 필드`.
- Unity-MCP editor code simulation after the Active Text policy change returned `runtimeSkills=3; A1=True:textActive=True:text='6/6'; A2=True:textActive=False:text=''; A3=True:textActive=False:text=''`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now routes `PrisonerChoicePopUp/OfferingBtn` through `OpenOfferingFromPrisonerChoice()` and `PrisonerChoicePopUp/Menifested` through `TryManifestFromPrisonerChoice()`, both of which set `prisonerChoicePopUp` inactive after click.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now copies `RunSession.RunMonsterState.LearnedActives`, `LearnedPassives`, and `ChosenChoiceIds` into `MonsterUnitRuntimeModel.State` before rebuilding learned active skills through its integrated Offering flow helper.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` remains the persistent source for learned active/passive skill IDs.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs` remains the runtime authority for rebuilding learned active skill instances from `MonsterUnitRuntimeModel.State`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.

### History

- 2026-05-18: Code Builder added DebugUI active-skill acquisition and patched OfferingUI runtime-state sync after inspecting that `RunSession` and `MonsterUnitRuntimeModel.State` are separate data structures.
- 2026-05-18 follow-up: Code Builder moved `MonsterPanelUI` runtime driver to always-active `Canvas`, fixed unbound serialized slot view binding, and verified learned-skill slot activation without running Play Mode.
- 2026-05-18 follow-up: Code Builder changed Active slot Text to magazine-count-only and made Offering/Menifested prisoner choice buttons close `PrisonerChoicePopUp` immediately.

## Task: 2026-05-18 Reward Prisoner Display Name Source Fix

### Task title

Use runtime enemy display names for prisoner reward UI.

### Goals

- Keep reward prisoner IDs as internal IDs while showing player-facing enemy display names in the reward UI.
- Preserve the active `NewRunScene` reward and prisoner choice flow.
- Avoid treating `stage1-swordsman` as bad CSV data when the inspected CSV row is valid.

### Constraints

- Role Owner is Code Builder.
- CSV rows were inspected but not changed.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that prisoner reward buttons still open `PrisonerChoicePopUp`, then Offering/Menifested flows consume the same `RewardButtonView.PrisonerId`.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` has `enemy_id=stage1-swordsman` and `display_name=검사`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still stores `RewardButtonView.PrisonerId` as the original prisoner ID, but displays `ResolvePrisonerDisplayName(prisonerId)` on the button label.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` resolves display names through `ResolveCatalog()` and `GameDataCatalog.GetStageOneEnemyById(...)`.
- Runtime and Editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-18: User reported that the visible `stage1-swordsman` issue came from code-side mojibake. Code Builder kept the ID as runtime state and moved the visible name to the CSV-backed display name path.

## Task: 2026-05-17 Surviving New Scene Flow Baseline

### Task title

Keep the surviving new scene flow and core Eve/status runtime handoff explicit.

### Goals

- Preserve `NewMainMenu.unity` and `NewRunScene.unity` as the surviving supported scene path.
- Preserve the current status-label refresh path and Eve-A choice-modifier execution path used by the kept new run flow.
- Keep the board clear that older Legacy controller retirement progress detail now lives in the archive snapshot.

### Constraints

- Role Owner is Code Builder.
- This retained baseline is kept because it still defines the active scene flow used by ongoing work.
- Detailed phase-by-phase migration history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Retained as the active new-scene flow baseline.

### Next Actions

- Future run work should assume only the `NewMainMenu` -> `NewRunScene` path survives.
- If scene ownership changes, update this file together with `boards/UI/UI_BLACKBOARD.md`.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity` and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` remain the surviving scene pair.
- `Pakuri/ProjectSettings/EditorBuildSettings.asset` was recorded as containing only those two kept scene paths.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `BaseUnitRuntimeModel.cs`, and `StatusEffectKind.cs` own the current status label refresh baseline used by `NewRunScene`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs`, `SkillRuntimeInstance.cs`, and `SkillExecutors.cs` own the current Eve-A chosen-choice execution path.

### History

- 2026-05-17: Legacy scene/controller cleanup, status label runtime, and Eve-A projectile modifier runtime were recorded against the surviving new-scene flow.

## Task: 2026-05-18 AreaAttack And SingleAttack Execution Runtime

### Task title

Add NewRunScene runtime executors for sustained area skills and one-shot area skills.

### Goals

- Make `ZoneSkillData` execute sustained area ticks instead of only routing.
- Add `SingleAttackData` execution for one immediate area hit.
- Keep targeting, damage, and status application on shared `InGameCombatManager` and roster paths.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that selected/learned monster skills with `AreaAttack` and `SingleAttack` route through the new executors.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutorRegistry.cs` registers `SingleAttackSkillExecutor` and `ZoneSkillExecutor`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now implements `ZoneSkillExecutor.Execute(...)` and `SingleAttackSkillExecutor.Execute(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` applies area ticks through `InGameCombatManager.ApplyDamage(...)` and `ApplyStatus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps Area duration from `active_duration_seconds` when present.
- Runtime/editor builds passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Code Builder added reusable area execution after the user requested Eve C/E AreaAttack and new SingleAttack support.

## Task: 2026-05-18 Enemy Passive Runtime CSV Sync

### Task title

Keep Stage 1 enemy passive runtime state synchronized from CSV.

### Goals

- Ensure runtime enemy creation copies CSV passive ID/value fields into `EnemyUnitRuntimeModel`.
- Keep physical-damage passive effects separate from generic outgoing damage effects.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- User verifies in Play Mode once enemy prefab assignment and runtime combat behavior are exercised.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` now copies `EnemyDefinition.PassiveSkillId` and `PassiveSkillValue` to `EnemyUnitRuntimeModel`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` now applies Stage 1 enemy passive multipliers through its integrated private helper methods instead of a separate `StageOneEnemyPassiveStatApplier.cs` file.
- Unity-MCP editor code synced CSV runtime catalogs and created stage-one enemies through `UnitFactory`, returning `sword=PhysicalDamageUp:0.1:phys=1.1:out=1;priest=HealingUp:0.15:heal=1.15:phys=1;captain=PhysicalDamageUp:0.12:phys=1.12`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Code Builder verified CSV passive ID/value data reaches created runtime enemy models through the current runtime catalog and unit factory path.
