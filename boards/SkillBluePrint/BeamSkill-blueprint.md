# BeamSkill Blueprint For InGame LineAttack Skills

## Purpose

This document is the primary implementation contract for InGame BeamSkill / LineAttack skills.

The intended workflow is simple:

- the caller provides already parsed beam skill data
- Code Builder reads this blueprint first
- if the skill fits the common BeamSkill path, Builder implements it through the shared line-attack runtime
- if the skill does not fit the common BeamSkill path, Builder stops and asks the user

This blueprint is not a data-discovery guide.
It should let an AI understand when BeamSkill work is straightforward, what inputs are required, what common behavior already exists, and when work must stop for clarification.

## Core Rule

For BeamSkill implementation work, do not search CSV files, monster reference files, or old monster-specific code just to rediscover numbers or behavior.

The caller owns parsed input.
Code Builder owns runtime wiring.

If a required value or behavior decision is missing, Builder must stop and report the missing item instead of guessing.

## Builder Working Mode

When the user says something like:

- implement `eve-b`
- implement a new beam skill
- connect this parsed `LineAttack` skill to runtime

Builder should assume this workflow:

1. the parsed skill values are already provided by the caller or task context
2. Builder does not re-open CSV files to find numbers
3. Builder uses the shared BeamSkill / line-attack runtime
4. Builder asks the user only when the requested behavior is outside the common BeamSkill contract

## What Builder May Read

Default mandatory markdown read set for BeamSkill implementation:

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

Do not read extra markdown files for BeamSkill work just to gather general background.

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
Builder may inspect the current shared BeamSkill runtime to confirm where parsed values are wired.
That is different from searching old data files for missing numbers.

## Required Parsed Input

Builder should expect the caller to provide a parsed BeamSkill package.

Minimum required fields:

- `SkillId`
- `RuntimeKind`
- `BaseDamage`
- `DamageAttribute`
- `PowerStat`
- `PowerCoefficient`
- `BeamWidth`
- `ActiveDurationSeconds`
- `TickIntervalSeconds`
- `CooldownSeconds`
- `TargetingMode`
- `CanManualAim`
- `CanAutoTarget`
- `HasBeamPrefab`

Optional but common fields:

- `OnHitStatusId`
- `OnHitStatusChance`
- `ChoiceModifierSpecs`
- `BeamLengthOverride`
- `SkillEffectPrefabOverride`

If any required field is missing, Builder must stop and report it.

Not part of the current common BeamSkill input contract:

- `ChargeDelaySeconds`
- `GroundWarningSeconds`
- `StopAtFirstTarget`
- `PushDistance`
- `ResistanceReductionSpec`
- `PerTargetHitCooldownSeconds`
- `CurvedPathSpec`
- `ForkSpec`
- `ReflectSpec`

Do not require those fields for normal BeamSkill work.
If a request truly depends on one of them, Builder should treat that as a special case and ask the user.

## Common BeamSkill Contract

The following behavior is considered normal shared BeamSkill work.
If the requested skill fits this list, Builder should implement it without asking extra design questions.

- straight static line projection
- one cast creates one line-attack actor or one routed fallback tick
- immediate first tick on creation
- repeated tick damage at a fixed interval while duration remains
- width from parsed `BeamWidth`
- duration from parsed `ActiveDurationSeconds`
- length from parsed `BeamLengthOverride` when explicitly provided, otherwise from the shared battlefield-boundary fallback
- cooldown-based cast gating
- nearest-target automatic direction
- manual aim when the current input flow already supports it
- prefab-based visual spawn through the current `EffectManager` / skill effect path
- on-hit damage
- on-hit status application
- choice-driven damage, width, duration, and tick-interval modifiers when the provided modifier data fits the current shared snapshot fields

In short:
If the skill is "data goes in, common straight ticking beam comes out," Builder should proceed.

## Minimal Runtime Understanding

Builder does not need to rediscover the whole project.
It only needs to understand this minimal runtime contract:

