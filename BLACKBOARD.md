# BLACKBOARD.md

## Task: Token Efficient Reviewer Wrapper

### Task title

Reduce unnecessary token use in the external Builder -> Reviewer wrapper while preserving evidence-based review.

### Goals

- Stop wrapper prompts from encouraging full `BLACKBOARD.md` dumps.
- Keep `AGENTS.md` full-read behavior and preserve related `BLACKBOARD.md` block checks.
- Provide Reviewer with direct changed-file evidence so it can review changed lines without broad repeated exploration.
- Create an HTML report explaining the before/after problem and solution.

### Constraints

- Role Owner is Code Builder.
- All claims must be grounded in actual files and command output.
- Because this modifies the external reviewer wrapper logic, Code Reviewer review is required after Builder implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, Reviewer feedback fixes, and external Code Reviewer PASS completed.

### Next Actions

- On the next actual wrapper run, compare new `*.console.txt` `tokens used` values against the prior 59k-83k token smoke-test logs.

### Evidence

- `codex_builder_reviewer.ps1` now adds `Get-BlackboardIndexText`, `Limit-Text`, `Get-ChangedPathList`, `Get-GitDiffText`, and `Get-AddedFileEvidenceText`.
- The wrapper now writes `blackboard_index.txt`, `loop_XX_git_diff.patch`, and `loop_XX_changed_file_evidence.txt` for each loop.
- `loop_XX_git_diff.patch` is git diff evidence for tracked changes; `loop_XX_changed_file_evidence.txt` is the fallback content evidence for existing changed files including untracked additions.
- Builder and Reviewer prompts now instruct agents to read `AGENTS.md` in full but use `BLACKBOARD.md` through the generated index and related task blocks instead of printing the full file.
- Reviewer prompts now include git diff evidence and changed file content evidence excerpts.
- Added `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- PowerShell parser validation for `codex_builder_reviewer.ps1` returned `PARSE_OK`.
- `git status --short` after Builder implementation showed `M codex_builder_reviewer.ps1`, `M BLACKBOARD.md`, and untracked `Pakuri/reference/Report/2026-04-28-token-efficient-reviewer-wrapper-report.html`.
- External Code Reviewer final rerun returned `REVIEW_RESULT: PASS` in `codex_loop_logs/token_wrapper_reviewer_20260428_rerun2.md`.
- `AGENTS.md` now says Reviewer runs once only, then reports issues to the user instead of continuing an automatic fix loop.
- `AGENTS.md` now says Codex does not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification, while Codex records build/compile/console/editor-state evidence only.

### History

- 2026-04-28: User asked to change the workflow so token use is reduced without weakening evidence-based hallucination prevention, and to create an HTML before/after report.
- 2026-04-28: Code Builder changed the wrapper to create targeted BLACKBOARD and changed-file evidence, then created the HTML report.
- 2026-04-28: External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES` because the HTML report overstated `loop_XX_git_diff.patch` as full changed diff evidence for untracked added files.
- 2026-04-28: Code Builder corrected the HTML report and BLACKBOARD wording to distinguish tracked git diff evidence from changed file content evidence.
- 2026-04-28: External Code Reviewer rerun still found one remaining HTML sentence that overstated full diff patch evidence; Code Builder corrected that sentence.
- 2026-04-28: External Code Reviewer final rerun returned `REVIEW_RESULT: PASS`.
- 2026-04-28: User requested a simple `AGENTS.md` policy update for one Reviewer run only and user-owned Unity-MCP Play Mode verification; Code Builder added the wording to `AGENTS.md` and the HTML report.

## Task: EnemySpawnPoint Editable Position

### Task title

Allow scene-edited `CombatRoot/EnemySpawnPoint` position to persist when starting the game.

### Goals

- Stop runtime scene reference resolution from resetting an existing `EnemySpawnPoint` to the hardcoded default `(29, 8, 0)`.
- Keep default creation behavior for missing anchors.
- Make enemy spawn placement use the edited `EnemySpawnPoint` transform position, including vertical movement.

### Constraints

- Role Owner is Code Builder after Designer handoff.
- User explicitly requested no Code Reviewer stage for this task; proceed with self-review only.
- All claims must be grounded in actual code and command output.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User moves `CombatRoot/EnemySpawnPoint` in `RunScene`, starts Play Mode, and verifies the marker no longer returns to `(29, 8, 0)`.
- User verifies spawned enemies appear around the edited `EnemySpawnPoint` position.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` previously called `EnsureChild(enemySpawnAnchor, "EnemySpawnPoint", new Vector3(29f, 8f, 0f))`.
- `EnsureChild()` previously assigned `current.position = worldPosition` and `existing.position = worldPosition`, which reset existing anchors.
- Added `DefaultEnemySpawnPosition` and changed `EnsureChild()` so existing `current` or found children are returned without overwriting their position.
- `SpawnEnemy()` now starts from `enemySpawnAnchor.position` and applies the configured Y random range as an offset from the default spawn Y, so edited spawn point Y also affects spawn placement.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- External Code Reviewer command was started but the user interrupted it and instructed to proceed with self-review only.

### History

- 2026-04-28: User reported that editing `Combat Root/EnemySpawnPoint` in the scene is reverted when starting the game.
- 2026-04-28: Confirmed reset cause in `EveVerticalSliceController.ResolveSceneReferences()` and `EnsureChild()`.
- 2026-04-28: Changed existing anchor handling to preserve scene-authored positions and adjusted enemy spawn placement to use the anchor transform as the base position.
- 2026-04-28: Per user instruction, skipped Code Reviewer and kept only Builder self-review plus build verification.

## Task: 2026-04-27 Combat Implementation Status Reports

### Task title

Create HTML reports comparing today's combat / monster / enemy implementation with the implementation plan, and separately summarizing code-review-resolved work.

### Goals

- Compare today's implemented skill, damage calculation, Stage 1 enemy, Monster, projectile, and HP bar work against `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.
- Generate one HTML report for implementation status.
- Generate a separate HTML report for work found and resolved through self-review / reviewer-related review flow.
- Keep external Reviewer status accurate and do not claim a PASS verdict where the reviewer command did not complete.

### Constraints

- Role Owner is Designer.
- All claims must be grounded in actual files, BLACKBOARD history, and command output.
- Do not claim Unity Play Mode verification.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can open `Pakuri/reference/Report/2026-04-27-combat-monster-enemy-implementation-status.html`.
- User can open `Pakuri/reference/Report/2026-04-27-code-review-resolved-work.html`.

### Evidence

- Created `Pakuri/reference/Report/2026-04-27-combat-monster-enemy-implementation-status.html`.
- Created `Pakuri/reference/Report/2026-04-27-code-review-resolved-work.html`.
- Read `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.
- Confirmed today's modified scripts with `Get-ChildItem Pakuri\Assets\Scripts -Recurse`.
- Confirmed actual code symbols with `Select-String` in `CombatStatModels.cs`, `DamageCalculator.cs`, `EnemyDefinition.cs`, `SkillDefinition.cs`, `MonsterDefinition.cs`, `GameDataCatalog.cs`, `PakuriGameDataSeeder.cs`, `EveVerticalSliceController.cs`, and `EnemyAttackResolver.cs`.
- Confirmed Stage 1 enemy assets exist under `Pakuri/Assets/Data/GameData/Enemies`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.

### History

- 2026-04-27: User requested two HTML reports: one comparing today's implementation with the combat-monster-enemy implementation plan, and another for code-review-resolved work.
- 2026-04-27: Generated both reports and verified their file presence and key headings.

## Task: Monster And Enemy Hp Slider Bars

### Task title

Add overhead HP text and HP slider bars for Stage 1 enemies and the selected Player Monster.

### Goals

- Add a simple HP slider-style bar above enemies using existing/basic Unity-rendered assets.
- Add the same kind of name, HP text, and HP bar above the selected Player Monster.
- Keep HP text/bar updates tied to the current runtime health values.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.
- Do not import new visual assets for this request; use the existing generated 1x1 shared sprite path in `EveVerticalSliceController`.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. External Reviewer execution was attempted but could not complete because the Codex CLI reported a usage limit. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that enemies show name, HP text, and HP bar above their heads.
- User verifies in Play Mode that the selected Player Monster shows name, HP text, and HP bar above the Monster.
- User verifies the bars shrink as HP decreases for both enemies and the selected Player Monster.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` `EnemyRuntime` now stores `HpBarFill`.
- `EveVerticalSliceController.cs` now stores `selectedMonsterLabel` and `selectedMonsterHpBarFill`.
- `EnsureSelectedMonsterStatusVisuals()` creates/reuses `MonsterHpLabel` and `MonsterHpBar` under `eveAnchor`.
- `SpawnEnemy()` creates `EnemyHpBar` under each spawned enemy, and `UpdateEnemyLabel()` updates both text and bar fill.
- `CreateHpBar()`, `EnsureHpBarPart()`, and `UpdateHpBarFill()` implement the shared world-space HP bar with `SpriteRenderer` and the existing shared 1x1 sprite.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query returned MCP-FOR-UNITY client handler entries only, not project script compile errors.
- External Reviewer command was attempted with `codex.exe exec --skip-git-repo-check`; it failed with a Codex usage-limit message and did not produce a review verdict.

### History

- 2026-04-27: User requested HP Slider Bar using basic assets and the same name/HP display for Player Monster as enemies.
- 2026-04-27: Implemented world-space SpriteRenderer HP bars for enemies and selected Player Monster in `EveVerticalSliceController.cs`.
- 2026-04-27: Attempted external Code Reviewer execution. The command exited before review due to Codex usage limit, so only local Builder self-review, build, Unity refresh, and console checks are available for this turn.

## Task: Enemy Target Priority Monster First

### Task title

Enemy combat flow targets the selected Monster before the Nexus.

### Goals

- Enemies should move toward and attack the Monster before attacking the tower/Nexus.
- If the Monster HP reaches 0, enemies should fall back to the existing Nexus target and Nexus defeat flow.
- Keep the change grounded in the existing `EveVerticalSliceController` combat flow.

### Constraints

- Role Owner is Code Builder.
- User will run Play Mode verification.
- Do not claim gameplay verification; only build, Unity refresh, console check, and self-review are performed here.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that Stage 1 enemies approach the selected Monster first.
- User verifies that Monster HP decreases before Nexus HP, and Nexus starts taking damage only after Monster HP reaches 0.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` now calls `GetEnemyPriorityTarget()` in `UpdateEnemies()` before moving or attacking.
- `GetEnemyPriorityTarget()` returns `eveAnchor` while `unitCurrentHealth > 0f`, then falls back to `nexusAnchor`.
- Enemy damage skills now call `ApplyEnemyDamageToPriorityTarget()`, which subtracts from `unitCurrentHealth` first and from `nexusCurrentHealth` only after Monster HP is depleted.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query showed MCP-FOR-UNITY transport/client handler entries only; no project script compile error was returned.

### History

- 2026-04-27: User requested enemies attack Monsters before hitting the tower.
- 2026-04-27: Confirmed the existing `UpdateEnemies()` flow targeted only `nexusAnchor` and the existing damage function subtracted only `nexusCurrentHealth`.
- 2026-04-27: Changed enemy movement and damage target selection to prefer the Monster while alive, with Nexus fallback after Monster HP reaches 0.
- 2026-04-27: Follow-up self-review fixes applied. Enemy attacks now resolve through `EnemyAttackResolver`, Monster defenses are cloned into runtime target defenses, enemy critical passive bonuses are copied from enemy stats into runtime, and fallback Stage 1 enemy ScriptableObjects are cached with `HideFlags.DontSave`.
- 2026-04-27: Ranged and melee/ranged enemies now fire enemy projectiles. HP damage is resolved only when those projectiles collide with the Monster or Nexus. Enemies now create a simple overhead `TextMesh` label showing name and HP.

## Task: Enemy Projectile And Overhead HP Display

### Task title

Ranged enemies use projectiles and enemies show simple overhead name/HP labels.

### Goals

- Ranged enemies should no longer damage the Monster or Nexus immediately at attack time.
- Enemy projectiles should apply HP damage only after touching the Monster or Nexus target.
- Enemies should show a simple overhead name and HP text.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that Archer/Rogue/Hero Karin style ranged attackers spawn visible enemy projectiles.
- User verifies Monster/Nexus HP changes only when enemy projectiles reach the target.
- User verifies enemy overhead labels remain readable enough and update HP after taking damage/healing.

### Evidence

- `EveVerticalSliceController.cs` `ProjectileRuntime` now has enemy projectile fields: source enemy, target transform, and Monster/Nexus target flag.
- `TryUseStageOneEnemySkill()` now routes `EnemyAttackType.Ranged` and `EnemyAttackType.MeleeAndRanged` default attacks through `FireEnemyProjectile()`.
- `UpdateProjectiles()` now branches enemy projectiles into `TryHitEnemyProjectileTarget()`, which applies Monster or Nexus damage only on collision.
- `SpawnEnemy()` now creates an overhead `TextMesh` through `CreateEnemyLabel()`, and `UpdateEnemyLabel()` writes enemy name and current/max HP.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query returned MCP-FOR-UNITY transport/client handler entries only, not project script compile errors.

### History

- 2026-04-27: User requested ranged enemy projectiles with collision-based HP damage and simple overhead enemy name/HP display.
- 2026-04-27: Implemented enemy projectile runtime path and overhead TextMesh labels in `EveVerticalSliceController.cs`.

## Task: Combat Script Self-Review Fixes

### Task title

Fix self-review findings for Monster defense, enemy critical passives, God Script pressure, and fallback enemy allocation.

### Goals

- Apply Monster attribute defenses when enemies damage the Monster.
- Make enemy critical chance/damage passive fields participate in damage resolution.
- Reduce `EveVerticalSliceController` responsibility by moving enemy attack damage resolution into a helper.
- Avoid creating new fallback Stage 1 enemy ScriptableObjects on every combat initialization.
- Skip text-encoding changes because user confirmed current text is not an issue.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.

### Role Owner

Code Builder

### Status

Builder fixes and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that enemy hits against Monster now use Monster defense and that archer/rogue critical passives can affect damage.
- Future cleanup should continue splitting `EveVerticalSliceController`; current change only extracts enemy attack damage resolution.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/EnemyAttackResolver.cs`.
- `EveVerticalSliceController.cs` now stores `selectedMonsterDefenses`, clones `monster.Defenses`, and passes them to `EnemyAttackResolver.ResolveAgainstMonster`.
- Enemy runtime now copies `CriticalChanceBonus` and `CriticalMultiplierBonus` from `CombatStatBlock` deltas and existing Stage 1 passives add onto those fields.
- Fallback Stage 1 enemy creation now uses static `fallbackStageOneEnemyCache` and `HideFlags.DontSave`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity initially reported missing `EnemyAttackResolver`; `manage_asset import` for `Assets/Scripts/Combat/EnemyAttackResolver.cs` generated/imported the MonoScript asset, and the later console error query showed only MCP-FOR-UNITY client handler entries.

