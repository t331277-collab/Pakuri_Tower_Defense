# UI_BLACKBOARD

## Task: 2026-08-07 Chosen One Acquisition And HUD Design

### Task title

선택받은자 유물을 정상 ArtifactPanel 후보에 추가하고 기존 다중 시너지 HUD를 재사용한다.

### Goals

- 정상 보상 후보에 `chosen-one`을 추가한다.
- 기존 RunSession 중복·용량 제한과 최대 3개 후보를 유지한다.
- 같은 시너지 누적, 다른 시너지 컨테이너 Y `-93.3` 배치를 재사용한다.

### Constraints

- `InGameInfoUI`의 이미 구현된 다중 시너지 로직은 수정하지 않는다.
- 존재하지 않는 chosen-one 시너지 icon asset 경로를 임의로 작성하지 않는다.
- Designer 단계에서는 UI 코드·Scene·Prefab을 수정하지 않는다.

### Role Owner

Designer.

### Status

획득·HUD 설계 완료. Code Builder 구현과 icon asset 결정 대기.

### Next Actions

- `ArtifactUI.PrepareChoices`의 허용 시너지에 `chosen-one`을 추가한다.
- 사용자가 실제 chosen-one 시너지 icon asset을 제공하거나 기존 asset 사용을 지정한다.

### Evidence

- `ArtifactUI.PrepareChoices`는 현재 `spirit-contract`, `executioner`만 필터링한다.
- `InGameInfoUI.RefreshArtifactDisplay`는 모든 보유 유물을 `SynergyName`별로 그룹화한다.
- `InGameInfoUI.GetArtifactContainer`는 새 시너지 컨테이너를 이전 컨테이너보다 Y `93.3` 아래에 배치한다.
- `artifact_synergies.csv`의 chosen-one icon 필드는 현재 비어 있다.

### History

- 2026-08-07: Designer가 선택받은자 획득 필터 변경과 HUD 무수정 재사용 경계를 기록했다.

## Current State

The field-unit Registry ownership migration is implemented. UI consumers now query `UnitSpawnManager` instead of `InGameCombatManager.UnitRegistry`.

The previous UI and RunScene UI boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/UI/`.

For new UI work, inspect the exact current scripts, scenes, prefabs, UXML, USS, or assets first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-08-07 MainMenu GameStart Raycast Fix

### Task title

MainMenu `GameStart` 터치 영역의 Raycast 차단 해소.

### Goals

- `MosterSelectUI/GameStart` 배경 Image가 전체 버튼 영역을 받도록 한다.
- 부모 배경과 선택 결과 텍스트가 버튼 입력을 가로채지 않도록 한다.

### Constraints

- `MainMenuScene.unity`의 Raycast 설정 네 개만 변경한다.
- 계층, Transform, Button, 텍스트, 스크립트는 변경하지 않는다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. 네 컴포넌트의 정적 값 확인과 Unity 씬 검증을 통과했다.

### Next Actions

- 몬스터 선택 후 `GameStart` 버튼의 넓은 영역 터치를 Play Mode에서 확인한다.

### Evidence

- `MainMenuScene.unity:1176`의 Image `914212245` (`GameStart`)는 `m_RaycastTarget: 1`이다.
- `MainMenuScene.unity:2719`의 Image `1780514919` (`MosterSelectUI` 부모)는 `m_RaycastTarget: 0`이다.
- `MainMenuScene.unity:1525`의 TMP `1101350769` (`Name`)와 `MainMenuScene.unity:2575`의 TMP `1699277527` (`Desc`)는 모두 `m_RaycastTarget: 0`이다.
- 네 ID를 YAML로 직접 검사한 출력은 각각 `1, 0, 0, 0`이었다.
- Unity MCP `manage_scene(validate, Assets/Scenes/NewScene/MainMenuScene.unity)` 결과: `totalIssues: 0`, `missingScripts: 0`, `brokenPrefabs: 0`.

### History

- 2026-08-07: Code Builder가 부모 Image·Standing 텍스트의 입력 차단을 끄고 `GameStart` 배경 Image의 Raycast를 켰다.

## Task: 2026-08-07 MainMenu Monster Standing Text

### Task title

몬스터 선택 시 `MonsterStanding/Name`·`Desc` 텍스트 표시.

### Goals

- 선택 몬스터의 `display_name`을 `Name`에 표시한다.
- 선택 몬스터의 `role_summary`를 `Desc`에 표시한다.

### Constraints

- 기존 `MainMenuUIManager.SelectMonster`와 Runtime catalog를 재사용한다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. Unity MCP 스크립트 검증과 솔루션 빌드를 통과했다.

### Next Actions

- Play Mode에서 5개 몬스터 클릭 후 Name/Desc 전환을 확인한다.

### Evidence

- `MainMenuUIManager`가 `MosterSelectUI/MonsterStanding/Name`·`Desc`의 `TextMeshProUGUI`를 바인딩한다.
- `SelectMonster`가 `MonsterDefinition.DisplayName`·`RoleSummary`를 각 `.text`에 할당한다.
- Unity MCP hierarchy에서 두 오브젝트와 `TextMeshProUGUI` 컴포넌트를 확인했다.
- Unity MCP `validate_script` 결과 diagnostics 0개, 콘솔 error/warning 0개.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` 완료: 오류 0개, 기존 참조 충돌 경고 2개.

### History

- 2026-08-07: Code Builder가 기존 몬스터 CSV 표시 데이터 계약을 MainMenu Standing 텍스트에 연결했다.

## Task: 2026-08-07 MainMenu Monster Standing Selection

### Task title

MainMenuScene 몬스터 버튼 선택 시 `MonsterStanding` 표시.

### Goals

- Ariel, Eve, Rin, Sein, Vega 버튼 클릭 시 `MosterSelectUI/MonsterStanding`을 활성화한다.
- 선택한 몬스터 정의의 `Image` Sprite를 `MonsterStanding` Image에 할당한다.

### Constraints

- 기존 `MainMenuUIManager` 버튼 바인딩과 `GameDataLoader` 카탈로그를 재사용한다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. 솔루션 빌드와 씬·CSV 정적 검증을 통과했다.

### Next Actions

- Play Mode에서 5개 버튼 클릭 후 Standing 표시와 이미지 전환을 확인한다.

### Evidence

