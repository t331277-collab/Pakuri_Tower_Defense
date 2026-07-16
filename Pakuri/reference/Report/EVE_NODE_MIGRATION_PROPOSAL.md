# Eve A-J Skill Graph Migration Proposal

## 1. 목표

Eve A-J 기본 스킬, 액티브 특성 25개, 액티브 마스터 10개, 패시브 특성 15개를
현재 Ariel `skill_graph_nodes` 구조로 이전하는 구현 제안이다.

이 제안은 다음 권한을 사용한다.

- Eve A-D/F-J: 현재 Eve A-J 레퍼런스의 기능 의도와 현재 runtime CSV 행
- Eve-E: 기존 runtime Eve-E 행을 폐기하고
  `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md`의 새 비탄약 장판 설계를 권한으로 사용
- graph 작성 규칙:
  `boards/MON/ARIEL_NODE_DECOMPOSITION_GUIDE.md`
- 실제 지원 판단:
  현재 node definition, graph materialize, Plan mapper, Effect composer, Executor 코드

이 문서는 Designer 제안서다. 이 작업에서는 CSV, C# 코드, 프리팹, 씬을 변경하지 않는다.

## 2. 범위

### 포함

- 기본 스킬 A-E
- 패시브 F-J
- 액티브 특성 5개씩, 총 25개
- 액티브 마스터 2개씩, 총 10개
- 패시브 특성 3개씩, 총 15개
- legacy effect 34행과 trigger 3행의 graph/trigger 재배치
- Eve-E의 새 base/특성/마스터 설계
- 현재 passive G-J 표시명 교정

### 제외

- 각 레퍼런스의 액티브 각성 1-5단계
- 최종 CSV 행 작성
- 신규 node/runtime 구현
- 프리팹, 시각 효과, 씬 연결
- Unity Play Mode 검증

각성은 현재 Eve runtime Choice 50행에 별도 행이 없으므로 이번 이전 범위에 섞지 않는다.

## 3. 검사 근거와 현재 상태

### 3.1 현재 Eve 데이터 집계

| 데이터 | Eve 행 수 |
|---|---:|
| base | 10 |
| Choice | 50 |
| skill graph node | 0 |
| legacy effect | 34 |
| trigger | 3 |
| legacy direct node | 0 |

legacy effect 분포:

- Eve-B 2행
- Eve-C 1행
- Eve-F 5행
- Eve-G 4행
- Eve-H 4행
- Eve-I 4행
- Eve-J 14행

trigger 분포:

- Eve-G 2행
- Eve-H 1행

Eve에는 legacy direct node가 없으므로 graph 행을 추가할 때
`skill_graph_nodes`/legacy direct-node 혼용 오류는 발생하지 않는다.

### 3.2 새 graph 파일이 필요한 kind

현재 graph 파일 존재 여부:

| kind | 현재 graph 파일 | Eve 사용 |
|---|---|---|
| projectile | 있음 | Eve-A |
| line_attack | 없음 | Eve-B |
| area_attack | 없음 | Eve-C, Eve-E |
| single_attack | 있음 | Eve-D |
| passive | 있음 | Eve-F-J |

따라서 구현 전에 다음 파일 추가 승인이 필요하다.

- `choices/line_attack/skill_graph_nodes_line_attack.csv`
- `choices/area_attack/skill_graph_nodes_area_attack.csv`

두 파일은 기존 graph 파일과 동일한 21컬럼 스키마를 사용한다.

### 3.3 passive 이름 불일치

현재 base/passive CSV와 레퍼런스가 다음처럼 어긋나 있다.

| skill_id | 현재 CSV 표시명 | 이번 이전 권한 |
|---|---|---|
| eve-f | 전압 보정 | 전압 보정 |
| eve-g | 약점 분석 | 입자 분리 |
| eve-h | 입자 분리 | 냉각 알고리즘 |
| eve-i | 냉각 알고리즘 | 과전류 회로 |
| eve-j | 과전류 회로 | 약점 분석 |

Eve graph 이전 시 G-J 표시명과 설명을 레퍼런스 슬롯에 맞춘다.

## 4. 지원 상태 분류

