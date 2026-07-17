# 2026-06-19 Ariel Effect Object + Trigger Binding Handoff

Role Owner: Designer

Status: Code-evidence-based design handoff

## Code Builder Implementation Result 2026-06-19

Role Owner: Code Builder

Implemented scope:

- Kept the user-selected storage decision: use generic `monster_skill_nodes.csv` and `monster_skill_node_params.csv`; no specialized `skill_effect_bindings.csv`, `skill_effect_defs.csv`, or `skill_effect_modifiers.csv` files were created.
- Added reusable normalized choice handlers for `CountStatusDamageMultiplier`, `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `DurationBonus`, `StatusActionSpeedBonus`, `StatusAttackPowerBonus`, `StatusAilmentResistanceBonus`, `StatusConditionalDamageTakenBonus`, `StatusElementDamageTakenBonus`, and `StatusCriticalDamageTakenBonus`.
- Wired `owner_kind=Passive` normalized nodes into `PassiveDefinition.NormalizedPlanNodes` and `PassiveSkillData.NormalizedPlanNodes` so passive-owned node rows are no longer validation-blocked as unwired.
- Fixed the combat snapshot path so `SkillExecutionSnapshot.ApplyChoiceDefinition(...)` applies normalized choice nodes before composing the runtime snapshot.
- Made multi-effect status creation use the existing `SkillStatusSpecUtility.ResolveStatusData(...)` snapshot override path.
- Added targeted `StatusActionSpeedBonus` support through optional `status_id`, so a node can modify `blessing` action speed without modifying every status row emitted by the same skill.
- Migrated 28 Ariel numeric choice modifiers from wide `monster_skill_choices.csv` behavior fields into normalized `Choice` owner nodes and params.
- Added `ariel-c-trait-2-blessing-action-speed` as a targeted `StatusActionSpeedBonus(status_id=blessing, bonus=0.06)` node.
- Reduced Ariel C blessing row explosion by disabling 9 pre-combined `ariel-c-*` effect rows and letting base rows compose with normalized trait/passive nodes.

Verification:

- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors. Existing `MSB3277` warnings for `System.Net.Http` and `System.IO.Compression` remained.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors. The same existing `MSB3277` warnings remained.
- TextFieldParser CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=47 bad=`, `monster_skill_node_params.csv header=4 rows=69 bad=`, and `monster_skill_effects.csv header=70 rows=131 bad=`.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` logged sync from `Assets/CSVdata/authoring` to `Assets/Resources/Pakuri/CSVRuntime`.
- Unity-MCP `Pakuri/Validate CSV Source Data` loaded the runtime catalog with 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity-MCP `Pakuri/InGame/Validate Skill Data` logged `InGame skill data validation passed with 0 warning(s).`

Remaining compatibility rows:

- `monster_skill_effects.csv` remains active for effect body rows, triggers, passive refresh auras, and behavior whose full generic effect-object runtime is not yet implemented.
- Ariel C `master1`, `master2`, and `trait5` still keep their conceptual effect rows, but no longer need separate trait2/trait3/H-trait3 duration combination rows.
- Full ownership cleanup for Ariel B/E/F/G/H/I/J passive/effect rows remains future work after Play Mode parity checks.

## Goal

Ariel A~J 스킬을 기존 "조합 결과 행 추가" 방식에서 "스킬 본체 + 작은 effect object + trigger binding + 조건부 modifier" 방식으로 개편한다.

목표는 다음 네 가지다.

- 스킬 본체는 작고 안정적으로 유지한다.
- 부가 효과는 중복이 적은 작은 기능 부품으로 분리한다.
- trait, master, passive, awakening은 완성 행을 새로 만들지 않고 modifier 또는 binding gate로 조립한다.
- 런타임은 현재 선택된 강화 상태를 보고 최종 스킬 효과를 계산한다.

## Inspected Evidence

- `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md` describes Ariel A as magazine holy projectile with damage, magazine, reload, pierce, shielded-ally damage, last-shot explosion, and holy exposure master effects.
- `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md` describes Ariel B as all-ally shield with shield amount, duration, cooldown, shield-expire damage, shielded holy damage, ailment resistance, and shield absorb reflection.
- `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md` describes Ariel C as holy area damage plus all-ally action-speed blessing, radius, duration, spell-power master, and second-wave master effects.
- `Pakuri/reference/2.Monster/ariel/skill/d-celestial-brand.md` describes Ariel D as strongest-target mark with holy exposure, target count, crit damage taken, and mark-expire burst from tracked holy incoming damage.
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md` describes Ariel E as battlefield holy damage plus all-ally shield, shield-duration extension, sanctuary damage reduction, and high-damage/low-shield master behavior.
- `Pakuri/reference/2.Monster/ariel/skill/f-guiding-light.md` to `j-sanctuary-proclamation.md` define passive effects tied to Ariel A~E.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skills.csv` contains 10 Ariel base rows: `ariel-a` through `ariel-j`.
- `monster_skills.csv` currently stores Ariel base identity and broad runtime families such as `MagazineProjectile`, `Shield`, `SingleAttack`, and `Passive`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_effects.csv` already contains Ariel row-like effect data such as `ariel-c-blessing-action-default`, `ariel-c-blessing-action-trait2-trait3-h-trait3`, `ariel-e-shield-base`, and `ariel-i-holy-exposure-damage-taken`.
- `monster_skill_effects.csv` currently mixes trigger timing, target rule, gate conditions, effect kind, status payload, damage payload, and runtime notes in one row.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_triger.csv` already contains Ariel trigger rows such as `ariel-a-master1-last-shot-explosion`, `ariel-b-trait4-shield-expire`, `ariel-b-master2-shield-absorb-reflect`, and `ariel-d-master2-mark-expire-burst`.
- `Pakuri/Assets/CSVdata/authoring/monster/skills/monster_skill_nodes.csv` currently has Ariel normalized node examples for `ariel-a-trait-1-damage-multiplier`, `ariel-b-trait-3-cooldown-multiplier`, and `ariel-c-trait-4-radius-multiplier`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` already executes `Damage`, `Status`, and `ExtendStatusDuration` effects and has entry points for `OnHit`, `OnExpire`, and `OnHitCount`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` already dispatches source-owned and passive-owned trigger events including projectile hit, shield expire, shield absorb, status expire, outgoing damage, skill cast, and kill.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs` already has `SkillExecutionPlanNode`, `SkillExecutionPlanNodeKind`, and normalized-row authoring source support, but current typed node payloads are still limited to cast condition, damage modifier, crit modifier, and kill action operations.