### History

- 2026-04-27: User asked to fix self-review findings in order, excluding the text-encoding item.
- 2026-04-27: Implemented enemy attack damage helper, Monster defense application, enemy critical passive participation, and fallback enemy cache.

## Task: Combat Monster Enemy Implementation

### Task title

전투 기본 규칙 기반 Stage 1 적 / Monster 데이터 / 피해 계산 로그 구현

### Goals

- `combat-monster-enemy-implementation-plan.html`의 방향대로 공통 전투 모델, 속성별 방어력 계산, Stage 1 적 데이터와 런타임 효과를 구현한다.
- Monster 5명의 액티브 A~E, 패시브 F~J 데이터 슬롯을 만든다.
- Monster가 적에게 피해를 입힐 때 Unity Console `Debug.Log`로 계산식과 적용 피해를 간단히 출력한다.

### Constraints

- Role Owner는 Code Builder다.
- 사용자가 플레이 실행 검증은 직접 수행한다고 했으므로 Codex는 Play Mode를 실행하지 않는다.
- 사용자가 자체 리뷰까지만 요청했으므로 외부 Reviewer는 호출하지 않고 Builder 자체 리뷰와 빌드/콘솔 확인까지만 수행했다.
- 판단은 실제 코드, asset, 명령 출력에 근거한다.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- 사용자가 Unity Play Mode에서 MainMenuScene 또는 RunScene 흐름을 실행해 Stage 1 적 스폰, 적 액티브/패시브, 몬스터 피해 계산 로그를 확인한다.
- Unity Console에서 `[CombatDamage]` 로그가 공격자, 스킬, 대상, 속성 방어력 공식, 최종 적용 피해를 출력하는지 확인한다.

### Evidence

- 추가한 공통 전투 타입: `Pakuri/Assets/Scripts/Combat/CombatStatModels.cs`.
- 확장한 피해 계산: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`가 속성별 방어력, 고정/퍼센트 방어 보정, 치명타 저항, 최종 배율, `FormulaLog`를 처리한다.
- 추가한 데이터 타입: `Pakuri/Assets/Scripts/Data/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs`.
- 확장한 카탈로그/몬스터 데이터: `GameDataCatalog.cs`에 `StageOneEnemies`, `MonsterDefinition.cs`에 `PrimaryAttribute`, `BaseStats`, `Defenses`, `ActiveSkills`, `PassiveSkills`를 추가했다.
- 전투 연결: `RunFlowController.cs`, `RunSceneBootstrap.cs`가 `GameDataCatalog`를 `EveVerticalSliceController.BeginConfiguredDay(...)`에 넘긴다.
- 전투 런타임: `EveVerticalSliceController.cs`가 Stage 1 적 풀을 사용하고, 검사/방패병/궁수/도적/사제/수호대장/공격대장/용사 카린의 액티브/패시브 런타임 효과를 처리한다.
- 11일차는 Stage 1 규칙대로 수호대장, 공격대장, 용사 카린을 모두 보스 스폰 대상으로 처리하도록 수정했다.
- 몬스터가 적에게 피해를 줄 때 `Debug.Log("[CombatDamage] ...")`로 속성 방어력 공식, 최종 피해, 실제 적용 피해, 남은 보호막/HP를 출력한다.
- `Pakuri/Seed Default Game Data` 메뉴 실행 후 `Pakuri/Assets/Data/GameData/Enemies` 아래 Stage 1 적 8종 asset이 생성됐고, `GameDataCatalog.asset`에 `StageOneEnemies` 참조가 기록됐다.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` 확인 결과 `PrimaryAttribute`, `ActiveSkills`, `PassiveSkills`, `ImplementationState`가 기록됐다.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`는 오류 0개로 통과했다. 남은 경고는 기존 Unity/MCPForUnity `System.Net.Http`, `System.IO.Compression` 버전 충돌 경고 2개다.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`는 오류 0개로 통과했다. 남은 경고는 동일한 기존 참조 경고 2개다.
- Unity console error 조회는 MCP-FOR-UNITY client handler exit 로그만 반환했고, 새 프로젝트 컴파일 오류는 확인되지 않았다.

### History

- 2026-04-27: 사용자 지시로 Designer 설계 HTML 기준 구현에 착수했다.
- 2026-04-27: `AGENTS.md`, `BLACKBOARD.md`, Unity MCP skill 지침을 먼저 확인했다.
- 2026-04-27: 기존 `EveVerticalSliceController`가 적 방어력을 `0f`로 넘기는 구조임을 확인하고 속성별 방어력 계산을 추가했다.
- 2026-04-27: Stage 1 적 데이터와 Monster 5명 스킬/패시브 데이터 자산 생성을 위해 `PakuriGameDataSeeder.cs`를 확장하고 메뉴를 실행했다.
- 2026-04-27: 자체 리뷰 중 11일차 다중 보스 규칙 누락을 발견해 수호대장, 공격대장, 용사 카린이 모두 스폰되도록 수정했다.
- 2026-04-27: 런타임/에디터 빌드와 Unity 콘솔 error 확인까지 완료했다.

## Task: Combat Monster Enemy Implementation Plan

### Task title

전투 기본 규칙, Monster 스킬, Stage 1 적 구현 방식 HTML 설계

### Goals

- `Pakuri/reference/3.combat` 전투 기본 기획서와 `Pakuri/reference/5.enemy` 적 기획서를 실제 파일 기준으로 읽고 구현 방향을 정리한다.
- 필요한 경우 `Pakuri/data` CSV의 역할을 확인하되, 실제 문서와 충돌하는 값은 그대로 사용하지 않는다.
- Monster의 속성별 방어력, 액티브 스킬, 기본 능력치, 패시브와 Stage 1 적 구현 방식을 HTML 문서로 정리한다.

### Constraints

- Role Owner는 Designer이며 실제 C# 구현은 하지 않는다.
- 모든 판단은 실제 문서, CSV, 현재 C# 코드 내용에 근거한다.
- 현재 프로젝트에는 CSV 런타임 로더가 확인되지 않았으므로 CSV 직접 로딩을 구현된 것처럼 쓰지 않는다.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- 사용자가 구현을 원하면 이 HTML을 기준으로 Code Builder에게 handoff한다.
- Builder 단계에서는 공통 전투 데이터 모델, 속성별 방어력 계산, Stage 1 적 자산, 스킬 실행기 순서로 들어간다.

### Evidence

- 읽은 전투 문서: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `buff-debuff.md`, `realtime-damage-meter.md`.
- 읽은 적 문서: `Pakuri/reference/5.enemy/stage-basic-rules.md`, `enemy-stage-index.md`, `stage-1-enemies.md`.
- 읽은 Monster 문서: `Pakuri/reference/2.Monster/monster-basic-rule.md`, `monster-skill-patterns.md`, `skill-choice-pool-rule.md`, 각 Monster tower 문서와 스킬 문서 목록.
- 확인한 CSV: `Pakuri/data/enemies.csv`, `enemy_runtime.csv`, `skills.csv`, `skill_runtime.csv`, `ally_units.csv`, `ally_runtime.csv`, `status_effects.csv`, `levelup_choices.csv`, `skill_branches.csv`, `levelup_rules.csv`.
- 확인한 현재 코드: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `EveVerticalSliceController.cs`, `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`.
- 생성한 문서: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

### History

- 2026-04-27: AGENTS.md와 BLACKBOARD.md를 먼저 읽었다.
- 2026-04-27: `rg`가 설치되어 있지 않아 PowerShell `Get-ChildItem`과 `Get-Content`로 실제 파일 목록과 내용을 확인했다.
- 2026-04-27: `Pakuri/reference/run-systems-integration-summary-report.html`는 BLACKBOARD 기록과 달리 해당 경로에 없고, 실제 파일은 `Pakuri/reference/Report/run-systems-integration-summary-report.html`에 있음을 확인했다.
- 2026-04-27: Stage 1 적 문서와 CSV의 현재 적 데이터가 직접 일치하지 않으므로 Stage 1 수치는 문서 우선, CSV는 스키마 참고로 정리했다.
- 2026-04-27: Designer 설계 HTML `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`를 추가했다.

## Task: 2026-04-26 Run UI Implementation Status Report

### Task title

HTML report for completed and incomplete Run / UI implementation work on 2026-04-26

### Goals

- Compare today's implementation against `run-systems-integration-summary-report.html` and `monster-select-run-ui-expansion-plan.html`.
- Document completed work, incomplete work, UI editability issues, and chosen UI editing direction.

### Constraints

- All claims must be based on actual files, scene state, command output, or `BLACKBOARD.md` history.
- Do not include work-time estimates in the report.
- Reflect the user's decision that game data is made inside Unity and consumed from Unity assets, not from runtime CSV loading.
- Reflect the user's decision that UI will use editable scene UI: Codex may create a base UI, and user-authored UI should be modified/bound rather than replaced.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User can open `Pakuri/reference/2026-04-26-run-ui-implementation-status-report.html` to review the report.

### Evidence

- Created `Pakuri/reference/2026-04-26-run-ui-implementation-status-report.html`.
- The report references actual implementation files including `MainMenuFlowController.cs`, `RunCombatUiController.cs`, `RunSceneBootstrap.cs`, `RunStartContext.cs`, `RunSession.cs`, `MonsterDefinition.cs`, and `GameDataCatalog.cs`.
- File timestamp check confirmed the report exists under `Pakuri/reference`.
- Updated the report to remove work-time content, UI Toolkit incomplete-scope content, and user Play Mode verification from the incomplete-scope table.
- Updated the report to state that CSV is not the runtime data path; Unity-created assets such as `MonsterDefinition` and `GameDataCatalog` are the chosen data source.

### History

- 2026-04-26: User requested an HTML work report based on `run-systems-integration-summary-report.html` and `monster-select-run-ui-expansion-plan.html`.
- 2026-04-26: Read both source HTML files, implementation file lists, data asset lists, scene file timestamps, manifest TextMeshPro evidence, and generated the report.
- 2026-04-26: User requested removal of Play Mode verification, work-time content, and UI Toolkit incomplete-scope content; user also fixed the direction to Unity-created data assets and editable scene Canvas UI. Updated the report accordingly.

## Task: RunScene Reward Button Visibility Fix

### Task title

RunScene stage-clear reward buttons are fixed editable slots and visible when rewards exist

### Goals

- Fix the RunScene issue where stage-clear reward buttons did not appear.
- Keep reward UI objects editable in Edit Mode instead of relying on delete/recreate behavior.
- Preserve authored button labels where possible, while runtime reward labels are still assigned from actual reward data.

### Constraints

- No external reviewer for this task; perform simple self-review only.
- Do not run Unity Play Mode; user performs gameplay verification.
- All claims must be based on actual files, scene state, or command output.

### Role Owner

Code Builder

### Status

Builder fix applied and self-reviewed. Waiting for user Play Mode verification.

### Next Actions

