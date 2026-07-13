# Sein A-J Skill Graph Migration Proposal

## 1. 목표

Sein A-J 기본 스킬, 액티브 특성 25개, 액티브 마스터 10개,
패시브 특성 15개와 `PassiveBase` 1개를 현재 `skill_graph_nodes` positional graph 구조로 이전하는 구현 제안이다.

이 제안은 다음 근거를 사용한다.

- 기능 의도: `Pakuri/reference/2.Monster/sein/skill/*.md`
- 현재 수치와 구현 상태: `Pakuri/Assets/CSVdata/runtime/monster/skills/**`의 Sein base/Choice/Effect/Trigger 행
- graph 작성 형식: `boards/MON/RIN_NODE_MIGRATION_PROPOSAL.md`와 현재 21컬럼 `skill_graph_nodes_*` 파일
- 실제 지원 판단: node definition, graph materializer, `InGameSkillDefinitionMapper`, Effect composer, Trigger runtime, 각 Executor 코드

이 문서는 Designer 제안서다. 이 작업에서는 CSV, C# 코드, 프리팹, 씬을 변경하지 않는다.

## 2. 범위

### 포함

- 기본 스킬 A-E
- 패시브 F-J
- 액티브 특성 5개씩, 총 25개
- 액티브 마스터 2개씩, 총 10개
- 패시브 특성 3개씩, 총 15개
- Sein-I의 `PassiveBase` 1개
- legacy Effect 19행의 Effect graph 이전
- Trigger 17행의 유지 경계 정리
- wide Choice 기능을 기존 runtime 의미 그대로 graph node로 노출

### 제외

- 각 레퍼런스의 액티브 각성 1-5단계
- 최종 CSV 행 작성
- 프리팹 visual의 런타임 조합 전환
- Sein-E 다중 배치와 prefab hitbox 구조 변경
- 신규 gameplay 규칙 추가
- Unity Play Mode 검증

각성은 현재 Sein Choice 행에 별도 데이터가 없으므로 이번 이전 범위에 포함하지 않는다.

## 3. 검사 근거와 현재 상태

### 3.1 현재 Sein 데이터 집계

| 데이터 | Sein 행 수 |
|---|---:|
| base | 10 |
| Choice | 51 |
| skill graph node | 0 |
| legacy Effect | 19 |
| Trigger | 17 |
| legacy direct node | 0 |
| legacy direct node param | 0 |

Choice 51행은 액티브 35행, 패시브 특성 15행, `sein-i-base-shot-interval` `PassiveBase` 1행이다.

legacy Effect 분포:

- Sein-C 5행
- Sein-D 4행
- Sein-E 6행
- Sein-F 4행

Trigger 분포:

- Sein-A master-2 적중 폭발 1행
- Sein-G 자동 발동/특성 조합 4행
- Sein-J 처치 환급 12행

Sein에는 legacy direct node가 없다.
따라서 `MaterializeSkillGraphRows(...)`의 monster 단위 graph/direct-node 혼용 오류에 걸릴 행은 없다.
단, graph로 이전한 Choice wide 필드와 legacy Effect는 같은 변경에서 제거하여 이중 합성을 막아야 한다.

### 3.2 필요한 graph 파일

| kind | 현재 graph 파일 | Sein 사용 |
|---|---|---|
| projectile | 있음 | Sein-A, B, C |
| area_attack | 있음 | Sein-D |
| single_attack | 있음 | Sein-E |
| passive | 있음 | Sein-F-J |

모든 파일은 동일한 21컬럼 positional schema를 사용한다.
Sein 이전을 위해 새 graph CSV 파일이나 새 CSV 컬럼을 만들 필요는 없다.

### 3.3 현재 asset/runtime 의존성

- Sein-C base는 `skills_projectile.csv`의 `Assets/Prefab/Skill/Sein/Sein_C.prefab`과 scene `EffectManager` projectile visual 경로를 사용한다.
- Sein-C master-1/2 legacy Effect는 각각 `Sein_C_Master_1.prefab`, `Sein_C_Master-2.prefab`을 사용한다.
- Sein-D base와 master-2는 각각 `Sein_D.prefab`, `Sein_D_Master_2.prefab`을 사용한다.
- Sein-E base는 `Sein_E.prefab`의 prefab hitbox와 다중 배치 `SingleAttack` 경로를 사용한다.
- Sein-E master-2는 각 배치 중심에 `Sein_D.prefab` 기반 지속 장판을 만든다.
- Sein-A master-2 Trigger는 `Sein_A_Master-2.prefab`의 `SingleAttack` hitbox를 사용한다.

