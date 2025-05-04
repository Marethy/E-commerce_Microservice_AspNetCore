using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="sectionName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetOptions<T>(this IServiceCollection services, string sectionName)
        where T : new()
    {
        using var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var section = configuration.GetSection(sectionName);
        var options = new T();
        section.Bind(options);

        return options;
    }
}