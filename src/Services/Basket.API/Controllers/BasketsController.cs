using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories.Interfaces;
using Contracts.Common.Events;
using Contracts.Services;
using EventBus.MessageComponents.Consumers.Basket;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketsController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger<BasketsController> _logger;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly StockItemGrpcService _stockItemGrpcService;
        private readonly IUserActivityService _userActivityService;

        public BasketsController(
            IBasketRepository repository, 
            ILogger<BasketsController> logger, 
            IMapper mapper, 
            IPublishEndpoint publishEndpoint, 
            StockItemGrpcService stockItemGrpcService,
            IUserActivityService userActivityService)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _stockItemGrpcService = stockItemGrpcService;
            _userActivityService = userActivityService;
        }

        /// <summary>
        /// Get the basket by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Cart>> GetBasketByUserName(string username)
        {
            var basket = await _repository.GetBasketByUserName(username);
            if (basket == null)
            {
                return NotFound();
            }
            
            // 🧠 Track basket view
            await TrackActivityAsync(
                username,
                UserActivityActions.View,
                UserActivityEntityTypes.Basket,
                username,
                new Dictionary<string, object>
                {
                    ["ItemCount"] = basket.Items?.Count ?? 0,
                    ["TotalPrice"] = basket.TotalPrice
                });
            
            return Ok(basket);
        }

        /// <summary>
        /// Update the basket
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Cart>> UpdateBasket([FromBody] Cart cart)
        {
            foreach (var item in cart.Items)
            {
                var stock = await _stockItemGrpcService.GetStock(item.ItemNo);
                item.AvailableQuanlity= stock.Quantity;
            }
            if (cart == null || string.IsNullOrEmpty(cart.Username))
            {
                return BadRequest("Invalid cart data.");
            }
             
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2), // Hard limit: 2 days max
                SlidingExpiration = TimeSpan.FromDays(1) // Refresh cache on each update
            };

            var updatedBasket = await _repository.UpdateBasket(cart, options);
            
            // 🧠 Track basket update (add to cart activity)
            await TrackActivityAsync(
                cart.Username,
                UserActivityActions.AddToCart,
                UserActivityEntityTypes.Basket,
                cart.Username,
                new Dictionary<string, object>
                {
                    ["ItemCount"] = cart.Items?.Count ?? 0,
                    ["TotalPrice"] = cart.TotalPrice,
                    ["Items"] = cart.Items?.Select(i => new { i.ItemNo, i.ItemName, i.Quantity, Price = i.ItemPrice }).ToList()
                });
            
            return Ok(updatedBasket);
        }

        /// <summary>
        /// Delete the basket by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpDelete("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBasket(string username)
        {
            var deleted = await _repository.DeleteBasketByUserName(username);
            
            if (deleted)
            {
                // 🧠 Track basket deletion
                await TrackActivityAsync(
                    username,
                    UserActivityActions.Delete,
                    UserActivityEntityTypes.Basket,
                    username);
            }
            
            return deleted ? Ok() : NotFound();
        }

        [HttpPost("checkout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            if (basketCheckout == null || string.IsNullOrEmpty(basketCheckout.Username))
            {
                return BadRequest("Invalid checkout data.");
            }

            var basket = await _repository.GetBasketByUserName(basketCheckout.Username);
            if (basket == null) 
            {
                return NotFound("Basket not found.");
            }

            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;

            // Publish event to RabbitMQ
            await _publishEndpoint.Publish(eventMessage);
            
            // 🧠 Track checkout activity for AI anomaly detection & forecasting
            await TrackActivityAsync(
                basketCheckout.Username,
                UserActivityActions.Checkout,
                UserActivityEntityTypes.Basket,
                basketCheckout.Username,
                new Dictionary<string, object>
                {
                    ["TotalPrice"] = basket.TotalPrice,
                    ["ItemCount"] = basket.Items?.Count ?? 0,
                    ["ShippingAddress"] = basketCheckout.ShippingAddress,
                    ["Items"] = basket.Items?.Select(i => new { i.ItemNo, i.ItemName, i.Quantity, Price = i.ItemPrice }).ToList()
                });
            
            // Clear the basket after publishing the event
            await _repository.DeleteBasketByUserName(basketCheckout.Username);
            return Accepted("Checkout event has been published.");
        }
        
        /// <summary>
        /// Helper method to track user activities for AI analytics
        /// </summary>
        private async Task TrackActivityAsync(
            string username,
            string action,
            string entityType,
            string entityId,
            Dictionary<string, object> metadata = null)
        {
            try
            {
                var correlationId = HttpContext.Items["X-Correlation-Id"]?.ToString();
                
                await _userActivityService.TrackActivityAsync(
                    username,
                    entityType,
                    entityId,
                    action,
                    correlationId,
                    metadata);
            }
            catch (Exception ex)
            {
                // Don't fail the request if activity tracking fails
                _logger.LogWarning(ex, "Failed to track user activity for {Action} on {EntityType}/{EntityId}", 
                    action, entityType, entityId);
            }
        }
    }
}