# Ariel Skill Graph Nodes 1차 마이그레이션 설계

## 문서 상태

- 역할: Designer 설계 → Code Builder 구현
- 상태: 1차 구현 및 Unity CSV 검증 완료, 사용자 Play Mode 회귀 검증 대기
- 대상: 현재 Ariel normalized node 데이터만 1차 전환
- 비대상: prefab/scene 변경, 다른 몬스터 graph 전환, legacy effects 삭제
- 절대 조건: 모든 판단은 현재 CSV와 런타임 코드에서 확인한 근거를 사용한다.

## 1. 목표

현재 스킬 동작과 수치가 다음 세 authoring 경로에 나뉘어 있는 문제를 단계적으로 제거한다.

1. `choices/{kind}/skill_choices_{kind}.csv`의 legacy wide 동작 컬럼
2. `effects/{kind}/{kind}_skill_effects.csv`의 legacy wide Effect 행
3. `nodes/{kind}/{kind}_skill_nodes.csv`와 `*_skill_node_params.csv`의 node instance와 실제 값

최종 책임은 다음처럼 만든다.

```text
nodes/definitions
└─ 노드 종류와 파라미터 계약만 정의

choices/{kind}/skill_graph_nodes_{kind}.csv
└─ 그래프 소유자, 노드 순서, 사용할 노드 종류, 실제 수치를 정의

C# handler
└─ handler_id가 실제로 수행할 동작을 구현

effects/{kind}
└─ 모든 몬스터의 legacy Effect 전환이 끝난 후 삭제
```

1차 구현은 Ariel만 새 `skill_graph_nodes` 경로로 옮긴다. 다른 몬스터와 legacy effects 경로는 그대로 작동해야 한다.

## 2. 현재 근거

### 2.1 현재 데이터 수

현재 디스크 CSV를 type row 제외 기준으로 집계한 결과다.

| 항목 | 현재 수 |
|---|---:|
| 전체 normalized nodes | 139 |
| 전체 node params | 212 |
| Ariel nodes | 124 |
| Ariel node params | 179 |
| Ariel Choice-owned nodes | 39 |
| Ariel Effect-owned nodes | 85 |
| Ariel Effect 논리 그룹 | 20 |
| Ariel에서 사용하는 handler 종류 | 32 |
| Rin/Vega legacy nodes | 15 |
| Rin/Vega legacy node params | 33 |
| legacy effects CSV 행 | 96 |
| Ariel legacy effects CSV 행 | 0 |

Ariel node 124개의 실제 param 개수 분포는 다음과 같다.

| 한 노드의 실제 param 수 | 노드 수 |
|---|---:|
| 0 | 11 |
| 1 | 79 |
| 2 | 14 |
| 3 | 10 |
| 4 | 8 |
| 5 | 2 |

현재 Ariel 실제 데이터에서는 한 노드가 최대 5개 값을 사용한다. 다만 현재 공용 handler 계약에는 `ApplyShield`처럼 최대 12개 optional param을 허용하는 handler가 있으므로 최종 graph row는 12개 argument slot을 지원한다.

### 2.2 현재 choices의 Ariel 수치 컬럼 상태

현재 Ariel Choice는 50행이다.

| choice_group | 행 수 |
|---|---:|
| `ActiveEnhancement` | 25 |
| `ActiveMaster` | 10 |
| `PassiveEnhancement` | 15 |

현재 Ariel 50행은 `description_text` 뒤의 legacy wide 동작 컬럼이 모두 비어 있다. 실제 Ariel 강화/마스터 수치는 현재 node param CSV가 보유한다.

따라서 1차 전환에서는 Choice wide 값과 node param 값을 병합하지 않는다. 현재 node param의 실제 값을 새 `skill_graph_nodes`로 이동하고 Choice CSV는 선택지 메타데이터 소유자로 유지한다.

### 2.3 현재 Effect 생성 경로

`PakuriCsvRuntimeData.Build.BuildSkillEffects`는 다음 두 결과를 합친다.

