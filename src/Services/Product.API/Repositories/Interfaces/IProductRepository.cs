using Contracts.Common.Interfaces;
using Product.API.Entities;
using Product.API.Persistence;

namespace Product.API.Repositories.Interfaces
{
    public interface IProductRepository : IRepositoryBase<CatalogProduct, Guid, ProductContext>
    {
        Task<IEnumerable<CatalogProduct>> GetProducts();
        Task<IEnumerable<CatalogProduct>> GetProductsByCategory(Guid categoryId);
        Task<CatalogProduct?> GetProduct(Guid id);
        Task<CatalogProduct?> GetProductByNo(string productNo);
        Task<bool> CategoryExists(Guid categoryId);
    }
}