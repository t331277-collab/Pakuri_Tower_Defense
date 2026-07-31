# Skill Node Runtime Resolver Consolidation Handoff

## Task title

Node 의미의 런타임 구현을 `SkillExecutionRuleResolver` 하나로 통합한다.

## Goals

- `Definitions/Nodes`의 `GetOperation<T>()` 해제와 Node 기반 값 계산을 `SkillExecutionRuleResolver`가 유일하게 소유한다.
- `SkillExecution`은 시전 검증, 학습 상태 전달, family/command 실행 조정, 기존 runtime API 호출과 Executor 분배를 담당한다.
- `SkillExecutionData`는 Resolver가 완성한 실행값을 보관하고 전달하는 수동 데이터 객체가 된다.
- `SkillTrigger`는 Resolver가 추출한 반응 정의를 받아 사건 gate를 판정하고, 성공한 반응을 `SkillExecution`으로 되돌려보낸다.
- `SkillTargeting`은 대상 조건을 받아 실제 대상 목록과 중심을 반환한다.
- `SkillStatus.cs`와 `SingleSkillRules.cs`의 Node 기반 계산을 Resolver로 옮긴 뒤 두 스크립트를 삭제한다.
- Executor와 Actor는 Node 의미를 해석하지 않고 확정된 실행값으로 물리적 실행만 수행한다.

## Constraints

- 사용자 승인 구조를 최우선으로 따른다.
- 모든 주장과 변경은 현재 코드와 명령 출력으로 검증한다.
- 새 Node별 handler, interface, factory, registry 또는 별도 runtime rule 스크립트를 만들지 않는다.
- Node operation 해제와 Node 기반 값 계산을 여러 Actor, Executor, Trigger, 상태 helper에 다시 분산하지 않는다.
- 기존 `SkillNode`, Node operation struct, `SkillActionContext`, `SkillExecutionData`, family Executor와 Actor를 재사용한다. 삭제 대상 context/state class를 유지하지 않는다.
- Node 데이터 계약, CSV, ID, 수치, 확률, 대상 결과, 지연, 반복, 내부 쿨다운과 재귀 제한을 보존한다.
- Projectile, Line, Single, Zone, Buff의 이동·충돌·히트박스·수명·Visual 실행은 기존 Executor와 Actor가 유지한다.
- 일반 시전과 Trigger 재실행은 같은 `SkillExecution -> Resolver -> SkillExecutionData -> Executor` 경로를 사용한다.
- Trigger가 직접 피해, 상태, 쿨다운, 재장전 또는 Zone 재시전을 실행하지 않는다.
- Resolver는 피해·상태·쿨다운·재장전·재시전 runtime API를 직접 호출하지 않는다.
- 상태 실제 적용은 `StatusCombatRules.ApplyStatus`, 피해 실제 적용은 `InGameCombatManager.ApplyDamage` 공통 경로를 유지한다.
- Trigger의 source-owned/passive-owned gate 비대칭과 재귀 제한 범위는 Phase 1 기준선으로 고정하며, 사용자 결정 없이 통합 과정에서 바꾸지 않는다.
- 구 Node 조합 경로와 Resolver 조합 경로를 같은 snapshot에 함께 적용하지 않는다.
- 이번 작업에서 `Implementation` 스크립트 수를 늘리지 않는다. 임시 migration 파일도 만들지 않는다.
- 코드 컨벤션은 가능한 한 기존 `.cs` 파일 하나를 책임 class 하나로 줄이는 것이다. 여러 class를 same-name 신규 파일로 분리하지 않는다.
- 다른 스크립트와 통합할 때 기존 class 본문을 대상 파일에 그대로 붙이지 않는다. 필요한 책임의 field·method·실행 흐름만 기존 책임 class에 흡수하고, 중복되거나 책임 밖인 class와 코드는 삭제한다.
- 책임을 잃은 class, field, method, wrapper와 호환성 facade는 실제 참조가 0이고 public/serialization 호환성 검토가 끝난 뒤 삭제한다. 미래 사용 가능성만으로 남기지 않는다.
- Unity Play Mode gameplay 검증은 사용자 소유다.
- `Skill Builder`는 비활성 역할이므로 이 작업에 사용하지 않는다.

## Role Owner

- 설계와 본 문서: Designer
- 구현: Code Builder
- 검토: 사용자 요청이 있을 때 Code Reviewer

## Status

- 사용자 구조 승인 완료.
- Code Builder가 지적 1~7을 코드로 검증하고 handoff 보강 완료.
- Phase 1 baseline/inventory 완료 및 커밋.
- Phase 2 Node composition 구현 및 빌드 검증 완료.
- Phase 4 SkillStatus·SingleSkillRules 흡수와 legacy type 삭제 완료; 다음은 Execution과 Actor의 Node 의미 정리다.

## Selected Code Builder tracks

- `AGENTS_ROLE/GAMEBULIDER_STRUCTURE.md`
- `AGENTS_ROLE/GAMEBULIDER_REFACT.md`
- `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`
- `AGENTS_ROLE/GAMEBULIDER_VERIFICATION.md`

## File and class convention

기본 원칙:

- 새 `.cs`를 추가하지 않는다.
- 기존 파일에 class가 여러 개면 파일 책임과 같은 주 class 하나만 최종적으로 남긴다.
- 나머지 class는 필요한 field와 method만 승인된 기존 책임 class에 흡수하고 type 자체는 삭제한다.
- 통합 결과는 class 수와 코드량이 함께 줄어야 한다. class 이름 변경, class 본문 복사, forwarding wrapper 추가는 통합으로 인정하지 않는다.
- private nested class로 이름만 숨기는 방식도 사용하지 않는다.
- enum과 작은 immutable value struct도 불필요하면 삭제하며, 이를 보존하려고 새 파일을 만들지 않는다.

현재 코드 근거와 목표:

- `SkillExecution.cs:17,87,1542,2028`에는 `SkillExecutionContext`, `SkillExecution`, `SkillUseState`, `SkillExecutionState` 네 top-level class가 있다.
- 이번 migration 뒤 `SkillExecution.cs`에는 `SkillExecution` class 하나만 남긴다. `SkillExecutionContext.cs`, `SkillUseState.cs`, `SkillExecutionState.cs`는 만들지 않는다.
- `SkillExecutionContext`, `SkillUseState`, `SkillExecutionState`의 caller를 migration하고, 실행 조정에 필요한 field와 method만 `SkillExecution`, `SkillExecutionData`, `SkillActionContext` 또는 이미 승인된 기존 소유자 책임에 맞춰 흡수한다. 세 class 자체와 중복 API는 삭제한다.
- `SkillStatus.cs:14,31`의 `ProjectileStatusHitSpec`과 `SkillStatus`는 새 파일로 분리하지 않는다. 필요한 상태 실행값은 기존 `SkillExecutionData`, `StatusApplicationSpec`, `StatusRuntimeData`와 공통 상태 API가 가진 계약으로 표현하고 두 class와 파일을 삭제한다.
- `SingleSkillRules.cs`의 `SingleDamageModifierState`와 `SingleSkillRules`도 새 class나 파일로 옮기지 않는다. 필요한 계산은 Resolver의 기존 method와 `SkillExecutionData` 결과로 흡수하고 두 타입과 파일을 삭제한다.

통합 원칙:

