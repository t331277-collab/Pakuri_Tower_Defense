# Projectile Blueprint For InGame Skills

## Purpose

This document is the primary implementation contract for InGame projectile skills.

The intended workflow is simple:

- the caller provides already parsed skill data
- Code Builder reads this blueprint first
- if the skill fits the common projectile path, Builder implements it through the shared projectile runtime
- if the skill does not fit the common projectile path, Builder stops and asks the user

This blueprint is not a data-discovery guide.
It should let an AI understand when projectile work is straightforward, what inputs are required, what common behavior already exists, and when work must stop for clarification.

## Core Rule

For projectile implementation work, do not search CSV files, monster reference files, or old monster-specific code just to rediscover numbers or behavior.

The caller owns parsed input.
Code Builder owns runtime wiring.

If a required value or behavior decision is missing, Builder must stop and report the missing item instead of guessing.

## Builder Working Mode

When the user says something like:

- implement `rin-a`
- implement a new projectile skill
- connect this parsed projectile skill to runtime

Builder should assume this workflow:

1. the parsed skill values are already provided by the caller or task context
2. Builder does not re-open CSV files to find numbers
3. Builder uses the shared projectile runtime
4. Builder asks the user only when the requested behavior is outside the common projectile contract

## What Builder May Read

Default mandatory markdown read set for projectile implementation:

