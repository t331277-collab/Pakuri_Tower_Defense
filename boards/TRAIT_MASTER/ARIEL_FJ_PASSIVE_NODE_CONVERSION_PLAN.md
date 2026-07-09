# Ariel F-J Passive Node Conversion Plan

## Task title

Ariel F-J passive node conversion plan

## Goals

- Ariel F-J 패시브 스킬과 강화 효과에서 복붙성 EffectTarget 파라미터를 제거하고, 실제 기능 노드 중심으로 정리한다.
- `projectile_skill_node_params.csv`처럼 값이 기능 노드에 직접 붙는 구조를 기준으로 삼는다.
- Code Builder가 CSV 수정 전에 어떤 행을 남기고 어떤 행을 줄일지 판단할 수 있게 한다.

## Constraints

- 이 문서는 설계 문서다. CSV는 아직 수정하지 않는다.
- 근거는 현재 확인한 runtime CSV와 C# 매핑 코드다.
- MSW-MCP는 사용하지 않는다.
- Unity Play Mode 검증은 사용자 소유다.

## Role Owner

Designer

## Status

Ready for Code Builder handoff.

## Evidence

- 현재 대상 CSV:
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/passive/passive_skill_nodes.csv`
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/passive/passive_skill_node_params.csv`
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/choices/passive/skill_choices_passive.csv`
- 비교 기준 CSV:
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/projectile/projectile_skill_nodes.csv`
  - `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/projectile/projectile_skill_node_params.csv`
- 코드 근거:
  - `PakuriCsvRuntimeData.Build.cs`
    - `EffectTarget`는 `target_side`, `target_selection`, `target_shape`, `center_mode`, `visual_anchor_mode`, `effect_timing`, `delay_seconds`, `apply_once`, `cover_all`을 `SkillEffectDefinition`에 복사한다.
    - Choice 노드는 choice의 `target_skill_id`를 기본 target으로 받아 `BuildSkillNodeDefinitions(...)`에 들어간다.
  - `SkillMultiEffectExecutor.cs`
    - 일반 status/shield 적용 경로는 `ResolveTargetList(...)`로 대상 목록을 고른다.
    - `target_selection=EventTarget`일 때만 `ResolveExplicitEventTarget(...)`로 이벤트 대상이 직접 사용된다.
    - `SpawnVisual(...)`과 `SpawnVisualOnTargets(...)`는 `SkillEffectPrefab`이 없으면 return 한다.
  - `SkillTargetingUtility.cs`
    - 일반 target list는 `TargetSide`로 아군/적 목록을 고른다.
    - 현재 확인한 status/shield 계열의 `target_selection=Owner`, `target_shape=Battlefield`는 실제 대상 목록 선택에는 쓰이지 않는다.
  - `InGamePassiveEffectRuntime.cs`
    - `apply_once=true`는 패시브 one-shot 실행 제어에 쓰인다.

## Current diagnosis

현재 Ariel F-J passive 노드는 완전히 옛 effect CSV 그대로는 아니다. 수치 자체는 이미 기능 노드에 들어가 있다.

예:

- `ariel-f-trait-1-holy-damage-bonus`는 `StatusDamageBonusRate(bonus=0.06)`이다.
- `ariel-f-trait-2-magazine-bonus`는 `MagazineBonus(bonus=2)`다.
- `ariel-f-party-holy-damage-status-damage-bonus-rate`는 `StatusDamageBonusRate(bonus=0.12, attribute=Holy)`다.
- `ariel-f-holy-skill-crit-trait3-status-critical-chance-bonus`는 `StatusCriticalChanceBonus(bonus=0.08)`다.

하지만 F-J의 Effect 그룹에는 아직 옛 effect CSV에서 복사된 것처럼 보이는 `EffectTarget` 파라미터가 많이 남아 있다.

대표 복붙성 후보:

- `target_selection=Owner`
- `target_shape=Battlefield`
- `center_mode=Caster`
- `visual_anchor_mode=AppliedTargets`

이 값들은 모든 경우에 무조건 불필요한 것은 아니지만, 현재 F-J status/shield 계열에서는 대부분 기능 결과를 만들지 않는다. 반대로 `target_side`, `apply_once`, 조건 노드, 지속시간, 실제 보정 수치 노드는 기능 결과를 만든다.

## Conversion principle

