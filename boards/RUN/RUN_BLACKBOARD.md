## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUN_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older run/combat flow history remains in that snapshot and earlier archives.
- This active file now keeps only the current `NewRunScene` authority split and the surviving new-scene flow baseline.

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

## Task: 2026-05-22 Unit-Rule Combat Execution And Auto Routing

### Task title

Run NewRunScene combat behavior by unit rules instead of `StageState.Combat`.

### Goals

- Let spawned enemies move and attack as soon as their target/range/cooldown rules allow it.
- Keep player learned active skill cooldowns advancing independent of the Stage flow state's `Combat` label.
- Keep selected 1P Auto off at start and when toggled off.
- Allow automatic player skill routing only when a living enemy exists inside `MainCamera` view.
- Pass the clicked world point into manual skill execution so click-targeted area and SingleAttack skills use the clicked location.
- Start monster skill cooldowns when a valid cast is committed even if the hit check finds no target.

### Constraints

- Role Owner is Code Builder.
- This task changes runtime execution policy only; no CSV, scene, reward, or run-session persistence fields were changed.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that enemies begin moving immediately after spawning, selected 1P Auto toggles on/off, Auto skills only fire with a visible MainCamera enemy, and clicked manual skills consume cooldown on valid casts.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs:172-179` remains the state authority for `Spawning`, `Combat`, and `RewardReady`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` no longer references `StageState.Combat`, `IsCombatStageActive()`, or a serialized `StageManager` gate for runtime behavior.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now ticks learned passives, player skill execution/manual input, and `enemyCombatSystem.Tick(...)` without checking the Stage flow state.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` still rejects automatic player skill routing without a visible living enemy in `MainCamera` or while the selected 1P Auto state is off.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionContext.cs:13-35` and `SkillExecutionSystem.cs:49-62` now carry a manual target point for clicked-position skills.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now treats committed SingleAttack, direct Beam, manual fallback Projectile, and valid Buff target attempts as routed for cooldown purposes even when a hit/status application does not occur.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP script refresh reached idle; console warning/error read showed only MCP client handler logs.

### History

- 2026-05-22: User reported DebugUI-learned Ariel skills were persisted into later rounds and could repeatedly execute outside intended combat conditions. Code Builder added the combat-state and visible-enemy gates plus clicked-position manual execution.
- 2026-05-22: User clarified that `Combat` should not gate enemy or monster behavior. Code Builder removed the Stage combat-state dependency from runtime combat behavior and kept Auto routing constrained by visible MainCamera enemies.

## Task: 2026-05-22 Offering Learned Passive Runtime Effects

### Task title

Refresh learned passive skill effects from runtime monster state during combat.

### Goals

- Use the existing Offering/run-session learned passive state as the authority for passive effect activation.
- Apply passive effect rows to current roster entries without adding a parallel run-state store.
- Keep one-shot passive effects from repeating every refresh tick.

### Constraints

- Role Owner is Code Builder.
- This task does not change Offering selection UI; it consumes the already-recorded `LearnedPassiveSkillIds`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that acquiring Ariel F-J through Offering changes the live combat state after runtime model refresh.
- If Offering can add passives during the same active combat and needs G's one-shot shield only at the next combat start, add a battle-start event boundary before applying one-shot passive effects.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` already copies `RunSession.RunMonsterState.LearnedPassives` into `MonsterUnitRuntimeModel.State.LearnedPassiveSkillIds`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` already records Offering passive choices before this task; this task consumes the model state rather than changing that storage.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGamePassiveEffectRuntime.cs` iterates `roster.Players`, reads each monster model's `LearnedPassiveSkillIds`, resolves `PassiveDefinition`, and executes `PassiveEffects`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` calls passive refresh from `Update()` and stores `appliedOneShotPassiveEffects` to prevent `apply_once` effects from repeating.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` marks `ariel-g-start-shield` with `apply_once=true`; other passive aura rows use short `0.5` second durations and refresh every `0.25` seconds.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User asked whether Ariel F-J passives with existing values could be implemented; Code Builder connected the already-recorded learned passive state to the shared effect runtime.

