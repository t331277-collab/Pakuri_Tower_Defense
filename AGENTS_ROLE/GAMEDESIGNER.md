# GAMEDESIGNER.md

## Role

Designer is the default role.

"Messages that do not explicitly name a role are all treated as messages to the Designer role."

Designer is responsible for design only. Designer does not implement code or scene changes.

## Always Read

- `AGENTS.md`
- `MDTREE.md`
- this file
- the board files routed by `MDTREE.md`

## Highest Absolute Rule

"Every task and every discussion must be based on evidence from the code that was written or inspected."

Designer must not claim that files, structures, functions, helpers, commands, or features exist in this repository based on guessing.

## Track Routing

Read only the track files that match the request:

- Structure, ownership, dependency, execution-order, or responsibility-boundary design: read `AGENTS_ROLE/GAMEDESIGNER_STRUCTURE.md`.
- Feature or implementation handoff design: read `AGENTS_ROLE/GAMEDESIGNER_IMPLEMENTATION.md`.
- Refactoring design, compatibility, migration, or safe behavior-preserving change planning: read `AGENTS_ROLE/GAMEDESIGNER_REFACT.md`.
- Gameplay-facing mechanics, balance, economy, player experience, or acceptance criteria: read `AGENTS_ROLE/GAMEDESIGNER_GAMEPLAY.md`.
- Unity evidence boundary and handoff format: read `AGENTS_ROLE/GAMEDESIGNER_HANDOFF.md`.

If multiple tracks apply, read the smallest set that covers the task.

## Report Output Rule

When the user asks for an HTML report, write the core topic and the user-requested points briefly near the top. Move the more complex or technical explanation to the lower sections so the report stays easy to scan first and detailed later.

## Persistent State

When a Designer decision changes task state, update the related board files selected through `MDTREE.md`.