## Current Problem

Ariel C shows the current problem clearly.

Current rows include separate authored combinations:

- `ariel-c-blessing-action-default`
- `ariel-c-blessing-action-trait3`
- `ariel-c-blessing-action-trait2`
- `ariel-c-blessing-action-trait2-trait3`
- `ariel-c-blessing-action-h-trait3`
- `ariel-c-blessing-action-trait3-h-trait3`
- `ariel-c-blessing-action-trait2-h-trait3`
- `ariel-c-blessing-action-trait2-trait3-h-trait3`

These are not different concepts. They are the same blessing effect with different modifier combinations.

Old model:

```text
base blessing result
+ trait2 result
+ trait3 result
+ trait2 + trait3 result
+ passive H trait3 result
+ trait2 + passive H trait3 result
+ trait3 + passive H trait3 result
+ trait2 + trait3 + passive H trait3 result
```

New model:

```text
base effect: action speed +12%, 4s
modifier: Ariel C trait 2 changes action speed amount
modifier: Ariel C trait 3 adds duration
modifier: Ariel H trait 3 adds duration
runtime composes the final values
```

The current data stores many final outcomes. The desired data should store causes and parts.

## Target Authoring Shape

Use four authoring concepts.

Important correction:

Do not make a unique effect object for every final phrase such as `damage_per_shielded_ally_0_06`, `shielded_holy_damage_bonus_0_12_5s`, `ailment_resistance_0_3_while_shielded`, or `holy_skill_crit_chance_0_08`.

Those are still too coarse.

The intended node model should split them into smaller reusable parts:

```text
condition node:
  target has status shield

target/query node:
  all allies
  count matching allies
  event target
  targets with Holy runtime skill

effect operation node:
  increase outgoing damage
  increase Holy outgoing damage
  increase ailment resistance
  increase critical chance
  increase incoming damage taken

parameter node:
  amount = 0.12
  duration_seconds = 5
  attribute = Holy
  per_count = true
```

So "shielded holy damage +12% for 5s" should be authored as:

```text
condition: target has status shield
effect: outgoing damage bonus
attribute filter: Holy
amount: 0.12
duration_seconds: 5
```

Not as:

```text
effect_type = shielded_holy_damage_bonus_0_12_5s
```

This distinction matters because the reusable part is not "shielded holy damage". The reusable parts are:

- condition: has status
- status id: shield
- stat operation: outgoing damage bonus
- attribute filter: Holy
- amount parameter
- duration parameter

The same condition node can then be reused for ailment resistance, holy damage, cooldown speed, and any future shield-gated behavior.

The same rule applies to trigger-like phrases.

Do not make a unique part named `last_projectile_area_damage_repeat`, `apply_holy_exposure_on_hit`, `conditional_damage_vs_holy_exposure_1_5`, `blessed_holy_damage_bonus_0_15`, `holy_exposure_target_damage_taken_0_10`, `after_ariel_e_action_speed_0_15_5s`, or `ariel_e_cooldown_multiplier_0_85`.

Split them like this:

```text
last projectile explosion
  trigger: projectile hit
  trigger filter: magazine projectile index is last
  action: execute skill/effect
  execution count: 2
  delay/repeat interval: 0.5
  target shape: circle
  damage operation: deal Holy damage

apply holy exposure on hit
  trigger: OnHit
  target: event target
  action: apply status
  status_id: holy-exposure

conditional damage versus holy exposure
  condition: target has status holy-exposure
  operation: damage multiplier
  amount: 1.5

blessed holy damage bonus
  condition: target has status blessing
  operation: outgoing damage bonus
  attribute filter: Holy
  amount: 0.15

holy exposure target damage taken
  condition: target has status holy-exposure
  operation: incoming damage taken bonus
  amount: 0.10

after Ariel E action speed
  trigger: OnSkillCast or OnSkillComplete
  event skill id: ariel-e
  target: all allies
  operation: action speed bonus
  amount: 0.15
  duration_seconds: 5

Ariel E cooldown multiplier
  owner: Ariel J trait 3
  target skill id: ariel-e
  operation: cooldown multiplier
  amount: 0.85
```

Current runtime already has trigger events for projectile hit, shield expire, shield absorb, status expire, outgoing damage, kill, and skill cast. It does not yet prove a fully generic "Nth projectile index" node or "OnSkillComplete" node. Use current `OnMagazineLastProjectileHit` first, and only generalize to `NthProjectileHit` if more skills need non-last projectile indexing.

### 1. Skill Body

Owned by `monster_skills.csv` or the existing `monster_skill_base.csv`.

Purpose:

- identity
- slot
- skill kind
- runtime kind
- core damage or shield baseline
- cooldown
- broad targeting shape when it is truly part of the body
- prefab reference when the prefab is part of the main execution actor

Example:

```text
ariel-c
  runtime_kind = SingleAttack
  base_damage = 28
  spell_power_coefficient = 1.2
  radius = 3
  cooldown_seconds = 8
```

### 2. Trigger Binding

New table or normalized node family.

Suggested table name:

```text
skill_effect_bindings.csv
```

Suggested columns:

