# Git Commit

Strict conventional commit enforcement. Every commit MUST follow these rules.

## Rules

1. **Format**: `<type>: <description>` — type MUST be one of the allowed types below.
2. **Length**: Description MUST be under 10 words.
3. **No attribution**: NEVER add `Co-Authored-By`, `Signed-off-by`, or any Claude/AI attribution.
4. **Language**: English only.

## Allowed Types

| Type | Usage |
|------|-------|
| `feat` | New feature |
| `fix` | Bug fix |
| `refactor` | Code restructuring without behavior change |
| `docs` | Documentation only |
| `test` | Adding or updating tests |
| `chore` | Build, CI, dependencies, tooling |
| `perf` | Performance improvement |
| `style` | Formatting, whitespace (no logic change) |
| `ci` | CI/CD pipeline changes |

## Examples

```
feat: add JWT authentication
fix: resolve null reference in payment service
refactor: extract order validation to domain service
docs: update API documentation for v2
test: add saga integration tests
chore: update NuGet packages to 10.0.9
perf: optimize product query with index
ci: add .NET 10 build pipeline
```

## Enforcement

A git `commit-msg` hook at `.githooks/commit-msg` validates every commit.
Install with: `git config core.hooksPath .githooks`
