# Scripts 역할 요약

이 문서는 `Pakuri/Assets/Scripts` 아래의 C# 스크립트 69개를 다른 사람에게 짧게 설명하기 위한 자료다.
각 스크립트는 현재 코드의 상단 역할 주석, 선언 타입, 공개·내부 메서드를 근거로 `역할`, `처리`, `결과` 세 줄로 정리한다.

## Combat

### Damage/DamageCalculator.cs

- 역할: 공격자의 능력치와 스킬 수치로 원본 피해와 보호막을 계산한다.
- 처리: 방어력, 속성, 상태 효과, 치명타와 최종 피해 배율을 순서대로 반영한다.
- 결과: 전투 관리자가 대상 자원에 적용할 최종 피해량을 반환한다.

### Effects/EffectManager.cs

- 역할: 전투에서 사용하는 스킬·상태 효과의 시각 오브젝트를 관리한다.
- 처리: 프리팹이나 런타임 비주얼을 생성하고 대상 부착, 갱신, 수명과 제거를 처리한다.
- 결과: 전투 효과의 생성부터 종료까지 한곳에서 제어한다.

### Effects/EffectVisualBuilder.cs

- 역할: EffectManager가 만든 오브젝트의 실제 외형과 충돌 영역을 구성한다.
- 처리: 스프라이트, 애니메이터, 방향, 크기, 범위와 유지 시간을 적용한다.
- 결과: 런타임 효과 오브젝트를 스킬 설정에 맞는 모습으로 완성한다.

### InGameCombatManager.cs

- 역할: 등록된 유닛을 기준으로 인게임 전투 시스템의 실행 순서를 조율한다.
- 처리: 피해, 회복, 상태 변화, 스킬, Trigger, 패시브와 사망 처리를 연결한다.
- 결과: 전투 결과를 유닛 모델, 액터 표시와 시각 효과에 전달한다.

## Combat/Skills/Choices

### SkillNode.cs

- 역할: 스킬 실행에 사용하는 조건, 수치 변경, 효과와 Trigger 노드 형식을 정의한다.
- 처리: 원본 스킬과 선택지 노드를 하나의 `SkillNodePlan`으로 조합한다.
- 결과: 모든 스킬 실행기가 공유할 정규화된 실행 계획을 제공한다.

### SkillSnapshot.cs

- 역할: 원본 스킬과 선택된 강화 결과를 한 번의 실행값으로 모은다.
- 처리: 선택지 노드에 따라 피해, 범위, 상태, 투사체와 조건부 보너스를 누적한다.
- 결과: 실행기가 읽을 최종 수치와 `SkillNodePlan`을 제공한다.

### SkillUpgrade.cs

- 역할: 유닛이 선택한 강화와 마스터 노드를 현재 스킬에 적용한다.
- 처리: 학습 상태와 Choice를 확인해 `SkillSnapshot`에 반영한다.
- 결과: 실행기가 Choice 원본을 다시 해석하지 않아도 되는 완성된 스냅샷을 반환한다.

## Combat/Skills/Definitions

### SkillDefinition.cs

- 역할: 작성 데이터에서 사용하는 액티브·패시브 스킬 정의 형식을 제공한다.
- 처리: 슬롯, 런타임 종류, 대상, 피해, 상태, 효과, Choice와 노드 정의를 보관한다.
- 결과: 로딩과 런타임 컴파일 단계가 사용할 스킬 원본 계약을 제공한다.

## Combat/Skills/Execution

### SkillEffect.cs

- 역할: 스킬에 연결된 피해·상태·지속시간 효과를 대상에게 적용한다.
- 처리: 시전, 적중, 적중 횟수, 만료 같은 실행 시점을 구분하고 대상 조건을 확인한다.
- 결과: 조건에 맞는 직접 효과와 지속 패시브 상태를 전투 시스템에 전달한다.

### SkillExecution.cs

- 역할: 모든 유닛의 스킬 상태 갱신과 실행 요청을 한곳에서 라우팅한다.
- 처리: 자동·수동·Trigger 요청에 Snapshot을 만들고 스킬 종류에 맞는 실행기를 선택한다.
- 결과: 선택된 스킬 실행과 시전 Trigger를 일관된 순서로 연결한다.

### SkillTargeting.cs

