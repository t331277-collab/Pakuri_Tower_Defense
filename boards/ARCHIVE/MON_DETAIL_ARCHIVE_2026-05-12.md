# MON Detail Archive 2026-05-12

Task blocks moved out of `boards/MON/*.md` while applying the active-board compaction rule to monster detail files.

The active files keep only their latest dated task blocks. Older or undated blocks are preserved below by source file.

# Source: boards/MON/ARIEL_MONSTER.md

## Task: 2026-05-08 Manifested Ariel Common Runtime Parity

### Task title

Apply Ariel Offering choices through the manifested common skill runtime.

### Goals

- Keep manifested Ariel skills sourced from `SkillDefinition` data.
- Apply Ariel manifested Offering choices in shared damage, cooldown, magazine, reload, and shield/buff-safe paths.
- Avoid treating Ariel shield/buff runtime kinds as damaging attacks.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Ariel private timers.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Ariel skills and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:503` prevents manifested buff/shield skills from applying enemy damage.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Ariel skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Ariel cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1250` and `:1278` include Ariel A magazine/reload choice handling.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

## Task: 2026-05-05 Ariel Skill Data RuntimeKind Audit

### Task title

Align Ariel active skill runtime kinds with non-magazine runtime behavior.

### Goals

- Keep Ariel A as the only Ariel magazine projectile skill.
- Classify Ariel B-E according to their implemented behavior so MonsterPanel and future data consumers do not treat them as magazine skills.
- Keep Ariel A-E/F-J implementation-state metadata aligned with existing runtime implementation.

### Constraints

- Role Owner is Code Builder.
- Data-only correction; no Play Mode verification was run by Codex.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel Active1-3 MonsterPanel display in Play Mode after learning B-E.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md` states Radiant Shield is `鍮꾪깂李?/ ?뚰떚 蹂댄샇 / 諛⑹뼱留?.
- `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md` states Blessing Wave is `鍮꾪깂李?/ ?뚰떚 媛뺥솕 / 踰붿쐞 ?쇳빐`.
- `Pakuri/reference/2.Monster/ariel/skill/d-celestial-brand.md` states Celestial Brand is `鍮꾪깂李?/ ?⑥씪 ?쒖떇 / ?좎꽦 ?몄텧`.
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md` states Archangel Descent is `鍮꾪깂李?/ ?꾩옣 愿묒뿭 / ?뚰떚 蹂댄샇 寃곗쟾湲?.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` contains `TryCastArielRadiantShield`, `TryCastArielBlessingWave`, `TryCastArielCelestialBrand`, and `TryCastArielArchangelDescent` with per-skill cooldown fields.
- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` now stores Ariel A `MagazineProjectile`, B `Shield`, C `AreaAttack`, D `Mark`, E `AreaAttack`, and F-J `Passive`, all `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores the same Ariel runtime kinds and `RuntimeImplemented` states.
- Unity-MCP read-only Editor import reported `ariel-a:MagazineProjectile:RuntimeImplemented`, `ariel-b:Shield:RuntimeImplemented`, `ariel-c:AreaAttack:RuntimeImplemented`, `ariel-d:Mark:RuntimeImplemented`, `ariel-e:AreaAttack:RuntimeImplemented`, and Ariel F-J passives as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-05: After verifying the Rin MonsterPanel fix, user requested auditing Eve and Ariel skill data so they apply correctly too.
- 2026-05-05: Builder corrected Ariel B-E away from `MagazineProjectile` and aligned the data with the implemented non-magazine cooldown skills.

## Task: 2026-05-03 Ariel J Passive Runtime Correction

### Task title

Correct Ariel passive J `Sanctuary Proclamation` runtime to match the current E/J reference documents.

### Source references

- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md`
- `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md`

### Skill slots A-J

- Legacy non-English note retained these code references: `ariel-f`.
- Legacy non-English note retained these code references: `ariel-g`.
- Legacy non-English note retained these code references: `ariel-h`.
- Legacy non-English note retained these code references: `ariel-i`.
- J `ariel-j` / Sanctuary Proclamation: corrected so post-E action speed uses its own 5-second timer and holy-damage bonus depends on remaining Archangel shield state.

### Runtime implementation status

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` now separates three Ariel timed states:
  - `arielBlessingTimer` for blessing-related windows.
  - `arielSanctuaryTimer` for E master 1 damage reduction.
  - `arielSanctuaryProclamationTimer` for J post-E action speed.
- The runtime now also tracks the remaining Archangel shield share through `arielArchangelShieldValue` and `arielArchangelShieldTimer`.
- The Archangel shield tracking follow-up now only marks E shield state when the new E shield actually becomes the pooled active shield, and clears that state when a stronger non-E Ariel shield replaces it.
- Ariel E now spawns a battlefield circle effect, and Ariel support-skill retries are now gated to real firing windows so C does not keep retrying every held-input frame while A cannot fire.

### Data asset status

- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` already contained J passive/trait definitions and did not need asset edits in this correction pass.

### DebugScene test status

Codex did not run Unity Play Mode. User Play Mode verification is still required.

### Code Reviewer status

2026-05-03 Code Reviewer result: NEEDS_CHANGES. Builder follow-up has now been applied, but Code Reviewer was not rerun because the user did not request another review.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md:18-19` requires `Archangel Descent` post-cast action speed for 5 seconds and holy-damage bonus while the E shield remains.
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md:22-24` defines the E shield amount/duration that J should follow and establishes E as the battlefield-wide cast this runtime visual should represent.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs:22-24` adds dedicated J timer / Archangel shield fields.
- `CombatRuntimeArielSkills.cs:136-143` now decrements and clears those dedicated states independently of the blessing timer.
- `CombatRuntimeArielSkills.cs:429-451` now starts J proclamation timing from E cast, marks Archangel shield ownership through the shared shield helper, and spawns the missing `ArchangelDescent` battlefield effect.
- `CombatRuntimeArielSkills.cs:554-580` now keeps Archangel ownership tied to the actual pooled shield owner instead of blindly writing the full E shield amount into tracking state.
- `CombatRuntimeArielSkills.cs:592-600` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:315-319` now reduce the tracked Archangel shield share when incoming damage is absorbed by the selected Monster shield.
- `CombatRuntimeArielSkills.cs:771`, `852`, and `898-900` now use the dedicated E-shield/J-state checks instead of the generic shield/blessing path.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:332-356` now limits Ariel support-skill retry checks to frames where Ariel A can actually fire, closing the held-input retry path behind the reported occasional C barrage.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP warnings.
- Unity script refresh finished with `resulting_state: idle`; Unity console error query returned only MCP-FOR-UNITY handler exit logs.
- External Code Reviewer executed once through the installed Codex CLI path `C:\Users\t3312\.vscode\extensions\openai.chatgpt-26.429.30905-win32-x64\bin\windows-x86_64\codex.exe` and returned one actionable finding.
- Reviewer finding: `CombatRuntimeArielSkills.cs:429-431` recorded the full E shield into `arielArchangelShieldValue` even when `ApplyArielUnitShield(...)` kept a larger pre-existing non-E shield; the Builder follow-up moved that ownership decision into `ApplyArielUnitShield(..., true)` and the non-E replacement path.

### History

- 2026-05-03: User requested implementing Ariel passive skills F-J from the reference folder.
- 2026-05-03: Code Builder confirmed the existing F-I wiring and corrected the incomplete J timer/shield-state implementation.
- 2026-05-03: User explicitly requested Code Reviewer execution; Reviewer returned NEEDS_CHANGES for remaining J shield-source tracking leakage.
- 2026-05-03: User requested fixing that reviewer finding and also reported missing Ariel E effect plus occasional Ariel C barrage behavior; Code Builder applied the follow-up and revalidated with build and Unity refresh evidence.

## Task: Ariel A-E Active And F-J Enhancement Runtime

### Task title

Implement Ariel skill documents A-E and their F-J enhancement/passive effects.

### Source references

- `Pakuri/reference/2.Monster/ariel/ariel-tower.md`
- `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`
- `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md`
- `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md`
- `Pakuri/reference/2.Monster/ariel/skill/d-celestial-brand.md`
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md`
- `Pakuri/reference/2.Monster/ariel/skill/f-guiding-light.md`
- `Pakuri/reference/2.Monster/ariel/skill/g-guardian-doctrine.md`
- `Pakuri/reference/2.Monster/ariel/skill/h-spread-blessing.md`
- `Pakuri/reference/2.Monster/ariel/skill/i-brand-revelation.md`
- `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md`

### Skill slots A-J

- Legacy non-English note retained these code references: `ariel-a`.
- Legacy non-English note retained these code references: `ariel-b`.
- Legacy non-English note retained these code references: `ariel-c`.
- Legacy non-English note retained these code references: `ariel-d`.
- E `ariel-e` / Archangel Descent: battlefield-wide Holy damage, selected-unit shield, Holy Exposure target bonus, sanctuary damage reduction, post-cast blessing.
- Legacy non-English note retained these code references: `ariel-f`.
- Legacy non-English note retained these code references: `ariel-g`.
- Legacy non-English note retained these code references: `ariel-h`.
- Legacy non-English note retained these code references: `ariel-i`.
- J `ariel-j` / Sanctuary Proclamation: E post-cast action speed, shielded Holy damage, E cooldown trait.

### Runtime implementation status

Implemented in `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` and integrated into:

- `CombatRuntimeController.cs`
- `CombatRuntimeProjectiles.cs`
- `CombatRuntimeEnemies.cs`
- `Pakuri/Assembly-CSharp.csproj`

Current runtime has one selected player Monster, not an ally party collection. Document phrases such as "all allies" are implemented against the selected Monster because that is the only allied combat unit present in the current code.

### Data asset status

- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` now marks Ariel A-E and F-J `ImplementationState: 2`.
- `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs` now marks Eve and Ariel A-E/F-J as runtime implemented when seeding from skill documents.

### DebugScene test status

Codex did not run Unity Play Mode. User Play Mode verification is still required.

### Code Reviewer status

2026-04-30 Code Reviewer result: FAIL. Code Builder applied fixes for the reported findings; a follow-up Code Reviewer run has not been executed yet.

### Evidence

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing 2 Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed; `mcpforunity://editor/state` returned `ready_for_tools=true`.
- Unity console error query returned MCP-FOR-UNITY handler logs only, not Ariel project compile errors.
- `git diff --check` for Ariel changed files returned exit code 0 with CRLF warnings only.
- External Reviewer execution returned `REVIEW_RESULT: FAIL`.
- Reviewer evidence: `CombatRuntimeArielSkills.cs:234` uses monster top-level runtime damage fields for Ariel A, while `ariel.asset:96-104` and `a-judgement-light.md:19-34` define different A skill values.
- Reviewer evidence: `CombatRuntimeArielSkills.cs:193` explodes White Judgement at `currentAttackPoint`; `a-judgement-light.md:52` describes the last projectile exploding.
- Reviewer evidence: `CombatRuntimeProjectiles.cs:310` passes absorbed shield damage without attacker context, and `CombatRuntimeArielSkills.cs:539-542` reflects to nearest enemy; `b-radiant-shield.md:48` says reflect to the attacker.
- Reviewer evidence: Holy damage multipliers are pre-applied in Ariel cast paths and then applied again through final damage calculation.
- Builder fix evidence: `CombatRuntimeArielSkills.cs:201-240` now creates Ariel A projectiles from `ariel-a` skill damage/range, uses projectile speed `17`, and stores last-shot explosion data on the projectile.
- Builder fix evidence: `CombatRuntimeProjectiles.cs:89` and `CombatRuntimeProjectiles.cs:102` now trigger Ariel A master explosion at projectile cleanup position, not immediately at click point.
- Builder fix evidence: `CombatRuntimeProjectiles.cs:141`, `CombatRuntimeProjectiles.cs:299-312`, `CombatRuntimeEnemies.cs:963`, and `CombatRuntimeArielSkills.cs:533-544` now pass the source enemy into selected-Monster damage and reflect Radiant Shield damage back to that attacker.
- Builder fix evidence: `CombatRuntimeArielSkills.cs:164`, `303`, `333`, and `380` no longer pre-apply the shared Holy damage multiplier before final damage calculation.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User requested reading Ariel Monster skill markdown files under `Pakuri/reference/2.Monster/ariel` and implementing skills A-E plus enhancement effects.
- 2026-04-30: Code Builder read the Ariel tower and A-J skill documents, confirmed the existing Ariel asset and combat runtime structure, implemented Ariel runtime effects, updated data implementation states, and completed build/Unity validation.
- 2026-04-30: Code Reviewer reviewed the Ariel runtime implementation and returned FAIL with behavior mismatches that require Builder fixes.
- 2026-04-30: User instructed Builder to fix the Reviewer findings; Builder applied Ariel A data/last-shot explosion, Radiant Shield attacker reflection, and Holy multiplier duplication fixes, then rebuilt and checked Unity console.