```csv
binding_id,owner_kind,owner_id,target_skill_id,trigger_event,target_side,target_selection,target_shape,center_mode,sort_order,enabled_by_default,requires_active_choice_id,excludes_active_choice_id,requires_passive_skill_id,excludes_passive_skill_id,condition_status_id,condition_target_side,effect_id,effect_list_id,runtime_support_state,runtime_support_notes
```

Purpose:

- when an effect runs
- who owns it
- which target rule it uses
- what gate activates it
- which effect object or effect list it invokes

### 3. Effect Object

New table or normalized node family.

Suggested table name:

```text
skill_effect_defs.csv
```

Suggested columns:

```csv
effect_id,effect_type,status_id,attribute,base_damage,attack_power_coefficient,spell_power_coefficient,damage_multiplier,duration_seconds,tick_interval_seconds,stack_amount,max_stacks,merge_policy,shield_refresh_policy,visual_prefab_path,runtime_support_state,runtime_support_notes
```

Purpose:

- the small reusable behavior part.
- one row should mean one reusable function, not one finished skill combination.

Recommended first effect types:

- `ApplyStatusModifier`
- `GrantShield`
- `DealDamage`
- `ExtendStatusDuration`
- `DamageMultiplier`
- `CooldownMultiplier`
- `RadiusMultiplier`
- `MagazineCapacityAdd`
- `ReloadMultiplier`
- `PierceAdd`
- `ConditionalDamageBonus`
- `TriggeredDamage`
- `ReflectDamage`
- `SecondWaveDamage`
- `TrackIncomingDamage`

These `effect_type` values should stay generic. Do not encode the condition, attribute, amount, and duration into the effect type name.

Good:

```text
effect_type = ApplyStatusModifier
condition_status_id = shield
modifier_stat = outgoing_damage_bonus
attribute = Holy
amount = 0.12
duration_seconds = 5
```

Bad:

```text
effect_type = ShieldedHolyDamageBonus12Percent5Seconds
```

### 4. Modifier

New table or normalized node family.

Suggested table name:

```text
skill_effect_modifiers.csv
```

Suggested columns:

```csv
modifier_id,owner_kind,owner_id,target_effect_id,target_binding_id,target_skill_id,operation,param_key,value_type,value,stacking_rule,requires_active_choice_id,excludes_active_choice_id,requires_passive_skill_id,excludes_passive_skill_id,condition_status_id,runtime_support_state,runtime_support_notes
```

Purpose:

- trait/master/passive/awakening changes an existing effect without adding every possible final combination row.

Examples:

```text
ariel-c-trait-3
  target_effect_id = ariel-c-blessing-action-speed
  operation = Add
  param_key = duration_seconds
  value = 2

ariel-h-trait-3
  target_effect_id = ariel-c-blessing-action-speed
  operation = Add
  param_key = duration_seconds
  value = 2
```

## Ariel A~J Decomposition Plan

### Ariel A: 심판의 빛

Reference intent:

- magazine holy projectile.
- base damage `18 + spell power * 1.0`.
- projectile speed 17.
- pierce 1.
- magazine 7.
- reload 4.6s.
- shot interval 0.36s.

Skill body:

```text
skill_id = ariel-a
runtime_kind = MagazineProjectile
attribute = Holy
base_damage = 18
spell_power_coefficient = 1.0
projectile_speed = 17
pierce_count = 1
magazine_capacity = 7
reload_seconds = 4.6
shot_interval_seconds = 0.36
```

Effect objects:

- `damage_multiplier_1_25`: generic damage multiplier, reused by any +25% damage trait.
- `magazine_capacity_add_3`: adds magazine capacity.
- `reload_multiplier_0_8`: reload time -20%.
- `pierce_add_1`: adds pierce count.
- shielded-ally scaling should be composed from `query allies with status shield` + `count matching targets` + `damage bonus per count` + `amount 0.06`.
- last projectile explosion should be composed from `trigger projectile hit` + `filter last magazine projectile` + `execute effect/skill` + `repeat count 2` + `area damage`.
- holy exposure on hit should be composed from `trigger OnHit` + `target event target` + `apply status` + `status_id holy-exposure`.

Bindings:

```text
ariel-a base projectile damage
  trigger_event = MainExecution
  effect = base projectile damage from skill body

ariel-a trait 1
  modifier target = ariel-a damage
  operation = Multiply
  value = 1.25

ariel-a trait 2
  modifier target = ariel-a magazine_capacity
  operation = Add
  value = 3

ariel-a trait 3
  modifier target = ariel-a reload_seconds
  operation = Multiply
  value = 0.8

ariel-a trait 4
  modifier target = ariel-a pierce_count
  operation = Add
  value = 1

ariel-a trait 5
  modifier target = ariel-a damage
  query = all allies with status shield
  operation = AddPerMatchingTarget
  param amount_per_target = 0.06

ariel-a master 1
  trigger_event = OnMagazineLastProjectileHit
  target = enemies in circle
  action = execute effect/skill
  repeat_count = 2
  repeat_interval_seconds = 0.5
  operation = deal damage
  params attribute = Holy, radius = 3

ariel-a master 2
  trigger_event = OnHit
  target = event target
  action = apply status
  params status_id = holy-exposure
```

Current old source:

- `monster_skill_triger.csv` already has `ariel-a-master1-last-shot-explosion`.
- `monster_skill_nodes.csv` already has `ariel-a-trait-1-damage-multiplier`.

Ambiguity:

- Ariel A master 2 exposure stack/duration should follow current CSV if present, but the exact migrated object should be confirmed against current implemented rows before deleting legacy support.

### Ariel B: 성광 방패

Reference intent:

- all-ally shield.
- shield amount `35 + spell power * 1.4`.
- duration 5s.
- cooldown 9s.
- shield refresh rule: highest value wins.

Skill body:

```text
skill_id = ariel-b
runtime_kind = Shield
base_shield = 35
spell_power_coefficient = 1.4
duration_seconds = 5
cooldown_seconds = 9
target_side = AllAllies
```

