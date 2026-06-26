using RabbitMQ.Client;
using Shared.Configuration;
using System.Text;
using System.Text.Json;

namespace Producer.services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQConfiguration _rabbitmqConfiguration;
        private readonly ConnectionFactory _factory;
        public RabbitMQService(RabbitMQConfiguration rabbitmqConfiguration)
        {
            _rabbitmqConfiguration = rabbitmqConfiguration;
            _factory = new ConnectionFactory()
            {
                HostName = _rabbitmqConfiguration.Server,
                UserName = _rabbitmqConfiguration.UserName,
                Password = _rabbitmqConfiguration.Password
            };

        }

        public async Task Publish<T>(T message)
        {
            using var connection = await _factory.CreateConnectionAsync();

            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: _rabbitmqConfiguration.ExchangeName,
                type: ExchangeType.Fanout
            );

                //await channel.QueueDeclareAsync(queue: _rabbitmqConfiguration.QueueName,
                //    durable: true,
                //    exclusive: false,
                //    autoDelete: false,
                //    arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await channel.BasicPublishAsync(
                exchange: _rabbitmqConfiguration.ExchangeName,                  // الـ Default Exchange مجهول الاسم
                routingKey: "", // في الـ Default Exchange، الـ routingKey هو نفسه اسم الطابور
                body: body
            );
        }
    }
}
