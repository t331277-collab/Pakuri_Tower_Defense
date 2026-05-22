# SingleAttack Blueprint For InGame Skills

## Purpose

This document is the primary implementation contract for InGame `SingleAttack` skills.

The intended workflow is simple:

- the caller provides already parsed single-attack skill data
- Code Builder reads this blueprint first
- if the skill fits the common SingleAttack path, Builder implements it through the shared one-shot area runtime
- if the skill does not fit the common SingleAttack path, Builder stops and asks the user

This blueprint is not a data-discovery guide.
It should let an AI understand when SingleAttack work is straightforward, what inputs are required, what common behavior already exists, and when work must stop for clarification.

## Core Rule

For SingleAttack implementation work, do not search CSV files, monster reference files, or old monster-specific code just to rediscover numbers or behavior.

The caller owns parsed input.
Code Builder owns runtime wiring.

If a required value or behavior decision is missing, Builder must stop and report the missing item instead of guessing.

## Builder Working Mode

When the user says something like:

- implement `rin-e`
- implement a new one-shot area skill
- connect this parsed `SingleAttack` skill to runtime

Builder should assume this workflow:

1. the parsed skill values are already provided by the caller or task context
2. Builder does not re-open CSV files to find numbers
3. Builder uses the shared SingleAttack / area-tick runtime
4. Builder asks the user only when the requested behavior is outside the common SingleAttack contract

## What Builder May Read

Default mandatory markdown read set for SingleAttack implementation:

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

Do not read extra markdown files for SingleAttack work just to gather general background.

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
Builder may inspect the current shared SingleAttack runtime to confirm where parsed values are wired.
That is different from searching old data files for missing numbers.

## Required Parsed Input

Builder should expect the caller to provide a parsed SingleAttack package.

Minimum required fields:

- `SkillId`
- `RuntimeKind`
- `BaseDamage`
- `DamageAttribute`
- `PowerStat`
- `PowerCoefficient`
- `Radius`
- `CooldownSeconds`
- `TargetingMode`
- `CanManualAim`
- `CanAutoTarget`
- `CoverAll`
- `HasSkillEffectPrefab`

Optional but common fields:

- `OnHitStatusId`
- `OnHitStatusChance`
- `ChoiceModifierSpecs`
- `SkillEffectPrefabOverride`

If any required field is missing, Builder must stop and report it.

Not part of the current common SingleAttack input contract:

- `ActiveDurationSeconds`
- `TickIntervalSeconds`
- `DeployDelaySeconds`
- `WarningSeconds`
- `RepeatCount`
- `PerTargetHitCooldownSeconds`
- `ExplosionChainSpec`
- `ConditionalDamageSpec`
- `MarkedTargetSearchSpec`
- `AllyEffectSpec`

Do not require those fields for normal SingleAttack work.
If a request truly depends on one of them, Builder should treat that as a special case and ask the user.

## Common SingleAttack Contract

The following behavior is considered normal shared SingleAttack work.
If the requested skill fits this list, Builder should implement it without asking extra design questions.

- prefab-authored contact hit when the skill prefab and shared runtime provide a collider hitbox
- one cast resolves one area center
- one immediate damage/status application
- circular area by parsed `Radius`
- battlefield-wide hit when parsed `CoverAll` is true
- nearest-target automatic center selection
- manual aim offset when the current input flow already supports it
- cooldown-based cast gating
- base damage plus one chosen stat coefficient
- damage attribute mapping
- prefab-based one-shot visual spawn through the current `EffectManager` / skill effect path
- on-hit damage
- on-hit status application
- choice-driven damage and radius modifiers when the provided modifier data fits the current shared snapshot fields

In short:
If the skill is "data goes in, one shared area hit comes out," Builder should proceed.

## Minimal Runtime Understanding

Builder does not need to rediscover the whole project.
It only needs to understand this minimal runtime contract:

