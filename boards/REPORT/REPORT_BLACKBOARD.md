## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps active report task blocks after the 2026-05-12 archive pass; newer report tasks may be appended above older retained context.
- Source file: `boards/REPORT/REPORT_BLACKBOARD.md`.

## Task: 2026-05-13 Combat Runtime Phase 2-E Alignment Report

### Task title

Create a Phase 2-E alignment report against the 2026-05-10 CombatRuntimeController refactor proposal.

### Goals

- Compare the current Phase 2-E refactor result with `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- Check whether the current direction satisfies section 9, `권장 결론`.
- Record what work remains after Phase 2-E.
- Save the result as an HTML report.

### Constraints

- Role Owner is Designer because this is a design/status report, not runtime code implementation.
- Base every conclusion on inspected files, board records, reviewer output, or command output.
- Do not run Unity Play Mode.
- Do not edit runtime C# behavior for this report task.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html` as the current Phase 2-E alignment summary.
- Continue the refactor with Phase 3 `Projectile / Effect / Drone Simulation Split` unless a smaller remaining Phase 2 formula/field-effect slice is identified from inspected code.

### Evidence

- Read `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`; lines `543` through `550` propose state ownership, Manifested Party, Projectile / Effect / Drone, Enemy Simulation, Selected Unit Combat, then adapter narrowing order.
- Read `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`; lines `730` through `736` define section 9, `권장 결론`.
- Read `boards/REFACTORING/REFACTORING.md`, `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md`, `boards/COMBAT/COMBAT_BLACKBOARD.md`, and `boards/COMBAT/PROJECTILE_BLACKBOARD.md` for Phase 0 through Phase 2-E evidence.
- Inspected current code evidence in `Pakuri/Assets/Scripts/Combat/Battlefield/CombatRuntimeBattlefield.cs`, `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeManifestedPartyRuntime.cs`, `CombatRuntimeManifestedPartyView.cs`, `CombatRuntimeManifestedPartySkills.cs`, `CombatRuntimeManifestedPartyDrones.cs`, `CombatRuntimeManifestedPartyVisuals.cs`, `CombatRuntimeManifestedPartyDamage.cs`, `CombatRuntimeController.cs`, `CombatRuntimeParty.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeEnemies.cs`, `CombatMonsterSkillRuntime.cs`, and `CombatUnitRuntime.cs`.
- Current `CombatRuntime*.cs` search found 20 files and 16,221 total lines under `Pakuri/Assets/Scripts/Combat`.
- `Select-String` found direct battlefield list additions centralized inside `CombatRuntimeBattlefield.cs:63`, `:68`, `:73`, and `:78`; other matches were projectile hit-set additions or `manifestedDrones`.
- `codex_loop_logs/phase2_manifested_party_reviewer_20260513.md` exists and ends with `REVIEW_RESULT: PASS`.
- Added `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html`.

### History

- 2026-05-13: User requested an HTML report checking whether Phase 2-E refactoring follows the 2026-05-10 proposal direction, whether section 9 is satisfied, and what work remains.
- 2026-05-13: Designer created the Phase 2-E alignment report without changing runtime C# behavior.

## Task: 2026-05-13 Combat Refactor Start Plan HTML

### Task title

Create a refactoring start plan from the two 2026-05-10 combat reports.

### Goals

- Read the existing shared combat target / temporary effect design report.
- Read the existing CombatRuntimeController AI-token refactor proposal report.
- Inspect current combat runtime code to confirm whether the reported problems still exist.
- Produce a new HTML design report that identifies what problem to solve first and what order to use for the broader refactor.

### Constraints

- Role Owner is Designer because the user requested refactoring structure design and an HTML report.
- Base all conclusions on inspected files and command output.
- Do not run Unity Play Mode.
- No code implementation is included in this design report.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If implementation starts, begin with a Code Builder task for a small `CombatBattlefield` / battlefield facade extraction before introducing full `CombatTargetModel` state ownership.

