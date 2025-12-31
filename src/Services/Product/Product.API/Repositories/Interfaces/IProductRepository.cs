using Contracts.Common.Interfaces;
using Product.API.Entities;
using Product.API.Persistence;
using Shared.DTOs.Product;

namespace Product.API.Repositories.Interfaces
{
    public interface IProductRepository : IRepositoryBase<CatalogProduct, Guid, ProductContext>
    {
        Task<IEnumerable<CatalogProduct>> GetProducts();
        Task<IEnumerable<CatalogProduct>> GetProductsByCategory(Guid categoryId);
        Task<(IEnumerable<CatalogProduct> Items, int TotalCount)> SearchProducts(ProductFilterDto filter, int page = 0, int size = 20);
        Task<CatalogProduct?> GetProduct(Guid id);
        Task<CatalogProduct?> GetProductForUpdate(Guid id);
        Task<CatalogProduct?> GetProductByNo(string productNo);
        Task<CatalogProduct?> GetProductBySlug(string slug);
        Task<IEnumerable<ProductImage>> GetProductImages(Guid productId);
        Task<bool> CategoryExists(Guid categoryId);
    }
}