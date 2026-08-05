# 유물·시너지 추가 효과 구현 설계

## 1. 문서 상태

- 역할: Designer / Code Builder
- 상태: 정령계약 시너지·정령왕 구현 착수. Phase 1·2·3의 데이터/로딩/유물 효과 기반은 완료
- Phase 1 범위: 두 Effect CSV와 정령왕 유닛·스킬 CSV 작성
- 첫 runtime 구현 범위: `ArtifactState`, `SynergyState`, `ArtifactSynergyManager` 뼈대와 정령계약 소속 유물 10개
- 첫 runtime 시너지 범위: 파티 전체 시너지 개수 계산과 Stage당 1회 로그만 수행하며 시너지 효과는 실행하지 않음
- 이번 구현 범위: 정령계약 시너지 2/4/6/8단계와 정령왕
- 후속 runtime 구현 범위: 나머지 유물, 처형관·선택받은자·파수꾼·포격대·추적자
- Phase 1 제외 범위: C#, Parsing, Node/Trigger, Prefab, Scene 생성

현재 저장소에는 Artifact/Synergy/Summon Definition과 Loading 경로, `ArtifactState`, `SynergyState`, `ArtifactSynergyManager`, Stage 준비 연결, Artifact Effect Node/Reaction runtime 소비 경로가 있다. `UnitRole.Summon`과 정령왕 runtime은 아직 없다.

## 2. 승인된 방향

유물·시너지 효과는 숨은 스킬이나 숨은 런타임 패시브가 아니다. 각각 독립된 유물 추가 효과 Definition이다.

기존 강화·마스터와 공유하는 것은 효과의 정체가 아니라 동작 방식이다.

- 강화·마스터: `SkillChoice`가 `SkillNode`를 통해 기존 스킬 snapshot을 변경
- 유물 효과: `ArtifactEffectDefinition`이 같은 Node 해석 경로를 통해 대상 스킬 snapshot을 변경
- 시너지 효과: `ArtifactSynergyEffectDefinition`이 같은 Node/Trigger 경로를 사용하거나 Generation에서 확정된 기존 계열 `SkillDefinition`을 실행

모든 개별 유물 효과는 수동 시전 없는 패시브 적용이다. 여기서 패시브는 발동 방식이며 `PassiveSkillDefinition`을 뜻하지 않는다.

- `SkillModifier`: 보유 중 항상 또는 조건부로 기존 스킬 snapshot을 Node로 변경
- `PassiveTrigger`: 전투 사건을 감시한 뒤 기존 Trigger 경로로 결과 Skill/Status를 실행

시너지 효과는 위 두 경로 외에 실제 스킬 실행을 허용한다.

- `ExecuteSkill`: Stage 시작 또는 Trigger 결과로 concrete `SkillDefinition` 실행
- `GrantSkill`: 선택 유닛이나 소환물의 `SkillState`에 Generation 확정 스킬 부여
- `SpawnUnit`: Stage 시작 시 Generation 확정 소환 몬스터 Definition을 기존 `UnitSpawnManager` 경로로 생성

금지:

- 유물 효과를 `PassiveSkillDefinition`으로 위장
- 유물 효과 ID를 `UnitSkills.LearnedPassiveSkillIds`에 삽입
- 유물 효과를 강화·마스터 Choice ID로 저장
- `ArtifactSynergyManager`가 직접 피해·치명타·방어막 수식을 구현
- 기존 `SkillDefinition` 원본을 Stage마다 직접 변경

허용:

- 기존 Node operation과 `SkillExecutionRules` 재사용
- 기존 `SkillReaction` 조건·예약·실행 경로 재사용
- Generation에서 스킬 결과를 기존 Single/Area/Buff 계열 Definition 참조로 확정
- 소환 효과는 `SummonSkillDefinition` 없이 `SpawnUnit` effect가 기존 유닛 생성 경로를 호출
- `ArtifactState`, `SynergyState`, `ArtifactSynergyManager`가 유물 전용 Definition을 선택하고 공통 실행 경로에 전달

## 3. 구현 범위

### 3.1 Phase 1: Effect·정령왕 CSV 작성

Phase 1에서는 Parsing에 연결하지 않고 다음 네 기초 데이터를 먼저 작성했다.

1. `Artifact/Effect/artifact_effects.csv`: 원문에 상세가 있는 개별 유물 50개의 패시브 추가 효과 헤더
2. `Artifact/Effect/artifact_synergy_effects.csv`: 원문에 단계 효과가 있는 정령계약·처형관·선택받은자·파수꾼·포격대의 추가 효과 헤더
3. `authoring/summon/summon_units.csv`: `monsters.csv`와 같은 열 형식의 정령왕 유닛 행
4. `authoring/summon/skill/summon_units_skill.csv`: `skills_area_attack.csv`와 같은 열 형식의 정령왕 스킬 5개 행

한 설명이 독립 적용 두 개를 포함하면 행을 나눈다. 예: 정령계약 2단계는 `정령왕 소환`과 `원소폭발 해금`을 별도 effect로 둔다. 추적자는 상세 단계와 유물 원문이 없으므로 Phase 1 행을 만들지 않는다.

### 3.2 첫 runtime 구현

Phase 2에서 모든 Artifact·Synergy·Summon Loading과 Definition 생성은 완료됐다. 첫 runtime 구현은 정령계약 **시너지 효과**가 아니라 정령계약에 속한 **개별 유물 10개**부터 진행한다.

1. 각 `RunSession.RunMonsterState`에 최대 3개 유물을 소유하는 `ArtifactState` 연결
2. `UnitCombatState`가 `UnitSkills`와 같은 방식으로 Run의 `ArtifactState` 참조 공유
3. `ArtifactSynergyManager.PrepareStage`에서 파티 전체 유물 효과 적용 목록과 `SynergyState` 개수 재구성
4. `StageManager`가 적 생성 전에 Stage당 한 번 `PrepareStage` 호출
5. `SynergyState`는 시너지별 보유 개수만 계산하고 한 줄 로그를 남김
6. 각 `ArtifactState`는 영구 보유 ID와 Stage 한정 활성 Effect ID를 분리하고, Manager가 `recipient_scope`에 따라 활성 Effect를 배포
7. 정령계약 유물 10개의 `SkillModifier` 8개와 `PassiveTrigger` 2개를 기존 Node·Trigger 경로에 연결
8. `SkillModifier`는 `SkillExecutionRules`가 활성 Effect의 Node를 최종 snapshot에 합성하고, `PassiveTrigger`는 `SkillTrigger`가 활성 Effect의 Reaction을 기존 gate/scheduler로 전달
9. `ArtifactSynergyEffectDefinition` 순회, 정령왕 소환, 단계별 스킬 해금은 실행하지 않음

첫 대상은 `elemental-prism`, `ember-crown`, `frost-lens`, `storm-capacitor`, `radiant-chalice`, `black-candlestick`, `spirit-elixir`, `rift-gem`, `elemental-codex`, `resonance-compass`다.

### 3.3 후속 runtime 구현

- 정령계약 시너지 2/4/6/8단계와 정령왕
- 정령계약 외 개별 유물 40개
- 처형관, 선택받은자, 파수꾼, 포격대 시너지
- 상세 원문 작성 뒤 추적자
- 아래 매핑에서 `신규 필요`로 표시한 공통 Node, Trigger event, 조건 resolver

## 4. 데이터 파일 구조

```text
Pakuri/Assets/CSVdata/
├─ Artifact/
│  ├─ artifacts.csv
│  ├─ artifact_synergies.csv
│  └─ Effect/
│     ├─ artifact_effects.csv
│     └─ artifact_synergy_effects.csv
├─ authoring/monster/skills/
│  ├─ choices/passive/skill_graph_nodes_passive.csv
│  ├─ triggers/passive/passive_skill_triger.csv
│  └─ nodes/definitions/
│     ├─ skill_node_definitions.csv
│     └─ skill_node_definition_params.csv
└─ authoring/summon/
   ├─ summon_units.csv
   └─ skill/
      └─ summon_units_skill.csv
```

1차에 필요하지 않은 Single/Buff/Projectile 파일과 빈 폴더는 만들지 않는다.

`artifacts.csv`의 `artifact_icon`은 `asset_path`이며, 실제 PNG가 있는 행만 `Assets/Image/Artifact/<artifact_id>.png`를 기록한다. Generation은 이를 `ArtifactDefinition.Icon`으로 해석한다.

### 4.1 `artifact_effects.csv`

유물 고유 추가 효과의 독립 Definition 헤더다.

