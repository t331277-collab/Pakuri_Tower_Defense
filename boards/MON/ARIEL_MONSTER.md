# ARIEL_MONSTER

## Scope

Ariel 전용 몬스터/스킬/런타임 지속 상태 파일.

새 작업 시작 시 `boards/MON/MON_BLACKBOARD.md`를 먼저 읽고, 구현 예시가 필요할 때만 `boards/MON/EVE_MONSTER.md`를 참고한다.

## Task: 2026-05-03 Ariel J Passive Runtime Correction

### Task title

Correct Ariel passive J `성역 선포` runtime to match the current E/J reference documents.

### Source references

- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md`
- `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md`

### Skill slots A-J

- F `ariel-f` / 빛의 인도: unchanged in this pass.
- G `ariel-g` / 수호 교리: unchanged in this pass.
- H `ariel-h` / 축복 전파: unchanged in this pass.
- I `ariel-i` / 낙인 계시: unchanged in this pass.
- J `ariel-j` / 성역 선포: corrected so post-E action speed uses its own 5-second timer and holy-damage bonus depends on remaining Archangel shield state.

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

- `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md:18-19` requires `대천사의 강림` post-cast action speed for 5 seconds and holy-damage bonus while the E shield remains.
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

- A `ariel-a` / 심판의 빛: held/click direction manual Holy projectile, magazine/reload, pierce, last-shot explosion master, Holy Exposure master.
- B `ariel-b` / 성광 방패: selected-unit shield, shield duration/cooldown, shield burst, Holy damage buff, reflection, shield scaling.
- C `ariel-c` / 축복의 파동: nearest-enemy area Holy damage, blessing timer, action-speed or spell-power master, second wave master.
- D `ariel-d` / 천상의 낙인: strongest-enemy Holy damage, Holy Exposure, multi-target trait, crit-damage and detonation masters.
- E `ariel-e` / 대천사의 강림: battlefield-wide Holy damage, selected-unit shield, Holy Exposure target bonus, sanctuary damage reduction, post-cast blessing.
- F `ariel-f` / 빛의 인도: Holy damage bonus, A magazine trait, Holy crit chance trait.
- G `ariel-g` / 수호 교리: shield amount bonus, battle-start shield, shielded Holy damage trait.
- H `ariel-h` / 축복 전파: blessing Holy damage, cooldown charge speed, blessing duration trait.
- I `ariel-i` / 낙인 계시: Holy Exposure target damage bonus, D cooldown trait, Holy resistance flat reduction trait.
- J `ariel-j` / 성역 선포: E post-cast action speed, shielded Holy damage, E cooldown trait.

### Runtime implementation status

Implemented in `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` and integrated into:

- `CombatRuntimeController.cs`
- `CombatRuntimeProjectiles.cs`
- `CombatRuntimeEnemies.cs`
- `Pakuri/Assembly-CSharp.csproj`

Current runtime has one selected player Monster, not an ally party collection. Document phrases such as "모든 아군" are implemented against the selected Monster because that is the only allied combat unit present in the current code.

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

Make Ariel A master `백색 심판` explode on hit and use the base circle sprite visual.

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

- User verifies `백색 심판` in Play Mode with `ariel-a-master-1` selected.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md` defines `백색 심판` as the last projectile exploding twice with area Holy damage.
- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` contains `MasterSkillChoices` entry `ChoiceId: ariel-a-master-1`, Title `백색 심판`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` keeps the explosion damage/count on the projectile and now returns whether `TryTriggerArielJudgementLightExplosion()` fired.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now triggers the Ariel judgement explosion immediately on enemy hit when the projectile has pending explosion data, then cleans up the projectile.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` uses `CreateCircleEffect()` / `GetCircleSprite()` for the explosion visual, with longer duration and higher sorting order.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the existing Unity/MCP warnings.
- Unity refresh/compile completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User reported Ariel A master `백색 심판` did not appear to apply and requested using the base circle asset if only the visual effect was missing.
- 2026-04-30: Code Builder changed the master explosion to fire immediately on hit and made the circle-sprite explosion more visible.
