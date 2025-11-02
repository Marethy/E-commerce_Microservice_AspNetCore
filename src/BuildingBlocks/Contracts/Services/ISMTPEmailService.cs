using Shared.Services.Email;

namespace Contracts.Services
{
    public interface ISMTPEmailService : ISMTPEmailService<MailRequestDto>
    {
    }
}