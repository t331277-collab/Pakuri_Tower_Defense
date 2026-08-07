# 파수꾼 시너지·유물 구현 Handoff

## 1. 문서 상태

- 작성일: 2026-08-07
- 설계 롤: Designer
- 구현 롤: Code Builder
- 선택 트랙: Gameplay Design, Implementation Design, Builder Handoff
- 상태: 설계 완료, 구현 대기
- 범위: 파수꾼 2/4/6/8 시너지, 개별 유물 10종, 방어력·방어막·피해 반사·최종 피해 감소 공통 계약
- 제외: 이 문서 작성 단계의 C#·CSV·Scene 구현, Git 커밋, Unity Play Mode 검증

## 2. 목표

1. 파수꾼 시너지와 유물 효과를 기존 Artifact Effect, Skill Node, Trigger 경로로 구현한다.
2. Ariel-B 특성 4와 Master 2가 사용하는 `OnShieldExpire`, `OnShieldAbsorb`, `ShieldAbsorbedAmount`, `ApplyDamage`를 재사용한다.
3. `artifact_effects.csv.recipient_scope`를 유일한 개별 유물 적용 범위로 사용한다.
4. `모든 아군`이라고 명시된 개별 유물만 `AllAllies`, 나머지는 `Owner`로 배포한다.
5. 대상 측 최종 피해 감소를 `DamageCalculator`의 최종 피해 구간에서 공격자 측 최종 피해 증가와 곱연산한다.
6. 수치와 조건은 CSV가 소유하고 C#에는 파수꾼 Effect ID 분기를 넣지 않는다.

## 3. 확인한 현재 저장소 근거

### 3.1 이미 존재하는 기반

- `UnitDefenseStats`는 `Physical`, `Fire`, `Lightning`, `Ice`, `Darkness`, `Holy` 여섯 방어력을 보관한다.
- `DamageCalculator.CalculateFinalDamage`는 피해 속성에 대응하는 방어력을 읽고 `100 / (100 + defense)`를 적용한다.
- `DamageCalculator`는 이후 주는 피해, 받는 피해, 치명타, `FinalDamageModifier`, `CriticalFinalDamageModifier`를 순서대로 곱한다.
- `StatusCombatRules.IncomingDamageMultiplier`와 `PassiveIncomingDamageMultiplier`는 일반 받는 피해 증감을 이미 제공한다.
- `StatusCombatRules.ElementResistMultiplier`와 `FlatElementResistReduction`은 속성 방어력 감소를 이미 제공한다.
- `ArtifactEffectRecipient.Owner`와 `ArtifactSynergyManager`의 Owner/AllAllies 배포가 이미 구현돼 있다.
- 개별 유물과 시너지 Effect는 `ActiveArtifactEffectNames`에 배포되고 `SkillExecutionRules`와 `SkillTrigger`가 Node/Reaction을 소비한다.
- `OnShieldAbsorb`, `OnShieldExpire`, `ShieldAppliedAmount`, `ShieldRemainingAmount`, `ShieldAbsorbedAmount`가 이미 존재한다.
- `InGameCombatManager.ApplyDamageToResources`는 보호막별 실제 흡수량을 `ShieldAbsorptionRecord`로 기록한다.
- Trigger 피해는 `AttackRule.IsTrigger=true`로 실행되어 추가 Trigger 재귀를 차단한다.

### 3.2 직접 재사용할 Ariel-B 계약

- `ariel-b-trait4-shield-expire`는 `OnShieldExpire`에서 `ShieldAppliedAmount × 0.60` 신성 범위 피해를 실행한다.
- `ariel-b-master2-shield-absorb-reflect`는 `OnShieldAbsorb`에서 `ShieldAbsorbedAmount × 0.35` 신성 피해를 사건 공격자에게 실행한다.
- 파수꾼 반사도 같은 `ApplyDamage(Holy) + ShieldAbsorbedAmount` 계약을 사용한다.
- 파수꾼의 종도 같은 `OnShieldExpire` 사건을 사용하되 결과만 `ApplyStatus(holy-exposure)`로 바꾼다.

