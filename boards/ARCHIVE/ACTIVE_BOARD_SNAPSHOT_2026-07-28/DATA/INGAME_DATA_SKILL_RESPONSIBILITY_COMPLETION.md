# InGame Data·Skill 책임 분리 완성

## Task title

Data와 Skills의 실제 클래스 책임 및 런타임 변환 경계 완성

## Goals

- `CsvDataLoader`에는 초기화와 전체 실행 순서만 남긴다.
- 현재 역할별 CSV 파일을 같은 이름의 실제 클래스로 분리한다.
- 활성 스킬과 패시브를 생성 시점에 `SkillRuntimeData`로 변환한다.
- 전투 실행 중 `GameDataCatalog`에서 선택지와 패시브 원본을 다시 조회하지 않는다.
- `SkillChoiceRuntimeData`의 상속과 `new NormalizedPlanNodes` 필드 숨김을 제거한다.
- 카탈로그 및 몬스터 대체 데이터 경로를 제거한다.

## Constraints

- Role Owner는 Code Builder다.
- CSV 값, 스킬 수치, Scene, Prefab, 저장 데이터 의미를 변경하지 않는다.
- 기존 `.meta`와 Unity 자산 참조를 보존한다.
- 모든 필수 데이터는 존재한다고 가정한다.
- 새 fallback 함수, 대체 카탈로그, 대체 몬스터, 첫 항목 반환을 만들지 않는다.
- 이름은 현재 파일과 역할을 그대로 드러내는 짧은 표현을 사용한다.
- `BLACKBOARD.md`와 기존 도메인 보드에는 기록하지 않는다.
- Play Mode 전투 검증은 사용자가 수행한다.

## Role Owner

Code Builder

## Status

Implementation complete. Unity Play Mode 전투 확인은 사용자 검증 항목으로 남긴다.

## Target flow

```text
CsvRuntimeCatalog
    ↓
CsvDataLoader
    ├─ CsvRowParser
    │   └─ CsvParser
    ├─ CsvDataValidator
    ├─ GameDataBuilder
    │   └─ SkillGraphBuilder
    └─ GameDataCatalog
            ↓
SkillRuntimeFactory
    ↓
SkillRuntimeCompiler
    ├─ SkillRuntimeData
    ├─ PassiveSkillRuntimeData
    └─ SkillChoiceRuntimeData
            ↓
UnitSkillRuntimeSet
            ↓
Executor / Trigger / Passive runtime
```

## Class responsibilities

### CsvDataLoader

- `CurrentCatalog` 초기화
- 런타임 CSV 카탈로그 로드
- 파싱, 검증, 생성 순서 조율

### CsvParser

- CSV 텍스트를 `CsvTable`과 `CsvRecord`로 변환
- 문자열, 정수, 실수, bool, enum 읽기

### CsvRowParser

- `CsvRuntimeCatalog`의 TextAsset을 행 데이터로 변환
- `CsvSourceModel` 구성

### CsvSourceModel

- 파싱된 몬스터, 적, 스킬, 선택지, 노드, Trigger, 상태 행 보관

### GameDataBuilder

- 검증된 `CsvSourceModel`을 `GameDataCatalog`로 변환

### SkillGraphBuilder

- Skill, Choice, Effect, Trigger 그래프와 노드 구성

### CsvDataValidator

- 원본 모델과 생성된 카탈로그 검증

### CsvAssetReferenceCollector

- CSV가 사용하는 Sprite, Prefab, AnimatorController 경로 수집

### CsvCatalogSync

- Unity Editor에서 `CsvRuntimeCatalog.asset` 생성과 동기화

### SkillRuntimeFactory

- 학습한 활성 스킬과 패시브를 함께 생성
- 완성된 런타임 데이터를 `UnitSkillRuntimeSet`에 등록

### SkillRuntimeCompiler

- `SkillDefinition`과 `PassiveDefinition`을 실행 데이터로 변환
- 모든 선택지를 `SkillChoiceRuntimeData`로 변환

### UnitSkillRuntimeSet

- 활성 스킬과 패시브 런타임 보관
- ID와 슬롯 조회 제공
- Tick은 활성 스킬에만 적용

## Skill choice structure

