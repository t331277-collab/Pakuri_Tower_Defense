## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps active report task blocks after the 2026-05-12 archive pass; newer report tasks may be appended above older retained context.
- Source file: `boards/REPORT/REPORT_BLACKBOARD.md`.

## Task: 2026-05-13 Roadmap Shared Target And Skill Reuse Amendment

### Task title

Amend the post-Phase-2-E roadmap with explicit coverage of the 2026-05-10 shared target / temporary effect proposal.

### Goals

- Confirm whether the 2026-05-10 shared combat target and temporary effect proposal is covered by the 2026-05-13 roadmap.
- Add missing timing details for same-type skill reuse, common target model, temporary effects, Monster / Enemy common base, and prefab-based actor authoring.
- Keep the output as an HTML report amendment without runtime C# changes.

### Constraints

- Role Owner is Designer.
- Base every conclusion on inspected reports, board records, or current code search.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Treat `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` as the amended roadmap.
- Keep same-type skill reuse scope open until Phase 6 begins.
- Keep prefab-based Monster / Enemy common authoring as a Phase 8 view/component question, not a replacement for Phase 7 target/effect model migration.

### Evidence

- Updated `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` with section `7. 2026-05-10 공통 대상 / 임시효과 제안 반영 여부`.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:258` through `:268` proposes `CombatTargetModel` with `ActiveEffects`.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:330` through `:346` proposes `ApplyTemporaryEffect(...)`, `GrantShield(...)`, and shield subsystem separation.
- `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html:385` through `:392` lists the migration checklist for selected target, `CombatUnitRuntime`, `EnemyRuntime`, modifier aggregator, action speed, movement/damage multipliers, shield, and status effects.
- Current code search confirmed `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeController.cs:28` defines `EnemyRuntime` as a private nested class and `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:8` defines manifested units as a `MonoBehaviour`.
- Current code search confirmed enemy objects are composed through `AddComponent` calls in `CombatRuntimeEnemies.cs:354`, `:419`, and `:517`; no `enemyPrefab` / `Instantiate` path was found in the searched enemy creation code.

### History

- 2026-05-13: User asked whether the 2026-05-10 shared target / temporary effect improvement proposal was included in the newly created roadmap and asked to amend the HTML with skill reuse, common parent/prefab, and temporary-effect timing.

## Task: 2026-05-13 Combat Runtime Refactor Roadmap After Phase 2-E

### Task title

Create an evidence-based HTML roadmap from Phase 2 closure through the remaining combat runtime refactor.

### Goals

- Confirm whether Phase 2 should be closed after Phase 2-E.
- Explain the remaining Phase 3 through Phase 7 sequence with inspected-code and board evidence.
- Place the user's proposed skill reuse refactor and common combat target / Monster-Enemy base model proposal at the safest timing.
- Save the result as an HTML report.

### Constraints

- Role Owner is Designer because this is design/report work, not runtime implementation.
- Base every conclusion on inspected files, board records, existing HTML reports, or command output.
- Do not change runtime C# behavior.
- Do not run Unity Play Mode.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Use `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html` as the current roadmap before starting Phase 3.
- If implementation starts, begin with a small Phase 3 projectile/effect/drone simulation boundary slice rather than common target model or full skill-executor reuse.

### Evidence

- Read `boards/REFACTORING/REFACTORING.md`, `boards/REFACTORING/COMBAT_STATE_OWNERSHIP_MAP.md`, `boards/COMBAT/COMBAT_BLACKBOARD.md`, `boards/COMBAT/PROJECTILE_BLACKBOARD.md`, `boards/COMBAT/ENEMY_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Inspected `Pakuri/reference/Report/2026-05-13-combat-runtime-controller-phase2e-alignment-report.html`, `Pakuri/reference/Report/2026-05-10-combat-runtime-controller-ai-token-refactor-proposal.html`, `Pakuri/reference/Report/2026-05-10-shared-combat-target-and-temporary-effect-design.html`, and `Pakuri/reference/Report/2026-05-13-combat-refactor-start-plan.html`.
- Code search confirmed `CombatRuntimeController.cs:307` through `:310` still owns `enemies`, `projectiles`, `skillEffects`, and `drones`.
- Code search confirmed `CombatRuntimeController.cs:498` through `:503` still calls `UpdateSpawning()`, `UpdateEnemies()`, `UpdateProjectiles()`, `UpdateMonsterSkillRuntimeEffects()`, `UpdateManifestedMonsterPartyCombat()`, and `UpdateSelectedMonsterCombat()` directly.
- Code search confirmed `CombatRuntimeProjectiles.cs:14` still owns `UpdateProjectiles()` and `CombatRuntimeProjectiles.cs:516` still owns `UpdateSelectedMonsterCombat()`.
- Code search confirmed `CombatRuntimeEnemies.cs:306`, `:706`, and `:945` still own enemy spawning, enemy update, and enemy target priority.
- Code search confirmed `CombatMonsterSkillRuntime.cs:29` still exposes the full `CombatRuntimeController` reference to monster runtime adapters.
- Code search confirmed `CombatUnitRuntime.cs:8` is a `MonoBehaviour` and `CombatUnitRuntime.cs:193` still calls `Owner.TickManifestedUnitSkill(...)`.
- Added `Pakuri/reference/Report/2026-05-13-combat-runtime-refactor-roadmap-after-phase2e.html`.

### History

- 2026-05-13: User asked to create a detailed evidence-based HTML roadmap starting from Phase 2 closure verification, including the timing for skill reuse and common combat model / Monster-Enemy base-class proposals.

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
