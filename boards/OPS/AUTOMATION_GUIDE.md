# AUTOMATION_GUIDE

## Task: 2026-08-26 Pakuri Architecture Script Tree

### Task title

Document the current `Pakuri/Assets/Scripts` architecture on the Notion page `아키텍쳐 구조`.

### Goals

- Render the current script architecture as an aligned `│` / `├──` / `└──` tree.
- Add a concise right-side responsibility note for every displayed structural area.
- Keep all represented paths and responsibilities grounded in current source evidence.

### Constraints

- Replace content only on the initially empty user-selected Notion page.
- Do not create, delete, or move Notion child pages or databases.
- Do not modify project code, Unity scenes, prefabs, data, or assets.

### Role Owner

Designer

### Status

Completed and re-fetched for verification.

### Next Actions

- None.

### Evidence

- Notion workspace search identified page ID `3c874254-7899-8097-b675-e5bc476fddee` with title `아키텍쳐 구조`; its fetched content contained only an empty block before editing.
- `rg --files Pakuri/Assets/Scripts -g "*.cs"` confirmed the displayed Combat, GameFlow, Loading, UI, and Units paths and their nested folders.
- Inspected representative current scripts: `Combat/InGameCombatManager.cs`, `GameFlow/RunSession.cs`, `Loading/GameDataLoader.cs`, `UI/InGame/InGameUIManager.cs`, `UI/MainMenu/MainMenuUIManager.cs`, and `Units/Model/UnitCombatState.cs`.
- Notion `replace_content` succeeded, and the post-update fetch confirmed the `├── Combat/`, `│   ├── Skills/`, and `└── Units/` markers plus right-side responsibility text.

### History

- 2026-08-26: User requested the current script structure on the architecture page in the same symbol-based tree style as the Markdown structure, with short descriptions on the right.
- 2026-08-26: Designer added a Notion code-block architecture tree organized by Combat, GameFlow, Loading, UI, and Units.

## Task: 2026-08-26 Pakuri Defense Notion Markdown Structure

### Task title

Reformat the `md 구조` section on the Pakuri Defense Notion page as a readable Notion directory tree.

### Goals

- Preserve the page's surrounding narrative and downstream sections.
- Replace the malformed ASCII directory tree with a complete, aligned Notion code-block tree.
- Keep the shown Markdown paths and responsibilities aligned with the inspected repository.

### Constraints

- Change only the `md 구조` section of the user-selected Notion page.
- Do not create, delete, or move Notion child pages or databases.
- Do not modify project code, Unity scenes, prefabs, data, or assets.

### Role Owner

Designer

### Status

Completed and re-fetched for verification.

### Next Actions

- None.

### Evidence

- Fetched Notion page `파쿠리 디펜스` at `https://app.notion.com/p/3c874254789980b584f3efa20de89e40` before editing; its `md 구조` toggle contained a single malformed ASCII tree with broken line grouping.
- `rg --files -g "*.md"` confirmed the root Markdown files, role documents, active boards, and archive paths represented in the replacement.
- Notion `update_content` succeeded for page ID `3c874254-7899-80b5-84f3-efa20de89e40`.
- Post-update fetch confirmed a code-block tree containing `│`, `├──`, and `└──` markers for the root documents, role documents, boards, and archives; it also preserved `Part 2`, `Part 3`, and later page sections.

### History

- 2026-08-26: User requested that the Markdown structure beneath the Pakuri Defense Notion page be made more suitable for Notion.
- 2026-08-26: Designer initially replaced only that section's content with readable groupings, then followed the user's correction by rendering the same verified paths as an aligned `│` / `├──` / `└──` directory tree in a Notion code block.

## Task: 2026-08-26 Global Notion MCP Connection

### Task title

Register and authenticate the hosted Notion MCP server for global Codex use.

### Goals

- Add the official Notion Streamable HTTP MCP endpoint to the global Codex configuration.
- Complete the required Notion OAuth authorization.
- Verify the configured transport, endpoint, enabled state, and authentication mode.

