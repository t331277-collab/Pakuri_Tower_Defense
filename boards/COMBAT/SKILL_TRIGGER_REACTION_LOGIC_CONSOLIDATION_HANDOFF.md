# Skill Trigger Reaction Logic Consolidation Handoff

## Task title

별도 Trigger 스킬 Definition을 제거하고 사건 조건과 기존 스킬 실행을 공통 경로로 통합한다.

## Goals

- `SkillTrigger.cs`는 전투 사건과 조건만 판단한다.
- `SkillTriggerDefinition`과 별도로 생성되는 `TriggeredSkill`을 제거한다.
- Trigger 결과는 원본 또는 명시된 기존 스킬의 `SkillExecutionData`를 사건값으로 보정한 뒤 `SkillExecution`으로 다시 실행한다.
- Executor와 Actor는 일반 시전과 Trigger 시전을 구분하지 않는다.
- Actor는 자신이 소유한 적중·만료 사건만 발행하며 Trigger 조건을 판단하지 않는다.
- 다른 스킬 실행, 사건값 피해, 별도 범위·대상·Visual, Actor 없는 passive, Zone 재생성, ChainLightning, 재귀 제한을 같은 사건 판정과 실행 스냅샷 경로로 통합한다.
- 적중, 피해 발생, 스킬 종료, 처치, N번째/마지막 탄환 적중, 방어막 사건, 다른 스킬 시전, 상태 종료 조건은 Trigger reaction으로 유지한다.
- 현재 outcome이 없는 진짜 Trigger 17개의 상태 효과를 기존 Skill/Choice/Passive 효과 경로로 복구한다.
- Trigger가 아닌 OnCast modifier 64개를 원본 시전의 Skill/Choice/Passive 효과로 복구한다.

## Constraints

- 새 C# 스크립트, Trigger 전용 Executor, Trigger 전용 Actor, 새 event bus, 새 family Definition을 만들지 않는다.
- `SkillTriggerDefinition.cs` 내용을 다른 파일에 그대로 복사하지 않는다.
- 현재 있는 `SkillNode`, `SkillExecutionData`, `SkillActionContext`, `SkillExecution`, `SkillTargeting`, family Executor, `SkillUseState` API를 공통 소유자로 사용한다.
- Trigger 조건을 `SkillExecution`, family Executor, Actor에 중복 구현하지 않는다.
- 피해·상태·보호막·Visual 실행 코드를 `SkillTrigger.cs`로 옮기지 않는다.
- Trigger 결과 때문에 숨은 `SingleSkillDefinition` 또는 `BuffSkillDefinition`을 만들지 않는다.
- 일반 시전 쿨다운·탄창은 Trigger 재실행에서 소비하지 않는다.
- 사건 당시 대상·중심·피해·상태·처형 여부는 지연 전에 확정한다.
- 현재보다 약한 재귀 제한을 사용하지 않는다.
- 현재 runtime 결과가 없는 81개는 진짜 Trigger 17개와 비-Trigger OnCast modifier 64개로 나눠 각각 승인된 정상 실행 경로로 복구한다.
- CSV 값, ID, asset 경로, 확률, 내부 쿨다운, 지연, 반복, 정렬 순서와 현재 대상 결과를 보존한다.
- Unity Play Mode gameplay 검증은 사용자 소유다.

## Role Owner

- 현재 문서와 구현: Code Builder
- Code Reviewer: 구현 완료 후 수행하며, 지적이 있으면 Code Reviewer 롤로 수정·재검증을 반복한다.

## Status

사용자가 `65 working / 17 incomplete / 76 non-Trigger` 분류, 17개 Trigger 효과 복구, 64개 일반 Choice/base/passive 효과 복구를 승인했다. Code Builder Phase 1~8 구현 완료. Code Reviewer 1~3차 수정 요청을 Code Builder가 반영했으며 재검토 대기.

## Core decision

```text
Trigger 조건       = 언제 실행할지
SkillNode           = 활성 Choice와 실행 보정값
SkillExecutionData  = 이번 한 번의 최종 실행값
SkillExecution      = 모든 실행의 공통 진입과 family 분배
Family Executor     = Actor 생성 또는 즉시 효과 적용
Actor               = 적중·Tick·만료 같은 수명 사건 발행
```

Trigger를 별도 스킬로 보지 않는다.

```text
현재
전투 사건
  -> SkillTriggerDefinition 조건
  -> TriggeredSkill Definition
  -> SkillExecution
  -> Executor

목표
전투 사건
  -> SkillTrigger 조건
  -> 기존 SkillExecutionData 복사/보정
  -> SkillExecution
  -> Executor
```

## Semantic Trigger rule

사용자가 정의한 Trigger는 다음 두 조건을 모두 만족해야 한다.

1. 어떤 스킬 또는 전투 사건이 먼저 발생한다.
2. 그 사건 뒤에 원래 시전 구성과 분리된 특수 효과가 새로 발생한다.

다음은 Trigger가 아니다.

- 스킬 시전 시 항상 함께 적용되는 기본 피해·상태·보호막.
- Choice가 원본의 위력, 계수, 지속시간, 쿨다운, 범위, 대상, 상태값을 바꾸는 것.
- 대상 상태나 체력에 따라 원본 피해량만 달라지는 것.
- passive가 상시 제공하는 조건부 보정.
- 같은 스킬의 `OnCast` 또는 `OnSkillCast`를 이용해 원래 시전 단계를 나눈 것.

즉 상태 조건이 있다는 이유만으로 Trigger가 아니다.

사건 조건과 효과 소유권은 분리한다.

- `SkillTrigger`: 적중, 피해 발생, 스킬 종료, 처치, N번째/마지막 탄환 적중, 방어막, 다른 스킬 시전, 상태 종료 조건만 판정한다.
- `Skill/Choice/Passive`: 조건이 참일 때 실행할 피해, 상태, 보호막, cooldown/reload, Zone, Visual 보정값을 소유한다.
- `requires_active_choice_id`가 있는 반응은 해당 Choice 선택 시에만 등록한다.
- Choice ID가 없는 `@effect` 또는 base 반응은 해당 Skill/Passive의 기본 반응으로 등록한다.
- 효과를 Skill/Choice/Passive에 소유시켜도 조건 충족 전에는 적용하지 않는다.

```text
상태를 계속 검사하여 원본 수치를 바꿈
  -> modifier / predicate

사건이 발생한 순간 새 피해·상태·명령을 실행
  -> Trigger reaction
```

### Ariel-B reference classification

| Choice | 실제 작성 | 의미 분류 |
|---|---|---|
| `ariel-b-trait-1` | `ShieldAmountMultiplier 1.3` | 원본 수치 수정. Trigger 아님 |
| `ariel-b-trait-2` | `DurationBonus 2` | 원본 수치 수정. Trigger 아님 |
| `ariel-b-trait-3` | `CooldownMultiplier 0.8` | 원본 수치 수정. Trigger 아님 |
| `ariel-b-trait-4` | `OnShieldExpire -> ApplyDamage` | 방어막 소멸 사건 뒤 별도 피해. Trigger |
| `ariel-b-trait-5` | 현재 `OnCast` + status modifier | 원본 성광 방패가 부여하는 추가 Buff. Trigger 아님 |
| `ariel-b-master-1` | shield/resistance modifier | 원본 수치·상태 수정. Trigger 아님 |
| `ariel-b-master-2` | `OnShieldAbsorb -> ApplyDamage` | 흡수 사건 뒤 반사 피해. Trigger |

