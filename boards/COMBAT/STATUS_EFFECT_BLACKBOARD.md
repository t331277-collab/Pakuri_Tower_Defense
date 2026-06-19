## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/STATUS_EFFECT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/status history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current shared status runtime baseline and the resource-display rule still relevant to active work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-06-19 Ariel Effect Object Trigger Binding Handoff

### Task title

Document how Ariel A-J can move from pre-combined CSV rows to skill body plus small effect objects, trigger bindings, and conditional modifiers.

### Goals

- Preserve the current runtime evidence that Ariel already has base skill rows, effect rows, trigger rows, and a small normalized node start.
- Define a migration handoff that reduces Ariel C-style row explosion.
- Explain how the old structure remains compatibility input until parity is verified.

### Constraints

- Role Owner started as Designer and continued as Code Builder after user explicitly requested implementation.
- The implementation uses generic `monster_skill_nodes.csv` and `monster_skill_node_params.csv`; no specialized effect object CSV files were added in this pass.
- User resolved the six ambiguous design questions before implementation.
- Unity Play Mode parity remains user-owned.

### Role Owner

Designer / Code Builder

### Status

Code Builder pass implemented normalized node handler expansion, Ariel numeric choice node migration, and Ariel C blessing row-explosion reduction.

### Next Actions

- User Play Mode verifies Ariel C combinations, Ariel B shield events, Ariel E shield composition, and Ariel J post-E / Ariel-E-shield-only behavior.
- Code Reviewer pass is pending after the Phase 2-5 implementation.

### Evidence

- `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md` was created.
- `Pakuri/reference/2.Monster/ariel/skill/` contains the inspected Ariel A-J reference markdown files.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skills.csv` contains Ariel base rows `ariel-a` through `ariel-j`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_effects.csv` contains Ariel effect rows including pre-combined Ariel C blessing rows.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_triger.csv` contains Ariel trigger rows for last projectile, shield expire, shield absorb, and status expire behavior.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` owns current multi-effect execution for damage, status, and status-duration extension.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` owns current combat trigger dispatch.
- User answers recorded in the handoff: D trait 5 requires the attacker itself to have shield; J shield condition requires Ariel-E-generated shield; I holy exposure damage taken applies to all incoming damage while exposure exists; passives are always active; durations stay seconds; generic node CSVs are the storage path.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs` now registers reusable handlers including `CountStatusDamageMultiplier`, `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `DurationBonus`, `StatusActionSpeedBonus`, `StatusAilmentResistanceBonus`, `StatusConditionalDamageTakenBonus`, and `StatusElementDamageTakenBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now applies normalized choice nodes on the combat snapshot path and supports status-targeted action speed bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now resolves status snapshot overrides through `SkillStatusSpecUtility.ResolveStatusData(...)`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_nodes.csv` now contains Ariel choice-owned normalized node rows for migrated numeric modifiers and `ariel-c-trait-2-blessing-action-speed`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_effects.csv` has 9 Ariel C pre-combined blessing rows disabled as `MigratedToEffectBinding`; the base rows now compose with normalized choice nodes.
- Phase 2-5 added `ShieldAmountMultiplier` so shield amount choices can avoid reusing generic damage multipliers when damage and shield behavior diverge.
- `SkillMultiEffectExecutor.ResolveStatusEffectShieldAmount(...)` now receives the combat snapshot and applies the shield-specific multiplier to status-effect shield amounts.
- `StatusEffectRuntime.MatchesConditionStatus(...)` now supports an optional required source skill id for effect condition checks.
- `ariel-j-shielded-holy-damage` now uses `condition_status_source_skill_id=ariel-e-shield-base`, matching the source id stored by the Ariel E shield effect status.
- `monster_skill_triger.csv` now keeps A last-shot, B shield-expire, B shield-absorb, and D mark-expire trigger rows as explicit runtime trigger-binding compatibility rows.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings for `System.Net.Http` and `System.IO.Compression` remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged runtime catalog load and `InGame skill data validation passed with 0 warning(s).`

### History

