using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class NotificationRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("recipient")]
        public string Recipient { get; set; } = string.Empty;

        [JsonProperty("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("notificationType")]
        public NotificationType NotificationType { get; set; }

        [JsonProperty("priority")]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Normal;

        [JsonProperty("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        [JsonProperty("attachments")]
        public List<Attachment>? Attachments { get; set; }

        [JsonProperty("scheduledTime")]
        public DateTime? ScheduledTime { get; set; }

        [JsonProperty("expiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Attachment
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonProperty("data")]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    public enum NotificationType
    {
        Email,
        Sms,
        Push,
        Slack,
        Discord,
        Webhook
    }

    public enum PriorityLevel
    {
        Low,
        Normal,
        High,
        Critical
    }
}