이번 node 이전은 위 prefab, collider, scene mapping 계약을 변경하지 않는다.

### 3.4 reference와 현재 runtime 수치 차이

- `c-flame-trajectory.md`는 Sein-C master-1 잔류 지대를 2초로 설명한다.
- 현재 `sein-c-master1-zone` Effect 행은 1.5초, 반경 1.2, 주기 0.5초다.
- 이 값은 기존 Sein 보드에서 추론값으로 기록되어 있다.

이 작업은 리팩터링이므로 현재 실행 데이터인 1.5초/1.2/0.5초를 보존한다.
2초로 바꾸는 것은 별도 gameplay/data 변경 승인이 필요하다.

## 4. 지원 상태 분류

| 등급 | 의미 |
|---|---|
| `재사용` | 현재 node definition과 소비 코드가 모두 존재 |
| `graph 노출` | wide runtime 의미는 존재하지만 positional node definition/mapper 연결이 없음 |
| `composer 확장` | legacy Effect가 가진 기존 payload를 generated Effect에서도 보존하도록 authoring composer 연결이 필요 |
| `Trigger 유지` | 이벤트, 확률, 내부 쿨다운, 자동 스킬, 대상별 환급 envelope로 Trigger CSV를 유지 |

Sein 이전에 신규 gameplay 의미는 없다.
추가되는 node/handler는 이미 실행 중인 wide Choice 또는 legacy Effect 의미를 graph에 노출하는 용도다.

## 5. 현재 그대로 재사용할 node

### Plan

- `DamageMultiplier`
- `CooldownMultiplier`
- `MagazineBonus`
- `ReloadTimeMultiplier`
- `PierceBonus`
- `RadiusMultiplier`
- `DurationBonus`
- `DurationMultiplier`
- `AdditionalProjectileBonus`
- `ShotIntervalMultiplier`
- `HitTargetCountBonus`
- `ConditionalDamageMultiplier`
- `CritChanceBonus`

### Effect operation/composition

- `ApplyStatus`
- `StatusModifier`
- `EffectDamage`
- `EffectTarget`
- `EffectLifetime`
- `EffectVisual`
- `ConditionSkillAttribute`
- `StatusCriticalChanceBonus`
- `StatusCriticalDamageBonus`
- `StatusDamageBonusRate`
- `StatusFlatElementResistReduction`
- `StatusElementDamageTakenBonus`

Effect graph는 operation node를 정확히 1개 사용한다.

## 6. graph 노출 및 composer 확장이 필요한 기존 기능

### 6.1 Choice wide 기능 노출

| 제안 node_type_id | param | 기존 근거 | Sein 사용처 |
|---|---|---|---|
| `DamageDelayMultiplier` | `multiplier:float` | Choice parser, `SkillChoiceEffectSpec`, snapshot, projectile impact delay가 이미 소비 | C 특성 4 |
| `ConsecutiveHitDamageBonus` | `bonus_rate:float`, `max_bonus:float` | Choice parser, snapshot, `SkillRuntimeInstance.ResolveConsecutiveHitDamageMultiplier(...)`가 이미 소비 | B 특성 5 |

두 node는 새 전투 규칙을 만들지 않는다.
현재 `damage_delay_multiplier`, `consecutive_hit_bonus_rate`, `consecutive_hit_max` wide 값을 positional Plan으로 옮긴다.

### 6.2 `EffectDamage` param definition 확장

현재 handler schema와 Effect composer는 다음 값을 이미 읽는다.

- `attack_power_coefficient`
- `tick_interval_seconds`

하지만 `skill_node_definition_params.csv`의 `EffectDamage` positional definition에는 두 param이 없다.
기존 arg 순서를 깨지 않도록 다음 순서로 뒤에 추가한다.

| param_order | param_key | 용도 |
|---:|---|---|
| 6 | `attack_power_coefficient` | C/D/E 지속 피해의 공격력 계수 보존 |
| 7 | `tick_interval_seconds` | C/D/E 지속 장판 주기 보존 |

기존 `EffectDamage` graph의 arg 1-5 의미는 변경하지 않는다.

### 6.3 피해 Effect의 status payload composer 확장

