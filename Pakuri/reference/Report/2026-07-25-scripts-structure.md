# `Pakuri/Assets/Scripts` 전체 구조

- 작성 기준일: 2026-07-25
- 범위: `Pakuri/Assets/Scripts` 아래 C# 스크립트 69개
- 제외: Unity가 관리하는 `.meta` 파일
- 기준: 현재 작업 폴더에서 직접 확인한 파일 경로, 타입 선언, 공개·내부 메서드, Unity 생명주기 메서드

## 기호

- `├─`, `└─`: 같은 단계의 폴더 또는 파일
- `│`: 상위 폴더 구조가 계속됨
- `└─ 책임:`: 바로 위 스크립트의 주 책임과 역할

## 전체 실행 구조 요약

```text
Data의 CSV 파싱·검증
    ↓
GameFlow/Loading의 런타임 카탈로그·스킬 정의 생성
    ↓
GameFlow/RunSession·Stage·Spawn의 진행 상태와 유닛 생성
    ↓
Units의 전투 상태 모델·Actor·Registry
    ↓
Combat의 피해·상태·스킬 실행
    ↓
UI와 InGame/Animation의 화면 표시·입력 피드백
```

`InGameCombatManager`가 전투 중추다. `CombatUnitRegistry`, `EnemyActionController`, `SkillExecution`, `PassiveSkill`, `PlayerCombatInputController`, `EffectManager`를 연결한다. 데이터 정의는 `GameDataLoader`가 CSV 원본을 읽고 검증한 뒤 `GameDataCatalogBuilder`로 런타임 카탈로그를 만들며, `SkillDefinitionCompiler`가 데이터 정의를 실제 전투 스킬 정의로 변환한다.

## 폴더·스크립트 구조

