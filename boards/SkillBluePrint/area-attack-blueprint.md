# AreaAttack Blueprint For InGame Skills

## Purpose

This document is the primary implementation contract for InGame `AreaAttack` skills.

The intended workflow is simple:

- the caller provides already parsed area-attack skill data
- Code Builder reads this blueprint first
- if the skill fits the common AreaAttack path, Builder implements it through the shared zone runtime
- if the skill does not fit the common AreaAttack path, Builder stops and asks the user

This blueprint is not a data-discovery guide.
It should let an AI understand when AreaAttack work is straightforward, what inputs are required, what common behavior already exists, and when work must stop for clarification.

## Core Rule

For AreaAttack implementation work, do not search CSV files, monster reference files, or old monster-specific code just to rediscover numbers or behavior.

The caller owns parsed input.
Code Builder owns runtime wiring.

If a required value or behavior decision is missing, Builder must stop and report the missing item instead of guessing.

## Builder Working Mode

When the user says something like:

- implement `eve-c`
- implement a new sustained area skill
- connect this parsed `AreaAttack` skill to runtime

Builder should assume this workflow:

1. the parsed skill values are already provided by the caller or task context
2. Builder does not re-open CSV files to find numbers
3. Builder uses the shared AreaAttack / zone runtime
4. Builder asks the user only when the requested behavior is outside the common AreaAttack contract

## What Builder May Read

Default mandatory markdown read set for AreaAttack implementation:

- `AGENTS.md`
- `MDTREE.md`
- `AGENTS_ROLE/COMMON.md`
- `AGENTS_ROLE/GAMEBULIDER.md`
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md`
- this blueprint

Conditional markdown reads only when explicitly justified:

- the relevant monster board when the user names a specific monster or the inspected failure path names it
- DATA or asset boards only when the user or inspected failure explicitly touches CSV, prefab, scene serialization, runtime catalog, or `EffectManager` wiring
- RUN boards only when the user or inspected failure explicitly touches `RunSession`, Offering, Menifest, or `NewRunScene` runtime ownership
- UI boards only when the user or inspected failure explicitly names UI objects, buttons, canvases, TMP, UXML, or USS

Do not read extra markdown files for AreaAttack work just to gather general background.

Allowed:

- this blueprint
- `AGENTS.md`
- `MDTREE.md`
- `AGENTS_ROLE/COMMON.md`
- `AGENTS_ROLE/GAMEBULIDER.md`
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md`
- the routed board files that are explicitly justified by the active request or inspected failure path
- the current runtime scripts that must be edited or compiled

Not allowed as value-discovery sources unless the user explicitly asks:

- `Pakuri/Assets/CSVdata/**/*.csv`
- `Pakuri/reference/2.Monster/**`
- `Pakuri/reference/5.enemy/**`
- old monster-specific implementations used only to infer behavior
- unrelated board markdown such as UI, RUN, DATA, OPS, or other monster boards when the request and inspected failure path do not explicitly touch those domains

Important:
Reading the current runtime scripts is still allowed.
Builder may inspect the current shared AreaAttack runtime to confirm where parsed values are wired.
That is different from searching old data files for missing numbers.

## Required Parsed Input

Builder should expect the caller to provide a parsed AreaAttack package.

Minimum required fields:

- `SkillId`
- `RuntimeKind`
- `BaseDamage`
- `DamageAttribute`
- `PowerStat`
- `PowerCoefficient`
- `Radius`
- `ActiveDurationSeconds`
- `TickIntervalSeconds`
- `CooldownSeconds`
- `TargetingMode`
- `CanManualAim`
- `CanAutoTarget`
- `CoverAll`
- `HasZonePrefab`

Optional but common fields:

- `OnTickStatusId`
- `OnTickStatusChance`
- `ChoiceModifierSpecs`
- `SkillEffectPrefabOverride`

If any required field is missing, Builder must stop and report it.

Not part of the current common AreaAttack input contract:

- `DeployDelaySeconds`
- `WarningSeconds`
- `ZoneAnchorMode`
- `ZoneMoveMode`
- `FollowTarget`
- `OnEnterEffect`
- `OnExitEffect`
- `PerTargetHitCooldownSeconds`
- `MagazineSize`
- `ReloadSeconds`
- `ProjectileSpawnSpec`
- `DroneSpec`
- `MarkedTargetSearchSpec`
- `ExecuteThresholdSpec`

Do not require those fields for normal AreaAttack work.
If a request truly depends on one of them, Builder should treat that as a special case and ask the user.

## Common AreaAttack Contract

The following behavior is considered normal shared AreaAttack work.
If the requested skill fits this list, Builder should implement it without asking extra design questions.

- one cast creates one zone actor or one routed fallback zone tick path
- immediate first tick on creation
- repeated tick damage at a fixed interval while duration remains
- circular area by parsed `Radius`
- battlefield-wide hit when parsed `CoverAll` is true
- nearest-target automatic center selection
- manual aim offset when the current input flow already supports it
- cooldown-based cast gating
- base damage plus one chosen stat coefficient
- damage attribute mapping
- prefab-based persistent visual spawn through the current `EffectManager` / skill effect path
- on-tick damage
- on-tick status application
- choice-driven damage, radius, duration, and tick-interval modifiers when the provided modifier data fits the current shared snapshot fields

