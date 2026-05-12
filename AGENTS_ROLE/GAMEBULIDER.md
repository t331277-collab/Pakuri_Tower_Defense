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

## Persistent State

When implementation changes facts, update all related board files selected through `MDTREE.md`.
