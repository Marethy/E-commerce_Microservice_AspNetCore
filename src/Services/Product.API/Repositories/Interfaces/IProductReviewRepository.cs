using Contracts.Common.Interfaces;
using Product.API.Entities;
using Product.API.Persistence;

namespace Product.API.Repositories.Interfaces
{
    public interface IProductReviewRepository : IRepositoryBase<ProductReview, Guid, ProductContext>
    {
        Task<IEnumerable<ProductReview>> GetReviewsByProduct(Guid productId);
        Task<IEnumerable<ProductReview>> GetReviewsByUser(string userId);
        Task<ProductReview?> GetReview(Guid id);
        Task<double> GetAverageRatingByProduct(Guid productId);
        Task<int> GetReviewCountByProduct(Guid productId);
        Task<bool> HasUserReviewedProduct(string userId, Guid productId);
    }
}