### 3.3 현재 비어 있거나 부족한 계약

- `artifact_effects.csv`와 `artifact_synergy_effects.csv`에는 파수꾼 Effect 헤더가 있지만 Artifact graph/trigger CSV에는 파수꾼 실행 행이 없다.
- 현재 `artifact_synergies.csv` 파수꾼 수치는 `15/25/40/60%`로, 이번 확정값 `5/10/15/20%`와 다르다.
- 파수꾼 개별 Effect 10개가 현재 모두 `AllAllies`다. 이번 범위 규칙상 `unbreakable-promise-effect`는 `Owner`여야 한다.
- 기본 방어력 증가와 고정 방어력 증가는 Artifact Node로 조회할 공통 연산이 없다.
- 대상 측 최종 피해 감소를 별도 조회하는 계약이 없다.
- 회복 또는 방어막 수령 사건, 보호막 파괴 전용 사건, 보스전 시작 사건이 없다.
- 현재 보호막 흡수·종료 Trigger context의 `EventSource`는 방어막 수혜자가 아니라 방어막 시전자다. 이 상태에서는 다른 아군이 준 방어막을 가진 수혜자의 Artifact `event_source_scope=owner`가 일치하지 않는다.
- `ApplyShield` Trigger 결과는 고정값과 주문력 계수만 지원하며 대상 최대 체력 비율을 지원하지 않는다.
- `StatusDurationBonus("shield")`는 상태 데이터에는 합성되지만 현재 Shield의 `PreparedDuration` 계산에 반영되지 않는다.
- `CooldownMultiplier`는 고정 쿨타임 길이 변경이다. 방어막 보유 중에만 동적으로 작동하는 `쿨타임 충전 속도`와 의미가 다르다.
- `ArtifactUI.PrepareChoices`는 현재 `spirit-contract`, `executioner`, `chosen-one`만 보상 후보로 허용한다.

## 4. 핵심 구현 원칙

1. 새 파수꾼 전용 manager, passive definition, trigger executor를 만들지 않는다.
2. 기존 Effect 헤더, Artifact graph/trigger CSV, 공통 Node 정의를 사용한다.
3. 방어·최종 피해·쿨타임 충전처럼 스킬 snapshot이 아닌 대상 전투 상태를 읽는 값만 공통 Artifact 전투 조회를 추가한다.
4. 각 파티원에게 `AllAllies` Effect ID가 하나씩 배포되므로 Trigger 결과 대상은 다시 `AllAllies`로 잡지 않는다. 각 수신자가 자신의 사건에 `Self/Owner`로 반응한다.
5. 반사 피해는 실제 보호막 흡수량을 원시 신성 피해로 사용하고 기존 피해 계산을 다시 통과한다.
6. 같은 원천 유닛의 반사율은 합산해 한 번의 Trigger 피해로 실행한다. 원천 유닛이 다르면 원천별로 각각 실행한다.
7. Stage 초기화는 기존 `ClearActiveEffects`, 상태 초기화, `PrepareStage`, `BeginPlayerCombat` 순서를 유지한다.
8. 보호막 사건의 Artifact owner는 방어막 시전자가 아니라 실제 방어막 수혜자다. Ariel의 source-owned 반응은 기존 `SourceModel` 인자를 계속 사용한다.

## 5. 파수꾼 시너지 데이터 계약

현재 시너지 단계는 달성한 이전 단계를 모두 누적 배포한다. 설명 수치를 각 단계에 그대로 반복하면 8시너지에서 방어율 `50%`, 고정 방어력 `63`이 되어 잘못된다. 따라서 표시 설명은 총합, Node 값은 단계별 증가분으로 기록한다.

