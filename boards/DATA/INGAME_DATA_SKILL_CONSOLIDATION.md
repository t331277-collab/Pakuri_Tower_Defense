# InGame Data·Skill 통폐합 설계

## 목표

- `InGame/Data`는 CSV 로딩, 원본 정의, 전체 데이터 조회만 담당한다.
- `InGame/Skills`는 스킬 실행 데이터, 상태 계산, Executor만 담당한다.
- 중복 카탈로그, 중복 스킬 선택지 모델, 중복 enum, 상태 기본값의 이중 권한을 제거한다.
- `InGame/Skills/Data` 폴더를 없애고 런타임 책임에 맞는 폴더로 이동한다.

## 현재 근거

- `CsvSourceCatalog`와 `CsvAssetCatalog`는 같은 Editor 동기화 과정에서 생성되고 같은 `Resources/Pakuri/CSVRuntime` 경로에서 로드된다.
- `GameDataCatalog`는 배열을 보관하고 `GameDataRegistry`는 같은 데이터를 Dictionary로 다시 등록한다.
- `InGameSkillCatalog`는 데이터를 보관하지 않고 `GameDataRegistry` 조회와 `InGameSkillDefinitionMapper` 호출을 중계한다.
- `SkillChoiceDefinition` 147개 필드와 `SkillChoiceEffectSpec` 155개 필드 중 140개 필드 이름이 같다.
- `SkillSlot`과 `InGameSkillSlot`, `DamageAttribute`와 `ElementType`은 같은 값을 중복 정의한다.
- `CharacterType`은 Mapper가 값을 넣고 `SkillData`가 보관하지만 실행 코드에서 읽는 사용처가 없다.
- `StatusEffectDefinitionData`와 `StatusEffectKind.cs`의 `StatusEffectDefinition`이 지속시간, 최대 중첩, 영구 여부를 각각 보관한다.
- `SkillData` 파생 타입은 런타임에서 임시 `ScriptableObject`로 생성되며 Scene, Prefab, Asset의 직렬화 참조가 발견되지 않았다.

## 목표 데이터 흐름

```text
CSV + Unity Asset
    ↓
CsvRuntimeCatalog
    ↓
CsvDataLoader
    ↓
GameDataCatalog
    ↓
SkillRuntimeCompiler
    ↓
SkillRuntimeData
    ↓
SkillRuntimeInstance / Executor
```

## 통폐합 규칙

### 카탈로그

- `CsvSourceCatalog`와 `CsvAssetCatalog`를 `CsvRuntimeCatalog`로 합친다.
- `GameDataCatalog`와 `GameDataRegistry`를 조회 가능한 하나의 `GameDataCatalog`로 합친다.
- `InGameSkillCatalog`는 제거한다.
- 스킬 정의 조회는 `GameDataCatalog`, 실행 데이터 변환은 `SkillRuntimeCompiler`, 런타임 인스턴스 생성은 `SkillRuntimeFactory`가 담당한다.

### 스킬 정의와 실행 데이터

- `SkillDefinition`은 CSV에서 구성된 정적 원본 정의로 유지한다.
- `SkillData` 계층은 `SkillRuntimeData` 계층으로 이름을 바꾸고 일반 런타임 클래스로 전환한다.
- 작은 파생 타입 파일은 `SkillRuntimeData.cs`에 모은다.
- `SkillChoiceEffectSpec`의 140개 중복 필드는 제거한다.
- 원본의 `SkillNodeDefinition[]`과 실행 단계의 `SkillExecutionPlanNode[]`는 형식이 다르므로, 실행 노드와 런타임 전용 값만 가진 얇은 `SkillChoiceRuntimeData`를 유지한다.
- 그래프 노드의 실행 형태 변환은 `SkillRuntimeCompiler`가 담당한다.

### 공용 enum

- `InGameSkillSlot`를 제거하고 `SkillSlot`을 사용한다.
- `ElementType`을 제거하고 `DamageAttribute`를 사용한다.
- 사용되지 않는 `CharacterType`을 제거한다.

