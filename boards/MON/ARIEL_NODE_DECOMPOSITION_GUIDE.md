# Ariel Node Decomposition Guide

## 1. 문서 목적

이 문서는 현재 Ariel 런타임 구현을 근거 모델로 삼아, 다른 몬스터가 아직 사용 중인
`Pakuri/Assets/CSVdata/runtime/monster/skills/effects/*_skill_effects.csv` 행을
`Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/*_skill_nodes.csv`와
`*_skill_node_params.csv`의 의미 단위 노드로 전환하는 기준을 정의한다.

이 문서만 전달받은 AI agent가 다음을 수행할 수 있어야 한다.

1. 참고 스킬 문장을 기능 단위로 분해한다.
2. 각 기능을 기본 스킬, Choice 수정값, Effect 효과 객체, Trigger 결합 중 하나로 분류한다.
3. 현재 구현된 핸들러를 먼저 재사용한다.
4. 현재 핸들러로 표현할 수 없는 기능만 근거와 함께 신규 공용 핸들러 후보로 보고한다.
5. legacy effect 행과 node-owned effect가 동시에 실행되지 않도록 한쪽 권한만 남긴다.
6. CSV, 런타임, 트리거 참조와 사용자 Play Mode 검증 경계를 보존한다.

이 문서는 설계·구현 핸드오프다. 이 문서를 작성한 작업에서는 코드나 CSV 스킬 행을 변경하지 않는다.

## 2. 근거와 현재 권한

### 2.1 검사한 근거

- Ariel 의도 문서 10개:
  - `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`
  - `b-radiant-shield.md`
  - `c-blessing-wave.md`
  - `d-celestial-brand.md`
  - `e-archangel-descent.md`
  - `f-guiding-light.md`
  - `g-guardian-doctrine.md`
  - `h-spread-blessing.md`
  - `i-brand-revelation.md`
  - `j-sanctuary-proclamation.md`