### 1. Choice 수치 노드는 그대로 둔다

강화 효과가 기존 값만 바꾸는 경우는 `owner_kind=Choice` 노드로 유지한다.

유지 대상:

- `StatusDamageBonusRate`
- `StatusActionSpeedBonus`
- `StatusDurationBonus`
- `StatusDamageTakenBonus`
- `CooldownMultiplier`
- `StatusShieldReceivedBonus`
- `ShieldAmountMultiplier`
- `MagazineBonus`

이 노드들의 수치는 `passive_skill_node_params.csv`에 이미 있다. choice CSV의 수치 컬럼으로 되돌리지 않는다.

### 2. EffectTarget은 최소 기능 파라미터만 남긴다

status/shield 계열의 전체 아군/전체 적 적용 효과는 대부분 `target_side`만으로 대상 편이 결정된다.

남길 값:

- `target_side=AllAllies`
- `target_side=Enemy`
- `apply_once=true`가 실제 one-shot 실행을 의미하는 경우

제거 후보:

- `target_selection=Owner`
- `target_shape=Battlefield`
- `center_mode=Caster`
- `visual_anchor_mode=AppliedTargets`

예외:

- `target_selection=EventTarget`은 이벤트 대상 직접 지정에 쓰이므로 제거하지 않는다.
- `EffectVisual`이 있는 Effect에서 `visual_anchor_mode=AppliedTargets`는 대상별 visual 부착에 의미가 있으므로 제거하지 않는다.
- `EffectDamage` 계열의 `target_shape`는 피해 범위에 영향을 줄 수 있으므로 이 문서의 F-J status/shield 정리 기준으로 삭제하지 않는다.
- `apply_once=true`는 `InGamePassiveEffectRuntime`에서 쓰이므로 제거하지 않는다.

### 3. 실제 기능 노드로 읽히는 것만 남긴다

각 Effect 그룹은 아래처럼 읽히는 노드만 남기는 것을 목표로 한다.

- operation 노드:
  - `StatusModifier`
  - `ApplyShield`
- 대상 편 노드:
  - `EffectTarget(target_side=...)`
  - 필요 시 `apply_once=true`
- 조건 노드:
  - `ConditionStatus`
  - `ConditionSkillAttribute`
- 지속시간 노드:
  - `EffectLifetime(duration_seconds=...)`
- 실제 보정 노드:
  - `StatusDamageBonusRate`
  - `StatusActionSpeedBonus`
  - `StatusShieldReceivedBonus`
  - `StatusDamageTakenBonus`
  - `StatusFlatElementResistReduction`
  - `StatusCriticalChanceBonus`

## Skill-by-skill conversion

### Ariel F

현재 기능:

- F 기본 효과: 모든 아군 신성 피해 +12%
- F trait 1: 모든 아군 신성 피해 +6%
- F trait 2: `ariel-a` 탄창 +2
- F trait 3: 신성 스킬 보유 아군 치명타 확률 +8%

유지할 노드:

- `ariel-f-trait-1-holy-damage-bonus`
  - `Choice`
  - `StatusDamageBonusRate`
  - `bonus=0.06`
- `ariel-f-trait-2-magazine-bonus`
  - `Choice`
  - `MagazineBonus`
  - `bonus=2`
- `ariel-f-party-holy-damage-status-modifier`
- `ariel-f-party-holy-damage-effect-target`
  - 남길 파라미터: `target_side=AllAllies`
- `ariel-f-party-holy-damage-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-f-party-holy-damage-status-damage-bonus-rate`
  - `bonus=0.12`
  - `attribute=Holy`
- `ariel-f-holy-skill-crit-trait3-status-modifier`
  - `requires_active_choice_id=ariel-f-trait-3`
- `ariel-f-holy-skill-crit-trait3-effect-target`
  - 남길 파라미터: `target_side=AllAllies`
- `ariel-f-holy-skill-crit-trait3-condition-skill-attribute`
  - `attribute=Holy`
- `ariel-f-holy-skill-crit-trait3-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-f-holy-skill-crit-trait3-status-critical-chance-bonus`
  - `bonus=0.08`

삭제 후보 파라미터:

- `ariel-f-party-holy-damage-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `center_mode=Caster`
- `ariel-f-holy-skill-crit-trait3-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `center_mode=Caster`

주의:

