# Enemy Shared Skill Runtime And CSV Migration Plan

## 1. 문서 상태

- 작성 역할: Designer
- 작성일: 2026-07-16
- 범위: Enemy 스킬 CSV, Enemy 런타임 스킬 실행, Enemy 스킬 프리팹의 데이터화
- 이 문서는 설계와 구현 이관 문서다.
- 초기 설계에서 `enemy_skill_loadouts.csv`, `UnitSkillBuildState` 등의 이름은 제안이었다. Phase 2에서 `enemy_skill_loadouts.csv`를 한때 추가했지만, 2026-07-17에 Enemy마다 A/B 한 행씩만 사용하는 실제 데이터 근거에 따라 이를 삭제하고 `enemies.csv`의 `skill_slot_a_id`, `skill_slot_b_id`로 병합했다. `UnitSkillBuildState`는 현재 저장소에 없으므로 여전히 미래 제안이다.
- 최초 설계 문서 작성 자체로는 코드, CSV, 프리팹, 씬 동작을 변경하지 않았다. 이후 아래 Code Builder 진행 상태에 기록된 Phase 0~8 변경을 적용했다.
- 2026-07-16 수정: 현재 Enemy에는 강화효과와 마스터 효과가 없으므로, 초기 마이그레이션에서 기본 스킬을 graph node로 분해하지 않는다. 종류별 base CSV와 공용 typed executor를 우선 사용하고, Choice/graph는 실제 강화 기능이 생길 때만 추가한다.
- 2026-07-16 수정: Enemy 프리팹 전환은 `boards/MON/VEGA_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`의 경계를 따른다. 프리팹 collider offset은 이전하지 않고, gameplay 판정에 필요한 hitbox size만 사용하며 중심은 `(0,0)`으로 고정한다.
- 아군 전환은 아직 구현되지 않은 미래 기능이므로 이 문서의 구현 범위, API, 마이그레이션 단계, 검증 기준에 포함하지 않는다.
- 2026-07-16 확정: `visual_override_id` 계층은 사용하지 않는다. Monster 종류별 base CSV처럼 각 Enemy base 스킬 행이 runtime visual 컬럼을 직접 소유한다.
- 2026-07-16 확정: `stage_one_enemies.csv`와 `stage_two_enemies.csv`는 최종적으로 `enemies.csv` 하나로 합치고 `stage_id` 컬럼으로 Stage 1/2를 구분한다.
- 2026-07-16 확정: `CombatStart` 실행 조건은 base 효과 행과 분리하여 종류별 Trigger CSV에 저장한다.

### 1.1 Code Builder 진행 상태

- 2026-07-16: Phase 0~3 구현 완료.
- 2026-07-16: Phase 4~6 공유 typed executor 전환 구현 완료. Unity Play Mode parity는 아직 남아 있다.
- 2026-07-16: Phase 7 Enemy AI runtime slot/cooldown 권한 전환 구현 완료. Unity Play Mode parity는 아직 남아 있다.
- 2026-07-16: Phase 8 미래 Choice 적용 기반 일반화 완료. 현재 Enemy Choice/graph 데이터는 추가하지 않았다.
- 2026-07-16: Phase 9A~9D 구현 완료. 활성 legacy Enemy CSV, Enemy 전용 Plan/Executor와 복제 모델, 씬 Enemy visual fallback을 제거했고 Enemy 스킬 프리팹 15개는 `.meta`를 보존하여 `Assets/Legacy/Enemy/Skill`로 이전했다.
- 2026-07-16: 최종 비-Play-Mode 삭제 게이트 통과. Unity Editor CSV 검증, Unity 재컴파일, solution build, CSV 참조/shape, legacy 심볼, prefab GUID/외부 참조 검사가 모두 통과했다.
- 2026-07-17: `enemy_skill_loadouts.csv`와 catalog/parser/build/validation 의존을 제거하고 A/B 스킬 ID를 `enemies.csv`에 직접 병합했다. Enemy 종류별 base CSV에서는 미사용 `description_text`, `summary` 열을 제거했고, `enemies.csv`에서는 prefab 생성 경로에서 사용하지 않는 `unit_sprite_path`와 비어 있던 `projectile_sprite_path`를 제거했다.
- 2026-07-17: Enemy 패시브 정의를 `skills/base/passive/skills_passive.csv` 16행으로 분리했다. `enemies.csv`는 `passive_id`만 소유하고, 표시명·적용 대상·modifier 종류·수치는 passive base CSV가 소유한다. 런타임은 ID를 `EnemyPassiveDefinition`으로 해결한 뒤 `EnemyPassiveRuntime`에서 적용한다. Choice/graph/학습 경로는 추가하지 않았다. solution compile, CSV shape/reference, Unity Editor CSV 검증을 통과했다.
- 2026-07-17: Enemy passive 전용 parser를 추가하고 `skills_passive.csv` 외부 계약을 `skill_id,display_name,apply_target,modifier_kind,modifier_value` 5열로 축소했다. `skill_kind=Passive`, `slot=F`, `runtime_kind=Passive`는 파일 종류로 이미 확정되므로 parser 내부 고정값으로 조립한다. solution compile, CSV shape/reference, Unity Editor CSV sync/validation을 통과했다.
- Unity Play Mode에서의 16개 스킬 체감·동작 parity는 사용자 실행 검증으로 남아 있다.
- Phase 0 기준선은 `2026-07-16-enemy-shared-skill-phase-0-baseline.md`에 분리 기록했다.
- Phase 1에서 공용 runtime state/factory/Trigger/Passive 입력을 `BaseUnitRuntimeModel` 기준으로 일반화하고 `CombatStart` 1회 dispatch를 추가했다.
- Phase 2에서 `enemies.csv` 16행과 `enemy_skill_loadouts.csv` 32행을 추가했으며, 2026-07-17 단순화 작업에서 32개 배정 행을 `enemies.csv`의 직접 A/B 열로 병합하고 별도 loadout 파일을 삭제했다.
- Phase 3에서 종류별 base 16행과 `CombatStart` Trigger 2행을 추가했다.
- Phase 0~8 동안 새 데이터는 legacy와 병렬 로드·검증했다. 현재 활성 Enemy 입력은 `enemies.csv`, 종류별 base CSV, 종류별 Trigger CSV다.
- Enemy 생성 시 `EnemyDefinition.ActiveSkills`를 `SkillRuntimeFactory.RebuildAssignedActiveSet(...)`으로 조립한다.
- Enemy AI는 A/B `SkillRuntimeInstance`를 선택하고 `SkillExecutionSystem.TryExecuteSelected(...)`의 공용 실행 경로로 전달한다.
- Enemy AI는 실행 결과와 별도 쿨다운 값을 직접 기록하지 않는다. `SkillRuntimeInstance.TryBeginCast(...)`가 cast/cooldown의 단일 권한이다.
- `EnemySkillPlanRuntime`, `EnemySkillExecutor`, `EnemyResolvedSkillData`, `StageOneEnemySkillKind`와 중복 Basic/Active 스칼라 필드는 Phase 9에서 제거했다.
- Enemy runtime skill은 Monster 자동 라우팅에서 제외하여 Enemy AI Tick과 이중 실행되지 않게 했다.
- Phase 4는 기존 Projectile/SingleAttack executor와 AreaAttack→SingleAttack hitbox profile을 사용한다.
- Phase 5는 공용 target scope를 사용하고 `HealSkillExecutor`를 추가했으며 Buff/Shield executor에 configured targeting을 연결했다.
- Phase 6은 `ChainAttackSkillExecutor`, `ChargeSkillExecutor`, `SharedChargeSkillRuntime`을 추가하고 FrostPressure를 SingleAttack+status로 조립한다.
- OpeningCharge와 Intimidation은 등록 시 공유 `CombatStart` Trigger로 실행되며 legacy CombatStart 재실행을 차단한다.
- 일반 Enemy AI 선택은 `CombatStart` Trigger가 붙은 runtime을 제외하여 OpeningCharge/Intimidation을 반복 시전하지 않는다.
- `SkillChoiceResolver`는 Monster 형식 검사 대신 `BaseUnitRuntimeModel.State.ChosenChoiceIds`를 읽는다. 따라서 미래 Enemy Choice가 실제로 추가되면 같은 snapshot/graph 적용 경로를 사용할 수 있다.
- 런타임/Editor C# 빌드는 오류 0으로 통과했다. 기존 Unity 참조의 MSB3277 경고 2개는 유지된다.
- 2026-07-17 독립 정적 검사에서 Enemy 16행, 직접 A/B 참조 누락 0건, base 16행, 미참조 base 0건, CombatStart Trigger 2행을 확인했다.
- Runtime/Editor C# 빌드는 Phase 4~6 변경 후에도 오류 0으로 통과했다.
- Enemy base CSV 7개 파일의 header/type/data 열 수 정적 검사는 불일치 0건이다.
- 열린 Unity Editor에서 `SyncAndValidateCsvRuntimeCatalogsForEditor()`를 실행했고 `[EnemyPhase9Validation] PASS` 로그를 확인했다.
- `EffectManager` Enemy registry와 `NewRunScene.unity`의 `enemySkillEffects` 직렬화 블록을 제거했다.
- active runtime 아래 legacy Enemy CSV 7개와 대응 `.meta`를 제거했다.
- `Pakuri/Assets/Prefab/Enemy/Skill`은 삭제하지 않고 `Pakuri/Assets/Legacy/Enemy/Skill`로 이전했다. 15개 prefab GUID 보존과 Legacy 외부 GUID 참조 0건을 확인했다.