```csv
"effect_id","artifact_id","application_mode","recipient_scope","repeat_rule","selection_rule","recipient_monster_id","target_skill_id","outcome_skill_id"
"id","id","enum:ArtifactEffectApplicationMode","enum:ArtifactEffectRecipient","enum:ArtifactEffectRepeatRule","enum:ArtifactEffectSelectionRule","string","skill_id","skill_id"
```

| 열 | 소유 의미 |
|---|---|
| `effect_id` | `ArtifactEffectDefinition` 고유 ID |
| `artifact_id` | `ArtifactDefinition` 외래 키 |
| `application_mode` | 개별 유물은 `SkillModifier` 또는 `PassiveTrigger`만 허용 |
| `recipient_scope` | 적용 대상 범위 |
| `repeat_rule` | 반복 적용 횟수 계산 방식. `None`, 시너지 보유 유물 수, 서로 다른 대표 속성 수를 Definition에 전달 |
| `selection_rule` | 효과 변형 선택 방식. `PartyDominantAttribute`는 원소 프리즘의 파티 대표 속성 결과만 활성화 |
| `recipient_monster_id` | 특정 캐릭터 효과일 때 대상 Monster ID |
| `target_skill_id` | 기존 스킬 snapshot 보정 대상, 없으면 빈 값 |
| `outcome_skill_id` | Trigger/Stage 효과가 실행할 Generation 확정 Skill Definition, 없으면 빈 값 |

Phase 1에 원문이 있는 50개 유물의 행을 작성한다. 실제 Node/Trigger 행과 Parsing 연결은 후속 Phase다.

### 4.2 `artifact_synergy_effects.csv`

시너지 단계 추가 효과의 독립 Definition 헤더다.

```csv
"effect_id","synergy_level_id","application_mode","recipient_scope","recipient_monster_id","target_skill_id","outcome_skill_id","spawn_monster_id"
"id","id","enum:ArtifactEffectApplicationMode","enum:ArtifactEffectRecipient","string","skill_id","skill_id","summoned_monster_id"
```

| 열 | 소유 의미 |
|---|---|
| `effect_id` | `ArtifactSynergyEffectDefinition` 고유 ID |
| `synergy_level_id` | `ArtifactSynergyLevelDefinition` 외래 키 |
| `application_mode` | `SkillModifier`, `PassiveTrigger`, `ExecuteSkill`, `GrantSkill`, `SpawnUnit` 중 하나 |
| `recipient_scope` | Stage, 파티 전체, 특정 캐릭터, 선택받은자, 소환물 등 |
| `recipient_monster_id` | 특정 캐릭터일 때만 사용 |
| `target_skill_id` | 기존 스킬 보정 대상 |
| `outcome_skill_id` | 실행하거나 해금할 Generation 확정 Skill Definition |
| `spawn_monster_id` | `SpawnUnit`일 때 생성할 소환 몬스터 Definition ID |

한 단계에 결과가 여러 개면 행을 나눈다. 예: 정령계약 2개 단계는 “정령왕 소환”과 “원소폭발 해금”을 서로 다른 `effect_id`로 둔다. Phase 1에는 상세 원문이 있는 다섯 시너지의 행만 작성한다.

### 4.3 `summon_units.csv`

Phase 1에서 별도 구현 정보 열을 추가하지 않고 `authoring/monster/monsters.csv`의 22개 열을 그대로 사용해 정령왕 1행을 작성했다.

```csv
"id","display_name","role_summary","element_label","primary_attribute","max_health","power_stat","base_damage","power_coefficient","base_attack_power","base_spell_power","base_move_speed","base_crit_chance","base_crit_damage","def_physical","def_fire","def_lightning","def_ice","def_darkness","def_holy","MonsterIconImage","Image"
"id","string","string","string","enum:DamageAttribute","float","float","float","float","float","float","float","float","float","float","float","float","float","float","float","asset_path","asset_path"
"spirit-king","정령왕","정령계약으로 소환되어 천천히 이동하며 단계별 원소 스킬을 자동 시전하는 임시 아군.","물리","Physical","1000","100","0","0","60","100","0.5","0.05","1.5","50","50","50","50","50","50","",""
```

- 사용자 확정값: `max_health=1000`, `primary_attribute=Physical`, 여섯 방어력 모두 `50`
- 기본 공격을 주지 않으므로 `base_damage=0`, `power_coefficient=0`
- 정령왕의 고정 이동 속도를 위해 `base_move_speed=0.5`
- 저장소에 정령왕 아이콘·Prefab 경로가 없으므로 두 asset 열은 빈 값이며 경로를 발명하지 않는다.
- 이 행은 일반 `monsters.csv`에 섞지 않는다. 같은 source 열을 읽어 `SummonDefinition`을 생성하고 소환 전용 lookup에 둔다.

### 4.4 `summon_units_skill.csv`

Phase 1에서 `authoring/monster/skills/base/area_attack/skills_area_attack.csv`의 33개 열을 그대로 사용해 정령왕 스킬 5행을 작성했다. `monster_id`에는 소환 유닛 ID인 `spirit-king`을 기록한다.

| `skill_id` | 슬롯 | 표시명 | `runtime_kind` | 피해/반경 | 쿨다운·지속·간격 | 용도 |
|---|---|---|---|---|---|---|
| `spirit-king-elemental-explosion` | A | 원소폭발 | `SingleAttack` | 80 / 2.5 | 5 / 0 / 0 | 밀집 지점 1곳에 배치되는 `SingleSkillDefinition` |
| `spirit-king-elemental-storm` | B | 원소폭풍 | `AreaAttack` | 틱당 18 / 3.5 | 10 / 6 / 0.5 | 6초 유지되는 `ZoneSkillDefinition` |
| `spirit-king-spirit-bombardment` | C | 정령 폭격 | `SingleAttack` | 회당 70 / 2.5 | 8 / 0 / 0.35 | 최초 1회와 반복 2회를 합쳐 총 3회 실행 |
| `spirit-king-dimensional-rift` | D | 차원붕괴 | `AreaAttack` | 0 / 4.5 | 999 / 1.2 / 0.1 | 전투당 1회, 중앙 균열이 1.2초 동안 적을 중심으로 끌어당기는 `ZoneSkillDefinition` |
| `spirit-king-dimensional-collapse-explosion` | E | 차원붕괴 폭발 | `SingleAttack` | 240 / 4.5 | 999 / 0 / 0 | 균열 종료 결과로만 실행하며 자동 해금하지 않는 후속 스킬 |

공통 작성값:

- `attribute=Physical`: 정령왕 기본 속성의 authoring fallback이다. 파티 대표 속성 resolver가 구현되면 실행 직전 준비 피해 속성만 대표 속성으로 바꾼다.
- `spell_power_coefficient=0`, `attack_power_coefficient=0`, `critical_allowed=false`: 원문의 고정 피해 80/18/70/240을 보존한다.
- `hit_target_count`는 빈 값, 탄창·재장전 열은 `0`, 상태 효과 열은 빈 값/`0`으로 둔다.
- `runtime_visual_scale=1`, `runtime_visual_sorting_order=0`; `runtime_hitbox_size_x/y`는 스킬 순서대로 `5/5`, `7/7`, `5/5`, `9/9`, `9/9`로 작성한다.
- 아직 정령왕 스킬 Prefab·sprite·animator·icon이 없으므로 모든 asset 열은 빈 값이다.
- 정령 폭격은 기존 `RepeatPerTarget` Node에 `repeat_count=2`, `repeat_interval_seconds=0.35`, `repeat_damage_multiplier=1`을 연결한다. 최초 실행까지 포함해 총 3회다.
- 차원붕괴는 기존 Zone lifecycle에 `PullToCenter(distance_per_tick=0.2)`를 연결하고, 중앙에서 데미지 0으로 1.2초 유지한다. Zone의 기존 `OnExpire -> ExecuteSkill(spirit-king-dimensional-collapse-explosion)` 경로로 종료 폭발을 실행한다.
- 정령 폭격은 기존 `RepeatPerTarget`에 최초 시전을 포함해 2회 반복을 연결하며, 반복마다 현재 적 위치를 다시 `Densest`로 계산한다.
- 원소폭발·원소폭풍·정령 폭격의 대상 중심은 `Densest`를 사용한다. 후보 적 위치별 반경 내 적 수가 최대인 위치를 선택하고, 동률이면 시전자와 가까운 위치, 다시 동률이면 Registry 순서를 따른다.
- `skills_area_attack.csv` 형식에 없는 선택·반복·후속 실행은 기존 graph-node/trigger CSV와 typed runtime operation으로 연결한다. 새로운 Effect 전용 CSV는 만들지 않는다.