Effect objects:

- `grant_shield_spell_35_1_4_5s`: shield grant.
- `shield_amount_multiplier_1_3`: shield amount +30%.
- `status_duration_add_2s`: duration +2s.
- `cooldown_multiplier_0_8`: cooldown -20%.
- shield expire damage should be composed from `trigger OnShieldExpire` + `damage source applied shield amount` + `damage multiplier 0.6` + `target enemies in circle`.
- shielded holy damage should be composed from `condition target has status shield` + `outgoing damage bonus` + `attribute Holy` + `amount 0.12` + `duration 5`.
- `shield_amount_multiplier_1_5`: master 1 shield +50%.
- shielded ailment resistance should be composed from `condition target has status shield` + `ailment resistance bonus` + `amount 0.3`.
- shield absorb reflection should be composed from `trigger OnShieldAbsorb` + `target event attacker` + `damage source absorbed shield amount` + `damage multiplier 0.35`.

Bindings:

```text
ariel-b OnCast
  effect = grant shield

ariel-b trait 1
  modifier target = grant shield amount
  Multiply 1.3

ariel-b trait 2
  modifier target = grant shield duration
  Add 2

ariel-b trait 3
  modifier target = ariel-b cooldown
  Multiply 0.8

ariel-b trait 4
  trigger_event = OnShieldExpire
  target = enemies in circle
  action = deal damage
  params damage_source = ShieldAppliedAmount, damage_source_multiplier = 0.6, attribute = Holy, radius = 3

ariel-b trait 5
  trigger_event = OnCast
  condition = target has shield
  effect = outgoing damage bonus
  params attribute = Holy, amount = 0.12, duration_seconds = 5

ariel-b master 1
  modifier target = grant shield amount
  Multiply 1.5
  plus condition target has shield + ailment resistance bonus + amount 0.3

ariel-b master 2
  trigger_event = OnShieldAbsorb
  target = event attacker
  action = deal damage
  params damage_source = ShieldAbsorbedAmount, damage_source_multiplier = 0.35, attribute = Holy
```

Current old source:

- `monster_skill_triger.csv` already has `ariel-b-trait4-shield-expire`.
- `monster_skill_triger.csv` already has `ariel-b-master2-shield-absorb-reflect`.
- `monster_skill_effects.csv` already has `ariel-b-shielded-holy-trait5`.
- `monster_skill_nodes.csv` already has `ariel-b-trait-3-cooldown-multiplier`.

Ambiguity:

- Ailment resistance while shielded should be represented as either a status modifier applied to shielded units or as a conditional status resolver. Pick one runtime path before migration.

### Ariel C: 축복의 파동

Reference intent:

- holy area damage.
- all-ally action-speed blessing.
- base damage `28 + spell power * 1.2`.
- radius 3.
- action speed +12%.
- blessing duration 4s.
- cooldown 8s.

Skill body:

```text
skill_id = ariel-c
runtime_kind = SingleAttack
attribute = Holy
base_damage = 28
spell_power_coefficient = 1.2
radius = 3
cooldown_seconds = 8
```

Effect objects:

- `blessing_action_speed_0_12_4s`: apply blessing with action speed +12% for 4s.
- `action_speed_amount_add_0_06`: trait 2 modifier.
- `duration_add_2s`: reusable duration modifier.
- `radius_multiplier_1_25`: radius +25%.
- shielded holy damage should reuse `condition target has status shield` + `outgoing damage bonus` + `attribute Holy` + `amount 0.10`.
- `blessing_spell_power_0_18_4s`: master 1 spell power +18%.
- second wave should be composed from `trigger OnCast` + `delay` + `repeat/execute effect` + `damage multiplier 0.6`.

Bindings:

```text
ariel-c OnCast
  effect = blessing action speed

ariel-c trait 1
  modifier target = ariel-c damage
  Multiply 1.25

ariel-c trait 2
  modifier target = blessing action speed amount
  Add 0.06

ariel-c trait 3
  modifier target = blessing duration
  Add 2

ariel-c trait 4
  modifier target = ariel-c radius
  Multiply 1.25

ariel-c trait 5
  trigger_event = OnCast
  condition = target has shield
  effect = outgoing damage bonus
  params attribute = Holy, amount = 0.10

ariel-c master 1
  trigger_event = OnCast
  effect = spell power blessing

ariel-c master 2
  trigger_event = OnCast
  action = delayed execute damage
  params damage_multiplier = 0.6
```

Current old source:

- `monster_skill_effects.csv` currently contains many pre-combined C blessing rows.
- `monster_skill_effects.csv` already has `ariel-c-master2-second-wave`.
- `monster_skill_nodes.csv` already has `ariel-c-trait-4-radius-multiplier`.

Migration priority:

- Ariel C should be the first pilot because it has obvious row explosion and low conceptual ambiguity.

### Ariel D: 천상의 낙인

Reference intent:

- strongest-target holy damage.
- holy exposure.
- exposure means target takes more holy damage.
- duration 6s.
- cooldown 10s.

Skill body:

```text
skill_id = ariel-d
runtime_kind = SingleAttack
attribute = Holy
base_damage = 40
spell_power_coefficient = 1.8
target_selection = HighestHealth
cooldown_seconds = 10
```

Effect objects:

- holy exposure should be composed from `apply status` + `status_id holy-exposure` + `incoming holy damage taken bonus` + `amount 0.20` + `duration 6`.
- `damage_multiplier_1_3`: trait 1.
- `holy_exposure_value_add_0_08`: trait 2.
- `duration_add_3s`: trait 3.
- `target_count_add_1`: trait 4.
- `damage_multiplier_0_8`: trait 4 drawback.
- `shielded_ally_damage_to_marked_0_10`: trait 5.
- `marked_target_crit_damage_taken_0_25`: master 1.
- mark expire burst should be composed from `trigger OnStatusExpire` + `condition expired status holy-exposure/source ariel-d` + `damage source tracked incoming Holy damage` + `damage multiplier 0.2`.