## 2. 결론

장기적으로 Enemy와 Monster는 같은 **스킬 구현·실행기**를 사용하는 것이 맞다.

다만 획득·선택 정책과 기본 스킬 작성 방식은 다음처럼 구분한다.

1. 현재 Enemy 기본 스킬
   - Enemy 설명 CSV의 `skill_slot_a_id`, `skill_slot_b_id`에서 보유 스킬 결정
   - Monster 종류별 base CSV와 같은 방식으로 기본 동작과 수치 작성
   - Projectile, AreaAttack, SingleAttack, Buff, Heal, Shield 등의 공용 typed executor 실행
   - 현재 존재하지 않는 Choice, 강화, Master graph는 만들지 않음

2. 미래 Enemy 강화 기능
   - Enemy 전용 획득 규칙으로 Choice 또는 Master 효과 결정
   - 선택된 강화효과만 공용 Choice/graph 조립 경로에 추가
   - 기본 스킬 base를 다시 구현하지 않음

3. 공용 실행
   - 타깃 선택
   - 효과 실행
   - 투사체, 상태이상, 강화효과
   - 쿨다운과 런타임 상태

4. 행동 주체
   - Monster: 플레이어 명령, 자동 스킬 라우팅
   - 적 상태 Enemy: `EnemyCombatSystem`의 AI 판단

즉, 현재 목표는 `서로 다른 획득·선택·Brain + 같은 base 스킬 구현·실행기`다.

`같은 스킬 실행 경로`가 `Enemy도 지금부터 모든 기본 스킬을 graph node로 작성한다`는 뜻은 아니다. 현재 Monster의 종류별 base CSV도 `base_damage`, 계수, 쿨다운, 범위, 상태효과, 시각 필드를 직접 가진다. Enemy도 먼저 같은 base 작성 방식을 사용한다.

현재 Enemy 전용 스킬 실행기를 Monster 쪽에 그대로 합치는 방식은 위험하다. Monster 런타임에도 `MonsterUnitRuntimeModel` 전용 가정이 남아 있고, Enemy AI도 별도 Tick을 수행하기 때문이다. 먼저 공용 스킬 런타임이 `BaseUnitRuntimeModel`을 받을 수 있게 일반화한 뒤 Enemy를 연결해야 한다.

## 3. 마이그레이션 시작 시 확인한 구조

이 절은 Phase 0 기준선이다. Phase 9 이후의 현행 권한은 10장 Phase 9 결과와 14장 완료 기준 상태를 따른다.

### 3.1 시작 시 Enemy CSV

현재 `Pakuri/Assets/CSVdata/authoring/enemy/`에는 다음 파일이 있다.

- `stage_one_enemies.csv`
- `stage_two_enemies.csv`
- `EnemySkillData.csv`
- `EnemySkillNodes.csv`
- `EnemySkillNodeParams.csv`

검사한 데이터 수:

- Enemy 스킬: 16개
- Enemy 스킬 노드: 16개
- Enemy 스킬 노드 파라미터: 21개

현재 `EnemySkillData.csv`의 런타임 종류:

| runtime kind | 개수 |
|---|---:|
| AreaAttack | 2 |
| Buff | 2 |
| CooldownProjectile | 4 |
| Heal | 2 |
| Shield | 2 |
| SingleAttack | 4 |

현재 노드는 사실상 스킬마다 하나의 큰 `action_op`를 실행한다.

- `DamageArea`
- `SpawnProjectile`
- `Heal`
- `Damage`
- `DamageThenDelayedChain`
- `DamageAndActionSpeedDebuff`
- `ApplySelfIncomingDamageMultiplier`
- `GrantShieldToEnemyAllies`
- `ApplyAllyMoveAndDamageMultiplier`
- `ApplyOutgoingDamageMultiplierStatus`
- `ChargeDamageStatus`

이 구조는 실행은 가능하지만, 강화효과를 작은 노드로 붙이는 Monster 그래프 구조와 다르다.

### 3.2 시작 시 Enemy 데이터 복제 경로

확인한 코드에서는 Enemy 스킬 값이 여러 단계에 복제된다.

```text
Enemy CSV
  -> PakuriCsvRuntimeData.EnemyRow / EnemySkillRow
  -> EnemyDefinition
  -> EnemyUnitRuntimeModel
  -> EnemyResolvedSkillData
  -> EnemySkillPlanRuntime / EnemySkillExecutor
```

관련 근거:

- `EnemyDefinition.cs`
  - `StageOneEnemySkillKind`
  - `EnemySkillPlanDefinition`
  - Basic/Active 스킬 스칼라 필드
- `PakuriCsvRuntimeData.EnemyDataset.cs`
  - Enemy 행과 스킬 행 파싱
  - 배정된 스킬 값을 Enemy 행에 다시 복사
- `PakuriCsvRuntimeData.Build.cs`
  - `EnemyDefinition` 생성 시 다시 복사
- `EnemyUnitRuntimeModel.cs`
  - Basic/Active 스킬 값과 Plan을 다시 보관
- `UnitFactory.cs`
  - 모델 생성 시 다시 복사
  - `ApplyStageOneEnemyPassive()`가 패시브 ID를 문자열 switch로 적용
- `EnemyCombatSystem.cs`
  - Enemy AI와 Enemy 전용 스킬 실행을 함께 소유

`Pakuri/Assets/Scripts2/InGame` 검사 기준으로 `StageOneEnemySkillKind` 참조는 43곳, Enemy Plan/Resolved/Executor 계열 참조는 68곳이었다.

### 3.3 시작 시 Monster 공유 런타임 경로

Monster는 다음 경로를 사용한다.

```text
종류별 base CSV
  + Choice CSV
  + 위치 기반 skill_graph_nodes CSV
  + 공용 node definition CSV
  -> SkillDefinition
  -> SkillData
  -> SkillRuntimeInstance
  -> SkillExecutionPlan
  -> 타입별 Executor
```

확인한 공용 노드 정의는 89개다.

공유 기반으로 이미 쓸 수 있는 구조:

- `BaseUnitRuntimeModel.SkillRuntime`
- `BaseUnitRuntimeModel.Statuses`
- `SkillExecutionPlan`
- `SkillTargetingUtility`
- `SkillTriggerRuntime`
- 상태이상 런타임
- 실행 노드 기반 강화효과 조립

그러나 현재 다음 Monster 전용 가정은 제거해야 한다.

- `SkillRuntimeFactory`가 `MonsterUnitRuntimeModel`을 요구
- `SkillChoiceResolver`가 `MonsterUnitRuntimeModel.State`를 직접 읽음
- 일부 Trigger/Passive 경로가 `MonsterUnitRuntimeModel` 또는 Monster 쪽 대상 목록을 직접 가정
- `InGameSkillDefinitionMapper`의 알 수 없는 캐릭터 ID fallback이 Eve

### 3.4 시작 시 Enemy 스킬 프리팹

`Pakuri/Assets/Prefab/Enemy/Skill/` 아래에서 15개 프리팹을 확인했다.

공통 구성:

- GameObject 1개
- SpriteRenderer 1개
- Animator 1개
- MonoBehaviour 0개

분류:

- `BoxCollider2D` 포함: 8개
- 시각 요소만 존재: 7개

즉 현재 프리팹 자체에는 스킬 실행 코드가 직렬화되어 있지 않다. 행동은 런타임 코드가 추가한다. 따라서 프리팹은 다음 정보의 원본으로 취급할 수 있다.

- 시각 프리팹 경로
- 로컬 스케일
- Sorting Order
- 필요한 경우 hitbox 크기

프리팹의 collider offset은 전환 값으로 읽거나 복사하지 않는다. Vega 런타임 시각 전환과 같이 런타임 오브젝트의 위치는 executor가 결정하고, hitbox 중심은 `(0,0)`을 사용한다.

따라서:

- 새 Enemy CSV에 `runtime_hitbox_offset_x/y` 열을 추가하지 않는다.
- gameplay 판정에 collider가 필요한 스킬만 `runtime_hitbox_size_x/y`를 작성한다.
- 프리팹 collider가 표현용이고 현재 executor의 대상/범위 판정이 따로 존재하면 size도 옮기지 않는다.
- 기존 non-zero prefab offset을 재현하는 것은 이 마이그레이션의 parity 기준이 아니다.

### 3.5 시작 시 씬 프리팹 매핑

`NewRunScene.unity`의 `EffectManager`에는 Enemy ID와 `StageOneEnemySkillKind` 조합으로 21개 매핑이 직렬화되어 있다.

확인된 문제:

- 키가 문자열 `skill_id`가 아니라 Enemy ID + enum이다.
- 같은 스킬을 여러 Enemy가 사용하면 프리팹 매핑이 반복될 수 있다.
- Stage 2의 `OpeningCharge`를 사용하는 Drake용 매핑은 확인되지 않았다.
- `EffectManager`에는 Monster용과 Enemy용 프리팹 해석 함수가 분리되어 있다.

새 구조에서는 씬의 enum 매핑을 유지하지 않는다. 프리팹에서 확인한 Sprite, Animator Controller, scale, sorting order와 필요한 hitbox size를 runtime visual CSV 필드로 옮기고, 자산 카탈로그와 `RuntimeSkillVisualFactory`가 런타임 오브젝트를 조립하는 경로가 최종 권한이 되어야 한다.