### 4.5 기존 Node와 Trigger 저작 경로

유물 전용 `effect_nodes.csv`와 `effect_triggers.csv`는 만들지 않는다. Node 정의 CSV는 operation 종류와 인자 계약만 소유하고 실제 효과 인스턴스는 기존 패시브 graph-node/trigger CSV에 작성한다.

- `skill_graph_nodes_passive.csv`: `owner_kind=Effect`, `owner_id=<effect_id>`로 실제 Node와 인자를 작성
- `passive_skill_triger.csv`: `source_skill_id=<effect_id>`로 실제 Trigger 사건과 gate를 작성
- `skill_node_definitions.csv`, `skill_node_definition_params.csv`: 기존 Node type/parameter 계약을 그대로 사용
- 현재 `SkillNodeOwnerKind.Effect`는 존재하지만 graph materialization이 완성되지 않았으므로 Phase 3에서 기존 Effect owner 경로만 완성
- artifact Effect는 monster 소유 스킬이 아니므로 Parser/Validation은 Effect owner에 `monster_id`와 learned passive 소유권을 강제하지 않음

새로운 범용 `effect_kind`, JSON payload, 문자열 수식 열은 만들지 않는다. 기존 Node로 표현되지 않는 동작만 새 typed Node/Trigger event로 추가한다.

`application_mode`는 효과 계산 타입이 아니라 어느 기존 경로로 보낼지 정하는 최소 routing 값이다. Manager 안에 switch식 피해/스탯 계산을 만들지 않는다.

`sort_order`는 UI 순서가 아니라 실행 순서가 필요한 Node/Trigger에만 둔다. 두 Effect Definition 헤더 CSV에는 넣지 않는다.

## 5. CSV에서 Definition까지

기존 Loading 구조와 같은 단방향 흐름을 사용한다.

```text
CSV
  -> Parsing
  -> CsvSourceModel
  -> Validation
  -> Generation
  -> Artifact/ArtifactSynergy/Effect + Summon/기존 Skill Definition
  -> RuntimeCatalog
  -> ArtifactState / SynergyState / ArtifactSynergyManager
```

### 5.1 Parsing

`CsvSourceLoader`가 다음 source row를 `CsvSourceModel`에 넣는다.

- Artifact row
- Artifact synergy row와 level row
- Artifact effect row
- Artifact synergy effect row
- 기존 graph node row의 `Effect` owner
- 기존 passive trigger row의 artifact `effect_id` source
- `authoring/summon/summon_units.csv` row
- `authoring/summon/skill/summon_units_skill.csv` row

Parsing은 문자열과 열 구조만 읽는다. 런타임 객체를 만들거나 효과를 적용하지 않는다.

### 5.2 Validation

Generation 전에 다음을 실패 처리한다.

- 중복 `artifact_id`, `synergy_id`, `synergy_level_id`, `effect_id`
- 존재하지 않는 artifact/synergy level 외래 키
- 존재하지 않는 `target_skill_id` 또는 `outcome_skill_id`
- `recipient_scope`와 `recipient_monster_id` 조합 오류
- Node/Trigger의 존재하지 않는 `owner_id`
- 정령계약 단계가 2/4/6/8과 일치하지 않음
- `SpawnUnit` 효과가 존재하지 않는 소환 몬스터 ID를 참조
- 정령왕 해금 스킬이 존재하지 않음
- 소환 몬스터를 playable `catalog.Monsters`에 넣어 Manifest 후보가 되게 함

### 5.3 Generation

Generation이 최종 typed 객체를 만든다.

- `ArtifactDefinition`
- `ArtifactSynergyDefinition`
- `ArtifactSynergyLevelDefinition`
- `ArtifactEffectDefinition`
- `ArtifactSynergyEffectDefinition`
- 정령왕용 `SummonDefinition`
- 정령왕의 기존 `SingleSkillDefinition`/`ZoneSkillDefinition`

Generation 책임:

- Effect Node를 typed `SkillNode[]`로 변환
- Effect Trigger를 typed `SkillReaction[]`로 변환
- `outcome_skill_id`를 문자열로 남기지 않고 실제 `SkillDefinition` 참조로 확정
- 시너지 Definition에 level Definition을 연결
- level Definition에 추가 효과 Definition을 연결
- Artifact Definition에 고유 추가 효과 Definition을 연결

현재 Trigger 결과가 Generation에서 기존 Single/Zone/Buff Definition으로 materialize되는 방향을 그대로 따른다. `SingleAttack`은 `SingleSkillDefinition`, `AreaAttack`은 `ZoneSkillDefinition`으로 생성한다. 정령왕 소환 때문에 새 `SkillDefinition` family를 만들지 않는다.

정령왕은 `SummonDefinition`으로 생성하고 playable `GameDataCatalog.Monsters`와 분리된 `GameDataCatalog.Summons` lookup에 등록한다. 현재 `MenifestUI.ResolveNextManifestCandidate`가 `GameDataCatalog.GetMonsters()` 전체를 후보로 사용하므로 같은 배열에 넣으면 정령왕이 파티 영입 후보가 된다.

### 5.4 RuntimeCatalog

RuntimeCatalog는 ID 조회를 제공한다.

- Artifact
- Artifact synergy와 level
- Artifact effect
- Artifact synergy effect
- 소환 몬스터 lookup
- 정령왕 스킬

효과 적용 시 CSV를 다시 읽거나 문자열 enum을 다시 파싱하지 않는다.

## 6. Definition 책임

### 6.1 `ArtifactDefinition`

- ID, 표시명, `synergy_id`, 설명, icon Sprite 보유
- Generation이 연결한 `ArtifactEffectDefinition[]` 보유
- 전투 계산과 Stage 순회는 하지 않음

### 6.2 `ArtifactEffectDefinition`

- 유물 고유 추가 효과의 독립 Definition
- 적용 대상, 대상 스킬, typed Nodes, typed Reactions, outcome Skill 참조 보유
- 강화·마스터와 같은 Node 해석기를 사용하지만 `SkillChoice`가 아님

### 6.3 `ArtifactSynergyDefinition` / `ArtifactSynergyLevelDefinition`

- 시너지 ID와 2/4/6/8 level 소유
- 각 level이 `ArtifactSynergyEffectDefinition[]` 소유
- 설명과 요구 개수는 `artifact_synergies.csv`가 원본

### 6.4 `ArtifactSynergyEffectDefinition`

- 시너지 단계 추가 효과의 독립 Definition
- 적용 범위, Nodes, Reactions, outcome Skill 또는 spawn Monster 참조 보유
- 정령계약에서는 `SpawnUnit`과 정령왕 스킬 해금을 표현

## 7. 상태와 Manager 책임

### 7.1 `ArtifactState`

각 `RunSession.RunMonsterState`가 하나 소유한다.

- `OwnedArtifactIds`: 영구 보유 `artifact_id`, 최대 3개
- `ActiveArtifactEffectIds`: 현재 Stage에서 이 유닛이 실제 적용받는 Effect ID
- `CanAdd`, `TryAdd`, `Remove`
- Stage 준비 때 활성 Effect 목록을 비운 뒤 `recipient_scope`에 따라 다시 배포
- 스킬 snapshot 생성 시 활성 `SkillModifier` Effect Node를 공통 해석기로 전달

`ArtifactState`는 강화·마스터 Choice 목록에 유물 ID를 섞지 않는다.

### 7.2 `SynergyState`

`ArtifactSynergyManager`가 Stage마다 다시 만든다. Phase 3에서는 개수만 소유하고 시너지 Effect를 활성화하지 않는다.

- 파티 전체 시너지별 보유 개수
- 현재 시너지 개수 로그

활성 level, 시너지 Effect와 정령왕 해금 스킬은 Phase 4 이후 범위다. 유물 효과에 필요한 파티 대표 속성과 파티원별 대표 속성은 `PrepareStage`에서 계산하고 `ActiveArtifactEffectIds` 배포로 고정한다.

### 7.3 `ArtifactSynergyManager`

Manager가 유물·시너지 추가 효과의 Stage 수명주기를 구현한다.

제안 책임:

1. `PrepareStage(RunSession)`에서 모든 `ArtifactState`를 순회
2. 모든 `ActiveArtifactEffectIds`를 지워 Stage 재진입 중복 누적 방지
3. Artifact Definition의 `synergy_id`로 개수 계산하고 `SynergyState` 재구성
4. 개별 Artifact Effect의 `recipient_scope`를 해석해 파티 유닛에 활성 Effect ID 배포
5. 정령계약 유물 10개만 배포하고 시너지 개수 한 줄 로그 출력

