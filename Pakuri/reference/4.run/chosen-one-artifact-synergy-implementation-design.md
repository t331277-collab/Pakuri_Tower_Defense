# 선택받은자 시너지·유물 구현 설계

## 1. 목표

`chosen-one` 유물을 가장 많이 보유한 파티원 한 명을 전투 시작 시 선택받은자로 지정하고, 2/4/6/8 시너지 효과와 선택받은자 유물 10종을 기존 유물·스킬·상태·피해 계산 경로에 연결한다.

이 문서는 Designer 구현 인계 문서다. 현재 코드와 CSV의 실제 구조를 근거로 작성했으며, 이 문서 작성 단계에서는 C#·CSV·Scene·Prefab을 수정하지 않는다.

## 2. 확정 효과

### 2.1 시너지

| 보유 수 | Effect ID | 효과 |
|---:|---|---|
| 2 | `chosen-one-level-1-encore` | 선택받은자가 액티브 스킬을 3회 사용할 때마다 마지막 액티브 스킬을 위력 50%로 한 번 더 실행한다. |
| 4 | `chosen-one-level-2-final-damage` | 선택받은자의 최종 데미지 배율 `1.18`. |
| 6 | `chosen-one-level-3-highlight` | 15초마다 선택받은자의 남은 쿨타임이 가장 긴 액티브 스킬 하나의 쿨타임을 초기화한다. |
| 8 | `chosen-one-level-4-finale` | 선택받은자가 보스에게 주는 최종 데미지 배율 `1.50`. |

각 단계는 누적 적용한다.

### 2.2 유물

| Artifact ID | Effect ID | 확정 계약 |
|---|---|---|
| `quantum-computation` | `quantum-computation-effect` | 이브의 번개 스킬 위력 배율 `1.50`. |
| `absolute-zero-circuit` | `absolute-zero-circuit-effect` | 이브의 얼음 스킬 위력 배율 `1.35`; 빙결 대상 적중은 추가 배율 `1.25`. |
| `nameless-ledger` | `nameless-ledger-effect` | 대상의 `name-mark` 1스택당 베가 `vega-e` 최종선고의 **스킬 위력 +6%**. |
| `black-execution-platform` | `black-execution-platform-effect` | 베가 `vega-d` 검은 명부 개방 위력 배율 `1.45`. |
| `convection-arrowhead` | 두 기존 Sein Effect | 세인 `sein-a`, `sein-b` 위력 배율 `1.40`. |
| `dooms-ember` | `dooms-ember-effect` | 세인 `sein-e` 쿨타임 배율 `0.75`, 위력 배율 `1.25`. |
| `archangel-sigil` | 두 기존 Ariel Effect | 아리엘 `ariel-b`, `ariel-e` 방어막량 배율 `1.45`. |
| `hymn-baton` | `hymn-baton-effect` | 아리엘 `ariel-c`가 최종 구성한 축복 행동속도 증가량을 `1.35`배 한다. |
| `shattering-glove` | `shattering-glove-effect` | 린 `rin-a` 파쇄권 위력 배율 `1.50`. |
| `martial-artists-breath` | `martial-artists-breath-effect` | 린이 물리 피해를 줄 때 자신에게 행동속도 `+0.04`, 최대 5스택. |

## 3. 이름 없는 명부의 위력 계약

이름 없는 명부는 기존 `TargetStatusStackDamageRateBonus`를 사용하지 않는다.

기존 노드는 스택 피해 구성 요소에 `base_damage × rate`를 추가한다. 확정 요구사항은 최종선고 전체의 스킬 위력 배율 증가이므로 다음처럼 계산한다.

```text
name-mark 스택 수 = S
이름 없는 명부 위력 배율 = 1 + 0.06 × S

최종선고의 적중별 DamageMultiplier
  *= 이름 없는 명부 위력 배율
```

예시:

| 표식 스택 | 명부 배율 |
|---:|---:|
| 0 | `1.00` |
| 1 | `1.06` |
| 5 | `1.30` |
| 10 | `1.60` |

