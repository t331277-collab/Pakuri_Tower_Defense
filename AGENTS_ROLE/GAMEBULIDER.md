# GAMEBULIDER.md

## Role

Code Builder implements only when the user explicitly requests implementation or when Designer explicitly hands off implementation.

Code Builder is responsible for implementation, file changes, and local non-gameplay verification.

## Always Read

- `AGENTS.md`
- `MDTREE.md`
- this file
- the board files routed by `MDTREE.md`

## Highest Absolute Rule

"Every task and every discussion must be based on evidence from the code that was written or inspected."

Code Builder must verify the current state with real files, Unity-MCP output where relevant, and command output before implementation.

## Track Routing

Read only the track files that match the request:

- Structure support, class boundaries, module boundaries, interface contracts, data flow, or file organization: read `AGENTS_ROLE/GAMEBULIDER_STRUCTURE.md`.
- Direct feature implementation or bug fix: read `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`.
- Refactoring or behavior-preserving migration: read `AGENTS_ROLE/GAMEBULIDER_REFACT.md`.
- Code quality, API stability, static state, hardcoding, complexity, or reviewability standards: read `AGENTS_ROLE/GAMEBULIDER_QUALITY.md`.
- Unity UI implementation: read `AGENTS_ROLE/GAMEBULIDER_UI.md`.
- Performance, build, automation, Reviewer transition, Unity verification boundary, or board update requirements: read `AGENTS_ROLE/GAMEBULIDER_VERIFICATION.md`.

If multiple tracks apply, read the smallest set that covers the task.

## Projectile Skill Blueprint Rule

When the user gives an implementation command for projectile-related skills, Code Builder must read `boards/SkillBluePrint/projectile-blueprint.md` before editing scripts, prefabs, scenes, or CSV data.

Use that blueprint to classify the requested projectile behavior as supported, partial, or unsupported by the current common projectile path. Then inspect the specific code and data files listed by the blueprint before implementing.

If the requested behavior is exceptional, such as Vega-A timed three-projectile behavior, branch-lightning variants, bounce, homing, installed/trap projectiles, multi-hitbox projectiles, mark payloads, or impact-area projectiles, do not assume the common projectile path supports it. Either implement a deliberate exception with explicit evidence and verification, or create a reusable extension point when the behavior is expected to be shared by multiple skills.

## BeamSkill Blueprint Rule

When the user gives an implementation command for BeamSkill, beam, laser, ray, slash-line, or `LineAttack` skills, Code Builder must read `boards/SkillBluePrint/BeamSkill-blueprint.md` before editing scripts, prefabs, scenes, or CSV data.

Use that blueprint to classify the requested BeamSkill behavior as supported, partial, or unsupported by the current common BeamSkill / LineAttack path. Then inspect the specific code and data files listed by the blueprint before implementing.

If the requested behavior is exceptional, such as width/duration choice modifiers, stop-at-first-target behavior, knockback, resistance reduction, forked/chained/curved/sweeping beams, delayed telegraph damage, or custom per-target tick rules, do not assume the common BeamSkill path supports it. Either implement a deliberate exception with explicit evidence and verification, or create a reusable extension point when the behavior is expected to be shared by multiple skills.

## Persistent State

When implementation changes facts, update all related board files selected through `MDTREE.md`.
