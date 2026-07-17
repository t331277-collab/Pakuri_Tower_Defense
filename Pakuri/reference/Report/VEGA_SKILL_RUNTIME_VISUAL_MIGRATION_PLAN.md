# Vega Skill Runtime Visual Migration Plan

## Goal

Vega A-E와 두 강화 실행 경로가 `Assets/Prefab/Skill/Vega/*.prefab`을 인스턴스화하지 않고, 활성 CSV와 그래프가 지정한 스프라이트·애니메이터·필요한 `BoxCollider2D`를 런타임에 조립하도록 전환한다.

프리팹 파일 자체의 삭제는 이 계획에 포함하지 않는다. 각 런타임 경로의 동작과 표현이 Play Mode에서 확인된 뒤 활성 CSV/그래프의 `skill_effect_prefab_path` 또는 `EffectVisual` 참조만 제거한다.

현재 활성 저작 경로는 `Pakuri/Assets/CSVdata/authoring/`이다. 2026-07-14 Code Builder 구현이 완료되었으며, 이번 작업은 스킬 표현/판정 리팩터링이므로 Skill Blueprint는 읽거나 변경하지 않았다.

## Inspected Evidence

- Unity-MCP로 `Assets/Prefab/Skill/Vega`를 조회한 결과 현재 스킬 프리팹은 `Vega_A`, `Vega_A_Master_2`, `Vega_B`, `Vega_C`, `Vega_D`, `Vega_E` 여섯 개다. 모두 단일 루트이며 자식 오브젝트가 없다.
- `Vega_A`와 `Vega_A_Master_2`는 `SpriteRenderer + BoxCollider2D`, `Vega_B`, `Vega_D`, `Vega_E`는 `SpriteRenderer + Animator + BoxCollider2D`, `Vega_C`는 `SpriteRenderer + Animator`로 구성된다.
- 활성 기본 CSV는 `vega-a`부터 `vega-e`까지 각각 위 프리팹을 참조한다. 추가로 `vega-b-master1-second-slash` Trigger가 `Vega_B.prefab`, `vega-a-master2-kill-transfer` Effect 그래프가 `Vega_A_Master_2.prefab`을 참조한다.
- 여섯 프리팹 GUID는 `NewRunScene.unity`에서 발견되지 않았다. Vega에는 제거해야 할 `EffectManager` 장면 매핑이 없으며, 현재 프리팹 의존은 CSV/그래프와 런타임 에셋 카탈로그에 한정된다.
- `RuntimeSkillVisualFactory`는 한 개의 스프라이트, 선택적 애니메이터 컨트롤러, 균일 초기 스케일, sorting order, 선택적 루트 `BoxCollider2D`를 조립할 수 있다. 여섯 프리팹의 단일 루트 구조는 이 표현 범위 안에 있다.
- `ProjectileSkillExecutor`는 `RuntimeVisual`을 프리팹보다 우선하며 `InGameProjectileActor`가 이동, 회전, 충돌과 trigger 상태를 담당한다.
- `InGameLineAttackActor.ApplyLineTick`은 공격 원점에서 길이 절반만큼 전진한 중심에 `length × width` 회전 `Physics2D.OverlapBox`를 실행하고, 결과에 포함된 대상 hitbox collider로 적중을 판정한다. 활성 collider가 없는 대상만 기존 중심점 수학 판정으로 fallback한다.
- `SkillTriggerRuntime.ExecuteLineAttackAction`도 Trigger 선형 공격을 같은 `ApplyLineTick` 경로로 처리한다. Trigger 자체에 runtime visual 열을 추가하지 않고 `triggered_skill_id` 또는 `source_skill_id`의 기본 스킬 runtime visual을 조회한다.
- `BuffSkillExecutor`는 자기 버프 대상 Transform에 런타임 비주얼을 `(0,0,0)` 로컬 위치로 부착하고 상태 지속시간 동안 유지할 수 있다.
- `InGameSkillDefinitionMapper`는 `vega-d`의 `deployment_required_target_status_id=name-mark` 때문에 `UsePrefabHitbox=true`, `UseMultiDeployment=true`를 강제한다. `SingleAttackSkillExecutor`는 각 표식 대상 중심에 만든 콜라이더로 D의 피해 대상을 판정한다.
- `vega-e`는 `UsePrefabHitbox=false`이며 반경/대상 선택으로 피해를 처리한 뒤 프리팹은 표현으로만 생성한다.
- `SkillMultiEffectExecutor`는 `RuntimeEffectVisual`에 hitbox가 있으면 콜라이더 기반 피해 경로로 전환한다. 현재 A master 2는 `EffectTarget=Nearest/Single` 피해 후 `EffectVisual`을 생성하므로 런타임 전환 때 hitbox를 넣으면 동작이 바뀐다.
- 기본 스킬과 Trigger CSV 파서는 선택적 런타임 비주얼/히트박스 필드를 읽을 수 있지만, 이번 작업은 `line_attack_skill_triger.csv`에 새 열을 추가하지 않았다.
- 현재 `RuntimeEffectVisual` 노드 정의는 스프라이트와 애니메이터 컨트롤러를 모두 필수로 요구한다. `Vega_A_Master_2.prefab`에는 Animator가 없으므로 정적 스프라이트 그래프 비주얼을 표현하려면 컨트롤러를 선택 항목으로 바꿔야 한다.