Phase 4 이후에만 활성 시너지 Effect 순회, outcome Skill 실행, SpawnUnit과 정령왕 Stage 정리를 추가한다.

Manager가 직접 `Instantiate`, `ApplyDamage`, 치명타 계산을 하지 않는다. 각 concrete Definition과 기존 Executor가 실제 결과를 실행한다.

## 8. 기존 강화·마스터 경로 재사용

현재 `SkillExecutionRules.ApplyChoice`는 `SkillChoice.Nodes`를 snapshot에 적용한다. 유물 Definition을 Choice로 변환하지 않고 Node 적용 부분만 공통화한다.

Phase 3의 snapshot 합성 순서는 `Definition -> 학습 Passive -> Enhancement -> Master -> Artifact SkillModifier`다. `SkillExecutionRules`는 현재 유닛의 `ArtifactState.ActiveArtifactEffectIds`만 순회하고 파티 전체를 매 시전마다 다시 검색하지 않는다.

목표 구조:

```text
SkillChoice
  -> 공통 SkillNode 적용 함수

ArtifactEffectDefinition
  -> 같은 공통 SkillNode 적용 함수

ArtifactSynergyEffectDefinition
  -> 같은 공통 SkillNode 적용 함수
```

Trigger도 같은 원칙이다.

```text
기존 SkillReaction
ArtifactEffectDefinition.Reactions
ArtifactSynergyEffectDefinition.Reactions
  -> 기존 Trigger gate/scheduler
  -> Generation 확정 SkillDefinition
  -> 기존 SkillExecution/Executor
```

따라서 유물 전용 두 번째 Node 해석기나 피해 Executor는 만들지 않는다.

### 8.1 실제 routing 경로

| `application_mode` | CSV 이후 경로 | 실행 지점 |
|---|---|---|
| `SkillModifier` | 기존 `skill_graph_nodes_passive.csv`의 `owner_kind=Effect` -> `SkillGraphParser` -> `CsvDataValidator` -> `GameDataCatalogBuilder.Nodes` -> `EffectDefinition.Nodes` | `SkillExecutionRules`가 `ArtifactState.ActiveArtifactEffectIds`를 순회하고 기존 `ApplyNodes` 호출 |
| `PassiveTrigger` | 기존 `passive_skill_triger.csv`와 graph node의 `effect_id` -> `SkillReaction` -> `SkillTrigger` | `SkillTrigger`가 활성 Artifact Reaction을 수집하고 기존 gate/scheduler 뒤 `SkillExecution -> family Executor` 실행 |
| `ExecuteSkill` | `outcome_skill_id -> Generation에서 concrete SkillDefinition 참조 확정` | `ArtifactSynergyManager.ActivateStageEffects` 또는 Trigger가 `SkillExecution` 호출 |
| `GrantSkill` | `outcome_skill_id -> Generation에서 concrete SkillDefinition 참조 확정` | `SynergyState`가 선택 유닛/정령왕의 Stage 한정 `SkillState`에 부여 |
| `SpawnUnit` | `spawn_monster_id -> Generation에서 SummonDefinition 참조 확정` | `ArtifactSynergyManager.ActivateStageEffects -> UnitSpawnManager.SpawnTemporaryMonster` |

두 Effect Definition과 Loading 경로, `ArtifactState`, Effect Definition의 Node/Reaction 필드, 기존 snapshot/Trigger 실행 연결까지 구현됐다. 새 Node/Trigger CSV나 `PassiveSkillDefinition` 없이 기존 Effect owner와 `SkillExecutionRules`/`SkillTrigger` 경로를 재사용한다.

### 8.2 공통 Node 선택 기준

| 목적 | 현재 존재하는 Node |
|---|---|
| 피해 배율 | `DamageMultiplier`, `ConditionalDamageMultiplier`, `TargetPredicateDamageMultiplier`, `TargetStatusStackDamageRateBonus` |
| 속성/상태 조건 | `ConditionSkillAttribute`, `ConditionAnyStatus`, `ConditionStatus`, `ConditionStatusExpression`, `RequiredSourceStatus` |
| 치명타 | `CritChanceBonus`, `CritDamageBonus`, `TargetStatusCritBonus`, `ExecuteCritChanceBonus` |
| 탄창/투사체 | `MagazineBonus`, `ReloadTimeMultiplier`, `ShotIntervalMultiplier`, `PierceBonus`, `FollowUpProjectile`, `StatusRuntimeKindFilter` |
| 방어막/상태 | `ShieldAmountMultiplier`, `StatusDurationBonus`, `StatusConditionalDamageTakenBonus`, `ApplyShield`, `ApplyStatus` |
| 사건 결과 | `ApplyDamage`, `ExtendStatusDuration`, `ExecuteSkill`, `RefundCooldown`, `ReduceReload` |

Node ID는 `skill_node_definitions.csv`, 인자는 `skill_node_definition_params.csv`가 현재 근거다. 아래 표의 `신규 필요`는 이 목록과 enum/runtime 실행 경로에 없는 기능이다.

## 9. Stage 실행 순서

현재 `StageManager.RunCurrentDayFlow`는 RunSession 준비, 플레이어 생성, Nexus 등록, 적 생성, Combat 전환 순서다. 1차 구현은 플레이어 등록의 기존 `CombatStart` 의미를 바꾸지 않고 명시적인 유물 Manager 호출을 추가한다.

```text
RunSession 준비
  -> 전체 파티 Restore/Spawn
  -> Nexus 등록
  -> ArtifactSynergyManager.PrepareStage
     -> ArtifactState 확인
     -> SynergyState 재계산
     -> 정령계약 단계/해금 스킬 확정
  -> ArtifactSynergyManager.ActivateStageEffects
     -> 이전 Stage 정령왕 정리
     -> SynergyState 2 이상이면 정령왕 1명 생성
     -> 보유 단계에 맞는 A~D 스킬만 학습
  -> 적 Encounter Spawn 완료
  -> StageState.Combat
```

Manager의 Stage 호출은 하루당 한 번만 허용한다. 플레이어 등록 이벤트에 의존하지 않으므로 살아남은 기존 파티원과 새 파티원의 동작이 같아진다.

## 10. 정령계약 1차 구현

### 10.1 소환

소환을 Skill로 만들지 않는다. 정령계약 2단계의 `SpawnUnit` effect를 Stage 시작 때 Manager가 실행한다.

재사용 구조:

- 유닛 데이터: `SummonDefinition`
- 전투 모델: 기존 `UnitCombatState`
- 표시 Actor: 기존 `MonsterActor`
- 생성/등록: 기존 `UnitCombatStateFactory`, `UnitSpawnManager`, `RegisterPlayer`, `NotifyPlayerUnitRegistered`
- 스킬: 기존 `SingleSkillDefinition`/`SkillRuntimeKind.SingleAttack`과 `ZoneSkillDefinition`/`SkillRuntimeKind.AreaAttack` 실행 경로
- 자동 시전: `SkillExecution.TryExecuteAutomaticSkills`
- 팀 판정: `UnitSide.Player`

최소 추가점:

- `UnitRole.Summon`: 파티 슬롯, Manifest, Offering, MonsterPanel, Day 회복 대상과 구분
- `UnitSpawnManager.SpawnTemporarySummon`
- playable `Monsters`와 분리된 `GameDataCatalog.Summons` lookup
- `UnitSpawnManager`의 기존 등록 목록을 순회하는 정령왕 이동 tick

정령왕을 일반 `monsters.csv`/`catalog.Monsters`에 넣지 않는다. 현재 monster Validation은 모든 Monster에 A~E active와 F~J passive slot을 요구하고, `MenifestUI`는 `GetMonsters()` 전체를 영입 후보로 사용한다. 소환 몬스터 CSV는 `SummonDefinition`을 생성해 별도 lookup에 둔다.

정령왕 기본 규칙:

- Stage당 최대 1명
- 전장 중앙 소환
- `Identity.Side=Player`, `Identity.Role=Summon`
- 기본 공격 Skill을 넣지 않음
- `AutoSkillEnabled=true`
- 소환 이후 시전되는 다른 몬스터의 `Ally/AllAllies` 스킬 대상에 포함
- 적 AI의 Player 대상 후보에 포함
- 소환 직후부터 `EnemySpawnPoint`를 향해 이동
- 적이 없으면 이동하지 않으며, 적이 다시 등록되면 이동을 재개
- 이동 속도는 `0.5`로 고정하고 `EnemySpawnPoint` 도착 후 정지
- 정령왕 사망 후 현재 Stage에서는 재소환하지 않음
- 다음 Stage 시작 시 시너지 개수가 2 이상이면 이전 소환물을 정리하고 새로 1명 소환
- 일반 파티원, Offering, Run 파티 슬롯에 포함하지 않음
- DamageMeter에는 포함하지 않고 기존 `MonsterActor`의 HP·피해 팝업만 사용