- 현재 노드 CSV:
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/{kind}/{kind}_skill_nodes.csv`
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/{kind}/{kind}_skill_node_params.csv`
- 현재 legacy effect CSV:
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/effects/{kind}/{kind}_skill_effects.csv`
- 노드 스키마와 검증:
  - `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`
- Effect 노드 그룹 조립:
  - `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`
- Choice/Skill 노드 매핑:
  - `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
  - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`
- Effect 실행:
  - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs`
  - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillPlanActionDispatcher.cs`

### 2.2 현재 디스크 기준 집계

type row를 제외한 실제 런타임 행 기준이다.

| 항목 | 현재 값 |
|---|---:|
| 전체 노드 | 139 |
| Ariel 노드 | 124 |
| Ariel Choice 노드 | 39 |
| Ariel Effect 노드 | 85 |
| Ariel Effect owner 그룹 | 20 |
| Rin 노드 | 11 |
| Vega 노드 | 4 |
| legacy effect 행 | 96 |
| Eve effect 행 | 34 |
| Rin effect 행 | 20 |
| Sein effect 행 | 19 |
| Vega effect 행 | 23 |

따라서 “현재 노드 기반 스킬은 Ariel뿐이다”는 표현은 다음처럼 고쳐 사용한다.

> Ariel은 Effect 효과 본체까지 `owner_kind=Effect` 노드로 옮기고 Ariel effect CSV를 제거한 첫 몬스터다. Rin과 Vega에도 일부 Choice 노드는 존재하지만, Eve/Rin/Sein/Vega의 효과 본체는 아직 legacy `effects` CSV 행을 사용한다.

### 2.3 현재 권한 경계

- 기본 스킬 수치와 runtime kind: `base/{kind}/skills_{kind}.csv`
- 강화/마스터 메타데이터: `choices/{kind}/skill_choices_{kind}.csv`
- 강화/마스터 기능 수정값: `owner_kind=Choice` 노드
- 독립적으로 실행되는 피해/상태/방어막/오라 효과: `owner_kind=Effect` 노드 그룹
- 이벤트와 효과/스킬 결합: `triggers/{kind}/{kind}_skill_triger.csv`
- 상태 id의 공통 의미: 현재 상태 정의와 `StatusEffectRuntime`

`PakuriCsvRuntimeData.Build.cs`는 legacy effect와 node-owned effect를 모두 `MultiEffects`에 추가한다.
같은 `effect_id`를 두 권한에 동시에 남기면 두 번 실행될 수 있으므로, effect 하나를 노드로 옮기는 작업에서는 해당 legacy 행을 같은 변경에서 제거해야 한다.

## 3. 용어

### 3.1 기본 스킬 본체

투사체 속도, 기본 피해, 주문력/공격력 계수, 기본 쿨타임, 기본 범위처럼 스킬 종류별 Executor가 직접 소비하는 값이다. 현재 `base` CSV에 남아 있다.

### 3.2 Choice 노드

특성, 강화, 마스터 선택지가 기존 스킬이나 기존 Effect의 값을 수정하는 작은 기능 노드다.

예:

- 피해 `+25%` → `DamageMultiplier`
- 쿨타임 `-20%` → `CooldownMultiplier(multiplier=0.8)`
- 방어막량 `+30%` → `ShieldAmountMultiplier(multiplier=1.3)`
- 축복 지속시간 `+2초` → `StatusDurationBonus(status_id=blessing, bonus_seconds=2)`

### 3.3 Effect owner 그룹

하나의 `owner_id` 아래 여러 의미 노드를 조합해 하나의 `SkillEffectDefinition`을 만든다.

한 그룹은 반드시 다음 구조를 따른다.

```text
정확히 1개의 주 연산 노드
+ 필요한 Target 노드
+ 필요한 Condition 노드
+ 필요한 Lifetime 노드
+ 0개 이상의 상태 수정 노드
+ 필요한 Visual 노드
```

### 3.4 Trigger 결합

`OnSkillCast`, `OnOutgoingDamage`, `OnShieldExpire`, `OnStatusExpire` 같은 이벤트를 Effect 또는 다른 스킬과 연결하는 데이터다.

현재 `owner_kind=Trigger` normalized node는 검증 단계에서 “runtime plans에 연결되지 않음” 오류를 발생시킨다. 따라서 이 전환에서는 Trigger 행을 노드로 옮기지 않는다. Trigger가 참조하는 `triggered_effect_id`만 Effect node 그룹의 `owner_id`와 일치시킨다.

### 3.5 핸들러

노드의 의미를 코드가 실행 가능한 데이터로 바꾸는 공용 처리기다. 스키마에 이름이 등록됐다는 사실만으로 런타임 동작이 구현됐다고 판단하면 안 된다. 아래 문서에서 “구현됨”은 실제 mapper 또는 effect composer가 값을 소비하는 경우만 뜻한다.

## 4. 노드 CSV 형식

### 4.1 노드 행

```text
node_id,monster_id,owner_kind,owner_id,target_skill_id,node_kind,handler_id,
sort_order,enabled_by_default,requires_active_choice_id,excludes_active_choice_id,
requires_passive_skill_id,excludes_passive_skill_id,runtime_support_state,runtime_support_notes
```

### 4.2 파라미터 행

```text
node_id,monster_id,param_key,value_type,value
```

현재 허용 `value_type`:

- `string`
- `int`
- `float`
- `bool`
- `enum`
- `asset_path`
- `skill_id`
- `status_id`
- `choice_id`

### 4.3 메타데이터 위치 규칙

Effect 그룹의 다음 값은 반드시 주 연산 노드에 둔다.

- `enabled_by_default`
- `requires_active_choice_id`
- `excludes_active_choice_id`
- `requires_passive_skill_id`
- `excludes_passive_skill_id`
- `runtime_support_state`
- `runtime_support_notes`

현재 builder는 Effect 정의의 게이트와 지원 상태를 주 연산 노드에서 생성한다. 조합 노드에만 게이트를 두면 Effect 전체 게이트로 반영되지 않는다.

## 5. 현재 구현된 Effect owner 핸들러

### 5.1 주 연산 핸들러

새 Effect 그룹은 아래 중 정확히 하나를 사용한다.

| 핸들러 | 의미 | 주요 파라미터 | 신규 작성 기준 |
|---|---|---|---|
| `ApplyStatus` | 실제 상태 id 적용 | 필수 `status_id`; 선택 상태 메타데이터 | `blessing`, `holy-exposure`, `shock` 등 실제 상태 |
| `ApplyShield` | 방어막 상태와 방어막량 적용 | `base_damage`, 공격/주문력 계수, `damage_multiplier` | 방어막 효과 |
| `StatusModifier` | 독립 상태 id가 아닌 능력치 오라/임시 modifier | 상태 메타데이터 선택 | legacy `status_effect_id=passive-buff` 대체 |
| `EffectDamage` | 독립 피해 또는 지속 피해 영역 | 필수 `attribute`; 피해 계수, 배율, 반경, tick | 추가 타격, 두 번째 파동, 폭발, 장판 |
| `EffectExtendStatusDuration` | 기존 상태 지속시간 연장 | 필수 `status_id` | 현재 방어막/상태 시간 연장 |
| `EffectStatus` | legacy 호환용 상태 carrier | 필수 `status_id` | 신규 작성 금지 |

`EffectStatus(status_id=passive-buff|shield|blessing)` 형태의 carrier 노드를 새로 만들지 않는다. Ariel의 현재 의미 노드 규칙처럼 각각 `StatusModifier`, `ApplyShield`, `ApplyStatus`를 사용한다.

### 5.2 Effect 조합 핸들러

| 핸들러 | 의미 | 파라미터 |
|---|---|---|
| `EffectTarget` | 적용 대상, 형태, 중심, 타이밍 | `target_side`, `target_selection`, `target_shape`, `center_mode`, `visual_anchor_mode`, `effect_timing`, `delay_seconds`, `apply_once`, `cover_all` |
| `EffectVisual` | Effect prefab | `skill_effect_prefab_path` |
| `ConditionStatus` | 적용 대상 상태 조건 | `status_id`, 선택 `target_side`, `source_skill_id`, `min_stacks` |
| `ConditionSkillAttribute` | 대상이 특정 속성 스킬 보유 | `attribute` |
| `EffectLifetime` | 상태 지속시간 또는 Damage zone 활성시간 | `duration_seconds` |

`EffectLifetime`은 Effect 종류에 따라 다른 필드에 투영된다.

- Damage Effect → `ActiveDurationSeconds`
- Status/Shield/Modifier Effect → `StatusDurationSeconds`

지연 Effect는 스키마에만 있는 `DelayedDamage`를 사용하지 않고, 현재 실행되는 Ariel C 마스터 2처럼 `EffectTarget(effect_timing=Delayed, delay_seconds=...)`를 사용한다.

### 5.3 Effect에서 현재 실제로 소비되는 modifier 핸들러

| 핸들러 | 의미 | 파라미터 |
|---|---|---|
| `StatusActionSpeedBonus` | 행동속도 수정 | `bonus` |
| `StatusSpellPowerBonus` | 주문력 수정 | `bonus` |
| `StatusDamageBonusRate` | 주는 속성 피해 수정 | `bonus`, 선택 `attribute` |
| `StatusShieldReceivedBonus` | 받는 방어막량 수정 | `bonus` |
| `StatusDamageTakenBonus` | 받는 피해 수정 | `bonus` |
| `StatusFlatElementResistReduction` | 고정 속성 저항 감소 | `bonus`, 선택 `attribute` |
| `StatusCriticalChanceBonus` | 치명타 확률 수정 | `bonus` |
| `DamageMultiplier` | Effect 피해 배율 | `multiplier` |
| `ShieldAmountMultiplier` | Effect 방어막 배율 | `multiplier` |

## 6. 현재 구현된 Choice/Skill plan 핸들러

다음 핸들러는 `InGameSkillDefinitionMapper` 또는 `SkillExecutionSnapshot`이 실제로 소비한다.

### 6.1 기본 수치와 상태 수정

- `DamageMultiplier`
- `ShieldAmountMultiplier`
- `CountStatusDamageMultiplier`
- `CooldownMultiplier`
- `MagazineBonus`
- `ReloadTimeMultiplier`
- `PierceBonus`
- `HitTargetCountBonus`
- `RadiusMultiplier`
- `RadiusBonus`
- `DurationBonus`
- `StatusDurationBonus`
- `StatusActionSpeedBonus`
- `StatusAttackPowerBonus`
- `StatusAilmentResistanceBonus`
- `StatusDamageBonusRate`
- `StatusShieldReceivedBonus`
- `StatusCriticalChanceBonus`
- `StatusDamageTakenBonus`
- `StatusFlatElementResistReduction`
- `StatusConditionalDamageTakenBonus`
- `StatusElementDamageTakenBonus`
- `StatusCriticalDamageTakenBonus`

### 6.2 조건, 처형, 보스, 사망 동작

- `TargetHealthRatioCondition`
- `TargetHealthRatioThresholdBonus`
- `ExecuteDamageMultiplier`
- `TargetPredicateDamageMultiplier` (`predicate=is_boss`)
- `BossDamageMultiplier`
- `ExecuteCritChanceBonus`
- `CooldownReset`
- `CooldownResetOnKill`
- `CooldownRefund`
- `CooldownRefundBonus`

### 6.3 명중과 반복 동작

- `AdditionalDamage`
- `EveryNthHitChainDamage`
- `RepeatPerTarget`
- `TargetStatusCritBonus`
- `RedistributeConsumedStatus`

이 목록은 핸들러가 모든 owner kind에서 동작한다는 뜻이 아니다. 예를 들어 `StatusAttackPowerBonus`는 Choice mapper에서는 구현됐지만 현재 Effect composer에서는 소비하지 않는다. owner kind별 구현 여부를 반드시 확인한다.

## 7. 스키마 등록만 있고 normalized runtime 동작이 연결되지 않은 핸들러

다음 핸들러는 `NormalizedSkillAuthoring.cs`에 스키마가 있으나 현재 normalized mapper/effect composer에서 실행 의미를 만들지 않는다.

- `DelayedDamage`
- `RequiredTargetStatus`
- `TargetStatusStackDamage`
- `ConsumeTargetStatus`
- `HitCountCooldownRefund`
- `BranchProjectile`
- `SpawnProjectile`

AI agent는 이 이름을 “이미 구현된 핸들러”로 간주하여 CSV를 작성하면 안 된다. 해당 기능이 필요하면 실제 mapper, snapshot, executor 소비 경로를 먼저 구현하고 검증해야 한다.

## 8. Ariel A-J가 기능을 나눈 실제 방식

참고 문서는 기능 의도를 설명하고, 현재 CSV/코드는 런타임에 실제 존재하는 분해 구조를 증명한다. 아래 표는 수치 parity 표가 아니라 기능 경계 비교표다.

| 슬롯 | 참고 문서의 기능 | 현재 분해 방식 |
|---|---|---|
| A 심판의 빛 | 기본 투사체, 피해/탄창/재장전/관통 특성, 마지막 탄 폭발, 명중 노출 | 기본 투사체는 base CSV. 특성 1~5는 `DamageMultiplier`, `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `CountStatusDamageMultiplier`. 마스터 1은 trigger CSV. 마스터 2는 `OnOutgoingDamage` trigger + `ApplyStatus`/`EffectTarget(EventTarget)` Effect 그룹. |
| B 성광 방패 | 기본 방어막, 방어막량/지속/쿨다운, 만료 피해, 보호 중 신성 피해, 흡수 반사 | 기본 방어막은 base CSV. 단순 수치는 Choice 노드. 특성 4와 마스터 2는 shield event trigger CSV. 특성 5는 `StatusModifier + EffectTarget + ConditionStatus(shield) + EffectLifetime + StatusDamageBonusRate`. |
| C 축복의 파동 | 기본 범위 피해, 아군 행동속도, 범위/지속/수치 특성, 주문력 대체, 두 번째 파동 | 기본 피해는 base CSV. 기본 축복은 `ApplyStatus(blessing) + EffectTarget(AllAllies) + EffectLifetime + StatusActionSpeedBonus`. 단순 강화는 Choice 노드. 방어막 아군 추가 효과는 별도 조건 Effect. 마스터 1은 기본 축복을 제외하고 별도 `ApplyStatus + StatusSpellPowerBonus`. 마스터 2는 `EffectDamage + EffectTarget(Delayed) + EffectVisual`. |
| D 천상의 낙인 | 단일 피해와 신성 노출, 대상 수/지속/피해 수정, 치명타 피해 증가, 만료 폭발 | 기본 피해와 기본 명중 상태는 base CSV. 특성/마스터 1은 Choice 노드. 마스터 2는 `OnStatusExpire` trigger CSV. 별도 Effect 복제품을 만들지 않는다. |
| E 대천사의 강림 | 전장 피해, 아군 방어막, 조건부 추가 피해, 방어막 지속 연장, 피해 감소, 피해/방어막 상반 수정 | 기본 전장 피해는 base CSV. 방어막은 `ApplyShield` Effect 그룹. 수치 강화는 Choice 노드. 노출 대상 추가 피해는 `EffectDamage + ConditionStatus`. 성역은 `StatusModifier + StatusDamageTakenBonus`. 현재 방어막 연장은 `EffectExtendStatusDuration`; 스킬 지속값 수정은 별도 Choice 노드. 마스터 2는 피해와 방어막을 서로 다른 Choice 핸들러로 수정. |
| F 빛의 인도 | 파티 신성 피해, A 탄창, 신성 스킬 보유 아군 치명타 | 기본 파티 신성 피해는 지속 갱신되는 `StatusModifier` Effect. 단순 증가는 Choice 노드. 스킬 속성 조건 치명타는 `StatusModifier + ConditionSkillAttribute + StatusCriticalChanceBonus`. |
| G 수호 교리 | 받는 방어막량, 전투 시작 방어막, 보호 중 신성 피해 | 받는 방어막 오라와 시작 방어막을 서로 다른 Effect 그룹으로 분리. 시작 방어막은 `ApplyShield + EffectTarget(apply_once=true)`. 특성 수치는 Choice 노드. 보호 중 피해는 조건 Effect. |
| H 축복 전파 | 축복 대상 신성 피해와 행동속도, 각 수치/지속 강화 | 기본은 하나의 `StatusModifier + ConditionStatus(blessing)` Effect에 두 modifier 노드를 조합. 각 수치와 축복 지속시간은 Choice 노드. |
| I 낙인 계시 | 노출 대상 받는 피해, 쿨타임, 신성 저항 감소 | 받는 피해와 신성 저항 감소는 의미가 다르므로 두 Effect 그룹. 단순 증가와 쿨타임은 Choice 노드. |
| J 성역 선포 | E 시전 후 행동속도, E 방어막 보유자 신성 피해 | 행동속도 Effect와 `OnSkillCast(event_skill_id=ariel-e)` trigger를 분리. E 방어막 보유 조건은 `ConditionStatus(shield, source_skill_id=ariel-e-shield-base)`. 수치와 E 쿨타임은 Choice 노드. |

