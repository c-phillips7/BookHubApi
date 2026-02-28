using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace BookHub.Services
{
    public class EmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("BookHub Support", _emailSettings.SmtpUsername));
                // Used to test service providing email.
            //message.To.Add(MailboxAddress.Parse(_emailSettings.SmtpUsername));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            // Added try catch block to figure out why not revieving emails
            try
            {
                using var client = new SmtpClient();
                Console.WriteLine($"Sending email to {toEmail} with subject '{subject}' and body length {body.Length}");
                client.Connect(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                client.Authenticate(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                client.Send(message);
                client.Disconnect(true);

            }
            catch (Exception ex)
            {
                // log error to console
                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw;
            }

        }
    }
}