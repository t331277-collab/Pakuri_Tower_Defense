# 처형관 시너지·유물 구현 설계

## 1. 문서 상태

- 작성일: 2026-08-07
- 역할: Designer
- 상태: Code Builder 구현 진행 중
- 원문: `Pakuri/reference/4.run/artifact-synergy-list.md`의 `## 5. 처형관`
- 범위: 처형관 유물 획득, 시너지 집계와 Stage 적용, 유물 10개, 2/4/6/8 시너지, 최종 피해, HUD 다중 시너지 표시
- 제외: 코드·CSV·Scene 구현, Git 커밋, Unity Play Mode 검증

## 2. 핵심 확정 규칙

1. 정령계약의 유물 획득, 파티 시너지 집계, Stage 준비 순서를 재사용한다.
2. 처형관 개별 유물과 시너지 효과는 학습 Passive나 Choice로 위장하지 않는다.
3. 개별 유물과 활성 시너지 Effect ID를 Stage 시작 때 대상 파티원의 `ActiveArtifactEffectNames`에 배포한다.
4. 모든 수치는 CSV가 소유한다. `ArtifactSynergyManager`가 피해·치명타 수식을 직접 계산하지 않는다.
5. 최종 피해는 치명타 판정과 치명타 피해 적용 뒤, 최종 반올림과 보호막·HP 차감 전에 곱한다.
6. HUD는 **같은 시너지 유물을 같은 `Artifact_Container`에 누적**한다.
7. 다른 시너지 유물을 처음 획득하면 마지막 원본 또는 복제 `Artifact_Container`를 복제하고, 새 컨테이너의 `localPosition.y`를 이전 컨테이너보다 **93.3 낮게** 배치한다.

```text
같은 synergy_name
  -> 기존 Artifact_Container의 count 증가

다른 synergy_name
  -> 마지막 Artifact_Container 복제
  -> clone.localPosition.y = previous.localPosition.y - 93.3
```

## 3. 현재 저장소 근거

### 3.1 이미 존재하는 데이터

- `artifact_synergies.csv`에 `executioner`와 2/4/6/8 단계 설명, 시너지 icon 경로가 있다.
- `artifacts.csv`에 처형관 유물 10개와 실제 icon 경로가 있다.
- `artifact_effects.csv`에 처형관 개별 유물 Effect 헤더 10개가 있다.
- `artifact_synergy_effects.csv`에 처형관 단계 Effect 헤더 4개가 있다.

### 3.2 아직 존재하지 않는 실행 연결

- `skill_graph_nodes_artifact.csv`에는 처형관 Effect의 실제 Node 행이 없다.
- `ArtifactSynergyEffectDefinition`에는 현재 `Nodes`와 `Reactions`가 없다.
- `GameDataCatalogBuilder.BuildArtifactSynergyEffects`는 현재 시너지 Effect graph를 생성하지 않는다.
- `ArtifactSynergyManager.ActivateStageEffects`는 현재 `SpawnUnit`과 소환물 `GrantSkill`만 처리한다.
- `SkillExecutionRules`와 `SkillTrigger`는 현재 `ArtifactEffectDefinition`만 활성 Effect ID로 해석한다.

### 3.3 현재 획득·HUD 제한

- `ArtifactUI.RewardSynergyName`은 `spirit-contract`로 고정돼 처형관 유물이 정상 보상 후보에 들어가지 않는다.
- `InGameInfoUI.DisplayedSynergyName`도 `spirit-contract`로 고정돼 한 시너지만 표시한다.
- 씬의 `HUD/Artifact_Container`는 `Transform` 루트다.
- 기존 표시 자식은 `Image/Icon`, `Image/Cur`, `Image/Lv2`, `Image/Lv4`, `Image/Lv6`, `Image/Lv8`이다.
- 개별 유물 이름이나 효과 설명을 나열할 Text/List 자식은 현재 없다.

### 3.4 현재 피해 계약 제한

