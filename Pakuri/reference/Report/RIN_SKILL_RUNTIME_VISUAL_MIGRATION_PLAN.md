# Rin Skill Runtime Visual Migration Plan

## Goal

Convert approved Rin skill prefabs to Ariel-style runtime visual composition without changing gameplay collision or named-hitbox behavior. Keep Rin D base prefab-backed by user decision, and extend the shared runtime hitbox spec only as far as needed to preserve Rin D master 1's authored collider offset.

Keep the prefab assets on disk during migration. Prefab deletion is outside this plan.

## Inspected Evidence

- Unity-MCP found seven Rin skill prefabs: `Rin_A`, `Rin_B`, `Rin_C`, `Rin_D`, `Rin_D_master_1`, `Rin_E`, and `Rin_F`.
- Unity-MCP hierarchy inspection found A/B/C/D/D-master1/F are single-root prefabs. `Rin_E.prefab` alone has the child `Rin_E/CoreHitBox`.
- `RuntimeSkillVisualFactory` creates one runtime root with `SpriteRenderer`, optional `Animator`, and at most one root `BoxCollider2D`. Its collider offset is always `Vector2.zero`.
- `ProjectileSkillExecutor`, buff attached visuals, `BeamSkillExecutor`, `SingleAttackSkillExecutor`, and `SkillTriggerRuntime` already prefer `RuntimeVisual` over prefab fallback when a runtime sprite is present.
- Current scene `EffectManager` mappings own Rin A-D base prefabs. Active CSV rows additionally reference Rin A for A master 2, Rin D master 1 for its kill burst, Rin E for the base skill, and Rin F for two follow-up triggers.
- No Rin G-J row contains a skill/status prefab path or runtime visual sprite path. They are not prefab-visual migration targets in the current data.

## Existing Runtime Representation Boundary

The current shared runtime visual model can preserve:

- one sprite;
- one animator controller;
- uniform scale;
- sorting order;
- one zero-offset root `BoxCollider2D`.

It cannot currently preserve:

- a deliberate non-zero collider offset;
- multiple colliders with different transforms;
- a named collider child such as `CoreHitBox`.

Root prefab local positions are not runtime authority because the executors instantiate at their resolved runtime position.

## Decision Summary

| Target | Decision | Reason |
|---|---|---|
| Rin A | Easy runtime conversion | Single root with sprite and zero-offset box collider; projectile executor adds its actor and sets the runtime collider as trigger. |
| Rin B | Easy runtime conversion | Single root with sprite and animator; no collider. Existing buff/attached runtime path already supports it. |
| Rin C | Easy runtime conversion | Single root with sprite and animator; no collider. `InGameLineAttackActor` owns line damage geometry and stretches the sprite to resolved length/width. |
| Rin D | Keep prefab | User selected the base Rin D visual to remain prefab-backed. Its current collider is not damage authority, but no base visual migration is requested. |
| Rin D master 1 | Runtime conversion with shared offset extension | The kill-burst Trigger uses the prefab collider as damage authority. Preserve its size and non-zero offset by extending `RuntimeSkillHitboxSpec` and the single-attack Trigger CSV/runtime mapping. |
| Rin E | Keep prefab | It requires root and named child `CoreHitBox` colliders. Current runtime visual spec can create only one unnamed root collider. |
| Rin F | Easy after small CSV exposure | Single root with sprite, animator, and zero-offset box collider. Trigger runtime already supports runtime visuals/hitboxes, but `passive_skill_triger.csv` does not currently expose the runtime visual columns. |
| Rin G-J | No conversion target | Current rows have no prefab/status visual references. Adding new visuals would be a new feature, not prefab-parity migration. |

## Per-Skill Runtime Values

### Rin A

Current prefab:

- sprite: `Assets/Image/Monster/Rin/Legacy/Rin_Shoot.png`
- animator: none
- scale: `1`
- box size: `(1.72, 1.72)`
- box offset: `(0, 0)`

Owner:

- base: `base/projectile/skills_projectile.csv`
- retained compatibility reference: `rin-a-master-2` in `choices/projectile/skill_choices_projectile.csv`
- scene fallback: `NewRunScene` `EffectManager`