## Current Prefab Reference Map

| Runtime use | Current prefab authority | Gameplay collider authority |
|---|---|---|
| Vega A projectile | `base/projectile/skills_projectile.csv` `vega-a` | 예. 투사체 접촉 판정 |
| Vega A master 2 kill slash | projectile Effect graph `vega-a-master2-kill-transfer` | 아니오. `Nearest/Single` 대상이 먼저 결정됨 |
| Vega B base slash | `base/line_attack/skills_line_attack.csv` `vega-b` | 예. `length × width`, offset `(0,0)` 런타임 `OverlapBox` |
| Vega B master 1 second slash | `triggers/line_attack/line_attack_skill_triger.csv` | 예. base와 같은 런타임 `OverlapBox` |
| Vega C self buff | `base/buff/skills_buff.csv` `vega-c` | 없음 |
| Vega D marked-target deployments | `base/single_attack/skills_single_attack.csv` `vega-d` | 예. 각 배치의 box overlap 판정 |
| Vega E final judgment | `base/single_attack/skills_single_attack.csv` `vega-e` | 아니오. 선택된 단일 대상에 수학 판정 |

활성 Vega F-J 스킬/Trigger/Effect 그래프에는 스킬 프리팹 또는 런타임 비주얼 참조가 없다. F-J에 새 비주얼을 추가하는 작업은 이 리팩터링 범위가 아니다.

## Runtime Representation And Offset Boundary

- 런타임 루트 위치는 투사체 원점, 선형 공격 중심, 버프 대상, Effect 대상, 단일 공격 배치 중심을 각 executor가 결정한다. 프리팹에 저장된 루트 local position은 복사하지 않는다.
- 별도 오브젝트 위치 오프셋 열은 필요하지 않다.
- Vega A, A master 2, D, E의 프리팹 콜라이더 offset은 `(0,0)`이다.
- Vega B 프리팹 콜라이더의 기존 `(0, 0.10566831)` offset은 이전하지 않는다. B와 B master 1 판정은 공격 데이터의 길이/폭으로 만든 중심 offset `(0,0)` 런타임 `OverlapBox`가 담당한다.
- 따라서 이번 Vega 전환에는 `runtime_hitbox_offset_x/y` 열이나 그래프 param을 추가하지 않는다.
- 런타임 콜라이더가 필요한 대상은 A, D, B 계열이며 모두 offset `(0,0)`을 사용한다. A/D는 생성 오브젝트의 `BoxCollider2D`, B 계열은 `Physics2D.OverlapBox` query다.

## Decision Summary

| Target | Decision | Reason |
|---|---|---|
| Vega A | 기존 기본 projectile runtime visual 열로 전환 | 투사체 executor가 생성·회전·이동·trigger 충돌을 이미 담당함 |
| Vega A master 2 | `EffectVisual`을 hitbox 없는 `RuntimeEffectVisual`로 교체 | 기존 `Nearest/Single` 대상 판정을 유지해야 함 |
| Vega B | 기존 기본 line runtime visual 열로 전환하고 공유 LineAttack 판정을 collider query로 변경 | prefab collider 없이 데이터 길이/폭과 대상 collider로 판정 |
| Vega B master 1 | Trigger prefab 경로를 제거하고 base `vega-b` runtime visual을 코드에서 재사용 | Trigger CSV 새 열 없이 같은 표현과 collider 판정 유지 |
| Vega C | 기존 기본 buff runtime visual 열로 전환 | 자기 Transform 부착과 6초 수명이 공유 executor에 존재함 |
| Vega D | 기존 single-attack runtime visual + hitbox 열로 전환 | 표식 대상별 배치 피해가 생성된 box collider를 실제 판정에 사용함 |
| Vega E | 기존 single-attack runtime visual 열로 전환하고 hitbox 생략 | 현재 피해는 `HighestStacks` 단일 대상 판정이며 프리팹 collider는 사용되지 않음 |
| Vega F-J | 전환 대상 없음 | 활성 프리팹 참조가 없음 |

