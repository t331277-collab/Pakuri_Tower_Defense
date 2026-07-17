# Ariel Skill Runtime Visual Migration Plan

## Goal

Convert only Ariel skill visual and hitbox prefabs under `Assets/Prefab/Skill/Ariel` into runtime-created visual/hitbox objects where the current prefab does not need to remain as authored prefab structure.

This plan was originally written as a design handoff. On 2026-07-10, Code Builder implemented the Ariel base/trigger/status runtime visual path from this plan.

## Inspected Evidence

- `Pakuri/Assets/Prefab/Skill/Ariel` contains `Airel_A.prefab`, `ariel-b-trait-4_Skill.prefab`, `Ariel_B.prefab`, `Ariel_C-Buff.prefab`, `Ariel_C.prefab`, `Ariel_D.prefab`, and `Ariel_E.prefab`.
- All seven Ariel skill prefabs have an Animator controller reference.
- `EffectManager.InstantiateSkillPrefab(...)` instantiates prefabs with the runtime position and rotation supplied by the executor, so root prefab `m_LocalPosition` and `m_LocalRotation` are not authoritative runtime data.
- `ProjectileSkillExecutor` spawns projectiles at the caster origin and direction rotation, then adds `InGameProjectileActor` when missing.
- `InGameProjectileActor` destroys projectile objects by boundary, lifetime, impact, or hit handling. Projectile visual animation length is not the projectile lifetime.
- `ZoneSkillExecutor` and `InGameZoneSkillActor` use skill duration/tick interval for area and field lifetime.
- `BeamSkillExecutor` and `InGameLineAttackActor` use skill duration/tick interval for line/beam lifetime.
- `SingleAttackSkillExecutor.SpawnVisual(...)` destroys transient single-attack visuals by `SkillVisualSpawnUtility.ResolveVisualLifetime(...)`, which reads Animator clip length.
- `Ariel_E.prefab` currently has a non-zero collider offset, but the user identified that as an authoring error and intends the offset to be `0,0`.
- `Ariel_C.prefab` has MonoBehaviour script GUID `e8261e6f2e5fac44da64da2b23939e9a`; a prior scan did not find a matching `.meta` under `Pakuri/Assets`, so this prefab needs a missing-script check before conversion.
- Current Ariel effect-owned rows already use normalized node CSVs. For example, `single_attack_skill_node_params.csv` references `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab`, `Assets/Prefab/Skill/Ariel/Ariel_C.prefab`, and `Assets/Prefab/Skill/Ariel/Ariel_B.prefab`. Those existing node references are not the target of the base/trigger CSV column migration in this plan.

## Responsibility Boundary

- Base skill-owned visual/hitbox data belongs in the matching `base/*/skills_*.csv`.
- Trigger-owned visual/hitbox data belongs in the matching `triggers/*/*_skill_triger.csv`.
- Do not add new runtime visual/hitbox columns to node CSVs for this Ariel migration.
- Existing node-owned effect prefab references are outside the base/trigger/status CSV column migration. They are explicit node-owned effect visuals, not the base/trigger prefab fallback path.
- Keep hard-to-represent prefabs as prefabs. `Rin_E.prefab`-style authored structures with named core hitboxes are not the target of this Ariel-only pass.

## Values Not To Add By Default

Do not add these as required CSV fields for Ariel:

- `local_position_x`
- `local_position_y`
- `local_rotation_z`
- `visual_lifetime_seconds`
- `hitbox_offset_x`
- `hitbox_offset_y`

Reason:

- Runtime executors already decide spawn position and rotation.
- Projectile, area, and line attack lifetimes are owned by runtime skill duration or projectile rules, not animation length.
- Single-attack transient visual lifetime can be resolved from Animator clip length.
- Ariel hitbox offset can default to `0,0`; `Ariel_E.prefab` should be corrected to offset `0,0` instead of preserving the current wrong offset.

## Minimal Runtime Visual Fields

