using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;
using Infrastructure.Common;
using Contracts.Common.Interfaces;

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
    
    public async Task<bool> CategoryExists(Guid categoryId)
        => await _context.Categories.AnyAsync(x => x.Id == categoryId);

}