using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using NotificationService.Models.DataModels;

namespace NotificationGateway.Services
{
    public class NotificationService
    {
        private readonly NotificationContext _context;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            NotificationContext context, 
            IRabbitMqService rabbitMqService,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        public async Task<NotificationResponse> SendNotificationAsync(NotificationRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Recipient))
                {
                    _logger.LogWarning("Invalid notification request: Recipient is required");
                    
                    return new NotificationResponse
                    {
                        Id = request.Id,
                        Status = NotificationStatus.Failed,
                        Message = "Recipient is required",
                        Timestamp = DateTime.UtcNow
                    };
                }

                // Create notification record
                var notificationRecord = new NotificationRecord
                {
                    Id = request.Id,
                    Recipient = request.Recipient,
                    Subject = request.Subject,
                    Message = request.Message,
                    NotificationType = request.NotificationType,
                    Priority = request.Priority,
                    Status = NotificationStatus.Queued,
                    CreatedAt = DateTime.UtcNow,
                    Metadata = request.Metadata
                };

                _context.NotificationRecords.Add(notificationRecord);
                await _context.SaveChangesAsync();

                // Create queue message
                var queueMessage = new QueueMessage
                {
                    NotificationId = request.Id,
                    Request = request,
                    AttemptCount = 0,
                    CreatedTime = DateTime.UtcNow
                };

                // Publish to appropriate queue based on notification type
                string queueName = GetQueueName(request.NotificationType);
                _rabbitMqService.PublishMessage(queueMessage, queueName);

                _logger.LogInformation($"Notification {request.Id} queued for {request.NotificationType} delivery to {request.Recipient}");

                return new NotificationResponse
                {
                    Id = request.Id,
                    Status = NotificationStatus.Queued,
                    Message = "Notification queued for delivery",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing notification {NotificationId}", request.Id);

                return new NotificationResponse
                {
                    Id = request.Id,
                    Status = NotificationStatus.Failed,
                    Message = $"Error queuing notification: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<NotificationRecord?> GetNotificationStatusAsync(string notificationId)
        {
            return await _context.NotificationRecords
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        private string GetQueueName(NotificationType notificationType)
        {
            return notificationType switch
            {
                NotificationType.Email => "email_notifications",
                NotificationType.Sms => "sms_notifications",
                NotificationType.Push => "push_notifications",
                NotificationType.Slack => "slack_notifications",
                NotificationType.Discord => "discord_notifications",
                NotificationType.Webhook => "webhook_notifications",
                _ => "general_notifications"
            };
        }
    }
}
