# 유닛·스킬 아키텍처 전면 개편 설계

## 1. 문서 목적

이 문서는 다음 두 기존 폴더의 구조를 새 유닛·스킬 아키텍처로 완전히 교체하기 위한 설계 기준이다.

- `Pakuri/Assets/Scripts/Combat/Skills`
- `Pakuri/Assets/Scripts/Units`

최종 완료 상태에서는 위 두 폴더와 해당 폴더의 `.meta` 파일이 존재하면 안 된다. 기존 파일을 그대로 남긴 채 새 구조를 병행하는 상태는 완료로 보지 않는다.

이 문서는 설계 문서다. 현재 저장소에 아직 없는 `UnitBaseModel`, `MonsterModel`, `EnemyModel`, `SkillModel` 등의 이름은 **구현 완료 사실이 아니라 새로 만들 목표 타입**을 뜻한다.

## 2. 코드에서 확인한 현재 상태

다음 내용은 실제 파일을 읽어 확인했다.

- `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs`
  - `UnitCombatState`가 유닛 공통 전투 상태를 가진다.
  - `UnitCombatState` 안에 `UnitSkills Skills`와 `SkillExecutionState SkillState`가 함께 있다.
  - `EnemyCombatState`만 `UnitCombatState`를 상속한다. 현재 Monster 전용 모델 클래스는 없다.
- `Pakuri/Assets/Scripts/Combat/Skills/UnitSkills.cs`
  - 학습 액티브/패시브 ID, 선택 강화 ID, 마스터 ID를 `HashSet<string>`으로 저장한다.
  - 실제 스킬 런타임 객체를 보유하는 구조는 아니다.
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
  - `SkillExecution`, `SkillUseState`, `SkillExecutionState`가 한 파일에 있다.
  - 실행 분배뿐 아니라 `RebuildLearnedSkillState`, `RebuildAssignedSkillState` 및 여러 런타임 상태 생성·재구성 책임이 함께 있다.
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionData.cs`
  - 정의와 선택 결과를 실행용 값으로 모으는 중앙 데이터 역할을 한다.
- `Pakuri/Assets/Scripts/Combat/Skills/Definitions/SkillDefinition.cs`
  - 공통 정의와 Buff, Line, Projectile, Single, Zone, Passive 등 여러 스킬 정의가 한 파일에 모여 있다.
  - `SkillChoiceDefinition`도 같은 파일에 있다.
- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitCombatStateFactory.cs`
  - Enemy는 `EnemyCombatState`, Monster와 Nexus는 `UnitCombatState`로 생성한다.
  - Monster 진행 상태를 `UnitSkills`에 반영한다.
- `Pakuri/Assets/Scripts/Units/Definitions/MonsterDefinition.cs`와 `EnemyDefinition.cs`
  - 유닛 정의가 액티브 스킬, 패시브, 트리거 정의 또는 ID를 참조한다.
- `Pakuri/Assets/Scripts/Combat/Effects/EffectManager.cs`
  - 비주얼 또는 프리팹으로 효과 GameObject를 만들고 제거한다.
  - 시간제·추적 비주얼에는 `SingleSkillActor`, `BuffSkillActor`를 붙인다.
- Projectile, Line, Zone Executor
  - `EffectManager.CreateEffect` 또는 `CreateSkillActorObject`로 GameObject를 만든다.
  - 생성된 GameObject에 각각 `ProjectileSkillActor`, `LineSkillActor`, `ZoneSkillActor`를 붙여 초기화한다.
- `Pakuri/Assets/Scripts/Units/Monster/Actor/MonsterActor.cs`
  - MonoBehaviour이며 `UnitCombatState Model`을 `Initialize`로 주입받는다.
- `Pakuri/Assets/Scripts/Units/Enemy/Actor/EnemyActor.cs`
  - MonoBehaviour이며 `EnemyCombatState Model`을 `Initialize`로 주입받는다.

따라서 현재 코드도 이미 "순수 상태 객체 + GameObject Actor" 방향을 일부 사용한다. 새 구조에서는 이 경계를 명확하게 만들고 중앙 집중된 스킬 상태·효과 조립 책임을 제거한다.

## 3. 확정 설계 결정

### 3.1 스킬 비주얼 GameObject에 무엇을 붙이는가

`SkillModel` 자체를 붙이지 않는다. `EffectManager`가 만든 GameObject에는 필요한 경우에만 스킬 종류별 **경량 Actor/Driver MonoBehaviour**를 붙인다.

