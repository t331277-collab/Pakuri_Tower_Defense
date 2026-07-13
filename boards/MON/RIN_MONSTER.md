## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-08 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/RIN_MONSTER.md`.

## Task: 2026-07-12 Rin Skill Runtime Visual Migration Feasibility

### Task title

Classify Rin skill prefab visuals for Ariel-style runtime composition.

### Goals

- Identify Rin visuals that fit the existing shared runtime sprite/animator/box model.
- Retain prefabs where collider offsets or named child hitboxes carry gameplay meaning.
- Define a behavior-preserving Code Builder migration order without deleting prefabs.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- Runtime/CSV implementation is included; prefab deletion and scene-mapping cleanup remain outside this pass.
- Prefab assets remain on disk until all converted paths pass user Play Mode verification.

### Role Owner

Code Builder

### Status

Rin A/B/C/F/D-master1 runtime visual migration implemented and source/build validated. User Play Mode parity verification remains.

### Next Actions

- User verifies Rin A/B/C/F and D master 1 visual/collision parity in Play Mode.
- User verifies Rin E base-area and `CoreHitBox` center effects after explicit prefab-hitbox routing.
- Keep Rin D base and Rin E prefab-backed; retain converted prefab/scene fallback references until parity is confirmed.

### Evidence

- Unity-MCP inspected all seven Rin skill prefab hierarchies: A/B/C/D/D-master1/F are single-root; E alone contains `Rin_E/CoreHitBox`.
- Current factory supports one sprite, one animator, uniform scale, and one zero-offset root box collider.
- A/B/C fit the existing runtime path; F fits after passive Trigger CSV columns are exposed.
- D master 1 has collider offset `(0.53632426, -0.41973162)`; user approved preserving it through a shared runtime hitbox-offset extension.
- D base remains prefab-backed by user decision. E remains prefab-backed because it has two differently transformed colliders including named child `CoreHitBox`.
- Active G-J rows contain no prefab or runtime visual path and therefore have no parity-migration target.
- Active Rin-E data maps to `UsePrefabHitbox=false`, while named core lookup only runs inside the prefab-hitbox branch; this is a verified pre-migration blocker.
- Shared runtime hitbox specs now preserve optional offset; D master 1 CSV carries exact size `(3.9373517, 3.788869)` and offset `(0.53632426, -0.41973162)`.
- Rin A/B/C base and Rin F follow-up rows now carry runtime visual data; runtime execution paths prefer those specs over prefab fallback.
- Rin E now carries `use_prefab_hitbox=true`; explicit prefab hitbox with no target count resolves all overlapping targets while retaining target-centered placement.
- CSV shape checks passed for all six edited files. Runtime and Editor builds passed with 0 errors.
- Unity-MCP source validation loaded 5 monsters without validation errors. No Rin prefab or `NewRunScene` diff exists.

### History

- 2026-07-12: User requested Rin A-J prefab-to-runtime feasibility verification using the Ariel migration approach.
- 2026-07-12: Designer classified A/B/C/D as easy, F as a small schema-exposure conversion, D master 1 as conditional, E as prefab-retained, and G-J as having no current visual prefab target.
- 2026-07-13: User selected Rin D base for prefab retention and Rin D master 1 for runtime conversion; Designer revised the handoff to preserve D master 1's non-zero collider offset through a shared optional offset extension.
- 2026-07-13: Code Builder implemented the approved runtime visual rows, shared offset support, and Rin-E explicit prefab-hitbox routing; prefab deletion/scene cleanup deferred until user Play Mode parity.

## Task: 2026-07-12 Rin A-J Node Migration Proposal

### Task title

Design Rin A-J migration from wide/legacy skill authoring to positional skill graphs.

### Goals

- Move Rin base/Choice/Effect behavior to existing graph kinds while preserving Trigger event envelopes.
- Reuse current wide/direct/runtime meanings and introduce no new gameplay semantics.
- Preserve Rin-E `CoreHitBox` and existing skill prefab contracts during the node migration.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- Existing prefab, scene, and Rin-E `CoreHitBox` contracts remain unchanged.
- Rin graph rows and Rin legacy direct nodes cannot coexist in one materialized dataset.

