# Enemy Skill Node Runtime Handoff

Date: 2026-06-19
Role Owner: Designer handoff for Code Builder
Scope: Enemy active skills, Stage1 plus Stage2

## Goal

Move enemy active skills toward the same target direction used by the monster skill runtime:

- skill body data stays in enemy skill data;
- behavior is expressed through compiled condition/action/modifier nodes;
- action handlers own damage, status, projectile, zone, cooldown/reload, visual, movement, and stat changes;
- prefab use becomes a visual/action payload choice, not the source of behavior.

This handoff includes existing `Assets/Prefab/Enemy/Skill/Stage1` skills. Stage1 should be migrated first because current Stage1 behavior already exists in code and gives a parity baseline before Stage2 work.

## Inspected Evidence

Files and paths inspected:

- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv`
- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv`
- `Pakuri/Assets/Scripts2/InGame/Enemy/EnemyDefinition.cs`
- `Pakuri/Assets/Scripts2/InGame/Enemy/EnemyCombatSystem.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillExecutionPlan.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillPlanActionDispatcher.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`
- `Pakuri/reference/5.enemy/stage-2-enemies.md`
- `Pakuri/Assets/Prefab/Enemy/Skill/Stage1`
- `Pakuri/Assets/Prefab/Enemy/Skill/Stage2`

Important current facts:

- `EnemySkillData.csv` currently has skill body fields such as `skill_id`, `runtime_kind`, `attribute`, damage coefficients, radius, cooldown, projectile speed/lifetime, status id, duration, flat value, movement multiplier, and outgoing damage multiplier.
- Enemy skill cast distance must use the `radius` value authored in `EnemySkillData.csv`. The CSV `radius` value is the source of truth for skill range.
- `EnemySkillData.csv` should not keep an `enemy_scope` gate. Remove `enemy_scope` and let any enemy unit use a skill when its enemy row or future node binding assigns that skill id.
- `stage_one_enemies.csv` binds enemies to `stage_one_skill`, `basic_skill`, passive names, passive ids, and passive values.
- `EnemyDefinition.cs` currently defines `StageOneEnemySkillKind` with Stage1-only values: `Slash`, `ShieldUp`, `AimedShot`, `ShurikenThrow`, `Heal`, `GuardianFlag`, `ChargeCommand`, `SacredSwordWave`.
- `EnemyCombatSystem.cs` currently resolves enemy basic/special skills into `EnemyResolvedSkillData`, then `EnemySkillExecutor.Execute(...)` switches directly on `StageOneEnemySkillKind`.
- `EnemySkillExecutor` currently implements Slash, projectile skills, Heal, ShieldUp, GuardianFlag, and ChargeCommand directly in code.
- Monster runtime already has `SkillExecutionPlan`, action wrappers, and `SkillPlanActionDispatcher`, but enemy skills are not yet compiled into that node/action route.

Current prefab evidence:

Stage1 prefabs:

- `Achor_Skill.prefab`
- `Karin_Skill 1.prefab`
- `Preist_Skill.prefab`
- `Rogue_Skill.prefab`
- `Shield_King_Skill.prefab`
- `Shield_Skill.prefab`
- `Warrior_King_Skill 1.prefab`
- `Warrior_Skill.prefab`

Stage2 prefabs:

- `arsen_Skill.prefab`
- `dark-assassin_Skill.prefab`
- `ethan_Skill.prefab`
- `fire-dragon-slayer.prefab`
- `holy-priest_Skill.prefab`
- `ice-guard_Skill.prefab`
- `lightning-scout_1.prefab`

## Stage1 Inclusion Judgment

Stage1 skills can be included in the same enemy node migration.

Reason:

- Stage1 already has complete runtime behavior in `EnemyCombatSystem.cs`.
- Stage1 already has prefab assets.
- Stage1 behaviors map cleanly to reusable action handler types: area damage, projectile spawn, heal, shield/stat modifier, ally radius selection, and visual spawn.
- Keeping Stage1 on the old switch while Stage2 uses nodes would create two enemy skill execution models. That makes future enemy skill addition and balancing harder.

Recommended migration order:

1. Keep old `EnemySkillExecutor` switch as fallback.
2. Add enemy plan/node compiler and dispatcher path.
3. Migrate Stage1 skills one by one to the node path and compare behavior.
4. Only remove old direct switch paths after Stage1 parity is verified.
5. Add Stage2 using the same node path.

