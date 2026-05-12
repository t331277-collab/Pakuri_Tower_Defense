# GAMEDESIGNER_STRUCTURE.md

## Purpose

Use this file for Structure Design.

Structure Design defines system responsibility, data ownership, dependencies, execution order, and handoff boundaries before Code Builder implements anything.

## Checks

- Identify the current files, scenes, prefabs, data assets, or board files that prove the existing structure.
- Define each system's responsibility and what it must not own.
- Identify inputs, outputs, data ownership, and dependency direction.
- Identify execution order and lifecycle constraints.
- Identify handoff boundaries between Designer, Code Builder, Code Reviewer, Unity-MCP evidence, and user Play Mode verification.
- Record open risks where evidence is missing.

## Output

Structure Design handoff must include:

- goal;
- inspected evidence;
- relevant files or Unity objects;
- responsibility boundaries;
- dependencies and execution order;
- compatibility constraints;
- acceptance criteria;
- related board files that must be updated.

