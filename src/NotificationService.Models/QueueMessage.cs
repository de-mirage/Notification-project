using Newtonsoft.Json;

namespace NotificationService.Models
{
    public class QueueMessage
    {
        [JsonProperty("notificationId")]
        public string NotificationId { get; set; } = string.Empty;

        [JsonProperty("request")]
        public NotificationRequest Request { get; set; } = new();

        [JsonProperty("attemptCount")]
        public int AttemptCount { get; set; } = 0;

        [JsonProperty("createdTime")]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

        [JsonProperty("nextAttemptTime")]
        public DateTime? NextAttemptTime { get; set; }
    }
}
