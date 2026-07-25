# New Core 아키텍처 청사진 — 설계 본문 한국어판

## 1. 핵심 설계 원칙

핵심 책임을 다음과 같이 분리한다.

```text
Definition       = 무엇인가
Model            = 현재 어떤 상태인가
SkillBucket      = 무엇을 학습했는가
SkillCooldown    = 지금 사용할 수 있는가
SkillTargeting   = 누구에게 사용할 것인가
Executor         = 무엇을 실행할 것인가
Actor            = 생성된 스킬이 언제 끝나는가
CombatManager    = 결과를 전투에 어떻게 반영하는가
EffectManager    = 어떻게 보이는가
StageManager     = Run이 어디까지 진행됐고 재화가 얼마인가
```

게임 시작 시 CSV를 한 번 파싱해 Definition을 만들고, 이후 불변 데이터로 취급한다.

각 `MonsterModel`은 자기 `MonsterSkillBucket`을 직접 소유한다. 다른 Manager가 몬스터의 액티브·패시브·강화·마스터 학습 상태를 대신 소유하지 않는다.


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

`SkillDefinition`은 공통 기본 스킬 정보를 정의한다.

공통 후보 필드:

- `skill_id`
- `monster_id`
- `slot`
- `display_name`
- `runtime_kind`
- `description_text`
- `summary`

파생 Definition은 대응 CSV에 실제 존재하는 정확한 컬럼명을 사용한다.

예를 들어 `ProjectileDefinition`은 다음 projectile CSV 용어를 사용한다.

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

Enemy Skill CSV에는 `heal`, `shield` 파일이 별도로 존재한다. 전체 CSV를 보존하려면 `HealDefinition`, `ShieldDefinition`이 필요하다.

### 2.2 Choice와 Node Definition

실제 CSV 계약은 Choice, Graph Node, Node Type, Node Parameter로 분리된다.

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
- `arg_1`~`arg_12`
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

Monster와 Enemy Trigger CSV 파싱에 다음 Definition이 필요하다.

```text
SkillTriggerDefinition
```

실제 Trigger CSV 용어:

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

Trigger CSV마다 실제 컬럼 구성이 다르므로, 특정 파일에 없는 필드가 존재한다고 가정하지 않는다.

### 2.4 Unit Definition

클래스 이름은 `UnitDefinotion`이 아니라 `UnitDefinition`으로 기록한다.

```text
UnitDefinition
├─ MonsterDefinition
└─ EnemyDefinition
```

`UnitDefinition` 공통 유닛 데이터:

- 최대 체력
- 공격력
- 주문력
- 이동 속도
- 치명타 확률
- 치명타 피해
- 치명타 저항
- 물리·불·번개·얼음·어둠·신성 방어력

`MonsterDefinition`은 Monster CSV 용어를 사용한다.

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

`EnemyDefinition`은 Enemy CSV 용어를 사용한다.

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

세 실제 CSV에 맞춰 Stage 데이터를 구성한다.

```text
StageDefinition
├─ StageDayDefinition
├─ StageEncounterDefinition
└─ StageRewardDefinition
```

`StageDayDefinition`:

- Day와 전투 유형 정의
- Encounter와 Reward 규칙 연결
- Shop, Event, Elite Choice 필드 보존

`StageEncounterDefinition`:

- 적 Spawn 순서
- 적 종류와 수
- Spawn 간격과 위치
- Boss 후보와 확정 Boss
- 확정 포로

`StageRewardDefinition`:

- Gold
- DarkTrace
- 포로 수 확률
- 현현 성공 확률
- 추가 Elite 포로
- 유물 Choice 수

## 3. 시작 시 CSV 파싱

```text
CsvParser.cs
    ↓
GameDefinitionCatalog
```

### 3.1 CsvParser

책임:

- 게임 시작 시 CSV를 한 번 파싱
- CSV 컬럼명을 변경하지 않고 읽기
- CSV 유형에 맞는 Definition 생성
- ID 참조 해결
- 중복 ID 검사
- 누락 참조 검사
- 잘못된 Enum과 숫자 검증
- 잘못된 Definition을 조용히 추측하거나 수정하지 않기

소유하지 않는 책임:

- 피해 계산
- 스킬 실행
- 유닛 생성
- Run 진행
- 스킬 학습 상태
- 시각 이펙트 생성

### 3.2 GameDefinitionCatalog

책임:

- 파싱된 Definition 저장
- ID로 Definition 조회
- 참조가 해결된 불변 게임 데이터 제공

현재 체력, 상태 효과, 쿨다운, 학습 상태는 저장하지 않는다.

### 3.3 초기화 순서

```text
Status Definition
→ NodeType / NodeParam Definition
→ Skill / Choice / ChoiceNode / Trigger Definition
→ Monster / Enemy Definition
→ Stage Definition
→ 모든 ID 참조 검증
→ GameDefinitionCatalog 확정
```

## 4. Runtime Model 계층

```text
UnitBaseModel
├─ MonsterModel
├─ EnemyModel
└─ NexusModel
```

### 4.1 UnitBaseModel

모든 유닛의 공통 런타임 값을 보유한다.

- Definition 참조
- 현재 체력
- 현재 보호막
- 현재 능력치
- 생존 상태
- 현재 위치
- 현재 상태 효과
- 현재 쿨다운 상태

### 4.2 MonsterModel

책임:

- `MonsterDefinition` 참조
- `MonsterSkillBucket` 소유
- 자동 공격 상태
- 자동 스킬 상태
- 자기 상태 효과와 자원 상태
- 다음 라운드를 위한 자기 상태 초기화

다른 Manager가 몬스터의 학습 스킬 목록을 대신 소유하지 않는다.

### 4.3 EnemyModel

책임:

- `EnemyDefinition` 참조
- `EnemySkillBucket` 소유
- 적에게 할당된 액티브·패시브 상태
- 자기 상태 효과와 자원 상태
- 생존과 Nexus 접촉 상태

### 4.4 NexusModel

책임:

- 현재 체력
- 최대 체력
- 생존 상태
- 받은 Nexus 피해 적용

### 4.5 RunSessionModel

한 Run에서 전투 사이에 유지되는 진행 상태를 저장한다.

책임:

- 현재 Stage ID
- 현재 Day
- 현재 Encounter ID
- `PartyRoster` 참조
- `PrisonerInventory` 참조
- 현재 보상 처리 상태
- Run 승리·패배 상태

사용자 명세에 따라 Gold와 DarkTrace는 `RunSessionModel`이 아니라 `StageManager`가 관리한다.

소유하지 않는 책임:

