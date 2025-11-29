using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using NotificationService.Models;

namespace PushService.Services
{
    public class PushService
    {
        private readonly ILogger<PushService> _logger;

        public PushService(ILogger<PushService> logger)
        {
            _logger = logger;

            // Initialize Firebase Admin SDK
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            try
            {
                // For demo purposes, Firebase initialization is commented out
                // Requires proper Firebase credentials configuration
                _logger.LogWarning("Firebase initialization is disabled. Provide FIREBASE_CONFIG_PATH environment variable for actual Firebase notifications.");

                /*
                var firebaseConfigPath = Environment.GetEnvironmentVariable("FIREBASE_CONFIG_PATH");

                if (!string.IsNullOrEmpty(firebaseConfigPath) && File.Exists(firebaseConfigPath))
                {
                    var credential = GoogleCredential.FromFile(firebaseConfigPath);
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential
                    });
                }
                else
                {
                    _logger.LogWarning("Firebase configuration not found. Push notifications will not work.");
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Firebase Admin SDK");
            }
        }

        public async Task<bool> SendPushNotificationAsync(NotificationRequest request)
        {
            try
            {
                var message = new Message
                {
                    Notification = new Notification
                    {
                        Title = request.Subject,
                        Body = request.Message,
                    },
                    Token = request.Recipient, // FCM registration token
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                _logger.LogInformation($"Push notification sent successfully to {request.Recipient} with response: {response}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to {Recipient}", request.Recipient);
                return false;
            }
        }
    }
}
