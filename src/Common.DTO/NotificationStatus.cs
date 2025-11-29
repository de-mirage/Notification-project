using System.Text.Json.Serialization;

namespace Common.DTO
{
    public class NotificationStatus
    {
        [JsonPropertyName("notificationId")]
        public string NotificationId { get; set; } = string.Empty;

        [JsonPropertyName("serviceType")]
        public string ServiceType { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // Pending, Queued, Processing, Sent, Failed

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("sentAt")]
        public DateTime? SentAt { get; set; }

        [JsonPropertyName("retryCount")]
        public int RetryCount { get; set; } = 0;
    }
}