팀 효과 근거: `CombatUnitRegistry`는 `Identity.Side`로 Players/Enemies를 나누고, `SkillTargeting.TargetList`는 Player 시전자의 `Ally/AllAllies`에 `roster.Players`를 사용한다. 따라서 정령왕은 적의 공격 대상과 몬스터의 팀 대상 스킬 대상에 포함된다. `NotifyPlayerUnitRegistered`가 등록 즉시 passive와 `CombatStart`를 실행하므로 정령왕 생성 전에 끝난 일회성 팀 효과는 소급하지 않는다.

이동은 현재 `EnemyActionController`가 `units.Enemies`만 순회하므로 그대로 재사용할 수 없다. 적 공격·대상 결정은 재사용하지 않고, 정령왕 이동을 `UnitSpawnManager` 또는 정령왕 전용 최소 이동 경로에서 처리한다. 대상은 적 유닛이 아니라 씬에 이미 연결된 `EnemySpawnPoint` Transform이다.

```text
InGameCombatManager.Update
  -> UnitSpawnManager.TickSummons
  -> roster.Players 중 UnitRole.Summon
  -> roster.Enemies.Count == 0이면 이동하지 않음
  -> EnemySpawnPoint까지 0.5 * deltaTime으로 MoveTowards
  -> EnemySpawnPoint 도착 후 이동 완료 상태 유지
```

정령왕 이동 속도는 `summon_units.csv`의 `base_move_speed=0.5`가 소유한다. 별도 `stop_distance` 열이나 적 이동속도 resolver는 만들지 않는다. 이동 AI는 스킬 선택·시전하지 않고 기존 자동 시전 루프가 담당한다.

### 10.2 단계별 효과

| 보유 수 | 1차 구현 효과 | 실행 계열 |
|---:|---|---|
| 2 | 정령왕 소환, 원소폭발 해금 | `SpawnUnit` + `SingleAttack` |
| 4 | 원소폭풍 추가 해금 | `AreaAttack`/Zone |
| 6 | 정령 폭격 추가 해금 | `SingleAttack` + `RepeatPerTarget` 총 3회 |
| 8 | 차원붕괴 추가 해금 | pull `ZoneSkill` 종료 후 폭발 `SingleAttack` |

스킬 값은 `artifact-synergy-list.md`를 그대로 사용한다.

- 원소폭발: 5초마다 밀집 지점, 80 속성 피해
- 원소폭풍: 6초 지속, 0.5초마다 18 속성 피해
- 정령 폭격: 8초마다 3개 지점, 각 70 속성 피해
- 차원붕괴: 전투 중 1회, 중앙 균열에서 `0.2 unit/tick`으로 끌어당긴 뒤 240 속성 피해
- 시너지가 8에서 6으로 감소하면 차원붕괴를 새로 학습하지 않는다. 다음 Stage의 해금 목록은 현재 시너지로 다시 계산한다.

### 10.3 기존 경로와 공백

재사용:

- `SingleSkillDefinition`/`SingleSkillExecutor`
- `ZoneSkillDefinition`/AreaAttack Executor
- cooldown, duration, tick interval
- `RepeatPerTarget`의 반복 배치
- `ExecuteSkill` 후속 Definition 실행
- Trigger 결과의 concrete Definition 실행
- `UnitCombatState.SkillState`의 일반 active skill 실행

새 기능:

- `SpawnUnit` effect와 임시 아군 spawn API
- `UnitRole.Summon`과 별도 소환 몬스터 catalog lookup
- `UnitSpawnManager`가 기존 등록 목록을 순회하는 정령왕 이동 tick
- `SkillTargetSelection.Densest`: 적이 가장 많이 몰린 지점
- Stage 중앙 위치 규칙
- 정령왕 해금 스킬 구성
- 차원붕괴 끌어당김 typed operation을 기존 Zone lifecycle에 연결
- Zone 종료 시 기존 `OnExpire`를 후속 폭발 Trigger로 연결
- 파티 대표 피해 속성 계산

## 11. 후속 구현 명시

정령계약 완료와 검증 뒤 별도 Phase로 진행한다.

1. 모든 유물 고유 효과와 `artifact_effects.csv` 실제 행
2. 처형관
3. 선택받은자
4. 파수꾼
5. 포격대
6. 추적자 원문 작성 후 추적자

후속 구현은 각 효과를 다시 현재 코드에서 검증한다. 지금 문서에서 “기존 시스템으로 전부 구현 가능”이라고 확정하지 않는다.

### 11.1 시너지별 Node·실행 경로

| 시너지 | 단계 | `application_mode`와 실제 경로 | 현재 공백 |
|---|---:|---|---|
| 정령계약 | 2 | `SpawnUnit`: Manager -> `UnitSpawnManager.SpawnTemporarySummon`; `GrantSkill`: 정령왕에 원소폭발 `SingleAttack` 부여 | 임시 아군 spawn/이동, `Densest`, 대표 속성 resolver 신규 필요 |
| 정령계약 | 4 | `GrantSkill`: 원소폭풍 `AreaAttack/Zone`, 기존 duration/tick 경로 | `Densest` 신규 필요 |
| 정령계약 | 6 | `GrantSkill`: 정령 폭격 `SingleAttack`; `RepeatPerTarget(repeat_count=2, interval=0.35, multiplier=1)` | 세 배치를 서로 다른 밀집 지점으로 재선정하려면 `Densest` 반복 재선택 규칙 필요 |
| 정령계약 | 8 | `GrantSkill`: 차원붕괴 pull `ZoneSkill`; Zone의 `OnExpire -> ExecuteSkill`로 폭발 `SingleAttack`; 쿨다운 999 | 끌어당김 typed Node, Stage 1회 gate 필요 |
| 처형관 | 2 | `SkillModifier`: `CritChanceBonus`; 저체력 추가분은 `ExecuteCritChanceBonus`를 체력 조건과 결합 | 현재 `TargetHealthRatioCondition`은 cast 조건이므로 hit별 체력 조건 연결 신규 필요 |
| 처형관 | 4 | `SkillModifier`: `CritDamageBonus` | 치명타가 난 공격만 최종 피해 +8% 처리할 crit-result 조건 신규 필요 |
| 처형관 | 6 | `SkillModifier`: 저체력 조건 + `CritDamageBonus` | 조건부 치명타 피해 Node 신규 필요 |
| 처형관 | 8 | `SkillModifier`: 보스 조건 치명타 확률/피해 | `TargetPredicateDamageMultiplier`는 피해 배율만 지원하므로 보스 조건 치명타 Node 신규 필요 |
| 선택받은자 | 2 | Manager가 1명 선택; `PassiveTrigger`: `OnSkillCast`, `EveryCount=3`, 마지막 active outcome 재실행 | event skill 자체를 50%로 재실행하는 동적 outcome 신규 필요 |
| 선택받은자 | 4 | `SkillModifier`: 선택 대상에게 `DamageMultiplier(1.18)` | 기존 Node 재사용 가능 |
| 선택받은자 | 6 | `PassiveTrigger`: 15초 gate -> `RefundCooldown` | 남은 쿨타임 최장 active 1개 선택 규칙 신규 필요 |
| 선택받은자 | 8 | `SkillModifier`: `TargetPredicateDamageMultiplier(is_boss,1.5)` | 기존 Node 재사용 가능 |
| 파수꾼 | 2/4/6/8 | `SkillModifier`: 파티 방어력 배율과 전 속성 저항 flat 보정 | 해당 Unit stat 보정 Node가 현재 node catalog에 없어 신규 필요 |
| 파수꾼 | 4/8 | `PassiveTrigger`: `OnShieldAbsorb` -> `ApplyDamage(Holy,value_source=ShieldAbsorbedAmount)` | 기존 event/value source/Node 재사용 가능 |
| 파수꾼 | 6 | `SkillModifier`: source가 shield 상태일 때 받는 피해 감소 | `StatusConditionalDamageTakenBonus` 경로 재사용 가능, 파티 효과 source 연결 검증 필요 |
| 포격대 | 2/4/6/8 | `SkillModifier`: `ReloadTimeMultiplier`; `PassiveTrigger` -> 지원 포격 `AreaAttack`; 4단계 `RadiusMultiplier`, 6단계 파편 outcome, 8단계 `DamageMultiplier`/`RadiusMultiplier` | `OnReloadComplete`, `Densest`, 파편 중복 대상 제외 신규 필요 |
| 추적자 | - | 행 생성 안 함 | 상세 단계와 유물 원문 없음 |