- 역할: 스킬이 공격하거나 지원할 대상과 범위 중심을 계산한다.
- 처리: 진영, 선택 방식, 거리, 형태와 Snapshot 범위 보정을 적용한다.
- 결과: 실행기가 사용할 정렬된 대상 목록, 방향과 범위 좌표를 반환한다.

## Combat/Skills/Runtime

### SkillRuntime.cs

- 역할: 컴파일된 스킬 하나의 전투 중 변경 상태를 관리한다.
- 처리: 쿨다운, 탄창, 재장전, Tick, 연속 발사와 적중 횟수를 갱신한다.
- 결과: 현재 Snapshot 기준의 시전 가능 여부와 실행 순서를 제공한다.

### SkillRuntimeCompiler.cs

- 역할: 작성된 액티브·패시브 스킬 정의를 전투용 데이터로 변환한다.
- 처리: 공통 필드와 스킬 종류별 필드를 옮기고 노드 종류와 파라미터를 해석한다.
- 결과: `SkillRuntimeData`, 패시브 데이터와 실행 가능한 `SkillNode`를 만든다.

### SkillRuntimeData.cs

- 역할: 전투에서 직접 사용하는 스킬 런타임 데이터 형식을 정의한다.
- 처리: 타이밍, 대상, 피해, 상태, 투사체, 범위, 버프와 종류별 세부값을 보관한다.
- 결과: 각 스킬 실행기가 공통 계약으로 읽을 데이터 구조를 제공한다.

## Combat/Skills/SkillType

### Buff/BuffSkillExecutors.cs

- 역할: 일반 버프, 보호막과 회복 스킬을 종류에 맞게 실행한다.
- 처리: 설정된 아군 대상을 구하고 버프 종류별 전용 실행기로 전달한다.
- 결과: 대상에게 상태, 보호막 또는 회복 결과를 적용한다.

### Line/LineSkillActor.cs

- 역할: 생성된 직선 공격 오브젝트의 위치, 충돌과 수명을 관리한다.
- 처리: 방향과 피해 정보를 초기화하고 충돌 범위 안의 대상에게 Tick 효과를 적용한다.
- 결과: 월드에 존재하는 직선 공격의 적중 결과를 전투 시스템에 전달한다.

### Line/LineSkillExecutor.cs

- 역할: Line 형식 스킬의 시전 과정을 시작한다.
- 처리: 대상, 방향, 피해와 시각 오브젝트를 구성해 LineSkillActor를 초기화한다.
- 결과: 전장에 실제 직선 공격을 생성하고 시전을 확정한다.

### Passive/PassiveSkill.cs

- 역할: 전투 사건에 따라 학습된 패시브 효과를 갱신한다.
- 처리: 로스터, 체력, 보호막과 상태 변경을 모아 지속 상태와 일회성 효과를 처리한다.
- 결과: 패시브 상태 적용·제거와 Trigger 쿨다운·횟수를 전투 단위로 유지한다.

### Projectile/ProjectileSkillActor.cs

- 역할: 생성된 투사체의 이동, 충돌과 수명 주기를 관리한다.
- 처리: 대상과 피해·상태·분기 정보를 받아 이동하고 적중 대상을 판정한다.
- 결과: 투사체 적중 피해와 후속 효과를 전투 시스템에 전달한다.

### Projectile/ProjectileSkillExecutor.cs

- 역할: 투사체 스킬의 발사 구성을 만든다.
- 처리: 발사 수, 연속 발사, 분기, 후속 투사체와 발사체별 보정을 Snapshot에서 해석한다.
- 결과: 설정이 완료된 ProjectileSkillActor 또는 직접 적중 결과를 생성한다.

### Single/SingleChargeActor.cs

- 역할: 돌진 스킬이 진행되는 동안 시전 유닛의 이동과 접촉을 처리한다.
- 처리: 저장된 돌진 상태로 목표를 추적하고 접촉한 대상에게 피해와 상태를 적용한다.
- 결과: 돌진 완료 여부와 실제 적중 결과를 실행기에 반환한다.

### Single/SingleChargeState.cs

- 역할: 진행 중인 단일 대상 돌진 스킬의 실행 상태를 보관한다.
- 처리: 목표, 이동, 피해, 상태와 스킬 실행에 필요한 값을 한 객체에 유지한다.
- 결과: SingleChargeActor가 매 프레임 이어서 처리할 돌진 정보를 제공한다.

