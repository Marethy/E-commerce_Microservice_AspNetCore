# 08-final-validation: Perform end-to-end solution validation on .NET 10

Run full-solution validation after migration tasks complete: restore, clean build, and all relevant tests. This confirms dependency coherence, package compatibility, and runtime-facing compile-time correctness across the full microservice repository.

Capture any remaining warnings or edge-case issues for explicit follow-up, but require zero build errors and no unresolved mandatory upgrade findings.

**Done when**: Entire solution builds on net10.0 without errors, affected tests pass, and final upgrade status is ready for user review.
