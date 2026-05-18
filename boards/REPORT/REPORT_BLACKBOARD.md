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

## Task: 2026-05-18 C# Script Role Map Report

### Task title

Document the current C# scripts with grounded role and integration-possibility judgments.

### Goals

- Preserve one browsable HTML report that covers every current `Pakuri/Assets` C# script.
- Keep the judgment grounded in inspected code structure instead of guessed ownership.
- Make future rename, merge, and refactor discussions point to one current inventory report.
- Record which previous `Integration = High` candidates were actually merged and how the remaining `Integration = Medium` candidates should be reduced.

### Constraints

- Role Owner is Designer.
- The report must be evidence-based and documentation-only.
- Judgments are structural heuristics from current file contents, code-reference counts, and Unity asset-reference counts, not gameplay verification.

### Role Owner

Designer

### Status

Updated after the Code Builder integration pass, then localized to Korean for the current HTML deliverable, and retained as the current whole-script inventory report.

### Next Actions

- Refresh this report when the script count changes materially or ownership boundaries are reorganized.
- Use this report as the starting point before discussing broad rename or integration work.

### Evidence

- `Pakuri/reference/Report/2026-05-18-csharp-script-role-map.html` now reflects the post-integration 75-script inventory.
- The same HTML report now exposes its visible title, summary, section headings, badges, and plan labels in Korean while preserving actual file paths, type names, and method names from the inspected code as evidence.
- `tools/generate_csharp_role_map.ps1` regenerated the report from the current inspected scripts, including completed `Integration = High` merges and a remaining `Integration = Medium` merge-plan table.
- Command inspection counted 75 `Pakuri/Assets` `.cs` files, and the generator reported `ScriptCount=75` plus `MediumIntegrationCount=29`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the integrations; only existing MSB3277 warnings remained.

### History

- 2026-05-18: User requested an HTML document describing every current C# script's role and importance/integration possibility.
- 2026-05-18: User then requested Code Builder to actually merge the previously identified `Integration = High` files, update the HTML report, and explicitly record the remaining `Integration = Medium` merge direction.
- 2026-05-18: User then requested the generated HTML report itself to be rewritten in Korean without changing the underlying code-evidence identifiers.