다른 위력 배율과는 합산하지 않고 곱한다. 예를 들어 다른 위력 배율이 `1.25`, 표식이 5스택이면 `1.25 × 1.30 = 1.625`다.

스택은 시전 snapshot 생성 시점이 아니라 실제 적중 대상에서 읽는다. 같은 공격이 서로 다른 표식 수를 가진 대상을 맞히더라도 대상별 결과가 달라야 하기 때문이다.

새 공통 Node 계약:

```text
TargetStatusStackDamageMultiplierBonus
  arg_1: status_name
  arg_2: bonus_rate_per_stack
```

`nameless-ledger-effect` 저작값:

```text
TargetStatusStackDamageMultiplierBonus, name-mark, 0.06
```

## 4. 현재 저장소 상태

### 4.1 이미 존재함

- `artifact_synergies.csv`에 `chosen-one` 2/4/6/8 단계가 있다.
- `artifacts.csv`에 선택받은자 유물 10개가 있다.
- `artifact_effects.csv`에 개별 Effect 헤더가 있다.
- `artifact_synergy_effects.csv`에 시너지 Effect 헤더 네 개가 있다.
- `ArtifactEffectRecipient.ChosenOne` enum이 있다.
- `ArtifactSynergyEffectDefinition`은 이미 `Nodes`와 `Reactions`를 가진다.
- `GameDataCatalogBuilder.Artifacts`는 개별 Effect와 시너지 Effect의 Node·Reaction을 생성한다.
- `SkillTrigger`에는 `OnSkillCast`, `OnOutgoingDamage`, `trigger_every_count`, `event_source_scope`가 있다.
- `SkillExecution.ResetCooldown`과 `SkillExecutionState.CooldownRemaining`이 있다.
- `DamageCalculator`는 치명타 계산 뒤 `FinalDamageModifier`를 적용한다.
- `InGameInfoUI`는 보유 유물을 `SynergyName`으로 그룹화하고 다른 시너지 컨테이너를 아래로 `93.3`씩 복제한다.

### 4.2 아직 없음 또는 연결되지 않음

- `ArtifactSynergyManager.DistributeSynergyEffects`는 `ChosenOne` 수신자를 처리하지 않는다.
- 선택받은자 선정 결과를 보관하는 Stage 상태가 없다.
- 선택받은자 Effect의 실제 graph/trigger 행이 없다.
- 현재 `ExecuteSkill` Node는 고정 `skill_name`만 실행하며 사건을 발생시킨 마지막 스킬을 실행하지 못한다.
- 주기 Trigger와 “가장 긴 남은 쿨타임 하나” 선택 기능이 없다.
- 보스 조건부 `FinalDamageModifier` Node가 없다.
- 동일 출처 상태를 최대치까지 더하는 merge 정책이 없다.
- `ArtifactUI.PrepareChoices`는 `spirit-contract`, `executioner`만 정상 보상 후보로 허용한다.
- `chosen-one` 시너지 icon 경로는 비어 있다.

## 5. 선택받은자 선정과 Effect 배포

`ArtifactSynergyManager.PrepareStage`에서 다음 순서로 처리한다.

```text
1. 모든 파티원의 ActiveArtifactEffectNames 초기화
2. 파티 전체 synergy_name 집계
3. chosen-one 수가 2 이상이면 파티원별 chosen-one 유물 수 계산
4. 가장 많이 가진 파티원 한 명 선택
5. 활성 시너지 단계 중 Recipient=ChosenOne Effect를 그 파티원에게만 배포
6. 개별 유물 Effect 배포
7. Stage 효과 활성화
```

동률 규칙은 파티 슬롯 순서가 빠른 파티원을 선택한다. 무작위 선택은 사용하지 않는다.

선정은 전투마다 다시 수행한다. 보상으로 유물 소유 현황이 바뀌면 다음 전투에서 선택받은자가 바뀔 수 있다.

