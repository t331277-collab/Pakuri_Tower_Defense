# Rin A-J Skill Graph Migration Proposal

## 1. 목표

Rin A-J 기본 스킬, 액티브 특성 25개, 액티브 마스터 10개, 패시브 특성 15개를
현재 `skill_graph_nodes` positional graph 구조로 이전하는 구현 제안이다.

이 제안은 다음 권한을 사용한다.

- 기능 의도: `Pakuri/reference/2.Monster/rin/skill/*.md`
- 현재 수치와 구현 상태: `Pakuri/Assets/CSVdata/authoring/monster/skills/**`의 Rin base/Choice/Effect/Trigger/direct-node 행
- graph 작성 형식: `boards/MON/EVE_NODE_MIGRATION_PROPOSAL.md`와 현재 21컬럼 `skill_graph_nodes_*` 파일
- 실제 지원 판단: node definition, graph materializer, `InGameSkillDefinitionMapper`, Effect composer, Trigger runtime, 각 Executor 코드

이 문서는 Designer 제안서다. 이 작업에서는 CSV, C# 코드, 프리팹, 씬을 변경하지 않는다.

## 2. 범위

### 포함

- 기본 스킬 A-E
- 패시브 F-J
- 액티브 특성 5개씩, 총 25개
- 액티브 마스터 2개씩, 총 10개
- 패시브 특성 3개씩, 총 15개
- legacy Effect 20행의 Effect graph 이전
- legacy direct node 11개와 param 22행의 positional Plan graph 이전
- Trigger 17행의 유지/graph-reference 경계 정리
- wide Choice 기능을 기존 runtime 의미 그대로 graph node로 노출

### 제외

- 각 레퍼런스의 액티브 각성 1-5단계
- 최종 CSV 행 작성
- 프리팹 시각 효과의 런타임 조합 전환
- Rin-E `CoreHitBox` 프리팹 구조 변경
- 신규 게임플레이 규칙 추가
- Unity Play Mode 검증

각성은 현재 Rin Choice 50행에 별도 행이 없으므로 이번 이전 범위에 포함하지 않는다.

## 3. 검사 근거와 현재 상태

### 3.1 현재 Rin 데이터 집계

| 데이터 | Rin 행 수 |
|---|---:|
| base | 10 |
| Choice | 50 |
| skill graph node | 0 |
| legacy Effect | 20 |
| Trigger | 17 |
| legacy direct node | 11 |
| legacy direct node param | 22 |

legacy Effect 분포:

- Rin-B 5행
- Rin-C 1행
- Rin-E/J 5행
- Rin-F/G/I 9행

Trigger 분포:

- passive trigger 16행
- Rin-D master-1 kill burst trigger 1행

legacy direct node 분포:

- Rin-A master-2: 2개 (`AdditionalDamage`, `EveryNthHitChainDamage`)
- Rin-D base/Choice: 9개

Rin에 `skill_graph_nodes` 행을 하나라도 추가하면
`MaterializeSkillGraphRows(...)`가 같은 monster의 legacy direct node 혼용을 오류로 처리한다.
따라서 Rin positional graph 이전은 기존 direct node 11개와 param 22행을 같은 변경에서 모두 치우는 원자적 작업이어야 한다.

### 3.2 필요한 graph 파일

| kind | 현재 graph 파일 | Rin 사용 |
|---|---|---|
| projectile | 있음 | Rin-A |
| buff | 있음 | Rin-B |
| line_attack | 있음 | Rin-C |
| single_attack | 있음 | Rin-D, Rin-E |
| passive | 있음 | Rin-F-J |

필요한 kind의 21컬럼 graph 파일이 모두 존재한다.
Rin 이전을 위해 새 graph CSV 파일을 만들 필요는 없다.

### 3.3 현재 asset/runtime 의존성