## Target Enemy Skill Structure

Use this target shape for enemy active skills:

- `EnemySkillData.csv`: skill body, identity, cooldown, base coefficients, attribute, radius, basic projectile values.
- `EnemySkillData.csv` must contain the Stage2 skill body rows too, so Stage1 and Stage2 enemy skills are managed from the same enemy skill data file.
- `EnemySkillData.csv` `radius` is the authoritative skill range value. Do not derive enemy skill range from prefab size, enemy attack type, or a separate `range` column for node-backed skills.
- Delete `enemy_scope` from `EnemySkillData.csv`; do not use scope filtering to decide whether a unit can use a skill.
- Enemy node table: per-skill behavior nodes.
- Enemy node params table: key/value payload for nodes.
- Runtime compiler: builds an enemy skill execution plan from skill body plus nodes.
- Enemy skill controller/system: decides when an enemy casts and creates execution request.
- Enemy action dispatcher: executes nodes through handlers.
- Handlers: damage, heal, status, projectile, visual, movement override, stat modifier, shield, chain damage, combat-start trigger.

Do not force enemy skills through monster `UnitSkillController` immediately. Current enemy control lives in `EnemyCombatSystem.cs`, so Builder should add an enemy adapter/controller path first and then converge shared handler code where practical.

## Proposed Data Files

Keep:

- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillData.csv`

Required `EnemySkillData.csv` cleanup/addition:

- Remove `enemy_scope`.
- Keep/add Stage1 and Stage2 enemy skill rows in this file.
- Use `radius` as the range source of truth for every skill row.
- Let unit assignment decide skill usability; do not block a skill because of scope.

Add or route after approval by Builder scope:

- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillNodes.csv`
- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillNodeParams.csv`

Optional only if needed:

- `Pakuri/Assets/CSVdata/authoring/enemy/EnemySkillTriggers.csv`

Suggested node columns:

- `skill_id`
- `node_id`
- `order`
- `node_kind`
- `target_selector`
- `action_op`
- `condition_key`
- `enabled`

Suggested param columns:

- `skill_id`
- `node_id`
- `param_key`
- `param_value`

Use strings already present in monster node/action naming where possible, but do not claim enemy support exists until Builder adds parser/runtime support.

## Proposed Runtime Classes

Minimal Code Builder target:

- `EnemySkillExecutionPlan`
- `EnemySkillExecutionPlanNode`
- `EnemySkillExecutionRequest`
- `EnemySkillPlanCompiler`
- `EnemySkillPlanActionDispatcher`
- `EnemySkillExecutionContext`
- `EnemySkillNodeHandlerRegistry` or equivalent dispatcher switch

Integration point:

- `EnemyCombatSystem` should continue to own enemy tick/cast decision at first.
- Replace direct `EnemySkillExecutor.Execute(...)` with:
  - compile/lookup plan;
  - execute plan if available;
  - fallback to current direct executor if plan is missing.

Later convergence:

- Shared action handlers can be extracted from monster `SkillPlanActionDispatcher`, `SkillMultiEffectExecutor`, and `SkillTriggerRuntime` after enemy parity exists.
- Do not start with a large shared abstraction unless Builder proves enemy and monster contexts can share it without breaking current monster validation.

## Stage1 Mapping

### Slash

Current shape:

- `StageOneEnemySkillKind.Slash`
- direct area/hitbox damage path in `EnemySkillExecutor`
- visual prefab likely `Warrior_Skill.prefab`

Node shape:

- `TargetSelector`: nearest tower or current attack target
- `Action`: `SpawnVisual` or `SpawnHitboxVisual`
- `Action`: `DamageArea`
- Params: radius, physical attribute, attack coefficient, critical flag if needed

### ShieldUp

Current shape:

- direct incoming damage multiplier change
- visual prefab likely `Shield_Skill.prefab`

Node shape:

- `TargetSelector`: self
- `Action`: `ApplyStatModifier`
- Params: `IncomingDamageMultiplier`, duration, value from skill body
- `Action`: `SpawnVisual`

### AimedShot

Current shape:

- projectile path
- visual prefab likely `Achor_Skill.prefab`

Node shape:

- `TargetSelector`: current or nearest tower
- `Action`: `SpawnProjectile`
- Trigger: projectile `OnHit`
- `Action`: `Damage`
- Params: projectile speed, lifetime, attribute, coefficients

### ShurikenThrow

Current shape:

- projectile path
- visual prefab likely `Rogue_Skill.prefab`

Node shape:

- same as AimedShot
- Params differ by projectile speed/lifetime/coefficient

### Heal

Current shape:

- finds lowest-health enemy ally
- heals
- visual prefab likely `Preist_Skill.prefab`

Node shape:

- `TargetSelector`: lowest-health enemy ally in range
- `Action`: `Heal`
- Params: base heal, spell coefficient, radius/range
- `Action`: `SpawnVisual`

### GuardianFlag

Current shape:

- grants shield to nearby enemy allies
- visual prefab likely `Shield_King_Skill.prefab`

Node shape:

- `TargetSelector`: enemy allies in radius
- `Action`: `GrantShield`
- Params: shield amount, duration, radius
- `Action`: `SpawnVisual`

### ChargeCommand

Current shape:

- modifies allied move speed and outgoing damage
- visual prefab likely `Warrior_King_Skill 1.prefab`

Node shape:

- `TargetSelector`: enemy allies in radius
- `Action`: `ApplyStatModifier`
- Params: `MoveSpeedMultiplier`, `OutgoingDamageMultiplier`, duration
- `Action`: `SpawnVisual`

### SacredSwordWave

Current shape:

- projectile path
- visual prefab likely `Karin_Skill 1.prefab`

Node shape:

- `TargetSelector`: current or nearest tower
- `Action`: `SpawnProjectile`
- Trigger: projectile `OnHit`
- `Action`: `Damage`
- Params: physical/holy or selected supported attribute, coefficient, speed, lifetime

Attribute note:

- If current enemy damage supports only one attribute, do not silently invent dual-attribute damage. Choose the existing CSV attribute for parity, or add explicit dual-attribute support only after approval.

## Stage2 Mapping

Source reference: `Pakuri/reference/5.enemy/stage-2-enemies.md`, section `## 2. 모든 적 액티브 스킬`.