## Exact Runtime Values

### Vega A Base Projectile

Owner: `base/projectile/skills_projectile.csv`의 `vega-a`.

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Vega/SkillEffect/Vega_Shoot2.png
runtime_visual_animator_controller_path=
runtime_visual_scale=0.39
runtime_visual_sorting_order=0
runtime_hitbox_size_x=1.55
runtime_hitbox_size_y=1.63
```

런타임 투사체가 콜라이더를 trigger로 설정하므로 프리팹의 `isTrigger=false` 직렬화 값은 복사하지 않는다. Play Mode에서 3발 burst, 속도 `16`, pierce `999`, 이름표식 부여가 동일한지 확인한 뒤 `skill_effect_prefab_path`를 비운다.

### Vega A Master 2 Kill Slash

Owner: projectile Effect graph `Trigger/vega-a-master2-kill-transfer/Effect/0`, 현재 node order `3`.

```csv
node_type_id=RuntimeEffectVisual
runtime_visual_sprite_path=Assets/Image/Monster/Vega/SkillEffect/Vega_Shoot2.png
runtime_visual_animator_controller_path=
runtime_visual_scale=0.19792499
runtime_visual_sorting_order=0
runtime_hitbox_size_x=
runtime_hitbox_size_y=
```

`EffectDamage`의 공격력 계수 `0.5`, `EffectTarget=Enemy/Nearest/Single`, 이름표식 `3`스택은 변경하지 않는다. Hitbox를 비워야 `TryExecuteRuntimeHitboxDamageEffect`로 분기되지 않는다.

필수 공유 스키마 정리:

1. `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`의 `RuntimeEffectVisual` handler에서 `runtime_visual_animator_controller_path`를 required에서 optional로 이동한다.
2. `nodes/definitions/skill_node_definition_params.csv`에서 같은 param의 `required`를 `false`로 맞춘다.
3. 런타임 factory는 이미 null controller를 허용하므로 새 executor나 Vega 전용 코드는 추가하지 않는다.

### Vega B Base Slash

Owner: `base/line_attack/skills_line_attack.csv`의 `vega-b`.

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/B_1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/B_1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
runtime_hitbox_size_x=
runtime_hitbox_size_y=
```

프리팹의 비균일 scale `(1.1819649, 0.55388147, 0.55388147)`은 이전하지 않는다. `InGameLineAttackActor.ConfigureVisual()`이 X/Y를 현재 공격 길이와 폭 `1.8`로 다시 계산하므로 초기 균일 scale은 `1`이면 충분하다. 판정은 프리팹 collider size/offset을 복제하지 않고 현재 공격의 `length × 1.8`, offset `(0,0)` `Physics2D.OverlapBox`로 수행한다.

### Vega B Master 1 Second Slash

Owner: `triggers/line_attack/line_attack_skill_triger.csv`의 `vega-b-master1-second-slash`.

line Trigger CSV 헤더와 행 shape는 변경하지 않는다. `skill_effect_prefab_path`만 비우고, `SkillTriggerRuntime.ExecuteLineAttackAction()`이 Trigger runtime visual이 없을 때 `triggered_skill_id=vega-b`의 기본 runtime visual을 조회하도록 변경했다. 기존 delay `0.4`, damage multiplier `0.45`, width `1.8`, OnHit silence Effect graph는 변경하지 않는다.

### Vega C Self Buff

