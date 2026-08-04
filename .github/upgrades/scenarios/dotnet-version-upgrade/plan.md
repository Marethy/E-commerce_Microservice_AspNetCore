# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade the microservice solution from .NET 8 to .NET 10.
**Scope**: Large solution (23 projects) with deep inter-project dependencies, incompatible packages, and API compatibility issues.

### Selected Strategy
**Top-Down (Application-First)** — Applications upgraded first, libraries multi-targeted temporarily.
**Rationale**: The solution has 23 projects, an 8-level dependency graph, and broad API/package incompatibilities; incremental app-first execution reduces blast radius and keeps progress observable.

## Tasks

### 01-prerequisites-tooling: Validate SDK and baseline upgrade settings

Validate and align toolchain prerequisites before project edits: ensure .NET 10 SDK resolution is correct, confirm solution-wide restore works from the working branch, and baseline global build behavior for comparison during later tasks. This task establishes the execution baseline for all following project upgrades and avoids mixing environment issues with code migration issues.

Given the assessment indicates mandatory project TFM changes across all projects, this task also confirms shared repository-level settings (including any global SDK pinning) are compatible with net10.0 and that upgrade artifacts are ready for execution.

**Done when**: .NET 10 SDK is resolved for the repo, baseline restore/build diagnostics are captured, and no environment/tooling blockers remain.

---

### 02-upgrade-identity-and-edge-apps: Upgrade identity, gateway, and orchestration entry points

Upgrade entry-point services that front authentication and edge routing first: IDP, OcelotApiGw, and Saga.Orchestrator. These projects have high API incompatibility and package churn signals, so isolating them early surfaces cross-cutting auth/routing breaks before broad rollout.

This task includes upgrading each app to net10.0, updating directly related package versions, and fixing compile/runtime-affecting API changes discovered in these app layers while keeping supporting libraries consumable.

**Done when**: IDP, OcelotApiGw, and Saga.Orchestrator target net10.0, restore/build passes for these projects, and known incompatible package/API issues in these apps are resolved or explicitly tracked.

---

### 03-upgrade-customer-product-and-scheduling-apps: Upgrade customer-facing and scheduled processing services

Upgrade Customer.API, Product.API, and Hangfire.API to net10.0. These applications combine API compatibility issues with package deprecations/recommendations and are independently deployable, making them a suitable second app wave.

This task addresses app-level breaking API changes inline and applies package updates required for net10 compatibility, while recording any deferred compatibility work under the selected unsupported package handling approach.

**Done when**: Customer.API, Product.API, and Hangfire.API build on net10.0 with required package updates applied and no unresolved compile-time blockers.

---

### 04-upgrade-inventory-and-basket-apps: Upgrade inventory and basket service boundary applications

Upgrade Inventory.Product.API, Inventory.Grpc, and Basket.API together because they are tightly coupled through gRPC and shared event contracts. Assessment signals show mandatory incompatibilities and project-level TFM updates across this boundary.

This task upgrades these applications to net10.0, resolves API/package incompatibilities affecting service-to-service integration, and verifies gRPC/message contract compatibility remains intact after framework and package changes.

**Done when**: Inventory.Product.API, Inventory.Grpc, and Basket.API target net10.0, compile successfully, and core inter-service contract/build validation passes.

---

### 05-upgrade-ordering-app-surface: Upgrade ordering API and finalize application-tier migration

Upgrade Ordering.API as the final application-tier endpoint after its dependent app layers have been modernized. Ordering has one of the deepest internal dependency stacks and notable API/package risk signals.

This task upgrades Ordering.API to net10.0, applies compatible package updates, and resolves app-facing API incompatibilities so that all top-level applications in the solution are migrated before starting library consolidation.

**Done when**: Ordering.API builds and restores on net10.0, all top-level applications in the solution are migrated, and no app-tier project remains on net8.0.

---

### 06-consolidate-shared-libraries-and-logging: Upgrade shared libraries and modernize logging abstractions

After all applications are upgraded, consolidate shared libraries to net10.0: Common.Logging, Shared (both), Contracts, Infrastructure, EventBus.Messages, EventBus.MessageComponents, Ordering.Domain, Ordering.Application, Ordering.Infrastructure, IDP.Infrastructure, and IDP.Presentation. This phase removes the remaining framework lag in reusable components.

Because Common.Logging is flagged for modernization, this task also migrates logging abstraction usage toward Microsoft.Extensions.Logging where required by impacted projects, along with associated package/reference updates.

**Done when**: All class library projects target net10.0, shared components restore/build successfully, and logging modernization changes compile across affected projects.

---

### 07-resolve-remaining-compatibility-and-security-updates: Close deferred package/API/security findings

Resolve remaining compatibility findings not fully closed during project-by-project upgrades, focusing on incompatible/deprecated packages and known vulnerable dependencies highlighted in assessment outputs.

This task ensures the selected package strategy is fully implemented, security-related NuGet updates are applied where compatible versions exist, and replacement/removal actions are completed for packages lacking net10 support.

**Done when**: Incompatible package findings are addressed per selected strategy, vulnerable/deprecated package actions are completed or documented with rationale, and solution restore succeeds without unresolved mandatory compatibility blockers.

---

### 08-final-validation: Perform end-to-end solution validation on .NET 10

Run full-solution validation after migration tasks complete: restore, clean build, and all relevant tests. This confirms dependency coherence, package compatibility, and runtime-facing compile-time correctness across the full microservice repository.

Capture any remaining warnings or edge-case issues for explicit follow-up, but require zero build errors and no unresolved mandatory upgrade findings.

**Done when**: Entire solution builds on net10.0 without errors, affected tests pass, and final upgrade status is ready for user review.