예시:

- `ProjectileSkillActor`: 이동, 충돌, 도착, 수명 종료
- `LineSkillActor`: 선형 판정의 표시, 틱 시점 전달, 수명 종료
- `AreaSkillActor`: 범위 표시, 틱 시점 전달, 수명 종료
- `FollowingEffectActor`: 대상 추적과 비주얼 수명 관리

Actor가 가져도 되는 것은 한 번의 실행에 필요한 불변 실행 정보와 최소 참조뿐이다.

- 시전자 식별 또는 시전자 모델 참조
- 이번 실행에서 확정된 `ResolvedSkillSpec`
- 대상 또는 위치
- 실행 식별자
- 제거를 요청할 효과 관리자 참조

Actor가 가지면 안 되는 것은 다음과 같다.

- 학습 여부
- 쿨타임의 원본 상태
- 강화·마스터 선택 목록
- 공유 Base 스킬 정의의 변경 가능한 사본
- 유닛 전체 스킬 목록
- 다음 시전까지 유지되는 스킬 상태의 소유권

스킬의 영속 런타임 상태는 시전자 `MonsterModel` 또는 `EnemyModel`이 보유한 `SkillModel`에 남는다. 비주얼이 없어져도 스킬과 쿨타임은 유지되고, 유닛이 제거되면 그 유닛이 가진 스킬 런타임도 함께 폐기된다.

`EffectManager`는 계속 범용 생성·제거 관리자여야 한다. 어떤 스킬인지 판정하거나 강화 효과를 계산하거나 `SkillModel`을 소유하지 않는다.

### 3.2 유닛 GameObject에 무엇을 붙이는가

`MonsterModel`과 `EnemyModel`을 MonoBehaviour로 만들어 GameObject에 붙이지 않는다. 두 모델은 Unity 생명주기와 분리한 순수 C# 런타임 객체로 만든다.

유닛 GameObject에는 다음과 같은 연결용 MonoBehaviour를 붙인다.

- `MonsterActor` 또는 최종 명칭 `MonsterView`
- `EnemyActor` 또는 최종 명칭 `EnemyView`
- 공통 표시가 필요하면 `UnitView`

이 컴포넌트는 Factory/Spawner가 만든 모델을 `Initialize(model)`로 주입받는다. 책임은 Transform, Animator, Collider, 피해 숫자, 사망 표시 등 Unity 표현과 모델 연결이다. 체력 계산, 스킬 학습, 쿨타임, 강화 적용은 모델이 담당한다.

이 결정은 현재 `MonsterActor.Initialize(UnitCombatState)`와 `EnemyActor.Initialize(EnemyCombatState)`에서 이미 확인되는 주입 방식을 유지하면서 모델 책임을 강화하는 방향이다.

## 4. 최종 책임 구조

```text
UnitBaseModel
├─ MonsterModel
│  ├─ MonsterSkillProgression
│  └─ SkillCollection<SkillModel>
├─ EnemyModel
│  ├─ EnemySkillLoadout
│  └─ SkillCollection<SkillModel>
└─ NexusModel (Nexus가 같은 전투 규칙을 사용할 경우)

SkillModel
├─ ActiveSkillModel
│  ├─ ProjectileSkillModel
│  ├─ LineSkillModel
│  ├─ SingleAttackSkillModel
│  ├─ AreaAttackSkillModel
│  ├─ BuffSkillModel
│  ├─ HealSkillModel
│  └─ ShieldSkillModel
├─ TriggerSkillModel
└─ PassiveSkillModel
```

### 4.1 `UnitBaseModel`

공통 유닛 상태와 규칙을 가진다.

- 체력, 공격력, 방어력, 공격 속도 등 공통 전투 능력치
- 생존 여부와 피해·회복 처리
- 공통 상태 효과 컨테이너
- 유닛 식별 정보
- 공통 전투 이벤트 발행

스킬의 구체 효과를 구현하지 않는다.

### 4.2 `MonsterModel`

- Monster 전용 학습 진행도 소유
- 학습한 Base 스킬, 패시브, 강화, 마스터 선택 소유
- 정의 저장소에서 전달받은 정의를 이용해 자신의 `SkillModel` 생성·강화
- 자신의 스킬 컬렉션에서 사용 가능 스킬 조회

### 4.3 `EnemyModel`

