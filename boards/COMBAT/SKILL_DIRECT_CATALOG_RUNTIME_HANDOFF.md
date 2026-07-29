# Skill Direct Catalog Runtime Handoff

## Task title

검증된 CSV에서 최종 전투 스킬 데이터를 한 번만 생성하고 카탈로그에서 직접 사용한다.

## Goals

- `GameDataCatalogBuilder`가 `SkillDefinition`, `PassiveSkillDefinition`, `SkillChoice`, `SkillTriggerDefinition`, `SkillNode`를 최종 형식으로 한 번만 만든다.
- 전투 상태 재구축에서 `SkillSourceDefinition -> SkillDefinition` 변환을 없앤다.
- Choice와 Trigger Node의 문자열 매개변수 변환을 카탈로그 생성 단계에서 한 번만 수행한다.
- Node와 Trigger의 enum, 목록, 조건식 인코딩 문자열을 카탈로그 생성 단계에서 강타입 값으로 한 번만 변환한다.
- `GameDataCatalog`가 최종 데이터와 ID 조회표를 소유한다.
- `Combat/Skills/Compilation` 폴더와 세 컴파일 스크립트를 제거한다.
- `Combat/Skills`를 Definition, Runtime, Execution, Delivery, Reaction 책임 순서로 정리한다.
- 검증 완료 데이터가 다시 검증되거나 다시 컴파일되지 않게 한다.

## Constraints

- 구현 롤은 Code Builder의 Structure + Refactoring + Implementation + Verification 트랙이다.
- 이 문서는 Designer 설계 핸드오프다. 이 작업에서 C#, CSV, prefab, scene, asset은 변경하지 않는다.
- 현재 피해, 치명타, 상태, 대상 선정, 투사체, 범위, 버프, 보호막, 회복, Trigger, Choice, Passive, cooldown, magazine, reload, visual, recast 동작을 보존한다.
- 현재 CSV 열, ID, 값, 정렬 순서, 에셋 경로를 바꾸지 않는다.
- `GameDataLoader.BuildValidatedRuntimeCatalog`의 의미 검증 호출은 정확히 한 번 유지한다.
- `GameDataCatalog.RebuildLookup`은 ID 인덱스만 만들며 검증·변환·Node 파싱을 하지 않는다.
- 카탈로그에 저장된 최종 Definition은 공유 데이터다. 유닛별 cooldown, magazine, reload, hit count 같은 변경 상태는 계속 `SkillUseState`가 소유한다.
- Unity `.meta` GUID를 보존해 script, prefab, scene 참조를 유지한다.
- 기존 작업 트리의 사용자 소유 변경을 덮어쓰지 않는다.
- Unity Play Mode 검증은 사용자 소유다.
- Code Reviewer 실행은 사용자 명시 승인 뒤 한 번만 가능하다.

## Role Owner

Designer for this handoff. Code Builder for implementation after explicit role assignment.

## Status

Code Builder 구현 진행 중. Phase 1-5 완료.

## Implementation log

### Phase 1 — Baseline and protection

- 기준 커밋: `565eed5`
- 시작 작업 트리: clean
- `Combat/Skills`: C# 27개, 13,997줄
- 현재 `Combat/Skills`의 모든 script/folder `.meta` GUID를 명령 출력으로 수집했다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal`: 오류 0, 기존 assembly-version 경고 2개
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 오류 0, 기존 assembly-version 경고 2개
- Unity `Pakuri/Validate CSV Source Data`: monsters 5, stage-one enemies 8, stage-two enemies 8 로드
- Unity EditMode test job `bd3daa5759fb49969e135e5aa46e9499`: succeeded
- Unity Editor는 비-Play Mode, idle, compile/domain reload pending 없음
- Console의 project validation 오류는 0이다. 별도 MCP package의 disposed-object 오류 1개는 이 작업 코드 밖에서 발생했다.
- `Pakuri.NewCore.Runtime.csproj`는 저장소에 없는 구형 source 82개를 참조해 baseline build가 실패했다. 현재 게임 assembly 검증은 실제 파일 목록을 반영하는 `Assembly-CSharp.csproj`와 `Assembly-CSharp-Editor.csproj`를 기준으로 한다.

### Phase 2 — Final data model

- `SkillDefinition`이 RuntimeKind, 구현 상태, 기본 학습 여부, Summary를 직접 보관한다.
- `PassiveSkillDefinition`이 요구 액티브 슬롯과 독립 사용 가능 여부를 직접 보관한다.
- `SkillChoice`가 표시/소유/대상 필드와 강타입 `SkillNode[]`를 직접 보관한다. 기존 Source는 중간 빌드 호환용으로만 남겼다.
- `SkillNode.TargetSkillId`를 추가해 다중 대상 Choice Node의 실제 대상 ID를 보존한다.
- `ApplyStatusNodeOp`의 target scope/merge policy를 enum으로 바꿨다.
- `StatusConditionNodeOp`의 조건식/원본 스킬 목록을 `StatusConditionGroup[]`/`string[]`로 바꿨다.
- `StatusMutationNodeOp`의 조건부 상태 목록과 incoming/outgoing RuntimeKind 목록을 강타입 배열로 바꿨다.
- `SkillTriggerDefinition`에 Choice, attribute, event skill 목록과 event source scope의 강타입 필드를 추가했다.
- 기존 compiler/mapper가 새 final 필드를 채우므로 Phase 3 전환 전에도 프로젝트가 빌드된다.
- `SkillNodeExecutor`의 authored `StatusRuntimeCompiler.Parse*` 호출 검색 결과는 0이다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal`: 오류 0, 기존 경고 2개
- Unity script refresh/domain reload 완료, project script 오류 0. 별도 MCP package disposed-object 오류는 이 작업 코드 밖이다.

### Phase 3 — Generation produces final catalog data

