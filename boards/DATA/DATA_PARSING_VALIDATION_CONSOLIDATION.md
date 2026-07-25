# Data 파싱·검증 책임 통폐합

## Task title

`Pakuri/Assets/Scripts/Data`를 CSV 파싱·검증 전용 경로로 축소

## Goals

- `Data` 아래에는 CSV 파싱, 파싱 결과 보관, CSV 경계 검증만 남긴다.
- 카탈로그, 게임 정의, 런타임 데이터, 런타임 컴파일, 게임 시작 로딩, Editor 동기화를 실제 사용 경로로 옮긴다.
- CSV 검증은 파싱 직후 한 번만 수행한다.
- 이름 추론, 조용한 기본값, 과거 Migration 검증, 생성 결과 재검증을 제거한다.
- 파일 이름과 대표 클래스 이름을 일치시킨다.

## Constraints

- Role Owner는 Code Builder다.
- CSV 값, 전투 수치, Scene, Prefab, 저장 데이터 의미는 변경하지 않는다.
- Unity가 참조하는 스크립트는 기존 `.meta`를 함께 이동해 GUID를 보존한다.
- `MonsterDefinition`과 `EnemyDefinition`은 서로 다른 스크립트 GUID를 유지해야 하므로 합치지 않는다.
- 새 fallback 함수는 만들지 않는다.
- 수정하는 코드에는 삼항 연산자와 null 축약 표현을 사용하지 않고 명시적인 `if` 문을 사용한다.
- `sealed`, `internal`은 Unity 직렬화나 실제 접근 제한에 필요한 경우만 유지한다.
- 기존 사용자 변경인 `InGameCombatManager`, `Combat/State`, `MonsterDayRecovery`는 수정하지 않는다.
- `BLACKBOARD.md` 및 도메인 BLACKBOARD 파일에는 기록하지 않는다.
- Unity Play Mode 전투 검증은 사용자가 수행한다.

## Role Owner

Code Builder

## Status

Completed. Code Builder 구현과 Code Reviewer 1회 검토를 통과했다.

## Current evidence

- `Pakuri/Assets/Scripts/Data`에는 C# 스크립트 20개가 있다.
- `CsvRowParser.cs`는 1,758줄, `CsvDataValidator.cs`는 1,536줄, `GameDataBuilder.cs`는 1,817줄이다.
- `CsvDataLoader`는 `RuntimeInitializeOnLoadMethod`를 사용하므로 CSV 파서가 아니라 게임 시작 로더다.
- `SkillDataCompiler.cs`의 대표 클래스 이름은 `SkillRuntimeCompiler`이며 파일명과 역할이 다르다.
- `CsvDataLoader.LoadAllOrThrow`는 원본 모델 검증 후 `ValidateRuntimeCatalogOrThrow`와 `ValidateCompiledSkillDataOrThrow`를 다시 호출한다.
- `CsvRowParser`에는 `ReadMonsterIdOrInfer`, `ReadSkillKindOrInfer`, `ReadRuntimeKindOrInfer`, `InferRequiredActiveSlot`과 fallback 인자 기반 읽기 함수가 있다.
- `CsvRowParser`와 `CsvDataValidator`에는 현재 데이터임에도 `EnemyMigrationRow`, `ValidateEnemyMigrationRows` 이름이 남아 있다.
- `GameDataLookup`은 `GameDataCatalog` 외부에서 직접 사용되지 않는다.
- `CsvRuntimeCatalog`, `GameDataCatalog`, `MonsterDefinition`, `EnemyDefinition`의 `.meta` GUID는 실제 Unity 에셋에서 사용되므로 보존해야 한다.

## Target structure

```text
Pakuri/Assets/Scripts/
├─ Data/
│  ├─ CsvParser.cs
│  ├─ CsvRowParser.cs
│  ├─ CsvSourceModel.cs
│  ├─ CsvDataValidator.cs
│  └─ SkillGraphParser.cs
├─ GameFlow/Loading/
│  ├─ GameDataLoader.cs
│  ├─ GameDataCatalog.cs
│  ├─ GameDataCatalogBuilder.cs
│  ├─ CsvRuntimeCatalog.cs
│  └─ CsvCatalogEditor.cs
├─ Combat/Skills/
│  ├─ Definitions/SkillDefinition.cs
│  └─ Runtime/
│     ├─ SkillRuntimeData.cs
│     └─ SkillRuntimeCompiler.cs
├─ Combat/Status/
│  ├─ StatusEffectDefinition.cs
│  └─ StatusRuntimeCompiler.cs
├─ Units/Definitions/
│  ├─ MonsterDefinition.cs
│  └─ EnemyDefinition.cs
```

