using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Contracts.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository(IDistributedCache redisCacheService, ISerializeService serializeService,ILogger logger) : IBasketRepository
    {
        private readonly IDistributedCache _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
        private readonly ISerializeService _serializeService = serializeService ?? throw new ArgumentNullException(nameof(serializeService));
        private readonly ILogger _logger = logger;
        public async Task<Cart?> GetBasketByUserName(string username)
        {
            var basket = await _redisCacheService.GetStringAsync(username);
            return string.IsNullOrEmpty(basket) ? null : _serializeService.Deserialize<Cart>(basket);
        }

        public async Task<Cart> UpdateBasket(Cart cart, DistributedCacheEntryOptions? options = null)
        {
            var basket = _serializeService.Serialize(cart);
            if (options == null)
            {
                await _redisCacheService.SetStringAsync(cart.Username, basket);
            }
            else
            {
                await _redisCacheService.SetStringAsync(cart.Username, basket, options);
            }
            return await GetBasketByUserName(cart.Username);
        }

        public async Task<bool> DeleteBasketByUserName(string username)
        {
            try
            {
                await _redisCacheService.RemoveAsync(username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting basket for {username}. Error: {ex.Message}");
                return false;
            }
        }
    }
}