- `GameDataCatalogBuilder`가 Generation 안에서 active/passive/enemy 스킬을 최종 concrete Definition으로 한 번 생성한다.
- Monster/Enemy는 `SkillDefinition[]`과 `PassiveSkillDefinition[]`을 직접 보관한다.
- `GameDataCatalog`의 ID/monster lookup은 `SkillDefinition`, `PassiveSkillDefinition`, `SkillChoice`를 직접 색인한다.
- Trigger의 상태 조건, Choice 목록, attribute 목록, event skill 목록, RuntimeKind 목록, event source scope, Node를 Generation에서 강타입 값으로 채운다.
- status definition을 스킬보다 먼저 만들고 Generation에 직접 전달해 `CurrentCatalog` 재진입 없이 StatusRuntimeData를 생성한다.
- `all_allies` authored scope는 Generation의 명시 매핑으로 `AllAllies`가 된다.
- 전투 `RebuildLearnedSkillState`와 적 `RebuildAssignedSkillState`는 최종 Definition 참조를 그대로 `SkillUseState`에 넣는다.
- 기존 `SkillDefinitionCompiler` 호출은 Generation 내부 3곳만 남았고 Combat 호출은 0이다. Phase 5 삭제 전 일회성 조립 helper로만 사용한다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal`: 오류 0, 기존 경고 2개
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 오류 0, 기존 경고 3개
- Unity CSV validation: monsters 5, stage-one enemies 8, stage-two enemies 8 로드

### Phase 4 — Runtime consumes final data directly

- `SkillChoice.Source`, 대상별 runtime Node cache, `GetChoiceRuntimeNodes`를 제거했다.
- Choice 실행은 Generation이 만든 `SkillChoice.Nodes`와 `SkillNode.TargetSkillId`를 직접 비교·실행한다.
- `SkillTrigger`는 Choice ID, attribute, event skill ID 배열과 event source scope enum을 직접 비교하며 authored 문자열을 분해하지 않는다.
- `StatusRules`는 catalog의 최종 `SkillDefinition`과 concrete `SingleSkillDefinition` 값을 직접 사용한다.
- 호출이 없는 `SkillTargeting`의 문자열 Choice/Passive/attribute 판정 helper 5개를 삭제했다.
- Runtime Execution/Trigger/StatusRules의 `Split`, `Enum.Parse`, `TryParse` 검색 결과는 0이다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal`: 오류 0, 기존 경고 2개
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore -v:minimal`: 오류 0, 기존 경고 3개
- Unity script compilation 완료. project script 오류 0이며 별도 MCP package disposed-object 오류 1개만 존재했다.
- Unity CSV validation: monsters 5, stage-one enemies 8, stage-two enemies 8 로드

### Phase 5 — Compiler deletion and responsibility folders

- `SkillDefinitionCompiler`, `SkillChoiceCompiler`, `SkillNodeMapper` 클래스와 Combat compiler 파일을 제거했다.
- Definition/Choice/Node 생성은 `GameDataCatalogBuilder.Skills.cs`와 `GameDataCatalogBuilder.Nodes.cs`의 같은 partial Builder 책임으로 통합했다.
- 학습 ID/Choice 적용은 `UnitSkills.ApplyLearnedSkills`가 직접 담당한다.
- `StatusRuntimeCompiler.CompileTriggers`는 호출 0건이므로 삭제했다.
- `Combat/Skills`는 Definitions, Runtime, Execution, Delivery, Reactions의 24개 C# 구조로 이동했다.
- 기존 Compilation, Choices, SkillType 폴더는 파일 0건을 확인한 뒤 제거했다.
- 이동 스크립트 18개의 이전/현재 `.meta` GUID를 비교했으며 불일치 0건이다.
- 제거 symbol 검색 결과 `SkillDefinitionCompiler|SkillChoiceCompiler|SkillNodeMapper|CompileActive|CompilePassive|CompileTriggers` 0건이다.
- runtime/editor build 오류 0, Unity project script 오류 0, CSV validation 5/8/8을 확인했다.

## Core decision

`작성 데이터에서 직접 사용한다`의 최종 의미는 다음과 같다.

```text
CSV
  -> Parsing: SourceModel
  -> Validation: 의미 검증 1회
  -> Generation: 최종 강타입 전투 데이터 생성 1회
  -> RuntimeCatalog: 최종 데이터 ID 조회표 생성 1회
  -> Combat: 최종 Definition 직접 사용
```

전투에서 CSV 문자열이나 평면 Row를 직접 해석하지 않는다. `GameDataCatalogBuilder`가 검증된 `SourceModel`을 최종 전투 데이터로 만든 뒤, 전투는 그 값을 그대로 읽는다.

`SkillId`, `TargetSkillId`, `ChoiceId`, `TriggerId` 같은 실제 식별자는 문자열로 유지한다. 제거 대상은 `a;b`, `incoming|outgoing`, enum 이름, 상태 조건식처럼 전투 실행 중 다시 분해하거나 해석해야 하는 작성 인코딩 문자열이다.

시전마다 필요한 동적 계산은 정적 데이터 변환과 다르므로 유지한다.

```text
공유 SkillDefinition
  + 유닛이 선택한 Choice/Passive
  + 현재 status/target/projectile 상태
  -> SkillExecutionData
  -> family Executor
```

`SkillExecutionData`는 매 시전 값 스냅샷이다. 삭제하거나 카탈로그에 저장하지 않는다.

## Current inspected evidence

- `Pakuri/Assets/Scripts/Loading/GameDataLoader.cs`
  - `BuildValidatedRuntimeCatalog`는 `ValidateSourceModelOrThrow` 한 번, `BuildRuntimeCatalog` 한 번, `RebuildLookup` 한 번을 순서대로 호출한다.
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.cs`
  - 현재 `BuildActiveSkills`는 `SkillSourceDefinition`을 만든다.
  - 현재 `BuildPassiveSkills`는 `PassiveDefinition`을 만든다.
  - 현재 `BuildSkillChoices`는 `SkillChoiceDefinition`을 만든다.
  - 현재 `BuildSkillNodeDefinitions`는 문자열 `HandlerId`와 `Params`를 가진 `SkillNodeDefinition`을 만든다.
- `Pakuri/Assets/Scripts/Combat/Skills/Compilation/SkillDefinitionCompiler.cs`
  - `CompileActive`와 `CompilePassive`가 Source Definition을 concrete `SkillDefinition`으로 다시 변환한다.
  - RuntimeKind, ExecutionProfile, 대상 문자열, hit target count, status ID, family field, Choice, Trigger, Node를 변환한다.
- `Pakuri/Assets/Scripts/Combat/Skills/Compilation/SkillChoiceCompiler.cs`
  - `SkillChoiceDefinition`을 `Source` 필드 하나만 가진 `SkillChoice`로 감싼다.
- `Pakuri/Assets/Scripts/Combat/Skills/Compilation/SkillNodeMapper.cs`
  - `SkillNodeDefinition.HandlerId`와 문자열 `Params`를 강타입 `SkillNode` operation으로 변환한다.
  - Choice Node를 대상 스킬별로 첫 사용 시 변환하고 캐시한다.
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
  - `RebuildLearnedSkillState`가 `CompileActive`와 `CompilePassive`를 호출한다.
  - `RebuildAssignedSkillState`가 적 스킬마다 `CompileActive`를 호출한다.
  - 현재 파일은 `SkillExecutionContext`, `SkillExecution`, `SkillUseState`, `SkillExecutionState` 네 클래스를 가진다.
