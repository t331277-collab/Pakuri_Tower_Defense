# UnitSkills 책임 축소 작업

## 목표

`Pakuri/Assets/Scripts/Combat/Skills/UnitSkills.cs`는 유닛이 획득한 스킬 선택 결과만 보관한다.

- 학습한 액티브 스킬 ID
- 학습한 패시브 스킬 ID
- 선택한 강화 효과 ID
- 선택한 마스터 스킬 ID
- 위 항목의 추가, 조회, 삭제

## 최종 책임

### UnitSkills

- 확정된 ID만 저장한다.
- 스킬 정의를 컴파일하지 않는다.
- 스킬 노드를 변환하거나 실행 값을 계산하지 않는다.
- 쿨타임, 시전 시간, 탄창, 재장전, 적중 횟수를 관리하지 않는다.
- 전투 대상과 상태이상 조건을 조회하지 않는다.

### SkillExecution

- 유닛별 실행 스킬과 변경 가능한 실행 상태를 관리한다.
- 쿨타임, 시전 시간, 탄창, 재장전 상태를 갱신한다.
- 실행 가능 여부를 판단한다.
- 실행할 스킬 정의와 현재 선택 결과를 Executor에 전달한다.

### 각 SkillExecutor

- `SkillDefinition`, `SkillNode`, `UnitSkills`를 읽어 해당 스킬 종류에 필요한 값을 완성한다.
- 즉시 실행 효과를 적용한다.
- Actor에 실제 실행에 필요한 값만 전달한다.

### 각 SkillActor

- 이동과 충돌을 처리한다.
- 실제 적중을 판정한다.
- 전달받은 피해와 상태 효과를 적용한다.
- 자신의 수명을 관리한다.

### GameFlow/Loading

- CSV 원본을 검증하고 `SkillDefinition`으로 변환한다.
- 전투 실행 중 같은 정의를 다시 컴파일하지 않게 준비한다.

## 기존 코드 이동

| 기존 내용 | 새 책임 |
|---|---|
| `SkillUseState` | `SkillExecution.cs`의 유닛별 실행 상태 |
| 기존 `UnitSkillData`의 강화 조합 | `SkillExecutionData`를 만드는 실행 상태와 각 스킬 종류의 Executor |
| 패시브 강화 조합 | `PassiveSkill.cs` |
| Trigger 강화 조합 | `SkillTrigger.cs` |
| `UnitSkillsBuilder`의 정의 변환 | `GameFlow/Loading` |
| `UnitSkillsBuilder`의 실행 상태 생성 | `SkillExecution` |
| 대상 상태 개수 계산 | `SkillTargeting.cs` |
| 피해 최종 계산 | `DamageCalculator.cs`와 각 Executor |
| 상태 효과 적용값 계산 | 각 Executor와 `StatusRules.cs` |

## UnitSkills 공개 API

- `AddActiveSkill`, `HasActiveSkill`, `RemoveActiveSkill`
- `AddPassiveSkill`, `HasPassiveSkill`, `RemovePassiveSkill`
- `AddEnhancement`, `HasEnhancement`, `RemoveEnhancement`
- `AddMasterSkill`, `HasMasterSkill`, `RemoveMasterSkill`
- `HasChoice`
- `Clear`

컬렉션은 외부에서 직접 수정하지 못하도록 읽기 전용으로 공개한다.

## 적용 순서

1. 강화 효과와 마스터 스킬 저장소를 분리한다.
2. 외부의 직접 컬렉션 수정을 `UnitSkills` 메소드 호출로 바꾼다.
3. 실행 상태와 실행 목록을 `SkillExecution`으로 옮긴다.
4. 입력, 적 AI, UI, 전투 관리자가 실행 목록을 `SkillExecution`에서 조회하게 바꾼다.
5. 각 Executor가 필요한 강화 노드를 직접 조합하게 바꾼다.
6. Actor에는 완성된 실행 값만 전달한다.
7. 공용 전투 코드의 `UnitSkillData` 이름을 제거하고 실행 시점 값은 `SkillExecutionData`로 명확히 구분한다.
8. 로딩 경계에서 스킬 정의를 준비하고 `UnitSkillsBuilder`를 삭제한다.
9. `UnitSkills.cs`에는 선택 결과 저장 API만 남긴다.

## 검증 기준

- `UnitSkills.cs`에 전투 시간, 대상 검색, 정의 컴파일, 노드 변환 코드가 없다.
- `UnitSkills.cs`에 `SkillUseState`, `SkillExecutionData`, 실행 상태 Builder 참조가 남지 않는다.
- 수정한 코드에 삼항 연산자를 새로 추가하지 않는다.
- 새 fallback 함수를 만들지 않는다.
- C# 프로젝트 컴파일 오류가 없다.
- Unity Play Mode 검증은 사용자가 수행한다.

## 적용 결과

- `UnitSkills.cs`는 학습한 액티브·패시브 스킬과 선택한 강화·마스터 ID만 보관한다.
- 쿨타임, 탄창, 재장전, 적중 횟수와 실행 목록은 `SkillExecution.cs`의 `SkillUseState`, `SkillExecutionState`로 이동했다.
- 기존 `UnitSkillData`는 실행 시점 값을 뜻하는 `SkillExecutionData`로 이름과 파일을 분리했다.
- 기존 `UnitSkillsBuilder`와 중간 `SkillExecutionStateBuilder`는 삭제했다.
- 런 세션 Choice ID의 강화·마스터 분류는 `SkillDefinitionCompiler.ApplyLearnedSkills`에서 한 번 수행한다.
- `UnitSkills` 컬렉션을 외부에서 직접 수정하던 코드는 추가·조회 API 호출로 바꿨다.
- C# 빌드 결과는 경고 2개, 오류 0개다. 경고는 작업 전에도 존재한 Unity 참조 어셈블리 버전 충돌이다.