### 8.1 Ariel에서 재사용해야 할 핵심 패턴

#### 패턴 A: 단순 강화는 Effect 복제품이 아니다

```text
기본 Effect 1개
+ Choice DamageMultiplier/Duration/Status modifier
```

특성 조합마다 Effect 그룹을 복제하지 않는다. 현재 Ariel cleanup은 이전에 복제됐던 `MigratedToEffectBinding` 조합 행을 제거하고 기능 Choice 노드로 되돌렸다.

#### 패턴 B: 새로운 행동만 별도 Effect다

다음처럼 실행 타이밍, 대상, 조건 또는 결과 종류가 달라지면 별도 Effect 그룹을 만든다.

- 두 번째 파동
- 노출 대상에게만 추가 피해
- 현재 방어막 지속시간 연장
- 보호 중에만 유지되는 오라

#### 패턴 C: 이벤트는 Trigger, 결과는 Effect

```text
Trigger row: 언제 실행하는가
Effect node group: 무엇을 적용하는가
```

예: Ariel A 마스터 2는 `OnOutgoingDamage`가 시점을 소유하고, Effect 그룹은 EventTarget에 `holy-exposure`를 적용한다.

#### 패턴 D: 패시브 오라는 짧은 수명으로 갱신할 수 있다

Ariel F-J의 조건 오라는 `EffectLifetime=0.5`와 passive refresh를 사용한다. 이 값은 모든 패시브에 무조건 복사하는 규칙이 아니다. 기존 effect의 지속시간과 `InGamePassiveEffectRuntime` 갱신 주기를 확인한 경우에만 사용한다.