### Single/SingleSkillExecutor.cs

- 역할: 단일 대상, 연쇄와 돌진형 스킬의 실행 순서를 담당한다.
- 처리: 대상 선정, 피해 계산, 상태 소모, 후속 공격과 처치 결과를 처리한다.
- 결과: 스킬 요청의 라우팅 여부와 시전 확정 결과를 반환한다.

### Single/SingleSkillRules.cs

- 역할: 단일 공격에서 공통으로 쓰는 조건과 수치 규칙을 계산한다.
- 처리: 처형 체력 기준, 보스·처형 피해 보정, 치명타와 처치 후 쿨다운 행동을 적용한다.
- 결과: SingleSkillExecutor가 사용할 최종 공격 보정과 처치 후 결과를 제공한다.

### Trigger/SkillTrigger.cs

- 역할: 전투 사건을 스킬 Trigger 조건과 연결한다.
- 처리: 시전, 적중, 피해, 처치, 보호막과 상태 만료 사건에 맞는 Trigger를 찾는다.
- 결과: 조건을 만족한 Trigger 효과나 연결 스킬 실행을 요청한다.

### Zone/ZoneSkillActor.cs

- 역할: 생성된 지속 범위 스킬의 위치, 충돌과 수명을 관리한다.
- 처리: 범위 안 대상을 반복해서 찾고 Tick마다 피해와 상태 효과를 적용한다.
- 결과: 전장에 남아 있는 범위 스킬의 지속 적중 결과를 만든다.

### Zone/ZoneSkillExecutor.cs

- 역할: 지속 범위 스킬의 최초 시전과 재시전을 실행한다.
- 처리: 범위 중심, 크기, 피해, 상태와 시각 오브젝트를 구성한다.
- 결과: ZoneSkillActor를 생성하거나 기존 효과 위치에서 재시전 결과를 만든다.

## Combat/Status

### StatusEffectDefinition.cs

- 역할: 작성용 상태 정의와 전투용 상태 데이터 형식을 함께 정의한다.
- 처리: 상태 종류, 중첩, 지속시간, 행동 제한, 능력치 변경과 조건 데이터를 보관한다.
- 결과: 로딩·컴파일·전투 상태 시스템이 공유할 상태 효과 계약을 제공한다.

### StatusRules.cs

- 역할: 상태 효과의 적용과 전투 수치 반영 규칙을 담당한다.
- 처리: 확률·지속시간·중첩을 계산하고 이동, 행동, 공격력과 피해 보정을 해석한다.
- 결과: 상태 적용 성공 여부와 유닛의 최종 전투 보정값을 반환한다.

### StatusRuntimeCompiler.cs

- 역할: 검증된 상태 ID와 스킬 설정을 전투용 상태 데이터로 변환한다.
- 처리: 상태 종류, 복수 상태, 조건식, 대상 범위와 병합 정책을 파싱한다.
- 결과: 스킬 효과와 Trigger가 바로 사용할 `StatusRuntimeData`를 만든다.

### StatusState.cs

- 역할: 유닛이 현재 보유한 상태 효과와 보호막 상태를 관리한다.
- 처리: 상태 적용, 갱신, 중첩 소모, 시간 감소, 제거와 보호막 흡수를 수행한다.
- 결과: 현재 상태 목록, 중첩 수와 피해 흡수 결과를 제공한다.

## Data

### CsvDataValidator.cs

- 역할: CSV 파싱 직후 원본 데이터와 참조 관계를 한 번 검증한다.
- 처리: 필수값, 스킬 수치, ID 연결, 그래프, Trigger와 Unity 자산 경로를 검사한다.
- 결과: 오류 목록을 제공하고 잘못된 데이터가 전투로 넘어가는 것을 막는다.

### CsvParser.cs

- 역할: CSV 텍스트를 표와 행으로 분리하고 셀 값을 읽는다.
- 처리: 헤더와 자료형을 확인하고 문자열, 숫자, 불리언과 Enum 값을 변환한다.
- 결과: CsvRowParser가 사용하는 `CsvTable`과 `CsvRecord`를 제공한다.

### CsvRowParser.cs

- 역할: authoring CSV의 각 행을 카탈로그 생성 전 중간 행 데이터로 변환한다.
- 처리: 몬스터, 보상, 스킬, Choice, Trigger와 적 스킬 열을 종류별로 읽는다.
- 결과: `SourceModel`에 저장할 타입별 Row 객체를 만든다.