### Role Owner

Code Builder

### Status

Rin A-J positional graph migration implemented and source/build validation completed. Play Mode behavior verification remains.

### Next Actions

- Verify Rin A-E damage, targeting, reload, slow, execute, `CoreHitBox`, and hit-count refund behavior in Play Mode.
- Verify Rin F-J Trigger cadence and passive Effect gates in Play Mode.

### Evidence

- Inspected Rin reference A-J files, normalized base/Choice/Effect/Trigger/direct-node CSV rows, node definitions, materializer, mapper, executors, status runtime, and Rin prefabs.
- Current Rin data contains base 10, Choice 50, graph 0, legacy Effect 20, Trigger 17, direct node 11, and direct param 22 rows.
- All needed graph kind files already exist; no new graph CSV file is proposed.
- Every requested Rin behavior already has a current wide/direct/Effect/Trigger runtime meaning, so the proposal requires zero new gameplay semantics.
- Rin now materializes from 138 positional graph rows; Rin legacy Effect rows, legacy direct nodes/params, and non-routing Choice behavior values are all zero.
- All 17 Rin Trigger rows remain; the two Rin-I kill triggers now reference Trigger-owned Effect graphs.
- Runtime and Editor C# builds completed with 0 errors; Unity `Pakuri/Validate CSV Source Data` completed without validation errors.
- `git diff --name-only -- Pakuri/Assets/Prefab Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` returned no changed prefab or scene path.

### History

- 2026-07-12: User requested an Eve-format Rin node migration proposal that maximizes reuse of existing features.
- 2026-07-12: Designer created `RIN_NODE_MIGRATION_PROPOSAL.md` and retained prefab/Trigger compatibility boundaries.
- 2026-07-12: Code Builder exposed the approved shared node meanings, migrated Rin A-J to positional graphs, removed overlapping Rin legacy authoring, and completed source/build validation.

## Task: 2026-06-07 Rin Animator Trigger Controller And Shared Actor Hook

### Task title

Move Rin unit animation routing from direct state-play attack/hit calls to Animator parameters, and make monster animation hooks reusable by other monster actors.

### Goals

- Add trigger/int parameter routing to `Rin_Animation_Cont.controller`.
- Change `Animation_Controller` attack and hit playback to use Animator parameters instead of hardcoded direct state names.
- Keep death final-frame freeze in script after the `Death` trigger.
- Remove the Rin-only `MonsterUnitActor` definition-id gate so other monster prefabs can opt in by adding `Animation_Controller` and a compatible Animator Controller.

### Constraints

- Role Owner is Code Builder.
- Existing Rin state names and animation clip references are preserved.
- Other characters still need their own compatible Animator Controller parameters and prefab component wiring before they can play animations.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin plays random `AttackIndex` 0-2 attacks, hit animation, death animation, and death final-frame freeze.
- For Ariel/Eve/Sein/Vega, add `Animator` plus `Animation_Controller` to each prefab and use an Animator Controller with `Attack`, `AttackIndex`, `Hit`, and `Death` parameters.

### Evidence