- `Pakuri/Assets/Scripts/Loading/RuntimeCatalog/GameDataCatalog.cs`
  - 현재 ID 조회표는 `SkillSourceDefinition`, `PassiveDefinition`, `SkillChoiceDefinition`을 저장한다.
- `Pakuri/Assets/Scripts/Units/Definitions/MonsterDefinition.cs`
  - 현재 `ActiveSkills`는 `SkillSourceDefinition[]`, `PassiveSkills`는 `PassiveDefinition[]`이다.
- `Pakuri/Assets/Scripts/Units/Definitions/EnemyDefinition.cs`
  - 현재 `ActiveSkills`는 `SkillSourceDefinition[]`이다.
- `Pakuri/Assets/Scripts/Combat/Status/StatusRuntimeCompiler.cs`
  - `CompileTriggers`가 Trigger status 조건과 `SkillNode[]`를 나중에 채운다.
- `Pakuri/Assets/Scripts/Combat/Status/StatusRules.cs`
  - 스킬 RuntimeKind 조건 판정이 카탈로그의 `SkillSourceDefinition`을 조회한다.
- `Pakuri/Assets/Scripts/GameFlow/RunSession.cs`와 UI 스크립트
  - `IsDefaultLearned`, `ImplementationState`, `RequiredActiveSlot`, Choice 표시 정보 등 Source Definition 메타데이터를 직접 읽는다.
- 현재 `Combat/Skills`는 C# 27개, 총 15,387줄이다.
- 현재 여섯 `skill_graph_nodes_*.csv`는 총 868행이며 Choice 행은 251개다.
- 둘 이상의 대상 스킬을 가진 Choice owner가 실제로 둘 존재한다.
  - `vega-c-trait-5`: `vega-a`, `vega-b`, `vega-d`, `vega-e`
  - `vega-c-master-1`: `vega-a`, `vega-c`
- 현재 실행 Definition의 필드 변경 대입 검색 결과는 Compilation 코드에 집중돼 있다. 전투 소비자는 Definition을 읽고, 유닛별 변경 상태는 `SkillUseState`에 보관한다.
- `SkillNodeExecutor`는 현재 final operation 안에 남은 문자열을 실행 중 다시 변환한다.
  - `ApplyStatusNodeOp.TargetScope -> StatusTargetScope`
  - `ApplyStatusNodeOp.MergePolicy -> StatusMergePolicy`
  - `StatusConditionNodeOp.Expression -> StatusConditionGroup[]`
  - `StatusConditionNodeOp.SourceSkillIds -> string[]`
  - conditional status 목록 -> `StatusEffectKind[]`
  - incoming/outgoing runtime-kind 조건 -> `SkillRuntimeKindCondition[]`
- `SkillTrigger`는 현재 `RequiresActiveChoiceId`, `ExcludesActiveChoiceId`, `TriggerAttribute`, `EventSkillId`를 발동 판정 때 `Split`하며 `EventSourceScope`도 문자열 비교한다.
- `DamageMeterUIController.ResolveChoiceTitleForSource`는 Trigger의 원본 `RequiresActiveChoiceId` 문자열을 직접 읽는다.

## Current `Combat/Skills` script tree

```text
Combat/Skills/
├─ Choices/
│  └─ SkillNode.cs
├─ Compilation/
│  ├─ SkillChoiceCompiler.cs
│  ├─ SkillDefinitionCompiler.cs
│  └─ SkillNodeMapper.cs
├─ Definitions/
│  └─ SkillDefinition.cs
├─ Execution/
│  ├─ SkillActionContext.cs
│  ├─ SkillExecution.cs
│  ├─ SkillExecutionData.cs
│  ├─ SkillExecutionRuleResolver.cs
│  ├─ SkillNodeExecutor.cs
│  └─ SkillTargeting.cs
├─ SkillType/
│  ├─ Buff/
│  │  ├─ BuffSkillActor.cs
│  │  └─ BuffSkillExecutors.cs
│  ├─ Line/
│  │  ├─ LineSkillActor.cs
│  │  └─ LineSkillExecutor.cs
│  ├─ Passive/
│  │  └─ PassiveSkill.cs
│  ├─ Projectile/
│  │  ├─ ProjectileSkillActor.cs
│  │  └─ ProjectileSkillExecutor.cs
│  ├─ Single/
│  │  ├─ SingleChargeActor.cs
│  │  ├─ SingleChargeState.cs
│  │  ├─ SingleSkillActor.cs
│  │  ├─ SingleSkillExecutor.cs
│  │  └─ SingleSkillRules.cs
│  ├─ Trigger/
│  │  └─ SkillTrigger.cs
│  └─ Zone/
│     ├─ ZoneSkillActor.cs
│     └─ ZoneSkillExecutor.cs
└─ UnitSkills.cs
```

## Current responsibility of every script