개별 선택받은자 유물의 `SpecificMonster` 계약은 유지한다. 유물을 들고 있는 파티원은 선정 점수의 소유자이고, 개별 효과의 실제 수신자는 CSV에 적힌 이브·베가·세인·아리엘·린이다.

## 6. 시너지 단계 구현

### 6.1 앙코르

기존 Trigger 게이트를 재사용한다.

```text
trigger_event: OnSkillCast
trigger_every_count: 3
event_source_scope: owner
outcome: ExecuteEventSkill(0.50)
```

새 `ExecuteEventSkill` Node는 `TriggerExecutionContext.EventSourceSkillName`으로 실제 스킬 Definition과 런타임을 찾고 같은 선택받은자가 다시 실행하게 한다.

- 재실행은 쿨타임과 탄창을 다시 소모하지 않는다.
- 원래 시전에서 확정된 조준점 또는 사건 중심을 재사용한다.
- `IsTrigger=true`로 실행해 앙코르 결과가 다시 `OnSkillCast` 3회 카운트를 올리지 않게 한다.
- 위력 배율은 `0.50`을 기존 snapshot `DamageMultiplier`에 곱한다.
- 상태·방어막 등 비피해 결과도 같은 스킬 정의를 실행하되 피해량만 50%가 된다. 방어막과 버프까지 50%로 줄이는 요구는 현재 확정되지 않았으므로 임의로 축소하지 않는다.

현재 유물 PassiveTrigger 공통 실행 경로는 Effect ID를 실제 스킬 이름으로 조회해 `sourceRuntime`을 찾지 못할 수 있다. `SkillExecution.ExecuteReactionOutcome`에서 사건 스킬 런타임을 우선 운반체로 사용하고, 사건 스킬이 없는 반응은 Effect Definition 기반 임시 런타임을 사용하도록 공통 수정한다. 가짜 런타임을 `ActiveSkills`에 등록하지 않는다.

### 6.2 최종 데미지 +18%

기존 Node를 그대로 사용한다.

```text
FinalDamageModifier, 1.18
```

### 6.3 하이라이트

현재 무기한 주기 Trigger가 없으므로 한 효과를 위해 전역 타이머 프레임워크를 새로 만들지 않는다.

`ArtifactSynergyManager`가 선택받은자와 `15`초 남은 시간을 보관하고, `StageManager`가 `Spawning` 또는 `Combat` 상태일 때만 갱신한다.

15초 도달 시:

1. 선택받은자의 `SkillState.ActiveSkills`를 순회한다.
2. `CooldownRemaining > 0`인 스킬 중 가장 큰 하나를 고른다.
3. 동률이면 ActiveSkills 순서가 빠른 스킬을 고른다.
4. 기존 `SkillExecution.ResetCooldown`을 호출한다.
5. 모든 쿨타임이 0이면 아무것도 초기화하지 않는다.
6. 다음 15초 주기를 시작한다.

Reward 상태와 Stage 종료 뒤에는 타이머가 진행되지 않는다.

### 6.4 피날레

기존 `TargetPredicateDamageMultiplier(is_boss)`는 일반 위력 계층이므로 사용하지 않는다.

새 공통 Node:

```text
TargetPredicateFinalDamageModifier
  arg_1: predicate = is_boss
  arg_2: multiplier = 1.50
```

대상별 적중 보정 확정 단계에서 `target.IsBoss`를 검사하고 해당 적중의 `FinalDamageModifier`에 `1.50`을 곱한다. `DamageCalculator`는 전달받은 최종 배율을 치명타 계산 뒤 적용하는 현재 책임을 유지한다.

## 7. 개별 유물 Node·Trigger 설계

### 7.1 기존 Node만으로 구현

