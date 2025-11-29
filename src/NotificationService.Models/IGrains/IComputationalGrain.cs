using Orleans;

namespace NotificationService.Models.IGrains
{
    public interface IComputationalGrain : IGrainWithGuidKey
    {
        Task<ComputationalResult> ExecuteTaskAsync(ComputationalTask task);
        Task<ComputationalResult> GetResultAsync();
        Task<bool> IsCompletedAsync();
    }

    public class ComputationalTask
    {
        public string TaskId { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public object[] Parameters { get; set; } = Array.Empty<object>();
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class ComputationalResult
    {
        public string TaskId { get; set; } = string.Empty;
        public object? Result { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
