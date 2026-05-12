## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-05-10` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/COMBAT/PROJECTILE_BLACKBOARD.md`.

## Task: 2026-05-10 Ariel Unit Projectile Runtime

### Task title

Route Manifested Ariel projectile hits and White Judgement explosion through Ariel unit logic.

### Goals

- Resolve Manifested Ariel A projectile damage through Ariel unit passive/choice helpers.
- Apply Ariel A master Holy Exposure and White Judgement explosion using the manifested source unit.
- Preserve selected Ariel projectile behavior.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode projectile verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies Manifested Ariel A projectile damage, pierce, Holy Exposure, and White Judgement explosion in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1062` checks `TryApplyArielUnitProjectileHit(...)` during Manifested projectile hit resolution.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:1300` implements Ariel unit projectile hit damage and A master Holy Exposure.
- `CombatRuntimeArielSkills.cs:991` and `:1026` route pending Ariel judgement explosions through manifested source-unit damage when the projectile is manifested.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:65` and `:77` trigger pending Ariel judgement explosions for manifested projectile hit cleanup and X-edge cleanup.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: Ariel unit executor migration added projectile-specific unit hooks for Manifested Ariel.

## Task: 2026-05-10 Vega Unit Projectile Runtime

### Task title

Route Manifested Vega projectile hits through Vega unit projectile logic.

### Goals

- Keep Manifested Vega A as projectile objects with three-sword cadence.
- Resolve Manifested Vega projectile damage through the source `CombatUnitRuntime` and F-J passive state.
- Preserve Vega name-mark source guarding while adding A master afterimage and unit buff mark bonuses.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode projectile verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP checks.

### Next Actions

- User verifies Manifested Vega A projectile damage, mark stacks, afterimage, and kill mark-transfer behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:1054` checks `TryApplyVegaUnitProjectileHit(...)` during Manifested projectile hit resolution.
- `CombatRuntimeParty.cs:1547` queues a fourth Manifested Vega A shot when `vega-a-master-1` is present.
- `CombatRuntimeParty.cs:1569` uses `GetVegaUnitThreeSwordDamageMultiplier(...)` for unit-owned Vega A projectile damage.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:1068` implements unit-owned Vega projectile hit damage with passive final damage, flat defense reduction, and critical chance.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-10: Vega unit executor migration added projectile-specific unit hooks for Manifested Vega.
