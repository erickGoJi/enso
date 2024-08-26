using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Enso.api.Models;

namespace Enso.api.Services
{
    public interface IEmailService
    {
        void SendEmail(Email email);
        Task<string> SendMailAsync(Email email);
        Task SendAttachment(string email, string subject, bool isBodyHtml, string message, List<Attachment> attachments);
    }
}
