using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
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

        public BasketsController(IBasketRepository repository, ILogger<BasketsController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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

        [HttpPost]
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

        [HttpDelete("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBasket(string username)
        {
            var deleted = await _repository.DeleteBasketByUserName(username);
            return deleted ? Ok() : NotFound();
        }
    }
}


