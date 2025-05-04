namespace Contracts.Messages
{
    public interface IMessageProducer
    {
        Task SendMessageAsync<T>(T message);
    }
}