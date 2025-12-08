using Contracts.Common.Interfaces;
using Product.API.Entities;
using Product.API.Persistence;

namespace Product.API.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepositoryBase<Category, Guid, ProductContext>
    {
        Task<IEnumerable<Category>> GetCategories();
        Task<Category?> GetCategory(Guid id);
        Task<Category?> GetCategoryByName(string name);
        Task<bool> CategoryExistsAsync(Guid id);
        Task<IEnumerable<Category>> GetCategoriesWithProducts();
        // ===== HIERARCHY METHODS =====
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubcategoriesAsync(Guid parentId);
        Task<IEnumerable<Category>> GetCategoryPathAsync(Guid categoryId);
        Task<Category?> GetCategoryWithHierarchyAsync(Guid categoryId);
        Task<bool> HasSubcategoriesAsync(Guid categoryId);
        Task<IEnumerable<Category>> GetCategoriesByProductIdAsync(Guid productId);
        Task<IEnumerable<Category>> GetFullHierarchyAsync();
    }
}
