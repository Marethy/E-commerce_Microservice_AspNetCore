using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Messages
{
    public interface IMessageProducer    
    {
        Task SendMessageAsync<T>(T message);
    }
}