### Constraints

- Do not change project code, Unity scenes, prefabs, data, or assets.
- Use the official hosted endpoint `https://mcp.notion.com/mcp`.
- Notion OAuth workspace selection and permission approval remain user-owned.
- A Codex restart or new task is required before the newly configured MCP tools appear in the task tool list.

### Role Owner

Designer

### Status

Completed. Registered, OAuth-authenticated, CLI-verified, loaded in the restarted Codex task, and read-only page discovery/search verified.

### Next Actions

- None for connection verification. Continue to obtain explicit user intent before any Notion write operation.

### Evidence

- `codex mcp add notion --url https://mcp.notion.com/mcp` reported `Added global MCP server 'notion'.`.
- `codex mcp login notion` reported `Successfully logged in to MCP server 'notion'.` after the user completed OAuth authorization.
- `codex mcp get notion` reports `enabled: true`, `transport: streamable_http`, and URL `https://mcp.notion.com/mcp`.
- `codex mcp list` reports `notion` as `enabled` with `OAuth` authentication.
- The restarted task's tool inventory contains 28 Notion tools, including page discovery, search, fetch, and write operations.
- Read-only discovery calls succeeded for private, shared, recent, and favorite pages: 25 private results, 1 shared result, 39 recent results, and 0 favorite results.
- Deduplicating those four discovery lists produced 39 observed items: 35 pages and 4 databases.
- A read-only workspace search for `파쿠리 디펜스` succeeded and returned the exact `파쿠리 디펜스` page plus related accessible content.

### History

- 2026-08-26: User requested registration of the official Notion MCP server and guidance at the OAuth approval step.
- 2026-08-26: Designer registered the global server, kept the OAuth callback listener active, and guided the user through authorization.
- 2026-08-26: The in-app browser displayed a local callback connection error after authorization, while the waiting Codex process confirmed successful login; CLI inspection then verified the enabled OAuth configuration.
- 2026-08-26: In a restarted task, Designer confirmed 28 loaded Notion tools, successfully listed accessible private/shared/recent/favorite content, and verified workspace search with `파쿠리 디펜스`.

## Task: 2026-07-31 Codex CLI 0.146.0 Update

### Task title

Update the global Codex CLI from 0.145.0 to 0.146.0.

### Goals

- Install Codex CLI 0.146.0 with the official update command.
- Verify the active CLI version.

### Constraints

- Do not change project code or Unity assets.
- Use actual installer and version output as evidence.

### Role Owner

Designer

### Status

Completed and verified.

### Next Actions

- None.

### Evidence

- Official installer output: `Codex CLI 0.146.0 installed successfully.`
- `codex --version` output: `codex-cli 0.146.0`.

### History

- 2026-07-31: Updated Codex CLI from 0.145.0 to 0.146.0 and verified the installed version.

## Archived History