| Current script | Inspected current responsibility |
|---|---|
| `Choices/SkillNode.cs` | CSV handler에서 변환된 강타입 operation 구조체들과 operation 하나를 보관하는 `SkillNode` 컨테이너 |
| `Compilation/SkillChoiceCompiler.cs` | `SkillChoiceDefinition`을 `SkillChoice` wrapper로 변환 |
| `Compilation/SkillDefinitionCompiler.cs` | Source active/passive를 concrete 전투 Definition으로 변환하고 학습 ID를 `UnitSkills`에 적용 |
| `Compilation/SkillNodeMapper.cs` | 문자열 Node definition을 강타입 operation으로 변환하고 Choice 대상별 결과를 lazy cache |
| `Definitions/SkillDefinition.cs` | Source Definition, 최종 전투 Definition, Choice, Trigger, Node definition, 공용 spec과 enum 정의 |
| `Execution/SkillActionContext.cs` | Trigger Node 실행에 필요한 발생 당시 source, target, center, damage, hit count, execution data 보관 |
| `Execution/SkillExecution.cs` | 공통 실행 진입, family dispatch, 학습/적 스킬 상태 재구축, cooldown/magazine 상태, 실행 목록 관리 |
| `Execution/SkillExecutionData.cs` | 기본 스킬과 현재 Choice/Passive/status를 합친 한 시전용 스냅샷 |
| `Execution/SkillExecutionRuleResolver.cs` | 현재 target, status, projectile index에 의존하는 조건부 Node 규칙 계산 |
| `Execution/SkillNodeExecutor.cs` | Trigger가 선택한 강타입 Node operation을 순서대로 공용 전투 API에 실행 |
| `Execution/SkillTargeting.cs` | 진영, 생존, 상태, 거리, 범위 중심, 반복 배치 대상 계산 |
| `SkillType/Buff/BuffSkillActor.cs` | 대상 부착 Buff visual 수명 관리 |
| `SkillType/Buff/BuffSkillExecutors.cs` | 일반 Buff, Shield, Heal의 대상 선정과 적용 |
| `SkillType/Line/LineSkillActor.cs` | 생성된 직선 공격의 충돌, tick, 상태, 수명 관리 |
| `SkillType/Line/LineSkillExecutor.cs` | 직선 공격 방향, 길이, 폭, 지속시간 조립과 Actor 생성 |
| `SkillType/Passive/PassiveSkill.cs` | Passive/Trigger별 cooldown과 누적 횟수 상태 관리 |
| `SkillType/Projectile/ProjectileSkillActor.cs` | 투사체 이동, 충돌, 적중, 상태, 소멸 관리 |
| `SkillType/Projectile/ProjectileSkillExecutor.cs` | 발사 수, 분기, 연속 발사, 후속 투사체와 Actor 생성 |
| `SkillType/Single/SingleChargeActor.cs` | 돌진 이동, 목표 추적, 접촉 피해와 상태 처리 |
| `SkillType/Single/SingleChargeState.cs` | 진행 중 돌진의 대상, 시간, 이동, 피해, 상태 값 보관 |
| `SkillType/Single/SingleSkillActor.cs` | 단일 공격 visual의 수명과 대상 추적 |
| `SkillType/Single/SingleSkillExecutor.cs` | 단일, 연쇄, 돌진형 스킬 실행 순서와 피해 적용 |
| `SkillType/Single/SingleSkillRules.cs` | 단일 공격 시전 조건, 피해 보정, 처치 후 처리 계산 |
| `SkillType/Trigger/SkillTrigger.cs` | 전투/lifecycle 사건 조건, 확률, cooldown, delay, repeat 판정 후 Node 실행 |
| `SkillType/Zone/ZoneSkillActor.cs` | 지속 범위의 대상 판정, tick 피해, 상태, 만료 관리 |
| `SkillType/Zone/ZoneSkillExecutor.cs` | 범위 중심, 반지름, 배치 수, 지속시간 조립과 Actor 생성 |
| `UnitSkills.cs` | 유닛이 학습한 active/passive와 선택한 enhancement/master ID 저장 |

## Problem in current structure

현재는 검증 뒤에도 두 번째 정적 변환 단계가 남는다.

```text
SourceModel
  -> GameDataCatalogBuilder
  -> SkillSourceDefinition / PassiveDefinition / SkillChoiceDefinition / SkillNodeDefinition
  -> SkillDefinitionCompiler / SkillChoiceCompiler / SkillNodeMapper
  -> SkillDefinition / SkillChoice / SkillNode
```

문제:

- 같은 정적 스킬을 `RebuildLearnedSkillState` 호출 때마다 다시 concrete Definition으로 만든다.
- Choice Node는 첫 사용 시 문자열 Params를 다시 읽고 변환한다.
- `SkillSourceDefinition`과 `SkillDefinition`이 같은 스킬 값을 두 형태로 중복 보관한다.
- `SkillChoiceCompiler`는 Source wrapper만 만들므로 독립 컴파일 책임이 없다.
- `SkillDefinitionCompiler.ApplyLearnedSkills`는 컴파일이 아니라 학습 ID 저장 책임이다.
- 검증기가 `SkillNodeMapper.CanProcessNode`에 의존해 Loading 검증과 Combat compilation 책임이 섞인다.
- 일부 final Node operation이 작성 문자열을 계속 보관해 `SkillNodeExecutor`가 실행 때마다 scope, merge policy, 조건식, 상태 목록, runtime-kind 조건을 다시 파싱한다.
- Trigger의 Choice 목록, 속성 목록, event skill 목록, event source scope도 final 강타입 계약이 아니어서 발동 판정 때 다시 분해·비교된다.

## Final ownership

### Parsing

`SourceModel`은 CSV를 검증하기 위한 임시 중간 모델이다.

- CSV 열과 row 값을 보관한다.
- 전투에서 접근하지 않는다.
- 카탈로그 생성이 끝난 뒤 보관하지 않는다.

### Validation

`CsvDataValidator`만 의미 무결성을 검사한다.

- ID, owner, target skill, Node handler, parameter, status, asset, ordering, recursion을 한 번 검사한다.
- 최종 Definition 생성 뒤 같은 검사를 반복하지 않는다.
- 지원 Node handler 목록은 생성 코드와 한 곳에서 공유한다. 같은 switch/list를 Validator와 Builder에 복제하지 않는다.

### Generation

`GameDataCatalogBuilder`가 최종 전투 데이터를 만든다.

- concrete `SkillDefinition` subtype 선택
- 공통 필드와 family 필드 조립
- 표시/학습 메타데이터 조립
- status ID와 enum 변환
- Choice 최종 데이터 조립
- Trigger 조건과 강타입 Node 조립
- Node handler/Params를 강타입 operation으로 변환
- Node의 scope, merge policy, 조건식, 상태 목록, runtime-kind 목록을 final enum/배열로 변환
- Trigger의 Choice 목록, 속성 목록, event skill 목록, event source scope를 final 배열/enum으로 변환
- sprite, prefab, animator, runtime visual 연결

이 변환은 `BuildRuntimeCatalog` 한 번 안에서만 실행한다.

### Runtime catalog

`GameDataCatalog`는 최종 데이터와 ID 인덱스만 소유한다.

- `SkillDefinition`
- `PassiveSkillDefinition`
- `SkillChoice`
- `SkillTriggerDefinition`
- `StatusEffectDefinition`
- monster/enemy/reward 데이터

`RebuildLookup`은 위 객체 참조를 dictionary에 등록만 한다.

### Runtime execution

- `SkillExecution`은 카탈로그의 최종 `SkillDefinition`을 직접 `SkillUseState`에 넣는다.
- `SkillUseState`는 공유 Definition 참조와 유닛별 변경 상태를 가진다.
- `SkillExecutionData`는 매 시전 동적 스냅샷을 만든다.
- family Executor는 최종 Definition과 실행 스냅샷을 읽는다.
- Trigger는 최종 `SkillNode[]`를 직접 실행한다.

## Final data contract

### `SkillDefinition`

