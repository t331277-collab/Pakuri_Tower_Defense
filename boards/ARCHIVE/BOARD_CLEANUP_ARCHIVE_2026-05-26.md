# Board Cleanup Archive 2026-05-26

Moved from active boards under COMBAT, DATA, ENEMY, MON, OPS, RUN, and UI during the 2026-05-26 cleanup request.
Criteria: archive task blocks dated before 2026-05-24 unless retained as current authority, baseline, active rules, or latest monster runtime-completion context.

## Source: boards\COMBAT\ENEMY_BLACKBOARD.md

## Task: 2026-05-23 Spawned Unit Root Registration For Enemy Hurtboxes

### Task title

Register spawned player/enemy units with their spawned root transform so shared collider-contact skills can see real body colliders.

### Goals

- Stop enemy roster entries from exposing only the nested `EnemyUnitActor` transform when body colliders live elsewhere on the spawned unit hierarchy.
- Keep `FindUnitByCollider(...)` able to resolve colliders found on the spawned unit root tree.
- Preserve existing spawn ownership and actor initialization flow.

### Constraints

- Role Owner is Code Builder.
- This task changes runtime registration only; it does not edit enemy CSV or prefab serialization.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies Stage 1 enemies can now be hit by collider-contact skills whose overlap checks previously saw `targetColliders=[]`.
- If a future spawned prefab has a different body-root split, pass the correct spawned root at registration time rather than reintroducing actor-child assumptions.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` now passes `spawnedUnit.transform` into `RegisterPlayer(...)` and `RegisterEnemy(...)` so roster entries know the spawned unit root.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now accepts optional `hitboxRoot` parameters on `RegisterPlayerMonster(...)` and `RegisterEnemy(...)`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs` now stores `HitboxRoot`, resolves target points from that root, caches hurtbox colliders from that hierarchy, and matches colliders through `ContainsTransform(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now uses `UnitRosterEntry.ContainsTransform(...)` inside `FindUnitByCollider(...)`, which lets trigger-based projectile contact resolve colliders on the spawned unit root tree.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the change; existing `MSB3277` warnings remained. One earlier parallel build attempt hit a temporary file-lock on `obj\\Debug\\Assembly-CSharp.dll` before the successful rerun.

### History

- 2026-05-23: Shared contact-hit debugging showed Stage 1 enemies were registered by nested actor transform rather than spawned unit root, so Code Builder corrected the roster registration contract.

## Task: 2026-05-22 Spawn-Time Enemy Action Policy

### Task title

Let spawned enemies move and attack by unit rules instead of waiting for `StageState.Combat`.

### Goals

- Remove the runtime behavior dependency where enemies waited until every encounter row finished spawning before moving.
- Keep enemy movement and attack decisions owned by `EnemyCombatSystem` target, range, status, and cooldown checks.
- Keep Stage flow states as run/reward flow information instead of the enemy action gate.

### Constraints

- Role Owner is Code Builder.
- This task changes runtime execution policy only; no enemy CSV, prefab, or scene serialization was changed.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that enemies begin moving as soon as they spawn and attack once their current target is in range.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` still owns the flow states `Spawning`, `Combat`, and `RewardReady`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now calls `enemyCombatSystem.Tick(...)` whenever `enemyCombatSimulationEnabled` is true, without checking `StageState.Combat`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` remains the owner of enemy target lookup, movement through `MoveToward(...)`, action permission through status checks, and basic/special skill cooldowns.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP script refresh reached idle; console warning/error read showed only MCP client handler logs.

### History

- 2026-05-22: User clarified that enemies should not wait for all spawns before moving. Code Builder removed the `StageState.Combat` gate from the enemy combat tick path so spawned enemies act by their own target/range/cooldown rules.

## Task: 2026-05-18 NewRun Prefix Removal Follow-up

### Task title

Keep enemy spawn references aligned after removing `NewRun` from runtime script names.

### Goals

- Keep enemy spawn manager records aligned with the renamed scene entry type.
- Preserve the existing `EnemySpawnManger` script GUID and scene component reference.

### Constraints

- Role Owner is Code Builder.
- Behavior must remain unchanged.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want runtime spawn behavior confirmation.

### Evidence

- The previous `NewRunSceneEntryManager.cs` script is now `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs`.
- `SceneEntryManager.cs` references `EnemySpawnManger` for configured enemy spawns.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` records `Pakuri.InGame.SceneEntryManager`, `Pakuri.InGame.StageManager`, and `Pakuri.InGame.EnemySpawnManger`.
- Search found no remaining `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunStartContext`, or `NewRunStageState` references in scripts, scene assets, prefab assets, asset files, or `Assembly-CSharp.csproj`.
- Runtime/editor builds passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-18: Code Builder removed the `NewRun` prefix from the remaining `NewRun*.cs` runtime scripts after the earlier `EnemySpawnManger` rename.

## Task: 2026-05-18 Enemy Spawn Manager Rename

### Task title

Rename the current NewRunScene spawn manager script to `EnemySpawnManger`.

### Goals

- Rename the existing spawn manager script and class used by `NewRunScene`.
- Preserve Unity scene/component compatibility by keeping the existing script `.meta` GUID.
- Keep `SceneEntryManager` references compiling against the renamed type.

### Constraints

- Role Owner is Code Builder.
- Requested source file `NewRunStageSpawnManager.cs` was not present in `Pakuri/Assets`; the existing wired spawn manager was the former `Pakuri/Assets/Scripts2/InGame/Core/NewRunUnitSpawnManager.cs`, now `EnemySpawnManger.cs`.
- This was a behavior-preserving rename only.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want to confirm scene component behavior after the Unity script rename.

### Evidence

- The former `Pakuri/Assets/Scripts2/InGame/Core/NewRunUnitSpawnManager.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` and `.meta`.
- `EnemySpawnManger.cs.meta` keeps GUID `fa013f8b8851bec4882efe505f98b801`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` now declares `public sealed class EnemySpawnManger : MonoBehaviour`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now references `EnemySpawnManger`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` still references GUID `fa013f8b8851bec4882efe505f98b801` and now has `m_EditorClassIdentifier: Assembly-CSharp::Pakuri.InGame.EnemySpawnManger`.
- `Pakuri/Assembly-CSharp.csproj` now compiles `Assets\Scripts2\InGame\Core\EnemySpawnManger.cs`.
- Search after the rename found no remaining `NewRunUnitSpawnManager` or `NewRunStageSpawnManager` references in scripts, scene assets, or `Assembly-CSharp.csproj`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings when rerun alone after a parallel-build file-lock attempt.
- Unity-MCP force refresh cleared the stale missing-file compile error; console warning/error read showed MCP client handler logs and a UnityEditor.Graphs `NullReferenceException`, not a C# compile error.

### History

- 2026-05-18: User requested Code Builder rename `NewRunStageSpawnManager.cs` to `EnemySpawnManger.cs`; Code Builder inspected the project, found no `NewRunStageSpawnManager.cs`, and applied the rename to the actual wired spawn manager `NewRunUnitSpawnManager.cs`.

## Source: boards\COMBAT\PROJECTILE_BLACKBOARD.md

## Task: 2026-05-17 InGame Projectile Modifier Execution

### Task title

Extend shared InGame projectiles with modifier-driven fan-out, pierce, status, and branch behavior.

### Goals

- Keep projectile behavior reusable instead of hardcoding Eve-A inside the projectile actor.
- Allow `SkillExecutionSnapshot` modifier fields to affect active projectile behavior.
- Preserve existing enemy projectile calls by keeping the previous `InGameProjectileActor.Initialize(...)` overload.

### Constraints

- Role Owner is Code Builder.
- Branch projectiles do not recursively branch or apply inherited status in this slice.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies projectile fan-out, pierce, and branch visuals in Play Mode.
- If branch visuals need different assets later, add a data field instead of changing the branch actor to an Eve-specific path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:82` applies `AdditionalProjectileBonus` and `:87` resolves branch behavior from the active snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs:60` adds an extended initialize overload for optional status/branch specs while preserving the existing initialize overload used by enemy projectiles.
- `InGameProjectileActor.cs:153` applies status on hit, and `:154` calls branch spawning after damage.
- `InGameProjectileActor.cs:186` through `:307` finds nearby branch targets, spawns branch projectiles, prevents immediate re-hit of the source target, and disables recursive branch/status on branch projectiles.
- Runtime/editor builds completed with 0 errors and existing assembly reference warnings.
- Unity-MCP script refresh reached idle and console warning/error read returned only MCP client handler logs.

### History

- 2026-05-17: New active projectile work resumed for Eve-A step 2, so this board was created per `MDTREE.md`.

## Source: boards\COMBAT\STATUS_EFFECT_BLACKBOARD.md

## Task: 2026-05-23 Shared Choice Status Max-Stack And Zone Conditional Damage Support

### Task title

Extend the shared status and AreaAttack tick runtime so choice snapshots can raise one status cap and apply damage only when a target already meets a status stack threshold.

### Goals

- Let a choice snapshot add max stacks to one specific applied status id instead of editing global status defaults.
- Let AreaAttack tick damage apply an extra multiplier only against targets already carrying a required status at or above a required stack count.
- Keep both behaviors on shared snapshot/runtime paths instead of adding Eve-only status branches.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The implementation must stay on shared status-resolution and zone-tick code paths.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity-refresh/console-checked.

### Next Actions

