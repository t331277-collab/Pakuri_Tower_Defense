# DATA_BLACKBOARD

## Task: 2026-08-07 Artifact Synergy Icon Catalog Reuse

- Task title: 기존 파싱된 `artifact_synergies.csv.Icon_Image` 런타임 데이터 재사용
- Goals: Reward UI가 CSV의 `Icon_Image`를 새 파싱 없이 `ArtifactSynergyDefinition.Icon`으로 소비하도록 연결한다.
- Constraints: `CsvRowParser`, `GameDataCatalogBuilder`, `ArtifactSynergyDefinition`의 기존 계약과 Sprite 로딩을 유지한다. CSV와 catalog schema는 변경하지 않는다.
- Role Owner: Code Builder
- Status: 구현 완료. 기존 catalog 파싱·Sprite 참조와 UI 소비 회귀 검증 통과.
- Next Actions: 사용자 Play Mode에서 실제 Reward 카드 표시를 확인한다.
- Evidence: `CsvRowParser.cs:273-299`가 `Icon_Image`를 `IconPath`로 읽고, `GameDataCatalogBuilder.Artifacts.cs:266`이 `LoadSprite(row.IconPath)`를 `ArtifactSynergyDefinition.Icon`에 넣는다. runtime catalog는 50 artifacts·6 synergies를 로드했고, 유물이 존재하는 5개 시너지의 Icon이 non-null임을 focused test `8093ef50...`가 확인했다. `tracker`는 CSV `Icon_Image`가 비어 있어 null로 유지된다.
- History: 2026-08-07 Code Builder는 데이터 파이프라인 변경 대신 이미 생성된 시너지 정의 Sprite를 `ArtifactUI`가 사용하도록 범위를 제한했다. 전체 EditMode 36/36과 솔루션 빌드 오류 0개를 확인했다.

## Task: 2026-08-07 Infinite Shell Magazine Runtime Diagnosis

- Task title: 무한 탄피 최대 탄창 미적용 진단
- Goals: `infinite-shell-effect`의 데이터 작성, 아군 배포, 탄창 런타임 반영 순서를 실제 코드로 확인한다.
- Constraints: CSV 수치·대상 범위는 변경하지 않는다. 기존 `InitializeRuntimeValues`를 재사용하고 전체 스킬 rebuild는 하지 않는다. 기존 사용자 변경 파일은 건드리지 않는다.
- Role Owner: Code Builder
- Status: Phase 1 구현·검증 완료. Stage 유물 배포 뒤 고정 런타임 값이 갱신된다.
- Next Actions: 사용자 Play Mode에서 무한 탄피 탄창 표시와 실제 발사/재장전 주기를 확인한다.
- Evidence: `UnitSkills.RefreshLearnedRuntimeValues`가 현재 active/passive 목록의 고정값을 기존 초기화 경로로 재계산한다. `InGameCombatManager.BeginPlayerCombat`이 passive와 CombatStart 전에 이를 호출한다. 새 EditMode 검사 `PreparedArtifactFixedValuesRefreshWithoutCompounding`은 무한 탄피, 신속 장전기, 난사 도면, 종말의 잔불, 포격대 2시너지 재장전 효과와 복합 유물 피해 배율 및 재호출 비누적을 검증해 1/1 통과했다. 전체 EditMode는 36/36 통과, `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal /p:UseSharedCompilation=false`는 오류 0·기존 참조 경고 2로 통과했다.
- History: 2026-08-07 데이터→배포→런타임 캐시 순서 결함과 같은 경로의 4개 유물 피해 범위를 확인했다. Code Builder가 전체 rebuild 없이 Stage 시작의 현재 런타임 고정값만 갱신하도록 구현하고 회귀 검사를 완료했다.

## Task: 2026-08-07 Artillery Nexus Caster Data Handoff

- Task title: 포격대 Nexus 발사자·유물 데이터 handoff
- Goals: 포격대 시너지와 유물 CSV 계약에 Nexus caster, 0.1초 주 포격 착탄 지연, 레벨 8 주 포격만 2배, 원래 폭발·파편 피해 중첩, `MagazineRemaining == MaxMagazineSize` 첫 발 조건을 반영한다.
- Constraints: 기존 `sein-c`를 복제하지 않고 Nexus가 학습한다. `artifact_effects.csv.recipient_scope`를 유물 대상 범위의 단일 원천으로 사용한다. 무한 탄피·관통 깃털만 `AllAllies`; 나머지는 `Owner`. fragment hit-exclusion 데이터는 만들지 않는다.
- Role Owner: Code Builder
- Status: Phase 1~4 구현과 Code Reviewer 수정, Unity EditMode 검증 완료.
- Next Actions: 사용자 Play Mode에서 실제 포격 발사 위치·시각·피해 중첩을 확인한다.
- Evidence: support trigger CSV는 `OnReloadComplete|MagazineProjectile|owner`; ExecuteSkill은 `sein-c|Nexus|true|Densest|60|Physical|0.1`. 단계 4는 raw 85/radius 1.15, 단계 6은 `3|0.3|3|30`, 단계 8은 main raw 120/radius 2. 포격대 artifact effect는 11행이며 `AllAllies`는 infinite shell·piercing feather 2행, 나머지 9행은 `Owner`. blessed quiver는 Holy/Fire 조건별 1.18, lightning magazine은 Lightning MagazineProjectile outgoing damage 20% Shock다. Reviewer에서 ExecuteSkill enum allowed-value 구분자 오류를 발견해 기존 parser 계약 `|`로 수정했다. `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` 오류 0, Unity EditMode job `235bba61ee924389b80aac73d652f55f` 35/35 통과.
- History: 2026-08-07 포격대 handoff 작성. Nexus는 공통 `UnitCombatState`/`SkillState`를 가지지만 `CreateNexus()`가 active skill을 학습시키지 않는 것을 확인. 사용자 clarification으로 Nexus 발사, 0.1초 주 폭발, 파편 제외 2배, 피해 중첩 허용, full-magazine 첫 발 조건을 반영. 2026-08-07 Phase 1은 별도 포격 Manager 대신 기존 SkillModifier/ExecuteSkill 계약을 최소 확장했다. 2026-08-07 Phase 2는 탄창 복구를 단일 helper로 모으고 8개 비속성 유물과 reload 시너지를 연결했다. 2026-08-07 Phase 3은 Nexus에 기존 Sein-C를 학습시키고 event-source synergy snapshot을 Nexus 실행에 전달했다. 2026-08-07 Phase 4는 blessed quiver 속성 분리, lightning magazine trigger, reward allowlist와 catalog 회귀 검사를 추가했다. Code Reviewer는 enum allowed-value delimiter와 ExecuteSkill status payload 손실을 발견했고 Code Builder가 공통 source에서 수정했다.

## Task: 2026-08-07 Sentinel Artifact And Synergy Data Handoff

### Task title

파수꾼 2/4/6/8 시너지와 개별 유물 10종을 기존 Artifact Effect graph/trigger CSV에 작성한다.

### Goals

- 파수꾼 시너지 설명을 방어 보너스 `5/10/15/20%`, 고정 방어력 `8/12/18/25`로 맞춘다.
- 누적 단계 배포에서 총합이 중복되지 않도록 단계별 증가분을 Node로 기록한다.
- `artifact_effects.csv.recipient_scope`를 사용자 문구에 맞게 확정한다.
- 기존 Artifact graph/trigger와 공통 Node 정의 CSV를 재사용한다.

### Constraints

- `모든 아군`이 명시된 유물만 `AllAllies`, 나머지는 `Owner`다.
- `unbreakable-promise-effect`만 `Owner`로 변경하고 나머지 파수꾼 9개는 `AllAllies`를 유지한다.
- 새 파수꾼 전용 CSV를 만들지 않는다.
- 수치를 C# Effect ID 분기로 하드코딩하지 않는다.
- Designer 단계에서는 실행 CSV를 수정하지 않는다.

### Role Owner

Designer handoff, Code Builder implementation.

### Status

구현 완료. CSV graph/trigger runtime 생성과 파수꾼 데이터 집중 EditMode 검증 통과. 사용자 Play Mode 검증 대기.

### Next Actions

- Play Mode에서 파수꾼 보상 노출, 2/4/6/8 표시 수치와 개별 유물 설명·적용 대상을 확인한다.

### Evidence

- 파수꾼 Artifact/시너지 Effect 헤더는 존재하지만 `skill_graph_nodes_artifact.csv`와 `artifact_skill_triger.csv`에 파수꾼 실행 행은 없다.
- 현재 `artifact_synergies.csv` 파수꾼 수치는 `15/25/40/60%`로 사용자 확정값과 다르다.
- 현재 파수꾼 개별 Effect 10개는 모두 `AllAllies`다.
- 기존 `Owner` enum, CSV parser, Stage 배포와 개별/시너지 Node/Reaction 생성 경로는 구현돼 있다.
- 상세 데이터 매핑은 `Pakuri/reference/4.run/sentinel-artifact-synergy-implementation-design.md`에 기록했다.
- `artifact_synergies.csv`는 총합 설명 5/10/15/20%와 고정 방어력 8/12/18/25를 사용하고 graph는 단계별 증가분 `0.05/8`, `0.05/4`, `0.05/6`, `0.05/7`을 사용한다.
- `artifact_effects.csv`는 `unbreakable-promise-effect`만 `Owner`, 나머지 파수꾼 9개는 `AllAllies`다. 집중 테스트가 10개 Effect의 실제 `ArtifactEffectRecipient`를 확인했다.
- `artifact_synergy_effects.csv`에 보호막 조건 최종 피해 Effect 1개를 추가해 총 28개가 됐다.
- Artifact graph 39행과 trigger 9행을 기존 CSV에 추가했고 신규 전용 CSV는 만들지 않았다. CSV 열 수와 Effect 참조 정적 검사 오류는 0개다.
- `ApplyShield.target_max_health_ratio`, `SelectTargets.radius`, `DefenseModifier`, `FinalDamageTakenMultiplier`, `CooldownChargeSpeedBonus`가 기존 Node 정의/인자 CSV와 builder에 연결됐다.
- 순백 방패 12%/9999초, 순례자 망토 50%/10초, 반사 25/20/20%, 향로 +2초, 기도석 +12%가 runtime definition 집중 테스트를 통과했다.
- `ArtifactUI.PrepareChoices` allowlist에 `sentinel`을 추가했고 변경 영향 보상 테스트가 통과했다.
- Phase 커밋: `4bb9bc0` 데이터 계약, `456efc6` 전투 보정, `49d1e0b` 보호막 사건 런타임.

### History

- 2026-08-07: 사용자가 파수꾼 시너지·유물 수치와 모든 아군/보유자 적용 규칙을 확정했다.
- 2026-08-07: Designer가 누적 단계 증가분과 17개 Effect 데이터 계약을 작성했다.
- 2026-08-07: Code Builder가 기존 Artifact CSV에 파수꾼 39개 graph Node와 9개 Trigger를 작성하고 공통 Node 스키마를 확장했다.
- 2026-08-07: runtime catalog 동기화 메뉴 실행, Unity 오류 0개, 파수꾼 집중 EditMode 7/7을 확인했다.

## Task: 2026-08-07 Chosen One Synergy Effect Visual

### Task title

선택받은자 유닛 위치에 CSV 지정 Sprite 시너지 이펙트를 표시한다.

### Goals

- `artifact_synergies.csv`에 시너지 이펙트 Sprite, Alpha, Layer 값을 저장한다.
- `chosen-one` 선택 유닛의 Transform에 기존 `EffectManager` Sprite 이펙트를 부착한다.
- `chosen-one` 시너지 보유 수량이 2 이상일 때만 해당 이펙트를 생성한다.

### Constraints

- 기존 `EffectManager.CreateEffect`와 `RuntimeSkillVisualSpec`을 재사용한다.
- Alpha는 0~100 퍼센트, Layer는 Monster SpriteRenderer 최대 sorting order보다 높은 38을 사용한다.
- 현재 단계에서는 Sprite만 표시하고 애니메이션·별도 이펙트 API는 추가하지 않는다.
- 시너지 수량이 2 미만이면 선택 유닛이 있어도 이펙트를 생성하지 않는다.

### Role Owner

Code Builder.

### Status

구현 완료. CSV 열 정합성, Unity runtime catalog 동기화, Core/Editor 빌드를 확인했다. 실제 Play Mode 표시 확인은 사용자 소유다.

### Next Actions

- Play Mode에서 선택받은 자 유닛 위치의 `SpotLight` 표시·투명도·레이어를 확인한다.

### Evidence

- `Pakuri/Assets/CSVdata/Artifact/artifact_synergies.csv`의 `chosen-one` 행은 `Assets/Image/Object/SpotLight.png`, Alpha `50`, Layer `38`이다.
- quote-aware CSV 검증 결과 8개 행 모두 헤더와 동일한 20열이다.
- `ArtifactSynergyManager`가 `UnitSpawnManager.Players`의 선택 유닛 Transform과 `combatManager.Effects`를 기존 `EffectManager.CreateEffect`에 전달한다.
- `CreateChosenOneEffect`가 `Synergies.GetCount("chosen-one") < 2`를 먼저 차단한다.
- `EffectVisualBuilder`가 SpriteRenderer 색상 Alpha와 sorting order를 `RuntimeSkillVisualSpec`에서 적용한다.
- Unity 로그에 `GameDataLoader loaded runtime catalog ... 6 artifact synergies`가 기록됐고, `CsvRuntimeCatalog.asset`에 `Assets/Image/Object/SpotLight.png` 참조가 생성됐다.
- Core/Editor `dotnet build --no-restore` 결과 오류 0개, 기존 Unity 참조 경고 2개다.

### History

- 2026-08-07: Code Builder가 선택받은자 시너지 Sprite/Alpha/Layer CSV 계약과 기존 EffectManager 부착 경로를 구현했다.
- 2026-08-07: tracker 행의 19열 오류를 20열로 보정하고 Unity CSV 검증·runtime catalog 동기화를 통과시켰다.
- 2026-08-07: Code Builder가 선택받은자 시너지 수량 2 미만의 이펙트 생성을 차단했다.

## Task: 2026-08-07 Chosen One Artifact Data Design

### Task title

기존 선택받은자 Effect 헤더에 실행 Node·Trigger를 연결하고 신규 공통 Node 인자를 정의한다.

### Goals

- 선택받은자 시너지 네 단계와 개별 유물 10종을 기존 Artifact graph/trigger CSV에 저작한다.
- 이름 없는 명부 설명을 `이름표식 1스택당 최종선고 위력 +6%`로 정정한다.
- 스택 비례 위력, 사건 스킬 실행, 상태 행동속도 배율, 조건부 최종 데미지 Node 계약을 추가한다.

### Constraints

- 새 선택받은자 전용 graph/trigger CSV를 만들지 않는다.
- 기존 Effect ID와 `SpecificMonster` 수신자 계약을 유지한다.
- CSV UTF-8, 열 수, 외래 키와 Node 인자 검증을 통과해야 한다.
- Designer 단계에서는 CSV를 수정하지 않는다.

### Role Owner

Designer.

### Status

데이터 구현 완료. `absolute-zero-circuit-effect` 인자 순서 오류 수정 및 정적 검증 완료. Unity 카탈로그 검증은 열린 Editor 잠금으로 대기.

### Next Actions

- Unity Editor 잠금이 해제되면 `CsvCatalogEditor.ValidateSourceDataMenu`를 실행한다.

### Evidence