Use these fields where a base skill or trigger owns the visual directly:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_visual_sorting_order
runtime_hitbox_size_x
runtime_hitbox_size_y
```

Defaults:

- `runtime_visual_scale`: `1`
- `runtime_visual_sorting_order`: `0`
- `runtime_hitbox_offset_x`: omitted, treated as `0`
- `runtime_hitbox_offset_y`: omitted, treated as `0`
- Runtime hitboxes are `BoxCollider2D` when `runtime_hitbox_size_x/y` are positive.
- Runtime hitbox trigger state is code-owned: projectile visuals pass `hitboxIsTrigger=true`; single-attack, trigger area/line, attached/status visual paths default to `false`.

Add optional offset columns only when a future prefab has a deliberate non-zero hitbox offset:

```csv
runtime_hitbox_offset_x
runtime_hitbox_offset_y
```

## Lifecycle Policy

Do not encode lifetime as a universal visual field.

- Projectile visuals follow projectile actor lifetime, hit, impact, and boundary behavior.
- Area/field visuals follow `active_duration_seconds` and tick interval.
- Line/beam visuals follow active duration and tick interval.
- Attached status visuals follow status duration.
- Single-attack transient visuals may use Animator clip length through `SkillVisualSpawnUtility.ResolveVisualLifetime(...)`.

If Code Builder needs an explicit policy later, add an enum-like optional field:

```csv
runtime_visual_lifetime_policy
```

Allowed values:

- `ProjectileLifecycle`
- `SkillDuration`
- `AnimationClip`
- `AttachedStatusDuration`

For Ariel, this can mostly be inferred from skill runtime kind and execution path, so do not add this field unless implementation evidence shows inference is ambiguous.

## Ariel Prefab Conversion Targets

### Ariel A: `Airel_A.prefab`

Current role:

- Base projectile visual/hitbox for `ariel-a`.
- Scene `EffectManager` currently maps `ariel-a` to this prefab.
- Prefab has `InGameProjectileActor`, `SpriteRenderer`, `Animator`, and `BoxCollider2D`.

CSV owner:

- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/projectile/skills_projectile.csv`

Runtime fields:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 15일 오후 05_37_22-Photoroom 1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 15일 오후 05_37_22-Photoroom 1.controller
runtime_visual_scale=0.4817
runtime_hitbox_size_x=1.13
runtime_hitbox_size_y=1.32
```

Implementation note:

- Runtime factory creates `GameObject + SpriteRenderer + Animator + BoxCollider2D`.
- `ProjectileSkillExecutor` already adds `InGameProjectileActor` when missing.
- Projectile runtime visual creation passes `hitboxIsTrigger=true` in code.

### Ariel B: `Ariel_B.prefab`

Current role:

- Shield/attached visual for `ariel-b` and Ariel E shield effect-owned node.
- Prefab has `InGameAttachedSkillEffectActor`, `SpriteRenderer`, and `Animator`.
- No collider.

CSV owner:

- Base `ariel-b` visual: `base/buff/skills_buff.csv`
- Existing Ariel E shield node visual can continue to reference the prefab until a separate node-effect visual migration is requested.

Runtime fields:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 15일 오후 05_27_46-Photoroom 1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 15일 오후 05_27_46-Photoroom 1.controller
runtime_visual_scale=0.3431
runtime_visual_sorting_order=-10
```

Implementation note:

- `SkillVisualSpawnUtility.SpawnAttached(...)` already adds `InGameAttachedSkillEffectActor` when missing.
- No hitbox fields are needed.

### Ariel B Trait 4: `ariel-b-trait-4_Skill.prefab`

Current role:

- Trigger visual/hitbox for `ariel-b-trait4-shield-expire`.
- Trigger CSV currently references `Assets/Prefab/Skill/Ariel/ariel-b-trait-4_Skill.prefab`.
- Prefab has `SpriteRenderer`, `Animator`, and `BoxCollider2D`.

CSV owner:

- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/buff/buff_skill_triger.csv`

Runtime fields:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/ariel/SkillEffect/1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/ariel/SkillEffect/1.controller
runtime_visual_scale=0.5832231
runtime_hitbox_size_x=5.85
runtime_hitbox_size_y=5.46
```

Implementation note:

- The trigger path currently treats any trigger with a prefab as a prefab-hitbox trigger.
- After conversion, trigger runtime routes positive `runtime_hitbox_size_x/y` through the runtime BoxCollider2D path.

### Ariel C Buff: `Ariel_C-Buff.prefab`

Current role:

- Attached/status visual used by Ariel C effect-owned nodes.
- Prefab has `SpriteRenderer` and `Animator`.
- No collider.

CSV owner:

- Existing owner is `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/single_attack/single_attack_skill_node_params.csv`.
- This plan does not add runtime visual columns to node CSVs.

Current node param to replace:

```csv
skill_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab
```

Implementation note:

- Do not convert this node-owned prefab reference in the base/trigger CSV migration.
- Convert it only in a separate node-effect visual migration, or keep the prefab.

### Ariel C: `Ariel_C.prefab`

Current role:

- Base `ariel-c` scene visual and Ariel A master-1 explosion / Ariel C master-2 second-wave visual.
- Prefab has `SpriteRenderer`, `Animator`, `BoxCollider2D`, and an unresolved MonoBehaviour GUID.

CSV owner:

- Base `ariel-c` visual: `base/single_attack/skills_single_attack.csv`
- Trigger `ariel-a-master1-last-shot-explosion`: `triggers/projectile/projectile_skill_triger.csv`
- Existing node-owned `ariel-c-master2-second-wave-effect-visual` can remain prefab-backed until a separate node-effect visual migration.

Runtime fields after missing-script audit:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 22일 오전 12_15_03-Photoroom 1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 22일 오전 12_15_03-Photoroom 1.controller
runtime_visual_scale=1.4438237
runtime_hitbox_size_x=3.07
runtime_hitbox_size_y=3.16
```

Implementation note:

- Do not remove the prefab until the unresolved MonoBehaviour GUID is explained or confirmed irrelevant.
- Single-attack transient visuals can use Animator clip length for lifetime.

### Ariel D: `Ariel_D.prefab`

Current role:

- Status-effect visual for `ariel-d`.
- Prefab has `SpriteRenderer` and `Animator`.
- No collider.

CSV owner:

- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/single_attack/skills_single_attack.csv`

Current field:

```csv
status_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_D.prefab
```

Runtime fields:

```csv
runtime_status_visual_sprite_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 22일 오전 12_25_00-Photoroom 2.png
runtime_status_visual_animator_controller_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 22일 오전 12_25_00-Photoroom 2.controller
runtime_status_visual_scale=0.1993
```

Implementation note:

- If Code Builder prefers a single naming scheme, use `runtime_visual_*` for both skill and status visuals, with an additional `runtime_visual_anchor=StatusTarget`.

### Ariel E: `Ariel_E.prefab`

Current role:

- Base `ariel-e` single-attack visual/hitbox.
- Prefab has `SpriteRenderer`, `Animator`, and `BoxCollider2D`.
- Current collider offset is non-zero, but user identified this as incorrect and wants `0,0`.

CSV owner:

- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/single_attack/skills_single_attack.csv`

Runtime fields:

```csv
runtime_visual_sprite_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 22일 오전 12_31_35-Photoroom 1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/ariel/SkillEffect/ChatGPT Image 2026년 5월 22일 오전 12_31_35-Photoroom 1.controller
runtime_visual_scale=0.72071654
runtime_hitbox_size_x=24.060738
runtime_hitbox_size_y=12.51
```

Implementation note:

- Do not copy the current prefab collider offset.
- Runtime factory should default hitbox offset to `0,0`.

## Suggested CSV Column Placement

### `base/projectile/skills_projectile.csv`

Add for `ariel-a`:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_hitbox_size_x
runtime_hitbox_size_y
```

### `base/buff/skills_buff.csv`

Add for `ariel-b`:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_visual_sorting_order
```

### `base/single_attack/skills_single_attack.csv`

Add for `ariel-c` and `ariel-e`:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_hitbox_size_x
runtime_hitbox_size_y
```

Add for `ariel-d` status visual, either as status-specific fields:

```csv
runtime_status_visual_sprite_path
runtime_status_visual_animator_controller_path
runtime_status_visual_scale
```