### 상태 효과

- `StatusEffectDefinitionData`를 `StatusEffectDefinition`으로 이름을 바꾸고 CSV 카탈로그를 기본값의 단일 권한으로 사용한다.
- 코드에 하드코딩된 상태 지속시간, 중첩, 영구 여부 정의를 제거한다.
- 스킬 출처별 덮어쓰기는 `RuntimeStatusData`가 `StatusEffectDefinition`을 참조해 보관한다.
- 상태 생성은 `StatusEffectFactory`, 전투 판정과 계산은 `StatusEffectRules`가 담당한다.

### CSV

- `CsvDataLoader`는 초기화와 전체 실행 순서만 담당한다.
- CSV 문자열 해석은 `CsvParser`, 행 변환은 `CsvRowParser`, 원본 중간 모델은 `CsvSourceModel`이 담당한다.
- 게임 정의 생성은 `GameDataBuilder`, 그래프 생성은 `SkillGraphBuilder`가 담당한다.
- 검증은 `CsvDataValidator`, 자산 경로 수집은 `CsvAssetReferenceCollector`가 담당한다.
- Editor 동기화 메서드는 `CsvDataLoader`의 `partial` 구현이므로 같은 런타임 어셈블리에 남겨야 한다. 파일은 `Data/Csv/CsvCatalogSync.cs`에 두고 `#if UNITY_EDITOR` 경계로 제한한다.
- `Data/Editor/CsvCatalogPostprocessor.cs`는 Unity Editor 콜백 클래스이므로 Editor 폴더에 둔다.

## 목표 구조

```text
InGame/
├─ Data/
│  ├─ Catalog/
│  │  ├─ CsvRuntimeCatalog.cs
│  │  ├─ GameDataCatalog.cs
│  │  └─ GameDataLookup.cs
│  ├─ Definitions/
│  │  ├─ Units/
│  │  │  ├─ MonsterDefinition.cs
│  │  │  └─ EnemyDefinition.cs
│  │  ├─ Skills/
│  │  │  ├─ SkillDefinition.cs
│  │  │  ├─ SkillChoiceDefinition.cs
│  │  │  ├─ SkillGraphDefinition.cs
│  │  │  └─ SkillVisualDefinition.cs
│  │  └─ Status/
│  │     └─ StatusEffectDefinition.cs
│  ├─ Csv/
│  │  ├─ CsvDataLoader.cs
│  │  ├─ CsvParser.cs
│  │  ├─ CsvSourceModel.cs
│  │  ├─ CsvRowParser.cs
│  │  ├─ GameDataBuilder.cs
│  │  ├─ SkillGraphBuilder.cs
│  │  ├─ CsvDataValidator.cs
│  │  ├─ CsvAssetReferenceCollector.cs
│  │  └─ CsvCatalogSync.cs
│  └─ Editor/
│     └─ CsvCatalogPostprocessor.cs
└─ Skills/
   ├─ Runtime/
   │  ├─ Data/
   │  │  ├─ SkillRuntimeData.cs
   │  │  └─ SkillChoiceRuntimeData.cs
   │  ├─ SkillRuntimeCompiler.cs
   │  ├─ SkillRuntimeFactory.cs
   │  ├─ SkillRuntimeInstance.cs
   │  └─ UnitSkillRuntimeSet.cs
   ├─ Status/
   │  ├─ StatusEffectKind.cs
   │  ├─ RuntimeStatusData.cs
   │  ├─ StatusEffectFactory.cs
   │  └─ StatusEffectRules.cs
   ├─ Validation/
   │  └─ SkillRuntimeDataValidator.cs
   └─ Execution/
```

## 구현 순서