```text
Pakuri/Assets/Scripts/
├─ Combat/                                      전투 계산과 스킬·상태 실행
│  ├─ InGameCombatManager.cs
│  │  └─ 책임: 유닛 등록, 전투 Tick, 피해·회복·상태·보호막 적용, 적 AI·플레이어 입력·패시브·트리거 실행을 조율하는 전투 중추.
│  ├─ Damage/
│  │  └─ DamageCalculator.cs
│  │     └─ 책임: 피해 속성, 공격·주문 계수, 방어력과 각종 배율을 사용해 원시 피해와 최종 피해를 계산.
│  ├─ Effects/
│  │  ├─ EffectManager.cs
│  │  │  └─ 책임: 스킬 Actor와 시각 효과 생성, 상태 효과 표시의 생성·갱신, 지속 효과 추적과 제거를 관리.
│  │  └─ EffectVisualBuilder.cs
│  │     └─ 책임: 효과 회전·크기·수명·범위·선형 모양·Hitbox 등 프리팹 기반 시각 표현을 공통 설정.
│  ├─ Skills/
│  │  ├─ UnitSkills.cs
│  │  │  └─ 책임: 유닛이 배운 액티브·패시브·강화·마스터 선택 ID를 보관하고 추가·조회·삭제.
│  │  ├─ Choices/
│  │  │  └─ SkillNode.cs
│  │  │     └─ 책임: 정규화된 스킬 노드를 실행 단계에서 쓰는 피해·처치·스킬 동작 연산 데이터로 표현.
│  │  ├─ Definitions/
│  │  │  └─ SkillDefinition.cs
│  │  │     └─ 책임: 스킬 슬롯·런타임 종류·대상·피해·상태·시각 효과·트리거·노드와 각 스킬 계열의 원본 및 런타임 정의 스키마를 제공.
│  │  ├─ Execution/
│  │  │  ├─ SkillExecution.cs
│  │  │  │  └─ 책임: 자동·수동·트리거 스킬 시전, 계열별 Executor 전달, 쿨다운·탄창·연사·Tick 상태, 학습 스킬 재구성을 관리.
│  │  │  ├─ SkillExecutionData.cs
│  │  │  │  └─ 책임: 기본 스킬과 선택 노드를 합쳐 한 번의 실행에 사용할 최종 수치·조건·추가 동작 스냅샷을 구성.
│  │  │  ├─ SkillExecutionRuleResolver.cs
│  │  │  │  └─ 책임: 상태 조건 피해, 치명타 보너스, 연사 순번 기반 피해·상태 보너스의 적용 가능 여부와 값을 판정.
│  │  │  └─ SkillTargeting.cs
│  │  │     └─ 책임: 대상 정렬·선정, 범위 중심·반지름·프리팹 배율, 효과 대상·연쇄 대상과 선택지·패시브·상태 요구 조건을 해석.
│  │  └─ SkillType/
│  │     ├─ Buff/
│  │     │  ├─ BuffSkillActor.cs
│  │     │  │  └─ 책임: Buff 시각 오브젝트를 대상에 따라가게 하고 정해진 수명 뒤 제거.
│  │     │  └─ BuffSkillExecutors.cs
│  │     │     └─ 책임: 일반 Buff, 보호막 Buff, 회복 Buff의 대상 선정·상태/수치 적용·추가 효과 실행.
│  │     ├─ Line/
│  │     │  ├─ LineSkillActor.cs
│  │     │  │  └─ 책임: 지속 선형 공격의 Tick, 선 내부 Hitbox 판정, 피해·넉백·상태·적중 효과와 시각 표현을 처리.
│  │     │  └─ LineSkillExecutor.cs
│  │     │     └─ 책임: 선형 스킬의 방향·길이·폭·지속 시간·Tick 주기·넉백·시각 Actor와 추가 효과를 구성해 실행.
│  │     ├─ Passive/
│  │     │  └─ PassiveSkill.cs
│  │     │     └─ 책임: 학습한 패시브의 지속 효과 바인딩, 일회성 효과, 사건 알림, 트리거 횟수·내부 쿨다운과 비활성 효과 제거를 관리.
│  │     ├─ Projectile/
│  │     │  ├─ ProjectileSkillActor.cs
│  │     │  │  └─ 책임: 투사체 이동·충돌·대상 적중·피해·상태·분기 피해·충격·만료 효과와 파괴 경계를 처리.
│  │     │  └─ ProjectileSkillExecutor.cs
│  │     │     └─ 책임: 투사체 수·확산·연사 순번·후속 투사체·직접 적중·상태·수명·시각 Actor를 계산하고 발사.
│  │     ├─ Single/
│  │     │  ├─ SingleChargeActor.cs
│  │     │  │  └─ 책임: 돌진형 단일 공격 상태를 Tick하고 접촉 대상 탐색과 적중 처리를 수행.
│  │     │  ├─ SingleChargeState.cs
│  │     │  │  └─ 책임: 돌진형 단일 공격의 진행 방향·속도·남은 거리·대상 등 실행 상태를 보관.
│  │     │  ├─ SingleSkillActor.cs
│  │     │  │  └─ 책임: 단일 공격의 일회성·애니메이션 기반·대상 추적 시각 오브젝트 수명을 관리.
│  │     │  ├─ SingleSkillExecutor.cs
│  │     │  │  └─ 책임: 단일·연쇄·돌진 공격, 다중 배치, 프리팹 Hitbox, 피해·상태·후속 공격·추가 효과를 실행.
│  │     │  └─ SingleSkillRules.cs
│  │     │     └─ 책임: 처형 임계값, 보스 피해, 단일 공격 피해 보정과 처치 후 쿨다운 초기화·환급 규칙을 판정.
│  │     ├─ Trigger/
│  │     │  └─ SkillTrigger.cs
│  │     │     └─ 책임: 전투 시작·스킬 시전·투사체 적중·피해·처치·상태/보호막 만료 사건을 조건·확률·횟수·쿨다운으로 걸러 후속 효과나 공격을 실행.
│  │     └─ Zone/
│  │        ├─ ZoneSkillActor.cs
│  │        │  └─ 책임: 지속 범위의 주기 Tick, Collider/반지름 대상 판정, 피해·상태·만료 효과와 시각 표현을 처리.
│  │        └─ ZoneSkillExecutor.cs
│  │           └─ 책임: 범위 중심·반지름·배치 수·지속 시간·Tick 주기·재시전·추가 피해 구역과 만료 효과를 구성해 실행.
│  └─ Status/
│     ├─ StatusEffectDefinition.cs
│     │  └─ 책임: 상태 분류·종류·조건·적용 범위·병합 정책·보호막 갱신 규칙과 런타임 상태 데이터 스키마를 정의.
│     ├─ StatusRules.cs
│     │  └─ 책임: 상태 적용 확률·임계 상태·이동/행동 제한·각종 전투 배율·조건 일치·스킬 효과 상태 적용 규칙을 계산.
│     ├─ StatusRuntimeCompiler.cs
│     │  └─ 책임: CSV 기반 상태·효과·트리거 문자열을 런타임 enum, 조건 그룹, `StatusRuntimeData`로 컴파일.
│     └─ StatusState.cs
│        └─ 책임: 유닛별 상태 컬렉션과 개별 상태 인스턴스의 중첩·지속 시간·보호막 흡수·병합·제거·Tick을 관리.
├─ Data/                                        CSV 원본 파싱·검증·중간 모델
│  ├─ CsvDataValidator.cs
│  │  └─ 책임: 원본 모델의 ID 참조, 런타임 값, 상태·트리거·스킬 그래프, Sprite·Prefab·Animator 경로를 종합 검증.
│  ├─ CsvParser.cs
│  │  └─ 책임: CSV 텍스트를 표와 행으로 분해하고 필수/선택 문자열·정수·실수·불리언 셀을 안전하게 읽는 저수준 파서.
│  ├─ CsvRowParser.cs
│  │  └─ 책임: 몬스터·스킬·선택지·트리거·적 CSV 행을 도메인별 Row로 변환하고 전체 `SourceModel`을 적재.
│  ├─ CsvSourceModel.cs
│  │  └─ 책임: 카탈로그·상태·상태 Payload 등 여러 CSV가 공유하는 원본 중간 모델과 공통 변환기를 정의.
│  └─ SkillGraphParser.cs
│     └─ 책임: 노드 타입·매개변수·그래프 행을 파싱·검증하고 소유자/게이트/허용값을 확인한 뒤 실행 가능한 정규화 노드를 생성.
├─ GameFlow/                                    데이터 초기화·런 진행·스테이지·생성
│  ├─ RunSession.cs
│  │  └─ 책임: 현재 런의 선택 몬스터, 파티, 학습 액티브·패시브·선택지, 보상 재화를 보관하고 선택 가능 조건을 판정.
│  ├─ Loading/
│  │  ├─ CsvCatalogEditor.cs
│  │  │  └─ 책임: Unity Editor에서 CSV 변경을 감지해 런타임 카탈로그 에셋을 동기화·검증하고 참조 에셋 목록을 갱신.
│  │  ├─ CsvRuntimeCatalog.cs
│  │  │  └─ 책임: CSV가 참조한 Sprite·Prefab·AnimatorController를 직렬화해 보관하고 정규화된 경로로 조회.
│  │  ├─ GameDataCatalog.cs
│  │  │  └─ 책임: 몬스터·적·상태·스킬·패시브·보상 선택 정의를 보관하고 ID·몬스터·슬롯 기반 조회 인덱스를 제공.
│  │  ├─ GameDataCatalogBuilder.cs
│  │  │  └─ 책임: 검증된 `SourceModel`을 몬스터·적·상태·스킬·효과·트리거·선택지·노드와 런타임 시각 데이터로 변환.
│  │  ├─ GameDataLoader.cs
│  │  │  └─ 책임: 씬 로드 전 CSV 런타임 카탈로그를 읽고 검증·빌드해 전역 `CurrentCatalog`로 초기화하며 실패를 보고.
│  │  └─ SkillDefinitionCompiler.cs
│  │     └─ 책임: 데이터 카탈로그의 스킬·패시브·노드·선택지 정의를 계열별 런타임 `SkillDefinition`과 실행 계획으로 컴파일.
│  ├─ Spawn/
│  │  ├─ UnitCombatStateFactory.cs
│  │  │  └─ 책임: 몬스터·현현 파티원·적·넥서스 정의와 런 상태를 독립 전투 상태 모델로 복제·초기화.
│  │  └─ UnitSpawnManager.cs
│  │     └─ 책임: 유닛 Prefab 생성·재사용·부활, 모델/Actor 바인딩, 세션 파티 복원, 적 생성과 전투 Registry 등록을 수행.
│  └─ Stage/
│     ├─ MonsterDayRecovery.cs
│     │  └─ 책임: 일차 전환 시 몬스터의 일시 상태·보호막·스킬 실행 상태를 초기화하고 체력을 복구.
│     └─ StageManager.cs
│        └─ 책임: 런 시작, 일차별 조우 Spawn, 적 전멸 대기, 보상·수감자 후보, 승패·다음 일차·메인 메뉴 흐름과 스테이지 CSV 표를 관리.
├─ InGame/                                      인게임 공통 표현
│  └─ Animation/
│     └─ AnimationController.cs
│        └─ 책임: 공격·피격·사망·대기 애니메이션 상태 전환, 사망 마지막 프레임 고정과 부활 복귀를 제어.
├─ UI/                                          메뉴와 인게임 화면·조작
│  ├─ MainMenu/
│  │  └─ MainMenuUIManager.cs
│  │     └─ 책임: 인트로·메인 메뉴·몬스터 선택 패널 전환, 선택 몬스터를 `StartContext`에 기록하고 게임 씬 시작.
│  └─ InGame/
│     ├─ DebugUI.cs
│     │  └─ 책임: 디버그용 액티브·패시브 학습, 강화·마스터 선택 적용, `RunSession`과 현재 유닛 스킬 상태 동기화.
│     ├─ InGameUIManager.cs
│     │  └─ 책임: 일차 보상, 재화 수령, 수감자 파티 편성, Offering 선택, Manifest 성공·실패 UI와 다음 일차 진행을 통합 관리.
│     ├─ DamageMeter/
│     │  ├─ DamageMeterRuntimeTracker.cs
│     │  │  └─ 책임: 전투 피해 적용 사건을 구독해 몬스터·스킬 발생 원본별 누적 피해 기록을 수집·초기화.
│     │  └─ DamageMeterUIController.cs
│     │     └─ 책임: 누적 피해를 파티·스킬별로 정렬하고 이름·색상·분할 막대·수치를 피해량 패널에 표시.
│     ├─ MonsterPanel/
│     │  └─ MonsterPanelUI.cs
│     │     └─ 책임: 파티 슬롯별 몬스터 초상과 액티브 스킬 슬롯의 아이콘·쿨다운·재장전 상태를 갱신.
│     ├─ Nexus/
│     │  └─ NexusHealthDisplay.cs
│     │     └─ 책임: 넥서스 현재 체력과 최대 체력을 UI 문구로 표시.
│     └─ UtilityPanel/
│        └─ InGameUtilityPanelController.cs
│           └─ 책임: 배속 순환과 선택 1P 몬스터의 자동 스킬 모드 토글 버튼을 연결하고 시간 배율을 복구.
└─ Units/                                       유닛 데이터·행동·표현·등록
   ├─ Collision/
   │  └─ UnitHitboxOverlap.cs
   │     └─ 책임: 대상 유닛의 Collider가 주어진 Hitbox Collider 집합과 실제로 겹치는지 공통 판정.
   ├─ Definitions/
   │  ├─ EnemyDefinition.cs
   │  │  └─ 책임: 적 기본 능력치·스킬·프리팹·패시브 정의와 적 패시브 보정 종류를 데이터 에셋으로 표현.
   │  └─ MonsterDefinition.cs
   │     └─ 책임: 몬스터 기본 능력치·초상·Prefab·Animator·스킬·패시브·보상 선택 정의를 데이터 에셋으로 표현.
   ├─ Enemy/
   │  ├─ Actor/
   │  │  └─ EnemyActor.cs
   │  │     └─ 책임: 적 전투 모델을 GameObject 표현에 연결하고 피해 숫자와 체력·상태 표시를 갱신.
   │  ├─ AI/
   │  │  ├─ EnemyActionController.cs
   │  │  │  └─ 책임: 적마다 이동·일반 공격·스킬 선택·지원 스킬·넥서스 공격을 Tick 단위로 실행.
   │  │  └─ EnemyCombatDecision.cs
   │  │     └─ 책임: 가장 가까운 플레이어, 최저 체력 적 아군, 공격/지원 스킬과 실행 가능 조건을 순수 판정.
   │  └─ Passive/
   │     └─ EnemyPassiveModifiers.cs
   │        └─ 책임: 적 생성 시 패시브 능력치 보정과 공격 시 속성별 최종 피해 배율을 적용.
   ├─ Model/
   │  └─ UnitCombatState.cs
   │     └─ 책임: 유닛 진영·역할·식별 정보·공격/방어 능력치·체력/보호막·스킬·상태 컬렉션과 적 전용 실행 상태를 보관.
   ├─ Monster/
   │  ├─ Actor/
   │  │  └─ MonsterActor.cs
   │  │     └─ 책임: 몬스터 모델을 GameObject에 연결하고 공격·피격·사망·부활 애니메이션, Collider, 월드 표시를 제어.
   │  └─ Input/
   │     └─ PlayerCombatInputController.cs
   │        └─ 책임: 선택 1P의 수동 스킬 키 입력, 자동 모드 전환, 자동 시전 가능 조건과 화면 내 적 존재 여부를 판정.
   ├─ Nexus/
   │  └─ Actor/
   │     └─ NexusActor.cs
   │        └─ 책임: 넥서스 전투 모델을 씬 Actor에 연결하고 체력 표시와 외부 체력 설정을 반영.
   ├─ Presentation/
   │  ├─ DamageNumberPopup.cs
   │  │  └─ 책임: 월드 공간 피해 숫자 인스턴스를 생성해 상승·페이드시키고 재사용 가능한 표시 흐름을 관리.
   │  └─ UnitWorldDisplay.cs
   │     └─ 책임: 유닛 체력·보호막 막대, 수치 문구, 상태 이름과 피해 숫자를 모델 상태에 맞춰 갱신.
   └─ Registry/
      └─ CombatUnitRegistry.cs
         └─ 책임: 전투 모델·Actor·Hitbox를 하나의 Entry로 등록하고 대상 위치·Collider 조회, 표시 갱신, 사망 처리와 역조회 제공.
```