- User verifies RunScene stage clear: reward panel appears with reward buttons, selecting a reward enables the continue flow.
- If reward panel appears but a button is blocked or misplaced, inspect the saved RectTransform values of `RewardPanel`, `RewardButtons`, and `RewardButton_0..2`.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now uses fixed `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` slots under `RewardButtons`.
- `RebuildRewardButtons()` clears only the tracked button list, calls `EnsureRewardButtonSlots(false)`, then activates slots based on `combatController.GetRewardChoiceCount()`.
- `EnsureRewardButtonSlots()` repairs zero-height `RewardButtons`, ensures the three named button slots, and hides non-slot legacy buttons such as `RewardPreviewButton`.
- Existing nonzero reward button slot RectTransforms keep their authored positions/sizes; default positions are applied only when a slot is newly created or has a broken zero size.
- `EnsureButton()` now preserves existing non-empty labels unless an overwrite is explicitly requested or a label is newly created/empty.
- Unity MCP RunScene inspection after `OnEnable` reported `RewardButton_0`, `RewardButton_1`, and `RewardButton_2` active in Edit Mode, and `RewardPreviewButton` inactive.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity console error check after clearing showed MCP-FOR-UNITY client handler exit logs only, not project script compile errors.

### History

- 2026-04-26: User reported RunScene reward buttons do not appear.
- 2026-04-26: Scene inspection found `RewardButtons` previously had zero height and fixed reward slots were missing, while monster assets contained reward choice data.
- 2026-04-26: Added persistent reward slots, repaired reward root sizing, hid legacy preview buttons, and made existing RunScene reward UI visible in Edit Mode.

## Task: MainMenu Persistent Editable Panels

### Task title

MainMenuScene stage-transition UI panels are persistent scene objects

### Goals

- MainMenuScene UI transitions must not create/delete runtime screen UI.
- Touch To Start, Run menu, and Character Select must all exist in the scene so the user can edit them together in Edit Mode.
- Future UI direction: authored scene UI is the source of truth; scripts bind callbacks, toggle visibility, and only create missing named anchors.

### Constraints

- No external reviewer for this task; perform simple self-review only.
- Do not run Unity Play Mode; user performs gameplay verification.
- All claims must be based on actual files, scene state, or command output.

### Role Owner

Code Builder

### Status

Builder changes applied and self-reviewed. Waiting for user Play Mode verification.

### Next Actions

- User verifies MainMenuScene flow: Touch To Start -> Run -> Character Select -> RunScene.
- If user edits any of `TouchToStartPanel`, `RunMenuPanel`, `CharacterSelectPanel`, or their child labels/buttons, verify those edits persist after entering Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs` now has separate persistent fields for `touchToStartPanel`, `runMenuPanel`, `characterSelectPanel`, and `monsterButtonRoot`.
- `MainMenuFlowController.OnEnable()` calls `ShowAllPanelsForEditing()` only when `Application.isPlaying` is false, so all panels are visible in Edit Mode.
- Runtime methods `ShowTouchToStart()`, `ShowRunMenu()`, and `ShowCharacterSelect()` call `SetPanelVisibility(...)` and no longer call `Destroy`, `DestroyImmediate`, or `ClearButtons`.
- `EnsureText()` and `EnsureButton()` set default text/style only when a component is newly created, preserving existing authored UI text and styling.
- Unity MCP scene check reported `MainMenuCanvas` child count 3 after cleanup.
- Unity MCP code execution reported `TouchToStartPanel active=True children=3`, `RunMenuPanel active=True children=3`, and `CharacterSelectPanel active=True children=4`.
- Unity MCP code execution reported five persistent character buttons under `MonsterButtons`: `MonsterButton_ariel`, `MonsterButton_eve`, `MonsterButton_rin`, `MonsterButton_sein`, and `MonsterButton_vega`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity console error check showed only MCP-FOR-UNITY client handler exit log entries, not project script compile errors.
- `Pakuri/Packages/manifest.json` search found `com.unity.ugui` and no `com.unity.textmeshpro` line.
- Asset search under `Pakuri/Assets` found no `TextMeshPro`, `TMP_Text`, `TMPro`, or `LiberationSans` usage/assets.
- Generated `Pakuri/Assembly-CSharp.csproj` contains `Unity.TextMeshPro` references, but current project UI scripts and scene-generated UI are still based on `UnityEngine.UI.Text`.

### History

- 2026-04-26: User requested MainMenuScene click-transition screens to be editable at once instead of created/deleted at runtime, and asked why UI text is not TextMeshPro text.
- 2026-04-26: Replaced the single dynamic `MainMenuPanel` flow with persistent `TouchToStartPanel`, `RunMenuPanel`, and `CharacterSelectPanel` scene objects.
- 2026-04-26: Removed the obsolete generated `MainMenuPanel` from `MainMenuScene` and saved the scene.
- 2026-04-26: Verified build, scene hierarchy, persistent character buttons, and console state.
- 2026-04-26: User reported UI Pos X / Pos Y could not be edited. Actual code and scene checks found `VerticalLayoutGroup` and `ContentSizeFitter` on generated UI containers.
- 2026-04-26: Updated `MainMenuFlowController` and `RunCombatUiController` so generated UI containers remove `VerticalLayoutGroup` / `ContentSizeFitter` instead of adding them.
- 2026-04-26: Verified MainMenuScene `TouchToStartPanel`, `RunMenuPanel`, `CharacterSelectPanel`, and `MonsterButtons` report `VLG=False, CSF=False`; also removed and saved those components from RunScene reward/defeat UI containers.

## Task: Preserve Authored UI Layouts

### Task title

사용자 편집 UI가 플레이 시작 시 코드 기본값으로 되돌아가는 문제 수정

### Goals

- 에디터에서 사용자가 수정한 UI 위치, 크기, 색, 폰트 설정이 게임 시작 시 유지되게 한다.
- `MainMenuFlowController`, `RunCombatUiController`가 기존 UI 계층을 발견하면 재생성/기본값 재적용 대신 참조만 캐싱하게 한다.
- 새 UI가 없을 때만 기본 UI를 생성한다.

### Constraints

- 외부 Code Reviewer는 호출하지 않고 자체 코드 리뷰만 수행한다.
- Codex가 Unity 플레이 모드를 실행해 검증하지 않고, 실제 플레이 검증은 사용자에게 맡긴다.
- 판단과 설명은 실제 파일, 실제 씬, 실제 명령 출력 근거를 기준으로 한다.

### Role Owner

Code Builder

### Status

Builder changes applied. 자체 빌드/콘솔 확인까지 완료했고, 사용자 플레이 검증 대기 상태다.

### Next Actions

- 사용자가 `MainMenuScene` 또는 `RunScene`에서 UI를 수정한 뒤 플레이를 시작해 위치/크기/색 등 편집값이 유지되는지 검증한다.
- 만약 특정 버튼이 단계 전환 중 새로 생성되어 스타일이 달라지는 경우, 해당 버튼 이름과 씬을 근거로 받아 고정 UI 패널 방식으로 더 분리한다.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`는 기존 `MainMenuPanel`이 있으면 `BuildUiScaffold()`를 다시 실행하지 않고 `CacheUiReferences()`로 기존 `Title`, `Summary`, `Buttons` 참조만 잡는다.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`는 기존 `HudPanel`, `RewardPanel`, `DefeatPanel`이 있으면 `BuildUiScaffold()`를 다시 실행하지 않고 `CacheUiReferences()`로 기존 참조만 잡는다.
- 두 컨트롤러 모두 새 오브젝트/컴포넌트가 생성된 경우에만 RectTransform 크기, Image 색, Text 폰트/정렬 같은 기본 스타일을 적용하도록 변경했다.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`는 오류 0개로 통과했다. 남은 경고는 Unity/MCPForUnity 참조의 `System.Net.Http`, `System.IO.Compression` 버전 충돌 경고 2개다.
- Unity 콘솔 error 조회에서는 새 스크립트 컴파일 오류가 보이지 않았고, MCP client 종료 로그만 확인됐다.

### History

- 2026-04-26: 사용자 검증에서 UI를 수정해도 게임 시작 시 코드 기본값으로 되돌아가는 문제가 보고됐다.
- 2026-04-26: 실제 코드 확인 결과 `BuildUiScaffold()`, `EnsurePanel()`, `EnsureText()`, `EnsureButton()`이 기존 UI에도 기본 RectTransform/색/텍스트 스타일을 반복 적용하고 있음을 확인했다.
- 2026-04-26: 기존 UI가 있으면 캐싱만 수행하고, 기본 스타일은 새로 생성된 UI에만 적용하도록 수정했다.

## Task: RunScene Combat UI Restoration And Edit Mode Visibility

### Task title

RunScene 전투 HUD / 보상 UI 복구와 에디터 비실행 UI 표시

### Goals

- `RunScene`에서 스테이지 클리어 후 보상창이 다시 뜨게 한다.
- 전투 중 타워 HP, 캐릭터 HP, 탄창, 리로드 남은 초, 재화 상태 HUD가 다시 보이게 한다.
- `MainMenuScene`과 `RunScene`의 UI가 플레이 실행 전 에디터 상태에서도 생성되어 직접 편집 가능하게 한다.

### Constraints

- 외부 Code Reviewer는 호출하지 않고 자체 코드 리뷰만 수행한다.
- Codex가 Unity 플레이 모드를 실행해 검증하지 않고, 실제 플레이 검증은 사용자에게 맡긴다.
- 판단과 설명은 실제 파일, 실제 씬, 실제 명령 출력 근거를 기준으로 한다.

### Role Owner

Code Builder

### Status

Builder changes applied. 자체 빌드/콘솔/씬 계층 확인까지 완료했고, 사용자 플레이 검증 대기 상태다.

### Next Actions

- 사용자가 Unity에서 `MainMenuScene -> RunScene` 흐름을 실행해 전투 HUD와 클리어 후 보상창 표시를 검증한다.
- 에디터 비실행 상태에서 `MainMenuCanvas`, `RunCombatCanvas` 하위 UI 오브젝트를 직접 선택/편집할 수 있는지 확인한다.

### Evidence

- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`에 `[ExecuteAlways]`를 추가하고, 비실행 상태에서도 `Touch To Start` UI를 생성하게 했다.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs`를 추가해 `RunScene` 전투 HUD, 보상 패널, 패배 패널을 담당하게 했다.
- `RunCombatUiController`는 HUD에 타워 HP, 캐릭터 HP, 탄창, 재장전 남은 시간, 골드, 흔적을 표시한다.
- `RunCombatUiController`는 전투 승리 후 `EveVerticalSliceController`의 보상 후보를 읽어 보상 버튼을 만들고, 보상 선택 후 다음 일차로 진행한다.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`는 `ActiveMonster`, `ActiveSession`, `FallbackMonsterId`를 공개해 전투 UI가 현재 런 세션을 읽을 수 있게 했다.
- Unity MCP 씬 작업으로 `RunScene`에 `RunCombatCanvas`와 `RunCombatUiController`를 추가했고, `CombatRoot` / `GameDataCatalog.asset` 참조를 연결했다.
- Unity MCP 계층 확인 결과 `MainMenuScene`의 `MainMenuCanvas`에는 자식 UI 1개가 생성됐다.
- Unity MCP 계층 확인 결과 `RunScene`의 `RunCombatCanvas`에는 자식 UI 3개가 생성됐다.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`는 오류 0개로 통과했다. 남은 경고는 Unity/MCPForUnity 참조의 `System.Net.Http`, `System.IO.Compression` 버전 충돌 경고 2개다.
- Unity 콘솔 error 조회에서는 새 스크립트 컴파일 오류가 보이지 않았고, MCP client 종료 로그만 확인됐다.

### History

- 2026-04-26: 사용자 플레이 검증 결과 `RunScene`에 HUD와 클리어 후 보상 UI가 표시되지 않는 문제가 보고됐다.
- 2026-04-26: 원인은 `RunScene` 분리 과정에서 기존 `RunFlowController`가 제거되며 전투 HUD/보상 UI 담당자가 사라진 것으로 판단했다.
- 2026-04-26: `RunCombatUiController`를 새로 추가하고 `RunScene`에 `RunCombatCanvas`를 배치했다.
- 2026-04-26: `MainMenuFlowController`와 `RunCombatUiController`가 에디터 비실행 상태에서도 UI 자식을 만들도록 `[ExecuteAlways]` 기반으로 보정했다.

## Task: Main Menu To RunScene Flow Separation

### Task title

MainMenuScene 단계 전환과 RunScene 전투 전용 진입 분리

### Goals

- `RunScene`에 들어 있던 캐릭터 선택 UI 흐름을 `MainMenuScene`으로 분리한다.
- `MainMenuScene`은 `Touch To Start -> 런 버튼 -> 캐릭터 선택 -> RunScene 입장` 단계 전환을 담당한다.
- `RunScene`은 선택된 캐릭터와 `RunSession`을 받아 전투만 시작한다.
- 씬 간 전달은 확장성을 고려해 `DontDestroyOnLoad` 기반 `RunStartContext`로 처리한다.

### Constraints

- 외부 Code Reviewer는 호출하지 않고 자체 코드 리뷰만 수행한다.
- Codex가 Unity 플레이 모드를 실행해 검증하지 않고, 실제 플레이 검증은 사용자에게 맡긴다.
- 판단과 설명은 실제 파일, 실제 씬, 실제 명령 출력 근거를 기준으로 한다.

### Role Owner

Code Builder

### Status

