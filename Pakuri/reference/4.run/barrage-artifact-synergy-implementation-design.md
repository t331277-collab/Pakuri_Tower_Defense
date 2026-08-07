# 포격대 시너지·유물 구현 Handoff

## 1. 문서 상태

- 작성일: 2026-08-07
- 작성 롤: Designer
- 구현 담당: Code Builder
- 상태: Code Builder 구현 완료 — Phase 1~4 정적 검증 완료, Code Reviewer 검토 예정
- 범위: 포격대 시너지 2/4/6/8단계와 포격대 유물 10종의 런타임·CSV 구현 방향
- 이번 문서 작성에서는 C#·CSV·Prefab·Scene을 수정하지 않는다.
- 이전 요청의 규칙에 따라 구현 Phase마다 GitHub 커밋한다. 이 설계 Phase에는 커밋하지 않는다.

## 2. 요청 이해

포격대는 탄창형 스킬의 재장전 완료를 사건으로 사용한다.

1. 탄창형 스킬 재장전 시간은 시너지와 유물 modifier로 줄인다.
2. 아군 탄창형 스킬이 재장전을 완료할 때마다 지원 포격 1발을 호출한다.
3. 지원 포격은 새 포격 스킬을 만들지 않고 Nexus가 학습한 Sein-C의 투사체 이동·도착 지연·폭발 실행 경로를 재사용한다.
4. 지원 포격의 발사자·피해 귀속자는 재장전 유닛이 아니라 아군 Nexus다.
5. 주 지원 포격의 착탄 후 폭발 지연은 0.1초다. 시너지 6 파편의 추가 지연은 주 폭발과 별도인 0.3초로 유지한다.
6. 시너지 4/8의 포격 범위·피해 증가는 기존 `RadiusMultiplier`·`DamageMultiplier` 계열의 공통 실행 경로를 재사용한다.
7. 시너지 6은 원래 포격 폭발 뒤 0.3초 후, 착탄점 반경 3 안의 임의 지점 3곳에서 동일한 폭발 실행을 호출한다.
8. 원래 폭발과 각 파편탄은 같은 대상에게 중첩 적중할 수 있다. fragment hit-exclusion은 구현하지 않는다.
7. 유물 문구에 `모든 아군`이 있는 유물만 `AllAllies`; 그 외 유물은 `Owner`다.
8. `artifact_effects.csv`의 `recipient_scope`가 유물 대상 범위의 데이터 원천이다. 런타임에서 설명 문자열을 파싱하지 않는다.

## 3. 확인된 코드 근거

### 3.1 Sein-C

| 근거 | 확인 내용 |
|---|---|
| `Pakuri/Assets/CSVdata/authoring/monster/skills/base/projectile/skills_projectile.csv`의 `sein-c` | `CooldownProjectile`, Fire, 기본 피해 38, 투사체 속도 20, 반경 1.8, 타깃 선택 `Nearest`, cooldown 6.5, damage delay 0.8 |
| `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/projectile/skill_graph_nodes_projectile.csv` | `c-trait-2`가 `RadiusMultiplier 1.25`; `c-trait-1`이 `DamageMultiplier 1.3` |
| `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/projectile/projectile_skill_triger.csv` | `sein-c`에 `OnExpire`·`OnHit` 트리거가 존재 |
| `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs` | 목표점 도착 후 지연을 거쳐 `ExecuteArrivalSkill()`에서 기존 `SingleSkill` 실행 경로를 호출하고, 이후 `OnExpire` lifecycle event를 발행 |
| `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs` | Nexus도 `Skills`·`SkillState`를 가진 공통 전투 모델이다. |
| `Pakuri/Assets/Scripts/Units/Runtime/UnitSkills.cs` | `RebuildLearnedSkillState()`에 active skill definition을 넘기면 소유 유닛의 runtime skill을 구성할 수 있다. |
| `Pakuri/Assets/Scripts/Units/Runtime/Actor/NexusActor.cs` | Nexus 모델과 Nexus 씬 Actor를 연결한다. |

따라서 지원 포격은 `sein-c`를 복제한 별도 스킬이 아니라, Nexus에 학습시킨 `sein-c` runtime을 기존 투사체 Actor와 arrival skill 실행 경로로 호출하는 반응이어야 한다.