1. 기존 호출을 유지한 채 새 이름과 새 책임의 타입을 도입한다.
2. 스킬 런타임 데이터와 컴파일러 호출을 새 구조로 이동한다.
3. 선택지와 enum 중복을 제거한다.
4. 상태 정의와 런타임 상태 데이터를 단일 권한 구조로 바꾼다.
5. CSV 카탈로그와 게임 데이터 조회 계층을 통합한다.
6. 모든 소비자를 새 API로 전환한 뒤 이전 타입과 파일을 삭제한다.
7. Runtime·Editor 빌드와 Unity 컴파일 상태를 확인한다.

## 호환성 주의

- Unity 파일 이동 시 기존 `.meta`를 함께 이동해 GUID를 보존한다.
- `CsvSourceCatalog.asset`과 `CsvAssetCatalog.asset`은 생성 자산이므로 통합 후 `CsvRuntimeCatalog.asset`로 다시 생성한다.
- `SkillData` 파생 자료형을 사용하는 `TypedSkillExecutor<T>` 계약은 모든 Executor 전환이 끝날 때까지 유지한다.
- 사용자 Play Mode 검증 전에는 전투 수치나 CSV 의미를 바꾸지 않는다.

## 구현 결과

- `CsvSourceCatalog`와 `CsvAssetCatalog`를 `CsvRuntimeCatalog` 하나로 합쳤다.
- `GameDataRegistry`의 공개 싱글턴을 제거하고 `GameDataCatalog`를 조회 진입점으로 통합했다. Dictionary 인덱스 구현은 내부 `GameDataLookup`만 담당한다.
- `Assets/Legacy/Data/GameData/GameDataCatalog.asset`이 스크립트 GUID를 실제 참조하므로 `GameDataCatalog`의 `ScriptableObject` 상속은 유지했다. 런타임 생성은 `ScriptableObject.CreateInstance<GameDataCatalog>()`를 사용한다.
- `InGameSkillCatalog`를 제거하고 `GameDataCatalog → SkillRuntimeCompiler → SkillRuntimeFactory` 흐름으로 바꿨다.
- `Skills/Data` 폴더를 제거했다. 런타임 데이터, 상태, 검증 코드를 각각 `Runtime/Data`, `Status`, `Validation`으로 이동했다.
- `SkillData` 계층을 일반 C# 객체인 `SkillRuntimeData` 계층으로 바꿨다. 작은 파생 형식은 한 파일에 합쳤다.
- `SkillChoiceEffectSpec`의 중복 선언을 제거하고 원본 정의를 상속하는 얇은 `SkillChoiceRuntimeData`로 교체했다.
- `InGameSkillSlot`, `ElementType`, `CharacterType`을 제거하고 각각 `SkillSlot`, `DamageAttribute`, 미사용 제거로 정리했다.
- 상태 기본값은 `StatusEffectDefinition`과 현재 `GameDataCatalog`만 조회한다. 런타임 상태 생성과 판정은 `StatusEffectFactory`, `StatusEffectRules`로 나눴다.
- `SkillDefinition.cs`의 선택지, 그래프, 시각 정의를 별도 파일로 분리했다.
- `Data/Runtime/Csv`를 `Data/Csv`, `Data/Definition`을 `Data/Definitions`, `Data/Catalogs`를 `Data/Catalog`로 정리했다.

## 검증 근거

- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore`: 오류 0.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore`: 오류 0.
- 이전 타입명 12종을 `Pakuri/Assets/Scripts`의 C# 파일에서 검색한 결과: 참조 0.
- Unity 메뉴 `Pakuri/Sync CSV Runtime Catalog Assets` 실행 후 `Assets/Resources/Pakuri/CSVRuntime/CsvRuntimeCatalog.asset` 생성 확인.
- Unity 메뉴 `Pakuri/Validate CSV Source Data` 실행 결과: 몬스터 5, 1단계 적 8, 2단계 적 8 로드 확인.
- Unity 메뉴 `Pakuri/InGame/Validate Skill Data` 실행 결과: 경고 0, 검증 통과.
- Unity 스크립트 재컴파일 후 Console 오류 0.
- Play Mode 전투 검증은 사용자 실행 범위로 남긴다.