- Rin-A/B/C/D base visual은 `NewRunScene`의 `EffectManager` mapping을 사용한다.
- Rin-E base는 `skills_single_attack.csv`의 `Assets/Prefab/Skill/Rin/Rin_E.prefab`을 사용한다.
- `Rin_E.prefab`에는 `CoreHitBox` 이름의 자식과 `BoxCollider2D`가 있으며,
  `SingleAttackSkillExecutor.ResolveCoreHitboxColliders(...)`가 이 이름을 조회한다.
- Rin-D master-1 kill burst는 `Rin_D_master_1.prefab`의 기존 Trigger/SingleAttack 경로를 사용한다.
- Rin-F follow-up은 `Rin_F.prefab`을 사용하는 Trigger 행을 가진다.

이번 node 이전은 위 asset 계약을 변경하지 않는다.

## 4. 지원 상태 분류

| 등급 | 의미 |
|---|---|
| `재사용` | 현재 node definition과 소비 코드가 모두 존재 |
| `graph 노출` | wide/direct runtime 의미는 이미 존재하지만 positional node definition 또는 mapper 연결이 없음 |
| `owner 확장` | Effect graph가 기존 status/effect 필드를 보존하도록 composer/materializer 연결이 필요 |
| `Trigger 유지` | 이벤트 감지, 횟수, 지연, 내부 쿨다운, 자동 스킬 실행 envelope로 Trigger CSV를 유지 |

Rin 이전에는 `신규 의미` 등급이 없다.
제안하는 모든 node는 현재 wide/direct/Effect/Trigger runtime에서 이미 실행되는 의미를 graph에 노출한다.

## 5. 현재 그대로 재사용할 node

### Plan

- `DamageMultiplier`
- `CooldownMultiplier`
- `MagazineBonus`
- `ReloadTimeMultiplier`
- `PierceBonus`
- `RadiusMultiplier`
- `DurationMultiplier`
- `StatusActionSpeedBonus`
- `StatusCriticalChanceBonus`
- `StatusDamageBonusRate`

### Effect operation/composition

- `ApplyStatus`
- `StatusModifier`
- `EffectTarget`
- `EffectLifetime`
- `EffectVisual`
- `ConditionStatus`
- `StatusActionSpeedBonus`
- `StatusAttackPowerBonus` handler
- `StatusCriticalChanceBonus`
- `StatusDamageBonusRate`

Effect graph는 operation node를 정확히 1개 사용한다.

## 6. graph 노출이 필요한 기존 공용 기능

### 6.1 Choice wide 기능 노출

| 제안 node_type_id | param | 기존 근거 | Rin 사용처 |
|---|---|---|---|
| `CritChanceBonus` | `bonus:float` | Choice/snapshot/critical 계산이 이미 소비 | A 특성 5 |
| `CritDamageBonus` | `bonus:float` | Choice/snapshot/critical 계산이 이미 소비 | A 특성 5, D 특성 4 |
| `BeamWidthBonus` | `bonus:float` | line Choice와 `BeamSkillExecutor` 폭 계산이 이미 소비 | C 특성 2, master 1/2 |
| `KnockbackDistanceMultiplier` | `multiplier:float` | line Choice/snapshot knockback 경로가 이미 소비 | C 특성 3, master 1 |
| `ReloadReducePerHit` | `target_skill_id`, `seconds_per_hit` | `SkillOnHitAdditionalDamageUtility`가 재장전 중인 target skill을 감소 | C 특성 5 |
| `CoreDamageMultiplier` | `hitbox_name`, `multiplier` | SingleAttack이 named core Collider 적중에만 multiplier 적용 | E 특성 4, master 1 |
| `CoreAdditionalDamage` | `hitbox_name`, `chance`, `multiplier`, `attribute` | SingleAttack core 추가 피해 wide 경로가 이미 소비 | E master 1 |
| `HitCountCooldownRefund` | `target_skill_id`, `min_targets`, `ratio` | SingleAttack hit count 후 target skill cooldown 감소 | E 특성 5 |

