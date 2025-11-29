using Microsoft.AspNetCore.Mvc;
using NotificationGateway.Services;
using NotificationService.Models;
using NotificationService.Models.DataModels;

namespace NotificationGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationGateway.Services.NotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            NotificationGateway.Services.NotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Notification request cannot be null");
            }

            if (string.IsNullOrEmpty(request.Recipient))
            {
                return BadRequest("Recipient is required");
            }

            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message is required");
            }

            var response = await _notificationService.SendNotificationAsync(request);
            
            return response.Status == NotificationStatus.Failed 
                ? StatusCode(500, response) 
                : Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Notification ID is required");
            }

            // In a real implementation, we would call the Status API via HTTP client
            // For now, we'll keep the original functionality but ideally this should
            // be refactored to call the Status API service
            var notification = await _notificationService.GetNotificationStatusAsync(id);
            
            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found");
            }

            var response = new NotificationResponse
            {
                Id = notification.Id,
                Status = notification.Status,
                Message = $"Notification status: {notification.Status}",
                Timestamp = notification.SentAt ?? notification.CreatedAt,
                Attempts = notification.Attempts,
                LastAttempt = notification.LastAttempt
            };

            return Ok(response);
        }

        [HttpPost("send-bulk")]
        public async Task<IActionResult> SendBulkNotifications([FromBody] List<NotificationRequest> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest("At least one notification request is required");
            }

            var results = new List<NotificationResponse>();

            foreach (var request in requests)
            {
                var response = await _notificationService.SendNotificationAsync(request);
                results.Add(response);
            }

            return Ok(results);
        }
    }
}