- Reuse the new targeted max-stack bonus path for future ?쐎nly this status cap increases??designs before editing `status_effects.csv`.
- Reuse the new target-status-threshold damage path for future zone skills that say ?쐀onus damage only when target already has X stacks.??

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs` now stores targeted status max-stack bonuses keyed by status id and shared conditional damage rules keyed by target status id plus minimum stacks.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now applies `ResolveStatusMaxStacksBonus(...)` while building `ProjectileStatusHitSpec`, so master-2-style vulnerable cap increases stay on the shared status-spec path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now resolves tick damage through `ResolveDamageAgainstTarget(...)`, which multiplies damage by the snapshot?셲 shared conditional damage rules before `ApplyDamage(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` still defines vulnerable with default max stacks `10`, so Eve-E master 2?셲 `+5` cap comes from the new choice-owned override rather than a global catalog edit.
- `dotnet build Pakuri\\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the shared runtime change; only the existing `MSB3277` warnings remained.
- Unity console warning/error read after forced refresh returned only MCP client-handler logs, not C# compile or status-parse errors.

### History

- 2026-05-23: Eve-E implementation required a shared way to raise vulnerable max stacks by choice and to gate AreaAttack bonus damage on `vulnerable >= 5`.

## Task: 2026-05-23 Eve-D Shock-Gated SingleAttack Follow-Up

### Task title

Let shared SingleAttack schedule a delayed follow-up only for targets that were already carrying the required status when the first hit landed.

### Goals

- Keep Eve-D base `shock` application on the existing shared status-spec path.
- Let a scoped `SingleAttack` choice require a pre-hit status before a delayed follow-up is registered.
- Keep the delayed follow-up from recursively scheduling another delayed follow-up.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The new behavior must stay on shared SingleAttack runtime code, not on an Eve-only status branch.
- Existing non-follow-up SingleAttack skills must keep their old behavior.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If a future skill wants the same contract, reuse the same shared pre-hit status + delayed follow-up path before adding another special-case runtime branch.
- User verifies that Eve-D trait 4 still adds extra `shock` through the normal status application path while master 1 still requires the target to be shocked before that hit.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` still resolves Eve-D's status application through `ProjectileSkillExecutor.ResolveStatusSpec(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now resolves a `SingleAttackFollowUpSpec` only when the snapshot carries `HasBranchCount`, `HasBranchDamageMultiplier`, `HasBranchSearchRadius`, and a required status id.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` `RegisterFollowUpTarget(...)` now checks `HasStatus(target.Model, followUpSpec.Value.RequiredStatusId)` before damage/status application, which keeps the requirement on the target's pre-hit state.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` `ExecuteConditionalFollowUpAfterDelay(...)` now clones the snapshot with the reduced damage multiplier and calls `ExecuteAtCenter(..., false)`, which reuses shared damage/status logic but blocks recursive follow-up scheduling.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors Eve-D trait 4 with `status_tag=shock` plus `status_stacks_bonus=1`, and Eve-D master 1 with `status_tag=shock`, `branch_count=1`, `branch_damage_multiplier=0.5`, and `branch_search_radius=0.5`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the shared follow-up path was added; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User clarified that Eve-D master 1 should trigger a second hit after `0.5` seconds only if the enemy hit by the first explosion was already in `shock`, and that the second hit must not explode again.

## Task: 2026-05-23 Shared Unit Hurtbox Contract For Contact Skills

### Task title

Route collider-contact skills through a shared unit hurtbox-root contract instead of nested actor-child transforms.

### Goals

- Let collider-contact damage paths query the spawned unit hierarchy that actually owns body colliders.
- Keep prefab-hitbox zone, SingleAttack hitbox, trigger hitbox, and projectile contact checks on one shared overlap rule.
- Leave non-contact skills such as battlefield/radius-only effects and explicit target-designated status skills on their existing logic.

### Constraints

- Role Owner is Code Builder.
- This task changes shared contact-resolution logic only; it does not redefine non-contact targeting.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies collider-contact skills no longer miss because roster entries point at actor-child transforms without colliders.
- If a future unit prefab introduces non-body colliders under the spawned root, narrow the shared hurtbox filter with fresh prefab evidence instead of reverting to actor-child lookups.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs` now records `HitboxRoot`, exposes `GetHitboxColliders()`, and centralizes shared overlap checks in `UnitHitboxUtility.IsTargetInsideHitbox(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `SkillTriggerRuntime.cs` now delegate prefab-hitbox target checks to that shared utility instead of directly calling `target.Transform.GetComponentsInChildren<Collider2D>()`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now uses the shared hurtbox contract on the prefab-hitbox zone path while keeping the older radius branch untouched for non-contact area skills.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` and `InGameEnemySkillHitboxActor.cs` now prefer collider-authoritative roster hit tests when the attacking object has colliders, and fall back to the previous radius-only check only when no collider hitbox exists.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` was intentionally not changed in this task, so line/radius contact rules remain position-based until a separate contract is requested.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the change; existing `MSB3277` warnings remained. One earlier parallel build attempt hit a temporary file-lock on `obj\\Debug\\Assembly-CSharp.dll` before the successful rerun.

### History

- 2026-05-23: Eve-C collider debug proved that the shared contact path was reading actor-child transforms with no colliders, so Code Builder added a spawned-unit-root hurtbox contract for collider-authoritative skills.

## Task: 2026-05-23 AreaAttack Prefab Collider Tick Routing

### Task title

Let shared AreaAttack ticks use prefab collider overlap when the instantiated zone prefab provides a hitbox.

### Goals

- Preserve shared area-status application through `InGameCombatManager.ApplyStatus(...)`.
- Let zone skills with authored collider prefabs route damage/status by collider overlap instead of only by radius distance checks.
- Keep the existing radius-based area tick path as fallback for zone prefabs without colliders.

### Constraints

- Role Owner is Code Builder.
- Keep the implementation generic in the shared `AreaAttack` runtime path.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies collider-authored zone skills such as Eve-C apply chill only to targets that overlap the live prefab collider.
- If a future zone prefab should remain radius-authored even though it has colliders, add an explicit runtime contract before weakening the current shared autodetect path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now detects child `Collider2D` hitboxes, routes repeated ticks through collider overlap checks, and still falls back to the older radius-squared branch when no hitbox exists.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now scales instantiated AreaAttack hitbox prefabs through the existing snapshot radius-multiplier path before `InGameZoneSkillActor.Initialize(...)`.
- Existing shared status application remains on `TryApplyStatus(...)` inside both the collider-overlap and fallback radius branches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the change; existing `MSB3277` warnings remained.

### History

- 2026-05-23: User requested Eve-C to follow prefab collider size like prefab-hitbox SingleAttack instead of staying on a fixed-radius zone path.

## Task: 2026-05-23 Eve-C Prefab Hitbox Debug Logging

### Task title

Instrument the shared prefab-hitbox zone path so Eve-C overlap failures can be diagnosed from runtime logs.

### Goals

- Record which prefab colliders a zone tick is using.
- Record which target colliders are being compared and whether `Distance(...).isOverlapped` is true.
- Keep the debug output constrained to Eve-C while debugging the shared collider-overlap path.

### Constraints

- Role Owner is Code Builder.
- Do not change shared damage or status routing semantics while adding logs.
- Keep the debug gate narrow enough that other zone skills do not spam the console.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User captures `[ZoneHitboxDebug:eve-c]` lines from Play Mode to identify whether the miss is caused by target selection, enemy child collider bounds, or overlap evaluation.
- If the logs show the wrong child collider getting compared, adjust the shared target-collider selection rule with fresh evidence from those logs.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now emits Eve-C-only logs for initialize, tick start/end, target collider collection, collider-pair comparisons, and hit/miss outcomes on the prefab-hitbox branch.
- The shared debug path is gated by `IsDebugSkill(...)` checking `runtime.SkillId` against `eve-c`, so the added instrumentation does not broaden normal shared AreaAttack output.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the logging edit; existing `MSB3277` warnings remained.

### History

- 2026-05-23: After collider-authored AreaAttack routing landed, the user reported visible Eve-C overlap without damage, so Code Builder added live overlap diagnostics to the shared zone hitbox path.

## Task: 2026-05-23 Targeted Status Duration Bonus And Threshold Status Runtime

### Task title

Extend the shared status runtime so choice snapshots can add duration to a selected status id and trigger a second status when a stack threshold is reached.

### Goals

- Let a choice snapshot carry a status-id-specific duration bonus instead of only a skill/zone duration bonus.
- Let status application resolve a shared threshold rule such as `chill >= 4 -> freeze`.
- Keep the rule generic for future stack-threshold status promotions, not only Eve-C.

### Constraints

- Role Owner is Code Builder.
- The implementation must remain on shared status application/runtime paths.
- Unity Play Mode gameplay verification remains user-owned.
- Native `codex review --uncommitted` could not complete because the local review command failed on blocked local/network execution, so the review result is a manual pass over the changed lines plus build evidence.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and manual-review-passed.

### Next Actions

- Future designs that say ?쐎nly freeze duration increases??should use the same targeted status-duration path instead of editing the global `status_effects.csv` default duration.
- Future designs that promote one status into another at a stack threshold should reuse the same threshold-status contract before adding new runtime branches.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillChoiceModifierRecord.cs` now define `StatusDurationBonusStatusId`, `StatusDurationBonus`, `ThresholdStatusId`, `ThresholdStatusMinStacks`, and `ThresholdApplyStatusId`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs` now stores targeted status-duration bonuses and resolves them by status id through `ResolveStatusDurationBonus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now applies those targeted bonuses both to base status specs and to multi-effect-authored status specs, and it builds the threshold follow-up status spec from the active snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs` now checks the target's live stack count after the first status application and applies the configured threshold status through the same shared `ApplyStatus(...)` path.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` already exposed `Statuses.GetStacks(...)`, which the new threshold rule now consumes instead of adding Eve-only state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the runtime change; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User asked for a shared extension rather than a global freeze default-duration edit, and required a generic threshold-status rule for Eve-C master 1.

## Task: 2026-05-22 Dynamic Shield Count And Source-Conditional Damage Runtime

### Task title

Extend the shared combat/status runtime so choice snapshots can count shielded allies and mark statuses can grant incoming-damage bonuses only from shielded attackers.

### Goals

- Let cast-time choice resolution count units on a selected side that currently carry a given status.
- Let applied status data carry a required source status tag plus a conditional incoming-damage bonus.
- Route the live damage path through the new source-target conditional status rule without adding Ariel-specific branches.

### Constraints

- Role Owner is Code Builder.
- The implementation must remain generic for future count-based damage and source-target status-condition skills.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that shield gain/loss immediately changes `ariel-a-trait-5` damage on the next cast because the count is evaluated from live roster status state.
- User verifies in Play Mode that Ariel-D's mark grants the extra damage only from attackers that currently have `shield`, not from unshielded attackers.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:116`, `:216-285` now resolve active choices with `UnitRosterService`, count matching status holders, and apply the resulting dynamic damage multiplier to the `SkillExecutionSnapshot`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:291-337` now clones status data with `ConditionalSourceStatusTag` and `ConditionalDamageTakenBonus` overrides from the active snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:234-246`, `:366-374` now evaluate target-side incoming-damage bonuses against the live attacker source and the required source status tag.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:965-1011` now passes `options.Source` into the final damage resolution and into `StatusEffectRuntime.ResolveIncomingDamageMultiplier(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs` now classifies positive conditional incoming-damage bonuses as debuff-like status payloads so harmful-source rules stay routed through the shared helper.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the last two Ariel rows that still required additional shared common logic.

## Task: 2026-05-22 Passive Choice Gating, Ailment Resistance, And Flat Resist Runtime

### Task title

Extend the shared status runtime so passive-choice-gated Ariel effects can apply crit chance, ailment resistance, and flat Holy resistance reduction through CSV-owned rows.

### Goals

- Let passive effect rows see the owner's chosen passive choices.
- Let shield or effect-authored statuses grant ailment resistance and reduce harmful status application chance.
- Let status effects add crit chance and flat element-resistance reduction through the shared damage/status path.

### Constraints

- Role Owner is Code Builder.
- The implementation must remain generic for future passive-choice and resistance-based skills.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly told Builder not to run Reviewer.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies `ariel-b-master-1` ailment resistance on shielded allies, `ariel-f-trait-3` crit chance on Holy-skill allies, and `ariel-i-trait-3` flat Holy resistance reduction on Holy Exposure targets.
- If future designs need ailment resistance to affect non-skill status sources, route those sources through the same shared application helper before adding new per-skill handling.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGamePassiveEffectRuntime.cs:56-106` now builds a `SkillExecutionSnapshot` from chosen passive choices before executing passive effect rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1439-1550` now supports `condition_skill_attribute`, active-skill-attribute checks, and runtime status payload mapping for crit chance, ailment resistance, and flat element resistance reduction.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:222-262` now resolves shared crit chance bonus, flat element resistance reduction, and ailment resistance bonuses from active statuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs:18-48` now subtracts `ResolveAilmentResistanceBonus(...)` from harmful status application chance instead of ignoring runtime resistance modifiers.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:946-988` now routes final damage through flat element-resistance reduction before incoming-damage modifiers and crit resolution.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the Ariel rows that were previously classified as CSV-only or needing a small shared runtime contract.

## Task: 2026-05-22 Shield Absorb, Status Expire, Crit Damage, And Duration Extension Runtime

### Task title

Extend the shared combat/status runtime for shield-absorb triggers, status-expire triggers, crit-aware skill damage, tracked incoming damage, and status-duration extension.

### Goals

- Dispatch reusable `OnShieldAbsorb` triggers with attacker, shield owner, and absorbed amount context.
- Dispatch reusable `OnStatusExpire` triggers for non-shield statuses.
- Record tracked incoming damage on active statuses so expiry bursts can resolve from stored totals.
- Apply critical chance/damage in the live InGame damage path instead of leaving crit fields data-only.
- Provide a shared runtime API to extend active status durations, including shield statuses.

### Constraints

- Role Owner is Code Builder.
- The work must remain reusable for future non-Ariel skills.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly said not to run it.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Future absorb-reflect or mark-expiry skills should add CSV rows against these shared contracts instead of introducing skill-specific runtime branches.
- If future designs need tracked damage by additional dimensions beyond `DamageAttribute`, extend the shared tracker structure before adding more trigger-specific logic.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:11-13` defines `DamageApplicationOptions`; `:135-140` collects absorbed shield records and dispatches shield-absorb triggers; `:271-277` adds shared `ExtendStatusDuration(...)`; `:571` dispatches status-expire triggers; `:834-849` records incoming damage before shield consumption; `:958-962` resolves crit-aware final damage.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` now stores per-status tracked incoming damage, exposes `ExtendDuration(...)`, and adds `ConsumeShield(...)` / `RecordIncomingDamage(...)` support used by the combat manager.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs:82-133` adds `ExecuteShieldAbsorb(...)` and `ExecuteStatusExpire(...)`; `:390-396` resolves `ShieldAbsorbedAmount` and `TrackedIncomingDamage`; `:515-518` prefers the event target when trigger targeting requires it.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now forwards crit settings/source through projectile, zone, line, prefab-hitbox, and limited-target damage application paths, and `SkillMultiEffectExecutor` now supports `ExtendStatusDuration`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs`, `InGameLineAttackActor.cs`, and `InGameZoneSkillActor.cs` now carry critical configuration through their shared damage-application calls.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the previously proposed reusable runtime support for Ariel shield reflection, mark expiry burst, mark crit amplification, target-count bonus use, and ally shield-duration extension.

## Task: 2026-05-22 Shield Expiry Trigger Dispatch

### Task title

Dispatch shield-expiry trigger skills from shared status runtime.

### Goals

- Preserve shield source unit and source definition on shield statuses so expiry effects can resolve the caster.
- Dispatch `OnShieldExpire` when shield statuses end by duration or are fully consumed by damage.
- Route Ariel-B trait 4 through the same generic trigger runtime as other CSV trigger rows.

### Constraints

- Role Owner is Code Builder.
- Keep shield expiry dispatch generic in status/combat runtime; no Ariel-only branch.
- Shield status UI/Play Mode behavior must be verified by the user in Unity.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies `ariel-b-trait-4` for both natural shield timeout and full shield depletion.
- Inspect future shield-reflection or absorb-trigger requests separately because they need different event payload semantics than `OnShieldExpire`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:100-104` collects fully depleted shield statuses during damage and dispatches shield-expiry triggers.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:198-225` records shield status source metadata through the `ApplyShieldStatus(..., source)` path.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:489-499` collects duration-expired statuses during status ticking and dispatches shield-expiry triggers.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:164-179` adds a status tick overload that returns removed statuses, and `:275-291` adds a shield consume overload that returns fully depleted shield statuses.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:401` stores shield source unit/definition metadata.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs:36` handles `ExecuteShieldExpire(...)`, and `:334-384` applies prefab-hitbox damage to overlapped targets.
- Runtime and editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User asked to implement `ariel-b-trait-4` as an `OnShieldExpire` trigger skill using a SingleAttack-style prefab hitbox.

## Task: 2026-05-22 Ariel Status Duration And Shield Snapshot Runtime

### Task title

Apply choice status duration modifiers and shield snapshot modifiers through shared runtime paths.

### Goals

- Let choice `duration_bonus` affect status duration for status-applying skills such as Ariel-D.
- Let shield skills use choice damage/duration modifiers for shield amount and shield duration.
- Let shield skills run generic multi-effect rows after successful shield application.

### Constraints

- Role Owner is Code Builder.
- Keep implementation generic in `SkillExecutors.cs`; no Ariel-only executor branch.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies Ariel-B trait 1/2/5 and master 1 shield behavior plus Ariel-D trait 3 duration in Play Mode.
- Keep event-trigger shield effects out of CSV until a shared trigger contract exists.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:229-247` now adjusts resolved status duration by `SkillExecutionSnapshot.DurationMultiplier` and `DurationBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1701-1718` now resolves shield amount/duration through snapshot modifiers.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1757-1764` now runs `SkillMultiEffectExecutor.Execute(...)` for routed shield skills.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1809-1820` now lets `ResolveShield(...)` apply snapshot base-damage and damage-multiplier modifiers.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:25-26` encode Ariel-D Holy Exposure bonus and duration support.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:10-15` encode Ariel-B shield modifier support state and values.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User asked Code Builder to implement CSV-only items first, followed by CSV plus small shared runtime extensions.

## Task: 2026-05-22 SingleAttack Multi-Effect Routing And Visual Spam Guard

### Task title

Treat successfully applied SingleAttack support multi-effects as routed and avoid spawning base visuals when no target/effect routes.

### Goals

- Stop failed SingleAttack executions from repeatedly creating visuals without cooldown.
- Let support effects such as all-ally shield/buff rows count as a routed skill execution when they actually apply.
- Keep multi-effect visuals spawned only after their damage/status effect has a routed target.
- Preserve shared status and shield application through `InGameCombatManager.ApplyStatus(...)` / `ApplyShieldStatus(...)`.

### Constraints

- Role Owner is Code Builder.
- The implementation stays generic in `SkillExecutors.cs`; no Ariel-only executor branch was added.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Ariel-C buff visuals and Ariel-E shield visuals no longer repeat every frame when the skill cannot legitimately execute.

### Evidence

- Before this task, `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:637` instantiated SingleAttack prefab hitbox visuals before confirming routed damage, and `SkillExecutionSystem.cs:132-134` only started cooldown when the executor returned Routed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:689-699` now ORs SingleAttack damage/hitbox routing with `SkillMultiEffectExecutor.Execute(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1013-1043` now returns whether any multi-effect routed or was scheduled.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1231-1250` spawns damage multi-effect visuals only after `ApplyAreaTick(...)` routes.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1254-1331` returns routed status effects only when at least one target received the status/shield and then spawns the matching visual.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User described `ariel-c` / `ariel-e` SingleAttack failures repeatedly creating buff/shield visuals without cooldown. Code Builder changed the shared SingleAttack/multi-effect routing contract so applied support effects start recovery and unrouted effects do not spawn visuals.

## Task: 2026-05-22 StatusEffectKind Mojibake Alias Cleanup

### Task title

Remove broken-encoding defensive status parse aliases from `StatusEffectKind`.

### Goals

- Keep supported status parsing on canonical ASCII IDs and normal Korean labels.
- Stop accepting mojibake strings as hidden compatibility aliases.
- Preserve current runtime status kinds and display names.

### Constraints

- Role Owner is Code Builder.
- Scope is limited to `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs`.
- This task does not remove normal Korean aliases such as `媛먯쟾`, `諛⑹뼱留?, or `?좎꽦 ?몄텧`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If the project later chooses ID-only status parsing, first enforce populated `status_effect_id` in CSV validation, then remove normal Korean label aliases.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` no longer contains mojibake alias cases such as `揶쏅Ŋ??, `?酉??, `?醫롪쉐 ?紐꾪뀱`, `燁삘뫀叫`, or `筌뤾퀣沅????`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` still keeps canonical IDs like `shock`, `shield`, `holy-exposure`, and normal Korean aliases like `媛먯쟾`, `諛⑹뼱留?, `?좎꽦 ?몄텧`.
- `Select-String` over `StatusEffectKind.cs` for the removed mojibake marker patterns only matched the normal C# conditional expression line in `BuildDisplaySuffix`, not a status parse alias.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: User asked Code Builder to remove all broken-encoding defensive strings from `StatusEffectKind.cs`.

## Task: 2026-05-22 Passive Buff And Shield Received Runtime

### Task title

Support passive buff statuses and shield-received modifiers in the shared status runtime.

### Goals

- Add a generic status kind for passive aura-style buffs that should not collide with Ariel-C `blessing` conditions.
- Let status modifiers increase shield amounts received by a target.
- Keep Holy damage, action speed, and incoming damage passive bonuses on the existing status modifier path.

### Constraints

- Role Owner is Code Builder.
- `blessing` remains reserved for authored blessing effects; passive aura rows use `passive-buff`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If passive buff status labels clutter combat UI, add a CSV/runtime display-hiding flag instead of reusing `blessing`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now defines and parses `StatusEffectKind.PassiveBuff` with id `passive-buff`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now includes `passive-buff` as a generic `Buff` row.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` now includes `BuffModifierSpec.ShieldReceivedBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now includes `ResolveShieldReceivedMultiplier(...)` and includes `ShieldReceivedBonus` in modifier magnitude.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` copies `SkillEffectDefinition.StatusShieldReceivedBonus` into created status data.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` applies the shield-received multiplier inside `ApplyShieldStatus(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: Ariel-G required all allies to receive `+18%` shield amount, so Code Builder added a generic shield-received status modifier instead of Ariel-specific shield code.

## Task: 2026-05-22 Multi-Effect Buff Stat Runtime

### Task title

Support multi-effect CSV buffs for action speed, spell power, and outgoing element damage.

### Goals

- Let `monster_skill_effects.csv` apply ally status effects through the shared status runtime.
- Add spell-power bonus support to runtime status modifiers.
- Add outgoing element damage bonus support for shielded-ally Ariel-C trait 5.
- Let multi-effect status rows play attached visuals on the units that actually received the status, without changing the status application target.

### Constraints

- Role Owner is Skill Builder.
- Status application remains routed through `InGameCombatManager.ApplyStatus(...)`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder

### Status

Implemented and locally validated by build plus direct CSV reference checks.

### Next Actions

- Future outgoing damage buffs should use the shared status modifier path rather than skill-specific damage branches.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` now adds `BuffModifierSpec.SpellPowerBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now adds `ResolveSpellPowerMultiplier(...)` and `ResolveOutgoingDamageMultiplier(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now applies spell-power status multipliers when resolving `StatSource.Intelligence` and applies outgoing element damage multipliers in `ResolveDamage(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:896` collects successfully applied status targets when `visual_anchor_mode=AppliedTargets`; `:938` routes those targets into attached visual spawning; `:1163` creates `InGameAttachedSkillEffectActor` instances on each target transform.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1108` resolves visual centers through `SkillMultiEffectCenterMode`, including `PrimarySkillCenter`, `Caster`, and `NearestEnemy`, so status target selection is no longer forced to double as the visual center.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` has Ariel-C rows with `status_action_speed_bonus=0.12`, trait 2 rows with `0.06`, master 1 rows with `status_spell_power_bonus=0.18`, and trait 5 rows with `status_damage_bonus_rate=0.1` scoped to Holy and conditioned on `shield`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` has Ariel-C status rows with `target_side=AllAllies` and `visual_anchor_mode=AppliedTargets`, so the buff applies to allies and the requested `Ariel_C-Buff.prefab` attaches to affected ally units.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- 2026-05-22 follow-up runtime/editor `dotnet build` commands passed with 0 errors after the applied-target visual extension; Unity console warning/error read showed only MCP client handler logs.

### History

- 2026-05-22: Skill Builder added shared status modifier support required by Ariel-C multi-effect rows.
- 2026-05-22: Code Builder separated multi-effect status targets from visual anchors and made applied-target status visuals attach through `InGameAttachedSkillEffectActor`.

## Task: 2026-05-21 Eve-A Recursive Branch Shared Shock Path

### Task title

Keep Eve-A recursive branch hits on the same shared projectile status path as the parent hit.

### Goals

- Ensure a branch projectile can still apply Eve-A shock through the shared projectile status helper.
- Ensure recursive branch hits reuse the shared projectile branch contract instead of adding a second Eve-only shock or branch path.
- Keep the branch damage falloff and branch chance tuning owned by the current choice CSV rows.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay inside the current shared projectile/status runtime.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that branch-generated hits still show the same shock application behavior as the base Arc Bolt hit.
- If a later status rule needs branch-only behavior, extend the shared projectile status spec first instead of forking Eve-A logic.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` still applies projectile statuses only through `TryApplyStatus(...)`, and branch children are now initialized with the same `statusOnHit` spec instead of `null`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` now passes `branchOnHit.CloneForChild()` into branch child initialization, so recursive branch hits continue through the same shared branch/status path without sharing transient branched-target state.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now keeps Eve-A branch tuning in choice data with `eve-a-trait-5 branch_damage_multiplier=0.7` and `eve-a-master-1 branch_damage_multiplier=0.7`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the edit; existing MSB3277 warnings remained.

### History

- 2026-05-21: Code Builder changed the shared projectile actor so Eve-A branch hits inherit status and branch specs, which keeps recursive branch shock on the common status path.

## Task: 2026-05-20 Shield And Buff Source-Aware Status Runtime

### Task title

Implement the shield/buff unification blueprint on the shared runtime status path.

### Goals

- Move player-skill shield application from raw `CurrentShield += amount` authority to timed status instances with mutable absorb payload.
- Make buff/shield merge identity source-aware by `status kind + source skill + merge policy`.
- Keep same-source refresh and different-source coexistence grounded in CSV-owned fields.
- Remove the hardcoded shield-duration fallback from runtime execution.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and locally verified by build, editor-side deterministic execution, and CSV runtime sync.

### Next Actions

- User verifies in Play Mode that Ariel-B shield lifetime and VFX lifetime match the CSV duration in live combat.
- If reviewer permission is given later, run the enforced Builder -> Reviewer flow instead of treating prompt memory as completion.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:179-202` adds `ApplyShieldStatus(...)` so shield now enters combat through the shared status path instead of only through raw resource grant.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:533-539` consumes `target.Statuses.ConsumeShield(finalDamage)` before direct shield and health, and `:653-673` derives `CurrentShield` from `DirectShield + timed shield`.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:118-153` merges source-aware statuses by `SourceSkillId` and `MergePolicy`; `:245-277` sums and consumes timed shield instances; `:347-455` stores mutable shield payload including `MergePolicy`, `RemainingShieldAmount`, and refresh behavior.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:850-875` resolves shield duration from `skill.ShieldDuration` or `skill.ShieldStatus.Duration` and applies shield through `context.CombatManager.ApplyShieldStatus(...)`; the previous hardcoded `5f` fallback path is gone.
- Unity editor `execute_code` returned `shieldAfterDamage=6;healthAfterDamage=100;sameSourceShieldCount=1;sameSourceShieldRemaining=8;differentSourceShieldCount=2;totalShieldAfterDifferentSource=15;sameSourceBuffCount=1;differentSourceBuffCount=2;totalBuffStacks=2;expiredShieldCount=0;shieldAfterExpire=0`, which proves same-source refresh, different-source coexistence, timed shield consumption, and timed shield expiration on the edited runtime classes.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `System.Net.Http` / `System.IO.Compression` MSB3277 warnings remained.

### History

- 2026-05-20: Code Builder implemented the blueprint by extending status identity, moving timed shield ownership into the shared status runtime, and synchronizing `CurrentShield` from active runtime state.
- 2026-05-20: Initial editor sync exposed two real data issues during verification: `monster_skills.csv` had not yet been reimported into Unity, and `status_effects.csv` shield row had a broken quote that collapsed row 10 to 2 columns.
- 2026-05-20: After asset refresh plus the `status_effects.csv` quote fix, `Pakuri/Sync CSV Runtime Catalog Assets` completed successfully.

## Task: 2026-05-20 Shield And Buff Source-Aware Status Unification Design Handoff

### Task title

Prepare a Code Builder handoff for converting timed ally shield/buff skills onto a source-aware runtime status model.

### Goals

- Ground the requested shield/buff redesign in inspected runtime and CSV evidence.
- Record why shield cannot stay on the current raw `GrantShield += amount` path.
- Hand Code Builder one implementation contract for CSV schema, runtime identity, shield payload, and verification expectations.

### Constraints

- Role Owner is Designer.
- This task creates a handoff document only; it does not implement runtime code.
- Unity Play Mode verification remains user-owned.
- The handoff must stay grounded in inspected files and current runtime behavior.

### Role Owner

Designer

### Status

Implementation handoff written for Code Builder.

### Next Actions

- Code Builder reads `boards/SkillBluePrint/shield-buff-status-unification-blueprint.md` before implementation.
- If Builder changes shield canonical id ownership or runtime identity names, record that exact migration choice here when implementation lands.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:840-845` shows shield duration currently falls back to hardcoded `5f`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:862` applies shield through `GrantShield(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:514-535` stores shield only as `CurrentShield`, with no duration/source tracking.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:114` stores statuses by `StatusEffectKind` only.
- `boards/SkillBluePrint/shield-buff-status-unification-blueprint.md` contains the current Builder contract for the redesign.

### History

- 2026-05-20: User asked whether shield should be buff/status-unified with per-skill merge rules.
- 2026-05-20: Designer confirmed the direction is viable but requires a source-aware runtime identity model and a mutable shield payload, then wrote the Builder handoff blueprint.

## Task: 2026-05-20 Ariel-A Master 2 Holy Exposure Shared Status Use

### Task title

Activate Ariel-A master 2 through the shared Holy Exposure status path, including a choice-level Holy damage taken override.

### Goals

- Reuse the shared `StatusEffectKind.HolyExposure` parse/display contract for Ariel-A master 2.
- Confirm that a choice-only status can apply without a base skill status row.
- Confirm that a choice-only status can override its own incoming Holy damage multiplier without changing the shared catalog row for every other user.
- Keep the shared executor rules explicit for future active choice debuffs.

### Constraints

- Role Owner is Code Builder.
- No gameplay verification was run by Codex.
- This task did not add a new status kind; it reused the current working-tree Holy Exposure support.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Confirmed and activated through existing shared status runtime with choice-level override support.

### Next Actions

- Future choice-only debuffs should populate `status_tag`, set stacks explicitly when deterministic one-stack behavior is required, and use choice-level override fields when one status kind needs different values per skill.
- If another debuff still appears missing in gameplay, inspect the active choice state before adding new runtime status code.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:191-241` shows the shared status resolver prefers `snapshot.StatusTag`, defaults missing base status chance to `1f`, applies `StatusStacksSet`, and clones the resolved status data when a choice-specific `StatusElementDamageTakenBonus` override is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:115-117` parses `holy-exposure` and `?좎꽦 ?몄텧`; `:174-175` defines the shared display label `?좎꽦 ?몄텧`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps `ariel-a` with no base status, which makes `ariel-a-master-2` a choice-only shared status case.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `ariel-a-master-2` `status_tag=holy-exposure`, `status_stacks_set=1`, and `status_element_damage_taken_bonus=0.15`.

### History

- 2026-05-20: User asked Code Builder to apply the previously explained Ariel-A master 2 Holy Exposure fix through the shared status path.
- 2026-05-20: User then required per-skill values, so Code Builder extended the shared status path with a choice-level `StatusElementDamageTakenBonus` override and set Ariel-A master 2 to `0.15`.

## Source: boards\DATA\DATA_BLACKBOARD.md

## Task: 2026-05-23 Eve-E Choice CSV Schema And Row Authoring

### Task title

Extend the shared choice CSV schema so Eve-E can author vulnerable max-stack bonuses and target-status-gated damage without Eve-only hardcoded rows.

### Goals

- Keep Eve-E choice behavior data-owned in `monster_skill_choices.csv`.
- Add generic choice columns for targeted status max-stack bonuses and conditional target-status damage multipliers.
- Re-author Eve-E rows so no Eve-E trait/master row remains partial or unsupported after the shared runtime extension.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV files were explicitly treated as the parsed source for this task.
- No new Eve-only companion CSV table was added; the work stays inside `monster_skill_choices.csv`.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Reuse `status_max_stacks_bonus_status_id` plus `status_max_stacks_bonus` for future choice-driven status-cap increases before adding another schema.
- Reuse `conditional_damage_multiplier` plus `conditional_target_status_id` / `conditional_target_status_min_stacks` for future hit-time target-threshold damage bonuses before adding skill-id-specific columns.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `status_max_stacks_bonus_status_id`, `status_max_stacks_bonus`, `conditional_damage_multiplier`, `conditional_target_status_id`, and `conditional_target_status_min_stacks` to the active choice schema.
- Eve-E rows in `monster_skill_choices.csv` are now all `RuntimeImplemented`; trait 1 keeps `magazine_bonus=1`, trait 4 keeps `reload_time_multiplier=0.76923` plus `branch_count=1`, master 1 keeps `shot_interval_multiplier=0.76923` plus `branch_count=2`, trait 5 now authors `conditional_damage_multiplier=1.4` gated by `vulnerable >= 5`, and master 2 now authors `status_critical_damage_taken_bonus=0.01` plus `status_max_stacks_bonus_status_id=vulnerable` and `status_max_stacks_bonus=5`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate those new choice columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Skills/Data/SkillChoiceEffectSpec.cs`, `Skills/Execution/SkillChoiceModifierRecord.cs`, and `Skills/Execution/SkillExecutionSnapshot.cs` now carry the new fields into runtime choice snapshots.
- `Import-Csv -Encoding UTF8 Pakuri\\Assets\\CSVdata\\source\\monster_skill_choices.csv | Where-Object { $_.skill_id -eq 'eve-e' }` returned all seven Eve-E choice rows with `runtime_support_state=RuntimeImplemented`.
- `dotnet build Pakuri\\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the schema and row-authoring change; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User asked Skill Builder to implement Eve-E from the current CSV/code as parsed source, which required new shared choice columns for the last unsupported Eve-E behaviors.

## Task: 2026-05-23 Eve-D Choice Payload On Existing CSV Fields

### Task title

Author Eve-D on current CSV/runtime authority by reusing existing choice fields instead of adding new SingleAttack follow-up columns.

### Goals

- Keep Eve-D base tuning on the existing `monster_skills.csv` row.
- Keep cooldown reduction on the existing `cooldown_multiplier` field.
- Reuse existing choice fields for the scoped delayed follow-up payload needed by Eve-D master 1.
- Avoid introducing new CSV columns for a one-skill exception while the current parser requires strict row-width alignment.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV and code were explicitly treated as the parsed source for this task.
- No new columns were added to `monster_skill_choices.csv`, `monster_skill_effects.csv`, or `monster_skill_triger.csv`.
- `branch_search_radius` is reused here as delay seconds only on the new shared SingleAttack follow-up interpretation path; this was not generalized into a new schema name in this task.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If another SingleAttack later needs the same delayed status-gated follow-up, author it on the same existing fields and cite this task rather than adding duplicate schema.
- If a future design needs both real branch-search radius and delayed follow-up timing on the same SingleAttack contract, revisit the schema with fresh parsed-source evidence before overloading more fields.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps Eve-D on one base row with `runtime_kind=SingleAttack`, `cooldown_seconds=7`, and `status_effect_id=shock`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `eve-d-trait-3` with `cooldown_multiplier=0.8`, which keeps cooldown reduction on the shared cooldown field instead of a new column.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `eve-d-master-1` with `status_tag=shock`, `branch_count=1`, `branch_damage_multiplier=0.5`, `branch_search_radius=0.5`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` still has no Eve-D effect rows, and `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` still has no Eve-D trigger rows, so this implementation stayed on the existing base/choice tables.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now consumes that existing choice payload in `ResolveFollowUpSpec(...)` and `ExecuteConditionalFollowUpAfterDelay(...)` instead of requiring schema expansion.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the Eve-D CSV-authoring pass; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User told Skill Builder to implement Eve-D from the current CSV/code as parsed source and explicitly required cooldown reduction to keep using existing cooldown CSV fields rather than inventing a new cooldown-decrease schema.

## Task: 2026-05-23 Zone Prefab Radius Multiplier Interpretation

### Task title

Keep Eve-C zone scaling on `radius_multiplier` while moving hit detection to prefab colliders.

### Goals

- Preserve current CSV authority without adding new Eve-C schema.
- Keep `radius_multiplier` as the authored scaling input for collider-based zone prefabs.
- Avoid repurposing `radius_bonus` for the requested `1.3 => 30% larger` behavior.

### Constraints

- Role Owner is Code Builder.
- No CSV source row or header was changed in this task.
- `radius` remains in the schema because other shared paths and effect rows still use it as data.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented in runtime interpretation only; no CSV content change was required.

### Next Actions

- Future Eve-C-style prefab growth should use `radius_multiplier`, not `radius_bonus`.
- If the project later wants to remove `radius` from AreaAttack schema entirely, inspect effect rows and non-hitbox fallback paths first instead of deleting it from current CSV blindly.

### Evidence

- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.radius_bonus -and $_.radius_bonus.Trim() -ne '' }` returned no authored runtime rows, so the current active data does not require a `radius_bonus=1.3` reinterpretation.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now scales collider-backed AreaAttack prefabs through the existing snapshot scale-factor path, which is driven by current choice snapshot radius scaling.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` still contains rows such as `eve-c-master2-expire-burst` that use `radius`, so the shared schema field was not removed in this task.
- No CSV file under `Pakuri/Assets/CSVdata/source/` was edited for this change.

### History

- 2026-05-23: User approved the Eve-C prefab-collider implementation and explicitly said not to use `radius_bonus=1.3` for the scaling behavior.

## Task: 2026-05-23 Eve-C Choice And Effect CSV Schema Follow-Up

### Task title

Extend the shared skill CSV schema so Eve-C can author targeted status-duration bonuses, threshold-status promotions, and an OnExpire burst through data-owned rows.

### Goals

- Add choice columns for targeted status-duration bonuses and threshold-status promotions.
- Re-author Eve-C trait/master rows on those generic columns instead of changing global status defaults.
- Add the Eve-C master 2 OnExpire effect row to `monster_skill_effects.csv`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- CSV remains the source of truth for Eve-C tuning; runtime code only adds generic consumers for the new columns.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified. Unity-MCP sync/console calls timed out during this task, so runtime catalog evidence for the new prefab path was recorded from the serialized asset catalog file rather than a fresh sync log.

### Next Actions

- Keep future ?쐀onus duration for one status only??skills on `status_duration_bonus_status_id` plus `status_duration_bonus`.
- Keep future ?쏼 stacks of A immediately applies B??skills on `threshold_status_id`, `threshold_status_min_stacks`, and `threshold_apply_status_id`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:1-2` now includes `status_duration_bonus_status_id`, `status_duration_bonus`, `threshold_status_id`, `threshold_status_min_stacks`, and `threshold_apply_status_id`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate those five columns.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `eve-c-trait-5` with `freeze +1.0s`, `eve-c-master-1` with `freeze +1.5s` plus `chill >= 4 -> freeze`, and `eve-c-master-2` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors `eve-c-master2-expire-burst` with `OnExpire`, `Ice`, `24` base damage, `1.5` spell coefficient, `requires_active_choice_id=eve-c-master-2`, and `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.monster_id -eq 'eve' -and $_.skill_id -eq 'eve-c' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned no rows.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_effects.csv | Where-Object { $_.effect_id -eq 'eve-c-master2-expire-burst' }` returned the authored OnExpire damage row.

### History

- 2026-05-23: User rejected a global `freeze` duration edit and asked for a shared choice-snapshot extension plus a data-owned Eve-C master-2 expire burst row.

## Task: 2026-05-22 Ariel Dynamic Choice Count And Conditional Status CSV Schema

### Task title

Extend the shared CSV schema so Ariel's last two choice rows stay data-owned through generic count-based and source-conditional status fields.

### Goals

- Add choice fields for dynamic per-cast status counting and per-count damage scaling.
- Add choice/status fields for source-status-gated incoming-damage bonuses on applied statuses.
- Keep the supporting schema in `monster_skill_choices.csv` and `monster_skills.csv` aligned with the current parser contract.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for authored Ariel choice behavior; runtime code only adds generic consumers for the new columns.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and CSV-sync-verified.

### Next Actions

- Future ?쐁ount allies with X status??or ?쐔arget status grants bonus only from attackers with Y status??designs should use these same fields before adding new skill-specific schema.
- Keep `monster_skills.csv` type/header rows aligned with parser-required status payload columns whenever shared status fields are added.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:1-2` now includes `count_status_id`, `count_target_side`, `damage_multiplier_per_count`, `count_max`, `status_conditional_source_status_id`, and `status_conditional_damage_taken_bonus`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:7` and `:28` now author `ariel-a-trait-5` and `ariel-d-trait-5` on those generic fields and mark both rows `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1-2` now matches the shared status payload parser by carrying `status_ailment_resistance_bonus` and `status_flat_element_resist_reduction` in both the header row and the type-description row.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now parse, map, and validate the new choice/status columns.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs` now carry the new fields from CSV definitions into runtime snapshots.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.monster_id -eq 'ariel' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned no rows after this schema pass.
- Unity-MCP console after clear plus `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-22: User asked Code Builder to finish the remaining Ariel choice rows by adding whatever shared common logic was still missing.

## Task: 2026-05-22 Ariel Passive Choice/Resistance CSV Schema Follow-Up

### Task title

Extend the CSV/runtime schema so Ariel passive-choice follow-up rows and shield ailment-resistance rows stay data-owned.

### Goals

- Add CSV fields for `condition_skill_attribute`, `status_ailment_resistance_bonus`, `status_flat_element_resist_reduction`, and `status_critical_chance_bonus`.
- Carry passive choice `status_ailment_resistance_bonus` through choice parsing, build, mapping, and runtime snapshots.
- Re-author the Ariel rows that became supported through these shared fields and record the reduced unsupported set.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for skill authoring; runtime code only adds generic consumers for the new columns.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly told Builder not to run Reviewer.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Keep future ?쐆as Holy active skill??or ?쐓tatus grants ailment resistance??designs on these same fields instead of introducing skill-ID-specific columns.
- Ariel choice rows still unsupported after this pass are only `ariel-a-trait-5` and `ariel-d-trait-5`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:15`, `:40-43`, and `:46-50` now mark the newly supported Ariel rows as `RuntimeImplemented`, and `ariel-b-master-1` stores `status_ailment_resistance_bonus=0.3`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:15`, `:25`, `:27`, `:29-30`, `:33-34`, and `:37` now author the Ariel follow-up rows that rely on the new condition/resistance/crit schema.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.StatusPayload.cs:25-64` now parses `status_ailment_resistance_bonus`, `status_flat_element_resist_reduction`, and `status_critical_chance_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:135-162`, `:350-378` now parse choice-level ailment-resistance overrides and effect-level `condition_skill_attribute`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:265`, `:431-432`, `:481-517` now map those parsed fields into runtime definitions.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:198-203`, `:265-266`, `:314-317`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs:58-59`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs:59-60`, `:182-185`, `:252-253` now carry the new fields through definition and snapshot layers.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only `ariel-a-trait-5` and `ariel-d-trait-5`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the Ariel rows previously classified as CSV-only or requiring only a small shared contract.

## Task: 2026-05-22 Ariel Choice/Trigger Schema Extension For Shared Runtime Effects

### Task title

Extend CSV/runtime schema so Ariel's remaining active/master effects stay data-owned on shared runtime contracts.

### Goals

- Add choice fields for target-count bonus, crit chance bonus, crit damage bonus, and status critical-damage-taken bonus.
- Add trigger support for tracked attribute payload and the new absorb/expire trigger contracts.
- Add multi-effect support for status-duration extension.
- Keep Ariel's new reflection, crit-mark, mark-expiry, and shield-duration rows authored in CSV rather than hardcoded by skill ID.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for these skill-authoring changes.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly said not to run it.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and coverage-audited.

### Next Actions

- Future skills that need absorb reflection, expiry bursts, or duration extension should add CSV rows against these shared fields first.
- Ariel still has unsupported or partial choice rows outside this schema slice: `ariel-a-trait-5`, `ariel-b-master-1`, `ariel-d-trait-5`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-h-trait-3`, `ariel-i-trait-1`, `ariel-i-trait-2`, `ariel-i-trait-3`, and `ariel-j-trait-1`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:16`, `:27`, `:29-30`, and `:35` now encode Ariel rows that use `hit_target_count_bonus`, `status_critical_damage_taken_bonus`, and the new runtime support states.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:5-6` now encode `OnShieldAbsorb` and `OnStatusExpire` Ariel trigger rows with `damage_source` values `ShieldAbsorbedAmount` and `TrackedIncomingDamage`, plus `tracked_attribute=Holy`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` now encodes `ExtendStatusDuration` for `ariel-e-trait-5`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` now parses `hit_target_count_bonus`, `crit_chance_bonus`, `crit_damage_bonus`, `status_critical_damage_taken_bonus`, and `tracked_attribute`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now maps those parsed fields into runtime `SkillChoiceEffectSpec`, `SkillTriggerDefinition`, and `SkillEffectDefinition` data.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:55`, `:103-113`, and `:248-260` define the runtime enums/fields backing the new CSV schema.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only the remaining unsupported/partial Ariel rows listed above, confirming the new Ariel rows moved out of unsupported state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the previously proposed shared data/runtime changes for Ariel and then verify Ariel-wide implementation coverage.

## Task: 2026-05-22 Monster Skill Trigger CSV Source

### Task title

Add CSV authority for trigger-called hidden skill executions.

### Goals

- Add `monster_skill_triger.csv` to source CSV data and runtime catalog loading.
- Parse, build, validate, asset-reference, and catalog-sync trigger rows into `MonsterDefinition.SkillTriggers`.
- Keep Ariel trigger behavior data-owned where trigger event, choice gate, repeat timing, damage source, target shape, and prefab path are enough.

### Constraints

- Role Owner is Code Builder.
- The requested CSV spelling is `monster_skill_triger.csv`.
- CSV trigger runtime initially supports `SingleAttack` trigger rows only.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- When adding more event-driven rows, add them to `monster_skill_triger.csv` only after a matching generic trigger event exists.
- Keep unsupported trigger categories marked unsupported in choice CSV until their event payload and runtime contract are implemented.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:3-4` contains the new Ariel trigger rows for last projectile hit and shield expiry.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeData.cs` defines `MonsterSkillTriggersFileName = "monster_skill_triger.csv"`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeSourceCatalog.cs:14` adds the `MonsterSkillTriggers` source TextAsset slot.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeData.SourceModel.cs` adds the source-model trigger dictionary, while loader/editor/build/asset-reference/validation files load, build, reference, and validate trigger rows.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeData.MonsterDataset.cs` adds the `SkillTriggerRow` parser.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` was synced and now references the generated `monster_skill_triger.csv` TextAsset.
- Runtime and editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings; Unity-MCP CSV catalog sync logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-22: User requested `monster_skill_triger.csv` creation for Ariel `ariel-a-master-1` and `ariel-b-trait-4` trigger skills.

## Task: 2026-05-22 Ariel Choice And Multi-Effect CSV Cleanup

### Task title

Record Ariel CSV-owned choice support corrections and new multi-effect rows.

### Goals

- Keep supported Ariel choice behavior represented in `monster_skill_choices.csv` and `monster_skill_effects.csv`.
- Replace Ariel-E unconditional choice multipliers with conditional multi-effect rows where required.
- Preserve unsupported event-trigger behavior as unsupported instead of encoding it incorrectly.

### Constraints

- Role Owner is Code Builder.
- CSV remains the source of truth for these data-shaped changes.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Future Ariel event-trigger work should extend runtime trigger support before adding CSV rows for last-shot, shield-expiry, shield-absorb, or mark-expiry effects.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:18-23` correct Ariel-C trait/master runtime support states to `ReferenceDirect`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:34` removes the unconditional `damage_multiplier=1.5` from `ariel-e-trait-4`; `monster_skill_effects.csv:7` adds a Holy Exposure-conditioned extra damage row.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:36` removes the unconditional `damage_multiplier=0.82` from `ariel-e-master-1`; `monster_skill_effects.csv:8` adds all-ally incoming damage `-0.18` for 5 seconds.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:3-6` encode Ariel-E base, trait 2, master 2, and combined trait 2 plus master 2 shield amount rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` adds Ariel-B trait 5 as a shield-conditioned all-ally Holy damage status.
- `Import-Csv -Encoding UTF8` checks over the edited choice/effect rows returned the expected fields.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-22: User asked to start implementation from items that can be solved by CSV, then proceed to small shared runtime extensions.

## Task: 2026-05-22 CSV Runtime Refactor Cleanup

### Task title

Reduce duplicate code in the CSV runtime load/build/validation path without changing CSV schema or runtime behavior.

### Goals

- Share CSV line split/join/escape logic between runtime CSV reading and the editor prefab exporter.
- Consolidate repeated skill/status-effect payload parsing and runtime assignment.
- Consolidate repeated build-time filter/sort patterns.
- Use one referenced-asset collection path for editor asset catalog creation and validation coverage.

### Constraints

- Role Owner is Code Builder.
- Keep current CSV column names and runtime definition fields compatible.
- Do not turn `PakuriCsvRuntimeData` into a larger God Class; new responsibilities stay in small helper files.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If Unity regenerates project files, confirm the new runtime CSV helper scripts remain included in generated project metadata.
- Future CSV asset-path additions should go through `CollectReferencedAssets(...)` so editor catalog generation and validation stay aligned.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvLineCodec.cs` now owns shared CSV line split/join/escape/unescape helpers.
- `Pakuri/Assets/Scripts2/InGame/Data/Editor/PakuriSkillEffectPrefabCsvExporter.cs` now uses `PakuriCsvLineCodec` instead of its duplicate local CSV helpers.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.StatusPayload.cs` now owns shared status payload parsing, and `PakuriCsvRuntimeData.Build.cs` applies it through `ApplyStatusPayload(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.AssetReferences.cs` now owns referenced sprite/prefab collection, including `Skill effect '{effect.Id}' status_effect_prefab_path`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` and `PakuriCsvRuntimeData.Validation.cs` now both use `CollectReferencedAssets(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now uses `FilterAndSort(...)` for reward, active skill, effect, passive skill, and skill choice row selection.
- `dotnet build 'Pakuri\Assembly-CSharp.csproj' --no-restore` passed with 0 errors; existing MSB3277 warnings for `System.Net.Http` and `System.IO.Compression` remained.
- `dotnet build 'Pakuri\Assembly-CSharp-Editor.csproj' --no-restore` passed with 0 errors; the same existing MSB3277 warnings remained.
- `git diff --check -- Pakuri\Assets\Scripts2\InGame\Data\Runtime\Csv Pakuri\Assets\Scripts2\InGame\Data\Editor\PakuriSkillEffectPrefabCsvExporter.cs Pakuri\Assembly-CSharp.csproj` reported only existing line-ending normalization warnings and no whitespace errors.

### History

- 2026-05-22: User asked Code Builder to implement the four previously identified duplicate-reduction targets under `InGame/Data/Runtime/Csv`.

## Task: 2026-05-22 Passive Skill Multi-Effect CSV Runtime

### Task title

Extend `monster_skill_effects.csv` so passive skills and passive-gated active effects can use the shared effect runtime.

### Goals

- Attach effect rows to passive skill definitions as runtime data.
- Add passive requirement/exclusion columns and one-shot effect support.
- Add shield-received status modifier data for CSV-authored shield scaling.
- Keep Ariel F-J implementation data-owned rather than hardcoded by skill ID.

### Constraints

- Role Owner is Code Builder.
- CSV remains UTF-8 without BOM and follows the header plus type-row convention.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Use `requires_passive_skill_id` / `excludes_passive_skill_id` for future passive-gated multi-effect rows.
- Use `apply_once=true` only for effects that should fire once per passive owner/effect key.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:1` now includes `requires_passive_skill_id`, `excludes_passive_skill_id`, `apply_once`, and `status_shield_received_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses those new columns, and `PakuriCsvRuntimeData.Build.cs` copies them into `SkillEffectDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now sets `PassiveDefinition.PassiveEffects = BuildSkillEffects(model, skill.Id)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` validates `requires_passive_skill_id` and `excludes_passive_skill_id` against passive skill rows.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains the generic `passive-buff` status row.
- A byte check on `monster_skill_effects.csv` returned leading bytes `34 101 102`, confirming the edited CSV starts with `"` / `e` / `f` and not a UTF-8 BOM.
- Unity console logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: Code Builder extended the multi-effect schema after the user asked for Ariel F-J passive implementation through CSV runtime-read effects if possible.

## Task: 2026-05-22 Monster Skill Multi-Effect CSV Runtime

### Task title

Add `monster_skill_effects.csv` as the reusable CSV source for secondary skill effects.

### Goals

- Add a new source CSV table for choice-gated secondary effects.
- Parse/build/validate the table through `PakuriCsvRuntimeData`.
- Use the table to encode Ariel-C ally buffs, master effects, trait 5 shielded-ally Holy damage, and master 2 second wave without hardcoded skill IDs.
- Separate effect application target fields from visual center/anchor fields so ally buffs can apply to allies while visual effects can attach to affected units or stay at the primary attack center.

### Constraints

- Role Owner is Skill Builder.
- CSV remains UTF-8 and follows the existing header plus type-row convention.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder

### Status

Implemented, synced to Unity runtime catalog assets, and locally validated by build plus direct CSV reference checks.

### Next Actions

- Future similar skills should add rows to `monster_skill_effects.csv`.
- If future effects need unsupported targeting or projectile behavior, extend the multi-effect blueprint/schema first.

### Evidence

- Added `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` and Unity generated `monster_skill_effects.csv.meta` with GUID `4ddf6bb31440b41438f4a7b82bbd5a92`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_effects.csv` returned 9 Ariel-C rows with `effect_kind` values `Status` and `Damage`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs` now defines `MonsterSkillEffectsFileName = "monster_skill_effects.csv"`.
- `PakuriCsvRuntimeData.Loader.cs`, `.SourceModel.cs`, `.MonsterDataset.cs`, `.Build.cs`, `.Validation.cs`, and `.Editor.cs` now load, parse, build, validate, and catalog effect rows and effect prefab paths.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:83` defines `SkillMultiEffectCenterMode`; `:91` defines `SkillMultiEffectVisualAnchorMode`; `:107` and `:108` store the parsed values on `SkillEffectDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:359` and `:360` parse `center_mode` and `visual_anchor_mode`; `PakuriCsvRuntimeData.Build.cs:281` and `:282` copy them into runtime definitions.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:1` now includes `center_mode` and `visual_anchor_mode`; Ariel-C buff rows use `PrimarySkillCenter` plus `AppliedTargets`, while the master 2 damage row uses `PrimarySkillCenter` plus `Center`.
- Representative Ariel-C buff visual rows use `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab`; trait 2 / trait 5 supplemental numeric rows keep prefab paths blank to avoid duplicate buff visuals.
- A PowerShell CSV reference check over `monster_skills.csv`, `monster_skill_choices.csv`, and `monster_skill_effects.csv` returned `OK effects=9 ariel_c=9`, including skill ID, choice ID, prefab path, damage, and status-effect ID checks.
- 2026-05-22 follow-up CSV check returned all 9 Ariel-C effect rows with parsed `center_mode` / `visual_anchor_mode` values and the expected `Ariel_C-Buff` / `Ariel_C` prefab split.
- Unity-MCP `execute_menu_item` currently fails to find `Pakuri/Validate CSV Source Data` even though `PakuriCsvRuntimeData.Editor.cs` contains `[MenuItem("Pakuri/Validate CSV Source Data")]`; Unity-MCP `execute_code` remains blocked by the known Windows mono path-length error, so final validation did not rely on those tool paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- 2026-05-22 follow-up runtime/editor `dotnet build` commands again passed with 0 errors and existing MSB3277 warnings after the center/visual schema extension.

### History

- 2026-05-22: User requested a Designer multi-effect CSV blueprint followed by Skill Builder schema/parser/build/shared-executor implementation.
- 2026-05-22: User asked Code Builder to implement separated multi-effect centers and applied-target visuals so Ariel-C ally buffs can apply to allies but use the requested `Ariel_C-Buff.prefab` unit-attached effect.

## Task: 2026-05-21 Eve-A Branch Choice CSV Retune

### Task title

Retune Eve-A Arc Bolt branch choice rows so the new branch rule stays data-owned.

### Goals

- Remove the forced `branch_chance_set=1` behavior from Eve-A trait 5 and master 1.
- Keep the new branch chance values as additive choice bonuses.
- Set the recursive branch damage falloff to 70% on the choice rows that enable the mechanic.

### Constraints

- Role Owner is Code Builder.
- `monster_skill_choices.csv` remains the source of truth for these choice modifiers.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and rechecked from the edited CSV.

### Next Actions

- If future Arc Bolt tuning changes branch chance or damage falloff again, edit these same choice fields before considering code changes.
- If a future base Eve-A row needs always-on branching, add that through the shared projectile data path rather than overloading these two choice rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-trait-5` to `branch_chance_bonus=0.35`, blank `branch_chance_set`, `branch_count=2`, and `branch_damage_multiplier=0.7`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-master-1` to `branch_chance_bonus=0.6`, blank `branch_chance_set`, `branch_count=2`, `branch_damage_multiplier=0.7`, and `branch_search_radius=4.5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -in @('eve-a-trait-5','eve-a-master-1','eve-a-master-2') }` returned the updated branch fields exactly, while `eve-a-master-2` remained the non-branch status choice row.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:266-288` is still the shared consumer that interprets these branch fields into runtime branch behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the edit; existing MSB3277 warnings remained.

### History

- 2026-05-21: User asked Code Builder to implement the new Arc Bolt branch rule with minimal code changes, so the choice CSV was retuned to additive chance plus 70% recursive branch falloff.

## Task: 2026-05-20 Shield And Buff Status Schema Implementation

### Task title

Implement the blueprint CSV/runtime schema for source-aware buff and shield status behavior.

### Goals

- Add explicit skill-row ownership for target scope, merge policy, and shield refresh policy.
- Normalize new shield CSV content onto canonical `status_effect_id=shield` while keeping legacy parse compatibility.
- Keep runtime validation strict enough to catch incomplete shield/buff rows during catalog sync.

### Constraints

- Role Owner is Code Builder.
- CSV files remain the authoritative source for skill-status tuning.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented, synced into the runtime catalog, and validation-backed in the editor.

### Next Actions

- Future timed ally buff/shield rows should populate `status_target_scope` and `status_merge_policy` instead of relying on code-only defaults.
- Future shield rows should continue using canonical `status_effect_id=shield`; keep `holy-shield` as parse compatibility only until old content is fully gone.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1` now includes `status_target_scope`, `status_merge_policy`, and `shield_amount_refresh_policy`; `:4` shows `ariel-b` populated as `shield / all_allies / same_source_refresh / take_highest`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv:10` now keeps the canonical shared shield row under `status_effect_id=shield`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:137-139` adds the three new schema fields to runtime skill definitions.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs:80-82` and `:226-228` parse the three new CSV columns into `SkillRow`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:234-236` copies the parsed values into `SkillDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs:308-328` now rejects buff/shield rows missing supported `status_target_scope`, `status_merge_policy`, or `shield_amount_refresh_policy`, and `:321` / `:351` enforce canonical shield id `shield`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:172-190` maps CSV-owned duration, target scope, refresh rule, and runtime status payload into buff/shield skill data.
- Unity menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` eventually logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` after the source CSV asset refresh and the `status_effects.csv` quote fix.

### History

- 2026-05-20: Code Builder implemented the schema proposed in the shield/buff blueprint across source CSV, runtime parse/build, mapper, and validation code.
- 2026-05-20: Editor sync first failed with `CSV table 'monster_skills.csv' is missing required column 'status_target_scope'`, which confirmed Unity had not yet reimported the edited CSV asset.
- 2026-05-20: After a forced asset refresh, editor sync failed again with `CSV file 'status_effects.csv' row 10 has 2 columns but expected 19`; Code Builder fixed the broken shield-row quote and reran sync successfully.

## Task: 2026-05-20 Shield And Buff CSV Schema Design Handoff

### Task title

Prepare the CSV/schema handoff for source-aware shield and buff runtime unification.

### Goals

- Record the requested new skill-row data ownership for target scope and merge policy.
- Record that shield duration must come from CSV/runtime data instead of code fallback.
- Give Code Builder one evidence-based schema contract before implementation begins.

### Constraints

- Role Owner is Designer.
- This task changes documentation only; no CSV source file changed yet.
- New field names remain proposal-level until Code Builder implements them.

### Role Owner

Designer

### Status

Schema handoff documented for Code Builder.

### Next Actions

- Code Builder implements the selected skill-row fields through the CSV runtime build path.
- If Builder renames any proposed field, update this file with the final adopted schema names in the implementation turn.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:4` shows `ariel-b` already owns `status_duration_seconds=5` but the current shield runtime does not honor that through timed gameplay state.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:14` and `:35` show buff rows already own duration/stack values.
- `Pakuri/Assets/CSVdata/source/status_effects.csv:10` already has a shield-like shared row under `holy-shield`.
- `boards/SkillBluePrint/shield-buff-status-unification-blueprint.md` proposes CSV-owned `status_target_scope`, merge-policy fields, and shield canonical-id normalization notes.

### History

- 2026-05-20: User requested a Builder-ready markdown handoff for the shield/buff redesign.
- 2026-05-20: Designer documented the required CSV schema direction and linked it to the new Builder blueprint.

## Task: 2026-05-20 Ariel-A Master 2 Status Choice CSV Activation

### Task title

Promote `ariel-a-master-2` from data-only to shared-status-supported choice data with a per-choice Holy damage taken bonus field.

### Goals

- Encode Ariel-A master 2 as a shared status choice in `monster_skill_choices.csv`.
- Let a projectile choice row carry its own `status_element_damage_taken_bonus` instead of forcing all users of the same status row to share one value.
- Keep the source of truth in the unified choice CSV rather than adding a second Ariel-specific data table.
- Sync the runtime catalog after the source CSV edit.

### Constraints

- Role Owner is Code Builder.
- The CSV file remains UTF-8.
- The unified choice CSV schema changed in this task by adding `status_element_damage_taken_bonus`.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented, schema-updated, and synced into the runtime catalog.

### Next Actions

- If later active choices need new debuff application, prefer `status_tag` plus optional stack/chance/override fields before introducing skill-specific runtime branches.
- Keep unsupported Ariel rows explicit until a matching shared runtime contract exists.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now contains the new header/type column `status_element_damage_taken_bonus`, and `ariel-a-master-2` sets `status_tag=holy-exposure`, `status_stacks_set=1`, `status_element_damage_taken_bonus=0.15`, `runtime_support_state=ReferenceDirect`, and `runtime_support_notes=Reference status effect mapped into unified choice CSV.`
- `git diff -- Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` shows the new shared choice column plus the `ariel-a-master-2` row changing from `DataOnlyUnsupported` to a `holy-exposure` shared-status row with `0.15`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -eq 'ariel-a-master-2' }` returned the new status fields exactly.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` returned `success:true` for this task; no new sync log line was captured afterward in the inspected console window.

### History

- 2026-05-20: User asked Code Builder to apply the `ariel-a-master-2` Holy Exposure data fix using the shared status choice path.
- 2026-05-20: User then required per-skill values, so Code Builder added `status_element_damage_taken_bonus` to `monster_skill_choices.csv` and set Ariel-A master 2 to `0.15`.

## Task: 2026-05-20 DebugModifiedUI Uses Unified Skill Choice CSV

### Task title

Reuse `monster_skill_choices.csv` runtime choice rows for debug active trait/master UI.

### Goals

- Keep `monster_skill_choices.csv` as the single source for active choice button text and debug-applied active choice IDs.
- Reuse the already built `SkillChoiceDefinition` runtime objects instead of adding a debug-only CSV or hardcoded label path.
- Keep current active choice grouping (`ActiveEnhancement`, `ActiveMaster`) authoritative for debug availability rules.

### Constraints

- Role Owner is Code Builder.
- No CSV file shape or content changed in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented as a new consumer of existing runtime choice data. Debug modifier UI now reads existing `SkillChoiceDefinition` rows and does not add a parallel data source.

### Next Actions

- If future debug UI needs richer formatting than one `Text (TMP)` per button, keep sourcing `Title` and `DescriptionText` from `SkillChoiceDefinition` rather than duplicating the strings elsewhere.
- If passive enhancement debug support is later added, continue using the same `SkillChoiceDefinition` runtime catalog path.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` already contains active choice rows with `choice_id`, `choice_group`, `title`, and `description_text`, for example `ariel-a-trait-1` through `ariel-a-master-2`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:52-83` defines `SkillChoiceDefinition` with `ChoiceId`, `ChoiceGroup`, `Title`, and `DescriptionText`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs:287-315` already builds `SkillChoiceDefinition` rows from the unified choice CSV into active skill and passive skill choice arrays.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now reads `sourceSkill.EnhancementChoices` and `sourceSkill.MasterSkillChoices` directly and writes their `Title` plus `DescriptionText` into `DebugModifiedUI` button labels.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now records applied debug modifier picks with the exact `choice.ChoiceId`, which keeps debug-applied choices aligned with the same runtime IDs used by Offering and combat execution.

### History

- 2026-05-20: User requested `DebugModifiedUI` button text and application behavior sourced from `monster_skill_choices.csv`; Code Builder reused the existing runtime choice catalog instead of adding new data assets.

## Task: 2026-05-20 Projectile Burst Count CSV Field

### Task title

Add shared projectile burst count data to active monster skill CSV.

### Goals

- Add a reusable CSV field for sequential projectile burst count.
- Keep `monster_skills.csv` as the source of Sein-B numeric runtime behavior.
- Keep existing simultaneous projectile modifiers compatible by using the existing `additional_projectile_bonus` column for burst skills.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- CSV stays UTF-8.
- Existing non-burst projectile skills should keep burst count 1.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and validated through build plus Unity runtime mapping inspection.

### Next Actions

- Future projectile skills that need sequential volleys should set `projectile_burst_count` instead of adding monster-specific runtime branches.
- If a future skill needs both sequential burst count and simultaneous fan count independently modified by choices, add a separate choice column instead of overloading `additional_projectile_bonus`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now includes `projectile_burst_count`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` parses `projectile_burst_count` into `SkillRow.ProjectileBurstCount`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` copies `SkillRow.ProjectileBurstCount` into `SkillDefinition.ProjectileBurstCount`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps `SkillDefinition.ProjectileBurstCount` into `ProjectileSkillData.Projectile.BurstProjectileCount`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` returned `sein-b` with display name `?묒뿴 ?쒖궗` and `projectile_burst_count=5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` returned `sein-b-trait-1 additional_projectile_bonus=2`, `sein-b-master-1 additional_projectile_bonus=4`, and `sein-b-master-2 additional_projectile_bonus=-2`.

### History

- 2026-05-20: Code Builder added the field for Sein-B and kept it generic for future projectile skills.

## Task: 2026-05-19 Monster Choice CSV Unification

### Task title

Unify monster choice runtime data into one choice CSV plus one slim Offering gate CSV.

### Goals

- Replace `monster_reward_choices.csv` with a slim `monster_modifier_skill_choice.csv` gate table.
- Move runtime-applicable monster choice modifiers into `monster_skill_choices.csv`.
- Keep unsupported special-case choices explicitly marked for later logic work instead of hiding them behind missing rows.

### Constraints

- Role Owner is Code Builder.
- Every CSV conclusion must stay tied to inspected code or inspected reference markdown under `Pakuri/reference/2.Monster/`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Active monster choice data is now split into one slim Offering gate CSV and one unified choice/modifier CSV.

### Next Actions

- If future special-case logic is implemented, start from the rows currently marked `DataOnlyUnsupported` or `PartialRuntimeSupport` in `monster_skill_choices.csv`.
- If CSV ownership changes again, update this file together with `boards/RUN/RUN_BLACKBOARD.md` and `boards/UI/RUNSCENE_UI.md`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.cs` now sets `MonsterRewardChoicesFileName = "monster_modifier_skill_choice.csv"`.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` now contains only `choice_id, monster_id, active_skill_id, passive_skill_id, sort_order`, and `Import-Csv -Encoding UTF8` over the file returned 250 data rows after excluding the type row.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv.meta` now uses GUID `2f9229f6de8506a4fae1fad9c093e347`, and `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` now references that same GUID for `MonsterRewardChoices`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` now parses the slim reward gate rows and the merged modifier/runtime-support columns from `monster_skill_choices.csv`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now builds `RewardChoiceDefinition` as a slim gate object and builds merged runtime modifier fields directly into `SkillChoiceDefinition`.
- Deleted `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` because active Scripts2 runtime code no longer reads them.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` no longer has the serialized `skillChoiceModifierCsv` field or the old modifier reload path.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` now returns 250 data rows after excluding the type row, with support-state counts `ReferenceDirect=104`, `PartialRuntimeSupport=24`, `DataOnlyUnsupported=115`, and `DerivedFromReference=7`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` keeps unsupported rows explicit, for example `ariel-a-master-1` remains `DataOnlyUnsupported` with a note that the final-shell double explosion still needs special-case logic.
- The applied numeric-choice values were rechecked against inspected reference markdown, including `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`, `Pakuri/reference/2.Monster/ariel/skill/f-guiding-light.md`, `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`, `Pakuri/reference/2.Monster/eve/skill/b-prism-ray.md`, `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md`, `Pakuri/reference/2.Monster/sein/skill/e-doomsday-line.md`, and `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing MSB3277 `System.Net.Http` / `System.IO.Compression` warnings remained.

### History

- 2026-05-19: Code Builder merged monster choice runtime values into `monster_skill_choices.csv`, introduced the slim `monster_modifier_skill_choice.csv` gate file, deleted the old reward/modifier CSV pair, aligned `PakuriCsvRuntimeSourceCatalog.asset` and the new gate CSV `.meta` on GUID `2f9229f6de8506a4fae1fad9c093e347`, and reclassified several Eve beam/area rows from unsupported to direct or partial support after rechecking the reference markdown and the current runtime field support.

## Task: 2026-05-19 CSV Auto-Sync Missing TextAsset Recovery

### Task title

Make CSV runtime auto-sync recover when a source CSV exists on disk but is not yet imported as a Unity `TextAsset`.

### Goals

- Explain and fix the `Required imported CSV TextAsset is missing` auto-sync failure for `monster_modifier_skill_choice.csv`.
- Keep external CSV edits recoverable without requiring a manual Unity reimport first.
- Preserve the existing runtime source catalog sync ownership.

### Constraints

- Role Owner is Code Builder.
- The fix must stay grounded in inspected editor sync code, real file existence, and Unity console evidence.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and editor-verified. Auto-sync now retries a synchronous asset import before treating a source CSV as missing.

### Next Actions

- If another imported CSV path is renamed externally, use the same recovery path instead of assuming the AssetDatabase has already created the `TextAsset`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` previously called `AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath)` once inside `LoadTextAssetOrThrow(...)` and threw immediately when the load returned `null`.
- The inspected filesystem still contained `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` and `monster_modifier_skill_choice.csv.meta` while the user-reported stack trace showed the exception was thrown before the asset became an imported `TextAsset`.
- `PakuriCsvRuntimeData.Editor.cs` now calls `TryImportTextAsset(assetPath)` before failing. That helper checks whether the file exists on disk, runs `AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport)`, then runs `AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport)`, and only throws if the `TextAsset` still cannot be loaded.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` after the fix logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity console after the fix no longer showed the previous `Required imported CSV TextAsset is missing at 'Assets/CSVdata/source/monster_modifier_skill_choice.csv'` exception.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after the fix; a parallel editor build attempt hit only a transient `Assembly-CSharp.dll` file lock, and a standalone `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` then passed with 0 errors. Existing MSB3277 warnings remained unchanged.

### History

- 2026-05-19: User reported Unity editor auto-sync failing on `monster_modifier_skill_choice.csv`; Code Builder traced the failure to a one-shot `LoadAssetAtPath<TextAsset>` check and added a synchronous refresh/import retry path before the fatal exception.

## Source: boards\DATA\GAMEDATA_ASSET_BLACKBOARD.md

## Task: 2026-05-23 Eve-E EffectManager Scene Wiring

### Task title

Wire Eve-E base AreaAttack visuals through the active `NewRunScene` `EffectManager` path.

### Goals

- Keep base Eve-E visual authority scene-owned through `NewRunScene` `EffectManager`.
- Resolve base Eve-E casts to `Assets/Prefab/Skill/Eve/Eve_E.prefab`.
- Avoid adding a parallel base-skill prefab-path route in monster skill CSV rows.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- No prefab content edit was required in this task.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and file-verified.

### Next Actions

- User verifies in Play Mode that base Eve-E fields show `Eve_E.prefab`.
- If a later Eve-E choice needs its own choice-level visual override, keep that on the runtime asset-catalog path while leaving the base visual scene-owned.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Eve/Eve_E.prefab` exists, and its root GameObject fileID is `1184936592282639523`.
- `Pakuri/Assets/Prefab/Skill/Eve/Eve_E.prefab.meta` stores GUID `1313fcd817f979e4981325d9c199fd30`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:12650-12651` now maps `SkillId: eve-e` to prefab GUID `1313fcd817f979e4981325d9c199fd30`, which is `Assets/Prefab/Skill/Eve/Eve_E.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` remains the active base monster skill visual resolver through `ResolveMonsterSkillEffectPrefab(...)`.

### History

- 2026-05-23: Eve-E implementation required adding the missing `EffectManager` scene mapping for the existing `Eve_E.prefab`.

## Task: 2026-05-23 Eve-D Base And Master-1 Visual Wiring

### Task title

Wire Eve-D base and master-1 follow-up visuals to the same `Eve_D.prefab` across the scene-owned EffectManager path and the runtime asset catalog.

### Goals

- Keep base Eve-D visual authority scene-owned through `NewRunScene` `EffectManager`.
- Make the master-1 choice-level prefab path resolvable through the runtime asset catalog.
- Use the same prefab path for base Eve-D and the delayed master-1 follow-up explosion.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- No prefab content edit was required in this task.
- Base visual authority stays on `EffectManager`; choice-level prefab path authority stays on the runtime asset catalog.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and file-verified.

### Next Actions

- User verifies in Play Mode that both the base Eve-D cast and the master-1 delayed follow-up display `Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- If a later Eve-D follow-up visual diverges from the base visual, update both the scene mapping and the runtime catalog evidence together.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Eve/Eve_D.prefab` exists.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `SkillId: eve-d` under monster `eve` to prefab GUID `ef1bb9690f7a9234dad21ff0d9c80e32`, which is `Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `eve-d-master-1` with `skill_effect_prefab_path=Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `AssetPath: Assets/Prefab/Skill/Eve/Eve_D.prefab` with root GameObject fileID `1107537072718467244` and GUID `ef1bb9690f7a9234dad21ff0d9c80e32`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the Eve-D visual wiring; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User required both the base Eve-D skill effect and the master-1 explosion effect to use `Assets/Prefab/Skill/Eve/Eve_D.prefab`.

## Task: 2026-05-23 Eve-C Runtime Visual Wiring And Catalog Entry

### Task title

Wire Eve-C base and master-2 visuals through the active scene/effect runtime paths.

### Goals

- Keep the base Eve-C skill visual scene-owned through `NewRunScene` `EffectManager`.
- Keep the Eve-C master-2 expire-burst prefab available to runtime asset resolution through `PakuriCsvRuntimeAssetCatalog`.
- Record the evidence even though Unity-MCP menu/console calls timed out during this task.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- No prefab content edits were required.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and file-verified. Fresh Unity sync confirmation could not be collected because Unity-MCP `execute_menu_item` and `read_console` timed out during this task.

### Next Actions

- When Unity-MCP becomes responsive again, rerun `Pakuri/Sync CSV Runtime Catalog Assets` to replace the file-level evidence with an editor sync log.
- User verifies in Play Mode that base Eve-C uses `Eve_C.prefab` and the master-2 expire burst uses `Eve_c-master-2.prefab`.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Eve/Eve_C.prefab` exists and its root GameObject fileID is `2181036612366644816`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:12646-12647` now maps `SkillId: eve-c` to prefab GUID `383d4c700df69d44898dc953ea18b9d4`, which is `Assets/Prefab/Skill/Eve/Eve_C.prefab`.
- `Pakuri/Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab` exists and `Pakuri/Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab.meta` stores GUID `30a4745c2cff29f41acf72125c981f67`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `AssetPath: Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab` with root GameObject fileID `4334470998071384926` and GUID `30a4745c2cff29f41acf72125c981f67`.
- `Test-Path Pakuri\Assets\Prefab\Skill\Eve\Eve_C.prefab` and `Test-Path Pakuri\Assets\Prefab\Skill\Eve\Eve_c-master-2.prefab` both returned `True`.

### History

- 2026-05-23: User supplied `Assets/Prefab/Skill/Eve/Eve_C.prefab` and `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab` as the required Eve-C visual paths for base and master-2 work.

## Task: 2026-05-22 Passive Effect Runtime Catalog Sync

### Task title

Sync passive-effect CSV schema/content into the Unity runtime catalog assets.

### Goals

- Confirm the runtime catalog accepts the new passive effect columns.
- Confirm new `passive-buff` status data and Ariel F-J effect rows are available to runtime catalog loading.
- Keep catalog evidence separate from gameplay verification.

### Constraints

- Role Owner is Code Builder.
- This task syncs CSV runtime catalog assets only; no prefab asset content was changed.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Synced and console-verified.

### Next Actions

- If a future passive effect adds prefab paths, rerun `Pakuri/Sync CSV Runtime Catalog Assets` so the asset catalog picks up the prefab reference.

### Evidence

- Unity `execute_menu_item` for `Pakuri/Sync CSV Runtime Catalog Assets` returned `success:true`.
- Unity console logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime` contains `PakuriCsvRuntimeAssetCatalog.asset` and `PakuriCsvRuntimeSourceCatalog.asset`; this passive task did not add new prefab files to the asset catalog.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now stores the new passive effect schema and Ariel F-J rows consumed by catalog build.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now stores `passive-buff`, which the synced source catalog can load through the existing status table.

### History

- 2026-05-22: Code Builder reran the runtime CSV catalog sync after extending passive effect CSV data and status definitions.

## Task: 2026-05-22 Multi-Effect Runtime Catalog Asset Sync

### Task title

Sync the new `monster_skill_effects.csv` source and Ariel-C prefab path into runtime catalog assets.

### Goals

- Add the new source CSV TextAsset to `PakuriCsvRuntimeSourceCatalog`.
- Add `Assets/Prefab/Skill/Ariel/Ariel_C.prefab` to the runtime prefab catalog through effect rows.
- Add `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab` to the runtime prefab catalog through Ariel-C ally buff effect rows.
- Add the `NewRunScene` `EffectManager` base visual mapping for `ariel-c` so the base SingleAttack attack-target visual can resolve separately from ally buff attached visuals.
- Keep prefab authority CSV-owned only for effect-row visuals introduced by the multi-effect table.

### Constraints

- Role Owner is Skill Builder.
- This task changes runtime catalog assets; it does not edit scene prefab wiring.
- Unity Play Mode verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented and asset-verified.

### Next Actions

- Future effect-row prefab paths should be added to `monster_skill_effects.csv` and synced through the same catalog path.

### Evidence

- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` now serializes `MonsterSkillEffects` to GUID `4ddf6bb31440b41438f4a7b82bbd5a92`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv.meta` stores GUID `4ddf6bb31440b41438f4a7b82bbd5a92`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `AssetPath: Assets/Prefab/Skill/Ariel/Ariel_C.prefab` with prefab GUID `f851084efb562e043a673ac67840693f`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset:27` now contains `AssetPath: Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab` with prefab GUID `33b5e950176a3454e9e779d062c8d540`.
- `Pakuri/Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab.meta` stores GUID `33b5e950176a3454e9e779d062c8d540`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:12636` maps `SkillId: ariel-c` to `Assets/Prefab/Skill/Ariel/Ariel_C.prefab` GUID `f851084efb562e043a673ac67840693f` for the base attack-target SingleAttack visual.
- `Test-Path Pakuri\Assets\Prefab\Skill\Ariel\Ariel_C.prefab` returned `True`.
- `Test-Path Pakuri\Assets\Prefab\Skill\Ariel\Ariel_C-Buff.prefab` returned `True`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` returned success earlier in this task, and final asset evidence is the serialized YAML plus `Test-Path`; Unity-MCP `execute_menu_item` currently fails to find `Pakuri/Validate CSV Source Data`, so final validation used direct CSV reference checks instead of the validation menu.
- 2026-05-22 follow-up Unity `Pakuri/Sync CSV Runtime Catalog Assets` returned success after `Ariel_C-Buff.prefab` was added to effect rows; Unity console warning/error read showed only MCP client handler logs, not CSV or C# compile errors.

### History

- 2026-05-22: Skill Builder added the multi-effect CSV and synced its source/prefab references into runtime catalog assets.
- 2026-05-22: Code Builder added the Ariel-C buff prefab catalog path and the `ariel-c` scene visual mapping needed to keep ally buff visuals and attack-target visuals separate.

## Task: 2026-05-20 Shield And Buff Runtime Catalog Asset Sync

### Task title

Resync runtime CSV catalog assets after the shield/buff schema change.

### Goals

- Make Unity reimport the edited CSV source assets before runtime catalog sync.
- Confirm the runtime source catalog accepts the new shield/buff schema and canonical shield row.
- Record the asset-side evidence because this task changed runtime catalog content, not only code.

### Constraints

- Role Owner is Code Builder.
- This task changes runtime CSV catalog assets, not scene prefab wiring.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Completed after source-asset refresh and one data-row fix.

### Next Actions

- If another external CSV edit appears to be ignored by catalog sync, refresh/import the source asset before assuming the source file contents are wrong.
- Keep this board aligned with `boards/DATA/DATA_BLACKBOARD.md` whenever runtime catalog source shape changes again.

### Evidence

- `git status --short -- Pakuri/Assets/Resources/Pakuri/CSVRuntime/* Pakuri/Assets/CSVdata/source/monster_skills.csv Pakuri/Assets/CSVdata/source/status_effects.csv` showed `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` modified after the schema work and sync path.
- Unity menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` first surfaced real asset/data-state failures rather than silent success: `CSV table 'monster_skills.csv' is missing required column 'status_target_scope'` before source asset refresh, then `CSV file 'status_effects.csv' row 10 has 2 columns but expected 19` before the shield-row quote fix.
- `Pakuri/Assets/CSVdata/source/status_effects.csv:10` now has a valid canonical shield row, which removed the row-shape failure during sync.
- Unity `refresh_unity` with `mode=force scope=assets` completed successfully, and the next `Pakuri/Sync CSV Runtime Catalog Assets` invocation logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-20: Code Builder changed source CSV schema/content for shield/buff unification and then used Unity-side refresh plus catalog sync to propagate those edits into runtime assets.
- 2026-05-20: The asset sync verification exposed a stale-import problem and a malformed shield row, both of which were fixed before the final successful sync.

## Task: 2026-05-20 Sein-B EffectManager Scene Wiring

### Task title

Wire Sein-B to the requested shared Sein projectile prefab.

### Goals

- Keep active monster skill visuals scene-owned through `EffectManager`.
- Reuse `Assets/Prefab/Skill/Sein/Sein_A.prefab` for `sein-b` as requested.
- Avoid adding a parallel CSV prefab-path route for base monster skill visuals.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User-authored prefab content is preserved.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and file-verified.

### Next Actions

- User verifies in Play Mode that Sein-B projectiles use the requested `Sein_A` visual.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab` exists and its `.meta` GUID is `256552cb82ec9c2499fc2e0e01d20dd2`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now serializes `sein-b` under the `sein` `EffectManager` group with prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` resolves monster skill visuals through `ResolveMonsterSkillEffectPrefab(monsterId, skillId)`.

### History

- 2026-05-20: User requested `Sein-b` to use `Assets/Prefab/Skill/Sein/Sein_A.prefab`; Code Builder added the scene mapping.

## Task: 2026-05-19 Sein-A EffectManager Scene Wiring

### Task title

Restore the missing `NewRunScene` `EffectManager` prefab mapping for `sein-a`.

### Goals

- Keep active monster projectile visuals wired through scene-owned `EffectManager` entries.
- Restore the `sein-a` projectile prefab link without adding a parallel prefab-resolution route.

### Constraints

- Role Owner is Code Builder.
- The fix must stay grounded in inspected prefab files and actual scene serialization.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented in scene serialization and file-verified.

### Next Actions

- If future Sein active skills gain retained visuals, add them to the same `EffectManager` group instead of moving prefab-path authority back into CSV.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab` exists and `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab.meta` stores GUID `256552cb82ec9c2499fc2e0e01d20dd2`.
- Before this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10469` serialized `MonsterId: sein` with `SkillEffects: []`.
- After this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10471` serializes `SkillId: sein-a` and the `Sein_A.prefab` GUID under the `sein` `EffectManager` group.

### History

- 2026-05-19: User reported that the in-game Sein check showed no assigned Sein prefab effect in `EffectManager`.
- 2026-05-19: Code Builder restored the `sein-a` scene mapping to `Assets/Prefab/Skill/Sein/Sein_A.prefab`.

## Source: boards\MON\EVE_MONSTER.md

## Task: 2026-05-23 Eve-E Shared AreaAttack Magazine And Vulnerable Runtime

### Task title

Implement Eve-E base runtime plus trait/master effects on the shared AreaAttack path with generic magazine, multi-deploy, vulnerable-threshold, and vulnerable-max-stack support.

### Goals

- Keep base `eve-e` on the shared `AreaAttack` runtime while honoring its authored `magazine_capacity=3`, `reload_seconds=6`, and `shot_interval_seconds=0.8`.
- Keep Eve-E visuals scene-owned through `NewRunScene` `EffectManager` using `Assets/Prefab/Skill/Eve/Eve_E.prefab`.
- Support trait 1 magazine/duration, trait 2 tick speed plus vulnerable stack amount, trait 3 damage, trait 4 reload plus simultaneous deploy count, trait 5 vulnerable-threshold lightning damage, master 1 simultaneous deploy count plus tick speed, and master 2 vulnerable max-stack plus crit-damage-taken-per-stack.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV and runtime code were explicitly treated as the parsed source for this task.
- The runtime change stays on shared `SkillData`, `SkillRuntimeInstance`, `ZoneSkillExecutor`, and shared status-resolution paths; no Eve-only executor class was added.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity-refresh/console-checked.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-E now spends 3 magazine charges before reload, can recast every `0.8` seconds, and shows `Eve_E.prefab`.
- User verifies trait 4 and master 1 spawn additional simultaneous fields on the shared multi-deploy path.
- User verifies trait 5 only grants the `+40%` lightning damage bonus against targets at `vulnerable >= 5`, and master 2 raises vulnerable max stacks by `+5` while adding `+1%` crit damage taken per vulnerable stack.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Eve-E row is `runtime_kind=AreaAttack`, `base_damage=14`, `spell_power_coefficient=0.9`, `cooldown_seconds=5`, `active_duration_seconds=5`, `magazine_capacity=3`, `reload_seconds=6`, `shot_interval_seconds=0.8`, and `status_effect_id=vulnerable`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` Eve-E trait/master rows are now all `runtime_support_state=RuntimeImplemented`; trait 5 now uses `conditional_damage_multiplier=1.4` with `conditional_target_status_id=vulnerable` and `conditional_target_status_min_stacks=5`, and master 2 now uses `status_critical_damage_taken_bonus=0.01` plus `status_max_stacks_bonus_status_id=vulnerable` and `status_max_stacks_bonus=5`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs`, `InGameSkillDefinitionMapper.cs`, and `Skills/Runtime/SkillRuntimeInstance.cs` now carry generic `MagazineCapacity` / `ReloadSeconds` into non-projectile active skills, so Eve-E uses the shared runtime magazine/reload state instead of a projectile-only path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now resolves targeted status max-stack bonuses and shared AreaAttack deployment counts, and `InGameZoneSkillActor.cs` now applies shared conditional damage multipliers against tick targets through the active snapshot.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:12650-12651` now maps `SkillId: eve-e` to prefab GUID `1313fcd817f979e4981325d9c199fd30`, which is `Assets/Prefab/Skill/Eve/Eve_E.prefab`.
- `dotnet build Pakuri\\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity `refresh_unity` completed successfully for a forced asset/script refresh request, and Unity console warning/error read returned only MCP client-handler logs, not C# compile or CSV parse errors.

### History

- 2026-05-23: User told Skill Builder to implement Eve-E and all enhancement/master effects while treating the current CSV/code as the parsed source.

## Task: 2026-05-23 Eve-D Shock-Gated Delayed Recast

### Task title

Implement Eve-D on the shared SingleAttack path with a shock-gated delayed follow-up that reuses the same prefab but does not recurse.

### Goals

- Keep base `eve-d` on the shared `SingleAttack` runtime using `Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- Keep trait 3 cooldown reduction on the existing shared cooldown multiplier field instead of adding a new cooldown schema.
- Keep trait 4 on the shared status-stack path by adding one extra `shock` stack on hit.
- Implement `master-1` so targets already in `shock` when struck by Eve-D receive one extra Eve-D follow-up after `0.5` seconds at `50%` damage.
- Prevent the follow-up cast from scheduling another follow-up explosion.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Current CSV and runtime files were explicitly treated as the parsed source for this task.
- The follow-up must stay on the shared `SingleAttack` executor path; no Eve-only executor class was added.
- No new CSV columns were added for this task because the current parser requires header-width alignment across every row.
- Base Eve-D visual and master-1 follow-up visual both use `Assets/Prefab/Skill/Eve/Eve_D.prefab`.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that base Eve-D uses `Eve_D.prefab`, trait 3 reduces cooldown, and trait 4 adds one extra `shock` stack.
- User verifies that `master-1` only recasts on enemies already carrying `shock` at the moment the first Eve-D hit lands, waits `0.5` seconds, deals `50%` damage, and does not trigger another follow-up.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps `eve-d` as `runtime_kind=SingleAttack`, `attribute=Lightning`, `base_damage=10`, `spell_power_coefficient=0.7`, `radius=3.5`, `cooldown_seconds=7`, `status_effect_id=shock`, and `status_chance=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-d-trait-3` to `cooldown_multiplier=0.8` and `damage_multiplier=1.15`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-d-trait-4` to `status_tag=shock` and `status_stacks_bonus=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-d-master-1` to `skill_effect_prefab_path=Assets/Prefab/Skill/Eve/Eve_D.prefab`, `branch_count=1`, `branch_damage_multiplier=0.5`, `branch_search_radius=0.5`, and `status_tag=shock`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now extends `SingleAttackSkillExecutor` so `ResolveFollowUpSpec(...)` interprets that scoped Eve-D choice payload, `RegisterFollowUpTarget(...)` records only targets that already have the required status, `ScheduleConditionalFollowUps(...)` waits per repeat, and `ExecuteAtCenter(..., false)` prevents recursive re-explosions.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `SkillId: eve-d` under monster `eve` to the `Eve_D.prefab` GUID.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the implementation; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User told Skill Builder to treat the current CSV/code as the parsed source and required Eve-D master 1 to recast once after `0.5` seconds only when the struck enemy was already shocked, without allowing the follow-up to explode again.

## Task: 2026-05-23 Eve-C Shared Hurtbox Root Fix

### Task title

Fix Eve-C prefab-hitbox misses by making shared collider-contact skills resolve enemy hurtboxes from the spawned unit root instead of the actor child transform.

### Goals

- Stop Eve-C from reading `targetColliders=[]` when Stage 1 enemies already have body colliders on the spawned unit hierarchy.
- Keep collider-authoritative contact skills using real unit hurtboxes.
- Leave non-contact skills such as explicit target-designated skills and radius/battlefield-only paths on their existing logic.

### Constraints

- Role Owner is Code Builder.
- Keep the solution shared; do not add an Eve-only collider lookup branch.
- Preserve existing non-contact targeting behavior for skills such as Ariel-D mark-style selection.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User reruns Eve-C in Play Mode and confirms `[ZoneHitboxDebug:eve-c]` no longer reports `targetColliders=[]`.
- User verifies Eve-C now damages only targets whose spawned-unit hurtboxes overlap the prefab collider.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitRosterService.cs` now stores `HitboxRoot`, caches unit hitbox colliders, and exposes shared collider-overlap utility logic through `UnitHitboxUtility`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` now registers player/enemy roster entries with the spawned unit root transform instead of only the nested actor transform.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now resolves `FindUnitByCollider(...)` through `UnitRosterEntry.ContainsTransform(...)`, which includes the spawned unit root hierarchy.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now reads target hurtboxes from the shared unit hitbox contract, which fixes the Eve-C prefab-hitbox path without changing the non-contact radius fallback branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` and `InGameEnemySkillHitboxActor.cs` now prefer collider-authoritative roster-hit checks when the attacking object actually has colliders, and keep the old radius fallback only when no collider hitbox exists.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the change; existing `MSB3277` warnings remained. One earlier parallel build attempt hit a temporary file-lock on `obj\\Debug\\Assembly-CSharp.dll` before the successful rerun.

### History

- 2026-05-23: Eve-C debug logs showed `targetColliders=[]` on visibly overlapping Stage 1 enemies, so Code Builder moved shared contact hit detection to a spawned-unit-root hurtbox contract.

## Task: 2026-05-23 Eve-C Prefab Hitbox Debug Logging

### Task title

Instrument Eve-C prefab-hitbox ticks so live overlap misses can be explained from runtime logs.

### Goals

- Log Eve-C zone initialization with cached prefab collider data.
- Log Eve-C tick candidate counts, target collider sets, collider-pair overlap results, and routed hit/miss outcomes.
- Keep the logs narrow to Eve-C so shared AreaAttack runtime spam stays contained.

### Constraints

- Role Owner is Code Builder.
- Do not change Eve-C gameplay behavior while adding the logs.
- Limit the debug path to inspected Eve-C runtime id evidence instead of enabling all zone skills.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User runs Eve-C in Play Mode and captures lines beginning with `[ZoneHitboxDebug:eve-c]`.
- If the logs show collider overlap `false` while visuals appear to touch, inspect the reported enemy child collider bounds instead of the sprite only.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now logs Eve-C-only prefab-hitbox initialization, tick start/end, per-target collider summaries, per collider-pair `Distance(...).isOverlapped` results, and hit/miss routing.
- The debug gate is `runtime.SkillId == "eve-c"` inside `IsDebugSkill(...)`, so other AreaAttack skills do not emit the new logs.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the logging edit; existing `MSB3277` warnings remained.

### History

- 2026-05-23: User reported that `Eve_C(Clone)` looked physically overlapping in scene view but enemies still took no damage, so Code Builder added runtime overlap diagnostics on the Eve-C prefab-hitbox path.

## Task: 2026-05-23 Eve-C Prefab Collider Tick AreaAttack

### Task title

Make Eve-C follow its prefab collider and prefab scale on the shared AreaAttack path.

### Goals

- Stop Eve-C from using a fixed radius-only zone hit check when `Eve_C.prefab` already has a collider.
- Keep Eve-C visual size owned by the instantiated prefab instead of force-fitting the sprite to `radius * 2`.
- Let Eve-C trait radius scaling keep working through `radius_multiplier` by scaling the prefab hitbox and visual together.

### Constraints

- Role Owner is Code Builder.
- Keep the change on the shared AreaAttack runtime path; do not add an Eve-only executor branch.
- Do not reinterpret `radius_bonus=1.3`; this task keeps `radius_multiplier` as the scaling input and does not author a new `radius_bonus` usage.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-C only hits targets overlapping the instantiated `Eve_C.prefab` collider.
- User verifies Eve-C trait radius upgrades enlarge the prefab hitbox and visible effect together instead of using the old fixed-radius sprite fit.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Eve/Eve_C.prefab` exists and includes `BoxCollider2D` with authored size data.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now scales instantiated AreaAttack prefabs only when they actually contain collider hitboxes, using the existing snapshot radius-multiplier path before zone ticking starts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now caches prefab colliders, skips the old sprite-to-radius rescale when a prefab hitbox exists, and applies tick damage/status through collider overlap checks with the old radius path kept as fallback.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the change; existing `MSB3277` warnings remained.
- Unity-MCP console read after refresh showed no C# compile errors; remaining entries were existing `UnityEditor.Graphs` null-reference logs and MCP transport logs.

### History

- 2026-05-23: User asked Code Builder to stop treating Eve-C as a fixed-size radius zone and to make it follow the prefab collider/scale path like projectile and prefab-hitbox SingleAttack behavior.

## Task: 2026-05-23 Eve-C Shared AreaAttack Completion

### Task title

Implement Eve-C base runtime plus trait/master support on the shared AreaAttack path.

### Goals

- Keep Eve-C on the shared `AreaAttack` runtime with `Assets/Prefab/Skill/Eve/Eve_C.prefab` as the base scene visual.
- Support `trait-3` cooldown reduction through the existing choice cooldown multiplier.
- Support `trait-5` and `master-1` freeze-duration bonuses through a targeted choice status-duration bonus path.
- Support `master-1` as a shared threshold-status rule: `chill >= 4 -> freeze`.
- Support `master-2` as a shared `OnExpire` effect that bursts once from the zone center with `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Eve-C must stay on shared AreaAttack and multi-effect runtime paths; no Eve-only executor branch.
- Unity Play Mode gameplay verification remains user-owned.
- Native `codex review --uncommitted` could not complete because the local review command failed first on missing PATH and then on blocked websocket/network access, so final Reviewer evidence is a manual pass over the changed diff plus build results.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, compile-verified, and manual-review-passed. Unity-MCP menu/console calls timed out during CSV runtime sync, so runtime catalog prefab evidence for `eve-c-master-2` was recorded from the serialized asset file after a direct catalog update.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-C ticks `chill`, immediately freezes at 4+ chill stacks when `master-1` is learned, and that freeze duration increases only for Eve-C trait/master paths.
- User verifies that `master-2` fires exactly once when the field ends and uses `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-c-trait-3` to `cooldown_multiplier=0.85`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-c-trait-5` to `status_duration_bonus_status_id=freeze` and `status_duration_bonus=1`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-c-master-1` to `status_duration_bonus_status_id=freeze`, `status_duration_bonus=1.5`, `threshold_status_id=chill`, `threshold_status_min_stacks=4`, and `threshold_apply_status_id=freeze`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `eve-c-master-2` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `eve-c-master2-expire-burst` with `effect_timing=OnExpire`, `attribute=Ice`, `base_damage=24`, `spell_power_coefficient=1.5`, `requires_active_choice_id=eve-c-master-2`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now routes choice-targeted status duration bonuses, threshold-status application, and zone `OnExpire` effects through the shared runtime.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs` now applies a second shared status when the newly applied source status reaches a configured stack threshold.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` now executes filtered `OnExpire` effect rows once before the zone actor is destroyed.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `SkillId: eve-c` to prefab GUID `383d4c700df69d44898dc953ea18b9d4`, which is `Assets/Prefab/Skill/Eve/Eve_C.prefab`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab` with GUID `30a4745c2cff29f41acf72125c981f67`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.monster_id -eq 'eve' -and $_.skill_id -eq 'eve-c' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned no rows after the edit.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-23: User asked Code Builder / Skill Builder to implement Eve-C with a shared choice status-duration bonus, a shared threshold-status rule for `chill >= 4 -> freeze`, and an `OnExpire` master-2 burst using `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab`.

## Task: 2026-05-21 Eve-A Recursive Branch Projectile Rule

### Task title

Implement Eve-A Arc Bolt branch recursion, branch damage falloff, and fallback branch directions on the shared projectile path.

### Goals

- Let Eve-A branch projectiles apply the same shared shock-on-hit rule as the parent projectile.
- Let branch projectiles branch again through the same shared projectile path.
- Keep branch damage falloff data-owned at 70% per generation.
- Keep trait 5 and master 1 branch chance as additive choice data instead of forced 100% set values.

### Constraints

- Role Owner is Code Builder.
- Keep the code change minimal and inside the existing shared projectile runtime.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented in the shared projectile actor and choice CSV, then compile-verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-A branch hits can recursively branch, apply shock, and fall off as `100 -> 70 -> 49`.
- If live tuning shows branch spread is too tight or too wide, tune only the fallback random-right angle range instead of adding Eve-only executor branches.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` now lets `TrySpawnBranches(...)` keep spawning up to `branchOnHit.Count`, use nearest enemies first, and fall back to `SpawnFallbackBranchProjectile(...)` when nearby targets are missing.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` now initializes child branch projectiles with the parent `statusOnHit` and `branchOnHit.CloneForChild()` instead of `null`, which keeps shock application and recursive branch checks on the shared path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` still scales child damage by `damage * branchOnHit.DamageMultiplier`, so `branch_damage_multiplier=0.7` yields the requested chained falloff.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-trait-5` to `branch_chance_bonus=0.35`, blank `branch_chance_set`, `branch_count=2`, and `branch_damage_multiplier=0.7`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `eve-a-master-1` to `branch_chance_bonus=0.6`, blank `branch_chance_set`, `branch_count=2`, `branch_damage_multiplier=0.7`, and `branch_search_radius=4.5`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv` returned `eve-a-trait-5 0.35 / blank / 2 / 0.7` and `eve-a-master-1 0.6 / blank / 2 / 0.7 / 4.5` for the branch fields after the edit.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the edit; existing MSB3277 warnings remained.

### History

- 2026-05-21: User required the new Arc Bolt branch rule to be implemented as minimal shared runtime code plus CSV tuning instead of an Eve-only special-case executor.

## Source: boards\MON\RIN_MONSTER.md

## Task: 2026-05-19 Rin-A Shared Projectile Wiring

### Task title

Wire `rin-a` into the current shared projectile runtime and common modifier table.

### Goals

- Bind `rin-a` base projectile visuals through the active `EffectManager` scene mapping.
- Keep `rin-a` on the shared `MagazineProjectile` runtime path.
- Add the common projectile-compatible Rin-A choice modifiers to `SkillChoiceModifierData.csv`.
- Leave unsupported crit-only or sequence-state behavior explicitly unsupported instead of guessing new monster-only runtime logic.

### Constraints

- Role Owner is Code Builder.
- Claims are based on inspected Scripts2 runtime code, active scene YAML, active modifier CSV, and the inspected `Rin_A.prefab` asset path provided by the user.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by file inspection and build.

### Next Actions

- User verifies in Play Mode that `rin-a` now spawns `Rin_A.prefab` through the shared projectile path.
- If full `rin-a-trait-5` crit modifiers or `rin-a-master-2` extra lightning / every-third-hit chain are required in Scripts2 runtime, request a shared extension or a one-off approved exception before implementing.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Rin/Rin_A.prefab` exists and its prefab GUID is `19bfba788239eba498a44cb67c2622c6`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps monster `rin` skill `rin-a` to `Rin_A.prefab` through the `EffectManager` `monsterSkillEffects` list.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` already maps projectile active rows into `ProjectileSkillData`, including magazine size, reload, shot interval, projectile speed, pierce count, and on-hit status.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` already routes `ProjectileSkillData` through `ProjectileSkillExecutor`, resolves base visuals through `EffectManager.ResolveMonsterSkillEffectPrefab(...)`, and applies modifier snapshot bonuses for additional projectiles and pierce.
- `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` now includes common-path `rin-a` rows for trait 1/2/3/4 and master 1, while trait 5 and master 2 are marked `DataOnlyUnsupported` because current shared projectile runtime has no crit modifier fields and no built-in every-third-hit chain behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-19: User requested Code Builder implementation of `rin-a`.
- 2026-05-19: User clarified the base effect prefab path as `Assets/Prefab/Skill/Rin/Rin_A.prefab`.

## Task: 2026-05-14 Rin NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Rin prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Rin_Unit` through `NewRunSceneEntryManager`.
- Verify Rin creates an exact `rin` runtime model and initializes `MonsterUnitActor`.
- Make Rin's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Rin combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified. 2026-05-18 Rin active skill CSV rows were updated to the new skill-owned projectile/status schema. 2026-05-18 Rin design-only labels remain non-runtime statuses with `status_chance=0`.

### Next Actions

- User verifies Rin selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Rin_Unit.prefab` in `rinUnitPrefab`.
- Unity-MCP verification returned `rin:prefab=Rin_Unit|modelOk=True|model=rin|actor=True|actorModel=True|hpText=HP 260/260|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Rin_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `rin-a` `projectile_speed=13`, `pierce_count=0`, `magazine_capacity=10`, `reload_seconds=4`, and `shot_interval_seconds=0.34`, matching `Pakuri/reference/2.Monster/rin/skill/a-shattering-fist.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `rin-c` `radius=1.6` and `status_effect_label=?됰갚`, matching `Pakuri/reference/2.Monster/rin/skill/c-shockwave.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores Rin design-only labels `?됰룞?띾룄 利앷?` and `?됰갚` with `status_chance=0`; runtime CSV validation rejects positive chance on unsupported status labels.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` can still resolve supported labels such as `媛먯쟾` from `status_effect_label` if a Rin row is intentionally edited to use a supported status later.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.
- 2026-05-18: Code Builder moved Rin projectile/status tuning into the skill CSV row and filled Rin-C width from the reference document.
- 2026-05-18: Code Builder normalized Rin design-only status labels to chance 0 and added supported status-label fallback/CSV sync batch support.

## Task: 2026-05-18 Rin-E SingleAttack Runtime Kind

### Task title

Route Rin-E collapse strike through the new SingleAttack runtime kind.

### Goals

- Keep Rin-E as one-shot area damage rather than sustained `AreaAttack`.
- Preserve CSV-authored damage, coefficient, radius, and cooldown.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Rin-E applies one immediate area hit.

### Evidence

- `Pakuri/reference/2.Monster/rin/skill/e-collapse-strike.md` names Rin-E `遺뺢눼 ?寃?.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `rin-e runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV row 17 as a one-shot area attack skill for the new `SingleAttack` type.

## Task: 2026-05-13 Rin Battlefield Facade Registration

### Task title

Route Rin battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Rin skill behavior while replacing direct battlefield list registration writes.
- Keep Rin projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin skills in Play Mode if needed.

### Evidence

- `CombatRuntimeRinSkills.cs:575` now calls `AddBattlefieldProjectile(...)`.
- Rin skill-effect registrations now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Rin battlefield object registration through facade methods.

## Task: 2026-05-08 Rin CombatUnitRuntime Parity Resume

### Task title

Route selected Rin and manifested Rin through shared unit skill runtime paths.

### Goals

- Make selected 1P Rin and manifested 2P-5P Rin call `CombatUnitRuntime` plus `CombatSkillRuntime` based execution for Rin B/C/D/E.
- Preserve Rin A magazine/projectile handling on the existing path.
- Keep manifested Rin Howling buff duration and Howling dark follow-up on the unit runtime, not on selected-only fields.
- Reuse existing RunScene slot status children for manifested monster name, HP text, and HP/shield bars.

### Constraints

- Role Owner is Code Builder.
- Claims are based on inspected files, Unity-MCP scene hierarchy output, and command output.
- Unity Play Mode verification is user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build, Unity refresh, and console checks.

### Next Actions

- User verifies selected Rin and manifested Rin B/C/D/E behavior in RunScene Play Mode.
- User verifies 2P-5P monster status UI does not duplicate labels or bars when manifested monsters appear.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:76` defines `TickSelectedRinUnitSkillRuntimes(...)` for selected Rin skill runtime ticking.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:128` routes Rin automatic skill execution through `TryTriggerRinUnitAutomaticSkills(CombatUnitRuntime runtime)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:240`, `:321`, and `:401` implement unit-runtime casts for Rin B, Rin D, and Rin E; Rin C is routed through the same unit skill tick and manifested shockwave path.
- `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs:15` through `:18` stores separate name label, HP label, HP bar fill, and shield bar fill references.
- `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs:25`, `:59`, `:104`, and `:128` store, tick, and reset manifested Rin Howling state on the unit runtime.
- Unity-MCP scene hierarchy inspection found `CombatRoot/2PMonster`, `3PMonster`, `4PMonster`, `5PMonster`, and `EveUnit`; 2P/3P/Eve children included `MonsterHpLabel`, `MonsterHpBar/Fill`, `MonsterHpBar/Shield`, and `MonsterNameLabel`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP script refresh reached idle; console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-08: User resumed an interrupted request to start from Rin and make selected 1P and manifested 2P-5P monsters use the same `CombatUnitRuntime` plus `CombatSkillRuntime` execution basis.

## Task: 2026-05-08 Manifested Rin C Shockwave Parity Fix

### Task title

Make manifested Rin C apply selected Rin C beam and knockback behavior.

### Goals

- Fix manifested Rin C so it does more than visual line damage.
- Apply selected Rin C's map-wide beam hit shape, knockback, width choices, master slow, master lightning follow-up, and reload reduction behavior where applicable.
- Keep damage multiplier sourced through existing manifested Rin C choice multiplier logic.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Rin C knockback in RunScene Play Mode.
- User verifies Rin C master/trait choices if those choices are learned on the manifested Rin.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:220` through `:310` shows selected Rin C uses map-wide range, `IsPointInsideBeam(...)`, `ApplyRinKnockback(...)`, master lightning follow-up, master slow, and trait reload reduction.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:499` routes manifested `rin-c` into `TryFireManifestedRinShockwave(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:545` implements the manifested Rin C beam path using selected-runtime helper methods and manifested Offering checks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:627` reduces manifested Rin A reload when manifested Rin C trait 5 hits while Rin A is reloading.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported selected Rin C knockback works, but manifested Rin C only showed effect/beam without moving enemies.

## Task: 2026-05-08 Manifested Rin Common Runtime Parity

### Task title

Apply Rin Offering choices through manifested projectile and common skill runtime.

### Goals

- Keep manifested Rin skills sourced from `SkillDefinition` data.
- Apply Rin manifested Offering choices in shared damage, cooldown, magazine, reload, and shot interval paths.
- Preserve manifested projectile/status handling through the common combat service.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Rin private skill code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Rin skills and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Rin skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Rin cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1250`, `:1278`, and `:1310` include Rin A magazine/reload/shot-interval choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:693` applies manifested projectile status from skill data.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: Manifested Rin common runtime parity was implemented and retained as the latest active Rin task block during MON board compaction.

## Required Sections For Future Work

- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Task: 2026-05-08 Manifested Rin Passive And Targeting Continuation

### Task title

Make manifested Rin use Rin passive skill runtime effects and participate as an enemy target.

### Goals

- Apply Rin F-J passive effects to manifested Rin A/C/D/E runtime paths through `CombatUnitRuntime`.
- Keep manifested Rin cooldown ticking affected by Rin action-speed passives.
- Fix missing manifested HP slide bar fallback.
- Allow enemies to target and damage manifested Rin and other manifested monsters.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification is user-owned.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build, diff check, Unity refresh, and console read.

### Next Actions

- User verifies in RunScene Play Mode that manifested Rin gets passive effects from Offering, has one HP bar, and can be attacked by enemies.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:197` ticks manifested Rin unit skill cooldowns with `GetRinUnitActionSpeedMultiplier(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1073` adds `TryApplyRinUnitProjectileHit(...)` for manifested Rin projectile damage with unit passive modifiers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1269` tracks manifested Rin physical hit count for Rin H.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:1848` implements manifested Rin action-speed passive calculation.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:793` routes manifested Rin C damage through `ApplyRinUnitSkillDamage(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `git diff --check` over touched combat files completed with exit code 0 and CRLF warnings only.
- Unity-MCP script refresh requested compilation; console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-08: User requested resuming work so manifested Rin gains passive skills like selected Rin, manifested monsters have HP slide bars, and enemies attack manifested monsters too.

## Source: boards\MON\VEGA_MONSTER.md

## Task: 2026-05-14 Vega NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Vega prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Vega_Unit` through `NewRunSceneEntryManager`.
- Verify Vega creates an exact `vega` runtime model and initializes `MonsterUnitActor`.
- Make Vega's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Vega combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified. 2026-05-18 Vega active skill CSV rows were updated to the new skill-owned projectile/status schema. 2026-05-18 Vega design-only labels remain non-runtime statuses with `status_chance=0`.

### Next Actions

- User verifies Vega selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Vega_Unit.prefab` in `vegaUnitPrefab`.
- Unity-MCP verification returned `vega:prefab=Vega_Unit|modelOk=True|model=vega|actor=True|actorModel=True|hpText=HP 225/225|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Vega_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `vega-a` `projectile_speed=16`, `magazine_capacity=5`, `reload_seconds=4.8`, and `shot_interval_seconds=0.55`, matching `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` stores `vega-a` `pierce_count=999` as the current finite-runtime sentinel for the reference document's `臾댄븳 愿??, because `InGameProjectileActor` currently consumes integer pierce counts.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` records Vega labels `?대쫫?쒖떇`, `移⑤У`, `紐곗궡 ?덇?`, and `?대쫫?쒖떇 ?곌퀎`; these are design labels because the current `StatusEffectKind` enum does not include those Vega-specific statuses.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `vega-b` `radius=1.8`, matching `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps Vega design labels `?대쫫?쒖떇`, `移⑤У`, `紐곗궡 ?덇?`, and `?대쫫?쒖떇 ?곌퀎` with `status_chance=0`; runtime CSV validation rejects positive chance on unsupported status labels.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` can resolve supported labels from `status_effect_label` if a Vega row is intentionally edited to a supported status such as `媛먯쟾` later.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.
- 2026-05-18: Code Builder moved Vega projectile/status tuning into skill CSV rows and encoded the reference infinite pierce as `pierce_count=999` for the current finite common projectile runtime.
- 2026-05-18: Code Builder normalized Vega design-only status labels to chance 0 and added supported status-label fallback/CSV sync batch support.

## Source: boards\OPS\AUTOMATION_GUIDE.md

## Task: 2026-05-23 Skill Builder Contact-Hitbox And Cooldown Authority Policy

### Task title

Clarify Skill Builder policy for prefab-contact attack skills and existing cooldown CSV authority.

### Goals

- Make Skill Builder default projectile, `SingleAttack`, and `AreaAttack` work use prefab-authored contact behavior when the shared runtime path already supports collider hitboxes.
- Keep explicit target-designated debuffs/marks and battlefield/global-aura effects on their existing non-contact structures instead of forcing them into collider-contact blueprints.
- Make cooldown reduction requests reuse existing CSV cooldown authority such as `cooldown_seconds` and `cooldown_multiplier` instead of inventing a new percentage-only field.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- Claims must stay grounded in inspected Skill Builder policy markdown and current skill CSV headers.
- No C# script, scene, prefab, or gameplay CSV values are changed by this task.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown and CSV-header inspection.

### Next Actions

- Future Skill Builder requests for projectile, `SingleAttack`, and `AreaAttack` should assume prefab-contact as the common path when the shared runtime/prefab supports it.
- Future cooldown reduction requests should edit existing cooldown-owned fields instead of proposing a new cooldown-percent field.

### Evidence

- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now states that projectile, `SingleAttack`, and `AreaAttack` should prefer shared prefab-contact hitbox behavior when that runtime path exists, while explicit target-designated and global-effect skills stay on non-contact structures.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now states that cooldown reduction work must reuse existing cooldown CSV authority and names base `cooldown_seconds` plus choice `cooldown_multiplier`.
- `boards/SkillBluePrint/projectile-blueprint.md`, `single-attack-blueprint.md`, and `area-attack-blueprint.md` now describe prefab-contact behavior as part of the common path and name explicit target-designated / battlefield-global cases as stop-and-ask non-contact structures.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1` contains the base cooldown column `cooldown_seconds`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:1` contains the enhancement cooldown column `cooldown_multiplier`.

### History

- 2026-05-23: User asked to document that projectile, `SingleAttack`, and `ZoneAttack` should be prefab-based by default while target-designated skills keep their other structure, and that cooldown reduction n% should manipulate existing cooldown CSV authority.

## Task: 2026-05-22 Skill Builder Companion Docs Compression

### Task title

Compress Skill Builder CSV companion docs into an exception-only path and restore blueprint-only as the default workflow.

### Goals

- Keep normal Skill Builder work on one selected blueprint only.
- Reduce the earlier three CSV companion docs to a smaller exception-only set.
- Allow exception docs only when blueprint-only work cannot safely continue.
- Update policy/routing files so Skill Builder does not over-read by default.

### Constraints

- Role Owner is Designer for the workflow contract and Code Builder for the markdown changes.
- Claims must stay grounded in inspected `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, `MDTREE.md`, `BLACKBOARD.md`, `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No runtime C# behavior, scene object, prefab, or gameplay CSV values are changed by this task.

### Role Owner

Designer / Code Builder

### Status

Implemented and locally verified by markdown/file inspection.

### Next Actions

- Future Skill Builder requests should try blueprint-only first.
- Use `skill-csv-exception-guide.md` plus `skill-builder-handoff-format.md` only when a scoped row bundle or row-combination ambiguity blocks blueprint-only work.

### Evidence

- Deleted `boards/SkillBluePrint/skill-csv-schema-dictionary.md`.
- Deleted `boards/SkillBluePrint/skill-csv-pattern-guide.md`.
- Added `boards/SkillBluePrint/skill-csv-exception-guide.md`.
- Kept `boards/SkillBluePrint/skill-builder-handoff-format.md` and rewrote it as an exception-path handoff doc.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now says the default Skill Builder path is blueprint-only and allows exception docs only when blueprint-only work cannot safely continue.
- `MDTREE.md` now lists only the two exception docs and describes them as exception-path workflow docs.

### History

- 2026-05-22: User pointed out that reading CSV interpretation docs on every skill task defeats the purpose of blueprint-first work and asked for compression plus exception-only usage.

## Task: 2026-05-22 Skill Builder CSV Companion Docs And Routing

### Task title

Add shared CSV interpretation and handoff companion docs for Skill Builder and route them through the active policy files.

### Goals

- Add one shared schema dictionary for the current skill CSV tables.
- Add one shared pattern guide for combining base, choice, effect, and trigger rows.
- Add one normalized handoff-format guide so Skill Builder can implement from a scoped row bundle plus one selected blueprint.
- Update Skill Builder routing so these docs are explicitly allowed companion reads rather than ad hoc extra markdown.

### Constraints

- Role Owner is Designer for the workflow contract and Builder for the markdown file creation handoff.
- Claims must stay grounded in inspected `monster_skills.csv`, `monster_skill_choices.csv`, `monster_skill_effects.csv`, `monster_skill_triger.csv`, `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, `MDTREE.md`, and existing blueprint files.
- No runtime C# behavior, scene object, prefab, or gameplay CSV values are changed by this task.

### Role Owner

Designer / Code Builder

### Status

Implemented and locally verified by targeted markdown/file inspection.

### Next Actions

- Future Skill Builder requests that start from scoped CSV rows should still choose exactly one primary blueprint, then use these companion docs only to interpret the row bundle.
- If future skill authoring adds new skill CSV tables, update these companion docs together with `AGENTS_ROLE/GAMEBULIDER_SKILL.md` and `MDTREE.md`.

### Evidence

- Added `boards/SkillBluePrint/skill-csv-schema-dictionary.md`.
- Added `boards/SkillBluePrint/skill-csv-pattern-guide.md`.
- Added `boards/SkillBluePrint/skill-builder-handoff-format.md`.
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now explicitly allows those three files as shared companion docs for CSV-driven Skill Builder work while still requiring exactly one selected blueprint.
- `MDTREE.md` now lists the three companion docs and clarifies that Skill Builder policy work may read only the specifically justified companion docs under `boards/SkillBluePrint/`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1-2`, `monster_skill_choices.csv:1-2`, `monster_skill_effects.csv:1-2`, and `monster_skill_triger.csv:1-2` were inspected as the grounding schema sources for the new docs.

### History

- 2026-05-22: User rejected adding explanation CSVs and instead asked for three md companion docs plus routing changes so Skill Builder can work from `csv + blueprint`.

## Task: 2026-05-22 Multi-Effect Skill CSV Blueprint

### Task title

Add a reusable multi-effect skill CSV blueprint and Skill Builder route.

### Goals

- Document the reusable CSV-owned route for skills that need secondary damage, ally buffs, delayed waves, or choice-gated effects.
- Keep Ariel-C style behavior out of monster-specific executor hardcoding.
- Add Skill Builder routing for `monster_skill_effects.csv` / multi-effect skill work.
- Extend the blueprint contract to cover separated effect centers and applied-target visual anchors.

### Constraints

- Role Owner is Designer for the blueprint and routing contract.
- Runtime implementation is recorded in DATA/COMBAT/Ariel boards.
- Claims are grounded in inspected `monster_skills.csv`, `SkillDefinition.cs`, `PakuriCsvRuntimeData.*`, and `SkillExecutors.cs`.

### Role Owner

Designer / Skill Builder

### Status

Implemented and locally verified.

### Next Actions

- Future bundled ally-effect or choice-gated secondary-effect skills should start from `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md`.
- If a future effect cannot fit the CSV columns, extend the blueprint/schema before adding executor branches.

### Evidence

- Added `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md`.
- `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md` now documents `center_mode` and `visual_anchor_mode`, including `PrimarySkillCenter` for delayed waves and `AppliedTargets` for unit-attached buff visuals.
- Updated `AGENTS_ROLE/GAMEBULIDER_SKILL.md` so multi-effect skill and `monster_skill_effects.csv` requests route to the new blueprint.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now invokes a shared `SkillMultiEffectExecutor` from the `SingleAttack` path instead of adding an Ariel-C branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now keeps multi-effect application targets separate from visual centers/anchors through generic `SkillMultiEffectCenterMode` and `SkillMultiEffectVisualAnchorMode` fields.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: User approved the Designer blueprint first, then Skill Builder implementation for reusable multi-effect skill CSV support.
- 2026-05-22: Code Builder updated the multi-effect blueprint and implementation so future skills can express applied-target attached visuals and primary-skill-center secondary waves without monster-specific executor branches.

## Task: 2026-05-21 SingleAttack And AreaAttack Blueprint Contracts

### Task title

Add parsed-input Skill Builder blueprints for SingleAttack and AreaAttack.

### Goals

- Add a `SingleAttack` blueprint for one-shot area damage skills.
- Add an `AreaAttack` blueprint for sustained ticking area skills.
- Keep both new blueprints aligned with the existing projectile / BeamSkill parsed-input contract style.
- Update Skill Builder blueprint selection so `SingleAttack` and `AreaAttack` requests route to the new files.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in inspected runtime scripts and existing blueprint text.
- `AreaAttack` must not silently absorb `Field`, `Mark`, or `Execute` behavior even though the current runtime maps those kinds to `ZoneSkillData`.

### Role Owner

Designer

### Status

Implemented and locally verified by markdown and targeted code-path inspection.

### Next Actions

- Future `SingleAttack` implementation requests should read `boards/SkillBluePrint/single-attack-blueprint.md` as the first-read contract.
- Future `AreaAttack` implementation requests should read `boards/SkillBluePrint/area-attack-blueprint.md` as the first-read contract.
- If future area-like work is actually `Field`, `Mark`, `Execute`, drone, trap, marked-target fanout, or ally-effect bundled behavior, create or select a more specific blueprint instead of forcing it through these contracts.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` and `boards/SkillBluePrint/BeamSkill-blueprint.md` define the parsed-input contract structure copied for the new blueprint style: `Purpose`, `Core Rule`, `Builder Working Mode`, `What Builder May Read`, `Required Parsed Input`, common contract, stop-and-ask rule, and Builder output.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:69-75` maps `SkillRuntimeKind.SingleAttack` to `SingleAttackData` and `SkillRuntimeKind.AreaAttack` to `ZoneSkillData`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:145-163` maps radius, duration, tick interval, cover-all, damage, and status into `ZoneSkillData` / `SingleAttackData`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:479` shows `ZoneSkillExecutor`; `:611` shows `SingleAttackSkillExecutor`; `:628` routes SingleAttack through `InGameZoneSkillActor.ApplyAreaTick(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs:24-66` initializes and applies an immediate area tick; `:140-160` repeats ticks until duration expires.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:194-195` registers `SingleAttackSkillExecutor` and `ZoneSkillExecutor` by default.
- Added `boards/SkillBluePrint/single-attack-blueprint.md`.
- Added `boards/SkillBluePrint/area-attack-blueprint.md`.
- Updated `AGENTS_ROLE/GAMEBULIDER_SKILL.md` known mappings for `SingleAttack` and `AreaAttack`.

### History

- 2026-05-21: User asked whether `SingleAttack` and `AreaAttack` blueprints could be created before implementing new skills; Designer inspected existing shared runtime evidence and concluded the contracts can be created first.
- 2026-05-21: User approved creating two blueprint files following the existing blueprint format and similar routing path.

## Task: 2026-05-21 Beam Blueprint Contract Rewrite

### Task title

Rewrite the BeamSkill blueprint to match the parsed-input contract style of the projectile blueprint.

### Goals

- Make `boards/SkillBluePrint/BeamSkill-blueprint.md` a blueprint-first contract instead of a value-rediscovery guide.
- Align BeamSkill routing, read-set boundaries, stop-and-ask behavior, and Builder output expectations with `boards/SkillBluePrint/projectile-blueprint.md`.
- Keep the BeamSkill blueprint grounded in the currently inspected shared `LineAttack` runtime path.

### Constraints

- Role Owner is Designer because this task changes workflow/design policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in inspected Beam runtime scripts and the current projectile blueprint text.

### Role Owner

Designer

### Status

Implemented and locally verified by markdown and targeted code-path inspection.

### Next Actions

- Future BeamSkill implementation requests should treat this blueprint as the first-read contract and should not reopen CSV/reference files for ordinary numeric rediscovery.
- If future Beam behavior requires charge phases, sweep arcs, stop-first-target, or other unsupported line behavior, extend the shared contract deliberately instead of weakening the stop-and-ask rule.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` is the active parsed-input contract reference that now defines the desired structure: `Purpose`, `Core Rule`, `Builder Working Mode`, `What Builder May Read`, `Required Parsed Input`, `Common ... Contract`, `Stop And Ask User Rule`, and `Required Builder Output`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:67-68` maps `SkillRuntimeKind.LineAttack` to `BeamSkillData`, and `:112-113` plus `:139` map active duration, tick interval, and beam width from runtime data.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:318-388` shows `BeamSkillExecutor` as the shared Beam path; `:428-469` confirms duration, width, and tick interval are resolved through shared helpers and snapshot modifiers.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs:25-56` shows immediate first tick on initialization, and `:119-149` shows repeated ticking until duration ends.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs:70-117` applies shared damage and status through `InGameCombatManager` but does not own special stop-first-target, knockback, or curved/sweeping behavior.
- `boards/SkillBluePrint/BeamSkill-blueprint.md` now follows the projectile-style contract and no longer tells Builder to rediscover numbers from CSV/reference files by default.

### History

- 2026-05-21: User explicitly asked to modify `BeamSkill-blueprint.md` like `projectile-blueprint.md`, verify routing, and perform the work in the Designer role.

## Task: 2026-05-20 Projectile Blueprint Burst Contract Update

### Task title

Promote uniform sequential projectile burst into the common projectile blueprint contract.

### Goals

- Keep `Skill Builder` from stopping on ordinary sequential burst projectile skills after the shared runtime extension.
- Document `BurstProjectileCount` as the common input for sequential projectile volleys.
- Keep special non-uniform delayed projectile behavior in the stop-and-ask path.

### Constraints

- Role Owner is Code Builder / Skill Builder because this policy update follows an implemented runtime extension.
- Do not broaden Skill Builder markdown reads.
- Keep unsupported special sequence behavior explicit.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented.

### Next Actions

- Future projectile blueprints should distinguish `BurstProjectileCount` sequential shots from simultaneous fan-spread projectile count.

### Evidence

- `boards/SkillBluePrint/projectile-blueprint.md` now lists `BurstProjectileCount` in required parsed input.
- `boards/SkillBluePrint/projectile-blueprint.md` now states that `BurstProjectileCount` is for sequential same-direction projectiles fired within one magazine cycle.
- `boards/SkillBluePrint/projectile-blueprint.md` now keeps non-uniform delayed repeated firing in the stop-and-ask examples.

### History

- 2026-05-20: Code Builder added shared burst projectile runtime support for Sein-B and updated the projectile blueprint so future Skill Builder work can use the new common contract.

## Task: 2026-05-19 OPS Board Active Compaction

### Task title

Compact OPS active boards and archive older operational task blocks.

### Goals

- Keep OPS active files focused on current operational state.
- Move older completed automation and reviewer task blocks to `boards/ARCHIVE/`.
- Preserve all moved task history instead of deleting it.
- Verify active and archive task counts after compaction.

### Constraints

- Role Owner is Designer because this task restructures persistent markdown state, not runtime code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Keep `CODEX_CLI_BLACKBOARD.md` and `UNITY_MCP_BLACKBOARD.md` active because each has only one task block.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- Use `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md` for older automation/policy history.
- Use `boards/ARCHIVE/OPS_REVIEWER_ARCHIVE_2026-05-19.md` for older completed Reviewer history.
- Keep future OPS active files limited to current unresolved or recently relevant task blocks.

### Evidence

- Before compaction, `AUTOMATION_GUIDE.md` had 552 lines and 19 task blocks.
- Before compaction, `REVIEWER_BLACKBOARD.md` had 171 lines and 6 task blocks.
- After compaction, `AUTOMATION_GUIDE.md` had 123 lines and 4 task blocks before this new task block was added.
- After compaction, `REVIEWER_BLACKBOARD.md` had 57 lines and 2 task blocks.
- Added `boards/ARCHIVE/OPS_AUTOMATION_ARCHIVE_2026-05-19.md` with 436 lines and 15 archived task blocks.
- Added `boards/ARCHIVE/OPS_REVIEWER_ARCHIVE_2026-05-19.md` with 121 lines and 4 archived task blocks.

### History

- 2026-05-19: User asked whether OPS markdown files were necessary, then requested cleaning the files under `boards/OPS`.

## Task: 2026-05-19 Role Markdown Common Rule Compaction

### Task title

Move repeated role rules into a shared common role file.

### Goals

- Keep `AGENTS.md` as the startup and role-entry authority.
- Add a shared common role file instead of repeating evidence, Unity Play Mode, Git, Reviewer, and board-update boundaries across role files.
- Compact Designer, Code Builder, Skill Builder, Code Reviewer, and track files so they keep only role-specific or track-specific instructions.
- Preserve `SimpelWorker` as a minimal role that does not read additional markdown after `AGENTS.md` and `MDTREE.md`.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- `AGENTS.md` remains the required startup file; it was not renamed to `AGENTS_COMMON.md`.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- Future role files should add only role-specific instructions and avoid repeating rules already owned by `AGENTS.md`, `MDTREE.md`, or `AGENTS_ROLE/COMMON.md`.
- If more shared rules are needed, add them to `AGENTS_ROLE/COMMON.md` and keep downstream files as references.

### Evidence

- Added `AGENTS_ROLE/COMMON.md` with shared evidence/failure rules, Unity Play Mode boundary, Git and Reviewer boundary, and board update boundary.
- `AGENTS.md` now instructs Designer, Code Builder, Skill Builder, and Code Reviewer to read `AGENTS_ROLE/COMMON.md`; `SimpelWorker` remains excluded.
- `MDTREE.md` now lists `AGENTS_ROLE/COMMON.md` as shared role rules.
- Removed repeated highest-evidence-rule text from `AGENTS_ROLE/GAMEDESIGNER.md`, `AGENTS_ROLE/GAMEBULIDER.md`, and `AGENTS_ROLE/GAMEREVIWER.md`.
- Removed repeated Play Mode, Git, evidence, and board-update boundary text from lower role/track files where those rules are now covered by `AGENTS_ROLE/COMMON.md`.
- Updated Skill Builder and skill blueprint read sets to include `AGENTS_ROLE/COMMON.md`.
- `git diff --check -- AGENTS.md MDTREE.md AGENTS_ROLE\*.md boards\SkillBluePrint\projectile-blueprint.md boards\SkillBluePrint\BeamSkill-blueprint.md BLACKBOARD.md boards\OPS\AUTOMATION_GUIDE.md` completed with no whitespace errors, aside from Git LF-to-CRLF normalization warnings.

### History

- 2026-05-19: User observed that core role markdown files repeated the same requirements and asked to apply the recommended common-rule structure.

## Task: 2026-05-19 Skill Builder Track Routing

### Task title

Move skill implementation blueprint routing into a dedicated Skill Builder track.

### Goals

- Stop adding individual skill-type blueprint rules directly to the Code Builder entry file.
- Add a reusable `Skill Builder` track for projectile, BeamSkill, future zone, and future skill blueprints.
- Keep future skill implementation markdown reads to `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEBULIDER.md`, `AGENTS_ROLE/GAMEBULIDER_SKILL.md`, and exactly one matching blueprint unless the selected blueprint or inspected failure path justifies more.
- Verify by simulated routing that unrelated MON, DATA, RUN, UI, OPS, archive, and other skill blueprint markdown are excluded for a simple projectile Skill Builder request.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- The repository does not contain `CODEBUILDER.md`; the inspected Code Builder entry file is `AGENTS_ROLE/GAMEBULIDER.md`.

### Role Owner

Designer

### Status

Implemented and locally verified by targeted markdown checks.

### Next Actions

- Future skill implementation requests should invoke `Skill Builder` and name or imply exactly one skill blueprint.
- Add future skill types by creating a new `boards/SkillBluePrint/*-blueprint.md` file and, when helpful, adding only a short mapping line in `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- If `zone-blueprint.md` is needed, create it before asking Skill Builder to implement zone/area/field skills through that blueprint.

### Evidence

- `AGENTS_ROLE/GAMEBULIDER_SKILL.md` now exists and defines the `Skill Builder` track, mandatory markdown read set, blueprint selection, blueprint authority, parsed-input rule, unsupported-behavior rule, routing decision log, and output requirements.
- `AGENTS.md` now recognizes `Skill Builder` as a Code Builder track and routes it through `AGENTS_ROLE/GAMEBULIDER.md` then `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- `AGENTS_ROLE/GAMEBULIDER.md` now routes skill implementation, skill runtime wiring, skill prefab/effect connection, and user-invoked `Skill Builder` work to `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- Removed the previous `Projectile Skill Blueprint Rule` and `BeamSkill Blueprint Rule` sections from `AGENTS_ROLE/GAMEBULIDER.md`.
- `MDTREE.md` now lists `AGENTS_ROLE/GAMEBULIDER_SKILL.md` under Code Builder track files.
- `boards/SkillBluePrint/projectile-blueprint.md` now uses `AGENTS_ROLE/GAMEBULIDER_SKILL.md` instead of `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md` in its mandatory/allowed markdown read set.
- `boards/SkillBluePrint/BeamSkill-blueprint.md` now has a `What Builder May Read` section using the Skill Builder mandatory read set and explicit conditional markdown rules.

### History

- 2026-05-19: User said the direct projectile/BeamSkill insertions in Code Builder felt messy and would not scale to future `zone_blueprint.md` and other skill blueprints.
- 2026-05-19: User requested a new role named `Skill Builder`, an explanation of deleted/added content, and a simulation proving the Skill Builder path reads only the intended markdown files.

## Task: 2026-05-19 Minimal Markdown Routing Tightening

### Task title

Tighten routing rules so Codex reads the smallest justified markdown set and skips unrelated boards by default.

### Goals

- Split routing guidance into mandatory reads versus conditional reads.
- Explicitly forbid reading unrelated domain markdown "just in case."
- Require a short routing decision log before broader work.
- Tighten the projectile blueprint so projectile implementation does not pull unrelated markdown by default.

### Constraints

- Role Owner is Designer because this task changes workflow policy, not runtime gameplay code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims must stay grounded in the inspected text of `AGENTS.md`, `MDTREE.md`, `AGENTS_ROLE/GAMEBULIDER.md`, and `boards/SkillBluePrint/projectile-blueprint.md`.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Future sessions should treat routing as a reduction step and justify every additional markdown read from the user request or the inspected failure path.
- Future projectile implementation tasks should start from the mandatory Builder set and add monster/DATA/RUN/UI boards only when the request or failure path explicitly requires them.

### Evidence

- `AGENTS.md` now says to decide the smallest markdown read set after reading `AGENTS.md` and `MDTREE.md`, to separate mandatory/conditional/excluded reads, and to avoid extra markdown reads "just in case."
- `AGENTS.md` now says that, when practical, the worker should state a short routing decision including request class, files to read next, and intentionally skipped markdown files.
- `MDTREE.md` now has `Minimal Read Set Rule`, explicit exclusion examples, and a policy-routing clause that sends root policy markdown edits to `boards/OPS/AUTOMATION_GUIDE.md` without automatically pulling MON/RUN/UI/DATA boards.
- `AGENTS_ROLE/GAMEBULIDER.md` now has `Minimal Builder Read Set` and `Routing Decision Log`, including explicit conditions for when monster, DATA, RUN, UI, and verification markdown may be added.
- `boards/SkillBluePrint/projectile-blueprint.md` now defines the default mandatory markdown set for projectile implementation and explicitly forbids unrelated UI/RUN/DATA/OPS/other-monster markdown reads unless the request or inspected failure path names those domains.

### History

- 2026-05-19: User noted that Codex could read unnecessary markdown under the existing routing wording and asked to apply the first four tightening ideas: mandatory/conditional split, explicit exclusions, routing decision log, and stronger projectile-blueprint bans.

## Task: 2026-05-19 Projectile Blueprint Parsed-Input And Stop-Ask Rewrite

### Task title

Rewrite the projectile blueprint around parsed input and stop-and-ask rules.

### Goals

- Change `boards/SkillBluePrint/projectile-blueprint.md` from a search-oriented guide into a blueprint-first contract for common projectile work.
- Make future projectile implementation tasks consume caller-provided parsed runtime inputs instead of rediscovering numbers from CSV or reference files.
- Make Builder stop and ask the user whenever a requested projectile behavior falls outside the current common projectile path.
- Remove overly heavy file-inventory style guidance when the real rule is "feed parsed data into the shared projectile runtime."

### Constraints

- Role Owner is Designer because this task changes implementation design policy, not runtime code.
- No C# script, scene, prefab, or CSV gameplay behavior was changed.
- Claims are based on the inspected current shared projectile runtime explanation already grounded in `InGameSkillDefinitionMapper.cs`, `SkillExecutors.cs`, `InGameProjectileActor.cs`, `SkillExecutionSystem.cs`, and the previous projectile blueprint text.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Future projectile implementation tasks should treat `boards/SkillBluePrint/projectile-blueprint.md` as the primary contract and should not reopen CSV/reference sources unless the user explicitly instructs that.
- If a task request does not include the required parsed input fields, Code Builder should stop and report the missing fields instead of searching for them.
- If a task requests timed burst, homing, bounce, last-shot explosion, trap/install, impact-area, mark payload, or other special projectile behavior, Code Builder should stop and ask the user instead of guessing.

### Evidence

- The previous `boards/SkillBluePrint/projectile-blueprint.md` explicitly redirected Builder toward large CSV/reference rediscovery and then toward a heavy `Fixed Implementation Surface` file list.
- The rewritten `boards/SkillBluePrint/projectile-blueprint.md` now centers on `Core Rule`, `Builder Working Mode`, `Required Parsed Input`, `Common Projectile Contract`, `Stop And Ask User Rule`, and `Preferred Builder Response Pattern`.
- The rewritten blueprint now states that projectile numbers and behavior intent must come from caller-provided parsed input, that shared projectile runtime is the default path, and that unsupported special behavior must trigger a user question instead of an inferred implementation.
- 2026-05-19 follow-up: `Optional but common fields` was narrowed to `ChoiceModifierSpecs`, `OnHitStatusId`, `OnHitStatusChance`, `ProjectilePrefabSource`, and `SkillEffectPrefabOverride`; fields such as `ProjectileCount`, `LifetimeSeconds`, `MaxTravelDistance`, `DestroyBoundaryPolicy`, `HitRadius`, `OnHitStatusStacks`, and `OnHitStatusDurationSeconds` were moved out of the current common projectile input contract.
- 2026-05-19 follow-up header check showed the active `Pakuri/Assets/CSVdata/source/monster_skills.csv` does not currently contain those removed fields, while `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` still contains `skill_effect_prefab_path`, so no active CSV column deletion was required for this follow-up.

### History

- 2026-05-19: User said the current projectile blueprint relies too much on other CSV/C# sources and requested a redesign so Builder can implement projectile skills by reading the blueprint alone.
- 2026-05-19: User then clarified that the blueprint should be understandable to AI, should favor parsed-data-to-common-runtime flow, and should stop and ask when a projectile requires special behavior such as timed firing, homing, or last-shot explosion.
- 2026-05-19: User requested shrinking the optional parsed-field list further and asked to remove unsupported field expectations while keeping prefab path support.

## Source: boards\OPS\CODEX_CLI_BLACKBOARD.md

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

## Legacy Non-English Section

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Task title
- Goals
- Constraints
- Role Owner
- Status
- Next Actions
- Evidence
- History

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

## Source: boards\OPS\REVIEWER_BLACKBOARD.md

## Task: 2026-05-14 InGame Phase2-A Base Unit Model Reviewer

### Task title

Run Code Reviewer after the Phase2-A base unit runtime model split.

### Goals

- Execute one Code Reviewer pass after Code Builder added `BaseUnitRuntimeModel`, `MonsterUnitRuntimeModel`, and `EnemyUnitRuntimeModel`.
- Review the uncommitted changed set for compile risks, missing helpers, null risks, and side effects.
- Record whether the Reviewer returned a pass decision or a fix request.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer must not edit files.
- Do not run Unity Play Mode; user owns gameplay verification.

### Role Owner

Code Reviewer

### Status

Reviewer executed and returned a fix request, not a pass decision.

### Next Actions

- Code Builder should not run another Reviewer pass for this task unless the user explicitly asks.
- The reported issue is in the existing uncommitted Phase1-D skill validator area, not in the Phase2-A unit model split files.
- Decide separately whether to fix `InGameSkillDataValidator` so Eve-E `MagazineProjectile` without `ShotIntervalSeconds` is accepted or remapped.

### Evidence

- Initial Reviewer command using `openai.chatgpt-26.417.40842-win32-x64\bin\windows-x86_64\codex.exe review --uncommitted` failed because that executable path no longer exists.
- `Get-ChildItem C:\Users\t3312\.vscode\extensions -Directory -Filter 'openai.chatgpt-*'` found `openai.chatgpt-26.506.31421-win32-x64`.
- Reviewer command using the current executable first failed with socket/network error `os error 10013`.
- Escalated Reviewer command using `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.506.31421-win32-x64\bin\windows-x86_64\codex.exe review --uncommitted` completed and reported `[P2] Do not require shot intervals for every magazine skill`.
- Reviewer cited `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs:363-365` and said the current catalog has Eve-E as `MagazineProjectile` with `ShotIntervalSeconds: 0`, while existing runtime `TryCastEveDroneBeacon()` uses `EveDroneAttackPeriod`.

### History

- 2026-05-14: Code Builder attempted the required Reviewer transition after the Phase2-A model split; Reviewer did not pass because it found a validator issue in the broader uncommitted set.

## Task: 2026-05-14 InGame Rename Reviewer Attempt

### Task title

Run Code Reviewer after the CombatV2-to-InGame rename.

### Goals

- Execute one Code Reviewer pass after Code Builder renamed the `Assets/Scripts2` runtime tree to `InGame`.
- Review script/class/path rename consistency, `.csproj` references, HTML report updates, and board updates.

### Constraints

- Role Owner is Code Reviewer.
- Reviewer must not edit files.
- Do not run Unity Play Mode; user owns gameplay verification.

### Role Owner

Code Reviewer

### Status

Reviewer execution attempted but did not complete.

### Next Actions

- Re-run Reviewer later when Codex CLI network/socket access is stable, or provide an approved external wrapper run that can wait longer than 180 seconds.

### Evidence

- First Reviewer command used the old extension path `openai.chatgpt-26.417.40842-win32-x64` and failed because `codex.exe` was not found.
- `Get-ChildItem C:\Users\t3312\.vscode\extensions -Directory -Filter 'openai.chatgpt-*'` found `openai.chatgpt-26.506.31421-win32-x64`.
- Reviewer command using `openai.chatgpt-26.506.31421-win32-x64\bin\windows-x86_64\codex.exe review --uncommitted` started but failed with socket/network error `os error 10013`.
- Escalated Reviewer command with the same executable timed out after 180 seconds before returning a review result.

### History

- 2026-05-14: Code Builder attempted the required Reviewer transition after the InGame rename, but no `PASS` or `FAIL` review result was produced.

## Source: boards\RUN\REWARD_BLACKBOARD.md

## Task: 2026-05-20 Reward Button Grid Inspector Controls

### Task title

Switch RewardPanel button placement from inferred spacing to inspector-driven grid layout.

### Goals

- Keep the first reward column starting at the inspected `PrisonerBtn` baseline.
- Use `122` Y spacing per row and move the fourth reward into the next column by X offset instead of pushing all rewards down one column.
- Let the scene owner tune reward button start position and spacing directly from the controlling UI script inspector.

### Constraints

- Role Owner is Code Builder.
- Reward layout conclusions must stay tied to inspected `InGameUIManager.cs` and inspected `NewRunScene.unity` `RewardBtnContainer` button RectTransforms.
- Reward claim, Offering, and Manifest button behavior must remain unchanged.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Reward buttons now place by column/row grid math using serialized inspector fields instead of template delta inference.

### Next Actions

- User verifies in Play Mode that 1-3 rewards stay in the left column, the 4th reward starts the next column, and inspector edits move the clones without changing template buttons.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now serializes `rewardButtonFirstColumnPosition`, `rewardButtonColumnSpacingX`, `rewardButtonRowSpacingY`, and `rewardButtonRowsPerColumn`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` `ArrangeRewardButton()` now keeps template anchors/size but places clones by `column = order / rowsPerColumn` and `row = order % rowsPerColumn`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` no longer uses `ResolveRewardButtonSpacing(...)`, so reward spacing is no longer inferred from template button deltas.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes `PrisonerBtn` at `(-321.97855, 295)`, `DarkBtn` at `(-321.98, 122)`, and `GoldBtn` at `(-321.98, -53)`, which is the inspected source for the default first-column anchor and `122` row spacing.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-20: User requested the RewardPanel cloned buttons use fixed row/column placement and that X/Y layout values be adjustable from the controlling script inspector.

## Task: 2026-05-16 Stage Reward CSV Seed

### Task title

Create active Stage reward rule CSV rows for NewRunScene Stage Flow.

### Goals

- Store Stage 1 reward payout rules outside code.
- Include prisoner count probabilities, gold, dark trace, elite bonus prisoner count, and artifact choice count.
- Keep artifact UI implementation deferred while preserving data columns for later use.

### Constraints

- Role Owner is Code Builder.
- CSV data only; no reward UI or prisoner flow code was changed.
- Event, shop, and artifact UI behavior remain unimplemented in this slice.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV consistency verified.

### Next Actions

- Future Stage Flow implementation should read `StageReward.csv` and apply rewards to `RunSession`.
- User-authored reward UI should display the parsed prisoner/gold/dark trace result after enemy clear.

### Evidence

- Added `Pakuri/Assets/CSVdata/StageReward.csv`.
- `StageReward.csv` has rows for `reward-stage1-normal`, `reward-stage1-elite`, `reward-stage1-midboss`, `reward-stage1-day10-midboss`, and `reward-stage1-boss`.
- `StageReward.csv` stores prisoner count odds `0.05`, `0.80`, and `0.15`, matching `Pakuri/reference/4.run/combat-reward-system.md`.
- `StageReward.csv` stores Stage 1 gold/dark trace values normal `10/10`, midboss `30/20`, and boss `50/50`.
- `Import-Csv` and cross-file consistency checks reported no missing reward references from `StageDay.csv`.
- `NewRunStageManager` reads `StageReward.csv` and exposes pending gold, dark trace, prisoner count, and prisoner IDs when the flow reaches `RewardReady`.
- Reward UI, Manifest UI, Offering UI, and artifact reward UI were not implemented in this slice.

### History

- 2026-05-16: User requested active CSV files including reward rules for the next StageManager implementation.
- 2026-05-16: Code Builder added StageManager reward-ready state and pending reward properties for future UI wiring.

## Task: 2026-05-17 Eve Offering Skill Choice Reward Mapping

### Task title

Map Eve skill choice IDs into NewRunScene Offering rewards.

### Goals

- Let Offering rewards store the same Eve choice IDs used by `SkillChoiceModifierData.csv`.
- Avoid showing B-E active enhancements before the corresponding active skill is learned.
- Avoid showing F-J passive enhancements before the corresponding passive is learned.
- Prevent selected modifiers for one skill from mutating another skill's execution snapshot.

### Constraints

- Role Owner is Code Builder.
- NewRunScene UI layout was not redesigned.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Offering order and random choice feel in Play Mode.
- Later passive runtime work should consume the `DataOnlyUnsupported` passive trait rows.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_reward_choices.csv` now contains 50 Eve reward rows matching the 50 Eve choice/modifier IDs.
- `InGameUIManager.cs` filters skill-choice reward IDs by learned active/passive ownership before adding enhancement choices to the Offering popup.
- `SkillChoiceResolver.cs` filters modifier records against the current skill's `EnhancementChoices` and `MasterChoices`, preventing cross-skill modifier leakage.
- CSV consistency check returned no missing reward, source choice, or modifier links for Eve choice IDs.
- Runtime/editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-17: User said data cleanup and Offering mapping should come first before Eve full skill implementation, then asked Code Builder to perform that work.

## Task: 2026-05-08 Manifested Runtime Resume Reward Context

### Task title

Record that this Rin-first runtime pass did not change reward selection code.

### Goals

- Keep reward board aligned because manifested monsters are acquired through prisoner Manifest flow.
- Record that this pass changed combat/runtime status binding, not reward candidate or Offering selection code.

### Constraints

- Role Owner is Code Builder.
- No Play Mode verification was run by Codex.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Context recorded; no reward code changes were made in this pass.

### Next Actions

- User verifies Manifest acquisition plus manifested Rin skill behavior in Play Mode.
- If reward candidate or Offering choice behavior is wrong, inspect `RunCombatUiController` and `RunSession` in a separate focused pass.

### Evidence

- `git status --short` showed modified reward/run files already existed in the worktree, but this pass changed combat runtime files and board records only.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:195` and `:1870` contain the manifested slot status-view reuse work.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:128` contains the Rin unit-runtime dispatch work.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User resumed a manifested monster runtime task whose acquisition context is RunScene Manifest flow, but the requested fix was combat/runtime parity and status UI reuse.

## Task: 2026-05-08 Manifest UI Wiring Status Report

### Task title

Record current prisoner Manifest reward UI wiring inspection.

### Goals

- Keep reward/prisoner modal state aligned with the current scene inspection.
- Record that `ManifestButton` owns the Manifest roll and failure popup path in current code.
- Record the UI hierarchy warning found in `PrisonerOfferingPanel`.

### Constraints

- Role Owner is Designer for documentation.
- No gameplay code or scene edits were made.
- User performs Play Mode verification.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- User verifies reward -> prisoner choice -> Manifest success/failure flow in Play Mode.
- If UI is visually wrong, first inspect or clean the nested `PrisonerOfferingPanel/DefeatPanel` and duplicate `Title`.

### Evidence

- `RunCombatUiController.cs:367` binds `ManifestButton` to `TryManifestPrisonerMonster`.
- `RunCombatUiController.cs:391` binds `SummonButton` to result close rather than the Manifest roll.
- `RunCombatUiController.cs:396` through `:400` creates/binds `PrisonerManifestFailurePopup`.
- `RunCombatUiController.cs:1711` through `:1717` includes the failure popup and prisoner panels in `IsRewardModalOpen()`.
- Unity-MCP scene inspection found all required prisoner reward panels and buttons present with button components and label text.
- Unity-MCP scene inspection found `PrisonerOfferingPanel` has an unexpected child `DefeatPanel` and duplicate `Title`.
- Report saved as `Pakuri/reference/Report/2026-05-08-runscene-manifest-ui-and-runtime-status.html`.

### History

- 2026-05-08: User requested current panel/button connection inspection and an HTML summary of current Manifest runtime structure.

## Migrated Task Blocks

## Task: 2026-05-08 Manifest Candidate Selected Monster Fix

### Task title

Allow the MainMenu-selected monster to appear as a Manifest candidate.

### Goals

- Fix the Manifest candidate filter that hid Eve when Eve was the selected MainMenu monster.
- Allow selecting Eve through Manifest to record Eve in `RunSession.ManifestedMonsterIds`.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies in RunScene Play Mode that Eve appears as a Manifest candidate and is added after a successful Manifest choice.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` filters Manifest candidates through `currentSession.HasManifestedMonster(monster.MonsterId)`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` now makes `HasManifestedMonster(...)` check only `ManifestedMonsterIds`, not `SelectedMonsterId`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` keeps `RecordManifestedMonster(...)` guarded by `HasManifestedMonster(...)`, so Eve can now be recorded when it is not already in `ManifestedMonsterIds`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings; Unity refresh returned idle and console error query returned only MCP client handler exit logs.

### History

- 2026-05-08: User reported Eve did not appear in Manifest candidates and selecting Eve did not add Eve.

## Task: 2026-05-08 Eve Unit Offering Runtime Choices

### Task title

Use unit-owned Offering choices in shared Eve skill execution.

### Goals

- Record that Eve B-E shared unit execution reads per-unit Offering choices.
- Keep selected and manifested Eve skill enhancements aligned with each unit's `RunMonsterState`.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies Offering-enhanced Eve B-E on selected and manifested Eve in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` uses `HasManifestedChoice(runtime, ...)` in shared Eve B-E caster methods.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` configures selected and manifested unit runtimes with `RunSession.EnsurePartyMemberState(...)` / per-member `RunMonsterState`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested object-oriented skill ownership so Offering-enhanced skills behave the same for selected and manifested units.

## Task: 2026-05-08 Manifested Offering Skill State

### Task title

Use per-monster Offering state when manifested Eve casts Frost Field.

### Goals

- Ensure Offering choices recorded on a manifested Eve affect the manifested Eve skill runtime.
- Record that Eve C trait choices are read from the manifested unit's `RunMonsterState.ChosenRewardIds`.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated for manifested Eve C.

### Next Actions

- User verifies Offering-acquired Frost Field on manifested Eve in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `RunSession.RecordOfferingChoice(string monsterId, ...)` already records choices into per-member `RunMonsterState.ChosenRewardIds` for non-selected party members.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now reads manifested Eve C trait checks from `runtime.State.ChosenRewardIds` via `HasManifestedChoice(...)`.
- The manifested Eve C persistent effect now applies trait 1 radius/duration, trait 2 tick/chill stacks, trait 3 damage/cooldown, trait 4 radius/damage, and trait 5 damage/freeze duration.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User clarified that Offering-enhanced skills on manifested units should behave like the same skill on the MainMenu-selected unit.

## Task: 2026-05-16 NewRunScene Reward Buttons And Prisoner Flow

### Task title

Drive NewRunScene rewards through RewardPanel buttons.

### Goals

- Clone reward buttons from `RewardBtnContainer` according to pending gold, dark trace, and prisoner rewards.
- Apply gold/dark trace only when their buttons are clicked.
- Use prisoner buttons for Offering or Manifest, then disable the consumed prisoner button.
- Store Manifest success chance in active reward CSV data.

### Constraints

- Role Owner is Code Builder.
- User-authored UI hierarchy is used as-is: `RewardPanel`, `PrisonerChoicePopUp`, `OfferingPanel`, `MenifestedFailPopUp`, and `MenifestedSuccessPopUp`.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies reward button count/spacing, resource counts, prisoner button disabled state, Offering choice application, and Manifest success/failure popups in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` creates reward buttons from `PrisonerBtn`, `GoldBtn`, and `DarkBtn`, updates `Goldinfo`, `Darkinfo`, and `StageInfo`, and binds Offering/Manifest popup buttons.
- `Pakuri/Assets/CSVdata/StageReward.csv` now has `manifest_success_chance` with `0.70` in all active reward rows.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStageManager.cs` exposes `PendingManifestSuccessChance`, `PendingGoldReward`, `PendingDarkTraceReward`, and `PendingPrisonerEnemyIds` for the UI.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.
- Reviewer reported duplicate prisoner sampling from one encounter row; Builder fixed candidate selection to sample without exceeding spawned row `Count`.

### History

- 2026-05-16: User specified the reward UI click flow and 70/30 Manifest probability from the prisoner-choice reference.
- 2026-05-16: Builder implemented the reward UI controller and CSV-backed Manifest probability.

## Source: boards\RUN\RUN_BLACKBOARD.md

## Task: 2026-05-22 Unit-Rule Combat Execution And Auto Routing

### Task title

Run NewRunScene combat behavior by unit rules instead of `StageState.Combat`.

### Goals

- Let spawned enemies move and attack as soon as their target/range/cooldown rules allow it.
- Keep player learned active skill cooldowns advancing independent of the Stage flow state's `Combat` label.
- Keep selected 1P Auto off at start and when toggled off.
- Allow automatic player skill routing only when a living enemy exists inside `MainCamera` view.
- Pass the clicked world point into manual skill execution so click-targeted area and SingleAttack skills use the clicked location.
- Start monster skill cooldowns when a valid cast is committed even if the hit check finds no target.

### Constraints

- Role Owner is Code Builder.
- This task changes runtime execution policy only; no CSV, scene, reward, or run-session persistence fields were changed.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that enemies begin moving immediately after spawning, selected 1P Auto toggles on/off, Auto skills only fire with a visible MainCamera enemy, and clicked manual skills consume cooldown on valid casts.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs:172-179` remains the state authority for `Spawning`, `Combat`, and `RewardReady`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` no longer references `StageState.Combat`, `IsCombatStageActive()`, or a serialized `StageManager` gate for runtime behavior.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now ticks learned passives, player skill execution/manual input, and `enemyCombatSystem.Tick(...)` without checking the Stage flow state.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` still rejects automatic player skill routing without a visible living enemy in `MainCamera` or while the selected 1P Auto state is off.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionContext.cs:13-35` and `SkillExecutionSystem.cs:49-62` now carry a manual target point for clicked-position skills.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now treats committed SingleAttack, direct Beam, manual fallback Projectile, and valid Buff target attempts as routed for cooldown purposes even when a hit/status application does not occur.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP script refresh reached idle; console warning/error read showed only MCP client handler logs.

### History

- 2026-05-22: User reported DebugUI-learned Ariel skills were persisted into later rounds and could repeatedly execute outside intended combat conditions. Code Builder added the combat-state and visible-enemy gates plus clicked-position manual execution.
- 2026-05-22: User clarified that `Combat` should not gate enemy or monster behavior. Code Builder removed the Stage combat-state dependency from runtime combat behavior and kept Auto routing constrained by visible MainCamera enemies.

## Task: 2026-05-22 Offering Learned Passive Runtime Effects

### Task title

Refresh learned passive skill effects from runtime monster state during combat.

### Goals

- Use the existing Offering/run-session learned passive state as the authority for passive effect activation.
- Apply passive effect rows to current roster entries without adding a parallel run-state store.
- Keep one-shot passive effects from repeating every refresh tick.

### Constraints

- Role Owner is Code Builder.
- This task does not change Offering selection UI; it consumes the already-recorded `LearnedPassiveSkillIds`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that acquiring Ariel F-J through Offering changes the live combat state after runtime model refresh.
- If Offering can add passives during the same active combat and needs G's one-shot shield only at the next combat start, add a battle-start event boundary before applying one-shot passive effects.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` already copies `RunSession.RunMonsterState.LearnedPassives` into `MonsterUnitRuntimeModel.State.LearnedPassiveSkillIds`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` already records Offering passive choices before this task; this task consumes the model state rather than changing that storage.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGamePassiveEffectRuntime.cs` iterates `roster.Players`, reads each monster model's `LearnedPassiveSkillIds`, resolves `PassiveDefinition`, and executes `PassiveEffects`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` calls passive refresh from `Update()` and stores `appliedOneShotPassiveEffects` to prevent `apply_once` effects from repeating.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` marks `ariel-g-start-shield` with `apply_once=true`; other passive aura rows use short `0.5` second durations and refresh every `0.25` seconds.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User asked whether Ariel F-J passives with existing values could be implemented; Code Builder connected the already-recorded learned passive state to the shared effect runtime.

## Task: 2026-05-20 DebugModifiedUI Active Choice Commit Path

### Task title

Route debug active enhancement picks through the same run-session choice state used by Offering.

### Goals

- Let debug UI apply active `Trait` and `Master` choices without inventing a separate debug-only choice state.
- Keep runtime chosen choice IDs and numeric reward modifiers flowing through the existing `RunSession` and runtime-model refresh path.
- Keep active skill enhancement availability aligned with the current Offering limits per skill.

### Constraints

- Role Owner is Code Builder.
- Implementation scope is limited to `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs`; no `RunSession`, `InGameUIManager`, or scene serialization code was changed in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Debug active enhancement picks now record exact choice IDs into run state and immediately rebuild player runtime skills.

### Next Actions

- User verifies in Play Mode that debug-applied trait/master picks persist in the current run session and immediately affect the selected monster.
- If debug UI later needs passive enhancement support, extend the same run-state path rather than creating a second modifier storage path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` already stores enhancement ownership through `ChosenRewardIds`, `ChosenChoiceIds`, and `AccumulateReward(...)`, which remains the authority used by this task.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs:616-645` already commits Offering enhancement choices through `RecordOfferingChoice(...)`, `AccumulateReward(...)`, and runtime refresh; `DebugUI.cs` now mirrors that path for debug active modifiers.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now applies clicked modifier choices through `RunSession.RecordOfferingChoice(monster.MonsterId, choice.ChoiceId, choice.ChoiceId, sourceSkill.SkillId, string.Empty)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now applies choice numeric effects through `RunSession.AccumulateReward(...)`, then copies chosen choice IDs back into `MonsterUnitRuntimeModel.State` via `RefreshRuntimeSkillModels(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now enforces current active choice gating by `SkillChoiceGroup`: up to three `ActiveEnhancement` choices and then up to one `ActiveMaster`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings after rerun outside the transient file-lock case.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-20: User requested Code Builder implementation so `DebugModifiedUI` could apply active trait/master choices like Offering without going through prisoner reward flow.

## Task: 2026-05-20 MonsterPanel Player Slot Projection

### Task title

Project current player roster slot ownership into `MonsterPanel` `1PMonster`-`5PMonster` UI.

### Goals

- Keep `NewRunScene` player runtime ownership authoritative through `UnitIdentity.SlotIndex`.
- Make manifested player monsters appear in the same slot index on the `MonsterPanel` UI path.
- Avoid scene serialization edits when the authored `MonsterPanel` child slot objects already exist.

### Constraints

- Role Owner is Code Builder.
- The implementation scope is limited to `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs`; no spawn logic or roster ownership code was changed.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. The active run UI now maps player roster models into panel slots `0`-`4` before rendering `1PMonster`-`5PMonster`.

### Next Actions

- User verifies in Play Mode that each manifested monster shows in the `MonsterPanel` slot matching its runtime `SlotIndex`.
- If a manifested monster still fails to appear, inspect the spawn/roster path to confirm that `combatManager.Roster.Players` actually contains a player model for that slot.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs:338-352` restores manifested players into slot indices `1`-`4`, and `SceneEntryManager.cs:372-384` already resolves active player entries by `identity.SlotIndex`.
- `Pakuri/Assets/Scripts2/InGame/Units\BaseUnitRuntimeModel.cs:19-26` defines `UnitIdentity.SlotIndex`, which is the runtime authority used by this UI task.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:44-82` now projects `combatManager.Roster.Players` into a five-slot model array keyed by `identity.SlotIndex`, with slot `0` falling back to `entryManager.SpawnedPlayerModel` only when needed.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:128-166` now binds `MonsterPanel` children named `1PMonster` through `5PMonster` instead of binding only `1PMonster`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains the authored slot roots `1PMonster`, `2PMonster`, `3PMonster`, `4PMonster`, and `5PMonster`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-20: User requested Code Builder implementation after code inspection showed the prior `MonsterPanelUI` path was hard-wired to `Players[0]` and `MonsterPanel/1PMonster`.

## Task: 2026-05-20 Manifest Popup Text Fix And Spawn-Point State Check

### Task title

Fix Menifest success popup label mojibake and verify current spawn-point active state without changing Manifest spawn logic.

### Goals

- Remove the broken hardcoded labels shown in the Menifest success popup description.
- Keep current Manifest runtime flow ownership unchanged.
- Verify whether `1PSpawnPoint` through `5PSpawnPoint` were already active in the current repository state before making a scene/runtime change request.

### Constraints

- Role Owner is Code Builder.
- The user explicitly scoped this task to popup text plus spawn-point active-state handling; do not modify Manifest slot calculation or spawn code in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Popup text implemented. Current repository evidence shows the authored `NewRunScene` spawn-point objects are already active, so no run-flow code or scene-serialization change was applied for spawn placement in this task.

### Next Actions

- User verifies in Play Mode whether the popup labels render correctly and whether Manifest spawn behavior still reproduces.
- If spawn placement still lands on the wrong slot, inspect runtime scene state and spawn ownership separately from this completed text fix.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs:1349-1350` now uses readable labels for the Menifest success popup description while preserving the existing runtime-fed monster values.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` inspection showed `1PSpawnPoint`, `2PSpawnPoint`, `3PSpawnPoint`, `4PSpawnPoint`, and `5PSpawnPoint` with `m_IsActive: 1`.
- Unity MCP `editor_state` showed the editor was ready, `manage_scene load` successfully opened `Assets/Scenes/NewScene/NewRunScene.unity`, and Unity MCP scene inspection returned `3PSpawnPoint active=true activeInHierarchy=true`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings after a first parallel build attempt hit only a transient file-lock error.

### History

- 2026-05-20: User requested a limited Code Builder pass that fixed popup text and handled spawn-point active state without expanding into a broader Manifest spawn-logic rewrite.

## Task: 2026-05-20 Day Advance Heal And Reward Grid Layout

### Task title

Restore the full player party on day advance and move reward button placement to inspector-driven grid settings.

### Goals

- Restore current player-side party members for the next day, including allies that died and were removed during the previous combat.
- Heal restored player-side roster units to max HP before the next day starts.
- Replace template-inferred reward button spacing with explicit grid placement rules that match the current RewardPanel layout.
- Expose reward button X/Y placement values on `InGameUIManager` so the scene owner can tune them in the inspector.

### Constraints

- Role Owner is Code Builder.
- Runtime conclusions must stay tied to inspected `StageManager`, `InGameCombatManager`, `UnitRosterService`, and `NewRunScene` serialization.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. `ContinueToNextDay()` now restores missing party units from `RunSession` and then fills current player HP before the next day flow restarts, and reward buttons now use inspector-driven grid coordinates instead of inferred single-column spacing.

### Next Actions

- User verifies in Play Mode that stage/day transition respawns dead allies into their original party slots and that all returned allies begin the next combat at full HP.
- User adjusts `InGameUIManager` inspector values if RewardPanel column count or spacing needs further tuning beyond the current `3 rows per column` grid.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` `RemoveUnitIfDead(...)` unregisters dead units from `roster` and destroys their actor object, which is why full next-day return required more than a health refill.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now exposes `restorePlayerHealthOnDayAdvance` and calls `RestorePlayerHealthForNextDay()` immediately after `activeSession.AdvanceDay()` and before `StartCurrentDay()`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now calls `entryManager.RestorePlayerPartyFromSession()` before filling each `combatManager.Roster.Players` entry to `model.Stats.MaxHealth` and refreshing the actor.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now restores the selected slot through `RestoreSelectedPlayerFromSession()` and manifested slots through `RestoreManifestedPlayersFromSession()`, using `RunSession.ActiveSession` plus current roster slot checks.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` now adds `RespawnSelectedPlayerUnit(RunSession activeSession, ...)`, which recreates the selected monster runtime from existing session party state instead of calling `RunSession.Begin(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `Roster` and `RefreshUnitActor(BaseUnitRuntimeModel model)`, which is the current runtime/UI refresh path used by the heal step.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` keeps selected and manifested party ownership in `SelectedMonsterId`, `ManifestedMonsterIds`, and `PartyMembers`, which is the source used for day-advance respawn.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now exposes `rewardButtonFirstColumnPosition`, `rewardButtonColumnSpacingX`, `rewardButtonRowSpacingY`, and `rewardButtonRowsPerColumn`, and `ArrangeRewardButton()` now computes `column` / `row` from `order`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes `PrisonerBtn` at `m_AnchoredPosition {x: -321.97855, y: 295}` and the same container holds `DarkBtn` at `y: 122` and `GoldBtn` at `y: -53`, which matches the default `122` row spacing carried into the new inspector-backed layout.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-20: User requested Code Builder implementation for day-advance HP restoration and RewardPanel button placement, with X/Y layout values adjustable in the inspector.

## Task: 2026-05-19 Selected 1P A-Skill Entry Policy And Sein Visual Wiring

### Task title

Keep the selected 1P primary-skill entry policy explicit while restoring the missing Sein projectile visual mapping.

### Goals

- Record that the selected 1P slot `A` does not auto-route on scene entry unless `playerAutoSkillEnabled` is enabled.
- Restore the missing `sein-a` visual prefab mapping in `NewRunScene` so manual fire or `AutoBtn` uses the expected projectile visual.

### Constraints

- Role Owner is Code Builder.
- Runtime conclusions must stay tied to the inspected Scripts2 combat manager and actual `NewRunScene` serialization.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Scene visual wiring was restored. The run-flow policy remains unchanged: selected 1P slot `A` starts in manual fire mode until `AutoBtn` enables auto fire.

### Next Actions

- If the user wants selected 1P `A` to auto-fire immediately on scene entry, treat that as a separate global run/combat policy change and update this board together with the relevant combat/UI boards.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` uses `ShouldAutoRouteSkill(...)` to suppress automatic routing for the selected player slot `A` unless `playerAutoSkillEnabled` is true.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` routes selected player slot `A` through `HandleSelectedPlayerPrimarySkillInput()` only while the primary mouse button is held and the pointer is not over UI.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10373` serializes `playerAutoSkillEnabled: 0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs` and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:14188-14189` show `AutoBtn` enables `InGameCombatManager.EnablePlayerAutoSkillMode()`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10471` now serializes the `sein-a` prefab entry under the `EffectManager` `sein` group.

### History

- 2026-05-19: User reported that Sein appeared not to attack in-game and also noted the missing `EffectManager` Sein prefab assignment.
- 2026-05-19: Code Builder confirmed the selected 1P `A` manual-fire entry policy, restored the missing `sein-a` visual prefab mapping, and left the global auto-fire policy unchanged.

## Task: 2026-05-19 Offering Choice Runtime Path Unification

### Task title

Use exact choice IDs for Offering gating and resolve runtime modifiers from unified choice definitions.

### Goals

- Keep Offering enhancement picks keyed by exact `choice_id`.
- Remove the old separate `SkillChoiceModifierData.csv` combat path.
- Let passive-linked choice rows target active skills through merged choice metadata.

### Constraints

- Role Owner is Code Builder.
- Runtime conclusions must stay tied to inspected Scripts2 code and verified builds.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Offering and combat runtime now resolve monster choice effects from one unified `SkillChoiceDefinition` path.

### Next Actions

- If later combat work adds new special-case choice behaviors, extend `SkillExecutionSnapshot` / executors from the rows already marked unsupported or partial in `monster_skill_choices.csv`.
- Keep this file aligned with `boards/DATA/DATA_BLACKBOARD.md` whenever choice/runtime ownership changes again.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now commits enhancement Offering picks with `choice.ChoiceId`, which is the exact row ID from `monster_modifier_skill_choice.csv` / `monster_skill_choices.csv`.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` still persists `ChosenRewardIds` and `ChosenChoiceIds` separately, so the gate row ID and runtime choice ID remain explicit even though enhancement picks now use the same exact `choice_id`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs` no longer uses `SkillChoiceModifierLibrary`; it resolves each chosen choice globally through `PakuriDataManager.TryGetData(choiceId, out SkillChoiceDefinition choice)` and applies it when `choice.TargetSkillId` or `choice.SkillId` matches the executing skill.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSnapshot.cs` now applies merged choice fields such as `CooldownMultiplier`, `RadiusMultiplier`, `DurationMultiplier`, `AdditionalProjectileBonus`, `PierceBonus`, branch fields, and status stack fields directly from `SkillChoiceDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now consumes snapshot radius/duration modifiers for beam skills, which is why rechecked Eve beam rows could be upgraded from unsupported to direct support.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` no longer exposes `skillChoiceModifierCsv`, and `Pakuri/Assets/CSVdata/SkillChoiceModifierData.csv` was deleted in this task.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing MSB3277 warnings remained.

### History

- 2026-05-19: Code Builder removed the separate choice-modifier CSV runtime path, switched combat resolution to unified `SkillChoiceDefinition` data, and kept exact `choice_id` persistence through Offering commit and runtime execution.

## Task: 2026-05-19 Remove Unused InGame Test Data Bootstrap

### Task title

Remove the unused `InGameTestDataManager` test bootstrap from the active `InGame` runtime.

### Goals

- Delete the unused test-only `InGameTestDataManager.cs` script and its `.meta`.
- Remove the explicit `Assembly-CSharp.csproj` compile entry for the deleted script.
- Keep the active `NewRunScene` runtime authority summary aligned with the actual surviving files.

### Constraints

- Role Owner is Code Builder.
- The deletion must stay evidence-based: only remove the script after confirming there is no active scene/prefab/asset reference in the inspected repository.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally verified by file inspection and build.

### Next Actions

- User verifies in Unity only if they want editor-side confirmation that no scene object was intentionally meant to use this removed test bootstrap.

### Evidence

- `Get-ChildItem Pakuri/Assets -Recurse -Include *.unity,*.prefab,*.asset | Select-String -Pattern 'b80e67b6202c23b46bf0867afa0f8b4e|InGameTestDataManager'` returned no active asset reference to the script class or GUID before deletion.
- `Pakuri/Assembly-CSharp.csproj` explicitly included `Assets\Scripts2\InGame\Core\InGameTestDataManager.cs` before this task, so the compile item had to be removed together with the file.
- Deleted `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs` and `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs.meta`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs`, `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs`, and `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` remain the inspected active runtime entry/spawn/UI owners for the surviving `NewRunScene` flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-19: User asked to delete `Pakuri/Assets/Scripts2/InGame/Core/InGameTestDataManager.cs` after reviewing its role as an unused test bootstrap rather than an active runtime manager.

## Task: 2026-05-18 Remove NewRun Prefix From Runtime Script Names

### Task title

Remove the `NewRun` prefix from current run-flow script filenames and matching type names.

### Goals

- Rename `NewRunSceneEntryManager.cs`, `NewRunStageManager.cs`, and `NewRunStartContext.cs` by removing the `NewRun` prefix.
- Keep Unity component compatibility by moving each `.meta` file with its script.
- Update C# references, scene class identifiers, and project compile paths.

### Constraints

- Role Owner is Code Builder.
- Behavior must remain unchanged; this is a naming refactor only.
- Unity Play Mode verification remains user-owned.
- Existing scene name/path strings such as `NewRunScene.unity` were not renamed because the request was limited to script filenames.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want runtime scene behavior confirmation after the script rename.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/NewRunSceneEntryManager.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` and `.meta`; GUID `b6ff00e786df7fb46ae905aa63bee059` was preserved.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStageManager.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` and `.meta`; GUID `7c2fbcf1f36342aca23eac2221b2c1e8` was preserved.
- `Pakuri/Assets/Scripts2/InGame/Core/NewRunStartContext.cs` and `.meta` were moved to `Pakuri/Assets/Scripts2/InGame/Core/StartContext.cs` and `.meta`; GUID `11eb246df33aa9b4388af02ec8175fd4` was preserved.
- `SceneEntryManager.cs`, `StageManager.cs`, and `StartContext.cs` now declare `SceneEntryManager`, `StageManager`, and `StartContext`; `NewRunStageState` was renamed to `StageState`.
- `DebugUI.cs`, `InGameUIManager.cs`, `MonsterPanelUI.cs`, and `UIManager.cs` now reference the renamed runtime types; the Menifest flow now lives inside `InGameUIManager.cs`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now records `Pakuri.InGame.SceneEntryManager` and `Pakuri.InGame.StageManager`.
- `Pakuri/Assembly-CSharp.csproj` now compiles `SceneEntryManager.cs`, `StageManager.cs`, and `StartContext.cs`.
- `Get-ChildItem -Path Pakuri\Assets -Recurse -File -Filter 'NewRun*.cs'` returned no files after the rename.
- Search found no remaining `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunStartContext`, or `NewRunStageState` references in scripts, scene assets, prefab assets, asset files, or `Assembly-CSharp.csproj`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP refresh reached idle; console warning/error read showed MCP client handler logs and the existing UnityEditor.Graphs `NullReferenceException`, not a C# compile error.

### History

- 2026-05-18: User asked to remove `NewRun` from all filenames that currently start with `NewRun`; Code Builder renamed the three inspected scripts and their matching C# types.

## Task: 2026-05-18 Enemy Spawn Manager Rename

### Task title

Keep scene entry flow wired after renaming the spawn manager to `EnemySpawnManger`.

### Goals

- Preserve `SceneEntryManager` spawning and manifest entry points after the script rename.
- Preserve the existing scene MonoBehaviour reference by retaining the script GUID.
- Keep the current new-scene runtime authority behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- Requested source file `NewRunStageSpawnManager.cs` did not exist; the inspected scene and scripts used the former `NewRunUnitSpawnManager.cs`, now `EnemySpawnManger.cs`.
- This task changed naming and references only, not spawn behavior.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode if runtime spawn behavior needs visual confirmation after the rename.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` uses `[RequireComponent(typeof(EnemySpawnManger))]`, a serialized `EnemySpawnManger unitSpawnManager`, and `GetComponent<EnemySpawnManger>()` / `AddComponent<EnemySpawnManger>()`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` preserves the previous spawn APIs, including selected player unit, manifested monster, and enemy spawn methods.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` continues to reference script GUID `fa013f8b8851bec4882efe505f98b801` and now records `Pakuri.InGame.EnemySpawnManger`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and a standalone `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP refresh reached idle; after clearing stale console output, no C# compile error remained.

### History

- 2026-05-18: Code Builder renamed the actual scene spawn manager file/class from `NewRunUnitSpawnManager` to `EnemySpawnManger` because the user-requested `NewRunStageSpawnManager.cs` was not present in the repository.

## Source: boards\UI\RUNSCENE_UI.md

## Task: 2026-05-22 AutoBtn Toggle And Manual Learned Skill Input

### Task title

Make `AutoBtn` toggle selected 1P auto-skill mode and require click input for selected 1P learned active skills while Auto is off.

### Goals

- Change `AutoBtn` from one-way enable to selected 1P Auto on/off toggle.
- Keep selected 1P learned A-E active skills out of automatic routing while Auto is off.
- Use one mouse click as the manual execution command for currently ready selected 1P learned active skills.
- Preserve UI pointer blocking so clicking UI does not fire skills.

### Constraints

- Role Owner is Code Builder.
- UGUI `Canvas/AutoBtn` keeps using `InGameAutoSkillButton`; no scene serialization edit was made.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that first `AutoBtn` click enables selected 1P Auto, second click disables it, and Auto-off selected 1P skills fire only from a gameplay-area mouse click.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs:20-28` now binds `AutoBtn` to `ToggleSelectedPlayerAutoSkillMode()` instead of one-way enable.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:309-321` now toggles and writes selected 1P `AutoSkillEnabled`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:363-405` now handles selected 1P manual skill execution on `leftButton.wasPressedThisFrame` and still exits when the pointer is over UI.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP script refresh reached idle; warning/error console read showed only MCP client handler logs.

### History

- 2026-05-22: User clarified `AutoBtn` must be a toggle: first click Auto on, second click Auto off. Code Builder implemented the toggle and selected 1P manual-click learned skill route.

## Task: 2026-05-20 DebugModifiedUI Choice Application

### Task title

Wire `Canvas/DebugModifiedUI` so learned active skills can apply CSV-backed trait/master choices through debug buttons.

### Goals

- Let `DebugUI` open `DebugModifiedUI` from `AmodifierBtn` through `EmodifierBtn`.
- Bind `Trait1`-`Trait5` and `Master1`-`Master2` button text from `monster_skill_choices.csv` runtime choice data.
- Apply the clicked active-skill enhancement through the same `RunSession` choice path used by Offering enhancement picks.
- Close `DebugModifiedUI` from its own `Close` button and when the parent `DebugUI` closes.

### Constraints

- Role Owner is Code Builder.
- Implementation stays in `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs`; no new scene patch was needed because the authored objects already exist in `NewRunScene`.
- Runtime rules should stay aligned with current Offering enhancement rules: max three active traits per skill, then one master choice.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. `DebugUI` now owns `DebugModifiedUI` open/close flow, dynamic choice text binding, and debug application of active enhancement choices.

### Next Actions

- User verifies in Play Mode that `AmodifierBtn`-`EmodifierBtn` only open for learned skills, that `Trait`/`Master` text matches the current monster/skill choice rows, and that clicked choices immediately affect runtime behavior.
- If a separate two-label title/description layout is desired later, add dedicated `Desc` child objects under the `DebugModifiedUI` buttons before changing code again.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains authored objects `Canvas/DebugModifiedUI`, `DebugModifiedUI/Trait1`-`Trait5`, `DebugModifiedUI/Master1`-`Master2`, `DebugModifiedUI/Close`, and `DebugUI/ABtn/AmodifierBtn` through `DebugUI/EBtn/EmodifierBtn`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now serializes `debugModifiedPanel`, `modifierOpenButtons`, `modifierCloseButton`, `traitButtons`, and `masterButtons`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now resolves `DebugModifiedUI` scene paths, closes the modifier panel in `Awake()`, and binds modifier-open and modifier-apply button listeners.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now populates `Trait` and `Master` button `Text (TMP)` labels from `SkillChoiceDefinition.Title` and `DescriptionText`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now applies clicked active enhancement choices through `RunSession.RecordOfferingChoice(...)`, `RunSession.AccumulateReward(...)`, `RefreshRuntimeSkillModels(...)`, and `MonsterPanelUI.RefreshNow()`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` now enforces the same current active-choice availability policy as Offering: up to three `ActiveEnhancement` picks, then up to one `ActiveMaster`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings after one transient parallel-build file lock.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-20: User added `Canvas/DebugModifiedUI` to the scene and requested Code Builder wiring so active skill modifier buttons could apply `monster_skill_choices.csv` trait/master choices in debug flow.

## Task: 2026-05-20 MonsterPanel Multi-Slot Runtime Binding

### Task title

Convert `MonsterPanelUI` from a selected-1P-only binding to a runtime slot-driven `1PMonster`-`5PMonster` panel binding.

### Goals

- Make `MonsterPanel` refresh `1PMonster` through `5PMonster` from actual player runtime slot ownership instead of only from `Players[0]`.
- Keep each panel slot bound to its own `Monster Image` and `Active1`-`Active3` children without requiring a scene hierarchy rewrite.
- Hide panel slots that do not currently have a player monster runtime model.

### Constraints

- Role Owner is Code Builder.
- The implementation scope is limited to `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. `MonsterPanelUI` now resolves `1PMonster`-`5PMonster` and refreshes them by `UnitIdentity.SlotIndex`.

### Next Actions

- User verifies in Play Mode that manifested player monsters in slots `2P`-`5P` now show their `Monster Image` and learned active skill icons in the matching `MonsterPanel` slots.
- If any slot still stays hidden in Play Mode, inspect whether that runtime model is missing from `combatManager.Roster.Players` rather than extending UI code first.

### Evidence

- Before this task, `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs` bound only `MonsterPanel/1PMonster` and resolved the selected entry from `Players[0]`.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:11-15` now defines `MaxPartySlots = 5` and stores slot views in `monsterSlots`.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:44-58` now resolves a `MonsterUnitRuntimeModel[]` by runtime slot instead of a single selected-player entry.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:128-166` now resolves `1PMonster` through `5PMonster` under `MonsterPanel` by name and binds each slot view on demand.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs:230-323` now applies per-slot monster image and active-skill UI refresh through `MonsterPanelSlotView.SetRuntime(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs:372-384` already identifies player roster ownership by `identity.Side == UnitSide.Player && identity.SlotIndex == slotIndex`, which matches the slot authority now used by `MonsterPanelUI`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains `MonsterPanel`, `1PMonster`, `2PMonster`, `3PMonster`, `4PMonster`, and `5PMonster`, so the implemented code-side binding matches authored scene objects.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-20: User requested Code Builder work to convert `MonsterPanelUI.cs` from a 1P-only structure to a multi-slot structure after confirming the issue was code-side wiring rather than just inactive scene objects.

## Task: 2026-05-20 Manifest Success Popup Label Cleanup

### Task title

Remove hardcoded mojibake labels from `MenifestedSuccessPopUp` while confirming authored spawn-point active state.

### Goals

- Replace the broken hardcoded labels in the Menifest success popup description with readable player-facing text.
- Keep the popup bound to the existing CSV/runtime-backed monster fields instead of adding a new data path.
- Verify whether `1PSpawnPoint` through `5PSpawnPoint` actually required a scene activation change before touching spawn logic.

### Constraints

- Role Owner is Code Builder.
- The user explicitly limited implementation scope: do not change Manifest spawn logic code in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented for popup text. Spawn-point activation did not require a repository scene edit because the inspected `NewRunScene.unity` file already stores `1PSpawnPoint` through `5PSpawnPoint` as active.

### Next Actions

- User verifies in Play Mode that `Canvas/MenifestedSuccessPopUp/MonsterDesc` no longer shows broken labels.
- If Manifest spawn placement still fails after this, treat it as a separate spawn/runtime investigation task instead of extending this UI-only fix.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs:1349-1350` now formats the popup description labels as `?띿꽦` and `?꾪닾?? instead of the previous mojibake hardcoded fragments.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs:1348-1351` still uses the existing runtime-backed `monster.RoleSummary`, `monster.ElementLabel`, `monster.MaxHealth`, `monster.PowerStat`, `monster.ActiveSkillName`, and `monster.PassiveSkillName` fields.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` inspection showed the named objects `1PSpawnPoint`, `2PSpawnPoint`, `3PSpawnPoint`, `4PSpawnPoint`, and `5PSpawnPoint` with `m_IsActive: 1`, so no scene-file toggle patch was needed in this repository state.
- Unity MCP `manage_scene load` opened `Assets/Scenes/NewScene/NewRunScene.unity`, and Unity MCP scene inspection returned `3PSpawnPoint active=true activeInHierarchy=true` after load.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings after a first parallel build attempt hit only a transient `Assembly-CSharp.dll` file lock.

### History

- 2026-05-20: User requested Code Builder implementation for the Menifest success popup mojibake and asked to make the spawn-point objects active without changing spawn logic.

## Task: 2026-05-20 Reward Panel Grid Layout Inspector Exposure

### Task title

Expose RewardPanel clone placement as inspector-tunable grid settings on `InGameUIManager`.

### Goals

- Preserve the existing `RewardBtnContainer` template buttons as the visual source while moving clone placement authority into serialized script fields.
- Keep the first visible reward row at `y = 295`, keep `122` vertical spacing, and let the next column begin by X spacing instead of relying on template inference.
- Leave reward claim, prisoner choice, Offering, and Manifest UI interactions unchanged.

### Constraints

- Role Owner is Code Builder.
- UI conclusions must stay tied to inspected `InGameUIManager.cs` and `NewRunScene` hierarchy/RectTransform serialization.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. RewardPanel clone placement is now script-configured through inspector fields, and the same `NextBtn -> ContinueToNextDay()` flow now restores dead allies from session state before the next combat starts.

### Next Actions

- User verifies in Play Mode that cloned reward buttons follow the configured grid, that inspector changes on `InGameUIManager` immediately affect the next reward popup, and that dead allies return to their slots on the next day.
- If the UI needs more than two columns regularly, tune `rewardButtonColumnSpacingX` and `rewardButtonRowsPerColumn` in the scene inspector rather than changing code first.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now serializes `rewardButtonFirstColumnPosition`, `rewardButtonColumnSpacingX`, `rewardButtonRowSpacingY`, and `rewardButtonRowsPerColumn`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` `ArrangeRewardButton()` now preserves template anchor/pivot/size but writes clone `anchoredPosition` from serialized grid math.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` keeps `Canvas/RewardPanel/RewardBtnContainer/PrisonerBtn`, `DarkBtn`, and `GoldBtn`; inspected RectTransforms place those templates at `y = 295`, `122`, and `-53`.
- `Pakuri/Assets/Scripts2/InGame/Core/StageManager.cs` now restores party members from session state and then restores HP before `StartCurrentDay()`, which is part of the same `NextBtn -> ContinueToNextDay()` UI flow owned by `InGameUIManager`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now rebuilds missing selected/manifested player slots from `RunSession` during that next-day UI flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-20: User requested Code Builder work to change RewardPanel clone layout math and make the X/Y placement tunable from the governing UI script inspector.

## Task: 2026-05-19 Offering Choice1-3 Data Binding Refresh

### Task title

Bind Offering `Choice1`-`Choice3` UI from the unified monster choice CSV path.

### Goals

- Show title, description, and icon from the unified `monster_skill_choices.csv` choice rows.
- Keep Offering availability driven by the slim `monster_modifier_skill_choice.csv` gate rows plus learned-skill state.
- Remove the old one-line button label fallback for enhancement Offering rows.

### Constraints

- Role Owner is Code Builder.
- UI conclusions must stay tied to inspected scene hierarchy and inspected `InGameUIManager.cs`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. The active Offering panel now binds icon/title/description from exact choice rows instead of the removed reward text columns.

### Next Actions

- User verifies in Play Mode that `OfferingPanel/Choice1`-`Choice3` show the intended icon, title, and description for active, passive, and enhancement offerings.
- If later UI localization is added, keep these bindings data-driven through the unified choice rows.

### Evidence

- Scene hierarchy inspection confirmed `Canvas/OfferingPanel/Choice1`, `Choice2`, and `Choice3` each contain child objects named `Icon`, `Text (TMP)`, and `Desc`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves those child bindings once through `ResolveButtonViews(...)` and writes icon/title/description through `BindChoiceButton(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves enhancement offerings through `ResolveChoice(reward.RewardId)` instead of the removed reward-row `linked_choice_id` / `title` / `description` fields.
- Active and passive Offering rows now use the monster plus learned skill/passive display name and their skill icons; enhancement rows use the exact choice row title/description plus `ResolveChoiceIcon(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now enforces learned-choice availability by exact `choice_id`, with active enhancements capped at three per skill, active masters unlocked after three active enhancements, and passive enhancements capped at one per passive skill.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing MSB3277 warnings remained.

### History

- 2026-05-19: Code Builder rewired the Offering panel so `Choice1`-`Choice3` bind `Icon`, `Text (TMP)`, and `Desc` from unified choice rows and no longer depend on the removed reward-row title/description/modifier columns.

## Task: 2026-05-18 NewRun Prefix Removal UI Reference Update

### Task title

Update UI scripts after removing `NewRun` from runtime manager script names.

### Goals

- Keep UI references compiling after `NewRunSceneEntryManager` became `SceneEntryManager`.
- Keep UI references compiling after `NewRunStageManager` became `StageManager` and `NewRunStageState` became `StageState`.
- Preserve existing UI behavior.

### Constraints

- Role Owner is Code Builder.
- This is a behavior-preserving naming refactor.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want UI behavior confirmation after the rename.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs`, `InGameUIManager.cs`, and `MonsterPanelUI.cs` now reference `StageManager` and/or `SceneEntryManager`; the Menifest flow now lives inside `InGameUIManager.cs`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now checks `StageState.RewardReady`.
- Search found no remaining `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunStartContext`, or `NewRunStageState` references in scripts, scene assets, prefab assets, asset files, or `Assembly-CSharp.csproj`.
- Runtime/editor builds passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-18: Code Builder updated UI script references as part of removing `NewRun` from current runtime script filenames and type names.

## Source: boards\COMBAT\ENEMY_BLACKBOARD.md

## Task: 2026-05-18 CSV-Backed Stage1 Enemy Passives

### Task title

Apply Stage 1 enemy passives from CSV passive ID/value fields.

### Goals

- Remove Stage 1 enemy passive stat changes from `StageOneSkill` hardcoded branches.
- Apply `PhysicalDamageUp` only to Physical damage.
- Keep existing defense, crit, healing, and incoming-damage passive behavior through reusable passive IDs.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- User verifies in Play Mode that Physical enemy passive buffs affect only Physical attacks and do not affect non-Physical enemy damage.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` now contains the integrated Stage 1 enemy passive application helper and switches on `EnemyUnitRuntimeModel.PassiveSkillId` instead of `StageOneSkill`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` maps `PhysicalDamageUp` to `PassivePhysicalDamageMultiplier`, `DefenseUp` to all defense stats, `CritChanceUp`, `CritDamageUp`, `HealingUp`, and `IncomingDamageDown`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitRuntimeModel.cs` now stores `PassiveSkillId`, `PassiveSkillValue`, and `PassivePhysicalDamageMultiplier`.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs` multiplies `PassivePhysicalDamageMultiplier` only when the resolved damage attribute is `DamageAttribute.Physical`.
- Unity-MCP editor execution after CSV sync returned `sword=PhysicalDamageUp:0.1:phys=1.1:out=1`, confirming the old generic outgoing multiplier stays at `1` for the swordsman passive.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Code Builder changed Stage 1 enemy passive application from skill-kind hardcoding to CSV passive ID/value application.

## Source: boards\COMBAT\STATUS_EFFECT_BLACKBOARD.md

## Task: 2026-05-18 LineAttack Status Application

### Task title

Route LineAttack status application through the shared status runtime.

### Goals

- Let Eve-B apply slow through `InGameCombatManager.ApplyStatus(...)`.
- Reuse CSV status fields for LineAttack skills.
- Avoid a separate Eve-only slow implementation path.

### Constraints

- Role Owner is Code Builder.
- Status chance and status ID are read from `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build plus Unity-MCP mapping inspection.

### Next Actions

- User verifies in Play Mode that Eve-B applies slow at the expected 20% tick chance and that the status label refreshes through the shared unit actor path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` exposes shared status-spec resolution and uses it for projectile and beam skills.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` applies status via `InGameCombatManager.ApplyStatus(...)`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Eve-B row has `status_effect_id=slow`, `status_chance=0.2`, and `status_effect_label=둔화`.
- Unity-MCP mapping inspection returned `status=slow|chance=0.2` for Eve-B.
- Runtime/editor builds passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Eve-B LineAttack implementation reused shared status runtime instead of adding an Eve-only slow path.

## Task: 2026-05-15 Rounded HP Shield Display Baseline

### Task title

Keep HP and shield mutation/display rules grounded in the current rounded-resource implementation.

### Goals

- Preserve whole-number HP and shield mutation results.
- Preserve left-to-right HP fill behavior inside the authored actor background bounds.
- Preserve current damage popup formatting and actor refresh ownership.

### Constraints

- Role Owner is Code Builder.
- This retained baseline is still relevant because HP/shield display remains part of the active InGame combat presentation.
- Detailed intermediate follow-up history is preserved in the archive snapshot.

### Role Owner

Code Builder

### Status

Retained as an active display rule that still affects current combat/runtime work.

### Next Actions

- If shield timing or presentation changes later, update this file together with `boards/RUN/RUN_BLACKBOARD.md` and `boards/UI/RUNSCENE_UI.md`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now contains the integrated resource-mutation helper that rounds applied damage, HP, and shield values.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs` owns the shared left-anchored HP/shield fill presentation used by `MonsterUnitActor.cs` and `EnemyUnitActor.cs`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` still routes rounded damage popup display through the actor layer.

### History

- 2026-05-15: Rounded HP/shield mutation and stable HP fill positioning were recorded as the current baseline.
- 2026-05-18: Code Builder moved shared HP/shield fill and damage-popup presentation from separate actor scripts into `UnitActorView.cs`.

## Task: 2026-05-18 Area Skill Status Application

### Task title

Route AreaAttack and SingleAttack status application through the shared status runtime.

### Goals

- Apply Eve C chill and Eve E vulnerable from CSV-driven area ticks.
- Apply one-shot area statuses through the same shared status helper path.
- Keep unsupported design-only labels at `status_chance=0` unless `StatusEffectKind` supports them.

### Constraints

- Role Owner is Code Builder.
- Status id/chance/label values are read from `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C applies chill per tick and Eve E applies vulnerable per tick.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` applies statuses through `InGameCombatManager.ApplyStatus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` reuses `ProjectileSkillExecutor.ResolveStatusSpec(...)` for `ZoneSkillExecutor` and `SingleAttackSkillExecutor`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` has `eve-c status_effect_id=chill status_chance=1` and `eve-e status_effect_id=vulnerable status_chance=1`.
- Unity-MCP `InGameSkillDataValidator.ValidateCatalog()` returned `valid=True; errors=0; warnings=0`.

### History

- 2026-05-18: Code Builder added area-status routing while adding AreaAttack and SingleAttack runtime execution.

## Task: 2026-05-21 Ariel-D SingleAttack Status Target Fix

### Task title

Keep Ariel-D status application on one strongest enemy.

### Goals

- Ensure Ariel-D's status application follows the same single target as its damage.
- Keep the status effect prefab path behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- This task does not implement party focus-target AI.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Ariel-D's mark/status VFX is attached to only the highest-HP enemy.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` applies damage and `TryApplyStatus(...)` to exactly one target in the `!areaCoversAll && areaRadius <= 0f` branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now prevents explicit-selection SingleAttack rows from setting `single.Area.CoverAll=true`.
- Ariel-D's CSV row has `target_selection=HighestHealth` and `status_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_D.prefab`.
- Runtime and Editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP console warning/error read after validation returned only MCP client handler logs.

### History

- 2026-05-21: After Mark/Execute conversion, Ariel-D still applied through the cover-all area branch because `Area.CoverAll` ignored `target_selection`; Builder aligned the SingleAttack area cover flag with explicit target selection.

## Task: 2026-05-22 Skill Targeting and Effect Utility Refactor

### Task title

Fix Self multi-effect targeting and route status/visual helpers through shared utilities.

### Goals

- Make `SkillTargetSide.Self` resolve to the caster only.
- Keep ally/all-allies targeting behavior unchanged.
- Centralize status application and common skill visual spawning paths.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was not run because Reviewer stage requires explicit user permission in this repository workflow.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Self multi-effects no longer apply to all allies.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTargetingUtility.cs` returns `new[] { caster }` for `SkillTargetSide.Self`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` delegates `FindNearestTarget`, `DirectionToTarget`, and `ResolveTargetList` to `SkillTargetingUtility`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs` now owns the shared `ApplyStatus(...)` chance path used by projectile, zone, line, and SingleAttack paths.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillVisualSpawnUtility.cs` now owns transient and attached skill visual spawning used by SingleAttack, multi-effect, buff, and shield paths.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity-MCP forced refresh removed the new utility type compile errors; remaining console entries were Unity graph/MCP client handler exceptions, not script compiler errors.

### History

- 2026-05-22: User requested Self target bug fix and utility extraction after Skills subtree review findings.

## Source: boards\DATA\DATA_BLACKBOARD.md

## Task: 2026-05-18 Monster Skill Active Duration CSV Field

### Task title

Add structured active-duration data for CSV-driven LineAttack skills.

### Goals

- Avoid parsing duration out of description text for Eve-B.
- Keep LineAttack duration as runtime CSV data.
- Preserve the current `monster_skills.csv` authority for skill damage, timing, and status tuning.

### Constraints

- Role Owner is Code Builder.
- Data values must stay in CSV, not skill-ID-specific code branches.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and synced through Unity-MCP CSV runtime catalog validation.

### Next Actions

- Future sustained LineAttack rows should set `active_duration_seconds` instead of relying on prose descriptions.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now includes `active_duration_seconds`.
- Eve-B row sets `active_duration_seconds=1.2`, while its damage, coefficient, width, cooldown, tick interval, status ID, status chance, and status label remain in the same CSV row.
- `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` executed through Unity-MCP and returned `csv-runtime-sync-ok`.
- Unity-MCP mapping inspection confirmed Eve-B maps to `BeamSkillData` with active duration `1.2`, tick `0.15`, damage `12`, coefficient `1.6`, width `3.2`, status `slow`, and chance `0.2`.

### History

- 2026-05-18: Added `active_duration_seconds` to support Eve-B without hardcoded duration values.

## Task: 2026-05-18 Prisoner/Offering UI Data Source Check

### Task title

Confirm CSV-backed display fields used by reward and Offering UI cleanup.

### Goals

- Confirm `stage1-swordsman` is valid enemy ID data, not corrupted CSV text.
- Keep player-facing prisoner names sourced from `stage_one_enemies.csv` display names through the runtime catalog.
- Keep Offering choice labels sourced from current monster skill, passive, and reward definition display fields.

### Constraints

- Role Owner is Code Builder.
- No CSV file was changed in this task.
- No authoritative UI localization CSV was found for static UI labels such as Reward, Prisoner, Gold, Dark Trace, Active, Passive, or Enhancement.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Confirmed and code path updated.

### Next Actions

- If static UI labels need localization, create or identify a dedicated UI string CSV before replacing the remaining English placeholder labels.

### Evidence

- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\stage_one_enemies.csv | Where-Object { $_.enemy_id -eq 'stage1-swordsman' }` returned `display_name : 검사`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` assigns `enemy.DisplayName = sourceEnemy.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves prisoner display text through `GameDataCatalog.GetStageOneEnemyById(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now uses CSV-backed `DisplayName`, `Title`, `DescriptionText`, `Summary`, and IDs for Offering choice text instead of broken hardcoded fragments through its integrated Offering flow helper.

### History

- 2026-05-18: Code Builder inspected CSV and runtime data definitions after the user reported code-side mojibake, then removed the broken hardcoded UI string fragments without changing CSV source data.

## Task: 2026-05-18 Monster AreaAttack And SingleAttack Runtime Data

### Task title

Split sustained AreaAttack rows from one-shot SingleAttack rows in monster skill CSV data.

### Goals

- Keep Eve C/E as sustained `AreaAttack` skills backed by `ZoneSkillData`.
- Add `SingleAttack` for one-shot area damage skills listed by the user.
- Correct Eve C/D display names against the Eve reference skill files.

### Constraints

- Role Owner is Code Builder.
- Numeric skill values stay in `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C/E tick over their authored durations and that SingleAttack skills apply one immediate area hit.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` names Eve C as `프로스트 필드`; `d-static-override.md` names Eve D as `스태틱 오버라이드`; `e-drone-beacon.md` names Eve E as `플라즈마 필드`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` rows for `ariel-c`, `ariel-e`, `rin-e`, `vega-b`, and `eve-d` now use `runtime_kind=SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` rows for `eve-c` and `eve-e` now use `runtime_kind=AreaAttack`; Eve C has `active_duration_seconds=4`, and Eve E has `active_duration_seconds=5`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` defines `SkillRuntimeKind.SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` defines the new one-shot area SkillData type.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps `SingleAttack` to `SingleAttackData` and keeps `AreaAttack` mapped to `ZoneSkillData`.
- Unity-MCP `InGameSkillDataValidator.ValidateCatalog()` returned `valid=True; errors=0; warnings=0`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: User clarified that Eve C/E should be `AreaAttack`, that row 46 and rows 5/7/17/34 should be one-shot area attacks, and requested Code Builder implementation.

## Task: 2026-05-18 Stage1 Enemy Passive CSV Fields

### Task title

Move Stage 1 enemy passive effect values into CSV-backed fields.

### Goals

- Add reusable passive IDs and numeric values beside the existing passive display name.
- Keep same-effect passive variants reusable through one ID with different values.
- Keep Physical damage passives represented as `PhysicalDamageUp`.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- Future Stage 1 enemy passive rows should set `passive_skill_id` and `passive_skill_value` rather than adding skill-kind-specific branches.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` now has `passive_skill_id` and `passive_skill_value` columns.
- The supported passive IDs are validated in `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs`: `PhysicalDamageUp`, `DefenseUp`, `CritChanceUp`, `CritDamageUp`, `HealingUp`, and `IncomingDamageDown`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.EnemyDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `EnemyDefinition.cs` now carry the passive ID/value from CSV into runtime definitions.
- Unity-MCP editor execution of `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` followed by `UnitFactory.CreateEnemy(...)` returned `sword=PhysicalDamageUp:0.1:phys=1.1:out=1;priest=HealingUp:0.15:heal=1.15:phys=1;captain=PhysicalDamageUp:0.12:phys=1.12`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- `git diff --check --` on the changed passive-related files passed with only line-ending warnings.

### History

- 2026-05-18: Code Builder added CSV-backed enemy passive IDs/values and synced them into runtime enemy models.

## Task: 2026-05-21 Explicit Target Selection SingleAttack Data Mapping

### Task title

Respect `target_selection` when mapping zero-radius SingleAttack CSV rows.

### Goals

- Keep legacy zero-radius SingleAttack rows with blank `target_selection` able to cover all targets.
- Let explicit target-selection rows such as Ariel-D route as one-target SingleAttack skills.

### Constraints

- Role Owner is Code Builder.
- No CSV data values were changed in this follow-up.
- The behavior is grounded in the active runtime CSV row and mapper code.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- For future zero-radius SingleAttack rows, leave `target_selection` blank only when full coverage is intended.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Ariel-D row contains `radius=0` and `target_selection=HighestHealth`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now sets `single.Area.CoverAll = source.Radius <= 0f && string.IsNullOrWhiteSpace(source.TargetSelection)`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-21: User reported Ariel-D looked like it hit all enemies; Builder fixed the data-to-runtime cover-all mapping so explicit `target_selection` wins over zero radius.

## Task: 2026-05-22 Skill Choice Beam Width Bonus

### Task title

Add CSV-backed `beam_width_bonus` for beam/line skill width upgrades.

### Goals

- Separate beam width upgrades from radius upgrades.
- Preserve Eve-B trait 2's damage +30% while moving 광선 폭 +30% to a dedicated field.
- Carry the new CSV field into runtime `SkillExecutionSnapshot`.

### Constraints

- Role Owner is Code Builder.
- Existing non-beam width notes that are still marked unsupported were not remapped.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve-B trait 2 in Play Mode: damage remains +30%, beam/line width increases by +30%.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now has `beam_width_bonus` after `max_health_bonus`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` row `eve-b-trait-2` now has `damage_multiplier=1.3`, blank `radius_multiplier`, and `beam_width_bonus=0.3`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs` reads `beam_width_bonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, `SkillDefinition.cs`, `SkillChoiceEffectSpec.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` carry `BeamWidthBonus` into skill execution.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-22: User requested `Beam_Width_Bonus`-style enhancement support so 광선 폭 +30% scales beam effect width instead of using radius fields.

## Source: boards\DATA\GAMEDATA_ASSET_BLACKBOARD.md

## Task: 2026-05-19 CSV Source Asset Import Recovery

### Task title

Harden runtime source catalog sync against not-yet-imported CSV assets.

### Goals

- Keep `PakuriCsvRuntimeSourceCatalog.asset` sync resilient when a source CSV exists on disk but Unity has not yet produced the `TextAsset`.
- Preserve the active `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime` sync path.

### Constraints

- Role Owner is Code Builder.
- Asset conclusions must stay grounded in inspected editor sync code, actual source files, and Unity console evidence.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- Reuse this recovery path for future externally-edited CSV assets instead of adding manual pre-import steps.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` now refreshes and imports a source CSV asset synchronously when `AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath)` initially returns `null`.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` and its `.meta` existed on disk while the user-facing auto-sync stack trace still reported the imported `TextAsset` as missing, confirming the failure lived in asset import state rather than in the file path string.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` after the fix logged a successful sync to `Assets/Resources/Pakuri/CSVRuntime` and did not reproduce the previous missing-TextAsset fatal exception.

### History

- 2026-05-19: Code Builder added a synchronous refresh/import retry path so runtime source catalog sync can recover from externally-created or freshly-renamed CSV assets that are present on disk but not yet imported into Unity's AssetDatabase.

## Task: 2026-05-18 Eve-B EffectManager Wiring Evidence

### Task title

Record Eve-B LineAttack visual asset availability and scene mapping.

### Goals

- Keep Eve-B visual authority grounded in the existing prefab and `EffectManager` scene mapping.
- Avoid reintroducing skill-effect prefab path authority into `monster_skills.csv`.

### Constraints

- Role Owner is Code Builder.
- User-authored prefab art/layout is preserved.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Confirmed through Unity-MCP during Eve-B LineAttack implementation.

### Next Actions

- If future monster LineAttack skills need visuals, wire their prefabs through `EffectManager` in the same style.

### Evidence

- Unity-MCP asset info confirmed `Assets/Prefab/Skill/Eve/Eve_B.prefab` exists as a `UnityEngine.GameObject` with GUID `224f5e7622cd0264b961ee388a015d65`.
- Unity-MCP `GameManager` component inspection confirmed `EffectManager` maps monster `eve` skill `eve-b` to `Assets/Prefab/Skill/Eve/Eve_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` resolves LineAttack visuals through `EffectManager.ResolveMonsterSkillEffectPrefab(...)`.

### History

- 2026-05-18: Eve-B LineAttack implementation confirmed the current `EffectManager` prefab route instead of adding prefab paths back to monster skill CSV rows.

## Source: boards\MON\EVE_MONSTER.md

## Task: 2026-05-18 Eve-B LineAttack Runtime

### Task title

Implement Eve-B as a reusable CSV-driven LineAttack and translate Eve skill rows.

### Goals

- Translate Eve A-J rows in `Pakuri/Assets/CSVdata/source/monster_skills.csv` to Korean display text.
- Keep Eve-B tuning in CSV instead of hardcoded skill-ID branches.
- Route Eve-B through the shared `BeamSkillData` / LineAttack runtime so later monster LineAttack skills can reuse it.

### Constraints

- Role Owner is Code Builder.
- Numeric tuning must come from `monster_skills.csv`.
- `Eve_B.prefab` is the visual asset for Eve-B.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated by runtime/editor builds, Unity CSV sync, and Unity-MCP data mapping inspection.

### Next Actions

- User verifies in `NewRunScene` Play Mode that learned Eve-B fires as a line attack, shows `Assets/Prefab/Skill/Eve/Eve_B.prefab`, ticks for 1.2 seconds at 0.15 second intervals, and applies slow at the CSV chance.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has Korean Eve A-J display/description/summary rows and `active_duration_seconds`; Eve-B is `LineAttack`, damage `12`, spell coefficient `1.6`, width `3.2`, cooldown `6.5`, active duration `1.2`, tick interval `0.15`, status `slow`, chance `0.2`, label `둔화`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now carry `ActiveDurationSeconds` into `SkillTimingSpec.ActiveDuration`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now implements `BeamSkillExecutor` for reusable line targeting, status resolution, visual instantiation, and CSV-driven damage/timing.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` applies repeated line ticks to targets inside the beam rectangle and applies status through `InGameCombatManager.ApplyStatus(...)`.
- Unity-MCP asset info confirmed `Assets/Prefab/Skill/Eve/Eve_B.prefab` exists with GUID `224f5e7622cd0264b961ee388a015d65`.
- Unity-MCP `GameManager` component inspection confirmed `EffectManager` maps monster `eve` skill `eve-b` to `Assets/Prefab/Skill/Eve/Eve_B.prefab`.
- Unity-MCP CSV mapping inspection returned `name=프리즘 레이|runtime=LineAttack|activeDuration=1.2|cooldown=6.5|tick=0.15|beamDuration=1.2|beamTick=0.15|damage=12|coef=1.6|width=3.2|status=slow|chance=0.2`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity-MCP refresh reached idle; console warning/error read after clearing showed only `UnityEditor.Graphs.Edge.WakeUp` and MCP client handler logs, not C# compile errors.

### History

- 2026-05-18: User requested Eve skill CSV Korean translation and Eve-B LineAttack implementation using `monster_skills.csv` tuning and `BeamSkillData.cs` as the reference structure.

## Source: boards\MON\VEGA_MONSTER.md

## Task: 2026-05-13 Vega Battlefield Facade Registration

### Task title

Route Vega battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Vega skill behavior while replacing direct battlefield list registration writes.
- Keep Vega projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega skills in Play Mode if needed.

### Evidence

- `CombatRuntimeVegaSkills.cs:706` now calls `AddBattlefieldProjectile(...)`.
- `CombatRuntimeVegaSkills.cs:888`, `:905`, and `:919` now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Vega battlefield object registration through facade methods.

## Task: 2026-05-10 Vega Unit Executor Migration

### Task title

Move Manifested Vega skill execution onto Vega unit executor paths.

### Goals

- Dispatch Manifested Vega A-E through Vega-specific `CombatUnitRuntime` / `CombatSkillRuntime` paths instead of the generic manifested fallback.
- Keep Vega A three-sword behavior while adding unit-owned Extermination Permit state.
- Make Manifested Vega B-E/F-J read the source Vega unit's Offering/passive state for silence, name marks, execute, vulnerability, critical, defense reduction, and cooldown charge.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Do not run Unity Play Mode from Codex.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP compile/console checks.

### Next Actions

- User verifies Manifested Vega A-E and F-J interactions in RunScene Play Mode, especially B silence/name marks, C action/attack buff, D area vulnerability/cooldown charge, E mark consumption/survivor vulnerability/kill cooldown charge, and A master afterimage.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:630` dispatches Manifested Vega through `TryTickVegaUnitSkill(...)` before the generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:139` implements the Vega unit skill dispatcher.
- `CombatRuntimeVegaSkills.cs:445`, `:507`, `:548`, and `:616` implement unit-owned Vega B/C/D/E active paths.
- `CombatRuntimeVegaSkills.cs:1068` implements `TryApplyVegaUnitProjectileHit(...)` for Vega projectile passive damage/critical/defense behavior.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:36` store Vega unit buff/charge state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings after a first parallel run hit only an `obj\Debug\Assembly-CSharp.dll` file lock.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- `git diff --check` over the three changed scripts completed with exit code 0 and only LF-to-CRLF warnings.
- Unity-MCP script refresh reached `resulting_state=idle`; console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-10: User requested Vega unit executor migration based on section 4 of `Pakuri/reference/Report/2026-05-08-monster-oop-refactor-manifested-work-status.html`.
- 2026-05-10: Code Builder added Vega unit dispatch, state, active paths, projectile hit damage hooks, and validation evidence.

## Source: boards\RUN\RUN_BLACKBOARD.md

## Task: 2026-05-18 NewRunScene Debug Skill Acquisition Runtime Sync

### Task title

Keep debug skill acquisition and Offering skill acquisition synchronized with active runtime models.

### Goals

- Debug A-E skill buttons must add the selected 1P monster's active skill through the Offering/session acquisition path.
- The active in-scene `MonsterUnitRuntimeModel` must receive the newly learned skill state before active runtime skills are rebuilt.
- Offering active-skill acquisition must use the same runtime-state synchronization so future Offering picks become usable immediately in the current combat scene.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented. Debug acquisition records the choice in `RunSession`, syncs the 1P runtime model's `UnitStateBucket`, rebuilds learned active skills, and refreshes the actor. 2026-05-18 follow-up verified the learned runtime-skill count drives `MonsterPanel` slot activation: one default learned skill activates `Active1`, and three learned skills activate `Active1`-`Active3`. Active slot Text now displays magazine count only for magazine skills.

### Next Actions

- User verifies in Play Mode that DebugUI skill acquisition immediately appears in the selected monster runtime and can be used by normal skill execution.
- If other runtime acquisition paths are added, keep the `RunSession` state and `MonsterUnitRuntimeModel.State` synchronization explicit.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` uses `RunSession.RecordOfferingChoice(...)`, `SkillRuntimeFactory.RebuildLearnedActiveSet(...)`, and `InGameCombatManager.RefreshUnitActor(...)` for debug active-skill acquisition.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs` reads `MonsterUnitRuntimeModel.SkillRuntime.ActiveSkills`, not all source-defined monster skills, so `Active1`-`Active3` represent the learned runtime skill list.
- Unity-MCP editor code simulation after registering an Eve model with only default `eve-a` returned `runtimeSkills=1`, `Active1=True`, `Active1Text=6/6`, `Active2=False`, and `Active3=False`.
- Unity-MCP editor code simulation after adding `eve-b` and `eve-e` to the same session path returned `runtimeSkills=3`, with `Active1=True:6/6`, `Active2=True:프리즘 레이`, and `Active3=True:플라즈마 필드`.
- Unity-MCP editor code simulation after the Active Text policy change returned `runtimeSkills=3; A1=True:textActive=True:text='6/6'; A2=True:textActive=False:text=''; A3=True:textActive=False:text=''`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now routes `PrisonerChoicePopUp/OfferingBtn` through `OpenOfferingFromPrisonerChoice()` and `PrisonerChoicePopUp/Menifested` through `TryManifestFromPrisonerChoice()`, both of which set `prisonerChoicePopUp` inactive after click.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now copies `RunSession.RunMonsterState.LearnedActives`, `LearnedPassives`, and `ChosenChoiceIds` into `MonsterUnitRuntimeModel.State` before rebuilding learned active skills through its integrated Offering flow helper.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` remains the persistent source for learned active/passive skill IDs.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeFactory.cs` remains the runtime authority for rebuilding learned active skill instances from `MonsterUnitRuntimeModel.State`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.

### History

- 2026-05-18: Code Builder added DebugUI active-skill acquisition and patched OfferingUI runtime-state sync after inspecting that `RunSession` and `MonsterUnitRuntimeModel.State` are separate data structures.
- 2026-05-18 follow-up: Code Builder moved `MonsterPanelUI` runtime driver to always-active `Canvas`, fixed unbound serialized slot view binding, and verified learned-skill slot activation without running Play Mode.
- 2026-05-18 follow-up: Code Builder changed Active slot Text to magazine-count-only and made Offering/Menifested prisoner choice buttons close `PrisonerChoicePopUp` immediately.

## Task: 2026-05-18 Reward Prisoner Display Name Source Fix

### Task title

Use runtime enemy display names for prisoner reward UI.

### Goals

- Keep reward prisoner IDs as internal IDs while showing player-facing enemy display names in the reward UI.
- Preserve the active `NewRunScene` reward and prisoner choice flow.
- Avoid treating `stage1-swordsman` as bad CSV data when the inspected CSV row is valid.

### Constraints

- Role Owner is Code Builder.
- CSV rows were inspected but not changed.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that prisoner reward buttons still open `PrisonerChoicePopUp`, then Offering/Menifested flows consume the same `RewardButtonView.PrisonerId`.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` has `enemy_id=stage1-swordsman` and `display_name=검사`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still stores `RewardButtonView.PrisonerId` as the original prisoner ID, but displays `ResolvePrisonerDisplayName(prisonerId)` on the button label.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` resolves display names through `ResolveCatalog()` and `GameDataCatalog.GetStageOneEnemyById(...)`.
- Runtime and Editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-18: User reported that the visible `stage1-swordsman` issue came from code-side mojibake. Code Builder kept the ID as runtime state and moved the visible name to the CSV-backed display name path.

## Task: 2026-05-18 AreaAttack And SingleAttack Execution Runtime

### Task title

Add NewRunScene runtime executors for sustained area skills and one-shot area skills.

### Goals

- Make `ZoneSkillData` execute sustained area ticks instead of only routing.
- Add `SingleAttackData` execution for one immediate area hit.
- Keep targeting, damage, and status application on shared `InGameCombatManager` and roster paths.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that selected/learned monster skills with `AreaAttack` and `SingleAttack` route through the new executors.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutorRegistry.cs` registers `SingleAttackSkillExecutor` and `ZoneSkillExecutor`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now implements `ZoneSkillExecutor.Execute(...)` and `SingleAttackSkillExecutor.Execute(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` applies area ticks through `InGameCombatManager.ApplyDamage(...)` and `ApplyStatus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` maps Area duration from `active_duration_seconds` when present.
- Runtime/editor builds passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Code Builder added reusable area execution after the user requested Eve C/E AreaAttack and new SingleAttack support.

## Task: 2026-05-18 Enemy Passive Runtime CSV Sync

### Task title

Keep Stage 1 enemy passive runtime state synchronized from CSV.

### Goals

- Ensure runtime enemy creation copies CSV passive ID/value fields into `EnemyUnitRuntimeModel`.
- Keep physical-damage passive effects separate from generic outgoing damage effects.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- User verifies in Play Mode once enemy prefab assignment and runtime combat behavior are exercised.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` now copies `EnemyDefinition.PassiveSkillId` and `PassiveSkillValue` to `EnemyUnitRuntimeModel`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitFactory.cs` now applies Stage 1 enemy passive multipliers through its integrated private helper methods instead of a separate `StageOneEnemyPassiveStatApplier.cs` file.
- Unity-MCP editor code synced CSV runtime catalogs and created stage-one enemies through `UnitFactory`, returning `sword=PhysicalDamageUp:0.1:phys=1.1:out=1;priest=HealingUp:0.15:heal=1.15:phys=1;captain=PhysicalDamageUp:0.12:phys=1.12`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Code Builder verified CSV passive ID/value data reaches created runtime enemy models through the current runtime catalog and unit factory path.

## Task: 2026-05-22 Runtime Skill Execution Cleanup

### Task title

Align NewRun skill execution with Self targeting, prefab scale, and beam width bonuses.

### Goals

- Fix Self multi-effect runtime resolution.
- Make prefab hitbox scale use resolved radius divided by base radius.
- Make beam width bonuses affect line hit width and prefab visual Y scale through resolved beam width.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- This task does not change enemy spawn/combat state flow.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies SingleAttack prefab radius upgrades and Eve-B beam width upgrades in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillAreaUtility.cs` resolves base radius, modified radius, and prefab scale factor.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now calls `SkillAreaUtility.ResolvePrefabScaleFactor(...)` for SingleAttack prefab hitbox scaling.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` resolves beam width with `1f + snapshot.BeamWidthBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` already scales sprite Y from resolved `width / sprite.bounds.size.y`, so `beam_width_bonus=0.3` increases the visual width and hit width by 30%.
- Runtime/editor builds passed with 0 errors; existing MSB3277 warnings remain.
- Unity-MCP forced refresh produced no remaining missing-type compiler errors after importing the new utility scripts.

### History

- 2026-05-22: User approved the previously discussed order: Self target fix, prefab scale cleanup, utility extraction, and beam width bonus support.

## Source: boards\UI\RUNSCENE_UI.md

## Task: 2026-05-18 NewRunScene DebugUI and MonsterPanel Skill UI

### Task title

Add `DebugUI` skill-learn buttons and `MonsterPanel` selected-monster skill status UI to `NewRunScene`.

### Goals

- `Canvas/DebugUIBtn` opens `Canvas/DebugUI`, and `Canvas/DebugUI/Close` closes it.
- `DebugUI` A-E buttons learn the selected 1P monster's A-E active skills when the skill exists and is not already learned.
- Missing or unavailable skills return without side effects.
- `MonsterPanel/1PMonster` shows the selected monster image and up to three learned active skill slots.
- Magazine skills show current magazine count in each Active slot text.
- Cooldown/reload waits are visualized through each slot's `CooldownOverlay` image using a vertical filled overlay.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.
- Debug skill acquisition must use the same session/offering record path as Offering active-skill acquisition.

### Role Owner

Code Builder

### Status

Implemented and scene-wired. `DebugUIBtn` was renamed from the actual scene object `DebugBtn` after inspection showed the user-requested `DebugUIBtn` name did not exist yet. 2026-05-18 follow-up fixed `MonsterPanelUI` so it runs from always-active `Canvas`, forces `MonsterPanel/1PMonster` visible, binds serialized `Active1`-`Active3` slot view objects to the real child GameObjects, and uses remaining cooldown ratio for `CooldownOverlay.fillAmount`. A later 2026-05-18 follow-up changed Active slot Text so only magazine skills show `current/max`; non-magazine learned skills keep their Text object inactive and empty.

### Next Actions

- User verifies in Play Mode that `DebugUIBtn`, `Close`, A-E learn buttons, magazine counts, and vertical cooldown overlay timing match expected UX.
- If more than three learned active skills must be visible at once, expand the current `MonsterPanel` slot count beyond `Active1`-`Active3`.

### Evidence

- Unity scene hierarchy inspection showed `Canvas/DebugUI` with `Close`, `ABtn`, `BBtn`, `CBtn`, `DBtn`, `EBtn`, and showed `Canvas/DebugBtn` instead of `DebugUIBtn`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` records debug skill acquisition through `RunSession.RecordOfferingChoice(monsterId, string.Empty, string.Empty, sourceSkill.SkillId, string.Empty)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` synchronizes the selected player model `UnitStateBucket` from `RunSession` and rebuilds active runtime skills with `SkillRuntimeFactory.RebuildLearnedActiveSet`.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs` resolves `MonsterPanel/1PMonster/Monster Image`, `Active1`, `Active2`, `Active3`, their `Text (TMP)` children, and their `CooldownOverlay` images.
- 2026-05-18 follow-up evidence: Unity-MCP `find_gameobjects` found `Pakuri.InGame.MonsterPanelUI` on `Canvas` only, and `Canvas/MonsterPanel` exists as the controlled panel.
- 2026-05-18 follow-up evidence: Unity-MCP editor code simulation with Eve default learned state returned `runtimeSkills=1; panel=True; oneP=True; active1=True; active1Text=6/6; active2=False; active3=False; overlayFill=0.00; overlayActive=False`.
- 2026-05-18 follow-up evidence: Unity-MCP editor code simulation after learning `eve-b` and `eve-e` returned `runtimeSkills=3; Active1=True:6/6; Active2=True:프리즘 레이; Active3=True:플라즈마 필드`.
- 2026-05-18 follow-up evidence: after the Active Text policy change, Unity-MCP editor code simulation after learning `eve-b` and `eve-e` returned `runtimeSkills=3; A1=True:textActive=True:text='6/6'; A2=True:textActive=False:text=''; A3=True:textActive=False:text=''`.
- 2026-05-18 prisoner label diagnosis: `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` line 85 contains the hardcoded label `?щ줈\n{prisoners[i]}`, while CSV search found `stage1-swordsman` in source CSV as ASCII enemy id data. The observed `?щ줈\nstage1-swordsman` therefore comes from code-side label mojibake, not from the prisoner id CSV value.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now hides `PrisonerChoicePopUp` immediately after `OfferingBtn` or `Menifested` is clicked.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now performs the same `RunSession` -> `MonsterUnitRuntimeModel.State` sync before rebuilding learned active skills through its integrated Offering flow helper.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` has `Pakuri.InGame.DebugUI` and `Pakuri.InGame.MonsterPanelUI` on `Canvas`, with `Canvas/MonsterPanel` as the controlled panel, and the scene was saved through Unity MCP.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.
- Unity console after compile still showed only the pre-existing MCP client handler logs and `UnityEditor.Graphs.Edge.WakeUp` `NullReferenceException`; no new C# compile errors were reported.

### History

- 2026-05-18: Code Builder inspected `NewRunScene` UI hierarchy, implemented `DebugUI.cs` and `MonsterPanelUI.cs`, patched Offering runtime-state sync, wired components into `NewRunScene`, saved the scene, and verified runtime/editor builds with 0 errors.
- 2026-05-18 follow-up: User clarified that only learned runtime skills should fill up to `Active1`-`Active3`. Code Builder found the serialized `ActiveSkillSlotView[]` entries were non-null but unbound to child GameObjects, so `ResolveSlot` skipped binding and left authored placeholder text/visibility unchanged. `MonsterPanelUI.cs` now rebinds unbound slot views and the scene now drives `MonsterPanelUI` from `Canvas`.
- 2026-05-18 follow-up: User clarified that Active slot Text should not show skill names and should only show magazine count for magazine skills. Code Builder changed `MonsterPanelUI.cs` accordingly and added `PrisonerChoicePopUp` hiding wrappers for Offering/Menifested clicks in `InGameUIManager.cs`.

## Task: 2026-05-18 NewRunScene Reward/Offering Mojibake Cleanup

### Task title

Remove broken hardcoded reward and Offering labels from the active `NewRunScene` UI path.

### Goals

- Stop prisoner reward buttons from showing mojibake plus raw enemy IDs such as `stage1-swordsman`.
- Resolve prisoner display names through the runtime CSV catalog built from `stage_one_enemies.csv`.
- Remove broken hardcoded Korean fragments from Offering choice titles and fallback descriptions.
- Keep Offering titles/descriptions driven by monster, skill, passive, and reward data fields that originate from the current CSV runtime catalog.

### Constraints

- Role Owner is Code Builder.
- CSV data itself was not changed because `stage1-swordsman` already exists as a valid ASCII enemy ID and `stage_one_enemies.csv` already provides `display_name`.
- No authoritative CSV for static UI category labels such as Reward, Prisoner, Gold, or Dark Trace was found during this task, so those static labels remain code-side English placeholders.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that reward prisoner buttons show the CSV enemy display name, for example `Prisoner` / `검사` for `stage1-swordsman`.
- If Korean/static UI labels should be data-driven too, add or identify a dedicated UI localization CSV for labels such as Reward, Prisoner, Gold, Dark Trace, Active, Passive, and Enhancement.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` contains `stage1-swordsman,검사`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps source enemy `DisplayName` into `EnemyDefinition.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/GameDataCatalog.cs` exposes `GetStageOneEnemyById(string enemyId)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now calls `ResolvePrisonerDisplayName(prisonerId)` and uses `GameDataCatalog.GetStageOneEnemyById(...)` before falling back to the raw ID.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now builds active/passive Offering titles from CSV-backed `monster.DisplayName`, `skill.DisplayName`, and `passive.DisplayName`, and enhancement Offering titles/descriptions from the exact `monster_skill_choices.csv` row resolved by `reward.RewardId`.
- `Get-ChildItem -Path Pakuri\Assets\Scripts2\InGame\UI -Recurse -Filter *.cs | Select-String -SimpleMatch ...` found 0 remaining matches for the inspected mojibake fragments after the change.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP console read after compile showed only the existing MCP client handler log and existing `UnityEditor.Graphs.Edge.WakeUp` `NullReferenceException`; no new C# compile errors were reported.

### History

- 2026-05-18: User clarified the `stage1-swordsman` problem was not CSV corruption, but broken hardcoded UI strings in the active reward and Offering code path. Code Builder inspected the CSV/runtime catalog path, replaced the broken code-side strings, and verified builds.

## Source: boards\MON\ARIEL_MONSTER.md

## Task: 2026-05-22 Ariel CSV-Only And Small Shared-Contract Follow-Up

### Task title

Implement the Ariel rows previously classified as CSV-only or requiring only small shared runtime/data contracts.

### Goals

- Finish `ariel-h-trait-3`, `ariel-i-trait-2`, and `ariel-j-trait-1` without adding skill-specific execution code.
- Add the smallest shared contracts needed to finish `ariel-b-master-1`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-i-trait-1`, and `ariel-i-trait-3`.
- Re-scan Ariel choice coverage and record the exact rows still unsupported after this pass.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay reusable in shared runtime/data paths rather than adding Ariel-only branches.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly told Builder not to run Reviewer.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that `ariel-b-master-1` grants shield amount `+50%` and status ailment resistance `+30%` only while the shield status remains active.
- User verifies passive-choice-gated effects for `ariel-f-trait-3`, `ariel-g-trait-1/2/3`, `ariel-i-trait-1/3`, and `ariel-j-trait-1`.
- Remaining Ariel choice rows still unsupported after this pass are only `ariel-a-trait-5` and `ariel-d-trait-5`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:15` now marks `ariel-b-master-1` as `RuntimeImplemented` with `status_ailment_resistance_bonus=0.3`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:40-43` now mark `ariel-f-trait-3` and `ariel-g-trait-1/2/3` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:46-50` now mark `ariel-h-trait-3`, `ariel-i-trait-1/2/3`, and `ariel-j-trait-1` as `RuntimeImplemented`; `ariel-i-trait-2` now targets `ariel-d`, and `ariel-j-trait-1` now targets `ariel-e`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:15`, `:25`, `:27`, `:29-30`, `:33-34`, and `:37` add the gated Ariel-C, Ariel-F, Ariel-G, Ariel-I, and Ariel-J effect rows used by this pass.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGamePassiveEffectRuntime.cs:56-106` now builds a passive-choice snapshot so passive effect rows can gate on chosen passive choices.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:298-326`, `:1439-1550` now apply shield choice ailment-resistance overrides, allow effect rows to filter by active-skill attribute, and map crit-chance / ailment-resistance / flat-element-resistance status payloads into runtime status data.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:222-262` now resolves crit chance bonus, flat element resistance reduction, and ailment resistance from active statuses; `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs:18-48` now applies ailment resistance to harmful status application chance.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only `ariel-a-trait-5` and `ariel-d-trait-5`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the rows previously classified as CSV-only or requiring only a small shared contract.

## Task: 2026-05-22 Ariel Shared Trigger/Crit/Duration Runtime Completion And Coverage Audit

### Task title

Implement Ariel's remaining shared-runtime-driven active/master effects and audit full Ariel coverage.

### Goals

- Implement `ariel-b-master-2` through a reusable shield-absorb trigger contract.
- Implement `ariel-d-trait-4` through choice-driven target-count bonus support.
- Implement `ariel-d-master-1` by wiring live InGame critical damage into the shared damage path and letting the Ariel-D mark carry a critical-damage-taken bonus.
- Implement `ariel-d-master-2` through reusable status-expire trigger plus tracked incoming damage.
- Implement `ariel-e-trait-5` through reusable runtime extension of active shield status durations.
- Re-scan Ariel choice/effect/trigger coverage and record the remaining unsupported rows.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay generic in shared runtime/data paths; no Ariel-only execution branches were added for these effects.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly said not to run it.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and coverage-audited.

### Next Actions

- User verifies in Play Mode that `ariel-b-master-2` reflects absorbed shield damage to the attacker, `ariel-d-master-1` increases critical damage taken on the marked target, `ariel-d-master-2` bursts on mark expiry from tracked Holy damage, and `ariel-e-trait-5` extends existing ally shield durations.
- Remaining Ariel rows still needing future work are `ariel-a-trait-5`, `ariel-b-master-1`, `ariel-d-trait-5`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-h-trait-3`, `ariel-i-trait-1`, `ariel-i-trait-2`, `ariel-i-trait-3`, and `ariel-j-trait-1`.
- `ariel-b-master-1` remains only partial because the shield amount portion is implemented, but the ailment-resistance portion still has no shared runtime contract.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:16` marks `ariel-b-master-2` `RuntimeImplemented`; `:27` marks `ariel-d-trait-4` with `hit_target_count_bonus=1`; `:29-30` mark `ariel-d-master-1` and `ariel-d-master-2` implemented; `:35` marks `ariel-e-trait-5` implemented.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:5-6` add the reusable `OnShieldAbsorb` and `OnStatusExpire` Ariel trigger rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` adds `ariel-e-trait5-extend-shield-duration` as a shared `ExtendStatusDuration` effect row.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:55`, `:103-113`, and `:248-260` add `ExtendStatusDuration`, the new trigger events/damage source, and the new choice/runtime fields used by Ariel.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:11-13`, `:135-140`, `:271-277`, `:571`, `:577-618`, and `:834-962` now route crit-aware damage, shield-absorb triggers, status-expire triggers, tracked incoming damage recording, and shared status-duration extension.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs:82-133`, `:390-396`, and `:515-518` execute shield-absorb and status-expire triggers, resolve `ShieldAbsorbedAmount` / `TrackedIncomingDamage`, and prioritize the event target when required.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only `ariel-a-trait-5`, `ariel-b-master-1`, `ariel-d-trait-5`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-h-trait-3`, `ariel-i-trait-1`, `ariel-i-trait-2`, `ariel-i-trait-3`, and `ariel-j-trait-1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to start implementing the previously proposed shared-runtime Ariel fixes, skip Code Reviewer, and then verify whether all Ariel skills, traits, and master effects were now implemented.

## Task: 2026-05-22 Ariel Triggered SingleAttack Runtime

### Task title

Implement Ariel last-shot and shield-expiry trigger skills through CSV-driven SingleAttack reuse.

### Goals

- Add `monster_skill_triger.csv` as the CSV authority for trigger-driven hidden skill executions.
- Implement `ariel-a-master-1` as two last-magazine-projectile hit explosions at 0.5 second intervals using `Assets/Prefab/Skill/Ariel/Ariel_C.prefab`.
- Implement `ariel-b-trait-4` as shield-expiry/depletion damage using `Assets/Prefab/Skill/Ariel/ariel-b-trait-4_Skill.prefab`.
- Reuse SingleAttack-style target resolution and prefab hitbox collision instead of adding Ariel-only skill branches.

### Constraints

- Role Owner is Code Builder.
- Trigger rows remain CSV-owned; runtime code stays generic for trigger event dispatch.
- The requested file name is `monster_skill_triger.csv`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that `ariel-a-master-1` fires two `Ariel_C` prefab-hitbox explosions from the final Ariel-A magazine projectile hit, spaced by 0.5 seconds.
- User verifies in Play Mode that `ariel-b-trait-4` triggers when Ariel-B shield statuses expire by timer or depletion and that the prefab collider matches the intended visual area.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:3` defines `ariel-a-master1-last-shot-explosion` with event `OnMagazineLastProjectileHit`, `repeat_count=2`, `repeat_interval_seconds=0.5`, and prefab `Assets/Prefab/Skill/Ariel/Ariel_C.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:4` defines `ariel-b-trait4-shield-expire` with event `OnShieldExpire`, shield-applied-amount damage source, multiplier `0.6`, and prefab `Assets/Prefab/Skill/Ariel/ariel-b-trait-4_Skill.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:8` and `:13` mark `ariel-a-master-1` and `ariel-b-trait-4` as `ReferenceDirect` with trigger CSV notes.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:97-131` defines trigger event, trigger damage source, and `SkillTriggerDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs:12`, `:36`, `:202`, and `:334` implement projectile-hit trigger dispatch, shield-expire trigger dispatch, SingleAttack trigger execution, and prefab-hitbox overlap damage.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs:184-196` runs the last-magazine-projectile hit trigger once per projectile.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:100-104`, `:489-499`, and `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:164-179`, `:275-291`, `:401` collect expired/depleted shield statuses and preserve shield source metadata for trigger dispatch.
- `Pakuri/Assets/Prefab/Skill/Ariel/ariel-b-trait-4_Skill.prefab:119` now has a `BoxCollider2D`; `:162` records size `{x: 5.85, y: 5.46}`.
- Runtime and editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings; Unity-MCP CSV catalog sync logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-22: User asked Code Builder to create `monster_skill_triger.csv` and implement `ariel-a-master-1` plus `ariel-b-trait-4` as trigger-called SingleAttack-style prefab-hitbox skills.

## Task: 2026-05-22 Ariel CSV-First Choice Cleanup And Shield Runtime Modifiers

### Task title

Implement Ariel CSV-only fixes first, then shared shield/status runtime modifiers.

### Goals

- Correct Ariel-C stale `runtime_support_state` choice rows that are already implemented through multi-effect rows.
- Move Ariel-E conditional shield/damage/sanctuary behavior into `monster_skill_effects.csv`.
- Let Ariel-B shield amount/duration choices apply through the shared shield executor.
- Let Ariel-D status duration and Holy Exposure value choices apply through shared status snapshot handling.

### Constraints

- Role Owner is Code Builder.
- Keep damage/status/shield effect data in CSV where the current multi-effect schema can express it.
- Do not implement event-trigger behaviors such as last projectile, shield expiry, shield absorb reflection, or mark expiry in this pass.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies Ariel-B shield amount/duration choices, Ariel-B trait 5 Holy damage buff, Ariel-D trait 2/3 mark effects, and Ariel-E trait/master effects in Play Mode.
- Implement remaining event-trigger Ariel items separately: `ariel-a-master-1`, `ariel-b-trait-4`, `ariel-b-master-2`, and `ariel-d-master-2`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:18`, `:19`, `:21`, `:22`, and `:23` now mark Ariel-C trait/master rows as `ReferenceDirect` because existing `monster_skill_effects.csv` rows implement them.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:10`, `:11`, `:14`, and `:15` now map Ariel-B shield amount/duration/Holy-damage-buff support, with master 1 marked partial because status ailment resistance remains unsupported.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:25` now gives `ariel-d-trait-2` `status_element_damage_taken_bonus=0.08`; `:26` keeps `duration_bonus=3` and marks it supported.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:32`, `:34`, `:36`, and `:37` now mark Ariel-E trait/master shield, conditional damage, sanctuary, and master 2 shield support as `ReferenceDirect`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:3-8` now has Ariel-E default/replacement shield rows, Holy Exposure-only bonus damage, and the master 1 sanctuary damage-reduction status row.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` now has the Ariel-B trait 5 shielded-ally Holy damage status row.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:229-247` applies choice duration modifiers to resolved status durations.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1701-1718` applies shield snapshot damage/duration modifiers, and `:1757-1764` runs shield skill multi-effects after routed shield application.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP CSV sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`; console after clear/sync showed only that sync log and MCP client handler logs.

### History

- 2026-05-22: User asked Code Builder to implement the earlier classification in order: CSV-only items first, then CSV plus small shared runtime extensions.

## Task: 2026-05-22 Ariel-C/E Debug Learned Skill Auto-Spam Fix

### Task title

Stop DebugUI-learned Ariel-C/E SingleAttack support effects from repeatedly firing outside valid combat input/auto conditions.

### Goals

- Keep Ariel-C and Ariel-E learned active skills usable after DebugUI acquisition.
- Prevent Ariel-C buff and Ariel-E shield effects from repeating during spawn/reward or failed auto execution.
- Preserve Ariel-C/E CSV-owned multi-effect behavior and avoid Ariel-specific executor branches.

### Constraints

- Role Owner is Code Builder.
- The fix is generic in NewRunScene combat input/routing and shared SingleAttack/multi-effect execution.
- No Ariel CSV rows or prefab mappings were changed in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that DebugUI-learned Ariel-C/E persist, but only fire from selected 1P manual click while Auto is off or from Auto mode when a visible enemy exists in MainCamera during combat.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:5` keeps `ariel-c` as `SingleAttack`; `:7` keeps `ariel-e` as `SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:3` keeps `ariel-e-shield-base` as an all-ally shield using `Assets/Prefab/Skill/Ariel/Ariel_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:4-12` keep Ariel-C blessing/master rows in reusable multi-effect CSV data.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:55-72` now gates skill execution to `StageState.Combat`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:460-469` now requires selected 1P Auto plus visible MainCamera enemies for automatic player skill routing.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:689-699` now lets successfully applied support multi-effects count as routed, starting cooldown/recovery instead of retrying every frame.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User reported the cause was not DebugUI itself, but learned Ariel-C/E active skills being auto-routed later and failed SingleAttack executions repeatedly spawning effects. Code Builder fixed the generic route/input and SingleAttack multi-effect behavior.

## Task: 2026-05-22 Ariel F-J Passive CSV Runtime

### Task title

Implement Ariel F-J passive skills through the reusable CSV effect runtime.

### Goals

- Make Offering-acquired Ariel F-J passives produce runtime effects from CSV-owned data.
- Keep the implementation generic by attaching passive `monster_skill_effects.csv` rows to `PassiveDefinition`.
- Add the missing shield-received multiplier needed by Ariel-G.
- Gate Ariel-E's post-cast action-speed effect on learned passive `ariel-j` without Ariel-specific executor branches.

### Constraints

- Role Owner is Code Builder.
- User required stopping if the behavior could not be implemented through CSV runtime-read effect structure; inspected code showed it could be done by extending `monster_skill_effects.csv` and shared runtime consumers.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compiled, and synced through Unity CSV runtime catalog.

### Next Actions

- User verifies in Play Mode that Offering acquisition of Ariel F-J affects combat: F Holy damage, G shield received/start shield, H blessed ally bonuses, I holy-exposure damage taken, and J shielded Holy damage plus Ariel-E action speed.
- If G's one-shot shield must apply to allies spawned after combat start, extend the one-shot keying/target tracking; current runtime applies when learned passives are refreshed for existing roster entries.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` already has `ariel-f` through `ariel-j` as `Passive` rows with design values in their summaries.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` previously had an empty `PassiveSkillExecutor`, while `RunSession`/UI paths already copied learned passive IDs into `MonsterUnitRuntimeModel.State.LearnedPassiveSkillIds`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now stores `PassiveDefinition.PassiveEffects` and passive-gating fields on `SkillEffectDefinition`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains Ariel F-J rows: `ariel-f-party-holy-damage`, `ariel-g-shield-received`, `ariel-g-start-shield`, `ariel-h-blessed-holy-damage-speed`, `ariel-i-holy-exposure-damage-taken`, `ariel-j-shielded-holy-damage`, and `ariel-e-passive-j-action-speed`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGamePassiveEffectRuntime.cs` applies learned passive effect rows through the shared `SkillMultiEffectExecutor`; `InGameCombatManager.Update()` refreshes them every `0.25s`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now multiplies shield status amounts by `StatusEffectRuntime.ResolveShieldReceivedMultiplier(...)`, so Ariel-G's `status_shield_received_bonus=0.18` affects shield application.
- Unity console logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement Ariel F-J, but to stop and ask if the effects could not be implemented through CSV runtime-read structure.
- 2026-05-22: Code Builder extended the existing multi-effect CSV/runtime path for passive effects instead of adding Ariel-only runtime branches.

## Task: 2026-05-22 Ariel-C Multi-Effect CSV Runtime

### Task title

Implement Ariel-C blessing, traits, and master effects through reusable multi-effect CSV rows.

### Goals

- Keep Ariel-C base `SingleAttack` enemy damage on the shared one-shot area path.
- Add all-ally action-speed blessing, trait 2, trait 3, trait 5, master 1, and master 2 behavior through `monster_skill_effects.csv`.
- Avoid Ariel-C-specific executor branches.

### Constraints

- Role Owner is Skill Builder.
- Ariel-C reference values are grounded in `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md`.
- The reference names a second wave but does not specify a time interval, so the CSV row uses `Delayed` with `delay_seconds=0` and runtime schedules it on the next frame instead of inventing a numeric delay.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder

### Status

Implemented and non-gameplay verified.

### Next Actions

- User verifies in Play Mode that Ariel-C hits once normally, applies ally blessing, applies trait/master choices, and master 2 creates the second 60% wave.
- If the second wave needs a designer-authored visible delay later, edit `delay_seconds` in `monster_skill_effects.csv`.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md` lists Ariel-C base Holy damage `28`, spell coefficient `1.2`, radius `3.0`, all-ally action speed `+12%`, buff duration `4.0珥?, cooldown `8.0珥?, trait 2 `+6%`, trait 3 `+2珥?, trait 5 shielded-allies Holy damage `+10%`, master 1 spell power `+18%`, and master 2 second wave `60%`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps `ariel-c runtime_kind=SingleAttack`, `base_damage=28`, `spell_power_coefficient=1.2`, `radius=3`, and `cooldown_seconds=8`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains 9 `ariel-c` effect rows covering the action-speed blessing, trait combinations, shielded Holy damage, master 1 spell-power replacement, and master 2 second wave.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now separates Ariel-C effect application targets from visual placement with `center_mode` and `visual_anchor_mode`; Ariel-C ally buff rows keep `target_side=AllAllies`, use `visual_anchor_mode=AppliedTargets`, and use `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab` only on representative visual rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:11` keeps the Ariel-C master 2 damage wave on `center_mode=PrimarySkillCenter` and `skill_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_C.prefab`, so the second wave stays on the first SingleAttack center instead of reselecting an ally or a different target.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:12636` now maps `ariel-c` to `Assets/Prefab/Skill/Ariel/Ariel_C.prefab` in `EffectManager` for the base attack-target SingleAttack visual.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` calls `SkillMultiEffectExecutor.Execute(...)` from `SingleAttackSkillExecutor` and uses choice IDs from `SkillExecutionSnapshot` for `requires_active_choice_id` / `excludes_active_choice_id`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now uses `SkillMultiEffectCenterMode` for visual/damage centers and attaches applied-target status visuals with `InGameAttachedSkillEffectActor` when `visual_anchor_mode=AppliedTargets`.
- A PowerShell CSV reference check returned `OK effects=9 ariel_c=9` for the Ariel-C multi-effect rows, including choice references and prefab path checks. Unity-MCP `execute_menu_item` currently fails to find `Pakuri/Validate CSV Source Data`, so the final verification does not rely on that menu path.
- 2026-05-22 follow-up verification: `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_effects.csv` showed Ariel-C rows with `PrimarySkillCenter`, ally buff rows with `AppliedTargets`, and representative buff prefab rows using `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab`; runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User asked for Ariel-C to be implemented without hardcoding and with reusable CSV structure for similar future skills.
- 2026-05-22: User asked Code Builder to split Ariel-C effect application target from visual center/anchor, use ally-target buff visuals, keep Ariel-B shield/buff visuals attached to units, and use `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab` for Ariel-C buff visuals.

## Task: 2026-05-20 Ariel-A Master 2 Holy Exposure Runtime Wiring

### Task title

Route `ariel-a-master-2` through the shared on-hit status runtime and allow a choice-specific Holy damage taken bonus.

### Goals

- Make `ariel-a-master-2` apply `holy-exposure` through the existing projectile hit path.
- Let `ariel-a-master-2` supply its own Holy damage taken bonus instead of being forced to share one global `holy-exposure` status-row value.
- Keep the effect data-authored in `monster_skill_choices.csv` rather than adding Ariel-only executor logic.
- Reuse the current shared `StatusEffectKind.HolyExposure` parse/display path already present in the working tree.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- `ariel-a-trait-5` and `ariel-a-master-1` remain unsupported and were not changed in this task.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented at the CSV/runtime-data and shared projectile-status runtime level, and non-gameplay verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that `ariel-a-master-2` now applies 1 stack of Holy Exposure on hit and increases incoming Holy damage by 15%.
- If later Ariel-A still appears to miss the debuff in gameplay, inspect whether the active choice is actually recorded in the current `RunSession`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `ariel-a-master-2` `status_tag=holy-exposure`, `status_stacks_set=1`, `status_element_damage_taken_bonus=0.15`, and `runtime_support_state=ReferenceDirect`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -eq 'ariel-a-master-2' }` returned `status_tag : holy-exposure`, `status_stacks_set : 1`, `status_element_damage_taken_bonus : 0.15`, and `runtime_support_state : ReferenceDirect`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still has `ariel-a` `status_effect_id` blank and `status_chance=0`, so the master choice must provide the status tag itself.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `SkillChoiceEffectSpec.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` now carry the new choice field `status_element_damage_taken_bonus` through the shared projectile choice path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:191-241` now resolves a choice-provided `snapshot.StatusTag`, defaults new choice-only statuses to `chance=1f` and `stacks=1`, and clones the resolved `StatusEffectData` when a choice-specific `StatusElementDamageTakenBonus` override is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:115-117` and `:174-175` already contain the current working-tree `holy-exposure` / `?좎꽦 ?몄텧` parse and display strings used by the shared status runtime.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` returned `success:true` for this task; no new sync log line was captured afterward in the inspected console window.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; a first parallel editor build failed only from `Assembly-CSharp.dll` file lock contention, and a standalone `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` then passed with 0 errors. Existing MSB3277 warnings remained.

### History

- 2026-05-20: User asked Code Builder to apply the previously explained `holy-exposure` fix path for `ariel-a-master-2`.
- 2026-05-20: User then required per-skill Holy damage taken values, so Code Builder extended the shared projectile choice status path with a choice-level `status_element_damage_taken_bonus` override and set Ariel-A master 2 to `0.15`.

## Task: 2026-05-17 Ariel-A Common Projectile Runtime Connection

### Task title

Connect Ariel-A Judgement Light through the shared InGame projectile path.

### Goals

- Route `ariel-a` to the shared `ProjectileSkillExecutor` / `InGameProjectileActor` path.
- Use the user-authored `Assets/Prefab/Skill/Ariel/Airel_A.prefab` as the Ariel-A projectile visual.
- Record which Ariel-A reference behavior is covered by the common projectile path and which behavior remains unsupported.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- Current runtime source schema does not expose per-skill base pierce count or per-skill projectile speed, so Ariel-A base pierce `1` and projectile speed `17` are mapped explicitly in `InGameSkillDefinitionMapper` from `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`.
- The common projectile path covers the base straight projectile, damage, magazine, reload, shot interval, prefab instantiation, and pierce. It does not implement Ariel-A critical rolls, shielded-ally damage scaling, White Judgement last-shot explosions, or Guiding Light holy exposure.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Builder implementation completed and local non-gameplay checks passed. 2026-05-18 Ariel-A projectile speed and pierce are now owned by `monster_skills.csv` instead of skill-ID-specific mapper code. 2026-05-18 supported runtime status labels can now be edited directly in CSV when `status_effect_id` is blank.

### Next Actions

- User verifies in NewRunScene Play Mode that Ariel-A fires `Airel_A.prefab`, damages enemies, and pierces one extra target.
- Add data/source schema fields for per-skill projectile speed and base pierce if more skills need those values without skill-ID-specific mapper exceptions.
- Implement separate runtime support before claiming Ariel-A master effects or shielded-ally scaling are active.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `NewRunScene.unity` now own the Ariel-A prefab mapping; `monster_skills.csv` no longer stores a base `skill_effect_prefab_path` column.
- `Pakuri/Assets/CSVData/SkillData.csv` now includes the Ariel-A reference row with base damage `18`, spell coefficient `1`, magazine `7`, reload `4.6`, shot interval `0.36`, pierce `1`, and projectile speed `17`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now serializes `arielAProjectilePrefab` and resolves `"ariel-a"` to it.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` assigns `arielAProjectilePrefab` to `Assets/Prefab/Skill/Ariel/Airel_A.prefab` GUID `66fcb365022930d4681ad320e5fff520`.
- `Pakuri/Assets/Prefab/Skill/Ariel/Airel_A.prefab` now has trigger `BoxCollider2D` and `Pakuri.InGame.InGameProjectileActor`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now includes `Assets/Prefab/Skill/Ariel/Airel_A.prefab`.
- CSV check returned `UpperA=ariel-a`, `Pierce=1`, `Speed=17`, `SourcePrefab=Assets/Prefab/Skill/Ariel/Airel_A.prefab`, `SourceMagazine=7`, `SourceReload=4.6`, and `SourceShot=0.36`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings; an earlier parallel runtime build failed only from an `obj\Debug\Assembly-CSharp.dll` file lock, then passed when rerun alone.
- Unity-MCP refresh reached idle; console warning/error read showed only MCP client handler logs, not C# compile errors.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `ariel-a` `projectile_speed=17`, `pierce_count=1`, `status_chance=0`, and `status_effect_label=?놁쓬`; the CSV `range` column was removed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` no longer has `ResolveProjectileSpeed(...)` or `ResolveBasePierceCount(...)` Ariel-A special cases.
- `ariel-b` `base_damage` in `monster_skills.csv` is now `35`, matching `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps Ariel design-only labels such as `諛⑹뼱留?, `異뺣났`, and `?좎꽦 ?몄텧` with `status_chance=0`; if `ariel-a` is edited to `status_effect_label=媛먯쟾`, `status_chance=1`, and `pierce_count=999`, the mapper can resolve the label to the supported `shock` status.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` parses Korean runtime labels including `媛먯쟾`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` falls back from blank `status_effect_id` to parseable `status_effect_label`.
- `SyncCsvRuntimeCatalogs.bat` was added for Unity batchmode sync; when the project was already open, Unity batchmode rejected duplicate project open, then Unity-MCP invoked `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` and the console logged successful sync/validation.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-17: User asked Code Builder to implement Ariel-A using `Assets/Prefab/Skill/Ariel/Airel_A.prefab` and to report any information the blueprint alone could not provide.
- 2026-05-18: Code Builder moved Ariel-A projectile speed/pierce from mapper hardcoding into the skill CSV row and filled Ariel-B shield base from the reference document.
- 2026-05-18: Code Builder added status-label fallback and CSV runtime sync batch support so supported status edits in `monster_skills.csv` can be synced without code changes.

## Task: 2026-05-18 Ariel One-Shot Area Runtime Kind

### Task title

Route Ariel C/E through the new SingleAttack runtime kind.

### Goals

- Make Ariel C and Ariel E one-shot area attacks instead of sustained `AreaAttack` rows.
- Keep the existing CSV numeric values unchanged.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Ariel C/E apply one immediate area hit through the shared executor.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md` names Ariel C `異뺣났???뚮룞`.
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md` names Ariel E `?泥쒖궗??媛뺣┝`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `ariel-c runtime_kind=SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `ariel-e runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV rows 5 and 7 as one-shot area attack skills for the new `SingleAttack` type.

## Task: 2026-05-15 Ariel-B Phase4-C-0 Shield Effect Minimum Execution

### Task title

Connect Ariel-B to the first shared InGame attached effect actor path.

### Goals

- Add a reusable attached skill-effect actor that follows a target transform for a configured duration.
- Connect Ariel-B shield execution through the shared `ShieldSkillExecutor`.
- Use the user-authored `Assets/Prefab/Skill/Ariel/Ariel_B.prefab` as the current Ariel-B visual prefab.
- Keep shield resource mutation in `InGameCombatManager.GrantShield(...)`.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- This slice grants shield values and expires the visual actor only; timed shield resource expiry is not implemented here.
- `Assets/Prefab/Skill/Ariel/Airel_A.prefab` exists with the typo `Airel_A`, but `SkillData.csv` currently has no `ariel-a` row in the inspected minimum data set, so Ariel-A was not connected in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and compile/editor-refresh verified.

### Next Actions

- User verifies in Play Mode that Ariel-B shield visual appears on player units when Ariel-B is learned and cast.
- Add a timed shield resource-expiry system before declaring support-shield duration behavior complete.
- Add Ariel-A only after a matching skill data row and execution target are confirmed.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAttachedSkillEffectActor.cs`.
- `SkillExecutors.cs` now makes `ShieldSkillExecutor` call `GrantShield(...)` and instantiate a shield visual using `InGameAttachedSkillEffectActor`.
- `NewRunScene.unity` assigns `arielBShieldEffectPrefab` to `Assets/Prefab/Skill/Ariel/Ariel_B.prefab`.
- `Assets/Prefab/Skill/Ariel/Ariel_B.prefab` has `Pakuri.InGame.InGameAttachedSkillEffectActor`.
- `Pakuri/Assets/Legacy/Data/GameData/Monsters/ariel.asset` stores `ariel-b` `BaseDamage: 35`, matching the inspected `SkillData.csv` shield base value.
- Runtime and editor builds passed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed no C# compile errors.

### History

- 2026-05-15: User asked Code Builder to create the common projectile/effect actor component and connect Ariel-B minimum execution as the first Phase4-C subtask.

## Task: 2026-05-14 Ariel NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Ariel prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Ariel_Unit` through `NewRunSceneEntryManager`.
- Verify Ariel creates an exact `ariel` runtime model and initializes `MonsterUnitActor`.
- Make Ariel's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Ariel combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies Ariel selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Ariel_Unit.prefab` in `arielUnitPrefab`.
- Unity-MCP verification returned `ariel:prefab=Ariel_Unit|modelOk=True|model=ariel|actor=True|actorModel=True|hpText=HP 240/240|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Ariel_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.

## Task: 2026-05-14 Ariel CSVData Phase0-2 Seed Rows

### Task title

Record Ariel rows added to the new CSVData files.

### Goals

- Seed Ariel identity/stat data in `MonsterStat.csv` so the shield sample skill has an owner row.
- Seed Ariel-B Radiant Shield in `SkillData.csv`.
- Preserve the no-damage shield attribute distinction in CSV fields.

### Constraints

- Role Owner is Code Builder.
- No Ariel runtime behavior, prefab, scene, or Play Mode changes.
- `ariel-b` stores `skill_element` as Holy and `damage_element` as None because the inspected reference says the shield has no damage attribute.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData mapping should handle `damage_element=None` for non-damage support skills.
- Reconfirm Ariel base HP ownership before CSVData becomes the authoritative source because `ariel-tower.md` does not list HP.

### Evidence

- `Pakuri/Assets/CSVData/MonsterStat.csv` now contains the `ariel` row with current project stat values and source notes.
- `Pakuri/Assets/CSVData/SkillData.csv` now contains `ariel-b` as `ShieldSkillData`.
- `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md` provides shield 35, spell coefficient 1.4, duration 5.0, cooldown 9.0, all-allies targeting, and highest-value refresh.
- `Import-Csv Pakuri\Assets\CSVData\SkillData.csv` returned `ariel-b` with `damage_element` None and `shield_base` 35.

### History

- 2026-05-14: Code Builder added Ariel seed data as part of CSVData Phase0~2.

## Task: 2026-05-13 Ariel Battlefield Facade Registration

### Task title

Route Ariel battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Ariel skill behavior while replacing direct battlefield list registration writes.
- Keep Ariel projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel skills in Play Mode if needed.

### Evidence

- `CombatRuntimeArielSkills.cs:244` now calls `AddBattlefieldProjectile(...)`.
- `CombatRuntimeArielSkills.cs:335`, `:722`, and `:1036` now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Ariel battlefield object registration through facade methods.

## Task: 2026-05-10 Ariel Manifested Shield Expiry And Archangel Effect Fix

### Task title

Fix 2P-5P Ariel shield expiry on 1P and make Archangel Descent effect visible through the shared Ariel path.

### Goals

- Make shields granted to the selected 1P monster by Manifested Ariel B/E expire when their duration ends, even when the selected 1P monster is not Ariel.
- Make Ariel E `Archangel Descent` use an explicit battlefield-wide visual path for selected and Manifested Ariel casts.
- Explain the bug from inspected runtime code.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in RunScene Play Mode that Manifested Ariel shields on 1P disappear after their duration.
- User verifies selected and Manifested Ariel E show the battlefield-wide Archangel Descent effect.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:83` through `:88` decremented `unitShieldTimer` inside `UpdateArielSkillCooldowns()`, which only runs for the selected monster's Ariel runtime.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` now calls `UpdateSelectedUnitShieldTimer(Time.deltaTime)` from the common selected-unit combat update.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:86` now defines `UpdateSelectedUnitShieldTimer(...)`, clearing selected shield state and mirrored `selectedUnitRuntime` shield/Ariel fields when the timer expires.
- `CombatRuntimeArielSkills.cs:12` defines `ArielArchangelEffectDuration`; `:438` and `:693` call `CreateArielArchangelDescentEffect(skill)` for selected and unit-owned Ariel E casts.
- `CombatRuntimeArielSkills.cs:700` creates the battlefield-wide `ArchangelDescent` circle with stronger alpha/sorting and adds it to `skillEffects`.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:35` adds `ShieldAppliedFrame`; `:160` through `:163` skip manifested shield timer decay on the frame the shield was applied.
- Follow-up: `CombatRuntimeArielSkills.cs:28` adds `unitShieldAppliedFrame`; `:95` through `:98` skip selected 1P shield timer decay on the frame the shield was applied.
- Follow-up: `CombatRuntimeArielSkills.cs:831` and `:902` stamp selected and manifested shield application with `Time.frameCount`; `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:79` mirrors the selected shield frame into `selectedUnitRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\Skill\CombatRuntimeArielSkills.cs Pakuri\Assets\Scripts\Combat\Manager\CombatRuntimeProjectiles.cs` completed with only LF-to-CRLF warnings.
- Unity-MCP script refresh recovered to ready; console warning/error read returned only MCP client handler logs, not C# compile errors.
- Follow-up `git diff --check` over `CombatUnitRuntime.cs`, `CombatRuntimeArielSkills.cs`, and `CombatRuntimeParty.cs` completed with only LF-to-CRLF warnings; Unity-MCP console read returned only MCP client handler/timeout logs, not C# compile errors.

### History

- 2026-05-10: User reported Manifested 2P-5P Ariel shields remain on selected 1P after Ariel's shield duration ends, and Ariel E's effect is not visible.
- 2026-05-10: Code Builder moved selected-unit shield timer ticking out of selected-Ariel-only cooldown logic and routed Ariel E selected/unit casts through a dedicated battlefield visual helper.
- 2026-05-10: User reported 1P shield duration now appeared shorter than 2P-5P after Ariel shield casts; Builder aligned selected and manifested shield timers by skipping decay on the frame a shield is applied.

## Task: 2026-05-10 Ariel Unit Executor Migration And Team Shield

### Task title

Move Manifested Ariel A-E onto Ariel unit executor paths and make Ariel shield skills protect party units.

### Goals

- Dispatch Manifested Ariel skills through Ariel-specific `CombatUnitRuntime` logic before the generic manifested fallback.
- Keep Ariel A projectile damage, Holy Exposure, and White Judgement explosion source-aware for manifested Ariel.
- Make Ariel B `Radiant Shield` and Ariel E `Archangel Descent` apply shield state to selected 1P plus living manifested 2P-5P party units.
- Confirm the prior MainMenu-selected Ariel shield behavior against actual code and correct it.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds, Unity-MCP refresh, console check, and `git diff --check`.

### Next Actions

- User verifies selected Ariel B/E shields on 2P-5P teammates in RunScene Play Mode.
- User verifies Manifested Ariel A-E and Holy Exposure interactions in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before this change, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:516` used selected-only `unitShieldValue` in `ApplyArielUnitShield(...)`, `CombatRuntimeProjectiles.cs:455` applied manifested damage directly to HP, and `CombatRuntimeParty.cs:2034` passed `0f` as manifested shield value.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:42` now stores per-unit shield and Ariel blessing/sanctuary/Archangel shield state.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:637` dispatches `TryTickArielUnitSkill(...)` before generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:422` through `:681` implements Ariel unit A-E execution paths.
- `CombatRuntimeArielSkills.cs:808` applies Ariel team shields to selected plus manifested units; `:1300` handles Ariel unit projectile hits.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:464` through `:473` applies shield absorption to manifested unit damage before HP loss.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP script refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: User requested the Ariel unit executor migration from the remaining-work report and asked whether MainMenu-selected Ariel shield skills protect teammates.
- 2026-05-10: Code inspection confirmed selected Ariel shields did not protect manifested teammates before this pass; Builder added party shield state and Ariel unit executor dispatch.

## Task: 2026-05-21 Ariel-D SingleAttack Target Fix

### Task title

Fix Ariel-D strongest-enemy targeting after Mark-to-SingleAttack conversion.

### Goals

- Keep Ariel-D authored as a SingleAttack skill.
- Preserve `HighestHealth` as the first implementation of "strongest enemy".
- Prevent Ariel-D's zero radius from turning into all-enemy coverage.

### Constraints

- Role Owner is Code Builder.
- Party focus-target AI remains intentionally unimplemented per user instruction.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel-D only damages/applies the mark status to the current highest-HP enemy in Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Ariel-D row has `runtime_kind=SingleAttack`, `radius=0`, `target_selection=HighestHealth`, `status_effect_id=holy-exposure`, and `status_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_D.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now sets `single.Area.CoverAll` to false when `source.TargetSelection` is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` passes `coverAll` into `InGameZoneSkillActor.ApplyAreaTick(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` uses the single-target branch only when `!areaCoversAll && areaRadius <= 0f`.
- Runtime and Editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-21: User reported Ariel-D appeared to hit all targets. Builder traced the behavior to `SingleAttackData.Area.CoverAll = source.Radius <= 0f` and changed it to respect explicit `target_selection`.

## Source: boards\MON\SEIN_MONSTER.md

## Task: 2026-05-19 Sein-A Auto Fire Clarification And Effect Wiring

### Task title

Clarify why selected Sein-A appears idle on scene entry and restore the missing `EffectManager` prefab mapping.

### Goals

- Confirm from inspected runtime code whether selected `sein-a` is supposed to auto-fire on scene entry.
- Restore the missing `NewRunScene` `EffectManager` mapping for `sein-a`.
- Keep the result grounded in the current Scripts2 runtime and actual scene/prefab assets.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Do not claim Sein-specific attack logic is broken without code evidence.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

`sein-a` visual mapping was restored in `NewRunScene`. The inspected runtime still keeps selected 1P slot `A` on manual fire by default until `AutoBtn` enables `playerAutoSkillEnabled`.

### Next Actions

- User verifies in Play Mode that `AutoBtn` or held primary mouse input now shows the `Sein_A` projectile visual.
- If the user wants the selected 1P default `A` skill to auto-fire immediately on scene entry for all monsters, that is a separate global combat-policy change.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` routes the selected 1P slot `A` through `HandleSelectedPlayerPrimarySkillInput()` when `playerAutoSkillEnabled` is false and only auto-routes that skill after `EnablePlayerAutoSkillMode()`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10373` serializes `playerAutoSkillEnabled: 0`, so the default scene state keeps selected 1P `A` on manual fire until the user clicks `AutoBtn`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs` and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:14188` show `AutoBtn` exists and is wired to `InGameCombatManager.EnablePlayerAutoSkillMode()`.
- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab` exists in the repository and its `.meta` GUID is `256552cb82ec9c2499fc2e0e01d20dd2`.
- Before this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468` serialized `MonsterId: sein` with `SkillEffects: []`.
- After this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10471` serializes `MonsterId: sein`, `SkillId: sein-a`, and prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`.

### History

- 2026-05-19: User reported that Sein did not appear to attack in-game and noted the missing `EffectManager` Sein prefab assignment.
- 2026-05-19: Code Builder confirmed the missing scene mapping, restored the `sein-a` prefab entry, and recorded that selected 1P `A` remains manual by default unless `AutoBtn` enables auto fire.

## Task: 2026-05-14 Sein NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Sein prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Sein_Unit` through `NewRunSceneEntryManager`.
- Verify Sein creates an exact `sein` runtime model and initializes `MonsterUnitActor`.
- Make Sein's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Sein combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified. 2026-05-18 Sein active skill CSV rows were updated to the new skill-owned projectile/status schema. 2026-05-18 Sein design-only labels remain non-runtime statuses with `status_chance=0`.

### Next Actions

- User verifies Sein selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Sein_Unit.prefab` in `seinUnitPrefab`.
- Unity-MCP verification returned `sein:prefab=Sein_Unit|modelOk=True|model=sein|actor=True|actorModel=True|hpText=HP 210/210|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Sein_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `sein-a` `projectile_speed=18`, `pierce_count=1`, `magazine_capacity=8`, `reload_seconds=4.4`, and `shot_interval_seconds=0.32`, matching `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `sein-b` `projectile_speed=20`, `pierce_count=0`, `magazine_capacity=4`, `reload_seconds=6`, and `shot_interval_seconds=0.18`, matching `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` records `sein-d` label `珥덉뿴 吏?` and `sein-e` label `?붿뿼 ???媛먯냼`; these are design labels because the current `StatusEffectKind` enum does not include Sein-specific fire-resistance statuses.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps Sein design labels `珥덉뿴 吏?` and `?붿뿼 ???媛먯냼` with `status_chance=0`; runtime CSV validation rejects positive chance on unsupported status labels.
- Supported labels can still be introduced later through CSV because `StatusEffectKind.cs` and `InGameSkillDefinitionMapper.cs` now parse supported Korean labels from `status_effect_label` when `status_effect_id` is blank.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.
- 2026-05-18: Code Builder moved Sein projectile/status tuning into skill CSV rows using the reference documents for A/B projectile values and D/E status labels.
- 2026-05-18: Code Builder normalized Sein design-only status labels to chance 0 and added supported status-label fallback/CSV sync batch support.

## Task: 2026-05-13 Sein Battlefield Facade Registration

### Task title

Route Sein battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Sein skill behavior while replacing direct battlefield list registration writes.
- Keep Sein projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein skills in Play Mode if needed.

### Evidence

- `CombatRuntimeSeinSkills.cs:704`, `:757`, `:814`, and `:871` now call `AddBattlefieldProjectile(...)`.
- Sein skill-effect registrations now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Sein battlefield object registration through facade methods.

## Task: 2026-05-09 Sein Unit Executor Migration Resume

### Task title

Resume Sein unit executor migration for A-J skill behavior.

### Goals

- Route manifested Sein A-E learned active skills through a Sein-specific `CombatUnitRuntime` executor before the generic manifested fallback.
- Make manifested Sein A/B/C projectiles use Sein unit fire-damage, critical, heat, and Flame Barrage passive hooks from the source unit state.
- Make manifested Sein C/D/E effect ticks and delayed/residual effects read the source unit's F-J passive and Offering choices.
- Preserve the selected 1P Sein manual A input path.

### Constraints

- Role Owner is Code Builder after Designer handoff from `Pakuri/reference/Report/2026-05-09-sein-unit-executor-migration-design.md`.
- Do not run Unity Play Mode; user performs gameplay verification.
- Unity-MCP refresh could not run because no Unity Editor instance was connected.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds.

### Next Actions

- User verifies manifested Sein A pierce/heat, B magazine volley, C delayed explosion/path/residual, D superheated zone, E sky-line/ash zones, and F-J passive effects in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/Report/2026-05-09-sein-unit-executor-migration-design.md` existed before this resume and identified the missing Sein unit executor.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:625` dispatches `TryTickSeinUnitSkill(...)` before generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1048` lets `TryApplySeinUnitProjectileHit(...)` resolve manifested Sein projectile damage before generic damage.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeSeinSkills.cs:127` adds `TryTickSeinUnitSkill(...)`.
- `CombatRuntimeSeinSkills.cs:160`, `:211`, `:277`, `:301`, and `:369` add unit executor paths for Sein A/B/C/D/E.
- `CombatRuntimeSeinSkills.cs:1352` adds manifested Sein unit projectile-hit damage and A heat/master explosion handling.
- `CombatRuntimeSeinSkills.cs:2064` adds `HasSeinUnitPassive(...)` so F-J passive checks can read the unit state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\Manager\CombatRuntimeParty.cs Pakuri\Assets\Scripts\Combat\Skill\CombatRuntimeSeinSkills.cs` completed with exit code 0.
- Unity-MCP `refresh_unity` returned `No Unity Editor instances found`.

### History

- 2026-05-09: User reported the Sein unit executor migration had been interrupted and asked to resume the A-J migration from the report's remaining-work section.
- 2026-05-09: Code Builder resumed the migration, added Sein unit active/projectile/effect/passive hooks, and validated with local C# builds.