`BeamWidthBonus`는 현재 wide 값의 의미를 그대로 보존한다.
예를 들어 `0.25`는 폭 +25%, `-0.25`는 폭 -25%, `0.60`은 폭 +60%이다.

### 6.2 legacy direct handler를 positional definition으로 승격

| node_type_id/handler | 현재 상태 | Rin 사용처 |
|---|---|---|
| `AdditionalDamage` | handler schema와 Choice mapper 존재, definition 없음 | A master 2, C master 1, D master 2, E master 2 |
| `EveryNthHitChainDamage` | handler schema와 Choice mapper 존재, definition 없음 | A master 2 |
| `TargetHealthRatioCondition` | Plan mapper와 executor 존재, definition 없음 | D base |
| `ExecuteDamageMultiplier` | Plan mapper와 executor 존재, definition 없음 | D base |
| `TargetPredicateDamageMultiplier` | Plan mapper와 executor 존재, definition 없음 | D 특성 5 |
| `CooldownRefund` | Plan mapper와 kill action 존재, definition 없음 | D base |
| `TargetHealthRatioThresholdBonus` | Plan mapper 존재, definition 없음 | D 특성 2/master 2 |
| `ExecuteCritChanceBonus` | Plan mapper와 crit op 존재, definition 없음 | D master 1 |
| `CooldownReset` | Plan mapper와 kill action 존재, definition 없음 | D master 1 |
| `CooldownRefundBonus` | Plan mapper와 kill action 존재, definition 없음 | D 특성 3 |

Rin-D base의 현재 `TargetPredicateDamageMultiplier(is_boss,1)`는 multiplier 1의 no-op이므로
positional graph에는 옮기지 않는다. 실제 boss 보너스는 D 특성 5 graph만 소유한다.

### 6.3 Effect graph 노출

| 제안 node_type_id | param | 기존 근거 | Rin 사용처 |
|---|---|---|---|
| `StatusMoveSpeedBonus` | `bonus:float` | Effect payload와 status runtime이 이미 소비 | C master 2, E master 2 |
| `StatusCriticalDamageBonus` | `bonus:float` | status modifier의 outgoing crit damage 경로가 이미 소비 | I 특성 2 |
| `StatusElementResistReduction` | `bonus:float`, `attribute:enum` | Effect payload의 비율 저항/방어 감소 경로가 이미 소비 | J base/특성 1 |
| `StatusOutgoingAdditionalDamage` | `multiplier`, `trigger_attribute`, `damage_attribute` | status outgoing additional damage runtime이 이미 소비 | B master 2 |
| `ConditionHealthRatioMax` | `ratio:float` | legacy Effect와 `SkillMultiEffectExecutor` 조건이 이미 소비 | I base/특성 1 |
| `ConditionHitCountMin` | `min_targets:int` | OnHitCount Effect 경로가 이미 소비 | J base/특성 2 |

`StatusAttackPowerBonus`는 handler schema와 Effect composer 분기가 이미 있지만
`skill_node_definitions.csv`에 positional definition이 없으므로 definition/param만 추가한다.

### 6.4 passive-to-active Effect gate owner 확장

Rin-J는 passive Skill/Choice가 만든 Effect를 target skill `rin-e`에 부착한다.
현재 graph materializer는 generated Effect의 `RequiresPassiveSkillId`를 항상 빈 값으로 만든다.

따라서 다음 공용 추론을 추가한다.

- owner가 passive Skill이면 generated Effect의 `RequiresPassiveSkillId=owner_id`
- owner가 passive Choice이면 해당 Choice의 parent passive skill ID를 `RequiresPassiveSkillId`로 설정
- 기존 active Skill/Choice/Trigger owner에는 이 추론을 적용하지 않음