- parsed active skill data becomes runtime skill data
- `SingleAttack` runtime kind maps to `SingleAttackData`
- learned active skills become runtime instances through the shared runtime factory
- SingleAttack skills execute through `SingleAttackSkillExecutor`
- one-shot area damage and status application go through `InGameZoneSkillActor.ApplyAreaTick`
- shared damage and status application go through `InGameCombatManager`
- choice modifiers apply through `SkillExecutionSnapshot`
- prefab lookup uses the current effect/prefab binding path

Builder may confirm these current connection points in code, but should not turn the task into a broad code exploration.

## Common Mapping Responsibility

When a parsed SingleAttack is implemented, Builder should wire the provided values into the shared SingleAttack path in this shape:

- parsed identity -> runtime skill identity
- parsed damage values -> shared area-hit damage spec
- parsed cooldown -> shared runtime timing state
- parsed radius / cover-all flag -> shared area targeting spec
- parsed targeting flags -> shared auto/manual center behavior
- parsed status values -> shared on-hit status spec
- parsed prefab info -> shared one-shot visual binding path
- parsed choice modifiers -> shared snapshot modifier path
- parsed cooldown edits or reductions -> existing cooldown authority such as base `cooldown_seconds` or choice `cooldown_multiplier`, not a new custom cooldown-percent field

The important rule is not the exact property names.
The important rule is:
do not invent new monster-only logic when the shared SingleAttack path already supports the requested behavior.

## Stop And Ask User Rule

Builder must stop and ask the user when the request contains behavior outside the common SingleAttack contract.

Stop-and-ask examples:

- delayed activation, warning zone, or ground telegraph before the hit
- repeated pulses or multiple timed hits
- target-specific conditional damage such as "damage per stack"
- marked-target-only search or marked-target fanout
- execute thresholds or missing-health scaling
- hit-count based follow-up effects
- ally shield, heal, or buff bundled into the same SingleAttack
- pull, knockback, stun, or other control behavior outside the current shared status path
- separate inner and outer radii
- chained explosions or secondary impact areas
- a special effect that depends on "only the first target", "third target", "on kill", or similar sequence state
- an explicit target-designated mark/debuff structure whose common behavior is target selection rather than prefab contact

When this happens, Builder should not try a best guess.
Builder should stop with a short question describing exactly which unsupported behavior was requested.

## Preferred Builder Response Pattern

When the request is normal:

- say it fits the common SingleAttack path
- implement it through the shared SingleAttack runtime
- report which parsed fields were consumed

When the request is not normal:

- say it does not fit the common SingleAttack path
- name the unsupported behavior
- ask the user whether to:
  - define a one-off exception, or
  - design a reusable shared extension

## Example Interpretation

Example 1:
User says "Implement `rin-e`."

If the provided parsed values describe a normal one-shot circular area hit with common damage, cooldown, radius, and status behavior, this blueprint should be enough for Builder to proceed through the shared SingleAttack path.

Example 2:
User says "Implement `eve-d`, and deal extra damage per Shock stack."

This blueprint is still enough for Builder to understand the situation, but Builder must stop and ask because stack-scaling conditional damage is not part of the current common SingleAttack contract.

Example 3:
User says "Implement `ariel-e`, and also shield all allies."

Builder must stop and ask because bundled ally shield behavior is not common SingleAttack behavior.

## Builder Checklist

Before implementation:

1. Confirm the parsed input package exists.
2. Confirm `RuntimeKind` is `SingleAttack`.
3. Confirm the request fits the common SingleAttack contract.
4. Stop immediately if the request contains unsupported special behavior.

During implementation:

1. Use the shared SingleAttack runtime.
2. Wire only the provided parsed values.
3. Keep behavior common unless the user approved an exception.
4. Avoid monster-only hardcoded branches unless the user explicitly approved a one-off rule.

After implementation:

1. compile
2. refresh Unity scripts if available
3. check console errors/warnings
4. report whether the implementation stayed inside the common SingleAttack path

## Required Builder Output

For each SingleAttack task, Builder should report:

- skill ID
- whether the request fit the common SingleAttack path
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

- Builder does not waste time rediscovering SingleAttack numbers in CSV files
- Builder does not over-read unrelated scripts
- Builder uses the common SingleAttack runtime by default
- Builder stops and asks when a skill needs special behavior

That is the desired behavior.
