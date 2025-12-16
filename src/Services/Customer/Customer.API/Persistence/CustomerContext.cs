using Microsoft.EntityFrameworkCore;

namespace Customer.API.Persistence
{
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options)
        {
        }

        public DbSet<Entities.Customer> Customers { get; set; } = null!;
        public DbSet<Entities.Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Entities.Customer>().HasIndex(x => x.UserName).IsUnique();
            modelBuilder.Entity<Entities.Customer>().HasIndex(x => x.Email).IsUnique();
            
            // Notification indexes for efficient queries
            modelBuilder.Entity<Entities.Notification>().HasIndex(x => x.UserId);
            modelBuilder.Entity<Entities.Notification>().HasIndex(x => x.NotificationDate);
            modelBuilder.Entity<Entities.Notification>().HasIndex(x => new { x.UserId, x.IsRead });
        }
    }
}