# Skill Trigger Executor Reuse Handoff

## Task title

Trigger는 발동 조건만 판단하고, 실제 스킬 효과는 기존 계열 Executor로 실행한다.

## Goals

- `SkillTrigger`를 전투 사건, 조건, 확률, 내부 쿨다운, 지연과 반복의 단일 판정자로 유지한다.
- Trigger의 피해, 상태, 보호막, 회복, Visual 실행을 기존 `Single`, `Zone`, `Line`, `Projectile`, `Buff` Executor 경로로 통합한다.
- `SkillExecution.TryExecuteTriggered`를 Trigger 스킬 효과의 공통 진입점으로 사용한다.
- `SkillNodeExecutor.cs`와 Trigger runtime Node 직접 실행 경로를 삭제한다.
- Trigger 작성 Node는 Generation 입력으로만 사용하고 runtime에는 최종 `SkillDefinition` 또는 강타입 상태 명령만 전달한다.
- 새 C# script를 만들지 않고 현재 script를 수정하거나 삭제한다.

## Constraints

- 구현 롤은 Code Builder의 Structure + Refactoring + Implementation + Verification 트랙이다.
- 이 문서는 Designer 핸드오프이며, 2026-07-29 사용자 승인 뒤 Code Builder 구현 기준으로 사용한다.
- 현재 Trigger ID, source skill ID, Choice/status 조건, 확률, 내부 쿨다운, 지연, 반복, 정렬 순서와 asset 경로를 보존한다.
- Trigger 조건을 `SkillExecution`이나 각 family Executor에 중복 구현하지 않는다.
- family Executor는 실행 요청이 일반, 수동, 자동, Trigger 중 어디서 왔는지 몰라도 동일하게 동작해야 한다.
- Trigger로 실행되는 스킬은 일반 시전 쿨다운·탄창을 소비하지 않는다. Trigger 자체의 내부 쿨다운과 횟수 제한을 사용한다.
- delayed Trigger는 발생 당시 `SkillActionContext` 값을 유지한다.
- Trigger 재귀와 Zone recast는 현재보다 약한 제한을 사용하지 않는다.
- Unity Play Mode 검증은 사용자 소유다.
- Code Reviewer 실행은 사용자 명시 승인 뒤 한 번만 가능하다.

## Role Owner

Designer for this handoff. Code Builder for implementation after explicit role assignment.

## Status

설계 보완 및 구현 승인 완료. Code Builder Phase 1 기준선 완료, Phase 2 진행 예정.

## Current inspected evidence

### Runtime scripts

- `Combat/Skills/Reactions/SkillTrigger.cs`는 현재 811줄이다.
- `Combat/Skills/Execution/SkillNodeExecutor.cs`는 현재 1,037줄이다.
- `Combat/Skills/Execution/SkillExecution.cs`는 현재 1,635줄이다.
- `SkillNodeExecutor.Execute`와 `HasRuntimeActions`의 실제 호출자는 `SkillTrigger.cs` 한 파일뿐이다.
- `SkillTrigger.TryExecuteOwnedNodes`는 `SkillTriggerDefinition.Nodes`를 `SkillNodeExecutor`에 전달한다.
- `SkillNodeExecutor.ExecuteSkill`은 이미 `SkillExecution.TryExecuteTriggered`를 호출한다.
- `SkillExecution.ExecuteSkill`은 concrete Definition을 기존 Executor에 전달한다.
  - `ProjectileSkillDefinition -> ProjectileSkillExecutor`
  - `LineSkillDefinition -> LineSkillExecutor`
  - `SingleSkillDefinition`, `SingleChainSkillDefinition`, `SingleChargeSkillDefinition -> SingleSkillExecutor`
  - `ZoneSkillDefinition -> ZoneSkillExecutor`
  - `BuffSkillDefinition -> BuffSkillExecutor`
  - `BuffShieldSkillDefinition -> BuffShieldSkillExecutor`
  - `BuffHealSkillDefinition -> BuffHealSkillExecutor`

### Event publishers

Trigger 사건은 family Executor 안에서만 발생하지 않는다.

