# Manifested Unit Runtime Refactor Design

## Evidence

- `CombatRuntimeParty.cs` currently stores 2P-5P manifested monster state in private nested classes inside `CombatRuntimeController`.
- `CombatRuntimeParty.cs` already reads manifested party members from `RunSession.ManifestedMonsterIds`, resolves each `MonsterDefinition`, and uses each `RunSession.RunMonsterState` for learned active skills and reward modifiers.
- `CombatRuntimeParty.cs` currently creates projectiles and effects through controller-owned battlefield helpers because those helpers need enemy lists, projectile roots, damage application, and status effect paths.
- `CombatRuntimeScene.cs` creates `EveUnit` as the selected 1P visual anchor, while `CombatRuntimeParty.cs` binds manifested party members to `2PMonster` through `5PMonster`.

## Target Structure

1. Keep `CombatRuntimeController` as the battlefield context for enemy lookup, projectile creation, effect creation, damage application, and status labels.
2. Move manifested unit state from controller-private objects into `CombatUnitRuntime` components attached to `2PMonster` through `5PMonster` objects.
3. Move manifested per-skill timers, magazine state, reload state, and queued Vega projectile state into `CombatSkillRuntime`.
4. Keep source data in `MonsterDefinition`, `SkillDefinition`, and `RunSession.RunMonsterState`.
5. Leave selected 1P `EveUnit` migration out of this pass. The user will decide step 6 separately.

## Ownership Rule

- Unit component: owns monster reference, run state reference, HP/stat snapshot, learned skill runtime list, and per-frame manifested skill ticking.
- Skill runtime: owns cooldown, magazine, reload, and queued projectile timing state for one learned active skill.
- Controller battlefield service: owns target finding, projectile/effect object creation, damage application, status effects, and UI snapshot aggregation.

## Handoff

Designer hands off steps 2-5 to Code Builder:

- Add `CombatUnitRuntime` and `CombatSkillRuntime`.
- Rebind manifested 2P-5P slots to `CombatUnitRuntime` components.
- Keep projectile/effect creation in `CombatRuntimeController` but call it from unit-owned skill ticking.
- Validate with compile/build/editor console checks only. Do not run Play Mode.
