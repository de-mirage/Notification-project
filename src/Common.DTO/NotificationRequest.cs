using System.Text.Json.Serialization;

namespace Common.DTO
{
    public class NotificationRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("recipient")]
        public string Recipient { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty; // email, sms, push, etc.

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        [JsonPropertyName("attachments")]
        public List<Attachment>? Attachments { get; set; }

        [JsonPropertyName("scheduledTime")]
        public DateTime? ScheduledTime { get; set; }

        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Attachment
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
