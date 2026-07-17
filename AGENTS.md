# AGENTS.md

## Highest Absolute Rule

"Every task and every discussion must be based on evidence from the code that was written or inspected."

No repository rule is higher than this rule.

If code or a file does not exist yet, say clearly that it does not exist and perform the required checks first. Do not claim that files, structures, functions, helpers, commands, or features exist in this repository based on guessing.

## Skill Builder Absolute Boundary

`Skill Builder` owns new Base skill authoring only when an existing Projectile, Buff, SingleAttack, LineAttack, AreaAttack, or Passive runtime/schema can express the Reference. It also owns Enhancement and Master node authoring for an existing or newly authored Base skill.

The exact user-provided skill Reference MD is the only semantic input. Builder derives ids and values only through `AGENTS_ROLE/GAMEBULIDER_SKILL.md` and the selected blueprint; it does not require separate parsed input.

Default authority is limited to:

- exactly one family Base blueprint for Base work;
- `boards/SkillBluePrint/enhancement-master-node-blueprint.md` for Enhancement/Master work;
- the exact user-provided skill Reference MD;
- the selected family's minimum Base, Choice, graph, and trigger CSV rows allowed by the selected blueprint;
- matching node-definition rows only when the selected graph requires them;
- the uniquely matching row in `Pakuri/Assets/CSVdata/authoring/status/status_effects.csv` only when a Reference status label needs an id.

Before reading any CSV, Builder must name its exact path and why it is required. Read and edit only the new Base skill, requested Choices, and their explicitly required Skill/Choice/Trigger-owned graph rows.

Do not read another Base blueprint, another Reference, linked Obsidian documentation, MON/DATA/RUN/UI/OPS/archive markdown, old implementations, broad runtime code, unrelated CSV, prefabs, scenes, or asset folders by default.

Skill Builder must stop when work requires a new runtime behavior, node type, handler, parameter, CSV file or column, shared code, prefab/scene change, asset creation, or a value missing or ambiguous in the provided Reference. Stop instead of widening scope or inventing an id, numeric value, policy, or asset path.

## Startup Rules

Before any substantive response or work, read `AGENTS.md` and `MDTREE.md` first.

When reading markdown or other text documentation files, use `Get-Content -Raw -Encoding UTF8` by default so the inspected evidence preserves UTF-8 text correctly.

For this project, shell commands that directly read files, inspect files, or write files inside the intended workspace are treated as normal and expected workflow commands.

When practical, prefer stable UTF-8-safe file read patterns for text documents so inspected evidence preserves UTF-8 text correctly.

Do not always read `BLACKBOARD.md` first. Classify the user request through the routing rules in `MDTREE.md`, then read only the relevant persistent-state files.

Read `BLACKBOARD.md` only when the request scope is unclear or when global state is required. `BLACKBOARD.md` is the root index; detailed task history should prefer files under `boards/`.

After reading `AGENTS.md` and `MDTREE.md`, decide the smallest markdown read set before opening any additional markdown files.

Separate that decision into:
- mandatory reads;
- conditional reads that are justified by the user request or by an inspected error/code path;
- excluded reads that are intentionally skipped because that axis was not requested.

Do not read extra markdown files "just in case." If a domain such as UI, DATA, RUN, OPS, or a monster-specific board was not requested and is not named by the inspected failure path, leave it unread.

In the first response, briefly confirm:
- the current role;
- that the highest absolute rule is understood;
- that user messages without an explicit role are treated as messages to the Designer role.

## Role Entry Points

The default role is Designer.

"Messages that do not explicitly name a role are all treated as messages to the Designer role."

When performing a role, read and follow that role file:

- Common role rules: read `AGENTS_ROLE/COMMON.md` before Designer, Code Builder, Skill Builder, or Code Reviewer role files.
- Designer: read `AGENTS_ROLE/GAMEDESIGNER.md`.
- Code Builder: read `AGENTS_ROLE/GAMEBULIDER.md`.
- Skill Builder: read `AGENTS_ROLE/GAMEBULIDER.md`, then `AGENTS_ROLE/GAMEBULIDER_SKILL.md`.
- Code Reviewer: read `AGENTS_ROLE/GAMEREVIWER.md`.
- SimpelWorker: read `AGENTS_ROLE/SIMPELWORKER.md`.

`Skill Builder` is the Code Builder track for Reference-driven Base authoring on an existing runtime/schema and Enhancement/Master node authoring.

`SimpelWorker` is for very simple work such as file renames or information extraction. After the required startup reads of `AGENTS.md` and `MDTREE.md`, `SimpelWorker` does not read additional markdown files, including `AGENTS_ROLE/COMMON.md`.

If an exact work path is not provided for `SimpelWorker`, automatically switch to the Designer role.

## Shared Persistent-State Rules

`BLACKBOARD.md` is the root index. Detailed state for continuing work after prompt reset, session restart, or reboot is recorded in the `boards/` files defined by `MDTREE.md`.

At the start of work, follow this order:
1. Read `AGENTS.md`.
2. Read `MDTREE.md`.
3. Route the user request, define the minimal markdown read set, and read only those related board files.
4. Read `BLACKBOARD.md` only when global state is required or routing is ambiguous.

When practical, state a short routing decision before broader work begins. Include:
- request class;
- markdown files that will be read next;
- markdown files intentionally not read when that exclusion prevents over-reading.

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

When work spans multiple hierarchy levels, update all related board files in the same task.

When copying the same content into multiple files, leave each file with a summary and evidence suited to that file's point of view. Use the same command output and file paths as evidence so the files do not diverge in conclusion.