## 4. 목표 책임 구조

```text
Enemy 설명 CSV
  -> 능력치, 태그, 기본 스킬 배정, AI 역할

Enemy 종류별 base CSV
  -> 기본 피해/회복/보호막/상태효과, 사거리, 쿨다운, 시각, hitbox

Shared Typed Executor
  -> Projectile, AreaAttack, SingleAttack, Buff, Heal, Shield 실행

미래 선택 확장
  -> 실제 강화 기능이 생겼을 때만 Choice CSV + Skill Graph CSV 추가

Shared Skill Runtime
  -> 타깃, 실행, 투사체, 상태이상, 쿨다운

Brain
  -> 언제 어느 스킬을 쓸지만 결정
```

핵심 책임:

| 계층 | 책임 | 책임지지 않는 것 |
|---|---|---|
| Enemy 정의 | 유닛 능력치, 소속 초기값, AI 설정, A/B 스킬 ID | 스킬 실행식 |
| Skill base | 현재 기본 스킬의 전체 기본 동작과 수치, 범위, 시각, hitbox | 스킬 획득 규칙 |
| Typed Executor | base 데이터에 따른 실제 공용 실행 | Enemy AI의 사용 판단 |
| Choice | 미래 강화 선택지 관계와 표시 | 현재 기본 스킬의 필수 실행 |
| Graph | 미래 강화효과, Master 효과, 선택적 추가 동작 | 현재 16개 기본 스킬의 필수 데이터 |
| Brain | 사용 스킬과 시점 결정 | 피해 계산 구현 |
| Shared Runtime | 조립과 실행 | Monster/Enemy의 스킬 획득 및 AI 판단 |

## 5. 제안 CSV 디렉터리

### 5.1 초기 필수 구조

아래는 현재 16개 Enemy 기본 스킬을 옮길 때 필요한 목표 구조다. 현재 존재하지 않는 제안 파일이 포함되어 있다.

```text
Pakuri/Assets/CSVdata/authoring/enemy/
├─ enemies.csv
└─ skills/
   ├─ base/
   │  ├─ projectile/
   │  │  └─ skills_projectile.csv
   │  ├─ area_attack/
   │  │  └─ skills_area_attack.csv
   │  ├─ single_attack/
   │  │  └─ skills_single_attack.csv
   │  ├─ buff/
   │  │  └─ skills_buff.csv
   │  ├─ heal/
   │  │  └─ skills_heal.csv
   │  ├─ shield/
   │  │  └─ skills_shield.csv
   │  └─ passive/
   │     └─ skills_passive.csv
   └─ triggers/
      ├─ buff/
      │  └─ buff_skill_triger.csv
      └─ single_attack/
         └─ single_attack_skill_triger.csv
```

현재 Monster에서 확인한 실제 base 디렉터리도 `projectile`, `area_attack`, `single_attack`, `buff`, `line_attack`, `passive`로 종류별 분리되어 있다. Enemy도 이 패턴을 따른다. Heal과 Shield는 현재 Enemy에 존재하지만 검사한 Monster base 목록에는 별도 파일이 없으므로, 구현 시 기존 Buff executor로 정확히 표현 가능한지 확인하고 불가능하면 shared `heal`/`shield` typed base와 executor를 추가한다.

현재 Monster Trigger도 `skills/triggers/{kind}/` 아래에 종류별 파일을 둔다. Enemy의 현재 `CombatStart` 스킬은 다음 위치로 이관한다.

- `Intimidation` → `skills/triggers/buff/buff_skill_triger.csv`
- `OpeningCharge` → `skills/triggers/buff/buff_skill_triger.csv`

`single_attack_skill_triger.csv`는 현재 header/type 행만 남아 있으며 활성 Trigger 데이터는 없다. 파일명의 `triger` 철자는 현재 Monster 경로와 호환하기 위해 그대로 유지한다. 전체 시스템에서 `trigger`로 일괄 교정하는 작업은 별도 이름 변경 범위다.

### 5.2 미래 선택 확장 구조

현재 Enemy에 강화효과 또는 Master 효과가 추가될 때만 다음 입력을 추가한다.

```text
Pakuri/Assets/CSVdata/authoring/enemy/
├─ enemy_skill_choice_sets.csv
└─ skills/
   ├─ choices/
   │  └─ enemy_skill_choices_*.csv
   └─ graphs/
      └─ enemy_skill_graph_nodes_*.csv
```

이 미래 단계에서 노드 정의는 Enemy/Monster 중 한쪽이 아니라 shared 권한을 사용한다. 초기 base 전환에는 Choice/graph 파일, 빈 Choice set, Enemy 전용 node definition 복사가 필요 없다.

## 6. CSV 계약과 예시

### 6.1 `enemies.csv`

Stage 1과 Stage 2 Enemy 자체를 한 파일에서 설명한다. 스킬 실행 수치는 넣지 않는다.

최종 권한:

- `enemies.csv`만 Enemy 정의를 소유한다.
- `stage_one_enemies.csv`와 `stage_two_enemies.csv`의 모든 행을 합친다.
- `stage_id` 값으로 `stage_one`과 `stage_two`를 구분한다.
- 기존 `stage_one_skill`, `basic_skill` 배정은 `skill_slot_a_id`, `skill_slot_b_id`로 직접 소유한다.
- 기존 두 파일은 이관 parity 확인 후 legacy 제거 대상이다.
- Enemy 생성은 씬의 prefab binding을 사용하므로 `unit_sprite_path`를 소유하지 않는다.
- 기존 `projectile_sprite_path`는 모든 행이 비어 있고 실행 경로에서 사용하지 않았으므로 소유하지 않는다.

구현 컬럼:

```csv
enemy_id,stage_id,sort_order,display_name,encounter_role,attack_type,attribute,max_health,attack_power,spell_power,move_speed,crit_chance,crit_damage,crit_resistance,def_physical,def_fire,def_lightning,def_ice,def_darkness,def_holy,skill_slot_a_id,skill_slot_b_id,passive_id,nexus_damage
```

현재 두 파일의 실제 행을 합치는 형태 예:

```csv
enemy_id,stage_id,sort_order,display_name,encounter_role,attack_type,attribute,max_health,attack_power,spell_power,move_speed,crit_chance,crit_damage,crit_resistance,def_physical,def_fire,def_lightning,def_ice,def_darkness,def_holy,skill_slot_a_id,skill_slot_b_id,passive_id,nexus_damage
stage1-swordsman,stage_one,0,검사,Normal,Melee,Physical,100,12,0,1,0.05,1.5,0,5,2,2,2,2,2,Slash,Slash,enemy-sword-mastery,1
stage2-drake,stage_two,6,용살자 드레이크,Day10Midboss,Melee,Physical,5400,40,0,1.30,0.05,1.5,0,22,10,8,7,5,5,Slash,OpeningCharge,enemy-dragon-slaying-swordsmanship,1
```

`stage_id`는 Enemy가 어느 단계 데이터에 속하는지 구분한다. `sort_order`는 기존 Stage 배열 출력 순서를 보존하기 위한 단계 내부 정렬값이다. 스테이지 출현 순서나 웨이브 편성까지 `enemies.csv`가 소유하는 것은 아니다.

### 6.2 Enemy A/B 배정 계약

`enemy_skill_loadouts.csv`는 삭제됐다. 검사한 16개 Enemy는 각각 A/B 한 행씩만 가지고 있었고 loadout 공유, 비활성 행, 슬롯별 복수 우선순위가 없었으므로 별도 32행 테이블이 동일한 배정을 반복했다.

현재 배정 권한:

- `enemies.csv.skill_slot_a_id`: Enemy의 A runtime slot에 조립할 base `skill_id`
- `enemies.csv.skill_slot_b_id`: Enemy의 B runtime slot에 조립할 base `skill_id`
- `enemies.csv.passive_id`: Enemy 생성 시 고정 장착할 passive base `skill_id`
- parser/validation은 두 열을 필수로 읽고 각 ID가 정확한 base 행을 가리키는지 검증한다.
- build는 두 ID를 각각 `SkillSlot.A`, `SkillSlot.B`로 조립한다.
- parser/validation은 `passive_id`가 `skills_passive.csv`의 Enemy passive 정의를 가리키고 `Self`, 유효한 modifier 종류, 양수 수치를 가지는지 검증한다.
- Enemy passive 전용 parser는 파일 종류를 근거로 `skill_kind=Passive`, `slot=F`, `runtime_kind=Passive`를 내부 `SkillRow`에 고정 조립한다. 세 값은 CSV 작성 컬럼이 아니다.
- build는 passive 행을 `EnemyPassiveDefinition`으로 해결한다. Enemy 획득/선택/Choice 상태에는 넣지 않는다.

Monster의 A/B/C/D/E runtime slot 모델과 실행 구현은 계속 공유하지만, Enemy의 획득/선택 규칙은 Enemy AI가 소유한다.

시각 데이터는 `enemies.csv`에 넣지 않는다. A/B 열은 `skill_id`만 선택하고, 실제 Sprite/Controller/scale/sorting/hitbox size는 해당 base 스킬 행이 직접 가진다.

결과 규칙:

