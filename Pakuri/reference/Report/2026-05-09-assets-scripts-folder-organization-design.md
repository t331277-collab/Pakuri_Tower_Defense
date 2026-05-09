# Assets/Scripts Folder Organization Design

## Task title

Organize `Pakuri/Assets/Scripts` into clearer domain subfolders.

## Goals

- Keep the existing `Combat`, `Data`, and `Run` top-level script domains.
- Add responsibility-based subfolders so file purpose is visible from path.
- Move `.cs` files together with their `.cs.meta` files to preserve Unity script GUIDs.
- Do not change C# class names, namespaces, serialized field names, or runtime behavior.

## Current evidence

- `Pakuri/Assets/Scripts` currently contains only `Combat`, `Data`, and `Run` top-level directories.
- `Pakuri/Assets/Scripts/Combat` currently keeps all combat files flat.
- `Pakuri/Assets/Scripts/Run` currently keeps all run files flat.
- `Pakuri/Assets/Scripts/Data` currently has only `Editor` as a subfolder.
- No `.asmdef` file exists under `Pakuri/Assets/Scripts`, so moving files within `Assets/Scripts` does not cross an assembly definition boundary.
- `Pakuri/Assembly-CSharp.csproj` and `Pakuri/Assembly-CSharp-Editor.csproj` contain explicit compile paths, so Unity refresh or project-file regeneration must be verified after moving files.

## Proposed structure

### Combat

- `Combat/Manager`: partial combat controller/service files that manage scene runtime systems.
- `Combat/Monster`: unit and monster/enemy runtime state or combat stat models.
- `Combat/Skill`: skill executors, skill runtime state, effect factory, and damage calculation.

### Data

- `Data/Definition`: ScriptableObject definition models.
- `Data/Runtime`: runtime data manager and runtime catalog assets.
- `Data/Runtime/Csv`: CSV runtime loader, parser, validation, and dataset partials.
- `Data/Editor`: editor-only CSV catalog/export tooling remains editor-only.

### Run

- `Run/Flow`: run entry, bootstrap, flow controller, state, and start context.
- `Run/Session`: run day/session state models.
- `Run/UI`: run-scene and debug-scene UI controllers.

## Handoff to Code Builder

Code Builder should implement this as a file-only organization change, then verify with Unity refresh/console and `dotnet build` for runtime/editor projects.