이 확장은 새 상태 규칙이 아니라 legacy Effect의 `requires_passive_skill_id` gate를 graph에서도 보존하는 작업이다.

## 7. 신규 공용 의미

없음.

구현 중 현재 wide/direct/Effect/Trigger runtime으로 설명되지 않는 새 동작이 발견되면
이 제안의 범위를 넘어선 것이므로 작업을 중단하고 사용자 승인을 받는다.

## 8. Rin-A 파쇄권

### 기본

base/projectile에 유지:

- 물리 피해 17, 공격력 1.0
- 탄창 10, 재장전 4초, 발사 간격 0.34초
- 투사체 속도 13, 관통 0
- 치명타 가능

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.25)` | 재사용 |
| 특성 2 | `MagazineBonus(4)` | 재사용 |
| 특성 3 | `ReloadTimeMultiplier(0.8)` | 재사용 |
| 특성 4 | `PierceBonus(1)` + `DamageMultiplier(0.9)` | 재사용 |
| 특성 5 | `CritChanceBonus(0.10)` + `CritDamageBonus(0.25)` | graph 노출 |
| 철권 연사 | `MagazineBonus(6)` + `ShotIntervalMultiplier(0.82)` + `DamageMultiplier(1.12)` | 재사용 |
| 뇌격 건틀릿 | `AdditionalDamage(0.4, Lightning)` + `EveryNthHitChainDamage(3,2,4.5,0.4,Lightning)` | definition 승격 |

현재 A master-2 legacy direct node 2개와 param 9행은 같은 의미의 projectile graph로 옮긴 뒤 제거한다.

## 9. Rin-B 하울링

### 기본

base/buff에 유지:

- 아군 전체 행동속도 +20%
- 지속 5초, 쿨다운 12초
- `same_source_refresh`, 최대 1스택

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DurationMultiplier(1.25)` | 재사용 |
| 특성 2 | `StatusActionSpeedBonus(action-speed-up,0.10)` | 재사용 |
| 특성 3 | `CooldownMultiplier(0.8)` | 재사용 |
| 특성 4 | self Effect: `StatusModifier + EffectTarget(Self) + EffectLifetime(5) + StatusAttackPowerBonus(0.15)` | definition 추가 |
| 특성 5 | ally Effect: `StatusModifier + EffectTarget(AllAllies) + EffectLifetime(5) + StatusCriticalChanceBonus(0.08)` | 재사용 |
| 전장의 포효 | `StatusActionSpeedBonus(0.15)` + `DurationMultiplier(1.2)` + ally Physical `StatusDamageBonusRate(0.18)` Effect | 재사용 |
| 심연 군가 | `StatusActionSpeedBonus(-0.05)` + ally `StatusOutgoingAdditionalDamage(0.25,Physical,Darkness)` Effect | graph 노출 |

Rin-B legacy Effect 5행은 위 Choice Plan/Effect graph가 생성하는 동일 Effect로 교체한 뒤 제거한다.

## 10. Rin-C 충격파

### 기본

base/line_attack에 유지:

- 물리 피해 28, 공격력 1.35
- 폭 1.6, 쿨다운 5.5초
- 넉백 거리 0.6
- 적중 대상마다 1회 피해

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.25)` | 재사용 |
| 특성 2 | `BeamWidthBonus(0.25)` | graph 노출 |
| 특성 3 | `KnockbackDistanceMultiplier(1.4)` | graph 노출 |
| 특성 4 | `CooldownMultiplier(0.8)` | 재사용 |
| 특성 5 | `ReloadReducePerHit(rin-a,0.25)` | graph 노출 |
| 압축 충격파 | `BeamWidthBonus(-0.25)` + `DamageMultiplier(1.8)` + `KnockbackDistanceMultiplier(1.5)` + `AdditionalDamage(0.6,Lightning)` | 혼합 |
| 광역 진탕 | `BeamWidthBonus(0.60)` + `DamageMultiplier(1.25)` + OnHit slow Effect | 혼합 |

광역 진탕 Effect graph:

```text
Choice/rin-c-master-2/Effect/0
ApplyStatus(slow)
+ EffectTarget(Enemy, EffectTarget, OnHit)
+ EffectLifetime(1.5)
+ StatusMoveSpeedBonus(-0.20)
```

legacy line Effect `rin-c-master2-slow`는 graph 생성 Effect와 교체한 뒤 제거한다.

## 11. Rin-D 종결 일격

### 기본 Plan

base/single_attack에 유지:

- 물리 피해 45, 공격력 2.4
- 가장 낮은 체력 대상, 쿨다운 9초
- 치명타 가능

Skill Plan:

```text
TargetHealthRatioCondition(threshold=0.30,reject_if_missing_target=true)
ExecuteDamageMultiplier(multiplier=1.8)
CooldownRefund(ratio=0.35)
```

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.3)` | 재사용 |
| 특성 2 | `TargetHealthRatioThresholdBonus(0.10)` | definition 승격 |
| 특성 3 | `CooldownRefundBonus(0.20)` | definition 승격 |
| 특성 4 | `CritDamageBonus(0.40)` | graph 노출 |
| 특성 5 | `TargetPredicateDamageMultiplier(is_boss,1.25)` | definition 승격 |
| 확정 종결 | `ExecuteCritChanceBonus(0.50)` + `CooldownReset(requires_execute=true)` | definition 승격 |
| 파멸권 | `DamageMultiplier(1.9)` + `TargetHealthRatioThresholdBonus(-0.10)` + `CooldownMultiplier(1.25)` + `AdditionalDamage(0.70,Darkness)` | 혼합 |

확정 종결의 처치 폭발 Trigger는 유지한다.
현재 Trigger/SingleAttack 경로가 `Rin_D_master_1.prefab`과 그 hitbox를 사용하므로,
반경 0의 단순 `EffectDamage` graph로 바꾸면 기존 prefab hitbox 판정과 같다는 근거가 없다.

현재 D legacy direct node 9개/param 13행은 positional graph로 교체한 뒤 제거한다.
base boss multiplier 1.0 no-op node는 제거만 하고 graph에 복제하지 않는다.

## 12. Rin-E 붕괴 타격

### 기본

base/single_attack에 유지:

- 물리 피해 40, 공격력 2.0
- 반경 2.4, 쿨다운 8초
- 범위 내 모든 적 1회 피해
- `Rin_E.prefab` hitbox 사용

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.3)` | 재사용 |
| 특성 2 | `RadiusMultiplier(1.25)` | 재사용 |
| 특성 3 | `CooldownMultiplier(0.8)` | 재사용 |
| 특성 4 | `CoreDamageMultiplier(CoreHitBox,1.5)` | graph 노출 |
| 특성 5 | `HitCountCooldownRefund(rin-b,3,0.20)` | graph 노출 |
| 압살 지점 | `RadiusMultiplier(0.8)` + `DamageMultiplier(2.0)` + `CoreAdditionalDamage(CoreHitBox,1,1.0,Fire)` | 혼합 |
| 균열 확산 | `RadiusMultiplier(1.5)` + `DamageMultiplier(1.35)` + `AdditionalDamage(0.45,Darkness)` + OnHit slow Effect | 혼합 |

균열 확산 slow Effect:

```text
Choice/rin-e-master-2/Effect/0
ApplyStatus(slow)
+ EffectTarget(Enemy, EffectTarget, OnHit)
+ EffectLifetime(2)
+ StatusMoveSpeedBonus(-0.25)
```

`CoreHitBox` 이름과 Collider는 이번 node 이전의 외부 asset 계약이다.
이름을 바꾸거나 prefab을 런타임 hitbox로 전환하는 작업은 별도 migration으로 분리한다.

## 13. Rin-F 양손잡이

### 기본 Effect graph

```text
Skill/rin-f/Effect/0
StatusModifier
+ EffectTarget(AllAllies)
+ EffectLifetime(0.5)
+ StatusDamageBonusRate(0.12,Physical)
```

### 특성과 Trigger

| 선택 | graph/Trigger | 지원 |
|---|---|---|
| 기본 추가타 | `rin-f-followup` Trigger 유지 | Trigger 유지 |
| 특성 1 | Physical `StatusDamageBonusRate(0.06)` Choice Effect | 재사용 |
| 특성 2 | `rin-f-followup-trait2` Trigger 유지 | Trigger 유지 |
| 특성 3 | `rin-f-followup-lightning-trait3` Trigger 유지 | Trigger 유지 |

세 Trigger는 지연 0.3초, event skill C/D/E, owner scope, 원본 피해 기반 추가타를 이미 표현한다.
이를 한 행으로 합치려면 trigger damage modifier라는 별도 runtime 확장이 필요하므로 이번에는 중복을 유지한다.

## 14. Rin-G 전장의 공명

### 기본 Effect graph

```text
Skill/rin-g/Effect/0
StatusModifier
+ EffectTarget(AllAllies)
+ ConditionStatus(action-speed-up,AllAllies)
+ EffectLifetime(0.5)
+ StatusAttackPowerBonus(0.14)
+ StatusActionSpeedBonus(0.08)
```

### 특성

| 선택 | graph/Trigger | 지원 |
|---|---|---|
| 특성 1 | conditional `StatusAttackPowerBonus(0.08)` Choice Effect | definition 추가 |
| 특성 2 | conditional `StatusCriticalChanceBonus(0.06)` Choice Effect | 재사용 |
| 특성 3 | 기존 OnSkillCast `ReloadReduce(rin-a,0.25)` Trigger 유지 | Trigger 유지 |

기본/특성 legacy passive Effect 3행은 graph로 교체한 뒤 제거한다.

## 15. Rin-H 파문 증폭

### Trigger 유지

현재 passive Trigger는 다음 조합을 8행으로 표현한다.

- 기본/특성 1: 10회 또는 8회마다 발동
- 기본/특성 2: 위력 75% 또는 90%
- 특성 3: 물리 자동 충격파에 30% Lightning 추가 피해
- 내부 쿨다운 3초, 모든 아군의 Physical outgoing damage count

현재 graph runtime에는 Trigger의 `trigger_every_count`와 triggered SingleAttack damage multiplier를
Choice Plan으로 합성하는 공용 의미가 없다. 새 runtime 의미를 만들지 않는 원칙에 따라 8행을 유지한다.

| 선택 | 처리 |
|---|---|
| 기본 | 기존 75%/10회 Trigger envelope |
| 특성 1 | 8회 조합 Trigger gate 유지 |
| 특성 2 | 90% 조합 Trigger gate 유지 |
| 특성 3 | Lightning 30% 조합 Trigger gate 유지 |

Rin-H에는 제거할 legacy Effect가 없다.

## 16. Rin-I 마무리 본능

### 기본 Effect graph

```text
Skill/rin-i/Effect/0
StatusModifier
+ EffectTarget(Enemy)
+ ConditionHealthRatioMax(0.35)
+ EffectLifetime(0.5)
+ StatusDamageTakenBonus(0.16)
```

### 특성/Trigger Effect

| 선택 | graph/Trigger | 지원 |
|---|---|---|
| 특성 1 | 동일 조건 `StatusDamageTakenBonus(0.08)` Choice Effect | graph 노출 + 재사용 |
| D 처치 시 행동속도 | Trigger-owned Effect graph: AllAllies, 4초, `StatusActionSpeedBonus(0.10)` | graph reference |
| 특성 2 | Trigger-owned Effect graph: AllAllies, 4초, `StatusCriticalDamageBonus(0.25)` | graph 노출/reference |
| 특성 3 | 현재 OnOutgoingDamage execute 조건 `CooldownRefund(rin-e,0.12)` Trigger 유지 | Trigger 유지 |

kill Trigger 2행은 유지하되 `triggered_effect_id` 대신
`triggered_graph_owner_kind=Trigger`, `triggered_graph_owner_id`, `Effect`, index를 참조한다.

## 17. Rin-J 붕괴 여파

### Rin-E에 부착되는 passive Effect graph

```text
Skill/rin-j/Effect/0 -> target_skill_id=rin-e
StatusModifier
+ EffectTarget(Enemy,OnHit)
+ EffectLifetime(4)
+ StatusElementResistReduction(0.18,Physical)

