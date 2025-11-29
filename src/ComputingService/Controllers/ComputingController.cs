using Microsoft.AspNetCore.Mvc;
using ComputingService.Services;
using NotificationService.Models;
using NotificationService.Models.IGrains;

namespace ComputingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComputingController : ControllerBase
    {
        private readonly OrleansComputingService _computingService;
        private readonly ILogger<ComputingController> _logger;

        public ComputingController(OrleansComputingService computingService, ILogger<ComputingController> logger)
        {
            _computingService = computingService;
            _logger = logger;
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteTask([FromBody] ComputationalTask task)
        {
            if (task == null)
            {
                return BadRequest("Task cannot be null");
            }

            if (string.IsNullOrEmpty(task.Operation))
            {
                return BadRequest("Operation is required");
            }

            var result = await _computingService.ExecuteTaskAsync(task);
            
            return result.Status == "Error" 
                ? StatusCode(500, result) 
                : Ok(result);
        }

        [HttpGet("result/{taskId}")]
        public async Task<IActionResult> GetResult(Guid taskId)
        {
            var result = await _computingService.GetResultAsync(taskId);
            
            if (result.Status == "Error" && result.ErrorMessage != null)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }

        [HttpGet("status/{taskId}")]
        public async Task<IActionResult> GetStatus(Guid taskId)
        {
            var isCompleted = await _computingService.IsTaskCompletedAsync(taskId);
            
            return Ok(new { TaskId = taskId, IsCompleted = isCompleted });
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("operation"))
            {
                return BadRequest("Operation is required");
            }

            var operation = parameters["operation"].ToString() ?? string.Empty;
            var task = new ComputationalTask
            {
                TaskId = Guid.NewGuid().ToString(),
                Operation = operation,
                Parameters = new object[0]
            };

            // Extract parameters based on operation type
            if (parameters.ContainsKey("parameters") && parameters["parameters"] is object[] paramArray)
            {
                task.Parameters = paramArray;
            }
            else if (parameters.ContainsKey("x") && parameters.ContainsKey("y"))
            {
                // For simple math operations
                task.Parameters = new object[] { parameters["x"], parameters["y"] };
            }
            else if (parameters.ContainsKey("value"))
            {
                // For single parameter operations like factorial
                task.Parameters = new object[] { parameters["value"] };
            }

            var result = await _computingService.ExecuteTaskAsync(task);
            
            return result.Status == "Error" 
                ? StatusCode(500, result) 
                : Ok(result);
        }
    }
}