`ariel-b-trait-5`는 현재 Trigger CSV와 Trigger-owned Node에 있지만 runtime outcome이 없다. 사용자 의미 기준으로는 Trigger에서 제거하고 `ariel-b`의 Choice-owned Buff 구성으로 통합해야 한다.

## Current inspected evidence

### Runtime path

- `SkillExecution.ExecuteSkill`은 concrete `SkillDefinition` 타입을 보고 Projectile, Line, Single, Zone, Buff Executor로 분배한다.
- 모든 family Executor는 최종 실행 입력으로 `SkillExecutionContext`와 `SkillExecutionData`를 받는다.
- `SkillExecution.TryExecuteTriggered`는 현재 `trigger.TriggeredSkill`로 snapshot을 만들고 공통 `ExecutePrepared`로 재진입한다.
- `SkillExecution`에는 현재 `MaxTriggeredExecutionDepth = 8`이 있다.
- `SkillTrigger`는 source-owned Trigger와 learned passive Trigger를 모두 검색한다.
- `InGameCombatManager`는 피해, 사망, 방어막, 상태 사건을 발행한다.
- `SkillExecution`은 시전 사건을 발행한다.
- Projectile, Line, Single, Zone Actor 또는 적중 처리부는 적중·배치·만료 사건을 발행한다.

### Authoring count

실제 monster Trigger CSV와 여섯 graph CSV 조인 결과:

- Trigger 행: 158개
- Trigger-owned Node: 606개
- runtime 결과가 있는 Trigger authoring row: 77개
- runtime 결과가 없는 Trigger authoring row: 81개
- 결과 77개 분포:
  - `ApplyDamage`: 27
  - `ApplyStatus`: 21
  - `ApplyShield`: 3
  - `ExecuteSkill`: 4
  - `RefundCooldown`: 14
  - `ReduceReload`: 6
  - `RecastZone`: 1
  - `ExtendStatusDuration`: 1
- learned passive가 소유한 runtime 결과: 40개

위 수치는 현재 기술적 authoring/runtime 분류다. 사용자 의미 기준으로 다시 나누면:

- 진짜 Trigger이며 현재 runtime 결과가 있는 행: 65개
- 진짜 Trigger 의도지만 현재 runtime outcome이 없는 행: 17개
- Trigger가 아닌 행: 76개
  - `OnCast`: 75개
    - runtime 결과 있음: 11개
    - runtime 결과 없음: 64개
- 같은 source skill의 `OnSkillCast` 후속 단계: 1개

따라서 기존 `77 active / 81 no-op`만으로 Trigger 여부를 판단하면 안 된다.

의미상 진짜 Trigger이며 현재 동작하는 65개 결과:

```text
ApplyDamage    24
ApplyStatus    16
ExecuteSkill    4
RefundCooldown 14
ReduceReload    6
RecastZone      1
```

현재 `ApplyShield` 3개와 `ExtendStatusDuration` 1개는 모두 OnCast 구성요소이므로 사용자 기준 Trigger에서 제외된다.

```text
기술적 active 77
  -> 진짜 Trigger 65
  -> 원본 실행 구성요소 12

기술적 no-outcome 81
  -> Trigger가 아닌 modifier 64
  -> 진짜 Trigger 의도지만 결과 누락 17
```

아래 걸림돌 분류는 서로 배타적인 총합이 아니다. 기술적 passive 40개 안에는 다른 스킬 실행 4개와 일부 추가 피해·명령이 포함된다.

## Target execution flow

### Initial cast

```text
SkillExecution
  -> source SkillDefinition과 활성 Choice로 SkillExecutionData 생성
  -> Trigger 조건 Node도 현재 실행 데이터에 포함
  -> SkillExecution.ExecutePrepared
  -> family Executor
  -> Actor 생성 또는 즉시 효과 적용
```

### Reaction

```text
SkillExecution / Actor / InGameCombatManager
  -> SkillActionContext 사건 발행
  -> SkillTrigger
     - event
     - source skill
     - Choice/status
     - attribute/runtime kind
     - event source scope
     - proc chance
     - internal cooldown
     - every-count
     - delay/repeat
     - recursion generation
  -> 조건 참
  -> 원본 또는 명시된 실행 대상 SkillUseState 선택
  -> SkillExecutionData 복사
  -> 사건 전용 damage/target/radius/duration/visual 보정
  -> SkillExecution 공통 Trigger 진입점
  -> 기존 family Executor
```

원래 Actor를 삭제하지 않는다. `OnExpire`만 Actor가 만료 사건을 먼저 발행한 뒤 정상 수명 경로로 제거된다.

## Data ownership consolidation

### Keep current authoring source

첫 구현에서는 Trigger CSV와 Trigger-owned graph CSV를 새 schema로 바꾸지 않는다. 이미 모든 사건 조건과 결과 Node를 가지고 있기 때문이다.

Generation이 현재 Trigger CSV와 graph Node를 읽는 것은 유지하되 결과를 `SkillTriggerDefinition`과 숨은 `SkillDefinition`으로 만들지 않는다.

대신:

1. 의미상 진짜 Trigger 65개와 결과 누락 Trigger 의도 17개만 Trigger reaction 후보로 분류한다.
2. Trigger 조건은 기존 `SkillNode` 조건 연산으로 생성한다.
3. 조건 Node는 source skill, passive 또는 해당 Choice의 기존 `Nodes`에 연결한다.
4. `SkillExecutionData.ApplyNodes`가 활성 Choice에 해당하는 Trigger 조건만 수집한다.
5. Trigger 결과 Node는 실행 시 효과를 직접 적용하지 않고 `SkillExecutionData` 보정값으로 정규화한다.
6. `SkillTrigger`는 `SkillExecutionData`에 수집된 조건을 읽는다.
7. Trigger가 아닌 76개는 `SkillTrigger` 입력에서 제외한다.

이 방식은 authoring schema 변경이 아니라 Generation 결과 소유권 통합이다.

### Non-Trigger rows

Trigger가 아닌 76개는 별도 반응 실행으로 만들지 않는다.

- runtime 결과가 있는 `OnCast` 11개는 원본 Skill/Choice/Passive 실행 Node로 통합한다.
- `vega-b-master1-second-slash`는 같은 스킬의 2차 시전 단계이므로 원본 스킬 follow-up 실행으로 통합한다.
- runtime 결과가 없는 `OnCast` modifier 64개는 owner별로 다음을 확인한다.
  - 이미 같은 일반 Choice/Passive Node가 있으면 Trigger 행을 삭제한다.
  - Trigger-owned Node에만 작성돼 있으면 원본 시전의 정상 Skill/Choice/Passive 효과로 옮겨 활성화한다.
  - 사용자가 2026-07-31 해당 64개의 설명문·Node 의도를 복구하는 gameplay 변경을 승인했다.