- `MainMenuUIManager`가 `MosterSelectUI/MonsterStanding` GameObject/Image를 바인딩한다.
- `SelectMonster`가 `GameDataLoader.CurrentCatalog.GetMonster(monsterName).Image`를 할당하고 `SetActive(true)`를 호출한다.
- `MainMenuScene.unity`의 `MonsterStanding`은 존재하며 초기 `m_IsActive: 0`이다.
- `monsters.csv` 5개 몬스터 행의 `Image` 경로가 실제 파일로 확인됐다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` 완료: 오류 0개, 기존 참조 충돌 경고 2개.

### History

- 2026-08-07: Code Builder가 기존 카탈로그·버튼 흐름에 Standing 이미지 표시를 연결했다.

## Task: 2026-08-07 Critical Damage Popup Color

### Task title

대상 유닛 프리팹의 `Damage` 팝업을 치명타일 때 빨간색으로 표시한다.

### Goals

- 기존 대상 표시 경로를 재사용해 `IsCritical`만 `DamageNumberPopup`까지 전달한다.
- 치명타 팝업은 상승·Fade 중에도 빨간색을 유지한다.

### Constraints

- `caster` 조회나 새 팝업 탐색 경로를 추가하지 않는다.
- 피해 계산·치명타 판정·프리팹 계층은 변경하지 않는다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. Core/Editor 솔루션 빌드 및 diff 검사를 통과했다.

### Next Actions

- Play Mode에서 일반 피해는 기존 색상, 치명타 피해는 빨간색인지 확인한다.

### Evidence

- `InGameCombatManager.ApplyDamage`가 `result.Target`으로 대상 엔트리를 찾고 `result.IsCritical`을 `ShowDamage`에 전달한다.
- `CombatUnitRegistry`, `MonsterActor`, `EnemyActor`, `UnitHpBar`가 같은 bool을 기존 `Damage` 팝업 소유 객체로 전달한다.
- `DamageNumberPopup.SpawnPopup`이 치명타에 `Color.red`를 선택하고 `ActiveDamagePopup.StartColor`에도 저장해 Fade 색상 재설정을 방지한다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- 대상 6개 스크립트의 `git diff --check`가 통과했다.

### History

- 2026-08-07: 사용자가 대상 프리팹의 `Damage` 표시만 치명타 시 빨간색으로 바꾸도록 구현을 요청했다.
- 2026-08-07: Code Builder가 기존 대상 표시 경로에 `isCritical`을 전달하고 팝업 색상만 분기했다.

## Task: 2026-08-06 Multi-Synergy Artifact HUD Design

### Task title

공통 `Artifact_Container`를 시너지 종류별로 재사용하고 추가 종류를 아래로 93.3만큼 복제 배치한다.

### Goals

- 정령계약 한 종류 고정을 제거한다.
- 보유 유물을 `SynergyName`별로 집계해 각 시너지 icon/count/2·4·6·8 상태를 표시한다.
- 첫 종류는 원본, 이후 종류는 마지막 슬롯 아래에 복제한다.

### Constraints

- 원본과 복제본은 캐시하고 Refresh마다 파괴/재생성하지 않는다.
- 각 다음 슬롯의 `localPosition.y`는 원본에서 `-93.3 * index`로 계산한다.
- 현재 계층에 없는 개별 유물 효과 설명 UI가 존재한다고 가정하지 않는다.
- Designer 단계에서는 Scene과 UI 코드를 수정하지 않는다.

### Role Owner

Code Builder.

### Status

씬 계층과 코드 감사, 구현 완료. 개별 유물 설명 표시 범위는 기존 설계대로 HUD 범위에서 제외한다.

### Next Actions

- `InGameInfoUI`의 단일 필드를 슬롯 목록으로 바꾸고 원본을 복제 원형으로 사용한다.
- 정령계약+처형관 동시 보유 시 두 번째 슬롯의 Y 오프셋과 단계 색을 Play Mode에서 확인한다.

### Evidence

- `InGameInfoUI.DisplayedSynergyName`은 현재 `spirit-contract`로 고정돼 있다.
- 활성 씬 `HUD/Artifact_Container` 루트는 `Transform`이며 자식은 `Artifact`, `Image`다.
- `Image` 아래에는 `Icon`, `Cur`, `Lv2`, `Lv4`, `Lv6`, `Lv8`이 있지만 개별 유물 이름/설명용 Text 목록은 없다.
- 현재 씬 원본 위치와 코드 경로를 직접 검사했으며 추가 슬롯 배치는 `localPosition` 기준으로 설계했다.
- `InGameInfoUI`는 보유 순서대로 `ArtifactDefinition.SynergyName`을 그룹화하고, 첫 시너지는 원본 컨테이너에 표시한다.
- 이후 시너지는 마지막 컨테이너를 `Instantiate`한 뒤 이전 컨테이너의 `localPosition.y - 93.3f`로 배치하고 목록에 캐시한다.
- `ArtifactUI.PrepareChoices`는 현재 구현 대상인 `spirit-contract`와 `executioner`만 보상 후보로 허용한다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-06: 사용자가 시너지 종류마다 공통 Artifact_Container를 복제하고 Y를 93.3씩 낮추도록 요청했다.
- 2026-08-06: Designer가 현재 씬에서 가능한 표시 범위와 추가 UI가 필요한 범위를 분리했다.
- 2026-08-07: 같은 시너지는 한 컨테이너에 누적하고 다른 시너지는 마지막 컨테이너 복제 후 Y `-93.3`으로 배치하는 규칙을 전용 구현 설계 문서에 기록했다.
- 2026-08-07: Code Builder가 다중 시너지 보상 후보와 HUD 컨테이너 캐시/복제 배치를 구현하고 정적 빌드를 통과시켰다.

## Task: 2026-08-05 Spirit Contract Artifact HUD Display

### Task title

Display the Spirit Contract artifact count, icon and cumulative level state in the existing single Artifact_Container.

### Goals

- Show `HUD/Artifact_Container` only when the party owns one or more `spirit-contract` artifacts.
- Count owned Spirit Contract artifacts across `RunSession.PartyMembers` and write the count to `Image/Cur/Text (TMP) (1)`.
- Resolve the synergy icon from `artifact_synergies.csv` through the existing CSV -> Definition -> runtime catalog path.
- Turn `Lv2`, `Lv4`, `Lv6` and `Lv8` text white at counts 2, 4, 6 and 8 respectively.
- Keep synergy effect execution out of this display-only phase.

### Constraints

- Display only the confirmed `spirit-contract` synergy because the scene has one `Artifact_Container`.
- Reuse `InGameInfoUI.Refresh`/`InGameUIManager.RefreshInfo`; do not add a second HUD manager or an artifact event system.
- Preserve the existing scene hierarchy and the user's `Lv2/Lv4/Lv6/Lv8` object names.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and statically verified. Unity MCP live validation timed out; the serialized runtime catalog asset contains the required Sprite reference and Play Mode remains user-owned.

### Next Actions

- User verifies artifact acquisition/debug grant updates the single HUD container, count, icon and cumulative level colors in `InGameScene` Play Mode.

### Evidence

- `InGameInfoUI.cs:12,35-47,49-70,136-246` binds and refreshes the existing `HUD/Artifact_Container` paths, scans every party member's `ArtifactState`, resolves catalog artifact/synergy Definitions and applies the 2/4/6/8 thresholds.
- `artifact_synergies.csv` has the `Icon_Image` `asset_path` column and the Spirit Contract path `Assets/Image/UI/Artifact/ChatGPT Image 2026년 8월 5일 오후 03_39_55.png`.
- `CsvRowParser.ArtifactSynergyRow`, `CsvAssetReferenceCollector`, `GameDataCatalogBuilder.Artifacts` and `ArtifactSynergyDefinition.Icon` carry the icon through the existing data pipeline.
- `InGameScene.unity` contains one `HUD/Artifact_Container`; it is serialized inactive until the owned count is positive.
- `CsvRuntimeCatalog.asset` contains the matching UI artifact Sprite path, GUID `8b537b0e0f060644cb22f8d33a5bbf01` and first-sprite fileID `1672546330545570191`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings; CSV column-count and icon-path checks passed.
- Unity `Validate CSV Source Data` and console requests were attempted after refresh but returned transport timeouts; no Play Mode state was changed.

### History

- 2026-08-05: Code Builder added the data-driven synergy icon column/pipeline and connected the existing HUD refresh to Spirit Contract count and threshold display without implementing synergy effects.

## Task: 2026-08-05 Artifact Debug Acquisition Flow

### Task title

Add a DebugUI synergy-to-artifact acquisition flow.

### Goals

- Open `ArtifactDebugUI` from the `Debug/DebugPanel/DebugUI/ArtifactDebug` button.
- List catalog artifact synergies on `Debug/DebugPanel/ArtifactDebugUI/A~JBtn` and list the selected synergy's artifacts on the sibling path `Debug/DebugPanel/ArtifactAchiveDebugUI/A~JBtn`.
- Write each display name to the authored TMP text child and leave excess slots visible but non-interactable with no listener action.
- Route an artifact choice through the existing `ArtifactAchiveDebugUI/1P~5P` receiving-unit buttons without activating PrisonPanel.
- Write each current party monster display name to the corresponding debug unit button TMP text.
- Disable already-owned artifact buttons and close `ArtifactAchiveDebugUI` after a successful debug grant without opening the normal RewardPanel completion flow.

### Constraints

- Reuse `GameDataLoader.CurrentCatalog` and `RunSession.CanAcquireArtifact`/`TryAcquireArtifact`.
- Do not add a second artifact ownership or duplicate/capacity rule.
- Do not change CSV data or add new scene objects; the existing `ArtifactDebug`, `ArtifactDebugUI`, sibling `ArtifactAchiveDebugUI`, A-J buttons and Close object are the binding targets.
- Unity Play Mode verification remains user-owned.
- The per-skill modifier buttons are optional; deleting them must not disable the artifact debug binding.

### Role Owner

Code Builder

### Status

Implemented and locally verified. Play Mode verification remains user-owned.

### Next Actions

- User verifies DebugUI → synergy → artifact → 1P-5P acquisition in Play Mode, including duplicate and full-artifact recipients.

### Evidence

- `DebugUI.cs` binds the existing ArtifactDebug button at `Debug/DebugPanel/DebugUI/ArtifactDebug`; the user-reorganized panels are siblings at `Debug/DebugPanel/ArtifactDebugUI` and `Debug/DebugPanel/ArtifactAchiveDebugUI`.
- `DebugUI.cs` reads `ArtifactSynergies` and `Artifacts` from the loaded catalog; current CSV inspection reports 6 synergies, 10 artifacts for each of the first 5 synergies, and 0 for Tracker.
- `DebugUI.cs` binds the existing `ArtifactAchiveDebugUI/1P~5P` buttons and their TMP text children, writes party monster names, and calls `RunSession.TryAcquireArtifact` directly.
- `RunSession.CanAcquireArtifact`/`TryAcquireArtifact` remain the only debug grant authority and enforce occupied-slot, duplicate and three-artifact capacity rules.
- Successful debug grants emit a `[ArtifactDebug] ... 획득` Unity log and deactivate `ArtifactAchiveDebugUI`; normal RewardPanel acquisition still uses `PrisonPanelUI`.
- Unity `InGameScene` path inspection found all requested targets; scene validation returned 0 issues, 0 missing scripts and 0 broken prefabs.
- The previous nested acquisition path no longer resolves; the current sibling paths resolve the ArtifactDebugUI panel and the ArtifactAchiveDebugUI A-J targets.
- `DebugUI.cs` now uses the existing `UiBindingUtility.BindOptionalChild` for per-skill modifier buttons, so missing modifier buttons cannot make `BindObject()` fail before ArtifactDebug listeners are registered.
- Post-change live scene inspection resolved all 10 synergy A-J and 10 artifact A-J paths; the old nested panel path resolved 0 objects.
- All six `EmodifierBtn` objects found under `ArtifactAchiveDebugUI/E~JBtn` were deleted; no `EmodifierBtn` remains below `ArtifactAchiveDebugUI` while unrelated DebugUI modifier buttons remain.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings.
- Unity script validation reported 0 errors for `DebugUI.cs`, `InGameUIManager.cs` and `PrisonPanelUI.cs`; only the existing generic string-concatenation warnings were reported.

### History

- 2026-08-05: Code Builder added catalog-backed synergy and artifact A-J debug lists, connected artifact selection to PrisonPanel acquisition, added debug completion return behavior, and preserved the normal RewardPanel artifact flow.
- 2026-08-05: User reorganized the DebugUI hierarchy; Code Builder corrected the nested ArtifactDebugUI binding paths, confirmed the missing `정령계약` display was caused by the stale path, deleted the six ArtifactAchiveDebugUI EmodifierBtn children, saved InGameScene, and revalidated the scene.
- 2026-08-05: User moved ArtifactDebugUI to be a DebugPanel child; Code Builder updated DebugUI to the sibling panel paths, verified the full A-J binding targets, compiled with 0 errors, validated the scene with 0 issues, and confirmed the existing artifact debug execution chain remains connected.
- 2026-08-05: User removed modifier buttons; Code Builder made those bindings optional so ArtifactDebugUI can initialize independently, then verified catalog-backed A-J target paths, scene validation, Unity diagnostics, and a 0-error build.
- 2026-08-05: User moved ArtifactAchiveDebugUI to a DebugPanel sibling; Code Builder updated its panel, Close, and A-J artifact bindings, saved InGameScene, and verified build and scene validation.
- 2026-08-05: User required debug artifact acquisition to use the existing ArtifactAchiveDebugUI 1P-5P buttons without PrisonPanel; Code Builder bound those buttons, assigned party monster names, reused RunSession grant validation, logged successful grants, disabled owned artifacts, and removed the obsolete debug PrisonPanel completion path.
- 2026-08-05: User reported that debug 1P-5P clicks produced no grant log; inspection found `ClearArtifactDebugState` removed their listeners and target refresh did not restore them. Code Builder now rebinds each existing unit-button listener during target refresh without changing the RunSession acquisition logic.

## Task: 2026-08-05 Boss Artifact Reward Panel Design

### Task title

Use fixed RewardPanel slots for all stage rewards and remove legacy templates.

### Goals

- Use the authored `RewardBtn` through `RewardBtn (12)` objects in order for every reward entry.
- Put the reward category in each slot's `Summary` and the detail/value in `What`.
- Reuse `RewardPanelUI` reward-button consumption behavior without runtime cloning.
- Open the existing `UI/Reward/ArtifactPanel` from a new artifact reward button.
- Bind each choice from loaded Definitions: synergy display name to `Summary`, artifact icon to `Icon`, description to `Desc`, and artifact display name to `ArtifactName`.
- After an artifact choice, reuse PrisonPanel as the receiving-unit selector: occupied 1P-5P slots show only `Acquisition` under their Button.
- Return to `RewardPanel` after one artifact is acquired.

### Constraints

- Reuse the existing reward-button, PrisonPanel slot and `Button.interactable` flows; do not introduce a second party roster or generic offer framework.
- ArtifactPanel uses a dedicated minimal `ArtifactUI`; only its three choice labels are named `ArtifactName`.
- The four legacy direct children `PrisonerBtn`, `DarkBtn`, `GoldBtn`, and `ArtifactBtn` are removed from `RewardBtnContainer`.
- `Summon` does not exist in the hierarchy; the existing field is `Summary`.
- Preserve one generated offer set while the reward remains unconsumed so closing/reopening cannot reroll it.
- Acquisition mode must suppress the existing `Reinforcement` and `Menifested` button labels and must not enter Offering or manifestation behavior.
- An occupied unit already holding three artifacts remains visible with `Acquisition` active. Keep its Button component and GameObject enabled; block only input with `Button.interactable = false`.

### Role Owner

Code Builder

### Status

Implemented and corrected for Day5 Midboss, Day10 Midboss and Day11 Boss rewards. RewardPanel now uses fixed authored slots 0-12; Play Mode rendering verification remains user-owned.

### Next Actions

- User: verify the artifact reward button on Day5, Day10 and Day11 in Play Mode.
- Add an icon asset/path for `resonance-compass` when that missing source asset is authored.

### Evidence

- `Pakuri/Assets/Scripts/UI/InGame/Reward/RewardPanelUI.cs` binds `RewardBtn` through `RewardBtn (12)` and activates them in reward order; it no longer instantiates reward buttons.
- `Pakuri/Assets/Scripts/UI/InGame/Reward/RewardButtonView.cs` resets each fixed slot and writes `Summary`/`What` before binding its click action.
- `Pakuri/Assets/Scripts/UI/InGame/Reward/OfferingUI.cs` already implements three-choice binding and in-place Fisher-Yates shuffle.
- `Pakuri/Assets/Scripts/UI/InGame/InGameUIManager.cs` binds Reward, Prison, and Offering panels but not ArtifactPanel.
- Inspected `InGameScene` hierarchy contains `UI/Reward/ArtifactPanel/Choice1..3` with `Summary`, `SkillName`, `Desc`, and `Icon`; ArtifactPanel currently carries `OfferingUI`.
- `PrisonPanelUI` already binds and refreshes five shared slot views, but each slot currently routes occupied clicks to Offering and the next empty slot to manifestation.
- Live Unity hierarchy inspection found `1P/Button/Acquisition` inactive, while 2P-5P Buttons contain only `Reinforcement` and `Menifested`.
- Current `PrisonPanelUI.RefreshPrisonPartySlot` already uses `Button.interactable` independently from Button GameObject visibility, so acquisition capacity can reuse that exact input-blocking mechanism.
- `ArtifactUI.cs` prepares at most three unowned Spirit Contract choices once, binds Definition display/icon/description fields, and carries the selected ID into PrisonPanel.
- `RewardPanelUI.cs` omits the artifact button when no choice is available, displays the actual remaining choice count, and leaves `NextBtn` skippable.
- `PrisonPanelUI.cs` shows `Acquisition` for occupied slots, keeps full slots visible with `Button.interactable=false`, and returns through `InGameUIManager.CompleteArtifactAcquisition` after a successful grant.
- Unity hierarchy validation reported zero issues, zero missing scripts and zero broken prefabs; the scene contains three `ArtifactName` labels and five `Acquisition` children.
- `InGameScene.unity` contains `RewardBtn` through `RewardBtn (12)`, each with `Summary` and `What` children; live hierarchy search found all 13 slots.
- Live hierarchy search found no `PrisonerBtn`, `DarkBtn`, `GoldBtn`, or `ArtifactBtn` after the scoped deletion.
- `StageManager` now forwards the nonnegative `artifact_choice_count` without a second exact-`Boss` gate, so all three configured Midboss/Boss reward rows reach the existing button clone path.

### History

- 2026-08-05: Traced current reward and offering UI paths; selected smallest separate ArtifactUI design rather than expanding OfferingUI into a generic controller.
- 2026-08-05: User selected a two-step artifact flow: ArtifactPanel chooses the artifact, then PrisonPanel acquisition mode chooses the receiving occupied party slot.
- 2026-08-05: User required full-artifact slots to remain enabled and visible; only their click interaction is blocked, without disabling the Button component or GameObject.
- 2026-08-05: Code Builder implemented the ArtifactPanel and PrisonPanel acquisition flow, preserved skippable `NextBtn`, saved `InGameScene`, and verified focused artifact tests 7/7.
- 2026-08-05: User reported no artifact reward button in Play Mode; Designer verified the scene/template path and isolated the unresolved boundary to Boss eligibility versus runtime button integration.
- 2026-08-05: User confirmed Day5/10 Midboss eligibility; Code Builder removed the duplicate Day11-only gate and verified the shared choice path.
- 2026-08-05: Code Builder replaced runtime reward-template cloning with the 13 authored RewardBtn slots, mapped category/value text to Summary/What, removed the four legacy template objects, saved InGameScene, and passed script/scene validation plus the project build.

## Task: 2026-07-29 Field Unit Query Migration

### Task title

Move UI field-unit reads to `UnitSpawnManager`.

### Goals

- Keep UI read-only with respect to field-unit registration.
- Query selected and manifested player models from the shared `UnitSpawnManager` roster.
- Remove the `SpawnedPlayerModel` fallback source.

### Constraints

- Preserve current party panel, damage meter, debug UI, Offering refresh, and auto-skill UI behavior.
- Do not change UGUI objects, scene hierarchy, prefabs, player-facing text, navigation, or input.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies party portraits, selected-monster damage-meter identity, learned-skill refresh, debug refresh, and auto-skill toggle in Play Mode.

### Evidence

- `MonsterPanelUI` reads `unitSpawnManager.Players` and no longer keeps a CombatManager Registry path or selected-player fallback.
- `DamageMeterUIController` resolves slot zero through `FindPlayerMonsterBySlot(0)`.
- `InGameUIManager`, `DebugUI`, and `InGameUtilityPanelController` receive `UnitSpawnManager` query access instead of `CombatUnitRegistry`.
- Active C# search finds zero `SpawnedPlayerModel` and `InGameCombatManager.UnitRegistry` references.
- Runtime and Editor project builds completed with zero errors.
- Unity Console contained zero errors after script refresh.

### History

- 2026-07-29: User approved one-owner field-unit management and read-only access for all other systems.
- 2026-07-29: Code Builder migrated affected UI consumers without changing UI assets or player-facing behavior.

## Task: 2026-07-29 Learned Skill UI Copy Removal

### Task title

Read learned skills from the shared `UnitSkills` source and rebuild only execution state.

### Goals

- Remove Offering and Debug UI copies from `RunMonsterState` lists into runtime models.
- Query learned active/passive state through `UnitSkills`.
- Preserve current button state, Offering completion, runtime rebuild, and display refresh behavior.

### Constraints

- Do not change UGUI objects, scenes, prefabs, labels, navigation, or player input.
- Keep reward commits routed through `RunSession.RecordOfferingChoice`.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies Offering buttons, Debug skill labels, modifier panels, learned-skill display, and the next combat's skill list in Play Mode.

### Evidence

- `InGameUIManager` and `DebugUI` no longer define or call `SyncModelStateFromSession`.
- Debug learned-state checks call `state.Skills.HasActiveSkill` and `HasPassiveSkill`.
- Offering and Debug refresh paths retain `SkillExecution.RebuildLearnedSkillState` and display refresh calls.
- Removed copy-symbol search returned zero active production references.
- Runtime and Editor builds completed with zero errors; Unity EditMode tests passed 5/5.
- Unity script compilation returned ready and the post-compile Console contained zero errors or warnings.

### History

- 2026-07-29: User approved `UnitSkills` as the single learned-skill source with post-combat full execution-state rebuilds.
- 2026-07-29: Code Builder removed UI copy helpers and converted UI reads to the shared source.

## Task: 2026-08-03 Offering Skill Popup Text

### Task title

Show distinct popup text for new active, new passive, and master skill offerings.

### Goals

- Keep all Choice1~3 presentation changes in the shared `BindChoiceButton` path.
- Keep A~E new active skills as `신규 획득!`.
- Show `패시브 스킬` for F~J passive skill acquisition.
- Show `마스터 스킬` for master skill acquisition.

### Constraints

- Do not add a production script.
- Do not change `NewRunScene` hierarchy, CSV data, `RunSession`, or `UnitSkills` ownership.
- Unity Play Mode UI verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally compiled.

### Next Actions

- User verifies Choice1~3 popup activation and text in `NewRunScene` Play Mode.

### Evidence

- `UI/InGame/Reward/OfferingUI.cs` now separates `NewActiveSkill` and `NewPassiveSkill` and assigns them in `AddActiveSkillChoices` and `AddPassiveSkillChoices`.
- `OfferingUI.BindChoiceButton` updates `PopUP`, `NewSkillPopUText`, and `SkillName` color through one shared path.
- `NewRunScene.unity` contains three `PopUP` objects and three `NewSkillPopUText` objects.
- Runtime and Editor project builds completed with zero errors and the existing two assembly-reference warnings.

### History

- 2026-08-03: Code Builder added runtime popup text switching without modifying scene serialization.

## Task: 2026-08-03 CSV Monster and Enemy Image UI Wiring

### Task title

Use catalog-backed monster Standing and enemy Images in PrisonPanel and the manifested success popup.

### Goals

- Bind `MonsterDefinition.Image` to `PrisonPanel/1~5P/Image`.
- Bind `MonsterDefinition.Image` to `MenifestedSuccessPopUp/MonsterImage`.
- Bind `EnemyDefinition.Image` to `PrisonPanel/Prisonal/Image`.
- Remove name-specific serialized monster portrait routing.

### Constraints

- Keep `MonsterIconImage` for icon consumers such as MonsterPanel and DamageMeter.
- Preserve the existing UGUI hierarchy and shared catalog ownership.
- Play Mode visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compiled. Scene validation reports 0 issues, 0 missing scripts and 0 broken prefabs.

### Next Actions

- User verifies all five Standing portraits, manifested success image and selected enemy prisoner image in Play Mode.

### Evidence

- `InGameUIManager.RefreshPrisonPartySlot` now uses `monster.Image`.
- Manifested success now uses `monster.Image` instead of `MonsterIconImage`.
- `RefreshSelectedPrisoner` now resolves `EnemyDefinition.Image` and assigns the prisoner Image.
- `arielPrisonPortrait` through `vegaPrisonPortrait` and `ResolveMonsterPortrait` have zero active references.

### History

- 2026-08-03: Code Builder replaced direct scene/name-based portrait references with CSV runtime catalog Image fields.

## Task: 2026-08-03 Auto Skill Button Visual State

### Task title

Keep AutoBtn's visual ON/OFF state synchronized with the working auto-skill toggle.

### Goals

- Preserve the existing `PlayerCombatInputController` auto-skill behavior.
- Make the second AutoBtn click visibly switch back to OFF.
- Keep the ON visual after clicking the time-scale button.

### Constraints

- Do not change the time-scale logic or combat skill execution.
- Preserve the existing `Button` scene component and UI hierarchy.
- Play Mode input verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compiled.

### Next Actions

- User verifies AutoBtn ON → time button → AutoBtn OFF in `NewRunScene` Play Mode.

### Evidence

- `InGameUtilityPanelController` caches the authored Button colors and applies the selected color to all interactive states while `AutoSkillEnabled` is true; it restores the normal color when false.
- The visual refresh runs on initialization, enable and immediately after `ToggleAutoSkillMode`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.
- Unity script refresh completed with no project compile errors reported; `NewRunScene` validation reported 0 issues, 0 missing scripts and 0 broken prefabs.

### History

- 2026-08-03: Code Builder fixed the visual-only toggle desynchronization without changing the working auto-skill state logic or time-scale behavior.

## Task: 2026-08-03 InGame UI Responsibility Split

### Task title

Split `InGameUIManager` into flow control and unit UI modules.

### Goals

- Keep `InGameUIManager` responsible for lifecycle, Stage reward detection, module wiring, and flow transitions only.
- Move reward button state, PrisonPanel slots, Offering choices, manifested popups, and information text updates into focused scripts.
- Organize the scripts under `UI/InGame/Info` and `UI/InGame/Reward` without changing the scene hierarchy.

### Constraints

- Preserve `RunSession` ownership and existing reward/Offering/manifest flow.
- Preserve the existing `OfferingPanel/Choice1~3` paths and popup text/color behavior.
- Do not add interfaces, factories, new MonoBehaviours, or speculative scene components.
- Unity Play Mode visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies reward opening, prisoner selection, Offering, manifest success/failure, and next-day flow in `NewRunScene` Play Mode.

### Evidence

- `InGameUIManager.cs` is reduced to 216 lines and contains Stage flow, module construction, callbacks, and Inspector reference wiring; it no longer contains `OfferingUI`, `MenifestUI`, `RewardButtonView`, or `PrisonPartySlotView` implementations.
- `UI/InGame/Reward/RewardPanelUI.cs` owns reward button creation, material claim, prisoner selection, and next-day button binding.
- `UI/InGame/Reward/PrisonPanelUI.cs` owns `PrisonPanel/1~5P` slot refresh, selected prisoner display, and Offering/manifest entry.
- `UI/InGame/Reward/OfferingUI.cs` owns Choice1~3 construction, popup activation/text, SkillName colors, skill commit, and runtime skill refresh.
- `UI/InGame/Reward/MenifestUI.cs` owns manifested success/failure popups and party commit.
- `UI/InGame/Info/InGameInfoUI.cs` owns top and PrisonPanel resource/stage text refresh; the serialized reference groups are defined in `UI/InGame/InGameUIReferences.cs`.
- All eight new/updated scripts have Unity `.meta` files; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.
- Unity Console returned 0 error/warning entries after refresh; loaded `Assets/Scenes/NewScene/NewRunScene.unity` validation returned 0 issues, 0 missing scripts, and 0 broken prefabs.

### History

- 2026-08-03: Code Builder extracted the InGame UI units and grouped reward-related scripts under `UI/InGame/Reward` while preserving the existing scene hierarchy and runtime ownership.

## Task: 2026-08-03 InGame UI Inspector Reference Wiring

### Task title

Replace InGame UI name/path lookup with Inspector-assigned serializable reference groups.

### Goals

- Keep `InGameUIManager` responsible for lifecycle, module construction, and flow control.
- Assign Stage/Combat/Spawn managers and all Reward, Prison, Offering, Menifest, and Info UI references in `NewRunScene` through `[Serializable]` groups.
- Remove the obsolete shared scene-path resolver.

### Constraints

- Preserve the existing `NewRunScene` hierarchy, UI behavior, popup text/color rules, and RunSession ownership.
- Keep fixed Prison slots and Offering choices directly visible as `partySlot1~5` and `choice1~3` Inspector fields.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and scene-validated.

### Next Actions

- User verifies reward, Prison, Offering, Menifest, and next-day flows in `NewRunScene` Play Mode.

### Evidence

- `UI/InGame/InGameUIReferences.cs` defines `[Serializable]` reference groups for every module and fixed UI slot/choice.
- `InGameUIManager` uses `CreateUiModules()` with Inspector references; no `InGameUiSceneResolver.cs` or InGameUI path/name lookup reference remains.
- `NewRunScene` Canvas inspection reports non-null StageManager, UnitSpawnManager, CombatManager, and all assigned module references; `titleLabel` remains null because Choice1~3 have no direct title TMP child in the inspected hierarchy.
- `NewRunScene` validation reported 0 issues, 0 missing scripts, and 0 broken prefabs.
- Unity Console contained 0 error/warning entries after refresh; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder replaced runtime scene lookup with serialized Inspector references, saved the NewRunScene assignments, and deleted `InGameUiSceneResolver.cs` plus its `.meta` file.

## Task: 2026-08-03 Canvas Boss HP Display

### Task title

Wire the authored `Canvas/BossHP` hierarchy to the in-game UI module and display the selected boss's combined HP and shield.

### Goals

- Bind `Name`, `HPText`, `BackGround`, `Fill`, and `Shield` through `InGameUIReferences`.
- Format HP as `{current health + shield} / {max health + shield}`.
- Keep BossHP inactive when no active boss is selected or transient reward UI is shown.

### Constraints

- Reuse the existing plain UI-module architecture and authored scene objects.
- Keep `BossHP` initially inactive in `NewRunScene`.
- Unity Play Mode visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and scene serialization verification complete.

### Next Actions

- User verifies Canvas position, fill widths, shield segment, name, and HP text in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/UI/InGame/InGameUIReferences.cs` contains serialized BossHP references for the root, two texts, and three RectTransforms.
- `Pakuri/Assets/Scripts/UI/InGame/InGameUIManager.cs` creates, refreshes, and hides `BossHpUI` with the existing UI lifecycle.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` contains `Canvas/BossHP`, all five child references, and `m_IsActive: 0` on the root.
- `BossHP` `Fill` and `Shield` are authored as Simple UI Images, so `BossHpUI` updates their RectTransform widths and positions.

### History

- 2026-08-03: Code Builder connected the existing BossHP scene hierarchy to the new runtime module and preserved the existing InGame UI reference architecture.

## Task: 2026-08-03 Direct Inspector UI Module Conversion and NewRunScene Organization

### Task title

Unify in-game UI ownership and scene references through MonoBehaviour Inspector wiring.

### Goals

- Make each scene-owned in-game UI module a `MonoBehaviour` with its own serialized Inspector references and lifecycle.
- Keep `InGameUIManager` as the flow controller that coordinates the UI modules.
- Remove `InGameUIReferences.cs` and all runtime scene-name/path lookup for the in-game UI.
- Preserve existing UI order, transforms, layers, and runtime flow while grouping the scene hierarchy.

### Constraints

- Preserve `NewRunScene` gameplay/UI behavior and existing object transforms.
- Keep UI objects on the existing UI layer and runtime/spawn objects on the existing Default layer.
- Keep Unity Play Mode verification user-owned.
- Do not modify unrelated Combat changes already present in the worktree.

### Role Owner

Code Builder

### Status

Implemented, serialized, compiled, and scene-validated.

### Next Actions

- User verifies reward, Prison, Offering, manifest, HUD, damage-meter, debug, spawn, and next-day flows in `NewRunScene` Play Mode.

### Evidence

- `UI/InGame/InGameUIManager.cs:13-21` now stores direct serialized references to Stage/Spawn/Combat and each UI module.
- `UI/InGame/Reward/RewardPanelUI.cs`, `PrisonPanelUI.cs`, `OfferingUI.cs`, `MenifestUI.cs`, `UI/InGame/Info/InGameInfoUI.cs`, and `BossHpUI.cs` are `MonoBehaviour` modules with direct `[SerializeField]` references.
- `GameFlow/Spawn/UnitSpawnManager.cs:20-25` owns serialized spawn references; `partySpawnPoints` is assigned to five scene Transforms and no target lookup remains.
- `InGameUIReferences.cs` and its `.meta` are absent; repository search found no `InGameUIReferences` or temporary `InGameSceneReferenceSetup` references.
- Live `NewRunScene` hierarchy has `UI` on layer 5 with `HUD`, `Reward`, `Popup`, `Debug`, `DamageMeter`, and `Result`; `Runtime` on layer 0 with `Enemies`, `Skills`, and `Monsters`; `Grid/SpawnPoint` contains six spawn children on layer 0.
- `NewRunScene.unity` stores non-zero direct references for `InGameUIManager`, `RewardPanelUI`, `PrisonPanelUI`, `OfferingUI`, and `UnitSpawnManager.partySpawnPoints[0..4]`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and 2 existing assembly-reference warnings.
- Unity `NewRunScene` validation returned 0 issues, 0 missing scripts, and 0 broken prefabs; editor state reports compilation complete and ready for tools. The cleared Unity console later contained one MCP transport disposed-object entry, not a project compiler diagnostic.

### History

- 2026-08-03: Code Builder converted scene-owned in-game UI modules to direct Inspector-wired MonoBehaviours, removed `InGameUIReferences.cs`, assigned all nested UI references, added direct party spawn-point references, and reorganized `NewRunScene` while preserving object order, transforms, and layers.

## Task: 2026-08-03 Damage Meter Duplicate Logic Consolidation

### Task title

Remove redundant lookup code from `DamageMeterUIController`.

### Goals

- Remove the unused catalog helper and local value.
- Share active/passive skill-name lookup code.
- Scan a damage source's reaction once and reuse it for choice-title and trigger-source display resolution.
- Share the active/passive reaction traversal without changing display priority or serialized Inspector fields.

### Constraints

- Preserve Damage Meter open/close, panel refresh, segment layout, tracker aggregation, and display fallback order.
- Preserve all serialized field names and scene references.
- Do not modify `DamageMeterRuntimeTracker` ownership or run Play Mode gameplay tests.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies Damage Meter opening, per-monster totals, skill segment labels, and percentage display in Play Mode.

### Evidence

- `DamageMeterUIController.cs:147-166` now resolves one `SkillReaction` and reuses it for both title and trigger-source paths.
- `DamageMeterUIController.cs:232-263` uses one shared `SkillDefinition[]` reaction scan for active and passive skills.
- `DamageMeterUIController.cs:268-299` uses one shared skill-name scan while preserving active-before-passive priority.
- Search returns no `ResolveCatalog`, `ResolveActiveSkillDisplayName`, `ResolvePassiveDisplayName`, `ResolveChoiceTitleForSource`, or unused `catalog` symbol.
- `DamageMeterRuntimeTracker.cs` remains the sole damage aggregation owner; this change only consolidates UI lookup/presentation code.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.
- Unity console returned 0 error/warning entries after refresh; `NewRunScene` validation returned 0 issues, 0 missing scripts, and 0 broken prefabs.

### History

- 2026-08-03: Code Builder removed dead catalog code and consolidated duplicate skill/reaction lookup paths in `DamageMeterUIController` without changing serialized scene data.

## Task: 2026-08-03 UI Dead Code and Duplicate Reduction

### Task title

Remove confirmed dead UI state and duplicate UI helper/lookup code.

### Goals

- Delete unused reward metadata, damage-record identity, serialized Inspector fields, and catalog plumbing.
- Reduce repeated DebugUI array/panel/monster-resolution code.
- Consolidate identical `SetActive(GameObject, bool)` implementations without changing UI behavior.

### Constraints

- Preserve UI behavior, public MonoBehaviour entry points, and active Inspector references.
- Remove matching stale `NewRunScene` serialized fields when script fields are deleted.
- Do not merge `BindButton` implementations because listener replacement behavior differs.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally compiled.

### Next Actions

- User verifies reward buttons, Prison/Offering/Manifest flow, Debug panels, BossHP, MainMenu panel switching, and Damage Meter in Play Mode.

### Evidence

- `RewardButtonView` no longer stores unused `RewardKind`, `Kind`, or `Amount` values; `MonsterDamageRecord` no longer stores unread `MonsterId`.
- Removed unused UI Inspector fields and matching `NewRunScene.unity` entries for `RewardPanelUI`, `OfferingUI`, `MonsterPanelUI`, `DebugUI`, and `InGameUIManager`.
- `MonsterPanelUI` no longer passes a catalog through methods that read `GameDataLoader.CurrentCatalog` directly.
- `DebugUI` uses one button-array guard, one shared selected-monster resolver, and one shared `UiObjectUtility.SetActive` helper.
- Six duplicate UI `SetActive` helpers were reduced to `UiObjectUtility` in `InGameUIManager.cs`; listener-binding helpers were left unchanged.
- `git diff --check` returned no whitespace errors.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder applied the confirmed dead-code and duplicate-code reductions from the UI audit and synchronized NewRunScene serialized fields.

## Task: 2026-08-03 MainMenu Inspector Reference Wiring

### Task title

Remove MainMenu runtime scene lookup and use preassigned Inspector references.

### Goals

- Keep `MainMenuUIManager` dependent only on serialized panel and button references.
- Store all `NewMainMenu` panel/button references directly in scene serialization.
- Remove name/path-based runtime object search code.

### Constraints

- Preserve MainMenu panel flow, monster selection, `StartContext`, and NewRunScene loading.
- Preserve the existing scene hierarchy and player-facing labels.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally compiled.

### Next Actions

- User verifies Intro → MainMenu → MonsterSelect transitions, all five monster buttons, and NewRunScene loading in Play Mode.

### Evidence

- `MainMenuUIManager.Awake()` now only binds the serialized fields; `ResolveSceneReferences`, `ResolveGameObject`, `ResolveButton`, `FindChild`, and `FindSceneGameObject` were removed.
- `NewMainMenu.unity` now identifies the component as `MainMenuUIManager` and stores non-zero fileIDs for three panels and eight Button components.
- Static search found no runtime scene lookup symbol in `MainMenuUIManager.cs`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.
- `git diff --check` returned no whitespace errors.

### History

- 2026-08-03: Code Builder converted MainMenu scene references from runtime name lookup to direct Inspector serialization and removed the fallback resolver code.

## Task: 2026-08-03 PrisonPanel Full-Card Raycast Fix

### Task title

Disable raycast interception on the 1P~5P parent background Images in NewRunScene.

### Goals

- Allow clicks in the full PrisonPanel slot area to reach the configured child Buttons.
- Keep the existing PrisonPanelUI Inspector Button references and hierarchy unchanged.

### Constraints

- Change only the parent background Image `m_RaycastTarget` values for 1P~5P.
- Do not resize or replace the child Buttons.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and scene diff verified.

### Next Actions

- User verifies center clicks on `UI/PrisonPanel/1P` through `5P` in NewRunScene Play Mode.

### Evidence

- `NewRunScene.unity` parent Image components `421615614`, `1448528057`, `1496035311`, `1590034690`, and `2110331148` now have `m_RaycastTarget: 0`.
- These components are the parent Images of 5P, 2P, 1P, 4P, and 3P respectively; the configured child Button fileIDs remain unchanged at `NewRunScene.unity:31783-31806`.
- `git diff --check -- Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` returned no whitespace errors.

### History

- 2026-08-03: Code Builder disabled parent background Image raycasts for all five PrisonPanel slots.

## Task: 2026-08-03 Stage End Panel UI Ownership

### Task title

Give win and defeat panels direct Inspector Button references.

### Goals

- Keep end-panel UI references on a `MonoBehaviour` attached to each panel.
- Let `StageManager` coordinate visibility and callbacks without discovering child Buttons.

### Constraints

- Preserve the existing `InGameScene` panel hierarchy and Button behavior.
- Do not add runtime object-name or child-path lookup.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, serialized, and compiled.

### Next Actions

- User verifies win/defeat panel visibility and MainMenu return in Play Mode.

### Evidence

- `Assets/Scripts/UI/InGame/StageEndPanelUI.cs` owns the serialized `returnButton` and listener binding.
- `InGameScene.unity` assigns `StageEndPanelUI.returnButton` to Button fileIDs `240062369` and `1240753997`.
- `StageManager` contains no `ResolveEndFlowReferences` or child Button discovery code.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder moved end-panel Button ownership from StageManager lookup into Inspector-wired `StageEndPanelUI` components.

## Task: 2026-08-04 MainMenu Runtime UI Reference Binding

### Task title

Remove MainMenu panel/button Inspector serialization and bind scene references at runtime.

### Goals

- Remove `[SerializeField]` from the three MainMenu panels and eight Button references.
- Keep `BindButtons()` as the event-binding path.
- Run reference binding, event binding, and initial Intro visibility from `Start()`.
- Convert fixed scene path and default monster values to code-owned constants.
- Log missing scene roots, paths, components, buttons, or actions as errors.

### Constraints

- Scope is `MainMenuUIManager.cs` and `MainMenuScene.unity` only, apart from this state record.
- Preserve Intro → MainMenu → MonsterSelect flow, monster selection, `StartContext`, and InGameScene loading.
- Preserve the actual scene spelling `MosterSelectUI` and the current scene path.
- Do not modify unrelated user changes under `reference/3.combat`.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Core/Editor compiled. Unity project-console recheck is pending because the Unity MCP instance disconnected; the only retrieved entry was an MCP disposed-object diagnostic, not a project compile error.

### Next Actions

- User verifies MainMenu Intro → MainMenu → MonsterSelect transitions, all five monster buttons, and InGameScene loading in Play Mode.
- Recheck Unity Console after the Unity MCP instance reconnects.

### Evidence

- `MainMenuUIManager.cs` has no `[SerializeField]` fields; `BindObject()` resolves `Canvas/Intro`, `Canvas/MainMenuUI`, `Canvas/MosterSelectUI`, and all eight Button paths before `BindButtons()`.
- Unity hierarchy inspection confirmed `Canvas/Intro/GameStart`, `Canvas/MainMenuUI/RunBtn`, `Canvas/MosterSelectUI/GameStart`, and `Ariel`, `Eve`, `Sein`, `Vega`, `Rin` Button objects, including inactive panels.
- Missing reference and event-binding paths now emit `Debug.LogError` with field, path, and expected type before initialization stops.
- `newRunScenePath` and `defaultMonsterId` scene fields were removed; `Assets/Scenes/NewScene/InGameScene.unity` and `eve` are code constants matching prior scene values.
- `MainMenuScene.unity` no longer contains the 13 removed serialized reference/configuration keys.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and 2 existing assembly-reference warnings.
- `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors and 3 warnings, including one existing file-lock retry.
- `git diff --check -- Pakuri/Assets/Scripts/UI/MainMenu/MainMenuUIManager.cs Pakuri/Assets/Scenes/NewScene/MainMenuScene.unity` returned no whitespace errors.