| 보유 수 | Effect ID | CSV Node 증가분 | 해당 단계 총합 |
|---:|---|---|---|
| 2 | `sentinel-level-1-defense-resistance` | 방어 보너스율 `+0.05`, 고정 방어력 `+8` | `+5%`, `+8` |
| 4 | `sentinel-level-2-defense-resistance` | 방어 보너스율 `+0.05`, 고정 방어력 `+4` | `+10%`, `+12` |
| 6 | `sentinel-level-3-defense-resistance-shield-reduction` | 방어 보너스율 `+0.05`, 고정 방어력 `+6` | `+15%`, `+18` |
| 8 | `sentinel-level-4-defense-resistance` | 방어 보너스율 `+0.05`, 고정 방어력 `+7` | `+20%`, `+25` |

### 5.1 방어력 합성식

모든 여섯 속성에 같은 보정값을 적용한다. `Physical`도 속성 방어 체계의 한 항목으로 포함한다.

```text
baseDefense = target.Defenses.Get(attribute)
artifactDefenseRate = 파수꾼 단계 bonus_rate 합
artifactFlatDefense = 파수꾼 단계 flat_bonus 합

defense = baseDefense
defense *= target.SkillState.PassiveDefenseMultiplier(attribute)
defense *= 1 + artifactDefenseRate
defense += artifactFlatDefense
defense *= StatusCombatRules.ElementResistMultiplier(target, attribute)
defense -= StatusCombatRules.FlatElementResistReduction(target, attribute)

damage *= 100 / max(0.01, 100 + defense)
```

- 기본 방어력 0인 플레이어 몬스터도 2시너지에서 고정 방어력 8을 얻는다.
- 시너지 보너스율은 단계 간 합산한다. `0.05 × 4 = 0.20`이다.
- 기존 Passive DefenseUp은 별도 배율로 곱한다.
- 적의 속성 저항 감소는 파수꾼 보정까지 포함한 현재 방어력에 적용한다.

### 5.2 반사 단계

| 보유 수 | Effect ID | 실행값 |
|---:|---|---|
| 4 | `sentinel-level-2-shield-reflection` | `OnShieldAbsorb`, `ShieldAbsorbedAmount × 0.25`, `Holy`, 사건 공격자 단일 대상 |
| 8 | `sentinel-level-4-shield-reflection` | 추가 `ShieldAbsorbedAmount × 0.20`, `Holy`, 사건 공격자 단일 대상 |

8시너지에서는 누적 배포된 25%와 추가 20%가 합쳐져 총 45%가 된다. Ariel-B Master 2, 반사 거울, 파수꾼 단계 반사는 `OnShieldAbsorb + ShieldAbsorbedAmount` 계약이 같은 경우 원천 유닛별로 반사율을 합산한 뒤 한 번의 Trigger 피해로 실행한다. 서로 다른 유닛이 원천이면 원천별 피해를 유지한다. Trigger 피해는 다시 반사를 발행하지 않는다.

### 5.3 6시너지 최종 피해 감소

기존 `sentinel-level-3-defense-resistance-shield-reduction`에는 방어 Node만 둔다. 조건이 방어 증가 전체에 번지는 것을 막기 위해 신규 Effect 헤더를 같은 단계에 추가한다.

- Effect ID: `sentinel-level-3-shield-final-damage-reduction`
- 적용 범위: `AllAllies`
- Node:
  - `RequiredSourceStatus(shield, 1)`
  - `FinalDamageTakenMultiplier(0.90)`

피해 계산 시작 시 대상의 총 보호막이 0보다 크면 현재 공격이 보호막을 모두 파괴하더라도 그 공격에는 `0.90`이 적용된다.

## 6. 개별 유물 계약