Stage2 `EnemySkillData.csv` radius requirements:

| Enemy | Skill | `radius` |
| --- | --- | ---: |
| Fire Dragon Soldier | 화룡 참격 | 2 |
| Lightning Scout | 연쇄 번개 | 7 |
| Ice Guard | 빙결 압박 | 2 |
| Dark Assassin | 어둠 찌르기 | 1.4 |
| Holy Priest | 성룡 치유 | 5 |
| Ethan | 성창 투척 | 14 |
| Drake | 개전 돌진 | 40 |
| Arsen | 위압감 | 40 |

Combat-start skills use very high radius values because they fire as soon as the enemy spawns. `Drake` and `Arsen` therefore use `radius=40` so their start-of-combat behavior can execute immediately.

### Fire Dragon Soldier: 화룡 참격

Reference behavior:

- short frontal fan, 3m
- attack 120% plus spell 40%
- cooldown 5s
- Fire damage

Node shape:

- `TargetSelector`: nearest/current tower
- `Action`: `DamageArea` or `DamageFan`
- `Action`: `SpawnVisual`
- Skill body: `radius=2`, attack coefficient 1.2, spell coefficient 0.4, Fire attribute, cooldown 5
- Prefab: `fire-dragon-slayer.prefab`

### Lightning Scout: 연쇄 번개

Reference behavior:

- one target
- max 2 chains
- spell 120%
- cooldown 5.5s
- Lightning damage
- chains once to nearby different target at 50% damage

Node shape:

- `TargetSelector`: primary tower target
- `Action`: `Damage`
- `Action`: `ChainDamage`
- Skill body: `radius=7`, spell coefficient 1.2, Lightning attribute, cooldown 5.5
- Params: chain count 1, chain multiplier 0.5
- Prefab: `lightning-scout_1.prefab`

New handler likely needed:

- `ChainDamage` or `FindNearbyDifferentTargetThenDamage`

### Ice Guard: 빙결 압박

Reference behavior:

- nearest target
- attack 80% plus spell 40%
- cooldown 6s
- Ice damage
- action speed -20% for 3s

Node shape:

