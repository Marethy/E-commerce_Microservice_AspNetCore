using Contracts.Common.Interfaces;
using Product.API.Entities;
using Product.API.Persistence;

namespace Product.API.Repositories.Interfaces;

public interface IWishlistRepository : IRepositoryBase<Wishlist, Guid, ProductContext>
{
    /// <summary>
    /// Get user's wishlist items with pagination
    /// </summary>
    Task<(IEnumerable<Wishlist> Items, int TotalCount)> GetUserWishlistAsync(
        string userId, 
        int page, 
        int limit);

    /// <summary>
    /// Check if product is in user's wishlist
    /// </summary>
    Task<bool> IsInWishlistAsync(string userId, Guid productId);

    /// <summary>
    /// Get wishlist item by user and product
    /// </summary>
    Task<Wishlist?> GetWishlistItemAsync(string userId, Guid productId);

    /// <summary>
    /// Get user's wishlist count
 /// </summary>
    Task<int> GetWishlistCountAsync(string userId);

    /// <summary>
    /// Get user's wishlist items added in last N days
    /// </summary>
    Task<int> GetRecentWishlistCountAsync(string userId, int days);

    /// <summary>
    /// Get most wishlisted products
    /// </summary>
    Task<IEnumerable<(Guid ProductId, int Count)>> GetMostWishlistedProductsAsync(int limit);

    /// <summary>
    /// Remove all wishlist items for a user
    /// </summary>
    Task ClearWishlistAsync(string userId);
}
