using System.ComponentModel.DataAnnotations;

namespace Shared.Services.Email
{
    public class MailRequestDto
    {
        [EmailAddress]
        public string From { get; set; } = default!;

        [EmailAddress]
        public string ToAddress { get; set; } = default!;

        public IEnumerable<string> ToAddresses { get; set; } = new List<string>();

        public string Subject { get; set; } = default!;

        public string Body { get; set; } = default!;

        public List<MailAttachmentDto> Attachments { get; set; } = new();
    }
    public class MailAttachmentDto
    {
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public byte[] Content { get; set; } = default!;
    }
}