| Artifact ID | Effect ID | recipient_scope | 구현 방식 |
|---|---|---|---|
| `pure-white-shield` | `pure-white-shield-effect` | `AllAllies` | `CombatStart`; Self에게 최대 체력 `0.12` 방어막; 지속 `9999초`로 Stage 종료 전까지 유지 |
| `sanctuary-fragment` | `sanctuary-fragment-effect` | `AllAllies` | 기존 `RequiredSourceStatus(shield,1)` + `ConditionSkillAttribute(Holy)` + `DamageMultiplier(1.18)` |
| `unbreakable-promise` | `unbreakable-promise-effect` | `Owner` | 신규 `OnShieldBreak`; Self에게 2초 상태; 기존 `StatusDamageTakenBonus(-0.30)` |
| `guardians-censer` | `guardians-censer-effect` | `AllAllies` | 기존 `StatusDurationBonus(shield, 2)`; Shield `PreparedDuration`가 이 값을 한 번 소비하도록 공통 수정 |
| `blue-cross` | `blue-cross-effect` | `AllAllies` | 신규 `OnHealOrShieldReceived`; Self에게 4초 `StatusActionSpeedBonus(0.10, action-speed-up)` |
| `pilgrims-cloak` | `pilgrims-cloak-effect` | `AllAllies` | 신규 `BossCombatStart`; Self에게 최대 체력 `0.50` 방어막; 지속 10초 |
| `faded-gate` | `faded-gate-effect` | `AllAllies` | `CombatStart`; Self에게 6초 상태; 기존 `StatusDamageTakenBonus(-0.20)` |
| `reflection-mirror` | `reflection-mirror-effect` | `AllAllies` | Ariel-B Master 2 재사용; `OnShieldAbsorb`, 흡수량 `×0.20`, `Holy`, 사건 공격자 |
| `prayer-stone` | `prayer-stone-effect` | `AllAllies` | `RequiredSourceStatus(shield,1)` + 신규 `CooldownChargeSpeedBonus(0.12)` |
| `sentinels-bell` | `sentinels-bell-effect` | `AllAllies` | `OnShieldExpire`; 방어막 소유자 위치 반경 3의 적에게 `holy-exposure` 1스택, 2초 |

### 6.1 recipient_scope 확정

- `unbreakable-promise-effect`만 현재 `AllAllies`에서 `Owner`로 변경한다.
- 나머지 9개는 사용자 문구에 `모든 아군`이 명시됐으므로 `AllAllies`를 유지한다.
- 설명 문자열을 런타임에서 해석하지 않는다.
- 시너지 Effect는 파티 시너지 자체이므로 계속 `AllAllies`다.

### 6.2 일반 받는 피해 감소와 최종 피해 감소 구분

- `깨지지 않는 약속`, `빛바랜 성문`: 기존 일반 받는 피해 배율을 사용한다.
- 파수꾼 6시너지: 명시적으로 최종 피해 감소이므로 신규 대상 측 최종 피해 배율을 사용한다.
- 같은 공격에 둘이 함께 있으면 일반 받는 피해 구간과 최종 피해 구간에서 각각 곱한다.

## 7. 신규 공통 Node 계약

기존 `skill_node_definitions.csv`와 `skill_node_definition_params.csv`에만 추가한다. 파수꾼 전용 Node CSV는 만들지 않는다.

| Node | 인자 | 의미 |
|---|---|---|
| `DefenseModifier` | `bonus_rate: float`, `flat_bonus: float` | 대상의 여섯 방어력에 적용할 합산 보너스율과 고정 보너스 |
| `FinalDamageTakenMultiplier` | `multiplier: float` | 대상이 받는 최종 피해 배율. 10% 감소는 `0.90` |
| `CooldownChargeSpeedBonus` | `bonus: float` | 쿨다운 감소 속도 보너스. 12% 증가는 `0.12` |

`ApplyShield`에는 새 Node를 만들지 않고 선택 인자 하나를 추가한다.

- `target_max_health_ratio: float`, 기본값 `0`
- 최종 방어막량: 기존 고정/능력치 방어막량 + 대상 최대 체력 × 비율
- 대상마다 최대 체력이 다르므로 `BuffSkillExecutor`의 대상 순회 안에서 계산한다.
- 이후 기존 `StatusCombatRules.ShieldReceivedMultiplier(target)`를 그대로 통과한다.

전용 연산 타입은 기존 `SkillNodeModifiers.cs`에 둔다.

- `DefenseModifierOp`
- `FinalDamageTakenMultiplierOp`
- `CooldownChargeSpeedBonusOp`