- `AttackRule.FinalDamageMultiplier`는 이름과 달리 치명타 전에 스킬 피해 배율로 적용된다.
- `DamageCalculator.CalculateFinalDamage`는 치명타 발생 여부를 외부로 반환하지 않는다.
- `InGameResourceChangeResult`와 `TriggerExecutionContext`에는 치명타 결과가 없다.
- C# 런타임에는 치명타 저항과 치명타 저항 관통 스탯이 없지만, 별빛 숫돌 변경으로 해당 신규 스탯은 더 이상 필요하지 않다.

## 4. 처형관 시너지 규칙

각 단계는 누적 적용한다. 8개 보유 시 2/4/6/8 효과를 모두 가진다.

| 보유 수 | Effect ID | 적용 효과 |
|---:|---|---|
| 2 | `executioner-level-1-critical-chance` | 모든 아군 치명타 확률 `+0.08`; 대상 현재 체력 비율이 `0.35` 이하이면 추가 `+0.08` |
| 4 | `executioner-level-2-critical-damage` | 모든 아군 치명타 피해 `+0.35`; 실제 치명타가 발생한 공격의 최종 피해 배율 `1.08` |
| 6 | `executioner-level-3-low-health-critical-damage` | 대상 현재 체력 비율이 `0.35` 이하이면 치명타 피해 `+0.60` |
| 8 | `executioner-level-4-boss-critical-bonus` | `target.IsBoss`이면 치명타 확률 `+0.15`, 치명타 피해 `+0.80` |

현재 `StageManager`가 중간보스와 보스 encounter에 모두 `isBoss=true`를 전달하므로 `UnitCombatState.IsBoss`를 8시너지 조건에 재사용한다.

## 5. 처형관 유물 규칙

| Artifact ID | Effect ID | 구현 계약 |
|---|---|---|
| `glass-heart` | `glass-heart-effect` | 모든 아군 snapshot에 기존 `CritDamageBonus(0.20)` 단일 효과 적용 |
| `prophets-eye` | `prophets-eye-effect` | 적중 대상이 현재 생존 적 중 최고 `CurrentHealth`이면 `CritChanceBonus(0.15)` |
| `execution-ring` | `execution-ring-effect` | 대상 현재 체력 비율 `<=0.35`이면 `CritDamageBonus(0.50)` |
| `sharp-chalice` | `sharp-chalice-effect` | 일반 신성 피해가 실제 치명타로 적중하면 대상 `HolyExposure` 지속시간 `+1초` |
| `red-scope` | `red-scope-effect` | `MagazineProjectile` 또는 `CooldownProjectile` 스킬에 `CritChanceBonus(0.12)` |
| `cracked-crown` | `cracked-crown-effect` | `target.IsBoss`이면 `CritChanceBonus(0.10)`과 `CritDamageBonus(0.25)` |
| `execution-ledger` | `execution-ledger-effect` | 대상이 `name-mark` 또는 `sein-a-hit-mark`를 가지면 `CritChanceBonus(0.15)` |
| `silver-needle` | `silver-needle-effect` | 기존 `PreparedMagazineLastProjectile`가 `true`인 투사체의 직접 피해에 `CritDamageBonus(0.70)` |
| `starlight-whetstone` | `starlight-whetstone-effect` | 모든 아군 snapshot에 기존 `CritChanceBonus(0.20)` 단일 효과 적용 |
| `coin-of-fate` | `coin-of-fate-effect` | 비치명타 일반 공격 뒤 다음 치명타 가능 공격의 확률 `+0.05`, 최대 `+0.25`; 치명타 발생 시 0으로 초기화 |

### 5.1 백은 바늘 마지막 투사체 정의

- 새 `HitSequenceCount`와 `HitSequenceIndex`를 만들지 않는다.
- 기존 `SkillExecution.PrepareProjectileRuntime`이 `MagazineRemaining == 1`일 때 확정하는 `PreparedMagazineLastProjectile`를 그대로 사용한다.
- 이 값은 `ProjectileSkillExecutor`를 거쳐 `ProjectileSkillActor.isMagazineLastProjectile`에 이미 전달된다.
- 백은 바늘은 이 값이 `true`인 투사체의 직접 `ApplyDamage` 호출에만 `CritDamageBonus(0.70)`을 추가한다.
- 관통으로 같은 마지막 투사체가 여러 적을 맞히면 그 투사체의 모든 직접 적중에 적용한다.
- 분기 피해, 도착 스킬, `AttackRule.IsTrigger=true`인 후속 피해에는 전파하지 않는다.
- 기존 `OnMagazineLastProjectileHit`는 피해 적용 뒤 실행되므로 현재 적중의 치명타 피해 보정에는 사용하지 않는다. 해당 이벤트의 기존 Ariel 반응 동작은 변경하지 않는다.

