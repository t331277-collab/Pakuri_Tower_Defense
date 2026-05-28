## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUN_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older run/combat flow history remains in that snapshot and earlier archives.
- This active file now keeps only the current `NewRunScene` authority split and the surviving new-scene flow baseline.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-05-27 NewRunScene Manual Projectile Hold Ownership

### Task title

Restrict manual hold-repeat input to projectile skills and preserve one-click behavior for other active skills in `NewRunScene`.

### Goals

- Keep manual input ownership in `InGameCombatManager` instead of moving projectile burst continuation into auto-skill routing.
- Let manual projectile skills re-sample the current cursor direction while the mouse button is held.
- Preserve beam, zone, and single-attack manual casts as one-click actions that do not retarget after activation.

### Constraints

- Role Owner is Code Builder.
- This is a runtime input/control fix only; no CSV authority or scene prefab registry change was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies that manual projectile skills continue firing while the button is held and that cursor movement affects subsequent projectile shots.
- User verifies that manual non-projectile skills still cast once per click and do not change direction or target after activation.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` previously gated all manual skill execution on `IsPrimaryMousePressedThisFrame()`, which prevented projectile burst follow-up shots from routing after the first click.
- The same file now distinguishes projectile runtimes from non-projectile runtimes during manual input handling, using held-button cursor sampling only for `ProjectileSkillData`.
- Manual projectile burst continuation now stays on the manual execution path by reusing latched manual aim/target data when the mouse button is no longer held but a projectile runtime is still bursting.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock on `Assembly-CSharp.dll`.

### History

- 2026-05-27: User requested that projectile skills gain manual hold-repeat behavior while beam, zone, and single-attack skills keep their current one-click activation model.

## Task: 2026-05-26 Rin-B/Rin-C NewRunScene Runtime Verification

### Task title

Verify the current `NewRunScene` runtime accepts the Rin-B/Rin-C shared-skill implementation without compile or refresh errors.

### Goals

- Confirm the shared beam/buff/status runtime changes compile on both runtime and editor assemblies.
- Confirm Unity refresh returns to idle after the Rin-B/Rin-C source changes.
- Confirm warning/error console reads do not show new C# or CSV runtime failures.

### Constraints

- Role Owner is Skill Builder.
- This task records runtime validation only; gameplay verification remains user-owned.
- Existing external assembly conflict warnings are preserved as-is.

### Role Owner

Skill Builder

### Status

Compile-verified and refresh-checked.

### Next Actions

- User verifies Rin-B/Rin-C behavior in Play Mode.
- If a later gameplay-only issue appears, start from the current compile/refresh-clean baseline instead of rechecking schema wiring first.

### Evidence

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after the Rin-B/Rin-C work; only the existing `System.Net.Http` / `System.IO.Compression` MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` first failed inside the sandbox with `Access to the path 'C:\Users\t3312\AppData\Local\Microsoft SDKs' is denied`, then passed with 0 errors when rerun unsandboxed; this was an environment permission issue, not a code error.
- Unity `refresh_unity` returned `resulting_state":"idle"` after the Rin-B/Rin-C source changes.
- Unity warning/error console reads after refresh returned only MCP-FOR-UNITY client connection/disposal logs and did not report C# compile errors or CSV runtime sync failures.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`, `Skills/Execution/Executors/BeamSkillExecutor.cs`, `Skills/Execution/Executors/SupportSkillExecutors.cs`, and `Skills/Execution/SkillMultiEffectExecutor.cs` are the inspected runtime owners for the new Rin-B/Rin-C execution paths validated by the builds and refresh.

### History

- 2026-05-26: Skill Builder completed Rin-B/C implementation and then verified the active `NewRunScene` runtime path through build plus Unity refresh/console checks.

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

Current active run/runtime authority summarized and retained for future work. 2026-05-18 Code Builder refactor keeps the same runtime authority while retaining Offering and Menifest flow helpers inside `InGameUIManager.cs`. 2026-05-18 monster projectile/status runtime tuning is now skill-row based. 2026-05-18 follow-up renamed the enemy combat owner to `EnemyCombatSystem.cs` and absorbed the former cooldown helper into that file.