- parsed active skill data becomes runtime skill data
- `LineAttack` runtime kind maps to `BeamSkillData`
- learned active skills become runtime instances through the shared runtime factory
- Beam skills execute through `BeamSkillExecutor`
- persistent line damage and repeated ticks happen in `InGameLineAttackActor`
- shared damage and status application go through `InGameCombatManager`
- choice modifiers apply through `SkillExecutionSnapshot`
- prefab lookup uses the current effect/prefab binding path

Builder may confirm these current connection points in code, but should not turn the task into a broad code exploration.

## Common Mapping Responsibility

When a parsed BeamSkill is implemented, Builder should wire the provided values into the shared BeamSkill path in this shape:

- parsed identity -> runtime skill identity
- parsed damage values -> shared beam tick damage spec
- parsed cooldown -> shared runtime timing state
- parsed width / duration / tick interval / optional beam length -> shared BeamSkill timing and geometry
- parsed targeting flags -> shared auto/manual direction behavior
- parsed status values -> shared on-hit status spec
- parsed prefab info -> shared line-attack visual binding path
- parsed choice modifiers -> shared snapshot modifier path

The important rule is not the exact property names.
The important rule is:
do not invent new monster-only logic when the shared BeamSkill path already supports the requested behavior.

## Stop And Ask User Rule

Builder must stop and ask the user when the request contains behavior outside the common BeamSkill contract.

Stop-and-ask examples:

- charge-up, warning zone, or delayed activation before the first damaging tick
- stop at first target
- knockback or pushback
- direct resistance reduction or other non-status debuffs outside the current shared status path
- curved, sweeping, forked, reflected, chained, or multi-segment beams
- persistent repeated ticking when no beam prefab / actor path is available
- per-target persistent hit cooldown independent of the beam tick interval
- a special effect that depends on "last tick", "third tick", "only the first hit", or similar sequence state

When this happens, Builder should not try a best guess.
Builder should stop with a short question describing exactly which unsupported behavior was requested.

## Preferred Builder Response Pattern

When the request is normal:

- say it fits the common BeamSkill path
- implement it through the shared BeamSkill runtime
- report which parsed fields were consumed

When the request is not normal:

- say it does not fit the common BeamSkill path
- name the unsupported behavior
- ask the user whether to:
  - define a one-off exception, or
  - design a reusable shared extension

## Example Interpretation

Example 1:
User says "Implement `eve-b`."

If the provided parsed values describe a normal straight line-attack with common damage, duration, tick, and status behavior, this blueprint should be enough for Builder to proceed through the shared BeamSkill path.

Example 2:
User says "Implement `eve-b`, and stop the beam at the first enemy it touches."

This blueprint is still enough for Builder to understand the situation, but Builder must stop and ask the user because stop-first-target is not part of the current common BeamSkill contract.

Example 3:
User says "Implement `eve-b`, but make it charge for 1 second and then sweep in an arc."

Builder must stop and ask the user because charge delay and sweeping beam behavior are not common BeamSkill behavior.

## Builder Checklist

Before implementation:

1. Confirm the parsed input package exists.
2. Confirm `RuntimeKind` is BeamSkill-compatible.
3. Confirm the request fits the common BeamSkill contract.
4. Stop immediately if the request contains unsupported special behavior.

During implementation:

1. Use the shared BeamSkill runtime.
2. Wire only the provided parsed values.
3. Keep behavior common unless the user approved an exception.
4. Avoid monster-only hardcoded branches unless the user explicitly approved a one-off rule.

After implementation:

1. compile
2. refresh Unity scripts if available
3. check console errors/warnings
4. report whether the implementation stayed inside the common BeamSkill path

## Required Builder Output

For each BeamSkill task, Builder should report:

- skill ID
- whether the request fit the common BeamSkill path
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
- check console errors/warnings
- leave Play Mode gameplay verification to the user

## Final Designer Intent

This blueprint is intentionally opinionated.

It is designed so that:

- Builder does not waste time rediscovering BeamSkill numbers in CSV files
- Builder does not over-read unrelated scripts
- Builder uses the common BeamSkill runtime by default
- Builder stops and asks when a skill needs special behavior

That is the desired behavior.
