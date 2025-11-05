using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders;
using Ordering.Application.Features.V1.Orders.Queries.GetOrders;
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
}