### History

- 2026-08-04: Code Builder replaced MainMenu Inspector panel/Button references with inactive-safe scene-root and relative-path binding, consolidated startup in `Start()`, converted fixed settings to constants, removed matching scene serialization, and preserved unrelated working-tree changes.

## Task: 2026-08-04 BossHpUI Runtime Reference Binding and Invalid Entry Guard

### Task title

Remove BossHpUI Inspector references, bind the authored BossHP hierarchy at runtime, and guard invalid displayed-boss entries.

### Goals

- Remove `[SerializeField]` from the seven BossHpUI references.
- Bind `Name`, `HPText`, `BackGround`, `Fill`, and `Shield` relative to the BossHP object.
- Find exactly one `UnitSpawnManager` in the current scene and log missing or duplicate references as errors.
- Prevent `Refresh()` from dereferencing a displayed entry whose combat model is null; log and clear the display safely.

### Constraints

- Scope is `BossHpUI.cs`, `InGameScene.unity`, and this state record.
- Preserve BossHP display behavior and restoration of the world HP bar.
- Preserve existing MainMenu changes and unrelated `reference/3.combat` changes.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented. Script validation, Unity InGameScene validation, Core/Editor builds, and whitespace checks passed. Runtime boss spawn/defeat behavior still requires user Play Mode verification.

