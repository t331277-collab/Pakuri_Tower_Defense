# Interactive Tutorial Implementation Handoff

## Task title

Eve로 입장하는 1-1~1-5 튜토리얼 Stage와 4개 안내 Phase, CSV 대사, 종료 후 MainMenu 복귀 구현.

## Goals

- `MainMenuScene/Canvas/MainMenuUI/Tutorial` 클릭 시 기존 비동기 InGameScene 입장 경로를 재사용한다.
- 튜토리얼 기본 캐릭터는 데이터 키 `eve`로 고정한다. Ariel은 플레이 캐릭터가 아니라 대사 속 안내자다.
- TutorialStage 1-1~1-5를 일반 Stage와 분리된 CSV와 runtime catalog로 실행한다.
- Phase 1에서 기본 공격, Phase 2에서 Offering 스킬 습득과 현현, Phase 3에서 Auto와 배속, Phase 4에서 유물을 안내한다.
- 사용자 제공 Line1-1~Line4-3 대사를 `TutorialLine.csv`에서 순서대로 출력한다.
- Stage 1-5의 최종 진행을 마치면 `TutorialUI/TutoEnd`를 활성화한다.
- `TutoEnd/Button` 클릭 시 timeScale과 튜토리얼 상태를 정리하고 MainMenuScene으로 돌아간다.

## Constraints

- 기존 코어 로직은 유지하고 튜토리얼에 필요한 mode 분기, 상태 event, input gate만 좁게 추가한다.
- 사용자가 제공한 대사 문구는 맞춤법을 임의 교정하지 않고 원문을 보존한다.
- 기존 일반 Run의 캐릭터 선택, 비동기 InGameScene 로드, Stage1/Stage2 진행, 보상, 승리/패배를 보존한다.
- 일반 StageDefinition과 TutorialStageDefinition을 섞지 않는다.
- 대사 중 timeScale이 0이어도 typewriter와 UGUI 입력은 동작해야 한다.
- 보상 성공은 Button click이 아니라 RunSession의 실제 상태 변경으로 판정한다.
- Tutorial mode 밖에서는 Offering 선택 고정, 포로 행동 잠금, Auto/배속 gate를 적용하지 않는다.
- 사용자 Play Mode gameplay 검증은 사용자 소유다.

## Role Owner

Code Builder.

## Status

Phase A~G 구현과 C# 빌드·CSV 정적 검증 완료. Unity Play Mode 전체 흐름 검증은 사용자 확인 대기.

## Next Actions

1. Unity Editor를 연결한 뒤 CSV source validation과 changed-script diagnostics를 재확인한다.
2. 사용자가 Unity Play Mode에서 Normal Run과 전체 1-1~1-5 튜토리얼을 검증한다.

## Feasibility Judgment

결론: 구현 가능하다. CSV 추가만으로는 부족하며 튜토리얼 전용 runtime orchestration이 필요하다.

### Already supported by inspected code/data

- 기본 캐릭터 `eve`가 monster catalog에 존재한다.
- `stage1-swordsman`, `stage1-priest`, `stage1-rogue`, `stage1-guardian-captain` enemy ID가 존재한다.
- `eve-b`, `eve-c`, `eve-d` active skill 정의가 존재한다.
- StageReward는 `prisoner_count_2_chance`, `manifest_success_chance`, `artifact_choice_count`를 이미 파싱한다.
- `prisoner_count_2_chance=1`, 다른 prisoner count chance를 0으로 두면 `RollPrisonerCount()`는 2를 반환한다.
- RewardPanel은 포로 보상별 Button, 골드, Dark Trace, 유물 보상, NextBtn을 지원한다.
- PrisonPanel은 점유 party slot을 클릭하면 OfferingPanel, 다음 빈 slot을 클릭하면 현현 흐름으로 진입한다.
- 현현 성공 후 `RunSession.TryAddPartyMonster()`와 실제 spawn 경로가 존재한다.
- 유물은 ArtifactPanel 선택 후 PrisonPanel 수령자 선택, `RunSession.TryAcquireArtifact()` 성공으로 확정된다.
- StageManager는 모든 encounter row spawn 완료 뒤 `Spawning -> Combat`, 적 전멸 뒤 `RewardReady`가 된다.
- PlayerCombatInputController는 non-UI 좌클릭/홀드를 읽고, Auto button은 선택 플레이어의 auto skill mode를 토글한다.
- UtilityPanel 배속 목록은 1x, 1.5x, 2x다.
- `TutorialUI/TutoLine`, `TutorialUI/TutoEnd`, `TutoEnd/Button`이 씬에 존재한다.

### Required implementation gaps

