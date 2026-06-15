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

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            var random = new Random();
            

            consumer.ReceivedAsync += (sender, args) =>
            {
                var processingTime = random.Next(1, 6);
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Message Received: {message}\n will take {processingTime} to process");
                Thread.Sleep(TimeSpan.FromSeconds(processingTime));
                channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false);
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: rabbitMQConfig.QueueName, autoAck: false, consumer: consumer);

            Console.WriteLine("Consumer started. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