- 피해 계산
- 유닛 행동
- 스킬 실행
- UI 표현
- CSV 파싱

### 4.6 PartyRoster

Run 안의 순서 있는 Monster Party를 관리한다.

책임:

- 처음 선택한 Monster를 첫 Slot에 등록
- 현현 Monster를 다음 빈 Slot에 등록
- 파티 순서 보존
- 최대 파티 Slot 제한
- 중복 Monster 등록 방지
- 파티원 추가 가능 여부 제공
- Monster ID로 파티원 조회

현재 필드 생존 여부는 판단하지 않는다. 현재 필드 전체 유닛과 생존 유닛은 `StageManager`가 관리한다.

현재 UI의 1P~5P 순서는 `PartyRoster` 순서를 그대로 사용한다. UI가 선택 Monster와 현현 Monster 목록을 다시 조합하지 않는다.

### 4.7 PrisonerInventory

전투 보상으로 얻은 포로를 관리한다.

책임:

- `enemy_id`로 포로 등록
- 보유 포로 조회
- 현현 또는 Offering용 포로 선택
- 소비 가능 여부 확인
- 포로 소비
- 이미 소비한 포로 재사용 방지
- 새 전투 보상 생성 시 이전 목록 제거
- 다음 Day 진행 시 남은 포로 제거

현현 성공 여부나 Offering 후보를 만들지 않는다.

기존 Scripts에서 포로는 현재 전투의 보상 단계에서만 유지되며 Run 전체 자원으로 누적되지 않는다. 미사용 포로는 다음 Day로 넘어가지 않는다.

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

Monster 학습 상태를 바꾸는 최종 권한은 해당 Monster의 `MonsterSkillBucket`이다.

### 5.2 EnemySkillBucket

각 `EnemyModel`이 하나씩 소유한다.

- 적에게 할당된 액티브
- 적에게 할당된 패시브
- 사용 가능한 스킬
- 적 스킬 Slot 제한

## 6. SkillCooldown

각 Model이 참조하며 스킬 사용 조건을 판정하는 객체다.

책임:

- 현재 쿨다운
- 탄창
- 재장전
- 발사 간격
- 사용 가능 여부 판정
- `CanUse()` 결과 반환
- 스킬 사용 후 런타임 상태 갱신
- 다음 라운드 초기화

소유하지 않는 책임:

- 대상 탐색
- 피해 계산
- 상태 효과 적용
- 시각 이펙트 생성
- 스킬 학습 상태 변경

## 7. SkillTargeting

자동 모드에서 스킬 대상을 찾는다.

책임:

- `target_selection` 적용
- `target_scope` 적용
- `radius` 적용
- 현재 생존 유닛 중 후보 사용
- 최종 대상 또는 대상 목록 반환

피해를 계산하거나 스킬을 실행하지 않는다.

수동 타기팅은 사용자 확정 요구와 검사한 호환 동작에 따라 8.6절에서 정한다.

## 8. 행동과 이동 구조

### 8.1 InGameActionManager

전투 중 Action Controller의 실행 순서를 조율한다.

책임:

- `StageManager`의 전투 진행 상태 확인
- 생존 유닛 Action Controller 갱신
- 플레이어 입력과 자동 행동 처리 순서 조율
- 행동 불가 유닛 실행 차단
- `SkillCooldown`, `SkillTargeting`, Executor 호출 흐름 연결

피해 계산과 스킬 학습 상태를 소유하지 않는다.

### 8.2 UnitActionController

```text
UnitActionController
├─ MonsterActionController
└─ EnemyActionController
```

공통 책임:

- 할당된 `UnitBaseModel` 참조
- 생존 상태 확인
- 이동·행동·특수 스킬 가능 여부 확인
- 사용 가능한 스킬 조회
- `SkillCooldown.CanUse()` 호출
- 대상 준비 시 Executor 실행 요청

### 8.3 MonsterActionController

책임:

- `MonsterModel`, `MonsterSkillBucket` 참조
- 선택 Monster의 수동·자동 스킬 상태 확인
- 현현 Monster의 자동 행동 처리
- 자동 모드에서 `SkillTargeting`으로 대상 선정
- 사용 가능한 스킬의 Executor 요청

수동 입력은 `PlayerInputController`가 전달한다. UI Button을 직접 찾지 않는다.

### 8.4 EnemyActionController

책임:

- `EnemyModel`, `EnemySkillBucket` 참조
- 공격 가능한 Player Unit 탐색
- 공격 대상이 없으면 Nexus 선택
- 사거리 밖이면 `UnitMovementController`에 이동 요청
- 사거리 안이면 사용 가능한 스킬 실행
- Nexus 접촉 조건 충족 시 `nexus_damage` 적용 요청

적 종류와 Spawn 시점은 결정하지 않는다. 해당 책임은 `StageManager`, `SpawnManager`에 있다.

### 8.5 UnitMovementController

이동 가능한 유닛의 위치 변경만 처리한다.

책임:

- 현재 위치와 목표 위치 확인
- 이동 속도 적용
- 상태 효과의 이동 가능 여부 적용
- `deltaTime`에 따른 위치 갱신
- 목표 도달 여부 반환

대상 선정, 공격, 피해 계산, 스킬 실행을 하지 않는다.

현재 게임의 적 이동을 처리한다. Monster 이동 규칙은 사용자 요구로 확정되지 않았으므로 기본 이동 주체로 설정하지 않는다.

### 8.6 PlayerInputController

선택 Player Monster의 수동 입력과 자동 상태 변경을 처리한다.

책임:

- 선택 Monster 식별
- 자동 스킬 상태 변경
- 수동 스킬 사용 요청
- UI 입력을 `MonsterActionController`에 전달

수동 스킬 타기팅 규칙:

- Party Slot 0의 선택 Monster만 수동 입력을 받는다.
- 선택 Monster의 Auto Skill이 꺼진 동안만 수동 입력을 처리한다.
- 마우스 왼쪽 버튼을 사용한다.
- Pointer가 UI 위에 있으면 전투 입력을 무시한다.
- 마우스 Screen 좌표를 Combat World 좌표로 변환한다.
- 선택 Monster 위치에서 World Aim Point로 향하는 방향을 `aimDirection`으로 사용한다.
- World Aim Point를 `targetPoint`로 사용한다.
- 비투사체 스킬은 버튼을 누른 Frame의 Aim Point로 한 번 실행을 시도한다.
- 투사체 스킬은 버튼을 누르는 동안 최신 Aim을 사용한다.
- Burst 투사체는 버튼을 놓아도 마지막 저장 Aim으로 남은 Shot을 계속한다.
- Area Center가 필요한 스킬은 수동 `targetPoint`를 우선한다.
- 수동 Aim Point가 없을 때만 자동 `target_selection`을 사용한다.

