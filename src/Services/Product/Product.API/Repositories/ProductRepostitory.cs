using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;
using Infrastructure.Common;
using Contracts.Common.Interfaces;
using Shared.DTOs.Product;

namespace Product.API.Repositories;

public class ProductRepository : RepositoryBase<CatalogProduct, Guid, ProductContext>, IProductRepository
{
    public ProductRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork)
        : base(dbContext, unitOfWork)
    {
    }
    
    public async Task<IEnumerable<CatalogProduct>> GetProducts() 
        => await FindAll(false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .AsNoTracking()
                .ToListAsync();

    public async Task<IEnumerable<CatalogProduct>> GetProductsByCategory(Guid categoryId)
        => await FindByCondition(x => x.CategoryId == categoryId, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .AsNoTracking()
                .Take(100)
                .ToListAsync();

    public async Task<(IEnumerable<CatalogProduct> Items, int TotalCount)> SearchProducts(
        ProductFilterDto filter, 
        int page = 0, 
        int size = 20)
    {
        var query = FindAll(false, x => x.Category, x => x.Brand, x => x.Seller)
            .Include(x => x.Images)
            .Include(x => x.ProductCategories)
            .AsNoTracking()
            .AsQueryable();

        if (filter.ProductIds != null && filter.ProductIds.Any())
        {
            query = query.Where(x => filter.ProductIds.Contains(x.Id));
        }

        // Filter by price range
        if (filter.MinPrice.HasValue)
            query = query.Where(x => x.Price >= filter.MinPrice.Value);
        
        if (filter.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= filter.MaxPrice.Value);

        // Filter by rating range
        if (filter.MinRating.HasValue)
            query = query.Where(x => x.RatingAverage >= filter.MinRating.Value);
        
        if (filter.MaxRating.HasValue)
            query = query.Where(x => x.RatingAverage <= filter.MaxRating.Value);

        // Filter by brand IDs
        if (filter.BrandIds != null && filter.BrandIds.Any())
            query = query.Where(x => x.BrandId.HasValue && filter.BrandIds.Contains(x.BrandId.Value));

        // Filter by brand names
        if (filter.BrandNames != null && filter.BrandNames.Any())
            query = query.Where(x => x.Brand != null && filter.BrandNames.Contains(x.Brand.Name));

        // Filter by category IDs with hierarchy support
        if (filter.CategoryIds != null && filter.CategoryIds.Count != 0)
        {
            // Get all descendant categories for the selected categories (including self)
            var allCategoryIds = await GetAllDescendantCategoryIds(filter.CategoryIds);
            
            query = query.Where(x => 
                allCategoryIds.Contains(x.CategoryId) ||
                x.ProductCategories.Any(pc => allCategoryIds.Contains(pc.CategoryId))
            );
        }

        // Filter by inventory status
        if (!string.IsNullOrWhiteSpace(filter.InventoryStatus))
        {
            if (filter.InventoryStatus.Equals("available", StringComparison.CurrentCultureIgnoreCase))
                query = query.Where(x => x.StockQuantity > 0);
            else if (filter.InventoryStatus.Equals("out_of_stock", StringComparison.CurrentCultureIgnoreCase))
                query = query.Where(x => x.StockQuantity <= 0);
        }

        // Filter by discount
        if (filter.HasDiscount.HasValue && filter.HasDiscount.Value)
            query = query.Where(x => x.DiscountPercentage.HasValue && x.DiscountPercentage.Value > 0);

        // Filter by minimum discount percentage
        if (filter.MinDiscountPercentage.HasValue && filter.MinDiscountPercentage.Value > 0)
            query = query.Where(x => x.DiscountPercentage.HasValue && x.DiscountPercentage.Value >= filter.MinDiscountPercentage.Value);

        // Get total count before pagination (for infinite scroll support)
        var totalCount = await query.CountAsync();

        // Sorting
        query = (filter.SortBy?.ToLower()) switch
        {
            "price" => filter.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(x => x.Price)
                : query.OrderBy(x => x.Price),
            "rating" => query.OrderByDescending(x => x.RatingAverage),
            "sales" => query.OrderByDescending(x => x.QuantitySoldLast30Days),
            "created" => query.OrderByDescending(x => x.CreatedDate),
            "newest" => query.OrderByDescending(x => x.CreatedDate),
            _ => query.OrderByDescending(x => x.CreatedDate) // Default: newest first
        };

        // Pagination (supports infinite scroll)
        var items = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Get all descendant category IDs for the given category IDs (including self)
    /// Uses recursive CTE for optimal performance instead of N+1 queries
    /// </summary>
    private async Task<List<Guid>> GetAllDescendantCategoryIds(List<Guid> categoryIds)
    {
        // Use raw SQL with CTE for better performance
        var sql = @"
            WITH RECURSIVE category_tree AS (
                -- Base: selected categories
                SELECT ""Id"" FROM ""Categories"" WHERE ""Id"" = ANY(@categoryIds)
                
                UNION ALL
                
                -- Recursive: all children
                SELECT c.""Id"" 
                FROM ""Categories"" c
                INNER JOIN category_tree ct ON c.""ParentId"" = ct.""Id""
            )
            SELECT DISTINCT ""Id"" FROM category_tree;
        ";

        var result = await _context.Categories
            .FromSqlInterpolated($@"
                WITH RECURSIVE category_tree AS (
                    SELECT ""Id"" FROM ""Categories"" WHERE ""Id"" = ANY({categoryIds.ToArray()})
                    UNION ALL
                    SELECT c.""Id"" FROM ""Categories"" c
                    INNER JOIN category_tree ct ON c.""ParentId"" = ct.""Id""
                )
                SELECT ""Id"" FROM category_tree")
            .Select(x => x.Id)
            .ToListAsync();

        return result;
    }

    public async Task<CatalogProduct?> GetProduct(Guid id)
        => await FindByCondition(x => x.Id == id, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Reviews)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .AsNoTracking()
                .FirstOrDefaultAsync();
    
    public async Task<CatalogProduct?> GetProductByNo(string productNo)
        => await FindByCondition(x => x.No == productNo, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .AsNoTracking()
                .FirstOrDefaultAsync();
    
    public async Task<CatalogProduct?> GetProductBySlug(string slug)
        => await FindByCondition(x => x.Slug == slug, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Reviews)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .AsNoTracking()
                .FirstOrDefaultAsync();
    
    public async Task<IEnumerable<ProductImage>> GetProductImages(Guid productId)
        => await _context.ProductImages
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.Position)
                .AsNoTracking()
                .ToListAsync();
    
    public async Task<bool> CategoryExists(Guid categoryId)
        => await _context.Categories.AnyAsync(x => x.Id == categoryId);

}