```text
quantum-computation-effect
  ConditionSkillAttribute, Lightning
  DamageMultiplier, 1.50

absolute-zero-circuit-effect
  ConditionSkillAttribute, Ice
  DamageMultiplier, 1.35
  ConditionalDamageMultiplier, freeze, 1, 1.25

black-execution-platform-effect
  DamageMultiplier, 1.45

convection-arrowhead-sein-a-effect
  DamageMultiplier, 1.40

convection-arrowhead-sein-b-effect
  DamageMultiplier, 1.40

dooms-ember-effect
  CooldownMultiplier, 0.75
  DamageMultiplier, 1.25

archangel-sigil-ariel-b-effect
  ShieldAmountMultiplier, 1.45

archangel-sigil-ariel-e-effect
  ShieldAmountMultiplier, 1.45

shattering-glove-effect
  DamageMultiplier, 1.50
```

빙결 대상의 총 위력 배율은 `1.35 × 1.25`다.

### 7.2 성가 지휘봉

아리엘 C의 기본 축복은 행동속도 `+0.12`이며 특성으로 추가 보정될 수 있다. 고정 `+0.042`를 더하지 않고 최종 축복 행동속도 증가량을 `1.35`배 한다.

새 공통 Node:

```text
StatusActionSpeedMultiplier, blessing, 1.35
```

예시:

```text
기본 축복 0.12 -> 0.162
기본 0.12 + 특성 0.06 -> 0.18 × 1.35 = 0.243
```

### 7.3 무투가의 호흡

Trigger:

```text
trigger_event: OnOutgoingDamage
trigger_attribute: Physical
event_source_scope: owner
proc_chance: 1
```

결과 상태:

```text
target: Self
duration: 9999
stack amount: 1
max stacks: 5
action speed bonus per stack: 0.04
merge policy: same_source_add_stacks
```

기존 `SameSourceRefresh`는 스택을 증가시키지 않고, `AlwaysStack`은 5스택 총상한을 보장하지 못한다. 따라서 `StatusMergePolicy.SameSourceAddStacks`를 추가한다.

`BuildNormalStatusModifierEffect`는 기존 `AttachStatusPayload`의 `status_stack_amount`, `status_max_stacks`, duration, merge policy를 실제 `StatusRuntimeData`에 복사해야 한다.

## 8. 공통 적중 보정 경계

현재 `ResolveHitCritModifiers`가 Projectile, Line, Single, Zone의 대상별 치명타 조건을 모은다. 이 경계를 대상별 피해 보정까지 포함하는 공통 적중 보정 경계로 확장한다.

입출력:

```text
입력: snapshot, target, roster
갱신: DamageMultiplier, FinalDamageModifier, CritChanceBonus, CritDamageBonus
```

여기서 처리할 신규 조건:

- `TargetStatusStackDamageMultiplierBonus`: 이름 없는 명부
- `TargetPredicateFinalDamageModifier(is_boss)`: 피날레

Actor별로 유물 ID를 직접 검사하지 않는다. Actor는 공통 resolver 결과를 기존 `ApplyDamage` 인자로 전달한다.

## 9. 획득·HUD

### 9.1 정상 보상

`ArtifactUI.PrepareChoices`의 현재 허용 시너지에 `chosen-one`을 추가한다. 기존 무작위 섞기, 최대 3개 후보, 런 전체 중복 금지, 파티원당 유물 3개 제한은 유지한다.

### 9.2 HUD

`InGameInfoUI`는 이미 `SynergyName`별 그룹화, 컨테이너 캐시, 다른 시너지 Y `-93.3` 배치를 구현했으므로 수정하지 않는다.

다만 `artifact_synergies.csv`의 `chosen-one` icon 경로는 현재 빈 값이다. HUD icon 표시를 원하면 실제 존재하는 Sprite asset을 준비하고 해당 CSV 경로를 채워야 한다. 존재하지 않는 asset 경로를 임의로 작성하지 않는다.

## 10. 수정 대상

### 10.1 C# 필수 수정

