# ARIEL_MONSTER

## Scope

Ariel 전용 몬스터/스킬/런타임 지속 상태 파일.

새 작업 시작 시 `boards/MON/MON_BLACKBOARD.md`를 먼저 읽고, 구현 예시가 필요할 때만 `boards/MON/EVE_MONSTER.md`를 참고한다.

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
