## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-04-25` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/OPS/CODEX_CLI_BLACKBOARD.md`.

## Task: 2026-07-10 Codex CLI 0.144.1 Update And PATH Repair

### Task title

Update local Codex CLI and keep the `codex` command executable from PowerShell.

### Goals

- Install/update Codex CLI from the official installer path the user requested.
- Repair the stale npm `codex.cmd` path so `codex` works even in shells that still see the npm wrapper first.
- Set the default Codex model from `gpt-5.5` to `gpt-5.6`.

### Constraints

- Role Owner is Designer / OPS.
- This task changes local Codex CLI installation/configuration, not Unity gameplay code.
- Evidence must come from installer output, PATH/config inspection, and command output.

### Role Owner

Designer / OPS

### Status

Completed.

### Next Actions

- Open a new PowerShell window before starting a fresh manual Codex CLI session so the updated user PATH is loaded normally.
- If model availability still differs from the Codex app, check account/workspace model entitlement from inside the updated CLI using `/model`.

### Evidence

- Official installer command `powershell -ExecutionPolicy ByPass -c "irm https://chatgpt.com/codex/install.ps1 | iex"` completed with `Codex CLI 0.144.1 installed successfully`.
- Installer output reported `Detected existing npm-managed Codex at C:\Users\t3312\AppData\Roaming\npm\codex.cmd` and warned that PATH order could be ambiguous.
- `C:\Users\t3312\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe --version` returned `codex-cli 0.144.1`.
- Current-shell `codex --version` returned `codex-cli 0.144.1` after updating the stale npm wrapper.
- Simulated fresh PATH from Machine plus User environment resolved `C:\Users\t3312\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe` before `C:\Users\t3312\AppData\Roaming\npm\codex.cmd`, then returned `codex-cli 0.144.1`.
- `C:\Users\t3312\AppData\Roaming\npm\codex.cmd` was backed up to `C:\Users\t3312\AppData\Roaming\npm\codex.cmd.bak-20260710` and rewritten to call the OpenAI Codex bin `codex.exe` first.
- `C:\Users\t3312\.codex\config.toml` was backed up to `C:\Users\t3312\.codex\config.toml.bak-20260710`; current config now has `model = "gpt-5.6"` and `model_reasoning_effort = "high"`.

### History

- 2026-07-10: User asked to update Codex CLI and keep the command path executable after Codex app exposed GPT-5.6 while the CLI did not.