The pre-cleanup OPS automation, Codex CLI, Reviewer, and Unity-MCP boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/OPS/`.

## Task: 2026-07-28 Active Board And Skill Builder Archive Cleanup

### Task title

Deactivate Skill Builder and archive obsolete active-board content.

### Goals

- Preserve Skill Builder as an inactive named role.
- Remove all active routing to its former blueprints.
- Move `boards/SkillBluePrint` intact under `boards/ARCHIVE/`.
- Keep only the current Trigger/Node design task active.
- Preserve completed, superseded, empty-shell, and old COMBAT, DATA, MON, OPS, RUN, and UI records under a dated archive snapshot.
- Align `AGENTS.md`, `MDTREE.md`, role files, and root status with the resulting paths.

### Constraints

- Role Owner is Code Builder.
- No C#, CSV, scene, prefab, Unity asset, or gameplay behavior changes are part of this task.
- No historical record is deleted.
- The 2026-07-28 Trigger/Node handoff and its COMBAT/DATA task blocks remain active.
- Archived Skill Builder blueprints are historical evidence only and cannot authorize implementation.

### Role Owner

Code Builder

### Status

Implemented and structurally validated.

### Next Actions

- Use only the active routes in `MDTREE.md` for future work.
- Read the dated snapshots only when historical evidence is explicitly required.
- Reactivate Skill Builder only through a new explicit user policy request.

### Evidence

- Pre-move audit found 22 Markdown files in the six requested domain folders excluding the current Trigger/Node handoff.
- Task-status inspection found the retained 2026-07-28 COMBAT/DATA design task as the only current unimplemented cross-domain work.
- `Pakuri/Assets/Scripts` contains 70 C# files: Combat 31, Data 5, GameFlow 11, InGame 1, UI 8, and Units 14.
- The move command preserved the 22 domain files under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/`.
- The seven former blueprint files moved intact to `boards/ARCHIVE/SkillBluePrint/`.
- The former root index moved intact to `boards/ARCHIVE/BLACKBOARD_2026-07-28_PRE_COMPACTION.md`.
- Hash comparison checked 26 unchanged moved files against their Git `HEAD` blobs with 0 missing files and 0 mismatches.
- The archived root `BLACKBOARD.md` hash exactly matches its Git `HEAD` blob.
- All 29 concrete paths parsed from `MDTREE.md` exist.
- Active policy and board files contain 0 references to the removed active blueprint path and 0 references to the removed active domain-board paths.
- Three active `## Task:` blocks and the standalone Trigger/Node handoff contain all eight required sections.
- Strict UTF-8 decoding passed for 44 policy, active-board, and moved archive files with 0 errors.
- `git diff --check` passed; Git emitted only working-copy LF-to-CRLF notices.
- No C#, CSV, scene, prefab, or Unity asset file changed.

### History

- 2026-07-28: User explicitly selected Code Builder, deactivated Skill Builder while preserving the role, requested the whole SkillBluePrint archive move, and requested old or structure-incompatible content removal from the six domain folders.
- 2026-07-28: Code Builder inspected role policy, all target-board task statuses, standalone design documents, current script inventory, and exact move targets before changing files.
- 2026-07-28: Code Builder completed the archive move, rebuilt the minimal active routes, and passed path, hash, task-schema, UTF-8, and Git diff validation.

## Task: 2026-07-29 Global Ponytail Codex Plugin Installation

### Task title

Install and activate the upstream Ponytail plugin for global Codex use.

### Goals

- Register the upstream Ponytail marketplace globally.
- Install and enable `ponytail@ponytail`.
- Confirm the installed version, Node runtime, bundled skills, and lifecycle hooks.
- Give the user the exact activation and usage commands.

### Constraints

- Use the inspected upstream repository and installed plugin files as evidence.
- Do not modify project C#, CSV, scene, prefab, or Unity assets.
- Lifecycle hooks require user review and trust through `/hooks`.
- A newly installed plugin requires a new Codex thread or application restart before its skills and hooks are loaded.

### Role Owner

Designer

### Status

Plugin installed and enabled. User-owned restart and hook trust remain.

### Next Actions

- User restarts Codex or opens a new thread.
- User opens `/hooks`, reviews the Ponytail lifecycle hooks, and trusts them.
- User invokes `@ponytail-help` or uses the automatically active default `full` mode.

### Evidence

- `codex plugin marketplace add DietrichGebert/ponytail` added marketplace `ponytail` from the upstream Git repository.
- `codex plugin add ponytail@ponytail` installed the plugin under `C:\Users\t3312\.codex\plugins\cache\ponytail\ponytail\4.8.4`.
- `codex plugin list` reports `ponytail@ponytail` as `installed, enabled`, version `4.8.4`.
- `node --version` reports `v24.14.0`.
- The installed manifest points to `./skills/` and `./hooks/claude-codex-hooks.json`.
- The installed help skill lists Codex invocations `@ponytail`, `@ponytail-review`, and `@ponytail-help`; the skills directory also contains audit, debt, and gain skills.
- The inspected hook manifest registers `SessionStart`, `SubagentStart`, and `UserPromptSubmit` Node commands.