- `InGameCombatManager`가 shield absorb/expire, kill, combat start, status expire, outgoing damage 사건을 발행한다.
- `SkillExecution`이 BuildExecutionData, OnCast, OnSkillCast 사건을 발행한다.
- Projectile, Line, Single, Zone Executor/Actor가 OnHit, OnExpire, OnHitCount, OnDeploymentCast 사건을 발행한다.

따라서 Trigger 조건 판정 책임을 `SkillExecution` 또는 family Executor로 흩어 놓을 수 없다.

### Active authoring data

현재 여섯 `skill_graph_nodes_*.csv`와 다섯 monster Trigger CSV의 실제 집계:

- 실제 Trigger: 158개
- Trigger-owned Node: 606행
- 기존 스킬을 호출하는 `ExecuteSkill`: 4개
  - `eve-g-auto-prism-ray -> eve-b`
  - `sein-g-auto-barrage-base -> sein-b`
  - `sein-g-auto-barrage-trait1 -> sein-b`
  - `sein-g-auto-barrage-trait2 -> sein-b`
- 직접 delivery 결과를 가진 Trigger: 51개
  - `ApplyDamage`: 27개
  - `ApplyStatus`: 21개
  - `ApplyShield`: 3개
- `RecastZone`: 1개
- 비스킬 상태 명령만 가진 Trigger: 21개
  - `RefundCooldown`: 14개
  - `ReduceReload`: 6개
  - `ExtendStatusDuration`: 1개
- `SkillNodeExecutor.HasRuntimeActions`가 인정하는 즉시 행동이 없는 modifier 중심 Trigger: 81개
- 한 Trigger owner가 서로 다른 delivery 종류를 둘 이상 가지는 경우: 0개
- 한 Trigger owner가 delivery 결과와 비스킬 상태 명령을 함께 가지는 경우: 0개

현재 데이터는 Trigger 하나를 하나의 최종 delivery 결과 또는 하나의 상태 명령으로 옮길 수 있는 형태다.

### Event-derived values

일부 Trigger 피해는 정적 스킬 수치가 아니라 사건 당시 값에서 계산된다.

- `ariel-b-trait4-shield-expire`: `ShieldAppliedAmount * 0.6`
- `ariel-b-master2-shield-absorb-reflect`: `ShieldAbsorbedAmount * 0.35`
- `rin-f-followup`: `EventAppliedDamage * 0.35`
- `rin-f-followup-trait2`: `EventAppliedDamage * 0.35`
- `rin-f-followup-lightning-trait3`: `EventAppliedDamage * 0.105`
- `sein-a-master2-hit-explosion`: `EventAppliedDamage * 0.5`
- `ariel-d-master2-mark-expire-burst`: `TrackedIncomingDamage * 0.2`

기존 family Executor 재사용 시 이 값은 `SkillActionContext`에서 한 번 스냅샷으로 읽어 `SkillExecutionData`에 전달해야 한다.

### Current direct-damage targeting behavior

- 직접 `ApplyDamage` 27개는 현재 모두 `SkillNodeExecutor.ExecuteDamage`가 즉시 한 번 적용한다.
- 작성 형태는 `Single` 9개, `Circle` 17개, `Battlefield` 1개다.
- 현재 Node 경로는 `Circle`의 `Radius`를 `SkillTargetingSpec`에 기록하지만 실제 대상 목록을 반경으로 거르지 않는다.
- `Circle + CoverAll=false + tick_interval_seconds=0.5`인 다음 3개도 현재는 반경 제한과 지속 Tick 없이 진영 대상에게 즉시 한 번 적용된다.
  - `sein-c-master-1`
  - `sein-d-master-2`
  - `sein-e-master-2`
- 동작 보존 구현은 27개 모두 `SingleSkillDefinition`으로 만들고, 위 3개도 현재 대상 집합을 유지하도록 `CoverAll`을 보정한다.
- 위 3개의 작성 반경과 지속 Tick 복구는 별도 gameplay 동작 변경 승인을 받기 전 활성화하지 않는다.

## Core decision