- `artifact_effects.csv` 33~44행에 선택받은자 개별 Effect 헤더가 있다.
- `artifact_synergy_effects.csv` 12~15행에 시너지 Effect 헤더가 있다.
- `skill_graph_nodes_artifact.csv`에 선택받은자 시너지 3개, 개별 유물 10종, 무투가 Trigger 그래프를 추가했다.
- `artifact_skill_triger.csv`에 앙코르(`OnSkillCast`, `every_count=3`)와 무투가(`OnOutgoingDamage`, `Physical`)를 추가했다.
- 기존 `TargetStatusStackDamageRateBonus`는 이름 없는 명부의 확정 위력 계약과 다르다.
- `GameDataCatalogBuilder.Artifacts`는 이미 개별·시너지 Effect의 Node와 Reaction을 생성한다.
- 정적 외래 키 검사 결과 graph 117행/trigger 14행의 소유자·효과 참조 오류 0개, node definition 누락/중복 0개다.
- `ConditionalDamageMultiplier` 정의 순서가 `status_name,min_stacks,multiplier`임을 확인하고 행을 `freeze,1,1.25`로 수정했다.
- Artifact graph numeric validation 결과 `artifact_graph_rows=117 invalid_numeric_params=0`이다.
- `artifact_skill_triger.csv` 헤더와 타입 행은 `event_skill_slots` 열을 포함해 각각 24열이며, 앙코르 데이터는 `A;B;C;D;E`, `proc_chance=1`, `trigger_every_count=3`이다.
- `CsvRowParser.SkillTriggerRow`, `StatusValueParser.ParseSkillSlots`, `GameDataCatalogBuilder`가 새 슬롯 필드를 `SkillReaction.EventSkillSlots`로 매핑하고 `CsvDataValidator`가 enum 값을 검증한다.
- `GameDataCatalogBuilder.IsNormalCastEffect`는 event skill name이 비어도 runtime kind/slot 조건이 있으면 일반 OnSkillCast로 분류하지 않아 앙코르 반응을 생성한다.

### History

- 2026-08-07: 사용자가 선택받은자 설계 MD와 수정 대상 목록 작성을 요청했다.
- 2026-08-07: Designer가 데이터 계약과 이름 없는 명부 위력 공식을 기록했다.
- 2026-08-07: Code Builder가 신규 Node 계약, Artifact graph/trigger 행, 이름 없는 명부 문구를 반영했다.
- 2026-08-07: Code Reviewer가 CSV 열 수·파라미터·외래 키 정적 검사를 통과시켰다.
- 2026-08-07: Unity가 `min_stacks=1.25`를 보고한 원인은 `absolute-zero-circuit-effect` CSV 인자 순서 오류였고, Code Builder가 행을 교정한 뒤 runtime/editor 빌드 오류 0개를 확인했다.
- 2026-08-07: Code Builder가 앙코르 Trigger에 `event_skill_slots` CSV 계약을 추가하고 parser/generator/validator 매핑을 구현했다.
- 2026-08-07: 앙코르 행의 빈 `event_skill_name` 때문에 반응 생성에서 제외되던 필터를 수정했다.
- 2026-08-07: 앙코르 슬롯 제한을 `A;B;C;D;E`로 확장하고 기존 탄창 마지막 발사 계약은 유지했다.

## Task: 2026-08-07 MainMenu Monster Standing Text Contract

### Task title

MainMenu 몬스터 선택이 기존 `display_name`·`role_summary` 계약을 사용하도록 연결한다.

### Goals

- CSV를 중복 파싱하지 않고 Runtime catalog의 `MonsterDefinition`을 사용한다.
- 선택 몬스터 표시 이름과 역할 설명을 UI 텍스트에 전달한다.

### Constraints

- `monsters.csv`와 CSV 스키마는 수정하지 않는다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. CSV 값 검증·Unity MCP 스크립트 검증·솔루션 빌드를 통과했다.

### Next Actions

- Unity Play Mode에서 실제 텍스트 전환을 확인한다.

### Evidence

- `monsters.csv` 헤더에 `display_name`, `role_summary`가 있고 5개 행 값이 비어 있지 않다.
- 기존 `CsvRowParser`·`GameDataCatalogBuilder`가 두 값을 `MonsterDefinition`에 보존한다.
- `MainMenuUIManager`가 `GetMonster(...).DisplayName`·`RoleSummary`를 각각 Name·Desc에 할당한다.

### History

- 2026-08-07: Code Builder가 기존 Data contract를 MainMenu Standing 텍스트 표시와 연결했다.

## Task: 2026-08-07 MainMenu Monster Standing Image Contract

### Task title

MainMenu 몬스터 선택이 `monsters.csv`의 기존 `Image` 계약을 사용하도록 연결한다.

### Goals

- CSV를 중복 파싱하지 않고 기존 Runtime catalog의 `MonsterDefinition.Image`를 사용한다.
- 5개 플레이 가능 몬스터 Image 경로와 파일 존재를 검증한다.

### Constraints

- `monsters.csv` 및 CSV 스키마는 수정하지 않는다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. CSV·에셋 정적 검증과 솔루션 빌드를 통과했다.

### Next Actions

- Unity Play Mode에서 실제 Sprite 전환을 확인한다.

### Evidence

- `CsvRowParser`가 `Image`를 읽고 `GameDataCatalogBuilder`가 `LoadSprite`로 `MonsterDefinition.Image`를 만든다.
- `monsters.csv`의 ariel/eve/rin/sein/vega 5행 Image 경로가 각각 실제 PNG로 확인됐다.
- `MainMenuUIManager`는 `GameDataLoader.CurrentCatalog.GetMonster(...).Image`를 사용한다.

### History

- 2026-08-07: Code Builder가 기존 Image CSV 계약을 MainMenu Standing 표시와 연결했다.

## Task: 2026-08-07 Artifact Owner Recipient Data Contract

### Task title

`artifact_effects.csv.recipient_scope`에 보유자 전용 `Owner` 계약을 추가한다.

### Goals

- 표시 문구가 아닌 typed CSV 값으로 유물 수신 범위를 결정한다.
- 정령계약·처형관의 구현된 Effect를 `AllAllies`와 `Owner`로 명확히 분리한다.

### Constraints

- CSV 열을 추가하지 않는다.
- `artifact_synergy_effects.csv`와 기존 Node/Trigger 수치는 변경하지 않는다.
- 미구현 Chosen One, Sentinel, Artillery Node/Trigger는 작성하지 않는다.

### Role Owner

Designer handoff, Code Builder implementation.

### Status

구현 완료. CSV 구조·Runtime catalog·집중 Unity EditMode 검증을 통과했다.

### Next Actions

- Play Mode에서 CSV 범위와 실제 유물 효과 주체를 확인한다.

### Evidence

- 현재 정령계약·처형관 Effect 대부분이 설명 범위와 무관하게 `AllAllies`로 작성돼 있다.
- `spirit-elixir-contract-count-effect`와 `elemental-codex-effect`는 Owner 전환 뒤에도 repeat rule을 사용한다.
- 확정 행 목록은 `Pakuri/reference/4.run/artifact-owner-recipient-implementation-handoff.md`에 기록했다.
- `artifact_effects.csv`는 63행, 9열 정합성을 유지하며 Owner 23행, AllAllies 27행, SpecificMonster 12행이다.
- Unity Runtime catalog가 50 artifacts, 6 synergies, 1 summon을 로드했고 Owner 배포 집중 검증이 3/3 통과했다.
- `CsvDataValidator`가 repeat rule에 AllAllies 또는 Owner를 허용한다.

### History

- 2026-08-07: 사용자가 모든 아군으로 명시되지 않은 구현 유물을 보유자 전용으로 분리하는 Handoff를 요청했다.
- 2026-08-07: Designer가 새 열 없이 enum 값과 기존 배포 경로를 재사용하는 데이터 계약을 확정했다.
- 2026-08-07: Code Builder가 구현된 정령계약·처형관 Effect의 recipient_scope와 repeat validator를 갱신했다.

## Task: 2026-08-06 Executioner Artifact And Synergy Effect Data Design

### Task title

처형관 유물 10개와 2/4/6/8 시너지 Effect 헤더를 실제 Node/Trigger 데이터에 연결한다.

### Goals

- 기존 Artifact graph/trigger CSV를 재사용한다.
- 시너지 Effect도 개별 유물 Effect와 같은 typed Node/Reaction을 보유한다.
- 모든 수치는 CSV가 소유한다.
- 유리 심장·별빛 숫돌은 기존 Effect ID에 각각 단일 `+0.20` 치명타 보정 Node를 기록한다.

### Constraints

- 새 유물 전용 Node/Trigger CSV를 만들지 않는다.
- 이미 존재하는 처형관 Effect ID와 icon 경로를 유지한다.
- 존재하지 않는 Node/Trigger 계약을 현재 구현된 것처럼 기록하지 않는다.
- Designer 단계에서는 CSV를 수정하지 않는다.

### Role Owner

Code Builder.

### Status

Phase 0~4 구현 완료. Node/Trigger CSV 행과 공통 parser/runtime 매핑이 반영됐다. 정적 CSV·빌드는 통과했고 Unity 카탈로그 런타임 검증은 MCP 인스턴스 0개로 보류됐다.

### Next Actions

- Unity MCP 인스턴스가 연결되면 CSV runtime catalog validation을 재실행한다.
- Unity Play Mode에서 처형관 보상 후보와 Stage 시작 Effect를 확인한다.

### Evidence

- `artifacts.csv`에는 처형관 유물 10개와 실제 icon 경로가 있다.
- `artifact_synergies.csv`에는 처형관 2/4/6/8 설명과 synergy icon 경로가 있다.
- `artifact_effects.csv`에는 10개 Effect 헤더, `artifact_synergy_effects.csv`에는 네 단계 Effect 헤더가 있다.
- `skill_graph_nodes_artifact.csv`에는 처형관 4단계와 10개 유물 Effect Node가 반영됐다.
- `ArtifactSynergyEffectDefinition`과 `BuildArtifactSynergyEffects`는 typed Node/Reaction을 생성한다.
- `skill_node_definitions.csv`와 params에 조건부 치명타, 마지막 투사체, 후치명타 최종 피해 Node 계약이 반영됐다.
- `artifact_skill_triger.csv`는 `require_event_critical` 필드를 포함하며 sharp chalice의 실제 치명타 반응을 제한한다.
- `artifacts.csv`에서 유리 심장과 별빛 숫돌은 각각 단일 `+20%` 설명으로 정정됐다.

### History

- 2026-08-06: Designer가 처형관 데이터의 존재 여부와 실제 runtime 연결 여부를 분리해 감사했다.
- 2026-08-07: 처형관 Effect ID, 공통 graph/trigger 계약과 CSV 검증 기준을 새 전용 구현 설계 문서에 기록했다.
- 2026-08-07: 별빛 숫돌은 치명타 확률 단일 `+0.20`, 유리 심장은 치명타 피해 단일 `+0.20`으로 확정했다. 백은 바늘은 기존 마지막 탄창 투사체 flag를 소비하도록 설계를 축소했다.
- 2026-08-07: Code Builder가 Phase 4 CSV Node/Trigger 데이터와 parser 연결을 커밋했고 필드 수 정합성·빌드를 통과시켰다.

## Task: 2026-08-06 Final Damage Modifier Node Contract

### Task title

스킬 Graph CSV에서 후치명타 최종 피해 배율을 작성할 `FinalDamageModifier` Node 계약을 추가한다.

### Goals

- `FinalDamageModifier` Node가 스킬 실행 스냅샷의 동명 배율에 반영되게 한다.
- 기존 `DamageMultiplier` Node와 데이터 의미를 명확히 분리한다.

### Constraints

- 새 Base 스킬 CSV 열은 추가하지 않는다. 기존 수치 보정 방식처럼 공통 Graph Node 정의를 사용한다.
- `skill_node_definitions.csv`에는 `FinalDamageModifier / FinalDamageModifier / DamageModifier`를 추가한다.
- `skill_node_definition_params.csv`에는 필수 float 매개변수 `multiplier` 하나를 추가한다.
- 값은 보너스율이 아니라 배율이다: +15%는 `1.15`, -15%는 `0.85`, 무효는 `1`이다.
- 실제 스킬 Graph 행은 별도 스킬 지정 없이는 추가하거나 기존 `DamageMultiplier`에서 자동 변환하지 않는다.

### Role Owner

Designer.

### Status

Design ready. Node 정의와 런타임 매핑은 아직 존재하지 않는다.

### Next Actions

- Code Builder가 두 Node 정의 CSV와 `SkillActionOpKind`, `GameDataCatalogBuilder.Nodes`, `SkillExecutionRules`를 함께 변경한다.
- CSV 구조 검사, 런타임 카탈로그 빌드와 스냅샷 배율 테스트를 수행한다.

### Evidence

- 현재 Node 정의에는 `DamageMultiplier`와 필수 float `multiplier` 매개변수가 있지만 `FinalDamageModifier`는 없다.
- 현재 Base active-skill CSV 여섯 종류에는 일반 `DamageMultiplier` 열도 없으며 수치 보정은 Graph Node로 작성된다.
- `GameDataCatalogBuilder.Nodes.MapSkillActionOp`가 문자열 Handler를 `SkillActionOpKind`로 바꾸고 `SkillExecutionRules.ApplyNodeAction`이 스냅샷에 반영한다.
- Monster Graph와 Artifact Graph에는 기존 `DamageMultiplier` 작성 사례가 있으므로 신규 의미도 공통 Node 계약으로 추가하는 것이 현재 데이터 구조와 일치한다.

### History

- 2026-08-06: 사용자가 CSV에서도 `FinalDamageModifier`를 최종 피해 의미로 사용할 것을 요청했다.
- 2026-08-06: Designer가 Base CSV 열 확장 없이 공통 Graph Node 계약으로 설계했다.

## Task: 2026-08-06 Artifact Icon Asset-Path Assignment

### Task title

Assign `artifacts.csv` `artifact_icon` paths from the inspected `*_Icon` asset folders.

### Goals

- Populate each artifact row whose `artifact_id` matches an inspected PNG filename.
- Preserve the existing `artifact_icon`/`asset_path` schema and use project-relative `Assets/...` paths.
- Leave artifacts without a name-matched image blank.

### Constraints

- Change only `Pakuri/Assets/CSVdata/Artifact/artifacts.csv` for this assignment.
- Use actual on-disk paths; do not invent a path for `resonance-compass`.
- Preserve existing user changes and the actual `shattering-glove'.png` filename.

### Role Owner

Code Builder

### Status

CSV assignment complete and statically verified. Unity TextAsset reimport/runtime catalog verification remains pending.

### Next Actions

- Reimport or sync the changed CSV in Unity and verify artifact Sprite resolution if runtime confirmation is required.
- Author/rename a matching `resonance-compass` icon before filling its blank path.

### Evidence

- `Pakuri/Assets/Image/Artifact` contains 49 artifact-name-matched PNGs across the five inspected `*_Icon` folders plus one timestamp-named PNG with no matching `artifact_id`.
- `artifacts.csv` contains 50 data rows; 49 `artifact_icon` paths are populated, `resonance-compass` remains blank, and all 49 populated paths exist on disk.
- The actual folders contain `sentinel` artifact images under `artillery_Icon` and `artillery` artifact images under `sentinel_Icon`; CSV paths use those actual locations.
- Static path check reports `MISSING_FILES=0`; duplicate populated paths are absent; `git diff --check -- Pakuri/Assets/CSVdata/Artifact/artifacts.csv` passed.
- One basename mismatch is intentional and evidence-backed: artifact ID `shattering-glove` points to existing file `shattering-glove'.png` without renaming it.

### History

- 2026-08-06: Code Builder inspected the CSV and all `*_Icon` descendants, then assigned 49 existing artifact PNG paths and left `resonance-compass` blank.

