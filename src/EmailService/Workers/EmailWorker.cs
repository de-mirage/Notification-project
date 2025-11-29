using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Models;
using NotificationService.Models.DataModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace EmailService.Workers
{
    public class EmailWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailWorker> _logger;
        private readonly IConfiguration _configuration;

        public EmailWorker(IServiceProvider serviceProvider, ILogger<EmailWorker> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Worker running...");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.GetValue<string>("RabbitMq:HostName") ?? "localhost",
                UserName = _configuration.GetValue<string>("RabbitMq:UserName") ?? "guest",
                Password = _configuration.GetValue<string>("RabbitMq:Password") ?? "guest",
                Port = _configuration.GetValue<int>("RabbitMq:Port", 5672)
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Declare the queue
            channel.QueueDeclare(queue: "email_notifications",
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                const int MaxAttempts = 3;

                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<QueueMessage>(json);

                    if (message != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<Services.EmailNotificationService>();
                        var context = scope.ServiceProvider.GetRequiredService<NotificationContext>();

                        // Get the notification record
                        var notificationRecord = await context.NotificationRecords
                            .FirstOrDefaultAsync(n => n.Id == message.NotificationId);

                        if (notificationRecord == null)
                        {
                            _logger.LogError($"Notification record not found: {message.NotificationId}");
                            channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                            return;
                        }

                        // Increment attempts
                        notificationRecord.Attempts += 1;
                        notificationRecord.LastAttempt = DateTime.UtcNow;

                        _logger.LogInformation($"Processing email notification: {message.NotificationId} (attempt {notificationRecord.Attempts}/{MaxAttempts})");

                        var success = await emailService.SendEmailAsync(message.Request);

                        if (success)
                        {
                            // Success
                            notificationRecord.Status = NotificationStatus.Sent;
                            notificationRecord.SentAt = DateTime.UtcNow;
                            _logger.LogInformation($"Successfully sent email notification: {message.NotificationId}");
                        }
                        else
                        {
                            // Failure - update error and check if we should retry
                            notificationRecord.ErrorMessage = "Failed to send email";
                            notificationRecord.Status = NotificationStatus.Failed;

                            if (notificationRecord.Attempts >= MaxAttempts)
                            {
                                // Max attempts reached - give up
                                _logger.LogError($"Maximum attempts reached for notification: {message.NotificationId}");
                                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                            }
                            else
                            {
                                // Retry
                                notificationRecord.Status = NotificationStatus.Queued;
                                _logger.LogWarning($"Failed to send email notification: {message.NotificationId}, retrying...");
                                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                            }
                        }

                        await context.SaveChangesAsync();
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        _logger.LogError("Invalid message received");
                        channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email message");
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            channel.BasicConsume(queue: "email_notifications", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Email Worker is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}