1. Normal/Tutorial RunMode 전달과 Tutorial 기본 캐릭터 Eve 고정.
2. TutorialLine CSV parser/runtime collection.
3. TutorialStage Day/Encounter/Reward source catalog와 별도 StageDefinition.
4. TutorialFlowManager와 TutorialLineView.
5. Stage `StateChanged` 또는 같은 의미의 authoritative transition event.
6. 유효한 non-UI 수동 좌클릭을 알리는 one-shot 입력 event.
7. 첫 포로는 Offering만, 두 번째 포로는 현현만 가능하게 하는 tutorial input gate.
8. OfferingPanel에 Eve B/C/D만 정확히 세 개 출력하는 tutorial-only 선택 source.
9. skill acquired, manifest committed, Auto changed, time scale changed, artifact acquired 결과 event.
10. Tutorial 1-5 Next 처리 시 일반 Day 1-6으로 가지 않고 TutoEnd로 전환하는 종료 branch.

## Designer Assumptions

사용자가 수량이나 후반 데이터 값을 명시하지 않은 부분은 다음처럼 설계한다.

- Phase 1의 `stage1-swordsman`, `stage1-priest`는 각 1마리다.
- Phase 3의 `stage1-swordsman`은 2마리, `stage1-rogue`, `stage1-priest`, `stage1-guardian-captain`은 각 1마리다.
- Tutorial 1-3, 1-4, 1-5는 현재 일반 Stage1의 day3/day4/day5 encounter와 reward 값을 TutorialStage CSV에 복제해 사용한다. 1-1에서 override한 `reward-stage1-normal`이 1-3/1-4에도 재사용되지 않도록 후반 reward key는 tutorial 전용 고유 이름을 쓴다.
- Tutorial 1-2는 guardian captain을 포함하므로 기존 `reward-stage1-midboss` 값에 대응하는 tutorial reward를 사용한다. 유물 선택지는 현재 midboss 계약과 같은 3개를 제시하고 1개를 획득한다.
- Tutorial 1-5는 RewardPanel의 필요한 처리를 끝내고 NextBtn을 클릭한 시점을 최종 완료로 본다. 적 전멸 즉시 TutoEnd를 띄우지 않는다.
- Phase 2의 “PrisonPanel에 Eve B/C/D”는 현재 UI 구조에 맞춰 “포로 보상 -> PrisonPanel -> Eve slot -> OfferingPanel에 Eve B/C/D 세 개”로 구현한다.
- 사용자 제공 대사의 띄어쓰기와 표현은 원문 그대로 유지한다.

이 assumptions가 다르면 runtime 구조가 아니라 TutorialStage/TutorialLine 데이터와 gate 순서만 수정하면 된다.

## Inspected Evidence

### Entry and character

- `Pakuri/Assets/Scripts/UI/MainMenu/MainMenuUIManager.cs`
  - 일반 Run은 `StartContext.Prepare(monsterName)` 뒤 기존 비동기 InGameScene 로드를 실행한다.
  - Tutorial 게임 시작 listener는 아직 없다.
- `Pakuri/Assets/CSVdata/authoring/catalog/catalog_monsters.csv`
  - `catalog-monster-eve,eve,1`이 존재한다.

### Enemy and skill IDs

- `Pakuri/Assets/CSVdata/authoring/enemy/enemies.csv`
  - 네 enemy ID가 모두 존재한다.
  - guardian captain의 EncounterRole은 `Day5Midboss`다.
- Eve active skills:
  - `skills_line_attack.csv`: `eve-b`.
  - `skills_area_attack.csv`: `eve-c`.
  - `skills_single_attack.csv`: `eve-d`.
- `OfferingUI.BuildOfferingChoices()`는 현재 신규 Active, Passive, Enhancement를 모두 모은 뒤 무작위 shuffle하고 최대 3개만 남긴다. 따라서 현재 상태로는 Eve B/C/D 확정이 아니다.

### Reward and prisoner flow

- 현재 `reward-stage1-normal` 값은 gold 10, dark trace 10, prisoner chances 0.05/0.80/0.15, manifest 0.70, artifact 0이다.
- Tutorial 1-1에서는 이 reward를 별도 TutorialStage row로 복제한 뒤 prisoner chances를 `0/1/0`, manifest를 `1`로 override한다.
- `RewardPanelUI`는 `PendingPrisonerEnemyNames` 순서로 포로 Button을 만들고 포로 클릭 시 PrisonPanel을 연다.
- `PrisonPanelUI`는 occupied party slot에서 Offering을, next empty party slot에서 MenifestUI를 연다.
- `MenifestUI`는 현재 미보유 catalog monster 중 하나를 무작위 후보로 고르고 `PendingManifestSuccessChance`를 사용한다.
- 성공 popup에서 선택을 확정해야 `TryAddPartyMonster()`와 spawn이 실행된다.

### Stage lifecycle