Builder changes applied. 자체 코드 리뷰와 빌드 확인까지 완료했고, 사용자 플레이 검증 대기 상태다.

### Next Actions

- 사용자가 Unity에서 `MainMenuScene`을 실행해 `Touch To Start -> 런 -> 캐릭터 선택 -> RunScene 전투 진입` 흐름을 검증한다.
- 검증 중 씬 전환, 입력, 전투 초기화 문제가 있으면 그 근거를 받아 다음 Builder 수정으로 이어간다.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunStartContext.cs`를 추가해 선택 몬스터와 `RunSession`을 `DontDestroyOnLoad` 컨텍스트로 전달하게 했다.
- `Pakuri/Assets/Scripts/Run/MainMenuFlowController.cs`를 추가해 `Touch To Start`, `런`, 캐릭터 선택 단계를 같은 `MainMenuScene` Canvas 안에서 전환하게 했다.
- `Pakuri/Assets/Scripts/Run/RunSceneBootstrap.cs`를 추가해 `RunScene`에서 `RunStartContext`를 읽고 `EveVerticalSliceController.BeginConfiguredDay(...)`를 호출하게 했다.
- `Pakuri/Assets/Scripts/Run/RunSession.cs`에는 누락되어 있던 `using System;`만 정리해 `Serializable`, `StringComparison`, `Math` 사용 근거를 명시했다.
- Unity MCP 씬 작업으로 `RunScene`에서 `RunUICanvas`가 제거됐고, `RunSceneBootstrap` 루트 오브젝트가 추가됐다.
- Unity MCP 씬 작업으로 `MainMenuScene`에는 `MainMenuCanvas`와 `MainMenuFlowController`, `EventSystem`이 추가됐다.
- `Pakuri/ProjectSettings/EditorBuildSettings.asset`는 `Assets/Scenes/MainMenuScene.unity`, `Assets/Scenes/RunScene.unity` 순서로 갱신됐다.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`는 오류 0개로 통과했다. 남은 경고는 Unity/MCPForUnity 참조의 `System.Net.Http`, `System.IO.Compression` 버전 충돌 경고 2개다.
- Unity 콘솔 error 조회에서는 새 스크립트 컴파일 오류가 보이지 않았고, MCP client 종료 로그만 확인됐다.

### History

- 2026-04-26: 사용자 지시로 외부 Reviewer 호출 없이 자체 리뷰만 수행하고, 실제 플레이 검증은 사용자에게 맡기기로 확정했다.
- 2026-04-26: 현재 실제 씬 파일이 `SampleScene.unity`가 아니라 `MainMenuScene.unity`, `RunScene.unity`임을 확인했다.
- 2026-04-26: `RunScene.unity`에 `RunUICanvas`와 `RunFlowController`가 남아 있어 캐릭터 선택이 전투 씬 안에 묶여 있음을 확인했다.
- 2026-04-26: `RunStartContext`, `MainMenuFlowController`, `RunSceneBootstrap`를 추가하고 `RunScene` / `MainMenuScene` / Build Settings를 갱신했다.
- 2026-04-26: 자체 검증으로 `dotnet build`와 Unity 콘솔 확인을 수행했다.

## Task: Reviewer Wrapper Smoke Test 2026-04-25 21:40

### Task title

Smoke test after reviewer wrapper fix

### Goals

- Confirm Code Builder can inspect `AGENTS.md` and `BLACKBOARD.md`.
- Confirm no project code changes are needed for this smoke test.
- Leave loop history/evidence for the external Reviewer phase.

### Constraints

- Do not modify project files except wrapper-managed logs and `BLACKBOARD.md` loop history.
- Base claims on actual files and command output.
- External wrapper will run Code Reviewer next.

### Role Owner

Code Builder

### Status

Builder phase completed. No project code changes were needed.

### Next Actions

- External wrapper should run Code Reviewer phase.
- Code Reviewer should verify this Builder result and end with `REVIEW_RESULT: PASS` if no issue is found.

### Evidence

- 2026-04-25 21:40:30 +09:00 `Get-Location` output: `C:\TowerDefence_Pakuri\Test`.
- `AGENTS.md` was read with `Get-Content -Raw -LiteralPath AGENTS.md`.
- `BLACKBOARD.md` was read with `Get-Content -Raw -LiteralPath BLACKBOARD.md`.
- `git rev-parse --is-inside-work-tree` output: `true`.
- `git status --short` output before this entry included existing changes: `M BLACKBOARD.md`, `M codex_builder_reviewer.ps1`, `M run_codex.bat`, and untracked `codex_loop_logs/...` entries.
- Latest wrapper log directory inspection found `codex_loop_logs\20260425_213901` containing `task.txt` and `loop_01_builder.md.console.txt`.
- No Unity/project source, scene, asset, reference, or wrapper script file was modified by this Builder phase.

### History

- 2026-04-25 21:40:30 +09:00: Builder inspected required files and command outputs, determined the smoke test requires no code changes, and recorded this loop history for Reviewer verification.

## 운영 규칙

이 파일은 프롬프트 초기화, 세션 재시작, 재부팅 후에도 작업을 이어가기 위한 지속 상태 파일이다.

새 작업이 시작되면 관련 작업 블록을 먼저 읽고 이어서 작업한다. 작업 블록은 작업이 완료되었거나 사용자가 명시적으로 삭제를 요청했을 때만 제거한다.

각 작업 블록에는 최소한 다음 항목을 유지한다.
- Task title
- Goals
- Constraints
- Role Owner
- Status
- Next Actions
- Evidence
- History

별도 저장소가 더 효율적이라고 판단되면 바로 바꾸지 말고 대안, 트레이드오프, 판단 기준을 먼저 보고한다.

## Task: Codex CLI Bootstrap

### Task title

Codex CLI 부트스트랩 및 Builder -> Reviewer 외부 강제 흐름 구성

### Goals

- `run_codex.bat`가 파일 위치를 루트로 잡고 UTF-8 콘솔에서 Codex CLI를 시작하게 한다.
- `codex_prompt.txt`를 UTF-8로 읽어 시작 프롬프트로 전달하게 한다.
- `AGENTS.md`에 근거 기반 작업 규칙과 Designer, Code Builder, Code Reviewer 롤을 정의한다.
- Builder 단계 직후 Reviewer 단계가 자동 실행되는 실제 외부 강제 흐름을 제공한다.
- 프롬프트 초기화나 재부팅 뒤에도 작업 상태를 이어갈 수 있게 한다.

### Constraints

- 모든 설명과 작업 판단은 실제 파일, 코드, 명령 출력 근거를 기준으로 한다.
- 구현되지 않은 것을 구현된 것처럼 말하지 않는다.
- 저장소에 없는 파일이나 구조는 먼저 확인하고, 없으면 없다고 말한다.
- `bat`, `txt`, `md` 파일은 UTF-8로 저장한다.
- Codex CLI 기본 실행 경로는 `%APPDATA%\npm\codex.cmd`다.
- Builder -> Reviewer 루프는 최대 3회만 허용한다.
- Git 저장소가 아닐 수 있으므로 Git 의존 흐름을 기본 전제로 삼지 않는다.

### Role Owner

Code Builder

### Status

Completed for bootstrap file creation, path correction, and Codex CLI path resolver hardening. No downstream Builder task has been run through the loop yet.

### Next Actions

- 일반 대화형 시작은 `run_codex.bat`를 실행한다.
- Builder -> Reviewer 강제 루프가 필요한 작업은 `powershell -NoProfile -ExecutionPolicy Bypass -File .\codex_builder_reviewer.ps1 -Task "작업 내용"` 형식으로 실행한다.
- 실제 Builder 작업을 래퍼로 실행하면 `codex_loop_logs`와 `BLACKBOARD.md`의 loop 기록을 확인한다.

### Evidence

- `Get-Location` 출력: `C:\TowerDefence_Pakuri\Test`
- 최초 `Get-ChildItem -Force` 출력에는 `.git`, `.gitignore`만 있었다.
- `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`는 최초 확인 시 존재하지 않았다.
- `Get-Command codex` 출력의 실제 경로: `c:\Users\t3312\.vscode\extensions\openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`
- `codex --version` 출력: `codex-cli 0.122.0-alpha.1`
- `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')` 출력: `False`
- `Join-Path $env:APPDATA 'npm\codex.cmd'` 출력: `C:\Users\t3312\AppData\Roaming\npm\codex.cmd`
- `codex --help` 출력에는 `exec`, `review`, `login`, `logout`, `mcp`, `marketplace`, `mcp-server`, `app-server`, `completion`, `sandbox`, `debug`, `apply`, `resume`, `fork`, `cloud`, `exec-server`, `features`, `help` 명령이 있었다.
- `codex --help`, `codex review --help`, `codex exec --help`, `codex debug --help`, `codex mcp --help` 출력에서 Claude Hooks와 같은 hook/event 명령은 확인되지 않았다.
- `codex review --help` 출력에는 `--uncommitted`, `--base`, `--commit` 옵션이 있었다.
- `codex exec --help` 출력에는 `--skip-git-repo-check`, `-C`, `--full-auto`, `-o` 옵션이 있었다.
- `git rev-parse --is-inside-work-tree` 출력: `true`
- 승인 후 `%APPDATA%\npm\codex.cmd` 래퍼를 생성했다.
- 승인된 검증에서 `Test-Path (Join-Path $env:APPDATA 'npm\codex.cmd')` 출력: `True`
- 승인된 검증에서 `%APPDATA%\npm\codex.cmd` 내용은 감지된 `codex.exe`를 호출했다.
- 승인된 검증에서 `& (Join-Path $env:APPDATA 'npm\codex.cmd') --version` 출력: `codex-cli 0.122.0-alpha.1`
- `cmd /d /c "call run_codex.bat < NUL"`은 `codex.cmd` 생성 전 오류 경로를 검증했고, `Required default path: C:\Users\t3312\AppData\Roaming\npm\codex.cmd`를 출력했다.
- `codex_builder_reviewer.ps1`는 PowerShell syntax check를 통과했다.
- 2026-04-23 `C:\Users\t3312\AppData\Roaming\npm\codex.cmd` 내용은 삭제된 VS Code 확장 경로 `openai.chatgpt-26.415.20818-win32-x64\bin\windows-x86_64\codex.exe`를 가리키고 있었다.
- 2026-04-23 실제 존재하는 Codex CLI 경로는 `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`였고 `codex-cli 0.122.0-alpha.13`을 출력했다.
- 2026-04-23 `run_codex.bat`는 `%APPDATA%\npm\codex.cmd`가 실행 가능하지 않으면 VS Code 확장 폴더의 최신 `codex.exe`를 탐색하도록 수정했다.
- 2026-04-23 `codex_builder_reviewer.ps1`도 동일하게 Codex CLI 경로를 해석하도록 `Resolve-CodexCommand`를 추가했다.
- 2026-04-23 수정 후 Codex CLI 경로 탐색은 `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe`를 찾았고 `codex-cli 0.122.0-alpha.13`을 출력했다.
- 2026-04-23 승인 후 `%APPDATA%\npm\codex.cmd` 래퍼를 현재 존재하는 `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe` 경로로 갱신했고 `codex-cli 0.122.0-alpha.13`을 출력했다.
- 2026-04-23 수정 후 `codex_builder_reviewer.ps1`는 PowerShell parser syntax check를 통과했다.
- 2026-04-23 Code Reviewer 외부 검토 로그 `codex_loop_logs\manual_reviewer_20260423_212033.md`는 `REVIEW_RESULT: PASS`를 반환했다.
- 2026-04-25 sandbox 내부 직접 `codex exec` smoke test는 `액세스가 거부되었습니다. (os error 5)`로 실패했다.
- 2026-04-25 승인된 외부 실행으로 최신 Codex CLI `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.422.30944-win32-x64\bin\windows-x86_64\codex.exe` reviewer smoke test가 `REVIEW_RESULT: PASS`를 반환했다.
- 2026-04-25 `codex_builder_reviewer.ps1`의 `Invoke-CodexExec`가 Codex 콘솔 출력을 반환값으로 섞어 `$builderExit`를 문자열로 만드는 문제를 확인했다.
- 2026-04-25 `Invoke-CodexExec`가 콘솔 출력을 `*.console.txt`로 저장하고 정수 종료 코드만 반환하도록 수정했다.
- 2026-04-25 Codex CLI stderr 배너가 `$ErrorActionPreference = 'Stop'`에서 `NativeCommandError`를 일으켜, `Invoke-CodexExec` 내부에서만 native stderr 처리를 `Continue`로 완화했다.
- 2026-04-25 수정 후 `codex_builder_reviewer.ps1`는 PowerShell parser syntax check에서 `PARSE_OK`를 반환했다.
- 2026-04-25 수정 후 smoke test 래퍼 실행은 `Reviewer PASS at loop 1.`을 반환했고, `codex_loop_logs\20260425_213006\loop_01_reviewer.md`는 `REVIEW_RESULT: PASS`를 포함한다.
- 2026-04-25 Code Reviewer 직접 검토 `codex_loop_logs\reviewer_restore_fix_review.md`는 `run_codex.bat`의 프롬프트 quote 변형, `BLACKBOARD.md`의 잘못된 history 위치, pre-fix 손상 exit code 기록을 지적하며 `REVIEW_RESULT: NEEDS_CHANGES`를 반환했다.
- 2026-04-25 `run_codex.bat`는 `codex_prompt.txt` UTF-8 내용을 변형 없이 전달하도록 `.Replace([string][char]34, [string][char]0x201D)`를 제거했다.
- 2026-04-25 `Add-BlackboardHistory`는 루프 기록을 파일 끝이 아니라 `Codex CLI Bootstrap` 작업의 `Builder Reviewer Loop` 섹션 앞에 삽입하도록 수정했다.
- 2026-04-25 잘못 붙었던 Eve 작업 하단의 wrapper smoke-test history 기록을 제거했다.
- 2026-04-25 최종 smoke test 래퍼 실행은 `Reviewer PASS at loop 1.`을 반환했고, `codex_loop_logs\20260425_213901\loop_01_reviewer.md`는 `REVIEW_RESULT: PASS`를 포함한다.

