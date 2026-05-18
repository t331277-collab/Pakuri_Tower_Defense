## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/ARIEL_MONSTER.md`.

# ARIEL_MONSTER

## Scope

Ariel dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Ariel file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Task: 2026-05-17 Ariel-A Common Projectile Runtime Connection

### Task title

Connect Ariel-A Judgement Light through the shared InGame projectile path.

### Goals

- Route `ariel-a` to the shared `ProjectileSkillExecutor` / `InGameProjectileActor` path.
- Use the user-authored `Assets/Prefab/Skill/Ariel/Airel_A.prefab` as the Ariel-A projectile visual.
- Record which Ariel-A reference behavior is covered by the common projectile path and which behavior remains unsupported.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- Current runtime source schema does not expose per-skill base pierce count or per-skill projectile speed, so Ariel-A base pierce `1` and projectile speed `17` are mapped explicitly in `InGameSkillDefinitionMapper` from `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`.
- The common projectile path covers the base straight projectile, damage, magazine, reload, shot interval, prefab instantiation, and pierce. It does not implement Ariel-A critical rolls, shielded-ally damage scaling, White Judgement last-shot explosions, or Guiding Light holy exposure.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Builder implementation completed and local non-gameplay checks passed. 2026-05-18 Ariel-A projectile speed and pierce are now owned by `monster_skills.csv` instead of skill-ID-specific mapper code. 2026-05-18 supported runtime status labels can now be edited directly in CSV when `status_effect_id` is blank.

### Next Actions

- User verifies in NewRunScene Play Mode that Ariel-A fires `Airel_A.prefab`, damages enemies, and pierces one extra target.
- Add data/source schema fields for per-skill projectile speed and base pierce if more skills need those values without skill-ID-specific mapper exceptions.
- Implement separate runtime support before claiming Ariel-A master effects or shielded-ally scaling are active.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `NewRunScene.unity` now own the Ariel-A prefab mapping; `monster_skills.csv` no longer stores a base `skill_effect_prefab_path` column.
- `Pakuri/Assets/CSVData/SkillData.csv` now includes the Ariel-A reference row with base damage `18`, spell coefficient `1`, magazine `7`, reload `4.6`, shot interval `0.36`, pierce `1`, and projectile speed `17`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now serializes `arielAProjectilePrefab` and resolves `"ariel-a"` to it.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` assigns `arielAProjectilePrefab` to `Assets/Prefab/Skill/Ariel/Airel_A.prefab` GUID `66fcb365022930d4681ad320e5fff520`.
- `Pakuri/Assets/Prefab/Skill/Ariel/Airel_A.prefab` now has trigger `BoxCollider2D` and `Pakuri.InGame.InGameProjectileActor`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now includes `Assets/Prefab/Skill/Ariel/Airel_A.prefab`.
- CSV check returned `UpperA=ariel-a`, `Pierce=1`, `Speed=17`, `SourcePrefab=Assets/Prefab/Skill/Ariel/Airel_A.prefab`, `SourceMagazine=7`, `SourceReload=4.6`, and `SourceShot=0.36`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings; an earlier parallel runtime build failed only from an `obj\Debug\Assembly-CSharp.dll` file lock, then passed when rerun alone.
- Unity-MCP refresh reached idle; console warning/error read showed only MCP client handler logs, not C# compile errors.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `ariel-a` `projectile_speed=17`, `pierce_count=1`, `status_chance=0`, and `status_effect_label=없음`; the CSV `range` column was removed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` no longer has `ResolveProjectileSpeed(...)` or `ResolveBasePierceCount(...)` Ariel-A special cases.
- `ariel-b` `base_damage` in `monster_skills.csv` is now `35`, matching `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps Ariel design-only labels such as `방어막`, `축복`, and `신성 노출` with `status_chance=0`; if `ariel-a` is edited to `status_effect_label=감전`, `status_chance=1`, and `pierce_count=999`, the mapper can resolve the label to the supported `shock` status.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` parses Korean runtime labels including `감전`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` falls back from blank `status_effect_id` to parseable `status_effect_label`.
- `SyncCsvRuntimeCatalogs.bat` was added for Unity batchmode sync; when the project was already open, Unity batchmode rejected duplicate project open, then Unity-MCP invoked `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` and the console logged successful sync/validation.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-17: User asked Code Builder to implement Ariel-A using `Assets/Prefab/Skill/Ariel/Airel_A.prefab` and to report any information the blueprint alone could not provide.
- 2026-05-18: Code Builder moved Ariel-A projectile speed/pierce from mapper hardcoding into the skill CSV row and filled Ariel-B shield base from the reference document.
- 2026-05-18: Code Builder added status-label fallback and CSV runtime sync batch support so supported status edits in `monster_skills.csv` can be synced without code changes.

## Task: 2026-05-15 Ariel-B Phase4-C-0 Shield Effect Minimum Execution

### Task title

Connect Ariel-B to the first shared InGame attached effect actor path.

### Goals

- Add a reusable attached skill-effect actor that follows a target transform for a configured duration.
- Connect Ariel-B shield execution through the shared `ShieldSkillExecutor`.
- Use the user-authored `Assets/Prefab/Skill/Ariel/Ariel_B.prefab` as the current Ariel-B visual prefab.
- Keep shield resource mutation in `InGameCombatManager.GrantShield(...)`.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- This slice grants shield values and expires the visual actor only; timed shield resource expiry is not implemented here.
- `Assets/Prefab/Skill/Ariel/Airel_A.prefab` exists with the typo `Airel_A`, but `SkillData.csv` currently has no `ariel-a` row in the inspected minimum data set, so Ariel-A was not connected in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and compile/editor-refresh verified.

### Next Actions

- User verifies in Play Mode that Ariel-B shield visual appears on player units when Ariel-B is learned and cast.
- Add a timed shield resource-expiry system before declaring support-shield duration behavior complete.
- Add Ariel-A only after a matching skill data row and execution target are confirmed.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAttachedSkillEffectActor.cs`.
- `SkillExecutors.cs` now makes `ShieldSkillExecutor` call `GrantShield(...)` and instantiate a shield visual using `InGameAttachedSkillEffectActor`.
- `NewRunScene.unity` assigns `arielBShieldEffectPrefab` to `Assets/Prefab/Skill/Ariel/Ariel_B.prefab`.
- `Assets/Prefab/Skill/Ariel/Ariel_B.prefab` has `Pakuri.InGame.InGameAttachedSkillEffectActor`.
- `Pakuri/Assets/Legacy/Data/GameData/Monsters/ariel.asset` stores `ariel-b` `BaseDamage: 35`, matching the inspected `SkillData.csv` shield base value.
- Runtime and editor builds passed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed no C# compile errors.