## 9. legacy effect 컬럼을 노드로 옮기는 표준 매핑

| legacy effect 컬럼 | node 위치 |
|---|---|
| `effect_id` | 모든 그룹 노드의 `owner_id` |
| `skill_id` | 모든 Effect 노드의 `target_skill_id` |
| `monster_id` | 노드/파라미터 `monster_id` |
| `sort_order` | 주 연산 노드의 `sort_order`; 조합 노드는 의미 순서로 연속 배치 |
| `effect_kind=Damage` | `EffectDamage` 주 연산 |
| `effect_kind=Status`, 실제 status id | `ApplyStatus` 주 연산 |
| `effect_kind=Status`, shield | `ApplyShield` 주 연산 |
| `effect_kind=Status`, passive-buff/stat aura | `StatusModifier` 주 연산 |
| target/shape/center/timing/apply/cover 컬럼 | `EffectTarget` 파라미터 |
| `condition_status_id`, `condition_target_side` | `ConditionStatus` |
| `condition_skill_attribute` | `ConditionSkillAttribute` |
| `active_duration_seconds` | Damage 그룹의 `EffectLifetime.duration_seconds` |
| `status_duration_seconds` | Status 그룹의 `EffectLifetime.duration_seconds` |
| `tick_interval_seconds` | `EffectDamage.tick_interval_seconds` |
| `skill_effect_prefab_path` | `EffectVisual.skill_effect_prefab_path` |
| `requires_active_choice_id` 등 gate | 주 연산 노드 메타데이터 |
| `runtime_support_state` | 주 연산 노드 메타데이터 |
| 상태 수치 컬럼 | 해당 의미의 `Status*Bonus` 노드 |

