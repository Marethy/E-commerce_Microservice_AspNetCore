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
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure PostgreSQL UUID generation
            if (Database.IsNpgsql())
            {
                modelBuilder.HasPostgresExtension("uuid-ossp");
            }
            
            // Configure CatalogProduct
            modelBuilder.Entity<CatalogProduct>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.CategoryId)
                      .HasColumnType("uuid");

                entity.Property(e => e.BrandId)
                      .HasColumnType("uuid");

                entity.Property(e => e.SellerId)
                      .HasColumnType("uuid");

                entity.Property(e => e.No)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(250);
                
                entity.Property(e => e.Summary)
                      .HasMaxLength(500);

                entity.Property(e => e.ShortDescription)
                      .HasMaxLength(1000);
                
                entity.Property(e => e.Price)
                      .HasPrecision(18, 2);

                entity.Property(e => e.OriginalPrice)
                      .HasPrecision(18, 2);

                entity.Property(e => e.RatingAverage)
                      .HasPrecision(3, 2);

                entity.Property(e => e.InventoryStatus)
                      .HasMaxLength(50)
                      .HasDefaultValue("IN_STOCK");

                entity.Property(e => e.Slug)
                      .HasMaxLength(300);

                entity.HasIndex(x => x.No)
                      .IsUnique()
                      .HasDatabaseName("IX_Products_No");

                entity.HasIndex(x => x.Slug)
                      .HasDatabaseName("IX_Products_Slug");
                
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Seller)
                      .WithMany(s => s.Products)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.ParentId)
                      .HasColumnType("uuid");

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.Property(e => e.Url)
                      .HasMaxLength(500);

                entity.HasIndex(x => x.Name)
                      .IsUnique()
                      .HasDatabaseName("IX_Categories_Name");

                // Self-referencing relationship
                entity.HasOne(c => c.Parent)
                      .WithMany(c => c.Children)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ProductReview
            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.ProductId)
                      .HasColumnType("uuid")
                      .IsRequired();
                
                entity.Property(e => e.UserId)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasColumnType("varchar(100)");

                entity.Property(e => e.Rating)
                      .HasPrecision(2, 1)
                      .IsRequired();

                entity.Property(e => e.Title)
                      .HasMaxLength(200);
                
                entity.Property(e => e.Comment)
                      .HasMaxLength(2000);

                entity.Property(e => e.ReviewDate)
                      .IsRequired();
                
                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(x => new { x.UserId, x.ProductId })
                      .IsUnique()
                      .HasDatabaseName("IX_ProductReviews_UserId_ProductId");

                entity.ToTable(t => t.HasCheckConstraint("CK_ProductReview_Rating", "\"Rating\" >= 1.0 AND \"Rating\" <= 5.0"));
            });

            // Configure Brand
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Slug)
                      .IsRequired()
                      .HasMaxLength(250);

                entity.Property(e => e.CountryOfOrigin)
                      .HasMaxLength(100);

                entity.Property(e => e.LogoUrl)
                      .HasMaxLength(500);

                entity.Property(e => e.Description)
                      .HasMaxLength(1000);

                entity.HasIndex(x => x.Name)
                      .IsUnique()
                      .HasDatabaseName("IX_Brands_Name");

                entity.HasIndex(x => x.Slug)
                      .IsUnique()
                      .HasDatabaseName("IX_Brands_Slug");
            });

            // Configure Seller
            modelBuilder.Entity<Seller>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Email)
                      .HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                      .HasMaxLength(20);

                entity.Property(e => e.Address)
                      .HasMaxLength(500);

                entity.Property(e => e.Rating)
                      .HasPrecision(3, 2);

                entity.HasIndex(x => x.Name)
                      .HasDatabaseName("IX_Sellers_Name");
            });

            // Configure ProductImage
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.ProductId)
                      .HasColumnType("uuid")
                      .IsRequired();

                entity.Property(e => e.Url)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.AltText)
                      .HasMaxLength(200);

                entity.HasOne(i => i.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.ProductId, x.Position })
                      .HasDatabaseName("IX_ProductImages_ProductId_Position");
            });

            // Configure ProductSpecification
            modelBuilder.Entity<ProductSpecification>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.ProductId)
                      .HasColumnType("uuid")
                      .IsRequired();

                entity.Property(e => e.SpecGroup)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.SpecName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.SpecValue)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.HasOne(s => s.Product)
                      .WithMany(p => p.Specifications)
                      .HasForeignKey(s => s.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.ProductId, x.SpecGroup, x.SpecName })
                      .HasDatabaseName("IX_ProductSpecifications_Product_Group_Name");
            });

            // Configure ProductCategory (Many-to-Many)
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(pc => new { pc.ProductId, pc.CategoryId });

                entity.Property(e => e.ProductId)
                      .HasColumnType("uuid");

                entity.Property(e => e.CategoryId)
                      .HasColumnType("uuid");

                entity.HasOne(pc => pc.Product)
                      .WithMany(p => p.ProductCategories)
                      .HasForeignKey(pc => pc.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Category)
                      .WithMany(c => c.ProductCategories)
                      .HasForeignKey(pc => pc.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Wishlist
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType("uuid")
                      .HasDefaultValueSql("uuid_generate_v4()");

                entity.Property(e => e.ProductId)
                      .HasColumnType("uuid")
                      .IsRequired();

                entity.Property(e => e.UserId)
                      .IsRequired()
                      .HasMaxLength(100)
                      .HasColumnType("varchar(100)");

                entity.Property(e => e.OriginalPrice)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(e => e.AddedDate)
                      .IsRequired();

                entity.HasOne(w => w.Product)
                      .WithMany()
                      .HasForeignKey(w => w.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.UserId, x.ProductId })
                      .IsUnique()
                      .HasDatabaseName("IX_Wishlists_UserId_ProductId");

                entity.HasIndex(x => x.UserId)
                      .HasDatabaseName("IX_Wishlists_UserId");
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