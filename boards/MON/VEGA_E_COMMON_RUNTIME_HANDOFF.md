# Vega E Common Runtime Code Builder Handoff

## Task title

Implement the shared runtime and CSV extensions required for `vega-e` to match its current authored intent without adding Vega-only hardcoded branches.

## Goals

- Keep `vega-e` on the shared `SingleAttack` base path.
- Preserve data ownership in active CSV wherever the behavior is data-shaped.
- Add only the shared logic needed for:
  - target selection by highest `name-mark` stacks
  - mark-stack-based damage scaling
  - mark consumption and consumption-driven follow-up behavior
  - target-status-conditional crit chance
  - kill-time redistribution of consumed `name-mark` stacks
- Reuse existing generic choice/runtime support where it already exists instead of extending code unnecessarily.

## Constraints

- Role Owner is Code Builder.
- Designer does not implement code or scene changes.
- Unity Play Mode gameplay verification remains user-owned.
- Do not solve this with Vega-only `if (skillId == "vega-e")` branches.
- Prefer shared runtime/data extensions over one-off executor logic.
- Before extending code, re-audit whether a behavior is already supported by current generic choice/runtime fields.

## Role Owner

Code Builder

## Status

Designer handoff created. Implementation not started.

## Selected track

- Designer implementation handoff
- Designer structure / ownership handoff
- Designer gameplay acceptance criteria

## Inspected evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` currently authors `vega-e` with:
  - `runtime_kind=SingleAttack`
  - `implementation_state=RuntimeImplemented`
  - `base_damage=55`
  - `attack_power_coefficient=2`
  - `target_selection=Nearest`
  - `critical_allowed=true`
  - empty prefab path
- The same row description says `vega-e` should deal heavy single-target damage proportional to the target's `name-mark` stacks.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:69-70` maps `SkillRuntimeKind.SingleAttack` into shared `SingleAttackData`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs:313-345` registers `SingleAttackSkillExecutor` and resolves chosen modifiers into a generic `SkillExecutionSnapshot`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:158-168` already applies generic `damage_multiplier` and `cooldown_multiplier`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:287-299` already applies generic crit bonus and kill cooldown refund modifiers.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:1455-1481` already handles kill-time cooldown reset/refund on shared `SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:118-122` maps `name-mark` and `이름표식 연계` to shared `StatusEffectKind.NameMark`.
- Current code search over `Pakuri/Assets/Scripts2/InGame` found no `vega-e` or `vega-e-` specific runtime branch.
- Current code search found no dedicated shared `NameMark` consumption or `consumed stack count` runtime contract in the inspected files.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` currently authors all `vega-e-*` choices as `runtime_support_state=DataOnlyUnsupported`.
- Those same choice rows currently state:
  - `vega-e-trait-1`: `기본 위력 +25%`
  - `vega-e-trait-2`: `표식 1스택당 추가 피해 +25%`
  - `vega-e-trait-3`: `쿨타임 -20%`
  - `vega-e-trait-4`: `이름표식 20스택 이상인 대상에게 치명타 확률 +35%`
  - `vega-e-trait-5`: `최종선고로 적을 처치하면 소모한 이름표식의 25%를 주변 적에게 분배`
  - `vega-e-master-1`: `표식 소모량 100%, 표식 1스택당 추가 피해 +80%`
  - `vega-e-master-2`: `기본 위력 -20%, 처치 시 최종선고 쿨타임 70% 반환`
- Some of those `DataOnlyUnsupported` notes are likely stale against current runtime:
  - trait 1 generic damage multiplier support already exists
  - trait 3 generic cooldown multiplier support already exists
  - master 2 kill cooldown refund support already exists on shared `SingleAttack`
- The harder remaining gaps are still not represented by current generic CSV/runtime evidence:
  - highest-mark target selection instead of `Nearest`
  - stack-proportional damage using target `name-mark`
  - consuming `name-mark` stacks and reusing the consumed amount later in the same cast/result
  - target-status-conditional crit chance bonus
  - kill-time redistribution based on consumed stacks

## Relevant files

- `Pakuri/Assets/CSVdata/source/monster_skills.csv`
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSystem.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillExecutionUtility.cs`

## Current behavior summary

- Base `vega-e` is already routed through shared `SingleAttack`.
- Current authored row still targets `Nearest`, not `highest name-mark target`.
- Generic damage/cooldown/kill-refund hooks already exist in snapshot/executor.
- Current inspected evidence does not show a shared contract for `consume target status stacks, remember consumed count, and use that count for later damage or redistribution`.

## Approved target behavior

- `vega-e` should stay on shared `SingleAttack`.
- Base cast should choose the intended marked target, not simply the nearest enemy.
- Base cast damage should scale from the target's `name-mark` stacks through shared reusable logic.
- If design requires stack consumption, the consumed count should be available to later same-cast logic through shared runtime state, not Vega-only globals.
- Trait 4 should grant crit chance only against targets meeting the `name-mark >= 20` condition.
- Trait 5 should redistribute a fraction of the consumed `name-mark` stacks to nearby enemies only when `vega-e` kills the target.
- Master 1 should increase consumed amount and per-stack damage through data-owned shared fields.
- Master 2 should use the current shared kill cooldown refund path if the authored behavior matches it.

## Expected implementation surface

### 1. Re-audit and re-author existing generic behaviors first

Before adding new runtime code, Code Builder should verify whether these can move to `RuntimeImplemented` by CSV re-authoring alone:

- `vega-e-trait-1` base damage increase
- `vega-e-trait-3` cooldown reduction
- `vega-e-master-2` kill cooldown refund