기존 target 컬럼을 기계적으로 전부 복사하지 않는다. 현재 builder 기본값은 다음과 같다.

```text
TargetSide=Enemy
TargetSelection=Nearest
TargetShape=Single
CenterMode=PrimarySkillCenter
VisualAnchorMode=Center
EffectTiming=OnCast
```

기본값과 같고 기능적으로 필요 없는 target 파라미터는 생략한다. 단, 대상 의미를 증명하는 `AllAllies`, `Self`, `EventTarget`, `Battlefield`, `apply_once` 등은 명시한다.

## 10. 다른 몬스터 effect 96행 전환 가능성 감사

현재 비-Ariel effect 96행의 실제 non-empty 동작 필드를 현재 effect composer와 비교했다.

| 분류 | 행 수 | 의미 |
|---|---:|---|
| 현재 핸들러만으로 전환 가능 | 58 | 코드 확장 없이 semantic Effect 노드로 이동 가능 |
| 기존 핸들러 재사용을 위한 effect composer 확장 필요 | 16 | 새 handler id보다 기존 handler의 owner_kind=Effect 지원을 먼저 추가 |
| 현재 의미를 보존하려면 신규 semantic handler 필요 | 22 | 기존 handler로 표현하면 의미가 달라지는 행 |

### 10.1 기존 핸들러를 먼저 확장할 16행

| 필요한 확장 | 행 수 | 처리 |
|---|---:|---|
| `StatusAttackPowerBonus`를 Effect composer에서도 소비 | 3 | 기존 handler/schema 재사용 |
| `StatusElementDamageTakenBonus`를 Effect composer에서도 소비하고 선택 `attribute` 지원 | 7 | 기존 handler 확장 |
| `StatusDamageTakenBonus`에 선택 `incoming_skill_runtime_kinds` 지원 | 6 | 기존 handler 확장; 별도 중복 handler를 만들지 않음 |

### 10.2 신규 핸들러가 실제로 필요한 22행

