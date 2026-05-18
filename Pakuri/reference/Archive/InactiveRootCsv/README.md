Inactive root CSV archive.

Moved on 2026-05-18 after verifying:
- `Scripts2` runtime reads `Assets/CSVdata/source/*.csv`, `Assets/CSVdata/EnemySkillData.csv`, and `Assets/CSVdata/SkillChoiceModifierData.csv`.
- No `Assets/*.unity`, `*.prefab`, `*.asset`, or `Scripts2/*.cs` text references remained for the GUIDs of:
  - `MonsterStat.csv`
  - `SkillData.csv`
  - `EnemyStat.csv`
  - `SkillChoiceData.csv`

These files are preserved here as inactive transition/reference data, not as live NewRunScene runtime sources.