### History

- 2026-04-19: 작업 폴더와 대상 파일 존재 여부를 확인했다.
- 2026-04-19: Codex CLI 실제 경로, 버전, `exec`, `review` 도움말을 확인했다.
- 2026-04-19: `%APPDATA%\npm\codex.cmd`가 현재 존재하지 않는다는 점을 확인했다.
- 2026-04-19: 네이티브 hook/event가 도움말 출력에서 확인되지 않아 외부 PowerShell 래퍼 방식으로 설계했다.
- 2026-04-19: `run_codex.bat`, `codex_prompt.txt`, `AGENTS.md`, `BLACKBOARD.md`, `codex_builder_reviewer.ps1`를 생성했다.
- 2026-04-19: 승인 후 `%APPDATA%\npm\codex.cmd` 래퍼를 생성하고 `--version` 실행으로 검증했다.
- 2026-04-23: VS Code 확장 업데이트로 `%APPDATA%\npm\codex.cmd`가 가리키는 고정 버전 경로가 깨진 문제를 확인했다.
- 2026-04-23: `run_codex.bat`와 `codex_builder_reviewer.ps1`를 고정 래퍼 의존에서 실행 가능한 래퍼 우선, 실패 시 최신 VS Code 확장 `codex.exe` 탐색 방식으로 수정했다.
- 2026-04-23: 승인 후 `%APPDATA%\npm\codex.cmd` 외부 래퍼 자체도 현재 존재하는 Codex CLI 실행 파일로 갱신했다.
- 2026-04-23: `codex_loop_logs\manual_reviewer_20260423_212033.md`에 Code Reviewer 통과 판정을 기록했다.
- 2026-04-25: Code Reviewer 강제 흐름 중단 원인이 Codex CLI 실행 실패와 래퍼의 종료 코드 반환 처리 오류임을 확인하고 `codex_builder_reviewer.ps1`를 수정했다.
- 2026-04-25: 수정 후 Builder -> Reviewer smoke test를 실행해 `codex_loop_logs\20260425_213006\loop_01_reviewer.md`에서 `REVIEW_RESULT: PASS`를 확인했다.
- 2026-04-25: Code Reviewer가 지적한 `run_codex.bat` 프롬프트 변형과 `BLACKBOARD.md` 기록 위치 문제를 수정한 뒤 `codex_loop_logs\20260425_213901\loop_01_reviewer.md`에서 `REVIEW_RESULT: PASS`를 확인했다.

- 2026-04-25 21:39:01 +09:00: Builder -> Reviewer loop started. Run directory: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901
- 2026-04-25 21:39:27 +09:00: Loop 1 Builder started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_builder.md
- 2026-04-25 21:41:53 +09:00: Loop 1 Builder finished with exit code 0.
- 2026-04-25 21:42:22 +09:00: Loop 1 Reviewer started. Output: C:\TowerDefence_Pakuri\Test\codex_loop_logs\20260425_213901\loop_01_reviewer.md
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer finished with exit code 0.
- 2026-04-25 21:44:07 +09:00: Loop 1 Reviewer decision: PASS. Builder -> Reviewer loop completed.
### Builder Reviewer Loop

- Enforcement method: External wrapper script
- Wrapper file: `codex_builder_reviewer.ps1`
- Git dependency: Not required
- Max loops: 3
- Current loop count: 1 in latest smoke test
- Last reviewer decision: PASS for wrapper log `codex_loop_logs\20260425_213901\loop_01_reviewer.md`
- Last log directory: `codex_loop_logs\20260425_213901`

## Task: Unity MCP Bridge Connection

### Task title

Unity MCP bridge 연결 및 등록 확인

### Goals

- 현재 워크스페이스의 Unity 프로젝트 `Pakuri`에서 Unity MCP bridge를 Codex MCP 서버와 연결한다.
- Codex CLI 쪽 MCP 등록 상태와 Unity Editor 쪽 bridge 실행 상태를 실제 명령 출력으로 구분한다.
- 사용자가 Unity Editor 내 MCP For Unity 설정을 직접 조작해야 하는 경우, 필요한 항목을 명확히 질문한다.

### Constraints

- 모든 판단은 실제 파일, 패키지 코드, 명령 출력에 근거한다.
- Unity 프로젝트 파일은 사용자 요청 없이 수정하지 않는다.
- Unity Editor 내부 bridge 시작은 실제 연결 확인 전까지 완료된 것으로 말하지 않는다.

### Role Owner

Code Builder

### Status

Completed. Unity Editor-side MCP For Unity bridge is connected to the current Codex MCP server.

### Next Actions

- 이후 Unity MCP가 끊기면 Unity Editor에서 Transport를 `Stdio`로 두고 `Session Active`를 다시 켠 뒤 `manage_scene get_active`로 재검증한다.
- Unity Test Runner 확인은 `run_tests EditMode` 후 `get_test_job`으로 결과를 확인한다.

### Evidence

- `Pakuri/ProjectSettings/ProjectVersion.txt` 출력: `m_EditorVersion: 6000.3.4f1`
- 2026-04-25 재확인 `Pakuri/ProjectSettings/ProjectVersion.txt` 출력: `m_EditorVersion: 6000.3.14f1`
- 2026-04-25 재확인 `Pakuri/ProjectSettings/ProjectVersion.txt` 출력: `m_EditorVersionWithRevision: 6000.3.14f1 (d68c3f99a318)`
- `Pakuri/Packages/manifest.json`에는 `com.coplaydev.unity-mcp` 의존성이 있다.
- `codex mcp get unityMCP` 출력: `enabled: true`, `transport: stdio`, `command: uvx`, `args: --from mcpforunityserver mcp-for-unity --transport stdio`
- Unity MCP 서버 `debug_request_context` 출력: server version `9.6.6`, `active_instance: null`, `all_keys_in_store: []`
- `manage_scene get_active` 출력: `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`
- `%USERPROFILE%\.unity-mcp` status directory는 존재하지 않았다.
- `Test-NetConnection 127.0.0.1:6400`은 TCP 연결 실패로 timeout 됐다.
- `StdioBridgeHost.cs`에는 `[InitializeOnLoad]`, `StartAutoConnect()`, `WriteHeartbeat()`, `%USERPROFILE%\.unity-mcp\unity-mcp-status-<hash>.json` 작성 코드가 있다.
- `McpCiBoot.cs`는 `EditorPrefs.SetBool(EditorPrefKeys.UseHttpTransport, false)` 후 `StdioBridgeHost.StartAutoConnect()`를 호출한다.
- `README.md` Quick start는 `Window > MCP for Unity`, `Auto-Setup`, 필요 시 `Start Bridge`를 안내한다.
- 사용자 조작 후 `%USERPROFILE%\.unity-mcp\unity-mcp-status-c88ab184.json`이 생성됐고 내용은 `unity_port: 6400`, `reason: ready`, `project_name: Pakuri`, `unity_version: 6000.3.4f1`였다.
- 사용자 조작 후 Unity MCP 서버 `debug_request_context` 출력은 `active_instance: Pakuri@c88ab184`였다.
- 사용자 조작 후 `manage_scene get_active` 출력은 `SampleScene`, `Assets/Scenes/SampleScene.unity`, `rootCount: 2`였다.
- `read_console` 출력에는 `Transport changed to: Stdio`, `StdioBridgeHost started on port 6400. (OS=WindowsEditor, server=9.6.6)`, `SkillSync complete: Added: 3, Updated: 0, Deleted: 0 (C:\Users\t3312\.codex\skills\unity-mcp-skill)`가 있었다.
- `manage_asset search`는 `Assets`에서 총 11개 에셋을 찾았다.
- `manage_scene get_hierarchy`는 루트 오브젝트 `Main Camera`, `Global Light 2D`를 반환했다.
- `run_tests EditMode`는 job `bee66234eeec4e67b238bafff3d63dc9`를 시작했고 `get_test_job` 결과는 `status: succeeded`, `resultState: Passed`, `total: 0`, `passed: 0`, `failed: 0`, `skipped: 0`였다.
- 2026-04-25 재확인 Unity MCP 서버 `debug_request_context` 출력은 `active_instance: Pakuri@0c8eeeb5`였다.

### History

- 2026-04-23: Unity 프로젝트 구조, MCP 패키지 설치, Codex CLI MCP 등록 상태를 확인했다.
- 2026-04-23: Unity MCP 서버는 실행 중이나 Unity Editor bridge 인스턴스가 등록되지 않았음을 확인했다.
- 2026-04-23: Unity Editor 내부 MCP For Unity 설정/bridge 시작이 필요하다고 판단했다.
- 2026-04-23: 사용자가 Unity Editor에서 Transport를 `Stdio`로 바꾸고 `Session Active`, Codex client `Configuration`을 수행했다.
- 2026-04-23: Unity MCP bridge 연결, scene/asset/console/hierarchy 접근, EditMode Test Runner 실행을 검증했다.
- 2026-04-25: 사용자 안내 후 `Pakuri/ProjectSettings/ProjectVersion.txt`를 다시 확인해 Unity 버전이 `6000.3.14f1`로 올라간 것을 기록했고, `debug_request_context`로 현재 MCP 활성 인스턴스가 `Pakuri@0c8eeeb5`인 점을 재확인했다.

## Task: Combat Automation Responsibility Guide

### Task title

기초 전투 시스템 구현 시 자동화 가능 범위와 사용자 수동 작업 범위 정리 HTML 작성

### Goals

- `reference/current-architecture-plan.html` 기준으로 기초 전투 시스템 구현 착수 시 역할 분담을 정리한다.
- 현재 Unity 프로젝트 구조와 MCP 연결 상태를 근거로 폴더 생성, 스크립트 생성, 씬 배치 자동화 가능 범위를 구분한다.
- 사용자가 직접 해야 하는 작업과 제가 자동으로 할 수 있는 작업을 HTML 문서 한 장으로 정리한다.

### Constraints

- 실제 파일, 실제 씬 상태, 실제 MCP 호출 결과에 근거해 정리한다.
- 구현되지 않은 자동화 능력을 구현된 것처럼 적지 않는다.
- 이 작업은 설계 문서 작성이며, 전투 시스템 코드 구현 자체는 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 문서를 기준으로 Designer handoff를 작성한다.
- 사용자가 명시적으로 구현을 지시하면 Code Builder 단계로 전환해 폴더, 스크립트, 씬 오브젝트 생성을 실제로 수행한다.

### Evidence

- `Pakuri/reference/current-architecture-plan.html` 파일이 존재하며 전투 시스템 시작 구조를 설명한다.
- `manage_asset search` 결과 `Assets`에는 `Scenes`, `Settings`와 기본 URP/InputSystem 자산만 있고 `Assets/Scripts` 폴더는 없다.
- `Get-ChildItem Pakuri\\Assets` 출력에도 `Scenes`, `Settings` 외 게임 전용 폴더가 없다.
- `manage_scene get_hierarchy` 결과 현재 `SampleScene` 루트 오브젝트는 `Main Camera`, `Global Light 2D`뿐이다.
- Unity MCP `debug_request_context` 결과 활성 인스턴스는 `Pakuri@c88ab184`다.
- 같은 세션에서 `manage_scene get_active`, `manage_scene get_hierarchy`, `run_tests EditMode`가 성공해 현재 자동화 연결이 살아 있음을 확인했다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `reference/current-architecture-plan.html`를 다시 읽었다.
- 2026-04-24: `manage_asset search`, `Get-ChildItem Pakuri\\Assets`, `manage_scene get_hierarchy`로 현재 프로젝트 구조와 씬 상태를 재확인했다.
- 2026-04-24: 자동화 가능 범위와 사용자 수동 작업 범위를 정리한 HTML 문서를 `Pakuri/reference`에 추가했다.

## Task: Eve Initial Combat Preview

### Task title

`dungeon-squad-run-structure.md` 기준 이브 단독 초기 전투 완성 모습 HTML 작성

### Goals

- `reference/4.run/dungeon-squad-run-structure.md`를 기준으로 초기 전투 로직을 어떻게 이해했는지 시각적으로 검증 가능한 HTML 문서를 만든다.
- 앞서 제안한 vertical slice 방향을 유지한 채, 이브만 구현했을 때의 초기 완성 상태를 정리한다.
- 문서 기반 확정 사항과 초기 구현용 제안을 분리해서 표시한다.

