using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
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
        public BasketsController(IBasketRepository repository, ILogger<BasketsController> logger, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
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

            // Clear the basket after publishing the event
            await _repository.DeleteBasketByUserName(basketCheckout.Username);

            return Accepted("Checkout event has been published.");
        }
    }
}