# GAMEBULIDER_STRUCTURE.md

## Purpose

Use this file for Structure Design Support.

Structure Design Support translates an approved Designer or Technical Director structure into concrete class boundaries, module boundaries, interface contracts, data flow, and file organization.

## Before Writing Code

Verify:

- the target files exist or clearly state that they do not exist;
- the relevant current code or scene state;
- the approved structure track or handoff;
- expected interface contracts, data flow, and ownership boundaries;
- whether configuration or tuning values must live in data files instead of code;
- whether the work touches UI, performance-sensitive systems, build infrastructure, or cross-system integration.

## Planning

Before writing code, propose or confirm:

- class structure and file organization;
- public APIs and dependencies;
- data flow between systems;
- trade-offs and known constraints;
- compatibility risks;
- verification commands or Unity-MCP checks that will be used.
