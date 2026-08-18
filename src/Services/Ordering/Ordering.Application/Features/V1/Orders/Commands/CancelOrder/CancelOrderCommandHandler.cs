using Mapster;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<OrderDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id);
        if (order == null)
            return new ApiErrorResult<OrderDto>($"Order with ID {request.Id} not found");

        var cancelled = await _orderRepository.CancelOrderAsync(request.Id, request.Reason);
        if (!cancelled)
            return new ApiErrorResult<OrderDto>("Cannot cancel order - only Pending or Confirmed orders can be cancelled");

        order = await _orderRepository.GetByIdAsync(request.Id);
        var result = order.Adapt<OrderDto>();
        return new ApiSuccessResult<OrderDto>(result);
    }
}