- `ariel-f-trait-2`의 적용 대상 `ariel-a`는 현재 `skill_choices_passive.csv`의 `target_skill_id=ariel-a`에 있다. 이 라우팅까지 node CSV로 옮기려면 런타임 적용 판단 구조를 함께 바꿔야 하므로 단순 CSV 정리 범위를 넘는다.

### Ariel G

현재 기능:

- G 기본 효과: 받는 보호막 증가
- G 시작 보호막: 모든 아군에게 시작 보호막 부여
- G trait 1: 받는 보호막량 추가 증가
- G trait 2: 시작 보호막량 증가
- G trait 3: 보호막 보유 아군 신성 피해 +10%

유지할 노드:

- `ariel-g-trait-1-shield-received-bonus`
  - `Choice`
  - `StatusShieldReceivedBonus`
  - `bonus=0.08`
- `ariel-g-trait-2-start-shield-amount-multiplier`
  - `Choice`
  - `ShieldAmountMultiplier`
  - `multiplier=1.4`
- `ariel-g-shield-received-status-modifier`
- `ariel-g-shield-received-effect-target`
  - 남길 파라미터: `target_side=AllAllies`
- `ariel-g-shield-received-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-g-shield-received-status-shield-received-bonus`
  - `bonus=0.18`
- `ariel-g-start-shield-apply-shield`
  - `base_damage=25`
  - `spell_power_coefficient=0.8`
- `ariel-g-start-shield-effect-target`
  - 남길 파라미터:
    - `target_side=AllAllies`
    - `apply_once=true`
- `ariel-g-start-shield-effect-lifetime`
  - `duration_seconds=9999`
- `ariel-g-shielded-holy-trait3-status-modifier`
  - `requires_active_choice_id=ariel-g-trait-3`
- `ariel-g-shielded-holy-trait3-effect-target`
  - 남길 파라미터: `target_side=AllAllies`
- `ariel-g-shielded-holy-trait3-condition-status`
  - `status_id=shield`
  - `target_side=AllAllies`
  - `min_stacks=1`
- `ariel-g-shielded-holy-trait3-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-g-shielded-holy-trait3-status-damage-bonus-rate`
  - `bonus=0.10`
  - `attribute=Holy`

삭제 후보 파라미터:

- `ariel-g-shield-received-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `center_mode=Caster`
- `ariel-g-start-shield-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `center_mode=Caster`
  - `visual_anchor_mode=AppliedTargets`
- `ariel-g-shielded-holy-trait3-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `center_mode=Caster`

주의:

- `apply_once=true`는 삭제하지 않는다. `InGamePassiveEffectRuntime`에서 one-shot 패시브 적용을 제어한다.

### Ariel H

현재 기능:

- H 기본 효과: blessing 보유 아군에게 신성 피해와 행동속도 보너스
- H trait 1: blessing의 신성 피해 증가량 추가
- H trait 2: blessing의 행동속도 증가량 추가
- H trait 3: blessing 지속시간 +2초

유지할 노드:

- `ariel-h-trait-1-blessed-holy-damage-bonus`
  - `Choice`
  - `StatusDamageBonusRate`
  - `bonus=0.07`
- `ariel-h-trait-2-blessed-action-speed-bonus`
  - `Choice`
  - `StatusActionSpeedBonus`
  - `bonus=0.05`
- `ariel-h-trait-3-duration-bonus`
  - `Choice`
  - `StatusDurationBonus`
  - `status_id=blessing`
  - `bonus_seconds=2`
- `ariel-h-blessed-holy-damage-speed-status-modifier`
- `ariel-h-blessed-holy-damage-speed-effect-target`
  - 남길 파라미터: `target_side=AllAllies`
- `ariel-h-blessed-holy-damage-speed-condition-status`
  - `status_id=blessing`
  - `target_side=AllAllies`
  - `min_stacks=1`
- `ariel-h-blessed-holy-damage-speed-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-h-blessed-holy-damage-speed-status-action-speed-bonus`
  - `bonus=0.10`
- `ariel-h-blessed-holy-damage-speed-status-damage-bonus-rate`
  - `bonus=0.15`
  - `attribute=Holy`

삭제 후보 파라미터:

- `ariel-h-blessed-holy-damage-speed-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `center_mode=Caster`

주의:

- H trait 1/2/3은 이미 Choice 기능 노드로 되어 있다. 이 수치를 effect 복사본으로 만들면 안 된다.

