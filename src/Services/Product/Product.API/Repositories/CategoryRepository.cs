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
        => await FindAll()
            .AsNoTracking()
            .ToListAsync();

    public async Task<Category?> GetCategory(Guid id)
        => await FindByCondition(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();

    public async Task<Category?> GetCategoryByName(string name)
        => await FindByCondition(x => x.Name == name)
            .AsNoTracking()
            .FirstOrDefaultAsync();

    public async Task<bool> CategoryExistsAsync(Guid id)
        => await FindByCondition(x => x.Id == id).AnyAsync();

    public async Task<IEnumerable<Category>> GetCategoriesWithProducts()
        => await FindAll(false, x => x.Products)
            .AsNoTracking()
            .ToListAsync();

    // ===== HIERARCHY METHODS =====
    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        => await FindByCondition(x => x.ParentId == null)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<IEnumerable<Category>> GetSubcategoriesAsync(Guid parentId)
        => await FindByCondition(x => x.ParentId == parentId)
            .Include(x => x.Children)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<IEnumerable<Category>> GetCategoryPathAsync(Guid categoryId)
    {
        var path = new List<Category>();
        var current = await FindByCondition(x => x.Id == categoryId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        
        while (current != null)
        {
            path.Insert(0, current);
            if (current.ParentId.HasValue)
                current = await FindByCondition(x => x.Id == current.ParentId.Value)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            else
                break;
        }
        
        return path;
    }

    public async Task<Category?> GetCategoryWithChildrenAsync(Guid categoryId)
        => await FindByCondition(x => x.Id == categoryId)
            .Include(x => x.Children)
            .AsNoTracking()
            .FirstOrDefaultAsync();

    public async Task<Category?> GetCategoryWithHierarchyAsync(Guid categoryId)
        => await FindByCondition(x => x.Id == categoryId)
            .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
            .AsNoTracking()
            .FirstOrDefaultAsync();

    public async Task<bool> HasSubcategoriesAsync(Guid categoryId)
        => await FindByCondition(x => x.ParentId == categoryId).AnyAsync();

    public async Task<IEnumerable<Category>> GetCategoriesByProductIdAsync(Guid productId)
        => await FindAll()
            .Include(x => x.ProductCategories)
            .Where(x => x.ProductCategories.Any(pc => pc.ProductId == productId))
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Category>> GetFullHierarchyAsync()
        => await FindByCondition(x => x.ParentId == null)
            .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                    .ThenInclude(x => x.Children)
                        .ThenInclude(x => x.Children)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
}