이 문서는 각 기능을 다음 네 등급으로 표시한다.

| 등급 | 의미 |
|---|---|
| `재사용` | 현재 node definition과 해당 owner-kind 소비 코드가 모두 존재 |
| `graph 노출` | 공용 wide CSV/runtime 기능은 존재하지만 graph node definition 또는 mapper 연결이 없음 |
| `owner 확장` | node type은 존재하지만 Effect/Trigger 등 필요한 owner 소비 분기가 없음 |
| `신규 의미` | 현재 공용 runtime에도 필요한 동작 의미가 없음 |

`graph 노출`은 새 게임 규칙이 아니다. 기존 wide Choice/Effect/Trigger 기능을
positional graph node로 옮기는 호환 작업이다.

## 5. 현재 그대로 재사용할 node

### Plan

- `DamageMultiplier`
- `CooldownMultiplier`
- `MagazineBonus`
- `ReloadTimeMultiplier`
- `PierceBonus`
- `RadiusMultiplier`
- `ShieldAmountMultiplier`
- `StatusDurationBonus`
- `StatusDamageTakenBonus`
- `StatusDamageBonusRate`
- `StatusFlatElementResistReduction`

### Effect operation/composition

- `ApplyStatus`
- `ApplyShield`
- `StatusModifier`
- `EffectDamage`
- `EffectTarget`
- `EffectLifetime`
- `EffectVisual`
- `ConditionStatus`
- `ConditionSkillAttribute`
- `StatusActionSpeedBonus`
- `StatusCriticalChanceBonus`
- `StatusDamageTakenBonus`
- `StatusDamageBonusRate`
- `StatusFlatElementResistReduction`

Effect graph는 항상 operation node를 정확히 1개 사용한다.

## 6. graph 노출 또는 owner 확장이 필요한 공용 기능

### 6.1 wide runtime을 graph에 노출

| 제안 node_type_id | param 제안 | 기존 근거 | 필요한 이유 |
|---|---|---|---|
| `AdditionalProjectileBonus` | 1 `bonus:int` | Choice record/snapshot/runtime이 이미 소비 | Eve-A 특성 3/4, 마스터 2 |
| `ShotIntervalMultiplier` | 1 `multiplier:float` | Choice record와 projectile/zone runtime이 이미 소비 | Eve-A/B/C/E 주기 변경 |
| `DurationMultiplier` | 1 `multiplier:float` | Choice record/snapshot이 이미 소비 | 비율 기반 지속시간 조합 parity |
| `StatusStackAmountBonus` | 1 `status_id`, 2 `bonus:int` | `status_stacks_bonus` wide 기능 존재 | C/E 상태 부여량 증가 |
| `StatusStackAmountSet` | 1 `status_id`, 2 `value:int` | `status_stacks_set` wide 기능 존재 | A 마스터 2의 감전 2스택 고정 |
| `ConditionalDamageMultiplier` | 1 `status_id`, 2 `min_stacks`, 3 `multiplier` | wide Choice와 snapshot target 조건이 이미 존재 | D/E/G/J 조건부 피해 |
| `StatusMaxStacksBonus` | 1 `status_id`, 2 `bonus:int` | wide Choice 기능 존재 | E 마스터 2 |
| `TriggerProcChanceBonus` | 1 `trigger_id`, 2 `bonus:float` | Trigger proc chance runtime 존재 | G 특성 1을 중복 trigger 행 없이 node화 |

### 6.2 기존 base/schema/runtime handler 승격

| handler | 변경 제안 | 사용처 |
|---|---|---|
| `BranchDamage` | definition과 Plan mapper에 `chance_bonus`, `count`, `damage_multiplier`, `search_radius` 추가 | Eve-A 특성 5, 마스터 1 |
| `StatusFilteredDeployment` | wide base의 `deployment_required_target_status_*`와 현재 SingleAttack 전체-roster 필터/대상별 배치 경로를 node definition과 Plan materialize에 연결 | Eve-D 기본 전체 필드 스캔 |
| `RepeatPerTarget` | 현재 schema/mapper를 node definition에 승격 | Eve-D 마스터 1 |
| `TargetStatusStackDamage` | 현재 schema와 SingleAttack stack damage 경로를 definition/Plan materialize에 연결 | Eve-D 기본 스택 피해 |

