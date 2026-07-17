# Sein Skill Runtime Visual Migration Plan

## Goal

Convert Sein's prefab-backed skill visuals to runtime-created sprite, animator, and `BoxCollider2D` objects while preserving the current projectile, delayed-impact, persistent-zone, Trigger, and multi-deployment behavior.

Keep all nine Sein skill prefab assets on disk during migration. Prefab deletion is outside this plan.

This is a Designer handoff. Implementation has not started. Runtime object position offset and collider offset are fixed at `(0,0)` by user decision. Do not add offset columns or graph params. The remaining shared runtime/common-logic extensions still require explicit user approval before implementation.

## Inspected Evidence

- Unity-MCP asset search found nine skill prefabs under `Assets/Prefab/Skill/Sein`: `Sein_A`, `Sein_A_Master-2`, `Sein_B`, `Sein_C`, `Sein_C_Master-2`, `Sein_C_Master_1`, `Sein_D`, `Sein_D_Master_2`, and `Sein_E`.
- Unity-MCP hierarchy inspection found every Sein skill prefab is a single-root prefab with no child objects. Every root has one `SpriteRenderer` and one `BoxCollider2D`; all except A and B also have one `Animator`.
- Serialized prefab inspection supplies the exact sprite/controller GUIDs, root scales, and collider sizes listed below. Matching `.meta` files resolve those GUIDs to the listed asset paths.
- Several current prefab roots/colliders serialize non-zero legacy offsets, but the user explicitly set the migration target to `(0,0)` for both runtime object position offset and collider offset. Those legacy offsets are inspected evidence, not runtime authoring values.
- `RuntimeSkillVisualFactory` creates one runtime root with a sprite, optional animator, uniform initial scale, sorting order, and one root `BoxCollider2D`. With no offset authoring, `RuntimeSkillHitboxSpec.Offset` remains its default `(0,0)`.
- Current base/Trigger CSV and graph `RuntimeEffectVisual` authoring already expose the sprite/controller/scale/sorting/hitbox-size fields needed for the centered runtime representation.
- `ProjectileSkillExecutor` prefers a skill runtime visual over scene/skill projectile prefabs. `InGameProjectileActor` forces its root collider to trigger and owns movement/contact handling.
- `ZoneSkillExecutor` and persistent `SkillMultiEffectExecutor` zones create runtime visuals when present. `InGameZoneSkillActor` uses instantiated colliders as tick hitboxes when the zone is not `CoverAll`; otherwise it falls back to radius/all-target behavior.
- `SkillTriggerRuntime.ExecuteSingleAttackAction` uses a runtime hitbox when the Trigger runtime visual has a hitbox. This supports Sein-A master 2 with its authored size and fixed zero offset.
- Sein-E currently becomes multi-deployment only when `InGameSkillDefinitionMapper` sees a prefab-backed `SingleAttack` with `hit_target_count > 1`. `SingleAttackSkillExecutor` also applies its line-style rotation/scaling only to the prefab path, not the runtime-visual path.
- `InGameProjectileActor` stores `impactEffectPrefab` as a `GameObject` and instantiates that prefab in `ResolveImpact()`. No separate runtime impact visual field exists.
- Current active authoring is under `Pakuri/Assets/CSVdata/authoring/`, not the older `Pakuri/Assets/CSVdata/source/` paths recorded in older board history.

## Current Prefab Reference Map

