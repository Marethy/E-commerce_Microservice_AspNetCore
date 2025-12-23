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
using Shared.SeedWork.ApiResult;
using System.Net;

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
        [ProducesResponseType(typeof(ApiResult<Cart>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<Cart>>> GetBasketByUserName(string username)
        {
            var basket = await _repository.GetBasketByUserName(username) ?? new Cart(username);
            
            await TrackActivityAsync(username, UserActivityActions.View, UserActivityEntityTypes.Basket, username,
                new Dictionary<string, object>
                {
                    ["ItemCount"] = basket.Items?.Count ?? 0,
                    ["TotalPrice"] = basket.TotalPrice
                });
    
            return Ok(new ApiSuccessResult<Cart>(basket));
        }

        /// <summary>
        /// Update the basket
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResult<Cart>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<ApiResult<Cart>>> UpdateBasket([FromBody] Cart cart)
        {
            foreach (var item in cart.Items)
            {
                var stock = await _stockItemGrpcService.GetStock(item.ItemNo);
                item.AvailableQuanlity = stock.Quantity;
            }

            if (cart == null || string.IsNullOrEmpty(cart.Username))
                return BadRequest(new ApiErrorResult<Cart>("Invalid cart data"));
 
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2),
                SlidingExpiration = TimeSpan.FromDays(1)
            };

            var updatedBasket = await _repository.UpdateBasket(cart, options);
            
            // 🧠 Track basket update (add to cart activity)
            await TrackActivityAsync(cart.Username, UserActivityActions.AddToCart, UserActivityEntityTypes.Basket, cart.Username,
                new Dictionary<string, object>
                {
                    ["ItemCount"] = cart.Items?.Count ?? 0,
                    ["TotalPrice"] = cart.TotalPrice,
                    ["Items"] = cart.Items?.Select(i => new { i.ItemNo, i.ItemName, i.Quantity, Price = i.ItemPrice }).ToList()
                });
            
            return Ok(new ApiSuccessResult<Cart>(updatedBasket));
        }

        /// <summary>
        /// Delete the basket by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpDelete("{username}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteBasket(string username)
        {
            var deleted = await _repository.DeleteBasketByUserName(username);
     
            if (deleted)
            {
                // 🧠 Track basket deletion
                await TrackActivityAsync(username, UserActivityActions.Delete, UserActivityEntityTypes.Basket, username);
                return NoContent();
            }
            
            return NotFound(new ApiErrorResult<object>($"Basket for user '{username}' not found"));
        }

        /// <summary>
        /// Checkout and publish checkout event
        /// </summary>
        /// <param name="basketCheckout"></param>
        /// <returns></returns>
        [HttpPost("checkout")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            if (basketCheckout == null || string.IsNullOrEmpty(basketCheckout.Username))
                return BadRequest(new ApiErrorResult<object>("Invalid checkout data"));

            var basket = await _repository.GetBasketByUserName(basketCheckout.Username);
            if (basket == null)
                return NotFound(new ApiErrorResult<object>("Basket not found"));

            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;

            // Publish event to RabbitMQ
            await _publishEndpoint.Publish(eventMessage);
            
            // 🧠 Track checkout activity for AI anomaly detection & forecasting
            await TrackActivityAsync(basketCheckout.Username, UserActivityActions.Checkout, UserActivityEntityTypes.Basket, basketCheckout.Username,
                new Dictionary<string, object>
                {
                    ["TotalPrice"] = basket.TotalPrice,
                    ["ItemCount"] = basket.Items?.Count ?? 0,
                    ["ShippingAddress"] = basketCheckout.ShippingAddress,
                    ["Items"] = basket.Items?.Select(i => new { i.ItemNo, i.ItemName, i.Quantity, Price = i.ItemPrice }).ToList()
                });
      
            // Clear the basket after publishing the event
            await _repository.DeleteBasketByUserName(basketCheckout.Username);
            return Accepted(new ApiSuccessResult<object>("Checkout event has been published"));
        }
        
        /// <summary>
        /// Get cart item count for badge display
        /// </summary>
        [HttpGet("{username}/count")]
        [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<object>>> GetCartItemCount(string username)
        {
            var basket = await _repository.GetBasketByUserName(username);
            var count = basket?.Items?.Sum(i => i.Quantity) ?? 0;
            return Ok(new ApiSuccessResult<object>(new { count }));
        }

        /// <summary>
        /// Validate cart before checkout - check stock availability
        /// </summary>
        [HttpGet("{username}/validate")]
        [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ApiResult<object>>> ValidateCart(string username)
        {
            var basket = await _repository.GetBasketByUserName(username);
            if (basket == null || basket.Items == null || !basket.Items.Any())
            {
                return Ok(new ApiSuccessResult<object>(new 
                { 
                    isValid = false, 
                    message = "Cart is empty",
                    issues = new[] { "Cart is empty" }
                }));
            }

            var issues = new List<string>();
            var invalidItems = new List<object>();

            foreach (var item in basket.Items)
            {
                try
                {
                    var stock = await _stockItemGrpcService.GetStock(item.ItemNo);
                    
                    if (stock.Quantity <= 0)
                    {
                        issues.Add($"{item.ItemName} is out of stock");
                        invalidItems.Add(new { itemNo = item.ItemNo, itemName = item.ItemName, issue = "OUT_OF_STOCK" });
                    }
                    else if (item.Quantity > stock.Quantity)
                    {
                        issues.Add($"Only {stock.Quantity} of {item.ItemName} available (requested: {item.Quantity})");
                        invalidItems.Add(new 
                        { 
                            itemNo = item.ItemNo, 
                            itemName = item.ItemName, 
                            issue = "INSUFFICIENT_STOCK",
                            requested = item.Quantity,
                            available = stock.Quantity
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to validate stock for item {ItemNo}", item.ItemNo);
                    issues.Add($"Failed to validate {item.ItemName}");
                    invalidItems.Add(new { itemNo = item.ItemNo, itemName = item.ItemName, issue = "VALIDATION_ERROR" });
                }
            }

            var isValid = !issues.Any();
            
            return Ok(new ApiSuccessResult<object>(new 
            { 
                isValid, 
                message = isValid ? "Cart is valid" : "Cart has issues",
                issues,
                invalidItems,
                totalItems = basket.Items.Count,
                totalPrice = basket.TotalPrice
            }));
        }

        /// <summary>
        /// Merge guest cart with user cart after login
        /// </summary>
        [HttpPost("{username}/merge")]
        [ProducesResponseType(typeof(ApiResult<Cart>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<ApiResult<Cart>>> MergeGuestCart(string username, [FromBody] Cart guestCart)
        {
            if (guestCart == null || guestCart.Items == null || !guestCart.Items.Any())
                return BadRequest(new ApiErrorResult<Cart>("Guest cart is empty"));

          var userCart = await _repository.GetBasketByUserName(username) ?? new Cart { Username = username, Items = new List<CartItem>() };

            foreach (var guestItem in guestCart.Items)
          {
      var existingItem = userCart.Items.FirstOrDefault(i => i.ItemNo == guestItem.ItemNo);
    
       if (existingItem != null)
  {
      existingItem.Quantity += guestItem.Quantity;
         var stock = await _stockItemGrpcService.GetStock(existingItem.ItemNo);
     
        if (existingItem.Quantity > stock.Quantity)
          {
  existingItem.Quantity = stock.Quantity;
          _logger.LogWarning("Capped quantity for {ItemNo} to {Quantity} due to stock limit", 
        existingItem.ItemNo, stock.Quantity);
     }
       existingItem.AvailableQuanlity = stock.Quantity;
                }
          else
             {
  var stock = await _stockItemGrpcService.GetStock(guestItem.ItemNo);
             guestItem.AvailableQuanlity = stock.Quantity;
        
     if (guestItem.Quantity > stock.Quantity)
    guestItem.Quantity = stock.Quantity;
            
           userCart.Items.Add(guestItem);
                }
    }

            var options = new DistributedCacheEntryOptions
 {
         AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2),
              SlidingExpiration = TimeSpan.FromDays(1)
        };

      var mergedCart = await _repository.UpdateBasket(userCart, options);
            
  await TrackActivityAsync(username, UserActivityActions.MergeCart, UserActivityEntityTypes.Basket, username,
              new Dictionary<string, object>
          {
           ["GuestItemCount"] = guestCart.Items.Count,
     ["MergedItemCount"] = mergedCart.Items?.Count ?? 0,
       ["TotalPrice"] = mergedCart.TotalPrice
    });

     return Ok(new ApiSuccessResult<Cart>(mergedCart));
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