Bindings:

```text
ariel-d OnHit or MainExecution status payload
  effect = apply holy exposure

ariel-d trait 1
  modifier target = ariel-d damage
  Multiply 1.3

ariel-d trait 2
  modifier target = holy exposure damage-taken amount
  Add 0.08

ariel-d trait 3
  modifier target = holy exposure duration
  Add 3

ariel-d trait 4
  modifier target = target count
  Add 1
  modifier target = ariel-d damage
  Multiply 0.8

ariel-d trait 5
  condition = attacker has shield or ally has shield
  effect = damage bonus to marked target

ariel-d master 1
  effect = crit damage taken on marked target

ariel-d master 2
  trigger_event = OnStatusExpire
  condition = expired status is holy-exposure from ariel-d
  action = deal damage
  params damage_source = TrackedIncomingDamage, tracked_attribute = Holy, damage_source_multiplier = 0.2
```

Current old source:

- `monster_skill_triger.csv` already has `ariel-d-master2-mark-expire-burst`.

Ambiguity:

- "방어막을 가진 아군이 낙인 대상에게 주는 피해 +10%" needs a precise condition: source has shield, any ally has shield, or count of shielded allies. Ask user before implementation.

### Ariel E: 대천사의 강림

Reference intent:

- battlefield holy damage.
- all-ally shield.
- base damage `58 + spell power * 2.4`.
- shield `50 + spell power * 1.6`.
- shield duration 6s.
- cooldown 17s.

Skill body:

```text
skill_id = ariel-e
runtime_kind = SingleAttack
attribute = Holy
base_damage = 58
spell_power_coefficient = 2.4
target_shape = Battlefield
cooldown_seconds = 17
```

Effect objects:

- `grant_shield_spell_50_1_6_6s`: all-ally shield.
- `shield_amount_multiplier_1_3`: trait 2.
- `cooldown_multiplier_0_8`: trait 3.
- conditional damage versus holy exposure should be composed from `condition target has status holy-exposure` + `damage multiplier` + `amount 1.5`.
- `extend_shield_duration_3s`: trait 5.
- `incoming_damage_taken_minus_0_18_5s`: master 1 sanctuary.
- `damage_multiplier_1_7`: master 2.
- `shield_amount_multiplier_0_7`: master 2 drawback.
- passive J action speed should be composed from `trigger OnSkillCast` + `event_skill_id ariel-e` + `target all allies` + `action speed bonus` + `amount 0.15` + `duration 5`.

Bindings:

```text
ariel-e OnCast
  effect = grant shield

ariel-e trait 1
  modifier target = ariel-e damage
  Multiply 1.3

ariel-e trait 2
  modifier target = grant shield amount
  Multiply 1.3

ariel-e trait 3
  modifier target = ariel-e cooldown
  Multiply 0.8

ariel-e trait 4
  condition = target has holy-exposure
  operation = damage multiplier
  param amount = 1.5

ariel-e trait 5
  trigger_event = OnCast
  effect = extend current shield duration

ariel-e master 1
  trigger_event = OnCast
  effect = incoming damage taken -18%, 5s

ariel-e master 2
  modifier target = ariel-e damage
  Multiply 1.7
  modifier target = ariel-e shield amount
  Multiply 0.7
```

Current old source:

- `monster_skill_effects.csv` already has `ariel-e-shield-base`, trait/master shield variants, `ariel-e-holy-exposed-damage-trait4`, `ariel-e-master1-sanctuary`, and `ariel-e-trait5-extend-shield-duration`.

Migration priority:

- Migrate after Ariel C and B because it combines damage, shield, conditional damage, and shield duration extension.

### Ariel F: 빛의 인도

Reference intent:

- all allies holy damage +12%.
- trait 1 adds +6%.
- trait 2 adds Ariel A magazine +2.
- trait 3 gives crit chance to allies with holy skills.

Effect objects:

- `aura_holy_damage_bonus_0_12`: base aura.
- `aura_holy_damage_bonus_add_0_06`: trait 1 modifier.
- `ariel_a_magazine_add_2`: trait 2 modifier.
- holy skill crit chance should be composed from `condition target has active skill attribute Holy` + `critical chance bonus` + `amount 0.08`.

Bindings:

```text
ariel-f passive learned
  trigger_event = PassiveAuraTick or OnCast refresh
  effect = holy damage bonus aura

ariel-f trait 2
  modifier target = ariel-a magazine capacity
  Add 2

ariel-f trait 3
  condition = target has active skill attribute Holy
  effect = critical chance bonus
  param amount = 0.08
```

Current old source:

- `monster_skill_effects.csv` has `ariel-f-party-holy-damage` and `ariel-f-holy-skill-crit-trait3`.

Ambiguity:

- Current executor appears to refresh passive status rows through `OnCast` style effect execution. If passive auras are intended to be always-on, Code Builder should decide whether to add explicit `PassiveAura` timing or keep the current refresh mechanism.

### Ariel G: 수호 교리

Reference intent:

- all allies shield received +18%.
- combat start all-ally shield `25 + spell power * 0.8`.
- trait 1 shield received +8%.
- trait 2 start shield +40%.
- trait 3 shielded allies holy damage +10%.

Effect objects:

- `shield_received_bonus_0_18`: base aura.
- `shield_received_bonus_add_0_08`: trait 1.
- `combat_start_shield_25_0_8`: start shield.
- `combat_start_shield_amount_add_0_4`: trait 2 or extra shield object.
- G trait 3 should reuse `condition target has status shield` + `outgoing damage bonus` + `attribute Holy` + `amount 0.10`.

Bindings:

