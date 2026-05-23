# Passive Stat Blueprint For InGame Skills

## Purpose

This document is the primary implementation contract for InGame passive skills whose behavior is fundamentally "always-on value modification."

The intended workflow is simple:

- the caller provides already parsed passive skill data
- Code Builder reads this blueprint first
- if the passive fits the common passive-stat path, Builder implements it through shared stat / skill / status modifier paths
- if the passive does not fit the common passive-stat path, Builder stops and asks the user

This blueprint is not a data-discovery guide.
It is designed so Skill Builder can implement ordinary passive number-adjustment skills without reopening CSV files, reference markdown, or old monster-specific code.

## Core Rule

For passive-stat implementation work, do not search CSV files, monster reference files, or old monster-specific code just to rediscover values or intent.

The caller owns parsed input.
Code Builder owns runtime wiring.

If a required value or behavior decision is missing, Builder must stop and report the missing item instead of guessing.

## Builder Working Mode

When the user says something like:

- implement Eve passive `f`
- implement this passive skill
- connect this parsed passive to runtime

Builder should assume this workflow:

1. the parsed passive values are already provided by the caller or task context
2. Builder does not reopen CSV files to rediscover values
3. Builder uses existing shared passive/stat/skill/status modifier paths
4. Builder asks the user only when the requested passive behavior is outside the common passive-stat contract

## What Builder May Read

Default mandatory markdown read set for passive-stat implementation:

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

Allowed:

- this blueprint
- `AGENTS.md`
- `MDTREE.md`
- `AGENTS_ROLE/COMMON.md`
- `AGENTS_ROLE/GAMEBULIDER.md`
- `AGENTS_ROLE/GAMEBULIDER_SKILL.md`
- the routed board files explicitly justified by the active request or inspected failure path
- the current runtime scripts that must be edited or compiled

Not allowed as value-discovery sources unless the user explicitly asks:

- `Pakuri/Assets/CSVdata/**/*.csv`
- `Pakuri/reference/**`
- old monster-specific implementations used only to infer behavior
- unrelated board markdown such as UI, RUN, DATA, OPS, or other monster boards when the request and inspected failure path do not explicitly touch those domains

Important:
Reading current runtime scripts is still allowed.
Builder may inspect the current shared passive/runtime path to confirm where parsed values are wired.
That is different from searching old data files for missing values.

## Required Parsed Input

Builder should expect the caller to provide a parsed passive package.

Minimum required fields:

- `PassiveSkillId`
- `RuntimeKind`
- `PassiveMode`
- `TargetScope`
- `ModifierSpecs`

Required rules:

- `RuntimeKind` must be exactly `Passive`
- `PassiveMode` must describe one of the common passive-stat modes below
- every item in `ModifierSpecs` must include:
  - `ModifierType`
  - `Operation`
  - `Value`

Optional but common fields:

- `TargetSkillId`
- `TargetStatusId`
- `ConditionSkillId`
- `ConditionStatusId`
- `ChoiceGateId`

If any required field is missing, Builder must stop and report it.

## Common Passive-Stat Contract

The following behavior is considered normal shared passive-stat work.
If the requested passive fits this list, Builder should implement it without asking extra design questions.

- always-on base stat bonuses or penalties
- always-on crit chance / crit damage / max health / resistance / damage-taken modifiers
- always-on skill-specific number modifiers on an existing shared skill path
- always-on status-rule modifiers that only adjust shared numeric fields
- one passive row that modifies existing shared values without creating a new gameplay actor, trigger, or event

In short:
If the passive is "existing shared values are modified while the passive is learned," Builder should proceed.

## Allowed Modifier Families

The first passive-stat blueprint should support only these shared modifier families:

- `AlwaysOnStatModifier`
  - examples: attack, spell power, crit chance, crit damage, max health, action speed, move speed, ailment resistance, incoming damage taken, outgoing damage, element damage taken

