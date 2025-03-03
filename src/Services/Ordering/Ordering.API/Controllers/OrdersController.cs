using Contracts.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders.Queries.GetOrders;
using Shared.Services.Email;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Ordering.API.Controllers
{
    [Route("api/orders")]
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
        /// Gửi email thử nghiệm.
        /// </summary>
        /// <param name="request">Thông tin email.</param>
        /// <returns>Kết quả gửi email.</returns>
        [HttpPost("send-test-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendTestEmail()
        {
            var message = new MailRequest
            {
                ToAddress = "thienkhiem2604@gmail.com",
                Subject = "Test email",
                Body = "This is a test email.",
            };

            await _emailService.SendEmailAsync(message);
            return Ok("Test email sent successfully.");
        }
    }
}