- `StageManager.RunCurrentDayFlow()`는 encounter rows를 모두 spawn한 뒤 State를 Combat으로 바꾼다.
- 적이 모두 사라지면 reward를 준비하고 State를 RewardReady로 바꾼다.
- 현재 State setter는 event를 발생시키지 않는다.
- 일반 `AdvanceDay()`는 11-Day 전제다.
- Normal combat은 `is_boss_candidate=true` row 중 하나를 무작위 run-assigned boss로 선택한다. 이 분기는 `StageDay.CombatType == "Normal"`일 때만 실행된다.
- Tutorial 1-1/1-2의 StageDay `combat_type`을 `Tutorial`로 두면 새 boss-selection 코드 없이 무작위 boss 선정을 피할 수 있다. guardian captain은 enemy `EncounterRole=Day5Midboss`라 original boss 판정은 유지된다.

### Input and utility

- `PlayerCombatInputController.HandleManualInput()`은 Mouse left pressed/held와 pointer-over-UI를 판정하지만 public input event는 없다.
- `AutoSkillEnabled` property는 존재하지만 변경 event는 없다.
- `InGameUtilityPanelController`는 1x, 1.5x, 2x를 순환하지만 current scale public event는 없다.

### Scene and source files

- `TutorialUI/TutoLine`과 `TutorialUI/TutoEnd`는 inactive다.
- `TutoEnd/Button`은 UGUI Button이며 persistent onClick은 비어 있다.
- `TutoLine/SkipBtn`은 Image-only다.
- `TutorialLine.csv`는 현재 0바이트다.
- `stage_flow/TutorialStage/`는 존재하지만 StageDay/StageEncounter/StageReward CSV는 없다.
- `TutorialFlowManager.cs`와 `TutorialLineView.cs`가 구현되어 기존 TutorialUI 계층을 런타임 바인딩한다.

## Tutorial Day Map

| Tutorial day | Purpose | Encounter | Reward/end |
|---|---|---|---|
| 1-1 | Phase 1 기본 공격 + Phase 2 포로/스킬/현현 | `combat_type=Tutorial`; swordsman 1, priest 1 | tutorial `reward-stage1-normal`: prisoner exactly 2, manifest 100%, artifact 0 |
| 1-2 | Phase 3 Auto/배속 + Phase 4 유물 | `combat_type=Tutorial`; swordsman 2, rogue 1, priest 1, guardian captain 1 | tutorial midboss-equivalent reward, artifact choices 3 |
| 1-3 | 자유 진행 | 현재 Stage1 day3 row 복제 | 현재 Stage1 normal reward 복제 |
| 1-4 | 자유 진행 | 현재 Stage1 day4 row 복제 | 현재 Stage1 normal reward 복제 |
| 1-5 | 최종 자유 진행 | 현재 Stage1 day5 row 복제 | 현재 Stage1 midboss reward 복제; Next가 TutoEnd로 전환 |

Planned StageDay mapping:

```csv
stage,day,day_key,combat_type,encounter_name,reward_rule_name,elite_option_chance,shop_option_enabled,event_roll_enabled,notes
int,int,string,string,string,string,float,bool,bool,string
1,1,tutorial-stage1-day1,Tutorial,tutorial-stage1-day1-basic,reward-stage1-normal,0,false,false,Tutorial basic attack and fixed prisoner reward.
1,2,tutorial-stage1-day2,Tutorial,tutorial-stage1-day2-auto-speed,reward-tutorial-stage1-day2-artifact,0,false,false,Tutorial auto speed and artifact reward.
1,3,tutorial-stage1-day3,Normal,stage1-day3-normal,reward-tutorial-stage1-day3-normal,0,false,false,Copy current Stage1 day3 values.
1,4,tutorial-stage1-day4,Normal,stage1-day4-normal,reward-tutorial-stage1-day4-normal,0,false,false,Copy current Stage1 day4 values.
1,5,tutorial-stage1-day5,Day5Midboss,stage1-day5-midboss,reward-tutorial-stage1-day5-midboss,0,false,false,Copy current Stage1 day5 values; final Next opens TutoEnd.
```

1-3~1-5의 Encounter/Reward 값은 기존 데이터를 복제하지만 source row key는 TutorialStage 내부에서 명확히 분리한다. 특히 1-1의 fixed two-prisoner reward를 1-3/1-4가 잘못 공유하면 안 된다.

### Tutorial 1-1 reward row

기존 StageReward schema를 그대로 사용한다.

```csv
reward_rule_name,combat_type,stage,gold,dark_trace,prisoner_count_1_chance,prisoner_count_2_chance,prisoner_count_3_chance,manifest_success_chance,elite_bonus_prisoners,artifact_choice_count,guaranteed_prisoner_source,notes
string,string,int,int,int,float,float,float,float,int,int,string,string
reward-stage1-normal,Normal,1,10,10,0,1,0,1,0,0,EncounterBoss,Tutorial day 1 fixed two prisoners and guaranteed manifestation.
```

