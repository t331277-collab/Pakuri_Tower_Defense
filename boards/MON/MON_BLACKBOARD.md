# MON_BLACKBOARD

이 파일은 BLACKBOARD.md 계층화 작업으로 생성된 도메인별 지속 상태 파일입니다.
관련 작업을 수행할 때 MDTREE.md 라우팅에 따라 이 파일과 필요한 상위/하위 파일을 동시에 갱신합니다.

## Scope

이 파일은 몬스터/플레이어블 캐릭터 생성과 스킬 데이터 공통 규칙을 담당한다.

캐릭터별 상세 구현 이력은 같은 폴더의 `{NAME}_MONSTER.md`에 둔다. 새 캐릭터를 만들 때 해당 파일이 없으면 먼저 생성하고, 공통 규칙은 이 파일을 기준으로 삼는다.

## Common Terms

- Monster / Player Monster: 사용자가 선택해 런을 시작하는 플레이어블 몬스터 캐릭터.
- Active skill: 슬롯 A-E. A는 기본 시작 액티브이고 B-E는 런 중 선택 가능한 액티브다.
- Passive skill: 슬롯 F-J. 패시브는 런 중 선택 가능한 효과이며, 현재 데이터 모델의 슬롯 범위는 A-J다.
- Enhancement: 스킬 또는 패시브의 강화 선택지.
- Master skill: 액티브 스킬의 마스터 선택지.
- `SkillSlot`: 현재 코드 기준 A-J만 존재한다. K 슬롯은 존재한다고 가정하지 않는다.
- `MonsterDefinition`: Unity 정적 자산으로 저장되는 몬스터 정의.
- `GameDataCatalog`: 5개 몬스터 자산을 묶어 런/DebugScene에서 참조하는 카탈로그.

## Creation Rules

- 저장소에 실제 문서, 코드, 자산이 없는 스킬/슬롯/헬퍼는 있다고 말하지 않는다.
- 새 몬스터 생성 시 먼저 `Pakuri/reference/2.Monster/{monster}` 문서와 현재 `Assets/Data/GameData/Monsters/*.asset` 구조를 확인한다.
- 공통 스킬 슬롯은 A-J 기준으로 유지한다.
- 캐릭터별 런타임 구현 예시가 필요하면 `boards/MON/EVE_MONSTER.md`를 참고하되, Eve 전용 동작을 새 캐릭터에 그대로 복사했다고 가정하지 않는다.
- DebugScene에서 몬스터 테스트 UI를 건드리면 `boards/UI/DEBUGSCENE_UI.md`도 함께 갱신한다.
- 데이터 자산을 건드리면 `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`도 함께 갱신한다.
- combat runtime을 건드리면 관련 `boards/COMBAT/*.md`도 함께 갱신한다.
- Unity-MCP Play Mode gameplay 검증은 사용자가 수행한다.
- Code Reviewer 실행은 사용자 허락이 있을 때만 수행한다.

## Character Board Rules

- Eve: `boards/MON/EVE_MONSTER.md`
- Vega: `boards/MON/VEGA_MONSTER.md`
- Ariel: `boards/MON/ARIEL_MONSTER.md`
- Sein: `boards/MON/SEIN_MONSTER.md`
- Rin: `boards/MON/RIN_MONSTER.md`

새 캐릭터 파일을 만들 때 최소 항목:
- Task title
- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Migrated Task Blocks

## Task: 2026-05-03 Player Monster Overhead Width Follow-up

### Task title

Tighten the selected player Monster HP bar width and keep direct Inspector tuning available.

### Goals

- Reduce the selected Monster HP bar width from the previous auto-layout result.
- Preserve separate name/HP text stacking and direct manual tuning for the selected Monster.

### Constraints

- Role Owner is Code Builder.
- Ground the change in the existing selected-Monster combat runtime code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected Monster overhead width in Play Mode.
- If needed, user can still disable `Auto Layout Selected Monster Status` and edit the manual selected-Monster layout fields directly.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:235-251` now uses a tighter selected-Monster automatic bar-width configuration and still exposes the manual selected-Monster local-position/scale override fields.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:344-375` now clamps selected-Monster automatic bar width to an explicit max value instead of allowing the previous wider result.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP warnings.

### History

- 2026-05-03: User reported that the selected Monster HP bar was still too long after the earlier overhead-stack split change.
- 2026-05-03: Code Builder tightened the automatic selected-Monster width clamp while keeping the manual override path.

## Task: 2026-05-03 Player Monster Overhead Status Layout Tuning

### Task title

Make the selected player Monster overhead name/HP display follow sprite size and expose manual layout overrides.

### Goals

- Keep the selected Monster name readable without overlapping the HP text or HP slider.
- Adjust the selected Monster overhead stack from the Monster sprite size instead of relying on one fixed offset for all Monsters.
- Give the user direct manual tuning fields in `CombatRuntimeController` when automatic layout is not enough for a specific Monster sprite.

### Constraints

- Role Owner is Code Builder.
- Ground the change in the existing selected-Monster combat runtime display code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies the selected Monster overhead layout for the Monsters they care about in Play Mode.
- If needed, user disables `Auto Layout Selected Monster Status` on the combat controller and edits the manual bar/name/HP text positions and scale fields directly.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:235-251` adds a dedicated serialized selected-Monster status-layout section so the player Monster overhead display can be tuned without another code edit.
- `CombatRuntimeController.cs:320-321` now stores separate selected-Monster name/HP text labels instead of one combined multiline label.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:253-283` now creates `MonsterNameLabel` and a separate `MonsterHpLabel`, and repositions the selected Monster HP bar from a computed layout.
- `CombatRuntimeScene.cs:344-380` computes the automatic layout from the selected Monster sprite bounds, while the manual mode uses the serialized `selectedMonsterStatusManual*` values exactly.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; only the existing Unity/MCP warnings remained.
- Unity refresh completed with `resulting_state: idle`, and Unity console error query returned only MCP-FOR-UNITY handler exit logs.

### History

- 2026-05-03: User requested fixing the selected Monster overhead HP slider/text/name overlap and asked for direct editability if automatic tuning was hard.
- 2026-05-03: Code Builder changed the selected Monster status visuals to separate name/HP labels, sprite-aware layout, and Inspector-visible manual overrides.

## Task: 2026-05-03 Ariel J Passive Runtime Correction

### Task title

Correct Ariel passive J `성역 선포` so its action-speed and holy-damage windows follow the Archangel Descent reference, then close the E-shield source leak and adjacent E/C runtime bugs.