- `AGENTS.md`
- `MDTREE.md`
- `AGENTS_ROLE/GAMEBULIDER.md`
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md`
- this blueprint

Conditional markdown reads only when explicitly justified:

- the relevant monster board when the user names a specific monster or the inspected failure path names it
- DATA or asset boards only when the user or inspected failure explicitly touches CSV, prefab, scene serialization, runtime catalog, or `EffectManager` wiring
- RUN boards only when the user or inspected failure explicitly touches `RunSession`, Offering, Menifest, or `NewRunScene` runtime ownership
- UI boards only when the user or inspected failure explicitly names UI objects, buttons, canvases, TMP, UXML, or USS

Do not read extra markdown files for projectile work just to gather general background.

Allowed:

- this blueprint
- `AGENTS.md`
- `MDTREE.md`
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
Builder may inspect the current shared projectile runtime to confirm where parsed values are wired.
That is different from searching old data files for missing numbers.

## Required Parsed Input

Builder should expect the caller to provide a parsed projectile package.

Minimum required fields:

- `SkillId`
- `RuntimeKind`
- `BaseDamage`
- `DamageAttribute`
- `PowerStat`
- `PowerCoefficient`
- `ProjectileSpeed`
- `PierceCount`
- `ShotIntervalSeconds`
- `CooldownSeconds`
- `MagazineSize`
- `ReloadSeconds`
- `TargetingMode`
- `CanManualAim`
- `CanAutoTarget`
- `HasProjectilePrefab`

Optional but common fields:

- `OnHitStatusId`
- `OnHitStatusChance`
- `ChoiceModifierSpecs`
- `ProjectilePrefabSource`
- `SkillEffectPrefabOverride`

If any required field is missing, Builder must stop and report it.

Not part of the current common projectile input contract:

- `ProjectileCount`
- `LifetimeSeconds`
- `MaxTravelDistance`
- `DestroyBoundaryPolicy`
- `HitRadius`
- `OnHitStatusStacks`
- `OnHitStatusDurationSeconds`

Do not require those fields for normal projectile work.
If a request truly depends on one of them, Builder should treat that as a special case and ask the user.

## Common Projectile Contract

The following behavior is considered normal shared projectile work.
If the requested skill fits this list, Builder should implement it without asking extra design questions.

- straight projectile travel
- one-shot projectile spawn
- simultaneous multi-projectile fan spread
- cooldown-based cast gating
- magazine and reload behavior
- shot interval gating
- base damage plus one chosen stat coefficient
- damage attribute mapping
- projectile speed
- pierce count
- nearest-target automatic aim
- manual aim when the current input flow already supports it
- prefab-based projectile visual spawn
- on-hit damage
- on-hit status application
- current shared branch-on-hit behavior only when the provided branch data matches the existing branch pattern

In short:
If the skill is "data goes in, common projectile comes out," Builder should proceed.

## Minimal Runtime Understanding

Builder does not need to rediscover the whole project.
It only needs to understand this minimal runtime contract:

- parsed active skill data becomes runtime skill data
- projectile runtime skills execute through the shared projectile executor
- projectile movement and hit handling happen in the shared projectile actor
- choice modifiers apply through the shared execution snapshot path
- prefab lookup uses the current effect/prefab binding path

Builder may confirm these current connection points in code, but should not turn the task into a broad code exploration.

## Common Mapping Responsibility

When a parsed projectile skill is implemented, Builder should wire the provided values into the shared projectile path in this shape:

- parsed identity -> runtime skill identity
- parsed damage values -> shared projectile damage spec
- parsed cooldown / interval / magazine / reload -> shared runtime timing and ammo state
- parsed projectile speed / pierce / count -> shared projectile blueprint spec
- parsed targeting flags -> shared auto/manual direction behavior
- parsed status values -> shared on-hit status spec
- parsed prefab info -> shared projectile visual binding path
- parsed choice modifiers -> shared snapshot modifier path

The important rule is not the exact property names.
The important rule is:
do not invent new monster-only logic when the shared projectile path already supports the requested behavior.

## Stop And Ask User Rule

Builder must stop and ask the user when the request contains behavior outside the common projectile contract.

Stop-and-ask examples:

- timed burst or delayed repeated firing
- homing or guided projectile
- bounce or ricochet
- trap, install, mine, or stationary projectile
- last-shot explosion
- impact area or explosion after hit
- projectile-carried mark payload
- multi-hitbox projectile
- custom target priority beyond current nearest-target behavior
- branch behavior that is not the current shared branch pattern
- a special effect that depends on "final bullet", "every third shot", "only the last hit", or similar sequence state

When this happens, Builder should not try a best guess.
Builder should stop with a short question describing exactly which unsupported behavior was requested.

## Preferred Builder Response Pattern

When the request is normal:

- say it fits the common projectile path
- implement it through the shared projectile runtime
- report which parsed fields were consumed

When the request is not normal:

- say it does not fit the common projectile path
- name the unsupported behavior
- ask the user whether to:
  - define a one-off exception, or
  - design a reusable shared extension

## Example Interpretation

Example 1:
User says "Implement `rin-a`."

If the provided parsed values describe a normal straight projectile with common damage/timing/status behavior, this blueprint should be enough for Builder to proceed through the shared projectile path.

Example 2:
User says "Implement `rin-a`, and the last projectile explodes in a circle."

This blueprint is still enough for Builder to understand the situation, but Builder must stop and ask the user because "last projectile explodes" is not part of the current common projectile contract.

Example 3:
User says "Implement `rin-a`, and make it a homing missile."

Builder must stop and ask the user because homing is not common projectile behavior.

## Builder Checklist

Before implementation:

1. Confirm the parsed input package exists.
2. Confirm `RuntimeKind` is projectile-compatible.
3. Confirm the request fits the common projectile contract.
4. Stop immediately if the request contains unsupported special behavior.

During implementation:

1. Use the shared projectile runtime.
2. Wire only the provided parsed values.
3. Keep behavior common unless the user approved an exception.
4. Avoid monster-only hardcoded branches unless the user explicitly approved a one-off rule.

After implementation:

1. compile
2. refresh Unity scripts if available
3. check console errors/warnings
4. report whether the implementation stayed inside the common projectile path

## Required Builder Output

For each projectile task, Builder should report:

- skill ID
- whether the request fit the common projectile path
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

- Builder does not waste time rediscovering projectile numbers in CSV files
- Builder does not over-read unrelated scripts
- Builder uses the common projectile runtime by default
- Builder stops and asks when a skill needs special behavior

That is the desired behavior.
