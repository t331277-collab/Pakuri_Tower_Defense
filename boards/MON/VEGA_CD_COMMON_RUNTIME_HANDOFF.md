# Vega C And D Common Runtime Code Builder Handoff

## Task title

Implement shared runtime extensions so `vega-c` and `vega-d` match the current Vega reference design without adding Vega-only hardcoded branches.

## Goals

- Keep `vega-c` on the shared `Buff` base path, but extend shared runtime so an active buff can modify later outgoing skill behavior and later outgoing damage behavior.
- Move `vega-d` off the current `AreaAttack` row shape and onto a shared `SingleAttack` multi-deployment path, because the intended behavior is repeated one-shot slashes at many target positions, not one persistent or one-centered zone attack.
- Keep both skills data-owned in CSV and use the provided prefabs:
  - `Assets/Prefab/Skill/Vega/Vega_C.prefab`
  - `Assets/Prefab/Skill/Vega/Vega_D.prefab`
- Preserve reusable runtime ownership so future monsters can reuse the same extensions.

## Constraints

- Role Owner is Code Builder.
- Designer does not implement code or scene changes.
- Unity Play Mode gameplay verification remains user-owned.
- Do not solve this with Vega-only `if (skillId == "vega-c")` or `if (skillId == "vega-d")` branches in executors.
- Prefer shared runtime/data extensions over hidden helper skills unless a helper skill is the cleaner shared pattern.
- Keep tuning authority in CSV wherever the behavior is data-shaped.

## Role Owner

Code Builder

## Status

Designer handoff created. Implementation not started.

## Selected track

- Designer implementation handoff
- Designer structure / ownership handoff
- Designer gameplay acceptance criteria

## Inspected evidence

- `Pakuri/reference/2.Monster/vega/skill/c-extermination-permit.md` defines Vega C as a self buff with:
  - base self action speed and attack power increase
  - duration variants
  - buff-active bonuses that affect later attacks and later `vega-a` behavior
- `Pakuri/reference/2.Monster/vega/skill/d-black-ledger-release.md` defines Vega D as:
  - one-shot slash hits
  - one slash at each marked target position
  - overlap allowed
  - no magazine use