스킬 snapshot에 적용할 `SkillActionOpKind`로 위장하지 않는다. 활성 Artifact Effect의 Node를 대상 전투 계산 시 조회한다.

## 8. Artifact 전투 조회 경계

신규 `ArtifactCombatRules` 한 곳에서 현재 유닛의 `ActiveArtifactEffectNames`를 순회한다.

책임:

- 개별 `ArtifactEffectDefinition`과 `ArtifactSynergyEffectDefinition`의 Node를 모두 읽는다.
- 기존 `RequiredSourceStatus` 조건을 대상 자신에게 평가한다.
- 방어 보너스율은 합산한다.
- 고정 방어력은 합산한다.
- 최종 피해 배율은 곱한다.
- 쿨타임 충전 속도 보너스는 합산 후 `1 + bonus`로 반환한다.

공개할 최소 조회:

```text
DefenseBonusRate(target)
FlatDefenseBonus(target)
FinalDamageTakenMultiplier(target)
CooldownChargeMultiplier(target)
ConditionsMatch(nodes, owner, skill)
```

`SkillExecutionRules`의 기존 private `ArtifactConditionsMatch`는 이 공통 조건 판정으로 옮겨 중복 구현을 피한다.

## 9. 신규 공통 Trigger 사건

### `OnShieldBreak`

- 피해 흡수로 `RemainingShieldAmount`가 0이 된 보호막에만 발행한다.
- 자연 시간 만료에는 발행하지 않는다.
- 기존 `OnShieldExpire`는 현재처럼 파괴와 시간 만료 모두에서 발행해 Ariel-B 특성 4와 파수꾼의 종 의미를 유지한다.
- 깨지지 않는 약속만 `OnShieldBreak`를 구독한다.
- `OnShieldAbsorb`, `OnShieldExpire`, `OnShieldBreak`의 Artifact/Passive owner 판정용 `EventSource`는 `shieldTarget`으로 전달한다.
- Ariel-B의 source-owned Trigger는 `ExecuteSourceOwnedTriggers`에 별도로 전달되는 방어막 시전자를 계속 사용하므로 동작을 유지한다.

### `OnHealOrShieldReceived`

- 실제 회복량이 0보다 클 때 발행한다.
- 적용 전후 총 방어막 증가량이 0보다 클 때 발행한다.
- 최대 체력 상태의 무효 회복, 더 낮은 `TakeHighest` 방어막 재적용은 발행하지 않는다.
- 사건 owner는 효과를 실제 받은 대상이다. Trigger의 `event_source_scope=owner`로 다른 아군의 수령 사건에 중복 반응하지 않는다.

### `BossCombatStart`

- `StageManager.SelectBossRows`와 `IsBossEncounter` 결과로 현재 encounter에 보스가 하나 이상 있을 때만 발행한다.
- Artifact Effect 배포 뒤, 적 스폰 전, 플레이어마다 한 번 발행한다.
- `StageManager`는 현재 Day/encounter 조회와 `SelectBossRows`를 `BeginPlayerCombat`보다 먼저 수행해야 한다.
- 일반 `CombatStart`는 모든 Stage에서 기존처럼 한 번 발행한다.

## 10. 최종 피해 곱연산 계약

대상 측 최종 피해 감소는 일반 `IncomingDamageMultiplier`에 섞지 않는다.

```text
damage
-> 방어력
-> 주는 피해 배율
-> 일반 받는 피해 배율
-> 치명타 피해
-> attackRule.FinalDamageModifier
-> 치명타면 attackRule.CriticalFinalDamageModifier
-> ArtifactCombatRules.FinalDamageTakenMultiplier(target)
-> 한 번만 Mathf.Round
-> 보호막/HP 차감
```

예시:

```text
최종 피해 증가 +15% = 1.15
최종 피해 감소 10% = 0.90
합성 배율 = 1.15 × 0.90 = 1.035
```

합산으로 `+5%` 처리하지 않는다. 모든 최종 피해 증가와 감소는 각자의 배율을 곱한다.

## 11. 쿨타임 충전 속도 계약

