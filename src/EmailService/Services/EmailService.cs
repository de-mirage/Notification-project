using MailKit.Net.Smtp;
using MimeKit;
using NotificationService.Models;

namespace EmailService.Services
{
    public class EmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(NotificationRequest request)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Notification Service", 
                    Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "noreply@notificationservice.com"));
                message.To.Add(new MailboxAddress("", request.Recipient));
                message.Subject = request.Subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = request.Message;
                bodyBuilder.TextBody = StripHtml(request.Message);

                // Add attachments if any
                if (request.Attachments != null)
                {
                    foreach (var attachment in request.Attachments)
                    {
                        bodyBuilder.Attachments.Add(attachment.FileName, attachment.Data, 
                            ContentType.Parse(attachment.ContentType));
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                // Connect to SMTP server
                var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "localhost";
                var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
                var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
                var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? "";

                await client.ConnectAsync(smtpHost, smtpPort, false);
                
                if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
                {
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {request.Recipient} with subject: {request.Subject}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient}", request.Recipient);
                return false;
            }
        }

        private string StripHtml(string html)
        {
            // Simple HTML tag removal - in production, use a more robust solution
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}
