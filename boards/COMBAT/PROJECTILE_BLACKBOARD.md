# PROJECTILE_BLACKBOARD

This is an active projectile-domain persistent state file created after new projectile runtime work resumed.
Older projectile history remains archived at `boards/ARCHIVE/PROJECTILE_BLACKBOARD_ARCHIVE_2026-05-14.md`.

## Scope

- InGame projectile spawning, movement, hit relay, pierce, branch, fan-out, and projectile-owned hit effects.
- Monster-specific projectile facts should also be recorded in the relevant `boards/MON/{NAME}_MONSTER.md` file.

## Task: 2026-05-27 Shared Projectile Delayed-Impact And Timed Follow-Up Runtime

### Task title

Extend the shared projectile runtime to support contact-stop delayed impacts plus on-hit and on-expire follow-up effects.

### Goals

- Let projectile skills stop on first contact, wait a configured delay, then resolve a delayed area impact.
- Let projectile skills run shared `monster_skill_effects.csv` `OnHit` and `OnExpire` rows without monster-specific executor branches.
- Preserve existing direct-hit fallback behavior for simple projectiles that do not need delayed impact or timed effects.

### Constraints

- Role Owner is Code Builder.
- This is a shared projectile/runtime extension, not a Sein-only branch.
- The new behavior is only exercised when projectile/effect data asks for delayed impact or timed effects.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- Reuse the delayed-impact path for future projectile skills before adding another projectile state machine.
- Keep projectile visuals scene-owned through `EffectManager` when a flying visual is distinct from the delayed impact visual.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` now stores delayed-impact state (`stopAfterFirstHit`, `impactDelaySeconds`, `impactEffectPrefab`, `hasImpactArea`, `onHitEffects`, and `onExpireEffects`) and resolves contact-stop delayed impacts through the shared actor.
- The same file now defers destroy-boundary cleanup while armed, executes `OnHit` follow-up effects on contact, resolves delayed explosion damage through `InGameZoneSkillActor.ApplyAreaTick(...)`, and waits for spawned impact visual lifetime before `OnExpire` follow-up effects.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now resolves projectile timed effects from `SkillMultiEffectTiming.OnHit` and `OnExpire`, prefers explicit projectile prefab then scene `EffectManager` mapping for the flying visual, and creates a projectile actor instead of using direct-hit fallback when delayed-impact behavior exists.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Utilities/SkillVisualSpawnUtility.cs` now exposes shared animation-length visual lifetime resolution used by projectile delayed-impact follow-up cleanup.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDataValidator.cs` now allows `CooldownProjectile` rows that rely on cooldown timing instead of magazine/reload fields while still validating projectile speed.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.

### History

- 2026-05-27: Sein-C required a shared projectile path for “flying arrow -> first-contact stop -> delayed explosion -> optional residual zone,” so the projectile actor and executor were extended instead of adding a Sein-only implementation.