## Task: 2026-05-20 DebugModifiedUI Active Choice Commit Path

### Task title

Route debug active enhancement picks through the same run-session choice state used by Offering.

### Goals

- Let debug UI apply active `Trait` and `Master` choices without inventing a separate debug-only choice state.
- Keep runtime chosen choice IDs and numeric reward modifiers flowing through the existing `RunSession` and runtime-model refresh path.
- Keep active skill enhancement availability aligned with the current Offering limits per skill.

### Constraints

- Role Owner is Code Builder.
- Implementation scope is limited to `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs`; no `RunSession`, `InGameUIManager`, or scene serialization code was changed in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Debug active enhancement picks now record exact choice IDs into run state and immediately rebuild player runtime skills.

### Next Actions

- User verifies in Play Mode that debug-applied trait/master picks persist in the current run session and immediately affect the selected monster.
- If debug UI later needs passive enhancement support, extend the same run-state path rather than creating a second modifier storage path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` already stores enhancement ownership through `ChosenRewardIds`, `ChosenChoiceIds`, and `AccumulateReward(...)`, which remains the authority used by this task.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs:616-645` already commits Offering enhancement choices through `RecordOfferingChoice(...)`, `AccumulateReward(...)`, and runtime refresh; `DebugUI.cs` now mirrors that path for debug active modifiers.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now applies clicked modifier choices through `RunSession.RecordOfferingChoice(monster.MonsterId, choice.ChoiceId, choice.ChoiceId, sourceSkill.SkillId, string.Empty)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now applies choice numeric effects through `RunSession.AccumulateReward(...)`, then copies chosen choice IDs back into `MonsterUnitRuntimeModel.State` via `RefreshRuntimeSkillModels(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now enforces current active choice gating by `SkillChoiceGroup`: up to three `ActiveEnhancement` choices and then up to one `ActiveMaster`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings after rerun outside the transient file-lock case.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-20: User requested Code Builder implementation so `DebugModifiedUI` could apply active trait/master choices like Offering without going through prisoner reward flow.

## Task: 2026-05-20 MonsterPanel Player Slot Projection

### Task title

Project current player roster slot ownership into `MonsterPanel` `1PMonster`-`5PMonster` UI.

### Goals

- Keep `NewRunScene` player runtime ownership authoritative through `UnitIdentity.SlotIndex`.
- Make manifested player monsters appear in the same slot index on the `MonsterPanel` UI path.
- Avoid scene serialization edits when the authored `MonsterPanel` child slot objects already exist.

### Constraints

- Role Owner is Code Builder.
- The implementation scope is limited to `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs`; no spawn logic or roster ownership code was changed.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. The active run UI now maps player roster models into panel slots `0`-`4` before rendering `1PMonster`-`5PMonster`.

### Next Actions

- User verifies in Play Mode that each manifested monster shows in the `MonsterPanel` slot matching its runtime `SlotIndex`.
- If a manifested monster still fails to appear, inspect the spawn/roster path to confirm that `combatManager.Roster.Players` actually contains a player model for that slot.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs:338-352` restores manifested players into slot indices `1`-`4`, and `SceneEntryManager.cs:372-384` already resolves active player entries by `identity.SlotIndex`.
- `Pakuri/Assets/Scripts2/InGame/Units\BaseUnitRuntimeModel.cs:19-26` defines `UnitIdentity.SlotIndex`, which is the runtime authority used by this UI task.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:44-82` now projects `combatManager.Roster.Players` into a five-slot model array keyed by `identity.SlotIndex`, with slot `0` falling back to `entryManager.SpawnedPlayerModel` only when needed.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:128-166` now binds `MonsterPanel` children named `1PMonster` through `5PMonster` instead of binding only `1PMonster`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains the authored slot roots `1PMonster`, `2PMonster`, `3PMonster`, `4PMonster`, and `5PMonster`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-20: User requested Code Builder implementation after code inspection showed the prior `MonsterPanelUI` path was hard-wired to `Players[0]` and `MonsterPanel/1PMonster`.

## Task: 2026-05-20 Manifest Popup Text Fix And Spawn-Point State Check

### Task title

Fix Menifest success popup label mojibake and verify current spawn-point active state without changing Manifest spawn logic.

### Goals

- Remove the broken hardcoded labels shown in the Menifest success popup description.
- Keep current Manifest runtime flow ownership unchanged.
- Verify whether `1PSpawnPoint` through `5PSpawnPoint` were already active in the current repository state before making a scene/runtime change request.

### Constraints

- Role Owner is Code Builder.
- The user explicitly scoped this task to popup text plus spawn-point active-state handling; do not modify Manifest slot calculation or spawn code in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Popup text implemented. Current repository evidence shows the authored `NewRunScene` spawn-point objects are already active, so no run-flow code or scene-serialization change was applied for spawn placement in this task.

### Next Actions

- User verifies in Play Mode whether the popup labels render correctly and whether Manifest spawn behavior still reproduces.
- If spawn placement still lands on the wrong slot, inspect runtime scene state and spawn ownership separately from this completed text fix.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs:1349-1350` now uses readable labels for the Menifest success popup description while preserving the existing runtime-fed monster values.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` inspection showed `1PSpawnPoint`, `2PSpawnPoint`, `3PSpawnPoint`, `4PSpawnPoint`, and `5PSpawnPoint` with `m_IsActive: 1`.
- Unity MCP `editor_state` showed the editor was ready, `manage_scene load` successfully opened `Assets/Scenes/NewScene/NewRunScene.unity`, and Unity MCP scene inspection returned `3PSpawnPoint active=true activeInHierarchy=true`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings after a first parallel build attempt hit only a transient file-lock error.

### History

- 2026-05-20: User requested a limited Code Builder pass that fixed popup text and handled spawn-point active state without expanding into a broader Manifest spawn-logic rewrite.

## Task: 2026-05-20 Day Advance Heal And Reward Grid Layout

### Task title

Restore the full player party on day advance and move reward button placement to inspector-driven grid settings.

### Goals

- Restore current player-side party members for the next day, including allies that died and were removed during the previous combat.
- Heal restored player-side roster units to max HP before the next day starts.
- Replace template-inferred reward button spacing with explicit grid placement rules that match the current RewardPanel layout.
- Expose reward button X/Y placement values on `InGameUIManager` so the scene owner can tune them in the inspector.

### Constraints

- Role Owner is Code Builder.
- Runtime conclusions must stay tied to inspected `StageManager`, `InGameCombatManager`, `UnitRosterService`, and `NewRunScene` serialization.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. `ContinueToNextDay()` now restores missing party units from `RunSession` and then fills current player HP before the next day flow restarts, and reward buttons now use inspector-driven grid coordinates instead of inferred single-column spacing.

### Next Actions

- User verifies in Play Mode that stage/day transition respawns dead allies into their original party slots and that all returned allies begin the next combat at full HP.
- User adjusts `InGameUIManager` inspector values if RewardPanel column count or spacing needs further tuning beyond the current `3 rows per column` grid.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` `RemoveUnitIfDead(...)` unregisters dead units from `roster` and destroys their actor object, which is why full next-day return required more than a health refill.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now exposes `restorePlayerHealthOnDayAdvance` and calls `RestorePlayerHealthForNextDay()` immediately after `activeSession.AdvanceDay()` and before `StartCurrentDay()`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now calls `entryManager.RestorePlayerPartyFromSession()` before filling each `combatManager.Roster.Players` entry to `model.Stats.MaxHealth` and refreshing the actor.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now restores the selected slot through `RestoreSelectedPlayerFromSession()` and manifested slots through `RestoreManifestedPlayersFromSession()`, using `RunSession.ActiveSession` plus current roster slot checks.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` now adds `RespawnSelectedPlayerUnit(RunSession activeSession, ...)`, which recreates the selected monster runtime from existing session party state instead of calling `RunSession.Begin(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `Roster` and `RefreshUnitActor(BaseUnitRuntimeModel model)`, which is the current runtime/UI refresh path used by the heal step.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` keeps selected and manifested party ownership in `SelectedMonsterId`, `ManifestedMonsterIds`, and `PartyMembers`, which is the source used for day-advance respawn.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now exposes `rewardButtonFirstColumnPosition`, `rewardButtonColumnSpacingX`, `rewardButtonRowSpacingY`, and `rewardButtonRowsPerColumn`, and `ArrangeRewardButton()` now computes `column` / `row` from `order`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes `PrisonerBtn` at `m_AnchoredPosition {x: -321.97855, y: 295}` and the same container holds `DarkBtn` at `y: 122` and `GoldBtn` at `y: -53`, which matches the default `122` row spacing carried into the new inspector-backed layout.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-20: User requested Code Builder implementation for day-advance HP restoration and RewardPanel button placement, with X/Y layout values adjustable in the inspector.

## Task: 2026-05-19 Selected 1P A-Skill Entry Policy And Sein Visual Wiring

### Task title

Keep the selected 1P primary-skill entry policy explicit while restoring the missing Sein projectile visual mapping.

### Goals

- Record that the selected 1P slot `A` does not auto-route on scene entry unless `playerAutoSkillEnabled` is enabled.
- Restore the missing `sein-a` visual prefab mapping in `NewRunScene` so manual fire or `AutoBtn` uses the expected projectile visual.

### Constraints

- Role Owner is Code Builder.
- Runtime conclusions must stay tied to the inspected Scripts2 combat manager and actual `NewRunScene` serialization.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Scene visual wiring was restored. The run-flow policy remains unchanged: selected 1P slot `A` starts in manual fire mode until `AutoBtn` enables auto fire.

### Next Actions

- If the user wants selected 1P `A` to auto-fire immediately on scene entry, treat that as a separate global run/combat policy change and update this board together with the relevant combat/UI boards.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` uses `ShouldAutoRouteSkill(...)` to suppress automatic routing for the selected player slot `A` unless `playerAutoSkillEnabled` is true.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` routes selected player slot `A` through `HandleSelectedPlayerPrimarySkillInput()` only while the primary mouse button is held and the pointer is not over UI.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10373` serializes `playerAutoSkillEnabled: 0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs` and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:14188-14189` show `AutoBtn` enables `InGameCombatManager.EnablePlayerAutoSkillMode()`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10471` now serializes the `sein-a` prefab entry under the `EffectManager` `sein` group.

### History

- 2026-05-19: User reported that Sein appeared not to attack in-game and also noted the missing `EffectManager` Sein prefab assignment.
- 2026-05-19: Code Builder confirmed the selected 1P `A` manual-fire entry policy, restored the missing `sein-a` visual prefab mapping, and left the global auto-fire policy unchanged.

## Task: 2026-05-19 Offering Choice Runtime Path Unification

### Task title

Use exact choice IDs for Offering gating and resolve runtime modifiers from unified choice definitions.

### Goals

- Keep Offering enhancement picks keyed by exact `choice_id`.
- Remove the old separate `SkillChoiceModifierData.csv` combat path.
- Let passive-linked choice rows target active skills through merged choice metadata.

### Constraints

- Role Owner is Code Builder.
- Runtime conclusions must stay tied to inspected Scripts2 code and verified builds.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Offering and combat runtime now resolve monster choice effects from one unified `SkillChoiceDefinition` path.

### Next Actions

- If later combat work adds new special-case choice behaviors, extend `SkillExecutionSnapshot` / executors from the rows already marked unsupported or partial in `monster_skill_choices.csv`.
- Keep this file aligned with `boards/DATA/DATA_BLACKBOARD.md` whenever choice/runtime ownership changes again.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now commits enhancement Offering picks with `choice.ChoiceId`, which is the exact row ID from `monster_modifier_skill_choice.csv` / `monster_skill_choices.csv`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` still persists `ChosenRewardIds` and `ChosenChoiceIds` separately, so the gate row ID and runtime choice ID remain explicit even though enhancement picks now use the same exact `choice_id`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs` no longer uses `SkillChoiceModifierLibrary`; it resolves each chosen choice globally through `PakuriDataManager.TryGetData(choiceId, out SkillChoiceDefinition choice)` and applies it when `choice.TargetSkillId` or `choice.SkillId` matches the executing skill.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs` now applies merged choice fields such as `CooldownMultiplier`, `RadiusMultiplier`, `DurationMultiplier`, `AdditionalProjectileBonus`, `PierceBonus`, branch fields, and status stack fields directly from `SkillChoiceDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now consumes snapshot radius/duration modifiers for beam skills, which is why rechecked Eve beam rows could be upgraded from unsupported to direct support.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` no longer exposes `skillChoiceModifierCsv`, and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` was deleted in this task.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing MSB3277 warnings remained.

### History

- 2026-05-19: Code Builder removed the separate choice-modifier CSV runtime path, switched combat resolution to unified `SkillChoiceDefinition` data, and kept exact `choice_id` persistence through Offering commit and runtime execution.

## Task: 2026-05-19 Remove Unused InGame Test Data Bootstrap

### Task title

Remove the unused `InGameTestDataManager` test bootstrap from the active `InGame` runtime.

### Goals

- Delete the unused test-only `InGameTestDataManager.cs` script and its `.meta`.
- Remove the explicit `Assembly-CSharp.csproj` compile entry for the deleted script.
- Keep the active `NewRunScene` runtime authority summary aligned with the actual surviving files.

### Constraints

- Role Owner is Code Builder.
- The deletion must stay evidence-based: only remove the script after confirming there is no active scene/prefab/asset reference in the inspected repository.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally verified by file inspection and build.

### Next Actions

- User verifies in Unity only if they want editor-side confirmation that no scene object was intentionally meant to use this removed test bootstrap.

### Evidence

- `Get-ChildItem Pakuri/Assets -Recurse -Include *.unity,*.prefab,*.asset | Select-String -Pattern 'b80e67b6202c23b46bf0867afa0f8b4e|InGameTestDataManager'` returned no active asset reference to the script class or GUID before deletion.
- `Pakuri/Assembly-CSharp.csproj` explicitly included `Assets\Scripts2\InGame\Core\InGameTestDataManager.cs` before this task, so the compile item had to be removed together with the file.
- Deleted `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs` and `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs.meta`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs`, `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs`, and `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` remain the inspected active runtime entry/spawn/UI owners for the surviving `NewRunScene` flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-19: User asked to delete `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs` after reviewing its role as an unused test bootstrap rather than an active runtime manager.

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

## Task: 2026-05-22 Runtime Skill Execution Cleanup

### Task title

Align NewRun skill execution with Self targeting, prefab scale, and beam width bonuses.

### Goals

- Fix Self multi-effect runtime resolution.
- Make prefab hitbox scale use resolved radius divided by base radius.
- Make beam width bonuses affect line hit width and prefab visual Y scale through resolved beam width.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- This task does not change enemy spawn/combat state flow.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies SingleAttack prefab radius upgrades and Eve-B beam width upgrades in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillAreaUtility.cs` resolves base radius, modified radius, and prefab scale factor.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now calls `SkillAreaUtility.ResolvePrefabScaleFactor(...)` for SingleAttack prefab hitbox scaling.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` resolves beam width with `1f + snapshot.BeamWidthBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` already scales sprite Y from resolved `width / sprite.bounds.size.y`, so `beam_width_bonus=0.3` increases the visual width and hit width by 30%.
- Runtime/editor builds passed with 0 errors; existing MSB3277 warnings remain.
- Unity-MCP forced refresh produced no remaining missing-type compiler errors after importing the new utility scripts.

### History

- 2026-05-22: User approved the previously discussed order: Self target fix, prefab scale cleanup, utility extraction, and beam width bonus support.