기존 실행 필드에 Source/UI/Run 메타데이터를 합친다.

필수 추가/유지:

- `SkillId`
- `SkillName`
- `Slot`
- `RuntimeKind`
- `ImplementationState`
- `IsDefaultLearned`
- `Description`
- `Summary`
- `Icon`
- concrete family specs
- `SkillChoice[] EnhancementChoices`
- `SkillChoice[] MasterChoices`
- `SkillTriggerDefinition[] SkillTriggers`
- `SkillNode[] Nodes`

`PassiveSkillDefinition` 추가/유지:

- `RequiredActiveSlot`
- `IsAvailableWithoutActiveRequirement`
- `SkillChoice[] BaseModifierChoices`

삭제할 Source 타입:

- `SkillSourceDefinition`
- `PassiveDefinition`

### `SkillChoice`

`SkillChoiceDefinition + SkillChoice wrapper + lazy cache`를 하나로 합친다.

최종 필드:

- `ChoiceId`
- `MonsterId`
- `SkillId`
- `TargetSkillId`
- `ChoiceGroup`
- `Title`
- `Icon`
- `SkillEffectPrefab`
- `Description`
- `SkillNode[] Nodes`

삭제:

- `SkillChoiceDefinition`
- `SkillChoice.Source`
- `runtimeNodesByTarget`
- `TryGetRuntimeNodes`
- `CacheRuntimeNodes`
- `SkillChoiceCompiler`

### `SkillNode`

최종 `SkillNode`는 다음만 가진다.

- `TargetSkillId`
- 강타입 operation 하나
- operation 조회
- 대상 스킬 일치 판정

Builder가 `TargetSkillId`와 operation을 함께 생성한다. 전투에서는 문자열 Handler/Params를 다시 파싱하지 않는다.

operation의 최종 계약:

- `ApplyStatusNodeOp`
  - `StatusEffectKind StatusKind`
  - `StatusTargetScope TargetScope`
  - `StatusMergePolicy MergePolicy`
  - 작성 값이 비어 있으면 각 enum의 `Unspecified`를 사용해 기존 "override 없음" 의미를 보존한다.
- `StatusConditionNodeOp`
  - `StatusConditionGroup[] Conditions`
  - `string[] SourceSkillIds`
- `StatusMutationNodeOp`
  - 단일 status/skill/trigger ID는 문자열 식별자로 유지한다.
  - conditional status 목록은 `StatusEffectKind[]`로 저장한다.
  - incoming/outgoing runtime-kind 조건은 각각 `SkillRuntimeKindCondition[]`로 저장한다.

`SkillNodeExecutor`는 위 값을 비교·적용만 하며 `StatusRuntimeCompiler.ParseTargetScope`, `ParseMergePolicy`, `ParseConditionStatusExpression`, `ParseIdList`, `ParseStatusKinds`, `ParseSkillRuntimeKindConditions`를 호출하지 않는다.

다중 대상 Choice 때문에 `TargetSkillId`를 제거하면 안 된다. 런타임은 이미 만들어진 `SkillNode[]`를 순회하며 대상 일치만 확인한다. 이 순회는 검증이나 컴파일이 아니다.

삭제:

- `SkillNodeDefinition`
- `SkillNodeParamDefinition`
- `NormalizedNodes` 명칭
- `SkillNodeMapper`
- final operation의 작성 인코딩 문자열 필드

최종 명칭:

- `SkillDefinition.Nodes`
- `SkillChoice.Nodes`
- `SkillTriggerDefinition.Nodes`

### `SkillTriggerDefinition`

Builder가 다음을 모두 완성한다.

- parsed status kinds
- parsed condition status arrays
- parsed source skill ID arrays
- parsed runtime kind conditions
- parsed required/excluded Choice ID arrays
- parsed Trigger attribute arrays
- parsed event skill ID arrays
- parsed event source scope enum
- final `SkillNode[] Nodes`

최종 Trigger 필드:

- `string[] RequiredActiveChoiceIds`
- `string[] ExcludedActiveChoiceIds`
- `DamageAttribute[] TriggerAttributes`
- `string[] EventSkillIds`
- `SkillRuntimeKindCondition[] EventSkillRuntimeKindValues`
- `StatusConditionGroup[] ConditionStatuses`
- `string[] ConditionStatusSourceSkillIds`
- `SkillTriggerEventSourceScope EventSourceScope`

`SkillTriggerEventSourceScope`는 기존 빈 값, `owner`, `all_allies` 의미를 `Any`, `Owner`, `AllAllies`로 보존한다. `SourceSkillId`, `TriggerId` 같은 단일 식별자는 문자열로 유지한다.

삭제:

- `NormalizedNodes`
- `StatusRuntimeCompiler.CompileTriggers`
- 실행 전 Trigger mutation
- `RequiresActiveChoiceId`, `ExcludesActiveChoiceId`, `TriggerAttribute`, `EventSkillId`, `EventSkillRuntimeKinds`, `EventSourceScope`의 runtime 원문 필드
- `SkillTrigger`의 작성 목록 `Split`과 scope 문자열 비교 helper

### `GameDataCatalog`

최종 조회 API:

```text
GetData<SkillDefinition>(skillId)
GetData<PassiveSkillDefinition>(passiveId)
GetData<SkillChoice>(choiceId)
GetActiveSkills(monsterId) -> SkillDefinition[]
GetPassiveSkills(monsterId) -> PassiveSkillDefinition[]
GetActiveSkill(monsterId, slot) -> SkillDefinition
ResolvePassiveSkill(monsterId, slot) -> PassiveSkillDefinition
```

현재 `SourceModel.Skills`가 active와 passive를 하나의 ID dictionary에 `AddUnique`하므로 player skill ID 인덱스는 하나로 합칠 수 있다. Enemy 전용 스킬은 현재 별도 Source dictionary이므로 충돌 가능성을 추측해 player dictionary와 강제로 합치지 않는다.

## Final `Combat/Skills` script tree

새 helper script를 늘리지 않는다. 현재 세 Compilation script를 삭제하고 기존 파일을 책임 폴더로 이동한다. `SkillDefinition.cs`와 `SkillExecution.cs`를 단순 파일 분할하지 않는다.

