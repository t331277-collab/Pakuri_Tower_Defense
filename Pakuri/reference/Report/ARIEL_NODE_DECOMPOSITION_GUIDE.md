# Current Skill Graph Conversion Guide

## 1. 문서 목적

이 문서는 현재 디스크의 Ariel `skill_graph_nodes` 구현을 기준으로, 아직
`Pakuri/Assets/CSVdata/runtime/monster/skills/effects/{kind}/{kind}_skill_effects.csv`
행을 사용하는 몬스터를 현재 그래프 작성 방식으로 전환하는 절차를 정의한다.

이 문서의 핵심은 다음 세 가지다.

1. 새 그래프 행은 기존 `*_skill_nodes.csv`와 `*_skill_node_params.csv`를 직접 작성하지 않는다.
2. `node_type_id`와 `arg_1~arg_12`의 의미는 노드 정의 CSV가 결정한다.
3. legacy effect의 넓은 컬럼 중 현재 그래프가 표현하지 못하는 값이 하나라도 있으면 삭제하지 않고 중단한다.

이 문서는 Designer의 전환 가이드다. 이 문서 수정 자체는 런타임 코드, 스킬 CSV 행,
프리팹 또는 씬을 변경하지 않는다.

## 2. 현재 검사 근거

### 2.1 CSV 권한

- 기본 스킬:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/base/{kind}/skills_{kind}.csv`
- Choice 메타데이터와 kind별 전용 값:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/choices/{kind}/skill_choices_{kind}.csv`
- 현재 그래프 인스턴스:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/choices/{kind}/skill_graph_nodes_{kind}.csv`
- 노드 타입 정의:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/definitions/skill_node_definitions.csv`
- 노드 인자 정의:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/definitions/skill_node_definition_params.csv`
- legacy direct node 호환 데이터:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/{kind}/{kind}_skill_nodes.csv`
  및 `{kind}_skill_node_params.csv`
- legacy effect 호환 데이터:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/effects/{kind}/{kind}_skill_effects.csv`
- Trigger:
  `Pakuri/Assets/CSVdata/runtime/monster/skills/triggers/{kind}/{kind}_skill_triger.csv`

### 2.2 런타임 코드

- 그래프 파싱, materialize, 검증, 생성 ID:
  `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`
- Trigger graph-reference 파싱:
  `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`
- Effect graph 조립:
  `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`
- Choice/Skill plan 소비:
  `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
  및 `SkillExecutionSnapshot.cs`

### 2.3 2026-07-11 현재 디스크 집계

type row를 제외한 실제 데이터 기준이다.

| 항목 | 현재 값 |
|---|---:|
| Ariel graph node 행 | 124 |
| Choice / Plan | 39행, 36그래프 |
| Choice / Effect | 45행, 11그래프 |
| Skill / Effect | 36행, 8그래프 |
| Trigger / Effect | 4행, 1그래프 |
| 전체 Effect graph | 20그래프 |
| legacy direct node | 15행: Rin 11, Vega 4 |
| legacy direct node param | 33행 |
| legacy effect | 96행: Eve 34, Rin 20, Sein 19, Vega 23 |

현재 graph 파일은 다음 네 kind에만 존재한다.

- `buff`
- `passive`
- `projectile`
- `single_attack`

현재 `area_attack`과 `line_attack`에는 `skill_graph_nodes_{kind}.csv`가 없다.
해당 kind 전환에 새 파일이 필요하면 새 CSV 파일 추가 범위이므로 사용자 승인 없이 만들지 않는다.

## 3. 현재 작성 모델

### 3.1 그래프 행 스키마

현재 네 graph CSV는 모두 동일한 21개 컬럼을 사용한다.

```text
monster_id,owner_kind,owner_id,graph_kind,graph_index,target_skill_id,
node_order,node_type_id,arg_1,arg_2,arg_3,arg_4,arg_5,arg_6,
arg_7,arg_8,arg_9,arg_10,arg_11,arg_12,excludes_active_choice_id
```