### History

- 2026-07-29: User approved global Ponytail installation and requested usage instructions.
- 2026-07-29: Designer registered the marketplace, installed and enabled Ponytail 4.8.4, and verified the local runtime and installed files.

## Task: 2026-07-30 Global Script Comment Normalization

### Task title

Replace stale comments under `Pakuri/Assets/Scripts` with complete Korean responsibility summaries.

### Goals

- Remove every pre-existing source comment from the 73 current C# scripts.
- Add a concise Korean role and responsibility header at the top of every script.
- Add a concise Korean responsibility summary to every type, method, constructor, and local function declaration.
- Preserve all non-comment code and runtime behavior.

### Constraints

- Role Owner is Code Builder.
- Scope is limited to `Pakuri/Assets/Scripts/**/*.cs` plus this persistent-state record.
- Comments must describe responsibilities found in the inspected current source.
- Public APIs, serialized fields, source tokens, preprocessor directives, scenes, prefabs, CSV data, and Unity assets must not change.
- Unity Play Mode remains user-owned.

### Role Owner

Code Builder

### Status

Completed and verified.

### Next Actions

- Use the new file and declaration summaries as the current structural documentation.
- Update the matching summary whenever a script or declaration responsibility changes.
- Run user-owned Play Mode gameplay verification only if behavioral work is later added.

### Evidence

- Initial inventory found 73 scripts and 3,773 lines containing legacy comment syntax under `Pakuri/Assets/Scripts`.
- Roslyn inspection with `UNITY_EDITOR` enabled found 244 type declarations, 1,195 method or constructor declarations, and 1 local function.
- Final structural audit found 73/73 file headers and 1,440/1,440 documented declarations, with 0 missing declarations and 0 unexpected legacy or inline comments.
- Korean localization audit found Korean text in 73/73 file headers and 1,440/1,440 declaration summaries, with 0 missing Korean summaries.
- A Roslyn token and preprocessor comparison against Git `HEAD` found 0 non-comment code mismatches across all 73 scripts.
- The Korean comment conversion compared every file before and after localization and found 0 non-comment code-token or preprocessor mismatches.
- `dotnet build Assembly-CSharp.csproj --no-restore --verbosity quiet` passed with 0 errors and the existing 2 assembly-reference warnings.
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --verbosity quiet` passed with 0 errors and the existing 2 assembly-reference warnings.
- Unity script refresh and compilation completed with the editor idle and 0 error console entries.
- Unity EditMode test job `f129a3b2a83947de9be624252c6666b1` passed 9/9 tests with 0 failures and 0 skips.
- After Korean localization, Unity EditMode test job `0c77612c4dc643bcaf92b7722801eafb` passed 9/9 tests with 0 failures and 0 skips.
- The temporary Roslyn audit utility and its generated build output were removed after verification.

### History

- 2026-07-30: User explicitly selected Code Builder and requested complete replacement of all comments under `Pakuri/Assets/Scripts`.
- 2026-07-30: Code Builder inspected the current file, type, method, constructor, local-function, comment, and build baseline before editing.
- 2026-07-30: Code Builder replaced legacy comments with file-level role/responsibility headers and declaration-level summaries without changing non-comment source tokens.
- 2026-07-30: Code Builder completed static coverage, Git token comparison, runtime/editor builds, Unity compilation, console inspection, and the full EditMode test suite.
- 2026-07-30: User requested Korean comments without damaging their meaning; Code Builder localized all file and declaration summaries while preserving code identifiers and responsibility scope.
- 2026-07-30: Code Builder revalidated Korean coverage, non-comment token identity, runtime/editor builds, Unity compilation, the error console, and the full EditMode test suite.