1. source class의 실제 caller와 사용 필드를 확인한다.
2. 대상 기존 class 책임에 필요한 입력·출력·계산만 정의한다.
3. caller를 새 책임 경계로 전환한다.
4. 중복 계산, 사용되지 않는 field/method, 전달만 하는 wrapper를 삭제한다.
5. source class 전체를 대상 파일로 복사하거나 이름만 바꾼 대체 class·새 파일을 만들지 않는다.

최종 `Implementation` 스크립트는 다음 6개로 고정한다.

- `SkillActionContext.cs`
- `SkillExecution.cs`
- `SkillExecutionData.cs`
- `SkillExecutionRuleResolver.cs`
- `SkillTargeting.cs`
- `SkillTrigger.cs`

## Approved responsibility model

### `SkillExecutionRuleResolver`

Node operation 해제와 Node 기반 값 계산의 유일한 런타임 해석기다.

책임:

- Definition만 받는 진입점과 owner/runtime/roster를 받는 진입점을 제공한다.
- Definition 진입점은 기본 Node만 해석하며 AI·UI·Editor 검사처럼 전투 owner가 없는 호출부에서 사용한다.
- owner/runtime 진입점은 기본 Node 뒤 passive base modifier, enhancement, master를 현재 순서대로 합성한다.
- 모든 `SkillNode.GetOperation<T>()` 호출과 Node 기반 조건·보정·상태·적중·분기·연사·처형·보스·처치 회복 값 계산을 소유한다.
- 시전 시점에 확정 가능한 값은 `SkillExecutionData`에 기록한다.
- 대상 상태, 실제 적중, 발사 순번과 사망 결과처럼 실행 중에만 알 수 있는 의미는 Actor가 전달한 런타임 문맥으로 판정한다.
- Node가 보정한 상태·피해·조건 값을 시전 또는 적중 시점의 계산 결과로 반환한다.
- `SkillReactionOp`에서 반응 정의를 추출하되 사건 gate는 판정하지 않는다.

금지:

- Actor 생성, GameObject 수명, 충돌 검사, 이동, Visual 배치 소유.
- 스킬 유형별 Executor 선택.
- Node별 새 handler class 생성.
- `InGameCombatManager.ApplyDamage`, `StatusCombatRules.ApplyStatus`, `ReduceCooldownRemaining`, `ReduceReloadRemaining`, `TryExecuteRecast` 직접 호출.

### `SkillExecution`

스킬 실행 조정자다.

책임:

- 시전자, runtime, 전투 행동 가능 여부, 쿨다운, 탄창과 시전 간격을 검증한다.
- Resolver에 실행에 필요한 명시적 입력, Skill Definition, 학습·선택 상태와 사건 문맥을 전달한다. 삭제 대상 `SkillExecutionContext` class에 의존하지 않는다.
- Resolver가 반환한 `SkillExecutionData`가 실행 가능한지 확인한다.
- `SkillRuntimeKind`에 맞는 기존 Executor로 분배한다.
- Actor가 전달한 적중 문맥과 Resolver 계산 결과를 받아 기존 피해·상태 공통 API를 호출한다.
- Trigger가 통과시킨 effect와 command를 조정하고 기존 쿨다운·재장전·상태 연장·Zone 재시전 API를 호출한다.
- 실행 성공 뒤 탄창·쿨다운·연사 상태와 lifecycle을 확정한다.
- 일반 시전과 Trigger 재실행의 공통 진입점을 유지한다.

금지:

- `SkillNode.GetOperation<T>()` 호출.
- 특정 Node operation 직접 해석. 단, Resolver 결과를 기존 runtime API에 전달하는 실행 책임은 유지한다.

### `SkillExecutionData`

완성된 실행값의 전달 객체다.

허용:

- 실행 입력과 `Prepared*` 결과 보관.
- 읽기 전용 컬렉션과 값 조회.
- Resolver가 내부 범위에서 값을 채울 수 있는 최소 접근자.
- Node를 읽지 않는 내부 copy 생성자 또는 기계적 clone.

이동 대상:

- 생성자 내부 `ApplyNodes` 호출.
- `ApplyChoiceSpec`, `ApplyNodes`, `ApplyNodeAction`.
- Node 종류별 모든 `Apply*Action`.
- Node 의미에 따라 값을 계산하는 변형 로직.

`SkillExecutionData`가 Resolver처럼 동작하지 않도록 한다. Resolver가 데이터를 만들고 반환하며, 데이터 객체 자체는 Node 의미를 해석하지 않는다. 빌드 완료 뒤 내부 컬렉션은 변경하지 않으며, copy가 컬렉션을 공유하더라도 원본과 사본 모두 해당 컬렉션을 수정하지 않는 불변 조건을 둔다.

### `SkillTrigger`

전투 사건 판정기다.

책임:

- Actor와 전투 시스템이 전달한 `SkillActionContext`를 받는다.
- 사건 종류, source scope, 선택 여부, 상태, 속성, 확률, 내부 쿨다운, 횟수, 지연과 반복을 판정한다.
- 조건을 통과한 반응을 `SkillExecution`의 공통 반응 진입점으로 전달한다.
- 반응 정의가 필요하면 Resolver가 이미 해석한 reaction 목록을 사용한다.
- `SkillReaction` 필드의 사건 gate 의미는 Trigger가 소유하며 Resolver가 대신 판정하지 않는다.
- source-owned와 passive-owned의 현재 gate 차이는 별도 승인 전까지 그대로 보존한다.

이동 대상:

- `TryExecuteOutcome`의 효과 실행 결정.
- `ExecuteCommand`의 대상 탐색과 상태·쿨다운·재장전 변경.
- `ResolveTriggeredRawDamage`의 event 값 계산은 `SkillExecution` 반응 조정 경로로 이동한다.
- 직접 `new SkillExecutionData(...)` 또는 `SkillExecutionState.CreateExecutionData(...)`로 Node를 조합하는 경로.

### `SkillTargeting`

대상 선택 전담이다.

- 대상 진영, 선택 방식, 상태 조건, 거리, 중심, 범위, EventTarget 고정과 연쇄 대상을 해석한다.
- `SkillExecution`, Resolver, Executor 또는 Actor가 전달한 조건에 맞는 대상·중심을 반환한다.
- Node 효과, Trigger gate 또는 피해를 소유하지 않는다.

사용자가 처음 말한 “`SkillTrigger`가 대상 탐색 조건을 받아 대상을 반환”은 이후 설명과 현재 코드 근거를 따라 `SkillTargeting` 책임으로 확정한다.

### Executor and Actor

- Executor는 완성된 `SkillExecutionData`를 받아 기존 family Actor를 생성하거나 즉시 효과를 전달한다.
- Actor는 이동, 충돌, 히트박스, Tick, 만료와 Visual 수명을 담당한다.
- Actor는 물리적으로 발생한 사건 문맥을 `SkillExecution`과 Trigger에 전달한다.
- Actor와 Executor는 Node operation 타입을 직접 검사하지 않는다.

## Target execution flow

### Initial cast

```text
input / auto route
  -> SkillExecution
     - actor/runtime validity
     - CanAct/cooldown/magazine/cast interval validation
  -> SkillExecutionRuleResolver
     - source Node + learned passive + selected enhancement/master
     - targeting/status/runtime values
     - final execution values
  -> SkillExecutionData
  -> family Executor
  -> Actor or immediate family effect
  -> SkillExecution commits runtime state
```