`GuaranteedPrisonerSource`는 현재 runtime 선택 로직에서 사용되지 않는다. 정확히 두 포로를 보장하려면 1-1 encounter의 swordsman/priest row를 각각 `guaranteed_prisoner=true`로 두고 총 enemy row count도 2로 유지한다.

## Dialogue Data Contract

Planned `TutorialLine.csv` schema:

```csv
line_id,phase_id,sequence_id,block_order,text
string,string,string,int,string
```

- 한 row는 Skip/Next 한 번으로 처리되는 한 대사 뭉치다.
- `line_id`는 사용자 명칭을 소문자 hyphen 형태로 보존한다. 예: `line1-1`.
- `(phase_id, sequence_id, block_order)`는 유일해야 한다.
- Action은 CSV 문자열이 아니라 TutorialFlowManager sequence transition으로 구현한다.
- `text` 내부 줄바꿈은 `\n`을 사용한다.
- 맞춤법 교정은 별도 사용자 지시 전 수행하지 않는다.

### Exact dialogue source

#### Line1-1

```text
안녕하세요! 소환자님!\n
저는 소환자님의 안내를도와줄 아리엘\n
 이라고해요!\n
잘부탁드립니다!
```

#### Line1-2

```text
그러면 먼저 이 정령과 감정의 세계에서,\n
저희들을 어떻게 지휘할지 알려드릴게요!
```

#### Line1-3

```text
앗 저기 저 영혼같은 정령들이 보이시나요? \n
여기세계에서는 긍정적인 감정들이 타락해서 생긴 정령들이에요!\n
일단 먼저 저기 있는 적들을 마우스 좌클릭으로 클릭해볼까요?\n
꾹 눌러도 된답니다!
```

#### Line1-4

```text
잘하셨어요! \n
이렇게 타락한 정령들을 처치할 수 있어요!\n
그러면 나머지 정령들도 처치해볼까요?\n
참고로 기본공격의 경우는 주어진 탄창과 재장전 시간이 있으니\n
신중하게 발사하셔야 합니다!
```

#### Line1-5

```text
참 잘하셨어요! \n
자 그럼 이제 보상을 받으러 가볼까요?\n
```

#### Line2-1

```text
여기는 교회입니다!\n
타락한 정령들을 정화하거나 보상을 획득할 수 있죠!\n
먼저 타락한 정령들을 정화해서 스킬을 습득해볼까요?\n
```

#### Line2-2

```text
총 3가지 선택지가 보이실까요?\n
이 선택지의 스킬을 습득해서 소환자를 강화할 수 있습니다!\n
마음에 드는 스킬을 선택해보세요!
```

#### Line2-3

```text
그렇다면 이제는 현현에 대해 배워보도록 하겠습니다!\n
현현이란 정화된 정령을 매게로 새로운 소환자를 정령 소환하는데요!\n
이렇게 소환자님의 진영을 더욱더 강하게 할 수 있습니다!\n
한번 실제 소환자를 소환까지 진행해볼까요?
```

#### Line2-4

```text
자 그렇다면 이제 나머지 보상을 획득하고, \n
다음 스테이지로 넘어가볼까요?\n
```

#### Line3-1

```text
잠깐! 혹시 클릭하는데 불편하시다고요? \n
 \n
그렇다면 왼쪽에 있는 Auto 버튼을 클릭해보실까요? \n
이 버튼은 자동으로 적을 타게팅 할 수 있게됩니다! \n
하지만 수동으로 적을 맞춰야 할때도 존재하니 맹신은 금물!\n
```

#### Line3-2

```text
또한, 이곳세계에서는 시간의 정령의 힘으로\n
시간을 빠르게 설정할 수 있어요!\n
Auto 버튼 왼쪽에 있는 버튼을 클릭해볼까요? \n
각각 1.5배속 2배속이랍니다!\n
한번 1.5배속, Auto 모드 활성화를 해볼까요?
```

#### Line3-3

```text
잘하셨어요! 그러면 저 타락한 정령들을 정화해보자고요!\n
```

#### Line4-1

```text
대단하네요! 이번에는 유물에 대해 알아볼게요!\n
유물이란 예전 정령이 쓰던 물품으로 강력한 효과를 지니고 있습니다!\n
각 유물마다 시너지 효과를 보유하고 있어서\n
같은 시너지를 모으는걸 추천드릴게요!
```

#### Line4-2

```text
그렇다면 한번 유물 보상을 선택해볼까요? \n
```

#### Line4-3

```text
잘하셨어요!\n
그렇다면 이제 저희 소환자들을 지휘해서 이 세계를 탈출하자고요!\n
```

## Skip And Next Contract