```text
Trigger = 언제 실행하는가
SkillDefinition = 무엇을 실행하는가
SkillExecution = 확정된 실행 요청의 공통 진입점
Family Executor = 계열 효과 구현
```

Trigger 조건을 Node나 family Executor에 넘겨 다시 판단하지 않는다.

```text
전투 사건
  -> SkillTrigger
     - event match
     - Choice/status/passive 조건
     - proc chance
     - internal cooldown/count
     - delay/repeat
  -> 결과 확정
     ├─ 스킬 결과
     │   -> SkillExecution.TryExecuteTriggered
     │   -> 기존 family Executor
     └─ 비스킬 상태 명령
         -> 기존 SkillUseState/Status runtime API
```

## Why `SkillTrigger.cs` remains

`SkillTrigger.cs`는 효과 Executor가 아니라 전투 사건의 반응 라우터다.

유지 책임:

- 전투 사건 수신
- source-owned/passive-owned Trigger 검색
- Choice/status/event skill/runtime-kind/attribute/source-scope 조건 판정
- proc chance
- internal cooldown과 trigger count
- delay와 repeat
- immutable event context 유지
- 확정된 스킬 또는 상태 명령을 기존 실행 API에 위임

삭제 책임:

- Trigger Node 대상 선택
- Trigger Node 피해 계산과 적용
- Trigger Node 상태·보호막 적용
- Trigger Node Visual 생성
- Trigger Node skill/recast/cooldown/reload dispatch

이 로직을 `SkillExecution.cs`에 합치면 파일만 없어지고 조건 판정 책임은 그대로 남으며, shield/status/combat-start 같은 외부 사건까지 `SkillExecution`이 소유하게 된다. 이는 통합이 아니라 책임 혼합이다.

## Why `SkillNodeExecutor.cs` is deleted

- 현재 runtime 호출자는 `SkillTrigger.cs`뿐이다.
- Choice와 기본 스킬 modifier Node는 `SkillExecutionData.ApplyNodes`가 직접 처리한다.
- Trigger delivery를 family Executor로 이관하면 ordered runtime Node dispatcher가 필요하지 않다.
- cooldown/reload/status-duration 명령은 기존 runtime API에 직접 위임할 수 있다.
- 새 대체 Executor script는 만들지 않는다.

## Final runtime contracts

### `SkillTriggerDefinition`

유지:

- Trigger ID와 source skill
- Trigger event
- required/excluded Choice
- source/event status 조건
- event skill ID와 RuntimeKind 조건
- trigger attribute와 event source scope
- proc chance
- internal cooldown
- trigger every count
- delay/repeat
- sort order

변경:

- runtime `SkillNode[] Nodes` 삭제
- Generation이 확정한 final triggered `SkillDefinition` 참조 또는 강타입 상태 명령을 보관
- 하나의 Trigger는 스킬 결과와 상태 명령을 동시에 보관하지 않는다.
- raw 문자열이나 generic key/value command bag을 추가하지 않는다.

### Triggered `SkillDefinition`

- 기존 스킬 재사용이 가능한 4개 Trigger는 catalog의 같은 final Definition 참조를 사용한다.
- 직접 delivery Trigger는 Generation이 기존 concrete Definition 타입으로 만든다.
  - `ApplyDamage 27개 -> SingleSkillDefinition`
  - `ApplyStatus -> BuffSkillDefinition`
  - `ApplyShield -> BuffShieldSkillDefinition`
  - `RecastZone -> 기존 ZoneSkillDefinition 재사용`
- `SingleSkillDefinition`은 `Single`, 즉시 원형, 즉시 전장 형태를 모두 표현한다.
- 직접 피해는 `CriticalAllowed=false`이며 현재 `Radius`, `CoverAll`, `MaxTargets`, Visual을 보존한다.
- 직접 피해의 `SetDuration`은 피해 지속시간이 아니라 `Area.Duration`을 통한 Visual 수명으로만 사용한다.
- 현재 무시되는 `tick_interval_seconds`는 별도 승인 전 계속 무시한다.
- Trigger 전용 Definition은 learned/automatic skill 목록에 등록하지 않는다.
- Trigger 전용 Definition의 gating은 `SkillTrigger`가 담당하고 일반 cast cooldown/magazine은 사용하지 않는다.