- `Pakuri/Assets/Image/Monster/Rin/Animation/Animation_Rin 1/Rin_Animation_Cont.controller` now has `Attack`, `AttackIndex`, `Hit`, and `Death` Animator parameters.
- The same controller now has Any State transitions for `Attack` plus `AttackIndex` 0, 1, and 2 into `Anim_Rin_Attack_1`, `Anim_Rin_Attack_2`, and `Anim_Rin_Attack_3`.
- The same controller now has Any State transitions for `Hit` into `Anim_Rin_Hit` and `Death` into `Anim_Rin_Dead_1`.
- Attack and hit states now transition back to `Anim_Rin_Idle` with exit time.
- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now calls `SetInteger("AttackIndex", ...)`, `SetTrigger("Attack")`, `SetTrigger("Hit")`, and `SetTrigger("Death")` when those Animator parameters exist, and keeps direct `Animator.Play(deadState, 0, 0.999f)` only for the death final-frame freeze.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` no longer contains `RinMonsterId` or `ShouldUseRinAnimation()` and now calls the resolved `Animation_Controller` for any monster actor that has the component.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `git diff --check -- Pakuri\Assets\Scripts2\InGame\Animation\Animation_Controller.cs Pakuri\Assets\Scripts2\InGame\Units\MonsterUnitActor.cs "Pakuri\Assets\Image\Monster\Rin\Animation\Animation_Rin 1\Rin_Animation_Cont.controller"` passed with only line-ending conversion warnings.

### History

- 2026-06-07: User asked Code Builder to update `Rin_Animation_Cont.controller`, `Animation_Controller.cs`, and `MonsterUnitActor.cs` so Rin uses Animator parameters/transitions for normal animation routing while `MonsterUnitActor` becomes reusable by other characters.

## Task: 2026-05-26 Rin-E SingleAttack Core Hitbox Skill Completion

### Task title

Implement Rin-E base skill, enhancement traits, and master effects on the shared SingleAttack prefab-hitbox path.

### Goals

- Use `Assets/Prefab/Skill/Rin/Rin_E.prefab` as Rin-E's skill effect prefab.
- Let `CoreHitBox` child colliders drive center-only Rin-E effects.
- Implement Rin-E trait 1-5 and master 1-2 without Rin-only hardcoded branches.
- Keep center damage, center Fire bonus, hit-count cooldown refund, Dark extra damage, and master-2 slow on shared runtime/data extensions.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/single-attack-blueprint.md`.
- User explicitly approved the reusable shared extension for behavior outside the original SingleAttack common contract.
- Unity Play Mode gameplay verification remains user-owned.
- Unity CSV runtime catalog sync is pending because batchmode reported another Unity instance has this project open.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and synced through the open Unity Editor menu after the follow-up CSV validation fix.

### Next Actions

- User verifies in Play Mode that Rin-E uses `Rin_E.prefab`, base hit timing follows current `damage_delay_seconds`, and `CoreHitBox` center hits apply center-only effects.
- User verifies in Play Mode that Rin-E master 2 applies the intended slow to hit enemies.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Rin/Rin_E.prefab` contains a child named `CoreHitBox` with an enabled `BoxCollider2D`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `skill_effect_prefab_path` and `rin-e` points to `Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `core_hitbox_name`, `core_damage_multiplier`, `core_on_hit_additional_damage_*`, and `hit_count_cooldown_refund_*` columns.
- Rin-E trait rows are now `RuntimeImplemented`: trait 1 damage `1.3`, trait 2 radius `1.25`, trait 3 cooldown `0.8`, trait 4 `CoreHitBox` damage `1.5`, and trait 5 `rin-b` cooldown refund ratio `0.2` when at least 3 targets are hit.
- Rin-E master rows are now `RuntimeImplemented`: master 1 damage `2`, radius `0.8`, and core Fire additional damage `1`; master 2 damage `1.35`, radius `1.5`, and Dark on-hit additional damage `0.45`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes `rin-e-master2-slow`, an OnHit status effect that applies `slow` for `2` seconds with `status_move_speed_bonus=-0.25`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now separates configured core hitbox colliders, applies core-only damage/extra damage, applies SingleAttack OnHit status effects, and applies hit-count cooldown refunds after a cast.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed only because Unity batchmode reported another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Follow-up enum validation found the `DamageAttribute` enum defines `Darkness`, not `Dark`; `rin-e-master-2` and `rin-e-master2-slow` were corrected from `Dark` to `Darkness`, and a CSV enum scan returned `ENUM_VALIDATION_OK`.
- Follow-up status-scope validation found `StatusEffectRuntime.TryParseStatusTargetScope(...)` only accepts `self` and `all_allies`; `rin-e-master2-slow` now leaves `status_target_scope` blank like `rin-c-master2-slow`, while `target_side=Enemy` still makes the OnHit status enemy-targeted.
- CSV source scans returned `STATUS_TARGET_SCOPE_OK`, `STATUS_MERGE_POLICY_OK`, and `DAMAGE_ATTRIBUTE_ENUM_OK` for `monster_skill_effects.csv`.
- `.NET TextFieldParser` scans returned `FIELD_COUNT_OK` for `monster_skill_effects.csv` 61 columns / 78 lines, `monster_skill_choices.csv` 86 columns / 252 lines, `monster_skills.csv` 57 columns / 52 lines, and `monster_skill_triger.csv` 34 columns / 10 lines.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the fix.

