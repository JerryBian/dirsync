using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DirSync.Core
{
    public class EmailUtil
    {
        public async Task SendAsync(string content, string[] attachments)
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                return;
            }

            var toEmailAddress = Environment.GetEnvironmentVariable("EMAIL_TO_ADDRESS");
            if (string.IsNullOrEmpty(toEmailAddress))
            {
                return;
            }

            var toEmailName = Environment.GetEnvironmentVariable("EMAIL_TO_NAME");
            if (string.IsNullOrEmpty(toEmailName))
            {
                toEmailName = toEmailAddress;
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("bot@dirsync.io", "dirsync");
            var to = new EmailAddress(toEmailAddress, toEmailName);
            var subject = "dirsync report";
            var message = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var atts = new List<Attachment>();
            foreach (var a in attachments)
            {
                atts.Add(new Attachment { Content = File.ReadAllText(a), Filename = Path.GetFileName(a) });
            }
            var response = await client.SendEmailAsync(message);
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                // success
            }
        }
    }
}