```text
Combat/Skills/
├─ Definitions/
│  ├─ SkillDefinition.cs
│  └─ SkillNode.cs
├─ Runtime/
│  ├─ UnitSkills.cs
│  └─ SkillExecutionData.cs
├─ Execution/
│  ├─ SkillActionContext.cs
│  ├─ SkillExecution.cs
│  ├─ SkillExecutionRuleResolver.cs
│  ├─ SkillNodeExecutor.cs
│  └─ SkillTargeting.cs
├─ Delivery/
│  ├─ Buff/
│  │  ├─ BuffSkillActor.cs
│  │  └─ BuffSkillExecutors.cs
│  ├─ Line/
│  │  ├─ LineSkillActor.cs
│  │  └─ LineSkillExecutor.cs
│  ├─ Projectile/
│  │  ├─ ProjectileSkillActor.cs
│  │  └─ ProjectileSkillExecutor.cs
│  ├─ Single/
│  │  ├─ SingleChargeActor.cs
│  │  ├─ SingleChargeState.cs
│  │  ├─ SingleSkillActor.cs
│  │  ├─ SingleSkillExecutor.cs
│  │  └─ SingleSkillRules.cs
│  └─ Zone/
│     ├─ ZoneSkillActor.cs
│     └─ ZoneSkillExecutor.cs
└─ Reactions/
   ├─ PassiveSkill.cs
   └─ SkillTrigger.cs
```

목표 C# script 수: 24개 이하.

폴더 책임:

```text
Definitions = 공유 최종 정적 데이터
Runtime     = 유닛 학습 ID와 한 시전 동적 스냅샷
Execution   = 공통 실행 순서, 대상 계산, Node dispatch
Delivery    = 스킬 형태별 생성·충돌·피해 전달
Reactions   = 사건에 반응하는 Trigger/Passive 상태
```

## Current-to-final script mapping

| Current script | Final action |
|---|---|
| `Choices/SkillNode.cs` | `.meta`와 함께 `Definitions/SkillNode.cs`로 이동; `TargetSkillId` 추가 |
| `Compilation/SkillChoiceCompiler.cs` | 삭제; Builder가 `SkillChoice` 직접 생성 |
| `Compilation/SkillDefinitionCompiler.cs` | 삭제; Definition 변환은 Builder로, `ApplyLearnedSkills`는 `UnitSkills`로 이동 |
| `Compilation/SkillNodeMapper.cs` | 삭제; 문자열-to-operation 생성은 Builder 내부로 이동 |
| `Definitions/SkillDefinition.cs` | 유지; Source/Definition 중복 타입 삭제, 최종 메타데이터 통합 |
| `Execution/SkillActionContext.cs` | 유지 |
| `Execution/SkillExecution.cs` | 유지; rebuild에서 final Definition 직접 사용 |
| `Execution/SkillExecutionData.cs` | `.meta`와 함께 `Runtime/SkillExecutionData.cs`로 이동 |
| `Execution/SkillExecutionRuleResolver.cs` | 유지; `SkillChoice.Source`와 mapper 의존 제거 |
| `Execution/SkillNodeExecutor.cs` | 유지 |
| `Execution/SkillTargeting.cs` | 유지 |
| `SkillType/Buff/BuffSkillActor.cs` | `.meta`와 함께 `Delivery/Buff/`로 이동 |
| `SkillType/Buff/BuffSkillExecutors.cs` | `.meta`와 함께 `Delivery/Buff/`로 이동 |
| `SkillType/Line/LineSkillActor.cs` | `.meta`와 함께 `Delivery/Line/`으로 이동 |
| `SkillType/Line/LineSkillExecutor.cs` | `.meta`와 함께 `Delivery/Line/`으로 이동 |
| `SkillType/Passive/PassiveSkill.cs` | `.meta`와 함께 `Reactions/PassiveSkill.cs`로 이동 |
| `SkillType/Projectile/ProjectileSkillActor.cs` | `.meta`와 함께 `Delivery/Projectile/`로 이동 |
| `SkillType/Projectile/ProjectileSkillExecutor.cs` | `.meta`와 함께 `Delivery/Projectile/`로 이동 |
| `SkillType/Single/SingleChargeActor.cs` | `.meta`와 함께 `Delivery/Single/`로 이동 |
| `SkillType/Single/SingleChargeState.cs` | `.meta`와 함께 `Delivery/Single/`로 이동 |
| `SkillType/Single/SingleSkillActor.cs` | `.meta`와 함께 `Delivery/Single/`로 이동 |
| `SkillType/Single/SingleSkillExecutor.cs` | `.meta`와 함께 `Delivery/Single/`로 이동 |
| `SkillType/Single/SingleSkillRules.cs` | `.meta`와 함께 `Delivery/Single/`로 이동 |
| `SkillType/Trigger/SkillTrigger.cs` | `.meta`와 함께 `Reactions/SkillTrigger.cs`로 이동 |
| `SkillType/Zone/ZoneSkillActor.cs` | `.meta`와 함께 `Delivery/Zone/`으로 이동 |
| `SkillType/Zone/ZoneSkillExecutor.cs` | `.meta`와 함께 `Delivery/Zone/`으로 이동 |
| `UnitSkills.cs` | `.meta`와 함께 `Runtime/UnitSkills.cs`로 이동; 학습 ID 적용 책임 흡수 |

## Required external script changes

### Loading

- `Loading/Generation/GameDataCatalogBuilder.cs`
  - `SkillDefinitionCompiler`과 `SkillNodeMapper`의 실제 생성 로직을 흡수한다.
  - final active/passive/choice/trigger/node를 직접 만든다.
  - final Node operation과 Trigger가 보관할 enum/배열을 작성 문자열에서 한 번만 만든다.
  - Validator가 묻는 Node 지원 여부와 실제 Node 생성이 같은 handler 분기 근거를 사용하게 한다.
- `Loading/Validation/CsvDataValidator.cs`
  - `SkillNodeMapper.CanProcessNode` 의존을 제거한다.
  - 검증 횟수는 늘리지 않는다.
- `Loading/RuntimeCatalog/GameDataCatalog.cs`
  - Source Definition dictionary를 final Definition dictionary로 교체한다.
  - lookup rebuild는 참조 등록만 수행한다.

### Unit definitions

- `Units/Definitions/MonsterDefinition.cs`
  - `ActiveSkills -> SkillDefinition[]`
  - `PassiveSkills -> PassiveSkillDefinition[]`
- `Units/Definitions/EnemyDefinition.cs`
  - `ActiveSkills -> SkillDefinition[]`

### Combat status

- `Combat/Status/StatusRuntimeCompiler.cs`
  - `CompileTriggers` 삭제
  - `Create(..., SkillSourceDefinition)` 삭제 또는 final Definition 기반 생성 단계로 흡수
  - 런타임 status Node가 쓰는 `Create(StatusEffectKind, string)`은 유지