- 2026-06-19: User asked how Ariel A-J would be decomposed into skill body, small effect objects, trigger bindings, and conditional modifiers, including runtime application and old-structure handling.
- 2026-06-19: User then requested Code Builder implementation and answered the ambiguity questions; Code Builder implemented the generic node handler expansion, migrated 28 Ariel numeric choice modifiers into normalized nodes, added the Ariel C trait2 targeted blessing node, and disabled 9 Ariel C pre-combined rows.
- 2026-06-19: User requested the remaining Phase 2-5 implementation; Code Builder added shield-specific multiplier support, E shield row reduction, J-owned post-E triggers, and source-specific effect conditions.

## Task: 2026-05-31 Nexus Exclusion From Skill And Status Targets

### Task title

Keep Nexus as a damageable enemy fallback target while excluding it from player skill, buff, shield, heal, and status target paths.

### Goals

- Preserve Nexus in the combat roster so enemies can attack it after monsters are gone.
- Prevent allied skills, buffs, shields, heals, status application, status-count targeting, and chained additional damage from selecting Nexus.
- Keep direct damage against Nexus allowed.

### Constraints

- Role Owner is Code Builder.
- Nexus remains registered as player-side `UnitRole.Nexus`; filtering happens in skill/status paths, not by removing it from the roster.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies in Play Mode that Nexus HP can still be damaged by enemies, but Monster buffs/skills no longer apply to Nexus.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs` now filters `UnitRole.Nexus` from resolved skill target lists, including `Self`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now filters `UnitRole.Nexus` from status-count target resolution.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` already guards Nexus from `GrantShield`, `SetShield`, `Heal`, `ApplyStatus`, `ApplyShieldStatus`, and `ExtendStatusDuration`, while `ApplyDamage` remains allowed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` already filters Nexus from all-allies cooldown target entries.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillOnHitAdditionalDamageUtility.cs` already skips Nexus as a chain target.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` returned `No Unity Editor instances found`, so Unity-side validation was not available in this session.

### History

- 2026-05-31: User reported Monster buffs appearing to apply to Nexus and clarified Nexus should only take damage, not be a skill/buff target.
- 2026-05-31: Code Builder verified Nexus is registered in the player roster for enemy fallback targeting, then tightened skill/status target filters instead of unregistering Nexus.

## Task: 2026-05-31 Shared Passive Aura, Runtime-Kind Filter, Burst Status Hook, And All-Allies Cooldown Refund For Vega F-J

### Task title

Extend the shared passive/status runtime so Vega F-J can stay on reusable common logic for burst-index mark bonus, owner-status-gated aura behavior, area-only passive modifiers/triggers, and teamwide cooldown refund.

### Goals

- Keep Vega passive work on shared data-driven runtime contracts instead of Vega-only branches.
- Let passive effects and passive triggers require a live owner status.
- Let status-based damage modifiers and trigger events filter by skill runtime kind such as `Area`.
- Let passive-triggered cooldown refund iterate allied skill runtimes, not only the owner's single target skill.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The extension stays on shared status/runtime/trigger paths.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Reuse the burst-status choice path before adding another projectile-only trigger event for “Nth projectile adds stacks” behavior.
- Reuse `required_source_status_id` and runtime-kind filters for future “while buff X is active” or “Area damage only” passives before adding monster-specific branches.
- Reuse `TargetSide=AllAllies` on cooldown/reload trigger actions when future support skills need teamwide refund behavior.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now exposes the shared fields `EventSkillRuntimeKinds`, `StatusConditionalIncomingSkillRuntimeKinds`, `StatusConditionalOutgoingSkillRuntimeKinds`, `HasBurstStatusProjectileIndex`, `BurstStatusProjectileIndex`, and `BurstStatusStacksBonus`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `PakuriCsvRuntimeData.StatusPayload.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse/map/validate the new burst-status, owner-status gate, and runtime-kind filter fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carries burst-status bonus data, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now applies `ResolveBurstStatusStacksBonus(...)` on projectile hit.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/InGamePassiveEffectRuntime.cs` and `SkillMultiEffectExecutor.cs` now honor owner live-status gates on passive effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now filters incoming/outgoing modifiers through `MatchesSkillRuntimeKinds(...)`, which is the shared status-side `Area` filter used by Vega-I debuffs.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now filters passive-trigger events through `trigger.EventSkillRuntimeKinds`, routes direct effect rows through `SkillMultiEffectExecutor.ExecuteDirect(...)`, and resolves multi-target cooldown/reload operations through `ResolveTargetRuntimes(...)`, including `TargetSide=AllAllies`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now uses the burst-status path on `vega-f-trait-3`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now uses owner-status-gated aura rows on `vega-h-*` and `Area`-only incoming-damage rows on `vega-d-i-area-vulnerability-*`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now uses `event_skill_runtime_kinds=Area` on `vega-i-area-cooldown-base` and `TargetSide=AllAllies` cooldown refund rows on `vega-j-cooldown-base` and `vega-j-cooldown-trait1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged a successful sync after the final effect-schema normalization and refresh.