아래 행 수는 중복 없는 effect 행 수 기준이며, 구현할 handler는 공용 의미 단위로 한 번만 만든다.

| 신규 후보 핸들러 | 영향 행 수 | 필요한 의미/파라미터 |
|---|---:|---|
| `EffectDamageStatus` | 4 | Damage/zone tick이 상태도 함께 적용. `ApplyStatus`를 별도 OnCast Effect로 분리하면 지속 zone refresh 의미가 깨지므로 신규 조합 handler 필요. 상태 payload 파라미터 재사용. |
| `StatusMoveSpeedBonus` | 2 | `bonus` |
| `StatusElementResistReduction` | 2 | 비율 저항 감소 `bonus`, 선택 `attribute`; flat 감소와 구분 |
| `StatusCriticalDamageBonus` | 2 | 주는 치명타 피해 `bonus`; 받는 치명타 피해와 구분 |
| `StatusCriticalResistanceBonus` | 1 | `bonus` |
| `StatusOutgoingAdditionalDamage` | 1 | `multiplier`, `trigger_attribute`, `damage_attribute` |
| `StatusConditionalStatusChanceBonus` | 1 | `target_status_id`, `bonus` |
| `StatusAppliedStatusDurationBonus` | 1 | `status_id`, `bonus_seconds` |
| `ConditionHealthRatioMax` | 2 | 대상 체력 비율 상한 `max_ratio` |
| `ConditionHitCountMin` | 2 | `OnHitCount` 실행 최소 명중 수 `min_count` |
| `RequiredSourceStatus` | 4 | 시전자/owner 상태 게이트 `status_id`, `min_stacks`; 대상 `ConditionStatus`와 의미가 다름 |

### 10.3 신규 핸들러 판단 규칙

신규 핸들러는 다음 조건을 모두 만족할 때만 만든다.

1. 현재 등록된 handler와 effect composer 소비 코드를 검색했다.
2. 기존 handler를 owner kind에 맞게 확장해도 의미가 충돌하지 않는지 확인했다.
3. legacy 필드가 실제 non-zero/non-empty 행에서 사용됨을 확인했다.
4. 기존 두 노드 조합으로 같은 lifecycle과 대상 의미를 만들 수 없음을 확인했다.
5. 공용 이름과 파라미터로 다른 몬스터도 재사용할 수 있다.
6. Skill Builder 작업이라면 shared runtime/common-logic 확장 전에 사용자 승인을 받았다.

`ConditionStatus(target_side=Self)`를 `RequiredSourceStatus` 대신 쓰거나, `StatusFlatElementResistReduction`을 비율 저항 감소에 쓰는 식의 의미 왜곡은 금지한다.

## 11. AI agent 구현 절차

### 단계 0: 작업 범위 라우팅

실제 Skill Builder는 먼저 다음 최소 읽기 집합을 선언한다.

- 대상 몬스터/스킬 참고 문서
- 대상 skill kind의 base CSV
- 대상 choice CSV
- 대상 effect CSV
- 대상 trigger CSV가 참조하는 effect를 옮기는 경우 해당 trigger CSV
- 대상 skill kind의 node와 node-param CSV
- 이 문서에 없는 handler가 필요한 경우 해당 handler schema/mapper/composer 코드

다른 몬스터 참고 문서나 오래된 구현을 값 추측용으로 읽지 않는다.

### 단계 1: 기능 원장 작성

참고 문장의 각 기능을 한 줄씩 기록한다.

| 기능 | 소유자 | 타이밍 | 대상 | 조건 | 결과 | 수치 출처 |
|---|---|---|---|---|---|---|
| 예: 보호 중 신성 피해 +12% | B trait5 | passive refresh/OnCast | AllAllies | target has shield | Status modifier | 현재 effect/참고 입력 |

값이 참고 문서와 현재 CSV에서 다르면 임의로 하나를 선택하지 않는다. 현재 런타임 parity 마이그레이션이면 현재 CSV 값을 유지하고, 디자인 변경이면 사용자 확인을 받는다.

### 단계 2: 소유권 분류

각 기능에 아래 질문을 순서대로 적용한다.

1. Executor가 직접 소비하는 기본 body인가? → base CSV 유지
2. 기존 스킬/Effect의 숫자만 바꾸는가? → Choice 노드
3. 새로운 피해/상태/방어막/오라/지속시간 연장 결과인가? → Effect 그룹
4. 특정 이벤트가 실행 시점을 결정하는가? → trigger CSV 유지 + Effect owner id 결합
5. 상태 자체의 전 몬스터 공통 의미인가? → status 정의 소유 여부 별도 검토

### 단계 3: 주 연산 선택

```text
Damage                 → EffectDamage
Status + shield        → ApplyShield
Status + real status   → ApplyStatus
Status + passive-buff  → StatusModifier
Extend existing status → EffectExtendStatusDuration
```

