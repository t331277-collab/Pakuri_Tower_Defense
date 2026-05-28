# Skill Builder Handoff Format

## Purpose

This file defines the normalized handoff package for Skill Builder work when the source of truth is a scoped bundle of skill CSV rows plus one selected blueprint.

Use this format only in the exception path where blueprint-only work is not enough.
Its purpose is still to avoid reopening broad CSV/reference history.

## Core Rule

The handoff must be small, scoped, and evidence-backed.

It should provide the selected blueprint and only the row bundle needed for the target skill.
It should not dump unrelated CSV rows or broad monster history.

## Required Handoff Sections

### 1. Goal

State:

- target skill id
- target monster id
- requested implementation outcome

### 2. Selected blueprint

State exactly one selected blueprint path, for example:

- `boards/SkillBluePrint/projectile-blueprint.md`
- `boards/SkillBluePrint/BeamSkill-blueprint.md`
- `boards/SkillBluePrint/single-attack-blueprint.md`
- `boards/SkillBluePrint/area-attack-blueprint.md`
- `boards/SkillBluePrint/multi-effect-skill-csv-blueprint.md`

If no single blueprint owns the work, stop and resolve that before Builder implementation.

### 3. Exception docs used

List only the needed exception docs:

- `boards/SkillBluePrint/skill-csv-exception-guide.md`
- `boards/SkillBluePrint/skill-builder-handoff-format.md`

### 4. Base skill row evidence

Provide:

- file path
- line reference when available
- normalized field summary for the one base `monster_skills.csv` row

Minimum fields to restate:

- `skill_id`
- `monster_id`
- `runtime_kind`
- `attribute`
- `base_damage`
- relevant coefficient
- relevant timing fields
- relevant targeting fields
- relevant base status payload fields

### 5. Choice row bundle

When choice rows exist, provide only the rows that affect this implementation.

For each row, restate:

- `choice_id`
- why it matters
- which columns are relevant to the implementation
- whether it is common-path or stop-and-ask under the selected blueprint

### 6. Effect row bundle

When effect rows exist, provide only the rows that affect this implementation.

For each row, restate:

- `effect_id`
- `effect_kind`
- gating fields
- timing fields
- target/center/visual fields
- payload fields
- why this row exists in addition to the base skill row

### 7. Trigger row bundle

When trigger rows exist, provide only the rows that affect this implementation.

For each row, restate:

- `trigger_id`
- `trigger_event`
- `triggered_skill_id`
- concrete damage payload fields: `base_damage`, `attack_power_coefficient`, `spell_power_coefficient`, `damage_multiplier`, `damage_source`
- payload source
- timing/repeat fields
- why the trigger is required

### 8. Prefab and asset references

List only the relevant prefab or asset paths referenced by the scoped rows.

### 9. Pattern classification

State which pattern from `skill-csv-exception-guide.md` the bundle matches.

Examples:

- base skill only
- base skill plus choices
- base skill plus multi-effect rows
- base skill plus trigger rows
- base skill plus choices plus multi-effect rows

### 10. Known unsupported points

State any behavior that still leaves the selected blueprint's common contract.

If none remain, say that explicitly.

### 11. Builder verification expectation

State the local non-gameplay verification expected from Builder, such as:

- markdown/file edit verification
- targeted CSV parse checks
- runtime/editor `dotnet build`
- Unity CSV catalog sync
- Unity console check

## Recommended Handoff Template

```md
## Skill Builder Handoff

### Goal
- Implement `<skill_id>` for `<monster_id>`.

### Selected blueprint
- `boards/SkillBluePrint/<selected-blueprint>.md`

### Exception docs used
- `boards/SkillBluePrint/skill-csv-exception-guide.md`

### Base skill row evidence
- Source: `<path:line>`
- Normalized summary:
  - `runtime_kind=...`
  - `attribute=...`
  - `base_damage=...`
  - `cooldown_seconds=...`
  - `target_selection=...`

### Choice rows
- `<choice_id>`: relevant fields `...`; purpose `...`

### Effect rows
- `<effect_id>`: relevant fields `...`; purpose `...`

### Trigger rows
- `<trigger_id>`: relevant fields `base_damage=...`, `attack_power_coefficient=...`, `spell_power_coefficient=...`, `damage_multiplier=...`, `damage_source=...`; purpose `...`

### Prefab refs
- `<asset path>`

### Pattern classification
- `<pattern name>`

### Known unsupported points
- none

### Builder verification expected
- `dotnet build ...`
- Unity CSV sync
- console check
```

## What Not To Put In The Handoff

Do not include:

- unrelated monster rows
- old monster-specific implementation summaries used only to infer behavior
- broad reference-document prose when the scoped CSV bundle already defines the behavior
- multiple competing blueprint choices without resolving the owner

## Stop Rule

If the handoff cannot fill the base row or cannot choose exactly one blueprint, Skill Builder should stop instead of guessing.