### Dynamic event value

- 현재 `NodeDamageValueSource`가 표현하는 사건 기반 값은 Trigger의 final damage contract로 이동한다.
- `SkillTrigger`는 지연 예약 전에 사건값으로 최종 raw damage를 한 번 계산한다.
- `SkillExecutionData.HasRawDamageOverride`와 `RawDamageOverride`가 per-cast 확정값을 보관한다.
- `SingleSkillExecutor`는 override가 있으면 그 값을 사용하고, 없으면 기존 `DamageCalculator`를 사용한다.
- 지연 뒤 실제 적중해도 당시 Trigger 사건값이 바뀌지 않아야 한다.
- family Executor는 Trigger 조건을 읽지 않고 확정된 실행 스냅샷만 사용한다.

### Trigger-only runtime and source identity

- 직접 delivery는 실행 때 `new SkillUseState(source, triggeredDefinition)`으로 임시 state를 만든다.
- 임시 state는 `source.SkillState`, learned/automatic 목록, UI 또는 catalog lookup에 등록하지 않는다.
- Actor가 필요한 경우에만 해당 실행의 임시 state를 보관한다.
- 기존 `ExecuteSkill` 4개는 호출 대상의 실제 `SkillUseState`와 그 스킬 snapshot을 사용한다.
- 직접 delivery 51개는 Trigger source skill의 기존 `SkillUseState`에서 snapshot을 만든다.
- hidden Definition ID는 실행 Definition 식별자이고, 피해와 Trigger 사건의 source skill ID는 원본 snapshot의 skill ID다.

### Event target and per-target predicates

- `SkillExecutionContext`는 `EventTarget`, `LockToEventTarget`, 사건 중심 정책을 보관한다.
- `LockToEventTarget`이면 해당 유닛만 반환하며, 대상이 없거나 죽었으면 다른 유닛으로 대체하지 않는다.
- `EffectTarget` 중심은 실행 시점 대상 위치를 사용하고, `PrimarySkillCenter`는 사건 당시 `EventCenter`를 사용한다.
- 직접 delivery의 대상 조건은 Trigger 전체 gate로 올리지 않고 대상 선택 뒤 각 대상에 적용한다.
- 현재 직접 delivery 조건 4개는 final `SkillTargetingSpec`으로 변환한다.
  - `ConditionStatus` 3개 -> `SelectionStatusKind`, `SelectionStatusMinStacks`
  - `ConditionSkillAttribute` 1개 -> `HasSelectionSkillAttribute`, `SelectionSkillAttribute`
- `DamageAttribute.Physical`이 유효한 기본 enum 값이므로 속성 조건 존재 여부를 별도 bool로 보관한다.

### Lifecycle and recursion

- `SkillExecutionContext.PublishSkillLifecycleEvents`는 skill lifecycle 발행 여부를 보관한다.
- 기존 `ExecuteSkill` 4개는 `true`, 직접 delivery 51개는 `false`다.
- `false`는 `BuildExecutionData`, `OnCast`, `OnSkillCast`, `OnHit`, `OnHitCount`, `OnDeploymentCast`, `OnExpire`만 억제한다.
- `OnOutgoingDamage`, `OnKill`, shield/status 사건처럼 `InGameCombatManager`가 발행하는 전투 사건은 억제하지 않는다.
- 하위 Executor가 새 context를 만들거나 Actor에 context를 넘길 때 이 정책과 source skill ID를 그대로 전파한다.
- 현재 `MaxExecutionDepth=8` 제한은 공통 Trigger skill 실행 경로로 이동한다.
- `RecastZone`은 별도의 기존 `MaxGeneration` 제한을 유지하며 정적 Validator가 runtime 제한을 대신하지 않는다.

### Shared status application

