using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;
using Infrastructure.Common;
using Contracts.Common.Interfaces;

namespace Product.API.Repositories;

public class ProductReviewRepository : RepositoryBase<ProductReview, Guid, ProductContext>, IProductReviewRepository
{
    public ProductReviewRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork)
        : base(dbContext, unitOfWork)
    {
    }

    public async Task<IEnumerable<ProductReview>> GetAllReviewsAsync()
        => await FindAll(false, x => x.Product)
                .OrderByDescending(x => x.CreatedDate)
                .Take(100) // Limit to prevent performance issues
                .ToListAsync();

    public async Task<IEnumerable<ProductReview>> GetReviewsByProduct(Guid productId)
        => await FindByCondition(x => x.ProductId == productId, false, x => x.Product)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

    public async Task<IEnumerable<ProductReview>> GetReviewsByUser(string userId)
        => await FindByCondition(x => x.UserId == userId, false, x => x.Product)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

    public async Task<ProductReview?> GetReview(Guid id)
        => await FindByCondition(x => x.Id == id, false, x => x.Product)
                .FirstOrDefaultAsync();

    public async Task<double> GetAverageRatingByProduct(Guid productId)
    {
        var reviews = await FindByCondition(x => x.ProductId == productId).ToListAsync();
        return reviews.Any() ? (double)reviews.Average(x => x.Rating) : 0;
    }

    public async Task<int> GetReviewCountByProduct(Guid productId)
        => await FindByCondition(x => x.ProductId == productId).CountAsync();

    public async Task<bool> HasUserReviewedProduct(string userId, Guid productId)
        => await FindByCondition(x => x.UserId == userId && x.ProductId == productId).AnyAsync();

    public async Task<(IEnumerable<ProductReview> Replies, int TotalCount)> GetReviewRepliesAsync(Guid reviewId, int page, int size)
    {
        // Reply functionality removed - ParentReviewId not in database
        return (Array.Empty<ProductReview>(), 0);
    }
}