### History

- 2026-05-15: User asked Code Builder to create the common projectile/effect actor component and connect Ariel-B minimum execution as the first Phase4-C subtask.

## Task: 2026-05-14 Ariel NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Ariel prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Ariel_Unit` through `NewRunSceneEntryManager`.
- Verify Ariel creates an exact `ariel` runtime model and initializes `MonsterUnitActor`.
- Make Ariel's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Ariel combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies Ariel selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Ariel_Unit.prefab` in `arielUnitPrefab`.
- Unity-MCP verification returned `ariel:prefab=Ariel_Unit|modelOk=True|model=ariel|actor=True|actorModel=True|hpText=HP 240/240|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Ariel_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.

## Task: 2026-05-14 Ariel CSVData Phase0-2 Seed Rows

### Task title

Record Ariel rows added to the new CSVData files.

### Goals

- Seed Ariel identity/stat data in `MonsterStat.csv` so the shield sample skill has an owner row.
- Seed Ariel-B Radiant Shield in `SkillData.csv`.
- Preserve the no-damage shield attribute distinction in CSV fields.

### Constraints

- Role Owner is Code Builder.
- No Ariel runtime behavior, prefab, scene, or Play Mode changes.
- `ariel-b` stores `skill_element` as Holy and `damage_element` as None because the inspected reference says the shield has no damage attribute.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData mapping should handle `damage_element=None` for non-damage support skills.
- Reconfirm Ariel base HP ownership before CSVData becomes the authoritative source because `ariel-tower.md` does not list HP.

### Evidence

- `Pakuri/Assets/CSVData/MonsterStat.csv` now contains the `ariel` row with current project stat values and source notes.
- `Pakuri/Assets/CSVData/SkillData.csv` now contains `ariel-b` as `ShieldSkillData`.
- `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md` provides shield 35, spell coefficient 1.4, duration 5.0, cooldown 9.0, all-allies targeting, and highest-value refresh.
- `Import-Csv Pakuri\Assets\CSVData\SkillData.csv` returned `ariel-b` with `damage_element` None and `shield_base` 35.

### History

- 2026-05-14: Code Builder added Ariel seed data as part of CSVData Phase0~2.

## Task: 2026-05-13 Ariel Battlefield Facade Registration

### Task title

Route Ariel battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Ariel skill behavior while replacing direct battlefield list registration writes.
- Keep Ariel projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel skills in Play Mode if needed.

### Evidence

- `CombatRuntimeArielSkills.cs:244` now calls `AddBattlefieldProjectile(...)`.
- `CombatRuntimeArielSkills.cs:335`, `:722`, and `:1036` now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Ariel battlefield object registration through facade methods.

## Task: 2026-05-10 Ariel Manifested Shield Expiry And Archangel Effect Fix

### Task title

Fix 2P-5P Ariel shield expiry on 1P and make Archangel Descent effect visible through the shared Ariel path.

### Goals

- Make shields granted to the selected 1P monster by Manifested Ariel B/E expire when their duration ends, even when the selected 1P monster is not Ariel.
- Make Ariel E `Archangel Descent` use an explicit battlefield-wide visual path for selected and Manifested Ariel casts.
- Explain the bug from inspected runtime code.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in RunScene Play Mode that Manifested Ariel shields on 1P disappear after their duration.
- User verifies selected and Manifested Ariel E show the battlefield-wide Archangel Descent effect.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:83` through `:88` decremented `unitShieldTimer` inside `UpdateArielSkillCooldowns()`, which only runs for the selected monster's Ariel runtime.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` now calls `UpdateSelectedUnitShieldTimer(Time.deltaTime)` from the common selected-unit combat update.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:86` now defines `UpdateSelectedUnitShieldTimer(...)`, clearing selected shield state and mirrored `selectedUnitRuntime` shield/Ariel fields when the timer expires.
- `CombatRuntimeArielSkills.cs:12` defines `ArielArchangelEffectDuration`; `:438` and `:693` call `CreateArielArchangelDescentEffect(skill)` for selected and unit-owned Ariel E casts.
- `CombatRuntimeArielSkills.cs:700` creates the battlefield-wide `ArchangelDescent` circle with stronger alpha/sorting and adds it to `skillEffects`.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:35` adds `ShieldAppliedFrame`; `:160` through `:163` skip manifested shield timer decay on the frame the shield was applied.
- Follow-up: `CombatRuntimeArielSkills.cs:28` adds `unitShieldAppliedFrame`; `:95` through `:98` skip selected 1P shield timer decay on the frame the shield was applied.
- Follow-up: `CombatRuntimeArielSkills.cs:831` and `:902` stamp selected and manifested shield application with `Time.frameCount`; `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:79` mirrors the selected shield frame into `selectedUnitRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\Skill\CombatRuntimeArielSkills.cs Pakuri\Assets\Scripts\Combat\Manager\CombatRuntimeProjectiles.cs` completed with only LF-to-CRLF warnings.
- Unity-MCP script refresh recovered to ready; console warning/error read returned only MCP client handler logs, not C# compile errors.
- Follow-up `git diff --check` over `CombatUnitRuntime.cs`, `CombatRuntimeArielSkills.cs`, and `CombatRuntimeParty.cs` completed with only LF-to-CRLF warnings; Unity-MCP console read returned only MCP client handler/timeout logs, not C# compile errors.

### History

- 2026-05-10: User reported Manifested 2P-5P Ariel shields remain on selected 1P after Ariel's shield duration ends, and Ariel E's effect is not visible.
- 2026-05-10: Code Builder moved selected-unit shield timer ticking out of selected-Ariel-only cooldown logic and routed Ariel E selected/unit casts through a dedicated battlefield visual helper.
- 2026-05-10: User reported 1P shield duration now appeared shorter than 2P-5P after Ariel shield casts; Builder aligned selected and manifested shield timers by skipping decay on the frame a shield is applied.

## Task: 2026-05-10 Ariel Unit Executor Migration And Team Shield

### Task title

Move Manifested Ariel A-E onto Ariel unit executor paths and make Ariel shield skills protect party units.

### Goals

- Dispatch Manifested Ariel skills through Ariel-specific `CombatUnitRuntime` logic before the generic manifested fallback.
- Keep Ariel A projectile damage, Holy Exposure, and White Judgement explosion source-aware for manifested Ariel.
- Make Ariel B `Radiant Shield` and Ariel E `Archangel Descent` apply shield state to selected 1P plus living manifested 2P-5P party units.
- Confirm the prior MainMenu-selected Ariel shield behavior against actual code and correct it.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds, Unity-MCP refresh, console check, and `git diff --check`.

### Next Actions

- User verifies selected Ariel B/E shields on 2P-5P teammates in RunScene Play Mode.
- User verifies Manifested Ariel A-E and Holy Exposure interactions in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before this change, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:516` used selected-only `unitShieldValue` in `ApplyArielUnitShield(...)`, `CombatRuntimeProjectiles.cs:455` applied manifested damage directly to HP, and `CombatRuntimeParty.cs:2034` passed `0f` as manifested shield value.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:42` now stores per-unit shield and Ariel blessing/sanctuary/Archangel shield state.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:637` dispatches `TryTickArielUnitSkill(...)` before generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:422` through `:681` implements Ariel unit A-E execution paths.
- `CombatRuntimeArielSkills.cs:808` applies Ariel team shields to selected plus manifested units; `:1300` handles Ariel unit projectile hits.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:464` through `:473` applies shield absorption to manifested unit damage before HP loss.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP script refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: User requested the Ariel unit executor migration from the remaining-work report and asked whether MainMenu-selected Ariel shield skills protect teammates.
- 2026-05-10: Code inspection confirmed selected Ariel shields did not protect manifested teammates before this pass; Builder added party shield state and Ariel unit executor dispatch.
