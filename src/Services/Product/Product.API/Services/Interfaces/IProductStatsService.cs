using Product.API.Entities;

namespace Product.API.Services.Interfaces
{
    public interface IProductStatsService
    {
        /// <summary>
        /// Update product rating average and review count
        /// </summary>
        Task UpdateProductRatingAsync(Guid productId);

        /// <summary>
        /// Update product sales statistics
        /// </summary>
        Task UpdateProductSalesAsync(Guid productId, int quantitySold);

        /// <summary>
        /// Update product inventory status
        /// </summary>
        Task UpdateInventoryStatusAsync(Guid productId, string status);

        /// <summary>
        /// Calculate and update rating average for a product
        /// </summary>
        Task<decimal> CalculateRatingAverageAsync(Guid productId);
    }
}
