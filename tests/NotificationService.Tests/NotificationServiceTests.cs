using Xunit;
using NotificationService.Models;
using NotificationService.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace NotificationService.Tests
{
    public class GatewayNotificationServiceTests
    {
        private readonly NotificationContext _context;
        private readonly Mock<IRabbitMqService> _mockRabbitMqService;
        private readonly Mock<ILogger<NotificationGateway.Services.NotificationService>> _mockLogger;

        public GatewayNotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<NotificationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique DB name for each test
                .Options;

            _context = new NotificationContext(options);
            _mockRabbitMqService = new Mock<IRabbitMqService>();
            _mockLogger = new Mock<ILogger<NotificationGateway.Services.NotificationService>>();
        }

        [Fact]
        public async Task SendNotificationAsync_ValidRequest_ReturnsQueuedResponse()
        {
            // Arrange
            var gatewayNotificationService = new NotificationGateway.Services.NotificationService(_context, _mockRabbitMqService.Object, _mockLogger.Object);
            var request = new NotificationRequest
            {
                Recipient = "test@example.com",
                Subject = "Test Subject",
                Message = "Test Message",
                NotificationType = NotificationType.Email
            };

            _mockRabbitMqService
                .Setup(x => x.PublishMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()))
                .Verifiable();

            // Act
            var result = await gatewayNotificationService.SendNotificationAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(NotificationStatus.Queued, result.Status);
            Assert.Equal(request.Id, result.Id);
            
            // Verify the message was published to the queue
            _mockRabbitMqService.Verify(x => x.PublishMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()), Times.Once);
            
            // Verify the notification was saved to the database
            var savedNotification = await _context.NotificationRecords.FirstOrDefaultAsync(n => n.Id == request.Id);
            Assert.NotNull(savedNotification);
            Assert.Equal(NotificationStatus.Queued, savedNotification.Status);
        }

        [Fact]
        public async Task SendNotificationAsync_InvalidRequest_ReturnsFailedResponse()
        {
            // Arrange
            var gatewayNotificationService = new NotificationGateway.Services.NotificationService(_context, _mockRabbitMqService.Object, _mockLogger.Object);
            var request = new NotificationRequest
            {
                Recipient = "", // Invalid - empty recipient
                Subject = "Test Subject",
                Message = "Test Message",
                NotificationType = NotificationType.Email
            };

            // Act & Assert
            var result = await gatewayNotificationService.SendNotificationAsync(request);
            Assert.NotNull(result);
            Assert.Equal(NotificationStatus.Failed, result.Status);
        }

        [Fact]
        public async Task GetNotificationStatusAsync_ExistingNotification_ReturnsNotificationRecord()
        {
            // Arrange
            var gatewayNotificationService = new NotificationGateway.Services.NotificationService(_context, _mockRabbitMqService.Object, _mockLogger.Object);
            var notificationId = "test-id";
            
            var notificationRecord = new NotificationRecord
            {
                Id = notificationId,
                Recipient = "test@example.com",
                Subject = "Test Subject",
                Message = "Test Message",
                NotificationType = NotificationType.Email,
                Status = NotificationStatus.Sent
            };
            
            _context.NotificationRecords.Add(notificationRecord);
            await _context.SaveChangesAsync();

            // Act
            var result = await gatewayNotificationService.GetNotificationStatusAsync(notificationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(notificationId, result.Id);
            Assert.Equal(NotificationStatus.Sent, result.Status);
        }

        [Fact]
        public async Task GetNotificationStatusAsync_NonExistingNotification_ReturnsNull()
        {
            // Arrange
            var gatewayNotificationService = new NotificationGateway.Services.NotificationService(_context, _mockRabbitMqService.Object, _mockLogger.Object);
            var notificationId = "non-existing-id";

            // Act
            var result = await gatewayNotificationService.GetNotificationStatusAsync(notificationId);

            // Assert
            Assert.Null(result);
        }
    }
}
