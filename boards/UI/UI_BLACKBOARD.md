# UI_BLACKBOARD

## Current State

The field-unit Registry ownership migration is implemented. UI consumers now query `UnitSpawnManager` instead of `InGameCombatManager.UnitRegistry`.

The previous UI and RunScene UI boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/UI/`.

For new UI work, inspect the exact current scripts, scenes, prefabs, UXML, USS, or assets first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-07-29 Field Unit Query Migration

### Task title

Move UI field-unit reads to `UnitSpawnManager`.

### Goals

- Keep UI read-only with respect to field-unit registration.
- Query selected and manifested player models from the shared `UnitSpawnManager` roster.
- Remove the `SpawnedPlayerModel` fallback source.

### Constraints

- Preserve current party panel, damage meter, debug UI, Offering refresh, and auto-skill UI behavior.
- Do not change UGUI objects, scene hierarchy, prefabs, player-facing text, navigation, or input.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies party portraits, selected-monster damage-meter identity, learned-skill refresh, debug refresh, and auto-skill toggle in Play Mode.

### Evidence

- `MonsterPanelUI` reads `unitSpawnManager.Players` and no longer keeps a CombatManager Registry path or selected-player fallback.
- `DamageMeterUIController` resolves slot zero through `FindPlayerMonsterBySlot(0)`.
- `InGameUIManager`, `DebugUI`, and `InGameUtilityPanelController` receive `UnitSpawnManager` query access instead of `CombatUnitRegistry`.
- Active C# search finds zero `SpawnedPlayerModel` and `InGameCombatManager.UnitRegistry` references.
- Runtime and Editor project builds completed with zero errors.
- Unity Console contained zero errors after script refresh.

### History

- 2026-07-29: User approved one-owner field-unit management and read-only access for all other systems.
- 2026-07-29: Code Builder migrated affected UI consumers without changing UI assets or player-facing behavior.

## Task: 2026-07-29 Learned Skill UI Copy Removal

### Task title

Read learned skills from the shared `UnitSkills` source and rebuild only execution state.

### Goals

- Remove Offering and Debug UI copies from `RunMonsterState` lists into runtime models.
- Query learned active/passive state through `UnitSkills`.
- Preserve current button state, Offering completion, runtime rebuild, and display refresh behavior.

### Constraints

- Do not change UGUI objects, scenes, prefabs, labels, navigation, or player input.
- Keep reward commits routed through `RunSession.RecordOfferingChoice`.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies Offering buttons, Debug skill labels, modifier panels, learned-skill display, and the next combat's skill list in Play Mode.

### Evidence

- `InGameUIManager` and `DebugUI` no longer define or call `SyncModelStateFromSession`.
- Debug learned-state checks call `state.Skills.HasActiveSkill` and `HasPassiveSkill`.
- Offering and Debug refresh paths retain `SkillExecution.RebuildLearnedSkillState` and display refresh calls.
- Removed copy-symbol search returned zero active production references.
- Runtime and Editor builds completed with zero errors; Unity EditMode tests passed 5/5.
- Unity script compilation returned ready and the post-compile Console contained zero errors or warnings.

### History

- 2026-07-29: User approved `UnitSkills` as the single learned-skill source with post-combat full execution-state rebuilds.
- 2026-07-29: Code Builder removed UI copy helpers and converted UI reads to the shared source.

## Task: 2026-08-03 Offering Skill Popup Text

### Task title

Show distinct popup text for new active, new passive, and master skill offerings.

### Goals

- Keep all Choice1~3 presentation changes in the shared `BindChoiceButton` path.
- Keep A~E new active skills as `신규 획득!`.
- Show `패시브 스킬` for F~J passive skill acquisition.
- Show `마스터 스킬` for master skill acquisition.

### Constraints

- Do not add a production script.
- Do not change `NewRunScene` hierarchy, CSV data, `RunSession`, or `UnitSkills` ownership.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally compiled.

### Next Actions

- User verifies Choice1~3 popup activation and text in `NewRunScene` Play Mode.

### Evidence

- `UI/InGame/Reward/OfferingUI.cs` now separates `NewActiveSkill` and `NewPassiveSkill` and assigns them in `AddActiveSkillChoices` and `AddPassiveSkillChoices`.
- `OfferingUI.BindChoiceButton` updates `PopUP`, `NewSkillPopUText`, and `SkillName` color through one shared path.
- `NewRunScene.unity` contains three `PopUP` objects and three `NewSkillPopUText` objects.
- Runtime and Editor project builds completed with zero errors and the existing two assembly-reference warnings.

### History

- 2026-08-03: Code Builder added runtime popup text switching without modifying scene serialization.

## Task: 2026-08-03 CSV Monster and Enemy Image UI Wiring

### Task title

Use catalog-backed monster Standing and enemy Images in PrisonPanel and the manifested success popup.

### Goals

- Bind `MonsterDefinition.Image` to `PrisonPanel/1~5P/Image`.
- Bind `MonsterDefinition.Image` to `MenifestedSuccessPopUp/MonsterImage`.
- Bind `EnemyDefinition.Image` to `PrisonPanel/Prisonal/Image`.
- Remove name-specific serialized monster portrait routing.

### Constraints

- Keep `MonsterIconImage` for icon consumers such as MonsterPanel and DamageMeter.
- Preserve the existing UGUI hierarchy and shared catalog ownership.
- Play Mode visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compiled. Scene validation reports 0 issues, 0 missing scripts and 0 broken prefabs.

### Next Actions

- User verifies all five Standing portraits, manifested success image and selected enemy prisoner image in Play Mode.

### Evidence

- `InGameUIManager.RefreshPrisonPartySlot` now uses `monster.Image`.
- Manifested success now uses `monster.Image` instead of `MonsterIconImage`.
- `RefreshSelectedPrisoner` now resolves `EnemyDefinition.Image` and assigns the prisoner Image.
- `arielPrisonPortrait` through `vegaPrisonPortrait` and `ResolveMonsterPortrait` have zero active references.

### History

- 2026-08-03: Code Builder replaced direct scene/name-based portrait references with CSV runtime catalog Image fields.

## Task: 2026-08-03 Auto Skill Button Visual State

### Task title

Keep AutoBtn's visual ON/OFF state synchronized with the working auto-skill toggle.

### Goals

- Preserve the existing `PlayerCombatInputController` auto-skill behavior.
- Make the second AutoBtn click visibly switch back to OFF.
- Keep the ON visual after clicking the time-scale button.

### Constraints

- Do not change the time-scale logic or combat skill execution.
- Preserve the existing `Button` scene component and UI hierarchy.
- Play Mode input verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compiled.

### Next Actions

- User verifies AutoBtn ON → time button → AutoBtn OFF in `NewRunScene` Play Mode.

### Evidence

- `InGameUtilityPanelController` caches the authored Button colors and applies the selected color to all interactive states while `AutoSkillEnabled` is true; it restores the normal color when false.
- The visual refresh runs on initialization, enable and immediately after `ToggleAutoSkillMode`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.
- Unity script refresh completed with no project compile errors reported; `NewRunScene` validation reported 0 issues, 0 missing scripts and 0 broken prefabs.

### History

- 2026-08-03: Code Builder fixed the visual-only toggle desynchronization without changing the working auto-skill state logic or time-scale behavior.

## Task: 2026-08-03 InGame UI Responsibility Split

### Task title

Split `InGameUIManager` into flow control and unit UI modules.

### Goals

- Keep `InGameUIManager` responsible for lifecycle, Stage reward detection, module wiring, and flow transitions only.
- Move reward button state, PrisonPanel slots, Offering choices, manifested popups, and information text updates into focused scripts.
- Organize the scripts under `UI/InGame/Info` and `UI/InGame/Reward` without changing the scene hierarchy.

### Constraints

- Preserve `RunSession` ownership and existing reward/Offering/manifest flow.
- Preserve the existing `OfferingPanel/Choice1~3` paths and popup text/color behavior.
- Do not add interfaces, factories, new MonoBehaviours, or speculative scene components.
- Unity Play Mode visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies reward opening, prisoner selection, Offering, manifest success/failure, and next-day flow in `NewRunScene` Play Mode.

### Evidence

- `InGameUIManager.cs` is reduced to 216 lines and contains Stage flow, module construction, callbacks, and Inspector reference wiring; it no longer contains `OfferingUI`, `MenifestUI`, `RewardButtonView`, or `PrisonPartySlotView` implementations.
- `UI/InGame/Reward/RewardPanelUI.cs` owns reward button creation, material claim, prisoner selection, and next-day button binding.
- `UI/InGame/Reward/PrisonPanelUI.cs` owns `PrisonPanel/1~5P` slot refresh, selected prisoner display, and Offering/manifest entry.
- `UI/InGame/Reward/OfferingUI.cs` owns Choice1~3 construction, popup activation/text, SkillName colors, skill commit, and runtime skill refresh.
- `UI/InGame/Reward/MenifestUI.cs` owns manifested success/failure popups and party commit.
- `UI/InGame/Info/InGameInfoUI.cs` owns top and PrisonPanel resource/stage text refresh; the serialized reference groups are defined in `UI/InGame/InGameUIReferences.cs`.
- All eight new/updated scripts have Unity `.meta` files; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.
- Unity Console returned 0 error/warning entries after refresh; loaded `Assets/Scenes/NewScene/NewRunScene.unity` validation returned 0 issues, 0 missing scripts, and 0 broken prefabs.

### History

- 2026-08-03: Code Builder extracted the InGame UI units and grouped reward-related scripts under `UI/InGame/Reward` while preserving the existing scene hierarchy and runtime ownership.

## Task: 2026-08-03 InGame UI Inspector Reference Wiring

### Task title

Replace InGame UI name/path lookup with Inspector-assigned serializable reference groups.

### Goals

- Keep `InGameUIManager` responsible for lifecycle, module construction, and flow control.
- Assign Stage/Combat/Spawn managers and all Reward, Prison, Offering, Menifest, and Info UI references in `NewRunScene` through `[Serializable]` groups.
- Remove the obsolete shared scene-path resolver.

### Constraints

- Preserve the existing `NewRunScene` hierarchy, UI behavior, popup text/color rules, and RunSession ownership.
- Keep fixed Prison slots and Offering choices directly visible as `partySlot1~5` and `choice1~3` Inspector fields.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and scene-validated.

### Next Actions

- User verifies reward, Prison, Offering, Menifest, and next-day flows in `NewRunScene` Play Mode.

### Evidence

- `UI/InGame/InGameUIReferences.cs` defines `[Serializable]` reference groups for every module and fixed UI slot/choice.
- `InGameUIManager` uses `CreateUiModules()` with Inspector references; no `InGameUiSceneResolver.cs` or InGameUI path/name lookup reference remains.
- `NewRunScene` Canvas inspection reports non-null StageManager, UnitSpawnManager, CombatManager, and all assigned module references; `titleLabel` remains null because Choice1~3 have no direct title TMP child in the inspected hierarchy.
- `NewRunScene` validation reported 0 issues, 0 missing scripts, and 0 broken prefabs.
- Unity Console contained 0 error/warning entries after refresh; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder replaced runtime scene lookup with serialized Inspector references, saved the NewRunScene assignments, and deleted `InGameUiSceneResolver.cs` plus its `.meta` file.
