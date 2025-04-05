using Contracts.Common.Interfaces;
using Contracts.Messages;
using RabbitMQ.Client;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure.Messages
{
    public class RabbitMQProducer(ISerializeService serializeService) : IMessageProducer
    {
        private readonly ISerializeService _serializeService = serializeService;



        public async Task SendMessageAsync<T>(T message)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            var connection = await connectionFactory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("orders", exclusive: false);

            var jsonData = _serializeService.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonData);

            await channel.BasicPublishAsync(exchange: "", routingKey: "orders", body: body);
        }

    }
}