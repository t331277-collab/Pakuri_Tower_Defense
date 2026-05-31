# AUTOMATION_GUIDE

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Archived History

Older OPS automation and role-policy task blocks were archived to `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md`.

## Task: 2026-05-30 UTF-8 Documentation Read Default

### Task title

Make UTF-8 raw text reads the default documentation inspection path for repository policy work.

### Goals

- Keep markdown and other text-document evidence readable without mojibake during Codex inspection.
- Record the repository-level expectation in `AGENTS.md`.
- Align future document reads on `Get-Content -Raw -Encoding UTF8` where PowerShell is used.

### Constraints

- Role Owner is Designer.
- This task changes workflow policy, not runtime gameplay code.
- Command approval behavior is controlled by the CLI/runtime, not by markdown policy alone.

### Role Owner

Designer

### Status

Implemented.

### Next Actions

- Future markdown and text-document inspection should prefer `Get-Content -Raw -Encoding UTF8`.
- If command approval prompts still appear, reuse the saved approved prefix instead of widening to unrelated command families.

### Evidence

- `AGENTS.md` startup rules now state that markdown and other text documentation files should be read with `Get-Content -Raw -Encoding UTF8` by default.
- A broad approved command prefix for `Get-Content -Raw -Encoding UTF8` was saved in the current CLI session.

### History

- 2026-05-30: User asked to default future document reads to UTF-8 and to record that policy in `AGENTS.md`.

## Task: 2026-05-30 File Read/Write Command Allowance For This Project

### Task title

Record that file read/write shell commands are expected workflow commands in this project.

### Goals

- Make file read/write inspection commands explicit as normal repository workflow.
- Avoid repeated policy churn around enumerating individual command examples.
- Keep UTF-8-safe text reads as the preferred documentation-read pattern.

### Constraints

- Role Owner is Designer.
- This task records project-local workflow policy only.
- CLI approval storage is still controlled by the runtime; markdown policy does not override runtime security.

### Role Owner

Designer

### Status

Implemented.

### Next Actions

- Prefer UTF-8-safe text reads for documentation where practical.
- Reuse saved CLI approvals where possible, while treating file read/write commands as the normal intended workflow in project policy.

### Evidence

- `AGENTS.md` now states that file read/write shell commands inside the intended workspace are normal and expected workflow commands for this project.
- `AGENTS.md` no longer enumerates a short fixed list of command examples and instead records the broader project-level allowance.

### History

- 2026-05-30: User asked to replace the explicit command list with a broader statement that file read/write commands are allowed for this project before continuing implementation work.

## Task: 2026-05-28 Skill Builder Trigger Payload Documentation Guard

### Task title

Document the trigger-row payload guard so Skill Builder handoffs do not produce source-validity errors for trigger-routed `SingleAttack` follow-ups.

### Goals

- Make the Skill Builder documentation explicitly state that trigger `SingleAttack` rows own their own concrete damage payload.
- Prevent future handoffs from treating `damage_multiplier` as an implicit source-skill payload reuse mechanism.
- Keep the guard close to the active Skill Builder blueprint and exception docs.

### Constraints

- Role Owner is Code Builder / Designer.
- This task changes workflow documentation, not runtime gameplay code by itself.
- Claims must stay grounded in the inspected validator, trigger CSV row contract, and edited markdown files.

### Role Owner

Code Builder / Designer

### Status

Implemented and locally verified by targeted markdown inspection plus Unity CSV validation.

### Next Actions

- Future Skill Builder row-bundle handoffs should always restate trigger damage payload fields when `monster_skill_triger.csv` participates in the implementation.

### Evidence

- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now tells Builder that routed `monster_skill_triger.csv` `SingleAttack` follow-up rows must carry positive fixed payload through `base_damage` or `attack/spell` coefficient rather than `damage_multiplier` alone.
- `boards/SkillBluePrint/single-attack-blueprint.md` now includes `TriggerSingleAttackRows` in optional parsed input and explicitly states that a `Fixed` trigger row must pass source CSV validation with its own positive damage payload.
- `boards/SkillBluePrint/skill-csv-exception-guide.md` now says the trigger row owns its own damage payload and that linked `triggered_effect_id` rows do not satisfy trigger-row damage validation.
- `boards/SkillBluePrint/skill-builder-handoff-format.md` now requires concrete trigger damage payload fields in the trigger-row bundle section.