1. `model.SkillEffects`에 로드한 legacy effects CSV 행
2. `owner_kind=Effect` node를 `owner_id`별로 묶어 만든 node-built Effect

현재 Ariel의 85 Effect-owned nodes는 20개 Effect 그룹을 만들지만 effects CSV의 `effect_id`를 참조하지 않는다. 현재 20개 owner id와 effects CSV의 96개 id 사이의 일치 항목은 0개다.

따라서 `owner_kind=Effect`의 실제 의미는 “effects CSV가 소유한다”가 아니라 “여러 node를 하나의 `SkillEffectDefinition`으로 합성한다”다.

### 2.4 현재 런타임 우선순위

`SkillExecutionSnapshot.ApplyChoiceDefinition`은 Choice에 normalized nodes가 하나라도 있으면 legacy Choice wide 값 적용 경로를 사용하지 않고 node-backed 경로로 즉시 분기한다.

현재 validation도 동일 Choice에서 node handler와 대응 legacy wide 값이 동시에 활성화되면 overlap 오류를 발생시킨다.

즉 현재 문제는 실제 이중 적용보다 같은 기능의 authoring 계약이 여러 CSV에 존재한다는 책임 중복이다.

## 3. 핵심 설계 결정

### 3.1 `choice_id`를 그래프 ID처럼 사용할 수 있는가

Choice가 소유하는 그래프에서는 사용할 수 있다.

기존 이름:

```text
ariel-a-master-2-holy-exposure-on-hit
```

새 authoring identity:

```text
owner_kind = Choice
owner_id = ariel-a-master-2
graph_kind = Effect
graph_index = 0
```

여기서 `owner_id`는 실제 `choice_id`다. 긴 행동 설명을 primary id로 사용하지 않는다.

다만 모든 그래프에 `choice_id`를 강제로 사용할 수는 없다.

- 기본 스킬 Effect에는 Choice가 없다.
- 패시브 기본 Effect에는 Choice가 없을 수 있다.
- Trigger 자체가 실행하는 Effect에는 Choice가 없을 수 있다.

따라서 전체 그래프의 주 키는 다음 복합 키로 정의한다.

```text
monster_id
+ owner_kind
+ owner_id
+ graph_kind
+ graph_index
```

`owner_id` 규칙은 다음과 같다.

| owner_kind | owner_id |
|---|---|
| `Choice` | 실제 `choice_id` |
| `Skill` | 실제 `skill_id` |
| `Trigger` | 실제 `trigger_id` |

### 3.2 `graph_kind`

그래프가 실행계획 modifier인지 하나의 Effect 합성 그룹인지 구분한다.

| graph_kind | 의미 |
|---|---|
| `Plan` | Choice/Skill snapshot에 직접 적용되는 condition, damage modifier, crit modifier, action node |
| `Effect` | 하나의 `SkillEffectDefinition`으로 합성되는 operation/target/condition/lifetime/visual/modifier node 그룹 |

같은 Choice가 Plan modifier와 독립 Effect를 모두 가질 수 있으므로 `owner_kind`만으로는 이 둘을 구분하지 않는다.

### 3.3 `graph_index`

한 owner가 같은 종류의 그래프를 여러 개 가질 수 있게 하는 0부터 시작하는 정수다.

예:

```text
Choice / ariel-x-master-1 / Plan / 0
Choice / ariel-x-master-1 / Effect / 0
Choice / ariel-x-master-1 / Effect / 1
```

긴 Effect 이름 대신 `choice_id + graph_kind + graph_index`로 그룹을 식별한다.

### 3.4 authored `node_id` 제거

새 `skill_graph_nodes`에는 현재와 같은 긴 `node_id`를 작성하지 않는다.

런타임/로그용 NodeId는 loader/build 단계에서 다음 값으로 결정적으로 생성한다.

```text
{owner_kind}:{owner_id}:{graph_kind}:{graph_index}:{node_order}
```

예:

```text
Choice:ariel-a-master-2:Effect:0:1
Choice:ariel-a-master-2:Effect:0:2
```

현재 `SkillExecutionPlanNode.RowId`는 plan row identity로 전달되므로 생성 ID는 실행마다 동일해야 한다.

