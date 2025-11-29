namespace NotificationService.Models
{
    public interface IRabbitMqService
    {
        void PublishMessage<T>(T message, string queueName) where T : class;
        void SubscribeToQueue<T>(string queueName, Func<T, Task> handler) where T : class;
        void SetupQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
    }
}
