using RabbitMQ.Client;
using Shared.Configurations;
using Shared.Messages;
using System.Text;
using System.Text.Json;

namespace Publisher.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQConfiguration _configuration;
        private readonly ConnectionFactory _factory;

        public RabbitMQService(RabbitMQConfiguration configuration)
        {
            _configuration = configuration;
            _factory = new ConnectionFactory
            {
                HostName = _configuration.Server,
                UserName = _configuration.Username,
                Password = _configuration.Password
            };
        }

        public async Task PublishAsync(CompetingConsumersMessage message)
        {
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _configuration.QueueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);
            for (int i = 0; i < 5; i++)
            {
                message.Id = i;
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                await channel.BasicPublishAsync(exchange: "",
                                            routingKey: _configuration.QueueName,
                                            body: body);
            }
            
        }
    }
}
