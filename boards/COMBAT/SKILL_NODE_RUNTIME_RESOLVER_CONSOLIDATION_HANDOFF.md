# Skill Node Runtime Resolver Consolidation And Common Recast Handoff

## Task title

Node 의미의 런타임 구현을 Resolver로 통합하고 일반 시전과 조건부 재시전을 하나의 실행 경로로 만든다.

## Goals

- `Definitions/Nodes`의 `GetOperation<T>()` 해제와 Node 기반 값 계산을 `SkillExecutionRuleResolver`가 유일하게 소유한다.
- `SkillExecution`은 시전 검증, 실행값 생성 요청, 공통 family 분배와 시전 상태 확정을 담당한다.
- `SkillExecutionData`는 Resolver가 완성한 실행값과 진행 상태를 보관하고 전달하며, 상태 진행 제어는 `SkillExecution`이 담당한다.
- `SkillTrigger`는 Resolver가 추출한 반응 정의를 받아 사건 gate와 지연·반복을 판정하고, 성공한 스킬 결과를 `SkillExecution`의 일반 실행 진입점으로 되돌려보낸다.
- `SkillTargeting`은 대상 조건을 받아 실제 대상 목록과 중심을 반환한다.
- `SkillStatus.cs`와 `SingleSkillRules.cs`의 Node 기반 계산을 Resolver로 옮긴 뒤 두 스크립트를 삭제한다.
- 실제 스킬 반응은 raw effect를 직접 실행하지 않고 concrete family `SkillDefinition`을 통해 기본 스킬과 같은 `SkillExecution -> Executor -> Actor` 경로를 사용한다.
- Executor는 확정된 실행값을 family Actor에 전달하며 피해를 직접 적용하지 않는다. Actor는 충돌·적중을 확정하고 `DamageCalculator`와 `InGameCombatManager.ApplyDamage` 경로를 사용한다.
- 비공간 상태 명령은 스킬로 위장하지 않는다. `RecastZone`은 Zone 재시전으로 통합하고, 쿨다운·재장전·상태 지속시간 변경은 typed command로 보존하되 Trigger와 Resolver가 직접 적용하지 않는다.

## Constraints

- 사용자 승인 구조를 최우선으로 따른다.
- 모든 주장과 변경은 현재 코드와 명령 출력으로 검증한다.
- 새 Node별 handler, interface, factory, registry 또는 별도 runtime rule 스크립트를 만들지 않는다.
- Node operation 해제와 Node 기반 값 계산을 여러 Actor, Executor, Trigger, 상태 helper에 다시 분산하지 않는다.
- 기존 `SkillNode`, Node operation struct, `SkillActionContext`, `SkillExecutionData`, family Executor와 Actor를 재사용한다. 삭제 대상 context/state class를 유지하지 않는다.
- CSV schema와 authored ID·수치·확률·대상 결과·지연·반복·내부 쿨다운을 보존한다. Generation 결과 계약은 common recast를 위해 변경할 수 있다.
- Projectile, Line, Single, Zone, Buff의 이동·충돌·히트박스·수명·Visual 실행은 기존 Executor와 Actor가 유지한다.
- 일반 시전과 실제 스킬 Trigger 재실행은 같은 `SkillExecution -> Resolver -> SkillExecutionData -> Executor -> Actor` 경로를 사용한다.
- Trigger가 직접 피해, 상태, 쿨다운, 재장전 또는 Zone 재시전을 실행하지 않는다.
- `SkillExecution.TryExecuteReactionEffect -> ExecuteCastEffect -> family Executor` 같은 Trigger 전용 payload 실행 경로를 최종 구조에 남기지 않는다.
- `SkillExecution`, Projectile/Line/Single/Zone Executor와 Buff charge contact helper는 `InGameCombatManager.ApplyDamage`를 직접 호출하지 않는다.
- Resolver는 피해·상태·쿨다운·재장전·재시전 runtime API를 직접 호출하지 않는다.
- 상태 실제 적용은 `StatusCombatRules.ApplyStatus`, 피해 실제 적용은 `InGameCombatManager.ApplyDamage` 공통 경로를 유지한다.
- Trigger의 source-owned/passive-owned event/source/status/choice/scope 비대칭과 재귀 제한 범위는 보존한다. count/proc/internal-cooldown만 현재 active-skill authoring의 non-default 값이 0임을 Phase 10 테스트로 재확인한 뒤 공통화할 수 있다.
- 구 Node 조합 경로와 Resolver 조합 경로를 같은 snapshot에 함께 적용하지 않는다.
- 이번 작업에서 `Implementation` 스크립트 수를 늘리지 않는다. 임시 migration 파일도 만들지 않는다.
- 코드 컨벤션은 가능한 한 기존 `.cs` 파일 하나를 책임 class 하나로 줄이는 것이다. 여러 class를 same-name 신규 파일로 분리하지 않는다.
- 다른 스크립트와 통합할 때 기존 class 본문을 대상 파일에 그대로 붙이지 않는다. 필요한 책임의 field·method·실행 흐름만 기존 책임 class에 흡수하고, 중복되거나 책임 밖인 class와 코드는 삭제한다.
- 책임을 잃은 class, field, method, wrapper와 호환성 facade는 실제 참조가 0이고 public/serialization 호환성 검토가 끝난 뒤 삭제한다. 미래 사용 가능성만으로 남기지 않는다.
- Unity Play Mode gameplay 검증은 사용자 소유다.
- `Skill Builder`는 비활성 역할이므로 이 작업에 사용하지 않는다.
- 각 신규 Phase는 별도 Git commit으로 완료하며, 한 Phase 안에서 legacy와 신규 runtime 경로를 동시에 활성화하지 않는다.

## Role Owner

- 설계와 본 문서: Designer
- 구현: Code Builder
- 검토: 사용자 요청이 있을 때 Code Reviewer

## Status

- 사용자 구조 승인 완료.
- Code Builder가 지적 1~7을 코드로 검증하고 handoff 보강 완료.
- Phase 1 baseline/inventory 완료 및 커밋.
- Phase 2 Node composition 구현 및 빌드 검증 완료.
- Phase 7 Context 제거, 주석 추상화, 최종 정적 검증 완료.
- Phase 8 Code Reviewer 책임 수정 완료: `SkillExecutionData` 런타임 lifecycle 조정을 `SkillExecution`으로 이동하고 Core/Editor 프로젝트 빌드를 오류 0개로 통과했다.
- Phase 9 학습 런타임 목록 재구성 책임을 `UnitSkills`로 이관하고 Core/Editor 프로젝트 빌드를 오류 0개로 통과했다.
- Phase 10~15 공통 재시전, resolved Definition materialization, Trigger forwarding, Actor 피해 적용과 dead contract 정리가 완료됐다. 최종 Code Reviewer 1회 지적은 `b7037d1`에서 수정됐다.

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
- Resolver에 실행에 필요한 명시적 입력, concrete family `SkillDefinition`, 학습·선택 상태와 사건 문맥을 전달한다. 삭제 대상 `SkillExecutionContext` class에 의존하지 않는다.
- Resolver가 반환한 `SkillExecutionData`가 실행 가능한지 확인한다.
- `SkillRuntimeKind`에 맞는 기존 Executor로 분배한다.
- 수동·자동·Trigger 입력을 하나의 private 실행 흐름으로 정규화한다. public/manual/AI/Trigger 진입점은 얇은 adapter만 허용한다.
- Trigger가 통과시킨 실제 스킬 결과는 raw effect 분기 없이 같은 snapshot 생성, `ExecutePrepared`, family 분배를 사용한다.
- typed non-skill command는 스킬 실행과 구분해 기존 상태 소유 API로 전달한다. 이를 위한 Actor, prefab 또는 hidden runtime kind를 만들지 않는다.
- 실행 성공 뒤 탄창·쿨다운·연사 상태와 lifecycle을 확정한다.
- 런타임 상태 초기화, 시간 진행, 시전 진입, 적중·발사 순번과 탄창·쿨다운·재장전 진행을 조정한다.
- 일반 시전과 Trigger 재실행의 공통 진입점을 유지한다.