### Ariel I

현재 기능:

- I 기본 효과: holy-exposure 대상의 받는 피해 +10%
- I trait 1: 받는 피해 증가량 +5%
- I trait 2: `ariel-d` 쿨타임 -20%
- I trait 3: holy-exposure 대상의 Holy 저항 -8

유지할 노드:

- `ariel-i-trait-1-exposure-damage-taken-bonus`
  - `Choice`
  - `StatusDamageTakenBonus`
  - `bonus=0.05`
- `ariel-i-trait-2-cooldown-multiplier`
  - `Choice`
  - `CooldownMultiplier`
  - `multiplier=0.8`
- `ariel-i-holy-exposure-damage-taken-status-modifier`
- `ariel-i-holy-exposure-damage-taken-effect-target`
  - 삭제 후 비어도 되는지 검토 가능하다. 현재 대상 조건은 `ConditionStatus(target_side=Enemy)`가 핵심이다.
- `ariel-i-holy-exposure-damage-taken-condition-status`
  - `status_id=holy-exposure`
  - `target_side=Enemy`
  - `min_stacks=1`
- `ariel-i-holy-exposure-damage-taken-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-i-holy-exposure-damage-taken-status-damage-taken-bonus`
  - `bonus=0.10`
- `ariel-i-holy-exposure-holy-resist-trait3-status-modifier`
  - `requires_active_choice_id=ariel-i-trait-3`
- `ariel-i-holy-exposure-holy-resist-trait3-effect-target`
  - 삭제 후 비어도 되는지 검토 가능하다. 현재 대상 조건은 `ConditionStatus(target_side=Enemy)`가 핵심이다.
- `ariel-i-holy-exposure-holy-resist-trait3-condition-status`
  - `status_id=holy-exposure`
  - `target_side=Enemy`
  - `min_stacks=1`
- `ariel-i-holy-exposure-holy-resist-trait3-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-i-holy-exposure-holy-resist-trait3-status-flat-element-resist-reduction`
  - `bonus=8`
  - `attribute=Holy`

삭제 후보 파라미터:

- `ariel-i-holy-exposure-damage-taken-effect-target`
  - `target_shape=Battlefield`
  - `center_mode=Caster`
- `ariel-i-holy-exposure-holy-resist-trait3-effect-target`
  - `target_shape=Battlefield`
  - `center_mode=Caster`

주의:

- 현재 I 계열은 `EffectTarget`에 `target_side=Enemy`가 없고, 조건 노드인 `ConditionStatus`가 `target_side=Enemy`를 가진다. Code Builder는 현재 런타임에서 이 구성이 실제 대상 목록을 Enemy로 제한하는지 반드시 확인해야 한다. 만약 `EffectTarget`의 기본값 Enemy에 기대고 있다면 명시적으로 `target_side=Enemy`만 남기는 방향이 더 읽기 쉽다.

### Ariel J

현재 기능:

- J after-E: Ariel E 사용 이후 모든 아군 행동속도 +15%, 5초
- J trait 1: after-E 행동속도 증가량 +7%
- J shielded holy: Ariel E shield를 가진 아군 신성 피해 증가
- J trait 2: 신성 피해 증가량 +10%
- J trait 3: `ariel-e` 쿨타임 -15%

유지할 노드:

- `ariel-j-trait-1-after-e-action-speed-bonus`
  - `Choice`
  - `StatusActionSpeedBonus`
  - `bonus=0.07`
- `ariel-j-trait-2-shielded-holy-damage-bonus`
  - `Choice`
  - `StatusDamageBonusRate`
  - `bonus=0.10`
- `ariel-j-trait-3-cooldown-multiplier`
  - `Choice`
  - `CooldownMultiplier`
  - `multiplier=0.85`
- `ariel-j-after-e-action-speed-status-modifier`
- `ariel-j-after-e-action-speed-effect-target`
  - 남길 파라미터: `target_side=AllAllies`
- `ariel-j-after-e-action-speed-effect-lifetime`
  - `duration_seconds=5`
- `ariel-j-after-e-action-speed-status-action-speed-bonus`
  - `bonus=0.15`
- `ariel-j-shielded-holy-damage-status-modifier`
- `ariel-j-shielded-holy-damage-effect-target`
  - 남길 파라미터: `target_side=AllAllies`