### Next Actions

- User verifies BossHP appears for a boss, updates during damage, restores the world HP bar after defeat, and records any remaining console errors.
- If the invalid-entry error reappears, inspect the producer that inserts the null-model `CombatUnitEntry`; this UI guard prevents the crash but does not manufacture a valid combat model.

### Evidence

- `BossHpUI.cs` now uses private non-serialized fields and `Awake()` calls `BindObject()` before `Hide()`.
- `BindObject()` resolves the authored child paths and logs `Debug.LogError` on missing components or duplicate/missing `UnitSpawnManager` instances.
- `Refresh()` checks the displayed entry's model, stats, and resources before `RefreshValues`; an invalid entry is logged once, cleared, and hidden.
- `InGameScene.unity` BossHpUI component block no longer contains `root`, `nameText`, `hpText`, `background`, `fill`, `shield`, or `unitSpawnManager` serialized keys; the authored child names are `Name`, `HPText`, `BackGround`, `Fill`, and `Shield`.
- `validate_script` returned 0 warnings and 0 errors for `Assets/Scripts/UI/InGame/Info/BossHpUI.cs`.
- Unity `manage_scene validate` returned `InGameScene is clean — no issues found` with 0 issues.
- Runtime and editor `dotnet build` both returned 0 errors and the existing 2 assembly-reference warnings.
- `git diff --check` returned no whitespace errors.

