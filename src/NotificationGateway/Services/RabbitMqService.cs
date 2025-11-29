using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NotificationGateway.Services
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(ILogger<RabbitMqService> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest",
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")
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
            // Send all notifications to a centralized queue for routing
            string targetQueue = "notifications";

            SetupQueue(targetQueue);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "",
                                 routingKey: targetQueue,
                                 basicProperties: null,
                                 body: body);

            _logger.LogInformation($"Message published to queue '{targetQueue}': {json}");
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

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