### Constraints

- 실제 reference 문서에 있는 내용만 확정으로 적고, 제안은 제안으로 명확히 구분한다.
- 현재 Unity 프로젝트와 씬 상태를 근거로 “아직 없는 것”과 “구현 후 기대 모습”을 구분한다.
- 이 작업은 설계 검증용 HTML 작성이며, 전투 시스템 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 확인 후 방향이 맞다고 판단하면, Designer handoff 문서로 구체적인 구현 순서를 내릴 수 있다.
- 사용자가 명시적으로 구현을 지시하면 Code Builder가 이 HTML의 구조를 기준으로 실제 폴더, 스크립트, 씬 오브젝트를 생성한다.

### Evidence

- `Pakuri/reference/4.run/dungeon-squad-run-structure.md`는 1일차 고정 전투, 전투 후 보상 확인, 포로 기반 선택, 다음 일차 이동 흐름을 정의한다.
- `Pakuri/reference/2.Monster/eve/eve-tower.md`는 이브를 번개/얼음 엔진형 보조 딜러로 정의하고, 첫 액티브로 `A. 아크 볼트`를 둔다.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`는 아크 볼트의 탄창 수 6, 재장전 4초, 발사 간격 0.35초, 번개 피해 계산식 `24 + 주문력 * 0.95`, 감전 15%를 정의한다.
- `Pakuri/reference/Scene/combat-scene-layout.md`는 테스트 전장 32x18, 넥서스 `(2,8)`, 적 우측 진입, 아군 배치 영역 `(4~10, 3~15)`를 정의한다.
- `Pakuri/reference/dungeon-squad-combat-player-controls.md`는 전투 중 플레이어 조작을 “공격 지점 지정”으로 정의한다.
- `Pakuri/reference/4.run/combat-reward-system.md`는 일반 전투 보상으로 포로 1~3명, 골드 10, 어둠의 흔적 10, 보스 포로 확정 포함을 정의한다.
- `Pakuri/reference/5.enemy/stage-1-enemies.md`는 1스테이지 일반몹 5종과 일반 전투 보스 강화 규칙을 정의한다.
- 현재 `manage_scene get_active` 결과는 `Assets/Scenes/SampleScene.unity`이며, `manage_scene get_hierarchy` 결과 씬 루트는 `Main Camera`, `Global Light 2D`뿐이다.
- 현재 `manage_asset search` 결과 `Assets`에는 기본 `Scenes`, `Settings`, URP/InputSystem 자산만 있고 게임 전용 스크립트 폴더는 없다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-run-structure.md`, `eve-tower.md`, `current-architecture-plan.html`를 다시 읽었다.
- 2026-04-24: `a-arc-bolt.md`, `combat-scene-layout.md`, `combat-reward-system.md`, `dungeon-squad-combat-player-controls.md`, `combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `stage-1-enemies.md`를 추가로 읽었다.
- 2026-04-24: 현재 Unity 씬과 에셋 상태를 다시 조회한 뒤, 이브 단독 초기 전투 완성 모습을 설명하는 HTML 문서를 `Pakuri/reference`에 추가했다.

## Task: Eve Combat Vertical Slice Implementation

### Task title

이브 단독 초기 전투 vertical slice 실제 구현 및 작업 설명 HTML 작성

### Goals

- `eve-initial-combat-vertical-slice-preview.html` 기반으로 Unity 프로젝트에 실제 전투 프로토타입을 만든다.
- 현재 씬의 메인 카메라를 전장 기준으로 맞추고 `CombatRoot` 및 앵커 오브젝트를 생성한다.
- 적 스폰 X는 고정하고 Y는 랜덤으로 생성되게 한다.
- 구현 후 실제 검증 근거와 작업 설명을 HTML로 남긴다.

### Constraints

- 실제 reference 문서와 실제 Unity 씬 상태를 기준으로 구현한다.
- 현재 프로젝트에 없는 아트 자산은 추측하지 않고 placeholder 비주얼로 처리한다.
- 로직 작업 후 reviewer 검수를 시도하고, 외부 reviewer 실행이 실패하면 그 실패 근거를 남긴다.

### Role Owner

Code Builder

### Status

Completed with manual reviewer pass in-session. External Codex reviewer commands timed out and did not produce a new review artifact.

### Next Actions

- 사용자가 원하면 이 프로토타입 위에 실제 아트 자산, 정식 UI, 추가 적 타입, 보상 데이터 구조를 붙인다.
- reviewer 외부 강제 흐름을 이 작업에도 안정적으로 연결하려면 `codex review`/`codex exec` 타임아웃 원인을 별도 확인한다.

### Evidence

- `Assets/Scripts/Combat/DamageCalculator.cs`를 생성했다.
- `Assets/Scripts/Combat/EveVerticalSliceController.cs`를 생성했다.
- `manage_asset search path=Assets/Scripts` 결과 `Combat`, `DamageCalculator.cs`, `EveVerticalSliceController.cs`가 존재한다.
- `SampleScene.unity`에는 `CombatRoot`와 `Pakuri.Combat.EveVerticalSliceController` 컴포넌트가 저장됐다.
- `manage_scene get_hierarchy include_transform=true` 결과:
  - `Main Camera` 위치 `15.5, 8.5, -10`
  - `Nexus` 위치 `2, 8, 0`
  - `EveUnit` 위치 `6, 8, 0`
  - `EnemySpawnPoint` 위치 `29, 8, 0`
  - `InputTarget` 위치 `16, 8, 0`
- `SampleScene.unity` 텍스트 확인 결과 `orthographic: 1`, `orthographic size: 10`, `CombatRoot`, `EveVerticalSliceController`, 각 좌표가 저장되어 있다.
- 플레이 모드 런타임 검사 `execute_code` 결과:
  - 적 스폰 런타임 오브젝트 `Enemy_Normal_01`, `Enemy_Boss_01`가 생성됐다.
  - 이후 `battleResolved=True`, `victory=True`, `waitingForRewardChoice=True` 상태를 확인했다.
- 게임 화면 캡처 파일:
  - `Assets/Screenshots/screenshot-20260424-165841.png`
  - `Assets/Screenshots/screenshot-20260424-165958.png`
- `validate_script`는 `DamageCalculator.cs`에 대해 성공했고, `EveVerticalSliceController.cs`는 실제 파일 내용 중복이 없는데도 duplicate signature 오탐을 반환했다.
- `codex review --uncommitted`는 실행 경로 문제 후 실제 실행에서 timeout 됐다.
- reviewer 전용 `codex exec`도 300초 timeout으로 끝났고 새 review 로그 파일을 남기지 못했다.
- 현재 세션에서 `DamageCalculator.cs`, `EveVerticalSliceController.cs`, `SampleScene.unity`를 line-by-line 확인했고 추가 blocking issue는 찾지 못했다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `eve-initial-combat-vertical-slice-preview.html`, 관련 reference 문서를 다시 읽었다.
- 2026-04-24: `Assets/Scripts`, `Assets/Scripts/Combat` 폴더를 생성했다.
- 2026-04-24: `DamageCalculator.cs`, `EveVerticalSliceController.cs`를 추가했다.
- 2026-04-24: `CombatRoot`를 만들고 `EveVerticalSliceController`를 붙였다.
- 2026-04-24: `Main Camera`를 전장 기준 위치와 orthographic 설정으로 맞췄다.
- 2026-04-24: `ExecuteAlways` 기반으로 `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`가 씬에 생성되도록 했다.
- 2026-04-24: 플레이 모드에서 적 스폰, 승리 상태, 보상 대기 상태를 확인했다.
- 2026-04-24: 외부 reviewer로 `codex review --uncommitted`, reviewer 전용 `codex exec`를 시도했으나 모두 timeout 됐다.
- 2026-04-24: 현재 세션에서 manual reviewer 검토를 수행하고 작업 설명 HTML을 추가했다.

## Task: Eve Projectile Click Hold Compliance Plan

### Task title

문서 준수형 아크 볼트 투사체 입력/적중 구조 수정 계획 HTML 작성

### Goals

- 현재 이브 전투 프로토타입을 기준으로, 아크 볼트를 문서 정의에 더 맞는 `투사체 / 탄창형` 구조로 바꾸는 작업 계획을 정리한다.
- 사용자가 요청한 `왼쪽 클릭 유지 시 연속 발사`, `투사체 적중 시 피해` 요구를 실제 코드와 reference 문서 차이 기준으로 설명한다.
- Code Builder가 바로 구현에 들어갈 수 있도록 수정 범위, 파일별 변경 계획, 검증 체크리스트를 HTML 한 장으로 남긴다.

### Constraints

- 실제 reference 문서와 실제 현재 코드에 근거해서만 적는다.
- 아직 없는 구현을 구현된 것처럼 적지 않는다.
- 이 작업은 설계 문서 작성이며, 코드 수정 자체는 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 문서를 기준으로 Code Builder 단계로 전환해 실제 투사체형 발사 로직을 구현한다.
- 구현 시 `EveVerticalSliceController.cs`의 즉시 피해 구조를 투사체 적중 구조로 바꾸고, hold 입력 검증과 reviewer 루프를 다시 수행한다.

### Evidence

- `Pakuri/reference/dungeon-squad-combat-player-controls.md`는 전투 중 플레이어 입력을 `공격 지점 지정`으로 정의한다.
- `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`는 아크 볼트를 `투사체 / 탄창형`으로 정의하고, 투사체 속도 `15.0`, 탄창 `6`, 재장전 `4초`, 발사 간격 `0.35초`, 감전 `15%`를 명시한다.
- `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`는 같은 속성 방어력 참조와 방어력 반영 후 치명타 적용 규칙을 정의한다.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` 현재 구현은 `wasPressedThisFrame` / `GetMouseButtonDown(0)` 입력과 즉시 피해 구조를 사용한다.
- 새 설계 문서 `Pakuri/reference/eve-projectile-click-hold-plan.html`를 추가했다.

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-combat-player-controls.md`, `a-arc-bolt.md`, `combat-attribute-and-damage-system.md`, `EveVerticalSliceController.cs`, `eve-combat-implementation-report.html`를 다시 읽었다.
- 2026-04-24: 현재 코드가 단발 클릭 입력과 즉시 피해 구조임을 확인했다.
- 2026-04-24: hold 입력 기반 연속 발사와 투사체 적중 기반 피해 처리로 옮기는 설계 HTML을 `Pakuri/reference/eve-projectile-click-hold-plan.html`에 추가했다.

## Task: Eve Projectile Click Implementation

### Task title

이브 아크 볼트를 클릭형 투사체 적중 구조로 수정하고 완료 보고 HTML 작성

### Goals

- 기존 즉시 피해 구조를 제거하고, 왼쪽 클릭 시에만 아크 볼트 투사체 1발이 생성되게 한다.
- 투사체가 실제로 이동하고 적과 닿을 때만 피해를 적용하게 한다.
- 수정 후 객체 역할, 동작 방식, 작업 중 문제, 타임스탬프 작업 로그를 포함한 완료 보고 HTML을 남긴다.

### Constraints

- 실제 현재 코드와 실제 Unity 런타임 검증을 근거로 작업한다.
- 적 스폰 축, 카메라, 전장 좌표는 기존 값을 유지한다.
- 로직 수정 후 reviewer 강제 흐름을 다시 시도하고, 실패 시 그 근거를 남긴다.

### Role Owner

Code Builder

### Status

Completed without Code Review. External reviewer commands timed out again, so only Builder-side validation was performed.

### Next Actions

- 사용자가 원하면 다음 단계로 실제 클릭 입력 기반 정식 플레이 테스트, 속성별 방어력 데이터 모델, Collider 기반 충돌로 확장한다.
- reviewer 외부 강제 흐름 timeout 원인을 별도 분리해서 해결해야 한다.
- 현재 상태는 Code Review 미수행 상태이므로, 이후 리뷰가 필요하면 별도 reviewer 단계를 다시 실행해야 한다.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`는 `ProjectileRuntime`, `projectileRoot`, `UpdateProjectiles()`, `TryHitEnemy()`, 클릭 기반 `HandlePointerInput()`를 포함하도록 수정됐다.
- `Pakuri/Assets/Scenes/SampleScene.unity`는 `ProjectileRoot`를 포함한 현재 전장 구조로 다시 저장됐다.
- `manage_scene save`가 `Assets/Scenes/SampleScene.unity` 저장 성공을 반환했다.
- `find_gameobjects by_name ProjectileRoot`는 씬에서 `ProjectileRoot`를 찾았다.
- 플레이 모드 통제 검증에서:
  - 발사 직후 `projectileCount = 1`
  - 1초 뒤 `projectileCount = 0`
  - 같은 검증에서 `enemyHealth = 37.95`
  - 최종 재검증에서 `currentShotsRemaining = 0`, `reloadRemaining = 4.0`