## Task: 2026-08-06 String Key Name Migration

### Task title

Rename string-backed ID key names to `_name`/`Name` across active CSV schemas, C# scripts, and serialized scene field references.

### Goals

- Rename active `Assets/CSVdata` schema tokens from `_id`/`id` to `_name`/`name` without converting values to numeric IDs.
- Rename corresponding C# string key identifiers and lookup method names from `Id`/`id` to `Name`/`name`.
- Keep display-name semantics distinct where an existing `SkillName` field collided with the renamed skill key; use `DisplayName` for the existing display field.
- Synchronize the active `InGameScene` serialized spawn field keys with the renamed C# fields.

### Constraints

- Change names only; do not convert string key values or CSV data values to integers.
- Preserve existing user-owned Artifact/Skill CSV changes and leave historical `Assets/Legacy` content untouched.
- Do not rename files or add a compatibility schema layer.

### Role Owner

Code Builder

### Status

Implemented and statically/build verified; Unity TextAsset reimport and Play Mode remain user-owned.

### Next Actions

- Reimport/sync the changed CSV TextAssets in Unity and run the existing CSV validation/catalog synchronization if runtime confirmation is required.
- Verify scene spawn references and skill display text in Play Mode after Unity serialization refresh.

### Evidence

- The inspected migration scope contained 52 CSV files with 324 old schema tokens and 99 C# files, of which 70 contained 2,866 old key-token matches; all 52 CSV files and those 70 C# files were updated.
- Post-change searches report `OLD_CSV_MATCHES=0`, `OLD_CS_MATCHES=0` across `Pakuri/Assets/CSVdata` and `Pakuri/Assets/Scripts`; active `InGameScene.unity` reports `OLD_SCENE_SERIALIZED_MATCHES=0`.
- PowerShell `Import-Csv` validation read 52 files and 2,358 rows with `CSV_BAD=0`; `git diff --check` reports 0 lines.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:q` completed with 0 errors and the existing 2 Unity assembly-reference warnings.
- `SkillDefinition` retains the renamed key as `SkillName` and moves the pre-existing display field to `DisplayName`, preventing the compiler-confirmed duplicate-member collision.

### History

- 2026-08-06: Code Builder inspected CSV type rows, parser string reads, all active C# string key declarations/callers, and the active scene serialization keys before editing.
- 2026-08-06: Code Builder mechanically renamed active CSV/C# string key names, synchronized `monsterName`/`enemyName`/`summonName` scene fields, corrected the `SkillName`/`DisplayName` semantic collision, and passed the final C# build.

## Task: 2026-08-06 Unity Console Compile Error Repair

### Task title

Repair the 96 Unity Console compile errors caused by stale ID API references in the editor runtime tests.

### Goals

- Remove stale `SkillId`/`MonsterId`/`FindBySkillId` and related API references from the editor test source.
- Preserve production runtime logic and change only test identifiers/reflection names to the already-implemented `Name` API.
- Verify Unity recompilation and the editor project build produce no compile errors.

### Constraints

- Modify only `Assets/Tests/Editor/SkillCatalogRuntimeTests.cs` for this error group.
- Do not restore obsolete ID aliases or change production behavior.
- Keep existing test values, assertions, and execution flow unchanged.

### Role Owner

Code Builder

### Status

Implemented and compile verified. Unity Console currently reports zero errors after recompile; Play Mode remains user-owned.

### Next Actions

- If full EditMode green status is required, separately review the three existing data/trigger baseline assertion failures; they are outside this compile-error repair.

### Evidence

- Unity `read_console` initially returned exactly 96 errors, all from `Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`, reporting missing renamed members such as `SkillId`, `MonsterId`, `FindBySkillId`, `ReactionId`, `EffectId`, and `ActiveArtifactEffectIds`.
- The test file contained 117 old ID-token matches; only those identifiers, local names, and the `PreparedSkillId` reflection string were renamed. Post-change search reports `OLD_TEST_ID_TOKENS=0`.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore -v:q` completed with 0 errors and the existing 2 Unity assembly-reference warnings.
- Unity script refresh/recompile completed; after clearing the Console, `read_console` returned `total=0` error entries.
- EditMode run completed 25 tests; the three reported failures are the known resonance-compass icon expectation and two existing trigger baseline assertions, not compile errors from this repair.
- `git diff --check` reports 0 lines; the test diff is identifier-only (103 replacements).

### History

- 2026-08-06: Code Builder traced all 96 Console errors to the single editor test file, renamed its stale API references, refreshed Unity scripts, passed the Editor C# build, and confirmed zero Console errors after clearing/rechecking.

## Task: 2026-08-06 Skill ID and Artifact CSV Ownership Split

### Task title

Remove legacy `@effect` IDs and separate artifact graph/trigger authoring from monster skill CSVs.

### Goals

- Replace all legacy `@effectN` references with explicit `*-trigger-effect-N` IDs while preserving Base/Trait IDs.
- Move artifact-owned graph and trigger rows out of monster passive files into `Assets/CSVdata/Artifact/Skill`.
- Keep the existing combined `SourceModel` and runtime Definition behavior unchanged.

### Constraints

- Preserve every moved row and all non-ID values exactly.
- Keep monster passive rows in their existing passive CSVs.
- Do not change `SetDuration` values or gameplay logic.
- Do not create a Git commit for this task.

### Role Owner

Code Builder.

### Status

Implemented and locally verified; Play Mode remains user-owned.

### Next Actions

- User verifies artifact UI/runtime behavior after the new CSV source paths are imported.

### Evidence

- `@` search across current skill CSVs and `Artifact/Skill` returns 0 matches; all 43 legacy occurrences were normalized with paired graph/trigger/reference IDs.
- `skill_graph_nodes_passive.csv` retains 454 monster rows; `Artifact/Skill/skill_graph_nodes_artifact.csv` contains the exact 75 artifact rows formerly embedded there.
- `passive_skill_triger.csv` retains 128 monster/summon rows; `Artifact/Skill/artifact_skill_triger.csv` contains the exact 11 artifact trigger rows formerly embedded there.
- Exact row comparison against `HEAD` reports `ARTIFACT_GRAPH_EXACT_DIFF=0` and `ARTIFACT_TRIGGER_EXACT_DIFF=0`.
- Combined graph/trigger validation reports 937 graph rows, 172 trigger rows, zero duplicate trigger IDs, and zero missing Trigger/Base owners.
- `CsvRuntimeCatalog`, `CsvCatalogEditor`, `CsvSourceLoader`, and `GameDataLoader` now load the two Artifact/Skill TextAsset arrays into the same source model path.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` passed with 0 errors and the existing 2 assembly-reference warnings.
- Unity catalog sync/validation loaded 5 monsters, 8+8 enemies, 50 artifacts, 6 synergies, and 1 summon. Focused passive lifetime test passed 1/1; artifact trigger runtime test passed 1/1.
- The combined artifact-definition test currently fails only because the user-owned `artifacts.csv` now assigns a `resonance-compass` icon while that existing test still expects null.

### History

- 2026-08-06: Code Builder normalized all legacy `@effectN` IDs, moved 75 artifact graph rows and 11 artifact trigger rows into `Artifact/Skill`, connected the new source arrays, and refreshed Unity catalogs without creating a commit.

## Archived History

The pre-cleanup file, including completed and superseded data tasks, is preserved at `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-08-06 Passive Base/Trait Table Normalization

### Task title

Make passive Base and trait authoring ownership explicit without changing runtime behavior.

### Goals

- Keep `skills_passive.csv` as the passive Base identity/description table.
- Keep only the 25 x 3 trait rows in `skill_choices_passive.csv`; remove the two inert `PassiveBase` rows and its redundant `choice_group` column.
- Use `owner_kind=Base` for passive Base graph groups and replace passive `@effect` owner IDs with explicit Base/Trait effect IDs.
- Preserve Stage-permanent `SetDuration=9999` and explicit timed durations such as Eve-F's 12 seconds.

### Constraints

- Preserve generated runtime effects, trigger ordering, conditions, target selection, and Stage lifetime values.
- The initial passive migration preserved active-skill `@effect` IDs; the follow-up Skill ID split normalizes all remaining `@effect` IDs with their paired graph/trigger/reference rows.
- Multi-effect passive groups require an effect suffix; a single `{slot}-base` ID cannot represent Eve-J's seven independent Base graphs.
- Keep CSV files UTF-8 and update every exact trigger/graph/reference ID together.

### Role Owner

Code Builder.

### Status

Implementation complete in source and authoring data; Unity catalog sync/validation and the focused EditMode runtime test passed. Play Mode OfferingPanel -> next Stage verification remains user-owned.

### Next Actions

- User verifies OfferingPanel acquisition and next-Stage passive application in Play Mode, specifically Eve-F's 12-second shield.

### Evidence

- `skill_choices_passive.csv` now has 75 data rows, seven columns, no `choice_group`, and every passive skill has exactly three inferred `PassiveEnhancement` choices.
- `CsvRowParser.ParseSkillChoiceRow` accepts an implicit group; `CsvSourceLoader` supplies `PassiveEnhancement` only for the passive choice file.
- `SkillGraphParser` and `CsvDataValidator` accept/validate `owner_kind=Base`; `GameDataCatalogBuilder` routes blank-choice passive Base triggers through Base graph owners.
- Passive Base trigger/graph IDs now use `*-base-effect-N`; trait auxiliary IDs use `*-trait-N-effect-N`. 144 Base graph rows have zero missing trigger/source joins; the two direct snapshot Base groups (`sein-i`, `vega-h`) are retained in `PassiveSkillDefinition.BaseNodes` and applied when execution data is built.
- Remaining five passive-file `@effect` groups are active-skill reaction owners (`rin-e`, `sein-c`, `sein-d`, `sein-e` sources), so they were intentionally not renamed as passive Base/Trait IDs.
- Original trigger non-ID fields and graph non-owner fields compare equal after ID normalization; `SetDuration=9999` and Eve-F shield `SetDuration=12` remain unchanged.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` passed with 0 errors and the existing two assembly-reference warnings.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` completed without catalog errors; focused EditMode `PassiveStageModifiersPreserveLifetimeAndDynamicConditions` passed 1/1, including the direct `0.8` shot interval and `1.2` duration snapshots.

### History

- 2026-08-06: Designer approved table-intuitive separation; Builder began implementation with Stage-permanent `9999` retained.
- 2026-08-06: Phase 1 contract committed as `91c4ebd`.
- 2026-08-06: Phase 2 removed the two inert Base choice rows and `choice_group`, added passive-file group inference, committed as `fb3cce6`.
- 2026-08-06: Phase 3 added Base owner routing, migrated passive Base/Trait IDs, removed the obsolete `PassiveBase` runtime path, updated tests, and committed as `73b4d91`.
- 2026-08-06: Unity validation exposed two orphaned former `Choice` graph owners (`sein-i`, `vega-h`); they were migrated to explicit Base owners with separate Base triggers, and direct Base snapshot nodes were wired through the passive Definition/execution path. Focused EditMode test passed 1/1.

## Task: 2026-08-05 Spirit King Skill Runtime Data

### Task title

Connect the authored Spirit King skills to the existing graph, trigger and Definition generation path.

### Goals

- Keep `summon_units.csv` in the existing monster-shaped schema with `base_move_speed=0.5`, max health 1000, Physical primary attribute and all six defenses 50.
- Generate four `SingleSkillDefinition` skills and one `ZoneSkillDefinition` from `summon_units_skill.csv`.
- Reuse existing visual resource fields for Sein-C Master 2 and Eve-C/Eve-D effects.
- Author Densest targeting, three-cast bombardment, Zone pull and OnExpire follow-up in existing graph/trigger CSVs.

### Constraints

- Do not create a new summon skill family or summon-only Node/Trigger CSV.
- `spirit-king-dimensional-rift` is `AreaAttack`; pull is `0.2 unit/tick`, damage is zero and the existing Zone lifecycle emits the follow-up.
- C repeats twice after the first cast, cycles available Densest enemy positions, and reuses the current center when target distribution is unavailable.
- CSV remains the source of skill values and visual resource paths; runtime code consumes generated Definitions.

### Role Owner

Code Builder

### Status

Phase 1 loading/graph implementation and Phase 2 Definition-driven skill ownership are complete; runtime consumption of pull/target selection remains in Phase 3.

### Next Actions

- Runtime Phase 3 consumes the generated `Densest`, `BattlefieldCenter` and `PullToCenterActionOp` values.
- Unity catalog import and focused Definition assertions remain to be run in the Unity environment.

### Evidence

- `summon_units.csv` uses `base_move_speed=0.5`; `summon_units_skill.csv` now authors visual reuse, A/B/C `Densest`, D/E `BattlefieldCenter` and D `AreaAttack`.
- Existing graph rows author C `RepeatPerTarget(2,0.1,1)`, D `PullToCenter(0.2)` and D `OnExpire -> ExecuteSkill(E)`; the D follow-up selects `Nearest` enemies at `EventCenter` so the expiry event does not require a null `EventTarget`.
- `GameDataCatalogBuilder.BuildSummons` now attaches summon-owned reactions to the generated active skill Definitions.
- `SkillGraphParser` and `CsvDataValidator` now accept summon-owned skills/triggers without adding a summon-specific graph/trigger file.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings; edited CSVs import structurally with uniform columns.
- `ArtifactSynergyManager` consumes generated `ArtifactSynergyEffectDefinition.OutcomeSkill` and `SpawnSummon` references; no synergy ID or skill ID switch was added.

### History

- 2026-08-05: Code Builder recorded the corrected Zone Rift, visual reuse, Densest re-selection, pull and follow-up contracts before implementation.
- 2026-08-05: Code Builder connected Spirit King rows and graph/trigger data to the shared summon Loading/Generation path; runtime pull and targeting execution remains deferred to the next phase.
- 2026-08-05: Code Builder corrected the D `OnExpire` target contract and added runtime nearest fallback plus no-live-enemy gating for automatic enemy-target skills.
- 2026-08-06: Code Builder aligned the C graph interval to `0.1`; the shared Single executor now schedules the two repeats even when `UseMultiDeployment` is false and cycles Densest targets with center fallback.

## Task: 2026-08-05 Artifact Synergy Icon Data Binding

### Task title

Load the Spirit Contract HUD icon from `artifact_synergies.csv` through the existing catalog pipeline.

### Goals

- Add optional `Icon_Image` asset-path data to the synergy source schema.
- Carry the field through `CsvRowParser` -> `CsvAssetReferenceCollector` -> Definition Generation -> `ArtifactSynergyDefinition.Icon`.
- Keep the current single Spirit Contract HUD container display-only; no synergy effect execution is added.

### Constraints

- Use the authored asset `Assets/Image/UI/Artifact/ChatGPT Image 2026년 8월 5일 오후 03_39_55.png`.
- Keep other synergy icon cells blank until their assets are authored; do not invent paths.
- Reuse the existing Sprite asset catalog and runtime `LoadSprite` path.

### Role Owner

Code Builder

### Status

Implemented and statically verified. Unity MCP validation timed out after the code/data change; the tracked runtime catalog entry was confirmed by direct file inspection.

### Next Actions

- On the next responsive Unity refresh, run the existing `Pakuri/Validate CSV Source Data` menu item to regenerate/confirm the serialized catalog automatically.
- User verifies the icon in `InGameScene` Play Mode.

### Evidence

