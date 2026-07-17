# RUNSCENE_UI

This is the active `NewRunScene` UI persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUNSCENE_UI_ARCHIVE_2026-05-18.md`.
- Older RunScene/Manifested UI history remains in that snapshot and earlier archive files.
- This active file now keeps only the current `NewRunScene` UI behavior still relevant to active work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-17 PrisonPanel Party And Prisoner UI

### Task title

Drive the authored PrisonPanel from current run and prisoner reward state.

### Goals

- Show `Stage N-N`, current Gold/Dark, the selected monster plus manifested monsters, and the selected prisoner.
- Show Reinforcement for occupied slots, Menifested only on the first empty slot, and disable later empty slot buttons.
- Restrict Offering candidates to the monster represented by the clicked occupied slot.

### Constraints

- Role Owner is Code Builder.
- Party order remains `SelectedMonsterId` followed by `ManifestedMonsterIds`.
- Player portraits use the five user-provided Sprite assets; prisoner portraits reuse the root SpriteRenderer of the existing enemy prefab binding.
- Unity Play Mode gameplay and visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated; user Play Mode verification pending.

### Next Actions

- User verifies every selected starting monster portrait/name, sequential slot activation, Stage 1/2 prisoner portrait/name, and per-unit Offering filtering.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` builds five slot views from the inspected UGUI hierarchy and renders them from `RunSession`.
- `OfferingUI.OpenOfferingPanel(string monsterId)` now builds candidates only for the passed monster.
- `MenifestUI` returns through one completion callback after failure-back, success-skip, or success-confirm.
- Unity-MCP verified the five serialized Sprite paths and saved `Canvas/PrisonPanel` with `activeSelf=false`.
- Both C# builds passed with 0 errors; Unity script/scene/console checks found no errors.

### History

- 2026-07-17: Code Builder implemented the user-approved PrisonPanel party, reinforcement, manifestation, and prisoner presentation flow.

## Task: 2026-07-15 DebugPanel Number-8 Toggle

### Task title

Hide and reveal the authored `Canvas/DebugPanel` with number 8.

### Goals

- Make the developer panel unavailable visually at scene start.
- Toggle the panel with either keyboard digit 8 control.
- Preserve existing DebugUI button, skill-learning, and modifier-panel bindings after their move beneath `DebugPanel`.

### Constraints

- Role Owner is Code Builder.
- `DebugUI.cs` remains the single owner; no additional scene component is added.
- The inspected hierarchy `Canvas/DebugPanel/{DebugUI, DebugModifiedUI, DebugPassiveModifiedUI, DebugUIBtn}` is authoritative.
- Unity Play Mode gameplay/input verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies the first 8 press activates the entire `DebugPanel`, the second deactivates it, and existing DebugUI controls still open and bind correctly.

### Evidence

- Unity-MCP reported `Canvas/DebugPanel` active in the authored scene and `DebugUI`, `DebugModifiedUI`, `DebugPassiveModifiedUI`, and `DebugUIBtn` as its inspected children.
- Unity-MCP reported the `Pakuri.InGame.DebugUI` component on `Canvas`, so disabling `DebugPanel` does not disable the `Update()` input listener.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` adds `debugRootPanel`, runtime initial hiding, top-row/numpad 8 toggling, and updates existing UI lookup paths to their current `DebugPanel/...` hierarchy.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and the existing 2 MSB3277 warning groups.
- Unity script validation reported 0 errors; scene validation reported 0 missing scripts and 0 broken prefabs.

### History

- 2026-07-15: User added `Canvas/DebugPanel` and requested number-8 visibility toggling through the Debug UI script.
- 2026-07-15: Code Builder implemented the toggle and aligned DebugUI child lookup paths with the inspected new hierarchy.

## Task: 2026-07-15 UtilPanel Auto And Time Controls

### Task title

Move Auto control ownership to `UtilPanel` and add 1x/1.5x/2x game-speed cycling.

### Goals

- Keep `AutoBtn` as selected-player auto-skill toggle.
- Cycle `TimeBtn` through 1x, 1.5x, 2x, then 1x.
- Show only `TimeBtn/1.5` at 1.5x, only `TimeBtn/2` at 2x, and neither at 1x.
- Apply speed to current scaled-time gameplay, including cooldowns, projectiles, skill actors, effects, and spawn/skill delays.

### Constraints

- Role Owner is Code Builder.
- Existing user-authored `NewRunScene` layout, image, and font changes are preserved.
- `Time.timeScale` is the shared speed authority; individual combat systems do not apply duplicate speed multipliers.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that `TimeBtn` cycles 1x → 1.5x → 2x → 1x and indicator visibility matches each state.
- User verifies cooldowns, projectile travel, skill effects, animations, and stage delays accelerate together.
- User verifies `AutoBtn` still toggles only selected-player auto skill mode.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUtilityPanelController.cs` replaced `InGameAutoSkillButton.cs` while preserving script GUID `b1158f7e84cec17449b6b904dd152208`.
- `InGameUtilityPanelController` binds `AutoBtn` and `TimeBtn`, applies `{1f, 1.5f, 2f}` through `Time.timeScale`, adjusts `Time.fixedDeltaTime`, and updates the two indicator GameObjects.
- Unity-MCP live scene inspection confirmed one controller on `Canvas/UtilPanel`, no controller on either button, and serialized references to `AutoBtn`, `TimeBtn`, `1.5`, `2`, and `GameManager`.
- Unity-MCP scene validation reported 0 issues, 0 missing scripts, and 0 broken prefabs.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing MCP assembly-reference MSB3277 warnings remained.

