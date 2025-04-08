using MassTransit;

namespace EventBus.MessageComponents.Consumers.Basket;

public interface IBasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
{
}