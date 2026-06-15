namespace Publisher.Services
{
    public interface IRabbitMQService
    {
        Task PublishAsync<T>(T message);
    }
}
