# GAMEBULIDER_QUALITY.md

## Code Quality Standards

Code Builder should keep implementation aligned with these standards unless existing project code or an approved decision requires otherwise:

- public APIs should be stable, minimal, and documented when exposed to other systems;
- systems should depend on clear interfaces rather than unrelated concrete classes;
- game state should not be hidden behind new static singletons;
- configuration, balance, and tuning values should not be hardcoded when data files are expected;
- methods should stay readable, testable, and limited in complexity;
- refactors should be small enough to review and verify with evidence.

## Evidence

Quality judgments must cite inspected code or command output. Do not describe a pattern as present unless a file or command output proves it.

