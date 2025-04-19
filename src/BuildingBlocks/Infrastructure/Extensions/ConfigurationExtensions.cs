using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    static  class ConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : class
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = configuration.GetSection(sectionName).Get<T>();
            if (options == null)
            {
                throw new ArgumentNullException($"Configuration section '{sectionName}' is not found or invalid.");
            }
            return options;
        }
    }
}