### History

- 2026-05-28: Vega-B master-1 follow-up validation failure showed that the existing Skill Builder docs did not explicitly guard against zero-payload trigger rows that only carried a multiplier.

## Task: 2026-05-24 Projectile Blueprint Nth Launch Branch Extension

### Task title

Promote nth-launch branch chance override into the projectile blueprint common path.

### Goals

- Keep Skill Builder from stopping on approved reusable projectile branch behavior that triggers every nth base projectile launch.
- Document `BranchLaunchPeriod` and `BranchLaunchChanceSet` as optional parsed projectile fields.
- Preserve the stop-and-ask rule for sequence-state behavior not covered by the shared nth-launch branch override.

### Constraints

- Role Owner is Code Builder / Skill Builder because this policy update follows an implemented runtime extension.
- No skill CSV row values, prefab assets, or scene objects were changed by this task.
- Skill Builder still requires parsed input or explicit current CSV/code discovery authorization before editing specific skill rows.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and locally verified by build, Unity refresh, and targeted source inspection.

### Next Actions

- Future projectile Skill Builder requests may use `BranchLaunchPeriod` plus `BranchLaunchChanceSet` for reusable nth-launch branch behavior.
- If a future request needs another nth-launch effect family, add a deliberate shared extension instead of hardcoding a monster-specific branch.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` now lists `BranchLaunchPeriod` and `BranchLaunchChanceSet` as optional common parsed fields.
- `boards/SkillBluePrint/projectile-blueprint.md` now includes nth-launch branch chance override in the common projectile contract.
- `boards/SkillBluePrint/projectile-blueprint.md` now keeps other sequence-state effects outside that shared pattern in stop-and-ask.
- Runtime support was added through `SkillRuntimeInstance.ProjectileLaunchCount`, `ProjectileSkillExecutor` per-launch branch resolution, and snapshot/choice fields for branch launch period/chance.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-05-24: User approved using a reusable launch-count function so `Rin-a` master-2 can make every 3rd launched projectile branch at 100%, and future skills can reuse nth-launch projectile behavior.

## Task: 2026-05-24 Skill Blueprint Execution Path Refresh

### Task title

Refresh Skill Builder blueprint references after the skill execution folder refactor.

### Goals

- Remove stale `SkillExecutors.cs` references from the multi-effect and shield/buff unification blueprints.
- Point future Skill Builder work at the current `Execution/Executors`, `Execution/Runtime`, and `Execution/Actors` files.
- Keep blueprint claims grounded in the currently inspected refactored code structure.

### Constraints

- Role Owner is Designer because this changes workflow/blueprint documentation, not runtime gameplay code.
- No C# script, scene, prefab, or gameplay CSV values were changed.
- Claims must stay grounded in inspected blueprint text and current runtime source paths.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown/code-path inspection.

### Next Actions

- Future Skill Builder work that uses these blueprints should follow the current role-folder paths instead of looking for `SkillExecutors.cs`.
- If more blueprints still mention deleted execution files, update them only after inspecting the current matching runtime type path.

### Evidence

- `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md` now cites `Executors/SingleAttackSkillExecutor.cs`, `Runtime/SkillMultiEffectExecutor.cs`, and `Executors/SupportSkillExecutors.cs` instead of the deleted `SkillExecutors.cs`.
- `boards/SkillBluePrint/shield-buff-status-unification-blueprint.md` now cites `Executors/SupportSkillExecutors.cs`, `Core/InGameCombatManager.cs`, and `Actors/InGameAttachedSkillEffectActor.cs` for the current shield/buff execution path.
- Targeted search in the two edited blueprint files found no remaining deleted `Execution/SkillExecutors.cs` path references; current `SupportSkillExecutors.cs` references are intentional.
- `Test-Path Pakuri\Assets\Scripts2\InGame\Skills\Execution\SkillExecutors.cs` returned `False`.

### History

- 2026-05-24: User asked to update the stale `SkillExecutors.cs` references in the multi-effect and shield/buff status unification blueprints after the execution folder role split.

## Task: 2026-05-24 Skill Builder Blueprint And Explicit-Path Only Default

### Task title

Make Skill Builder default to blueprint plus explicit parsed input/path authority, with CSV/code discovery allowed only by explicit user instruction.

### Goals

- Raise the default Skill Builder boundary to a top-level repository rule.
- Make `Skill Builder` stop instead of inspecting unrelated CSV/code when parsed fields or work paths are missing.
- Reserve current CSV/code inspection as an explicit exception path that the user must request.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in the inspected text of `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/COMMON.md`, `AGENTS_ROLE/GAMEDESIGNER.md`, `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, and this board.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Future `Skill Builder` requests should provide either parsed fields or explicit work paths if code inspection inside a scoped area is expected.
- If the user wants Builder to derive missing values from current CSV/code, the instruction should say so explicitly instead of leaving that as the default.
- Future blueprint updates should preserve this default boundary unless the user explicitly requests a policy change.