수동 입력은 Aim 방향과 Point만 제공한다. 최종 피격 대상과 범위 포함 여부는 Executor와 `SkillTargeting`이 판단한다.

### 8.7 행동 실행 순서

```text
1. Passive 변경 적용
2. 등록된 모든 유닛의 Cooldown, Magazine, Reload Tick
3. 등록 순서와 Active Skill 목록 순서로 Player·현현 Monster 자동 스킬 시도
4. 선택 Monster 수동 입력 처리
5. 적 등록 순서로 적 행동 처리
6. SkillActorManager가 등록 순서로 현재 활성 Skill Actor Tick
7. 모든 유닛 상태 효과 지속시간과 만료 처리
8. 상태 변경으로 생긴 최종 Passive 변경 적용
```

적 하나의 Frame 내부 행동 순서:

```text
1. 사망과 AutoAttackEnabled 확인
2. 진행 중 Charge Action Tick
3. 가장 가까운 생존 Player 탐색
4. Player가 없으면 Nexus 선택
5. 사용 가능한 Support 계열 B 스킬 우선 시도
6. 공격형 B 스킬 우선, B 사용 불가면 A 선택
7. 사거리 밖이면 이동
8. 사거리 안이고 행동 가능하면 선택 스킬 실행
9. Nexus 접촉 시 nexus_damage 적용 후 적 제거
```

이 순서는 기존 `InGameCombatManager.Update()`, `SkillExecution.TryExecuteAutomaticSkills(...)`, `PlayerCombatInputController.HandleManualInput(...)`, `EnemyActionController.Tick(...)`의 실제 호출 순서를 호환 기준으로 사용한다.

새 구조에서는 Skill Actor마다 독립 Unity `Update()`를 두지 않는다. `InGameActionManager`가 Frame당 한 번 Tick하고 6단계에서 `SkillActorManager.Tick(deltaTime)`을 호출한다.

현재 Frame에 생성한 Skill Actor는 `pendingAdd`에 등록하고 다음 Frame부터 Tick한다. Tick 중 종료된 Actor는 `pendingRemove`에 등록하고 반복 종료 후 제거한다. 이를 통해 Collection 변경과 생성 Frame 중복 Tick을 방지한다.

## 9. Skill Executor

스킬 유형마다 Executor를 제공한다.

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

책임:

- Skill Definition 읽기
- 시전자 `SkillBucket`의 학습 내용 읽기
- 적용 가능한 Choice와 Node 조합
- Trigger 조건 적용
- 실제 스킬 실행 결과 생성
- 필요한 Actor 생성 요청
- `InGameCombatManager`에 피해·회복·보호막·상태 효과 적용 요청

스킬 학습 상태를 직접 변경하지 않는다.

Stage 진행이나 시각 이펙트 삭제를 처리하지 않는다.

## 10. Skill Actor

생성된 스킬의 런타임 수명주기를 추적한다.

```text
ProjectileActor
LineAttackActor
AreaAttackActor
SingleAttackActor
BuffActor
```

책임:

- 생성 시점 기록
- 이동
- 지속시간
- 충돌
- 적중 처리
- 종료 조건
- 종료 시 스킬 효과 종료 신호
- `EffectManager`에 시각 이펙트 삭제 신호

피해 공식을 소유하지 않는다.

Monster 학습 상태를 소유하지 않는다.

독립 Unity `Update()`를 갖지 않으며 공통 메서드로만 갱신한다.

```text
Tick(float deltaTime)
```

### 10.1 SkillActorManager

현재 활성 Skill Actor의 중앙 수명주기를 관리한다.

책임:

- 생성된 Skill Actor 등록 요청 수신
- 다음 Frame 등록용 `pendingAdd` 관리
- 등록 순서로 활성 Actor Tick
- 종료 Actor용 `pendingRemove` 관리
- 반복 종료 후 Actor 제거
- Actor 제거 시 `EffectManager`에 시각 이펙트 삭제 요청
- 전투 종료와 다음 라운드 전환 시 모든 Actor 제거

피해 계산, 대상 선정, 스킬 학습 상태를 처리하지 않는다.

`SkillActorManager.Tick(deltaTime)`은 `InGameActionManager`만 호출한다. 다른 Manager나 UI가 직접 Tick하지 않는다.

## 11. InGameCombatManager

인게임 전투 결과를 조율한다.

책임:

- 스킬 실행 요청 수신
- 적절한 Executor 호출
- 최종 피해 계산
- 피해 적용
- 회복 적용
- 보호막 적용
- 상태 효과 적용 요청
- 적중·처치·스킬 활성화 이벤트 발행

금지 책임:

- 스킬 학습 상태 소유
- 적 Spawn
- 현현 Monster Spawn
- Stage와 Day 진행
- Prefab 수명주기 관리
- Run 보상 소유

모든 로직을 직접 구현하는 거대 객체가 아니라 전투 실행을 연결하는 조율자다.

## 12. SpawnManager

인게임 유닛 생성을 담당한다.

책임:

- 적 Spawn
- 현현 Monster Spawn
- Definition으로 Model 생성
- Model과 Scene Actor 연결
- 생성 유닛을 `StageManager` 필드 유닛 목록에 등록

금지 책임:

- 어느 Day에 어떤 적이 나오는지 결정
- 피해 계산
- 스킬 학습
- 다음 Stage 결정

## 13. StageManager

`RunSessionModel`, Player 재화, 현재 필드 진행을 담당한다.

책임:

- 활성 `RunSessionModel` 소유
- 현재 Stage·Day·Encounter
- Player `Gold`, `DarkTrace` 관리
- 재화 추가·소비 가능 여부·소비
- 전체 필드 유닛과 현재 생존 유닛
- Day 시작
- 적 Spawn 순서 진행
- 전투 완료 판정
- 다음 Day·Stage 진행
- 승리·패배
- 라운드 전환 시 유닛 초기화 요청
- `SpawnManager`에 유닛 생성 명령

재화 변경은 `StageManager` 메서드로만 수행한다.

```text
AddGold(amount)
CanSpendGold(amount)
SpendGold(amount)
AddDarkTrace(amount)
CanSpendDarkTrace(amount)
SpendDarkTrace(amount)
```

`RewardService`, `OfferingService`, `ManifestationService`, UI는 Gold와 DarkTrace 필드를 직접 변경하지 않는다.