| Runtime use | Current prefab authority |
|---|---|
| Sein-A projectile | `NewRunScene` `EffectManager` mapping to `Sein_A.prefab` |
| Sein-B projectile | `NewRunScene` `EffectManager` mapping to `Sein_B.prefab` |
| Sein-C projectile | `NewRunScene` `EffectManager` mapping to `Sein_B.prefab` |
| Sein-C delayed impact | `skills_projectile.csv` `sein-c.skill_effect_prefab_path=Sein_C.prefab` |
| Sein-A master 2 hit explosion | projectile Trigger row `sein-a-master2-hit-explosion` -> `Sein_A_Master-2.prefab` |
| Sein-C master 1 residual zone | projectile choice graph `sein-c-master-1` `EffectVisual` -> `Sein_C_Master_1.prefab` |
| Sein-C master 2 contact visual | projectile choice graph `sein-c-master-2` `EffectVisual` -> `Sein_C_Master-2.prefab` |
| Sein-D base zone | `skills_area_attack.csv` `sein-d.skill_effect_prefab_path=Sein_D.prefab` |
| Sein-D master 2 residual zone | area choice graph `sein-d-master-2` `EffectVisual` -> `Sein_D_Master_2.prefab` |
| Sein-E base deployments | `skills_single_attack.csv` `sein-e.skill_effect_prefab_path=Sein_E.prefab` |
| Sein-E master 2 zone | single-attack choice graph `sein-e-master-2` `EffectVisual` -> `Sein_D.prefab` |

No active Sein F-J row or scene mapping references a skill/status prefab or runtime visual. F-J are not visual migration targets in current data.

## Existing Runtime Representation Boundary

Current shared runtime visual representation can preserve:

- one sprite;
- one animator controller;
- one uniform initial scale;
- one sorting order;
- one root `BoxCollider2D` with authored local size and default zero offset.

It can represent every inspected Sein prefab hierarchy because all nine prefabs are single-root and use one box collider.

Current authoring/execution still cannot preserve the complete Sein flow without small extensions:

- Sein-E runtime visuals do not currently participate in prefab-dependent multi-deployment detection or line-style presentation;
- Sein-C needs two simultaneous visual roles: `Sein_B` as the flying projectile and `Sein_C` as the delayed impact. The skill owns only one `RuntimeVisual`, while impact remains prefab-only.

Prefab root local positions are not runtime authority. Executors instantiate at resolved origin, impact center, zone center, event target, or deployment center.

## Decision Summary

| Target | Decision | Reason |
|---|---|---|
| Sein A | Easy runtime conversion | Single root, one sprite, one centered box. Projectile executor already owns rotation, movement, trigger state, and collision. |
| Sein B | Easy runtime conversion | Same single-root sprite/box representation as A. Burst cadence remains runtime-owned. |
| Sein C projectile | Convert from `Sein_B` runtime values | Current scene mapping proves the flying visual is `Sein_B`, not `Sein_C`. Keep delayed-impact role separate. |
| Sein C delayed impact | Add shared runtime impact visual contract, or retain prefab until approved | `InGameProjectileActor` currently accepts only `impactEffectPrefab`; using the skill's one runtime visual would replace the projectile visual instead. |
| Sein A master 2 | Easy Trigger runtime conversion | Trigger runtime already supports a runtime collider. Author size only; offset stays zero. |
| Sein C master 1 | Easy graph runtime conversion | Persistent zone keeps collider-size authority while its offset is intentionally normalized to zero. |
| Sein C master 2 | Easy graph runtime visual conversion without hitbox | Current `EventTarget` damage is resolved before transient prefab spawning. Adding a runtime hitbox would change it to collider-based targeting. |
| Sein D | Easy runtime conversion | Current zone actor uses the collider for tick hit detection. Preserve size and center it at zero offset. |
| Sein D master 2 | Easy graph runtime conversion | Persistent residual zone keeps collider-size authority with zero offset. |
| Sein E | Convert only after shared mapper/executor support | Current multi-deployment detection requires a prefab and current line-style transform logic skips runtime visuals. Both must accept runtime hitboxes first. |
| Sein E master 2 | Easy graph runtime conversion | It reuses the centered Sein-D runtime visual/collider contract for each deployment zone. |
| Sein F-J | No target | No active visual prefab/runtime reference exists. Adding visuals would be a new feature. |

## Exact Runtime Values

