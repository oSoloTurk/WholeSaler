using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace WholeSaler.Services
{
    public class EmailService : IEmailSender
    {
        public async void SendEmail(string to, string subject, string root, string templateName, Dictionary<String, String> replaces)
        {
            var pathToFile = root
                + Path.DirectorySeparatorChar.ToString()
                + "Templates"
                + Path.DirectorySeparatorChar.ToString()
                + "EmailTemplates"
                + Path.DirectorySeparatorChar.ToString()
                + templateName;
            var body = "";
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                body = SourceReader.ReadToEnd();

            }
            foreach (KeyValuePair<string, string> entry in replaces)
            {
                body = body.Replace(entry.Key, entry.Value);
            }

            await SendEmailAsync(
                to,
                subject,
                htmlMessage: body);
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SmtpClient client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("wholesaler.mh.sau@gmail.com", "ukdwmeszspkwimek")
            };
            var mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.Body = htmlMessage;
            mail.Subject = subject;
            mail.Priority = MailPriority.High;
            mail.Sender = new MailAddress("wholesaler.mh.sau@gmail.com", "wholesaler.mh.sau@gmail.com");
            mail.From = mail.Sender;
            mail.To.Add(email);
            return client.SendMailAsync(mail);
        }
    }
}
