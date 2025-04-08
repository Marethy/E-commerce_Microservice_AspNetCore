using MediatR;
using Ordering.Application.Common.Exceptions;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Serilog;

namespace Ordering.Application.Features.V1.Orders;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    private const string MethodName = "DeleteOrderCommandHandler";

    public async Task Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var orderEntity = await _orderRepository.GetByIdAsync(request.Id);
        if (orderEntity is null) throw new NotFoundException(nameof(Order), request.Id);

        _logger.Information($"BEGIN: {MethodName} - Order: {request.Id}");

        _orderRepository.DeleteOrder(orderEntity);
        orderEntity.DeleteOrder();
        await _orderRepository.SaveChangeAsync();

        _logger.Information($"Order {request.Id} was successfully deleted.");

        _logger.Information($"END: {MethodName} - Order: {request.Id}");
    }
}