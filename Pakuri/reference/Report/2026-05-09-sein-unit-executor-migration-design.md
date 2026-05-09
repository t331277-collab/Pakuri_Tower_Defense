# Sein Unit Executor Migration Design

## Task title

Migrate Sein A-J behavior to a `CombatUnitRuntime` based executor.

## Goals

- Route selected and manifested Sein B-E active skills through unit-owned `CombatSkillRuntime` where possible.
- Route manifested Sein A through Sein-specific projectile/passive logic instead of the generic manifested projectile damage path.
- Make Sein F-J passive checks work from the unit's `RunMonsterState` for manifested 2P-5P units while preserving selected 1P behavior.
- Keep existing selected 1P manual A input behavior intact.

## Current evidence

- `CombatRuntimeSeinSkills.cs` currently owns selected-Sein timers and selected-only helpers such as `HasSeinHeatedAim()`, `TryCastSeinBlazingVolley()`, `TryCastSeinFlameTrajectory()`, `TryCastSeinSuperheatedZone()`, and `TryCastSeinDoomsdayLine()`.
- `CombatRuntimeRinSkills.cs` already has a reusable pattern: `TryTickRinUnitSkill(...)`, `TryTriggerRinUnitAutomaticSkills(...)`, `HasRinUnitPassive(...)`, and unit damage helpers.
- `CombatRuntimeEveSkills.cs` already has a reusable pattern: selected and manifested units both use `CombatUnitRuntime` plus `CombatSkillRuntime` for Eve unit skills.
- `CombatRuntimeParty.cs` currently calls Eve and Rin unit tick methods before falling back to generic manifested skill execution, but it has no Sein unit tick call yet.

## Handoff to Code Builder

Code Builder should add Sein unit executor methods, call them before generic manifested fallback, keep selected Sein UI cooldowns synchronized from the selected unit runtime, and validate with build/Unity console checks.
