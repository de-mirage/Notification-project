using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RouterService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IConnection? _connection;
    private IModel? _channel;  // Use IModel instead of IChannel for RabbitMQ.Client API
    private readonly string _inputQueue = "notifications";
    private readonly string _emailQueue = "email_notifications";
    private readonly string _smsQueue = "sms_notifications";
    private readonly string _pushQueue = "push_notifications";
    private readonly string _slackQueue = "slack_notifications";
    private readonly string _discordQueue = "discord_notifications";
    private readonly string _webhookQueue = "webhook_notifications";

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    private async Task InitializeRabbitMq()
    {
        var factory = new ConnectionFactory()
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };

        int maxRetries = 15;
        int retryDelay = 10000; // 10 seconds
        int attempts = 0;

        while (attempts < maxRetries)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to RabbitMQ... Attempt {Attempt} of {MaxRetries}", attempts + 1, maxRetries);
                _logger.LogInformation("RabbitMQ Connection Parameters: Host={Host}, Port={Port}, User={User}", 
                    factory.HostName, factory.Port, factory.UserName);

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare input queue
                _channel.QueueDeclare(queue: _inputQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                // Declare output queues
                _channel.QueueDeclare(queue: _emailQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: _smsQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: _pushQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: _slackQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: _discordQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: _webhookQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("Successfully connected to RabbitMQ");
                break; // Exit the retry loop on success
            }
            catch (Exception ex)
            {
                attempts++;
                _logger.LogWarning(ex, "Attempt {Attempt} to connect to RabbitMQ failed. Retrying in {Delay}ms...", attempts, retryDelay);

                if (attempts >= maxRetries)
                {
                    _logger.LogError("Max retries reached. Unable to connect to RabbitMQ.");
                    throw;
                }

                _logger.LogInformation("Waiting {0} seconds before next attempt...", retryDelay / 1000);
                await Task.Delay(retryDelay);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMq();
        
        _logger.LogInformation("Router Service started");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                var notificationRequest = JsonSerializer.Deserialize<NotificationRequest>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (notificationRequest != null)
                {
                    RouteNotification(notificationRequest);
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification message");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: _inputQueue, autoAck: false, consumer: consumer);

        _logger.LogInformation("Router Service is listening for notifications...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

    private void RouteNotification(NotificationRequest notification)
    {
        try
        {
            _logger.LogInformation("Routing notification: {NotificationId}, Type: {Type}",
                notification.Id, notification.NotificationType);

            string targetQueue = notification.NotificationType switch
            {
                NotificationType.Email => _emailQueue,
                NotificationType.Sms => _smsQueue,
                NotificationType.Push => _pushQueue,
                NotificationType.Slack => _slackQueue,
                NotificationType.Discord => _discordQueue,
                NotificationType.Webhook => _webhookQueue,
                _ => throw new ArgumentException($"Unknown notification type: {notification.NotificationType}")
            };

            var message = JsonSerializer.Serialize(notification);
            var body = Encoding.UTF8.GetBytes(message);

            _channel!.BasicPublish(exchange: "", routingKey: targetQueue, basicProperties: null, body: body);

            _logger.LogInformation("Notification {NotificationId} routed to {Queue}",
                notification.Id, targetQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing notification {NotificationId}", notification.Id);
        }
    }
}