- 하나의 `skill_id`는 하나의 base 실행과 하나의 runtime visual을 가진다.
- 같은 `skill_id=Slash`를 여러 Enemy가 사용하면 모두 같은 Slash visual을 사용한다.
- 서로 다른 외형이 반드시 필요하면 `visual_override_id`를 추가하지 않고 서로 다른 `skill_id`와 base 행으로 분리한다.
- 이 경우에도 AreaAttack executor 구현은 공유하므로 코드가 복제되는 것은 아니다.

### 6.3 종류별 base CSV 계약

현재 Monster 종류별 base CSV는 공통 메타데이터만 가지는 얇은 테이블이 아니다. 검사한 실제 헤더에는 다음 값들이 종류별 CSV에 직접 존재한다.

- Projectile: `base_damage`, 공격/주문 계수, magazine, reload, shot interval, projectile speed, pierce, status, radius, target selection, cooldown, runtime visual/hitbox
- AreaAttack: `base_damage`, 계수, radius, hit count, cooldown, duration, status, runtime visual/hitbox
- SingleAttack: `base_damage`, 계수, target selection, cooldown, status, execute/target-status 조건, runtime visual/hitbox
- Buff: cooldown, status ID/label/duration/target scope/merge policy, action speed/attack power bonus, runtime visual/hitbox

Enemy base도 같은 원칙을 쓴다.

- 아래 Enemy 예시의 `Hostile`, `Friendly`, `execution_profile`, Heal/Shield 전용 필드는 목표 계약 제안이며 현재 Monster CSV에 존재한다고 주장하는 값이 아니다.
- 기본 피해량, 배율, 회복량, 보호막량, 상태효과, 지연, 연쇄 파라미터는 종류별 base CSV에 둔다.
- 현재 기본 스킬을 실행하기 위해 별도 graph 행을 요구하지 않는다.
- `cast_range`와 `effect_radius`는 의미가 다르면 분리한다.
- runtime visual은 현재 Monster처럼 `runtime_visual_sprite_path`, `runtime_visual_animator_controller_path`, scale, sorting order로 옮긴다.
- collider가 없는 프리팹은 hitbox를 비워도 된다.
- collider가 있어도 gameplay 판정에 사용하지 않는다면 hitbox를 비운다.
- gameplay 판정에 필요한 collider만 size를 이관하고 offset은 항상 `(0,0)`으로 처리한다.
- `runtime_hitbox_offset_x/y` 열은 추가하지 않는다.
- 미래 Choice/Master가 기본값을 변경하거나 추가 동작을 붙일 때만 graph를 사용한다.

### 6.4 Projectile base 예: `AimedShot`

현재 `AimedShot`은 `CooldownProjectile`, `SpawnProjectile`, `CurrentTarget`으로 확인됐다.

```csv
skill_id,display_name,runtime_kind,base_damage,attack_power_coefficient,cooldown_seconds,cast_range,target_selection,projectile_speed,runtime_visual_sprite_path,runtime_visual_animator_controller_path,runtime_visual_scale,runtime_visual_sorting_order,runtime_hitbox_size_x,runtime_hitbox_size_y
AimedShot,Aimed Shot,Projectile,<current_damage_value>,<current_coefficient_value>,<current_cooldown_value>,<current_range_value>,Hostile,<current_projectile_speed>,Assets/Enemy/Stage1/Enemy/Stage1/Achor/ChatGPT Image 2026년 5월 15일 오후 07_37_41 1.png,Assets/Enemy/Stage1/Enemy/Stage1/Achor/ChatGPT Image 2026년 5월 15일 오후 07_37_41 1.controller,1,0,0.97,0.45
```

이 한 base 행을 `ProjectileSkillExecutor`가 실행한다. 현재 기본 AimedShot을 위해 `SelectTarget`, `SpawnProjectile` graph 행을 별도로 만들지 않는다.

hitbox size `(0.97, 0.45)`는 투사체 접촉 판정에 실제 collider가 필요하다는 전제에서만 작성한다. 런타임 collider offset은 `(0,0)`이며 CSV 열로 저장하지 않는다.

`<current_...>` 값은 기존 Enemy CSV의 실제 값을 옮긴다.

### 6.5 Area Attack base 예: `Slash`

현재 `Slash`는 `AreaAttack`, `DamageArea`, `CurrentTarget`이며 11개 Enemy 배정에서 재사용된다.

```csv
skill_id,display_name,runtime_kind,base_damage,attack_power_coefficient,cooldown_seconds,cast_range,radius,target_selection,runtime_visual_sprite_path,runtime_visual_animator_controller_path,runtime_visual_scale,runtime_visual_sorting_order,runtime_hitbox_size_x,runtime_hitbox_size_y
Slash,Slash,AreaAttack,<current_damage_value>,<current_coefficient_value>,<current_cooldown_value>,<current_range_value>,<current_radius_value>,Hostile,Assets/Enemy/Stage1/Enemy/Stage1/Warrior/ChatGPT Image 2026년 5월 15일 오후 07_34_03-Photoroom (1) 1.png,Assets/Enemy/Stage1/Enemy/Stage1/Warrior/ChatGPT Image 2026년 5월 15일 오후 07_34_03-Photoroom (1) 1.controller,1,0,1.297173,1.2520766
```

이 한 base 행을 공용 AreaAttack executor가 실행한다. 현재 기본 Slash를 graph로 다시 표현하지 않는다.

여기서 `Hostile`은 Monster와 Enemy가 같은 executor를 사용하면서 각 caster의 공격 대상을 구분하기 위한 제안 이름이다.

Warrior 프리팹 collider offset은 읽거나 옮기지 않는다. AreaAttack executor가 radius 또는 런타임 box query로 판정한다면 그 실행 데이터가 권한이며, prefab collider가 표현용이라면 위 size 두 값도 비워야 한다. Code Builder는 스킬별 gameplay collider authority를 확인한 뒤 size 작성 여부를 확정한다.

### 6.6 Support base 예: `Intimidation`

현재 `Intimidation`은 전투 시작 시 모든 Tower에 outgoing damage multiplier status를 적용한다. 실제 `EnemySkillData.csv`와 `EnemySkillNodeParams.csv`에 저장된 배율은 `0.7`이다. `EnemyCombatSystem.ExecuteOutgoingDamageMultiplierStatus(...)`는 이를 `DamageBonusRate = multiplier - 1f`로 변환하므로 status bonus는 `-0.3`, 최종 outgoing damage는 기존 값의 70%가 된다.

```csv
skill_id,display_name,runtime_kind,cooldown_seconds,status_effect_id,status_outgoing_damage_multiplier,status_duration_seconds,status_target_scope,runtime_visual_sprite_path,runtime_visual_animator_controller_path,runtime_visual_scale,runtime_visual_sorting_order
Intimidation,Intimidation,Buff,30,<current_status_id>,0.7,<current_duration_value>,HostileAll,Assets/Enemy/Stage2/Skill/arsen_1.png,Assets/Enemy/Stage2/Skill/arsen_1.controller,0.43436006,50
```

Base 행은 “무엇을 실행하는가”를 소유한다.

- outgoing damage multiplier `0.7`
- 대상 범위
- 상태 지속시간
- runtime visual

Trigger 행은 “언제 실행하는가”만 소유한다. `CombatStart`는 일반 쿨다운/AI 시전이 아니라 전투 시작 이벤트이므로 `skills/triggers/buff/buff_skill_triger.csv`에 둔다.

```csv
trigger_id,source_skill_id,trigger_event,triggered_skill_id,runtime_kind,sort_order,enabled
intimidation-combat-start,Intimidation,CombatStart,Intimidation,Buff,10,true
```

이것이 구현된 Enemy Trigger CSV의 실제 계약이다. 대상과 효과는 Trigger 행이 복제하지 않고 `triggered_skill_id`가 가리키는 base 행에서 해석한다. shared `SkillTriggerEvent.CombatStart`와 등록 시 1회 dispatch는 Phase 1에서 구현됐다.

`OpeningCharge`도 같은 원리다.

현재 구현에서는 `runtime_kind=Buff`다.

```csv
trigger_id,source_skill_id,trigger_event,triggered_skill_id,runtime_kind,sort_order,enabled
opening-charge-combat-start,OpeningCharge,CombatStart,OpeningCharge,Buff,10,true
```

기본 charge 이동/접촉 피해/상태 값은 `base/buff/skills_buff.csv`에 두고, 전투 시작 실행 조건도 `triggers/buff/buff_skill_triger.csv`에 둔다.

### 6.7 미래 Choice CSV

이 절은 초기 전환 요구사항이 아니다. Enemy 강화효과가 실제로 추가될 때 사용한다.

Choice CSV는 정의와 표시 정보만 가진다. 강화 게임플레이 값은 graph에 둔다.

제안 컬럼:

```csv
choice_id,skill_id,tier,slot,display_name,description_text,sort_order
```

아래 예시는 현재 Enemy 튜닝값이 아니라 구조 설명용 제안이다.

```csv
choice_id,skill_id,tier,slot,display_name,description_text,sort_order
slash-choice-radius,Slash,1,1,Wide Slash,공격 범위가 증가한다.,10
slash-choice-armorbreak,Slash,1,2,Armor Break,피격 대상에게 방어 약화를 부여한다.,20
slash-choice-chain,Slash,2,1,Chain Slash,첫 공격 뒤 추가 피해를 준다.,10
```

### 6.8 미래 Choice graph 예

강화효과는 기존 실행을 교체하기보다 필요한 노드를 추가하거나 수정한다.

