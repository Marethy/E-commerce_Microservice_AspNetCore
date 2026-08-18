using Mapster;
using MediatR;
using Ordering.Application.Common.Exceptions;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models;
using Ordering.Domain.Entities;
using Serilog;
using Shared.SeedWork;
using Shared.SeedWork.ApiResult;

namespace Ordering.Application.Features.V1.Orders;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, ApiResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository, ILogger logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    private const string MethodName = "UpdateOrderCommandHandler";

    public async Task<ApiResult<OrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderEntity = await _orderRepository.GetByIdAsync(request.Id);
        if (orderEntity is null) throw new NotFoundException(nameof(Order), request.Id);

        _logger.Information($"BEGIN: {MethodName} - Order: {request.Id}");

        orderEntity = request.Adapt(orderEntity);
        var updatedOrder = await _orderRepository.UpdateOrderAsync(orderEntity);
        await _orderRepository.SaveChangesAsync();

        _logger.Information($"Order {request.Id} was successfully updated.");

        var result = updatedOrder.Adapt<OrderDto>();
        _logger.Information($"END: {MethodName} - Order: {request.Id}");

        return new ApiSuccessResult<OrderDto>(result);
    }
}