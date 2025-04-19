namespace EventBus.Messages;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime CreationDate { get; }
}