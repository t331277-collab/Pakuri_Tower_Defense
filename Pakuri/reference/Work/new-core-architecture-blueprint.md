# New Core Architecture Blueprint

## Task title

새 Core 구조 청사진

## Goals

- 기존 코드 구조를 설계 기준으로 사용하지 않고 새로운 Core 구조를 정의한다.
- `Pakuri/Assets/CSVdata`와 각 CSV의 열 이름 및 용어를 그대로 유지한다.
- 게임 시작 시 CSV를 Definition으로 한 번 파싱한다.
- Definition, 런타임 Model, 스킬 학습 상태, 전투 실행, 스킬 생명주기, 시각 효과, 런 진행 책임을 분리한다.
- 각 몬스터가 자신의 스킬 학습 정도를 직접 관리하도록 한다.
- 현재 CSV, 프리팹, 씬, 스프라이트, 애니메이션, AnimatorController 리소스를 새 Core에 다시 연결한다.
- 최종 전환 뒤 `Pakuri/Assets/Scripts`의 이전 타입에 대한 런타임, 직렬화, 컴파일 의존을 0으로 만든다.

## Constraints

- 현재 문서는 청사진 초안이다.
- 기존 게임 코드, CSV, 프리팹, 씬에는 적용하지 않는다.
- 기존 구조는 새 구조의 설계 기준으로 사용하지 않는다.
- 실제 존재 여부와 유지할 CSV 계약은 검사한 파일을 근거로 한다.
- CSV Definition 계층에서는 CSV 열 이름을 임의로 바꾸지 않는다.
- 기존 `Pakuri/Assets/Scripts`의 폴더·클래스 분리는 새 구조의 설계 기준으로 복사하지 않는다.
- 사용자가 확정하지 않은 플레이 규칙은 기존 `Pakuri/Assets/Scripts`의 실제 동작을 호환성 근거로 검사한다.
- 기존 Scripts를 검사해도 의미가 하나로 확정되지 않으면 구현자가 임의로 정하지 않고 사용자에게 질문한다.
- 기존 Scripts는 전환 전 동작과 자산 연결을 확인하는 읽기 전용 근거일 뿐 새 코드의 호출 대상, 상속 대상, fallback 또는 호환성 계층이 아니다.
- 최종 구조는 기존 Scripts의 타입, namespace, Script GUID, 정적 상태, 런타임 객체를 참조하지 않는다.
- 이 완전 교체 작업의 진행 상태와 검증 근거는 이 문서의 Phase 실행 기록에만 남기고 `BLACKBOARD.md`와 `boards/**/*BLACKBOARD.md`에는 기록하지 않는다.
- 모든 Phase는 Unity 재컴파일과 Console 로그 확인을 통과해야 다음 Phase로 진행한다.
- Play Mode는 정적 검사와 비실행 검증으로 증명할 수 없는 동작에만 사용하며 사용자에게 실행 목적과 검증 시나리오를 먼저 제시한다.
- 근거 없는 클래스, 기능, 필드, 수치를 추가하지 않는다.

## Role Owner

Designer

## Status

Draft v0.6. 완전 교체 작업을 7개 Phase로 나누고, 청사진 내부 전용 기록, Phase별 Unity Console 확인, 최소 Play Mode 검증 규칙을 확정했다. 문서 기록만 완료했으며 구현은 시작하지 않았다.

## Next Actions

- 사용자 지시에 따라 새 Core 구성요소를 추가하거나 기존 초안을 수정한다.
- 각 클래스의 공개 API, 소유 상태, 입력, 출력, 금지 책임을 확정한다.
- Definition 간 ID 참조와 게임 시작 초기화 순서를 상세화한다.
- 구현 전 각 새 타입의 직접 소유자, 호출자, 상태 권위, 삭제 조건을 확정한다.
- 기존 Script GUID를 참조하는 모든 씬, 프리팹, `.asset`의 전환 목록과 새 소유자를 확정한다.
- 활성 리소스 재연결과 불필요한 Legacy 직렬화 자산 처리 방침을 구현 전에 확정한다.
- Phase 0부터 순서대로 진행하며 각 Phase의 종료 조건과 Unity Console 게이트를 통과한다.
- 구현 중 상태, 변경 경로, 로그 근거, Play Mode 여부는 이 문서의 Phase 실행 기록만 갱신한다.
- Code Builder handoff에서 공개 API와 로컬 검증 절차를 확정한다.
- 최종 청사진이 확정되기 전에는 코드에 적용하지 않는다.

## Evidence

- `Pakuri/Assets/CSVdata/authoring/monster/skills/base/`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/choices/`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/triggers/`
- `Pakuri/Assets/CSVdata/authoring/monster/monsters.csv`
- `Pakuri/Assets/CSVdata/authoring/enemy/enemies.csv`
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/`
- `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv`
- `Pakuri/Assets/CSVdata/stage_flow/StageDay.csv`
- `Pakuri/Assets/CSVdata/stage_flow/StageEncounter.csv`
- `Pakuri/Assets/CSVdata/stage_flow/StageReward.csv`
- `Pakuri/Assets/Scripts/GameFlow/Stage/MonsterDayRecovery.cs`
- `Pakuri/Assets/Scripts/UI/InGame/InGameUIManager.cs`
- `Pakuri/Assets/Scripts/GameFlow/RunSession.cs`
- `Pakuri/Assets/Scripts/Units/Monster/Input/PlayerCombatInputController.cs`
- `Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTargeting.cs`
- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyActionController.cs`
- `Pakuri/Assets/Scripts/Units/Enemy/AI/EnemyCombatDecision.cs`
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`
- `Pakuri/Assets/Scenes/NewScene/NewMainMenu.unity`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/CsvRuntimeCatalog.asset`
- `Pakuri/Assets/Legacy/Data/GameData/`
- `boards/UI/UI_BLACKBOARD.md`
- `boards/UI/RUNSCENE_UI.md`

## History

- 2026-07-23: 사용자가 기존 구조를 적용하거나 참고하지 않고 새로운 객체지향 Core 구조의 청사진만 작성하도록 요청했다.
- 2026-07-23: 유지 대상 CSV 계약과 `MonsterDayRecovery.cs`의 회차 초기화 동작을 검사했다.
- 2026-07-23: Definition, 런타임 Model, SkillBucket, Manager, Executor, Actor, 상태효과, 시각효과 책임 초안을 기록했다.
- 2026-07-23: `RunSessionModel`, `PartyRoster`, `PrisonerInventory`, `RewardService`, `OfferingService`, `ManifestationService`, 행동·이동 구조를 추가하고 `StageRunManager` 명칭을 `StageManager`로 통합했다.
- 2026-07-23: 현재 `NewRunScene` UI 구조를 유지하되 새 Core의 Model과 Service API에 맞게 바인딩을 개조하는 경계를 추가했다.
- 2026-07-23: 현현 후보, 수감자 소비, 공양 균등 추첨, 수동 마우스 조준, 전투 Manager 내부 실행 순서를 기존 Scripts의 실제 코드로 확정했다.
- 2026-07-23: 구현 중 미정 의미는 기존 `Pakuri/Assets/Scripts`를 먼저 검사하고, 그래도 불명확하면 사용자에게 질문하는 호환성 원칙과 최종 목표 폴더 트리를 추가했다.
- 2026-07-23: 모든 Skill Actor를 중앙 Tick으로 통합하고, 기존 현현 성공 팝업의 영입·넘기기 흐름을 유지하며, Save/Load는 만들지 않는 것으로 확정했다.
- 2026-07-23: 기존 Scripts의 코드 작성 형식을 참고하되 청사진의 책임 경계를 우선하고 Naive Code Filter의 불필요한 우회·다중 권위·중복 검증·dead code 기준에 걸리지 않도록 구현 컨벤션을 추가했다.
- 2026-07-23: 사용자가 현재 리소스는 유지하되 기존 Scripts의 모든 의존을 끊고 새 청사진 구현으로 완전히 교체하는 최종 목표를 확정했다.
- 2026-07-23: Unity 직렬화 파일 239개를 검사해 기존 Script 21종이 40개 자산에서 56회 참조되는 것을 확인했고, 씬·프리팹뿐 아니라 런타임 카탈로그와 Legacy `.asset`까지 전환 또는 제거 판정 대상에 포함했다.
- 2026-07-23: 사용자가 완전 교체 작업을 Phase로 나누고, 이 작업을 BLACKBOARD 계열 파일에 기록하지 않으며, 매 Phase Unity 로그를 확인하고 꼭 필요한 경우에만 Play Mode로 검증하도록 요청했다.

---

## 1. Core 설계 원칙

핵심 책임은 다음과 같이 구분한다.

```text
Definition       = 무엇인가
Model            = 현재 어떤 상태인가
SkillBucket      = 무엇을 배웠는가
SkillCooldown    = 지금 사용할 수 있는가
SkillTargeting   = 누구에게 사용할 것인가
Executor         = 무엇을 실행할 것인가
Actor            = 생성된 스킬이 언제 끝나는가
CombatManager    = 결과를 전투에 어떻게 반영하는가
EffectManager    = 어떻게 보이는가
StageManager     = 런이 어디까지 진행됐고 재화가 얼마인가
```

Definition은 게임 시작 시 CSV에서 생성한 뒤 불변 데이터로 취급한다.

각 `MonsterModel`은 자신의 `MonsterSkillBucket`을 소유한다. 몬스터의 액티브, 패시브, 강화, 마스터 학습 상태를 다른 Manager가 대신 소유하지 않는다.

### 1.1 구현 중 미확정 동작의 근거 우선순위

```text
1. 사용자가 이 청사진에서 직접 확정한 규칙
2. 유지 대상 CSV의 실제 열과 데이터
3. Pakuri/Assets/Scripts의 실제 플레이 동작
4. 그래도 하나로 정해지지 않으면 구현 중단 후 사용자 질문
```

기존 `Pakuri/Assets/Scripts`는 새 폴더 구조나 클래스 책임을 복사하기 위한 기준이 아니다. 새 구조에서 의미가 빠진 플레이 동작을 확인하고 기존 게임과의 행동 호환성을 유지하기 위한 근거다.

구현자는 기존 Scripts에 없는 숫자, 매핑, 우선순위, 실패 처리, 대상 규칙을 임의로 만들지 않는다.

기존 Scripts를 검사하는 행위는 새 구현이 기존 타입을 참조한다는 뜻이 아니다. 새 구현에는 이전 타입을 매개변수, 필드, 상속, 인터페이스, 이벤트, reflection 문자열, 어댑터 또는 fallback으로 연결하지 않는다.

## 2. Definition 계층

### 2.1 Skill Definition

```text
SkillDefinition
├─ ProjectileDefinition
├─ LineAttackDefinition
├─ AreaAttackDefinition
├─ SingleAttackDefinition
├─ BuffDefinition
├─ HealDefinition
├─ ShieldDefinition
└─ PassiveDefinition
```

`SkillDefinition`은 스킬의 공통 기본정보를 정의한다.

공통 후보 필드:

- `skill_id`
- `monster_id`
- `slot`
- `display_name`
- `runtime_kind`
- `description_text`
- `summary`

각 하위 Definition은 담당 CSV에 실제 존재하는 열 이름을 그대로 사용한다.

예를 들어 `ProjectileDefinition`은 다음과 같은 투사체 CSV 용어를 사용한다.