현재 `UnitCombatStateFactory.CreateNexus()`는 Nexus 모델을 만들지만 active skill을 학습시키지 않고, `AutoSkillEnabled=false`다. 따라서 Nexus 스킬을 일반 자동 시전 목록에 넣는 방식은 사용하지 않는다. `UnitSpawnManager.RegisterNexus()` 또는 그 하위 factory 경로에서 `sein-c` definition을 Nexus `Skills`에 등록하고 `SkillState.RebuildLearnedSkillState()`를 호출한 뒤, reload reaction이 Nexus entry/runtime을 명시해 사용한다.

단, Sein-C 원본의 속성은 Fire 38이고 지원 포격 요구값은 Physical 60/85다. 단순 `DamageMultiplier`만 적용하면 속성이 Fire로 남는다. 지원 포격 반응에만 적용되는 공통 `damage/attribute override` 또는 동등한 데이터 계약이 필요하다.

### 3.2 이미 재사용 가능한 공통 실행

| 파일 | 확인된 재사용 경로 |
|---|---|
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs` | artifact/synergy `SkillModifier`의 node를 execution data에 적용 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionState.cs` | `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `ShotIntervalMultiplier`, `CritChanceBonus`, `DamageMultiplier`, `RadiusMultiplier`, follow-up projectile 상태 보유 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs` | 탄창 소모, `ReloadRemaining`, 재장전 완료 후 탄창 복구, 반응 예약 경로 보유 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs` | `PassiveTrigger`, `EventSourceScope`, 지연·반복 반응, artifact/synergy 효과 dispatch 보유 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs` | `Densest` 밀집 지점 선택 보유 |
| `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillExecutor.cs` | 투사체 생성과 기존 follow-up projectile 실행 보유 |

### 3.3 현재 존재하지 않거나 그대로는 부족한 계약

다음 항목은 확인된 코드에 없거나 요구사항을 충족하지 못한다. Builder는 이름을 그대로 전제하지 말고 기존 타입에 최소 확장한다.

- `SkillTriggerEvent.OnReloadComplete`가 없다.
- `SkillExecution.Tick()`과 재장전 감소 경로는 탄창만 복구하고 재장전 완료 lifecycle event를 발행하지 않는다.
- `SkillTargeting.Random`는 임의 위치가 아니라 랜덤 적 유닛을 고른다. 착탄점 반경 3 안의 랜덤 지점 생성 기능은 없다.
- 현재 반복 반응은 같은 event center를 재사용한다. `RepeatCount=3`만으로 서로 다른 파편 위치를 만들 수 없다.
- 현재 투사체 follow-up은 burst 전체 뒤 실행될 수 있다. `쌍열 코어`의 확정 조건인 `MagazineRemaining == MaxMagazineSize`를 검사하는 첫 발 의미를 보장하지 않는다.
- `OnMagazineLastProjectileHit`는 피해 적용 뒤 발행되며, 기존 `MagazineLastProjectileCritDamageBonus`는 치명타 피해만 바꾼다. 마지막 탄환의 일반 피해 +60%에는 선행 damage modifier가 필요하다.
- `artifact_effects.csv`의 현재 포격대 유물 10행은 모두 `AllAllies`로 되어 있으나, 사용자 문구 기준 2개만 `AllAllies`여야 한다.
- `artifact_synergy_effects.csv`의 포격대 일부 combat effect가 `Stage`다. 현재 `ArtifactSynergyManager.DistributeSynergyEffects()`는 combat `SkillModifier`·`PassiveTrigger`를 `AllAllies`·`ChosenOne`·`SpecificMonster` 중심으로 배포하며, 해당 `Stage` 행은 전투 효과로 전달되지 않는다.

## 4. 구현 원칙

### 4.1 새 포격 스킬을 만들지 않는다

지원 포격은 Nexus에 학습시킨 기존 `sein-c` projectile definition과 `ProjectileSkillActor.ExecuteArrivalSkill()` 경로를 호출한다.

새로운 포격 전용 Projectile Actor, 새 폭발 전용 Skill Executor, 포격대 전용 Manager를 추가하지 않는다. 지원 포격에 필요한 피해·속성·타깃 선택은 기존 reaction/execution context에 선택적 override를 추가한다.