Owner: `base/buff/skills_buff.csv`의 `vega-c`.

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/ChatGPT Image 2026년 5월 28일 오후 10_11_00-Photoroom 1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/ChatGPT Image 2026년 5월 28일 오후 10_11_00-Photoroom 1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
runtime_hitbox_size_x=
runtime_hitbox_size_y=
```

버프 지속시간 `6`, 대상 `self`, 행동속도 `+0.25`, 공격력 `+0.2`는 변경하지 않는다. 루트 local position은 버리고 대상 Transform의 local `(0,0,0)`에 부착한다.

### Vega D Marked-Target Deployments

Owner: `base/single_attack/skills_single_attack.csv`의 `vega-d`.

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/D_1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/D_1.controller
runtime_visual_scale=0.7112
runtime_visual_sorting_order=0
runtime_hitbox_size_x=2.64
runtime_hitbox_size_y=2.63
runtime_visual_anchor=
```

`deployment_required_target_status_id=name-mark`, 최소 stack `1`, `hit_target_count=global`, radius `1.25`를 유지한다. D는 런타임 hitbox가 없으면 표시 오브젝트만 만들어지고 box overlap 결과가 0이 되므로 collider size 두 값을 반드시 함께 작성한다. Offset은 프리팹과 동일하게 `(0,0)`이다.

### Vega E Final Judgment

Owner: `base/single_attack/skills_single_attack.csv`의 `vega-e`.

```csv
runtime_visual_sprite_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/E_1.png
runtime_visual_animator_controller_path=Assets/Image/Monster/Vega/SkillEffect/Sprite/E_1.controller
runtime_visual_scale=1
runtime_visual_sorting_order=0
runtime_hitbox_size_x=
runtime_hitbox_size_y=
runtime_visual_anchor=
```

프리팹 collider size `(7.06, 7.24)`는 현재 실행 경로에서 피해 판정에 사용되지 않으므로 이전하지 않는다. `HighestStacks`, `name-mark>=1`, 스택 비례 피해와 50% 스택 소모는 그대로 유지한다.

## Implemented Changes

### Active CSV And Graph

1. A-E 기본 행에 위 runtime visual 값을 작성하고 prefab 경로를 비웠다.
2. line Trigger CSV에는 열을 추가하지 않고 B master 1의 prefab 경로만 비웠다. 실행 코드가 base B runtime visual을 재사용한다.
3. A master 2 그래프의 `EffectVisual`을 hitbox 없는 `RuntimeEffectVisual`로 바꿨다.
4. 정적 그래프 스프라이트를 허용하도록 `RuntimeEffectVisual` animator param을 optional로 정리했다.
5. Eve B, Vega B, Vega B master 1을 포함한 공유 LineAttack 판정을 런타임 collider query로 변경했다.

### Runtime Asset Catalog

다음 스프라이트와 컨트롤러가 `PakuriCsvRuntimeAssetCatalog.asset`에서 해석되도록 CSV runtime catalog sync를 실행하고 검증한다.

- `Vega_Shoot2.png`
- `B_1.png`, `B_1.controller`
- `ChatGPT Image 2026년 5월 28일 오후 10_11_00-Photoroom 1.png` 및 `.controller`
- `D_1.png`, `D_1.controller`
- `E_1.png`, `E_1.controller`

### Prefab And Scene

- 여섯 Vega 프리팹 파일은 유지한다.
- `NewRunScene.unity`에는 해당 프리팹 GUID 참조가 없으므로 장면 수정은 하지 않는다.
- 런타임 카탈로그에 남는 prefab entry 정리는 모든 CSV/그래프 prefab 참조가 제거된 뒤 별도 검증 단계에서 수행한다.

## Completed Migration Order

1. `RuntimeEffectVisual` animator param을 optional로 정리하고 CSV/그래프 validation을 통과시킨다.
2. Vega A, C, E처럼 기존 executor가 그대로 지원하는 표현-only/기본 projectile 경로를 작성한다.
3. Vega B base를 작성하고 B master 1이 base runtime visual을 재사용하도록 실행 코드를 연결한다.
4. Vega D의 런타임 collider를 작성하고 표식 대상별 overlap 판정을 우선 검증한다.
5. Vega A master 2 `EffectVisual`을 `RuntimeEffectVisual`로 교체한다.
6. runtime catalog를 sync하고 CSV validation, 런타임/editor build를 실행한다.
7. 모든 활성 Vega prefab 참조를 제거하고 정적 validation/build를 다시 실행했다.
8. 사용자 Play Mode에서 시각/판정 parity를 확인한다.

## Compatibility Constraints

