# Shield And Buff Status Unification Blueprint

## Purpose

This document is the implementation handoff for converting timed ally `Shield` / `Buff` skills onto one source-aware runtime status model while preserving shield-specific absorb behavior.

Code Builder should use this file as the primary contract for the requested work.

## Goal

- Make `runtime_kind == Buff` use CSV-owned duration, target scope, and source-aware merge rules.
- Move `runtime_kind == Shield` off the current direct `CurrentShield += amount` path and into a timed runtime instance model that still preserves shield amount absorption.
- Ensure same-skill recast refreshes instead of stacking.
- Ensure different skills with the same status kind may coexist and stack as separate runtime instances.
- Remove shield-duration hardcoding from runtime execution.

## Selected Track

- Designer implementation handoff
- Designer structure / ownership handoff

## Inspected Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:4` defines `ariel-b` as `runtime_kind=Shield` with `status_effect_label=방어막` and `status_duration_seconds=5`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:14` defines `rin-b` as `runtime_kind=Buff` with `status_effect_id=action-speed-up`, `status_duration_seconds=6`, `status_max_stacks=1`, `status_stack_amount=1`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:35` defines `vega-c` as `runtime_kind=Buff` with `status_effect_id=slaughter-permit`, `status_duration_seconds=6`, `status_max_stacks=1`, `status_stack_amount=1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:169-178` already maps buff duration from `StatusDurationSeconds`, but `BuffSkillExecutor` does not rely on `BuffDuration` as the authoritative runtime value.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs:182-189` maps shield base/coefficient/refresh rule but does not map `ShieldDuration`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:840-845` falls back shield duration to hardcoded `5f`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:862` applies shield through `GrantShield(target.Model, shield)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:514-535` shows `GrantShield` only adds to `CurrentShield` and stores no duration, source skill, or refresh identity.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAttachedSkillEffectActor.cs:12-25` shows attached effect visuals only follow the passed lifetime; they do not own gameplay state.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:97-129` shows timed statuses are currently keyed only by `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:114` uses `Find(kind)`, which prevents two different source skills from holding the same status kind independently.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:235-271` shows `StatusEffectData` has duration/stack/modifier fields but no mutable shield-remaining payload.
- `Pakuri/Assets/CSVdata/source/status_effects.csv:10` already contains a shield-like row under `holy-shield`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:102-108` already parses both `shield` and `holy-shield`.

## Current Behavior

- Buff duration is already sourced from `status_duration_seconds` through `StatusEffectRuntime.CreateStatusData(...)`.
- Buff status application is timed, but same-kind storage is global by kind, not by source skill.
- Shield visuals time out, but actual shield value remains until consumed by damage because shield state is a raw resource mutation, not a timed runtime status instance.
- Shield recast stacks by raw addition because `GrantShield(...)` always adds and stores no merge identity.

## Approved Target Behavior

### Buff

- Buff target scope must come from `monster_skills.csv`, not from code-only defaults.
- Same status kind from the same `skill_id` must refresh instead of stack.
- Same status kind from different `skill_id` values may coexist as separate instances.
- For same-skill refresh, the stronger active value wins when the skill defines a magnitude-bearing modifier.
- Recast must refresh duration when the merge rule says refresh.

### Shield

- Shield must behave like a timed ally status in ownership and refresh semantics.
- Shield must still preserve mutable absorb amount per applied instance.
- Same `skill_id` shield recast must refresh duration and keep the higher remaining or newly applied shield amount.
- Different `skill_id` shield instances may coexist.
- Expired shield instances must be removed even if they still have remaining absorb amount.
- Visual lifetime must match the gameplay lifetime sourced from CSV/runtime data, not a hardcoded fallback.

## Constraints

- Do not keep `Shield` on the raw `GrantShield += amount`-only path.
- Do not implement the new rule by broad special-casing individual monster skill IDs.
- Do not rely on Unity Play Mode as proof; Builder verification stops at compile, inspected code paths, and local deterministic checks.
- Preserve backward parsing compatibility for existing shield labels/ids during migration.
- Keep tuning authority in CSV where the requested behavior is data-shaped.

## Relevant Files

- `Pakuri/Assets/CSVdata/source/monster_skills.csv`
- `Pakuri/Assets/CSVdata/source/status_effects.csv`
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs`
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAttachedSkillEffectActor.cs`

## Expected Implementation Surface

### 1. CSV Schema

Add skill-row fields in `monster_skills.csv` and the matching runtime data pipeline:

- `status_target_scope`
  - initial supported values: `self`, `all_allies`
- `status_merge_policy`
  - initial supported values: `same_source_take_highest`, `same_source_refresh`, `always_stack`
- `shield_amount_refresh_policy`
  - initial supported values: `take_highest`, `replace`, `stack`
  - this field is only meaningful for shield-like statuses

Builder may choose exact field names only if they remain explicit and do not overlap existing semantics. If names differ, they must be documented in the board updates.

### 2. Status Identity Model

Replace the current status lookup model that keys only by `StatusEffectKind`.

The runtime status instance identity must support:

- status kind / canonical status id
- source skill id
- merge policy

Minimum acceptable runtime behavior:

- same kind + same source skill -> merge by the requested rule
- same kind + different source skill -> keep separate active instances

Query helpers such as total stacks, movement modifier sum, attack power sum, and incoming damage modifier sum must aggregate across all active instances of the same kind.

### 3. Shield Runtime Payload

Do not force shield amount into the current immutable `StatusEffectData` shape alone.

Shield runtime instances must additionally track mutable state such as:

- `SourceSkillId`
- `AppliedShieldAmount`
- `RemainingShieldAmount`
- `DurationRemaining`
- `RefreshPolicy`

Builder may add a dedicated runtime subclass/companion payload if that is cleaner than overloading the generic status data object.

### 4. Combat Ownership

Damage resolution must consume active shield runtime instances first, then derive `CurrentShield` from the sum of remaining active shield instances.

This means:

- direct `GrantShield(...)` should no longer be the authoritative timed shield path for player skills
- `CurrentShield` should become a derived or synchronized view of active shield instances, not the sole source of truth

### 5. Mapper / Executor Ownership

- Buff mapper must read the new target / merge data from CSV.
- Shield mapper must read duration from `status_duration_seconds` first.
- Shield executor must stop using the hardcoded `5f` fallback.
- Attached VFX lifetime must use the resolved runtime duration that also governs gameplay expiration.

### 6. Canonical Shield Status Id

Current code accepts both `shield` and `holy-shield`.

Builder should normalize to one canonical data id for new CSV content.

Preferred direction:

- source CSV may use `shield`
- parser keeps `holy-shield` as a legacy alias for compatibility
- `status_effects.csv` should not keep two competing canonical shield rows

If Builder keeps `holy-shield` as canonical instead, the compatibility and reason must be recorded in the related board updates.

## Tuning And Data Ownership

- `status_duration_seconds` remains the authoritative duration for timed buff/shield status instances.
- target scope must be data-owned in `monster_skills.csv`.
- merge policy must be data-owned in `monster_skills.csv`.
- same-skill magnitude conflict resolution must be data-owned where it is not globally fixed.
- shared display label / category / action-rule defaults remain in `status_effects.csv`.
- per-skill shield strength remains on the skill row through the existing base/coefficient fields.

## Dependencies And Responsibility Boundaries

- CSV parsing/build pipeline owns schema ingestion.
- mapper owns conversion from skill row -> runtime skill data / status application contract.
- runtime unit status store owns source-aware merge identity.
- combat manager owns shield consumption order and actor refresh.
- attached effect actor remains presentation-only.

## Edge Cases

- same skill recast with a weaker shield while a stronger same-skill shield is active
- same skill recast with a stronger shield while a weaker same-skill shield is active
- different skills applying the same status kind with different magnitudes
- shield expiration while unconsumed absorb amount remains
- shield partially consumed, then same skill recast before expiration
- max-stack 1 buffs that are reapplied rapidly before expiration
- status rows with blank `status_effect_id` but parseable `status_effect_label`
- legacy `holy-shield` rows or scenes still pointing at the old id

## Degenerate Strategies To Prevent

- unlimited permanent shield accumulation through cooldown recast
- losing all shield duration logic while only VFX expire
- collapsing different-skill buffs of the same kind into one instance because storage still keys only by kind
- reintroducing behavior through hardcoded monster skill ID branches

## Acceptance Criteria

- `Shield` skill applications expire by runtime duration even when no damage is taken.
- `Shield` VFX lifetime matches the gameplay lifetime.
- same `skill_id` shield recast refreshes instead of raw-add stacking.
- different `skill_id` shield instances may coexist.
- `Buff` target scope is sourced from CSV for at least `self` and `all_allies`.
- same kind, same source skill buff refreshes by the requested merge policy.
- same kind, different source skill buffs can coexist and aggregate.
- no authoritative player-skill shield duration remains hardcoded to `5f`.

## Verification Expected From Code Builder

- compile/build success for `Assembly-CSharp` and `Assembly-CSharp-Editor`
- inspected evidence that new CSV fields parse into runtime definitions
- inspected evidence that shield executor no longer falls back to hardcoded `5f`
- inspected evidence that same-kind status storage is no longer keyed only by `StatusEffectKind`
- inspected evidence that shield consumption uses active timed shield instances rather than only `CurrentShield += amount`
- if practical without Play Mode, a small deterministic runtime/unit-level test or editor-side proof for:
  - same-skill shield refresh
  - different-skill same-kind coexistence
  - timed shield expiration

## Related Board Files That Must Be Updated

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md` only if runtime catalog / asset wiring changes

## Implementation Outcome

- Implemented on 2026-05-20 by Code Builder.
- Runtime shield ownership now goes through `InGameCombatManager.ApplyShieldStatus(...)` plus `UnitStatusRuntimeSet` mutable shield instances instead of relying on player-skill `GrantShield += amount` authority alone.
- Buff/shield source-aware identity, target scope, merge policy, and shield refresh policy were added through `monster_skills.csv`, runtime parse/build, mapper, and validation.
- Canonical new shield data now uses `status_effect_id=shield` while parser compatibility for legacy `holy-shield` input remains in code.
- Verification completed through:
  - `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`
  - `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`
  - Unity editor deterministic execution result `shieldAfterDamage=6;healthAfterDamage=100;sameSourceShieldCount=1;sameSourceShieldRemaining=8;differentSourceShieldCount=2;totalShieldAfterDifferentSource=15;sameSourceBuffCount=1;differentSourceBuffCount=2;totalBuffStacks=2;expiredShieldCount=0;shieldAfterExpire=0`
  - Unity menu sync log `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Reviewer execution was not run because explicit user permission was not provided in this task.