- `TargetSelector`: nearest tower
- `Action`: `Damage`
- `Action`: `ApplyStatus` or `ApplyStatModifier`
- Skill body: `radius=2`, attack coefficient 0.8, spell coefficient 0.4, Ice attribute, cooldown 6
- Params: action speed multiplier or delta, duration 3
- Prefab: `ice-guard_Skill.prefab`

New handler/data likely needed if tower action-speed debuff is not already supported on enemy-to-tower status path.

### Dark Assassin: 어둠 찌르기

Reference behavior:

- nearest target
- attack 160%
- cooldown 6s
- Darkness damage
- critical allowed

Node shape:

- `TargetSelector`: nearest tower
- `Action`: `Damage`
- Skill body: `radius=1.4`, attack coefficient 1.6, Darkness attribute, critical allowed true, cooldown 6
- Prefab: `dark-assassin_Skill.prefab`

### Holy Priest: 성룡 치유

Reference behavior:

- nearby enemy ally one target
- spell 130%
- cooldown 7s
- heal lowest-health nearby enemy
- base heal 80

Node shape:

- `TargetSelector`: lowest-health enemy ally nearby
- `Action`: `Heal`
- Skill body: `radius=5`, base heal 80, spell coefficient 1.3, cooldown 7
- Runtime behavior must use the same target/execute logic as current Stage1 `Heal`.
- Prefab: `holy-priest_Skill.prefab`

### Ethan: 성창 투척

Reference behavior:

- farthest target
- attack 180% plus spell 60%
- cooldown 8s
- physical/lightning spear projectile

Node shape:

- `TargetSelector`: farthest tower
- `Action`: `SpawnProjectile`
- Trigger: projectile `OnHit`
- `Action`: `Damage`
- Skill body: `radius=14`, attack coefficient 1.8, spell coefficient 0.6, cooldown 8, projectile speed/lifetime, supported attribute
- Prefab: `ethan_Skill.prefab`

Attribute note:

- The reference says physical/lightning. If runtime supports only one attribute on enemy damage, Builder must either select a primary attribute in CSV or add explicit multi-attribute support after approval.

User note:

- User mentioned an Ethan-like charge with no prefab. The inspected Stage2 reference names Ethan as a spear throw and Drake as the combat-start charge. If design direction changes and Ethan becomes a charge, use the Drake charge node pattern below and treat prefab as optional/unused.

### Drake: 개전 돌진

Reference behavior:

- combat start
- random tower target
- target max HP 100%
- once per combat
- physical damage plus stun 5s

Node shape:

- `Trigger`: `OnCombatStart`
- `TargetSelector`: random tower
- `Action`: `MovementOverride` or `ChargeToTarget`
- Trigger: movement/collider `OnHit`
- `Action`: `DamageByTargetMaxHealth`
- `Action`: `ApplyStatus`
- Skill body: `radius=40`, Physical attribute
- Params: damage target max HP ratio 1.0, stun duration 5, once per combat true

Prefab policy:

- No prefab is required for this behavior. The moving enemy actor/collider can be the skill carrier.
- Optional `SpawnVisual` can be added later if a prefab is assigned.

New handlers likely needed:

- `OnCombatStart`
- `ChargeToTarget`
- `OnColliderHit`
- `DamageByTargetMaxHealth`
- tower stun status application

### Arsen: 위압감

Reference behavior:

- combat start
- all towers
- once per combat
- tower power -30%

Node shape:

- `Trigger`: `OnCombatStart`
- `TargetSelector`: all towers
- `Action`: `ApplyStatModifier` or `ApplyStatus`
- Skill body: `radius=40`
- Params: tower power multiplier 0.7 or outgoing damage multiplier 0.7, once per combat true
- Prefab: `arsen_Skill.prefab`

New handler/data likely needed if tower power reduction is not already represented as status/stat modifier.

## Prefab Policy

Use prefabs as optional visual/action payloads, not as the definition of skill behavior.

Rules:

- A skill can execute with no prefab if its behavior is movement, stat change, status, or direct damage.
- A prefab can be used only for `SpawnVisual`, projectile body, hitbox body, or zone visual.
- A prefab must not be required to define damage coefficient, status id, cooldown, target rule, or trigger condition.
- Stage1 existing prefabs should be mapped during parity migration.
- Stage2 existing prefabs should be mapped where behavior actually needs visuals/projectiles.
- Drake-style charge can be implemented with no prefab by moving the enemy unit and applying OnHit effects through collision/contact.

## Required Handler Coverage