- 직접 `ApplyStatus` 21개는 `BuffSkillDefinition`으로 만든다.
- `BuffSkillExecutor`는 다른 family Executor와 동일하게 `StatusCombatRules.ApplyStatus`를 사용한다.
- 상태 확률 보너스, 상태이상 저항, 지속시간 보정, threshold 적용이 기존 Node 경로와 같아야 한다.
- 공용 Buff 경로 변경이므로 기존 일반 Buff의 상태 적용 회귀도 함께 검증한다.

### Non-skill commands

다음은 가짜 Buff/Single Definition으로 만들지 않는다.

- cooldown refund
- reload reduction
- status duration extension

`SkillTrigger`는 조건 통과 후 기존 `SkillUseState` 또는 status runtime API에 강타입 값으로 위임한다. 피해, 상태 적용, 보호막, 회복, Visual 구현은 하지 않는다.

## Current 81 modifier-only Triggers

현재 81개 owner는 `SetDuration`, `SelectTargets`, `StatusModifier`와 modifier Node만 가지며 `SkillNodeExecutor.HasRuntimeActions`를 통과하지 못한다.

구현 시 다음을 명시적으로 처리한다.

- 현재 runtime에서 실제로 실행되지 않는다는 기준선을 테스트로 고정한다.
- 작성 의도가 Buff/Status delivery라면 Generation에서 `BuffSkillDefinition`으로 변환한다.
- 현재 no-op 보존과 작성 의도 복구 중 어느 쪽인지 owner별 근거를 기록한다.
- 확인 없이 81개를 일괄 활성화하여 전투 수치를 바꾸지 않는다.

이 항목은 동작 보존과 누락 복구의 경계이므로 Code Builder가 추측해서 결정하지 않는다.

## Existing script changes

### `Combat/Skills/Definitions/SkillDefinition.cs`

- `SkillTriggerDefinition.Nodes` 삭제
- final triggered Definition 또는 typed non-skill command 계약 추가
- Trigger 조건과 결과 참조를 분리
- Trigger 전용 concrete Definition이 기존 family 타입을 사용하게 한다.

### `Combat/Skills/Definitions/SkillNode.cs`

- Choice/base modifier에서 계속 사용하는 operation은 유지
- Trigger runtime 전용 action operation 삭제
- 사건 기반 damage value enum이 final skill contract로 이동하면 Node 전용 이름을 제거

### `Combat/Skills/Reactions/SkillTrigger.cs`

- 현재 event publisher API는 유지해 호출부 변경을 최소화
- 조건, 확률, cooldown/count, delay/repeat만 판단
- 스킬 결과는 `SkillExecution.TryExecuteTriggered`로 전달
- 상태 명령은 기존 runtime API로 전달
- `TryExecuteOwnedNodes`, `SkillNodeExecutor` 의존과 직접 payload 실행 삭제

### `Combat/Skills/Reactions/PassiveSkill.cs`

- Trigger별 internal cooldown과 count 상태 유지
- 새 실행 책임 추가 없음

### `Combat/Skills/Execution/SkillExecution.cs`

- final `SkillDefinition`과 `SkillActionContext`를 받는 Trigger 실행 경로로 확장
- 기존 `ExecuteSkill` family dispatch 재사용
- Trigger 실행은 일반 cast cooldown/magazine을 소비하지 않음
- Trigger 전용 Definition을 learned/automatic state로 등록하지 않음
- 사건 기반 raw damage/target center를 per-cast snapshot에 확정
- Trigger 조건 판정은 추가하지 않음

### `Combat/Skills/Execution/SkillExecutionData.cs`

- 사건 기반 확정 damage/target override를 per-cast 값으로 보관
- catalog 또는 shared Definition을 변경하지 않음
- Choice modifier Node 적용 책임 유지

### `Combat/Skills/Execution/SkillActionContext.cs`

- 현재 immutable event snapshot 책임 유지
- 새 Trigger context wrapper를 만들지 않음

### Family Executors/Actors

- 기존 concrete Definition 실행 책임 유지
- Trigger 조건 판정 추가 금지
- `SingleSkillExecutor`는 raw damage override와 `Area.Duration` Visual 수명을 공용 실행 값으로 소비
- `BuffSkillExecutor`는 `StatusCombatRules.ApplyStatus`를 공용 상태 적용 경로로 사용
- 정확한 EventTarget과 대상별 predicate는 공용 `SkillTargeting`을 재사용
- lifecycle 발행은 `SkillExecutionContext.PublishSkillLifecycleEvents`를 따름

