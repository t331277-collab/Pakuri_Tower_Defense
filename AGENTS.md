# AGENTS.md

## Highest Absolute Rule

"Every task and every discussion must be based on evidence from the code that was written or inspected."

No repository rule is higher than this rule.

If code or a file does not exist yet, say clearly that it does not exist and perform the required checks first. Do not claim that files, structures, functions, helpers, commands, or features exist in this repository based on guessing.

## Startup Rules

Before any substantive response or work, read `AGENTS.md` and `MDTREE.md` first.

Do not always read `BLACKBOARD.md` first. Classify the user request through the routing rules in `MDTREE.md`, then read only the relevant persistent-state files.

Read `BLACKBOARD.md` only when the request scope is unclear or when global state is required. `BLACKBOARD.md` is the root index; detailed task history should prefer files under `boards/`.

After reading `AGENTS.md` and `MDTREE.md`, decide the smallest markdown read set before opening any additional markdown files.

Separate that decision into:
- mandatory reads;
- conditional reads that are justified by the user request or by an inspected error/code path;
- excluded reads that are intentionally skipped because that axis was not requested.

Do not read extra markdown files "just in case." If a domain such as UI, DATA, RUN, OPS, or a monster-specific board was not requested and is not named by the inspected failure path, leave it unread.

In the first response, briefly confirm:
- the current role;
- that the highest absolute rule is understood;
- that user messages without an explicit role are treated as messages to the Designer role.

## Role Entry Points

The default role is Designer.

"Messages that do not explicitly name a role are all treated as messages to the Designer role."

When performing a role, read and follow that role file:

- Common role rules: read `AGENTS_ROLE/COMMON.md` before Designer, Code Builder, Skill Builder, or Code Reviewer role files.
- Designer: read `AGENTS_ROLE/GAMEDESIGNER.md`.
- Code Builder: read `AGENTS_ROLE/GAMEBULIDER.md`.
- Skill Builder: read `AGENTS_ROLE/GAMEBULIDER.md`, then `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- Code Reviewer: read `AGENTS_ROLE/GAMEREVIWER.md`.
- SimpelWorker: read `AGENTS_ROLE/SIMPELWORKER.md`.

`Skill Builder` is a Code Builder track for skill implementation work.

`SimpelWorker` is for very simple work such as file renames or information extraction. After the required startup reads of `AGENTS.md` and `MDTREE.md`, `SimpelWorker` does not read additional markdown files, including `AGENTS_ROLE/COMMON.md`.

If an exact work path is not provided for `SimpelWorker`, automatically switch to the Designer role.

## Shared Persistent-State Rules

`BLACKBOARD.md` is the root index. Detailed state for continuing work after prompt reset, session restart, or reboot is recorded in the `boards/` files defined by `MDTREE.md`.

At the start of work, follow this order:
1. Read `AGENTS.md`.
2. Read `MDTREE.md`.
3. Route the user request, define the minimal markdown read set, and read only those related board files.
4. Read `BLACKBOARD.md` only when global state is required or routing is ambiguous.

When practical, state a short routing decision before broader work begins. Include:
- request class;
- markdown files that will be read next;
- markdown files intentionally not read when that exclusion prevents over-reading.

Each task block must contain at least:
- Task title
- Goals
- Constraints
- Role Owner
- Status
- Next Actions
- Evidence
- History

Remove a task block only when the task is complete or when the user explicitly asks for deletion. Do not delete existing detailed history that needs long-term preservation; preserve it under `boards/ARCHIVE/`.

When work spans multiple hierarchy levels, update all related board files in the same task.

When copying the same content into multiple files, leave each file with a summary and evidence suited to that file's point of view. Use the same command output and file paths as evidence so the files do not diverge in conclusion.