```text
ariel-g passive learned
  effect = shield received bonus

ariel-g combat start
  trigger_event = OnCombatStart
  effect = start shield

ariel-g trait 3
  condition = target has shield
  effect = outgoing damage bonus
  params attribute = Holy, amount = 0.10
```

Current old source:

- `monster_skill_effects.csv` has `ariel-g-shield-received`, `ariel-g-shield-received-trait1`, `ariel-g-start-shield`, `ariel-g-start-shield-trait2`, and `ariel-g-shielded-holy-trait3`.

Ambiguity:

- Need confirm whether `OnCombatStart` is a first-class trigger in current runtime, or whether current data emulates it through initial passive cast.

### Ariel H: 축복 전파

Reference intent:

- allies with blessing gain holy damage +15%.
- allies with blessing gain cooldown charge speed +10%.
- trait 1 adds holy damage +7%.
- trait 2 adds cooldown charge speed +5%.
- trait 3 increases Ariel C blessing duration +2s.

Effect objects:

- blessing holy damage should be composed from `condition target has status blessing` + `outgoing damage bonus` + `attribute Holy` + `amount 0.15`.
- blessing cooldown charge should be composed from `condition target has status blessing` + `cooldown charge speed bonus` + `amount 0.10`.
- `holy_damage_bonus_add_0_07`: trait 1.
- `cooldown_charge_speed_add_0_05`: trait 2.
- `ariel_c_blessing_duration_add_2s`: trait 3.

Bindings:

```text
ariel-h passive learned
  condition = target has blessing
  effect = outgoing damage bonus, params attribute = Holy, amount = 0.15
  effect = cooldown charge speed bonus, params amount = 0.10

ariel-h trait 3
  modifier target = ariel-c blessing duration
  Add 2
```

Current old source:

- `monster_skill_effects.csv` has `ariel-h-blessed-holy-damage-speed`.
- Ariel C currently has extra pre-combined rows for `ariel-h-trait-3` duration interaction.

Migration value:

- This passive is a key reason to move away from pre-combined rows.

### Ariel I: 낙인 계시

Reference intent:

- holy-exposure targets take +10% damage from all allies.
- trait 1 adds +5%.
- trait 2 makes Ariel D cooldown -20%.
- trait 3 reduces holy-exposure target holy resistance by 8.

Effect objects:

- holy exposure target damage taken should be composed from `condition target has status holy-exposure` + `incoming damage taken bonus` + `amount 0.10`.
- `damage_taken_add_0_05`: trait 1.
- `ariel_d_cooldown_multiplier_0_8`: trait 2.
- holy exposure target holy resist reduction should be composed from `condition target has status holy-exposure` + `resist reduction` + `attribute Holy` + `flat amount -8`.

Bindings:

```text
ariel-i passive learned
  condition = target has holy-exposure
  effect = incoming damage taken bonus
  param amount = 0.10

ariel-i trait 2
  modifier target = ariel-d cooldown
  Multiply 0.8

ariel-i trait 3
  condition = target has holy-exposure
  effect = holy resist reduction
```

Current old source:

- `monster_skill_effects.csv` has `ariel-i-holy-exposure-damage-taken`, `ariel-i-holy-exposure-damage-taken-trait1`, and `ariel-i-holy-exposure-holy-resist-trait3`.

Ambiguity:

- Need confirm whether damage taken +10% is all damage or only allied damage. Reference says "모든 아군에게 받는 피해", so runtime should probably scope source side to allies, not global incoming damage from every source.

### Ariel J: 성역 선포

Reference intent:

- after Ariel E, all allies gain action speed +15% for 5s.
- allies with Ariel E shield gain holy damage +20%.
- trait 1 adds action speed +7%.
- trait 2 adds holy damage +10%.
- trait 3 makes Ariel E cooldown -15%.

Effect objects:

- after Ariel E action speed should be composed from `trigger OnSkillCast` + `event_skill_id ariel-e` + `target all allies` + `action speed bonus` + `amount 0.15` + `duration 5`.
- `action_speed_add_0_07`: trait 1.
- shielded holy damage should reuse `condition target has status shield` + `outgoing damage bonus` + `attribute Holy` + `amount 0.20`.
- `holy_damage_bonus_add_0_10`: trait 2.
- Ariel E cooldown reduction should be composed from `owner Ariel J trait 3` + `target skill ariel-e` + `cooldown multiplier` + `amount 0.85`.

Bindings:

```text
ariel-j passive learned
  trigger_event = OnSkillCast
  event_skill_id = ariel-e
  target = all allies
  effect = action speed bonus
  params amount = 0.15, duration_seconds = 5

ariel-j passive learned
  condition = target has shield from ariel-e or target has shield
  effect = holy damage +20%

ariel-j trait 3
  modifier target = ariel-e cooldown
  Multiply 0.85
```

Current old source:

- `monster_skill_effects.csv` has `ariel-j-shielded-holy-damage`.
- Ariel E currently has `ariel-e-passive-j-action-speed` and `ariel-e-passive-j-action-speed-trait1`, which couples passive J behavior into Ariel E rows.

Migration value:

- Move J's logic out of Ariel E pre-combined rows and into J-owned bindings.

Ambiguity:

- Need confirm whether "대천사의 강림 방어막이 남아있는 아군" means specifically shield status sourced by `ariel-e`, or any active shield. Current data uses condition `shield`, which may be broader than the reference text.

## Runtime Assembly Model

Target runtime flow:

```text
1. Skill cast or combat event occurs.
2. Runtime resolves skill body from base skill data.
3. Runtime creates SkillExecutionContext and current SkillExecutionSnapshot.
4. Runtime finds bindings where owner/target/event match.
5. Runtime checks gates:
   - active choice required/excluded
   - passive required/excluded
   - condition status
   - event skill id
   - source status
6. Runtime loads effect object or effect list from binding.
7. Runtime applies active modifiers to the effect object:
   - trait modifiers
   - master modifiers
   - passive modifiers
   - awakening modifiers
8. Runtime executes the final composed effect through an effect handler.
9. Status effects enter existing status runtime.
10. Status expiry, shield absorption, projectile hit, and kill events dispatch trigger bindings.
```