### 11.2 정령계약 유물

모두 수동 시전 없는 유물 패시브다.

| 유물 | mode | Node·경로 | 현재 공백 |
|---|---|---|---|
| 원소 프리즘 | `SkillModifier` | 학습 active A~E의 비Physical 속성을 파티 전체에서 집계 -> 최다 속성 하나의 `ConditionSkillAttribute` -> `DamageMultiplier(1.12)` | 구현 완료. 동률은 slot A~E, 같은 slot은 1P~5P 순서 |
| 불씨 왕관 | `SkillModifier` | `ConditionSkillAttribute(Fire)` + `DamageMultiplier(1.18)`; `ConditionStatus(fire-exposure)` + `ConditionalDamageMultiplier(1.10)` | 기존 Node 재사용 가능 |
| 서리 렌즈 | `SkillModifier` | `ConditionSkillAttribute(Ice)` + `DamageMultiplier(1.18)`; `ConditionAnyStatus(chill,freeze)` + `ConditionalDamageMultiplier(1.10)` | 기존 Node 재사용 가능 |
| 폭풍 축전기 | `SkillModifier` | `ConditionSkillAttribute(Lightning)` + `DamageMultiplier(1.18)`; `ConditionStatus(shock)` + `ConditionalDamageMultiplier(1.10)` | 기존 Node 재사용 가능 |
| 성광 잔 | `SkillModifier` | `ConditionSkillAttribute(Holy)` + `DamageMultiplier(1.18)`; source shield 조건 + `DamageMultiplier(1.08)` | source 상태 조건 Node 연결 신규 필요 |
| 검은 촛대 | `SkillModifier` | `ConditionSkillAttribute(Darkness)` + `DamageMultiplier(1.18)`; `ConditionAnyStatus(name-mark;holy-exposure;sein-a-hit-mark)` + `DamageMultiplier(1.08)` | 구현 완료 |
| 정령의 비약 | `SkillModifier` | 모든 속성 `DamageMultiplier(1.10)` + 파티 정령계약 유물 수만큼 `DamageMultiplier(1.02)` 반복 배포 | 구현 완료. 자기 자신 포함 |
| 균열 보석 | `PassiveTrigger` | 소유자 `CombatStart` -> 전 적 대상 영구 Buff/Status outcome 6개 -> Physical/Fire/Lightning/Ice/Darkness/Holy 각각 `StatusFlatElementResistReduction(5)` | 구현 완료. 다른 source와 합산 |
| 원소 도감 | `SkillModifier` | 파티원별 학습 active A~E 중 비Physical 대표 속성을 구하고 서로 다른 수만큼 `DamageMultiplier(1.04)` 반복 배포 | 구현 완료. 대표 속성 동률은 A~E 순서, 최종 피해는 모든 속성 대상 |
| 공명 나침반 | `PassiveTrigger` | 비Physical `OnOutgoingDamage`, `ProcChance=.08`, `EventAppliedDamage * .30` -> 같은 속성 `ApplyDamage`; Rin 양손잡이 후속타 visual 재사용 | 구현 완료 |

### 11.3 처형관 유물

| 유물 | mode | Node·경로 | 현재 공백 |
|---|---|---|---|
| 유리 심장 | `SkillModifier` | `CritDamageBonus(.30)` | 기존 Node 재사용 가능 |
| 예언자의 눈 | `SkillModifier` | 최고 체력 적 조건 -> `CritChanceBonus(.15)` | `HighestHealth`는 cast selection만 있으므로 hit 대상 predicate 신규 필요 |
| 처형 반지 | `SkillModifier` | 체력 35% 이하 조건 -> `CritDamageBonus(.50)` | 조건부 치명타 피해 Node 신규 필요 |
| 날 선 성배 | `PassiveTrigger` | 신성 crit hit -> `ExtendStatusDuration(holy-exposure,1)` | crit-result Trigger 조건 신규 필요 |
| 붉은 조준경 | `SkillModifier` | projectile runtime-kind 조건 -> `CritChanceBonus(.12)` | `StatusRuntimeKindFilter`는 status용이므로 `ConditionSkillRuntimeKind` 신규 필요 |
| 금 간 왕관 | `SkillModifier` | boss predicate -> `CritChanceBonus(.10)` + `CritDamageBonus(.25)` | 보스 조건 치명타 Node 신규 필요 |
| 사형 명부 | `SkillModifier` | `TargetStatusCritBonus(mark status,.15,0)` | 표식으로 인정할 실제 status ID 목록 확정 필요 |
| 백은 바늘 | `SkillModifier` | multi-hit 마지막 index 조건 -> `CritDamageBonus(.70)` | `OnMagazineLastProjectileHit`와 다른 hit-index 조건 신규 필요 |
| 별빛 숫돌 | `SkillModifier` | 치명타 저항 관통 stat 보정 | 현재 전투 stat/Node에 치명타 저항 관통 없음 |
| 운명의 동전 | `PassiveTrigger` | 비치명타 연속 상태 -> 다음 공격 `CritChanceBonus(.05*stack)` | crit-result event와 유물별 stack state 신규 필요 |

### 11.4 선택받은자 유물

| 유물 | mode | Node·경로 | 현재 공백 |
|---|---|---|---|
| 양자 연산 | `SkillModifier` | `recipient_monster_id=eve`; `ConditionSkillAttribute(Lightning)` -> `DamageMultiplier(1.50)` | `monsters.csv`의 `eve` 확인됨 |
| 절대영점 회로 | `SkillModifier` | `recipient_monster_id=eve`; `ConditionSkillAttribute(Ice)` -> `DamageMultiplier(1.35)`; `ConditionStatus(freeze)` -> `ConditionalDamageMultiplier(1.25)` | 기존 Monster/attribute/status ID 확인됨 |
| 이름 없는 명부 | `SkillModifier` | `target_skill_id=vega-e`; `TargetStatusStackDamageRateBonus(name-mark,.06)` | 기존 skill/status ID 확인됨 |
| 검은 처형대 | `SkillModifier` | `target_skill_id=vega-d`; `DamageMultiplier(1.45)` | 기존 skill ID 확인됨 |
| 대류 화살촉 | `SkillModifier` | `target_skill_id=sein-a`, `sein-b`; 각 `DamageMultiplier(1.40)` | 기존 skill ID 확인됨 |
| 종말의 잔불 | `SkillModifier` | `target_skill_id=sein-e`; `CooldownMultiplier(.75)` + `DamageMultiplier(1.25)` | 기존 skill ID 확인됨 |
| 대천사 인장 | `SkillModifier` | `target_skill_id=ariel-b`, `ariel-e`; `ShieldAmountMultiplier(1.45)` | 두 skill의 shield 결과에 같은 Node가 적용되는지 Generation 검증 필요 |
| 성가 지휘봉 | `SkillModifier` | `target_skill_id=ariel-c`; 축복 status 효과 Node 보정 | “축복 효과”가 피해/지속/행동속도 중 무엇인지 원문 수치 항목 확정 필요 |
| 파쇄 장갑 | `SkillModifier` | `target_skill_id=rin-a`; `DamageMultiplier(1.50)` | 기존 skill ID 확인됨 |
| 무투가의 호흡 | `PassiveTrigger` | Rin `OnOutgoingDamage(Physical)` -> self `ApplyStatus`; status에 `StatusActionSpeedBonus(.04)`, max 5 | 전용 status row 필요; 기존 Trigger/Node 재사용 가능 |

### 11.5 파수꾼 유물