## 책임 경계와 의존 방향

1. `Data`는 CSV를 읽고 검증하는 계층이다. 전투를 직접 실행하지 않는다.
2. `GameFlow/Loading`은 검증된 원본을 런타임 정의로 바꾼다. 씬 전투 상태를 직접 소유하지 않는다.
3. `RunSession`은 런 전체에서 유지할 선택 결과를 소유한다. 현재 체력·쿨다운 같은 전투 순간 상태는 `UnitCombatState`와 `SkillUseState`가 소유한다.
4. `UnitSpawnManager`와 `UnitCombatStateFactory`는 정의·세션 상태에서 전투 모델과 Actor를 만든다.
5. `CombatUnitRegistry`는 현재 전장 참가자 연결을 소유한다. `InGameCombatManager`는 Registry를 기준으로 전투 규칙을 실행한다.
6. 각 Skill `Executor`는 시전 시 배치와 초기 적용을 맡고, 각 Skill `Actor`는 생성 뒤 프레임·Tick·충돌 기반 지속 동작을 맡는다.
7. `UI`와 `Presentation`은 모델·사건을 읽어 표시하거나 사용자 명령을 전달한다. 피해·상태 계산 규칙은 `Combat`에 남는다.

## 주요 생명주기

```text
GameDataLoader.EnsureInitialized()
    → CSV 파싱·검증
    → GameDataCatalogBuilder.BuildRuntimeCatalog()
    → GameDataCatalog 조회 준비

StageManager.StartCurrentDay()
    → RunSession 준비
    → UnitSpawnManager로 파티·적 생성
    → UnitCombatStateFactory로 전투 모델 생성
    → CombatUnitRegistry 등록

InGameCombatManager.Update()
    → 대기 중인 패시브 변경 반영
    → 스킬 쿨다운·탄창 상태 Tick
    → 플레이어 자동 스킬
    → 플레이어 수동 입력
    → 적 AI
    → 유닛 상태 지속 시간 Tick
    → 새로 대기 중인 패시브 변경 반영

시전·적중 사건
    → Skill Executor/Actor
    → Damage·Status 적용
    → Trigger 후속 처리

전투 종료
    → StageManager 보상·수감자·승패 처리
    → 다음 일차에서 MonsterDayRecovery 적용
```

## 확인 근거와 한계

- `rg --files Pakuri/Assets/Scripts -g '*.cs'` 결과: C# 파일 69개.
- 파일별 타입 선언, 공개·내부 메서드, Unity 생명주기 메서드를 현재 작업 트리에서 확인했다.
- 현재 작업 트리에는 이 문서 작성 전부터 `EffectManager.cs` 수정 사항이 존재했다. 본 문서는 현재 보이는 수정 상태를 기준으로 설명하며 해당 스크립트를 변경하지 않는다.
- 이 문서는 코드 구조 설명이다. Prefab·Scene의 실제 직렬화 연결과 Unity Play Mode 동작은 별도 검증 범위다.