### 4.2 효과는 데이터가 소유한다

시너지 단계와 유물 수치는 CSV node/reaction이 소유한다. C#에서 artifact ID 또는 synergy ID를 검사하는 분기문을 만들지 않는다.

다음은 공통 계약 후보이며, 현재 코드에 없으므로 구현 시 추가해야 한다.

- 재장전 완료 lifecycle event
- 일반 투사체 modifier용 `SkillRuntimeKind` condition
- 첫 번째 투사체 조건
- 마지막 탄환 일반 피해 multiplier
- reaction 전용 damage attribute/raw damage/target selection override
- Nexus를 reaction caster로 선택하는 공통 `ReactionCasterScope` 또는 동등한 실행 계약
- 필요할 경우 arrival impact lifecycle event

## 5. 시너지 실행 흐름

```text
MagazineProjectile Cast
  -> MagazineRemaining 감소
  -> ReloadRemaining 감소
  -> ReloadRemaining 0 전이 1회
  -> OnReloadComplete(event source = 재장전한 유닛, event skill = 해당 탄창 스킬)
  -> 포격대 PassiveTrigger가 Owner 범위로 자기 이벤트만 수신
  -> Nexus SkillState의 학습 Sein-C runtime 선택
  -> Nexus Transform에서 지원 Sein-C projectile 생성
  -> Densest 위치로 이동
  -> 기존 arrival delay + SingleSkill 폭발
  -> 시너지 6이면 impact 후 0.3초 대기
  -> 반경 3 안 임의 위치 3개에서 동일 폭발 실행
  -> 주 폭발·각 파편 폭발 피해 중첩 허용
```

`AllAllies`로 시너지 효과를 모든 유닛에게 배포하더라도 지원 포격 반응의 `event_source_scope`는 `Owner`로 둔다. 현재 `SkillTrigger.MatchesEventSourceScope()`는 Owner와 event source가 같은 유닛인지 비교할 수 있으므로, 한 유닛의 재장전 완료가 파티원 수만큼 중복 포격되지 않는다.

지원 포격의 실행 소유자와 피해 귀속자는 Nexus로 둔다. reload event source는 발동 조건과 Densest 계산에만 사용하고, 실제 `TryExecuteReaction()`의 `entry`, `runtime`, `snapshot owner`는 Nexus로 교체한다. Nexus가 `sein-c`를 학습하므로 투사체 원점도 Nexus Transform이 된다.

## 6. 시너지 데이터 계약

### 6.1 재장전 시간

사용자 표의 수치는 각 단계의 누적 최종값이다. 현재 synergy level effect가 누적 활성화되고 execution data가 multiplier를 곱하므로, CSV에 0.90/0.82/0.75/0.65를 그대로 모두 넣으면 과도하게 중첩된다.

| 단계 | 목표 최종 재장전 배율 | 해당 단계에 추가할 delta multiplier | 누적 검산 |
|---:|---:|---:|---:|
| 2 | 0.90 | 0.90 | 0.90 |
| 4 | 0.82 | 0.82 / 0.90 = 0.9111111111 | 0.90 × 0.9111111111 ≈ 0.82 |
| 6 | 0.75 | 0.75 / 0.82 = 0.9146341463 | 0.82 × 0.9146341463 ≈ 0.75 |
| 8 | 0.65 | 0.65 / 0.75 = 0.8666666667 | 0.75 × 0.8666666667 ≈ 0.65 |

모든 행에는 `MagazineProjectile` runtime-kind 조건을 둔다. `ReloadTimeMultiplier` 공통 op를 재사용한다.

### 6.2 지원 포격

포격 발동 trigger는 단계 2의 base effect 하나만 둔다. 단계 4/6/8에서 trigger를 다시 만들면 누적 effect 배포 때문에 포격 한 번에 여러 발이 나갈 수 있다.

| 단계 | 계약 |
|---:|---|
| 2 | `OnReloadComplete`, `MagazineProjectile`, Densest target, Physical 60, Nexus에서 발사하는 Sein-C projectile/arrival/explosion, 주 포격 착탄 지연 0.1초 |
| 4 | 같은 지원 포격 profile의 총 피해 85, 기본 폭발 반경에 `×1.15` |
| 6 | 원래 폭발 후 0.3초, 반경 3 내 임의 위치 3곳에서 Physical 30 폭발, 원래 폭발·각 파편과 대상 중첩 허용 |
| 8 | 파편을 제외한 주 지원 포격만 기본 대비 피해·범위 `×2`; 시너지 4의 범위 `×1.15`를 다시 곱하지 않고 100% 증가값으로 대체 |