### 3.5 runtime EffectId 생성

Effect 그래프는 현재 runtime이 사용하는 `SkillEffectDefinition.EffectId`를 계속 제공해야 한다. EffectId는 status source identity, 동일 대상 중복 방지, trigger effect 검색에 사용된다.

새 규칙은 다음과 같다.

| owner | 첫 Effect graph | 추가 Effect graph |
|---|---|---|
| Choice | `{choice_id}` | `{choice_id}@effect{index+1}` |
| Skill | `{skill_id}@effect1` | `{skill_id}@effect{index+1}` |
| Trigger | `{trigger_id}` | `{trigger_id}@effect{index+1}` |

`ariel-a-master-2-holy-exposure-on-hit`의 새 runtime EffectId는 첫 Choice Effect graph이므로 `ariel-a-master-2`가 된다.

EffectId 변경은 status source 비교에 영향을 줄 수 있다. Ariel 현재 데이터에서 기존 Effect owner id를 직접 참조하는 곳은 다음 세 곳이다.

- `ariel-a-master2-holy-exposure-on-hit` trigger의 `triggered_effect_id`
- `ariel-j-after-e-action-speed-trigger`의 `triggered_effect_id`
- `ariel-j-shielded-holy-damage-condition-status`의 `source_skill_id=ariel-e-shield-base`

1차 구현에서 이 세 참조를 graph reference로 함께 변경해야 한다. 문자열만 변경하고 참조 코드를 그대로 두어서는 안 된다.

## 4. 새 CSV 책임과 스키마

### 4.1 Node type definition 파일

신규 경로:

```text
Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/definitions/skill_node_definitions.csv
Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/definitions/skill_node_definition_params.csv
```

`skill_node_definitions.csv`:

```csv
node_type_id,handler_id,node_kind,runtime_support_state,runtime_support_notes
```

1차에서는 불필요한 alias 중복을 피하기 위해 `node_type_id`를 기존 `handler_id`와 동일하게 사용한다.

예:

```csv
DamageMultiplier,DamageMultiplier,DamageModifier,RuntimeImplemented,
ApplyStatus,ApplyStatus,Action,RuntimeImplemented,
EffectTarget,EffectTarget,Action,RuntimeImplemented,
```

`skill_node_definition_params.csv`:

```csv
node_type_id,param_order,param_key,value_type,required,allowed_values
```

예:

```csv
DamageMultiplier,1,multiplier,float,true,
ApplyStatus,1,status_id,status_id,true,
ApplyStatus,2,status_chance,float,false,
EffectTarget,1,target_side,enum,false,Enemy|AllAllies|Self
EffectTarget,2,target_selection,enum,false,Nearest|EventTarget|Owner
```

정의 파일에는 실제 게임 수치를 넣지 않는다. `1.25`, `holy-exposure`, `3` 같은 값은 모두 graph node row에만 존재한다.

1차 Ariel 구현에서는 현재 Ariel이 실제 사용하는 32 handler 정의만 추가한다. Rin/Vega가 legacy node path에 남아 있는 동안 기존 C# handler schema는 호환 validation을 위해 유지한다.

최종 전환 후에는 CSV node definition을 authoring schema의 단일 기준으로 사용하고, C# registry는 `handler_id` 실행 구현이 존재하는지만 검증한다.

### 4.2 Skill graph node 파일

신규 경로:

```text
Pakuri/Assets/CSVdata/runtime/monster/skills/choices/projectile/skill_graph_nodes_projectile.csv
Pakuri/Assets/CSVdata/runtime/monster/skills/choices/buff/skill_graph_nodes_buff.csv
Pakuri/Assets/CSVdata/runtime/monster/skills/choices/single_attack/skill_graph_nodes_single_attack.csv
Pakuri/Assets/CSVdata/runtime/monster/skills/choices/passive/skill_graph_nodes_passive.csv
```

1차 Ariel에는 area/line graph row가 없으므로 해당 신규 파일을 만들지 않는다. 향후 실제 row가 생길 때 같은 규칙으로 추가한다.

