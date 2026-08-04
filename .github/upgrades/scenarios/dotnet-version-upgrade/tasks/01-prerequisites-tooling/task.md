# 01-prerequisites-tooling: Validate SDK and baseline upgrade settings

Validate and align toolchain prerequisites before project edits: ensure .NET 10 SDK resolution is correct, confirm solution-wide restore works from the working branch, and baseline global build behavior for comparison during later tasks. This task establishes the execution baseline for all following project upgrades and avoids mixing environment issues with code migration issues.

Given the assessment indicates mandatory project TFM changes across all projects, this task also confirms shared repository-level settings (including any global SDK pinning) are compatible with net10.0 and that upgrade artifacts are ready for execution.

**Done when**: .NET 10 SDK is resolved for the repo, baseline restore/build diagnostics are captured, and no environment/tooling blockers remain.

## Research Findings

- `validate_dotnet_sdk_installation(net10.0)` returned **Compatible SDK found**.
- `validate_dotnet_sdk_in_globaljson(net10.0)` returned **Success: no changes were needed in global.json**.
- Baseline full-solution validation executed via `run_build` and returned **Build successful**.
- Current upgrade branch for scenario execution is `upgrade-dotnet-10` with initialization artifacts present under `.github/upgrades/scenarios/dotnet-version-upgrade/`.

## Validation Against Done-When

- **.NET 10 SDK resolved for repo**: Verified by SDK validation tool.
- **Baseline restore/build diagnostics captured**: Verified by successful solution build baseline.
- **No environment/tooling blockers**: No SDK/global.json/build blockers detected.
