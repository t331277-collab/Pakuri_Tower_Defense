# RUNSCENE_UI

This is the active `NewRunScene` UI persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUNSCENE_UI_ARCHIVE_2026-05-18.md`.
- Older RunScene/Manifested UI history remains in that snapshot and earlier archive files.
- This active file now keeps only the current `NewRunScene` UI behavior still relevant to active work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

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
