# RUN_BLACKBOARD

## Current State

There is no active Run, reward, or save/load task block after the 2026-07-28 cleanup.

The previous Run, reward, and save/load boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/RUN/`.

For new Run work, inspect the exact current code and data first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-08-05 Artifact and Synergy Stage Runtime Design

### Task title

Design per-monster artifact ownership, derived synergy state and Stage-start effect composition.

### Goals

- Persist at most three artifact IDs per `RunMonsterState`.
- Rebuild per-unit artifact effects and count-only synergy state once per Stage.
- Route all combat outcomes through the existing skill trigger/execution pipeline.
- Defer Spirit King spawn and all synergy-effect execution until after the artifact-first runtime is verified.
- Classify every individual artifact effect as passive modifier/trigger application.
- Make two Effect CSVs and Spirit King unit/skill authoring Phase 1, then limit first runtime implementation to the ten Spirit Contract artifacts.

### Constraints

- Designer task: no runtime implementation.
- Do not mutate persistent unit stats/defenses cumulatively between Days.
- Do not represent artifact effects as learned skills, hidden passives or enhancement/master Choices.
- `ArtifactState`, `SynergyState` and `ArtifactSynergyManager` consume generated artifact effect Definitions.
- Spirit King uses `UnitSide.Player` and a non-party `UnitRole.Summon`; no `SummonSkillDefinition` or `SummonSkillExecutor`.

### Role Owner

Designer.

### Status

Phase 1 and Phase 2 data/Definition work is complete; the revised artifact-first Run/runtime integration remains unstarted.

### Next Actions

- Implement `ArtifactState`, count-only `SynergyState`, `ArtifactSynergyManager.PrepareStage` and the `StageManager` hook.
- Log current party synergy counts once per Stage without executing synergy Definitions.
- Implement and verify only the ten Spirit Contract artifact effects before the Spirit Contract 2/4/6/8 synergy and Spirit King runtime.

### Evidence

- `RunSession.RunMonsterState` currently owns learned `UnitSkills` and reward IDs but no artifact state.
- `StageManager.RunCurrentDayFlow` spawns the selected player before encounter enemies and has no artifact/synergy preparation call.
- The corrected artifact-first design adds `PrepareStage` without executing `ArtifactSynergyEffectDefinition` or representing effects in `UnitSkills`.
- `ArtifactState`, `SynergyState`, `ArtifactSynergyManager`, `UnitRole.Summon` and temporary allied-monster spawn do not exist yet.
- The first implementation scope is the ten Spirit Contract artifacts only: eight `SkillModifier` effects and two `PassiveTrigger` effects; all synergy gaps are deferred.
- The revised design identifies exact existing Node/Trigger/Executor routes and marks missing reload-complete, densest, temporary allied spawn/movement and conditional-crit paths as required extensions.
- Spirit Bombardment reuses `SingleSkillDefinition` plus `RepeatPerTarget` for three total casts; Dimensional Collapse is split into pull and follow-up explosion SingleAttack Definitions. Current `SingleSkillExecutor` publishes `OnDeploymentCast` and completes without timed `OnExpire`, so that lifecycle is an explicit minimal extension.
- `CombatUnitRegistry` groups by `Identity.Side`, and `SkillTargeting.TargetList` gives Player-side `Ally/AllAllies` skills the full `Players` list; a Player-side Spirit King therefore receives team effects cast after it spawns.
- `SkillExecution.TryExecuteAutomaticSkills` scans all registered entries, while current movement exists only in `EnemyActionController`; Spirit King can reuse automatic skills but needs a small allied movement controller.
- Full runtime design and acceptance criteria are recorded in `Pakuri/reference/4.run/artifact-synergy-runtime-design.md`.
- Phase 1 now has 27 synergy-effect rows for all 20 detailed non-Tracker levels; Spirit Contract rows reference the authored Spirit King and four granted skill Definitions without adding runtime integration.
- Phase 2 now generates and indexes all Artifact/Synergy/Summon Definitions, but `ArtifactState`, `SynergyState`, `ArtifactSynergyManager` and every artifact runtime consumer remain absent.

### History

- 2026-08-05: Designer traced Run, Stage, spawn, skill rebuild, Trigger and Executor paths and produced the Stage composition design.
- 2026-08-05: User corrected effect identity; Designer removed hidden passive composition and specified generated Artifact effect Definitions managed by ArtifactState/SynergyState/ArtifactSynergyManager, with Spirit Contract first.
- 2026-08-05: Designer made all individual artifacts passive modifier/trigger effects, documented concrete Node paths, and moved two Effect CSVs to Phase 1 before runtime work.
- 2026-08-05: User removed the Summon-skill concept; Designer changed Spirit King to a temporary Player-side monster using existing Unit/Skill paths plus a movement-only extension.
- 2026-08-05: Designer moved Spirit King unit and five skill source rows into Phase 1 and recorded the exact SingleAttack/AreaAttack execution split.
- 2026-08-05: Code Builder authored and validated the four Phase 1 CSVs; RunSession, StageManager and runtime code remain unchanged.
- 2026-08-05: User restricted the next implementation target to the ten Spirit Contract artifacts; Designer made synergy state count/log-only and deferred Spirit King and all synergy-effect execution.

## Task: 2026-08-05 Artifact Synergy Foundation CSVs

### Task title

Record the initial Run artifact synergy and artifact catalog data.

### Goals

- Represent the Run artifact synergy designs as foundation CSV data.
- Associate every currently authored artifact with its owning synergy.
- Keep the files ready for a later parser/runtime task without implementing that task now.

### Constraints

- Do not connect the catalogs to RunSession, rewards, UI or runtime behavior.
- Do not fabricate Tracker details missing from `artifact-synergy-list.md`.
- Preserve the source document's player-facing wording without Markdown backticks.

### Role Owner

Code Builder.

### Status

Complete. Run artifact foundation data exists without unused ordering metadata; no gameplay system consumes it yet.

### Next Actions

- Complete the Tracker design and artifact list.
- Define Run ownership and parsing in a separate explicit implementation task.

### Evidence

- `artifact_synergies.csv` records six synergies and their common 2/4/6/8 activation counts.
- `artifacts.csv` records the 50 artifacts present in the source's five detailed synergy sections.
- Import validation confirmed unique IDs and valid artifact-to-synergy references.
- Source inspection confirmed Tracker appears only in the six-synergy summary and has no detailed section or artifact table.
- Both catalogs omit `sort_order`; deterministic UI/runtime ordering remains deferred until a consumer defines that requirement.
- The Spirit Contract artifact row now uses `spirit-elixir`, `정령의 비약`, and the revised all-damage/resistance-down description.

### History

- 2026-08-05: Code Builder added the unparsed Run artifact catalogs and recorded the source's Tracker data gap.
- 2026-08-05: Code Builder removed unused `sort_order` fields from both unparsed catalogs and preserved the existing row order and content.
- 2026-08-05: Code Builder renamed the CSVs and preserved their Unity `.meta` GUIDs and file contents.
- 2026-08-05: Designer synchronized the requested `정령의 비약` change into source and the unparsed artifact catalog.

## Task: 2026-08-01 NewRunScene Monster Prefab Serialization Migration

### Task title

Move `NewRunScene` monster prefab references into `MonsterPrefabBinding[]`.

### Goals

- Replace the five `UnitSpawnManager` scene fields with one serialized binding array.
- Preserve the five existing prefab GUID references in `NewRunScene`.
- Keep selected-monster and manifested-party spawn call sites unchanged.

### Constraints

- Preserve scene references and runtime spawn behavior.
- Do not change RunSession or learned-skill ownership.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies selected and manifested monster spawning in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:23616` now contains five `monsterPrefabBindings` entries.
- Unity loaded `NewRunScene` successfully and scene validation reported 0 issues, 0 missing scripts, and 0 broken prefabs.
- Unity component inspection reported the five expected monster IDs and prefab asset paths.
- Core and Editor builds completed with 0 errors; only the existing two assembly-reference warnings remain.

