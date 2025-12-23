using Product.API.Entities;
using Shared.DTOs.Product;

namespace Product.API.Services.Interfaces
{
    public interface IClipSearchService
    {
        Task IndexProductAsync(CatalogProduct product);
        Task BulkIndexProductsAsync(List<CatalogProduct> products);
        Task DeleteProductIndexAsync(Guid productId);
        Task<(List<Guid> ProductIds, int Total)> SearchProductIdsAsync(string? query, int page, int size);
    }
}