- `base_damage`
- `spell_power_coefficient`
- `magazine_capacity`
- `reload_seconds`
- `shot_interval_seconds`
- `projectile_burst_count`
- `projectile_speed`
- `pierce_count`
- `critical_allowed`
- `target_selection`
- `cooldown_seconds`
- `runtime_visual_sprite_path`
- `runtime_impact_visual_sprite_path`

적 스킬 CSV에는 별도 `heal` 및 `shield` 스킬 파일이 존재한다. 따라서 전체 CSV를 유지하려면 `HealDefinition`과 `ShieldDefinition`이 필요하다.

### 2.2 Choice 및 Node Definition

실제 CSV 계약은 Choice, 그래프 노드, 노드 종류, 노드 파라미터로 분리되어 있다.

```text
SkillChoiceDefinition
ChoiceNodeDefinition
NodeTypeDefinition
NodeParamDefinition
```

`SkillChoiceDefinition`:

- `choice_id`
- `skill_id`
- `monster_id`
- `target_skill_id`
- `choice_group`
- `sort_order`
- `title`
- `description_text`

`ChoiceNodeDefinition`:

- `monster_id`
- `owner_kind`
- `owner_id`
- `graph_kind`
- `graph_index`
- `target_skill_id`
- `node_order`
- `node_type_id`
- `arg_1`
- `arg_2`
- `arg_3`
- `arg_4`
- `arg_5`
- `arg_6`
- `arg_7`
- `arg_8`
- `arg_9`
- `arg_10`
- `arg_11`
- `arg_12`
- `excludes_active_choice_id`

`NodeTypeDefinition`:

- `node_type_id`
- `handler_id`
- `node_kind`
- `runtime_support_state`
- `runtime_support_notes`

`NodeParamDefinition`:

- `node_type_id`
- `param_order`
- `param_key`
- `value_type`
- `required`
- `allowed_values`

`handler_id`와 `param_key`는 서로 다른 CSV에 존재한다. 원본 CSV 계약을 유지하기 위해 하나의 `ChoiceNodeDefinition`에 강제로 합치지 않는다.

### 2.3 Trigger Definition

몬스터와 적의 Trigger CSV를 파싱하기 위해 다음 Definition이 필요하다.

```text
SkillTriggerDefinition
```

`SkillTriggerDefinition`은 실제 Trigger CSV의 다음 용어를 기반으로 한다.

- `trigger_id`
- `source_skill_id`
- `trigger_event`
- `triggered_skill_id`
- `runtime_kind`
- `sort_order`
- `target_side`
- `target_selection`
- `target_shape`
- `center_mode`
- `proc_chance`
- `trigger_action`
- `internal_cooldown_seconds`

Trigger CSV마다 실제 열 구성이 다르므로 각 파일에 없는 필드를 있다고 가정하지 않는다.

### 2.4 Unit Definition

클래스 이름은 `UnitDefinotion`이 아니라 `UnitDefinition`으로 기록한다.

```text
UnitDefinition
├─ MonsterDefinition
└─ EnemyDefinition
```

`UnitDefinition`은 유닛 공통 내용을 정의한다.

- 최대 체력
- 공격력
- 주문력
- 이동속도
- 치명타 확률
- 치명타 피해
- 치명타 저항
- 물리 방어력
- 화염 방어력
- 번개 방어력
- 얼음 방어력
- 어둠 방어력
- 신성 방어력

`MonsterDefinition`은 몬스터 CSV 용어를 사용한다.

- `id`
- `display_name`
- `role_summary`
- `element_label`
- `primary_attribute`
- `max_health`
- `power_stat`
- `base_damage`
- `power_coefficient`
- `base_attack_power`
- `base_spell_power`
- `base_move_speed`
- `base_crit_chance`
- `base_crit_damage`
- `base_crit_resistance`
- `def_physical`
- `def_fire`
- `def_lightning`
- `def_ice`
- `def_darkness`
- `def_holy`
- `MonsterIconImage`

`EnemyDefinition`은 적 CSV 용어를 사용한다.

- `enemy_id`
- `stage_id`
- `sort_order`
- `display_name`
- `encounter_role`
- `attack_type`
- `attribute`
- `max_health`
- `attack_power`
- `spell_power`
- `move_speed`
- `crit_chance`
- `crit_damage`
- `crit_resistance`
- `def_physical`
- `def_fire`
- `def_lightning`
- `def_ice`
- `def_darkness`
- `def_holy`
- `skill_slot_a_id`
- `skill_slot_b_id`
- `passive_id`
- `nexus_damage`

### 2.5 Stage Definition

스테이지 데이터는 실제 CSV 세 개에 맞춰 구성한다.

```text
StageDefinition
├─ StageDayDefinition
├─ StageEncounterDefinition
└─ StageRewardDefinition
```

`StageDayDefinition`:

- 일차와 전투 종류 정의
- 조우 및 보상 규칙 연결
- 상점, 이벤트, 엘리트 선택 필드 보관

`StageEncounterDefinition`:

- 적 생성 순서
- 적 종류와 수량
- 생성 간격과 위치
- 보스 후보 및 확정 보스
- 확정 수감자

`StageRewardDefinition`:

- 골드
- 어둠의 흔적
- 수감자 수 확률
- 현현 성공 확률
- 엘리트 추가 수감자
- 유물 선택 수

## 3. CSV 시작 파싱

```text
CsvParser.cs
    ↓
GameDefinitionCatalog
```

### 3.1 CsvParser

`CsvParser` 책임:

- 게임 시작 시 CSV를 한 번 파싱한다.
- 각 CSV의 열 이름을 그대로 읽는다.
- CSV 종류에 맞는 Definition을 생성한다.
- ID 참조를 연결한다.
- 중복 ID를 검사한다.
- 누락 참조를 검사한다.
- 잘못된 enum과 숫자를 검사한다.
- 오류가 있는 Definition을 조용히 추측하거나 보정하지 않는다.

`CsvParser`는 다음 런타임 책임을 갖지 않는다.

- 피해 계산
- 스킬 실행
- 유닛 생성
- 런 진행
- 스킬 학습 상태
- 시각 효과 생성

### 3.2 GameDefinitionCatalog

`GameDefinitionCatalog` 책임:

- 파싱 완료된 Definition 보관
- ID 기반 Definition 조회
- 참조가 연결된 불변 게임 데이터 제공

`GameDefinitionCatalog`는 런타임의 현재 체력, 상태효과, 쿨다운, 학습 상태를 보관하지 않는다.

### 3.3 초기화 순서

```text
Status Definition
→ NodeType / NodeParam Definition
→ Skill / Choice / ChoiceNode / Trigger Definition
→ Monster / Enemy Definition
→ Stage Definition
→ 전체 ID 참조 검증
→ GameDefinitionCatalog 확정
```

## 4. 런타임 Model 계층

```text
UnitBaseModel
├─ MonsterModel
├─ EnemyModel
└─ NexusModel
```

### 4.1 UnitBaseModel

각 유닛의 공통 런타임 수치를 보유한다.

- Definition 참조
- 현재 체력
- 현재 방어막
- 현재 능력치
- 생존 여부
- 현재 위치
- 현재 상태효과
- 현재 쿨다운 상태

### 4.2 MonsterModel

`MonsterModel` 책임:

- `MonsterDefinition` 참조
- `MonsterSkillBucket` 소유
- 자동 공격 상태
- 자동 스킬 상태
- 자신의 상태효과와 자원 상태
- 다음 회차를 위한 자신의 상태 초기화

다른 Manager가 몬스터의 학습 목록을 대신 소유하지 않는다.

### 4.3 EnemyModel

`EnemyModel` 책임:

- `EnemyDefinition` 참조
- `EnemySkillBucket` 소유
- 적에게 배정된 액티브와 패시브 상태
- 자신의 상태효과와 자원 상태
- 생존 및 넥서스 접촉 상태

### 4.4 NexusModel

`NexusModel` 책임:

- 현재 체력
- 최대 체력
- 생존 여부
- 받은 넥서스 피해 반영

### 4.5 RunSessionModel

한 번의 런에서 전투 사이에 유지되는 진행 상태를 보관한다.

`RunSessionModel` 책임:

- 현재 스테이지 식별자
- 현재 일차
- 현재 조우 식별자
- `PartyRoster` 참조
- `PrisonerInventory` 참조
- 현재 보상 처리 상태
- 런 승리 및 패배 상태

플레이어의 `Gold`와 `DarkTrace`는 사용자 지정에 따라 `RunSessionModel`이 아니라 `StageManager`가 관리한다.

`RunSessionModel`은 다음 책임을 갖지 않는다.

- 피해 계산
- 유닛 행동
- 스킬 실행
- UI 표시
- CSV 파싱

### 4.6 PartyRoster

한 런에서 플레이어가 보유한 순서 있는 몬스터 파티를 관리한다.

`PartyRoster` 책임:

- 최초 선택 몬스터를 첫 슬롯에 등록
- 현현된 몬스터를 다음 빈 슬롯에 등록
- 파티원 순서 보존
- 최대 파티 슬롯 제한
- 중복 몬스터 등록 방지
- 파티원 추가 가능 여부 반환
- 몬스터 식별자로 파티원 조회

`PartyRoster`는 파티원이 현재 필드에서 살아 있는지 판단하지 않는다. 현재 필드의 전체 유닛과 생존 유닛은 `StageManager`가 관리한다.

현재 UI에서 사용하는 1P부터 5P까지의 순서는 `PartyRoster` 순서를 그대로 사용한다. UI가 선택 몬스터와 현현 몬스터 목록을 다시 조합하지 않는다.

### 4.7 PrisonerInventory

전투 보상으로 획득한 수감자를 관리한다.

`PrisonerInventory` 책임:

- `enemy_id` 기반 수감자 등록
- 보유 수감자 조회
- 현현 또는 공양 대상 수감자 선택
- 수감자 소비 가능 여부 확인
- 수감자 소비
- 이미 소비한 수감자의 재사용 방지
- 새 전투 보상 생성 시 이전 목록 초기화
- 다음 일차로 넘어갈 때 남은 수감자 초기화

`PrisonerInventory`는 현현 성공 판정이나 공양 후보 생성을 하지 않는다.

기존 Scripts의 수감자는 런 전체 누적 자원이 아니라 현재 전투의 보상 단계에서만 유지된다. 사용하지 않은 수감자는 다음 일차에 이월하지 않는다.

## 5. SkillBucket

```text
SkillBucket
├─ MonsterSkillBucket
└─ EnemySkillBucket
```

클래스 이름은 `Skillbucket`이 아니라 `SkillBucket`으로 기록한다.

### 5.1 MonsterSkillBucket

각 `MonsterModel`이 하나씩 소유한다.

- 학습한 액티브 스킬
- 학습한 패시브 스킬
- 선택한 강화
- 선택한 마스터
- 스킬별 획득 제한
- 중복 학습 방지
- 스킬 사용 시 적용할 Choice와 Node 제공

몬스터의 학습 상태를 변경하는 최종 주체는 해당 몬스터의 `MonsterSkillBucket`이다.

### 5.2 EnemySkillBucket

각 `EnemyModel`이 하나씩 소유한다.

