using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Contracts.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCacheService;
        private readonly ISerializeService _serializeService;
        private readonly ILogger<BasketRepository> _logger;

        public BasketRepository(IDistributedCache redisCacheService, ISerializeService serializeService, ILogger<BasketRepository> logger)
        {
            _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
            _serializeService = serializeService ?? throw new ArgumentNullException(nameof(serializeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Cart?> GetBasketByUserName(string username)
        {
            _logger.LogInformation("Getting basket for user {Username}", username);
            var basket = await _redisCacheService.GetStringAsync(username);
            if (string.IsNullOrEmpty(basket))
            {
                _logger.LogInformation("No basket found for user {Username}", username);
                return null;
            }
            _logger.LogInformation("Basket found for user {Username}", username);
            return _serializeService.Deserialize<Cart>(basket);
        }

        public async Task<Cart> UpdateBasket(Cart cart, DistributedCacheEntryOptions? options = null)
        {
            _logger.LogInformation("Updating basket for user {Username}", cart.Username);
            var basket = _serializeService.Serialize(cart);
            if (options == null)
            {
                await _redisCacheService.SetStringAsync(cart.Username, basket);
            }
            else
            {
                await _redisCacheService.SetStringAsync(cart.Username, basket, options);
            }
            _logger.LogInformation("Basket updated for user {Username}", cart.Username);
            return await GetBasketByUserName(cart.Username);
        }

        public async Task<bool> DeleteBasketByUserName(string username)
        {
            _logger.LogInformation("Deleting basket for user {Username}", username);
            try
            {
                await _redisCacheService.RemoveAsync(username);
                _logger.LogInformation("Basket deleted for user {Username}", username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting basket for user {Username}", username);
                return false;
            }
        }
    }
}