단계 4/8 modifier는 원본 Sein-C까지 바꾸면 안 된다. 현재 `TargetSkill=sein-c`만으로는 원본 사용과 지원 포격을 구분할 수 없으므로, reaction에 지원 포격 execution tag/profile을 전달하고 해당 tag에만 stage modifier가 적용되게 한다.

원본 `skills_projectile.csv`의 `sein-c` damage delay 0.8초는 유지한다. Nexus 지원 포격 reaction profile에만 0.1초 override를 전달한다.

현재 `sein-c`가 `Nearest`인 것도 확인됐다. 원본 Sein-C를 보존하려면 지원 포격 reaction에만 `Densest` target-selection override를 전달한다. 원본 skill CSV의 `Nearest`를 바꾸는 방식은 사용하지 않는다.

### 6.3 시너지 recipient scope

포격대 시너지 combat effect는 시너지 설명상 모든 아군에게 적용되므로 combat 배포 대상은 `AllAllies`로 통일한다. 현재 `Stage`로 된 포격대 combat rows는 `AllAllies`로 고친다.

## 7. 유물별 구현 계약

현재 `Pakuri/Assets/CSVdata/Artifact/artifacts.csv`에서 확인된 설명과 `artifact_effects.csv` scope 기준이다.

| 유물 | `recipient_scope` | 구현 계약 |
|---|---|---|
| 무한 탄피 | `AllAllies` | `MagazineProjectile` 조건 + `MagazineBonus(1)` |
| 과열 약실 | `Owner` | 탄창을 전부 소모하면 pending flag를 세우고, 다음 재장전 완료 후 해당 소유 유닛의 다음 탄창 피해에 `×1.25`를 1회 적용 |
| 쌍열 코어 | `Owner` | 탄창형 투사체 사용의 첫 번째 projectile에 `FollowUpProjectile` 1개, 피해 `×0.30`; burst 전체 뒤가 아니라 첫 launch 조건 필요 |
| 신속 장전기 | `Owner` | `MagazineProjectile` 조건 + `ReloadTimeMultiplier(0.82)` |
| 관통 깃털 | `AllAllies` | projectile runtime-kind 조건 + `PierceBonus(1)` |
| 난사 도면 | `Owner` | projectile runtime-kind 조건 + `ShotIntervalMultiplier(0.88)` + `DamageMultiplier(0.95)` |
| 축복 화살통 | `Owner` | projectile runtime-kind + Holy 또는 Fire 조건 + `DamageMultiplier(1.18)`; OR 조건을 현재 AND 조건과 혼동하지 않도록 Fire/Holy effect를 분리하거나 공통 Any 조건 추가 |
| 번개 탄창 | `Owner` | Lightning projectile hit, 20% proc, `OnOutgoingDamage` 또는 기존 projectile hit event에서 Shock 1 stack 적용 |
| 처형 탄환 | `Owner` | `MagazineRemaining == 1`인 projectile의 실제 피해 전에 일반 피해 `×1.60` 적용 |
| 회전 약실 | `Owner` | 기존 `SkillRuntimeKindCritBonus` 패턴 + `MagazineProjectile` + `CritChanceBonus(0.10)` |

유물 scope를 런타임에서 표시 문구로 판단하지 않는다. `artifact_effects.csv`를 수정하고, 기존 `ArtifactSynergyManager.DistributeEffects()`의 typed recipient 경로를 재사용한다.

### 7.1 과열 약실 상태

현재 코드에서 “탄창을 모두 소모한 뒤 다음 재장전 완료”를 직접 표현하는 유물 one-shot 상태는 확인되지 않았다. `EveryCount`만 사용하면 전체 탄창 소모 조건을 보장하지 못한다.

최소 구현은 다음과 같다.