- 적에게 배정된 액티브 스킬
- 적에게 배정된 패시브
- 사용 가능한 스킬
- 적 스킬 슬롯 제한

## 6. SkillCooldown

각 Model이 참조하는 스킬 사용 조건 판정 객체다.

`SkillCooldown` 책임:

- 현재 쿨다운
- 탄창
- 재장전
- 발사 간격
- 사용 가능 여부 판정
- `CanUse()` 결과 반환
- 스킬 사용 후 런타임 상태 갱신
- 다음 회차 초기화

`SkillCooldown`은 다음 책임을 갖지 않는다.

- 대상 탐색
- 피해 계산
- 상태효과 적용
- 시각 효과 생성
- 스킬 학습 상태 변경

## 7. SkillTargeting

자동 상태에서 스킬 대상을 찾는다.

`SkillTargeting` 책임:

- `target_selection` 적용
- `target_scope` 적용
- `radius` 적용
- 현재 살아 있는 유닛 후보 사용
- 최종 대상 또는 대상 목록 반환

`SkillTargeting`은 피해 계산이나 스킬 실행을 하지 않는다.

수동 대상 지정은 아직 사용자 요구에 포함되지 않았으므로 현재 초안에서 확정하지 않는다.

## 8. 행동·이동 구조

### 8.1 InGameActionManager

전투 중 행동 Controller의 실행 순서를 조율한다.

`InGameActionManager` 책임:

- `StageManager`의 전투 진행 상태 확인
- 살아 있는 유닛의 행동 Controller 갱신
- 플레이어 입력 처리 순서와 자동 행동 처리 순서 조율
- 행동 불가 상태인 유닛의 실행 차단
- `SkillCooldown`, `SkillTargeting`, Executor 호출 흐름 연결

`InGameActionManager`는 피해 계산이나 스킬 학습 상태를 소유하지 않는다.

### 8.2 UnitActionController

```text
UnitActionController
├─ MonsterActionController
└─ EnemyActionController
```

공통 책임:

- 담당 `UnitBaseModel` 참조
- 유닛 생존 여부 확인
- 이동·행동·특수 스킬 가능 상태 확인
- 사용 가능한 스킬 조회
- `SkillCooldown.CanUse()` 호출
- 대상이 준비되면 Executor 실행 요청

### 8.3 MonsterActionController

`MonsterActionController` 책임:

- `MonsterModel`과 `MonsterSkillBucket` 참조
- 선택 몬스터의 수동 또는 자동 스킬 상태 확인
- 현현 몬스터의 자동 행동 처리
- 자동 상태일 때 `SkillTargeting`으로 대상 선택
- 사용 가능한 스킬의 Executor 실행 요청

수동 입력은 `PlayerInputController`가 전달한다. `MonsterActionController`가 직접 UI 버튼을 찾지 않는다.

### 8.4 EnemyActionController

`EnemyActionController` 책임:

- `EnemyModel`과 `EnemySkillBucket` 참조
- 공격 가능한 플레이어 유닛 탐색
- 공격 가능한 대상이 없을 때 넥서스 목표 사용
- 공격 범위 밖이면 `UnitMovementController`에 이동 요청
- 공격 범위 안이면 사용 가능한 스킬 실행
- 넥서스 접촉 조건에서 `nexus_damage` 적용 요청

적 생성 시점과 적 종류는 결정하지 않는다. 이는 `StageManager`와 `SpawnManager` 책임이다.

### 8.5 UnitMovementController

이동 가능한 유닛의 위치 변경만 담당한다.

`UnitMovementController` 책임:

- 현재 위치와 목표 위치 확인
- 이동속도 적용
- 상태효과의 이동 가능 여부 적용
- `deltaTime`에 따른 위치 갱신
- 목표 도달 여부 반환

`UnitMovementController`는 대상 선정, 공격, 피해 계산, 스킬 실행을 하지 않는다.

현재 게임에서 적의 목표 접근 이동을 담당한다. 몬스터 이동 규칙은 아직 사용자 요구에 없으므로 기본 이동 대상으로 확정하지 않는다.

### 8.6 PlayerInputController

선택 플레이어 몬스터의 수동 입력과 자동 상태 변경을 담당한다.

`PlayerInputController` 책임:

- 선택 몬스터 식별
- 자동 스킬 상태 변경
- 수동 스킬 사용 요청
- UI 입력을 `MonsterActionController`에 전달

수동 스킬 대상 지정 규칙:

- 파티 슬롯 0의 선택 몬스터만 수동 입력을 받는다.
- 선택 몬스터의 Auto 스킬 상태가 꺼져 있을 때만 수동 입력을 처리한다.
- 마우스 왼쪽 버튼 입력을 사용한다.
- 포인터가 UI 위에 있으면 전투 입력을 무시한다.
- 마우스 화면 좌표를 전투 월드 좌표로 변환한다.
- 선택 몬스터 위치에서 월드 조준점까지의 방향을 `aimDirection`으로 사용한다.
- 월드 조준점을 `targetPoint`로 사용한다.
- 비투사체 스킬은 버튼을 누른 프레임의 조준점으로 한 번 실행을 시도한다.
- 투사체 스킬은 버튼을 누르고 있는 동안 최신 조준을 사용한다.
- 연속 발사 중인 투사체는 버튼을 놓아도 마지막 저장 조준으로 남은 발사를 계속한다.
- 범위 중심이 필요한 스킬은 수동 `targetPoint`를 우선 사용한다.
- 수동 조준점이 없을 때만 해당 스킬의 자동 `target_selection` 규칙을 사용한다.

수동 입력은 조준 방향과 목표 지점만 전달한다. 최종 적중 대상과 범위 포함 여부는 각 Executor와 `SkillTargeting`이 판정한다.

### 8.7 행동 실행 순서

```text
1. 패시브 변경 반영
2. 모든 등록 유닛의 쿨다운·탄창·재장전 상태 Tick
3. 플레이어 및 현현 몬스터 자동 스킬을 등록 순서와 액티브 스킬 목록 순서로 시도
4. 선택 몬스터 수동 입력 처리
5. 적 행동을 적 등록 순서로 처리
6. `SkillActorManager`가 현재 활성 Skill Actor를 등록 순서로 Tick
7. 모든 유닛의 상태효과 지속시간과 만료 처리
8. 상태 변경으로 발생한 패시브 변경 최종 반영
```

적 한 유닛의 프레임 내부 행동 순서:

```text
1. 사망 및 AutoAttackEnabled 확인
2. 진행 중인 돌진 행동 Tick
3. 가장 가까운 살아 있는 플레이어 탐색
4. 플레이어가 없으면 넥서스 선택
5. 사용 가능한 지원형 B 스킬 우선 시도
6. 공격형 B 스킬 우선, 사용할 수 없으면 A 스킬 선택
7. 사거리 밖이면 이동
8. 사거리 안이고 행동 가능하면 선택 스킬 실행
9. 넥서스 접촉 시 nexus_damage 적용 후 적 제거
```

위 순서는 기존 `InGameCombatManager.Update()`, `SkillExecution.TryExecuteAutomaticSkills(...)`, `PlayerCombatInputController.HandleManualInput(...)`, `EnemyActionController.Tick(...)`의 실제 호출 순서를 호환성 기준으로 사용한다.

새 구조에서는 모든 Skill Actor의 독립 Unity `Update()`를 제거한다. `InGameActionManager`가 한 프레임에 한 번 위 순서대로 Tick하고, 6단계에서 `SkillActorManager.Tick(deltaTime)`을 호출한다.

현재 프레임의 스킬 실행으로 새로 생성된 Skill Actor는 `pendingAdd`에 등록하고 다음 프레임부터 Tick한다. Tick 중 종료된 Actor는 `pendingRemove`에 등록하고 현재 Actor 순회가 끝난 뒤 제거한다. 이 규칙으로 컬렉션 변경과 생성 프레임의 중복 Tick을 방지한다.

## 9. Skill Executor

스킬 종류별 Executor를 둔다.

```text
ProjectileExecutor
LineAttackExecutor
AreaAttackExecutor
SingleAttackExecutor
BuffExecutor
HealExecutor
ShieldExecutor
PassiveExecutor
```

Executor 책임:

- 스킬 Definition 읽기
- 시전자 `SkillBucket`의 학습 내용 읽기
- 적용 가능한 Choice와 Node 결합
- Trigger 조건 반영
- 실제 스킬 실행 결과 생성
- 필요한 Actor 생성 요청
- `InGameCombatManager`에 피해, 회복, 방어막, 상태효과 적용 요청

Executor는 스킬 학습 상태를 직접 변경하지 않는다.

Executor는 스테이지 진행이나 시각 효과 삭제를 담당하지 않는다.

## 10. Skill Actor

각 소환된 스킬의 런타임 생명주기를 감시한다.

예시:

```text
ProjectileActor
LineAttackActor
AreaAttackActor
SingleAttackActor
BuffActor
```

Actor 책임:

- 생성 시점 기록
- 이동
- 지속시간
- 충돌
- 적중
- 종료 조건
- 종료 시 스킬 효과 종료 신호
- `EffectManager`에 시각 효과 삭제 신호

Actor는 피해 공식을 소유하지 않는다.

Actor는 몬스터의 학습 상태를 소유하지 않는다.

Actor는 독립 Unity `Update()`를 갖지 않는다. 다음 공통 메서드로만 갱신한다.

```text
Tick(float deltaTime)
```

### 10.1 SkillActorManager

현재 활성 Skill Actor의 중앙 생명주기를 관리한다.

`SkillActorManager` 책임:

- 생성된 Skill Actor 등록 요청 수신
- 다음 프레임 등록을 위한 `pendingAdd` 관리
- 활성 Actor를 등록 순서로 Tick
- 종료 Actor의 `pendingRemove` 관리
- 순회 종료 뒤 Actor 제거
- Actor 제거 시 `EffectManager`에 시각 효과 삭제 요청
- 전투 종료와 다음 회차 전환 시 모든 Actor 정리

`SkillActorManager`는 피해 계산, 대상 선정, 스킬 학습 상태를 담당하지 않는다.

`InGameActionManager`만 `SkillActorManager.Tick(deltaTime)`을 호출한다. 다른 Manager나 UI가 Skill Actor를 직접 Tick하지 않는다.

## 11. InGameCombatManager

게임 내 전투 결과를 조율한다.

책임:

- 스킬 실행 요청 접수
- 해당 Executor 호출
- 최종 피해 계산
- 피해 할당
- 회복 할당
- 방어막 할당
- 상태효과 적용 요청
- 피격, 처치, 스킬 발동 이벤트 전달

금지 책임:

- 스킬 학습 상태 소유
- 적 스폰
- 현현 몬스터 스폰
- 스테이지와 일차 진행
- 프리팹 생명주기
- 런 보상 소유

`InGameCombatManager`는 모든 로직을 직접 구현하는 거대 객체가 아니라 전투 실행을 연결하는 조정자다.

## 12. SpawnManager

게임 내 유닛 생성을 담당한다.

책임:

- 적 스폰
- 현현 몬스터 스폰
- Definition을 기반으로 Model 생성
- Model과 씬 Actor 연결
- 생성된 유닛을 `StageManager`의 필드 유닛 목록에 등록

금지 책임:

- 어떤 적이 어느 일차에 나올지 결정
- 피해 계산
- 스킬 학습
- 다음 스테이지 결정