헤더:

```csv
monster_id,owner_kind,owner_id,graph_kind,graph_index,target_skill_id,node_order,node_type_id,arg_1,arg_2,arg_3,arg_4,arg_5,arg_6,arg_7,arg_8,arg_9,arg_10,arg_11,arg_12,excludes_active_choice_id
```

규칙:

- `target_skill_id`가 비어 있으면 owner에서 파생한다.
- Choice owner는 Choice 행의 `target_skill_id`, `runtime_target_skill_ids`, `skill_id` 순서에 맞는 현재 resolution 규칙을 재사용한다.
- Skill owner는 `owner_id`를 기본 target으로 사용한다.
- Trigger owner는 trigger source/graph reference에서 target을 결정한다.
- `node_order`는 같은 graph 안에서 고유해야 한다.
- `arg_n`은 definition param의 `param_order=n`에 대응한다.
- `arg_n` CSV type row는 전부 `string`으로 읽고 definition의 `value_type`으로 재검증한다.
- required arg가 비면 validation error다.
- definition에 없는 추가 arg가 채워져 있으면 validation error다.
- Choice 소유 Effect의 required-choice 조건은 `owner_kind=Choice + owner_id`에서 loader가 자동 생성하므로 별도 컬럼을 두지 않는다.
- 현재 Ariel에서 실제 필요한 graph gate는 기본 `ariel-c` Effect를 `ariel-c-master-1` 선택 시 막는 `excludes_active_choice_id` 1건뿐이므로 이 컬럼만 유지한다.
- passive gate 두 컬럼은 현재 값과 사용 행이 없어서 제거하고, 향후 필요하면 별도 condition node 설계를 우선한다.
- runtime support 상태와 메모는 graph instance 값이 아니라 node definition/handler 구현 책임이므로 graph CSV에서 제거한다.
- 12개를 넘는 param이 필요한 새 handler는 graph CSV 컬럼을 즉시 늘리지 않고 handler 분해 가능성을 먼저 검토한다.

### 4.3 `ariel-a-master-2` 변환 예

현재 node instance:

```text
owner_id = ariel-a-master-2-holy-exposure-on-hit

ApplyStatus
└─ status_id = holy-exposure

EffectTarget
├─ target_selection = EventTarget
├─ center_mode = EffectTarget
└─ visual_anchor_mode = AppliedTargets
```

새 graph rows:

```csv
ariel,Choice,ariel-a-master-2,Effect,0,ariel-a,1,ApplyStatus,holy-exposure,,,,,,,,,,,,
ariel,Choice,ariel-a-master-2,Effect,0,ariel-a,2,EffectTarget,,EventTarget,,EffectTarget,AppliedTargets,,,,,,,
```

`EffectTarget`의 arg 위치는 definition param order가 결정한다. 빈 값은 handler default를 유지한다.

이제 실제 수치 `holy-exposure`와 target 설정은 `skill_graph_nodes_projectile.csv`만 소유한다. nodes definition은 `ApplyStatus`와 `EffectTarget`이 어떤 param을 받는지만 정의한다.

## 5. Trigger graph 참조

현재 trigger CSV의 `triggered_effect_id`는 EffectId 문자열을 직접 참조한다. 새 graph identity를 참조할 수 있도록 다음 optional source 컬럼을 추가한다.

```csv
triggered_graph_owner_kind,triggered_graph_owner_id,triggered_graph_kind,triggered_graph_index
```

1차 Ariel 변환:

| trigger_id | 새 graph ref |
|---|---|
| `ariel-a-master2-holy-exposure-on-hit` | `Choice / ariel-a-master-2 / Effect / 0` |
| `ariel-j-after-e-action-speed-trigger` | `Trigger / ariel-j-after-e-action-speed-trigger / Effect / 0` |

Build 단계는 graph ref를 generated runtime EffectId로 변환하여 기존 `SkillTriggerDefinition.TriggeredEffectId`에 넣는다. 따라서 `SkillTriggerRuntime.ResolveTriggeredEffect`와 `SkillMultiEffectExecutor`는 1차에서 변경하지 않는 것을 우선한다.