or as generic fields plus an anchor:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_visual_anchor
```

### `triggers/buff/buff_skill_triger.csv`

Add for `ariel-b-trait4-shield-expire`:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_hitbox_size_x
runtime_hitbox_size_y
```

### `triggers/projectile/projectile_skill_triger.csv`

Add for `ariel-a-master1-last-shot-explosion` if `Ariel_C.prefab` is converted:

```csv
runtime_visual_sprite_path
runtime_visual_animator_controller_path
runtime_visual_scale
runtime_hitbox_size_x
runtime_hitbox_size_y
```

## Runtime Implementation Surface

Code Builder should implement these pieces:

1. Add source CSV parsing fields for runtime visual and hitbox data on skill rows and trigger rows.
2. Add runtime data types such as `RuntimeSkillVisualSpec` and `RuntimeSkillHitboxSpec`.
3. Add a runtime factory that creates:
   - `GameObject`
   - `SpriteRenderer`
   - `Animator` with runtime controller
   - optional `BoxCollider2D`
   - actor component added by the existing executor path where possible
4. Update projectile, single-attack, zone/field, line/beam, attached visual, and trigger-hitbox execution paths to use runtime specs when prefab is absent.
5. For Ariel rows with `runtime_visual_*` data, skip scene `EffectManager` prefab resolution and `skill_effect_prefab_path` fallback at execution time.
6. Keep Ariel prefab assets on disk for reference/parity checks, but do not instantiate them from the converted base/trigger/status execution paths.

## 2026-07-10 Implementation Notes

- Added shared `RuntimeSkillVisualSpec` and `RuntimeSkillHitboxSpec` data definitions.
- Added `RuntimeSkillVisualFactory` to create runtime `GameObject + SpriteRenderer + Animator + optional BoxCollider2D`.
- Extended runtime CSV parsing and asset cataloging for `runtime_visual_sprite_path`, `runtime_visual_animator_controller_path`, `runtime_visual_scale`, `runtime_visual_sorting_order`, `runtime_hitbox_size_x`, and `runtime_hitbox_size_y`.
- Removed the earlier shape and trigger-state columns from the implemented CSV/code path because all current runtime hitboxes use BoxCollider2D and trigger state is determined by the executor path.
- Extended the runtime asset catalog to include `RuntimeAnimatorController` entries.
- Updated projectile, buff/shield attached visual, single-attack hitbox/visual, trigger area/line visual, and status-effect visual paths to prefer runtime visual specs.
- Updated Ariel rows in `skills_projectile.csv`, `skills_buff.csv`, `skills_single_attack.csv`, `buff_skill_triger.csv`, and `projectile_skill_triger.csv`.
- Cleared Ariel D `status_effect_prefab_path` and the converted Ariel trigger `skill_effect_prefab_path` values so those runtime paths do not load the old Ariel prefabs.
- Ariel prefabs under `Assets/Prefab/Skill/Ariel` were not deleted.

## Acceptance Criteria

- Ariel A projectile appears animated, moves, collides, and destroys by projectile rules without `Airel_A.prefab`.
- Ariel B shield visual appears animated and follows targets without `Ariel_B.prefab`.
- Ariel B trait 4 shield-expire trigger applies the same hit area and visual without `ariel-b-trait-4_Skill.prefab`.
- Ariel C buff node-owned visual remains prefab-backed unless a separate node-effect visual migration is approved.
- Ariel C base/master visuals are not migrated until the unresolved MonoBehaviour on `Ariel_C.prefab` is resolved.
- Ariel D status visual appears animated without `Ariel_D.prefab`.
- Ariel E visual/hitbox uses offset `0,0`, not the current incorrect prefab offset.
- Single-attack transient visuals still use animation clip length for visual lifetime.
- Projectile, area/field, and line/beam visuals do not use animation length as gameplay lifetime.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passes.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passes.
- Unity-MCP CSV sync/validation returns no CSV/runtime asset errors after implementation.
- User Play Mode verification confirms visual parity and gameplay collision parity.

## Related Boards

- `boards/MON/ARIEL_MONSTER.md`
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`