## 13. StageManager

`RunSessionModel`, 플레이어 재화, 현재 필드 진행을 담당한다.

책임:

- 활성 `RunSessionModel` 소유
- 현재 스테이지
- 현재 일차
- 현재 조우
- 플레이어 `Gold` 관리
- 플레이어 `DarkTrace` 관리
- 재화 추가와 사용 가능 여부 검사
- 재화 소비
- 전체 필드 유닛
- 현재 살아 있는 유닛
- 일차 시작
- 적 생성 순서 진행
- 전투 종료 판단
- 다음 일차
- 다음 스테이지
- 승리와 패배
- 회차 전환 시 유닛 초기화 요청
- `SpawnManager`에 생성 명령

재화 변경은 `StageManager`의 메서드를 통해서만 수행한다.

```text
AddGold(amount)
CanSpendGold(amount)
SpendGold(amount)
AddDarkTrace(amount)
CanSpendDarkTrace(amount)
SpendDarkTrace(amount)
```

`RewardService`, `OfferingService`, `ManifestationService`, UI는 Gold와 DarkTrace 필드를 직접 변경하지 않는다.

`StageManager`는 `PartyRoster`와 `PrisonerInventory`를 직접 구현하지 않는다. 활성 `RunSessionModel`을 통해 두 객체에 접근하고, 실제 파티·수감자 규칙은 각 객체와 Service에 맡긴다.

`StageManager`가 유닛의 내부 상태 필드를 직접 초기화하지 않는다.

각 Model에 다음과 같은 초기화 요청을 전달한다.

```text
monsterModel.ResetForNextDay()
enemyModel.ResetForNextDay()
```

검사한 `MonsterDayRecovery.cs` 기준 회차 초기화 대상:

- 상태효과 제거
- 직접 방어막 제거
- 현재 방어막 제거
- 스킬 런타임 상태 초기화
- 체력 완전 회복
- 자동 공격 활성화
- 선택 몬스터가 아닌 경우 자동 스킬 활성화

새 구조에서는 이 동작을 `StageManager`가 순회 요청하고, 실제 초기화는 각 Model이 자기 상태에 수행한다.

### 13.1 RewardService

전투 종료 후 `StageRewardDefinition`을 실제 런 보상으로 변환하고 지급한다.

`RewardService` 책임:

- 현재 스테이지와 전투 종류에 맞는 `StageRewardDefinition` 조회
- Gold와 DarkTrace 보상 계산
- 수감자 수 결정
- 보스·확정 수감자 규칙 적용
- `StageManager`를 통한 Gold와 DarkTrace 지급
- `PrisonerInventory`에 수감자 등록
- UI가 표시할 보상 결과 반환

`RewardService`는 UI 버튼을 생성하지 않고 현현 또는 공양을 실행하지 않는다.

### 13.2 OfferingService

선택한 파티 몬스터의 공양 후보를 만들고 선택 결과를 적용한다.

`OfferingService` 책임:

- 대상 몬스터가 `PartyRoster`에 있는지 확인
- 대상 `MonsterSkillBucket` 조회
- 기존 등장 규칙에 따라 학습 가능한 액티브 후보 생성
- 기존 등장 규칙에 따라 학습 가능한 패시브 후보 생성
- 기존 등장 규칙에 따라 선택 가능한 강화 및 마스터 후보 생성
- 모든 자격 후보를 하나의 목록에 합치기
- 모든 후보를 동일한 가중치로 균등 셔플
- 셔플 결과의 앞 3개만 반환
- 한 번 생성한 후보 세트는 선택이 끝날 때까지 보관
- 재추첨 제공 금지
- 선택 결과를 해당 `MonsterSkillBucket`에 적용
- 사용한 수감자를 `PrisonerInventory`에서 소비

`OfferingService`는 스킬 전투 효과를 실행하지 않는다. 학습 결과만 변경한다.

기존 등장 자격 규칙:

- 기본 A 액티브 외 추가 액티브는 최대 2개
- 패시브는 최대 5개
- 선행 액티브가 필요한 패시브는 해당 액티브를 배운 뒤에만 후보가 된다.
- 액티브 강화는 대상 스킬당 최대 3개
- 액티브 마스터는 대상 스킬의 액티브 강화 3개를 선택한 뒤에만 후보가 되며 최대 1개
- 패시브 강화는 대상 패시브당 최대 1개
- 이미 학습하거나 선택한 항목은 후보에서 제외

공양 버튼을 누르면 위 규칙을 통과한 모든 후보를 같은 확률로 섞고 최대 3개를 표시한다. 종류별 추가 가중치나 고정 비율은 없다.

공양 패널을 열었지만 자격 후보가 하나도 없으면 수감자를 소비하지 않는다. 후보 중 하나를 확정했을 때만 수감자를 소비한다.

### 13.3 ManifestationService

수감자를 사용해 새 몬스터를 파티에 추가하는 규칙을 담당한다.

`ManifestationService` 책임:

- 선택한 수감자의 보유 여부 확인
- `PartyRoster`의 다음 빈 슬롯 확인
- 중복 몬스터와 최대 파티 제한 확인
- 현현 시도를 시작할 때 수감자 즉시 소비
- `enemy_id`와 무관하게 현재 파티에 없는 전체 플레이어 몬스터를 후보로 구성
- 모든 현현 후보를 동일한 확률로 무작위 선택
- 현재 보상의 `manifest_success_chance` 적용
- 실패해도 소비한 수감자를 반환하지 않음
- 성공 시 `PartyRoster`에 현현 몬스터 등록
- 성공·실패 결과 반환
- 성공 몬스터 영입을 확정하면 `StageManager`를 통해 `SpawnManager`에 즉시 배치 요청
- 다음 회차를 기다리지 않고 현재 필드의 다음 파티 슬롯에 배치

`ManifestationService`는 직접 프리팹을 생성하지 않는다.

수감자 `enemy_id`와 현현 가능한 `monster_id` 사이에는 직접 매핑을 두지 않는다. 수감자의 `enemy_id`는 UI 표시와 재료 식별에 사용하며 현현 후보 선정에는 사용하지 않는다.

현현 성공 후 기존 UI에는 성공 몬스터를 영입하거나 넘길 수 있는 선택이 존재한다. 영입을 확정하면 같은 흐름에서 파티 등록과 씬 스폰을 즉시 수행한다. 넘기면 수감자는 이미 소비된 상태로 유지되고 몬스터는 파티에 추가하지 않는다.

### 13.4 런 보상 흐름

```text
전투 종료
→ StageManager가 Reward 상태 진입
→ RewardService가 보상 생성
→ StageManager에 Gold·DarkTrace 지급
→ PrisonerInventory에 수감자 등록
→ 사용자가 수감자 선택
→ OfferingService 또는 ManifestationService 실행
→ 다음 일차 진행
```

## 14. StatusDefinition과 StatusEffect

### 14.1 StatusDefinition

`status_effects.csv`의 상태이상 원본 정의를 보관한다.

- `status_effect_id`
- `status_effect_label`
- `effect_type`
- `attribute`
- `default_duration_seconds`
- `is_permanent`
- `max_stacks`
- `base_stack_amount`
- `can_move`
- `can_act`
- `can_use_special_skill`
- `action_speed_bonus_per_stack`
- `move_speed_bonus_per_stack`
- `attack_power_bonus_per_stack`
- `damage_taken_bonus_per_stack`
- `critical_damage_taken_bonus_per_stack`
- `critical_resistance_bonus_per_stack`
- `element_resist_reduction_per_stack`
- `element_damage_taken_bonus_per_stack`
- `status_effect_prefab_path`

### 14.2 StatusEffect

특정 유닛에게 실제 적용된 상태를 보관하고 실행한다.

- `StatusDefinition` 참조
- 남은 지속시간
- 현재 중첩
- 적용한 유닛
- 적용받은 유닛
- 효과 적용
- 효과 갱신
- 효과 중첩
- 효과 해제

`StatusDefinition`은 불변 데이터다. `StatusEffect`는 전투 중 변하는 상태다.

## 15. EffectManager

스킬의 런타임 시각 효과와 프리팹 시각 효과를 관리한다.

책임:

- 시각 효과 생성
- 프리팹 생성
- Actor별 Effect handle 반환
- 위치와 방향 갱신
- Actor 또는 Executor의 종료 신호 처리
- 시각 효과 삭제

금지 책임:

- 피해 판정
- 상태효과 판정
- 스킬 사용 가능 여부
- 대상 선정
- 스킬 학습 상태

## 16. 기존 UI 구조 재사용과 개조 경계

현재 `NewRunScene`의 UI 계층과 시각 배치는 재사용한다. 새 청사진은 UI를 새로 설계하지 않고, 기존 UI가 읽고 호출하는 대상을 새 Core API로 교체한다.

재사용 대상:

- 메인 메뉴와 런 진입 흐름
- RewardPanel
- PrisonPanel
- 1P부터 5P까지의 파티 슬롯
- 공양 UI
- 현현 성공·실패 UI
- 몬스터 패널
- 넥서스 체력 표시
- 데미지 미터
- Auto 버튼
- 게임 속도 버튼
- DebugPanel

UI 개조 원칙:

- UI는 `UnitBaseModel`, `MonsterSkillBucket`, 재화 필드를 직접 변경하지 않는다.
- Gold와 DarkTrace는 `StageManager`의 읽기 전용 값으로 표시한다.
- 파티 슬롯은 `RunSessionModel.PartyRoster` 순서를 그대로 표시한다.
- 수감자 목록은 `RunSessionModel.PrisonerInventory`를 표시한다.
- 보상 UI는 `RewardService`가 반환한 결과를 표시한다.
- 공양 UI는 `OfferingService`에서 후보를 받고 선택 명령만 전달한다.
- 현현 UI는 `ManifestationService`에 실행 명령을 전달하고 성공·실패 결과를 표시한다.
- Auto UI는 `PlayerInputController`를 통해 선택 몬스터 상태를 변경한다.
- 데미지 미터는 `InGameCombatManager`의 피해 확정 이벤트를 구독한다.
- 넥서스 UI는 `NexusModel` 상태를 표시한다.
- `EffectManager`는 스킬 시각 효과만 관리하며 패널 UI를 관리하지 않는다.

기존 씬 오브젝트 이름과 레이아웃을 유지할 수 있다. 기존 UI 스크립트가 이전 상태 객체나 Manager의 내부 필드를 직접 읽는 부분은 새 `StageManager`, `RunSessionModel`, `PartyRoster`, Service API에 맞게 개조해야 한다.

### 16.1 GameBootstrap

게임 시작과 새 Core 연결을 담당하는 진입점이 필요하다.

`GameBootstrap` 책임:

- `CsvParser` 실행
- `GameDefinitionCatalog` 생성
- `RunSessionModel` 생성
- `StageManager` 초기화
- `InGameCombatManager`와 `InGameActionManager` 초기화
- `RewardService`, `OfferingService`, `ManifestationService` 생성 및 연결
- 기존 UI Controller에 새 Core 조회·명령 API 연결

`GameBootstrap`은 게임 규칙을 직접 구현하지 않는다.

## 17. 전체 실행 흐름

### 17.1 게임 시작

