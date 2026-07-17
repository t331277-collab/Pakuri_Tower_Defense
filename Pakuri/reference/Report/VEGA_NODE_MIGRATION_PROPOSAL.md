# Vega A-J Skill Graph Migration Proposal

## 1. 목표

Vega A-J 스킬의 현재 동작을 `skill_graph_nodes_*.csv` 기반 positional graph로 이전한다.

- 활성 authoring 기준은 `Pakuri/Assets/CSVdata/authoring/monster/skills/`이다.
- Sein 전환처럼 `Plan` graph는 Choice modifier를, `Effect` graph는 효과 조립을 담당한다.
- base skill 10행과 Trigger의 이벤트/확률/쿨다운 envelope는 유지한다.
- legacy Effect 23행과 legacy direct node 4행/param 11행은 이전 완료와 동시에 제거한다.
- 새 graph 파일과 새 graph 열은 만들지 않는다. 기존 21열 positional graph 파일 5개를 사용한다.
- 노드 리팩터링이므로 Blueprint, reference 수치 탐색, prefab 수정, visual offset 추가는 범위 밖이다.

문서 상태: **Code Builder 구현 및 자동 검증 완료 / 사용자 Play Mode 확인 대기**

## 2. 범위

### 포함

- `projectile`, `line_attack`, `buff`, `single_attack`, `passive` Choice 행동 값을 positional `Plan` graph로 이전
- legacy Effect 23행을 positional `Effect` graph로 이전
- Trigger가 실행하는 Effect를 `triggered_graph_owner_*` 참조로 교체
- Vega legacy direct node 4행/param 11행을 positional graph로 이전
- 기존 런타임 필드를 graph node에서 설정할 최소 handler/composer 노출
- graph와 중복되는 Choice wide 행동 필드 비우기
- authoring validation, runtime data validation, 관련 EditMode 테스트

### 제외

- `boards/SkillBluePrint/*-blueprint.md` 열람 또는 수정
- archive/reference markdown 및 다른 몬스터 구현 탐색
- Vega 밸런스 재설계와 base skill 10행 기본 수치 변경
- prefab, sprite, animator, collider, object position 변경
- object/collider offset 열 추가 또는 값 변경
- Trigger 이벤트 구조 자체를 graph로 흡수
- 사용자 소유 Play Mode 실행

## 3. 검사 근거와 현재 상태

### 3.1 전환 전 활성 Vega 데이터 집계

`Pakuri/Assets/CSVdata/authoring/monster/skills/`에서 `monster_id=vega`를 집계했다.

| 구분 | 현재 행 수 | 근거 |
|---|---:|---|
| base skill | 10 | projectile 1, line attack 1, buff 1, single attack 2, passive 5 |
| Choice | 51 | A-E 35, F-J 및 `vega-h-base-duration` 16 |
| positional graph | 0 | 5개 `skill_graph_nodes_*.csv`에 Vega 행 없음 |
| legacy Effect | 23 | projectile 1, line attack 3, single attack 8, passive 11 |
| Trigger | 15 | projectile 1, line attack 1, passive 13 |
| legacy direct node | 4 | `single_attack_skill_nodes.csv` |
| legacy direct node param | 11 | `single_attack_skill_node_params.csv` |

검사 시작 명령:

```powershell
rg -l 'vega' 'Pakuri\Assets\CSVdata\authoring\monster\skills' -g '*.csv'
```

각 CSV는 `Import-Csv` 후 `monster_id -eq 'vega'`로 집계했다. 이 제안서는 검사 시점에 존재하지 않았고 본 파일이 최초 생성본이다.

### 3.2 사용할 positional graph 파일

아래 파일은 모두 이미 존재하며 같은 21열 schema를 사용한다.

