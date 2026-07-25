# RUNSCENE_UI

This is the active `NewRunScene` UI persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archived History

- Non-July task blocks from `boards\UI\RUNSCENE_UI.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUNSCENE_UI_ARCHIVE_2026-05-18.md`.
- Older RunScene/Manifested UI history remains in that snapshot and earlier archive files.
- This active file now keeps only the current `NewRunScene` UI behavior still relevant to active work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-23 Party UI Direct Roster Binding

### Task title

Bind PrisonPanel and DamageMeter directly to the ordered deployed-party ID list.

### Goals

- Render occupied PrisonPanel slots from `RunSession.PartyMonsterIds`.
- Enable manifestation only at the next list index.
- Build DamageMeter party order from the same list without reconstructing selected and manifested IDs.

### Constraints

- Role Owner is Code Builder.
- Existing five authored slots, portraits, labels, buttons, popup flow, and visual layout remain unchanged.
- No scene or prefab file changes.
- Unity Play Mode gameplay and visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity console checked.

### Next Actions

- User verifies PrisonPanel 1P-5P order, next-slot activation, and DamageMeter panel order in Play Mode.

### Evidence

- `InGameUIManager.cs` reads `PartyMonsterIds` directly for occupied count, slot IDs, next manifestation slot, and candidate exclusion.
- The former `ResolvePrisonPartyMonsterIds(...)` selected-plus-manifested reconstruction helper is removed.
- `DamageMeterUIController.cs` copies `PartyMonsterIds` in order and retains its spawned-player fallback only when no active session party exists.
- No UGUI object, serialized field, scene, or prefab changed.
- `git diff --check` passed.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity 6000.3.14f1 instance `Pakuri@0c8eeeb5` reported 0 console error entries.

### History

- 2026-07-23: Code Builder removed duplicated party-list reconstruction from PrisonPanel and DamageMeter.

## Task: 2026-07-23 Remove Dormant Offering Health Field

### Task title

Remove the inactive maximum-health value from Offering and Debug UI commit paths.

### Goals

- Keep Offering presentation and selection flow free of an unauthored view value.
- Preserve reward consumption, UI completion callbacks, Choice recording, and skill-model refresh.

### Constraints

- Role Owner is Code Builder.
- No UGUI object, serialized field, prefab, scene, icon, label, or button binding changes.
- Unity Play Mode gameplay and visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, solution-build verified, and Unity console checked.

### Next Actions

- User verifies Offering and Debug UI acquisition flows in Play Mode.

### Evidence

- `InGameUIManager.cs` removes `OfferingChoiceView.MaxHealthBonus`, its Choice-data copy, and its commit branch.
- `DebugUI.cs` removes the matching maximum-health commit branch.
- Offering continues to call `RecordOfferingChoice(...)`, refresh runtime skills, consume the prisoner button, close the panel, refresh information, and complete the prison action.
- No scene or asset file changed.
- `git diff --check` passed.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity 6000.3.14f1 instance `Pakuri@0c8eeeb5` reported 0 console error entries.

### History

- 2026-07-23: Code Builder removed the dormant UI value without changing visible Offering or Debug UI behavior.

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

## Task: 2026-07-23 Party State Consumer Consolidation

### Task title

Read one RunSession party-member collection from run UI, Debug UI, and DamageMeter.

### Goals

- Remove UI-side party ID reconstruction and duplicate RunSession state lookups.
- Render and target party slots from ordered `RunSession.PartyMembers`.
- Preserve existing manifestation, reinforcement, Debug, and meter behavior.

### Constraints

- Role Owner is Code Builder.
- No scene, prefab, or serialized UI hierarchy change.
- Unity Play Mode visual and interaction verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated. Play Mode UI verification remains.

### Next Actions

- User verifies PrisonPanel slot order, next-empty-slot manifestation, per-slot reinforcement target, and DamageMeter order.
- User verifies normal Offering and Debug acquisition controls against all active/passive/Enhancement/Master limits.

### Evidence

- `InGameUIManager.cs` reads `session.PartyMembers` for occupied slots and uses `GetPartyMemberState(...)` for candidate exclusion and reward targeting.
- `DamageMeterUIController.cs` copies ordered monster IDs from `PartyMembers`; its no-session spawned-player fallback remains.
- `DebugUI.cs` resolves one `RunMonsterState` per acquisition operation and passes it to RunSession eligibility/recording methods.
- No scene or prefab file was modified by this consolidation.
- Related removed-symbol search returned 0 matches; `git diff --check` passed.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 `MSB3277` warning groups.
- Unity 6000.3.14f1 instance `Pakuri@0c8eeeb5` returned 0 console error entries.

### History

- 2026-07-23: Code Builder migrated UI consumers from the interim ID list to the unified party-member state collection.

## Task: 2026-07-25 Offering Basic Skill Record Repair

### Goals

- Prevent the Offering UI from presenting a base skill ID as a Choice ID during commit.

### Constraints

- Role Owner: Code Builder.
- No visual, button binding, scene, or prefab change.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies Vega E basic-skill Offering selection in Play Mode.

### Evidence

- `InGameUIManager.cs` base active/passive `OfferingChoiceView` creation no longer fills `ChoiceId`; `CommitOfferingChoice(...)` passes no linked Choice for those two reward kinds.
- Enhancement/Master UI behavior remains unchanged because its view still fills `ChoiceId` from `reward.RewardId`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal` completed with 0 errors; Unity Console error query returned 0 entries.

### History

- 2026-07-25: Code Builder corrected the UI-to-run-state ID handoff that threw `Unknown learned skill choice 'vega-e'`.
