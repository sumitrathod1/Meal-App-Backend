﻿using MealApp.Models;
using MimeKit;
using MailKit.Net.Smtp;

namespace MealApp.Repo
{
    public class EmailRepository: IEmailRepository
    {
        private readonly IConfiguration _config;
        public EmailRepository(IConfiguration configuration)
        { 
            _config = configuration;
        }

        public void SendEmail(EmailModel emailModel)
        {
            var emailMessage = new MimeMessage();
            var from = _config["EmailSettings:From"];
            emailMessage.From.Add(new MailboxAddress("RISE MEAL FACILITY", from));
            emailMessage.To.Add(new MailboxAddress(emailModel.To, emailModel.To));
            emailMessage.Subject = emailModel.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = emailModel.Content
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_config["EmailSettings:SmtpServer"], 465, true);
                    client.Authenticate(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
                    client.Send(emailMessage);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"An error occurred while sending email: {ex.Message}");
                    throw; // Rethrow the exception for further handling
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
        }
    }
}
