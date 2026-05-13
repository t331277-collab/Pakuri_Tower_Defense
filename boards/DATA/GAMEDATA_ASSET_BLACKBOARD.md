## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-08` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.

## Task: 2026-05-13 Combat V2 Data Compatibility Note

### Task title

Record data-asset compatibility requirements for Combat V2.

### Goals

- Keep current CSV/Data loading and ScriptableObject definitions as the source of combat data.
- Use `SkillDefinition` data plus reusable skill executors rather than hardcoding every skill into one controller.
- Treat `SkillEffectPrefab` as presentation data, not as the owner of skill logic.

### Constraints

- Role Owner is Designer.
- No data asset or CSV edits in this task.
- Combat V2 implementation must preserve current data compatibility unless a later task explicitly migrates data schema.

### Role Owner

Designer

### Status

Completed as design context.

### Next Actions

- Code Builder should reuse `MonsterDefinition`, `EnemyDefinition`, `SkillDefinition`, `PassiveDefinition`, and `RunSession.RunMonsterState` in the first Combat V2 implementation slice.
- Any new executor mapping should be additive and should not require changing existing CSV rows first.

### Evidence

- Added `Pakuri/reference/Report/2026-05-13-combat-v2-foundation-architecture.html`.
- `Pakuri/Assets/Scripts/Data/Definition/MonsterDefinition.cs` already exposes combat tuning, active skills, passive skills, and reward choices.
- `Pakuri/Assets/Scripts/Data/Definition/EnemyDefinition.cs` already exposes stats, defenses, attack type, Stage 1 skill kind, and active skill values.
- `Pakuri/Assets/Scripts/Data/Definition/SkillDefinition.cs` already exposes `SkillRuntimeKind`, `SkillEffectPrefab`, coefficients, cooldown, magazine, reload, status ID, and enhancement/master choices.
- User requested skill management that is reusable and avoids hardcoding as much as possible.

### History

- 2026-05-13: User confirmed Combat V2 should preserve existing CSV/Data loading and use a flexible reusable skill structure.

## Task: 2026-05-08 Rin F-J CSV/SO Runtime State Alignment

### Task title

Align Rin F-J CSV implementation state with the existing SO state.

### Goals

- Remove the Rin F-J CSV/SO state mismatch before implementing Manifest party flow.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from actual CSV/SO inspection and build output.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Keep CSV and SO implementation-state fields aligned when future skill runtime states change.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now marks `rin-f`, `rin-g`, `rin-h`, `rin-i`, and `rin-j` as `RuntimeImplemented`.
- Existing Rin SO data had F-J `ImplementationState: 2`; this task changed the CSV side to match the SO/runtime-implemented state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked to clean up the Rin F-J CSV/SO mismatch before Manifest implementation.
