# Trait/Master Node Function Guide

## Task title

Trait/Master node function guide

## Goals

- 현재 구현되어 있는 스킬 노드 기능을 `handler_id` 기준으로 정리한다.
- 다음 구현자가 스킬 강화와 마스터 스킬을 기존 통합 효과 복사 방식이 아니라 노드 단위로 분해할 수 있게 한다.
- `passive_skill_nodes.csv`의 `handler_id`와 `passive_skill_node_params.csv`의 `param_key`를 혼동하지 않도록 기준을 남긴다.

## Constraints

- 이 문서는 실제 확인한 CSV와 C# 매핑 코드에 근거한다.
- 새 런타임 기능이 아니라 문서화 작업이다.
- MSW-MCP는 사용하지 않는다.

## Role Owner

Designer

## Status

Complete

## Evidence

- CSV 노드 데이터:
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes`
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/passive/passive_skill_nodes.csv`
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/passive/passive_skill_node_params.csv`
- 스키마 등록:
  - `Pakuri/Assets/Scripts2/Data/Runtime/Model/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`
  - `RegisterKnownNodeSchemas`에서 노드 스키마와 허용 enum 파라미터를 등록한다.
- Effect 소유 노드 조립:
  - `Pakuri/Assets/Scripts2/Data/Runtime/Model/PakuriCsvRuntimeData.Build.cs`
  - `IsEffectOperationHandler`, `BuildEffectOwnedSkillEffectDefinition`, `ApplyEffectOwnedSkillEffectOperationNode`, `ApplyEffectOwnedSkillEffectNode`
- Choice/Skill 노드 런타임 매핑:
  - `Pakuri/Assets/Scripts2/InGame/Skill/SkillDataAdapters/InGameSkillDefinitionMapper.cs`
  - `ApplyNormalizedChoiceNode`, `MapSkillActionOp`
- 선택된 강화 노드 적용:
  - `Pakuri/Assets/Scripts2/InGame/Skill/SkillExecutionSnapshot.cs`
  - `ApplyNodeBackedChoiceDefinition`, `ApplyPlanAction`
- 스킬 적용 범위:
  - `Pakuri/Assets/Scripts2/InGame/Skill/SkillExecutionSystem.cs`
  - `AppliesToSkill`
- 멀티 이펙트의 상태 지속시간/보호막 보정:
  - `Pakuri/Assets/Scripts2/InGame/Skill/SkillMultiEffectExecutor.cs`
  - `ResolveStatusSpec`, `ResolveStatusEffectShieldAmount`

## Core concepts

### handler_id

`handler_id`는 노드가 어떤 기능을 실행할지 정하는 함수 ID다.

예:

- `DamageMultiplier`는 피해량 배율 보정 기능이다.
- `ApplyShield`는 보호막 부여 이펙트 기능이다.
- `EffectTarget`은 Effect 소유 노드의 대상 지정 기능이다.
- `StatusActionSpeedBonus`는 상태 효과에 행동속도 보너스를 붙이는 기능이다.

`passive_skill_nodes.csv`의 각 행은 하나의 노드이고, 그 노드의 기능은 `handler_id`로 결정된다.

### param_key

`param_key`는 특정 `handler_id`가 사용하는 입력값 이름이다.

예:

- `DamageMultiplier`의 `param_key=multiplier`는 피해량 배율 값이다.
- `EffectLifetime`의 `param_key=duration_seconds`는 지속시간 값이다.
- `EffectTarget`의 `param_key=target_side`, `target_selection`, `target_shape`는 대상 지정 입력값이다.
- `StatusActionSpeedBonus`의 `param_key=status_id`, `bonus`는 어떤 상태에 얼마의 행동속도 보너스를 줄지 정한다.

즉, `handler_id`는 기능이고 `param_key`는 그 기능에 들어가는 인자다.

### owner_kind

`owner_kind`는 노드가 어디에 붙는지 정한다.

- `Choice`: 스킬 강화나 마스터 선택지에 붙는 노드다. 기존 스킬/효과의 수치나 조건을 보정할 때 사용한다.
- `Effect`: 새 효과 단위를 만드는 노드다. 적용 대상, 시각효과, 조건, 지속시간, 실제 효과 연산을 함께 조립한다.
- `Skill`: 기본 스킬 자체에 붙는 실행 규칙 노드다.

강화/마스터를 쪼갤 때 가장 먼저 판단할 것은 "기존 것을 보정하는가"와 "새 Effect를 만드는가"다.

## Decomposition rules

### 1. 기존 값을 바꾸는 강화는 Choice 노드로 둔다

스킬 강화나 마스터가 기존 스킬의 피해량, 보호막량, 탄창, 쿨타임, 상태 보너스만 바꾼다면 `Choice` 소유 노드로 분해한다.

대표 노드:

- `DamageMultiplier`
- `ShieldAmountMultiplier`
- `CooldownMultiplier`
- `MagazineBonus`
- `ReloadTimeMultiplier`
- `PierceBonus`
- `RadiusMultiplier`
- `DurationBonus`
- `StatusActionSpeedBonus`
- `StatusDurationBonus`
- `StatusDamageBonusRate`
- `StatusDamageTakenBonus`

예: Ariel E의 보호막 강화는 기존 `ApplyShield` Effect를 복사하지 않고 `ShieldAmountMultiplier` Choice 노드로 보정한다.

### 2. 새 효과를 생성하면 Effect 노드 그룹으로 둔다

강화/마스터가 새 상태, 새 보호막, 새 피해, 새 장판, 새 지속 효과를 만든다면 `Effect` 소유 노드 그룹으로 분해한다.

Effect 그룹은 보통 아래처럼 구성한다.

1. 실제 효과 연산 노드 1개
   - `ApplyStatus`
   - `ApplyShield`
   - `StatusModifier`
   - `EffectDamage`
   - `EffectExtendStatusDuration`
2. 대상 지정 노드
   - `EffectTarget`
3. 필요하면 조건 노드
   - `ConditionStatus`
   - `ConditionSkillAttribute`
4. 필요하면 지속시간 노드
   - `EffectLifetime`
5. 필요하면 시각효과 노드
   - `EffectVisual`
6. `StatusModifier`라면 실제 상태 보정 노드
   - `StatusActionSpeedBonus`
   - `StatusDamageBonusRate`
   - `StatusShieldReceivedBonus`
   - `StatusDamageTakenBonus`
   - 기타 상태 보정 노드

### 3. target_* 파라미터 자체는 잘못이 아니다

`target_side`, `target_selection`, `target_shape`, `center_mode`, `visual_anchor_mode`는 `EffectTarget`의 정상 파라미터다.

문제는 강화/마스터를 기존 통합 효과 CSV에서 복사해 온 결과로, 단순 수치 보정이어야 할 선택지가 새 Effect처럼 둔갑하는 경우다. 대상 지정 파라미터가 있다는 사실만으로 오류는 아니지만, 해당 노드가 "새 효과 생성"이 아니라 "기존 효과 보정"이어야 한다면 잘못 분해된 것이다.

### 4. 상태 지속시간 보정은 StatusDurationBonus를 우선한다

특정 상태 효과의 지속시간을 늘리는 강화라면 `DurationBonus`보다 `StatusDurationBonus(status_id, bonus_seconds)`가 의도에 맞다.

예: `blessing` 상태의 지속시간을 늘리는 Ariel C/H 계열 강화는 `status_id=blessing`을 가진 `StatusDurationBonus`로 둔다.

### 5. 조합별 복사 Effect를 만들지 않는다

강화 조합마다 `MigratedToEffectBinding` 같은 통합 Effect를 복사해서 만들면 노드화의 의미가 사라진다.

예:

- 기본 효과: `ApplyShield`
- 강화 1: `ShieldAmountMultiplier`
- 마스터 1: `StatusDamageTakenBonus`

이렇게 분해해야 한다. 강화 조합마다 `ApplyShield + Target + Lifetime + Visual`을 다시 만든 별도 Effect를 만들지 않는다.

## Handler reference

### Choice modifier nodes

| handler_id | params | use |
| --- | --- | --- |
| `DamageMultiplier` | `multiplier` | 스킬 피해량 배율 보정 |
| `ShieldAmountMultiplier` | `multiplier` | 보호막량 배율 보정 |
| `CooldownMultiplier` | `multiplier` | 쿨타임 배율 보정 |
| `MagazineBonus` | `bonus` | 탄창 수 증가 |
| `ReloadTimeMultiplier` | `multiplier` | 재장전 시간 배율 보정 |
| `PierceBonus` | `bonus` | 관통 수 증가 |
| `RadiusMultiplier` | `multiplier` | 반경 배율 보정 |
| `DurationBonus` | `bonus_seconds` | 일반 지속시간 증가 |
| `HitTargetCountBonus` | `bonus` | 타격 대상 수 증가 |
| `CountStatusDamageMultiplier` | `status_id`, `target_side`, `amount_per_count` | 상태 수량 기반 피해 배율 보정 |
| `StatusActionSpeedBonus` | `status_id`, `bonus` | 특정 상태가 주는 행동속도 보너스 |
| `StatusDurationBonus` | `status_id`, `bonus_seconds` | 특정 상태 지속시간 증가 |
| `StatusDamageBonusRate` | `attribute`, `bonus` | 특정 속성 피해 보너스 |
| `StatusShieldReceivedBonus` | `bonus` | 받는 보호막량 보너스 |
| `StatusAilmentResistanceBonus` | `bonus` | 상태이상 저항 보너스 |
| `StatusDamageTakenBonus` | `bonus` | 받는 피해 증가/감소 보정 |
| `StatusConditionalDamageTakenBonus` | `source_status_id`, `bonus` | 특정 상태 조건부 받는 피해 보정 |
| `StatusElementDamageTakenBonus` | `bonus` | 속성 피해 받는 정도 보정 |
| `StatusCriticalDamageTakenBonus` | `bonus` | 치명 피해 받는 정도 보정 |

### Effect operation nodes

| handler_id | params | use |
| --- | --- | --- |
| `ApplyStatus` | `status_id` | 상태 효과를 적용한다 |
| `ApplyShield` | `base_damage`, `spell_power_coefficient` | 보호막을 부여한다 |
| `StatusModifier` | none | 상태 보정 효과 컨테이너를 만든다 |
| `EffectDamage` | `attribute`, `base_damage`, `spell_power_coefficient`, `damage_multiplier`, `radius` | Effect 소유 피해를 만든다 |
| `EffectExtendStatusDuration` | `status_id` | 기존 상태 지속시간을 연장한다 |

### Effect assembler nodes

| handler_id | params | use |
| --- | --- | --- |
| `EffectTarget` | `target_side`, `target_selection`, `target_shape`, `center_mode`, `visual_anchor_mode`, `effect_timing`, `delay_seconds`, `apply_once` | Effect의 대상과 적용 타이밍을 정한다 |
| `EffectVisual` | `skill_effect_prefab_path` | Effect 시각효과 프리팹을 정한다 |
| `ConditionStatus` | `status_id`, `min_stacks`, `target_side`, `source_skill_id` | 특정 상태 조건을 요구한다 |
| `ConditionSkillAttribute` | `attribute` | 특정 스킬 속성 조건을 요구한다 |
| `EffectLifetime` | `duration_seconds` | Effect 또는 상태 지속시간을 정한다 |

### Effect status modifier nodes

| handler_id | params | use |
| --- | --- | --- |
| `StatusActionSpeedBonus` | `status_id`, `bonus` | 상태 효과에 행동속도 보너스를 붙인다 |
| `StatusSpellPowerBonus` | `bonus` | 상태 효과에 주문력 보너스를 붙인다 |
| `StatusDamageBonusRate` | `attribute`, `bonus` | 상태 효과에 속성 피해 보너스를 붙인다 |
| `StatusShieldReceivedBonus` | `bonus` | 상태 효과에 받는 보호막량 보너스를 붙인다 |
| `StatusDamageTakenBonus` | `bonus` | 상태 효과에 받는 피해 보정을 붙인다 |
| `StatusFlatElementResistReduction` | `attribute`, `bonus` | 상태 효과에 속성 저항 감소를 붙인다 |
| `StatusCriticalChanceBonus` | `bonus` | 상태 효과에 치명 확률 보너스를 붙인다 |

### Skill/base and advanced nodes

| handler_id | params | use |
| --- | --- | --- |
| `TargetHealthRatioCondition` | `threshold`, `reject_if_missing_target` | 대상 체력 비율 조건 |
| `TargetHealthRatioThresholdBonus` | `threshold_bonus` | 처형 체력 기준 보정 |
| `ExecuteDamageMultiplier` | `multiplier`, `threshold_source` | 처형 피해 배율 |
| `ExecuteCritChanceBonus` | `crit_chance_bonus` | 처형 치명 확률 보너스 |
| `CooldownRefund` | `ratio` | 쿨타임 반환 |
| `CooldownRefundBonus` | `ratio_bonus` | 쿨타임 반환량 보정 |
| `CooldownReset` | `requires_execute` | 조건부 쿨타임 초기화 |
| `TargetPredicateDamageMultiplier` | `predicate`, `multiplier` | 대상 조건 기반 피해 배율 |
| `AdditionalDamage` | `target`, `attribute`, `chance`, `multiplier` | 추가 피해 |
| `EveryNthHitChainDamage` | `hit_count`, `radius`, `max_targets`, `attribute`, `multiplier` | N번째 타격 연쇄 피해 |
| `RepeatPerTarget` | `repeat_count`, `repeat_interval_seconds`, `repeat_damage_multiplier` | 대상별 반복 타격 |
| `TargetStatusCritBonus` | `status_id`, `min_stacks`, `crit_chance_bonus` | 대상 상태 조건부 치명 보너스 |
| `RedistributeConsumedStatus` | `status_id`, `radius`, `target_count`, `ratio` | 소모 상태 재분배 |

## Examples

### Ariel J after-E action speed

목표:

- 스킬 사용 이후 모든 아군에게 행동속도 15%를 부여한다.
- 효과 Lifetime은 5초다.
- Trait 1은 이 행동속도 보너스를 추가로 올린다.

분해:

- 기본 Effect: `ariel-j-after-e-action-speed`
  - `StatusModifier`
  - `EffectTarget`
    - 모든 아군 대상
    - 전장/소유자 기준 대상 지정
  - `EffectLifetime(duration_seconds=5)`
  - `StatusActionSpeedBonus(status_id=..., bonus=0.15)`
- Trait 1 Choice:
  - `ariel-j-trait-1-after-e-action-speed-bonus`
  - `StatusActionSpeedBonus(status_id=..., bonus=0.07)`

판단:

- 대상 지정과 지속시간은 기본 Effect가 담당한다.
- 강화는 새 Effect를 복사하지 않고 보너스 수치만 Choice 노드로 더한다.

### Ariel E shield

목표:

- 기본 스킬은 보호막을 부여한다.
- Trait 2는 보호막량을 30% 증가시킨다.
- Master 2는 피해량 70% 증가와 보호막량 30% 감소 같은 수치 보정을 한다.

분해:

- 기본 Effect: `ariel-e-shield-base`
  - `ApplyShield(base_damage=50, spell_power_coefficient=1.6)`
  - `EffectTarget`
- Trait 2 Choice:
  - `ShieldAmountMultiplier(multiplier=1.3)`
- Master 2 Choice:
  - `DamageMultiplier(multiplier=1.7)`
  - `ShieldAmountMultiplier(multiplier=0.7)`

판단:

- 보호막 Effect를 강화별로 복사하지 않는다.
- 강화/마스터는 기존 보호막량과 피해량을 보정하는 Choice 노드로 둔다.

### Ariel C blessing duration

목표:

- 기본 Effect가 `blessing` 상태를 부여한다.
- Trait 2는 `blessing`의 행동속도 보너스를 올린다.
- Trait 3은 `blessing` 지속시간을 2초 늘린다.

분해:

- 기본 Effect:
  - `ApplyStatus(status_id=blessing)`
  - `EffectTarget`
  - `EffectLifetime`
- Trait 2 Choice:
  - `StatusActionSpeedBonus(status_id=blessing, bonus=0.06)`
- Trait 3 Choice:
  - `StatusDurationBonus(status_id=blessing, bonus_seconds=2)`

판단:

- 특정 상태의 지속시간 변경이므로 `StatusDurationBonus`가 맞다.
- 조합별 `blessing` Effect 복사본을 만들지 않는다.

### Ariel G shielded holy condition

목표:

- 보호막이 있는 아군에게 신성 피해 보너스를 부여한다.

분해:

- Effect:
  - `StatusModifier`
  - `ConditionStatus(status_id=shield, target_side=AllAllies, min_stacks=1)`
  - `StatusDamageBonusRate(attribute=Holy, bonus=0.10)`
  - 필요하면 `EffectTarget`, `EffectLifetime`

판단:

- "보호막이 있는 대상" 조건은 `ConditionStatus`로 둔다.
- 실제 보너스는 `StatusDamageBonusRate`로 둔다.

## Current implemented handler inventory

아래 목록은 현재 CSV에서 확인된 `handler_id` 기준이다.

| handler_id | owner_kind | params |
| --- | --- | --- |
| `AdditionalDamage` | `Choice` | `attribute`, `chance`, `multiplier`, `target` |
| `ApplyShield` | `Effect` | `base_damage`, `spell_power_coefficient` |
| `ApplyStatus` | `Effect` | `status_id` |
| `ConditionSkillAttribute` | `Effect` | `attribute` |
| `ConditionStatus` | `Effect` | `min_stacks`, `source_skill_id`, `status_id`, `target_side` |
| `CooldownMultiplier` | `Choice` | `multiplier` |
| `CooldownRefund` | `Skill` | `ratio` |
| `CooldownRefundBonus` | `Choice` | `ratio_bonus` |
| `CooldownReset` | `Choice` | `requires_execute` |
| `CountStatusDamageMultiplier` | `Choice` | `amount_per_count`, `status_id`, `target_side` |
| `DamageMultiplier` | `Choice` | `multiplier` |
| `DurationBonus` | `Choice` | `bonus_seconds` |
| `EffectDamage` | `Effect` | `attribute`, `base_damage`, `damage_multiplier`, `radius`, `spell_power_coefficient` |
| `EffectExtendStatusDuration` | `Effect` | `status_id` |
| `EffectLifetime` | `Effect` | `duration_seconds` |
| `EffectTarget` | `Effect` | `apply_once`, `center_mode`, `delay_seconds`, `effect_timing`, `target_selection`, `target_shape`, `target_side`, `visual_anchor_mode` |
| `EffectVisual` | `Effect` | `skill_effect_prefab_path` |
| `EveryNthHitChainDamage` | `Choice` | `attribute`, `hit_count`, `max_targets`, `multiplier`, `radius` |
| `ExecuteCritChanceBonus` | `Choice` | `crit_chance_bonus` |
| `ExecuteDamageMultiplier` | `Skill` | `multiplier`, `threshold_source` |
| `HitTargetCountBonus` | `Choice` | `bonus` |
| `MagazineBonus` | `Choice` | `bonus` |
| `PierceBonus` | `Choice` | `bonus` |
| `RadiusMultiplier` | `Choice` | `multiplier` |
| `RedistributeConsumedStatus` | `Choice` | `radius`, `ratio`, `status_id`, `target_count` |
| `ReloadTimeMultiplier` | `Choice` | `multiplier` |
| `RepeatPerTarget` | `Choice` | `repeat_count`, `repeat_damage_multiplier`, `repeat_interval_seconds` |
| `ShieldAmountMultiplier` | `Choice` | `multiplier` |
| `StatusActionSpeedBonus` | `Choice`, `Effect` | `bonus`, `status_id` |
| `StatusAilmentResistanceBonus` | `Choice` | `bonus` |
| `StatusConditionalDamageTakenBonus` | `Choice` | `bonus`, `source_status_id` |
| `StatusCriticalChanceBonus` | `Effect` | `bonus` |
| `StatusCriticalDamageTakenBonus` | `Choice` | `bonus` |
| `StatusDamageBonusRate` | `Choice`, `Effect` | `attribute`, `bonus` |
| `StatusDamageTakenBonus` | `Choice`, `Effect` | `bonus` |
| `StatusDurationBonus` | `Choice` | `bonus_seconds`, `status_id` |
| `StatusElementDamageTakenBonus` | `Choice` | `bonus` |
| `StatusFlatElementResistReduction` | `Effect` | `attribute`, `bonus` |
| `StatusModifier` | `Effect` | none |
| `StatusShieldReceivedBonus` | `Choice`, `Effect` | `bonus` |
| `StatusSpellPowerBonus` | `Effect` | `bonus` |
| `TargetHealthRatioCondition` | `Skill` | `reject_if_missing_target`, `threshold` |
| `TargetHealthRatioThresholdBonus` | `Choice` | `threshold_bonus` |
| `TargetPredicateDamageMultiplier` | `Choice`, `Skill` | `multiplier`, `predicate` |
| `TargetStatusCritBonus` | `Choice` | `crit_chance_bonus`, `min_stacks`, `status_id` |

## Implementation checklist

1. 먼저 강화/마스터 설명을 "수치 보정", "조건 추가", "새 효과 생성"으로 나눈다.
2. 수치 보정이면 `Choice` 노드로 둔다.
3. 새 효과 생성이면 `Effect` 노드 그룹으로 둔다.
4. Effect 그룹에는 실제 연산 노드와 `EffectTarget`을 분리해서 둔다.
5. 상태 지속시간 보정은 특정 `status_id`가 있으면 `StatusDurationBonus`를 우선 검토한다.
6. 특정 상태가 있을 때만 적용되는 효과는 `ConditionStatus`를 사용한다.
7. 특정 속성 스킬일 때만 적용되는 효과는 `ConditionSkillAttribute`를 사용한다.
8. 강화 조합마다 기존 Effect를 복사하지 않는다.
9. `passive_skill_nodes.csv`에 노드를 추가한 뒤, 필요한 값만 `passive_skill_node_params.csv`에 추가한다.
10. 추가한 `param_key`가 해당 `handler_id`에서 실제로 읽히는지 C# 매핑 코드로 확인한다.

## History

- 2026-07-09: 현재 runtime node CSV와 C# 매핑 코드를 근거로 Trait/Master 노드 기능 가이드를 작성했다.