### Evidence

- `AGENTS.md` now has `Skill Builder Absolute Boundary`, which limits default Skill Builder authority to the selected blueprint, explicit parsed input, and explicit code/prefab/scene/asset paths, and says CSV/code discovery requires explicit user instruction.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now says default Skill Builder authority is limited to the selected blueprint, explicit parsed skill data, explicit work paths, and files inside those paths only when blueprint-required inspection is needed.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now says reading current CSV/code as a parsed-source discovery step is forbidden by default and allowed only when the user explicitly instructs Builder to do so.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now requires Builder to stop and report missing parsed fields, scoped row bundles, or explicit work paths instead of broad repository discovery.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` final output requirements now include whether explicit CSV/code discovery was used.

### History

- 2026-05-24: User asked whether Skill Builder could be made to implement from blueprint and user-given paths only, with related CSV/code inspection treated as an exception path that runs only on explicit user command.

## Task: 2026-05-24 Passive Stat Blueprint And Skill Builder Route

### Task title

Add a blueprint-only passive-stat contract and route Skill Builder passive requests to it.

### Goals

- Create a dedicated Skill Builder blueprint for always-on passive number-adjustment skills.
- Keep ordinary passive implementation on blueprint plus parsed input only, without CSV/code rediscovery.
- Force triggered, damage-dealing, target-search, or proc-based passive behavior into stop-and-ask instead of implicit implementation.
- Add a clear routing mapping so passive requests no longer fail on missing blueprint selection.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- No C# script, scene, prefab, or gameplay CSV values were changed by this task.
- Claims must stay grounded in inspected Skill Builder routing markdown and existing blueprint patterns.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- Future `Skill Builder` passive requests should route first to `boards/SkillBluePrint/passive-stat-blueprint.md`.
- If a passive creates gameplay actions instead of always-on numeric modifications, Builder should stop and ask instead of weakening the passive-stat contract.
- If a reusable event-driven passive contract is needed later, create a separate passive-trigger blueprint rather than broadening the passive-stat blueprint.

### Evidence

- Added `boards/SkillBluePrint/passive-stat-blueprint.md`.
- `passive-stat-blueprint.md` now defines a blueprint-only contract for `RuntimeKind == Passive` work, including required parsed input, allowed modifier families, common passive-stat contract, and stop-and-ask rules.
- `passive-stat-blueprint.md` explicitly forbids CSV/reference rediscovery by default and blocks event-driven, damage-dealing, target-search, proc-based, and spawn-creating passives behind stop-and-ask.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` known mappings now route `passive`, `passive skill`, `always-on passive`, and `stat passive` requests to `boards/SkillBluePrint/passive-stat-blueprint.md`.

### History

- 2026-05-24: User asked to create a passive blueprint that lets Skill Builder implement ordinary passives from the blueprint alone and to wire the routing path for future passive requests.
