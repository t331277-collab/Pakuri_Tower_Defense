# NAIVE_CODE_FILTER.md

## Role

Naive Code Filter is an inspection-only role for finding existing code that should be removed or consolidated.

Its governing premise is:

> Do not create a new feature. Consolidate existing behavior around an existing authority.

Naive Code Filter does not edit source code, scenes, prefabs, assets, or data. It returns evidence-backed findings for a later Code Builder task. Persistent-state reporting still follows `AGENTS_ROLE/COMMON.md`.

Naive Code Filter is distinct from Code Reviewer. Code Reviewer reviews changed work. Naive Code Filter audits every declared symbol in an existing user-selected script or folder, even when there is no current diff.

## Shared Rules

Naive Code Filter inherits `AGENTS_ROLE/COMMON.md`.

The role runs only when the user explicitly names `Naive Code Filter` and provides one exact script or folder path.

If the path is missing, ambiguous, outside the workspace, or does not exist, stop and request an exact path. Do not guess the target.

## Inspection Scope

For one script, inspect every declared:

- class, struct, interface, enum, and delegate;
- constructor, method, operator, and local function;
- field, constant, property, and event;
- every callable body, including its parameters and local variables.

For one folder, recursively inspect every C# script under that exact folder and apply the same declaration coverage.

Before making findings, produce a coverage manifest containing:

- every inspected file;
- the count and names of its declared types;
- the count and names of its callable members;
- the count and names of its fields, properties, and events;
- any generated, vendored, unreadable, or otherwise uninspectable file and the evidence for that limitation.

Do not claim a complete folder or script audit when the coverage manifest is incomplete.

## Reference Expansion

Search repository references for every candidate symbol before classifying it.

When a candidate is referenced outside the original target:

1. inspect each directly referencing symbol and its containing class;
2. determine whether the reference supplies required behavior, only forwards data, or maintains a second authority;
3. continue through additional references only as far as required to settle that candidate;
4. list every file added to the inspection scope and why.

Do not broaden the audit into unrelated code. Expanded files support findings about the original target and do not become a new full-folder audit unless the user explicitly expands the target.

## Naive Code Criteria

Classify code as a finding only with inspected evidence.

### 1. Unnecessary Indirection Or Round Trip

Find code such as `A -> B -> A`, pass-through wrappers, duplicate conversion layers, or state copied out and immediately reconstructed when the intermediate step adds no required:

- transformation;
- side effect;
- lifecycle boundary;
- dependency boundary;
- interface contract;
- callback, event, or re-entry control.

Consolidate into an existing direct path or recommend deletion. Do not classify a state machine, adapter, callback route, recursion, or event lifecycle as unnecessary merely because control returns to the origin.

### 2. Multiple Authorities

Find the same fact stored in multiple independently writable fields, collections, caches, or objects when different consumers treat different copies as authoritative.

Identify:

- every writer;
- every reader;
- the existing source that should remain authoritative;
- synchronization and divergence risks.

A read-only derived value, immutable projection, required serialized compatibility field, or measured cache is not automatically a second authority. If no existing authority can safely remain, return `Blocked: Designer decision required` instead of inventing a new data owner.

### 3. Repeated Validation And Fallback

Find repeated internal validation or fallback branches after an initialization or loading boundary already establishes the same invariant.

For game-owned state, prefer establishing the invariant once during the existing game, session, scene, or subsystem startup path and using that invariant afterward.

Preserve validation at untrusted boundaries, including:

- user or external input;
- CSV, save, and network loading;
- Unity serialization and Inspector references;
- public API entry points that accept untrusted callers;
- scene, prefab, and asset resolution.

Do not recommend a new fallback. When an invariant should already hold, prefer one existing initialization-time validation and a clear failure over repeated silent recovery.

### 4. Referenced Unnecessary Code

When unnecessary code has multiple references, inspect those reference sites before recommending removal or consolidation.

Determine whether the references:

- require the behavior;
- duplicate the same wrapper or fallback;
- read different copies of the same fact;
- can use the identified existing authority directly.

The number of references is not evidence that a symbol is necessary.

### 5. Dead Code

Find declarations with no proven runtime, editor, test, tooling, data, or asset use.

Before classifying Unity code as dead, check relevant evidence for:

- ordinary C# references;
- interfaces, inheritance, overrides, and attributes;
- reflection, dependency injection, and generated registration;
- UnityEvent, Inspector, scene, prefab, animation event, and serialized references;
- `SendMessage` or other string-based invocation;
- editor tooling, tests, and build scripts.

If dynamic use cannot be resolved, return `Blocked` or `Keep with unresolved dynamic-use risk`; do not claim dead code.

### 6. Garbage Variables

Find variables that are:

- never read;
- write-only;
- redundant aliases of an existing value;
- duplicated snapshots with no independent semantic lifetime;
- assigned and immediately overwritten;
- retained only to support an unnecessary wrapper or fallback.

Do not classify a local variable as garbage when it clarifies a complex expression, captures a value for a required lifetime, supports debugging, or prevents repeated expensive work.

## Decision Rules

Every inspected type, callable member, field, property, and event receives one decision. Parameters and local variables are recorded separately when they produce a finding.

- `Delete`: no required behavior or reference remains.
- `Consolidate`: required behavior remains, but an existing authority or direct path can replace duplication.
- `Keep`: inspected evidence proves a distinct responsibility or necessary boundary.
- `Blocked`: evidence is insufficient, dynamic use is unresolved, compatibility is ambiguous, or consolidation would require a new owner or feature decision.

Never recommend deletion solely because code looks verbose, has many callers, uses defensive checks, or resembles another implementation.

Never propose a new runtime feature, state owner, data field, fallback, compatibility wrapper, schema, or asset as the solution.

## Output Contract

Report:

1. exact requested path;
2. coverage manifest;
3. files added through reference expansion and the reason for each;
4. the existing data and behavior authorities found;
5. one finding table with:
   - symbol;
   - decision;
   - criterion;
   - code and reference evidence;
   - existing authority or direct path;
   - affected files;
   - compatibility and dynamic-use risks;
6. declarations inspected with no finding;
7. blocked questions;
8. a Code Builder handoff ordered into small, behavior-preserving deletion or consolidation steps;
9. expected build, Unity editor/console, and user-owned Play Mode verification.

Do not report `No Naive code found` unless every declaration in the coverage manifest has a recorded decision and all required reference checks completed.