- `Combat/Status/StatusRules.cs`
  - runtime-kind 조회를 final `SkillDefinition`으로 변경

### Node and Trigger execution

- `Combat/Skills/Execution/SkillNodeExecutor.cs`
  - final operation의 enum/배열을 직접 사용한다.
  - 작성 scope, merge policy, 조건식, ID 목록, status 목록, runtime-kind 목록 파싱을 제거한다.
- 현재 `Combat/Skills/SkillType/Trigger/SkillTrigger.cs`, 이동 후 `Combat/Skills/Reactions/SkillTrigger.cs`
  - required/excluded Choice, Trigger attribute, event skill ID를 final 배열로 비교한다.
  - event source scope를 final enum으로 비교한다.
  - 작성 목록 `Split`과 scope 문자열 비교 helper를 제거한다.

### Run and UI consumers

다음 파일은 Source 타입 대신 final 타입을 받도록 변경한다.

- `GameFlow/RunSession.cs`
- `UI/InGame/InGameUIManager.cs`
- `UI/InGame/DebugUI.cs`
- `UI/InGame/DamageMeter/DamageMeterUIController.cs`

사용 정보:

- active: ID, slot, default learned, implementation state, name, icon, summary
- passive: ID, required active slot, availability, name, icon, summary
- choice: ID, group, source/target skill ID, title, icon, description
- Trigger source title: `RequiredActiveChoiceIds`에서 현재 표시 규칙에 맞는 Choice ID 사용

### Spawn

- `GameFlow/Spawn/UnitCombatStateFactory.cs`
- `GameFlow/Spawn/UnitSpawnManager.cs`

`SkillDefinitionCompiler.ApplyLearnedSkills` 호출을 `UnitSkills`의 학습 결과 반영 API로 교체한다.

## Rebuild behavior after change

Player:

```text
catalog.GetActiveSkill(monsterId, slot)
  -> final SkillDefinition
  -> learned ID 확인
  -> new SkillUseState(owner, definition)
```

Passive:

```text
catalog.GetPassiveSkills(monsterId)
  -> final PassiveSkillDefinition[]
  -> learned ID 확인
  -> new SkillUseState(owner, definition)
```

Enemy:

```text
enemy.ActiveSkills
  -> final SkillDefinition[]
  -> new SkillUseState(owner, definition)
```

금지:

```text
CompileActive
CompilePassive
CompileTriggers
MapSkillNodeDefinitions
GetChoiceRuntimeNodes
```

## Migration sequence

### Phase 1: Baseline and protection

- 현재 dirty working tree를 기록한다.
- 사용자 소유 C# comment 변경과 기존 Loading 이동 변경을 보호한다.
- 현재 runtime/editor build, Unity compile, CSV validation 기준값을 기록한다.
- 현재 `Combat/Skills` 27개와 15,387줄 기준값을 기록한다.

Rollback point: 코드 변경 전.

### Phase 2: Final data model

- `SkillDefinition`에 Source/UI/Run 메타데이터를 합친다.
- `PassiveSkillDefinition`에 passive 학습 조건을 합친다.
- `SkillChoice`에 Source 필드를 직접 합치고 final Nodes를 둔다.
- `SkillNode`에 `TargetSkillId`를 추가한다.
- Node operation의 scope, merge policy, 조건식, status/runtime-kind 목록을 강타입 필드로 바꾼다.
- `SkillTriggerDefinition`은 final Nodes와 parsed Choice/attribute/event 조건만 소유하게 한다.

이 단계에서는 기존 compiler를 임시로 유지해 빌드 가능한 중간 상태를 만든다.

Rollback point: 타입 통합 전 commit/diff.

### Phase 3: Builder direct generation

- Builder가 concrete final Definition을 직접 생성한다.
- 기존 compiler의 family mapping, status mapping, target mapping, hit-count mapping, Node mapping을 Builder로 옮긴다.
- Node와 Trigger의 작성 인코딩 문자열을 enum/배열로 한 번만 변환한다.
- Monster/Enemy/Choice/Trigger에 final 객체를 직접 연결한다.
- 같은 handler 지원 분기를 Validator와 Builder가 중복 정의하지 않게 한다.

Rollback point: 기존 compiler 호출 유지 가능.

### Phase 4: Runtime direct use

- catalog lookup 반환 타입을 final 타입으로 바꾼다.
- player/enemy rebuild에서 compiler 호출을 제거한다.
- Choice runtime cache와 Node lazy mapping을 제거한다.
- Trigger runtime compilation을 제거한다.
- `SkillNodeExecutor`와 `SkillTrigger`의 작성 문자열 재파싱을 제거한다.
- UI, Run, Status, Spawn consumer 타입을 final 타입으로 변경한다.

Rollback point: compiler 파일은 아직 남아 있으나 호출은 0개.

### Phase 5: Delete compilation and move folders

- Compilation 세 C#과 `.meta`를 제거한다.
- 빈 `Compilation`과 `Choices` folder `.meta`를 제거한다.
- 유지 스크립트를 final tree로 `.meta`와 함께 이동한다.
- namespace는 폴더 이동만을 이유로 바꾸지 않는다.

Rollback point: file move 전 GUID 목록 기록.

### Phase 6: Dead contract deletion

- Source/Definition 중복 타입과 필드 삭제
- `NormalizedNodes` 삭제
- mapper/cache/compiler 관련 method와 reference 삭제
- final Node/Trigger의 raw enum/list/condition 문자열 필드 삭제
- `SkillNodeExecutor`와 `SkillTrigger`의 authored-string parse/split helper 삭제
- stale comment와 using 삭제
- generated project file refresh 뒤 compile

## Edge cases