Rin/Vega legacy trigger는 기존 `triggered_effect_id`를 계속 사용한다. 한 trigger 행에서 legacy effect id와 graph ref가 동시에 채워지면 validation error다.

## 6. Ariel 1차 데이터 이동 범위

### 6.1 이동 대상

현재 Ariel node 124개와 param 179개를 새 graph row 124개로 변환한다.

현재 skill-kind별 Ariel node 수:

| kind | graph row 수 |
|---|---:|
| projectile | 7 |
| buff | 10 |
| single_attack | 49 |
| passive | 58 |
| 합계 | 124 |

새 owner 분류:

| 새 owner/graph | node row 수 | 근거 |
|---|---:|---|
| `Choice / Plan` | 39 | 현재 `owner_kind=Choice` nodes |
| `Choice / Effect` | 45 | Choice gate가 있는 11 Effect 그룹 |
| `Skill / Effect` | 36 | Choice gate가 없는 기본/패시브 Effect 그룹 |
| `Trigger / Effect` | 4 | `ariel-j-after-e-action-speed-trigger`가 직접 실행하는 Effect 그룹 |
| 합계 | 124 | 현재 Ariel 전체 node 수와 동일 |

`ariel-a-master-2` Effect graph는 Trigger가 실행하지만 Choice가 실제 기능과 값을 소유하므로 `Choice / Effect`로 분류하고 trigger가 graph ref를 가진다.

### 6.2 legacy node 파일 처리

1차 완료 후 기존 kind별 node instance CSV에서는 Ariel 행만 제거한다.

남아야 하는 legacy 데이터:

| 몬스터 | legacy nodes | legacy params |
|---|---:|---:|
| Rin | 11 | 현재 실제 행 유지 |
| Vega | 4 | 현재 실제 행 유지 |
| 합계 | 15 | 33 |

기존 node loader와 builder는 이 15개 legacy node를 계속 처리해야 한다.

동일 Ariel 기능을 old node row와 graph row 양쪽에 남기면 중복 실행 가능성이 있으므로 Ariel graph 변환과 old row 제거는 같은 구현 change에서 수행한다.

### 6.3 이번 1차에서 이동하지 않는 것

- `effects/{kind}`의 96 legacy rows
- Eve/Rin/Sein/Vega effects
- Rin/Vega normalized node instances
- base skill의 기본 공격력, cooldown, magazine 등 skill chassis 값
- trigger의 event 조건 자체
- trigger가 직접 보유한 damage source, 반복, runtime visual wide fields
- prefab/scene 구조

Ariel trigger wide payload 자체까지 graph node로 바꾸려면 `damage_source`, tracked damage, shield absorbed amount, repeat, triggered skill spawn을 표현하는 추가 node 계약이 필요하다. 현재 존재하는 Ariel node instance 이동과 다른 shared runtime 확장이므로 1차 호환 마이그레이션에서 제외한다.

## 7. 런타임 구현 표면

### 7.1 Source catalog/editor sync

