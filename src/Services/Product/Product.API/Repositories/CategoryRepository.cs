using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;
using Infrastructure.Common;
using Contracts.Common.Interfaces;

namespace Product.API.Repositories;

public class CategoryRepository : RepositoryBase<Category, Guid, ProductContext>, ICategoryRepository
{
    public CategoryRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork)
        : base(dbContext, unitOfWork)
    {
    }

    public async Task<IEnumerable<Category>> GetCategories()
        => await FindAll().ToListAsync();

    public async Task<Category?> GetCategory(Guid id)
        => await GetByIdAsync(id);

    public async Task<Category?> GetCategoryByName(string name)
        => await FindByCondition(x => x.Name == name).FirstOrDefaultAsync();

    public async Task<bool> CategoryExistsAsync(Guid id)
        => await FindByCondition(x => x.Id == id).AnyAsync();

    public async Task<IEnumerable<Category>> GetCategoriesWithProducts()
        => await FindAll(false, x => x.Products).ToListAsync();
}