using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders.Queries.GetOrders;
using System.ComponentModel.DataAnnotations;

namespace Ordering.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng theo username.
        /// </summary>
        /// <param name="username">Tên người dùng (bắt buộc).</param>
        /// <returns>Danh sách đơn hàng.</returns>
        [HttpGet("{username}")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUsername([Required] string username)
        {
            var query = new GetOrderQuery(username);
            var orders = await _mediator.Send(query);
            return Ok(orders);
        }
    }
}