대표적인 runtime 결과가 있는 비-Trigger 12개:

```text
ariel-a-master-2
ariel-c@effect1
ariel-c-master-1
ariel-c-master-2
ariel-c-trait-5
ariel-e@effect1
ariel-e-trait-4
ariel-e-trait-5
ariel-g@effect2
eve-f@effect1
eve-h-trait-3
vega-b-master1-second-slash
```

`eve-h-trait-3`의 `OnCast` 행은 Choice 설명과 맞지 않는다. 같은 효과의 실제 Trigger 행인 `eve-h-freeze-expire-burst`가 `OnStatusExpire + freeze`로 별도 존재하므로 `OnCast` 중복은 Trigger 후보에서 제외한다.

### Trigger-intent rows with no current outcome

다음 17개는 사건 조건은 진짜 Trigger지만 `ApplyStatus` 같은 최종 outcome이 없어 현재 runtime no-op이다.

```text
eve-b-master-2
eve-b-master-2@effect2
rin-j@effect1
rin-j-trait-1
rin-j@effect2
rin-j-trait-2
rin-i-finishing-kill-action-speed
rin-i-finishing-kill-crit-damage-trait2
vega-i-area-vulnerability-base
vega-i-area-vulnerability-trait1
vega-i-area-vulnerability-trait1-trait2
vega-i-area-vulnerability-trait2
vega-i-area-vulnerability-trait3
vega-i-area-vulnerability-trait3-trait2
vega-j-survive-target-base
vega-j-survive-target-trait2
ariel-j-after-e-action-speed-trigger
```

이들은 Trigger 분류와 사건 조건을 유지한다. 사용자가 2026-07-31 효과 복구를 승인했으므로 작성 설명과 Node 값을 owner별로 대조한 뒤 기존 Skill/Choice/Passive 상태 효과 경로에 outcome을 연결한다.

### Do not relocate `SkillTriggerDefinition`

`SkillTriggerDefinition.cs`의 각 계약은 다음처럼 기존 공통 계약에 흡수한다.

| 현재 Trigger 계약 | 통합 대상 |
|---|---|
| 사건 종류 | `SkillActionContext`와 `SkillTrigger`가 공유하는 사건 enum |
| 대상 진영·선택·형태 | 기존 `SkillTargetingSpec` |
| 중심·EventTarget 고정 | 기존 `SkillExecutionContext`와 `SkillTargeting` |
| 확률·내부 쿨다운·횟수 | `SkillTrigger`의 기존 gate |
| 지연·반복 | `SkillTrigger`의 기존 coroutine |
| damage value source | 사건 context에서 `SkillExecutionData.RawDamageOverride`로 확정 |
| TriggeredSkill | 제거. 기존 `SkillUseState.Data`와 snapshot 사용 |
| Command enum | 제거. 기존 `SkillUseState`/status API 사용 |
| Trigger damage/status/shield/visual | 제거. snapshot과 기존 family Executor 사용 |

별도 class 전체를 다른 script에 옮기지 않는다. 중복 enum은 기존 targeting/runtime enum으로 치환하고, 실제 사건 enum과 source-scope처럼 필요한 최소 계약만 사건 판정 소유자와 함께 둔다.

## Common execution snapshot

현재 Executor들은 이미 `SkillExecutionData`만으로 실행값을 받는다. 차이는 `SkillExecution.ExecuteSkill`이 Definition concrete 타입으로 family를 고르는 부분이다.

통합 후:

1. 기존 `SkillDefinition.RuntimeKind`를 `SkillExecutionData`에 확정값으로 복사한다.
2. 일반 시전과 Trigger 시전 모두 snapshot의 기존 `SkillRuntimeKind`로 family Executor를 고른다.
3. Trigger 결과 Node는 snapshot의 damage, targeting, duration, visual, status, shield 값을 보정한다.
4. Executor는 입력 출처가 일반 시전인지 Trigger인지 알지 않는다.

기존 enum으로 분배한다.

```text
MagazineProjectile / CooldownProjectile -> ProjectileSkillExecutor
LineAttack                              -> LineSkillExecutor
SingleAttack / Mark / Execute           -> SingleSkillExecutor
AreaAttack                              -> 현재 Definition family에 따라 SingleSkillExecutor 또는 ZoneSkillExecutor
Field                                   -> ZoneSkillExecutor
Buff / Shield / Heal                    -> BuffSkillExecutor
Passive                                 -> 직접 실행하지 않음
```

새 Executor나 새 delivery enum을 만들지 않는다.

## Blocker conversion

### 1. 다른 스킬을 실행하는 passive 4개

| Trigger | Passive owner | 실행 대상 | 배율 |
|---|---|---|---:|
| `eve-g-auto-prism-ray` | `eve-g` | `eve-b` | 1.0 |
| `sein-g-auto-barrage-base` | `sein-g` | `sein-b` | 0.6 |
| `sein-g-auto-barrage-trait1` | `sein-g` | `sein-b` | 0.6 |
| `sein-g-auto-barrage-trait2` | `sein-g` | `sein-b` | 0.8 |

전환:

- 공통 Trigger 조건 Node에는 선택적인 실행 대상 skill ID만 둔다.
- 값이 없으면 사건 source skill을 재사용한다.
- 값이 있으면 `owner.SkillState.FindBySkillId(targetSkillId)`로 실제 learned runtime을 찾는다.
- 찾은 runtime의 기존 `SkillDefinition`과 `SkillExecutionData`를 사용한다.
- 배율만 snapshot에 적용하고 `SkillExecution`으로 재진입한다.
- passive 전용 실행 분기와 숨은 Definition을 만들지 않는다.

### 2. 사건 피해량을 사용하는 추가 피해 7개

| Trigger | 사건값 | 배율 |
|---|---|---:|
| `ariel-b-trait4-shield-expire` | `ShieldAppliedAmount` | 0.6 |
| `ariel-b-master2-shield-absorb-reflect` | `ShieldAbsorbedAmount` | 0.35 |
| `rin-f-followup` | `EventAppliedDamage` | 0.35 |
| `rin-f-followup-trait2` | `EventAppliedDamage` | 0.35 |
| `rin-f-followup-lightning-trait3` | `EventAppliedDamage` | 0.105 |
| `sein-a-master2-hit-explosion` | `EventAppliedDamage` | 0.5 |
| `ariel-d-master2-mark-expire-burst` | `TrackedIncomingDamage` | 0.2 |

전환:

- 현재 `SkillActionContext`가 보관하는 shield, applied damage, tracked damage 값을 그대로 사용한다.
- `SkillTrigger`는 조건 통과 직후, 지연 시작 전에 `사건값 × 배율`을 한 번 계산한다.
- 계산값을 기존 `SkillExecutionData.RawDamageOverride`에 기록한다.
- 지연 뒤에도 사건 당시 값이 유지된다.
- Executor는 사건값 종류를 알지 않고 확정된 raw damage만 사용한다.
- 7개별 피해 계산 함수를 만들지 않고 현재 `ResolveTriggeredRawDamage`의 source switch 하나를 유지·축소한다.