### History

- 2026-05-31: Designer's Vega F-J handoff narrowed the remaining blockers to burst-index status bonus, source-status-gated aura, runtime-kind filter, and multi-unit cooldown refund.
- 2026-05-31: Code Builder implemented those shared runtime contracts and Skill Builder authored Vega F-J on that path.
- 2026-05-31: Final Unity validation passed after the effect CSV header/type rows were normalized to match the widened shared status schema.

## Task: 2026-05-31 Shared SingleAttack HitAllTargets Origin Fix For Status-Filtered Fanout

### Task title

Fix the shared `SingleAttack` prefab-hitbox origin rule so status-filtered fanout skills can stay target-centered even when they also hit all local targets.

### Goals

- Keep caster-anchored `HitAllTargets` behavior for skills that are intentionally self-origin slashes.
- Prevent `HitAllTargets` from overriding the resolved deployment center on status-filtered fanout skills such as Vega-D.
- Preserve the current shared deployment-center, overlap, and repeat logic without adding a new executor mode.

### Constraints

- Role Owner is Code Builder.
- This task modifies only the shared hitbox-origin guard in `SingleAttackSkillExecutor.cs`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor-validated.

### Next Actions

- Reuse the same `!UsesStatusFilteredDeployments(skill)` guard when another shared `SingleAttack` row combines `hit_target_count=global` with status-filtered multi-center deployment.
- If a future skill needs explicit caster-origin behavior even with status-filtered deployment, add a named shared flag instead of relying on the old implicit coupling.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now narrows `ResolvePrefabHitboxCenter(...)` so only non-status-filtered `HitAllTargets` skills snap the prefab origin back to the caster.
- The same executor still resolves status-filtered centers via `ResolveDeploymentCenters(...)`, still uses `UsePrefabHitbox`, and still applies overlap/repeat behavior on those centers.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` completed to the point that the console logged runtime catalog load plus sync without a new C# or CSV failure.

### History

- 2026-05-31: Vega-D exposed that the shared `HitAllTargets` origin rule was still assuming a caster-anchored slash even after Vega-D had been re-authored to use target-centered status-filtered fanout AoE.

## Task: 2026-05-31 Shared SingleAttack Overlapping Fanout Reuse For Vega-D

### Task title

Reuse the current shared status-filtered `SingleAttack` fanout path for overlapping local AoE hits and delayed repeats through data-only Vega-D row changes.

### Goals

- Keep one deployment center per status-matched target.
- Allow each deployment center to hit all enemies in its local area when the skill row authors `hit_target_count=global`.
- Reuse the existing shared per-target repeat scheduler for delayed extra slashes instead of adding a Vega-only coroutine path.

### Constraints

- Role Owner is Code Builder.
- No new runtime code was added for this task; the change was limited to authoring values already supported by the inspected shared executor and snapshot.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented through active CSV re-authoring and compile/editor validation.

### Next Actions

- Reuse `hit_target_count=global` on status-filtered `SingleAttack` rows when local overlap stacking is intended.
- Reuse `repeat_count_per_target` plus `repeat_interval_seconds` when a fanout slash should add delayed extra hits at each resolved deployment center.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` still resolves status-filtered deployment centers with `ResolveDeploymentCenters(...)`, computes local hit count with `ResolveEffectiveHitTargetCount(...)`, and schedules delayed repeats per center in `ScheduleRepeatedDeployments(...)`.
- The same executor computes repeat timing as `delaySeconds = snapshot.RepeatIntervalSeconds * repeatIndex`, which is why authored `repeat_count_per_target=2` plus `repeat_interval_seconds=0.5` yields `+0.5s` and `+1.0s` follow-up hits after the immediate base hit.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillAreaUtility.cs` and `SkillExecutionUtility.cs` still route radius multipliers into both collision radius and live prefab scale, so overlap and visual growth stay aligned on the shared path.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors Vega-D with `hit_target_count=global`, and `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-d-master-1` with `repeat_count_per_target=2` and `repeat_interval_seconds=0.5`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` completed to the point that the console logged runtime catalog load plus sync without a new C# or CSV failure.