| State | Button text | Click |
|---|---|---|
| Typing | `SKIP!` | 현재 block 전체만 즉시 출력 |
| Complete | `Next!` | 다음 block 또는 현재 sequence의 다음 Action으로 진행 |

- 다음 block 시작 시 즉시 `SKIP!`으로 복귀한다.
- double click으로 두 block을 건너뛰지 않는다.
- Action wait 중에는 TutoLine과 SkipBtn을 숨긴다.
- typewriter는 `Time.unscaledDeltaTime`을 사용한다.

## Authoritative Tutorial Flow

### Entry

```text
Tutorial 버튼
  -> StartRun("eve", RunMode.Tutorial)
  -> 기존 async InGameScene load
  -> Eve Tutorial RunSession 생성
  -> TutorialStage 선택
  -> 일반 Stage auto-start 보류
  -> timeScale 0
  -> TutoLine on / TutoEnd off
```

### Phase 1 — Basic attack

1. Line1-1 출력.
2. Line1-2 출력.
3. 마지막 Next 후 TutoLine off, timeScale 1.
4. Tutorial 1-1 encounter spawn 시작.
5. StageManager가 Combat state가 되면 모든 지정 monster spawn 완료로 판정.
6. 즉시 tutorial pause, Line1-3 출력.
7. Line1-3 마지막 Next 후 resume.
8. Eve의 기본 공격으로 enemy에게 실제 damage가 적용될 때까지 대기.
9. hit event를 한 번만 소비하고 pause, Line1-4 출력.
10. Line1-4 마지막 Next 후 resume.
11. 모든 enemy가 제거되어 RewardReady가 될 때까지 대기.
12. pause, Line1-5 출력.

Phase 1 hit 판정은 mouse click 자체가 아니라 `DamageApplied`의 실제 적 피해다. 잘못된 위치 click은 성공 처리하지 않는다.

### Phase 2 — Reward, skill and manifestation

1. Line1-5 완료 뒤 RewardPanel visible을 확인하고 Line2-1 출력.
2. Line2-1 완료 후 첫 번째 미소비 prisoner reward만 허용.
3. 첫 prisoner의 PrisonPanel에서는 Eve occupied slot만 interactable. next empty manifest slot은 비활성.
4. Eve slot 선택 후 OfferingPanel을 연다.
5. Tutorial-only Offering source로 `eve-b`, `eve-c`, `eve-d`를 순서 고정해 정확히 세 개 표시.
6. OfferingPanel visible 확인 후 Line2-2 출력.
7. Line2-2 완료 후 세 skill Button을 허용.
8. 하나의 skill이 실제 RunSession에 추가되면 첫 prisoner reward consumed, RewardPanel 복귀 완료를 기다린다.
9. pause, Line2-3 출력.
10. Line2-3 완료 후 두 번째 미소비 prisoner reward만 허용.
11. 두 번째 PrisonPanel에서는 occupied Offering slot을 비활성하고 next empty manifest slot만 허용.
12. manifest chance는 Tutorial 1-1 reward의 `1.0`을 사용한다.
13. success popup의 Choice로 `TryAddPartyMonster()`가 true이고 실제 spawn이 끝나야 현현 완료다.
14. RewardPanel 복귀 완료 후 pause, Line2-4 출력.
15. Line2-4 완료 후 gold/dark trace 등 남은 reward를 허용한다.
16. 두 prisoner action과 요구 reward가 완료된 뒤 NextBtn을 허용한다.
17. NextBtn click으로 Tutorial 1-2를 시작한다.

현재 OfferingUI는 무작위 shuffle을 사용하므로 전체 Offering 알고리즘을 바꾸지 않는다. Tutorial mode에서만 explicit skill ID 세 개를 전달하는 좁은 API를 추가한다.

### Phase 3 — Auto and speed

1. Tutorial 1-2 encounter spawn 완료 후 Combat state 진입.
2. 유효한 non-UI manual left click/hold가 처음 감지될 때 one-shot timer 시작.
3. `WaitForSecondsRealtime(2f)` 뒤 pause.
4. Line3-1 출력.
5. Line3-2 출력.
6. Line3-2 마지막 Next 후 resume하고 Auto button, Time button을 허용.
7. 아래 두 상태가 동시에 참일 때 성공:
   - `AutoSkillEnabled == true`.
   - current time scale이 `1.5x` 또는 `2x`.
8. 성공 즉시 pause, Line3-3 출력.
9. Line3-3 마지막 Next 후 현재 1.5x/2x와 Auto 상태를 유지한 채 resume.
10. 모든 enemy 제거와 RewardPanel visible을 기다린다.
11. pause하고 Phase 4로 이동.

기존 Auto 버튼의 실제 코드 의미는 선택 플레이어의 auto skill mode다. 사용자 문구의 “자동 타게팅” 체감은 Play Mode에서 확인해야 한다.