### History

- 2026-05-26: User asked Code Builder and Skill Builder to apply the approved SingleAttack CoreHitBox extension and implement Rin-E with all enhancement and master effects using `Assets/Prefab/Skill/Rin/Rin_E.prefab`.
- 2026-05-26: User reported Unity auto-sync failing on `monster_skill_effects.csv` row 78 because `attribute=Dark` was not a valid `DamageAttribute`; Builder corrected the enum value to `Darkness` and checked CSV enum columns for the same class of error.
- 2026-05-26: User reported Unity CSV validation still failing on `rin-e-master2-slow status_target_scope=enemy`; Builder cleared that unsupported scope, verified the relevant CSV schemas and enum/status-scope scans, and synced the runtime catalog through the open Unity Editor menu.

## Task: 2026-05-26 Rin Unit Animator Runtime Hook

### Task title

Connect Rin's prefab Animator to the current Scripts2 active-skill, hit, and death runtime events.

### Goals

- Add an `Animation_Controller` runtime component that drives the existing `Rin_Animation_Cont` states directly.
- Play one random Rin attack state whenever a non-triggered active skill cast is successfully routed.
- Play Rin hit animation on non-lethal monster damage and Rin death animation before the dead unit is destroyed.
- Keep the first implementation scoped to `Assets/Prefab/Monster/Rin_Unit.prefab`.

### Constraints

- Role Owner is Code Builder.
- The existing animator controller has no parameters or transitions, so direct `Animator.Play(...)` calls are used.
- Runtime animation requests are gated by the unit model `DefinitionId == "rin"` in `MonsterUnitActor`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally verified through compile, prefab inspection, and Unity editor code inspection.

### Next Actions

- User verifies in Play Mode that Rin plays random attack animations on active skill casts, hit animation on non-lethal damage, and death animation at HP 0.
- If other monsters need the same behavior later, promote the Rin-only model gate into shared data or prefab configuration.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Animation/Animation_Controller.cs` now resolves the local `Animator`, plays `Anim_Rin_Attack_1`, `Anim_Rin_Attack_2`, or `Anim_Rin_Attack_3` randomly, plays `Anim_Rin_Hit`, locks on `Anim_Rin_Dead_1`, and returns transient attack/hit states to idle after the clip length.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` now exposes `TryPlayActiveSkillAnimation()`, `TryPlayHitAnimation()`, and `TryPlayDeathAnimation()` and only routes them when the model definition id is `rin`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now calls the active-skill animation hook only after `executor.Execute(...)` routes and `runtime.TryBeginCast(snapshot)` succeeds.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls the hit animation for non-lethal monster damage and the death animation before `Destroy(actor.gameObject, 0.95f)`.
- `Pakuri/Assets/Prefab/Monster/Rin_Unit.prefab` now has `Pakuri.InGame.Animation_Controller` on the root beside `MonsterUnitActor` and `Animator`.
- Unity editor code inspection returned `actor=True|animator=True|animationController=True|controllerName=Rin_Animation_Cont|clips=Anim_Rin_Idle,Anim_Rin_Attack_1,Anim_Rin_Attack_2,Anim_Rin_Attack_3,Anim_Rin_Dead_1,Anim_Rin_Hit`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone; the first parallel attempt hit only an `Assembly-CSharp.dll` file lock.

### History

- 2026-05-26: User asked Code Builder to implement the Designer-approved Rin animation plan using `Animation_Controller.cs`, `Rin_Unit.prefab`, and the existing `Rin_Animation_Cont.controller`.