다음 legacy Effect 3행은 `EffectKind=Damage`이면서 tick마다 status를 적용한다.

- `sein-d-zone-presence`
- `sein-e-master2-zone-damage`
- `sein-e-master2-zone-presence`

`EffectDamage + ApplyStatus`를 같이 쓰면 operation node가 2개가 되어 현재 graph 검증에 실패한다.
따라서 비-operation composition node를 추가한다.

```text
AttachStatusPayload(
  status_id,
  status_chance,
  status_label,
  status_duration_seconds,
  status_max_stacks,
  status_stack_amount,
  status_merge_policy
)
```

`AttachStatusPayload`는 generated `SkillEffectDefinition`의 기존 status 필드만 채운다.
Effect kind, 피해 계산, tick cadence, target 선택은 바꾸지 않는다.

## 7. 신규 공용 gameplay 의미

없음.

구현 중 현재 wide Choice, legacy Effect, Trigger runtime으로 설명되지 않는 동작이 발견되면
이 제안 범위를 넘어선 것이므로 중단하고 사용자 승인을 받는다.

## 8. Sein-A 열풍 화살

### 기본

base/projectile에 유지:

- Fire 피해 22, 공격력 1.1
- 탄창 8, 재장전 4.4초, 발사 간격 0.32초
- 투사체 속도 18, 관통 1
- 적중 시 `sein-a-hit-mark` 5초
- 치명타 가능

### 특성/마스터 분해

| 선택 | graph/Trigger | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.25)` | 재사용 |
| 특성 2 | `MagazineBonus(4)` | 재사용 |
| 특성 3 | `ReloadTimeMultiplier(0.7692307692307692)` | 재사용 |
| 특성 4 | `PierceBonus(1)` + `DamageMultiplier(1.1)` | 재사용 |
| 특성 5 | `ShotIntervalMultiplier(0.8)` + `DamageMultiplier(0.9)` | 재사용 |
| 백열 화살 | `DamageMultiplier(1.55)` + `PierceBonus(1)` | 재사용 |
| 폭염 화살 | `sein-a-master2-hit-explosion` Trigger 유지 | Trigger 유지 |

폭염 화살 Trigger는 `EventAppliedDamage * 0.5` Fire 피해와 prefab collider를 사용한다.
단순 `EffectDamage(radius=0)`로 치환하면 기존 hitbox 판정과 같다는 근거가 없으므로 유지한다.

## 9. Sein-B 작열 난사

### 기본

base/projectile에 유지:

- Fire 피해 14, 공격력 0.65
- 한 cycle 5발, 탄창 4, 재장전/쿨다운 6초
- 발사 간격 0.18초, 투사체 속도 20
- 치명타 가능

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `AdditionalProjectileBonus(2)` | 재사용 |
| 특성 2 | `DamageMultiplier(1.25)` | 재사용 |
| 특성 3 | `ReloadTimeMultiplier(0.7692307692307692)` | 재사용 |
| 특성 4 | `ShotIntervalMultiplier(0.75)` | 재사용 |
| 특성 5 | `ConsecutiveHitDamageBonus(0.08,0.40)` | graph 노출 |
| 포화 사격 | `AdditionalProjectileBonus(4)` + `DamageMultiplier(0.8)` | 재사용 |
| 집중 사격 | `AdditionalProjectileBonus(-2)` + `DamageMultiplier(1.9)` + `CritChanceBonus(0.2)` | 재사용 |

연속 적중 상태는 기존 `SkillRuntimeInstance`가 보유한다.
node는 수치만 snapshot에 전달하며 상태 소유권을 graph/materializer로 옮기지 않는다.

## 10. Sein-C 화염궤도

### 기본

base/projectile에 유지:

- Fire 피해 38, 공격력 1.8
- 반경 1.8, 지연 0.8초, 쿨다운 6.5초
- contact stop 뒤 delayed impact
- 치명타 가능

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.3)` | 재사용 |
| 특성 2 | `RadiusMultiplier(1.25)` | 재사용 |
| 특성 3 | `CooldownMultiplier(0.8)` | 재사용 |
| 특성 4 | `DamageDelayMultiplier(0.6)` | graph 노출 |
| 특성 5 | `ConditionalDamageMultiplier(sein-a-hit-mark,1,1.35)` | 재사용 |
| 낙화 궤적 | OnExpire persistent `EffectDamage` graph | composer 확장 |
| 관통 궤도 | OnHit contact `EffectDamage` graph | composer 확장 |

