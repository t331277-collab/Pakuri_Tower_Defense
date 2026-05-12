## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/RUN_BLACKBOARD.md`.

## Task: 2026-05-09 Assets Scripts Folder Organization

### Task title

Organize Run scripts under Flow, Session, and UI subfolders.

### Goals

- Make the Run script structure easier to scan from the folder tree.
- Keep run behavior unchanged by moving files only, with `.cs.meta` files moved together.

### Constraints

- Role Owner is Designer -> Code Builder.
- Do not change C# class names, namespaces, serialized field names, or gameplay logic.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Use `Pakuri/Assets/Scripts/Run/Flow`, `Session`, and `UI` as the current Run script map.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added design document `Pakuri/reference/Report/2026-05-09-assets-scripts-folder-organization-design.md`.
- Moved `MainMenuFlowController.cs`, `RunFlowController.cs`, `RunFlowState.cs`, `RunSceneBootstrap.cs`, and `RunStartContext.cs` to `Pakuri/Assets/Scripts/Run/Flow`.
- Moved `RunDayModel.cs` and `RunSession.cs` to `Pakuri/Assets/Scripts/Run/Session`.
- Moved `DebugSceneController.cs` and `RunCombatUiController.cs` to `Pakuri/Assets/Scripts/Run/UI`.
- Moved `.cs.meta` files with their matching `.cs` files to preserve Unity script GUIDs.
- Unity-MCP `refresh_unity` reached idle after script refresh.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings after rerunning it alone; the earlier parallel editor build failed only because the runtime build held an `obj\Debug` cache file lock.
- Unity-MCP console warning/error read returned only MCP client handler logs.

### History

- 2026-05-09: User requested organizing `Assets/Scripts` so Run and other domains are clearer from the folder structure.