1. 마지막 탄환을 성공적으로 소비해 `MagazineRemaining == 0`이 되는 순간 pending을 기록한다.
2. 동일 skill runtime의 재장전 완료에서 pending을 소비한다.
3. 재장전 뒤 시작하는 다음 탄창의 실제 탄환 피해에만 `×1.25`를 적용한다.
4. 적용이 끝나면 pending/one-shot 상태를 제거한다.

“다음 재장전 완료 시 위력”이 지원 포격에도 적용되는지는 사용자 확인이 필요하다. 기본 설계는 소유 유닛의 해당 탄창형 스킬 피해에만 적용한다.

### 7.2 축복 화살통의 OR 조건

현재 `ArtifactCombatRules.ConditionsMatch()`는 조건 목록을 AND로 평가하는 경로다. 한 node에 Holy와 Fire를 함께 넣으면 둘 다인 스킬만 통과할 위험이 있다.

최소안은 하나의 유물에 Fire용 effect row와 Holy용 effect row를 따로 두고 각각 `Owner`로 배포하는 것이다. 공통 `AnySkillAttribute`가 이미 없다면 포격대 전용 OR 분기를 만들지 않는다.

## 8. 파편탄 실행 계약

### 8.1 폭발 위치

- 기준점: 원래 Sein-C 지원 포격의 `arrivalCenter`
- 주 포격 착탄 지연: 0.1초
- 파편 추가 지연: 원래 주 폭발 실행이 끝난 뒤 0.3초
- 개수: 정확히 3개
- 위치: 기준점 주변 반경 3 이내의 임의 위치
- 실행: 직접 `ApplyDamage`하지 않고 같은 Sein-C arrival/explosion skill execution 호출
- 적이 없으면 지원 포격 및 파편은 실행하지 않는다. origin으로 임의 발사하지 않는다.

`SkillTargeting.Random`는 랜덤 적을 고르는 기능이므로 파편 위치에 재사용하지 않는다. 기존 Unity random을 이용한 공통 `RandomPointAround(center, radius)` 또는 동등한 helper가 필요하다.

### 8.2 피해 중첩

fragment execution마다 독립적인 기존 Sein-C 폭발 실행을 호출한다. 별도 공유 `HashSet`이나 hit-exclusion context를 전달하지 않는다.

- 원래 폭발이 맞힌 대상도 fragment가 다시 맞힐 수 있다.
- fragment 1이 맞힌 대상도 fragment 2/3이 다시 맞힐 수 있다.
- 기존 `ProjectileSkillActor.TryApplyBranchDamage()`의 한 branch 내부 중복 방지는 이번 규칙과 별개인 기존 branch 동작이므로 포격 fragment 구현에 새 exclusion을 추가하지 않는다.

## 9. 필요한 파일과 변경 표면

아래는 확인된 실행 경로를 기준으로 한 최소 변경 후보이다. 실제 Builder는 기존 partial/definition 구조를 먼저 확인하고 불필요한 파일은 건너뛴다.

| 파일 | 변경 목적 |
|---|---|
| `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNodeConditions.cs` | `OnReloadComplete`, runtime-kind/첫 projectile/last projectile 조건 또는 필요한 공통 enum 확장 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs` | 재장전 완료 전이를 한 곳에서 감지하고 event를 정확히 1회 발행; first projectile marker와 pending overheat 전달 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs` | reload event dispatch, Owner scope 수신, Nexus caster로 support barrage reaction, impact 후 delayed fragment batch |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionState.cs` | Nexus에 학습된 Sein-C runtime, pending overheat, first projectile/last projectile execution data 등 runtime 상태 확장 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs` | runtime-kind modifier, 마지막 탄환 일반 피해, support profile override 적용 |
| `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillExecutor.cs` | 첫 projectile follow-up 및 reaction execution context 전달 |
| `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs` | 0.1초 주 폭발 지연과 arrival impact context 전달; 현재 `ExecuteArrivalSkill()`·`OnExpire` 순서 검증 |
| `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs` | Densest override와 임의 위치 helper. 이미 있는 Densest 선택은 재사용 |
| `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs` | 새 node operation/condition CSV parse |
| `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.cs` | `OnReloadComplete` 및 reaction override 데이터 parse/검증 |
| `Pakuri/Assets/Scripts/Units/Runtime/UnitSkills.cs` | Nexus가 학습한 Sein-C definition을 `SkillState` runtime으로 구성 |
| `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs` | 별도 Nexus 모델 타입을 만들지 않고 기존 `Nexus` role의 Skills/SkillState 사용 |
| `Pakuri/Assets/Scripts/Units/Runtime/Actor/NexusActor.cs` | 실제 발사 원점 Transform 확인. 일반 스킬 실행은 이 Actor가 아니라 모델 registry entry를 사용 |
| `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitCombatStateFactory.cs` | `CreateNexus()`가 학습 skill definition을 받아 `Skills`와 `SkillState`를 초기화하도록 확장 |
| `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs` | `RegisterNexus()`에서 `sein-c` definition을 resolve해 Nexus factory에 전달 |
| `Pakuri/Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs` | 현재 후보 allowlist가 `spirit-contract`, `executioner`, `chosen-one`, `sentinel`만 포함하므로 포격대 노출 여부 확인 후 `artillery` 추가 |
| `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_effects.csv` | 유물 10종 scope 정정 |
| `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_synergy_effects.csv` | 포격대 combat effect의 `Stage`를 `AllAllies`로 정정하고 profile/수치 데이터 추가 |
| `Pakuri/Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv` | 포격대 유물 modifier graph rows 추가 |
| `Pakuri/Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv` | reload, lightning, overheat 등 passive reaction rows 추가 |
| `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs` | CSV catalog와 공통 runtime contract 최소 회귀 검사 |

