# GAMEBULIDER_VERIFICATION.md

## Performance and Build Checks

When work touches performance-sensitive systems, Code Builder records evidence or risk notes for:

- CPU, GPU, memory, I/O, and load-time impact where relevant;
- regressions compared with known prior behavior when evidence exists;
- Unity console, compile, editor-state, build, or profiler evidence available without running Play Mode.

When work touches automation or build infrastructure, Code Builder must preserve:

- reproducible one-command builds where they exist;
- CI compile, test, lint, benchmark, or pass/fail gates where they exist;
- artifact naming, versioning, storage, retention, and tester distribution rules where they exist.

## Reviewer Transition

After logic work, Code Reviewer review runs only when the user explicitly permits it.

Without permission, Reviewer execution is deferred, and Code Builder records only Codex-performed verification evidence such as build, compile, console, or file checks.

When Reviewer is executed, run it only once. If a problem is found, report it to the user and wait for the next instruction.

The Builder to Reviewer transition is not considered complete based only on AI memory or prompt instructions. If Codex CLI has a verified native hook or event feature, use that feature; otherwise enforce the transition through an external wrapper or orchestration flow. Actual Reviewer execution still requires user permission.

If Builder and Reviewer stages are connected through an external enforced flow, record each loop count and the final decision in `boards/OPS/REVIEWER_BLACKBOARD.md` or a separate log file. Add only links to the root `BLACKBOARD.md` when needed.

## Unity Verification Boundary

Do not directly run Play Mode to verify gameplay. Play Mode verification belongs to the user.

Codex records evidence only up to build, compile, console, and editor-state checks.

Use Unity-MCP for Unity project checks. This project does not use MSW-MCP.

## Evidence Rules

All implementation judgments must be based on real files, code, and command output.

If a command cannot be executed or a file cannot be read, state that fact first.

Do not assume this is a Git repository. Use Git-based review only when command output confirms that Git is available and the current folder is a Git work tree. If Git-based review is unavailable, explicitly collect the changed file list or review through a Reviewer-only `codex exec` flow.

## Board Updates

When work spans multiple hierarchy levels, update all related board files in the same task.

Examples:

- Eve skill implementation: update `boards/MON/MON_BLACKBOARD.md`, `boards/MON/EVE_MONSTER.md`, and, if needed, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md` or `boards/COMBAT/PROJECTILE_BLACKBOARD.md`.
- DebugScene UI fix: update `boards/UI/DEBUGSCENE_UI.md`; if it relates to monster tests, also update `boards/MON/MON_BLACKBOARD.md`; if it is Eve-specific, also update `boards/MON/EVE_MONSTER.md`.
- Run reward fix: update `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, and, if UI is involved, `boards/UI/RUNSCENE_UI.md`.
- Reviewer/wrapper/automation fix: update `boards/OPS/REVIEWER_BLACKBOARD.md` and, if needed, `boards/OPS/CODEX_CLI_BLACKBOARD.md` or `boards/OPS/AUTOMATION_GUIDE.md`.

When the user explicitly says a task is finished, or when Code Builder believes a task no longer needs to stay in active board context, Code Builder should briefly ask whether to move that task block to `boards/ARCHIVE/`.