### History

- 2026-08-04: Code Builder converted BossHpUI Inspector references to runtime binding, removed matching scene serialization, and added a once-logged error/log-and-clear guard for invalid combat models.

## Task: 2026-08-04 InGame UI Runtime Binding Migration

### Task title

Replace InGame UI object-reference serialization with per-module `BindObject()` runtime binding.

### Goals

- Bind scene-owned UI objects and manager dependencies from the authored `InGameScene` hierarchy.
- Keep only RewardPanel layout tuning values serialized.
- Centralize path lookup and required-reference error logging in `UiBindingUtility`.
- Disable a module after a required binding failure instead of continuing with silent null references.

### Constraints

- Preserve the authored hierarchy, component types, UI behavior, and existing manager ownership.
- Do not convert tunable RewardPanel layout values to runtime lookup.
- Do not claim Play Mode verification while the Unity MCP instance is unavailable.
- Preserve unrelated working-tree changes under MainMenu and `reference/3.combat`.

### Role Owner

Code Builder

### Status

Implemented and statically verified. Play Mode remains user-owned.

### Next Actions

- User verifies InGame HUD, Reward/Prison/Offering/Manifest flow, MonsterPanel, Damage Meter, Debug UI, end panels, and error-free scene startup in Play Mode.
- Re-run Unity scene/console validation after the Unity MCP instance reconnects.

