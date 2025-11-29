using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using NotificationService.Models;

namespace SmsService.Services
{
    public class SmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
            
            // Initialize Twilio client
            var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            
            if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            {
                TwilioClient.Init(accountSid, authToken);
            }
        }

        public async Task<bool> SendSmsAsync(NotificationRequest request)
        {
            try
            {
                var fromNumber = Environment.GetEnvironmentVariable("TWILIO_FROM_NUMBER") ?? "+1234567890";
                var toNumber = request.Recipient;

                var message = await MessageResource.CreateAsync(
                    body: request.Message,
                    from: new PhoneNumber(fromNumber),
                    to: new PhoneNumber(toNumber)
                );

                _logger.LogInformation($"SMS sent successfully to {request.Recipient} with SID: {message.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {Recipient}", request.Recipient);
                return false;
            }
        }
    }
}