- Enemy 전용 스킬 구성 소유
- Enemy 전용 패시브와 트리거 소유
- 자신의 스킬 컬렉션에서 사용 가능 여부 판단

Monster와 Enemy의 차이는 별도 중앙 실행기의 분기문이 아니라 각 모델에 주입되는 Loadout/Progression과 실제 `SkillModel` 구성으로 표현한다.

### 4.4 `SkillModel`

각 유닛이 개별 인스턴스로 소유한다.

- 공유 불변 `BaseSkillDefinition` 참조
- 현재 레벨
- 해당 유닛이 선택한 Enhancement/Master
- 현재 쿨타임
- 충전 수, 탄창, 연속 적중 등 해당 스킬에 실제로 필요한 런타임 상태
- `CanUse(context)`
- 실행 예약, 성공 확정, 실패 취소
- `ResolvedSkillSpec` 생성 또는 캐시 무효화

모든 종류에 필요하지 않은 상태를 Base에 넣지 않는다. 예를 들어 투사체 발사 횟수는 `ProjectileSkillModel` 또는 한 번의 투사체 실행 세션이 소유한다.

### 4.5 정의와 강화

정의 객체는 공유 가능한 불변 데이터다.

- `BaseSkillDefinition`
- 종류별 정의: Projectile, Line, SingleAttack, AreaAttack, Buff, Passive 등
- `EnhancementDefinition`
- `MasterEffectDefinition`
- `TriggerDefinition`
- `PassiveDefinition`

강화와 마스터는 공유 정의를 직접 변경하지 않는다. 다음 수정자를 통해 해당 유닛의 결과에만 반영한다.

- `ISkillModifier`: 피해, 범위, 횟수, 쿨타임 등 스킬 수정
- `IUnitModifier`: 체력, 공격력, 방어력 등 유닛 수정
- `ITriggerModifier`: 이벤트 반응 조건·후속 행동 수정

최종 실행 값은 `ResolvedSkillSpec`으로 만든다. 이 객체는 한 번의 실행 동안 변경하지 않는다.

### 4.6 패시브와 트리거

- `PassiveSkillModel`은 유닛 또는 보유 스킬에 Modifier를 제공한다.
- `TriggerSkillModel`은 소유 유닛의 전투 이벤트를 구독하고 조건 충족 시 자신의 스킬 실행을 요청한다.
- 패시브와 트리거의 런타임 상태도 중앙 싱글턴이 아니라 소유 유닛에 귀속한다.
- 비주얼이 없는 패시브·트리거는 GameObject나 Actor를 만들지 않는다.

## 5. 스킬 실행 흐름

```text
AI / 입력 / Trigger
        │
        ▼
MonsterModel 또는 EnemyModel에서 SkillModel 조회
        │
        ▼
SkillModel.CanUse(context)
        │
        ▼
SkillModel이 실행 상태 예약 + ResolvedSkillSpec 확정
        │
        ▼
SkillRunner가 종류별 ISkillExecutor 선택
        │
        ▼
Executor가 수치 적용 또는 EffectManager에 비주얼 생성 요청
        │
        ▼
필요한 비주얼 GameObject에 경량 SkillActor 초기화
        │
        ▼
성공: SkillModel.CommitUse / 실패: SkillModel.RollbackUse
```

`SkillRunner`의 최종 책임은 다음으로 제한한다.

- 실행 요청 수신
- 소유 유닛과 스킬의 유효성 확인
- `CanUse` 결과 확인
- 종류에 맞는 Executor 호출
- 성공 또는 실패를 `SkillModel`에 확정

`SkillRunner`는 강화 효과, 발사 횟수, 명중 횟수, 쿨타임 원본 상태를 소유하지 않는다. 스킬 효과를 새로 만들지도 않는다.

## 6. 최종 폴더 구조

기존 두 폴더 안에 새 구조를 만들지 않는다. 다음처럼 독립된 새 루트로 옮긴다.

