## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/VEGA_MONSTER.md`.

# VEGA_MONSTER

## Scope

Vega dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Vega file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Vega active skills A-E and passive skills F-J are implemented and locally validated.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-05-18 Vega-B SingleAttack Runtime Kind

### Task title

Route Vega-B through the new SingleAttack runtime kind for one-shot area damage.

### Goals

- Move Vega-B out of `LineAttack` because the requested CSV row belongs to one-shot `SingleAttack`.
- Preserve existing CSV-authored damage, coefficient, radius, and cooldown.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Vega-B now behaves as a one-shot area hit in the current shared executor path.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md` names Vega-B `移⑤У????쒕룄`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `vega-b runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV row 34 as a one-shot area attack skill for the new `SingleAttack` type.
