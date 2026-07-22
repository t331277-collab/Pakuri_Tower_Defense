# SkillExecutionData 삭제 작업

## 목표

`Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionData.cs`와 `.meta`를 삭제한다.

모든 스킬이 하나의 공용 Snapshot을 만드는 구조를 없애고 다음 책임 구조를 적용한다.

## 책임 구조

### SkillDefinition

- CSV 로딩 단계에서 완성된 스킬 기본 정의를 보관한다.
- 실행 중 변경되는 값을 보관하지 않는다.

### UnitSkills

- 학습한 액티브·패시브 스킬 ID를 보관한다.
- 선택한 강화·마스터 ID를 보관한다.
- 실행 수치와 전투 상태를 계산하지 않는다.

### SkillExecution

- 스킬 실행 가능 여부를 판단한다.
- 조준과 실행 문맥을 준비한다.
- 시전을 확정한다.
- 스킬 종류에 맞는 Executor를 호출한다.
- 공용 강화 Snapshot을 만들지 않는다.

### 각 SkillExecutor

- `SkillDefinition`, `SkillNode`, `UnitSkills`에서 자기 스킬 종류에 필요한 정보만 읽는다.
- 선택한 강화와 마스터 효과를 자기 실행 값에 직접 반영한다.
- 즉시 실행 효과를 처리한다.
- Actor에 확정된 값만 전달한다.

### 각 SkillActor

- 이동과 충돌을 처리한다.
- 실제 적중을 판정한다.
- Executor가 전달한 확정 피해와 상태 효과를 적용한다.
- 자신의 수명을 관리한다.
- Choice나 SkillNode를 다시 조합하지 않는다.

### 공용 전투 코드

- `DamageCalculator`는 확정된 피해 계산 입력만 받는다.
- `StatusRules`는 확정된 상태 적용 입력만 받는다.
- `SkillTargeting`은 대상과 범위 조건만 처리한다.
- `EffectVisualBuilder`는 확정된 프리팹과 크기만 처리한다.

## 삭제 대상

- `SkillExecutionData`
- `SkillExecutionState.CreateExecutionData`
- `SkillExecutionState.BuildExecutionData`
- `SkillExecutionState.ApplyChoices`
- `SkillExecutionState.ApplyPassiveBaseModifiers`
- `SkillExecutionState.ResolveActiveChoices`
- `SkillExecutionState.ResolvePassiveChoices`
- 공용 Snapshot을 복사하거나 보정하는 메소드

## 이전 기준

| 기존 데이터 | 이전 위치 |
|---|---|
| 투사체 수, 관통, 분기, 연속 발사 | `ProjectileSkillExecutor` |
| 선 폭, 지속시간, Tick, 밀치기 | `LineSkillExecutor` |
| 범위, 배치 수, 지속시간, Tick | `ZoneSkillExecutor` |
| 단일 공격, 연쇄, 충전, 처형 | `SingleSkillExecutor` |
| 보호막, 회복, 상태 효과 | `BuffSkillExecutor` |
| 패시브 Choice와 지속 효과 | `PassiveSkill` |
| Trigger 확률과 추가 행동 | `SkillTrigger` |
| 조건부 피해와 치명타 | 해당 Executor와 `DamageCalculator` |
| 상태 지속시간과 최대 중첩 | 해당 Executor와 `StatusRules` |
| 효과 프리팹과 표시 크기 | 해당 Executor와 `EffectVisualBuilder` |

## 금지 사항

- `SkillExecutionData`를 다른 이름으로 다시 만들지 않는다.
- 모든 스킬 종류의 값을 담는 새 공용 Snapshot을 만들지 않는다.
- 새 fallback 함수를 만들지 않는다.
- 삼항 연산자를 새로 만들지 않는다.
- 필요하지 않은 `sealed`, `internal`을 추가하지 않는다.
- `BLACKBOARD.md`와 `boards/**`를 수정하지 않는다.

## 완료 기준

- `SkillExecutionData.cs`와 `.meta`가 없다.
- `SkillExecutionData` 참조가 0개다.
- `CreateExecutionData`, `BuildExecutionData`, `ResolveActiveChoices`, `ResolvePassiveChoices`가 없다.
- Executor가 자기 스킬 종류의 강화 값을 완성한다.
- Actor는 확정된 실행 값만 받는다.
- C# 빌드 오류가 없다.
- Unity Editor 콘솔 컴파일 오류가 없다.
- Play Mode 검증은 사용자가 수행한다.

## 구현 진행 상태

- 완료: `DamageCalculator`가 확정된 기본 피해 추가값과 피해 배율만 받도록 변경했다.
- 완료: `EffectVisualBuilder`가 확정된 반경 배율과 추가 반경만 받도록 변경했다.
- 완료: `SkillTargeting`의 Choice 조건이 `UnitSkills.HasChoice`를 직접 사용하도록 변경했다.
- 진행 중: `StatusRules`의 상태 강화 입력 분리.
- 진행 중: Projectile, Line, Zone, Single, Buff Executor별 강화 값 계산 이전.
- 진행 중: Actor에 전달되는 `SkillExecutionData`를 확정 값으로 교체.
- 미완료: `SkillExecutionData.cs`와 `.meta` 삭제.
- 미실행: 완료 후 Code Reviewer 검증.