### Goals

- Keep Ariel passive F-I behavior unchanged.
- Make J action speed trigger after `대천사의 강림` for 5 seconds even when E master 1 is not selected.
- Make J holy-damage bonus depend on the remaining `대천사의 강림` shield, not any generic shield/buff timer.
- Ensure `대천사의 강림` shows a visible battlefield effect when cast.
- Stop Ariel support-skill retries from running every held-input frame while the primary shot is unavailable, which was surfacing as occasional C-skill barrage behavior.

### Constraints

- Role Owner is Code Builder.
- Ground the correction in actual Ariel reference markdown and current runtime code.
- User performs Play Mode verification.
- Code Reviewer was run once earlier for this patch line, and no second review is allowed without a new explicit user request.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally validated. Code Reviewer has not been rerun because the user did not request another review.

### Next Actions

- User verifies in Play Mode that Ariel J holy-damage bonus drops as soon as the active pooled shield is no longer the E shield.
- User verifies that Ariel E now shows a visible battlefield effect on cast.
- User verifies that holding attack no longer causes Ariel C to occasionally barrage while Ariel A is reloading or on shot cooldown.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md:18-19` defines J as `대천사의 강림 이후 모든 아군 행동속도 +15%, 5초` and `대천사의 강림 방어막이 남아있는 아군의 신성 피해 +20%`.
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md:22-24` defines the E shield amount, duration, and cooldown that J depends on, and documents E as a battlefield-wide effect.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs:429` now routes E shield application through `ApplyArielUnitShield(shield, duration, true)`, and `CombatRuntimeArielSkills.cs:554-580` now only marks Archangel shield state when the new shield actually claims the pooled selected-Monster shield slot while clearing it if a stronger non-E shield replaces that slot.
- `CombatRuntimeArielSkills.cs:592-600` still reduces tracked Archangel shield value on shield absorption, so J holy-damage gating continues to decay with incoming damage.
- `CombatRuntimeArielSkills.cs:444-451` now creates the missing `ArchangelDescent` battlefield circle effect for Ariel E.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:332-356` now gates Ariel automatic support-skill retries to real firing windows, preventing held-input per-frame retries while Ariel A is blocked by reload or shot cooldown.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh finished with `resulting_state: idle`, and Unity console error query returned only MCP-FOR-UNITY handler exit logs.

### History

- 2026-05-03: User requested implementing Ariel passive skills F-J from the reference folder.
- 2026-05-03: Code Builder verified that F-I were already wired, found that J was reusing the wrong timer/state path, and applied a correction pass grounded in the Ariel E/J documents.
- 2026-05-03: User explicitly requested Code Reviewer execution; Reviewer returned NEEDS_CHANGES for the remaining J shield-source leak.
- 2026-05-03: User then requested fixing the reviewer finding plus Ariel E effect omission and Ariel C occasional barrage behavior; Code Builder applied the follow-up in runtime shield ownership, E visual spawning, and Ariel-only automatic-skill trigger cadence.

## Task: Monster Shield Bar Split Visual

### Task title

Display player Monster shields as one fixed-width HP/shield split bar.

### Goals

- Keep Monster HP and shield numeric values unchanged.
- When shield is present, draw one bar with red HP and white shield sharing the fixed visual width by ratio.
- Apply the shared visual path to the selected Monster status bar.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies shielded Monster HP bar visuals in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now has `UpdateHpShieldBarFill()` using `health + shield` as the visual total while shield is present, so HP 10 and shield 1 are drawn as adjacent red/white segments within the same root bar width.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now calls `UpdateHpShieldBarFill()` for `selectedMonsterHpBarFill` and `selectedMonsterShieldBarFill`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.

### History

- 2026-04-30: User requested League-style shield visuals for all Monster shield-granting skills.
- 2026-04-30: Code Builder changed the shared Monster HP/shield bar update path.
- 2026-04-30: User requested HP Bar `Background` color to differ from white shield; Code Builder changed shared HP bar background renderers to `Color.black`.

## Task: Ariel A-E Active And F-J Enhancement Runtime

### Task title

Implement Ariel skill documents A-E and their F-J enhancement/passive effects.

### Goals

- Read actual Ariel skill markdown under `Pakuri/reference/2.Monster/ariel`.
- Implement Ariel active skills A-E in the combat runtime.
- Implement Ariel passive/enhancement effects F-J where the current selected-Monster runtime has corresponding state.
- Keep the implementation grounded in the existing single selected Monster combat model.

### Constraints

- Role Owner is Code Builder.
- User performs Unity Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this implementation.
- Current combat runtime has one selected allied Monster, not a collection of allied units.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Code Reviewer returned FAIL for Ariel behavior mismatches, and Code Builder has applied the requested correction pass.

### Next Actions

- User verifies Ariel A-E and F-J selected effects in DebugScene or RunScene Play Mode.
- If exact multi-ally party behavior is added later, revisit Ariel "모든 아군" effects and expand them from selected Monster to the full ally collection.

### Evidence

- `boards/MON/ARIEL_MONSTER.md` contains the Ariel-specific skill slot and runtime evidence.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` was added for Ariel-specific runtime behavior.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now routes Ariel A to `FireManualArielJudgementLight(direction)` and combines Ariel damage/defense/critical modifiers with projectile hit resolution.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now tracks and displays Ariel Holy Exposure on enemies.
- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` marks Ariel A-E and F-J as runtime implemented.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity editor state returned `ready_for_tools=true`; console errors were MCP-FOR-UNITY handler logs only.
- 2026-04-30 follow-up fixes addressed Reviewer findings: Ariel A now uses `ariel-a` skill damage/range with projectile speed `17`, last-shot explosion happens from projectile cleanup position, Radiant Shield reflection receives the source attacker, and Holy damage bonuses are not pre-applied before final damage calculation.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Follow-up Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User requested Ariel A-E skill implementation plus enhancement effects after pointing to `Pakuri/reference/2.Monster/ariel`.
- 2026-04-30: Code Builder implemented Ariel runtime behavior and updated related data/state files.
- 2026-04-30: User instructed Builder to fix Code Reviewer findings; Builder applied the Ariel correction pass and did not rerun Code Reviewer because a new review was not explicitly requested.

## Task: Hold Input Primary Skill Fire

### Task title

Allow all 5 Monster A skills to keep firing while left mouse or touch input is held.

### Goals

