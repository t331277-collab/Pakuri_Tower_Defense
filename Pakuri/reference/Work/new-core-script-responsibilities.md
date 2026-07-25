# NewCore 스크립트 구조와 책임

## 범위

- 대상: `Pakuri/Assets/Scripts/**/*.cs`
- 포함: 생산 C# 스크립트 86개
- 제외: `Core/Tests/Editor/**/*.cs`, `.meta`, 어셈블리 파일, 씬, 프리팹, 기타 에셋
- 근거: 검사한 스크립트 선언, 의존성, 공개 API, 책임 주석

## 전체 흐름

```text
CSV
 └─→ CsvParser
      └─→ GameDefinitionCatalog
           └─→ GameBootstrap
                ├─→ StageManager ─→ SpawnManager ─→ Unit Model ↔ Unit Actor
                ├─→ InGameActionManager
                │    ├─→ Unit Action Controller
                │    ├─→ SkillExecutionRuntime ─→ Skill Executor
                │    │                          ├─→ InGameCombatManager
                │    │                          ├─→ SkillActorManager
                │    │                          └─→ EffectManager
                │    └─→ Status / Cooldown Tick
                └─→ InGame UI Controllers
```

## 스크립트 트리

```text
Scripts/
├─ Core/ — 불변 정의, CSV 로딩, 시작 구성
│  ├─ Bootstrap/
│  │  └─ GameBootstrap.cs
│  │     └─ CSV·리소스 초기화, 씬 Manager 구성, 중앙 전투 Tick 진입점
│  ├─ Catalog/
│  │  └─ GameDefinitionCatalog.cs
│  │     └─ 파싱된 불변 Definition 저장과 ID 조회 제공
│  ├─ Parsing/
│  │  └─ CsvParser.cs
│  │     └─ CSV 행을 타입 변환하고 검증된 Definition Catalog 생성
│  └─ Definitions/
│     ├─ Choices/
│     │  ├─ SkillChoiceDefinition.cs
│     │  │  └─ 스킬 Choice와 몬스터 보상 선택 매핑 정의
│     │  ├─ ChoiceNodeDefinition.cs
│     │  │  └─ Choice·Skill·Trigger 그래프 노드 행 정의
│     │  ├─ NodeTypeDefinition.cs
│     │  │  └─ 노드 타입·핸들러·종류·런타임 지원 정보 정의
│     │  └─ NodeParamDefinition.cs
│     │     └─ 노드 파라미터 순서와 검증 계약 정의
│     ├─ Skills/
│     │  ├─ SkillDefinition.cs
│     │  │  └─ 모든 스킬 CSV Definition의 공통 불변 기반
│     │  ├─ ProjectileDefinition.cs
│     │  │  └─ 투사체 스킬의 타입화된 불변 Definition
│     │  ├─ LineAttackDefinition.cs
│     │  │  └─ 선형 공격의 타입화된 불변 Definition
│     │  ├─ AreaAttackDefinition.cs
│     │  │  └─ 범위·필드 공격의 타입화된 불변 Definition
│     │  ├─ SingleAttackDefinition.cs
│     │  │  └─ 단일 공격의 타입화된 불변 Definition
│     │  ├─ BuffDefinition.cs
│     │  │  └─ 버프 스킬의 타입화된 불변 Definition
│     │  ├─ HealDefinition.cs
│     │  │  └─ 회복 스킬의 타입화된 불변 Definition
│     │  ├─ ShieldDefinition.cs
│     │  │  └─ 보호막 스킬의 타입화된 불변 Definition
│     │  ├─ PassiveDefinition.cs
│     │  │  └─ 패시브 스킬의 타입화된 불변 Definition
│     │  └─ SkillTriggerDefinition.cs
│     │     └─ 이벤트 기반 스킬 Trigger 행 정의
│     ├─ Stage/
│     │  ├─ StageDefinition.cs
│     │  │  └─ Stage CSV Definition의 공통 불변 기반
│     │  ├─ StageDayDefinition.cs
│     │  │  └─ Day 순서와 연결된 Encounter·Reward 구성 정의
│     │  ├─ StageEncounterDefinition.cs
│     │  │  └─ Encounter 적 구성·타이밍·배치 정의
│     │  └─ StageRewardDefinition.cs
│     │     └─ 재화·포로·현현·유물 보상 정의
│     ├─ Status/
│     │  └─ StatusDefinition.cs
│     │     └─ 불변 상태 효과 규칙과 보정치 정의
│     └─ Units/
│        ├─ UnitDefinition.cs
│        │  └─ 유닛 전투 데이터의 공통 불변 기반
│        ├─ MonsterDefinition.cs
│        │  └─ 몬스터 데이터와 Catalog 순서 매핑 정의
│        └─ EnemyDefinition.cs
│           └─ 적 능력치·스킬·패시브·Nexus 피해 정의
│
├─ Run/ — 현재 Run 상태와 보상 진행
│  ├─ RunSessionModel.cs
│  │  └─ 현재 Stage·Day·보상 처리 상태와 최종 Run 결과 소유
│  ├─ StageManager.cs
│  │  └─ Run 진행·재화·필드 유닛·씬 Stage 연결 소유
│  ├─ PartyRoster.cs
│  │  └─ 파티 순서·정원·중복 방지 소유
│  ├─ PrisonerInventory.cs
│  │  └─ 포로 등록·교체·조회·정확한 소비 소유
│  └─ Services/
│     ├─ RewardService.cs
│     │  └─ 전투 후 재화·포로 보상 계산과 지급
│     ├─ OfferingService.cs
│     │  └─ 포로를 소비하는 스킬 Choice 후보 생성과 확정
│     └─ ManifestationService.cs
│        └─ 현현 시도 판정과 성공 후보 영입
│
├─ Spawn/
│  └─ SpawnManager.cs
│     └─ Encounter Model 생성, Monster·Enemy Actor 인스턴스화, Spawn 기록 등록
│
├─ Units/ — 유닛 런타임 상태와 Unity 표현
│  ├─ Models/
│  │  ├─ UnitBaseModel.cs
│  │  │  └─ 전투 위치·체력·보호막 레이어·상태·런타임 보정치 소유
│  │  ├─ MonsterModel.cs
│  │  │  └─ Monster Definition·SkillBucket·자동 행동·라운드 초기화 소유
│  │  ├─ EnemyModel.cs
│  │  │  └─ Enemy Definition·SkillBucket·Nexus 접촉·라운드 초기화 소유
│  │  └─ NexusModel.cs
│  │     └─ Nexus 체력·생존·Nexus 피해 적용 표현
│  └─ Actors/
│     ├─ UnitActor.cs
│     │  └─ Model 위치·월드 상태·피해 숫자를 Unity에 투영
│     ├─ MonsterActor.cs
│     │  └─ Monster Model 연결과 공격·피격·사망 애니메이션 표현
│     ├─ EnemyActor.cs
│     │  └─ Enemy Model 연결과 공격·Nexus 접촉·패배 표현
│     └─ NexusActor.cs
│        └─ Nexus Model 연결과 Inspector 기반 체력 표현
│
├─ Combat/ — 전투 순서·실행·상태 변경·시각 수명주기
│  ├─ InGameCombatManager.cs
│  │  └─ 스킬 실행·최종 전투 계산·상태 적용·이벤트·Trigger 조율
│  ├─ Actions/
│  │  ├─ InGameActionManager.cs
│  │  │  └─ 단일 전투 Tick 순서 실행과 전투 범위 행동 상태 정리
│  │  ├─ UnitActionController.cs
│  │  │  └─ 공통 쿨다운 확인과 스킬 실행 명령 제공
│  │  ├─ MonsterActionController.cs
│  │  │  └─ 몬스터 자동 행동과 수동 스킬 가능 여부 중재
│  │  ├─ EnemyActionController.cs
│  │  │  └─ 적 대상 선정·스킬 선택·이동·Nexus 접촉 행동 실행
│  │  ├─ PlayerInputController.cs
│  │  │  └─ 수동 명령 상태 소유와 Unity 마우스 입력 번역
│  │  └─ UnitMovementController.cs
│  │     └─ 전투 좌표의 일반 이동과 강제 변위 계산
│  ├─ Effects/
│  │  └─ EffectManager.cs
│  │     └─ 이펙트 요청·Unity 인스턴스·Transform 동기화·삭제 소유
│  ├─ Status/
│  │  └─ StatusEffect.cs
│  │     └─ 상태 지속시간·스택과 시간제 런타임 보정 수명주기 소유
│  └─ Skills/
│     ├─ Runtime/
│     │  ├─ SkillCooldown.cs
│     │  │  └─ 쿨다운·탄창·재장전·발사 간격 런타임 상태 소유
│     │  ├─ SkillBucket.cs
│     │  │  └─ 유닛 스킬과 해당 쿨다운 권한의 공통 저장소
│     │  ├─ MonsterSkillBucket.cs
│     │  │  └─ 몬스터 스킬 학습·Choice 제한·패시브 선행 조건 소유
│     │  └─ EnemySkillBucket.cs
│     │     └─ 고정 적 스킬 슬롯과 공유 쿨다운 구성 소유
│     ├─ Actors/
│     │  ├─ SkillActorManager.cs
│     │  │  └─ 활성·시간제·예약 SkillActor 컬렉션과 중앙 Tick 소유
│     │  ├─ ProjectileActor.cs
│     │  │  └─ 투사체 이동·교차 적중·관통·종료 소유
│     │  ├─ AreaAttackActor.cs
│     │  │  └─ 범위 공격 시각 지속시간 소유
│     │  ├─ LineAttackActor.cs
│     │  │  └─ 선형 공격 시각 지속시간 소유
│     │  ├─ SingleAttackActor.cs
│     │  │  └─ 단일 공격 시각 지속시간 소유
│     │  └─ BuffActor.cs
│     │     └─ 버프 시각 지속시간 소유
│     └─ Execution/
│        ├─ SkillExecutionRequest.cs
│        │  └─ 시전자·대상·Trigger 문맥·계보·실행 결과 전달
│        ├─ SkillExecutionPlan.cs
│        │  └─ 학습 노드를 수치·조건·대상·쿨다운 규칙으로 해석
│        ├─ SkillExecutionRuntime.cs
│        │  └─ 요청 검증과 Plan·계열 Executor·쿨다운 확정 조율
│        ├─ SkillExecutor.cs
│        │  └─ 대상·피해·상태·그래프·시각 실행기의 공통 기반
│        ├─ SkillTargeting.cs
│        │  └─ 진영·거리·체력·상태·형태·투사체 교차로 대상 선정
│        ├─ SkillEffectGraphRuntime.cs
│        │  └─ Choice·Skill·Trigger 효과 그래프를 전투 작업으로 해석
│        ├─ SkillTriggerDispatcher.cs
│        │  └─ 전투 이벤트 평가와 일치 Trigger 예약·실행
│        ├─ ProjectileExecutor.cs
│        │  └─ 투사체 Actor 생성과 충돌 기반 효과 적용
│        ├─ LineAttackExecutor.cs
│        │  └─ 선형 범위 대상 선정과 공격 결과 적용
│        ├─ AreaAttackExecutor.cs
│        │  └─ 즉시 범위와 대상 재평가 지속 필드 실행
│        ├─ SingleAttackExecutor.cs
│        │  └─ 단일 대상 피해·배치·부가 상태 실행
│        ├─ BuffExecutor.cs
│        │  └─ 버프 대상 선정과 상태·런타임 보정 적용
│        ├─ HealExecutor.cs
│        │  └─ 회복 대상 선정과 체력 회복 요청
│        ├─ ShieldExecutor.cs
│        │  └─ 보호막 대상 선정과 보호막 적용 요청
│        └─ PassiveExecutor.cs
│           └─ 즉시 패시브 효과 실행 경계 제공
│
└─ UI/ — Unity 패널과 사용자 명령
   ├─ MainMenu/
   │  └─ NewCoreMainMenuController.cs
   │     └─ Main Menu 패널 전환과 선택 Monster Run 시작
   └─ InGame/
      ├─ InGameUIManager.cs
      │  └─ 인게임 패널 전환과 Stage 결과 명령 조율
      ├─ RewardPanelController.cs
      │  └─ 지급 보상 표시와 포로·계속 명령 소유
      ├─ PrisonPanelController.cs
      │  └─ 포로·파티 슬롯 표시와 Offering·현현 선택 분기
      ├─ OfferingPanelController.cs
      │  └─ Offering 후보 표시와 선택 후보 확정
      ├─ ManifestationPanelController.cs
      │  └─ 현현 결과 표시와 모집·건너뛰기 명령 소유
      ├─ DamageMeter/
      │  ├─ NewCoreDamageMeterTracker.cs
      │  │  └─ 전투 이벤트에서 Monster·스킬 출처별 피해 집계
      │  └─ NewCoreDamageMeterUIController.cs
      │     └─ 파티 총 피해와 스킬별 피해 구간 표시
      ├─ Debug/
      │  └─ NewCoreDebugUIController.cs
      │     └─ 개발자 스킬 학습·Choice 선택 제어 제공
      ├─ MonsterPanel/
      │  └─ NewCoreMonsterPanelUI.cs
      │     └─ 파티 Portrait와 액티브 스킬 쿨다운 표시
      └─ UtilityPanel/
         └─ NewCoreUtilityPanelController.cs
            └─ Auto 전투 명령과 `Time.timeScale` 순환 소유
```

## 책임 경계 요약

```text
Definition = 불변 Authoring 데이터
Model      = 현재 런타임 상태
Bucket     = 학습·할당 스킬과 쿨다운
Controller = 사용자·유닛 명령 중재
Executor   = 스킬 계열 하나의 실행
Actor      = 생성된 유닛·스킬 표현 또는 수명주기
Manager    = 시스템 전체 순서·조율·소유권
Service    = Run 도메인의 단일 업무 처리
UI         = 표시와 사용자 명령 전달
```