아래 수치는 구조 설명용이다. 실제 구현 값은 Designer 승인 또는 기존 데이터 근거가 필요하다.

```csv
skill_id,choice_id,plan_index,node_index,node_type,target_scope,arg_1,arg_2,arg_3
Slash,slash-choice-radius,0,0,ModifyRadius,SelfSkill,<designer_value>,,
Slash,slash-choice-armorbreak,0,0,ApplyStatus,HitTargets,DefenseDown,<designer_value>,
Slash,slash-choice-chain,0,0,DelayedDamage,HitTargets,<designer_value>,<designer_value>,
```

이 방식이면 미래 강화효과가 추가되어도 기본 스킬 base를 복제하지 않고 선택 효과만 붙일 수 있다.

현재 16개 기본 스킬에는 이 행들을 만들지 않는다.

### 6.9 미래 위치 기반 graph 컬럼

Enemy 강화효과를 Monster 그래프 조립기에 연결할 때만 위치 기반 노드 순서를 사용한다.

권장 컬럼:

```csv
skill_owner_id,skill_id,choice_id,plan_index,node_index,node_type,target_scope,arg_1,arg_2,arg_3
```

장기적으로 `monster_id`라는 열 이름은 shared 문맥에 맞지 않는다. 호환 기간에는 기존 파서가 받는 이름을 유지할 수 있지만 최종적으로 `skill_owner_id` 또는 소유자 열 제거를 권장한다.

스킬 ID가 전역 유일하다면 `skill_owner_id` 없이 다음 키로 충분하다.

```text
skill_id + choice_id + plan_index + node_index
```

### 6.10 `ChainLightning` base 예

현재 Enemy action은 `DamageThenDelayedChain` 하나로 묶여 있고 다음 파라미터를 확인했다.

- 후속 피해 배율: `0.5`
- 지연: `0.5`
- 탐색 반경: `7`
- 첫 대상 제외: 활성

이 값은 현재 기본 스킬 동작이므로 base에 둔다.

```csv
skill_id,display_name,runtime_kind,execution_profile,base_damage,attack_power_coefficient,cooldown_seconds,target_selection,chain_damage_multiplier,chain_delay_seconds,chain_radius,exclude_primary_target
ChainLightning,Chain Lightning,SingleAttack,DamageThenDelayedChain,<current_primary_damage_value>,<current_coefficient_value>,<current_cooldown_value>,Hostile,0.5,0.5,7,true
```

공용 SingleAttack executor가 이 profile을 지원하지 않으면, 재사용 가능한 typed 실행 profile을 공용 런타임에 추가한다. 현재 기본 스킬을 위해 `DelayedChainDamage` 같은 새 graph node를 만드는 것이 우선안은 아니다.

### 6.11 미래 `enemy_skill_choice_sets.csv`

Enemy마다 사용할 Choice 세트를 다르게 할 필요가 있을 때만 둔다.

제안 컬럼:

```csv
choice_set_id,skill_id,choice_id,enabled
```

구조 예:

```csv
choice_set_id,skill_id,choice_id,enabled
default-slash-choices,Slash,slash-choice-radius,true
default-slash-choices,Slash,slash-choice-armorbreak,true
future-slash-choices,Slash,slash-choice-chain,true
```

현재 Enemy에 Choice가 없으므로 첫 전환에서는 이 파일과 Choice/graph CSV를 만들지 않는다.

## 7. 현재 16개 스킬의 목표 분류

| skill_id | 현재 kind | 현재 action | 목표 base 분류 |
|---|---|---|---|
| Slash | AreaAttack | DamageArea | area_attack |
| ShieldUp | Shield | ApplySelfIncomingDamageMultiplier | shield |
| AimedShot | CooldownProjectile | SpawnProjectile | projectile |
| ShurikenThrow | CooldownProjectile | SpawnProjectile | projectile |
| Heal | Heal | Heal | heal |
| GuardianFlag | Shield | GrantShieldToEnemyAllies | shield |
| ChargeCommand | Buff | ApplyAllyMoveAndDamageMultiplier | buff |
| SacredSwordWave | CooldownProjectile | SpawnProjectile | projectile |
| FireDragonSlash | AreaAttack | DamageArea | area_attack |
| ChainLightning | SingleAttack | DamageThenDelayedChain | single_attack |
| FrostPressure | SingleAttack | DamageAndActionSpeedDebuff | single_attack |
| DarkStab | SingleAttack | Damage | single_attack |
| HolyDragonHeal | Heal | Heal | heal |
| HolySpearThrow | CooldownProjectile | SpawnProjectile | projectile |
| OpeningCharge | Buff | ChargeDamageStatus | buff |
| Intimidation | Buff | ApplyOutgoingDamageMultiplierStatus | buff |

마이그레이션 시작 시 패시브는 `UnitFactory.ApplyStageOneEnemyPassive()` 문자열 switch에서 적용됐다. 확인된 modifier 종류는 다음 10종이다.

- `CritChanceUp`
- `CritDamageUp`
- `DefenseUp`
- `FireDefenseUp`
- `HealingUp`
- `HolyDefenseUp`
- `IceDefenseUp`
- `IncomingDamageDown`
- `LightningDamageUp`
- `PhysicalDamageUp`

2026-07-17 구현 후 권한은 다음과 같다.

```csv
skill_id,display_name,apply_target,modifier_kind,modifier_value
enemy-sword-mastery,검술 숙련,Self,PhysicalDamageUp,0.10
enemy-thick-armor,두꺼운 갑옷,Self,DefenseUp,0.10
```

- `skills_passive.csv`: 패시브 ID, 표시명, 적용 대상, 기능 종류, 수치의 유일한 데이터 권한
- `enemies.csv`: 어떤 Enemy가 어떤 `passive_id`를 고정 장착하는지만 소유
- Enemy passive 전용 parser: 파일 종류를 근거로 공용 런타임에 필요한 `Passive/F/Passive` 값을 내부 고정 조립
- `EnemyPassiveDefinition`: CSV 행의 런타임 정의
- `EnemyPassiveRuntime`: 기존 10종 modifier 계산 적용
- `UnitFactory.CreateEnemy()`: 생성된 정의를 스폰 시 1회 적용

현재 Enemy 패시브는 유닛 자체 강화이므로 `apply_target=Self`만 지원한다. Monster 패시브처럼 획득하거나 스킬 강화 노드를 활성화하는 경로는 사용하지 않는다. 미래에 실제 강화/마스터 기획이 추가되면 base 패시브 정의를 유지한 채 별도 Choice/graph 계층을 추가할 수 있다.

## 8. 공용 런타임에 필요한 변경

이 절은 마이그레이션 시작 시 확인한 구현 간극과 목표다. Phase 1~9에서 아래 Enemy 연결에 필요한 변경을 구현했다.

### 8.1 공용 빌드 상태

현재 `SkillRuntimeFactory`가 Monster 모델을 직접 요구한다. Choice 해석도 Monster 모델을 직접 요구하지만, 현재 Enemy base 전환에는 Enemy Choice가 없으므로 Choice 일반화는 초기 blocker가 아니다.

제안:

```csharp
public sealed class UnitSkillBuildState
{
    public BaseUnitRuntimeModel Owner;
    public IReadOnlyDictionary<string, SkillChoiceState> Choices; // 미래 선택 기능용, 현재 Enemy는 빈 상태
}
```

정확한 타입과 필드명은 Code Builder가 현재 모델에 맞춰 확정한다. 중요한 것은 Factory가 `MonsterUnitRuntimeModel` 자체를 요구하지 않는 것이다.

### 8.2 공용 SkillRuntime 생성

목표:

```text
MonsterDefinition -> Shared SkillRuntimeFactory
EnemyDefinition   -> Shared SkillRuntimeFactory
```

Enemy 전용 `EnemyResolvedSkillData`와 `EnemySkillPlanRuntime`은 호환 기간에 fallback으로 유지하고, 공용 실행 결과가 일치한 스킬부터 제거한다.

### 8.3 AI와 실행의 분리

현재 `EnemyCombatSystem`은 AI 판단과 실행을 함께 가진다.

목표:

```text
Enemy Brain
  -> skill slot 선택
  -> target hint 제공
  -> Shared SkillExecutionSystem에 cast 요청
```

주의:

- `SkillExecutionSystem.Tick`은 roster 전체를 순회한다.
- `InGameCombatManager.Update`는 `EnemyCombatSystem`도 별도로 Tick한다.
- Enemy에 `SkillRuntime`만 채우고 두 시스템을 동시에 활성화하면 중복 시전 위험이 있다.

따라서 각 유닛은 한 시점에 하나의 cast owner만 가져야 한다.

제안 상태:

```text
PlayerBrain
AutoMonsterBrain
EnemyAiBrain
```

공용 Runtime은 Brain 종류를 몰라야 한다.

### 8.4 공용 TargetScope

Monster와 Enemy가 같은 executor를 사용하려면 caster 기준 공격 대상과 지원 대상을 구분해야 한다. 현재 `SkillTargetingUtility`의 기존 분기 구조를 공용 executor에서 사용한다.

필요한 scope:

- `Self`
- `Friendly`
- `FriendlyAll`
- `FriendlyInRadius`
- `Hostile`
- `HostileAll`
- `HostileInRadius`
- `FarthestHostile`
- `RandomHostile`