### CsvSourceModel.cs

- 역할: 파싱된 CSV 행 형식과 전체 원본 데이터 모음을 정의한다.
- 처리: 몬스터, 스킬, Choice, 노드, 상태와 적 데이터를 ID별 컬렉션에 보관한다.
- 결과: 검증기와 GameDataCatalogBuilder가 공유할 중간 데이터 모델을 제공한다.

### SkillGraphParser.cs

- 역할: 스킬 노드와 실행 그래프 CSV를 읽고 정규화한다.
- 처리: 노드 종류, 소유자, 값 형식, 파라미터 스키마와 그래프 참조를 검사한다.
- 결과: 실행 순서가 정리된 노드 행을 `SourceModel`에 저장한다.

## GameFlow/Loading

### CsvCatalogEditor.cs

- 역할: Unity Editor에서 authoring CSV를 런타임 카탈로그 자산으로 동기화한다.
- 처리: CSV TextAsset과 Sprite, Prefab, Animator 참조를 수집하고 검증한다.
- 결과: Editor에서 사용할 최신 CsvRuntimeCatalog 자산을 생성하거나 갱신한다.

### CsvRuntimeCatalog.cs

- 역할: 런타임 CSV 원본과 Unity 자산 참조를 함께 보관한다.
- 처리: Sprite, Prefab과 Animator 경로를 조회표로 구성한다.
- 결과: 로더와 카탈로그 빌더가 경로로 Unity 자산을 찾을 수 있게 한다.

### GameDataCatalog.cs

- 역할: 게임에서 사용하는 모든 런타임 정의와 ID 조회표를 제공한다.
- 처리: 몬스터, 적, 상태, 스킬, 패시브와 보상 정의를 등록하고 조회표를 재구성한다.
- 결과: 다른 시스템이 ID나 타입으로 필요한 게임 데이터를 찾게 한다.

### GameDataCatalogBuilder.cs

- 역할: 검증된 SourceModel을 실제 게임용 GameDataCatalog로 변환한다.
- 처리: 몬스터, 적, 상태, 스킬, Choice, 효과, Trigger와 Unity 자산을 연결한다.
- 결과: 전투와 게임 진행이 직접 사용할 완성된 런타임 정의를 만든다.

### GameDataLoader.cs

- 역할: Scene 로드 전에 CSV 런타임 데이터를 한 번 초기화한다.
- 처리: 원본을 파싱하고 검증한 뒤 GameDataCatalog를 만들고 조회 대상으로 등록한다.
- 결과: 이후 Scene과 전투 시스템에 검증 완료된 전역 게임 데이터를 제공한다.

## GameFlow

### RunSession.cs

- 역할: 한 번의 Run 동안 유지되는 진행 상태와 몬스터별 성장 상태를 보관한다.
- 처리: 스테이지, 날짜, 재화, 포로, 파티와 학습한 스킬·Choice를 기록한다.
- 결과: 보상 적용과 다음 날짜 진행에 필요한 영속적인 Run 상태를 제공한다.

## GameFlow/Spawn

### UnitCombatStateFactory.cs

- 역할: 정의 데이터와 Run 상태로 아군, 적과 Nexus 전투 모델을 만든다.
- 처리: 기본 능력치, 방어력, 체력, 역할과 성장 상태를 `UnitCombatState`에 복사한다.
- 결과: SpawnManager가 Actor에 연결할 완성된 전투 모델을 반환한다.

### UnitSpawnManager.cs

- 역할: RunSession의 파티와 스테이지 적을 실제 전투 유닛으로 생성한다.
- 처리: 모델 생성, 프리팹 인스턴스화, Actor 초기화와 로스터 등록을 순서대로 연결한다.
- 결과: 선택 몬스터, 현현 파티, 적과 Nexus를 전장에 배치한다.

## GameFlow/Stage

### MonsterDayRecovery.cs

- 역할: 하루가 끝난 몬스터를 다음 전투에 사용할 상태로 회복한다.
- 처리: 임시 상태, 보호막, 스킬 실행 상태를 지우고 체력과 자동 행동 설정을 복구한다.
- 결과: 다음 날짜에 재사용할 정리된 몬스터 전투 모델을 만든다.

