## Archived History

- Non-July task blocks from `boards\UI\UI_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/UI_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older UI cleanup history remains in that snapshot and earlier archive bundles.
- This active file now keeps only the surviving new-scene UI flow baseline.

## Task: 2026-07-17 PrisonPanel Reward Flow

### Task title

Replace the prisoner reward popup entry with the authored `Canvas/PrisonPanel` flow.

### Goals

- Open `PrisonPanel` from a prisoner reward and return to `RewardPanel` after Offering or Menifest completion.
- Render the current stage, resources, ordered party slots, and selected prisoner.
- Keep only the next empty party slot available for manifestation.

### Constraints

- Role Owner is Code Builder.
- The existing `PrisonerChoicePopUp` remains in the scene but is no longer the prisoner reward entry path.
- Unity Play Mode gameplay and visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated; user Play Mode verification pending.

### Next Actions

- User verifies RewardPanel → PrisonPanel → Offering/Menifest → RewardPanel navigation in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` resolves and binds `PrisonPanel/1P` through `5P`, `Prisonal`, `StageSum`, `Goldinfo`, and `Darkinfo`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` stores `PrisonPanel` inactive and serializes all five requested player portrait sprites on `InGameUIManager`.
- Runtime and editor C# builds passed with 0 errors; existing MSB3277 warnings remained.
- Unity script validation returned 0 errors, scene validation returned 0 issues, and the error-only console query returned 0 entries.

### History

- 2026-07-17: User approved retaining the existing random player-unit manifestation logic and requested Code Builder implementation.
- 2026-07-17: Code Builder connected the authored PrisonPanel hierarchy to the active reward flow.

## Task: 2026-07-15 DebugPanel Keyboard Toggle

### Task title

Toggle the `NewRunScene` developer UI root with keyboard number 8.

### Goals

- Start `Canvas/DebugPanel` hidden at runtime.
- Toggle the whole panel on and off with the top-row 8 key or numeric-keypad 8 key.
- Keep the input owner active while the developer panel itself is hidden.

### Constraints

- Role Owner is Code Builder.
- Input uses the project's installed Unity Input System `Keyboard` controls.
- No scene serialization change is required because `DebugUI` remains attached to the active root `Canvas` and resolves `DebugPanel` by the inspected hierarchy path.
- Unity Play Mode input verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that 8 opens `Canvas/DebugPanel`, another 8 closes it, and both top-row and numpad 8 work.

### Evidence

- Unity-MCP found active `Canvas/DebugPanel` and found `Pakuri.InGame.DebugUI` attached to `Canvas`, outside that panel.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now resolves `DebugPanel`, hides it in `Awake()`, and checks `Keyboard.current.digit8Key` plus `Keyboard.current.numpad8Key` in `Update()`.
- Installed Input System source `Library/PackageCache/com.unity.inputsystem@21a28c3a6c83/InputSystem/Devices/Keyboard.cs` defines both inspected controls.
- Runtime C# build passed with 0 errors; existing MCP assembly-reference MSB3277 warnings remained.
- Unity script validation reported 0 errors, Unity console read returned 0 errors after clear, and `NewRunScene` validation reported 0 issues.

### History

- 2026-07-15: User requested a Code Builder implementation that toggles `Canvas/DebugPanel` with keyboard number 8.
- 2026-07-15: Code Builder implemented runtime hide/toggle behavior in the existing `DebugUI` owner.