낙화 궤적:

```text
Choice/sein-c-master-1/Effect/0
EffectDamage(Fire,38,spell=0,damage=0.25,radius=1.2,attack=1.8,tick=0.5)
+ EffectTarget(Enemy,Nearest,Circle,EffectTarget,Center,OnExpire)
+ EffectLifetime(1.5)
+ EffectVisual(Assets/Prefab/Skill/Sein/Sein_C_Master_1.prefab)
```

관통 궤도:

```text
Choice/sein-c-master-2/Effect/0
EffectDamage(Fire,38,spell=0,damage=0.4,radius=0,attack=1.8)
+ EffectTarget(Enemy,EventTarget,Single,EffectTarget,Center,OnHit,apply_once=true)
+ EffectVisual(Assets/Prefab/Skill/Sein/Sein_C_Master-2.prefab)
```

## 11. Sein-D 초열 지대

### 기본

base/area_attack에 유지:

- Fire 1틱 피해 12, 공격력 0.55
- 반경 3.2, 지속 4초, 주기 0.5초
- 쿨다운 9초, 치명타 가능
- 적중 시 `sein-d-heat-stack`

기본 presence Effect graph:

```text
Skill/sein-d/Effect/0
EffectDamage(Fire,0,spell=0,damage=1,radius=3.2,attack=0,tick=0.5)
+ EffectTarget(Enemy,Nearest,Circle,PrimarySkillCenter,Center,OnCast)
+ EffectLifetime(4)
+ AttachStatusPayload(sein-d-superheated-presence,1,"초열 지대 노출",0.75,1,1)
```

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DurationMultiplier(1.25)` | 재사용 |
| 특성 2 | `ShotIntervalMultiplier(0.8)` | 재사용 |
| 특성 3 | `RadiusMultiplier(1.3)` | 재사용 |
| 특성 4 | `CooldownMultiplier(0.8)` | 재사용 |
| 특성 5 | `ConditionalDamageMultiplier(sein-d-heat-stack,4,1.35)` | 재사용 |
| 열압 폭풍 | `ShotIntervalMultiplier(0.6666667)` + `RadiusMultiplier(0.8)` | 재사용 |
| 잔불 지대 | OnExpire persistent `EffectDamage` graph | composer 확장 |

잔불 지대는 현재 legacy 값인 Fire 12, 공격력 0.55, 배율 0.4, 반경 3.2,
지속 3초, 주기 0.5초와 `Sein_D_Master_2.prefab`을 그대로 옮긴다.

## 12. Sein-E 종말의 사선

### 기본

base/single_attack에 유지:

- Fire 피해 70, 공격력 3.2
- 3개 독립 prefab-hitbox 배치, 쿨다운 16초
- `fire-resist-down` 10, 5초
- `Sein_E.prefab` 사용

### 특성/마스터 분해

| 선택 | graph node | 지원 |
|---|---|---|
| 특성 1 | `DamageMultiplier(1.3)` | 재사용 |
| 특성 2 | `CooldownMultiplier(0.8)` | 재사용 |
| 특성 3 | OnHit `fire-resist-down` +8 Effect graph | 재사용 |
| 특성 4 | `HitTargetCountBonus(1)` + `DamageMultiplier(0.85)` | 재사용 |
| 특성 5 | `ConditionalDamageMultiplier(sein-d-superheated-presence,1,1.5)` | 재사용 |
| 붉은 종말 | `DamageMultiplier(1.8)` + `CooldownMultiplier(1.25)` | 재사용 |
| 잿빛 하늘 | 배치별 persistent damage/presence Effect graph 2개 | composer 확장 |

특성 3:

```text
Choice/sein-e-trait-3/Effect/0
ApplyStatus(fire-resist-down)
+ EffectTarget(Enemy,EventTarget,Single,EffectTarget,Center,OnHit,apply_once=true)
+ EffectLifetime(5)
+ StatusFlatElementResistReduction(8,Fire)
```

잿빛 하늘 damage graph는 Fire 12, 공격력 0.55, 반경 3.2, 지속 3초, 주기 0.5초,
`sein-d-heat-stack` payload와 `Sein_D.prefab`을 보존한다.
presence graph는 같은 반경/지속/주기로 `sein-d-superheated-presence`를 갱신한다.
두 graph 모두 `OnDeploymentCast`를 사용하여 Sein-E의 각 배치 중심에서 실행한다.

`Sein_E.prefab`의 collider와 다중 배치 중심 할당은 외부 asset/runtime 계약이며 이번 작업에서 변경하지 않는다.

## 13. Sein-F 가열 조준

### 기본 Effect graph

```text
Skill/sein-f/Effect/0
StatusModifier
+ EffectTarget(AllAllies,Owner,Battlefield,Caster,Center,OnCast)
+ EffectLifetime(0.5)
+ StatusDamageBonusRate(0.12,Fire)

