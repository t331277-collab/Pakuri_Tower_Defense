## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/` on 2026-05-12.
- This file keeps only task blocks dated `2026-04-25` based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/OPS/CODEX_CLI_BLACKBOARD.md`.

## Task: 2026-07-19 Codex CLI 0.144.6 Update

### Task title

Update local Codex CLI from 0.144.5 to 0.144.6.

### Goals

- Install Codex CLI 0.144.6 through the official installer supplied by the user.
- Verify the active `codex` command and official installed executable report the target version.
- Preserve the existing repository launcher files.

### Constraints

- Role Owner is Designer / OPS.
- This task changes the user-local Codex CLI installation, not gameplay code.
- Evidence comes from installer and command output.

### Role Owner

Designer / OPS

### Status

Completed.

### Next Actions

- Start the next CLI session normally so the new 0.144.6 process and helper files are loaded together.

### Evidence

- Official installer reported `Updating Codex CLI from 0.144.5 to 0.144.6` and `Codex CLI 0.144.6 installed successfully`.
- `Get-Command codex` resolved `C:\Users\t3312\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe`.
- `codex --version` and the official installed executable both returned `codex-cli 0.144.6`.
- `run_codex.bat`, `run_codex_prompt_launcher.ps1`, and `codex_prompt.txt` all still exist.
- The OpenAI Codex manual helper refreshed the local official manual cache before installation.

### History

- 2026-07-19: User requested the advertised Codex CLI 0.144.6 update; the official installer and version checks completed successfully.

## Task: 2026-07-17 Codex CLI 0.144.5 Update

### Task title

Update local Codex CLI from 0.144.4 to 0.144.5.

### Goals

- Install Codex CLI 0.144.5 through the official installer supplied by the user.
- Verify the active codex command and official installed executable report the target version.
- Confirm run_codex.bat still resolves and launches the official executable.

### Constraints

- Role Owner is Designer / OPS.
- This task changes the user-local Codex CLI installation, not gameplay code.
- Evidence comes from inspected launcher files and command output.

### Role Owner

Designer / OPS

### Status

Completed.

### Next Actions

- Start the next CLI session through run_codex.bat so the new process loads the 0.144.5 helper files.

### Evidence

- Official installer reported update from 0.144.4 to 0.144.5 and successful installation.
- codex --version and the official installed executable both returned codex-cli 0.144.5.
- Get-Command codex resolved C:\Users\t3312\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe before the legacy npm wrapper.
- run_codex.bat resolves Get-Command codex first, validates its version, and passes that path to run_codex_prompt_launcher.ps1.
- codex_prompt.txt and run_codex_prompt_launcher.ps1 both existed during verification.
- run_codex_prompt_launcher.ps1 accepts the resolved executable path, validates it, and starts it with the repository root and UTF-8 prompt.
- The already-running pre-update session could not refresh codex-windows-sandbox-setup.exe after installation; a new session is required for updated helper loading.

### History

- 2026-07-17: User requested Codex CLI update to 0.144.5 and confirmed run_codex.bat must keep launching the CLI.

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