각 행은 독립 효과 하나가 아니라 그래프 안의 노드 하나다. 같은 아래 키를 가진 행들이
하나의 그래프를 구성한다.

```text
monster_id + owner_kind + owner_id + graph_kind + graph_index
```

`node_order`는 해당 그래프 안에서 중복될 수 없다.

### 3.2 현재 지원되는 owner_kind

그래프 target 해석 코드가 현재 지원하는 owner는 다음 세 종류다.

| owner_kind | owner_id 의미 | 기본 target_skill_id |
|---|---|---|
| `Choice` | 실제 `choice_id` | Choice의 `target_skill_id`, 없으면 Choice의 `skill_id` |
| `Skill` | 실제 `skill_id` | owner인 skill 자체 |
| `Trigger` | 실제 `trigger_id` | Trigger의 `source_skill_id` |

행의 `target_skill_id`가 비어 있지 않으면 위 기본값을 덮어쓴다.

`Passive`나 `Effect`는 enum 이름이 존재하더라도 현재 graph target 해석에서 지원되지 않는다.
새 그래프 owner로 사용하지 않는다.

### 3.3 graph_kind

| graph_kind | 역할 | 규칙 |
|---|---|---|
| `Plan` | 기존 스킬 실행 수치나 Choice modifier | Effect 전용 handler 사용 금지 |
| `Effect` | 별도 `SkillEffectDefinition` 생성 | 주 연산 handler가 정확히 1개 필요 |

`Choice + Effect`는 materialize 과정에서 `requires_active_choice_id=owner_id`가 자동 생성된다.
따라서 Choice 효과의 활성 조건을 별도 컬럼에 반복하지 않는다.

현재 그래프 인스턴스에 남아 있는 추가 gate는 `excludes_active_choice_id` 하나뿐이다.

### 3.4 같은 몬스터의 두 node 작성 방식 혼용 금지

현재 materialize 검증은 어떤 몬스터에 `skill_graph_nodes` 행이 하나라도 있으면,
그 몬스터의 legacy `*_skill_nodes.csv` 행이 동시에 존재하는 것을 오류로 처리한다.

따라서:

- Eve와 Sein은 현재 legacy direct node가 없으므로 graph 전환을 시작할 수 있다.
- Rin은 legacy direct node 11행을 함께 처리하지 않으면 graph 행을 추가할 수 없다.
- Vega는 legacy direct node 4행을 함께 처리하지 않으면 graph 행을 추가할 수 없다.
- Rin/Vega legacy handler가 현재 `skill_node_definitions.csv`에 없으면 정의 확장 승인을 먼저 받는다.

몬스터 단위 전환 경계를 무시하고 effect 행만 일부 graph로 옮기지 않는다.

## 4. `node_type_id`와 `arg_1~arg_12`

### 4.1 위치 기반 매핑

`arg_N`은 이름 없는 자유 입력값이 아니다.
`skill_node_definition_params.csv`의 다음 관계로 해석된다.

```text
graph.node_type_id
  -> definition param의 node_type_id
  -> param_order N
  -> arg_N
  -> param_key + value_type + value
```

필수 param의 `arg_N`이 비어 있으면 검증 오류다. 해당 node type에 정의되지 않은
`arg_N`에 값을 넣어도 검증 오류다.

현재 정의의 최대 `param_order`는 `EffectTarget`의 8이다.
따라서 `arg_9~arg_12`는 현재 사용할 수 없는 예약 슬롯이다.

현재 Ariel 124행에서 실제 사용된 가장 높은 슬롯은 `arg_5`지만,
정의상 `EffectTarget`만 `arg_6~arg_8`까지 사용할 수 있다.

### 4.2 현재 node type 인자표

아래 표가 현재 graph 작성의 인자 권한이다.

