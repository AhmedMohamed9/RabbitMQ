namespace Producer.services
{
    public interface IRabbitMQService
    {
        public Task Publish<T>(T message);
    }
}