관련 파일:

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeSourceCatalog.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Loader.cs`

필요 작업:

1. node definition TextAsset 필드를 추가한다.
2. `skill_graph_nodes_*.csv`를 재귀 수집한다.
3. legacy `*_skill_nodes.csv`, `*_skill_node_params.csv`, `*_skill_effects.csv` 수집은 유지한다.
4. 신규 graph 파일이 일부 kind에만 있어도 정상 로드해야 한다.

### 7.2 Source model/parser

관련 파일:

- `PakuriCsvRuntimeData.SourceModel.cs`
- `PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`
- 필요하면 신규 partial `PakuriCsvRuntimeData.SkillGraphAuthoring.cs`

신규 row model:

```text
SkillNodeTypeRow
SkillNodeTypeParamRow
SkillGraphNodeRow
SkillGraphKey
```

기존 `SkillNodeRow`, `SkillNodeParamRow`는 Rin/Vega compatibility를 위해 유지한다.

### 7.3 Validation

필수 validation:

1. `node_type_id`가 definition에 존재한다.
2. definition의 `handler_id`가 runtime handler에 등록되어 있다.
3. owner kind에 맞는 `choice_id`, `skill_id`, `trigger_id`가 존재한다.
4. graph 복합 키 안에서 `node_order`가 중복되지 않는다.
5. Plan graph에는 Effect operation/composition node를 넣지 않는다.
6. Effect graph에는 정확히 하나의 operation handler가 있어야 한다.
7. required arg가 모두 존재한다.
8. arg 값이 definition type과 enum 허용 값에 맞는다.
9. Ariel graph row와 Ariel legacy node row가 동시에 존재하지 않는다.
10. trigger에서 legacy `triggered_effect_id`와 graph ref를 동시에 사용하지 않는다.
11. graph ref가 실제 Effect graph를 가리킨다.
12. generated runtime EffectId가 같은 monster 안에서 중복되지 않는다.

현재 `ValidateNormalizedSkillAuthoringRows`의 required/allowed param, enum, asset/skill/status/choice reference 검증을 graph arg 검증에 재사용한다.

### 7.4 Build

관련 파일:

- `PakuriCsvRuntimeData.Build.cs`
- `SkillDefinition.cs`

신규 build 책임:

```text
SkillGraphNodeRow
  -> definition param order로 arg를 이름/타입 param으로 변환
  -> generated NodeId 생성
  -> graph_kind=Plan이면 SkillNodeDefinition 생성
  -> graph_kind=Effect이면 graph별 SkillEffectDefinition 합성