Current code already has pieces:

- `SkillMultiEffectExecutor` can execute status, damage, and extend-status effects.
- `SkillTriggerRuntime` can dispatch major combat events.
- `SkillExecutionPlan` can carry normalized nodes, but more typed effect/action payloads are needed for full object composition.

Required runtime additions:

- Add an effect-object registry keyed by `effect_type`.
- Add binding resolution by `owner_kind`, `owner_id`, `target_skill_id`, and `trigger_event`.
- Add modifier resolution by target effect/binding/skill and current player choices/passives.
- Add a composed effect data object so modifier application does not mutate source definitions.
- Add validation to prevent the same behavior from being active in both old rows and new bindings.

## Reusable Effect Parts To Prefer

Use reusable atomic parts when the behavior is identical. Prefer composing condition, target/query, operation, and parameter nodes instead of naming a final special-case effect.

Examples:

- `duration_add_2s`
- `duration_add_3s`
- `cooldown_multiplier_0_8`
- `cooldown_multiplier_0_85`
- `damage_multiplier_1_25`
- `damage_multiplier_1_3`
- `shield_amount_multiplier_1_3`
- `action_speed_add_0_07`
- `holy_damage_bonus_add_0_10`
- `apply_holy_exposure`
- `condition_has_status`
- `condition_has_active_skill_attribute`
- `query_allies_with_status`
- `query_matching_target_count`
- `operation_outgoing_damage_bonus`
- `operation_critical_chance_bonus`
- `operation_ailment_resistance_bonus`
- `operation_incoming_damage_taken_bonus`
- `operation_resist_reduction`
- `operation_cooldown_multiplier`
- `operation_cooldown_charge_speed_bonus`
- `attribute_filter_holy`
- `trigger_on_hit`
- `trigger_on_skill_cast`
- `trigger_on_shield_expire`
- `trigger_on_shield_absorb`
- `trigger_on_status_expire`
- `filter_event_skill_id`
- `filter_last_magazine_projectile`
- `action_execute_skill_or_effect`
- `action_apply_status`
- `damage_source_fixed`
- `damage_source_shield_applied_amount`
- `damage_source_shield_absorbed_amount`
- `damage_source_tracked_incoming_damage`
- `extend_shield_duration`

Do not create separate duplicate objects such as:

```text
ariel-c-trait3-duration-effect
ariel-h-trait3-duration-effect
ariel-b-trait2-duration-effect
shielded-holy-damage-12-percent
shielded-ailment-resistance-30-percent
holy-skill-crit-chance-8-percent
last-projectile-area-damage-repeat
after-ariel-e-action-speed
holy-exposure-target-damage-taken
```

unless they differ in target, condition, or runtime timing.

Preferred decomposition examples:

```text
shielded holy damage +12%, 5s
  condition_has_status(status_id=shield)
  operation_outgoing_damage_bonus(attribute=Holy, amount=0.12, duration_seconds=5)

shielded ailment resistance +30%
  condition_has_status(status_id=shield)
  operation_ailment_resistance_bonus(amount=0.30)

holy skill crit chance +8%
  condition_has_active_skill_attribute(attribute=Holy)
  operation_critical_chance_bonus(amount=0.08)

damage +6% per shielded ally
  query_allies_with_status(status_id=shield)
  query_matching_target_count()
  operation_damage_bonus_per_count(amount_per_target=0.06)

last projectile area damage repeat
  trigger_on_hit()
  filter_last_magazine_projectile()
  action_execute_skill_or_effect(repeat_count=2, repeat_interval_seconds=0.5)
  operation_deal_damage(attribute=Holy, target_shape=Circle, radius=3)

after Ariel E action speed
  trigger_on_skill_cast()
  filter_event_skill_id(skill_id=ariel-e)
  action_apply_status()
  operation_action_speed_bonus(amount=0.15, duration_seconds=5)

holy exposure target damage taken
  condition_has_status(status_id=holy-exposure)
  operation_incoming_damage_taken_bonus(amount=0.10)
```

## Runtime Feasibility Of Atomic Nodes

This finer split is feasible, but not all atomic nodes are currently first-class runtime nodes.

Already close to supported by existing runtime:

- `condition_has_status`: current effect rows already have `condition_status_id`, and `SkillMultiEffectExecutor.TargetMatchesCondition(...)` checks it.
- `condition_has_active_skill_attribute`: current effect rows already have `condition_skill_attribute`, and `SkillMultiEffectExecutor` checks active skill attributes.
- `trigger_on_hit`, `trigger_on_skill_cast`, `trigger_on_shield_expire`, `trigger_on_shield_absorb`, `trigger_on_status_expire`: current `SkillTriggerRuntime` already has dispatch entry points for these event families, though not all are exposed as generic atomic authoring nodes.
- `damage_source_shield_applied_amount`, `damage_source_shield_absorbed_amount`, `damage_source_tracked_incoming_damage`: current trigger runtime already resolves these damage sources for trigger rows.
- `operation_action_speed_bonus`, `operation_outgoing_damage_bonus`, `operation_critical_chance_bonus`, `operation_ailment_resistance_bonus`, `operation_incoming_damage_taken_bonus`, and resist-related operations are close to current status modifier fields.

Needs new generic node/runtime work:

- Generic `NthProjectileHit` or `ProjectileIndexFilter`. Current code proves last-magazine projectile support, not arbitrary Nth projectile support.
- Generic `OnSkillComplete`. Current code proves `OnSkillCast`; completion timing should be added only if the design truly needs after-resolution rather than on-cast.
- Generic effect list execution and effect reuse by id.
- Generic modifier application over composed effect parameters.
- Generic query-count node such as "count allies with status shield".
- Validation that prevents final special-case effect types from being added when the same behavior can be built from atomic nodes.

