using Hangfire.Dashboard;

namespace Infrastructure.ScheduleJobs;

public class AuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}