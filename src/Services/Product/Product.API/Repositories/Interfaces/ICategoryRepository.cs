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
    }
}