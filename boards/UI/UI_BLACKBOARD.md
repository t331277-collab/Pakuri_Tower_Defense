## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/UI_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older UI cleanup history remains in that snapshot and earlier archive bundles.
- This active file now keeps only the surviving new-scene UI flow baseline.

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

## Task: 2026-05-17 Surviving NewScene UI Flow

### Task title

Keep the active UI flow grounded in `NewMainMenu` plus `NewRunScene` only.

### Goals

- Preserve `Assets/Scenes/NewScene/NewMainMenu.unity` -> `Assets/Scenes/NewScene/NewRunScene.unity` as the surviving menu/run path.
- Preserve `Pakuri/Assets/Scripts2/UI/UIManager.cs` as the active menu-side flow owner.
- Keep older Legacy UI/controller retirement detail out of the active board while preserving it in the archive snapshot.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.

### Role Owner

Code Builder

### Status

Current active UI flow summarized and retained for future work.

### Next Actions

- Future menu/run flow work should update this file together with `boards/RUN/RUN_BLACKBOARD.md`.
- Use the archive snapshot when the deleted Legacy scene/controller cleanup history is actually needed.

### Evidence

- `Pakuri/Assets/Scripts2/UI/UIManager.cs` still owns the active menu flow and loads `Assets/Scenes/NewScene/NewRunScene.unity`.
- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity` remains the current menu scene used by that flow.
- `Pakuri/ProjectSettings/EditorBuildSettings.asset` was recorded as containing only the kept `NewMainMenu.unity` and `NewRunScene.unity` scene paths.

### History

- 2026-05-14: `UIManager`-owned `NewMainMenu` flow binding became the retained baseline.
- 2026-05-17: Legacy scene/controller cleanup left only the new scene pair as the supported UI flow.

## Task: 2026-05-26 Floating Damage Text Multi-Popup Retention

### Task title

Keep each floating damage text visible for its own 1 second instead of replacing the previous number on the next hit.

### Goals

- Change the shared world-space damage text path so repeated hits create separate popup text instances.
- Keep the existing per-unit `Damage` TextMesh child as the template and anchor.
- Avoid requiring scene or prefab edits for existing monster and enemy units.

### Constraints

- Role Owner is Code Builder.
- The implementation is scoped to the shared `UnitActorView` damage popup helper.
- Unity Play Mode gameplay/visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that rapid repeated damage keeps previous numbers visible for about 1 second while new numbers appear separately.
- If the popup stacking spacing is visually too tight or too tall, tune `stackVerticalSpacing` on the shared popup component behavior.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs` now keeps `InGameDamageTextPopup` as a template manager and spawns separate cloned `TextMesh` popup objects per `Show(...)` call.
- `InGameDamageTextPopup` default duration is now `1f`, keeps up to `12` active popup instances per unit, offsets concurrent popups by `0.18f`, and destroys each clone after its own timer.
- Existing actor flow is unchanged: `MonsterUnitActor.ShowDamage(...)` and `EnemyUnitActor.ShowDamage(...)` still call the shared popup object.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `validate_script` for `Assets/Scripts2/InGame/Units/UnitActorView.cs` passed with 0 errors and one validator warning about string concatenation in Update.
- Unity `refresh_unity` recovered after reconnect and reported the editor ready; console error read showed existing UnityEditor.Graphs and MCP client handler exception logs, not a compile error from this file.

### History

- 2026-05-26: User asked whether current damage text could remain for 1 second when another hit arrives, then approved Code Builder implementation using the shared popup-clone approach.
