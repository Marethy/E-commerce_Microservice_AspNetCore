namespace EventBus.Messages.Events;

public interface IBasketCheckoutEvent : IIntegrationEvent
{
    string UserName { get; set; }
    decimal TotalPrice { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string EmailAddress { get; set; }
}