- `artifact_synergies.csv` now has 17 columns including `Icon_Image`; the type row is `asset_path`, Spirit Contract has the requested path and the other five rows are blank.
- `CsvRowParser.cs` reads `Icon_Image` into `ArtifactSynergyRow.IconPath`.
- `CsvAssetReferenceCollector.cs` adds each synergy icon path to the shared Sprite reference set.
- `GameDataCatalogBuilder.Artifacts.cs` assigns `ArtifactSynergyDefinition.Icon = LoadSprite(row.IconPath)`.
- `ArtifactDefinitions.cs` exposes `ArtifactSynergyDefinition.Icon` as a `Sprite`.
- The asset exists and its `.meta` GUID is `8b537b0e0f060644cb22f8d33a5bbf01`; `CsvRuntimeCatalog.asset` contains the corresponding path/GUID/first-sprite fileID entry.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings; CSV column-count, path-existence and diff checks passed.

### History

- 2026-08-05: Code Builder added the optional synergy icon field and completed source-model, validation/collection, Definition generation and runtime catalog wiring.

## Task: 2026-08-05 Boss Artifact Reward Data Contract Design

### Task title

Use StageDay boss classification and existing artifact Definitions for reward choices.

### Goals

- Enable artifact rewards for Stage 1/2 `Day5Midboss`, `Day10Midboss` and `Boss` rows through `artifact_choice_count`.
- Keep `StageReward.csv` `artifact_choice_count` as the choice-count switch; no new reward CSV or schema.
- Populate ArtifactPanel from `ArtifactDefinition` and `ArtifactSynergyDefinition`, not direct CSV reads.

### Constraints

- Reuse the existing StageDay, StageReward and loaded Definition contracts; no new reward CSV or schema.
- The first release draw pool is limited to the ten `spirit-contract` artifacts with implemented effects.
- `resonance-compass` has no authored `artifact_icon` path; its choice intentionally hides the missing Icon instead of inventing an asset.

### Role Owner

Code Builder

### Status

Implemented. Stage 1/2 Day5 Midboss, Day10 Midboss and Day11 Boss reward counts are three; normal rows remain zero. The Spirit Contract pool has 9/10 authored icons.

### Next Actions

- Author and assign an icon for `resonance-compass` when the source asset is available.
- User: verify rendered text and icon assignments in Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/stage_flow/StageDay.csv` contains Boss at Stage 1 Day 11 and Stage 2 Day 11.
- Stage 1 and Stage 2 `StageReward.csv` already define `artifact_choice_count`; both midboss rows and boss rows currently use value 3.
- `Pakuri/Assets/Scripts/Loading/Generation/StageDefinitionBuilder.cs` already parses `artifact_choice_count` into `ArtifactChoiceCount`.
- `Pakuri/Assets/Scripts/Combat/Artifact/Definition/ArtifactDefinitions.cs` exposes artifact display name, synergy ID, description, and loaded Sprite icon plus synergy display name.
- UTF-8 `Import-Csv` inspection excluding the type row reported 50 artifacts, 9 nonempty icon paths, and 41 missing icon paths.
- Stage 1 and Stage 2 `StageReward.csv` use `artifact_choice_count=3` on Day5 Midboss, Day10 Midboss and Day11 Boss; normal and inactive elite rows remain zero.
- Runtime binding uses `ArtifactDefinition.DisplayName` -> `ArtifactName`, `Description` -> `Desc`, `Icon` -> `Icon`, and `ArtifactSynergyDefinition.DisplayName` -> `Summary`.
- Both StageReward files were reimported as Unity TextAssets, and focused catalog verification passed all six eligible reward IDs at count three.

### History

- 2026-08-05: Chose existing StageDay/StageReward and Definition contracts; rejected a new artifact-reward CSV as unnecessary.
- 2026-08-05: Code Builder normalized both StageReward files for Boss-only artifact choices and restricted runtime draws to the ten Spirit Contract Definitions.
- 2026-08-05: Designer rechecked the missing-button report against current Stage data and recorded the Day11-only eligibility boundary for user confirmation.
- 2026-08-05: User confirmed Midboss inclusion; Code Builder restored count three on all four Midboss rows and removed the redundant runtime combat-type gate.

## Task: 2026-08-05 Artifact and Synergy Runtime Reuse Design

### Task title

Design first-class artifact/synergy additional-effect Definitions on the existing authoring/runtime pipeline.

### Goals

- Keep the two Effect header CSV contracts under `Artifact/Effect` and reuse the existing passive graph-node/trigger authoring files for concrete effect behavior.
- Make Phase 1 the unparsed authoring of two Effect CSVs plus Spirit King unit and skill rows.
- Route CSV through Parsing, `CsvSourceModel`, Validation and Generation before runtime use.
- Reuse Choice-like Node/Trigger mechanics without converting effects into hidden passives or Choices.
- Limit the first runtime implementation to the ten Spirit Contract artifacts; defer Spirit Contract synergy execution and the Spirit King.
- Load authored artifact icon paths into `ArtifactDefinition.Icon` through the shared Sprite asset catalog.

### Constraints

- Phase 2 owns Loading and Definition code only; prefab, scene, Stage Manager and combat execution remain excluded.
- Artifact effects must generate `ArtifactEffectDefinition` or `ArtifactSynergyEffectDefinition`, not `PassiveSkillDefinition`.
- Do not add artifact-only `effect_nodes.csv` or `effect_triggers.csv`; use existing `skill_graph_nodes_passive.csv`, `passive_skill_triger.csv` and Node definition contracts with `effect_id` ownership.
- Every individual artifact effect uses passive `SkillModifier` or `PassiveTrigger` application; synergy effects may also execute or grant concrete skills.
- Do not invent Tracker details or unsupported Nodes/events.
- Spirit King spawn is `SpawnUnit` effect data and `SummonDefinition`, not a new SkillDefinition family.
- `summon_units.csv` must copy the existing `monsters.csv` columns; `summon_units_skill.csv` must copy the existing `skills_area_attack.csv` columns without speculative metadata columns.
- Do not invent icon paths for artifacts without an existing matching PNG.

### Role Owner

Designer for contract; Code Builder for Phase 1 and Phase 2 implementation.

### Status

Phase 1, Phase 2, the ten-artifact Phase 3 data/runtime scope, and the Spirit Contract/Spirit King runtime wiring are complete in source. Unity Play Mode verification remains.

### Next Actions

- Keep future Effect additions in the existing `skill_graph_nodes_passive.csv` and `passive_skill_triger.csv` owner paths.
- Keep the other 40 artifacts and other synergies deferred.
- User verifies Spirit King skill targeting, Zone visuals and stage behavior in Unity Play Mode.
- Enforce no-duplicate artifact acquisition in the future acquisition flow; Phase 3 does not expand `ArtifactState` for it.

### Evidence

- `Pakuri/reference/4.run/artifact-synergy-runtime-design.md` defines `artifact_effects.csv` and `artifact_synergy_effects.csv` as first-class Definition headers with Node/Trigger owners and Generation-resolved outcome skills.
- The same design maps all 50 authored artifacts and five detailed synergies to `SkillModifier`, `PassiveTrigger`, `ExecuteSkill` or `GrantSkill`, naming existing Nodes and unsupported gaps.
- Existing monster authoring already separates base family CSVs, choices, triggers and graph nodes under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
- `skill_node_definitions.csv` and `skill_node_definition_params.csv` define operation contracts, while `skill_graph_nodes_passive.csv` and `passive_skill_triger.csv` already own concrete Node/Trigger instances.
- `SkillNodeOwnerKind.Effect` now materializes ArtifactEffect-owned graph rows, and artifact-owned Trigger rows validate their effect source without requiring a monster skill.
- Existing Loading code already follows Parsing -> Validation -> Generation -> RuntimeCatalog; artifact source rows and Definitions must join that same path.
- `GameDataCatalog` now indexes Artifact, ArtifactEffect, Synergy and SynergyEffect Definitions; no runtime state or consumer uses them yet.
- `SkillTriggerEvent` has no reload-complete/heal-received event and `SkillTargetSelection` has no densest selector; no Summon runtime kind is required by the revised design.
- Current monster Validation requires A-E active and F-J passive slots, and `MenifestUI` uses `GameDataCatalog.GetMonsters()` as Manifest candidates; Phase 2 therefore generates a separate `SummonDefinition` and `GameDataCatalog.Summons` lookup.
- `authoring/summon/summon_units.csv` now contains the Spirit King row and `authoring/summon/skill/summon_units_skill.csv` contains its five skill rows using the inspected 22/33-column schemas.
- Existing runtime Generation maps `SingleAttack` to `SingleSkillDefinition` and `AreaAttack` to `ZoneSkillDefinition`; existing `RepeatPerTarget` supports Spirit Bombardment's initial cast plus two repeats.
- Phase 1 verification passed strict UTF-8 for all four files, exact 22/33-column reference-header matching, unique IDs, catalog foreign keys and required Spirit King values. Result: 52 artifact-effect rows covering 50 artifacts, 27 synergy-effect rows covering 20 detailed levels, one summon unit and five summon skills.
- Phase 2 added six `CsvRuntimeCatalog` sources, dedicated artifact/synergy/effect/summon source collections, foreign-key and summon-slot/runtime validation, typed Definition generation and RuntimeCatalog lookups.
- `GameDataCatalogBuilder` reuses the existing active-skill generator for `SummonDefinition`; generated Spirit King skills are four `SingleSkillDefinition` and one `ZoneSkillDefinition` without entering `GameDataCatalog.Monsters`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` updated `CsvRuntimeCatalog.asset` with all six source references.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and 2 existing assembly-reference warnings.
- Artifact runtime ownership scripts are organized under `Pakuri/Assets/Scripts/Combat/Artifact`, and `ArtifactDefinitions.cs` is under `Combat/Artifact/Definition`; Loading parser, validator and generator files remain under `Loading`.
- Focused Unity EditMode test `ArtifactAndSummonCatalogBuildsResolvedDefinitions` passed 1/1 and reported 5 monsters, 8+8 enemies, 50 artifacts, 6 synergies and 1 summon. Full `SkillCatalogRuntimeTests` ran 17 tests: 15 passed; two Trigger baseline assertions failed (`ResolvedDefinition` null entries and missing expected Silence status). Phase 2 changed no Trigger/Node source or runtime script; these failures remain a separate verification gap.
- `artifacts.csv` now has a typed `artifact_icon` column: 50 rows preserved, 9 exact filename/ID matches populated, 41 unavailable icons blank, and 0 populated paths missing on disk.
- All 9 referenced PNG `.meta` files use `textureType: 8`; `CsvAssetReferenceCollector`, Generation and `ArtifactDefinition.Icon` reuse the shared Sprite catalog.
- Unity catalog sync serialized 9 matched Artifact Sprite paths; `elemental-prism.Icon` resolves and unmatched `resonance-compass.Icon` remains null.
- The source and foundation catalog now use `정령의 비약` / `spirit-elixir`; Tracker detail remains absent.
- Current CSV evidence identifies exactly ten Spirit Contract artifacts with eight `SkillModifier` effects and two `PassiveTrigger` effects; this is the revised first runtime scope.
- `artifact_effects.csv` now has 62 effect rows; dynamic Prism variants and split conditional/count effects remain independent generated Definitions.
- `artifact_effects.csv` has 64 rows including header/type rows, 9 columns, 2 typed repeat-rule rows and 5 typed Prism selection-rule rows; all rows pass uniform column-count validation.
- `GameDataCatalogBuilder` generates typed Nodes/Reactions on `ArtifactEffectDefinition`; all ten Spirit Contract artifacts now have concrete existing Node/Trigger data.
- Four changed CSVs pass strict parsed column-count checks; focused EditMode tests pass 3/3; solution build completes with 0 errors and the existing 2 warnings.
- Full `SkillCatalogRuntimeTests` result is 19/21 passed; only the previously recorded Trigger baseline assertions remain failing, while every artifact catalog/state/resolver/trigger test passes.
- Focused EditMode verification for the new Definition metadata and manager path passes 4/4; solution build completes with 0 errors, and manager search finds 0 former artifact/synergy ID constant references.
- `summon_units_skill.csv` now authors `spirit-king-dimensional-rift` with `cooldown_seconds=20`, `target_selection=Nearest`, and the Eve-E sprite/controller paths; `artifact_synergies.csv` describes the same nearest-enemy 20-second behavior.
- `CsvRuntimeCatalog.asset` registers the Eve-E Zone sprite path and its inspected asset GUID; the Eve-E controller was already registered. Static CSV/catalog assertions pass and `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` completes with 0 errors and the existing 2 assembly-reference warnings.
- User changed the Spirit Bombardment `3.png` importer to `spriteMode: 1` (Single); `Eve_D.anim` now references exactly three Sprite frames and uses main Sprite fileID `21300000` only for the third frame.
- Spirit Bombardment CSV now uses `shot_interval_seconds=0.1`, doubles `radius` to `5`, `runtime_visual_scale` to `1.2876`, and doubles runtime hitbox size to `10/10`.

### History

- 2026-08-05: Designer inspected current catalogs, source authoring schemas and runtime lookups, then recorded the minimal binding and skill-reuse contract.
- 2026-08-05: User rejected the hidden-runtime-passive model; Designer replaced it with first-class additional-effect Definitions and restricted first implementation to Spirit Contract.
- 2026-08-05: Designer classified all individual artifacts as passive modifier/trigger effects, added concrete Node/path mapping, and changed Phase 1 to authoring both Effect CSVs.
- 2026-08-05: User replaced the Summon-skill plan; Designer added `SpawnUnit`/`spawn_monster_id`, removed Summon Skill Definitions, and retained existing Monster/Zone Definitions.
- 2026-08-05: Designer added Spirit King unit/skill authoring to Phase 1: HP 1000, Physical primary attribute, all defenses 50, four SingleAttack rows, one AreaAttack row, three-cast repeat routing, and the Dimensional Collapse follow-up contract.
- 2026-08-05: Code Builder completed Phase 1 CSV authoring and non-runtime structural validation; no parser, C#, Node/Trigger, prefab or scene was added. Unity auto-import generated four standard `TextScriptImporter` `.meta` files for the authored CSV assets.
- 2026-08-05: Code Builder completed Phase 2 Parsing, SourceModel, Validation, Definition Generation, RuntimeCatalog registration, asset sync and focused EditMode verification using a separate `SummonDefinition`.
- 2026-08-05: Code Builder added `artifact_icon`, mapped the 9 available ID-matched images, wired the field through Parsing/asset collection/Generation, synchronized Unity RuntimeCatalog and passed focused verification.
- 2026-08-05: User moved the first runtime target to the ten Spirit Contract artifacts; Designer moved state/manager skeleton and count-only synergy logging into Phase 3 and deferred all synergy effect execution.
- 2026-08-05: User rejected artifact-only Node/Trigger CSVs; Designer changed Phase 3 to independent ArtifactState ownership plus existing passive graph-node/trigger authoring reuse, without creating `PassiveSkillDefinition` data.
- 2026-08-05: Code Builder completed Effect-owner pipeline integration and authored only the confirmed Spirit Contract modifier nodes, leaving decision-dependent data absent rather than guessed.
- 2026-08-05: Code Builder authored the resolved Prism, Black Candlestick, Spirit Elixir, Rift Gem, Elemental Codex and Resonance Compass data and verified generated Definitions plus dynamic Stage distribution.
- 2026-08-05: Code Builder moved the three artifact runtime state/manager scripts and Unity `.meta` files to `Combat/Artifact`, preserving GUIDs; generated Definition/source-model files were not moved because they belong to the existing Loading pipeline.
- 2026-08-05: Code Builder organized `ArtifactDefinitions.cs` under `Combat/Artifact/Definition` while keeping CSV parser, validator and generator code in `Loading`.
- 2026-08-05: Code Builder added `repeat_rule` and `selection_rule` to the Artifact Effect pipeline, removed manager ID constants, and verified 4/4 focused tests plus 0-error solution build.
- 2026-08-05: Code Builder changed Spirit King Dimension Rift authoring from battlefield-center/999-second to nearest-enemy/20-second, assigned Eve-E Zone sprite/controller resources, synchronized the sprite in `CsvRuntimeCatalog.asset`, and intentionally made no GitHub commit per user instruction.
- 2026-08-06: Code Builder reduced the Spirit Bombardment animation to three `1.png`, `2.png`, `3.png` keys, verified three curve/mapping entries and `git diff --check`, and made no GitHub commit.
- 2026-08-06: Code Builder changed Spirit Bombardment repeat interval to `0.1` seconds and doubled its authored visual/range values; CSV assertions and `git diff --check` passed without a GitHub commit.

