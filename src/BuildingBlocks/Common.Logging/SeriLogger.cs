using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Common.Logging
{
    public static class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
            (context, configuration) =>
            {
                var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");
                var environmentName = context.HostingEnvironment.EnvironmentName ?? "Development";
                var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");
                var userName = context.Configuration.GetValue<string>("ElasticConfiguration:UserName");
                var password = context.Configuration.GetValue<string>("ElasticConfiguration:Password");


                configuration
                    .WriteTo.Debug()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}")
                     .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                     {
                         IndexFormat = $"ecommercelogs -{applicationName}-{environmentName}-{DateTime.UtcNow:yyyy-MM}",
                         AutoRegisterTemplate = true,
                         NumberOfReplicas = 1,
                         NumberOfShards = 2,
                         ModifyConnectionSettings = x => x.BasicAuthentication(userName, password)
                     })
                    .Enrich.FromLogContext()

                    .Enrich.WithProperty("Environment", environmentName)
                    .Enrich.WithProperty("Application", applicationName)
                    .ReadFrom.Configuration(context.Configuration);
            };
    }
}