| node_type_id | arg 배치 |
|---|---|
| `ApplyShield` | 1 `base_damage`, 2 `spell_power_coefficient` |
| `ApplyStatus` | 1 `status_id` 필수 |
| `ConditionSkillAttribute` | 1 `attribute` 필수 |
| `ConditionStatus` | 1 `status_id` 필수, 2 `target_side`, 3 `source_skill_id`, 4 `min_stacks` |
| `CooldownMultiplier` | 1 `multiplier` 필수 |
| `CountStatusDamageMultiplier` | 1 `status_id`, 2 `target_side`, 3 `amount_per_count`; 모두 필수 |
| `DamageMultiplier` | 1 `multiplier` 필수 |
| `DurationBonus` | 1 `bonus_seconds` 필수 |
| `EffectDamage` | 1 `attribute` 필수, 2 `base_damage`, 3 `spell_power_coefficient`, 4 `damage_multiplier`, 5 `radius` |
| `EffectExtendStatusDuration` | 1 `status_id` 필수 |
| `EffectLifetime` | 1 `duration_seconds` 필수 |
| `EffectTarget` | 1 `target_side`, 2 `target_selection`, 3 `target_shape`, 4 `center_mode`, 5 `visual_anchor_mode`, 6 `effect_timing`, 7 `delay_seconds`, 8 `apply_once` |
| `EffectVisual` | 1 `skill_effect_prefab_path` 필수 |
| `HitTargetCountBonus` | 1 `bonus` 필수 |
| `MagazineBonus` | 1 `bonus` 필수 |
| `PierceBonus` | 1 `bonus` 필수 |
| `RadiusMultiplier` | 1 `multiplier` 필수 |
| `ReloadTimeMultiplier` | 1 `multiplier` 필수 |
| `ShieldAmountMultiplier` | 1 `multiplier` 필수 |
| `StatusActionSpeedBonus` | 1 `bonus` 필수, 2 `status_id` |
| `StatusAilmentResistanceBonus` | 1 `bonus` 필수 |
| `StatusConditionalDamageTakenBonus` | 1 `source_status_id`, 2 `bonus`; 모두 필수 |
| `StatusCriticalChanceBonus` | 1 `bonus` 필수 |
| `StatusCriticalDamageTakenBonus` | 1 `bonus` 필수 |
| `StatusDamageBonusRate` | 1 `bonus` 필수, 2 `attribute` |
| `StatusDamageTakenBonus` | 1 `bonus` 필수 |
| `StatusDurationBonus` | 1 `status_id`, 2 `bonus_seconds`; 모두 필수 |
| `StatusElementDamageTakenBonus` | 1 `bonus` 필수 |
| `StatusFlatElementResistReduction` | 1 `bonus` 필수, 2 `attribute` |
| `StatusModifier` | 인자 없음 |
| `StatusShieldReceivedBonus` | 1 `bonus` 필수 |
| `StatusSpellPowerBonus` | 1 `bonus` 필수 |

새 `node_type_id`, 새 param, 또는 기존 param 순서 변경은 정의 CSV와 런타임 계약 변경이다.
선택된 전환 작업 범위를 넘어가므로 사용자 승인 없이 추가하지 않는다.

## 5. Effect graph 조립 규칙

### 5.1 주 연산은 정확히 하나

현재 graph에서 사용할 수 있는 주 연산은 다음 다섯 개다.

| 주 연산 | 생성 의미 |
|---|---|
| `ApplyStatus` | 지정 `status_id` 적용 |
| `ApplyShield` | `shield` 상태와 방어막량 생성 |
| `StatusModifier` | `passive-buff` 기반 수치 modifier 생성 |
| `EffectDamage` | 독립 Damage effect 생성 |
| `EffectExtendStatusDuration` | 기존 status 지속시간 연장 |

코드에는 legacy 호환용 `EffectStatus` 분기가 남아 있지만 현재
`skill_node_definitions.csv`에 `EffectStatus` 정의가 없으므로 graph에 작성할 수 없다.

### 5.2 Effect 전용 조합 노드

| node_type_id | 역할 |
|---|---|
| `EffectTarget` | 대상, 선택, 형태, 중심, 시각 anchor, timing, delay, apply-once |
| `EffectVisual` | effect prefab |
| `ConditionStatus` | status, 대상 side, source skill, 최소 stack 조건 |
| `ConditionSkillAttribute` | skill attribute 조건 |
| `EffectLifetime` | Damage의 active duration 또는 Status의 status duration |