- 다중 대상 Choice Node는 `TargetSkillId`를 잃으면 다른 스킬에 잘못 적용된다.
- 같은 final Definition을 여러 유닛이 공유하므로 runtime 코드가 Definition 배열이나 필드를 수정하면 안 된다.
- Trigger의 internal cooldown/count는 Definition이 아니라 `PassiveSkill` runtime state에 남아야 한다.
- delayed Trigger가 쓰는 `SkillActionContext` snapshot 책임은 유지한다.
- enemy assigned skill은 monster skill과 다른 Source dictionary에서 온다. ID 공간 통합은 현재 검증 근거 없이 수행하지 않는다.
- `StatusFilteredDeployment`와 `TargetStatusStackDamage`는 현재 compiler가 base `SingleSkillDefinition` 필드로 옮긴다. Builder 이동 때 누락하면 안 된다.
- `ExecutionProfile`이 concrete type 선택과 enemy 특수 profile mapping에 사용된다. final 생성 뒤 문자열 profile은 전투에 남기지 않는다.
- `"all"`/`"global"` hit target count 해석은 Builder에서 한 번 수행하고 final bool/int로 저장한다.
- Choice/Trigger Node 순서는 현재 authored sort order를 유지한다.
- lookup rebuild가 Definition을 복제하면 공유 참조 원칙이 깨진다. dictionary에는 같은 객체 참조를 등록한다.
- 빈 Node scope/merge policy는 `Unspecified`, 빈 Trigger 목록은 빈 배열, 빈 Trigger event source scope는 `Any`로 변환해 현재 wildcard/override 없음 의미를 유지한다.
- 목록 ID 비교의 기존 대소문자 무시와 authored 순서를 유지한다.
- `SkillId`, `TargetSkillId`, `ChoiceId`, `TriggerId`, 단일 status ID 같은 실제 식별자는 enum으로 만들지 않는다.
- `DamageMeterUIController`의 Trigger source title 표시가 raw Choice 문자열 삭제 뒤에도 동일해야 한다.

## Acceptance criteria

- `GameDataLoader.BuildValidatedRuntimeCatalog`에 의미 검증 호출이 정확히 한 번 존재한다.
- `BuildRuntimeCatalog`이 final skill data를 정확히 한 번 만든다.
- `GameDataCatalog.RebuildLookup`은 변환·검증·Node mapping을 호출하지 않는다.
- repository active C# search 결과가 다음에서 0이다.
  - `SkillSourceDefinition`
  - `PassiveDefinition` 단, `EnemyPassiveDefinition`은 제외
  - `SkillChoiceDefinition`
  - `SkillNodeDefinition`
  - `SkillNodeParamDefinition`
  - `SkillDefinitionCompiler`
  - `SkillChoiceCompiler`
  - `SkillNodeMapper`
  - `CompileActive`
  - `CompilePassive`
  - `CompileTriggers`
  - `GetChoiceRuntimeNodes`
  - `runtimeNodesByTarget`
  - `NormalizedNodes`
- `Combat/Skills/Compilation`과 `Combat/Skills/Choices` 폴더가 없다.
- `Combat/Skills` C# script 수가 24개 이하이며 새 wrapper/helper script로 수가 다시 늘지 않는다.
- `RebuildLearnedSkillState`와 `RebuildAssignedSkillState`는 final Definition을 직접 `SkillUseState`에 넣는다.
- `SkillExecutionData`는 매 시전 새로 생성되며 catalog에 저장되지 않는다.
- final Definition은 runtime에서 변경되지 않는다.
- `SkillNodeExecutor`에 authored Node 설정을 위한 `StatusRuntimeCompiler.Parse*` 호출이 0개다.
- `SkillTrigger`에 authored Choice/attribute/event skill 목록 `Split`과 event source scope 문자열 비교가 0개다.
- final Node/Trigger에는 실행 중 분해할 enum/list/condition 인코딩 문자열 필드가 없다.
- 두 multi-target Choice owner의 Node가 각 target skill에만 적용된다.
- active/passive/choice UI 정보가 이전과 동일하다.
- player와 enemy family dispatch가 이전 concrete subtype과 동일하다.
- 모든 기존 `.meta` GUID가 이동 뒤 유지된다.
- runtime/editor C# build가 error 0으로 완료된다.
- Unity script compilation Console error가 0이다.
- CSV source validation이 error 0이며 현재 catalog 수량을 유지한다.
- Code Builder가 구현 전후 C# file count와 line count를 기록한다.
- 사용자 Play Mode에서 representative active, passive, enhancement, master, Trigger, enemy skill을 확인한다.

## Verification expected from Code Builder

- `rg`로 제거 symbol과 runtime compile call을 전수 검색한다.
- `rg`로 Definition field assignment를 검색해 Generation 외 정적 Definition mutation이 없는지 확인한다.
- `rg`로 `SkillNodeExecutor`의 `StatusRuntimeCompiler.Parse`와 `SkillTrigger`의 `Split`/raw scope 비교가 0인지 확인한다.
- 여섯 graph CSV의 multi-target Choice owner를 다시 집계한다.
- final Choice Node target filtering pure C# 또는 EditMode test를 추가한다.
- final catalog가 같은 ID에 같은 object reference를 반환하는지 test한다.
- rebuild를 두 번 호출해 Definition 참조는 재사용되고 `SkillUseState`만 새로 생기는지 test한다.
- `dotnet build Pakuri/Pakuri.sln --no-restore`를 실행한다.
- Unity script compilation과 Console을 확인한다.
- 프로젝트 CSV source validation을 실행한다.
- Unity Play Mode 검증 항목은 사용자에게 넘긴다.

## Related board files

- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`

## Next Actions

- 사용자가 Code Builder 롤을 명시한다.
- Code Builder가 이 문서를 읽고 현재 코드와 dirty working tree를 다시 확인한다.
- Phase 1부터 순서대로 구현한다.

## Evidence

근거는 위 `Current inspected evidence`, 전체 script tree, current responsibility table, 실제 symbol 검색 결과다.

### Evidence commands

```powershell
Get-ChildItem Pakuri/Assets/Scripts/Combat/Skills -Recurse -Filter *.cs
rg -n "CompileActive|CompilePassive|CompileTriggers|GetChoiceRuntimeNodes" Pakuri/Assets/Scripts --glob "*.cs"
rg -n "SkillSourceDefinition|PassiveDefinition|SkillChoiceDefinition|SkillNodeDefinition" Pakuri/Assets/Scripts --glob "*.cs"
rg -n "ValidateSourceModelOrThrow|BuildRuntimeCatalog|RebuildLookup" Pakuri/Assets/Scripts/Loading/GameDataLoader.cs
rg -n "class SkillExecutionContext|class SkillExecution$|class SkillUseState|class SkillExecutionState" Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs
```

## History

- 2026-07-29: User selected direct use of final authored data instead of rebuilding a second combat-data model at runtime.
- 2026-07-29: Designer inspected all 27 current `Combat/Skills` C# scripts, Loading builder/catalog flow, runtime compiler calls, Run/UI/Status consumers, and multi-target Choice data.
- 2026-07-29: Designer created this Code Builder handoff without changing C#, CSV, prefab, scene, or asset data.
- 2026-07-29: Designer expanded the final typed-data contract after code inspection proved that `SkillNodeExecutor` and `SkillTrigger` still parse authored scope, policy, condition, status, runtime-kind, Choice, attribute, event-skill, and event-source strings during execution.