| 파일 | 수정 책임 |
|---|---|
| `Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs` | 선택받은자 선정·`ChosenOne` Effect 배포·하이라이트 타이머와 대상 스킬 선택 |
| `Assets/Scripts/GameFlow/Stage/StageManager.cs` | `Spawning/Combat` 상태에서 하이라이트 Stage tick 호출 |
| `Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNodeActions.cs` | 사건 스킬 실행, 스택 비례 위력, 조건부 최종 피해, 상태 수치 배율 Operation 정의 |
| `Assets/Scripts/Combat/Skills/Execution/SkillExecutionState.cs` | 신규 Operation의 snapshot 상태 보관 |
| `Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs` | 신규 Node 적용과 대상별 적중 배율 계산 |
| `Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs` | `ExecuteEventSkill`, 유물 PassiveTrigger 실행 운반체, 기존 `ResetCooldown` 재사용 |
| `Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs` | 신규 Node 파싱·생성, `AttachStatusPayload`의 스택 계약 반영 |
| `Assets/Scripts/Combat/Status/Definitions/StatusEffectDefinition.cs` | `SameSourceAddStacks` merge enum 추가 |
| `Assets/Scripts/Combat/Status/Runtime/StatusState.cs` | 동일 출처 스택 합산과 MaxStacks 적용 |
| `Assets/Scripts/Loading/Parsing/StatusValueParser.cs` | `same_source_add_stacks` 문자열 파싱 |
| `Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillActor.cs` | 공통 대상별 적중 보정 결과 전달 |
| `Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs` | 공통 대상별 적중 보정 결과 전달 |
| `Assets/Scripts/Combat/Skills/Activation/Line/LineSkillActor.cs` | 공통 대상별 적중 보정 결과 전달 |
| `Assets/Scripts/Combat/Skills/Activation/Zone/ZoneSkillActor.cs` | 공통 대상별 적중 보정 결과 전달 |
| `Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs` | 정상 보상 후보에 `chosen-one` 추가 |

### 10.2 CSV 필수 수정

| 파일 | 수정 책임 |
|---|---|
| `Assets/CSVdata/Artifact/artifacts.csv` | 이름 없는 명부 설명을 `이름표식 1스택당 최종선고 위력 +6%`로 정정 |
| `Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv` | 선택받은자 시너지·개별 유물 Node 작성 |
| `Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv` | 앙코르·무투가의 호흡 Trigger 작성 |
| `Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv` | 신규 공통 Node 등록 |
| `Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv` | 신규 Node 인자 계약 등록 |
| `Assets/CSVdata/Artifact/artifact_synergies.csv` | 실제 chosen-one 시너지 icon asset이 준비되면 경로 기록 |

### 10.3 수정하지 않는 대상

- `DamageCalculator.cs`: 전달받은 일반·최종 배율을 계산하는 현재 책임 유지
- `SkillTrigger.cs`: 기존 `OnSkillCast`, `OnOutgoingDamage`, 횟수 게이트, owner scope 재사용
- `ArtifactDefinitions.cs`: `ChosenOne`, `Nodes`, `Reactions`가 이미 존재
- `GameDataCatalogBuilder.Artifacts.cs`: 개별·시너지 Node/Reaction 생성 경로가 이미 존재
- `InGameInfoUI.cs`: 다중 시너지 컨테이너와 Y `-93.3` 배치가 이미 존재
- Scene/Prefab: 현재 설계 범위에서 새 계층을 요구하지 않음

## 11. 구현 Phase

### Phase 1: 데이터 계약

- 신규 Node 정의와 params
- 선택받은자 graph/trigger 행
- 이름 없는 명부 문구 정정
- CSV 열 수·외래 키·Node 인자 검증

### Phase 2: 선택받은자 수신자

- 파티원별 chosen-one 보유 수 계산
- 결정적 동률 처리
- `Recipient=ChosenOne` 배포
- Stage 재선정과 활성 Effect 중복 방지

### Phase 3: 공통 적중 배율

- 이름표식 스택 비례 위력
- 보스 조건부 최종 데미지
- Single/Projectile/Line/Zone 동일 적용

### Phase 4: Trigger 결과

- 유물 PassiveTrigger 실행 운반체 수정
- `ExecuteEventSkill`과 앙코르 3회 게이트
- `SameSourceAddStacks`와 무투가의 호흡 5스택

### Phase 5: 하이라이트·획득