`BranchDamage`는 원 투사체 적중 위치에서 가까운 적에게 즉시 피해를 적용한다.
자식 투사체를 생성하지 않고, 갈래 피해는 다시 갈래를 발동하지 않는다.

`StatusFilteredDeployment`의 제안 param은 다음 두 개뿐이다.

```text
1 status_id:status_id
2 min_stacks:int
```

이 node는 스킬 발동 시 적 전체 roster를 한 번 가져오고, 지정 상태와 최소 스택을 만족하는
모든 대상을 필터링한 뒤 각 대상 위치를 독립 deployment 중심으로 만든다. 일치 대상이 없으면
deployment도 생성하지 않는다. 대상 탐색에는 거리나 반경을 사용하지 않는다.

폭발 반경은 이 node가 아니라 single-attack base의 `radius`와 기존 `RadiusMultiplier`가 소유한다.
따라서 가까운 감전 대상 둘 이상이 만든 폭발이 겹치면, 겹친 적은 각 deployment의 독립 피해
판정을 모두 받을 수 있다. 현재 executor가 이미 같은 순서로 동작하므로 새 gameplay runtime
의미가 아니라 node definition/Plan 연결 작업이다.

### 6.3 기존 runtime 의미를 Effect graph에 노출

| 제안 | 기존 근거 | 사용처 |
|---|---|---|
| `ConditionAnyStatus` | legacy condition parser가 `chill;freeze` OR 표현을 소비 | Eve-H |
| `StatusConditionalStatusChanceBonus` | legacy effect/runtime 필드가 이미 존재 | Eve-H |
| `StatusElementDamageTakenBonus` Effect composer 연결 | node definition과 Plan mapper는 있으나 Effect composer 분기가 없음 | Eve-I |

`ConditionStatus`를 chill과 freeze 두 graph로 복제하면 두 상태를 동시에 가진 대상에게
보너스가 중첩될 수 있다. 따라서 OR 조건을 하나의 effect에서 표현해야 한다.

## 7. 실제 신규 공용 의미가 필요한 node

### 7.1 `TargetStatusStackDamageRateBonus`

제안 param:

```text
1 status_id:status_id
2 bonus_rate_per_stack:float
```

사용처: Eve-D 특성 2와 마스터 2.

레퍼런스는 감전 스택당 피해율을 `+15%p`, `+25%p`로 가산한다.
기존 multiplier만 연속 적용하면 두 선택 조합에서 35% + 15% + 25%가 아닌 곱연산이 되므로,
percentage-point 가산 의미가 별도로 필요하다.

### 7.2 `RecastZone`

Effect operation으로 제안한다.

```text
1 source_skill_id:skill_id
2 delay_seconds:float
3 duration_seconds:float
4 radius_multiplier:float
5 inherit_snapshot:bool
6 max_generation:int
```

사용처: Eve-E 마스터 1 `플라즈마 붕괴`.

필수 의미:

- 원본 zone 종료 위치 사용
- 0.5초 지연
- 원본 최종 반경의 60%
- 3초 지속
- 원본의 최종 피해/주기/치명타/취약 snapshot 상속
- cooldown과 탄창을 다시 소비하지 않음
- `max_generation=1`로 재시전 zone의 재귀 발동 차단

현재 `EffectDamage`는 1회/지속 피해 effect를 만들 수 있지만 같은 ZoneSkill을 snapshot과 함께
다시 생성하지 못한다. 따라서 단순 delayed damage가 아니라 새 operation 의미가 필요하다.

## 8. Eve-A 아크 볼트

### 기본

base/projectile에 유지:

