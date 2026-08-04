# 04-upgrade-inventory-and-basket-apps: Upgrade inventory and basket service boundary applications

Upgrade Inventory.Product.API, Inventory.Grpc, and Basket.API together because they are tightly coupled through gRPC and shared event contracts. Assessment signals show mandatory incompatibilities and project-level TFM updates across this boundary.

This task upgrades these applications to net10.0, resolves API/package incompatibilities affecting service-to-service integration, and verifies gRPC/message contract compatibility remains intact after framework and package changes.

**Done when**: Inventory.Product.API, Inventory.Grpc, and Basket.API target net10.0, compile successfully, and core inter-service contract/build validation passes.