금지:

- `SkillNode.GetOperation<T>()` 호출.
- 특정 Node operation 직접 해석. 단, Resolver 결과를 기존 runtime API에 전달하는 실행 책임은 유지한다.
- 직접 피해 적용, 적중 대상 순회, Trigger 전용 `SkillCastEffect` payload 실행.

### `SkillExecutionData`

완성된 실행값의 전달 객체다.

허용:

- 실행 입력과 `Prepared*` 결과 보관.
- 탄창·쿨다운·시전·재장전·연사 진행값의 저장.
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
- 조건을 통과한 반응의 concrete family `SkillDefinition`과 사건 문맥을 `SkillExecution`의 일반 실행 진입점으로 전달한다.
- 반응 정의가 필요하면 Resolver가 이미 해석한 reaction 목록을 사용한다.
- `SkillReaction` 필드의 사건 gate 의미는 Trigger가 소유하며 Resolver가 대신 판정하지 않는다.
- source-owned와 passive-owned의 event/source/status/choice/scope 차이는 보존한다. count/proc/internal-cooldown은 generated OnHit outcome을 위해 공통 gate로 올리되 Phase 10 authoring baseline과 회귀 테스트를 먼저 고정한다.

이동 대상:

- `TryExecuteOutcome`의 효과 실행 결정은 삭제하고 Generation이 확정한 outcome 계약만 전달한다.
- `ExecuteCommand`의 대상 탐색과 상태·쿨다운·재장전 변경은 typed command 조정 경로로 이동한다.
- event 값은 사건 발생 시 snapshot으로 고정해 공통 실행 입력의 raw damage override로 전달한다.
- 직접 `new SkillExecutionData(...)` 또는 `SkillExecutionState.CreateExecutionData(...)`로 Node를 조합하는 경로.

### `SkillTargeting`

대상 선택 전담이다.

- 대상 진영, 선택 방식, 상태 조건, 거리, 중심, 범위, EventTarget 고정과 연쇄 대상을 해석한다.
- `SkillExecution`, Resolver, Executor 또는 Actor가 전달한 조건에 맞는 대상·중심을 반환한다.
- Node 효과, Trigger gate 또는 피해를 소유하지 않는다.

사용자가 처음 말한 “`SkillTrigger`가 대상 탐색 조건을 받아 대상을 반환”은 이후 설명과 현재 코드 근거를 따라 `SkillTargeting` 책임으로 확정한다.

### Executor and Actor

- Projectile, Line, Single, Zone Executor는 완성된 `SkillExecutionData`를 받아 기존 family Actor를 생성하고 초기화한다.
- Buff Executor는 상태·회복·보호막처럼 충돌 없는 지원 효과를 적용할 수 있지만 피해는 직접 적용하지 않는다.
- Actor는 이동, 충돌, 히트박스, Tick, 만료, Visual 수명과 물리적 적중 확정을 담당한다.
- 피해가 있는 스킬은 Actor가 `DamageCalculator`의 결과를 사용해 `InGameCombatManager.ApplyDamage`를 호출한다.
- Actor는 물리적으로 발생한 사건 문맥을 Trigger에 전달하고, Trigger가 통과시킨 후속 스킬은 다시 `SkillExecution` 일반 경로로 들어간다.
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
  -> family Actor for physical skills / immediate support application for non-damage Buff
  -> SkillExecution commits runtime state
```

### Hit-time rule

```text
Actor detects collision/tick/hit
  -> SkillExecutionRuleResolver
     - target status
     - conditional damage/critical
     - consecutive/burst/branch index
     - hit-time resolved values
  -> Actor applies damage through InGameCombatManager
  -> Actor publishes event context to SkillTrigger
```

Node 의미와 피해값은 Resolver와 `DamageCalculator`가 계산한다. Actor는 충돌·적중을 확정하고 기존 combat API로 피해를 적용한다. Executor와 `SkillExecution`은 피해를 직접 적용하지 않는다.

### Trigger reaction

```text
Actor or combat system event
  -> SkillTrigger
     - event/source/status/choice/proc/count/cooldown/delay/repeat gate
     - generated concrete outcome Definition and event snapshot forwarding
  -> SkillExecution common cast entry
  -> SkillExecutionRuleResolver
     - ordinary Definition + learned modifiers + event override
  -> SkillExecutionData
  -> same family Executor used by an initial cast
  -> new family Actor
```

실제 스킬 outcome만 위 common cast를 사용한다. `RefundCooldown`, `ReduceReload`, `ExtendStatusDuration`은 물리적 스킬이 아닌 typed state command이므로 Actor로 위장하지 않는다. Trigger와 Resolver는 직접 적용하지 않고 `SkillExecution`이 기존 `UnitSkills`/combat 상태 API로 전달한다. `RecastZone`은 command 예외에서 제거하고 concrete Zone Definition의 common recast로 전환한다.

`SkillReaction.Effect`처럼 runtime에 raw damage/status/shield payload를 남기지 않는다. Generation이 payload를 existing family concrete `SkillDefinition`으로 한 번 materialize하고, reaction은 그 Definition을 직접 참조한다. 이미 학습된 스킬 재시전도 같은 reference 계약을 사용한다. 이를 통해 Trigger 시점의 catalog 문자열 재조회와 `UnitSkills.FindBySkillId` 의존을 피한다.

## Final outcome mapping

| 현재 runtime outcome | 최종 계약 | 보존 기준 |
|---|---|---|
| `SkillReaction.TargetSkillId` | existing concrete Definition을 가리키는 resolved `SkillCastEffect` link | learned runtime lookup 없이 같은 Definition과 source attribution 사용 |
| raw `SkillCastEffect.Damage` | generated `SingleSkillDefinition` link | 현재 `ExecuteCastEffect`가 area 설정과 무관하게 `SingleSkillExecutor`를 사용하므로 임의로 Zone으로 변경하지 않음 |
| raw status-only effect | generated `BuffSkillDefinition` with `BuffEffectKind.Status` | 대상, status clone, chance, stacks, refresh, Visual 보존 |
| raw shield effect | generated `BuffSkillDefinition` with `BuffEffectKind.Shield` | base/coefficient/stat source, duration, status data, Visual 보존 |
| normal cast `SkillCastEffectOp` | 같은 resolved Definition link | delay, source aim/center inheritance와 lifecycle policy 보존 |
| `RecastZone` command | source Zone Definition common recast link | inherited snapshot, center, radius multiplier, duration, max generation 보존 |
| `RefundCooldown` | typed non-skill command | target skill, ratio, target selection 보존; Actor/Executor 생성 없음 |
| `ReduceReload` | typed non-skill command | target skill, ratio, target selection 보존; Actor/Executor 생성 없음 |
| `ExtendStatusDuration` | typed non-skill command | status kind, duration, target selection 보존; Actor/Executor 생성 없음 |
| `ApplyHitEnhancements` additional/chain damage | generated OnHit `SingleSkillDefinition` link | chance, hit period, damage multiplier, attribute, chain target/radius와 lifecycle suppression 보존 |

`SkillCastEffect`는 위 link에 필요한 Definition reference와 실행 metadata만 남긴다. raw payload를 다른 파일이나 nested class로 옮기지 않는다.

## Baseline evidence before Resolver migration

### Node meaning is currently interpreted inside data

- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillExecutionData.cs:565`
  - `ApplyNodes`가 모든 `SkillNode.GetOperation<T>()`를 직접 호출한다.
