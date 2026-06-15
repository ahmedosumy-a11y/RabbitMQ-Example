using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Configurations;
using System.Text;

namespace Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var rabbitMQConfig = configuration
                .GetSection(nameof(RabbitMQConfiguration))
                .Get<RabbitMQConfiguration>()
                ?? throw new InvalidOperationException("RabbitMQConfiguration section is missing.");

            var factory = new ConnectionFactory
            {
                HostName = rabbitMQConfig.Server,
                UserName = rabbitMQConfig.Username,
                Password = rabbitMQConfig.Password
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: rabbitMQConfig.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += (sender, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message Received: {message}");

                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: rabbitMQConfig.QueueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Consumer started. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
