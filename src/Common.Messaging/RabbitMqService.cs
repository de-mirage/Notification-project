using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Common.Messaging
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqService> _logger;
        private readonly string _hostName;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;

        public RabbitMqService(ILogger<RabbitMqService> logger, string hostName = "localhost", int port = 5672, string userName = "guest", string password = "guest")
        {
            _logger = logger;
            _hostName = hostName;
            _port = port;
            _userName = userName;
            _password = password;

            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
                Port = _port
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void SetupQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: durable,
                                 exclusive: exclusive,
                                 autoDelete: autoDelete,
                                 arguments: null);
            
            // Set QoS to process one message at a time per consumer
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            
            _logger.LogInformation($"Queue '{queueName}' declared.");
        }

        public void PublishMessage<T>(T message, string queueName) where T : class
        {
            SetupQueue(queueName);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($"Message published to queue '{queueName}': {json}");
        }

        public void SubscribeToQueue<T>(string queueName, Func<T, Task> handler) where T : class
        {
            SetupQueue(queueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(json);

                    if (message != null)
                    {
                        await handler(message);
                    }

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue '{QueueName}'", queueName);
                    
                    // Reject and requeue the message
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            _logger.LogInformation($"Subscribed to queue '{queueName}'");
        }

        public IConnection CreateConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
                Port = _port
            };

            return factory.CreateConnection();
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
