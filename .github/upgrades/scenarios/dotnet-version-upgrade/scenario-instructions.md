# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: main
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: Top-Down

### Compatibility
- Unsupported Packages: Defer Resolution (9 incompatible packages)
- Unsupported API Handling: Fix Inline

### Modernization
- Logging Framework: Migrate to Microsoft.Extensions.Logging

## Strategy
**Selected**: Top-Down (Application-First)
**Rationale**: Large modern-.NET solution (23 projects) with deep dependency graph and high API/package incompatibility counts requires incremental application-first rollout.

### Execution Constraints
- Upgrade application entry points before shared library consolidation.
- Keep compatibility fixes inline per task; only defer package resolution where required by selected option.
- Start shared library consolidation only after all top-level apps are upgraded.
- Run build/test validation gates after each completed task before moving forward.
- Maintain commit cadence after each task unless user changes commit strategy.

## Build Tool Decisions
- **microservice.sln**: dotnet build (SDK-style modern .NET solution baseline validated)
