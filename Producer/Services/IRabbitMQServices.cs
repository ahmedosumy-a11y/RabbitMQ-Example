using Shared.Messages;

namespace Publisher.Services
{
    public interface IRabbitMQService
    {
        Task PublishAsync(CompetingConsumersMessage message);
    }
}