## Task: 2026-05-26 Rin-D Execute Gate And Execute-Only Kill Effects

### Task title

Implement Rin-D cast gating at the execute threshold and restrict execute-only kill rewards to the primary Rin-D hit on shared Scripts2 runtime paths.

### Goals

- Make `rin-d` reject casts unless its selected target is within the current execute threshold.
- Keep Rin-D target ordering on the existing `LowestHealth` raw-current-health selection.
- Make Rin-D master 1 cooldown reset and Holy burst require an execute kill from the primary `rin-d` hit.
- Keep the fix on shared `SingleAttack`, damage, and trigger runtime paths without Rin-only hardcoded branches.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested that target selection remain unchanged.
- New behavior is data-driven through shared CSV/runtime flags instead of monster-specific branches.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-D does not cast above the current execute threshold.
- User verifies that Rin-D master 1 Holy burst triggers only on execute kills from the primary Rin-D hit and does not chain its own kill reset behavior.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now adds shared `require_execute_threshold_to_cast` and sets it to `true` on `rin-d`.
- The same `monster_skills.csv` file had to be normalized so all active skill rows carry the new trailing column; a post-fix CSV field-count scan returned `ALL_ROWS_OK` for 55-column rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds shared `kill_resets_cooldown_requires_execute` and sets it to `true` on `rin-d-master-1`.
- The same `monster_skill_choices.csv` file had to be normalized so pre-existing choice rows also carry the new trailing column; post-fix field-count scans returned `UTF8_ALL_ROWS_OK` and `ALL_ROWS_OK_AFTER_BOM` for 78-column rows after the file was rewritten as UTF-8 BOM.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now adds shared `require_event_execute` and sets it to `true` on `rin-d-master1-kill-burst`.
- The same `monster_skill_triger.csv` file had to be normalized so pre-existing trigger rows also carry the new trailing column; a post-fix CSV field-count scan returned `ALL_ROWS_OK` for 34-column rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now rejects `SingleAttack` casts when `RequireExecuteThresholdToCast` is enabled and the selected target is above threshold, and it passes execute-hit state into shared kill recovery.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now carry shared execute-kill context through damage and kill trigger dispatch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now attributes triggered damage to `triggered_skill_id` first, so triggered Holy burst kills no longer report as primary `rin-d` kills.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after a rerun; the first parallel attempt failed only because `Assembly-CSharp.dll` was temporarily locked by the concurrent runtime build.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the `monster_skill_choices.csv` row-width normalization follow-up.

### History

- 2026-05-26: User requested Code Builder implementation so Rin-D casts only below threshold, keeps current target selection, and applies master-1 cooldown reset only to execute kills from the primary Rin-D hit.
- 2026-05-26: Follow-up fix normalized `monster_skills.csv` row widths after Unity CSV runtime sync reported row 3 had 54 columns while the updated header expected 55.
- 2026-05-26: Additional follow-up fix normalized `monster_skill_triger.csv` row widths after Unity CSV runtime sync reported row 3 had 33 columns while the updated header expected 34.
- 2026-05-26: Additional follow-up fix normalized `monster_skill_choices.csv` row widths after Unity CSV runtime sync reported row 3 had 77 columns while the updated header expected 78, then rewrote the file as UTF-8 BOM and re-synced the runtime catalog successfully.

## Task: 2026-05-26 Rin-D Execute Condition And Master Effect Audit

### Task title

Audit current `rin-d` SingleAttack runtime against the authored execute threshold, enhancement effects, and master effects.

### Goals

- Verify whether `rin-d` cast gating matches the authored 30% execute-health behavior.
- Verify that Rin-D trait and master choice fields map into the current Scripts2 runtime as intended.
- Record confirmed implementation mismatches before Builder follow-up.

### Constraints