### History

- 2026-07-15: User requested Code Builder implementation of shared Auto and game-speed controls under `UtilPanel`.
- 2026-07-15: Code Builder renamed the script, centralized both button bindings on `UtilPanel`, wired the live scene, and saved `NewRunScene`.

## Task: 2026-05-31 Offering Choice Card Summary And SkillName

### Task title

Bind `NewRunScene` Offering choice cards to monster summary and source skill names.

### Goals

- Fill `Choice1` through `Choice3` `Summary` labels with the monster display name.
- Fill `Choice1` through `Choice3` `SkillName` labels with the source skill and choice title.
- Preserve the existing `Desc` label as the effect description.
- Preserve fallback behavior for older card layouts that only have `Text (TMP)`.

### Constraints

- Role Owner is Code Builder.
- This is UGUI `Canvas/OfferingPanel` behavior in `NewRunScene`.
- No scene asset edit was required because the inspected scene already contains `Summary` and `SkillName` label names.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that Offering card text appears in the intended authored labels and does not overflow or bind to the wrong TMP child.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains `Choice1`, `Choice2`, and `Choice3` related `Summary` and `SkillName` child names.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves `Summary`, `SkillName`, `Desc`, `Icon`, and fallback `Text (TMP)` for each Offering button.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now sets `Summary` to the monster display name and `SkillName` to values such as `심판의 빛·특성 1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` for `Assets/Scripts2/InGame/UI/InGameUIManager.cs` reported 0 errors and the existing `Update()` string-concatenation GC warning.

### History

- 2026-05-31: User requested Offering `Choice1` through `Choice3` UI labels to place the monster name in `Summary` and source skill plus trait title in `SkillName`.
- 2026-05-31: Code Builder implemented the UGUI label binding in `InGameUIManager.cs`.

## Task: 2026-05-31 Offering Skill Choice Commit Refresh

### Task title

Keep Offering active/passive skill choices visible to runtime UI/model state immediately after commit, including dead scene actors.

### Goals

- Give active/passive Offering choices non-empty `ChoiceId` values.
- Refresh registered roster monster models and dead/unregistered scene monster actor models after Offering commit.
- Rebuild learned active skill runtime sets after the session state is copied into each model.

### Constraints

- Role Owner is Code Builder.
- `InGameUIManager.cs` remains the active NewRunScene Offering owner.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies in Play Mode that Offering skill choices update the monster's available skills and that dead monsters keep those choices after next-day revive.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` commits Offering choices through `session.RecordOfferingChoice(choice.MonsterId, choice.RewardId, choice.ChoiceId, choice.ActiveSkillId, choice.PassiveSkillId)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now assigns `ChoiceId = skill.SkillId` for active skill Offering choices and `ChoiceId = passive.PassiveId` for passive skill Offering choices.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now refreshes scene-valid `MonsterUnitActor` models from `RunSession` and rebuilds learned active runtime sets after Offering commit.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User reported Offering-acquired skills were not acquired.
- 2026-05-31: Code Builder patched the Offering UI commit refresh so it no longer depends only on `combatManager.Roster.Players`.

## Task: 2026-05-31 Nexus HP And End Panels

### Task title

Bind Nexus HP text and Win/Defeat panels for the `NewRunScene` end flow.

### Goals

- Display Nexus HP as `current / max` in `Canvas/Info/NexusHPinfo`.
- Show `Canvas/DefeatPanel` on Nexus defeat.
- Show `Canvas/WinPanel` on the configured Stage 2-11 prototype clear condition.
- Use the same return-to-main-menu method for both Win and Defeat buttons.

### Constraints

- Role Owner is Code Builder.
- The real main menu scene path is `Assets/Scenes/NewScene/NewMainMenu.unity`.
- `NexusUnitActor` auto-resolves `Canvas/Info/NexusHPinfo` when the serialized field is blank.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, scene-bound, and compile/editor validated.

### Next Actions