### `Combat/Skills/Execution/SkillNodeExecutor.cs`

- script와 `.meta` 삭제
- 대체 script 생성 없음

### Loading

#### `Loading/Generation/GameDataCatalogBuilder.cs`

- Trigger-owned authored Node를 final triggered Definition 또는 typed state command로 한 번 변환
- Trigger runtime `SkillNode[]` 생성 중단
- 기존 `ExecuteSkill` ID는 catalog final Definition 참조로 연결
- 직접 delivery Node 묶음은 기존 concrete Definition 타입으로 생성
- modifier-only 81개는 승인된 owner별 결론에 따라 변환 또는 현재 no-op 보존

#### `Loading/Generation/GameDataCatalogBuilder.Nodes.cs`

- Choice/base modifier Node mapping 유지
- Trigger runtime action mapping 제거
- Trigger authoring-to-final outcome 변환에 필요한 최소 읽기 helper만 유지

#### `Loading/Validation/CsvDataValidator.cs`

- Trigger owner가 정확히 하나의 결과 범주를 가지는지 검증
- delivery와 state command 동시 소유 금지
- event-derived value가 지원되는 사건에서만 사용되는지 검증
- triggered skill reference와 source monster 일관성 검증
- recursion/recast 깊이와 delayed context 요구 검증

## Final `Combat/Skills` structure

새 script는 없고 현재 24개에서 `SkillNodeExecutor.cs` 하나가 삭제되어 최대 23개다.

```text
Combat/Skills/
├─ Definitions/
│  ├─ SkillDefinition.cs
│  └─ SkillNode.cs
├─ Runtime/
│  ├─ UnitSkills.cs
│  └─ SkillExecutionData.cs
├─ Execution/
│  ├─ SkillActionContext.cs
│  ├─ SkillExecution.cs
│  ├─ SkillExecutionRuleResolver.cs
│  └─ SkillTargeting.cs
├─ Delivery/
│  ├─ Buff/
│  ├─ Line/
│  ├─ Projectile/
│  ├─ Single/
│  └─ Zone/
└─ Reactions/
   ├─ PassiveSkill.cs
   └─ SkillTrigger.cs
```

## Migration phases

### Phase 1 — Baseline and classification

- 현재 158 Trigger와 606 Trigger-owned Node를 owner별 결과 범주로 고정한다.
- 기존 4 `ExecuteSkill`, 51 direct delivery, 1 recast, 21 state command, 81 modifier-only 목록을 기록한다.
- event publisher와 current no-op owner의 EditMode 기준선을 추가한다.
- owner별 parity에는 다음 열을 기록한다.
  - owner
  - 현재 실행 또는 no-op
  - final family
  - 즉시 또는 지속
  - 실제 대상 규칙
  - 대상별 조건
  - source skill ID
  - skill lifecycle 발행
  - dynamic value source
- 직접 피해의 authored Radius/Tick과 현재 실제 적용 범위가 다르면 둘을 분리 기록한다.

Rollback point: code/data 변경 전 commit.

Phase 1 evidence:

- Trigger 158개, Trigger-owned Node 606행
- action owner 77개, current no-action owner 81개
- `Combat/Skills` C# 24개, 12,102줄
- runtime/editor C# build error 0
- 기존 Unity package 참조의 `System.Net.Http`, `System.IO.Compression` 충돌 warning 2개는 기준선에도 존재

### Phase 2 — Generation final outcomes

- current authored Trigger Node를 final triggered Definition 또는 typed state command로 생성한다.
- 기존 runtime Node 경로를 유지한 채 새 final 결과와 parity를 비교한다.
- event-derived damage와 target center snapshot을 final contract에 추가한다.

Rollback point: Trigger는 기존 Nodes를 계속 실행 가능.

### Phase 3 — Shared triggered execution

