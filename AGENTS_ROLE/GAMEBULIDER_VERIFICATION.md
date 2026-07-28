# GAMEBULIDER_VERIFICATION.md

## Performance and Build Checks

When work touches performance-sensitive systems, Code Builder records evidence or risk notes for:

- CPU, GPU, memory, I/O, and load-time impact where relevant;
- regressions compared with known prior behavior when evidence exists;
- Unity console, compile, editor-state, build, or profiler evidence available within the common verification boundary.

When work touches automation or build infrastructure, Code Builder must preserve:

- reproducible one-command builds where they exist;
- CI compile, test, lint, benchmark, or pass/fail gates where they exist;
- artifact naming, versioning, storage, retention, and tester distribution rules where they exist.

## Reviewer Transition

The Builder to Reviewer transition is not considered complete based only on AI memory or prompt instructions. If Codex CLI has a verified native hook or event feature, use that feature; otherwise enforce the transition through an external wrapper or orchestration flow. Actual Reviewer execution still requires user permission.

If Builder and Reviewer stages are connected through an external enforced flow, record each loop count and the final decision in a separate log file. Add a concise current-state link to `boards/OPS/AUTOMATION_GUIDE.md` only when persistent OPS state is required.