### History

- 2026-08-01: Code Builder migrated the existing NewRunScene monster prefab references from individual fields to serialized binding entries without changing spawn callers.

## Task: 2026-08-01 Player Party Restore Consolidation

### Task title

Consolidate selected-player and additional-player session restoration into one traversal.

### Goals

- Keep one `RestorePlayerPartyFromSession` entry point for every party slot.
- Preserve registry checks and revival of existing runtime monsters.
- Preserve selected-player creation for slot 0 and manifested-monster creation for later slots.

### Constraints

- Keep the public `RestorePlayerPartyFromSession` API and existing creation methods.
- Preserve `RunSession` ownership and next-day restoration behavior.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies next-day party revival and restoration in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs:144` now loops from slot 0 through `PartyMembers` in one method.
- Repository search found zero `RestoreSelectedPlayerFromSession` and `RestoreAdditionalPlayersFromSession` references.
- Core and Editor builds completed with 0 errors; only the existing two assembly-reference warnings remain.
- Unity script validation reported 0 warnings and 0 errors; Unity Console reported 0 error/warning entries.

### History

- 2026-08-01: Code Builder merged the two private restoration traversals while retaining their slot-specific creation branches.

## Task: 2026-07-29 Unit Skill Ownership Consolidation

### Task title

Keep each run monster's learned skills in one shared `UnitSkills` instance.

### Goals

- Remove duplicate learned-active, learned-passive, and chosen-Choice collections from `RunMonsterState`.
- Keep `RunSession` responsible for Offering transactions, learning limits, party state, and reward-consumption history.
- Share the same `UnitSkills` instance with each player monster runtime model.

### Constraints

- Preserve current learning limits, default skill selection, Offering behavior, day restoration, and skill execution.
- Keep `ChosenRewardIds` in `RunMonsterState`.
- Keep full `SkillExecutionState` rebuilds because learning occurs after combat.
- Do not add or delete production scripts.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies default skill, active/passive learning, Choice application, and next-day party restoration in Play Mode.

### Evidence

- `RunMonsterState` now contains `MonsterId`, one `UnitSkills Skills`, and `ChosenRewardIds`.
- Production skill mutations now occur only in `RunSession`.
- Active C# search returns zero `LearnedActives`, `LearnedPassives`, `ChosenChoiceIds`, `ApplyLearnedSkills`, and `SyncModelStateFromSession` references.
- Runtime and Editor project builds completed with zero errors and the two existing assembly-reference warnings.
- `SkillCatalogRuntimeTests` passed 5/5; `MonsterRuntimeSharesRunSessionSkills` proves the run state and runtime model share one instance.
- Unity script compilation returned ready and the post-compile Console contained zero errors or warnings.

### History

- 2026-07-29: Designer and user agreed that `UnitSkills` owns learned skill and Choice state while `RunSession` owns run rules and reward transactions.
- 2026-07-29: Code Builder removed duplicate run collections and converted spawn, restoration, Offering UI, and debug paths to the shared instance.

## Task: 2026-08-03 Offering Skill Popup Text

### Task title

Update NewRunScene Offering popup text by learned-skill category.

### Goals

- Preserve `RunSession` Offering selection and learned-skill ownership.
- Display `신규 획득!` for new A~E active skills.
- Display `패시브 스킬` for new F~J passive skills.
- Display `마스터 스킬` for master choices.

### Constraints

- Keep the existing `OpenOfferingPanel → BuildOfferingChoices → BindChoiceButton` flow.
- Do not modify `RunSession`, `UnitSkills`, CSV data, or scene hierarchy.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally compiled.

### Next Actions

- User verifies the popup text after opening each Offering choice category in Play Mode.

### Evidence

- `InGameUIManager` assigns category-specific `OfferingKind` values before shared button binding.
- `RunSession` remains the existing source for learnability and master-choice eligibility.
- Runtime and Editor project builds completed with zero errors and the existing two assembly-reference warnings.

### History

- 2026-08-03: Code Builder added category-specific `NewSkillPopUText` updates through the shared Offering UI path.

## Task: 2026-08-03 NewRunScene CSV Image Binding

### Task title

Remove obsolete scene Sprite ownership for PrisonPanel and use runtime catalog Images.

### Goals

- Remove five serialized monster portrait fields from `InGameUIManager` and `NewRunScene`.
- Clear the direct Karin Sprite from `PrisonPanel/Prisonal/Image`.
- Keep the scene hierarchy and UI object paths unchanged.

### Constraints

- Do not change `RunSession`, `UnitSkills`, Offering flow or scene hierarchy.
- Keep Play Mode verification user-owned.
- Preserve unrelated existing scene changes.

### Role Owner

Code Builder

### Status

Implemented and scene-validated.

### Next Actions

- User verifies prisoner selection and monster party image refresh in `NewRunScene` Play Mode.

### Evidence

- `NewRunScene.unity` no longer serializes `arielPrisonPortrait`, `evePrisonPortrait`, `rinPrisonPortrait`, `seinPrisonPortrait` or `vegaPrisonPortrait`.
- The direct `Karin.png` Sprite on `PrisonPanel/Prisonal/Image` was cleared to `fileID: 0`.
- Unity scene validation reported 0 issues, 0 missing scripts and 0 broken prefabs.

### History

- 2026-08-03: Code Builder removed obsolete scene Sprite references; UI now assigns catalog-backed Images at refresh time.

## Task: 2026-08-03 Run UI Inspector Reference Wiring

### Task title

Wire NewRunScene reward, Offering, and Menifest UI through serialized Inspector references.

### Goals

- Preserve the existing Offering and manifest flow while removing runtime scene-name lookup from the extracted UI modules.
- Keep Choice1~3 popup activation/text/color behavior and Prison party slot behavior unchanged.
- Store all current NewRunScene module references on `Canvas/InGameUIManager`.

### Constraints

- Preserve RunSession ownership, scene hierarchy, and user-facing flow.
- Do not change CSV or runtime skill/manifest rules.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and scene-validated.

### Next Actions

- User verifies Offering selection, master/passive popup labels, manifest success/failure, and next-day flow in Play Mode.

### Evidence

- `OfferingUI`, `MenifestUI`, `PrisonPanelUI`, `RewardPanelUI`, and `InGameInfoUI` constructors now consume typed serializable reference groups.
- `NewRunScene` Canvas inspection reports assigned Offering choice buttons/popups/texts, Menifest controls/images, Prison slots, reward controls, and resource labels.
- Scene validation reported 0 issues, 0 missing scripts, and 0 broken prefabs; Unity Console contained 0 error/warning entries after refresh.
- Runtime and Editor project build completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder saved the NewRunScene Inspector reference graph and removed the obsolete scene resolver used by the former UI modules.

## Task: 2026-08-03 NewRunScene Spawn and UI Hierarchy Normalization

### Task title

Use direct Inspector spawn/UI references and organize `NewRunScene` without changing layer-sensitive layout.

### Goals

- Store `UnitSpawnManager` party spawn points as direct serialized scene references.
- Keep scene-owned UI modules as `MonoBehaviour` components with their own Inspector references.
- Keep `InGameUIManager` as the coordinator and remove the shared `InGameUIReferences` object.
- Group spawn, runtime, and UI objects while preserving their previous order, world transforms, and layers.

### Constraints

- Preserve the existing NewRunScene runtime flow and authored UI positions.
- Preserve Default layer 0 for Grid/Runtime objects and UI layer 5 for Canvas/UI objects.
- Do not modify unrelated Combat changes already present in the worktree.
- Play Mode behavior verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, serialized, compiled, and scene-validated.

### Next Actions

- User verifies player/enemy spawn positions, reward flow, Offering flow, manifest flow, and next-day transition in Play Mode.

### Evidence

- `GameFlow/Spawn/UnitSpawnManager.cs:20-25,448-457` uses serialized `partySpawnPoints` and resolves by party-slot index; the former `GameObject.Find` path is gone.
- `NewRunScene.unity` stores five non-zero `partySpawnPoints` fileIDs plus direct player/enemy/runtime-root references.
- Live hierarchy reports `Grid/SpawnPoint`, `Runtime/Enemies`, `Runtime/Skills`, `Runtime/Monsters`, and the six UI category roots.
- Live hierarchy reports layer 0 for Grid/Runtime categories and layer 5 for UI and its category roots; scene validation reports 0 issues, 0 missing scripts, and 0 broken prefabs.
- `InGameUIReferences.cs` and its `.meta` are absent, and no deleted setup type remains in project files or source search.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and 2 existing assembly-reference warnings; Unity editor state reports no active compilation.

### History

- 2026-08-03: Code Builder replaced spawn/UI lookup with direct Inspector references, converted the scene UI modules to MonoBehaviours, removed the shared reference script, and grouped NewRunScene objects without changing their world transforms or layer assignments.

## Task: 2026-08-03 Manifest Failure Overlay and Scene Rename

### Task title

Keep `ManifestFailPopup` over `PrisonPanel`, restore PrisonPanel on failure Back, and rename the active menu/run scenes.

### Goals

- Keep `PrisonPanel` active when manifest fails so `ManifestFailPopup` renders above it.
- Make the failure popup Back action reopen `PrisonPanel`.
- Rename the existing run scene to `InGameScene` and the existing main menu scene to `MainMenuScene`.

### Constraints

- Preserve the existing success-manifest flow and popup hierarchy.
- Update all active serialized/code/build-settings scene paths.
- The requested `NewMainScene.unity` did not exist; use the actual `NewMainMenu.unity` as the main-menu source.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, statically verified, and compiled.

### Next Actions

- User verifies manifest failure overlay, failure Back return, success flow, and MainMenu ↔ InGame scene transitions in Play Mode.

### Evidence

- `MenifestUI.IsFailurePopupVisible` exposes the failure popup state; `CompleteAfterFailure` now calls `OpenPrisonPanel()`.
- `PrisonPanelUI` hides the panel only when the manifest result is not the failure popup, leaving the existing `Popup` sibling above `Panels` in the scene hierarchy.
- `NewRunScene` was renamed to `InGameScene`; `NewMainMenu` was renamed to `MainMenuScene`, with their `.meta` GUIDs preserved.
- `MainMenuUIManager`, `StageManager`, both serialized scene fields, and `EditorBuildSettings.asset` now use `InGameScene`/`MainMenuScene` paths.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings; `git diff --check` returned no whitespace errors.

### History

- 2026-08-03: Code Builder changed failure overlay/back behavior, renamed the two existing scenes, synchronized active references, and verified the build.

## Task: 2026-08-03 Manifest Back Binding and Debug Skill Reference Fix

### Task title

Bind `ManifestFailPopup` Back from an active UI owner and restore `DebugUI` StageManager access.

### Goals

- Ensure `ManifestFailPopup` Back closes the fail popup and returns to `RewardPanel`.
- Keep the failed-manifest popup over `PrisonPanel` until Back.
- Allow `DebugPanel` skill buttons to resolve the active `RunSession`.

### Constraints

- Preserve success manifestation flow.
- Keep direct Inspector references.
- Do not bypass existing `RunSession` skill-learning rules.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, statically verified, and compiled.

### Next Actions

- User verifies failure popup Back, `RewardPanel` return, and `DebugPanel` active/passive skill acquisition in Play Mode.

### Evidence

- `MenifestUI` now belongs to the active `Popup` GameObject (`InGameScene.unity` fileID `310674459`), so `Awake()` binds `manifestedFailBackButton`.
- `CompleteAfterFailure()` disables the failure popup and calls `InGameUIManager.CompletePrisonAction()`, which hides `PrisonPanel` and shows `RewardPanel`.
- `DebugUI.stageManager` now points to StageManager fileID `1427799829` instead of `{fileID: 0}`, allowing `ResolveSession()` to return the active session.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings; `git diff --check` passed for the modified code and scene.

### History

- 2026-08-03: Code Builder moved `MenifestUI` to the active `Popup` owner, restored `RewardPanel` return on failure Back, and restored the `DebugUI` StageManager Inspector reference.

## Task: 2026-08-03 Stage End UI Inspector Wiring and Nexus Persistence

### Task title

Move Stage end-flow UI references into UI components and keep one Nexus runtime model across days.

### Goals

- Remove `StageManager.ResolveEndFlowReferences` and bind end buttons through Inspector-owned UI components.
- Remove StageManager's direct CSV loading and health preserve/restore workaround.
- Prevent repeated Nexus registration from replacing the registry's persistent Nexus model.

### Constraints

- Preserve the existing win/defeat panel hierarchy, Button objects, and MainMenu return flow.
- Use the Loading runtime catalog for Stage data.
- Preserve Nexus current health during `ContinueToNextDay` without a second health-copy path.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, serialized, and compiled. Play Mode verification remains user-owned.

### Next Actions

- User verifies win/defeat Back buttons, multi-day Nexus health persistence, Stage transition, and enemy spawning in Play Mode.

### Evidence

- `StageManager` now stores `StageEndPanelUI winPanelUI` and `defeatPanelUI`; `InGameScene.unity` assigns both components and their Inspector Button references.
- Search across `Assets/Scripts` and `InGameScene.unity` returned `NO_OLD_STAGE_END_FLOW_REFERENCES` for the old CSV fields, `StageFlowTable`, `ResolveEndFlowReferences`, and health preserve/restore symbols.
- `UnitSpawnManager.RegisterNexus` returns when an existing registered player model has `IsNexus`, so Day transition does not create a second `nexus` model.
- `InGameCombatManager.ResetCombatState` clears Nexus transient status/shield state but does not reset `Resources.CurrentHealth`; the existing model therefore keeps current health.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder added `StageEndPanelUI`, moved end-button ownership to Inspector references, consumed the Loading Stage catalog, removed the Nexus health workaround, and made Nexus registration idempotent.