Damage Effect에 명중/틱 상태 payload가 함께 있으면 `EffectDamageStatus` 공용 지원이 구현되기 전에는 마이그레이션을 중단한다.

### 단계 4: 조합 노드 추가

- 대상/타이밍이 기본값과 다름 → `EffectTarget`
- 대상 상태 조건 → `ConditionStatus`
- 스킬 속성 보유 조건 → `ConditionSkillAttribute`
- 수명 → `EffectLifetime`
- prefab → `EffectVisual`
- 상태 수치 → 의미별 `Status*` 노드

### 단계 5: Choice 중복 제거

기존 Effect 행이 `requires_active_choice_id` 때문에 기본 Effect 전체를 복제한 행인지 검사한다.

- 숫자만 다름 → 복제 Effect를 만들지 말고 Choice 기능 노드로 이동
- 타이밍/대상/조건/결과 종류가 다름 → choice-gated Effect 그룹 유지
- base Effect를 대체함 → base operation의 `excludes_active_choice_id`와 대체 operation의 `requires_active_choice_id`를 명시

### 단계 6: Trigger 참조 보존

legacy `effect_id`를 Effect 그룹 `owner_id`로 그대로 유지한다. Trigger의 `triggered_effect_id`는 변경하지 않는 것을 기본으로 한다.

Trigger owner normalized node는 현재 금지한다.

### 단계 7: legacy 행 제거

하나의 effect를 node-owned effect로 만들었으면 같은 `effect_id`의 legacy 행을 동일 변경에서 제거한다.

현재 effect 파일은 monster별 파일이 아니라 skill kind 통합 파일이다. 한 몬스터 전환이 끝났다고 전체 `{kind}_skill_effects.csv` 파일을 삭제하면 안 된다. 해당 monster 행만 제거하고, 파일에 실제 행이 하나도 남지 않을 때만 파일 삭제를 별도 검토한다.

## 12. 작성 예시

### 12.1 단순 Choice 수치

```csv
"example-trait-damage","monster","Choice","example-trait-1","example-a","DamageModifier","DamageMultiplier","200",...
```

```csv
"example-trait-damage","monster","multiplier","float","1.25"
```

### 12.2 실제 상태 적용 Effect

```text
owner_id=example-on-hit-status
operation: ApplyStatus(status_id=shock)
target:    EffectTarget(target_selection=EventTarget, center_mode=EffectTarget)
lifetime:  EffectLifetime(duration_seconds=4)
```

Trigger가 필요하면 `triggered_effect_id=example-on-hit-status`로 연결한다.

### 12.3 아군 조건 오라

```text
owner_id=example-shielded-party-damage
operation: StatusModifier
target:    EffectTarget(target_side=AllAllies)
condition: ConditionStatus(status_id=shield, target_side=AllAllies, min_stacks=1)
lifetime:  EffectLifetime(duration_seconds=0.5)
modifier:  StatusDamageBonusRate(bonus=0.12, attribute=Holy)
```

### 12.4 지연 추가 피해

```text
owner_id=example-second-wave
operation: EffectDamage(attribute=Holy, base_damage=28, spell_power_coefficient=1.2, damage_multiplier=0.6, radius=3)
target:    EffectTarget(target_shape=Circle, effect_timing=Delayed, delay_seconds=1)
visual:    EffectVisual(skill_effect_prefab_path=...)
```

## 13. 금지 사항

- legacy effect 한 행의 모든 컬럼을 `*-base` carrier 노드 하나로 복사하지 않는다.
- `EffectStatus(status_id=passive-buff)`를 신규 작성하지 않는다.
- 특성 조합마다 동일한 Effect 그룹을 복제하지 않는다.
- base skill 수치까지 무조건 Effect 노드로 이동하지 않는다.
- Trigger 행을 현재 지원되지 않는 `owner_kind=Trigger` 노드로 옮기지 않는다.
- 스키마 등록만 된 핸들러를 런타임 구현 완료로 간주하지 않는다.
- Damage + status tick을 서로 다른 OnCast Effect로 임의 분리하지 않는다.
- target 기본값을 의미 확인 없이 전부 복사하지 않는다.
- reference와 runtime CSV 값이 다를 때 값을 추측하지 않는다.
- 다른 몬스터의 수치를 Ariel에서 복사하지 않는다.

## 14. 호환성과 위험

### 14.1 반드시 보존할 것

- 기존 `effect_id`
- `skill_id`/`target_skill_id`
- sort order
- choice/passive gate
- target side/selection/shape/center
- effect timing과 delay
- apply-once와 cover-all
- 상태 merge/stack/duration 의미
- prefab asset path
- trigger 참조
- persistent Damage의 active duration과 tick interval

### 14.2 현재 구조 위험