### Runtime Offset Policy

- Do not add `runtime_hitbox_offset_x/y` columns or `RuntimeEffectVisual` offset params.
- Executors place runtime roots directly at projectile origins, event centers, zone centers, and deployment centers; no separate object position offset is authored.
- Every runtime `BoxCollider2D.offset` remains `(0,0)`.
- Preserve inspected collider sizes. Do not copy current prefab collider offsets into runtime data.
- This is an intentional user-authorized normalization, not exact spatial parity with the current non-zero legacy prefab offsets.

### Sein A And Sein B Projectile

Both current prefabs serialize the same values:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sein_Shoot.png
runtime_visual_scale=0.7869
runtime_visual_sorting_order=0
runtime_hitbox_size_x=1.86
runtime_hitbox_size_y=1.0006348
```

No animator controller is present.

Owner rows: `sein-a` and `sein-b` in `base/projectile/skills_projectile.csv`.

After runtime parity verification, remove the `NewRunScene` `EffectManager` mappings for `sein-a` and `sein-b`.

### Sein C Projectile

The current flying visual comes from the scene `sein-c -> Sein_B.prefab` mapping. Use the same runtime values as Sein B above on the `sein-c` base projectile row.

Do not use `Sein_C.prefab` values for the projectile root. `InGameSkillDefinitionMapper` maps `sein-c.skill_effect_prefab_path` to `ImpactEffectPrefab`, and `InGameProjectileActor` spawns it after the delay.

After projectile parity verification, remove only the scene `sein-c` mapping. Retain `sein-c.skill_effect_prefab_path` until the separate impact runtime visual step is complete.

### Sein C Delayed Impact

Current `Sein_C.prefab`:

```csv
runtime_impact_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/B-1.png
runtime_impact_visual_animator_controller_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/B-1.controller
runtime_impact_visual_scale=1
runtime_impact_visual_sorting_order=0
```

Its prefab box is size `(4.59, 3.5115628)`, but inspected `InGameProjectileActor.ResolveImpact()` applies delayed impact damage through `ApplyAreaTick` and the authored radius, not through that visual collider. Do not add an impact runtime hitbox unless later code gives it an explicit gameplay responsibility.

Required shared implementation:

1. Add optional projectile impact runtime visual fields to the active projectile CSV/runtime model.
2. Carry a separate `RuntimeSkillVisualSpec` into `ProjectileSkillData` / `InGameProjectileActor` without replacing the projectile `RuntimeVisual`.
3. In `ResolveImpact()`, prefer the runtime impact visual and fall back to `impactEffectPrefab`.
4. Preserve animation-derived lifetime behavior.
5. Clear `sein-c.skill_effect_prefab_path` only after impact parity verification.

If the user does not approve this new shared contract, retain `Sein_C.prefab`; the rest of Sein can migrate independently.

### Sein A Master 2

Current `Sein_A_Master-2.prefab`:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
runtime_hitbox_size_x=1.7675297
runtime_hitbox_size_y=1.5542119
```

Owner: `triggers/projectile/projectile_skill_triger.csv` row `sein-a-master2-hit-explosion`.

Author the existing runtime sprite/controller/scale/sorting/hitbox-size fields, then clear `skill_effect_prefab_path` after verification. Preserve `hit_target_count=global`; Trigger runtime still resolves hits through the centered runtime-created collider.

### Sein C Master 1

Current `Sein_C_Master_1.prefab`:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/C-1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/C-1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
runtime_hitbox_size_x=3.8611279
runtime_hitbox_size_y=2.2587402
```

Owner: projectile choice graph `sein-c-master-1`, Effect graph index `0`.

Replace its `EffectVisual` node with the existing `RuntimeEffectVisual`. Preserve `OnExpire`, duration `1.5`, radius `1.2`, and tick interval `0.5`; the centered runtime collider remains the persistent zone's hit authority.

### Sein C Master 2

Current visual assets match Sein-A master 2:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
```