### 3. 원본과 다른 범위·대상·Visual을 가진 추가 피해 20개

7개 사건값 피해를 제외한 나머지 `ApplyDamage` 20개:

```text
eve-c-master-2
sein-d-master-2
sein-e-master-2
eve-h-trait-3
eve-h-freeze-expire-burst
rin-h-auto-shockwave-base-base
rin-h-auto-shockwave-lightning-base-base
rin-h-auto-shockwave-base-t2
rin-h-auto-shockwave-lightning-base-t2
rin-h-auto-shockwave-t1-base
rin-h-auto-shockwave-lightning-t1-base
rin-h-auto-shockwave-t1-t2
rin-h-auto-shockwave-lightning-t1-t2
sein-c-master-1
sein-c-master-2
vega-a-master2-kill-transfer
ariel-a-master1-last-shot-explosion
ariel-c-master-2
ariel-e-trait-4
rin-d-master1-kill-burst
```

이 20개는 기술적 `ApplyDamage` 집계다. 의미 재분류:

- 진짜 Trigger 추가 피해: 17개
- 원본 실행 구성요소: 3개
  - `ariel-c-master-2`: 시전의 두 번째 파동
  - `ariel-e-trait-4`: 신성 노출 대상에 대한 원본 위력 보정
  - `eve-h-trait-3`: 잘못 중복된 OnCast 행. 실제 빙결 해제 Trigger는 `eve-h-freeze-expire-burst`

전환:

- 진짜 Trigger 17개를 원본 스킬의 일반 시전 targeting으로 강제하지 않는다.
- Trigger-owned `ApplyDamage`, `SelectTargets`, `SetDuration`, `ShowVisual` Node를 하나의 실행 snapshot 보정으로 적용한다.
- 결과 snapshot의 runtime kind는 기존 `SingleAttack`을 사용한다.
- 대상은 기존 `SkillTargeting`, 실행은 기존 `SingleSkillExecutor`, Visual은 기존 `EffectManager`/`EffectVisualBuilder`를 사용한다.
- 별도 `SingleSkillDefinition`은 만들지 않는다.
- Circle, Single, Battlefield, EventTarget, PrimarySkillCenter, NearestEnemy 값은 snapshot에 확정한다.
- 현재 실제 동작이 authored Radius와 다른 `sein-c-master-1`, `sein-d-master-2`, `sein-e-master-2`는 이 구조 변경에서 범위 동작을 고치지 않는다. 현재 대상 결과를 먼저 보존한다.

핵심은 “Trigger가 피해를 실행”하는 것이 아니라 “Node가 Single 실행 snapshot을 완성하고 공통 Single Executor가 실행”하는 것이다.

### 4. Actor가 없는 passive runtime 결과 40개

실제 분포:

```text
ApplyDamage    13
ApplyShield     2
ApplyStatus     1
ExecuteSkill    4
RefundCooldown 14
ReduceReload    6
```

이 40개도 기술적 runtime 결과 수다.

- 의미상 진짜 Trigger: 37개
- 원본 passive 실행 구성요소: 3개
  - `ariel-g@effect2`
  - `eve-f@effect1`
  - `eve-h-trait-3` OnCast 중복

Actor가 없어도 Trigger 판단에는 문제가 없다. 현재도 `SkillTrigger`가 roster의 learned passive runtime을 순회한다.

전환:

- passive 학습 시 `PassiveSkillDefinition.Nodes`와 선택 Choice Node에 Trigger 조건이 연결된다.
- 사건 발생 시 `SkillTrigger`가 owner의 기존 passive `SkillUseState`에서 조건 snapshot을 만든다.
- 진짜 Trigger skill 결과 17개는 owner entry와 보정된 `SkillExecutionData`를 `SkillExecution`에 전달한다.
- Executor가 필요하면 Actor를 만든다.
- cooldown/reload 20개는 스킬 효과가 아니므로 Executor를 거치지 않고 기존 `SkillUseState.ReduceCooldownRemaining`과 `ReduceReloadRemaining`을 사용한다.
- 조건, 확률, cooldown/count, source scope는 두 결과 모두 같은 `SkillTrigger` gate를 사용한다.
- passive 전용 Actor, passive 전용 Executor, dummy SkillDefinition을 만들지 않는다.
- 비-Trigger 3개는 passive의 일반 실행/적용 Node로 통합한다.

40개 ID는 다음과 같다.

```text
ApplyDamage:
eve-h-freeze-expire-burst
eve-h-trait-3
rin-f-followup
rin-f-followup-lightning-trait3
rin-f-followup-trait2
rin-h-auto-shockwave-base-base
rin-h-auto-shockwave-base-t2
rin-h-auto-shockwave-lightning-base-base
rin-h-auto-shockwave-lightning-base-t2
rin-h-auto-shockwave-lightning-t1-base
rin-h-auto-shockwave-lightning-t1-t2
rin-h-auto-shockwave-t1-base
rin-h-auto-shockwave-t1-t2

ApplyShield:
ariel-g@effect2
eve-f@effect1

ApplyStatus:
vega-g-mark-on-hit-base

ExecuteSkill:
eve-g-auto-prism-ray
sein-g-auto-barrage-base
sein-g-auto-barrage-trait1
sein-g-auto-barrage-trait2

ReduceReload:
rin-g-howling-reload-rin-a-trait3
sein-g-auto-barrage-reload-trait3
sein-j-kill-sein-a-reload
sein-j-kill-sein-a-reload-trait2
sein-j-kill-sein-b-reload
sein-j-kill-sein-b-reload-trait2

RefundCooldown:
rin-i-execute-hit-rin-e-cooldown-trait3
rin-j-defense-down-kill-rin-d-cooldown-trait3
sein-j-kill-sein-b-cooldown
sein-j-kill-sein-b-cooldown-trait2
sein-j-kill-sein-c-cooldown
sein-j-kill-sein-c-cooldown-trait2
sein-j-kill-sein-d-cooldown
sein-j-kill-sein-d-cooldown-trait2
sein-j-kill-sein-e-cooldown
sein-j-kill-sein-e-cooldown-trait2
vega-i-area-cooldown-base
vega-j-cooldown-base
vega-j-cooldown-trait1
vega-j-vega-d-cooldown-trait3
```

### 5. 조건부 Zone 재생성 1개

현재:

```text
eve-e-master-1
OnExpire
delay 0.5
duration 3
radius multiplier 0.6
inherit snapshot true
max generation 1
```

전환:

- `RecastZone` 전용 command 실행 분기를 제거한다.
- 만료 사건 당시 원본 `eve-e` snapshot을 복사한다.
- duration, radius multiplier, event center, generation만 보정한다.
- 기존 `SkillExecution`으로 재진입하고 `ZoneSkillExecutor`가 실행한다.
- `recastGeneration + 1`과 `max generation 1`은 기존 `SkillExecutionContext`에서 유지한다.
- Zone 재생성만을 위한 Definition, Actor, Executor를 만들지 않는다.

### 6. ChainLightning 대상 제외·지연·배율

현재 CSV:

```text
source skill             ChainLightning
damage multiplier        0.5
delay                    0.5
search radius            7
exclude primary target   true
```