- 같은 파일 `:728` 이후
  - `SkillActionOpKind` switch와 Node별 `Apply*Action`이 의미를 구현한다.
- 따라서 초기 기준선의 `SkillExecutionData`는 수동 데이터 객체가 아니었다.

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

Phase 1~9는 이 비대칭 전체를 현행 기준선으로 보존했다. 새 Phase 13은 inspected authoring에서 active-skill non-default가 0인 count/proc/internal-cooldown만 공통화할 수 있으며 event/source/status/choice/scope 차이와 command 재귀 정책은 그대로 유지한다.

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
| `SkillReaction` | `SkillReactionOp`에서 `reactions`로 저장 | `SkillTrigger`와 `SkillExecution` | Resolver는 추출만, Trigger는 gate/예약, Execution은 generated Definition을 공통 시전 |
| `StatusStackCondition` | 조건 operation 내부 값 | Resolver, Trigger, 기존 Single 규칙 | 사용 시점의 source/target 상태 판정 |
| generated outcome Definition | Generation이 raw `SkillCastEffect`를 concrete family Definition으로 materialize | `SkillExecution` 공통 시전 | 실제 스킬 outcome의 유일한 실행 계약 |
| `SkillReactionCommand` | `SkillReaction.Command` | `SkillExecution` typed command 조정 | 비공간 상태 변경만 허용; `RecastZone` 제외 |

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
  - 검증, Resolver 호출, 공통 family 분배, typed non-skill command 조정과 성공 상태 확정을 맡는다.
  - `TryExecuteSelected`, `TryExecuteManual`, Trigger adapter가 하나의 snapshot 생성과 `ExecutePrepared`를 사용하게 한다.
  - `TryExecuteReactionEffect`, raw `ExecuteCastEffect`, Trigger outcome dispatcher와 직접 hit application helper를 caller migration 뒤 삭제한다.
- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillExecutionData.cs`
  - 값 보관과 조회만 남긴다.
- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillTrigger.cs`
  - 사건 gate와 `SkillExecution` 재진입만 남긴다.
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/*`
  - Node별 분기를 제거하고 generic prepared value 실행만 남긴다.
  - Projectile, Line, Single, Zone Actor가 적중과 피해 적용을 소유한다. Executor의 직접 `ApplyDamage`는 0건으로 만든다.

### Keep responsibility

- `Pakuri/Assets/Scripts/Combat/Skills/Implementation/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Line/LineSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Zone/ZoneSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Buff/BuffSkillExecutor.cs`
- 각 family Actor의 이동·충돌·수명 책임.

`BuffSkillExecutor`의 상태·회복·보호막 즉시 적용은 충돌 없는 support family의 기존 동작으로 유지한다. `ApplyChargeContact`의 피해 적용은 접촉을 확정한 Actor 쪽으로 옮기고 Executor에는 prepared value 전달만 남긴다.

### Migrate external callers

- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyCombatDecision.cs`
- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyActionController.cs`
- `Pakuri/Assets/Scripts/Units/Monster/Input/PlayerCombatInputController.cs`
- `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs`
- `Pakuri/Assets/Scripts/UI/InGame/DamageMeter/DamageMeterUIController.cs`
- `Pakuri/Assets/Scripts/UI/InGame/MonsterPanel/MonsterPanelUI.cs`
- `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs`

AI와 DamageMeter의 Definition-only 조회는 Resolver 진입점으로 옮긴다. `SkillUseState`와 `SkillExecutionState`를 소비하는 Unit/UI caller는 새 파일이나 호환 wrapper 없이 승인된 기존 `SkillExecution`·unit state API로 migration한다. Generation은 raw reaction payload를 final family Definition reference로 바꾸고, Editor 테스트는 `Effect/Command/TargetSkillId` 분기 수가 아니라 final skill outcome/typed state command 계약을 검증한다.

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

Status: complete. SkillExecutionData stores per-skill runtime state, SkillExecution owns runtime-state progression, UnitSkills owns active/passive runtime lists and lookup, SkillExecutionData has no Node extraction or lifecycle methods, C# callers contain no SkillUseState or SkillExecutionState, and dotnet build Pakuri/Assembly-CSharp.csproj --no-restore exits 0 with 0 errors.

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

Status: complete. Cast effect and cast-condition reads now enter through Resolver APIs, Single repeat/core/status/refund calculations are centralized, Projectile/Line/Zone use shared hit calculation, and damage/status/trigger/reload runtime application is owned by SkillExecution. Assembly-CSharp builds with 0 errors.

1. `SkillExecution`의 `SkillCastEffectOp`, `CastConditionOp`, `CountStatusDamageActionOp` Node 해석을 Resolver로 이동한다.
2. Single Actor의 repeat/core/status redistribution/hit-count refund 해석을 Resolver로 이동한다.
3. Projectile, Line, Zone의 조건부 Node 계산을 Resolver API 하나로 통일한다.
4. Resolver는 계산 결과만 반환하고 `SkillExecution`이 기존 피해·상태·쿨다운·재장전 API를 호출한다.
5. Executor와 Actor는 prepared value와 generic runtime result만 사용한다.

### Phase 6 — Trigger becomes gate-only

Status: complete. Trigger retains source/passive gate asymmetry, count/proc/cooldown gates, and command generation limits, while accepted reactions now enter SkillExecution for delay, repeat, outcome, command, targeting, and runtime application. Assembly-CSharp builds with 0 errors.

1. Trigger의 Node extraction은 Resolver로, outcome/command 실행 조정은 SkillExecution 반응 경로로 이동한다.
2. Trigger의 직접 대상 탐색과 runtime 상태 변경을 제거한다.
3. Trigger가 통과한 사건은 항상 SkillExecution으로 반환한다.
4. Trigger 지연, 반복, proc, count와 내부 쿨다운 의미를 보존한다.
5. source/passive gate 비대칭과 command 재귀 우회는 별도 사용자 결정 없이는 변경하지 않는다.

### Phase 7 — cleanup and verification

Status: complete. `SkillExecutionContext` was absorbed into the existing `SkillActionContext`, `SkillExecution.cs` has one top-level class, every method under `Combat/Skills` has a concise abstract comment, and Assembly-CSharp builds with 0 errors. Legacy-symbol, six-file, Resolver-boundary, comment, and diff checks pass.

1. `SkillExecutionContext`, `SkillUseState`, `SkillExecutionState`, `ProjectileStatusHitSpec`, `SingleDamageModifierState`, `SkillStatus`, `SingleSkillRules` caller를 모두 기존 책임 API로 전환한다.
2. 위 타입과 책임을 잃은 class, 중복 helper, 사용되지 않는 field/method를 참조 0 확인 뒤 삭제한다.
3. `Implementation`에 신규 `.cs`가 없고 최종 6개 파일만 남는지 검사한다.
4. `SkillExecution.cs`에는 `SkillExecution` class 하나만 남고, 삭제 class 본문을 다른 파일에 붙이거나 nested class/wrapper로 숨기지 않았는지 검사한다.
5. Node operation 소비가 Resolver 하나인지 검사한다.
6. 빌드, diff, Unity compile/console을 검증한다.
7. 관련 board에 구현 결과와 실제 명령 출력을 기록한다.

### Phase 8 — Code Reviewer responsibility correction

Status: complete. Code Reviewer found that `SkillExecutionData` still owned runtime lifecycle methods. Code Builder moved runtime-state coordination into the existing `SkillExecution`, migrated all repository callers without adding files, exposed the Definition-only Resolver entry points required by Editor tests, and passed both Core and Editor builds with 0 errors and 2 existing reference warnings each.

1. `SkillExecutionData` keeps runtime values and Resolver-facing data access only.
2. `SkillExecution` owns runtime reset, tick, cast entry, hit/launch counters, burst progress, cooldown, and reload progression.
3. `UnitSkills`, recovery flow, family Executors, Actors, and editor tests call the new `SkillExecution` owner methods.
4. Existing six-file Implementation scope, one `SkillExecution` class, Resolver boundary, Trigger gate asymmetry, and legacy-symbol deletion remain unchanged.

### Phase 9 — learned runtime state ownership

Status: complete. `RebuildLearnedSkillState` was removed from `SkillExecution` and integrated into the existing `UnitSkills` state owner. The new method reuses `Clear`, `HasActiveSkill`, `HasPassiveSkill`, and `AddOrReplace`; all runtime, spawn, UI, and Editor test callers use `model.SkillState.RebuildLearnedSkillState(...)`. Core and Editor builds passed with 0 errors and 2 existing reference warnings each.

1. `UnitSkills` resolves catalog definitions for the existing active slots when callers do not provide definitions.
2. `UnitSkills` filters definitions through the learned ID sets and rebuilds the runtime list through existing list APIs.
3. `SkillExecution` no longer owns catalog lookup, active-slot selection, or learned-state reconstruction.
4. Existing behavior and caller coverage remain unchanged; no new script or compatibility facade was added.

### Phase 10 — common recast baseline and contract

Status: complete. Baseline/inventory commit: `593f70c`. Current build baseline: Core/Editor compile passed with 0 errors; Unity EditMode execution remains blocked by another Unity instance using the project.

1. Fix the current reaction outcome baseline from Generation and Editor tests: raw effect, learned-skill reference, typed command, and missing outcome counts.
2. Record every caller of `TryExecuteReaction`, `TryExecuteReactionEffect`, `ExecuteCastEffect`, `ExecuteTriggeredReaction`, `ApplyResolvedHits`, and `ApplyHitEnhancements`.
3. Record every direct `InGameCombatManager.ApplyDamage` call under `SkillExecution`, family Executors, and family Actors.
4. Fix normal cast and Trigger cast differences for cast-state consumption, lifecycle publication, target locking, event center, damage override, depth, and recast generation.
5. Add a focused test that proves the current learned-skill reaction reaches `ExecutePrepared -> ExecuteSkill` and another that proves a raw effect currently bypasses it. The second test is the migration baseline, not the final expectation.
6. Commit the inventory and tests separately.

### Phase 11 — materialize skill outcomes during Generation

Status: complete. Generation materialization implementation commit: `e81d7ed`; current Phase 15 cleanup removes the remaining raw payload contract. Core/Editor build remains 0 errors.

1. Reuse `SkillCastEffect` as a small resolved execution link instead of adding a new outcome class or script.
2. The final link stores a concrete family `SkillDefinition` plus only execution metadata that cannot live in the Definition: delay, damage multiplier, source aim/center inheritance, event-target lock, lifecycle policy, and optional event damage override policy.
3. Remove raw damage, status, shield, targeting, area, prefab, visual, duration, and status-extension payload ownership from the runtime `SkillCastEffect`; Generation writes those values into the concrete family Definition once.
4. Existing learned-skill outcomes point to the existing Definition. Raw effect outcomes materialize an auxiliary Single, Zone, or Buff Definition but do not register it as a learned active skill or add a runtime kind/Executor.
5. Replace the `SkillReaction.TargetSkillId`/raw `Effect` dual path with one resolved skill outcome link. Keep one typed non-skill command alternative.
6. Convert `RecastZone` to a resolved Zone outcome link while preserving inherited snapshot, radius, duration, center and max-generation values.
7. Preserve current CSV schema and authored values. Only Generation and runtime contracts change.
8. Commit Generation and catalog tests before routing runtime execution through the new link.
9. Inventory `AdditionalDamageActionOp` and `HitChainDamageActionOp` values consumed by `ApplyHitEnhancements`. If they become generated OnHit skill outcomes, materialize their concrete Definitions in this Phase instead of retaining raw damage fields.

### Phase 12 — one SkillExecution cast pipeline

Status: complete. Common cast routing implementation commit: `e81d7ed`; normal and resolved reactions now enter `ExecutePrepared` and use the single `ExecuteSkill` family switch. Core/Editor build: 0 errors.

1. Refactor the existing `TryExecuteSkill` rather than adding a request class, interface, factory, or new script.
2. `TryExecuteSelected`, `TryExecuteManual`, AI routing and Trigger execution become thin adapters into the same snapshot creation and `ExecutePrepared` path.
3. Separate persistent cast-state ownership from the executed Definition. Normal casts use their learned runtime and consume cast state; generated reaction Definitions can execute without entering `UnitSkills` and without consuming an unrelated skill cooldown.
4. Preserve `beginCast` behavior under a clearer cast-state flag. Same path does not mean every Trigger reaction consumes cooldown, magazine or cast interval.
5. Apply event target, center, raw damage override, damage multiplier, lifecycle policy, recast generation and recursion depth before `ExecutePrepared`.
6. Remove direct family Executor selection from the Trigger/cast-effect adapter. `ExecuteSkill` remains the only family switch.
7. Commit after Core and Editor builds pass and both normal/Trigger tests reach the same `ExecutePrepared -> ExecuteSkill` path.

### Phase 13 — Trigger gate and scheduling only

Status: complete. Trigger forwarding and scheduling implementation commit: `e81d7ed`; gate asymmetry remains explicit, accepted skill outcomes use the resolved common execution entry, and typed state commands remain the non-skill exception. Core/Editor build: 0 errors.

1. Keep event, source scope, status, attribute, choice, count, proc, internal cooldown, delay and repeat ownership in `SkillTrigger`.
2. Snapshot event-derived values before delay so later shield/status changes do not alter the queued reaction.
3. After the gate and delay, forward the resolved skill outcome link and event snapshot to the common `SkillExecution` entry.
4. Delete `ExecuteTriggeredReaction`, `ExecuteTriggeredReactionOnce`, `ExecuteTriggeredOutcome`, `TryExecuteReactionEffect`, raw `ExecuteCastEffect` branches and `ResolveTriggeredRawDamage` after callers reach the common entry.
5. Keep typed non-skill commands outside the Actor path. Delete the Trigger-specific outcome switch; one generic command adjustment path may remain in `SkillExecution` for cooldown, reload and status-duration changes.
6. Before using `ProcChance`, `EveryCount` or `InternalCooldownSeconds` for generated source-owned OnHit outcomes, re-run the current authoring inventory. The inspected baseline is active-skill reactions 37/non-default 0 and passive reactions 126/non-default 13.
7. If the Phase 10 inventory matches, apply count/proc/internal-cooldown through one shared gate for source-owned and passive-owned reactions. Preserve all other source/passive gate differences.
8. Commit after depth, delay/repeat, proc/count and recast-generation tests pass.

### Phase 14 — Actor owns physical hit application

Status: complete. Actor physical-hit ownership commit: `6e7ba5a`; `SkillExecution` and family Executors no longer apply physical damage, while Actor paths retain collision confirmation and combat API application. Core/Editor build: 0 errors.

1. Projectile, Line, Single and Zone Actors retain collision/target confirmation and call the common damage calculator/combat manager path.
2. Replace `ZoneSkillActor -> SkillExecution.ApplyResolvedHits` with Zone Actor-owned target iteration using existing `SkillTargeting` and Resolver values; do not add an Actor base class for one shared helper.
3. Replace Actor calls to `SkillExecution.ApplyHitEnhancements`. Actual additional/chain skill outcomes re-enter `SkillTrigger -> SkillExecution` as resolved skill outcomes; cooldown/reload adjustments remain typed non-skill behavior.
4. Move Buff charge contact damage out of `BuffSkillExecutor.ApplyChargeContact` to the contact-owning Actor path. Keep status, heal and shield support execution unchanged unless their behavior requires an Actor.
5. Delete `ApplyResolvedHits` and `ApplyHitEnhancements` after references reach zero.
6. Verify that `SkillExecution` and every `*SkillExecutor.cs` contain zero `ApplyDamage` calls.
7. Commit after physical Actor and support-family tests/builds pass.

### Phase 15 — dead contract removal and final verification

Status: complete. Final cleanup commit: `5213b14`; Code Reviewer found one recast-generation guard regression and Code Builder fixed it in `b7037d1`. Core/Editor builds passed with 0 errors; Unity EditMode remains blocked by another Unity instance using the project.

1. Remove obsolete `SkillReaction.TargetSkillId`, raw `SkillReaction.Effect` payload fields, unsupported `RecastZone` command shape and runtime readers after Generation migration.
2. Remove obsolete raw fields from `SkillCastEffect`; keep only the resolved Definition link and required execution metadata if normal cast follow-ups still use it.
3. Update `SkillCatalogRuntimeTests` from outcome-kind counts to concrete Definition family/reference, typed command, dynamic-value, target, visual and timing parity.
4. Confirm no new `.cs`, runtime kind, Executor, Actor base class, compatibility wrapper or catalog lookup layer was introduced.
5. Run static searches, Core/Editor builds, Unity compile/console and focused EditMode tests.
6. Keep Play Mode gameplay verification user-owned; no further runtime implementation remains in this Phase.

## Compatibility requirements

- 모든 기존 Node operation의 계산 순서와 합산·곱산 방식을 보존한다.
- 기본 Skill Node 적용 뒤 passive base modifier, enhancement, master choice를 적용하는 현재 순서를 보존한다.
- 조건부 값은 현재와 같은 시점에 판정한다.
  - 시전 조건: 시전 전.
  - 대상 상태 조건부 피해: 실제 적중 시점.
  - burst/launch 조건: 해당 발사 순번 확정 시점.
  - kill 효과: 피해 결과가 사망으로 확정된 뒤.
  - Trigger gate: 사건 발생 뒤, 지연 시작 전 필요한 사건값을 고정.
- `MaxTriggeredExecutionDepth`, recast generation과 lifecycle 발행 정책을 약화하지 않는다. 공통 진입점으로 옮길 때 command가 새 재귀 우회로를 만들지 않게 한다.
- source-owned/passive-owned Trigger의 event/source/status/choice/scope 비대칭은 behavior baseline으로 보존한다. count/proc/internal-cooldown은 active-skill non-default 0 baseline이 유지될 때만 공통화한다.
- player-facing behavior, Unity serialization, prefab와 asset 참조를 보존한다.
- 삭제 대상 public type의 저장소 내부 caller는 새 책임 API로 모두 migration한다. 구 type 이름을 보존하기 위한 wrapper는 만들지 않는다.
- CSV schema와 authored 값은 변경하지 않는다. Generation 결과는 raw runtime payload에서 resolved concrete Definition link로 변경한다.
- Trigger outcome Definition이 학습 목록에 없어도 passive/enhancement/master 보정, source skill attribution, DamageMeter 이름, lifecycle과 kill/outgoing-damage source ID가 현재 결과와 같아야 한다.
- 지원형 Buff의 상태·회복·보호막 즉시 적용은 보존한다. “Executor는 피해를 적용하지 않는다”를 “모든 지원 효과도 Actor를 만들어야 한다”로 확대 해석하지 않는다.

## Edge cases and risks

- Resolver가 커지더라도 Node별 class나 handler를 새로 만들지 않는다. 영역별 private method로만 정리한다.
- cast-time과 hit-time 값을 한 시점에 강제로 확정하지 않는다.
- Trigger reaction 목록은 Trigger가 Node를 직접 읽지 않도록 Resolver가 제공한다.
- passive Trigger는 활성 Actor snapshot이 없어도 Resolver를 통해 학습 상태에서 reaction을 얻을 수 있어야 한다.
- 현재 raw effect outcome은 학습 runtime이 없으므로 `UnitSkills.FindBySkillId`만으로 공통 시전할 수 없다. Generation이 concrete family Definition reference를 만들고 `SkillExecution`이 persistent runtime과 executed Definition을 분리해야 한다.
- auxiliary outcome Definition은 실행을 위해 필요하지만 learned catalog/slot에 등록하지 않는다. 이를 UI 노출, 쿨다운 소유 또는 별도 스킬 학습으로 취급하지 않는다.
- `RefundCooldown`, `ReduceReload`, `ExtendStatusDuration`을 Actor skill로 위장하지 않는다. 이들은 typed non-skill command이며 공통 스킬 시전 보장의 적용 대상이 아니다.
- `RecastZone`은 실제 스킬 재시전이므로 typed command에 남기지 않는다. inherited snapshot, radius, duration과 generation을 잃으면 무한 recast 또는 수치 회귀가 생긴다.
- 상태 데이터 clone과 catalog fallback을 보존한다.
- `SkillExecutionData` copy 시 컬렉션을 수정하지 않는 불변 조건으로 원본 실행값 오염을 막는다. 이후 변경이 필요해질 때만 deep copy를 추가한다.
- Actor가 같은 적중을 Resolver와 Trigger에 중복 전달하지 않게 한다.
- 일반 시전과 Trigger 재실행의 쿨다운·탄창 소비 차이를 보존한다.
- 사건 피해, 보호막 적용량·잔량·흡수량과 추적 피해는 delay 전에 값으로 고정한다. 지연 뒤 live 상태를 다시 읽지 않는다.
- generated outcome의 `SourceSkillId`와 표시 ID를 분리하지 않으면 DamageMeter, kill/outgoing-damage Trigger, lifecycle source 판정이 바뀔 수 있다.
- lifecycle을 켠 Trigger outcome은 다시 `OnCast`/`OnHit` 반응을 만들 수 있다. depth 8과 recast generation을 공통 진입점에서 검사해 무한 루프를 막는다.
- raw effect에서 concrete Definition으로 옮길 때 targeting, center inheritance, event-target lock, visual prefab/runtime visual, duration, status clone과 critical policy가 누락되기 쉽다. operation별 migration 표와 parity test가 필요하다.
- `ApplyHitEnhancements`는 OnHit publication, hit counter, reload reduction, additional damage와 chain damage를 함께 갖는다. 메소드만 삭제하지 말고 각 동작을 skill outcome, typed command 또는 Actor event로 분류해 이관한다.
- additional/chain 값을 일반 source-owned reaction으로 옮길 경우 현재 source-owned gate에는 passive와 같은 count/proc/internal-cooldown 판정이 없다. inspected CSV는 active-skill 37개 모두 default이고 passive 126개 중 13개만 non-default이므로, Phase 10 재확인 뒤 이 세 gate만 공통화하는 것이 별도 hit-enhancement counter를 남기는 것보다 단순하다.
- `BuffSkillExecutor`는 상태·회복·보호막도 직접 적용한다. 피해 금지 규칙은 charge contact `ApplyDamage`에 적용하고 support effect까지 가짜 Actor로 만들지 않는다.
- normal cast의 `SkillCastEffectOp`도 같은 raw payload 타입을 사용한다. Trigger reader만 먼저 삭제하면 일반 cast follow-up이 손실되므로 Generation과 runtime reader를 함께 inventory한다.
- migration 중 raw effect와 generated Definition을 동시에 실행하면 피해·상태·Visual이 이중 적용된다. 각 Phase runtime은 한 outcome 경로만 사용한다.
- Resolver 비교용 snapshot을 실제 runtime snapshot에 덧씌우지 않는다.
- 물리적 class 이동만으로 책임 통합이 끝났다고 판단하지 않는다. caller와 계산 소유권이 목표 경계로 바뀌어야 한다.
- 삭제 후보를 빈 wrapper나 forwarding method로 남기지 않는다. 실제 public/serialization 호환성 근거가 있는 경우만 예외를 기록한다.

### Validated better choices

- 새 execution request class보다 기존 `TryExecuteSkill`과 `ExecutePrepared`를 확장하는 편이 파일·추상화·분기 수가 적다.
- runtime `TargetSkillId` 재조회보다 Generation이 확정한 `SkillDefinition` direct reference가 안전하다. learned skill과 auxiliary outcome을 같은 계약으로 실행하고 missing catalog/learned-runtime 분기를 없앤다.
- 모든 reaction 결과를 Actor로 강제하는 것보다 실제 skill outcome과 typed state command를 분리하는 편이 책임에 맞다. 동일 경로 보장은 피해·상태·방어막 등 family skill outcome에 적용하고, 쿨다운·재장전·상태 지속시간 변경은 command 예외로 명시한다.
- support Buff는 현재 즉시 적용을 유지하고 피해가 있는 charge contact만 Actor hit path로 옮기는 것이 최소 변경이다.

## Acceptance criteria

1. `SkillExecutionRuleResolver`만 `Definitions/Nodes`의 `GetOperation<T>()`와 Node 기반 값 계산을 수행한다.
2. `SkillExecutionData`에 `GetOperation<T>()`, `ApplyNodes`, `ApplyChoiceSpec`, Node별 `Apply*Action`이 없다.
3. `SkillExecution`, `SkillTrigger`, Executor와 Actor가 Node operation 타입을 직접 검사하지 않는다.
4. `SkillStatus.cs`, `SkillStatus.cs.meta`, `SingleSkillRules.cs`, `SingleSkillRules.cs.meta`가 삭제된다.
5. `SkillStatus.`와 `SingleSkillRules.` 참조가 0건이다.
6. `SkillTrigger`가 직접 피해·상태·쿨다운·재장전·재시전을 적용하지 않는다.
7. Trigger 조건을 통과한 actual skill outcome이 일반 시전과 같은 `SkillExecution -> ExecutePrepared -> ExecuteSkill -> family Executor` 경로를 사용한다.
8. Trigger가 사건 gate를 소유하고 Resolver는 gate를 대신 판정하지 않는다.
9. `SkillExecutionRuleResolver`에 피해·상태·쿨다운·재장전·재시전 runtime API 호출이 없다.
10. `SkillExecution`과 모든 `*SkillExecutor.cs`에 직접 `ApplyDamage` 호출이 없다.
11. Definition-only Resolver 진입점으로 Enemy AI와 DamageMeter 반응 조회가 유지된다.
12. owner/runtime Resolver 진입점이 기본 → passive → enhancement → master 순서를 유지한다.
13. 실제 runtime snapshot에는 구 Node 경로와 Resolver 경로 중 하나만 적용된다.
14. source/passive Trigger의 event/source/status/choice/scope 비대칭은 유지되고, count/proc/internal-cooldown만 active-skill non-default 0 baseline 아래 공통화된다.
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
26. `SkillExecutionData`에는 runtime lifecycle method declaration이 없고, 해당 조정 method는 `SkillExecution`에만 있다.
27. runtime raw effect outcome이 concrete Single/Zone/Buff Definition link로 materialize되며 Trigger 시점에 family Executor를 직접 고르지 않는다.
28. learned-skill outcome과 auxiliary generated outcome이 같은 common cast entry를 사용한다.
29. auxiliary outcome Definition은 `UnitSkills` active/passive 목록과 플레이어 선택 슬롯에 등록되지 않는다.
30. `RecastZone`은 typed command가 아니라 common Zone recast이며 max generation, inherited snapshot, center, radius와 duration을 보존한다.
31. typed command에는 cooldown refund, reload reduction, status-duration extension 같은 비공간 상태 변경만 남는다.
32. `TryExecuteReactionEffect`, raw `ExecuteCastEffect` family branches, `ExecuteTriggeredReaction`, `ExecuteTriggeredReactionOnce`, `ExecuteTriggeredOutcome`, `ExecuteTriggeredCommand`, `CommandRuntimes`, `ResolveTriggeredRawDamage`, `ApplyResolvedHits`, `ApplyHitEnhancements`와 해당 참조가 0건이다.
33. event-derived raw damage 값은 delay 전 snapshot으로 고정되고 common execution data override로 전달된다.
34. Projectile, Line, Single, Zone과 Buff charge 피해는 Actor가 확정한 적중 뒤 기존 combat API로 한 번만 적용된다.
35. Buff status/heal/shield 결과는 기존 값·대상·Visual·지속시간을 유지하며 불필요한 Actor를 새로 만들지 않는다.
36. 새 runtime kind, Executor, Actor base class, request class, interface, factory, registry, compatibility wrapper가 없다.
37. `SkillCatalogRuntimeTests`가 기존 raw effect/learned reference/command count 대신 final Definition family/reference와 typed command parity를 검증한다.
38. 각 Phase가 별도 commit이며 Phase별 build/test 결과와 commit hash가 이 문서와 관련 board에 기록된다.
39. `ApplyHitEnhancements`의 additional chance, hit-chain period, hit count, reload reduction과 OnHit 순서가 보존되고 source-owned Trigger에는 검증된 count/proc/internal-cooldown 공통 gate 외 다른 passive gate가 추가되지 않는다.

## Verification expected from Code Builder

```powershell
rg -n "GetOperation<" Pakuri\Assets\Scripts\Combat\Skills --glob "*.cs"
rg -n "SkillStatus\.|SingleSkillRules\." Pakuri\Assets\Scripts --glob "*.cs"
rg -n "ApplyNodes|ApplyChoiceSpec|ApplyNodeAction" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecutionData.cs
rg -n "ApplyDamage|ApplyStatus|ExtendStatusDuration|ReduceCooldownRemaining|ReduceReloadRemaining|TryExecuteRecast" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecutionRuleResolver.cs Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillTrigger.cs
rg -n "new SkillExecutionData|CreateExecutionData|ApplyChoiceSpec|ApplyNodes|ApplyDynamicDamageMultiplier|ScaleDamageMultiplier|SetRawDamageOverride|CopyWithDamageMultiplier" Pakuri\Assets --glob "*.cs"
rg -n "^\s*(public|internal|private|protected)?\s*(static\s+|sealed\s+|partial\s+)*class\s+" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecution.cs
rg -n "^\s*(public|internal|private|protected).*\b(ResetRuntimeState|AdvanceProjectileLaunchCount|AdvanceSkillHitCount|ConsecutiveHitDamageMultiplier|Tick|CanCastWithData|TryBeginCast|StopActive|CurrentBurstProjectileIndex|ReduceReloadRemaining|ReduceCooldownRemaining|ResetCooldown|TickDown|RefreshRuntimeModifiers|BeginRecoveryIfNeeded)\s*\(" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecutionData.cs
rg -n "SkillExecutionContext|SkillUseState|SkillExecutionState|ProjectileStatusHitSpec|SingleDamageModifierState|class SkillStatus|class SingleSkillRules" Pakuri\Assets\Scripts --glob "*.cs"
rg -n "TryExecuteReactionEffect|ExecuteTriggeredReaction|ExecuteTriggeredOutcome|ExecuteTriggeredCommand|CommandRuntimes|ResolveTriggeredRawDamage|ApplyResolvedHits|ApplyHitEnhancements" Pakuri\Assets --glob "*.cs"
rg -n "ApplyDamage\(" Pakuri\Assets\Scripts\Combat\Skills\Implementation Pakuri\Assets\Scripts\Combat\Skills\Activation --glob "*SkillExecutor.cs"
rg -n "SingleSkillExecutor\.Execute|BuffSkillExecutor\.Execute|ZoneSkillExecutor\.Execute|ProjectileSkillExecutor\.Execute|LineSkillExecutor\.Execute" Pakuri\Assets\Scripts\Combat\Skills\Implementation\SkillExecution.cs
rg -n "new SkillCastEffect|\.Effect\b|\.Command\b|TargetSkillId" Pakuri\Assets\Scripts\Loading\Generation Pakuri\Assets\Scripts\Combat\Skills Pakuri\Assets\Tests --glob "*.cs"
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
- Trigger 전용 실행/helper 검색: 출력 없음.
- `ApplyDamage` 검색: `SkillExecution`과 모든 Executor에서 출력 없음. Projectile/Line/Single/Zone 및 charge contact 소유 Actor의 승인된 호출만 남음.
- family Executor 호출 검색: `SkillExecution.ExecuteSkill`의 한 family switch에서만 출력되고 raw cast-effect/Trigger branch에는 출력 없음.
- reaction contract 검색: raw runtime payload writer/reader가 없고, typed non-skill command와 Generation의 resolved Definition link만 출력.
- `rg --files`: 승인된 6개 `.cs`만 출력.
- baseline과 final 줄 수·class 선언 수 비교: 둘 다 감소.
- 두 build: 성공.
- `git diff --check`: 오류 없음.

Unity-MCP로 editor compile과 console을 확인한다. Play Mode gameplay 검증은 사용자가 수행한다.

최소 Editor 회귀 검증:

- 기본 Definition → passive base modifier → enhancement → master 적용 순서와 계산 parity.
- cast-time 값과 hit-time 조건 계산 분리.
- 상태 데이터 clone, 지속시간, 중첩, 최대 중첩 결과.
- source-owned/passive-owned Trigger의 event/source/status/choice/scope 비대칭 기준선과 count/proc/internal-cooldown 공통화 결과.
- effect/skill 반응의 depth 8과 command의 현행 재귀·recast generation 기준선.
- normal/manual/AI/learned-reaction/generated-reaction이 같은 `ExecutePrepared -> ExecuteSkill` 경로를 사용함.
- generated Single/Zone/Buff outcome의 damage/status/shield, targeting, visual, duration과 source attribution parity.
- event damage/shield 값이 delay 전에 고정되고 지연 실행 뒤 바뀌지 않음.
- `RecastZone`의 inherited snapshot, center, radius, duration과 max generation.
- Actor 적중당 피해·OnHit publication이 정확히 한 번이고 추가/chain outcome이 common recast를 사용함.
- typed cooldown/reload/status-duration command가 Actor나 family Executor를 생성하지 않고 기존 결과를 유지함.
- source-owned reaction의 current non-default proc/count/internal-cooldown inventory와 migrated additional/chain outcome parity.
- Definition-only 반응 조회로 Enemy AI `CombatStart`와 DamageMeter reaction ID 해석 유지.
- snapshot copy 뒤 원본 scalar와 컬렉션 불변.

## Related board files

- Primary: `boards/COMBAT/SKILL_NODE_RUNTIME_RESOLVER_CONSOLIDATION_HANDOFF.md`
- Related completed baseline: `boards/COMBAT/SKILL_TRIGGER_REACTION_LOGIC_CONSOLIDATION_HANDOFF.md`
- Combat status follow-up: `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- Data contract follow-up: `boards/DATA/DATA_BLACKBOARD.md`
- Routing index: `MDTREE.md`

구현 중 사실이 바뀌면 primary handoff를 갱신한다. 상태 효과 동작이나 Trigger baseline이 달라지면 관련 COMBAT board도 같은 turn에 갱신한다.

## Next Actions

1. Phase 1~15 구현과 각 Phase 기록 커밋은 완료됐다.
2. Core/Editor 빌드와 정적 경계 검증은 완료됐다.
3. Unity EditMode는 다른 Unity 인스턴스가 프로젝트를 점유해 실행하지 못했으므로 사용자 환경에서 재실행한다.
4. Play Mode gameplay 검증은 사용자가 수행한다.

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
- `rg -n "SkillExecution\\.RebuildLearnedSkillState" Pakuri --glob "*.cs"`
  - 구 `SkillExecution` API 참조가 0건임을 확인했다.
- `rg -n "RebuildLearnedSkillState\\(" Pakuri/Assets/Scripts/GameFlow/UnitSkills.cs Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs Pakuri/Assets/Scripts/UI/InGame/DebugUI.cs Pakuri/Assets/Scripts/UI/InGame/InGameUIManager.cs Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`
  - 새 `UnitSkills` 인스턴스 API와 모든 저장소 호출부가 연결됨을 확인했다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal /p:UseSharedCompilation=false`, `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore -v:minimal /p:UseSharedCompilation=false`
  - 두 프로젝트 모두 오류 0개, 기존 참조 경고 2개로 통과했다.
- 2026-07-31 common recast 설계 재검증:
  - `TryExecuteSelected`와 `TryExecuteManual`은 `TryExecuteSkill -> ExecutePrepared -> ExecuteSkill`을 사용한다.
  - `SkillReaction.TargetSkillId` 경로는 `TryExecuteReaction -> ExecutePrepared`에 도달하지만 raw `SkillReaction.Effect`는 `TryExecuteReactionEffect -> ExecuteCastEffect`에서 family Executor를 직접 선택한다.
  - `SkillTrigger.cs:383,451`은 gate 통과 뒤 `SkillExecution.ExecuteTriggeredReaction`을 호출한다.
  - `SkillExecution.cs:2030,2112`의 `ApplyResolvedHits`와 `ApplyHitEnhancements`가 직접 피해를 적용하며 Zone/Projectile/Line/Single Actor caller가 남아 있다.
  - `BuffSkillExecutor.cs:173`의 charge contact path가 Executor에서 직접 피해를 적용한다. 같은 Executor의 status/heal/shield는 충돌 없는 support 적용이므로 별도 보존 대상이다.
  - `GameDataCatalogBuilder.Nodes.cs`는 `EffectDamage`, `ApplyStatus`, `ApplyShield`, `RecastZone`, `RefundCooldown`, `ReduceReload`, `ExtendStatusDuration`을 raw effect 또는 typed command로 생성한다.
  - `SkillCatalogRuntimeTests.cs`는 `Effect`, `TargetSkillId`, `Command` outcome 분포와 RecastZone command 값을 직접 검증하므로 contract migration surface다.
  - current DATA board baseline은 final reactions를 effect 57, learned-skill reference 4, command 21, missing 0으로 기록한다. Code Builder가 Phase 10에서 현재 실행 결과로 재확정한다.
  - Trigger CSV `Import-Csv` 집계는 active-skill files 37 rows/non-default proc·count·internal-cooldown 0, passive file 126 rows/non-default 13이다. generated additional/chain outcome을 위해 이 세 gate만 공통화해도 current active authoring 결과는 바뀌지 않는다.
- 2026-07-31 Phase 10~14 기록 commits: `05e5b22`, `22e8516`, `3075a5d`, `55ca337`, `dfa7d53`; runtime implementation commits: `e81d7ed`, `6e7ba5a`.
- 2026-07-31 Phase 15 `5213b14`: `SkillCastEffect`를 resolved Definition link와 실행 metadata로 축소하고, Generation에서 Single/Zone/Buff 결과를 직접 materialize했으며, RecastZone을 공통 Zone 결과로 전환했다.
- 2026-07-31 Code Reviewer 1회: `MaxGeneration`이 resolved recast 진입점에서 검사되지 않는 문제를 지적했다. Code Builder가 `TryExecuteResolvedEffect`에 `recastGeneration >= MaxGeneration` guard를 복원해 `b7037d1`로 커밋했다.
- 2026-07-31 수정 후 `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`와 `Assembly-CSharp-Editor.csproj`가 각각 `빌드했습니다.`로 종료했다. legacy helper/legacy type/direct `ApplyDamage` 경계 검색은 출력이 없었고 `git diff --check`도 통과했다.
- 2026-07-31 Unity EditMode 실행은 다른 Unity 인스턴스가 같은 프로젝트를 열고 있어 batchmode가 중단됐다. Play Mode와 실제 gameplay 검증은 사용자 소유다.

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
- 2026-07-31: Phase 5 moved cast/repeat/core/status/refund value resolution into SkillExecutionRuleResolver, moved shared damage/status/trigger/reload application into SkillExecution, unified family hit multipliers, and passed Assembly-CSharp build with 0 errors.
- 2026-07-31: Phase 6 kept Trigger gate asymmetry and command generation limits, routed accepted reactions to SkillExecution for delay/repeat/outcome/command/targeting/runtime application, and passed Assembly-CSharp build with 0 errors.
- 2026-07-31: Phase 7 absorbed `SkillExecutionContext` into existing `SkillActionContext`, normalized concise abstract comments across `Combat/Skills`, confirmed six Implementation scripts and one `SkillExecution` class, found zero legacy symbols and zero runtime-application calls in Resolver, and passed `git diff --check` plus `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` with 0 errors and 2 existing reference warnings.
- 2026-07-31: Code Reviewer found runtime lifecycle methods still owned by `SkillExecutionData`; Code Builder moved them into `SkillExecution`, migrated repository callers, exposed the Definition-only Resolver entry points, changed the Editor test to inspect the internal data mutator through its existing reflection pattern, confirmed zero lifecycle method declarations remain in `SkillExecutionData`, and passed both Core and Editor builds with 0 errors and 2 existing reference warnings each.
- 2026-07-31: Code Builder moved learned runtime-state reconstruction from `SkillExecution` into the existing `UnitSkills` class, reused its existing learning checks and runtime-list APIs, migrated spawn/UI/Editor callers, and passed Core/Editor builds with 0 errors and 2 existing reference warnings each.
- 2026-07-31: 사용자가 조건부 스킬 결과도 Actor 사건 뒤 기본 스킬과 동일한 `SkillExecution -> Executor -> Actor` 경로로 다시 시전하고 Executor가 피해를 직접 적용하지 않도록 구조를 정정했다.
- 2026-07-31: Designer가 current normal/learned reaction/raw effect/command 경로와 direct damage caller를 재검증하고 Phase 10~15 common recast handoff를 추가했다.
- 2026-07-31: 자체 검증에서 raw effect는 learned runtime이 없어 direct `TargetSkillId` lookup만으로 실행할 수 없고, non-spatial command를 Actor로 강제하면 가짜 스킬이 필요함을 확인했다. 최종 설계는 Generation-resolved concrete Definition link와 typed state-command 예외를 사용한다.
