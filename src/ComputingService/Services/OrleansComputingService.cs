using NotificationService.Models;
using NotificationService.Models.IGrains;

namespace ComputingService.Services
{
    public class OrleansComputingService
    {
        private readonly IGrainFactory _grainFactory;
        private readonly ILogger<OrleansComputingService> _logger;

        public OrleansComputingService(IGrainFactory grainFactory, ILogger<OrleansComputingService> logger)
        {
            _grainFactory = grainFactory;
            _logger = logger;
        }

        public async Task<ComputationalResult> ExecuteTaskAsync(ComputationalTask task)
        {
            try
            {
                // Create a grain with a unique ID
                var grainId = Guid.NewGuid();
                var grain = _grainFactory.GetGrain<IComputationalGrain>(grainId);

                _logger.LogInformation($"Executing task {task.TaskId} on grain {grainId}");

                var result = await grain.ExecuteTaskAsync(task);

                _logger.LogInformation($"Task {task.TaskId} completed with status: {result.Status}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing task {TaskId}", task.TaskId);
                
                return new ComputationalResult
                {
                    TaskId = task.TaskId,
                    Status = "Error",
                    ErrorMessage = ex.Message,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<ComputationalResult> GetResultAsync(Guid taskId)
        {
            try
            {
                var grain = _grainFactory.GetGrain<IComputationalGrain>(taskId);
                var result = await grain.GetResultAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting result for task {TaskId}", taskId);
                
                return new ComputationalResult
                {
                    TaskId = taskId.ToString(),
                    Status = "Error",
                    ErrorMessage = ex.Message,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> IsTaskCompletedAsync(Guid taskId)
        {
            try
            {
                var grain = _grainFactory.GetGrain<IComputationalGrain>(taskId);
                var isCompleted = await grain.IsCompletedAsync();

                return isCompleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking completion status for task {TaskId}", taskId);
                return false;
            }
        }
    }
}
