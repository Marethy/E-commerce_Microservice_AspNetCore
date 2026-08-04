# Upgrade Options — microservice.sln

Assessment: 23 projects on net8.0, 9 incompatible packages, 66 binary + 52 source API incompatibilities, deep dependency graph (8 levels).

## Strategy

### Upgrade Strategy
Given 23 projects and a deep dependency graph, an incremental approach is safer and keeps the solution buildable throughout migration.

| Value | Description |
|-------|-------------|
| **Top-Down** (selected) | Upgrade entry-point applications first, then consolidate shared libraries after all apps are moved. |
| All-at-Once | Upgrade all projects in one atomic pass; fastest, but likely causes temporary solution-wide breakage. |

## Compatibility

### Unsupported Packages
The assessment reports 9 incompatible NuGet packages, which is above inline-research threshold and better handled with staged resolution.

| Value | Description |
|-------|-------------|
| Resolve Inline | Research and resolve each incompatible package within the same task with no deferred work. |
| **Defer Resolution** (selected) | Keep projects compiling with temporary stubs/conditioning first, then resolve package replacements in follow-up subtasks. |
| Compatibility Mode | Keep .NET Framework references with compatibility shims; use only for narrow transitive-only cases. |

### Unsupported API Handling
API incompatibilities are broad, but for modern-to-modern upgrade the default is to fix directly within each task to avoid technical debt.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve API changes in the same task, including complex replacements where needed. |
| Defer Complex Changes | Apply simple fixes now and defer complex replacements using temporary stubs and follow-up subtasks. |

## Modernization

### Logging Framework
Common.Logging is present and is not a strong long-term fit for modern .NET hosting patterns.

| Value | Description |
|-------|-------------|
| **Migrate to Microsoft.Extensions.Logging** (selected) | Replace legacy logging abstraction with built-in Extensions.Logging providers and patterns. |
| Keep Existing Logging Framework | Retain current framework with adapter packages to minimize immediate code changes. |