### Hit-time rule

```text
Actor detects collision/tick/hit
  -> runtime hit context
  -> SkillExecution hit entry
  -> SkillExecutionRuleResolver
     - target status
     - conditional damage/critical
     - consecutive/burst/branch index
     - additional/chain/core/kill/reload effects
  -> resolved generic hit values
  -> SkillExecution invokes existing combat/status APIs
  -> Actor publishes event context
```

Node 의미는 Resolver가 계산한다. Actor는 충돌과 수명 사실만 제공하고, 실제 피해와 상태는 기존 공통 API가 적용한다.

### Trigger reaction

```text
Actor or combat system event
  -> SkillTrigger
     - event/source/status/choice/proc/count/cooldown/delay/repeat gate
  -> SkillExecution reaction entry
  -> SkillExecutionRuleResolver
     - reaction이 참조하는 Node 기반 실행값
  -> SkillExecution
     - event target/center/raw event value 조정
     - family effect는 SkillExecutionData와 기존 Executor로 분배
     - command는 기존 runtime API로 실행
```

`RefundCooldown`, `ReduceReload`, `ExtendStatusDuration`처럼 family Executor로 표현되지 않는 현재 command는 Trigger와 Resolver가 직접 실행하지 않는다. `SkillExecution`이 Resolver 계산 뒤 기존 runtime API로 처리하고 성공 여부를 반환한다. 이 예외를 위한 새 Executor는 만들지 않는다.

## Current inspected evidence

### Node meaning is currently interpreted inside data

- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillExecutionData.cs:565`
  - `ApplyNodes`가 모든 `SkillNode.GetOperation<T>()`를 직접 호출한다.
- 같은 파일 `:728` 이후
  - `SkillActionOpKind` switch와 Node별 `Apply*Action`이 의미를 구현한다.
- 따라서 현재 `SkillExecutionData`는 수동 데이터 객체가 아니다.

### Node meaning is spread across runtime scripts

- `SkillExecution.cs:542`
  - `SkillCastEffect`를 직접 해석하고 실행한다.
- `SkillExecution.cs:1277`
  - `CastConditionOp`를 직접 합산한다.
- `SkillExecution.cs:2359`
  - `CountStatusDamageActionOp`를 직접 읽는다.
- `SkillExecutionRuleResolver.cs`
  - 조건부 피해·치명타, burst, branch, 공통 적중과 후속 효과 일부만 담당한다.
- `SingleSkillRules.cs`
  - 처형 조건, 보스·처형 피해, 치명타와 처치 쿨다운 회복을 담당한다.
- `SkillStatus.cs`
  - 상태 중첩·지속시간·최대 중첩과 상태 modifier를 확정한다.
- `SingleSkillActor.cs`
  - 반복, core hitbox, 상태 소비·재분배와 적중 수 쿨다운 환급을 직접 구현한다.
- Projectile, Line, Zone Actor도 조건부 피해와 공통 적중 helper를 호출한다.

### Trigger currently executes outcomes

- `SkillTrigger.cs:758`
  - `TryExecuteOutcome`이 반응 결과 종류를 직접 분기한다.
- `SkillTrigger.cs:878`
  - `ExecuteCommand`가 대상 탐색, 상태 연장, 쿨다운 환급과 재장전 감소를 직접 수행한다.
- `SkillTrigger.cs:912`
  - Trigger가 직접 `new SkillExecutionData(skill)`을 호출한다.
- 이는 “조건 판정 후 SkillExecution으로 반환” 책임과 다르다.

### Delete targets have active callers

- `SkillStatus` 호출부:
  - `SkillExecution.cs`
  - `SingleSkillActor.cs`
- `SingleSkillRules` 호출부:
  - `SkillExecution.cs`
  - `SingleSkillActor.cs`
- 두 스크립트는 호출부를 Resolver API로 교체한 뒤에만 삭제한다.

### Repository-wide construction and mutation surface

생성자 Node 적용을 제거하면 Combat 밖 호출도 영향을 받는다. Phase 1에서 다음 전체 표면을 고정한다.

- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyCombatDecision.cs:193`
  - Definition만으로 `Reactions`를 읽어 `CombatStart` 반응 보유 여부를 판단한다.
- `Pakuri/Assets/Scripts/UI/InGame/DamageMeter/DamageMeterUIController.cs:255,271`
  - active/passive Definition의 반응 ID를 이름 표시용으로 찾는다.
- `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs:24,26,35,36,340,719,736,737,752,759`
  - `SkillExecutionData` 생성자와 `ApplyChoiceSpec`, `ApplyDynamicDamageMultiplier`, 내부 `ScaleDamageMultiplier`를 직접 검증한다.
- `SkillExecution.cs:180,236,305,471,506,2074,2281,2326,2353,2621,2661`
  - owner/runtime 조합, 선택 적용과 Definition 없는 choice snapshot을 만든다.
- `SkillTrigger.cs:409,435,906,912`
  - source/passive 반응과 recast snapshot을 직접 만든다.
- `ProjectileSkillExecutor.cs:55`, `SingleSkillActor.cs:261,722`
  - `CopyWithDamageMultiplier`로 실행 사본을 만든다.

최소 Resolver 진입점:

1. Definition-only: 기본 Definition Node만 해석한다. AI, UI, Editor 검사에서 owner/runtime 없이 사용한다.
2. owner/runtime/roster: 기본 Node 뒤 학습 passive, enhancement, master를 현재 순서로 합성한다.

기존 호출부를 삭제하기 전 위 목록을 저장소 전체 `rg` 결과로 다시 확정한다.

### Trigger baseline asymmetry

현재 source-owned와 passive-owned 반응은 같은 gate를 사용하지 않는다.

- source-owned는 `SkillTrigger.cs:384`에서 event/source skill/event skill/runtime kind/execute/choice/source status를 확인한 뒤 바로 실행한다.
- passive-owned는 `SkillTrigger.cs:446`에서 위 조건 외 target status, status source skill, attribute, event source scope를 확인하고 count/proc/internal cooldown gate까지 적용한다.
- 전역 `MaxTriggeredExecutionDepth = 8`은 `SkillExecution.TryExecuteReaction`과 `TryExecuteReactionEffect`에만 적용된다.
- command는 `SkillTrigger.ExecuteCommand`에서 전역 깊이 제한을 거치지 않는다. `RecastZone`만 `MaxGeneration`을 별도로 확인한다.

통합은 이 비대칭을 현행 기준선으로 보존한다. source-owned gate 통일이나 command 재귀 제한 추가는 별도 버그 수정이며 사용자 승인 전 수행하지 않는다.

### Dual-path comparison boundary

현재 `SkillExecutionData(SkillDefinition)`은 생성 중 즉시 `ApplyNodes(source.Nodes)`를 실행한다. Resolver가 같은 snapshot에 다시 Node를 적용하면 배율, 중첩과 목록 항목이 이중 반영된다.

- 구 경로 snapshot과 Resolver snapshot은 별도로 생성해 테스트에서만 비교한다.
- 실제 runtime은 각 Phase에서 구 경로 또는 Resolver 경로 하나만 사용한다.
- runtime 전환은 parity 검증 뒤 한 번에 수행하며 두 조합 경로를 겹치지 않는다.