### History

- 2026-05-31: Earlier same-day Vega-D row authoring had constrained local hits to single-target behavior.
- 2026-05-31: User later requested overlap-stacking AoE plus two delayed extra hits, and Code Builder confirmed the existing shared executor already supported those semantics through current CSV fields alone.

## Task: 2026-05-31 Shared SingleAttack Status-Filtered Fanout Single-Target Fix

### Task title

Split status-filtered `SingleAttack` fanout from line-style multi-deployment presentation so per-target repeated casts can remain single-target.

### Goals

- Preserve the shared deployment resolution that fans out across enemies carrying a required runtime status.
- Stop status-filtered fanout from inheriting the long line visual transform used by non-status multi-deployment prefab slashes.
- Restore authored hit-target-count handling for status-filtered fanout deployments.

### Constraints

- Role Owner is Code Builder.
- The existing shared runtime already supported status-filtered deployment centers, so this task stayed within current common logic instead of introducing a new shared deployment system.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor-validated.

### Next Actions

- Reuse the same split whenever another shared `SingleAttack` skill needs one cast per status-matched target without the line-style stretched visual treatment.
- If a future skill truly needs status-filtered fanout plus line-style stretching, author or add an explicit shared flag instead of relying on the old implicit `UseMultiDeployment` coupling.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` still couples `DeploymentRequiredTargetStatusId` to `UseMultiDeployment`, so shared executor handling remains the right place to separate visual semantics from deployment semantics.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now adds `UsesStatusFilteredDeployments(...)`, `UsesLineStyleMultiDeploymentVisual(...)`, and `ResolveEffectiveHitTargetCount(...)` so status-filtered fanout no longer automatically means line-style stretched visuals or unlimited hit count.
- The same executor still resolves one deployment center per status-matched target through `ResolveDeploymentCenters(...)`, so existing shared marked-target fanout behavior remains intact.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` completed to the point that the console logged runtime catalog load plus sync without a new C# or CSV failure.

### History

- 2026-05-31: Vega D exposed that the old shared `UseMultiDeployment` branch conflated three concerns: repeated deployment centers, line-style prefab presentation, and unlimited hit count.
- 2026-05-31: Code Builder split the status-filtered fanout path from the line-style branch while keeping the same shared deployment-center resolution.

## Task: 2026-05-31 Shared Target-Status Consumption And Redistribution Support For Vega E

### Task title

Extend the shared combat/status runtime so `SingleAttack` can scale from target status stacks, consume part of those stacks, and optionally redistribute consumed stacks on kill.

### Goals