```text
Pakuri/Assets/Scripts/
├─ Combatants/
│  ├─ Definitions/
│  │  ├─ UnitDefinition.cs
│  │  ├─ MonsterDefinition.cs
│  │  └─ EnemyDefinition.cs
│  ├─ Models/
│  │  ├─ UnitBaseModel.cs
│  │  ├─ MonsterModel.cs
│  │  ├─ EnemyModel.cs
│  │  └─ NexusModel.cs
│  ├─ Skills/
│  │  ├─ SkillCollection.cs
│  │  ├─ MonsterSkillProgression.cs
│  │  └─ EnemySkillLoadout.cs
│  ├─ Factory/
│  │  └─ CombatantModelFactory.cs
│  └─ Presentation/
│     ├─ UnitView.cs
│     ├─ MonsterActor.cs
│     └─ EnemyActor.cs
│
├─ Abilities/
│  ├─ Definitions/
│  │  ├─ BaseSkillDefinition.cs
│  │  ├─ Active/
│  │  ├─ Passive/
│  │  ├─ Trigger/
│  │  └─ Upgrades/
│  ├─ Models/
│  │  ├─ SkillModel.cs
│  │  ├─ Active/
│  │  ├─ PassiveSkillModel.cs
│  │  └─ TriggerSkillModel.cs
│  ├─ Modifiers/
│  │  ├─ ISkillModifier.cs
│  │  ├─ IUnitModifier.cs
│  │  └─ ITriggerModifier.cs
│  ├─ Resolution/
│  │  ├─ SkillSpecResolver.cs
│  │  └─ ResolvedSkillSpec.cs
│  ├─ Execution/
│  │  ├─ SkillRunner.cs
│  │  ├─ ISkillExecutor.cs
│  │  └─ Executors/
│  └─ Presentation/
│     ├─ ProjectileSkillActor.cs
│     ├─ LineSkillActor.cs
│     ├─ AreaSkillActor.cs
│     └─ FollowingEffectActor.cs
│
└─ Combat/
   └─ Effects/
      └─ EffectManager.cs
```

`Combat/Effects/EffectManager.cs`는 두 교체 대상 폴더 밖에 있고 현재도 범용 생성·제거 역할이므로, 스킬 도메인 책임을 추가하지 않고 유지한다. 필요한 수정은 새 실행 데이터 타입을 받는 참조 변경 수준으로 제한한다.

## 7. 두 기존 폴더 밖의 코드 변경 제한

다음 원칙을 구현의 강제 조건으로 둔다.

### 허용

- `using` 및 namespace 변경
- 이전 타입명을 새 타입명으로 변경
- 생성자와 Factory 호출 대상 변경
- 이전 프로퍼티 접근을 의미가 같은 새 공개 API 호출로 변경
- 이동한 MonoBehaviour 스크립트의 `.meta`를 함께 이동하여 Unity GUID 유지

### 원칙적으로 금지

- 외부 폴더에 스킬 효과 계산 로직 추가
- 외부 폴더에 쿨타임·학습·강화 상태 저장
- 외부 폴더에서 스킬 종류별 대형 분기 추가
- Prefab/Scene 구조 변경
- `EffectManager`에 스킬 정의 조회나 강화 계산 추가

현재 `UnitCombatStateFactory.cs`처럼 두 대상 폴더 밖에서 기존 내부 API를 직접 조립하는 코드는 단순 이름 변경만으로 전환되지 않을 가능성이 있다. 이 경우 새 `CombatantModelFactory` 또는 호환 Facade에 조립 책임을 넣고, 외부 파일은 그 Facade를 호출하도록 최소 변경한다. 외부 파일에 새 스킬 로직을 복제하지 않는다.

Prefab/Scene가 참조하는 MonoBehaviour 파일을 이동할 때는 반드시 기존 `.meta`를 같이 이동해 GUID를 보존한다. 이 방법으로 Prefab/Scene 수정을 피하는 것을 우선한다.

## 8. 단계별 이전 계획

### 1단계: 참조 조사와 경계 고정

- 두 기존 폴더의 모든 C# 타입과 외부 참조 위치를 실제 검색 결과로 목록화한다.
- Prefab/Scene가 참조하는 MonoBehaviour와 `.meta` GUID를 확인한다.
- 외부 변경 예상 파일 목록을 먼저 확정한다.

### 2단계: 새 정의와 모델 구축

- `Combatants`와 `Abilities` 새 루트를 만든다.
- `UnitBaseModel`, `MonsterModel`, `EnemyModel`을 만든다.
- `SkillModel` 계층과 정의, Modifier, `ResolvedSkillSpec`을 만든다.
- 이 단계에서는 기존 런타임을 제거하지 않고 단위 테스트로 새 모델 규칙을 검증한다.

### 3단계: 유닛별 스킬 소유 전환

