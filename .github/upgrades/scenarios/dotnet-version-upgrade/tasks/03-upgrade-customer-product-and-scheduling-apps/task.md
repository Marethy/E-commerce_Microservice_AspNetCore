# 03-upgrade-customer-product-and-scheduling-apps: Upgrade customer-facing and scheduled processing services

Upgrade Customer.API, Product.API, and Hangfire.API to net10.0. These applications combine API compatibility issues with package deprecations/recommendations and are independently deployable, making them a suitable second app wave.

This task addresses app-level breaking API changes inline and applies package updates required for net10 compatibility, while recording any deferred compatibility work under the selected unsupported package handling approach.

**Done when**: Customer.API, Product.API, and Hangfire.API build on net10.0 with required package updates applied and no unresolved compile-time blockers.