- 피해 24, 주문력 0.95
- 탄창 6, 재장전 4초, 발사 간격 0.35초
- 투사체 속도 15, 관통 0
- 감전 15%, 1스택

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.2)` + `MagazineBonus(4)` | 재사용 |
| 특성 2 | `ReloadTimeMultiplier(0.76923)` + `PierceBonus(1)` | 재사용 |
| 특성 3 | `AdditionalProjectileBonus(1)` + `ReloadTimeMultiplier(1.2)` | graph 노출 + 재사용 |
| 특성 4 | `AdditionalProjectileBonus(2)` + `ShotIntervalMultiplier(1.25)` | graph 노출 |
| 특성 5 | `DamageMultiplier(1.25)` + `BranchDamage(chance_bonus=0.35,count=2,damage_multiplier=0.7)` | 공용 즉시 갈래 피해 |
| 갈래 회로 | `DamageMultiplier(1.35)` + `MagazineBonus(2)` + `BranchDamage(chance_bonus=0.6,count=2,damage_multiplier=0.7,search_radius=4.5)` | 공용 즉시 갈래 피해 |
| 과충전 일제사격 | `DamageMultiplier(1.45)` + `PierceBonus(2)` + `AdditionalProjectileBonus(2)` + `ShotIntervalMultiplier(1.2)` + `StatusStackAmountSet(shock,2)` | 혼합 |

현재 CSV의 마스터 1 제목 `분기 회로`는 레퍼런스의 `갈래 회로`로 교정한다.

## 9. Eve-B 프리즘 레이

### 기본

base/line_attack에 유지:

- 피해 12, 주문력 1.6
- 폭 3.2, 지속 1.2초, 주기 0.15초, 쿨다운 6.5초
- 둔화 20%, 2초

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.25)` + `ShotIntervalMultiplier(0.8)` | 재사용 + graph 노출 |
| 특성 2 | `DamageMultiplier(1.3)` + `RadiusMultiplier(1.3)` | 재사용; line radius를 폭으로 사용 |
| 특성 3 | `CooldownMultiplier(0.74074074)` + `DurationMultiplier(1.15)` | 재사용 + graph 노출 |
| 특성 4 | `DamageMultiplier(2.0)` + `DurationMultiplier(0.5)` | 재사용 + graph 노출 |
| 특성 5 | `CooldownMultiplier(0.76923077)` + `ShotIntervalMultiplier(0.83333)` | 재사용 + graph 노출 |
| 초집중 레이 | `DamageMultiplier(1.75)` + `DurationMultiplier(0.55)` + `ShotIntervalMultiplier(0.57143)` | 혼합 |
| 분해 스펙트럼 | Lightning/Ice 저항 감소 Effect graph 2개 | 재사용 |

특성 5는 레퍼런스에 위력 증가가 없으므로 현재 wide CSV의 `damage_multiplier=1.2`를 이전하지 않는다.

분해 스펙트럼은 attribute별 definition이 하나만 존재하므로 다음 두 Effect graph로 나눈다.

```text
Choice/eve-b-master-2/Effect/0: StatusModifier + EffectTarget(EventTarget, OnHit)
  + EffectLifetime(5) + StatusFlatElementResistReduction(10, Lightning)

Choice/eve-b-master-2/Effect/1: StatusModifier + EffectTarget(EventTarget, OnHit)
  + EffectLifetime(5) + StatusFlatElementResistReduction(10, Ice)
```

## 10. Eve-C 프로스트 필드

### 기본

base/area_attack에 유지:

- 피해 8, 주문력 0.8
- 반경 3.2, 지속 4초, 주기 0.5초, 쿨다운 8초
- 추위 1스택

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `RadiusMultiplier(1.25)` + `DurationMultiplier(1.15)` | 재사용 + graph 노출 |
| 특성 2 | `ShotIntervalMultiplier(0.8)` + `StatusStackAmountBonus(chill,1)` | graph 노출 |
| 특성 3 | `DamageMultiplier(1.3)` + `CooldownMultiplier(0.85)` | 재사용 |
| 특성 4 | `DamageMultiplier(1.8)` + `RadiusMultiplier(0.8)` | 재사용 |
| 특성 5 | `DamageMultiplier(1.2)` + `StatusDurationBonus(freeze,1)` | 재사용 |
| 절대영도 구역 | threshold ApplyStatus Effect graph | 재사용 |
| 결정 폭풍 | OnExpire `EffectDamage` graph | 재사용 |

특성 2는 레퍼런스에 위력 증가가 없으므로 현재 wide CSV의 `damage_multiplier=1.25`를 이전하지 않는다.

