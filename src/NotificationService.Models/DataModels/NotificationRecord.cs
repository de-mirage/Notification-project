using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.DataModels
{
    public class NotificationRecord
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        public string Recipient { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType NotificationType { get; set; }

        public PriorityLevel Priority { get; set; } = PriorityLevel.Normal;

        public NotificationStatus Status { get; set; } = NotificationStatus.Queued;

        public int Attempts { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        public DateTime? LastAttempt { get; set; }

        public string? ErrorMessage { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }
}