`SkillGraphBuilder`는 실행 코드가 아니라 그래프 CSV 파싱·검증·정규화 코드 838줄로 확인됐다. 이를 `SkillGraphParser`로 이름과 책임을 바로잡아 Data에 남긴다. `CsvRowParser`나 `CsvDataValidator`에 합치면 한 파일이 2,000줄을 넘으므로 독립 보조 파서로 유지한다. `GameDataLookup`은 `GameDataCatalog` 안으로 합친다. Editor의 CSV 동기화와 후처리는 `CsvCatalogEditor`로 통합한다. 이 파일은 Data의 internal 파서를 같은 어셈블리에서 사용해야 하므로 `GameFlow/Loading`에 두고 파일 전체를 `UNITY_EDITOR` 조건으로 제한한다. 런타임에서도 필요한 자산 참조 수집기는 `CsvDataValidator.cs`에 합친다.

## Validation boundary

```text
CSV TextAsset
    -> CsvParser
    -> CsvRowParser
    -> CsvSourceModel
    -> CsvDataValidator.ValidateSourceModelOrThrow (한 번)
    -> GameDataCatalogBuilder
    -> SkillRuntimeCompiler
    -> Combat runtime
```

검증기는 필수 파일과 열, 값 형식, ID 중복, 참조 ID, 지원 enum/노드/핸들러, 실행 불가능한 숫자 범위, 입력된 Unity 자산 경로의 존재만 검사한다. 생성된 카탈로그를 원본과 다시 비교하거나 모든 스킬을 재컴파일하는 검증은 삭제한다.

## Fallback removal rules

- 필수 `monster_id`를 파일 이름에서 추론하지 않는다.
- 활성/패시브 CSV 종류는 로더가 선택한 명시적인 파서 경로로 구분한다.
- 지원하지 않는 enum, 선택 그룹, 비주얼 앵커는 기본값으로 바꾸지 않고 CSV 검증 오류로 처리한다.
- 입력된 자산 경로를 찾지 못하면 `null`을 반환하지 않고 로딩 오류로 처리한다.
- 필수 노드 매개변수는 strict getter로 읽는다.
- 선택 열의 기본값은 CSV 계약상 빈 값이 허용될 때 파싱 경계에서 한 번만 명시한다.
- `Migration`, `LegacyOverlap` 전환 검증은 현재 형식 검증으로 이름을 바꾸거나 불필요하면 삭제한다.

## Implementation order

1. 이동 대상의 외부 참조와 `.meta` GUID를 기록한다.
2. 파일과 `.meta`를 함께 이동하고 파일명과 대표 클래스명을 맞춘다.
3. `GameDataLookup`과 Editor CSV 관리 코드를 각각 통합한다.
4. 그래프 행/검증/생성 책임을 분리하고 `SkillGraphBuilder`를 제거한다.
5. CSV 원본 경계 검증 한 번만 남긴다.
6. 추론, fallback, 과거 Migration 검증을 제거한다.
7. 수정 범위의 어려운 축약 표현을 명시적인 `if` 문으로 바꾼다.
8. Runtime·Editor 빌드와 Unity 컴파일/Console을 확인한다.
9. Code Reviewer가 이 문서와 실제 diff를 한 번 대조한다.

## Acceptance criteria

- `Pakuri/Assets/Scripts/Data`의 C# 파일 5개는 파싱·원본 모델·검증 역할만 가진다.
- `SkillDataCompiler.cs`, `GameDataBuilder.cs`, `CsvDataLoader.cs`처럼 파일명과 대표 클래스가 어긋난 파일이 없다.
- `ValidateRuntimeCatalogOrThrow`와 `ValidateCompiledSkillDataOrThrow` 참조가 0건이다.
- 현재 CSV 경로에서 `Infer`, `Migration`, `LegacyOverlap` 기반 처리가 0건이다.
- 필수 값을 조용히 기본값으로 바꾸는 새 fallback 함수가 없다.
- Unity 직렬화 대상 스크립트의 기존 GUID가 유지된다.
- 기존 CSV, Scene, Prefab과 사용자 변경 파일은 수정되지 않는다.
- Runtime·Editor 빌드 오류가 0이다.
- Code Reviewer 결과와 남은 사용자 Play Mode 검증 항목이 이 문서에 기록된다.

## Next Actions

- Unity Play Mode 전투 동작은 사용자가 확인한다.

## Implementation evidence