절대영도 구역:

```text
Choice/eve-c-master-1/Effect/0
ApplyStatus(freeze)
+ ConditionStatus(chill, Enemy, min_stacks=4)
+ EffectTarget(EventTarget, EffectTarget, effect_timing=OnHit)
+ EffectLifetime(1.5)
```

결정 폭풍:

```text
Choice/eve-c-master-2/Effect/0
EffectDamage(Ice, base=24, spell=1.5, radius=3.2)
+ EffectTarget(Enemy, Circle, PrimarySkillCenter, effect_timing=OnExpire, apply_once=true)
+ EffectVisual(Eve_c-master-2.prefab)
```

## 11. Eve-D 스태틱 오버라이드

### 기본 분해

base/single_attack:

- 기본 피해 10, 주문력 0.7, 쿨다운 7초
- 맵 전체의 감전 1스택 이상 적을 한 번 스캔
- 감전 대상 각각의 위치에 반경 1.8 폭발을 별도로 배치
- base의 항상 적용되는 shock payload는 제거

Skill Plan:

```text
StatusFilteredDeployment(shock,1)
TargetStatusStackDamage(shock,base_damage=3.5,spell_power_coefficient=0.245)
```

스택당 추가 피해 `3.5 + 주문력 * 0.245`는 기본식의 35%이므로
최종적으로 `기본식 * (1 + 감전 스택 * 0.35)`가 된다. Consume node를 사용하지 않아 감전을 유지한다.
현재 `SkillTargetingUtility.ResolveTargetList`는 적 전체 roster를 반환하고,
`SingleAttackSkillExecutor.ResolveDeploymentCenters`는 그 목록을 감전 상태로 필터링한 뒤
일치하는 모든 대상 위치를 deployment 중심으로 만든다. 별도 탐색 반경 node는 필요 없다.
각 deployment는 독립 실행되므로 폭발 반경이 겹친 적은 각 폭발의 피해를 모두 받을 수 있다.

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `RadiusMultiplier(1.15)` | 재사용 |
| 특성 2 | `TargetStatusStackDamageRateBonus(shock,0.15)` | 신규 의미 |
| 특성 3 | `DamageMultiplier(1.15)` + `CooldownMultiplier(0.8)` | 재사용 |
| 특성 4 | Choice Effect `ApplyStatus(shock)` + `EffectTarget(EventTarget,OnHit)` | 재사용 |
| 특성 5 | `ConditionalDamageMultiplier(shock,3,1.5)` | graph 노출 |
| 연쇄 과부하 | `RepeatPerTarget(1,0,0.5)` | schema 승격 |
| 전자기 붕괴 | `RadiusMultiplier(1.4)` + `TargetStatusStackDamageRateBonus(shock,0.25)` + `CooldownMultiplier(1.2)` | 혼합 |

특성 1의 기존 탐색 범위 +25%는 전체 필드 스캔 설계와 함께 제거한다.
폭발 반경 +15%만 기존 `RadiusMultiplier`가 소유한다.

## 12. Eve-E 플라즈마 필드

기존 Eve-E base와 Choice 행은 호환 이전 대상이 아니라 폐기 대상이다.
새 레퍼런스에서 다시 작성한다.

### 새 base

base/area_attack:

- AreaAttack, 비탄약
- 번개 피해 14, 주문력 0.9
- 반경 3.2
- 지속 5초
- 주기 0.8초
- 쿨다운 10초
- 범위 안 모든 적
- 취약 1스택, 최대 10
- magazine/reload/hit_target_count=1 제거

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `RadiusMultiplier(1.25)` + `DurationMultiplier(1.2)` | 재사용 + graph 노출 |
| 특성 2 | `ShotIntervalMultiplier(0.8)` + `StatusStackAmountBonus(vulnerable,1)` | graph 노출 |
| 특성 3 | `DamageMultiplier(1.3)` + `CooldownMultiplier(0.85)` | 재사용 |
| 특성 4 | `DamageMultiplier(1.8)` + `RadiusMultiplier(0.8)` | 재사용 |
| 특성 5 | `ConditionalDamageMultiplier(vulnerable,5,1.4)` | graph 노출 |
| 플라즈마 붕괴 | OnExpire `RecastZone(eve-e,0.5,3,0.6,true,1)` | 신규 의미 |
| 약점 고정 | `StatusMaxStacksBonus(vulnerable,5)` + `StatusCriticalDamageTakenBonus(0.01)` | graph 노출 + 재사용 |

