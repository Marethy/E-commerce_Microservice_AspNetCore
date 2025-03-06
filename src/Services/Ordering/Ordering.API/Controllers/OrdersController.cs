using Contracts.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders.Commands.CreateOrder;
using Ordering.Application.Features.V1.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.V1.Orders.Queries.GetOrders;
using Shared.Services.Email;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Ordering.API.Controllers
{
    [Route("api/v1/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISMTPEmailService<MailRequest> _emailService;

        public OrdersController(IMediator mediator, ISMTPEmailService<MailRequest> emailService)
        {
            _mediator = mediator;
            _emailService = emailService;
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

        /// <summary>
        /// Tạo đơn hàng mới.
        /// </summary>
        /// <param name="command">Thông tin đơn hàng.</param>
        /// <returns>Kết quả tạo đơn hàng.</returns>
        [HttpPost("create-order")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        /// <summary>
        /// Cập nhật đơn hàng.
        /// </summary>
        /// <param name="id">ID của đơn hàng.</param>
        /// <param name="command">Thông tin cập nhật đơn hàng.</param>
        /// <returns>Kết quả cập nhật đơn hàng.</returns>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateOrder(long id, [FromBody] UpdateOrderCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("Order ID mismatch.");
            }

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        /// <summary>
        /// Gửi email thử nghiệm.
        /// </summary>
        /// <param name="request">Thông tin email.</param>
        /// <returns>Kết quả gửi email.</returns>
        [HttpPost("send-test-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendTestEmail([FromBody] MailRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _emailService.SendEmailAsync(request);
            return Ok("Test email sent successfully.");
        }
    }
}