### 5.2 운명의 동전 상태 정의

- 유물을 적용받는 파티원마다 독립 누적값을 가진다.
- `AttackRule.IsTrigger=true`인 후속 피해는 누적과 초기화에서 제외한다.
- 다음 치명타 가능 일반 피해 사건의 판정 전에 현재 누적 보너스를 사용한다.
- 비치명타면 1 증가, 최대 5중첩이다.
- 치명타면 해당 공격에는 기존 누적 보너스를 적용한 뒤 0으로 초기화한다.
- Stage 시작 때 누적값을 0으로 초기화한다.

## 6. 데이터와 Definition 설계

### 6.1 기존 CSV 재사용

- Effect 헤더: `Artifact/Effect/artifact_effects.csv`
- 시너지 Effect 헤더: `Artifact/Effect/artifact_synergy_effects.csv`
- 실제 Node: `Artifact/Skill/skill_graph_nodes_artifact.csv`
- 실제 Trigger: `Artifact/Skill/artifact_skill_triger.csv`
- Node 계약: 기존 공통 `skill_node_definitions.csv`, `skill_node_definition_params.csv`

새 처형관 전용 Node CSV나 Trigger CSV를 만들지 않는다.

### 6.2 시너지 Effect Definition 보강

`ArtifactSynergyEffectDefinition`에 다음 기존 타입 필드를 추가한다.

```csharp
public SkillNode[] Nodes = Array.Empty<SkillNode>();
public SkillReaction[] Reactions = Array.Empty<SkillReaction>();
```

`BuildArtifactSynergyEffects`는 개별 유물 Effect와 같은 `SkillNodeOwnerKind.Effect` 경로로 Node를 만들고, Effect ID를 source로 쓰는 기존 Reaction 생성 경로를 사용한다.

### 6.3 공통 활성 Effect 해석

`ActiveArtifactEffectNames` 저장 형식은 바꾸지 않는다. 소비 시 다음 두 Definition을 순서대로 조회한다.

1. `ArtifactEffectDefinition`
2. `ArtifactSynergyEffectDefinition`

두 타입 모두 기존 `ApplyNodes`와 Reaction scheduler로 보낸다. 공통 인터페이스, 별도 시너지 계산기, 숨은 Passive Definition은 추가하지 않는다.

## 7. Stage 적용 순서

```text
StageManager.RunCurrentDayFlow
  1. 파티 유닛 복원/등록
  2. ArtifactSynergyManager.PrepareStage
     a. 모든 파티원의 ActiveArtifactEffectNames 초기화
     b. 전체 보유 유물의 synergy_name 집계
     c. 개별 유물 Effect 배포
     d. 달성한 2/4/6/8 시너지 Effect를 누적 배포
     e. SpawnUnit/GrantSkill 계열만 소환 활성화 경로 실행
  3. InGameCombatManager.BeginPlayerCombat
  4. 적 생성
```

시너지 `SkillModifier`와 `PassiveTrigger` 배포는 `spawnManager` 유무에 의존하지 않는다. `SpawnUnit`과 `GrantSkill`만 소환 경로에 남긴다.

## 8. 치명타와 최종 피해 공통 설계

### 8.1 배율 분리

- 현재 선치명타 `FinalDamageMultiplier`를 실제 의미인 `DamageMultiplier`로 이름 변경한다.
- `SkillExecutionState`와 `AttackRule`에 기본값 `1f`인 `FinalDamageModifier`를 추가한다.
- 치명타일 때만 적용되는 처형관 4시너지용 `CriticalFinalDamageModifier`도 기본값 `1f`로 둔다.
- 여러 최종 피해 보정은 합산하지 않고 곱한다.

