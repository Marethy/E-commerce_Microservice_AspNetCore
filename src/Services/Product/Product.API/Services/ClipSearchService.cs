using System.Text.Json;
using Product.API.Entities;
using Product.API.Services.Interfaces;
using Shared.DTOs.Product;

namespace Product.API.Services
{
   public class ClipSearchService : IClipSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClipSearchService> _logger;

        public ClipSearchService(IHttpClientFactory httpClientFactory, ILogger<ClipSearchService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ClipSearch");
            _logger = logger;
        }

        public async Task IndexProductAsync(CatalogProduct product)
        {
            try
            {
                var specifications = product.Specifications != null && product.Specifications.Any()
                    ? string.Join(" ", product.Specifications.Select(s => $"{s.SpecName}: {s.SpecValue}"))
                    : null;

                var request = new
                {
                    id = product.Id.ToString(),
                    name = product.Name,
                    description = product.Description,
                    shortDescription = product.Summary
                };

                var response = await _httpClient.PostAsJsonAsync("/index-product", request);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Indexed product {ProductId} to CLIP search", product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index product {ProductId} to CLIP search", product.Id);
            }
        }

        public async Task BulkIndexProductsAsync(List<CatalogProduct> products)
        {
            try
            {
                var requests = products.Select(product => new
                {
                    id = product.Id.ToString(),
                    name = product.Name,
                    description = product.Description,
                    shortDescription = product.Summary
                }).ToList();

                var response = await _httpClient.PostAsJsonAsync("/bulk-index-products", requests);
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Bulk indexed {Count} products to CLIP search", products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk index {Count} products to CLIP search", products.Count);
            }
        }


        public async Task DeleteProductIndexAsync(Guid productId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/index-product/{productId}");
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Deleted product {ProductId} from CLIP search index", productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product {ProductId} from CLIP search index", productId);
            }
        }

        public async Task<(List<Guid> ProductIds, int Total)> SearchProductIdsAsync(string? query, int page, int size, byte[]? imageBytes = null)
        {
            try
            {
                var request = new
                {
                    query,
                    image = imageBytes != null ? Convert.ToBase64String(imageBytes) : null,
                    page,
                    size
                };

                var response = await _httpClient.PostAsJsonAsync("/search", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<SearchResponse>();
                
                if (result == null)
                    return (new List<Guid>(), 0);

                var productIds = result.ProductIds
                    .Select(id => Guid.Parse(id))
                    .ToList();

                return (productIds, result.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search products via CLIP search");
                return (new List<Guid>(), 0);
            }
        }

        private class SearchResponse
        {
            public List<string> ProductIds { get; set; } = new();
            public int Total { get; set; }
        }
    }
}
