using Shared.Services.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Services
{
    public interface ISMTPEmailService<T> where T : class 
    {
        Task SendEmailAsync(T request, CancellationToken cancellationToken = new CancellationToken());

    }
}