```

Choice build:

- 새 Choice Plan graph를 `SkillChoiceDefinition.NormalizedPlanNodes`로 만든다.
- legacy Choice-owned node가 있는 non-Ariel은 기존 build를 사용한다.
- 동일 Choice에 두 경로가 동시에 존재하면 error다.

Effect build:

- 기존 legacy effects CSV 결과를 먼저 유지한다.
- 새 graph-owned Effect 결과를 추가한다.
- legacy Effect-owned node 결과는 Ariel 외 legacy rows에 대해서만 유지한다.
- generated EffectId를 사용한다.

Trigger build:

- graph ref가 있으면 해당 graph의 generated EffectId를 기존 `TriggeredEffectId`에 기록한다.
- legacy ref이면 기존 값을 유지한다.

### 7.5 기존 실행 런타임 유지 범위

1차 목표는 graph CSV를 기존 runtime definition으로 compile하는 것이다. 가능하면 다음 실행 계층은 변경하지 않는다.

- `InGameSkillDefinitionMapper`
- `SkillExecutionSnapshot.ApplyNodeBackedChoiceDefinition`
- `SkillExecutionPlan`
- `SkillMultiEffectExecutor`
- `SkillTriggerRuntime.ResolveTriggeredEffect`

이 계층이 받는 `SkillNodeDefinition`, `SkillEffectDefinition`, `TriggeredEffectId` 결과가 현재와 동등하도록 builder에서 호환한다.

## 8. effects 삭제 정책

1차 Ariel 구현에서는 effects 디렉터리를 삭제하지 않는다.

현재 effects 96행은 다음 몬스터가 사용한다.

| 몬스터 | legacy effects |
|---|---:|
| Eve | 34 |
| Rin | 20 |
| Sein | 19 |
| Vega | 23 |

기존 감사 결과:

| 전환 난이도 | 행 수 |
|---|---:|
| 현재 handler로 즉시 표현 가능 | 58 |
| 기존 handler Effect composer 확장 필요 | 16 |
| 새로운 semantic handler 필요 | 22 |

삭제 조건:

1. 96행 모두 graph node로 이동했다.
2. 각 legacy `effect_id`를 참조하던 trigger가 graph ref로 이동했다.
3. legacy effect와 graph effect가 동시에 생성되지 않는다.
4. `BuildSkillEffects`에서 `model.SkillEffects` 경로를 제거해도 전체 Effect 수와 실행 순서가 유지된다.
5. source catalog/editor sync에서 `_skill_effects.csv` 수집을 제거했다.
6. validation에서 legacy effect reference가 0건이다.
7. compile/editor validation과 사용자 Play Mode 검증이 끝났다.

이 조건 전에는 effects 파일이나 loader를 삭제하지 않는다.

## 9. 단계별 구현 순서

### Phase 1: definition과 graph loader 추가

1. 신규 CSV row model과 source catalog 필드를 추가한다.
2. Ariel이 사용하는 32 node type definition을 작성한다.
3. graph arg validation을 추가한다.
4. 아직 기존 Ariel node rows를 제거하지 않고 loader/build 단위 테스트용으로 graph parsing만 검증한다.

이 단계에서는 graph 결과를 runtime에 동시에 활성화하지 않는다.

### Phase 2: Ariel graph 데이터 생성

1. Ariel 124 node와 179 param을 복합 graph row 124개로 변환한다.
2. handler param order에 맞춰 `arg_1..arg_12`를 채운다.
3. 39 Choice Plan nodes를 옮긴다.
4. 20 Effect 그룹을 Choice 11, Skill 8, Trigger 1 owner로 다시 분류한다.
5. runtime support 메타데이터는 node definition/handler 책임으로 유지하고 실제 asset/status/skill reference를 보존한다.

### Phase 3: runtime build 전환

1. Ariel Choice graph를 `NormalizedPlanNodes`로 build한다.
2. Ariel Effect graph를 `SkillEffectDefinition`으로 build한다.
3. 두 Ariel trigger의 graph ref를 build한다.
4. generated runtime EffectId에 맞춰 Ariel 내부 effect-source graph reference를 변환한다.

### Phase 4: Ariel legacy node rows 제거

1. 기존 nodes/params 파일에서 Ariel 124/179행을 제거한다.
2. Rin/Vega 15/33행이 그대로 남았는지 확인한다.
3. Ariel old node id가 0건인지 확인한다.
4. Ariel graph node가 정확히 124행인지 확인한다.

### Phase 5: 검증

1. CSV shape/type row 검증
2. definition/graph foreign key 검증
3. graph operation 수 검증
4. generated NodeId/EffectId 중복 검증
5. runtime/editor compile
6. Unity-MCP catalog sync와 validation
7. 사용자 Play Mode Ariel A-J 회귀 검증

## 10. 호환성과 위험

### 10.1 EffectId 변경

EffectId는 단순 표시 문자열이 아니다. status source와 target 중복 방지에 사용된다. graph owner 기반 EffectId로 바꿀 때 모든 내부 참조를 함께 바꿔야 한다.

### 10.2 positional arg

definition의 `param_order`를 바꾸면 기존 graph row의 의미가 바뀐다.

규칙:

- 출시된 node type의 기존 param order는 변경하지 않는다.
- 새 optional param은 마지막 순서에만 추가한다.
- 중간 삽입이 필요하면 새 node type/version을 만든다.

### 10.3 Choice target resolution

현재 일부 Choice node의 `target_skill_id`가 비어 있고 build가 Choice의 기본 target을 사용한다. graph 변환에서도 같은 resolution을 사용해야 하며 모든 blank를 임의로 `skill_id`로 덮어쓰면 안 된다.

### 10.4 legacy 병행 경로

전환 중에는 다음 세 경로가 동시에 존재한다.

- Ariel graph nodes
- Rin/Vega legacy normalized nodes
- Eve/Rin/Sein/Vega legacy effects

monster/owner 기준으로 단일 경로만 활성화되도록 validation해야 한다.

### 10.5 Trigger wide payload

현재 Ariel trigger 중 폭발, 반사, 추적 피해 등은 trigger wide 필드가 실제 damage source를 소유한다. 이를 graph로 옮기는 것은 새 handler/graph contract 설계가 필요한 후속 작업이다. 1차 node storage migration 성공 조건에 섞지 않는다.

## 11. 완료 기준

Code Builder 구현은 다음 조건을 모두 만족해야 한다.

- [x] node definition CSV 2개가 존재하고 Ariel 사용 handler 32종을 정의한다.
- [x] Ariel `skill_graph_nodes_{kind}.csv` graph row 합계가 124다.
- [x] Ariel graph arg 값이 기존 node param 179개의 의미와 일치한다.
- [x] Ariel legacy node row가 0개다.
- [x] Ariel legacy node param row가 0개다.
- [x] Rin/Vega legacy node 15개와 param 33개가 유지된다.
- [x] legacy effects 96행이 변경 없이 유지된다.
- [x] Choice/Skill/Trigger graph owner reference가 모두 유효하다.
- [x] Effect graph 20개가 정확히 하나의 operation node를 가진다.
- [x] `ariel-a-master-2` authored Effect graph owner id가 실제 choice id다.
- [x] Ariel 두 effect trigger가 legacy effect string 대신 graph ref를 사용한다.
- [x] Ariel 내부 effect-source 참조가 새 graph identity로 resolve된다.
- [x] 동일 Ariel 기능이 graph와 legacy node 양쪽에서 생성되지 않는다.
- [x] 기존 `SkillNodeDefinition`, `SkillEffectDefinition`, execution plan에 동등한 runtime 구조가 생성된다.
- [x] runtime/editor build가 0 error다.
- [x] Unity-MCP CSV sync/validation에 fatal error가 없다.
- [ ] 사용자 Play Mode에서 Ariel A-J 기본/특성/마스터 조합을 검증한다.

구조 및 에디터 검증은 2026-07-11 Code Builder 구현에서 완료했다. 마지막 항목은 실제 플레이 조작이 필요한 사용자 검증이므로 완료 처리하지 않는다.

## 12. Code Builder 검증 명령/증거 요구

Code Builder는 최소한 다음 증거를 남긴다.

1. CSV header/type row와 모든 data row field count
2. definition id 중복 0건
3. graph 복합 키 + node_order 중복 0건
4. unknown owner/node type/graph ref 0건
5. Ariel graph rows 124
6. Ariel legacy nodes/params 0
7. Rin/Vega legacy nodes 15, params 33
8. legacy effects 96
9. generated Effect graph 20개와 trigger graph ref resolution 결과
10. `dotnet build Pakuri/Assembly-CSharp.csproj`
11. `dotnet build Pakuri/Assembly-CSharp-Editor.csproj`
12. Unity-MCP catalog sync/validation console 결과

Unity Play Mode gameplay verification은 사용자 소유다.

## 13. 관련 파일

현재 CSV:

- `Pakuri/Assets/CSVdata/runtime/monster/skills/choices/{kind}/skill_choices_{kind}.csv`
- `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/{kind}/{kind}_skill_nodes.csv`
- `Pakuri/Assets/CSVdata/runtime/monster/skills/nodes/{kind}/{kind}_skill_node_params.csv`
- `Pakuri/Assets/CSVdata/runtime/monster/skills/effects/{kind}/{kind}_skill_effects.csv`
- `Pakuri/Assets/CSVdata/runtime/monster/skills/triggers/{kind}/{kind}_skill_triger.csv`

현재 코드:

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeSourceCatalog.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Loader.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.SourceModel.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.NormalizedSkillAuthoring.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionPlan.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs`

관련 persistent state:

- `boards/MON/ARIEL_MONSTER.md`
- `boards/DATA/DATA_BLACKBOARD.md`

## 14. 최종 구조

모든 몬스터 전환 완료 후 목표 구조다.

```text
skills/
├─ base/
│  └─ 기본 skill chassis 수치
├─ choices/
│  ├─ {kind}/skill_choices_{kind}.csv
│  └─ {kind}/skill_graph_nodes_{kind}.csv
├─ nodes/
│  └─ definitions/
│     ├─ skill_node_definitions.csv
│     └─ skill_node_definition_params.csv
└─ triggers/
   └─ event binding과 graph reference
```

최종적으로 삭제되는 데이터 경로:

```text
skills/effects/
skills/nodes/{kind}/{kind}_skill_nodes.csv
skills/nodes/{kind}/{kind}_skill_node_params.csv
```

단, 삭제는 모든 monster graph 전환과 trigger graph reference 전환이 완료된 뒤에만 수행한다.
