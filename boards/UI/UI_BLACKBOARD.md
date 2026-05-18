## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/UI_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older UI cleanup history remains in that snapshot and earlier archive bundles.
- This active file now keeps only the surviving new-scene UI flow baseline.

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
