# 05-upgrade-ordering-app-surface: Upgrade ordering API and finalize application-tier migration

Upgrade Ordering.API as the final application-tier endpoint after its dependent app layers have been modernized. Ordering has one of the deepest internal dependency stacks and notable API/package risk signals.

This task upgrades Ordering.API to net10.0, applies compatible package updates, and resolves app-facing API incompatibilities so that all top-level applications in the solution are migrated before starting library consolidation.

**Done when**: Ordering.API builds and restores on net10.0, all top-level applications in the solution are migrated, and no app-tier project remains on net8.0.