- 검증 캡처 `Pakuri/Assets/Screenshots/eve-projectile-click-runtime.png`를 생성했다.
- `validate_script`는 이번에도 duplicate signature false positive를 냈다.
- `read_console`에서는 `FindObjectOfType<Camera>()` obsolete warning이 나왔고 이후 `FindFirstObjectByType<Camera>()`로 수정했다.
- 외부 reviewer 시도:
  - `codex review --uncommitted` timeout
  - reviewer 전용 `codex exec` timeout

### History

- 2026-04-24: `AGENTS.md`, `BLACKBOARD.md`, `eve-projectile-click-hold-plan.html`, `a-arc-bolt.md`, `dungeon-squad-combat-player-controls.md`, 현재 `EveVerticalSliceController.cs`를 다시 읽었다.
- 2026-04-24: 즉시 피해 구조를 제거하고 클릭형 투사체 생성/이동/적중 구조로 `EveVerticalSliceController.cs`를 교체했다.
- 2026-04-24: `ProjectileRoot` 생성과 hierarchy 반영을 확인했다.
- 2026-04-24: 플레이 모드 통제 검증으로 투사체 적중 시 피해 적용을 확인했다.
- 2026-04-24: 수동 line review에서 마지막 탄 이후 자동 재장전 지연 문제를 찾아 `FireArcBolt()`에서 즉시 재장전 시작으로 수정했다.
- 2026-04-24: obsolete camera 탐색 경고를 `FindFirstObjectByType<Camera>()`로 수정했다.
- 2026-04-24: 작업 완료 보고서 `Pakuri/reference/eve-projectile-click-implementation-report.html`를 추가했다.
- 2026-04-24: 외부 reviewer로 `codex review --uncommitted`, reviewer 전용 `codex exec`를 다시 시도했으나 모두 timeout 됐다.

## Task: Monster Select Run UI Expansion Plan

### Task title

몬스터 선택 UI, Run 시작, 전투 후 스킬 강화 흐름 확장 설계 HTML 작성

### Goals

- 현재 구현된 이브 단독 전투 프로토타입을 기준으로, 몬스터 선택 UI와 Run 시작 흐름을 어떻게 일반화할지 정리한다.
- `2.Monster` 문서군과 `skill-choice-pool-rule.md`, `combat-reward-system.md`를 근거로 몬스터별 시작 스킬 A, 최대 액티브 3개, 최대 패시브 3개, 전투 후 강화 선택 흐름을 설계한다.
- 구현 전에 필요한 공통 시스템, UI 패널 구조, 열린 질문을 HTML 문서로 남긴다.

### Constraints

- 실제 현재 코드, 실제 씬 상태, 실제 reference 문서에 근거해서만 적는다.
- 구현되지 않은 UI/런 시스템을 이미 있는 것처럼 적지 않는다.
- 이 작업은 Designer 설계 문서 작성이며, 실제 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 설계 문서를 기준으로 Designer handoff를 작성해 Code Builder 구현 범위를 고정한다.
- 사용자가 명시적으로 구현을 지시하면, 먼저 UI 뼈대와 RunSession 분리부터 들어가는 것이 안전하다.
- 1차 구현 범위는 문서가 완비된 `아리엘`, `이브`, `세인`, `베가` 4몬스터 우선으로 잡고, `린`은 더미 상태로 둔다.
- 린의 `g~j` 패시브 문서가 실제 저장소에 없으므로, 린을 플레이 가능 대상으로 올리는 작업은 후속 문서 보강 이후로 미룬다.

### Evidence

- `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`만 현재 게임 전용 스크립트로 존재한다.
- 현재 활성 씬은 `Assets/Scenes/SampleScene.unity`이며 루트 오브젝트는 `Main Camera`, `Global Light 2D`, `CombatRoot`다.
- `CombatRoot` 하위에는 `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`가 있다.
- `Pakuri/Assets` 아래에서는 `NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`가 확인돼 별도 UI 자산이 없음을 재확인했다.
- `Pakuri/reference/2.Monster/monster-basic-rule.md`는 몬스터가 액티브 A를 기본 습득 상태로 시작하고, 런 중 액티브 최대 3개, 패시브 최대 3개를 가진다고 정의한다.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`는 신규 액티브, 신규 패시브, 액티브 특성, 마스터 스킬을 하나의 선택지 풀로 합쳐 3개를 제시하는 규칙을 정의한다.
- `Pakuri/reference/4.run/combat-reward-system.md`는 일반 전투/중간보스/보스 전투별 포로, 유물, 골드, 어둠의 흔적 보상 규칙을 정의한다.
- `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`로 현재 구현 대상 몬스터 5종을 확인했다.
- 사용자 응답으로 모든 몬스터는 패시브 슬롯 `F~J` 총 5개를 가지며, 런 중 실제로 선택 가능한 패시브는 최대 3개라는 설계 기준을 확정했다.
- 사용자 응답으로 이번 범위의 포로 보상은 `표시만 하는 정보`로 처리하고, 영입 시스템은 나중에 붙이기로 확정했다.
- 사용자 응답으로 1차 구현은 문서가 완비된 4몬스터(`아리엘`, `이브`, `세인`, `베가`)부터 진행하고, `린`은 더미 상태로 두기로 확정했다.
- 실제 저장소 확인 결과 아리엘, 이브, 세인, 베가는 `f~j` 패시브 문서가 모두 존재하지만, 린은 `f-ambidextrous.md`만 있고 `g~j` 패시브 문서는 아직 없다.
- 새 설계 문서 `Pakuri/reference/monster-select-run-ui-expansion-plan.html`를 추가했다.

### History

- 2026-04-25: `AGENTS.md`, `BLACKBOARD.md`를 다시 읽고 현재 작업 규칙과 기존 작업 블록을 재확인했다.
- 2026-04-25: `2.Monster` 폴더 전체, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`, 각 몬스터 타워 문서를 읽었다.
- 2026-04-25: 현재 코드와 씬 상태를 다시 확인해 현재 구현이 이브 단독 전투 프로토타입과 임시 HUD 수준임을 재확인했다.
- 2026-04-25: UI 자산 부재, 보상 풀 미구현, 속성/상태 공통 시스템 부족을 현재 확장 작업의 핵심 갭으로 정리했다.
- 2026-04-25: 몬스터 선택 UI, Run 시작, 전투 후 보상/스킬 선택 흐름을 정리한 설계 HTML `Pakuri/reference/monster-select-run-ui-expansion-plan.html`를 추가했다.
- 2026-04-25: 사용자 답변을 반영해 패시브는 슬롯 `F~J` 총 5개, 런 중 최대 3개 습득으로 설계를 고정했고, 포로 보상은 우선 표시 전용 정보로 처리하기로 기록했다.
- 2026-04-25: 실제 저장소에서 린의 `g~j` 패시브 문서가 없음을 다시 확인해, 문서 기반 전체 몬스터 구현 전에 남은 자료 갭으로 기록했다.
- 2026-04-25: 사용자 답변을 반영해 1차 구현 범위를 `아리엘`, `이브`, `세인`, `베가` 4몬스터 우선으로 고정하고, `린`은 더미 상태로 남기기로 기록했다.

## Task: SaveAndLoad Direction Plan

### Task title

Run / Meta 저장 경계와 SaveAndLoad 구조 설계 HTML 작성

### Goals

- 현재 Run 확장 설계와 `reference/4.run`, `reference/6.meta` 문서를 근거로 저장 / 불러오기 방향을 정리한다.
- 런 내부 저장과 메타 영구 저장의 경계를 분리한다.
- v1에서 저장할 것, 나중에 미룰 것, 저장하지 않을 런타임 상태를 HTML 문서 한 장으로 정리한다.

### Constraints

- 실제 문서와 실제 현재 코드 구조를 근거로만 적는다.
- 아직 미작성인 메타 해금 문서를 구현된 것처럼 적지 않는다.
- 이 작업은 Designer 설계 문서 작성이며, 실제 SaveLoad 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 문서를 기준으로 Code Builder handoff를 작성해 `RunSession`, `MetaSaveData`, `RunSnapshot`, `SaveLoadService` 구현 순서를 고정한다.
- 실제 구현은 `GameDataCatalog` 부팅 로드 구조와 `RunSession` 분리 후 체크포인트 저장부터 시작하는 것이 맞다.

### Evidence

- `Pakuri/reference/4.run/dungeon-squad-run-structure.md`는 11일 단위 스테이지, 일반 진행일 선택지, 전투 후 보상, 다음 일차 이동 흐름을 정의한다.
- `Pakuri/reference/4.run/combat-reward-system.md`는 골드가 런 내부 재화이며 런 종료 시 사라지고, 어둠의 흔적이 런 외부 재화라고 정의한다.
- `Pakuri/reference/4.run/shop-system.md`는 상점이 스테이지당 1회, 6~9일 중 하루만 등장한다고 정의한다.
- `Pakuri/reference/4.run/event-system.md`는 일반 / 정예 전투 진입 직후 20% 확률 이벤트와 전투 복귀 흐름을 정의한다.
- `Pakuri/reference/6.meta/meta-growth-index.md`는 메타 성장에서 현재 확정된 범위와 미작성 범위를 구분한다.
- `Pakuri/reference/6.meta/meta-growth-node-list.md`는 캐릭터별 공통 스탯 강화와 초기화 규칙을 정의한다.
- `Pakuri/reference/6.meta/active-skill-growth-node-list.md`는 캐릭터별 액티브 메타 강화 규칙을 정의한다.
- `Pakuri/reference/6.meta/dark-trace-currency-system.md`는 어둠 계열 재화 티어, 승급, 사용처, 메타 초기화 규칙을 정의한다.
- `Pakuri/reference/monster-select-run-ui-expansion-plan.html`은 `RunSession` 분리와 Run 세션 데이터 제안을 포함한다.
- `Pakuri/reference/monster-select-run-ui-builder-handoff.html`은 고정 구현 순서에서 `RunSession` / `RunFlowController` 분리를 먼저 요구한다.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`는 현재 전투, 일차 진행, 보상, UI를 한 클래스에 함께 들고 있다.
- `Pakuri/data` CSV는 `Assets` 바깥에 있고, 현재 `Assets/Resources`, `Assets/StreamingAssets`, CSV 로더 흔적이 없다.
- `Pakuri/reference/save-and-load-plan.html`은 이제 저장 구조뿐 아니라 `CSV 저작 원본 -> 런타임 생성 자산 -> 게임 시작 시 1회 로드` 방향까지 포함한다.

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`, `monster-select-run-ui-expansion-plan.html`, `monster-select-run-ui-builder-handoff.html`, `reference/4.run`, `reference/6.meta`, 현재 `EveVerticalSliceController.cs`를 다시 읽었다.
- 2026-04-26: SaveAndLoad를 `MetaSaveData`, `RunSnapshot`, `EphemeralRuntime` 3층으로 나누고, v1은 일차 경계 체크포인트 저장만 지원하는 방향으로 정리한 HTML을 `Pakuri/reference/save-and-load-plan.html`에 추가했다.
- 2026-04-26: `Pakuri/data` CSV 검토 결과를 반영해 `save-and-load-plan.html`에 정적 게임 데이터 로딩 방향, importer 기반 생성 자산 구조, 부팅 시 1회 로드 방식을 추가했다.

## Task: CSV Data Role And Loading Review

### Task title

`Pakuri/data` CSV 역할 파악 및 게임 로딩 방식 검토

### Goals

- `Pakuri/data` 아래 CSV들의 실제 역할을 파일 구조와 샘플 행 기준으로 분류한다.
- 현재 프로젝트 코드가 이 CSV들을 실제로 읽고 있는지 확인한다.
- 게임에서 이 데이터를 언제, 어떤 방식으로 불러오는 것이 맞는지 설계 판단을 남긴다.

### Constraints

- 실제 CSV 내용, 실제 현재 스크립트, 실제 폴더 위치를 근거로만 판단한다.
- 아직 없는 CSV 로더나 데이터 파이프라인을 이미 있다고 말하지 않는다.
- 이 작업은 Designer 분석이며, CSV 로더 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 분석을 기준으로 Code Builder handoff를 작성해 CSV importer 또는 ScriptableObject 생성 파이프라인 구현 범위를 고정한다.
- 추천 방향은 `Pakuri/data`를 저작 원본으로 유지하고, 빌드용 런타임 데이터는 `Assets` 아래 생성 자산으로 변환하는 방식이다.

### Evidence

