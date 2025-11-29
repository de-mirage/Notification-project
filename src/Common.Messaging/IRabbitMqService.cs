using RabbitMQ.Client;

namespace Common.Messaging
{
    public interface IRabbitMqService
    {
        void SetupQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
        void PublishMessage<T>(T message, string queueName) where T : class;
        void SubscribeToQueue<T>(string queueName, Func<T, Task> handler) where T : class;
        IConnection CreateConnection();
    }
}