| 유물 | mode | Node·경로 | 현재 공백 |
|---|---|---|---|
| 순백 방패 | `PassiveTrigger` | `CombatStart` -> AllAllies Shield outcome | 최대 체력 12% shield coefficient 신규 필요 |
| 성역 조각 | `SkillModifier` | source shield 조건 + `ConditionSkillAttribute(Holy)` + `DamageMultiplier(1.18)` | source 상태 조건 Node 신규 필요 |
| 깨지지 않는 약속 | `PassiveTrigger` | `OnShieldExpire` -> self `ApplyStatus`; `StatusDamageTakenBonus(-.30)`, 2초 | 기존 event/Node 재사용 가능 |
| 수호자의 향로 | `SkillModifier` | shield status/skill 대상 `StatusDurationBonus(2)` | 적용 대상 shield status 연결 검증 필요 |
| 푸른 십자가 | `PassiveTrigger` | heal/shield received -> target `ApplyStatus`; `StatusActionSpeedBonus(.10)`, 4초 | heal/shield received event 없음 |
| 순례자 망토 | `SkillModifier` | boss Stage에서 shield skill에 `ShieldAmountMultiplier(1.50)` | boss Stage 조건 resolver 신규 필요 |
| 빛바랜 성문 | `PassiveTrigger` | `CombatStart` -> AllAllies Buff/`ApplyStatus`; `StatusDamageTakenBonus(-.20)`, 6초 | 기존 Trigger/Skill 경로 재사용 가능 |
| 반사 거울 | `PassiveTrigger` | `OnShieldAbsorb` -> `ApplyDamage(Holy,value_source=ShieldAbsorbedAmount,value_source_multiplier=.20)` | 기존 event/value source/Node 재사용 가능 |
| 기도석 | `SkillModifier` | source shield 조건 -> cooldown charge speed +12% | 행동속도와 분리된 조건부 cooldown charge Node 신규 필요 |
| 파수꾼의 종 | `PassiveTrigger` | `OnShieldExpire` -> 주변 적 Area/Buff outcome -> `ApplyStatus(holy-exposure,2초)` | 기존 event/outcome 경로 재사용 가능 |

### 11.6 포격대 유물

| 유물 | mode | Node·경로 | 현재 공백 |
|---|---|---|---|
| 무한 탄피 | `SkillModifier` | MagazineProjectile 조건 -> `MagazineBonus(1)` | `ConditionSkillRuntimeKind` 신규 필요 |
| 과열 약실 | `PassiveTrigger` | 탄창 소진 상태 -> 다음 `OnReloadComplete` -> 1회 `DamageMultiplier(1.25)` | event와 다음 1회 state 신규 필요 |
| 쌍열 코어 | `SkillModifier` | 첫 projectile 조건 -> `FollowUpProjectile(1,delay,0.30)` | 첫 projectile index 조건 신규 필요 |
| 신속 장전기 | `SkillModifier` | MagazineProjectile 조건 -> `ReloadTimeMultiplier(.82)` | runtime-kind 조건 신규 필요 |
| 관통 깃털 | `SkillModifier` | projectile 조건 -> `PierceBonus(1)` | runtime-kind 조건 신규 필요 |
| 난사 도면 | `SkillModifier` | projectile 조건 -> `ShotIntervalMultiplier(.88)` + `DamageMultiplier(.95)` | runtime-kind 조건 신규 필요 |
| 축복 화살통 | `SkillModifier` | projectile 조건 + `ConditionSkillAttribute(Holy/Fire)` -> `DamageMultiplier(1.18)` | runtime-kind 조건 신규 필요 |
| 번개 탄창 | `PassiveTrigger` | Lightning projectile hit, `ProcChance=.20` -> `ApplyStatus(shock,1)` | runtime-kind 조건 신규 필요; 나머지 Trigger 재사용 가능 |
| 처형 탄환 | `SkillModifier` | magazine 마지막 projectile 조건 -> `DamageMultiplier(1.60)` | 현재 `OnMagazineLastProjectileHit`는 적중 후 event라 원탄 피해 보정 불가; pre-hit index 조건 신규 필요 |
| 회전 약실 | `SkillModifier` | MagazineProjectile 조건 -> `CritChanceBonus(.10)` | runtime-kind 조건 신규 필요 |

## 12. 구현 Phase

### Phase 1: Effect·정령왕 CSV 작성

- `Artifact/Effect/artifact_effects.csv`: 현재 원문 50개 유물 효과 작성
- `Artifact/Effect/artifact_synergy_effects.csv`: 현재 상세 원문이 있는 다섯 시너지 단계 효과 작성
- `authoring/summon/summon_units.csv`: `monsters.csv` 형식으로 정령왕 1행 작성
- `authoring/summon/skill/summon_units_skill.csv`: `skills_area_attack.csv` 형식으로 정령왕 스킬 5행 작성
- 정령왕은 체력 1000, 주 속성 Physical, 여섯 방어력 50으로 작성하고 나머지 값은 4.3 계약을 따른다.
- 원소폭발·정령 폭격·차원붕괴 폭발은 `SingleAttack`, 원소폭풍과 차원붕괴 중앙 균열은 `AreaAttack`으로 작성한다.
- 복합 설명은 독립 effect 행으로 분리
- 모든 유물 행은 `SkillModifier` 또는 `PassiveTrigger`
- 시너지 행은 `SkillModifier`, `PassiveTrigger`, `ExecuteSkill`, `GrantSkill`, `SpawnUnit`
- Phase 1 완료 시점에는 Parsing, C#, runtime 연결 없음
- 위 Node·경로 표에서 `신규 필요`인 효과도 식별 헤더는 작성하되, 존재하지 않는 `target_skill_id`/`outcome_skill_id`는 발명하지 않고 비워 둠

### Phase 2: Artifact·정령왕 Loading 기반

- 완료: 두 catalog, 두 Effect CSV, 정령왕 유닛·스킬 Parsing
- 완료: `CsvSourceModel` source rows와 외래 키·slot·runtime 값 Validation
- 완료: `ArtifactDefinition`, `ArtifactSynergyDefinition`, level/effect Definition, `SummonDefinition` Generation
- 완료: RuntimeCatalog 등록과 concrete `SkillDefinition`/`SummonDefinition` 참조 해석
- 검증: `dotnet build Pakuri/Pakuri.sln --no-restore` 오류 0, 집중 EditMode 테스트 1/1 통과

### Phase 3: 유물 상태 뼈대와 정령계약 유물 10개

상태: 완료. 정령계약 유물 10종의 modifier/trigger 데이터와 Stage 파티 resolver를 구현했고 기존 Node/Trigger 실행 경로로 검증했다.

- `ArtifactState.OwnedArtifactIds`, 유닛당 최대 3개
- `ArtifactState.ActiveArtifactEffectIds`, Stage 한정 수신 효과 목록
- `RunSession.RunMonsterState`와 `UnitCombatState`가 같은 `ArtifactState` 참조 공유
- `SynergyState`는 파티 전체 시너지별 보유 개수만 소유
- `ArtifactSynergyManager.PrepareStage`와 `StageManager` Stage당 1회 호출 연결
- 현재 시너지 개수를 한 줄 로그로 출력하고 시너지 Effect는 실행하지 않음
- 유물 전용 Node/Trigger CSV는 만들지 않음
- 정령계약 유물 10개의 실제 Node는 기존 `skill_graph_nodes_passive.csv`, Trigger는 기존 `passive_skill_triger.csv`에 `effect_id` 소유로 작성
- `ArtifactEffectDefinition`에 typed Node/Reaction 참조 연결
- `SkillModifier`는 Passive/Enhancement/Master 뒤 `SkillExecutionRules.ApplyNodes`로 최종 snapshot에 합성
- `PassiveTrigger`는 `SkillTrigger`가 활성 Artifact Reaction을 수집하고 기존 gate/scheduler/실행 경로 재사용
- 유물 ID를 `LearnedPassiveSkillIds`에 넣거나 `PassiveSkillDefinition`으로 생성하지 않음
- Stage 재진입 시 적용 목록을 지우고 다시 만들어 중복 누적 방지
- 원소 프리즘·원소 도감 대표 속성은 Physical을 제외하고 학습한 active A~E만 집계
- 정령의 비약과 원소 도감의 가변 배율은 기존 additive `DamageMultiplier` Effect ID 반복 배포로 합성
- 균열 보석의 6속성 영구 저항 감소와 공명 나침반의 5속성 후속 피해는 기존 Trigger outcome Definition으로 실행

### Phase 4: 정령계약 시너지 Node·Trigger 연결 및 스킬 해금

- 정령계약 시너지 Node·Trigger도 기존 graph-node/trigger CSV의 `Effect` owner 경로 사용
- 정령 폭격 `RepeatPerTarget` 총 3회 연결 및 반복별 `Densest` 재선정
- 차원붕괴 `ZoneSkill`의 `PullToCenter(0.2 unit/tick)`와 종료 후 폭발 `ExecuteSkill` 연결
- 기존 Zone 1.2초 lifecycle과 종료 `OnExpire` 재사용
- A~E 스킬 중 현재 시너지 단계에 해당하는 스킬만 정령왕에 학습
- 밀집 지점·대표 속성 resolver 연결
- `ArtifactSynergyEffectDefinition`에 typed Node/Reaction/outcome 참조 연결

