using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders;
using Ordering.Application.Features.V1.Orders.Queries.GetAverageOrderValue;
using Ordering.Application.Features.V1.Orders.Queries.GetDailyRevenue;
using Ordering.Application.Features.V1.Orders.Queries.GetOrders;
using Ordering.Application.Features.V1.Orders.Queries.GetRevenueByStatus;
using Shared.SeedWork.ApiResult;
using System.Net;

namespace Ordering.API.Controllers;

[Route("api/v1/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("users/{userName}")]
    [ProducesResponseType(typeof(ApiResult<List<OrderDto>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ApiResult<List<OrderDto>>>> GetOrdersByUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return BadRequest(new ApiErrorResult<List<OrderDto>>("Username is required"));

        var query = new GetOrderQuery(userName);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResult<OrderDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ApiResult<OrderDto>>> GetOrderById(long id)
    {
        if (id <= 0)
            return BadRequest(new ApiErrorResult<OrderDto>("Invalid order ID"));

        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<long>), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ApiResult<long>>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Data }, result);
     
        return BadRequest(result);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResult<OrderDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ApiResult<OrderDto>>> UpdateOrder(long id, [FromBody] UpdateOrderCommand command)
    {
        if (id <= 0)
            return BadRequest(new ApiErrorResult<OrderDto>("Invalid order ID"));

        command.SetId(id);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> DeleteOrder(long id)
    {
        if (id <= 0)
            return BadRequest(new ApiErrorResult<object>("Invalid order ID"));

        var command = new DeleteOrderCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("admin")]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<object>>> GetAllOrders(
        [FromQuery] int page = 0,
        [FromQuery] int limit = 20,
        [FromQuery] string? status = null)
    {
        var query = new GetAllOrdersQuery(page, limit, status);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("{id:long}/status")]
    [ProducesResponseType(typeof(ApiResult<OrderDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<OrderDto>>> UpdateOrderStatus(
        long id,
        [FromBody] UpdateOrderStatusCommand command)
    {
        if (id <= 0)
            return BadRequest(new ApiErrorResult<OrderDto>("Invalid order ID"));
        
        command.SetId(id);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id:long}/cancel")]
    [ProducesResponseType(typeof(ApiResult<OrderDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ApiResult<OrderDto>>> CancelOrder(
        long id,
        [FromBody] CancelOrderCommand? command = null)
    {
        if (id <= 0)
            return BadRequest(new ApiErrorResult<OrderDto>("Invalid order ID"));
        
        var cancelCommand = command ?? new CancelOrderCommand();
        cancelCommand.SetId(id);
        var result = await _mediator.Send(cancelCommand);
        return Ok(result);
    }

    [HttpGet("admin/stats")]
    [ProducesResponseType(typeof(ApiResult<OrderStatistics>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<OrderStatistics>>> GetOrderStatistics()
    {
        var query = new GetOrderStatisticsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("admin/analytics/revenue-daily")]
    [ProducesResponseType(typeof(ApiResult<List<DailyRevenueDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<DailyRevenueDto>>>> GetDailyRevenue(
        [FromQuery] int days = 30)
    {
        var query = new GetDailyRevenueQuery(days);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("admin/analytics/revenue-by-status")]
    [ProducesResponseType(typeof(ApiResult<List<RevenueByStatusDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<RevenueByStatusDto>>>> GetRevenueByStatus()
    {
        var query = new GetRevenueByStatusQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("admin/analytics/avg-order-value")]
    [ProducesResponseType(typeof(ApiResult<List<AverageOrderValueDto>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<List<AverageOrderValueDto>>>> GetAverageOrderValue(
        [FromQuery] int days = 30)
    {
        var query = new GetAverageOrderValueQuery(days);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // Temporary debug endpoint - remove after finding issue
    [HttpGet("admin/analytics/debug/statuses")]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<object>>> DebugOrderStatuses([FromServices] Ordering.Application.Common.Interfaces.IOrderRepository orderRepo)
    {
        var allOrders = await orderRepo.FindAll().ToListAsync();
        var statusCounts = allOrders
            .GroupBy(o => (int)o.Status)
            .Select(g => new { 
                StatusValue = g.Key, 
                StatusName = ((Shared.Enums.Order.OrderStatus)g.Key).ToString(), 
                Count = g.Count() 
            })
            .OrderBy(x => x.StatusValue)
            .ToList();
        
        var result = new { 
            TotalOrders = allOrders.Count, 
            StatusBreakdown = statusCounts 
        };
        
        return Ok(new ApiSuccessResult<object>(result));
    }

    [HttpGet("check-purchase/{productNo}")]
    [ProducesResponseType(typeof(ApiResult<object>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ApiResult<object>>> CheckUserPurchase(
        string productNo,
        [FromQuery] string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return BadRequest(new ApiErrorResult<object>("Username is required"));
            
        var query = new CheckUserPurchaseQuery(userName, productNo);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
