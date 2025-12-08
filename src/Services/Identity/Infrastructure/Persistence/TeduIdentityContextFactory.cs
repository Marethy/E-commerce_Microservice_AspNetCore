using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IDP.Infrastructure.Persistence
{
    public class TeduIdentityContextFactory
        : IDesignTimeDbContextFactory<TeduIdentityContext>
    {
        public TeduIdentityContext CreateDbContext(string[] args)
        {
            // 1. Ensure we look in the correct folder for appsettings.json
            var basePath = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            // 2. Read your named connection string
            var connStr = config.GetConnectionString("IdentitySqlConnection");

            // 3. Configure the DbContextOptions (point migrations at this assembly)
            var optionsBuilder = new DbContextOptionsBuilder<TeduIdentityContext>();
            optionsBuilder.UseSqlServer(connStr, sql =>
                sql.MigrationsAssembly(typeof(TeduIdentityContextFactory).Assembly.FullName)
            );

            return new TeduIdentityContext(optionsBuilder.Options);
        }
    }
}
