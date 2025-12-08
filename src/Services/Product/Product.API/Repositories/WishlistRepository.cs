using Contracts.Common.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Product.API.Entities;
using Product.API.Persistence;
using Product.API.Repositories.Interfaces;

namespace Product.API.Repositories;

public class WishlistRepository : RepositoryBase<Wishlist, Guid, ProductContext>, IWishlistRepository
{
    public WishlistRepository(ProductContext dbContext, IUnitOfWork<ProductContext> unitOfWork) 
 : base(dbContext, unitOfWork)
    {
    }

    public async Task<(IEnumerable<Wishlist> Items, int TotalCount)> GetUserWishlistAsync(
    string userId, 
 int page, 
        int limit)
    {
        var query = FindByCondition(w => w.UserId == userId)
            .Include(w => w.Product)
                .ThenInclude(p => p.Brand)
         .Include(w => w.Product)
                .ThenInclude(p => p.Images.Where(i => i.IsPrimary))
   .OrderByDescending(w => w.AddedDate);

        var totalCount = await query.CountAsync();
    
     var items = await query
 .Skip(page * limit)
.Take(limit)
         .ToListAsync();

      return (items, totalCount);
    }

    public async Task<bool> IsInWishlistAsync(string userId, Guid productId)
    {
        return await FindByCondition(w => w.UserId == userId && w.ProductId == productId)
            .AnyAsync();
    }

    public async Task<Wishlist?> GetWishlistItemAsync(string userId, Guid productId)
 {
        return await FindByCondition(w => w.UserId == userId && w.ProductId == productId)
          .FirstOrDefaultAsync();
  }

    public async Task<int> GetWishlistCountAsync(string userId)
    {
        return await FindByCondition(w => w.UserId == userId).CountAsync();
    }

    public async Task<int> GetRecentWishlistCountAsync(string userId, int days)
    {
    var cutoffDate = DateTimeOffset.UtcNow.AddDays(-days);
  return await FindByCondition(w => w.UserId == userId && w.AddedDate >= cutoffDate)
     .CountAsync();
    }

    public async Task<IEnumerable<(Guid ProductId, int Count)>> GetMostWishlistedProductsAsync(int limit)
    {
        return await FindAll()
            .GroupBy(w => w.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
   .OrderByDescending(x => x.Count)
         .Take(limit)
  .Select(x => ValueTuple.Create(x.ProductId, x.Count))
            .ToListAsync();
    }

    public async Task ClearWishlistAsync(string userId)
{
        var items = await FindByCondition(w => w.UserId == userId).ToListAsync();
  foreach (var item in items)
     {
       await DeleteAsync(item);
        }
    }
}
