using Xunit;
using NotificationGateway.Services;
using NotificationService.Models;
using NotificationService.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace NotificationService.Tests
{
    public class LoadTest
    {
        [Fact(Skip = "Load test - run manually")]
        public async Task SimulateHighLoadNotifications()
        {
            // This is a performance/load test that should be run manually
            // It simulates a high volume of notification requests to test system performance
            
            var options = new DbContextOptionsBuilder<NotificationContext>()
                .UseInMemoryDatabase(databaseName: "LoadTestDb")
                .Options;

            using var context = new NotificationContext(options);
            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockLogger = new Mock<ILogger<NotificationGateway.Services.NotificationService>>();

            var notificationService = new NotificationGateway.Services.NotificationService(context, mockRabbitMqService.Object, mockLogger.Object);

            // Mock the RabbitMQ service to avoid actual queue operations during load testing
            mockRabbitMqService
                .Setup(x => x.PublishMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()))
                .Verifiable();

            const int numberOfRequests = 1000;
            var requests = new List<NotificationRequest>();

            // Generate test requests
            for (int i = 0; i < numberOfRequests; i++)
            {
                requests.Add(new NotificationRequest
                {
                    Recipient = $"user{i}@example.com",
                    Subject = $"Test Subject {i}",
                    Message = $"Test Message {i}",
                    NotificationType = (NotificationType)(i % 3), // Cycle through Email, SMS, Push
                    Priority = (i % 10 == 0) ? PriorityLevel.High : PriorityLevel.Normal
                });
            }

            var stopwatch = Stopwatch.StartNew();
            
            // Send all requests concurrently
            var tasks = requests.Select(request => notificationService.SendNotificationAsync(request)).ToList();
            var results = await Task.WhenAll(tasks);
            
            stopwatch.Stop();

            // Verify results
            Assert.Equal(numberOfRequests, results.Length);
            Assert.All(results, result => Assert.NotNull(result));
            
            Console.WriteLine($"Processed {numberOfRequests} notifications in {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per notification: {stopwatch.ElapsedMilliseconds / (double)numberOfRequests} ms");
            
            // Verify that all notifications were saved to the database
            var savedNotificationsCount = await context.NotificationRecords.CountAsync();
            Assert.Equal(numberOfRequests, savedNotificationsCount);
            
            // Verify that the RabbitMQ service was called for each notification
            mockRabbitMqService.Verify(x => x.PublishMessage(It.IsAny<QueueMessage>(), It.IsAny<string>()), 
                Times.Exactly(numberOfRequests));
        }

        [Fact(Skip = "Load test - run manually")]
        public async Task SimulateConcurrentNotificationStatusQueries()
        {
            // This test simulates concurrent queries to the notification status endpoint
            
            var options = new DbContextOptionsBuilder<NotificationContext>()
                .UseInMemoryDatabase(databaseName: "LoadTestStatusDb")
                .Options;

            using var context = new NotificationContext(options);
            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockLogger = new Mock<ILogger<NotificationGateway.Services.NotificationService>>();

            var notificationService = new NotificationGateway.Services.NotificationService(context, mockRabbitMqService.Object, mockLogger.Object);

            // Pre-populate the database with some notifications
            var notifications = new List<NotificationRecord>();
            for (int i = 0; i < 100; i++)
            {
                notifications.Add(new NotificationRecord
                {
                    Id = $"notification-{i}",
                    Recipient = $"user{i}@example.com",
                    Subject = $"Test Subject {i}",
                    Message = $"Test Message {i}",
                    NotificationType = (NotificationType)(i % 3),
                    Status = NotificationStatus.Queued
                });
            }
            
            context.NotificationRecords.AddRange(notifications);
            await context.SaveChangesAsync();

            const int numberOfConcurrentQueries = 50;
            var stopwatch = Stopwatch.StartNew();
            
            // Execute concurrent queries
            var tasks = new List<Task<NotificationRecord?>>();
            for (int i = 0; i < numberOfConcurrentQueries; i++)
            {
                var notificationId = $"notification-{i % 100}"; // Cycle through available notifications
                tasks.Add(Task.Run(() => notificationService.GetNotificationStatusAsync(notificationId)));
            }
            
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Verify results
            Assert.Equal(numberOfConcurrentQueries, results.Length);
            Assert.All(results, result => Assert.NotNull(result));
            
            Console.WriteLine($"Processed {numberOfConcurrentQueries} status queries in {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per query: {stopwatch.ElapsedMilliseconds / (double)numberOfConcurrentQueries} ms");
        }
    }
}