```text
게임 시작
→ CsvParser
→ Definition 생성
→ ID 참조 검증
→ GameDefinitionCatalog 확정
→ GameBootstrap
→ RunSessionModel 생성
→ StageManager와 Service 초기화
→ 기존 UI를 새 Core API에 연결
```

### 17.2 유닛 생성

```text
StageManager
→ SpawnManager
→ UnitDefinition 조회
→ UnitBaseModel 하위 Model 생성
→ SkillBucket 생성
→ 필드 유닛 등록
```

### 17.3 자동 스킬 실행

```text
InGameActionManager
→ MonsterActionController 또는 EnemyActionController
→ SkillBucket에서 사용 스킬 조회
→ SkillCooldown.CanUse()
→ SkillTargeting으로 대상 조회
→ 스킬 종류별 Executor 실행
→ InGameCombatManager가 전투 결과 반영
→ 필요한 Actor 생성
→ EffectManager가 시각 효과 생성
```

### 17.4 적 이동과 넥서스 공격

```text
EnemyActionController가 공격 대상 확인
→ 공격 범위 밖이면 UnitMovementController 호출
→ 공격 범위 안이면 적 스킬 Executor 실행
→ 플레이어 유닛이 없으면 넥서스로 이동
→ 넥서스 접촉 시 InGameCombatManager에 nexus_damage 적용 요청
```

### 17.5 스킬 종료

```text
Actor가 종료 조건 감지
→ 전투 효과 종료 신호
→ EffectManager에 삭제 신호
→ Actor 제거
```

### 17.6 보상·공양·현현

```text
전투 종료
→ RewardService가 보상 생성
→ StageManager가 Gold·DarkTrace 갱신
→ PrisonerInventory가 수감자 등록
→ 기존 PrisonPanel에 PartyRoster와 수감자 표시
→ OfferingService 또는 ManifestationService 실행
→ MonsterSkillBucket 또는 PartyRoster 변경
```

### 17.7 다음 회차

```text
StageManager가 전투 종료 확인
→ 전체 필드 유닛에 ResetForNextDay() 요청
→ 현재 일차 갱신
→ 다음 StageDefinition 조회
→ SpawnManager에 다음 조우 생성 요청
```

## 18. 구현 가능성 판정

### 18.1 현재 활성 게임 요소

이 청사진 v0.2는 다음 활성 요소에 모두 명시적인 책임 주체를 제공한다.

- 몬스터, 적, 넥서스
- 파티와 현현 몬스터
- 액티브, 패시브, 강화, 마스터
- Skill Choice, Node, Trigger
- 피해, 회복, 방어막, 상태효과
- 투사체, 직선, 범위, 단일 공격, 버프
- 자동 행동, 수동 스킬 요청
- 적 이동과 넥서스 공격
- 스테이지, 일차, 조우, 스폰
- 승리, 패배, 회차 초기화
- Gold, DarkTrace
- 수감자
- 보상
- 공양
- 현현
- 기존 파티, 보상, 수감자, 공양, 현현, 넥서스, 데미지 미터, Auto UI

따라서 현재 확인된 활성 게임 요소는 이 구조로 구현할 수 있다.

### 18.2 확정된 호환성 규칙

다음 규칙은 사용자 지시와 기존 Scripts 검사로 확정됐다.

- 수감자 `enemy_id`와 현현 `monster_id`는 직접 연결하지 않는다.
- 현현 후보는 현재 파티에 없는 플레이어 몬스터 전체에서 동일 확률로 선택한다.
- 현현 시도 시작 시 수감자를 소비하며 실패해도 반환하지 않는다.
- 현현 몬스터 영입을 확정하면 현재 필드에 즉시 배치한다.
- 공양 후보는 기존 학습·선행·개수 제한을 통과한 전체 후보에서 동일 확률로 최대 3개를 뽑는다.
- 공양 종류별 별도 가중치와 재추첨은 없다.
- 공양 선택을 확정할 때 수감자를 소비한다.
- 수동 스킬은 선택 몬스터의 마우스 월드 조준점과 방향을 사용한다.
- 중앙 전투 프레임 내부 순서는 쿨다운 Tick, 자동 스킬, 수동 입력, 적 행동, 상태효과 Tick 순서다.

### 18.3 추가 확정 사항

- 모든 Skill Actor는 독립 Unity `Update()`를 사용하지 않고 중앙 `SkillActorManager` Tick을 사용한다.
- 현현 성공 팝업은 기존 UI 흐름을 유지한다.
- 사용자는 성공한 몬스터를 영입하거나 넘길 수 있다.
- 영입하면 즉시 파티 등록과 필드 배치를 수행한다.
- 넘기면 수감자는 소비된 상태로 유지되고 몬스터는 추가하지 않는다.
- Save/Load는 현재 존재하지 않으며 이번 구조에서 만들지 않는다.
- Gold, DarkTrace, PartyRoster, PrisonerInventory는 실행 중인 현재 런의 메모리 상태만 담당한다.

현재 활성 게임 요소 구현을 막는 추가 정보 부족은 없다. 구현 중 새 의미 공백이 발견되면 1.1절 근거 우선순위를 적용한다.

### 18.4 현재 비활성 또는 데이터 전용 요소

CSV에 필드는 있으나 현재 활성 요소로 확인되지 않은 상점, 이벤트, 엘리트 선택, 유물 UI는 이 청사진에서 런타임 구현 대상으로 확정하지 않는다.

이 요소를 활성화하려면 별도 Service, 상태 Model, UI 책임을 추가해야 한다.

### 18.5 완전 교체와 리소스 재연결 경계

최종 목표는 기존 Scripts 위에 새 구조를 덧씌우는 공존 구조가 아니다.

```text
유지
├─ Pakuri/Assets/CSVdata
├─ 현재 사용하는 씬 계층과 UI 오브젝트
├─ 현재 사용하는 프리팹
├─ 스프라이트
├─ 애니메이션과 AnimatorController
└─ Inspector에서 사용하는 리소스 값

교체
├─ 기존 MonoBehaviour 연결
├─ 기존 ScriptableObject 타입 연결
├─ 기존 런타임 Manager와 정적 상태
├─ 기존 코드 사이의 호출 관계
└─ 기존 Scripts를 요구하는 초기화 경로
```

청사진의 Markdown 파일 자체를 Unity 리소스에 연결할 수는 없다. Code Builder가 청사진을 근거로 새 C# 타입을 구현한 뒤 새 타입의 Script GUID와 직렬화 필드에 현재 리소스를 다시 연결해야 한다.

전환 기간에는 기존 Scripts를 동작·연결 비교용으로 읽을 수 있다. 그러나 새 런타임과 기존 런타임을 동시에 게임 상태의 권위로 실행하지 않는다. 기능 단위 전환이 완료되면 해당 기존 컴포넌트를 씬과 프리팹에서 제거하고 새 컴포넌트만 연결한다.

현재 검사에서 확인한 전환 범위:

- Unity 직렬화 파일 239개를 검사했다.
- 기존 Script 21종이 40개 직렬화 자산에서 총 56회 참조된다.
- 참조 위치에는 씬, 프리팹, `CsvRuntimeCatalog.asset`, `Legacy/Data/GameData/*.asset`이 포함된다.
- `.anim` 파일에서 직렬화된 Animation Event 함수 이름은 확인되지 않았다.
- 현재 씬·프리팹의 `m_Script: {fileID: 0}` 누락 연결은 0개다.

따라서 새 구현 완료만으로 전환이 끝나지 않는다. 모든 기존 Script GUID 참조를 분류해 다음 중 하나로 처리해야 한다.

1. 현재 플레이에 필요한 자산은 새 컴포넌트 또는 새 ScriptableObject 타입으로 재연결한다.
2. CSV 파싱으로 대체되어 더 이상 필요 없는 Legacy 데이터 자산은 실제 참조 여부를 확인한 뒤 제거 또는 `Assets` 밖 보관 대상으로 사용자 승인을 받는다.
3. 새 구조에서도 필요한 Inspector 값은 의미가 같은 새 직렬화 필드로 명시적으로 이전한다.
4. 이전 타입을 요구하는 자산이 남으면 완전 교체 미완료로 판정한다.

최종 전환은 준비와 검증을 단계적으로 마친 뒤 한 번에 활성화할 수 있다. 구현, 자산 재연결, 컴파일, 플레이 검증을 근거 없이 한 번의 변경으로 완료됐다고 판정하지 않는다.

### 18.6 완전 교체 완료 조건

다음 조건을 모두 만족해야 정상적인 게임 플레이가 가능한 완전 교체로 판정한다.

- 새 코드에서 기존 Scripts의 타입과 namespace 참조가 0개다.
- 새 코드에서 기존 Scripts를 reflection 문자열, `SendMessage`, fallback 또는 어댑터로 호출하는 경로가 0개다.
- `Pakuri/Assets`의 Unity 직렬화 자산에서 이전 Script GUID 참조가 0개다.
- 씬과 프리팹의 Missing Script가 0개다.
- 유지 대상 CSV 파일과 열 이름은 변경되지 않는다.
- 활성 프리팹, 스프라이트, 애니메이션, AnimatorController가 새 소유자에 연결된다.
- 전투, 행동·이동, 스테이지, 보상, 공양, 현현, UI 흐름이 새 Core만으로 실행된다.
- 중앙 Tick 밖에서 전투 상태를 변경하는 이전 컴포넌트가 없다.
- 컴파일 오류와 Unity Console 오류가 없다.
- 사용자가 Unity Play Mode에서 정상 게임 플레이를 확인한다.

기존 `.cs` 파일을 씬에서 분리하는 것만으로는 컴파일 의존이 제거되지 않는다. 최종 단계에서 이전 소스가 `Assets` 아래 남아 있으면 Unity 컴파일 대상이다. 모든 새 자산 연결과 플레이 검증이 끝난 뒤, 이전 소스는 사용자 승인하에 삭제하거나 Unity 컴파일 대상이 아닌 보관 위치로 이동해야 한다.

## 19. 최종 목표 폴더·스크립트 구조

아래 구조는 구현 후 목표 상태다. 현재 저장소에 이 구조가 이미 존재한다는 뜻이 아니다.

