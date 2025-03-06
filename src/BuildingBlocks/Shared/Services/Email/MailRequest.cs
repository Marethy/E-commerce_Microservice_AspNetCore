using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services.Email
{
    public  class MailRequest
    {
        [EmailAddress] public string From { get; set; }
        [EmailAddress] public string ToAddress { get; set; }
        public IEnumerable<string> ToAddresses { get; set; }= new List<string>();
        public required string Subject { get; set; } 
        public required string Body { get; set; }
        public List<IFormFile> Attachments { get; set; } = new();

    }
}
