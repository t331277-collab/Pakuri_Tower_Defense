## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-08` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/RUN/REWARD_BLACKBOARD.md`.

## Task: 2026-05-31 Offering Choice Labels And Active Skill Cap

### Task title

Make Offering reward choices identify their source skill and enforce active skill reward limits.

### Goals

- Show the source monster in each Offering choice card `Summary`.
- Show the source skill and choice title in each Offering choice card `SkillName`.
- Keep Offering active skill rewards capped at two non-default active skills beyond the default A/default active skill.

### Constraints

- Role Owner is Code Builder.
- Reward choice commit still goes through `RunSession.RecordOfferingChoice(...)`.
- No CSV row, column, or schema change was introduced.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile/editor validated.

### Next Actions

- User verifies in Play Mode that Offering reward cards no longer show only `아리엘 · 특성 1` style labels and that active skill choices stop appearing after two additional active skills.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now uses separate `Summary` and `SkillName` fields in `OfferingChoiceView`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves enhancement display names through linked skill ids and formats examples like `심판의 빛·특성 1`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now gates active skill candidate generation with `MaxAdditionalActiveSkillCount = 2` and excludes `IsDefaultLearned` or slot `A` active skills from the count.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- Unity-MCP `validate_script` for `Assets/Scripts2/InGame/UI/InGameUIManager.cs` reported 0 errors and the existing `Update()` string-concatenation GC warning.

### History

- 2026-05-31: User reported Offering enhancement choices were hard to identify because labels appeared as monster name plus generic trait number.
- 2026-05-31: Code Builder changed Offering reward label binding and active skill candidate gating.

## Task: 2026-05-31 Offering Skill Acquisition Runtime Sync

### Task title

Fix Offering active/passive skill acquisition so the selected runtime model and revived party models receive the learned skill state.

### Goals

- Preserve exact Offering skill choice ids for active/passive skill rewards.
- Make Offering commit refresh every scene-valid monster actor model from `RunSession`.
- Make next-day revive sync learned skill state before the actor is re-registered.

### Constraints

- Role Owner is Code Builder.
- Offering still records through `RunSession.RecordOfferingChoice(...)`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. Unity-MCP validator could not run because no Unity Editor instance was found.

### Next Actions

- User verifies active/passive skills obtained through Offering become usable and remain usable after day advance revive.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now sets `ChoiceId` on active skill Offering choices to `skill.SkillId` and on passive skill Offering choices to `passive.PassiveId`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now calls `RefreshSceneMonsterActorSkillModels(...)` after roster-player runtime refresh, syncing all scene-valid `MonsterUnitActor.Model` instances from `RunSession`.
- `Pakuri/Assets/Scripts2/InGame/Core/SceneEntryManager.cs` now calls `SyncExistingMonsterModelFromSession(model)` before `actor.ReviveForNextDay()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-31: User asked Code Builder to fix Offering skill acquisition together with the Nexus buff-target issue.
- 2026-05-31: Code Builder kept `RunSession` as the reward state authority and patched runtime synchronization paths that missed unregistered dead monster actors.