```text
Pakuri/Assets/Scripts/
├─ Core/
│  ├─ Bootstrap/
│  │  └─ GameBootstrap.cs
│  ├─ Parsing/
│  │  └─ CsvParser.cs
│  ├─ Catalog/
│  │  └─ GameDefinitionCatalog.cs
│  └─ Definitions/
│     ├─ Skills/
│     │  ├─ SkillDefinition.cs
│     │  ├─ ProjectileDefinition.cs
│     │  ├─ LineAttackDefinition.cs
│     │  ├─ AreaAttackDefinition.cs
│     │  ├─ SingleAttackDefinition.cs
│     │  ├─ BuffDefinition.cs
│     │  ├─ HealDefinition.cs
│     │  ├─ ShieldDefinition.cs
│     │  ├─ PassiveDefinition.cs
│     │  └─ SkillTriggerDefinition.cs
│     ├─ Choices/
│     │  ├─ SkillChoiceDefinition.cs
│     │  ├─ ChoiceNodeDefinition.cs
│     │  ├─ NodeTypeDefinition.cs
│     │  └─ NodeParamDefinition.cs
│     ├─ Units/
│     │  ├─ UnitDefinition.cs
│     │  ├─ MonsterDefinition.cs
│     │  └─ EnemyDefinition.cs
│     ├─ Stage/
│     │  ├─ StageDefinition.cs
│     │  ├─ StageDayDefinition.cs
│     │  ├─ StageEncounterDefinition.cs
│     │  └─ StageRewardDefinition.cs
│     └─ Status/
│        └─ StatusDefinition.cs
├─ Run/
│  ├─ RunSessionModel.cs
│  ├─ StageManager.cs
│  ├─ PartyRoster.cs
│  ├─ PrisonerInventory.cs
│  └─ Services/
│     ├─ RewardService.cs
│     ├─ OfferingService.cs
│     └─ ManifestationService.cs
├─ Units/
│  ├─ Models/
│  │  ├─ UnitBaseModel.cs
│  │  ├─ MonsterModel.cs
│  │  ├─ EnemyModel.cs
│  │  └─ NexusModel.cs
│  └─ Actors/
│     ├─ UnitActor.cs
│     ├─ MonsterActor.cs
│     ├─ EnemyActor.cs
│     └─ NexusActor.cs
├─ Combat/
│  ├─ InGameCombatManager.cs
│  ├─ Actions/
│  │  ├─ InGameActionManager.cs
│  │  ├─ UnitActionController.cs
│  │  ├─ MonsterActionController.cs
│  │  ├─ EnemyActionController.cs
│  │  ├─ UnitMovementController.cs
│  │  └─ PlayerInputController.cs
│  ├─ Skills/
│  │  ├─ Runtime/
│  │  │  ├─ SkillBucket.cs
│  │  │  ├─ MonsterSkillBucket.cs
│  │  │  ├─ EnemySkillBucket.cs
│  │  │  └─ SkillCooldown.cs
│  │  ├─ Execution/
│  │  │  ├─ SkillTargeting.cs
│  │  │  ├─ ProjectileExecutor.cs
│  │  │  ├─ LineAttackExecutor.cs
│  │  │  ├─ AreaAttackExecutor.cs
│  │  │  ├─ SingleAttackExecutor.cs
│  │  │  ├─ BuffExecutor.cs
│  │  │  ├─ HealExecutor.cs
│  │  │  ├─ ShieldExecutor.cs
│  │  │  └─ PassiveExecutor.cs
│  │  └─ Actors/
│  │     ├─ SkillActorManager.cs
│  │     ├─ ProjectileActor.cs
│  │     ├─ LineAttackActor.cs
│  │     ├─ AreaAttackActor.cs
│  │     ├─ SingleAttackActor.cs
│  │     └─ BuffActor.cs
│  ├─ Status/
│  │  └─ StatusEffect.cs
│  └─ Effects/
│     └─ EffectManager.cs
├─ Spawn/
│  └─ SpawnManager.cs
└─ UI/
   ├─ MainMenu/
   │  └─ 기존 MainMenu UI 스크립트 재사용·개조
   └─ InGame/
      ├─ InGameUIManager.cs
      ├─ RewardPanelController.cs
      ├─ PrisonPanelController.cs
      ├─ OfferingPanelController.cs
      ├─ ManifestationPanelController.cs
      ├─ MonsterPanel/
      │  └─ 기존 MonsterPanel UI 스크립트 재사용·개조
      ├─ Nexus/
      │  └─ 기존 Nexus UI 스크립트 재사용·개조
      ├─ DamageMeter/
      │  └─ 기존 DamageMeter UI 스크립트 재사용·개조
      ├─ UtilityPanel/
      │  └─ 기존 Auto·시간 배속 UI 스크립트 재사용·개조
      └─ Debug/
         └─ 기존 Debug UI 스크립트 재사용·개조
```

### 19.1 Core

게임 시작 데이터 계층이다.

- `CsvParser`가 유지 대상 CSV를 읽는다.
- Definition은 CSV 용어를 그대로 보관한다.
- `GameDefinitionCatalog`가 검증 완료 Definition을 제공한다.
- `GameBootstrap`이 Manager, Service, 기존 UI를 연결한다.

### 19.2 Run

전투 사이에 유지되는 런 진행 계층이다.

- `RunSessionModel`이 현재 런 상태를 묶는다.
- `StageManager`가 스테이지, 일차, 필드 유닛, Gold, DarkTrace를 관리한다.
- `PartyRoster`가 1P부터 5P까지의 파티 순서를 관리한다.
- `PrisonerInventory`가 수감자를 관리한다.
- 세 Service가 보상, 공양, 현현 규칙을 각각 실행한다.

### 19.3 Units

Definition과 씬 GameObject 사이의 유닛 계층이다.

- Model은 현재 체력, 상태, 스킬 상태를 소유한다.
- Actor는 Transform, Collider, Animation, 씬 표시를 연결한다.
- Model은 UI나 프리팹을 직접 찾지 않는다.

### 19.4 Combat

전투 판단과 실행 계층이다.

- `InGameCombatManager`가 피해, 회복, 방어막, 상태 적용 결과를 조율한다.
- Actions 폴더가 자동 행동, 수동 입력, 적 판단, 이동을 담당한다.
- Skill Runtime은 유닛별 학습·쿨다운 상태를 담당한다.
- Executor는 스킬 종류별 게임 효과를 실행한다.
- Skill Actor는 생성된 스킬의 생명주기를 담당한다.
- `EffectManager`는 시각 효과만 담당한다.

### 19.5 Spawn

Definition을 이용해 유닛 Model과 Actor를 생성하고 `StageManager`에 등록한다.

스테이지 선택, 현현 성공 판정, 파티 제한, 피해 계산은 담당하지 않는다.

### 19.6 UI

현재 씬 계층과 시각 배치를 재사용한다.

UI 스크립트는 새 Core의 읽기 전용 상태를 표시하고 Service 또는 Controller에 명령을 전달한다. Model 필드, 재화, SkillBucket을 직접 변경하지 않는다.

## 20. 코드 작성 요령과 컨벤션

### 20.1 구현 참고 기준

새 코드는 전환 전 `Pakuri/Assets/Scripts`의 실제 코드를 읽기 전용 근거로 검사하고 다음 내용을 참고한다.

- 현재 플레이 동작
- Unity 컴포넌트 연결 방식
- 입력 처리 방식
- 전투 결과 전달 방식
- 기존 UI 오브젝트와 이벤트 연결
- 명명 방식과 중괄호·들여쓰기 형식
- 오류 처리와 Unity 직렬화 경계

기존 Scripts의 거대한 클래스, 중복 상태, 임시 fallback, 우회 호출 구조를 그대로 복사하지 않는다. 동작과 작성 형식을 참고하고 책임 배치는 이 청사진을 따른다.

참고는 소스 의존을 뜻하지 않는다. 새 코드가 기존 타입을 호출, 상속, 래핑하거나 기존 Manager에 결과를 되돌려 보내면 완전 교체 조건을 위반한다.

구현 우선순위:

```text
청사진의 책임 경계
→ 사용자 확정 플레이 규칙
→ CSV 계약
→ 기존 Scripts의 동작과 코드 작성 형식
```

### 20.2 파일과 이름

- 한 파일의 기본 공개 타입 이름과 파일 이름을 일치시킨다.
- 타입, 메서드, 프로퍼티는 `PascalCase`를 사용한다.
- private 필드와 지역 변수는 `camelCase`를 사용한다.
- CSV Definition의 CSV-backed 필드는 사용자 요구에 따라 실제 CSV 열 이름을 그대로 사용한다.
- `UnitDefinotion`, `Skillbucket`, `CSVparser` 같은 오타 이름을 만들지 않는다.
- `Manager`, `Service`, `Controller`, `Model`, `Definition`, `Actor` 접미사는 이 문서의 책임 의미와 일치할 때만 사용한다.
- 의미가 불명확한 `Helper`, `Util`, `Common`, `Temp`, `Data2`, `New` 이름을 사용하지 않는다.

### 20.3 책임 분리

- 클래스 하나는 하나의 상태 권위 또는 하나의 실행 책임을 갖는다.
- Manager는 흐름을 조율하고 하위 객체의 세부 규칙을 다시 구현하지 않는다.
- Model은 자신의 가변 상태를 소유하고 UI나 프리팹을 찾지 않는다.
- Definition은 불변 데이터이며 런타임 상태를 소유하지 않는다.
- Service는 하나의 도메인 규칙을 실행하고 씬 오브젝트를 직접 생성하지 않는다.
- Executor는 스킬 효과를 실행하고 학습 상태나 시각 효과 목록을 소유하지 않는다.
- Actor는 생명주기와 씬 표현을 담당하고 피해 공식이나 스킬 학습 상태를 소유하지 않는다.
- UI는 상태를 표시하고 명령을 전달하며 Core 상태를 직접 변경하지 않는다.

두 번째 독립 책임이 필요해지면 기존 책임과의 경계를 먼저 설명한 뒤 분리한다. 단순히 메서드가 길다는 이유만으로 전달만 하는 wrapper 클래스를 추가하지 않는다.

### 20.4 단일 권위

같은 사실을 둘 이상의 독립적으로 수정 가능한 위치에 저장하지 않는다.

권위 예시:

- Gold와 DarkTrace: `StageManager`
- 파티 순서: `PartyRoster`
- 현재 보상 단계 수감자: `PrisonerInventory`
- 몬스터 학습 상태: 해당 `MonsterSkillBucket`
- 적 스킬 상태: 해당 `EnemySkillBucket`
- 유닛 체력·방어막·상태: 해당 `UnitBaseModel`
- 쿨다운·탄창·재장전: 해당 `SkillCooldown`
- 활성 Skill Actor 목록: `SkillActorManager`
- 불변 게임 데이터: `GameDefinitionCatalog`

UI 표시용 값은 위 권위에서 읽은 projection이어야 한다. UI에 별도 쓰기 가능한 복사본을 만들지 않는다.

### 20.5 직접 경로와 불필요한 우회 금지

다음 구조는 필요한 변환, 생명주기, 의존성 경계가 없다면 만들지 않는다.

```text
A → B → A
A → Wrapper → 실제 A 메서드
Model → 임시 DTO → 같은 Model 복원
Service → Manager → 같은 Service 재호출
```

메서드가 단순 전달만 한다면 해당 경계가 필요한 이유를 코드와 청사진에서 증명해야 한다.

이벤트는 실제 다중 구독, UI 분리, 비동기 생명주기, 재진입 차단이 필요한 경우에만 사용한다.

### 20.6 검증과 fallback

검증은 신뢰할 수 없는 경계에서 한 번 수행한다.

- CSV 입력: `CsvParser`
- ID 연결과 중복: `GameDefinitionCatalog` 생성 전
- Unity Inspector·씬 참조: `GameBootstrap` 또는 해당 Actor 초기화
- 사용자 UI 입력: 해당 UI Controller와 Service 공개 진입점
- 외부 호출 가능한 공개 API: 해당 API 진입점

초기화가 성공한 뒤 내부 호출마다 같은 null, ID, enum, 컬렉션 검사를 반복하지 않는다.

필수 데이터가 없을 때 임의 기본값, 임시 객체, 이전 시스템 fallback으로 조용히 계속하지 않는다. 명확한 오류를 반환하거나 초기화를 실패시킨다.

`manifest_success_chance`, 스킬 수치, 상태효과 수치처럼 CSV가 권위인 값에 코드 fallback 숫자를 추가하지 않는다.