현재 기본값은 코드에서 다음처럼 생성된다.

```text
TargetSide = Enemy
TargetSelection = Nearest
TargetShape = Single
CenterMode = PrimarySkillCenter
VisualAnchorMode = Center
EffectTiming = OnCast
DamageMultiplier = 1
StatusChance = 1
StatusMaxStacks = 1
StatusStackAmount = 1
```

기능적으로 기본값과 같은 값은 생략할 수 있다. 다만 `AllAllies`, `EventTarget`,
`Battlefield`, `Delayed`, `apply_once`처럼 동작을 바꾸는 값은 명시한다.

### 5.3 Effect composer가 현재 실제 소비하는 modifier

- `DamageMultiplier`
- `ShieldAmountMultiplier`
- `StatusActionSpeedBonus`
- `StatusSpellPowerBonus`
- `StatusDamageBonusRate`
- `StatusShieldReceivedBonus`
- `StatusDamageTakenBonus`
- `StatusFlatElementResistReduction`
- `StatusCriticalChanceBonus`

다른 node type이 정의 CSV에 있다는 이유만으로 Effect graph에 넣지 않는다.
예를 들어 `StatusElementDamageTakenBonus`와 `StatusCriticalDamageTakenBonus`는
현재 Ariel `Plan`에서 사용되지만 현재 Effect composer 분기에는 없다.

## 6. Plan graph 작성 기준

현재 Ariel `Choice + Plan`에서 실제 사용 중인 node type은 다음과 같다.

- `CooldownMultiplier`
- `CountStatusDamageMultiplier`
- `DamageMultiplier`
- `DurationBonus`
- `HitTargetCountBonus`
- `MagazineBonus`
- `PierceBonus`
- `RadiusMultiplier`
- `ReloadTimeMultiplier`
- `ShieldAmountMultiplier`
- `StatusActionSpeedBonus`
- `StatusAilmentResistanceBonus`
- `StatusConditionalDamageTakenBonus`
- `StatusCriticalDamageTakenBonus`
- `StatusDamageBonusRate`
- `StatusDamageTakenBonus`
- `StatusDurationBonus`
- `StatusElementDamageTakenBonus`
- `StatusShieldReceivedBonus`

단순 수치 수정은 같은 효과의 복제 Effect graph가 아니라 Plan node를 우선한다.
새로운 적용 대상, 실행 시점, 조건, 결과 종류가 생길 때만 별도 Effect graph를 만든다.

## 7. 생성 ID와 Trigger 참조

### 7.1 생성 node ID

각 graph 행은 다음 ID로 materialize된다.

```text
{owner_kind}:{owner_id}:{graph_kind}:{graph_index}:{node_order}
```

### 7.2 생성 Effect ID

legacy `effect_id`를 graph의 `owner_id`에 그대로 복사하는 방식이 아니다.
현재 생성 규칙은 다음과 같다.

| owner_kind | graph_index | 생성 Effect ID |
|---|---:|---|
| `Choice` 또는 `Trigger` | 0 | `{owner_id}` |
| `Choice` 또는 `Trigger` | 1 이상 | `{owner_id}@effect{graph_index + 1}` |
| `Skill` | 0 이상 | `{owner_id}@effect{graph_index + 1}` |

예:

```text
Choice/ariel-a-master-2/Effect/0 -> ariel-a-master-2
Skill/ariel-c/Effect/0           -> ariel-c@effect1
Skill/ariel-g/Effect/1           -> ariel-g@effect2
```

생성 Effect ID가 legacy effect 행의 ID와 겹치면 검증 오류다.
전환 시 생성 ID를 먼저 계산하고 모든 참조를 확인한 뒤 legacy 행을 제거한다.

### 7.3 Trigger graph reference

현재 Ariel은 다음 두 방식의 graph owner를 실제로 사용한다.

- `Choice/ariel-a-master-2/Effect/0`
- `Trigger/ariel-j-after-e-action-speed-trigger/Effect/0`

graph-reference 컬럼이 있는 trigger 행은 다음 tuple을 사용한다.