Existing/currently needed enemy handlers:

- `Damage`
- `DamageArea`
- `SpawnProjectile`
- projectile `OnHit`
- `Heal`
- `GrantShield`
- `ApplyStatModifier`
- `SpawnVisual`

New or likely missing for Stage2:

- `DamageFan`
- `ChainDamage`
- `ApplyStatus` from enemy skill to tower
- `OnCombatStart`
- `ChargeToTarget`
- movement/collider `OnHit`
- `DamageByTargetMaxHealth`
- tower stun
- all-tower selector
- random tower selector
- farthest tower selector

## Code Builder Implementation Plan

1. Confirm exact active data route.
   - Read only current enemy skill CSVs needed for Stage1/Stage2.
   - Keep `EnemySkillData.csv` as skill body source.
   - Add node CSV files only after deciding exact columns.

2. Add enemy node data model and parser.
   - Add runtime CSV model for enemy skill nodes and params.
   - Add validation for missing skill id, missing node handler, invalid target selector, and invalid prefab path.

3. Add enemy execution plan runtime.
   - Create enemy plan/node/request/context classes.
   - Plan lookup should be keyed by `skill_id`.
   - Do not remove `EnemyResolvedSkillData` immediately; use it as compatibility input until all Stage1 skills are migrated.

4. Add dispatcher and handlers.
   - Start with Stage1 parity handlers.
   - Reuse monster handler naming and simple payload style where possible.
   - Keep actual enemy model/tower service calls inside enemy context or handler layer.

5. Integrate with `EnemyCombatSystem`.
   - Current tick/cast decision remains in `EnemyCombatSystem`.
   - When skill is selected, execute enemy plan if present.
   - If no plan exists, fall back to current `EnemySkillExecutor.Execute(...)`.

6. Migrate Stage1.
   - Add nodes for Slash, ShieldUp, AimedShot, ShurikenThrow, Heal, GuardianFlag, ChargeCommand, SacredSwordWave.
   - Verify each skill still works before removing its old switch body.

7. Add Stage2.
   - Add skill body rows for Stage2 active skills to `EnemySkillData.csv` if not already present.
   - Author the requested Stage2 `radius` values in `EnemySkillData.csv`: Fire Dragon Soldier 2, Lightning Scout 7, Ice Guard 2, Dark Assassin 1.4, Holy Priest 5, Ethan 14, Drake 40, Arsen 40.
   - Remove `enemy_scope` from `EnemySkillData.csv` and keep skill usability controlled by enemy skill assignment.
   - Add nodes and params for Fire Dragon Soldier, Lightning Scout, Ice Guard, Dark Assassin, Holy Priest, Ethan, Drake, Arsen.
   - Add new handlers only when a Stage2 skill requires them.

8. Retire old direct execution carefully.
   - Remove direct switch cases only after Stage1 and Stage2 plan validation passes.
   - Keep compact fallback logging during transition so missing node plans are visible.

## Verification Checklist

Required after implementation:

- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`
- Unity-MCP script/source validation for touched scripts.
- Unity skill/enemy data validation menu if available.
- Unity console warning/error check.
- Runtime smoke checks:
  - Stage1 melee skill damages target.
  - Stage1 projectile skill spawns projectile and applies OnHit damage.
  - Stage1 heal targets lowest-health enemy ally.
  - Stage1 shield/buff modifies enemy model and expires.
  - Stage2 chain lightning hits primary and one nearby different target.
  - Stage2 ice skill applies slow.
  - Stage2 charge moves unit and applies OnHit damage/status without prefab dependency.
  - Stage2 combat-start global debuff applies once.

## Stop Conditions

Stop and ask before widening scope if:

- dual-attribute damage is required but current damage model cannot represent it;
- tower stun/action-speed/status application path does not exist and needs new shared status architecture;
- enemy node CSVs require new import/sync pipeline work outside current enemy runtime data scope;
- Builder needs to inspect unrelated monster CSVs or old archive markdown to infer enemy values;
- deleting old direct executor paths would remove behavior before node parity is verified.

## Non-Goals

- Do not refactor all monster skill runtime while implementing enemy nodes.
- Do not force enemy skills into the monster `UnitSkillController` in the first pass.
- Do not make prefab presence mandatory for every skill.
- Do not use MSW-MCP. This project uses Unity-MCP only.
