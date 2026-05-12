# GAMEBULIDER_REFACT.md

## Purpose

Use this file for refactoring.

Refactoring changes existing code in safe incremental steps while preserving behavior, public APIs, serialized data compatibility, and testability.

## Rules

- Verify the current code before editing.
- Preserve player-facing behavior unless the user explicitly approves behavior changes.
- Preserve public APIs, serialized field names, Unity asset references, scene references, and saved data compatibility where relevant.
- Keep refactors small enough to review and verify with evidence.
- Avoid unrelated cleanup.
- Prefer mechanical moves or extraction steps that can be validated separately.

## Evidence

After refactoring, record:

- changed files;
- behavior that should be unchanged;
- compatibility constraints checked;
- verification commands and results;
- Unity-MCP evidence where relevant;
- remaining risks or user-only Play Mode verification needs.