기도석은 기존 `CooldownMultiplier(0.88)`로 바꾸지 않는다.

```text
actionDeltaTime = deltaTime × ActionSpeedMultiplier(owner)
cooldownDeltaTime = actionDeltaTime × ArtifactCombatRules.CooldownChargeMultiplier(owner)

CooldownRemaining -= cooldownDeltaTime
CastRemaining -= actionDeltaTime
TickRemaining -= actionDeltaTime
```

- 방어막이 있는 동안만 `1.12`가 된다.
- 방어막이 사라진 다음 프레임부터 `1.0`으로 돌아간다.
- 시전시간, 공격 주기, 재장전 시간에는 기도석 보너스를 적용하지 않는다.
- 행동속도와 쿨타임 충전 속도는 서로 곱한다.

## 12. 데이터 작성 대상

### 기존 행 수정

- `artifact_synergies.csv`
  - 파수꾼 2/4/6/8 설명을 이번 확정 수치 `5/10/15/20%`, `8/12/18/25`로 변경
- `artifacts.csv`
  - 사용자 확정 문구로 10개 설명 정리
  - 순백 방패의 Stage 지속, 순례자 망토의 10초를 명시
- `artifact_effects.csv`
  - `unbreakable-promise-effect.recipient_scope = Owner`
  - 나머지 파수꾼 9개는 `AllAllies`

### Effect 헤더

- 기존 파수꾼 개별 Effect 10개 유지
- 기존 시너지 Effect 6개 유지
- `sentinel-level-3-shield-final-damage-reduction` 헤더 1개 추가

### 실제 실행 데이터

- `skill_graph_nodes_artifact.csv`: 파수꾼 시너지와 개별 유물 Node 작성
- `artifact_skill_triger.csv`: CombatStart, BossCombatStart, OnShieldAbsorb, OnShieldExpire, OnShieldBreak, OnHealOrShieldReceived Trigger 작성
- 공통 Node 정의/인자 CSV: 3개 Node와 `ApplyShield.target_max_health_ratio` 추가

새 파수꾼 전용 graph/trigger CSV는 만들지 않는다.

## 13. 예상 C# 수정 대상

| 파일 | 변경 내용 |
|---|---|
| `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactCombatRules.cs` | 신규. 활성 개별/시너지 Effect Node의 방어, 최종 피해, 쿨타임 충전 조회 |
| `Pakuri/Assets/Scripts/Combat/Damage/DamageCalculator.cs` | Artifact 방어 보정과 대상 측 최종 피해 배율 적용 |
| `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNodeModifiers.cs` | 세 신규 Artifact 전투 연산 타입 |
| `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNodeConditions.cs` | `OnShieldBreak`, `OnHealOrShieldReceived`, `BossCombatStart` |
| `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs` | 신규 Node 매핑, `ApplyShield.target_max_health_ratio` 생성 |
| `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Buff/BuffSkillDefinition.cs` | 대상 최대 체력 비례 방어막 필드 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionState.cs` | 준비된 대상 최대 체력 방어막 비율 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs` | Shield 준비값 전달, 쿨타임 충전 속도 적용 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs` | Artifact 조건 판정 공통화, Shield `StatusDurationBonus` 1회 반영 |
| `Pakuri/Assets/Scripts/Combat/Skills/Activation/Buff/BuffSkillExecutor.cs` | 대상별 최대 체력 방어막 계산, Heal source 전달 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs` | 세 신규 사건 발행과 기존 Artifact Reaction scheduler 연결 |
| `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs` | 실제 회복/방어막 수령, 보호막 파괴 사건 발행 |
| `Pakuri/Assets/Scripts/GameFlow/Stage/StageManager.cs` | 보스 encounter 확정 뒤 `BeginPlayerCombat` 호출 |
| `Pakuri/Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs` | 구현 완료 후 `sentinel`을 보상 후보 allowlist에 추가 |
| `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs` | 데이터, 계산, 사건, scope 집중 회귀 테스트 |

### 수정하지 않는 핵심 파일