Owner: projectile choice graph `sein-c-master-2`, Effect graph index `0`.

Replace `EffectVisual` with `RuntimeEffectVisual`, but leave hitbox size blank. Current graph targets the single `EventTarget` on `OnHit`; the prefab collider is only part of the transient visual. Adding runtime hitbox values would activate `TryExecuteRuntimeHitboxDamageEffect` and change damage targeting.

### Sein D

Current `Sein_D.prefab`:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/C-1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/C-1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
runtime_hitbox_size_x=3.2719479
runtime_hitbox_size_y=1.7052673
```

Owner: `base/area_attack/skills_area_attack.csv` row `sein-d`.

Author these values through the existing base area runtime visual fields and keep radius `3.2` as fallback only. The centered runtime-created box remains the normal tick hit authority. Clear `skill_effect_prefab_path` after collider-boundary verification.

### Sein D Master 2

Current `Sein_D_Master_2.prefab`:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/C-1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/C-1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
runtime_hitbox_size_x=3.593319
runtime_hitbox_size_y=1.8302448
```

Owner: area choice graph `sein-d-master-2`, Effect graph index `0`.

Replace `EffectVisual` with the existing `RuntimeEffectVisual`. Preserve `OnExpire`, duration `3`, radius `3.2`, and tick interval `0.5`.

### Sein E

