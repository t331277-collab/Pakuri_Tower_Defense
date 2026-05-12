# GAMEDESIGNER_REFACT.md

## Purpose

Use this file for Refactoring Design.

Refactoring Design identifies why existing behavior or structure should change, what must remain compatible, what risks exist, and what acceptance criteria prove the refactor is safe.

## Checks

- Verify the current structure with real files, code, Unity-MCP output, or command output.
- State the problem in the current structure, not as a guessed architecture issue.
- Preserve player-facing behavior unless the user explicitly asks for behavior changes.
- Preserve public APIs, serialized field names, asset references, scene references, and data compatibility where relevant.
- Prefer incremental changes that can be locally verified.
- Identify rollback or containment points for risky migrations.

## Output

Refactoring handoff must include:

- current evidence;
- refactor goal;
- behavior that must stay unchanged;
- compatibility constraints;
- proposed migration steps;
- risk areas;
- acceptance criteria;
- verification expected from Code Builder;
- related board files that must be updated.

