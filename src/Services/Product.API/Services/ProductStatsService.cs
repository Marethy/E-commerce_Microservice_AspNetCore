using Microsoft.EntityFrameworkCore;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;
using Product.API.Services.Interfaces;

namespace Product.API.Services
{
    public class ProductStatsService : IProductStatsService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductReviewRepository _reviewRepository;
        private readonly ProductContext _context;

        public ProductStatsService(
            IProductRepository productRepository,
            IProductReviewRepository reviewRepository,
            ProductContext context)
        {
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
            _context = context;
        }

        public async Task UpdateProductRatingAsync(Guid productId)
        {
            var product = await _productRepository.GetProduct(productId);
            if (product == null) return;

            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if (reviews.Any())
            {
                product.RatingAverage = reviews.Average(r => r.Rating);
                product.ReviewCount = reviews.Count;
            }
            else
            {
                product.RatingAverage = 0;
                product.ReviewCount = 0;
            }

            await _productRepository.UpdateAsync(product);
        }

        public async Task UpdateProductSalesAsync(Guid productId, int quantitySold)
        {
            var product = await _productRepository.GetProduct(productId);
            if (product == null) return;

            product.AllTimeQuantitySold += quantitySold;
            product.QuantitySoldLast30Days += quantitySold;

            // Update inventory status if stock is low
            if (product.StockQuantity <= 0)
            {
                product.InventoryStatus = Constants.InventoryStatus.OutOfStock;
            }
            else if (product.StockQuantity <= 10)
            {
                product.InventoryStatus = Constants.InventoryStatus.LowStock;
            }

            await _productRepository.UpdateAsync(product);
        }

        public async Task UpdateInventoryStatusAsync(Guid productId, string status)
        {
            var product = await _productRepository.GetProduct(productId);
            if (product == null) return;

            product.InventoryStatus = status;
            await _productRepository.UpdateAsync(product);
        }

        public async Task<decimal> CalculateRatingAverageAsync(Guid productId)
        {
            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }
    }
}
