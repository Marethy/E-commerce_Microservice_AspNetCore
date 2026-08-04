# .NET Version Upgrade Progress

## Overview

This scenario upgrades the microservice solution from .NET 8 to .NET 10 using a top-down, application-first strategy. Application entry points are migrated first, then shared libraries are consolidated, followed by compatibility/security closure and final validation.
**Progress**: 0/8 tasks complete <progress value="0" max="100"></progress> 0%
**Progress**: 0/8 tasks complete <progress value="0" max="100"></progress> 0%

## Tasks
- 🔄 01-prerequisites-tooling: Validate SDK and baseline upgrade settings ([Content](tasks/01-prerequisites-tooling/task.md))
- 🔲 01-prerequisites-tooling: Validate SDK and baseline upgrade settings
- 🔲 02-upgrade-identity-and-edge-apps: Upgrade identity, gateway, and orchestration entry points
- 🔲 03-upgrade-customer-product-and-scheduling-apps: Upgrade customer-facing and scheduled processing services
- 🔲 04-upgrade-inventory-and-basket-apps: Upgrade inventory and basket service boundary applications
- 🔲 05-upgrade-ordering-app-surface: Upgrade ordering API and finalize application-tier migration
- 🔲 06-consolidate-shared-libraries-and-logging: Upgrade shared libraries and modernize logging abstractions
- 🔲 07-resolve-remaining-compatibility-and-security-updates: Close deferred package/API/security findings
- 🔲 08-final-validation: Perform end-to-end solution validation on .NET 10