Runtime values:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Rin/Legacy/Rin_Shoot.png
runtime_visual_scale=1
runtime_hitbox_size_x=1.72
runtime_hitbox_size_y=1.72
```

After parity verification, clear the A master-2 prefab path and the scene fallback. The current A master-2 graph uses shared additional/chain damage operations rather than a separate authored visual hierarchy.

### Rin B

Current prefab:

- sprite: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/1.png`
- controller: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/1.controller`
- scale: `1`
- collider: none

Owner: `base/buff/skills_buff.csv`, with the current scene mapping as fallback.

### Rin C

Current prefab:

- sprite: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/ChatGPT Image 2026년 5월 8일 오후 06_12_43-Photoroom 1.png`
- controller: the matching `.controller` beside that sprite
- scale: `1`
- collider: none

Owner: `base/line_attack/skills_line_attack.csv`, with the current scene mapping as fallback.

No hitbox fields are needed. Line damage uses the actor's resolved line length and width rather than a prefab collider.

### Rin D

Current prefab:

- sprite: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/2-1.png`
- controller: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/2-1.controller`
- scale: `1`
- authored box: size `(4.658527, 1.5578728)`, offset `(0.20213056, -2.0774589)`

Owner: `base/single_attack/skills_single_attack.csv`, with the current scene mapping as fallback.

The current `rin-d` row has blank `hit_target_count` and radius `0`. The mapper selects limited-target logic and leaves `UsePrefabHitbox=false`; therefore the authored collider is not current damage authority. Even so, the user selected Rin D base to remain prefab-backed, so do not add runtime visual values or remove its scene `EffectManager` mapping in this migration.

### Rin D Master 1

Current prefab:

- sprite: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/2-5-1.png`
- animator: none
- scale: `1`
- box size: `(3.9373517, 3.788869)`
- box offset: `(0.53632426, -0.41973162)`

Owner: `triggers/single_attack/single_attack_skill_triger.csv` row `rin-d-master1-kill-burst`.

Convert this Trigger visual/hitbox at runtime by adding optional offset support. Required values:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Rin/Legacy/Effect_Sprite/2-5-1.png
runtime_visual_scale=1
runtime_hitbox_size_x=3.9373517
runtime_hitbox_size_y=3.788869
runtime_hitbox_offset_x=0.53632426
runtime_hitbox_offset_y=-0.41973162
```

Implementation surface:

1. Add `Offset` to `RuntimeSkillHitboxSpec`; default remains `(0,0)` for all existing rows.
2. Parse optional `runtime_hitbox_offset_x/y` for skill and Trigger runtime visual specs.
3. Make `RuntimeSkillVisualFactory.ConfigureHitbox(...)` use the spec offset instead of unconditional `Vector2.zero`.
4. Add runtime sprite/controller/scale/sorting/hitbox size/hitbox offset columns to `triggers/single_attack/single_attack_skill_triger.csv`.
5. Fill those values on `rin-d-master1-kill-burst`, then clear its `skill_effect_prefab_path` after parity validation.

No animator controller value is needed because `Rin_D_master_1.prefab` has no Animator.

### Rin E

Current prefab structure:

- root sprite: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/4-1.png`
- root controller: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/4-1.controller`
- root box: size `(6.0808043, 2.1332593)`, offset `(0.18654752, -1.5973132)`
- child: `CoreHitBox`, local position `(0, -1.42, 0)`, box size `(1, 1)`, offset `(0,0)`

Owner: `base/single_attack/skills_single_attack.csv`.

Rin-E trait 4 and master 1 graph nodes explicitly name `CoreHitBox`. Converting E with the current one-box runtime spec would remove that name-based distinction, so the prefab is the safer representation.

### Rin E Pre-Migration Blocker

The active `rin-e` row has radius `2.4` and blank `hit_target_count`. `InGameSkillDefinitionMapper` therefore leaves `UsePrefabHitbox=false`, while `SingleAttackSkillExecutor` resolves named `CoreHitBox` colliders only inside the prefab-hitbox branch.

This means the inspected active code/data path does not currently reach the named CoreHitBox logic even though the prefab and graph nodes exist. Code Builder must verify and correct this existing routing issue before any E visual migration is considered. The correction is separate from visual conversion and must preserve the intended base-area and core-only effects.

### Rin F

Current prefab:

- sprite: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/5-1.png`
- controller: `Assets/Image/Monster/Rin/Legacy/Effect_Sprite/ChatGPT Image 2026년 5월 24일 오후 12_53_20-Photoroom 2_0.controller`
- scale: `0.4258`
- box size: `(2.18, 2.35)`
- box offset: `(0,0)`

Owner: `triggers/passive/passive_skill_triger.csv` rows `rin-f-followup` and `rin-f-followup-trait2`.

The common parser and Trigger runtime already understand runtime visual fields. The passive Trigger CSV header must expose:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_visual_sorting_order
runtime_hitbox_size_x
runtime_hitbox_size_y
```

Both F rows can then share the same values and clear `skill_effect_prefab_path` after parity verification.

## Recommended Migration Order

1. Convert Rin B and C first because they are sprite/animator-only and do not move gameplay collision authority.
2. Convert Rin A because its zero-offset projectile collider fits the current shared runtime model.
3. Add runtime visual columns to the passive Trigger CSV and convert Rin F.
4. Extend shared runtime hitbox offset support, expose the fields on the single-attack Trigger CSV, and convert Rin D master 1.
5. Keep Rin D base and its scene `EffectManager` mapping prefab-backed.
6. Keep Rin E prefab-backed and first repair/verify the current `UsePrefabHitbox` routing for `CoreHitBox` behavior.
7. Do not create visuals for G-J as part of this parity migration.

## Compatibility Constraints

- Preserve all current skill timings, target selection, damage, status effects, and Trigger cadence.
- Runtime visuals take precedence over scene/CSV prefab fallback, but prefab assets remain on disk until all migrated skills pass user Play Mode verification.
- Do not add position or rotation fields; executors own runtime placement and facing.
- Do not add a universal lifetime field; projectile, attached, line, single-attack, and Trigger paths already own their lifetimes.
- Preserve non-zero authored collider offsets through optional runtime hitbox offset fields; existing rows default to zero.
- Do not represent Rin E as one root collider; its named child is gameplay data, not decoration.

## Acceptance Criteria

- Rin A projectile uses the runtime sprite and zero-offset box, moves/collides through `InGameProjectileActor`, and preserves A master-2 behavior.
- Rin B attached buff visual follows its target and uses the existing animation without `Rin_B.prefab` instantiation.
- Rin C visual is created at runtime, stretches to the actor's resolved line length/width, and preserves line damage/knockback/status behavior.
- Rin D base continues to instantiate `Rin_D.prefab` and keeps its current scene mapping.
- Rin D master 1 kill burst uses a runtime-created sprite and box collider with exact size `(3.9373517, 3.788869)` and offset `(0.53632426, -0.41973162)`.
- Rin F follow-ups use runtime sprite/animator/hitbox values and preserve both base and trait-2 Trigger damage.
- Rin E continues to instantiate its retained prefab during this pass.
- Rin E `CoreHitBox` routing is independently fixed and Play Mode-verified before any later E conversion.
- Runtime and Editor builds pass with 0 errors.
- Unity-MCP CSV sync/validation reports no source/catalog errors.
- User Play Mode verification confirms visual and collision parity.

## 2026-07-13 Implementation Notes

- Code Builder added shared `RuntimeSkillHitboxSpec.Offset` and changed `RuntimeSkillVisualFactory` to apply the authored offset instead of forcing zero.
- Skill and Trigger CSV parsing/build paths now read optional `runtime_hitbox_offset_x/y` values.
- Rin A/B/C base rows now author runtime sprite/controller/scale/hitbox values. Existing scene mappings remain as fallback but runtime visual presence takes precedence.
- Rin F base/trait-2 follow-up Trigger rows now author the same runtime sprite/controller/scale/zero-offset hitbox values.
- Rin D master 1 kill-burst Trigger now authors runtime sprite, exact box size `(3.9373517, 3.788869)`, and exact offset `(0.53632426, -0.41973162)`.
- Rin D base remains unchanged and scene-prefab-backed.
- Rin E remains prefab-backed and now explicitly authors `use_prefab_hitbox=true`. An explicit prefab hitbox with no target-count limit resolves all overlapping targets without changing its target-centered placement.
- Converted Trigger prefab paths and A-D scene mappings remain as fallback/parity evidence until user Play Mode verification. No Rin prefab was deleted or edited.
- Runtime and Editor builds passed with 0 errors; existing assembly conflict warnings remained.
- Unity-MCP `Pakuri/Validate CSV Source Data` loaded the runtime catalog with 5 monsters and reported no validation error.

## Related Boards

- `boards/MON/RIN_MONSTER.md`
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md` only if Code Builder changes Rin-E authoring/runtime routing or Trigger CSV schema.