- `ArtifactDefinitions.cs`: `Owner`, Nodes, Reactions 계약이 이미 존재한다.
- `GameDataCatalogBuilder.Artifacts.cs`: 개별/시너지 Node와 Reaction 생성이 이미 연결돼 있다.
- `ArtifactSynergyManager.cs`: 기존 누적 단계 배포와 Owner/AllAllies 배포를 그대로 사용한다.
- `StatusRules.cs`: 일반 받는 피해와 저항 감소는 기존 동작을 유지한다.
- 피해 Actor/Executor: 파수꾼 ID 분기를 추가하지 않는다.

## 14. 구현 Phase

### Phase 1: 데이터·공통 Node 계약

- 파수꾼 설명과 recipient scope 수정
- 3개 공통 Node와 `ApplyShield.target_max_health_ratio`
- 파수꾼 Effect graph/trigger 행 작성
- CSV 열 수, enum, 외래 키, Node 인자 검증

### Phase 2: 방어·최종 피해·쿨타임 조회

- `ArtifactCombatRules`
- 방어 합성식
- 대상 측 최종 피해 배율
- 동적 쿨타임 충전 속도
- Shield `StatusDurationBonus` 반영

### Phase 3: 지원·보호막 사건

- `OnShieldBreak`
- `OnHealOrShieldReceived`
- `BossCombatStart`
- 대상 최대 체력 비례 방어막

### Phase 4: 보상 노출과 검증

- `ArtifactUI` sentinel allowlist
- 집중 EditMode 테스트
- Runtime/Editor 빌드
- Unity runtime catalog 동기화
- 사용자 Play Mode 검증

각 Phase는 정적 검사와 빌드가 통과한 뒤 다음 Phase로 진행한다.

## 15. Edge Case

1. 보호막이 여러 개면 흡수한 각 `ShieldAbsorptionRecord`마다 반사가 발생한다.
2. 한 흡수 사건에서 같은 원천 유닛의 반사율은 합산한다. 서로 다른 원천 유닛의 반사는 원천별로 나눠 실행한다.
3. Trigger 반사 피해는 다시 반사되지 않는다.
4. 공격자가 null이면 반사 대상이 없으므로 피해를 실행하지 않는다.
5. 대상이 공격 전에 보호막을 보유했다면 그 공격으로 보호막이 깨져도 6시너지 최종 피해 감소를 받는다.
6. 시간 만료 보호막은 깨지지 않는 약속을 발동하지 않지만 파수꾼의 종은 발동한다.
7. 실제 증가가 없는 회복/방어막 갱신은 푸른 십자가를 발동하지 않는다.
8. 행동속도 버프는 기존 상태 merge/max stack 규칙을 사용해 무한 중첩하지 않는다.
9. 순백 방패, 순례자 망토와 다른 이름의 보호막은 수치만 총합으로 표시하고 각 `SourceSkillName`별 인스턴스와 남은 시간을 유지한다. 한 보호막 만료 시 해당 수치만 총합에서 빠진다.
10. 8시너지 반사는 25%와 추가 20%를 피해 계산 전 합쳐 단일 45% 원시 피해로 계산한다.
11. Nexus는 기존 `ApplyShieldStatus`의 `target.IsNexus` 차단을 유지해 `모든 아군` 대상에서 제외된다.
12. Stage 전환 시 기존 상태·활성 Effect 초기화 뒤 다시 한 번만 적용한다.
13. Ariel이 Eve에게 준 방어막을 Eve가 보유한 파수꾼 효과가 감지해야 한다. 반대로 Ariel만 가진 Owner 유물이 Eve의 수혜 사건에서 실행되면 안 된다.

## 16. 수용 기준

### 데이터와 배포

- 파수꾼 시너지 2/4/6/8 표시값이 각각 `5/10/15/20%`, `8/12/18/25`다.
- 시너지 0/1개에서 파수꾼 Effect가 없다.
- 2/4/6/8개에서 단계 증가분의 누적 결과가 설명 총합과 일치한다.
- 깨지지 않는 약속은 보유자에게만 배포된다.
- 나머지 9개 유물은 모든 플레이어 아군에게 정확히 한 번 배포된다.
- 파수꾼 유물이 보상 후보에 노출된다.

