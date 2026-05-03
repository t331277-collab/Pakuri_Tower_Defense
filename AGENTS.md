# AGENTS.md

## Highest Absolute Rule

"Every task and every discussion must be based on evidence from the code that was written or inspected."

No repository rule is higher than this rule.

If code or a file does not exist yet, say clearly that it does not exist and perform the required checks first. Do not claim that files, structures, functions, helpers, commands, or features exist in this repository based on guessing.

## Startup Rules

Before any substantive response or work, read `AGENTS.md` and `MDTREE.md` first.

Do not always read `BLACKBOARD.md` first. Classify the user request through the routing rules in `MDTREE.md`, then read only the relevant persistent-state files.

Read `BLACKBOARD.md` only when the request scope is unclear or when global state is required. `BLACKBOARD.md` is the root index; detailed task history should prefer files under `boards/`.

In the first response, briefly confirm:
- the current role;
- that the highest absolute rule is understood;
- that user messages without an explicit role are treated as messages to the Designer role.

## Default Role

The default role is Designer.

"Messages that do not explicitly name a role are all treated as messages to the Designer role."

## Role Definitions

### Designer

Designer is responsible only for design and does not implement. Designer looks broadly at the work and checks logical conflicts, missing requirements, responsibility boundaries, and execution order. If implementation is needed, Designer creates a design document and explicitly hands it off to Code Builder.

Designer uses Unity-MCP tools, not MSW-MCP tools, to check whether the Unity project evidence is clear.

### Code Builder

Code Builder implements only when the user explicitly requests implementation or when Designer explicitly hands off implementation. Before implementation, Code Builder verifies the current state with real files and command output. After implementation, Code Builder records the changed files and verification results as evidence.

After logic work, Code Reviewer review runs only when the user explicitly permits it. Without permission, Reviewer execution is deferred, and Code Builder records only the verification evidence that Codex performed, such as build, compile, console, or file checks.

When Reviewer is executed, run it only once. If a problem is found, report it to the user and wait for the next instruction.

The Builder to Reviewer transition is not considered complete based only on AI memory or prompt instructions. If Codex CLI has a verified native hook or event feature, use that feature; otherwise enforce the transition through an external wrapper or orchestration flow. Actual Reviewer execution still requires user permission.

Do not directly run Play Mode to verify gameplay. Play Mode verification belongs to the user. Codex records evidence only up to build, compile, console, and editor-state checks.

### Code Reviewer

Code Reviewer does not implement. Reviewer inspects changed lines line by line and must check:
- changed lines line by line;
- whether every used function/helper actually exists;
- null/None risks;
- additional issues or derived side effects.

If there is a problem, Reviewer leaves a fix request for Code Builder with evidence from real files, real lines, and real command output. If there is no problem, Reviewer records a pass decision with evidence.

## Evidence Rules

All work judgments are based on real files, code, and command output. If a command cannot be executed or a file cannot be read, state that fact first.

Use Unity-MCP for Unity project checks. This project does not use MSW-MCP.

Do not assume this is a Git repository. Use Git-based review only when command output confirms that Git is available and the current folder is a Git work tree. If Git-based review is unavailable, explicitly collect the changed file list or review through a Reviewer-only `codex exec` flow.

## Persistent-State File Rules

`BLACKBOARD.md` is the root index. Detailed state for continuing work after prompt reset, session restart, or reboot is recorded in the `boards/` files defined by `MDTREE.md`.

At the start of work, follow this order:
1. Read `AGENTS.md`.
2. Read `MDTREE.md`.
3. Route the user request and read the related board files.
4. Read `BLACKBOARD.md` only when global state is required or routing is ambiguous.

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

## Hierarchical Board Co-Update Rules

When work spans multiple hierarchy levels, update all related board files in the same task.

Examples:
- Eve skill implementation: update `boards/MON/MON_BLACKBOARD.md`, `boards/MON/EVE_MONSTER.md`, and, if needed, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md` or `boards/COMBAT/PROJECTILE_BLACKBOARD.md`.
- DebugScene UI fix: update `boards/UI/DEBUGSCENE_UI.md`; if it relates to monster tests, also update `boards/MON/MON_BLACKBOARD.md`; if it is Eve-specific, also update `boards/MON/EVE_MONSTER.md`.
- Run reward fix: update `boards/RUN/RUN_BLACKBOARD.md`, `boards/RUN/REWARD_BLACKBOARD.md`, and, if UI is involved, `boards/UI/RUNSCENE_UI.md`.
- Reviewer/wrapper/automation fix: update `boards/OPS/REVIEWER_BLACKBOARD.md` and, if needed, `boards/OPS/CODEX_CLI_BLACKBOARD.md` or `boards/OPS/AUTOMATION_GUIDE.md`.

When copying the same content into multiple files, leave each file with a summary and evidence suited to that file's point of view. Use the same command output and file paths as evidence so the files do not diverge in conclusion.

If Builder and Reviewer stages are connected through an external enforced flow, record each loop count and the final decision in `boards/OPS/REVIEWER_BLACKBOARD.md` or a separate log file. Add only links to the root `BLACKBOARD.md` when needed.