플라즈마 붕괴는 Choice Effect graph로 작성한다.

```text
Choice/eve-e-master-1/Effect/0
RecastZone(eve-e, delay=0.5, duration=3, radius_multiplier=0.6,
           inherit_snapshot=true, max_generation=1)
+ EffectTarget(effect_timing=OnExpire, center_mode=PrimarySkillCenter)
```

재시전 zone은 원본의 최종 피해/주기/치명타/취약 부여량을 상속하고,
cooldown을 소비하지 않으며 자신이 종료될 때 재시전하지 않는다.

`StatusCriticalDamageTakenBonus(0.01)`는 별도 stack node가 필요 없다.
현재 `StatusEffectRuntime.SumStacked`가 modifier 값을 runtime status stack 수만큼 곱하므로,
취약 1스택당 받는 치명타 피해 +1%가 된다.

## 13. Eve-F 전압 보정

### 기본 Effect graph

```text
Skill/eve-f/Effect/0
ApplyShield(base=0, spell=1.2)
+ EffectTarget(AllAllies, apply_once=true)
+ ConditionSkillAttribute(Lightning)
+ EffectLifetime(12)

Skill/eve-f/Effect/1
StatusModifier
+ EffectTarget(Enemy)
+ ConditionStatus(shock, Enemy)
+ EffectLifetime(0.5)
+ StatusDamageTakenBonus(0.10)
```

### 특성

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `ShieldAmountMultiplier(1.4)` | 재사용 |
| 특성 2 | Choice Effect: `StatusModifier + EffectTarget(Enemy) + ConditionStatus(shock) + EffectLifetime(0.5) + StatusDamageTakenBonus(0.06)` | 재사용 |
| 특성 3 | Choice Effect: `StatusModifier + EffectTarget(AllAllies) + ConditionStatus(shield) + EffectLifetime(0.5) + StatusActionSpeedBonus(0.12)` | 재사용 |

기존 trait1 별도 shield effect 복제품은 제거하고 기본 shield 하나에 multiplier를 합성한다.

## 14. Eve-G 입자 분리

### 기본

Trigger CSV 한 행:

```text
OnOutgoingDamage, attribute=Lightning;Ice, proc=0.04,
internal_cooldown=1.5, triggered_skill=eve-b, target=EventTarget
```

Effect graph:

```text
Skill/eve-g/Effect/0: StatusModifier + EffectTarget(AllAllies)
  + EffectLifetime(0.5) + StatusDamageBonusRate(0.08,Lightning)

Skill/eve-g/Effect/1: StatusModifier + EffectTarget(AllAllies)
  + EffectLifetime(0.5) + StatusDamageBonusRate(0.08,Ice)
```

### 특성

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `TriggerProcChanceBonus(eve-g-auto-prism-ray,0.03)` | graph 노출/trigger modifier 연결 |
| 특성 2 | Lightning/Ice `StatusModifier + StatusDamageBonusRate(0.05,attribute)` Choice Effect graph 2개 | 재사용 |
| 특성 3 | `ConditionalDamageMultiplier(shield,1,4.0)`, target skill Eve-B | graph 노출 |

현재 base/trait1 trigger 2행은 base trigger 1행과 trait1 node로 통합한다.

## 15. Eve-H 냉각 알고리즘

### 기본 Effect graph

```text
Skill/eve-h/Effect/0
StatusModifier
+ EffectTarget(Enemy)
+ ConditionAnyStatus(chill;freeze, Enemy)
+ EffectLifetime(0.5)
+ StatusDamageTakenBonus(0.14)

Skill/eve-h/Effect/1
StatusModifier
+ EffectTarget(AllAllies)
+ EffectLifetime(0.5)
+ StatusConditionalStatusChanceBonus(chill;freeze,0.10)
```