### 방어와 피해

- 기본 방어력 0인 아군이 2시너지에서 모든 속성 방어력 8을 가진 계산 결과를 낸다.
- 8시너지에서 `base × 1.20 + 25`가 저항 감소 전 방어력이다.
- 최종 피해 증가 `1.15`와 최종 피해 감소 `0.90`이 `1.035`로 곱해진다.
- 6시너지 최종 피해 감소는 보호막이 있을 때만 적용된다.
- 일반 받는 피해 감소와 최종 피해 감소가 별도 구간에서 곱해진다.

### 방어막과 Trigger

- 순백 방패는 전투 시작 시 각 아군 최대 체력의 12%를 한 번 부여한다.
- 순례자 망토는 보스 encounter에서만 각 아군 최대 체력의 50%를 10초 부여한다.
- 수호자의 향로는 모든 유효 시간제 방어막 지속시간에 정확히 2초를 더한다.
- 4시너지 반사는 실제 흡수량의 25%, 8시너지는 합계 45%다.
- 반사 거울은 실제 흡수량의 20%를 반사한다.
- 깨지지 않는 약속은 파괴에만 발동하고 자연 만료에는 발동하지 않는다.
- 파수꾼의 종은 파괴와 자연 만료 모두에서 주변 적에게 신성 노출 2초를 부여한다.
- 푸른 십자가는 실제 회복 또는 방어막 증가 때만 4초 행동속도 +10%를 부여한다.
- 기도석은 방어막 보유 중 쿨다운 감소량만 12% 늘린다.

## 17. Code Builder 검증 요구

### 정적

- 수정 CSV의 header/type/data 열 수 일치
- 파수꾼 17개 Effect ID의 헤더와 graph/trigger 외래 키 일치
- 신규 Node 정의와 params 중복/누락 0개
- `rg`로 C#의 파수꾼 artifact/effect ID 하드코딩 0건 확인
- `git diff --check`

### 집중 EditMode

최소 다음 한 묶음의 실행 가능한 회귀 테스트를 남긴다.

1. recipient scope: 깨지지 않는 약속 Owner, 나머지 AllAllies
2. 시너지 0/1/2/4/6/8 방어 합계
3. 방어 0/기본 방어/Passive DefenseUp/저항 감소 조합
4. 최종 피해 `1.15 × 0.90`
5. 보호막 있음/없음의 6시너지 최종 피해 감소
6. 흡수량 25/45/20% 반사와 Trigger 재귀 차단
7. 방어막 시전자와 수혜자가 다른 경우의 owner 판정, 보호막 파괴와 시간 만료 사건 분리
8. 무효/유효 회복과 방어막 수령 사건
9. 일반/보스 encounter의 순례자 망토
10. 최대 체력이 다른 두 아군의 12%/50% 방어막
11. 방어막 지속시간 +2초
12. 방어막 보유 전/중/후 쿨타임 충전 속도

### 빌드와 Unity

- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore`
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore`
- Unity CSV source validation과 runtime catalog 동기화
- Unity Play Mode 실제 전투 검증은 사용자 수행

## 18. 관련 보드

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `BLACKBOARD.md`

## 19. 근거 파일

- `Pakuri/Assets/CSVdata/Artifact/artifact_synergies.csv`
- `Pakuri/Assets/CSVdata/Artifact/artifacts.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_synergy_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/buff/skill_graph_nodes_buff.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/buff/buff_skill_triger.csv`
- `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs`
- `Pakuri/Assets/Scripts/Combat/Damage/DamageCalculator.cs`
- `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts/Combat/Status/Runtime/StatusState.cs`
- `Pakuri/Assets/Scripts/Combat/Status/Execution/StatusRules.cs`
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactState.cs`
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- `Pakuri/Assets/Scripts/GameFlow/Stage/StageManager.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs`
- `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`
