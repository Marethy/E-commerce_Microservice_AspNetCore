# 02-upgrade-identity-and-edge-apps: Upgrade identity, gateway, and orchestration entry points

Upgrade entry-point services that front authentication and edge routing first: IDP, OcelotApiGw, and Saga.Orchestrator. These projects have high API incompatibility and package churn signals, so isolating them early surfaces cross-cutting auth/routing breaks before broad rollout.

This task includes upgrading each app to net10.0, updating directly related package versions, and fixing compile/runtime-affecting API changes discovered in these app layers while keeping supporting libraries consumable.

**Done when**: IDP, OcelotApiGw, and Saga.Orchestrator target net10.0, restore/build passes for these projects, and known incompatible package/API issues in these apps are resolved or explicitly tracked.