### 20.7 Dead code와 추측성 확장 금지

- 현재 호출자가 없는 공개 API를 미리 만들지 않는다.
- 미래 Save/Load용 인터페이스, 필드, 빈 메서드를 만들지 않는다.
- 현재 활성화되지 않은 상점, 이벤트, 유물 구현용 stub을 만들지 않는다.
- 빈 Executor, 빈 Actor, 빈 Service를 등록하지 않는다.
- 읽는 곳이 없는 필드와 쓰기 전용 상태를 만들지 않는다.
- 사용하지 않는 overload와 범용 문자열 조회 메서드를 만들지 않는다.
- 단 한 번도 실행되지 않는 compatibility branch를 남기지 않는다.
- `TODO`만 있는 실행 경로를 완료된 기능처럼 연결하지 않는다.

새 타입을 추가할 때 다음 네 항목을 먼저 기록한다.

```text
Owner   = 누가 이 타입의 생명주기를 소유하는가
Caller  = 누가 이 타입을 실제 호출하는가
State   = 어떤 상태의 유일한 권위인가
Delete  = 어떤 조건이면 이 타입이 불필요해지는가
```

네 항목 중 하나라도 답이 없으면 타입을 추가하지 않는다.

### 20.8 접근 범위와 의존 방향

- 필드는 기본 `private`.
- 외부 읽기가 필요하면 읽기 전용 프로퍼티나 명확한 조회 메서드를 제공한다.
- 변경은 해당 상태 권위의 명령 메서드로만 수행한다.
- 구현 전용 타입은 `internal`을 우선한다.
- Unity Inspector 연결이 필요한 필드만 `[SerializeField] private`을 사용한다.
- Core Definition은 Unity 씬과 UI에 의존하지 않는다.
- Run은 UI에 의존하지 않는다.
- Combat은 UI 패널에 의존하지 않는다.
- UI는 Core와 Run의 공개 조회·명령 API에 의존한다.
- 서로 참조하는 순환 의존을 만들지 않는다.

### 20.9 중앙 Tick과 Unity 생명주기

- 전투 Tick 진입점은 하나만 둔다.
- `InGameActionManager`가 정해진 순서로 Cooldown, 자동 스킬, 수동 입력, 적 행동, Skill Actor, Status를 Tick한다.
- 각 Skill Actor는 독립 `Update()`를 갖지 않는다.
- `SkillActorManager`만 Skill Actor를 Tick한다.
- Tick 중 컬렉션을 직접 추가·삭제하지 않고 `pendingAdd`, `pendingRemove`를 사용한다.
- 새 Actor는 다음 프레임부터 Tick한다.
- 전투 종료와 다음 일차 전환 시 중앙 Actor 목록과 pending 목록을 모두 비운다.
- UI의 표시용 `Update()`가 Core 게임 상태를 변경하지 않도록 한다.

### 20.10 메서드 작성

- 메서드 하나는 하나의 행동 또는 판정을 수행한다.
- 예상 가능한 실패는 `Try...` 또는 `Can...` 결과로 표현한다.
- 초기화 불변식 위반은 조용히 무시하지 않는다.
- 의미 있는 중간값과 생명주기 캡처는 지역 변수로 유지할 수 있다.
- 기존 값을 이름만 바꿔 복사하는 지역 변수는 만들지 않는다.
- 매 프레임 전체 씬 검색이나 `FindObjectsOfType`를 사용하지 않는다.
- 목록 순서가 게임 규칙이면 정렬·등록 순서를 명시한다.
- 무작위 선택은 후보 목록과 균등·가중 규칙을 호출부에서 명확히 드러낸다.

### 20.11 주석

기존 Scripts처럼 책임과 실행 이유가 필요한 곳에 짧은 한국어 주석을 사용한다.

주석 대상:

- 프레임 실행 순서
- 상태 권위
- 실패 시 소비 여부
- 후보 제외 조건
- 다음 프레임 등록 같은 생명주기 결정
- CSV 필드와 런타임 의미가 다르게 보일 수 있는 지점

코드를 그대로 한국어로 다시 읽는 주석은 만들지 않는다.

### 20.12 Naive Code Filter 대응 체크

구현자는 각 파일을 다음 기준으로 자체 점검한다.

- 모든 타입과 메서드에 실제 호출자가 있는가
- 모든 필드에 필요한 writer와 reader가 있는가
- 같은 상태가 두 곳에서 수정되지 않는가
- 다른 객체로 갔다가 원래 객체로 되돌아오는 불필요한 왕복이 없는가
- 초기화 뒤에도 같은 validation과 fallback을 반복하지 않는가
- 전달만 하는 wrapper가 필요한 경계를 실제로 제공하는가
- 사용하지 않는 overload, 임시 변수, 캐시, compatibility branch가 없는가
- UnityEvent, Inspector, scene, prefab, animation event 같은 동적 참조를 확인했는가
- 삭제 가능한 이전 권위가 새 권위와 함께 영구적으로 남아 있지 않은가

Naive Code Filter는 검사 전용 역할이다. 실제 구현을 자동 승인하거나 수정하지 않는다. 별도 검사 요청이 있을 때 정확한 스크립트 또는 폴더를 대상으로 실행한다.

### 20.13 기존 Scripts 의존 제거 검사

최종 전환 전 다음 검사를 별도 게이트로 수행한다.

- 새 `.cs` 파일의 기존 namespace와 타입 참조 검사
- 씬, 프리팹, `.asset`, AnimatorController를 포함한 Unity 직렬화 파일의 이전 Script GUID 검사
- Missing Script 검사
- Inspector 직렬화 값 이전 전후 비교
- 기존 Manager와 새 Manager의 동시 실행 여부 검사
- 이전 소스를 제거한 상태의 Unity 재컴파일 검사
- 사용자 Play Mode 전체 흐름 검사

이 중 하나라도 실패하면 이전 Scripts 의존 제거와 정상 게임 플레이 구현은 완료되지 않은 상태다.

## 21. 완전 교체 작업 Phase 계획

### 21.1 작업 기록의 단일 위치

이 완전 교체 작업은 다른 활성 작업 보드와 상태를 섞지 않는다.

- 진행 상태, 막힌 이유, 다음 행동, 검사 결과는 이 문서만 갱신한다.
- 루트 `BLACKBOARD.md`를 수정하지 않는다.
- `boards/` 아래 이름에 `BLACKBOARD.md`가 포함된 파일을 수정하지 않는다.
- Phase 진행을 이유로 MON, COMBAT, DATA, RUN, UI, OPS 보드에 중복 기록하지 않는다.
- 프롬프트 초기화, 세션 재시작, 재부팅 뒤에는 이 문서의 `21.11 Phase 실행 기록`에서 마지막 미완료 Phase와 다음 행동을 확인한다.
- 사용자가 별도로 보드 갱신을 명시적으로 요청하면 그 요청 범위만 다시 판단한다.

이 규칙은 `new-core-architecture-blueprint.md` 완전 교체 작업에만 적용한다. 다른 독립 작업의 기록 정책을 변경하지 않는다.

### 21.2 모든 Phase의 공통 실행 게이트

각 Phase는 다음 순서로 진행한다.

```text
Phase 범위 확인
→ 변경 전 Unity Console 상태 확인
→ 해당 Phase 범위만 구현
→ Unity Refresh·재컴파일
→ 컴파일 완료 대기
→ Unity Console Error·Exception·Warning 확인
→ 정적 파일·참조·테스트 검사
→ 필요한 경우에만 사용자 Play Mode 검증
→ Phase 실행 기록 갱신
→ 종료 조건 충족 시 다음 Phase
```

공통 종료 조건:

- Unity 컴파일 오류가 0개다.
- 해당 Phase 변경으로 발생한 새로운 Error와 Exception이 0개다.
- Warning은 원인과 처리 여부를 실행 기록에 남긴다.
- Inspector, 씬, 프리팹 또는 `.asset`을 변경한 Phase는 Missing Script와 필수 참조 누락을 검사한다.
- 실행한 테스트 또는 검사 명령과 실제 결과를 Evidence에 기록한다.
- 실패한 검사를 숨기기 위해 임시 fallback, 빈 컴포넌트 또는 이전 Manager 호출을 추가하지 않는다.
- 종료 조건을 통과하지 못하면 다음 Phase로 넘어가지 않는다.

Console 확인 절차:

1. 변경 전에 현재 Error, Exception, Warning을 읽어 기존 로그 기준선을 기록한다.
2. 이전 로그와 새 로그를 구분할 수 있도록 Console을 정리한다.
3. Asset Refresh와 재컴파일을 실행하고 Unity의 컴파일 완료 상태를 기다린다.
4. Error와 Exception을 먼저 확인하고 Warning을 별도로 확인한다.
5. 오류가 있으면 정확한 스택과 연결된 파일을 근거로 현재 Phase 안에서 해결한다.
6. 해결 뒤 같은 절차로 다시 확인한다.

### 21.3 Play Mode 최소 실행 규칙

Unity Play Mode 게임 플레이 검증은 사용자 소유다. Codex는 임의로 Play Mode를 시작하지 않는다.

Play Mode를 요청할 수 있는 조건:

- 프레임 실행 순서 또는 `Time.deltaTime` 동작을 확인해야 한다.
- Collider, Rigidbody, 충돌 또는 투사체 이동을 확인해야 한다.
- 실제 키보드·마우스 입력과 자동 전투 전환을 확인해야 한다.
- Animator, 이펙트, UI 표시와 버튼 흐름을 실제 씬에서 확인해야 한다.
- 일차 전환, 보상, 공양, 현현처럼 여러 시스템을 통과하는 통합 흐름을 확인해야 한다.
- 최종 기존 Scripts 제거 뒤 전체 게임 플레이를 확인해야 한다.

Play Mode를 요청하지 않는 경우:

- C# 컴파일 오류 확인
- CSV 열, ID, 중복, 참조 무결성 검사
- 새 코드의 기존 namespace와 타입 참조 검사
- Unity 직렬화 파일의 Script GUID 검사
- Missing Script 정적 검사
- 순수 Model, Definition, Service의 결정적 테스트

Play Mode 요청 전 반드시 기록할 내용:

```text
Reason        = 정적 검사로 증명할 수 없는 이유
Scene         = 실행할 정확한 씬
Setup         = 필요한 시작 상태
Actions       = 사용자가 수행할 입력
Expected      = 기대 결과
Failure       = 실패 판정
LogCheck      = 종료 뒤 확인할 Console 로그
```

Play Mode가 필요한 Phase라도 컴파일과 Console 게이트가 먼저 통과해야 한다. 인접 Phase의 검증 시나리오를 하나의 짧은 통합 실행으로 합칠 수 있으면 중복 실행하지 않는다.

### 21.4 Phase 0 — 기준선과 전환 목록 고정

**Task title:** 기존 Scripts 완전 교체 기준선

**Goals:**

- 유지할 CSV와 활성 리소스 목록을 고정한다.
- 기존 Script GUID를 참조하는 씬, 프리팹, `.asset` 전체 목록을 만든다.
- 기존 플레이에서 보존할 동작과 사용자 확정 규칙을 인수 조건으로 고정한다.

**Constraints:**