### Phase 4 — Artifact

1. RewardPanel 위에서 Line4-1 출력.
2. Line4-2 출력.
3. Line4-2 마지막 Next 후 artifact reward만 우선 허용.
4. ArtifactPanel에서 artifact 선택.
5. PrisonPanel에서 수령 party member 선택.
6. `RunSession.TryAcquireArtifact()` true와 RewardPanel 복귀를 완료 조건으로 사용.
7. pause, Line4-3 출력.
8. Line4-3 마지막 Next 후 남은 reward와 NextBtn을 허용.
9. NextBtn으로 Tutorial 1-3 시작.

### Tutorial 1-3 to 1-5 and ending

- 1-3, 1-4, 1-5는 추가 TutoLine 없이 정상 combat/reward loop를 사용한다.
- TutorialStage CSV에 복제한 day3/day4/day5 data만 사용한다.
- 1-5 RewardPanel Next 전까지 일반 reward 처리를 유지한다.
- 1-5 Next click은 `AdvanceDay()`를 호출하지 않는다.
- TutorialFlowManager가 다음 종료 순서를 실행한다.

```text
Reward/transient panels off
TutoLine off
timeScale 0
TutoEnd on
TutoEnd/Button only interactable
```

TutoEnd/Button click:

```text
button interactable false
event unsubscribe
tutorial input lock clear
timeScale 1 + fixedDeltaTime restore
existing MainMenu scene path load
```

## Responsibility Boundaries

### MainMenuUIManager

- 공용 `StartRun(monsterName, runMode)`와 비동기 씬 로드만 소유.
- Tutorial Phase를 소유하지 않는다.

### StartContext and RunSession

- monster key와 RunMode를 전달/보존.
- StartContext clear 뒤에도 RunSession에서 `IsTutorial`을 조회할 수 있어야 한다.

### StageManager

- TutorialStage 선택, encounter spawn, Combat/RewardReady, day progression을 소유.
- State 변경 event와 tutorial final-day interception을 제공.
- dialogue/UI block 순서를 소유하지 않는다.

### TutorialFlowManager — implemented

- Phase/substep, event wait, pause, input gate, panel gate, final TutoEnd를 소유.
- 공격/보상 실제 mutation을 직접 수행하지 않는다.

### TutorialLineView — implemented

- CSV block 표시, typewriter, Skip/Next UI만 소유.
- Phase 조건을 판단하지 않는다.

### Existing UI controllers

- RewardPanelUI: reward Button/Next availability와 visible state event.
- PrisonPanelUI: tutorial-only allowed action mode `OfferingOnly`/`ManifestOnly`/`ArtifactRecipient`.
- OfferingUI: normal random source와 분리된 tutorial explicit B/C/D source.
- MenifestUI: 기존 100% chance와 commit flow 유지, successful commit event만 노출.
- Utility controller/input controller: 상태 조회와 change/input event 노출.

## Tutorial Data Files

Implemented files:

```text
Pakuri/Assets/CSVdata/Tutorial/TutorialLine.csv
Pakuri/Assets/CSVdata/stage_flow/TutorialStage/StageDay.csv
Pakuri/Assets/CSVdata/stage_flow/TutorialStage/StageEncounter.csv
Pakuri/Assets/CSVdata/stage_flow/TutorialStage/StageReward.csv
```

- 세 Stage file은 기존 header/type schema를 그대로 사용한다.
- TutorialLine과 TutorialStage는 runtime catalog/editor sync/validation에 명시적으로 추가한다.
- normal StageDefinition과 별도 `GameDataCatalog.TutorialStage`로 빌드한다.
- tutorial enemy/reward key는 별도 StageDefinition 내부에서만 lookup한다.

## Expected Implementation Surface

### Existing files expected to change

- `Pakuri/Assets/Scripts/UI/MainMenu/MainMenuUIManager.cs`
- `Pakuri/Assets/Scripts/GameFlow/Stage/StageManager.cs`
- `Pakuri/Assets/Scripts/GameFlow/RunSession.cs`
- `Pakuri/Assets/Scripts/Units/Runtime/Input/PlayerCombatInputController.cs`
- `Pakuri/Assets/Scripts/UI/InGame/UtilityPanel/InGameUtilityPanelController.cs`
- `Pakuri/Assets/Scripts/UI/InGame/InGameUIManager.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/RewardPanelUI.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/PrisonPanelUI.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/OfferingUI.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/MenifestUI.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Reward/ArtifactUI.cs` or authoritative acquisition boundary only if needed.
- `Pakuri/Assets/Scripts/Loading/Parsing/CsvRuntimeCatalog.cs`
- `Pakuri/Assets/Scripts/Loading/Parsing/EditorSync/CsvCatalogEditor.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.cs`
- `Pakuri/Assets/Scripts/Loading/Generation/StageDefinitionBuilder.cs`
- `Pakuri/Assets/Scripts/Loading/RuntimeCatalog/GameDataCatalog.cs`
- `Pakuri/Assets/CSVdata/Tutorial/TutorialLine.csv`

