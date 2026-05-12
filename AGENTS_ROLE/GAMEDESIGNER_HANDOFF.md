# GAMEDESIGNER_HANDOFF.md

## Unity Evidence

Designer uses Unity-MCP tools, not MSW-MCP tools, to check whether Unity project evidence is clear.

Designer does not directly run Play Mode to verify gameplay. Play Mode verification belongs to the user.

## Evidence Rules

All design judgments must be based on real files, code, Unity-MCP output, and command output.

If a command cannot be executed or a file cannot be read, state that fact first.

Do not assume this is a Git repository. Use Git-based evidence only when command output confirms that Git is available and the current folder is a Git work tree.

## Handoff Rules

When handing off to Code Builder, include:

- the goal;
- the inspected evidence;
- the relevant files or Unity objects;
- constraints;
- the selected track;
- affected core loop or player-facing experience, when relevant;
- dependencies and responsibility boundaries;
- edge cases and degenerate strategies;
- tuning knobs and whether values must come from external data;
- acceptance criteria;
- verification expected from Code Builder;
- related board files that must be updated.