- Data C# 파일 수: 20개에서 5개로 축소.
- `SkillGraphCompiler`, `SkillGraphBuilder`, `CsvDataLoader`, `GameDataBuilder`, `SkillDataCompiler`, `StatusDataCompiler` 참조: 0건.
- 수정 범위의 `fallback`, `Infer`, `Migration`, `LegacyOverlap`, 삼항/null 축약 `?`, `sealed`: 0건.
- `ValidateRuntimeCatalogOrThrow`, `ValidateCompiledSkillDataOrThrow` 참조: 0건.
- Unity 스크립트 강제 갱신과 컴파일 후 Console 오류: 0건.
- `Pakuri/Validate CSV Source Data` 실행 결과: 5 monsters, stage-one enemies 8, stage-two enemies 8 로드.
- `Assembly-CSharp.csproj` 빌드: 오류 0건.
- `Assembly-CSharp-Editor.csproj` 빌드: 오류 0건.
- `git diff --check`: 오류 0건.
- CSV, Scene, Prefab 변경: 0건.
- 이동한 Unity 스크립트의 기존 `.meta` GUID를 보존했다.

## Code Reviewer result

- 판정: PASS.
- 검토 트랙: Structure Design Support, Implementation, Refactoring.
- 실제 변경 hunk와 현재 파일을 대조해 클래스·헬퍼 존재, null 경로, 의존 방향, CSV 경계 검증, fallback 제거, 직렬화 호환성을 확인했다.
- 이동한 스크립트 `.meta` GUID 17개가 원본과 모두 일치했다.
- 통합 후 삭제한 `GameDataLookup`, `CsvAssetReferenceCollector`, `CsvCatalogPostprocessor`의 기존 GUID는 Unity asset, prefab, scene, meta 참조가 각각 0건이었다.
- 통합 클래스는 각각 한 번만 선언되며 옛 클래스 이름과 옛 파일 경로는 남지 않았다.
- 기존 사용자 작업인 Combat/State, MonsterDayRecovery 및 현재 작업 밖의 Combat·UI 변경은 리뷰 대상과 구현 변경에서 제외했다.
- 확인된 코드 결함과 추가 수정 요청은 없다.
- 남은 검증 공백: Unity Play Mode 실제 전투 흐름은 사용자가 확인해야 한다.

## History

- 2026-07-22: Designer가 실제 Data 스크립트 20개, 선언, 외부 참조, 줄 수, `.meta` GUID를 조사했다.
- 2026-07-22: 사용자가 설계 문서 작성 후 Code Builder 구현과 Code Reviewer 검토를 승인했다.
- 2026-07-22: Code Builder가 Data를 5개 파싱·검증 스크립트로 축소하고 나머지 정의·로딩·컴파일 파일을 실제 사용 경로로 이동했다.
- 2026-07-22: Unity 컴파일, CSV 경계 검증, Runtime·Editor 빌드, 정적 검색을 완료했다.
- 2026-07-22: Code Reviewer가 문서와 실제 변경을 한 번 대조해 PASS 판정을 내렸다.

---

## Task title

NewCore `sealed` 및 조건부 삼항 연산자 정리

## Goals

- `Pakuri/Assets/Scripts/NewCore` 아래 C# 클래스의 `sealed` 한정자를 제거한다.
- 같은 범위의 조건부 삼항 연산자 `?:`를 명시적인 `if`/`else` 흐름으로 바꾼다.

## Constraints

- `abstract` 클래스는 유지한다.
- `throw new`, `?? throw` 등 예외 계약은 제거하거나 완화하지 않는다.
- 공개 멤버, Unity 직렬화 필드, 실행 결과를 바꾸지 않는다.
- Scene, Prefab, CSV, `.meta`는 수정하지 않는다.
- Code Reviewer와 Unity Play Mode는 실행하지 않는다.

## Role Owner

Code Builder

## Status

구현 및 컴파일 검증 완료. EditMode 117개 중 Presentation 10개 실패가 남아 있다.

## Next Actions

- 사용자가 원하면 `NewCorePresentationTests`의 asset/catalog null 실패 10개를 별도 진단한다.
- Unity Play Mode 동작은 사용자가 확인한다.

## Evidence