Current `Sein_E.prefab`:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/E-1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Sein/SkillEffect/Sprite/E-1.controller
runtime_visual_scale=0.22610311
runtime_visual_sorting_order=0
runtime_hitbox_size_x=18.27921
runtime_hitbox_size_y=13.082982
```

Owner: `base/single_attack/skills_single_attack.csv` row `sein-e`.

Required shared implementation before clearing its prefab:

1. Let `InGameSkillDefinitionMapper` treat a valid runtime hitbox as visual/hitbox authority when deciding `useMultiDeployment`; do not require `source.SkillEffectPrefab != null`.
2. Let `SingleAttackSkillExecutor` apply `ConfigureMultiDeploymentPrefabVisual(...)` to runtime-created line-style deployments too. Current code gates that transform with `!hasRuntimeVisual`.
3. Preserve deployment count `3`, trait-4 bonus deployment, nearest-distinct target allocation, repeat fallback, runtime collider damage, and current line length/width orientation.
4. Author the existing runtime visual and hitbox-size fields; collider offset remains zero.
5. Clear `skill_effect_prefab_path` only after manual and auto multi-deployment verification.

### Sein E Master 2

Its graph references `Sein_D.prefab`, so use the exact Sein-D runtime values rather than creating a new visual family.

Owner: single-attack choice graph `sein-e-master-2`, Effect graph index `0`.

Replace `EffectVisual` with the existing `RuntimeEffectVisual`. Preserve `OnDeploymentCast`, duration `3`, radius `3.2`, tick interval `0.5`, and per-deployment zone spawning. Effect graph index `1` remains a status-only presence zone and needs no visual node.

## Required Shared Runtime Changes

No offset authoring change is required. Implementation must not add offset columns or graph params. Explicit user approval is still required for these shared runtime changes:

1. Extend shared SingleAttack mapping/execution so runtime hitboxes retain Sein-E multi-deployment and line-style transforms.
2. For complete removal of `Sein_C.prefab` runtime dependency, add a separate projectile impact runtime visual contract. Keep this optional/contained if user wants a staged migration.

## Recommended Migration Order

1. Convert Sein A and B projectiles with centered runtime colliders, then remove their scene mappings after verification.
2. Convert the Sein C flying projectile from the current Sein-B scene values; keep the delayed-impact prefab during this phase.
3. Convert Sein C master 2 as a visual-only `RuntimeEffectVisual` with no hitbox.
4. Convert Sein A master 2 with its exact hitbox size and zero offset.
5. Convert collider-authoritative zones: Sein C master 1, Sein D, Sein D master 2, and Sein E master 2. Preserve sizes; center every box at zero offset.
6. Add shared Sein-E runtime multi-deployment/line-style support, then convert Sein E with zero object/collider offset.
7. If approved, add the separate projectile impact runtime visual contract and convert Sein C delayed impact.
8. Remove the remaining CSV/scene prefab references only after each target passes user Play Mode verification. Keep prefab assets on disk.

## Compatibility Constraints

- Preserve all current damage, status, cooldown, reload, burst, delay, duration, tick cadence, target selection, deployment count, and Trigger event gates.
- Preserve exact collider sizes where the collider is current damage authority. Normalize every runtime object/collider offset to `(0,0)` by user decision.
- Do not author runtime hitboxes for visual-only effects such as Sein C master 2.
- Preserve Sein C's current split: `Sein_B` is the flying projectile visual; `Sein_C` is the delayed impact visual.
- Preserve Sein-E manual/auto center allocation, line orientation, length/width scaling, and unlimited hits per deployment collider.
- Keep radius values as current fallback/tuning authority; do not replace collider-owned hit detection with radius unless the instantiated runtime object has no collider.
- Do not add runtime position or rotation columns. Executors own origin, direction, event center, zone center, and deployment center.
- Do not add a universal lifetime field. Projectile, impact, transient effect, zone, Trigger, and SingleAttack paths already own lifetime differently.
- Do not delete or edit prefab assets during migration.

## Risks And Containment

- Zero-offset normalization shifts some hitbox centers relative to current serialized prefabs. Containment: treat this as an intentional behavior change and verify each centered boundary in Play Mode before removing fallback references.
- Adding hitbox values to Sein C master 2 changes `EventTarget` damage into collider-area damage. Containment: runtime visual only, hitbox blank.
- Clearing Sein-E prefab before mapper/executor changes collapses three deployments to one and removes line-style scaling. Containment: runtime extension and tests before CSV reference removal.
- Reusing the skill `RuntimeVisual` for Sein C impact replaces the flying projectile visual. Containment: separate impact contract or retained prefab.
- Scene mapping removal too early removes A/B/C fallback visuals. Containment: remove mapping only after runtime catalog validation and user Play Mode parity.

## Acceptance Criteria

- Sein A and B create runtime projectiles with sprite `Sein_Shoot.png`, scale `0.7869`, exact box size, zero object/collider offset, current burst/magazine behavior, and no scene-prefab instantiation.
- Sein C flies with the same runtime representation as current `Sein_B.prefab`, then keeps the current delayed impact timing, radius damage, animation, and visual role.
- Sein A master 2 uses a runtime-created animated box with exact size `(1.7675297, 1.5542119)` and zero offset.
- Sein C master 1, Sein D, Sein D master 2, and Sein E master 2 damage only targets overlapping their exact runtime-created zone colliders, with radius fallback still available when no collider exists.
- Sein C master 2 remains single `EventTarget` contact damage and uses runtime sprite/animation without runtime collider targeting.
- Sein E creates 3 base deployments (4 with trait 4), keeps current manual/auto target-center behavior, line rotation/scaling, and exact collider parity.
- No Sein F-J runtime visual is added.
- Active CSV shape/node validation passes with no unknown/missing param errors.
- Runtime and Editor builds pass with 0 errors.
- Unity-MCP refresh/compile/console checks report no new errors. Unity-MCP CSV validation/sync reports no catalog errors.
- User Play Mode verification confirms visual, animation, collider boundary, timing, and multi-deployment parity.

## Verification Expected From Code Builder

- Record exact changed CSV headers/rows and graph nodes.
- Prove no Sein migration table or graph adds offset authoring fields.
- Prove every created Sein runtime root is placed at the executor-resolved center and every runtime collider resolves to offset `(0,0)`.
- Prove Sein-E runtime visuals enter the same multi-deployment and line-style transform path as the retained prefab path.
- Prove Sein C projectile and impact visuals remain separate authorities.
- Run source CSV field-count/node validation, runtime and Editor builds, Unity-MCP refresh/compile/console checks, and Unity-MCP CSV validate/sync.
- Stop before Play Mode gameplay verification; that remains user-owned.

## Related Boards

- `boards/MON/SEIN_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md` only when Code Builder is authorized to change active CSV schema/rows.
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md` only when Code Builder changes scene `EffectManager` mappings or runtime asset catalog wiring.

