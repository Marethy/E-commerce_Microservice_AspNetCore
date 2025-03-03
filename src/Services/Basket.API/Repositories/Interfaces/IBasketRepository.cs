using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Repositories.Interfaces
{
    public interface IBasketRepository
    {
        Task<Cart?> GetBasketByUserName(string username);
        Task<Cart> UpdateBasket(Cart cart, DistributedCacheEntryOptions options=null);
        Task<bool> DeleteBasketByUserName(string username);


    }
}