### Evidence

- `Pakuri/Assets/Scripts/UI/InGame/UiBinding.cs` provides child, scene, self, optional, and duplicate-detection binding helpers; required failures use `Debug.LogError` with owner, field, path, and expected type.
- `InGameUIManager`, `InGameInfoUI`, `RewardPanelUI`, `PrisonPanelUI`, `OfferingUI`, `MenifestUI`, `StageEndPanelUI`, `InGameUtilityPanelController`, `MonsterPanelUI`, `DamageMeterUIController`, `DamageMeterRuntimeTracker`, and `BossHpUI` now bind required scene references at runtime.
- `RewardPanelUI.cs` is the only InGame UI script retaining `[SerializeField]`; its four remaining fields are layout tuning values, not object references.
- YAML hierarchy path check inspected 174 required object paths in `InGameScene.unity` and reported `missing 0`.
- `InGameScene.unity` contains no removed InGame object-reference keys; RewardPanel layout values remain serialized.
- Runtime and Editor `dotnet build` commands completed with 0 errors; `git diff --check` reported no whitespace errors.

### History

- 2026-08-04: Code Builder added the shared binding utility, migrated InGame UI modules and nested slot/choice views, removed stale object-reference serialization, preserved RewardPanel tuning fields, and corrected scene-confirmed DamageMeter, Debug, and MonsterPanel paths.