- User verifies in Play Mode that Nexus HP text updates, DefeatPanel/WinPanel activate at the right time, and both buttons load `NewMainMenu`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/NexusUnitActor.cs` writes Nexus HP as `current / max` and auto-resolves `Canvas/Info/NexusHPinfo`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` resolves `Canvas/WinPanel`, `Canvas/DefeatPanel`, and panel child `Button` components, then binds both to `ReturnToMainMenu()`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` hides Win/Defeat panels on startup and shows the matching panel on victory or defeat.
- Unity-MCP scene inspection found `Canvas/Info/NexusHPinfo`, `Canvas/WinPanel`, `Canvas/DefeatPanel`, and `Nexus` with `NexusUnitActor`.
- Unity-MCP `validate_script` on `Assets/Scripts2/InGame/Units/NexusUnitActor.cs` and `Assets/Scripts2/InGame/Core/StageManager.cs` reported 0 warnings and 0 errors.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User asked for Nexus HP display plus Win/Defeat buttons that return to `NewMainMenu`.
- 2026-05-31: Code Builder implemented `NexusUnitActor` HP text binding and StageManager end-panel/button flow.

## Task: 2026-05-31 DebugUI Passive Buttons And Offering-State Recording

### Task title

Extend `NewRunScene` DebugUI to learn passive F-J skills and passive enhancements through the same run-state path used by Offering.

### Goals

- Add F-J DebugUI skill buttons to the same A-J slot resolution path.
- Let F-J buttons learn passive skills for the currently selected player monster.
- Let each F-J button child `EmodifierBtn` open `DebugPassiveModifiedUI`.
- Let `DebugPassiveModifiedUI/Trait1` through `Trait3` acquire passive enhancement choices.
- Record DebugUI skill and enhancement acquisition through `RunSession.RecordOfferingChoice` so learned skills and chosen choice ids can be filtered out of later Offering choices.

### Constraints

- Role Owner is Code Builder.
- This is UGUI `NewRunScene` debug tooling work, not a gameplay balance or CSV schema change.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that F-J buttons learn passives for the selected monster.
- User verifies that F-J `EmodifierBtn` opens `DebugPassiveModifiedUI` and Trait1-Trait3 can be acquired.
- User verifies that DebugUI-acquired skills/enhancements do not appear again in Offering.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:17` expands `DebugSlots` from A-E to A-J.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:133` adds `TryLearnPassiveSlot`, resolving passives with `PakuriDataManager.Instance.ResolvePassiveSkill(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:324` resolves `DebugPassiveModifiedUI`, and `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:377` resolves `DebugPassiveModifiedUI/Trait1` through `Trait3`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:678` adds `ApplyPassiveModifierChoice` for passive enhancement acquisition.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:900` adds `CommitDebugOfferingChoice`, which calls `RunSession.RecordOfferingChoice(...)` and then refreshes runtime skill models, button labels, modifier buttons, and the monster panel.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs:936` keeps enhancement reward lookup exact by returning a matching reward id only when `RewardId == choice.ChoiceId`; otherwise it records the exact choice id fallback.
- Unity-MCP scene inspection found `Canvas/DebugUI/FBtn` through `JBtn`, each with child `EmodifierBtn`, and found `Canvas/DebugPassiveModifiedUI/Trait1` through `Trait3`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after an initial parallel-build file lock.
- Unity-MCP `validate_script` on `Assets/Scripts2/InGame/UI/DebugUI.cs` reported 0 errors and one pre-existing-style validator warning about string concatenation in `Update()`.
- Unity-MCP warning/error console read after script refresh returned 0 entries.

### History

- 2026-05-31: User asked Designer how to extend `DebugUI` for F-J passive acquisition and passive enhancement acquisition.
- 2026-05-31: User then asked Code Builder to implement the described DebugUI and Offering-state recording changes.
- 2026-05-31: Code Builder implemented A-J slot binding, passive acquisition, passive modifier panel binding, and shared run-state commit logic in `DebugUI.cs`.

## Task: 2026-05-17 NewRunScene Active UI Rules

### Task title

Keep the current `NewRunScene` UI behavior compact and explicit.

### Goals

- Preserve active status suffix display on unit name labels.
- Preserve the current `AutoBtn` route that switches 1P A between manual and automatic execution.
- Preserve the current Offering enhancement availability filter based on learned active/passive state.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older UI task history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active `NewRunScene` UI rules summarized and retained for future work. 2026-05-18 Code Builder refactor centralizes shared unit actor display logic and now keeps Offering/Menifest behavior inside `InGameUIManager.cs` through integrated helper types.

### Next Actions

- User verifies in Play Mode that label suffixes, AutoBtn behavior, and Offering gating still match the retained baseline.
- Future UI work should update this file only when those active rules change.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` now delegate shared name/status/HP/shield/damage-popup presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs` plus `InGameCombatManager.cs` own the current AutoBtn behavior.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` keeps `Canvas/AutoBtn` wired to `Pakuri.InGame.InGameAutoSkillButton`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` owns top-level `NewRunScene` UI lookup/binding and now contains the Offering/Menifest flow helper types directly in the same file.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still owns the learned-skill Offering enhancement filter, Offering choice commit path, Menifest popup state, candidate commit, and skip behavior through those integrated helper types.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.

### History

- 2026-05-15: AutoBtn manual/auto routing became part of the active baseline.
- 2026-05-17: Status suffix display and Offering enhancement availability filtering were added to that active baseline.
- 2026-05-18: Code Builder split `InGameUIManager` into Offering and Menifest helper flows, and commonized `MonsterUnitActor`/`EnemyUnitActor` presentation through `UnitActorView.cs`.
- 2026-05-18: Code Builder later re-merged the Offering and Menifest helper files into `InGameUIManager.cs` during the repository-wide high-integration consolidation pass.

## Task: 2026-05-29 Damage Meter UI Handoff

### Task title

Prepare the Code Builder handoff for the authored `NewRunScene` damage meter overlay.

### Goals

- Keep the damage meter UI work grounded in the existing authored `Canvas/DamageMeterUI` hierarchy.
- Route implementation to a separate damage meter UI/controller path instead of expanding Offering/Menifest ownership in `InGameUIManager.cs`.
- Preserve 1P to 5P panel order based on selected monster plus `RunSession.ManifestedMonsterIds`.
- Keep damage meter skill bars bounded by `MeterBG` width, with 1st-place total damage as the full-width reference.
- Apply repeated skill segment colors in red, blue, light green, sky blue, yellow, purple, and dark green order.
- Preserve the authored `Skill-Meter` RectTransform Y/anchor/pivot while resizing cloned skill segments.
- Resolve trigger-based damage meter labels back to the trigger source skill/passive display name when available.
- Prefer `monster_skills.csv` active/passive `display_name` over choice or trigger-derived names when the damage source id is a real skill id.

### Constraints

- Role Owner is Code Builder.
- Designer created the handoff only; no code or scene implementation was performed.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented by Code Builder.

### Next Actions

- User verifies live Play Mode numbers, button behavior, and visual fit for the authored meter layout.
- Future icon work can fill `MonsterIconImage` values in `monsters.csv`; blank values are currently supported.

### Evidence

- Unity-MCP found `Canvas/DamageMeterUIBtn` and `Canvas/DamageMeterUI` in `NewRunScene`.
- Unity-MCP found `Canvas/DamageMeterUI/1PDamagePanel` through `5PDamagePanel`; `1PDamagePanel` includes `Image`, `Monster_Name_Text`, `Total_Damage`, `Total_Damage_Persent`, `MeterBG`, and `Skill-Meter/SkillName`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `ApplyDamage(... source, ... sourceSkillId ...)` and returns `InGameResourceChangeResult`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` stores `ManifestedMonsterIds` in append order.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterRuntimeTracker.cs` records player monster damage by actual health plus shield delta.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` auto-resolves `Canvas/DamageMeterUIBtn`, `Canvas/DamageMeterUI`, `Close`, and `1P~5PDamagePanel` children by name.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` calculates each skill meter width from `source.Damage / leaderDamage`, clamps accumulated width to `MeterBG`, and applies a fixed seven-color segment palette.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` now caches the authored `Skill-Meter` anchor, pivot, and Y position so clones only change X/width.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` now resolves trigger ids such as `rin-f-followup` through `SkillTriggerDefinition.SourceSkillId`, so `rin-f` damage can display the passive name from `monster_skills.csv`.
- `Pakuri/Assets/Scripts2/InGame/UI/DamageMeterUIController.cs` now resolves active/passive skill ids before choice titles or trigger source fallback, and trigger fallback no longer matches `TriggeredSkillId`, preventing `rin-a`/`sein-a` from being overwritten by related passive or trigger labels.
- Unity-MCP component inspection found `Pakuri.InGame.DamageMeterRuntimeTracker` and `Pakuri.InGame.DamageMeterUIController` attached to `Canvas`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity console after CSV validation/sync logged runtime catalog load and CSV runtime catalog sync without Pakuri CSV failure.

### History

- 2026-05-29: User requested a Code Builder implementation handoff for the damage meter UI design.
- 2026-05-29: Code Builder implemented the runtime tracker, UI controller, combat hook, and Canvas scene binding for the authored damage meter overlay.
- 2026-05-29: Code Builder changed skill meter widths to use the leader-damage scale and added the requested seven-color repeating segment palette.
- 2026-05-29: Code Builder preserved authored skill-meter Y/anchor/pivot on clones and routed trigger damage labels back to their source skill/passive display names.
- 2026-05-29: Code Builder changed damage meter label resolution so active/passive `monster_skills.csv` display names take priority over choice and trigger-derived labels.