### 특성

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `ConditionAnyStatus(chill;freeze)` + `StatusDamageTakenBonus(0.06)` Choice Effect graph | graph 노출 + 재사용 |
| 특성 2 | `StatusDurationBonus(freeze,1.0)` | 재사용 |
| 특성 3 | OnStatusExpire trigger + `EffectDamage(Ice,16,spell=1.0)` EventTarget graph | 재사용 |

특성 3 trigger는 현재 한 행을 유지하되 `triggered_graph_*` tuple로 Choice/Effect graph를 참조한다.

## 16. Eve-I 과전류 회로

### 기본 Effect graph

```text
Skill/eve-i/Effect/0
StatusModifier
+ EffectTarget(Enemy)
+ ConditionStatus(shock,Enemy)
+ EffectLifetime(0.5)
+ StatusElementDamageTakenBonus(0.18)

Skill/eve-i/Effect/1
StatusModifier
+ EffectTarget(Enemy)
+ ConditionStatus(shock,Enemy,min_stacks=5)
+ EffectLifetime(0.5)
+ StatusFlatElementResistReduction(12,Lightning)
```

`StatusElementDamageTakenBonus`는 node type은 존재하지만 Effect composer에서 소비하지 않으므로
owner 확장이 선행되어야 한다.

### 특성

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `ConditionStatus(shock)` + `StatusElementDamageTakenBonus(0.08)` Choice Effect graph | owner 확장 후 재사용 |
| 특성 2 | `ConditionStatus(shock,min_stacks=5)` + `StatusFlatElementResistReduction(6,Lightning)` Choice Effect graph | 재사용 |
| 특성 3 | `RadiusMultiplier(1.25)`, target skill Eve-D | 재사용 |

## 17. Eve-J 약점 분석

### 기본 Effect graph

```text
Skill/eve-j/Effect/0
StatusModifier + EffectTarget(Enemy) + ConditionStatus(vulnerable,Enemy)
+ EffectLifetime(0.5) + StatusDamageTakenBonus(0.12)
```

모든 저항 -8은 attribute 하나당 Effect graph 하나가 필요하다.

```text
Skill/eve-j/Effect/1..6
StatusModifier + EffectTarget(Enemy) + ConditionStatus(vulnerable,Enemy)
+ EffectLifetime(0.5)
+ StatusFlatElementResistReduction(8, Physical|Fire|Lightning|Ice|Darkness|Holy)
```

현재 composer는 하나의 definition에서 attribute 하나와 reduction scalar 하나만 보존하므로
여섯 attribute를 한 graph로 합치지 않는다.

### 특성

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `ConditionStatus(vulnerable)` + `StatusDamageTakenBonus(0.06)` Choice Effect graph | 재사용 |
| 특성 2 | attribute별 `StatusFlatElementResistReduction(4,attribute)` Choice Effect graph 6개 | 재사용 |
| 특성 3 | `ConditionalDamageMultiplier(vulnerable,5,1.75)`, target skill Eve-E | graph 노출 |

## 18. legacy 행 제거 계획

| legacy 데이터 | 처리 |
|---|---|
| Eve-B effect 2행 | 분해 스펙트럼 graph 2개로 교체 후 제거 |
| Eve-C effect 1행 | 결정 폭풍 graph로 교체 후 제거 |
| Eve-F effect 5행 | 기본 graph 2개 + Choice node/effect로 교체 후 제거 |
| Eve-G effect 4행 | 기본 graph 2개 + trait2 node로 교체 후 제거 |
| Eve-H effect 4행 | 기본 graph 2개 + trait node/trigger graph로 교체 후 제거 |
| Eve-I effect 4행 | 기본 graph 2개 + trait node로 교체 후 제거 |
| Eve-J effect 14행 | 기본 7 graph + trait node/6 graph로 교체 후 제거 |
| Eve-G trigger 2행 | base trigger 1행 + proc bonus node로 통합 |
| Eve-H trigger 1행 | 유지하되 graph reference로 전환 |

같은 생성 Effect ID와 legacy effect 행을 동시에 남기지 않는다.
다른 몬스터 행이 남아 있으므로 kind별 effect CSV 파일 자체는 삭제하지 않는다.