- `ariel-j-shielded-holy-damage-condition-status`
  - `status_id=shield`
  - `target_side=AllAllies`
  - `min_stacks=1`
  - `source_skill_id=ariel-e-shield-base`
- `ariel-j-shielded-holy-damage-effect-lifetime`
  - `duration_seconds=0.5`
- `ariel-j-shielded-holy-damage-status-damage-bonus-rate`
  - `bonus=0.20`
  - `attribute=Holy`

삭제 후보 파라미터:

- `ariel-j-after-e-action-speed-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `visual_anchor_mode=AppliedTargets`
- `ariel-j-shielded-holy-damage-effect-target`
  - `target_selection=Owner`
  - `target_shape=Battlefield`
  - `center_mode=Caster`

주의:

- `source_skill_id=ariel-e-shield-base`는 삭제하지 않는다. J의 shield 조건을 Ariel E shield에 묶는 핵심 조건이다.

## Choice CSV handling

`skill_choices_passive.csv`에서 수치 컬럼은 비워 둔 현재 방향이 맞다. Ariel F-J의 수치와 효과 값은 node CSV가 소유해야 한다.

하지만 choice CSV 자체를 없애거나 `target_skill_id`를 무조건 삭제하면 안 된다.

현재 필요한 역할:

- 선택지 ID
- 선택지 제목/설명
- 선택 그룹과 정렬
- cross-skill 적용 라우팅

현재 확인된 cross-skill 라우팅:

- `ariel-f-trait-2` -> `target_skill_id=ariel-a`
- `ariel-h-trait-1/2/3` -> `target_skill_id=ariel-c`
- `ariel-i-trait-2` -> `target_skill_id=ariel-d`
- `ariel-j-trait-1/2/3` -> `target_skill_id=ariel-e`

이 라우팅은 `PakuriCsvRuntimeData.Build.cs`에서 Choice 노드의 기본 target으로 들어가고, `SkillExecutionSystem.AppliesToSkill(...)`도 choice의 `TargetSkillId`와 `RuntimeTargetSkillIds`를 기준으로 적용 여부를 판단한다. 따라서 현재 런타임 구조에서는 choice CSV의 라우팅을 남기는 것이 안전하다.

## Code Builder checklist

1. `passive_skill_node_params.csv`에서 F-J EffectTarget 파라미터를 기능별로 분류한다.
2. `visual_anchor_mode=AppliedTargets`는 같은 owner Effect 그룹에 `EffectVisual`이 없으면 제거한다.
3. `center_mode=Caster`는 같은 owner Effect 그룹에 `EffectVisual`이 없고 status/shield/extend 계열이면 제거한다.
4. `target_selection=Owner`는 status/shield/extend 계열에서 제거한다. `EventTarget`은 제거하지 않는다.
5. `target_shape=Battlefield`는 status/shield/extend 계열에서 제거 후보로 처리한다. `EffectDamage` 계열에는 이 규칙을 적용하지 않는다.
6. `target_side=AllAllies`와 `target_side=Enemy`는 대상 편을 결정하므로 남긴다.
7. `ConditionStatus`, `ConditionSkillAttribute`, `EffectLifetime`, 실제 보정 노드의 값은 남긴다.
8. `apply_once=true`는 패시브 one-shot 효과에 쓰이므로 남긴다.
9. choice CSV의 수치 컬럼은 계속 비워 둔다.
10. choice CSV의 `target_skill_id`는 현재 런타임 라우팅 근거가 있으므로 별도 코드 변경 없이 삭제하지 않는다.

## Suggested verification

- CSV shape check:
  - 모든 runtime CSV 행의 컬럼 수가 헤더와 일치해야 한다.
- Node-param reference check:
  - 모든 param `node_id`가 `passive_skill_nodes.csv`에 존재해야 한다.
- Removed-param scan:
  - F-J `EffectTarget`에서 제거하기로 한 파라미터가 남아 있지 않아야 한다.
- Retained-value scan:
  - F-J의 `bonus`, `multiplier`, `duration_seconds`, `attribute`, `status_id`, `source_skill_id`, `apply_once`가 남아 있어야 한다.
- Compile:
  - `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`
  - `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`

## History

- 2026-07-09: Ariel F-J passive nodes and params were inspected. The plan records how to reduce copied EffectTarget params while preserving actual functional nodes and choice routing.
