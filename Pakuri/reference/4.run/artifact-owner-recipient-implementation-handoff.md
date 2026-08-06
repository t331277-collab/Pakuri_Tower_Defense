# 유물 보유자 전용 수신 범위 구현 Handoff

## 1. 상태와 역할

- 작성일: 2026-08-07
- 설계 롤: Designer
- 구현 롤: Code Builder
- 선택 트랙: Structure Design, Implementation Design, Builder Handoff
- 상태: 구현 완료

## 2. 목표

개별 유물 Effect의 수신 범위를 데이터에서 명시적으로 구분한다.

- `AllAllies`: 모든 파티원에게 Effect ID를 배포한다.
- `Owner`: 유물을 실제로 보유한 파티원에게만 Effect ID를 배포한다.
- `SpecificMonster`: 현재처럼 지정 몬스터에게 배포한다.
- `Stage`: 전장·Stage 공통 결과를 소유하는 기존 용도로 유지한다.
- `Summon`: 소환 유닛 대상 용도로 유지한다.

플레이어에게 `모든 아군` 효과라고 명시된 유물만 `AllAllies`를 사용한다. 보유자 전용 여부를 `description_text` 문자열에서 런타임에 추론하지 않고 `artifact_effects.csv.recipient_scope`를 유일한 실행 계약으로 사용한다.

## 3. 확인한 현재 코드 근거

1. `RunSession.TryAcquireArtifact(member, artifactName)`은 선택한 `member.Artifacts`에만 유물 이름을 저장한다.
2. `ArtifactSynergyManager.PrepareStage`는 보유자를 알고 `DistributeEffects(session, owner, artifact, ...)`를 호출한다.
3. 현재 `DistributeEffects`는 `AllAllies` Effect를 모든 `PartyMembers`에 복사한다. `Stage`는 보유자에게만 `AddEffect`한다.
4. `SkillExecutionRules`와 `SkillTrigger`는 실행 유닛 자신의 `Artifacts.ActiveArtifactEffectNames`만 읽는다.
5. `InGameCombatManager`의 운명의 동전 처리도 공격 source 자신의 `Artifacts`만 읽는다.
6. 강화와 마스터는 `RunSession.RecordOfferingChoice`가 선택한 `member.Skills`에 기록하고 `SkillExecutionRules.BuildExecutionData`가 현재 `owner.Skills`만 읽는다. 유물 수신 범위 변경 대상이 아니다.
7. `ArtifactEffectRecipient`에는 `Owner`가 없다. `ChosenOne`은 선언돼 있지만 `ArtifactSynergyManager`의 개별 유물 배포 분기에서 처리되지 않으며 보유자 의미도 아니다.
8. `CsvRowParser`는 `recipient_scope`를 `ReadEnum<ArtifactEffectRecipient>`로 읽으므로 enum 추가 뒤 별도 parser 분기는 필요하지 않다.
9. 현재 실제 Artifact Node/Trigger가 존재하는 범위는 정령계약 10개와 처형관 10개다. Chosen One, Sentinel, Artillery의 개별 Effect 헤더는 존재하지만 실행 Node/Trigger는 아직 없다.

## 4. 책임 경계

### `artifact_effects.csv`

각 개별 유물 Effect의 수신 범위를 소유한다. 표시 문구가 아니라 이 열이 런타임의 권위다.

### `ArtifactSynergyManager`

Stage 시작 시 `recipient_scope`에 따라 Effect ID를 올바른 `ArtifactState.ActiveArtifactEffectNames`에 배포한다. 피해량, 치명타, 스킬 조건은 계산하지 않는다.

### `SkillExecutionRules` / `SkillTrigger` / `InGameCombatManager`

현재 유닛에게 배포된 Effect만 소비한다. `Owner`가 올바르게 배포되면 기존 소비 코드를 그대로 재사용한다.

### 스킬 강화·마스터

현재 `member.Skills` 소유 모델을 유지한다. 유물 `Owner` 계약과 합치거나 공통 수신 시스템을 만들지 않는다.

## 5. 수정 대상

### 5.1 C#

| 파일 | 변경 내용 |
|---|---|
| `Pakuri/Assets/Scripts/Combat/Artifact/Definition/ArtifactDefinitions.cs` | `ArtifactEffectRecipient`에 `Owner` 추가 |
| `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs` | `DistributeEffects`에서 `Owner`를 현재 `owner`에게 한 번만 `AddEffect`하고 종료하는 분기 추가 |
| `Pakuri/Assets/Scripts/Loading/Validation/CsvDataValidator.cs` | `repeat_rule` 수신 범위 검증에서 `Owner`를 허용하고 오류 문구 갱신 |
| `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs` | 보유자 전용, 모든 아군 유지, 반복 Effect 배포 회귀 검증 |