## Task: 2026-08-05 Reward Flow Lazy Binding Guard

### Task title

Bind inactive Reward-flow UI modules before public entry methods use their references.

### Goals

- Prevent `RewardPanelUI.Show()` from dereferencing an unbound `uiManager` when the RewardPanel starts inactive.
- Apply the same entry-time binding guarantee to Prison, Offering, and Menifest flow modules.
- Preserve required-reference error logging through `UiBindingUtility` and fail safely when binding is invalid.

### Constraints

- Keep the existing authored hierarchy and runtime UI flow.
- Do not add new abstractions or serialized object references.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and statically verified. Play Mode remains pending.

### Next Actions

- User verifies enemy-clear → RewardPanel, prisoner selection → PrisonPanel, Offering, manifest success/failure, and next-day flow in Unity Play Mode.

### Evidence

- `InGameScene.unity` shows `OfferingPanel`, `RewardPanel`, and `PrisonPanel` start with `m_IsActive: 0`.
- The reported crash line was `RewardPanelUI.Show()`'s direct `uiManager.ResolvePrisonerDisplayName(...)` call; `Show()` previously did not call `BindObject()`.
- `RewardPanelUI.Show()`, `PrisonPanelUI.Open()`, `OfferingUI.OpenOfferingPanel()`, and `MenifestUI.TryManifestPrisoner()` now return after `BindObject()` failure; RewardPanel also rebinds the Next button listener at entry.
- Static guard inspection found `GUARD=present` for all four public reward-flow entry methods; required binding failures still log through `UiBindingUtility`.
- Runtime and Editor `dotnet build --no-restore /p:UseSharedCompilation=false` completed with 0 errors; `git diff --check` completed without whitespace errors.