시작 시 Enemy 대상 지정의 `FarthestTower`, `RandomTower`에 대응하는 공용 Farthest/Random 선택이 부족했다. Phase 4~6에서 `FarthestHostile`, `RandomHostile` 공용 선택으로 확장했다.

시작 시 shared `SkillTriggerEvent`에는 `CombatStart`가 없었다. Phase 1에서 이벤트 enum, 등록 시 전투 시작 dispatch, 1회 실행 상태를 추가했다.

### 8.5 Heal executor

시작 시 `SkillRuntimeKind.Heal`이 `BuffSkillData`로 매핑됐고, 검사한 `BuffSkillExecutor`에는 실제 HP 회복 처리가 없었다.

Phase 5에서 Enemy `Heal`과 `HolyDragonHeal`이 사용하는 shared `HealSkillExecutor`를 구현했다.

### 8.6 특수 base 실행 profile

다음 현재 Enemy action은 기본 스킬 자체의 동작이다.

- `DamageThenDelayedChain`
- `DamageAndActionSpeedDebuff`
- `ChargeDamageStatus`
- `GrantShieldToEnemyAllies`
- `ApplyAllyMoveAndDamageMultiplier`
- `ApplyOutgoingDamageMultiplierStatus`

우선 판정:

1. 기존 공용 typed executor와 base 필드로 가능
   - base 행만 작성
2. 기존 executor에 일반적인 실행 profile을 추가하면 가능
   - shared executor/profile 확장
3. 현재 Enemy 전용 executor만 동작 가능
   - legacy adapter를 shared typed executor 뒤에 임시 연결
   - parity 완료 후 공용 profile로 치환

현재 기본 동작을 graph node로 쪼개는 것은 기본안이 아니다. 향후 Choice가 이 동작을 부분 수정하거나 추가 행동을 붙여야 할 때 graph 확장을 검토한다.

### 8.7 공용 runtime visual 해석

현재:

```text
Monster -> ResolveMonsterSkillEffectPrefab
Enemy   -> ResolveEnemySkillEffectPrefab
```

목표:

```text
runtime_visual_sprite_path
  + runtime_visual_animator_controller_path
  + scale / sorting order
  + optional gameplay hitbox size
  -> PakuriCsvRuntimeAssetCatalog
  -> RuntimeSkillVisualSpec
  -> RuntimeSkillVisualFactory.Create
```

현재 `RuntimeSkillVisualFactory.ConfigureHitbox(...)`는 `RuntimeSkillHitboxSpec.Size`와 `Offset`을 collider에 적용한다. Enemy CSV는 offset을 작성하지 않아 기본값 `(0,0)`을 사용한다.

`InGameSkillDefinitionMapper`의 알 수 없는 캐릭터 fallback도 Enemy 공용화 전에 제거하거나 명시적 오류로 바꿔야 한다. Enemy 스킬이 Eve 자산으로 잘못 해석되면 안 된다.

## 9. 프리팹 전환 규칙

### 9.1 프리팹은 유지

15개 프리팹은 삭제하지 않는다.

현재 확인 경로:

- 원본: `Pakuri/Assets/Prefab/Enemy/Skill`
- 보존 대상: `Pakuri/Assets/Legacy/Enemy/Skill`
- Phase 9 이전 원본에는 15개 `.prefab`, 15개 `.prefab.meta`, `Skill.meta`, `Stage1.meta`, `Stage2.meta`가 존재했다.
- Phase 9 이후 `Pakuri/Assets/Prefab/Enemy/Skill`은 존재하지 않고 `Pakuri/Assets/Legacy/Enemy/Skill`에 동일한 15개 prefab과 metadata가 존재한다.

Phase 9에서도 Enemy 스킬 프리팹은 삭제 후보가 아니다. 씬 fallback과 serialized GUID 참조가 0건임을 확인한 뒤 `Assets/Prefab/Enemy/Skill` 폴더 전체를 `Assets/Legacy/Enemy/Skill`로 이전했다.

이전 규칙:

- Unity AssetDatabase 이동 또는 `.meta`를 보존하는 동등한 이동 절차를 사용한다.
- `Skill.meta`, 하위 폴더 `.meta`, 각 `.prefab.meta`를 함께 보존하여 GUID를 바꾸지 않는다.
- 프리팹 파일을 개별 삭제 후 재생성하지 않는다.
- 이전 전후 prefab GUID를 비교해 15개 모두 동일함을 확인했다.
- Legacy 폴더 밖에서 15개 prefab GUID를 참조하는 파일이 0건임을 확인했다.

초기 전환:

- 프리팹의 Sprite 경로를 runtime visual CSV로 추출
- 프리팹의 Animator Controller 경로를 runtime visual CSV로 추출
- 기존 scale과 sorting order를 runtime visual CSV로 이관
- gameplay 판정에 필요한 collider의 size만 CSV로 복사
- prefab collider offset은 복사하지 않고 런타임 중심 `(0,0)` 사용
- SpriteRenderer, Animator, 선택적 BoxCollider2D 조립은 `RuntimeSkillVisualFactory`가 담당
- 이동, 충돌, 지속시간 같은 런타임 actor 책임은 공용 executor가 담당

### 9.2 프리팹 분류

| 분류 | 현재 수 | 전환 |
|---|---:|---|
| visual + BoxCollider2D | 8 | sprite/controller/scale/sorting + gameplay-required size |
| visual only | 7 | sprite/controller/scale/sorting |
| MonoBehaviour behavior | 0 | 이관할 프리팹 행동 코드 없음 |

`BoxCollider2D`가 프리팹에 존재한다는 사실만으로 runtime hitbox를 만들지 않는다.

- Projectile 접촉이나 생성된 box overlap이 현재 gameplay 판정 권한이면 size 작성.
- radius, target selection, line query 등 executor가 이미 판정을 소유하면 prefab collider size 생략.
- 작성된 runtime hitbox의 offset은 항상 `(0,0)`.
- Enemy 스킬 prefab 전환을 위해 `runtime_hitbox_offset_x/y` 열이나 별도 graph param을 추가하지 않음.

### 9.3 base 행의 직접 시각 권한

현재 `Slash`는 11개 Enemy 배정에서 재사용된다. 새 구조에서는 씬의 Enemy별 시각 매핑이나 `visual_override_id`를 사용하지 않는다.

권한:

- `skills_area_attack.csv`의 Slash base 행이 Slash runtime visual을 직접 소유한다.
- 모든 Slash 사용자는 같은 base 행과 같은 visual을 사용한다.
- Enemy별로 다른 visual이 필요하면 서로 다른 skill ID/base 행을 만든다.

예:

```csv
skill_id,display_name,runtime_kind,...,runtime_visual_sprite_path,runtime_visual_animator_controller_path,runtime_visual_scale,runtime_visual_sorting_order
SlashWarrior,Slash,AreaAttack,...,<warrior_sprite>,<warrior_controller>,1,0
SlashRogue,Slash,AreaAttack,...,<rogue_sprite>,<rogue_controller>,1,0
```

두 행 모두 같은 AreaAttack executor를 사용하므로 실행 코드 복제는 없다. 데이터 행만 시각과 필요한 수치를 명시적으로 소유한다.

### 9.4 씬 fallback 제거 조건

다음 조건을 만족하여 `EffectManager`의 Enemy enum 매핑을 제거했다.

1. 16개 스킬 모두 base CSV에 존재
2. 각 base 행의 sprite/controller visual이 asset catalog에 등록
3. 15개 프리팹의 scale/sorting과 gameplay-required hitbox size 확인
4. visual 자산 경로 누락 0건
5. `OpeningCharge`는 기존 scene/prefab visual mapping이 없었으므로 의도적으로 visual 없이 shared charge behavior만 실행
6. 공용 resolver 실패 시 명시적 로그 존재

`EffectManager.cs`의 Enemy registry와 `NewRunScene.unity`의 `enemySkillEffects` 블록은 제거됐다.

## 10. 단계별 마이그레이션

### Phase 0. 현재 동작 고정

- 16개 Enemy 스킬별 현재 입력과 결과 표 작성
- 15개 프리팹의 path/scale/sorting/collider size snapshot 작성
- prefab collider offset은 증거로만 기록하고 런타임 이관값에서 제외
- 스킬별 gameplay collider authority를 executor 코드로 분류
- Stage 1/2 대표 재생 시나리오 확보
- 아직 기존 실행 코드를 제거하지 않음

### Phase 1. 공용 모델 일반화

- `SkillRuntimeFactory`의 Monster 전용 입력 제거
- Trigger/Passive의 `roster.Players` 고정 가정 제거
- shared `SkillTriggerEvent.CombatStart`와 전투 시작 dispatch 추가
- 알 수 없는 캐릭터 Eve fallback 제거
- 현재 Enemy는 Choice가 없으므로 Choice 해석 일반화는 미래 강화 Phase까지 미뤄도 됨

### Phase 2. Enemy 설명과 A/B 배정 병합

- `stage_one_enemies.csv`와 `stage_two_enemies.csv`의 행을 `enemies.csv`로 병합
- 모든 행에 `stage_id=stage_one` 또는 `stage_id=stage_two` 작성
- 기존 `stage_one_skill`, `basic_skill` 배정을 `skill_slot_a_id`, `skill_slot_b_id`로 대체
- 중간 이관에서 추가했던 `enemy_skill_loadouts.csv`는 2026-07-17 직접 A/B 병합 후 삭제
- parity 기간에는 기존 두 stage 파일을 fallback으로 유지
- parity 완료 뒤 `enemies.csv`를 유일한 Enemy 정의 권한으로 전환하고 기존 두 파일 제거

