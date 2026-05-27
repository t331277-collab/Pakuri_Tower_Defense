## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/ARIEL_MONSTER.md`.

# ARIEL_MONSTER

## Scope

Ariel dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Ariel file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Task: 2026-05-22 Ariel Final Shared Choice Runtime Completion

### Task title

Implement `ariel-a-trait-5` and `ariel-d-trait-5` through shared choice/status contracts and re-audit Ariel coverage.

### Goals

- Add a shared choice snapshot rule that counts shielded allies and converts the count into a per-cast damage multiplier.
- Add a shared status rule that increases incoming damage only when the attacker has a required status and the target carries the marked status.
- Confirm that no Ariel skill, choice, effect, or trigger row remains unsupported after this pass.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay reusable in shared runtime/data paths rather than adding Ariel-only execution branches.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and CSV-sync-verified.

### Next Actions

- User verifies in Play Mode that `ariel-a-trait-5` scales Ariel-A damage by `+6%` per currently shielded ally at cast time.
- User verifies in Play Mode that `ariel-d-trait-5` increases damage only when the attacker has `shield` and the target carries Ariel-D's `holy-exposure` mark.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:7` now marks `ariel-a-trait-5` as `RuntimeImplemented` with `count_status_id=shield`, `count_target_side=AllAllies`, and `damage_multiplier_per_count=0.06`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:28` now marks `ariel-d-trait-5` as `RuntimeImplemented` with `status_conditional_source_status_id=shield` and `status_conditional_damage_taken_bonus=0.1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:216-285` now resolves choices with roster context, counts matching status holders, and applies the dynamic damage multiplier to the cast snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:291-337`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:234-246`, `:366-374`, and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:965-1011` now carry source-conditional incoming-damage status data through status resolution and the live damage path.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1-2` now contains the current status payload schema columns, including `status_ailment_resistance_bonus` and `status_flat_element_resist_reduction`, so editor CSV sync matches the parser contract.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv | Where-Object { $_.monster_id -eq 'ariel' -and $_.implementation_state -notin @('RuntimeImplemented','ReferenceDirect') }`, the matching `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv` checks all returned no rows.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.
- Unity-MCP console after clear plus `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-22: User asked Code Builder to implement `ariel-a-trait-5` and `ariel-d-trait-5` and confirm whether every Ariel skill was now implemented.