```text
triggered_graph_owner_kind
triggered_graph_owner_id
triggered_graph_kind
triggered_graph_index
```

현재 이 네 컬럼은 `passive_skill_triger.csv`와 `projectile_skill_triger.csv`에만 존재한다.
다른 trigger kind는 기존 `triggered_effect_id`로 생성 Effect ID를 참조해야 한다.
새 graph-reference 컬럼이 필요하면 새 CSV 컬럼과 parser 범위이므로 사용자 승인을 받는다.

## 8. legacy effect를 현재 graph로 옮기는 매핑

### 8.1 현재 직접 표현 가능한 핵심 필드

| legacy effect 의미 | graph 작성 |
|---|---|
| `effect_kind=Damage` | `EffectDamage` 주 연산 |
| 실제 status 적용 | `ApplyStatus(status_id)` |
| shield 적용 | `ApplyShield` |
| passive-buff/stat aura | `StatusModifier` |
| status 지속시간 연장 | `EffectExtendStatusDuration(status_id)` |
| `target_side`~`effect_timing`, delay, apply-once | `EffectTarget`의 정의된 arg 위치 |
| `condition_status_id`, target side, source skill, min stacks | `ConditionStatus` |
| `condition_skill_attribute` | `ConditionSkillAttribute` |
| `active_duration_seconds` 또는 `status_duration_seconds` | `EffectLifetime` |
| `skill_effect_prefab_path` | `EffectVisual` |
| 받는 피해, 주는 속성 피해, 행동속도 등 현재 composer 지원 수치 | 대응 `Status*Bonus` modifier |
| Choice에 의한 단순 수치 변경 | `Choice + Plan` |

### 8.2 현재 정의 CSV로는 직접 옮길 수 없는 필드

다음은 코드 일부에 필드가 남아 있어도 현재 node param 정의 또는 Effect composer가
완전하게 표현하지 못하는 대표 범위다.

- `EffectTarget.cover_all`
- `EffectDamage.attack_power_coefficient`
- `EffectDamage.tick_interval_seconds`
- `ApplyShield.attack_power_coefficient`
- status chance, label, max stack, stack amount, target scope, merge policy
- shield refresh policy
- status move speed / attack power / element resist reduction
- outgoing additional damage 계열
- target-status conditional status chance
- applied-status별 duration bonus
- health-ratio / hit-count 조건
- status critical damage / critical resistance
- required source status 및 incoming runtime-kind 조건
- graph 정의에 없는 legacy direct-node handler

`Build.cs`에 읽기 분기가 존재하더라도 `skill_node_definition_params.csv`에 param이 없으면
graph의 `arg_N`으로 작성할 수 없다. 반대로 node type이 정의되어 있어도 해당 owner kind의
실제 mapper/composer가 소비하지 않으면 구현된 기능으로 간주하지 않는다.

### 8.3 전환 완료 판단

legacy 행의 모든 non-empty 기능 컬럼을 다음 셋 중 하나로 표시한다.

1. base/choice/trigger의 기존 권한에 이미 존재한다.
2. 현재 graph node와 arg로 옮겼다.
3. 기능적으로 불필요한 기본값이며 코드 기본값과 같다는 근거가 있다.

어느 셋에도 속하지 않는 값이 하나라도 있으면 해당 legacy 행은 삭제하지 않는다.
예전 가이드의 `58/16/22` 분류는 현재 graph schema 이전 집계이므로 더 이상 사용하지 않는다.

## 9. 현재 Ariel 작성 예시

### 9.1 단순 Choice Plan

```csv
ariel,Choice,ariel-a-trait-1,Plan,0,ariel-a,1,DamageMultiplier,1.25,...
```

`DamageMultiplier.arg_1=multiplier`이므로 `1.25`가 스킬 피해 배율이 된다.

### 9.2 Choice가 만드는 Effect

```csv
ariel,Choice,ariel-a-master-2,Effect,0,ariel-a,1,ApplyStatus,holy-exposure,...
ariel,Choice,ariel-a-master-2,Effect,0,ariel-a,2,EffectTarget,,EventTarget,,EffectTarget,AppliedTargets,...
```