### Phase 3. 16개 base CSV 생성

- 종류별 base CSV 작성
- 기존 16개 기본 동작과 21개 파라미터를 종류별 base 필드로 기계적으로 이관
- 각 base 행에 Sprite/Controller/scale/sorting과 gameplay-required hitbox size 직접 이관
- runtime hitbox offset 열을 추가하지 않고 중심 `(0,0)` 사용
- `Intimidation`, `OpeningCharge`의 `CombatStart` 조건을 종류별 Trigger CSV로 이관
- Choice/graph CSV는 만들지 않음

### Phase 4. 단순 base 스킬 공용 executor 전환

구현 상태: Code complete, C# compile complete, Play Mode parity pending.

- DarkStab와 Slash/FireDragonSlash는 공유 `SingleAttackSkillExecutor`로 실행한다.
- AreaAttack base는 gameplay-required runtime hitbox size와 `hit_target_count`를 사용한다.
- AimedShot, ShurikenThrow, SacredSwordWave, HolySpearThrow는 공유 `ProjectileSkillExecutor`로 실행한다.
- Projectile lifetime과 Farthest target selection은 base CSV 값에서 runtime data로 전달한다.

우선순위:

1. `DarkStab`
2. `Slash`
3. `AimedShot`
4. `ShurikenThrow`
5. `SacredSwordWave`
6. `FireDragonSlash`
7. `HolySpearThrow`

이유: Damage, DamageArea, SpawnProjectile 중심이라 기존 공용 typed executor와 parity 비교가 쉽다.

### Phase 5. 지원 스킬 전환

구현 상태: Code complete, C# compile complete, Play Mode parity pending.

- ShieldUp와 ChargeCommand, Intimidation은 공유 `BuffSkillExecutor`와 status runtime을 사용한다.
- GuardianFlag는 공유 `ShieldSkillExecutor`를 사용한다.
- Heal/HolyDragonHeal은 새 공유 `HealSkillExecutor`를 사용한다.
- Hostile/Friendly/Self scope, radius filter, lowest-health target, permanent outgoing-damage debuff를 typed data로 조립한다.

- `ShieldUp`
- `Heal`
- `HolyDragonHeal`
- `GuardianFlag`
- `ChargeCommand`
- `Intimidation`

이 단계에서는 Buff, Heal, Shield typed base/executor로 나눈다. 기존 Buff executor가 Heal/Shield를 정확히 실행하지 못하면 shared Heal/Shield executor를 추가한다. 공용 Hostile/Friendly scope도 필요하다.

### Phase 6. 복합 스킬 전환

구현 상태: Code complete, C# compile complete, Play Mode parity pending.

- ChainLightning은 `ChainAttackSkillExecutor`가 primary hit 후 CSV delay/multiplier/radius로 2차 타격한다.
- FrostPressure는 combined AP/SP damage와 action-speed-down status를 `SingleAttackSkillExecutor`로 실행한다.
- OpeningCharge는 `ChargeSkillExecutor`와 `SharedChargeSkillRuntime`이 random hostile 선택, 속도 ramp, hitbox 충돌, max-health damage, Freeze를 처리한다.
- CombatStart Trigger는 shared runtime registration dispatch를 사용한다.

- `ChainLightning`
- `FrostPressure`
- `OpeningCharge`

지연 연쇄, 복합 debuff, 전투 시작 charge의 parity를 별도 검증한다.

### Phase 7. Enemy AI 연결

구현 상태: Code complete, C# compile complete, Play Mode parity pending.

- Enemy Brain은 `InGameSkillSlot.B` 지원/특수 runtime을 우선 판정하고, 실행 가능하지 않으면 `InGameSkillSlot.A` 기본 runtime을 판정한다.
- `CombatStart` Trigger가 붙은 runtime은 일반 AI 선택에서 제외한다.
- `EnemyCombatSystem` 활성 Tick에서 legacy 쿨다운 tick, pending plan action, legacy active charge, Plan/Executor fallback 실행을 제거했다.
- Enemy AI는 `CanExecuteSelectedSkill(...)`로 readiness만 확인하고 `TryExecuteSelectedSkill(...)`로 공용 실행을 요청한다.
- `SkillExecutionSystem.TryRouteSkill(...)`과 `SkillRuntimeInstance.TryBeginCast(...)`가 cast/cooldown을 단독 소유한다.
- 자동 Monster 라우팅은 Enemy entry를 계속 거부하므로 Enemy AI Tick과 중복 실행되지 않는다.

### Phase 8. 미래 Choice 강화효과 연결

구현 상태: Future data absent, shared runtime readiness complete.

- 현재 Enemy Choice/graph CSV는 만들지 않는다.
- `SkillChoiceResolver`의 선택 ID 입력을 `BaseUnitRuntimeModel.State.ChosenChoiceIds`로 일반화했다.
- `InGameSkillDefinitionMapper`는 기존처럼 Enhancement/Master Choice와 normalized plan node를 `SkillData`에 매핑한다.
- 실제 Enemy 강화/마스터 기능이 기획된 시점에만 Enemy Choice set과 graph 노드를 base 위에 부착한다.
- 그때 기본 스킬 실행과 강화 노드가 중복 적용되지 않는지 Play Mode로 검증한다.

### Phase 9. legacy 제거

구현 상태: **Phase 9A~9D와 최종 비-Play-Mode 삭제 게이트 완료**.

#### Phase 9A. Enemy CSV 단일 권한

- `enemies.csv`에 `sort_order`를 추가하고 `stage_id` + `sort_order`로 기존 Stage 1/2 배열 출력을 조립한다.
- loader/source catalog/build/validation에서 다음 활성 입력을 제거했다.
  - `stage_one_enemies.csv`
  - `stage_two_enemies.csv`
  - `EnemySkillData.csv`
  - `EnemySkillNodes.csv`
  - `EnemySkillNodeParams.csv`
  - `catalog_stage_one_enemies.csv`
  - `catalog_stage_two_enemies.csv`
- 현행 Enemy 입력은 `enemies.csv`, 종류별 base CSV 7개, 종류별 Trigger CSV 2개다.
- `enemies.csv`가 A/B base skill ID를 직접 소유하며, 별도 loadout 파일과 catalog 직렬화 필드는 없다.
- legacy 비교 검증 대신 stage, sort order, A/B base/Trigger 참조를 현재 데이터 자체로 검증한다.

#### Phase 9B. Enemy 전용 실행 모델 제거

- 제거:
  - `StageOneEnemySkillKind`
  - Enemy Basic/Active 중복 스칼라 필드
  - `EnemyResolvedSkillData`
  - `EnemySkillPlanDefinition` 계열
  - `EnemySkillPlanRuntime`
  - `EnemySkillExecutor`
  - `UnitFactory.ApplyStageOneEnemyPassive()`
- Enemy AI는 shared runtime A/B slot을 선택하고 typed executor 실행만 요청한다.
- Enemy 패시브는 `enemies.csv.passive_id`로 `skills_passive.csv` 정의를 해결하고 `EnemyPassiveDefinition`, `EnemyPassiveModifierKind`, `EnemyPassiveRuntime`으로 적용한다.

#### Phase 9C. 씬 visual fallback 제거

- `EffectManager`의 Enemy enum/prefab registry와 resolver를 제거했다.
- `NewRunScene.unity`의 `enemySkillEffects` 직렬화 블록을 제거했다.
- 15개 시각 보유 스킬은 base CSV의 Sprite/Animator/scale/sorting/hitbox size와 runtime asset catalog를 사용한다.
- `OpeningCharge`는 시작 기준선에 scene/prefab visual mapping이 없었으므로 의도적으로 visual-less다. 이동·충돌·피해·Freeze는 shared charge runtime이 계속 담당한다.

#### Phase 9D. legacy 파일 삭제와 prefab 보존 이전

- active runtime의 legacy Enemy CSV 7개와 대응 `.meta`를 삭제했다.
- `PakuriCsvRuntimeData.EnemyDataset.cs`와 `.meta`를 삭제했다.
- Enemy 스킬 prefab은 삭제하지 않았다.
- `Pakuri/Assets/Prefab/Enemy/Skill` 전체를 `Pakuri/Assets/Legacy/Enemy/Skill`로 이동했다.
- 15개 `.prefab.meta`의 이동 전후 GUID가 모두 동일하고 Legacy 폴더 밖 GUID 참조가 0건이다.

#### 최종 삭제 게이트 결과

| 게이트 | 결과 | 근거 |
|---|---|---|
| solution compile | 통과 | `dotnet build Pakuri/Pakuri.sln --no-restore /p:UseSharedCompilation=false`: 오류 0, 기존 경고 2 |
| Unity Editor CSV 검증 | 통과 | 열린 Editor에서 `[EnemyPhase9Validation] PASS` 확인 |
| Unity script reload | 통과 | 임시 검증 훅 제거 뒤 `Assembly-CSharp-Editor.dll`이 source보다 새로 생성되고 compiler error 0 |
| Enemy CSV 참조/shape | 통과 | Enemy 16, Stage 1 8, Stage 2 8, 직접 A/B 참조 누락 0, active base 16, Trigger 2; runtime Enemy CSV 10개 width 일치 |
| legacy active CSV | 통과 | active runtime 경로에 제거 대상 파일 0 |
| legacy code/scene symbol | 통과 | 제거 대상 Plan/Executor/enum/scene field 검색 0 |
| runtime visual asset | 통과 | visual 보유 15개 asset 누락 0, `OpeningCharge`만 의도적 visual-less |
| hitbox offset 경계 | 통과 | Enemy CSV offset 열 0, runtime consumer 0, size-only validation guard 유지 |
| prefab 보존 | 통과 | prefab 15, meta 15, GUID 보존 15, Legacy 외부 GUID 참조 0 |

