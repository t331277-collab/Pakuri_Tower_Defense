## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/STATUS_EFFECT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/status history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current shared status runtime baseline and the resource-display rule still relevant to active work.

## Task: 2026-05-17 InGame Shared Status Runtime Baseline

### Task title

Keep the current Scripts2 status runtime grounded in `StatusEffectKind` and the shared unit-status store.

### Goals

- Keep all new status work routed through `StatusEffectKind` instead of ad hoc strings.
- Keep status storage, ticking, apply/remove/query, and label refresh owned by shared runtime code.
- Keep Eve-A shock application on the shared projectile hit path.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older status-effect slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active status runtime baseline summarized and retained for future work. 2026-05-18 Code Builder refactor keeps status labels on the actor path while centralizing shared actor presentation in `UnitActorView`. 2026-05-18 projectile/status tuning now reads status chance and label from `monster_skills.csv`; supported runtime labels can now be used as a fallback when `status_effect_id` is blank.

### Next Actions

- Future skills should apply statuses only through `InGameCombatManager.ApplyStatus(...)`.
- Later passive/resistance/damage work should query `StatusEffectKind`-based runtime state rather than adding parallel status storage.
- Use the archive snapshot when older shield/freeze/temporary-effect details are needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` defines the shared enum and central status display helpers.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` owns the current unit status store and ticking behavior.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` owns status apply/remove/query plus actor refresh on state changes.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` delegate active status label presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `InGameProjectileActor.cs` currently route Eve-A shock through the shared projectile hit path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now contains `status_chance` and `status_effect_label` per skill; Eve-A stores `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=감전`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` passes CSV `StatusChance` into `StatusApplicationSpec.Chance`; `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains the Eve-A shock chance special case.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels `감전`, `추위`, `냉기`, `빙결`, `둔화`, `취약`, and `방어막` in addition to the canonical ids.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now resolves blank `status_effect_id` from a parseable `status_effect_label` and stores the canonical status tag from `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` rejects positive `status_chance` values on unsupported runtime status labels/ids.
- Unsupported design labels such as `침묵`, `이름표식`, `신성 노출`, `화염 저항 감소`, `행동속도 증가`, and `넉백` remain label-only in `monster_skills.csv` with `status_chance=0` unless a matching `StatusEffectKind` is added later.

### History

- 2026-05-17: Shared status runtime, enum centralization, label suffix display, and Eve-A shock application became the active baseline.
- 2026-05-18: Code Builder commonized `MonsterUnitActor`/`EnemyUnitActor` display refresh through `UnitActorView.cs`.
- 2026-05-18: Code Builder moved status chance/label authority from monster-level rows and hardcoded Eve-A executor logic into per-skill CSV rows.
- 2026-05-18: Code Builder made supported Korean status labels parseable from CSV, added validation for unsupported positive `status_chance`, and normalized design-only labels to chance 0.

## Task: 2026-05-18 LineAttack Status Application

### Task title

Route LineAttack status application through the shared status runtime.

### Goals

- Let Eve-B apply slow through `InGameCombatManager.ApplyStatus(...)`.
- Reuse CSV status fields for LineAttack skills.
- Avoid a separate Eve-only slow implementation path.

### Constraints

- Role Owner is Code Builder.
- Status chance and status ID are read from `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build plus Unity-MCP mapping inspection.

### Next Actions

- User verifies in Play Mode that Eve-B applies slow at the expected 20% tick chance and that the status label refreshes through the shared unit actor path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` exposes shared status-spec resolution and uses it for projectile and beam skills.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` applies status via `InGameCombatManager.ApplyStatus(...)`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Eve-B row has `status_effect_id=slow`, `status_chance=0.2`, and `status_effect_label=둔화`.
- Unity-MCP mapping inspection returned `status=slow|chance=0.2` for Eve-B.
- Runtime/editor builds passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Eve-B LineAttack implementation reused shared status runtime instead of adding an Eve-only slow path.

## Task: 2026-05-15 Rounded HP Shield Display Baseline

### Task title

Keep HP and shield mutation/display rules grounded in the current rounded-resource implementation.

### Goals

- Preserve whole-number HP and shield mutation results.
- Preserve left-to-right HP fill behavior inside the authored actor background bounds.
- Preserve current damage popup formatting and actor refresh ownership.

### Constraints

- Role Owner is Code Builder.
- This retained baseline is still relevant because HP/shield display remains part of the active InGame combat presentation.
- Detailed intermediate follow-up history is preserved in the archive snapshot.

### Role Owner

Code Builder

### Status

Retained as an active display rule that still affects current combat/runtime work.

### Next Actions

- If shield timing or presentation changes later, update this file together with `boards/RUN/RUN_BLACKBOARD.md` and `boards/UI/RUNSCENE_UI.md`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now contains the integrated resource-mutation helper that rounds applied damage, HP, and shield values.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs` owns the shared left-anchored HP/shield fill presentation used by `MonsterUnitActor.cs` and `EnemyUnitActor.cs`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` still routes rounded damage popup display through the actor layer.

### History

- 2026-05-15: Rounded HP/shield mutation and stable HP fill positioning were recorded as the current baseline.
- 2026-05-18: Code Builder moved shared HP/shield fill and damage-popup presentation from separate actor scripts into `UnitActorView.cs`.

## Task: 2026-05-18 Area Skill Status Application

### Task title

Route AreaAttack and SingleAttack status application through the shared status runtime.

### Goals

- Apply Eve C chill and Eve E vulnerable from CSV-driven area ticks.
- Apply one-shot area statuses through the same shared status helper path.
- Keep unsupported design-only labels at `status_chance=0` unless `StatusEffectKind` supports them.

### Constraints

- Role Owner is Code Builder.
- Status id/chance/label values are read from `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C applies chill per tick and Eve E applies vulnerable per tick.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` applies statuses through `InGameCombatManager.ApplyStatus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` reuses `ProjectileSkillExecutor.ResolveStatusSpec(...)` for `ZoneSkillExecutor` and `SingleAttackSkillExecutor`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` has `eve-c status_effect_id=chill status_chance=1` and `eve-e status_effect_id=vulnerable status_chance=1`.
- Unity-MCP `InGameSkillDataValidator.ValidateCatalog()` returned `valid=True; errors=0; warnings=0`.

### History

- 2026-05-18: Code Builder added area-status routing while adding AreaAttack and SingleAttack runtime execution.
