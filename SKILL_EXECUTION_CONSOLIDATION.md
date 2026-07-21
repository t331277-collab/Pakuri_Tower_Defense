# Skill Execution 통폐합 기준

## 목표

`Pakuri/Assets/Scripts/Combat/Skills/Execution`의 실행 경로를 단순화한다. 상태가 없는 계약 계층, 단순 전달 객체, 한 번만 위임하는 유틸을 제거하고 실제 전투 규칙과 Unity 생명주기 코드는 분리 상태를 유지한다.

## 작업 전 근거

- Execution 하위 C# 파일: 27개
- `IInGameSkillExecutor` 구현은 고정된 실행기 9개뿐이며 외부 등록 경로가 없다.
- 각 실행기는 인스턴스 필드가 없는 무상태 객체다.
- `SkillExecutionResult`에서 실제 분기에 쓰이는 값은 `Routed`뿐이다.
- `UnitSkillController`는 유닛 상태를 소유하지 않고 요청 객체를 만들어 `SkillExecutionSystem`으로 전달한다.
- `SkillAreaUtility`, `SkillDeploymentCenterUtility`는 대상·범위 계산이라는 같은 책임을 `SkillTargetingUtility`와 나눈다.
- `SkillStatusSpecUtility`, `SkillStatusApplyUtility`는 상태 데이터 해석과 적용이라는 한 실행 책임을 나눈다.
- `SingleSkillRuleHandlers`의 인터페이스들은 구현이 하나뿐이며 런타임 교체 지점이 없다.
- `ZoneSkillActor`의 `ZoneHitboxDebug`와 `eve-c` 조건은 특정 스킬용 진단 코드다.

## 필수 변경

### 1. 실행 계약 제거

- `Contracts/SkillExecutorContracts.cs` 삭제
- `SkillExecutorRegistry` 삭제
- 실행기를 무상태 `static` 클래스로 변경
- `SkillExecutionSystem`이 런타임 데이터 형식에 따라 실행기를 직접 호출
- 실행 결과는 `SkillExecutionResult` 대신 `bool` 사용
- `SkillExecutionStatus`, 실행기 이름, 라우팅 전용 로그 삭제
- `SkillExecutionContext`는 `SkillExecutionSystem.cs`로 이동
- 실제 실행에 쓰이지 않는 `DeltaTime`, `SkillRuntimeData` 문맥 값 삭제

### 2. 전달 계층 제거

- `Runtime/UnitSkillController.cs` 삭제
- `Runtime/SkillExecutionRequest.cs` 삭제
- 자동·수동·선택·트리거 실행을 `SkillExecutionSystem`에서 직접 조율
- 컨트롤러 캐시와 정리 목록 삭제

### 3. 규칙과 유틸 통합

- `SingleSkillRuleHandlers.cs`를 `SingleSkillRules.cs`로 이름 변경
- 구현 하나뿐인 규칙 인터페이스와 클래스를 정적 규칙 함수로 통합
- `SkillAreaUtility`와 `SkillDeploymentCenterUtility`를 `SkillTargetingUtility`에 병합
- `SkillStatusSpecUtility`와 `SkillStatusApplyUtility`를 `SkillStatusUtility`에 병합
- `SkillExecutionUtility`에서 다른 유틸을 그대로 호출하는 대상 검색 래퍼 제거

### 4. 진단 전용 코드 제거

- `InGameCombatManager`의 `logSkillExecutionContracts`와 전달 인자 삭제
- `SkillExecutionSystem`의 라우팅 성공 로그 삭제
- `ZoneSkillActor`의 `ZoneHitboxDebug`, `eve-c` 전용 분기와 문자열 조립 함수 삭제

### 5. 안전한 데드 멤버 정리

- `SkillExecutionPlan`에서 읽히지 않는 보조 속성만 삭제
- CSV 그래프 작성 코드가 사용하는 `SkillExecutionPlanNodeKind`와 실행 정의는 유지
- 호출 근거가 있는 Snapshot 공개 멤버는 유지

## 유지 대상

- `LineSkillActor`, `ProjectileSkillActor`, `ZoneSkillActor`: MonoBehaviour 생명주기와 Unity 직렬화가 있으므로 유지
- `SkillExecutionSnapshot`: 실행 시점 데이터 고정 책임 유지
- `SkillTriggerRuntime`: 이벤트·재진입·쿨다운 상태 책임 유지
- `SkillMultiEffectExecutor`: 그래프의 여러 효과 실행 책임 유지
- `PassiveEffectRuntime.MaxRefreshPasses`: 이벤트 연쇄의 무한 반복 방지이므로 유지
- 대상 없음, 파괴된 Actor/Collider, 선택적 이펙트·상태, 코루틴 지연 뒤의 유효성 검사는 정상 게임 흐름이므로 유지
- 실제 설정 오류를 알리는 MultiEffect 오류 로그는 유지

## 목표 구조

```text
Execution/
  Actors/
    LineSkillActor.cs
    ProjectileSkillActor.cs
    ZoneSkillActor.cs
  Executors/
    BuffAndSingleSkillExecutors.cs
    LineSkillExecutor.cs
    ProjectileSkillExecutor.cs
    SingleSkillExecutor.cs
    SingleSkillRules.cs
    ZoneSkillExecutor.cs
  Runtime/
    PassiveEffectRuntime.cs
    SkillExecutionPlan.cs
    SkillExecutionSnapshot.cs
    SkillExecutionSystem.cs
    SkillMultiEffectExecutor.cs
    SkillPlanActionDispatcher.cs
    SkillTriggerRuntime.cs
  Utilities/
    SkillExecutionUtility.cs
    SkillOnHitAdditionalDamageUtility.cs
    SkillStatusUtility.cs
    SkillTargetingUtility.cs
```

목표 파일 수는 20개다. Unity 직렬화 대상의 `.meta` GUID는 보존하고, 삭제 파일의 GUID 참조가 없는지 확인한다.

## 완료 기준

- Execution 하위 C# 파일이 20개다.
- `Contracts`, `SkillExecutorRegistry`, `TypedSkillExecutor`, `SkillExecutionResult`, `SkillExecutionRequest`, `UnitSkillController` 참조가 없다.
- `SkillAreaUtility`, `SkillDeploymentCenterUtility`, `SkillStatusSpecUtility`, `SkillStatusApplyUtility`, `SingleSkillRuleHandlers` 참조가 없다.
- `logSkillExecutionContracts`, `ZoneHitboxDebug`, `eve-c` 전용 진단 분기가 없다.
- Unity 스크립트 컴파일 오류 0건
- 프로젝트 스크립트 검증 오류 0건
- 스킬 데이터 검증 오류 0건
- Unity Console 오류 0건

## Reviewer 판정 기준

Reviewer는 위 완료 기준과 실제 diff를 대조한다. 추가 통폐합은 다음 조건을 모두 만족할 때만 권고한다.

- 다른 책임을 섞지 않는다.
- Unity 생명주기 또는 직렬화 경계를 무너뜨리지 않는다.
- 런타임 교체 가능성이라는 추측이 아니라 현재 참조와 상태 소유 근거가 있다.
- 코드 감소가 새 분기나 우회 호출을 만들지 않는다.