- Change the current one-click A skill trigger into a held-input trigger.
- Preserve existing shot interval, magazine, reload, and active-skill trigger behavior.
- Support mouse left-button hold and mobile touch hold.
- Keep the change in the shared combat input path so all 5 player Monsters use the same behavior.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual files and command output.
- Do not run Unity Play Mode gameplay verification; user verifies gameplay.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that holding left mouse or touch continuously fires A skill toward the held pointer position for each Monster.
- User verifies left-button/touch-triggered active skill effects still respect their cooldowns while held.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeHud.cs` now uses held input checks: `Mouse.current.leftButton.isPressed`, `Touchscreen.current.primaryTouch.press.isPressed`, `Input.GetMouseButton(0)`, and `Input.touchCount`.
- `CombatRuntimeHud.cs` still sets `fireRequestedThisFrame = true` after converting the current pointer/touch screen position into the clamped world attack point.
- `CombatRuntimeProjectiles.cs` already gates primary fire through `shotCooldown`, `currentShotsRemaining`, and `reloadRemaining`, so held input repeats through the existing fire interval and reload rules.
- `CombatRuntimeProjectiles.cs` calls `TryTriggerEveAutomaticSkills()` whenever the shared fire request is active, preserving left-button active trigger behavior through existing cooldown logic.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing 2 Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity editor state returned `ready_for_tools=true`; Unity console error query returned MCP-FOR-UNITY client handler logs only, not project compile errors.

### History

- 2026-04-30: User requested that holding left mouse click, or mobile touch, continuously fires the 5 Monsters' A skill toward the held pointer position and keeps the same active-skill trigger behavior.
- 2026-04-30: Code Builder changed the shared combat pointer input to treat held mouse/touch as a fire request and validated compilation.

## Task: Monster A-J Skill Data Cleanup

### Task title

Prepare the 5 monster A-J skill data cleanup from reference documents.

### Goals

- Use `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html` step 5 as the implementation direction.
- Compare the 5 monster A-J skill documents under `Pakuri/reference/2.Monster` against current `Assets/Data/GameData/Monsters/*.asset`.
- Represent A as the default active skill, B-E as selectable actives, F as a selectable base passive, and G-J as passives unlocked by their matching active skills.
- Keep this pass focused on data/selection/unlock structure before full runtime effects.

### Constraints

- Role Owner is Designer until explicit Builder handoff.
- Ground all claims in actual files and command output.
- Current `SkillDefinition`/`PassiveDefinition` can store base skill/passive fields but has no structured fields for active enhancements, passive enhancements, or master skill branches.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation completed, and the user reported Play Mode verification completed. The required one-shot external Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; the user chose not to fix that reviewer finding for now. The finding is limited to trailing whitespace in `Pakuri/Assets/Data/GameData/Monsters/eve.asset`.

### Next Actions

- Continue to the next requested design or implementation task.
- If the user later wants the reviewer finding cleaned, remove the trailing whitespace in `eve.asset`, rerun `git diff --check`, rebuild, and update this block.

### Evidence

- Roadmap report step 5 says to organize monster A-J skill data first, completing selection/unlock structure before all complex effects.
- `Pakuri/reference/2.Monster` contains `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `monster-skill-patterns.md`, 5 monster tower documents, and 50 A-J skill documents.
- `SkillDefinition.cs` currently contains `SkillId`, `DisplayName`, `Slot`, `RuntimeKind`, `ImplementationState`, damage/range/cooldown/magazine fields, `StatusEffectId`, and `Summary`.
- `PassiveDefinition` currently contains `PassiveId`, `DisplayName`, `Slot`, `RequiredActiveSlot`, `ImplementationState`, and `Summary`.
- `MonsterDefinition.cs` currently stores `InitialRewardChoices`, `ActiveSkills`, and `PassiveSkills`, but no active-enhancement, passive-enhancement, or master-skill structured data.
- Current monster assets already contain A-E active entries and F-J passive entries; all A entries are `RuntimeImplemented`, B-E and F-J are `DataOnly`.
- `monster-basic-rule.md` states each monster starts with active A learned, starts with no passives learned, F is selectable without a specific active unlock, and G-J unlock after the matching B-E active is learned.
- `skill-choice-pool-rule.md` defines active enhancements, passive enhancements, and master skill candidates, but the current SO model has no dedicated structures for these candidates.
- `SkillDefinition.cs` now adds `SkillChoiceDefinition`, `SkillIcon`, `SkillEffectPrefab`, `DescriptionText`, active `EnhancementChoices`, active `MasterSkillChoices`, passive `EnhancementChoices`, `IsDefaultLearned`, and `IsAvailableWithoutActiveRequirement`.
- `PakuriGameDataSeeder.cs` now reads `Pakuri/reference/2.Monster/{monster}/skill/*.md` and populates A-E active and F-J passive data from those documents.
- `RunCombatUiController.cs` now adds structured active enhancements, passive enhancements, and master skill choices to the prisoner offering pool; it bypasses the active requirement only when `PassiveDefinition.IsAvailableWithoutActiveRequirement` is true.
- After running `Pakuri/Seed Default Game Data`, each monster asset has 5 `SkillId` entries, 5 `PassiveId` entries, 10 `EnhancementChoices` blocks, and 5 `MasterSkillChoices` blocks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing 2 Unity/MCP reference warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; verified with `git diff --check -- Pakuri\Assets\Data\GameData\Monsters\eve.asset`, which reports trailing whitespace at lines 225, 238, 288, 301, 352, and 365.
- Added `Pakuri/reference/Report/2026-04-29-roadmap-implementation-result.html` comparing today's implementation result against `2026-04-28-reference-implementation-roadmap.html`.
- Added `Pakuri/reference/Report/2026-04-29-token-optimization-savings.html` estimating token savings from document parsing/token reduction based on measured file sizes.

### History

- 2026-04-29: User requested starting roadmap step 5, monster A-J skill data cleanup, and asked for questions if needed.
- 2026-04-29: User selected the data-structure expansion path, requested per-skill icon/effect/description fields, confirmed reference documents are the conflict source of truth, and confirmed F passive should be selectable from prisoner offering instead of default-granted.
- 2026-04-29: Code Builder expanded skill data structures, connected structured choices to prisoner offering, seeded monster A-J data from reference documents, and ran build/Unity validation.
- 2026-04-29: External Code Reviewer one-shot review returned `NEEDS_CHANGES` for trailing whitespace in `eve.asset`; Builder paused for user instruction per AGENTS.md.
- 2026-04-29: User reported Play Mode verification completed and chose not to fix the reviewer-raised whitespace issue for now.
- 2026-04-29: Designer added roadmap comparison and token optimization savings HTML reports under `Pakuri/reference/Report`.

## Task: Combat Monster Enemy Implementation

### Task title

?꾪닾 湲곕낯 洹쒖튃 湲곕컲 Stage 1 ??/ Monster ?곗씠??/ ?쇳빐 怨꾩궛 濡쒓렇 援ы쁽

### Goals

- `combat-monster-enemy-implementation-plan.html`??諛⑺뼢?濡?怨듯넻 ?꾪닾 紐⑤뜽, ?띿꽦蹂?諛⑹뼱??怨꾩궛, Stage 1 ???곗씠?곗? ?고????④낵瑜?援ы쁽?쒕떎.
- Monster 5紐낆쓽 ?≫떚釉?A~E, ?⑥떆釉?F~J ?곗씠???щ’??留뚮뱺??
- Monster媛 ?곸뿉寃??쇳빐瑜??낇옄 ??Unity Console `Debug.Log`濡?怨꾩궛?앷낵 ?곸슜 ?쇳빐瑜?媛꾨떒??異쒕젰?쒕떎.

### Constraints

- Role Owner??Code Builder??
- ?ъ슜?먭? ?뚮젅???ㅽ뻾 寃利앹? 吏곸젒 ?섑뻾?쒕떎怨??덉쑝誘濡?Codex??Play Mode瑜??ㅽ뻾?섏? ?딅뒗??
- ?ъ슜?먭? ?먯껜 由щ럭源뚯?留??붿껌?덉쑝誘濡??몃? Reviewer???몄텧?섏? ?딄퀬 Builder ?먯껜 由щ럭? 鍮뚮뱶/肄섏넄 ?뺤씤源뚯?留??섑뻾?덈떎.
- ?먮떒? ?ㅼ젣 肄붾뱶, asset, 紐낅졊 異쒕젰??洹쇨굅?쒕떎.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- ?ъ슜?먭? Unity Play Mode?먯꽌 MainMenuScene ?먮뒗 RunScene ?먮쫫???ㅽ뻾??Stage 1 ???ㅽ룿, ???≫떚釉??⑥떆釉? 紐ъ뒪???쇳빐 怨꾩궛 濡쒓렇瑜??뺤씤?쒕떎.
- Unity Console?먯꽌 `[CombatDamage]` 濡쒓렇媛 怨듦꺽?? ?ㅽ궗, ??? ?띿꽦 諛⑹뼱??怨듭떇, 理쒖쥌 ?곸슜 ?쇳빐瑜?異쒕젰?섎뒗吏 ?뺤씤?쒕떎.

### Evidence

- 異붽???怨듯넻 ?꾪닾 ??? `Pakuri/Assets/Scripts/Combat/CombatStatModels.cs`.
- ?뺤옣???쇳빐 怨꾩궛: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`媛 ?띿꽦蹂?諛⑹뼱?? 怨좎젙/?쇱꽱??諛⑹뼱 蹂댁젙, 移섎챸? ??? 理쒖쥌 諛곗쑉, `FormulaLog`瑜?泥섎━?쒕떎.
- 異붽????곗씠????? `Pakuri/Assets/Scripts/Data/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs`.
- ?뺤옣??移댄깉濡쒓렇/紐ъ뒪???곗씠?? `GameDataCatalog.cs`??`StageOneEnemies`, `MonsterDefinition.cs`??`PrimaryAttribute`, `BaseStats`, `Defenses`, `ActiveSkills`, `PassiveSkills`瑜?異붽??덈떎.
- ?꾪닾 ?곌껐: `RunFlowController.cs`, `RunSceneBootstrap.cs`媛 `GameDataCatalog`瑜?`EveVerticalSliceController.BeginConfiguredDay(...)`???섍릿??
- ?꾪닾 ?고??? `EveVerticalSliceController.cs`媛 Stage 1 ??????ъ슜?섍퀬, 寃??諛⑺뙣蹂?沅곸닔/?꾩쟻/?ъ젣/?섑샇???怨듦꺽????⑹궗 移대┛???≫떚釉??⑥떆釉??고????④낵瑜?泥섎━?쒕떎.
- 11?쇱감??Stage 1 洹쒖튃?濡??섑샇??? 怨듦꺽??? ?⑹궗 移대┛??紐⑤몢 蹂댁뒪 ?ㅽ룿 ??곸쑝濡?泥섎━?섎룄濡??섏젙?덈떎.
- 紐ъ뒪?곌? ?곸뿉寃??쇳빐瑜?以???`Debug.Log("[CombatDamage] ...")`濡??띿꽦 諛⑹뼱??怨듭떇, 理쒖쥌 ?쇳빐, ?ㅼ젣 ?곸슜 ?쇳빐, ?⑥? 蹂댄샇留?HP瑜?異쒕젰?쒕떎.
- `Pakuri/Seed Default Game Data` 硫붾돱 ?ㅽ뻾 ??`Pakuri/Assets/Data/GameData/Enemies` ?꾨옒 Stage 1 ??8醫?asset???앹꽦?먭퀬, `GameDataCatalog.asset`??`StageOneEnemies` 李몄“媛 湲곕줉?먮떎.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` ?뺤씤 寃곌낵 `PrimaryAttribute`, `ActiveSkills`, `PassiveSkills`, `ImplementationState`媛 湲곕줉?먮떎.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`???ㅻ쪟 0媛쒕줈 ?듦낵?덈떎. ?⑥? 寃쎄퀬??湲곗〈 Unity/MCPForUnity `System.Net.Http`, `System.IO.Compression` 踰꾩쟾 異⑸룎 寃쎄퀬 2媛쒕떎.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`???ㅻ쪟 0媛쒕줈 ?듦낵?덈떎. ?⑥? 寃쎄퀬???숈씪??湲곗〈 李몄“ 寃쎄퀬 2媛쒕떎.
- Unity console error 議고쉶??MCP-FOR-UNITY client handler exit 濡쒓렇留?諛섑솚?덇퀬, ???꾨줈?앺듃 而댄뙆???ㅻ쪟???뺤씤?섏? ?딆븯??

### History

- 2026-04-27: ?ъ슜??吏?쒕줈 Designer ?ㅺ퀎 HTML 湲곗? 援ы쁽??李⑹닔?덈떎.
- 2026-04-27: `AGENTS.md`, `BLACKBOARD.md`, Unity MCP skill 吏移⑥쓣 癒쇱? ?뺤씤?덈떎.
- 2026-04-27: 湲곗〈 `EveVerticalSliceController`媛 ??諛⑹뼱?μ쓣 `0f`濡??섍린??援ъ“?꾩쓣 ?뺤씤?섍퀬 ?띿꽦蹂?諛⑹뼱??怨꾩궛??異붽??덈떎.
- 2026-04-27: Stage 1 ???곗씠?곗? Monster 5紐??ㅽ궗/?⑥떆釉??곗씠???먯궛 ?앹꽦???꾪빐 `PakuriGameDataSeeder.cs`瑜??뺤옣?섍퀬 硫붾돱瑜??ㅽ뻾?덈떎.
- 2026-04-27: ?먯껜 由щ럭 以?11?쇱감 ?ㅼ쨷 蹂댁뒪 洹쒖튃 ?꾨씫??諛쒓껄???섑샇??? 怨듦꺽??? ?⑹궗 移대┛??紐⑤몢 ?ㅽ룿?섎룄濡??섏젙?덈떎.
- 2026-04-27: ?고????먮뵒??鍮뚮뱶? Unity 肄섏넄 error ?뺤씤源뚯? ?꾨즺?덈떎.

## Task: Combat Monster Enemy Implementation Plan

### Task title

?꾪닾 湲곕낯 洹쒖튃, Monster ?ㅽ궗, Stage 1 ??援ы쁽 諛⑹떇 HTML ?ㅺ퀎

### Goals

- `Pakuri/reference/3.combat` ?꾪닾 湲곕낯 湲고쉷?쒖? `Pakuri/reference/5.enemy` ??湲고쉷?쒕? ?ㅼ젣 ?뚯씪 湲곗??쇰줈 ?쎄퀬 援ы쁽 諛⑺뼢???뺣━?쒕떎.
- ?꾩슂??寃쎌슦 `Pakuri/data` CSV????븷???뺤씤?섎릺, ?ㅼ젣 臾몄꽌? 異⑸룎?섎뒗 媛믪? 洹몃?濡??ъ슜?섏? ?딅뒗??
- Monster???띿꽦蹂?諛⑹뼱?? ?≫떚釉??ㅽ궗, 湲곕낯 ?λ젰移? ?⑥떆釉뚯? Stage 1 ??援ы쁽 諛⑹떇??HTML 臾몄꽌濡??뺣━?쒕떎.

### Constraints

- Role Owner??Designer?대ŉ ?ㅼ젣 C# 援ы쁽? ?섏? ?딅뒗??
- 紐⑤뱺 ?먮떒? ?ㅼ젣 臾몄꽌, CSV, ?꾩옱 C# 肄붾뱶 ?댁슜??洹쇨굅?쒕떎.
- ?꾩옱 ?꾨줈?앺듃?먮뒗 CSV ?고???濡쒕뜑媛 ?뺤씤?섏? ?딆븯?쇰?濡?CSV 吏곸젒 濡쒕뵫??援ы쁽??寃껋쿂???곗? ?딅뒗??

### Role Owner

Designer

### Status

Completed.

### Next Actions

- ?ъ슜?먭? 援ы쁽???먰븯硫???HTML??湲곗??쇰줈 Code Builder?먭쾶 handoff?쒕떎.
- Builder ?④퀎?먯꽌??怨듯넻 ?꾪닾 ?곗씠??紐⑤뜽, ?띿꽦蹂?諛⑹뼱??怨꾩궛, Stage 1 ???먯궛, ?ㅽ궗 ?ㅽ뻾湲??쒖꽌濡??ㅼ뼱媛꾨떎.

### Evidence

- ?쎌? ?꾪닾 臾몄꽌: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `buff-debuff.md`, `realtime-damage-meter.md`.
- ?쎌? ??臾몄꽌: `Pakuri/reference/5.enemy/stage-basic-rules.md`, `enemy-stage-index.md`, `stage-1-enemies.md`.
- ?쎌? Monster 臾몄꽌: `Pakuri/reference/2.Monster/monster-basic-rule.md`, `monster-skill-patterns.md`, `skill-choice-pool-rule.md`, 媛?Monster tower 臾몄꽌? ?ㅽ궗 臾몄꽌 紐⑸줉.
- ?뺤씤??CSV: `Pakuri/data/enemies.csv`, `enemy_runtime.csv`, `skills.csv`, `skill_runtime.csv`, `ally_units.csv`, `ally_runtime.csv`, `status_effects.csv`, `levelup_choices.csv`, `skill_branches.csv`, `levelup_rules.csv`.
- ?뺤씤???꾩옱 肄붾뱶: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `EveVerticalSliceController.cs`, `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`.
- ?앹꽦??臾몄꽌: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

### History

- 2026-04-27: AGENTS.md? BLACKBOARD.md瑜?癒쇱? ?쎌뿀??
- 2026-04-27: `rg`媛 ?ㅼ튂?섏뼱 ?덉? ?딆븘 PowerShell `Get-ChildItem`怨?`Get-Content`濡??ㅼ젣 ?뚯씪 紐⑸줉怨??댁슜???뺤씤?덈떎.
- 2026-04-27: `Pakuri/reference/run-systems-integration-summary-report.html`??BLACKBOARD 湲곕줉怨??щ━ ?대떦 寃쎈줈???녾퀬, ?ㅼ젣 ?뚯씪? `Pakuri/reference/Report/run-systems-integration-summary-report.html`???덉쓬???뺤씤?덈떎.
- 2026-04-27: Stage 1 ??臾몄꽌? CSV???꾩옱 ???곗씠?곌? 吏곸젒 ?쇱튂?섏? ?딆쑝誘濡?Stage 1 ?섏튂??臾몄꽌 ?곗꽑, CSV???ㅽ궎留?李멸퀬濡??뺣━?덈떎.
- 2026-04-27: Designer ?ㅺ퀎 HTML `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`瑜?異붽??덈떎.

## Task: Monster Select Run UI Expansion Plan

### Task title

紐ъ뒪???좏깮 UI, Run ?쒖옉, ?꾪닾 ???ㅽ궗 媛뺥솕 ?먮쫫 ?뺤옣 ?ㅺ퀎 HTML ?묒꽦

### Goals

- ?꾩옱 援ы쁽???대툕 ?⑤룆 ?꾪닾 ?꾨줈?좏??낆쓣 湲곗??쇰줈, 紐ъ뒪???좏깮 UI? Run ?쒖옉 ?먮쫫???대뼸寃??쇰컲?뷀븷吏 ?뺣━?쒕떎.
- `2.Monster` 臾몄꽌援곌낵 `skill-choice-pool-rule.md`, `combat-reward-system.md`瑜?洹쇨굅濡?紐ъ뒪?곕퀎 ?쒖옉 ?ㅽ궗 A, 理쒕? ?≫떚釉?3媛? 理쒕? ?⑥떆釉?3媛? ?꾪닾 ??媛뺥솕 ?좏깮 ?먮쫫???ㅺ퀎?쒕떎.
- 援ы쁽 ?꾩뿉 ?꾩슂??怨듯넻 ?쒖뒪?? UI ?⑤꼸 援ъ“, ?대┛ 吏덈Ц??HTML 臾몄꽌濡??④릿??

### Constraints

- ?ㅼ젣 ?꾩옱 肄붾뱶, ?ㅼ젣 ???곹깭, ?ㅼ젣 reference 臾몄꽌??洹쇨굅?댁꽌留??곷뒗??
- 援ы쁽?섏? ?딆? UI/???쒖뒪?쒖쓣 ?대? ?덈뒗 寃껋쿂???곸? ?딅뒗??
- ???묒뾽? Designer ?ㅺ퀎 臾몄꽌 ?묒꽦?대ŉ, ?ㅼ젣 肄붾뱶 援ы쁽? ?ы븿?섏? ?딅뒗??

### Role Owner

Designer

### Status

Completed

### Next Actions

- ?ъ슜?먭? ?먰븯硫????ㅺ퀎 臾몄꽌瑜?湲곗??쇰줈 Designer handoff瑜??묒꽦??Code Builder 援ы쁽 踰붿쐞瑜?怨좎젙?쒕떎.
- ?ъ슜?먭? 紐낆떆?곸쑝濡?援ы쁽??吏?쒗븯硫? 癒쇱? UI 堉덈?? RunSession 遺꾨━遺???ㅼ뼱媛??寃껋씠 ?덉쟾?섎떎.
- 1李?援ы쁽 踰붿쐞??臾몄꽌媛 ?꾨퉬??`?꾨━??, `?대툕`, `?몄씤`, `踰좉?` 4紐ъ뒪???곗꽑?쇰줈 ?↔퀬, `由?? ?붾? ?곹깭濡??붾떎.
- 由곗쓽 `g~j` ?⑥떆釉?臾몄꽌媛 ?ㅼ젣 ??μ냼???놁쑝誘濡? 由곗쓣 ?뚮젅??媛????곸쑝濡??щ━???묒뾽? ?꾩냽 臾몄꽌 蹂닿컯 ?댄썑濡?誘몃，??

### Evidence

- `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`留??꾩옱 寃뚯엫 ?꾩슜 ?ㅽ겕由쏀듃濡?議댁옱?쒕떎.
- ?꾩옱 ?쒖꽦 ?ъ? `Assets/Scenes/SampleScene.unity`?대ŉ 猷⑦듃 ?ㅻ툕?앺듃??`Main Camera`, `Global Light 2D`, `CombatRoot`??
- `CombatRoot` ?섏쐞?먮뒗 `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`媛 ?덈떎.
- `Pakuri/Assets` ?꾨옒?먯꽌??`NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`媛 ?뺤씤??蹂꾨룄 UI ?먯궛???놁쓬???ы솗?명뻽??
- `Pakuri/reference/2.Monster/monster-basic-rule.md`??紐ъ뒪?곌? ?≫떚釉?A瑜?湲곕낯 ?듬뱷 ?곹깭濡??쒖옉?섍퀬, ??以??≫떚釉?理쒕? 3媛? ?⑥떆釉?理쒕? 3媛쒕? 媛吏꾨떎怨??뺤쓽?쒕떎.
- `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`???좉퇋 ?≫떚釉? ?좉퇋 ?⑥떆釉? ?≫떚釉??뱀꽦, 留덉뒪???ㅽ궗???섎굹???좏깮吏 ?濡??⑹퀜 3媛쒕? ?쒖떆?섎뒗 洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/reference/4.run/combat-reward-system.md`???쇰컲 ?꾪닾/以묎컙蹂댁뒪/蹂댁뒪 ?꾪닾蹂??щ줈, ?좊Ъ, 怨⑤뱶, ?대몺???붿쟻 蹂댁긽 洹쒖튃???뺤쓽?쒕떎.
- `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`濡??꾩옱 援ы쁽 ???紐ъ뒪??5醫낆쓣 ?뺤씤?덈떎.
- ?ъ슜???묐떟?쇰줈 紐⑤뱺 紐ъ뒪?곕뒗 ?⑥떆釉??щ’ `F~J` 珥?5媛쒕? 媛吏硫? ??以??ㅼ젣濡??좏깮 媛?ν븳 ?⑥떆釉뚮뒗 理쒕? 3媛쒕씪???ㅺ퀎 湲곗????뺤젙?덈떎.
- ?ъ슜???묐떟?쇰줈 ?대쾲 踰붿쐞???щ줈 蹂댁긽? `?쒖떆留??섎뒗 ?뺣낫`濡?泥섎━?섍퀬, ?곸엯 ?쒖뒪?쒖? ?섏쨷??遺숈씠湲곕줈 ?뺤젙?덈떎.
- ?ъ슜???묐떟?쇰줈 1李?援ы쁽? 臾몄꽌媛 ?꾨퉬??4紐ъ뒪??`?꾨━??, `?대툕`, `?몄씤`, `踰좉?`)遺??吏꾪뻾?섍퀬, `由?? ?붾? ?곹깭濡??먭린濡??뺤젙?덈떎.
- ?ㅼ젣 ??μ냼 ?뺤씤 寃곌낵 ?꾨━?? ?대툕, ?몄씤, 踰좉???`f~j` ?⑥떆釉?臾몄꽌媛 紐⑤몢 議댁옱?섏?留? 由곗? `f-ambidextrous.md`留??덇퀬 `g~j` ?⑥떆釉?臾몄꽌???꾩쭅 ?녿떎.
- ???ㅺ퀎 臾몄꽌 `Pakuri/reference/monster-select-run-ui-expansion-plan.html`瑜?異붽??덈떎.

### History

- 2026-04-25: `AGENTS.md`, `BLACKBOARD.md`瑜??ㅼ떆 ?쎄퀬 ?꾩옱 ?묒뾽 洹쒖튃怨?湲곗〈 ?묒뾽 釉붾줉???ы솗?명뻽??
- 2026-04-25: `2.Monster` ?대뜑 ?꾩껜, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`, 媛?紐ъ뒪?????臾몄꽌瑜??쎌뿀??
- 2026-04-25: ?꾩옱 肄붾뱶? ???곹깭瑜??ㅼ떆 ?뺤씤???꾩옱 援ы쁽???대툕 ?⑤룆 ?꾪닾 ?꾨줈?좏??낃낵 ?꾩떆 HUD ?섏??꾩쓣 ?ы솗?명뻽??
- 2026-04-25: UI ?먯궛 遺?? 蹂댁긽 ? 誘멸뎄?? ?띿꽦/?곹깭 怨듯넻 ?쒖뒪??遺議깆쓣 ?꾩옱 ?뺤옣 ?묒뾽???듭떖 媛?쑝濡??뺣━?덈떎.
- 2026-04-25: 紐ъ뒪???좏깮 UI, Run ?쒖옉, ?꾪닾 ??蹂댁긽/?ㅽ궗 ?좏깮 ?먮쫫???뺣━???ㅺ퀎 HTML `Pakuri/reference/monster-select-run-ui-expansion-plan.html`瑜?異붽??덈떎.
- 2026-04-25: ?ъ슜???듬???諛섏쁺???⑥떆釉뚮뒗 ?щ’ `F~J` 珥?5媛? ??以?理쒕? 3媛??듬뱷?쇰줈 ?ㅺ퀎瑜?怨좎젙?덇퀬, ?щ줈 蹂댁긽? ?곗꽑 ?쒖떆 ?꾩슜 ?뺣낫濡?泥섎━?섍린濡?湲곕줉?덈떎.
- 2026-04-25: ?ㅼ젣 ??μ냼?먯꽌 由곗쓽 `g~j` ?⑥떆釉?臾몄꽌媛 ?놁쓬???ㅼ떆 ?뺤씤?? 臾몄꽌 湲곕컲 ?꾩껜 紐ъ뒪??援ы쁽 ?꾩뿉 ?⑥? ?먮즺 媛?쑝濡?湲곕줉?덈떎.
- 2026-04-25: ?ъ슜???듬???諛섏쁺??1李?援ы쁽 踰붿쐞瑜?`?꾨━??, `?대툕`, `?몄씤`, `踰좉?` 4紐ъ뒪???곗꽑?쇰줈 怨좎젙?섍퀬, `由?? ?붾? ?곹깭濡??④린湲곕줈 湲곕줉?덈떎.

## Task: Run Flow UICanvas Prototype Implementation

### Task title

`run-systems-integration-summary-report.html` 湲곗? 泥?援ы쁽 ?щ씪?댁뒪 李⑹닔

### Goals

- 5紐ъ뒪???좏깮, `RunSession`, `RunFlowController`, `UICanvas` 湲곕컲 ?먮쫫??泥?援ы쁽 ?щ씪?댁뒪瑜?留뚮뱺??
- ?뺤쟻 ?곗씠?곕뒗 CSV ?고???濡쒕뱶 ???Unity ?꾨줈?앺듃 ?대? ?먯궛?쇰줈 留뚮뱺??
- ?꾩옱 `EveVerticalSliceController`瑜??좏깮 紐ъ뒪??湲곕컲 怨듯넻 A ?ㅽ궗 ?꾨줈?좏????꾪닾? A/F 理쒖냼 蹂댁긽 猷⑦봽媛 媛?ν븳 援ъ“濡??곕떎.

### Constraints

- ?ъ슜?먯쓽 ?붿껌?濡??좊땲???뚮젅???ㅽ뻾 寃利앹? ?ъ슜?먯뿉寃?留↔린怨? ???肄붾뱶/???먯궛 以鍮꾩? ?먮뵒???곹깭 ?뺤씤源뚯?留??쒕떎.
- UI??`UICanvas` 湲곗??쇰줈 ?ъ뿉 吏곸젒 諛곗튂?쒕떎.
- ?꾩옱 ?ъ슜?먯쓽 吏?쒕줈 ?몃? Reviewer ?④퀎???좎떆 以묒??섍퀬, Builder 醫낅즺 ?꾩뿉??媛꾨떒???먯껜 ?먭?留??섑뻾?쒕떎.
- 援ы쁽?섏? ?딆? B~E, G~J, ?좊Ъ 3??, ?꾩껜 ?쇳빀 蹂댁긽 ?? ?대쾲 ?щ씪?댁뒪 踰붿쐞???ｌ? ?딅뒗??

### Role Owner

Code Builder

### Status

Builder changes applied. ?몃? Reviewer 1??寃곌낵 諛섏쁺源뚯????꾨즺?먭퀬, ?댄썑 Reviewer ?④퀎???ъ슜??吏?쒕줈 ?좎떆 以묒??덈떎. `LegacyRuntime.ttf` 援먯껜? Unity ?ъ뺨?뚯씪源뚯? 留덉낀怨? ?꾩옱???ъ슜???뚮젅??寃利??湲??곹깭??

### Next Actions

- ?ъ슜?먭? Unity?먯꽌 ?뚮젅??紐⑤뱶濡?`RunUICanvas` ?숈옉, 5紐ъ뒪???좏깮, ?꾪닾 吏꾩엯, 理쒖냼 蹂댁긽 ?좏깮, ?ㅼ쓬 ?쇱감 吏꾪뻾??寃利앺븳??
- 寃利?以?UI 諛곗튂 臾몄젣???낅젰 臾몄젣, ?꾪닾 ?먮쫫 臾몄젣瑜??뺤씤?섎㈃ 洹?洹쇨굅瑜?諛쏆븘 ?ㅼ쓬 Builder ?섏젙?쇰줈 ?댁뼱媛꾨떎.
- ?댄썑 ?뺤옣? `?좊Ъ 3??`, `?좉퇋 ?≫떚釉??⑥떆釉??뱀꽦/留덉뒪???꾩껜 ?`, `B/G, C/H, D/I, E/J` ?쒖쑝濡?媛꾨떎.

### Evidence

- ???고????곗씠???ㅽ겕由쏀듃 `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`瑜?異붽??덈떎.
- ?먮뵒???쒕뱶 ?ㅽ겕由쏀듃 `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`瑜?異붽??덇퀬, Unity 硫붾돱 `Pakuri/Seed Default Game Data` ?ㅽ뻾?쇰줈 `Assets/Data/GameData/GameDataCatalog.asset`? 5媛?紐ъ뒪???먯궛???앹꽦?덈떎.
- ?????먮쫫 ?ㅽ겕由쏀듃 `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`瑜?異붽??덈떎.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`瑜??좏깮 紐ъ뒪??湲곕컲 怨듯넻 A ?ㅽ궗 ?꾨줈?좏????꾪닾? 理쒖냼 蹂댁긽 猷⑦봽瑜?泥섎━?섎룄濡??ш쾶 ?섏젙?덈떎.
- Unity ??`Assets/Scenes/SampleScene.unity`??猷⑦듃 `RunUICanvas`? `EventSystem`??吏곸젒 ?앹꽦?섍퀬 ??ν뻽??
- Unity asset search 寃곌낵 `Assets/Data/GameData/GameDataCatalog.asset`? `Assets/Data/GameData/Monsters/*.asset` 5媛쒓? ?ㅼ젣濡??앹꽦?먮떎.
- Unity root hierarchy ?ы솗??寃곌낵 `RunUICanvas`?먮뒗 `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`媛 遺숈뿀怨? `EventSystem`?먮뒗 `EventSystem`, `InputSystemUIInputModule`媛 遺숈뿀??
- ?몃? Reviewer 1??寃곌낵????媛吏 ?댁뒋瑜?吏?곹뻽?? 蹂댁긽 ?④낵媛 ?ㅼ쓬 ?쇱감???좎??섏? ?딅뒗 臾몄젣, ?ㅽ뀒?댁? 諛곗쑉???꾪닾/蹂댁긽??諛섏쁺?섏? ?딅뒗 臾몄젣, ?뚮젅??以?踰꾪듉 ?ъ깮?????뚮㈇ ?꾪뿕.
- 洹?吏?곸쓣 諛섏쁺??`RunSession`???꾩쟻 蹂댁긽 ?섏튂瑜?異붽??섍퀬, `EveVerticalSliceController.BeginConfiguredDay(...)`媛 ?몄뀡 ?꾩쟻 蹂댁긽???ㅼ떆 ?곸슜?섎룄濡??섏젙?덈떎.
- 媛숈? ?섏젙?먯꽌 `EveVerticalSliceController`??`stageIndex` 湲곕컲 ??泥대젰 諛곗쑉怨??대몺???붿쟻 吏湲?諛곗쑉??諛섏쁺?섎룄濡??섏젙?덈떎.
- `RunFlowController.ClearButtons(...)`???뚮젅??以??ъ깮??踰꾪듉??媛숈? ?대쫫?쇰줈 ?ъ궗?⑸릺吏 ?딅룄濡?`QueuedForDestroy` ?대쫫 蹂寃????쒓굅?섎룄濡??섏젙?덈떎.
- 2026-04-26 ?ъ슜???뚮젅??寃利앹뿉??`RunFlowController.ResolveReferences()`??`Arial.ttf` 李몄“媛 Unity ?댁옣 ?고듃 ?뺤콉怨?留욎? ?딆븘 `ArgumentException`??諛쒖깮?덇퀬, ?대? `LegacyRuntime.ttf`濡?援먯껜?덈떎.
- `LegacyRuntime.ttf` 援먯껜 ??Unity ?ㅽ겕由쏀듃 ?ъ뺨?뚯씪???붿껌?덇퀬, 理쒓렐 Unity console 20媛?濡쒓렇 ?ы솗?몄뿉?쒕뒗 ?숈씪??`Arial.ttf` ?덉쇅媛 ?ㅼ떆 蹂댁씠吏 ?딆븯??
- ?몃? Reviewer ?ъ떎?됱? 10遺???꾩븘???덉뿉 ?앸굹吏 ?딆븯怨? ?댄썑 Reviewer ?④퀎???ъ슜??吏?쒕줈 ?좎떆 以묒??덈떎.

### History

- 2026-04-26: Designer 湲곗??쇰줈 ?꾩옱 HTML怨??ㅼ젣 肄붾뱶/???곹깭瑜??ㅼ떆 ?쎄퀬 泥?Builder ?щ씪?댁뒪 踰붿쐞瑜?`?뺤쟻 ?곗씠???먯궛 + RunSession/RunFlowController + UICanvas + A/F 理쒖냼 蹂댁긽 猷⑦봽`濡?怨좎젙?덈떎.
- 2026-04-26: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`瑜??덈줈 異붽??덈떎.
- 2026-04-26: `Pakuri/Seed Default Game Data`瑜??ㅽ뻾??5紐ъ뒪??湲곕낯 ?먯궛怨?`GameDataCatalog.asset`瑜??앹꽦?덈떎.
- 2026-04-26: `RunUICanvas`, `EventSystem`???ъ뿉 異붽??섍퀬 ??ν뻽??
- 2026-04-26: ?몃? Reviewer 1?뚭? 蹂댁긽 ?좎?, ?ㅽ뀒?댁? 諛곗쑉, 踰꾪듉 ?ъ깮??臾몄젣瑜?吏?곹뻽怨? Builder媛 媛숈? ?댁뿉?????댁뒋瑜??섏젙?덈떎.
- 2026-04-26: ?섏젙 ??Unity console?먯꽌????而댄뙆???ㅻ쪟媛 蹂댁씠吏 ?딆븯怨? ?몃? Reviewer ?ъ떎?됱? ?쒓컙 珥덇낵濡?醫낅즺?먮떎.
- 2026-04-26: ?ъ슜???뚮젅??寃利앹뿉??`Resources.GetBuiltinResource<Font>("Arial.ttf")` ?덉쇅媛 蹂닿퀬?먭퀬, `RunFlowController`??湲곕낯 ?고듃瑜?`LegacyRuntime.ttf`濡?援먯껜?덈떎. 媛숈? ?쒖젏???ъ슜???붿껌?쇰줈 ?몃? Reviewer ?④퀎???좎떆 以묒??섍퀬 ?먯껜 ?먭?留??좎??섍린濡??덈떎.
- 2026-04-26: `LegacyRuntime.ttf` 援먯껜 ??Unity ?ъ뺨?뚯씪怨?理쒓렐 肄섏넄 濡쒓렇瑜??ㅼ떆 ?뺤씤?덇퀬, ?숈씪???고듃 ?덉쇅???ы쁽?섏? ?딆븯??

