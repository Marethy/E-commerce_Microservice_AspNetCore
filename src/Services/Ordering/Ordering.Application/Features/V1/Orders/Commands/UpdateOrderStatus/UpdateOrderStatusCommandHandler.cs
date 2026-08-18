using Mapster;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, ApiResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<ApiResult<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.UpdateOrderStatusAsync(request.Id, request.Status);
        if (order == null)
            return new ApiErrorResult<OrderDto>("Order not found or invalid status");

        var result = order.Adapt<OrderDto>();
        return new ApiSuccessResult<OrderDto>(result);
    }
}
