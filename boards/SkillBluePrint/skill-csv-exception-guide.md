# Skill CSV Exception Guide

## Purpose

This file is an exception-only companion for Skill Builder work.

Default Skill Builder work should use:

- one selected `boards/SkillBluePrint/*-blueprint.md`

and nothing more.

Read this file only when the selected blueprint cannot proceed on its own because the request arrives as a scoped skill CSV row bundle or because row combinations create an exception-level interpretation problem.

## Core Rule

This file does not replace blueprint selection.

Use the base `monster_skills.csv` row to select the primary blueprint from `runtime_kind` first.
Then use this file only if the scoped rows still need interpretation.

## When Skill Builder May Read This File

Read this file only when at least one of these is true:

- the caller provides a scoped row bundle instead of a fully parsed skill package
- the selected blueprint stops because choice/effect/trigger row combinations need a reusable interpretation
- the task needs a quick table-ownership check to decide whether behavior belongs in base, choice, effect, or trigger rows

If none of those are true, skip this file.

## Table Ownership Short Form

### `monster_skills.csv`

Owns the base skill row.

Use it for:

- base runtime kind
- base damage/timing/targeting
- one base status payload

Do not use it for:

- additive delayed extra rows
- trigger-event follow-ups
- choice-gated replacement effects that need separate execution rows

### `monster_skill_choices.csv`

Owns choice-driven modifiers.

Use it for:

- snapshot modifiers
- supported status/branch modifiers
- supported count-based modifiers
- supported source-conditional status overrides

Do not use it for:

- delayed extra execution rows
- trigger-event behavior

### `monster_skill_effects.csv`

Owns additional or replacement per-cast effect rows.

Use it for:

- extra damage/status/buff/debuff rows
- delayed follow-up rows that still belong to one cast flow
- choice-gated replacement/additive effect rows

### `monster_skill_triger.csv`

Owns event-triggered hidden follow-up execution rows.

Use it for:

- last-shot
- shield-expire
- status-expire
- shield-absorb
- other already-supported shared trigger events

## Row Combination Patterns

### Pattern A: Base skill only

- one `monster_skills.csv` row
- no required choice/effect/trigger interpretation

Use the selected blueprint only.

### Pattern B: Base skill plus choice rows

- one base skill row
- one or more `monster_skill_choices.csv` rows

Use when the choice remains a snapshot/status modifier already supported by the selected blueprint.

Stop and ask if the choice implies:

- delayed secondary execution
- event-triggered behavior
- unsupported sequence state

### Pattern C: Base skill plus effect rows

- one base skill row
- one or more `monster_skill_effects.csv` rows

Use when the base cast needs additive or replacement effect rows that still fit the shared multi-effect contract.

### Pattern D: Base skill plus trigger rows

- one base skill row
- one or more `monster_skill_triger.csv` rows

Use when the follow-up is event-driven instead of ordinary per-cast execution.

### Pattern E: Mixed rows

- base skill row
- choices and effects
- choices and triggers
- or all three

Interpret in this order:

1. base skill selects the blueprint
2. choice rows modify or gate
3. effect rows add or replace per-cast execution
4. trigger rows add event-driven execution

## Current Shared Exception Contracts

These are the main exception-level contracts currently represented in shared CSV/runtime support:

- count-based choice damage scaling through `count_status_id`, `count_target_side`, `damage_multiplier_per_count`, `count_max`
- source-conditional incoming-damage bonus through `status_conditional_source_status_id` and `status_conditional_damage_taken_bonus`
- multi-effect additive or replacement rows through `monster_skill_effects.csv`
- trigger-called hidden follow-up execution through `monster_skill_triger.csv`

If the scoped row bundle needs behavior outside these contracts, the selected blueprint still wins and Skill Builder should stop and ask.

## Relationship To Handoff

If this file is needed, the caller should preferably also provide:

- `boards/SkillBluePrint/skill-builder-handoff-format.md`

That handoff format tells Skill Builder how the scoped row bundle should be packaged so broad CSV rediscovery is still avoided.