- `SkillExecution.TryExecuteTriggered`가 final Definition과 `SkillActionContext`를 직접 받게 한다.
- 기존 family dispatch를 재사용한다.
- 일반 cast resource를 소비하지 않고 Trigger gating만 사용한다.
- Single/Buff/Shield와 기존 4 ExecuteSkill 사례를 순서대로 전환한다.
- 직접 delivery는 lifecycle을 억제하고, 기존 ExecuteSkill은 lifecycle을 유지한다.
- 임시 runtime과 source snapshot을 분리한다.

Rollback point: owner 단위로 기존 Node 실행 유지.

### Phase 4 — Trigger reduction and commands

- `SkillTrigger`를 조건·scheduling·delegation으로 축소한다.
- cooldown/reload/status-duration은 기존 runtime API로 위임한다.
- 81 modifier-only owner는 승인된 결론대로 전환한다.
- Trigger runtime `Nodes` 필드를 제거한다.

Rollback point: unmigrated owner만 기존 Node 실행.

### Phase 5 — `SkillNodeExecutor` deletion

- `SkillNodeExecutor` 호출 0건 확인
- script와 `.meta` 삭제
- Trigger runtime action operation과 Generation mapping 삭제
- 빈 helper와 stale comments 삭제
- final folder/script count와 net line reduction 기록

Rollback point: Phase 4 commit.

### Phase 6 — Full verification

- static search, tests, solution build, Unity compilation, CSV validation 수행
- Trigger family별 data parity 확인
- 사용자 Play Mode 검증 항목 전달
- Phase별 로컬 Git commit을 남긴다.

## Edge cases

- shield/event damage 기반 Trigger 값은 delay 뒤에도 사건 당시 값이어야 한다.
- event target이 delay 중 파괴되면 기존 fallback/cancel 의미를 보존한다.
- Trigger 실행 스킬이 다시 같은 Trigger를 무한 발동하지 않게 한다.
- OnHit와 OnOutgoingDamage를 같은 사건으로 취급하지 않는다.
- status-only hit는 damage 0이어도 필요한 사건을 발행한다.
- OnExpire는 projectile/zone actor당 정확히 한 번 발생한다.
- passive-owned Trigger는 owner/all-allies source scope를 보존한다.
- Trigger 정렬, repeat, repeat interval과 internal cooldown 의미를 보존한다.
- hidden triggered Definition은 UI, 학습 목록, 자동 시전 목록에 나타나지 않는다.
- shared catalog Definition은 runtime에서 변경하지 않는다.

## Acceptance criteria

- 새 C# script가 0개다.
- `Combat/Skills/Execution/SkillNodeExecutor.cs`와 `.meta`가 없다.
- active C# search 결과 `SkillNodeExecutor`가 0건이다.
- `SkillTriggerDefinition`에 runtime `SkillNode[] Nodes`가 없다.
- `SkillTrigger.cs`에 직접 damage/status/shield/visual/target 실행이 없다.
- `SkillTrigger.cs`가 모든 combat/lifecycle Trigger 조건의 단일 판정자다.
- family Executor에 Trigger Choice/status/proc/cooldown 조건 판정이 없다.
- 모든 Trigger delivery가 `SkillExecution.TryExecuteTriggered -> ExecuteSkill -> family Executor` 경로를 사용한다.
- cooldown/reload/status-duration 명령은 기존 runtime API를 사용한다.
- 기존 4 `ExecuteSkill` mapping이 동일한 final Definition을 재사용한다.
- 51 direct delivery, 1 recast, 21 state command가 owner 누락 없이 전환된다.
- 81 modifier-only owner가 승인된 owner별 결과와 일치한다.
- 51 direct delivery의 대상 집합, 적용 횟수, lifecycle 사건 발행 전후가 동일하다.
- 직접 피해 27개가 승인 없이 지속 Tick으로 변하지 않는다.
- `sein-c-master-1`, `sein-d-master-2`, `sein-e-master-2`의 현재 반경 미적용 대상 집합을 유지한다.
- Trigger 전용 임시 runtime이 학습, 자동 시전, UI, catalog 목록에 등록되지 않는다.
- 직접 delivery는 source snapshot을 사용하고 기존 4 ExecuteSkill은 호출 대상 snapshot을 사용한다.
- hidden Definition ID와 피해/Trigger source skill ID가 분리된다.
- 21 상태 delivery의 확률, 저항, 지속시간 계산이 동일하다.
- event-derived 피해 7개와 `ShieldAppliedAmount`, `ShieldAbsorbedAmount`, `EventAppliedDamage`, `TrackedIncomingDamage` 네 source 종류의 수치가 동일하다.
- delay/repeat와 recursion 제한이 유지된다.
- `TryExecuteTriggered` 깊이 8과 `RecastZone.MaxGeneration` 제한이 삭제 뒤에도 유지된다.
- `Combat/Skills` C# script 수가 23개 이하이다.
- runtime/editor C# build가 error 0으로 완료된다.
- Unity script Console project error가 0이다.
- CSV source validation이 error 0이며 catalog 5/8/8 수량을 유지한다.
- 최종 C# file/line 순감소를 기록한다.
- 사용자가 representative Trigger Play Mode 동작을 확인한다.