- Role Owner is Designer.
- Claims are limited to inspected CSV rows and current Scripts2 runtime code.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` does not exist in the current workspace, so no legacy-side comparison was possible from that path.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer

### Status

Inspection completed. Confirmed code/data mismatches exist in the current Scripts2 Rin-D behavior.

### Next Actions

- Builder should decide whether Rin-D must refuse execution unless a target is within the execute threshold, or whether only the execute bonus should be gated while cast remains allowed.
- Builder should add an execute-only gate for Rin-D master-1 kill reset and kill burst if the authored text is meant to apply only to executed targets.
- Builder should review whether Rin-D target selection should prefer lowest health ratio instead of lowest raw current health.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` defines `rin-d` as `runtime_kind=SingleAttack`, `target_selection=LowestHealth`, `execute_health_ratio_threshold=0.3`, `execute_damage_multiplier=1.8`, and `kill_cooldown_refund_ratio=0.35`.
- `Pakuri/reference/2.Monster/rin/skill/d-finishing-blow.md` authors Rin-D around `泥섑삎 湲곗? 泥대젰 30% ?댄븯`, trait 2 `泥섑삎 湲곗? 泥대젰 +10%`, master 1 `泥섑삎 ??곸뿉寃?移섎챸? ?뺣쪧 +50%, 泥섏튂 ??荑⑤떎???꾩쟾 珥덇린??, and master 2 `泥섑삎 湲곗? 泥대젰 -10%`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:708` through `:714` only use the execute threshold to add execute damage and execute crit chance; the cast path itself does not stop when the target is above the threshold.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:359` through `:389` always damage the first ordered target from `ResolveOrderedTargets(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs:68` through `:97` and `SkillExecutionUtility.cs:219` through `:221` implement `LowestHealth` with raw `CurrentHealth`, not health ratio.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` gives `rin-d-master-1` `execute_crit_chance_bonus=0.5` and `kill_resets_cooldown=true`; `rin-d-master-2` gives `damage_multiplier=1.9`, `execute_health_ratio_bonus=-0.1`, `cooldown_multiplier=1.25`, and guaranteed Darkness on-hit additional damage.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:755` through `:767` reset or refund cooldown on any Rin-D kill, without checking whether that hit was an execute hit.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:10` plus `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs:181` through `:212` wire the Rin-D master-1 Holy burst to generic `OnKill`, and the trigger context does not carry an execute/non-execute flag.

### History

- 2026-05-26: User reported that Rin-D seemed to fire even when the opponent was not below 30% HP and asked for a full inspection of the skill, enhancement effects, and master effects.

## Task: 2026-05-26 Rin-B And Rin-C Skill Builder Completion

### Task title

Implement Rin-B and Rin-C active enhancement/master effects on the current Scripts2 shared skill runtime.

### Goals

- Keep `rin-c` on the shared `LineAttack` / beam runtime and finish all trait/master effects.
- Keep `rin-b` on the shared buff runtime and finish all trait/master effects, including the ally-wide master follow-up damage.
- Reuse shared CSV/runtime paths instead of adding Rin-only hardcoded branches.

### Constraints

- Role Owner is Skill Builder.
- User explicitly authorized current Rin CSV/reference files as the parsed source for this task.
- Base skill visuals remain scene-owned through `NewRunScene` `EffectManager`; choice/effect behavior remains data-owned through the active CSV/runtime pipeline.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented, compile-verified, and Unity refresh-checked.

### Next Actions

- User verifies in Play Mode that Rin-B ally buffs and Rin-C knockback / reload reduction / lightning follow-up / slow behave as authored.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps `rin-b` at `status_duration_seconds=5` and `status_action_speed_bonus=0.2`, and adds `knockback_distance=0.6` to `rin-c`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks all `rin-b-*` and `rin-c-*` choice rows as `RuntimeImplemented`; `rin-c-trait-2` uses `beam_width_bonus=0.25`, `rin-c-trait-3` uses `knockback_distance_multiplier=1.4`, `rin-c-trait-5` uses `reload_reduce_target_skill_id=rin-a` plus `reload_reduce_seconds_per_hit=0.25`, `rin-c-master-1` uses the shared on-hit lightning fields, and `rin-c-master-2` uses `beam_width_bonus=0.6`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `rin-b-trait2-action-speed`, `rin-b-trait4-self-attack`, `rin-b-trait5-crit`, `rin-b-master1-roar`, `rin-b-master2-abyss`, and `rin-c-master2-slow`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/BeamSkillExecutor.cs`, `InGameLineAttackActor.cs`, `SkillOnHitAdditionalDamageUtility.cs`, and `SkillRuntimeInstance.cs` now cover Rin-C width/knockback/reload-reduction/on-hit additional damage on the shared beam path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SupportSkillExecutors.cs`, `SkillMultiEffectExecutor.cs`, `StatusEffectRuntime.cs`, and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now cover Rin-B scaled buff multi-effects and status-driven outgoing additional damage on the shared buff/status path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` also passed with 0 errors after rerun outside the sandbox-denied path. Existing MSB3277 warnings remain.
- Unity `refresh_unity` returned `resulting_state":"idle"`, and warning/error console reads after the refresh showed only MCP-FOR-UNITY client-handler logs, not C# or CSV runtime errors.

### History

- 2026-05-26: User instructed Skill Builder to start with Rin-C, explicitly approved current Rin CSV/reference files as parsed source, and then requested the same treatment for Rin-B.

# RIN_MONSTER

## Scope

Rin dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Rin file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Not populated yet.

## Task: 2026-05-24 Rin-A Master-2 On-Hit Lightning Revision

### Task title

Revise `rin-a-master-2` from projectile branch launch behavior to shared on-hit Lightning additional damage and every-third-hit chain damage.

### Goals

- Make every Rin-A primary hit apply Lightning additional damage equal to 40% of the resolved hit damage.
- Make every 3rd Rin-A primary hit chain Lightning damage equal to 40% of the resolved hit damage to up to 2 enemies near the hit target.
- Keep the behavior on a shared on-hit damage extension usable by projectile, beam, zone, and single-attack hit paths.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User provided the parsed behavior values in the request.
- No Rin-only hardcoded runtime branch was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-A master-2 applies the direct Lightning extra hit on each hit and chains to 2 nearby enemies every 3rd primary hit.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gives `rin-a-master-2` `on_hit_additional_damage_chance=1`, `on_hit_additional_damage_multiplier=0.4`, `on_hit_additional_damage_attribute=Lightning`, `on_hit_additional_damage_target=HitTarget`, `on_hit_chain_hit_period=3`, `on_hit_chain_target_count=2`, `on_hit_chain_search_radius=4.5`, `on_hit_chain_damage_multiplier=0.4`, and `on_hit_chain_damage_attribute=Lightning`.
- The same row now has blank `branch_chance_set`, `branch_count`, `branch_damage_multiplier`, `branch_launch_period`, and `branch_launch_chance_set`, so it no longer uses the projectile branch launch override path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillOnHitAdditionalDamageUtility.cs` applies shared direct on-hit extra damage and every-nth-hit chain damage.
- Unity-MCP editor execution returned `rin-a-master-2|extra=True:1:0.4:Lightning:HitTarget|chain=3:2:4.5:0.4:Lightning|branch=False:False:0:False`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-24: User clarified that master-2 should be on-hit Lightning additional damage plus every-third-hit chain damage, not projectile branch launch chance behavior.

## Task: 2026-05-24 Rin-A Choice Runtime Completion

### Task title

Implement Rin-A remaining enhancement and master choice data on the shared projectile path.

### Goals

- Move `rin-a-trait-5` from partial support to shared projectile critical bonus fields.
- Keep `rin-a-master-1` on existing shared damage, magazine, and shot-interval fields.
- Implement `rin-a-master-2` with shared projectile branch behavior plus every-third-projectile-launch chance override.
- Use `Assets/Prefab/Skill/Rin/Rin_A.prefab` for Rin-A master-2 effect prefab resolution.

### Constraints

- Role Owner is Skill Builder.
- User explicitly approved treating current CSV/code as the parsed source.
- Implementation stayed on the selected projectile blueprint common path and did not add Rin-only hardcoded runtime logic.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-A trait 5 applies critical chance/damage bonuses.
- User verifies in Play Mode that Rin-A master 2 branches with 40% chance normally and 100% chance every 3rd projectile launch.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now includes `branch_launch_period` and `branch_launch_chance_set`.
- `rin-a-trait-5` now has `crit_chance_bonus=0.1`, `crit_damage_bonus=0.25`, blank `damage_multiplier`, and `runtime_support_state=RuntimeImplemented`.
- `rin-a-master-1` remains data-authored with `damage_multiplier=1.12`, `magazine_bonus=6`, and `shot_interval_multiplier=0.8200000000000001`.
- `rin-a-master-2` now has `skill_effect_prefab_path=Assets/Prefab/Skill/Rin/Rin_A.prefab`, `branch_chance_set=0.4`, `branch_count=2`, `branch_damage_multiplier=0.4`, `branch_search_radius=4.5`, `branch_launch_period=3`, `branch_launch_chance_set=1`, and `runtime_support_state=RuntimeImplemented`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `Assets/Prefab/Skill/Rin/Rin_A.prefab` with GUID `19bfba788239eba498a44cb67c2622c6`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-24: User authorized current CSV/code as parsed source for Rin-A master-2, remaining enhancements, and master-1 implementation.

## Task: 2026-05-26 Rin F-J Passive Shared Trigger/Status Implementation

### Task title

Implement Rin passive F-J on shared status/effect/trigger runtime.

### Goals

- Implement Rin-F delayed follow-up attacks through `SingleAttack` trigger rows with `trigger_delay_seconds=0.3`.
- Implement Rin-H as all-allied physical-damage count tracking before triggering auto shockwave rows.
- Keep Rin-G, Rin-I, and Rin-J authored through common status/effect/trigger structures instead of Rin-only hardcoded runtime branches.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User explicitly approved the shared extensions needed for all-allied physical count, trigger action, cooldown/reload reduction, trigger delay, and status/effect conditions.
- Skill Builder CSV reads stayed limited to `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and CSV-validation/build verified.

### Next Actions

- User verifies in Play Mode that Rin-F follow-up uses `Assets/Prefab/Skill/Rin/Rin_F.prefab` after 0.3 seconds.
- User verifies in Play Mode that Rin-H counts all allied physical damage events and fires on the configured 10-hit / 8-hit trait cadence.
- User verifies in Play Mode that Rin-G/I/J passive effects and cooldown/reload reductions match the design sheet intent.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `rin-f-followup`, `rin-f-followup-trait2`, and `rin-f-followup-lightning-trait3` with `trigger_action=SingleAttack`, `event_skill_id=rin-a;rin-c;rin-d;rin-e`, `event_source_scope=owner`, and `trigger_delay_seconds=0.3`; the physical rows use `Assets/Prefab/Skill/Rin/Rin_F.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains Rin-H all-ally physical trigger rows with `trigger_attribute=Physical`, `event_source_scope=all_allies`, and `trigger_every_count=10` or `8` depending on trait 1.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains Rin F-J passive status/effect rows including `rin-i-finishing-kill-crit-damage-trait2`, `rin-j-physical-defense-down`, and `rin-j-hitcount-action-speed`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now supports `trigger_action`, `event_skill_id`, `event_source_scope`, `trigger_delay_seconds`, `trigger_every_count`, effect triggers, cooldown refund, and reload reduction.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now stores passive trigger counts and dispatches skill-cast triggers; `SkillExecutionSystem.cs` dispatches active skill-cast events after routed non-triggered casts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` and `SingleAttackSkillExecutor.cs` now support `OnHitCount` multi-effects for hit-count-gated shared passive effects.
- CSV field-count scan passed: `monster_skill_effects.csv` 64 columns / 91 lines, `monster_skill_triger.csv` 44 columns / 26 lines, `monster_skill_choices.csv` 86 columns / 252 lines, and `monster_skills.csv` 57 columns / 52 lines.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and did not log the prior `unsupported status_target_scope` CSV error.

### History

- 2026-05-26: User approved extending the shared trigger/status runtime, then clarified Rin-H should count all allied physical-damage skill usage before triggering.
