# 06-consolidate-shared-libraries-and-logging: Upgrade shared libraries and modernize logging abstractions

After all applications are upgraded, consolidate shared libraries to net10.0: Common.Logging, Shared (both), Contracts, Infrastructure, EventBus.Messages, EventBus.MessageComponents, Ordering.Domain, Ordering.Application, Ordering.Infrastructure, IDP.Infrastructure, and IDP.Presentation. This phase removes the remaining framework lag in reusable components.

Because Common.Logging is flagged for modernization, this task also migrates logging abstraction usage toward Microsoft.Extensions.Logging where required by impacted projects, along with associated package/reference updates.

**Done when**: All class library projects target net10.0, shared components restore/build successfully, and logging modernization changes compile across affected projects.