## Verification expected from Code Builder

- `rg`로 `SkillNodeExecutor`, `SkillTriggerDefinition.Nodes`, direct Trigger payload call을 전수 검색한다.
- 158 Trigger owner를 결과 범주별로 다시 집계한다.
- Trigger 하나가 delivery와 state command를 함께 소유하지 않는지 검증한다.
- EditMode test:
  - 조건 불일치 시 실행 0회
  - 조건 일치 시 기존 family Executor 경로 1회
  - 일반 cast cooldown/magazine 미소비
  - delayed context snapshot
  - recursion guard
  - event-derived damage 7개
  - lifecycle 억제/유지
  - temporary runtime 미등록
  - EventTarget 고정과 대상별 상태/속성 조건
- 기존 일반 Buff와 Trigger 상태 delivery가 `StatusCombatRules.ApplyStatus` 결과를 공유하는지 검증한다.
- `dotnet build Pakuri/Pakuri.sln --no-restore`
- Unity script refresh와 Console error 확인
- `Pakuri/Validate CSV Source Data`
- 사용자 Play Mode:
  - 기존 ExecuteSkill 4개
  - Single/Zone/Buff/Shield 대표 Trigger
  - shield/event-damage 기반 Trigger
  - cooldown/reload/status-duration command
  - delayed/repeat/recast/passive Trigger

## Related board files

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `boards/COMBAT/SKILL_DIRECT_CATALOG_RUNTIME_HANDOFF.md`

## Next Actions

- Code Builder가 보완된 계약을 Phase 1 기준선으로 커밋한다.
- Phase 2부터 final outcome Generation을 구현한다.

## Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/SkillDefinition.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/SkillNode.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/SkillExecutionData.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillActionContext.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillNodeExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Reactions/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs`
- `Pakuri/Assets/Scripts/Loading/Validation/CsvDataValidator.cs`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/**/*.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/**/skill_graph_nodes_*.csv`

## History

- 2026-07-28: Earlier design selected Trigger as activation authority and `SkillNodeExecutor` as direct payload dispatcher.
- 2026-07-29: Direct catalog migration generated final typed Trigger Nodes and removed runtime authored-string parsing.
- 2026-07-29: User selected a smaller final structure: Trigger judges conditions, existing family Executors implement skill effects, and `SkillNodeExecutor` is deleted.
- 2026-07-29: Designer replaced the obsolete Node-dispatch handoff with this executor-reuse contract based on current C# call sites and active CSV counts.
- 2026-07-29: Review added one-shot Single mapping, exact EventTarget, per-target predicates, shared status rules, temporary runtime/source snapshot separation, lifecycle policy, runtime recursion, seven dynamic damage cases, and current Circle radius parity.
- 2026-07-29: User approved the corrected document and assigned Code Builder implementation with Ponytail Ultra and one local Git commit per phase.
- 2026-07-29: Phase 1 fixed the 158/606/77/81 and 24-script/12,102-line baseline and completed runtime/editor builds with zero errors.