- A의 투사체 burst/피해/표식 규칙은 변경하지 않는다.
- A master 2의 단일 대상 선택을 collider 기반 범위 피해로 바꾸지 않는다.
- B와 B master 1의 적중 폭은 각각 CSV radius `1.8`을 유지하고, offset `(0,0)` 런타임 collider query로 대상 collider를 판정한다.
- C의 비주얼은 caster에 붙어 상태 지속시간과 함께 제거되어야 한다.
- D는 표식이 있는 각 적 위치에 한 번씩 배치되어야 하며 런타임 collider 크기 `(2.64,2.63)`을 유지해야 한다.
- E는 prefab collider를 활성화하지 않고 기존 `HighestStacks` 단일 대상 판정을 유지해야 한다.
- 새 Vega 전용 MonoBehaviour, executor, hidden skill row 또는 scene mapping을 만들지 않는다.
- 오브젝트 position offset, collider offset, 비균일 scale용 새 CSV 열을 만들지 않는다.

## Risks And Containment

- A master 2에 hitbox 값을 넣으면 단일 대상 공격이 collider 범위 공격으로 바뀐다. 두 hitbox 값을 반드시 비운다.
- D의 hitbox 값을 하나라도 빼면 `UsePrefabHitbox` 경로가 collider 없는 런타임 오브젝트를 생성해 피해가 누락될 수 있다. 두 size 값을 한 변경으로 취급한다.
- B 프리팹의 비균일 scale 또는 collider offset을 복제하면 실행기 길이/폭/중심 권한과 충돌한다. 초기 scale `1`, query offset `(0,0)`을 유지한다.
- line Trigger CSV에는 새 열을 추가하지 않는다. 기존 37열 shape를 유지한다.
- A master 2용 schema optional 변경은 전역 node contract이므로 모든 기존 `RuntimeEffectVisual` 그래프 validation을 다시 실행한다.
- prefab 경로를 validation 전에 제거하면 catalog 누락이나 controller path 오타를 fallback이 가리지 못한다. 경로 제거는 마지막 단계다.

## Acceptance Criteria

- 활성 Vega A-E 기본 행과 두 강화 실행 경로에 Vega prefab path가 남지 않는다.
- `NewRunScene.unity`에는 Vega skill prefab 추가/변경이 없다.
- Vega A 투사체가 runtime-created sprite와 `(1.55,1.63)` trigger collider로 동일하게 적중한다.
- Vega A master 2가 가장 가까운 적 한 개체에만 기존 피해와 이름표식 3스택을 적용한다.
- Vega B와 B master 1이 runtime-created animated slash를 표시하고, 현재 길이/폭의 `Physics2D.OverlapBox`로 대상 collider를 감지해 기존 피해/침묵을 적용한다.
- Vega C 비주얼이 caster에 붙고 버프 종료 시 제거된다.
- Vega D가 `name-mark>=1`인 각 적 위치에 runtime-created `(2.64,2.63)` box를 만들고 대상별 피해를 적용한다.
- Vega E가 표식 최대 대상 한 개체를 계속 선택하며 prefab collider 없이 기존 피해와 스택 소모를 수행한다.
- CSV graph materialization과 runtime catalog validation이 오류 없이 통과한다.
- `Assembly-CSharp.csproj`와 `Assembly-CSharp-Editor.csproj` 빌드가 0 error다.
- Unity-MCP 오류 전용 Console 조회에 새 CSV/runtime/asset 오류가 없다.

## Verification Expected From Code Builder

- 변경 전후 prefab reference 검색 결과
- 변경한 모든 CSV의 header/row field-count 검사
- A-E runtime visual 및 B master 1 base-visual fallback의 asset catalog 해석 결과
- A master 2 그래프 materialization 결과와 단일 대상 유지 확인
- D의 런타임 BoxCollider2D size/offset 및 표식 대상별 overlap 확인
- runtime/editor build 결과
- Unity-MCP CSV sync/validation 및 error-only Console 결과
- 사용자 Play Mode 확인이 필요한 항목을 분리한 최종 보고

## Related Boards

- `boards/MON/VEGA_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`
- 참고 형식: `boards/MON/SEIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`

## Task: 2026-07-14 Vega Skill Runtime Visual Migration Design

### Task title

Design the prefab-to-runtime visual migration for Vega A-E and their prefab-backed enhancement paths.

