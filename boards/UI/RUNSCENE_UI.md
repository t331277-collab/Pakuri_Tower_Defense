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