- Monster 학습 정보를 `MonsterSkillProgression`으로 옮긴다.
- Enemy 스킬 구성을 `EnemySkillLoadout`으로 옮긴다.
- Factory/Assembler가 정의를 조회한 뒤 실제 `SkillModel` 인스턴스를 각 유닛 모델에 주입한다.
- 모델 내부에서 전역 Catalog를 직접 조회하지 않는다.

### 4단계: 실행기와 Actor 전환

- `SkillRunner`는 검증·Executor 호출·결과 확정만 담당하게 만든다.
- 기존 종류별 Executor에서 계산과 상태 소유를 각 SkillModel/ResolvedSpec으로 옮긴다.
- 비주얼 GameObject에는 경량 Actor만 붙인다.
- 발사 횟수, 명중 횟수, 연속 적중 등은 해당 SkillModel 또는 1회 실행 세션이 소유하게 한다.

### 5단계: 외부 참조 교체

- 두 대상 폴더 밖의 코드는 새 타입·Facade 참조로 바꾼다.
- 외부 파일 변경이 단순 참조 변경을 넘는 경우 이유와 대안을 먼저 기록한다.
- Prefab/Scene는 GUID 보존으로 무수정을 우선한다.

### 6단계: 구 구조 제거

- 외부 참조가 0인지 검색한다.
- 필요한 파일과 `.meta`를 새 위치로 이동 완료한다.
- 다음 폴더와 폴더 `.meta`를 제거한다.
  - `Pakuri/Assets/Scripts/Combat/Skills`
  - `Pakuri/Assets/Scripts/Units`
- 호환용 임시 클래스, 중복 정의, 사용하지 않는 중앙 실행 상태를 제거한다.

## 9. 완료 조건

다음을 모두 만족해야 완료다.

- `UnitBaseModel`을 `MonsterModel`, `EnemyModel`이 상속한다.
- 각 Monster/Enemy 모델이 자신의 `SkillModel` 인스턴스를 보유한다.
- 쿨타임과 스킬별 런타임 상태는 해당 `SkillModel`이 판단·보유한다.
- 강화·마스터는 공유 정의를 변경하지 않고 해당 유닛의 스킬 결과에만 반영된다.
- `SkillRunner`는 검증, Executor 선택·실행, 성공/실패 확정만 수행한다.
- `EffectManager`는 비주얼 생성·등록·제거만 담당한다.
- 스킬 비주얼 GameObject에는 경량 Actor만 붙고, 전체 SkillModel은 붙지 않는다.
- 유닛 GameObject에는 Actor/View만 붙고, 순수 MonsterModel/EnemyModel은 주입된다.
- 두 대상 폴더 밖의 변경은 사전에 확정된 참조·호출 교체 또는 불가피한 최소 Adapter 변경뿐이다.
- Prefab/Scene 변경 없이 MonoBehaviour GUID가 유지된다. 실제 조사로 불가능한 경우 별도 승인 전 작업을 멈춘다.
- 컴파일 오류가 없다.
- 관련 Edit Mode 테스트가 통과한다.
- Unity Console 오류가 없다.
- 사용자가 수행하는 Play Mode 검증에서 스폰, 공격, 스킬, 패시브, 강화, 사망·재생성 흐름이 정상이다.
- 아래 경로 검사 결과가 모두 `False`다.

```powershell
Test-Path -LiteralPath 'Pakuri/Assets/Scripts/Combat/Skills'
Test-Path -LiteralPath 'Pakuri/Assets/Scripts/Combat/Skills.meta'
Test-Path -LiteralPath 'Pakuri/Assets/Scripts/Units'
Test-Path -LiteralPath 'Pakuri/Assets/Scripts/Units.meta'
```

## 10. 핵심 결론

- 스킬은 비주얼 GameObject가 관리하지 않는다. 유닛 모델이 소유한 `SkillModel`이 관리한다.
- 비주얼 GameObject는 필요한 종류별 Actor를 붙여 이동·충돌·표시·수명만 처리한다.
- 유닛 GameObject는 `MonsterModel`/`EnemyModel` 자체를 컴포넌트로 붙이지 않고 Actor/View를 통해 순수 모델과 연결한다.
- 중앙 실행기가 모든 효과와 상태를 보유하는 구조를 없애고, 각 유닛과 각 스킬이 자기 상태를 소유한다.
- 최종 단계에서 `Combat/Skills`와 `Units`는 완전히 없어져야 한다.
