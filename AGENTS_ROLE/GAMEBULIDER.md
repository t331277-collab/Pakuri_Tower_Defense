# GAMEBULIDER.md

## Role

Code Builder implements only when the user explicitly requests implementation or when Designer explicitly hands off implementation.

Code Builder is responsible for implementation, file changes, and local non-gameplay verification.

## Shared Rules

Code Builder inherits `AGENTS_ROLE/COMMON.md`.

Before implementation, Code Builder verifies the current state with real files, Unity-MCP output where relevant, and command output.

## Track Routing

Read only the track files that match the request:

- Structure support, class boundaries, module boundaries, interface contracts, data flow, or file organization: read `AGENTS_ROLE/GAMEBULIDER_STRUCTURE.md`.
- Direct feature implementation or bug fix: read `AGENTS_ROLE/GAMEBULIDER_IMPLEMENTATION.md`.
- Skill implementation, skill runtime wiring, skill prefab/effect connection, or user-invoked "Skill Builder" work: read `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- Refactoring or behavior-preserving migration: read `AGENTS_ROLE/GAMEBULIDER_REFACT.md`.
- Code quality, API stability, static state, hardcoding, complexity, or reviewability standards: read `AGENTS_ROLE/GAMEBULIDER_QUALITY.md`.
- Unity UI implementation: read `AGENTS_ROLE/GAMEBULIDER_UI.md`.
- Performance, build, automation, Reviewer transition, Unity verification boundary, or board update requirements: read `AGENTS_ROLE/GAMEBULIDER_VERIFICATION.md`.

If multiple tracks apply, read the smallest set that covers the task.

### Minimal Builder Read Set

For most Builder tasks, the mandatory markdown set is:
- `AGENTS_ROLE/GAMEBULIDER.md`
- one primary Builder track file
- one primary routed board when `MDTREE.md` requires it

Add more markdown files only under these conditions:
- Read a monster board only when the user names that monster or the inspected failure path names it.
- Read DATA boards only when the user or the inspected failure explicitly touches CSV, prefab, scene serialization, runtime catalog, or asset wiring.
- Read RUN boards only when the user or the inspected failure explicitly touches `RunSession`, Offering, Menifest, or `NewRunScene` flow ownership.
- Read `AGENTS_ROLE/GAMEBULIDER_UI.md` or UI boards only when the user or the inspected failure explicitly touches UI objects, canvases, buttons, TMP, UXML, USS, focus, or navigation.
- Read `AGENTS_ROLE/GAMEBULIDER_VERIFICATION.md` when build/editor/reviewer/automation constraints are part of the task, not by default for every small code edit.

Do not read a domain markdown file only because it might become relevant later.

### Routing Decision Log

Before reading additional markdown beyond the mandatory startup set, Builder should state a short routing decision in commentary.

Use this format or equivalent:
- request class
- markdown files to read next
- markdown files intentionally not read because that axis was not requested and is not named by the inspected failure path

## CSV Encoding Rule

When Code Builder creates or edits CSV files, store them as UTF-8 so text does not break across tools or sessions.

If the inspected target CSV is not valid UTF-8, convert it to UTF-8 while preserving the current data shape and field content before or during the requested CSV change.

If the inspected target CSV is already valid UTF-8, do not rewrite it only to change BOM style unless the user explicitly asks for that normalization.
