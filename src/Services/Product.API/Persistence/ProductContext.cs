using Contracts.Domains.Interfaces;
using Microsoft.EntityFrameworkCore;
using Product.API.Entities;

namespace Product.API.Persistence
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }

        public DbSet<CatalogProduct> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure PostgreSQL UUID generation
            if (Database.IsNpgsql())
            {
                modelBuilder.HasPostgresExtension("uuid-ossp");
            }
            
            // Configure CatalogProduct with Data Annotations moved here (DDD approach)
            modelBuilder.Entity<CatalogProduct>(entity =>
            {
                // Configure UUID for PostgreSQL
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.CategoryId)
                      .HasColumnType("uuid");

                entity.Property(e => e.No)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(250);
                
                entity.Property(e => e.Summary)
                      .HasMaxLength(500);
                
                entity.Property(e => e.Price)
                      .HasPrecision(18, 2);
                
                entity.Property(e => e.CategoryId)
                      .IsRequired();

                entity.HasIndex(x => x.No)
                      .IsUnique()
                      .HasDatabaseName("IX_Products_No");
                
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                // Configure UUID for PostgreSQL
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                      .IsUnique()
                      .HasDatabaseName("IX_Categories_Name");
            });

            // Configure ProductReview
            modelBuilder.Entity<ProductReview>(entity =>
            {
                // Configure UUID for PostgreSQL
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.ProductId)
                      .HasColumnType("uuid")
                      .IsRequired();
                
                // Configure UserId as string (not UUID)
                entity.Property(e => e.UserId)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasColumnType("varchar(100)");
                
                entity.Property(e => e.Rating)
                      .IsRequired();
                
                entity.Property(e => e.Comment)
                      .HasMaxLength(1000);
                
                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(x => new { x.UserId, x.ProductId })
                      .IsUnique()
                      .HasDatabaseName("IX_ProductReviews_UserId_ProductId");

                // PostgreSQL-specific check constraint
                entity.HasCheckConstraint("CK_ProductReview_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5");
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var modifiedEntries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted);

            foreach (var item in modifiedEntries)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        // Handle IDateTracking (IAuditable inherits this)
                        if (item.Entity is IDateTracking addedDateEntity)
                        {
                            addedDateEntity.CreatedDate = DateTimeOffset.UtcNow;
                        }
                        
                        // Handle IUserTracking (IAuditable inherits this)  
                        if (item.Entity is IUserTracking addedUserEntity)
                        {
                            // You can get current user from ICurrentUserService if available
                            // addedUserEntity.CreatedBy = _currentUserService.UserId;
                        }
                        break;

                    case EntityState.Modified:
                        // Prevent Id modification
                        Entry(item.Entity).Property("Id").IsModified = false;
                        
                        // Handle IDateTracking
                        if (item.Entity is IDateTracking modifiedDateEntity)
                        {
                            modifiedDateEntity.LastModifiedDate = DateTimeOffset.UtcNow;
                            // Prevent CreatedDate modification
                            Entry(item.Entity).Property(nameof(IDateTracking.CreatedDate)).IsModified = false;
                        }
                        
                        // Handle IUserTracking
                        if (item.Entity is IUserTracking modifiedUserEntity)
                        {
                            // modifiedUserEntity.UpdatedBy = _currentUserService.UserId;
                            // Prevent CreatedBy modification
                            Entry(item.Entity).Property(nameof(IUserTracking.CreatedBy)).IsModified = false;
                        }
                        break;

                    case EntityState.Deleted:
                        // Handle Soft Delete
                        if (item.Entity is ISoftDeletable softDeleteEntity)
                        {
                            // Convert hard delete to soft delete
                            item.State = EntityState.Modified;
                            softDeleteEntity.IsDeleted = true;
                            softDeleteEntity.DeletedDate = DateTimeOffset.UtcNow;
                            // softDeleteEntity.DeletedBy = _currentUserService.UserId;
                        }
                        break;
                }
            }
            
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}