이 그래프의 생성 Effect ID는 `ariel-a-master-2`이고 Choice gate도 자동 생성된다.
projectile trigger가 graph tuple로 이 Effect를 참조한다.

### 9.3 Skill 기본 Effect

```text
Skill/ariel-c/Effect/0
1 ApplyStatus(blessing)
2 EffectTarget(AllAllies, Owner, Battlefield, _, AppliedTargets)
3 EffectVisual(Ariel_C-Buff.prefab)
4 EffectLifetime(4)
5 StatusActionSpeedBonus(0.12)
excludes_active_choice_id = ariel-c-master-1
```

이 그래프의 생성 Effect ID는 `ariel-c@effect1`이다.

### 9.4 Trigger owner Effect

```text
Trigger/ariel-j-after-e-action-speed-trigger/Effect/0
1 StatusModifier
2 EffectTarget(AllAllies)
3 EffectLifetime(5)
4 StatusActionSpeedBonus(0.15)
```

생성 Effect ID는 trigger ID와 같은 `ariel-j-after-e-action-speed-trigger`다.

## 10. AI agent 전환 절차

### 단계 0: 범위 라우팅

선택 몬스터와 skill kind를 먼저 확정한다. Skill Builder 구현이라면 읽기 전에 다음 CSV를
명시적으로 라우팅한다.

- 해당 `base` CSV
- 해당 `skill_choices` CSV
- 해당 `skill_graph_nodes` CSV가 존재할 때 그 파일
- 해당 legacy `effects` CSV
- 해당 `triggers` CSV
- 공용 node definition 2개
- 선택 몬스터에 legacy direct node가 있을 때 해당 node/param CSV

다른 몬스터 reference 문서, archive, 오래된 구현을 값 발견 목적으로 읽지 않는다.

### 단계 1: 혼용 가능성 검사

선택 몬스터의 legacy direct node 행을 집계한다.

- 0행이면 graph 전환을 계속한다.
- 1행 이상이면 같은 몬스터의 모든 direct node를 현재 graph schema로 옮길 수 있는지 먼저 확인한다.
- 현재 node definition에 없는 handler가 필요하면 중단한다.

### 단계 2: 기능 원장 작성

각 legacy effect 행마다 non-empty 기능 컬럼을 전부 기록한다.

| legacy effect_id | 필드 | 값 | 현재 소유권 | graph 표현 | 상태 |
|---|---|---|---|---|---|

수치 하나라도 누락된 상태에서 graph 행을 작성하지 않는다.

### 단계 3: 소유권 분류

- 기본 스킬 Executor가 직접 소비하는 값 -> base
- Choice의 단순 수치 변경 -> `Choice + Plan`
- 독립 적용/피해/방어막/오라 -> `Effect`
- 발생 시점과 이벤트 조건 -> Trigger

### 단계 4: graph owner와 index 결정

- Choice가 선택될 때만 존재하는 효과 -> `Choice`
- 스킬에 기본 포함되는 효과 -> `Skill`
- Trigger 자체가 결과 효과를 소유 -> `Trigger`

같은 owner에 Effect가 여러 개면 `graph_index`를 0부터 사용한다.
생성 Effect ID와 기존 Trigger 참조를 미리 계산한다.

### 단계 5: node와 arg 작성

1. Effect graph면 주 연산을 정확히 하나 선택한다.
2. `EffectTarget`, condition, lifetime, visual을 필요한 만큼 추가한다.
3. 수치 modifier는 현재 owner-kind 소비 코드가 지원하는 것만 추가한다.
4. 각 `arg_N`은 definition의 `param_order`와 대조한다.
5. node order 중복과 빈 필수 arg를 검사한다.

### 단계 6: Trigger 참조 전환

- graph-reference 컬럼이 있으면 owner tuple을 작성한다.
- 컬럼이 없으면 생성 Effect ID를 `triggered_effect_id`에 작성한다.
- `triggered_skill_id`, `triggered_effect_id`, graph tuple이 서로 다른 실행 경로를 중복 생성하지 않는지 확인한다.