`PartyRoster`, `PrisonerInventory` 규칙을 직접 구현하지 않는다. 활성 `RunSessionModel`을 통해 접근하며 각 객체와 Service가 실제 규칙을 소유한다.

유닛 내부 필드를 직접 초기화하지 않고 각 Model에 요청한다.

```text
monsterModel.ResetForNextDay()
enemyModel.ResetForNextDay()
```

검사한 `MonsterDayRecovery.cs`에 따른 라운드 초기화 대상:

- 상태 효과 제거
- 직접 보호막 제거
- 현재 보호막 제거
- 스킬 런타임 상태 초기화
- 체력 완전 회복
- 자동 공격 활성화
- 선택 Monster가 아닌 경우 자동 스킬 활성화

`StageManager`는 유닛을 순회하며 초기화를 요청하고, 각 Model이 자기 상태를 직접 초기화한다.

### 13.1 RewardService

전투 종료 후 `StageRewardDefinition`을 실제 Run 보상으로 변환해 지급한다.

책임:

- 현재 Stage와 전투 유형에 맞는 `StageRewardDefinition` 조회
- Gold·DarkTrace 보상 계산
- 포로 수 결정
- Boss와 확정 포로 규칙 적용
- `StageManager`를 통해 Gold·DarkTrace 지급
- `PrisonerInventory`에 포로 등록
- UI 표시용 보상 결과 반환

UI Button을 만들지 않고 현현이나 Offering을 실행하지 않는다.

### 13.2 OfferingService

선택 Party Monster의 Offering 후보를 만들고 선택 결과를 적용한다.

책임:

- 대상 Monster가 `PartyRoster`에 있는지 확인
- 대상 `MonsterSkillBucket` 조회
- 기존 출현 규칙에 따른 학습 가능 액티브 후보 생성
- 기존 출현 규칙에 따른 학습 가능 패시브 후보 생성
- 기존 출현 규칙에 따른 선택 가능 강화·마스터 후보 생성
- 모든 적격 후보를 하나의 목록으로 결합
- 동일 가중치로 균등 Shuffle
- Shuffle 결과 앞의 최대 3개만 반환
- 선택 완료 전까지 생성한 후보 Set 유지
- Reroll 제공 금지
- 해당 `MonsterSkillBucket`에 선택 결과 적용
- 사용한 포로를 `PrisonerInventory`에서 소비

전투 스킬 효과를 실행하지 않고 학습 결과만 변경한다.

기존 출현 적격 규칙:

- 기본 A 외 추가 액티브 최대 2개
- 패시브 최대 5개
- 선행 액티브가 필요한 패시브는 해당 액티브 학습 후에만 적격
- 대상 액티브당 강화 최대 3개
- 액티브 마스터는 대상 액티브 강화 3개를 모두 선택한 뒤 적격이며 최대 1개
- 대상 패시브당 패시브 강화 최대 1개
- 이미 학습·선택한 항목 제외

Offering Button 입력 시 위 규칙을 통과한 모든 후보를 균등 Shuffle하고 최대 3개 표시한다. 유형별 추가 가중치나 고정 비율은 없다.

적격 후보가 없으면 포로를 소비하지 않는다. 후보 하나를 확정할 때만 소비한다.

### 13.3 ManifestationService

포로를 사용해 새 Monster를 Party에 추가하는 규칙을 소유한다.

책임:

- 선택 포로 소유 여부 확인
- `PartyRoster`의 다음 빈 Slot 탐색
- 중복 Monster와 최대 Party 제한 확인
- 현현 시도 시작 즉시 포로 소비
- `enemy_id`와 무관하게 현재 Party에 없는 모든 Player Monster로 후보 구성
- 모든 현현 후보에서 동일 확률로 무작위 선택
- 현재 보상의 `manifest_success_chance` 적용
- 실패 시 소비한 포로 반환 금지
- 성공 시 `PartyRoster`에 현현 Monster 등록
- 성공·실패 결과 반환
- 성공 Monster 모집 확정 시 `StageManager`를 통해 `SpawnManager`에 즉시 배치 요청
- 다음 라운드를 기다리지 않고 현재 필드의 다음 Party Slot에 배치

Prefab을 직접 인스턴스화하지 않는다.

포로 `enemy_id`와 현현 가능 `monster_id` 사이에 직접 매핑은 없다. `enemy_id`는 UI 표시와 재료 식별에만 사용한다.

현현 성공 후 기존 UI처럼 모집 또는 건너뛰기를 선택한다. 모집하면 즉시 Party 등록과 Scene Spawn을 실행한다. 건너뛰면 포로는 소비된 채 Monster를 추가하지 않는다.

### 13.4 Run 보상 흐름

```text
전투 종료
→ StageManager가 Reward 상태 진입
→ RewardService가 보상 생성
→ StageManager를 통해 Gold·DarkTrace 지급
→ PrisonerInventory에 포로 등록
→ 사용자가 포로 선택
→ OfferingService 또는 ManifestationService 실행
→ 다음 Day 진행
```

## 14. StatusDefinition과 StatusEffect

### 14.1 StatusDefinition

`status_effects.csv`의 상태 효과 원본 Definition을 저장한다.

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

특정 유닛에 실제 적용된 상태를 저장하고 실행한다.

- `StatusDefinition` 참조
- 남은 지속시간
- 현재 Stack
- 적용 유닛
- 영향받는 유닛
- 효과 적용
- 효과 갱신
- Stack 처리
- 효과 제거

`StatusDefinition`은 불변 데이터이고 `StatusEffect`는 전투 중 변경되는 상태다.

## 15. EffectManager

스킬 런타임 시각 효과와 Prefab 시각 효과를 관리한다.

책임:

- 시각 이펙트 생성
- Prefab 인스턴스화
- Actor별 Effect Handle 반환
- 위치·방향 갱신
- Actor 또는 Executor의 종료 신호 처리
- 시각 이펙트 제거

금지 책임:

- 피해 결정
- 상태 효과 결정
- 스킬 사용 가능 여부 결정
- 대상 선정
- 스킬 학습 상태 소유

## 16. 기존 UI 구조 재사용과 변경 경계

현재 `NewRunScene` UI 계층과 시각 Layout을 재사용한다. UI를 재설계하지 않고, 기존 UI가 읽고 호출하던 객체를 새 Core API로 교체한다.

재사용 대상:

- Main Menu와 Run 진입 흐름
- RewardPanel
- PrisonPanel
- 1P~5P Party Slot
- Offering UI
- 현현 성공·실패 UI
- Monster Panel
- Nexus 체력 표시
- Damage Meter
- Auto Button
- Game Speed Button
- DebugPanel

UI 변경 원칙:

- UI는 `UnitBaseModel`, `MonsterSkillBucket`, 재화 필드를 직접 변경하지 않는다.
- Gold·DarkTrace는 `StageManager`의 읽기 전용 값으로 표시한다.
- Party Slot은 `RunSessionModel.PartyRoster` 순서를 그대로 표시한다.
- 포로 목록은 `RunSessionModel.PrisonerInventory`에서 표시한다.
- Reward UI는 `RewardService` 결과를 표시한다.
- Offering UI는 `OfferingService` 후보를 받고 선택 명령만 보낸다.
- 현현 UI는 `ManifestationService`에 실행 명령을 보내고 결과만 표시한다.
- Auto UI는 `PlayerInputController`를 통해 선택 Monster 상태를 변경한다.
- Damage Meter는 `InGameCombatManager` 확정 피해 이벤트를 구독한다.
- Nexus UI는 `NexusModel` 상태를 표시한다.
- `EffectManager`는 Panel UI가 아니라 스킬 시각 효과만 관리한다.

기존 Scene Object 이름과 Layout은 유지할 수 있다. 이전 상태 객체나 Manager 내부 필드를 직접 읽는 기존 UI Script는 새 `StageManager`, `RunSessionModel`, `PartyRoster`, Service API를 사용하도록 변경한다.

### 16.1 GameBootstrap

게임 시작과 새 Core 연결을 위한 진입점이다.

책임:

- `CsvParser` 실행
- `GameDefinitionCatalog` 생성
- `RunSessionModel` 생성
- `StageManager` 초기화
- `InGameCombatManager`, `InGameActionManager` 초기화
- `RewardService`, `OfferingService`, `ManifestationService` 생성·연결
- 기존 UI Controller를 새 Core Query·Command API에 연결

게임 규칙을 직접 구현하지 않는다.

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
→ UnitBaseModel 파생 Model 생성
→ SkillBucket 생성
→ 필드 유닛 등록
```

### 17.3 자동 스킬 실행

```text
InGameActionManager
→ MonsterActionController 또는 EnemyActionController
→ SkillBucket에서 사용 스킬 조회
→ SkillCooldown.CanUse()
→ SkillTargeting으로 대상 선정
→ 스킬 유형 Executor 실행
→ InGameCombatManager가 전투 결과 적용
→ 필요한 Actor 생성
→ EffectManager가 시각 이펙트 생성
```

### 17.4 적 이동과 Nexus 공격

```text
EnemyActionController가 공격 대상 확인
→ 사거리 밖이면 UnitMovementController 호출
→ 사거리 안이면 적 스킬 Executor 실행
→ Player Unit이 없으면 Nexus로 이동
→ Nexus 접촉 시 InGameCombatManager에 nexus_damage 적용 요청
```

### 17.5 스킬 종료

```text
Actor가 종료 조건 감지
→ 전투 효과 종료 신호
→ EffectManager에 이펙트 제거 신호
→ Actor 제거
```

### 17.6 보상·Offering·현현

```text
전투 종료
→ RewardService가 보상 생성
→ StageManager가 Gold·DarkTrace 갱신
→ PrisonerInventory에 포로 등록
→ 기존 PrisonPanel이 PartyRoster와 포로 표시
→ OfferingService 또는 ManifestationService 실행
→ MonsterSkillBucket 또는 PartyRoster 변경
```

### 17.7 다음 라운드

```text
StageManager가 전투 완료 확인
→ 모든 필드 유닛에 ResetForNextDay() 요청
→ 현재 Day 갱신
→ 다음 StageDefinition 조회
→ SpawnManager에 다음 Encounter 생성 요청
```

## 18. 구현 가능성 평가

### 18.1 현재 활성 게임 요소

이 청사진은 다음 활성 요소마다 명시적 소유자를 제공한다.

- Monster, Enemy, Nexus
- Party와 현현 Monster
- Active, Passive, Enhancement, Master
- Skill Choice, Node, Trigger
- 피해, 회복, 보호막, 상태 효과
- Projectile, Line, Area, SingleAttack, Buff
- 자동 행동과 수동 스킬 요청
- 적 이동과 Nexus 공격
- Stage, Day, Encounter, Spawn
- 승리, 패배, 라운드 초기화
- Gold, DarkTrace
- 포로와 보상
- Offering과 현현
- 기존 Party·Reward·Prisoner·Offering·Manifestation·Nexus·Damage Meter·Auto UI

현재 확인된 활성 게임 요소는 이 구조로 구현할 수 있다.

### 18.2 확정 호환 규칙

사용자 지시와 기존 Scripts 검사로 확정한 규칙:

- 포로 `enemy_id`와 현현 `monster_id`를 직접 연결하지 않는다.
- 현재 Party에 없는 모든 Player Monster에서 동일 확률로 현현 후보를 고른다.
- 현현 시도 시작 시 포로를 소비하고 실패해도 반환하지 않는다.
- 성공 Monster 모집 확정 시 현재 필드에 즉시 배치한다.
- 기존 학습·선행·개수 제한을 통과한 모든 Offering 후보에서 동일 확률로 최대 3개를 선택한다.
- Offering 유형별 가중치와 Reroll은 없다.
- Offering 선택 확정 시 포로를 소비한다.
- 수동 스킬은 선택 Monster의 Mouse World Aim Point와 방향을 사용한다.
- 중앙 전투 Frame 순서는 Cooldown Tick, 자동 스킬, 수동 입력, 적 행동, 상태 효과 Tick이다.

### 18.3 추가 확정 사항

- 모든 Skill Actor는 독립 Unity `Update()`가 아니라 중앙 `SkillActorManager` Tick을 사용한다.
- 현현 성공 Popup의 기존 UI 흐름을 유지한다.
- 성공 Monster를 모집하거나 건너뛸 수 있다.
- 모집하면 즉시 Party 등록 후 필드에 배치한다.
- 건너뛰면 포로는 소비되고 Monster는 추가되지 않는다.
- Save/Load는 현재 없으며 이 구조에서 새로 만들지 않는다.
- Gold, DarkTrace, PartyRoster, PrisonerInventory는 현재 활성 Run의 Memory 상태만 소유한다.

현재 활성 요소 구현을 막는 추가 정보는 없다. 구현 중 새 의미 공백을 찾으면 1.1절의 근거 우선순위를 적용한다.

### 18.4 현재 비활성 또는 Data-only 요소

CSV에 필드는 있지만 Shop, Event, Elite Selection, Relic UI는 활성 요소로 확인되지 않았고 이 청사진의 Runtime 구현 대상으로 확정되지 않았다.

활성화하려면 별도 Service, 상태 Model, UI 책임을 추가해야 한다.

### 18.5 완전 교체와 리소스 재연결 경계

최종 목표는 기존 Scripts 위에 새 구조를 겹쳐 실행하는 공존 구조가 아니다.

```text
유지
├─ Pakuri/Assets/CSVdata
├─ 현재 사용하는 Scene 계층과 UI Object
├─ 현재 사용하는 Prefab
├─ Sprite
├─ Animation과 AnimatorController
└─ Inspector에 설정된 Resource 값