- `Pakuri/data` 아래 CSV는 총 22개이며 총 크기는 약 28.22KB다.
- `ally_units.csv`, `ally_runtime.csv`, `enemies.csv`, `enemy_runtime.csv`는 정적 스탯과 런타임 전투 파라미터가 분리된 구조다.
- `skills.csv`, `skill_runtime.csv`, `skill_branches.csv`, `levelup_choices.csv`, `levelup_rules.csv`는 스킬 / 분기 / 레벨업 선택지 데이터를 가진다.
- `waves_chapter1.csv`, `waves_chapter2.csv`, `waves_chapter3.csv`, `waves_runtime.csv`, `boss_patterns.csv`는 웨이브 / 보스 패턴 / 전투 진행 데이터를 가진다.
- `items.csv`, `status_effects.csv`, `formations.csv`, `balance_targets.csv`는 장비 / 상태이상 / 배치 / 밸런스 목표 데이터를 가진다.
- `spawn_points.csv`는 2번째 줄에 `적 스폰 좌표는 CSV가 아니라 코드에서 처리한다.`고 적혀 있어 현재 비활성 데이터다.
- `towers.csv`, `tower_skills.csv`는 `TOWER_001` 중심의 구형 단일 타워 프로토타입 데이터다.
- `ally_units.csv`는 `ALLY_*` 체계인데 `skills.csv`는 `TOWER_001` 소유 스킬만 가지고 있어 데이터 모델이 혼재되어 있다.
- 실제 무결성 확인 결과 `ally_units.csv`, `levelup_choices.csv`, `skill_branches.csv`가 참조하는 `SKILL_004` 이상 다수가 `skills.csv`에 없다.
- `Pakuri/data`는 `Assets` 바깥에 있으며, 현재 `Assets/Resources`, `Assets/StreamingAssets` 디렉터리는 존재하지 않는다.
- `Pakuri/Assets/Scripts`와 프로젝트 텍스트 파일 검색 결과 CSV 로더나 `TextAsset`, `Resources.Load`, `StreamingAssets` 사용 흔적은 확인되지 않았다.

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`를 다시 읽고 `Pakuri/data` 전체 CSV 목록, 헤더, 첫 행 샘플을 확인했다.
- 2026-04-26: 스킬 참조 무결성을 점검해 `ALLY_*` 기반 데이터와 `TOWER_*` 기반 데이터가 혼재되어 있고, 일부 스킬 참조가 비어 있음을 확인했다.
- 2026-04-26: 현재 CSV는 빌드 포함 위치에 있지 않고 로더도 없으므로, 런타임 직접 CSV 파싱보다 빌드 전 변환 자산 방식이 더 안전하다고 정리했다.
- 2026-04-26: 위 판단을 `Pakuri/reference/save-and-load-plan.html` 본문에도 반영해 SaveAndLoad와 정적 데이터 로딩 경계를 함께 문서화했다.

## Task: Run Systems Integration Summary Report

### Task title

`monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan` 통합 보고서 HTML 작성

### Goals

- 기존 3개 설계 HTML의 공통 결론을 한 장으로 합쳐 현재 프로젝트가 어떤 구조로 작업될지 빠르게 보여준다.
- 현재 실제 코드 상태와 문서 기준 구조를 함께 정리해, 구현 예정 범위와 아직 이른 범위를 분리한다.
- 기획서가 아직 부족한 부분과 현재 적용하기 이른 데이터 파이프라인을 명시적으로 `추후 구현 예정`으로 기록한다.

### Constraints

- 실제 존재하는 3개 HTML, 실제 현재 코드, 실제 문서 상태를 근거로만 적는다.
- 아직 구현되지 않은 UI, 저장, 데이터 importer를 구현된 것처럼 적지 않는다.
- 이 작업은 Designer 보고서 작성이며, 실제 코드 구현은 포함하지 않는다.

### Role Owner

Designer

### Status

Completed

### Next Actions

- 사용자가 원하면 이 통합 보고서를 기준으로 Designer가 Code Builder handoff 문서를 더 짧게 다시 정리할 수 있다.
- 실제 구현은 보고서에 적은 순서대로 `RunSession` 분리, UI 흐름 분리, 정적 데이터 자산, A/F 최소 보상 / 스킬선택, 체크포인트 저장 순으로 들어가는 것이 안전하다.

### Evidence

- `Pakuri/reference/monster-select-run-ui-builder-handoff.html`는 `RunSession`, `RunFlowController` 또는 동등 구조를 먼저 세우는 고정 구현 순서를 제안한다.
- `Pakuri/reference/monster-select-run-ui-expansion-plan.html`는 몬스터 선택 UI, Run 시작, 전투 후 보상/선택 흐름과 `RunSession` 중심 구조를 설명한다.
- `Pakuri/reference/save-and-load-plan.html`는 `MetaSaveData`, `RunSnapshot`, `GameDataCatalog` 분리와 부팅 시 1회 데이터 로드를 정의한다.
- 현재 프로젝트의 게임 전용 스크립트는 `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`만 확인된다.
- 현재 `Pakuri/Assets` 아래에는 `Scenes`, `Screenshots`, `Scripts`, `Settings`만 있고, `Resources`, `StreamingAssets`, `DataGenerated`는 없다.
- 현재 프로젝트에는 `.uxml`, `.uss` UI Toolkit 자산이 없다.
- 실제 CSV 원본은 `Pakuri/data`에 있지만 현재 로더와 생성 자산 파이프라인은 없다.
- 새 통합 문서 `Pakuri/reference/run-systems-integration-summary-report.html`를 추가했고, 문서 안에 현재 구조, 작업 순서, 저장/데이터 방향, `추후 구현 예정` 항목을 함께 정리했다.
- 2026-04-26 재확인 결과 `Pakuri/reference/2.Monster/rin/rin-tower.md`와 `rin/skill/g~j` 문서가 존재해, 린의 패시브 문서 부족 전제는 더 이상 유효하지 않다.
- 2026-04-26 재확인 결과 `Pakuri/Assets` 재귀 검색에서 `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset` 관련 정적 데이터 로더 / 자산 정의는 확인되지 않았다.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`는 현재 보상 패널에서 이브 전용 고정 선택지 3개만 직접 생성한다.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`와 `Pakuri/reference/4.run/combat-reward-system.md`는 전체 보상 / 스킬선택 규칙을 정의하지만, 현재 구현은 그 전체 범위에 아직 도달하지 않았다.

### History

- 2026-04-26: `AGENTS.md`, `BLACKBOARD.md`, 기존 3개 설계 HTML을 다시 읽고 서로 겹치는 구조와 고정 결론을 추렸다.
- 2026-04-26: 현재 실제 코드와 자산 상태를 다시 확인해, 아직 없는 UI Toolkit 자산과 데이터 생성 파이프라인을 보고서에 명시적으로 비구현 상태로 적었다.
- 2026-04-26: `Pakuri/reference/run-systems-integration-summary-report.html`를 추가해 현재 구조, 권장 구현 순서, 데이터/저장 경계, 기획 부족 영역과 이른 데이터 적용 범위를 `추후 구현 예정`으로 분리했다.
- 2026-04-26: 린 문서 갱신과 데이터 방향 변경을 반영해 `run-systems-integration-summary-report.html`를 수정했고, 린을 5몬스터 범위에 포함시키고 정적 데이터는 CSV importer 전제가 아니라 Unity 프로젝트 내부 정적 자산 기준으로 정리했다.
- 2026-04-26: 보상 / 스킬선택은 완전히 나중으로 미루지 않고, `RunSession` / UI / 공통 전투 코어 다음 마일스톤에서 A/F 최소 범위를 같이 붙이는 방향으로 `run-systems-integration-summary-report.html`를 다시 수정했다.

## Task: Run Flow UICanvas Prototype Implementation

### Task title

`run-systems-integration-summary-report.html` 기준 첫 구현 슬라이스 착수

### Goals

- 5몬스터 선택, `RunSession`, `RunFlowController`, `UICanvas` 기반 흐름의 첫 구현 슬라이스를 만든다.
- 정적 데이터는 CSV 런타임 로드 대신 Unity 프로젝트 내부 자산으로 만든다.
- 현재 `EveVerticalSliceController`를 선택 몬스터 기반 공통 A 스킬 프로토타입 전투와 A/F 최소 보상 루프가 가능한 구조로 연다.

### Constraints

- 사용자의 요청대로 유니티 플레이 실행 검증은 사용자에게 맡기고, 저는 코드/씬/자산 준비와 에디터 상태 확인까지만 한다.
- UI는 `UICanvas` 기준으로 씬에 직접 배치한다.
- 현재 사용자의 지시로 외부 Reviewer 단계는 잠시 중지하고, Builder 종료 후에는 간단한 자체 점검만 수행한다.
- 구현되지 않은 B~E, G~J, 유물 3택1, 전체 혼합 보상 풀은 이번 슬라이스 범위에 넣지 않는다.

### Role Owner

Code Builder

### Status

Builder changes applied. 외부 Reviewer 1회 결과 반영까지는 완료됐고, 이후 Reviewer 단계는 사용자 지시로 잠시 중지했다. `LegacyRuntime.ttf` 교체와 Unity 재컴파일까지 마쳤고, 현재는 사용자 플레이 검증 대기 상태다.

### Next Actions

- 사용자가 Unity에서 플레이 모드로 `RunUICanvas` 동작, 5몬스터 선택, 전투 진입, 최소 보상 선택, 다음 일차 진행을 검증한다.
- 검증 중 UI 배치 문제나 입력 문제, 전투 흐름 문제를 확인하면 그 근거를 받아 다음 Builder 수정으로 이어간다.
- 이후 확장은 `유물 3택1`, `신규 액티브/패시브/특성/마스터 전체 풀`, `B/G, C/H, D/I, E/J` 순으로 간다.

### Evidence

- 새 런타임 데이터 스크립트 `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`를 추가했다.
- 에디터 시드 스크립트 `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`를 추가했고, Unity 메뉴 `Pakuri/Seed Default Game Data` 실행으로 `Assets/Data/GameData/GameDataCatalog.asset`와 5개 몬스터 자산을 생성했다.
- 새 런 흐름 스크립트 `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`를 추가했다.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`를 선택 몬스터 기반 공통 A 스킬 프로토타입 전투와 최소 보상 루프를 처리하도록 크게 수정했다.
- Unity 씬 `Assets/Scenes/SampleScene.unity`에 루트 `RunUICanvas`와 `EventSystem`을 직접 생성하고 저장했다.
- Unity asset search 결과 `Assets/Data/GameData/GameDataCatalog.asset`와 `Assets/Data/GameData/Monsters/*.asset` 5개가 실제로 생성됐다.
- Unity root hierarchy 재확인 결과 `RunUICanvas`에는 `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`가 붙었고, `EventSystem`에는 `EventSystem`, `InputSystemUIInputModule`가 붙었다.
- 외부 Reviewer 1회 결과는 세 가지 이슈를 지적했다: 보상 효과가 다음 일차에 유지되지 않는 문제, 스테이지 배율이 전투/보상에 반영되지 않는 문제, 플레이 중 버튼 재생성 시 소멸 위험.
- 그 지적을 반영해 `RunSession`에 누적 보상 수치를 추가하고, `EveVerticalSliceController.BeginConfiguredDay(...)`가 세션 누적 보상을 다시 적용하도록 수정했다.
- 같은 수정에서 `EveVerticalSliceController`는 `stageIndex` 기반 적 체력 배율과 어둠의 흔적 지급 배율을 반영하도록 수정했다.
- `RunFlowController.ClearButtons(...)`는 플레이 중 재생성 버튼이 같은 이름으로 재사용되지 않도록 `QueuedForDestroy` 이름 변경 후 제거하도록 수정했다.
- 2026-04-26 사용자 플레이 검증에서 `RunFlowController.ResolveReferences()`의 `Arial.ttf` 참조가 Unity 내장 폰트 정책과 맞지 않아 `ArgumentException`이 발생했고, 이를 `LegacyRuntime.ttf`로 교체했다.
- `LegacyRuntime.ttf` 교체 후 Unity 스크립트 재컴파일을 요청했고, 최근 Unity console 20개 로그 재확인에서는 동일한 `Arial.ttf` 예외가 다시 보이지 않았다.
- 외부 Reviewer 재실행은 10분 타임아웃 안에 끝나지 않았고, 이후 Reviewer 단계는 사용자 지시로 잠시 중지했다.

### History

- 2026-04-26: Designer 기준으로 현재 HTML과 실제 코드/씬 상태를 다시 읽고 첫 Builder 슬라이스 범위를 `정적 데이터 자산 + RunSession/RunFlowController + UICanvas + A/F 최소 보상 루프`로 고정했다.
- 2026-04-26: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`를 새로 추가했다.
- 2026-04-26: `Pakuri/Seed Default Game Data`를 실행해 5몬스터 기본 자산과 `GameDataCatalog.asset`를 생성했다.
- 2026-04-26: `RunUICanvas`, `EventSystem`을 씬에 추가하고 저장했다.
- 2026-04-26: 외부 Reviewer 1회가 보상 유지, 스테이지 배율, 버튼 재생성 문제를 지적했고, Builder가 같은 턴에서 세 이슈를 수정했다.
- 2026-04-26: 수정 후 Unity console에서는 새 컴파일 오류가 보이지 않았고, 외부 Reviewer 재실행은 시간 초과로 종료됐다.
- 2026-04-26: 사용자 플레이 검증에서 `Resources.GetBuiltinResource<Font>("Arial.ttf")` 예외가 보고됐고, `RunFlowController`의 기본 폰트를 `LegacyRuntime.ttf`로 교체했다. 같은 시점에 사용자 요청으로 외부 Reviewer 단계는 잠시 중지하고 자체 점검만 유지하기로 했다.
- 2026-04-26: `LegacyRuntime.ttf` 교체 후 Unity 재컴파일과 최근 콘솔 로그를 다시 확인했고, 동일한 폰트 예외는 재현되지 않았다.