Designer expectation:

- if current generic fields already support the behavior, do not invent a new Vega-E-specific extension
- update stale `runtime_support_state` and row payloads instead

### 2. Shared target-selection rule for highest target status stacks

Add a shared targeting rule that can select one target by highest stacks of a required status.

Minimum reusable shape:

- `target_selection=HighestStatusStacks` or equivalent shared selector
- `target_selection_status_id`
- optional `target_selection_status_min_stacks`
- deterministic tie-breaker fallback when multiple targets have the same stacks

This should live in shared targeting logic, not in a Vega-only branch.

### 3. Shared status-stack-scaling damage rule

Add a shared way for `SingleAttack` damage to scale from target status stacks.

Minimum reusable shape:

- `damage_per_target_status_stack`
- `damage_per_target_status_stack_status_id`
- optional `damage_per_target_status_stack_cap`

This rule should read the target's current stacks at hit resolution time.

### 4. Shared status-stack consumption contract

Add a shared way for an active hit to consume some or all stacks of a target status and expose the consumed amount to later same-cast logic.

Minimum reusable shape:

- `consume_target_status_id`
- `consume_target_status_ratio` or `consume_target_status_stacks`
- consumed stack count stored in a shared execution/result context

This is the critical prerequisite for master 1 and trait 5 if their wording remains consumption-based.

### 5. Shared consumption-driven follow-up rules

Once consumed stack count exists in shared runtime state, add reusable rule shapes for:

- `additional_damage_per_consumed_stack`
- `redistribute_consumed_status_ratio_on_kill`
- `redistribute_consumed_status_id`
- nearby-target search radius / target count if redistribution needs them

This should stay generic enough for future consume-and-redistribute skills.

### 6. Shared target-status-conditional crit chance

Current inspected runtime shows generic conditional damage rules, but not inspected evidence for target-status-conditional crit chance.

Minimum reusable shape:

- `conditional_crit_chance_bonus`
- `conditional_crit_target_status_id`
- `conditional_crit_target_status_min_stacks`

This is the expected path for `vega-e-trait-4`.

## Recommended implementation order

1. Re-audit `vega-e-trait-1`, `vega-e-trait-3`, and `vega-e-master-2` against current generic CSV/runtime support.
2. Re-author those rows if no code extension is needed.
3. Add shared highest-status-stack target selection.
4. Add shared target-status-stack-based damage scaling.
5. Add shared target-status-stack consumption contract.
6. Add shared consumed-stack follow-up payload support.
7. Add shared target-status-conditional crit chance.
8. Re-author the remaining `vega-e` trait/master rows on those shared fields.

## Dependencies and responsibility boundaries

- Target selection belongs in shared targeting/resolution logic, not in monster-specific skill code.
- Damage scaling and consumed-stack-based damage belong in shared execution/snapshot/executor logic.
- Stack consumption and redistribution belong in shared status/runtime logic.
- Choice payload ownership should remain in active CSV when the behavior is data-shaped.
- If Builder finds that current `vega-e` design depends on data not present in active CSV authority, stop and ask before widening scope.

## Tuning and data ownership

- Base damage and attack coefficient remain on `monster_skills.csv`.
- Generic always-on modifiers should stay on `monster_skill_choices.csv`.
- Highest-stack target selection should be CSV owned if new selector fields are added.
- Per-stack damage, stack-consumption amount, and redistribution ratio should be CSV owned.
- Trait 4 condition and crit value should be CSV owned.
- If master 1 changes consumption amount and per-stack damage, both should be CSV owned.

## Edge cases and degenerate strategies

- No enemy has `name-mark`.
- Several enemies share the same highest `name-mark` stack count.
- Target dies before stack-consumption follow-up logic reads consumed amount.
- Target has fewer stacks than the authored consumption amount.
- Trait 5 redistribution when there are no nearby enemies.
- Redistribution when nearby enemies already have very high `name-mark`.
- Trait 4 crit bonus should not leak onto non-qualified targets in the same fight.
- Master 2 kill refund should not trigger on non-kill hits.

## Acceptance criteria

- `vega-e` remains on shared `SingleAttack`.
- Base cast selects the intended highest-mark target through shared logic, not `Nearest`.
- Base damage can scale from target `name-mark` stacks through shared logic.
- If authored, mark consumption happens through shared logic and the consumed amount is reusable in the same cast result.
- Trait 1, trait 3, and master 2 use existing shared logic when that is sufficient.
- Trait 4 grants crit chance only against valid `name-mark >= 20` targets.
- Trait 5 redistributes the intended fraction of consumed marks only on kill.
- No Vega-only `vega-e` executor branch is introduced.

## Verification expected from Code Builder

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`
- Unity `Pakuri/Validate CSV Source Data`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets`
- Unity console read with no new CSV parse or C# compile failures
- Inspected evidence that any new selector/consumption logic is shared, not Vega-only
- Explicit row evidence for which `vega-e` choices were solved by re-authoring only versus new shared runtime
- User Play Mode verification for:
  - target selection behavior
  - stack-scaled damage
  - mark consumption behavior
  - trait 5 redistribution
  - master 2 kill refund

## Related board files that must be updated by Code Builder

- `boards/MON/VEGA_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md` if shared status consumption or redistribution rules are added

## History

- 2026-05-30: User asked Designer which parts of `vega-e` still require shared runtime extension.
- 2026-05-30: Designer inspected current active CSV rows and shared runtime files, then produced this Code Builder handoff.
