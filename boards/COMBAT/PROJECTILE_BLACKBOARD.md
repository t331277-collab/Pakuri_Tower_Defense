# PROJECTILE_BLACKBOARD

This is an active projectile-domain persistent state file created after new projectile runtime work resumed.
Older projectile history remains archived at `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`.

## Scope

- InGame projectile spawning, movement, hit relay, pierce, branch, fan-out, and projectile-owned hit effects.
- Monster-specific projectile facts should also be recorded in the relevant `boards/MON/{NAME}_MONSTER.md` file.

## Task: 2026-05-17 InGame Projectile Modifier Execution

### Task title

Extend shared InGame projectiles with modifier-driven fan-out, pierce, status, and branch behavior.

### Goals

- Keep projectile behavior reusable instead of hardcoding Eve-A inside the projectile actor.
- Allow `SkillExecutionSnapshot` modifier fields to affect active projectile behavior.
- Preserve existing enemy projectile calls by keeping the previous `InGameProjectileActor.Initialize(...)` overload.

### Constraints

- Role Owner is Code Builder.
- Branch projectiles do not recursively branch or apply inherited status in this slice.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies projectile fan-out, pierce, and branch visuals in Play Mode.
- If branch visuals need different assets later, add a data field instead of changing the branch actor to an Eve-specific path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:82` applies `AdditionalProjectileBonus` and `:87` resolves branch behavior from the active snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs:60` adds an extended initialize overload for optional status/branch specs while preserving the existing initialize overload used by enemy projectiles.
- `InGameProjectileActor.cs:153` applies status on hit, and `:154` calls branch spawning after damage.
- `InGameProjectileActor.cs:186` through `:307` finds nearby branch targets, spawns branch projectiles, prevents immediate re-hit of the source target, and disables recursive branch/status on branch projectiles.
- Runtime/editor builds completed with 0 errors and existing assembly reference warnings.
- Unity-MCP script refresh reached idle and console warning/error read returned only MCP client handler logs.

### History

- 2026-05-17: New active projectile work resumed for Eve-A step 2, so this board was created per `MDTREE.md`.