## Task: 2026-08-05 Artifact Synergy Foundation CSVs

### Task title

Create the initial artifact synergy and artifact catalogs without runtime parsing.

### Goals

- Create a six-row synergy catalog from `artifact-synergy-list.md`.
- Create an artifact catalog containing every artifact currently detailed by the source document.
- Preserve stable IDs, UI text, fixed 2/4/6/8 thresholds and artifact-to-synergy references.

### Constraints

- Do not add CSV parsing, runtime code or Unity `.meta` files.
- Do not invent the missing Tracker detail section or Tracker artifact list.
- Store both CSV files as UTF-8.

### Role Owner

Code Builder.

### Status

Complete. Foundation CSVs created and structurally verified; runtime parsing remains intentionally absent, and unused `sort_order` columns have been removed.

### Next Actions

- Author Tracker descriptions, four level effects and artifacts in the source document before filling the blank Tracker data.
- Add parsing only through a future explicit implementation request.

### Evidence

- `Pakuri/Assets/CSVdata/Artifact/artifact_synergies.csv` contains six synergy rows with unique IDs and 2/4/6/8 thresholds.
- `Pakuri/Assets/CSVdata/Artifact/artifacts.csv` contains 50 unique artifacts: ten each for Spirit Contract, Executioner, Chosen One, Sentinel and Artillery.
- Strict UTF-8 decoding and PowerShell `Import-Csv` validation passed; all 50 artifact `synergy_id` values reference an existing synergy.
- Tracker summary and common thresholds come from the source summary, while its unavailable detailed description, level effects and artifacts remain blank/absent.
- Neither foundation CSV contains `sort_order`; no Artifact parser or code consumer exists that requires authored ordering metadata.
- The Spirit Contract catalog row now uses `spirit-elixir`, `정령의 비약`, and the revised all-damage/resistance-down description from the source document.

### History

- 2026-08-05: Code Builder created and validated the two non-parsed foundation CSV catalogs from the inspected artifact synergy reference.
- 2026-08-05: Code Builder removed the unused `sort_order` column from both catalogs and revalidated all source text, IDs, references and thresholds.
- 2026-08-05: Code Builder renamed the CSVs to `artifacts.csv` and `artifact_synergies.csv` and moved their existing Unity `.meta` files without changing hashes or GUIDs.
- 2026-08-05: Designer synchronized the requested `정령의 비약` wording and stable English ID into the unparsed foundation catalog.

## Task: 2026-08-03 Remove SingleSkill Internal Delay Data Contract

### Task title

Remove the unused SingleSkill `DamageDelaySeconds` runtime contract while preserving projectile arrival delay data.

### Goals

- Remove `SingleSkillDefinition.DamageDelaySeconds` and `SkillExecutionState.PreparedDamageDelay`.
- Stop copying source delay data into SingleSkill definitions during Generation.
- Keep `ActiveSkillBuildData.DamageDelaySeconds` for projectile arrival generation and CSV validation.

### Constraints

- Preserve `skills_projectile.csv` and the authored `sein-c` value `0.8`.
- Preserve generated `sein-c@arrival` creation and the existing runtime execution route.
- Do not change CSV schema or unrelated loading/UI behavior.

### Role Owner

Code Builder.

### Status

Implementation complete. Static data/code checks passed. Full C# build is currently blocked by 3 out-of-scope UI errors.

### Next Actions

- Unity catalog refresh and Play Mode verification remain user-owned.

### Evidence

- `GameDataCatalogBuilder.Skills.cs` still maps source `DamageDelaySeconds` to Projectile `ArrivalDelaySeconds` and builds `sein-c@arrival` when the value is positive, but no longer assigns it to `SingleSkillDefinition`.
- `SingleSkillDefinition.cs`, `SkillExecutionState.cs` and `SkillExecution.cs` no longer contain the removed SingleSkill delay members.
- `skills_projectile.csv` has the only positive authored `damage_delay_seconds` row: `sein-c`, runtime kind `CooldownProjectile`, value `0.8`.
- `skill_graph_nodes_projectile.csv` keeps `sein-c-trait-4` `DamageDelayMultiplier=0.6`; this continues to modify projectile arrival delay, not SingleSkill internal damage delay.
- `rg` found no remaining `PreparedDamageDelay`, `SingleSkillDefinition.DamageDelaySeconds`, or SingleSkill delayed-application method references.
- The full build errors are limited to modified `MonsterPanelUI.cs:146`, `DebugUI.cs:665`, and `DebugUI.cs:686`; no changed Loading or Combat file produced a reported compiler error.

### History

- 2026-08-03: Code Builder removed the unused SingleSkill delay fields and preserved projectile arrival delay as the separate generated SingleSkill flow.

## Task: 2026-08-03 Sein-C Projectile Arrival SingleSkill Migration

### Task title

Replace the delayed projectile impact-area path with target-point arrival and the existing `SingleSkill` execution path.

### Goals

- Let Sein-C fly to its cast-time target point and preserve collision-triggered trait effects.
- Execute the generated arrival `SingleSkill` after `damage_delay_seconds` at the target point.
- Remove `ProjectileSkillActor`'s direct impact-area target collection and execution path.

### Constraints

- Preserve existing CSV schema, values, authored triggers and unrelated user changes.
- Reuse `TryExecuteReaction` and the existing `SingleSkill` runtime; do not add a second area-damage executor.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete. Static references and `Assembly-CSharp.csproj` build verified.

### Next Actions

- In Unity Play Mode, verify Sein-C collision trait damage, target-point delay, arrival damage, and `sein-c-master-1` OnExpire behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs` now uses `BeginArrivalDelay` and `ExecuteArrivalSkill`; `ApplyImpactAreaTargets`, `ArmImpact`, and the old impact fields are absent.
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Skills.cs` creates a generated arrival `SingleSkillDefinition` from the projectile source data when `damage_delay_seconds > 0`.
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs` stores the cast-time projectile target point and passes arrival data through `SkillExecutionState` and `ProjectileSkillExecutor`.
- `Import-Csv Pakuri/Assets/CSVdata/authoring/monster/skills/base/projectile/skills_projectile.csv` confirms `sein-c`: `radius=1.8`, `damage_delay_seconds=0.8`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal` completed with 0 errors and 2 existing assembly-reference warnings.
- `rg` over Combat skill and Generation scripts found 0 old `ApplyImpactAreaTargets`, `ArmImpact`, `StopOnFirstHit`, `PreparedImpact`, and `impactArmed` references.

### History

- 2026-08-03: Code Builder migrated delayed projectile impact execution to target-point arrival plus generated `SingleSkill`; kept OnHit trigger publication active when base contact damage is disabled.

## Task: 2026-08-03 Monster Skill Icon Asset Copy

### Task title

Create A-E skill icon folders for the five monsters and copy each available `runtime_visual_sprite_path` PNG as `<slot>_Icon.png`.

### Goals

- Create `Pakuri/Assets/Image/Monster/Icon/Skill/<monster>/<A-E>/` for `ariel`, `eve`, `rin`, `sein`, and `vega`.
- Copy the 23 available A-E source PNGs without changing CSV authoring data.
- Keep `rin/D` and `rin/E` folders present while their CSV sprite paths remain empty.

### Constraints

- Copy/rename PNG files only; do not edit skill CSVs or add runtime code.
- Use the exact `runtime_visual_sprite_path` values from `Pakuri/Assets/CSVdata/authoring/monster/skills/base`.
- Do not overwrite an existing destination with different content.

### Role Owner

Code Builder.

### Status

23 of 25 requested icons copied and SHA-256 verified. The two Rin source paths are unavailable because `rin-d` and `rin-e` have empty `runtime_visual_sprite_path` fields.

### Next Actions

- Obtain the intended source PNG paths for `rin-d` and `rin-e`, then copy them to `rin/D/D_Icon.png` and `rin/E/E_Icon.png`.
- Let Unity generate/import child-folder and PNG `.meta` files if they are not created automatically by the editor.

### Evidence

- UTF-8 `Import-Csv` read found 25 A-E rows across six base skill CSVs and five monster IDs: `ariel`, `eve`, `rin`, `sein`, `vega`.
- Validation found 25 slot folders, 23 PNGs and 23 source/destination SHA-256 matches.
- `git status --short -- Pakuri/Assets/CSVdata/authoring/monster/skills/base` returned `NONE`.

### History

- 2026-08-03: Code Builder created the five-monster/A-E folder structure and copied 23 validated PNGs; no CSV files were changed.

## Task: 2026-08-03 Monster and Skill Icon CSV References

### Task title

Populate `MonsterIconImage` and add `SkillIconImage` to the monster skill authoring CSVs so generated runtime data resolves the icon sprites.

### Goals

- Assign all five `MonsterIconImage` values from `Assets/Image/Monster/Icon/Monster`.
- Add `SkillIconImage` plus its `asset_path` type row to all six monster base skill CSVs.
- Populate A-E skill rows from the existing `Icon/Skill` PNGs and include the generated paths in the runtime catalog.

### Constraints

- Preserve the existing CSV row values and use only verified project asset paths.
- Keep F-J passive `SkillIconImage` cells empty because no F-J icon folders/assets exist in the inspected workspace.
- Reuse the existing `SkillRow.SkillIconPath`, `GameDataCatalogBuilder.LoadSprite`, `SkillDefinition.Icon`, and asset collector flow; do not add duplicate runtime icon fields.

### Role Owner

Code Builder.

### Status

Complete. Monster and active-skill icon paths are authored, parsed and generated into `CsvRuntimeCatalog.asset`.

### Next Actions

- If passive F-J icons are required later, create those assets first, then populate their currently empty `SkillIconImage` cells.

### Evidence

