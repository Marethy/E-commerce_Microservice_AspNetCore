﻿
using Common.Logging;
using Serilog;

namespace Saga.Orchestrator.Extensions;

public static class HostExtensions
{
    internal static void AddAppConfiguration(this ConfigureHostBuilder host)
    {
        host.ConfigureAppConfiguration((context, config) =>
        {
            var env = context.HostingEnvironment;
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables();
        }).UseSerilog(SeriLogger.Configure);
    }
}