교체
├─ 기존 MonoBehaviour 연결
├─ 기존 ScriptableObject 타입 연결
├─ 기존 Runtime Manager와 Static 상태
├─ 기존 코드 사이의 호출 관계
└─ 기존 Scripts가 필요한 초기화 경로
```

청사진 Markdown 자체는 Unity Resource에 연결할 수 없다. Code Builder가 새 C# 타입을 구현한 후, 현재 Resource를 새 타입의 Script GUID와 Serialized Field에 다시 연결해야 한다.

전환 중 기존 Scripts는 동작과 연결 비교용으로 읽을 수 있다. 그러나 기존 Runtime과 새 Runtime이 같은 게임 상태의 권한으로 동시에 실행되어서는 안 된다. 기능 단위 전환이 끝나면 Scene·Prefab에서 해당 기존 Component를 제거하고 새 Component만 연결한다.

현재 검사로 확정한 전환 범위:

- 명시적 허용 확장자 `.unity`, `.prefab`, `.asset`, `.controller`, `.overrideController`, `.anim`, `.playable`, `.mat`, `.scenetemplate`의 Unity Serialized File 240개 검사
- 기존 Script Type 21개가 Serialized Asset 40개에서 총 56회 참조됨
- Scene, Prefab, `CsvRuntimeCatalog.asset`, `Legacy/Data/GameData/*.asset`에 참조 존재
- `.anim`의 Serialized Animation Event Function 이름 0개
- 현재 Scene·Prefab의 누락 `m_Script: {fileID: 0}` 연결 0개

유지 Resource Inventory는 `Assets` 전체가 활성이라고 가정하지 않고 근거 기반 Reachability 경계를 사용한다.

1. 기존 Script Component를 보유한 Non-Legacy Serialized Asset 24개에서 시작한다.
2. `_path`로 끝나는 Non-schema CSV 컬럼 값 전체를 추가한다.
3. 해당 Root에서 Serialized GUID 참조를 재귀 추적해 `Pakuri/Assets`의 Project Asset을 추가한다.
4. 기존 `.cs`, `.asmdef`, `.dll` 의존성은 Script-reference Manifest와 최종 의존성 제거 Gate가 별도로 추적하므로 제외한다.

이 재현 가능한 경계의 결과는 Resource 참조 행 781개, 고유 유지 Project Asset 593개다. 현재 Scene, Prefab, Sprite, Animation, AnimatorController, Font, Shader, CSV TextAsset, 참조 Data Asset을 포함하며 기록된 경로는 모두 존재한다.

Inspector Snapshot은 기존 Script 참조 56개의 정확한 Serialized MonoBehaviour YAML Payload를 UTF-8 Base64와 SHA-256으로 저장한다. 이전 Component를 Runtime 의존성으로 사용하지 않으면서 Phase 5 전후 Inspector 값을 보존한다.

새 코드 구현만으로 전환은 끝나지 않는다. 모든 기존 Script GUID 참조를 다음 중 하나로 처리한다.

1. 현재 게임에 필요한 Asset을 새 Component 또는 새 ScriptableObject 타입에 연결한다.
2. CSV 파싱으로 대체된 Legacy Data Asset은 실제 참조를 확인한 뒤 사용자 승인을 받아 삭제하거나 `Assets` 밖에 보관한다.
3. 새 구조에 필요한 Inspector 값을 동일 의미의 새 Serialized Field로 명시적으로 이전한다.
4. 이전 타입이 필요한 Asset이 하나라도 남으면 완전 교체 미완료로 판정한다.

준비와 검증은 점진적으로 수행한 뒤 최종 전환을 한 번에 활성화할 수 있다. 근거 없이 구현·Asset 재연결·Compile·Gameplay 검증이 한 변경에서 완료됐다고 주장하지 않는다.

### 18.6 완전 교체 완료 조건

다음을 모두 충족해야 정상 게임이 가능한 완전 교체로 판정한다.

- 새 코드에서 기존 Scripts 타입·Namespace 참조 0개
- Reflection String, `SendMessage`, Fallback, Adapter를 통한 기존 Scripts 호출 경로 0개
- `Pakuri/Assets` Serialized Asset의 이전 Script GUID 참조 0개
- Scene·Prefab Missing Script 0개
- 유지 CSV와 컬럼명 변경 없음
- 활성 Prefab·Sprite·Animation·AnimatorController를 새 소유자에 연결
- 전투·행동·이동·Stage·보상·Offering·현현·UI가 새 Core만 사용
- 중앙 Tick 밖에서 기존 Component가 전투 상태를 변경하지 않음
- Compile Error·Unity Console Error 0개
- 사용자가 Unity Play Mode에서 정상 Gameplay 확인

Scene에서 기존 `.cs` 연결만 끊어도 Compile 의존성은 제거되지 않는다. 이전 Source가 최종 Phase에도 `Assets` 아래에 남으면 Unity가 계속 Compile한다. 새 Asset 연결과 Gameplay 검증이 끝난 후 사용자 승인을 받아 이전 Source를 삭제하거나 Unity Compile 범위 밖으로 이동해야 한다.

## 19. 최종 목표 폴더와 Script 구조

활성 Production Source는 `Pakuri/Assets/Scripts` 바로 아래에 물리적으로 배치한다.

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
   │  └─ 기존 MainMenu UI Script 재사용·수정
   └─ InGame/
      ├─ InGameUIManager.cs
      ├─ RewardPanelController.cs
      ├─ PrisonPanelController.cs
      ├─ OfferingPanelController.cs
      ├─ ManifestationPanelController.cs
      ├─ MonsterPanel/
      │  └─ 기존 MonsterPanel UI Script 재사용·수정
      ├─ Nexus/
      │  └─ 기존 Nexus UI Script 재사용·수정
      ├─ DamageMeter/
      │  └─ 기존 DamageMeter UI Script 재사용·수정
      ├─ UtilityPanel/
      │  └─ 기존 Auto·Time Scale UI Script 재사용·수정
      └─ Debug/
         └─ 기존 Debug UI Script 재사용·수정
```

`Pakuri/Assets/Scripts/Legacy`는 추후 제거 대상으로 분류된 이전 Runtime C# 69개의 중간 보관 위치다. `Assets` 아래이므로 Unity가 계속 Compile하며 Phase 6 최종 상태가 아니다. 실제 삭제 또는 Unity Compile 범위 밖 이동은 Phase 6의 정확한 대상 승인과 Acceptance Gate가 필요하다.

### 19.1 Core

게임 시작 Data 계층:

- `CsvParser`가 유지 CSV를 읽는다.
- Definition은 CSV 용어를 정확히 보존한다.
- `GameDefinitionCatalog`가 검증된 Definition을 제공한다.
- `GameBootstrap`이 Manager, Service, 기존 UI를 연결한다.

### 19.2 Run

전투 사이에 유지되는 Run 진행 계층:

- `RunSessionModel`이 현재 Run 상태를 묶는다.
- `StageManager`가 Stage·Day·필드 유닛·Gold·DarkTrace를 관리한다.
- `PartyRoster`가 1P~5P Party 순서를 관리한다.
- `PrisonerInventory`가 포로를 관리한다.
- 세 Service가 각각 Reward, Offering, Manifestation 규칙을 실행한다.

### 19.3 Units

Definition과 Scene GameObject 사이의 유닛 계층:

- Model이 현재 체력·상태·스킬 상태를 소유한다.
- Actor가 Transform·Collider·Animation·Scene 표현을 연결한다.
- Model은 UI나 Prefab을 직접 찾지 않는다.

### 19.4 Combat

전투 판단과 실행 계층:

- `InGameCombatManager`가 피해·회복·보호막·상태 적용 결과를 조율한다.
- Actions가 자동 행동·수동 입력·적 판단·이동을 처리한다.
- Skill Runtime이 유닛별 학습·쿨다운 상태를 소유한다.
- Executor가 스킬 유형별 게임 효과를 실행한다.
- Skill Actor가 생성된 스킬 수명주기를 소유한다.
- `EffectManager`는 시각 효과만 처리한다.

### 19.5 Spawn

Definition으로 유닛 Model과 Actor를 생성해 `StageManager`에 등록한다.

Stage 선택, 현현 성공 판정, Party 제한, 피해 계산을 소유하지 않는다.

### 19.6 UI

현재 Scene 계층과 시각 Layout을 재사용한다.

UI Script는 새 Core의 읽기 전용 상태를 표시하고 Service 또는 Controller에 명령을 보낸다. Model 필드, 재화, SkillBucket을 직접 변경하지 않는다.

## 20. 코딩 지침과 규칙

### 20.1 구현 참고 기준

전환 전 `Pakuri/Assets/Scripts` 실제 코드를 읽기 전용 근거로 검사하고 다음을 참고한다.

- 현재 Gameplay 동작
- Unity Component 연결 방식
- 입력 처리 방식
- 전투 결과 전달 방식
- 기존 UI Object와 Event 연결
- Naming, Brace, Indentation Style
- Error Handling과 Unity Serialization 경계

기존 Scripts의 거대 클래스, 중복 상태, 임시 Fallback, 간접 호출 구조를 그대로 복사하지 않는다. 동작과 작성 Style은 참고하되 책임 배치는 이 청사진을 따른다.

참고는 Source 의존성을 뜻하지 않는다. 새 코드가 기존 타입을 호출·상속·감싸거나 결과를 기존 Manager로 보내면 완전 교체 조건 위반이다.

```text
청사진 책임 경계
→ 사용자 확정 Gameplay 규칙
→ CSV 계약
→ 기존 Scripts의 동작과 Coding Style
```

### 20.2 파일과 이름

- 파일의 Primary Public Type 이름을 파일명과 일치시킨다.
- Type·Method·Property는 `PascalCase`.
- Private Field·Local Variable은 `camelCase`.
- CSV Definition의 CSV 기반 필드는 실제 CSV 컬럼명을 정확히 사용한다.
- `UnitDefinotion`, `Skillbucket`, `CSVparser` 같은 오타를 만들지 않는다.
- `Manager`, `Service`, `Controller`, `Model`, `Definition`, `Actor` 접미사는 이 문서의 책임 의미와 맞을 때만 사용한다.
- `Helper`, `Util`, `Common`, `Temp`, `Data2`, `New` 같은 의미 불명 이름을 쓰지 않는다.

### 20.3 책임 분리

- 클래스 하나는 상태 권한 하나 또는 실행 책임 하나를 가진다.
- Manager는 흐름을 조율하며 하위 객체 세부 규칙을 재구현하지 않는다.
- Model은 변경 가능한 자기 상태를 소유하고 UI나 Prefab을 찾지 않는다.
- Definition은 불변 데이터이며 Runtime 상태를 소유하지 않는다.
- Service는 Domain 규칙 하나를 실행하며 Scene Object를 직접 인스턴스화하지 않는다.
- Executor는 스킬 효과를 실행하며 학습 상태나 시각 이펙트 목록을 소유하지 않는다.
- Actor는 수명주기와 Scene 표현을 담당하며 피해 공식이나 학습 상태를 소유하지 않는다.
- UI는 상태를 표시하고 명령을 전달하며 Core 상태를 직접 변경하지 않는다.

독립 책임이 두 번째로 필요하면 기존 책임과의 경계를 먼저 설명하고 분리한다. 메서드가 길다는 이유만으로 Pass-through Wrapper Class를 만들지 않는다.

### 20.4 단일 권한

같은 사실을 독립적으로 변경 가능한 두 곳 이상에 저장하지 않는다.

- Gold·DarkTrace: `StageManager`
- Party 순서: `PartyRoster`
- 현재 Reward 단계 포로: `PrisonerInventory`
- Monster 학습 상태: 해당 `MonsterSkillBucket`
- Enemy 스킬 상태: 해당 `EnemySkillBucket`
- 유닛 체력·보호막·상태: 해당 `UnitBaseModel`
- Cooldown·Magazine·Reload: 해당 `SkillCooldown`
- 활성 Skill Actor 목록: `SkillActorManager`
- 불변 Game Data: `GameDefinitionCatalog`

UI 표시 값은 위 권한의 Projection이어야 하며 별도 쓰기 가능 Copy를 만들지 않는다.

### 20.5 직접 경로와 불필요한 간접화 금지

필수 변환·수명주기·의존성 경계를 제공하지 않는 한 다음 구조를 만들지 않는다.

```text
A → B → A
A → Wrapper → 실제 A Method
Model → 임시 DTO → 같은 Model 복원
Service → Manager → 같은 Service 재호출
```

호출만 전달하는 메서드는 해당 경계가 필요한 이유를 코드와 청사진으로 증명해야 한다.

실제 다중 구독 전달, UI 분리, 비동기 수명주기, 재진입 방지가 필요할 때만 Event를 사용한다.

### 20.6 검증과 Fallback

신뢰할 수 없는 경계에서 한 번 검증한다.

- CSV 입력: `CsvParser`
- ID 연결·중복: `GameDefinitionCatalog` 생성 전
- Unity Inspector·Scene 참조: `GameBootstrap` 또는 해당 Actor 초기화
- 사용자 UI 입력: 해당 UI Controller·Service 공개 진입점
- 외부 호출 가능 Public API: 해당 API 진입점

초기화 성공 후 같은 Null·ID·Enum·Collection 검사를 모든 내부 호출에서 반복하지 않는다.

필수 데이터가 없으면 임의 기본값, 임시 객체, 이전 시스템 Fallback으로 조용히 계속하지 않는다. 명시적 오류를 반환하거나 초기화를 실패시킨다.

`manifest_success_chance`, 스킬 값, 상태 효과 값처럼 CSV가 권한인 수치에 Code Fallback을 추가하지 않는다.

### 20.7 Dead Code와 추측성 확장 금지

- 현재 Caller가 없는 Public API를 미리 만들지 않는다.
- 미래 Save/Load용 Interface·Field·빈 Method를 만들지 않는다.
- 비활성 Shop·Event·Relic Stub을 만들지 않는다.
- 빈 Executor·Actor·Service를 등록하지 않는다.
- Reader가 없는 Field나 Write-only 상태를 만들지 않는다.
- 미사용 Overload나 범용 String Lookup Method를 만들지 않는다.
- 실행되지 않는 호환 Branch를 남기지 않는다.
- `TODO`만 있는 실행 경로를 완료 기능처럼 연결하지 않는다.

새 Type 추가 전 다음 네 항목을 기록한다.

```text
Owner   = 이 Type의 수명주기를 누가 소유하는가
Caller  = 실제로 누가 호출하는가
State   = 이 Type만 소유하는 상태는 무엇인가
Delete  = 어떤 조건에서 불필요해지는가
```

하나라도 답이 없으면 Type을 추가하지 않는다.

### 20.8 접근 범위와 의존 방향

- Field는 기본 `private`.
- 외부 읽기가 필요하면 Read-only Property 또는 명시적 Query Method 제공.
- 변경은 해당 상태 권한의 Command Method로만 수행.
- 구현 전용 Type은 `internal` 우선.
- Unity Inspector 연결이 필요한 Field만 `[SerializeField] private`.
- Core Definition은 Unity Scene·UI에 의존하지 않는다.
- Run은 UI에 의존하지 않는다.
- Combat은 UI Panel에 의존하지 않는다.
- UI는 Core와 Run의 Public Query·Command API에 의존한다.
- 순환 의존성을 만들지 않는다.

### 20.9 중앙 Tick과 Unity 수명주기

- 전투 Tick 진입점은 정확히 하나.
- `InGameActionManager`가 정해진 순서로 Cooldown, 자동 스킬, 수동 입력, 적 행동, Skill Actor, Status를 Tick.
- Skill Actor마다 독립 `Update()` 금지.
- `SkillActorManager`만 Skill Actor를 Tick.
- Tick 중 Collection을 직접 변경하지 않고 `pendingAdd`, `pendingRemove` 사용.
- 새 Actor는 다음 Frame부터 Tick.
- 전투 종료·다음 Day 시작 시 중앙 Actor 목록과 Pending 목록 모두 제거.
- UI 표시용 `Update()`가 Core 게임 상태를 변경하지 않도록 보장.

### 20.10 메서드 설계

- 메서드 하나는 행동 또는 판단 하나만 수행.
- 예상 가능한 실패는 `Try...`, `Can...` 결과로 표현.
- 초기화 불변식 위반을 조용히 무시하지 않음.
- 의미 있는 중간값과 수명주기 Capture는 Local Variable로 유지 가능.
- 기존 값을 다른 이름으로 복사하기만 하는 Local Variable 금지.
- 매 Frame 전체 Scene 검색이나 `FindObjectsOfType` 사용 금지.
- List 순서가 게임 규칙이면 Sort 또는 등록 순서를 명시.
- 호출부에서 후보 목록과 균등·가중 Random 선택 규칙을 명시.

### 20.11 주석

기존 Scripts처럼 책임과 실행 이유 설명이 필요한 곳에 짧은 한국어 주석을 사용한다.

주석 대상:

- Frame 실행 순서
- 상태 권한
- 실패 시 Resource 소비 여부
- 후보 제외 조건
- 다음 Frame 등록 같은 수명주기 결정
- CSV 필드와 Runtime 의미가 달라 보이는 곳

코드를 한국어로 그대로 반복하는 주석은 쓰지 않는다.

### 20.12 Naive Code Filter 준비 점검

구현자는 각 파일을 다음 기준으로 자체 점검한다.

- 모든 Type·Method에 실제 Caller가 있는가?
- 모든 Field에 필요한 Writer·Reader가 있는가?
- 같은 상태를 여러 곳에서 변경하는가?
- 다른 객체에 들어갔다 원래 객체로 돌아오는 불필요한 왕복이 있는가?
- 초기화 이후 같은 검증·Fallback을 반복하는가?
- Pass-through Wrapper가 필요한 경계를 실제로 제공하는가?
- 미사용 Overload·Temporary Variable·Cache·Compatibility Branch가 있는가?
- UnityEvent·Inspector·Scene·Prefab·Animation Event 같은 동적 참조를 확인했는가?
- 제거 가능한 이전 권한이 새 권한과 영구 공존하는가?

Naive Code Filter는 검사 전용 역할이다. 자동 승인하거나 구현을 수정하지 않는다. 별도 요청 시 정확한 Script 또는 Folder를 대상으로 실행한다.

### 20.13 기존 Scripts 의존성 제거 점검

최종 전환 전 별도 Gate로 다음을 확인한다.

- 새 `.cs`의 기존 Namespace·Type 참조
- Scene, Prefab, `.asset`, AnimatorController를 포함한 Unity Serialized File의 이전 Script GUID
- Missing Script
- Migration 전후 Inspector Serialized 값 비교
- 기존 Manager와 새 Manager의 동시 실행 여부
- 이전 Source 제거 상태의 Unity Recompile
- 사용자 Play Mode 전체 흐름 확인

하나라도 실패하면 기존 Scripts 의존성 제거와 정상 Gameplay 구현은 미완료다.