- `monsters.csv` now maps `ariel`, `eve`, `rin`, `sein`, and `vega` to five existing `Monster/*_Icon.png` files.
- Six base skill CSV headers now contain `SkillIconImage`; all 25 A-E rows contain existing icon paths. Passive F-J rows remain empty by design.
- `CsvRowParser.ParseSkillRow` now reads `SkillIconImage` and falls back to legacy `skill_icon_path`; existing Generation already loads the field into `SkillDefinition.Icon`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and 2 pre-existing assembly-reference warnings.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` completed; `CsvRuntimeCatalog.asset` contains five monster icon paths and 25 active skill icon paths. Unity console reported a runtime catalog with 5 monsters, 8 stage-one enemies and 8 stage-two enemies.
- All 25 target PNG `.meta` files were verified with `textureType: 8` (Sprite).

### History

- 2026-08-03: Code Builder added the CSV field, parser compatibility, generated runtime catalog references, and ran compile/path verification without adding a new Definition type.

## Task: 2026-07-31 Resolved Skill Outcome Materialization

### Task title

Materialize Trigger skill outcomes as concrete family Definitions during Generation.

### Goals

- Keep `SkillCastEffect` as a small resolved execution link instead of a second raw payload model.
- Generate Single, Zone and Buff Definitions once, then route learned and generated outcomes through the common runtime path.
- Preserve authored CSV values, targeting, visual, status, shield, timing and recast metadata.

### Constraints

- Do not change the CSV schema or add a runtime kind, Executor, Actor base class, catalog lookup layer or new Implementation script.
- Keep cooldown refund, reload reduction and status-duration extension as typed non-spatial commands.
- Auxiliary generated Definitions must not enter `UnitSkills` learned active/passive lists.
- Use the existing Generation builders, `UnitSkills.FindByDefinition`, `SkillExecution` and family Executors.

### Role Owner

Code Builder for implementation; Code Reviewer ran once after implementation by explicit user request.

### Status

Complete. Phase records: `05e5b22`, `22e8516`, `3075a5d`, `55ca337`, `dfa7d53`; implementation `5213b14`; recast guard fix `b7037d1`.

### Next Actions

- User performs Unity Play Mode and gameplay parity verification.
- Reopen this task only if runtime evidence identifies a data-generation regression.

### Evidence

- `GameDataCatalogBuilder.Nodes.cs` now writes concrete Single/Zone/Buff Definition references into `SkillCastEffect`.
- `SkillReaction.TargetSkillId` and raw `SkillCastEffect` damage/status/shield/targeting payload fields have no runtime readers.
- Core and Editor project builds both ended with `빌드했습니다.`; static legacy-contract and direct-damage boundary searches returned no output.
- Unity EditMode batchmode was blocked because another Unity instance had this project open.

### History

- 2026-07-31: Code Builder completed Generation materialization, common execution routing, Actor hit ownership and raw contract cleanup with per-Phase commits.
- 2026-07-31: Code Reviewer found the resolved Recast path did not enforce `MaxGeneration`; Code Builder restored the guard in `b7037d1`.

## Task: 2026-07-28 Skill Trigger / Node Data Contract Design

### Task title

Replace kind-branched graph authoring with Trigger-owned and owner-keyed Nodes.

### Goals

- Remove `graph_kind` from all six `skill_graph_nodes_*.csv`.
- Add no replacement grouping column or intermediate grouping type.
- Move Trigger payload fields into Trigger-owned Node data while Trigger rows retain activation rules.

### Constraints

- Role Owner is Designer for the handoff and Code Builder refactoring track for later implementation.
- Preserve all current IDs, values, asset paths, ordering, gates, and generated catalog behavior during migration.
- Keep the legacy graph reader until converted CSV and runtime parity pass.
- No active CSV was changed in this design task.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Superseded by the 2026-07-29 Trigger Final Outcome Generation Design.

### Next Actions

- No action. The preserved historical handoff is under `boards/ARCHIVE/`.
- Current work follows `boards/COMBAT/SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`.

### Evidence

- `SkillGraphParser` currently branches graph kinds and rewrites Effect rows to generated Effect ownership.
- `GameDataCatalogBuilder` separately materializes ordinary Nodes and Effect definitions.
- Active graph authoring contains 508 Effect rows and 256 ordinary modifier rows.
- The handoff removes `graph_kind`, rejects replacement grouping IDs, defines owner-keyed Nodes, expands Trigger events, and specifies parser/validator/catalog/compiler migration.

### History

- 2026-07-28: User requested removal of `graph_kind`, rejection of the intermediate grouping term, and Trigger-based Node activation.
- 2026-07-28: Designer recorded the replacement data contract without editing CSV or runtime catalogs.
- 2026-07-28: Code Builder archived older DATA task history and retained this as the only active DATA task.
- 2026-07-29: User superseded runtime Trigger Node dispatch with final outcome generation and existing Executor reuse.

## Task: 2026-07-29 Trigger Visual Duration Data Repair

### Task title

Restore explicit one-second lifetime Nodes for standalone Trigger visuals.

### Goals

- Repair ten Trigger-owned Node collections whose `ShowVisual` rows had no `SetDuration`.
- Keep visual lifetime explicit in authoring data.

### Constraints

- No runtime fallback and no validator change.
- Preserve the 19-column Node CSV contract and contiguous owner-local `node_order`.
- Preserve all existing values and add only the missing duration rows.

### Role Owner

Code Builder

### Status

Complete except user-owned Play Mode verification.

### Next Actions

- User verifies one-second visual removal for representative OnExpire, OnHit, OnKill, OnOutgoingDamage, OnShieldExpire, and last-projectile events.

### Evidence

- Five graph CSV files received ten total `SetDuration=1` rows; the line-attack graph required no change.
- All six graph files retain a 19-column width for every header and row.
- Each repaired owner has exactly one positive duration Node, and the standalone non-positive Trigger visual count is zero.
- Unity CSV source validation completed without errors and the runtime catalog loaded 5/8/8 definitions.

### History

- 2026-07-29: User required explicit data duration and prohibited a runtime zero-duration fallback.
- 2026-07-29: Code Builder restored the ten missing lifetime Nodes from reference intent and pre-migration one-second behavior.

## Task: 2026-07-29 CSV Loading Pipeline Responsibility Refactor

### Task title

Reorganize CSV loading into one ordered pipeline with four responsibility folders.

### Goals

- Implement the approved Parsing, Validation, Generation, and RuntimeCatalog structure.
- Keep one parsed `SourceModel`, one semantic validation pass, one catalog build, and one lookup rebuild.
- Remove duplicate ownership and implicit static builder dependencies.

### Constraints

- Preserve current CSV, serialized asset, runtime catalog, public API, ordering, and gameplay behavior.
- Preserve existing `.meta` GUIDs and the runtime Resources path.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and non-Play-Mode verification complete.

### Next Actions

- User verifies representative gameplay flows in Unity Play Mode.

### Evidence

- The approved handoff records current file ownership, target paths, stage contracts, the single-validation rule, and compatibility gates.
- Baseline runtime and editor C# builds completed with zero errors before implementation.
- Loading now has explicit Parsing, Validation, Generation, and RuntimeCatalog folders; combat skill compilation moved to `Combat/Skills/Compilation`.
- Static search found one semantic-validation call, one catalog-build call, and one lookup-rebuild call in the ordered loader path.
- Static search found zero references to the removed `runtimeCsvCatalog` loader state.
- All moved scripts retain their original GUIDs, and all new scripts have `.meta` files.
- `Assembly-CSharp.csproj` built with zero errors; Unity compiled without project errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.

### History

- 2026-07-29: User selected Code Builder, required the handoff MD first, and authorized implementation from that MD.
- 2026-07-29: User prohibited unnecessary duplicate structure and repeat validation of an already validated source model.
- 2026-07-29: Code Builder completed the handoff implementation and all available non-Play-Mode checks.

## Task: 2026-07-29 Ponytail Loading Pipeline Simplification

### Task title

Delete dead CSV-loading code and merge duplicate lookup and handler ownership.

### Goals

- Keep the Parsing -> Validation -> Generation -> RuntimeCatalog pipeline behavior.
- Delete unused parser, DTO, validator, builder, and skill-handler metadata.
- Merge runtime lookup storage into `GameDataCatalog`.

### Constraints

- Ponytail leads the implementation; existing markdown is reference material only.
- Preserve active CSV contracts, serialized fields, public lookup APIs, and gameplay behavior.
- Preserve unrelated pre-existing working-tree changes.

### Role Owner

Code Builder, ponytail-led

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative CSV loading and skill execution in Unity Play Mode.

### Evidence

- `Loading` changed from 13 C# files and 7,084 lines to 12 C# files and 5,718 lines: net reduction 1,366 lines.
- `GameDataLookup.cs` and its `.meta` were removed; lookup registration and queries now live in `GameDataCatalog.cs`.
- Static search found zero remaining removed-symbol or block-comment matches and retained the single ordered validation, build, and lookup-rebuild calls.
- Every remaining Loading C# file has a `.meta` file.
- Runtime and Editor `dotnet build` checks completed with zero errors; the Unity EditMode test passed 1/1.
- Unity finished script compilation idle and ready with zero `Assets/Scripts/Loading` console errors; one separate MCP package transport error was present.

### History

- 2026-07-29: User assigned Code Builder and required ponytail-led deletion, consolidation, and a final net-line-reduction report.
- 2026-07-29: Code Builder removed dead data and helpers, deleted duplicate handler metadata, merged lookup ownership, and completed static, build, EditMode, and Unity console checks.

## Task: 2026-07-29 Final Skill Catalog Generation Design

### Task title

Make Loading Generation produce final typed skill data once.

### Goals

- Make `GameDataCatalogBuilder` directly create final active, passive, Choice, Trigger, and Node data.
- Parse Node and Trigger enum/list/condition authoring strings into final typed values exactly once in Generation.
- Make `GameDataCatalog` index final data instead of Source Definition wrappers.
- Prevent repeated validation, Definition compilation, Trigger compilation, and Choice Node parsing.

### Constraints

- Keep the existing Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Keep exactly one semantic validation, one build, and one lookup rebuild.
- Preserve CSV schemas, values, IDs, ordering, asset paths, and runtime behavior.
- Avoid duplicate handler-support lists between Validator and Builder.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder implementation and available non-Play-Mode verification complete. Phases 1-6 complete.

### Next Actions

- User verifies representative CSV-loaded active, passive, enhancement, master, Trigger, and enemy skill behavior in Unity Play Mode.

### Evidence

- `GameDataLoader.BuildValidatedRuntimeCatalog` currently calls validation, catalog build, and lookup rebuild once each.
- `GameDataCatalogBuilder` currently stops at Source Definition and string-param Node Definition creation.
- Combat compiler scripts perform a second static conversion during unit state rebuild or first Choice use.
- `SkillNodeExecutor` and `SkillTrigger` still parse authored scope, policy, condition, status, runtime-kind, Choice, attribute, event-skill, and event-source values during execution.
- Final Loading and Combat contracts are specified in `boards/COMBAT/SKILL_DIRECT_CATALOG_RUNTIME_HANDOFF.md`.
- Phase 1 baseline `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` builds completed with zero errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; the EditMode test job succeeded.
- Phase 2 added the final typed contracts that Generation will populate directly: final Choice Nodes, Node target IDs, typed status conditions, typed Trigger lists, and event source scope.
- Phase 3 Generation now produces and indexes final active, passive, enemy, Choice, Trigger, and Node data once.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies through the final catalog path.
- Phase 4 runtime consumers use final Choice Nodes, typed Trigger arrays/scope, and final SkillDefinition lookup values directly.
- Runtime Execution/Trigger/StatusRules search found zero authored `Split`, `Enum.Parse`, or `TryParse` calls.
- Runtime and Editor builds completed with zero errors; Unity CSV validation retained 5 monsters and 8/8 enemies.
- Phase 5 integrated final skill/Choice/Node creation into `GameDataCatalogBuilder` partials and removed all compiler/mapper symbols.
- `UnitSkills` owns learned-ID/Choice application; `StatusRuntimeCompiler.CompileTriggers` was deleted as dead code.
- All 18 moved script GUID pairs matched; Runtime/Editor builds and Unity CSV validation passed.
- Phase 6 confined temporary build contracts to Loading/Generation and removed duplicate public Source/Definition contracts from Combat.
- Removed-symbol, runtime parsing, and Generation-outside Definition-mutation searches all returned zero.
- EditMode target-filter/reference-reuse tests passed 2/2; solution build and Unity script compilation completed with zero errors; CSV validation retained 5/8/8.
- Whole-task C# diff is 909 additions and 1,069 deletions: net reduction 160 lines.

### History

- 2026-07-29: User approved direct use of final authored skill data and requested a Code Builder-ready design.
- 2026-07-29: Designer recorded the cross-domain Loading/Combat handoff without changing runtime code or CSV.
- 2026-07-29: Designer updated the Generation contract so encoded authoring strings are converted once and final runtime consumers receive enum/array values.
- 2026-07-29: Code Builder completed Phase 1 baseline protection before changing the final data contracts.
- 2026-07-29: Code Builder completed Phase 2 final typed contracts with the current compiler retained only as an intermediate compatibility path.
- 2026-07-29: Code Builder completed Phase 3 final catalog generation and final-type RuntimeCatalog indexing.
- 2026-07-29: Code Builder completed Phase 4 final catalog direct runtime consumption.
- 2026-07-29: Code Builder completed Phase 5 Generation ownership integration and Combat skill folder migration.
- 2026-07-29: Code Builder completed Phase 6 temporary-contract cleanup and full non-Play-Mode verification.

## Task: 2026-07-29 Trigger Final Outcome Generation Design

### Task title

Generate final triggered skill Definitions or typed state commands once.

### Goals

- Convert Trigger-owned authored Nodes into final concrete `SkillDefinition` references or typed non-skill commands in Generation.
- Stop building runtime `SkillNode[]` payloads for Trigger execution.
- Reuse existing catalog Definitions for the four current `ExecuteSkill` mappings.
- Preserve Choice/base modifier Node generation.

### Constraints

- Keep the current Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Do not add a new CSV schema or C# script unless inspected code proves it necessary.
- Preserve IDs, values, ordering, asset paths, dynamic event-value semantics, and one validation/build/lookup flow.
- Do not silently activate the 81 current modifier-only owners without owner-level evidence.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder Phase 1-6 complete. Final catalog and non-Play-Mode verification passed.

### Next Actions

- User verifies representative Trigger behavior in Play Mode.
- Run Code Reviewer only after separate explicit user approval.

### Evidence

- No Trigger owner currently combines two delivery kinds.
- No Trigger owner currently combines delivery and a non-skill state command.
- Current delivery shapes map to existing Single, Zone, Buff, and Shield Definition families.
- Event-derived damage uses shield-applied, shield-absorbed, or event-applied damage snapshots.
- The full Generation mapping and validation rules are recorded in the handoff.
- Phase 1 confirmed 158 Trigger rows and 606 Trigger-owned Node rows with runtime/editor build error 0.
- Phase 2 generated 55 final Definitions and 22 typed commands while retaining 81 current no-action owners.
- Unity EditMode catalog verification passed 1/1 and both C# builds completed with error 0.
- Phase 3 consumes 55 final Definitions through the existing family dispatch without catalog registration of hidden Definitions.
- Runtime/editor builds completed with error 0; `SkillCatalogRuntimeTests` passed 3/3.
- Phase 4 removes `SkillTriggerDefinition.Nodes`; Generation now stores only the final Definition or typed command on each Trigger.
- The generated 22 commands are verified as recast 1, cooldown 14, reload 6, and status-duration 1; Unity EditMode tests pass 3/3.
- Phase 5 deletes the runtime Node executor and Trigger-only public operation/mapping contracts; status mutation assembly remains private to Generation.
- Runtime/editor builds remain error 0 and final-outcome catalog tests pass 3/3 after deletion.
- Final static searches return zero deleted symbol, Trigger runtime Node, and runtime consumer authored-parse hits.
- Solution build error 0, Unity Console error/warning 0, full EditMode 3/3, CSV validation catalog 5/8/8.
- Git C# diff from the Phase 1 baseline is net -968 lines in `Combat/Skills` and net -443 lines across production `Assets/Scripts`.

### History

- 2026-07-29: User selected existing Executor reuse instead of runtime Trigger Node effect dispatch.
- 2026-07-29: Designer recorded the corresponding final Generation outcome contract.
- 2026-07-29: User approved Code Builder implementation; Phase 1 fixed the current owner and build baseline.
- 2026-07-29: Code Builder completed Phase 2 final outcome Generation and focused catalog verification.
- 2026-07-29: Code Builder completed Phase 3 runtime consumption with source snapshot, lifecycle, target, and dynamic-value policies.
- 2026-07-29: Code Builder completed Phase 4 typed command consumption and removed the runtime Trigger Node payload.
- 2026-07-29: Code Builder completed Phase 5 Trigger executor/operation deletion and confined remaining authored mutation assembly to Generation.
- 2026-07-29: Code Builder completed Phase 6 final static/build/Unity/CSV verification.

## Task: 2026-07-29 Final Status Catalog Generation

### Task title

Generate final status runtime data once and index it in `GameDataCatalog`.

### Goals

- Keep status authoring parsing in `Loading/Parsing`.
- Build each `StatusRuntimeData` from its validated `StatusEffectDefinition` in Generation.
- Index final status runtime data by `StatusEffectKind` during `GameDataCatalog.RebuildLookup`.
- Remove Combat-side status compilation and lookup helpers.

### Constraints

- Preserve the existing Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Keep one validation, one catalog build, and one lookup rebuild.
- Preserve CSV schema and values.
- Reuse existing `StatusRuntimeData`, `StatusEffectDefinition`, and `GameDataCatalog` types without a replacement compiler layer.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative CSV-authored status behavior in Unity Play Mode.

### Evidence

- `GameDataCatalogBuilder.BuildStatusEffects` now assigns one generated `RuntimeData` value to every status definition.
- Skill and Trigger Generation clone the generated status template before applying owner-specific overrides.
- `GameDataCatalog.GetStatusRuntimeData(StatusEffectKind)` returns the indexed final runtime reference.
- `StatusValueParser` is internal and all of its callers are under `Loading`.
- `StatusRuntimeCompiler` and `StatusEffectLookup` searches return zero references.
- EditMode verification passes 4/4 and asserts every status definition owns non-null generated runtime data reused by RuntimeCatalog.
- Solution build completes with zero errors; final Unity Console contains zero errors/warnings.

### History

- 2026-07-29: User approved aligning status data flow with the final skill catalog structure.
- 2026-07-29: Code Builder moved parse-only functions to `Loading/Parsing/StatusValueParser.cs` and absorbed runtime-data construction into Generation.
- 2026-07-29: Code Builder completed RuntimeCatalog indexing, Combat direct use, and non-Play-Mode verification.

## Task: 2026-07-30 Enemy Passive Shared Data Contract

### Task title

Generate and register Enemy passives as shared `PassiveSkillDefinition` data.

### Goals

- Replace `EnemyPassiveDefinition` and `EnemyPassiveModifierKind` with the shared passive definition contract.
- Preserve the existing Enemy passive CSV shape, IDs, values, and attribute rules.
- Register generated Enemy passives in the common passive lookup.

### Constraints

- Keep the existing five-column Enemy passive CSV and all 16 authored rows.
- Preserve one semantic validation, one catalog build, and one lookup rebuild.
- Do not alter Monster passive CSV or reward contracts.
- Store the edited CSV as UTF-8.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative Enemy passive behavior in Play Mode.

### Evidence

- Enemy passive CSV type metadata now names shared `PassiveModifierKind`; all authored rows and values are unchanged.
- `CsvRowParser` and `CsvDataValidator` parse and validate the shared enum while retaining existing attribute and positive-value rules.
- `GameDataCatalogBuilder` creates `PassiveSkillDefinition` directly for Enemy passive rows.
- `GameDataCatalog.RegisterEnemies` registers each generated Enemy passive in the common passive lookup.
- Unity catalog verification loads 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- `EnemyCatalogBuildsSharedLearnedPassives` verifies all 16 Enemy passives are generated, registered, learned, and rebuilt through the common runtime.
- Full Unity EditMode tests pass 9/9; Runtime and Editor C# builds complete with zero errors.

### History

- 2026-07-30: User approved replacing the separate Enemy passive data/runtime with shared learned passives assigned at spawn.
- 2026-07-30: Code Builder migrated parsing, validation, generation, registration, runtime initialization, and verification.

## Task: 2026-07-30 Skill Definition Family Consolidation

### Task title

Generate final Single and Buff family Definitions without compatibility subclasses.

### Goals

- Generate Chain follow-up as a final hidden Trigger `SingleSkillDefinition`.
- Generate Status, Heal, Shield, and Charge as one final `BuffSkillDefinition`.
- Keep current CSV schemas and authored tuning values.
- Remove obsolete subclass-specific Generation branches and writes.

### Constraints

- Follow `boards/COMBAT/SKILL_DEFINITION_FAMILY_CONSOLIDATION_HANDOFF.md`.
- Preserve one validation, one catalog build, and one lookup rebuild.
- Keep existing Chain fields as the tuning source; do not add duplicate Trigger CSV columns.
- Preserve all current IDs, values, paths, and ordering.

### Role Owner

Code Builder

### Status

Code implementation and non-Play-Mode verification complete.

### Next Actions

- User Play Mode verification of generated skill behavior.

### Evidence

- `ChainLightning` already carries multiplier, delay, radius, and primary-exclusion values in the base skill CSV.
- `OpeningCharge` is already authored as Buff and already has an explicit CombatStart Trigger.
- Generation currently creates the four subclasses that this task removes.
- Generation now creates only final family Definitions; searches return zero removed subclass symbols.
- ChainLightning uses its existing CSV chain values to generate a hidden Trigger Single without schema changes.
- OpeningCharge, Heal, Shield, and Status profiles generate one `BuffSkillDefinition` distinguished by `BuffEffectKind`.
- No CSV file or schema was changed.
- Focused generated-family test passed 1/1; full Unity EditMode tests passed 10/10.

### History

- 2026-07-30: User approved final family Generation and requested implementation after a written handoff.
- 2026-07-30: Code Builder completed final family Generation and catalog verification.

## Task: 2026-07-31 Trigger Reaction Generation Consolidation Design

### Task title

Generate Trigger conditions as existing skill/passive/Choice reaction Nodes instead of runtime Trigger Definitions.

### Goals

- Keep current Trigger CSV and Trigger-owned graph CSV as the first migration authoring source.
- Attach generated reaction conditions and execution-data adjustments to existing Skill, Passive, and Choice Node ownership.
- Stop generating hidden Trigger SkillDefinitions after the common runtime path is verified.

### Constraints

- Add no authoring schema or C# script in the first migration.
- Preserve the single Parsing -> Validation -> Generation -> RuntimeCatalog flow.
- Preserve IDs, values, ordering, asset paths, and current working outcomes.
- Restore the approved 17 event outcomes and 64 normal cast outcomes without mixing their ownership.

### Role Owner

Code Builder.

### Status

User approved Generation/runtime implementation. Phases 1-8 complete. Code Reviewer corrections 1-4 are implemented without schema changes and final PASS.

### Next Actions

- Re-run Code Reviewer and correct findings until approval.

### Evidence

- Full design: `boards/COMBAT/SKILL_TRIGGER_REACTION_LOGIC_CONSOLIDATION_HANDOFF.md`.
- Current `GameDataCatalogBuilder` builds `SkillTriggerDefinition`, hidden direct-delivery Definitions, one RecastZone command, and a hidden ChainLightning Definition.
- Current authoring contains 158 Trigger rows and 606 Trigger-owned graph Nodes.
- Current runtime outcomes are 55 skill deliveries and 22 typed commands including one Zone recast; 81 owners have no runtime outcome.
- Trigger CSV and graph Node contracts already contain the required event, condition, targeting, value-source, timing, and visual data, so the first migration needs no new schema.
- Semantic audit found 65 working Trigger reactions, 17 event-driven rows with no final outcome, and 76 rows that belong to ordinary Skill/Choice/Passive execution.
- The 76 non-Trigger rows are 75 OnCast rows plus `vega-b-master1-second-slash`, a same-source follow-up.
- The technical no-outcome 81 split into 64 OnCast modifiers and 17 incomplete event reactions; Generation must not treat those groups the same.
- `SkillCatalogRuntimeTests.TriggerSemanticClassificationBaselineIsStable` fixes the Generation result classification at `65/17/76`.
- Phase 1 solution build completed with error 0; Unity focused EditMode test passed 1/1 and loaded catalog 5/8/8.
- Phase 2 changed no CSV, Parsing, Validation, or Generation contract; runtime-kind family verification passed 13/13.
- Phase 3 changed no CSV, Parsing, Validation, or Generation contract; existing-skill reactions now pass the learned target runtime and Definition directly into the common reaction entry point.
- Phase 3 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 4 Generation excludes all 76 semantic non-Triggers from final Trigger arrays, attaches 74 normal cast/passive payloads to existing Nodes, and maps `ariel-e-trait-4` to the existing conditional-damage Choice handler.
- Phase 4 retains the current Trigger CSV schema; the `ariel-e-trait-4` graph owner/handler/value is corrected from separate Trigger damage to Choice `ConditionalDamageMultiplier(holy-exposure, 1, 1.5)`.
- Phase 4 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 5 Generation converts 40 direct-delivery outcomes to common effect payloads and materializes the 17 previously incomplete `StatusModifier` outcomes without changing CSV schema.
- Phase 5 final runtime counts are effect 57, learned-skill reference 4, command 21, and missing outcome 0.
- Phase 5 solution build completed with error 0; Unity full EditMode tests passed 14/14.
- Phase 6 final catalog has 48 passive source reactions: effect 24, learned-skill reference 4, and command 20; all have outcomes.
- Phase 6 uses the existing cooldown refund 14 and reload reduction 6 commands without schema changes.
- Phase 6 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 7 Generation no longer creates the `ChainLightning__chain` SkillDefinition and maps RecastZone node delay into the Trigger scheduler.
- Phase 7 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 8 Generation emits `SkillReactionOp` into existing Skill/Choice/Passive Nodes and no longer emits runtime Trigger owner arrays or hidden Trigger Definitions.
- `SkillTriggerDefinition` C# references are zero; solution build error 0, Unity Console error 0, and full EditMode tests passed 15/15.
- Reviewer correction 1 reuses the existing `ExecuteSkill` Node parameters to encode Vega B's `vega-b` 0.45 follow-up; no node definition or CSV schema was added.
- Final normal cast/passive payload count is 73 after excluding two duplicated event payload rows and mapping `ariel-e-trait-4` to its Choice modifier.
- Reviewer correction 2 changes only runtime execution policy for the generated Vega follow-up and does not alter Parsing, validation, or schema.
- Reviewer correction 3 changes only runtime reaction multiplier composition and does not alter CSV parsing, validation, Generation, or schema.
- Solution build completed with error 0; Unity EditMode tests passed 16/16.
- Reviewer correction 4 removes only an unused runtime catalog lookup; CSV parsing, validation, Generation, and schema remain unchanged.
- Code Reviewer final PASS confirms no data-contract change; C# obsolete Trigger symbol search is 0 and EditMode `TestResults.xml` is 16/16 passed.

### History

- 2026-07-31: User required integration rather than moving the old runtime class to another script.
- 2026-07-31: Designer recorded a Generation migration that retains the current authoring source while removing the final runtime Trigger Definition and hidden skill output.
- 2026-07-31: User clarified the semantic boundary using Ariel-B trait 4 versus traits 1~3 and 5.
- 2026-07-31: Designer corrected the Generation plan so ordinary cast/modifier rows do not become Trigger reactions.
- 2026-07-31: User approved restoration of both the 17 event outcomes and 64 normal cast outcomes and assigned Code Builder.
- 2026-07-31: Code Builder completed Phase 1 semantic catalog baseline verification.
- 2026-07-31: Code Builder completed Phase 2 without changing the data contract.
- 2026-07-31: Code Builder completed Phase 3 existing-skill runtime reuse without changing the authoring schema.
- 2026-07-31: Code Builder completed Phase 4 final ownership separation without changing the authoring schema.
- 2026-07-31: Code Builder completed Phase 5 direct-delivery and incomplete event-outcome Generation.
- 2026-07-31: Code Builder completed Phase 6 final passive-source and state-command count verification.
- 2026-07-31: Code Builder completed Phase 7 Zone/Chain Generation consolidation.
- 2026-07-31: Code Builder completed Phase 8 obsolete Trigger contract deletion without changing Parsing or authoring schemas.
- 2026-07-31: Code Builder applied Reviewer correction 1 with one existing graph handler value change and no Parsing/schema changes.
- 2026-07-31: Code Builder applied Reviewer correction 2 without data-contract changes.
- 2026-07-31: Code Builder applied Reviewer correction 3 without data-contract changes; reaction multiplier now composes multiplicatively with existing skill modifiers.
- 2026-07-31: Code Builder applied Reviewer correction 4 without data-contract changes; removed unused catalog access from `SkillTrigger`.
- 2026-07-31: Code Reviewer completed final PASS; data/CSV path remains unchanged.

## Task: 2026-08-02 Enemy Slash SingleAttack CSV Migration

### Task title

Move `Slash` and `FireDragonSlash` from the enemy AreaAttack authoring table to the SingleAttack authoring table.

### Goals

- Remove both rows from `skills_area_attack.csv`.
- Add both rows to `skills_single_attack.csv` with `runtime_kind=SingleAttack` and the SingleAttack column layout.
- Keep their damage, targeting, cooldown, visual and hitbox values while converting `DamageArea` to `Damage`.

### Constraints

- Change only the two enemy skill CSV files.
- Do not add a new runtime kind, parser field, builder branch or combat implementation.
- Do not claim Unity catalog/runtime validation; only static CSV validation was run in this task.

### Role Owner

Code Builder.

### Status

CSV migration is corrected on disk and static schema checks pass; Unity TextAsset reimport/runtime sync is pending.

### Next Actions

- In Unity, run `Pakuri/Sync CSV Runtime Catalog Assets`, then `Pakuri/Validate CSV Source Data`.
- If the same 48-column error remains, reimport the enemy `skills_single_attack.csv` TextAsset or restart the Unity Editor before validating again.

### Evidence

- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/area_attack/skills_area_attack.csv` now contains only its header and type row; `Slash` and `FireDragonSlash` are absent.
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/single_attack/skills_single_attack.csv` contains both rows with 49 columns, `SingleAttack`, `Damage`, and `charge_ramp_seconds=3` / `charge_move_speed_multiplier=2.5`.
- Direct UTF-8 disk read reports header=49 columns and `Slash` row 6=49 columns; Unity previously reported an imported row 6 with 48 columns.
- `git diff --check` completed without whitespace errors.
- `GameDataCatalogBuilder.Skills.cs` maps `DamageArea` to `SingleSkillDefinition`; the migrated rows now use the explicit `Damage` profile.

### History

- 2026-08-02: Code Builder moved and converted the two enemy rows; no C# files were changed.
- 2026-08-02: Unity reported the pre-correction 48-column imported row; the disk CSV was rechecked at 49 columns and Unity reimport was left as the next verification step.

## Task: 2026-07-31 Reaction Outcome Definition Materialization

### Task title

Materialize skill-like reaction payloads as concrete family Definitions for the common cast path.

### Goals

- Preserve current Trigger and graph CSV schema while changing Generation output from raw runtime effect payloads to resolved Single, Zone or Buff Definition links.
- Reuse existing learned Definitions when a reaction executes an existing skill.
- Keep cooldown, reload and status-duration changes as typed non-skill commands and convert `RecastZone` to a Zone skill outcome.

### Constraints

- Preserve IDs, values, targeting, timing, source attribution, visuals, dynamic event-value policies and current outcome count parity.
- Do not register auxiliary outcome Definitions in learned active/passive slots or add a runtime kind, Executor, Actor or C# script.
- Do not enable raw effect and generated Definition outcomes simultaneously.
- Keep Parsing and authored CSV unchanged unless Phase 10 evidence proves a required value has no existing source.

### Role Owner

Designer for data-contract handoff; Code Builder for Generation and Editor-test migration.

### Status

Design pending implementation. The current completed baseline remains effect 57, learned-skill reference 4, command 21, missing 0 and must be reverified in Phase 10.

### Next Actions

- Inventory every raw effect field and map it to its concrete family Definition field before deleting runtime payload fields.
- Inventory additional-damage and hit-chain Node payloads currently consumed by `ApplyHitEnhancements`; materialize Definitions only after their proc/count semantics are fixed by tests.
- Change Editor tests to verify final Definition family/reference and typed command parity instead of the current `Effect`/`TargetSkillId`/`Command` shape.
- Record each Phase commit and build/EditMode result here and in the primary COMBAT handoff.

### Evidence

- `GameDataCatalogBuilder.Nodes.cs` currently creates raw `SkillCastEffect` values for damage, status and shield outcomes.
- The same builder creates `RecastZone`, `RefundCooldown`, `ReduceReload` and `ExtendStatusDuration` commands.
- `SkillCatalogRuntimeTests.cs` directly asserts raw effect fields, command kinds, RecastZone values and outcome-kind counts.
- `SkillExecution.TryExecuteReaction` currently requires learned runtime data, so raw effects without learned runtimes cannot be migrated by `TargetSkillId` lookup alone.
- Trigger CSV inspection found 37 active-skill reactions with zero non-default proc/count/internal-cooldown rows and 126 passive reactions with 13 non-default rows; Phase 10 must fix this as the gate-migration baseline.

### History

- 2026-07-31: User approved normal-cast-path reuse for conditional skills.
- 2026-07-31: Designer selected Generation-resolved Definition references to avoid runtime payload interpretation and runtime catalog lookup, while retaining typed non-spatial command exceptions.

## Task: 2026-08-03 Monster and Enemy Image CSV Runtime Wiring

### Task title

Load distinct monster Standing and enemy display Sprites from CSV Image paths.

### Goals

- Keep `MonsterIconImage` for monster icons.
- Add `Image` to `monsters.csv` for five Standing Sprites.
- Add `Image` to `enemies.csv` for all 16 current enemy Sprites.
- Parse, validate, generate and serialize both Image path sets through the existing runtime catalog.

### Constraints

- Use only inspected asset paths and prefab Sprite GUID mappings.
- Preserve the existing CSV-to-`CsvRuntimeCatalog` pipeline.
- Do not remove `MonsterIconImage` or add a duplicate icon field.

### Role Owner

Code Builder

### Status

Implemented. CSV asset validation, Unity catalog sync, scene validation and solution build passed; Unity EditMode suite has two unrelated existing Trigger test failures.

### Next Actions

- User verifies Standing images in `PrisonPanel` and `MenifestedSuccessPopUp`, and enemy images in `PrisonPanel/Prisonal/Image` during Play Mode.

### Evidence

- `monsters.csv` contains five `Image` paths under `Assets/Image/Monster/*/Standing` and preserves `MonsterIconImage`.
- `enemies.csv` contains 16 `Image` paths matched to current Stage1/Stage2 prefab `m_Sprite` GUIDs and `.meta` files.
- `CsvRowParser`, `MonsterDefinition`, `EnemyDefinition`, `GameDataCatalogBuilder` and `CsvAssetReferenceCollector` now carry the new paths.
- Static verification reported `monster_rows=5`, `monster_images=5`, `bad_monster=0`, `enemy_rows=16`, `enemy_images=16`, `bad_enemy=0`.
- Unity sync completed and catalog load reported 5 monsters, 8 stage-one enemies and 8 stage-two enemies.
- `dotnet build Pakuri/Pakuri.sln --no-restore`: 0 errors, 2 existing assembly-reference warnings.

### History

- 2026-08-03: Code Builder added distinct monster/enemy Image CSV fields, runtime wiring, UI consumers and removed old serialized monster portrait/Karin Sprite references.

## Task: 2026-08-03 Stage Flow CSV Split

### Task title

Organize Stage Encounter and Reward CSV data under separate `Stage1` and `Stage2` folders.

### Goals

- Create `stage_flow/Stage1/StageEncounter.csv` and `StageReward.csv`.
- Create `stage_flow/Stage2/StageEncounter.csv` and `StageReward.csv`.
- Keep StageManager runtime loading all four files in one StageFlowTable.

### Constraints

- Preserve every existing Encounter and Reward row and column value.
- Keep `StageDay.csv` at the stage_flow root because it was not part of the requested split.
- Do not overwrite unrelated user changes in prefabs, scene UI, combat scripts, or prior task files.
- Store CSV files as UTF-8.

### Role Owner

Code Builder

### Status

Implemented and statically verified; Unity asset reimport and Play Mode remain user-owned.

### Next Actions

- In Unity, allow the new folders/CSV TextAssets to import, then validate `NewRunScene` stage 1 and stage 2 day progression.

### Evidence

- Exact normalized split comparison returned `encounter_exact_split=True` for 60 Encounter rows and `reward_exact_split=True` for 9 Reward rows.
- Static CSV validation found Stage1 Encounter 30 rows, Stage2 Encounter 30 rows, Stage1 Reward 5 rows, and Stage2 Reward 4 rows; all Encounter files have 14 columns and all Reward files have 13 columns.
- `StageManager.cs` now has four serialized stage CSV references and `StageFlowTable.Load` loads both stage file pairs.
- `NewRunScene.unity` assigns the four new TextAsset GUIDs to `stage1EncounterCsv`, `stage1RewardCsv`, `stage2EncounterCsv`, and `stage2RewardCsv`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; the existing two Unity reference-conflict warnings remain.

### History

- 2026-08-03: Code Builder split the two stage-flow tables by stage, updated Inspector references and runtime loading, and removed the duplicate root CSV files after exact data comparison.

## Task: 2026-08-03 Skill Reaction IsTrigger Runtime Contract

### Task title

후속 반응 스킬을 공통 실행 경로에서 식별할 `SkillReaction.IsTrigger` 계약을 추가한다.

### Goals

- 반응 정의가 실행 스냅샷에 `IsTrigger`를 전달하도록 한다.
- 반응으로 생성된 스킬이 다시 사건 반응을 발행하지 않도록 Combat 실행 경로와 연결한다.

### Constraints

- CSV 열과 스키마를 추가하지 않는다.
- 모든 반응은 기존 `GameDataCatalogBuilder`가 생성하는 런타임 `SkillReaction` 객체의 기본값을 사용한다.
- 일반 스킬 정의 자체의 실행 경로와 기존 반복·지연 값은 변경하지 않는다.

### Role Owner

Code Builder.

### Status

Implementation complete. 런타임 생성 경로와 Assembly-CSharp 빌드를 확인했다.

### Next Actions

- Unity Play Mode에서 CSV 런타임 카탈로그 생성 후 반응 객체의 `IsTrigger` 기본값이 적용되는지 확인한다.

### Evidence

- `GameDataCatalogBuilder.BuildRuntimeCatalog`가 매번 `SkillReaction` 객체를 생성하며, `SkillNodeConditions.SkillReaction.IsTrigger`의 기본값은 `true`다.
- 일반 시전 효과 변환은 `SkillReaction`을 임시로 사용한 뒤 `SkillCastEffect` 노드만 반환하므로 `IsTrigger` 실행 태그를 직접 사용하지 않는다.
- `SkillExecution.ExecuteReactionOutcome`가 반응의 `IsTrigger`를 `TryExecuteResolvedEffect`로 전달한다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore`는 오류 0개, 기존 Unity 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-03: Code Builder가 기존 사건 연쇄 상태 타입을 제거하는 공통 실행 계약으로 `SkillReaction.IsTrigger`를 추가했다.

## Task: 2026-08-03 Restore NewRunScene Stage CSV Inspector References

### Task title

Reconnect the four Stage1/Stage2 Encounter and Reward TextAssets on `NewRunScene.StageManager`.

### Goals

- Remove the `{fileID: 0}` serialized references that stop `StageManager.LoadTables()`.
- Preserve the existing StageFlowTable loading code and CSV files.

### Constraints

- Change only the four StageManager CSV fields in `NewRunScene.unity`.
- Use the actual GUIDs from the four CSV `.meta` files.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and statically verified.

### Next Actions

- User reloads `NewRunScene` and verifies StageManager starts and enemy spawning proceeds in Play Mode.

### Evidence

- `NewRunScene.unity` now assigns `stage1EncounterCsv`, `stage1RewardCsv`, `stage2EncounterCsv`, and `stage2RewardCsv` with `fileID: 4900000` and the matching CSV GUIDs.
- The four GUIDs resolve to `CSVdata/stage_flow/Stage1/StageEncounter.csv`, `Stage1/StageReward.csv`, `Stage2/StageEncounter.csv`, and `Stage2/StageReward.csv`.
- `git diff --check -- Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` returned no whitespace errors.

### History

- 2026-08-03: Code Builder restored the four missing StageManager TextAsset Inspector references.

## Task: 2026-08-03 Stage Runtime Catalog Migration

### Task title

Move Stage CSV parsing into the Loading runtime catalog.

### Goals

- Build one `StageDefinition` from the five Stage CSV sources in Loading.
- Let `StageManager` consume `GameDataLoader.CurrentCatalog.Stage` instead of parsing CSV directly.
- Preserve the existing Stage day, encounter, reward, boss, and prisoner values.

### Constraints

- Reuse the existing `CsvParser` and ordered Loading pipeline.
- Keep the current Stage CSV paths and runtime Resources catalog.
- Do not change unrelated Combat or UI behavior.

### Role Owner

Code Builder

### Status

Implemented and compiled. Play Mode verification remains user-owned.

### Next Actions

- User verifies Stage 1/2 day progression, enemy spawning, rewards, and boss selection in Play Mode.

### Evidence

- `Assets/Scripts/Loading/RuntimeCatalog/StageDefinition.cs` defines Stage day, encounter, and reward runtime models.
- `Assets/Scripts/Loading/Generation/StageDefinitionBuilder.cs` parses the five stage TextAssets through `CsvParser.CsvTable.Load`.
- `GameDataCatalogBuilder` assigns the model to `GameDataCatalog.Stage`; `GameDataLoader` requires all five Stage source references.
- The five active Stage CSVs now contain header, type, and data rows; UTF-8 `Import-Csv` checks report 23/31/6/31/5 data rows with matching 10/14/13/14/13 columns.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder added the Stage runtime model and builder, connected the Loading catalog, normalized Stage CSV type rows, and removed StageManager's local CSV table parser.

## Task: 2026-08-03 Remove Critical Resistance Authoring Schema

### Task title

Remove critical-resistance columns from active unit/status authoring CSVs and align Generation with the existing critical-chance bonus contract.

### Goals

- Remove `base_crit_resistance`, `crit_resistance` and `critical_resistance_bonus_per_stack` from current authoring schemas.
- Remove their parser/model/generation mappings.
- Change the Vega conditional trait row to `AllAllies` plus `StatusCriticalChanceBonus` `0.10`.

### Constraints

- Do not change the current CSV loading architecture or add a replacement resistance column.
- Preserve all non-resistance unit defenses and status values, including `vulnerable` critical-damage-taken `0.03`.
- Leave `Assets/Legacy` historical source files untouched.

### Role Owner

Code Builder.

### Status

Implemented and statically verified; Unity runtime catalog synchronization is pending because Unity Editor processes are active.

### Next Actions

- Sync/reimport the current authoring CSV TextAssets in Unity, then validate the generated catalog and Vega trait behavior.

### Evidence

- `CsvRowParser.cs`, `CsvSourceModel.cs`, `GameDataCatalogBuilder.cs` and `GameDataCatalogBuilder.Skills.cs` no longer read or map critical-resistance fields.
- `skill_node_definition_params.csv` and `skill_node_definitions.csv` no longer define `StatusCriticalResistanceBonus`; current passive CSV uses `StatusCriticalChanceBonus` for Vega G trait 3.
- Current CSV checks report matching imported field counts: monsters 22 columns/5 data rows, enemies 24 columns/16 data rows, status effects 19 columns/18 data rows.
- Active authoring/script search for `CriticalResistance`, `CriticalResistanceBonus`, `StatusCriticalResistanceBonus`, `crit_resistance`, `base_crit_resistance` and `critical_resistance_bonus_per_stack` returned no results.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: User approved moving the design toward `CritChanceBonus` and removing the target critical-resistance stat.
- 2026-08-03: Code Builder removed the active authoring/runtime schema fields and preserved only the existing attacker-side critical-chance bonus mechanism.

## Task: 2026-08-03 AreaAttack Authoring Unification

### Task title

Use `AreaAttack` for the `sein-d` area skill and remove `Field` from the active skill data contract.

### Goals

- Change the `sein-d` `runtime_kind` value from `Field` to `AreaAttack`.
- Ensure the authoring loader accepts only `AreaAttack` for the area-attack CSV.
- Remove the obsolete `Field` enum value and all active generation/execution/test references.

### Constraints

- Preserve every other `sein-d` CSV value and keep the existing area-attack CSV layout.
- Do not add a replacement runtime kind or alter zone damage/timing behavior.
- Do not modify historical `Assets/Legacy` data.

### Role Owner

Code Builder.

### Status

Implemented and statically verified; Unity TextAsset reimport/runtime catalog sync remains pending.

### Next Actions

- Reimport/sync the authoring CSV in Unity and validate the generated catalog after the Editor refresh.

### Evidence

- `skills_area_attack.csv:5` has `runtime_kind=AreaAttack`; CSV import reports all three rows with the expected 33 properties.
- `CsvSourceLoader` area base/choice loaders now pass only `SkillRuntimeKind.AreaAttack` as the allowed runtime kind.
- Removing `Field` from the enum makes the generic `CsvDataValidator` enum parsing reject the obsolete value automatically; no explicit Field validator branch remains.
- Active `SkillRuntimeKind.Field` and exact `"Field"` searches returned no results.
- Solution build completed with 0 errors and 2 existing assembly-reference warnings.

### History

- 2026-08-03: User requested that the `Field` skill kind and its validation rules be removed after AreaAttack unification.
- 2026-08-03: Code Builder migrated `sein-d` and removed the obsolete data-contract references.

## Task: 2026-08-03 Shorten Skill Graph Owner IDs

### Task title

Store only the owner suffix in `skill_graph_nodes_*.csv` and reconstruct the full owner ID from `monster_id` during parsing.

### Goals

- Convert values such as `eve-c-trait-1` to `c-trait-1` in the graph authoring CSVs.
- Keep the parsed `SkillGraphNodeRow.OwnerId` canonical as `eve-c-trait-1`.
- Avoid changing existing choice, trigger, skill and target ID tables.

### Constraints

- Apply the transformation to the six active `skill_graph_nodes_*.csv` files under `monster/skills/choices` only.
- Preserve all node order, node types, arguments, target skill IDs and exclusion IDs.
- Use `monster_id + "-" + owner_id`; existing repository IDs use hyphens.

### Role Owner

Code Builder.

### Status

Implemented and statically verified; Unity TextAsset reimport/runtime catalog validation is pending.

### Next Actions

- Reimport the changed graph CSV TextAssets in Unity and run the existing CSV validation/catalog load.

### Evidence

- The six graph CSV files changed 858 duplicated prefixes; the post-transform import contains 858 rows with no `owner_id` still beginning with its `monster_id-` prefix.
- `SkillGraphParser.cs` reads `monster_id` and `owner_id` separately, then canonicalizes the owner before `ValidateSkillNodeOwner`, `ResolveSkillGraphTargetSkillId` and `MaterializeSkillGraphRows` consume it.
- The normalization is idempotent, so canonical owner IDs remain accepted during migration.
- `git diff --check` passed and the full solution build completed with 0 errors and 2 existing assembly-reference warnings.

### History

- 2026-08-03: User approved the short `owner_id` authoring format and parser reconstruction approach.
- 2026-08-03: Code Builder applied the CSV transformation and canonicalized graph owner IDs at the parser boundary.

## Task: Repair CSV Header/Type Column Counts After Schema Cleanup

### Goals

- Keep the current authoring CSV schema loadable after the reported `monsters.csv` header/type count failure.
- Record the exact data-file changes and static validation evidence for the runtime catalog input.

### Constraints

- Change only the type rows in `monsters.csv`, `enemies.csv`, and `status_effects.csv`.
- Preserve CSV data rows and leave the parser implementation unchanged.

### Role Owner

Code Builder.

### Status

Completed and statically verified; Unity Editor auto-sync/reimport remains pending.

### Next Actions

- Let Unity reimport the changed TextAssets and verify the runtime catalog synchronization in the Editor.

### Evidence

- Header/type counts are aligned at 22/22 (`monsters.csv`), 24/24 (`enemies.csv`), and 19/19 (`status_effects.csv`).
- A quote-aware scan of all 39 current authoring CSV files and all nonblank data rows returned `bad=0`.
- `git diff --check` passed; `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: The reported Unity error identified a type-row count mismatch at `CsvParser.cs:122`.
- 2026-08-03: Code Builder repaired the three current authoring CSV type rows without changing data values or parser code.

## Task: 2026-08-06 Passive StatusModifier Stage Lifetime Contract

### Task title

Replace the obsolete 0.5-second passive `StatusModifier` authoring convention with the existing Stage-permanent duration contract.

### Goals

- Preserve explicit effect durations such as the 12-second `eve-f` shield.
- Author Stage-long passive modifiers with the existing 9999 permanent sentinel.
- Keep conditional passive modifiers present for the Stage while runtime calculations decide whether their condition currently applies.

### Constraints

- Change only passive `OnCast` `StatusModifier` duration rows proven by the trigger/graph join.
- Do not change `ApplyShield`, damage, heal, recast, or other explicitly timed effects.
- Keep CSV UTF-8 and preserve row/column shape, IDs, node order, values, targets, and conditions.
- Do not add a periodic refresh interval or a new schema column.

### Role Owner

Code Builder.

### Status

Implementation and focused EditMode verification complete; Unity Play Mode gameplay verification remains user-owned.

### Next Actions

- Reimport is complete in the connected Unity Editor; verify the Offering-to-next-Stage Eve-F flow in Play Mode.

### Evidence

- Trigger/graph join found 58 passive `OnCast` `StatusModifier` effects: 25 `AllAllies`, 33 `Enemy`, 11 unconditional, and 47 conditional.
- All 58 currently use `SetDuration(0.5)`.
- `GameDataCatalogBuilder.Nodes.BuildNormalStatusModifierEffect` maps durations of at least 9999 to `StatusRuntimeData.Permanent=true`.
- `StatusRuntimeInstance.Tick` does not decrement permanent statuses, while `MonsterDayRecovery.ResetTransient` clears the complete status collection between Stages.
- The passive graph CSV diff contains exactly 58 `0.5` removals and 58 `9999` additions; parsed verification reports `PASSIVE_0_5=0`.
- Parsed verification reports one unchanged `eve-f@effect1` `SetDuration(12)` row.
- No schema, node ID, node order, target, condition, or non-duration value changed in the passive graph.
- `PassiveStageModifiersPreserveLifetimeAndDynamicConditions` loaded the refreshed runtime catalog and passed 1/1, asserting exactly 58 permanent 9999 passive modifiers and the unchanged 12-second Eve-F shield.
- The full 25-test EditMode run still reports two separate trigger-semantic failures; neither failure assertion is the passive lifetime regression added here.

### History

- 2026-08-06: User selected explicit authored durations for timed effects and the existing Stage-permanent contract for Stage-long passive effects.
- 2026-08-06: Code Builder recorded the exact affected-row inventory before changing CSV data.
- 2026-08-06: Phase 3 replaced only the 58 verified Stage-long passive modifier durations with 9999 and retained explicit timed effects.
- 2026-08-06: Phase 4 refreshed the Unity CSV TextAsset, passed the focused lifetime regression, and recorded remaining suite failures without changing unrelated trigger data.
