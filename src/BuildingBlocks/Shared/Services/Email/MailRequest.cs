using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Shared.Services.Email
{
    public class MailRequest
    {
        [EmailAddress] public string From { get; set; }
        [EmailAddress] public string ToAddress { get; set; }
        public IEnumerable<string> ToAddresses { get; set; } = new List<string>();
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; } = new();
    }
}