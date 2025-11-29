using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models.DataModels;
using NotificationService.Models;

namespace StatusApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly NotificationContext _context;
        private readonly ILogger<StatusController> _logger;

        public StatusController(NotificationContext context, ILogger<StatusController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Notification ID is required");
            }

            var notification = await _context.NotificationRecords
                .FirstOrDefaultAsync(n => n.Id == id);

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

        [HttpGet]
        public async Task<IActionResult> GetAllNotifications([FromQuery] int page = 1, int pageSize = 10)
        {
            var notifications = await _context.NotificationRecords
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Status = n.Status,
                Message = $"Notification status: {n.Status}",
                Timestamp = n.SentAt ?? n.CreatedAt,
                Attempts = n.Attempts,
                LastAttempt = n.LastAttempt
            }).ToList();

            return Ok(response);
        }

        [HttpGet("by-type/{notificationType}")]
        public async Task<IActionResult> GetNotificationsByType(string notificationType, [FromQuery] int page = 1, int pageSize = 10)
        {
            if (!Enum.TryParse<NotificationType>(notificationType, true, out var type))
            {
                return BadRequest("Invalid notification type");
            }

            var notifications = await _context.NotificationRecords
                .Where(n => n.NotificationType == type)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Status = n.Status,
                Message = $"Notification status: {n.Status}",
                Timestamp = n.SentAt ?? n.CreatedAt,
                Attempts = n.Attempts,
                LastAttempt = n.LastAttempt
            }).ToList();

            return Ok(response);
        }
    }
}