### 8.2 계산 순서

```text
RawDamage
-> 방어력
-> DamageMultiplier / 주는 피해
-> 받는 피해
-> 치명타 판정
-> 치명타 피해 배율
-> FinalDamageModifier
-> 치명타였으면 CriticalFinalDamageModifier
-> Mathf.Round
-> 보호막 / HP 차감
```

`DamageCalculator`는 전달받은 값만 계산한다. 대상 조건과 유물 상태 조회는 중앙 피해 요청 경로에서 `AttackRule`을 만들기 전에 끝낸다.

### 8.3 치명타 결과 전달

- 기존 float 반환 호출을 깨지 않도록 호환 overload를 유지한다.
- 전투 Manager 호출은 `out bool isCritical`을 받는다.
- `InGameResourceChangeResult.IsCritical`에 기록한다.
- `TriggerExecutionContext.EventWasCritical`로 전달한다.
- 날 선 성배와 운명의 동전은 이 공통 결과를 사용한다.

### 8.4 백은 바늘 치명타 피해 합성

- `SkillExecutionState`에 `MagazineLastProjectileCritDamageBonus`를 기본값 `0f`로 추가한다.
- `MagazineLastProjectileCritDamageBonus` Node는 기존 `CritDamageBonus`와 같은 보너스율 형식을 사용한다.
- `ProjectileSkillActor`는 `isMagazineLastProjectile`일 때만 기본 `critDamageBonus`와 이 값을 기존 치명타 피해 곱연산으로 합성한 뒤 `ApplyDamage`에 전달한다.
- 곱연산 식을 중복 작성하지 않도록 기존 `CritDamageBonus` 합성식을 작은 공통 메서드로 추출해 Node 적용과 마지막 투사체 적용이 함께 사용한다.
- `DamageCalculator`는 최종 전달값만 계산하므로 백은 바늘 전용 분기를 추가하지 않는다.

## 9. 공통 적중 조건

처형관 조건을 Single Actor에만 넣지 않는다. 모든 Line, Projectile, Single, Zone 피해가 통과하는 중앙 `ApplyDamage` 경계에서 확정한다.

필요 입력:

- source
- target
- source skill name와 `SkillRuntimeKind`
- target 현재 체력 비율
- `target.IsBoss`
- 현재 생존 적 roster
- 투사체의 기존 `isMagazineLastProjectile`
- 운명의 동전 누적값

최고 체력 판정은 현재 `SkillTargeting.HighestHealth`와 같은 절대 `CurrentHealth` 기준을 사용한다. 동률 대상은 모두 조건을 만족한 것으로 처리해 Registry 순서 때문에 같은 HP 대상의 보너스가 임의로 빠지지 않게 한다.

## 10. 유물 보상 획득 설계

### 재사용

- `RunSession.HasArtifactCapacity`
- `RunSession.HasArtifact`
- `RunSession.CanAcquireArtifact`
- `RunSession.TryAcquireArtifact`
- `PrisonPanelUI.OpenArtifactAcquisition`
- 유닛당 최대 유물 3개와 같은 Artifact ID 중복 금지

### 변경

- `ArtifactUI.PrepareChoices`의 단일 `RewardSynergyName` 조건을 제거한다.
- 현재 실제 구현 대상인 `spirit-contract`와 `executioner`만 후보로 허용한다.
- 데이터만 있고 실행되지 않는 `chosen-one`, `sentinel`, `artillery` 유물은 보상 후보에 노출하지 않는다.
- 후보 섞기와 최대 3개 선택지는 기존 코드를 유지한다.

## 11. HUD 다중 시너지 컨테이너 설계

### 11.1 그룹 기준

- 파티 전체 보유 유물을 `ArtifactDefinition.SynergyName`으로 그룹화한다.
- 같은 `synergy_name` 유물은 같은 컨테이너의 보유 수만 증가한다.
- 서로 다른 `synergy_name`마다 컨테이너 한 개를 사용한다.

### 11.2 생성과 배치