## Resolved Design Questions From User

These answers were provided before the Code Builder pass on 2026-06-19.

1. Ariel D trait 5: the attacking ally itself must have the shield status.
2. Ariel J shield condition: require shield generated by Ariel E, not any shield.
3. Ariel I base passive: while holy exposure exists, apply the incoming damage taken bonus to all incoming damage.
4. Passive aura timing: passives are always active.
5. Duration units: keep seconds.
6. Effect object storage: use generic `monster_skill_nodes.csv` and `monster_skill_node_params.csv`.

## Previous Structure Handling

Do not delete the previous structure during the first Ariel migration.

Keep these files active as compatibility input:

- `monster_skills.csv`
- `monster_skill_choices.csv`
- `monster_skill_effects.csv`
- `monster_skill_triger.csv`
- `monster_skill_nodes.csv`
- `monster_skill_node_params.csv`

Migration policy:

1. Add new binding/effect/modifier authoring path beside old rows.
2. Migrate Ariel C first because it has the clearest row explosion.
3. Mark migrated old rows as disabled or compatibility-only, but do not remove them until parity is verified.
4. Add validation to prevent double application when the same behavior exists in old and new form.
5. After Ariel C passes code build, CSV validation, and gameplay verification, migrate Ariel B and E.
6. After active skills A~E are stable, migrate passives F~J.
7. Only after all Ariel behaviors are verified, decide whether old Ariel rows are deleted, archived, or retained as disabled compatibility examples.

Recommended old-row states:

```text
RuntimeImplemented
  still active old path

MigratedToEffectBinding
  no longer active, retained for traceability

DeprecatedCompatibility
  readable by parser, blocked from new authoring

Archived
  moved out of active runtime CSV only after full verification
```

Do not keep two active sources for the same behavior.

Bad:

```text
ariel-c-blessing-action-trait2-trait3-h-trait3 active
+ new modifier-composed blessing also active
```

Good:

```text
old pre-combined Ariel C blessing rows disabled
+ one base blessing effect
+ trait/passive modifiers
```

## Suggested Implementation Phases

### Phase 1: Ariel C Pilot

Purpose:

- Prove the composition model on the worst row-explosion case.

Tasks:

- Create base `blessing_action_speed_0_12_4s`.
- Create modifiers for Ariel C trait 2, Ariel C trait 3, Ariel H trait 3.
- Create C trait 5 shielded holy damage binding.
- Create master 1 spell-power blessing binding.
- Create master 2 second-wave binding.
- Disable or gate old pre-combined C blessing rows so they do not double apply.

Acceptance:

- No more pre-combined C blessing rows are needed for trait/passive duration combinations.
- Current C reference behavior can be represented as base effect + modifiers.

### Phase 2: Ariel B Shield and Shield Events

Purpose:

- Prove shield amount/duration modifiers plus shield event triggers.

Tasks:

- Convert shield amount and duration to effect + modifiers.
- Convert shield expire damage to trigger binding.
- Convert shield absorb reflection to trigger binding.
- Decide ailment resistance while shielded path before implementation.

Acceptance:

- B trait/master shield behavior is composed without duplicate shield rows.

### Phase 3: Ariel E Mixed Damage + Shield

Purpose:

- Prove multi-effect skill body and passive J decoupling.

Tasks:

- Convert E shield variants to one shield effect plus modifiers.
- Convert holy-exposure conditional damage.
- Convert shield duration extension.
- Convert sanctuary damage reduction.
- Move passive J action speed logic out of Ariel E pre-combined rows into J-owned binding if user confirms passive ownership rule.

Acceptance:

- E no longer needs separate shield rows for base, trait2, master2, trait2+master2 combinations.

### Phase 4: Ariel A and D Triggers

Purpose:

- Prove projectile hit, mark expiration, tracked damage, and on-hit status paths.

Tasks:

- Convert A last-shot explosion and A holy exposure on-hit.
- Convert D mark expiration burst.
- Convert D exposure amount/duration modifiers.
- Resolve D trait 5 ambiguity.

Acceptance:

- Existing trigger rows have equivalent binding/effect objects or are explicitly retained as specialized trigger compatibility rows.

### Phase 5: Ariel F~J Passive Ownership Cleanup

Purpose:

- Move passive effects to passive-owned bindings rather than active-skill pre-combination rows.

Tasks:

- Define passive aura timing.
- Migrate F holy damage and crit chance.
- Migrate G shield received, combat-start shield, shielded holy damage.
- Migrate H blessing conditional buffs.
- Migrate I holy exposure target effects.
- Migrate J post-E and shielded effects.

Acceptance:

- Passive behavior is authored under passive skill ownership.
- Active skill rows no longer carry unrelated passive-specific final combinations.

## Verification Required From Code Builder

- CSV field-count check for all active CSV files.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets`.
- Unity `Pakuri/Validate CSV Source Data`.
- Unity console check for new errors/warnings.
- Gameplay verification by user for at least Ariel C before migrating the rest.

## Non-Goals

- Do not migrate all 5 monsters in this handoff.
- Do not delete old Ariel rows during the first code pass.
- Do not redesign Ariel balance numbers unless the user asks.
- Do not move visual prefab behavior into arbitrary per-prefab unique skill scripts.
- Do not make every new behavior a new CSV column.
- Do not claim Play Mode parity without user-side gameplay verification.

## Final Design Decision

Use this rule for Ariel migration:

```text
monster_skills.csv = skill body
binding rows = when and where a part fires
effect rows = what the part does
modifier rows = how trait/master/passive/awakening changes that part
runtime = composes final effect and executes it
```

This changes the authoring model from:

```text
add one CSV row for every final combination
```

to:

```text
add one reusable part, then attach or modify it through bindings
```

That is the intended "합체로봇" model.