### Next Actions

- If runtime ownership changes again, update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/COMBAT/ENEMY_BLACKBOARD.md`.
- Use the archive snapshot when older step-by-step migration history is needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `Assets/Scenes/NewScene/NewRunScene.unity` own the current monster/enemy skill visual registry path.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now owns `EnemyCombatSystem`, and `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` now holds both the enemy combat loop and the former cooldown-rule helper logic used during `NewRunScene` combat ticks.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` now keeps `ChosenRewardIds` and `ChosenChoiceIds` separately.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` keeps the top-level reward/UI binding and now contains the Offering and Menifest flow helper types directly in the same file.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now passes `rewardId` plus the exact enhancement `choiceId` into the session and owns active/passive/enhancement Offering choice construction through its integrated helper types.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still owns Menifest candidate, fail, success, commit, and skip popup flow while preserving the same scene-binding entry points.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` no longer keeps the retained `fallbackCatalog` scene dependency.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now owns per-skill projectile speed, pierce count, status chance, and status label; `monsters.csv` no longer owns those duplicate projectile/status columns.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps projectile speed, pierce, and status chance directly from `SkillDefinition`; `SkillExecutors.cs` no longer overrides Eve-A shock chance in code.

### History

- 2026-05-18: EffectManager scene authority, reward/choice separation, and removal of the serialized fallback catalog became the active run/runtime baseline.
- 2026-05-18: Code Builder split Offering and Menifest UI flows into separate helpers while keeping `InGameUIManager.cs` as the scene-binding facade.
- 2026-05-18: Code Builder later merged `OfferingUI.cs` and `MenifestUI.cs` back into `InGameUIManager.cs` during the repository-wide high-integration consolidation pass, keeping the same flow ownership in one file.
- 2026-05-18: Code Builder consolidated monster projectile/status tuning into skill rows and verified runtime/editor builds with 0 errors.
- 2026-05-18: Code Builder renamed `EnemyCombatSimulationSystem.cs` to `EnemyCombatSystem.cs` and absorbed `EnemySkillCooldown.cs` into that owner while preserving the same `NewRunScene` runtime authority path.

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

## Task: 2026-05-29 Damage Meter Runtime Handoff

### Task title

Prepare the runtime damage-source tracking portion of the damage meter UI handoff.

### Goals

- Track player monster damage at the `InGameCombatManager.ApplyDamage` boundary.
- Use actual applied health plus shield delta for current-round totals.
- Preserve `RunSession.ManifestedMonsterIds` order for 2P to 5P display.

### Constraints

- Role Owner is Code Builder.
- Designer created the handoff only; no runtime implementation was performed.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented by Code Builder.

### Next Actions

- User verifies live Play Mode damage totals and source segmentation during combat.
- If future damage executors need more granular source names, pass `damageMeterSourceId` / `damageMeterDisplayName` through those specific executor paths.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `ApplyDamage(... BaseUnitRuntimeModel source, ... string sourceSkillId ...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` returns `InGameResourceChangeResult` with `PreviousHealth`, `CurrentHealth`, `PreviousShield`, `CurrentShield`, and `AppliedDamage`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` appends manifested monster ids in `ManifestedMonsterIds`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` uses `session.ManifestedMonsterIds.Count` to compute manifested spawn slot index.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `DamageMeterRuntimeTracker.RecordDamage(options, result)` immediately after `resourceMutations.ApplyDamage(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now resets `DamageMeterRuntimeTracker.Active` in `StartCurrentDay()` before the current day combat flow starts.
- `DamageApplicationOptions` now carries optional meter-only `DamageMeterSourceId` and `DamageMeterDisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` passes trigger ids as meter source ids for direct trigger damage where the runtime path exposes them.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` accepts optional `damageMeterSourceId` and forwards it to `ApplyDamage`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-29: User requested a Code Builder handoff for damage meter runtime tracking and source naming.
- 2026-05-29: Code Builder implemented the damage meter runtime hook and meter-only source metadata path.