### History

- 2026-08-05: Code Builder traced the null dereference to inactive-panel lazy initialization and added minimum entry-point binding guards across the Reward flow.

## Task: 2026-08-05 UiBinding Script Rename

### Task title

Rename the shared InGame binding helper file to `UiBinding.cs`.

### Goals

- Use `UiBinding.cs` and its matching Unity meta filename for the existing binding helper.
- Preserve the helper class and all call sites without changing runtime behavior.

### Constraints

- Rename only the source/meta paths and the generated C# project include.
- Preserve the existing Unity meta GUID and binding implementation.

### Role Owner

Code Builder

### Status

Implemented and compiled.

### Next Actions

- No code follow-up; Unity may reimport the renamed asset on the next editor refresh.

### Evidence

- The previous utility source/meta paths no longer exist; `UiBinding.cs` and `UiBinding.cs.meta` exist.
- `Assembly-CSharp.csproj` now includes `Assets\\Scripts\\UI\\InGame\\UiBinding.cs`.
- Meta GUID remains `5e1de2e7d44f4b2b9c2a0e47c9bbf214`.
- Repository search found no old filename reference; Runtime and Editor builds completed with 0 errors and `git diff --check` passed.

### History

- 2026-08-05: Code Builder renamed the binding helper source and meta files, updated the project include after the first stale-reference check, and preserved the helper class name/call sites.

## Task: 2026-08-05 Damage Meter Refresh Interval

### Task title

Limit Damage Meter UI refreshes to once every 0.5 seconds.

### Goals

- Keep damage accumulation event-driven in `DamageMeterRuntimeTracker`.
- Stop `DamageMeterUIController` from refreshing immediately on every tracker version change.
- Preserve immediate initial refresh when the meter is enabled or opened.

### Constraints

- Do not queue or delay actual damage records; `DamageApplied` events remain lossless.
- Preserve the Damage Meter hierarchy, panel layout, and display calculations.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and statically compiled.

### Next Actions

- User verifies that damage values and segments update approximately every 0.5 seconds while the meter is open.

### Evidence

- `DamageMeterRuntimeTracker.Record()` still receives `InGameCombatManager.DamageApplied` and increments accumulated records immediately.
- `DamageMeterUIController.RefreshIntervalSeconds` is `0.5f`; `Update()` now refreshes only when `refreshRemaining <= 0f`.
- The former `tracker.Version` immediate-refresh condition and `lastTrackerVersion` state were removed from the controller.
- Runtime and Editor builds completed with 0 errors; `git diff --check` completed without whitespace errors.

### History

- 2026-08-05: Code Builder changed the display timer from 0.2 to 0.5 seconds and removed version-triggered immediate UI refreshes without changing event-based damage accumulation.