- 기존 `HUD/Artifact_Container`를 첫 슬롯이자 복제 원본으로 사용한다.
- `InGameInfoUI`가 원본과 복제본을 목록으로 캐시한다.
- 처음 보는 시너지 종류는 목록 마지막 컨테이너를 복제해 같은 부모에 둔다.
- 새 복제본의 X/Z, rotation, scale은 원본 복제 결과를 유지한다.
- Y만 이전 컨테이너의 `localPosition.y - 93.3f`로 설정한다.
- Refresh마다 컨테이너를 파괴하거나 다시 만들지 않는다.
- 필요한 슬롯만 활성화하고 남는 슬롯은 비활성화한다.

```csharp
clone.localPosition = previous.localPosition + new Vector3(0f, -93.3f, 0f);
```

### 11.3 표시 내용

각 컨테이너는 기존 자식을 상대 경로로 바인딩한다.

- `Image/Icon`: 시너지 icon
- `Image/Cur/Text (TMP) (1)`: 해당 시너지 유물 총수
- `Image/Lv2/Text (TMP) (1)`: 2개 달성 색
- `Image/Lv4/Text (TMP) (1)`: 4개 달성 색
- `Image/Lv6/Text (TMP) (1)`: 6개 달성 색
- `Image/Lv8/Text (TMP) (1)`: 8개 달성 색

표시 순서는 현재 Run에서 시너지가 처음 발견된 순서를 유지한다. 정령계약을 먼저 얻고 처형관을 얻으면 정령계약 원본 아래에 처형관 복제본이 온다.

### 11.4 현재 UI 범위 제한

현재 `Artifact_Container`에는 개별 유물 이름·icon 목록·효과 설명용 자식이 없다. 이 구현 범위는 시너지 icon, 보유 수, 2/4/6/8 달성 표시까지다. 개별 유물 효과 문구까지 HUD에 표시하려면 Scene 계층과 별도 상호작용 설계가 추가로 필요하다.

## 12. 예상 수정 대상

### C#