위 표는 Phase 9 삭제 당시 게이트 결과다. 이후 2026-07-17 Enemy passive CSV 분리 변경은 solution compile 오류 0, Enemy 16/passive 16/누락 0/미사용 0, CSV width 오류 0, `git diff --check` 통과를 확인했다. 열린 Editor에서 임시 검증 훅으로 `SyncAndValidateCsvRuntimeCatalogsForEditor()`를 실행하여 `[EnemyPassiveCsvValidation] PASS`를 확인했으며 훅과 `.meta`는 즉시 삭제했다. Play Mode 확인만 남아 있다.

남은 사용자 검증:

- Unity Play Mode에서 Stage 1/2 대표 Enemy와 16개 스킬의 피해, 대상, 쿨다운, 상태효과, 시각, 중심 hitbox를 확인한다.
- Unity Play Mode에서 대표 Enemy 패시브의 피해 증가, 방어 증가, 치명타, 치유량, 받는 피해 감소 적용을 확인한다.
- 이 검증은 코드/CSV/씬 legacy 삭제를 되돌리는 선행 조건이 아니라 최종 gameplay parity 확인이다. 문제가 확인되면 shared runtime/base CSV 경로에서 수정하며 legacy 실행 경로를 복원하지 않는다.

## 11. 호환성과 주의점

### 11.1 스킬 ID는 실행 권한

같은 `skill_id`는 같은 실행 의미를 가져야 한다.

- Enemy마다 기본 피해 배율이 다르면 caster stat 또는 별도 base skill ID로 처리
- 외형만 달라도 별도 `visual_override_id`를 만들지 않고 다른 `skill_id`와 base 행으로 분리
- 실행이 다르면 다른 skill_id

### 11.2 Basic/Active 이름에 의존 금지

현재 Enemy는 Basic/Active 두 축이지만 Monster 공용 runtime과 Enemy AI 역할을 분리하기 위해 runtime slot과 AI role을 분리한다.

### 11.3 no-target 처리

공용 executor는 대상이 없을 때 다음을 스킬별로 명시해야 한다.

- cast 취소
- 쿨다운 미소모
- self fallback
- 가장 가까운 적 재탐색

기존 Enemy 동작과 비교해 결정한다.

### 11.4 미래 강화효과 대상

향후 Choice를 추가할 때 노드가 `SelfSkill`인지, `HitTargets`인지, `Caster`인지 분명해야 한다.

예:

- 범위 증가: SkillRuntime 수정
- 방어 감소: HitTargets에 Status
- 쿨다운 감소: 해당 SkillRuntimeInstance
- 우호 대상 강화: Friendly 대상

### 11.5 데이터 실패 정책

권장:

- 잘못된 skill ID: 로드 오류
- 없는 node type: 로드 오류
- 없는 runtime visual sprite/controller path: 명시적 경고 + 시각 없이 실행 여부 정책
- 없는 Choice target skill: 로드 오류
- 중복 plan/node index: 로드 오류

## 12. Code Builder 구현 대상

우선 확인·수정할 코드 면:

- Enemy CSV loader/build 경로
  - `PakuriCsvRuntimeData.EnemyDataset.cs`
  - `PakuriCsvRuntimeData.Build.cs`
- Enemy 정의와 모델
  - `EnemyDefinition.cs`
  - `EnemyUnitRuntimeModel.cs`
  - `UnitFactory.cs`
- Enemy AI/실행
  - `EnemyCombatSystem.cs`
  - `EnemySkillPlanRuntime`
  - `EnemySkillExecutor`
- 공용 스킬
  - `SkillRuntimeFactory`
  - `SkillChoiceResolver`
  - `SkillExecutionSystem`
  - `SkillTargetingUtility`
  - `SkillTriggerRuntime`
  - 종류별 executor
- 시각
  - `EffectManager`
  - `PakuriCsvRuntimeAssetCatalog`
  - `InGameSkillDefinitionMapper`
  - `InGameProjectileActor`
- 데이터 카탈로그
  - `GameDataCatalog`
  - `PakuriDataManager`

이 목록은 검사한 현재 구조의 변경 면이다. 구현 시 실제 클래스 경로와 의존성을 다시 확인해야 한다.

## 13. 검증 계획

### 13.1 데이터 검증

- Enemy 16개 base skill_id가 base에 정확히 1번 존재
- 모든 `skill_slot_a_id`, `skill_slot_b_id`가 base를 참조
- 모든 `passive_id`가 `skills_passive.csv`의 Enemy passive 정의를 정확히 1번 참조
- `skills_passive.csv` 헤더가 `skill_id,display_name,apply_target,modifier_kind,modifier_value` 5열과 일치
- passive 16행의 `apply_target=Self`, `modifier_kind != None`, `modifier_value > 0`
- 모든 enemies.csv 행의 stage_id가 `stage_one` 또는 `stage_two`
- 모든 enemies.csv 행이 A/B 스킬 ID를 직접 가진다.
- 모든 visual path가 asset catalog에 등록
- 모든 CombatStart 스킬이 종류별 Trigger CSV에서 정확히 한 번 연결
- shared Trigger parser가 `CombatStart`를 읽고 각 Enemy당 한 번만 dispatch
- 기존 21개 param이 누락 없이 종류별 base 필드로 이동
- 초기 Enemy Choice/graph 입력이 없어도 16개 기본 스킬이 로드됨
- 미래 Choice 기능을 추가한 경우에만 node type, Choice target, graph position을 추가 검증

### 13.2 동작 parity

스킬별 검증:

- 피해량
- 타깃
- 범위
- 쿨다운
- 시전 시점
- 투사체 속도
- 지연 시간
- 상태효과 값과 지속시간
- visual scale/sorting
- gameplay-required hitbox size
- 생성된 runtime collider offset `(0,0)`
- 표현용 prefab collider가 runtime 피해 판정을 새로 만들지 않는지 확인

### 13.3 회귀

- Monster 스킬 조립 결과 변화 없음
- Monster Choice graph 결과 변화 없음
- Monster 자동 스킬 라우팅 변화 없음
- Enemy A/B runtime 선택 우선순위와 기존 Stage 1/2 행동 parity 유지
- Enemy runtime 선택과 Monster 자동 라우팅이 같은 cast를 중복 실행하지 않음
- Enemy Choice 데이터가 없는 현재 상태에서도 base 스킬 snapshot 결과가 바뀌지 않음

## 14. 완료 기준

다음을 모두 만족하면 전환 완료다.

1. Enemy 16개 스킬이 shared `SkillExecutionPlan`으로 실행된다.
2. Enemy 16개 기본 스킬은 종류별 base CSV만으로 실행되며 Choice/graph 입력을 요구하지 않는다.
3. Enemy AI는 사용 판단만 하고 효과 실행을 직접 구현하지 않는다.
4. Monster와 Enemy가 같은 executor를 사용하면서 각자의 Hostile/Friendly 대상 규칙을 유지한다.
5. 각 base 스킬 행이 runtime visual을 직접 소유하고 `visual_override_id`가 존재하지 않는다.
6. `Intimidation`과 `OpeningCharge`의 base 효과와 CombatStart Trigger 책임이 분리된다.
7. `enemies.csv` 하나가 Stage 1/2 Enemy 정의와 A/B 스킬 ID를 소유하고 `stage_id`로 구분한다.
8. 15개 기존 스킬 프리팹의 시각과 gameplay-required hitbox size가 확인된다.
9. runtime hitbox offset은 `(0,0)`이며 새 offset CSV 열이 없다.
10. 표현용 prefab collider 때문에 기존 단일/범위 대상 판정이 바뀌지 않는다.
11. `EffectManager` Enemy enum 매핑이 제거된다.
12. Enemy 전용 Plan/Executor와 중복 스칼라 필드가 제거된다.
13. Monster 기존 스킬과 Choice 회귀가 없다.
14. 미래 Enemy 강화효과가 필요해지면 base를 재작성하지 않고 Monster와 같은 Choice/graph 확장 계층을 선택적으로 붙일 수 있다.
15. Enemy 스킬 프리팹은 삭제하지 않고 `.meta`를 보존하여 `Pakuri/Assets/Legacy/Enemy/Skill`로 이전된다.
16. `enemy_skill_loadouts.csv`, `unit_sprite_path`, `projectile_sprite_path`, Enemy base의 `description_text`, `summary`는 활성 Enemy CSV 계약에 존재하지 않는다.
17. `enemies.csv`는 `passive_id`만 소유하고, Enemy 패시브 표시명·기능·수치는 5열 `skills_passive.csv`가 소유하며 스폰 시 `EnemyPassiveDefinition`을 통해 적용된다. `Passive/F/Passive`는 Enemy passive 전용 parser가 내부 조립한다.

## 15. 관련 영속 상태

- `boards/COMBAT/ENEMY_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`