| runtime kind | graph 파일 |
|---|---|
| Projectile | `choices/projectile/skill_graph_nodes_projectile.csv` |
| LineAttack | `choices/line_attack/skill_graph_nodes_line_attack.csv` |
| Buff | `choices/buff/skill_graph_nodes_buff.csv` |
| SingleAttack | `choices/single_attack/skill_graph_nodes_single_attack.csv` |
| Passive | `choices/passive/skill_graph_nodes_passive.csv` |

공통 열:

```text
monster_id,owner_kind,owner_id,graph_kind,graph_index,target_skill_id,node_order,node_type_id,arg_1..arg_12,excludes_active_choice_id
```

따라서 graph 파일 또는 graph 열 신설은 필요 없다.

### 3.3 direct node 동시 이전 근거

`PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`의 `MaterializeSkillGraphRows(...)`는 positional graph가 있는 몬스터에 legacy direct node가 하나라도 남으면 오류를 만든다.

```text
Monster '{monsterId}' has both skill_graph_nodes rows and legacy node '{nodeId}'. Remove one authoring path.
```

Vega는 현재 direct node 4행을 갖고 있으므로 첫 Vega graph 행을 추가하는 변경에서 4행/11 param도 모두 positional graph로 옮겨야 한다.

### 3.4 현재 런타임 재사용 근거

- `SkillExecutionSnapshot`은 burst damage/status, follow-up projectile, threshold status, target-status stack damage, consume ratio override를 이미 실행한다.
- `InGameSkillDefinitionMapper`는 `RepeatPerTarget`, `TargetStatusCritBonus`, `RedistributeConsumedStatus` direct node를 이미 `SkillChoiceEffectSpec`으로 매핑한다.
- `PakuriCsvRuntimeData.Build.cs`는 Effect graph 조립을 지원하며 최종 definition에 source-status gate와 status runtime-kind filter 필드가 이미 있다.
- Trigger loader는 CSV에 열이 있으면 `triggered_graph_owner_kind/id/kind/index`를 읽고 validation은 graph Effect 참조를 검사한다.

따라서 신규 gameplay 시스템이 아니라 이미 실행되는 필드를 positional node로 authoring할 수 있게 노출하는 작업이다.

## 4. 전환 원칙

1. base skill 10행은 유지한다.
2. Choice의 순수 행동 수치는 `Plan` 또는 `Effect` graph로 이동하고 wide 값은 빈 값으로 만든다.
3. `runtime_target_skill_ids`, `required_source_status_id`, `required_source_status_min_stacks`는 선택 적용 범위/source gate이므로 필요한 Choice에 유지한다.
4. Trigger의 event, source, active/excluded Choice, internal cooldown, target, cooldown refund는 Trigger CSV에 유지한다.
5. Trigger가 호출하는 legacy Effect만 Trigger-owner `Effect` graph로 바꾼다.
6. Effect graph 하나에는 operation node를 정확히 하나 둔다.
7. passive 상시 효과는 Skill-owner, 선택 효과는 Choice-owner, Trigger 호출 효과는 Trigger-owner graph를 사용한다.
8. 동일 행동의 wide 값과 node를 동시에 활성화하지 않는다.
9. visual/collider offset은 추가하지 않고 기존 0 상태를 유지한다.

## 5. 그대로 재사용할 node

### Plan

| node | Vega 사용처 |
|---|---|
| `DamageMultiplier` | A/B/D/E 일반 위력 배율 |
| `MagazineBonus` | A 특성 2 |
| `ReloadTimeMultiplier` | A 특성 3, C 특성 4 |
| `ConditionalDamageMultiplier` | A 특성 5, C 특성 5, D 특성 4 |
| `StatusDurationBonus` | B 특성 2, G 특성 2 |
| `CooldownMultiplier` | B/D/E 쿨다운 선택지 |
| `RadiusMultiplier` | B/D 반경 선택지 |
| `DurationBonus` | C 특성 1, C 마스터 1/2 |
| `DurationMultiplier` | `vega-h-base-duration` |
| `StatusActionSpeedBonus` | C 특성 2, C 마스터 2 |
| `StatusAttackPowerBonus` | C 특성 3, C 마스터 2 |
| `StatusStackAmountBonus` | C 마스터 1의 `name-mark +1` |
| `StatusStackAmountSet` | D 특성 5 |
| `RepeatPerTarget` | D 마스터 1 |
| `CooldownRefundBonus` | E 마스터 2 |

