using HealthChecks.UI.Client;
using Infrastructure.ScheduleJobs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Customer.API.Extensions;

public static class ApplicationExtensions
{
    public static void UseInfrastructure(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        //app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.UseHangfireDashboard(configuration);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapDefaultControllerRoute();
        });
    }
}