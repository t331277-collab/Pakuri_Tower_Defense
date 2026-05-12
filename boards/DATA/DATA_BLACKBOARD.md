## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-09` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-05-09 Assets Scripts Folder Organization

### Task title

Organize Data scripts under Definition and Runtime subfolders.

### Goals

- Make the Data script structure easier to scan from the folder tree.
- Keep data loading behavior unchanged by moving files only, with `.cs.meta` files moved together.

### Constraints

- Role Owner is Designer -> Code Builder.
- Do not change C# class names, namespaces, serialized field names, or runtime data logic.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Use `Pakuri/Assets/Scripts/Data/Definition`, `Runtime`, `Runtime/Csv`, and `Editor` as the current Data script map.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added design document `Pakuri/reference/Report/2026-05-09-assets-scripts-folder-organization-design.md`.
- Moved `EnemyDefinition.cs`, `GameDataCatalog.cs`, `MonsterDefinition.cs`, and `SkillDefinition.cs` to `Pakuri/Assets/Scripts/Data/Definition`.
- Moved `PakuriDataManager.cs`, `PakuriCsvRuntimeAssetCatalog.cs`, and `PakuriCsvRuntimeSourceCatalog.cs` to `Pakuri/Assets/Scripts/Data/Runtime`.
- Moved `PakuriCsvRuntimeData*.cs` runtime/CSV partials to `Pakuri/Assets/Scripts/Data/Runtime/Csv`.
- Kept editor-only scripts under `Pakuri/Assets/Scripts/Data/Editor`.
- Moved `.cs.meta` files with their matching `.cs` files to preserve Unity script GUIDs.
- Unity-MCP `refresh_unity` reached idle after script refresh.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings after rerunning it alone; the earlier parallel editor build failed only because the runtime build held an `obj\Debug` cache file lock.
- Unity-MCP console warning/error read returned only MCP client handler logs.

### History

- 2026-05-09: User requested organizing `Assets/Scripts` so Data and other domains are clearer from the folder structure.

## Migrated Task Blocks