Skill/sein-f/Effect/1
StatusModifier
+ EffectTarget(AllAllies,Owner,Battlefield,Caster,Center,OnCast)
+ EffectLifetime(0.5)
+ ConditionSkillAttribute(Fire)
+ StatusCriticalChanceBonus(0.08)
```

### 특성

| 선택 | graph | 지원 |
|---|---|---|
| 특성 1 | Fire `StatusDamageBonusRate(0.06)` Effect | 재사용 |
| 특성 2 | target `sein-a`, `MagazineBonus(3)` Plan | 재사용 |
| 특성 3 | Fire 조건 `StatusCriticalDamageBonus(0.20)` Effect | 재사용 |

legacy passive Effect 4행은 위 graph로 교체한 뒤 제거한다.

## 14. Sein-G 불꽃 탄막

### Trigger 유지

현재 Trigger 4행이 다음 조합을 표현한다.

- 기본: 모든 아군 Fire outgoing damage, 4%, 내부 쿨다운 1.5초, Sein-B 60% 자동 발동
- 특성 1: 확률 7%
- 특성 2: 자동 발동 위력 80%
- 특성 3: Sein-G가 발동한 Sein-B cast만 감지하여 Sein-A reload 10% 감소

`TriggerProcChanceBonus`는 존재하지만 triggered-skill damage override와
trigger-origin marker 조건까지 한 Plan graph로 합성하는 공용 node 조합은 없다.
새 trigger modifier 의미를 추가하지 않고 기존 4행을 유지한다.

## 15. Sein-H 연소 궤적

### Sein-C에 부착되는 Effect graph

```text
Skill/sein-h/Effect/0 -> target_skill_id=sein-c
ApplyStatus(fire-resist-down)
+ EffectTarget(Enemy,EventTarget,Single,EffectTarget,Center,OnHit,apply_once=true)
+ EffectLifetime(5)
+ StatusFlatElementResistReduction(12,Fire)
```

### 특성

| 선택 | graph | 지원 |
|---|---|---|
| 특성 1 | target `sein-c`, Fire flat resist reduction 6 Effect | 재사용 |
| 특성 2 | target `sein-c`, `RadiusMultiplier(1.25)` Plan | 재사용 |
| 특성 3 | target `sein-c`, Fire `StatusElementDamageTakenBonus(0.10)` Effect | 재사용 |

generated Effect에는 현재 materializer의 passive owner 추론으로
`RequiresPassiveSkillId=sein-h`가 보존되어야 한다.

## 16. Sein-I 열압 확산

### 기본 Plan/Effect graph

```text
Choice/sein-i-base-shot-interval/Plan/0 -> target_skill_id=sein-d
ShotIntervalMultiplier(0.8)

