namespace Contracts.Services
{
    public interface ISMTPEmailService<T> where T : class
    {
        Task SendEmailAsync(T request, CancellationToken cancellationToken = new CancellationToken());
    }
}