## Task: Ariel A Lifetime And Shield Bar Split Visual

### Task title

Fix Ariel A early cleanup and display HP/shield as one fixed-width split bar.

### Goals

- Prevent Ariel A projectiles from disappearing too soon after firing.
- Keep actual HP and shield values unchanged.
- Display shielded Monster HP as a single fixed-width bar split between red HP and white shield.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel A projectile lifetime and shield bar ratio visuals in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` now computes Ariel A projectile lifetime as the maximum of the configured projectile lifetime and `range / ArielJudgementProjectileSpeed`, preventing the previous `8.5 / 17 = 0.5` second cleanup path from dominating.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now has `UpdateHpShieldBarFill()` and `UpdateBarSegment()` to draw red HP and white shield as adjacent segments inside the same bar width when shield is present.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now uses the shared HP/shield split visual for the selected Monster bar.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing 2 Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity editor state showed Play Mode active, so Codex did not force script refresh or Play Mode changes. Console error query returned MCP-FOR-UNITY handler logs only, not project compile errors.

### History

- 2026-04-30: User reported Ariel A projectiles disappear soon after firing and requested League-style HP/shield bar ratio visuals where actual HP and shield values remain unchanged.
- 2026-04-30: Code Builder fixed Ariel A lifetime calculation and changed the shared selected-Monster shield bar visual to a fixed-width HP/shield split.

## Task: Ariel White Judgement Hit Explosion Visual

### Task title

Make Ariel A master `White Judgement` explode on hit and use the base circle sprite visual.

### Goals

- Ensure `ariel-a-master-1` visibly and mechanically triggers when the marked last projectile hits.
- Keep fallback explosion on projectile lifetime expiry if the last projectile hits nothing.
- Use the existing generated circle sprite for the explosion visual.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies `White Judgement` in Play Mode with `ariel-a-master-1` selected.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md` defines `White Judgement` as the last projectile exploding twice with area Holy damage.
- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` contains `MasterSkillChoices` entry `ChoiceId: ariel-a-master-1`, Title `White Judgement`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` keeps the explosion damage/count on the projectile and now returns whether `TryTriggerArielJudgementLightExplosion()` fired.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now triggers the Ariel judgement explosion immediately on enemy hit when the projectile has pending explosion data, then cleans up the projectile.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` uses `CreateCircleEffect()` / `GetCircleSprite()` for the explosion visual, with longer duration and higher sorting order.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the existing Unity/MCP warnings.
- Unity refresh/compile completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User reported Ariel A master `White Judgement` did not appear to apply and requested using the base circle asset if only the visual effect was missing.
- 2026-04-30: Code Builder changed the master explosion to fire immediately on hit and made the circle-sprite explosion more visible.

# Source: boards/MON/EVE_MONSTER.md

## Task: 2026-05-08 Manifested Eve A Auto-Target Runtime

### Task title

Move manifested Eve A onto Eve Arc Bolt-specific unit runtime execution.

### Goals

- Use the original Eve A projectile/enhancement logic for manifested Eve.
- Add only automatic target/direction selection for the manifested unit.
- Keep Eve A magazine, reload, projectile count, pierce, branch, status, and damage choices sourced from the manifested Eve `RunMonsterState`.

### Constraints

- Role Owner is Code Builder.
- Selected Eve manual fire is not Play Mode verified by Codex.
- User performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Eve A auto-fire, branching, pierce, magazine, reload, and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs:192` routes Eve unit A runtime to `TryFireEveUnitArcBolt(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs:650` computes target direction from the manifested unit position to the nearest enemy and applies Eve A choices.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs:780` creates manifested Arc Bolt projectiles with lightning attribute, status, pierce, and branch fields.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:661` applies manifested projectile branch logic on hit.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked why manifested Eve A could not simply use original Eve A with auto aim and then requested that path to be implemented.

## Task: 2026-05-08 Eve Manifest Candidate Availability

### Task title

Allow selected Eve to be added as a manifested Eve party member.

### Goals

- Fix the case where Eve does not appear as a Manifest candidate when Eve is also the MainMenu-selected unit.
- Allow Manifest selection to add Eve to the manifested party list.

### Constraints

