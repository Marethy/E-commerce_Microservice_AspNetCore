using Product.API.Repositories.Interfaces;
using Product.API.Services.Interfaces;

namespace Product.API.Services
{
    public class ProductIndexingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProductIndexingService> _logger;

        public ProductIndexingService(IServiceProvider serviceProvider, ILogger<ProductIndexingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting 15 seconds for services to be ready...");
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var clipSearchService = scope.ServiceProvider.GetRequiredService<IClipSearchService>();

            try
            {
                _logger.LogInformation("Starting initial product indexing to CLIP search...");
                var products = await productRepository.GetProducts();
                var productList = products.ToList();

                _logger.LogInformation("Found {Count} products to index", productList.Count);

                const int batchSize = 1000;
                int indexed = 0;

                for (int i = 0; i < productList.Count; i += batchSize)
                {
                    try
                    {
                        var batch = productList.Skip(i).Take(batchSize).ToList();
                        await clipSearchService.BulkIndexProductsAsync(batch);
                        indexed += batch.Count;
                        _logger.LogInformation("Indexed {Indexed}/{Total} products", indexed, productList.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to index batch starting at {Index}", i);
                    }
                }

                _logger.LogInformation("Completed initial indexing of {Indexed}/{Total} products", indexed, productList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial product indexing");
            }
        }
    }
}
