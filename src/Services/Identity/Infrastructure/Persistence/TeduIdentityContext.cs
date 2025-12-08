using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.Extensions;

namespace IDP.Infrastructure.Persistence;

public class TeduIdentityContext : IdentityDbContext<User>
{
    public IDbConnection Connection => Database.GetDbConnection();

    public TeduIdentityContext(DbContextOptions<TeduIdentityContext> options) : base(options)
    {
    }

    public DbSet<Permission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(TeduIdentityContext).Assembly);
        builder.ApplyIdentityConfiguration();
    }
}
