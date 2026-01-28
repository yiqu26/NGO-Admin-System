using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace NGOPlatformWeb.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            try
            {
                // Get SMTP settings
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                // Read HTML template file
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(),
                    "Views", "Shared", "EmailTemplates", "PasswordReset.html");

                var htmlTemplate = await File.ReadAllTextAsync(templatePath);

                // Replace variables in template
                var htmlContent = htmlTemplate.Replace("{resetLink}", resetLink);

                // Create mail message
                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(fromEmail, fromName);
                mailMessage.To.Add(email);
                mailMessage.Subject = "密碼重設通知 - NGO平台";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = htmlContent;

                // Configure SMTP client
                var smtpClient = new SmtpClient(smtpHost, smtpPort);
                smtpClient.EnableSsl = true; // 啟用 SSL
                smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                // Send email
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (FileNotFoundException)
            {
                // Template file not found
                throw new Exception("找不到郵件模板檔案，請確認 Views/Shared/EmailTemplates/PasswordReset.html是否存在");
            }
            catch (Exception ex)
            {
                // Log error (should use ILogger in production)
                throw new Exception($"發送郵件失敗: {ex.Message}", ex);
            }
        }
    }
}
