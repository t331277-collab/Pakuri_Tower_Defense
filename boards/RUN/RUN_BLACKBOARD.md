## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUN_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older run/combat flow history remains in that snapshot and earlier archives.
- This active file now keeps only the current `NewRunScene` authority split and the surviving new-scene flow baseline.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-06-19 Stage Flow CSV Folder Move

### Task title

Move active `NewRunScene` stage-flow CSV files into `Assets/CSVdata/stage_flow`.

### Goals

- Separate stage-flow CSV files from runtime catalog CSV files.
- Preserve `NewRunScene` serialized `StageManager` TextAsset references by moving `.meta` files with the CSV files.
- Keep Stage 1 and Stage 2 day/encounter/reward data available to the existing `StageManager` flow.

### Constraints

- Role Owner is Code Builder.
- No StageManager gameplay behavior or CSV row content was changed.
- Unity Play Mode progression verification remains user-owned.

### Role Owner

Code Builder

### Status

Moved and CSV-shape checked.

### Next Actions

- Future stage-flow rows should be edited under `Pakuri/Assets/CSVdata/stage_flow/`.
- User verifies Play Mode day/stage progression as usual.

### Evidence

- `Pakuri/Assets/CSVdata/stage_flow/StageDay.csv` exists after the move with 10 columns, 22 data/type rows after the header, and no field-count mismatch.
- `Pakuri/Assets/CSVdata/stage_flow/StageEncounter.csv` exists after the move with 14 columns, 60 data/type rows after the header, and no field-count mismatch.
- `Pakuri/Assets/CSVdata/stage_flow/StageReward.csv` exists after the move with 13 columns, 9 data/type rows after the header, and no field-count mismatch.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` had serialized `stageDayCsv`, `stageEncounterCsv`, and `stageRewardCsv` TextAsset GUID references before the move; the CSV `.meta` files were moved with the CSV files to preserve those GUIDs.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors after the move.

### History

- 2026-06-19: Code Builder moved `StageDay.csv`, `StageEncounter.csv`, and `StageReward.csv` from `Assets/CSVdata/` to `Assets/CSVdata/stage_flow/` while preserving `.meta` files.

## Task: 2026-05-31 Offering Choice Labels And Active Skill Cap

### Task title

Split Offering choice card labels into monster summary and skill name, and enforce the active skill acquisition cap.

### Goals

- Display the monster name in each Offering choice card `Summary` label.
- Display the source skill plus choice title in each Offering choice card `SkillName` label, such as `심판의 빛·특성 1`.
- Stop active skill Offering candidates after the monster has learned two non-default active skills beyond its default A/default active skill.

### Constraints

- Role Owner is Code Builder.
- `InGameUIManager.cs` remains the `NewRunScene` Offering UI owner.
- No CSV schema or runtime catalog ownership change was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that `Choice1` through `Choice3` show monster names in `Summary`, names like `심판의 빛·특성 1` in `SkillName`, and stop offering extra active skills after two non-default active acquisitions.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now binds Offering card `Summary` and `SkillName` labels separately, while keeping the previous `Text (TMP)` fallback path.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now builds enhancement skill names from `TargetSkillId`, `SkillId`, or reward active/passive ids and resolves display names from active/passive definitions.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` replaced the previous `MaxRunActiveSkillCount = 5` gate with `MaxAdditionalActiveSkillCount = 2`, counting learned active skills while excluding default/A active skills.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` for `Assets/Scripts2/InGame/UI/InGameUIManager.cs` reported 0 errors and the existing `Update()` string-concatenation GC warning.

### History

- 2026-05-31: User asked Designer whether Offering labels could include the source skill and whether active skill acquisition was capped at A plus two extra active skills.
- 2026-05-31: User approved Code Builder implementation for `Summary`/`SkillName` card display and active skill cap enforcement.

## Task: 2026-05-31 Offering Learned Skill Sync For Revived Monsters

### Task title

Ensure Offering-acquired skills persist into runtime models, including monsters that died before the Offering choice and are revived on the next day.

### Goals

- Record active/passive Offering skill choices with stable choice ids.
- Refresh learned skill runtime models for scene monster actors even when they are not currently registered in the combat roster.
- Sync session learned active/passive/choice state back into existing dead monster models before `ReviveForNextDay()` re-registers them.

### Constraints

- Role Owner is Code Builder.
- Existing RunSession state remains the authority for learned skills.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies in Play Mode that Offering-acquired skills appear immediately for living monsters and remain available after dead selected/manifested monsters revive on the next day.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now writes `ChoiceId = skill.SkillId` for active skill Offering choices and `ChoiceId = passive.PassiveId` for passive skill Offering choices.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now refreshes all scene-valid `MonsterUnitActor` models from `RunSession` and rebuilds learned active runtime sets after Offering commit, not only currently registered roster players.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now syncs `LearnedActives`, `LearnedPassives`, and `ChosenChoiceIds` from `ActiveSession` into an existing monster model before `ReviveForNextDay()` and `RegisterPlayerMonster(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now filters revived player-slot lookup to `UnitRole.Monster`, keeping Nexus out of player monster revive lookup.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User reported Offering skill acquisition did not result in skills being acquired.
- 2026-05-31: Code inspection showed `RunSession.RecordOfferingChoice(...)` records learned active/passive ids, but dead monsters are absent from `combatManager.Roster.Players`; Code Builder added scene-actor refresh and revive-time session sync.

## Task: 2026-05-31 Day Advance Monster Revive And Nexus HP Persistence

### Task title

Reuse defeated monster actors on day advance and preserve Nexus HP across rounds.

### Goals

- Revive existing defeated player monster actors instead of spawning replacement prefabs when the next day starts.
- Restore revived monster HP to max and return animation to idle.
- Re-register revived monsters into the combat roster so enemies can target them again.
- Preserve Nexus current HP across day transitions.

### Constraints

- Role Owner is Code Builder.
- Nexus HP persistence no longer relies only on `NexusUnitActor.Model` surviving; `StageManager` preserves and reapplies the current Nexus HP during day advance.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. 2026-05-31 follow-up fixed Nexus HP reset during day advance and restored revived monster combat state.

### Next Actions

- User verifies in Play Mode that a dead selected or manifested monster revives on next day without a new prefab instance, attacks again, and that Nexus HP remains at the previous round value.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now tries `TryReviveExistingPlayerBySlot(...)` for selected and manifested party slots before calling prefab respawn/spawn paths.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` finds existing `MonsterUnitActor` instances by `Identity.SlotIndex`, calls `ReviveForNextDay()`, and re-registers them with `InGameCombatManager.RegisterPlayerMonster(...)`.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now exposes `ReviveForNextDay()`, restores HP to max, re-enables child `Collider2D` components, refreshes the view, and returns animation to idle.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now restores common revived monster combat state by enabling auto attacks, clearing statuses/shields, resetting active skill runtime state, and restoring AutoSkill only for non-selected monsters.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now resolves the selected player entry by `UnitRole.Monster` and `SlotIndex == 0` instead of taking `roster.Players[0]`, so Nexus cannot receive the selected monster Auto setting.
- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now exposes `ReviveToIdle()` to stop death freeze, restore animator speed, clear dead state, and play idle.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` captures Nexus current HP before `RunSession.AdvanceDay()`, excludes non-monster player units from the day-advance HP restore loop, and reapplies preserved Nexus HP after `NexusUnitActor.Initialize()`.
- `Pakuri/Assets/Scripts2/InGame/Units/NexusUnitActor.cs` now exposes `TryGetCurrentHealth(...)` and `SetCurrentHealth(...)` for StageManager HP carryover.
- Unity-MCP `validate_script` reported 0 errors for `SceneEntryManager.cs`, `MonsterUnitActor.cs`, and `Animation_Controller.cs`; only the existing `Animation_Controller.Update()` GC warning remained.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- 2026-05-31 follow-up validation: Unity-MCP `validate_script` reported 0 errors for `MonsterUnitActor.cs`, `StageManager.cs`, and `NexusUnitActor.cs`; `InGameCombatManager.cs` validator reported a duplicate `ResolveEffectManager` signature, but PowerShell search found only one declaration at line 1202 and both dotnet builds passed with 0 errors.
- 2026-05-31 follow-up validation: Unity refresh reached idle and Unity warning/error console read returned 0 entries.

### History

- 2026-05-31: User requested Code Builder work so dead monsters revive on stage/day advance instead of spawning new prefabs, while Nexus HP persists across rounds.
- 2026-05-31: Code Builder added actor revive/re-register path and confirmed Nexus initialization already preserves current HP when the model exists.
- 2026-05-31: User reported revived monsters could not attack/Auto and Nexus HP still reset to max on next stage; Code Builder added common revived monster combat-state restore, selected-monster Auto lookup by monster slot, and explicit StageManager Nexus HP carryover.

## Task: 2026-05-31 Nexus Assault Win Defeat Flow

### Task title

Add a `NewRunScene` Nexus target, defeat flow, and configurable Stage 2-11 win flow.

### Goals

- Register the `Nexus` as a player-side runtime target after the selected player monster spawns.
- Make enemies attack the Nexus only after no non-Nexus player targets remain.
- Show `Canvas/DefeatPanel` when Nexus HP reaches 0.
- Show `Canvas/WinPanel` when the configured clear stage/day is reached.
- Route both Win and Defeat buttons through the same return-to-main-menu method.

### Constraints

- Role Owner is Code Builder.
- The actual build scene is `Assets/Scenes/NewScene/NewMainMenu.unity`; no `NewMainScene` file was found in the build scene list.
- `winStageIndex` and `winDayIndex` remain inspector-editable because Stage 2-11 is prototype authority.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, scene-bound, compile-verified, and CSV-synced.

### Next Actions

- User verifies in Play Mode that enemies damage the Nexus after monsters are gone, disappear after applying Nexus damage, and both end-flow buttons return to `NewMainMenu`.
- If the prototype win condition changes, update `StageManager.winStageIndex` and `StageManager.winDayIndex` in the inspector.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now registers `NexusUnitActor`, handles `OnNexusDefeated`, hides/shows Win/Defeat panels, and routes both end buttons to `ReturnToMainMenu()`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` exposes `winStageIndex=2`, `winDayIndex=11`, and `mainMenuScenePath=Assets/Scenes/NewScene/NewMainMenu.unity`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyTargeting.cs` searches non-Nexus player targets first, then falls back to Nexus.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` moves enemies toward Nexus, applies `enemyModel.NexusDamage`, then despawns the damaging enemy.
- Unity-MCP scene inspection found `Nexus` with `Pakuri.InGame.NexusUnitActor` and trigger `BoxCollider2D`; `StageManager` serialized `nexusActor`, `winPanel`, and `defeatPanel`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User asked Code Builder to implement the Nexus attack/defeat/win flow and make enemies disappear after damaging the Nexus.
- 2026-05-31: Code Builder added a Nexus runtime actor/model, enemy Nexus assault path, StageManager end-flow handling, and scene bindings.

## Task: 2026-05-31 Stage2 NewRunScene Stage Flow Rows

### Task title

Add Stage 2 day, encounter, reward, and prefab-binding data needed by the active `NewRunScene` run flow.

### Goals

- Ensure `RunSession.AdvanceDay()` can move from Stage 1 day 11 to Stage 2 day 1 without missing `StageDay` data.
- Ensure `StageManager` can resolve Stage 2 encounter and reward rule IDs.
- Ensure `EnemySpawnManger` can resolve Stage 2 enemy prefabs when `StageEncounter.csv` emits Stage 2 enemy ids.

### Constraints

- Role Owner is Code Builder.
- Stage 2 reward values are temporary copies of Stage 1 values until a Stage 2 reward-balance source is provided.
- Unity Play Mode progression verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and non-gameplay verified; Stage 2 spawn positions normalized to Stage 1 coordinates.

### Next Actions

- User verifies reaching Stage 2 after Stage 1 boss in Play Mode.
- If Stage 2 spawn density or reward economy feels wrong, update the CSV rows rather than adding code branches.

### Evidence

- `Pakuri/Assets/CSVdata/StageDay.csv` contains 11 Stage 2 day rows from day 1 through day 11.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` contains Stage 2 normal, midboss, day-10 midboss, and boss encounter rows.
- `Pakuri/Assets/CSVdata/StageEncounter.csv` Stage 2 normal and escort rows use `spawn_x=9.02`, `spawn_y_min=-5`, `spawn_y_max=5`; Stage 2 guaranteed boss rows use `spawn_x=9.02`, `spawn_y_min=0`, `spawn_y_max=0`, matching the Stage 1 spawn-coordinate pattern.
- `Pakuri/Assets/CSVdata/StageReward.csv` contains the Stage 2 reward rule IDs referenced by `StageDay.csv`.
- PowerShell reference check returned no missing day encounter, day reward, or encounter enemy references.
- PowerShell spawn-coordinate check returned `stage2Rows=30 badNormal=0 badBoss=0`.
- Unity-MCP scene inspection showed `EnemySpawnManger.enemyPrefabBindings` populated with all 8 Stage 2 prefab references after reloading `Assets/Scenes/NewScene/NewRunScene.unity`.

### History

- 2026-05-31: User requested Stage 2 spawn-rule connection through `StageManager` after Stage 2 prefab/component setup.
- 2026-05-31: Code Builder added the data rows that the existing `StageManager` already consumes, avoiding a new StageManager code branch.
- 2026-05-31: Code Builder changed Stage 2 encounter spawn coordinates from the previous far/right Stage 2 values to the same Stage 1 coordinate pattern after the user reported abnormal Stage 2 enemy spawn positions.

## Task: 2026-05-31 DebugUI Offering-State Commit Path

### Task title

Keep DebugUI skill and enhancement acquisition synchronized with the Offering duplicate-filter state.

### Goals

- Route DebugUI active skill acquisition through the same learned-active state used by Offering.
- Route DebugUI passive skill acquisition through the same learned-passive state used by Offering.
- Route DebugUI active and passive enhancement acquisition through `RunSession.RecordOfferingChoice`.
- Preserve `ChosenChoiceIds` recording so later Offering candidates can suppress choices already taken from DebugUI.

### Constraints

- Role Owner is Code Builder.
- This task changes debug acquisition plumbing only; it does not change the Offering candidate builder or CSV reward schema.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that DebugUI-acquired active skills, passive skills, active enhancements, and passive enhancements do not reappear in Offering.
- If a specific reward row still reappears, inspect that row's `choiceId`/`rewardId` linkage before widening runtime logic.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` is the inspected Offering owner and commits choices with `RecordOfferingChoice(choice.MonsterId, choice.RewardId, choice.ChoiceId, choice.ActiveSkillId, choice.PassiveSkillId)`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` stores `ChosenRewardIds`, `ChosenChoiceIds`, `LearnedActives`, and `LearnedPassives`, and exposes `HasLearnedActive(...)` and `HasLearnedPassive(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:130` now routes active debug skill acquisition through `CommitDebugOfferingChoice(...)` instead of directly mutating only learned active state.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:165` routes passive debug skill acquisition through `CommitDebugOfferingChoice(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:673` routes active enhancement acquisition through `CommitDebugOfferingChoice(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:703` routes passive enhancement acquisition through `CommitDebugOfferingChoice(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:900` calls `RunSession.RecordOfferingChoice(...)`; `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:936` resolves exact reward id matches and otherwise records the exact choice id fallback.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after an initial parallel-build file lock.
- Unity-MCP `validate_script` on `Assets/Scripts2/InGame/UI/DebugUI.cs` reported 0 errors and one pre-existing-style validator warning about string concatenation in `Update()`.
- Unity-MCP warning/error console read after script refresh returned 0 entries.

### History

- 2026-05-31: User required DebugUI, DebugModifiedUI, and DebugPassiveModifiedUI acquisition to use the Offering acquisition path so selected items stop appearing in Offering.
- 2026-05-31: Code Builder added `DebugUI.CommitDebugOfferingChoice(...)` and routed active/passive skills plus active/passive enhancement buttons through that helper.

## Task: 2026-05-27 NewRunScene Manual Projectile Hold Ownership

### Task title

Restrict manual hold-repeat input to projectile skills and preserve one-click behavior for other active skills in `NewRunScene`.

### Goals

- Keep manual input ownership in `InGameCombatManager` instead of moving projectile burst continuation into auto-skill routing.
- Let manual projectile skills re-sample the current cursor direction while the mouse button is held.
- Preserve beam, zone, and single-attack manual casts as one-click actions that do not retarget after activation.

### Constraints

- Role Owner is Code Builder.
- This is a runtime input/control fix only; no CSV authority or scene prefab registry change was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies that manual projectile skills continue firing while the button is held and that cursor movement affects subsequent projectile shots.
- User verifies that manual non-projectile skills still cast once per click and do not change direction or target after activation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` previously gated all manual skill execution on `IsPrimaryMousePressedThisFrame()`, which prevented projectile burst follow-up shots from routing after the first click.
- The same file now distinguishes projectile runtimes from non-projectile runtimes during manual input handling, using held-button cursor sampling only for `ProjectileSkillData`.
- Manual projectile burst continuation now stays on the manual execution path by reusing latched manual aim/target data when the mouse button is no longer held but a projectile runtime is still bursting.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock on `Assembly-CSharp.dll`.

### History

- 2026-05-27: User requested that projectile skills gain manual hold-repeat behavior while beam, zone, and single-attack skills keep their current one-click activation model.

## Task: 2026-05-26 Rin-B/Rin-C NewRunScene Runtime Verification

### Task title

Verify the current `NewRunScene` runtime accepts the Rin-B/Rin-C shared-skill implementation without compile or refresh errors.

### Goals

- Confirm the shared beam/buff/status runtime changes compile on both runtime and editor assemblies.
- Confirm Unity refresh returns to idle after the Rin-B/Rin-C source changes.
- Confirm warning/error console reads do not show new C# or CSV runtime failures.

### Constraints

- Role Owner is Skill Builder.
- This task records runtime validation only; gameplay verification remains user-owned.
- Existing external assembly conflict warnings are preserved as-is.

### Role Owner

Skill Builder

### Status

Compile-verified and refresh-checked.

### Next Actions

- User verifies Rin-B/Rin-C behavior in Play Mode.
- If a later gameplay-only issue appears, start from the current compile/refresh-clean baseline instead of rechecking schema wiring first.

### Evidence

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after the Rin-B/Rin-C work; only the existing `System.Net.Http` / `System.IO.Compression` MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` first failed inside the sandbox with `Access to the path 'C:\Users\t3312\AppData\Local\Microsoft SDKs' is denied`, then passed with 0 errors when rerun unsandboxed; this was an environment permission issue, not a code error.
- Unity `refresh_unity` returned `resulting_state":"idle"` after the Rin-B/Rin-C source changes.
- Unity warning/error console reads after refresh returned only MCP-FOR-UNITY client connection/disposal logs and did not report C# compile errors or CSV runtime sync failures.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `Skills/Execution/Executors/BeamSkillExecutor.cs`, `Skills/Execution/Executors/SupportSkillExecutors.cs`, and `Skills/Execution/SkillMultiEffectExecutor.cs` are the inspected runtime owners for the new Rin-B/Rin-C execution paths validated by the builds and refresh.

### History

- 2026-05-26: Skill Builder completed Rin-B/C implementation and then verified the active `NewRunScene` runtime path through build plus Unity refresh/console checks.

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
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now passes `rewardId` plus the exact enhancement `choiceId` into the session and owns active/passive/enhancement Offering choice construction through its integrated helper types.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still owns Menifest candidate, fail, success, commit, and skip popup flow while preserving the same scene-binding entry points.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` no longer keeps the retained `fallbackCatalog` scene dependency.
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

## Task: 2026-05-29 Damage Meter Runtime Handoff

### Task title

Prepare the runtime damage-source tracking portion of the damage meter UI handoff.

### Goals

- Track player monster damage at the `InGameCombatManager.ApplyDamage` boundary.
- Use actual applied health plus shield delta for current-round totals.
- Preserve `RunSession.ManifestedMonsterIds` order for 2P to 5P display.

### Constraints

- Role Owner is Code Builder.
- Designer created the handoff only; no runtime implementation was performed.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented by Code Builder.

### Next Actions

- User verifies live Play Mode damage totals and source segmentation during combat.
- If future damage executors need more granular source names, pass `damageMeterSourceId` / `damageMeterDisplayName` through those specific executor paths.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `ApplyDamage(... BaseUnitRuntimeModel source, ... string sourceSkillId ...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` returns `InGameResourceChangeResult` with `PreviousHealth`, `CurrentHealth`, `PreviousShield`, `CurrentShield`, and `AppliedDamage`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` appends manifested monster ids in `ManifestedMonsterIds`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` uses `session.ManifestedMonsterIds.Count` to compute manifested spawn slot index.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `DamageMeterRuntimeTracker.RecordDamage(options, result)` immediately after `resourceMutations.ApplyDamage(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now resets `DamageMeterRuntimeTracker.Active` in `StartCurrentDay()` before the current day combat flow starts.
- `DamageApplicationOptions` now carries optional meter-only `DamageMeterSourceId` and `DamageMeterDisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` passes trigger ids as meter source ids for direct trigger damage where the runtime path exposes them.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` accepts optional `damageMeterSourceId` and forwards it to `ApplyDamage`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-29: User requested a Code Builder handoff for damage meter runtime tracking and source naming.
- 2026-05-29: Code Builder implemented the damage meter runtime hook and meter-only source metadata path.