```text
SkillChoiceRuntimeData
├─ Source: SkillChoiceDefinition
├─ PlanNodes: SkillExecutionPlanNode[]
├─ AddedModifiers
└─ 런타임 전용 상태 보정값
```

- 원본 정의를 상속하지 않는다.
- 같은 이름의 노드 필드를 두 개 선언하지 않는다.
- 전투 코드는 선택지 ID로 카탈로그를 다시 조회하지 않는다.

## Implementation order

1. `SkillChoiceRuntimeData`를 합성 구조로 전환한다.
2. 패시브와 선택지를 생성 시점에 컴파일한다.
3. 실행, Trigger, 패시브 코드의 원본 선택지 재조회를 제거한다.
4. 카탈로그 fallback 인자와 함수를 제거한다.
5. CSV partial 구현을 실제 클래스 책임으로 순차 전환한다.
6. 이전 호출과 타입 참조가 0인지 검사한다.
7. Runtime·Editor 빌드, Unity 컴파일, CSV 검증, 스킬 검증을 실행한다.

## Acceptance criteria

- `Data/Csv`에 `partial CsvDataLoader` 선언이 없고 `CsvDataLoader.cs`만 `CsvDataLoader`를 선언한다.
- 실행 계층에서 `SkillChoiceDefinition` 직접 조회가 없다.
- `SkillRuntimeCompiler.CompilePassive`가 실제 런타임 생성 경로에서 호출된다.
- `SkillChoiceRuntimeData`에 `new` 필드가 없다.
- `ResolveCatalogOrFallback`과 `fallbackMonster` 참조가 없다.
- 기존 CSV 원본, Scene, Prefab은 변경되지 않는다.
- Runtime·Editor 빌드 오류 0.
- Unity Console 오류 0.
- CSV 런타임 카탈로그가 몬스터 5, 1단계 적 8, 2단계 적 8을 로드한다.
- InGame 스킬 데이터 검증 경고 0.

## Next Actions

- 사용자가 Unity Play Mode에서 스킬 선택, 패시브 적용, Trigger 실행을 확인한다.

## Evidence

- 작업 전 `Data/Csv`의 역할 파일 9개가 모두 `public static partial class CsvDataLoader`를 선언했다.
- 작업 전 `SkillExecutionSystem`, `InGamePassiveEffectRuntime`, `SkillTriggerRuntime`이 `SkillChoiceDefinition`을 카탈로그에서 직접 조회했다.
- 작업 전 `SkillRuntimeFactory`는 활성 스킬만 생성했고 `CompilePassive`는 검증기에서만 호출됐다.
- 작업 전 Runtime·Editor 빌드는 오류 0, 기존 `MSB3277` 경고 2개였다.
- 작업 전 사용자 변경 `Pakuri/Assets/Scripts/InGame/Skills/Execution/UI.meta` 삭제가 존재했다.
- `CsvParser`, `CsvRowParser`, `CsvSourceModel`, `GameDataBuilder`, `SkillGraphBuilder`, `CsvDataValidator`, `CsvAssetReferenceCollector`, `CsvCatalogSync`가 각각 실제 클래스로 선언됐다.
- `SkillRuntimeFactory.RebuildLearnedSkillSet`이 활성 스킬과 패시브를 함께 컴파일한다.
- 실행 계층의 `SkillChoiceDefinition` 및 `PassiveDefinition` 카탈로그 재조회가 0건이다.
- `fallbackMonster`, `ResolveCatalogOrFallback`, `RebuildLearnedActiveSet` 참조가 0건이다.
- Runtime·Editor 빌드 결과 오류 0, 기존 `MSB3277` 경고 2개다.
- Unity Console 오류 조회 결과 0건이다.

## History

- 2026-07-20: Designer 검사에서 파일 구조 통합은 완료됐지만 실제 클래스 책임과 실행 변환 경계가 남았음을 확인했다.
- 2026-07-20: 사용자가 Code Builder 구현과 fallback 없는 필수 데이터 전제를 승인했다.
- 2026-07-20: CSV 역할 클래스 분리, 활성·패시브 런타임 통합, 선택지 합성 구조, 카탈로그 fallback 제거를 완료했다.