## Complete Node operation flow inventory

다음 표는 현재 기준선이다. 통합 뒤 `GetOperation<T>()`와 writer는 Resolver로 이동하지만, runtime reader는 operation 타입 대신 Resolver가 반환한 중립 값만 소비해야 한다.

| Operation | 현재 writer 또는 해제 위치 | 저장값 | 현재 runtime reader | 판정 시점 |
|---|---|---|---|---|
| `DamageModifierOp` | `SkillExecutionData.ApplyNodes:587` | `damageModifierOps` | `SingleSkillRules` | 대상 적중 |
| `CritModifierOp` | `ApplyNodes:593` | `critModifierOps` | `SingleSkillRules` | 대상 적중 |
| `ConditionalDamageActionOp` | `ApplyNodes:635` → `ApplyConditionalDamageAction` | `conditionalDamageActions` | `SkillExecutionRuleResolver.ConditionalDamageMultiplier` | 대상 적중 |
| `ConditionalCritChanceActionOp` | `ApplyNodes:641` → `ApplyConditionalCritChanceAction` | `conditionalCritChanceActions` | `SkillExecutionRuleResolver.ConditionalCritChanceBonus` | 대상 적중 |
| `StatusConditionalDamageTakenActionOp` | `ApplyNodes:659` → `ApplyStatusConditionalDamageTakenAction` | `HasStatusConditionalDamageTakenBonus`, bonus, source status kind | `SkillStatus` | 상태 실행값 구성 |
| `CastConditionOp` | `ApplyNodes:581` | `castConditionOps` | `SkillExecution:1277`, `SingleSkillRules` | 시전 검증 전 |
| `SourceStatusRequirementOp` | `SkillExecutionRuleResolver.MeetsSourceStatusRequirements:501` | 저장하지 않고 직접 판정 | `SkillExecutionState.ApplyChoices/ApplyResolvedChoices` | choice 합성 |
| `SkillReactionOp` | `ApplyNodes:611` | `reactions` | `SkillTrigger` | 사건 발생 뒤 gate 시작 전 |
| `SkillCastEffectOp` | `ApplyNodes:605` | `castEffects` | `SkillExecution.ExecuteCastEffects` | 일반 시전/반응 실행 |
| `KillActionOp` | `ApplyNodes:599` | `killActionOps` | `SingleSkillRules` | 피해 결과의 사망 확정 뒤 |
| `SkillActionOp` | `ApplyNodes:617` → `ApplyNodeAction` | 아래 kind별 필드와 map | `SkillExecution`, `SkillStatus`, `SkillTrigger`, Executor/Actor | kind별 시전·적중 시점 |
| `ConsecutiveHitActionOp` | `ApplyNodes:623` → `ApplyConsecutiveHitAction` | `ConsecutiveHitBonusRate`, `ConsecutiveHitMax` | `SkillExecution.ConsecutiveHitDamageMultiplier`, Projectile Actor | 연속 적중 |
| `BranchDamageActionOp` | `ApplyNodes:629` → `ApplyBranchDamageAction` | branch chance/count/damage/radius 필드 | Resolver branch 계산, Single follow-up | 발사 순번 또는 적중 후속 |
| `BurstDamageActionOp` | `ApplyNodes:647` → `ApplyBurstDamageAction` | `burstDamageActions` | Resolver를 호출하는 `SkillExecution` | 발사 순번 확정 |
| `BurstStatusActionOp` | `ApplyNodes:653` → `ApplyBurstStatusAction` | `burstStatusActions` | Resolver를 호출하는 `SkillExecution` | 발사 순번 확정 |
| `FollowUpProjectileActionOp` | `ApplyNodes:665` → `ApplyFollowUpProjectileAction` | follow-up count/delay/damage 필드 | `SkillExecution`, Projectile Executor | 발사 계획/지연 실행 |
| `ThresholdStatusActionOp` | `ApplyNodes:671` → `ApplyThresholdStatusAction` | threshold source/min/applied status 필드 | `SkillStatus` | 상태 실행값 구성과 적중 |
| `RepeatPerTargetActionOp` | `ApplyNodes:677` → `ApplyRepeatPerTargetAction` | repeat count/interval/damage 필드 | Single Actor | 최초 적중 뒤 반복 |
| `RedistributeConsumedStatusActionOp` | `ApplyNodes:683` → `ApplyRedistributeConsumedStatusAction` | redistribution ratio/kind/radius/count | Single Actor | 상태를 소비한 대상 사망 뒤 |
| `AdditionalDamageActionOp` | `ApplyNodes:689` → `ApplyAdditionalDamageAction` | on-hit chance/multiplier/attribute/target | Resolver `ApplyHitEnhancements` | 적중 뒤 |
| `CoreDamageActionOp` | `ApplyNodes:695` → `ApplyCoreDamageAction` | core hitbox/multiplier | Single Actor | prefab core 충돌 뒤 |
| `CoreAdditionalDamageActionOp` | `ApplyNodes:701` → `ApplyCoreAdditionalDamageAction` | core 추가 피해 필드 | Single Actor | core 적중 뒤 |
| `HitChainDamageActionOp` | `ApplyNodes:707` → `ApplyHitChainDamageAction` | hit period/target/radius/damage/attribute | Resolver `ApplyHitEnhancements` | 적중 횟수 충족 뒤 |
| `HitCountCooldownRefundActionOp` | `ApplyNodes:713` → `ApplyHitCountCooldownRefundAction` | target skill/min targets/ratio | Single Actor | 한 실행의 적중 수 확정 뒤 |
| `ReloadReducePerHitActionOp` | `ApplyNodes:719` → `ApplyReloadReducePerHitAction` | target skill/seconds per hit | Resolver `ApplyHitEnhancements` | 적중 뒤 |
| `CountStatusDamageActionOp` | `SkillExecution.ApplyDynamicChoiceRules:2380` | `DamageMultiplier`에 즉시 반영 | `SkillExecutionData` 소비 경로 전체 | owner/roster를 받는 snapshot 합성 |

### `SkillActionOpKind` field map

| Kind 그룹 | 저장값 | 현재 reader | 판정 시점 |
|---|---|---|---|
| `DamageMultiplier`, `ShieldAmountMultiplier`, `CooldownMultiplier`, `MagazineBonus`, `ReloadTimeMultiplier`, `PierceBonus`, `RadiusMultiplier`, `RadiusBonus`, `DurationBonus`, `DurationMultiplier`, `DamageDelayMultiplier`, `AdditionalProjectileBonus`, `ShotIntervalMultiplier`, `HitTargetCountBonus`, `LineCastRepeatCountBonus`, `CritChanceBonus`, `CritDamageBonus`, `BeamWidthBonus`, `KnockbackDistanceMultiplier` | 같은 이름의 scalar | `SkillExecution`, family Executor/Actor, `SkillStatus` | snapshot 합성 뒤 family 준비/적중 |
| `StatusStackAmountBonus`, `StatusStackAmountSet`, `StatusMaxStacksBonus`, `StatusActionSpeedBonus`, `StatusAttackPowerBonus`, `StatusAilmentResistanceBonus`, `StatusDamageBonusRate`, `StatusShieldReceivedBonus`, `StatusCriticalChanceBonus`, `StatusDamageTakenBonus`, `StatusFlatElementResistReduction`, `StatusDurationBonus`, `StatusElementDamageTakenBonus`, `StatusCriticalDamageTakenBonus` | 상태 scalar와 status ID별 map | `SkillStatus` | 상태 실행값 구성 |
| `TargetStatusStackDamageRateBonus`, `TargetStatusStackDamageMultiplier`, `ConsumeTargetStatusRatioOverride` | status ID별 damage rate, multiplier, consume ratio | `SkillExecution` 준비와 Single Actor | 대상 적중 |
| `TriggerProcChanceBonus` | reaction ID별 proc bonus map | `SkillTrigger.PassesProcGate` | passive-owned 사건 gate |