- 작업 전 `git grep -P -o '\bsealed\s+class\b' HEAD -- Pakuri/Assets/Scripts/NewCore`: 107건.
- 현재 `rg -n -g "*.cs" "\bsealed\s+class\b" Pakuri/Assets/Scripts/NewCore`: 0건.
- 작업 전 조건부 삼항 검색 일치 줄: 231줄.
- 현재 `rg -n -g "*.cs" "\s\?\s|\?\s*$" Pakuri/Assets/Scripts/NewCore`: 0건.
- 현재 `rg -n -g "*.cs" "\?[^?\r\n]*:" Pakuri/Assets/Scripts/NewCore`: 0건.
- `abstract class`: 11건 유지.
- `throw new`: 310건 유지.
- 변경 범위: C# 87개(런타임 82개, Editor 테스트 5개).
- `dotnet build Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`: 오류 0건, 기존 어셈블리 참조 충돌 경고 2건.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`: 오류 0건, 경고 3건.
- `dotnet build Pakuri.NewCore.EditMode.Tests.csproj --no-restore /p:UseSharedCompilation=false`: 오류 0건, 경고 0건.
- Unity EditMode `Pakuri.NewCore.EditMode.Tests`: 117개 실행, 107개 통과, 10개 실패.
- 실패 10개는 모두 `NewCorePresentationTests`이며 8개는 `NullReferenceException`, 2개는 asset null assertion이다.
- 실패한 10개만 재실행해 같은 10개 실패를 재현했다.
- `git diff --check -- Pakuri/Assets/Scripts/NewCore`: 오류 0건.

## History

- 2026-07-25: 사용자가 Code Builder에게 `sealed`와 조건부 삼항 정리를 지시했다.
- 2026-07-25: NewCore C# 전체에서 `sealed`를 제거하고 조건부 삼항을 `if`/`else`로 치환했다.
- 2026-07-25: 정적 검색, Runtime·Editor·EditMode Tests 어셈블리 빌드, Unity EditMode 테스트를 수행했다.

---

## Task title

NewCore 검사 `throw` 제거

## Goals

- `Pakuri/Assets/Scripts/NewCore`의 입력·상태·CSV 검사 실패를 명시적으로 던지는 `throw` 문을 제거한다.
- `?? throw` 생성자 대입은 직접 대입으로 바꾼다.
- 검사 제거 과정에서 함께 사라질 수 있는 `TryAdd`, `TryGetValue`, `TryConsume`, `TryUse`, `TryRegisterFieldUnit` 등의 정상 경로 부수 효과는 유지한다.
- 기존 예외 발생을 기대하는 EditMode 검사 계약 테스트를 제거한다.

## Constraints

- Role Owner는 Code Builder다.
- catch에서 상태를 되돌린 뒤 원래 예외를 다시 전달하는 `throw;`는 검사 `throw`가 아니므로 유지한다.
- Scene, Prefab, CSV, `.meta`는 수정하지 않는다.
- Code Reviewer와 Unity Play Mode는 실행하지 않는다.

## Role Owner

Code Builder

## Status

구현과 Runtime·Editor·EditMode Tests 어셈블리 빌드 검증 완료. Unity EditMode Presentation 실패 10개는 이전 작업과 동일하게 남아 있다.

## Next Actions

- 사용자가 Unity Play Mode에서 정상 CSV와 정상 scene 연결을 사용하는 실제 전투·보상·현현·Offering 흐름을 확인한다.
- 사용자가 원하면 기존 Presentation asset/catalog null 실패 10개를 별도 진단한다.

## Evidence

- 작업 전 `throw new`: 310건.
- 작업 전 전체 `throw`: 352건.
- 현재 `throw new`: 0건.
- 현재 `?? throw`: 0건.
- 현재 `throw Invalid(...)`, `throw Missing(...)`, `throw InvalidValue(...)`, `throw InvalidNodeArgument(...)`: 0건.
- 현재 전체 `throw`: 2건. `ManifestationService`와 `SkillTriggerDispatcher` catch의 상태 복구 후 재던지기 `throw;`만 유지했다.
- 런타임 스크립트 41개와 EditMode 테스트 4개를 변경했다.
- 예외 발생 자체를 기대하던 검사 계약 테스트 13개를 제거했다.
- 읽기 전용 카탈로그 변경 시 .NET 불변 컬렉션이 거부하는 `Assert.Throws<NotSupportedException>` 1건은 NewCore 검사 `throw`가 아니므로 유지했다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false`: 오류 0건, 경고 8건.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false`: 오류 0건, 기존 어셈블리 버전 충돌 경고 2건.
- `dotnet build Pakuri/Pakuri.NewCore.EditMode.Tests.csproj --no-restore /p:UseSharedCompilation=false`: 오류 0건, 경고 0건.
- Unity script 강제 컴파일 후 `Assets/Scripts/NewCore` 필터 Console 오류: 0건.
- Unity EditMode `Pakuri.NewCore.EditMode.Tests`: 104개 실행, 94개 통과, Presentation 10개 실패.
- 실패 10개는 이전 작업에서 확인된 동일한 `NewCorePresentationTests` asset/catalog null 실패다.
- `git diff --check -- Pakuri/Assets/Scripts/NewCore`: 오류 0건. 줄바꿈 변환 안내만 출력됐다.

## History

- 2026-07-25: 사용자가 Code Builder에게 NewCore 전체의 검사 `throw` 제거를 지시했다.
- 2026-07-25: `throw new`, `?? throw`, 검사 helper throw를 제거하고 정상 경로의 등록·소비·쿨다운·조회 부수 효과를 복구했다.
- 2026-07-25: 예외 계약 테스트를 정리하고 Runtime·Editor·EditMode Tests 빌드와 Unity 컴파일·EditMode 테스트를 수행했다.