### StageManager.cs

- 역할: 현재 Run의 날짜별 전투와 보상 진행 순서를 관리한다.
- 처리: 스테이지 표를 읽어 적 생성, 전투 종료 대기, 보상과 다음 날짜 진행을 수행한다.
- 결과: 날짜 전환, Nexus 체력 보존과 최종 승패 화면까지 이어 준다.

## UI/InGame/DamageMeter

### DamageMeterRuntimeTracker.cs

- 역할: 전투 피해 이벤트를 몬스터와 스킬별로 누적 기록한다.
- 처리: 피해 출처 ID별 합계와 몬스터 전체 피해량을 갱신한다.
- 결과: 피해량 UI가 조회할 런타임 기록을 제공한다.

### DamageMeterUIController.cs

- 역할: 누적 피해 기록을 파티원별 피해량 패널로 표시한다.
- 처리: 파티 순서, 피해량, 스킬 이름과 비율을 계산해 UI 항목을 갱신한다.
- 결과: 플레이어가 몬스터와 스킬별 전투 기여도를 확인하게 한다.

## UI/InGame

### DebugUI.cs

- 역할: 선택 몬스터의 스킬 학습과 Choice 적용을 직접 시험한다.
- 처리: 카탈로그로 버튼을 만들고 선택 결과를 RunSession과 전투 모델에 동기화한다.
- 결과: 변경된 학습 상태로 런타임 스킬을 즉시 다시 구성한다.

### InGameUIManager.cs

- 역할: 인게임 진행 표시, 보상 선택과 포로 현현 화면을 연결한다.
- 처리: 스테이지·재화·파티 UI를 갱신하고 Offering과 Menifest 결과를 적용한다.
- 결과: UI 선택을 RunSession과 현재 전투 모델의 실제 진행 상태로 반영한다.

### MonsterPanel/MonsterPanelUI.cs

- 역할: 전투 중 파티 몬스터의 초상화, 체력과 액티브 스킬 상태를 표시한다.
- 처리: 파티 모델과 스킬 런타임을 패널 슬롯에 연결하고 표시값을 갱신한다.
- 결과: 각 파티원의 생존 상태와 스킬 사용 가능 상태를 보여 준다.

### Nexus/NexusHealthDisplay.cs

- 역할: Nexus의 현재 체력과 최대 체력을 UI 문자열로 표시한다.
- 처리: Nexus 전투 모델의 자원 값을 읽어 TMP 텍스트를 갱신한다.
- 결과: 화면에 최신 Nexus 체력 정보를 제공한다.

### UtilityPanel/InGameUtilityPanelController.cs

- 역할: 자동 스킬 사용과 전투 배속 버튼을 제어한다.
- 처리: 버튼 입력을 플레이어 자동 전투 설정과 게임 시간 배율에 반영한다.
- 결과: 플레이어가 자동 사용 여부와 전투 속도를 바꿀 수 있게 한다.

## UI/MainMenu

### MainMenuUIManager.cs

- 역할: 인트로, 메인 메뉴와 몬스터 선택 화면을 전환한다.
- 처리: UI 버튼을 연결하고 선택 몬스터를 StartContext에 기록한 뒤 Scene을 불러온다.
- 결과: 선택한 몬스터 정보로 새로운 Run을 시작한다.

## Units/Collision

### UnitHitboxOverlap.cs

- 역할: 두 전투 유닛의 실제 Collider가 겹치는지 검사한다.
- 처리: 공격자의 충돌체 목록과 대상의 충돌체를 비교한다.
- 결과: 접촉형 공격이 대상을 적중했는지 반환한다.

## Units/Definitions

### EnemyDefinition.cs

- 역할: CSV에서 구성되는 적 유닛의 원본 정의 형식을 제공한다.
- 처리: 능력치, 방어력, 공격 속성, 액티브 스킬, Trigger와 패시브를 보관한다.
- 결과: 적 전투 모델과 런타임 스킬을 생성할 정의 데이터를 제공한다.

### MonsterDefinition.cs

- 역할: CSV에서 구성되는 플레이어 몬스터의 원본 정의를 보관한다.
- 처리: 능력치, 외형, 초기 보상, 액티브·패시브 스킬과 Trigger를 연결한다.
- 결과: RunSession과 플레이어 전투 모델 생성에 필요한 데이터를 제공한다.

