using Newtonsoft.Json;

namespace NotificationService.Models
{
    public class NotificationResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("status")]
        public NotificationStatus Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonProperty("attempts")]
        public int Attempts { get; set; } = 0;

        [JsonProperty("lastAttempt")]
        public DateTime? LastAttempt { get; set; }
    }

    public enum NotificationStatus
    {
        Queued,
        Processing,
        Sent,
        Failed,
        Delivered,
        Expired
    }
}