Skill/rin-j/Effect/1 -> target_skill_id=rin-e
StatusModifier
+ EffectTarget(AllAllies,effect_timing=OnHitCount)
+ ConditionHitCountMin(3)
+ EffectLifetime(3)
+ StatusActionSpeedBonus(0.12)
```

### 특성

| 선택 | graph/Trigger | 지원 |
|---|---|---|
| 특성 1 | Physical `StatusElementResistReduction(0.08)` Choice Effect | graph 노출 |
| 특성 2 | `ConditionHitCountMin(3)` + `StatusAttackPowerBonus(0.15)` Choice Effect | graph 노출 |
| 특성 3 | 현재 status-source-gated OnKill `CooldownRefund(rin-d,0.15)` Trigger 유지 | Trigger 유지 |

Rin-J graph 생성 Effect에는 section 6.4의 passive gate 추론이 반드시 적용되어야 한다.
그렇지 않으면 Rin-J를 배우지 않아도 Rin-E에 효과가 부착될 수 있다.

## 18. legacy 행 제거 계획

| legacy 데이터 | 처리 |
|---|---|
| Rin-A direct node 2개/param 9행 | projectile positional graph로 교체 후 제거 |
| Rin-D direct node 9개/param 13행 | single-attack positional graph로 교체 후 제거 |
| Rin-B Effect 5행 | buff Choice Plan/Effect graph로 교체 후 제거 |
| Rin-C Effect 1행 | C master-2 Effect graph로 교체 후 제거 |
| Rin-E/J Effect 5행 | E master-2 및 J passive/Choice Effect graph로 교체 후 제거 |
| Rin-F/G/I Effect 9행 | passive Skill/Choice/Trigger Effect graph로 교체 후 제거 |
| Trigger 17행 | 이벤트 envelope로 유지; I kill Effect 2행만 graph reference로 전환 |

같은 generated Effect ID와 legacy Effect 행을 동시에 남기지 않는다.
Rin graph row를 추가하는 변경에서는 Rin legacy direct node를 한 개도 남기지 않는다.

Choice 행의 ID, title, description, group, sort order는 유지한다.
graph로 이전된 wide behavior 필드는 기본값/blank로 정리하여 중복 합성을 막는다.

## 19. 구현 순서

### Phase 0: 승인과 graph 노출 범위 확정

1. 신규 gameplay 의미 0개 원칙 승인
2. Choice wide graph 노출 node 승인
3. Effect payload graph 노출 node 승인
4. passive-to-active generated Effect gate 추론 승인
5. Trigger 17행 유지 경계 승인

### Phase 1: 공용 positional node 기반

1. 기존 handler의 node definition/param 추가
2. Crit/beam/knockback/reload/core/hit-count wide mapper 연결
3. Effect composer에 move speed/crit damage/resist reduction/outgoing additional damage/condition 연결
4. passive Skill/Choice generated Effect gate 보존
5. Rin graph/direct-node 혼용 방지를 위한 원자적 데이터 전환 준비

### Phase 2: A-E active 이전

1. A projectile graph와 legacy node 제거
2. B buff graph와 legacy Effect 제거
3. C line graph와 legacy Effect 제거
4. D single graph와 legacy node 제거
5. E single graph와 legacy Effect 제거

### Phase 3: F-J passive 이전

1. F/G Effect graph 이전
2. H Trigger 유지 검증
3. I Effect/Trigger graph reference 이전
4. J passive-to-E Effect graph 이전
5. legacy passive Effect 제거

### Phase 4: 검증

1. Rin graph/direct-node 혼용 0
2. Effect graph operation 정확히 1개
3. generated Effect ID/Trigger graph reference 정합성
4. Rin legacy Effect 0행
5. Trigger 17행의 event/gate/cadence parity
6. wide behavior 필드와 graph 중복 0
7. Runtime/Editor build
8. Unity-MCP CSV sync/source validation/console 확인
9. 사용자 Play Mode A-J 조합 검증

## 20. 위험과 중단 조건

- Rin graph 행을 추가한 상태에서 legacy direct node를 남기지 않는다.
- Rin-E `CoreHitBox` 이름, prefab 자식 구조, Collider를 이번 작업에서 변경하지 않는다.
- D master-1 kill burst를 prefab hitbox parity 근거 없이 `EffectDamage(radius=0)`로 치환하지 않는다.
- H Trigger 8행을 새 trigger modifier runtime 없이 억지로 한 행으로 합치지 않는다.
- F Trigger 3행을 새 trigger damage modifier runtime 없이 합치지 않는다.
- J passive-to-active Effect의 `RequiresPassiveSkillId` gate 없이 이전하지 않는다.
- B master-2의 25% Darkness 추가 피해를 단순 Physical damage bonus로 바꾸지 않는다.
- E 중심부 보너스를 전체 범위 multiplier로 바꾸지 않는다.
- D 처형 threshold bonus와 damage multiplier 적용 순서를 바꾸지 않는다.
- 구현 중 새 gameplay node 의미나 새 CSV 파일/컬럼이 추가로 필요하면 중단하고 사용자에게 보고한다.

## 21. 수용 기준

### 기능

- A-E 기본 수치와 25개 특성/10개 마스터가 현재 reference 및 runtime 동작을 유지한다.
- F-J 기본 효과와 15개 특성이 현재 Trigger/Effect cadence를 유지한다.
- Rin-A master-2는 매 적중 40% Lightning 추가 피해와 3번째 적중 2대상 chain을 유지한다.
- Rin-D는 30% 처형 gate, 1.8배 피해, 35% kill refund를 유지한다.
- Rin-E 중심부 판정은 `CoreHitBox` Collider에만 적용된다.
- Rin-H는 3초 내부 쿨다운과 10/8회, 75/90%, Lightning 30% 조합을 유지한다.

### 데이터

- Rin base 10행과 Choice 50행 ID를 유지한다.
- 새 graph CSV 파일을 추가하지 않는다.
- Rin positional graph는 기존 21컬럼 schema를 사용한다.
- Rin legacy direct node/param은 모두 제거된다.
- Rin legacy Effect는 graph로 대체된 뒤 0행이다.
- Trigger 17행은 이벤트 envelope로 유지하고 Effect action만 graph reference를 사용할 수 있다.
- graph로 이전한 wide behavior 필드는 중복 값이 남지 않는다.

### 검증 경계

- Code Builder/Skill Builder: CSV shape, node definition/handler 정합성, build, Unity-MCP sync/validation/console
- 사용자: Play Mode 실제 target, cadence, hit count, core hitbox, execute, cooldown/reload 조합 parity
- Code Reviewer는 사용자 명시 승인 시 1회 실행
- MSW-MCP는 사용하지 않고 Unity-MCP만 사용

## 22. 관련 보드

구현 시 최소 갱신:

- `boards/MON/RIN_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- status Effect composer 의미를 노출하므로 `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `CoreHitBox` 또는 skill prefab 계약을 실제 변경할 때만 `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`

