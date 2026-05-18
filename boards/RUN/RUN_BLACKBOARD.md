## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUN_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older run/combat flow history remains in that snapshot and earlier archives.
- This active file now keeps only the current `NewRunScene` authority split and the surviving new-scene flow baseline.

## Task: 2026-05-18 NewRunScene Current Runtime Authority

### Task title

Keep the current `NewRunScene` runtime authority split explicit and compact.

### Goals

- Preserve `EffectManager` as the current monster/enemy skill visual authority in the kept new scene flow.
- Preserve the explicit separation between chosen reward IDs and chosen runtime choice IDs.
- Preserve the current CSV runtime catalog path without the serialized `fallbackCatalog` dependency on `NewRunScene`.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed intermediate migration history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active run/runtime authority summarized and retained for future work. 2026-05-18 Code Builder refactor keeps the same runtime authority while moving Offering and Menifest UI logic out of `InGameUIManager`. 2026-05-18 monster projectile/status runtime tuning is now skill-row based.

### Next Actions

- If runtime ownership changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/COMBAT/ENEMY_BLACKBOARD.md`.
- Use the archive snapshot when older step-by-step migration history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `Assets/Scenes/NewScene/NewRunScene.unity` own the current monster/enemy skill visual registry path.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` now keeps `ChosenRewardIds` and `ChosenChoiceIds` separately.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` keeps the top-level reward/UI binding and delegates Offering choice handling to `Pakuri/Assets/Scripts2/InGame/UI/OfferingUI.cs`.
- `Pakuri/Assets/Scripts2/InGame/UI/OfferingUI.cs` passes `rewardId` plus `linkedChoiceId` separately into the session and owns active/passive/enhancement Offering choice construction.
- `Pakuri/Assets/Scripts2/InGame/UI/MenifestUI.cs` owns Menifest candidate, fail, success, commit, and skip popup flow while preserving the `InGameUIManager` scene-binding entry point.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunUnitSpawnManager.cs` and `InGameTestDataManager.cs` no longer keep the retained `fallbackCatalog` scene dependency.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now owns per-skill projectile speed, pierce count, status chance, and status label; `monsters.csv` no longer owns those duplicate projectile/status columns.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps projectile speed, pierce, and status chance directly from `SkillDefinition`; `SkillExecutors.cs` no longer overrides Eve-A shock chance in code.

### History

- 2026-05-18: EffectManager scene authority, reward/choice separation, and removal of the serialized fallback catalog became the active run/runtime baseline.
- 2026-05-18: Code Builder split Offering and Menifest UI flows into `OfferingUI.cs` and `MenifestUI.cs` while keeping `InGameUIManager.cs` as the scene-binding facade.
- 2026-05-18: Code Builder consolidated monster projectile/status tuning into skill rows and verified runtime/editor builds with 0 errors.

## Task: 2026-05-17 Surviving New Scene Flow Baseline

### Task title

Keep the surviving new scene flow and core Eve/status runtime handoff explicit.

### Goals

- Preserve `NewMainMenu.unity` and `NewRunScene.unity` as the surviving supported scene path.
- Preserve the current status-label refresh path and Eve-A choice-modifier execution path used by the kept new run flow.
- Keep the board clear that older Legacy controller retirement progress detail now lives in the archive snapshot.

### Constraints

- Role Owner is Code Builder.
- This retained baseline is kept because it still defines the active scene flow used by ongoing work.
- Detailed phase-by-phase migration history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Retained as the active new-scene flow baseline.

### Next Actions

- Future run work should assume only the `NewMainMenu` -> `NewRunScene` path survives.
- If scene ownership changes, update this file together with `boards/UI/UI_BLACKBOARD.md`.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity` and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` remain the surviving scene pair.
- `Pakuri/ProjectSettings/EditorBuildSettings.asset` was recorded as containing only those two kept scene paths.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `BaseUnitRuntimeModel.cs`, and `StatusEffectKind.cs` own the current status label refresh baseline used by `NewRunScene`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs`, `SkillRuntimeInstance.cs`, and `SkillExecutors.cs` own the current Eve-A chosen-choice execution path.

### History

- 2026-05-17: Legacy scene/controller cleanup, status label runtime, and Eve-A projectile modifier runtime were recorded against the surviving new-scene flow.
