using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories.Interfaces;
using EventBus.MessageComponents.Consumers.Basket;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketsController(IBasketRepository repository, ILogger<BasketsController> logger, IMapper mapper, IPublishEndpoint publishEndpoint, StockItemGrpcService stockItemGrpcService) : ControllerBase
    {
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
            var basket = await repository.GetBasketByUserName(username);
            if (basket == null)
            {
                return NotFound();
            }
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
                var stock = await stockItemGrpcService.GetStock(item.ItemNo);
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

            var updatedBasket = await repository.UpdateBasket(cart, options);
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
            var deleted = await repository.DeleteBasketByUserName(username);
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

            var basket = await repository.GetBasketByUserName(basketCheckout.Username);
            if (basket == null)
            {
                return NotFound("Basket not found.");
            }

            var eventMessage = mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;

            // Publish event to RabbitMQ
            await publishEndpoint.Publish(eventMessage);

            // Clear the basket after publishing the event
            await repository.DeleteBasketByUserName(basketCheckout.Username);

            return Accepted("Checkout event has been published.");
        }
    }
}