In short:
If the skill is "data goes in, common ticking area comes out," Builder should proceed.

## Minimal Runtime Understanding

Builder does not need to rediscover the whole project.
It only needs to understand this minimal runtime contract:

- parsed active skill data becomes runtime skill data
- `AreaAttack` runtime kind maps to `ZoneSkillData`
- learned active skills become runtime instances through the shared runtime factory
- AreaAttack skills execute through `ZoneSkillExecutor`
- persistent area damage and repeated ticks happen in `InGameZoneSkillActor`
- shared damage and status application go through `InGameCombatManager`
- choice modifiers apply through `SkillExecutionSnapshot`
- prefab lookup uses the current effect/prefab binding path

Builder may confirm these current connection points in code, but should not turn the task into a broad code exploration.

Important:
The current runtime also maps `Field`, `Mark`, and `Execute` to `ZoneSkillData`.
This blueprint does not authorize treating those runtime kinds as normal AreaAttack work.
If the requested `RuntimeKind` is not exactly `AreaAttack`, Builder must stop or select the matching future blueprint.

## Common Mapping Responsibility

When a parsed AreaAttack is implemented, Builder should wire the provided values into the shared AreaAttack path in this shape:

- parsed identity -> runtime skill identity
- parsed damage values -> shared zone tick damage spec
- parsed cooldown -> shared runtime timing state
- parsed radius / duration / tick interval / cover-all flag -> shared zone timing and geometry
- parsed targeting flags -> shared auto/manual center behavior
- parsed status values -> shared on-tick status spec
- parsed prefab info -> shared persistent zone visual binding path
- parsed choice modifiers -> shared snapshot modifier path

The important rule is not the exact property names.
The important rule is:
do not invent new monster-only logic when the shared AreaAttack path already supports the requested behavior.

## Stop And Ask User Rule

Builder must stop and ask the user when the request contains behavior outside the common AreaAttack contract.

Stop-and-ask examples:

- delayed activation, warning zone, or ground telegraph before the first damaging tick
- moving, following, expanding, shrinking, or rotating zones
- target-attached zones
- projectile that deploys or carries a zone
- drone, turret, mine, trap, or install behavior
- magazine or reload behavior attached to the zone
- marked-target-only search or marked-target fanout
- execute thresholds or missing-health scaling
- on-enter or on-exit effects
- per-target persistent hit cooldown independent of the zone tick interval
- ally shield, heal, or buff bundled into the same AreaAttack
- special behavior for "last tick", "third tick", "first enemy entering", or similar sequence state

When this happens, Builder should not try a best guess.
Builder should stop with a short question describing exactly which unsupported behavior was requested.

## Preferred Builder Response Pattern

When the request is normal:

- say it fits the common AreaAttack path
- implement it through the shared AreaAttack runtime
- report which parsed fields were consumed

When the request is not normal:

- say it does not fit the common AreaAttack path
- name the unsupported behavior
- ask the user whether to:
  - define a one-off exception, or
  - design a reusable shared extension

## Example Interpretation

Example 1:
User says "Implement `eve-c`."

If the provided parsed values describe a normal fixed-position ticking area with common damage, duration, tick interval, radius, and status behavior, this blueprint should be enough for Builder to proceed through the shared AreaAttack path.

Example 2:
User says "Implement `eve-e`, and make it a drone field with magazine reload."

This blueprint is still enough for Builder to understand the situation, but Builder must stop and ask because drone and magazine behavior are not part of the current common AreaAttack contract.

Example 3:
User says "Implement `vega-d`, and hit all marked enemies wherever they are."

Builder must stop and ask because marked-target fanout is not common AreaAttack behavior.

## Builder Checklist

Before implementation:

1. Confirm the parsed input package exists.
2. Confirm `RuntimeKind` is exactly `AreaAttack`.
3. Confirm the request fits the common AreaAttack contract.
4. Stop immediately if the request contains unsupported special behavior.

During implementation:

1. Use the shared AreaAttack runtime.
2. Wire only the provided parsed values.
3. Keep behavior common unless the user approved an exception.
4. Avoid monster-only hardcoded branches unless the user explicitly approved a one-off rule.

After implementation:

1. compile
2. refresh Unity scripts if available
3. check console errors/warnings
4. report whether the implementation stayed inside the common AreaAttack path

## Required Builder Output

For each AreaAttack task, Builder should report:

- skill ID
- whether the request fit the common AreaAttack path
- which parsed fields were consumed
- whether any requested behavior forced a stop-and-ask decision
- which runtime scripts were edited
- compile and console verification results

## Verification Expected From Code Builder

For documentation-only changes:

- run a targeted markdown/file existence check
- do not run Play Mode

For code changes:

- run `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`
- run `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` when editor scripts or serialization may be affected
- refresh Unity scripts if Unity is available
- check console warnings/errors
- leave Play Mode gameplay verification to the user

## Final Designer Intent

This blueprint is intentionally opinionated.

It is designed so that:

- Builder does not waste time rediscovering AreaAttack numbers in CSV files
- Builder does not over-read unrelated scripts
- Builder uses the common AreaAttack runtime by default
- Builder stops and asks when a skill needs special behavior

That is the desired behavior.