- `AlwaysOnSkillModifier`
  - examples: damage multiplier for one skill, cooldown multiplier for one skill, radius multiplier for one skill, duration multiplier for one skill, tick-interval multiplier for one skill, magazine bonus for one skill, reload multiplier for one skill

- `AlwaysOnStatusRuleModifier`
  - examples: status max stacks bonus, status duration bonus, status stack amount bonus, status critical-damage-taken bonus, status element-damage-taken bonus

These modifiers must reuse existing shared runtime fields.
Do not invent a monster-only passive storage path when a shared modifier path already exists.

## Minimal Runtime Understanding

Builder does not need to rediscover the whole project.
It only needs to understand this minimal runtime contract:

- parsed passive data becomes runtime passive data
- passive modifiers must flow into existing shared stat / skill / status modifier holders
- learned passives must stay always-on while owned by the actor
- the implementation should modify existing shared calculations rather than spawn new runtime actors

Builder may confirm these current connection points in code, but should not turn the task into broad project exploration.

## Stop And Ask User Rule

Builder must stop and ask the user when the request contains behavior outside the common passive-stat contract.

Stop-and-ask examples:

- passive directly deals damage
- passive spawns a projectile, beam, zone, drone, trap, mine, turret, or install
- passive triggers on hit, on crit, on kill, on status expire, on taking damage, on shield break, or any other event
- passive uses probability or proc chance
- passive counts attacks, stacks, kills, or elapsed hits before firing a separate effect
- passive applies a new buff/debuff instance to units instead of only modifying existing shared numeric interpretation
- passive searches nearby enemies, fans out to marked targets, or chooses targets dynamically
- passive creates delayed explosions, chained effects, or follow-up attacks
- passive changes behavior on first/third/last occurrence or any other sequence state

When this happens, Builder should not try a best guess.
Builder should stop with a short question describing exactly which unsupported behavior was requested.

## Preferred Builder Response Pattern

When the request is normal:

- say it fits the common passive-stat path
- implement it through shared passive/stat/skill/status modifier runtime
- report which parsed fields were consumed

When the request is not normal:

- say it does not fit the common passive-stat path
- name the unsupported behavior
- ask the user whether to:
  - define a one-off exception, or
  - design a reusable shared extension

## Example Interpretation

Example 1:
User says "Implement Eve passive `f`."

If the provided parsed values describe an always-on damage multiplier, crit bonus, or skill cooldown modifier, this blueprint should be enough for Builder to proceed.

Example 2:
User says "Implement this passive that fires lightning every third hit."

Builder must stop and ask because event-driven repeat firing is not common passive-stat behavior.

Example 3:
User says "Implement this passive that creates an explosion around nearby enemies when shield breaks."

Builder must stop and ask because the passive creates triggered damage and target search behavior.

## Builder Checklist

Before implementation:

1. Confirm the parsed passive package exists.
2. Confirm `RuntimeKind` is exactly `Passive`.
3. Confirm the request fits the common passive-stat contract.
4. Stop immediately if the passive contains unsupported triggered or damage-dealing behavior.

During implementation:

1. Use existing shared passive/stat/skill/status modifier runtime.
2. Wire only the provided parsed values.
3. Keep behavior common unless the user approved an exception.
4. Avoid monster-only hardcoded branches unless the user explicitly approved a one-off rule.

After implementation:

1. compile
2. refresh Unity scripts if available
3. check console errors/warnings
4. report whether the implementation stayed inside the common passive-stat path

## Required Builder Output

For each passive-stat task, Builder should report:

- passive skill ID
- whether the request fit the common passive-stat path
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

This blueprint is intentionally narrow.

It is designed so that:

- Skill Builder can implement ordinary passive number-adjustment skills from blueprint plus parsed input alone
- Builder does not waste time rediscovering passive values from CSV files
- Builder does not over-read unrelated markdown or code
- event-driven, damage-dealing, and target-search passive behavior is blocked behind stop-and-ask

That is the desired behavior.