### Implemented new files

- `Pakuri/Assets/Scripts/GameFlow/Tutorial/TutorialFlowManager.cs`
- `Pakuri/Assets/Scripts/UI/InGame/Tutorial/TutorialLineView.cs`
- Loading ownership 아래 TutorialLine runtime definition/parser 또는 builder.
- TutorialStage의 StageDay/StageEncounter/StageReward CSV 세 개.

## Implementation Order

### Phase A — Run mode and Eve entry

- Normal/Tutorial RunMode.
- 공용 async StartRun.
- Tutorial은 `eve` 고정.
- RunSession에 mode 보존.

### Phase B — Tutorial data loading

- TutorialLine schema/parser/validation.
- TutorialStage 세 source와 separate StageDefinition.
- 1-1/1-2 custom rows, 1-3~1-5 inspected Stage1 row 복제.
- 1-1/1-2 StageDay `combat_type=Tutorial`로 현재 Normal-only random boss branch를 피한다. 일반 boss 코드는 변경하지 않는다.

### Phase C — Dialogue view

- existing TutoLine binding.
- SkipBtn Button 추가/저장.
- unscaled typewriter와 Skip/Next.
- exact 15 dialogue blocks load/order test.

### Phase D — Director and stage events

- StateChanged, RewardPanel visible, Next requested.
- Phase1 spawn-complete pause, hit, clear flow.
- 1-5 final Next interception.

### Phase E — Prisoner tutorial gates

- first prisoner OfferingOnly.
- Eve B/C/D explicit Offering.
- second prisoner ManifestOnly.
- 100% manifest commit event.
- remaining reward/Next unlock.

### Phase F — Auto, speed, artifact

- valid manual input event and realtime 2-second delay.
- Auto changed and time scale changed events.
- simultaneous Auto + 1.5x/2x gate.
- artifact actual acquisition gate.

### Phase G — End and compatibility

- 1-3~1-5 normal loop within TutorialStage.
- TutoEnd/Button MainMenu return.
- event/time/input cleanup.
- Normal Run regression.

## Edge Cases And Failure Strategy

- TutorialLine missing/empty/duplicate line_id: tutorial combat를 시작하지 않고 정확한 validation error와 safe MainMenu exit 제공.
- TutorialStage missing row/reference: catalog validation failure.
- Line1-2 뒤 timeScale을 풀지 않아 spawn coroutine이 멈추는 문제를 금지. spawn 중에는 반드시 resume한다.
- Phase1 click이 빗나감: hit 성공 아님. 계속 대기.
- Phase1 enemy가 비정상 제거돼 DamageApplied가 없었는데 전멸: configuration/runtime error로 처리하고 진행을 자동 위조하지 않는다.
- 첫 prisoner에서 manifest path 진입 금지.
- 두 번째 prisoner에서 Offering path 재진입 금지.
- Eve B/C/D 중 이미 보유한 skill이 있으면 tutorial entry 초기 상태 오류다. 새 Tutorial Run은 Eve 기본 상태에서 시작해야 한다.
- 현현 candidate가 없거나 party full이면 Phase2를 완료하지 않고 명시적 오류/exit를 제공.
- manual click timer 중 Stage가 끝나면 timer를 취소하고 RewardReady 흐름으로 안전 전환.
- Auto와 speed는 어느 순서로 눌러도 되지만 둘이 동시에 요구 상태일 때만 완료.
- speed가 2x여도 성공. 1x면 미완료.
- artifact pool이 비었거나 수령 실패면 Phase4 미완료.
- 1-5 Next 연타로 TutoEnd와 Day 1-6이 동시에 실행되지 않도록 one-shot guard.
- scene unload/disable 때 coroutine 중단, event 해제, timeScale/fixedDeltaTime 복구.

## Acceptance Criteria

### Functional