현재 Generation은 `ChainLightning__chain` 숨은 `SingleSkillDefinition`과 `ChainLightning__chain_on_hit` Trigger를 만든다.

전환:

- 숨은 Definition과 숨은 Trigger 생성 코드를 제거한다.
- `DamageThenDelayedChain`의 기존 CSV 값은 source skill의 Trigger 조건 Node로 Generation한다.
- OnHit context의 event target을 기준으로 기존 `NearestOtherFromEventTarget` targeting을 snapshot에 적용한다.
- radius 7, multiplier 0.5, delay 0.5를 같은 공통 reaction snapshot에 적용한다.
- 원본 `ChainLightning`의 attribute, 계수, Visual을 재사용한다.
- primary 대상은 기존 `SkillTargeting`에서 제외한다.
- 후속타 lifecycle은 현재처럼 억제하여 자기 자신이 다시 chain을 발행하지 않게 한다.
- Chain 전용 hidden Definition과 Builder 특례를 만들지 않는다.

### 7. 재귀 무한 실행 방지

재귀 제한은 Actor나 각 Executor에 넣지 않는다.

공통 `SkillExecution` Trigger 진입점 하나에서 다음을 적용한다.

1. 현재 `MaxTriggeredExecutionDepth = 8` 유지.
2. `SkillExecutionContext.RecastGeneration` 유지.
3. Node별 최대 generation 기본값 1.
4. 같은 reaction이 자기 lifecycle을 다시 발행하지 않아야 하는 경우 lifecycle 발행 억제.
5. 다른 Trigger 연쇄가 필요한 기존 `ExecuteSkill`은 lifecycle을 유지하되 depth 8을 넘지 않음.
6. 지연 실행도 발생 당시 generation을 보존함.

직접 자기 호출:

```text
OnCast A
  -> A 재실행
  -> 같은 OnCast A
```

은 같은 reaction의 generation 제한에서 차단한다. depth 8은 서로 다른 reaction이 순환하는 최종 안전장치다.

## Non-skill commands

다음 결과는 스킬 재시전으로 위장하지 않는다.

- `RefundCooldown`
- `ReduceReload`
- `ExtendStatusDuration`

공통화 지점은 실행 방식이 아니라 조건 판정이다.

```text
SkillTrigger 공통 gate
  ├─ skill reaction -> SkillExecution -> Executor
  └─ state reaction -> 기존 SkillUseState/Status API
```

이 분기는 유지해야 책임이 섞이지 않는다. 가짜 Buff 또는 Single snapshot을 만들어 cooldown/reload를 처리하지 않는다.

## Existing script integration surface

### `Combat/Skills/Definitions/SkillDefinition.cs`

- `SkillTriggerDefinition[] SkillTriggers` 제거.
- 기존 `SkillNode[] Nodes`가 Trigger 조건 Node도 소유.
- 기존 `SkillRuntimeKind`를 공통 실행 분배 기준으로 유지.

### `Combat/Skills/Definitions/Nodes/SkillNodeConditions.cs`

- 현재 조건 operation 체계 안에 Trigger 사건 조건을 표현.
- `SkillTriggerDefinition` 전체 필드를 복제하지 않음.
- targeting, runtime kind, status 조건은 기존 강타입 계약 참조.

### `Combat/Skills/Runtime/SkillExecutionData.cs`

- 활성 Choice에서 수집된 Trigger 조건을 보관.
- 기존 `RuntimeKind`, targeting, raw damage override, status, shield, visual 실행값을 한 snapshot으로 확정.
- Trigger 결과 Node는 snapshot을 보정할 뿐 효과를 실행하지 않음.

### `Combat/Skills/Execution/SkillActionContext.cs`

- 사건 당시 source, target, center, damage, shield, status, execute, generation snapshot 유지.
- Trigger Definition의 효과·targeting 책임을 받지 않음.

### `Combat/Skills/Reactions/SkillTrigger.cs`

- 유지:
  - 사건 수신
  - source/passive 조건 검색
  - 조건·확률·cooldown/count
  - delay/repeat
  - 사건값 확정
  - skill reaction과 state reaction 분기
- 제거:
  - `SkillTriggerDefinition` 의존
  - 숨은 TriggeredSkill 의존
  - RecastZone 특례
  - Trigger 전용 damage/status/shield/visual 실행

### `Combat/Skills/Execution/SkillExecution.cs`

- 기존 Trigger 공통 진입점 유지.
- 입력을 `SkillTriggerDefinition`이 아니라 실행 대상 runtime, reaction snapshot, event context로 축소.
- snapshot의 기존 `SkillRuntimeKind`로 family Executor 분배.
- 일반 cooldown/magazine 소비 없이 실행.
- depth/generation/lifecycle 정책의 단일 소유자.

### Family Executors and Actors

- 기존 `SkillExecutionContext`와 `SkillExecutionData`만 받음.
- Trigger 조건을 읽지 않음.
- Trigger별 분기 추가 금지.
- Actor는 자신의 hit/tick/expire 사건만 발행.

### `Loading/Generation`

- Trigger CSV와 Trigger-owned Node를 읽는 현재 Parsing/Validation은 첫 구현에서 유지.
- `BuildSkillTriggers`, `BuildTriggerOutcome`, `BuildEnemyChainTrigger`의 별도 Definition 생성 책임 제거.
- 기존 Skill/Choice/Passive Node Generation에 Trigger 조건과 reaction snapshot 보정을 연결.
- runtime catalog에 Trigger 배열 또는 hidden Definition을 등록하지 않음.

## Migration order

### Phase 1: Baseline

- 158 Trigger, 606 Node, 기술적 결과 77, no-outcome 81을 테스트로 고정.
- 의미 분류 `working Trigger 65 / incomplete Trigger 17 / non-Trigger 76`을 owner ID 단위로 고정.
- 기술적 4/7/20/40/1/Chain 집계와 의미상 4/7/17/37/1/Chain 집계를 함께 기록.
- Trigger별 event, target, damage, visual, command 결과를 기록.
- 완료 증거:
  - `SkillCatalogRuntimeTests.TriggerSemanticClassificationBaselineIsStable`이 `65/17/76`, OnCast 75, same-source follow-up 1을 고정한다.
  - `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`: 오류 0, 기존 assembly version 경고 2.
  - Unity EditMode 집중 테스트 1/1 통과, catalog 5/8/8.

### Phase 2: Snapshot-driven executor routing

- 일반 스킬 동작을 바꾸지 않고 `SkillExecutionData`의 기존 runtime kind로 같은 family Executor가 선택되는지 검증.
- 모든 일반 skill family 회귀를 먼저 통과.
- 완료 증거:
  - `SkillExecution`의 preparation과 Executor 분배가 `SkillRuntimeKind`를 공통 기준으로 사용한다.
  - 기존 concrete Definition은 family 값 검증에만 사용하며 새 Executor나 Definition을 추가하지 않았다.
  - `RuntimeKindsMatchExistingExecutorFamilies`가 Monster, Enemy, Trigger 결과 Definition 전체의 kind/family 대응을 검증한다.
  - solution build 오류 0, Unity EditMode 13/13 통과.