Skill/sein-i/Effect/0 -> target_skill_id=sein-d
ApplyStatus(fire-exposure)
+ EffectTarget(Enemy,EventTarget,Single,EffectTarget,Center,OnHit,apply_once=true)
+ EffectLifetime(4)
+ StatusElementDamageTakenBonus(0.15,Fire)
```

### 특성

| 선택 | graph | 지원 |
|---|---|---|
| 특성 1 | target `sein-d`, Fire exposure +0.07 Effect | 재사용 |
| 특성 2 | target `sein-d`, `DurationBonus(2)` Plan | 재사용 |
| 특성 3 | target `sein-d`, `RadiusMultiplier(1.25)` Plan | 재사용 |

주의: 현재 특성 2는 wide `duration_bonus=2`이며 snapshot의 일반 duration bonus로 소비된다.
따라서 status duration뿐 아니라 같은 snapshot을 사용하는 지속시간에도 영향을 줄 수 있다.
node 이전에서는 현재 동작을 그대로 `DurationBonus(2)`로 보존한다.
오직 exposure 지속시간만 +2초로 제한하려면 별도 gameplay correction으로 분리한다.

## 17. Sein-J 종말 예고

### Sein-E에 부착되는 Effect graph

```text
Skill/sein-j/Effect/0 -> target_skill_id=sein-e
ApplyStatus(fire-exposure)
+ EffectTarget(Enemy,EventTarget,Single,EffectTarget,Center,OnHit,apply_once=true)
+ EffectLifetime(5)
+ StatusElementDamageTakenBonus(0.20,Fire)
```

### 특성/Trigger

| 선택 | graph/Trigger | 지원 |
|---|---|---|
| 특성 1 | target `sein-e`, Fire exposure +0.10 Effect | 재사용 |
| 특성 2 | 기존 6개 추가 10% cooldown/reload 환급 Trigger 유지 | Trigger 유지 |
| 특성 3 | target `sein-e`, Fire flat resist reduction 8 Effect | 재사용 |

기본 환급 6행과 특성 2 추가 환급 6행을 유지한다.
Sein-A/B는 magazine/reload와 cooldown을 별도로 다루고 C/D/E는 cooldown을 다루므로
현재 12행을 하나로 합치면 대상별 action 의미를 잃는다.

generated Effect에는 `RequiresPassiveSkillId=sein-j`가 보존되어야 한다.

## 18. legacy 행 제거 계획

| legacy 데이터 | 처리 |
|---|---|
| Sein-C Effect 5행 | C master graph와 H passive-to-C graph로 교체 후 제거 |
| Sein-D Effect 4행 | D presence/master graph와 I passive-to-D graph로 교체 후 제거 |
| Sein-E Effect 6행 | E trait/master graph와 J passive-to-E graph로 교체 후 제거 |
| Sein-F Effect 4행 | F Skill/Choice Effect graph로 교체 후 제거 |
| Trigger 17행 | 이벤트 envelope로 전부 유지 |
| legacy direct node/param | 현재 0행이므로 제거 대상 없음 |

같은 generated Effect 의미와 legacy Effect 행을 동시에 남기지 않는다.
Choice ID, title, description, group, sort order와 target skill 연결은 유지한다.
graph로 이전한 wide behavior 필드는 blank/default로 정리하여 중복 합성을 막는다.

## 19. 구현 순서

### Phase 0: 승인과 경계 확정

1. 신규 gameplay 의미 0개 원칙 승인
2. `DamageDelayMultiplier`, `ConsecutiveHitDamageBonus` graph 노출 승인
3. `EffectDamage` param 6/7 추가 승인
4. `AttachStatusPayload` composer 확장 승인
5. Trigger 17행 유지 승인

### Phase 1: 공용 positional node 기반

1. 새 Choice handler schema/definition/param 추가
2. `InGameSkillDefinitionMapper.ApplyNormalizedChoiceNode(...)`에 두 wide 의미 연결
3. legacy-overlap 검증에 새 wide 필드 추가
4. `EffectDamage` param definition에 attack coefficient/tick interval 추가
5. `AttachStatusPayload` handler schema/definition/composer 연결

### Phase 2: A-E active 이전

1. A projectile Plan graph, master-2 Trigger 유지
2. B projectile Plan graph
3. C projectile Plan/Effect graph와 legacy Effect 제거
4. D area Plan/Effect graph와 legacy Effect 제거
5. E single Plan/Effect graph와 legacy Effect 제거

### Phase 3: F-J passive 이전

1. F Effect/Choice graph와 legacy Effect 제거
2. G Trigger 4행 유지 검증
3. H passive-to-C Effect/Choice graph 이전
4. I `PassiveBase`, passive-to-D Effect/Choice graph 이전
5. J passive-to-E Effect/Choice graph 이전, Trigger 12행 유지

### Phase 4: 검증

1. Sein graph/direct-node 혼용 0
2. Effect graph마다 operation 정확히 1개
3. generated Effect와 legacy Effect 중복 0
4. Sein legacy Effect 0행
5. Trigger 17행 event/gate/cadence parity
6. wide behavior와 graph 중복 0
7. base 10행/Choice 51행 ID 유지
8. Runtime/Editor build
9. Unity-MCP CSV sync/source validation/console 확인
10. 사용자 Play Mode A-J 조합 검증

## 20. 위험과 중단 조건

- Sein-C master-1의 현재 1.5초를 reference 2초로 조용히 바꾸지 않는다.
- Sein-A master-2 prefab hitbox Trigger를 근거 없이 반경 Effect로 치환하지 않는다.
- Sein-G Trigger 4행을 trigger damage/origin modifier node 없이 합치지 않는다.
- Sein-J 대상별 환급 Trigger 12행을 단일 action으로 합치지 않는다.
- D/E persistent damage Effect의 status payload를 누락한 채 이전하지 않는다.
- E 다중 배치 수를 단순 hit target cap으로 바꾸지 않는다.
- E의 `OnDeploymentCast` Effect를 전체 cast 1회 Effect로 바꾸지 않는다.
- passive-to-active Effect의 `RequiresPassiveSkillId` gate 없이 이전하지 않는다.
- graph로 옮긴 wide 필드를 남겨 이중 multiplier를 만들지 않는다.
- 구현 중 새 gameplay 의미, 새 CSV 파일, 새 CSV 컬럼이 필요하면 중단하고 사용자에게 보고한다.

## 21. 수용 기준

### 기능

- A-E 기본 수치, 액티브 특성 25개, 마스터 10개가 현재 runtime 동작을 유지한다.
- F-J 기본 효과, 패시브 특성 15개, Sein-I `PassiveBase`가 현재 동작을 유지한다.
- Sein-B 연속 적중은 대상 변경 시 초기화되고 +8%씩 최대 +40%를 유지한다.
- Sein-C는 contact stop, delayed impact, OnHit/OnExpire follow-up 순서를 유지한다.
- Sein-D presence와 E master-2 presence는 기존 tick마다 status를 갱신한다.
- Sein-E는 3/4개 독립 prefab-hitbox 배치와 배치별 master-2 zone을 유지한다.
- Sein-G는 4/7% proc, 60/80% triggered B, 1.5초 내부 쿨다운, origin-gated reload를 유지한다.
- Sein-J는 기본 20%와 특성 추가 10% 대상별 환급을 유지한다.

### 데이터

- Sein base 10행과 Choice 51행 ID를 유지한다.
- 새 graph CSV 파일과 새 CSV 컬럼을 추가하지 않는다.
- Sein positional graph는 기존 21컬럼 schema를 사용한다.
- Sein legacy direct node/param은 0행을 유지한다.
- Sein legacy Effect는 graph로 대체된 뒤 0행이다.
- Trigger 17행은 유지한다.
- graph로 이전한 wide behavior 필드는 기본값/blank가 된다.

### 검증 경계

- Code Builder: CSV shape, node definition/handler/composer 정합성, build, Unity-MCP sync/validation/console
- 사용자: Play Mode projectile cadence, delayed impact, persistent tick/status, multi-deployment, trigger chance/cooldown/refund parity
- Code Reviewer는 사용자 명시 승인 시 1회 실행
- MSW-MCP는 사용하지 않고 Unity-MCP만 사용

## 22. 관련 보드

구현 시 최소 갱신:

- `boards/MON/SEIN_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- status Effect composer와 persistent-zone payload를 변경하므로 `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- prefab/collider/scene mapping을 실제 변경할 때만 `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`

## 23. 구현 결과 (2026-07-13)

Code Builder 구현 완료 상태다.

- Sein positional graph: 121행
- Sein Choice: 51행 유지, graph 이전 대상 wide behavior 잔존 0
- Sein legacy Effect: 19행에서 0행
- Sein Trigger: 17행 유지
- Sein legacy direct node/param: 0행 유지
- 신규 graph CSV/컬럼: 없음
- 프리팹/씬 변경: 없음

공유 확장:

- `DamageDelayMultiplier`: 기존 snapshot의 damage-delay multiplier 필드로 합성
- `ConsecutiveHitDamageBonus`: 기존 연속 적중 bonus/max 필드로 합성
- `AttachStatusPayload`: 현재 Effect operation kind를 유지한 채 status payload를 합성
- `EffectDamage`: runtime이 이미 읽던 attack-power coefficient와 tick interval을 positional param으로 노출

검증:

- runtime skill CSV shape 및 graph 정적 검사 오류 0
- Runtime/Editor C# build 오류 0; 기존 assembly conflict warning 각 2개
- Unity-MCP CSV source validation 성공
- Unity-MCP runtime catalog sync 성공
- Play Mode A-J 조합 parity는 사용자 검증 필요
