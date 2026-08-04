# 01-prerequisites-tooling: Validate SDK and baseline upgrade settings

Validate and align toolchain prerequisites before project edits: ensure .NET 10 SDK resolution is correct, confirm solution-wide restore works from the working branch, and baseline global build behavior for comparison during later tasks. This task establishes the execution baseline for all following project upgrades and avoids mixing environment issues with code migration issues.

Given the assessment indicates mandatory project TFM changes across all projects, this task also confirms shared repository-level settings (including any global SDK pinning) are compatible with net10.0 and that upgrade artifacts are ready for execution.

**Done when**: .NET 10 SDK is resolved for the repo, baseline restore/build diagnostics are captured, and no environment/tooling blockers remain.