## 19. 구현 순서

### Phase 0: 승인과 공용 graph 기반

1. `line_attack`/`area_attack` graph 파일 추가 승인
2. graph 노출 node 목록 승인
3. owner 확장 3종 승인
4. 신규 의미 2종 승인

### Phase 1: 공용 node definition/runtime

1. wide runtime 기반 graph 노출 node 구현
2. schema-only handler 승격
3. Effect composer owner 확장
4. D status-filtered deployment graph 노출과 stack-rate 기능 구현
5. E `RecastZone` 구현과 stack crit modifier graph 노출

### Phase 2: A-E active 이전

1. A projectile
2. B line
3. C area
4. D single
5. E 새 area 설계

E는 기존 행을 변환하지 않고 새 레퍼런스로 base/choice를 다시 작성한다.

### Phase 3: F-J passive 이전

1. passive 표시명 G-J 교정
2. F, G, H, I, J 순서로 graph 이전
3. 대응 legacy effect 제거
4. G/H trigger 정규화

### Phase 4: 검증

1. Eve graph/direct-node 혼용 0
2. Effect graph operation 정확히 1개
3. 생성 Effect ID/trigger reference 정합성
4. Eve legacy effect 0행
5. Eve graph node/param definition 정합성
6. Runtime/Editor build
7. Unity-MCP CSV sync/source validation/console 확인
8. 사용자 Play Mode A-J 조합 검증

## 20. 위험과 중단 조건

- 새 graph 파일 2개 승인 전에는 B/C/E graph 행을 작성하지 않는다.
- 새 node definition/param 또는 shared runtime 연결 승인 전에는 구현을 시작하지 않는다.
- Eve-D에 거리 제한이나 탐색 반경을 다시 추가하지 않는다. `radius`는 대상별 폭발 반경만 뜻한다.
- Eve-D의 +15%p/+25%p stack rate를 multiplier 곱으로 바꾸지 않는다.
- Eve-E 기존 magazine/reload/branch 값을 새 설계로 가져오지 않는다.
- `RecastZone` generation guard 없이 Eve-E를 구현하지 않는다.
- chill/freeze 조건을 중첩 가능한 두 effect로 잘못 복제하지 않는다.
- J의 모든 저항 감소를 한 attribute scalar로 축약하지 않는다.
- reference에 없는 B 특성 5 damage +20%, C 특성 2 damage +25%를 이전하지 않는다.
- 구현 중 새 CSV column, node type, common runtime 확장이 추가로 필요하면 중단하고 사용자에게 보고한다.

## 21. 수용 기준

### 기능

- A-E 기본 기능과 특성/마스터가 레퍼런스와 일치한다.
- F-J 기본 효과와 15개 특성이 레퍼런스 슬롯과 일치한다.
- Eve-E는 비탄약, 반경 3.2, 5초/0.8초/10초 zone이다.
- 플라즈마 붕괴는 종료 위치에 0.5초 후 60% 반경/3초로 1회만 재시전한다.
- Eve-D는 맵 전체를 한 번 스캔해 모든 감전 대상 위치에 반경 1.8 폭발을 만들고, 스택당 35%, 비소모, 중첩 피해를 보존한다.

### 데이터

- Eve base 10행과 Choice 50행의 ID는 유지한다.
- Eve graph node는 현재 21컬럼 schema를 따른다.
- Eve legacy effect는 모두 graph로 대체된 뒤 0행이다.
- Eve-G 중복 proc trigger는 1행으로 줄고 trait1 node가 +3%를 소유한다.
- Eve-H trigger는 graph reference를 사용한다.
- passive display name은 F 전압 보정, G 입자 분리, H 냉각 알고리즘, I 과전류 회로, J 약점 분석이다.

### 검증 경계

- Code Builder/Skill Builder: CSV shape, reference, build, Unity-MCP sync/validation/console
- 사용자: Play Mode의 실제 cadence, target, status stack, 조합 parity
- MSW-MCP는 사용하지 않는다.

## 22. 관련 보드

구현 시 최소 갱신:

- `boards/MON/EVE_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- status runtime의 공용 의미가 실제로 확장될 때만
  `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