### 5.2 CSV

| 파일 | 변경 내용 |
|---|---|
| `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_effects.csv` | 아래 확정 매핑대로 `recipient_scope` 변경 |

### 5.3 수정하지 않는 파일

- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts/GameFlow/RunSession.cs`
- `Pakuri/Assets/Scripts/Units/Runtime/UnitSkills.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/OfferingUI.cs`
- `Pakuri/Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_synergy_effects.csv`
- `DamageCalculator` 및 피해 공식 파일

`artifacts.csv.description_text`는 실행 로직이 아니므로 이번 구현의 필수 수정 대상이 아니다. 별도 문구 정리 작업을 하지 않는 한 현재 표시 문구를 유지한다.

## 6. 최소 구현 계약

### 6.1 enum

`ArtifactEffectRecipient`에 `Owner`를 추가한다. 기존 enum 이름을 바꾸거나 `Stage`/`ChosenOne`을 재사용하지 않는다.

`Owner`는 enum 마지막에 추가해 기존 Unity 직렬화 자산의 `SpecificMonster`, `Stage`, `Summon`, `ChosenOne` 숫자 값을 이동시키지 않는다.

### 6.2 배포 분기

`ArtifactSynergyManager.DistributeEffects`에서 기존 repeat 계산 뒤 다음 순서를 적용한다.

1. `Owner`: 현재 유물 보유자인 `owner`에게 `AddEffect(owner, ...)` 후 다음 Effect로 이동
2. `Stage`: 기존 동작대로 `owner`에게 `AddEffect(owner, ...)` 후 다음 Effect로 이동
3. `AllAllies`: 기존 파티 순회에서 모든 파티원에게 배포
4. `SpecificMonster`: 기존 파티 순회에서 지정 몬스터에게만 배포

신규 manager, resolver, interface, event를 추가하지 않는다.

### 6.3 반복 Effect 검증

현재 validator는 `repeat_rule`을 `SkillModifier + AllAllies`에서만 허용한다. 다음 두 Owner Effect가 반복 규칙을 사용하므로 `SkillModifier + (AllAllies 또는 Owner)`를 허용해야 한다.

- `spirit-elixir-contract-count-effect`: `SynergyArtifactCount`
- `elemental-codex-effect`: `DistinctRepresentativeAttributeCount`

`selection_rule`은 현재 Owner로 바꿀 행이 사용하지 않으므로 기존 `AllAllies` 제한을 유지한다.

## 7. 확정 CSV 매핑

### 7.1 정령계약

| Artifact | Effect | 확정 범위 |
|---|---|---|
| `elemental-prism` | `elemental-prism-*-effect` 5개 | `AllAllies` 유지 |
| `ember-crown` | 기본/노출 Effect 2개 | `Owner` |
| `frost-lens` | 기본/상태 Effect 2개 | `Owner` |
| `storm-capacitor` | 기본/감전 Effect 2개 | `Owner` |
| `radiant-chalice` | 기본/방어막 Effect 2개 | `Owner` |
| `black-candlestick` | 기본/표식 Effect 2개 | `Owner` |
| `spirit-elixir` | 기본/정령계약 개수 Effect 2개 | `Owner` |
| `rift-gem` | `rift-gem-effect` | `Owner` |
| `elemental-codex` | `elemental-codex-effect` | `Owner` |
| `resonance-compass` | `resonance-compass-effect` | `Owner` |

균열 보석의 Effect carrier만 `Owner`로 명확히 한다. 기존 Trigger의 `SelectTargets Enemy / Battlefield`는 유지하므로 실제 저항 감소 대상은 계속 전장의 적이다.

### 7.2 처형관

| Artifact | Effect | 확정 범위 |
|---|---|---|
| `glass-heart` | `glass-heart-effect` | `AllAllies` 유지 |
| `starlight-whetstone` | `starlight-whetstone-effect` | `AllAllies` 유지 |
| `prophets-eye` | `prophets-eye-effect` | `Owner` |
| `execution-ring` | `execution-ring-effect` | `Owner` |
| `sharp-chalice` | `sharp-chalice-effect` | `Owner` |
| `red-scope` | `red-scope-effect` | `Owner` |
| `cracked-crown` | `cracked-crown-effect` | `Owner` |
| `execution-ledger` | `execution-ledger-effect` | `Owner` |
| `silver-needle` | `silver-needle-effect` | `Owner` |
| `coin-of-fate` | `coin-of-fate-effect` | `Owner` |

## 8. 기존 로직 재사용 결과

- Owner `SkillModifier`는 보유자의 snapshot에만 합성된다.
- Owner `PassiveTrigger`는 보유자가 source인 사건에서만 조회된다.
- 운명의 동전의 누적 상태는 보유자 `ArtifactState`에만 설정되고 증가·초기화된다.
- 정령의 비약과 원소 도감은 파티 시너지 개수/대표 속성 개수를 기존처럼 계산하지만 배율 혜택은 보유자에게만 간다.
- 유리 심장과 별빛 숫돌은 기존처럼 모든 파티원 snapshot에 적용된다.
- 강화·마스터는 기존 소유 유닛 전용 상태를 유지한다.

## 9. 제외 범위

- Chosen One, Sentinel, Artillery의 미구현 Node/Trigger 작성
- `ChosenOne` recipient의 런타임 구현
- 시너지 단계 Effect의 수신 범위 변경
- 유물 설명 문구 일괄 수정
- 보상 UI, 획득 제한, 저장 형식 변경
- 피해 공식, 치명타 공식, 상태 공식 변경

향후 나머지 유물을 구현할 때는 다음 우선순위로 `recipient_scope`를 명시한다.

1. 특정 몬스터가 명시된 효과: `SpecificMonster`
2. 모든 아군 효과: `AllAllies`
3. 전장·Stage 결과: `Stage`
4. 소환 유닛 효과: `Summon`
5. 그 외 보유자 효과: `Owner`

## 10. Edge Case

1. 한 유물이 Effect 행을 여러 개 가지면 모든 행을 같은 의도에 맞춰 변경한다. 한 행만 `Owner`로 바꾸지 않는다.
2. `RunSession.HasArtifact`가 파티 전체 중복 획득을 막으므로 동일 유물이 여러 owner에게 중복 배포되는 경우는 현재 없다.
3. `Owner` Effect의 repeat count는 파티 전체 시너지 상태를 참조할 수 있지만 Effect ID는 owner에게만 반복 추가한다.
4. `AllAllies` Effect를 보유한 유닛도 자기 자신을 포함해 정확히 한 번 수신한다.
5. `ChosenOne`은 owner의 동의어로 사용하지 않는다.

## 11. 검증 기준

### 정적·빌드

- 변경 CSV의 모든 행이 헤더와 같은 9열인지 검사
- `Owner` 값이 runtime catalog에서 enum으로 생성되는지 확인
- `git diff --check`
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`