## Task: 2026-07-13 Sein Runtime Visual Migration Implementation

### Task title

Implement Sein A-E skill visuals through runtime-composed sprite, animator, and box-collider objects.

### Goals

- Make Sein A/B/C projectiles, A master 2, C master 1/master 2, D, D master 2, E, and E master 2 prefer runtime-composed visuals.
- Keep Sein C flying and delayed-impact visuals as separate runtime contracts.
- Preserve Sein E multi-deployment and line-style transform behavior on runtime-created hitboxes.
- Keep object position and collider offset at `(0,0)` without adding Sein offset authoring columns or params.

### Constraints

- Role Owner is Code Builder; this is refactoring work and does not use a Skill Blueprint.
- Existing prefab paths and scene mappings remain as fallbacks until user Play Mode parity verification.
- Prefab files are not deleted or edited by this implementation.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and source/build/Unity-MCP verified; user Play Mode parity remains.

### Next Actions

- User verifies Sein A-E visuals, animation timing, centered collider boundaries, Sein C impact timing, and Sein E 3/4-line deployment behavior in Play Mode.
- Remove retained Sein prefab and scene fallback references only after that verification.

### Evidence

- `skills_projectile.csv` now authors Sein A/B/C flying visuals from `Sein_Shoot.png`; Sein C separately authors `B-1.png` and `B-1.controller` through the new optional impact visual fields.
- `skills_area_attack.csv`, `skills_single_attack.csv`, and `projectile_skill_triger.csv` author the inspected Sein D, E, and A-master-2 sprite/controller/scale/box-size values.
- The three relevant choice graph CSVs add `RuntimeEffectVisual` nodes for C master 1, C master 2, D master 2, and E master 2. C master 2 intentionally leaves hitbox size blank.
- No edited Sein CSV header or graph row adds an offset field. `BuildRuntimeVisual(...)` defaults absent offsets to zero; `RuntimeSkillVisualFactory.Create(...)` places the root at the executor-provided position, and `ConfigureHitbox(...)` applies the zero-valued spec offset.
- `SkillDefinition`, CSV parsing/build/asset collection, projectile mapping/data, and `InGameProjectileActor` now carry a separate optional impact runtime visual. `ResolveImpact()` prefers it over the retained impact prefab.
- `InGameSkillDefinitionMapper` accepts runtime hitboxes as Sein-E multi-deployment authority, and `SingleAttackSkillExecutor` applies the existing line-style transform to runtime-created deployments as well as prefab deployments.
- TextFieldParser validation reported matching row counts for all 7 edited CSVs: 42, 32, 43, 50, and three 21-column graph tables.
- `dotnet build Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and the Editor project both passed with 0 errors; only existing MSB3277 reference warnings remained.
- Unity-MCP refreshed/compiled the project, synced `Assets/CSVdata/authoring` to `Assets/Resources/Pakuri/CSVRuntime`, revalidated the catalog, and reported 0 error entries. Play Mode was not started.

### History

- 2026-07-13: User classified the work as Code Builder refactoring and explicitly excluded blueprints.
- 2026-07-13: User fixed all original Sein prefab collider offsets to `(0,0)` and required runtime object position offset `(0,0)`.
- 2026-07-13: Code Builder implemented the runtime visual migration while retaining prefab/scene fallback references for user Play Mode parity verification.