- 게임 코드, CSV, 씬, 프리팹을 변경하지 않는다.
- 기존 Scripts는 읽기 전용 동작·연결 근거로만 검사한다.
- 현재 집계값 239개 직렬화 파일, 기존 Script 21종, 40개 자산, 56회 참조를 반복 가능한 검사로 재확인한다.

**Role Owner:** Designer가 기준을 확정하고 Code Builder가 반복 가능한 검사 결과를 제공한다.

**Status:** Not Started

**Next Actions:**

- 정확한 이전 Script GUID별 자산 연결표를 작성한다.
- 활성 자산과 Legacy 자산을 분류하되 제거 여부는 근거와 사용자 승인 없이 결정하지 않는다.
- 다음 Phase에서 만들 타입의 Owner, Caller, State, Delete를 확정한다.

**Evidence:** 현재 청사진 18.5절의 직렬화 자산 검사 결과. Phase 실행 시 실제 명령과 출력으로 교체한다.

**History:** 2026-07-23 Phase 계획 생성.

**Play Mode:** 기본 실행하지 않는다. 정적 검사로 확정할 수 없는 기존 플레이 동작이 발견될 때만 사용자에게 기준선 확인을 요청한다.

**Exit Gate:** 유지 자산 목록, 이전 Script 참조 목록, 플레이 호환성 목록이 서로 모순 없이 확정돼야 한다.

### 21.5 Phase 1 — CSV Definition과 Bootstrap 기반

**Task title:** 새 Core 데이터 기반

**Goals:**

- CSV 열 이름을 유지하는 Definition 계층을 구현한다.
- `CsvParser`, `GameDefinitionCatalog`, `GameBootstrap`의 초기화 경계를 구현한다.
- 잘못된 CSV, 중복 ID, 누락 참조가 초기화 단계에서 명확히 실패하게 한다.

**Constraints:**

- `Pakuri/Assets/CSVdata`의 파일, 열 이름, 값을 변경하지 않는다.
- 기존 데이터 타입이나 Parser를 호출하지 않는다.
- 런타임 Model, 전투, UI를 미리 구현하지 않는다.

**Role Owner:** Code Builder

**Status:** Not Started

**Next Actions:** Phase 0 종료 뒤 구체적인 파일 단위 구현 목록을 확정한다.

**Evidence:** 구현 파일, CSV별 파싱 검사, ID 연결 검사, Unity 재컴파일과 Console 결과.

**History:** 2026-07-23 Phase 계획 생성.

**Play Mode:** 실행하지 않는다. 파싱과 카탈로그는 비실행 또는 결정적 테스트로 검증한다.

**Exit Gate:** 유지 대상 CSV가 새 Definition으로 파싱되고, 성공·실패 경로가 기존 타입 없이 검증돼야 한다.

### 21.6 Phase 2 — 런 상태와 유닛 Model

**Task title:** 새 상태 권위 구성

**Goals:**

- `RunSessionModel`, `StageManager`, `PartyRoster`, `PrisonerInventory`를 구현한다.
- `UnitBaseModel`, `MonsterModel`, `EnemyModel`, `NexusModel`을 구현한다.
- SkillBucket, SkillCooldown, 상태효과의 소유권과 수명주기를 구현한다.

**Constraints:**

- Model은 씬, 프리팹, UI를 탐색하지 않는다.
- Gold, DarkTrace, 파티, 수감자, 체력, 스킬 학습 상태를 둘 이상의 객체가 수정하지 않는다.
- 아직 기존 씬 컴포넌트를 교체하지 않는다.

**Role Owner:** Code Builder

**Status:** Not Started

**Next Actions:** Phase 1의 Definition과 공개 조회 API를 입력으로 파일 단위 구현 범위를 확정한다.

**Evidence:** 상태 전이 테스트, 상태 권위 검사, Unity 재컴파일과 Console 결과.

**History:** 2026-07-23 Phase 계획 생성.

**Play Mode:** 실행하지 않는다. 순수 상태 전이는 결정적 테스트로 검증한다.

**Exit Gate:** 각 가변 상태의 writer가 하나이며 새 Model이 기존 런타임 타입 없이 동작해야 한다.

### 21.7 Phase 3 — 중앙 전투와 행동·이동

**Task title:** 새 전투 실행 루프

**Goals:**

- `InGameActionManager` 중앙 Tick을 구현한다.
- 피해, 대상 지정, 자동·수동 스킬, 적 행동, 이동, 상태효과 실행 순서를 구현한다.
- Executor, SkillActorManager, Skill Actor, EffectManager 사이의 책임을 구현한다.

**Constraints:**

- Skill Actor는 독립 `Update()`를 사용하지 않는다.
- 기존 Combat Manager, Executor, Actor 또는 입력 Controller를 호출하지 않는다.
- Tick 중 컬렉션 변경은 pending 목록을 사용한다.

**Role Owner:** Code Builder

**Status:** Not Started

**Next Actions:** 스킬 family별 최소 수직 경로와 행동·이동 호출 순서를 구현 단위로 확정한다.

**Evidence:** Tick 순서 검사, 피해·쿨다운·대상 결정 테스트, Unity 재컴파일과 Console 결과.

**History:** 2026-07-23 Phase 계획 생성.

**Play Mode:** 조건부다. 물리 충돌, 프레임 시간, 마우스 조준처럼 정적·결정적 테스트로 증명할 수 없는 항목만 사용자에게 제한된 씬 시나리오를 요청한다.

**Exit Gate:** 중앙 Tick 하나만 전투 상태를 변경하며 각 활성 스킬 family의 실행과 종료가 기존 타입 없이 검증돼야 한다.

### 21.8 Phase 4 — 스테이지·스폰·보상 Service

**Task title:** 새 런 진행 흐름

**Goals:**

- 스테이지와 일차 전환, 적 스폰, 승리·패배, 회차 초기화를 구현한다.
- Reward, Offering, Manifestation 흐름을 구현한다.
- 확정된 수감자 소비, 후보 균등 선택, 영입·넘기기 규칙을 구현한다.

**Constraints:**

- 런 상태는 Phase 2의 권위만 변경한다.
- Service가 씬 오브젝트를 직접 찾거나 UI를 직접 변경하지 않는다.
- 기존 StageManager, RunSession, SpawnManager, UI Manager를 fallback으로 호출하지 않는다.

**Role Owner:** Code Builder

**Status:** Not Started

**Next Actions:** Phase 2 상태 권위와 Phase 3 전투 종료 신호를 연결하는 공개 API를 확정한다.

**Evidence:** 일차·스테이지 상태 전이, 보상 후보, 수감자 소비 테스트, Unity 재컴파일과 Console 결과.

**History:** 2026-07-23 Phase 계획 생성.

**Play Mode:** 기본 실행하지 않는다. 여러 시스템의 실제 씬 생명주기에서만 발생하는 문제가 남으면 Phase 5 통합 검증과 합쳐 한 번 실행한다.

**Exit Gate:** 전투 시작부터 보상 종료와 다음 일차 진입까지의 상태 전이가 UI 없이 검증돼야 한다.

### 21.9 Phase 5 — 현재 리소스 재연결

**Task title:** 씬·프리팹·UI·시각 리소스 마이그레이션

**Goals:**

- 현재 씬 계층과 UI 오브젝트를 새 Controller와 Service API에 연결한다.
- 활성 유닛과 스킬 프리팹을 새 Actor와 시각 경계에 연결한다.
- 스프라이트, 애니메이션, AnimatorController, Inspector 값을 새 소유자에게 이전한다.
- `CsvRuntimeCatalog.asset`을 새 초기화 경계에 맞게 교체한다.

**Constraints:**

- 현재 플레이에 필요한 시각 리소스를 근거 없이 새로 만들거나 교체하지 않는다.
- 기존 컴포넌트와 새 컴포넌트를 동일 상태 권위로 동시에 실행하지 않는다.
- 직렬화 필드 값은 이전 전후 대응표를 남긴다.

**Role Owner:** Code Builder가 자산 연결을 수행하고 사용자가 Play Mode 게임 플레이를 검증한다.

**Status:** Not Started

**Next Actions:** Phase 0의 자산 연결표를 새 컴포넌트별 이전 체크리스트로 변환한다.

**Evidence:** 씬·프리팹·`.asset` 참조 검사, Missing Script 검사, Inspector 대응표, Unity 재컴파일과 Console 결과.

**History:** 2026-07-23 Phase 계획 생성.

**Play Mode:** 필요하다. 컴파일과 정적 연결 검사가 성공한 뒤 사용자에게 입력, 전투, UI, 이펙트, 스테이지 전환을 묶은 정확한 통합 시나리오를 요청한다.

**Exit Gate:** 활성 게임 흐름이 새 Core와 새 컴포넌트만으로 실행되고 현재 리소스가 정상 표시돼야 한다.

### 21.10 Phase 6 — 기존 Scripts 제거와 최종 전환

**Task title:** 기존 의존 0과 최종 인수

**Goals:**

- 모든 이전 Script GUID 참조를 0으로 만든다.
- 불필요해진 기존 `.cs`와 Legacy 직렬화 자산을 승인된 방식으로 제거하거나 Unity 컴파일 대상 밖으로 이동한다.
- 새 Core만 남긴 상태에서 전체 게임 플레이를 인수한다.

**Constraints:**

- 소스 삭제나 `Assets` 밖 이동은 정확한 대상 목록과 사용자 승인 뒤 수행한다.
- 이전 코드 fallback, compatibility component, 빈 대체 컴포넌트를 남기지 않는다.
- 완료 판정은 18.6절과 20.13절을 모두 만족해야 한다.

**Role Owner:** Code Builder가 정적 전환과 로그 증거를 제공하고 사용자가 최종 Play Mode를 검증한다.

**Status:** Not Started

**Next Actions:** Phase 5 성공 뒤 제거 대상과 보존 대상의 정확한 경로를 제시하고 승인을 받는다.

**Evidence:** 기존 타입 참조 0, 이전 Script GUID 참조 0, Missing Script 0, Unity 재컴파일, Console, 최종 사용자 Play Mode 결과.

**History:** 2026-07-23 Phase 계획 생성.

**Play Mode:** 필요하다. 이전 소스 제거와 재컴파일이 성공한 뒤 최종 전체 런 시나리오를 사용자에게 요청한다.

**Exit Gate:** 기존 Scripts 없이 컴파일되고, Console 오류가 없으며, 사용자가 정상 게임 플레이를 확인해야 한다.

### 21.11 Phase 실행 기록

구현이 시작되면 이 절 아래에 최신 기록을 위로 추가한다. 아직 구현이 시작되지 않았으므로 실행 기록은 없다.

기록 형식:

```text
## Phase Record — YYYY-MM-DD HH:mm

Task title:
Goals:
Constraints:
Role Owner:
Status:
Next Actions:
Changed Paths:
Evidence:
Unity Before Log:
Unity Compile Result:
Unity Error/Exception:
Unity Warning:
Play Mode:
Play Mode Reason:
User Result:
History:
```

`Play Mode` 값은 다음 중 하나만 사용한다.

```text
Not Run
Requested From User
Completed By User
Failed By User Evidence
```

실행하지 않았다면 `Play Mode Reason`에 정적 검사만으로 충분했던 이유를 기록한다. 요청했다면 21.3절의 Reason, Scene, Setup, Actions, Expected, Failure, LogCheck를 함께 기록한다.