- legacy effect와 node effect가 함께 로드되므로 중복 실행 방지가 구현자 책임이다.
- Effect 그룹에 operation이 두 개면 builder가 명확히 거부하지 않고 후속 operation이 정의를 덮을 수 있다. CSV 검증 스크립트로 정확히 1개를 강제해야 한다.
- operation 외 노드에 둔 choice/passive gate는 Effect 전체 gate가 되지 않는다.
- 현재 Trigger normalized node는 runtime 미지원이다.
- schema 등록과 runtime mapping은 별개다.
- 참고 문서는 의도, 현재 CSV는 현재 런타임 값이다. parity migration과 디자인 변경을 섞지 않는다.

## 15. 권장 구현 단계

### Phase 1: 현재 핸들러로 가능한 58행

- monster와 skill kind별로 작은 묶음으로 전환한다.
- 각 Effect 그룹을 만들고 동일 legacy 행을 제거한다.
- trigger 참조가 있는 묶음을 우선 smoke test한다.

### Phase 2: 기존 핸들러 Effect 지원 확장 16행

- `StatusAttackPowerBonus`
- `StatusElementDamageTakenBonus`
- `StatusDamageTakenBonus(incoming_skill_runtime_kinds)`

새 handler id를 만들기 전에 기존 handler schema와 effect composer를 확장한다.

### Phase 3: 신규 semantic handler 기반 22행

10.2의 신규 후보를 공용 코드에 구현하고, 각 handler마다 최소 한 개의 실제 effect 행을 대표 검증한다.

### Phase 4: monster별 effect 권한 제거

해당 monster의 legacy effect id 집합과 node operation owner id 집합이 완전히 일치할 때 그 monster 행을 effect CSV에서 모두 제거한다.

## 16. 수용 기준

### 16.1 CSV 구조

- 모든 node id가 유일하다.
- 모든 param의 node id가 존재한다.
- param key와 value type이 handler schema와 일치한다.
- type row를 제외한 모든 행에 `monster_id`가 있다.
- 각 Effect owner 그룹에 주 연산 handler가 정확히 1개다.
- Effect 그룹의 모든 노드가 같은 `owner_id`, `monster_id`, `target_skill_id`를 사용한다.

### 16.2 효과 권한

- legacy effect id와 node-owned effect id가 중복되지 않는다.
- 전환 대상 effect id 누락이 없다.
- trigger의 `triggered_effect_id`가 legacy 또는 node-owned effect 중 정확히 하나에 존재한다.
- Choice 수치 강화가 base Effect 복제품으로 남지 않는다.

### 16.3 코드 검증

- 신규/확장 handler가 schema에 등록된다.
- owner kind에 맞는 mapper 또는 effect composer가 실제 값을 소비한다.
- runtime definition까지 파라미터가 전달된다.
- executor/status runtime이 해당 값을 실제로 사용한다.
- schema-only handler가 실행 가능하다고 오판되지 않는다.

### 16.4 명령과 Unity 검증

Code Builder가 최소한 다음 근거를 남긴다.

- 전체 runtime skill CSV field-count 검사
- node/param 참조 검사
- Effect 그룹 operation-count 검사
- legacy/node effect id 중복 및 parity 검사
- trigger effect 참조 검사
- `git diff --check`
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`
- Unity-MCP `Pakuri/Validate CSV Source Data`
- Unity-MCP `Pakuri/InGame/Validate Skill Data`
- Unity console error/warning 확인

Unity Play Mode에서의 실제 피해, 상태, 타이밍, 시각 parity는 사용자 검증 영역이다.

## 17. 중단하고 사용자에게 확인할 조건

AI agent는 다음 경우 값을 추측하거나 범위를 넓히지 않는다.

- reference와 현재 CSV의 수치 또는 조건이 다르다.
- 필요한 status id가 현재 상태 런타임에 없다.
- 현재 handler로 의미를 보존할 수 없다.
- shared runtime/common handler 확장이 필요하다.
- Trigger owner normalized node가 필요하다.
- 새로운 CSV 컬럼이나 새로운 CSV 파일이 필요하다.
- Damage + status persistent zone처럼 lifecycle을 분리하면 의미가 달라진다.
- prefab/scene/asset path가 입력 범위에 없거나 실제 파일이 없다.

신규 handler가 필요하다는 결론에는 반드시 다음을 포함한다.

1. 표현하려는 실제 effect id와 legacy 필드
2. 재사용을 검토한 기존 handler
3. 기존 handler로 표현할 때 깨지는 의미
4. 제안 handler 이름과 파라미터
5. mapper/composer/runtime 소비 지점
6. 대표 검증 행과 수용 기준

## 18. 관련 보드 업데이트

실제 전환 구현은 다음 보드를 함께 갱신한다.

- 대상 몬스터의 `boards/MON/{MONSTER}_MONSTER.md`
- CSV 권한과 handler 변경을 기록하는 `boards/DATA/DATA_BLACKBOARD.md`
- 상태 modifier/runtime 의미가 추가되면 `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- trigger runtime 자체가 변경되면 해당 active combat board

루트 `BLACKBOARD.md`는 전역 상태나 라우팅 변경이 없는 한 갱신하지 않는다.