`DamageCalculator.cs`는 포격대만을 위해 수정하지 않는다. 지원 포격은 공통 `ApplyDamage` 경로를 사용하고 Physical 속성/피해값을 execution context에서 제공한다.

쌍열 코어의 first-projectile marker는 `TryBeginCast()`가 현재 탄창을 감소시키기 전, `runtime.MagazineRemaining == runtime.MaxMagazineSize`를 검사해 snapshot에 기록한다. burst 내부 후속 projectile에는 이 marker를 다시 세우지 않는다.

## 10. 데이터 예시 계약

아래 ID는 구현 시 사용할 수 있는 제안 이름이다. 현재 존재한다고 주장하는 이름이 아니다.

```text
artillery-level-1-support-bombardment
  application_mode = PassiveTrigger
  recipient_scope = AllAllies
  trigger_event = OnReloadComplete
  event_skill_runtime_kinds = MagazineProjectile
  event_source_scope = Owner
  outcome_skill = sein-c
  execution_profile = SupportBarrage
  target_selection_override = Densest
  damage_attribute_override = Physical
  raw_damage_override = 60

artillery-level-2-support-bombardment
  application_mode = SkillModifier/ProfileModifier
  recipient_scope = AllAllies
  execution_profile = SupportBarrage
  radius_multiplier = 1.15
  support_damage = 85

artillery-level-3-shrapnel
  application_mode = SkillModifier/ProfileModifier
  recipient_scope = AllAllies
  execution_profile = SupportBarrage
  fragment_count = 3
  fragment_delay_seconds = 0.3
  fragment_radius = 3
  fragment_raw_damage = 30
  fragment_damage_overlap = true

artillery-level-4-support-bombardment
  application_mode = SkillModifier/ProfileModifier
  recipient_scope = AllAllies
  execution_profile = SupportBarrage
  damage_multiplier = 2
  radius_multiplier = 2
```

이 예시는 현재 CSV에 존재하는 완성 schema가 아니다. 특히 `execution_profile`, `target_selection_override`, `raw_damage_override`는 현재 타입/CSV에서 확인되지 않았으므로 Builder가 공통 schema를 먼저 확정해야 한다.

## 11. Phase 계획과 커밋

### Phase 1 — 데이터 scope와 공통 계약

- `artifact_effects.csv` scope를 사용자 규칙대로 정정
- 포격대 synergy combat effect의 `Stage` 정정
- reload event, runtime-kind condition, support profile, first/last projectile 계약 확정
- CSV parser/catalog 검증 추가
- 커밋: `feat: define artillery synergy and artifact data contracts`

### Phase 2 — 재장전·탄창 공통 런타임

- 재장전 완료 전이 1회 event
- 누적 재장전 multiplier
- infinite shell, rapid loader, piercing feather, barrage blueprint, revolving chamber
- overheat pending state와 last projectile normal damage
- first projectile follow-up
- 커밋: `feat: add magazine reload artillery modifiers`