### Nested reaction contracts

| Contract | 저장 위치 | 현재 reader | 책임 경계 |
|---|---|---|---|
| `SkillReaction` | `SkillReactionOp`에서 `reactions`로 저장 | `SkillTrigger`와 `SkillExecution` | Resolver는 추출만, Trigger는 gate, Execution은 outcome 조정 |
| `StatusStackCondition` | 조건 operation 내부 값 | Resolver, Trigger, 기존 Single 규칙 | 사용 시점의 source/target 상태 판정 |
| `SkillReactionCommand` | `SkillReaction.Command` | 현재 `SkillTrigger.ExecuteCommand` | 목표 구조에서는 Trigger gate 뒤 `SkillExecution`이 기존 runtime API 호출 |

### Reader exists but writer is absent

`SkillExecutionData.cs`의 private-set property를 assignment 검색한 결과 다음 값은 선언 외 writer가 없다.

| 필드 | 현재 reader | 현재 기준값 |
|---|---|---|
| `HasBranchChanceSet`, `BranchChanceSet` | `HasBranchBehavior`, Resolver branch 계산 | `false`, `0` |
| `BranchLaunchPeriod`, `HasBranchLaunchChanceSet`, `BranchLaunchChanceSet` | `HasBranchLaunchTrigger`, Resolver branch 계산 | `0`, `false`, `0` |
| `StatusTag` | Skill runtime reader 없음 | `null` |
| `StatusChanceBonus` | `SkillStatus:63` | `0` |
| `HasConsumeTargetStatusStacksOverride`, `ConsumeTargetStatusStacksOverride` | `SkillExecution:1324` | `false`, `0`; Definition 값 fallback 사용 |

Phase 1은 이 기본값을 기준선으로 기록한다. 보존 또는 삭제는 별도 결정을 남긴 뒤 수행하며, writer가 없다는 이유만으로 임의 삭제하지 않는다.

## File-level implementation surface

### Expand

- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillExecutionRuleResolver.cs`
  - Node 조합과 Node 기반 값 계산을 통합한다.
  - 현재 `SkillExecutionData`, `SkillStatus`, `SingleSkillRules`, `SkillExecution`, `SkillTrigger`, Actor에 흩어진 Node 의미를 이동한다.
  - Resolver 내부에서 같은 의미를 두 경로로 구현하지 않는다.

### Reduce

- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillExecution.cs`
  - 검증, Resolver 호출, family/command 조정, 기존 runtime API 호출, Executor 분배와 성공 상태 확정을 맡는다.
- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillExecutionData.cs`
  - 값 보관과 조회만 남긴다.
- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillTrigger.cs`
  - 사건 gate와 `SkillExecution` 재진입만 남긴다.
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/*`
  - Node별 분기를 제거하고 generic prepared value 실행만 남긴다.

### Keep responsibility

- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Line/LineSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Zone/ZoneSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Buff/BuffSkillExecutor.cs`
- 각 family Actor의 이동·충돌·수명 책임.

### Migrate external callers

- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyCombatDecision.cs`
- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyActionController.cs`
- `Pakuri/Assets/Scripts/Units/Monster/Input/PlayerCombatInputController.cs`
- `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs`
- `Pakuri/Assets/Scripts/UI/InGame/DamageMeter/DamageMeterUIController.cs`
- `Pakuri/Assets/Scripts/UI/InGame/MonsterPanel/MonsterPanelUI.cs`
- `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`

AI와 DamageMeter의 Definition-only 조회는 Resolver 진입점으로 옮긴다. `SkillUseState`와 `SkillExecutionState`를 소비하는 Unit/UI caller는 새 파일이나 호환 wrapper 없이 승인된 기존 `SkillExecution`·unit state API로 migration한다. Editor 테스트는 삭제 예정 데이터 API 대신 최종 책임 API를 검증한다.

### Consolidate without adding files

- `SkillExecution.cs`
  - `SkillExecution` class 하나만 유지한다.
  - 실행 문맥, 개별 스킬 runtime 상태와 unit skill 목록에서 계속 필요한 기능은 중복을 제거한 뒤 승인된 기존 class 책임으로 흡수한다.
  - `SkillExecutionContext`, `SkillUseState`, `SkillExecutionState` type과 직접 caller를 제거한다.
- `SkillExecutionData.cs`
  - 한 번의 실행에 필요한 수동 데이터만 유지한다. persistent unit/skill state 전체를 떠넘기지 않는다.
- `SkillActionContext.cs`
  - 지연 사건에 필요한 불변 문맥만 유지한다. `SkillExecutionContext` class를 이름만 바꿔 붙이지 않는다.
- `SkillExecutionRuleResolver.cs`
  - Node 해제와 계산 method만 흡수한다. `SingleSkillRules` 또는 상태 helper class 본문을 붙이지 않는다.
- `SkillTargeting.cs`, `SkillTrigger.cs`
  - 승인된 기존 책임만 유지한다.

새 `.cs`는 0개다. 최종 파일 수 감소는 삭제 대상 2개로만 발생한다.

### Delete after migration

- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillStatus.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillStatus.cs.meta`
- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SingleSkillRules.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SingleSkillRules.cs.meta`

`ProjectileStatusHitSpec` class는 새 파일이나 다른 기존 파일로 옮기지 않는다. 실제 사용 필드만 기존 실행·상태 계약으로 치환하고 모든 caller 전환 뒤 class와 함께 삭제한다.

`SingleDamageModifierState`도 Resolver 파일로 물리적으로 옮기지 않는다. 필요한 damage/crit 값만 기존 Resolver method와 실행 데이터로 반환하고 기존 타입은 caller 전환 뒤 삭제한다. 대체 class는 만들지 않는다.

## Migration phases

### Phase 1 — baseline and inventory

1. 현재 빌드 결과를 기록한다.
2. 저장소 전체 `new SkillExecutionData`, `CreateExecutionData`, `ApplyChoiceSpec`, `ApplyNodes`, `ApplyDynamicDamageMultiplier`, `ScaleDamageMultiplier`, `SetRawDamageOverride`, `CopyWithDamageMultiplier` 호출부를 기록한다.
3. 위 operation flow 표의 writer, stored field, runtime reader와 판정 시점을 재검증한다.
4. Definition-only와 owner/runtime/roster Resolver 진입점 계약을 확정한다.
5. source-owned/passive-owned Trigger gate 차이와 command 재귀 우회 기준선을 테스트로 고정한다.
6. migration 대상 파일의 top-level class와 nested type을 기록하고 각 타입의 독립 책임·외부 caller를 확인한다.
7. 현재 `Implementation` `.cs` 8개와 최종 6개 고정 목록을 기록하고 Phase마다 파일 수가 늘지 않는지 검사한다.
8. 현재 public/internal API와 serialized data shape를 기록한다.

### Phase 2 — Resolver owns Node composition

Status: complete. Phase 1 baseline commit: `8398d68`. Phase 2 verification: `rg` shows `GetOperation<T>()` only in this Resolver; `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` exits 0 with 0 errors.

1. `SkillExecutionData.ApplyNodes`와 Node별 `Apply*Action` 내용을 Resolver로 이동한다.
2. 기본 Skill Node, passive base modifier, enhancement와 master choice의 합성 순서를 보존한다.
3. Resolver가 완성된 `SkillExecutionData`를 반환하게 한다.
4. 구 생성자 경로 snapshot과 Resolver snapshot을 별도로 만들어 parity 테스트에서 비교한다.
5. 실제 runtime에는 한 경로만 연결한다. 같은 snapshot에 구 경로와 Resolver를 함께 적용하지 않는다.

### Phase 3 — remove active behavior from data

Status: complete. SkillExecutionData now owns the former per-skill runtime state, UnitSkills owns active/passive runtime lists and lookup, SkillExecutionData has no Node extraction methods, C# callers contain no SkillUseState or SkillExecutionState, and dotnet build Pakuri/Assembly-CSharp.csproj --no-restore exits 0 with 0 errors.

1. 생성자의 Node 적용을 제거한다.
2. `ApplyChoiceSpec`, `ApplyNodes`, Node별 `Apply*Action`을 제거한다.
3. Node 기반 계산은 Resolver로 옮기고, reaction/raw damage 같은 실행 조정은 `SkillExecution`에 둔다.
4. Node를 읽지 않는 내부 copy/clone은 `SkillExecutionData`에 유지할 수 있다.
5. 빌드 완료 뒤 컬렉션 불변 조건과 원본 snapshot 비오염 테스트를 추가한다.
6. `SkillExecutionState`의 목록·조회 caller를 승인된 기존 소유자로 옮기고, 중복 조합 helper와 class 자체를 제거한다.

### Phase 4 — absorb `SkillStatus` and `SingleSkillRules`

Status: complete. Resolved status values now use the existing StatusApplicationSpec contract, status calculation lives in SkillExecutionRuleResolver, SingleSkillRules values are returned through existing execution data and Resolver methods, both legacy scripts and meta files are deleted, Assembly-CSharp builds with 0 errors, and Implementation remains at the six existing scripts.

1. `SkillStatus`의 Node 기반 상태 스펙 계산을 Resolver로 이동한다.
2. 실제 상태 적용은 `StatusCombatRules.ApplyStatus` 공통 경로에 유지한다.
3. `SingleSkillRules`의 처형·보스·처치 회복 계산을 family 중립 Resolver 결과로 이동한다.
4. 충돌 판정은 Actor/`UnitCollisionResolver`, 실제 피해 적용은 `InGameCombatManager.ApplyDamage` 공통 경로에 유지한다.
5. 모든 호출부를 Resolver 또는 `SkillExecution` 공통 적용 API로 교체한다.
6. 참조가 0임을 확인한 뒤 두 `.cs`와 `.meta`를 삭제한다.

### Phase 5 — remove Node meaning from Execution and Actors

1. `SkillExecution`의 `SkillCastEffectOp`, `CastConditionOp`, `CountStatusDamageActionOp` Node 해석을 Resolver로 이동한다.
2. Single Actor의 repeat/core/status redistribution/hit-count refund 해석을 Resolver로 이동한다.
3. Projectile, Line, Zone의 조건부 Node 계산을 Resolver API 하나로 통일한다.
4. Resolver는 계산 결과만 반환하고 `SkillExecution`이 기존 피해·상태·쿨다운·재장전 API를 호출한다.
5. Executor와 Actor는 prepared value와 generic runtime result만 사용한다.

### Phase 6 — Trigger becomes gate-only

1. Trigger의 Node extraction은 Resolver로, outcome/command 실행 조정은 SkillExecution 반응 경로로 이동한다.
2. Trigger의 직접 대상 탐색과 runtime 상태 변경을 제거한다.
3. Trigger가 통과한 사건은 항상 SkillExecution으로 반환한다.
4. Trigger 지연, 반복, proc, count와 내부 쿨다운 의미를 보존한다.
5. source/passive gate 비대칭과 command 재귀 우회는 별도 사용자 결정 없이는 변경하지 않는다.

### Phase 7 — cleanup and verification

1. `SkillExecutionContext`, `SkillUseState`, `SkillExecutionState`, `ProjectileStatusHitSpec`, `SingleDamageModifierState`, `SkillStatus`, `SingleSkillRules` caller를 모두 기존 책임 API로 전환한다.
2. 위 타입과 책임을 잃은 class, 중복 helper, 사용되지 않는 field/method를 참조 0 확인 뒤 삭제한다.
3. `Implementation`에 신규 `.cs`가 없고 최종 6개 파일만 남는지 검사한다.
4. `SkillExecution.cs`에는 `SkillExecution` class 하나만 남고, 삭제 class 본문을 다른 파일에 붙이거나 nested class/wrapper로 숨기지 않았는지 검사한다.
5. Node operation 소비가 Resolver 하나인지 검사한다.
6. 빌드, diff, Unity compile/console을 검증한다.
7. 관련 board에 구현 결과와 실제 명령 출력을 기록한다.

## Compatibility requirements

- 모든 기존 Node operation의 계산 순서와 합산·곱산 방식을 보존한다.
- 기본 Skill Node 적용 뒤 passive base modifier, enhancement, master choice를 적용하는 현재 순서를 보존한다.
- 조건부 값은 현재와 같은 시점에 판정한다.
  - 시전 조건: 시전 전.
  - 대상 상태 조건부 피해: 실제 적중 시점.
  - burst/launch 조건: 해당 발사 순번 확정 시점.
  - kill 효과: 피해 결과가 사망으로 확정된 뒤.
  - Trigger gate: 사건 발생 뒤, 지연 시작 전 필요한 사건값을 고정.
- `MaxTriggeredExecutionDepth`, recast generation과 lifecycle 발행 정책을 약화하지 않는다.
- source-owned/passive-owned Trigger gate의 현재 비대칭을 behavior baseline으로 보존한다.
- player-facing behavior, Unity serialization, prefab와 asset 참조를 보존한다.
- 삭제 대상 public type의 저장소 내부 caller는 새 책임 API로 모두 migration한다. 구 type 이름을 보존하기 위한 wrapper는 만들지 않는다.
- CSV와 생성 데이터는 이 구조 작업에서 변경하지 않는다.

## Edge cases and risks

- Resolver가 커지더라도 Node별 class나 handler를 새로 만들지 않는다. 영역별 private method로만 정리한다.
- cast-time과 hit-time 값을 한 시점에 강제로 확정하지 않는다.
- Trigger reaction 목록은 Trigger가 Node를 직접 읽지 않도록 Resolver가 제공한다.
- passive Trigger는 활성 Actor snapshot이 없어도 Resolver를 통해 학습 상태에서 reaction을 얻을 수 있어야 한다.
- command outcome을 새 hidden skill이나 새 Executor로 변환하지 않는다.
- 상태 데이터 clone과 catalog fallback을 보존한다.
- `SkillExecutionData` copy 시 컬렉션을 수정하지 않는 불변 조건으로 원본 실행값 오염을 막는다. 이후 변경이 필요해질 때만 deep copy를 추가한다.
- Actor가 같은 적중을 Resolver와 Trigger에 중복 전달하지 않게 한다.
- 일반 시전과 Trigger 재실행의 쿨다운·탄창 소비 차이를 보존한다.
- Resolver 비교용 snapshot을 실제 runtime snapshot에 덧씌우지 않는다.
- 물리적 class 이동만으로 책임 통합이 끝났다고 판단하지 않는다. caller와 계산 소유권이 목표 경계로 바뀌어야 한다.
- 삭제 후보를 빈 wrapper나 forwarding method로 남기지 않는다. 실제 public/serialization 호환성 근거가 있는 경우만 예외를 기록한다.

## Acceptance criteria

1. `SkillExecutionRuleResolver`만 `Definitions/Nodes`의 `GetOperation<T>()`와 Node 기반 값 계산을 수행한다.
2. `SkillExecutionData`에 `GetOperation<T>()`, `ApplyNodes`, `ApplyChoiceSpec`, Node별 `Apply*Action`이 없다.
3. `SkillExecution`, `SkillTrigger`, Executor와 Actor가 Node operation 타입을 직접 검사하지 않는다.
4. `SkillStatus.cs`, `SkillStatus.cs.meta`, `SingleSkillRules.cs`, `SingleSkillRules.cs.meta`가 삭제된다.
5. `SkillStatus.`와 `SingleSkillRules.` 참조가 0건이다.
6. `SkillTrigger`가 직접 피해·상태·쿨다운·재장전·재시전을 적용하지 않는다.
7. Trigger 조건 통과 결과가 `SkillExecution` 공통 진입점으로 전달된다.
8. Trigger가 사건 gate를 소유하고 Resolver는 gate를 대신 판정하지 않는다.
9. `SkillExecutionRuleResolver`에 피해·상태·쿨다운·재장전·재시전 runtime API 호출이 없다.
10. `SkillExecution`이 family/command 실행을 조정하고 기존 runtime API를 호출한다.
11. Definition-only Resolver 진입점으로 Enemy AI와 DamageMeter 반응 조회가 유지된다.
12. owner/runtime Resolver 진입점이 기본 → passive → enhancement → master 순서를 유지한다.
13. 실제 runtime snapshot에는 구 Node 경로와 Resolver 경로 중 하나만 적용된다.
14. source/passive Trigger gate와 command 재귀 기준선이 승인 없이 바뀌지 않는다.
15. snapshot copy 뒤 원본 컬렉션과 결과가 변경되지 않는다.
16. `SkillExecution.cs`에는 `SkillExecution` class 하나만 남고 `SkillExecutionContext`, `SkillUseState`, `SkillExecutionState` type과 참조는 저장소에 없다.
17. `ProjectileStatusHitSpec`, `SingleDamageModifierState`, `SkillStatus`, `SingleSkillRules` type과 참조가 저장소에 없다.
18. `Implementation`에는 신규 `.cs`가 없고 승인된 6개 스크립트만 남는다.
19. 삭제 class 본문을 Resolver, Data, 기존 다른 파일 또는 private nested class로 그대로 옮긴 흔적이 없다.
20. 책임을 잃은 field, method, helper와 forwarding wrapper는 참조 0 확인 뒤 삭제된다.
21. `Implementation`의 class 수와 C# 총 줄 수가 migration 전 기준보다 감소한다.
22. family Executor와 Actor가 기존 물리적 실행 결과를 유지한다.
23. 모든 Node operation이 현재와 같은 결과를 내며 누락된 operation이 없다.
24. 관련 C# 프로젝트 빌드가 성공한다.
25. Unity console에 이 변경으로 생긴 compile error가 없다.

## Verification expected from Code Builder

```powershell
rg -n "GetOperation<" Pakuri\Assets\Scripts\Combat\Skills --glob "*.cs"
rg -n "SkillStatus\.|SingleSkillRules\." Pakuri\Assets\Scripts --glob "*.cs"
rg -n "ApplyNodes|ApplyChoiceSpec|ApplyNodeAction" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecutionData.cs
rg -n "ApplyDamage|ApplyStatus|ExtendStatusDuration|ReduceCooldownRemaining|ReduceReloadRemaining|TryExecuteRecast" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecutionRuleResolver.cs Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillTrigger.cs
rg -n "new SkillExecutionData|CreateExecutionData|ApplyChoiceSpec|ApplyNodes|ApplyDynamicDamageMultiplier|ScaleDamageMultiplier|SetRawDamageOverride|CopyWithDamageMultiplier" Pakuri\Assets --glob "*.cs"
rg -n "^\s*(public|internal|private|protected)?\s*(static\s+|sealed\s+|partial\s+)*class\s+" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecution.cs
rg -n "SkillExecutionContext|SkillUseState|SkillExecutionState|ProjectileStatusHitSpec|SingleDamageModifierState|class SkillStatus|class SingleSkillRules" Pakuri\Assets\Scripts --glob "*.cs"
rg --files Pakuri\Assets\Scripts\Combat\Skills\Implementation -g "*.cs"
dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false
dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false
git diff --check
git status --short
```

Expected structural result:

- 첫 번째 `rg`: `SkillNode.cs` wrapper와 `SkillExecutionRuleResolver.cs`만 출력.
- 두 번째 `rg`: 출력 없음.
- 세 번째 `rg`: 출력 없음.
- 네 번째 `rg`: Resolver와 Trigger의 직접 runtime 적용 호출이 출력되지 않음.
- 다섯 번째 `rg`: 승인된 DTO copy/조회 또는 Resolver 진입점 외 삭제 예정 생성·변형 API가 출력되지 않음.
- 여섯 번째 `rg`: `SkillExecution` class 한 건만 출력.
- 일곱 번째 `rg`: 출력 없음.
- `rg --files`: 승인된 6개 `.cs`만 출력.
- baseline과 final 줄 수·class 선언 수 비교: 둘 다 감소.
- 두 build: 성공.
- `git diff --check`: 오류 없음.

Unity-MCP로 editor compile과 console을 확인한다. Play Mode gameplay 검증은 사용자가 수행한다.

최소 Editor 회귀 검증:

- 기본 Definition → passive base modifier → enhancement → master 적용 순서와 계산 parity.
- cast-time 값과 hit-time 조건 계산 분리.
- 상태 데이터 clone, 지속시간, 중첩, 최대 중첩 결과.
- source-owned/passive-owned Trigger gate 비대칭 기준선.
- effect/skill 반응의 depth 8과 command의 현행 재귀·recast generation 기준선.
- Definition-only 반응 조회로 Enemy AI `CombatStart`와 DamageMeter reaction ID 해석 유지.
- snapshot copy 뒤 원본 scalar와 컬렉션 불변.

## Related board files

- Primary: `boards/COMBAT/SKILL_NODE_RUNTIME_RESOLVER_CONSOLIDATION_HANDOFF.md`
- Related completed baseline: `boards/COMBAT/SKILL_TRIGGER_REACTION_LOGIC_CONSOLIDATION_HANDOFF.md`
- Combat status follow-up: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- Routing index: `MDTREE.md`

구현 중 사실이 바뀌면 primary handoff를 갱신한다. 상태 효과 동작이나 Trigger baseline이 달라지면 관련 COMBAT board도 같은 turn에 갱신한다.

## Next Actions

1. Code Builder가 필수 역할·track 문서와 본 handoff를 읽는다.
2. Phase 1의 baseline과 Node inventory를 기록한다.
3. Phase 2부터 순서대로 구현한다.
4. 각 Phase 뒤 최소 build와 구조 `rg`를 실행하고 해당 Phase를 커밋한다.
5. 완료 뒤 primary handoff의 Status, Evidence와 History를 갱신한다.
6. 사용자가 요청한 경우에만 Code Reviewer를 실행한다.

## Evidence

- 2026-07-31 `Get-Content -Raw -Encoding UTF8`로 다음 파일을 확인했다.
  - `Definitions/Nodes/SkillNode.cs`
  - `Definitions/Nodes/SkillNodeModifiers.cs`
  - `Definitions/Nodes/SkillNodeConditions.cs`
  - `Definitions/Nodes/SkillNodeActions.cs`
  - `Implementation/SkillExecutionData.cs`
  - `Implementation/SkillExecution.cs`
  - `Implementation/SkillExecutionRuleResolver.cs`
  - `Implementation/SkillStatus.cs`
  - `Implementation/SingleSkillRules.cs`
  - `Implementation/SkillTrigger.cs`
  - `Implementation/SkillTargeting.cs`
- `rg -n "SkillStatus\.|SingleSkillRules\." Pakuri\Assets\Scripts --glob "*.cs"`
  - `SkillStatus` 호출부는 `SkillExecution.cs`, `SingleSkillActor.cs`다.
  - `SingleSkillRules` 호출부는 `SkillExecution.cs`, `SingleSkillActor.cs`다.
- `rg -n "new SkillExecutionData|ApplyChoiceSpec|ApplyNodes\(" ...`
  - Node 조합과 데이터 변형이 `SkillExecutionData`, `SkillExecution`, `SkillTrigger`, Actor에 분산되어 있음을 확인했다.
- `rg -n "SkillTargeting\." ...`
  - 대상 선택 API의 현재 소유자는 `SkillTargeting.cs`임을 확인했다.
- `rg -n "new SkillExecutionData\(|CreateExecutionData\(|ApplyChoiceSpec\(|ApplyNodes\(|CopyWithDamageMultiplier\(" Pakuri/Assets --glob "*.cs"`
  - Enemy AI, DamageMeter, Editor 테스트, Trigger, Execution과 두 Activation 호출부가 migration surface임을 확인했다.
- `SkillTrigger.cs:384,446`
  - source-owned는 기본 조건 뒤 바로 실행하고 passive-owned만 추가 상태·속성·scope와 count/proc/internal cooldown gate를 적용함을 확인했다.
- `SkillExecution.cs:89,231,501`, `SkillTrigger.cs:901`
  - 전역 depth 8은 skill/effect 반응 경로에만 있고 command는 우회하며 Zone recast만 generation을 검사함을 확인했다.
- `SkillExecutionData.cs:513,547`
  - 생성자가 즉시 Node를 적용하고 `CopyWithDamageMultiplier`는 `MemberwiseClone` 뒤 scalar만 변경함을 확인했다.
- private-set property assignment 검색
  - branch set/launch set, skill status tag/chance와 consume stacks override 필드는 writer가 없음을 확인했다.
- top-level type 선언 검색
  - `SkillExecution.cs`에 public class 4개, `SkillStatus.cs`에 top-level class 2개, `SingleSkillRules.cs`에 top-level struct와 static class가 함께 있음을 확인했다.
- `rg -n "ProjectileStatusHitSpec|SingleDamageModifierState" Pakuri/Assets/Scripts --glob "*.cs"`
  - `ProjectileStatusHitSpec`는 상태 공통 API와 Projectile/Line/Single/Zone이 공유하며, `SingleDamageModifierState`는 `SingleSkillRules`와 Single Actor 경로에만 있음을 확인했다.
- `rg --files Pakuri/Assets/Scripts/Combat/Skills/Implementation`
  - 현재 Implementation은 `.cs` 8개이며 삭제 대상 2개를 제외한 승인 최종 목록은 기존 6개다.
- `rg -n "SkillExecutionContext|SkillUseState|SkillExecutionState" Pakuri/Assets/Scripts --glob "*.cs" --glob "!SkillExecution.cs"`
  - 세 class는 Combat Activation/Implementation뿐 아니라 Units와 UI caller도 있어 class 삭제 전에 저장소 전체 caller migration이 필요함을 확인했다.

## History

- 2026-07-31: 사용자가 Node 의미의 유일한 구현자를 `SkillExecutionRuleResolver`로 지정했다.
- 2026-07-31: 사용자가 `SkillExecution`, `SkillExecutionData`, `SkillTrigger`, `SkillTargeting`, Executor와 Actor의 목표 책임을 승인했다.
- 2026-07-31: 사용자가 `SkillStatus`와 `SingleSkillRules` 삭제를 지정했다.
- 2026-07-31: Designer가 실제 코드와 호출부를 검사하고 Code Builder handoff를 작성했다.
- 2026-07-31: Code Builder가 사용자 지적 1~7을 실제 호출부와 writer/reader 흐름으로 검증했다.
- 2026-07-31: Resolver를 Node 계산 전용으로 제한하고 Trigger gate, SkillExecution runtime 적용, 외부 migration surface, 이중 적용 금지, DTO copy 불변, operation flow와 회귀 검증을 handoff에 반영했다.
- 2026-07-31: 사용자가 가능한 한 한 스크립트에 한 class만 두고, 통합을 class 본문의 물리적 이동이 아닌 책임 기반 리팩터링으로 수행하도록 지정했다.
- 2026-07-31: Designer가 current multi-class 파일을 확인하고 same-name class 분리, 불필요 책임 삭제, legacy class 붙여넣기 금지와 구조 검증 기준을 추가했다.
- 2026-07-31: 사용자가 same-name 파일 분리가 아니라 스킬 파일 수 고정과 한 기존 class로의 책임 흡수·축소가 목적이라고 정정했다.
- 2026-07-31: Designer가 신규 파일 계획을 폐기하고 최종 Implementation 6개 고정, `SkillExecution` 단일 class, 기존 계약을 통한 상태값 치환, legacy type과 caller 삭제, class/줄 수 감소 기준으로 handoff를 수정했다.
- 2026-07-31: Code Builder resumed with per-Phase GitHub commits; Phase 1 baseline/inventory was closed before runtime edits.
- 2026-07-31: Phase 2 moved Node extraction and Node action value composition to Resolver; Assembly-CSharp build passed with 0 errors.
- 2026-07-31: Phase 3 absorbed per-skill runtime state into SkillExecutionData, skill-list state into UnitSkills, removed SkillUseState and SkillExecutionState from C# callers, and passed Assembly-CSharp build with 0 errors.
- 2026-07-31: Phase 4 moved status calculation into SkillExecutionRuleResolver, reused StatusApplicationSpec for resolved status values, moved Single damage calculation and recovery orchestration to existing owners, deleted SkillStatus and SingleSkillRules with their meta files, and passed Assembly-CSharp build with 0 errors.