### Effect operation/composition

| node | 용도 |
|---|---|
| `EffectDamage` | A 마스터 2 작은 참격 |
| `ApplyStatus` | `name-mark`, `silence` 부여 |
| `StatusModifier` | F-J passive-buff 생성 |
| `AttachStatusPayload` | 피해 Effect에 `name-mark` 부착 |
| `EffectTarget` | 대상/형태/중심/타이밍 |
| `EffectLifetime` | passive-buff 지속시간 |
| `EffectVisual` | A 마스터 2 prefab |
| `ConditionStatus` | status/stack 조건 |
| `StatusDamageTakenBonus` | F/G/H/I/J 피해 증가 debuff |
| `StatusFlatElementResistReduction` | F 물리 저항 고정 감소 |
| `StatusActionSpeedBonus` | H 행동속도 aura |
| `StatusAttackPowerBonus` | H 공격력 aura |

## 6. positional graph 노출이 필요한 기존 기능

### 6.1 Choice Plan handler

| 신규 node type | params | 기존 필드 | 사용처 |
|---|---|---|---|
| `BurstDamageRule` | `projectile_index`, `multiplier` | burst damage rule | A 특성 4 |
| `FollowUpProjectile` | `count`, `delay_seconds`, `damage_multiplier` | follow-up projectile | A 마스터 1 |
| `ThresholdApplyStatus` | `source_status_id`, `min_stacks`, `apply_status_id` | threshold status | B 마스터 2 |
| `TargetStatusStackDamageMultiplier` | `multiplier` | stack damage multiplier | E 특성 2, E 마스터 1 |
| `ConsumeTargetStatusRatioOverride` | `ratio` | consume ratio override | E 마스터 1 |
| `BurstStatusStacksBonus` | `projectile_index`, `bonus` | burst status rule | F 특성 3 |

필요 변경:

- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`: handler schema와 wide/node overlap validation
- `InGameSkillDefinitionMapper.cs`: node param을 기존 `SkillChoiceEffectSpec` 필드에 매핑
- `skill_node_definitions.csv`, `skill_node_definition_params.csv`

Snapshot과 executor는 해당 필드를 이미 소비하므로 새 gameplay 의미를 만들지 않는다.

### 6.2 direct node의 positional definition 노출

아래 handler schema와 runtime mapper는 이미 있지만 positional definition CSV에는 없다.

| node type | params | 현재 legacy node |
|---|---|---|
| `TargetStatusCritBonus` | `status_id`, `crit_chance_bonus`, `crit_damage_bonus`, `min_stacks` | `vega-e-trait-4-conditional-crit` |
| `RedistributeConsumedStatus` | `status_id`, `ratio`, `radius`, `stacks`, `target_count` | `vega-e-trait-5-redistribute-consumed-status` |

두 node는 코드 handler 신설 없이 definition/param CSV 노출로 positional graph에서 재사용한다.

### 6.3 Effect composer 확장

| 신규/확장 node | params | 사용처 |
|---|---|---|
| `RequiredSourceStatus` | `status_id`, `min_stacks` | H의 `slaughter-permit` gate 4개 |
| `StatusRuntimeKindFilter` | `incoming_skill_runtime_kinds`, `outgoing_skill_runtime_kinds` | I의 `Area` 필터 6개 |
| `StatusCriticalResistanceBonus` | `bonus` | G 특성 3의 `-0.10` |
| `EffectTarget` param 확장 | 기존 optional `cover_all` | battlefield/all-allies 효과 |
| `StatusModifier` param 확장 | 기존 optional `status_target_scope`, `status_merge_policy` | `all_allies`, `same_source_refresh` 보존 |

`PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`에 schema/Effect-only 분류를 추가하고, `PakuriCsvRuntimeData.Build.cs`에서 기존 `SkillEffectDefinition` 필드로 대입한다.

### 6.4 LineAttack Trigger graph 참조 열

Projectile/Passive Trigger CSV에는 아래 4열이 이미 있지만 `line_attack_skill_triger.csv`에는 없다.

```text
triggered_graph_owner_kind
triggered_graph_owner_id
triggered_graph_kind
triggered_graph_index
```

B 마스터 1 Trigger는 실행 envelope를 유지하고 silence legacy Effect만 Trigger-owner graph로 바꿔야 한다. 따라서 line attack Trigger CSV에 위 4열을 추가한다. loader/validation은 optional column을 이미 지원하므로 새 parser 동작은 필요 없다.

## 7. Vega-A 삼검난무

base `vega-a`의 3연사, 탄창 5, 재장전 4.8초, `name-mark` 1스택, 기본 마지막 탄환 배율은 유지한다.

| Choice | graph |
|---|---|
| trait 1 | `DamageMultiplier(1.2)` |
| trait 2 | `MagazineBonus(2)` |
| trait 3 | `ReloadTimeMultiplier(0.8)` |
| trait 4 | `BurstDamageRule(0, 1.5)` |
| trait 5 | `ConditionalDamageMultiplier(name-mark, 10, 1.25)` |
| master 1 | `FollowUpProjectile(1, 0, 0.45)` |
| master 2 | Trigger 유지 + Trigger-owner Effect graph |

master 1 후속 투사체는 기존 A의 `name-mark` status spec을 상속하는 executor 동작을 유지한다.

master 2 graph:

```text
Owner: Trigger / vega-a-master2-kill-transfer / Effect / 0
Target: vega-a
EffectDamage(Physical, attack_power_coefficient=0.5)
EffectTarget(Enemy, Nearest, Single, NearestEnemy, ..., OnCast)
EffectVisual(Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab)
AttachStatusPayload(name-mark, chance=1, max_stacks=0, stack_amount=3)
```

Trigger의 `triggered_effect_id`를 비우고 graph reference를 채운다.

### 현재 불일치

`vega-a-trait-4`에는 `burst_damage_multiplier=1.5`만 있고 projectile index가 없다. Snapshot은 index와 multiplier가 모두 있을 때만 choice burst rule을 추가한다. 현재 실제 경로는 설명의 “마지막 탄환 +50%”를 적용하지 않는 것으로 읽힌다.

사용자 승인에 따라 `projectile_index=0`, `multiplier=1.5`를 적용했다. `SkillExecutionSnapshot.MatchesBurstProjectileIndex(...)`의 index 0 마지막 탄환 의미를 사용한다.

## 8. Vega-B 침묵의 대태도

| Choice | graph |
|---|---|
| trait 1 | `DamageMultiplier(1.25)` |
| trait 2 | `StatusDurationBonus(silence, 1)` |
| trait 3 | `CooldownMultiplier(0.8)` |
| trait 4 | `RadiusMultiplier(1.3)` |
| trait 5 | Choice-owner `ApplyStatus(name-mark)` Effect, stack 2 |
| master 1 | 두 번째 참격 Trigger 유지 + silence Effect |
| master 2 | `DamageMultiplier(1.7)`, `RadiusMultiplier(0.75)`, `ThresholdApplyStatus(name-mark, 10, silence)` |

B master 1 Trigger의 base damage 30, attack coefficient 1.4, damage multiplier 0.45, radius 1.8, delay 0.4초는 Trigger CSV에 유지한다. Trigger가 실행하는 1초 silence만 Trigger-owner Effect graph로 이전한다.

## 9. Vega-C 몰살 허가

| Choice | graph |
|---|---|
| trait 1 | `DurationBonus(2)` |
| trait 2 | `StatusActionSpeedBonus(0.1)` |
| trait 3 | `StatusAttackPowerBonus(0.1)` |
| trait 4 | target A `ReloadTimeMultiplier(0.7692307692307692)` |
| trait 5 | `runtime_target_skill_ids=A/B/D/E` 유지 + `ConditionalDamageMultiplier(name-mark, 1, 1.15)` 1행 |
| master 1 | `DurationBonus(2)` + target A `StatusStackAmountBonus(name-mark, 1)` |
| master 2 | `DurationBonus(-2)`, `StatusActionSpeedBonus(0.25)`, `StatusAttackPowerBonus(0.25)` |

trait 4/5와 master 1의 `runtime_target_skill_ids`, `required_source_status_id=slaughter-permit`, `required_source_status_min_stacks=1`은 cross-skill 적용/gate이므로 유지한다. Snapshot은 choice의 `runtime_target_skill_ids`를 먼저 fanout하므로 trait 5 node를 대상별로 4회 복제하지 않는다. 같은 multiplier node 4행은 각 대상 스냅샷에서 4중 적용되므로 1행이 기존 의미와 맞다.

## 10. Vega-D 검은 명부 개방

| Choice | graph |
|---|---|
| trait 1 | `DamageMultiplier(1.25)` |
| trait 2 | `RadiusMultiplier(1.25)` |
| trait 3 | `CooldownMultiplier(0.8)` |
| trait 4 | `ConditionalDamageMultiplier(name-mark, 10, 1.3)` |
| trait 5 | `StatusStackAmountSet(name-mark, 1)` |
| master 1 | `DamageMultiplier(0.65)` + `RepeatPerTarget(2, 0.15, 0.6)` |
| master 2 | `DamageMultiplier(1.3)`, `CooldownMultiplier(1.2)`, `RadiusMultiplier(1.5)` |

master 1의 legacy node 2행/param 4행을 같은 param으로 positional graph에 옮기고 기존 행을 삭제한다.

## 11. Vega-E 최종선고

| Choice | graph |
|---|---|
| trait 1 | `DamageMultiplier(1.25)` |
| trait 2 | `TargetStatusStackDamageMultiplier(1.25)` |
| trait 3 | `CooldownMultiplier(0.8)` |
| trait 4 | `TargetStatusCritBonus(name-mark, 0.35, 0, min_stacks)` |
| trait 5 | `RedistributeConsumedStatus(name-mark, 0.25, 5, 0, 3)` |
| master 1 | `TargetStatusStackDamageMultiplier(1.8)` + `ConsumeTargetStatusRatioOverride(1)` |
| master 2 | `DamageMultiplier(0.8)` + `CooldownRefundBonus(0.7)` |

trait 4/5의 legacy direct node 2행/param 7행을 positional graph로 옮긴 뒤 기존 행을 삭제한다.

### 현재 불일치

E trait 4 설명은 `name-mark` 20스택 이상이지만 전환 전 direct param은 `min_stacks=1`이었다. 사용자 승인에 따라 positional `TargetStatusCritBonus`의 `min_stacks=20`으로 정정했다.

## 12. Vega-F 각인 심화

- Skill-owner Effect: `name-mark` 대상 피해 증가 `+0.10`
- Choice-owner trait 1 Effect: 추가 `+0.05`
- Skill-owner Effect: `name-mark:10` 대상 Physical flat resist reduction `8`
- Choice-owner trait 2 Effect: 추가 `4`
- Choice-owner trait 3 Plan: target A `BurstStatusStacksBonus(0, 1)`

F trait 1 wide `damage_multiplier=1.05`는 같은 +5%를 Effect가 표현한 shadow 값이다. Passive Plan damage로 옮기지 않고 Effect graph만 남긴 뒤 wide 값을 비운다.

trait 3은 `runtime_target_skill_ids=vega-a`를 유지한다. index 0은 `SkillExecutionSnapshot.MatchesBurstProjectileIndex(...)`에서 마지막 탄환을 뜻한다.

## 13. Vega-G 봉인검식

- Skill-owner Effect: `silence` 대상 피해 증가 `+0.14`
- Choice-owner trait 1 Effect: 추가 `+0.06`
- trait 2 Plan: target B `StatusDurationBonus(silence, 1)`
- Choice-owner trait 3 Effect: `ConditionStatus(silence&name-mark)` + `StatusCriticalResistanceBonus(-0.10)`
- `vega-g-mark-on-hit-base` Trigger 유지 + Trigger-owner `name-mark` 2스택 Effect

G trait 1 wide `damage_multiplier=1.06`도 Effect와 중복된 shadow 값이므로 Effect graph만 남긴다.

## 14. Vega-H 처형 준비

- `vega-h-base-duration`: target C `DurationMultiplier(1.2)`
- Skill-owner Effect: `slaughter-permit` 중 AllAllies 행동속도 `+0.12`
- Choice-owner trait 1 Effect: 추가 `+0.06`
- Choice-owner trait 2 Effect: AllAllies 공격력 `+0.08`
- Choice-owner trait 3 Effect: source gate + `ConditionStatus(name-mark)` + 적 피해 증가 `+0.10`

각 Effect에 `RequiredSourceStatus(slaughter-permit, 1)`을 둔다. trait 3 wide `damage_multiplier=1.1`은 Effect와 중복된 shadow 값이므로 제거한다. `vega-h-base-duration`의 `runtime_target_skill_ids=vega-c`는 유지한다.

## 15. Vega-I 연쇄 참결

### Trigger 유지

Vega-I Trigger 7개는 `vega-d` outgoing damage, trait 2 유무, trait 1/3 조합, 모든 아군 Area damage 감지, internal cooldown 1초, D cooldown 3% 반환 envelope를 유지한다.

legacy single-attack Effect 6행을 각각 대응 Trigger-owner graph로 옮긴다.

| 변형 | 지속시간 | 피해 증가 | 추가 조건 |
|---|---:|---:|---|
| base | 4 | 0.15 | incoming `Area` |
| trait 2 | 6 | 0.15 | incoming `Area` |
| trait 1 | 4 | 0.07 | incoming `Area` |
| trait 1 + 2 | 6 | 0.07 | incoming `Area` |
| trait 3 | 4 | 0.10 | `name-mark:10`, incoming `Area` |
| trait 3 + 2 | 6 | 0.10 | `name-mark:10`, incoming `Area` |

각 graph는 `StatusModifier`, `EffectTarget`, `EffectLifetime`, `StatusRuntimeKindFilter`, 선택적 `ConditionStatus`, `StatusDamageTakenBonus`로 조립한다.

## 16. Vega-J 사형 집행인

다음 Trigger는 유지한다.

- OnKill all-allies cooldown refund `0.20`
- trait 1 추가 refund `0.10`
- trait 3의 `vega-d` refund `0.20`
- `vega-e` 대상 생존 시 debuff Trigger 2개

생존 Effect 2행은 Trigger-owner graph로 옮긴다.

| 변형 | 지속시간 | 모든 아군 피해 증가 |
|---|---:|---:|
| base | 5 | 0.10 |
| trait 2 | 5 | 0.15 |

CooldownRefund Trigger 3행은 Effect graph로 바꾸지 않는다.

## 17. legacy 제거 계획

### Effect

| 파일 | 제거할 Vega 행 수 |
|---|---:|
| `effects/projectile/projectile_skill_effects.csv` | 1 |
| `effects/line_attack/line_attack_skill_effects.csv` | 3 |
| `effects/single_attack/single_attack_skill_effects.csv` | 8 |
| `effects/passive/passive_skill_effects.csv` | 11 |
| 합계 | 23 |

### direct node

`nodes/single_attack/single_attack_skill_nodes.csv`에서 아래 4행을 제거한다.

- `vega-d-master-1-damage-multiplier`
- `vega-d-master-1-repeat-per-target`
- `vega-e-trait-4-conditional-crit`
- `vega-e-trait-5-redistribute-consumed-status`

대응 param 11행도 함께 제거한다.

### Trigger

Trigger 15행은 삭제하지 않는다.

- Effect action Trigger의 `triggered_effect_id`만 비운다.
- 대응 graph reference 4열을 채운다.
- LineAttack Trigger 파일에는 동일 4열을 먼저 추가한다.
- 직접 CooldownRefund/LineAttack action은 기존 필드를 유지한다.

## 18. 구현 순서

### Phase 0: 의미 불일치 승인

1. 완료: A trait 4는 `BurstDamageRule(0, 1.5)`로 마지막 탄환 +50%를 실현한다.
2. 완료: E trait 4는 설명의 `min_stacks=20`을 적용한다.
3. 완료: 구현 결과와 검증 근거를 MON/DATA/COMBAT 보드에 기록한다.

### Phase 1: 공용 positional node 기반

1. Plan handler 6개 schema/definition/param 등록
2. `TargetStatusCritBonus`, `RedistributeConsumedStatus` positional definition 등록
3. Effect composer handler 3개와 기존 optional param 노출
4. Mapper/Build 매핑 및 overlap/graph validation 보강
5. line attack Trigger graph reference 4열 추가

### Phase 2: A-E active 이전

1. projectile/line/buff/single graph 작성
2. A/B Trigger Effect 이전
3. D/E direct node 4행/param 11행 제거
4. A/B legacy Effect 4행 제거

### Phase 3: F-J passive 이전

1. F/G/H Skill/Choice-owner Effect graph 작성
2. G/I/J Trigger-owner Effect graph 작성
3. F/G/H shadow wide damage 값 제거
4. passive Effect 11행과 single-attack Effect 8행 제거

### Phase 4: 검증

1. 5개 graph 파일 21열 유지
2. Vega graph 존재, legacy direct node/param 0
3. legacy Effect 0, Trigger 15 유지
4. Choice wide/node overlap 0
5. `PakuriDataManager` runtime data validation 0 error
6. 관련 EditMode 테스트와 compile/reload 오류 0
7. 사용자 Play Mode 인계

## 19. 위험과 중단 조건

1. **A trait 4 의미 변경**: 사용자 승인값 `projectile_index=0`, `multiplier=1.5`를 유지한다.
2. **E trait 4 수치 불일치**: 사용자 승인값 `min_stacks=20`을 유지한다.
3. **line trigger schema**: B master 1 완전 이전에는 graph 참조 4열 추가가 필요하다.
4. **source gate 손실**: H Effect를 `ConditionStatus`로 대체하지 않고 `RequiredSourceStatus`로 보존한다.
5. **Area filter 손실**: I Effect는 반드시 incoming `Area` filter를 보존한다.
6. **wide/node 중복**: 행동 wide 값은 node 활성화와 동시에 비운다.
7. **direct node 혼용**: Vega graph 추가 시 legacy direct node 4행을 반드시 제거한다.
8. **경계 확대**: 새 gameplay 규칙, 새 graph 파일/열, reference 탐색이 필요하면 중단하고 보고한다. 문서에 명시한 line Trigger 4열은 승인 대상 schema 변경이다.

## 20. 수용 기준

### 기능

- A-E 35 Choice가 승인된 의미로 동작한다.
- F-J passive 효과, 조건, source gate, Area filter가 보존된다.
- Trigger 15개의 event/choice/internal cooldown/target/refund 의미가 보존된다.
- Trigger Effect가 graph reference로 실행된다.

### 데이터

- Vega graph는 5개 기존 graph 파일에만 존재한다.
- Vega legacy Effect/direct node/direct param은 각각 0이다.
- Vega Trigger는 15행이다.
- graph와 겹치는 Choice wide 행동 값은 비어 있다.
- cross-skill routing/source gate metadata는 필요한 행에 남는다.
- object/collider offset은 추가되지 않고 0 상태를 유지한다.

### 검증 경계

- CSV parse/graph materialization error 0
- node definition/param validation error 0
- triggered graph reference validation error 0
- runtime data validation error 0
- 관련 EditMode test 통과
- 사용자 Play Mode 인계 기록

## 21. 관련 보드

- `boards/MON/VEGA_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- 형식 참고: `boards/MON/SEIN_NODE_MIGRATION_PROPOSAL.md`