### Phase 3: Existing-skill reactions

- `ExecuteSkill` 4개를 hidden Definition 없이 실제 target runtime snapshot으로 실행.
- 같은 skill 재실행 Node를 공통 경로에 연결.
- 완료 증거:
  - `SkillExecution.TryExecuteReaction`은 `SkillTriggerDefinition` 전체를 받지 않고 기존 `SkillUseState`, `SkillDefinition`, snapshot runtime과 실행 보정값만 받는다.
  - `SkillTrigger.TryExecuteOutcome`은 `UsesExistingSkillRuntime` 결과에서 실제 learned target runtime을 찾아 그 runtime과 Definition을 공통 실행 경로에 전달한다.
  - 임시 direct-delivery 결과는 기존 source runtime으로 snapshot을 만들고 현재 generated Definition을 실행하므로 Phase 5 이전 동작을 보존한다.
  - 재귀 깊이 8, lifecycle 발행, event target 고정, raw damage override, damage multiplier 계약을 유지한다.
  - solution build 오류 0, Unity 강제 script compile 뒤 EditMode 14/14 통과.

### Phase 4: Non-Trigger extraction

- `OnCast` 75개를 Trigger 반응 경로에서 제외.
- 같은 source의 `vega-b-master1-second-slash`를 원본 follow-up 실행으로 이전.
- runtime 결과가 있는 비-Trigger 12개의 현재 결과를 일반 Skill/Choice/Passive Node에서 보존.
- no-outcome modifier 64개는 중복 삭제와 원본 시전의 Skill/Choice/Passive 효과 복구를 owner별로 수행.
- 완료 증거:
  - final runtime Trigger 배열은 `OnCast` 75개와 same-source `OnSkillCast` 1개를 모두 제외해 82개(`65 working + 17 incomplete`)만 보관한다.
  - 중복 `eve-h-trait-3` OnCast 행은 등록하지 않고 실제 `OnStatusExpire` reaction만 유지한다.
  - 일반 cast/passive payload 74개는 기존 source Skill, 요구 Choice 또는 Passive의 `SkillNode`에 `SkillCastEffectOp`로 연결된다.
  - `ariel-e-trait-4`는 별도 피해 payload가 아니라 기존 `ConditionalDamageMultiplier` Choice Node(`holy-exposure`, 최소 1, ×1.5)로 교정된다.
  - `SkillExecutionData`가 활성 Choice의 일반 효과를 수집하고, active cast 뒤 또는 passive 전투 시작 시 기존 Single/Buff/status API로 실행한다.
  - `StatusModifier` 64개는 `PassiveBuff` runtime data로 생성되어 대상 상태식, 최소 stack, source skill, 체력 비율, source status 조건을 보존한다.
  - `ariel-b-trait-5`는 Trigger 배열에 없고 Choice effect로 5초간 신성 피해 +12% 상태를 소유한다.
  - solution build 오류 0, Unity 강제 script compile 뒤 EditMode 14/14 통과.

### Phase 5: Direct delivery reactions

- 사건값 피해 7개를 raw damage override로 이전.
- 의미상 Trigger delivery 피해 17개를 Single snapshot 보정으로 이전.
- 의미상 Trigger status 16개를 Buff snapshot 보정으로 이전.
- Trigger-intent no-outcome 17개는 사건 조건을 유지하고 기존 상태 효과 경로로 outcome 연결.
- 완료 증거:
  - hidden direct-delivery Definition 40개를 제거하고 `SkillCastEffect` 57개(피해 24, 상태 33)로 통합했다.
  - 사건값 피해 7개는 지연 전에 복사된 `TriggerExecutionContext` 값으로 raw damage를 결정한다.
  - 미완성 17개 `StatusModifier`는 기존 상태 적용 API로 연결되며 원래 Trigger 조건·지연·반복 판정은 `SkillTrigger`에 남는다.
  - runtime Trigger 82개 모두 outcome을 가지며 남은 `TriggeredSkill` 4개는 실제 learned cross-skill 재사용뿐이다.
  - solution build 오류 0, Unity EditMode 14/14 통과.

### Phase 6: Actor-less and state reactions

- final catalog의 passive source reaction 48개를 learned passive runtime 검색 경로로 고정.
- 실제 breakdown은 공통 effect 24, learned cross-skill 재사용 4, 기존 state command 20이다.
- 설계 단계의 `37`은 구형 hidden-Definition 결과 분류였으며 final passive source ownership 수가 아니므로 이 code-derived 48개로 교정.
- passive 비-Trigger 결과 3개는 일반 passive 적용 Node로 이전.
- cooldown/reload/status-duration는 기존 state API에 연결.
- 완료 증거:
  - `SkillTrigger.ExecutePassiveOwnerTriggers`가 Actor가 아닌 roster의 learned passive runtime을 검색한다.
  - passive source reaction 48개 전부 outcome 보유; effect 24, skill reuse 4, command 20.
  - 전체 state command는 cooldown refund 14, reload reduction 6, Zone recast 1이다.
  - 새 Actor/Executor/state API 없이 기존 공통 경로만 사용한다.
  - solution build 오류 0, Unity EditMode 15/15 통과.

### Phase 7: Recast and Chain

- `eve-e-master-1`을 원본 Zone snapshot 재실행으로 이전.
- ChainLightning hidden Definition을 제거하고 원본 snapshot 보정으로 이전.
- depth, generation, lifecycle 제한 검증.
- 완료 증거:
  - `eve-e-master-1`은 기존 source runtime snapshot을 상속하고 0.5초 뒤 반경 ×0.6, 지속 3초, 최대 generation 1로 Zone Executor에 재진입한다.
  - `RecastZone.delay_seconds`는 이전 코드에서 command에 저장만 되고 소비되지 않았으나, 이제 Trigger 예약 지연으로 통합된다.
  - ChainLightning은 `__chain` Definition 없이 원본 Damage 참조와 source runtime snapshot을 사용한다.
  - Chain 보정은 0.5초 지연, 반경 7, `NearestOtherFromEventTarget`, 피해 ×0.5, lifecycle 미발행이다.
  - solution build 오류 0, Unity EditMode 15/15 통과.

### Phase 8: Delete obsolete contracts

- `SkillTriggerDefinition.cs`와 `.meta`를 삭제했다.
- `SkillDefinition.SkillTriggers`와 Monster/Enemy Trigger 배열을 삭제했다.
- runtime reaction은 기존 Skill/Choice/Passive `Nodes`의 `SkillReactionOp`로 소유한다.
- `SkillExecutionData`는 활성 Node에서 `SkillReaction`을 수집한다.
- `SkillTrigger`는 source skill과 learned passive의 실행 snapshot에서 reaction을 읽고 사건·조건만 판단한다.
- direct delivery는 `SkillCastEffect`, cross-skill은 `TargetSkillId`, 상태 명령은 `SkillReactionCommand`로 기존 실행/API에 위임한다.
- hidden Trigger Definition Generation과 Chain hidden Definition은 없다.
- 삭제된 `SkillTriggerDefinition` 타입 참조는 C# 정적 검색 결과 0이다.
- solution build 오류 0, Unity Console 오류 0, EditMode 15/15 통과.