## Units/Enemy

### Actor/EnemyActor.cs

- 역할: 적 GameObject와 EnemyCombatState를 연결한다.
- 처리: 초기화, 피해 숫자와 월드 표시 갱신을 UnitWorldDisplay에 전달한다.
- 결과: 적 모델의 상태를 Scene 오브젝트에 표현한다.

### AI/EnemyActionController.cs

- 역할: 등록된 적들의 매 프레임 행동 순서를 조율한다.
- 처리: 상태에 따른 행동 가능 여부를 확인하고 이동과 선택된 스킬 실행을 연결한다.
- 결과: 각 적이 현재 전투 상황에 맞게 이동하거나 스킬을 사용하게 한다.

### AI/EnemyCombatDecision.cs

- 역할: 적이 공격할 대상과 사용할 스킬을 결정한다.
- 처리: 가까운 플레이어, 낮은 체력의 아군과 실행 가능한 공격·지원 스킬을 찾는다.
- 결과: EnemyActionController가 실행할 대상과 스킬 선택 결과를 반환한다.

### Passive/EnemyPassiveModifiers.cs

- 역할: 적 정의에 작성된 패시브 능력치 보정을 전투 상태에 적용한다.
- 처리: 패시브 종류에 따라 적 능력치와 주는 피해 배율을 계산한다.
- 결과: 적 모델과 피해 계산기가 사용할 최종 패시브 보정값을 제공한다.

## Units/Model

### UnitCombatState.cs

- 역할: 모든 전투 유닛의 공통 모델과 적 전용 상태를 정의한다.
- 처리: 식별, 능력치, 자원, 방어력, 상태, 스킬 진행과 런타임 스킬을 보관한다.
- 결과: 전투 시스템 전체가 공유하는 유닛 상태의 기준 객체를 제공한다.

## Units/Monster

### Actor/MonsterActor.cs

- 역할: 아군 Monster GameObject와 전투 모델·애니메이션을 연결한다.
- 처리: 피해, 표시, 스킬·피격 애니메이션, 사망과 부활 상태를 갱신한다.
- 결과: 플레이어 몬스터 모델의 변화를 Scene 오브젝트에 표현한다.

### Input/PlayerCombatInputController.cs

- 역할: 선택 몬스터의 수동 스킬 입력과 자동 사용 모드를 처리한다.
- 처리: 포인터와 키 입력을 실행 요청으로 바꾸고 스킬·대상 상태를 확인한다.
- 결과: 유효한 입력을 SkillExecution에 전달하고 자동 전투 설정을 모델에 반영한다.

## Units/Nexus

### Actor/NexusActor.cs

- 역할: Nexus GameObject와 전투 상태를 연결한다.
- 처리: 모델을 초기화하고 현재 체력 변경을 월드 표시와 UI에 반영한다.
- 결과: Nexus 모델의 생존 상태를 Scene 오브젝트에 표시한다.

## Units/Presentation

### Animation/AnimationController.cs

- 역할: 몬스터 유닛의 Animator와 전투 애니메이션을 제어한다.
- 처리: 대기, 무작위 공격, 피격, 사망과 부활 애니메이션을 전환한다.
- 결과: 유닛의 현재 전투 상태에 맞는 애니메이션을 재생한다.

### DamageNumberPopup.cs

- 역할: 유닛 머리 위에 받은 피해량 숫자를 표시한다.
- 처리: 텍스트 오브젝트를 복제해 위로 이동시키고 시간에 따라 투명하게 만든다.
- 결과: 짧은 시간 동안 보이는 피해 숫자 팝업을 생성한다.

### UnitWorldDisplay.cs

- 역할: Enemy와 Monster Actor가 공유하는 월드 표시를 관리한다.
- 처리: 이름, 체력, 보호막, 상태 표시와 피해 숫자 참조를 찾아 갱신한다.
- 결과: 두 Actor의 중복 없이 유닛 모델 상태를 월드 UI에 표현한다.

## Units/Registry

### CombatUnitRegistry.cs

- 역할: 전투 유닛 모델과 Actor를 연결하고 아군·적 목록을 관리한다.
- 처리: 유닛 등록·해제·검색, Collider 연결, 표시 갱신과 사망 처리를 제공한다.
- 결과: 전투 시스템이 모든 유닛을 같은 방식으로 조회하고 제어하게 한다.