- User clarified that Vega D should be treated as `SingleAttack` semantics, not as one `AreaAttack` zone skill.
- `Pakuri/Assets/Prefab/Skill/Vega/Vega_C.prefab` and `Pakuri/Assets/Prefab/Skill/Vega/Vega_D.prefab` exist in the repository.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:35` currently authors `vega-c` as `runtime_kind=Buff`, `status_effect_id=slaughter-permit`, `status_target_scope=self`, `status_merge_policy=same_source_refresh`, `status_action_speed_bonus=0.25`, and `status_attack_power_bonus=0.2`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv:16` already contains `slaughter-permit` with base duration `6`, `action_speed_bonus_per_stack=0.25`, and `attack_power_bonus_per_stack=0.2`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:226-234` maps `runtime_kind=Buff` into `BuffSkillData`, attached status, and attached damage fields.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SupportSkillExecutors.cs:8-78` shows `BuffSkillExecutor` currently applies the buff status to targets and stops there, except for generic multi-effects on cast.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs:143` shows later cast cadence already reads `StatusEffectRuntime.ResolveActionSpeedMultiplier(Owner)`, so active self buff action speed already affects later casts.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillExecutionUtility.cs:181` and `:124` show later damage already reads source attack power and source outgoing damage multiplier at attack execution time.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:93-98` builds action speed and attack power bonuses into status data, but hard-resets `DamageBonusRate` to `0f`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:267-270` resolves outgoing damage bonus only from active status `DamageBonusRate`, and only through the shared status path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:351-354` already has a shared concept of source-status + target-status conditional status bonus.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs` and `SkillChoiceModifierRecord.cs` expose common choice modifiers like duration, cooldown, radius, reload, conditional damage, and status stack bonuses, but no CSV-loaded choice fields for attached buff `status_action_speed_bonus` or `status_attack_power_bonus`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` currently marks several Vega C choices as `DataOnlyUnsupported` or `PartialRuntimeSupport`:
  - `vega-c-trait-2`
  - `vega-c-trait-3`
  - `vega-c-trait-4`
  - `vega-c-trait-5`
  - `vega-c-master-1`
  - `vega-c-master-2`
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:36` currently authors `vega-d` as `runtime_kind=AreaAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ZoneSkillExecutor.cs:21-89` shows `AreaAttack` resolves one area center, then optionally repeats deployments from the ordered target list. It is still a zone-centered path, not a one-shot per-marked-target slash contract.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillAreaUtility.cs:26-31` resolves one primary center from nearest/manual target.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs:127-157` resolves targets only by side (`Enemy`, `Self`, `Ally`, `AllAllies`). It does not filter by required target status.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs:88-217` already supports shared multi-deployment `SingleAttack` and target-anchored repeated centers.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillDeploymentCenterUtility.cs:14-89` already supports resolving one deployment center per ordered target, but not status-filtered target selection.
- `boards/SkillBluePrint/single-attack-blueprint.md` explicitly lists `marked-target-only search or marked-target fanout` as outside the current common SingleAttack contract and says Builder must stop and ask unless the behavior is promoted to a shared extension.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs:430-438` and `:623-685` already support conditional per-target damage multiplier rules driven by `conditional_target_status_id` and `conditional_target_status_min_stacks`.
- `monster_skill_choices.csv` currently authors `vega-d-trait-4` only as plain `damage_multiplier=1.3`, so it does not yet match the reference condition `only on targets with 10+ name-mark stacks`.

## Relevant files

- `Pakuri/reference/2.Monster/vega/skill/c-extermination-permit.md`
- `Pakuri/reference/2.Monster/vega/skill/d-black-ledger-release.md`
- `Pakuri/Assets/Prefab/Skill/Vega/Vega_C.prefab`
- `Pakuri/Assets/Prefab/Skill/Vega/Vega_D.prefab`
- `Pakuri/Assets/CSVdata/source/monster_skills.csv`
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`
- `Pakuri/Assets/CSVdata/source/status_effects.csv`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Modifiers/SkillChoiceModifierRecord.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SupportSkillExecutors.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ZoneSkillExecutor.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillAreaUtility.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillDeploymentCenterUtility.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillTargetingUtility.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`
- `boards/SkillBluePrint/single-attack-blueprint.md`

## Current behavior summary

### Vega C

- Base self buff already works through shared `Buff` status application.
- Base action speed and attack power buff already affect later outgoing behavior through shared status resolution.
- Buff choice rows cannot currently override attached buff action-speed / attack-power numbers through the existing common choice CSV loader.
- Buff-active cross-skill behavior like `while slaughter-permit active, vega-a applies more name-mark` has no shared contract yet.

### Vega D

- Current authored row uses `AreaAttack`, which is mismatched with the intended one-shot repeated slash behavior.
- Shared `SingleAttack` already has the correct visual/one-shot attack shape and already supports multi-deployment.
- Shared target acquisition and deployment-center logic still cannot say `deploy once per enemy that has specific status`.

## Approved target behavior

### Vega C

- Keep base cast on shared `Buff`.
- Allow active `slaughter-permit` to modify later outgoing Vega actions through shared reusable runtime rules.
- Support at least these reusable rule shapes:
  - attached buff numeric override from choice data
  - while-source-has-status, specific skill reload modifier
  - while-source-has-status, outgoing hit adds extra status stacks
  - while-source-has-status and target-has-status, outgoing damage multiplier

### Vega D

- Re-author base Vega D onto shared `SingleAttack`, not `AreaAttack`.
- On cast, resolve all valid marked enemies and perform one one-shot slash deployment per matched enemy position.
- Preserve overlap if two marked enemies stand inside another slash radius.
- Use shared `SingleAttack` hit logic, prefab logic, on-hit status logic, and per-target conditional damage logic.
- Support optional extra repeated slashes per matched target for master-1 through a shared repeat/fanout extension, not a Vega-only branch.

## Expected implementation surface

### 1. Shared buff-choice override path for attached buff status values

Code Builder should add a shared way for `runtime_kind=Buff` choice data to override or add attached status scalar fields that are currently fixed on the base skill row.

Minimum needed for Vega C:

- attached buff action speed bonus override
- attached buff attack power bonus override
- attached buff duration bonus already exists and should remain on the current duration path

This extension must be reusable for other buff skills, not Vega-only.

### 2. Shared buff-active outgoing modifier path

Add a shared runtime contract so an active source status can grant later outgoing modifiers beyond plain action speed / attack power.

Minimum reusable shapes needed for Vega C:

- `required_source_status_id`
- optional `target_skill_id` or `target_skill_ids`
- optional `required_target_status_id`
- optional `required_target_status_min_stacks`
- `reload_time_multiplier`
- `additional_applied_status_id`
- `additional_applied_status_stacks`
- `conditional_damage_multiplier`

This is the preferred direction because Vega C trait 4, trait 5, and master 1 are all buff-active rules, not one-cast rules.

### 3. Re-route Vega D onto SingleAttack

Change Vega D base authored contract from `AreaAttack` to `SingleAttack`.

Designer intent:

- Vega D is not a zone-duration skill.
- Vega D is not a one-center area drop.
- Vega D is a collection of repeated one-shot slashes.

### 4. Shared status-filtered multi-deployment for SingleAttack

Extend shared `SingleAttack` target-center resolution so multi-deployment can use a filtered target list, not only the current ordered nearest/health list.

Minimum needed:

- resolve enemy targets by side
- filter to targets that have `required_target_status_id`
- optional `required_target_status_min_stacks`
- deploy one center per matched target transform
- preserve current fallback behavior when no filter is configured

This should live near shared target resolution or deployment-center resolution, not inside a Vega branch in `SingleAttackSkillExecutor`.

### 5. Shared per-target repeat support for fanout skills

For Vega D master 1, add reusable support for repeated deployments per resolved fanout target.

Minimum reusable shape:

- `repeat_count_per_target`
- `repeat_interval_seconds`
- `repeat_damage_multiplier`

If Builder decides this should reuse trigger rows instead of new base-skill fields, that is acceptable only if the shared runtime still stays reusable and the per-target location contract remains explicit.

## Recommended implementation order

1. Implement shared buff choice override for attached buff scalar values.
2. Implement shared buff-active outgoing modifier contract.
3. Re-author `vega-c` prefab path to `Assets/Prefab/Skill/Vega/Vega_C.prefab`.
4. Re-author `vega-d` base row from `AreaAttack` to `SingleAttack`.
5. Implement shared status-filtered SingleAttack multi-deployment.
6. Re-author `vega-d` prefab path to `Assets/Prefab/Skill/Vega/Vega_D.prefab`.
7. Re-map `vega-d-trait-4` onto the existing shared conditional per-target damage fields instead of plain `damage_multiplier=1.3`.
8. Add shared repeat support for `vega-d-master-1`.
9. Only after shared runtime works, finish the currently unsupported Vega C and Vega D choice rows.

## Tuning and data ownership

- Base buff duration, action speed, and attack power remain skill-row owned.
- Buff-active modifier rules should be CSV owned if Builder introduces new shared fields.
- Vega D required mark filter should be CSV owned, not hardcoded to `name-mark`.
- Vega D repeat count / repeat interval / repeat damage multiplier should be CSV owned.
- `vega-d-trait-4` condition should be represented through shared conditional-target-status fields, not a handwritten code exception.
- Prefab path authority remains on `monster_skills.csv` and/or any shared trigger/effect row the Builder chooses.

## Edge cases

- Vega C buff reapplied while already active.
- Vega C trait 4 reload bonus affecting only `vega-a`, not all Vega skills.
- Vega C trait 5 damage bonus applying only when source has `slaughter-permit` and target has `name-mark`.
- Vega C master 1 adding extra `name-mark` only on the intended outgoing skill path.
- Vega D cast with zero marked enemies.
- Vega D cast with many marked enemies.
- Vega D cast where several marked enemies are stacked close enough that slash areas overlap.
- Vega D master 1 repeated slashes at each target position without collapsing into one center.
- Vega D trait 4 conditional damage on some marked targets but not others in the same cast.

## Acceptance criteria

- `vega-c` base cast still uses shared `Buff` and applies self `slaughter-permit`.
- Vega C buff choices can change attached buff action-speed and attack-power numbers through shared logic.
- Vega C buff-active rules can modify later outgoing behavior without Vega-only executor branches.
- `vega-d` base row uses shared `SingleAttack`, not `AreaAttack`.
- Vega D one cast can spawn one slash deployment per matched marked enemy position.
- Vega D overlap remains possible when multiple marked targets are close together.
- Vega D trait 4 damages only valid 10+ mark targets with the bonus, not every target in the cast.
- Vega D master 1 can repeat slashes per matched target through shared logic.
- Both skills use the provided Vega C/D prefabs through data-owned prefab paths.

## Verification expected from Code Builder

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`
- Unity `Pakuri/Validate CSV Source Data`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets`
- Unity console read with no new CSV parse or C# compile failures
- Inspected evidence that Vega D no longer routes through `AreaAttack`
- Inspected evidence that status-filtered SingleAttack center resolution is shared, not Vega-only
- User Play Mode verification for:
  - Vega C buff-active follow-up behavior
  - Vega D marked-target fanout and overlap behavior

## Related board files that must be updated by Code Builder

- `boards/MON/VEGA_MONSTER.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`

## History

- 2026-05-30: User asked Designer to describe, in English markdown, how Code Builder should extend common logic for Vega C and Vega D.
- 2026-05-30: User clarified Vega D should be treated as SingleAttack-style repeated slashes, not as a zone-style area attack.
