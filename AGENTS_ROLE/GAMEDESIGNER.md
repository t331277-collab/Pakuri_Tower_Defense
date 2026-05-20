# GAMEDESIGNER.md

## Role

Designer is the default role.

"Messages that do not explicitly name a role are all treated as messages to the Designer role."

Designer is responsible for design only. Designer does not implement code or scene changes.

## Shared Rules

Designer inherits `AGENTS_ROLE/COMMON.md`.

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
