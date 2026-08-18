using EventBus.MessageComponents.Consumers.Basket;
using Mapster;
using MassTransit;
using MediatR;
using Ordering.Application.Features.V1.Orders;
using ILogger = Serilog.ILogger;

namespace Ordering.API.EventBusConsumer;

public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public BasketCheckoutConsumer(IMediator mediator, ILogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var command = context.Message.Adapt<CreateOrderCommand>();
        var result = await _mediator.Send(command);
        _logger.Information($"BasketCheckoutEvent consumed successfully. Order is created with Id: {result.Data}");
    }
}