### Goals

- Replace active Vega skill prefab instantiation with CSV/graph-authored runtime sprite, animator, and only gameplay-required box colliders.
- Preserve current targeting and damage authority instead of copying unused prefab colliders.
- Define the smallest shared schema changes needed for B master 1 and static A master 2 graph visuals.

### Constraints

- Role Owner is Designer.
- Evidence is limited to the referenced Sein plan, active Vega CSV/graph rows, six Vega prefabs, runtime execution code, and related MON/DATA boards.
- Skill Blueprints, unrelated monster plans, UI, RUN, COMBAT, archive boards, and `BLACKBOARD.md` were intentionally excluded.
- Implementation, prefab deletion, and Play Mode mutation are not included.

### Role Owner

Designer

### Status

Designed; ready for explicit Code Builder implementation request.

### Next Actions

- Code Builder implements the ordered CSV/schema/catalog changes in this document.
- User verifies visual and gameplay parity in Play Mode before fallback prefab paths are removed.

### Evidence

- Active Vega prefab references are at `skills_projectile.csv:9`, `skills_line_attack.csv:5`, `skills_buff.csv:5`, `skills_single_attack.csv:10-11`, `line_attack_skill_triger.csv:3`, and `skill_graph_nodes_projectile.csv:82`.
- `InGameLineAttackActor.ApplyLineTick`과 `SkillTriggerRuntime.ExecuteLineAttackAction`은 B 계열이 prefab collider 없이 런타임 box query를 사용함을 증명한다.
- `InGameSkillDefinitionMapper.cs:304-305` and `SingleAttackSkillExecutor.cs:440-475` prove Vega D requires a runtime collider for its current status-filtered deployments.
- `SkillMultiEffectExecutor.cs:356-465` proves adding a runtime hitbox to A master 2 would change its target resolution.
- Unity-MCP prefab hierarchy inspection and serialized prefab values provide the exact asset paths, scales, and collider sizes recorded above.

### History

- 2026-07-14: User requested a Vega prefab-to-runtime migration plan modeled on the Sein migration plan.
- 2026-07-14: Designer separated gameplay collider authority from presentation-only prefab colliders and identified runtime visual authoring requirements.
- 2026-07-14: User fixed all object/collider offsets to `(0,0)`, prohibited new CSV columns, and requested Eve B/Vega B collider-based LineAttack detection.
- 2026-07-14: Code Builder implemented shared LineAttack collider queries, base-visual fallback for B master 1, Vega runtime visual rows/graph, catalog sync, and validation.

## Task: 2026-07-14 Vega Runtime Visual And Line Collider Implementation

### Task title

Implement Vega prefab-independent runtime visuals and collider-based LineAttack detection.

### Goals

- Remove active Vega A-E, A master 2, and B master 1 skill-prefab references.
- Make Eve B, Vega B, and Vega B master 1 detect target colliders through runtime `Physics2D.OverlapBox`.
- Keep all runtime object/collider offsets `(0,0)` without adding CSV columns.

### Constraints

- Role Owner is Code Builder.
- `Assets/Prefab/Skill` values were evidence only; prefab and scene files were not modified or deleted.
- Skill Blueprint, unrelated boards, and MSW-MCP were excluded. Unity-MCP alone performed Editor sync/validation.

### Role Owner

Code Builder

### Status

Implemented and statically validated; user Play Mode parity remains.

### Next Actions

- User verifies Eve B, Vega B/base master slash overlap boundaries and Vega A/C/D/E visuals in Play Mode.
- If parity fails, inspect only the reported skill path and runtime instance state.

### Evidence

- `InGameLineAttackActor.ApplyLineTick` now runs a centered `length × width` rotated `Physics2D.OverlapBox` and matches returned colliders against each target's cached hitbox colliders.
- `SkillTriggerRuntime.ExecuteLineAttackAction` resolves missing Trigger visuals from the base `vega-b` runtime definition.
- Active runtime CSV/graph search returns no `Assets/Prefab/Skill/Vega` reference; line Trigger header remains 37 columns.
- Seven edited CSV files passed full row/header field-count checks.
- Unity-MCP catalog sync succeeded; CSV source load succeeded; InGame skill validation passed with 0 warnings.

### History

- 2026-07-14: Code Builder completed implementation under the user's zero-offset/no-new-column boundary.