- Keep Vega E mark interaction on shared runtime contracts rather than a Vega-only status branch.
- Support partial stack consumption on runtime statuses through the existing shared unit status store.
- Let shared `SingleAttack` resolve conditional crit and consumed-status redistribution from snapshot data.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The implementation stays on shared status storage, combat-manager helpers, and `SingleAttack` runtime paths.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Reuse the shared consume-stack helper path if another skill later needs partial status consumption instead of whole-status removal.
- Reuse the existing redistribution snapshot fields when another inspected skill needs explicit search radius/count authority instead of adding a parallel status spread system.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` now exposes shared `ConsumeStacks(...)` helpers on `UnitStatusRuntimeSet` and `UnitStatusRuntime`, which lets status stacks be reduced without clearing the whole status entry.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now wraps shared status-stack consumption through `ConsumeStatusStacks(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carries shared target-status-stack damage multipliers, consume overrides, conditional crit rules, and consumed-status redistribution fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves target-status-stack additive damage, consumes planned stacks on hit, and redistributes a portion of consumed stacks on kill when snapshot data requests it.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillStatusSpecUtility.cs` now creates a direct status spec for redistribution application without adding a Vega-specific status application path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` loaded the runtime catalog, and Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-31: Vega E required shared target-status-stack damage plus partial mark consumption, so Code Builder extended the shared status/combat path instead of hardcoding `name-mark` logic inside a Vega-only branch.
- 2026-05-31: Builder also added shared consumed-status redistribution support.
- 2026-05-31: User later supplied Vega-E trait-5 search radius `100` and target count `1`, and Skill Builder finished the active redistribution row on that existing shared path.

## Task: 2026-05-30 Shared Source-Status Modifier And Marked-Target Fanout Support For Vega C/D

### Task title

Extend shared status-aware combat runtime so buff-active choice modifiers and marked-target fanout can stay on reusable common paths.

### Goals

- Let choice rows require an active source status before they modify later outgoing skill behavior.
- Let attached buff status data receive shared choice-driven action-speed and attack-power scalar overrides.
- Let shared contact-target resolution filter targets by required runtime status and minimum stacks for marked-target fanout skills such as Vega D.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The runtime additions remain shared status/targeting behavior, not Vega-only executor branches.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Reuse `RequiredSourceStatusId` plus `RuntimeTargetSkillIds` when a future buff should change only specific later skills while the source buff is active.
- Reuse `DeploymentRequiredTargetStatusId` when a future `SingleAttack` or other resolved-deployment skill must fan out only across marked targets.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs` now checks `RequiredSourceStatusId` / minimum stacks before a choice spec is applied and now matches delimited `RuntimeTargetSkillIds`, which is the shared gate Vega C uses for buff-active trait/master behavior.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillStatusSpecUtility.cs` now clones attached status data with snapshot-provided `status_action_speed_bonus` and `status_attack_power_bonus` overrides, so buff status scalars no longer have to stay fixed on the base skill row only.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs` and `.../SkillExecutionUtility.cs` now expose shared target resolution filtered by required target status id and minimum stacks.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves one deployment center per matched target carrying the required status and supports repeat-per-target fanout through shared snapshot fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carries shared repeat-per-target values and attached-buff scalar override flags through the execution snapshot used by both Vega C and Vega D.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.

### History

- 2026-05-30: Vega C and Vega D implementation required the shared runtime to understand buff-active source-status gates, attached buff scalar overrides, and marked-target deployment fanout before the routed Vega rows could move out of `DataOnlyUnsupported` / mismatched runtime states.

## Task: 2026-05-28 Shared Silence Default Duration For Vega-B Threshold Refresh

### Task title

Align the shared `silence` base duration with Vega-B threshold silence refresh so the extra second can be authored without a duplicate status id.

### Goals

- Let Vega-B base silence remain `3s` while the master-2 threshold refresh lands at `4s`.
- Let trait-2 `+1s` stack naturally on both the base silence and the threshold refresh.
- Avoid creating a second silence status id only for Vega-B.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The shared status id `silence` was changed only after inspecting the active Vega-B CSV usage in the current routed skill-authoring scope.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- If another inspected skill later requires a different shared silence base, revisit whether `silence` should stay shared or whether that skill needs a distinct status id.

### Evidence

- `Pakuri/Assets/CSVdata/source/status_effects.csv` now sets `silence` default duration to `4`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors Vega-B base silence explicitly at `status_duration_seconds=3`, so the base hit stays at `3s`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now uses the shared threshold reapply path for `vega-b-master-2`, so the reapplied silence reads the shared default `4s`, and the same choice CSV applies `vega-b-trait-2` as `status_duration_bonus_status_id=silence` / `status_duration_bonus=1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` resolves status duration from explicit duration first and otherwise falls back to the shared status default, then adds snapshot duration bonuses for the matching status id.

### History

- 2026-05-28: Vega-B master-2 needed “Name Mark 10 stacks or more -> silence duration +1 second” on the shared threshold reapply path, which reads status defaults instead of the original base-skill explicit duration.

## Task: 2026-05-27 Zero-Damage Persistent Presence Zone Validation

### Task title

Keep shared presence-status zones valid when they intentionally deal no damage.

### Goals

- Preserve the `sein-d-superheated-presence` refresh path as a zero-damage persistent zone.
- Avoid adding fake damage to presence-only effect rows.
- Verify the shared CSV validator recognizes this status-only persistent-zone pattern.

### Constraints

- Role Owner is Code Builder.
- The runtime/status behavior stays shared; no Sein-only validator bypass was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- Reuse the same shared validation allowance for future persistent zones that exist only to refresh a status and intentionally deal zero damage.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now treats zero-damage `Damage` effects as valid only when they are persistent zones with status payloads and zero stat coefficients.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` keeps `sein-d-zone-presence` and `sein-e-master2-zone-presence` at `base_damage=0` while continuing to apply `sein-d-superheated-presence`.
- Unity menu `Pakuri/Validate CSV Source Data` completed and logged the runtime catalog load summary without the previous `requires positive base_damage` errors.

### History

- 2026-05-27: Shared validation originally forced positive base damage on all `Damage` effect rows, which incorrectly rejected presence-only persistent zones.

## Task: 2026-05-27 Sein-D Superheated Presence Shared Status

### Task title

Add a shared zone-presence status so Sein-E conditional damage can query whether a target is currently inside a Sein-D-style superheated zone.

### Goals

- Keep `Sein-E trait-5` on the existing conditional-target-status damage path.
- Avoid overloading `sein-d-heat-stack`, which represents repeated zone hits rather than current zone occupancy.
- Reuse shared persistent-zone multi-effects so both base Sein-D and Sein-E master-2 can refresh the same presence status.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The status store remains the shared `StatusEffectKind` / combat-manager status runtime; no parallel zone-presence registry was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that enemies inside base Sein-D and Sein-E master-2 zones keep the short-lived `sein-d-superheated-presence` status refreshed while they remain inside the area.
- User verifies that leaving the zone drops the status quickly enough for `Sein-E trait-5` damage to stop applying.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now defines `SeinDSuperheatedPresence`, parses id `sein-d-superheated-presence`, and returns a shared runtime definition with default duration `0.75s` and max stacks `1`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains `sein-d-superheated-presence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `sein-d-zone-presence` so base Sein-D can refresh that shared presence status through an `OnCast` persistent-zone companion effect.
- The same effect CSV now contains `sein-e-master2-zone-presence` so each Sein-E master-2 deployment center spawns a matching persistent presence zone through shared `OnDeploymentCast` routing.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gates `sein-e-trait-5` on `conditional_target_status_id=sein-d-superheated-presence` and `conditional_target_status_min_stacks=1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-E trait-5 required “currently inside superheated zone” semantics, so Builder added a new shared presence status instead of reusing the existing repeated-hit stack status.

## Task: 2026-05-27 Sein Projectile/Zone Conditional Status Additions

### Task title

Add shared runtime status identities for Sein-C trait-5 and Sein-D trait-5 conditional damage logic.

### Goals

- Keep Sein-C trait-5 on a shared conditional-target-status path instead of a hardcoded target-memory branch.
- Keep Sein-D trait-5 on a shared status-stack threshold path driven by the zone hit runtime.
- Route both statuses through the existing `StatusEffectKind` / shared combat-manager status store.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The status ids were added to shared status/runtime files and active CSV authority; no parallel status store was introduced.
- `sein-a-hit-mark` duration `5s` is inferred because the inspected request bundle did not provide an explicit duration.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-C trait-5 damage only increases against enemies recently hit by Sein-A.
- User verifies in Play Mode that Sein-D trait-5 only increases damage after the same target has accumulated at least 4 recent zone-hit stacks.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now defines `SeinAHitMark` and `SeinDHeatStack`, accepts ids `sein-a-hit-mark` and `sein-d-heat-stack` in `TryParse(...)`, and returns shared runtime definitions for both statuses.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains rows `sein-a-hit-mark` and `sein-d-heat-stack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now applies `sein-a-hit-mark` from Sein-A hits and `sein-d-heat-stack` from Sein-D zone ticks.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now gates `sein-c-trait-5` with `conditional_target_status_id=sein-a-hit-mark` and `conditional_target_status_min_stacks=1`, and gates `sein-d-trait-5` with `conditional_target_status_id=sein-d-heat-stack` and `conditional_target_status_min_stacks=4`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-C trait-5 and Sein-D trait-5 required reusable status-gated damage conditions, so new shared status identities were added instead of hardcoding those checks inside the skill runtime.

## Task: 2026-05-26 SingleAttack OnHit Status Effect Support

### Task title

Let shared SingleAttack hits apply choice-gated OnHit status effects for Rin-E master-2 slow.

### Goals

- Reuse `monster_skill_effects.csv` OnHit status rows for SingleAttack hit targets.
- Keep Rin-E master-2 slow on shared status application instead of a Rin-only branch.
- Preserve existing shared `SkillStatusApplyUtility` status application.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The change is scoped to SingleAttack hit targets and status-type OnHit effects.
- Unity Play Mode gameplay verification remains user-owned.
- Unity CSV runtime catalog sync is pending because batchmode reported another Unity instance has this project open.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Rin-E master 2 applies the 2-second, -25% move speed slow to each hit enemy.
- Sync runtime catalog assets once Unity project locking allows it.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now resolves `SkillMultiEffectTiming.OnHit` status effects with `SkillMultiEffectExecutor.ShouldRun(...)` and applies them to each SingleAttack hit target through `SkillStatusApplyUtility.TryApplyStatus(...)`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `rin-e-master2-slow` with `effect_timing=OnHit`, `status_effect_id=slow`, `status_duration_seconds=2`, and `status_move_speed_bonus=-0.25`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `cmd /c SyncCsvRuntimeCatalogs.bat` failed only because Unity batchmode reported another Unity instance has `C:/TowerDefence_Pakuri/Test/Pakuri` open.
- Follow-up enum validation found the `DamageAttribute` enum defines `Darkness`, not `Dark`; `rin-e-master2-slow.attribute` was corrected to `Darkness`, and a CSV enum scan returned `ENUM_VALIDATION_OK`.

### History

- 2026-05-26: Rin-E master-2 implementation required slow on each SingleAttack hit target, so SingleAttack adopted the existing shared OnHit status-effect pattern already used by beam/line runtime paths.
- 2026-05-26: User reported Unity auto-sync failing on `monster_skill_effects.csv` row 78 because `attribute=Dark` was not a valid enum value; Builder corrected the status-effect row to use `Darkness`.

## Task: 2026-05-24 Shared Passive Condition-Status And Trigger Expression Support

### Task title

Extend shared status/trigger runtime so passive effect rows and trigger rows can target expression-style condition statuses and shared proc-gated routed skills.

### Goals

- Let shared runtime parse condition-status expressions such as `chill;freeze` and `shock:5`.
- Let passive effect rows and trigger rows both consume the same condition-status matcher instead of duplicating string logic.
- Keep routed trigger validation aligned with actual runtime semantics so non-`SingleAttack` routed skills do not need fake damage payloads.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The implementation must stay on shared status parsing, trigger runtime, and CSV validation paths.
- Unity Play Mode gameplay verification remains user-owned.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, compile-verified, and Unity CSV validation passed.

### Next Actions

- Reuse the shared condition-status expression format for future passive or trigger work that needs OR lists or minimum stack gates before inventing another status-condition schema.
- Keep trigger damage-field validation scoped to `SingleAttack` unless a future routed trigger runtime begins consuming its own damage payload.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now defines shared condition-status parsing and matching helpers used by both target status checks and status-expire trigger checks.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `SkillTriggerRuntime.cs` now delegate condition-status checks to `StatusEffectRuntime`, and `SkillTriggerRuntime.cs` now supports multi-attribute trigger filters such as `Lightning;Ice`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now validates expression-style `condition_status_id` values and shared trigger-attribute lists, and now limits trigger damage payload validation to `runtime_kind=SingleAttack`.
- The validation follow-up was grounded by the failing Eve-G trigger rows `eve-g-auto-prism-ray` and `eve-g-auto-prism-ray-trait1`, which route `LineAttack` `eve-b` and therefore should not require synthetic `base_damage` values on the trigger row.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors after the shared runtime/validation change; only the existing `MSB3277` warnings remained.
- Unity `Pakuri/Validate CSV Source Data` then completed successfully and logged the runtime catalog load summary instead of `CsvFatalException`.

### History

- 2026-05-24: Eve F-J passive completion exposed that shared trigger validation was over-constraining routed non-`SingleAttack` triggers and that passive condition-status rows needed shared expression parsing.

## Task: 2026-05-26 Rin F-J Passive Status/Trigger Runtime Extensions

### Task title

Support Rin F-J passive status bonuses, hit-count effects, and trigger actions on shared combat runtime paths.

### Goals

- Let statuses grant outgoing critical damage bonus in the same modifier path as action speed, attack power, and critical chance.
- Let multi-effect rows run on `OnHitCount` for hit-count-gated passive effects.
- Let passive triggers filter by event skill, event source scope, trigger count, and status source skill before running a `SingleAttack`, effect, cooldown refund, or reload reduction action.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The extension is shared runtime behavior, not Rin-only branches.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile/CSV-validation verified.

### Next Actions

- Use `StatusEffectRuntime.ResolveCriticalDamageBonus(...)` for future outgoing critical damage status bonuses.
- Use `SkillMultiEffectTiming.OnHitCount` plus `condition_hit_count_min` for future hit-count-gated passive effects.
- Keep count gate evaluation before proc/internal cooldown consumption for `trigger_every_count` trigger rows.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` now includes `BuffModifierSpec.CritDamageBonusRate`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now initializes, measures, and resolves critical-damage status bonuses through `ResolveCriticalDamageBonus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` adds resolved status critical damage to outgoing critical damage calculation and stores passive trigger counts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now supports `ExecuteOnHitCount(...)`, health-ratio target conditions, hit-count conditions, and status critical damage bonus mapping.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now supports `SkillTriggerActionKind` actions, event-applied damage source, delayed triggers, event skill filters, event source scope filters, condition status source skill filters, count gates, cooldown refund, and reload reduction.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged the runtime catalog load summary and did not log a Pakuri CSV validation failure.

### History

- 2026-05-26: User approved extending shared runtime support for Rin F-J, including all-allied physical damage counts and reusable trigger/effect structures.

## Task: 2026-05-17 InGame Shared Status Runtime Baseline

### Task title

Keep the current Scripts2 status runtime grounded in `StatusEffectKind` and the shared unit-status store.

### Goals

- Keep all new status work routed through `StatusEffectKind` instead of ad hoc strings.
- Keep status storage, ticking, apply/remove/query, and label refresh owned by shared runtime code.
- Keep Eve-A shock application on the shared projectile hit path.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older status-effect slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active status runtime baseline summarized and retained for future work. 2026-05-18 Code Builder refactor keeps status labels on the actor path while centralizing shared actor presentation in `UnitActorView`. 2026-05-18 projectile/status tuning now reads status chance and label from `monster_skills.csv`; supported runtime labels can now be used as a fallback when `status_effect_id` is blank.

### Next Actions

- Future skills should apply statuses only through `InGameCombatManager.ApplyStatus(...)`.
- Later passive/resistance/damage work should query `StatusEffectKind`-based runtime state rather than adding parallel status storage.
- Use the archive snapshot when older shield/freeze/temporary-effect details are needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` defines the shared enum and central status display helpers.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` owns the current unit status store and ticking behavior.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` owns status apply/remove/query plus actor refresh on state changes.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` delegate active status label presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `InGameProjectileActor.cs` currently route Eve-A shock through the shared projectile hit path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now contains `status_chance` and `status_effect_label` per skill; Eve-A stores `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=媛먯쟾`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` passes CSV `StatusChance` into `StatusApplicationSpec.Chance`; `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains the Eve-A shock chance special case.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels `媛먯쟾`, `異붿쐞`, `?됯린`, `鍮숆껐`, `?뷀솕`, `痍⑥빟`, and `諛⑹뼱留? in addition to the canonical ids.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now resolves blank `status_effect_id` from a parseable `status_effect_label` and stores the canonical status tag from `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` rejects positive `status_chance` values on unsupported runtime status labels/ids.
- Unsupported design labels such as `移⑤У`, `?대쫫?쒖떇`, `?좎꽦 ?몄텧`, `?붿뿼 ???媛먯냼`, `?됰룞?띾룄 利앷?`, and `?됰갚` remain label-only in `monster_skills.csv` with `status_chance=0` unless a matching `StatusEffectKind` is added later.

### History

- 2026-05-17: Shared status runtime, enum centralization, label suffix display, and Eve-A shock application became the active baseline.
- 2026-05-18: Code Builder commonized `MonsterUnitActor`/`EnemyUnitActor` display refresh through `UnitActorView.cs`.
- 2026-05-18: Code Builder moved status chance/label authority from monster-level rows and hardcoded Eve-A executor logic into per-skill CSV rows.
- 2026-05-18: Code Builder made supported Korean status labels parseable from CSV, added validation for unsupported positive `status_chance`, and normalized design-only labels to chance 0.