### 집중 EditMode

`SkillCatalogRuntimeTests`에서 최소 다음을 검증한다.

1. Ariel이 `ember-crown`을 보유하고 Eve가 미보유이면 Ariel만 해당 Effect를 가진다.
2. Ariel이 `glass-heart` 또는 `elemental-prism`을 보유하면 Ariel과 Eve 모두 해당 Effect를 가진다.
3. `spirit-elixir-contract-count-effect`와 `elemental-codex-effect`의 반복 개수는 기존 계산값을 유지하면서 owner에게만 존재한다.
4. `rift-gem-effect`는 보유자에게만 존재한다.
5. `coin-of-fate-effect`는 보유자에게만 존재하고 다른 파티원의 Fate Coin 상태는 설정되지 않는다.

### 사용자 Play Mode

- 서로 다른 두 유닛에게 Owner/AllAllies 유물을 나누어 지급한 뒤 실제 피해·치명타·Trigger 적용 주체를 확인한다.
- 강화·마스터가 기존처럼 학습한 유닛에게만 적용되는지 회귀 확인한다.

## 12. 완료 조건

- 구현된 정령계약·처형관 개별 유물이 확정 매핑대로 배포된다.
- `AllAllies` 유물은 기존 파티 전체 효과를 유지한다.
- `Owner` 유물은 비보유 파티원의 `ActiveArtifactEffectNames`에 들어가지 않는다.
- 기존 스킬 강화·마스터, 피해 계산, Trigger Node 계약을 수정하지 않는다.
- CSV 구조 검사, 솔루션 빌드, 집중 EditMode 검증이 통과한다.

## 13. 관련 보드

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`

## 14. 구현 결과

- `ArtifactEffectRecipient.Owner`와 보유자 배포 분기를 추가했다.
- 구현된 정령계약·처형관 Effect 23개를 `Owner`로 변경하고 `AllAllies` Effect는 유지했다.
- 반복 규칙의 `Owner` 검증을 허용했다.
- 집중 Unity EditMode 3개가 3/3 통과했다.
- Runtime/Editor 솔루션 빌드는 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- Unity Play Mode 검증은 사용자 소유로 남아 있다.
