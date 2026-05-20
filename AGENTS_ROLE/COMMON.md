# COMMON.md

## Scope

These shared rules apply to Designer, Code Builder, Skill Builder, and Code Reviewer.

`SimpelWorker` is intentionally excluded so it can remain a minimal path-based role.

## Inherited Authority

`AGENTS.md` remains the authority for startup order, the highest evidence rule, role entry points, and persistent-state rules.

`MDTREE.md` remains the authority for markdown routing, minimal read-set reduction, active board selection, and cross-domain board update examples.

Role and track files should add only role-specific or track-specific rules instead of repeating those root rules.

## Evidence And Failure Rules

All claims must be grounded in inspected files, code, Unity-MCP output, or command output.

If a file cannot be read or a command cannot be executed, state that fact before drawing conclusions.

Do not claim that a file, function, helper, scene object, asset, command, or feature exists unless inspected evidence proves it.

## Unity And Play Mode Boundary

Unity Play Mode gameplay verification belongs to the user.

Codex records non-gameplay evidence such as file checks, compile/build results, Unity editor state, Unity console output, and scene or asset inspection.

Use Unity-MCP for Unity project checks. This project does not use MSW-MCP.

## Git And Reviewer Boundary

Do not assume Git is available. Use Git-based evidence only after command output confirms the current folder is a Git work tree.

Code Reviewer execution requires explicit user permission.

When Reviewer is executed after Builder work, run it only once unless the user explicitly asks for another review pass.

## Board Update Boundary

When work changes persistent task facts, update the board files selected through `MDTREE.md`.

When work crosses multiple routed domains, update every related active board in the same task.

When a completed or no-longer-needed task block can leave active context, ask the user before moving it to `boards/ARCHIVE/`.