### Phase 5: 임시 아군 정령왕 runtime

- `UnitRole.Summon`
- `SummonDefinition` 기반 Factory/Spawn
- 별도 소환 몬스터 RuntimeCatalog lookup
- 정령왕 Prefab binding
- 정령왕 SingleSkill/ZoneSkill Definition 실행
- 단계별 스킬 해금
- `EnemySpawnPoint`를 향한 고정 속도 0.5 이동과 도착 정지
- 사망 시 현재 Stage 재소환 금지, 다음 Stage에서 새 인스턴스 생성
- 기존 적 대상 판정·아군 팀 대상 판정·MonsterActor HP/피해 팝업 재사용

### Phase 6: 정령계약 검증

- 2/4/6/8 누적 효과와 8→6 감소 시 차원붕괴 비학습
- 첫 Day/다음 Day에 시너지 2 이상이면 Stage당 1회 소환
- Stage당 정령왕 1명
- 현재 Stage 사망 후 재소환하지 않음
- 정령왕 스킬 주기·피해·대상 위치
- 다음 Stage 소환물 정리

### 후속 Phase

- 매핑 표의 공통 Trigger/Node/조건 resolver를 필요한 순서로 추가
- 모든 유물 고유 효과 runtime
- 처형관, 선택받은자, 파수꾼, 포격대 runtime
- 상세 원문 작성 뒤 추적자 데이터/runtime

## 13. 결정 필요 항목

정령계약 유물 Phase 3 효과 규칙은 확정·구현됐다. 같은 artifact ID의 중복 획득 금지 규칙은 확정됐지만 실제 획득 시스템 구현 Phase에서 검사한다. 현재 `ArtifactState`의 3개 제한만 Phase 3 범위다.

정령계약 시너지·정령왕 구현에서 적용할 확정 규칙:

- 정령왕은 기존 MonsterActor/UnitCombatState 피해 경로로 피해를 받고 죽을 수 있다.
- 정령왕 생성 전에 이미 적용된 `CombatStart`/일회성 팀 효과는 소급하지 않는다.
- 정령왕은 `UnitRole.Summon`과 `UnitSide.Player`로 등록하므로 적 AI의 Player 대상과 아군 `Ally/AllAllies` 대상에 포함된다.
- 정령왕은 DamageMeter 기록에서 제외하고 HP 및 피해 팝업만 표시한다.

모든 수치는 CSV가 소유한다. 코드 상수로 박지 않는다.

## 14. 수용 기준

### 데이터

- Phase 1 완료 시 두 Effect CSV가 현재 원문 50개 유물과 다섯 상세 시너지의 effect 식별 행을 가진다.
- Phase 1 완료 시 `summon_units.csv`에 정령왕 1행, `summon_units_skill.csv`에 정령왕 스킬 5행이 4.3/4.4 계약대로 존재한다.
- 모든 개별 유물 행의 `application_mode`는 `SkillModifier` 또는 `PassiveTrigger`다.
- CSV가 `Parsing -> CsvSourceModel -> Validation -> Generation -> RuntimeCatalog` 한 경로를 통과한다.
- Effect CSV는 최종 Definition으로 생성된다.
- runtime에서 CSV 문자열을 다시 해석하지 않는다.
- 유물 Effect는 `SkillChoice`나 `PassiveSkillDefinition`으로 생성되지 않는다.
- 유물 전용 `effect_nodes.csv`, `effect_triggers.csv`는 존재하지 않고 기존 Node 정의·graph·trigger CSV를 재사용한다.

### 상태

- 파티원별 `OwnedArtifactIds` 0~3개 제한이 `ArtifactState`에서 지켜진다.
- `ActiveArtifactEffectIds`는 Manager가 Stage마다 비우고 `recipient_scope`에 따라 다시 배포한다.
- 전체 파티 유물로 정령계약 단계가 계산된다.
- Phase 3에서는 계산된 시너지 개수만 Stage당 한 번 로그로 확인할 수 있다.
- 다음 Day에 정령계약 유물 효과가 중복 누적되지 않는다.

### 실행

- Phase 3의 `ArtifactSynergyManager`는 Stage당 한 번 유물 상태를 재구성하고 정령계약 유물 효과만 배포한다.
- Phase 3에서는 `ArtifactSynergyEffectDefinition`을 실행하지 않는다.
- 정령계약 유물의 `SkillModifier`와 `PassiveTrigger`는 기존 Node/Trigger 경로를 통과한다.
- `SkillModifier`는 최종 스킬 snapshot에만 적용되며 원본 `SkillDefinition`을 변경하지 않는다.
- Artifact Trigger는 기존 `SkillReaction` gate/scheduler를 사용하고 별도 유물 Trigger executor를 만들지 않는다.
- 아래 정령왕 실행 기준은 Phase 4~6 후속 범위다.
- 정령왕 생성은 `SpawnUnit` Effect에서 `UnitSpawnManager.SpawnTemporaryMonster`를 통과한다.
- 정령왕 공격은 기존 SingleAttack/AreaAttack 실행 경로를 통과한다.
- 정령 폭격은 최초 실행을 포함해 정확히 3회 배치되고, 차원붕괴 폭발은 균열 종료 뒤 한 번만 실행된다.
- 정령왕은 `UnitSide.Player`로 등록되어 소환 이후 `Ally/AllAllies` 효과를 받고 적을 공격한다.
- 정령왕은 `UnitRole.Summon`이라 Run 파티, Manifest, Offering, MonsterPanel 대상에 들어가지 않는다.
- 정령왕 이동은 적이 존재할 때만 시작·재개하고 `EnemySpawnPoint`에 도착하면 정지한다.
- 정령왕 이동 속도는 `0.5`이며, 현재 Stage 사망 후 재소환하지 않는다.
- 정령왕은 DamageMeter에서 제외되고 `MonsterActor`의 HP·피해 팝업만 표시한다.
- Manager가 직접 피해를 적용하지 않는다.
- Offering 학습 ID와 강화·마스터 Choice 목록에 유물 효과가 들어가지 않는다.

### 범위

- Phase 1 완료 판정은 두 Effect CSV와 정령왕 유닛·스킬 CSV 데이터 작성까지만 대상으로 한다.
- Phase 3 runtime 완료 판정은 상태·Manager 뼈대, 시너지 개수 로그, 정령계약 유물 10개만 대상으로 한다.
- 정령계약 시너지 효과와 정령왕은 Phase 4 이후에 진행한다.
- 정령계약 외 유물 40개와 다른 시너지는 정령계약 유물 검증 뒤 진행한다.
- Tracker는 상세 원문이 준비되기 전 임의 구현하지 않는다.

## 15. 근거 파일

- 원문: `Pakuri/reference/4.run/artifact-synergy-list.md`
- 현재 catalog: `Pakuri/Assets/CSVdata/Artifact/artifacts.csv`, `artifact_synergies.csv`
- Phase 1 작성 대상: `Pakuri/Assets/CSVdata/authoring/summon/summon_units.csv`, `authoring/summon/skill/summon_units_skill.csv`
- 유닛·스킬 열 근거: `Pakuri/Assets/CSVdata/authoring/monster/monsters.csv`, `authoring/monster/skills/base/area_attack/skills_area_attack.csv`
- 현재 Loading 흐름: `Pakuri/Assets/Scripts/Loading/Parsing/CsvSourceLoader.cs`, `CsvSourceModel.cs`, `Pakuri/Assets/Scripts/Loading/Generation/`
- RuntimeCatalog: `Pakuri/Assets/Scripts/Loading/RuntimeCatalog/GameDataCatalog.cs`
- Run 상태: `Pakuri/Assets/Scripts/GameFlow/RunSession.cs`
- Stage 순서: `Pakuri/Assets/Scripts/GameFlow/Stage/StageManager.cs`
- Spawn: `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs`, `UnitCombatStateFactory.cs`
- 유닛 상태: `Pakuri/Assets/Scripts/Units/Model/UnitCombatState.cs`
- 강화·마스터 snapshot: `Pakuri/Assets/Scripts/Units/Runtime/UnitSkills.cs`, `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- Trigger: `Pakuri/Assets/Scripts/Combat/Skills/Definitions/Nodes/SkillNodeConditions.cs`, `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`
- 실행 계열: `Pakuri/Assets/Scripts/Combat/Skills/Definitions/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs`
- runtime_kind Generation: `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Skills.cs`
- Single 반복 근거: `Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillExecutor.cs`, `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecutionRules.cs`
- Node 저작 계약: `Pakuri/Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv`, `skill_node_definition_params.csv`