## 22. Evidence

- Vega base/Choice/Effect/Trigger/node CSV: `Pakuri/Assets/CSVdata/authoring/monster/skills/`
- graph materializer/schema/overlap: `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`
- Effect graph 조립: `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`
- Trigger graph 열 파싱/검증: `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Validation.cs`
- Choice node runtime 매핑: `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
- 기존 필드 실행: `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`

## 23. 구현 결과 (2026-07-13)

### 데이터 전환

- Vega positional graph: 154행, 58 graph (`Plan` 45행/35 graph, `Effect` 109행/23 graph).
- Vega legacy Effect 23행은 0행으로, direct node 4행과 param 11행은 각각 0행으로 전환했다.
- Vega Trigger 15행은 유지했고 그중 11행은 Trigger-owner Effect graph를 참조한다.
- graph와 중복되는 Vega Choice wide 행동 값은 0개다. `runtime_target_skill_ids`와 source-status gate metadata만 유지했다.
- A trait 4는 projectile graph의 `BurstDamageRule(0, 1.5)`, E trait 4는 single-attack graph의 `TargetStatusCritBonus(name-mark, 0.35, min_stacks=20)`이다.

### 공용 런타임 연결

- Plan positional handler 6개와 direct-node 이전 handler 2개를 definition/param/schema에 노출했다.
- Effect composer에 `RequiredSourceStatus`, `StatusRuntimeKindFilter`, `StatusCriticalResistanceBonus`를 연결했다.
- `ConditionStatusExpression`은 기존 `ConditionStatus` handler를 재사용해 `silence&name-mark` AND 표현식을 positional arg로 보존한다.
- node-backed Choice에서 action op로 변환되지 않는 9개 handler는 `ApplyNormalizedChoiceCompatibilityNodes(...)`와 `ApplyNodeBackedChoiceFields(...)`를 거쳐 기존 Snapshot 필드에 적용된다. action op handler는 기존 `ApplyPlanActionNodes(...)` 경로만 사용해 중복 적용하지 않는다.
- line-attack Trigger CSV에 기존 다른 Trigger kind와 같은 graph reference 4열을 추가했다. 새 graph 파일, graph 열, offset 열은 추가하지 않았다.

### 검증

- PowerShell graph 계약 검사: 알 수 없는 node type, 중복 graph order, required/undefined arg, Effect graph operation 수, Trigger graph 참조 오류 0.
- CSV shape: 변경된 runtime CSV 30개 모두 header와 각 행 field 수 일치.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore`: 오류 0, 기존 `MSB3277` 경고 2.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore`: 오류 0, 기존 `MSB3277` 경고 2.
- Unity-MCP `Pakuri/Validate CSV Source Data`: runtime catalog 5 monsters, stage-one 8, stage-two 8 로드 성공.
- Unity-MCP `Pakuri/InGame/Validate Skill Data`: `0 warning(s)` 통과.
- Unity EditMode runner는 test case 0개를 발견했다. `TestResults.xml`은 `total=0`, `failed=0`, `result=Passed`이며 MCP job wrapper는 실행할 test가 없어 initialization timeout으로 종료됐다.
- Play Mode는 사용자 소유이므로 실행하지 않았다.