### Phase 3 — 지원 포격과 파편

- Nexus에 Sein-C 학습 및 Nexus entry/runtime 실행
- Sein-C projectile/arrival 재사용
- Densest target override
- Physical 60/85 및 stage profile
- 주 포격 착탄 지연 0.1초
- impact 후 0.3초 fragment execution 3개
- random point 3개와 피해 중첩 허용
- 커밋: `feat: add artillery support bombardment`

### Phase 4 — 속성 유물·노출·검증

- blessed quiver OR data
- lightning magazine shock proc
- ArtifactUI artillery allowlist
- catalog/runtime tests 및 Unity 검증
- 커밋: `feat: complete artillery artifact effects`

각 Phase 종료 시 해당 Phase의 검사 결과를 기록하고 즉시 커밋한다. 커밋 전 기존 사용자 변경 `Pakuri/reference/4.run/artifact-synergy-list.md`를 덮어쓰지 않는다.

## 12. 수용 기준

- 재장전 시간 단계 2/4/6/8 최종값이 각각 90/82/75/65%다.
- 탄창형이 아닌 스킬에는 재장전 modifier가 적용되지 않는다.
- 한 유닛의 reload completion 하나당 Nexus가 지원 포격을 정확히 1발 발사한다.
- Nexus는 `sein-c`를 학습하고 현재 Sein-C의 투사체→0.1초 도착 지연→폭발 경로를 사용한다.
- 지원 포격은 원본 Sein-C의 `Nearest`를 변경하지 않고 Densest 위치를 사용한다.
- 지원 포격의 기본/4단계 피해와 속성이 각각 Physical 60/85다.
- 6단계는 원래 폭발 뒤 0.3초에 반경 3 안에서 3회 폭발한다.
- 원래 폭발·각 파편탄은 동일 대상에게 중첩 적중할 수 있다.
- 8단계는 파편을 제외한 주 포격의 기본 피해·범위를 각각 2배로 하며, 1.15배를 추가 곱하지 않는다.
- `무한 탄피`, `관통 깃털`만 유물 `AllAllies`다.
- 나머지 8개 포격대 유물은 `Owner`다.
- lightning magazine은 20% proc일 때만 Shock 1 stack을 적용한다.
- execution round는 마지막 탄환의 실제 일반 피해에 +60%를 적용한다.
- 쌍열 코어는 burst 전체 종료 후가 아니라 첫 projectile에 추가 투사체를 만든다.
- 원본 Sein-C와 포격대 지원 포격의 modifier가 서로 오염되지 않는다.

## 13. 구현 전 사용자 확인 필요

아래는 코드에서 안전하게 추론할 수 없어 Builder가 임의로 확정하면 안 되는 항목이다.

1. 기존 `sein-c`를 Nexus가 그대로 학습·사용한다. 별도 projectile/arrival 스킬 복제는 하지 않는다.
2. 주 포격 착탄 지연은 0.1초, 시너지 6 파편 추가 지연은 0.3초로 분리한다.
3. 쌍열 코어의 첫 번째 투사체는 `MagazineRemaining == MaxMagazineSize`일 때의 첫 발이다.
4. 포격대 유물을 `ArtifactUI` 보상 후보에 포함한다. `ArtifactUI.PrepareChoices()` 기존 allowlist에 `artillery`를 추가한다.

위 1~4는 사용자 구현 요청 범위와 실제 Phase 1~4 변경에 반영했다.

## 14. 근거 파일

- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/projectile/skills_projectile.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/projectile/skill_graph_nodes_projectile.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/projectile/projectile_skill_triger.csv`
- `Pakuri/Assets/CSVdata/Artifact/artifacts.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Effect/artifact_synergy_effects.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/skill_graph_nodes_artifact.csv`
- `Pakuri/Assets/CSVdata/Artifact/Skill/artifact_skill_triger.csv`
- `Pakuri/Assets/Scripts/Combat/Artifact/ArtifactSynergyManager.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionState.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs`
- `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs`
- `Pakuri/Assets/Scripts/Units/Runtime/UnitSkills.cs`
- `Pakuri/Assets/Scripts/Units/Runtime/Actor/NexusActor.cs`
- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitCombatStateFactory.cs`
- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs`