### 단계 7: legacy 행 제거

모든 기능 컬럼의 대체 근거가 확인된 행만 같은 변경에서 제거한다.
같은 생성 Effect ID를 legacy effect와 graph 양쪽에 남기지 않는다.

kind별 effect CSV는 다른 몬스터 행이 남아 있으면 파일 자체를 삭제하지 않는다.

## 11. 금지 사항과 중단 조건

다음 경우 즉시 중단하고 사용자에게 필요한 범위를 보고한다.

- 선택 kind에 `skill_graph_nodes_{kind}.csv`가 없다.
- 새 CSV 파일이나 새 컬럼이 필요하다.
- 새 `node_type_id` 또는 새 param 정의가 필요하다.
- 기존 handler의 새 owner-kind composer/mapper 연결이 필요하다.
- 선택 몬스터의 legacy direct node를 함께 전환할 수 없다.
- legacy effect의 non-empty 기능 필드를 현재 graph가 표현하지 못한다.
- 기존 effect ID를 보존해야 하지만 생성 ID 규칙과 충돌한다.
- trigger 참조를 안전하게 다시 연결할 수 없다.
- 새 shared runtime/common-logic 확장이 필요하다.

금지 사항:

- Ariel의 arg 값을 다른 node type에 위치만 맞춰 복사하지 않는다.
- `arg_9~arg_12`에 값을 넣지 않는다.
- Effect graph에 주 연산을 0개 또는 2개 이상 넣지 않는다.
- Plan에 Effect 전용 node를 넣지 않는다.
- 같은 몬스터의 graph node와 legacy direct node를 혼용하지 않는다.
- 넓은 legacy effect 행을 불완전한 graph로 바꾼 뒤 원본을 삭제하지 않는다.
- 스키마 정의만 보고 실제 runtime 소비를 추정하지 않는다.

## 12. 수용 기준

### 12.1 CSV 구조

- graph 행은 21개 컬럼이다.
- graph key와 `node_order` 조합이 중복되지 않는다.
- 모든 `node_type_id`가 현재 definitions에 존재한다.
- 모든 필수 arg가 채워져 있다.
- 정의되지 않은 arg가 비어 있다.
- 선택 몬스터에 graph와 legacy direct node가 동시에 남지 않는다.

### 12.2 Effect 권한

- 각 Effect graph에 주 연산이 정확히 1개다.
- 생성 Effect ID가 계산대로 나온다.
- legacy effect ID와 충돌하지 않는다.
- Choice Effect gate가 owner choice에서 자동 생성되는 구조를 보존한다.
- trigger가 graph tuple 또는 생성 Effect ID 중 하나로 올바르게 연결된다.
- legacy 행의 모든 non-empty 기능 필드에 대체 근거가 있다.

### 12.3 검증 경계

Code Builder 또는 Skill Builder 구현 시 최소 검증은 다음 순서다.

1. UTF-8 CSV header/row 폭 검사
2. graph key / node order / arg-definition 정합성 검사
3. 선택 몬스터의 graph/direct-node 혼용 검사
4. 생성 Effect ID / legacy overlap / trigger 참조 검사
5. Runtime 및 Editor `dotnet build`
6. 필요 시 Unity-MCP로 CSV sync, source validation, console 확인
7. 사용자 Play Mode에서 실제 효과 parity 확인

Unity Play Mode gameplay 검증은 사용자 소유다. MSW-MCP는 사용하지 않는다.

## 13. 관련 보드 업데이트

실제 전환 구현 시 다음을 같은 작업에서 갱신한다.

- 선택 몬스터의 `boards/MON/{MONSTER}_MONSTER.md`
- CSV 권한 또는 schema 사실이 바뀌면 `boards/DATA/DATA_BLACKBOARD.md`
- status runtime 확장이 실제로 발생한 경우에만
  `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`

이 가이드 자체의 현재 근거 상태는 `boards/MON/ARIEL_MONSTER.md`와
`boards/DATA/DATA_BLACKBOARD.md`에 기록한다.
