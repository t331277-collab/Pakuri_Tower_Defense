# STATUS_EFFECT_BLACKBOARD

## Archived History

The pre-cleanup file, including all completed July tasks, is preserved at `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.

## Task: 2026-07-28 Skill Trigger / Node Unification Design

### Task title

Design Trigger as the sole activation authority and ordered Nodes as the sole payload authority.

### Goals

- Remove `graph_kind`, Effect runtime ownership, and the removed intermediate terminology after a behavior-preserving migration.
- Route former Effect timing and payload through Trigger-owned Nodes.
- Keep `SkillNode` as one compiled operation container.

### Constraints

- Role Owner is Designer for the handoff and Code Builder refactoring track for later implementation.
- Preserve current damage, status, shield, timing, targeting, delay, repeat, visual, recast, Choice, Passive, and Trigger behavior.
- Do not delete the legacy Effect path before migrated family parity exists.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Design handoff complete. Implementation not started.

### Next Actions

- User approves or revises `boards/COMBAT/SKILL_TRIGGER_NODE_UNIFICATION_HANDOFF.md`.
- Code Builder begins with the new Action Context and Node Executor foundation while retaining the legacy Effect path.

### Evidence

- `SkillEffectDefinition` and `SkillTriggerDefinition` both own timing/conditions/target/payload axes.
- Current Trigger events do not cover all current Effect timings.
- Active authoring contains 508 Effect graph rows and 256 ordinary modifier graph rows.
- Fifteen current C# files reference `SkillEffectDefinition`.
- Full design, deletion surface, final responsibilities, migration phases, parser/validator changes, risks, and acceptance criteria are recorded in `boards/COMBAT/SKILL_TRIGGER_NODE_UNIFICATION_HANDOFF.md`.

### History

- 2026-07-28: User selected Trigger-to-Node as the unified execution direction and removed `graph_kind` plus the rejected intermediate terminology.
- 2026-07-28: Designer created the implementation handoff without changing runtime code or CSV.
- 2026-07-28: Code Builder archived older COMBAT task history and retained this as the only active COMBAT task.

## Task: 2026-07-29 Trigger Visual Duration Restoration

### Task title

Restore the pre-migration lifetime of standalone Trigger visuals.

### Goals

- Add one positive `SetDuration` Node to each of the ten standalone Trigger visual owners that lost its lifetime during Node migration.
- Preserve the prior one-second visual lifetime without adding a runtime fallback.

### Constraints

- Runtime and validator code remain unchanged.
- Damage, targeting, status payload, visual assets, Trigger gates, and existing Node order remain unchanged.
- `Pakuri/reference/2.Monster` remains the gameplay-intent source; the pre-migration runtime is the exact lifetime source.

### Role Owner

Code Builder

### Status

CSV implementation and non-Play-Mode verification complete. User Play Mode verification remains.

### Next Actions

- User verifies that representative Trigger visuals disappear after one second and that damage still occurs exactly once.

### Evidence

- Pre-migration `SkillTrigger`, `ZoneSkillExecutor`, and `SingleSkillExecutor` assigned transient additional-damage visuals a `1f` lifetime.
- The relevant monster references define these ten payloads as instantaneous explosions, reflections, follow-ups, or slashes and do not define a persistent visual duration.
- Ten Trigger owners now each contain exactly one `SetDuration=1` Node.
- All sixteen Trigger owners with `ShowVisual` now have a positive duration or status-owned lifetime; standalone non-positive duration count is zero.
- Unity CSV source validation passed and loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity Console reported zero errors; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with zero errors.

### History

- 2026-07-29: User rejected a runtime zero-duration fallback and directed data-only restoration from the monster references and prior behavior.
- 2026-07-29: Code Builder restored `SetDuration=1` for ten affected Trigger owners.

## Task: 2026-07-29 Eve-E Recast Generation Regression Diagnosis

### Task title

Diagnose the Eve E zone that continues displaying and dealing damage after its intended lifetime.

### Goals

- Distinguish the actual `eve-e-master-1` OnExpire Trigger from the non-Trigger `eve-e-master-2` Choice modifiers.
- Identify why an Eve E recast zone can repeat without reaching its authored generation limit.

### Constraints

- Preserve all authored duration, generation, Trigger, Choice, damage, status, visual, prefab, scene, and generated-catalog values.
- Keep non-lifecycle Trigger events at their existing zero recast generation.
- Every conclusion must follow from current authoring and runtime code.
- Unity Play Mode reproduction remains user-owned.

### Role Owner

Code Builder

### Status

Runtime correction and local non-Play-Mode verification complete. User Play Mode verification remains.

### Next Actions

- User verifies that `eve-e-master-1` creates only one three-second recast and that `eve-e-master-2` creates no recast.

### Evidence

- `eve-e-master-2` owns only `StatusMaxStacksBonus` and `StatusCriticalDamageTakenBonus` Choice Nodes; no Trigger or visual Node with that ID exists.
- `eve-e-master-1` owns the `OnExpire` Trigger and `RecastZone` Node with `max_generation=1`.
- `ZoneSkillActor` publishes OnExpire with its current `recastGeneration`.
- Before the correction, `SkillTrigger.PublishLifecycleEvent` converted the lifecycle context to `TriggerExecutionContext`, which had no recast-generation field.
- Before the correction, `SkillTrigger.TryExecuteOwnedNodes` constructed a new `SkillExecutionContext` without a recast-generation argument, resetting it to zero.
- Before the correction, `ZoneSkillExecutor.ExecuteRecast` therefore saw zero on every expiration and passed the generation guard repeatedly.
- The pre-legacy-deletion path passed `context.RecastGeneration` directly into the next recast actor.
- `TriggerExecutionContext` now stores a non-negative `RecastGeneration`; lifecycle publication copies it from `SkillExecutionContext`, and Trigger-owned Node execution copies it into the next `SkillExecutionContext`.
- Repository-wide authoring inspection found one `RecastZone`: `eve-e-master-1`, with one matching `OnExpire` Trigger, duration 3, and `max_generation=1`; static validation reported zero errors.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with zero errors and the two pre-existing assembly-version warnings.
- Unity script validation, forced compilation, domain reload, and Console inspection completed with zero diagnostics.

### History

- 2026-07-29: User reported that the Eve E master effect remained indefinitely and continued applying damage.
- 2026-07-29: Designer found an ID mismatch in current authoring and a recast-generation loss introduced in the Trigger-to-Node execution path.
- 2026-07-29: User confirmed `eve-e-master-1` and authorized Code Builder correction plus cross-skill verification.
- 2026-07-29: Code Builder restored recast-generation propagation in the common Trigger execution path and verified every authored `RecastZone`.