- 15초 Stage timer
- 가장 긴 남은 쿨타임 하나 초기화
- ArtifactPanel에 chosen-one 후보 추가
- 기존 HUD 재사용 확인

### Phase 6: 검증

- Runtime/Editor 빌드
- CSV 구조와 RuntimeCatalog 생성
- 집중 EditMode 테스트
- Unity Play Mode gameplay 검증은 사용자 수행

## 12. 수용 기준

### 선택과 Stage

- chosen-one 0/1개에서는 선택받은자 시너지 Effect가 없다.
- 2개 이상이면 가장 많이 보유한 파티원 한 명만 선택된다.
- 동률 결과가 파티 슬롯 순서로 재현 가능하다.
- 다음 전투에서 유물 소유 수가 바뀌면 다시 선정된다.

### 이름 없는 명부

- 0/1/5/10스택에서 위력 배율이 각각 `1.00/1.06/1.30/1.60`이다.
- 기존 최종선고의 고정 표식 추가 피해 계산은 유지된다.
- 이름 없는 명부 배율은 전체 최종선고 스킬 피해에 적용된다.
- 서로 다른 대상의 표식 스택은 서로 섞이지 않는다.

### 시너지

- 세 번째 정상 액티브 시전마다 마지막 스킬이 피해 위력 50%로 한 번 재실행된다.
- 앙코르 재실행은 다시 앙코르 횟수를 올리지 않고 쿨타임·탄창을 소모하지 않는다.
- 4단계 최종 데미지 `1.18`은 치명타 계산 뒤 적용된다.
- 15초마다 양수 쿨타임 중 가장 긴 하나만 0이 된다.
- 보스 적중에서만 피날레 최종 데미지 `1.50`이 추가된다.

### 유물

- 번개·얼음·빙결·특정 스킬·방어막 조건이 지정된 몬스터와 스킬에만 적용된다.
- 성가 지휘봉은 특성까지 반영된 축복 행동속도 증가량을 `1.35`배 한다.
- 무투가의 호흡은 물리 피해 사건당 1스택, 최대 5스택이며 다른 PassiveBuff와 합쳐지지 않는다.
- Stage reset 뒤 무투가의 호흡 스택은 남지 않는다.

### 획득·표시

- 정상 ArtifactPanel에서 정령계약·처형관·선택받은자 유물 전체 중 미보유 유물 최대 3개가 후보가 된다.
- 선택받은자 유물은 기존 RunSession 중복·용량 제한을 따른다.
- 같은 시너지는 한 HUD 컨테이너에 누적되고 다른 시너지는 Y `-93.3` 간격으로 표시된다.

## 13. Code Builder 검증 요구

- 모든 수정 CSV의 header/type/data 열 수 검사
- 선택받은자 Effect 14개가 header와 graph/trigger에 연결됐는지 `rg`로 검사
- 이름 없는 명부 `0/1/5/10`스택 위력 테스트
- 앙코르 1/2/3/6회 시전과 재귀 방지 테스트
- 선택받은자 2/4/6/8 단계 누적 테스트
- 하이라이트 15초, 동률, 전부 쿨타임 0인 경우 테스트
- 무투가의 호흡 0~6회 물리 적중과 비물리 적중 테스트
- `git diff --check`
- Runtime/Editor `dotnet build --no-restore`
- Unity Play Mode에서 실제 보상 획득, 다음 전투 재선정, HUD, 앙코르, 하이라이트를 사용자가 확인

## 14. 근거 파일

- `Pakuri/Assets/CSVdata/Artifact/artifact_synergies.csv`
- `Pakuri/Assets/CSVdata/Artifact/artifacts.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_synergy_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/single_attack/skill_graph_nodes_single_attack.csv`
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs`
- `Pakuri/Assets/Scripts/Combat/Artifact/Definition/ArtifactDefinitions.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Artifacts.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionState.cs`
- `Pakuri/Assets/Scripts/Combat/Status/Runtime/StatusState.cs`
- `Pakuri/Assets/Scripts/Combat/Damage/DamageCalculator.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Info/InGameInfoUI.cs`