- Role Owner is Code Builder.
- This pass changes RunSession manifest duplicate logic, not Eve skill runtime behavior.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies Eve appears in the Manifest candidate panel and is added after selection.
- Follow-up may still be needed if selected Eve and manifested Eve must have independent Offering state while sharing the same `MonsterId`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` gets Manifest candidates from monster data and excludes ids only through `currentSession.HasManifestedMonster(monster.MonsterId)`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` now makes `HasManifestedMonster(...)` return true only when `monsterId` is in `ManifestedMonsterIds`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` keeps `RecordManifestedMonster(...)` adding `monster.MonsterId` to `ManifestedMonsterIds`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings; Unity refresh returned idle and console error query returned only MCP client handler exit logs.

### History

- 2026-05-08: User reported Eve did not show in Manifest candidates and selecting Eve did not add Eve.

## Task: 2026-05-08 Eve B-E Shared Unit Runtime

### Task title

Move Eve automatic support skills onto a shared caster-based unit runtime path.

### Goals

- Make selected EveUnit and manifested Eve use the same caster-based execution functions for Eve B-E.
- Read skill source data from `CombatSkillRuntime.Skill`.
- Read Offering choices from the caster's `RunMonsterState.ChosenRewardIds`.

### Constraints

- Role Owner is Code Builder.
- Eve A manual primary fire still needs a separate follow-up to move fully out of selected-primary globals.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies selected Eve and manifested Eve Prism Ray, Frost Field, Static Override, and Drone Beacon in Play Mode.
- Follow-up migrates Arc Bolt manual projectile runtime into the same caster path.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now has `TryTriggerEveUnitAutomaticSkills(...)`, `TryTickEveUnitSkill(...)`, `TryCastEveUnitPrismRay(...)`, `TryCastEveUnitFrostField(...)`, `TryCastEveUnitStaticOverride(...)`, and `TryCastEveUnitDroneBeacon(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` selected Eve automatic triggering now calls `TryTriggerEveUnitAutomaticSkills(selectedUnitRuntime)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` calls `TryTickEveUnitSkill(...)` for manifested Eve units before the older generic manifested path.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` reads selected Eve cooldown display from selected Eve `CombatSkillRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested unit-owned skill behavior rather than copying the visual EveUnit object.

## Task: 2026-05-08 Manifested Eve Frost Field Parity

### Task title

Make manifested Eve C follow selected Eve Frost Field tick and status behavior.

### Goals

- Ensure manifested Eve Frost Field is not a one-shot area hit.
- Apply repeated ice damage, chill stacks, and freeze duration from Eve C traits while using the manifested Eve unit's Offering state.
- Keep manifested Eve damage resolution separate from selected-Eve-only passive checks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in RunScene Play Mode that manifested Eve C applies repeated damage and chill/freeze effects after Offering acquisition.
- Consider follow-up extraction of Eve A/B/D/E selected-skill code into unit-owned executors if exact manifested parity is required for all Eve skills.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Selected Eve C in `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` uses `CreateCircleEffect(...)`, `skillEffects.Add(effect)`, `TickSkillEffect(...)`, and applies `ApplyChill(...)` for `SkillId == "eve-c"`.
- Before this pass, `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` handled manifested `SkillRuntimeKind.Field` by applying `ApplyManifestedSkillDamage(...)` once in the radius and then creating only a visual.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now creates a `ManifestedFrostField` persistent effect with Eve C trait modifiers from `runtime.State.ChosenRewardIds`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now routes manifested persistent effects to `ApplyManifestedSkillEffectDamage(...)`, which applies ice damage plus `ApplyChill(...)` and freeze duration for `eve-c`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User provided the repro: selected Eve Frost Field applies ongoing freeze/chill damage, but manifested Eve Frost Field only deals the first hit.

## Task: 2026-05-05 Eve Skill Data RuntimeKind Audit

### Task title

Align Eve skill data with implemented runtime behavior for MonsterPanel and runtime selection.

### Goals

- Keep Eve active skill `RuntimeKind` values consistent with actual combat code and reference skill documents.
- Mark Eve A-E and F-J as runtime implemented in both ScriptableObject and CSV source data.
- Preserve Eve E Drone Beacon as the only non-A Eve active with magazine-style charges/reload display.

### Constraints

- Role Owner is Code Builder.
- Data-only correction; no Play Mode verification was run by Codex.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve Active1-3 MonsterPanel display in Play Mode after learning B-E.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/d-static-override.md` states Static Override is `踰붿쐞 / 鍮꾪깂李?/ 媛먯쟾 ?곌퀎`.
- `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` states Drone Beacon is `?꾩갹 / ?쒕줎 / ?쒖떇 / ?붾쾭?? with magazine count 3 and reload 6 seconds.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` contains `TryCastEvePrismRay`, `TryCastEveFrostField`, `TryCastEveStaticOverride`, and `TryCastEveDroneBeacon`; Drone Beacon uses `eveDroneChargesRemaining` and `eveDroneReloadRemaining`.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` now stores Eve A `MagazineProjectile`, B `LineAttack`, C `Field`, D `AreaAttack`, E `MagazineProjectile`, and F-J `Passive`, all `ImplementationState: 2`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores the same Eve runtime kinds and `RuntimeImplemented` states.
- Unity-MCP read-only Editor import reported `eve-a:MagazineProjectile:RuntimeImplemented`, `eve-b:LineAttack:RuntimeImplemented`, `eve-c:Field:RuntimeImplemented`, `eve-d:AreaAttack:RuntimeImplemented`, `eve-e:MagazineProjectile:RuntimeImplemented`, and Eve F-J passives as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-05: After verifying the Rin MonsterPanel fix, user requested auditing Eve and Ariel skill data so they apply correctly too.
- 2026-05-05: Builder corrected Eve Static Override away from `MagazineProjectile`, kept Drone Beacon as a magazine-charge skill, and aligned implementation-state metadata.

## Task: Eve Arc Branch And DebugScene Skill Toggle Runtime

### Task title

Narrow Eve Arc Bolt extra projectile spread, implement immediate lightning branch damage, and add DebugScene skill-toggle testing UI.

### Goals

- Reduce the extra projectile spread angle for Eve A Arc Bolt.
- Change Eve A lightning branch semantics from status chance to immediate branch damage on hit.
- Draw a thin straight rectangular lightning line from the hit enemy to each branch target.
- Add a DebugScene-only controller under `Assets/Scenes/DebugScene.unity` that can test the 5 monster assets and toggle skills A-J plus enhancement/master effects.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Do not run external Code Reviewer unless the user explicitly grants permission.
- The current data model has `SkillSlot` A-J only; there is no K enum value. DebugScene shows K as a disabled no-data toggle rather than inventing runtime data.
- Preserve unrelated existing worktree changes from previous Eve runtime and report tasks.

### Role Owner

Code Builder

### Status

Builder correction pass completed for the prior `eve-a-master-1` findings, DebugScene UI flow was reworked to match the newer user request, and a follow-up SkillDebugPanel visibility fix was applied. A later Builder pass changed `DebugSceneController` toward scene-bound editable UI and saved static skill/choice toggle slots into `DebugScene.unity`. User then instructed Builder to restore `DebugSetupPanel` and setup controls; those scene paths were restored and build/console validation passed. The later root-scale finding was fixed by serializing the `DebugSceneController` root `RectTransform` scale as `{1,1,1}` and by guarding only the zero-scale case in `EnsureCanvasShell()`, while external Code Reviewer execution is deferred until user permission.

### Next Actions

- User Play Mode verifies `DebugScene` because Codex does not run Unity-MCP Play Mode gameplay verification.
- Run external Code Reviewer only after explicit user permission.

### Evidence

- `Pakuri/Assets/Scripts/Data/SkillDefinition.cs` defines `SkillSlot` A through J only; no K slot exists.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` has `eve-a-trait-5` with branch chance text and `eve-a-master-1` with branch circuit text.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now uses `EveArcExtraProjectileAngleStep = 3f` instead of the previous 4-degree spread step.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now applies branch damage immediately after an Eve projectile hit, selects nearby targets within branch radius, and creates line visuals through `CreateEveArcBranchLine`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` was added to create a DebugScene-only uGUI panel with 5 monster buttons, skill toggles, enhancement/master toggles, and immediate `RunSession` restart into `CombatRuntimeController.BeginConfiguredDay(...)`.
- `Pakuri/Assets/Scenes/DebugScene.unity` now contains a `DebugSceneController` object wired to `Assets/Data/GameData/GameDataCatalog.asset` and `CombatRoot`.
- In `Pakuri/Assets/Scenes/DebugScene.unity`, the duplicated `RunSceneBootstrap` and `RunCombatUiController` components are serialized disabled to keep DebugScene separate from the RunScene flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `eve-a-master-1` branch damage is implemented as 100% in `CombatRuntimeEveSkills.cs`, but `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` says branch damage is 60%.
- Reviewer finding 2: `eve-a-master-1` does not add the documented magazine +2; `GetEveArcMagazineCapacity()` currently only applies `eve-a-trait-1` +4.
- User approved the Reviewer findings. `CombatRuntimeEveSkills.cs` now sets `eve-a-master-1` branch damage multiplier to `0.60f`, and `GetEveArcMagazineCapacity()` now adds `+2` for `eve-a-master-1`.
- User clarified DebugScene flow: starting DebugScene must not spawn enemies; user selects monster, opens skill debug, toggles A-J, chooses enhancement effects in a separate closable UI, then presses Start to spawn enemies.
- `CombatRuntimeController.cs` now exposes `ApplyDebugSelection(...)` so DebugScene can update selected monster/skill state without calling `BeginPrototypeDay(...)` or spawning enemies.
- `DebugSceneController.cs` now keeps monster selection and skill/enhancement configuration separate from `StartCombat()`, uses `BeginConfiguredDay(...)` only when Start is pressed, and uses `ApplyDebugSelection(...)` for pre-start or mid-combat debug changes.
- `DebugSceneController.cs` now disables passive F unless active A is checked; unchecking A also clears F.
- `DebugSceneController.cs` now opens an enhancement modal when a skill/passive is checked, and the modal has a close button.
- `DebugSceneController.cs` ignores prisoner reward UI entirely; after combat resolves, the existing `CombatRuntimeController` stays resolved until the DebugScene Start button is pressed again.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY transport/client handler logs, not project compile errors.
- User reported the SkillDebugPanel skill list was not visible.
- `DebugSceneController.OpenSkillWindow()` now activates `SkillDebugPanel` before rebuilding its toggles.
- `DebugSceneController.RebuildSkillToggles()` and `OpenEnhancementModal()` now call `RefreshToggleContentHeight(...)` after creating toggles so ScrollRect content has a concrete height.
- `DebugSceneController.EnsureToggle(...)` now assigns fixed toggle anchors/pivot plus `LayoutElement` min/preferred height, and `EnsureScrollContent(...)` assigns fixed scroll viewport `LayoutElement` height.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested after the SkillDebugPanel fix; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer command was attempted for the latest DebugScene changes, but Codex CLI exited with usage-limit errors and did not return a review verdict.
- User reported the SkillDebugPanel skill list still did not appear and requested editable scene UI instead of UI generated during game execution.
- `DebugSceneController.cs` was changed so `Awake()` calls `BindSceneUi()` instead of `BuildUi()`, and the controller now binds buttons/toggles from scene object paths instead of creating panels/toggles at runtime.
- Unity Edit Mode code saved static toggle objects in `Pakuri/Assets/Scenes/DebugScene.unity`: `Active_A` through `Active_E`, `Passive_F` through `Passive_J`, and `Choice_01` through `Choice_08`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor state `ready_for_tools=true`; Unity console error query showed only MCP-FOR-UNITY client handler logs, not project compile errors.
- Required external Code Reviewer returned `REVIEW_RESULT: FAIL`.
- Reviewer finding 1: `DebugSceneController.cs` line 156 requires direct child `DebugSetupPanel`, but `DebugScene.unity` line 8213 shows only `SkillDebugPanel` and `EnhancementModal` under `DebugSceneController`; `Select-String` found no `m_Name: DebugSetupPanel`.
- Reviewer finding 2: `DebugSceneController.cs` line 166 expects `Title`, `Status`, `CombatText`, `MonsterButtons`, `SkillWindowButton`, and `StartButton` under `DebugSetupPanel`, but scene search found those setup paths absent.
- User instructed Code Builder to restore `DebugSetupPanel` and setup controls into the scene and re-run build validation.
- Unity Edit Mode code restored and saved scene objects for `DebugSetupPanel`, `DebugSetupPanel/Title`, `DebugSetupPanel/Status`, `DebugSetupPanel/MonsterButtons`, `DebugSetupPanel/SkillWindowButton`, `DebugSetupPanel/StartButton`, and `DebugSetupPanel/CombatText`.
- The same scene save pass also ensured `SkillDebugPanel/SkillScroll/Viewport/Content`, `EnhancementModal/ChoiceScroll/Viewport/Content`, A-J skill toggles, and `Choice_01` through `Choice_08` exist as editable scene objects.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the previous `DebugSceneController requires DebugSetupPanel...` project error.
- Required external Code Reviewer verified the requested scene paths now exist, but returned `REVIEW_RESULT: FAIL`.
- Latest Reviewer finding: `DebugScene.unity` line 10104 stores `DebugSceneController` root `RectTransform` with `m_LocalScale: {x: 0, y: 0, z: 0}`; since child UI is parented under this root, the UI can remain visually collapsed/non-interactive.
- User later instructed that Code Reviewer must be run only with user permission.
- `Pakuri/Assets/Scenes/DebugScene.unity` now stores the `DebugSceneController` root `RectTransform` with `m_LocalScale: {x: 1, y: 1, z: 1}`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` now restores `transform.localScale` only when it is exactly `Vector3.zero`, preserving non-zero user-edited UI scale and position.
- `Select-String` confirmed `DebugSetupPanel`, `SkillDebugPanel`, `EnhancementModal`, `Active_A` through `Active_E`, `Passive_F` through `Passive_J`, and `Choice_01` / `Choice_08` are present in `Pakuri/Assets/Scenes/DebugScene.unity`.
- Unity read-only `execute_code` confirmed `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` has children `Active_A,Active_B,Active_C,Active_D,Active_E,Passive_F,Passive_G,Passive_H,Passive_I,Passive_J`.
- Unity `mcpforunity://scene/gameobject/65632` showed the loaded `DebugSceneController` transform scale and lossyScale are `{1,1,1}`.
- User reported that `Content` child skill toggles were clickable but their descriptions and checkmark were invisible.
- `DebugSceneController.ConfigureToggle(...)` now calls `ConfigureToggleVisuals(...)` to rebuild each scene-bound toggle slot's `Background`, `Checkmark/Glyph`, and `Label` visuals every time the slot is bound.
- `DebugSceneController.ConfigureToggleVisuals(...)` uses a separate `Checkmark/Glyph` child `Text` as the Toggle graphic, because the existing `Checkmark` object already has an `Image` graphic and Unity did not add a second `Text` graphic to the same GameObject in the runtime inspection.
- Legacy non-English note retained these ASCII code references: `execute_code`, `Active_A`, `toggle.graphic=Text:Checkmark/Glyph`, `labelAlpha=1`.
- Runtime Unity missing-script inspection returned `missingTotal=0`; the visible console still contained older `The referenced script (Unknown) on this Behaviour is missing!` entries with no file/line.
- User reported the Label skill text and checkbox were still not visible. Builder replaced the Text-glyph checkmark approach with Unity built-in `UISprite` and `Checkmark` sprites in `DebugSceneController.ConfigureToggleVisuals(...)`.
- Legacy non-English note retained these ASCII code references: `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content`, `Active_A`, `Passive_J`, `DebugScene.unity`, `labelAlpha=1`, `bgSprite=UISprite`, `checkSprite=Checkmark`, `toggleGraphic=Checkmark`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor state `ready_for_tools=true`; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the previous `DebugSceneController requires DebugSetupPanel...` project error.
- User reported `Failed to find UI/Skin/UISprite.psd` from `DebugSceneController.ConfigureToggleVisuals(...)`.
- `Select-String` confirmed the old `UI/Skin` and `GetBuiltinResource<Sprite>` calls were removed from `Pakuri/Assets/Scripts/Run/DebugSceneController.cs`; the only sprite load is now `Resources.Load<Sprite>("DebugUiSolid")`.
- `Pakuri/Assets/Resources/DebugUiSolid.png` was created as a project-owned 1x1 Sprite resource, avoiding Unity built-in UI skin paths.
- Unity Edit Mode scene save updated the actual `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` slots so `Active_A` through `Passive_J` remain editable scene objects and their `Background` / `Background/Checkmark` images use `DebugUiSolid`.
- Legacy non-English note retained these ASCII code references: `execute_code`, `resourceSprite=DebugUiSolid`, `contentCount=10`, `labelAlpha=1`, `bgSprite=DebugUiSolid`, `checkSprite=DebugUiSolid`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Unity refresh/compile completed with `resulting_state=idle`; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the `Failed to find UI/Skin/UISprite.psd` project error.
- User requested the same visible/editable rebuild for `EnhancementModal` children.
- Unity read-only `execute_code` first confirmed `EnhancementModal/ChoiceScroll/Viewport/Content` had 8 choices but `Choice_01` had `bgSprite=null` and `checkSprite=null`.
- Unity Edit Mode code deleted all existing children under `DebugSceneController/EnhancementModal`, recreated `Title`, `Summary`, `CloseButton`, `ChoiceScroll/Viewport/Content`, and `Choice_01` through `Choice_08`, and saved `Assets/Scenes/DebugScene.unity`.
- Unity read-only `execute_code` then confirmed `modalActive=False`, `title=Enhancements`, `closeButton=True`, `count=8`, `choice01Label=Choice Slot 01`, `labelAlpha=1`, `bgSprite=DebugUiSolid`, and `checkSprite=DebugUiSolid`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings after the scene rebuild.
- Unity refresh completed with `resulting_state=idle`; Unity console error query showed only MCP-FOR-UNITY client handler logs.

### History

- 2026-04-29: User requested narrower extra projectile angle, immediate lightning branch damage/visuals, and a separate DebugScene for testing monster skills and enhancements through toggles.
- 2026-04-29: Code Builder inspected actual combat scripts, scene files, monster assets, and the SkillSlot enum before editing.
- 2026-04-29: Code Builder implemented branch projectile fields, immediate branch damage, line visuals, DebugScene controller script, and DebugScene scene wiring.
- 2026-04-29: External Code Reviewer found two `eve-a-master-1` spec mismatches and returned `NEEDS_CHANGES`; Builder paused per AGENTS.md.
- 2026-04-30: User approved the prior Reviewer findings and then clarified the desired DebugScene UI flow.
- 2026-04-30: Code Builder fixed `eve-a-master-1` branch damage and magazine size, added `CombatRuntimeController.ApplyDebugSelection(...)`, and rewrote `DebugSceneController` so enemies spawn only from the DebugScene Start button.
- 2026-04-30: User reported the SkillDebugPanel skill list was not visible; Code Builder updated the panel activation order and ScrollRect/LayoutElement sizing, then rebuilt and checked Unity console.
- 2026-04-30: Code Builder attempted the required external Code Reviewer pass, but Codex CLI reported a usage limit and no verdict was produced.
- 2026-04-30: User reported the SkillDebugPanel issue persisted and requested editable scene UI rather than game-run generated UI. Code Builder changed the controller toward scene-bound UI and saved static toggle slots, but the required external Code Reviewer found missing `DebugSetupPanel` setup controls and returned `FAIL`; Builder paused per AGENTS.md.
- 2026-04-30: User instructed Builder to restore `DebugSetupPanel` and setup controls. Builder restored those scene objects and validated build/console, then external Reviewer found the root scale `{0,0,0}` scene issue and returned `FAIL`; Builder paused per AGENTS.md.
- 2026-04-30: User instructed Builder to fix the actual Content skill-list visibility and preserve user-edited UI Scale/Position, and also instructed that Code Reviewer execution now requires user permission. Builder fixed the serialized root scale, kept only a zero-scale runtime guard, rebuilt, refreshed Unity, checked the console, and did not run Code Reviewer.
- 2026-04-30: User reported the `Content` child skill descriptions and checkmarks were still invisible while clicks worked. Builder changed toggle visual binding to normalize scene-bound Label/Background/Checkmark/Glyph elements, applied the same normalization to the current runtime instance, rebuilt, checked Unity console/missing-script state, and did not run Code Reviewer.
- 2026-04-30: User reported the Label skill text and checkbox were still invisible. Builder switched checkboxes to built-in Unity UI sprites, saved the actual scene slots in Edit Mode, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer.
- 2026-04-30: User reported `Failed to find UI/Skin/UISprite.psd` and asked to rebuild `SkillDebugPanel` as visible editable scene UI. Builder removed built-in UI skin sprite usage, created project-owned `Assets/Resources/DebugUiSolid.png`, saved the scene toggle visuals against that sprite, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer because user permission was not granted.
- 2026-04-30: User requested the same rebuild for `EnhancementModal` children. Builder deleted and recreated the modal children as editable scene uGUI objects using `DebugUiSolid`, verified the first choice label/checkmark state, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer because user permission was not granted.

## Task: Eve Passive Runtime Implementation

### Task title

Implement Eve passive runtime effects for the Eve skill documents under `Pakuri/reference/2.Monster/eve`.

### Goals

- Implement Eve passive effects from the existing Eve passive documents `f-voltage-calibration.md`, `g-weakness-analysis.md`, `h-particle-separation.md`, `i-cooling-algorithm.md`, and `j-overcurrent-circuit.md`.
- Connect selected passive and passive-trait reward ids to runtime combat behavior.
- Add a white shield HP bar overlay to the selected monster HP bar while keeping the full HP bar length unchanged.
- Apply behavior speed, cooldown, duration, firing interval, and damage-area adjustments according to `Pakuri/reference/3.combat/combat-stat-system.md`.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run one Code Reviewer after Builder logic changes.
- The user mentioned `k`, but the actual Eve skill folder contains `f` through `j` and no `k` file; this pass treated the existing `h-particle-separation.md` / slot H document as the missing fifth passive.
- Preserve unrelated existing worktree changes, including the prior next-work HTML report and the user-deferred `eve.asset` trailing whitespace finding.

### Role Owner

Code Builder

### Status

Builder implementation and reviewer correction pass completed. Local build/Unity console validation completed, and the follow-up external Code Reviewer returned `REVIEW_RESULT: PASS`.

### Next Actions

- User can Play Mode verify Eve passive effects, including Voltage Calibration shield/reload acceleration, Particle Separation Prism Ray proc, Cooling Algorithm freeze interactions, Overcurrent Circuit lightning bonuses, and Weakness Analysis vulnerable-target bonuses.
- Continue to the next requested design or implementation task.

### Evidence

- Actual Eve passive files present under `Pakuri/reference/2.Monster/eve/skill`: `f-voltage-calibration.md`, `g-weakness-analysis.md`, `h-particle-separation.md`, `i-cooling-algorithm.md`, and `j-overcurrent-circuit.md`; no `k` file exists.
- `combat-stat-system.md` says action speed accelerates projectile firing interval and active skill cooldown charging, while duration and firing interval are separate stats.
- `CombatRuntimeController.cs` now has learned passive state and selected monster shield runtime fields.
- `CombatRuntimeScene.cs` now creates and updates a white selected monster shield bar overlay on `MonsterHpBar`.
- `CombatRuntimeProjectiles.cs` now applies Eve passive damage/defense/status chance modifiers and selected monster shield absorption.
- `CombatRuntimeEnemies.cs` now applies selected monster shield absorption to direct enemy attacks and triggers Eve H trait 3 freeze-release damage.
- `CombatRuntimeEveSkills.cs` now implements Eve F/G/H/I/J passive checks, shield, action speed helper, passive damage multipliers, resistance reductions, status chance bonus, and particle-separation Prism Ray proc.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Initial parallel Editor build failed with a file lock on `obj\Debug\Assembly-CSharp.dll`; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` sequentially completed with 0 errors and the same warnings.
- Unity script refresh/compile was requested; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `CombatRuntimeProjectiles.cs` line 250 decrements Arc Bolt reload with raw `Time.deltaTime`, so `eve-f-trait-3` action speed does not affect reload while shielded.
- Reviewer finding 2: current uncommitted changes include the prior unrelated `Next Roadmap Work Plan Report` block in `BLACKBOARD.md` and untracked `Pakuri/reference/Report/2026-04-29-next-work-plan.html`, which are outside the Eve passive runtime implementation scope unless explicitly justified or separated.
- Reviewer finding 1 was corrected by applying `GetEveActionSpeedMultiplier()` to the Arc Bolt reload countdown in `CombatRuntimeProjectiles.UpdateSelectedMonsterCombat()`.
- Reviewer finding 2 is explicitly justified here: `Pakuri/reference/Report/2026-04-29-next-work-plan.html` and the `Next Roadmap Work Plan Report` BLACKBOARD block were created in the immediately preceding user-requested Designer task, are preserved as completed task evidence, and are not part of the Eve passive runtime implementation logic.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Follow-up parallel Editor build hit a transient write lock on `obj\Debug\Assembly-CSharp.dll`; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` sequentially completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- Follow-up external Code Reviewer confirmed prior finding 1 fixed, accepted the explicit separation/justification for prior finding 2, and returned `REVIEW_RESULT: PASS`.

### History

- 2026-04-29: User requested implementation of Eve passive effects for active skills A-E, shield HP bar overlay, and timing/range handling based on `combat-stat-system.md`.
- 2026-04-29: Code Builder confirmed actual Eve passive documents are F-J and no K document exists; implementation treated H as the missing fifth passive.
- 2026-04-29: Code Builder implemented the runtime pass and completed local build/Unity console validation.
- 2026-04-29: External Code Reviewer returned `NEEDS_CHANGES`; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to fix the reviewer findings; Builder applied the Arc Bolt reload action-speed correction and documented the prior next-work report as a separate completed user-requested task.
- 2026-04-29: Code Builder rebuilt, rechecked Unity console, and follow-up external Code Reviewer returned `PASS`.

## Task: Eve Active Skill Status Runtime

### Task title

Implement Eve active skill A-E runtime status effects before roadmap step 6.

### Goals

- Make Eve learned active skills A-E cast on player click with automatic nearest-enemy targeting.
- Keep skills from auto-casting without a click.
- Implement Eve-related combat statuses first: shock, chill/freeze blue tint, slow, vulnerability, and shield bar visuals.
- Apply selected Eve active trait choices to actual runtime behavior.
- Use Eve's implementation shape as the later framework for other monsters.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run one Code Reviewer after Builder logic changes.
- Preserve the existing user-deferred reviewer finding in `Pakuri/Assets/Data/GameData/Monsters/eve.asset` without fixing it unless requested.

### Role Owner

Code Builder

### Status

Builder implemented the user-approved correction pass for Eve A manual firing, B-E click-triggered automatic targeting, infinite skill target range, the prior reviewer findings, the mojibake status message fix, and RunScene manual transform preservation for EveUnit status visuals. Build, Unity console validation, and the required one-shot external Code Reviewer pass completed with `REVIEW_RESULT: PASS`.

### Next Actions

- User can Play Mode verify Eve A/B-E behavior and RunScene manual transform preservation.
- Continue to the next requested design or implementation task.

### Evidence

- User clarified that learned active skills should be cast by player click, auto-targeting the nearest enemy in range, but should not auto-cast by themselves.
- User clarified selected trait enhancement effects should actually apply.
- User accepted targeting recommendation for Eve D: target the nearest shocked enemy in range, and do not cast if none exists.
- User clarified chill and freeze can both use the same blue-tint visual for now and should be documented later in HTML.
- `CombatRuntimeEveSkills.cs` was added to implement Eve A-E click-cast behavior, beam/field/drone runtime objects, status application helpers, and trait checks by `eve-*-trait-*` reward ids.
- `CombatRuntimeProjectiles.cs` now supports player projectile pierce, per-projectile hit tracking, Eve drone vulnerability application, and delegates Eve click casting before legacy click-to-point firing.
- `CombatRuntimeEnemies.cs` now tracks shock/chill/freeze/slow/vulnerability timers/stacks, applies blue tint for shock/chill/freeze, and updates a white shield bar overlay.
- Enemy and selected monster HP bars are now red, while the shield bar is white.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `eve-a-trait-5` applies power +25% but not the documented lightning/status chance +35%; reviewer cited `CombatRuntimeEveSkills.cs` around line 172, `CombatRuntimeProjectiles.cs` around lines 58-60, and `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` line 52.
- Reviewer finding 2: `FreezeTimer` is declared/consumed but no code path sets it; reviewer cited `CombatRuntimeController.cs` around line 62, `CombatRuntimeEnemies.cs` around lines 643 and 671, `CombatRuntimeEveSkills.cs` around line 360, and `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` line 44.
- User clarified the correction: Eve A must be manual firing toward the clicked direction, not automatic casting or automatic targeting; that same click is the trigger for the other Eve skills.
- User clarified B-E should conditionally auto-cast and auto-target once the click trigger fires.
- User clarified skill range should be infinite; if the trigger works, the skill should execute on the nearest enemy or the skill-specific priority target.
- `CombatRuntimeProjectiles.UpdateSelectedMonsterCombat()` now calls `TryTriggerEveAutomaticSkills()` on click without consuming the primary A firing path.
- `CombatRuntimeProjectiles.FirePrimarySkill()` now routes Eve A to `FireManualEveArcBolt(direction)` after deriving the clicked direction from `currentAttackPoint`.
- `CombatRuntimeEveSkills.TryTriggerEveAutomaticSkills()` now triggers only B-E, not A.
- `CombatRuntimeEveSkills.FireManualEveArcBolt()` now applies Eve A trait projectile count, pierce, damage, fire interval, reload, and trait 5 status chance modifiers while preserving clicked-direction firing.
- `ProjectileRuntime.StatusChance` and projectile hit handling now allow Eve A trait 5 to add +35% status chance without changing the global configured chance for other projectiles.
- Eve B, C, D, and drone E targeting now use `float.PositiveInfinity` range; D still keeps its shocked-target predicate as the skill-specific priority.
- `SkillEffectRuntime.FreezeDuration` is now set by `eve-c-trait-5`, and Frost Field ticks apply `enemy.FreezeTimer` when that trait is selected.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review for the correction pass returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Latest reviewer finding: `CombatRuntimeEveSkills.cs` contains mojibake user-facing `statusLabel` messages at and around lines 87, 106, 171, 288, 353, 425, and 489. Reviewer verified the core logic requirements as satisfied but flagged the visible broken text.
- `CombatRuntimeEveSkills.cs` statusLabel messages at lines 87, 106, 171, 288, 354, 425, and 489 were changed to readable ASCII English text to resolve the mojibake finding.
- `CombatRuntimeScene.EnsureStatusLabel()` now preserves existing `MonsterHpLabel` local position and scale, assigning defaults only when the label object is newly created.
- `CombatRuntimeEnemies.CreateHpBar()` now preserves existing `MonsterHpBar` root position and scale and preserves existing Background/Fill transforms, assigning defaults only when those objects are newly created.
- `CombatRuntimeEnemies.CreateShieldBarFill()` now preserves an existing Shield transform and only assigns default shield transform values when newly created.
- `CombatRuntimeScene.EnsureSpriteRenderer()` no longer overwrites existing anchors with SpriteRenderers; in the current `RunScene`, `EveUnit` already has a SpriteRenderer, so its scene-authored scale is preserved.
- `CombatRuntimeScene.EnsureBattlefieldBackgroundVisual()` no longer forces `BattlefieldBackground` position; scale is still only changed when `autoFitBattlefieldBackgroundToField` is true. `RunScene.unity` currently has `autoFitBattlefieldBackgroundToField: 0`.
- `Pakuri/Assets/Scenes/RunScene.unity` contains actual scene-authored `EveUnit`, `MonsterHpLabel`, `MonsterHpBar`, and `BattlefieldBackground` objects.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review for the latest changes returned `REVIEW_RESULT: PASS`.
- Added `Pakuri/reference/Report/2026-04-29-eve-active-skill-runtime-implementation.html` documenting the Eve A-E runtime implementation, the user clarification process that reduced implementation ambiguity, status/effect wiring, manual transform preservation, and verification results.

### History

- 2026-04-29: User requested Eve Monster active skill A-E status/effect runtime before roadmap step 6 and provided detailed semantics for pierce, extra projectiles, beams, area instant skills, drones, blue status tint, red HP bar, and white shield bar.
- 2026-04-29: Designer asked five implementation interpretation questions; user clarified click-cast auto-targeting, actual trait application, D shocked-target behavior, and blue tint for both ice states.
- 2026-04-29: Code Builder implemented Eve A-E runtime behavior and completed local build/Unity console validation.
- 2026-04-29: External Code Reviewer found two missing trait/status behavior issues; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to prioritize restoring A as manual clicked-direction firing, make B-E click-triggered automatic infinite-range skills, and fix the two reviewer findings.
- 2026-04-29: Code Builder implemented the correction pass and completed local build/Unity console validation; required external Reviewer pass remains pending.
- 2026-04-29: External Code Reviewer verified the correction logic but returned `NEEDS_CHANGES` for mojibake status messages in `CombatRuntimeEveSkills.cs`; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to fix the reviewer finding and preserve manually edited RunScene `EveUnit` child HP Label/HPBar position and scale, plus other scene-authored transforms where applicable.
- 2026-04-29: Code Builder fixed Eve status messages, preserved existing status visual transforms and scene-authored anchor transforms, completed build/Unity validation, and external Code Reviewer returned `PASS`.
- 2026-04-29: Code Builder added an HTML implementation report for the Eve active skill runtime work under `Pakuri/reference/Report`.

## Task: Eve Initial Combat Preview

### Task title

Legacy non-English note retained these code references: `dungeon-squad-run-structure.md`.

### Goals

- Legacy non-English note retained these code references: `reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/eve/eve-tower.md`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/Scene/combat-scene-layout.md`, `(2,8)`, `(4~10, 3~15)`.
- Legacy non-English note retained these code references: `Pakuri/reference/dungeon-squad-combat-player-controls.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/5.enemy/stage-1-enemies.md`.
- Legacy non-English note retained these code references: `manage_scene get_active`, `Assets/Scenes/SampleScene.unity`, `manage_scene get_hierarchy`, `Main Camera`, `Global Light 2D`.
- Legacy non-English note retained these code references: `manage_asset search`, `Assets`, `Scenes`, `Settings`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-run-structure.md`, `eve-tower.md`, `current-architecture-plan.html`.
- Legacy non-English note retained these code references: `a-arc-bolt.md`, `combat-scene-layout.md`, `combat-reward-system.md`, `dungeon-squad-combat-player-controls.md`, `combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `stage-1-enemies.md`.
- Legacy non-English note retained these code references: `Pakuri/reference`.

## Task: Eve Combat Vertical Slice Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `eve-initial-combat-vertical-slice-preview.html`.
- Legacy non-English note retained these code references: `CombatRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed with manual reviewer pass in-session. External Codex reviewer commands timed out and did not produce a new review artifact.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `codex review`, `codex exec`.

### Evidence

- Legacy non-English note retained these code references: `Assets/Scripts/Combat/DamageCalculator.cs`.
- Legacy non-English note retained these code references: `Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `manage_asset search path=Assets/Scripts`, `Combat`, `DamageCalculator.cs`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `SampleScene.unity`, `CombatRoot`, `Pakuri.Combat.EveVerticalSliceController`.
- Legacy non-English note retained these code references: `manage_scene get_hierarchy include_transform=true`.
  - Legacy non-English note retained these code references: `Main Camera`, `15.5, 8.5, -10`.
  - Legacy non-English note retained these code references: `Nexus`, `2, 8, 0`.
  - Legacy non-English note retained these code references: `EveUnit`, `6, 8, 0`.
  - Legacy non-English note retained these code references: `EnemySpawnPoint`, `29, 8, 0`.
  - Legacy non-English note retained these code references: `InputTarget`, `16, 8, 0`.
- Legacy non-English note retained these code references: `SampleScene.unity`, `orthographic: 1`, `orthographic size: 10`, `CombatRoot`, `EveVerticalSliceController`.
- Legacy non-English note retained these code references: `execute_code`.
  - Legacy non-English note retained these code references: `Enemy_Normal_01`, `Enemy_Boss_01`.
  - Legacy non-English note retained these code references: `battleResolved=True`, `victory=True`, `waitingForRewardChoice=True`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - `Assets/Screenshots/screenshot-20260424-165841.png`
  - `Assets/Screenshots/screenshot-20260424-165958.png`
- Legacy non-English note retained these code references: `validate_script`, `DamageCalculator.cs`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `codex review --uncommitted`.
- Legacy non-English note retained these code references: `codex exec`.
- Legacy non-English note retained these code references: `DamageCalculator.cs`, `EveVerticalSliceController.cs`, `SampleScene.unity`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `eve-initial-combat-vertical-slice-preview.html`.
- Legacy non-English note retained these code references: `Assets/Scripts`, `Assets/Scripts/Combat`.
- Legacy non-English note retained these code references: `DamageCalculator.cs`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `CombatRoot`, `EveVerticalSliceController`.
- Legacy non-English note retained these code references: `Main Camera`.
- Legacy non-English note retained these code references: `ExecuteAlways`, `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `codex review --uncommitted`, `codex exec`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

## Task: Eve Projectile Click Hold Compliance Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/dungeon-squad-combat-player-controls.md`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`, `wasPressedThisFrame`, `GetMouseButtonDown(0)`.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-hold-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-combat-player-controls.md`, `a-arc-bolt.md`, `combat-attribute-and-damage-system.md`, `EveVerticalSliceController.cs`, `eve-combat-implementation-report.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-hold-plan.html`.

## Task: Eve Projectile Click Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed without Code Review. External reviewer commands timed out again, so only Builder-side validation was performed.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`, `ProjectileRuntime`, `projectileRoot`, `UpdateProjectiles()`, `TryHitEnemy()`, `HandlePointerInput()`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scenes/SampleScene.unity`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `manage_scene save`, `Assets/Scenes/SampleScene.unity`.
- Legacy non-English note retained these code references: `find_gameobjects by_name ProjectileRoot`, `ProjectileRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - Legacy non-English note retained these code references: `projectileCount = 1`.
  - Legacy non-English note retained these code references: `projectileCount = 0`.
  - Legacy non-English note retained these code references: `enemyHealth = 37.95`.
  - Legacy non-English note retained these code references: `currentShotsRemaining = 0`, `reloadRemaining = 4.0`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Screenshots/eve-projectile-click-runtime.png`.
- Legacy non-English note retained these code references: `validate_script`.
- Legacy non-English note retained these code references: `read_console`, `FindObjectOfType<Camera>()`, `FindFirstObjectByType<Camera>()`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - `codex review --uncommitted` timeout
  - Legacy non-English note retained these code references: `codex exec`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `eve-projectile-click-hold-plan.html`, `a-arc-bolt.md`, `dungeon-squad-combat-player-controls.md`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `ProjectileRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `FireArcBolt()`.
- Legacy non-English note retained these code references: `FindFirstObjectByType<Camera>()`.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-implementation-report.html`.
- Legacy non-English note retained these code references: `codex review --uncommitted`, `codex exec`.

# Task: 2026-05-08 Manifested Eve Arc Bolt Correction

### Task title

Prevent Manifested Eve A from using Prism Ray prefab/line behavior.

### Goals

- Keep Eve A as `MagazineProjectile` and default learned for Manifested Eve.
- Remove the CSV `eve-a` reference to the Eve B Prism Ray prefab.
- Route Manifested Eve A through projectile sprite and magazine/reload state.

### Constraints

- Role Owner is Code Builder.
- Eve-specific CSV data and combat behavior must remain evidence-based.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve fires Arc Bolt-style projectiles and does not show the Prism Ray prefab.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:13` now leaves `eve-a` `skill_effect_prefab_path` empty.
- `monster_skills.csv:14` still keeps `eve-b` pointing at `Assets/Image/Monster/Eve/Effect_Prefab/Eve_Skill_B.prefab`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:465` creates Manifested A projectile visuals from `runtime.Monster.ProjectileSprite`, not `SkillEffectPrefab`.
- `CombatRuntimeParty.cs:418` through `:463` applies magazine/reload state to Manifested Eve A because `eve-a` is `MagazineProjectile` with `MagazineCapacity=6`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported Manifested Eve played the B Prism Ray effect and attacked abnormally instead of firing Arc Bolt.
- 2026-05-08: Code Builder removed the incorrect `eve-a` CSV effect-prefab reference and changed Manifested projectile handling.

# Task: 2026-05-08 Manifested Eve Sustained Skills Follow-up

### Task title

Keep Manifested Eve Prism Ray, Frost Field, and Drone Beacon visible for their Eve runtime durations.

### Goals

- Use Eve's existing selected-monster duration constants for Manifested Eve sustained visuals.
- Make Manifested Eve Drone Beacon deploy a timed drone that fires projectiles.

### Constraints

- Role Owner is Code Builder.
- This pass changes the Manifested party runtime path, not the selected 1P Eve runtime path.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve B, C, and E in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` defines `EveBeamDuration = 1.2f`, `EveFrostFieldDuration = 4f`, `EveDroneDuration = 5f`, and `EveDroneAttackPeriod = 0.8f`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now maps `eve-b`, `eve-c`, and `eve-e` to those durations in `ResolveManifestedSkillVisualDuration(...)`.
- `CombatRuntimeParty.cs` now routes Manifested `eve-e` through `DeployManifestedEveDroneBeacon(...)` before the generic projectile branch.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User specifically named Eve Drone Beacon, Frost Field, and Prism Ray as sustained skills whose Manifested duration appeared too short.

# Source: boards/MON/RIN_MONSTER.md

## Task: 2026-05-05 Rin MonsterPanel Skill Kind Correction

### Task title

Correct Rin active skill runtime kinds for MonsterPanel ammo and cooldown display.

### Goals

- Keep Rin A as the only Rin magazine projectile skill for MonsterPanel ammo display.
- Make Rin B Howling, C Shockwave, D Finishing Blow, and E Collapse Strike use their own cooldown state instead of the shared A-skill magazine/reload state.
- Preserve the shared MonsterPanel behavior for RunScene and DebugScene.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Rin Active1 shows ammo, while Howling/Shockwave in Active2/3 show no ammo and use their own cooldown overlay timing.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before this fix, `Pakuri/Assets/Data/GameData/Monsters/rin.asset` stored Rin B, C, D, and E as `RuntimeKind: 0`, which maps to `SkillRuntimeKind.MagazineProjectile`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` also stored `rin-b`, `rin-c`, `rin-d`, and `rin-e` as `MagazineProjectile`.
- `rin.asset` now stores Rin B as `RuntimeKind: 5` (`Buff`), Rin C as `RuntimeKind: 2` (`LineAttack`), Rin D as `RuntimeKind: 9` (`Execute`), and Rin E as `RuntimeKind: 3` (`AreaAttack`).
- `monster_skills.csv` now stores `rin-b` as `Buff`, `rin-c` as `LineAttack`, `rin-d` as `Execute`, and `rin-e` as `AreaAttack`, with `RuntimeImplemented`.
- Unity-MCP read-only Editor code after asset import reported `rin-a:MagazineProjectile:mag=10:cd=0|rin-b:Buff:mag=0:cd=12|rin-c:LineAttack:mag=0:cd=5.5|rin-d:Execute:mag=0:cd=9|rin-e:AreaAttack:mag=0:cd=8`.
- `CombatRuntimeController.CreateMonsterPanelSkillView(...)` now requires `MagazineCapacity > 0` in addition to `RuntimeKind == MagazineProjectile` before a skill can use ammo/reload state.
- `CombatMonsterPanelUiController.ApplySlot(...)` now disables ammo text for non-magazine skills and `EnsureCooldownOverlay(...)` assigns `DebugUiSolid` to the overlay image.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and sequential `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.

### History

- 2026-05-05: User reported that adding Howling and Shockwave made Active2/3 appear, but those non-magazine skills still showed ammo, followed Active1 cooldown, and skipped the visible black-to-white cooldown fill.
- 2026-05-05: Builder found the concrete data cause in Rin skill RuntimeKind values and fixed both the Rin data and the shared MonsterPanel UI guard.

## Task: 2026-05-05 Rin F Follow-up Visual And Debug Damage Labels

### Task title

Add Rin F follow-up visual feedback and debug damage-type labels.

### Goals

- Show a white circle effect when Rin F `Ambidextrous` follow-up damage is applied.
- Show debug-only damage popup text with the damage attribute after the number, such as `32(臾쇰━)` or `34(踰덇컻)`.
- For Rin F mixed follow-up damage, combine the terms with ` + ` in one white popup, such as `32(臾쇰━) + 45(踰덇컻)`.

### Constraints

- Role Owner is Code Builder.
- The popup notation is debugging-only display text.
- Current code evidence shows `DamageAttribute` has only `Physical`, `Fire`, `Lightning`, `Ice`, `Darkness`, and `Holy`; no additional damage attributes were found in code.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Rin F follow-up hits show the white circle effect.
- User verifies in Play Mode that damage popups show the debug attribute notation and that mixed Rin F follow-up terms use ` + `.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs` defines six damage attributes: `Physical`, `Fire`, `Lightning`, `Ice`, `Darkness`, and `Holy`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now formats typed damage popups through `FormatDamagePopupAmount(...)`, `FormatDamagePopupTerm(...)`, and `GetDamageAttributeKoreanLabel(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` now creates `RinAmbidextrousFollowup` with a white `CreateCircleEffect(...)` result and a combined popup for physical plus optional lightning follow-up damage.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now returns total applied damage, including shield absorption, so Rin F visual feedback is not skipped when the follow-up is absorbed by shield.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors; Unity ready wait timed out after the compile request.
- `git diff --check` on changed combat and board files completed with no whitespace errors and CRLF conversion warnings only.

### History

- 2026-05-05: User requested a white circle effect for Rin F follow-up hits and debug damage popup labels such as `32(臾쇰━)` and mixed terms like `32(臾쇰━) + 45(踰덇컻)`.
- 2026-05-05: Builder implemented the Rin F follow-up visual, typed debug popup labels, mixed popup composition, total-applied-damage return alignment, and local validation.

## Task: 2026-05-05 Rin F-J Passive Runtime Implementation

### Task title

Implement Rin passive skills F-J and their trait effects.

### Goals

- Implement Rin F `Ambidextrous`, G `Battle Resonance`, H `Wave Amplification`, I `Finisher Instinct`, and J `Collapse Aftermath`.
- Keep passive behavior grounded in `Pakuri/reference/2.Monster/rin/skill/f-ambidextrous.md` through `j-collapse-aftermath.md`.
- Mark Rin passive definitions F-J as runtime implemented in the Rin monster asset.

### Constraints

- Role Owner is Code Builder.
- Current runtime has one selected allied Monster, so "all ally" passive wording applies to the current selected Monster combat model.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin F-J passive behavior in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/rin/skill/f-ambidextrous.md`, `g-battle-resonance.md`, `h-wave-amplification.md`, `i-finisher-instinct.md`, and `j-collapse-aftermath.md` were read with UTF-8 decoding and used as source references.
- The initially guessed filenames `f-fighting-spirit.md`, `g-berserk-instinct.md`, `h-battle-flow.md`, `i-breaking-strategy.md`, and `j-body-mastery.md` do not exist in the Rin skill folder.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` now tracks Rin passive timers/counters for H auto shockwave, I kill buffs, and J multi-hit buffs.
- `CombatRuntimeRinSkills.cs` now implements F physical damage bonus and C/D/E follow-up hit, G Howling attack/action/crit/reload effects, H physical-hit-count auto Shockwave, I low-health target damage and Finishing Blow kill buffs, and J Collapse Strike physical-defense reduction plus 3-hit buffs and kill cooldown charging.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores Rin physical-defense reduction state on `EnemyRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now ticks Rin physical-defense reduction state and displays `臾쇰갑媛먯냼` while active.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now routes Rin projectile damage through the Rin percent-defense-reduction and kill-trigger helper paths.
- `Pakuri/Assets/Data/GameData/Monsters/rin.asset` now marks passive F-J `ImplementationState: 2`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh returned `resulting_state=idle`; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.

### History

- 2026-05-05: User requested implementing Rin passive skills F-J from `Pakuri/reference/2.Monster/rin/skill` and asked to ask questions if terms were unclear.
- 2026-05-05: Builder verified the actual Rin passive filenames, restored readable Korean text with `Get-Content -Encoding UTF8`, implemented F-J runtime hooks, marked Rin passive data implemented, and validated builds plus Unity console state.

## Task: 2026-05-04 Rin D Execution Target And Hit Effect Fix

### Task title

Make Rin D `Finishing Blow` cast only on execution-threshold enemies and show a simple hit effect.

### Goals

- Stop Rin D from attacking full-health or otherwise non-executable enemies.
- Make Rin D cast only when an enemy is at or below the execution-health threshold.
- Preserve the master skill meaning that execution threshold `-10%` changes the base threshold from 30% max HP to 20% max HP.
- Add a simple visible circle effect when Rin D lands.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Rin D does not fire when every enemy is above the current execution threshold.
- User verifies in Play Mode that Rin D shows the new circle hit effect on an executable target.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Before this fix, `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` `FindRinFinishingBlowTarget(...)` kept a nearest-enemy fallback and returned `executeTarget ?? fallback`, so Rin D could attack a 100% HP enemy whenever no enemy met the execution threshold.
- `CombatRuntimeRinSkills.cs:604-632` now returns only an enemy whose health ratio is at or below `GetRinFinishingBlowExecuteThreshold()`, with no nearest fallback.
- `CombatRuntimeRinSkills.cs:635-649` now centralizes the execution threshold calculation: base `30%`, trait 2 `+10%`, and master 2 `-10%`, so master 2 changes the base 30% threshold to 20% when applied alone.
- `CombatRuntimeRinSkills.cs:336` now creates a Rin D hit effect after damage is applied, and `:651-667` creates `RinFinishingBlowHit` using the existing circle effect helper and generated circle sprite.
- `Pakuri/reference/2.Monster/rin/skill/d-finishing-blow.md:55-61` now documents that Finishing Blow only casts on enemies at or below the threshold, does not fire without such a target, and that threshold `-10%` means 30% to 20%.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh reached idle with `ready_for_tools=true`; Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.

### History

- 2026-05-04: User reported Rin D was targeting 100% HP enemies instead of only enemies at or below 30% max HP.
- 2026-05-04: Builder found the cause was the nearest-enemy fallback in `FindRinFinishingBlowTarget(...)`, removed that fallback, added the hit effect, clarified the master threshold rule, and validated with builds plus Unity refresh/console checks.

## Task: 2026-05-04 Rin Non-Magazine Skill Map-Wide Range

### Task title

Make Rin non-magazine active skills use the whole battlefield map as runtime range.

### Goals

- Document that Rin non-magazine skills B-E use the whole battlefield map as target/search range.
- Keep Rin A as the magazine projectile skill, while changing Rin C/D/E runtime target acquisition away from short asset `Range` values.
- Explain why Rin skills initially felt very short-ranged.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin B-E in Play Mode, especially C/D/E against enemies beyond the old 7.5-8.5 unit range.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Data/GameData/Monsters/rin.asset` stores short active-skill ranges: `rin-c` Range `8.5`, `rin-d` Range `7.5`, and `rin-e` Range `8`.
- Before this change, `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` used those `skill.Range` values directly in `FindNearestEnemy(...)` / `FindRinFinishingBlowTarget(...)`, so non-magazine target selection was limited to those short distances from `eveAnchor`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:19` adds `RinMapWideSkillRangePadding`, and `:731-735` computes battlefield-wide range from `fieldSize`, `EnemySpawnX`, and `BattlefieldMaxY`.
- `CombatRuntimeRinSkills.cs:196-211` now uses the map-wide range for Rin C target search and beam length.
- `CombatRuntimeRinSkills.cs:297-298` now uses the map-wide range for Rin D target search.
- `CombatRuntimeRinSkills.cs:367-368` now uses the map-wide range for Rin E target search.
- `Pakuri/reference/2.Monster/rin/skill/b-howling.md:41-44`, `c-shockwave.md:53-56`, `d-finishing-blow.md:55-58`, and `e-collapse-strike.md:53-56` now document the Rin non-magazine whole-battlefield runtime range rule.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing System.Net.Http/System.IO.Compression warnings.
- A first parallel `Assembly-CSharp-Editor` build failed with `CS2012` because the shared `obj\Debug\Assembly-CSharp.dll` was locked by the simultaneous build process; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` by itself completed with 0 errors and the same existing warnings.
- Unity refresh reached idle; Unity console error query returned only MCP-FOR-UNITY client handler disposal/exit logs, not project compile errors.

### History

- 2026-05-04: User reported Rin skill range was very short and clarified that non-magazine skills should treat the whole map as range.
- 2026-05-04: Builder found the short range came from Rin C/D/E reading the `SkillDefinition.Range` values from `rin.asset`.
- 2026-05-04: Builder documented the non-magazine map-wide range rule in Rin B-E skill markdown files and changed Rin C/D/E runtime target search to use battlefield-wide range.

## Task: 2026-05-04 Rin Runtime Sprite And A Projectile Cleanup Follow-up

### Task title

Fix Rin runtime sprite catalog paths and prevent Rin A from expiring by short lifetime.

### Goals

- Ensure Rin's runtime selected-Monster sprite and projectile sprite resolve through the CSV runtime catalog.
- Make Rin A projectile deletion follow the common selected-Monster projectile X-edge rule instead of the computed lifetime.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this follow-up.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin A travel and cleanup in Play Mode.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Unity AssetDatabase inspection showed `Pakuri/Assets/Data/GameData/Monsters/rin.asset` already had `UnitSprite=Rin_Temp (2)` and `ProjectileSprite=Rin_Shoot`.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now fills Rin with `Assets/Image/Monster/Rin/Rin_Temp (2).png` and `Assets/Image/Monster/Rin/Rin_Shoot.png`.
- Unity-MCP import/sync validation resolved runtime `rin` as `UnitSprite=Rin_Temp (2), ProjectileSprite=Rin_Shoot, ProjectileLifetime=4`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now uses `HasPlayerProjectileReachedBattlefieldXEdge(...)` for selected-Monster projectile cleanup after no hit, so Rin A no longer depends on `range / RinShatteringFistProjectileSpeed` lifetime for cleanup.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log.

### History

- 2026-05-04: User reported Rin's sprite application issue and Rin A disappearing again after about 0.5 seconds.
- 2026-05-04: Builder fixed the CSV runtime sprite path and changed selected-Monster projectile cleanup to battlefield X-edge cleanup.

## Task: 2026-05-04 Rin A-E Active Runtime Implementation

### Task title

Implement Rin active skills A-E and their enhancement/master effects from the Rin reference skill documents.

### Goals

- Implement `rin-a` Shattering Fist as Rin's physical magazine projectile.
- Implement `rin-b` Howling, `rin-c` Shockwave, `rin-d` Finishing Blow, and `rin-e` Collapse Strike in the selected-Monster combat runtime.
- Apply active-skill trait and master choices from the Rin A-E reference documents.
- Use the user's confirmed defaults: `rin-e` auto-targets the nearest enemy, Rin A chain radius is `3.0`, Rin D explosion radius is `2.4`, and elemental extra damage is calculated from the physical final damage dealt by the source hit.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this implementation.

### Role Owner

Code Builder

### Status

Implemented, Reviewer findings fixed, and locally validated. Code Reviewer has not been rerun because the user has not requested a second review.

### Next Actions

- User verifies Rin A-E behavior in Play Mode.
- Run another Code Reviewer pass only if the user explicitly requests it.
- If a later full ally-party model is added, revisit Howling's all-ally wording; this implementation applies it to the current selected Monster combat model.

### Evidence

- `Pakuri/reference/2.Monster/rin/skill/a-shattering-fist.md` through `e-collapse-strike.md` were read as the source references for Rin A-E values and enhancement/master effects.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:77` implements Rin A projectile fire, `:152` implements Howling, `:187` implements Shockwave, `:287` implements Finishing Blow, and `:357` implements Collapse Strike.
- `CombatRuntimeRinSkills.cs:445` handles Rin A master 2 extra lightning and every-third-hit chain using the user-confirmed `3.0` radius.
- `CombatRuntimeProjectiles.cs:52` now passes the actual `appliedDamage` from `ApplyDamageToEnemy(...)` into `HandleRinProjectileHit(...)` for Rin A follow-up effects.
- `CombatRuntimeRinSkills.cs:502`, `:504`, and `:523` now use and return the actual applied physical/additional damage instead of returning the calculated `FinalDamage`.
- `CombatRuntimeRinSkills.cs:507` applies elemental extra damage from the actual physical damage applied by the source hit.
- `Pakuri/Assets/Data/GameData/Monsters/rin.asset:88`, `:155`, `:222`, `:287`, and `:354` mark Rin A-E as `ImplementationState: 2`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- Unity-MCP script refresh reached `resulting_state: idle`; Unity console error query returned only MCP-FOR-UNITY client handler logs.
- External Reviewer output at `codex_loop_logs\rin_skill_reviewer_20260504.md` returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding was fixed: Rin A/C/D/E elemental extra damage now receives the physical damage actually dealt after `ApplyDamageToEnemy(...)` caps shields and remaining HP.

### History

- 2026-05-04: User requested implementation of Rin active skills A-E and enhancements from `Pakuri/reference/2.Monster/rin/skill`.
- 2026-05-04: Code Builder asked for clarification on targeting, branch/explosion radius, and elemental extra-damage basis.
- 2026-05-04: User confirmed the default targeting/radius assumptions and clarified that elemental extra damage is based on the physical damage dealt by the source hit.
- 2026-05-04: Code Builder implemented Rin A-E runtime behavior and validated with dotnet builds plus Unity-MCP refresh/console checks.
- 2026-05-04: User requested Code Reviewer execution; external Reviewer ran once and returned `NEEDS_CHANGES` for the elemental extra-damage basis.
- 2026-05-04: User requested fixing the Reviewer findings; Code Builder changed Rin projectile and skill follow-up damage to use applied damage, then revalidated with both dotnet builds and Unity-MCP refresh/console checks.

# Source: boards/MON/SEIN_MONSTER.md

## Task: 2026-05-08 Manifested Sein A Pierce And Hit-State Fix

### Task title

Make manifested Sein A keep selected Sein A pierce and hit-state behavior.

### Goals

- Fix manifested Sein A base pierce being lost in the generic manifested projectile path.
- Apply manifested Sein A pierce upgrades from `sein-a-trait-4` and `sein-a-master-1`.
- Preserve Sein A hit-state behavior by setting `SeinScorchingArrowTimer` from manifested projectile hits.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Sein A pierces one enemy by default and more when the relevant choices are learned.
- User verifies manifested Sein A heat-state interactions in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:1057` shows selected Sein A pierce starts at `1`, adds `sein-a-trait-4`, and adds `sein-a-master-1`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:674` now sends generic manifested projectiles through `ResolveManifestedProjectilePierce(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:688` through `:694` implements manifested Sein A pierce as selected base `1`, plus trait 4 and master 1.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:825` sets `enemy.SeinScorchingArrowTimer` on manifested Sein A hits and checks `sein-a-master-2` via the projectile's `ManifestedSource`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported selected Sein A had one pierce, but manifested Sein A had no pierce.

## Task: 2026-05-08 Manifested Sein Common Runtime Parity

### Task title

Apply Sein Offering choices and field status through the manifested common skill runtime.

### Goals

- Keep manifested Sein skills sourced from `SkillDefinition` data.
- Apply Sein manifested Offering choices in shared damage, cooldown, reload, and shot interval paths.
- Let manifested Sein D field ticks mark superheated zone state.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Sein private skill code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Sein skills and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:760` applies Sein D manifested field tick superheated-zone state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Sein skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Sein cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1278` and `:1310` include Sein reload and shot-interval choice handling.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

## Required Sections For Future Work

- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Task: 2026-05-04 Sein Runtime Sprite Catalog Follow-up

### Task title

Fix Sein runtime sprite catalog paths.

### Goals

- Ensure Sein's runtime selected-Monster sprite and projectile sprite resolve through the active CSV runtime catalog.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this follow-up.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein selection and projectile visuals in Play Mode.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Unity AssetDatabase inspection showed `Pakuri/Assets/Data/GameData/Monsters/sein.asset` already had assigned `UnitSprite` and `ProjectileSprite`.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now fills Sein with `Assets/Image/Monster/Sein/Sein_Temp.png` and `Assets/Image/Monster/Sein/Sein_Shoot.png`.
- Unity-MCP import/sync validation resolved runtime `sein` with non-null UnitSprite and ProjectileSprite assets.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains the Sein sprite path entries generated from the CSV runtime source.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log.

### History

- 2026-05-04: User reported Sein was one of the two Monsters whose `PrototypeCombatTuning` sprites were not applied.
- 2026-05-04: Builder found the active runtime CSV path was empty for Sein and filled/synced the runtime catalog.

## Task: 2026-05-06 Sein A-E Active Runtime Implementation

### Task title

Implement Sein active skills A-E from the reference skill documents.

### Goals

- Implement Sein A `Scorching Arrow` as the manual magazine projectile path.
- Implement Sein B-E as click-triggered automatic skills using current selected-Monster combat runtime.
- Mark Sein A-E runtime data as implemented and classify B-E with non-shared-magazine runtime kinds where needed.

### Constraints

- Role Owner is Code Builder.
- Implementation is grounded in `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md` through `e-doomsday-line.md`.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein A-E behavior in Play Mode, especially A magazine fire, B volley cadence, C delayed explosion, D field ticks, and E fire-resistance reduction.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md` through `e-doomsday-line.md` define Sein A-E active skill stats and trait/master effects.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now implements Sein A-E runtime helpers and selected-Monster checks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` routes Sein A primary fire through `FireManualSeinScorchingArrow(...)` and calls `TrackSeinProjectileHit(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` now includes Sein in selected-Monster cooldown, automatic-skill, magazine, and action-speed dispatch.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` adds enemy state for Sein heat/fire-resistance effects and panel cooldown reporting for Sein B-E.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` now mark Sein A-E as `RuntimeImplemented`; B is `CooldownProjectile`, C is `AreaAttack`, D is `Field`, and E is `LineAttack`.
- Unity-MCP `execute_code` confirmed both `sein.asset` and `PakuriCsvRuntimeData.ResolveCatalogOrFallback(...)` read Sein A-E as `RuntimeImplemented` with the expected runtime kinds.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity refresh/compile was requested; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.

### History

- 2026-05-06: User requested implementation of active skills A-E under `Pakuri/reference/2.Monster/sein/skill`.
- 2026-05-06: Code Builder implemented Sein A-E runtime code, updated data classifications, and completed local build/Unity-MCP validation.

## Task: 2026-05-06 Sein F-J Passive Runtime Implementation

### Task title

Implement Sein passive skills F-J from the reference skill documents and CSV data.

### Goals

- Implement Sein F heated-aim passive bonuses for fire damage, fire crit chance, crit damage trait, and Scorching Arrow magazine trait.
- Implement Sein G flame-barrage passive auto Blazing Volley proc with internal cooldown and proc traits.
- Implement Sein H, I, and J target debuff/passive interactions for fire resistance, fire damage taken, Superheated Zone tick speed/radius, and Doomsday cooldown charge.
- Mark Sein F-J runtime data as implemented.

### Constraints

- Role Owner is Code Builder.
- Implementation is grounded in `Pakuri/reference/2.Monster/sein/skill/f-heated-aim.md` through `j-doomsday-omen.md`, `Pakuri/Assets/CSVdata/source/monster_skills.csv`, and `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein F-J passive behavior in Play Mode, especially G auto volley proc cadence, H/I/J target debuffs, and J cooldown charge on Doomsday kill.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now implements Sein F-J passive checks, fire damage/critical modifiers, Flame Barrage proc handling, H/I/J timed target debuffs, D tick speed/radius passive modifiers, and J cooldown charge helper.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now applies Sein final-damage, critical chance, critical multiplier, flat fire-defense reduction, and projectile-hit passive tracking hooks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores enemy timers and damage-taken bonuses for Sein H/I/J passive debuffs.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` now mark Sein F-J as `RuntimeImplemented`.
- Unity-MCP `execute_code` confirmed runtime catalog rows `sein-f` through `sein-j` resolve as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity refresh/compile was requested; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.

### History

- 2026-05-06: User requested implementation of the remaining Sein passive skills F-J.
- 2026-05-06: Code Builder implemented Sein F-J passive runtime code, updated data implementation flags, and completed local build/Unity-MCP validation.

## Task: 2026-05-07 Sein Active Skill Correction Pass

### Task title

Correct Sein A, B, C, and E active skill behavior from user gameplay feedback.

### Goals

- Make A `??뿼 ?붿궡` explosion also damage the original hit target.
- Change B from fan-like volley cooldown behavior to a separate 4-shot magazine skill where each use fires 5 arrows and reloads for 6 seconds.
- Change C into a target-locked curved projectile that explodes on contact with its selected enemy target.
- Change E so its visual lines start from sky position `(10, 10)` and damage only up to one target per line.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies A explosion target inclusion, B magazine/reload cadence, C target-locked projectile explosion, and E sky-origin target-only lines in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now removes the A explosion exclusion so `sein-a-master-2` also damages the original hit target.
- `CombatRuntimeSeinSkills.cs` now tracks B ammo, reload, and shot interval separately with `seinBlazingVolleyShotsRemaining`, `seinBlazingVolleyReloadRemaining`, and `seinBlazingVolleyShotCooldownRemaining`.
- `CombatRuntimeSeinSkills.cs` now creates target-locked C projectiles through `FireSeinFlameTrajectoryProjectile(...)` and updates their homing/arc movement through `UpdateSeinLockedTargetProjectile(...)`.
- `CombatRuntimeSeinSkills.cs` now creates E line visuals from `GetSeinDoomsdaySkyOrigin()` and selects one enemy per line through `FindNearestSeinDoomsdayTarget(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now resolves locked-target projectiles only against their assigned enemy target and shares projectile damage resolution through `ResolvePlayerProjectileDamage(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores locked-target projectile metadata and shows Sein B panel ammo/cooldown using the B magazine state.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` now classify `sein-b` as `MagazineProjectile`.
- Unity-MCP `execute_code` confirmed both `sein.asset` and `PakuriCsvRuntimeData.ResolveCatalogOrFallback(null)` read `sein-b` as `MagazineProjectile`, `RuntimeImplemented`, magazine `4`, reload `6`, cooldown `6`, and interval `0.18`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity console error query after refresh returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-07: User clarified four corrections for Sein A, B, C, and E active skills.
- 2026-05-07: Code Builder applied the correction pass and validated through builds, Unity import, runtime catalog inspection, console check, and `git diff --check`.

## Task: 2026-05-07 Sein C/E Follow-up Correction

### Task title

Fix Sein C delayed explosion/path residual behavior and E ash-zone target placement.

### Goals

- Make C `Flame Trajectory` explode after projectile contact using the documented 0.8 second delay.
- Make C `Piercing Trajectory` leave short path segments while the projectile travels instead of drawing/applying the full line immediately.
- Make C `Falling Trajectory` spawn its residual fire zone after the delayed explosion.
- Make E `Ashen Sky` create zones on actual hit targets instead of grouping all zones around the initially selected target.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies C delayed explosion, C moving path trail, C residual fire zone, and E per-target ash zones in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/c-flame-trajectory.md:23` defines C delay as `0.8珥?; `:49` defines `?숉솕 沅ㅼ쟻`; `:50` defines `愿??沅ㅻ룄`.
- `Pakuri/reference/2.Monster/sein/skill/e-doomsday-line.md:22` defines E as 3 straight-line hits; `:51` defines `?용튆 ?섎뒛`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:25`, `:29`, and `:52` now record previous projectile position, create C path segments during travel, and route C contact into delayed impact handling before normal projectile damage.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:484` implements C moving path segment visuals/damage; `:529` implements delayed C impact; `:624` creates C falling residual fire zone after explosion expiry.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:358` passes actual E hit enemies into ash-zone creation; `:762` creates up to 3 ash zones from those target positions.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings when rerun without the parallel file-lock conflict.
- `git diff --check` completed with no whitespace errors; it reported only CRLF conversion warnings.
- Unity-MCP refresh/compile was requested. Unity console error query returned 3 existing missing-script reference errors with no file/line and 2 MCP client handler logs, not C# compile errors from this change.

### History

- 2026-05-07: User reported C delay/path/residual behavior issues and E `Ashen Sky` zones incorrectly grouping around the first target.
- 2026-05-07: Code Builder changed C impact from immediate explosion to delayed skill effect, changed `Piercing Trajectory` into travel-time path segments, added C residual-zone expiry handling, and changed E ash zones to use actual hit target positions.

# Source: boards/MON/VEGA_MONSTER.md

## Task: 2026-05-08 Manifested Vega Common Runtime Parity

### Task title

Apply Vega Offering choices through manifested projectile and common skill runtime.

### Goals

- Preserve manifested Vega A three-sword cadence.
- Apply Vega manifested Offering choices to damage, cooldown, magazine, reload, and name-mark projectile state.
- Keep Vega field/area skills on shared manifested SO-driven execution.

### Constraints

- Role Owner is Code Builder.
- This is common manifested runtime work, not a full line-by-line copy of selected Vega private skill code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Vega A and Offering-upgraded skills in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1073` applies Vega A damage and mark-stack choices inside the manifested queued projectile update.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:866` includes Vega skill-specific damage multipliers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:991` includes Vega cooldown choice handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:1250` and `:1278` include Vega A magazine/reload choice handling.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

## Required Sections For Future Work

- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Task: 2026-05-07 Vega Active Skills A-E Runtime Implementation

### Task title

Implement Vega active skills A-E from `Pakuri/reference/2.Monster/vega/skill`.

### Goals

- Implement A `?쇨??쒕Т` as a magazine skill that queues three piercing sword projectiles per shot and applies `?대쫫?쒖떇`.
- Implement B `移⑤У????쒕룄` as a line slash that applies physical damage and silence.
- Implement C `紐곗궡 ?덇?` as a self buff for Vega action speed and attack power.
- Implement D `寃? 紐낅? 媛쒕갑` as area slashes around all enemies with `?대쫫?쒖떇`.
- Implement E `理쒖쥌?좉퀬` as a single-target execute-style hit against the highest-mark enemy with mark-scaled damage and mark consumption.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega A-E behavior in Play Mode, especially A delayed three-shot cadence, B silence, D overlap behavior, and E mark consumption.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Source references read: `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md`, `b-silent-greatblade.md`, `c-extermination-permit.md`, `d-black-ledger-release.md`, and `e-final-sentence.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs` now contains `FireManualVegaThreeSwordFlurry(...)`, `TryCastVegaSilentGreatblade(...)`, `TryCastVegaExterminationPermit(...)`, `TryCastVegaBlackLedgerRelease(...)`, `TryCastVegaFinalSentence(...)`, `HandleVegaProjectileHit(...)`, and Vega mark/silence/damage helpers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores Vega enemy mark/silence state, Vega projectile mark stacks, and Vega delayed effect silence duration.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeEveSkills.cs`, and `CombatRuntimeEnemies.cs` now connect Vega cooldowns, action speed, projectile hit tracking, skill-effect ticks, status labels, and silence blocking.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/vega.asset` now mark Vega A-E as `RuntimeImplemented`; B/C/D/E are classified as `LineAttack`, `Buff`, `AreaAttack`, and `Execute`.
- Unity-MCP `execute_code` confirmed runtime catalog rows: `vega-a:MagazineProjectile:RuntimeImplemented`, `vega-b:LineAttack:RuntimeImplemented`, `vega-c:Buff:RuntimeImplemented`, `vega-d:AreaAttack:RuntimeImplemented`, `vega-e:Execute:RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP refresh/compile was requested; console error query returned existing missing-script reference errors and MCP client handler logs, not C# compile errors from this change.
- `git diff --check` completed with no whitespace errors; it reported only CRLF conversion warnings.

### History

- 2026-05-07: User requested implementation of Vega active skills A-E.
- 2026-05-07: Code Builder implemented Vega A-E runtime behavior, updated Vega data state, and completed local build/Unity-MCP validation.

## Task: 2026-05-07 Vega Passive Skills F-J Runtime Implementation

### Task title

Implement Vega passive skills F-J from `Pakuri/reference/2.Monster/vega/skill`.

### Goals

- Implement F `媛곸씤 ?ы솕` as mark-target damage amplification and physical defense reduction at 10+ name-mark stacks.
- Implement G `遊됱씤寃?? as silence-target damage amplification, B mark application, silence duration trait, and critical chance trait.
- Implement H `泥섑삎 以鍮? as Extermination Permit duration, action-speed, attack-power, and mark-target damage expansion.
- Implement I `?곗뇙 李멸껐` as Black Ledger area-damage vulnerability and D cooldown charge after area damage.
- Implement J `?ы삎 吏묓뻾?? as Final Sentence kill cooldown charge and survivor damage vulnerability.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega F-J behavior in Play Mode, especially B mark stacking, D area vulnerability, and E kill/survivor branches.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Source references read: `f-deep-engraving.md`, `g-sealing-sword-form.md`, `h-execution-prep.md`, `i-chain-cleaving.md`, and `j-executioner.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:725` adds B passive mark-stack resolution.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:798`, `:846`, and `:862` apply Vega passive damage, defense-reduction, and critical-chance modifiers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:922`, `:939`, and `:963` add I/J target vulnerability and J cooldown-charge helper paths.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:88` through `:91` add Vega I/J enemy timer/bonus state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:909` and `:914` display the new Vega vulnerability labels.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:48` through `:52` mark Vega F-J as `RuntimeImplemented`.
- `Pakuri/Assets/Data/GameData/Monsters/vega.asset:434`, `:470`, `:505`, `:540`, and `:579` mark Vega F-J as `ImplementationState: 2`.
- Unity-MCP `execute_code` synced the CSV runtime catalogs and returned `vega-f` through `vega-j` as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP console error query returned MCP client handler logs only, not C# compile errors from this change.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\CombatRuntimeVegaSkills.cs Pakuri\Assets\Scripts\Combat\CombatRuntimeController.cs Pakuri\Assets\Scripts\Combat\CombatRuntimeEnemies.cs Pakuri\Assets\CSVdata\source\monster_skills.csv Pakuri\Assets\Data\GameData\Monsters\vega.asset` completed with CRLF warnings only.

### History

- 2026-05-07: User requested Vega passive skill F-J implementation after the A-E active pass.
- 2026-05-07: Code Builder implemented the passive runtime behavior, updated Vega data state, synced Unity runtime catalog output, and completed local build/Unity-MCP validation.

## Task: 2026-05-07 Vega Projectile Sprite CSV Runtime Fix

### Task title

Fix Vega runtime projectile sprite source after SO-only sprite assignment was not reflected in Play Mode.

### Goals

- Make Vega runtime projectile visuals use the same sprite assigned in `Assets/Data/GameData/Monsters/vega.asset`.
- Keep the active CSV runtime source and generated runtime asset catalog aligned.

### Constraints

- Role Owner is Code Builder after Designer confirmed the data-source mismatch.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega projectile visuals in Play Mode.
- Keep future Vega unit/projectile sprite changes in `Pakuri/Assets/CSVdata/source/monsters.csv`, then sync the CSV runtime catalog.

### Evidence

- Unity-MCP `execute_code` first reported Vega SO projectile path `Assets/Image/Monster/Vega/Vega_Shoot2.png` while runtime path was `Assets/Image/Monster/Vega/Vega_Shoot_Temp.png`.
- `Pakuri/Assets/CSVdata/source/monsters.csv:7` now uses `Assets/Image/Monster/Vega/Vega_Shoot2.png` for Vega projectile sprite path.
- Unity-MCP `execute_code` imported `monsters.csv`, ran `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()`, and then reported runtime Vega projectile path `Assets/Image/Monster/Vega/Vega_Shoot2.png`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset:33` now contains `Assets/Image/Monster/Vega/Vega_Shoot2.png`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP console error query returned existing missing-script and MCP client-handler logs, not C# compile errors from this change.

### History

- 2026-05-07: User reported that the newly assigned Projectile Sprite in Vega SO was not applied and the old sprite still appeared.
- 2026-05-07: Code Builder aligned the CSV runtime sprite path and regenerated the runtime asset catalog.

## Task: 2026-05-07 Vega B Target Rectangle Correction

### Task title

Change Vega B from Vega-origin line damage to target-centered instant rectangle damage.

### Goals

- Stop B from drawing and damaging along a line from Vega to the enemy.
- Make B pick an enemy target and immediately apply area damage in a temporary 3 by 1 rectangle centered on that target.
- Preserve existing B damage, silence, name-mark, passive, and cooldown behavior where applicable.

### Constraints

- Role Owner is Code Builder after Designer handoff.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega B in Play Mode: no Vega-origin line, immediate rectangle effect on the target, and silence/name-mark application on enemies inside the 3 by 1 area.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before this correction, `CombatRuntimeVegaSkills.cs` `TryCastVegaSilentGreatblade()` called `ApplyVegaLineSlash(...)`, which used `CreateVegaLineVisual(...)` and `IsPointInsideBeam(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:12` and `:13` now define `VegaSilentGreatbladeAreaWidth = 3f` and `VegaSilentGreatbladeAreaHeight = 1f`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:207` now calls `ApplyVegaTargetRectangleSlash(...)` at the target enemy position.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:422` through `:441` applies immediate damage, silence, and name marks only to enemies inside the target-centered rectangle.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:595` creates the rectangular visual through `CreateVegaRectangleVisual(...)`, not from Vega's position.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:919` adds rectangle containment through `IsPointInsideVegaRectangle(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` removed the now-unused delayed Vega silence field from `SkillEffectRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP script refresh reached idle and console error query returned only MCP client-handler logs.

### History

- 2026-05-07: User reported Vega B should not connect a line from Vega to the enemy and requested an immediate temporary 3 by 1 rectangular effect on the enemy.
- 2026-05-07: Code Builder changed Vega B to target-centered rectangle damage and removed the leftover delayed line/silence state.

## Task: 2026-05-08 Manifested Vega A Three-Sword Follow-up

### Task title

Make Manifested Vega use `vega-a` as three sequential sword projectiles.

### Goals

- Preserve Vega A reference behavior when Vega is Manifested into the party.
- Fire three projectiles per A magazine shot.
- Apply the third projectile's 2x damage and Vega name-mark stacks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Vega A in RunScene Play Mode.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md` defines Vega A as three projectiles, 0.12 second bullet interval, third projectile 200% damage, magazine 5, and shot interval 0.55.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:728` detects Manifested `vega-a`.
- `CombatRuntimeParty.cs:747` through `:774` queues three Manifested Vega projectiles and applies 2x damage to the third projectile.
- `CombatRuntimeParty.cs:581`, `:625`, and `:627` carry and apply Vega name-mark stacks on Manifested projectile hits.
- Unity-MCP `execute_code` confirmed runtime catalog `vega-a` resolves as `MagazineProjectile`, magazine `5`, and shot interval `0.55`.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported Manifested Vega did not appear to reference Vega A's proper three-projectile basic attack.
- 2026-05-08: Code Builder added Vega-specific Manifested A projectile burst behavior.

## Task: 2026-05-08 Manifested Vega Name-Mark Guard

### Task title

Keep Manifested Vega A name marks while blocking non-Vega mark leakage.

### Goals

- Preserve Manifested Vega A's name-mark application from queued sword projectiles.
- Ensure the shared Manifested projectile path does not give non-Vega monsters Vega name marks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Vega A still applies marks, and non-Vega Manifested A attacks do not.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:554` passes 0 name-mark stacks for generic Manifested projectile fire.
- `CombatRuntimeParty.cs:617` gates stored `VegaNameMarkStacks` behind `IsManifestedVegaThreeSwordFlurry(skill)`.
- `CombatRuntimeParty.cs:1120` and `:1121` keep Manifested Vega A queued projectiles passing mark stacks.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User reported non-Vega Manifested A attacks appeared to leave Vega name marks.


