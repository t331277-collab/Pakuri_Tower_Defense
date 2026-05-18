## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/REPORT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older report history remains in that snapshot and earlier archive bundles.
- This active file now keeps only the currently relevant reports and one active divergence note.

## Task: 2026-05-18 Active Runtime Structure Reports

### Task title

Keep only the currently relevant 2026-05-18 runtime-structure reports in the active report board.

### Goals

- Preserve the current HTML explanation of Stage 1 enemy CSV skill runtime.
- Preserve the current HTML roadmap for CSV runtime cleanup direction.
- Preserve the note that the roadmap became partially stale after `EffectManager` scene authority landed.

### Constraints

- Role Owner is Designer for report authoring and Code Builder for the recorded divergence note.
- These retained report entries are documentation-only state.
- Older HTML and markdown reports remain preserved in the archive snapshot.

### Role Owner

Designer

### Status

Current active report set summarized and retained for future work.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-18-stage1-enemy-csv-skill-runtime-explained.html` for current enemy-skill responsibility explanations.
- Use `Pakuri/reference/Report/2026-05-18-csv-runtime-structure-roadmap.html` as the working cleanup roadmap, but refresh it before relying on outdated base-skill prefab-path conclusions.
- Update this board again only when a newer report becomes more useful than one of these retained entries.

### Evidence

- `Pakuri/reference/Report/2026-05-18-stage1-enemy-csv-skill-runtime-explained.html` documents the current `stage_one_enemies.csv` + `EnemySkillData.csv` + `EffectManager` split.
- `Pakuri/reference/Report/2026-05-18-csv-runtime-structure-roadmap.html` documents the current CSV runtime structure and cleanup direction.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs`, `Pakuri/Assets/CSVdata/source/monster_skills.csv`, `Pakuri/Assets/CSVdata/EnemySkillData.csv`, and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` were recorded as the evidence for the roadmap divergence note.

### History

- 2026-05-18: The current active report set was established around enemy skill CSV runtime explanation, CSV runtime cleanup planning, and the post-EffectManager divergence note.
