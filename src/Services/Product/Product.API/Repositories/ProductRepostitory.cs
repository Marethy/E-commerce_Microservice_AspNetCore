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
                .ToListAsync();

    public async Task<IEnumerable<CatalogProduct>> GetProductsByCategory(Guid categoryId)
        => await FindByCondition(x => x.CategoryId == categoryId, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .ToListAsync();

    public async Task<(IEnumerable<CatalogProduct> Items, int TotalCount)> SearchProducts(
        ProductFilterDto filter, 
        int page = 0, 
        int size = 20)
    {
        var query = FindAll(false, x => x.Category, x => x.Brand, x => x.Seller)
            .Include(x => x.Images)
            .Include(x => x.Specifications)
            .AsQueryable();

        // Search by keyword (name, description, no)
        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var keyword = filter.Q.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                x.Description.ToLower().Contains(keyword) ||
                x.No.ToLower().Contains(keyword)
            );
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

        // Filter by category IDs
        if (filter.CategoryIds != null && filter.CategoryIds.Any())
            query = query.Where(x => filter.CategoryIds.Contains(x.CategoryId));

        // Filter by inventory status
        if (!string.IsNullOrWhiteSpace(filter.InventoryStatus))
        {
            if (filter.InventoryStatus.ToLower() == "available")
                query = query.Where(x => x.StockQuantity > 0);
            else if (filter.InventoryStatus.ToLower() == "out_of_stock")
                query = query.Where(x => x.StockQuantity <= 0);
        }

        // Get total count before pagination
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

        // Pagination
        var items = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<CatalogProduct?> GetProduct(Guid id)
        => await FindByCondition(x => x.Id == id, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Reviews)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .FirstOrDefaultAsync();
    
    public async Task<CatalogProduct?> GetProductByNo(string productNo)
        => await FindByCondition(x => x.No == productNo, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .FirstOrDefaultAsync();
    
    public async Task<CatalogProduct?> GetProductBySlug(string slug)
        => await FindByCondition(x => x.Slug == slug, false, x => x.Category, x => x.Brand, x => x.Seller)
                .Include(x => x.Reviews)
                .Include(x => x.Images)
                .Include(x => x.Specifications)
                .FirstOrDefaultAsync();
    
    public async Task<IEnumerable<ProductImage>> GetProductImages(Guid productId)
        => await _context.ProductImages
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.Position)
                .ToListAsync();
    
    public async Task<bool> CategoryExists(Guid categoryId)
        => await _context.Categories.AnyAsync(x => x.Id == categoryId);

}