### Code Reviewer correction 1

- 중복 `ariel-a-master-2` OnCast payload를 일반 효과 등록에서 제외하고 실제 `ariel-a-master2-holy-exposure-on-hit` reaction만 유지했다.
- `vega-b-master1-second-slash`는 기존 `ExecuteSkill` Node로 `vega-b` 0.45배 재사용을 작성하고, 원본 준비 방향과 침묵 payload를 공통 `SkillExecution` 재진입에 전달한다.
- `ariel-c-master-2` 1초 지연 파동은 원본 실행의 `PreparedCenters`를 재사용한다.
- 전투 중 Choice 반영 뒤 `RefreshPassiveEffects`를 호출해 새로 활성화된 passive 일반 효과를 현재 roster에 다시 적용한다.
- 최종 일반 cast/passive payload는 73개다. 76개 non-Trigger 행 중 `eve-h-trait-3`과 `ariel-a-master-2`는 실제 event reaction과 중복되어 제외되고, `ariel-e-trait-4`는 기존 조건부 위력 Choice Node로 통합된다.
- solution build 오류 0, Unity Console 오류 0, EditMode 15/15 통과.

### Code Reviewer correction 2

- Vega B 후속타 snapshot에 같은 일반 follow-up effect가 다시 포함되어 비동기 재귀할 수 있는 경로를 차단했다.
- 공통 `TryExecuteReaction`/`ExecutePrepared`에 기존 cast effect 실행 여부를 전달하고, Vega B의 한 번짜리 재사용만 `false`로 호출한다.
- 기존 cross-skill 재사용과 일반 시전은 기본값 `true`를 유지한다.
- solution build 오류 0, Unity Console 오류 0, EditMode 15/15 통과.

### Code Reviewer correction 3

- 반응 배율은 기존 Choice 누적 modifier와 별개로 최종 `DamageMultiplier`에 곱하도록 `SkillExecutionData.ScaleDamageMultiplier`를 추가했다.
- `TryExecuteReaction`과 `TryExecuteReactionEffect`만 새 곱셈 경로를 사용하며, 일반 `TryExecuteSkill`과 상태 수 기반 보정은 기존 `ApplyDynamicDamageMultiplier` 누적 의미를 유지한다.
- Vega B 특성 1의 1.25배와 두 번째 참격 0.45배가 `0.5625`가 되는 회귀 테스트를 추가했다.
- solution build 오류 0, Unity EditMode 16/16 통과. Unity console에는 컴파일 오류가 없고 Test Runner가 결과 파일을 저장하는 Exception 로그 1건이 남는다.

## Risk boundaries

- `SkillRuntimeKind` 기반 분배가 모든 기존 concrete family 매핑과 정확히 같아야 한다.
- final passive source reaction 48개는 Actor 부재 때문에 누락되기 쉬우므로 roster passive 검색을 유지한다.
- Trigger snapshot이 일반 cast cooldown/magazine을 소비하면 안 된다.
- delayed reaction은 live 객체 값을 다시 읽지 않는다.
- event target이 죽었을 때 현재 LockToEventTarget 결과를 보존한다.
- direct delivery 20개의 현재 대상 집합과 authored Radius 차이를 이번 구조 변경에서 수정하지 않는다.
- lifecycle 억제를 과도하게 적용하면 합법적인 OnHit/OnKill 연쇄가 사라진다.
- lifecycle을 모두 허용하면 OnCast와 Chain이 재귀할 수 있다.
- no-outcome 81개를 하나로 취급하면 안 된다. modifier 64개와 Trigger-intent 17개를 분리한다.
- modifier 64개는 OnCast Trigger를 제거하고 원본 시전 효과로만 활성화한다.
- incomplete Trigger 17개는 원래 사건 조건을 통과한 경우에만 활성화한다.

## Acceptance criteria

- `SkillTriggerDefinition` 참조 검색 결과가 0이다.
- runtime에 hidden Trigger `SkillDefinition` 생성이 없다.
- `SkillDefinition`, Monster, Enemy에 Trigger 배열이 없다.
- Trigger 조건 판정은 `SkillTrigger.cs` 한 곳이다.
- family Executor와 Actor에 Trigger별 조건 분기가 없다.
- 158개 authoring 행이 `working Trigger 65 / incomplete Trigger 17 / non-Trigger 76`으로 고정된다.
- 4개 cross-skill passive가 실제 대상 skill runtime을 사용한다.
- 7개 사건값 피해가 지연 전 사건값 snapshot을 사용한다.
- 의미상 Trigger direct damage 17개가 기존 대상·횟수·Visual 결과를 유지한다.
- passive source reaction 48개(effect 24, skill reuse 4, command 20)가 Actor 없이 실행된다.
- 비-Trigger 12개 runtime 결과가 일반 Skill/Choice/Passive 실행에서 보존된다.
- `eve-e-master-1`이 0.5초 뒤 반경 0.6, 지속 3초, 최대 1세대로 재실행된다.
- ChainLightning이 0.5초 뒤 반경 7 안의 primary 제외 대상에게 0.5배로 실행된다.
- Trigger 재실행이 일반 cooldown·magazine을 소비하지 않는다.
- direct self recursion, cross-trigger cycle, Zone recast가 각 제한에서 종료된다.
- incomplete Trigger 17개가 원래 사건 조건에서 작성된 상태 효과를 실행한다.
- modifier 64개가 원본 Skill/Choice/Passive 시전 효과로 실행되며 Trigger 반응 경로에는 등록되지 않는다.
- `ariel-b-trait-5`는 방어막 부여 시 대상에게 5초간 신성 피해 +12%를 적용하고, 방어막 만료 Trigger로 분류되지 않는다.
- Runtime/Editor build error 0.
- 기존 Loading semantic validation, catalog build, lookup rebuild는 각각 한 번이다.
- 관련 Unity EditMode 테스트가 통과한다.
- 최종 gameplay 검증은 사용자가 Play Mode에서 수행한다.

## Verification expected from Code Builder

- 변경 전/후 owner ID별 기술적 결과 77개와 의미상 Trigger 65개 비교.
- `65/17/76` 의미 분류 고정 테스트.
- 기술적 `4/7/20/40/1/Chain` 이력과 final passive source `48(24/4/20)` 집중 EditMode 테스트.
- 일반 Projectile, Line, Single, Zone, Buff 회귀 테스트.
- static search:
  - `SkillTriggerDefinition`
  - `TriggeredSkill`
  - hidden `__chain`
  - `BuildTriggerOutcome`
  - `BuildEnemyChainTrigger`
  - family Executor의 Trigger 조건
- Runtime과 Editor C# build.
- Unity compile/console 확인.
- CSV semantic validation과 runtime catalog count 확인.
- 사용자 Play Mode 확인 항목을 결과에 명시.

## Next Actions

- Code Reviewer 2차 수정 결과를 별도 Git commit으로 기록한다.
- Code Reviewer 롤로 다시 전환해 Phase 1~8과 수정 결과를 검증한다.
- Reviewer가 수정을 요구하면 Code Reviewer 롤로 수정·재검증하고 통과할 때까지 반복한다.
- 기존 `SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`는 현재 구현의 근거 기록으로 보존하며 삭제하지 않는다.

## Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Trigger/SkillTriggerDefinition.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/SkillDefinition.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNode.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNodeConditions.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Runtime/SkillExecutionData.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillActionContext.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Reactions/SkillTrigger.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Nodes.cs`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/`
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/single_attack/skills_single_attack.csv`
- PowerShell CSV join: Trigger 158, Trigger Node 606, active 77, passive active 40, dynamic damage 7, remaining direct damage 20.
- Semantic audit: working Trigger 65, incomplete Trigger 17, non-Trigger OnCast 75, non-Trigger same-source OnSkillCast 1.
- Non-Trigger runtime result audit: OnCast outcome 11 plus `vega-b-master1-second-slash`.
- Non-Trigger no-outcome audit: OnCast modifier 64.
- Ariel-B audit: trait 1~3 are Choice modifiers, trait 4 is OnShieldExpire damage, trait 5 is an OnCast status modifier with no current runtime outcome, master 2 is OnShieldAbsorb damage.
- Phase 1 test: `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`
- Phase 1 verification: solution build error 0; Unity EditMode `TriggerSemanticClassificationBaselineIsStable` 1/1.
- Phase 2 verification: solution build error 0; Unity EditMode 13/13.
- Phase 2 family exception: authored `Slash`와 `FireDragonSlash`는 `SkillRuntimeKind.AreaAttack`이지만 CSV `DamageArea` Generation 결과가 `SingleSkillDefinition`이다. 따라서 현재 `AreaAttack`은 기존 Definition family를 검증해 Single 또는 Zone Executor를 선택한다.
- Phase 3 verification: `TryExecuteTriggered` 참조 0; `TryExecuteReaction`이 Trigger Definition 없이 기존 runtime/Definition을 받는다; solution build error 0; Unity EditMode 14/14.
- Phase 4 verification: runtime Trigger 82, leaked OnCast/same-source Trigger 0, 일반 Skill/Choice/Passive cast effect 74 + 기존 조건부 위력 Choice 1, `eve-h-trait-3` 중복 0; solution build error 0; Unity EditMode 14/14.
- Phase 5 verification: runtime Trigger effect 57(피해 24, 상태 33), existing-skill reuse 4, command 21, outcome 누락 0; solution build error 0; Unity EditMode 14/14.
- Phase 6 verification: passive source reaction 48(effect 24, skill reuse 4, command 20), cooldown refund 14, reload reduction 6; solution build error 0; Unity EditMode 15/15.
- Phase 7 verification: `__chain` SkillDefinition 0, Chain 원본 Damage 참조/지연 0.5/배율 0.5/반경 7/primary 제외, Zone 지연 0.5/반경 0.6/지속 3/generation 1; solution build error 0; Unity EditMode 15/15.
- Phase 8 verification: `SkillTriggerDefinition` C# 참조 0; Skill/Monster/Enemy runtime Trigger 배열 제거; reaction은 기존 Skill/Choice/Passive Node에서 수집; hidden Trigger Definition 0; solution build error 0; Unity Console error 0; Unity EditMode 15/15.
- Reviewer correction 1 verification: `ariel-a-master-2` 일반 payload 0/실제 OnOutgoingDamage reaction 1; Vega B follow-up target `vega-b`/배율 0.45/지연 0.4/침묵/원본 방향 재사용; Ariel C 지연 파동 원본 center 재사용; 일반 payload 73; solution build error 0; Unity Console error 0; Unity EditMode 15/15.
- Reviewer correction 2 verification: Vega B follow-up calls common reaction execution with `executeCastEffects=false`; other call sites retain the default `true`; solution build error 0; Unity Console error 0; Unity EditMode 15/15.
- Reviewer correction 3 verification: reaction paths call `ScaleDamageMultiplier`, normal skill path still calls additive `ApplyDynamicDamageMultiplier`; `1.25 × 0.45 = 0.5625` regression test; solution build error 0; Unity EditMode 16/16.

## History

- 2026-07-31: User defined Trigger skill as reuse of an existing skill with adjusted values and required `SkillTrigger` to remain the central event-condition judge.
- 2026-07-31: User rejected a separate Trigger skill Definition and requested logic consolidation instead of copying the old class into another script.
- 2026-07-31: Designer inspected current runtime, Generation, Trigger CSV, graph Node, passive outcomes, Zone recast, ChainLightning, and recursion guards.
- 2026-07-31: Designer recorded the no-new-script common snapshot and existing-Executor migration in this handoff.
- 2026-07-31: User clarified that ordinary skill modifiers and cast-time payloads are not Trigger reactions; Ariel-B trait 4 is a Trigger while traits 1~3 and 5 are not.
- 2026-07-31: Designer re-audited all 158 authoring rows and corrected the design to 65 working Triggers, 17 incomplete Trigger-intent rows, and 76 non-Trigger rows.
- 2026-07-31: User approved preserving event conditions, restoring the 17 incomplete Trigger effects through existing effect execution, and restoring the 64 non-Trigger modifiers through normal Skill/Choice/Passive execution.
- 2026-07-31: User assigned Code Builder, required one Git commit per Phase, and approved repeated Code Reviewer correction passes until approval.
- 2026-07-31: Code Builder completed Phase 1 semantic baseline test and non-Play-Mode verification.
- 2026-07-31: Code Builder completed Phase 2 runtime-kind executor routing and full EditMode verification.
- 2026-07-31: Code Builder completed Phase 3 existing-skill runtime reuse and removed the common execution entry point's dependency on `SkillTriggerDefinition`.
- 2026-07-31: Code Builder completed Phase 4 by removing 76 semantic non-Triggers from runtime reaction registration and restoring 75 non-duplicate normal effects through existing Skill/Choice/Passive Nodes.
- 2026-07-31: Code Builder completed Phase 5 by replacing 40 hidden direct-delivery Definitions and restoring 17 incomplete event outcomes through the common cast-effect path.
- 2026-07-31: Code Builder completed Phase 6 by fixing the final catalog's 48 passive source reactions and existing state commands as the Actor-less common-path baseline.
- 2026-07-31: Code Builder completed Phase 7 by deleting the Chain hidden Definition and wiring Zone node delay into the common Trigger scheduler.
- 2026-07-31: Code Builder completed Phase 8 by deleting the obsolete Trigger Definition and owner arrays and attaching the final reaction contract to existing Skill/Choice/Passive Nodes.
- 2026-07-31: Code Reviewer requested corrections for duplicate hit status, missing Vega B follow-up damage, delayed center loss, and passive Choice refresh.
- 2026-07-31: Code Builder applied Reviewer correction 1 through existing Node, snapshot, SkillExecution, and passive refresh paths.
- 2026-07-31: Code Reviewer found the delayed self-follow-up recursion path; Code Builder disabled nested cast effects only for that reuse call.
- 2026-07-31: Code Reviewer found reaction scaling was additive against existing Choice damage modifiers; Code Builder separated reaction multiplication from normal additive modifier accumulation and passed the 16-test EditMode suite.