- Tutorial 클릭은 Eve로 InGameScene에 입장한다.
- 일반 Run은 선택 캐릭터를 유지한다.
- Line1-1~Line4-3 총 15개 block이 CSV 원문 순서대로 출력된다.
- Skip/Next가 현재 block만 처리한다.
- 1-1에 swordsman 1, priest 1만 spawn되고 tutorial random boss가 지정되지 않는다.
- 두 enemy가 모두 spawn된 뒤 Line1-3에서 pause된다.
- Eve의 실제 enemy hit 뒤 Line1-4가 나온다.
- enemy 전멸 뒤 Line1-5와 RewardPanel 흐름이 나온다.
- 1-1 reward prisoner 수는 항상 2, manifest chance는 100%다.
- 첫 prisoner는 Eve B/C/D Offering만 가능하다.
- 두 번째 prisoner는 manifest만 가능하며 실제 party add/spawn 뒤 완료된다.
- 남은 reward 처리 후 Next로 1-2에 진입한다.
- 1-2 encounter 구성은 swordsman 2, rogue 1, priest 1, guardian captain 1이다.
- 첫 유효 manual input 2초 뒤 Line3-1이 나온다.
- Auto on과 1.5x 또는 2x가 동시에 만족돼야 Line3-3이 나온다.
- 1-2 적 전멸 후 유물 reward가 표시된다.
- 실제 artifact acquisition 뒤 Line4-3이 나온다.
- 1-3~1-5가 TutorialStage data로 진행된다.
- 1-5 final Next는 Day 1-6 대신 TutoEnd를 활성화한다.
- TutoEnd/Button은 한 번만 MainMenuScene을 로드하고 timeScale을 1로 복구한다.

### Experiential

- 각 대사 뒤 무엇을 클릭하거나 처치해야 하는지 명확하다.
- 첫 포로에서 현현, 두 번째 포로에서 Offering으로 잘못 빠질 수 없다.
- Auto/배속은 버튼을 직접 눌러 학습한다.
- 튜토리얼 뒤 1-3~1-5 자유 진행으로 배운 기능을 반복 체험한다.

### Compatibility

- 기존 Stage1/Stage2 CSV 값과 normal catalog 결과가 바뀌지 않는다.
- normal Offering random choice는 유지된다.
- normal random boss selection은 유지된다.
- normal RewardPanel/PrisonPanel/Menifest/Artifact 흐름은 유지된다.
- MainMenu Run/Tutorial 0.15-second layout transition은 유지된다.

## Verification Expected From Code Builder

### Static/editor

- changed/new file inventory.
- TutorialLine 15 block count, unique line_id, phase/sequence ordering, UTF-8/column validation.
- TutorialStage Day 1-1~1-5 mapping과 Encounter/Reward reference validation.
- 1-1 exact 2 enemies, 1-2 exact 5 enemies count validation.
- 1-1 prisoner chance sum 1, count2=1, manifest=1 validation.
- Eve B/C/D skill ID lookup validation.
- runtime catalog sync.
- Runtime/Editor build, changed script diagnostics.
- InGameScene missing script/broken reference validation.
- task-owned markdown trailing whitespace 0.
- Git work-tree 확인 후 task-owned diff check.

### User-owned Play Mode

- Normal Run regression.
- Eve tutorial entry.
- 모든 15개 dialogue와 action boundary.
- first OfferingOnly/second ManifestOnly.
- manual click 후 실제 2초 timing.
- Auto + 1.5x와 Auto + 2x 두 성공 조합.
- artifact acquisition.
- 1-3~1-5 진행과 TutoEnd MainMenu 복귀.

## Related Board Files

- `boards/UI/UI_BLACKBOARD.md`
- `boards/RUN/RUN_BLACKBOARD.md`
- `boards/DATA/DATA_BLACKBOARD.md`

## Evidence

- 2026-08-26 code/data inspection confirmed Eve catalog key, four requested enemy IDs, Eve B/C/D active skills, current reward probabilities, two-prisoner roll contract, random Offering behavior, PrisonPanel Offering/Manifest split, manifest 100% data hook, Stage spawn/clear states, manual mouse input, Auto state, and 1/1.5/2 time scales.
- 2026-08-26 initial scene/filesystem inspection confirmed existing TutoLine/TutoEnd/Button and, before implementation, empty TutorialLine/TutorialStage sources and absent tutorial scripts.
- 2026-08-26 current Stage1 data inspection confirmed reusable day3/day4/day5 encounter/reward mappings.

## History

- 2026-08-26: Initial design used Ariel entry, six broad phases, 1.5x-only speed success, and TutoEnd after Phase 6.
- 2026-08-26: User replaced the flow with Eve entry, exact Line1-1~Line4-3 text/actions, fixed two-prisoner 100% manifestation reward, combined Auto/speed phase, artifact phase, and Tutorial 1-3~1-5 continuation.
- 2026-08-26: Designer inspected the revised code/data paths, recorded the necessary tutorial-only gates/events, and replaced the handoff before implementation began.
- 2026-08-26: Code Builder implemented Phase A~F in commits `27699cf`, `c145f5f`, `655c5ca`, `fd1c527`, `335de5f`, and `44956b4`.
- 2026-08-26: Phase G added Tutorial 1-5 final Next interception, TutoEnd/Button one-shot MainMenu return, time restoration, and Normal-mode reward compatibility. C# build completed with 0 errors and 2 existing reference warnings; final Unity MCP checks were unavailable because no Editor instance was connected.