- `Assets/Scripts/Combat/Artifact/Definition/ArtifactDefinitions.cs`
- `Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Artifacts.cs`
- `Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs`
- `Assets/Scripts/Combat/Artifact/ArtifactState.cs`
- `Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- `Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`
- `Assets/Scripts/Combat/Skills/Execution/SkillExecutionState.cs`
- `Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNodeActions.cs`
- `Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs`
- `Assets/Scripts/Combat/InGameCombatManager.cs`
- `Assets/Scripts/Combat/Damage/DamageCalculator.cs`
- 조건부 대상 치명타를 공통 적용할 Line/Projectile/Single/Zone Actor·Executor
- 백은 바늘의 기존 마지막 탄창 투사체 값을 소비할 `Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs`
- `Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs`
- `Assets/Scripts/UI/InGame/Info/InGameInfoUI.cs`

### CSV

- `Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv`
- `Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv`
- `Assets/CSVdata/Artifact/Effect/artifact_effects.csv`
- `Assets/CSVdata/Artifact/artifacts.csv`
- 기존 공통 `skill_node_definitions.csv`, `skill_node_definition_params.csv`

기존 처형관 artifact/synergy/effect 헤더 행과 icon 경로는 유지한다.

## 13. 구현 Phase

### Phase 1: 시너지 Effect 공통 연결

- `ArtifactSynergyEffectDefinition.Nodes/Reactions`
- Generation graph 연결
- 활성 시너지 `SkillModifier/PassiveTrigger` Stage 배포
- 개별/시너지 Effect 공통 소비

### Phase 2: 치명타·최종 피해 계약

- 선치명타 `DamageMultiplier` 이름 정정
- `FinalDamageModifier`, `CriticalFinalDamageModifier`
- 치명타 결과 반환과 Trigger 전달

### Phase 3: 적중 조건과 상태

- 저체력, 보스, 최고 현재 HP, projectile kind 조건
- 기존 마지막 탄창 투사체의 조건부 치명타 피해
- 운명의 동전 Stage 상태

### Phase 4: 처형관 Node·Trigger 데이터

- 네 시너지 단계
- 개별 유물 10개
- CSV 외래 키, Node 인자, Trigger 조건 검증

### Phase 5: 획득과 HUD

- 정령계약+처형관 보상 후보
- `synergy_name`별 컨테이너 그룹
- 다른 시너지 컨테이너 복제와 Y `-93.3`

### Phase 6: 검증

- Runtime/Editor 빌드
- CSV 구조와 RuntimeCatalog 생성
- 집중 EditMode 테스트
- Unity Play Mode gameplay 검증은 사용자 수행

## 14. 수용 기준

### 데이터·Catalog

- 처형관 유물 10개와 네 단계 Effect가 빈 헤더가 아니라 typed Node/Reaction을 가진다.
- 존재하지 않는 Effect, Node, Trigger, status, skill 참조는 catalog 생성 전에 실패한다.
- 기존 정령계약 데이터와 runtime 동작이 유지된다.

### Stage

- 처형관 유물 0/1개에서는 시너지 단계가 발동하지 않는다.
- 2/4/6/8개에서 해당 단계까지 누적 발동한다.
- 다음 Stage에서 활성 Effect와 운명의 동전 중첩을 초기화하고 정확히 한 번 재구성한다.
- 개별 유물 소유자는 어느 파티원이든 `AllAllies` 효과를 전체 파티에 배포한다.

### 피해

- 최종 피해 +8%는 실제 치명타가 발생한 공격에만 치명타 배율 뒤 적용된다.
- 저체력, 보스, 최고 HP, 표식, projectile 조건이 모든 관련 피해 Actor에서 같은 결과를 낸다.
- 백은 바늘은 `PreparedMagazineLastProjectile=true`인 투사체 직접 피해에만 치명타 피해 `+0.70`을 적용한다.
- 유리 심장은 보유 여부만으로 모든 아군 치명타 피해 `+0.20`을 적용한다.
- 별빛 숫돌은 보유 여부만으로 모든 아군 치명타 확률 `+0.20`을 적용한다.
- Trigger 후속 피해는 운명의 동전 중첩을 변경하지 않는다.

### 획득·HUD

- 정상 보상에서 정령계약과 처형관 유물이 모두 후보가 된다.
- 같은 Artifact ID는 중복 획득할 수 없다.
- 같은 시너지 유물은 한 컨테이너 count에 누적된다.
- 다른 시너지 유물은 새 컨테이너를 사용한다.
- 두 번째 컨테이너 Y는 첫 컨테이너보다 `93.3` 낮다. 세 번째는 두 번째보다 `93.3` 낮다.
- 정령계약과 처형관을 동시에 보유해도 icon, count, 단계 색이 서로 섞이지 않는다.

## 15. Code Builder 검증 요구

- `rg`로 처형관 14개 Effect ID가 Definition과 graph/trigger에 연결됐는지 확인한다.
- 모든 수정 CSV의 header/type/data 열 수를 검사한다.
- `git diff --check`를 통과한다.
- Runtime과 Editor 프로젝트를 빌드한다.
- 집중 EditMode 테스트를 남긴다:
  - 시너지 0/1/2/4/6/8 집계
  - 치명타/비치명타 최종 피해
  - 저체력/보스/최고 HP/표식/projectile/마지막 탄창 투사체
  - 유리 심장/별빛 숫돌 단일 효과 `+0.20`
  - 운명의 동전 누적/상한/초기화
  - 동일 시너지 한 컨테이너, 다른 시너지 Y `-93.3`
- Unity Play Mode에서는 사용자가 실제 보상 획득, 다음 Stage 적용, 두 시너지 HUD를 확인한다.

## 16. 근거 파일

- `Pakuri/reference/4.run/artifact-synergy-list.md`
- `Pakuri/Assets/CSVdata/Artifact/artifact_synergies.csv`
- `Pakuri/Assets/CSVdata/Artifact/artifacts.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_synergy_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv`
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactState.cs`
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs`
- `Pakuri/Assets/Scripts/Combat/Artifact/Definition/ArtifactDefinitions.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Artifacts.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts/Combat/Damage/DamageCalculator.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Info/InGameInfoUI.cs`