### Evidence

- Read `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html`.
- Read `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- Inspected `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs`, `CombatRuntimeParty.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeEnemies.cs`, `CombatUnitRuntime.cs`, and `CombatSkillRuntime.cs`.
- Current partial `CombatRuntimeController` files total 14 files, 14,022 lines, and 668,782 characters by command output.
- Added `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html`.
- 2026-05-13 follow-up verification: user-provided `C:\Users\t3312\Downloads\2026-05-10-shared-combat-target-and-temporary-effect-design.html` did not exist by `Test-Path`, so the same local report under `Pakuri/reference/Report/` was used as inspected evidence.
- 2026-05-13 follow-up verification: updated `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html` with a goal-by-goal verification matrix covering God Class, skill reuse, common target model, temporary effects, Monster/Enemy objectification, and common base-class inheritance.
- 2026-05-13 planning follow-up: added `boards/REFACTORING/REFACTORING.md` as the phase-order board for the `CombatRuntimeController` structure split described by `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`.
- 2026-05-13 Phase 0 follow-up: added `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md` as the concrete state ownership map required before the first code extraction slice.

### History

- 2026-05-13: User asked to recognize the current structural problem from the two 2026-05-10 reports and create an HTML plan for which refactor work should start first.
- 2026-05-13: User asked whether following the new HTML would actually satisfy the two proposals' goals such as skill reuse, common Monster/Enemy objectification, inheritance, and God Class removal; Designer verified and amended the report with explicit coverage and gaps.
- 2026-05-13: User asked to record the `CombatRuntimeController` structure split implementation order in `boards/REFACTORING/REFACTORING.md`.
- 2026-05-13: User asked to start from Phase 0, `State Ownership Map`; Designer created the ownership map as a refactoring board artifact.

## Task: 2026-05-12 Boards Korean Translation Export

### Task title

Translate board Markdown files into category-level Korean Markdown reports.

### Goals

- Translate all Markdown files under `boards/` into category-level Markdown outputs.
- Save the generated outputs under `Report/`.
- Preserve source file boundaries so each translated category report can be traced back to the original board file.

### Constraints

- Role Owner is Designer -> Code Builder because the user request was documentation generation and file output.
- Evidence must come from actual `boards/**/*.md` file discovery and generated file checks.
- Code identifiers, file paths, command names, evidence strings, and already-corrupted legacy encoding text are preserved as much as possible for evidence integrity.
- No Unity Play Mode or gameplay verification is involved.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Designer -> Code Builder

### Status

Implemented and locally verified.

### Next Actions

- Use `Report/boards_korean_translation_index.md` as the entry point for the generated category translation files.
- If a later task requires polished human translation for a specific category, start from the corresponding file under `Report/boards_korean_translation/`.

### Evidence

- `Get-ChildItem -Path boards -Recurse -File -Filter *.md` found 26 source Markdown files across 8 categories: `ARCHIVE`, `COMBAT`, `DATA`, `MON`, `OPS`, `REPORT`, `RUN`, and `UI`.
- Generated `Report/boards_korean_translation_index.md`.
- Generated `Report/boards_korean_translation/ARCHIVE.md`, `COMBAT.md`, `DATA.md`, `MON.md`, `OPS.md`, `REPORT.md`, `RUN.md`, and `UI.md`.
- `Select-String -Path Report\boards_korean_translation\*.md -Pattern '^## 원본 파일:' | Measure-Object` returned `Count = 26`, matching the discovered source Markdown file count.
- UTF-8 verification read `Report/boards_korean_translation_index.md` and returned Korean character code points such as `52852`, `53580`, `44256`, and `47532`, confirming the file contents are stored as Korean Unicode even though the PowerShell console rendering displayed mojibake.

### History

- 2026-05-12: User requested translating all category Markdown files under `C:\TowerDefence_Pakuri\Test\boards` and saving category-level Markdown outputs under `C:\TowerDefence_Pakuri\Test\Report`.
